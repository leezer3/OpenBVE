using System;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Interface;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Systems;
using TrainEditor2.ViewModels.Panels;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private void ModifierKeysDownUp(KeyEventArgs e)
		{
			MotorViewModel.TrackViewModel track = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value.SelectedTrack.Value;

			if (glControlMotor.Focused)
			{
				if (track != null)
				{
					track.CurrentModifierKeys.Value = InputEventModel.ModifierKeys.None;

					if (e.Alt)
					{
						track.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Alt;
					}

					if (e.Control)
					{
						track.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Control;
					}

					if (e.Shift)
					{
						track.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Shift;
					}
				}
			}
		}

		private void SetSubject(Func<PanelViewModel, SubjectViewModel> selector)
		{
			EmbeddedCabViewModel embeddedCab = null;
			ControlledMotorCarViewModel controlledMotorCar = app.Train.Value.SelectedCar.Value as ControlledMotorCarViewModel;
			ControlledTrailerCarViewModel controlledTrailerCar = app.Train.Value.SelectedCar.Value as ControlledTrailerCarViewModel;

			if (controlledMotorCar != null)
			{
				embeddedCab = controlledMotorCar.Cab.Value as EmbeddedCabViewModel;
			}

			if (controlledTrailerCar != null)
			{
				embeddedCab = controlledTrailerCar.Cab.Value as EmbeddedCabViewModel;
			}

			if (embeddedCab == null)
			{
				return;
			}

			using (FormSubject form = new FormSubject(selector(embeddedCab.Panel.Value)))
			{
				form.ShowDialog(this);
			}
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

			groupBoxCab.Text = Utilities.GetInterfaceString("general_settings", "cab", "name");
			labelCabX.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "x")}:";
			labelCabY.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "y")}:";
			labelCabZ.Text = $@"{Utilities.GetInterfaceString("general_settings", "cab", "z")}:";

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

			tabPageCar1.Text = Utilities.GetInterfaceString("car_settings", "name");

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
			buttonFrontBogieSet.Text = Utilities.GetInterfaceString("navigation", "set");
			labelRearBogie.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "rear_bogie")}:";
			buttonRearBogieSet.Text = Utilities.GetInterfaceString("navigation", "set");
			labelLoadingSway.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "loading_sway")}:";
			labelReversed.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "reversed")}:";
			labelObject.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "object")}:";
			buttonObjectOpen.Text = Utilities.GetInterfaceString("navigation", "open");

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
			toolStripMenuItemCleanup.Text = Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cleanup");
			toolStripMenuItemDelete.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "delete")}(&D)";

			toolStripButtonUndo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "undo")} (Ctrl+Z)";
			toolStripButtonRedo.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "redo")} (Ctrl+Y)";
			toolStripButtonCleanup.Text = Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "cleanup");
			toolStripButtonDelete.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "menu", "edit", "delete")} (Del)";

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

			groupBoxArea.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "name");
			checkBoxMotorLoop.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "loop");
			checkBoxMotorConstant.Text = Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "constant");
			labelMotorAccel.Text = $@"{Utilities.GetInterfaceString("motor_sound_settings", "play_setting", "area", "acceleration")}:";

			tabPageCoupler.Text = Utilities.GetInterfaceString("coupler_settings", "name");

			groupBoxCouplerGeneral.Text = Utilities.GetInterfaceString("coupler_settings", "general", "name");
			labelCouplerMin.Text = $@"{Utilities.GetInterfaceString("coupler_settings", "general", "min")}:";
			labelCouplerMax.Text = $@"{Utilities.GetInterfaceString("coupler_settings", "general", "max")}:";

			tabPagePanel.Text = Utilities.GetInterfaceString("panel_settings", "name");

			tabPageThis.Text = Utilities.GetInterfaceString("panel_settings", "this", "name");

			tabPageSound.Text = Utilities.GetInterfaceString("sound_settings", "name");

			tabPageStatus.Text = $@"{Utilities.GetInterfaceString("status", "name")} ({Interface.LogMessages.Count})";
			toolStripMenuItemError.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Error || m.Type == MessageType.Critical)} {Utilities.GetInterfaceString("status", "menu", "error")}";
			toolStripMenuItemWarning.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Warning)} {Utilities.GetInterfaceString("status", "menu", "warning")}";
			toolStripMenuItemInfo.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Information)} {Utilities.GetInterfaceString("status", "menu", "info")}";
			toolStripMenuItemClear.Text = Utilities.GetInterfaceString("status", "menu", "clear");

			columnHeaderLevel.Text = Utilities.GetInterfaceString("status", "level");
			columnHeaderText.Text = Utilities.GetInterfaceString("status", "description");
		}
	}
}
