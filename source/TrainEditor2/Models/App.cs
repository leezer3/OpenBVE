using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Xml;
using OpenBveApi.Interface;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.IO.IntermediateFile;
using TrainEditor2.IO.Panels.Bve4;
using TrainEditor2.IO.Panels.Xml;
using TrainEditor2.IO.Sounds.Bve2;
using TrainEditor2.IO.Sounds.Bve4;
using TrainEditor2.IO.Sounds.Xml;
using TrainEditor2.IO.Trains.ExtensionsCfg;
using TrainEditor2.IO.Trains.TrainDat;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.Models
{
	internal class App : BindableBase
	{
		internal enum TrainFileType
		{
			OldFormat
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

		private readonly CultureInfo culture;

		private string saveLocation;
		private string currentLanguageCode;

		private Train train;
		private Panel panel;
		private Sound sound;

		private MessageBox messageBox;
		private OpenFileDialog openFileDialog;
		private SaveFileDialog saveFileDialog;

		private TrainFileType currentTrainFileType;
		private PanelFileType currentPanelFileType;
		private SoundFileType currentSoundFileType;

		private string trainDatImportLocation;
		private string trainDatExportLocation;
		private string extensionsCfgImportLocation;
		private string extensionsCfgExportLocation;
		private string panel2CfgImportLocation;
		private string panel2CfgExportLocation;
		private string panelXmlImportLocation;
		private string panelXmlExportLocation;
		private string trainFolderImportLocation;
		private string soundCfgImportLocation;
		private string soundCfgExportLocation;
		private string soundXmlImportLocation;
		private string soundXmlExportLocation;

		private TreeViewItemModel selectedTreeItem;

		internal string SaveLocation
		{
			get
			{
				return saveLocation;
			}
			set
			{
				SetProperty(ref saveLocation, value);
			}
		}

		internal string CurrentLanguageCode
		{
			get
			{
				return currentLanguageCode;
			}
			set
			{
				SetProperty(ref currentLanguageCode, value);
			}
		}

		internal Train Train
		{
			get
			{
				return train;
			}
			set
			{
				SetProperty(ref train, value);
			}
		}

		internal Panel Panel
		{
			get
			{
				return panel;
			}
			set
			{
				SetProperty(ref panel, value);
			}
		}

		internal Sound Sound
		{
			get
			{
				return sound;
			}
			set
			{
				SetProperty(ref sound, value);
			}
		}

		internal MessageBox MessageBox
		{
			get
			{
				return messageBox;
			}
			set
			{
				SetProperty(ref messageBox, value);
			}
		}

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

		internal string TrainDatImportLocation
		{
			get
			{
				return trainDatImportLocation;
			}
			set
			{
				SetProperty(ref trainDatImportLocation, value);
			}
		}

		internal string TrainDatExportLocation
		{
			get
			{
				return trainDatExportLocation;
			}
			set
			{
				SetProperty(ref trainDatExportLocation, value);
			}
		}

		internal string ExtensionsCfgImportLocation
		{
			get
			{
				return extensionsCfgImportLocation;
			}
			set
			{
				SetProperty(ref extensionsCfgImportLocation, value);
			}
		}

		internal string ExtensionsCfgExportLocation
		{
			get
			{
				return extensionsCfgExportLocation;
			}
			set
			{
				SetProperty(ref extensionsCfgExportLocation, value);
			}
		}

		internal string Panel2CfgImportLocation
		{
			get
			{
				return panel2CfgImportLocation;
			}
			set
			{
				SetProperty(ref panel2CfgImportLocation, value);
			}
		}

		internal string Panel2CfgExportLocation
		{
			get
			{
				return panel2CfgExportLocation;
			}
			set
			{
				SetProperty(ref panel2CfgExportLocation, value);
			}
		}

		internal string PanelXmlImportLocation
		{
			get
			{
				return panelXmlImportLocation;
			}
			set
			{
				SetProperty(ref panelXmlImportLocation, value);
			}
		}

		internal string PanelXmlExportLocation
		{
			get
			{
				return panelXmlExportLocation;
			}
			set
			{
				SetProperty(ref panelXmlExportLocation, value);
			}
		}

		internal string TrainFolderImportLocation
		{
			get
			{
				return trainFolderImportLocation;
			}
			set
			{
				SetProperty(ref trainFolderImportLocation, value);
			}
		}

		internal string SoundCfgImportLocation
		{
			get
			{
				return soundCfgImportLocation;
			}
			set
			{
				SetProperty(ref soundCfgImportLocation, value);
			}
		}

		internal string SoundCfgExportLocation
		{
			get
			{
				return soundCfgExportLocation;
			}
			set
			{
				SetProperty(ref soundCfgExportLocation, value);
			}
		}

		internal string SoundXmlImportLocation
		{
			get
			{
				return soundXmlImportLocation;
			}
			set
			{
				SetProperty(ref soundXmlImportLocation, value);
			}
		}

		internal string SoundXmlExportLocation
		{
			get
			{
				return soundXmlExportLocation;
			}
			set
			{
				SetProperty(ref soundXmlExportLocation, value);
			}
		}

		internal TreeViewItemModel SelectedTreeItem
		{
			get
			{
				return selectedTreeItem;
			}
			set
			{
				SetProperty(ref selectedTreeItem, value);
			}
		}

		internal ObservableCollection<TreeViewItemModel> TreeItems;

		internal ObservableCollection<ListViewItemModel> VisibleLogMessages;

		internal App()
		{
			culture = CultureInfo.InvariantCulture;

			CurrentLanguageCode = "en-US";

			MessageBox = new MessageBox();
			OpenFileDialog = new OpenFileDialog();
			SaveFileDialog = new SaveFileDialog();

			TreeItems = new ObservableCollection<TreeViewItemModel>();

			VisibleLogMessages = new ObservableCollection<ListViewItemModel>();

			CreateNewFile();
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = Utilities.GetInterfaceString("tree_cars", "train") };
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = Utilities.GetInterfaceString("tree_cars", "general") });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = Utilities.GetInterfaceString("tree_cars", "cars") });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = Utilities.GetInterfaceString("tree_cars", "couplers") });
			treeItem.Children[1].Children.AddRange(Train.Cars.Select((x, i) => new TreeViewItemModel(treeItem.Children[1]) { Title = i.ToString(culture), Tag = x }));
			treeItem.Children[2].Children.AddRange(Train.Couplers.Select((x, i) => new TreeViewItemModel(treeItem.Children[2]) { Title = i.ToString(culture), Tag = x }));
			TreeItems.Add(treeItem);
		}

		internal void CreateNewFile()
		{
			if (Train != null || Panel != null || Sound != null)
			{
				MessageBox = new MessageBox
				{
					Title = Utilities.GetInterfaceString("menu", "file", "new"),
					Icon = BaseDialog.DialogIcon.Question,
					Button = BaseDialog.DialogButton.YesNoCancel,
					Text = Utilities.GetInterfaceString("menu", "message", "new"),
					IsOpen = true
				};

				if (MessageBox.DialogResult == null)
				{
					return;
				}

				if (MessageBox.DialogResult == true)
				{
					SaveFile();
				}
			}

			Interface.LogMessages.Clear();

			SaveLocation = string.Empty;

			train = new Train();
			train.Cars.Add(new MotorCar());
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));

			Panel = new Panel();

			Sound = new Sound();

			CreateTreeItem();

			SelectedTreeItem = null;
		}

		internal void OpenFile()
		{
			MessageBox = new MessageBox
			{
				Title = Utilities.GetInterfaceString("menu", "file", "open"),
				Icon = BaseDialog.DialogIcon.Question,
				Button = BaseDialog.DialogButton.YesNoCancel,
				Text = Utilities.GetInterfaceString("menu", "message", "open"),
				IsOpen = true
			};

			if (MessageBox.DialogResult == null)
			{
				return;
			}

			if (MessageBox.DialogResult == true)
			{
				SaveFile();
			}

			Interface.LogMessages.Clear();

			OpenFileDialog = new OpenFileDialog
			{
				Filter = @"Intermediate files (*.te)|*.te|All files (*.*)|*",
				CheckFileExists = true,
				IsOpen = true
			};

			if (OpenFileDialog.DialogResult != true)
			{
				return;
			}

			SaveLocation = OpenFileDialog.FileName;

			try
			{
				IntermediateFile.Parse(SaveLocation, out train, out panel, out sound);

				OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(Panel)));
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sound)));

				CreateTreeItem();

				foreach (MotorCar car in Train.Cars.OfType<MotorCar>())
				{
					car.Motor.CreateTreeItem();
				}

				Panel.CreateTreeItem();
				Sound.CreateTreeItem();

				SelectedTreeItem = null;
			}
			catch (Exception e)
			{
				if (e is XmlException && SaveLocation.ToLowerInvariant().EndsWith(".dat") || e is InvalidDataException)
				{
					/* At the minute, we need to use the import function to get existing trains into TrainEditor2
					 * Detect this is actually an existing train format and show a more useful error message
					 */
					MessageBox = new MessageBox
					{
						Title = Utilities.GetInterfaceString("menu", "file", "open"),
						Icon = BaseDialog.DialogIcon.Error,
						Button = BaseDialog.DialogButton.Ok,
						Text = Utilities.GetInterfaceString("menu", "file", "wrongtype"),
						IsOpen = true
					};

				}
				else
				{
					/* Generic error message-
					 * This isn't a file we recognise
					 */
					MessageBox = new MessageBox
					{
						Title = Utilities.GetInterfaceString("menu", "file", "open"),
						Icon = BaseDialog.DialogIcon.Error,
						Button = BaseDialog.DialogButton.Ok,
						Text = e.Message,
						IsOpen = true
					};
				}


				SaveLocation = string.Empty;

				train = null;
				panel = null;
				sound = null;

				CreateNewFile();
			}
		}

		internal void SaveFile()
		{
			if (string.IsNullOrEmpty(SaveLocation))
			{
				SaveAsFile();
				return;
			}

			try
			{
				IntermediateFile.Write(saveLocation, Train, Panel, Sound);

				SystemSounds.Asterisk.Play();
			}
			catch (Exception e)
			{
				MessageBox = new MessageBox
				{
					Title = Utilities.GetInterfaceString("menu", "file", "save"),
					Icon = BaseDialog.DialogIcon.Error,
					Button = BaseDialog.DialogButton.Ok,
					Text = e.Message,
					IsOpen = true
				};
			}
		}

		internal void SaveAsFile()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"Intermediate files (*.te)|*.te|All files (*.*)|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			SaveLocation = SaveFileDialog.FileName;

			try
			{
				IntermediateFile.Write(saveLocation, Train, Panel, Sound);
			}
			catch (Exception e)
			{
				MessageBox = new MessageBox
				{
					Title = Utilities.GetInterfaceString("menu", "file", "save_as"),
					Icon = BaseDialog.DialogIcon.Error,
					Button = BaseDialog.DialogButton.Ok,
					Text = e.Message,
					IsOpen = true
				};

				SaveLocation = string.Empty;
			}
		}

		internal void ImportFiles()
		{
			try
			{
				switch (CurrentTrainFileType)
				{
					case TrainFileType.OldFormat:
						if (!string.IsNullOrEmpty(TrainDatImportLocation))
						{
							TrainDat.Parse(TrainDatImportLocation, out train);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));
						}

						if (!string.IsNullOrEmpty(ExtensionsCfgImportLocation))
						{
							ExtensionsCfg.Parse(ExtensionsCfgImportLocation, Train);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				CreateTreeItem();

				foreach (MotorCar car in Train.Cars.OfType<MotorCar>())
				{
					car.Motor.CreateTreeItem();
				}
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			try
			{
				switch (CurrentPanelFileType)
				{
					case PanelFileType.Panel2Cfg:
						if (!string.IsNullOrEmpty(Panel2CfgImportLocation))
						{
							PanelCfgBve4.Parse(Panel2CfgImportLocation, out panel);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Panel)));
						}
						break;
					case PanelFileType.PanelXml:
						if (!string.IsNullOrEmpty(PanelXmlImportLocation))
						{
							PanelCfgXml.Parse(PanelXmlImportLocation, out panel);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Panel)));
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Panel.CreateTreeItem();
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			try
			{
				switch (CurrentSoundFileType)
				{
					case SoundFileType.NoSettingFile:
						if (!string.IsNullOrEmpty(TrainFolderImportLocation))
						{
							SoundCfgBve2.Parse(TrainFolderImportLocation, out sound);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sound)));
						}
						break;
					case SoundFileType.SoundCfg:
						if (!string.IsNullOrEmpty(SoundCfgImportLocation))
						{
							SoundCfgBve4.Parse(SoundCfgImportLocation, out sound);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sound)));
						}
						break;
					case SoundFileType.SoundXml:
						if (!string.IsNullOrEmpty(SoundXmlImportLocation))
						{
							SoundCfgXml.Parse(SoundXmlImportLocation, out sound);
							OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sound)));
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Sound.CreateTreeItem();
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
			}

			SelectedTreeItem = null;
		}

		internal void ExportFiles()
		{
			try
			{
				switch (CurrentTrainFileType)
				{
					case TrainFileType.OldFormat:
						if (!string.IsNullOrEmpty(TrainDatExportLocation))
						{
							TrainDat.Write(TrainDatExportLocation, Train);
						}

						if (!string.IsNullOrEmpty(ExtensionsCfgExportLocation))
						{
							ExtensionsCfg.Write(ExtensionsCfgExportLocation, Train);
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

			try
			{
				switch (CurrentPanelFileType)
				{
					case PanelFileType.Panel2Cfg:
						if (!string.IsNullOrEmpty(Panel2CfgExportLocation))
						{
							PanelCfgBve4.Write(Panel2CfgExportLocation, Panel);
						}
						break;
					case PanelFileType.PanelXml:
						if (!string.IsNullOrEmpty(PanelXmlExportLocation))
						{
							PanelCfgXml.Write(PanelXmlExportLocation, Panel);
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

			try
			{
				switch (CurrentSoundFileType)
				{
					case SoundFileType.NoSettingFile:
						break;
					case SoundFileType.SoundCfg:
						if (!string.IsNullOrEmpty(SoundCfgExportLocation))
						{
							SoundCfgBve4.Write(SoundCfgExportLocation, Sound);
						}
						break;
					case SoundFileType.SoundXml:
						if (!string.IsNullOrEmpty(SoundXmlExportLocation))
						{
							SoundCfgXml.Write(SoundXmlExportLocation, Sound);
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

		internal void OutputLogs()
		{
			SaveFileDialog = new SaveFileDialog
			{
				Filter = @"Text files (*.txt)|*.txt|All files (*.*)|*",
				OverwritePrompt = true,
				IsOpen = true
			};

			if (SaveFileDialog.DialogResult != true)
			{
				return;
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"TrainEditor2 Log: {DateTime.Now}");

			foreach (string message in Interface.LogMessages.Select(x => $"{x.Type.ToString()}: {x.Text}"))
			{
				builder.AppendLine(message);
			}

			File.WriteAllText(SaveFileDialog.FileName, builder.ToString());
		}

		private void RenameTreeViewItem(ObservableCollection<TreeViewItemModel> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].Title = i.ToString(culture);
			}
		}

		internal void UpCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedTreeItem.Tag);
			Train.Cars.Move(index, index - 1);

			TreeItems[0].Children[1].Children.Move(index, index - 1);
			RenameTreeViewItem(TreeItems[0].Children[1].Children);
		}

		internal void DownCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedTreeItem.Tag);
			Train.Cars.Move(index, index + 1);

			TreeItems[0].Children[1].Children.Move(index, index + 1);
			RenameTreeViewItem(TreeItems[0].Children[1].Children);
		}

		internal void AddCar()
		{
			Train.Cars.Add(new TrailerCar());
			Train.Couplers.Add(new Coupler());

			Train.ApplyPowerNotchesToCar();
			Train.ApplyBrakeNotchesToCar();
			Train.ApplyLocoBrakeNotchesToCar();

			TreeItems[0].Children[1].Children.Add(new TreeViewItemModel(TreeItems[0].Children[1]) { Title = (Train.Cars.Count - 1).ToString(culture), Tag = Train.Cars.Last() });
			TreeItems[0].Children[2].Children.Add(new TreeViewItemModel(TreeItems[0].Children[2]) { Title = (Train.Couplers.Count - 1).ToString(culture), Tag = Train.Couplers.Last() });
			SelectedTreeItem = TreeItems[0].Children[1].Children.Last();
		}

		internal void RemoveCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedTreeItem.Tag);
			Train.Cars.RemoveAt(index);
			Train.Couplers.RemoveAt(index == 0 ? 0 : index - 1);

			TreeItems[0].Children[1].Children.RemoveAt(index);
			TreeItems[0].Children[2].Children.RemoveAt(index == 0 ? 0 : index - 1);
			RenameTreeViewItem(TreeItems[0].Children[1].Children);
			RenameTreeViewItem(TreeItems[0].Children[2].Children);

			SelectedTreeItem = null;
		}

		internal void CopyCar()
		{
			Train.Cars.Add((Car)((Car)SelectedTreeItem.Tag).Clone());
			Train.Couplers.Add(new Coupler());

			TreeItems[0].Children[1].Children.Add(new TreeViewItemModel(TreeItems[0].Children[1]) { Title = (Train.Cars.Count - 1).ToString(culture), Tag = Train.Cars.Last() });
			TreeItems[0].Children[2].Children.Add(new TreeViewItemModel(TreeItems[0].Children[2]) { Title = (Train.Couplers.Count - 1).ToString(culture), Tag = Train.Couplers.Last() });
			SelectedTreeItem = TreeItems[0].Children[1].Children.Last();
		}

		internal void UpCoupler()
		{
			int index = Train.Couplers.IndexOf((Coupler)SelectedTreeItem.Tag);
			Train.Couplers.Move(index, index - 1);

			TreeItems[0].Children[2].Children.Move(index, index - 1);
			RenameTreeViewItem(TreeItems[0].Children[2].Children);
		}

		internal void DownCoupler()
		{
			int index = Train.Couplers.IndexOf((Coupler)SelectedTreeItem.Tag);
			Train.Couplers.Move(index, index + 1);

			TreeItems[0].Children[2].Children.Move(index, index + 1);
			RenameTreeViewItem(TreeItems[0].Children[2].Children);
		}

		internal void ChangeCarClass(int carIndex)
		{
			MotorCar motorCar = Train.Cars[carIndex] as MotorCar;
			TrailerCar trailerCar = Train.Cars[carIndex] as TrailerCar;

			if (motorCar != null)
			{
				Train.Cars[carIndex] = new TrailerCar(motorCar);
			}

			if (trailerCar != null)
			{
				Train.Cars[carIndex] = new MotorCar(trailerCar);

				Train.ApplyPowerNotchesToCar();
			}

			TreeItems[0].Children[1].Children[carIndex].Tag = Train.Cars[carIndex];
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedTreeItem)));
		}

		private string GetMessageTypeString(MessageType type)
		{
			switch (type)
			{
				case MessageType.Information:
					return Utilities.GetInterfaceString("message", "info");
				case MessageType.Warning:
					return Utilities.GetInterfaceString("message", "warning");
				case MessageType.Error:
				case MessageType.Critical:
					return Utilities.GetInterfaceString("message", "error");
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		internal void AddLogMessage(LogMessage message)
		{
			VisibleLogMessages
				.Add(new ListViewItemModel
				{
					SubItems = new ObservableCollection<ListViewSubItemModel>(new[] { new ListViewSubItemModel { Text = GetMessageTypeString(message.Type) }, new ListViewSubItemModel { Text = message.Text } }),
					ImageIndex = (int)message.Type,
					Tag = message
				});
		}

		internal void RemoveLogMessage(LogMessage message)
		{
			VisibleLogMessages.RemoveAll(x => x.Tag == message);
		}

		internal void ResetLogMessages()
		{
			VisibleLogMessages.Clear();
		}

		internal void ChangeVisibleLogMessages(MessageType type, bool visible)
		{
			if (visible)
			{
				VisibleLogMessages
					.AddRange(
						Interface.LogMessages
							.Where(x => x.Type == type)
							.Select(x => new ListViewItemModel
							{
								SubItems = new ObservableCollection<ListViewSubItemModel>(new[] { new ListViewSubItemModel { Text = GetMessageTypeString(x.Type) }, new ListViewSubItemModel { Text = x.Text } }),
								ImageIndex = (int)x.Type,
								Tag = x
							})
					);
			}
			else
			{
				VisibleLogMessages.RemoveAll(x => ((LogMessage)x.Tag).Type == type);
			}
		}
	}
}
