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
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using TouchElement = TrainEditor2.Models.Panels.TouchElement;

namespace TrainEditor2.Models
{
	internal class App : BindableBase
	{
		private readonly CultureInfo culture;

		private string saveLocation;
		private string currentLanguageCode;

		private Train train;
		private Sound sound;

		private MessageBox messageBox;
		private OpenFileDialog openFileDialog;
		private SaveFileDialog saveFileDialog;

		private ImportTrainFile importTrainFile;
		private ImportPanelFile importPanelFile;
		private ImportSoundFile importSoundFile;

		private ExportTrainFile exportTrainFile;
		private ExportPanelFile exportPanelFile;
		private ExportSoundFile exportSoundFile;

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

		internal ImportTrainFile ImportTrainFile
		{
			get
			{
				return importTrainFile;
			}
			set
			{
				SetProperty(ref importTrainFile, value);
			}
		}

		internal ImportPanelFile ImportPanelFile
		{
			get
			{
				return importPanelFile;
			}
			set
			{
				SetProperty(ref importPanelFile, value);
			}
		}

		internal ImportSoundFile ImportSoundFile
		{
			get
			{
				return importSoundFile;
			}
			set
			{
				SetProperty(ref importSoundFile, value);
			}
		}

		internal ExportTrainFile ExportTrainFile
		{
			get
			{
				return exportTrainFile;
			}
			set
			{
				SetProperty(ref exportTrainFile, value);
			}
		}

		internal ExportPanelFile ExportPanelFile
		{
			get
			{
				return exportPanelFile;
			}
			set
			{
				SetProperty(ref exportPanelFile, value);
			}
		}

		internal ExportSoundFile ExportSoundFile
		{
			get
			{
				return exportSoundFile;
			}
			set
			{
				SetProperty(ref exportSoundFile, value);
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

			ImportTrainFile = new ImportTrainFile(this);
			ImportPanelFile = new ImportPanelFile(this);
			ImportSoundFile = new ImportSoundFile(this);

			ExportTrainFile = new ExportTrainFile(this);
			ExportPanelFile = new ExportPanelFile(this);
			ExportSoundFile = new ExportSoundFile(this);

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
			if (Train != null || Sound != null)
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
			train.Cars.Add(new ControlledMotorCar());
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));

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
				IntermediateFile.Parse(SaveLocation, out train, out sound);

				OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sound)));

				CreateTreeItem();

				foreach (Motor motor in Train.Cars.OfType<MotorCar>().Select(x => x.Motor))
				{
					motor.CreateTreeItem();
				}

				foreach (Panel panel in Train.Cars.OfType<ControlledMotorCar>().Select(x => x.Cab).OfType<EmbeddedCab>().Select(x => x.Panel))
				{
					panel.CreateTreeItem();

					foreach (TouchElement touch in panel.Screens.SelectMany(x => x.TouchElements))
					{
						touch.CreateTreeItem();
					}
				}

				foreach (Panel panel in Train.Cars.OfType<ControlledTrailerCar>().Select(x => x.Cab).OfType<EmbeddedCab>().Select(x => x.Panel))
				{
					panel.CreateTreeItem();

					foreach (TouchElement touch in panel.Screens.SelectMany(x => x.TouchElements))
					{
						touch.CreateTreeItem();
					}
				}

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
				IntermediateFile.Write(saveLocation, Train, Sound);

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
				IntermediateFile.Write(saveLocation, Train, Sound);
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

			try
			{
				File.WriteAllText(SaveFileDialog.FileName, builder.ToString());
			}
			catch (Exception e)
			{
				MessageBox = new MessageBox
				{
					Title = @"Output logs...",
					Icon = BaseDialog.DialogIcon.Error,
					Button = BaseDialog.DialogButton.Ok,
					Text = e.Message,
					IsOpen = true
				};
			}
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
			Train.Cars.Add(new UncontrolledTrailerCar());
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

		internal void ChangeBaseCarClass()
		{
			int index = Train.Cars.IndexOf((Car)SelectedTreeItem.Tag);

			if (Train.Cars[index] is MotorCar)
			{
				ControlledMotorCar controlledMotorCar = Train.Cars[index] as ControlledMotorCar;

				if (controlledMotorCar != null)
				{
					Train.Cars[index] = new ControlledTrailerCar(controlledMotorCar);
				}
				else
				{
					Train.Cars[index] = new UncontrolledTrailerCar(Train.Cars[index]);
				}
			}
			else
			{
				ControlledTrailerCar controlledTrailerCar = Train.Cars[index] as ControlledTrailerCar;

				if (controlledTrailerCar != null)
				{
					Train.Cars[index] = new ControlledMotorCar(controlledTrailerCar);
				}
				else
				{
					Train.Cars[index] = new UncontrolledMotorCar(Train.Cars[index]);
				}

				Train.ApplyPowerNotchesToCar();
			}

			TreeItems[0].Children[1].Children[index].Tag = Train.Cars[index];
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedTreeItem)));
		}

		internal void ChangeControlledCarClass()
		{
			int index = Train.Cars.IndexOf((Car)SelectedTreeItem.Tag);

			MotorCar motorCar = Train.Cars[index] as MotorCar;

			if (motorCar != null)
			{
				if (motorCar is ControlledMotorCar)
				{
					Train.Cars[index] = new UncontrolledMotorCar(motorCar);
				}
				else
				{
					Train.Cars[index] = new ControlledMotorCar(motorCar);
				}
			}
			else
			{
				if (Train.Cars[index] is ControlledTrailerCar)
				{
					Train.Cars[index] = new UncontrolledTrailerCar(Train.Cars[index]);
				}
				else
				{
					Train.Cars[index] = new ControlledTrailerCar(Train.Cars[index]);
				}
			}

			TreeItems[0].Children[1].Children[index].Tag = Train.Cars[index];
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedTreeItem)));
		}

		internal void ChangeCabClass()
		{
			ControlledMotorCar controlledMotorCar = SelectedTreeItem.Tag as ControlledMotorCar;
			ControlledTrailerCar controlledTrailerCar = SelectedTreeItem.Tag as ControlledTrailerCar;

			if (controlledMotorCar != null)
			{
				if (controlledMotorCar.Cab is EmbeddedCab)
				{
					controlledMotorCar.Cab = new ExternalCab(controlledMotorCar.Cab);
				}
				else
				{
					controlledMotorCar.Cab = new EmbeddedCab(controlledMotorCar.Cab);
				}
			}

			if (controlledTrailerCar != null)
			{
				if (controlledTrailerCar.Cab is EmbeddedCab)
				{
					controlledTrailerCar.Cab = new ExternalCab(controlledTrailerCar.Cab);
				}
				else
				{
					controlledTrailerCar.Cab = new EmbeddedCab(controlledTrailerCar.Cab);
				}
			}

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
