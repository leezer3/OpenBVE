using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using OpenBveApi.Interface;
using Prism.Mvvm;
using TrainEditor2.IO.Panels.Bve4;
using TrainEditor2.IO.Panels.Xml;
using TrainEditor2.IO.Sounds.Bve2;
using TrainEditor2.IO.Sounds.Bve4;
using TrainEditor2.IO.Sounds.Xml;
using TrainEditor2.IO.Trains.ExtensionsCfg;
using TrainEditor2.IO.Trains.TrainDat;
using TrainEditor2.IO.Trains.Xml;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using TouchElement = TrainEditor2.Models.Panels.TouchElement;

namespace TrainEditor2.Models
{
	internal enum TrainFileType
	{
		OldFormat,
		NewFormat
	}

	internal enum PanelFileType
	{
		Panel2Cfg,
		PanelXml
	}

	internal enum SoundFileType
	{
		NoSettingFile,
		SoundCfg,
		SoundXml
	}

	internal abstract class ImportFile : BindableBase
	{
		protected readonly App app;

		private OpenFileDialog openFileDialog;

		internal OpenFileDialog OpenFileDialog
		{
			get
			{
				return openFileDialog;
			}
			set
			{
				SetProperty(ref openFileDialog, value);
			}
		}

		protected ImportFile(App app)
		{
			this.app = app;
			OpenFileDialog = new OpenFileDialog();
		}

		internal abstract void Import();
	}

	internal class ImportTrainFile : ImportFile
	{
		private TrainFileType currentTrainFileType;

		private string trainDatLocation;
		private string extensionsCfgLocation;
		private string trainXmlLocation;

		internal TrainFileType CurrentTrainFileType
		{
			get
			{
				return currentTrainFileType;
			}
			set
			{
				SetProperty(ref currentTrainFileType, value);
			}
		}

		internal string TrainDatLocation
		{
			get
			{
				return trainDatLocation;
			}
			set
			{
				SetProperty(ref trainDatLocation, value);
			}
		}

		internal string ExtensionsCfgLocation
		{
			get
			{
				return extensionsCfgLocation;
			}
			set
			{
				SetProperty(ref extensionsCfgLocation, value);
			}
		}

		internal string TrainXmlLocation
		{
			get
			{
				return trainXmlLocation;
			}
			set
			{
				SetProperty(ref trainXmlLocation, value);
			}
		}

		internal ImportTrainFile(App app) : base(app)
		{
		}

		internal void SetTrainDatFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"train.dat files|train.dat|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			TrainDatLocation = OpenFileDialog.FileName;
		}

		internal void SetExtensionsCfgFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"extensions.cfg files|extensions.cfg|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			ExtensionsCfgLocation = OpenFileDialog.FileName;
		}

		internal void SetTrainXmlFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"train.xml files|train.xml|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			TrainXmlLocation = OpenFileDialog.FileName;
		}

		internal override void Import()
		{
			try
			{
				switch (CurrentTrainFileType)
				{
					case TrainFileType.OldFormat:
						if (!string.IsNullOrEmpty(TrainDatLocation))
						{
							Train train;
							TrainDat.Parse(TrainDatLocation, out train);
							app.Train = train;
						}

						if (!string.IsNullOrEmpty(ExtensionsCfgLocation))
						{
							ExtensionsCfg.Parse(ExtensionsCfgLocation, app.Train);
						}
						break;
					case TrainFileType.NewFormat:
						if (!string.IsNullOrEmpty(TrainXmlLocation))
						{
							Train train;
							TrainXml.Parse(TrainXmlLocation, out train);
							app.Train = train;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				app.CreateTreeItem();

				foreach (Motor motor in app.Train.Cars.OfType<MotorCar>().Select(x => x.Motor))
				{
					motor.CreateTreeItem();
				}

				foreach (Panel panel in app.Train.Cars.OfType<ControlledMotorCar>().Select(x => x.Cab).OfType<EmbeddedCab>().Select(x => x.Panel))
				{
					panel.CreateTreeItem();

					foreach (TouchElement touch in panel.Screens.SelectMany(x => x.TouchElements))
					{
						touch.CreateTreeItem();
					}
				}

				foreach (Panel panel in app.Train.Cars.OfType<ControlledTrailerCar>().Select(x => x.Cab).OfType<EmbeddedCab>().Select(x => x.Panel))
				{
					panel.CreateTreeItem();

					foreach (TouchElement touch in panel.Screens.SelectMany(x => x.TouchElements))
					{
						touch.CreateTreeItem();
					}
				}

			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			app.SelectedTreeItem = null;
		}
	}

	internal class ImportPanelFile : ImportFile
	{
		private PanelFileType currentPanelFileType;

		private string panel2CfgLocation;
		private string panelXmlLocation;

		internal PanelFileType CurrentPanelFileType
		{
			get
			{
				return currentPanelFileType;
			}
			set
			{
				SetProperty(ref currentPanelFileType, value);
			}
		}

		internal string Panel2CfgLocation
		{
			get
			{
				return panel2CfgLocation;
			}
			set
			{
				SetProperty(ref panel2CfgLocation, value);
			}
		}

		internal string PanelXmlLocation
		{
			get
			{
				return panelXmlLocation;
			}
			set
			{
				SetProperty(ref panelXmlLocation, value);
			}
		}

		internal ObservableCollection<ListViewItemModel> ImportCarsList;

		internal ImportPanelFile(App app) : base(app)
		{
			ImportCarsList = new ObservableCollection<ListViewItemModel>();
		}

		internal void UpdateImportCarsList()
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			for (int i = app.Train.Cars.Count; i < ImportCarsList.Count; i++)
			{
				ImportCarsList.RemoveAt(i);
			}

			for (int i = 0; i < ImportCarsList.Count; i++)
			{
				ImportCarsList[i].SubItems[0].Text = i.ToString(culture);
			}

			for (int i = ImportCarsList.Count; i < app.Train.Cars.Count; i++)
			{
				ImportCarsList.Add(new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(new[] { new ListViewSubItemModel { Text = i.ToString(culture) } }) });
			}
		}

		internal void SetPanel2CfgFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"panel2.cfg files|panel2.cfg|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			Panel2CfgLocation = OpenFileDialog.FileName;
		}

		internal void SetPanelXmlFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"panel.xml files|panel.xml|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			PanelXmlLocation = OpenFileDialog.FileName;
		}

		internal override void Import()
		{
			try
			{
				Panel panel = null;

				switch (CurrentPanelFileType)
				{
					case PanelFileType.Panel2Cfg:
						if (!string.IsNullOrEmpty(Panel2CfgLocation))
						{
							PanelCfgBve4.Parse(Panel2CfgLocation, out panel);
						}
						break;
					case PanelFileType.PanelXml:
						if (!string.IsNullOrEmpty(PanelXmlLocation))
						{
							PanelCfgXml.Parse(PanelXmlLocation, out panel);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (panel == null)
				{
					return;
				}

				for (int i = 0; i < app.Train.Cars.Count; i++)
				{
					if (!ImportCarsList[i].Checked)
					{
						continue;
					}

					MotorCar motorCar = app.Train.Cars[i] as MotorCar;

					if (motorCar != null)
					{
						app.Train.Cars[i] = new ControlledMotorCar(motorCar) { Cab = new EmbeddedCab { Panel = (Panel)panel.Clone() } };
					}
					else
					{
						app.Train.Cars[i] = new ControlledTrailerCar(app.Train.Cars[i]) { Cab = new EmbeddedCab { Panel = (Panel)panel.Clone() } };
					}
				}
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			app.SelectedTreeItem = null;
		}
	}

	internal class ImportSoundFile : ImportFile
	{
		private OpenFolderDialog openFolderDialog;

		private SoundFileType currentSoundFileType;

		private string trainFolderLocation;
		private string soundCfgLocation;
		private string soundXmlLocation;

		internal OpenFolderDialog OpenFolderDialog
		{
			get
			{
				return openFolderDialog;
			}
			set
			{
				SetProperty(ref openFolderDialog, value);
			}
		}

		internal SoundFileType CurrentSoundFileType
		{
			get
			{
				return currentSoundFileType;
			}
			set
			{
				SetProperty(ref currentSoundFileType, value);
			}
		}

		internal string TrainFolderLocation
		{
			get
			{
				return trainFolderLocation;
			}
			set
			{
				SetProperty(ref trainFolderLocation, value);
			}
		}

		internal string SoundCfgLocation
		{
			get
			{
				return soundCfgLocation;
			}
			set
			{
				SetProperty(ref soundCfgLocation, value);
			}
		}

		internal string SoundXmlLocation
		{
			get
			{
				return soundXmlLocation;
			}
			set
			{
				SetProperty(ref soundXmlLocation, value);
			}
		}

		internal ImportSoundFile(App app) : base(app)
		{
			OpenFolderDialog = new OpenFolderDialog();
		}

		internal void SetTrainFolder()
		{
			OpenFolderDialog = new OpenFolderDialog { IsOpen = true };

			if (OpenFolderDialog.DialogResult != true)
			{
				return;
			}

			TrainFolderLocation = OpenFolderDialog.Folder;
		}

		internal void SetSoundCfgFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"sound.cfg files|sound.cfg|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			SoundCfgLocation = OpenFileDialog.FileName;
		}

		internal void SetSoundXmlFile()
		{
			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"sound.xml files|sound.xml|All files|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			SoundXmlLocation = OpenFileDialog.FileName;
		}

		internal override void Import()
		{
			try
			{
				switch (CurrentSoundFileType)
				{
					case SoundFileType.NoSettingFile:
						if (!string.IsNullOrEmpty(TrainFolderLocation))
						{
							Sound sound;
							SoundCfgBve2.Parse(TrainFolderLocation, out sound);
							app.Sound = sound;
						}
						break;
					case SoundFileType.SoundCfg:
						if (!string.IsNullOrEmpty(SoundCfgLocation))
						{
							Sound sound;
							SoundCfgBve4.Parse(SoundCfgLocation, out sound);
							app.Sound = sound;
						}
						break;
					case SoundFileType.SoundXml:
						if (!string.IsNullOrEmpty(SoundXmlLocation))
						{
							Sound sound;
							SoundCfgXml.Parse(SoundXmlLocation, out sound);
							app.Sound = sound;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				app.Sound.CreateTreeItem();
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			app.SelectedTreeItem = null;
		}
	}

	internal abstract class ExportFile : BindableBase
	{
		protected readonly App app;

		private SaveFileDialog saveFileDialog;

		internal SaveFileDialog SaveFileDialog
		{
			get
			{
				return saveFileDialog;
			}
			set
			{
				SetProperty(ref saveFileDialog, value);
			}
		}

		protected ExportFile(App app)
		{
			this.app = app;
			SaveFileDialog = new SaveFileDialog();
		}

		internal abstract void Export();
	}

	internal class ExportTrainFile : ExportFile
	{
		private TrainFileType currentTrainFileType;

		private string trainDatLocation;
		private string extensionsCfgLocation;
		private string trainXmlLocation;

		internal TrainFileType CurrentTrainFileType
		{
			get
			{
				return currentTrainFileType;
			}
			set
			{
				SetProperty(ref currentTrainFileType, value);
			}
		}

		internal string TrainDatLocation
		{
			get
			{
				return trainDatLocation;
			}
			set
			{
				SetProperty(ref trainDatLocation, value);
			}
		}

		internal string ExtensionsCfgLocation
		{
			get
			{
				return extensionsCfgLocation;
			}
			set
			{
				SetProperty(ref extensionsCfgLocation, value);
			}
		}

		internal string TrainXmlLocation
		{
			get
			{
				return trainXmlLocation;
			}
			set
			{
				SetProperty(ref trainXmlLocation, value);
			}
		}

		internal ExportTrainFile(App app) : base(app)
		{
		}

		internal void SetTrainDatFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"train.dat files|train.dat|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			TrainDatLocation = SaveFileDialog.FileName;
		}

		internal void SetExtensionsCfgFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"extensions.cfg files|extensions.cfg|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			ExtensionsCfgLocation = SaveFileDialog.FileName;
		}

		internal void SetTrainXmlFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"train.xml files|train.xml|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			TrainXmlLocation = SaveFileDialog.FileName;
		}

		internal override void Export()
		{
			try
			{
				switch (CurrentTrainFileType)
				{
					case TrainFileType.OldFormat:
						if (!string.IsNullOrEmpty(TrainDatLocation))
						{
							TrainDat.Write(TrainDatLocation, app.Train);
						}

						if (!string.IsNullOrEmpty(ExtensionsCfgLocation))
						{
							ExtensionsCfg.Write(ExtensionsCfgLocation, app.Train);
						}
						break;
					case TrainFileType.NewFormat:
						if (!string.IsNullOrEmpty(TrainXmlLocation))
						{
							TrainXml.Write(TrainXmlLocation, app.Train);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}
		}
	}

	internal class ExportPanelFile : ExportFile
	{
		private PanelFileType currentPanelFileType;

		private string panel2CfgLocation;
		private string panelXmlLocation;

		private int exportTrainIndex;

		internal PanelFileType CurrentPanelFileType
		{
			get
			{
				return currentPanelFileType;
			}
			set
			{
				SetProperty(ref currentPanelFileType, value);
			}
		}

		internal string Panel2CfgLocation
		{
			get
			{
				return panel2CfgLocation;
			}
			set
			{
				SetProperty(ref panel2CfgLocation, value);
			}
		}

		internal string PanelXmlLocation
		{
			get
			{
				return panelXmlLocation;
			}
			set
			{
				SetProperty(ref panelXmlLocation, value);
			}
		}

		internal int ExportTrainIndex
		{
			get
			{
				return exportTrainIndex;
			}
			set
			{
				SetProperty(ref exportTrainIndex, value);
			}
		}

		internal ExportPanelFile(App app) : base(app)
		{
		}

		internal void SetPanel2CfgFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"panel2.cfg files|panel2.cfg|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			Panel2CfgLocation = SaveFileDialog.FileName;
		}

		internal void SetPanelXmlFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"panel.xml files|panel.xml|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			PanelXmlLocation = SaveFileDialog.FileName;
		}

		internal override void Export()
		{
			EmbeddedCab embeddedCab = null;
			ControlledMotorCar controlledMotorCar = app.Train.Cars[ExportTrainIndex] as ControlledMotorCar;
			ControlledTrailerCar controlledTrailerCar = app.Train.Cars[ExportTrainIndex] as ControlledTrailerCar;

			if (controlledMotorCar != null)
			{
				embeddedCab = (EmbeddedCab)controlledMotorCar.Cab;
			}

			if (controlledTrailerCar != null)
			{
				embeddedCab = (EmbeddedCab)controlledTrailerCar.Cab;
			}

			if (embeddedCab == null)
			{
				return;
			}

			try
			{
				switch (CurrentPanelFileType)
				{
					case PanelFileType.Panel2Cfg:
						if (!string.IsNullOrEmpty(Panel2CfgLocation))
						{
							PanelCfgBve4.Write(Panel2CfgLocation, embeddedCab.Panel);
						}
						break;
					case PanelFileType.PanelXml:
						if (!string.IsNullOrEmpty(PanelXmlLocation))
						{
							PanelCfgXml.Write(PanelXmlLocation, embeddedCab.Panel);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}
		}
	}

	internal class ExportSoundFile : ExportFile
	{
		private SoundFileType currentSoundFileType;

		private string soundCfgLocation;
		private string soundXmlLocation;

		internal SoundFileType CurrentSoundFileType
		{
			get
			{
				return currentSoundFileType;
			}
			set
			{
				SetProperty(ref currentSoundFileType, value);
			}
		}

		internal string SoundCfgLocation
		{
			get
			{
				return soundCfgLocation;
			}
			set
			{
				SetProperty(ref soundCfgLocation, value);
			}
		}

		internal string SoundXmlLocation
		{
			get
			{
				return soundXmlLocation;
			}
			set
			{
				SetProperty(ref soundXmlLocation, value);
			}
		}

		internal ExportSoundFile(App app) : base(app)
		{
			CurrentSoundFileType = SoundFileType.SoundCfg;
		}

		internal void SetSoundCfgFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"sound.cfg files|sound.cfg|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			SoundCfgLocation = SaveFileDialog.FileName;
		}

		internal void SetSoundXmlFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"sound.xml files|sound.xml|All files|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			SoundXmlLocation = SaveFileDialog.FileName;
		}

		internal override void Export()
		{
			try
			{
				switch (CurrentSoundFileType)
				{
					case SoundFileType.NoSettingFile:
						break;
					case SoundFileType.SoundCfg:
						if (!string.IsNullOrEmpty(SoundCfgLocation))
						{
							SoundCfgBve4.Write(SoundCfgLocation, app.Sound);
						}
						break;
					case SoundFileType.SoundXml:
						if (!string.IsNullOrEmpty(SoundXmlLocation))
						{
							SoundCfgXml.Write(SoundXmlLocation, app.Sound);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}
		}
	}
}
