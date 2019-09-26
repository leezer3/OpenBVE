using System.Windows.Forms;

namespace TrainEditor2.Views
{
	partial class FormEditor
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			disposable.Dispose();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabControlEditor = new System.Windows.Forms.TabControl();
			this.tabPageTrain = new System.Windows.Forms.TabPage();
			this.groupBoxDevice = new System.Windows.Forms.GroupBox();
			this.labelDoorMaxToleranceUnit = new System.Windows.Forms.Label();
			this.textBoxDoorMaxTolerance = new System.Windows.Forms.TextBox();
			this.labelDoorWidthUnit = new System.Windows.Forms.Label();
			this.textBoxDoorWidth = new System.Windows.Forms.TextBox();
			this.comboBoxDoorCloseMode = new System.Windows.Forms.ComboBox();
			this.comboBoxDoorOpenMode = new System.Windows.Forms.ComboBox();
			this.comboBoxPassAlarm = new System.Windows.Forms.ComboBox();
			this.comboBoxReAdhesionDevice = new System.Windows.Forms.ComboBox();
			this.comboBoxAtc = new System.Windows.Forms.ComboBox();
			this.comboBoxAts = new System.Windows.Forms.ComboBox();
			this.checkBoxHoldBrake = new System.Windows.Forms.CheckBox();
			this.checkBoxEb = new System.Windows.Forms.CheckBox();
			this.checkBoxConstSpeed = new System.Windows.Forms.CheckBox();
			this.labelDoorMaxTolerance = new System.Windows.Forms.Label();
			this.labelDoorWidth = new System.Windows.Forms.Label();
			this.labelDoorCloseMode = new System.Windows.Forms.Label();
			this.labelDoorOpenMode = new System.Windows.Forms.Label();
			this.labelPassAlarm = new System.Windows.Forms.Label();
			this.labelReAdhesionDevice = new System.Windows.Forms.Label();
			this.labelHoldBrake = new System.Windows.Forms.Label();
			this.labelConstSpeed = new System.Windows.Forms.Label();
			this.labelEb = new System.Windows.Forms.Label();
			this.labelAtc = new System.Windows.Forms.Label();
			this.labelAts = new System.Windows.Forms.Label();
			this.groupBoxCab = new System.Windows.Forms.GroupBox();
			this.comboBoxDriverCar = new System.Windows.Forms.ComboBox();
			this.labelCabZUnit = new System.Windows.Forms.Label();
			this.textBoxCabZ = new System.Windows.Forms.TextBox();
			this.labelCabYUnit = new System.Windows.Forms.Label();
			this.textBoxCabY = new System.Windows.Forms.TextBox();
			this.labelCabXUnit = new System.Windows.Forms.Label();
			this.textBoxCabX = new System.Windows.Forms.TextBox();
			this.labelDriverCar = new System.Windows.Forms.Label();
			this.labelCabZ = new System.Windows.Forms.Label();
			this.labelCabY = new System.Windows.Forms.Label();
			this.labelCabX = new System.Windows.Forms.Label();
			this.groupBoxHandle = new System.Windows.Forms.GroupBox();
			this.numericUpDownLocoBrakeNotches = new System.Windows.Forms.NumericUpDown();
			this.labelLocoBrakeNotches = new System.Windows.Forms.Label();
			this.comboBoxLocoBrakeHandleType = new System.Windows.Forms.ComboBox();
			this.labelLocoBrakeHandleType = new System.Windows.Forms.Label();
			this.comboBoxEbHandleBehaviour = new System.Windows.Forms.ComboBox();
			this.labelEbHandleBehaviour = new System.Windows.Forms.Label();
			this.numericUpDownDriverBrakeNotches = new System.Windows.Forms.NumericUpDown();
			this.labelDriverBrakeNotches = new System.Windows.Forms.Label();
			this.numericUpDownDriverPowerNotches = new System.Windows.Forms.NumericUpDown();
			this.labelDriverPowerNotches = new System.Windows.Forms.Label();
			this.numericUpDownPowerNotchReduceSteps = new System.Windows.Forms.NumericUpDown();
			this.labelPowerNotchReduceSteps = new System.Windows.Forms.Label();
			this.numericUpDownBrakeNotches = new System.Windows.Forms.NumericUpDown();
			this.labelBrakeNotches = new System.Windows.Forms.Label();
			this.numericUpDownPowerNotches = new System.Windows.Forms.NumericUpDown();
			this.labelPowerNotches = new System.Windows.Forms.Label();
			this.comboBoxHandleType = new System.Windows.Forms.ComboBox();
			this.labelHandleType = new System.Windows.Forms.Label();
			this.tabPageCar = new System.Windows.Forms.TabPage();
			this.groupBoxPressure = new System.Windows.Forms.GroupBox();
			this.labelBrakePipeNormalPressureUnit = new System.Windows.Forms.Label();
			this.textBoxBrakePipeNormalPressure = new System.Windows.Forms.TextBox();
			this.labelBrakePipeNormalPressure = new System.Windows.Forms.Label();
			this.labelBrakeCylinderServiceMaximumPressureUnit = new System.Windows.Forms.Label();
			this.labelMainReservoirMaximumPressureUnit = new System.Windows.Forms.Label();
			this.labelMainReservoirMinimumPressureUnit = new System.Windows.Forms.Label();
			this.labelBrakeCylinderEmergencyMaximumPressureUnit = new System.Windows.Forms.Label();
			this.textBoxMainReservoirMaximumPressure = new System.Windows.Forms.TextBox();
			this.textBoxMainReservoirMinimumPressure = new System.Windows.Forms.TextBox();
			this.textBoxBrakeCylinderEmergencyMaximumPressure = new System.Windows.Forms.TextBox();
			this.textBoxBrakeCylinderServiceMaximumPressure = new System.Windows.Forms.TextBox();
			this.labelMainReservoirMaximumPressure = new System.Windows.Forms.Label();
			this.labelMainReservoirMinimumPressure = new System.Windows.Forms.Label();
			this.labelBrakeCylinderServiceMaximumPressure = new System.Windows.Forms.Label();
			this.labelBrakeCylinderEmergencyMaximumPressure = new System.Windows.Forms.Label();
			this.groupBoxBrake = new System.Windows.Forms.GroupBox();
			this.textBoxBrakeControlSpeed = new System.Windows.Forms.TextBox();
			this.labelBrakeControlSpeedUnit = new System.Windows.Forms.Label();
			this.comboBoxBrakeControlSystem = new System.Windows.Forms.ComboBox();
			this.comboBoxLocoBrakeType = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakeType = new System.Windows.Forms.ComboBox();
			this.labelLocoBrakeType = new System.Windows.Forms.Label();
			this.labelBrakeType = new System.Windows.Forms.Label();
			this.labelBrakeControlSpeed = new System.Windows.Forms.Label();
			this.labelBrakeControlSystem = new System.Windows.Forms.Label();
			this.groupBoxMove = new System.Windows.Forms.GroupBox();
			this.labelBrakeCylinderDownUnit = new System.Windows.Forms.Label();
			this.textBoxBrakeCylinderDown = new System.Windows.Forms.TextBox();
			this.labelBrakeCylinderUpUnit = new System.Windows.Forms.Label();
			this.textBoxBrakeCylinderUp = new System.Windows.Forms.TextBox();
			this.labelJerkBrakeDownUnit = new System.Windows.Forms.Label();
			this.textBoxJerkBrakeDown = new System.Windows.Forms.TextBox();
			this.labelJerkBrakeUpUnit = new System.Windows.Forms.Label();
			this.textBoxJerkBrakeUp = new System.Windows.Forms.TextBox();
			this.labelJerkPowerDownUnit = new System.Windows.Forms.Label();
			this.textBoxJerkPowerDown = new System.Windows.Forms.TextBox();
			this.labelJerkPowerUpUnit = new System.Windows.Forms.Label();
			this.textBoxJerkPowerUp = new System.Windows.Forms.TextBox();
			this.labelJerkPowerUp = new System.Windows.Forms.Label();
			this.labelJerkPowerDown = new System.Windows.Forms.Label();
			this.labelJerkBrakeUp = new System.Windows.Forms.Label();
			this.labelJerkBrakeDown = new System.Windows.Forms.Label();
			this.labelBrakeCylinderUp = new System.Windows.Forms.Label();
			this.labelBrakeCylinderDown = new System.Windows.Forms.Label();
			this.groupBoxDelay = new System.Windows.Forms.GroupBox();
			this.buttonDelayLocoBrakeSet = new System.Windows.Forms.Button();
			this.buttonDelayBrakeSet = new System.Windows.Forms.Button();
			this.buttonDelayPowerSet = new System.Windows.Forms.Button();
			this.labelDelayLocoBrake = new System.Windows.Forms.Label();
			this.labelDelayBrake = new System.Windows.Forms.Label();
			this.labelDelayPower = new System.Windows.Forms.Label();
			this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
			this.textBoxAerodynamicDragCoefficient = new System.Windows.Forms.TextBox();
			this.textBoxCoefficientOfRollingResistance = new System.Windows.Forms.TextBox();
			this.textBoxCoefficientOfStaticFriction = new System.Windows.Forms.TextBox();
			this.labelDecelerationUnit = new System.Windows.Forms.Label();
			this.textBoxDeceleration = new System.Windows.Forms.TextBox();
			this.labelAerodynamicDragCoefficient = new System.Windows.Forms.Label();
			this.labelCoefficientOfStaticFriction = new System.Windows.Forms.Label();
			this.labelDeceleration = new System.Windows.Forms.Label();
			this.labelCoefficientOfRollingResistance = new System.Windows.Forms.Label();
			this.groupBoxCarGeneral = new System.Windows.Forms.GroupBox();
			this.labelUnexposedFrontalAreaUnit = new System.Windows.Forms.Label();
			this.textBoxUnexposedFrontalArea = new System.Windows.Forms.TextBox();
			this.labelUnexposedFrontalArea = new System.Windows.Forms.Label();
			this.labelRearBogie = new System.Windows.Forms.Label();
			this.labelFrontBogie = new System.Windows.Forms.Label();
			this.buttonRearBogieSet = new System.Windows.Forms.Button();
			this.buttonFrontBogieSet = new System.Windows.Forms.Button();
			this.checkBoxReversed = new System.Windows.Forms.CheckBox();
			this.checkBoxLoadingSway = new System.Windows.Forms.CheckBox();
			this.checkBoxDefinedAxles = new System.Windows.Forms.CheckBox();
			this.checkBoxIsMotorCar = new System.Windows.Forms.CheckBox();
			this.buttonObjectOpen = new System.Windows.Forms.Button();
			this.labelExposedFrontalAreaUnit = new System.Windows.Forms.Label();
			this.labelCenterOfMassHeightUnit = new System.Windows.Forms.Label();
			this.labelLoadingSway = new System.Windows.Forms.Label();
			this.labelHeightUnit = new System.Windows.Forms.Label();
			this.labelWidthUnit = new System.Windows.Forms.Label();
			this.labelLengthUnit = new System.Windows.Forms.Label();
			this.labelMassUnit = new System.Windows.Forms.Label();
			this.textBoxMass = new System.Windows.Forms.TextBox();
			this.textBoxLength = new System.Windows.Forms.TextBox();
			this.textBoxWidth = new System.Windows.Forms.TextBox();
			this.textBoxHeight = new System.Windows.Forms.TextBox();
			this.textBoxCenterOfMassHeight = new System.Windows.Forms.TextBox();
			this.textBoxExposedFrontalArea = new System.Windows.Forms.TextBox();
			this.textBoxObject = new System.Windows.Forms.TextBox();
			this.labelObject = new System.Windows.Forms.Label();
			this.labelReversed = new System.Windows.Forms.Label();
			this.groupBoxAxles = new System.Windows.Forms.GroupBox();
			this.labelRearAxleUnit = new System.Windows.Forms.Label();
			this.labelFrontAxleUnit = new System.Windows.Forms.Label();
			this.textBoxRearAxle = new System.Windows.Forms.TextBox();
			this.textBoxFrontAxle = new System.Windows.Forms.TextBox();
			this.labelRearAxle = new System.Windows.Forms.Label();
			this.labelFrontAxle = new System.Windows.Forms.Label();
			this.labelDefinedAxles = new System.Windows.Forms.Label();
			this.labelExposedFrontalArea = new System.Windows.Forms.Label();
			this.labelCenterOfMassHeight = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelLength = new System.Windows.Forms.Label();
			this.labelMass = new System.Windows.Forms.Label();
			this.labelIsMotorCar = new System.Windows.Forms.Label();
			this.tabPageAccel = new System.Windows.Forms.TabPage();
			this.pictureBoxAccel = new System.Windows.Forms.PictureBox();
			this.panelAccel = new System.Windows.Forms.Panel();
			this.groupBoxPreview = new System.Windows.Forms.GroupBox();
			this.buttonAccelReset = new System.Windows.Forms.Button();
			this.buttonAccelZoomOut = new System.Windows.Forms.Button();
			this.buttonAccelZoomIn = new System.Windows.Forms.Button();
			this.labelAccelYValue = new System.Windows.Forms.Label();
			this.labelAccelXValue = new System.Windows.Forms.Label();
			this.labelAccelXmaxUnit = new System.Windows.Forms.Label();
			this.labelAccelXminUnit = new System.Windows.Forms.Label();
			this.labelAccelYmaxUnit = new System.Windows.Forms.Label();
			this.labelAccelYminUnit = new System.Windows.Forms.Label();
			this.textBoxAccelYmax = new System.Windows.Forms.TextBox();
			this.textBoxAccelYmin = new System.Windows.Forms.TextBox();
			this.textBoxAccelXmax = new System.Windows.Forms.TextBox();
			this.textBoxAccelXmin = new System.Windows.Forms.TextBox();
			this.labelAccelYmax = new System.Windows.Forms.Label();
			this.labelAccelYmin = new System.Windows.Forms.Label();
			this.labelAccelXmax = new System.Windows.Forms.Label();
			this.labelAccelXmin = new System.Windows.Forms.Label();
			this.labelAccelY = new System.Windows.Forms.Label();
			this.labelAccelX = new System.Windows.Forms.Label();
			this.checkBoxSubtractDeceleration = new System.Windows.Forms.CheckBox();
			this.groupBoxParameter = new System.Windows.Forms.GroupBox();
			this.labeAccelA0Unit = new System.Windows.Forms.Label();
			this.textBoxAccelA0 = new System.Windows.Forms.TextBox();
			this.labelAccelA0 = new System.Windows.Forms.Label();
			this.labelAccelA1Unit = new System.Windows.Forms.Label();
			this.textBoxAccelA1 = new System.Windows.Forms.TextBox();
			this.labelAccelA1 = new System.Windows.Forms.Label();
			this.labelAccelV1Unit = new System.Windows.Forms.Label();
			this.textBoxAccelV1 = new System.Windows.Forms.TextBox();
			this.labelAccelV1 = new System.Windows.Forms.Label();
			this.labelAccelV2Unit = new System.Windows.Forms.Label();
			this.textBoxAccelV2 = new System.Windows.Forms.TextBox();
			this.labelAccelV2 = new System.Windows.Forms.Label();
			this.textBoxAccelE = new System.Windows.Forms.TextBox();
			this.labelAccelE = new System.Windows.Forms.Label();
			this.groupBoxNotch = new System.Windows.Forms.GroupBox();
			this.comboBoxNotch = new System.Windows.Forms.ComboBox();
			this.tabPageMotor = new System.Windows.Forms.TabPage();
			this.panelMotorSound = new System.Windows.Forms.Panel();
			this.toolStripContainerDrawArea = new System.Windows.Forms.ToolStripContainer();
			this.glControlMotor = new OpenTK.GLControl();
			this.toolStripToolBar = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorRedo = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonTearingOff = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCopy = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPaste = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCleanup = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorEdit = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonSelect = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDot = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLine = new System.Windows.Forms.ToolStripButton();
			this.panelMotorSetting = new System.Windows.Forms.Panel();
			this.groupBoxDirect = new System.Windows.Forms.GroupBox();
			this.buttonDirectDot = new System.Windows.Forms.Button();
			this.buttonDirectMove = new System.Windows.Forms.Button();
			this.textBoxDirectY = new System.Windows.Forms.TextBox();
			this.labelDirectY = new System.Windows.Forms.Label();
			this.labelDirectXUnit = new System.Windows.Forms.Label();
			this.textBoxDirectX = new System.Windows.Forms.TextBox();
			this.labelDirectX = new System.Windows.Forms.Label();
			this.groupBoxPlay = new System.Windows.Forms.GroupBox();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonPause = new System.Windows.Forms.Button();
			this.buttonPlay = new System.Windows.Forms.Button();
			this.groupBoxArea = new System.Windows.Forms.GroupBox();
			this.checkBoxMotorConstant = new System.Windows.Forms.CheckBox();
			this.checkBoxMotorLoop = new System.Windows.Forms.CheckBox();
			this.buttonMotorSwap = new System.Windows.Forms.Button();
			this.textBoxMotorAreaLeft = new System.Windows.Forms.TextBox();
			this.labelMotorAreaUnit = new System.Windows.Forms.Label();
			this.textBoxMotorAreaRight = new System.Windows.Forms.TextBox();
			this.labelMotorAccel = new System.Windows.Forms.Label();
			this.labelMotorAccelUnit = new System.Windows.Forms.Label();
			this.textBoxMotorAccel = new System.Windows.Forms.TextBox();
			this.groupBoxSource = new System.Windows.Forms.GroupBox();
			this.numericUpDownRunIndex = new System.Windows.Forms.NumericUpDown();
			this.labelRun = new System.Windows.Forms.Label();
			this.checkBoxTrack2 = new System.Windows.Forms.CheckBox();
			this.checkBoxTrack1 = new System.Windows.Forms.CheckBox();
			this.groupBoxView = new System.Windows.Forms.GroupBox();
			this.buttonMotorReset = new System.Windows.Forms.Button();
			this.buttonMotorZoomOut = new System.Windows.Forms.Button();
			this.buttonMotorZoomIn = new System.Windows.Forms.Button();
			this.labelMotorMaxVolume = new System.Windows.Forms.Label();
			this.textBoxMotorMaxVolume = new System.Windows.Forms.TextBox();
			this.labelMotorMinVolume = new System.Windows.Forms.Label();
			this.textBoxMotorMinVolume = new System.Windows.Forms.TextBox();
			this.labelMotorMaxPitch = new System.Windows.Forms.Label();
			this.textBoxMotorMaxPitch = new System.Windows.Forms.TextBox();
			this.labelMotorMinPitch = new System.Windows.Forms.Label();
			this.textBoxMotorMinPitch = new System.Windows.Forms.TextBox();
			this.labelMotorMaxVelocityUnit = new System.Windows.Forms.Label();
			this.textBoxMotorMaxVelocity = new System.Windows.Forms.TextBox();
			this.labelMotorMaxVelocity = new System.Windows.Forms.Label();
			this.labelMotorMinVelocityUnit = new System.Windows.Forms.Label();
			this.textBoxMotorMinVelocity = new System.Windows.Forms.TextBox();
			this.labelMotorMinVelocity = new System.Windows.Forms.Label();
			this.statusStripStatus = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTool = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelMode = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTrack = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelType = new System.Windows.Forms.ToolStripStatusLabel();
			this.menuStripMotor = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorUndo = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemTearingOff = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemCleanup = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemView = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPower = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPowerTrack1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPowerTrack2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemBrake = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemBrakeTrack1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemBrakeTrack2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemInput = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPitch = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemVolume = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemIndex = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripComboBoxIndex = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripMenuItemTool = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSelect = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemMove = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDot = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLine = new System.Windows.Forms.ToolStripMenuItem();
			this.tabPageCoupler = new System.Windows.Forms.TabPage();
			this.groupBoxCouplerGeneral = new System.Windows.Forms.GroupBox();
			this.buttonCouplerObject = new System.Windows.Forms.Button();
			this.textBoxCouplerObject = new System.Windows.Forms.TextBox();
			this.labelCouplerObject = new System.Windows.Forms.Label();
			this.labelCouplerMaxUnit = new System.Windows.Forms.Label();
			this.textBoxCouplerMax = new System.Windows.Forms.TextBox();
			this.labelCouplerMax = new System.Windows.Forms.Label();
			this.labelCouplerMinUnit = new System.Windows.Forms.Label();
			this.textBoxCouplerMin = new System.Windows.Forms.TextBox();
			this.labelCouplerMin = new System.Windows.Forms.Label();
			this.tabPagePanel = new System.Windows.Forms.TabPage();
			this.splitContainerPanel = new System.Windows.Forms.SplitContainer();
			this.treeViewPanel = new System.Windows.Forms.TreeView();
			this.listViewPanel = new System.Windows.Forms.ListView();
			this.panelPanelNavi = new System.Windows.Forms.Panel();
			this.panelPanelNaviCmd = new System.Windows.Forms.Panel();
			this.buttonPanelCopy = new System.Windows.Forms.Button();
			this.buttonPanelAdd = new System.Windows.Forms.Button();
			this.buttonPanelRemove = new System.Windows.Forms.Button();
			this.tabControlPanel = new System.Windows.Forms.TabControl();
			this.tabPageThis = new System.Windows.Forms.TabPage();
			this.buttonThisTransparentColorSet = new System.Windows.Forms.Button();
			this.buttonThisNighttimeImageOpen = new System.Windows.Forms.Button();
			this.buttonThisDaytimeImageOpen = new System.Windows.Forms.Button();
			this.groupBoxThisOrigin = new System.Windows.Forms.GroupBox();
			this.textBoxThisOriginY = new System.Windows.Forms.TextBox();
			this.textBoxThisOriginX = new System.Windows.Forms.TextBox();
			this.labelThisOriginY = new System.Windows.Forms.Label();
			this.labelThisOriginX = new System.Windows.Forms.Label();
			this.groupBoxThisCenter = new System.Windows.Forms.GroupBox();
			this.textBoxThisCenterY = new System.Windows.Forms.TextBox();
			this.textBoxThisCenterX = new System.Windows.Forms.TextBox();
			this.labelThisCenterY = new System.Windows.Forms.Label();
			this.labelThisCenterX = new System.Windows.Forms.Label();
			this.textBoxThisTransparentColor = new System.Windows.Forms.TextBox();
			this.textBoxThisNighttimeImage = new System.Windows.Forms.TextBox();
			this.textBoxThisDaytimeImage = new System.Windows.Forms.TextBox();
			this.textBoxThisBottom = new System.Windows.Forms.TextBox();
			this.textBoxThisTop = new System.Windows.Forms.TextBox();
			this.textBoxThisRight = new System.Windows.Forms.TextBox();
			this.textBoxThisLeft = new System.Windows.Forms.TextBox();
			this.textBoxThisResolution = new System.Windows.Forms.TextBox();
			this.labelThisResolution = new System.Windows.Forms.Label();
			this.labelThisLeft = new System.Windows.Forms.Label();
			this.labelThisRight = new System.Windows.Forms.Label();
			this.labelThisTop = new System.Windows.Forms.Label();
			this.labelThisBottom = new System.Windows.Forms.Label();
			this.labelThisDaytimeImage = new System.Windows.Forms.Label();
			this.labelThisNighttimeImage = new System.Windows.Forms.Label();
			this.labelThisTransparentColor = new System.Windows.Forms.Label();
			this.tabPageScreen = new System.Windows.Forms.TabPage();
			this.numericUpDownScreenLayer = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownScreenNumber = new System.Windows.Forms.NumericUpDown();
			this.labelScreenLayer = new System.Windows.Forms.Label();
			this.labelScreenNumber = new System.Windows.Forms.Label();
			this.tabPagePilotLamp = new System.Windows.Forms.TabPage();
			this.buttonPilotLampTransparentColorSet = new System.Windows.Forms.Button();
			this.numericUpDownPilotLampLayer = new System.Windows.Forms.NumericUpDown();
			this.groupBoxPilotLampLocation = new System.Windows.Forms.GroupBox();
			this.textBoxPilotLampLocationY = new System.Windows.Forms.TextBox();
			this.textBoxPilotLampLocationX = new System.Windows.Forms.TextBox();
			this.labelPilotLampLocationY = new System.Windows.Forms.Label();
			this.labelPilotLampLocationX = new System.Windows.Forms.Label();
			this.buttonPilotLampSubjectSet = new System.Windows.Forms.Button();
			this.labelPilotLampSubject = new System.Windows.Forms.Label();
			this.labelPilotLampLayer = new System.Windows.Forms.Label();
			this.buttonPilotLampNighttimeImageOpen = new System.Windows.Forms.Button();
			this.buttonPilotLampDaytimeImageOpen = new System.Windows.Forms.Button();
			this.textBoxPilotLampTransparentColor = new System.Windows.Forms.TextBox();
			this.textBoxPilotLampNighttimeImage = new System.Windows.Forms.TextBox();
			this.textBoxPilotLampDaytimeImage = new System.Windows.Forms.TextBox();
			this.labelPilotLampDaytimeImage = new System.Windows.Forms.Label();
			this.labelPilotLampNighttimeImage = new System.Windows.Forms.Label();
			this.labelPilotLampTransparentColor = new System.Windows.Forms.Label();
			this.tabPageNeedle = new System.Windows.Forms.TabPage();
			this.checkBoxNeedleDefinedDampingRatio = new System.Windows.Forms.CheckBox();
			this.checkBoxNeedleDefinedNaturalFreq = new System.Windows.Forms.CheckBox();
			this.labelNeedleDefinedDampingRatio = new System.Windows.Forms.Label();
			this.labelNeedleDefinedNaturalFreq = new System.Windows.Forms.Label();
			this.buttonNeedleTransparentColorSet = new System.Windows.Forms.Button();
			this.buttonNeedleColorSet = new System.Windows.Forms.Button();
			this.checkBoxNeedleDefinedOrigin = new System.Windows.Forms.CheckBox();
			this.labelNeedleDefinedOrigin = new System.Windows.Forms.Label();
			this.checkBoxNeedleDefinedRadius = new System.Windows.Forms.CheckBox();
			this.labelNeedleDefinedRadius = new System.Windows.Forms.Label();
			this.numericUpDownNeedleLayer = new System.Windows.Forms.NumericUpDown();
			this.checkBoxNeedleSmoothed = new System.Windows.Forms.CheckBox();
			this.labelNeedleSmoothed = new System.Windows.Forms.Label();
			this.checkBoxNeedleBackstop = new System.Windows.Forms.CheckBox();
			this.labelNeedleBackstop = new System.Windows.Forms.Label();
			this.textBoxNeedleDampingRatio = new System.Windows.Forms.TextBox();
			this.labelNeedleDampingRatio = new System.Windows.Forms.Label();
			this.textBoxNeedleNaturalFreq = new System.Windows.Forms.TextBox();
			this.labelNeedleNaturalFreq = new System.Windows.Forms.Label();
			this.textBoxNeedleMaximum = new System.Windows.Forms.TextBox();
			this.labelNeedleMaximum = new System.Windows.Forms.Label();
			this.textBoxNeedleMinimum = new System.Windows.Forms.TextBox();
			this.labelNeedleMinimum = new System.Windows.Forms.Label();
			this.textBoxNeedleLastAngle = new System.Windows.Forms.TextBox();
			this.labelNeedleLastAngle = new System.Windows.Forms.Label();
			this.textBoxNeedleInitialAngle = new System.Windows.Forms.TextBox();
			this.labelNeedleInitialAngle = new System.Windows.Forms.Label();
			this.groupBoxNeedleOrigin = new System.Windows.Forms.GroupBox();
			this.textBoxNeedleOriginY = new System.Windows.Forms.TextBox();
			this.textBoxNeedleOriginX = new System.Windows.Forms.TextBox();
			this.labelNeedleOriginY = new System.Windows.Forms.Label();
			this.labelNeedleOriginX = new System.Windows.Forms.Label();
			this.textBoxNeedleColor = new System.Windows.Forms.TextBox();
			this.labelNeedleColor = new System.Windows.Forms.Label();
			this.textBoxNeedleRadius = new System.Windows.Forms.TextBox();
			this.labelNeedleRadius = new System.Windows.Forms.Label();
			this.groupBoxNeedleLocation = new System.Windows.Forms.GroupBox();
			this.textBoxNeedleLocationY = new System.Windows.Forms.TextBox();
			this.textBoxNeedleLocationX = new System.Windows.Forms.TextBox();
			this.labelNeedleLocationY = new System.Windows.Forms.Label();
			this.labelNeedleLocationX = new System.Windows.Forms.Label();
			this.buttonNeedleSubjectSet = new System.Windows.Forms.Button();
			this.labelNeedleSubject = new System.Windows.Forms.Label();
			this.labelNeedleLayer = new System.Windows.Forms.Label();
			this.buttonNeedleNighttimeImageOpen = new System.Windows.Forms.Button();
			this.buttonNeedleDaytimeImageOpen = new System.Windows.Forms.Button();
			this.textBoxNeedleTransparentColor = new System.Windows.Forms.TextBox();
			this.textBoxNeedleNighttimeImage = new System.Windows.Forms.TextBox();
			this.textBoxNeedleDaytimeImage = new System.Windows.Forms.TextBox();
			this.labelNeedleDaytimeImage = new System.Windows.Forms.Label();
			this.labelNeedleNighttimeImage = new System.Windows.Forms.Label();
			this.labelNeedleTransparentColor = new System.Windows.Forms.Label();
			this.tabPageDigitalNumber = new System.Windows.Forms.TabPage();
			this.buttonDigitalNumberTransparentColorSet = new System.Windows.Forms.Button();
			this.numericUpDownDigitalNumberLayer = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownDigitalNumberInterval = new System.Windows.Forms.NumericUpDown();
			this.labelDigitalNumberInterval = new System.Windows.Forms.Label();
			this.groupBoxDigitalNumberLocation = new System.Windows.Forms.GroupBox();
			this.textBoxDigitalNumberLocationY = new System.Windows.Forms.TextBox();
			this.textBoxDigitalNumberLocationX = new System.Windows.Forms.TextBox();
			this.labelDigitalNumberLocationY = new System.Windows.Forms.Label();
			this.labelDigitalNumberLocationX = new System.Windows.Forms.Label();
			this.buttonDigitalNumberSubjectSet = new System.Windows.Forms.Button();
			this.labelDigitalNumberSubject = new System.Windows.Forms.Label();
			this.labelDigitalNumberLayer = new System.Windows.Forms.Label();
			this.buttonDigitalNumberNighttimeImageOpen = new System.Windows.Forms.Button();
			this.buttonDigitalNumberDaytimeImageOpen = new System.Windows.Forms.Button();
			this.textBoxDigitalNumberTransparentColor = new System.Windows.Forms.TextBox();
			this.textBoxDigitalNumberNighttimeImage = new System.Windows.Forms.TextBox();
			this.textBoxDigitalNumberDaytimeImage = new System.Windows.Forms.TextBox();
			this.labelDigitalNumberDaytimeImage = new System.Windows.Forms.Label();
			this.labelDigitalNumberNighttimeImage = new System.Windows.Forms.Label();
			this.labelDigitalNumberTransparentColor = new System.Windows.Forms.Label();
			this.tabPageDigitalGauge = new System.Windows.Forms.TabPage();
			this.buttonDigitalGaugeColorSet = new System.Windows.Forms.Button();
			this.numericUpDownDigitalGaugeLayer = new System.Windows.Forms.NumericUpDown();
			this.textBoxDigitalGaugeStep = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeStep = new System.Windows.Forms.Label();
			this.labelDigitalGaugeLayer = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeMaximum = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeMaximum = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeMinimum = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeMinimum = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeLastAngle = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeLastAngle = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeInitialAngle = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeInitialAngle = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeColor = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeColor = new System.Windows.Forms.Label();
			this.textBoxDigitalGaugeRadius = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeRadius = new System.Windows.Forms.Label();
			this.groupBoxDigitalGaugeLocation = new System.Windows.Forms.GroupBox();
			this.textBoxDigitalGaugeLocationY = new System.Windows.Forms.TextBox();
			this.textBoxDigitalGaugeLocationX = new System.Windows.Forms.TextBox();
			this.labelDigitalGaugeLocationY = new System.Windows.Forms.Label();
			this.labelDigitalGaugeLocationX = new System.Windows.Forms.Label();
			this.buttonDigitalGaugeSubjectSet = new System.Windows.Forms.Button();
			this.labelDigitalGaugeSubject = new System.Windows.Forms.Label();
			this.tabPageLinearGauge = new System.Windows.Forms.TabPage();
			this.buttonLinearGaugeTransparentColorSet = new System.Windows.Forms.Button();
			this.buttonLinearGaugeNighttimeImageOpen = new System.Windows.Forms.Button();
			this.buttonLinearGaugeDaytimeImageOpen = new System.Windows.Forms.Button();
			this.textBoxLinearGaugeTransparentColor = new System.Windows.Forms.TextBox();
			this.textBoxLinearGaugeNighttimeImage = new System.Windows.Forms.TextBox();
			this.textBoxLinearGaugeDaytimeImage = new System.Windows.Forms.TextBox();
			this.labelLinearGaugeDaytimeImage = new System.Windows.Forms.Label();
			this.labelLinearGaugeNighttimeImage = new System.Windows.Forms.Label();
			this.labelLinearGaugeTransparentColor = new System.Windows.Forms.Label();
			this.numericUpDownLinearGaugeLayer = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownLinearGaugeWidth = new System.Windows.Forms.NumericUpDown();
			this.labelLinearGaugeWidth = new System.Windows.Forms.Label();
			this.groupBoxLinearGaugeDirection = new System.Windows.Forms.GroupBox();
			this.numericUpDownLinearGaugeDirectionY = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownLinearGaugeDirectionX = new System.Windows.Forms.NumericUpDown();
			this.labelLinearGaugeDirectionY = new System.Windows.Forms.Label();
			this.labelLinearGaugeDirectionX = new System.Windows.Forms.Label();
			this.labelLinearGaugeLayer = new System.Windows.Forms.Label();
			this.textBoxLinearGaugeMaximum = new System.Windows.Forms.TextBox();
			this.labelLinearGaugeMaximum = new System.Windows.Forms.Label();
			this.textBoxLinearGaugeMinimum = new System.Windows.Forms.TextBox();
			this.labelLinearGaugeMinimum = new System.Windows.Forms.Label();
			this.groupBoxLinearGaugeLocation = new System.Windows.Forms.GroupBox();
			this.textBoxLinearGaugeLocationY = new System.Windows.Forms.TextBox();
			this.textBoxLinearGaugeLocationX = new System.Windows.Forms.TextBox();
			this.labelLinearGaugeLocationY = new System.Windows.Forms.Label();
			this.labelLinearGaugeLocationX = new System.Windows.Forms.Label();
			this.buttonLinearGaugeSubjectSet = new System.Windows.Forms.Button();
			this.labelLinearGaugeSubject = new System.Windows.Forms.Label();
			this.tabPageTimetable = new System.Windows.Forms.TabPage();
			this.buttonTimetableTransparentColorSet = new System.Windows.Forms.Button();
			this.numericUpDownTimetableLayer = new System.Windows.Forms.NumericUpDown();
			this.labelTimetableLayer = new System.Windows.Forms.Label();
			this.textBoxTimetableTransparentColor = new System.Windows.Forms.TextBox();
			this.labelTimetableTransparentColor = new System.Windows.Forms.Label();
			this.textBoxTimetableHeight = new System.Windows.Forms.TextBox();
			this.labelTimetableHeight = new System.Windows.Forms.Label();
			this.textBoxTimetableWidth = new System.Windows.Forms.TextBox();
			this.labelTimetableWidth = new System.Windows.Forms.Label();
			this.groupBoxTimetableLocation = new System.Windows.Forms.GroupBox();
			this.textBoxTimetableLocationY = new System.Windows.Forms.TextBox();
			this.textBoxTimetableLocationX = new System.Windows.Forms.TextBox();
			this.labelTimetableLocationY = new System.Windows.Forms.Label();
			this.labelTimetableLocationX = new System.Windows.Forms.Label();
			this.tabPageTouch = new System.Windows.Forms.TabPage();
			this.comboBoxTouchCommand = new System.Windows.Forms.ComboBox();
			this.labelTouchCommand = new System.Windows.Forms.Label();
			this.numericUpDownTouchCommandOption = new System.Windows.Forms.NumericUpDown();
			this.labelTouchCommandOption = new System.Windows.Forms.Label();
			this.numericUpDownTouchSoundIndex = new System.Windows.Forms.NumericUpDown();
			this.labelTouchSoundIndex = new System.Windows.Forms.Label();
			this.numericUpDownTouchJumpScreen = new System.Windows.Forms.NumericUpDown();
			this.labelTouchJumpScreen = new System.Windows.Forms.Label();
			this.groupBoxTouchSize = new System.Windows.Forms.GroupBox();
			this.textBoxTouchSizeY = new System.Windows.Forms.TextBox();
			this.textBoxTouchSizeX = new System.Windows.Forms.TextBox();
			this.labelTouchSizeY = new System.Windows.Forms.Label();
			this.labelTouchSizeX = new System.Windows.Forms.Label();
			this.groupBoxTouchLocation = new System.Windows.Forms.GroupBox();
			this.textBoxTouchLocationY = new System.Windows.Forms.TextBox();
			this.textBoxTouchLocationX = new System.Windows.Forms.TextBox();
			this.labelTouchLocationY = new System.Windows.Forms.Label();
			this.labelTouchLocationX = new System.Windows.Forms.Label();
			this.tabPageSound = new System.Windows.Forms.TabPage();
			this.panelSoundSetting = new System.Windows.Forms.Panel();
			this.splitContainerSound = new System.Windows.Forms.SplitContainer();
			this.treeViewSound = new System.Windows.Forms.TreeView();
			this.listViewSound = new System.Windows.Forms.ListView();
			this.panelSoundSettingEdit = new System.Windows.Forms.Panel();
			this.groupBoxSoundEntry = new System.Windows.Forms.GroupBox();
			this.buttonSoundRemove = new System.Windows.Forms.Button();
			this.groupBoxSoundKey = new System.Windows.Forms.GroupBox();
			this.numericUpDownSoundKeyIndex = new System.Windows.Forms.NumericUpDown();
			this.comboBoxSoundKey = new System.Windows.Forms.ComboBox();
			this.buttonSoundAdd = new System.Windows.Forms.Button();
			this.groupBoxSoundValue = new System.Windows.Forms.GroupBox();
			this.checkBoxSoundRadius = new System.Windows.Forms.CheckBox();
			this.checkBoxSoundDefinedPosition = new System.Windows.Forms.CheckBox();
			this.textBoxSoundRadius = new System.Windows.Forms.TextBox();
			this.groupBoxSoundPosition = new System.Windows.Forms.GroupBox();
			this.labelSoundPositionZUnit = new System.Windows.Forms.Label();
			this.textBoxSoundPositionZ = new System.Windows.Forms.TextBox();
			this.labelSoundPositionZ = new System.Windows.Forms.Label();
			this.labelSoundPositionYUnit = new System.Windows.Forms.Label();
			this.textBoxSoundPositionY = new System.Windows.Forms.TextBox();
			this.labelSoundPositionY = new System.Windows.Forms.Label();
			this.labelSoundPositionXUnit = new System.Windows.Forms.Label();
			this.textBoxSoundPositionX = new System.Windows.Forms.TextBox();
			this.labelSoundPositionX = new System.Windows.Forms.Label();
			this.labelSoundFileName = new System.Windows.Forms.Label();
			this.buttonSoundFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxSoundFileName = new System.Windows.Forms.TextBox();
			this.tabPageStatus = new System.Windows.Forms.TabPage();
			this.listViewStatus = new System.Windows.Forms.ListView();
			this.columnHeaderLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.menuStripStatus = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemError = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWarning = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemClear = new System.Windows.Forms.ToolStripMenuItem();
			this.panelCars = new System.Windows.Forms.Panel();
			this.treeViewCars = new System.Windows.Forms.TreeView();
			this.panelCarsNavi = new System.Windows.Forms.Panel();
			this.buttonCarsCopy = new System.Windows.Forms.Button();
			this.buttonCarsRemove = new System.Windows.Forms.Button();
			this.buttonCarsAdd = new System.Windows.Forms.Button();
			this.buttonCarsUp = new System.Windows.Forms.Button();
			this.buttonCarsDown = new System.Windows.Forms.Button();
			this.menuStripMenu = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorOpen = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorSave = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemImport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorExport = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripComboBoxLanguage = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripStatusLabelLanguage = new System.Windows.Forms.ToolStripStatusLabel();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.tabControlEditor.SuspendLayout();
			this.tabPageTrain.SuspendLayout();
			this.groupBoxDevice.SuspendLayout();
			this.groupBoxCab.SuspendLayout();
			this.groupBoxHandle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLocoBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverPowerNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotchReduceSteps)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotches)).BeginInit();
			this.tabPageCar.SuspendLayout();
			this.groupBoxPressure.SuspendLayout();
			this.groupBoxBrake.SuspendLayout();
			this.groupBoxMove.SuspendLayout();
			this.groupBoxDelay.SuspendLayout();
			this.groupBoxPerformance.SuspendLayout();
			this.groupBoxCarGeneral.SuspendLayout();
			this.groupBoxAxles.SuspendLayout();
			this.tabPageAccel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxAccel)).BeginInit();
			this.panelAccel.SuspendLayout();
			this.groupBoxPreview.SuspendLayout();
			this.groupBoxParameter.SuspendLayout();
			this.groupBoxNotch.SuspendLayout();
			this.tabPageMotor.SuspendLayout();
			this.panelMotorSound.SuspendLayout();
			this.toolStripContainerDrawArea.ContentPanel.SuspendLayout();
			this.toolStripContainerDrawArea.TopToolStripPanel.SuspendLayout();
			this.toolStripContainerDrawArea.SuspendLayout();
			this.toolStripToolBar.SuspendLayout();
			this.panelMotorSetting.SuspendLayout();
			this.groupBoxDirect.SuspendLayout();
			this.groupBoxPlay.SuspendLayout();
			this.groupBoxArea.SuspendLayout();
			this.groupBoxSource.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunIndex)).BeginInit();
			this.groupBoxView.SuspendLayout();
			this.statusStripStatus.SuspendLayout();
			this.menuStripMotor.SuspendLayout();
			this.tabPageCoupler.SuspendLayout();
			this.groupBoxCouplerGeneral.SuspendLayout();
			this.tabPagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPanel)).BeginInit();
			this.splitContainerPanel.Panel1.SuspendLayout();
			this.splitContainerPanel.Panel2.SuspendLayout();
			this.splitContainerPanel.SuspendLayout();
			this.panelPanelNavi.SuspendLayout();
			this.panelPanelNaviCmd.SuspendLayout();
			this.tabControlPanel.SuspendLayout();
			this.tabPageThis.SuspendLayout();
			this.groupBoxThisOrigin.SuspendLayout();
			this.groupBoxThisCenter.SuspendLayout();
			this.tabPageScreen.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenLayer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenNumber)).BeginInit();
			this.tabPagePilotLamp.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPilotLampLayer)).BeginInit();
			this.groupBoxPilotLampLocation.SuspendLayout();
			this.tabPageNeedle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNeedleLayer)).BeginInit();
			this.groupBoxNeedleOrigin.SuspendLayout();
			this.groupBoxNeedleLocation.SuspendLayout();
			this.tabPageDigitalNumber.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalNumberLayer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalNumberInterval)).BeginInit();
			this.groupBoxDigitalNumberLocation.SuspendLayout();
			this.tabPageDigitalGauge.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalGaugeLayer)).BeginInit();
			this.groupBoxDigitalGaugeLocation.SuspendLayout();
			this.tabPageLinearGauge.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeLayer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeWidth)).BeginInit();
			this.groupBoxLinearGaugeDirection.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeDirectionY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeDirectionX)).BeginInit();
			this.groupBoxLinearGaugeLocation.SuspendLayout();
			this.tabPageTimetable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimetableLayer)).BeginInit();
			this.groupBoxTimetableLocation.SuspendLayout();
			this.tabPageTouch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchCommandOption)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchSoundIndex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchJumpScreen)).BeginInit();
			this.groupBoxTouchSize.SuspendLayout();
			this.groupBoxTouchLocation.SuspendLayout();
			this.tabPageSound.SuspendLayout();
			this.panelSoundSetting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerSound)).BeginInit();
			this.splitContainerSound.Panel1.SuspendLayout();
			this.splitContainerSound.Panel2.SuspendLayout();
			this.splitContainerSound.SuspendLayout();
			this.panelSoundSettingEdit.SuspendLayout();
			this.groupBoxSoundEntry.SuspendLayout();
			this.groupBoxSoundKey.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundKeyIndex)).BeginInit();
			this.groupBoxSoundValue.SuspendLayout();
			this.groupBoxSoundPosition.SuspendLayout();
			this.tabPageStatus.SuspendLayout();
			this.menuStripStatus.SuspendLayout();
			this.panelCars.SuspendLayout();
			this.panelCarsNavi.SuspendLayout();
			this.menuStripMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// tabControlEditor
			// 
			this.tabControlEditor.AllowDrop = true;
			this.tabControlEditor.Controls.Add(this.tabPageTrain);
			this.tabControlEditor.Controls.Add(this.tabPageCar);
			this.tabControlEditor.Controls.Add(this.tabPageAccel);
			this.tabControlEditor.Controls.Add(this.tabPageMotor);
			this.tabControlEditor.Controls.Add(this.tabPageCoupler);
			this.tabControlEditor.Controls.Add(this.tabPagePanel);
			this.tabControlEditor.Controls.Add(this.tabPageSound);
			this.tabControlEditor.Controls.Add(this.tabPageStatus);
			this.tabControlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlEditor.Location = new System.Drawing.Point(200, 24);
			this.tabControlEditor.Name = "tabControlEditor";
			this.tabControlEditor.SelectedIndex = 0;
			this.tabControlEditor.Size = new System.Drawing.Size(800, 696);
			this.tabControlEditor.TabIndex = 9;
			// 
			// tabPageTrain
			// 
			this.tabPageTrain.Controls.Add(this.groupBoxDevice);
			this.tabPageTrain.Controls.Add(this.groupBoxCab);
			this.tabPageTrain.Controls.Add(this.groupBoxHandle);
			this.tabPageTrain.Location = new System.Drawing.Point(4, 22);
			this.tabPageTrain.Name = "tabPageTrain";
			this.tabPageTrain.Size = new System.Drawing.Size(792, 670);
			this.tabPageTrain.TabIndex = 3;
			this.tabPageTrain.Text = "Train settings";
			this.tabPageTrain.UseVisualStyleBackColor = true;
			// 
			// groupBoxDevice
			// 
			this.groupBoxDevice.Controls.Add(this.labelDoorMaxToleranceUnit);
			this.groupBoxDevice.Controls.Add(this.textBoxDoorMaxTolerance);
			this.groupBoxDevice.Controls.Add(this.labelDoorWidthUnit);
			this.groupBoxDevice.Controls.Add(this.textBoxDoorWidth);
			this.groupBoxDevice.Controls.Add(this.comboBoxDoorCloseMode);
			this.groupBoxDevice.Controls.Add(this.comboBoxDoorOpenMode);
			this.groupBoxDevice.Controls.Add(this.comboBoxPassAlarm);
			this.groupBoxDevice.Controls.Add(this.comboBoxReAdhesionDevice);
			this.groupBoxDevice.Controls.Add(this.comboBoxAtc);
			this.groupBoxDevice.Controls.Add(this.comboBoxAts);
			this.groupBoxDevice.Controls.Add(this.checkBoxHoldBrake);
			this.groupBoxDevice.Controls.Add(this.checkBoxEb);
			this.groupBoxDevice.Controls.Add(this.checkBoxConstSpeed);
			this.groupBoxDevice.Controls.Add(this.labelDoorMaxTolerance);
			this.groupBoxDevice.Controls.Add(this.labelDoorWidth);
			this.groupBoxDevice.Controls.Add(this.labelDoorCloseMode);
			this.groupBoxDevice.Controls.Add(this.labelDoorOpenMode);
			this.groupBoxDevice.Controls.Add(this.labelPassAlarm);
			this.groupBoxDevice.Controls.Add(this.labelReAdhesionDevice);
			this.groupBoxDevice.Controls.Add(this.labelHoldBrake);
			this.groupBoxDevice.Controls.Add(this.labelConstSpeed);
			this.groupBoxDevice.Controls.Add(this.labelEb);
			this.groupBoxDevice.Controls.Add(this.labelAtc);
			this.groupBoxDevice.Controls.Add(this.labelAts);
			this.groupBoxDevice.Location = new System.Drawing.Point(432, 8);
			this.groupBoxDevice.Name = "groupBoxDevice";
			this.groupBoxDevice.Size = new System.Drawing.Size(336, 288);
			this.groupBoxDevice.TabIndex = 2;
			this.groupBoxDevice.TabStop = false;
			this.groupBoxDevice.Text = "Device";
			// 
			// labelDoorMaxToleranceUnit
			// 
			this.labelDoorMaxToleranceUnit.Location = new System.Drawing.Point(304, 256);
			this.labelDoorMaxToleranceUnit.Name = "labelDoorMaxToleranceUnit";
			this.labelDoorMaxToleranceUnit.Size = new System.Drawing.Size(24, 16);
			this.labelDoorMaxToleranceUnit.TabIndex = 23;
			this.labelDoorMaxToleranceUnit.Text = "mm";
			this.labelDoorMaxToleranceUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxDoorMaxTolerance
			// 
			this.textBoxDoorMaxTolerance.Location = new System.Drawing.Point(128, 256);
			this.textBoxDoorMaxTolerance.Name = "textBoxDoorMaxTolerance";
			this.textBoxDoorMaxTolerance.Size = new System.Drawing.Size(168, 19);
			this.textBoxDoorMaxTolerance.TabIndex = 22;
			// 
			// labelDoorWidthUnit
			// 
			this.labelDoorWidthUnit.Location = new System.Drawing.Point(304, 232);
			this.labelDoorWidthUnit.Name = "labelDoorWidthUnit";
			this.labelDoorWidthUnit.Size = new System.Drawing.Size(24, 16);
			this.labelDoorWidthUnit.TabIndex = 21;
			this.labelDoorWidthUnit.Text = "mm";
			this.labelDoorWidthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxDoorWidth
			// 
			this.textBoxDoorWidth.Location = new System.Drawing.Point(128, 232);
			this.textBoxDoorWidth.Name = "textBoxDoorWidth";
			this.textBoxDoorWidth.Size = new System.Drawing.Size(168, 19);
			this.textBoxDoorWidth.TabIndex = 20;
			// 
			// comboBoxDoorCloseMode
			// 
			this.comboBoxDoorCloseMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDoorCloseMode.FormattingEnabled = true;
			this.comboBoxDoorCloseMode.Items.AddRange(new object[] {
            "Semi-automatic",
            "Automatic",
            "Manual"});
			this.comboBoxDoorCloseMode.Location = new System.Drawing.Point(128, 208);
			this.comboBoxDoorCloseMode.Name = "comboBoxDoorCloseMode";
			this.comboBoxDoorCloseMode.Size = new System.Drawing.Size(168, 20);
			this.comboBoxDoorCloseMode.TabIndex = 19;
			// 
			// comboBoxDoorOpenMode
			// 
			this.comboBoxDoorOpenMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDoorOpenMode.FormattingEnabled = true;
			this.comboBoxDoorOpenMode.Items.AddRange(new object[] {
            "Semi-automatic",
            "Automatic",
            "Manual"});
			this.comboBoxDoorOpenMode.Location = new System.Drawing.Point(128, 184);
			this.comboBoxDoorOpenMode.Name = "comboBoxDoorOpenMode";
			this.comboBoxDoorOpenMode.Size = new System.Drawing.Size(168, 20);
			this.comboBoxDoorOpenMode.TabIndex = 18;
			// 
			// comboBoxPassAlarm
			// 
			this.comboBoxPassAlarm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxPassAlarm.FormattingEnabled = true;
			this.comboBoxPassAlarm.Items.AddRange(new object[] {
            "None",
            "Single",
            "Looping"});
			this.comboBoxPassAlarm.Location = new System.Drawing.Point(128, 160);
			this.comboBoxPassAlarm.Name = "comboBoxPassAlarm";
			this.comboBoxPassAlarm.Size = new System.Drawing.Size(168, 20);
			this.comboBoxPassAlarm.TabIndex = 17;
			// 
			// comboBoxReAdhesionDevice
			// 
			this.comboBoxReAdhesionDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxReAdhesionDevice.FormattingEnabled = true;
			this.comboBoxReAdhesionDevice.Items.AddRange(new object[] {
            "None",
            "Type A (instant)",
            "Type B (slow)",
            "Type C (medium)",
            "Type D (fast)"});
			this.comboBoxReAdhesionDevice.Location = new System.Drawing.Point(128, 136);
			this.comboBoxReAdhesionDevice.Name = "comboBoxReAdhesionDevice";
			this.comboBoxReAdhesionDevice.Size = new System.Drawing.Size(168, 20);
			this.comboBoxReAdhesionDevice.TabIndex = 16;
			// 
			// comboBoxAtc
			// 
			this.comboBoxAtc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAtc.FormattingEnabled = true;
			this.comboBoxAtc.Items.AddRange(new object[] {
            "None",
            "Manual switching",
            "Automatic switching"});
			this.comboBoxAtc.Location = new System.Drawing.Point(128, 40);
			this.comboBoxAtc.Name = "comboBoxAtc";
			this.comboBoxAtc.Size = new System.Drawing.Size(168, 20);
			this.comboBoxAtc.TabIndex = 15;
			// 
			// comboBoxAts
			// 
			this.comboBoxAts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAts.FormattingEnabled = true;
			this.comboBoxAts.Items.AddRange(new object[] {
            "None",
            "ATS-SN",
            "ATS-SN / ATS-P"});
			this.comboBoxAts.Location = new System.Drawing.Point(128, 16);
			this.comboBoxAts.Name = "comboBoxAts";
			this.comboBoxAts.Size = new System.Drawing.Size(168, 20);
			this.comboBoxAts.TabIndex = 14;
			// 
			// checkBoxHoldBrake
			// 
			this.checkBoxHoldBrake.Location = new System.Drawing.Point(128, 112);
			this.checkBoxHoldBrake.Name = "checkBoxHoldBrake";
			this.checkBoxHoldBrake.Size = new System.Drawing.Size(168, 16);
			this.checkBoxHoldBrake.TabIndex = 13;
			this.checkBoxHoldBrake.UseVisualStyleBackColor = true;
			// 
			// checkBoxEb
			// 
			this.checkBoxEb.Location = new System.Drawing.Point(128, 64);
			this.checkBoxEb.Name = "checkBoxEb";
			this.checkBoxEb.Size = new System.Drawing.Size(168, 16);
			this.checkBoxEb.TabIndex = 12;
			this.checkBoxEb.UseVisualStyleBackColor = true;
			// 
			// checkBoxConstSpeed
			// 
			this.checkBoxConstSpeed.Location = new System.Drawing.Point(128, 88);
			this.checkBoxConstSpeed.Name = "checkBoxConstSpeed";
			this.checkBoxConstSpeed.Size = new System.Drawing.Size(168, 16);
			this.checkBoxConstSpeed.TabIndex = 11;
			this.checkBoxConstSpeed.UseVisualStyleBackColor = true;
			// 
			// labelDoorMaxTolerance
			// 
			this.labelDoorMaxTolerance.Location = new System.Drawing.Point(8, 256);
			this.labelDoorMaxTolerance.Name = "labelDoorMaxTolerance";
			this.labelDoorMaxTolerance.Size = new System.Drawing.Size(112, 16);
			this.labelDoorMaxTolerance.TabIndex = 10;
			this.labelDoorMaxTolerance.Text = "DoorMaxTolerance:";
			this.labelDoorMaxTolerance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDoorWidth
			// 
			this.labelDoorWidth.Location = new System.Drawing.Point(8, 232);
			this.labelDoorWidth.Name = "labelDoorWidth";
			this.labelDoorWidth.Size = new System.Drawing.Size(112, 16);
			this.labelDoorWidth.TabIndex = 9;
			this.labelDoorWidth.Text = "DoorWidth:";
			this.labelDoorWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDoorCloseMode
			// 
			this.labelDoorCloseMode.Location = new System.Drawing.Point(8, 208);
			this.labelDoorCloseMode.Name = "labelDoorCloseMode";
			this.labelDoorCloseMode.Size = new System.Drawing.Size(112, 16);
			this.labelDoorCloseMode.TabIndex = 8;
			this.labelDoorCloseMode.Text = "DoorCloseMode:";
			this.labelDoorCloseMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDoorOpenMode
			// 
			this.labelDoorOpenMode.Location = new System.Drawing.Point(8, 184);
			this.labelDoorOpenMode.Name = "labelDoorOpenMode";
			this.labelDoorOpenMode.Size = new System.Drawing.Size(112, 16);
			this.labelDoorOpenMode.TabIndex = 7;
			this.labelDoorOpenMode.Text = "DoorOpenMode:";
			this.labelDoorOpenMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPassAlarm
			// 
			this.labelPassAlarm.Location = new System.Drawing.Point(8, 160);
			this.labelPassAlarm.Name = "labelPassAlarm";
			this.labelPassAlarm.Size = new System.Drawing.Size(112, 16);
			this.labelPassAlarm.TabIndex = 6;
			this.labelPassAlarm.Text = "PassAlarm:";
			this.labelPassAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelReAdhesionDevice
			// 
			this.labelReAdhesionDevice.Location = new System.Drawing.Point(8, 136);
			this.labelReAdhesionDevice.Name = "labelReAdhesionDevice";
			this.labelReAdhesionDevice.Size = new System.Drawing.Size(112, 16);
			this.labelReAdhesionDevice.TabIndex = 5;
			this.labelReAdhesionDevice.Text = "ReAdhesionDevice:";
			this.labelReAdhesionDevice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelHoldBrake
			// 
			this.labelHoldBrake.Location = new System.Drawing.Point(8, 112);
			this.labelHoldBrake.Name = "labelHoldBrake";
			this.labelHoldBrake.Size = new System.Drawing.Size(112, 16);
			this.labelHoldBrake.TabIndex = 4;
			this.labelHoldBrake.Text = "HoldBrake:";
			this.labelHoldBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelConstSpeed
			// 
			this.labelConstSpeed.Location = new System.Drawing.Point(8, 88);
			this.labelConstSpeed.Name = "labelConstSpeed";
			this.labelConstSpeed.Size = new System.Drawing.Size(112, 16);
			this.labelConstSpeed.TabIndex = 3;
			this.labelConstSpeed.Text = "ConstSpeed:";
			this.labelConstSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelEb
			// 
			this.labelEb.Location = new System.Drawing.Point(8, 64);
			this.labelEb.Name = "labelEb";
			this.labelEb.Size = new System.Drawing.Size(112, 16);
			this.labelEb.TabIndex = 2;
			this.labelEb.Text = "Eb:";
			this.labelEb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAtc
			// 
			this.labelAtc.Location = new System.Drawing.Point(8, 40);
			this.labelAtc.Name = "labelAtc";
			this.labelAtc.Size = new System.Drawing.Size(112, 16);
			this.labelAtc.TabIndex = 1;
			this.labelAtc.Text = "Atc:";
			this.labelAtc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAts
			// 
			this.labelAts.Location = new System.Drawing.Point(8, 16);
			this.labelAts.Name = "labelAts";
			this.labelAts.Size = new System.Drawing.Size(112, 16);
			this.labelAts.TabIndex = 0;
			this.labelAts.Text = "Ats:";
			this.labelAts.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxCab
			// 
			this.groupBoxCab.Controls.Add(this.comboBoxDriverCar);
			this.groupBoxCab.Controls.Add(this.labelCabZUnit);
			this.groupBoxCab.Controls.Add(this.textBoxCabZ);
			this.groupBoxCab.Controls.Add(this.labelCabYUnit);
			this.groupBoxCab.Controls.Add(this.textBoxCabY);
			this.groupBoxCab.Controls.Add(this.labelCabXUnit);
			this.groupBoxCab.Controls.Add(this.textBoxCabX);
			this.groupBoxCab.Controls.Add(this.labelDriverCar);
			this.groupBoxCab.Controls.Add(this.labelCabZ);
			this.groupBoxCab.Controls.Add(this.labelCabY);
			this.groupBoxCab.Controls.Add(this.labelCabX);
			this.groupBoxCab.Location = new System.Drawing.Point(8, 256);
			this.groupBoxCab.Name = "groupBoxCab";
			this.groupBoxCab.Size = new System.Drawing.Size(416, 120);
			this.groupBoxCab.TabIndex = 1;
			this.groupBoxCab.TabStop = false;
			this.groupBoxCab.Text = "Cab";
			// 
			// comboBoxDriverCar
			// 
			this.comboBoxDriverCar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDriverCar.FormattingEnabled = true;
			this.comboBoxDriverCar.Location = new System.Drawing.Point(192, 88);
			this.comboBoxDriverCar.Name = "comboBoxDriverCar";
			this.comboBoxDriverCar.Size = new System.Drawing.Size(184, 20);
			this.comboBoxDriverCar.TabIndex = 10;
			// 
			// labelCabZUnit
			// 
			this.labelCabZUnit.Location = new System.Drawing.Point(384, 64);
			this.labelCabZUnit.Name = "labelCabZUnit";
			this.labelCabZUnit.Size = new System.Drawing.Size(24, 16);
			this.labelCabZUnit.TabIndex = 9;
			this.labelCabZUnit.Text = "mm";
			this.labelCabZUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxCabZ
			// 
			this.textBoxCabZ.Location = new System.Drawing.Point(192, 64);
			this.textBoxCabZ.Name = "textBoxCabZ";
			this.textBoxCabZ.Size = new System.Drawing.Size(184, 19);
			this.textBoxCabZ.TabIndex = 8;
			// 
			// labelCabYUnit
			// 
			this.labelCabYUnit.Location = new System.Drawing.Point(384, 40);
			this.labelCabYUnit.Name = "labelCabYUnit";
			this.labelCabYUnit.Size = new System.Drawing.Size(24, 16);
			this.labelCabYUnit.TabIndex = 7;
			this.labelCabYUnit.Text = "mm";
			this.labelCabYUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxCabY
			// 
			this.textBoxCabY.Location = new System.Drawing.Point(192, 40);
			this.textBoxCabY.Name = "textBoxCabY";
			this.textBoxCabY.Size = new System.Drawing.Size(184, 19);
			this.textBoxCabY.TabIndex = 6;
			// 
			// labelCabXUnit
			// 
			this.labelCabXUnit.Location = new System.Drawing.Point(384, 16);
			this.labelCabXUnit.Name = "labelCabXUnit";
			this.labelCabXUnit.Size = new System.Drawing.Size(24, 16);
			this.labelCabXUnit.TabIndex = 5;
			this.labelCabXUnit.Text = "mm";
			this.labelCabXUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxCabX
			// 
			this.textBoxCabX.Location = new System.Drawing.Point(192, 16);
			this.textBoxCabX.Name = "textBoxCabX";
			this.textBoxCabX.Size = new System.Drawing.Size(184, 19);
			this.textBoxCabX.TabIndex = 4;
			// 
			// labelDriverCar
			// 
			this.labelDriverCar.Location = new System.Drawing.Point(8, 88);
			this.labelDriverCar.Name = "labelDriverCar";
			this.labelDriverCar.Size = new System.Drawing.Size(176, 16);
			this.labelDriverCar.TabIndex = 3;
			this.labelDriverCar.Text = "DriverCar:";
			this.labelDriverCar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCabZ
			// 
			this.labelCabZ.Location = new System.Drawing.Point(8, 64);
			this.labelCabZ.Name = "labelCabZ";
			this.labelCabZ.Size = new System.Drawing.Size(176, 16);
			this.labelCabZ.TabIndex = 2;
			this.labelCabZ.Text = "Z:";
			this.labelCabZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCabY
			// 
			this.labelCabY.Location = new System.Drawing.Point(8, 40);
			this.labelCabY.Name = "labelCabY";
			this.labelCabY.Size = new System.Drawing.Size(176, 16);
			this.labelCabY.TabIndex = 1;
			this.labelCabY.Text = "Y:";
			this.labelCabY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCabX
			// 
			this.labelCabX.Location = new System.Drawing.Point(8, 16);
			this.labelCabX.Name = "labelCabX";
			this.labelCabX.Size = new System.Drawing.Size(176, 16);
			this.labelCabX.TabIndex = 0;
			this.labelCabX.Text = "X:";
			this.labelCabX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxHandle
			// 
			this.groupBoxHandle.Controls.Add(this.numericUpDownLocoBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.labelLocoBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.comboBoxLocoBrakeHandleType);
			this.groupBoxHandle.Controls.Add(this.labelLocoBrakeHandleType);
			this.groupBoxHandle.Controls.Add(this.comboBoxEbHandleBehaviour);
			this.groupBoxHandle.Controls.Add(this.labelEbHandleBehaviour);
			this.groupBoxHandle.Controls.Add(this.numericUpDownDriverBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.labelDriverBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.numericUpDownDriverPowerNotches);
			this.groupBoxHandle.Controls.Add(this.labelDriverPowerNotches);
			this.groupBoxHandle.Controls.Add(this.numericUpDownPowerNotchReduceSteps);
			this.groupBoxHandle.Controls.Add(this.labelPowerNotchReduceSteps);
			this.groupBoxHandle.Controls.Add(this.numericUpDownBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.labelBrakeNotches);
			this.groupBoxHandle.Controls.Add(this.numericUpDownPowerNotches);
			this.groupBoxHandle.Controls.Add(this.labelPowerNotches);
			this.groupBoxHandle.Controls.Add(this.comboBoxHandleType);
			this.groupBoxHandle.Controls.Add(this.labelHandleType);
			this.groupBoxHandle.Location = new System.Drawing.Point(8, 8);
			this.groupBoxHandle.Name = "groupBoxHandle";
			this.groupBoxHandle.Size = new System.Drawing.Size(416, 240);
			this.groupBoxHandle.TabIndex = 0;
			this.groupBoxHandle.TabStop = false;
			this.groupBoxHandle.Text = "Handle";
			// 
			// numericUpDownLocoBrakeNotches
			// 
			this.numericUpDownLocoBrakeNotches.Location = new System.Drawing.Point(192, 208);
			this.numericUpDownLocoBrakeNotches.Name = "numericUpDownLocoBrakeNotches";
			this.numericUpDownLocoBrakeNotches.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownLocoBrakeNotches.TabIndex = 17;
			// 
			// labelLocoBrakeNotches
			// 
			this.labelLocoBrakeNotches.Location = new System.Drawing.Point(8, 208);
			this.labelLocoBrakeNotches.Name = "labelLocoBrakeNotches";
			this.labelLocoBrakeNotches.Size = new System.Drawing.Size(176, 16);
			this.labelLocoBrakeNotches.TabIndex = 16;
			this.labelLocoBrakeNotches.Text = "LocoBrakeNotches:";
			this.labelLocoBrakeNotches.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxLocoBrakeHandleType
			// 
			this.comboBoxLocoBrakeHandleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxLocoBrakeHandleType.FormattingEnabled = true;
			this.comboBoxLocoBrakeHandleType.Items.AddRange(new object[] {
            "Combined",
            "Independant",
            "Blocking"});
			this.comboBoxLocoBrakeHandleType.Location = new System.Drawing.Point(192, 184);
			this.comboBoxLocoBrakeHandleType.Name = "comboBoxLocoBrakeHandleType";
			this.comboBoxLocoBrakeHandleType.Size = new System.Drawing.Size(216, 20);
			this.comboBoxLocoBrakeHandleType.TabIndex = 15;
			// 
			// labelLocoBrakeHandleType
			// 
			this.labelLocoBrakeHandleType.Location = new System.Drawing.Point(8, 184);
			this.labelLocoBrakeHandleType.Name = "labelLocoBrakeHandleType";
			this.labelLocoBrakeHandleType.Size = new System.Drawing.Size(176, 16);
			this.labelLocoBrakeHandleType.TabIndex = 14;
			this.labelLocoBrakeHandleType.Text = "LocoBrakeHandleType:";
			this.labelLocoBrakeHandleType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxEbHandleBehaviour
			// 
			this.comboBoxEbHandleBehaviour.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxEbHandleBehaviour.FormattingEnabled = true;
			this.comboBoxEbHandleBehaviour.Items.AddRange(new object[] {
            "No action",
            "Return power to neutral",
            "Return reverser to neutral",
            "Return power and reverser to neutral"});
			this.comboBoxEbHandleBehaviour.Location = new System.Drawing.Point(192, 160);
			this.comboBoxEbHandleBehaviour.Name = "comboBoxEbHandleBehaviour";
			this.comboBoxEbHandleBehaviour.Size = new System.Drawing.Size(216, 20);
			this.comboBoxEbHandleBehaviour.TabIndex = 13;
			// 
			// labelEbHandleBehaviour
			// 
			this.labelEbHandleBehaviour.Location = new System.Drawing.Point(8, 160);
			this.labelEbHandleBehaviour.Name = "labelEbHandleBehaviour";
			this.labelEbHandleBehaviour.Size = new System.Drawing.Size(176, 16);
			this.labelEbHandleBehaviour.TabIndex = 12;
			this.labelEbHandleBehaviour.Text = "EbHandleBehaviour:";
			this.labelEbHandleBehaviour.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownDriverBrakeNotches
			// 
			this.numericUpDownDriverBrakeNotches.Location = new System.Drawing.Point(192, 136);
			this.numericUpDownDriverBrakeNotches.Name = "numericUpDownDriverBrakeNotches";
			this.numericUpDownDriverBrakeNotches.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownDriverBrakeNotches.TabIndex = 11;
			// 
			// labelDriverBrakeNotches
			// 
			this.labelDriverBrakeNotches.Location = new System.Drawing.Point(8, 136);
			this.labelDriverBrakeNotches.Name = "labelDriverBrakeNotches";
			this.labelDriverBrakeNotches.Size = new System.Drawing.Size(176, 16);
			this.labelDriverBrakeNotches.TabIndex = 10;
			this.labelDriverBrakeNotches.Text = "DriverBrakeNotches:";
			this.labelDriverBrakeNotches.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownDriverPowerNotches
			// 
			this.numericUpDownDriverPowerNotches.Location = new System.Drawing.Point(192, 112);
			this.numericUpDownDriverPowerNotches.Name = "numericUpDownDriverPowerNotches";
			this.numericUpDownDriverPowerNotches.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownDriverPowerNotches.TabIndex = 9;
			// 
			// labelDriverPowerNotches
			// 
			this.labelDriverPowerNotches.Location = new System.Drawing.Point(8, 112);
			this.labelDriverPowerNotches.Name = "labelDriverPowerNotches";
			this.labelDriverPowerNotches.Size = new System.Drawing.Size(176, 16);
			this.labelDriverPowerNotches.TabIndex = 8;
			this.labelDriverPowerNotches.Text = "DriverPowerNotches:";
			this.labelDriverPowerNotches.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownPowerNotchReduceSteps
			// 
			this.numericUpDownPowerNotchReduceSteps.Location = new System.Drawing.Point(192, 88);
			this.numericUpDownPowerNotchReduceSteps.Name = "numericUpDownPowerNotchReduceSteps";
			this.numericUpDownPowerNotchReduceSteps.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownPowerNotchReduceSteps.TabIndex = 7;
			// 
			// labelPowerNotchReduceSteps
			// 
			this.labelPowerNotchReduceSteps.Location = new System.Drawing.Point(8, 88);
			this.labelPowerNotchReduceSteps.Name = "labelPowerNotchReduceSteps";
			this.labelPowerNotchReduceSteps.Size = new System.Drawing.Size(176, 16);
			this.labelPowerNotchReduceSteps.TabIndex = 6;
			this.labelPowerNotchReduceSteps.Text = "PowerNotchReduceSteps:";
			this.labelPowerNotchReduceSteps.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownBrakeNotches
			// 
			this.numericUpDownBrakeNotches.Location = new System.Drawing.Point(192, 64);
			this.numericUpDownBrakeNotches.Name = "numericUpDownBrakeNotches";
			this.numericUpDownBrakeNotches.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownBrakeNotches.TabIndex = 5;
			// 
			// labelBrakeNotches
			// 
			this.labelBrakeNotches.Location = new System.Drawing.Point(8, 64);
			this.labelBrakeNotches.Name = "labelBrakeNotches";
			this.labelBrakeNotches.Size = new System.Drawing.Size(176, 16);
			this.labelBrakeNotches.TabIndex = 4;
			this.labelBrakeNotches.Text = "BrakeNotches:";
			this.labelBrakeNotches.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownPowerNotches
			// 
			this.numericUpDownPowerNotches.Location = new System.Drawing.Point(192, 40);
			this.numericUpDownPowerNotches.Name = "numericUpDownPowerNotches";
			this.numericUpDownPowerNotches.Size = new System.Drawing.Size(216, 19);
			this.numericUpDownPowerNotches.TabIndex = 3;
			// 
			// labelPowerNotches
			// 
			this.labelPowerNotches.Location = new System.Drawing.Point(8, 40);
			this.labelPowerNotches.Name = "labelPowerNotches";
			this.labelPowerNotches.Size = new System.Drawing.Size(176, 16);
			this.labelPowerNotches.TabIndex = 2;
			this.labelPowerNotches.Text = "PowerNotches:";
			this.labelPowerNotches.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxHandleType
			// 
			this.comboBoxHandleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxHandleType.FormattingEnabled = true;
			this.comboBoxHandleType.Items.AddRange(new object[] {
            "Separated",
            "Combined"});
			this.comboBoxHandleType.Location = new System.Drawing.Point(192, 16);
			this.comboBoxHandleType.Name = "comboBoxHandleType";
			this.comboBoxHandleType.Size = new System.Drawing.Size(216, 20);
			this.comboBoxHandleType.TabIndex = 1;
			// 
			// labelHandleType
			// 
			this.labelHandleType.Location = new System.Drawing.Point(8, 16);
			this.labelHandleType.Name = "labelHandleType";
			this.labelHandleType.Size = new System.Drawing.Size(176, 16);
			this.labelHandleType.TabIndex = 0;
			this.labelHandleType.Text = "HandleType:";
			this.labelHandleType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageCar
			// 
			this.tabPageCar.Controls.Add(this.groupBoxPressure);
			this.tabPageCar.Controls.Add(this.groupBoxBrake);
			this.tabPageCar.Controls.Add(this.groupBoxMove);
			this.tabPageCar.Controls.Add(this.groupBoxDelay);
			this.tabPageCar.Controls.Add(this.groupBoxPerformance);
			this.tabPageCar.Controls.Add(this.groupBoxCarGeneral);
			this.tabPageCar.Location = new System.Drawing.Point(4, 22);
			this.tabPageCar.Name = "tabPageCar";
			this.tabPageCar.Size = new System.Drawing.Size(792, 670);
			this.tabPageCar.TabIndex = 4;
			this.tabPageCar.Text = "Car settings";
			this.tabPageCar.UseVisualStyleBackColor = true;
			// 
			// groupBoxPressure
			// 
			this.groupBoxPressure.Controls.Add(this.labelBrakePipeNormalPressureUnit);
			this.groupBoxPressure.Controls.Add(this.textBoxBrakePipeNormalPressure);
			this.groupBoxPressure.Controls.Add(this.labelBrakePipeNormalPressure);
			this.groupBoxPressure.Controls.Add(this.labelBrakeCylinderServiceMaximumPressureUnit);
			this.groupBoxPressure.Controls.Add(this.labelMainReservoirMaximumPressureUnit);
			this.groupBoxPressure.Controls.Add(this.labelMainReservoirMinimumPressureUnit);
			this.groupBoxPressure.Controls.Add(this.labelBrakeCylinderEmergencyMaximumPressureUnit);
			this.groupBoxPressure.Controls.Add(this.textBoxMainReservoirMaximumPressure);
			this.groupBoxPressure.Controls.Add(this.textBoxMainReservoirMinimumPressure);
			this.groupBoxPressure.Controls.Add(this.textBoxBrakeCylinderEmergencyMaximumPressure);
			this.groupBoxPressure.Controls.Add(this.textBoxBrakeCylinderServiceMaximumPressure);
			this.groupBoxPressure.Controls.Add(this.labelMainReservoirMaximumPressure);
			this.groupBoxPressure.Controls.Add(this.labelMainReservoirMinimumPressure);
			this.groupBoxPressure.Controls.Add(this.labelBrakeCylinderServiceMaximumPressure);
			this.groupBoxPressure.Controls.Add(this.labelBrakeCylinderEmergencyMaximumPressure);
			this.groupBoxPressure.Location = new System.Drawing.Point(288, 440);
			this.groupBoxPressure.Name = "groupBoxPressure";
			this.groupBoxPressure.Size = new System.Drawing.Size(448, 144);
			this.groupBoxPressure.TabIndex = 33;
			this.groupBoxPressure.TabStop = false;
			this.groupBoxPressure.Text = "Pressure";
			// 
			// labelBrakePipeNormalPressureUnit
			// 
			this.labelBrakePipeNormalPressureUnit.Location = new System.Drawing.Point(408, 112);
			this.labelBrakePipeNormalPressureUnit.Name = "labelBrakePipeNormalPressureUnit";
			this.labelBrakePipeNormalPressureUnit.Size = new System.Drawing.Size(24, 16);
			this.labelBrakePipeNormalPressureUnit.TabIndex = 39;
			this.labelBrakePipeNormalPressureUnit.Text = "kPa";
			this.labelBrakePipeNormalPressureUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxBrakePipeNormalPressure
			// 
			this.textBoxBrakePipeNormalPressure.Location = new System.Drawing.Point(248, 112);
			this.textBoxBrakePipeNormalPressure.Name = "textBoxBrakePipeNormalPressure";
			this.textBoxBrakePipeNormalPressure.Size = new System.Drawing.Size(152, 19);
			this.textBoxBrakePipeNormalPressure.TabIndex = 38;
			// 
			// labelBrakePipeNormalPressure
			// 
			this.labelBrakePipeNormalPressure.Location = new System.Drawing.Point(8, 112);
			this.labelBrakePipeNormalPressure.Name = "labelBrakePipeNormalPressure";
			this.labelBrakePipeNormalPressure.Size = new System.Drawing.Size(232, 16);
			this.labelBrakePipeNormalPressure.TabIndex = 37;
			this.labelBrakePipeNormalPressure.Text = "BrakePipeNormalPressure:";
			this.labelBrakePipeNormalPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderServiceMaximumPressureUnit
			// 
			this.labelBrakeCylinderServiceMaximumPressureUnit.Location = new System.Drawing.Point(408, 16);
			this.labelBrakeCylinderServiceMaximumPressureUnit.Name = "labelBrakeCylinderServiceMaximumPressureUnit";
			this.labelBrakeCylinderServiceMaximumPressureUnit.Size = new System.Drawing.Size(24, 16);
			this.labelBrakeCylinderServiceMaximumPressureUnit.TabIndex = 32;
			this.labelBrakeCylinderServiceMaximumPressureUnit.Text = "kPa";
			this.labelBrakeCylinderServiceMaximumPressureUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelMainReservoirMaximumPressureUnit
			// 
			this.labelMainReservoirMaximumPressureUnit.Location = new System.Drawing.Point(408, 88);
			this.labelMainReservoirMaximumPressureUnit.Name = "labelMainReservoirMaximumPressureUnit";
			this.labelMainReservoirMaximumPressureUnit.Size = new System.Drawing.Size(24, 16);
			this.labelMainReservoirMaximumPressureUnit.TabIndex = 36;
			this.labelMainReservoirMaximumPressureUnit.Text = "kPa";
			this.labelMainReservoirMaximumPressureUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelMainReservoirMinimumPressureUnit
			// 
			this.labelMainReservoirMinimumPressureUnit.Location = new System.Drawing.Point(408, 64);
			this.labelMainReservoirMinimumPressureUnit.Name = "labelMainReservoirMinimumPressureUnit";
			this.labelMainReservoirMinimumPressureUnit.Size = new System.Drawing.Size(24, 16);
			this.labelMainReservoirMinimumPressureUnit.TabIndex = 35;
			this.labelMainReservoirMinimumPressureUnit.Text = "kPa";
			this.labelMainReservoirMinimumPressureUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelBrakeCylinderEmergencyMaximumPressureUnit
			// 
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.Location = new System.Drawing.Point(408, 40);
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.Name = "labelBrakeCylinderEmergencyMaximumPressureUnit";
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.Size = new System.Drawing.Size(24, 16);
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.TabIndex = 34;
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.Text = "kPa";
			this.labelBrakeCylinderEmergencyMaximumPressureUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxMainReservoirMaximumPressure
			// 
			this.textBoxMainReservoirMaximumPressure.Location = new System.Drawing.Point(248, 88);
			this.textBoxMainReservoirMaximumPressure.Name = "textBoxMainReservoirMaximumPressure";
			this.textBoxMainReservoirMaximumPressure.Size = new System.Drawing.Size(152, 19);
			this.textBoxMainReservoirMaximumPressure.TabIndex = 34;
			// 
			// textBoxMainReservoirMinimumPressure
			// 
			this.textBoxMainReservoirMinimumPressure.Location = new System.Drawing.Point(248, 64);
			this.textBoxMainReservoirMinimumPressure.Name = "textBoxMainReservoirMinimumPressure";
			this.textBoxMainReservoirMinimumPressure.Size = new System.Drawing.Size(152, 19);
			this.textBoxMainReservoirMinimumPressure.TabIndex = 33;
			// 
			// textBoxBrakeCylinderEmergencyMaximumPressure
			// 
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Location = new System.Drawing.Point(248, 40);
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Name = "textBoxBrakeCylinderEmergencyMaximumPressure";
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Size = new System.Drawing.Size(152, 19);
			this.textBoxBrakeCylinderEmergencyMaximumPressure.TabIndex = 32;
			// 
			// textBoxBrakeCylinderServiceMaximumPressure
			// 
			this.textBoxBrakeCylinderServiceMaximumPressure.Location = new System.Drawing.Point(248, 16);
			this.textBoxBrakeCylinderServiceMaximumPressure.Name = "textBoxBrakeCylinderServiceMaximumPressure";
			this.textBoxBrakeCylinderServiceMaximumPressure.Size = new System.Drawing.Size(152, 19);
			this.textBoxBrakeCylinderServiceMaximumPressure.TabIndex = 31;
			// 
			// labelMainReservoirMaximumPressure
			// 
			this.labelMainReservoirMaximumPressure.Location = new System.Drawing.Point(8, 88);
			this.labelMainReservoirMaximumPressure.Name = "labelMainReservoirMaximumPressure";
			this.labelMainReservoirMaximumPressure.Size = new System.Drawing.Size(232, 16);
			this.labelMainReservoirMaximumPressure.TabIndex = 11;
			this.labelMainReservoirMaximumPressure.Text = "MainReservoirMaximumPressure:";
			this.labelMainReservoirMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMainReservoirMinimumPressure
			// 
			this.labelMainReservoirMinimumPressure.Location = new System.Drawing.Point(8, 64);
			this.labelMainReservoirMinimumPressure.Name = "labelMainReservoirMinimumPressure";
			this.labelMainReservoirMinimumPressure.Size = new System.Drawing.Size(232, 16);
			this.labelMainReservoirMinimumPressure.TabIndex = 10;
			this.labelMainReservoirMinimumPressure.Text = "MainReservoirMinimumPressure:";
			this.labelMainReservoirMinimumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderServiceMaximumPressure
			// 
			this.labelBrakeCylinderServiceMaximumPressure.Location = new System.Drawing.Point(8, 16);
			this.labelBrakeCylinderServiceMaximumPressure.Name = "labelBrakeCylinderServiceMaximumPressure";
			this.labelBrakeCylinderServiceMaximumPressure.Size = new System.Drawing.Size(232, 16);
			this.labelBrakeCylinderServiceMaximumPressure.TabIndex = 9;
			this.labelBrakeCylinderServiceMaximumPressure.Text = "BrakeCylinderServiceMaximumPressure:";
			this.labelBrakeCylinderServiceMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderEmergencyMaximumPressure
			// 
			this.labelBrakeCylinderEmergencyMaximumPressure.Location = new System.Drawing.Point(8, 40);
			this.labelBrakeCylinderEmergencyMaximumPressure.Name = "labelBrakeCylinderEmergencyMaximumPressure";
			this.labelBrakeCylinderEmergencyMaximumPressure.Size = new System.Drawing.Size(232, 16);
			this.labelBrakeCylinderEmergencyMaximumPressure.TabIndex = 8;
			this.labelBrakeCylinderEmergencyMaximumPressure.Text = "BrakeCylinderEmergencyMaximumPressure:";
			this.labelBrakeCylinderEmergencyMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxBrake
			// 
			this.groupBoxBrake.Controls.Add(this.textBoxBrakeControlSpeed);
			this.groupBoxBrake.Controls.Add(this.labelBrakeControlSpeedUnit);
			this.groupBoxBrake.Controls.Add(this.comboBoxBrakeControlSystem);
			this.groupBoxBrake.Controls.Add(this.comboBoxLocoBrakeType);
			this.groupBoxBrake.Controls.Add(this.comboBoxBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelLocoBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelBrakeControlSpeed);
			this.groupBoxBrake.Controls.Add(this.labelBrakeControlSystem);
			this.groupBoxBrake.Location = new System.Drawing.Point(288, 312);
			this.groupBoxBrake.Name = "groupBoxBrake";
			this.groupBoxBrake.Size = new System.Drawing.Size(448, 120);
			this.groupBoxBrake.TabIndex = 4;
			this.groupBoxBrake.TabStop = false;
			this.groupBoxBrake.Text = "Brake";
			// 
			// textBoxBrakeControlSpeed
			// 
			this.textBoxBrakeControlSpeed.Location = new System.Drawing.Point(136, 88);
			this.textBoxBrakeControlSpeed.Name = "textBoxBrakeControlSpeed";
			this.textBoxBrakeControlSpeed.Size = new System.Drawing.Size(264, 19);
			this.textBoxBrakeControlSpeed.TabIndex = 31;
			// 
			// labelBrakeControlSpeedUnit
			// 
			this.labelBrakeControlSpeedUnit.Location = new System.Drawing.Point(408, 88);
			this.labelBrakeControlSpeedUnit.Name = "labelBrakeControlSpeedUnit";
			this.labelBrakeControlSpeedUnit.Size = new System.Drawing.Size(32, 16);
			this.labelBrakeControlSpeedUnit.TabIndex = 32;
			this.labelBrakeControlSpeedUnit.Text = "km/h";
			this.labelBrakeControlSpeedUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxBrakeControlSystem
			// 
			this.comboBoxBrakeControlSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeControlSystem.FormattingEnabled = true;
			this.comboBoxBrakeControlSystem.Items.AddRange(new object[] {
            "None",
            "Closing electromagnetic valve",
            "Delay-including control"});
			this.comboBoxBrakeControlSystem.Location = new System.Drawing.Point(136, 64);
			this.comboBoxBrakeControlSystem.Name = "comboBoxBrakeControlSystem";
			this.comboBoxBrakeControlSystem.Size = new System.Drawing.Size(264, 20);
			this.comboBoxBrakeControlSystem.TabIndex = 17;
			// 
			// comboBoxLocoBrakeType
			// 
			this.comboBoxLocoBrakeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxLocoBrakeType.FormattingEnabled = true;
			this.comboBoxLocoBrakeType.Items.AddRange(new object[] {
            "Not fitted",
            "Notched air brake",
            "Air brake with partial release"});
			this.comboBoxLocoBrakeType.Location = new System.Drawing.Point(136, 40);
			this.comboBoxLocoBrakeType.Name = "comboBoxLocoBrakeType";
			this.comboBoxLocoBrakeType.Size = new System.Drawing.Size(264, 20);
			this.comboBoxLocoBrakeType.TabIndex = 16;
			// 
			// comboBoxBrakeType
			// 
			this.comboBoxBrakeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeType.FormattingEnabled = true;
			this.comboBoxBrakeType.Items.AddRange(new object[] {
            "Electromagnetic straight air brake",
            "Electro-pneumatic air brake without brake pipe",
            "Air brake with partial release feature"});
			this.comboBoxBrakeType.Location = new System.Drawing.Point(136, 16);
			this.comboBoxBrakeType.Name = "comboBoxBrakeType";
			this.comboBoxBrakeType.Size = new System.Drawing.Size(264, 20);
			this.comboBoxBrakeType.TabIndex = 15;
			// 
			// labelLocoBrakeType
			// 
			this.labelLocoBrakeType.Location = new System.Drawing.Point(8, 40);
			this.labelLocoBrakeType.Name = "labelLocoBrakeType";
			this.labelLocoBrakeType.Size = new System.Drawing.Size(120, 16);
			this.labelLocoBrakeType.TabIndex = 8;
			this.labelLocoBrakeType.Text = "LocoBrakeType:";
			this.labelLocoBrakeType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeType
			// 
			this.labelBrakeType.Location = new System.Drawing.Point(8, 16);
			this.labelBrakeType.Name = "labelBrakeType";
			this.labelBrakeType.Size = new System.Drawing.Size(120, 16);
			this.labelBrakeType.TabIndex = 7;
			this.labelBrakeType.Text = "BrakeType:";
			this.labelBrakeType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeControlSpeed
			// 
			this.labelBrakeControlSpeed.Location = new System.Drawing.Point(8, 88);
			this.labelBrakeControlSpeed.Name = "labelBrakeControlSpeed";
			this.labelBrakeControlSpeed.Size = new System.Drawing.Size(120, 16);
			this.labelBrakeControlSpeed.TabIndex = 6;
			this.labelBrakeControlSpeed.Text = "BrakeControlSpeed:";
			this.labelBrakeControlSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeControlSystem
			// 
			this.labelBrakeControlSystem.Location = new System.Drawing.Point(8, 64);
			this.labelBrakeControlSystem.Name = "labelBrakeControlSystem";
			this.labelBrakeControlSystem.Size = new System.Drawing.Size(120, 16);
			this.labelBrakeControlSystem.TabIndex = 5;
			this.labelBrakeControlSystem.Text = "BrakeControlSystem:";
			this.labelBrakeControlSystem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxMove
			// 
			this.groupBoxMove.Controls.Add(this.labelBrakeCylinderDownUnit);
			this.groupBoxMove.Controls.Add(this.textBoxBrakeCylinderDown);
			this.groupBoxMove.Controls.Add(this.labelBrakeCylinderUpUnit);
			this.groupBoxMove.Controls.Add(this.textBoxBrakeCylinderUp);
			this.groupBoxMove.Controls.Add(this.labelJerkBrakeDownUnit);
			this.groupBoxMove.Controls.Add(this.textBoxJerkBrakeDown);
			this.groupBoxMove.Controls.Add(this.labelJerkBrakeUpUnit);
			this.groupBoxMove.Controls.Add(this.textBoxJerkBrakeUp);
			this.groupBoxMove.Controls.Add(this.labelJerkPowerDownUnit);
			this.groupBoxMove.Controls.Add(this.textBoxJerkPowerDown);
			this.groupBoxMove.Controls.Add(this.labelJerkPowerUpUnit);
			this.groupBoxMove.Controls.Add(this.textBoxJerkPowerUp);
			this.groupBoxMove.Controls.Add(this.labelJerkPowerUp);
			this.groupBoxMove.Controls.Add(this.labelJerkPowerDown);
			this.groupBoxMove.Controls.Add(this.labelJerkBrakeUp);
			this.groupBoxMove.Controls.Add(this.labelJerkBrakeDown);
			this.groupBoxMove.Controls.Add(this.labelBrakeCylinderUp);
			this.groupBoxMove.Controls.Add(this.labelBrakeCylinderDown);
			this.groupBoxMove.Location = new System.Drawing.Point(288, 136);
			this.groupBoxMove.Name = "groupBoxMove";
			this.groupBoxMove.Size = new System.Drawing.Size(448, 168);
			this.groupBoxMove.TabIndex = 3;
			this.groupBoxMove.TabStop = false;
			this.groupBoxMove.Text = "Move";
			// 
			// labelBrakeCylinderDownUnit
			// 
			this.labelBrakeCylinderDownUnit.Location = new System.Drawing.Point(376, 136);
			this.labelBrakeCylinderDownUnit.Name = "labelBrakeCylinderDownUnit";
			this.labelBrakeCylinderDownUnit.Size = new System.Drawing.Size(64, 16);
			this.labelBrakeCylinderDownUnit.TabIndex = 31;
			this.labelBrakeCylinderDownUnit.Text = "kPa/s";
			this.labelBrakeCylinderDownUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxBrakeCylinderDown
			// 
			this.textBoxBrakeCylinderDown.Location = new System.Drawing.Point(184, 136);
			this.textBoxBrakeCylinderDown.Name = "textBoxBrakeCylinderDown";
			this.textBoxBrakeCylinderDown.Size = new System.Drawing.Size(184, 19);
			this.textBoxBrakeCylinderDown.TabIndex = 30;
			// 
			// labelBrakeCylinderUpUnit
			// 
			this.labelBrakeCylinderUpUnit.Location = new System.Drawing.Point(376, 112);
			this.labelBrakeCylinderUpUnit.Name = "labelBrakeCylinderUpUnit";
			this.labelBrakeCylinderUpUnit.Size = new System.Drawing.Size(64, 16);
			this.labelBrakeCylinderUpUnit.TabIndex = 29;
			this.labelBrakeCylinderUpUnit.Text = "kPa/s";
			this.labelBrakeCylinderUpUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxBrakeCylinderUp
			// 
			this.textBoxBrakeCylinderUp.Location = new System.Drawing.Point(184, 112);
			this.textBoxBrakeCylinderUp.Name = "textBoxBrakeCylinderUp";
			this.textBoxBrakeCylinderUp.Size = new System.Drawing.Size(184, 19);
			this.textBoxBrakeCylinderUp.TabIndex = 28;
			// 
			// labelJerkBrakeDownUnit
			// 
			this.labelJerkBrakeDownUnit.Location = new System.Drawing.Point(376, 88);
			this.labelJerkBrakeDownUnit.Name = "labelJerkBrakeDownUnit";
			this.labelJerkBrakeDownUnit.Size = new System.Drawing.Size(64, 16);
			this.labelJerkBrakeDownUnit.TabIndex = 27;
			this.labelJerkBrakeDownUnit.Text = "1/100 m/s³";
			this.labelJerkBrakeDownUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxJerkBrakeDown
			// 
			this.textBoxJerkBrakeDown.Location = new System.Drawing.Point(184, 88);
			this.textBoxJerkBrakeDown.Name = "textBoxJerkBrakeDown";
			this.textBoxJerkBrakeDown.Size = new System.Drawing.Size(184, 19);
			this.textBoxJerkBrakeDown.TabIndex = 26;
			// 
			// labelJerkBrakeUpUnit
			// 
			this.labelJerkBrakeUpUnit.Location = new System.Drawing.Point(376, 64);
			this.labelJerkBrakeUpUnit.Name = "labelJerkBrakeUpUnit";
			this.labelJerkBrakeUpUnit.Size = new System.Drawing.Size(64, 16);
			this.labelJerkBrakeUpUnit.TabIndex = 25;
			this.labelJerkBrakeUpUnit.Text = "1/100 m/s³";
			this.labelJerkBrakeUpUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxJerkBrakeUp
			// 
			this.textBoxJerkBrakeUp.Location = new System.Drawing.Point(184, 64);
			this.textBoxJerkBrakeUp.Name = "textBoxJerkBrakeUp";
			this.textBoxJerkBrakeUp.Size = new System.Drawing.Size(184, 19);
			this.textBoxJerkBrakeUp.TabIndex = 24;
			// 
			// labelJerkPowerDownUnit
			// 
			this.labelJerkPowerDownUnit.Location = new System.Drawing.Point(376, 40);
			this.labelJerkPowerDownUnit.Name = "labelJerkPowerDownUnit";
			this.labelJerkPowerDownUnit.Size = new System.Drawing.Size(64, 16);
			this.labelJerkPowerDownUnit.TabIndex = 23;
			this.labelJerkPowerDownUnit.Text = "1/100 m/s³";
			this.labelJerkPowerDownUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxJerkPowerDown
			// 
			this.textBoxJerkPowerDown.Location = new System.Drawing.Point(184, 40);
			this.textBoxJerkPowerDown.Name = "textBoxJerkPowerDown";
			this.textBoxJerkPowerDown.Size = new System.Drawing.Size(184, 19);
			this.textBoxJerkPowerDown.TabIndex = 22;
			// 
			// labelJerkPowerUpUnit
			// 
			this.labelJerkPowerUpUnit.Location = new System.Drawing.Point(376, 16);
			this.labelJerkPowerUpUnit.Name = "labelJerkPowerUpUnit";
			this.labelJerkPowerUpUnit.Size = new System.Drawing.Size(64, 16);
			this.labelJerkPowerUpUnit.TabIndex = 21;
			this.labelJerkPowerUpUnit.Text = "1/100 m/s³";
			this.labelJerkPowerUpUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxJerkPowerUp
			// 
			this.textBoxJerkPowerUp.Location = new System.Drawing.Point(184, 16);
			this.textBoxJerkPowerUp.Name = "textBoxJerkPowerUp";
			this.textBoxJerkPowerUp.Size = new System.Drawing.Size(184, 19);
			this.textBoxJerkPowerUp.TabIndex = 20;
			// 
			// labelJerkPowerUp
			// 
			this.labelJerkPowerUp.Location = new System.Drawing.Point(8, 16);
			this.labelJerkPowerUp.Name = "labelJerkPowerUp";
			this.labelJerkPowerUp.Size = new System.Drawing.Size(168, 16);
			this.labelJerkPowerUp.TabIndex = 6;
			this.labelJerkPowerUp.Text = "JerkPowerUp:";
			this.labelJerkPowerUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelJerkPowerDown
			// 
			this.labelJerkPowerDown.Location = new System.Drawing.Point(8, 40);
			this.labelJerkPowerDown.Name = "labelJerkPowerDown";
			this.labelJerkPowerDown.Size = new System.Drawing.Size(168, 16);
			this.labelJerkPowerDown.TabIndex = 5;
			this.labelJerkPowerDown.Text = "JerkPowerDown:";
			this.labelJerkPowerDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelJerkBrakeUp
			// 
			this.labelJerkBrakeUp.Location = new System.Drawing.Point(8, 64);
			this.labelJerkBrakeUp.Name = "labelJerkBrakeUp";
			this.labelJerkBrakeUp.Size = new System.Drawing.Size(168, 16);
			this.labelJerkBrakeUp.TabIndex = 4;
			this.labelJerkBrakeUp.Text = "JerkBrakeUp:";
			this.labelJerkBrakeUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelJerkBrakeDown
			// 
			this.labelJerkBrakeDown.Location = new System.Drawing.Point(8, 88);
			this.labelJerkBrakeDown.Name = "labelJerkBrakeDown";
			this.labelJerkBrakeDown.Size = new System.Drawing.Size(168, 16);
			this.labelJerkBrakeDown.TabIndex = 3;
			this.labelJerkBrakeDown.Text = "JerkBrakeDown:";
			this.labelJerkBrakeDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderUp
			// 
			this.labelBrakeCylinderUp.Location = new System.Drawing.Point(8, 112);
			this.labelBrakeCylinderUp.Name = "labelBrakeCylinderUp";
			this.labelBrakeCylinderUp.Size = new System.Drawing.Size(168, 16);
			this.labelBrakeCylinderUp.TabIndex = 2;
			this.labelBrakeCylinderUp.Text = "BrakeCylinderUp:";
			this.labelBrakeCylinderUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderDown
			// 
			this.labelBrakeCylinderDown.Location = new System.Drawing.Point(8, 136);
			this.labelBrakeCylinderDown.Name = "labelBrakeCylinderDown";
			this.labelBrakeCylinderDown.Size = new System.Drawing.Size(168, 16);
			this.labelBrakeCylinderDown.TabIndex = 1;
			this.labelBrakeCylinderDown.Text = "BrakeCylinderDown:";
			this.labelBrakeCylinderDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxDelay
			// 
			this.groupBoxDelay.Controls.Add(this.buttonDelayLocoBrakeSet);
			this.groupBoxDelay.Controls.Add(this.buttonDelayBrakeSet);
			this.groupBoxDelay.Controls.Add(this.buttonDelayPowerSet);
			this.groupBoxDelay.Controls.Add(this.labelDelayLocoBrake);
			this.groupBoxDelay.Controls.Add(this.labelDelayBrake);
			this.groupBoxDelay.Controls.Add(this.labelDelayPower);
			this.groupBoxDelay.Location = new System.Drawing.Point(8, 480);
			this.groupBoxDelay.Name = "groupBoxDelay";
			this.groupBoxDelay.Size = new System.Drawing.Size(272, 96);
			this.groupBoxDelay.TabIndex = 2;
			this.groupBoxDelay.TabStop = false;
			this.groupBoxDelay.Text = "Delay";
			// 
			// buttonDelayLocoBrakeSet
			// 
			this.buttonDelayLocoBrakeSet.Location = new System.Drawing.Point(160, 64);
			this.buttonDelayLocoBrakeSet.Name = "buttonDelayLocoBrakeSet";
			this.buttonDelayLocoBrakeSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDelayLocoBrakeSet.TabIndex = 37;
			this.buttonDelayLocoBrakeSet.Text = "Set...";
			this.buttonDelayLocoBrakeSet.UseVisualStyleBackColor = true;
			this.buttonDelayLocoBrakeSet.Click += new System.EventHandler(this.ButtonDelayLocoBrakeSet_Click);
			// 
			// buttonDelayBrakeSet
			// 
			this.buttonDelayBrakeSet.Location = new System.Drawing.Point(160, 40);
			this.buttonDelayBrakeSet.Name = "buttonDelayBrakeSet";
			this.buttonDelayBrakeSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDelayBrakeSet.TabIndex = 35;
			this.buttonDelayBrakeSet.Text = "Set...";
			this.buttonDelayBrakeSet.UseVisualStyleBackColor = true;
			this.buttonDelayBrakeSet.Click += new System.EventHandler(this.ButtonDelayBrakeSet_Click);
			// 
			// buttonDelayPowerSet
			// 
			this.buttonDelayPowerSet.Location = new System.Drawing.Point(160, 16);
			this.buttonDelayPowerSet.Name = "buttonDelayPowerSet";
			this.buttonDelayPowerSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDelayPowerSet.TabIndex = 33;
			this.buttonDelayPowerSet.Text = "Set...";
			this.buttonDelayPowerSet.UseVisualStyleBackColor = true;
			this.buttonDelayPowerSet.Click += new System.EventHandler(this.ButtonDelayPowerSet_Click);
			// 
			// labelDelayLocoBrake
			// 
			this.labelDelayLocoBrake.Location = new System.Drawing.Point(8, 64);
			this.labelDelayLocoBrake.Name = "labelDelayLocoBrake";
			this.labelDelayLocoBrake.Size = new System.Drawing.Size(144, 16);
			this.labelDelayLocoBrake.TabIndex = 6;
			this.labelDelayLocoBrake.Text = "LocoBrake:";
			this.labelDelayLocoBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDelayBrake
			// 
			this.labelDelayBrake.Location = new System.Drawing.Point(8, 40);
			this.labelDelayBrake.Name = "labelDelayBrake";
			this.labelDelayBrake.Size = new System.Drawing.Size(144, 16);
			this.labelDelayBrake.TabIndex = 4;
			this.labelDelayBrake.Text = "Brake:";
			this.labelDelayBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDelayPower
			// 
			this.labelDelayPower.Location = new System.Drawing.Point(8, 16);
			this.labelDelayPower.Name = "labelDelayPower";
			this.labelDelayPower.Size = new System.Drawing.Size(144, 16);
			this.labelDelayPower.TabIndex = 2;
			this.labelDelayPower.Text = "Power:";
			this.labelDelayPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxPerformance
			// 
			this.groupBoxPerformance.Controls.Add(this.textBoxAerodynamicDragCoefficient);
			this.groupBoxPerformance.Controls.Add(this.textBoxCoefficientOfRollingResistance);
			this.groupBoxPerformance.Controls.Add(this.textBoxCoefficientOfStaticFriction);
			this.groupBoxPerformance.Controls.Add(this.labelDecelerationUnit);
			this.groupBoxPerformance.Controls.Add(this.textBoxDeceleration);
			this.groupBoxPerformance.Controls.Add(this.labelAerodynamicDragCoefficient);
			this.groupBoxPerformance.Controls.Add(this.labelCoefficientOfStaticFriction);
			this.groupBoxPerformance.Controls.Add(this.labelDeceleration);
			this.groupBoxPerformance.Controls.Add(this.labelCoefficientOfRollingResistance);
			this.groupBoxPerformance.Location = new System.Drawing.Point(288, 8);
			this.groupBoxPerformance.Name = "groupBoxPerformance";
			this.groupBoxPerformance.Size = new System.Drawing.Size(448, 120);
			this.groupBoxPerformance.TabIndex = 1;
			this.groupBoxPerformance.TabStop = false;
			this.groupBoxPerformance.Text = "Performance";
			// 
			// textBoxAerodynamicDragCoefficient
			// 
			this.textBoxAerodynamicDragCoefficient.Location = new System.Drawing.Point(184, 88);
			this.textBoxAerodynamicDragCoefficient.Name = "textBoxAerodynamicDragCoefficient";
			this.textBoxAerodynamicDragCoefficient.Size = new System.Drawing.Size(184, 19);
			this.textBoxAerodynamicDragCoefficient.TabIndex = 25;
			// 
			// textBoxCoefficientOfRollingResistance
			// 
			this.textBoxCoefficientOfRollingResistance.Location = new System.Drawing.Point(184, 64);
			this.textBoxCoefficientOfRollingResistance.Name = "textBoxCoefficientOfRollingResistance";
			this.textBoxCoefficientOfRollingResistance.Size = new System.Drawing.Size(184, 19);
			this.textBoxCoefficientOfRollingResistance.TabIndex = 24;
			// 
			// textBoxCoefficientOfStaticFriction
			// 
			this.textBoxCoefficientOfStaticFriction.Location = new System.Drawing.Point(184, 40);
			this.textBoxCoefficientOfStaticFriction.Name = "textBoxCoefficientOfStaticFriction";
			this.textBoxCoefficientOfStaticFriction.Size = new System.Drawing.Size(184, 19);
			this.textBoxCoefficientOfStaticFriction.TabIndex = 23;
			// 
			// labelDecelerationUnit
			// 
			this.labelDecelerationUnit.Location = new System.Drawing.Point(376, 16);
			this.labelDecelerationUnit.Name = "labelDecelerationUnit";
			this.labelDecelerationUnit.Size = new System.Drawing.Size(48, 16);
			this.labelDecelerationUnit.TabIndex = 22;
			this.labelDecelerationUnit.Text = "km/h/s";
			this.labelDecelerationUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxDeceleration
			// 
			this.textBoxDeceleration.Location = new System.Drawing.Point(184, 16);
			this.textBoxDeceleration.Name = "textBoxDeceleration";
			this.textBoxDeceleration.Size = new System.Drawing.Size(184, 19);
			this.textBoxDeceleration.TabIndex = 20;
			// 
			// labelAerodynamicDragCoefficient
			// 
			this.labelAerodynamicDragCoefficient.Location = new System.Drawing.Point(8, 88);
			this.labelAerodynamicDragCoefficient.Name = "labelAerodynamicDragCoefficient";
			this.labelAerodynamicDragCoefficient.Size = new System.Drawing.Size(168, 16);
			this.labelAerodynamicDragCoefficient.TabIndex = 5;
			this.labelAerodynamicDragCoefficient.Text = "AerodynamicDragCoefficient:";
			this.labelAerodynamicDragCoefficient.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCoefficientOfStaticFriction
			// 
			this.labelCoefficientOfStaticFriction.Location = new System.Drawing.Point(8, 40);
			this.labelCoefficientOfStaticFriction.Name = "labelCoefficientOfStaticFriction";
			this.labelCoefficientOfStaticFriction.Size = new System.Drawing.Size(168, 16);
			this.labelCoefficientOfStaticFriction.TabIndex = 4;
			this.labelCoefficientOfStaticFriction.Text = "CoefficientOfStaticFriction:";
			this.labelCoefficientOfStaticFriction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDeceleration
			// 
			this.labelDeceleration.Location = new System.Drawing.Point(8, 16);
			this.labelDeceleration.Name = "labelDeceleration";
			this.labelDeceleration.Size = new System.Drawing.Size(168, 16);
			this.labelDeceleration.TabIndex = 3;
			this.labelDeceleration.Text = "Deceleration:";
			this.labelDeceleration.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCoefficientOfRollingResistance
			// 
			this.labelCoefficientOfRollingResistance.Location = new System.Drawing.Point(8, 64);
			this.labelCoefficientOfRollingResistance.Name = "labelCoefficientOfRollingResistance";
			this.labelCoefficientOfRollingResistance.Size = new System.Drawing.Size(168, 16);
			this.labelCoefficientOfRollingResistance.TabIndex = 2;
			this.labelCoefficientOfRollingResistance.Text = "CoefficientOfRollingResistance:";
			this.labelCoefficientOfRollingResistance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxCarGeneral
			// 
			this.groupBoxCarGeneral.Controls.Add(this.labelUnexposedFrontalAreaUnit);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxUnexposedFrontalArea);
			this.groupBoxCarGeneral.Controls.Add(this.labelUnexposedFrontalArea);
			this.groupBoxCarGeneral.Controls.Add(this.labelRearBogie);
			this.groupBoxCarGeneral.Controls.Add(this.labelFrontBogie);
			this.groupBoxCarGeneral.Controls.Add(this.buttonRearBogieSet);
			this.groupBoxCarGeneral.Controls.Add(this.buttonFrontBogieSet);
			this.groupBoxCarGeneral.Controls.Add(this.checkBoxReversed);
			this.groupBoxCarGeneral.Controls.Add(this.checkBoxLoadingSway);
			this.groupBoxCarGeneral.Controls.Add(this.checkBoxDefinedAxles);
			this.groupBoxCarGeneral.Controls.Add(this.checkBoxIsMotorCar);
			this.groupBoxCarGeneral.Controls.Add(this.buttonObjectOpen);
			this.groupBoxCarGeneral.Controls.Add(this.labelExposedFrontalAreaUnit);
			this.groupBoxCarGeneral.Controls.Add(this.labelCenterOfMassHeightUnit);
			this.groupBoxCarGeneral.Controls.Add(this.labelLoadingSway);
			this.groupBoxCarGeneral.Controls.Add(this.labelHeightUnit);
			this.groupBoxCarGeneral.Controls.Add(this.labelWidthUnit);
			this.groupBoxCarGeneral.Controls.Add(this.labelLengthUnit);
			this.groupBoxCarGeneral.Controls.Add(this.labelMassUnit);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxMass);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxLength);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxWidth);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxHeight);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxCenterOfMassHeight);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxExposedFrontalArea);
			this.groupBoxCarGeneral.Controls.Add(this.textBoxObject);
			this.groupBoxCarGeneral.Controls.Add(this.labelObject);
			this.groupBoxCarGeneral.Controls.Add(this.labelReversed);
			this.groupBoxCarGeneral.Controls.Add(this.groupBoxAxles);
			this.groupBoxCarGeneral.Controls.Add(this.labelDefinedAxles);
			this.groupBoxCarGeneral.Controls.Add(this.labelExposedFrontalArea);
			this.groupBoxCarGeneral.Controls.Add(this.labelCenterOfMassHeight);
			this.groupBoxCarGeneral.Controls.Add(this.labelHeight);
			this.groupBoxCarGeneral.Controls.Add(this.labelWidth);
			this.groupBoxCarGeneral.Controls.Add(this.labelLength);
			this.groupBoxCarGeneral.Controls.Add(this.labelMass);
			this.groupBoxCarGeneral.Controls.Add(this.labelIsMotorCar);
			this.groupBoxCarGeneral.Location = new System.Drawing.Point(8, 8);
			this.groupBoxCarGeneral.Name = "groupBoxCarGeneral";
			this.groupBoxCarGeneral.Size = new System.Drawing.Size(272, 464);
			this.groupBoxCarGeneral.TabIndex = 0;
			this.groupBoxCarGeneral.TabStop = false;
			this.groupBoxCarGeneral.Text = "General";
			// 
			// labelUnexposedFrontalAreaUnit
			// 
			this.labelUnexposedFrontalAreaUnit.Location = new System.Drawing.Point(216, 184);
			this.labelUnexposedFrontalAreaUnit.Name = "labelUnexposedFrontalAreaUnit";
			this.labelUnexposedFrontalAreaUnit.Size = new System.Drawing.Size(40, 16);
			this.labelUnexposedFrontalAreaUnit.TabIndex = 38;
			this.labelUnexposedFrontalAreaUnit.Text = "m²";
			this.labelUnexposedFrontalAreaUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxUnexposedFrontalArea
			// 
			this.textBoxUnexposedFrontalArea.Location = new System.Drawing.Point(160, 184);
			this.textBoxUnexposedFrontalArea.Name = "textBoxUnexposedFrontalArea";
			this.textBoxUnexposedFrontalArea.Size = new System.Drawing.Size(48, 19);
			this.textBoxUnexposedFrontalArea.TabIndex = 37;
			// 
			// labelUnexposedFrontalArea
			// 
			this.labelUnexposedFrontalArea.Location = new System.Drawing.Point(8, 184);
			this.labelUnexposedFrontalArea.Name = "labelUnexposedFrontalArea";
			this.labelUnexposedFrontalArea.Size = new System.Drawing.Size(144, 16);
			this.labelUnexposedFrontalArea.TabIndex = 36;
			this.labelUnexposedFrontalArea.Text = "UnexposedFrontalArea:";
			this.labelUnexposedFrontalArea.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelRearBogie
			// 
			this.labelRearBogie.Location = new System.Drawing.Point(8, 336);
			this.labelRearBogie.Name = "labelRearBogie";
			this.labelRearBogie.Size = new System.Drawing.Size(144, 16);
			this.labelRearBogie.TabIndex = 35;
			this.labelRearBogie.Text = "RearBogie:";
			this.labelRearBogie.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelFrontBogie
			// 
			this.labelFrontBogie.Location = new System.Drawing.Point(8, 312);
			this.labelFrontBogie.Name = "labelFrontBogie";
			this.labelFrontBogie.Size = new System.Drawing.Size(144, 16);
			this.labelFrontBogie.TabIndex = 34;
			this.labelFrontBogie.Text = "FrontBogie:";
			this.labelFrontBogie.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonRearBogieSet
			// 
			this.buttonRearBogieSet.Location = new System.Drawing.Point(160, 336);
			this.buttonRearBogieSet.Name = "buttonRearBogieSet";
			this.buttonRearBogieSet.Size = new System.Drawing.Size(48, 19);
			this.buttonRearBogieSet.TabIndex = 33;
			this.buttonRearBogieSet.Text = "Set...";
			this.buttonRearBogieSet.UseVisualStyleBackColor = true;
			this.buttonRearBogieSet.Click += new System.EventHandler(this.ButtonRearBogieSet_Click);
			// 
			// buttonFrontBogieSet
			// 
			this.buttonFrontBogieSet.Location = new System.Drawing.Point(160, 312);
			this.buttonFrontBogieSet.Name = "buttonFrontBogieSet";
			this.buttonFrontBogieSet.Size = new System.Drawing.Size(48, 19);
			this.buttonFrontBogieSet.TabIndex = 32;
			this.buttonFrontBogieSet.Text = "Set...";
			this.buttonFrontBogieSet.UseVisualStyleBackColor = true;
			this.buttonFrontBogieSet.Click += new System.EventHandler(this.ButtonFrontBogieSet_Click);
			// 
			// checkBoxReversed
			// 
			this.checkBoxReversed.Location = new System.Drawing.Point(160, 384);
			this.checkBoxReversed.Name = "checkBoxReversed";
			this.checkBoxReversed.Size = new System.Drawing.Size(48, 16);
			this.checkBoxReversed.TabIndex = 31;
			this.checkBoxReversed.UseVisualStyleBackColor = true;
			// 
			// checkBoxLoadingSway
			// 
			this.checkBoxLoadingSway.Location = new System.Drawing.Point(160, 360);
			this.checkBoxLoadingSway.Name = "checkBoxLoadingSway";
			this.checkBoxLoadingSway.Size = new System.Drawing.Size(48, 16);
			this.checkBoxLoadingSway.TabIndex = 30;
			this.checkBoxLoadingSway.UseVisualStyleBackColor = true;
			// 
			// checkBoxDefinedAxles
			// 
			this.checkBoxDefinedAxles.Location = new System.Drawing.Point(160, 208);
			this.checkBoxDefinedAxles.Name = "checkBoxDefinedAxles";
			this.checkBoxDefinedAxles.Size = new System.Drawing.Size(48, 16);
			this.checkBoxDefinedAxles.TabIndex = 29;
			this.checkBoxDefinedAxles.UseVisualStyleBackColor = true;
			// 
			// checkBoxIsMotorCar
			// 
			this.checkBoxIsMotorCar.Location = new System.Drawing.Point(160, 16);
			this.checkBoxIsMotorCar.Name = "checkBoxIsMotorCar";
			this.checkBoxIsMotorCar.Size = new System.Drawing.Size(48, 16);
			this.checkBoxIsMotorCar.TabIndex = 28;
			this.checkBoxIsMotorCar.UseVisualStyleBackColor = true;
			// 
			// buttonObjectOpen
			// 
			this.buttonObjectOpen.Location = new System.Drawing.Point(208, 432);
			this.buttonObjectOpen.Name = "buttonObjectOpen";
			this.buttonObjectOpen.Size = new System.Drawing.Size(56, 19);
			this.buttonObjectOpen.TabIndex = 3;
			this.buttonObjectOpen.Text = "Open...";
			this.buttonObjectOpen.UseVisualStyleBackColor = true;
			this.buttonObjectOpen.Click += new System.EventHandler(this.ButtonObjectOpen_Click);
			// 
			// labelExposedFrontalAreaUnit
			// 
			this.labelExposedFrontalAreaUnit.Location = new System.Drawing.Point(216, 160);
			this.labelExposedFrontalAreaUnit.Name = "labelExposedFrontalAreaUnit";
			this.labelExposedFrontalAreaUnit.Size = new System.Drawing.Size(40, 16);
			this.labelExposedFrontalAreaUnit.TabIndex = 27;
			this.labelExposedFrontalAreaUnit.Text = "m²";
			this.labelExposedFrontalAreaUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCenterOfMassHeightUnit
			// 
			this.labelCenterOfMassHeightUnit.Location = new System.Drawing.Point(216, 136);
			this.labelCenterOfMassHeightUnit.Name = "labelCenterOfMassHeightUnit";
			this.labelCenterOfMassHeightUnit.Size = new System.Drawing.Size(48, 16);
			this.labelCenterOfMassHeightUnit.TabIndex = 24;
			this.labelCenterOfMassHeightUnit.Text = "m";
			this.labelCenterOfMassHeightUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelLoadingSway
			// 
			this.labelLoadingSway.Location = new System.Drawing.Point(8, 360);
			this.labelLoadingSway.Name = "labelLoadingSway";
			this.labelLoadingSway.Size = new System.Drawing.Size(144, 16);
			this.labelLoadingSway.TabIndex = 11;
			this.labelLoadingSway.Text = "LoadingSway:";
			this.labelLoadingSway.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelHeightUnit
			// 
			this.labelHeightUnit.Location = new System.Drawing.Point(216, 112);
			this.labelHeightUnit.Name = "labelHeightUnit";
			this.labelHeightUnit.Size = new System.Drawing.Size(48, 16);
			this.labelHeightUnit.TabIndex = 23;
			this.labelHeightUnit.Text = "m";
			this.labelHeightUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelWidthUnit
			// 
			this.labelWidthUnit.Location = new System.Drawing.Point(216, 88);
			this.labelWidthUnit.Name = "labelWidthUnit";
			this.labelWidthUnit.Size = new System.Drawing.Size(48, 16);
			this.labelWidthUnit.TabIndex = 22;
			this.labelWidthUnit.Text = "m";
			this.labelWidthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelLengthUnit
			// 
			this.labelLengthUnit.Location = new System.Drawing.Point(216, 64);
			this.labelLengthUnit.Name = "labelLengthUnit";
			this.labelLengthUnit.Size = new System.Drawing.Size(48, 16);
			this.labelLengthUnit.TabIndex = 21;
			this.labelLengthUnit.Text = "m";
			this.labelLengthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelMassUnit
			// 
			this.labelMassUnit.Location = new System.Drawing.Point(216, 40);
			this.labelMassUnit.Name = "labelMassUnit";
			this.labelMassUnit.Size = new System.Drawing.Size(48, 16);
			this.labelMassUnit.TabIndex = 20;
			this.labelMassUnit.Text = "1000 kg";
			this.labelMassUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxMass
			// 
			this.textBoxMass.Location = new System.Drawing.Point(160, 40);
			this.textBoxMass.Name = "textBoxMass";
			this.textBoxMass.Size = new System.Drawing.Size(48, 19);
			this.textBoxMass.TabIndex = 19;
			// 
			// textBoxLength
			// 
			this.textBoxLength.Location = new System.Drawing.Point(160, 64);
			this.textBoxLength.Name = "textBoxLength";
			this.textBoxLength.Size = new System.Drawing.Size(48, 19);
			this.textBoxLength.TabIndex = 18;
			// 
			// textBoxWidth
			// 
			this.textBoxWidth.Location = new System.Drawing.Point(160, 88);
			this.textBoxWidth.Name = "textBoxWidth";
			this.textBoxWidth.Size = new System.Drawing.Size(48, 19);
			this.textBoxWidth.TabIndex = 17;
			// 
			// textBoxHeight
			// 
			this.textBoxHeight.Location = new System.Drawing.Point(160, 112);
			this.textBoxHeight.Name = "textBoxHeight";
			this.textBoxHeight.Size = new System.Drawing.Size(48, 19);
			this.textBoxHeight.TabIndex = 16;
			// 
			// textBoxCenterOfMassHeight
			// 
			this.textBoxCenterOfMassHeight.Location = new System.Drawing.Point(160, 136);
			this.textBoxCenterOfMassHeight.Name = "textBoxCenterOfMassHeight";
			this.textBoxCenterOfMassHeight.Size = new System.Drawing.Size(48, 19);
			this.textBoxCenterOfMassHeight.TabIndex = 15;
			// 
			// textBoxExposedFrontalArea
			// 
			this.textBoxExposedFrontalArea.Location = new System.Drawing.Point(160, 160);
			this.textBoxExposedFrontalArea.Name = "textBoxExposedFrontalArea";
			this.textBoxExposedFrontalArea.Size = new System.Drawing.Size(48, 19);
			this.textBoxExposedFrontalArea.TabIndex = 14;
			// 
			// textBoxObject
			// 
			this.textBoxObject.Location = new System.Drawing.Point(160, 408);
			this.textBoxObject.Name = "textBoxObject";
			this.textBoxObject.Size = new System.Drawing.Size(104, 19);
			this.textBoxObject.TabIndex = 13;
			// 
			// labelObject
			// 
			this.labelObject.Location = new System.Drawing.Point(8, 408);
			this.labelObject.Name = "labelObject";
			this.labelObject.Size = new System.Drawing.Size(144, 16);
			this.labelObject.TabIndex = 10;
			this.labelObject.Text = "Object:";
			this.labelObject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelReversed
			// 
			this.labelReversed.Location = new System.Drawing.Point(8, 384);
			this.labelReversed.Name = "labelReversed";
			this.labelReversed.Size = new System.Drawing.Size(144, 16);
			this.labelReversed.TabIndex = 9;
			this.labelReversed.Text = "Reversed:";
			this.labelReversed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxAxles
			// 
			this.groupBoxAxles.Controls.Add(this.labelRearAxleUnit);
			this.groupBoxAxles.Controls.Add(this.labelFrontAxleUnit);
			this.groupBoxAxles.Controls.Add(this.textBoxRearAxle);
			this.groupBoxAxles.Controls.Add(this.textBoxFrontAxle);
			this.groupBoxAxles.Controls.Add(this.labelRearAxle);
			this.groupBoxAxles.Controls.Add(this.labelFrontAxle);
			this.groupBoxAxles.Location = new System.Drawing.Point(8, 232);
			this.groupBoxAxles.Name = "groupBoxAxles";
			this.groupBoxAxles.Size = new System.Drawing.Size(256, 72);
			this.groupBoxAxles.TabIndex = 8;
			this.groupBoxAxles.TabStop = false;
			this.groupBoxAxles.Text = "Axles";
			// 
			// labelRearAxleUnit
			// 
			this.labelRearAxleUnit.Location = new System.Drawing.Point(208, 40);
			this.labelRearAxleUnit.Name = "labelRearAxleUnit";
			this.labelRearAxleUnit.Size = new System.Drawing.Size(40, 16);
			this.labelRearAxleUnit.TabIndex = 26;
			this.labelRearAxleUnit.Text = "m";
			this.labelRearAxleUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelFrontAxleUnit
			// 
			this.labelFrontAxleUnit.Location = new System.Drawing.Point(208, 16);
			this.labelFrontAxleUnit.Name = "labelFrontAxleUnit";
			this.labelFrontAxleUnit.Size = new System.Drawing.Size(40, 16);
			this.labelFrontAxleUnit.TabIndex = 25;
			this.labelFrontAxleUnit.Text = "m";
			this.labelFrontAxleUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxRearAxle
			// 
			this.textBoxRearAxle.Location = new System.Drawing.Point(152, 40);
			this.textBoxRearAxle.Name = "textBoxRearAxle";
			this.textBoxRearAxle.Size = new System.Drawing.Size(48, 19);
			this.textBoxRearAxle.TabIndex = 12;
			// 
			// textBoxFrontAxle
			// 
			this.textBoxFrontAxle.Location = new System.Drawing.Point(152, 16);
			this.textBoxFrontAxle.Name = "textBoxFrontAxle";
			this.textBoxFrontAxle.Size = new System.Drawing.Size(48, 19);
			this.textBoxFrontAxle.TabIndex = 11;
			// 
			// labelRearAxle
			// 
			this.labelRearAxle.Location = new System.Drawing.Point(8, 40);
			this.labelRearAxle.Name = "labelRearAxle";
			this.labelRearAxle.Size = new System.Drawing.Size(136, 16);
			this.labelRearAxle.TabIndex = 10;
			this.labelRearAxle.Text = "RearAxle:";
			this.labelRearAxle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelFrontAxle
			// 
			this.labelFrontAxle.Location = new System.Drawing.Point(8, 16);
			this.labelFrontAxle.Name = "labelFrontAxle";
			this.labelFrontAxle.Size = new System.Drawing.Size(136, 16);
			this.labelFrontAxle.TabIndex = 9;
			this.labelFrontAxle.Text = "FrontAxle:";
			this.labelFrontAxle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDefinedAxles
			// 
			this.labelDefinedAxles.Location = new System.Drawing.Point(8, 208);
			this.labelDefinedAxles.Name = "labelDefinedAxles";
			this.labelDefinedAxles.Size = new System.Drawing.Size(144, 16);
			this.labelDefinedAxles.TabIndex = 7;
			this.labelDefinedAxles.Text = "DefinedAxles:";
			this.labelDefinedAxles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelExposedFrontalArea
			// 
			this.labelExposedFrontalArea.Location = new System.Drawing.Point(8, 160);
			this.labelExposedFrontalArea.Name = "labelExposedFrontalArea";
			this.labelExposedFrontalArea.Size = new System.Drawing.Size(144, 16);
			this.labelExposedFrontalArea.TabIndex = 6;
			this.labelExposedFrontalArea.Text = "ExposedFrontalArea:";
			this.labelExposedFrontalArea.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCenterOfMassHeight
			// 
			this.labelCenterOfMassHeight.Location = new System.Drawing.Point(8, 136);
			this.labelCenterOfMassHeight.Name = "labelCenterOfMassHeight";
			this.labelCenterOfMassHeight.Size = new System.Drawing.Size(144, 16);
			this.labelCenterOfMassHeight.TabIndex = 5;
			this.labelCenterOfMassHeight.Text = "CenterOfMassHeight:";
			this.labelCenterOfMassHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelHeight
			// 
			this.labelHeight.Location = new System.Drawing.Point(8, 112);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(144, 16);
			this.labelHeight.TabIndex = 4;
			this.labelHeight.Text = "Height:";
			this.labelHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelWidth
			// 
			this.labelWidth.Location = new System.Drawing.Point(8, 88);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(144, 16);
			this.labelWidth.TabIndex = 3;
			this.labelWidth.Text = "Width:";
			this.labelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLength
			// 
			this.labelLength.Location = new System.Drawing.Point(8, 64);
			this.labelLength.Name = "labelLength";
			this.labelLength.Size = new System.Drawing.Size(144, 16);
			this.labelLength.TabIndex = 2;
			this.labelLength.Text = "Length:";
			this.labelLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMass
			// 
			this.labelMass.Location = new System.Drawing.Point(8, 40);
			this.labelMass.Name = "labelMass";
			this.labelMass.Size = new System.Drawing.Size(144, 16);
			this.labelMass.TabIndex = 1;
			this.labelMass.Text = "Mass:";
			this.labelMass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelIsMotorCar
			// 
			this.labelIsMotorCar.Location = new System.Drawing.Point(8, 16);
			this.labelIsMotorCar.Name = "labelIsMotorCar";
			this.labelIsMotorCar.Size = new System.Drawing.Size(144, 16);
			this.labelIsMotorCar.TabIndex = 0;
			this.labelIsMotorCar.Text = "IsMotorCar:";
			this.labelIsMotorCar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageAccel
			// 
			this.tabPageAccel.Controls.Add(this.pictureBoxAccel);
			this.tabPageAccel.Controls.Add(this.panelAccel);
			this.tabPageAccel.Location = new System.Drawing.Point(4, 22);
			this.tabPageAccel.Name = "tabPageAccel";
			this.tabPageAccel.Size = new System.Drawing.Size(792, 670);
			this.tabPageAccel.TabIndex = 5;
			this.tabPageAccel.Text = "Acceleration settings";
			this.tabPageAccel.UseVisualStyleBackColor = true;
			// 
			// pictureBoxAccel
			// 
			this.pictureBoxAccel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBoxAccel.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxAccel.Name = "pictureBoxAccel";
			this.pictureBoxAccel.Size = new System.Drawing.Size(576, 670);
			this.pictureBoxAccel.TabIndex = 1;
			this.pictureBoxAccel.TabStop = false;
			this.pictureBoxAccel.MouseEnter += new System.EventHandler(this.PictureBoxAccel_MouseEnter);
			this.pictureBoxAccel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxAccel_MouseMove);
			// 
			// panelAccel
			// 
			this.panelAccel.Controls.Add(this.groupBoxPreview);
			this.panelAccel.Controls.Add(this.groupBoxParameter);
			this.panelAccel.Controls.Add(this.groupBoxNotch);
			this.panelAccel.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelAccel.Location = new System.Drawing.Point(576, 0);
			this.panelAccel.Name = "panelAccel";
			this.panelAccel.Size = new System.Drawing.Size(216, 670);
			this.panelAccel.TabIndex = 0;
			// 
			// groupBoxPreview
			// 
			this.groupBoxPreview.Controls.Add(this.buttonAccelReset);
			this.groupBoxPreview.Controls.Add(this.buttonAccelZoomOut);
			this.groupBoxPreview.Controls.Add(this.buttonAccelZoomIn);
			this.groupBoxPreview.Controls.Add(this.labelAccelYValue);
			this.groupBoxPreview.Controls.Add(this.labelAccelXValue);
			this.groupBoxPreview.Controls.Add(this.labelAccelXmaxUnit);
			this.groupBoxPreview.Controls.Add(this.labelAccelXminUnit);
			this.groupBoxPreview.Controls.Add(this.labelAccelYmaxUnit);
			this.groupBoxPreview.Controls.Add(this.labelAccelYminUnit);
			this.groupBoxPreview.Controls.Add(this.textBoxAccelYmax);
			this.groupBoxPreview.Controls.Add(this.textBoxAccelYmin);
			this.groupBoxPreview.Controls.Add(this.textBoxAccelXmax);
			this.groupBoxPreview.Controls.Add(this.textBoxAccelXmin);
			this.groupBoxPreview.Controls.Add(this.labelAccelYmax);
			this.groupBoxPreview.Controls.Add(this.labelAccelYmin);
			this.groupBoxPreview.Controls.Add(this.labelAccelXmax);
			this.groupBoxPreview.Controls.Add(this.labelAccelXmin);
			this.groupBoxPreview.Controls.Add(this.labelAccelY);
			this.groupBoxPreview.Controls.Add(this.labelAccelX);
			this.groupBoxPreview.Controls.Add(this.checkBoxSubtractDeceleration);
			this.groupBoxPreview.Location = new System.Drawing.Point(8, 216);
			this.groupBoxPreview.Name = "groupBoxPreview";
			this.groupBoxPreview.Size = new System.Drawing.Size(200, 232);
			this.groupBoxPreview.TabIndex = 2;
			this.groupBoxPreview.TabStop = false;
			this.groupBoxPreview.Text = "Preview";
			// 
			// buttonAccelReset
			// 
			this.buttonAccelReset.Location = new System.Drawing.Point(136, 200);
			this.buttonAccelReset.Name = "buttonAccelReset";
			this.buttonAccelReset.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelReset.TabIndex = 42;
			this.buttonAccelReset.UseVisualStyleBackColor = true;
			// 
			// buttonAccelZoomOut
			// 
			this.buttonAccelZoomOut.Location = new System.Drawing.Point(72, 200);
			this.buttonAccelZoomOut.Name = "buttonAccelZoomOut";
			this.buttonAccelZoomOut.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelZoomOut.TabIndex = 41;
			this.buttonAccelZoomOut.UseVisualStyleBackColor = true;
			// 
			// buttonAccelZoomIn
			// 
			this.buttonAccelZoomIn.Location = new System.Drawing.Point(8, 200);
			this.buttonAccelZoomIn.Name = "buttonAccelZoomIn";
			this.buttonAccelZoomIn.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelZoomIn.TabIndex = 40;
			this.buttonAccelZoomIn.UseVisualStyleBackColor = true;
			// 
			// labelAccelYValue
			// 
			this.labelAccelYValue.Location = new System.Drawing.Point(88, 80);
			this.labelAccelYValue.Name = "labelAccelYValue";
			this.labelAccelYValue.Size = new System.Drawing.Size(104, 16);
			this.labelAccelYValue.TabIndex = 39;
			this.labelAccelYValue.Text = "0.00 km/h/s";
			this.labelAccelYValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXValue
			// 
			this.labelAccelXValue.Location = new System.Drawing.Point(88, 56);
			this.labelAccelXValue.Name = "labelAccelXValue";
			this.labelAccelXValue.Size = new System.Drawing.Size(104, 16);
			this.labelAccelXValue.TabIndex = 38;
			this.labelAccelXValue.Text = "0.00 km/h";
			this.labelAccelXValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXmaxUnit
			// 
			this.labelAccelXmaxUnit.Location = new System.Drawing.Point(144, 128);
			this.labelAccelXmaxUnit.Name = "labelAccelXmaxUnit";
			this.labelAccelXmaxUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelXmaxUnit.TabIndex = 37;
			this.labelAccelXmaxUnit.Text = "km/h";
			this.labelAccelXmaxUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXminUnit
			// 
			this.labelAccelXminUnit.Location = new System.Drawing.Point(144, 104);
			this.labelAccelXminUnit.Name = "labelAccelXminUnit";
			this.labelAccelXminUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelXminUnit.TabIndex = 36;
			this.labelAccelXminUnit.Text = "km/h";
			this.labelAccelXminUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelYmaxUnit
			// 
			this.labelAccelYmaxUnit.Location = new System.Drawing.Point(144, 176);
			this.labelAccelYmaxUnit.Name = "labelAccelYmaxUnit";
			this.labelAccelYmaxUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelYmaxUnit.TabIndex = 35;
			this.labelAccelYmaxUnit.Text = "km/h/s";
			this.labelAccelYmaxUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelYminUnit
			// 
			this.labelAccelYminUnit.Location = new System.Drawing.Point(144, 152);
			this.labelAccelYminUnit.Name = "labelAccelYminUnit";
			this.labelAccelYminUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelYminUnit.TabIndex = 34;
			this.labelAccelYminUnit.Text = "km/h/s";
			this.labelAccelYminUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelYmax
			// 
			this.textBoxAccelYmax.Location = new System.Drawing.Point(88, 176);
			this.textBoxAccelYmax.Name = "textBoxAccelYmax";
			this.textBoxAccelYmax.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelYmax.TabIndex = 25;
			// 
			// textBoxAccelYmin
			// 
			this.textBoxAccelYmin.Location = new System.Drawing.Point(88, 152);
			this.textBoxAccelYmin.Name = "textBoxAccelYmin";
			this.textBoxAccelYmin.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelYmin.TabIndex = 24;
			// 
			// textBoxAccelXmax
			// 
			this.textBoxAccelXmax.Location = new System.Drawing.Point(88, 128);
			this.textBoxAccelXmax.Name = "textBoxAccelXmax";
			this.textBoxAccelXmax.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelXmax.TabIndex = 23;
			// 
			// textBoxAccelXmin
			// 
			this.textBoxAccelXmin.Location = new System.Drawing.Point(88, 104);
			this.textBoxAccelXmin.Name = "textBoxAccelXmin";
			this.textBoxAccelXmin.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelXmin.TabIndex = 22;
			// 
			// labelAccelYmax
			// 
			this.labelAccelYmax.Location = new System.Drawing.Point(8, 176);
			this.labelAccelYmax.Name = "labelAccelYmax";
			this.labelAccelYmax.Size = new System.Drawing.Size(72, 16);
			this.labelAccelYmax.TabIndex = 8;
			this.labelAccelYmax.Text = "Ymax:";
			this.labelAccelYmax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelYmin
			// 
			this.labelAccelYmin.Location = new System.Drawing.Point(8, 152);
			this.labelAccelYmin.Name = "labelAccelYmin";
			this.labelAccelYmin.Size = new System.Drawing.Size(72, 16);
			this.labelAccelYmin.TabIndex = 7;
			this.labelAccelYmin.Text = "Ymin:";
			this.labelAccelYmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelXmax
			// 
			this.labelAccelXmax.Location = new System.Drawing.Point(8, 128);
			this.labelAccelXmax.Name = "labelAccelXmax";
			this.labelAccelXmax.Size = new System.Drawing.Size(72, 16);
			this.labelAccelXmax.TabIndex = 6;
			this.labelAccelXmax.Text = "Xmax:";
			this.labelAccelXmax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelXmin
			// 
			this.labelAccelXmin.Location = new System.Drawing.Point(8, 104);
			this.labelAccelXmin.Name = "labelAccelXmin";
			this.labelAccelXmin.Size = new System.Drawing.Size(72, 16);
			this.labelAccelXmin.TabIndex = 5;
			this.labelAccelXmin.Text = "Xmin:";
			this.labelAccelXmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelY
			// 
			this.labelAccelY.Location = new System.Drawing.Point(8, 80);
			this.labelAccelY.Name = "labelAccelY";
			this.labelAccelY.Size = new System.Drawing.Size(72, 16);
			this.labelAccelY.TabIndex = 4;
			this.labelAccelY.Text = "Y:";
			this.labelAccelY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelX
			// 
			this.labelAccelX.Location = new System.Drawing.Point(8, 56);
			this.labelAccelX.Name = "labelAccelX";
			this.labelAccelX.Size = new System.Drawing.Size(72, 16);
			this.labelAccelX.TabIndex = 3;
			this.labelAccelX.Text = "X:";
			this.labelAccelX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxSubtractDeceleration
			// 
			this.checkBoxSubtractDeceleration.Location = new System.Drawing.Point(8, 16);
			this.checkBoxSubtractDeceleration.Name = "checkBoxSubtractDeceleration";
			this.checkBoxSubtractDeceleration.Size = new System.Drawing.Size(184, 32);
			this.checkBoxSubtractDeceleration.TabIndex = 0;
			this.checkBoxSubtractDeceleration.Text = "Subtract deceleration due to air and rolling resistance";
			this.checkBoxSubtractDeceleration.UseVisualStyleBackColor = true;
			// 
			// groupBoxParameter
			// 
			this.groupBoxParameter.Controls.Add(this.labeAccelA0Unit);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelA0);
			this.groupBoxParameter.Controls.Add(this.labelAccelA0);
			this.groupBoxParameter.Controls.Add(this.labelAccelA1Unit);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelA1);
			this.groupBoxParameter.Controls.Add(this.labelAccelA1);
			this.groupBoxParameter.Controls.Add(this.labelAccelV1Unit);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelV1);
			this.groupBoxParameter.Controls.Add(this.labelAccelV1);
			this.groupBoxParameter.Controls.Add(this.labelAccelV2Unit);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelV2);
			this.groupBoxParameter.Controls.Add(this.labelAccelV2);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelE);
			this.groupBoxParameter.Controls.Add(this.labelAccelE);
			this.groupBoxParameter.Location = new System.Drawing.Point(8, 64);
			this.groupBoxParameter.Name = "groupBoxParameter";
			this.groupBoxParameter.Size = new System.Drawing.Size(200, 144);
			this.groupBoxParameter.TabIndex = 1;
			this.groupBoxParameter.TabStop = false;
			this.groupBoxParameter.Text = "Parameter";
			// 
			// labeAccelA0Unit
			// 
			this.labeAccelA0Unit.Location = new System.Drawing.Point(144, 16);
			this.labeAccelA0Unit.Name = "labeAccelA0Unit";
			this.labeAccelA0Unit.Size = new System.Drawing.Size(48, 16);
			this.labeAccelA0Unit.TabIndex = 33;
			this.labeAccelA0Unit.Text = "km/h/s";
			this.labeAccelA0Unit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelA0
			// 
			this.textBoxAccelA0.Location = new System.Drawing.Point(88, 16);
			this.textBoxAccelA0.Name = "textBoxAccelA0";
			this.textBoxAccelA0.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelA0.TabIndex = 32;
			// 
			// labelAccelA0
			// 
			this.labelAccelA0.Location = new System.Drawing.Point(8, 16);
			this.labelAccelA0.Name = "labelAccelA0";
			this.labelAccelA0.Size = new System.Drawing.Size(72, 16);
			this.labelAccelA0.TabIndex = 31;
			this.labelAccelA0.Text = "a0:";
			this.labelAccelA0.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelA1Unit
			// 
			this.labelAccelA1Unit.Location = new System.Drawing.Point(144, 40);
			this.labelAccelA1Unit.Name = "labelAccelA1Unit";
			this.labelAccelA1Unit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelA1Unit.TabIndex = 30;
			this.labelAccelA1Unit.Text = "km/h/s";
			this.labelAccelA1Unit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelA1
			// 
			this.textBoxAccelA1.Location = new System.Drawing.Point(88, 40);
			this.textBoxAccelA1.Name = "textBoxAccelA1";
			this.textBoxAccelA1.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelA1.TabIndex = 29;
			// 
			// labelAccelA1
			// 
			this.labelAccelA1.Location = new System.Drawing.Point(8, 40);
			this.labelAccelA1.Name = "labelAccelA1";
			this.labelAccelA1.Size = new System.Drawing.Size(72, 16);
			this.labelAccelA1.TabIndex = 28;
			this.labelAccelA1.Text = "a1:";
			this.labelAccelA1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelV1Unit
			// 
			this.labelAccelV1Unit.Location = new System.Drawing.Point(144, 64);
			this.labelAccelV1Unit.Name = "labelAccelV1Unit";
			this.labelAccelV1Unit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelV1Unit.TabIndex = 27;
			this.labelAccelV1Unit.Text = "km/h";
			this.labelAccelV1Unit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelV1
			// 
			this.textBoxAccelV1.Location = new System.Drawing.Point(88, 64);
			this.textBoxAccelV1.Name = "textBoxAccelV1";
			this.textBoxAccelV1.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelV1.TabIndex = 26;
			// 
			// labelAccelV1
			// 
			this.labelAccelV1.Location = new System.Drawing.Point(8, 64);
			this.labelAccelV1.Name = "labelAccelV1";
			this.labelAccelV1.Size = new System.Drawing.Size(72, 16);
			this.labelAccelV1.TabIndex = 25;
			this.labelAccelV1.Text = "v1:";
			this.labelAccelV1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelV2Unit
			// 
			this.labelAccelV2Unit.Location = new System.Drawing.Point(144, 88);
			this.labelAccelV2Unit.Name = "labelAccelV2Unit";
			this.labelAccelV2Unit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelV2Unit.TabIndex = 24;
			this.labelAccelV2Unit.Text = "km/h";
			this.labelAccelV2Unit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelV2
			// 
			this.textBoxAccelV2.Location = new System.Drawing.Point(88, 88);
			this.textBoxAccelV2.Name = "textBoxAccelV2";
			this.textBoxAccelV2.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelV2.TabIndex = 23;
			// 
			// labelAccelV2
			// 
			this.labelAccelV2.Location = new System.Drawing.Point(8, 88);
			this.labelAccelV2.Name = "labelAccelV2";
			this.labelAccelV2.Size = new System.Drawing.Size(72, 16);
			this.labelAccelV2.TabIndex = 22;
			this.labelAccelV2.Text = "v2:";
			this.labelAccelV2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxAccelE
			// 
			this.textBoxAccelE.Location = new System.Drawing.Point(88, 112);
			this.textBoxAccelE.Name = "textBoxAccelE";
			this.textBoxAccelE.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelE.TabIndex = 21;
			// 
			// labelAccelE
			// 
			this.labelAccelE.Location = new System.Drawing.Point(8, 112);
			this.labelAccelE.Name = "labelAccelE";
			this.labelAccelE.Size = new System.Drawing.Size(72, 16);
			this.labelAccelE.TabIndex = 2;
			this.labelAccelE.Text = "e (2.0):";
			this.labelAccelE.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxNotch
			// 
			this.groupBoxNotch.Controls.Add(this.comboBoxNotch);
			this.groupBoxNotch.Location = new System.Drawing.Point(8, 8);
			this.groupBoxNotch.Name = "groupBoxNotch";
			this.groupBoxNotch.Size = new System.Drawing.Size(200, 48);
			this.groupBoxNotch.TabIndex = 0;
			this.groupBoxNotch.TabStop = false;
			this.groupBoxNotch.Text = "Notch";
			// 
			// comboBoxNotch
			// 
			this.comboBoxNotch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxNotch.FormattingEnabled = true;
			this.comboBoxNotch.Location = new System.Drawing.Point(8, 16);
			this.comboBoxNotch.Name = "comboBoxNotch";
			this.comboBoxNotch.Size = new System.Drawing.Size(72, 20);
			this.comboBoxNotch.TabIndex = 0;
			// 
			// tabPageMotor
			// 
			this.tabPageMotor.Controls.Add(this.panelMotorSound);
			this.tabPageMotor.Location = new System.Drawing.Point(4, 22);
			this.tabPageMotor.Name = "tabPageMotor";
			this.tabPageMotor.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageMotor.Size = new System.Drawing.Size(792, 670);
			this.tabPageMotor.TabIndex = 1;
			this.tabPageMotor.Text = "Motor sound settings";
			this.tabPageMotor.UseVisualStyleBackColor = true;
			// 
			// panelMotorSound
			// 
			this.panelMotorSound.Controls.Add(this.toolStripContainerDrawArea);
			this.panelMotorSound.Controls.Add(this.panelMotorSetting);
			this.panelMotorSound.Controls.Add(this.statusStripStatus);
			this.panelMotorSound.Controls.Add(this.menuStripMotor);
			this.panelMotorSound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMotorSound.Location = new System.Drawing.Point(3, 3);
			this.panelMotorSound.Name = "panelMotorSound";
			this.panelMotorSound.Size = new System.Drawing.Size(786, 664);
			this.panelMotorSound.TabIndex = 5;
			// 
			// toolStripContainerDrawArea
			// 
			// 
			// toolStripContainerDrawArea.ContentPanel
			// 
			this.toolStripContainerDrawArea.ContentPanel.Controls.Add(this.glControlMotor);
			this.toolStripContainerDrawArea.ContentPanel.Size = new System.Drawing.Size(568, 593);
			this.toolStripContainerDrawArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainerDrawArea.Location = new System.Drawing.Point(0, 24);
			this.toolStripContainerDrawArea.Name = "toolStripContainerDrawArea";
			this.toolStripContainerDrawArea.Size = new System.Drawing.Size(568, 618);
			this.toolStripContainerDrawArea.TabIndex = 4;
			this.toolStripContainerDrawArea.Text = "toolStripContainer1";
			// 
			// toolStripContainerDrawArea.TopToolStripPanel
			// 
			this.toolStripContainerDrawArea.TopToolStripPanel.Controls.Add(this.toolStripToolBar);
			// 
			// glControlMotor
			// 
			this.glControlMotor.BackColor = System.Drawing.Color.Black;
			this.glControlMotor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glControlMotor.Location = new System.Drawing.Point(0, 0);
			this.glControlMotor.Name = "glControlMotor";
			this.glControlMotor.Size = new System.Drawing.Size(568, 593);
			this.glControlMotor.TabIndex = 0;
			this.glControlMotor.VSync = false;
			this.glControlMotor.Load += new System.EventHandler(this.glControlMotor_Load);
			this.glControlMotor.Paint += new System.Windows.Forms.PaintEventHandler(this.GlControlMotor_Paint);
			this.glControlMotor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlControlMotor_KeyDown);
			this.glControlMotor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseDown);
			this.glControlMotor.MouseEnter += new System.EventHandler(this.GlControlMotor_MouseEnter);
			this.glControlMotor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseMove);
			this.glControlMotor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseUp);
			this.glControlMotor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.GlControlMotor_PreviewKeyDown);
			// 
			// toolStripToolBar
			// 
			this.toolStripToolBar.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonUndo,
            this.toolStripButtonRedo,
            this.toolStripSeparatorRedo,
            this.toolStripButtonTearingOff,
            this.toolStripButtonCopy,
            this.toolStripButtonPaste,
            this.toolStripButtonCleanup,
            this.toolStripButtonDelete,
            this.toolStripSeparatorEdit,
            this.toolStripButtonSelect,
            this.toolStripButtonMove,
            this.toolStripButtonDot,
            this.toolStripButtonLine});
			this.toolStripToolBar.Location = new System.Drawing.Point(3, 0);
			this.toolStripToolBar.Name = "toolStripToolBar";
			this.toolStripToolBar.Size = new System.Drawing.Size(275, 25);
			this.toolStripToolBar.TabIndex = 0;
			// 
			// toolStripButtonUndo
			// 
			this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonUndo.Name = "toolStripButtonUndo";
			this.toolStripButtonUndo.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonUndo.Text = "Undo (Ctrl+Z)";
			// 
			// toolStripButtonRedo
			// 
			this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRedo.Name = "toolStripButtonRedo";
			this.toolStripButtonRedo.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonRedo.Text = "Redo (Ctrl+Y)";
			// 
			// toolStripSeparatorRedo
			// 
			this.toolStripSeparatorRedo.Name = "toolStripSeparatorRedo";
			this.toolStripSeparatorRedo.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonTearingOff
			// 
			this.toolStripButtonTearingOff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonTearingOff.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonTearingOff.Name = "toolStripButtonTearingOff";
			this.toolStripButtonTearingOff.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonTearingOff.Text = "Cut (Ctrl+X)";
			// 
			// toolStripButtonCopy
			// 
			this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCopy.Name = "toolStripButtonCopy";
			this.toolStripButtonCopy.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonCopy.Text = "Copy (Ctrl+C)";
			// 
			// toolStripButtonPaste
			// 
			this.toolStripButtonPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPaste.Name = "toolStripButtonPaste";
			this.toolStripButtonPaste.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonPaste.Text = "Paste (Ctrl+V)";
			// 
			// toolStripButtonCleanup
			// 
			this.toolStripButtonCleanup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCleanup.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCleanup.Name = "toolStripButtonCleanup";
			this.toolStripButtonCleanup.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonCleanup.Text = "Cleanup";
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonDelete.Text = "Delete (Del)";
			// 
			// toolStripSeparatorEdit
			// 
			this.toolStripSeparatorEdit.Name = "toolStripSeparatorEdit";
			this.toolStripSeparatorEdit.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonSelect
			// 
			this.toolStripButtonSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSelect.Name = "toolStripButtonSelect";
			this.toolStripButtonSelect.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonSelect.Text = "Select (Alt+A)";
			// 
			// toolStripButtonMove
			// 
			this.toolStripButtonMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMove.Name = "toolStripButtonMove";
			this.toolStripButtonMove.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonMove.Text = "Move (Alt+S)";
			// 
			// toolStripButtonDot
			// 
			this.toolStripButtonDot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDot.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDot.Name = "toolStripButtonDot";
			this.toolStripButtonDot.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonDot.Text = "Dot (Alt+D)";
			// 
			// toolStripButtonLine
			// 
			this.toolStripButtonLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonLine.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLine.Name = "toolStripButtonLine";
			this.toolStripButtonLine.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonLine.Text = "Line (Alt+F)";
			// 
			// panelMotorSetting
			// 
			this.panelMotorSetting.Controls.Add(this.groupBoxDirect);
			this.panelMotorSetting.Controls.Add(this.groupBoxPlay);
			this.panelMotorSetting.Controls.Add(this.groupBoxView);
			this.panelMotorSetting.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelMotorSetting.Location = new System.Drawing.Point(568, 24);
			this.panelMotorSetting.Name = "panelMotorSetting";
			this.panelMotorSetting.Size = new System.Drawing.Size(218, 618);
			this.panelMotorSetting.TabIndex = 3;
			// 
			// groupBoxDirect
			// 
			this.groupBoxDirect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxDirect.Controls.Add(this.buttonDirectDot);
			this.groupBoxDirect.Controls.Add(this.buttonDirectMove);
			this.groupBoxDirect.Controls.Add(this.textBoxDirectY);
			this.groupBoxDirect.Controls.Add(this.labelDirectY);
			this.groupBoxDirect.Controls.Add(this.labelDirectXUnit);
			this.groupBoxDirect.Controls.Add(this.textBoxDirectX);
			this.groupBoxDirect.Controls.Add(this.labelDirectX);
			this.groupBoxDirect.Location = new System.Drawing.Point(8, 254);
			this.groupBoxDirect.Name = "groupBoxDirect";
			this.groupBoxDirect.Size = new System.Drawing.Size(200, 96);
			this.groupBoxDirect.TabIndex = 2;
			this.groupBoxDirect.TabStop = false;
			this.groupBoxDirect.Text = "Direct input";
			// 
			// buttonDirectDot
			// 
			this.buttonDirectDot.Location = new System.Drawing.Point(8, 64);
			this.buttonDirectDot.Name = "buttonDirectDot";
			this.buttonDirectDot.Size = new System.Drawing.Size(56, 24);
			this.buttonDirectDot.TabIndex = 22;
			this.buttonDirectDot.UseVisualStyleBackColor = true;
			// 
			// buttonDirectMove
			// 
			this.buttonDirectMove.Location = new System.Drawing.Point(72, 64);
			this.buttonDirectMove.Name = "buttonDirectMove";
			this.buttonDirectMove.Size = new System.Drawing.Size(56, 24);
			this.buttonDirectMove.TabIndex = 21;
			this.buttonDirectMove.UseVisualStyleBackColor = true;
			// 
			// textBoxDirectY
			// 
			this.textBoxDirectY.Location = new System.Drawing.Point(112, 40);
			this.textBoxDirectY.Name = "textBoxDirectY";
			this.textBoxDirectY.Size = new System.Drawing.Size(40, 19);
			this.textBoxDirectY.TabIndex = 19;
			// 
			// labelDirectY
			// 
			this.labelDirectY.Location = new System.Drawing.Point(8, 40);
			this.labelDirectY.Name = "labelDirectY";
			this.labelDirectY.Size = new System.Drawing.Size(96, 16);
			this.labelDirectY.TabIndex = 18;
			this.labelDirectY.Text = "y coordinate:";
			this.labelDirectY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDirectXUnit
			// 
			this.labelDirectXUnit.Location = new System.Drawing.Point(160, 16);
			this.labelDirectXUnit.Name = "labelDirectXUnit";
			this.labelDirectXUnit.Size = new System.Drawing.Size(32, 16);
			this.labelDirectXUnit.TabIndex = 17;
			this.labelDirectXUnit.Text = "km/h";
			this.labelDirectXUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxDirectX
			// 
			this.textBoxDirectX.Location = new System.Drawing.Point(112, 16);
			this.textBoxDirectX.Name = "textBoxDirectX";
			this.textBoxDirectX.Size = new System.Drawing.Size(40, 19);
			this.textBoxDirectX.TabIndex = 16;
			// 
			// labelDirectX
			// 
			this.labelDirectX.Location = new System.Drawing.Point(8, 16);
			this.labelDirectX.Name = "labelDirectX";
			this.labelDirectX.Size = new System.Drawing.Size(96, 16);
			this.labelDirectX.TabIndex = 15;
			this.labelDirectX.Text = "x coordinate:";
			this.labelDirectX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxPlay
			// 
			this.groupBoxPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxPlay.Controls.Add(this.buttonStop);
			this.groupBoxPlay.Controls.Add(this.buttonPause);
			this.groupBoxPlay.Controls.Add(this.buttonPlay);
			this.groupBoxPlay.Controls.Add(this.groupBoxArea);
			this.groupBoxPlay.Controls.Add(this.groupBoxSource);
			this.groupBoxPlay.Location = new System.Drawing.Point(8, 359);
			this.groupBoxPlay.Name = "groupBoxPlay";
			this.groupBoxPlay.Size = new System.Drawing.Size(200, 248);
			this.groupBoxPlay.TabIndex = 1;
			this.groupBoxPlay.TabStop = false;
			this.groupBoxPlay.Text = "Playback setting";
			// 
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(136, 216);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(56, 24);
			this.buttonStop.TabIndex = 4;
			this.buttonStop.UseVisualStyleBackColor = true;
			// 
			// buttonPause
			// 
			this.buttonPause.Location = new System.Drawing.Point(72, 216);
			this.buttonPause.Name = "buttonPause";
			this.buttonPause.Size = new System.Drawing.Size(56, 24);
			this.buttonPause.TabIndex = 3;
			this.buttonPause.UseVisualStyleBackColor = true;
			// 
			// buttonPlay
			// 
			this.buttonPlay.Location = new System.Drawing.Point(8, 216);
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(56, 24);
			this.buttonPlay.TabIndex = 2;
			this.buttonPlay.UseVisualStyleBackColor = true;
			// 
			// groupBoxArea
			// 
			this.groupBoxArea.Controls.Add(this.checkBoxMotorConstant);
			this.groupBoxArea.Controls.Add(this.checkBoxMotorLoop);
			this.groupBoxArea.Controls.Add(this.buttonMotorSwap);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAreaLeft);
			this.groupBoxArea.Controls.Add(this.labelMotorAreaUnit);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAreaRight);
			this.groupBoxArea.Controls.Add(this.labelMotorAccel);
			this.groupBoxArea.Controls.Add(this.labelMotorAccelUnit);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAccel);
			this.groupBoxArea.Location = new System.Drawing.Point(8, 88);
			this.groupBoxArea.Name = "groupBoxArea";
			this.groupBoxArea.Size = new System.Drawing.Size(184, 120);
			this.groupBoxArea.TabIndex = 1;
			this.groupBoxArea.TabStop = false;
			this.groupBoxArea.Text = "Area setting";
			// 
			// checkBoxMotorConstant
			// 
			this.checkBoxMotorConstant.AutoSize = true;
			this.checkBoxMotorConstant.Location = new System.Drawing.Point(8, 40);
			this.checkBoxMotorConstant.Name = "checkBoxMotorConstant";
			this.checkBoxMotorConstant.Size = new System.Drawing.Size(104, 16);
			this.checkBoxMotorConstant.TabIndex = 19;
			this.checkBoxMotorConstant.Text = "Constant speed";
			this.checkBoxMotorConstant.UseVisualStyleBackColor = true;
			// 
			// checkBoxMotorLoop
			// 
			this.checkBoxMotorLoop.AutoSize = true;
			this.checkBoxMotorLoop.Location = new System.Drawing.Point(8, 16);
			this.checkBoxMotorLoop.Name = "checkBoxMotorLoop";
			this.checkBoxMotorLoop.Size = new System.Drawing.Size(97, 16);
			this.checkBoxMotorLoop.TabIndex = 18;
			this.checkBoxMotorLoop.Text = "Loop playback";
			this.checkBoxMotorLoop.UseVisualStyleBackColor = true;
			// 
			// buttonMotorSwap
			// 
			this.buttonMotorSwap.Location = new System.Drawing.Point(56, 88);
			this.buttonMotorSwap.Name = "buttonMotorSwap";
			this.buttonMotorSwap.Size = new System.Drawing.Size(32, 19);
			this.buttonMotorSwap.TabIndex = 17;
			this.buttonMotorSwap.UseVisualStyleBackColor = true;
			// 
			// textBoxMotorAreaLeft
			// 
			this.textBoxMotorAreaLeft.Location = new System.Drawing.Point(8, 88);
			this.textBoxMotorAreaLeft.Name = "textBoxMotorAreaLeft";
			this.textBoxMotorAreaLeft.Size = new System.Drawing.Size(40, 19);
			this.textBoxMotorAreaLeft.TabIndex = 16;
			// 
			// labelMotorAreaUnit
			// 
			this.labelMotorAreaUnit.Location = new System.Drawing.Point(144, 88);
			this.labelMotorAreaUnit.Name = "labelMotorAreaUnit";
			this.labelMotorAreaUnit.Size = new System.Drawing.Size(32, 16);
			this.labelMotorAreaUnit.TabIndex = 15;
			this.labelMotorAreaUnit.Text = "km/h";
			this.labelMotorAreaUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMotorAreaRight
			// 
			this.textBoxMotorAreaRight.Location = new System.Drawing.Point(96, 88);
			this.textBoxMotorAreaRight.Name = "textBoxMotorAreaRight";
			this.textBoxMotorAreaRight.Size = new System.Drawing.Size(40, 19);
			this.textBoxMotorAreaRight.TabIndex = 3;
			// 
			// labelMotorAccel
			// 
			this.labelMotorAccel.Location = new System.Drawing.Point(8, 64);
			this.labelMotorAccel.Name = "labelMotorAccel";
			this.labelMotorAccel.Size = new System.Drawing.Size(72, 16);
			this.labelMotorAccel.TabIndex = 2;
			this.labelMotorAccel.Text = "Acceleration:";
			this.labelMotorAccel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMotorAccelUnit
			// 
			this.labelMotorAccelUnit.Location = new System.Drawing.Point(128, 64);
			this.labelMotorAccelUnit.Name = "labelMotorAccelUnit";
			this.labelMotorAccelUnit.Size = new System.Drawing.Size(44, 16);
			this.labelMotorAccelUnit.TabIndex = 1;
			this.labelMotorAccelUnit.Text = "km/h/s";
			this.labelMotorAccelUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMotorAccel
			// 
			this.textBoxMotorAccel.Location = new System.Drawing.Point(88, 64);
			this.textBoxMotorAccel.Name = "textBoxMotorAccel";
			this.textBoxMotorAccel.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorAccel.TabIndex = 0;
			// 
			// groupBoxSource
			// 
			this.groupBoxSource.Controls.Add(this.numericUpDownRunIndex);
			this.groupBoxSource.Controls.Add(this.labelRun);
			this.groupBoxSource.Controls.Add(this.checkBoxTrack2);
			this.groupBoxSource.Controls.Add(this.checkBoxTrack1);
			this.groupBoxSource.Location = new System.Drawing.Point(8, 16);
			this.groupBoxSource.Name = "groupBoxSource";
			this.groupBoxSource.Size = new System.Drawing.Size(184, 64);
			this.groupBoxSource.TabIndex = 0;
			this.groupBoxSource.TabStop = false;
			this.groupBoxSource.Text = "Sound source setting";
			// 
			// numericUpDownRunIndex
			// 
			this.numericUpDownRunIndex.Location = new System.Drawing.Point(112, 16);
			this.numericUpDownRunIndex.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numericUpDownRunIndex.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownRunIndex.Name = "numericUpDownRunIndex";
			this.numericUpDownRunIndex.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownRunIndex.TabIndex = 16;
			this.numericUpDownRunIndex.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			// 
			// labelRun
			// 
			this.labelRun.Location = new System.Drawing.Point(8, 16);
			this.labelRun.Name = "labelRun";
			this.labelRun.Size = new System.Drawing.Size(96, 16);
			this.labelRun.TabIndex = 4;
			this.labelRun.Text = "Running sound:";
			this.labelRun.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxTrack2
			// 
			this.checkBoxTrack2.AutoSize = true;
			this.checkBoxTrack2.Location = new System.Drawing.Point(96, 40);
			this.checkBoxTrack2.Name = "checkBoxTrack2";
			this.checkBoxTrack2.Size = new System.Drawing.Size(59, 16);
			this.checkBoxTrack2.TabIndex = 2;
			this.checkBoxTrack2.Text = "Track2";
			this.checkBoxTrack2.UseVisualStyleBackColor = true;
			// 
			// checkBoxTrack1
			// 
			this.checkBoxTrack1.AutoSize = true;
			this.checkBoxTrack1.Location = new System.Drawing.Point(8, 40);
			this.checkBoxTrack1.Name = "checkBoxTrack1";
			this.checkBoxTrack1.Size = new System.Drawing.Size(59, 16);
			this.checkBoxTrack1.TabIndex = 1;
			this.checkBoxTrack1.Text = "Track1";
			this.checkBoxTrack1.UseVisualStyleBackColor = true;
			// 
			// groupBoxView
			// 
			this.groupBoxView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxView.Controls.Add(this.buttonMotorReset);
			this.groupBoxView.Controls.Add(this.buttonMotorZoomOut);
			this.groupBoxView.Controls.Add(this.buttonMotorZoomIn);
			this.groupBoxView.Controls.Add(this.labelMotorMaxVolume);
			this.groupBoxView.Controls.Add(this.textBoxMotorMaxVolume);
			this.groupBoxView.Controls.Add(this.labelMotorMinVolume);
			this.groupBoxView.Controls.Add(this.textBoxMotorMinVolume);
			this.groupBoxView.Controls.Add(this.labelMotorMaxPitch);
			this.groupBoxView.Controls.Add(this.textBoxMotorMaxPitch);
			this.groupBoxView.Controls.Add(this.labelMotorMinPitch);
			this.groupBoxView.Controls.Add(this.textBoxMotorMinPitch);
			this.groupBoxView.Controls.Add(this.labelMotorMaxVelocityUnit);
			this.groupBoxView.Controls.Add(this.textBoxMotorMaxVelocity);
			this.groupBoxView.Controls.Add(this.labelMotorMaxVelocity);
			this.groupBoxView.Controls.Add(this.labelMotorMinVelocityUnit);
			this.groupBoxView.Controls.Add(this.textBoxMotorMinVelocity);
			this.groupBoxView.Controls.Add(this.labelMotorMinVelocity);
			this.groupBoxView.Location = new System.Drawing.Point(8, 54);
			this.groupBoxView.Name = "groupBoxView";
			this.groupBoxView.Size = new System.Drawing.Size(200, 192);
			this.groupBoxView.TabIndex = 0;
			this.groupBoxView.TabStop = false;
			this.groupBoxView.Text = "View setting";
			// 
			// buttonMotorReset
			// 
			this.buttonMotorReset.Location = new System.Drawing.Point(136, 160);
			this.buttonMotorReset.Name = "buttonMotorReset";
			this.buttonMotorReset.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorReset.TabIndex = 25;
			this.buttonMotorReset.UseVisualStyleBackColor = true;
			// 
			// buttonMotorZoomOut
			// 
			this.buttonMotorZoomOut.Location = new System.Drawing.Point(72, 160);
			this.buttonMotorZoomOut.Name = "buttonMotorZoomOut";
			this.buttonMotorZoomOut.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorZoomOut.TabIndex = 24;
			this.buttonMotorZoomOut.UseVisualStyleBackColor = true;
			// 
			// buttonMotorZoomIn
			// 
			this.buttonMotorZoomIn.Location = new System.Drawing.Point(8, 160);
			this.buttonMotorZoomIn.Name = "buttonMotorZoomIn";
			this.buttonMotorZoomIn.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorZoomIn.TabIndex = 23;
			this.buttonMotorZoomIn.UseVisualStyleBackColor = true;
			// 
			// labelMotorMaxVolume
			// 
			this.labelMotorMaxVolume.Location = new System.Drawing.Point(8, 136);
			this.labelMotorMaxVolume.Name = "labelMotorMaxVolume";
			this.labelMotorMaxVolume.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMaxVolume.TabIndex = 14;
			this.labelMotorMaxVolume.Text = "y-max(Volume):";
			this.labelMotorMaxVolume.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMotorMaxVolume
			// 
			this.textBoxMotorMaxVolume.Location = new System.Drawing.Point(120, 136);
			this.textBoxMotorMaxVolume.Name = "textBoxMotorMaxVolume";
			this.textBoxMotorMaxVolume.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMaxVolume.TabIndex = 13;
			// 
			// labelMotorMinVolume
			// 
			this.labelMotorMinVolume.Location = new System.Drawing.Point(8, 112);
			this.labelMotorMinVolume.Name = "labelMotorMinVolume";
			this.labelMotorMinVolume.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMinVolume.TabIndex = 12;
			this.labelMotorMinVolume.Text = "y-min(Volume):";
			this.labelMotorMinVolume.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMotorMinVolume
			// 
			this.textBoxMotorMinVolume.Location = new System.Drawing.Point(120, 112);
			this.textBoxMotorMinVolume.Name = "textBoxMotorMinVolume";
			this.textBoxMotorMinVolume.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMinVolume.TabIndex = 11;
			// 
			// labelMotorMaxPitch
			// 
			this.labelMotorMaxPitch.Location = new System.Drawing.Point(8, 88);
			this.labelMotorMaxPitch.Name = "labelMotorMaxPitch";
			this.labelMotorMaxPitch.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMaxPitch.TabIndex = 10;
			this.labelMotorMaxPitch.Text = "y-max(Pitch):";
			this.labelMotorMaxPitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMotorMaxPitch
			// 
			this.textBoxMotorMaxPitch.Location = new System.Drawing.Point(120, 88);
			this.textBoxMotorMaxPitch.Name = "textBoxMotorMaxPitch";
			this.textBoxMotorMaxPitch.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMaxPitch.TabIndex = 9;
			// 
			// labelMotorMinPitch
			// 
			this.labelMotorMinPitch.Location = new System.Drawing.Point(8, 64);
			this.labelMotorMinPitch.Name = "labelMotorMinPitch";
			this.labelMotorMinPitch.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMinPitch.TabIndex = 8;
			this.labelMotorMinPitch.Text = "y-min(Pitch):";
			this.labelMotorMinPitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMotorMinPitch
			// 
			this.textBoxMotorMinPitch.Location = new System.Drawing.Point(120, 64);
			this.textBoxMotorMinPitch.Name = "textBoxMotorMinPitch";
			this.textBoxMotorMinPitch.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMinPitch.TabIndex = 7;
			// 
			// labelMotorMaxVelocityUnit
			// 
			this.labelMotorMaxVelocityUnit.Location = new System.Drawing.Point(160, 40);
			this.labelMotorMaxVelocityUnit.Name = "labelMotorMaxVelocityUnit";
			this.labelMotorMaxVelocityUnit.Size = new System.Drawing.Size(32, 16);
			this.labelMotorMaxVelocityUnit.TabIndex = 5;
			this.labelMotorMaxVelocityUnit.Text = "km/h";
			this.labelMotorMaxVelocityUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMotorMaxVelocity
			// 
			this.textBoxMotorMaxVelocity.Location = new System.Drawing.Point(120, 40);
			this.textBoxMotorMaxVelocity.Name = "textBoxMotorMaxVelocity";
			this.textBoxMotorMaxVelocity.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMaxVelocity.TabIndex = 4;
			// 
			// labelMotorMaxVelocity
			// 
			this.labelMotorMaxVelocity.Location = new System.Drawing.Point(8, 40);
			this.labelMotorMaxVelocity.Name = "labelMotorMaxVelocity";
			this.labelMotorMaxVelocity.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMaxVelocity.TabIndex = 3;
			this.labelMotorMaxVelocity.Text = "x-max(Velocity):";
			this.labelMotorMaxVelocity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMotorMinVelocityUnit
			// 
			this.labelMotorMinVelocityUnit.Location = new System.Drawing.Point(160, 16);
			this.labelMotorMinVelocityUnit.Name = "labelMotorMinVelocityUnit";
			this.labelMotorMinVelocityUnit.Size = new System.Drawing.Size(32, 16);
			this.labelMotorMinVelocityUnit.TabIndex = 2;
			this.labelMotorMinVelocityUnit.Text = "km/h";
			this.labelMotorMinVelocityUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMotorMinVelocity
			// 
			this.textBoxMotorMinVelocity.Location = new System.Drawing.Point(120, 16);
			this.textBoxMotorMinVelocity.Name = "textBoxMotorMinVelocity";
			this.textBoxMotorMinVelocity.Size = new System.Drawing.Size(32, 19);
			this.textBoxMotorMinVelocity.TabIndex = 1;
			// 
			// labelMotorMinVelocity
			// 
			this.labelMotorMinVelocity.Location = new System.Drawing.Point(8, 16);
			this.labelMotorMinVelocity.Name = "labelMotorMinVelocity";
			this.labelMotorMinVelocity.Size = new System.Drawing.Size(104, 16);
			this.labelMotorMinVelocity.TabIndex = 0;
			this.labelMotorMinVelocity.Text = "x-min(Velocity):";
			this.labelMotorMinVelocity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// statusStripStatus
			// 
			this.statusStripStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelY,
            this.toolStripStatusLabelX,
            this.toolStripStatusLabelTool,
            this.toolStripStatusLabelMode,
            this.toolStripStatusLabelTrack,
            this.toolStripStatusLabelType});
			this.statusStripStatus.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.statusStripStatus.Location = new System.Drawing.Point(0, 642);
			this.statusStripStatus.Name = "statusStripStatus";
			this.statusStripStatus.Size = new System.Drawing.Size(786, 22);
			this.statusStripStatus.TabIndex = 1;
			this.statusStripStatus.Text = "statusStrip1";
			// 
			// toolStripStatusLabelY
			// 
			this.toolStripStatusLabelY.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelY.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelY.Name = "toolStripStatusLabelY";
			this.toolStripStatusLabelY.Size = new System.Drawing.Size(16, 17);
			this.toolStripStatusLabelY.Text = "Y";
			// 
			// toolStripStatusLabelX
			// 
			this.toolStripStatusLabelX.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelX.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelX.Name = "toolStripStatusLabelX";
			this.toolStripStatusLabelX.Size = new System.Drawing.Size(16, 17);
			this.toolStripStatusLabelX.Text = "X";
			// 
			// toolStripStatusLabelTool
			// 
			this.toolStripStatusLabelTool.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelTool.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelTool.Name = "toolStripStatusLabelTool";
			this.toolStripStatusLabelTool.Size = new System.Drawing.Size(31, 17);
			this.toolStripStatusLabelTool.Text = "Tool";
			// 
			// toolStripStatusLabelMode
			// 
			this.toolStripStatusLabelMode.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelMode.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelMode.Name = "toolStripStatusLabelMode";
			this.toolStripStatusLabelMode.Size = new System.Drawing.Size(36, 17);
			this.toolStripStatusLabelMode.Text = "Mode";
			// 
			// toolStripStatusLabelTrack
			// 
			this.toolStripStatusLabelTrack.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelTrack.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelTrack.Name = "toolStripStatusLabelTrack";
			this.toolStripStatusLabelTrack.Size = new System.Drawing.Size(38, 17);
			this.toolStripStatusLabelTrack.Text = "Track";
			// 
			// toolStripStatusLabelType
			// 
			this.toolStripStatusLabelType.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelType.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelType.Name = "toolStripStatusLabelType";
			this.toolStripStatusLabelType.Size = new System.Drawing.Size(34, 17);
			this.toolStripStatusLabelType.Text = "Type";
			// 
			// menuStripMotor
			// 
			this.menuStripMotor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemView,
            this.toolStripMenuItemInput,
            this.toolStripMenuItemTool});
			this.menuStripMotor.Location = new System.Drawing.Point(0, 0);
			this.menuStripMotor.Name = "menuStripMotor";
			this.menuStripMotor.Size = new System.Drawing.Size(786, 24);
			this.menuStripMotor.TabIndex = 0;
			this.menuStripMotor.Text = "menuStrip1";
			// 
			// toolStripMenuItemEdit
			// 
			this.toolStripMenuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUndo,
            this.toolStripMenuItemRedo,
            this.toolStripSeparatorUndo,
            this.toolStripMenuItemTearingOff,
            this.toolStripMenuItemCopy,
            this.toolStripMenuItemPaste,
            this.toolStripMenuItemCleanup,
            this.toolStripMenuItemDelete});
			this.toolStripMenuItemEdit.Name = "toolStripMenuItemEdit";
			this.toolStripMenuItemEdit.Size = new System.Drawing.Size(52, 20);
			this.toolStripMenuItemEdit.Text = "Edit(&E)";
			// 
			// toolStripMenuItemUndo
			// 
			this.toolStripMenuItemUndo.Name = "toolStripMenuItemUndo";
			this.toolStripMenuItemUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.toolStripMenuItemUndo.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemUndo.Text = "Undo(&U)";
			// 
			// toolStripMenuItemRedo
			// 
			this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
			this.toolStripMenuItemRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.toolStripMenuItemRedo.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemRedo.Text = "Redo(&R)";
			// 
			// toolStripSeparatorUndo
			// 
			this.toolStripSeparatorUndo.Name = "toolStripSeparatorUndo";
			this.toolStripSeparatorUndo.Size = new System.Drawing.Size(149, 6);
			// 
			// toolStripMenuItemTearingOff
			// 
			this.toolStripMenuItemTearingOff.Name = "toolStripMenuItemTearingOff";
			this.toolStripMenuItemTearingOff.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.toolStripMenuItemTearingOff.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemTearingOff.Text = "Cut(&T)";
			// 
			// toolStripMenuItemCopy
			// 
			this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
			this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemCopy.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemCopy.Text = "Copy(&C)";
			// 
			// toolStripMenuItemPaste
			// 
			this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
			this.toolStripMenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPaste.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemPaste.Text = "Paste(&P)";
			// 
			// toolStripMenuItemCleanup
			// 
			this.toolStripMenuItemCleanup.Name = "toolStripMenuItemCleanup";
			this.toolStripMenuItemCleanup.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemCleanup.Text = "Cleanup";
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemDelete.Text = "Delete(&D)";
			// 
			// toolStripMenuItemView
			// 
			this.toolStripMenuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemPower,
            this.toolStripMenuItemBrake});
			this.toolStripMenuItemView.Name = "toolStripMenuItemView";
			this.toolStripMenuItemView.Size = new System.Drawing.Size(58, 20);
			this.toolStripMenuItemView.Text = "View(&V)";
			// 
			// toolStripMenuItemPower
			// 
			this.toolStripMenuItemPower.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemPowerTrack1,
            this.toolStripMenuItemPowerTrack2});
			this.toolStripMenuItemPower.Name = "toolStripMenuItemPower";
			this.toolStripMenuItemPower.Size = new System.Drawing.Size(116, 22);
			this.toolStripMenuItemPower.Text = "Power(&P)";
			// 
			// toolStripMenuItemPowerTrack1
			// 
			this.toolStripMenuItemPowerTrack1.Name = "toolStripMenuItemPowerTrack1";
			this.toolStripMenuItemPowerTrack1.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemPowerTrack1.Text = "Track1(&1)";
			// 
			// toolStripMenuItemPowerTrack2
			// 
			this.toolStripMenuItemPowerTrack2.Name = "toolStripMenuItemPowerTrack2";
			this.toolStripMenuItemPowerTrack2.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemPowerTrack2.Text = "Track2(&2)";
			// 
			// toolStripMenuItemBrake
			// 
			this.toolStripMenuItemBrake.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemBrakeTrack1,
            this.toolStripMenuItemBrakeTrack2});
			this.toolStripMenuItemBrake.Name = "toolStripMenuItemBrake";
			this.toolStripMenuItemBrake.Size = new System.Drawing.Size(116, 22);
			this.toolStripMenuItemBrake.Text = "Brake(&B)";
			// 
			// toolStripMenuItemBrakeTrack1
			// 
			this.toolStripMenuItemBrakeTrack1.Name = "toolStripMenuItemBrakeTrack1";
			this.toolStripMenuItemBrakeTrack1.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemBrakeTrack1.Text = "Track1(&1)";
			// 
			// toolStripMenuItemBrakeTrack2
			// 
			this.toolStripMenuItemBrakeTrack2.Name = "toolStripMenuItemBrakeTrack2";
			this.toolStripMenuItemBrakeTrack2.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemBrakeTrack2.Text = "Track2(&2)";
			// 
			// toolStripMenuItemInput
			// 
			this.toolStripMenuItemInput.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemPitch,
            this.toolStripMenuItemVolume,
            this.toolStripMenuItemIndex});
			this.toolStripMenuItemInput.Name = "toolStripMenuItemInput";
			this.toolStripMenuItemInput.Size = new System.Drawing.Size(53, 20);
			this.toolStripMenuItemInput.Text = "Input(I)";
			// 
			// toolStripMenuItemPitch
			// 
			this.toolStripMenuItemPitch.Name = "toolStripMenuItemPitch";
			this.toolStripMenuItemPitch.Size = new System.Drawing.Size(181, 22);
			this.toolStripMenuItemPitch.Text = "Pitch(&P)";
			// 
			// toolStripMenuItemVolume
			// 
			this.toolStripMenuItemVolume.Name = "toolStripMenuItemVolume";
			this.toolStripMenuItemVolume.Size = new System.Drawing.Size(181, 22);
			this.toolStripMenuItemVolume.Text = "Volume(&V)";
			// 
			// toolStripMenuItemIndex
			// 
			this.toolStripMenuItemIndex.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxIndex});
			this.toolStripMenuItemIndex.Name = "toolStripMenuItemIndex";
			this.toolStripMenuItemIndex.Size = new System.Drawing.Size(181, 22);
			this.toolStripMenuItemIndex.Text = "Sound source index(&I)";
			// 
			// toolStripComboBoxIndex
			// 
			this.toolStripComboBoxIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxIndex.Items.AddRange(new object[] {
            "None",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
			this.toolStripComboBoxIndex.Name = "toolStripComboBoxIndex";
			this.toolStripComboBoxIndex.Size = new System.Drawing.Size(121, 20);
			// 
			// toolStripMenuItemTool
			// 
			this.toolStripMenuItemTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelect,
            this.toolStripMenuItemMove,
            this.toolStripMenuItemDot,
            this.toolStripMenuItemLine});
			this.toolStripMenuItemTool.Name = "toolStripMenuItemTool";
			this.toolStripMenuItemTool.Size = new System.Drawing.Size(54, 20);
			this.toolStripMenuItemTool.Text = "Tool(&T)";
			// 
			// toolStripMenuItemSelect
			// 
			this.toolStripMenuItemSelect.Name = "toolStripMenuItemSelect";
			this.toolStripMenuItemSelect.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
			this.toolStripMenuItemSelect.Size = new System.Drawing.Size(151, 22);
			this.toolStripMenuItemSelect.Text = "Select(&S)";
			// 
			// toolStripMenuItemMove
			// 
			this.toolStripMenuItemMove.Name = "toolStripMenuItemMove";
			this.toolStripMenuItemMove.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
			this.toolStripMenuItemMove.Size = new System.Drawing.Size(151, 22);
			this.toolStripMenuItemMove.Text = "Move(&M)";
			// 
			// toolStripMenuItemDot
			// 
			this.toolStripMenuItemDot.Name = "toolStripMenuItemDot";
			this.toolStripMenuItemDot.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
			this.toolStripMenuItemDot.Size = new System.Drawing.Size(151, 22);
			this.toolStripMenuItemDot.Text = "Dot(&D)";
			// 
			// toolStripMenuItemLine
			// 
			this.toolStripMenuItemLine.Name = "toolStripMenuItemLine";
			this.toolStripMenuItemLine.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
			this.toolStripMenuItemLine.Size = new System.Drawing.Size(151, 22);
			this.toolStripMenuItemLine.Text = "Line(&L)";
			// 
			// tabPageCoupler
			// 
			this.tabPageCoupler.Controls.Add(this.groupBoxCouplerGeneral);
			this.tabPageCoupler.Location = new System.Drawing.Point(4, 22);
			this.tabPageCoupler.Name = "tabPageCoupler";
			this.tabPageCoupler.Size = new System.Drawing.Size(792, 670);
			this.tabPageCoupler.TabIndex = 7;
			this.tabPageCoupler.Text = "Coupler settings";
			this.tabPageCoupler.UseVisualStyleBackColor = true;
			// 
			// groupBoxCouplerGeneral
			// 
			this.groupBoxCouplerGeneral.Controls.Add(this.buttonCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMaxUnit);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerMax);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMax);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMinUnit);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerMin);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMin);
			this.groupBoxCouplerGeneral.Location = new System.Drawing.Point(8, 8);
			this.groupBoxCouplerGeneral.Name = "groupBoxCouplerGeneral";
			this.groupBoxCouplerGeneral.Size = new System.Drawing.Size(196, 115);
			this.groupBoxCouplerGeneral.TabIndex = 0;
			this.groupBoxCouplerGeneral.TabStop = false;
			this.groupBoxCouplerGeneral.Text = "General";
			// 
			// buttonCouplerObject
			// 
			this.buttonCouplerObject.Location = new System.Drawing.Point(134, 88);
			this.buttonCouplerObject.Name = "buttonCouplerObject";
			this.buttonCouplerObject.Size = new System.Drawing.Size(56, 19);
			this.buttonCouplerObject.TabIndex = 30;
			this.buttonCouplerObject.Text = "Open...";
			this.buttonCouplerObject.UseVisualStyleBackColor = true;
			this.buttonCouplerObject.Click += new System.EventHandler(this.buttonCouplerObject_Click);
			// 
			// textBoxCouplerObject
			// 
			this.textBoxCouplerObject.Location = new System.Drawing.Point(60, 64);
			this.textBoxCouplerObject.Name = "textBoxCouplerObject";
			this.textBoxCouplerObject.Size = new System.Drawing.Size(130, 19);
			this.textBoxCouplerObject.TabIndex = 29;
			// 
			// labelCouplerObject
			// 
			this.labelCouplerObject.Location = new System.Drawing.Point(8, 64);
			this.labelCouplerObject.Name = "labelCouplerObject";
			this.labelCouplerObject.Size = new System.Drawing.Size(46, 16);
			this.labelCouplerObject.TabIndex = 28;
			this.labelCouplerObject.Text = "Object:";
			this.labelCouplerObject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCouplerMaxUnit
			// 
			this.labelCouplerMaxUnit.Location = new System.Drawing.Point(116, 40);
			this.labelCouplerMaxUnit.Name = "labelCouplerMaxUnit";
			this.labelCouplerMaxUnit.Size = new System.Drawing.Size(16, 16);
			this.labelCouplerMaxUnit.TabIndex = 27;
			this.labelCouplerMaxUnit.Text = "m";
			this.labelCouplerMaxUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxCouplerMax
			// 
			this.textBoxCouplerMax.Location = new System.Drawing.Point(60, 40);
			this.textBoxCouplerMax.Name = "textBoxCouplerMax";
			this.textBoxCouplerMax.Size = new System.Drawing.Size(48, 19);
			this.textBoxCouplerMax.TabIndex = 26;
			// 
			// labelCouplerMax
			// 
			this.labelCouplerMax.Location = new System.Drawing.Point(8, 40);
			this.labelCouplerMax.Name = "labelCouplerMax";
			this.labelCouplerMax.Size = new System.Drawing.Size(32, 16);
			this.labelCouplerMax.TabIndex = 25;
			this.labelCouplerMax.Text = "Max:";
			this.labelCouplerMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCouplerMinUnit
			// 
			this.labelCouplerMinUnit.Location = new System.Drawing.Point(116, 16);
			this.labelCouplerMinUnit.Name = "labelCouplerMinUnit";
			this.labelCouplerMinUnit.Size = new System.Drawing.Size(16, 16);
			this.labelCouplerMinUnit.TabIndex = 24;
			this.labelCouplerMinUnit.Text = "m";
			this.labelCouplerMinUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxCouplerMin
			// 
			this.textBoxCouplerMin.Location = new System.Drawing.Point(60, 16);
			this.textBoxCouplerMin.Name = "textBoxCouplerMin";
			this.textBoxCouplerMin.Size = new System.Drawing.Size(48, 19);
			this.textBoxCouplerMin.TabIndex = 23;
			// 
			// labelCouplerMin
			// 
			this.labelCouplerMin.Location = new System.Drawing.Point(8, 16);
			this.labelCouplerMin.Name = "labelCouplerMin";
			this.labelCouplerMin.Size = new System.Drawing.Size(32, 16);
			this.labelCouplerMin.TabIndex = 22;
			this.labelCouplerMin.Text = "Min:";
			this.labelCouplerMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPagePanel
			// 
			this.tabPagePanel.Controls.Add(this.splitContainerPanel);
			this.tabPagePanel.Controls.Add(this.tabControlPanel);
			this.tabPagePanel.Location = new System.Drawing.Point(4, 22);
			this.tabPagePanel.Name = "tabPagePanel";
			this.tabPagePanel.Size = new System.Drawing.Size(792, 670);
			this.tabPagePanel.TabIndex = 2;
			this.tabPagePanel.Text = "Panel settings";
			this.tabPagePanel.UseVisualStyleBackColor = true;
			// 
			// splitContainerPanel
			// 
			this.splitContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerPanel.Location = new System.Drawing.Point(0, 0);
			this.splitContainerPanel.Name = "splitContainerPanel";
			// 
			// splitContainerPanel.Panel1
			// 
			this.splitContainerPanel.Panel1.Controls.Add(this.treeViewPanel);
			// 
			// splitContainerPanel.Panel2
			// 
			this.splitContainerPanel.Panel2.Controls.Add(this.listViewPanel);
			this.splitContainerPanel.Panel2.Controls.Add(this.panelPanelNavi);
			this.splitContainerPanel.Size = new System.Drawing.Size(472, 670);
			this.splitContainerPanel.SplitterDistance = 157;
			this.splitContainerPanel.TabIndex = 1;
			// 
			// treeViewPanel
			// 
			this.treeViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPanel.HideSelection = false;
			this.treeViewPanel.Location = new System.Drawing.Point(0, 0);
			this.treeViewPanel.Name = "treeViewPanel";
			this.treeViewPanel.Size = new System.Drawing.Size(157, 670);
			this.treeViewPanel.TabIndex = 0;
			// 
			// listViewPanel
			// 
			this.listViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewPanel.FullRowSelect = true;
			this.listViewPanel.HideSelection = false;
			this.listViewPanel.Location = new System.Drawing.Point(0, 0);
			this.listViewPanel.MultiSelect = false;
			this.listViewPanel.Name = "listViewPanel";
			this.listViewPanel.Size = new System.Drawing.Size(311, 598);
			this.listViewPanel.TabIndex = 1;
			this.listViewPanel.UseCompatibleStateImageBehavior = false;
			this.listViewPanel.View = System.Windows.Forms.View.Details;
			// 
			// panelPanelNavi
			// 
			this.panelPanelNavi.Controls.Add(this.panelPanelNaviCmd);
			this.panelPanelNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelPanelNavi.Location = new System.Drawing.Point(0, 598);
			this.panelPanelNavi.Name = "panelPanelNavi";
			this.panelPanelNavi.Size = new System.Drawing.Size(311, 72);
			this.panelPanelNavi.TabIndex = 0;
			// 
			// panelPanelNaviCmd
			// 
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelCopy);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelAdd);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelRemove);
			this.panelPanelNaviCmd.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelPanelNaviCmd.Location = new System.Drawing.Point(176, 0);
			this.panelPanelNaviCmd.Name = "panelPanelNaviCmd";
			this.panelPanelNaviCmd.Size = new System.Drawing.Size(135, 72);
			this.panelPanelNaviCmd.TabIndex = 7;
			// 
			// buttonPanelCopy
			// 
			this.buttonPanelCopy.Location = new System.Drawing.Point(8, 40);
			this.buttonPanelCopy.Name = "buttonPanelCopy";
			this.buttonPanelCopy.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelCopy.TabIndex = 6;
			this.buttonPanelCopy.Text = "Copy";
			this.buttonPanelCopy.UseVisualStyleBackColor = true;
			// 
			// buttonPanelAdd
			// 
			this.buttonPanelAdd.Location = new System.Drawing.Point(8, 8);
			this.buttonPanelAdd.Name = "buttonPanelAdd";
			this.buttonPanelAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelAdd.TabIndex = 4;
			this.buttonPanelAdd.Text = "Add";
			this.buttonPanelAdd.UseVisualStyleBackColor = true;
			// 
			// buttonPanelRemove
			// 
			this.buttonPanelRemove.Location = new System.Drawing.Point(72, 8);
			this.buttonPanelRemove.Name = "buttonPanelRemove";
			this.buttonPanelRemove.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelRemove.TabIndex = 2;
			this.buttonPanelRemove.Text = "Remove";
			this.buttonPanelRemove.UseVisualStyleBackColor = true;
			// 
			// tabControlPanel
			// 
			this.tabControlPanel.Controls.Add(this.tabPageThis);
			this.tabControlPanel.Controls.Add(this.tabPageScreen);
			this.tabControlPanel.Controls.Add(this.tabPagePilotLamp);
			this.tabControlPanel.Controls.Add(this.tabPageNeedle);
			this.tabControlPanel.Controls.Add(this.tabPageDigitalNumber);
			this.tabControlPanel.Controls.Add(this.tabPageDigitalGauge);
			this.tabControlPanel.Controls.Add(this.tabPageLinearGauge);
			this.tabControlPanel.Controls.Add(this.tabPageTimetable);
			this.tabControlPanel.Controls.Add(this.tabPageTouch);
			this.tabControlPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.tabControlPanel.Location = new System.Drawing.Point(472, 0);
			this.tabControlPanel.Multiline = true;
			this.tabControlPanel.Name = "tabControlPanel";
			this.tabControlPanel.SelectedIndex = 0;
			this.tabControlPanel.Size = new System.Drawing.Size(320, 670);
			this.tabControlPanel.TabIndex = 0;
			// 
			// tabPageThis
			// 
			this.tabPageThis.Controls.Add(this.buttonThisTransparentColorSet);
			this.tabPageThis.Controls.Add(this.buttonThisNighttimeImageOpen);
			this.tabPageThis.Controls.Add(this.buttonThisDaytimeImageOpen);
			this.tabPageThis.Controls.Add(this.groupBoxThisOrigin);
			this.tabPageThis.Controls.Add(this.groupBoxThisCenter);
			this.tabPageThis.Controls.Add(this.textBoxThisTransparentColor);
			this.tabPageThis.Controls.Add(this.textBoxThisNighttimeImage);
			this.tabPageThis.Controls.Add(this.textBoxThisDaytimeImage);
			this.tabPageThis.Controls.Add(this.textBoxThisBottom);
			this.tabPageThis.Controls.Add(this.textBoxThisTop);
			this.tabPageThis.Controls.Add(this.textBoxThisRight);
			this.tabPageThis.Controls.Add(this.textBoxThisLeft);
			this.tabPageThis.Controls.Add(this.textBoxThisResolution);
			this.tabPageThis.Controls.Add(this.labelThisResolution);
			this.tabPageThis.Controls.Add(this.labelThisLeft);
			this.tabPageThis.Controls.Add(this.labelThisRight);
			this.tabPageThis.Controls.Add(this.labelThisTop);
			this.tabPageThis.Controls.Add(this.labelThisBottom);
			this.tabPageThis.Controls.Add(this.labelThisDaytimeImage);
			this.tabPageThis.Controls.Add(this.labelThisNighttimeImage);
			this.tabPageThis.Controls.Add(this.labelThisTransparentColor);
			this.tabPageThis.Location = new System.Drawing.Point(4, 40);
			this.tabPageThis.Name = "tabPageThis";
			this.tabPageThis.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageThis.Size = new System.Drawing.Size(312, 626);
			this.tabPageThis.TabIndex = 0;
			this.tabPageThis.Text = "This";
			this.tabPageThis.UseVisualStyleBackColor = true;
			// 
			// buttonThisTransparentColorSet
			// 
			this.buttonThisTransparentColorSet.Location = new System.Drawing.Point(248, 176);
			this.buttonThisTransparentColorSet.Name = "buttonThisTransparentColorSet";
			this.buttonThisTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonThisTransparentColorSet.TabIndex = 36;
			this.buttonThisTransparentColorSet.Text = "Set...";
			this.buttonThisTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonThisTransparentColorSet.Click += new System.EventHandler(this.ButtonThisTransparentColorSet_Click);
			// 
			// buttonThisNighttimeImageOpen
			// 
			this.buttonThisNighttimeImageOpen.Location = new System.Drawing.Point(248, 152);
			this.buttonThisNighttimeImageOpen.Name = "buttonThisNighttimeImageOpen";
			this.buttonThisNighttimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonThisNighttimeImageOpen.TabIndex = 35;
			this.buttonThisNighttimeImageOpen.Text = "Open...";
			this.buttonThisNighttimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonThisNighttimeImageOpen.Click += new System.EventHandler(this.ButtonThisNighttimeImageOpen_Click);
			// 
			// buttonThisDaytimeImageOpen
			// 
			this.buttonThisDaytimeImageOpen.Location = new System.Drawing.Point(248, 128);
			this.buttonThisDaytimeImageOpen.Name = "buttonThisDaytimeImageOpen";
			this.buttonThisDaytimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonThisDaytimeImageOpen.TabIndex = 34;
			this.buttonThisDaytimeImageOpen.Text = "Open...";
			this.buttonThisDaytimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonThisDaytimeImageOpen.Click += new System.EventHandler(this.ButtonThisDaytimeImageOpen_Click);
			// 
			// groupBoxThisOrigin
			// 
			this.groupBoxThisOrigin.Controls.Add(this.textBoxThisOriginY);
			this.groupBoxThisOrigin.Controls.Add(this.textBoxThisOriginX);
			this.groupBoxThisOrigin.Controls.Add(this.labelThisOriginY);
			this.groupBoxThisOrigin.Controls.Add(this.labelThisOriginX);
			this.groupBoxThisOrigin.Location = new System.Drawing.Point(8, 280);
			this.groupBoxThisOrigin.Name = "groupBoxThisOrigin";
			this.groupBoxThisOrigin.Size = new System.Drawing.Size(184, 72);
			this.groupBoxThisOrigin.TabIndex = 33;
			this.groupBoxThisOrigin.TabStop = false;
			this.groupBoxThisOrigin.Text = "Origin";
			// 
			// textBoxThisOriginY
			// 
			this.textBoxThisOriginY.Location = new System.Drawing.Point(128, 40);
			this.textBoxThisOriginY.Name = "textBoxThisOriginY";
			this.textBoxThisOriginY.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisOriginY.TabIndex = 33;
			// 
			// textBoxThisOriginX
			// 
			this.textBoxThisOriginX.Location = new System.Drawing.Point(128, 16);
			this.textBoxThisOriginX.Name = "textBoxThisOriginX";
			this.textBoxThisOriginX.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisOriginX.TabIndex = 32;
			// 
			// labelThisOriginY
			// 
			this.labelThisOriginY.Location = new System.Drawing.Point(8, 40);
			this.labelThisOriginY.Name = "labelThisOriginY";
			this.labelThisOriginY.Size = new System.Drawing.Size(112, 16);
			this.labelThisOriginY.TabIndex = 10;
			this.labelThisOriginY.Text = "Y:";
			this.labelThisOriginY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisOriginX
			// 
			this.labelThisOriginX.Location = new System.Drawing.Point(8, 16);
			this.labelThisOriginX.Name = "labelThisOriginX";
			this.labelThisOriginX.Size = new System.Drawing.Size(112, 16);
			this.labelThisOriginX.TabIndex = 9;
			this.labelThisOriginX.Text = "X:";
			this.labelThisOriginX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxThisCenter
			// 
			this.groupBoxThisCenter.Controls.Add(this.textBoxThisCenterY);
			this.groupBoxThisCenter.Controls.Add(this.textBoxThisCenterX);
			this.groupBoxThisCenter.Controls.Add(this.labelThisCenterY);
			this.groupBoxThisCenter.Controls.Add(this.labelThisCenterX);
			this.groupBoxThisCenter.Location = new System.Drawing.Point(8, 200);
			this.groupBoxThisCenter.Name = "groupBoxThisCenter";
			this.groupBoxThisCenter.Size = new System.Drawing.Size(184, 72);
			this.groupBoxThisCenter.TabIndex = 32;
			this.groupBoxThisCenter.TabStop = false;
			this.groupBoxThisCenter.Text = "Center";
			// 
			// textBoxThisCenterY
			// 
			this.textBoxThisCenterY.Location = new System.Drawing.Point(128, 40);
			this.textBoxThisCenterY.Name = "textBoxThisCenterY";
			this.textBoxThisCenterY.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisCenterY.TabIndex = 33;
			// 
			// textBoxThisCenterX
			// 
			this.textBoxThisCenterX.Location = new System.Drawing.Point(128, 16);
			this.textBoxThisCenterX.Name = "textBoxThisCenterX";
			this.textBoxThisCenterX.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisCenterX.TabIndex = 32;
			// 
			// labelThisCenterY
			// 
			this.labelThisCenterY.Location = new System.Drawing.Point(8, 40);
			this.labelThisCenterY.Name = "labelThisCenterY";
			this.labelThisCenterY.Size = new System.Drawing.Size(112, 16);
			this.labelThisCenterY.TabIndex = 10;
			this.labelThisCenterY.Text = "Y:";
			this.labelThisCenterY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisCenterX
			// 
			this.labelThisCenterX.Location = new System.Drawing.Point(8, 16);
			this.labelThisCenterX.Name = "labelThisCenterX";
			this.labelThisCenterX.Size = new System.Drawing.Size(112, 16);
			this.labelThisCenterX.TabIndex = 9;
			this.labelThisCenterX.Text = "X:";
			this.labelThisCenterX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxThisTransparentColor
			// 
			this.textBoxThisTransparentColor.Location = new System.Drawing.Point(136, 176);
			this.textBoxThisTransparentColor.Name = "textBoxThisTransparentColor";
			this.textBoxThisTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxThisTransparentColor.TabIndex = 31;
			// 
			// textBoxThisNighttimeImage
			// 
			this.textBoxThisNighttimeImage.Location = new System.Drawing.Point(136, 152);
			this.textBoxThisNighttimeImage.Name = "textBoxThisNighttimeImage";
			this.textBoxThisNighttimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxThisNighttimeImage.TabIndex = 30;
			// 
			// textBoxThisDaytimeImage
			// 
			this.textBoxThisDaytimeImage.Location = new System.Drawing.Point(136, 128);
			this.textBoxThisDaytimeImage.Name = "textBoxThisDaytimeImage";
			this.textBoxThisDaytimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxThisDaytimeImage.TabIndex = 29;
			// 
			// textBoxThisBottom
			// 
			this.textBoxThisBottom.Location = new System.Drawing.Point(136, 104);
			this.textBoxThisBottom.Name = "textBoxThisBottom";
			this.textBoxThisBottom.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisBottom.TabIndex = 28;
			// 
			// textBoxThisTop
			// 
			this.textBoxThisTop.Location = new System.Drawing.Point(136, 80);
			this.textBoxThisTop.Name = "textBoxThisTop";
			this.textBoxThisTop.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisTop.TabIndex = 27;
			// 
			// textBoxThisRight
			// 
			this.textBoxThisRight.Location = new System.Drawing.Point(136, 56);
			this.textBoxThisRight.Name = "textBoxThisRight";
			this.textBoxThisRight.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisRight.TabIndex = 26;
			// 
			// textBoxThisLeft
			// 
			this.textBoxThisLeft.Location = new System.Drawing.Point(136, 32);
			this.textBoxThisLeft.Name = "textBoxThisLeft";
			this.textBoxThisLeft.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisLeft.TabIndex = 25;
			// 
			// textBoxThisResolution
			// 
			this.textBoxThisResolution.Location = new System.Drawing.Point(136, 8);
			this.textBoxThisResolution.Name = "textBoxThisResolution";
			this.textBoxThisResolution.Size = new System.Drawing.Size(48, 19);
			this.textBoxThisResolution.TabIndex = 24;
			// 
			// labelThisResolution
			// 
			this.labelThisResolution.Location = new System.Drawing.Point(8, 8);
			this.labelThisResolution.Name = "labelThisResolution";
			this.labelThisResolution.Size = new System.Drawing.Size(120, 16);
			this.labelThisResolution.TabIndex = 9;
			this.labelThisResolution.Text = "Resolution:";
			this.labelThisResolution.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisLeft
			// 
			this.labelThisLeft.Location = new System.Drawing.Point(8, 32);
			this.labelThisLeft.Name = "labelThisLeft";
			this.labelThisLeft.Size = new System.Drawing.Size(120, 16);
			this.labelThisLeft.TabIndex = 8;
			this.labelThisLeft.Text = "Left:";
			this.labelThisLeft.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisRight
			// 
			this.labelThisRight.Location = new System.Drawing.Point(8, 56);
			this.labelThisRight.Name = "labelThisRight";
			this.labelThisRight.Size = new System.Drawing.Size(120, 16);
			this.labelThisRight.TabIndex = 7;
			this.labelThisRight.Text = "Right:";
			this.labelThisRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisTop
			// 
			this.labelThisTop.Location = new System.Drawing.Point(8, 80);
			this.labelThisTop.Name = "labelThisTop";
			this.labelThisTop.Size = new System.Drawing.Size(120, 16);
			this.labelThisTop.TabIndex = 6;
			this.labelThisTop.Text = "Top:";
			this.labelThisTop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisBottom
			// 
			this.labelThisBottom.Location = new System.Drawing.Point(8, 104);
			this.labelThisBottom.Name = "labelThisBottom";
			this.labelThisBottom.Size = new System.Drawing.Size(120, 16);
			this.labelThisBottom.TabIndex = 5;
			this.labelThisBottom.Text = "Bottom:";
			this.labelThisBottom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisDaytimeImage
			// 
			this.labelThisDaytimeImage.Location = new System.Drawing.Point(8, 128);
			this.labelThisDaytimeImage.Name = "labelThisDaytimeImage";
			this.labelThisDaytimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelThisDaytimeImage.TabIndex = 4;
			this.labelThisDaytimeImage.Text = "DaytimeImage:";
			this.labelThisDaytimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisNighttimeImage
			// 
			this.labelThisNighttimeImage.Location = new System.Drawing.Point(8, 152);
			this.labelThisNighttimeImage.Name = "labelThisNighttimeImage";
			this.labelThisNighttimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelThisNighttimeImage.TabIndex = 3;
			this.labelThisNighttimeImage.Text = "NighttimeImage:";
			this.labelThisNighttimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelThisTransparentColor
			// 
			this.labelThisTransparentColor.Location = new System.Drawing.Point(8, 176);
			this.labelThisTransparentColor.Name = "labelThisTransparentColor";
			this.labelThisTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelThisTransparentColor.TabIndex = 2;
			this.labelThisTransparentColor.Text = "TransparentColor:";
			this.labelThisTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageScreen
			// 
			this.tabPageScreen.Controls.Add(this.numericUpDownScreenLayer);
			this.tabPageScreen.Controls.Add(this.numericUpDownScreenNumber);
			this.tabPageScreen.Controls.Add(this.labelScreenLayer);
			this.tabPageScreen.Controls.Add(this.labelScreenNumber);
			this.tabPageScreen.Location = new System.Drawing.Point(4, 40);
			this.tabPageScreen.Name = "tabPageScreen";
			this.tabPageScreen.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageScreen.Size = new System.Drawing.Size(312, 625);
			this.tabPageScreen.TabIndex = 1;
			this.tabPageScreen.Text = "Screen";
			this.tabPageScreen.UseVisualStyleBackColor = true;
			// 
			// numericUpDownScreenLayer
			// 
			this.numericUpDownScreenLayer.Location = new System.Drawing.Point(136, 32);
			this.numericUpDownScreenLayer.Name = "numericUpDownScreenLayer";
			this.numericUpDownScreenLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownScreenLayer.TabIndex = 31;
			// 
			// numericUpDownScreenNumber
			// 
			this.numericUpDownScreenNumber.Location = new System.Drawing.Point(136, 8);
			this.numericUpDownScreenNumber.Name = "numericUpDownScreenNumber";
			this.numericUpDownScreenNumber.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownScreenNumber.TabIndex = 30;
			// 
			// labelScreenLayer
			// 
			this.labelScreenLayer.Location = new System.Drawing.Point(8, 32);
			this.labelScreenLayer.Name = "labelScreenLayer";
			this.labelScreenLayer.Size = new System.Drawing.Size(120, 16);
			this.labelScreenLayer.TabIndex = 28;
			this.labelScreenLayer.Text = "Layer:";
			this.labelScreenLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelScreenNumber
			// 
			this.labelScreenNumber.Location = new System.Drawing.Point(8, 8);
			this.labelScreenNumber.Name = "labelScreenNumber";
			this.labelScreenNumber.Size = new System.Drawing.Size(120, 16);
			this.labelScreenNumber.TabIndex = 26;
			this.labelScreenNumber.Text = "Number:";
			this.labelScreenNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPagePilotLamp
			// 
			this.tabPagePilotLamp.Controls.Add(this.buttonPilotLampTransparentColorSet);
			this.tabPagePilotLamp.Controls.Add(this.numericUpDownPilotLampLayer);
			this.tabPagePilotLamp.Controls.Add(this.groupBoxPilotLampLocation);
			this.tabPagePilotLamp.Controls.Add(this.buttonPilotLampSubjectSet);
			this.tabPagePilotLamp.Controls.Add(this.labelPilotLampSubject);
			this.tabPagePilotLamp.Controls.Add(this.labelPilotLampLayer);
			this.tabPagePilotLamp.Controls.Add(this.buttonPilotLampNighttimeImageOpen);
			this.tabPagePilotLamp.Controls.Add(this.buttonPilotLampDaytimeImageOpen);
			this.tabPagePilotLamp.Controls.Add(this.textBoxPilotLampTransparentColor);
			this.tabPagePilotLamp.Controls.Add(this.textBoxPilotLampNighttimeImage);
			this.tabPagePilotLamp.Controls.Add(this.textBoxPilotLampDaytimeImage);
			this.tabPagePilotLamp.Controls.Add(this.labelPilotLampDaytimeImage);
			this.tabPagePilotLamp.Controls.Add(this.labelPilotLampNighttimeImage);
			this.tabPagePilotLamp.Controls.Add(this.labelPilotLampTransparentColor);
			this.tabPagePilotLamp.Location = new System.Drawing.Point(4, 40);
			this.tabPagePilotLamp.Name = "tabPagePilotLamp";
			this.tabPagePilotLamp.Size = new System.Drawing.Size(312, 625);
			this.tabPagePilotLamp.TabIndex = 2;
			this.tabPagePilotLamp.Text = "PilotLamp";
			this.tabPagePilotLamp.UseVisualStyleBackColor = true;
			// 
			// buttonPilotLampTransparentColorSet
			// 
			this.buttonPilotLampTransparentColorSet.Location = new System.Drawing.Point(248, 160);
			this.buttonPilotLampTransparentColorSet.Name = "buttonPilotLampTransparentColorSet";
			this.buttonPilotLampTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonPilotLampTransparentColorSet.TabIndex = 49;
			this.buttonPilotLampTransparentColorSet.Text = "Set...";
			this.buttonPilotLampTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonPilotLampTransparentColorSet.Click += new System.EventHandler(this.ButtonPilotLampTransparentColorSet_Click);
			// 
			// numericUpDownPilotLampLayer
			// 
			this.numericUpDownPilotLampLayer.Location = new System.Drawing.Point(136, 184);
			this.numericUpDownPilotLampLayer.Name = "numericUpDownPilotLampLayer";
			this.numericUpDownPilotLampLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownPilotLampLayer.TabIndex = 34;
			// 
			// groupBoxPilotLampLocation
			// 
			this.groupBoxPilotLampLocation.Controls.Add(this.textBoxPilotLampLocationY);
			this.groupBoxPilotLampLocation.Controls.Add(this.textBoxPilotLampLocationX);
			this.groupBoxPilotLampLocation.Controls.Add(this.labelPilotLampLocationY);
			this.groupBoxPilotLampLocation.Controls.Add(this.labelPilotLampLocationX);
			this.groupBoxPilotLampLocation.Location = new System.Drawing.Point(8, 32);
			this.groupBoxPilotLampLocation.Name = "groupBoxPilotLampLocation";
			this.groupBoxPilotLampLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxPilotLampLocation.TabIndex = 48;
			this.groupBoxPilotLampLocation.TabStop = false;
			this.groupBoxPilotLampLocation.Text = "Location";
			// 
			// textBoxPilotLampLocationY
			// 
			this.textBoxPilotLampLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxPilotLampLocationY.Name = "textBoxPilotLampLocationY";
			this.textBoxPilotLampLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxPilotLampLocationY.TabIndex = 33;
			// 
			// textBoxPilotLampLocationX
			// 
			this.textBoxPilotLampLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxPilotLampLocationX.Name = "textBoxPilotLampLocationX";
			this.textBoxPilotLampLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxPilotLampLocationX.TabIndex = 32;
			// 
			// labelPilotLampLocationY
			// 
			this.labelPilotLampLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelPilotLampLocationY.Name = "labelPilotLampLocationY";
			this.labelPilotLampLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelPilotLampLocationY.TabIndex = 10;
			this.labelPilotLampLocationY.Text = "Y:";
			this.labelPilotLampLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPilotLampLocationX
			// 
			this.labelPilotLampLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelPilotLampLocationX.Name = "labelPilotLampLocationX";
			this.labelPilotLampLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelPilotLampLocationX.TabIndex = 9;
			this.labelPilotLampLocationX.Text = "X:";
			this.labelPilotLampLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonPilotLampSubjectSet
			// 
			this.buttonPilotLampSubjectSet.Location = new System.Drawing.Point(136, 8);
			this.buttonPilotLampSubjectSet.Name = "buttonPilotLampSubjectSet";
			this.buttonPilotLampSubjectSet.Size = new System.Drawing.Size(48, 19);
			this.buttonPilotLampSubjectSet.TabIndex = 47;
			this.buttonPilotLampSubjectSet.Text = "Set...";
			this.buttonPilotLampSubjectSet.UseVisualStyleBackColor = true;
			this.buttonPilotLampSubjectSet.Click += new System.EventHandler(this.ButtonPilotLampSubjectSet_Click);
			// 
			// labelPilotLampSubject
			// 
			this.labelPilotLampSubject.Location = new System.Drawing.Point(8, 8);
			this.labelPilotLampSubject.Name = "labelPilotLampSubject";
			this.labelPilotLampSubject.Size = new System.Drawing.Size(120, 16);
			this.labelPilotLampSubject.TabIndex = 46;
			this.labelPilotLampSubject.Text = "Subject:";
			this.labelPilotLampSubject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPilotLampLayer
			// 
			this.labelPilotLampLayer.Location = new System.Drawing.Point(8, 184);
			this.labelPilotLampLayer.Name = "labelPilotLampLayer";
			this.labelPilotLampLayer.Size = new System.Drawing.Size(120, 16);
			this.labelPilotLampLayer.TabIndex = 44;
			this.labelPilotLampLayer.Text = "Layer:";
			this.labelPilotLampLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonPilotLampNighttimeImageOpen
			// 
			this.buttonPilotLampNighttimeImageOpen.Location = new System.Drawing.Point(248, 136);
			this.buttonPilotLampNighttimeImageOpen.Name = "buttonPilotLampNighttimeImageOpen";
			this.buttonPilotLampNighttimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonPilotLampNighttimeImageOpen.TabIndex = 43;
			this.buttonPilotLampNighttimeImageOpen.Text = "Open...";
			this.buttonPilotLampNighttimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonPilotLampNighttimeImageOpen.Click += new System.EventHandler(this.ButtonPilotLampNighttimeImageOpen_Click);
			// 
			// buttonPilotLampDaytimeImageOpen
			// 
			this.buttonPilotLampDaytimeImageOpen.Location = new System.Drawing.Point(248, 112);
			this.buttonPilotLampDaytimeImageOpen.Name = "buttonPilotLampDaytimeImageOpen";
			this.buttonPilotLampDaytimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonPilotLampDaytimeImageOpen.TabIndex = 42;
			this.buttonPilotLampDaytimeImageOpen.Text = "Open...";
			this.buttonPilotLampDaytimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonPilotLampDaytimeImageOpen.Click += new System.EventHandler(this.ButtonPilotLampDaytimeImageOpen_Click);
			// 
			// textBoxPilotLampTransparentColor
			// 
			this.textBoxPilotLampTransparentColor.Location = new System.Drawing.Point(136, 160);
			this.textBoxPilotLampTransparentColor.Name = "textBoxPilotLampTransparentColor";
			this.textBoxPilotLampTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxPilotLampTransparentColor.TabIndex = 41;
			// 
			// textBoxPilotLampNighttimeImage
			// 
			this.textBoxPilotLampNighttimeImage.Location = new System.Drawing.Point(136, 136);
			this.textBoxPilotLampNighttimeImage.Name = "textBoxPilotLampNighttimeImage";
			this.textBoxPilotLampNighttimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxPilotLampNighttimeImage.TabIndex = 40;
			// 
			// textBoxPilotLampDaytimeImage
			// 
			this.textBoxPilotLampDaytimeImage.Location = new System.Drawing.Point(136, 112);
			this.textBoxPilotLampDaytimeImage.Name = "textBoxPilotLampDaytimeImage";
			this.textBoxPilotLampDaytimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxPilotLampDaytimeImage.TabIndex = 39;
			// 
			// labelPilotLampDaytimeImage
			// 
			this.labelPilotLampDaytimeImage.Location = new System.Drawing.Point(8, 112);
			this.labelPilotLampDaytimeImage.Name = "labelPilotLampDaytimeImage";
			this.labelPilotLampDaytimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelPilotLampDaytimeImage.TabIndex = 38;
			this.labelPilotLampDaytimeImage.Text = "DaytimeImage:";
			this.labelPilotLampDaytimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPilotLampNighttimeImage
			// 
			this.labelPilotLampNighttimeImage.Location = new System.Drawing.Point(8, 136);
			this.labelPilotLampNighttimeImage.Name = "labelPilotLampNighttimeImage";
			this.labelPilotLampNighttimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelPilotLampNighttimeImage.TabIndex = 37;
			this.labelPilotLampNighttimeImage.Text = "NighttimeImage:";
			this.labelPilotLampNighttimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPilotLampTransparentColor
			// 
			this.labelPilotLampTransparentColor.Location = new System.Drawing.Point(8, 160);
			this.labelPilotLampTransparentColor.Name = "labelPilotLampTransparentColor";
			this.labelPilotLampTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelPilotLampTransparentColor.TabIndex = 36;
			this.labelPilotLampTransparentColor.Text = "TransparentColor:";
			this.labelPilotLampTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageNeedle
			// 
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleDefinedDampingRatio);
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleDefinedNaturalFreq);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDefinedDampingRatio);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDefinedNaturalFreq);
			this.tabPageNeedle.Controls.Add(this.buttonNeedleTransparentColorSet);
			this.tabPageNeedle.Controls.Add(this.buttonNeedleColorSet);
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleDefinedOrigin);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDefinedOrigin);
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleDefinedRadius);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDefinedRadius);
			this.tabPageNeedle.Controls.Add(this.numericUpDownNeedleLayer);
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleSmoothed);
			this.tabPageNeedle.Controls.Add(this.labelNeedleSmoothed);
			this.tabPageNeedle.Controls.Add(this.checkBoxNeedleBackstop);
			this.tabPageNeedle.Controls.Add(this.labelNeedleBackstop);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleDampingRatio);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDampingRatio);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleNaturalFreq);
			this.tabPageNeedle.Controls.Add(this.labelNeedleNaturalFreq);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleMaximum);
			this.tabPageNeedle.Controls.Add(this.labelNeedleMaximum);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleMinimum);
			this.tabPageNeedle.Controls.Add(this.labelNeedleMinimum);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleLastAngle);
			this.tabPageNeedle.Controls.Add(this.labelNeedleLastAngle);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleInitialAngle);
			this.tabPageNeedle.Controls.Add(this.labelNeedleInitialAngle);
			this.tabPageNeedle.Controls.Add(this.groupBoxNeedleOrigin);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleColor);
			this.tabPageNeedle.Controls.Add(this.labelNeedleColor);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleRadius);
			this.tabPageNeedle.Controls.Add(this.labelNeedleRadius);
			this.tabPageNeedle.Controls.Add(this.groupBoxNeedleLocation);
			this.tabPageNeedle.Controls.Add(this.buttonNeedleSubjectSet);
			this.tabPageNeedle.Controls.Add(this.labelNeedleSubject);
			this.tabPageNeedle.Controls.Add(this.labelNeedleLayer);
			this.tabPageNeedle.Controls.Add(this.buttonNeedleNighttimeImageOpen);
			this.tabPageNeedle.Controls.Add(this.buttonNeedleDaytimeImageOpen);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleTransparentColor);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleNighttimeImage);
			this.tabPageNeedle.Controls.Add(this.textBoxNeedleDaytimeImage);
			this.tabPageNeedle.Controls.Add(this.labelNeedleDaytimeImage);
			this.tabPageNeedle.Controls.Add(this.labelNeedleNighttimeImage);
			this.tabPageNeedle.Controls.Add(this.labelNeedleTransparentColor);
			this.tabPageNeedle.Location = new System.Drawing.Point(4, 40);
			this.tabPageNeedle.Name = "tabPageNeedle";
			this.tabPageNeedle.Size = new System.Drawing.Size(312, 625);
			this.tabPageNeedle.TabIndex = 3;
			this.tabPageNeedle.Text = "Needle";
			this.tabPageNeedle.UseVisualStyleBackColor = true;
			// 
			// checkBoxNeedleDefinedDampingRatio
			// 
			this.checkBoxNeedleDefinedDampingRatio.Location = new System.Drawing.Point(136, 504);
			this.checkBoxNeedleDefinedDampingRatio.Name = "checkBoxNeedleDefinedDampingRatio";
			this.checkBoxNeedleDefinedDampingRatio.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleDefinedDampingRatio.TabIndex = 93;
			this.checkBoxNeedleDefinedDampingRatio.UseVisualStyleBackColor = true;
			// 
			// checkBoxNeedleDefinedNaturalFreq
			// 
			this.checkBoxNeedleDefinedNaturalFreq.Location = new System.Drawing.Point(136, 456);
			this.checkBoxNeedleDefinedNaturalFreq.Name = "checkBoxNeedleDefinedNaturalFreq";
			this.checkBoxNeedleDefinedNaturalFreq.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleDefinedNaturalFreq.TabIndex = 92;
			this.checkBoxNeedleDefinedNaturalFreq.UseVisualStyleBackColor = true;
			// 
			// labelNeedleDefinedDampingRatio
			// 
			this.labelNeedleDefinedDampingRatio.Location = new System.Drawing.Point(8, 504);
			this.labelNeedleDefinedDampingRatio.Name = "labelNeedleDefinedDampingRatio";
			this.labelNeedleDefinedDampingRatio.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleDefinedDampingRatio.TabIndex = 91;
			this.labelNeedleDefinedDampingRatio.Text = "DefinedDampingRatio:";
			this.labelNeedleDefinedDampingRatio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleDefinedNaturalFreq
			// 
			this.labelNeedleDefinedNaturalFreq.Location = new System.Drawing.Point(16, 456);
			this.labelNeedleDefinedNaturalFreq.Name = "labelNeedleDefinedNaturalFreq";
			this.labelNeedleDefinedNaturalFreq.Size = new System.Drawing.Size(112, 16);
			this.labelNeedleDefinedNaturalFreq.TabIndex = 90;
			this.labelNeedleDefinedNaturalFreq.Text = "DefinedNaturalFreq:";
			this.labelNeedleDefinedNaturalFreq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonNeedleTransparentColorSet
			// 
			this.buttonNeedleTransparentColorSet.Location = new System.Drawing.Point(248, 232);
			this.buttonNeedleTransparentColorSet.Name = "buttonNeedleTransparentColorSet";
			this.buttonNeedleTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonNeedleTransparentColorSet.TabIndex = 89;
			this.buttonNeedleTransparentColorSet.Text = "Set...";
			this.buttonNeedleTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonNeedleTransparentColorSet.Click += new System.EventHandler(this.ButtonNeedleTransparentColorSet_Click);
			// 
			// buttonNeedleColorSet
			// 
			this.buttonNeedleColorSet.Location = new System.Drawing.Point(248, 208);
			this.buttonNeedleColorSet.Name = "buttonNeedleColorSet";
			this.buttonNeedleColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonNeedleColorSet.TabIndex = 88;
			this.buttonNeedleColorSet.Text = "Set...";
			this.buttonNeedleColorSet.UseVisualStyleBackColor = true;
			this.buttonNeedleColorSet.Click += new System.EventHandler(this.ButtonNeedleColorSet_Click);
			// 
			// checkBoxNeedleDefinedOrigin
			// 
			this.checkBoxNeedleDefinedOrigin.Location = new System.Drawing.Point(136, 256);
			this.checkBoxNeedleDefinedOrigin.Name = "checkBoxNeedleDefinedOrigin";
			this.checkBoxNeedleDefinedOrigin.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleDefinedOrigin.TabIndex = 87;
			this.checkBoxNeedleDefinedOrigin.UseVisualStyleBackColor = true;
			// 
			// labelNeedleDefinedOrigin
			// 
			this.labelNeedleDefinedOrigin.Location = new System.Drawing.Point(8, 256);
			this.labelNeedleDefinedOrigin.Name = "labelNeedleDefinedOrigin";
			this.labelNeedleDefinedOrigin.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleDefinedOrigin.TabIndex = 86;
			this.labelNeedleDefinedOrigin.Text = "DefinedOrigin:";
			this.labelNeedleDefinedOrigin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxNeedleDefinedRadius
			// 
			this.checkBoxNeedleDefinedRadius.Location = new System.Drawing.Point(136, 112);
			this.checkBoxNeedleDefinedRadius.Name = "checkBoxNeedleDefinedRadius";
			this.checkBoxNeedleDefinedRadius.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleDefinedRadius.TabIndex = 85;
			this.checkBoxNeedleDefinedRadius.UseVisualStyleBackColor = true;
			// 
			// labelNeedleDefinedRadius
			// 
			this.labelNeedleDefinedRadius.Location = new System.Drawing.Point(8, 112);
			this.labelNeedleDefinedRadius.Name = "labelNeedleDefinedRadius";
			this.labelNeedleDefinedRadius.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleDefinedRadius.TabIndex = 84;
			this.labelNeedleDefinedRadius.Text = "DefinedRadius:";
			this.labelNeedleDefinedRadius.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownNeedleLayer
			// 
			this.numericUpDownNeedleLayer.Location = new System.Drawing.Point(136, 600);
			this.numericUpDownNeedleLayer.Name = "numericUpDownNeedleLayer";
			this.numericUpDownNeedleLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownNeedleLayer.TabIndex = 83;
			// 
			// checkBoxNeedleSmoothed
			// 
			this.checkBoxNeedleSmoothed.Location = new System.Drawing.Point(136, 576);
			this.checkBoxNeedleSmoothed.Name = "checkBoxNeedleSmoothed";
			this.checkBoxNeedleSmoothed.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleSmoothed.TabIndex = 82;
			this.checkBoxNeedleSmoothed.UseVisualStyleBackColor = true;
			// 
			// labelNeedleSmoothed
			// 
			this.labelNeedleSmoothed.Location = new System.Drawing.Point(8, 576);
			this.labelNeedleSmoothed.Name = "labelNeedleSmoothed";
			this.labelNeedleSmoothed.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleSmoothed.TabIndex = 81;
			this.labelNeedleSmoothed.Text = "Smoothed:";
			this.labelNeedleSmoothed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxNeedleBackstop
			// 
			this.checkBoxNeedleBackstop.Location = new System.Drawing.Point(136, 552);
			this.checkBoxNeedleBackstop.Name = "checkBoxNeedleBackstop";
			this.checkBoxNeedleBackstop.Size = new System.Drawing.Size(48, 16);
			this.checkBoxNeedleBackstop.TabIndex = 80;
			this.checkBoxNeedleBackstop.UseVisualStyleBackColor = true;
			// 
			// labelNeedleBackstop
			// 
			this.labelNeedleBackstop.Location = new System.Drawing.Point(8, 552);
			this.labelNeedleBackstop.Name = "labelNeedleBackstop";
			this.labelNeedleBackstop.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleBackstop.TabIndex = 79;
			this.labelNeedleBackstop.Text = "Backstop:";
			this.labelNeedleBackstop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleDampingRatio
			// 
			this.textBoxNeedleDampingRatio.Location = new System.Drawing.Point(136, 528);
			this.textBoxNeedleDampingRatio.Name = "textBoxNeedleDampingRatio";
			this.textBoxNeedleDampingRatio.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleDampingRatio.TabIndex = 78;
			// 
			// labelNeedleDampingRatio
			// 
			this.labelNeedleDampingRatio.Location = new System.Drawing.Point(8, 528);
			this.labelNeedleDampingRatio.Name = "labelNeedleDampingRatio";
			this.labelNeedleDampingRatio.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleDampingRatio.TabIndex = 77;
			this.labelNeedleDampingRatio.Text = "DampingRatio:";
			this.labelNeedleDampingRatio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleNaturalFreq
			// 
			this.textBoxNeedleNaturalFreq.Location = new System.Drawing.Point(136, 480);
			this.textBoxNeedleNaturalFreq.Name = "textBoxNeedleNaturalFreq";
			this.textBoxNeedleNaturalFreq.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleNaturalFreq.TabIndex = 76;
			// 
			// labelNeedleNaturalFreq
			// 
			this.labelNeedleNaturalFreq.Location = new System.Drawing.Point(8, 480);
			this.labelNeedleNaturalFreq.Name = "labelNeedleNaturalFreq";
			this.labelNeedleNaturalFreq.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleNaturalFreq.TabIndex = 75;
			this.labelNeedleNaturalFreq.Text = "NaturalFreq:";
			this.labelNeedleNaturalFreq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleMaximum
			// 
			this.textBoxNeedleMaximum.Location = new System.Drawing.Point(136, 432);
			this.textBoxNeedleMaximum.Name = "textBoxNeedleMaximum";
			this.textBoxNeedleMaximum.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleMaximum.TabIndex = 74;
			// 
			// labelNeedleMaximum
			// 
			this.labelNeedleMaximum.Location = new System.Drawing.Point(8, 432);
			this.labelNeedleMaximum.Name = "labelNeedleMaximum";
			this.labelNeedleMaximum.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleMaximum.TabIndex = 73;
			this.labelNeedleMaximum.Text = "Maximum:";
			this.labelNeedleMaximum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleMinimum
			// 
			this.textBoxNeedleMinimum.Location = new System.Drawing.Point(136, 408);
			this.textBoxNeedleMinimum.Name = "textBoxNeedleMinimum";
			this.textBoxNeedleMinimum.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleMinimum.TabIndex = 72;
			// 
			// labelNeedleMinimum
			// 
			this.labelNeedleMinimum.Location = new System.Drawing.Point(8, 408);
			this.labelNeedleMinimum.Name = "labelNeedleMinimum";
			this.labelNeedleMinimum.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleMinimum.TabIndex = 71;
			this.labelNeedleMinimum.Text = "Minimum:";
			this.labelNeedleMinimum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleLastAngle
			// 
			this.textBoxNeedleLastAngle.Location = new System.Drawing.Point(136, 384);
			this.textBoxNeedleLastAngle.Name = "textBoxNeedleLastAngle";
			this.textBoxNeedleLastAngle.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleLastAngle.TabIndex = 70;
			// 
			// labelNeedleLastAngle
			// 
			this.labelNeedleLastAngle.Location = new System.Drawing.Point(8, 384);
			this.labelNeedleLastAngle.Name = "labelNeedleLastAngle";
			this.labelNeedleLastAngle.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleLastAngle.TabIndex = 69;
			this.labelNeedleLastAngle.Text = "LastAngle:";
			this.labelNeedleLastAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleInitialAngle
			// 
			this.textBoxNeedleInitialAngle.Location = new System.Drawing.Point(136, 360);
			this.textBoxNeedleInitialAngle.Name = "textBoxNeedleInitialAngle";
			this.textBoxNeedleInitialAngle.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleInitialAngle.TabIndex = 68;
			// 
			// labelNeedleInitialAngle
			// 
			this.labelNeedleInitialAngle.Location = new System.Drawing.Point(8, 360);
			this.labelNeedleInitialAngle.Name = "labelNeedleInitialAngle";
			this.labelNeedleInitialAngle.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleInitialAngle.TabIndex = 67;
			this.labelNeedleInitialAngle.Text = "InitialAngle:";
			this.labelNeedleInitialAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxNeedleOrigin
			// 
			this.groupBoxNeedleOrigin.Controls.Add(this.textBoxNeedleOriginY);
			this.groupBoxNeedleOrigin.Controls.Add(this.textBoxNeedleOriginX);
			this.groupBoxNeedleOrigin.Controls.Add(this.labelNeedleOriginY);
			this.groupBoxNeedleOrigin.Controls.Add(this.labelNeedleOriginX);
			this.groupBoxNeedleOrigin.Location = new System.Drawing.Point(8, 280);
			this.groupBoxNeedleOrigin.Name = "groupBoxNeedleOrigin";
			this.groupBoxNeedleOrigin.Size = new System.Drawing.Size(184, 72);
			this.groupBoxNeedleOrigin.TabIndex = 66;
			this.groupBoxNeedleOrigin.TabStop = false;
			this.groupBoxNeedleOrigin.Text = "Origin";
			// 
			// textBoxNeedleOriginY
			// 
			this.textBoxNeedleOriginY.Location = new System.Drawing.Point(128, 40);
			this.textBoxNeedleOriginY.Name = "textBoxNeedleOriginY";
			this.textBoxNeedleOriginY.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleOriginY.TabIndex = 33;
			// 
			// textBoxNeedleOriginX
			// 
			this.textBoxNeedleOriginX.Location = new System.Drawing.Point(128, 16);
			this.textBoxNeedleOriginX.Name = "textBoxNeedleOriginX";
			this.textBoxNeedleOriginX.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleOriginX.TabIndex = 32;
			// 
			// labelNeedleOriginY
			// 
			this.labelNeedleOriginY.Location = new System.Drawing.Point(8, 40);
			this.labelNeedleOriginY.Name = "labelNeedleOriginY";
			this.labelNeedleOriginY.Size = new System.Drawing.Size(112, 16);
			this.labelNeedleOriginY.TabIndex = 10;
			this.labelNeedleOriginY.Text = "Y:";
			this.labelNeedleOriginY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleOriginX
			// 
			this.labelNeedleOriginX.Location = new System.Drawing.Point(8, 16);
			this.labelNeedleOriginX.Name = "labelNeedleOriginX";
			this.labelNeedleOriginX.Size = new System.Drawing.Size(112, 16);
			this.labelNeedleOriginX.TabIndex = 9;
			this.labelNeedleOriginX.Text = "X:";
			this.labelNeedleOriginX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleColor
			// 
			this.textBoxNeedleColor.Location = new System.Drawing.Point(136, 208);
			this.textBoxNeedleColor.Name = "textBoxNeedleColor";
			this.textBoxNeedleColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxNeedleColor.TabIndex = 65;
			// 
			// labelNeedleColor
			// 
			this.labelNeedleColor.Location = new System.Drawing.Point(8, 208);
			this.labelNeedleColor.Name = "labelNeedleColor";
			this.labelNeedleColor.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleColor.TabIndex = 64;
			this.labelNeedleColor.Text = "Color:";
			this.labelNeedleColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxNeedleRadius
			// 
			this.textBoxNeedleRadius.Location = new System.Drawing.Point(136, 136);
			this.textBoxNeedleRadius.Name = "textBoxNeedleRadius";
			this.textBoxNeedleRadius.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleRadius.TabIndex = 63;
			// 
			// labelNeedleRadius
			// 
			this.labelNeedleRadius.Location = new System.Drawing.Point(8, 136);
			this.labelNeedleRadius.Name = "labelNeedleRadius";
			this.labelNeedleRadius.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleRadius.TabIndex = 62;
			this.labelNeedleRadius.Text = "Radius:";
			this.labelNeedleRadius.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxNeedleLocation
			// 
			this.groupBoxNeedleLocation.Controls.Add(this.textBoxNeedleLocationY);
			this.groupBoxNeedleLocation.Controls.Add(this.textBoxNeedleLocationX);
			this.groupBoxNeedleLocation.Controls.Add(this.labelNeedleLocationY);
			this.groupBoxNeedleLocation.Controls.Add(this.labelNeedleLocationX);
			this.groupBoxNeedleLocation.Location = new System.Drawing.Point(8, 32);
			this.groupBoxNeedleLocation.Name = "groupBoxNeedleLocation";
			this.groupBoxNeedleLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxNeedleLocation.TabIndex = 61;
			this.groupBoxNeedleLocation.TabStop = false;
			this.groupBoxNeedleLocation.Text = "Location";
			// 
			// textBoxNeedleLocationY
			// 
			this.textBoxNeedleLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxNeedleLocationY.Name = "textBoxNeedleLocationY";
			this.textBoxNeedleLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleLocationY.TabIndex = 33;
			// 
			// textBoxNeedleLocationX
			// 
			this.textBoxNeedleLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxNeedleLocationX.Name = "textBoxNeedleLocationX";
			this.textBoxNeedleLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxNeedleLocationX.TabIndex = 32;
			// 
			// labelNeedleLocationY
			// 
			this.labelNeedleLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelNeedleLocationY.Name = "labelNeedleLocationY";
			this.labelNeedleLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelNeedleLocationY.TabIndex = 10;
			this.labelNeedleLocationY.Text = "Y:";
			this.labelNeedleLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleLocationX
			// 
			this.labelNeedleLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelNeedleLocationX.Name = "labelNeedleLocationX";
			this.labelNeedleLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelNeedleLocationX.TabIndex = 9;
			this.labelNeedleLocationX.Text = "X:";
			this.labelNeedleLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonNeedleSubjectSet
			// 
			this.buttonNeedleSubjectSet.Location = new System.Drawing.Point(136, 8);
			this.buttonNeedleSubjectSet.Name = "buttonNeedleSubjectSet";
			this.buttonNeedleSubjectSet.Size = new System.Drawing.Size(48, 19);
			this.buttonNeedleSubjectSet.TabIndex = 60;
			this.buttonNeedleSubjectSet.Text = "Set...";
			this.buttonNeedleSubjectSet.UseVisualStyleBackColor = true;
			this.buttonNeedleSubjectSet.Click += new System.EventHandler(this.ButtonNeedleSubjectSet_Click);
			// 
			// labelNeedleSubject
			// 
			this.labelNeedleSubject.Location = new System.Drawing.Point(8, 8);
			this.labelNeedleSubject.Name = "labelNeedleSubject";
			this.labelNeedleSubject.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleSubject.TabIndex = 59;
			this.labelNeedleSubject.Text = "Subject:";
			this.labelNeedleSubject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleLayer
			// 
			this.labelNeedleLayer.Location = new System.Drawing.Point(8, 600);
			this.labelNeedleLayer.Name = "labelNeedleLayer";
			this.labelNeedleLayer.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleLayer.TabIndex = 57;
			this.labelNeedleLayer.Text = "Layer:";
			this.labelNeedleLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonNeedleNighttimeImageOpen
			// 
			this.buttonNeedleNighttimeImageOpen.Location = new System.Drawing.Point(248, 184);
			this.buttonNeedleNighttimeImageOpen.Name = "buttonNeedleNighttimeImageOpen";
			this.buttonNeedleNighttimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonNeedleNighttimeImageOpen.TabIndex = 56;
			this.buttonNeedleNighttimeImageOpen.Text = "Open...";
			this.buttonNeedleNighttimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonNeedleNighttimeImageOpen.Click += new System.EventHandler(this.ButtonNeedleNighttimeImageOpen_Click);
			// 
			// buttonNeedleDaytimeImageOpen
			// 
			this.buttonNeedleDaytimeImageOpen.Location = new System.Drawing.Point(248, 160);
			this.buttonNeedleDaytimeImageOpen.Name = "buttonNeedleDaytimeImageOpen";
			this.buttonNeedleDaytimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonNeedleDaytimeImageOpen.TabIndex = 55;
			this.buttonNeedleDaytimeImageOpen.Text = "Open...";
			this.buttonNeedleDaytimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonNeedleDaytimeImageOpen.Click += new System.EventHandler(this.ButtonNeedleDaytimeImageOpen_Click);
			// 
			// textBoxNeedleTransparentColor
			// 
			this.textBoxNeedleTransparentColor.Location = new System.Drawing.Point(136, 232);
			this.textBoxNeedleTransparentColor.Name = "textBoxNeedleTransparentColor";
			this.textBoxNeedleTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxNeedleTransparentColor.TabIndex = 54;
			// 
			// textBoxNeedleNighttimeImage
			// 
			this.textBoxNeedleNighttimeImage.Location = new System.Drawing.Point(136, 184);
			this.textBoxNeedleNighttimeImage.Name = "textBoxNeedleNighttimeImage";
			this.textBoxNeedleNighttimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxNeedleNighttimeImage.TabIndex = 53;
			// 
			// textBoxNeedleDaytimeImage
			// 
			this.textBoxNeedleDaytimeImage.Location = new System.Drawing.Point(136, 160);
			this.textBoxNeedleDaytimeImage.Name = "textBoxNeedleDaytimeImage";
			this.textBoxNeedleDaytimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxNeedleDaytimeImage.TabIndex = 52;
			// 
			// labelNeedleDaytimeImage
			// 
			this.labelNeedleDaytimeImage.Location = new System.Drawing.Point(8, 160);
			this.labelNeedleDaytimeImage.Name = "labelNeedleDaytimeImage";
			this.labelNeedleDaytimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleDaytimeImage.TabIndex = 51;
			this.labelNeedleDaytimeImage.Text = "DaytimeImage:";
			this.labelNeedleDaytimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleNighttimeImage
			// 
			this.labelNeedleNighttimeImage.Location = new System.Drawing.Point(8, 184);
			this.labelNeedleNighttimeImage.Name = "labelNeedleNighttimeImage";
			this.labelNeedleNighttimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleNighttimeImage.TabIndex = 50;
			this.labelNeedleNighttimeImage.Text = "NighttimeImage:";
			this.labelNeedleNighttimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNeedleTransparentColor
			// 
			this.labelNeedleTransparentColor.Location = new System.Drawing.Point(8, 232);
			this.labelNeedleTransparentColor.Name = "labelNeedleTransparentColor";
			this.labelNeedleTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelNeedleTransparentColor.TabIndex = 49;
			this.labelNeedleTransparentColor.Text = "TransparentColor:";
			this.labelNeedleTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageDigitalNumber
			// 
			this.tabPageDigitalNumber.Controls.Add(this.buttonDigitalNumberTransparentColorSet);
			this.tabPageDigitalNumber.Controls.Add(this.numericUpDownDigitalNumberLayer);
			this.tabPageDigitalNumber.Controls.Add(this.numericUpDownDigitalNumberInterval);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberInterval);
			this.tabPageDigitalNumber.Controls.Add(this.groupBoxDigitalNumberLocation);
			this.tabPageDigitalNumber.Controls.Add(this.buttonDigitalNumberSubjectSet);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberSubject);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberLayer);
			this.tabPageDigitalNumber.Controls.Add(this.buttonDigitalNumberNighttimeImageOpen);
			this.tabPageDigitalNumber.Controls.Add(this.buttonDigitalNumberDaytimeImageOpen);
			this.tabPageDigitalNumber.Controls.Add(this.textBoxDigitalNumberTransparentColor);
			this.tabPageDigitalNumber.Controls.Add(this.textBoxDigitalNumberNighttimeImage);
			this.tabPageDigitalNumber.Controls.Add(this.textBoxDigitalNumberDaytimeImage);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberDaytimeImage);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberNighttimeImage);
			this.tabPageDigitalNumber.Controls.Add(this.labelDigitalNumberTransparentColor);
			this.tabPageDigitalNumber.Location = new System.Drawing.Point(4, 40);
			this.tabPageDigitalNumber.Name = "tabPageDigitalNumber";
			this.tabPageDigitalNumber.Size = new System.Drawing.Size(312, 625);
			this.tabPageDigitalNumber.TabIndex = 4;
			this.tabPageDigitalNumber.Text = "DigitalNumber";
			this.tabPageDigitalNumber.UseVisualStyleBackColor = true;
			// 
			// buttonDigitalNumberTransparentColorSet
			// 
			this.buttonDigitalNumberTransparentColorSet.Location = new System.Drawing.Point(248, 160);
			this.buttonDigitalNumberTransparentColorSet.Name = "buttonDigitalNumberTransparentColorSet";
			this.buttonDigitalNumberTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalNumberTransparentColorSet.TabIndex = 86;
			this.buttonDigitalNumberTransparentColorSet.Text = "Set...";
			this.buttonDigitalNumberTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonDigitalNumberTransparentColorSet.Click += new System.EventHandler(this.ButtonDigitalNumberTransparentColorSet_Click);
			// 
			// numericUpDownDigitalNumberLayer
			// 
			this.numericUpDownDigitalNumberLayer.Location = new System.Drawing.Point(136, 208);
			this.numericUpDownDigitalNumberLayer.Name = "numericUpDownDigitalNumberLayer";
			this.numericUpDownDigitalNumberLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownDigitalNumberLayer.TabIndex = 85;
			// 
			// numericUpDownDigitalNumberInterval
			// 
			this.numericUpDownDigitalNumberInterval.Location = new System.Drawing.Point(136, 184);
			this.numericUpDownDigitalNumberInterval.Name = "numericUpDownDigitalNumberInterval";
			this.numericUpDownDigitalNumberInterval.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownDigitalNumberInterval.TabIndex = 84;
			// 
			// labelDigitalNumberInterval
			// 
			this.labelDigitalNumberInterval.Location = new System.Drawing.Point(8, 184);
			this.labelDigitalNumberInterval.Name = "labelDigitalNumberInterval";
			this.labelDigitalNumberInterval.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberInterval.TabIndex = 62;
			this.labelDigitalNumberInterval.Text = "Interval:";
			this.labelDigitalNumberInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxDigitalNumberLocation
			// 
			this.groupBoxDigitalNumberLocation.Controls.Add(this.textBoxDigitalNumberLocationY);
			this.groupBoxDigitalNumberLocation.Controls.Add(this.textBoxDigitalNumberLocationX);
			this.groupBoxDigitalNumberLocation.Controls.Add(this.labelDigitalNumberLocationY);
			this.groupBoxDigitalNumberLocation.Controls.Add(this.labelDigitalNumberLocationX);
			this.groupBoxDigitalNumberLocation.Location = new System.Drawing.Point(8, 32);
			this.groupBoxDigitalNumberLocation.Name = "groupBoxDigitalNumberLocation";
			this.groupBoxDigitalNumberLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxDigitalNumberLocation.TabIndex = 61;
			this.groupBoxDigitalNumberLocation.TabStop = false;
			this.groupBoxDigitalNumberLocation.Text = "Location";
			// 
			// textBoxDigitalNumberLocationY
			// 
			this.textBoxDigitalNumberLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxDigitalNumberLocationY.Name = "textBoxDigitalNumberLocationY";
			this.textBoxDigitalNumberLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalNumberLocationY.TabIndex = 33;
			// 
			// textBoxDigitalNumberLocationX
			// 
			this.textBoxDigitalNumberLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxDigitalNumberLocationX.Name = "textBoxDigitalNumberLocationX";
			this.textBoxDigitalNumberLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalNumberLocationX.TabIndex = 32;
			// 
			// labelDigitalNumberLocationY
			// 
			this.labelDigitalNumberLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelDigitalNumberLocationY.Name = "labelDigitalNumberLocationY";
			this.labelDigitalNumberLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelDigitalNumberLocationY.TabIndex = 10;
			this.labelDigitalNumberLocationY.Text = "Y:";
			this.labelDigitalNumberLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalNumberLocationX
			// 
			this.labelDigitalNumberLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelDigitalNumberLocationX.Name = "labelDigitalNumberLocationX";
			this.labelDigitalNumberLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelDigitalNumberLocationX.TabIndex = 9;
			this.labelDigitalNumberLocationX.Text = "X:";
			this.labelDigitalNumberLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonDigitalNumberSubjectSet
			// 
			this.buttonDigitalNumberSubjectSet.Location = new System.Drawing.Point(136, 8);
			this.buttonDigitalNumberSubjectSet.Name = "buttonDigitalNumberSubjectSet";
			this.buttonDigitalNumberSubjectSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalNumberSubjectSet.TabIndex = 60;
			this.buttonDigitalNumberSubjectSet.Text = "Set...";
			this.buttonDigitalNumberSubjectSet.UseVisualStyleBackColor = true;
			this.buttonDigitalNumberSubjectSet.Click += new System.EventHandler(this.ButtonDigitalNumberSubjectSet_Click);
			// 
			// labelDigitalNumberSubject
			// 
			this.labelDigitalNumberSubject.Location = new System.Drawing.Point(8, 8);
			this.labelDigitalNumberSubject.Name = "labelDigitalNumberSubject";
			this.labelDigitalNumberSubject.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberSubject.TabIndex = 59;
			this.labelDigitalNumberSubject.Text = "Subject:";
			this.labelDigitalNumberSubject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalNumberLayer
			// 
			this.labelDigitalNumberLayer.Location = new System.Drawing.Point(8, 208);
			this.labelDigitalNumberLayer.Name = "labelDigitalNumberLayer";
			this.labelDigitalNumberLayer.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberLayer.TabIndex = 57;
			this.labelDigitalNumberLayer.Text = "Layer:";
			this.labelDigitalNumberLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonDigitalNumberNighttimeImageOpen
			// 
			this.buttonDigitalNumberNighttimeImageOpen.Location = new System.Drawing.Point(248, 136);
			this.buttonDigitalNumberNighttimeImageOpen.Name = "buttonDigitalNumberNighttimeImageOpen";
			this.buttonDigitalNumberNighttimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalNumberNighttimeImageOpen.TabIndex = 56;
			this.buttonDigitalNumberNighttimeImageOpen.Text = "Open...";
			this.buttonDigitalNumberNighttimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonDigitalNumberNighttimeImageOpen.Click += new System.EventHandler(this.ButtonDigitalNumberNighttimeImageOpen_Click);
			// 
			// buttonDigitalNumberDaytimeImageOpen
			// 
			this.buttonDigitalNumberDaytimeImageOpen.Location = new System.Drawing.Point(248, 112);
			this.buttonDigitalNumberDaytimeImageOpen.Name = "buttonDigitalNumberDaytimeImageOpen";
			this.buttonDigitalNumberDaytimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalNumberDaytimeImageOpen.TabIndex = 55;
			this.buttonDigitalNumberDaytimeImageOpen.Text = "Open...";
			this.buttonDigitalNumberDaytimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonDigitalNumberDaytimeImageOpen.Click += new System.EventHandler(this.ButtonDigitalNumberDaytimeImageOpen_Click);
			// 
			// textBoxDigitalNumberTransparentColor
			// 
			this.textBoxDigitalNumberTransparentColor.Location = new System.Drawing.Point(136, 160);
			this.textBoxDigitalNumberTransparentColor.Name = "textBoxDigitalNumberTransparentColor";
			this.textBoxDigitalNumberTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxDigitalNumberTransparentColor.TabIndex = 54;
			// 
			// textBoxDigitalNumberNighttimeImage
			// 
			this.textBoxDigitalNumberNighttimeImage.Location = new System.Drawing.Point(136, 136);
			this.textBoxDigitalNumberNighttimeImage.Name = "textBoxDigitalNumberNighttimeImage";
			this.textBoxDigitalNumberNighttimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxDigitalNumberNighttimeImage.TabIndex = 53;
			// 
			// textBoxDigitalNumberDaytimeImage
			// 
			this.textBoxDigitalNumberDaytimeImage.Location = new System.Drawing.Point(136, 112);
			this.textBoxDigitalNumberDaytimeImage.Name = "textBoxDigitalNumberDaytimeImage";
			this.textBoxDigitalNumberDaytimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxDigitalNumberDaytimeImage.TabIndex = 52;
			// 
			// labelDigitalNumberDaytimeImage
			// 
			this.labelDigitalNumberDaytimeImage.Location = new System.Drawing.Point(8, 112);
			this.labelDigitalNumberDaytimeImage.Name = "labelDigitalNumberDaytimeImage";
			this.labelDigitalNumberDaytimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberDaytimeImage.TabIndex = 51;
			this.labelDigitalNumberDaytimeImage.Text = "DaytimeImage:";
			this.labelDigitalNumberDaytimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalNumberNighttimeImage
			// 
			this.labelDigitalNumberNighttimeImage.Location = new System.Drawing.Point(8, 136);
			this.labelDigitalNumberNighttimeImage.Name = "labelDigitalNumberNighttimeImage";
			this.labelDigitalNumberNighttimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberNighttimeImage.TabIndex = 50;
			this.labelDigitalNumberNighttimeImage.Text = "NighttimeImage:";
			this.labelDigitalNumberNighttimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalNumberTransparentColor
			// 
			this.labelDigitalNumberTransparentColor.Location = new System.Drawing.Point(8, 160);
			this.labelDigitalNumberTransparentColor.Name = "labelDigitalNumberTransparentColor";
			this.labelDigitalNumberTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalNumberTransparentColor.TabIndex = 49;
			this.labelDigitalNumberTransparentColor.Text = "TransparentColor:";
			this.labelDigitalNumberTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageDigitalGauge
			// 
			this.tabPageDigitalGauge.Controls.Add(this.buttonDigitalGaugeColorSet);
			this.tabPageDigitalGauge.Controls.Add(this.numericUpDownDigitalGaugeLayer);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeStep);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeStep);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeLayer);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeMaximum);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeMaximum);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeMinimum);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeMinimum);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeLastAngle);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeLastAngle);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeInitialAngle);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeInitialAngle);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeColor);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeColor);
			this.tabPageDigitalGauge.Controls.Add(this.textBoxDigitalGaugeRadius);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeRadius);
			this.tabPageDigitalGauge.Controls.Add(this.groupBoxDigitalGaugeLocation);
			this.tabPageDigitalGauge.Controls.Add(this.buttonDigitalGaugeSubjectSet);
			this.tabPageDigitalGauge.Controls.Add(this.labelDigitalGaugeSubject);
			this.tabPageDigitalGauge.Location = new System.Drawing.Point(4, 40);
			this.tabPageDigitalGauge.Name = "tabPageDigitalGauge";
			this.tabPageDigitalGauge.Size = new System.Drawing.Size(312, 625);
			this.tabPageDigitalGauge.TabIndex = 5;
			this.tabPageDigitalGauge.Text = "DigitalGauge";
			this.tabPageDigitalGauge.UseVisualStyleBackColor = true;
			// 
			// buttonDigitalGaugeColorSet
			// 
			this.buttonDigitalGaugeColorSet.Location = new System.Drawing.Point(248, 136);
			this.buttonDigitalGaugeColorSet.Name = "buttonDigitalGaugeColorSet";
			this.buttonDigitalGaugeColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalGaugeColorSet.TabIndex = 88;
			this.buttonDigitalGaugeColorSet.Text = "Set...";
			this.buttonDigitalGaugeColorSet.UseVisualStyleBackColor = true;
			this.buttonDigitalGaugeColorSet.Click += new System.EventHandler(this.ButtonDigitalGaugeColorSet_Click);
			// 
			// numericUpDownDigitalGaugeLayer
			// 
			this.numericUpDownDigitalGaugeLayer.Location = new System.Drawing.Point(136, 280);
			this.numericUpDownDigitalGaugeLayer.Name = "numericUpDownDigitalGaugeLayer";
			this.numericUpDownDigitalGaugeLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownDigitalGaugeLayer.TabIndex = 87;
			// 
			// textBoxDigitalGaugeStep
			// 
			this.textBoxDigitalGaugeStep.Location = new System.Drawing.Point(136, 256);
			this.textBoxDigitalGaugeStep.Name = "textBoxDigitalGaugeStep";
			this.textBoxDigitalGaugeStep.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeStep.TabIndex = 86;
			// 
			// labelDigitalGaugeStep
			// 
			this.labelDigitalGaugeStep.Location = new System.Drawing.Point(8, 256);
			this.labelDigitalGaugeStep.Name = "labelDigitalGaugeStep";
			this.labelDigitalGaugeStep.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeStep.TabIndex = 85;
			this.labelDigitalGaugeStep.Text = "Step:";
			this.labelDigitalGaugeStep.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalGaugeLayer
			// 
			this.labelDigitalGaugeLayer.Location = new System.Drawing.Point(8, 280);
			this.labelDigitalGaugeLayer.Name = "labelDigitalGaugeLayer";
			this.labelDigitalGaugeLayer.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeLayer.TabIndex = 83;
			this.labelDigitalGaugeLayer.Text = "Layer:";
			this.labelDigitalGaugeLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeMaximum
			// 
			this.textBoxDigitalGaugeMaximum.Location = new System.Drawing.Point(136, 232);
			this.textBoxDigitalGaugeMaximum.Name = "textBoxDigitalGaugeMaximum";
			this.textBoxDigitalGaugeMaximum.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeMaximum.TabIndex = 82;
			// 
			// labelDigitalGaugeMaximum
			// 
			this.labelDigitalGaugeMaximum.Location = new System.Drawing.Point(8, 232);
			this.labelDigitalGaugeMaximum.Name = "labelDigitalGaugeMaximum";
			this.labelDigitalGaugeMaximum.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeMaximum.TabIndex = 81;
			this.labelDigitalGaugeMaximum.Text = "Maximum:";
			this.labelDigitalGaugeMaximum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeMinimum
			// 
			this.textBoxDigitalGaugeMinimum.Location = new System.Drawing.Point(136, 208);
			this.textBoxDigitalGaugeMinimum.Name = "textBoxDigitalGaugeMinimum";
			this.textBoxDigitalGaugeMinimum.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeMinimum.TabIndex = 80;
			// 
			// labelDigitalGaugeMinimum
			// 
			this.labelDigitalGaugeMinimum.Location = new System.Drawing.Point(8, 208);
			this.labelDigitalGaugeMinimum.Name = "labelDigitalGaugeMinimum";
			this.labelDigitalGaugeMinimum.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeMinimum.TabIndex = 79;
			this.labelDigitalGaugeMinimum.Text = "Minimum:";
			this.labelDigitalGaugeMinimum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeLastAngle
			// 
			this.textBoxDigitalGaugeLastAngle.Location = new System.Drawing.Point(136, 184);
			this.textBoxDigitalGaugeLastAngle.Name = "textBoxDigitalGaugeLastAngle";
			this.textBoxDigitalGaugeLastAngle.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeLastAngle.TabIndex = 78;
			// 
			// labelDigitalGaugeLastAngle
			// 
			this.labelDigitalGaugeLastAngle.Location = new System.Drawing.Point(8, 184);
			this.labelDigitalGaugeLastAngle.Name = "labelDigitalGaugeLastAngle";
			this.labelDigitalGaugeLastAngle.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeLastAngle.TabIndex = 77;
			this.labelDigitalGaugeLastAngle.Text = "LastAngle:";
			this.labelDigitalGaugeLastAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeInitialAngle
			// 
			this.textBoxDigitalGaugeInitialAngle.Location = new System.Drawing.Point(136, 160);
			this.textBoxDigitalGaugeInitialAngle.Name = "textBoxDigitalGaugeInitialAngle";
			this.textBoxDigitalGaugeInitialAngle.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeInitialAngle.TabIndex = 76;
			// 
			// labelDigitalGaugeInitialAngle
			// 
			this.labelDigitalGaugeInitialAngle.Location = new System.Drawing.Point(8, 160);
			this.labelDigitalGaugeInitialAngle.Name = "labelDigitalGaugeInitialAngle";
			this.labelDigitalGaugeInitialAngle.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeInitialAngle.TabIndex = 75;
			this.labelDigitalGaugeInitialAngle.Text = "InitialAngle:";
			this.labelDigitalGaugeInitialAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeColor
			// 
			this.textBoxDigitalGaugeColor.Location = new System.Drawing.Point(136, 136);
			this.textBoxDigitalGaugeColor.Name = "textBoxDigitalGaugeColor";
			this.textBoxDigitalGaugeColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxDigitalGaugeColor.TabIndex = 70;
			// 
			// labelDigitalGaugeColor
			// 
			this.labelDigitalGaugeColor.Location = new System.Drawing.Point(8, 136);
			this.labelDigitalGaugeColor.Name = "labelDigitalGaugeColor";
			this.labelDigitalGaugeColor.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeColor.TabIndex = 69;
			this.labelDigitalGaugeColor.Text = "Color:";
			this.labelDigitalGaugeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxDigitalGaugeRadius
			// 
			this.textBoxDigitalGaugeRadius.Location = new System.Drawing.Point(136, 112);
			this.textBoxDigitalGaugeRadius.Name = "textBoxDigitalGaugeRadius";
			this.textBoxDigitalGaugeRadius.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeRadius.TabIndex = 68;
			// 
			// labelDigitalGaugeRadius
			// 
			this.labelDigitalGaugeRadius.Location = new System.Drawing.Point(8, 112);
			this.labelDigitalGaugeRadius.Name = "labelDigitalGaugeRadius";
			this.labelDigitalGaugeRadius.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeRadius.TabIndex = 67;
			this.labelDigitalGaugeRadius.Text = "Radius:";
			this.labelDigitalGaugeRadius.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxDigitalGaugeLocation
			// 
			this.groupBoxDigitalGaugeLocation.Controls.Add(this.textBoxDigitalGaugeLocationY);
			this.groupBoxDigitalGaugeLocation.Controls.Add(this.textBoxDigitalGaugeLocationX);
			this.groupBoxDigitalGaugeLocation.Controls.Add(this.labelDigitalGaugeLocationY);
			this.groupBoxDigitalGaugeLocation.Controls.Add(this.labelDigitalGaugeLocationX);
			this.groupBoxDigitalGaugeLocation.Location = new System.Drawing.Point(8, 32);
			this.groupBoxDigitalGaugeLocation.Name = "groupBoxDigitalGaugeLocation";
			this.groupBoxDigitalGaugeLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxDigitalGaugeLocation.TabIndex = 66;
			this.groupBoxDigitalGaugeLocation.TabStop = false;
			this.groupBoxDigitalGaugeLocation.Text = "Location";
			// 
			// textBoxDigitalGaugeLocationY
			// 
			this.textBoxDigitalGaugeLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxDigitalGaugeLocationY.Name = "textBoxDigitalGaugeLocationY";
			this.textBoxDigitalGaugeLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeLocationY.TabIndex = 33;
			// 
			// textBoxDigitalGaugeLocationX
			// 
			this.textBoxDigitalGaugeLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxDigitalGaugeLocationX.Name = "textBoxDigitalGaugeLocationX";
			this.textBoxDigitalGaugeLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxDigitalGaugeLocationX.TabIndex = 32;
			// 
			// labelDigitalGaugeLocationY
			// 
			this.labelDigitalGaugeLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelDigitalGaugeLocationY.Name = "labelDigitalGaugeLocationY";
			this.labelDigitalGaugeLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelDigitalGaugeLocationY.TabIndex = 10;
			this.labelDigitalGaugeLocationY.Text = "Y:";
			this.labelDigitalGaugeLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDigitalGaugeLocationX
			// 
			this.labelDigitalGaugeLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelDigitalGaugeLocationX.Name = "labelDigitalGaugeLocationX";
			this.labelDigitalGaugeLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelDigitalGaugeLocationX.TabIndex = 9;
			this.labelDigitalGaugeLocationX.Text = "X:";
			this.labelDigitalGaugeLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonDigitalGaugeSubjectSet
			// 
			this.buttonDigitalGaugeSubjectSet.Location = new System.Drawing.Point(136, 8);
			this.buttonDigitalGaugeSubjectSet.Name = "buttonDigitalGaugeSubjectSet";
			this.buttonDigitalGaugeSubjectSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDigitalGaugeSubjectSet.TabIndex = 65;
			this.buttonDigitalGaugeSubjectSet.Text = "Set...";
			this.buttonDigitalGaugeSubjectSet.UseVisualStyleBackColor = true;
			this.buttonDigitalGaugeSubjectSet.Click += new System.EventHandler(this.ButtonDigitalGaugeSubjectSet_Click);
			// 
			// labelDigitalGaugeSubject
			// 
			this.labelDigitalGaugeSubject.Location = new System.Drawing.Point(8, 8);
			this.labelDigitalGaugeSubject.Name = "labelDigitalGaugeSubject";
			this.labelDigitalGaugeSubject.Size = new System.Drawing.Size(120, 16);
			this.labelDigitalGaugeSubject.TabIndex = 64;
			this.labelDigitalGaugeSubject.Text = "Subject:";
			this.labelDigitalGaugeSubject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageLinearGauge
			// 
			this.tabPageLinearGauge.Controls.Add(this.buttonLinearGaugeTransparentColorSet);
			this.tabPageLinearGauge.Controls.Add(this.buttonLinearGaugeNighttimeImageOpen);
			this.tabPageLinearGauge.Controls.Add(this.buttonLinearGaugeDaytimeImageOpen);
			this.tabPageLinearGauge.Controls.Add(this.textBoxLinearGaugeTransparentColor);
			this.tabPageLinearGauge.Controls.Add(this.textBoxLinearGaugeNighttimeImage);
			this.tabPageLinearGauge.Controls.Add(this.textBoxLinearGaugeDaytimeImage);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeDaytimeImage);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeNighttimeImage);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeTransparentColor);
			this.tabPageLinearGauge.Controls.Add(this.numericUpDownLinearGaugeLayer);
			this.tabPageLinearGauge.Controls.Add(this.numericUpDownLinearGaugeWidth);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeWidth);
			this.tabPageLinearGauge.Controls.Add(this.groupBoxLinearGaugeDirection);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeLayer);
			this.tabPageLinearGauge.Controls.Add(this.textBoxLinearGaugeMaximum);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeMaximum);
			this.tabPageLinearGauge.Controls.Add(this.textBoxLinearGaugeMinimum);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeMinimum);
			this.tabPageLinearGauge.Controls.Add(this.groupBoxLinearGaugeLocation);
			this.tabPageLinearGauge.Controls.Add(this.buttonLinearGaugeSubjectSet);
			this.tabPageLinearGauge.Controls.Add(this.labelLinearGaugeSubject);
			this.tabPageLinearGauge.Location = new System.Drawing.Point(4, 40);
			this.tabPageLinearGauge.Name = "tabPageLinearGauge";
			this.tabPageLinearGauge.Size = new System.Drawing.Size(312, 625);
			this.tabPageLinearGauge.TabIndex = 6;
			this.tabPageLinearGauge.Text = "LinearGauge";
			this.tabPageLinearGauge.UseVisualStyleBackColor = true;
			// 
			// buttonLinearGaugeTransparentColorSet
			// 
			this.buttonLinearGaugeTransparentColorSet.Location = new System.Drawing.Point(248, 160);
			this.buttonLinearGaugeTransparentColorSet.Name = "buttonLinearGaugeTransparentColorSet";
			this.buttonLinearGaugeTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonLinearGaugeTransparentColorSet.TabIndex = 105;
			this.buttonLinearGaugeTransparentColorSet.Text = "Set...";
			this.buttonLinearGaugeTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonLinearGaugeTransparentColorSet.Click += new System.EventHandler(this.ButtonLinearGaugeTransparentColorSet_Click);
			// 
			// buttonLinearGaugeNighttimeImageOpen
			// 
			this.buttonLinearGaugeNighttimeImageOpen.Location = new System.Drawing.Point(248, 136);
			this.buttonLinearGaugeNighttimeImageOpen.Name = "buttonLinearGaugeNighttimeImageOpen";
			this.buttonLinearGaugeNighttimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonLinearGaugeNighttimeImageOpen.TabIndex = 104;
			this.buttonLinearGaugeNighttimeImageOpen.Text = "Open...";
			this.buttonLinearGaugeNighttimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonLinearGaugeNighttimeImageOpen.Click += new System.EventHandler(this.ButtonLinearGaugeNighttimeImageOpen_Click);
			// 
			// buttonLinearGaugeDaytimeImageOpen
			// 
			this.buttonLinearGaugeDaytimeImageOpen.Location = new System.Drawing.Point(248, 112);
			this.buttonLinearGaugeDaytimeImageOpen.Name = "buttonLinearGaugeDaytimeImageOpen";
			this.buttonLinearGaugeDaytimeImageOpen.Size = new System.Drawing.Size(48, 19);
			this.buttonLinearGaugeDaytimeImageOpen.TabIndex = 103;
			this.buttonLinearGaugeDaytimeImageOpen.Text = "Open...";
			this.buttonLinearGaugeDaytimeImageOpen.UseVisualStyleBackColor = true;
			this.buttonLinearGaugeDaytimeImageOpen.Click += new System.EventHandler(this.ButtonLinearGaugeDaytimeImageOpen_Click);
			// 
			// textBoxLinearGaugeTransparentColor
			// 
			this.textBoxLinearGaugeTransparentColor.Location = new System.Drawing.Point(136, 160);
			this.textBoxLinearGaugeTransparentColor.Name = "textBoxLinearGaugeTransparentColor";
			this.textBoxLinearGaugeTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxLinearGaugeTransparentColor.TabIndex = 102;
			// 
			// textBoxLinearGaugeNighttimeImage
			// 
			this.textBoxLinearGaugeNighttimeImage.Location = new System.Drawing.Point(136, 136);
			this.textBoxLinearGaugeNighttimeImage.Name = "textBoxLinearGaugeNighttimeImage";
			this.textBoxLinearGaugeNighttimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxLinearGaugeNighttimeImage.TabIndex = 101;
			// 
			// textBoxLinearGaugeDaytimeImage
			// 
			this.textBoxLinearGaugeDaytimeImage.Location = new System.Drawing.Point(136, 112);
			this.textBoxLinearGaugeDaytimeImage.Name = "textBoxLinearGaugeDaytimeImage";
			this.textBoxLinearGaugeDaytimeImage.Size = new System.Drawing.Size(104, 19);
			this.textBoxLinearGaugeDaytimeImage.TabIndex = 100;
			// 
			// labelLinearGaugeDaytimeImage
			// 
			this.labelLinearGaugeDaytimeImage.Location = new System.Drawing.Point(8, 112);
			this.labelLinearGaugeDaytimeImage.Name = "labelLinearGaugeDaytimeImage";
			this.labelLinearGaugeDaytimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeDaytimeImage.TabIndex = 99;
			this.labelLinearGaugeDaytimeImage.Text = "DaytimeImage:";
			this.labelLinearGaugeDaytimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLinearGaugeNighttimeImage
			// 
			this.labelLinearGaugeNighttimeImage.Location = new System.Drawing.Point(8, 136);
			this.labelLinearGaugeNighttimeImage.Name = "labelLinearGaugeNighttimeImage";
			this.labelLinearGaugeNighttimeImage.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeNighttimeImage.TabIndex = 98;
			this.labelLinearGaugeNighttimeImage.Text = "NighttimeImage:";
			this.labelLinearGaugeNighttimeImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLinearGaugeTransparentColor
			// 
			this.labelLinearGaugeTransparentColor.Location = new System.Drawing.Point(8, 160);
			this.labelLinearGaugeTransparentColor.Name = "labelLinearGaugeTransparentColor";
			this.labelLinearGaugeTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeTransparentColor.TabIndex = 97;
			this.labelLinearGaugeTransparentColor.Text = "TransparentColor:";
			this.labelLinearGaugeTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownLinearGaugeLayer
			// 
			this.numericUpDownLinearGaugeLayer.Location = new System.Drawing.Point(136, 336);
			this.numericUpDownLinearGaugeLayer.Name = "numericUpDownLinearGaugeLayer";
			this.numericUpDownLinearGaugeLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownLinearGaugeLayer.TabIndex = 96;
			// 
			// numericUpDownLinearGaugeWidth
			// 
			this.numericUpDownLinearGaugeWidth.Location = new System.Drawing.Point(136, 312);
			this.numericUpDownLinearGaugeWidth.Name = "numericUpDownLinearGaugeWidth";
			this.numericUpDownLinearGaugeWidth.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownLinearGaugeWidth.TabIndex = 95;
			// 
			// labelLinearGaugeWidth
			// 
			this.labelLinearGaugeWidth.Location = new System.Drawing.Point(8, 312);
			this.labelLinearGaugeWidth.Name = "labelLinearGaugeWidth";
			this.labelLinearGaugeWidth.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeWidth.TabIndex = 94;
			this.labelLinearGaugeWidth.Text = "Width:";
			this.labelLinearGaugeWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxLinearGaugeDirection
			// 
			this.groupBoxLinearGaugeDirection.Controls.Add(this.numericUpDownLinearGaugeDirectionY);
			this.groupBoxLinearGaugeDirection.Controls.Add(this.numericUpDownLinearGaugeDirectionX);
			this.groupBoxLinearGaugeDirection.Controls.Add(this.labelLinearGaugeDirectionY);
			this.groupBoxLinearGaugeDirection.Controls.Add(this.labelLinearGaugeDirectionX);
			this.groupBoxLinearGaugeDirection.Location = new System.Drawing.Point(8, 232);
			this.groupBoxLinearGaugeDirection.Name = "groupBoxLinearGaugeDirection";
			this.groupBoxLinearGaugeDirection.Size = new System.Drawing.Size(184, 72);
			this.groupBoxLinearGaugeDirection.TabIndex = 88;
			this.groupBoxLinearGaugeDirection.TabStop = false;
			this.groupBoxLinearGaugeDirection.Text = "Direction";
			// 
			// numericUpDownLinearGaugeDirectionY
			// 
			this.numericUpDownLinearGaugeDirectionY.Location = new System.Drawing.Point(128, 40);
			this.numericUpDownLinearGaugeDirectionY.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownLinearGaugeDirectionY.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownLinearGaugeDirectionY.Name = "numericUpDownLinearGaugeDirectionY";
			this.numericUpDownLinearGaugeDirectionY.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownLinearGaugeDirectionY.TabIndex = 107;
			// 
			// numericUpDownLinearGaugeDirectionX
			// 
			this.numericUpDownLinearGaugeDirectionX.Location = new System.Drawing.Point(128, 16);
			this.numericUpDownLinearGaugeDirectionX.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownLinearGaugeDirectionX.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownLinearGaugeDirectionX.Name = "numericUpDownLinearGaugeDirectionX";
			this.numericUpDownLinearGaugeDirectionX.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownLinearGaugeDirectionX.TabIndex = 106;
			// 
			// labelLinearGaugeDirectionY
			// 
			this.labelLinearGaugeDirectionY.Location = new System.Drawing.Point(8, 40);
			this.labelLinearGaugeDirectionY.Name = "labelLinearGaugeDirectionY";
			this.labelLinearGaugeDirectionY.Size = new System.Drawing.Size(112, 16);
			this.labelLinearGaugeDirectionY.TabIndex = 10;
			this.labelLinearGaugeDirectionY.Text = "Y:";
			this.labelLinearGaugeDirectionY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLinearGaugeDirectionX
			// 
			this.labelLinearGaugeDirectionX.Location = new System.Drawing.Point(8, 16);
			this.labelLinearGaugeDirectionX.Name = "labelLinearGaugeDirectionX";
			this.labelLinearGaugeDirectionX.Size = new System.Drawing.Size(112, 16);
			this.labelLinearGaugeDirectionX.TabIndex = 9;
			this.labelLinearGaugeDirectionX.Text = "X:";
			this.labelLinearGaugeDirectionX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLinearGaugeLayer
			// 
			this.labelLinearGaugeLayer.Location = new System.Drawing.Point(8, 336);
			this.labelLinearGaugeLayer.Name = "labelLinearGaugeLayer";
			this.labelLinearGaugeLayer.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeLayer.TabIndex = 92;
			this.labelLinearGaugeLayer.Text = "Layer:";
			this.labelLinearGaugeLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxLinearGaugeMaximum
			// 
			this.textBoxLinearGaugeMaximum.Location = new System.Drawing.Point(136, 208);
			this.textBoxLinearGaugeMaximum.Name = "textBoxLinearGaugeMaximum";
			this.textBoxLinearGaugeMaximum.Size = new System.Drawing.Size(48, 19);
			this.textBoxLinearGaugeMaximum.TabIndex = 91;
			// 
			// labelLinearGaugeMaximum
			// 
			this.labelLinearGaugeMaximum.Location = new System.Drawing.Point(8, 208);
			this.labelLinearGaugeMaximum.Name = "labelLinearGaugeMaximum";
			this.labelLinearGaugeMaximum.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeMaximum.TabIndex = 90;
			this.labelLinearGaugeMaximum.Text = "Maximum:";
			this.labelLinearGaugeMaximum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxLinearGaugeMinimum
			// 
			this.textBoxLinearGaugeMinimum.Location = new System.Drawing.Point(136, 184);
			this.textBoxLinearGaugeMinimum.Name = "textBoxLinearGaugeMinimum";
			this.textBoxLinearGaugeMinimum.Size = new System.Drawing.Size(48, 19);
			this.textBoxLinearGaugeMinimum.TabIndex = 89;
			// 
			// labelLinearGaugeMinimum
			// 
			this.labelLinearGaugeMinimum.Location = new System.Drawing.Point(8, 184);
			this.labelLinearGaugeMinimum.Name = "labelLinearGaugeMinimum";
			this.labelLinearGaugeMinimum.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeMinimum.TabIndex = 88;
			this.labelLinearGaugeMinimum.Text = "Minimum:";
			this.labelLinearGaugeMinimum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxLinearGaugeLocation
			// 
			this.groupBoxLinearGaugeLocation.Controls.Add(this.textBoxLinearGaugeLocationY);
			this.groupBoxLinearGaugeLocation.Controls.Add(this.textBoxLinearGaugeLocationX);
			this.groupBoxLinearGaugeLocation.Controls.Add(this.labelLinearGaugeLocationY);
			this.groupBoxLinearGaugeLocation.Controls.Add(this.labelLinearGaugeLocationX);
			this.groupBoxLinearGaugeLocation.Location = new System.Drawing.Point(8, 32);
			this.groupBoxLinearGaugeLocation.Name = "groupBoxLinearGaugeLocation";
			this.groupBoxLinearGaugeLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxLinearGaugeLocation.TabIndex = 87;
			this.groupBoxLinearGaugeLocation.TabStop = false;
			this.groupBoxLinearGaugeLocation.Text = "Location";
			// 
			// textBoxLinearGaugeLocationY
			// 
			this.textBoxLinearGaugeLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxLinearGaugeLocationY.Name = "textBoxLinearGaugeLocationY";
			this.textBoxLinearGaugeLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxLinearGaugeLocationY.TabIndex = 33;
			// 
			// textBoxLinearGaugeLocationX
			// 
			this.textBoxLinearGaugeLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxLinearGaugeLocationX.Name = "textBoxLinearGaugeLocationX";
			this.textBoxLinearGaugeLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxLinearGaugeLocationX.TabIndex = 32;
			// 
			// labelLinearGaugeLocationY
			// 
			this.labelLinearGaugeLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelLinearGaugeLocationY.Name = "labelLinearGaugeLocationY";
			this.labelLinearGaugeLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelLinearGaugeLocationY.TabIndex = 10;
			this.labelLinearGaugeLocationY.Text = "Y:";
			this.labelLinearGaugeLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLinearGaugeLocationX
			// 
			this.labelLinearGaugeLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelLinearGaugeLocationX.Name = "labelLinearGaugeLocationX";
			this.labelLinearGaugeLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelLinearGaugeLocationX.TabIndex = 9;
			this.labelLinearGaugeLocationX.Text = "X:";
			this.labelLinearGaugeLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonLinearGaugeSubjectSet
			// 
			this.buttonLinearGaugeSubjectSet.Location = new System.Drawing.Point(136, 8);
			this.buttonLinearGaugeSubjectSet.Name = "buttonLinearGaugeSubjectSet";
			this.buttonLinearGaugeSubjectSet.Size = new System.Drawing.Size(48, 19);
			this.buttonLinearGaugeSubjectSet.TabIndex = 86;
			this.buttonLinearGaugeSubjectSet.Text = "Set...";
			this.buttonLinearGaugeSubjectSet.UseVisualStyleBackColor = true;
			this.buttonLinearGaugeSubjectSet.Click += new System.EventHandler(this.ButtonLinearGaugeSubjectSet_Click);
			// 
			// labelLinearGaugeSubject
			// 
			this.labelLinearGaugeSubject.Location = new System.Drawing.Point(8, 8);
			this.labelLinearGaugeSubject.Name = "labelLinearGaugeSubject";
			this.labelLinearGaugeSubject.Size = new System.Drawing.Size(120, 16);
			this.labelLinearGaugeSubject.TabIndex = 85;
			this.labelLinearGaugeSubject.Text = "Subject:";
			this.labelLinearGaugeSubject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageTimetable
			// 
			this.tabPageTimetable.Controls.Add(this.buttonTimetableTransparentColorSet);
			this.tabPageTimetable.Controls.Add(this.numericUpDownTimetableLayer);
			this.tabPageTimetable.Controls.Add(this.labelTimetableLayer);
			this.tabPageTimetable.Controls.Add(this.textBoxTimetableTransparentColor);
			this.tabPageTimetable.Controls.Add(this.labelTimetableTransparentColor);
			this.tabPageTimetable.Controls.Add(this.textBoxTimetableHeight);
			this.tabPageTimetable.Controls.Add(this.labelTimetableHeight);
			this.tabPageTimetable.Controls.Add(this.textBoxTimetableWidth);
			this.tabPageTimetable.Controls.Add(this.labelTimetableWidth);
			this.tabPageTimetable.Controls.Add(this.groupBoxTimetableLocation);
			this.tabPageTimetable.Location = new System.Drawing.Point(4, 40);
			this.tabPageTimetable.Name = "tabPageTimetable";
			this.tabPageTimetable.Size = new System.Drawing.Size(312, 625);
			this.tabPageTimetable.TabIndex = 7;
			this.tabPageTimetable.Text = "Timetable";
			this.tabPageTimetable.UseVisualStyleBackColor = true;
			// 
			// buttonTimetableTransparentColorSet
			// 
			this.buttonTimetableTransparentColorSet.Location = new System.Drawing.Point(248, 136);
			this.buttonTimetableTransparentColorSet.Name = "buttonTimetableTransparentColorSet";
			this.buttonTimetableTransparentColorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonTimetableTransparentColorSet.TabIndex = 106;
			this.buttonTimetableTransparentColorSet.Text = "Set...";
			this.buttonTimetableTransparentColorSet.UseVisualStyleBackColor = true;
			this.buttonTimetableTransparentColorSet.Click += new System.EventHandler(this.ButtonTimetableTransparentColorSet_Click);
			// 
			// numericUpDownTimetableLayer
			// 
			this.numericUpDownTimetableLayer.Location = new System.Drawing.Point(136, 160);
			this.numericUpDownTimetableLayer.Name = "numericUpDownTimetableLayer";
			this.numericUpDownTimetableLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTimetableLayer.TabIndex = 103;
			// 
			// labelTimetableLayer
			// 
			this.labelTimetableLayer.Location = new System.Drawing.Point(8, 160);
			this.labelTimetableLayer.Name = "labelTimetableLayer";
			this.labelTimetableLayer.Size = new System.Drawing.Size(120, 16);
			this.labelTimetableLayer.TabIndex = 102;
			this.labelTimetableLayer.Text = "Layer:";
			this.labelTimetableLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxTimetableTransparentColor
			// 
			this.textBoxTimetableTransparentColor.Location = new System.Drawing.Point(136, 136);
			this.textBoxTimetableTransparentColor.Name = "textBoxTimetableTransparentColor";
			this.textBoxTimetableTransparentColor.Size = new System.Drawing.Size(104, 19);
			this.textBoxTimetableTransparentColor.TabIndex = 101;
			// 
			// labelTimetableTransparentColor
			// 
			this.labelTimetableTransparentColor.Location = new System.Drawing.Point(8, 136);
			this.labelTimetableTransparentColor.Name = "labelTimetableTransparentColor";
			this.labelTimetableTransparentColor.Size = new System.Drawing.Size(120, 16);
			this.labelTimetableTransparentColor.TabIndex = 100;
			this.labelTimetableTransparentColor.Text = "TransparentColor:";
			this.labelTimetableTransparentColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxTimetableHeight
			// 
			this.textBoxTimetableHeight.Location = new System.Drawing.Point(136, 112);
			this.textBoxTimetableHeight.Name = "textBoxTimetableHeight";
			this.textBoxTimetableHeight.Size = new System.Drawing.Size(48, 19);
			this.textBoxTimetableHeight.TabIndex = 99;
			// 
			// labelTimetableHeight
			// 
			this.labelTimetableHeight.Location = new System.Drawing.Point(8, 112);
			this.labelTimetableHeight.Name = "labelTimetableHeight";
			this.labelTimetableHeight.Size = new System.Drawing.Size(120, 16);
			this.labelTimetableHeight.TabIndex = 98;
			this.labelTimetableHeight.Text = "Height:";
			this.labelTimetableHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxTimetableWidth
			// 
			this.textBoxTimetableWidth.Location = new System.Drawing.Point(136, 88);
			this.textBoxTimetableWidth.Name = "textBoxTimetableWidth";
			this.textBoxTimetableWidth.Size = new System.Drawing.Size(48, 19);
			this.textBoxTimetableWidth.TabIndex = 97;
			// 
			// labelTimetableWidth
			// 
			this.labelTimetableWidth.Location = new System.Drawing.Point(8, 88);
			this.labelTimetableWidth.Name = "labelTimetableWidth";
			this.labelTimetableWidth.Size = new System.Drawing.Size(120, 16);
			this.labelTimetableWidth.TabIndex = 96;
			this.labelTimetableWidth.Text = "Width:";
			this.labelTimetableWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxTimetableLocation
			// 
			this.groupBoxTimetableLocation.Controls.Add(this.textBoxTimetableLocationY);
			this.groupBoxTimetableLocation.Controls.Add(this.textBoxTimetableLocationX);
			this.groupBoxTimetableLocation.Controls.Add(this.labelTimetableLocationY);
			this.groupBoxTimetableLocation.Controls.Add(this.labelTimetableLocationX);
			this.groupBoxTimetableLocation.Location = new System.Drawing.Point(8, 8);
			this.groupBoxTimetableLocation.Name = "groupBoxTimetableLocation";
			this.groupBoxTimetableLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxTimetableLocation.TabIndex = 88;
			this.groupBoxTimetableLocation.TabStop = false;
			this.groupBoxTimetableLocation.Text = "Location";
			// 
			// textBoxTimetableLocationY
			// 
			this.textBoxTimetableLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxTimetableLocationY.Name = "textBoxTimetableLocationY";
			this.textBoxTimetableLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxTimetableLocationY.TabIndex = 33;
			// 
			// textBoxTimetableLocationX
			// 
			this.textBoxTimetableLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxTimetableLocationX.Name = "textBoxTimetableLocationX";
			this.textBoxTimetableLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxTimetableLocationX.TabIndex = 32;
			// 
			// labelTimetableLocationY
			// 
			this.labelTimetableLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelTimetableLocationY.Name = "labelTimetableLocationY";
			this.labelTimetableLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelTimetableLocationY.TabIndex = 10;
			this.labelTimetableLocationY.Text = "Y:";
			this.labelTimetableLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTimetableLocationX
			// 
			this.labelTimetableLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelTimetableLocationX.Name = "labelTimetableLocationX";
			this.labelTimetableLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelTimetableLocationX.TabIndex = 9;
			this.labelTimetableLocationX.Text = "X:";
			this.labelTimetableLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageTouch
			// 
			this.tabPageTouch.Controls.Add(this.comboBoxTouchCommand);
			this.tabPageTouch.Controls.Add(this.labelTouchCommand);
			this.tabPageTouch.Controls.Add(this.numericUpDownTouchCommandOption);
			this.tabPageTouch.Controls.Add(this.labelTouchCommandOption);
			this.tabPageTouch.Controls.Add(this.numericUpDownTouchSoundIndex);
			this.tabPageTouch.Controls.Add(this.labelTouchSoundIndex);
			this.tabPageTouch.Controls.Add(this.numericUpDownTouchJumpScreen);
			this.tabPageTouch.Controls.Add(this.labelTouchJumpScreen);
			this.tabPageTouch.Controls.Add(this.groupBoxTouchSize);
			this.tabPageTouch.Controls.Add(this.groupBoxTouchLocation);
			this.tabPageTouch.Location = new System.Drawing.Point(4, 40);
			this.tabPageTouch.Name = "tabPageTouch";
			this.tabPageTouch.Size = new System.Drawing.Size(312, 625);
			this.tabPageTouch.TabIndex = 8;
			this.tabPageTouch.Text = "Touch";
			this.tabPageTouch.UseVisualStyleBackColor = true;
			// 
			// comboBoxTouchCommand
			// 
			this.comboBoxTouchCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTouchCommand.FormattingEnabled = true;
			this.comboBoxTouchCommand.Location = new System.Drawing.Point(136, 216);
			this.comboBoxTouchCommand.Name = "comboBoxTouchCommand";
			this.comboBoxTouchCommand.Size = new System.Drawing.Size(104, 20);
			this.comboBoxTouchCommand.TabIndex = 111;
			// 
			// labelTouchCommand
			// 
			this.labelTouchCommand.Location = new System.Drawing.Point(8, 216);
			this.labelTouchCommand.Name = "labelTouchCommand";
			this.labelTouchCommand.Size = new System.Drawing.Size(120, 16);
			this.labelTouchCommand.TabIndex = 110;
			this.labelTouchCommand.Text = "Command:";
			this.labelTouchCommand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownTouchCommandOption
			// 
			this.numericUpDownTouchCommandOption.Location = new System.Drawing.Point(136, 240);
			this.numericUpDownTouchCommandOption.Name = "numericUpDownTouchCommandOption";
			this.numericUpDownTouchCommandOption.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchCommandOption.TabIndex = 109;
			// 
			// labelTouchCommandOption
			// 
			this.labelTouchCommandOption.Location = new System.Drawing.Point(8, 240);
			this.labelTouchCommandOption.Name = "labelTouchCommandOption";
			this.labelTouchCommandOption.Size = new System.Drawing.Size(120, 16);
			this.labelTouchCommandOption.TabIndex = 108;
			this.labelTouchCommandOption.Text = "CommandOption:";
			this.labelTouchCommandOption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownTouchSoundIndex
			// 
			this.numericUpDownTouchSoundIndex.Location = new System.Drawing.Point(136, 192);
			this.numericUpDownTouchSoundIndex.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numericUpDownTouchSoundIndex.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownTouchSoundIndex.Name = "numericUpDownTouchSoundIndex";
			this.numericUpDownTouchSoundIndex.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchSoundIndex.TabIndex = 107;
			// 
			// labelTouchSoundIndex
			// 
			this.labelTouchSoundIndex.Location = new System.Drawing.Point(8, 192);
			this.labelTouchSoundIndex.Name = "labelTouchSoundIndex";
			this.labelTouchSoundIndex.Size = new System.Drawing.Size(120, 16);
			this.labelTouchSoundIndex.TabIndex = 106;
			this.labelTouchSoundIndex.Text = "SoundIndex:";
			this.labelTouchSoundIndex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownTouchJumpScreen
			// 
			this.numericUpDownTouchJumpScreen.Location = new System.Drawing.Point(136, 168);
			this.numericUpDownTouchJumpScreen.Name = "numericUpDownTouchJumpScreen";
			this.numericUpDownTouchJumpScreen.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchJumpScreen.TabIndex = 105;
			// 
			// labelTouchJumpScreen
			// 
			this.labelTouchJumpScreen.Location = new System.Drawing.Point(8, 168);
			this.labelTouchJumpScreen.Name = "labelTouchJumpScreen";
			this.labelTouchJumpScreen.Size = new System.Drawing.Size(120, 16);
			this.labelTouchJumpScreen.TabIndex = 104;
			this.labelTouchJumpScreen.Text = "JumpScreen:";
			this.labelTouchJumpScreen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxTouchSize
			// 
			this.groupBoxTouchSize.Controls.Add(this.textBoxTouchSizeY);
			this.groupBoxTouchSize.Controls.Add(this.textBoxTouchSizeX);
			this.groupBoxTouchSize.Controls.Add(this.labelTouchSizeY);
			this.groupBoxTouchSize.Controls.Add(this.labelTouchSizeX);
			this.groupBoxTouchSize.Location = new System.Drawing.Point(8, 88);
			this.groupBoxTouchSize.Name = "groupBoxTouchSize";
			this.groupBoxTouchSize.Size = new System.Drawing.Size(184, 72);
			this.groupBoxTouchSize.TabIndex = 90;
			this.groupBoxTouchSize.TabStop = false;
			this.groupBoxTouchSize.Text = "Size";
			// 
			// textBoxTouchSizeY
			// 
			this.textBoxTouchSizeY.Location = new System.Drawing.Point(128, 40);
			this.textBoxTouchSizeY.Name = "textBoxTouchSizeY";
			this.textBoxTouchSizeY.Size = new System.Drawing.Size(48, 19);
			this.textBoxTouchSizeY.TabIndex = 33;
			// 
			// textBoxTouchSizeX
			// 
			this.textBoxTouchSizeX.Location = new System.Drawing.Point(128, 16);
			this.textBoxTouchSizeX.Name = "textBoxTouchSizeX";
			this.textBoxTouchSizeX.Size = new System.Drawing.Size(48, 19);
			this.textBoxTouchSizeX.TabIndex = 32;
			// 
			// labelTouchSizeY
			// 
			this.labelTouchSizeY.Location = new System.Drawing.Point(8, 40);
			this.labelTouchSizeY.Name = "labelTouchSizeY";
			this.labelTouchSizeY.Size = new System.Drawing.Size(112, 16);
			this.labelTouchSizeY.TabIndex = 10;
			this.labelTouchSizeY.Text = "Y:";
			this.labelTouchSizeY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTouchSizeX
			// 
			this.labelTouchSizeX.Location = new System.Drawing.Point(8, 16);
			this.labelTouchSizeX.Name = "labelTouchSizeX";
			this.labelTouchSizeX.Size = new System.Drawing.Size(112, 16);
			this.labelTouchSizeX.TabIndex = 9;
			this.labelTouchSizeX.Text = "X:";
			this.labelTouchSizeX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxTouchLocation
			// 
			this.groupBoxTouchLocation.Controls.Add(this.textBoxTouchLocationY);
			this.groupBoxTouchLocation.Controls.Add(this.textBoxTouchLocationX);
			this.groupBoxTouchLocation.Controls.Add(this.labelTouchLocationY);
			this.groupBoxTouchLocation.Controls.Add(this.labelTouchLocationX);
			this.groupBoxTouchLocation.Location = new System.Drawing.Point(8, 8);
			this.groupBoxTouchLocation.Name = "groupBoxTouchLocation";
			this.groupBoxTouchLocation.Size = new System.Drawing.Size(184, 72);
			this.groupBoxTouchLocation.TabIndex = 89;
			this.groupBoxTouchLocation.TabStop = false;
			this.groupBoxTouchLocation.Text = "Location";
			// 
			// textBoxTouchLocationY
			// 
			this.textBoxTouchLocationY.Location = new System.Drawing.Point(128, 40);
			this.textBoxTouchLocationY.Name = "textBoxTouchLocationY";
			this.textBoxTouchLocationY.Size = new System.Drawing.Size(48, 19);
			this.textBoxTouchLocationY.TabIndex = 33;
			// 
			// textBoxTouchLocationX
			// 
			this.textBoxTouchLocationX.Location = new System.Drawing.Point(128, 16);
			this.textBoxTouchLocationX.Name = "textBoxTouchLocationX";
			this.textBoxTouchLocationX.Size = new System.Drawing.Size(48, 19);
			this.textBoxTouchLocationX.TabIndex = 32;
			// 
			// labelTouchLocationY
			// 
			this.labelTouchLocationY.Location = new System.Drawing.Point(8, 40);
			this.labelTouchLocationY.Name = "labelTouchLocationY";
			this.labelTouchLocationY.Size = new System.Drawing.Size(112, 16);
			this.labelTouchLocationY.TabIndex = 10;
			this.labelTouchLocationY.Text = "Y:";
			this.labelTouchLocationY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTouchLocationX
			// 
			this.labelTouchLocationX.Location = new System.Drawing.Point(8, 16);
			this.labelTouchLocationX.Name = "labelTouchLocationX";
			this.labelTouchLocationX.Size = new System.Drawing.Size(112, 16);
			this.labelTouchLocationX.TabIndex = 9;
			this.labelTouchLocationX.Text = "X:";
			this.labelTouchLocationX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageSound
			// 
			this.tabPageSound.Controls.Add(this.panelSoundSetting);
			this.tabPageSound.Location = new System.Drawing.Point(4, 22);
			this.tabPageSound.Name = "tabPageSound";
			this.tabPageSound.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSound.Size = new System.Drawing.Size(792, 670);
			this.tabPageSound.TabIndex = 0;
			this.tabPageSound.Text = "Sound settings";
			this.tabPageSound.UseVisualStyleBackColor = true;
			// 
			// panelSoundSetting
			// 
			this.panelSoundSetting.Controls.Add(this.splitContainerSound);
			this.panelSoundSetting.Controls.Add(this.panelSoundSettingEdit);
			this.panelSoundSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSoundSetting.Location = new System.Drawing.Point(3, 3);
			this.panelSoundSetting.Name = "panelSoundSetting";
			this.panelSoundSetting.Size = new System.Drawing.Size(786, 664);
			this.panelSoundSetting.TabIndex = 0;
			// 
			// splitContainerSound
			// 
			this.splitContainerSound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerSound.Location = new System.Drawing.Point(0, 0);
			this.splitContainerSound.Name = "splitContainerSound";
			// 
			// splitContainerSound.Panel1
			// 
			this.splitContainerSound.Panel1.Controls.Add(this.treeViewSound);
			// 
			// splitContainerSound.Panel2
			// 
			this.splitContainerSound.Panel2.Controls.Add(this.listViewSound);
			this.splitContainerSound.Size = new System.Drawing.Size(570, 664);
			this.splitContainerSound.SplitterDistance = 190;
			this.splitContainerSound.TabIndex = 2;
			// 
			// treeViewSound
			// 
			this.treeViewSound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewSound.HideSelection = false;
			this.treeViewSound.Location = new System.Drawing.Point(0, 0);
			this.treeViewSound.Name = "treeViewSound";
			this.treeViewSound.Size = new System.Drawing.Size(190, 664);
			this.treeViewSound.TabIndex = 0;
			// 
			// listViewSound
			// 
			this.listViewSound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewSound.FullRowSelect = true;
			this.listViewSound.HideSelection = false;
			this.listViewSound.Location = new System.Drawing.Point(0, 0);
			this.listViewSound.MultiSelect = false;
			this.listViewSound.Name = "listViewSound";
			this.listViewSound.Size = new System.Drawing.Size(376, 664);
			this.listViewSound.TabIndex = 0;
			this.listViewSound.UseCompatibleStateImageBehavior = false;
			this.listViewSound.View = System.Windows.Forms.View.Details;
			// 
			// panelSoundSettingEdit
			// 
			this.panelSoundSettingEdit.Controls.Add(this.groupBoxSoundEntry);
			this.panelSoundSettingEdit.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelSoundSettingEdit.Location = new System.Drawing.Point(570, 0);
			this.panelSoundSettingEdit.Name = "panelSoundSettingEdit";
			this.panelSoundSettingEdit.Size = new System.Drawing.Size(216, 664);
			this.panelSoundSettingEdit.TabIndex = 1;
			// 
			// groupBoxSoundEntry
			// 
			this.groupBoxSoundEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundRemove);
			this.groupBoxSoundEntry.Controls.Add(this.groupBoxSoundKey);
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundAdd);
			this.groupBoxSoundEntry.Controls.Add(this.groupBoxSoundValue);
			this.groupBoxSoundEntry.Location = new System.Drawing.Point(8, 320);
			this.groupBoxSoundEntry.Name = "groupBoxSoundEntry";
			this.groupBoxSoundEntry.Size = new System.Drawing.Size(200, 336);
			this.groupBoxSoundEntry.TabIndex = 6;
			this.groupBoxSoundEntry.TabStop = false;
			this.groupBoxSoundEntry.Text = "Edit entry";
			// 
			// buttonSoundRemove
			// 
			this.buttonSoundRemove.Location = new System.Drawing.Point(72, 304);
			this.buttonSoundRemove.Name = "buttonSoundRemove";
			this.buttonSoundRemove.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundRemove.TabIndex = 4;
			this.buttonSoundRemove.Text = "Remove";
			this.buttonSoundRemove.UseVisualStyleBackColor = true;
			// 
			// groupBoxSoundKey
			// 
			this.groupBoxSoundKey.Controls.Add(this.numericUpDownSoundKeyIndex);
			this.groupBoxSoundKey.Controls.Add(this.comboBoxSoundKey);
			this.groupBoxSoundKey.Location = new System.Drawing.Point(8, 16);
			this.groupBoxSoundKey.Name = "groupBoxSoundKey";
			this.groupBoxSoundKey.Size = new System.Drawing.Size(184, 48);
			this.groupBoxSoundKey.TabIndex = 1;
			this.groupBoxSoundKey.TabStop = false;
			this.groupBoxSoundKey.Text = "Key type select";
			// 
			// numericUpDownSoundKeyIndex
			// 
			this.numericUpDownSoundKeyIndex.Location = new System.Drawing.Point(136, 16);
			this.numericUpDownSoundKeyIndex.Name = "numericUpDownSoundKeyIndex";
			this.numericUpDownSoundKeyIndex.Size = new System.Drawing.Size(40, 19);
			this.numericUpDownSoundKeyIndex.TabIndex = 1;
			// 
			// comboBoxSoundKey
			// 
			this.comboBoxSoundKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSoundKey.FormattingEnabled = true;
			this.comboBoxSoundKey.Location = new System.Drawing.Point(8, 16);
			this.comboBoxSoundKey.Name = "comboBoxSoundKey";
			this.comboBoxSoundKey.Size = new System.Drawing.Size(120, 20);
			this.comboBoxSoundKey.TabIndex = 0;
			// 
			// buttonSoundAdd
			// 
			this.buttonSoundAdd.Location = new System.Drawing.Point(8, 304);
			this.buttonSoundAdd.Name = "buttonSoundAdd";
			this.buttonSoundAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundAdd.TabIndex = 3;
			this.buttonSoundAdd.Text = "Add";
			this.buttonSoundAdd.UseVisualStyleBackColor = true;
			// 
			// groupBoxSoundValue
			// 
			this.groupBoxSoundValue.Controls.Add(this.checkBoxSoundRadius);
			this.groupBoxSoundValue.Controls.Add(this.checkBoxSoundDefinedPosition);
			this.groupBoxSoundValue.Controls.Add(this.textBoxSoundRadius);
			this.groupBoxSoundValue.Controls.Add(this.groupBoxSoundPosition);
			this.groupBoxSoundValue.Controls.Add(this.labelSoundFileName);
			this.groupBoxSoundValue.Controls.Add(this.buttonSoundFileNameOpen);
			this.groupBoxSoundValue.Controls.Add(this.textBoxSoundFileName);
			this.groupBoxSoundValue.Location = new System.Drawing.Point(8, 72);
			this.groupBoxSoundValue.Name = "groupBoxSoundValue";
			this.groupBoxSoundValue.Size = new System.Drawing.Size(184, 224);
			this.groupBoxSoundValue.TabIndex = 2;
			this.groupBoxSoundValue.TabStop = false;
			this.groupBoxSoundValue.Text = "Input value";
			// 
			// checkBoxSoundRadius
			// 
			this.checkBoxSoundRadius.Location = new System.Drawing.Point(8, 72);
			this.checkBoxSoundRadius.Name = "checkBoxSoundRadius";
			this.checkBoxSoundRadius.Size = new System.Drawing.Size(80, 16);
			this.checkBoxSoundRadius.TabIndex = 22;
			this.checkBoxSoundRadius.Text = "Radius:";
			this.checkBoxSoundRadius.UseVisualStyleBackColor = true;
			// 
			// checkBoxSoundDefinedPosition
			// 
			this.checkBoxSoundDefinedPosition.Location = new System.Drawing.Point(8, 96);
			this.checkBoxSoundDefinedPosition.Name = "checkBoxSoundDefinedPosition";
			this.checkBoxSoundDefinedPosition.Size = new System.Drawing.Size(104, 16);
			this.checkBoxSoundDefinedPosition.TabIndex = 21;
			this.checkBoxSoundDefinedPosition.Text = "Position setting";
			this.checkBoxSoundDefinedPosition.UseVisualStyleBackColor = true;
			// 
			// textBoxSoundRadius
			// 
			this.textBoxSoundRadius.Location = new System.Drawing.Point(96, 72);
			this.textBoxSoundRadius.Name = "textBoxSoundRadius";
			this.textBoxSoundRadius.Size = new System.Drawing.Size(56, 19);
			this.textBoxSoundRadius.TabIndex = 20;
			// 
			// groupBoxSoundPosition
			// 
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionZUnit);
			this.groupBoxSoundPosition.Controls.Add(this.textBoxSoundPositionZ);
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionZ);
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionYUnit);
			this.groupBoxSoundPosition.Controls.Add(this.textBoxSoundPositionY);
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionY);
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionXUnit);
			this.groupBoxSoundPosition.Controls.Add(this.textBoxSoundPositionX);
			this.groupBoxSoundPosition.Controls.Add(this.labelSoundPositionX);
			this.groupBoxSoundPosition.Location = new System.Drawing.Point(8, 120);
			this.groupBoxSoundPosition.Name = "groupBoxSoundPosition";
			this.groupBoxSoundPosition.Size = new System.Drawing.Size(168, 96);
			this.groupBoxSoundPosition.TabIndex = 3;
			this.groupBoxSoundPosition.TabStop = false;
			this.groupBoxSoundPosition.Text = "Position";
			// 
			// labelSoundPositionZUnit
			// 
			this.labelSoundPositionZUnit.Location = new System.Drawing.Point(152, 64);
			this.labelSoundPositionZUnit.Name = "labelSoundPositionZUnit";
			this.labelSoundPositionZUnit.Size = new System.Drawing.Size(8, 16);
			this.labelSoundPositionZUnit.TabIndex = 26;
			this.labelSoundPositionZUnit.Text = "m";
			this.labelSoundPositionZUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxSoundPositionZ
			// 
			this.textBoxSoundPositionZ.Location = new System.Drawing.Point(88, 64);
			this.textBoxSoundPositionZ.Name = "textBoxSoundPositionZ";
			this.textBoxSoundPositionZ.Size = new System.Drawing.Size(56, 19);
			this.textBoxSoundPositionZ.TabIndex = 25;
			// 
			// labelSoundPositionZ
			// 
			this.labelSoundPositionZ.Location = new System.Drawing.Point(8, 64);
			this.labelSoundPositionZ.Name = "labelSoundPositionZ";
			this.labelSoundPositionZ.Size = new System.Drawing.Size(72, 16);
			this.labelSoundPositionZ.TabIndex = 24;
			this.labelSoundPositionZ.Text = "z coordinate:";
			this.labelSoundPositionZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSoundPositionYUnit
			// 
			this.labelSoundPositionYUnit.Location = new System.Drawing.Point(152, 40);
			this.labelSoundPositionYUnit.Name = "labelSoundPositionYUnit";
			this.labelSoundPositionYUnit.Size = new System.Drawing.Size(8, 16);
			this.labelSoundPositionYUnit.TabIndex = 23;
			this.labelSoundPositionYUnit.Text = "m";
			this.labelSoundPositionYUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxSoundPositionY
			// 
			this.textBoxSoundPositionY.Location = new System.Drawing.Point(88, 40);
			this.textBoxSoundPositionY.Name = "textBoxSoundPositionY";
			this.textBoxSoundPositionY.Size = new System.Drawing.Size(56, 19);
			this.textBoxSoundPositionY.TabIndex = 22;
			// 
			// labelSoundPositionY
			// 
			this.labelSoundPositionY.Location = new System.Drawing.Point(8, 40);
			this.labelSoundPositionY.Name = "labelSoundPositionY";
			this.labelSoundPositionY.Size = new System.Drawing.Size(72, 16);
			this.labelSoundPositionY.TabIndex = 21;
			this.labelSoundPositionY.Text = "y coordinate:";
			this.labelSoundPositionY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSoundPositionXUnit
			// 
			this.labelSoundPositionXUnit.Location = new System.Drawing.Point(152, 16);
			this.labelSoundPositionXUnit.Name = "labelSoundPositionXUnit";
			this.labelSoundPositionXUnit.Size = new System.Drawing.Size(8, 16);
			this.labelSoundPositionXUnit.TabIndex = 20;
			this.labelSoundPositionXUnit.Text = "m";
			this.labelSoundPositionXUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxSoundPositionX
			// 
			this.textBoxSoundPositionX.Location = new System.Drawing.Point(88, 16);
			this.textBoxSoundPositionX.Name = "textBoxSoundPositionX";
			this.textBoxSoundPositionX.Size = new System.Drawing.Size(56, 19);
			this.textBoxSoundPositionX.TabIndex = 19;
			// 
			// labelSoundPositionX
			// 
			this.labelSoundPositionX.Location = new System.Drawing.Point(8, 16);
			this.labelSoundPositionX.Name = "labelSoundPositionX";
			this.labelSoundPositionX.Size = new System.Drawing.Size(72, 16);
			this.labelSoundPositionX.TabIndex = 18;
			this.labelSoundPositionX.Text = "x coordinate:";
			this.labelSoundPositionX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSoundFileName
			// 
			this.labelSoundFileName.Location = new System.Drawing.Point(8, 16);
			this.labelSoundFileName.Name = "labelSoundFileName";
			this.labelSoundFileName.Size = new System.Drawing.Size(56, 16);
			this.labelSoundFileName.TabIndex = 2;
			this.labelSoundFileName.Text = "Filename:";
			this.labelSoundFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonSoundFileNameOpen
			// 
			this.buttonSoundFileNameOpen.Location = new System.Drawing.Point(120, 40);
			this.buttonSoundFileNameOpen.Name = "buttonSoundFileNameOpen";
			this.buttonSoundFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundFileNameOpen.TabIndex = 1;
			this.buttonSoundFileNameOpen.Text = "Open...";
			this.buttonSoundFileNameOpen.UseVisualStyleBackColor = true;
			this.buttonSoundFileNameOpen.Click += new System.EventHandler(this.ButtonSoundFileNameOpen_Click);
			// 
			// textBoxSoundFileName
			// 
			this.textBoxSoundFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxSoundFileName.Name = "textBoxSoundFileName";
			this.textBoxSoundFileName.Size = new System.Drawing.Size(104, 19);
			this.textBoxSoundFileName.TabIndex = 0;
			// 
			// tabPageStatus
			// 
			this.tabPageStatus.Controls.Add(this.listViewStatus);
			this.tabPageStatus.Controls.Add(this.menuStripStatus);
			this.tabPageStatus.Location = new System.Drawing.Point(4, 22);
			this.tabPageStatus.Name = "tabPageStatus";
			this.tabPageStatus.Size = new System.Drawing.Size(792, 670);
			this.tabPageStatus.TabIndex = 6;
			this.tabPageStatus.Text = "Status (0)";
			this.tabPageStatus.UseVisualStyleBackColor = true;
			// 
			// listViewStatus
			// 
			this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderLevel,
            this.columnHeaderText});
			this.listViewStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewStatus.FullRowSelect = true;
			this.listViewStatus.HideSelection = false;
			this.listViewStatus.Location = new System.Drawing.Point(0, 24);
			this.listViewStatus.MultiSelect = false;
			this.listViewStatus.Name = "listViewStatus";
			this.listViewStatus.Size = new System.Drawing.Size(792, 646);
			this.listViewStatus.TabIndex = 0;
			this.listViewStatus.UseCompatibleStateImageBehavior = false;
			this.listViewStatus.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderLevel
			// 
			this.columnHeaderLevel.Text = "Level";
			// 
			// columnHeaderText
			// 
			this.columnHeaderText.Text = "Description";
			// 
			// menuStripStatus
			// 
			this.menuStripStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemError,
            this.toolStripMenuItemWarning,
            this.toolStripMenuItemInfo,
            this.toolStripMenuItemClear});
			this.menuStripStatus.Location = new System.Drawing.Point(0, 0);
			this.menuStripStatus.Name = "menuStripStatus";
			this.menuStripStatus.Size = new System.Drawing.Size(792, 24);
			this.menuStripStatus.TabIndex = 1;
			this.menuStripStatus.Text = "menuStrip1";
			// 
			// toolStripMenuItemError
			// 
			this.toolStripMenuItemError.Name = "toolStripMenuItemError";
			this.toolStripMenuItemError.Size = new System.Drawing.Size(52, 20);
			this.toolStripMenuItemError.Text = "0 Error";
			// 
			// toolStripMenuItemWarning
			// 
			this.toolStripMenuItemWarning.Name = "toolStripMenuItemWarning";
			this.toolStripMenuItemWarning.Size = new System.Drawing.Size(67, 20);
			this.toolStripMenuItemWarning.Text = "0 Warning";
			// 
			// toolStripMenuItemInfo
			// 
			this.toolStripMenuItemInfo.Name = "toolStripMenuItemInfo";
			this.toolStripMenuItemInfo.Size = new System.Drawing.Size(84, 20);
			this.toolStripMenuItemInfo.Text = "0 Information";
			// 
			// toolStripMenuItemClear
			// 
			this.toolStripMenuItemClear.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripMenuItemClear.Name = "toolStripMenuItemClear";
			this.toolStripMenuItemClear.Size = new System.Drawing.Size(44, 20);
			this.toolStripMenuItemClear.Text = "Clear";
			// 
			// panelCars
			// 
			this.panelCars.Controls.Add(this.treeViewCars);
			this.panelCars.Controls.Add(this.panelCarsNavi);
			this.panelCars.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelCars.Location = new System.Drawing.Point(0, 24);
			this.panelCars.Name = "panelCars";
			this.panelCars.Size = new System.Drawing.Size(200, 696);
			this.panelCars.TabIndex = 10;
			// 
			// treeViewCars
			// 
			this.treeViewCars.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewCars.HideSelection = false;
			this.treeViewCars.Location = new System.Drawing.Point(0, 0);
			this.treeViewCars.Name = "treeViewCars";
			this.treeViewCars.Size = new System.Drawing.Size(200, 624);
			this.treeViewCars.TabIndex = 2;
			// 
			// panelCarsNavi
			// 
			this.panelCarsNavi.Controls.Add(this.buttonCarsCopy);
			this.panelCarsNavi.Controls.Add(this.buttonCarsRemove);
			this.panelCarsNavi.Controls.Add(this.buttonCarsAdd);
			this.panelCarsNavi.Controls.Add(this.buttonCarsUp);
			this.panelCarsNavi.Controls.Add(this.buttonCarsDown);
			this.panelCarsNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelCarsNavi.Location = new System.Drawing.Point(0, 624);
			this.panelCarsNavi.Name = "panelCarsNavi";
			this.panelCarsNavi.Size = new System.Drawing.Size(200, 72);
			this.panelCarsNavi.TabIndex = 3;
			// 
			// buttonCarsCopy
			// 
			this.buttonCarsCopy.Location = new System.Drawing.Point(72, 40);
			this.buttonCarsCopy.Name = "buttonCarsCopy";
			this.buttonCarsCopy.Size = new System.Drawing.Size(56, 24);
			this.buttonCarsCopy.TabIndex = 5;
			this.buttonCarsCopy.Text = "Copy";
			this.buttonCarsCopy.UseVisualStyleBackColor = true;
			// 
			// buttonCarsRemove
			// 
			this.buttonCarsRemove.Location = new System.Drawing.Point(136, 8);
			this.buttonCarsRemove.Name = "buttonCarsRemove";
			this.buttonCarsRemove.Size = new System.Drawing.Size(56, 24);
			this.buttonCarsRemove.TabIndex = 3;
			this.buttonCarsRemove.Text = "Remove";
			this.buttonCarsRemove.UseVisualStyleBackColor = true;
			// 
			// buttonCarsAdd
			// 
			this.buttonCarsAdd.Location = new System.Drawing.Point(72, 8);
			this.buttonCarsAdd.Name = "buttonCarsAdd";
			this.buttonCarsAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonCarsAdd.TabIndex = 2;
			this.buttonCarsAdd.Text = "Add";
			this.buttonCarsAdd.UseVisualStyleBackColor = true;
			// 
			// buttonCarsUp
			// 
			this.buttonCarsUp.Location = new System.Drawing.Point(8, 8);
			this.buttonCarsUp.Name = "buttonCarsUp";
			this.buttonCarsUp.Size = new System.Drawing.Size(56, 24);
			this.buttonCarsUp.TabIndex = 0;
			this.buttonCarsUp.Text = "Up";
			this.buttonCarsUp.UseVisualStyleBackColor = true;
			// 
			// buttonCarsDown
			// 
			this.buttonCarsDown.Location = new System.Drawing.Point(8, 40);
			this.buttonCarsDown.Name = "buttonCarsDown";
			this.buttonCarsDown.Size = new System.Drawing.Size(56, 24);
			this.buttonCarsDown.TabIndex = 1;
			this.buttonCarsDown.Text = "Down";
			this.buttonCarsDown.UseVisualStyleBackColor = true;
			// 
			// menuStripMenu
			// 
			this.menuStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile,
            this.toolStripComboBoxLanguage,
            this.toolStripStatusLabelLanguage});
			this.menuStripMenu.Location = new System.Drawing.Point(0, 0);
			this.menuStripMenu.Name = "menuStripMenu";
			this.menuStripMenu.Size = new System.Drawing.Size(1000, 24);
			this.menuStripMenu.TabIndex = 8;
			this.menuStripMenu.Text = "menuStrip1";
			// 
			// toolStripMenuItemFile
			// 
			this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNew,
            this.toolStripMenuItemOpen,
            this.toolStripSeparatorOpen,
            this.toolStripMenuItemSave,
            this.toolStripMenuItemSaveAs,
            this.toolStripSeparatorSave,
            this.toolStripMenuItemImport,
            this.toolStripMenuItemExport,
            this.toolStripSeparatorExport,
            this.toolStripMenuItemExit});
			this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
			this.toolStripMenuItemFile.Size = new System.Drawing.Size(51, 20);
			this.toolStripMenuItemFile.Text = "File(&F)";
			// 
			// toolStripMenuItemNew
			// 
			this.toolStripMenuItemNew.Name = "toolStripMenuItemNew";
			this.toolStripMenuItemNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.toolStripMenuItemNew.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemNew.Text = "New(&N)";
			// 
			// toolStripMenuItemOpen
			// 
			this.toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
			this.toolStripMenuItemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.toolStripMenuItemOpen.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemOpen.Text = "Open...(&O)";
			// 
			// toolStripSeparatorOpen
			// 
			this.toolStripSeparatorOpen.Name = "toolStripSeparatorOpen";
			this.toolStripSeparatorOpen.Size = new System.Drawing.Size(197, 6);
			// 
			// toolStripMenuItemSave
			// 
			this.toolStripMenuItemSave.Name = "toolStripMenuItemSave";
			this.toolStripMenuItemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.toolStripMenuItemSave.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemSave.Text = "Save(&S)";
			// 
			// toolStripMenuItemSaveAs
			// 
			this.toolStripMenuItemSaveAs.Name = "toolStripMenuItemSaveAs";
			this.toolStripMenuItemSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.toolStripMenuItemSaveAs.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemSaveAs.Text = "Save as...(&A)";
			// 
			// toolStripSeparatorSave
			// 
			this.toolStripSeparatorSave.Name = "toolStripSeparatorSave";
			this.toolStripSeparatorSave.Size = new System.Drawing.Size(197, 6);
			// 
			// toolStripMenuItemImport
			// 
			this.toolStripMenuItemImport.Name = "toolStripMenuItemImport";
			this.toolStripMenuItemImport.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemImport.Text = "Import...";
			this.toolStripMenuItemImport.Click += new System.EventHandler(this.ToolStripMenuItemImport_Click);
			// 
			// toolStripMenuItemExport
			// 
			this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
			this.toolStripMenuItemExport.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemExport.Text = "Export...";
			this.toolStripMenuItemExport.Click += new System.EventHandler(this.ToolStripMenuItemExport_Click);
			// 
			// toolStripSeparatorExport
			// 
			this.toolStripSeparatorExport.Name = "toolStripSeparatorExport";
			this.toolStripSeparatorExport.Size = new System.Drawing.Size(197, 6);
			// 
			// toolStripMenuItemExit
			// 
			this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
			this.toolStripMenuItemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.toolStripMenuItemExit.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemExit.Text = "Exit(&X)";
			this.toolStripMenuItemExit.Click += new System.EventHandler(this.ToolStripMenuItemExit_Click);
			// 
			// toolStripComboBoxLanguage
			// 
			this.toolStripComboBoxLanguage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripComboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxLanguage.Name = "toolStripComboBoxLanguage";
			this.toolStripComboBoxLanguage.Size = new System.Drawing.Size(150, 20);
			this.toolStripComboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBoxLanguage_SelectedIndexChanged);
			// 
			// toolStripStatusLabelLanguage
			// 
			this.toolStripStatusLabelLanguage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripStatusLabelLanguage.Name = "toolStripStatusLabelLanguage";
			this.toolStripStatusLabelLanguage.Size = new System.Drawing.Size(55, 15);
			this.toolStripStatusLabelLanguage.Text = "Language:";
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1000, 720);
			this.Controls.Add(this.tabControlEditor);
			this.Controls.Add(this.panelCars);
			this.Controls.Add(this.menuStripMenu);
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStripMenu;
			this.Name = "FormEditor";
			this.Text = "FormEditor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditor_FormClosing);
			this.Load += new System.EventHandler(this.FormEditor_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormEditor_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormEditor_KeyUp);
			this.tabControlEditor.ResumeLayout(false);
			this.tabPageTrain.ResumeLayout(false);
			this.groupBoxDevice.ResumeLayout(false);
			this.groupBoxDevice.PerformLayout();
			this.groupBoxCab.ResumeLayout(false);
			this.groupBoxCab.PerformLayout();
			this.groupBoxHandle.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLocoBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverPowerNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotchReduceSteps)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotches)).EndInit();
			this.tabPageCar.ResumeLayout(false);
			this.groupBoxPressure.ResumeLayout(false);
			this.groupBoxPressure.PerformLayout();
			this.groupBoxBrake.ResumeLayout(false);
			this.groupBoxBrake.PerformLayout();
			this.groupBoxMove.ResumeLayout(false);
			this.groupBoxMove.PerformLayout();
			this.groupBoxDelay.ResumeLayout(false);
			this.groupBoxPerformance.ResumeLayout(false);
			this.groupBoxPerformance.PerformLayout();
			this.groupBoxCarGeneral.ResumeLayout(false);
			this.groupBoxCarGeneral.PerformLayout();
			this.groupBoxAxles.ResumeLayout(false);
			this.groupBoxAxles.PerformLayout();
			this.tabPageAccel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxAccel)).EndInit();
			this.panelAccel.ResumeLayout(false);
			this.groupBoxPreview.ResumeLayout(false);
			this.groupBoxPreview.PerformLayout();
			this.groupBoxParameter.ResumeLayout(false);
			this.groupBoxParameter.PerformLayout();
			this.groupBoxNotch.ResumeLayout(false);
			this.tabPageMotor.ResumeLayout(false);
			this.panelMotorSound.ResumeLayout(false);
			this.panelMotorSound.PerformLayout();
			this.toolStripContainerDrawArea.ContentPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.PerformLayout();
			this.toolStripContainerDrawArea.ResumeLayout(false);
			this.toolStripContainerDrawArea.PerformLayout();
			this.toolStripToolBar.ResumeLayout(false);
			this.toolStripToolBar.PerformLayout();
			this.panelMotorSetting.ResumeLayout(false);
			this.groupBoxDirect.ResumeLayout(false);
			this.groupBoxDirect.PerformLayout();
			this.groupBoxPlay.ResumeLayout(false);
			this.groupBoxArea.ResumeLayout(false);
			this.groupBoxArea.PerformLayout();
			this.groupBoxSource.ResumeLayout(false);
			this.groupBoxSource.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunIndex)).EndInit();
			this.groupBoxView.ResumeLayout(false);
			this.groupBoxView.PerformLayout();
			this.statusStripStatus.ResumeLayout(false);
			this.statusStripStatus.PerformLayout();
			this.menuStripMotor.ResumeLayout(false);
			this.menuStripMotor.PerformLayout();
			this.tabPageCoupler.ResumeLayout(false);
			this.groupBoxCouplerGeneral.ResumeLayout(false);
			this.groupBoxCouplerGeneral.PerformLayout();
			this.tabPagePanel.ResumeLayout(false);
			this.splitContainerPanel.Panel1.ResumeLayout(false);
			this.splitContainerPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPanel)).EndInit();
			this.splitContainerPanel.ResumeLayout(false);
			this.panelPanelNavi.ResumeLayout(false);
			this.panelPanelNaviCmd.ResumeLayout(false);
			this.tabControlPanel.ResumeLayout(false);
			this.tabPageThis.ResumeLayout(false);
			this.tabPageThis.PerformLayout();
			this.groupBoxThisOrigin.ResumeLayout(false);
			this.groupBoxThisOrigin.PerformLayout();
			this.groupBoxThisCenter.ResumeLayout(false);
			this.groupBoxThisCenter.PerformLayout();
			this.tabPageScreen.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenLayer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownScreenNumber)).EndInit();
			this.tabPagePilotLamp.ResumeLayout(false);
			this.tabPagePilotLamp.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPilotLampLayer)).EndInit();
			this.groupBoxPilotLampLocation.ResumeLayout(false);
			this.groupBoxPilotLampLocation.PerformLayout();
			this.tabPageNeedle.ResumeLayout(false);
			this.tabPageNeedle.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNeedleLayer)).EndInit();
			this.groupBoxNeedleOrigin.ResumeLayout(false);
			this.groupBoxNeedleOrigin.PerformLayout();
			this.groupBoxNeedleLocation.ResumeLayout(false);
			this.groupBoxNeedleLocation.PerformLayout();
			this.tabPageDigitalNumber.ResumeLayout(false);
			this.tabPageDigitalNumber.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalNumberLayer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalNumberInterval)).EndInit();
			this.groupBoxDigitalNumberLocation.ResumeLayout(false);
			this.groupBoxDigitalNumberLocation.PerformLayout();
			this.tabPageDigitalGauge.ResumeLayout(false);
			this.tabPageDigitalGauge.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDigitalGaugeLayer)).EndInit();
			this.groupBoxDigitalGaugeLocation.ResumeLayout(false);
			this.groupBoxDigitalGaugeLocation.PerformLayout();
			this.tabPageLinearGauge.ResumeLayout(false);
			this.tabPageLinearGauge.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeLayer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeWidth)).EndInit();
			this.groupBoxLinearGaugeDirection.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeDirectionY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinearGaugeDirectionX)).EndInit();
			this.groupBoxLinearGaugeLocation.ResumeLayout(false);
			this.groupBoxLinearGaugeLocation.PerformLayout();
			this.tabPageTimetable.ResumeLayout(false);
			this.tabPageTimetable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimetableLayer)).EndInit();
			this.groupBoxTimetableLocation.ResumeLayout(false);
			this.groupBoxTimetableLocation.PerformLayout();
			this.tabPageTouch.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchCommandOption)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchSoundIndex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchJumpScreen)).EndInit();
			this.groupBoxTouchSize.ResumeLayout(false);
			this.groupBoxTouchSize.PerformLayout();
			this.groupBoxTouchLocation.ResumeLayout(false);
			this.groupBoxTouchLocation.PerformLayout();
			this.tabPageSound.ResumeLayout(false);
			this.panelSoundSetting.ResumeLayout(false);
			this.splitContainerSound.Panel1.ResumeLayout(false);
			this.splitContainerSound.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerSound)).EndInit();
			this.splitContainerSound.ResumeLayout(false);
			this.panelSoundSettingEdit.ResumeLayout(false);
			this.groupBoxSoundEntry.ResumeLayout(false);
			this.groupBoxSoundKey.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundKeyIndex)).EndInit();
			this.groupBoxSoundValue.ResumeLayout(false);
			this.groupBoxSoundValue.PerformLayout();
			this.groupBoxSoundPosition.ResumeLayout(false);
			this.groupBoxSoundPosition.PerformLayout();
			this.tabPageStatus.ResumeLayout(false);
			this.tabPageStatus.PerformLayout();
			this.menuStripStatus.ResumeLayout(false);
			this.menuStripStatus.PerformLayout();
			this.panelCars.ResumeLayout(false);
			this.panelCarsNavi.ResumeLayout(false);
			this.menuStripMenu.ResumeLayout(false);
			this.menuStripMenu.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl tabControlEditor;
		private System.Windows.Forms.TabPage tabPageTrain;
		private System.Windows.Forms.GroupBox groupBoxDevice;
		private System.Windows.Forms.Label labelDoorMaxToleranceUnit;
		private System.Windows.Forms.TextBox textBoxDoorMaxTolerance;
		private System.Windows.Forms.Label labelDoorWidthUnit;
		private System.Windows.Forms.TextBox textBoxDoorWidth;
		private System.Windows.Forms.ComboBox comboBoxDoorCloseMode;
		private System.Windows.Forms.ComboBox comboBoxDoorOpenMode;
		private System.Windows.Forms.ComboBox comboBoxPassAlarm;
		private System.Windows.Forms.ComboBox comboBoxReAdhesionDevice;
		private System.Windows.Forms.ComboBox comboBoxAtc;
		private System.Windows.Forms.ComboBox comboBoxAts;
		private System.Windows.Forms.CheckBox checkBoxHoldBrake;
		private System.Windows.Forms.CheckBox checkBoxEb;
		private System.Windows.Forms.CheckBox checkBoxConstSpeed;
		private System.Windows.Forms.Label labelDoorMaxTolerance;
		private System.Windows.Forms.Label labelDoorWidth;
		private System.Windows.Forms.Label labelDoorCloseMode;
		private System.Windows.Forms.Label labelDoorOpenMode;
		private System.Windows.Forms.Label labelPassAlarm;
		private System.Windows.Forms.Label labelReAdhesionDevice;
		private System.Windows.Forms.Label labelHoldBrake;
		private System.Windows.Forms.Label labelConstSpeed;
		private System.Windows.Forms.Label labelEb;
		private System.Windows.Forms.Label labelAtc;
		private System.Windows.Forms.Label labelAts;
		private System.Windows.Forms.GroupBox groupBoxCab;
		private System.Windows.Forms.Label labelCabZUnit;
		private System.Windows.Forms.TextBox textBoxCabZ;
		private System.Windows.Forms.Label labelCabYUnit;
		private System.Windows.Forms.TextBox textBoxCabY;
		private System.Windows.Forms.Label labelCabXUnit;
		private System.Windows.Forms.TextBox textBoxCabX;
		private System.Windows.Forms.Label labelDriverCar;
		private System.Windows.Forms.Label labelCabZ;
		private System.Windows.Forms.Label labelCabY;
		private System.Windows.Forms.Label labelCabX;
		private System.Windows.Forms.GroupBox groupBoxHandle;
		private System.Windows.Forms.NumericUpDown numericUpDownLocoBrakeNotches;
		private System.Windows.Forms.Label labelLocoBrakeNotches;
		private System.Windows.Forms.ComboBox comboBoxLocoBrakeHandleType;
		private System.Windows.Forms.Label labelLocoBrakeHandleType;
		private System.Windows.Forms.ComboBox comboBoxEbHandleBehaviour;
		private System.Windows.Forms.Label labelEbHandleBehaviour;
		private System.Windows.Forms.NumericUpDown numericUpDownDriverBrakeNotches;
		private System.Windows.Forms.Label labelDriverBrakeNotches;
		private System.Windows.Forms.NumericUpDown numericUpDownDriverPowerNotches;
		private System.Windows.Forms.Label labelDriverPowerNotches;
		private System.Windows.Forms.NumericUpDown numericUpDownPowerNotchReduceSteps;
		private System.Windows.Forms.Label labelPowerNotchReduceSteps;
		private System.Windows.Forms.NumericUpDown numericUpDownBrakeNotches;
		private System.Windows.Forms.Label labelBrakeNotches;
		private System.Windows.Forms.NumericUpDown numericUpDownPowerNotches;
		private System.Windows.Forms.Label labelPowerNotches;
		private System.Windows.Forms.ComboBox comboBoxHandleType;
		private System.Windows.Forms.Label labelHandleType;
		private System.Windows.Forms.TabPage tabPageCar;
		private System.Windows.Forms.GroupBox groupBoxPressure;
		private System.Windows.Forms.Label labelBrakeCylinderServiceMaximumPressureUnit;
		private System.Windows.Forms.Label labelMainReservoirMaximumPressureUnit;
		private System.Windows.Forms.Label labelMainReservoirMinimumPressureUnit;
		private System.Windows.Forms.Label labelBrakeCylinderEmergencyMaximumPressureUnit;
		private System.Windows.Forms.TextBox textBoxMainReservoirMaximumPressure;
		private System.Windows.Forms.TextBox textBoxMainReservoirMinimumPressure;
		private System.Windows.Forms.TextBox textBoxBrakeCylinderEmergencyMaximumPressure;
		private System.Windows.Forms.TextBox textBoxBrakeCylinderServiceMaximumPressure;
		private System.Windows.Forms.Label labelMainReservoirMaximumPressure;
		private System.Windows.Forms.Label labelMainReservoirMinimumPressure;
		private System.Windows.Forms.Label labelBrakeCylinderServiceMaximumPressure;
		private System.Windows.Forms.Label labelBrakeCylinderEmergencyMaximumPressure;
		private System.Windows.Forms.GroupBox groupBoxBrake;
		private System.Windows.Forms.TextBox textBoxBrakeControlSpeed;
		private System.Windows.Forms.Label labelBrakeControlSpeedUnit;
		private System.Windows.Forms.ComboBox comboBoxBrakeControlSystem;
		private System.Windows.Forms.ComboBox comboBoxLocoBrakeType;
		private System.Windows.Forms.ComboBox comboBoxBrakeType;
		private System.Windows.Forms.Label labelLocoBrakeType;
		private System.Windows.Forms.Label labelBrakeType;
		private System.Windows.Forms.Label labelBrakeControlSpeed;
		private System.Windows.Forms.Label labelBrakeControlSystem;
		private System.Windows.Forms.GroupBox groupBoxMove;
		private System.Windows.Forms.Label labelBrakeCylinderDownUnit;
		private System.Windows.Forms.TextBox textBoxBrakeCylinderDown;
		private System.Windows.Forms.Label labelBrakeCylinderUpUnit;
		private System.Windows.Forms.TextBox textBoxBrakeCylinderUp;
		private System.Windows.Forms.Label labelJerkBrakeDownUnit;
		private System.Windows.Forms.TextBox textBoxJerkBrakeDown;
		private System.Windows.Forms.Label labelJerkBrakeUpUnit;
		private System.Windows.Forms.TextBox textBoxJerkBrakeUp;
		private System.Windows.Forms.Label labelJerkPowerDownUnit;
		private System.Windows.Forms.TextBox textBoxJerkPowerDown;
		private System.Windows.Forms.Label labelJerkPowerUpUnit;
		private System.Windows.Forms.TextBox textBoxJerkPowerUp;
		private System.Windows.Forms.Label labelJerkPowerUp;
		private System.Windows.Forms.Label labelJerkPowerDown;
		private System.Windows.Forms.Label labelJerkBrakeUp;
		private System.Windows.Forms.Label labelJerkBrakeDown;
		private System.Windows.Forms.Label labelBrakeCylinderUp;
		private System.Windows.Forms.Label labelBrakeCylinderDown;
		private System.Windows.Forms.GroupBox groupBoxDelay;
		private System.Windows.Forms.Button buttonDelayLocoBrakeSet;
		private System.Windows.Forms.Button buttonDelayBrakeSet;
		private System.Windows.Forms.Button buttonDelayPowerSet;
		private System.Windows.Forms.Label labelDelayLocoBrake;
		private System.Windows.Forms.Label labelDelayBrake;
		private System.Windows.Forms.Label labelDelayPower;
		private System.Windows.Forms.GroupBox groupBoxPerformance;
		private System.Windows.Forms.TextBox textBoxAerodynamicDragCoefficient;
		private System.Windows.Forms.TextBox textBoxCoefficientOfRollingResistance;
		private System.Windows.Forms.TextBox textBoxCoefficientOfStaticFriction;
		private System.Windows.Forms.Label labelDecelerationUnit;
		private System.Windows.Forms.TextBox textBoxDeceleration;
		private System.Windows.Forms.Label labelAerodynamicDragCoefficient;
		private System.Windows.Forms.Label labelCoefficientOfStaticFriction;
		private System.Windows.Forms.Label labelDeceleration;
		private System.Windows.Forms.Label labelCoefficientOfRollingResistance;
		private System.Windows.Forms.GroupBox groupBoxCarGeneral;
		private System.Windows.Forms.Label labelUnexposedFrontalAreaUnit;
		private System.Windows.Forms.TextBox textBoxUnexposedFrontalArea;
		private System.Windows.Forms.Label labelUnexposedFrontalArea;
		private System.Windows.Forms.Label labelRearBogie;
		private System.Windows.Forms.Label labelFrontBogie;
		private System.Windows.Forms.Button buttonRearBogieSet;
		private System.Windows.Forms.Button buttonFrontBogieSet;
		private System.Windows.Forms.CheckBox checkBoxReversed;
		private System.Windows.Forms.CheckBox checkBoxLoadingSway;
		private System.Windows.Forms.CheckBox checkBoxDefinedAxles;
		private System.Windows.Forms.CheckBox checkBoxIsMotorCar;
		private System.Windows.Forms.Button buttonObjectOpen;
		private System.Windows.Forms.Label labelExposedFrontalAreaUnit;
		private System.Windows.Forms.Label labelCenterOfMassHeightUnit;
		private System.Windows.Forms.Label labelLoadingSway;
		private System.Windows.Forms.Label labelHeightUnit;
		private System.Windows.Forms.Label labelWidthUnit;
		private System.Windows.Forms.Label labelLengthUnit;
		private System.Windows.Forms.Label labelMassUnit;
		private System.Windows.Forms.TextBox textBoxMass;
		private System.Windows.Forms.TextBox textBoxLength;
		private System.Windows.Forms.TextBox textBoxWidth;
		private System.Windows.Forms.TextBox textBoxHeight;
		private System.Windows.Forms.TextBox textBoxCenterOfMassHeight;
		private System.Windows.Forms.TextBox textBoxExposedFrontalArea;
		private System.Windows.Forms.TextBox textBoxObject;
		private System.Windows.Forms.Label labelObject;
		private System.Windows.Forms.Label labelReversed;
		private System.Windows.Forms.GroupBox groupBoxAxles;
		private System.Windows.Forms.Label labelRearAxleUnit;
		private System.Windows.Forms.Label labelFrontAxleUnit;
		private System.Windows.Forms.TextBox textBoxRearAxle;
		private System.Windows.Forms.TextBox textBoxFrontAxle;
		private System.Windows.Forms.Label labelRearAxle;
		private System.Windows.Forms.Label labelFrontAxle;
		private System.Windows.Forms.Label labelDefinedAxles;
		private System.Windows.Forms.Label labelExposedFrontalArea;
		private System.Windows.Forms.Label labelCenterOfMassHeight;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Label labelLength;
		private System.Windows.Forms.Label labelMass;
		private System.Windows.Forms.Label labelIsMotorCar;
		private System.Windows.Forms.TabPage tabPageAccel;
		private System.Windows.Forms.PictureBox pictureBoxAccel;
		private System.Windows.Forms.Panel panelAccel;
		private System.Windows.Forms.GroupBox groupBoxPreview;
		private System.Windows.Forms.Button buttonAccelReset;
		private System.Windows.Forms.Button buttonAccelZoomOut;
		private System.Windows.Forms.Button buttonAccelZoomIn;
		private System.Windows.Forms.Label labelAccelYValue;
		private System.Windows.Forms.Label labelAccelXValue;
		private System.Windows.Forms.Label labelAccelXmaxUnit;
		private System.Windows.Forms.Label labelAccelXminUnit;
		private System.Windows.Forms.Label labelAccelYmaxUnit;
		private System.Windows.Forms.Label labelAccelYminUnit;
		private System.Windows.Forms.TextBox textBoxAccelYmax;
		private System.Windows.Forms.TextBox textBoxAccelYmin;
		private System.Windows.Forms.TextBox textBoxAccelXmax;
		private System.Windows.Forms.TextBox textBoxAccelXmin;
		private System.Windows.Forms.Label labelAccelYmax;
		private System.Windows.Forms.Label labelAccelYmin;
		private System.Windows.Forms.Label labelAccelXmax;
		private System.Windows.Forms.Label labelAccelXmin;
		private System.Windows.Forms.Label labelAccelY;
		private System.Windows.Forms.Label labelAccelX;
		private System.Windows.Forms.CheckBox checkBoxSubtractDeceleration;
		private System.Windows.Forms.GroupBox groupBoxParameter;
		private System.Windows.Forms.Label labeAccelA0Unit;
		private System.Windows.Forms.TextBox textBoxAccelA0;
		private System.Windows.Forms.Label labelAccelA0;
		private System.Windows.Forms.Label labelAccelA1Unit;
		private System.Windows.Forms.TextBox textBoxAccelA1;
		private System.Windows.Forms.Label labelAccelA1;
		private System.Windows.Forms.Label labelAccelV1Unit;
		private System.Windows.Forms.TextBox textBoxAccelV1;
		private System.Windows.Forms.Label labelAccelV1;
		private System.Windows.Forms.Label labelAccelV2Unit;
		private System.Windows.Forms.TextBox textBoxAccelV2;
		private System.Windows.Forms.Label labelAccelV2;
		private System.Windows.Forms.TextBox textBoxAccelE;
		private System.Windows.Forms.Label labelAccelE;
		private System.Windows.Forms.GroupBox groupBoxNotch;
		private System.Windows.Forms.ComboBox comboBoxNotch;
		private System.Windows.Forms.TabPage tabPageMotor;
		private System.Windows.Forms.Panel panelMotorSound;
		private System.Windows.Forms.ToolStripContainer toolStripContainerDrawArea;
		private System.Windows.Forms.ToolStrip toolStripToolBar;
		private System.Windows.Forms.ToolStripButton toolStripButtonUndo;
		private System.Windows.Forms.ToolStripButton toolStripButtonRedo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorRedo;
		private System.Windows.Forms.ToolStripButton toolStripButtonTearingOff;
		private System.Windows.Forms.ToolStripButton toolStripButtonCopy;
		private System.Windows.Forms.ToolStripButton toolStripButtonPaste;
		private System.Windows.Forms.ToolStripButton toolStripButtonCleanup;
		private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorEdit;
		private System.Windows.Forms.ToolStripButton toolStripButtonSelect;
		private System.Windows.Forms.ToolStripButton toolStripButtonMove;
		private System.Windows.Forms.ToolStripButton toolStripButtonDot;
		private System.Windows.Forms.ToolStripButton toolStripButtonLine;
		private System.Windows.Forms.Panel panelMotorSetting;
		private System.Windows.Forms.GroupBox groupBoxDirect;
		private System.Windows.Forms.Button buttonDirectDot;
		private System.Windows.Forms.Button buttonDirectMove;
		private System.Windows.Forms.TextBox textBoxDirectY;
		private System.Windows.Forms.Label labelDirectY;
		private System.Windows.Forms.Label labelDirectXUnit;
		private System.Windows.Forms.TextBox textBoxDirectX;
		private System.Windows.Forms.Label labelDirectX;
		private System.Windows.Forms.GroupBox groupBoxPlay;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Button buttonPause;
		private System.Windows.Forms.Button buttonPlay;
		private System.Windows.Forms.GroupBox groupBoxArea;
		private System.Windows.Forms.CheckBox checkBoxMotorConstant;
		private System.Windows.Forms.CheckBox checkBoxMotorLoop;
		private System.Windows.Forms.Button buttonMotorSwap;
		private System.Windows.Forms.TextBox textBoxMotorAreaLeft;
		private System.Windows.Forms.Label labelMotorAreaUnit;
		private System.Windows.Forms.TextBox textBoxMotorAreaRight;
		private System.Windows.Forms.Label labelMotorAccel;
		private System.Windows.Forms.Label labelMotorAccelUnit;
		private System.Windows.Forms.TextBox textBoxMotorAccel;
		private System.Windows.Forms.GroupBox groupBoxSource;
		private System.Windows.Forms.Label labelRun;
		private System.Windows.Forms.CheckBox checkBoxTrack2;
		private System.Windows.Forms.CheckBox checkBoxTrack1;
		private System.Windows.Forms.GroupBox groupBoxView;
		private System.Windows.Forms.Button buttonMotorReset;
		private System.Windows.Forms.Button buttonMotorZoomOut;
		private System.Windows.Forms.Button buttonMotorZoomIn;
		private System.Windows.Forms.Label labelMotorMaxVolume;
		private System.Windows.Forms.TextBox textBoxMotorMaxVolume;
		private System.Windows.Forms.Label labelMotorMinVolume;
		private System.Windows.Forms.TextBox textBoxMotorMinVolume;
		private System.Windows.Forms.Label labelMotorMaxPitch;
		private System.Windows.Forms.TextBox textBoxMotorMaxPitch;
		private System.Windows.Forms.Label labelMotorMinPitch;
		private System.Windows.Forms.TextBox textBoxMotorMinPitch;
		private System.Windows.Forms.Label labelMotorMaxVelocityUnit;
		private System.Windows.Forms.TextBox textBoxMotorMaxVelocity;
		private System.Windows.Forms.Label labelMotorMaxVelocity;
		private System.Windows.Forms.Label labelMotorMinVelocityUnit;
		private System.Windows.Forms.TextBox textBoxMotorMinVelocity;
		private System.Windows.Forms.Label labelMotorMinVelocity;
		private System.Windows.Forms.StatusStrip statusStripStatus;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelY;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelX;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTool;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelMode;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTrack;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelType;
		private System.Windows.Forms.TabPage tabPageCoupler;
		private System.Windows.Forms.GroupBox groupBoxCouplerGeneral;
		private System.Windows.Forms.Label labelCouplerMaxUnit;
		private System.Windows.Forms.TextBox textBoxCouplerMax;
		private System.Windows.Forms.Label labelCouplerMax;
		private System.Windows.Forms.Label labelCouplerMinUnit;
		private System.Windows.Forms.TextBox textBoxCouplerMin;
		private System.Windows.Forms.Label labelCouplerMin;
		private System.Windows.Forms.TabPage tabPagePanel;
		private System.Windows.Forms.SplitContainer splitContainerPanel;
		private System.Windows.Forms.TreeView treeViewPanel;
		private System.Windows.Forms.ListView listViewPanel;
		private System.Windows.Forms.Panel panelPanelNavi;
		private System.Windows.Forms.Button buttonPanelAdd;
		private System.Windows.Forms.Button buttonPanelRemove;
		private System.Windows.Forms.TabControl tabControlPanel;
		private System.Windows.Forms.TabPage tabPageThis;
		private System.Windows.Forms.Button buttonThisNighttimeImageOpen;
		private System.Windows.Forms.Button buttonThisDaytimeImageOpen;
		private System.Windows.Forms.GroupBox groupBoxThisOrigin;
		private System.Windows.Forms.TextBox textBoxThisOriginY;
		private System.Windows.Forms.TextBox textBoxThisOriginX;
		private System.Windows.Forms.Label labelThisOriginY;
		private System.Windows.Forms.Label labelThisOriginX;
		private System.Windows.Forms.GroupBox groupBoxThisCenter;
		private System.Windows.Forms.TextBox textBoxThisCenterY;
		private System.Windows.Forms.TextBox textBoxThisCenterX;
		private System.Windows.Forms.Label labelThisCenterY;
		private System.Windows.Forms.Label labelThisCenterX;
		private System.Windows.Forms.TextBox textBoxThisTransparentColor;
		private System.Windows.Forms.TextBox textBoxThisNighttimeImage;
		private System.Windows.Forms.TextBox textBoxThisDaytimeImage;
		private System.Windows.Forms.TextBox textBoxThisBottom;
		private System.Windows.Forms.TextBox textBoxThisTop;
		private System.Windows.Forms.TextBox textBoxThisRight;
		private System.Windows.Forms.TextBox textBoxThisLeft;
		private System.Windows.Forms.TextBox textBoxThisResolution;
		private System.Windows.Forms.Label labelThisResolution;
		private System.Windows.Forms.Label labelThisLeft;
		private System.Windows.Forms.Label labelThisRight;
		private System.Windows.Forms.Label labelThisTop;
		private System.Windows.Forms.Label labelThisBottom;
		private System.Windows.Forms.Label labelThisDaytimeImage;
		private System.Windows.Forms.Label labelThisNighttimeImage;
		private System.Windows.Forms.Label labelThisTransparentColor;
		private System.Windows.Forms.TabPage tabPageScreen;
		private System.Windows.Forms.NumericUpDown numericUpDownScreenLayer;
		private System.Windows.Forms.NumericUpDown numericUpDownScreenNumber;
		private System.Windows.Forms.Label labelScreenLayer;
		private System.Windows.Forms.Label labelScreenNumber;
		private System.Windows.Forms.TabPage tabPagePilotLamp;
		private System.Windows.Forms.NumericUpDown numericUpDownPilotLampLayer;
		private System.Windows.Forms.GroupBox groupBoxPilotLampLocation;
		private System.Windows.Forms.TextBox textBoxPilotLampLocationY;
		private System.Windows.Forms.TextBox textBoxPilotLampLocationX;
		private System.Windows.Forms.Label labelPilotLampLocationY;
		private System.Windows.Forms.Label labelPilotLampLocationX;
		private System.Windows.Forms.Button buttonPilotLampSubjectSet;
		private System.Windows.Forms.Label labelPilotLampSubject;
		private System.Windows.Forms.Label labelPilotLampLayer;
		private System.Windows.Forms.Button buttonPilotLampNighttimeImageOpen;
		private System.Windows.Forms.Button buttonPilotLampDaytimeImageOpen;
		private System.Windows.Forms.TextBox textBoxPilotLampTransparentColor;
		private System.Windows.Forms.TextBox textBoxPilotLampNighttimeImage;
		private System.Windows.Forms.TextBox textBoxPilotLampDaytimeImage;
		private System.Windows.Forms.Label labelPilotLampDaytimeImage;
		private System.Windows.Forms.Label labelPilotLampNighttimeImage;
		private System.Windows.Forms.Label labelPilotLampTransparentColor;
		private System.Windows.Forms.TabPage tabPageNeedle;
		private System.Windows.Forms.CheckBox checkBoxNeedleDefinedOrigin;
		private System.Windows.Forms.Label labelNeedleDefinedOrigin;
		private System.Windows.Forms.CheckBox checkBoxNeedleDefinedRadius;
		private System.Windows.Forms.Label labelNeedleDefinedRadius;
		private System.Windows.Forms.NumericUpDown numericUpDownNeedleLayer;
		private System.Windows.Forms.CheckBox checkBoxNeedleSmoothed;
		private System.Windows.Forms.Label labelNeedleSmoothed;
		private System.Windows.Forms.CheckBox checkBoxNeedleBackstop;
		private System.Windows.Forms.Label labelNeedleBackstop;
		private System.Windows.Forms.TextBox textBoxNeedleDampingRatio;
		private System.Windows.Forms.Label labelNeedleDampingRatio;
		private System.Windows.Forms.TextBox textBoxNeedleNaturalFreq;
		private System.Windows.Forms.Label labelNeedleNaturalFreq;
		private System.Windows.Forms.TextBox textBoxNeedleMaximum;
		private System.Windows.Forms.Label labelNeedleMaximum;
		private System.Windows.Forms.TextBox textBoxNeedleMinimum;
		private System.Windows.Forms.Label labelNeedleMinimum;
		private System.Windows.Forms.TextBox textBoxNeedleLastAngle;
		private System.Windows.Forms.Label labelNeedleLastAngle;
		private System.Windows.Forms.TextBox textBoxNeedleInitialAngle;
		private System.Windows.Forms.Label labelNeedleInitialAngle;
		private System.Windows.Forms.GroupBox groupBoxNeedleOrigin;
		private System.Windows.Forms.TextBox textBoxNeedleOriginY;
		private System.Windows.Forms.TextBox textBoxNeedleOriginX;
		private System.Windows.Forms.Label labelNeedleOriginY;
		private System.Windows.Forms.Label labelNeedleOriginX;
		private System.Windows.Forms.TextBox textBoxNeedleColor;
		private System.Windows.Forms.Label labelNeedleColor;
		private System.Windows.Forms.TextBox textBoxNeedleRadius;
		private System.Windows.Forms.Label labelNeedleRadius;
		private System.Windows.Forms.GroupBox groupBoxNeedleLocation;
		private System.Windows.Forms.TextBox textBoxNeedleLocationY;
		private System.Windows.Forms.TextBox textBoxNeedleLocationX;
		private System.Windows.Forms.Label labelNeedleLocationY;
		private System.Windows.Forms.Label labelNeedleLocationX;
		private System.Windows.Forms.Button buttonNeedleSubjectSet;
		private System.Windows.Forms.Label labelNeedleSubject;
		private System.Windows.Forms.Label labelNeedleLayer;
		private System.Windows.Forms.Button buttonNeedleNighttimeImageOpen;
		private System.Windows.Forms.Button buttonNeedleDaytimeImageOpen;
		private System.Windows.Forms.TextBox textBoxNeedleTransparentColor;
		private System.Windows.Forms.TextBox textBoxNeedleNighttimeImage;
		private System.Windows.Forms.TextBox textBoxNeedleDaytimeImage;
		private System.Windows.Forms.Label labelNeedleDaytimeImage;
		private System.Windows.Forms.Label labelNeedleNighttimeImage;
		private System.Windows.Forms.Label labelNeedleTransparentColor;
		private System.Windows.Forms.TabPage tabPageDigitalNumber;
		private System.Windows.Forms.NumericUpDown numericUpDownDigitalNumberLayer;
		private System.Windows.Forms.NumericUpDown numericUpDownDigitalNumberInterval;
		private System.Windows.Forms.Label labelDigitalNumberInterval;
		private System.Windows.Forms.GroupBox groupBoxDigitalNumberLocation;
		private System.Windows.Forms.TextBox textBoxDigitalNumberLocationY;
		private System.Windows.Forms.TextBox textBoxDigitalNumberLocationX;
		private System.Windows.Forms.Label labelDigitalNumberLocationY;
		private System.Windows.Forms.Label labelDigitalNumberLocationX;
		private System.Windows.Forms.Button buttonDigitalNumberSubjectSet;
		private System.Windows.Forms.Label labelDigitalNumberSubject;
		private System.Windows.Forms.Label labelDigitalNumberLayer;
		private System.Windows.Forms.Button buttonDigitalNumberNighttimeImageOpen;
		private System.Windows.Forms.Button buttonDigitalNumberDaytimeImageOpen;
		private System.Windows.Forms.TextBox textBoxDigitalNumberTransparentColor;
		private System.Windows.Forms.TextBox textBoxDigitalNumberNighttimeImage;
		private System.Windows.Forms.TextBox textBoxDigitalNumberDaytimeImage;
		private System.Windows.Forms.Label labelDigitalNumberDaytimeImage;
		private System.Windows.Forms.Label labelDigitalNumberNighttimeImage;
		private System.Windows.Forms.Label labelDigitalNumberTransparentColor;
		private System.Windows.Forms.TabPage tabPageDigitalGauge;
		private System.Windows.Forms.NumericUpDown numericUpDownDigitalGaugeLayer;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeStep;
		private System.Windows.Forms.Label labelDigitalGaugeStep;
		private System.Windows.Forms.Label labelDigitalGaugeLayer;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeMaximum;
		private System.Windows.Forms.Label labelDigitalGaugeMaximum;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeMinimum;
		private System.Windows.Forms.Label labelDigitalGaugeMinimum;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeLastAngle;
		private System.Windows.Forms.Label labelDigitalGaugeLastAngle;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeInitialAngle;
		private System.Windows.Forms.Label labelDigitalGaugeInitialAngle;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeColor;
		private System.Windows.Forms.Label labelDigitalGaugeColor;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeRadius;
		private System.Windows.Forms.Label labelDigitalGaugeRadius;
		private System.Windows.Forms.GroupBox groupBoxDigitalGaugeLocation;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeLocationY;
		private System.Windows.Forms.TextBox textBoxDigitalGaugeLocationX;
		private System.Windows.Forms.Label labelDigitalGaugeLocationY;
		private System.Windows.Forms.Label labelDigitalGaugeLocationX;
		private System.Windows.Forms.Button buttonDigitalGaugeSubjectSet;
		private System.Windows.Forms.Label labelDigitalGaugeSubject;
		private System.Windows.Forms.TabPage tabPageLinearGauge;
		private System.Windows.Forms.Button buttonLinearGaugeNighttimeImageOpen;
		private System.Windows.Forms.Button buttonLinearGaugeDaytimeImageOpen;
		private System.Windows.Forms.TextBox textBoxLinearGaugeTransparentColor;
		private System.Windows.Forms.TextBox textBoxLinearGaugeNighttimeImage;
		private System.Windows.Forms.TextBox textBoxLinearGaugeDaytimeImage;
		private System.Windows.Forms.Label labelLinearGaugeDaytimeImage;
		private System.Windows.Forms.Label labelLinearGaugeNighttimeImage;
		private System.Windows.Forms.Label labelLinearGaugeTransparentColor;
		private System.Windows.Forms.NumericUpDown numericUpDownLinearGaugeLayer;
		private System.Windows.Forms.NumericUpDown numericUpDownLinearGaugeWidth;
		private System.Windows.Forms.Label labelLinearGaugeWidth;
		private System.Windows.Forms.GroupBox groupBoxLinearGaugeDirection;
		private System.Windows.Forms.Label labelLinearGaugeDirectionY;
		private System.Windows.Forms.Label labelLinearGaugeDirectionX;
		private System.Windows.Forms.Label labelLinearGaugeLayer;
		private System.Windows.Forms.TextBox textBoxLinearGaugeMaximum;
		private System.Windows.Forms.Label labelLinearGaugeMaximum;
		private System.Windows.Forms.TextBox textBoxLinearGaugeMinimum;
		private System.Windows.Forms.Label labelLinearGaugeMinimum;
		private System.Windows.Forms.GroupBox groupBoxLinearGaugeLocation;
		private System.Windows.Forms.TextBox textBoxLinearGaugeLocationY;
		private System.Windows.Forms.TextBox textBoxLinearGaugeLocationX;
		private System.Windows.Forms.Label labelLinearGaugeLocationY;
		private System.Windows.Forms.Label labelLinearGaugeLocationX;
		private System.Windows.Forms.Button buttonLinearGaugeSubjectSet;
		private System.Windows.Forms.Label labelLinearGaugeSubject;
		private System.Windows.Forms.TabPage tabPageTimetable;
		private System.Windows.Forms.NumericUpDown numericUpDownTimetableLayer;
		private System.Windows.Forms.Label labelTimetableLayer;
		private System.Windows.Forms.TextBox textBoxTimetableTransparentColor;
		private System.Windows.Forms.Label labelTimetableTransparentColor;
		private System.Windows.Forms.TextBox textBoxTimetableHeight;
		private System.Windows.Forms.Label labelTimetableHeight;
		private System.Windows.Forms.TextBox textBoxTimetableWidth;
		private System.Windows.Forms.Label labelTimetableWidth;
		private System.Windows.Forms.GroupBox groupBoxTimetableLocation;
		private System.Windows.Forms.TextBox textBoxTimetableLocationY;
		private System.Windows.Forms.TextBox textBoxTimetableLocationX;
		private System.Windows.Forms.Label labelTimetableLocationY;
		private System.Windows.Forms.Label labelTimetableLocationX;
		private System.Windows.Forms.TabPage tabPageTouch;
		private System.Windows.Forms.ComboBox comboBoxTouchCommand;
		private System.Windows.Forms.Label labelTouchCommand;
		private System.Windows.Forms.NumericUpDown numericUpDownTouchCommandOption;
		private System.Windows.Forms.Label labelTouchCommandOption;
		private System.Windows.Forms.NumericUpDown numericUpDownTouchSoundIndex;
		private System.Windows.Forms.Label labelTouchSoundIndex;
		private System.Windows.Forms.NumericUpDown numericUpDownTouchJumpScreen;
		private System.Windows.Forms.Label labelTouchJumpScreen;
		private System.Windows.Forms.GroupBox groupBoxTouchSize;
		private System.Windows.Forms.TextBox textBoxTouchSizeY;
		private System.Windows.Forms.TextBox textBoxTouchSizeX;
		private System.Windows.Forms.Label labelTouchSizeY;
		private System.Windows.Forms.Label labelTouchSizeX;
		private System.Windows.Forms.GroupBox groupBoxTouchLocation;
		private System.Windows.Forms.TextBox textBoxTouchLocationY;
		private System.Windows.Forms.TextBox textBoxTouchLocationX;
		private System.Windows.Forms.Label labelTouchLocationY;
		private System.Windows.Forms.Label labelTouchLocationX;
		private System.Windows.Forms.TabPage tabPageSound;
		private System.Windows.Forms.Panel panelSoundSetting;
		private System.Windows.Forms.ListView listViewSound;
		private System.Windows.Forms.Panel panelSoundSettingEdit;
		private System.Windows.Forms.GroupBox groupBoxSoundEntry;
		private System.Windows.Forms.Button buttonSoundRemove;
		private System.Windows.Forms.GroupBox groupBoxSoundKey;
		private System.Windows.Forms.NumericUpDown numericUpDownSoundKeyIndex;
		private System.Windows.Forms.ComboBox comboBoxSoundKey;
		private System.Windows.Forms.Button buttonSoundAdd;
		private System.Windows.Forms.GroupBox groupBoxSoundValue;
		private System.Windows.Forms.CheckBox checkBoxSoundRadius;
		private System.Windows.Forms.CheckBox checkBoxSoundDefinedPosition;
		private System.Windows.Forms.TextBox textBoxSoundRadius;
		private System.Windows.Forms.GroupBox groupBoxSoundPosition;
		private System.Windows.Forms.Label labelSoundPositionZUnit;
		private System.Windows.Forms.TextBox textBoxSoundPositionZ;
		private System.Windows.Forms.Label labelSoundPositionZ;
		private System.Windows.Forms.Label labelSoundPositionYUnit;
		private System.Windows.Forms.TextBox textBoxSoundPositionY;
		private System.Windows.Forms.Label labelSoundPositionY;
		private System.Windows.Forms.Label labelSoundPositionXUnit;
		private System.Windows.Forms.TextBox textBoxSoundPositionX;
		private System.Windows.Forms.Label labelSoundPositionX;
		private System.Windows.Forms.Label labelSoundFileName;
		private System.Windows.Forms.Button buttonSoundFileNameOpen;
		private System.Windows.Forms.TextBox textBoxSoundFileName;
		private System.Windows.Forms.TabPage tabPageStatus;
		private System.Windows.Forms.ListView listViewStatus;
		private System.Windows.Forms.ColumnHeader columnHeaderLevel;
		private System.Windows.Forms.ColumnHeader columnHeaderText;
		private System.Windows.Forms.MenuStrip menuStripStatus;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemError;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWarning;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInfo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClear;
		private System.Windows.Forms.Panel panelCars;
		private System.Windows.Forms.TreeView treeViewCars;
		private System.Windows.Forms.Panel panelCarsNavi;
		private System.Windows.Forms.Button buttonCarsCopy;
		private System.Windows.Forms.Button buttonCarsRemove;
		private System.Windows.Forms.Button buttonCarsAdd;
		private System.Windows.Forms.Button buttonCarsUp;
		private System.Windows.Forms.Button buttonCarsDown;
		private System.Windows.Forms.MenuStrip menuStripMenu;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNew;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorOpen;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSave;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSave;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxLanguage;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLanguage;
		private System.Windows.Forms.ComboBox comboBoxDriverCar;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImport;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExport;
		private System.Windows.Forms.MenuStrip menuStripMotor;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUndo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRedo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorUndo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTearingOff;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCleanup;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPower;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPowerTrack1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPowerTrack2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrake;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrakeTrack1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrakeTrack2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInput;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPitch;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVolume;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemIndex;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxIndex;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTool;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelect;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMove;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDot;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLine;
		private System.Windows.Forms.NumericUpDown numericUpDownRunIndex;
		private System.Windows.Forms.Button buttonPanelCopy;
		private System.Windows.Forms.Button buttonThisTransparentColorSet;
		private System.Windows.Forms.Button buttonPilotLampTransparentColorSet;
		private System.Windows.Forms.Button buttonNeedleTransparentColorSet;
		private System.Windows.Forms.Button buttonNeedleColorSet;
		private System.Windows.Forms.Label labelNeedleDefinedDampingRatio;
		private System.Windows.Forms.Label labelNeedleDefinedNaturalFreq;
		private System.Windows.Forms.CheckBox checkBoxNeedleDefinedDampingRatio;
		private System.Windows.Forms.CheckBox checkBoxNeedleDefinedNaturalFreq;
		private System.Windows.Forms.Button buttonDigitalNumberTransparentColorSet;
		private System.Windows.Forms.Button buttonLinearGaugeTransparentColorSet;
		private System.Windows.Forms.Button buttonDigitalGaugeColorSet;
		private System.Windows.Forms.Button buttonTimetableTransparentColorSet;
		private System.Windows.Forms.Panel panelPanelNaviCmd;
		private System.Windows.Forms.NumericUpDown numericUpDownLinearGaugeDirectionY;
		private System.Windows.Forms.NumericUpDown numericUpDownLinearGaugeDirectionX;
		private System.Windows.Forms.Label labelBrakePipeNormalPressureUnit;
		private System.Windows.Forms.TextBox textBoxBrakePipeNormalPressure;
		private System.Windows.Forms.Label labelBrakePipeNormalPressure;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private SplitContainer splitContainerSound;
		private TreeView treeViewSound;
		private TextBox textBoxCouplerObject;
		private Label labelCouplerObject;
		private Button buttonCouplerObject;
		private OpenTK.GLControl glControlMotor;
	}
}
