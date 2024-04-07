using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Systems;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		internal static TreeNode TreeViewItemViewModelToTreeNode(TreeViewItemViewModel item)
		{
			return new TreeNode(item.Title.Value, item.Children.Select(TreeViewItemViewModelToTreeNode).ToArray()) { Tag = item };
		}

		internal static TreeNode SearchTreeNode(TreeViewItemViewModel item, TreeNode node)
		{
			return node.Tag == item ? node : node.Nodes.OfType<TreeNode>().Select(x => SearchTreeNode(item, x)).FirstOrDefault(x => x != null);
		}

		internal static ColumnHeader ListViewColumnHeaderViewModelToColumnHeader(ListViewColumnHeaderViewModel column)
		{
			return new ColumnHeader { Text = column.Text.Value, Tag = column };
		}

		internal static ListViewItem ListViewItemViewModelToListViewItem(ListViewItemViewModel item)
		{
			return new ListViewItem(item.Texts.ToArray()) { ImageIndex = item.ImageIndex.Value, Tag = item };
		}

		internal static void UpdateListViewItem(ListViewItem item, ListViewItemViewModel viewModel)
		{
			for (int i = 0; i < viewModel.Texts.Count; i++)
			{
				item.SubItems[i].Text = viewModel.Texts[i];
			}
		}

		private InputEventModel.EventArgs MouseEventArgsToModel(MouseEventArgs e)
		{
			return new InputEventModel.EventArgs
			{
				LeftButton = e.Button == MouseButtons.Left ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				MiddleButton = e.Button == MouseButtons.Middle ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				RightButton = e.Button == MouseButtons.Right ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				XButton1 = e.Button == MouseButtons.XButton1 ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				XButton2 = e.Button == MouseButtons.XButton2 ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				X = e.X,
				Y = e.Y
			};
		}

		private Cursor CursorTypeToCursor(InputEventModel.CursorType cursorType)
		{
			switch (cursorType)
			{
				case InputEventModel.CursorType.No:
					return Cursors.No;
				case InputEventModel.CursorType.Arrow:
					return Cursors.Arrow;
				case InputEventModel.CursorType.AppStarting:
					return Cursors.AppStarting;
				case InputEventModel.CursorType.Cross:
					return Cursors.Cross;
				case InputEventModel.CursorType.Help:
					return Cursors.Help;
				case InputEventModel.CursorType.IBeam:
					return Cursors.IBeam;
				case InputEventModel.CursorType.SizeAll:
					return Cursors.SizeAll;
				case InputEventModel.CursorType.SizeNESW:
					return Cursors.SizeNESW;
				case InputEventModel.CursorType.SizeNS:
					return Cursors.SizeNS;
				case InputEventModel.CursorType.SizeNWSE:
					return Cursors.SizeNWSE;
				case InputEventModel.CursorType.SizeWE:
					return Cursors.SizeWE;
				case InputEventModel.CursorType.UpArrow:
					return Cursors.UpArrow;
				case InputEventModel.CursorType.Wait:
					return Cursors.WaitCursor;
				case InputEventModel.CursorType.Hand:
					return Cursors.Hand;
				case InputEventModel.CursorType.ScrollNS:
					return Cursors.NoMoveVert;
				case InputEventModel.CursorType.ScrollWE:
					return Cursors.NoMoveHoriz;
				case InputEventModel.CursorType.ScrollAll:
					return Cursors.NoMove2D;
				case InputEventModel.CursorType.ScrollN:
					return Cursors.PanNorth;
				case InputEventModel.CursorType.ScrollS:
					return Cursors.PanSouth;
				case InputEventModel.CursorType.ScrollW:
					return Cursors.PanWest;
				case InputEventModel.CursorType.ScrollE:
					return Cursors.PanEast;
				case InputEventModel.CursorType.ScrollNW:
					return Cursors.PanNW;
				case InputEventModel.CursorType.ScrollNE:
					return Cursors.PanNE;
				case InputEventModel.CursorType.ScrollSW:
					return Cursors.PanSW;
				case InputEventModel.CursorType.ScrollSE:
					return Cursors.PanSE;
				default:
					throw new ArgumentOutOfRangeException(nameof(cursorType), cursorType, null);
			}
		}

		private void ModifierKeysDownUp(KeyEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			if (glControlMotor.Focused)
			{
				if (car != null)
				{
					car.Motor.Value.CurrentModifierKeys.Value = InputEventModel.ModifierKeys.None;

					if (e.Alt)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Alt;
					}

					if (e.Control)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Control;
					}

					if (e.Shift)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Shift;
					}
				}
			}
		}

		internal static void OpenFileDialog(TextBox textBox)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = @"All files (*.*)|*";
				dialog.CheckFileExists = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				textBox.Text = dialog.FileName;
			}
		}

		private void OpenColorDialog(TextBox textBox)
		{
			using (ColorDialog dialog = new ColorDialog())
			{
				Color24.TryParseHexColor(textBox.Text, out Color24 nowColor);
				dialog.Color = nowColor;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				textBox.Text = ((Color24)dialog.Color).ToString();
			}
		}

		internal static Icon GetIcon()
		{
			return new Icon(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico"));
		}

		private Bitmap GetImage(string path)
		{
			string folder = Program.FileSystem.GetDataFolder("TrainEditor2");
			Bitmap image = new Bitmap(OpenBveApi.Path.CombineFile(folder, path));
			image.MakeTransparent();
			return image;
		}

		private void ApplyLanguage()
		{
			toolStripMenuItemFile.Text = $@"{Utilities.GetInterfaceString("menu", "file", "name")}(&F)";
			toolStripMenuItemNew.Text = $@"{Utilities.GetInterfaceString("menu", "file", "new")}(&N)";
			toolStripMenuItemOpen.Text = $@"{Utilities.GetInterfaceString("menu", "file", "open")}(&O)";
			toolStripMenuItemSave.Text = $@"{Utilities.GetInterfaceString("menu", "file", "save")}(&S)";
			toolStripMenuItemSaveAs.Text = $@"{Utilities.GetInterfaceString("menu", "file", "save_as")}(&A)";
			toolStripMenuItemImport.Text = Utilities.GetInterfaceString("menu", "file", "import");
			toolStripMenuItemExport.Text = Utilities.GetInterfaceString("menu", "file", "export");
			toolStripMenuItemExit.Text = $@"{Utilities.GetInterfaceString("menu", "file", "exit")}(&X)";

			toolStripStatusLabelLanguage.Text = $@"{Utilities.GetInterfaceString("menu", "language")}:";

			buttonCarsUp.Text = Utilities.GetInterfaceString("navigation", "up");
			buttonCarsDown.Text = Utilities.GetInterfaceString("navigation", "down");
			buttonCarsAdd.Text = Utilities.GetInterfaceString("navigation", "add");
			buttonCarsRemove.Text = Utilities.GetInterfaceString("navigation", "remove");
			buttonCarsCopy.Text = Utilities.GetInterfaceString("navigation", "copy");

			tabPageTrain.Text = Utilities.GetInterfaceString("general_settings", "name");

			groupBoxHandle.Text = Utilities.GetInterfaceString("general_settings", "handle", "name");
			labelHandleType.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "handle_type", "name")}:";
			comboBoxHandleType.Items[0] = Utilities.GetInterfaceString("general_settings", "handle", "handle_type", "separated");
			comboBoxHandleType.Items[1] = Utilities.GetInterfaceString("general_settings", "handle", "handle_type", "combined");
			comboBoxHandleType.Items[2] = Utilities.GetInterfaceString("general_settings", "handle", "handle_type", "separated_interlocked");
			comboBoxHandleType.Items[3] = Utilities.GetInterfaceString("general_settings", "handle", "handle_type", "separated_interlocked_reverser");
			labelPowerNotches.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "power_notches")}:";
			labelBrakeNotches.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "brake_notches")}:";
			labelPowerNotchReduceSteps.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "power_notch_reduce_steps")}:";
			labelDriverPowerNotches.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "driver_power_notches")}:";
			labelDriverBrakeNotches.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "driver_brake_notches")}:";
			labelEbHandleBehaviour.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "eb_handle_behaviour", "name")}:";
			comboBoxEbHandleBehaviour.Items[0] = Utilities.GetInterfaceString("general_settings", "handle", "eb_handle_behaviour", "no");
			comboBoxEbHandleBehaviour.Items[1] = Utilities.GetInterfaceString("general_settings", "handle", "eb_handle_behaviour", "power");
			comboBoxEbHandleBehaviour.Items[2] = Utilities.GetInterfaceString("general_settings", "handle", "eb_handle_behaviour", "reverser");
			comboBoxEbHandleBehaviour.Items[3] = Utilities.GetInterfaceString("general_settings", "handle", "eb_handle_behaviour", "power_reverser");
			labelLocoBrakeHandleType.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "loco_brake_handle_type", "name")}:";
			comboBoxLocoBrakeHandleType.Items[0] = Utilities.GetInterfaceString("general_settings", "handle", "loco_brake_handle_type", "combined");
			comboBoxLocoBrakeHandleType.Items[1] = Utilities.GetInterfaceString("general_settings", "handle", "loco_brake_handle_type", "independent");
			comboBoxLocoBrakeHandleType.Items[2] = Utilities.GetInterfaceString("general_settings", "handle", "loco_brake_handle_type", "blocking");
			labelLocoBrakeNotches.Text = $@"{Utilities.GetInterfaceString("general_settings", "handle", "loco_brake_notches")}:";
			labelDelayElectricBrake.Text = $@"{Utilities.GetInterfaceString("car_settings", "delay", "electric_brake")}:";

			groupBoxCab.Text = Utilities.GetInterfaceString("general_settings", "cab", "name");
			labelCabX.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "x")}:";
			labelCabY.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "y")}:";
			labelCabZ.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "z")}:";
			labelDriverCar.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "driver_car")}:";

			groupBoxDevice.Text = Utilities.GetInterfaceString("general_settings", "device", "name");
			labelAts.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "ats", "name")}:";
			comboBoxAts.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "ats", "none");
			comboBoxAts.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "ats", "ats_sn");
			comboBoxAts.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "ats", "ats_sn_ats_p");
			labelAtc.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "atc", "name")}:";
			comboBoxAtc.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "atc", "none");
			comboBoxAtc.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "atc", "manual");
			comboBoxAtc.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "atc", "automatic");
			labelEb.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "eb")}:";
			labelConstSpeed.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "const_speed")}:";
			labelHoldBrake.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "hold_brake")}:";
			labelReAdhesionDevice.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "name")}:";
			comboBoxReAdhesionDevice.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "none");
			comboBoxReAdhesionDevice.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "type_a");
			comboBoxReAdhesionDevice.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "type_b");
			comboBoxReAdhesionDevice.Items[3] = Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "type_c");
			comboBoxReAdhesionDevice.Items[4] = Utilities.GetInterfaceString("general_settings", "device", "re_adhesion_device", "type_d");
			labelPassAlarm.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "pass_alarm", "name")}:";
			comboBoxPassAlarm.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "pass_alarm", "none");
			comboBoxPassAlarm.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "pass_alarm", "single");
			comboBoxPassAlarm.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "pass_alarm", "looping");
			labelDoorOpenMode.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "door_mode", "open")}:";
			comboBoxDoorOpenMode.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "semi_automatic");
			comboBoxDoorOpenMode.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "automatic");
			comboBoxDoorOpenMode.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "manual");
			labelDoorCloseMode.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "door_mode", "close")}:";
			comboBoxDoorCloseMode.Items[0] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "semi_automatic");
			comboBoxDoorCloseMode.Items[1] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "automatic");
			comboBoxDoorCloseMode.Items[2] = Utilities.GetInterfaceString("general_settings", "device", "door_mode", "manual");
			labelDoorWidth.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "door_width")}:";
			labelDoorMaxTolerance.Text = $@"{Utilities.GetInterfaceString("general_settings", "device", "door_max_tolerance")}:";

			tabPageCar.Text = Utilities.GetInterfaceString("car_settings", "name");

			groupBoxCarGeneral.Text = Utilities.GetInterfaceString("car_settings", "general", "name");
			labelIsMotorCar.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "is_motor_car")}:";
			labelMass.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "mass")}:";
			labelLength.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "length")}:";
			labelWidth.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "width")}:";
			labelHeight.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "height")}:";
			labelCenterOfMassHeight.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "center_of_gravity_height")}:";
			labelExposedFrontalArea.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "exposed_frontal_area")}:";
			labelUnexposedFrontalArea.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "unexposed_frontal_area")}:";
			labelDefinedAxles.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "defined_axles")}:";
			groupBoxAxles.Text = Utilities.GetInterfaceString("car_settings", "general", "axles", "name");
			labelFrontAxle.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "axles", "front")}:";
			labelRearAxle.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "axles", "rear")}:";
			labelFrontBogie.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "front_bogie")}:";
			labelRearBogie.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "rear_bogie")}:";
			labelLoadingSway.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "loading_sway")}:";
			labelReversed.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "reversed")}:";
			labelObject.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "object")}:";

			groupBoxPerformance.Text = Utilities.GetInterfaceString("car_settings", "performance", "name");
			labelDeceleration.Text = $@"{Utilities.GetInterfaceString("car_settings", "performance", "deceleration")}:";
			labelCoefficientOfStaticFriction.Text = $@"{Utilities.GetInterfaceString("car_settings", "performance", "static_friction")}:";
			labelCoefficientOfRollingResistance.Text = $@"{Utilities.GetInterfaceString("car_settings", "performance", "rolling_resistance")}:";
			labelAerodynamicDragCoefficient.Text = $@"{Utilities.GetInterfaceString("car_settings", "performance", "aerodynamic_drag")}:";

			groupBoxMove.Text = Utilities.GetInterfaceString("car_settings", "move", "name");
			labelJerkPowerUp.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "jerk_power_up")}:";
			labelJerkPowerDown.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "jerk_power_down")}:";
			labelJerkBrakeUp.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "jerk_brake_up")}:";
			labelJerkBrakeDown.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "jerk_brake_down")}:";
			labelBrakeCylinderUp.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "brake_cylinder_up")}:";
			labelBrakeCylinderDown.Text = $@"{Utilities.GetInterfaceString("car_settings", "move", "brake_cylinder_down")}:";

			groupBoxBrake.Text = Utilities.GetInterfaceString("car_settings", "brake", "name");
			labelBrakeType.Text = $@"{Utilities.GetInterfaceString("car_settings", "brake", "brake_type", "name")}:";
			comboBoxBrakeType.Items[0] = Utilities.GetInterfaceString("car_settings", "brake", "brake_type", "smee");
			comboBoxBrakeType.Items[1] = Utilities.GetInterfaceString("car_settings", "brake", "brake_type", "ecb");
			comboBoxBrakeType.Items[2] = Utilities.GetInterfaceString("car_settings", "brake", "brake_type", "cl");
			labelLocoBrakeType.Text = $@"{Utilities.GetInterfaceString("car_settings", "brake", "loco_brake_type", "name")}:";
			comboBoxLocoBrakeType.Items[0] = Utilities.GetInterfaceString("car_settings", "brake", "loco_brake_type", "none");
			comboBoxLocoBrakeType.Items[1] = Utilities.GetInterfaceString("car_settings", "brake", "loco_brake_type", "notched_air_brake");
			comboBoxLocoBrakeType.Items[2] = Utilities.GetInterfaceString("car_settings", "brake", "loco_brake_type", "cl");
			labelBrakeControlSystem.Text = $@"{Utilities.GetInterfaceString("car_settings", "brake", "brake_control_system", "name")}:";
			comboBoxBrakeControlSystem.Items[0] = Utilities.GetInterfaceString("car_settings", "brake", "brake_control_system", "none");
			comboBoxBrakeControlSystem.Items[1] = Utilities.GetInterfaceString("car_settings", "brake", "brake_control_system", "lock_out_valve");
			comboBoxBrakeControlSystem.Items[2] = Utilities.GetInterfaceString("car_settings", "brake", "brake_control_system", "delay_including_control");
			labelBrakeControlSpeed.Text = $@"{Utilities.GetInterfaceString("car_settings", "brake", "brake_control_speed")}:";

			groupBoxPressure.Text = Utilities.GetInterfaceString("car_settings", "pressure", "name");
			labelBrakeCylinderServiceMaximumPressure.Text = $@"{Utilities.GetInterfaceString("car_settings", "pressure", "brake_cylinder_service_max")}:";
			labelBrakeCylinderEmergencyMaximumPressure.Text = $@"{Utilities.GetInterfaceString("car_settings", "pressure", "brake_cylinder_emergency_max")}:";
			labelMainReservoirMinimumPressure.Text = $@"{Utilities.GetInterfaceString("car_settings", "pressure", "main_reservoir_min")}:";
			labelMainReservoirMaximumPressure.Text = $@"{Utilities.GetInterfaceString("car_settings", "pressure", "main_reservoir_max")}:";
			labelBrakePipeNormalPressure.Text = $@"{Utilities.GetInterfaceString("car_settings", "pressure", "brake_pipe_normal")}:";

			groupBoxDelay.Text = Utilities.GetInterfaceString("car_settings", "delay", "name");
			labelDelayPower.Text = $@"{Utilities.GetInterfaceString("car_settings", "delay", "power")}:";
			labelDelayBrake.Text = $@"{Utilities.GetInterfaceString("car_settings", "delay", "brake")}:";
			labelDelayLocoBrake.Text = $@"{Utilities.GetInterfaceString("car_settings", "delay", "loco_brake")}:";

			tabPageAccel.Text = Utilities.GetInterfaceString("acceleration_settings", "name");
			groupBoxNotch.Text = Utilities.GetInterfaceString("acceleration_settings", "notch");
			groupBoxParameter.Text = Utilities.GetInterfaceString("acceleration_settings", "parameter", "name");
			labelAccelA0.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "parameter", "a0")}:";
			labelAccelA1.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "parameter", "a1")}:";
			labelAccelV1.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "parameter", "v1")}:";
			labelAccelV2.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "parameter", "v2")}:";
			labelAccelE.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "parameter", "e")}:";
			groupBoxPreview.Text = Utilities.GetInterfaceString("acceleration_settings", "preview", "name");
			checkBoxSubtractDeceleration.Text = Utilities.GetInterfaceString("acceleration_settings", "preview", "subtract_deceleration");
			labelAccelX.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "x")}:";
			labelAccelY.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "y")}:";
			labelAccelXmin.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "x_min")}:";
			labelAccelXmax.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "x_max")}:";
			labelAccelYmin.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "y_min")}:";
			labelAccelYmax.Text = $@"{Utilities.GetInterfaceString("acceleration_settings", "preview", "y_max")}:";

			tabPageMotor.Text = Utilities.GetInterfaceString("motor_sound_settings", "name");

			toolStripMenuItemEdit.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "name")}(&E)";
			toolStripMenuItemUndo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "undo")}(&U)";
			toolStripMenuItemRedo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "redo")}(&R)";
			toolStripMenuItemTearingOff.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cut")}(&T)";
			toolStripMenuItemCopy.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "copy")}(&C)";
			toolStripMenuItemPaste.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "paste")}(&P)";
			toolStripMenuItemCleanup.Text = Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cleanup");
			toolStripMenuItemDelete.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "delete")}(&D)";

			toolStripButtonUndo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "undo")} (Ctrl+Z)";
			toolStripButtonRedo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "redo")} (Ctrl+Y)";
			toolStripButtonTearingOff.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cut")} (Ctrl+X)";
			toolStripButtonCopy.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "copy")} (Ctrl+C)";
			toolStripButtonPaste.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "paste")} (Ctrl+V)";
			toolStripButtonCleanup.Text = Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cleanup");
			toolStripButtonDelete.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "delete")} (Del)";

			toolStripMenuItemView.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "view", "name")}(&V)";
			toolStripMenuItemPower.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "view", "power")}(&P)";
			toolStripMenuItemBrake.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "view", "brake")}(&B)";
			toolStripMenuItemPowerTrack1.Text = toolStripMenuItemBrakeTrack1.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "view", "track")}1(&1)";
			toolStripMenuItemPowerTrack2.Text = toolStripMenuItemBrakeTrack2.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "view", "track")}2(&2)";

			toolStripMenuItemInput.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "input", "name")}(&I)";
			toolStripMenuItemPitch.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "input", "pitch")}(&P)";
			toolStripMenuItemVolume.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "input", "volume")}(&V)";
			toolStripMenuItemIndex.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "input", "sound_index")}(&I)";
			toolStripComboBoxIndex.Items[0] = Utilities.GetInterfaceString("motor_sound_settings", "menu", "input", "sound_index_none");

			toolStripMenuItemTool.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "name")}(&T)";
			toolStripMenuItemSelect.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "select")}(&S)";
			toolStripMenuItemMove.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "move")}(&M)";
			toolStripMenuItemDot.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "dot")}(&D)";
			toolStripMenuItemLine.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "line")}(&L)";

			toolStripButtonSelect.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "select")} (Alt+A)";
			toolStripButtonMove.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "move")} (Alt+S)";
			toolStripButtonDot.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "dot")} (Alt+D)";
			toolStripButtonLine.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "tool", "line")} (Alt+F)";

			groupBoxView.Text = Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "name");
			labelMotorMinVelocity.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "min_velocity")}:";
			labelMotorMaxVelocity.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "max_velocity")}:";
			labelMotorMinPitch.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "min_pitch")}:";
			labelMotorMaxPitch.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "max_pitch")}:";
			labelMotorMinVolume.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "min_volume")}:";
			labelMotorMaxVolume.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "view_setting", "max_volume")}:";

			groupBoxDirect.Text = Utilities.GetInterfaceString("motor_sound_settings", "direct_input", "name");
			labelDirectX.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "direct_input", "x")}:";
			labelDirectY.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "direct_input", "y")}:";

			groupBoxPlay.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "name");

			groupBoxSource.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "source", "name");
			labelRun.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "source", "run")}:";
			checkBoxTrack1.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "source", "track")}1";
			checkBoxTrack2.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "source", "track")}2";

			groupBoxArea.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "name");
			checkBoxMotorLoop.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "loop");
			checkBoxMotorConstant.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "constant");
			labelMotorAccel.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "acceleration")}:";

			tabPageCoupler.Text = Utilities.GetInterfaceString("coupler_settings", "name");

			groupBoxCouplerGeneral.Text = Utilities.GetInterfaceString("coupler_settings", "general", "name");
			labelCouplerMin.Text = $@"{Utilities.GetInterfaceString("coupler_settings", "general", "min")}:";
			labelCouplerMax.Text = $@"{Utilities.GetInterfaceString("coupler_settings", "general", "max")}:";
			labelCouplerObject.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "object")}:";

			tabPagePanel.Text = Utilities.GetInterfaceString("panel_settings", "name");

			tabPageThis.Text = Utilities.GetInterfaceString("panel_settings", "this", "name");
			labelThisResolution.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "resolution")}:";
			labelThisLeft.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "left")}:";
			labelThisRight.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "right")}:";
			labelThisTop.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "top")}:";
			labelThisBottom.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "bottom")}:";
			labelThisDaytimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "daytime_image")}:";
			labelThisNighttimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "nighttime_image")}:";
			labelThisTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "transparent_color")}:";
			groupBoxThisCenter.Text = Utilities.GetInterfaceString("panel_settings", "this", "center", "name");
			labelThisCenterX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "center", "x")}:";
			labelThisCenterY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "center", "y")}:";
			groupBoxThisOrigin.Text = Utilities.GetInterfaceString("panel_settings", "this", "origin", "name");
			labelThisOriginX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "origin", "x")}:";
			labelThisOriginY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "this", "origin", "y")}:";

			tabPageScreen.Text = Utilities.GetInterfaceString("panel_settings", "screen", "name");
			labelScreenNumber.Text = $@"{Utilities.GetInterfaceString("panel_settings", "screen", "number")}:";
			labelScreenLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "screen", "layer")}:";

			tabPagePilotLamp.Text = Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "name");
			labelPilotLampSubject.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "subject")}:";
			groupBoxPilotLampLocation.Text = Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "location", "name");
			labelPilotLampLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "location", "x")}:";
			labelPilotLampLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "location", "y")}:";
			labelPilotLampDaytimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "daytime_image")}:";
			labelPilotLampNighttimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "nighttime_image")}:";
			labelPilotLampTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "transparent_color")}:";
			labelPilotLampLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "pilot_lamp", "layer")}:";

			tabPageNeedle.Text = Utilities.GetInterfaceString("panel_settings", "needle", "name");
			labelNeedleSubject.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "subject")}:";
			groupBoxNeedleLocation.Text = Utilities.GetInterfaceString("panel_settings", "needle", "location", "name");
			labelNeedleLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "location", "x")}:";
			labelNeedleLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "location", "y")}:";
			labelNeedleDefinedRadius.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "defined_radius")}:";
			labelNeedleRadius.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "radius")}:";
			labelNeedleDaytimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "daytime_image")}:";
			labelNeedleNighttimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "nighttime_image")}:";
			labelNeedleColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "color")}:";
			labelNeedleTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "transparent_color")}:";
			labelNeedleDefinedOrigin.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "defined_origin")}:";
			groupBoxNeedleOrigin.Text = Utilities.GetInterfaceString("panel_settings", "needle", "origin", "name");
			labelNeedleOriginX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "origin", "x")}:";
			labelNeedleOriginY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "origin", "y")}:";
			labelNeedleInitialAngle.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "initial_angle")}:";
			labelNeedleLastAngle.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "last_angle")}:";
			labelNeedleMinimum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "minimum")}:";
			labelNeedleMaximum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "maximum")}:";
			labelNeedleDefinedDampingRatio.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "defined_natural_freq")}:";
			labelNeedleDampingRatio.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "natural_freq")}:";
			labelNeedleDefinedNaturalFreq.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "defined_damping_ratio")}:";
			labelNeedleNaturalFreq.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "damping_ratio")}:";
			labelNeedleBackstop.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "backstop")}:";
			labelNeedleSmoothed.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "smoothed")}:";
			labelNeedleLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "needle", "layer")}:";

			tabPageDigitalNumber.Text = Utilities.GetInterfaceString("panel_settings", "digital_number", "name");
			labelDigitalNumberSubject.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "subject")}:";
			groupBoxDigitalNumberLocation.Text = Utilities.GetInterfaceString("panel_settings", "digital_number", "location", "name");
			labelDigitalNumberLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "location", "x")}:";
			labelDigitalNumberLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "location", "y")}:";
			labelDigitalNumberDaytimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "daytime_image")}:";
			labelDigitalNumberNighttimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "nighttime_image")}:";
			labelDigitalNumberTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "transparent_color")}:";
			labelDigitalNumberInterval.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "interval")}:";
			labelDigitalNumberLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_number", "layer")}:";

			tabPageDigitalGauge.Text = Utilities.GetInterfaceString("panel_settings", "digital_gauge", "name");
			labelDigitalGaugeSubject.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "subject")}:";
			groupBoxDigitalGaugeLocation.Text = Utilities.GetInterfaceString("panel_settings", "digital_gauge", "location", "name");
			labelDigitalGaugeLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "location", "x")}:";
			labelDigitalGaugeLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "location", "y")}:";
			labelDigitalGaugeRadius.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "radius")}:";
			labelDigitalGaugeColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "color")}:";
			labelDigitalGaugeInitialAngle.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "initial_angle")}:";
			labelDigitalGaugeLastAngle.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "last_angle")}:";
			labelDigitalGaugeMinimum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "minimum")}:";
			labelDigitalGaugeMaximum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "maximum")}:";
			labelDigitalGaugeStep.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "step")}:";
			labelDigitalGaugeLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "digital_gauge", "layer")}:";

			tabPageLinearGauge.Text = Utilities.GetInterfaceString("panel_settings", "linear_gauge", "name");
			labelLinearGaugeSubject.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "subject")}:";
			groupBoxLinearGaugeLocation.Text = Utilities.GetInterfaceString("panel_settings", "linear_gauge", "location", "name");
			labelLinearGaugeLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "location", "x")}:";
			labelLinearGaugeLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "location", "y")}:";
			labelLinearGaugeDaytimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "daytime_image")}:";
			labelLinearGaugeNighttimeImage.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "nighttime_image")}:";
			labelLinearGaugeMinimum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "minimum")}:";
			labelLinearGaugeMaximum.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "maximum")}:";
			groupBoxLinearGaugeDirection.Text = Utilities.GetInterfaceString("panel_settings", "linear_gauge", "direction", "name");
			labelLinearGaugeDirectionX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "direction", "x")}:";
			labelLinearGaugeDirectionY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "direction", "y")}:";
			labelLinearGaugeTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "transparent_color")}:";
			labelLinearGaugeWidth.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "width")}:";
			labelLinearGaugeLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "linear_gauge", "layer")}:";

			tabPageTimetable.Text = Utilities.GetInterfaceString("panel_settings", "timetable", "name");
			labelTimetableHeight.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "height")}:";
			labelTimetableWidth.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "width")}:";
			labelTimetableTransparentColor.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "transparent_color")}:";
			labelTimetableLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "layer")}:";
			groupBoxTimetableLocation.Text = Utilities.GetInterfaceString("panel_settings", "timetable", "location", "name");
			labelTimetableLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "location", "x")}:";
			labelTimetableLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "timetable", "location", "y")}:";

			tabPageTouch.Text = Utilities.GetInterfaceString("panel_settings", "touch", "name");
			groupBoxTouchLocation.Text = Utilities.GetInterfaceString("panel_settings", "touch", "location", "name");
			labelTouchLocationX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "location", "x")}:";
			labelTouchLocationY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "location", "y")}:";
			groupBoxTouchSize.Text = Utilities.GetInterfaceString("panel_settings", "touch", "size", "name");
			labelTouchSizeX.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "size", "x")}:";
			labelTouchSizeY.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "size", "y")}:";
			labelTouchJumpScreen.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "jump_screen")}:";
			labelTouchSoundCommand.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "sound_command")}:";
			labelTouchLayer.Text = $@"{Utilities.GetInterfaceString("panel_settings", "touch", "layer")}:";

			tabPageSound.Text = Utilities.GetInterfaceString("sound_settings", "name");

			labelSoundFileName.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "filename");
			groupBoxSoundValue.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "name");
			groupBoxSoundEntry.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "name");
			groupBoxSoundKey.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "key", "name");
			checkBoxSoundRadius.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "radius");
			groupBoxSoundPosition.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "position", "name");
			checkBoxSoundDefinedPosition.Text = Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "position");
			labelSoundPositionX.Text = $@"{Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "position", "x")}:";
			labelSoundPositionY.Text = $@"{Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "position", "y")}:";
			labelSoundPositionZ.Text = $@"{Utilities.GetInterfaceString("sound_settings", "edit_entry", "value", "position", "z")}:";

			tabPageStatus.Text = $@"{Utilities.GetInterfaceString("status", "name")} ({Interface.LogMessages.Count})";
			toolStripMenuItemError.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Error || m.Type == MessageType.Critical)} {Utilities.GetInterfaceString("status", "menu", "error")}";
			toolStripMenuItemWarning.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Warning)} {Utilities.GetInterfaceString("status", "menu", "warning")}";
			toolStripMenuItemInfo.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Information)} {Utilities.GetInterfaceString("status", "menu", "info")}";
			toolStripMenuItemClear.Text = Utilities.GetInterfaceString("status", "menu", "clear");

			columnHeaderLevel.Text = Utilities.GetInterfaceString("status", "level");
			columnHeaderText.Text = Utilities.GetInterfaceString("status", "description");

			// misc open buttons etc
			// N.B. sub-forms are instanced
			buttonRearBogieSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonFrontBogieSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDelayPowerSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDelayBrakeSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDelayLocoBrakeSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDelayElectricBrake.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonThisTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonPilotLampTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonPilotLampSubjectSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonNeedleTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonNeedleColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonNeedleSubjectSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDigitalNumberTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDigitalNumberSubjectSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDigitalGaugeColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonDigitalGaugeSubjectSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonLinearGaugeTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonLinearGaugeSubjectSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonTimetableTransparentColorSet.Text = Utilities.GetInterfaceString("navigation", "set");
			buttonTouchSoundCommand.Text = Utilities.GetInterfaceString("navigation", "set");

			buttonObjectOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonCouplerObject.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonThisNighttimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonThisDaytimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonPilotLampNighttimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonPilotLampDaytimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonNeedleNighttimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonNeedleDaytimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonDigitalNumberNighttimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonDigitalNumberDaytimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonLinearGaugeNighttimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonLinearGaugeDaytimeImageOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonSoundFileNameOpen.Text = Utilities.GetInterfaceString("navigation", "open");
			buttonSoundAdd.Text = Utilities.GetInterfaceString("navigation", "add");
			buttonSoundRemove.Text = Utilities.GetInterfaceString("navigation", "remove");
			buttonPanelAdd.Text = Utilities.GetInterfaceString("navigation", "add");
			buttonPanelCopy.Text = Utilities.GetInterfaceString("navigation", "copy");
			buttonPanelRemove.Text = Utilities.GetInterfaceString("navigation", "remove");
		}
	}
}
