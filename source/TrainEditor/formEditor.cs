﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.SafetySystems;

namespace TrainEditor {
	public partial class formEditor : Form {
		public formEditor() {
			InitializeComponent();
		}

		// members
		private TrainDat.Train Train = new TrainDat.Train();
		private string FileName = null;

		private string CurrentLanguageCode = "en-US";

		
		// ----------------------------------------
		// form loading / closing                 |
		// ----------------------------------------

		// load
		private void FormEditorLoad(object sender, EventArgs e) {
			this.MinimumSize = this.Size;
			comboboxBrakeType.Items.Add("Electromagnetic straight air brake");
			comboboxBrakeType.Items.Add("Electro-pneumatic air brake without brake pipe");
			comboboxBrakeType.Items.Add("Air brake with partial release");
			comboboxBrakeControlSystem.Items.Add("None");
			comboboxBrakeControlSystem.Items.Add("Closing electromagnetic valve");
			comboboxBrakeControlSystem.Items.Add("Delay-including control");
			comboboxHandleType.Items.Add("Separated");
			comboboxHandleType.Items.Add("Combined");
			comboboxHandleType.Items.Add("Separated (Interlocked)");
			comboboxHandleType.Items.Add("Separated (Reverser Interlocked)");
			comboboxAts.Items.Add("None");
			comboboxAts.Items.Add("ATS-SN");
			comboboxAts.Items.Add("ATS-SN / ATS-P");
			comboboxAtc.Items.Add("None");
			comboboxAtc.Items.Add("Manual switching");
			comboboxAtc.Items.Add("Automatic switching");
			comboboxReAdhesionDevice.Items.Add("None");
			comboboxReAdhesionDevice.Items.Add("Type A (instant)");
			comboboxReAdhesionDevice.Items.Add("Type B (slow)");
			comboboxReAdhesionDevice.Items.Add("Type C (medium)");
			comboboxReAdhesionDevice.Items.Add("Type D (fast)");
			comboboxPassAlarm.Items.Add("None");
			comboboxPassAlarm.Items.Add("Single");
			comboboxPassAlarm.Items.Add("Looping");
			comboboxDoorOpenMode.Items.Add("Semi-automatic");
			comboboxDoorOpenMode.Items.Add("Automatic");
			comboboxDoorOpenMode.Items.Add("Manual");
			comboboxDoorCloseMode.Items.Add("Semi-automatic");
			comboboxDoorCloseMode.Items.Add("Automatic");
			comboboxDoorCloseMode.Items.Add("Manual");
			comboboxSoundIndex.Items.Add("None");
			comboBoxEBHandleBehaviour.Items.Add("No Action");
			comboBoxEBHandleBehaviour.Items.Add("Return Power to neutral");
			comboBoxEBHandleBehaviour.Items.Add("Return Reverser to neutral");
			comboBoxEBHandleBehaviour.Items.Add("Return Power & Reverser to neutral");
			comboBoxLocoBrakeSystemType.Items.Add("Not fitted");
			comboBoxLocoBrakeSystemType.Items.Add("Notched air brake");
			comboBoxLocoBrakeSystemType.Items.Add("Air brake with partial release");
			comboBoxLocoBrakeType.SelectedIndex = 0;
			CultureInfo culture = CultureInfo.InvariantCulture;
			for (int i = 0; i < 16; i++) {
				comboboxSoundIndex.Items.Add(i.ToString(culture));
			}
			comboboxSoundIndex.SelectedIndex = 0;
			LoadControlContent();
			// --- load options and controls ---
			Interface.LoadOptions();
			// language
			{
				Translations.CurrentLanguageCode = Interface.CurrentOptions.LanguageCode;
				string folder = Program.FileSystem.GetDataFolder("Languages");
				Translations.LoadLanguageFiles(folder);
				Translations.ListLanguages(comboboxLanguages);
				ApplyLanguage();
			}
		}

		// form closing
		private void FormEditorFormClosing(object sender, FormClosingEventArgs e) {
			switch (MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new []{"general","close_message"}), Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Yes:
					if (SaveControlContent()) {
						if (buttonSave.Enabled) {
							ButtonSaveClick(null, null);
						} else {
							ButtonSaveAsClick(null, null);
						}
					} else{
						e.Cancel = true;
					}
					break;
				case DialogResult.Cancel:
					e.Cancel = true;
					break;
			}
			if (!e.Cancel) {
				// finish
				Interface.CurrentOptions.LanguageCode = CurrentLanguageCode;
#if !DEBUG
				try {
#endif
					Interface.SaveOptions();
#if !DEBUG
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, "Save options", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
#endif
			}
		}

		// load control content
		/// <summary>Loads the content from the form's Train member into the textboxes etc.</summary>
		private void LoadControlContent() {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			// acceleration
			comboboxAccelerationNotch.Items.Clear();
			for (int i = 0; i < Train.Handle.PowerNotches; i++) {
				comboboxAccelerationNotch.Items.Add((i + 1).ToString(Culture));
			}
			if (Train.Handle.PowerNotches != 0) {
				comboboxAccelerationNotch.SelectedIndex = 0;
			}
			// performance
			textboxDeceleration.Text = Train.Performance.Deceleration.ToString(Culture);
			textboxCoefficientOfStaticFriction.Text = Train.Performance.CoefficientOfStaticFriction.ToString(Culture);
			textboxCoefficientOfRollingResistance.Text = Train.Performance.CoefficientOfRollingResistance.ToString(Culture);
			textboxAerodynamicDragCoefficient.Text = Train.Performance.AerodynamicDragCoefficient.ToString(Culture);

			// move
			textboxJerkPowerUp.Text = Train.Move.JerkPowerUp.ToString(Culture);
			textboxJerkPowerDown.Text = Train.Move.JerkPowerDown.ToString(Culture);
			textboxJerkBrakeUp.Text = Train.Move.JerkBrakeUp.ToString(Culture);
			textboxJerkBrakeDown.Text = Train.Move.JerkBrakeDown.ToString(Culture);
			textboxBrakeCylinderUp.Text = Train.Move.BrakeCylinderUp.ToString(Culture);
			textboxBrakeCylinderDown.Text = Train.Move.BrakeCylinderDown.ToString(Culture);
			// brake
			comboboxBrakeType.SelectedIndex = (int)Train.Brake.BrakeType;
			comboboxBrakeControlSystem.SelectedIndex = (int)Train.Brake.BrakeControlSystem;
			textboxBrakeControlSpeed.Text = Train.Brake.BrakeControlSpeed.ToString(Culture);
			// pressure
			textboxBrakeCylinderServiceMaximumPressure.Text = Train.Pressure.BrakeCylinderServiceMaximumPressure.ToString(Culture);
			textboxBrakeCylinderEmergencyMaximumPressure.Text = Train.Pressure.BrakeCylinderEmergencyMaximumPressure.ToString(Culture);
			textboxMainReservoirMinimumPressure.Text = Train.Pressure.MainReservoirMinimumPressure.ToString(Culture);
			textboxMainReservoirMaximumPressure.Text = Train.Pressure.MainReservoirMaximumPressure.ToString(Culture);
			textboxBrakePipeNormalPressure.Text = Train.Pressure.BrakePipeNormalPressure.ToString(Culture);
			// handle
			comboboxHandleType.SelectedIndex = (int)Train.Handle.HandleType;
			numericUpDownPowerNotches.Value = Train.Handle.PowerNotches;
			numericUpDownBrakeNotches.Value = Train.Handle.BrakeNotches;
			numericUpDownDriverPowerNotches.Value = Train.Handle.DriverPowerNotches;
			numericUpDownDriverBrakeNotches.Value = Train.Handle.DriverBrakeNotches;
			textboxPowerNotchReduceSteps.Text = Train.Handle.PowerNotchReduceSteps.ToString(Culture);
			// cab
			textboxX.Text = Train.Cab.Driver.X.ToString(Culture);
			textboxY.Text = Train.Cab.Driver.Y.ToString(Culture);
			textboxZ.Text = Train.Cab.Driver.Z.ToString(Culture);
			textboxDriverCar.Text = Train.Cab.DriverCar.ToString(Culture);
			// car
			textboxMotorCarMass.Text = Train.Car.MotorCarMass.ToString(Culture);
			textboxNumberOfMotorCars.Text = Train.Car.NumberOfMotorCars.ToString(Culture);
			textboxTrailerCarMass.Text = Train.Car.TrailerCarMass.ToString(Culture);
			textboxNumberOfTrailerCars.Text = Train.Car.NumberOfTrailerCars.ToString(Culture);
			textboxLengthOfACar.Text = Train.Car.LengthOfACar.ToString(Culture);
			checkboxFrontCarIsMotorCar.Checked = Train.Car.FrontCarIsAMotorCar;
			textboxWidthOfACar.Text = Train.Car.WidthOfACar.ToString(Culture);
			textboxHeightOfACar.Text = Train.Car.HeightOfACar.ToString(Culture);
			textboxCenterOfGravityHeight.Text = Train.Car.CenterOfGravityHeight.ToString(Culture);
			textboxExposedFrontalArea.Text = Train.Car.ExposedFrontalArea.ToString(Culture);
			textboxUnexposedFrontalArea.Text = Train.Car.UnexposedFrontalArea.ToString(Culture);
			// device
			comboboxAts.SelectedIndex = (int)Train.Device.Ats + 1;
			comboboxAtc.SelectedIndex = (int)Train.Device.Atc;
			checkboxEb.Checked = Train.Device.Eb;
			checkboxConstSpeed.Checked = Train.Device.ConstSpeed;
			checkboxHoldBrake.Checked = Train.Device.HoldBrake;
			comboboxReAdhesionDevice.SelectedIndex = (int)Train.Device.ReAdhesionDevice + 1;
			comboboxPassAlarm.SelectedIndex = (int)Train.Device.PassAlarm;
			comboboxDoorOpenMode.SelectedIndex = (int)Train.Device.DoorOpenMode;
			comboboxDoorCloseMode.SelectedIndex = (int)Train.Device.DoorCloseMode;
			textboxDoorWidth.Text = Train.Device.DoorWidth.ToString(Culture);
			textboxDoorMaxTolerance.Text = Train.Device.DoorMaxTolerance.ToString(Culture);
			comboBoxEBHandleBehaviour.SelectedIndex = (int)Train.Handle.HandleBehaviour;
			comboBoxLocoBrakeSystemType.SelectedIndex = (int) Train.Brake.LocoBrakeType;
		}
		
		// save control content
		/// <summary>Saves the content of all textboxes etc. into the form's Train member, stopping at the first control which contains invalid data and raising a message box at that point.</summary>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private bool SaveControlContent() {
			// performance
			if (!SaveControlContent(textboxDeceleration, "Deceleration", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Performance.Deceleration)) return false;
			if (!SaveControlContent(textboxCoefficientOfStaticFriction, "CoefficientOfStaticFriction", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Performance.CoefficientOfStaticFriction)) return false;
			if (!SaveControlContent(textboxCoefficientOfRollingResistance, "CoefficientOfRollingResistance", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Performance.CoefficientOfRollingResistance)) return false;
			if (!SaveControlContent(textboxAerodynamicDragCoefficient, "AerodynamicDragCoefficient", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Performance.AerodynamicDragCoefficient)) return false;
			// Jerk
			if (!SaveControlContent(textboxJerkPowerUp, "JerkPowerUp", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.JerkPowerUp)) return false;
			if (!SaveControlContent(textboxJerkPowerDown, "JerkPowerDown", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.JerkPowerDown)) return false;
			if (!SaveControlContent(textboxJerkBrakeUp, "JerkBrakeUp", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.JerkBrakeUp)) return false;
			if (!SaveControlContent(textboxJerkBrakeDown, "JerkBrakeDown", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.JerkBrakeDown)) return false;
			if (!SaveControlContent(textboxBrakeCylinderUp, "BrakeCylinderUp", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.BrakeCylinderUp)) return false;
			if (!SaveControlContent(textboxBrakeCylinderDown, "BrakeCylinderDown", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Move.BrakeCylinderDown)) return false;
			// brake
			Train.Brake.BrakeType = (BrakeSystemType)comboboxBrakeType.SelectedIndex;
			Train.Brake.BrakeControlSystem = (EletropneumaticBrakeType)comboboxBrakeControlSystem.SelectedIndex;
			if (!SaveControlContent(textboxBrakeControlSpeed, "BrakeControlSpeed", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Brake.BrakeControlSpeed)) return false;
			// pressure
			if (!SaveControlContent(textboxBrakeCylinderServiceMaximumPressure, "BrakeCylinderServiceMaximumPressure", tabpagePropertiesOne, NumberRange.Positive, out Train.Pressure.BrakeCylinderServiceMaximumPressure)) return false;
			if (!SaveControlContent(textboxBrakeCylinderEmergencyMaximumPressure, "BrakeCylinderEmergencyMaximumPressure", tabpagePropertiesOne, NumberRange.Positive, out Train.Pressure.BrakeCylinderEmergencyMaximumPressure)) return false;
			if (!SaveControlContent(textboxMainReservoirMinimumPressure, "MainReservoirMinimumPressure", tabpagePropertiesOne, NumberRange.Positive, out Train.Pressure.MainReservoirMinimumPressure)) return false;
			if (!SaveControlContent(textboxMainReservoirMaximumPressure, "MainReservoirMaximumPressure", tabpagePropertiesOne, NumberRange.Positive, out Train.Pressure.MainReservoirMaximumPressure)) return false;
			if (!SaveControlContent(textboxBrakePipeNormalPressure, "BrakePipeNormalPressure", tabpagePropertiesOne, NumberRange.Positive, out Train.Pressure.BrakePipeNormalPressure)) return false;
			// handle
			Train.Handle.HandleType = (HandleType)comboboxHandleType.SelectedIndex;
			Train.Handle.PowerNotches = (int)numericUpDownPowerNotches.Value;
			Train.Handle.BrakeNotches = (int)numericUpDownBrakeNotches.Value;
			if (Train.Handle.BrakeNotches == 0  & checkboxHoldBrake.Checked) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new []{"handle","brake_notches_error_message"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = tabpagePropertiesOne;
				numericUpDownBrakeNotches.Focus();
				return false;
			}
			Train.Handle.DriverPowerNotches = (int)numericUpDownDriverPowerNotches.Value;
			if (Train.Handle.DriverPowerNotches > Train.Handle.PowerNotches) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new []{"handle","driver_power_notches_error_message"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = tabpagePropertiesOne;
				numericUpDownDriverPowerNotches.Focus();
				return false;
			}
			Train.Handle.DriverBrakeNotches = (int)numericUpDownDriverBrakeNotches.Value;
			if (Train.Handle.DriverBrakeNotches > Train.Handle.BrakeNotches) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new []{ "handle","driver_brake_notches_error_message"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = tabpagePropertiesOne;
				numericUpDownDriverBrakeNotches.Focus();
				return false;
			}
			Train.Handle.HandleBehaviour = (EbHandleBehaviour) comboBoxEBHandleBehaviour.SelectedIndex;
			Train.Handle.LocoBrakeNotches = (int)numericUpDownLocoBrakeNotches.Value;
			if (!SaveControlContent(textboxPowerNotchReduceSteps, "PowerNotchReduceSteps", tabpagePropertiesOne, NumberRange.NonNegative, out Train.Handle.PowerNotchReduceSteps)) return false;
			// cab
			if (!SaveControlContent(textboxX, "X", tabpagePropertiesTwo, NumberRange.Any, out Train.Cab.Driver.X)) return false;
			if (!SaveControlContent(textboxY, "Y", tabpagePropertiesTwo, NumberRange.Any, out Train.Cab.Driver.Y)) return false;
			if (!SaveControlContent(textboxZ, "Z", tabpagePropertiesTwo, NumberRange.Any, out Train.Cab.Driver.Z)) return false;
			if (!SaveControlContent(textboxDriverCar, "DriverCar", tabpagePropertiesTwo, NumberRange.NonNegative, out Train.Cab.DriverCar)) return false;
			// car
			if (!SaveControlContent(textboxMotorCarMass, "MotorCarMass", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.MotorCarMass)) return false;
			if (!SaveControlContent(textboxNumberOfMotorCars, "NumberOfMotorCars", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.NumberOfMotorCars)) return false;
			if (!SaveControlContent(textboxTrailerCarMass, "TrailerCarMass", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.TrailerCarMass)) return false;
			if (!SaveControlContent(textboxNumberOfTrailerCars, "NumberOfTrailerCars", tabpagePropertiesTwo, NumberRange.NonNegative, out Train.Car.NumberOfTrailerCars)) return false;
			if (Train.Car.NumberOfTrailerCars == 0 & !checkboxFrontCarIsMotorCar.Checked) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new [] {"car","number_of_trailer_cars_error_message"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = tabpagePropertiesTwo;
				textboxNumberOfTrailerCars.SelectAll();
				textboxNumberOfTrailerCars.Focus();
				return false;
			}
			if (Train.Cab.DriverCar >= Train.Car.NumberOfMotorCars + Train.Car.NumberOfTrailerCars) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"cab","driver_car_error_message"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = tabpagePropertiesTwo;
				textboxDriverCar.SelectAll();
				textboxDriverCar.Focus();
				return false;
			}
			if (!SaveControlContent(textboxLengthOfACar, "LengthOfACar", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.LengthOfACar)) return false;
			Train.Car.FrontCarIsAMotorCar = checkboxFrontCarIsMotorCar.Checked;
			if (!SaveControlContent(textboxWidthOfACar, "WidthOfACar", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.WidthOfACar)) return false;
			if (!SaveControlContent(textboxHeightOfACar, "HeightOfACar", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.HeightOfACar)) return false;
			if (!SaveControlContent(textboxCenterOfGravityHeight, "CenterOfGravityHeight", tabpagePropertiesTwo, NumberRange.Any, out Train.Car.CenterOfGravityHeight)) return false;
			if (!SaveControlContent(textboxExposedFrontalArea, "ExposedFrontalArea", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.ExposedFrontalArea)) return false;
			if (!SaveControlContent(textboxUnexposedFrontalArea, "UnexposedFrontalArea", tabpagePropertiesTwo, NumberRange.Positive, out Train.Car.UnexposedFrontalArea)) return false;
			// device
			Train.Device.Ats = (AtsModes)(comboboxAts.SelectedIndex - 1);
			Train.Device.Atc = (AtcModes)comboboxAtc.SelectedIndex;
			Train.Device.Eb = checkboxEb.Checked;
			Train.Device.ConstSpeed = checkboxConstSpeed.Checked;
			Train.Device.HoldBrake = checkboxHoldBrake.Checked;
			Train.Device.ReAdhesionDevice = (ReadhesionDeviceType)(comboboxReAdhesionDevice.SelectedIndex - 1);
			Train.Device.PassAlarm = (PassAlarmType)comboboxPassAlarm.SelectedIndex;
			Train.Device.DoorOpenMode = (DoorMode)comboboxDoorOpenMode.SelectedIndex;
			Train.Device.DoorCloseMode = (DoorMode)comboboxDoorCloseMode.SelectedIndex;
			if (!SaveControlContent(textboxDoorWidth, "DoorWidth", tabpagePropertiesTwo, NumberRange.NonNegative, out Train.Device.DoorWidth)) return false;
			if (!SaveControlContent(textboxDoorMaxTolerance, "DoorMaxTolerance", tabpagePropertiesTwo, NumberRange.NonNegative, out Train.Device.DoorMaxTolerance)) return false;
			// finish
			return true;
		}
		/// <summary>Specifies constants defining the allowed number range for a conversion process.</summary>
		private enum NumberRange {
			Any = 0,
			Positive = 1,
			NonNegative = 2
		}
		/// <summary>Saves the content of a textbox into an output parameter and creates a message box if the textbox contains invalid data.</summary>
		/// <param name="Box">A textbox control.</param>
		/// <param name="Description">The description of the textbox.</param>
		/// <param name="Page">The tabpage the textbox resides in.</param>
		/// <param name="Range">The allowed number range.</param>
		/// <param name="Value">The output parameter that receives the numeric value of the textbox content.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private bool SaveControlContent(TextBox Box, string Description, TabPage Page, NumberRange Range, out int Value) {
			bool error;
			if (int.TryParse(Box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out Value)) {
				switch (Range) {
					case NumberRange.Positive:
						error = Value <= 0;
						break;
					case NumberRange.NonNegative:
						error = Value < 0;
						break;
					default:
						error = false;
						break;
				}
			} else {
				error = true;
			}
			if (error) {
				string prefix;
				switch (Range) {
					case NumberRange.Positive:
						prefix = "positive ";
						break;
					case NumberRange.NonNegative:
						prefix = "non-negative ";
						break;
					default:
						prefix = "";
						break;
				}
				MessageBox.Show(Description + " must be a " + prefix + "integer.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = Page;
				Box.SelectAll();
				Box.Focus();
				return false;
			} else {
				return true;
			}
		}
		/// <summary>Saves the content of a textbox into an output parameter and creates a message box if the textbox contains invalid data.</summary>
		/// <param name="Box">A textbox control.</param>
		/// <param name="Description">The description of the textbox.</param>
		/// <param name="Page">The tabpage the textbox resides in.</param>
		/// <param name="Range">The allowed number range.</param>
		/// <param name="Value">The output parameter that receives the numeric value of the textbox content.</param>
		/// <returns>A boolean indicating the success of the operation.</returns>
		private bool SaveControlContent(TextBox Box, string Description, TabPage Page, NumberRange Range, out double Value) {
			bool error;
			if (double.TryParse(Box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Value)) {
				switch (Range) {
					case NumberRange.Positive:
						error = Value <= 0.0;
						break;
					case NumberRange.NonNegative:
						error = Value < 0.0;
						break;
					default:
						error = false;
						break;
				}
			} else {
				error = true;
			}
			if (error) {
				string prefix;
				switch (Range) {
					case NumberRange.Positive:
						prefix = "positive ";
						break;
					case NumberRange.NonNegative:
						prefix = "non-negative ";
						break;
					default:
						prefix = "";
						break;
				}
				MessageBox.Show(Description + " must be a " + prefix + "floating-point number.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				tabcontrolTabs.SelectedTab = Page;
				Box.SelectAll();
				Box.Focus();
				return false;
			} else {
				return true;
			}
		}

		
		// ----------------------------------------
		// pane buttons                           |
		// ----------------------------------------
		
		// new
		private void ButtonNewClick(object sender, EventArgs e) {
			switch (MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new [] {"general","new_message"}), "New", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Yes:
					if (buttonSave.Enabled) {
						ButtonSaveClick(null, null);
					} else {
						ButtonSaveAsClick(null, null);
					}
					break;
				case DialogResult.Cancel:
					return;
			}
			Train = new TrainDat.Train();
			FileName = null;
			LoadControlContent();
			pictureboxAcceleration.Invalidate();
			pictureboxMotorP1.Invalidate();
			pictureboxMotorP2.Invalidate();
			pictureboxMotorB1.Invalidate();
			pictureboxMotorB2.Invalidate();
			buttonSave.Enabled = false;
		}
		
		// open
		private void ButtonOpenClick(object sender, EventArgs e) {
			switch (MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor, new [] {"general","open_message"}), "Open", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Yes:
					if (buttonSave.Enabled) {
						ButtonSaveClick(null, null);
					} else {
						ButtonSaveAsClick(null, null);
					}
					break;
				case DialogResult.Cancel:
					return;
			}
			using (OpenFileDialog Dialog = new OpenFileDialog()) {
				Dialog.Filter = "train.dat files|train.dat|All files|*";
				Dialog.CheckFileExists = true;
				if (Dialog.ShowDialog() == DialogResult.OK) {
					try {
						FileName = Dialog.FileName;
						Train = TrainDat.Load(FileName);
						this.Text = System.IO.Path.GetFileName(Path.GetDirectoryName(FileName)) + @" - " + Application.ProductName;
						buttonSave.Enabled = true;
					} catch (Exception ex) {
						MessageBox.Show(ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						FileName = null;
						Train = new TrainDat.Train();
						this.Text = Application.ProductName;
						buttonSave.Enabled = false;
					}
					LoadControlContent();
					pictureboxAcceleration.Invalidate();
					pictureboxMotorP1.Invalidate();
					pictureboxMotorP2.Invalidate();
					pictureboxMotorB1.Invalidate();
					pictureboxMotorB2.Invalidate();
				}
			}
		}
		
		// save
		private void ButtonSaveClick(object sender, EventArgs e) {
			if (SaveControlContent()) {
				if (FileName != null) {
					try {
						TrainDat.Save(FileName, Train);
						System.Media.SystemSounds.Asterisk.Play();
					} catch (Exception ex) {
						MessageBox.Show(ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				} else {
					System.Media.SystemSounds.Hand.Play();
				}
			}
		}
		
		// save as
		private void ButtonSaveAsClick(object sender, EventArgs e) {
			if (SaveControlContent()) {
				using (SaveFileDialog Dialog = new SaveFileDialog()) {
					Dialog.Filter = @"train.dat files|train.dat|All files|*";
					Dialog.OverwritePrompt = true;
					if (Dialog.ShowDialog() == DialogResult.OK) {
						try {
							FileName = Dialog.FileName;
							TrainDat.Save(FileName, Train);
							this.Text = System.IO.Path.GetFileName(Path.GetDirectoryName(FileName)) + @" - " + Application.ProductName;
						} catch (Exception ex) {
							MessageBox.Show(ex.Message, "Save as", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						}
						LoadControlContent();
					}
				}
			}
		}

		// close
		private void ButtonCloseClick(object sender, EventArgs e) {
			this.Close();
		}

		
		// ----------------------------------------
		// tabs                                   |
		// ----------------------------------------

		// selecting tab
		private void TabcontrolTabsSelecting(object sender, TabControlCancelEventArgs e) {
			if (e.TabPage == tabpageAcceleration) {
				SaveControlContent();
				int n = Train.Acceleration.Entries.Length;
				if (Train.Handle.PowerNotches > n) {
					Array.Resize(ref Train.Acceleration.Entries, Train.Handle.PowerNotches);
					for (int i = n; i < Train.Handle.PowerNotches; i++)
					{
						Train.Acceleration.Entries[i] = new BveAccelerationCurve(1.0, 1.0, 25, 25, 1.0);
					}
				}
				comboboxAccelerationNotch.Items.Clear();
				for (int i = 0; i < Train.Handle.PowerNotches; i++) {
					comboboxAccelerationNotch.Items.Add((i + 1).ToString(CultureInfo.InvariantCulture));
				}
				comboboxAccelerationNotch.SelectedIndex = 0;
			}
		}
		
		
		// ----------------------------------------
		// acceleration tab                       |
		// ----------------------------------------

		// members
		private float AccelerationMaximumX = 160.0f;
		private float AccelerationMaximumY = 4.0f;

		// paint
		private void PictureboxAccelerationPaint(object sender, PaintEventArgs e) {
			e.Graphics.Clear(Color.Black);
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.High;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			Pen grayPen = new Pen(Color.FromArgb(64, 64, 64));
			Brush grayBrush = Brushes.DimGray;
			Font font = new Font(this.Font.FontFamily, 7.0f);
			CultureInfo culture = CultureInfo.InvariantCulture;
			int width = pictureboxAcceleration.ClientRectangle.Width;
			int height = pictureboxAcceleration.ClientRectangle.Height;
			// horizontal grid
			for (float y = 0.0f; y <= AccelerationMaximumY; y += 1.0f) {
				float yf = (1.0f - y / AccelerationMaximumY) * (float)height;
				int yi = (int)Math.Round(yf);
				e.Graphics.DrawLine(grayPen, new Point(0, yi), new Point(width, yi));
				e.Graphics.DrawString(y.ToString("0", culture), font, grayBrush, new PointF(1.0f, yf));
			}
			// vertical grid
			for (float x = 0.0f; x <= AccelerationMaximumX; x += 10.0f) {
				float xf = x / AccelerationMaximumX * (float)width;
				int xi = (int)Math.Round(xf);
				e.Graphics.DrawLine(grayPen, new Point(xi, 0), new Point(xi, height));
				if (x != 0.0f) {
					e.Graphics.DrawString(x.ToString("0", culture), font, grayBrush, new PointF(xf, 1.0f));
				}
			}
			// curves
			DrawDecelerationCurve(e);
			int j = comboboxAccelerationNotch.SelectedIndex;
			for (int i = 0; i < Train.Handle.PowerNotches; i++) {
				if (i != j) {
					DrawAccelerationCurve(e, i, Train.Handle.PowerNotches, false);
				}
			}
			if (j >= 0 & j < Train.Acceleration.Entries.Length) {
				DrawAccelerationCurve(e, j, Train.Handle.PowerNotches, true);
			}
			// border
			e.Graphics.DrawRectangle(new Pen(SystemColors.ButtonShadow), new Rectangle(0, 0, width - 1, height - 1));
		}
		private void DrawAccelerationCurve(PaintEventArgs e, int Index, int Count, bool Selected) {
			// curve
			Point[] points = new Point[pictureboxAcceleration.ClientRectangle.Width];
			double factorX = AccelerationMaximumX / (double)pictureboxAcceleration.ClientRectangle.Width;
			double factorY = -(double)pictureboxAcceleration.ClientRectangle.Height / AccelerationMaximumY;
			double offsetY = (double)pictureboxAcceleration.ClientRectangle.Height;
			bool resistance = checkboxAccelerationSubtractDeceleration.Checked;
			for (int x = 0; x < pictureboxAcceleration.ClientRectangle.Width; x++) {
				double speed = (double)x * factorX;
				double a;
				if (resistance) {
					a = Math.Max(GetAcceleration(Index, speed) - GetDeceleration(speed), 0.0);
				} else {
					a = GetAcceleration(Index, speed);
				}
				int y = (int)Math.Round(offsetY + a * factorY);
				points[x] = new Point(x, y);
			}
			double hue;
			if (Count <= 1) {
				hue = 1.0;
			} else {
				hue = 0.5 * (double)Index / (double)(Count - 1);
			}
			Color color = GetColor(hue, Selected);
			e.Graphics.DrawLines(new Pen(color), points);
			// points
			{
				double v = Train.Acceleration.Entries[Index].StageOneSpeed;
				double a = Train.Acceleration.Entries[Index].StageOneAcceleration;
				if (resistance) a -= GetDeceleration(v);
				int x = (int)Math.Round(v / factorX);
				int y = (int)Math.Round(offsetY + a * factorY);
				e.Graphics.FillEllipse(new SolidBrush(color), new Rectangle(x - 2, y - 2, 5, 5));
				v = Train.Acceleration.Entries[Index].StageTwoSpeed;
				a = GetAcceleration(Index, v);
				if (resistance) a -= GetDeceleration(v);
				x = (int)Math.Round(v / factorX);
				y = (int)Math.Round(offsetY + a * factorY);
				e.Graphics.FillEllipse(new SolidBrush(color), new Rectangle(x - 2, y - 2, 5, 5));
			}
		}
		private void DrawDecelerationCurve(PaintEventArgs e) {
			if (!checkboxAccelerationSubtractDeceleration.Checked) {
				Point[] points = new Point[pictureboxAcceleration.ClientRectangle.Width];
				double factorX = AccelerationMaximumX / (double)pictureboxAcceleration.ClientRectangle.Width;
				double factory = -(double)pictureboxAcceleration.ClientRectangle.Height / AccelerationMaximumY;
				double offsety = (double)pictureboxAcceleration.ClientRectangle.Height;
				for (int x = 0; x < pictureboxAcceleration.ClientRectangle.Width; x++) {
					double speed = (double)x * factorX;
					double a = GetDeceleration(speed);
					int y = (int)Math.Round(offsety + a * factory);
					points[x] = new Point(x, y);
				}
				e.Graphics.DrawLines(Pens.DimGray, points);
			}
		}

		// mouse move
		private void PictureboxAccelerationMouseMove(object sender, MouseEventArgs e) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			double x = (double)e.X / (double)(pictureboxAcceleration.ClientRectangle.Width - 1);
			double y = (1.0 - (double)e.Y / (double)(pictureboxAcceleration.ClientRectangle.Height - 1));
			x = AccelerationMaximumX * x;
			y = AccelerationMaximumY * y;
			labelAccelerationInfo.Text =
				@"X: " + x.ToString("0.00", Culture) + @" km/h\n" +
				@"Y: " + y.ToString("0.00", Culture) + @" km/h/s";
		}

		// notch selected
		private void ComboboxAccelerationNotchSelectedIndexChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				this.Tag = new object();
				textboxA0.Text = Train.Acceleration.Entries[i].StageZeroAcceleration.ToString(Culture);
				textboxA1.Text = Train.Acceleration.Entries[i].StageOneAcceleration.ToString(Culture);
				textboxV1.Text = Train.Acceleration.Entries[i].StageOneSpeed.ToString(Culture);
				textboxV2.Text = Train.Acceleration.Entries[i].StageTwoSpeed.ToString(Culture);
				textboxE.Text = Train.Acceleration.Entries[i].StageTwoExponent.ToString(Culture);
				this.Tag = null;
			}
			pictureboxAcceleration.Invalidate();
		}

		// textboxes
		private void TextboxA0TextChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length & this.Tag == null) {
				double a0;
				if (double.TryParse(textboxA0.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out a0)) {
					Train.Acceleration.Entries[i].StageZeroAcceleration = Math.Max(a0, 0.0);
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		private void TextboxA1TextChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length & this.Tag == null) {
				double a1;
				if (double.TryParse(textboxA1.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out a1)) {
					Train.Acceleration.Entries[i].StageOneAcceleration = Math.Max(a1, 0.0);
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		private void TextboxV1TextChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length & this.Tag == null) {
				double v1;
				if (double.TryParse(textboxV1.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out v1)) {
					Train.Acceleration.Entries[i].StageOneSpeed = Math.Max(v1, 0.0);
					if (Train.Acceleration.Entries[i].StageTwoSpeed < Train.Acceleration.Entries[i].StageOneSpeed) {
						Train.Acceleration.Entries[i].StageTwoSpeed = Train.Acceleration.Entries[i].StageOneSpeed;
					}
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		private void TextboxV2TextChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length & this.Tag == null) {
				double v2;
				if (double.TryParse(textboxV2.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out v2)) {
					Train.Acceleration.Entries[i].StageTwoSpeed = Math.Max(v2, Train.Acceleration.Entries[i].StageOneSpeed);
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		private void TextboxETextChanged(object sender, EventArgs e) {
			int i = comboboxAccelerationNotch.SelectedIndex;
			if (i >= 0 & i < Train.Acceleration.Entries.Length & this.Tag == null) {
				double e2;
				if (double.TryParse(textboxE.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out e2)) {
					Train.Acceleration.Entries[i].StageTwoExponent = e2;
					pictureboxAcceleration.Invalidate();
				}
			}
		}

		// subtract deceleration
		private void CheckboxAccelerationSubtractDecelerationCheckedChanged(object sender, EventArgs e) {
			pictureboxAcceleration.Invalidate();
		}

		// maximum values
		private void TextboxAccelerationMaxXTextChanged(object sender, EventArgs e) {
			float x;
			if (float.TryParse(textboxAccelerationMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) {
				if (x > 0.0) {
					AccelerationMaximumX = x;
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		private void TextboxAccelerationMaxYTextChanged(object sender, EventArgs e) {
			float y;
			if (float.TryParse(textboxAccelerationMaxY.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out y)) {
				if (y > 0.0) {
					AccelerationMaximumY = y;
					pictureboxAcceleration.Invalidate();
				}
			}
		}
		

		// ----------------------------------------
		// motor sound tab                        |
		// ----------------------------------------
		
		// members
		private float MotorMinimumX = 0.0f;
		private float MotorMaximumX = 40.0f;
		private float MotorMaximumYPitch = 400.0f;
		private float MotorMaximumYVolume = 200.0f;
		private float MotorSelectionStartX = 0.0f;
		private float MotorSelectionStartYPitch = 0.0f;
		private float MotorSelectionStartYVolume = 0.0f;
		private PictureBox MotorSelectionBox = null;
		private float MotorHoverX = 0.0f;
		private float MotorHoverYPitch = 0.0f;
		private float MotorHoverYVolume = 0.0f;
		private class PictureBoxUpdateQueue {
			private readonly PictureBox[] Boxes = new PictureBox[4];
			internal void Add(PictureBox Box) {
				int i;
				for (i = 0; i < 4; i++) {
					if (this.Boxes[i] == Box) return;
					if (this.Boxes[i] == null) break;
				}
				this.Boxes[i] = Box;
			}
			internal PictureBox Next() {
				PictureBox b = this.Boxes[0];
				if (b == null) return null;
				for (int i = 0; i < 3; i++) {
					this.Boxes[i] = this.Boxes[i + 1];
				}
				this.Boxes[3] = null;
				return b;
			}
			internal PictureBox Seek() {
				return this.Boxes[0];
			}
		}
		private readonly PictureBoxUpdateQueue MotorUpdateQueue = new PictureBoxUpdateQueue();
		
		// motor p1
		private void PictureboxMotorP1Paint(object sender, PaintEventArgs e) {
			DrawMotorCurve(e, Train.MotorP1, "MOTOR_P1", pictureboxMotorP1);
		}
		private void PictureboxMotorP1MouseDown(object sender, MouseEventArgs e) {
			MotorMouseDown(e, pictureboxMotorP1);
		}
		private void PictureboxMotorP1MouseMove(object sender, MouseEventArgs e) {
			MotorMouseMove(e, pictureboxMotorP1);
		}
		private void PictureboxMotorP1MouseUp(object sender, MouseEventArgs e) {
			MotorMouseUp(e, Train.MotorP1, pictureboxMotorP1);
		}
		
		// motor p2
		private void PictureboxMotorP2Paint(object sender, PaintEventArgs e) {
			DrawMotorCurve(e, Train.MotorP2, "MOTOR_P2", pictureboxMotorP2);
		}
		private void PictureboxMotorP2MouseDown(object sender, MouseEventArgs e) {
			MotorMouseDown(e, pictureboxMotorP2);
		}
		private void PictureboxMotorP2MouseMove(object sender, MouseEventArgs e) {
			MotorMouseMove(e, pictureboxMotorP2);
		}
		private void PictureboxMotorP2MouseUp(object sender, MouseEventArgs e) {
			MotorMouseUp(e, Train.MotorP2, pictureboxMotorP2);
		}

		// motor b1
		private void PictureboxMotorB1Paint(object sender, PaintEventArgs e) {
			DrawMotorCurve(e, Train.MotorB1, "MOTOR_B1", pictureboxMotorB1);
		}
		private void PictureboxMotorB1MouseDown(object sender, MouseEventArgs e) {
			MotorMouseDown(e, pictureboxMotorB1);
		}
		private void PictureboxMotorB1MouseMove(object sender, MouseEventArgs e) {
			MotorMouseMove(e, pictureboxMotorB1);
		}
		private void PictureboxMotorB1MouseUp(object sender, MouseEventArgs e) {
			MotorMouseUp(e, Train.MotorB1, pictureboxMotorB1);
		}
		
		// motor b2
		private void PictureboxMotorB2Paint(object sender, PaintEventArgs e) {
			DrawMotorCurve(e, Train.MotorB2, "MOTOR_B2", pictureboxMotorB2);
		}
		private void PictureboxMotorB2MouseDown(object sender, MouseEventArgs e) {
			MotorMouseDown(e, pictureboxMotorB2);
		}
		private void PictureboxMotorB2MouseMove(object sender, MouseEventArgs e) {
			MotorMouseMove(e, pictureboxMotorB2);
		}
		private void PictureboxMotorB2MouseUp(object sender, MouseEventArgs e) {
			MotorMouseUp(e, Train.MotorB2, pictureboxMotorB2);
		}
		
		// tools
		private void RadiobuttonSoundIndexCheckedChanged(object sender, EventArgs e) {
			pictureboxMotorP1.Invalidate();
			pictureboxMotorP2.Invalidate();
			pictureboxMotorB1.Invalidate();
			pictureboxMotorB2.Invalidate();
		}
		private void RadiobuttonPitchCheckedChanged(object sender, EventArgs e) {
			pictureboxMotorP1.Invalidate();
			pictureboxMotorP2.Invalidate();
			pictureboxMotorB1.Invalidate();
			pictureboxMotorB2.Invalidate();
		}
		private void RadiobuttonVolumeCheckedChanged(object sender, EventArgs e) {
			pictureboxMotorP1.Invalidate();
			pictureboxMotorP2.Invalidate();
			pictureboxMotorB1.Invalidate();
			pictureboxMotorB2.Invalidate();
		}
		
		// textboxes
		private void TextboxMotorMinXTextChanged(object sender, EventArgs e) {
			float x;
			if (float.TryParse(textboxMotorMinX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) {
				if (x >= 0.0 & x < MotorMaximumX) {
					MotorMinimumX = x;
					pictureboxMotorP1.Invalidate();
					pictureboxMotorP2.Invalidate();
					pictureboxMotorB1.Invalidate();
					pictureboxMotorB2.Invalidate();
				}
			}
		}
		private void TextboxMotorMaxXTextChanged(object sender, EventArgs e) {
			float x;
			if (float.TryParse(textboxMotorMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) {
				if (x > MotorMinimumX) {
					MotorMaximumX = x;
					pictureboxMotorP1.Invalidate();
					pictureboxMotorP2.Invalidate();
					pictureboxMotorB1.Invalidate();
					pictureboxMotorB2.Invalidate();
				}
			}
		}
		private void TextboxMotorMaxYPitchTextChanged(object sender, EventArgs e) {
			float y;
			if (float.TryParse(textboxMotorMaxYPitch.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out y)) {
				if (y > 0.0) {
					MotorMaximumYPitch = y;
					pictureboxMotorP1.Invalidate();
					pictureboxMotorP2.Invalidate();
					pictureboxMotorB1.Invalidate();
					pictureboxMotorB2.Invalidate();
				}
			}
		}
		private void TextboxMotorMaxYVolumeTextChanged(object sender, EventArgs e) {
			float y;
			if (float.TryParse(textboxMotorMaxYVolume.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out y)) {
				if (y > 0.0) {
					MotorMaximumYVolume = y;
					pictureboxMotorP1.Invalidate();
					pictureboxMotorP2.Invalidate();
					pictureboxMotorB1.Invalidate();
					pictureboxMotorB2.Invalidate();
				}
			}
		}

		// buttons
		private void ButtonMotorLeftClick(object sender, EventArgs e) {
			float xmin, xmax;
			if (float.TryParse(textboxMotorMinX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmin)) {
				if (xmin < 0.0) {
					xmin = 0.0f;
				}
			} else {
				xmin = 0.0f;
			}
			if (float.TryParse(textboxMotorMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmax)) {
				if (xmax <= xmin) {
					xmax = xmin + 40.0f;
				}
			} else {
				xmax = xmin + 40.0f;
			}
			float range = xmax - xmin;
			float delta = 0.1f * range;
			xmin -= delta;
			if (xmin < 0.0f) {
				xmin = 0.0f;
			}
			xmax = xmin + range;
			textboxMotorMinX.Text = xmin.ToString(CultureInfo.InvariantCulture);
			textboxMotorMaxX.Text = xmax.ToString(CultureInfo.InvariantCulture);
		}
		private void ButtonMotorRightClick(object sender, EventArgs e) {
			float xmin, xmax;
			if (float.TryParse(textboxMotorMinX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmin)) {
				if (xmin < 0.0) {
					xmin = 0.0f;
				}
			} else {
				xmin = 0.0f;
			}
			if (float.TryParse(textboxMotorMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmax)) {
				if (xmax <= xmin) {
					xmax = xmin + 40.0f;
				}
			} else {
				xmax = xmin + 40.0f;
			}
			float range = xmax - xmin;
			float delta = 0.1f * range;
			xmin += delta;
			xmax += delta;
			textboxMotorMinX.Text = xmin.ToString(CultureInfo.InvariantCulture);
			textboxMotorMaxX.Text = xmax.ToString(CultureInfo.InvariantCulture);
		}
		private void ButtonMotorInClick(object sender, EventArgs e) {
			float xmin, xmax;
			if (float.TryParse(textboxMotorMinX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmin)) {
				if (xmin < 0.0) {
					xmin = 0.0f;
				}
			} else {
				xmin = 0.0f;
			}
			if (float.TryParse(textboxMotorMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmax)) {
				if (xmax <= xmin) {
					xmax = xmin + 40.0f;
				}
			} else {
				xmax = xmin + 40.0f;
			}
			float range = xmax - xmin;
			float center = 0.5f * (xmin + xmax);
			float radius = 0.5f * range;
			xmin = center - 0.9f * radius;
			xmax = center + 0.9f * radius;
			textboxMotorMinX.Text = xmin.ToString(CultureInfo.InvariantCulture);
			textboxMotorMaxX.Text = xmax.ToString(CultureInfo.InvariantCulture);
		}
		private void ButtonMotorOutClick(object sender, EventArgs e) {
			float xmin, xmax;
			if (float.TryParse(textboxMotorMinX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmin)) {
				if (xmin < 0.0) {
					xmin = 0.0f;
				}
			} else {
				xmin = 0.0f;
			}
			if (float.TryParse(textboxMotorMaxX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out xmax)) {
				if (xmax <= xmin) {
					xmax = xmin + 40.0f;
				}
			} else {
				xmax = xmin + 40.0f;
			}
			float range = xmax - xmin;
			float center = 0.5f * (xmin + xmax);
			float radius = 0.5f * range;
			xmin = center - 1.11111111111111f * radius;
			if (xmin < 0.0) {
				xmin = 0.0f;
				xmax = 1.11111111111111f * range;
			} else {
				xmax = center + 1.11111111111111f * radius;
			}
			textboxMotorMinX.Text = xmin.ToString(CultureInfo.InvariantCulture);
			textboxMotorMaxX.Text = xmax.ToString(CultureInfo.InvariantCulture);
		}

		// draw motor curve
		private void DrawMotorCurve(PaintEventArgs e, TrainDat.Motor Motor, string Description, PictureBox Box) {
			// prepare
			int width = Box.ClientRectangle.Width;
			int height = Box.ClientRectangle.Height;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.High;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			e.Graphics.Clear(Color.Black);
			Font font = new Font(this.Font.FontFamily, 7.0f);
			Pen grayPen = new Pen(Color.FromArgb(64, 64, 64));
			Brush grayBrush = Brushes.DimGray;
			float factorX = (float)width / (MotorMaximumX - MotorMinimumX);
			float factorYpitch = -(float)height / MotorMaximumYPitch;
			float factorYvolume = -(float)height / MotorMaximumYVolume;
			float offsetY = (float)height;
			bool selectedPitch = radiobuttonPitch.Checked;
			bool selectedVolume = radiobuttonVolume.Checked;
			const double huefactor = 0.785398163397448;
			CultureInfo culture = CultureInfo.InvariantCulture;
			// vertical grid
			for (float x = 0.0f; x < MotorMaximumX; x += 10.0f) {
				float a = (x - MotorMinimumX) * factorX;
				e.Graphics.DrawLine(grayPen, new PointF(a, 0.0f), new PointF(a, (float)height));
				e.Graphics.DrawString(x.ToString("0", culture), font, grayBrush, new PointF(a, 1.0f));
			}
			// horizontal base lines
			{
				float yp = offsetY + (float)100.0f * factorYpitch;
				float yv = offsetY + (float)128.0f * factorYvolume;
				e.Graphics.DrawLine(grayPen, new PointF(0.0f, yp), new PointF((float)width, yp));
				e.Graphics.DrawLine(grayPen, new PointF(0.0f, yv), new PointF((float)width, yv));
				e.Graphics.DrawString("p=100", font, grayBrush, new PointF(1.0f, yp));
				e.Graphics.DrawString("v=128", font, grayBrush, new PointF(1.0f, yv));
			}
			// hover
			{
				float x = (MotorHoverX - MotorMinimumX) * factorX;
				Pen p = new Pen(Color.DimGray);
				p.DashStyle = DashStyle.Dash;
				e.Graphics.DrawLine(p, new PointF(x, 0.0f), new PointF(x, (float)height));
				p.Dispose();
			}
			// pen
			if (Box == MotorSelectionBox) {
				if (radiobuttonSoundIndex.Checked) {
					float ax = (MotorSelectionStartX - MotorMinimumX) * factorX;
					float bx = (MotorHoverX - MotorMinimumX) * factorX;
					float x = Math.Min(ax, bx);
					float w = Math.Abs(bx - ax);
					if (w > 0.0f) {
						int s;
						if (!int.TryParse(comboboxSoundIndex.Text, NumberStyles.Integer, culture, out s)) {
							s = -1;
						}
						Color c;
						if (s >= 0) {
							double hue = huefactor * (double)s;
							hue -= Math.Floor(hue);
							c = GetColor(hue, true);
						} else {
							c = Color.DimGray;
						}
						e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(32, c)), new RectangleF(x, 0.0f, w, (float)height));
					}
				}
			}
			// curves
			PointF[] pointsPitch = new PointF[Motor.Entries.Length + 1];
			PointF[] pointsVolume = new PointF[Motor.Entries.Length + 1];
			int length = 0;
			int soundIndex = -1;
			for (int i = 0; i < Motor.Entries.Length; i++) {
				float v = 0.2f * (float)i;
				float x = (v - MotorMinimumX) * factorX;
				float yPitch = offsetY + (float)Motor.Entries[i].Pitch * factorYpitch;
				float yVolume = offsetY + (float)Motor.Entries[i].Gain * factorYvolume;
				if (soundIndex != Motor.Entries[i].SoundIndex & length != 0) {
					pointsPitch[length] = new PointF(x, pointsPitch[length - 1].Y);
					pointsVolume[length] = new PointF(x, pointsVolume[length - 1].Y);
					length++;
					Array.Resize(ref pointsPitch, length);
					Array.Resize(ref pointsVolume, length);
					Color colorPitch, colorVolume;
					if (soundIndex >= 0) {
						double hue = huefactor * (double)soundIndex;
						hue -= Math.Floor(hue);
						colorPitch = GetColor(hue, selectedPitch);
						colorVolume = GetColor(hue, selectedVolume);
					} else {
						colorPitch = selectedPitch ? Color.DimGray : Color.FromArgb(64, 64, 64);
						colorVolume = selectedVolume ? Color.DimGray : Color.FromArgb(64, 64, 64);
					}
					e.Graphics.DrawLines(new Pen(colorPitch), pointsPitch);
					e.Graphics.DrawLines(new Pen(colorVolume), pointsVolume);
					pointsPitch = new PointF[Motor.Entries.Length + 1];
					pointsVolume = new PointF[Motor.Entries.Length + 1];
					soundIndex = Motor.Entries[i].SoundIndex;
					length = 0;
				}
				pointsPitch[length] = new PointF(x, yPitch);
				pointsVolume[length] = new PointF(x, yVolume);
				length++;
			}
			if (length != 0) {
				float v = 0.2f * (float)Motor.Entries.Length;
				float x = (v - MotorMinimumX) * factorX;
				pointsPitch[length] = new PointF(x, pointsPitch[length - 1].Y);
				pointsVolume[length] = new PointF(x, pointsVolume[length - 1].Y);
				length++;
				Array.Resize(ref pointsPitch, length);
				Array.Resize(ref pointsVolume, length);
				Color colorPitch, colorVolume;
				if (soundIndex >= 0) {
					double hue = huefactor * (double)soundIndex;
					hue -= Math.Floor(hue);
					colorPitch = GetColor(hue, selectedPitch);
					colorVolume = GetColor(hue, selectedVolume);
				} else {
					colorPitch = selectedPitch ? Color.DimGray : Color.FromArgb(64, 64, 64);
					colorVolume = selectedVolume ? Color.DimGray : Color.FromArgb(64, 64, 64);
				}
				e.Graphics.DrawLines(new Pen(colorPitch), pointsPitch);
				e.Graphics.DrawLines(new Pen(colorVolume), pointsVolume);
			}
			soundIndex = -1;
			for (int i = 0; i < Motor.Entries.Length; i++) {
				if (Motor.Entries[i].SoundIndex != soundIndex) {
					float v = 0.2f * (float)i;
					float x = (v - MotorMinimumX) * factorX;
					float yPitch = offsetY + (float)Motor.Entries[i].Pitch * factorYpitch;
					float yVolume = offsetY + (float)Motor.Entries[i].Gain * factorYvolume;
					Color colorPitch, colorVolume;
					if (Motor.Entries[i].SoundIndex >= 0) {
						double hue = huefactor * (double)Motor.Entries[i].SoundIndex;
						hue -= Math.Floor(hue);
						colorPitch = GetColor(hue, selectedPitch);
						colorVolume = GetColor(hue, selectedVolume);
					} else {
						colorPitch = selectedPitch ? Color.DimGray : Color.FromArgb(64, 64, 64);
						colorVolume = selectedVolume ? Color.DimGray : Color.FromArgb(64, 64, 64);
					}
					e.Graphics.FillEllipse(new SolidBrush(colorPitch), new RectangleF(x - 2.0f, yPitch - 2.0f, 5.0f, 5.0f));
					e.Graphics.FillEllipse(new SolidBrush(colorVolume), new RectangleF(x - 2.0f, yVolume - 2.0f, 5.0f, 5.0f));
					e.Graphics.DrawString(Motor.Entries[i].SoundIndex.ToString(CultureInfo.InvariantCulture) + "p", font, new SolidBrush(colorPitch), new PointF(x, yPitch - 16.0f));
					e.Graphics.DrawString(Motor.Entries[i].SoundIndex.ToString(CultureInfo.InvariantCulture) + "v", font, new SolidBrush(colorVolume), new PointF(x, yVolume - 16.0f));
					soundIndex = Motor.Entries[i].SoundIndex;
				}
			}
			// pen
			if (Box == MotorSelectionBox) {
				if (radiobuttonPitch.Checked) {
					float av = MotorSelectionStartX;
					float bv = MotorHoverX;
					float ax = (av - MotorMinimumX) * factorX;
					float bx = (bv - MotorMinimumX) * factorX;
					float ay = offsetY + MotorSelectionStartYPitch * factorYpitch;
					float by = offsetY + MotorHoverYPitch * factorYpitch;
					if (av > bv) {
						float t = ax;
						ax = bx;
						bx = t;
						t = ay;
						ay = by;
						by = t;
					}
					e.Graphics.DrawLine(Pens.White, new PointF(ax, ay), new PointF(bx, by));
				} else if (radiobuttonVolume.Checked) {
					float av = MotorSelectionStartX;
					float bv = MotorHoverX;
					float ax = (av - MotorMinimumX) * factorX;
					float bx = (bv - MotorMinimumX) * factorX;
					float ay = offsetY + MotorSelectionStartYVolume * factorYvolume;
					float by = offsetY + MotorHoverYVolume * factorYvolume;
					if (av > bv) {
						float t = ax;
						ax = bx;
						bx = t;
						t = ay;
						ay = by;
						by = t;
					}
					e.Graphics.DrawLine(Pens.White, new PointF(ax, ay), new PointF(bx, by));
				}
			}
			// text
			e.Graphics.DrawString(Description, this.Font, Brushes.White, new PointF(2.0f, 12.0f));
			// border
			e.Graphics.DrawRectangle(new Pen(SystemColors.ButtonShadow), new Rectangle(0, 0, width - 1, height - 1));
			// queue
			PictureBox b = MotorUpdateQueue.Next();
			b?.Invalidate();
		}
		
		// mouse
		private void MotorMouseDown(MouseEventArgs e, PictureBox Box) {
			float width = (float)Box.ClientRectangle.Width;
			float height = (float)Box.ClientRectangle.Height;
			MotorSelectionStartX = MotorMinimumX + (MotorMaximumX - MotorMinimumX) * (float)e.X / (float)width;
			MotorSelectionStartX = 0.2f * (float)Math.Round(5.0 * (double)MotorSelectionStartX);
			MotorSelectionStartYPitch = (1.0f - (float)e.Y / height) * MotorMaximumYPitch;
			MotorSelectionStartYVolume = (1.0f - (float)e.Y / height) * MotorMaximumYVolume;
			MotorSelectionBox = Box;
		}
		private void MotorMouseMove(MouseEventArgs e, PictureBox Box) {
			float width = (float)Box.ClientRectangle.Width;
			float height = (float)Box.ClientRectangle.Height;
			float oldhoverx = MotorHoverX;
			MotorHoverX = MotorMinimumX + (MotorMaximumX - MotorMinimumX) * (float)e.X / (float)width;
			MotorHoverX = 0.2f * (float)Math.Round(5.0 * (double)MotorHoverX);
			MotorHoverYPitch = (1.0f - (float)e.Y / height) * MotorMaximumYPitch;
			MotorHoverYVolume = (1.0f - (float)e.Y / height) * MotorMaximumYVolume;
			if (oldhoverx != MotorHoverX) {
				bool start = MotorUpdateQueue.Seek() == null;
				MotorUpdateQueue.Add(Box);
				MotorUpdateQueue.Add(pictureboxMotorP1);
				MotorUpdateQueue.Add(pictureboxMotorP2);
				MotorUpdateQueue.Add(pictureboxMotorB1);
				MotorUpdateQueue.Add(pictureboxMotorB2);
				if (start) {
					MotorUpdateQueue.Next().Invalidate();
				}
			} else if (MotorSelectionBox == Box) {
				bool start = MotorUpdateQueue.Seek() == null;
				MotorUpdateQueue.Add(Box);
				if (start) {
					MotorUpdateQueue.Next().Invalidate();
				}
			}
			CultureInfo culture = CultureInfo.InvariantCulture;
			labelMotorInfo.Text =
				@"X = #" + ((int)Math.Floor(5.0 * (double)MotorHoverX)).ToString(culture) + @" (" + MotorHoverX.ToString("0.00", culture) + @" km/h)\n\n" +
				Translations.GetInterfaceString(HostApplication.TrainEditor, new []{"motor","y_pitch"}) + @" = " + MotorHoverYPitch.ToString("0.00", culture) + @"\n" +
				Translations.GetInterfaceString(HostApplication.TrainEditor, new []{"motor","y_volume"}) + @" = " + MotorHoverYVolume.ToString("0.00", culture) + @" (" + (0.78125 * MotorHoverYVolume).ToString("0", culture) + @"%)";
		}
		private void MotorMouseUp(MouseEventArgs e, TrainDat.Motor Motor, PictureBox Box) {
			if (MotorSelectionBox != null) {
				float width = (float)Box.ClientRectangle.Width;
				float height = (float)Box.ClientRectangle.Height;
				float x = MotorMinimumX + (MotorMaximumX - MotorMinimumX) * (float)e.X / (float)width;
				x = 0.2f * (float)Math.Round(5.0 * (double)x);
				int ia = (int)Math.Round((double)(5.0 * MotorSelectionStartX));
				int ib = (int)Math.Round((double)(5.0 * x));
				bool swap;
				if (ia > ib) {
					int t = ia;
					ia = ib;
					ib = t;
					swap = true;
				} else {
					swap = false;
				}
				if (ia < 0) {
					ia = 0;
				}
				if (ib >= Motor.Entries.Length) {
					int n = Motor.Entries.Length;
					Array.Resize(ref Motor.Entries, ib + 1);
					for (int i = n; i < Motor.Entries.Length; i++) {
						Motor.Entries[i].SoundIndex = -1;
						Motor.Entries[i].Pitch = 100.0f;
						Motor.Entries[i].Gain = 128.0f;
					}
				}
				if (ia <= ib) {
					if (radiobuttonSoundIndex.Checked) {
						if (ia < ib){
							ib--;
						}
						int j;
						if (!int.TryParse(comboboxSoundIndex.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out j)) {
							j = -1;
						} else if (j < -1) {
							j = -1;
						}
						for (int i = ia ; i <= ib; i++) {
							Motor.Entries[i].SoundIndex = j;
						}
					} else if (radiobuttonPitch.Checked) {
						float yPitch = (1.0f - (float)e.Y / height) * MotorMaximumYPitch;
						float y = swap ? yPitch : MotorSelectionStartYPitch;
						float dy = 0.2f * (yPitch - MotorSelectionStartYPitch) / (x - MotorSelectionStartX);
						for (int i = ia ; i <= ib; i++) {
							Motor.Entries[i].Pitch = (float)Math.Max(y, 1.0);
							y += dy;
						}
					} else if (radiobuttonVolume.Checked) {
						float yVolume = (1.0f - (float)e.Y / height) * MotorMaximumYVolume;
						float y = swap ? yVolume : MotorSelectionStartYVolume;
						float dy = 0.2f * (yVolume - MotorSelectionStartYVolume) / (x - MotorSelectionStartX);
						for (int i = ia ; i <= ib; i++) {
							Motor.Entries[i].Gain = (float)Math.Max(y, 0.0);
							y += dy;
						}
					}
				}
				bool start = MotorUpdateQueue.Seek() == null;
				MotorUpdateQueue.Add(Box);
				MotorUpdateQueue.Add(pictureboxMotorP1);
				MotorUpdateQueue.Add(pictureboxMotorP2);
				MotorUpdateQueue.Add(pictureboxMotorB1);
				MotorUpdateQueue.Add(pictureboxMotorB2);
				if (start) {
					MotorUpdateQueue.Next().Invalidate();
				}
				MotorSelectionBox = null;
			}
		}

		
		// ----------------------------------------
		// helper functions                       |
		// ----------------------------------------

		// get color
		private Color GetColor(double Hue, bool Selected) {
			double r, g, b;
			if (Hue < 0.0) {
				r = 0.0; g = 0.0; b = 0.0;
			} else if (Hue <= 0.166666666666667) {
				double x = 6.0 * Hue;
				r = 1.0; g = x; b = 0.0;
			} else if (Hue <= 0.333333333333333) {
				double x = 6.0 * Hue - 1.0;
				r = 1.0 - x; g = 1.0; b = 0.0;
			} else if (Hue <= 0.5) {
				double x = 6.0 * Hue - 2.0;
				r = 0.0; g = 1.0; b = x;
			} else if (Hue <= 0.666666666666667) {
				double x = 6.0 * Hue - 3.0;
				r = 0.0; g = 1.0 - x; b = 1.0;
			} else if (Hue <= 0.833333333333333) {
				double x = 6.0 * Hue - 4.0;
				r = x; g = 0.0; b = 1.0;
			} else if (Hue <= 1.0) {
				double x = 6.0 * Hue - 5.0;
				r = 1.0; g = 0.0; b = 1.0 - x;
			} else {
				r = 1.0; g = 1.0; b = 1.0;
			}
			if (r < 0.0) r = 0.0; else if (r > 1.0) r = 1.0;
			if (g < 0.0) g = 0.0; else if (g > 1.0) g = 1.0;
			if (b < 0.0) b = 0.0; else if (b > 1.0) b = 1.0;
			if (!Selected) {
				r *= 0.6;
				g *= 0.6;
				b *= 0.6;
			}
			return Color.FromArgb((int)Math.Round(255.0 * r), (int)Math.Round(255.0 * g), (int)Math.Round(255.0 * b));
		}

		// get acceleration
		/// <summary>Returns the acceleration produced by the train as a whole for the given parameters.</summary>
		/// <param name="Index">The acceleration curve entry index.</param>
		/// <param name="Speed">The speed in km/h.</param>
		/// <returns>The acceleration in km/h/s.</returns>
		private double GetAcceleration(int Index, double Speed) {
			Speed *= 0.277777777777778;
			double a0 = 0.277777777777778 * Train.Acceleration.Entries[Index].StageZeroAcceleration;
			double a1 = 0.277777777777778 * Train.Acceleration.Entries[Index].StageOneAcceleration;
			double v1 = 0.277777777777778 * Train.Acceleration.Entries[Index].StageOneSpeed;
			double v2 = 0.277777777777778 * Train.Acceleration.Entries[Index].StageTwoSpeed;
			double e = Train.Acceleration.Entries[Index].StageTwoExponent;
			double a;
			if (Speed == 0.0) {
				a = a0;
			} else if (Speed < v1) {
				a = a0 + (a1 - a0) * Speed / v1;
			} else if (Speed < v2) {
				a = v1 * a1 / Speed;
			} else {
				a = v1 * a1 * Math.Pow(v2, e - 1.0) * Math.Pow(Speed, -e);
			}
			return 3.6 * a;
		}

		// get deceleration
		/// <summary>Returns the deceleration due to air resistance of the whole train. Environmental constants are assumed.</summary>
		/// <param name="Speed">The speed in km/h.</param>
		/// <returns>The deceleration in km/h/s.</returns>
		private double GetDeceleration(double Speed) {
			double Acceleration = 0.0;
			double Mass = 0.0;
			for (int i = 0; i < Train.Car.NumberOfMotorCars; i++) {
				double a = i == 0 & Train.Car.FrontCarIsAMotorCar ? Train.Car.ExposedFrontalArea: Train.Car.UnexposedFrontalArea;
				double r = GetDeceleration(Speed, Train.Car.MotorCarMass, a);
				Acceleration += r * Train.Car.MotorCarMass;
				Mass += Train.Car.MotorCarMass;
			}
			for (int i = 0; i < Train.Car.NumberOfTrailerCars; i++) {
				double a = i == 0 & !Train.Car.FrontCarIsAMotorCar ? Train.Car.ExposedFrontalArea: Train.Car.UnexposedFrontalArea;
				double r = GetDeceleration(Speed, Train.Car.TrailerCarMass, a);
				Acceleration += r * Train.Car.TrailerCarMass;
				Mass += Train.Car.TrailerCarMass;
			}
			return Acceleration / Mass;
		}
		/// <summary>Returns the deceleration due to air and rolling resistance for a single car with the given parameters. Environmental constants are assumed.</summary>
		/// <param name="Speed">The speed in km/h.</param>
		/// <param name="Mass">The mass in 1000 kg.</param>
		/// <param name="Area">The frontal area in m².</param>
		/// <returns>The deceleration in km/h/s.</returns>
		private double GetDeceleration(double Speed, double Mass, double Area) {
			const double AccelerationDueToGravity = 9.80665;
			const double AirDensity = 1.22497705587732;
			Speed *= 0.277777777777778;
			Mass *= 1000.0;
			double f = Area * Train.Performance.AerodynamicDragCoefficient * AirDensity / (2.0 * Mass);
			double a = AccelerationDueToGravity * Train.Performance.CoefficientOfRollingResistance + f * Speed * Speed;
			return 3.6 * a;
		}

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			if (Translations.SelectedLanguage(ref CurrentLanguageCode, comboboxLanguages)) {
				ApplyLanguage();
			}
		}

		/// <summary>This function is called to change the display language of the program</summary>
		private void ApplyLanguage() {
			Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			buttonNew.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","new"});
			buttonOpen.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","open"});
			buttonSave.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","save"});
			buttonSaveAs.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","save_as"});
			buttonClose.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","close"});

			tabpagePropertiesOne.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","properties_one"});
			tabpagePropertiesTwo.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"general","properties_two"});
			tabpageAcceleration.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","acceleration"});
			tabpageMotor.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","sound"});
			tabPageExtended.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","features"});

			groupboxPerformance.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"performance","performance"});
			labelDeceleration.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"performance","deceleration"});
			labelCoefficientOfStaticFriction.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"performance","static_friction"});
			labelCoefficientOfRollingResistance.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"performance","rolling_resistance"});
			labelAerodynamicDragCoefficient.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"performance","aerodynamic_drag"});

			groupboxDelay.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","delay"});
			labelDelayPowerUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","power_up"}) + @":";
			labelDelayPowerDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","power_down"}) + @":";
			labelDelayBrakeUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","brake_up"}) + @":";
			labelDelayBrakeDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","brake_down"}) + @":";
			buttonSetDelayPowerUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","set"});
			buttonSetDelayPowerDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","set"});
			buttonSetDelayBrakeUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","set"});
			buttonSetDelayBrakeDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","set"});

			groupboxMove.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","move"});
			labelJerkPowerUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","jerk_power_up"});
			labelJerkPowerDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","jerk_power_down"});
			labelJerkBrakeUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","jerk_brake_up"});
			labelJerkBrakeDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","jerk_brake_down"});
			labelBrakeCylinderUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","brake_cylinder_up"});
			labelBrakeCylinderDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"move","brake_cylinder_down"});

			groupboxBrake.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","brake"});
			labelBrakeType.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","type"});
			labelBrakeControlSystem.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","control_system"});
			labelBrakeControlSpeed.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","control_speed"});
			comboboxBrakeType.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","smee"});
			comboboxBrakeType.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","ecb"});
			comboboxBrakeType.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","cl"});
			comboboxBrakeControlSystem.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","control_system_none"});
			comboboxBrakeControlSystem.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","lock_out_valve"});
			comboboxBrakeControlSystem.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"brake","delay_including_control"});

			groupboxPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","pressure"});
			labelBrakeCylinderServiceMaximumPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","brake_cylinder_service_max"});
			labelBrakeCylinderEmergencyMaximumPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","brake_cylinder_emergency_max"});
			labelMainReservoirMinimumPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","main_reservoir_min"});
			labelMainReservoirMaximumPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","main_reservoir_max"});
			labelBrakePipeNormalPressure.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"pressure","brake_pipe_normal"});

			groupboxHandle.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","handle"});
			labelHandleType.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","type"});
			labelPowerNotches.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","power_notches"});
			labelBrakeNotches.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","brake_notches"});
			labelDriverPowerNotches.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","driver_power_notches"});
			labelDriverBrakeNotches.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","driver_brake_notches"});
			labelPowerNotchReduceSteps.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","power_notch_reduce_steps"});
			comboboxHandleType.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","separated"});
			comboboxHandleType.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","combined"});
			comboboxHandleType.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","separated_interlocked"});
			comboboxHandleType.Items[3] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"handle","separated_interlocked_reverser"});

			groupboxCab.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"cab","cab"});
			labelDriverCar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"cab","driver_car"});

			groupboxCar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","car"});
			labelMotorCarMass.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","motor_car_mass"});
			labelNumberOfMotorCars.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","number_of_motor_cars"});
			labelTrailerCarMass.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","trailer_car_mass"});
			labelNumberOfTrailerCars.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","number_of_trailer_cars"});
			labelLengthOfACar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","length_of_a_car"});
			labelFrontCarIsMotorCar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","front_car_is_motor_car"});
			labelWidthOfACar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","width_of_a_car"});
			labelHeightOfACar.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","height_of_a_car"});
			labelCenterOfGravityHeight.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","center_of_gravity_height"});
			labelExposedFrontalArea.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","exposed_frontal_area"});
			labelUnexposedFrontalArea.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"car","unexposed_frontal_area"});

			groupboxDevice.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","device"});
			labelConstSpeed.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","const_speed"});
			labelHoldBrake.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","hold_brake"});
			labelReAdhesionDevice.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","readhesion_device"});
			labelPassAlarm.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","pass_alarm"});
			labelDoorOpenMode.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","door_open_mode"});
			labelDoorCloseMode.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","door_close_mode"});
			labelDoorWidth.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","door_width"});
			labelDoorMaxTolerance.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","door_max_tolerance"});
			comboboxAts.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","none"});
			comboboxAtc.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","none"});
			comboboxAtc.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","manual_switching"});
			comboboxAtc.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","automatic_switching"});
			comboboxReAdhesionDevice.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","none"});
			comboboxReAdhesionDevice.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","type_a"});
			comboboxReAdhesionDevice.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","type_b"});
			comboboxReAdhesionDevice.Items[3] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","type_c"});
			comboboxReAdhesionDevice.Items[4] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","type_d"});
			comboboxPassAlarm.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","none"});
			comboboxPassAlarm.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","single"});
			comboboxPassAlarm.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","looping"});
			comboboxDoorOpenMode.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","semi_automatic"});
			comboboxDoorOpenMode.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","automatic"});
			comboboxDoorOpenMode.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","manual"});
			comboboxDoorCloseMode.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","semi_automatic"});
			comboboxDoorCloseMode.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","automatic"});
			comboboxDoorCloseMode.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"device","manual"});

			labelAccelerationNotch.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","notch"});
			groupboxAccelerationData.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","data"});
			groupboxAccelerationPreview.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","preview"});
			checkboxAccelerationSubtractDeceleration.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","subtract_deceleration"});
			labelAccelerationMaxX.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","max_x"});
			labelAccelerationMaxY.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"acceleration","max_y"});

			groupboxMotorEdit.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","edit"});
			radiobuttonSoundIndex.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","sound_index"});
			radiobuttonPitch.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","pitch"});
			radiobuttonVolume.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","volume"});
			comboboxSoundIndex.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","sound_none"});

			groupboxMotorPreview.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","preview"});
			labelMotorInfo.Text =
				@"X: 0.00 km/h\n\n" +
				Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","y_pitch"}) + @": 100.00 (100%)\n" +
				Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","y_volume"}) + @": 128.00 (100%)";
			labelMotorMinX.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","min_x"});
			labelMotorMaxX.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","max_x"});
			buttonMotorLeft.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","left"});
			buttonMotorRight.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","right"});
			buttonMotorIn.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","in"});
			buttonMotorOut.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","out"});
			labelMotorMaxYPitch.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","max_y_pitch"});
			labelMotorMaxYVolume.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"motor","max_y_volume"});

			labelExtendedNote.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","note"});

			groupBoxLocoBrake.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake"});
			labelLocoBrakeSystemType.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_system_type"});
			labelLocoBrakeNotches.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_notches"});
			labelLocoBrakeDelayUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_delay_up"});
			labelLocoBrakeDelayDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_delay_down"});
			label2.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_type"});
			buttonLocoBrakeDelayUp.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_set"});
			buttonLocoBrakeDelayDown.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_set"});
			comboBoxLocoBrakeSystemType.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_none"});
			comboBoxLocoBrakeSystemType.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_notched_air_brake"});
			comboBoxLocoBrakeSystemType.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_cl"});
			comboBoxLocoBrakeType.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_combined"});
			comboBoxLocoBrakeType.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_independant"});
			comboBoxLocoBrakeType.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","loco_brake_blocking"});

			groupBoxMisc.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc"});
			labelEBHandleBehaviour.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc_eb"});
			comboBoxEBHandleBehaviour.Items[0] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc_eb_no"});
			comboBoxEBHandleBehaviour.Items[1] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc_eb_power"});
			comboBoxEBHandleBehaviour.Items[2] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc_eb_reverser"});
			comboBoxEBHandleBehaviour.Items[3] = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","misc_eb_power_reverser"});

			groupBoxLanguage.Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","language"});
		}

		private void buttonSetDelayPowerUp_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayPowerUp, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","power_up"}));
		}

		private void buttonSetDelayPowerDown_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayPowerDown, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","power_down"}));
		}

		private void buttonSetDelayBrakeUp_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayBrakeUp, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","brake_up"}));
		}

		private void buttonSetDelayBrakeDown_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayBrakeDown, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","brake_down"}));
		}

		private void buttonLocoBrakeDelayUp_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayLocoBrakeUp, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","delay_loco_brake_up"}));
		}

		private void buttonLocoBrakeDelayDown_Click(object sender, EventArgs e)
		{
			this.setDelay(ref this.Train.Delay.DelayLocoBrakeDown, Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"extended","delay_loco_brake_down"}));
		}

		private void setDelay(ref double[] delayValues, string delayType)
		{
			using (Form formDelay = new Form())
			{
				formDelay.ShowIcon = false;
				formDelay.FormBorderStyle = FormBorderStyle.FixedSingle;
				formDelay.MinimizeBox = false;
				formDelay.MaximizeBox = false;
				formDelay.StartPosition = FormStartPosition.CenterParent;
				formDelay.Text = delayType;
				int currentPosition = 10;
				for (int index = 0; index < delayValues.Length; ++index)
				{
					TextBox t = new TextBox
					{
						Location = new Point(20, currentPosition),
						Text = delayValues[index].ToString(CultureInfo.InvariantCulture)
					};
					formDelay.Controls.Add(t);
					Label l = new Label
					{
						Location = new Point(140, currentPosition + 3),
						Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","notch"}) + @" " + (object)index
					};
					formDelay.Controls.Add(l);
					currentPosition += 25;
				}
				Button buttonOK = new Button
				{
					Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","save"}),
					DialogResult = DialogResult.OK,
					Location = new Point(30, currentPosition)
				};
				formDelay.Controls.Add(buttonOK);
				Button buttonCancel = new Button
				{
					Text = Translations.GetInterfaceString(HostApplication.TrainEditor, new[] {"delay","cancel"}),
					DialogResult = DialogResult.Cancel,
					Location = new Point(110, currentPosition)
				};
				formDelay.Controls.Add(buttonCancel);
				int height = currentPosition + 75;
				formDelay.Size = new Size(210, height);
				if (formDelay.ShowDialog() != DialogResult.OK)
					return;
				int arrayIndex = 0;
				for (int i = 0; i < formDelay.Controls.Count; i++)
				{
					if (formDelay.Controls[i] is TextBox)
					{
						if (this.SaveControlContent((TextBox)formDelay.Controls[i], delayType + (object)i, this.tabpagePropertiesOne, formEditor.NumberRange.NonNegative, out delayValues[arrayIndex]))
						{
							arrayIndex++;
						}
						else
						{
							break;
						}
					}
				}
			}
		}

		private void numericUpDownPowerNotches_ValueChanged(object sender, EventArgs e)
		{
			int length = Train.Delay.DelayPowerUp.Length;
			if (length == numericUpDownPowerNotches.Value)
			{
				return;
			}
			Array.Resize(ref Train.Delay.DelayPowerUp, (int)numericUpDownPowerNotches.Value);
			Array.Resize(ref Train.Delay.DelayPowerDown, (int)numericUpDownPowerNotches.Value);
			if (numericUpDownPowerNotches.Value > length)
			{
				for (int i = length; i < numericUpDownPowerNotches.Value; i++)
				{
					Train.Delay.DelayPowerUp[i] = 0.0;
					Train.Delay.DelayPowerDown[i] = 0.0;
				}
			}
		}

		private void numericUpDownBrakeNotches_ValueChanged(object sender, EventArgs e)
		{
			int length = Train.Delay.DelayBrakeUp.Length;
			if (length == numericUpDownBrakeNotches.Value)
			{
				return;
			}
			Array.Resize(ref Train.Delay.DelayBrakeUp, (int)numericUpDownBrakeNotches.Value);
			Array.Resize(ref Train.Delay.DelayBrakeDown, (int)numericUpDownBrakeNotches.Value);
			if (numericUpDownBrakeNotches.Value > length)
			{
				for (int i = length; i < numericUpDownBrakeNotches.Value; i++)
				{
					Train.Delay.DelayBrakeUp[i] = 0.0;
					Train.Delay.DelayBrakeDown[i] = 0.0;
				}
			}
		}

		private void comboBoxEBHandleBehaviour_SelectedIndexChanged(object sender, EventArgs e)
		{
			Train.Handle.HandleBehaviour = (EbHandleBehaviour)comboBoxEBHandleBehaviour.SelectedIndex;
		}

		private void comboBoxLocoBrakeType_SelectedIndexChanged(object sender, EventArgs e)
		{
			Train.Handle.LocoBrake = (LocoBrakeType)comboBoxLocoBrakeType.SelectedIndex;
		}

		private void comboBoxLocoBrakeSystemType_SelectedIndexChanged(object sender, EventArgs e)
		{
			Train.Brake.LocoBrakeType = (TrainDat.Brake.LocoBrakeTypes)comboBoxLocoBrakeSystemType.SelectedIndex;
			if (Train.Brake.LocoBrakeType == TrainDat.Brake.LocoBrakeTypes.NotFitted)
			{
				numericUpDownLocoBrakeNotches.Enabled = false;
				buttonLocoBrakeDelayUp.Enabled = false;
				buttonLocoBrakeDelayDown.Enabled = false;
				comboBoxLocoBrakeType.Enabled = false;
			}
			else
			{
				numericUpDownLocoBrakeNotches.Enabled = Train.Brake.LocoBrakeType != TrainDat.Brake.LocoBrakeTypes.AutomaticAirBrake;
				buttonLocoBrakeDelayUp.Enabled = true;
				buttonLocoBrakeDelayDown.Enabled = true;
				comboBoxLocoBrakeType.Enabled = true;
			}
		}
	}
}
