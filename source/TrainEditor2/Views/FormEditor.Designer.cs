﻿using System.Windows.Forms;

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
			this.toolStripMenuItemImportTrain = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemImportPanel = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemImportSound = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportTrain = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportPanel = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportSound = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorExport = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripComboBoxLanguage = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripStatusLabelLanguage = new System.Windows.Forms.ToolStripStatusLabel();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.tabPageStatus = new System.Windows.Forms.TabPage();
			this.listViewStatus = new System.Windows.Forms.ListView();
			this.columnHeaderLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panelStatusNavi = new System.Windows.Forms.Panel();
			this.buttonOutputLogs = new System.Windows.Forms.Button();
			this.menuStripStatus = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemError = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWarning = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemClear = new System.Windows.Forms.ToolStripMenuItem();
			this.tabPageSound = new System.Windows.Forms.TabPage();
			this.panelSoundSetting = new System.Windows.Forms.Panel();
			this.splitContainerSound = new System.Windows.Forms.SplitContainer();
			this.treeViewSound = new System.Windows.Forms.TreeView();
			this.listViewSound = new System.Windows.Forms.ListView();
			this.panelSoundSettingEdit = new System.Windows.Forms.Panel();
			this.groupBoxSoundEntry = new System.Windows.Forms.GroupBox();
			this.buttonSoundUp = new System.Windows.Forms.Button();
			this.buttonSoundDown = new System.Windows.Forms.Button();
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
			this.tabPagePanel = new System.Windows.Forms.TabPage();
			this.splitContainerPanel = new System.Windows.Forms.SplitContainer();
			this.treeViewPanel = new System.Windows.Forms.TreeView();
			this.listViewPanel = new System.Windows.Forms.ListView();
			this.panelPanelNavi = new System.Windows.Forms.Panel();
			this.panelPanelNaviCmd = new System.Windows.Forms.Panel();
			this.buttonPanelUp = new System.Windows.Forms.Button();
			this.buttonPanelDown = new System.Windows.Forms.Button();
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
			this.numericUpDownTouchLayer = new System.Windows.Forms.NumericUpDown();
			this.labelTouchLayer = new System.Windows.Forms.Label();
			this.buttonTouchSoundCommand = new System.Windows.Forms.Button();
			this.labelTouchSoundCommand = new System.Windows.Forms.Label();
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
			this.tabPageCoupler = new System.Windows.Forms.TabPage();
			this.groupBoxCouplerGeneral = new System.Windows.Forms.GroupBox();
			this.comboBoxCouplerMaxUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCouplerMinUnit = new System.Windows.Forms.ComboBox();
			this.buttonCouplerObject = new System.Windows.Forms.Button();
			this.textBoxCouplerObject = new System.Windows.Forms.TextBox();
			this.labelCouplerObject = new System.Windows.Forms.Label();
			this.textBoxCouplerMax = new System.Windows.Forms.TextBox();
			this.labelCouplerMax = new System.Windows.Forms.Label();
			this.textBoxCouplerMin = new System.Windows.Forms.TextBox();
			this.labelCouplerMin = new System.Windows.Forms.Label();
			this.tabPageMotor = new System.Windows.Forms.TabPage();
			this.splitContainerMotor = new System.Windows.Forms.SplitContainer();
			this.treeViewMotor = new System.Windows.Forms.TreeView();
			this.panelMoterNavi = new System.Windows.Forms.Panel();
			this.buttonMotorCopy = new System.Windows.Forms.Button();
			this.buttonMotorRemove = new System.Windows.Forms.Button();
			this.buttonMotorAdd = new System.Windows.Forms.Button();
			this.buttonMotorUp = new System.Windows.Forms.Button();
			this.buttonMotorDown = new System.Windows.Forms.Button();
			this.toolStripContainerDrawArea = new System.Windows.Forms.ToolStripContainer();
			this.glControlMotor = new OpenTK.GLControl();
			this.toolStripToolBar = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorRedo = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonCleanup = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorEdit = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonSelect = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDot = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLine = new System.Windows.Forms.ToolStripButton();
			this.panelMotorSetting = new System.Windows.Forms.Panel();
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
			this.textBoxMotorAccel = new System.Windows.Forms.TextBox();
			this.groupBoxSource = new System.Windows.Forms.GroupBox();
			this.numericUpDownRunIndex = new System.Windows.Forms.NumericUpDown();
			this.labelRun = new System.Windows.Forms.Label();
			this.groupBoxDirect = new System.Windows.Forms.GroupBox();
			this.buttonDirectDot = new System.Windows.Forms.Button();
			this.buttonDirectMove = new System.Windows.Forms.Button();
			this.textBoxDirectY = new System.Windows.Forms.TextBox();
			this.labelDirectY = new System.Windows.Forms.Label();
			this.labelDirectXUnit = new System.Windows.Forms.Label();
			this.textBoxDirectX = new System.Windows.Forms.TextBox();
			this.labelDirectX = new System.Windows.Forms.Label();
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
			this.groupBoxTrack = new System.Windows.Forms.GroupBox();
			this.comboBoxTrackType = new System.Windows.Forms.ComboBox();
			this.labelTrackType = new System.Windows.Forms.Label();
			this.menuStripMotor = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorUndo = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemCleanup = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
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
			this.statusStripStatus = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTool = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelMode = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelType = new System.Windows.Forms.ToolStripStatusLabel();
			this.tabPageAccel = new System.Windows.Forms.TabPage();
			this.pictureBoxAccel = new System.Windows.Forms.PictureBox();
			this.panelAccel = new System.Windows.Forms.Panel();
			this.groupBoxPreview = new System.Windows.Forms.GroupBox();
			this.comboBoxAccelYUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxAccelXUnit = new System.Windows.Forms.ComboBox();
			this.labelAccelYUnit = new System.Windows.Forms.Label();
			this.labelAccelXUnit = new System.Windows.Forms.Label();
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
			this.comboBoxAccelV2Unit = new System.Windows.Forms.ComboBox();
			this.comboBoxAccelV1Unit = new System.Windows.Forms.ComboBox();
			this.comboBoxAccelA1Unit = new System.Windows.Forms.ComboBox();
			this.comboBoxAccelA0Unit = new System.Windows.Forms.ComboBox();
			this.textBoxAccelA0 = new System.Windows.Forms.TextBox();
			this.labelAccelA0 = new System.Windows.Forms.Label();
			this.textBoxAccelA1 = new System.Windows.Forms.TextBox();
			this.labelAccelA1 = new System.Windows.Forms.Label();
			this.textBoxAccelV1 = new System.Windows.Forms.TextBox();
			this.labelAccelV1 = new System.Windows.Forms.Label();
			this.textBoxAccelV2 = new System.Windows.Forms.TextBox();
			this.labelAccelV2 = new System.Windows.Forms.Label();
			this.textBoxAccelE = new System.Windows.Forms.TextBox();
			this.labelAccelE = new System.Windows.Forms.Label();
			this.groupBoxNotch = new System.Windows.Forms.GroupBox();
			this.comboBoxNotch = new System.Windows.Forms.ComboBox();
			this.tabPageCar1 = new System.Windows.Forms.TabPage();
			this.groupBoxCarGeneral = new System.Windows.Forms.GroupBox();
			this.comboBoxUnexposedFrontalAreaUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxExposedFrontalAreaUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCenterOfMassHeightUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxHeightUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxWidthUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxLengthUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxMassUnit = new System.Windows.Forms.ComboBox();
			this.buttonRightDoorSet = new System.Windows.Forms.Button();
			this.buttonLeftDoorSet = new System.Windows.Forms.Button();
			this.checkBoxIsControlledCar = new System.Windows.Forms.CheckBox();
			this.comboBoxReAdhesionDevice = new System.Windows.Forms.ComboBox();
			this.labelIsControlledCar = new System.Windows.Forms.Label();
			this.labelReAdhesionDevice = new System.Windows.Forms.Label();
			this.labelRightDoor = new System.Windows.Forms.Label();
			this.labelLeftDoor = new System.Windows.Forms.Label();
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
			this.labelLoadingSway = new System.Windows.Forms.Label();
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
			this.comboBoxRearAxleUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxFrontAxleUnit = new System.Windows.Forms.ComboBox();
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
			this.groupBoxCab = new System.Windows.Forms.GroupBox();
			this.comboBoxCabZUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCabYUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCabXUnit = new System.Windows.Forms.ComboBox();
			this.groupBoxExternalCab = new System.Windows.Forms.GroupBox();
			this.buttonCabFileNameOpen = new System.Windows.Forms.Button();
			this.groupBoxCameraRestriction = new System.Windows.Forms.GroupBox();
			this.comboBoxCameraRestrictionDownUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraRestrictionUpUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraRestrictionRightUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraRestrictionLeftUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraRestrictionBackwardsUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraRestrictionForwardsUnit = new System.Windows.Forms.ComboBox();
			this.checkBoxCameraRestrictionDefinedDown = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedDown = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionDown = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionDown = new System.Windows.Forms.Label();
			this.checkBoxCameraRestrictionDefinedUp = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedUp = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionUp = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionUp = new System.Windows.Forms.Label();
			this.checkBoxCameraRestrictionDefinedRight = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedRight = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionRight = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionRight = new System.Windows.Forms.Label();
			this.checkBoxCameraRestrictionDefinedLeft = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedLeft = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionLeft = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionLeft = new System.Windows.Forms.Label();
			this.checkBoxCameraRestrictionDefinedBackwards = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedBackwards = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionBackwards = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionBackwards = new System.Windows.Forms.Label();
			this.checkBoxCameraRestrictionDefinedForwards = new System.Windows.Forms.CheckBox();
			this.labelCameraRestrictionDefinedForwards = new System.Windows.Forms.Label();
			this.textBoxCameraRestrictionForwards = new System.Windows.Forms.TextBox();
			this.labelCameraRestrictionForwards = new System.Windows.Forms.Label();
			this.textBoxCabFileName = new System.Windows.Forms.TextBox();
			this.labelCabFileName = new System.Windows.Forms.Label();
			this.checkBoxIsEmbeddedCab = new System.Windows.Forms.CheckBox();
			this.labelIsEmbeddedCab = new System.Windows.Forms.Label();
			this.textBoxCabZ = new System.Windows.Forms.TextBox();
			this.textBoxCabY = new System.Windows.Forms.TextBox();
			this.textBoxCabX = new System.Windows.Forms.TextBox();
			this.labelCabZ = new System.Windows.Forms.Label();
			this.labelCabY = new System.Windows.Forms.Label();
			this.labelCabX = new System.Windows.Forms.Label();
			this.groupBoxPressure = new System.Windows.Forms.GroupBox();
			this.groupBoxBrakeCylinder = new System.Windows.Forms.GroupBox();
			this.comboBoxBrakeCylinderReleaseRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakeCylinderEmergencyRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit = new System.Windows.Forms.ComboBox();
			this.textBoxBrakeCylinderReleaseRate = new System.Windows.Forms.TextBox();
			this.labelBrakeCylinderServiceMaximumPressure = new System.Windows.Forms.Label();
			this.labelBrakeCylinderEmergencyMaximumPressure = new System.Windows.Forms.Label();
			this.textBoxBrakeCylinderServiceMaximumPressure = new System.Windows.Forms.TextBox();
			this.textBoxBrakeCylinderEmergencyMaximumPressure = new System.Windows.Forms.TextBox();
			this.labelBrakeCylinderEmergencyRate = new System.Windows.Forms.Label();
			this.labelBrakeCylinderReleaseRate = new System.Windows.Forms.Label();
			this.textBoxBrakeCylinderEmergencyRate = new System.Windows.Forms.TextBox();
			this.groupBoxStraightAirPipe = new System.Windows.Forms.GroupBox();
			this.comboBoxStraightAirPipeReleaseRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxStraightAirPipeEmergencyRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxStraightAirPipeServiceRateUnit = new System.Windows.Forms.ComboBox();
			this.labelStraightAirPipeReleaseRate = new System.Windows.Forms.Label();
			this.textBoxStraightAirPipeReleaseRate = new System.Windows.Forms.TextBox();
			this.labelStraightAirPipeEmergencyRate = new System.Windows.Forms.Label();
			this.textBoxStraightAirPipeEmergencyRate = new System.Windows.Forms.TextBox();
			this.labelStraightAirPipeServiceRate = new System.Windows.Forms.Label();
			this.textBoxStraightAirPipeServiceRate = new System.Windows.Forms.TextBox();
			this.groupBoxBrakePipe = new System.Windows.Forms.GroupBox();
			this.comboBoxBrakePipeEmergencyRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakePipeServiceRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakePipeChargeRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakePipeNormalPressureUnit = new System.Windows.Forms.ComboBox();
			this.labelBrakePipeEmergencyRate = new System.Windows.Forms.Label();
			this.textBoxBrakePipeEmergencyRate = new System.Windows.Forms.TextBox();
			this.labelBrakePipeServiceRate = new System.Windows.Forms.Label();
			this.textBoxBrakePipeServiceRate = new System.Windows.Forms.TextBox();
			this.labelBrakePipeChargeRate = new System.Windows.Forms.Label();
			this.textBoxBrakePipeChargeRate = new System.Windows.Forms.TextBox();
			this.labelBrakePipeNormalPressure = new System.Windows.Forms.Label();
			this.textBoxBrakePipeNormalPressure = new System.Windows.Forms.TextBox();
			this.groupBoxEqualizingReservoir = new System.Windows.Forms.GroupBox();
			this.comboBoxEqualizingReservoirEmergencyRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxEqualizingReservoirServiceRateUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxEqualizingReservoirChargeRateUnit = new System.Windows.Forms.ComboBox();
			this.labelEqualizingReservoirEmergencyRate = new System.Windows.Forms.Label();
			this.textBoxEqualizingReservoirEmergencyRate = new System.Windows.Forms.TextBox();
			this.labelEqualizingReservoirServiceRate = new System.Windows.Forms.Label();
			this.textBoxEqualizingReservoirServiceRate = new System.Windows.Forms.TextBox();
			this.labelEqualizingReservoirChargeRate = new System.Windows.Forms.Label();
			this.textBoxEqualizingReservoirChargeRate = new System.Windows.Forms.TextBox();
			this.groupBoxAuxiliaryReservoir = new System.Windows.Forms.GroupBox();
			this.comboBoxAuxiliaryReservoirChargeRateUnit = new System.Windows.Forms.ComboBox();
			this.labelAuxiliaryReservoirChargeRate = new System.Windows.Forms.Label();
			this.textBoxAuxiliaryReservoirChargeRate = new System.Windows.Forms.TextBox();
			this.groupBoxMainReservoir = new System.Windows.Forms.GroupBox();
			this.comboBoxMainReservoirMaximumPressureUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxMainReservoirMinimumPressureUnit = new System.Windows.Forms.ComboBox();
			this.labelMainReservoirMaximumPressure = new System.Windows.Forms.Label();
			this.labelMainReservoirMinimumPressure = new System.Windows.Forms.Label();
			this.textBoxMainReservoirMaximumPressure = new System.Windows.Forms.TextBox();
			this.textBoxMainReservoirMinimumPressure = new System.Windows.Forms.TextBox();
			this.groupBoxCompressor = new System.Windows.Forms.GroupBox();
			this.comboBoxCompressorRateUnit = new System.Windows.Forms.ComboBox();
			this.labelCompressorRate = new System.Windows.Forms.Label();
			this.textBoxCompressorRate = new System.Windows.Forms.TextBox();
			this.groupBoxBrake = new System.Windows.Forms.GroupBox();
			this.comboBoxBrakeControlSpeedUnit = new System.Windows.Forms.ComboBox();
			this.textBoxBrakeControlSpeed = new System.Windows.Forms.TextBox();
			this.comboBoxBrakeControlSystem = new System.Windows.Forms.ComboBox();
			this.comboBoxLocoBrakeType = new System.Windows.Forms.ComboBox();
			this.comboBoxBrakeType = new System.Windows.Forms.ComboBox();
			this.labelLocoBrakeType = new System.Windows.Forms.Label();
			this.labelBrakeType = new System.Windows.Forms.Label();
			this.labelBrakeControlSpeed = new System.Windows.Forms.Label();
			this.labelBrakeControlSystem = new System.Windows.Forms.Label();
			this.groupBoxJerk = new System.Windows.Forms.GroupBox();
			this.buttonJerkBrakeSet = new System.Windows.Forms.Button();
			this.buttonJerkPowerSet = new System.Windows.Forms.Button();
			this.labelJerkPower = new System.Windows.Forms.Label();
			this.labelJerkBrake = new System.Windows.Forms.Label();
			this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
			this.comboBoxDecelerationUnit = new System.Windows.Forms.ComboBox();
			this.textBoxAerodynamicDragCoefficient = new System.Windows.Forms.TextBox();
			this.textBoxCoefficientOfRollingResistance = new System.Windows.Forms.TextBox();
			this.textBoxCoefficientOfStaticFriction = new System.Windows.Forms.TextBox();
			this.textBoxDeceleration = new System.Windows.Forms.TextBox();
			this.labelAerodynamicDragCoefficient = new System.Windows.Forms.Label();
			this.labelCoefficientOfStaticFriction = new System.Windows.Forms.Label();
			this.labelDeceleration = new System.Windows.Forms.Label();
			this.labelCoefficientOfRollingResistance = new System.Windows.Forms.Label();
			this.groupBoxDelay = new System.Windows.Forms.GroupBox();
			this.buttonDelayLocoBrakeSet = new System.Windows.Forms.Button();
			this.buttonDelayBrakeSet = new System.Windows.Forms.Button();
			this.buttonDelayPowerSet = new System.Windows.Forms.Button();
			this.labelDelayLocoBrake = new System.Windows.Forms.Label();
			this.labelDelayBrake = new System.Windows.Forms.Label();
			this.labelDelayPower = new System.Windows.Forms.Label();
			this.tabPageTrain = new System.Windows.Forms.TabPage();
			this.groupBoxTrainGeneral = new System.Windows.Forms.GroupBox();
			this.comboBoxInitialDriverCar = new System.Windows.Forms.ComboBox();
			this.labelInitialDriverCar = new System.Windows.Forms.Label();
			this.groupBoxDevice = new System.Windows.Forms.GroupBox();
			this.comboBoxDoorCloseMode = new System.Windows.Forms.ComboBox();
			this.comboBoxDoorOpenMode = new System.Windows.Forms.ComboBox();
			this.comboBoxPassAlarm = new System.Windows.Forms.ComboBox();
			this.comboBoxAtc = new System.Windows.Forms.ComboBox();
			this.comboBoxAts = new System.Windows.Forms.ComboBox();
			this.checkBoxHoldBrake = new System.Windows.Forms.CheckBox();
			this.checkBoxEb = new System.Windows.Forms.CheckBox();
			this.checkBoxConstSpeed = new System.Windows.Forms.CheckBox();
			this.labelDoorCloseMode = new System.Windows.Forms.Label();
			this.labelDoorOpenMode = new System.Windows.Forms.Label();
			this.labelPassAlarm = new System.Windows.Forms.Label();
			this.labelHoldBrake = new System.Windows.Forms.Label();
			this.labelConstSpeed = new System.Windows.Forms.Label();
			this.labelEb = new System.Windows.Forms.Label();
			this.labelAtc = new System.Windows.Forms.Label();
			this.labelAts = new System.Windows.Forms.Label();
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
			this.tabControlEditor = new System.Windows.Forms.TabControl();
			this.tabPageCar2 = new System.Windows.Forms.TabPage();
			this.groupBoxUnit = new System.Windows.Forms.GroupBox();
			this.comboBoxMotorXUnit = new System.Windows.Forms.ComboBox();
			this.labelMotorXUnit = new System.Windows.Forms.Label();
			this.comboBoxMotorAccelUnit = new System.Windows.Forms.ComboBox();
			this.panelCars.SuspendLayout();
			this.panelCarsNavi.SuspendLayout();
			this.menuStripMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.tabPageStatus.SuspendLayout();
			this.panelStatusNavi.SuspendLayout();
			this.menuStripStatus.SuspendLayout();
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
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchLayer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchJumpScreen)).BeginInit();
			this.groupBoxTouchSize.SuspendLayout();
			this.groupBoxTouchLocation.SuspendLayout();
			this.tabPageCoupler.SuspendLayout();
			this.groupBoxCouplerGeneral.SuspendLayout();
			this.tabPageMotor.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMotor)).BeginInit();
			this.splitContainerMotor.Panel1.SuspendLayout();
			this.splitContainerMotor.Panel2.SuspendLayout();
			this.splitContainerMotor.SuspendLayout();
			this.panelMoterNavi.SuspendLayout();
			this.toolStripContainerDrawArea.ContentPanel.SuspendLayout();
			this.toolStripContainerDrawArea.TopToolStripPanel.SuspendLayout();
			this.toolStripContainerDrawArea.SuspendLayout();
			this.toolStripToolBar.SuspendLayout();
			this.panelMotorSetting.SuspendLayout();
			this.groupBoxPlay.SuspendLayout();
			this.groupBoxArea.SuspendLayout();
			this.groupBoxSource.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunIndex)).BeginInit();
			this.groupBoxDirect.SuspendLayout();
			this.groupBoxView.SuspendLayout();
			this.groupBoxTrack.SuspendLayout();
			this.menuStripMotor.SuspendLayout();
			this.statusStripStatus.SuspendLayout();
			this.tabPageAccel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxAccel)).BeginInit();
			this.panelAccel.SuspendLayout();
			this.groupBoxPreview.SuspendLayout();
			this.groupBoxParameter.SuspendLayout();
			this.groupBoxNotch.SuspendLayout();
			this.tabPageCar1.SuspendLayout();
			this.groupBoxCarGeneral.SuspendLayout();
			this.groupBoxAxles.SuspendLayout();
			this.groupBoxCab.SuspendLayout();
			this.groupBoxExternalCab.SuspendLayout();
			this.groupBoxCameraRestriction.SuspendLayout();
			this.groupBoxPressure.SuspendLayout();
			this.groupBoxBrakeCylinder.SuspendLayout();
			this.groupBoxStraightAirPipe.SuspendLayout();
			this.groupBoxBrakePipe.SuspendLayout();
			this.groupBoxEqualizingReservoir.SuspendLayout();
			this.groupBoxAuxiliaryReservoir.SuspendLayout();
			this.groupBoxMainReservoir.SuspendLayout();
			this.groupBoxCompressor.SuspendLayout();
			this.groupBoxBrake.SuspendLayout();
			this.groupBoxJerk.SuspendLayout();
			this.groupBoxPerformance.SuspendLayout();
			this.groupBoxDelay.SuspendLayout();
			this.tabPageTrain.SuspendLayout();
			this.groupBoxTrainGeneral.SuspendLayout();
			this.groupBoxDevice.SuspendLayout();
			this.groupBoxHandle.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLocoBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverPowerNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotchReduceSteps)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrakeNotches)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotches)).BeginInit();
			this.tabControlEditor.SuspendLayout();
			this.tabPageCar2.SuspendLayout();
			this.groupBoxUnit.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelCars
			// 
			this.panelCars.Controls.Add(this.treeViewCars);
			this.panelCars.Controls.Add(this.panelCarsNavi);
			this.panelCars.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelCars.Location = new System.Drawing.Point(0, 24);
			this.panelCars.Name = "panelCars";
			this.panelCars.Size = new System.Drawing.Size(200, 744);
			this.panelCars.TabIndex = 10;
			// 
			// treeViewCars
			// 
			this.treeViewCars.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewCars.HideSelection = false;
			this.treeViewCars.Location = new System.Drawing.Point(0, 0);
			this.treeViewCars.Name = "treeViewCars";
			this.treeViewCars.Size = new System.Drawing.Size(200, 672);
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
			this.panelCarsNavi.Location = new System.Drawing.Point(0, 672);
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
			this.toolStripMenuItemImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemImportTrain,
            this.toolStripMenuItemImportPanel,
            this.toolStripMenuItemImportSound});
			this.toolStripMenuItemImport.Name = "toolStripMenuItemImport";
			this.toolStripMenuItemImport.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemImport.Text = "Import";
			// 
			// toolStripMenuItemImportTrain
			// 
			this.toolStripMenuItemImportTrain.Name = "toolStripMenuItemImportTrain";
			this.toolStripMenuItemImportTrain.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemImportTrain.Text = "Train...";
			this.toolStripMenuItemImportTrain.Click += new System.EventHandler(this.ToolStripMenuItemImportTrain_Click);
			// 
			// toolStripMenuItemImportPanel
			// 
			this.toolStripMenuItemImportPanel.Name = "toolStripMenuItemImportPanel";
			this.toolStripMenuItemImportPanel.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemImportPanel.Text = "Panel...";
			this.toolStripMenuItemImportPanel.Click += new System.EventHandler(this.ToolStripMenuItemImportPanel_Click);
			// 
			// toolStripMenuItemImportSound
			// 
			this.toolStripMenuItemImportSound.Name = "toolStripMenuItemImportSound";
			this.toolStripMenuItemImportSound.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemImportSound.Text = "Sound...";
			this.toolStripMenuItemImportSound.Click += new System.EventHandler(this.ToolStripMenuItemImportSound_Click);
			// 
			// toolStripMenuItemExport
			// 
			this.toolStripMenuItemExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExportTrain,
            this.toolStripMenuItemExportPanel,
            this.toolStripMenuItemExportSound});
			this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
			this.toolStripMenuItemExport.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemExport.Text = "Export";
			// 
			// toolStripMenuItemExportTrain
			// 
			this.toolStripMenuItemExportTrain.Name = "toolStripMenuItemExportTrain";
			this.toolStripMenuItemExportTrain.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemExportTrain.Text = "Train...";
			this.toolStripMenuItemExportTrain.Click += new System.EventHandler(this.ToolStripMenuItemExportTrain_Click);
			// 
			// toolStripMenuItemExportPanel
			// 
			this.toolStripMenuItemExportPanel.Name = "toolStripMenuItemExportPanel";
			this.toolStripMenuItemExportPanel.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemExportPanel.Text = "Panel...";
			this.toolStripMenuItemExportPanel.Click += new System.EventHandler(this.ToolStripMenuItemExportPanel_Click);
			// 
			// toolStripMenuItemExportSound
			// 
			this.toolStripMenuItemExportSound.Name = "toolStripMenuItemExportSound";
			this.toolStripMenuItemExportSound.Size = new System.Drawing.Size(107, 22);
			this.toolStripMenuItemExportSound.Text = "Sound...";
			this.toolStripMenuItemExportSound.Click += new System.EventHandler(this.ToolStripMenuItemExportSound_Click);
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
			// tabPageStatus
			// 
			this.tabPageStatus.Controls.Add(this.listViewStatus);
			this.tabPageStatus.Controls.Add(this.panelStatusNavi);
			this.tabPageStatus.Controls.Add(this.menuStripStatus);
			this.tabPageStatus.Location = new System.Drawing.Point(4, 22);
			this.tabPageStatus.Name = "tabPageStatus";
			this.tabPageStatus.Size = new System.Drawing.Size(792, 686);
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
			this.listViewStatus.Size = new System.Drawing.Size(792, 622);
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
			// panelStatusNavi
			// 
			this.panelStatusNavi.Controls.Add(this.buttonOutputLogs);
			this.panelStatusNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelStatusNavi.Location = new System.Drawing.Point(0, 646);
			this.panelStatusNavi.Name = "panelStatusNavi";
			this.panelStatusNavi.Size = new System.Drawing.Size(792, 40);
			this.panelStatusNavi.TabIndex = 4;
			// 
			// buttonOutputLogs
			// 
			this.buttonOutputLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOutputLogs.Location = new System.Drawing.Point(704, 8);
			this.buttonOutputLogs.Name = "buttonOutputLogs";
			this.buttonOutputLogs.Size = new System.Drawing.Size(80, 24);
			this.buttonOutputLogs.TabIndex = 0;
			this.buttonOutputLogs.Text = "Output logs...";
			this.buttonOutputLogs.UseVisualStyleBackColor = true;
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
			// tabPageSound
			// 
			this.tabPageSound.Controls.Add(this.panelSoundSetting);
			this.tabPageSound.Location = new System.Drawing.Point(4, 22);
			this.tabPageSound.Name = "tabPageSound";
			this.tabPageSound.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSound.Size = new System.Drawing.Size(792, 686);
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
			this.panelSoundSetting.Size = new System.Drawing.Size(786, 680);
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
			this.splitContainerSound.Size = new System.Drawing.Size(570, 680);
			this.splitContainerSound.SplitterDistance = 190;
			this.splitContainerSound.TabIndex = 2;
			// 
			// treeViewSound
			// 
			this.treeViewSound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewSound.HideSelection = false;
			this.treeViewSound.Location = new System.Drawing.Point(0, 0);
			this.treeViewSound.Name = "treeViewSound";
			this.treeViewSound.Size = new System.Drawing.Size(190, 680);
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
			this.listViewSound.Size = new System.Drawing.Size(376, 680);
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
			this.panelSoundSettingEdit.Size = new System.Drawing.Size(216, 680);
			this.panelSoundSettingEdit.TabIndex = 1;
			// 
			// groupBoxSoundEntry
			// 
			this.groupBoxSoundEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundUp);
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundDown);
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundRemove);
			this.groupBoxSoundEntry.Controls.Add(this.groupBoxSoundKey);
			this.groupBoxSoundEntry.Controls.Add(this.buttonSoundAdd);
			this.groupBoxSoundEntry.Controls.Add(this.groupBoxSoundValue);
			this.groupBoxSoundEntry.Location = new System.Drawing.Point(8, 304);
			this.groupBoxSoundEntry.Name = "groupBoxSoundEntry";
			this.groupBoxSoundEntry.Size = new System.Drawing.Size(200, 368);
			this.groupBoxSoundEntry.TabIndex = 6;
			this.groupBoxSoundEntry.TabStop = false;
			this.groupBoxSoundEntry.Text = "Edit entry";
			// 
			// buttonSoundUp
			// 
			this.buttonSoundUp.Location = new System.Drawing.Point(8, 304);
			this.buttonSoundUp.Name = "buttonSoundUp";
			this.buttonSoundUp.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundUp.TabIndex = 5;
			this.buttonSoundUp.Text = "Up";
			this.buttonSoundUp.UseVisualStyleBackColor = true;
			// 
			// buttonSoundDown
			// 
			this.buttonSoundDown.Location = new System.Drawing.Point(8, 336);
			this.buttonSoundDown.Name = "buttonSoundDown";
			this.buttonSoundDown.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundDown.TabIndex = 6;
			this.buttonSoundDown.Text = "Down";
			this.buttonSoundDown.UseVisualStyleBackColor = true;
			// 
			// buttonSoundRemove
			// 
			this.buttonSoundRemove.Location = new System.Drawing.Point(136, 304);
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
			this.numericUpDownSoundKeyIndex.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.buttonSoundAdd.Location = new System.Drawing.Point(72, 304);
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
			// tabPagePanel
			// 
			this.tabPagePanel.Controls.Add(this.splitContainerPanel);
			this.tabPagePanel.Controls.Add(this.tabControlPanel);
			this.tabPagePanel.Location = new System.Drawing.Point(4, 22);
			this.tabPagePanel.Name = "tabPagePanel";
			this.tabPagePanel.Size = new System.Drawing.Size(792, 686);
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
			this.splitContainerPanel.Size = new System.Drawing.Size(472, 686);
			this.splitContainerPanel.SplitterDistance = 157;
			this.splitContainerPanel.TabIndex = 1;
			// 
			// treeViewPanel
			// 
			this.treeViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPanel.HideSelection = false;
			this.treeViewPanel.Location = new System.Drawing.Point(0, 0);
			this.treeViewPanel.Name = "treeViewPanel";
			this.treeViewPanel.Size = new System.Drawing.Size(157, 686);
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
			this.listViewPanel.Size = new System.Drawing.Size(311, 614);
			this.listViewPanel.TabIndex = 1;
			this.listViewPanel.UseCompatibleStateImageBehavior = false;
			this.listViewPanel.View = System.Windows.Forms.View.Details;
			// 
			// panelPanelNavi
			// 
			this.panelPanelNavi.Controls.Add(this.panelPanelNaviCmd);
			this.panelPanelNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelPanelNavi.Location = new System.Drawing.Point(0, 614);
			this.panelPanelNavi.Name = "panelPanelNavi";
			this.panelPanelNavi.Size = new System.Drawing.Size(311, 72);
			this.panelPanelNavi.TabIndex = 0;
			// 
			// panelPanelNaviCmd
			// 
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelUp);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelDown);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelCopy);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelAdd);
			this.panelPanelNaviCmd.Controls.Add(this.buttonPanelRemove);
			this.panelPanelNaviCmd.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelPanelNaviCmd.Location = new System.Drawing.Point(112, 0);
			this.panelPanelNaviCmd.Name = "panelPanelNaviCmd";
			this.panelPanelNaviCmd.Size = new System.Drawing.Size(199, 72);
			this.panelPanelNaviCmd.TabIndex = 7;
			// 
			// buttonPanelUp
			// 
			this.buttonPanelUp.Location = new System.Drawing.Point(8, 8);
			this.buttonPanelUp.Name = "buttonPanelUp";
			this.buttonPanelUp.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelUp.TabIndex = 7;
			this.buttonPanelUp.Text = "Up";
			this.buttonPanelUp.UseVisualStyleBackColor = true;
			// 
			// buttonPanelDown
			// 
			this.buttonPanelDown.Location = new System.Drawing.Point(8, 40);
			this.buttonPanelDown.Name = "buttonPanelDown";
			this.buttonPanelDown.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelDown.TabIndex = 8;
			this.buttonPanelDown.Text = "Down";
			this.buttonPanelDown.UseVisualStyleBackColor = true;
			// 
			// buttonPanelCopy
			// 
			this.buttonPanelCopy.Location = new System.Drawing.Point(72, 40);
			this.buttonPanelCopy.Name = "buttonPanelCopy";
			this.buttonPanelCopy.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelCopy.TabIndex = 6;
			this.buttonPanelCopy.Text = "Copy";
			this.buttonPanelCopy.UseVisualStyleBackColor = true;
			// 
			// buttonPanelAdd
			// 
			this.buttonPanelAdd.Location = new System.Drawing.Point(72, 8);
			this.buttonPanelAdd.Name = "buttonPanelAdd";
			this.buttonPanelAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonPanelAdd.TabIndex = 4;
			this.buttonPanelAdd.Text = "Add";
			this.buttonPanelAdd.UseVisualStyleBackColor = true;
			// 
			// buttonPanelRemove
			// 
			this.buttonPanelRemove.Location = new System.Drawing.Point(136, 8);
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
			this.tabControlPanel.Size = new System.Drawing.Size(320, 686);
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
			this.tabPageThis.Size = new System.Drawing.Size(312, 642);
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
			this.tabPageScreen.Size = new System.Drawing.Size(312, 642);
			this.tabPageScreen.TabIndex = 1;
			this.tabPageScreen.Text = "Screen";
			this.tabPageScreen.UseVisualStyleBackColor = true;
			// 
			// numericUpDownScreenLayer
			// 
			this.numericUpDownScreenLayer.Location = new System.Drawing.Point(136, 32);
			this.numericUpDownScreenLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numericUpDownScreenLayer.Name = "numericUpDownScreenLayer";
			this.numericUpDownScreenLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownScreenLayer.TabIndex = 31;
			// 
			// numericUpDownScreenNumber
			// 
			this.numericUpDownScreenNumber.Location = new System.Drawing.Point(136, 8);
			this.numericUpDownScreenNumber.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPagePilotLamp.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownPilotLampLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageNeedle.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownNeedleLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageDigitalNumber.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownDigitalNumberLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numericUpDownDigitalNumberLayer.Name = "numericUpDownDigitalNumberLayer";
			this.numericUpDownDigitalNumberLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownDigitalNumberLayer.TabIndex = 85;
			// 
			// numericUpDownDigitalNumberInterval
			// 
			this.numericUpDownDigitalNumberInterval.Location = new System.Drawing.Point(136, 184);
			this.numericUpDownDigitalNumberInterval.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageDigitalGauge.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownDigitalGaugeLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageLinearGauge.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownLinearGaugeLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numericUpDownLinearGaugeLayer.Name = "numericUpDownLinearGaugeLayer";
			this.numericUpDownLinearGaugeLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownLinearGaugeLayer.TabIndex = 96;
			// 
			// numericUpDownLinearGaugeWidth
			// 
			this.numericUpDownLinearGaugeWidth.Location = new System.Drawing.Point(136, 312);
			this.numericUpDownLinearGaugeWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageTimetable.Size = new System.Drawing.Size(312, 642);
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
			this.numericUpDownTimetableLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.tabPageTouch.Controls.Add(this.numericUpDownTouchLayer);
			this.tabPageTouch.Controls.Add(this.labelTouchLayer);
			this.tabPageTouch.Controls.Add(this.buttonTouchSoundCommand);
			this.tabPageTouch.Controls.Add(this.labelTouchSoundCommand);
			this.tabPageTouch.Controls.Add(this.numericUpDownTouchJumpScreen);
			this.tabPageTouch.Controls.Add(this.labelTouchJumpScreen);
			this.tabPageTouch.Controls.Add(this.groupBoxTouchSize);
			this.tabPageTouch.Controls.Add(this.groupBoxTouchLocation);
			this.tabPageTouch.Location = new System.Drawing.Point(4, 40);
			this.tabPageTouch.Name = "tabPageTouch";
			this.tabPageTouch.Size = new System.Drawing.Size(312, 642);
			this.tabPageTouch.TabIndex = 8;
			this.tabPageTouch.Text = "Touch";
			this.tabPageTouch.UseVisualStyleBackColor = true;
			// 
			// numericUpDownTouchLayer
			// 
			this.numericUpDownTouchLayer.Location = new System.Drawing.Point(136, 216);
			this.numericUpDownTouchLayer.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numericUpDownTouchLayer.Name = "numericUpDownTouchLayer";
			this.numericUpDownTouchLayer.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchLayer.TabIndex = 111;
			// 
			// labelTouchLayer
			// 
			this.labelTouchLayer.Location = new System.Drawing.Point(8, 216);
			this.labelTouchLayer.Name = "labelTouchLayer";
			this.labelTouchLayer.Size = new System.Drawing.Size(120, 16);
			this.labelTouchLayer.TabIndex = 110;
			this.labelTouchLayer.Text = "Layer:";
			this.labelTouchLayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonTouchSoundCommand
			// 
			this.buttonTouchSoundCommand.Location = new System.Drawing.Point(136, 192);
			this.buttonTouchSoundCommand.Name = "buttonTouchSoundCommand";
			this.buttonTouchSoundCommand.Size = new System.Drawing.Size(48, 19);
			this.buttonTouchSoundCommand.TabIndex = 109;
			this.buttonTouchSoundCommand.Text = "Set...";
			this.buttonTouchSoundCommand.UseVisualStyleBackColor = true;
			this.buttonTouchSoundCommand.Click += new System.EventHandler(this.ButtonTouchSoundCommand_Click);
			// 
			// labelTouchSoundCommand
			// 
			this.labelTouchSoundCommand.Location = new System.Drawing.Point(8, 192);
			this.labelTouchSoundCommand.Name = "labelTouchSoundCommand";
			this.labelTouchSoundCommand.Size = new System.Drawing.Size(120, 16);
			this.labelTouchSoundCommand.TabIndex = 108;
			this.labelTouchSoundCommand.Text = "Sound and Command:";
			this.labelTouchSoundCommand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownTouchJumpScreen
			// 
			this.numericUpDownTouchJumpScreen.Location = new System.Drawing.Point(136, 168);
			this.numericUpDownTouchJumpScreen.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			// tabPageCoupler
			// 
			this.tabPageCoupler.Controls.Add(this.groupBoxCouplerGeneral);
			this.tabPageCoupler.Location = new System.Drawing.Point(4, 22);
			this.tabPageCoupler.Name = "tabPageCoupler";
			this.tabPageCoupler.Size = new System.Drawing.Size(792, 686);
			this.tabPageCoupler.TabIndex = 7;
			this.tabPageCoupler.Text = "Coupler settings";
			this.tabPageCoupler.UseVisualStyleBackColor = true;
			// 
			// groupBoxCouplerGeneral
			// 
			this.groupBoxCouplerGeneral.Controls.Add(this.comboBoxCouplerMaxUnit);
			this.groupBoxCouplerGeneral.Controls.Add(this.comboBoxCouplerMinUnit);
			this.groupBoxCouplerGeneral.Controls.Add(this.buttonCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerObject);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerMax);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMax);
			this.groupBoxCouplerGeneral.Controls.Add(this.textBoxCouplerMin);
			this.groupBoxCouplerGeneral.Controls.Add(this.labelCouplerMin);
			this.groupBoxCouplerGeneral.Location = new System.Drawing.Point(8, 8);
			this.groupBoxCouplerGeneral.Name = "groupBoxCouplerGeneral";
			this.groupBoxCouplerGeneral.Size = new System.Drawing.Size(196, 115);
			this.groupBoxCouplerGeneral.TabIndex = 0;
			this.groupBoxCouplerGeneral.TabStop = false;
			this.groupBoxCouplerGeneral.Text = "General";
			// 
			// comboBoxCouplerMaxUnit
			// 
			this.comboBoxCouplerMaxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCouplerMaxUnit.FormattingEnabled = true;
			this.comboBoxCouplerMaxUnit.Location = new System.Drawing.Point(120, 40);
			this.comboBoxCouplerMaxUnit.Name = "comboBoxCouplerMaxUnit";
			this.comboBoxCouplerMaxUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCouplerMaxUnit.TabIndex = 57;
			// 
			// comboBoxCouplerMinUnit
			// 
			this.comboBoxCouplerMinUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCouplerMinUnit.FormattingEnabled = true;
			this.comboBoxCouplerMinUnit.Location = new System.Drawing.Point(120, 16);
			this.comboBoxCouplerMinUnit.Name = "comboBoxCouplerMinUnit";
			this.comboBoxCouplerMinUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCouplerMinUnit.TabIndex = 56;
			// 
			// buttonCouplerObject
			// 
			this.buttonCouplerObject.Location = new System.Drawing.Point(128, 88);
			this.buttonCouplerObject.Name = "buttonCouplerObject";
			this.buttonCouplerObject.Size = new System.Drawing.Size(56, 19);
			this.buttonCouplerObject.TabIndex = 30;
			this.buttonCouplerObject.Text = "Open...";
			this.buttonCouplerObject.UseVisualStyleBackColor = true;
			this.buttonCouplerObject.Click += new System.EventHandler(this.ButtonCouplerObject_Click);
			// 
			// textBoxCouplerObject
			// 
			this.textBoxCouplerObject.Location = new System.Drawing.Point(64, 64);
			this.textBoxCouplerObject.Name = "textBoxCouplerObject";
			this.textBoxCouplerObject.Size = new System.Drawing.Size(120, 19);
			this.textBoxCouplerObject.TabIndex = 29;
			// 
			// labelCouplerObject
			// 
			this.labelCouplerObject.Location = new System.Drawing.Point(8, 64);
			this.labelCouplerObject.Name = "labelCouplerObject";
			this.labelCouplerObject.Size = new System.Drawing.Size(48, 16);
			this.labelCouplerObject.TabIndex = 28;
			this.labelCouplerObject.Text = "Object:";
			this.labelCouplerObject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCouplerMax
			// 
			this.textBoxCouplerMax.Location = new System.Drawing.Point(64, 40);
			this.textBoxCouplerMax.Name = "textBoxCouplerMax";
			this.textBoxCouplerMax.Size = new System.Drawing.Size(48, 19);
			this.textBoxCouplerMax.TabIndex = 26;
			// 
			// labelCouplerMax
			// 
			this.labelCouplerMax.Location = new System.Drawing.Point(8, 40);
			this.labelCouplerMax.Name = "labelCouplerMax";
			this.labelCouplerMax.Size = new System.Drawing.Size(48, 16);
			this.labelCouplerMax.TabIndex = 25;
			this.labelCouplerMax.Text = "Max:";
			this.labelCouplerMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCouplerMin
			// 
			this.textBoxCouplerMin.Location = new System.Drawing.Point(64, 16);
			this.textBoxCouplerMin.Name = "textBoxCouplerMin";
			this.textBoxCouplerMin.Size = new System.Drawing.Size(48, 19);
			this.textBoxCouplerMin.TabIndex = 23;
			// 
			// labelCouplerMin
			// 
			this.labelCouplerMin.Location = new System.Drawing.Point(8, 16);
			this.labelCouplerMin.Name = "labelCouplerMin";
			this.labelCouplerMin.Size = new System.Drawing.Size(48, 16);
			this.labelCouplerMin.TabIndex = 22;
			this.labelCouplerMin.Text = "Min:";
			this.labelCouplerMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageMotor
			// 
			this.tabPageMotor.Controls.Add(this.splitContainerMotor);
			this.tabPageMotor.Location = new System.Drawing.Point(4, 22);
			this.tabPageMotor.Name = "tabPageMotor";
			this.tabPageMotor.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageMotor.Size = new System.Drawing.Size(792, 718);
			this.tabPageMotor.TabIndex = 1;
			this.tabPageMotor.Text = "Motor sound settings";
			this.tabPageMotor.UseVisualStyleBackColor = true;
			// 
			// splitContainerMotor
			// 
			this.splitContainerMotor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMotor.Location = new System.Drawing.Point(3, 3);
			this.splitContainerMotor.Name = "splitContainerMotor";
			// 
			// splitContainerMotor.Panel1
			// 
			this.splitContainerMotor.Panel1.Controls.Add(this.treeViewMotor);
			this.splitContainerMotor.Panel1.Controls.Add(this.panelMoterNavi);
			// 
			// splitContainerMotor.Panel2
			// 
			this.splitContainerMotor.Panel2.Controls.Add(this.toolStripContainerDrawArea);
			this.splitContainerMotor.Panel2.Controls.Add(this.panelMotorSetting);
			this.splitContainerMotor.Panel2.Controls.Add(this.menuStripMotor);
			this.splitContainerMotor.Panel2.Controls.Add(this.statusStripStatus);
			this.splitContainerMotor.Size = new System.Drawing.Size(786, 712);
			this.splitContainerMotor.SplitterDistance = 200;
			this.splitContainerMotor.TabIndex = 6;
			// 
			// treeViewMotor
			// 
			this.treeViewMotor.CheckBoxes = true;
			this.treeViewMotor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewMotor.HideSelection = false;
			this.treeViewMotor.Location = new System.Drawing.Point(0, 0);
			this.treeViewMotor.Name = "treeViewMotor";
			this.treeViewMotor.Size = new System.Drawing.Size(200, 640);
			this.treeViewMotor.TabIndex = 5;
			// 
			// panelMoterNavi
			// 
			this.panelMoterNavi.Controls.Add(this.buttonMotorCopy);
			this.panelMoterNavi.Controls.Add(this.buttonMotorRemove);
			this.panelMoterNavi.Controls.Add(this.buttonMotorAdd);
			this.panelMoterNavi.Controls.Add(this.buttonMotorUp);
			this.panelMoterNavi.Controls.Add(this.buttonMotorDown);
			this.panelMoterNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelMoterNavi.Location = new System.Drawing.Point(0, 640);
			this.panelMoterNavi.Name = "panelMoterNavi";
			this.panelMoterNavi.Size = new System.Drawing.Size(200, 72);
			this.panelMoterNavi.TabIndex = 4;
			// 
			// buttonMotorCopy
			// 
			this.buttonMotorCopy.Location = new System.Drawing.Point(72, 40);
			this.buttonMotorCopy.Name = "buttonMotorCopy";
			this.buttonMotorCopy.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorCopy.TabIndex = 5;
			this.buttonMotorCopy.Text = "Copy";
			this.buttonMotorCopy.UseVisualStyleBackColor = true;
			// 
			// buttonMotorRemove
			// 
			this.buttonMotorRemove.Location = new System.Drawing.Point(136, 8);
			this.buttonMotorRemove.Name = "buttonMotorRemove";
			this.buttonMotorRemove.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorRemove.TabIndex = 3;
			this.buttonMotorRemove.Text = "Remove";
			this.buttonMotorRemove.UseVisualStyleBackColor = true;
			// 
			// buttonMotorAdd
			// 
			this.buttonMotorAdd.Location = new System.Drawing.Point(72, 8);
			this.buttonMotorAdd.Name = "buttonMotorAdd";
			this.buttonMotorAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorAdd.TabIndex = 2;
			this.buttonMotorAdd.Text = "Add";
			this.buttonMotorAdd.UseVisualStyleBackColor = true;
			// 
			// buttonMotorUp
			// 
			this.buttonMotorUp.Location = new System.Drawing.Point(8, 8);
			this.buttonMotorUp.Name = "buttonMotorUp";
			this.buttonMotorUp.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorUp.TabIndex = 0;
			this.buttonMotorUp.Text = "Up";
			this.buttonMotorUp.UseVisualStyleBackColor = true;
			// 
			// buttonMotorDown
			// 
			this.buttonMotorDown.Location = new System.Drawing.Point(8, 40);
			this.buttonMotorDown.Name = "buttonMotorDown";
			this.buttonMotorDown.Size = new System.Drawing.Size(56, 24);
			this.buttonMotorDown.TabIndex = 1;
			this.buttonMotorDown.Text = "Down";
			this.buttonMotorDown.UseVisualStyleBackColor = true;
			// 
			// toolStripContainerDrawArea
			// 
			// 
			// toolStripContainerDrawArea.ContentPanel
			// 
			this.toolStripContainerDrawArea.ContentPanel.Controls.Add(this.glControlMotor);
			this.toolStripContainerDrawArea.ContentPanel.Size = new System.Drawing.Size(364, 641);
			this.toolStripContainerDrawArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainerDrawArea.Location = new System.Drawing.Point(0, 24);
			this.toolStripContainerDrawArea.Name = "toolStripContainerDrawArea";
			this.toolStripContainerDrawArea.Size = new System.Drawing.Size(364, 666);
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
			this.glControlMotor.Size = new System.Drawing.Size(364, 641);
			this.glControlMotor.TabIndex = 0;
			this.glControlMotor.VSync = false;
			this.glControlMotor.Load += new System.EventHandler(this.GlControlMotor_Load);
			this.glControlMotor.Paint += new System.Windows.Forms.PaintEventHandler(this.GlControlMotor_Paint);
			this.glControlMotor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlControlMotor_KeyDown);
			this.glControlMotor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseDown);
			this.glControlMotor.MouseEnter += new System.EventHandler(this.GlControlMotor_MouseEnter);
			this.glControlMotor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseMove);
			this.glControlMotor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GlControlMotor_MouseUp);
			this.glControlMotor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.GlControlMotor_PreviewKeyDown);
			this.glControlMotor.Resize += new System.EventHandler(this.GlControlMotor_Resize);
			// 
			// toolStripToolBar
			// 
			this.toolStripToolBar.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonUndo,
            this.toolStripButtonRedo,
            this.toolStripSeparatorRedo,
            this.toolStripButtonCleanup,
            this.toolStripButtonDelete,
            this.toolStripSeparatorEdit,
            this.toolStripButtonSelect,
            this.toolStripButtonMove,
            this.toolStripButtonDot,
            this.toolStripButtonLine});
			this.toolStripToolBar.Location = new System.Drawing.Point(3, 0);
			this.toolStripToolBar.Name = "toolStripToolBar";
			this.toolStripToolBar.Size = new System.Drawing.Size(206, 25);
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
			this.panelMotorSetting.Controls.Add(this.groupBoxUnit);
			this.panelMotorSetting.Controls.Add(this.groupBoxPlay);
			this.panelMotorSetting.Controls.Add(this.groupBoxDirect);
			this.panelMotorSetting.Controls.Add(this.groupBoxView);
			this.panelMotorSetting.Controls.Add(this.groupBoxTrack);
			this.panelMotorSetting.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelMotorSetting.Location = new System.Drawing.Point(364, 24);
			this.panelMotorSetting.Name = "panelMotorSetting";
			this.panelMotorSetting.Size = new System.Drawing.Size(218, 666);
			this.panelMotorSetting.TabIndex = 3;
			// 
			// groupBoxPlay
			// 
			this.groupBoxPlay.Controls.Add(this.buttonStop);
			this.groupBoxPlay.Controls.Add(this.buttonPause);
			this.groupBoxPlay.Controls.Add(this.buttonPlay);
			this.groupBoxPlay.Controls.Add(this.groupBoxArea);
			this.groupBoxPlay.Controls.Add(this.groupBoxSource);
			this.groupBoxPlay.Location = new System.Drawing.Point(8, 424);
			this.groupBoxPlay.Name = "groupBoxPlay";
			this.groupBoxPlay.Size = new System.Drawing.Size(200, 233);
			this.groupBoxPlay.TabIndex = 1;
			this.groupBoxPlay.TabStop = false;
			this.groupBoxPlay.Text = "Playback setting";
			// 
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(136, 200);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(56, 24);
			this.buttonStop.TabIndex = 4;
			this.buttonStop.UseVisualStyleBackColor = true;
			// 
			// buttonPause
			// 
			this.buttonPause.Location = new System.Drawing.Point(72, 200);
			this.buttonPause.Name = "buttonPause";
			this.buttonPause.Size = new System.Drawing.Size(56, 24);
			this.buttonPause.TabIndex = 3;
			this.buttonPause.UseVisualStyleBackColor = true;
			// 
			// buttonPlay
			// 
			this.buttonPlay.Location = new System.Drawing.Point(8, 200);
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(56, 24);
			this.buttonPlay.TabIndex = 2;
			this.buttonPlay.UseVisualStyleBackColor = true;
			// 
			// groupBoxArea
			// 
			this.groupBoxArea.Controls.Add(this.comboBoxMotorAccelUnit);
			this.groupBoxArea.Controls.Add(this.checkBoxMotorConstant);
			this.groupBoxArea.Controls.Add(this.checkBoxMotorLoop);
			this.groupBoxArea.Controls.Add(this.buttonMotorSwap);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAreaLeft);
			this.groupBoxArea.Controls.Add(this.labelMotorAreaUnit);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAreaRight);
			this.groupBoxArea.Controls.Add(this.labelMotorAccel);
			this.groupBoxArea.Controls.Add(this.textBoxMotorAccel);
			this.groupBoxArea.Location = new System.Drawing.Point(8, 72);
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
			this.groupBoxSource.Location = new System.Drawing.Point(8, 16);
			this.groupBoxSource.Name = "groupBoxSource";
			this.groupBoxSource.Size = new System.Drawing.Size(184, 48);
			this.groupBoxSource.TabIndex = 0;
			this.groupBoxSource.TabStop = false;
			this.groupBoxSource.Text = "Sound source setting";
			// 
			// numericUpDownRunIndex
			// 
			this.numericUpDownRunIndex.Location = new System.Drawing.Point(112, 16);
			this.numericUpDownRunIndex.Maximum = new decimal(new int[] {
            1024,
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
			// groupBoxDirect
			// 
			this.groupBoxDirect.Controls.Add(this.buttonDirectDot);
			this.groupBoxDirect.Controls.Add(this.buttonDirectMove);
			this.groupBoxDirect.Controls.Add(this.textBoxDirectY);
			this.groupBoxDirect.Controls.Add(this.labelDirectY);
			this.groupBoxDirect.Controls.Add(this.labelDirectXUnit);
			this.groupBoxDirect.Controls.Add(this.textBoxDirectX);
			this.groupBoxDirect.Controls.Add(this.labelDirectX);
			this.groupBoxDirect.Location = new System.Drawing.Point(8, 320);
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
			// groupBoxView
			// 
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
			this.groupBoxView.Location = new System.Drawing.Point(8, 120);
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
			// groupBoxTrack
			// 
			this.groupBoxTrack.Controls.Add(this.comboBoxTrackType);
			this.groupBoxTrack.Controls.Add(this.labelTrackType);
			this.groupBoxTrack.Location = new System.Drawing.Point(8, 8);
			this.groupBoxTrack.Name = "groupBoxTrack";
			this.groupBoxTrack.Size = new System.Drawing.Size(200, 48);
			this.groupBoxTrack.TabIndex = 3;
			this.groupBoxTrack.TabStop = false;
			this.groupBoxTrack.Text = "Track setting";
			// 
			// comboBoxTrackType
			// 
			this.comboBoxTrackType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTrackType.FormattingEnabled = true;
			this.comboBoxTrackType.Items.AddRange(new object[] {
            "Power",
            "Brake"});
			this.comboBoxTrackType.Location = new System.Drawing.Point(112, 16);
			this.comboBoxTrackType.Name = "comboBoxTrackType";
			this.comboBoxTrackType.Size = new System.Drawing.Size(81, 20);
			this.comboBoxTrackType.TabIndex = 6;
			// 
			// labelTrackType
			// 
			this.labelTrackType.Location = new System.Drawing.Point(8, 16);
			this.labelTrackType.Name = "labelTrackType";
			this.labelTrackType.Size = new System.Drawing.Size(96, 16);
			this.labelTrackType.TabIndex = 5;
			this.labelTrackType.Text = "Track type:";
			this.labelTrackType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// menuStripMotor
			// 
			this.menuStripMotor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemInput,
            this.toolStripMenuItemTool});
			this.menuStripMotor.Location = new System.Drawing.Point(0, 0);
			this.menuStripMotor.Name = "menuStripMotor";
			this.menuStripMotor.Size = new System.Drawing.Size(582, 24);
			this.menuStripMotor.TabIndex = 0;
			this.menuStripMotor.Text = "menuStrip1";
			// 
			// toolStripMenuItemEdit
			// 
			this.toolStripMenuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUndo,
            this.toolStripMenuItemRedo,
            this.toolStripSeparatorUndo,
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
			this.toolStripMenuItemUndo.Size = new System.Drawing.Size(149, 22);
			this.toolStripMenuItemUndo.Text = "Undo(&U)";
			// 
			// toolStripMenuItemRedo
			// 
			this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
			this.toolStripMenuItemRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.toolStripMenuItemRedo.Size = new System.Drawing.Size(149, 22);
			this.toolStripMenuItemRedo.Text = "Redo(&R)";
			// 
			// toolStripSeparatorUndo
			// 
			this.toolStripSeparatorUndo.Name = "toolStripSeparatorUndo";
			this.toolStripSeparatorUndo.Size = new System.Drawing.Size(146, 6);
			// 
			// toolStripMenuItemCleanup
			// 
			this.toolStripMenuItemCleanup.Name = "toolStripMenuItemCleanup";
			this.toolStripMenuItemCleanup.Size = new System.Drawing.Size(149, 22);
			this.toolStripMenuItemCleanup.Text = "Cleanup";
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(149, 22);
			this.toolStripMenuItemDelete.Text = "Delete(&D)";
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
			// statusStripStatus
			// 
			this.statusStripStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelY,
            this.toolStripStatusLabelX,
            this.toolStripStatusLabelTool,
            this.toolStripStatusLabelMode,
            this.toolStripStatusLabelType});
			this.statusStripStatus.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.statusStripStatus.Location = new System.Drawing.Point(0, 690);
			this.statusStripStatus.Name = "statusStripStatus";
			this.statusStripStatus.Size = new System.Drawing.Size(582, 22);
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
			// tabPageAccel
			// 
			this.tabPageAccel.Controls.Add(this.pictureBoxAccel);
			this.tabPageAccel.Controls.Add(this.panelAccel);
			this.tabPageAccel.Location = new System.Drawing.Point(4, 22);
			this.tabPageAccel.Name = "tabPageAccel";
			this.tabPageAccel.Size = new System.Drawing.Size(792, 718);
			this.tabPageAccel.TabIndex = 5;
			this.tabPageAccel.Text = "Acceleration settings";
			this.tabPageAccel.UseVisualStyleBackColor = true;
			// 
			// pictureBoxAccel
			// 
			this.pictureBoxAccel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBoxAccel.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxAccel.Name = "pictureBoxAccel";
			this.pictureBoxAccel.Size = new System.Drawing.Size(576, 718);
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
			this.panelAccel.Size = new System.Drawing.Size(216, 718);
			this.panelAccel.TabIndex = 0;
			// 
			// groupBoxPreview
			// 
			this.groupBoxPreview.Controls.Add(this.comboBoxAccelYUnit);
			this.groupBoxPreview.Controls.Add(this.comboBoxAccelXUnit);
			this.groupBoxPreview.Controls.Add(this.labelAccelYUnit);
			this.groupBoxPreview.Controls.Add(this.labelAccelXUnit);
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
			this.groupBoxPreview.Size = new System.Drawing.Size(200, 280);
			this.groupBoxPreview.TabIndex = 2;
			this.groupBoxPreview.TabStop = false;
			this.groupBoxPreview.Text = "Preview";
			// 
			// comboBoxAccelYUnit
			// 
			this.comboBoxAccelYUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelYUnit.FormattingEnabled = true;
			this.comboBoxAccelYUnit.Location = new System.Drawing.Point(88, 80);
			this.comboBoxAccelYUnit.Name = "comboBoxAccelYUnit";
			this.comboBoxAccelYUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelYUnit.TabIndex = 63;
			// 
			// comboBoxAccelXUnit
			// 
			this.comboBoxAccelXUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelXUnit.FormattingEnabled = true;
			this.comboBoxAccelXUnit.Location = new System.Drawing.Point(88, 56);
			this.comboBoxAccelXUnit.Name = "comboBoxAccelXUnit";
			this.comboBoxAccelXUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelXUnit.TabIndex = 62;
			// 
			// labelAccelYUnit
			// 
			this.labelAccelYUnit.Location = new System.Drawing.Point(8, 80);
			this.labelAccelYUnit.Name = "labelAccelYUnit";
			this.labelAccelYUnit.Size = new System.Drawing.Size(72, 16);
			this.labelAccelYUnit.TabIndex = 44;
			this.labelAccelYUnit.Text = "Y unit:";
			this.labelAccelYUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelXUnit
			// 
			this.labelAccelXUnit.Location = new System.Drawing.Point(8, 56);
			this.labelAccelXUnit.Name = "labelAccelXUnit";
			this.labelAccelXUnit.Size = new System.Drawing.Size(72, 16);
			this.labelAccelXUnit.TabIndex = 43;
			this.labelAccelXUnit.Text = "X unit:";
			this.labelAccelXUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonAccelReset
			// 
			this.buttonAccelReset.Location = new System.Drawing.Point(136, 248);
			this.buttonAccelReset.Name = "buttonAccelReset";
			this.buttonAccelReset.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelReset.TabIndex = 42;
			this.buttonAccelReset.UseVisualStyleBackColor = true;
			// 
			// buttonAccelZoomOut
			// 
			this.buttonAccelZoomOut.Location = new System.Drawing.Point(72, 248);
			this.buttonAccelZoomOut.Name = "buttonAccelZoomOut";
			this.buttonAccelZoomOut.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelZoomOut.TabIndex = 41;
			this.buttonAccelZoomOut.UseVisualStyleBackColor = true;
			// 
			// buttonAccelZoomIn
			// 
			this.buttonAccelZoomIn.Location = new System.Drawing.Point(8, 248);
			this.buttonAccelZoomIn.Name = "buttonAccelZoomIn";
			this.buttonAccelZoomIn.Size = new System.Drawing.Size(56, 24);
			this.buttonAccelZoomIn.TabIndex = 40;
			this.buttonAccelZoomIn.UseVisualStyleBackColor = true;
			// 
			// labelAccelYValue
			// 
			this.labelAccelYValue.Location = new System.Drawing.Point(88, 128);
			this.labelAccelYValue.Name = "labelAccelYValue";
			this.labelAccelYValue.Size = new System.Drawing.Size(104, 16);
			this.labelAccelYValue.TabIndex = 39;
			this.labelAccelYValue.Text = "0.00 km/h/s";
			this.labelAccelYValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXValue
			// 
			this.labelAccelXValue.Location = new System.Drawing.Point(88, 104);
			this.labelAccelXValue.Name = "labelAccelXValue";
			this.labelAccelXValue.Size = new System.Drawing.Size(104, 16);
			this.labelAccelXValue.TabIndex = 38;
			this.labelAccelXValue.Text = "0.00 km/h";
			this.labelAccelXValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXmaxUnit
			// 
			this.labelAccelXmaxUnit.Location = new System.Drawing.Point(144, 176);
			this.labelAccelXmaxUnit.Name = "labelAccelXmaxUnit";
			this.labelAccelXmaxUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelXmaxUnit.TabIndex = 37;
			this.labelAccelXmaxUnit.Text = "km/h";
			this.labelAccelXmaxUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelXminUnit
			// 
			this.labelAccelXminUnit.Location = new System.Drawing.Point(144, 152);
			this.labelAccelXminUnit.Name = "labelAccelXminUnit";
			this.labelAccelXminUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelXminUnit.TabIndex = 36;
			this.labelAccelXminUnit.Text = "km/h";
			this.labelAccelXminUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelYmaxUnit
			// 
			this.labelAccelYmaxUnit.Location = new System.Drawing.Point(144, 224);
			this.labelAccelYmaxUnit.Name = "labelAccelYmaxUnit";
			this.labelAccelYmaxUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelYmaxUnit.TabIndex = 35;
			this.labelAccelYmaxUnit.Text = "km/h/s";
			this.labelAccelYmaxUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAccelYminUnit
			// 
			this.labelAccelYminUnit.Location = new System.Drawing.Point(144, 200);
			this.labelAccelYminUnit.Name = "labelAccelYminUnit";
			this.labelAccelYminUnit.Size = new System.Drawing.Size(48, 16);
			this.labelAccelYminUnit.TabIndex = 34;
			this.labelAccelYminUnit.Text = "km/h/s";
			this.labelAccelYminUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxAccelYmax
			// 
			this.textBoxAccelYmax.Location = new System.Drawing.Point(88, 224);
			this.textBoxAccelYmax.Name = "textBoxAccelYmax";
			this.textBoxAccelYmax.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelYmax.TabIndex = 25;
			// 
			// textBoxAccelYmin
			// 
			this.textBoxAccelYmin.Location = new System.Drawing.Point(88, 200);
			this.textBoxAccelYmin.Name = "textBoxAccelYmin";
			this.textBoxAccelYmin.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelYmin.TabIndex = 24;
			// 
			// textBoxAccelXmax
			// 
			this.textBoxAccelXmax.Location = new System.Drawing.Point(88, 176);
			this.textBoxAccelXmax.Name = "textBoxAccelXmax";
			this.textBoxAccelXmax.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelXmax.TabIndex = 23;
			// 
			// textBoxAccelXmin
			// 
			this.textBoxAccelXmin.Location = new System.Drawing.Point(88, 152);
			this.textBoxAccelXmin.Name = "textBoxAccelXmin";
			this.textBoxAccelXmin.Size = new System.Drawing.Size(48, 19);
			this.textBoxAccelXmin.TabIndex = 22;
			// 
			// labelAccelYmax
			// 
			this.labelAccelYmax.Location = new System.Drawing.Point(8, 224);
			this.labelAccelYmax.Name = "labelAccelYmax";
			this.labelAccelYmax.Size = new System.Drawing.Size(72, 16);
			this.labelAccelYmax.TabIndex = 8;
			this.labelAccelYmax.Text = "Ymax:";
			this.labelAccelYmax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelYmin
			// 
			this.labelAccelYmin.Location = new System.Drawing.Point(8, 200);
			this.labelAccelYmin.Name = "labelAccelYmin";
			this.labelAccelYmin.Size = new System.Drawing.Size(72, 16);
			this.labelAccelYmin.TabIndex = 7;
			this.labelAccelYmin.Text = "Ymin:";
			this.labelAccelYmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelXmax
			// 
			this.labelAccelXmax.Location = new System.Drawing.Point(8, 176);
			this.labelAccelXmax.Name = "labelAccelXmax";
			this.labelAccelXmax.Size = new System.Drawing.Size(72, 16);
			this.labelAccelXmax.TabIndex = 6;
			this.labelAccelXmax.Text = "Xmax:";
			this.labelAccelXmax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelXmin
			// 
			this.labelAccelXmin.Location = new System.Drawing.Point(8, 152);
			this.labelAccelXmin.Name = "labelAccelXmin";
			this.labelAccelXmin.Size = new System.Drawing.Size(72, 16);
			this.labelAccelXmin.TabIndex = 5;
			this.labelAccelXmin.Text = "Xmin:";
			this.labelAccelXmin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelY
			// 
			this.labelAccelY.Location = new System.Drawing.Point(8, 128);
			this.labelAccelY.Name = "labelAccelY";
			this.labelAccelY.Size = new System.Drawing.Size(72, 16);
			this.labelAccelY.TabIndex = 4;
			this.labelAccelY.Text = "Y:";
			this.labelAccelY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelX
			// 
			this.labelAccelX.Location = new System.Drawing.Point(8, 104);
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
			this.groupBoxParameter.Controls.Add(this.comboBoxAccelV2Unit);
			this.groupBoxParameter.Controls.Add(this.comboBoxAccelV1Unit);
			this.groupBoxParameter.Controls.Add(this.comboBoxAccelA1Unit);
			this.groupBoxParameter.Controls.Add(this.comboBoxAccelA0Unit);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelA0);
			this.groupBoxParameter.Controls.Add(this.labelAccelA0);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelA1);
			this.groupBoxParameter.Controls.Add(this.labelAccelA1);
			this.groupBoxParameter.Controls.Add(this.textBoxAccelV1);
			this.groupBoxParameter.Controls.Add(this.labelAccelV1);
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
			// comboBoxAccelV2Unit
			// 
			this.comboBoxAccelV2Unit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelV2Unit.FormattingEnabled = true;
			this.comboBoxAccelV2Unit.Location = new System.Drawing.Point(144, 88);
			this.comboBoxAccelV2Unit.Name = "comboBoxAccelV2Unit";
			this.comboBoxAccelV2Unit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelV2Unit.TabIndex = 61;
			// 
			// comboBoxAccelV1Unit
			// 
			this.comboBoxAccelV1Unit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelV1Unit.FormattingEnabled = true;
			this.comboBoxAccelV1Unit.Location = new System.Drawing.Point(144, 64);
			this.comboBoxAccelV1Unit.Name = "comboBoxAccelV1Unit";
			this.comboBoxAccelV1Unit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelV1Unit.TabIndex = 60;
			// 
			// comboBoxAccelA1Unit
			// 
			this.comboBoxAccelA1Unit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelA1Unit.FormattingEnabled = true;
			this.comboBoxAccelA1Unit.Location = new System.Drawing.Point(144, 40);
			this.comboBoxAccelA1Unit.Name = "comboBoxAccelA1Unit";
			this.comboBoxAccelA1Unit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelA1Unit.TabIndex = 59;
			// 
			// comboBoxAccelA0Unit
			// 
			this.comboBoxAccelA0Unit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAccelA0Unit.FormattingEnabled = true;
			this.comboBoxAccelA0Unit.Location = new System.Drawing.Point(144, 16);
			this.comboBoxAccelA0Unit.Name = "comboBoxAccelA0Unit";
			this.comboBoxAccelA0Unit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAccelA0Unit.TabIndex = 58;
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
			// tabPageCar1
			// 
			this.tabPageCar1.Controls.Add(this.groupBoxCarGeneral);
			this.tabPageCar1.Controls.Add(this.groupBoxCab);
			this.tabPageCar1.Location = new System.Drawing.Point(4, 22);
			this.tabPageCar1.Name = "tabPageCar1";
			this.tabPageCar1.Size = new System.Drawing.Size(792, 686);
			this.tabPageCar1.TabIndex = 4;
			this.tabPageCar1.Text = "Car settings (1)";
			this.tabPageCar1.UseVisualStyleBackColor = true;
			// 
			// groupBoxCarGeneral
			// 
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxUnexposedFrontalAreaUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxExposedFrontalAreaUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxCenterOfMassHeightUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxHeightUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxWidthUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxLengthUnit);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxMassUnit);
			this.groupBoxCarGeneral.Controls.Add(this.buttonRightDoorSet);
			this.groupBoxCarGeneral.Controls.Add(this.buttonLeftDoorSet);
			this.groupBoxCarGeneral.Controls.Add(this.checkBoxIsControlledCar);
			this.groupBoxCarGeneral.Controls.Add(this.comboBoxReAdhesionDevice);
			this.groupBoxCarGeneral.Controls.Add(this.labelIsControlledCar);
			this.groupBoxCarGeneral.Controls.Add(this.labelReAdhesionDevice);
			this.groupBoxCarGeneral.Controls.Add(this.labelRightDoor);
			this.groupBoxCarGeneral.Controls.Add(this.labelLeftDoor);
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
			this.groupBoxCarGeneral.Controls.Add(this.labelLoadingSway);
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
			this.groupBoxCarGeneral.Size = new System.Drawing.Size(280, 560);
			this.groupBoxCarGeneral.TabIndex = 0;
			this.groupBoxCarGeneral.TabStop = false;
			this.groupBoxCarGeneral.Text = "General";
			// 
			// comboBoxUnexposedFrontalAreaUnit
			// 
			this.comboBoxUnexposedFrontalAreaUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxUnexposedFrontalAreaUnit.FormattingEnabled = true;
			this.comboBoxUnexposedFrontalAreaUnit.Location = new System.Drawing.Point(216, 184);
			this.comboBoxUnexposedFrontalAreaUnit.Name = "comboBoxUnexposedFrontalAreaUnit";
			this.comboBoxUnexposedFrontalAreaUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxUnexposedFrontalAreaUnit.TabIndex = 61;
			// 
			// comboBoxExposedFrontalAreaUnit
			// 
			this.comboBoxExposedFrontalAreaUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxExposedFrontalAreaUnit.FormattingEnabled = true;
			this.comboBoxExposedFrontalAreaUnit.Location = new System.Drawing.Point(216, 160);
			this.comboBoxExposedFrontalAreaUnit.Name = "comboBoxExposedFrontalAreaUnit";
			this.comboBoxExposedFrontalAreaUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxExposedFrontalAreaUnit.TabIndex = 60;
			// 
			// comboBoxCenterOfMassHeightUnit
			// 
			this.comboBoxCenterOfMassHeightUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCenterOfMassHeightUnit.FormattingEnabled = true;
			this.comboBoxCenterOfMassHeightUnit.Location = new System.Drawing.Point(216, 136);
			this.comboBoxCenterOfMassHeightUnit.Name = "comboBoxCenterOfMassHeightUnit";
			this.comboBoxCenterOfMassHeightUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCenterOfMassHeightUnit.TabIndex = 59;
			// 
			// comboBoxHeightUnit
			// 
			this.comboBoxHeightUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxHeightUnit.FormattingEnabled = true;
			this.comboBoxHeightUnit.Location = new System.Drawing.Point(216, 112);
			this.comboBoxHeightUnit.Name = "comboBoxHeightUnit";
			this.comboBoxHeightUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxHeightUnit.TabIndex = 58;
			// 
			// comboBoxWidthUnit
			// 
			this.comboBoxWidthUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxWidthUnit.FormattingEnabled = true;
			this.comboBoxWidthUnit.Location = new System.Drawing.Point(216, 88);
			this.comboBoxWidthUnit.Name = "comboBoxWidthUnit";
			this.comboBoxWidthUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxWidthUnit.TabIndex = 57;
			// 
			// comboBoxLengthUnit
			// 
			this.comboBoxLengthUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxLengthUnit.FormattingEnabled = true;
			this.comboBoxLengthUnit.Location = new System.Drawing.Point(216, 64);
			this.comboBoxLengthUnit.Name = "comboBoxLengthUnit";
			this.comboBoxLengthUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxLengthUnit.TabIndex = 56;
			// 
			// comboBoxMassUnit
			// 
			this.comboBoxMassUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMassUnit.FormattingEnabled = true;
			this.comboBoxMassUnit.Location = new System.Drawing.Point(216, 40);
			this.comboBoxMassUnit.Name = "comboBoxMassUnit";
			this.comboBoxMassUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMassUnit.TabIndex = 55;
			// 
			// buttonRightDoorSet
			// 
			this.buttonRightDoorSet.Location = new System.Drawing.Point(160, 480);
			this.buttonRightDoorSet.Name = "buttonRightDoorSet";
			this.buttonRightDoorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonRightDoorSet.TabIndex = 54;
			this.buttonRightDoorSet.Text = "Set...";
			this.buttonRightDoorSet.UseVisualStyleBackColor = true;
			this.buttonRightDoorSet.Click += new System.EventHandler(this.ButtonRightDoorSet_Click);
			// 
			// buttonLeftDoorSet
			// 
			this.buttonLeftDoorSet.Location = new System.Drawing.Point(160, 456);
			this.buttonLeftDoorSet.Name = "buttonLeftDoorSet";
			this.buttonLeftDoorSet.Size = new System.Drawing.Size(48, 19);
			this.buttonLeftDoorSet.TabIndex = 53;
			this.buttonLeftDoorSet.Text = "Set...";
			this.buttonLeftDoorSet.UseVisualStyleBackColor = true;
			this.buttonLeftDoorSet.Click += new System.EventHandler(this.ButtonLeftDoorSet_Click);
			// 
			// checkBoxIsControlledCar
			// 
			this.checkBoxIsControlledCar.Location = new System.Drawing.Point(160, 528);
			this.checkBoxIsControlledCar.Name = "checkBoxIsControlledCar";
			this.checkBoxIsControlledCar.Size = new System.Drawing.Size(48, 16);
			this.checkBoxIsControlledCar.TabIndex = 52;
			this.checkBoxIsControlledCar.UseVisualStyleBackColor = true;
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
			this.comboBoxReAdhesionDevice.Location = new System.Drawing.Point(160, 504);
			this.comboBoxReAdhesionDevice.Name = "comboBoxReAdhesionDevice";
			this.comboBoxReAdhesionDevice.Size = new System.Drawing.Size(112, 20);
			this.comboBoxReAdhesionDevice.TabIndex = 16;
			// 
			// labelIsControlledCar
			// 
			this.labelIsControlledCar.Location = new System.Drawing.Point(8, 528);
			this.labelIsControlledCar.Name = "labelIsControlledCar";
			this.labelIsControlledCar.Size = new System.Drawing.Size(144, 16);
			this.labelIsControlledCar.TabIndex = 51;
			this.labelIsControlledCar.Text = "IsControlledCar:";
			this.labelIsControlledCar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelReAdhesionDevice
			// 
			this.labelReAdhesionDevice.Location = new System.Drawing.Point(8, 504);
			this.labelReAdhesionDevice.Name = "labelReAdhesionDevice";
			this.labelReAdhesionDevice.Size = new System.Drawing.Size(144, 16);
			this.labelReAdhesionDevice.TabIndex = 5;
			this.labelReAdhesionDevice.Text = "ReAdhesionDevice:";
			this.labelReAdhesionDevice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelRightDoor
			// 
			this.labelRightDoor.Location = new System.Drawing.Point(8, 480);
			this.labelRightDoor.Name = "labelRightDoor";
			this.labelRightDoor.Size = new System.Drawing.Size(144, 16);
			this.labelRightDoor.TabIndex = 41;
			this.labelRightDoor.Text = "RightDoor:";
			this.labelRightDoor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLeftDoor
			// 
			this.labelLeftDoor.Location = new System.Drawing.Point(8, 456);
			this.labelLeftDoor.Name = "labelLeftDoor";
			this.labelLeftDoor.Size = new System.Drawing.Size(144, 16);
			this.labelLeftDoor.TabIndex = 39;
			this.labelLeftDoor.Text = "LeftDoor:";
			this.labelLeftDoor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			this.buttonObjectOpen.Location = new System.Drawing.Point(216, 432);
			this.buttonObjectOpen.Name = "buttonObjectOpen";
			this.buttonObjectOpen.Size = new System.Drawing.Size(56, 19);
			this.buttonObjectOpen.TabIndex = 3;
			this.buttonObjectOpen.Text = "Open...";
			this.buttonObjectOpen.UseVisualStyleBackColor = true;
			this.buttonObjectOpen.Click += new System.EventHandler(this.ButtonObjectOpen_Click);
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
			this.textBoxObject.Size = new System.Drawing.Size(112, 19);
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
			this.groupBoxAxles.Controls.Add(this.comboBoxRearAxleUnit);
			this.groupBoxAxles.Controls.Add(this.comboBoxFrontAxleUnit);
			this.groupBoxAxles.Controls.Add(this.textBoxRearAxle);
			this.groupBoxAxles.Controls.Add(this.textBoxFrontAxle);
			this.groupBoxAxles.Controls.Add(this.labelRearAxle);
			this.groupBoxAxles.Controls.Add(this.labelFrontAxle);
			this.groupBoxAxles.Location = new System.Drawing.Point(8, 232);
			this.groupBoxAxles.Name = "groupBoxAxles";
			this.groupBoxAxles.Size = new System.Drawing.Size(264, 72);
			this.groupBoxAxles.TabIndex = 8;
			this.groupBoxAxles.TabStop = false;
			this.groupBoxAxles.Text = "Axles";
			// 
			// comboBoxRearAxleUnit
			// 
			this.comboBoxRearAxleUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxRearAxleUnit.FormattingEnabled = true;
			this.comboBoxRearAxleUnit.Location = new System.Drawing.Point(208, 40);
			this.comboBoxRearAxleUnit.Name = "comboBoxRearAxleUnit";
			this.comboBoxRearAxleUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxRearAxleUnit.TabIndex = 57;
			// 
			// comboBoxFrontAxleUnit
			// 
			this.comboBoxFrontAxleUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxFrontAxleUnit.FormattingEnabled = true;
			this.comboBoxFrontAxleUnit.Location = new System.Drawing.Point(208, 16);
			this.comboBoxFrontAxleUnit.Name = "comboBoxFrontAxleUnit";
			this.comboBoxFrontAxleUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxFrontAxleUnit.TabIndex = 56;
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
			// groupBoxCab
			// 
			this.groupBoxCab.Controls.Add(this.comboBoxCabZUnit);
			this.groupBoxCab.Controls.Add(this.comboBoxCabYUnit);
			this.groupBoxCab.Controls.Add(this.comboBoxCabXUnit);
			this.groupBoxCab.Controls.Add(this.groupBoxExternalCab);
			this.groupBoxCab.Controls.Add(this.checkBoxIsEmbeddedCab);
			this.groupBoxCab.Controls.Add(this.labelIsEmbeddedCab);
			this.groupBoxCab.Controls.Add(this.textBoxCabZ);
			this.groupBoxCab.Controls.Add(this.textBoxCabY);
			this.groupBoxCab.Controls.Add(this.textBoxCabX);
			this.groupBoxCab.Controls.Add(this.labelCabZ);
			this.groupBoxCab.Controls.Add(this.labelCabY);
			this.groupBoxCab.Controls.Add(this.labelCabX);
			this.groupBoxCab.Location = new System.Drawing.Point(296, 8);
			this.groupBoxCab.Name = "groupBoxCab";
			this.groupBoxCab.Size = new System.Drawing.Size(272, 504);
			this.groupBoxCab.TabIndex = 1;
			this.groupBoxCab.TabStop = false;
			this.groupBoxCab.Text = "Cab";
			// 
			// comboBoxCabZUnit
			// 
			this.comboBoxCabZUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCabZUnit.FormattingEnabled = true;
			this.comboBoxCabZUnit.Location = new System.Drawing.Point(200, 64);
			this.comboBoxCabZUnit.Name = "comboBoxCabZUnit";
			this.comboBoxCabZUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCabZUnit.TabIndex = 58;
			// 
			// comboBoxCabYUnit
			// 
			this.comboBoxCabYUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCabYUnit.FormattingEnabled = true;
			this.comboBoxCabYUnit.Location = new System.Drawing.Point(200, 40);
			this.comboBoxCabYUnit.Name = "comboBoxCabYUnit";
			this.comboBoxCabYUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCabYUnit.TabIndex = 57;
			// 
			// comboBoxCabXUnit
			// 
			this.comboBoxCabXUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCabXUnit.FormattingEnabled = true;
			this.comboBoxCabXUnit.Location = new System.Drawing.Point(200, 16);
			this.comboBoxCabXUnit.Name = "comboBoxCabXUnit";
			this.comboBoxCabXUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCabXUnit.TabIndex = 56;
			// 
			// groupBoxExternalCab
			// 
			this.groupBoxExternalCab.Controls.Add(this.buttonCabFileNameOpen);
			this.groupBoxExternalCab.Controls.Add(this.groupBoxCameraRestriction);
			this.groupBoxExternalCab.Controls.Add(this.textBoxCabFileName);
			this.groupBoxExternalCab.Controls.Add(this.labelCabFileName);
			this.groupBoxExternalCab.Location = new System.Drawing.Point(8, 112);
			this.groupBoxExternalCab.Name = "groupBoxExternalCab";
			this.groupBoxExternalCab.Size = new System.Drawing.Size(256, 384);
			this.groupBoxExternalCab.TabIndex = 31;
			this.groupBoxExternalCab.TabStop = false;
			this.groupBoxExternalCab.Text = "ExternalCab";
			// 
			// buttonCabFileNameOpen
			// 
			this.buttonCabFileNameOpen.Location = new System.Drawing.Point(192, 40);
			this.buttonCabFileNameOpen.Name = "buttonCabFileNameOpen";
			this.buttonCabFileNameOpen.Size = new System.Drawing.Size(56, 19);
			this.buttonCabFileNameOpen.TabIndex = 35;
			this.buttonCabFileNameOpen.Text = "Open...";
			this.buttonCabFileNameOpen.UseVisualStyleBackColor = true;
			this.buttonCabFileNameOpen.Click += new System.EventHandler(this.ButtonCabFileNameOpen_Click);
			// 
			// groupBoxCameraRestriction
			// 
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionDownUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionUpUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionRightUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionLeftUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionBackwardsUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.comboBoxCameraRestrictionForwardsUnit);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedDown);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedDown);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionDown);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDown);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedUp);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedUp);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionUp);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionUp);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedRight);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedRight);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionRight);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionRight);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedLeft);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedLeft);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionLeft);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionLeft);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedBackwards);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedBackwards);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionBackwards);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionBackwards);
			this.groupBoxCameraRestriction.Controls.Add(this.checkBoxCameraRestrictionDefinedForwards);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionDefinedForwards);
			this.groupBoxCameraRestriction.Controls.Add(this.textBoxCameraRestrictionForwards);
			this.groupBoxCameraRestriction.Controls.Add(this.labelCameraRestrictionForwards);
			this.groupBoxCameraRestriction.Location = new System.Drawing.Point(8, 64);
			this.groupBoxCameraRestriction.Name = "groupBoxCameraRestriction";
			this.groupBoxCameraRestriction.Size = new System.Drawing.Size(240, 312);
			this.groupBoxCameraRestriction.TabIndex = 34;
			this.groupBoxCameraRestriction.TabStop = false;
			this.groupBoxCameraRestriction.Text = "CameraRestriction";
			// 
			// comboBoxCameraRestrictionDownUnit
			// 
			this.comboBoxCameraRestrictionDownUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionDownUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionDownUnit.Location = new System.Drawing.Point(184, 280);
			this.comboBoxCameraRestrictionDownUnit.Name = "comboBoxCameraRestrictionDownUnit";
			this.comboBoxCameraRestrictionDownUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionDownUnit.TabIndex = 62;
			// 
			// comboBoxCameraRestrictionUpUnit
			// 
			this.comboBoxCameraRestrictionUpUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionUpUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionUpUnit.Location = new System.Drawing.Point(184, 232);
			this.comboBoxCameraRestrictionUpUnit.Name = "comboBoxCameraRestrictionUpUnit";
			this.comboBoxCameraRestrictionUpUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionUpUnit.TabIndex = 61;
			// 
			// comboBoxCameraRestrictionRightUnit
			// 
			this.comboBoxCameraRestrictionRightUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionRightUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionRightUnit.Location = new System.Drawing.Point(184, 184);
			this.comboBoxCameraRestrictionRightUnit.Name = "comboBoxCameraRestrictionRightUnit";
			this.comboBoxCameraRestrictionRightUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionRightUnit.TabIndex = 60;
			// 
			// comboBoxCameraRestrictionLeftUnit
			// 
			this.comboBoxCameraRestrictionLeftUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionLeftUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionLeftUnit.Location = new System.Drawing.Point(184, 136);
			this.comboBoxCameraRestrictionLeftUnit.Name = "comboBoxCameraRestrictionLeftUnit";
			this.comboBoxCameraRestrictionLeftUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionLeftUnit.TabIndex = 59;
			// 
			// comboBoxCameraRestrictionBackwardsUnit
			// 
			this.comboBoxCameraRestrictionBackwardsUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionBackwardsUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionBackwardsUnit.Location = new System.Drawing.Point(184, 88);
			this.comboBoxCameraRestrictionBackwardsUnit.Name = "comboBoxCameraRestrictionBackwardsUnit";
			this.comboBoxCameraRestrictionBackwardsUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionBackwardsUnit.TabIndex = 58;
			// 
			// comboBoxCameraRestrictionForwardsUnit
			// 
			this.comboBoxCameraRestrictionForwardsUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCameraRestrictionForwardsUnit.FormattingEnabled = true;
			this.comboBoxCameraRestrictionForwardsUnit.Location = new System.Drawing.Point(184, 40);
			this.comboBoxCameraRestrictionForwardsUnit.Name = "comboBoxCameraRestrictionForwardsUnit";
			this.comboBoxCameraRestrictionForwardsUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCameraRestrictionForwardsUnit.TabIndex = 57;
			// 
			// checkBoxCameraRestrictionDefinedDown
			// 
			this.checkBoxCameraRestrictionDefinedDown.Location = new System.Drawing.Point(128, 256);
			this.checkBoxCameraRestrictionDefinedDown.Name = "checkBoxCameraRestrictionDefinedDown";
			this.checkBoxCameraRestrictionDefinedDown.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedDown.TabIndex = 56;
			this.checkBoxCameraRestrictionDefinedDown.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedDown
			// 
			this.labelCameraRestrictionDefinedDown.Location = new System.Drawing.Point(8, 256);
			this.labelCameraRestrictionDefinedDown.Name = "labelCameraRestrictionDefinedDown";
			this.labelCameraRestrictionDefinedDown.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedDown.TabIndex = 55;
			this.labelCameraRestrictionDefinedDown.Text = "DefinedDown:";
			this.labelCameraRestrictionDefinedDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionDown
			// 
			this.textBoxCameraRestrictionDown.Location = new System.Drawing.Point(128, 280);
			this.textBoxCameraRestrictionDown.Name = "textBoxCameraRestrictionDown";
			this.textBoxCameraRestrictionDown.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionDown.TabIndex = 53;
			// 
			// labelCameraRestrictionDown
			// 
			this.labelCameraRestrictionDown.Location = new System.Drawing.Point(8, 280);
			this.labelCameraRestrictionDown.Name = "labelCameraRestrictionDown";
			this.labelCameraRestrictionDown.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDown.TabIndex = 52;
			this.labelCameraRestrictionDown.Text = "Down:";
			this.labelCameraRestrictionDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxCameraRestrictionDefinedUp
			// 
			this.checkBoxCameraRestrictionDefinedUp.Location = new System.Drawing.Point(128, 208);
			this.checkBoxCameraRestrictionDefinedUp.Name = "checkBoxCameraRestrictionDefinedUp";
			this.checkBoxCameraRestrictionDefinedUp.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedUp.TabIndex = 51;
			this.checkBoxCameraRestrictionDefinedUp.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedUp
			// 
			this.labelCameraRestrictionDefinedUp.Location = new System.Drawing.Point(8, 208);
			this.labelCameraRestrictionDefinedUp.Name = "labelCameraRestrictionDefinedUp";
			this.labelCameraRestrictionDefinedUp.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedUp.TabIndex = 50;
			this.labelCameraRestrictionDefinedUp.Text = "DefinedUp:";
			this.labelCameraRestrictionDefinedUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionUp
			// 
			this.textBoxCameraRestrictionUp.Location = new System.Drawing.Point(128, 232);
			this.textBoxCameraRestrictionUp.Name = "textBoxCameraRestrictionUp";
			this.textBoxCameraRestrictionUp.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionUp.TabIndex = 48;
			// 
			// labelCameraRestrictionUp
			// 
			this.labelCameraRestrictionUp.Location = new System.Drawing.Point(8, 232);
			this.labelCameraRestrictionUp.Name = "labelCameraRestrictionUp";
			this.labelCameraRestrictionUp.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionUp.TabIndex = 47;
			this.labelCameraRestrictionUp.Text = "Up:";
			this.labelCameraRestrictionUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxCameraRestrictionDefinedRight
			// 
			this.checkBoxCameraRestrictionDefinedRight.Location = new System.Drawing.Point(128, 160);
			this.checkBoxCameraRestrictionDefinedRight.Name = "checkBoxCameraRestrictionDefinedRight";
			this.checkBoxCameraRestrictionDefinedRight.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedRight.TabIndex = 46;
			this.checkBoxCameraRestrictionDefinedRight.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedRight
			// 
			this.labelCameraRestrictionDefinedRight.Location = new System.Drawing.Point(8, 160);
			this.labelCameraRestrictionDefinedRight.Name = "labelCameraRestrictionDefinedRight";
			this.labelCameraRestrictionDefinedRight.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedRight.TabIndex = 45;
			this.labelCameraRestrictionDefinedRight.Text = "DefinedRight:";
			this.labelCameraRestrictionDefinedRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionRight
			// 
			this.textBoxCameraRestrictionRight.Location = new System.Drawing.Point(128, 184);
			this.textBoxCameraRestrictionRight.Name = "textBoxCameraRestrictionRight";
			this.textBoxCameraRestrictionRight.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionRight.TabIndex = 43;
			// 
			// labelCameraRestrictionRight
			// 
			this.labelCameraRestrictionRight.Location = new System.Drawing.Point(8, 184);
			this.labelCameraRestrictionRight.Name = "labelCameraRestrictionRight";
			this.labelCameraRestrictionRight.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionRight.TabIndex = 42;
			this.labelCameraRestrictionRight.Text = "Right:";
			this.labelCameraRestrictionRight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxCameraRestrictionDefinedLeft
			// 
			this.checkBoxCameraRestrictionDefinedLeft.Location = new System.Drawing.Point(128, 112);
			this.checkBoxCameraRestrictionDefinedLeft.Name = "checkBoxCameraRestrictionDefinedLeft";
			this.checkBoxCameraRestrictionDefinedLeft.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedLeft.TabIndex = 41;
			this.checkBoxCameraRestrictionDefinedLeft.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedLeft
			// 
			this.labelCameraRestrictionDefinedLeft.Location = new System.Drawing.Point(8, 112);
			this.labelCameraRestrictionDefinedLeft.Name = "labelCameraRestrictionDefinedLeft";
			this.labelCameraRestrictionDefinedLeft.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedLeft.TabIndex = 40;
			this.labelCameraRestrictionDefinedLeft.Text = "DefinedLeft:";
			this.labelCameraRestrictionDefinedLeft.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionLeft
			// 
			this.textBoxCameraRestrictionLeft.Location = new System.Drawing.Point(128, 136);
			this.textBoxCameraRestrictionLeft.Name = "textBoxCameraRestrictionLeft";
			this.textBoxCameraRestrictionLeft.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionLeft.TabIndex = 38;
			// 
			// labelCameraRestrictionLeft
			// 
			this.labelCameraRestrictionLeft.Location = new System.Drawing.Point(8, 136);
			this.labelCameraRestrictionLeft.Name = "labelCameraRestrictionLeft";
			this.labelCameraRestrictionLeft.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionLeft.TabIndex = 37;
			this.labelCameraRestrictionLeft.Text = "Left:";
			this.labelCameraRestrictionLeft.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxCameraRestrictionDefinedBackwards
			// 
			this.checkBoxCameraRestrictionDefinedBackwards.Location = new System.Drawing.Point(128, 64);
			this.checkBoxCameraRestrictionDefinedBackwards.Name = "checkBoxCameraRestrictionDefinedBackwards";
			this.checkBoxCameraRestrictionDefinedBackwards.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedBackwards.TabIndex = 36;
			this.checkBoxCameraRestrictionDefinedBackwards.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedBackwards
			// 
			this.labelCameraRestrictionDefinedBackwards.Location = new System.Drawing.Point(8, 64);
			this.labelCameraRestrictionDefinedBackwards.Name = "labelCameraRestrictionDefinedBackwards";
			this.labelCameraRestrictionDefinedBackwards.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedBackwards.TabIndex = 35;
			this.labelCameraRestrictionDefinedBackwards.Text = "DefinedBackwards:";
			this.labelCameraRestrictionDefinedBackwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionBackwards
			// 
			this.textBoxCameraRestrictionBackwards.Location = new System.Drawing.Point(128, 88);
			this.textBoxCameraRestrictionBackwards.Name = "textBoxCameraRestrictionBackwards";
			this.textBoxCameraRestrictionBackwards.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionBackwards.TabIndex = 33;
			// 
			// labelCameraRestrictionBackwards
			// 
			this.labelCameraRestrictionBackwards.Location = new System.Drawing.Point(8, 88);
			this.labelCameraRestrictionBackwards.Name = "labelCameraRestrictionBackwards";
			this.labelCameraRestrictionBackwards.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionBackwards.TabIndex = 32;
			this.labelCameraRestrictionBackwards.Text = "Backwards:";
			this.labelCameraRestrictionBackwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxCameraRestrictionDefinedForwards
			// 
			this.checkBoxCameraRestrictionDefinedForwards.Location = new System.Drawing.Point(128, 16);
			this.checkBoxCameraRestrictionDefinedForwards.Name = "checkBoxCameraRestrictionDefinedForwards";
			this.checkBoxCameraRestrictionDefinedForwards.Size = new System.Drawing.Size(48, 16);
			this.checkBoxCameraRestrictionDefinedForwards.TabIndex = 31;
			this.checkBoxCameraRestrictionDefinedForwards.UseVisualStyleBackColor = true;
			// 
			// labelCameraRestrictionDefinedForwards
			// 
			this.labelCameraRestrictionDefinedForwards.Location = new System.Drawing.Point(8, 16);
			this.labelCameraRestrictionDefinedForwards.Name = "labelCameraRestrictionDefinedForwards";
			this.labelCameraRestrictionDefinedForwards.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionDefinedForwards.TabIndex = 9;
			this.labelCameraRestrictionDefinedForwards.Text = "DefinedForwards:";
			this.labelCameraRestrictionDefinedForwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCameraRestrictionForwards
			// 
			this.textBoxCameraRestrictionForwards.Location = new System.Drawing.Point(128, 40);
			this.textBoxCameraRestrictionForwards.Name = "textBoxCameraRestrictionForwards";
			this.textBoxCameraRestrictionForwards.Size = new System.Drawing.Size(48, 19);
			this.textBoxCameraRestrictionForwards.TabIndex = 7;
			// 
			// labelCameraRestrictionForwards
			// 
			this.labelCameraRestrictionForwards.Location = new System.Drawing.Point(8, 40);
			this.labelCameraRestrictionForwards.Name = "labelCameraRestrictionForwards";
			this.labelCameraRestrictionForwards.Size = new System.Drawing.Size(112, 16);
			this.labelCameraRestrictionForwards.TabIndex = 6;
			this.labelCameraRestrictionForwards.Text = "Forwards:";
			this.labelCameraRestrictionForwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCabFileName
			// 
			this.textBoxCabFileName.Location = new System.Drawing.Point(136, 16);
			this.textBoxCabFileName.Name = "textBoxCabFileName";
			this.textBoxCabFileName.Size = new System.Drawing.Size(112, 19);
			this.textBoxCabFileName.TabIndex = 33;
			// 
			// labelCabFileName
			// 
			this.labelCabFileName.Location = new System.Drawing.Point(8, 16);
			this.labelCabFileName.Name = "labelCabFileName";
			this.labelCabFileName.Size = new System.Drawing.Size(120, 16);
			this.labelCabFileName.TabIndex = 32;
			this.labelCabFileName.Text = "Filename:";
			this.labelCabFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxIsEmbeddedCab
			// 
			this.checkBoxIsEmbeddedCab.Location = new System.Drawing.Point(144, 88);
			this.checkBoxIsEmbeddedCab.Name = "checkBoxIsEmbeddedCab";
			this.checkBoxIsEmbeddedCab.Size = new System.Drawing.Size(48, 16);
			this.checkBoxIsEmbeddedCab.TabIndex = 30;
			this.checkBoxIsEmbeddedCab.UseVisualStyleBackColor = true;
			// 
			// labelIsEmbeddedCab
			// 
			this.labelIsEmbeddedCab.Location = new System.Drawing.Point(8, 88);
			this.labelIsEmbeddedCab.Name = "labelIsEmbeddedCab";
			this.labelIsEmbeddedCab.Size = new System.Drawing.Size(128, 16);
			this.labelIsEmbeddedCab.TabIndex = 10;
			this.labelIsEmbeddedCab.Text = "IsEmbeddedCab:";
			this.labelIsEmbeddedCab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCabZ
			// 
			this.textBoxCabZ.Location = new System.Drawing.Point(144, 64);
			this.textBoxCabZ.Name = "textBoxCabZ";
			this.textBoxCabZ.Size = new System.Drawing.Size(48, 19);
			this.textBoxCabZ.TabIndex = 8;
			// 
			// textBoxCabY
			// 
			this.textBoxCabY.Location = new System.Drawing.Point(144, 40);
			this.textBoxCabY.Name = "textBoxCabY";
			this.textBoxCabY.Size = new System.Drawing.Size(48, 19);
			this.textBoxCabY.TabIndex = 6;
			// 
			// textBoxCabX
			// 
			this.textBoxCabX.Location = new System.Drawing.Point(144, 16);
			this.textBoxCabX.Name = "textBoxCabX";
			this.textBoxCabX.Size = new System.Drawing.Size(48, 19);
			this.textBoxCabX.TabIndex = 4;
			// 
			// labelCabZ
			// 
			this.labelCabZ.Location = new System.Drawing.Point(8, 64);
			this.labelCabZ.Name = "labelCabZ";
			this.labelCabZ.Size = new System.Drawing.Size(128, 16);
			this.labelCabZ.TabIndex = 2;
			this.labelCabZ.Text = "Z:";
			this.labelCabZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCabY
			// 
			this.labelCabY.Location = new System.Drawing.Point(8, 40);
			this.labelCabY.Name = "labelCabY";
			this.labelCabY.Size = new System.Drawing.Size(128, 16);
			this.labelCabY.TabIndex = 1;
			this.labelCabY.Text = "Y:";
			this.labelCabY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCabX
			// 
			this.labelCabX.Location = new System.Drawing.Point(8, 16);
			this.labelCabX.Name = "labelCabX";
			this.labelCabX.Size = new System.Drawing.Size(128, 16);
			this.labelCabX.TabIndex = 0;
			this.labelCabX.Text = "X:";
			this.labelCabX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxPressure
			// 
			this.groupBoxPressure.Controls.Add(this.groupBoxBrakeCylinder);
			this.groupBoxPressure.Controls.Add(this.groupBoxStraightAirPipe);
			this.groupBoxPressure.Controls.Add(this.groupBoxBrakePipe);
			this.groupBoxPressure.Controls.Add(this.groupBoxEqualizingReservoir);
			this.groupBoxPressure.Controls.Add(this.groupBoxAuxiliaryReservoir);
			this.groupBoxPressure.Controls.Add(this.groupBoxMainReservoir);
			this.groupBoxPressure.Controls.Add(this.groupBoxCompressor);
			this.groupBoxPressure.Location = new System.Drawing.Point(8, 8);
			this.groupBoxPressure.Name = "groupBoxPressure";
			this.groupBoxPressure.Size = new System.Drawing.Size(304, 672);
			this.groupBoxPressure.TabIndex = 33;
			this.groupBoxPressure.TabStop = false;
			this.groupBoxPressure.Text = "Pressure";
			// 
			// groupBoxBrakeCylinder
			// 
			this.groupBoxBrakeCylinder.Controls.Add(this.comboBoxBrakeCylinderReleaseRateUnit);
			this.groupBoxBrakeCylinder.Controls.Add(this.comboBoxBrakeCylinderEmergencyRateUnit);
			this.groupBoxBrakeCylinder.Controls.Add(this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit);
			this.groupBoxBrakeCylinder.Controls.Add(this.comboBoxBrakeCylinderServiceMaximumPressureUnit);
			this.groupBoxBrakeCylinder.Controls.Add(this.textBoxBrakeCylinderReleaseRate);
			this.groupBoxBrakeCylinder.Controls.Add(this.labelBrakeCylinderServiceMaximumPressure);
			this.groupBoxBrakeCylinder.Controls.Add(this.labelBrakeCylinderEmergencyMaximumPressure);
			this.groupBoxBrakeCylinder.Controls.Add(this.textBoxBrakeCylinderServiceMaximumPressure);
			this.groupBoxBrakeCylinder.Controls.Add(this.textBoxBrakeCylinderEmergencyMaximumPressure);
			this.groupBoxBrakeCylinder.Controls.Add(this.labelBrakeCylinderEmergencyRate);
			this.groupBoxBrakeCylinder.Controls.Add(this.labelBrakeCylinderReleaseRate);
			this.groupBoxBrakeCylinder.Controls.Add(this.textBoxBrakeCylinderEmergencyRate);
			this.groupBoxBrakeCylinder.Location = new System.Drawing.Point(8, 544);
			this.groupBoxBrakeCylinder.Name = "groupBoxBrakeCylinder";
			this.groupBoxBrakeCylinder.Size = new System.Drawing.Size(288, 120);
			this.groupBoxBrakeCylinder.TabIndex = 46;
			this.groupBoxBrakeCylinder.TabStop = false;
			this.groupBoxBrakeCylinder.Text = "BrakeCylinder";
			// 
			// comboBoxBrakeCylinderReleaseRateUnit
			// 
			this.comboBoxBrakeCylinderReleaseRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeCylinderReleaseRateUnit.FormattingEnabled = true;
			this.comboBoxBrakeCylinderReleaseRateUnit.Location = new System.Drawing.Point(232, 88);
			this.comboBoxBrakeCylinderReleaseRateUnit.Name = "comboBoxBrakeCylinderReleaseRateUnit";
			this.comboBoxBrakeCylinderReleaseRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakeCylinderReleaseRateUnit.TabIndex = 59;
			// 
			// comboBoxBrakeCylinderEmergencyRateUnit
			// 
			this.comboBoxBrakeCylinderEmergencyRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeCylinderEmergencyRateUnit.FormattingEnabled = true;
			this.comboBoxBrakeCylinderEmergencyRateUnit.Location = new System.Drawing.Point(232, 64);
			this.comboBoxBrakeCylinderEmergencyRateUnit.Name = "comboBoxBrakeCylinderEmergencyRateUnit";
			this.comboBoxBrakeCylinderEmergencyRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakeCylinderEmergencyRateUnit.TabIndex = 58;
			// 
			// comboBoxBrakeCylinderEmergencyMaximumPressureUnit
			// 
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.FormattingEnabled = true;
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.Location = new System.Drawing.Point(232, 40);
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.Name = "comboBoxBrakeCylinderEmergencyMaximumPressureUnit";
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakeCylinderEmergencyMaximumPressureUnit.TabIndex = 57;
			// 
			// comboBoxBrakeCylinderServiceMaximumPressureUnit
			// 
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.FormattingEnabled = true;
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.Name = "comboBoxBrakeCylinderServiceMaximumPressureUnit";
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakeCylinderServiceMaximumPressureUnit.TabIndex = 57;
			// 
			// textBoxBrakeCylinderReleaseRate
			// 
			this.textBoxBrakeCylinderReleaseRate.Location = new System.Drawing.Point(176, 88);
			this.textBoxBrakeCylinderReleaseRate.Name = "textBoxBrakeCylinderReleaseRate";
			this.textBoxBrakeCylinderReleaseRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakeCylinderReleaseRate.TabIndex = 30;
			// 
			// labelBrakeCylinderServiceMaximumPressure
			// 
			this.labelBrakeCylinderServiceMaximumPressure.Location = new System.Drawing.Point(8, 16);
			this.labelBrakeCylinderServiceMaximumPressure.Name = "labelBrakeCylinderServiceMaximumPressure";
			this.labelBrakeCylinderServiceMaximumPressure.Size = new System.Drawing.Size(160, 16);
			this.labelBrakeCylinderServiceMaximumPressure.TabIndex = 9;
			this.labelBrakeCylinderServiceMaximumPressure.Text = "ServiceMaximumPressure:";
			this.labelBrakeCylinderServiceMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderEmergencyMaximumPressure
			// 
			this.labelBrakeCylinderEmergencyMaximumPressure.Location = new System.Drawing.Point(8, 40);
			this.labelBrakeCylinderEmergencyMaximumPressure.Name = "labelBrakeCylinderEmergencyMaximumPressure";
			this.labelBrakeCylinderEmergencyMaximumPressure.Size = new System.Drawing.Size(160, 16);
			this.labelBrakeCylinderEmergencyMaximumPressure.TabIndex = 8;
			this.labelBrakeCylinderEmergencyMaximumPressure.Text = "EmergencyMaximumPressure:";
			this.labelBrakeCylinderEmergencyMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakeCylinderServiceMaximumPressure
			// 
			this.textBoxBrakeCylinderServiceMaximumPressure.Location = new System.Drawing.Point(176, 16);
			this.textBoxBrakeCylinderServiceMaximumPressure.Name = "textBoxBrakeCylinderServiceMaximumPressure";
			this.textBoxBrakeCylinderServiceMaximumPressure.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakeCylinderServiceMaximumPressure.TabIndex = 31;
			// 
			// textBoxBrakeCylinderEmergencyMaximumPressure
			// 
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Location = new System.Drawing.Point(176, 40);
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Name = "textBoxBrakeCylinderEmergencyMaximumPressure";
			this.textBoxBrakeCylinderEmergencyMaximumPressure.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakeCylinderEmergencyMaximumPressure.TabIndex = 32;
			// 
			// labelBrakeCylinderEmergencyRate
			// 
			this.labelBrakeCylinderEmergencyRate.Location = new System.Drawing.Point(8, 64);
			this.labelBrakeCylinderEmergencyRate.Name = "labelBrakeCylinderEmergencyRate";
			this.labelBrakeCylinderEmergencyRate.Size = new System.Drawing.Size(160, 16);
			this.labelBrakeCylinderEmergencyRate.TabIndex = 2;
			this.labelBrakeCylinderEmergencyRate.Text = "EmergencyRate:";
			this.labelBrakeCylinderEmergencyRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrakeCylinderReleaseRate
			// 
			this.labelBrakeCylinderReleaseRate.Location = new System.Drawing.Point(8, 88);
			this.labelBrakeCylinderReleaseRate.Name = "labelBrakeCylinderReleaseRate";
			this.labelBrakeCylinderReleaseRate.Size = new System.Drawing.Size(160, 16);
			this.labelBrakeCylinderReleaseRate.TabIndex = 1;
			this.labelBrakeCylinderReleaseRate.Text = "ReleaseRate:";
			this.labelBrakeCylinderReleaseRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakeCylinderEmergencyRate
			// 
			this.textBoxBrakeCylinderEmergencyRate.Location = new System.Drawing.Point(176, 64);
			this.textBoxBrakeCylinderEmergencyRate.Name = "textBoxBrakeCylinderEmergencyRate";
			this.textBoxBrakeCylinderEmergencyRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakeCylinderEmergencyRate.TabIndex = 28;
			// 
			// groupBoxStraightAirPipe
			// 
			this.groupBoxStraightAirPipe.Controls.Add(this.comboBoxStraightAirPipeReleaseRateUnit);
			this.groupBoxStraightAirPipe.Controls.Add(this.comboBoxStraightAirPipeEmergencyRateUnit);
			this.groupBoxStraightAirPipe.Controls.Add(this.comboBoxStraightAirPipeServiceRateUnit);
			this.groupBoxStraightAirPipe.Controls.Add(this.labelStraightAirPipeReleaseRate);
			this.groupBoxStraightAirPipe.Controls.Add(this.textBoxStraightAirPipeReleaseRate);
			this.groupBoxStraightAirPipe.Controls.Add(this.labelStraightAirPipeEmergencyRate);
			this.groupBoxStraightAirPipe.Controls.Add(this.textBoxStraightAirPipeEmergencyRate);
			this.groupBoxStraightAirPipe.Controls.Add(this.labelStraightAirPipeServiceRate);
			this.groupBoxStraightAirPipe.Controls.Add(this.textBoxStraightAirPipeServiceRate);
			this.groupBoxStraightAirPipe.Location = new System.Drawing.Point(8, 440);
			this.groupBoxStraightAirPipe.Name = "groupBoxStraightAirPipe";
			this.groupBoxStraightAirPipe.Size = new System.Drawing.Size(288, 96);
			this.groupBoxStraightAirPipe.TabIndex = 45;
			this.groupBoxStraightAirPipe.TabStop = false;
			this.groupBoxStraightAirPipe.Text = "StraightAirPipe";
			// 
			// comboBoxStraightAirPipeReleaseRateUnit
			// 
			this.comboBoxStraightAirPipeReleaseRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxStraightAirPipeReleaseRateUnit.FormattingEnabled = true;
			this.comboBoxStraightAirPipeReleaseRateUnit.Location = new System.Drawing.Point(232, 64);
			this.comboBoxStraightAirPipeReleaseRateUnit.Name = "comboBoxStraightAirPipeReleaseRateUnit";
			this.comboBoxStraightAirPipeReleaseRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxStraightAirPipeReleaseRateUnit.TabIndex = 64;
			// 
			// comboBoxStraightAirPipeEmergencyRateUnit
			// 
			this.comboBoxStraightAirPipeEmergencyRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxStraightAirPipeEmergencyRateUnit.FormattingEnabled = true;
			this.comboBoxStraightAirPipeEmergencyRateUnit.Location = new System.Drawing.Point(232, 40);
			this.comboBoxStraightAirPipeEmergencyRateUnit.Name = "comboBoxStraightAirPipeEmergencyRateUnit";
			this.comboBoxStraightAirPipeEmergencyRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxStraightAirPipeEmergencyRateUnit.TabIndex = 63;
			// 
			// comboBoxStraightAirPipeServiceRateUnit
			// 
			this.comboBoxStraightAirPipeServiceRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxStraightAirPipeServiceRateUnit.FormattingEnabled = true;
			this.comboBoxStraightAirPipeServiceRateUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxStraightAirPipeServiceRateUnit.Name = "comboBoxStraightAirPipeServiceRateUnit";
			this.comboBoxStraightAirPipeServiceRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxStraightAirPipeServiceRateUnit.TabIndex = 62;
			// 
			// labelStraightAirPipeReleaseRate
			// 
			this.labelStraightAirPipeReleaseRate.Location = new System.Drawing.Point(8, 64);
			this.labelStraightAirPipeReleaseRate.Name = "labelStraightAirPipeReleaseRate";
			this.labelStraightAirPipeReleaseRate.Size = new System.Drawing.Size(160, 16);
			this.labelStraightAirPipeReleaseRate.TabIndex = 61;
			this.labelStraightAirPipeReleaseRate.Text = "ReleaseRate:";
			this.labelStraightAirPipeReleaseRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxStraightAirPipeReleaseRate
			// 
			this.textBoxStraightAirPipeReleaseRate.Location = new System.Drawing.Point(176, 64);
			this.textBoxStraightAirPipeReleaseRate.Name = "textBoxStraightAirPipeReleaseRate";
			this.textBoxStraightAirPipeReleaseRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxStraightAirPipeReleaseRate.TabIndex = 59;
			// 
			// labelStraightAirPipeEmergencyRate
			// 
			this.labelStraightAirPipeEmergencyRate.Location = new System.Drawing.Point(8, 40);
			this.labelStraightAirPipeEmergencyRate.Name = "labelStraightAirPipeEmergencyRate";
			this.labelStraightAirPipeEmergencyRate.Size = new System.Drawing.Size(160, 16);
			this.labelStraightAirPipeEmergencyRate.TabIndex = 58;
			this.labelStraightAirPipeEmergencyRate.Text = "EmergencyRate:";
			this.labelStraightAirPipeEmergencyRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxStraightAirPipeEmergencyRate
			// 
			this.textBoxStraightAirPipeEmergencyRate.Location = new System.Drawing.Point(176, 40);
			this.textBoxStraightAirPipeEmergencyRate.Name = "textBoxStraightAirPipeEmergencyRate";
			this.textBoxStraightAirPipeEmergencyRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxStraightAirPipeEmergencyRate.TabIndex = 56;
			// 
			// labelStraightAirPipeServiceRate
			// 
			this.labelStraightAirPipeServiceRate.Location = new System.Drawing.Point(8, 16);
			this.labelStraightAirPipeServiceRate.Name = "labelStraightAirPipeServiceRate";
			this.labelStraightAirPipeServiceRate.Size = new System.Drawing.Size(160, 16);
			this.labelStraightAirPipeServiceRate.TabIndex = 55;
			this.labelStraightAirPipeServiceRate.Text = "ServiceRate:";
			this.labelStraightAirPipeServiceRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxStraightAirPipeServiceRate
			// 
			this.textBoxStraightAirPipeServiceRate.Location = new System.Drawing.Point(176, 16);
			this.textBoxStraightAirPipeServiceRate.Name = "textBoxStraightAirPipeServiceRate";
			this.textBoxStraightAirPipeServiceRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxStraightAirPipeServiceRate.TabIndex = 53;
			// 
			// groupBoxBrakePipe
			// 
			this.groupBoxBrakePipe.Controls.Add(this.comboBoxBrakePipeEmergencyRateUnit);
			this.groupBoxBrakePipe.Controls.Add(this.comboBoxBrakePipeServiceRateUnit);
			this.groupBoxBrakePipe.Controls.Add(this.comboBoxBrakePipeChargeRateUnit);
			this.groupBoxBrakePipe.Controls.Add(this.comboBoxBrakePipeNormalPressureUnit);
			this.groupBoxBrakePipe.Controls.Add(this.labelBrakePipeEmergencyRate);
			this.groupBoxBrakePipe.Controls.Add(this.textBoxBrakePipeEmergencyRate);
			this.groupBoxBrakePipe.Controls.Add(this.labelBrakePipeServiceRate);
			this.groupBoxBrakePipe.Controls.Add(this.textBoxBrakePipeServiceRate);
			this.groupBoxBrakePipe.Controls.Add(this.labelBrakePipeChargeRate);
			this.groupBoxBrakePipe.Controls.Add(this.textBoxBrakePipeChargeRate);
			this.groupBoxBrakePipe.Controls.Add(this.labelBrakePipeNormalPressure);
			this.groupBoxBrakePipe.Controls.Add(this.textBoxBrakePipeNormalPressure);
			this.groupBoxBrakePipe.Location = new System.Drawing.Point(8, 312);
			this.groupBoxBrakePipe.Name = "groupBoxBrakePipe";
			this.groupBoxBrakePipe.Size = new System.Drawing.Size(288, 120);
			this.groupBoxBrakePipe.TabIndex = 44;
			this.groupBoxBrakePipe.TabStop = false;
			this.groupBoxBrakePipe.Text = "BrakePipe";
			// 
			// comboBoxBrakePipeEmergencyRateUnit
			// 
			this.comboBoxBrakePipeEmergencyRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakePipeEmergencyRateUnit.FormattingEnabled = true;
			this.comboBoxBrakePipeEmergencyRateUnit.Location = new System.Drawing.Point(232, 88);
			this.comboBoxBrakePipeEmergencyRateUnit.Name = "comboBoxBrakePipeEmergencyRateUnit";
			this.comboBoxBrakePipeEmergencyRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakePipeEmergencyRateUnit.TabIndex = 60;
			// 
			// comboBoxBrakePipeServiceRateUnit
			// 
			this.comboBoxBrakePipeServiceRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakePipeServiceRateUnit.FormattingEnabled = true;
			this.comboBoxBrakePipeServiceRateUnit.Location = new System.Drawing.Point(232, 64);
			this.comboBoxBrakePipeServiceRateUnit.Name = "comboBoxBrakePipeServiceRateUnit";
			this.comboBoxBrakePipeServiceRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakePipeServiceRateUnit.TabIndex = 59;
			// 
			// comboBoxBrakePipeChargeRateUnit
			// 
			this.comboBoxBrakePipeChargeRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakePipeChargeRateUnit.FormattingEnabled = true;
			this.comboBoxBrakePipeChargeRateUnit.Location = new System.Drawing.Point(232, 40);
			this.comboBoxBrakePipeChargeRateUnit.Name = "comboBoxBrakePipeChargeRateUnit";
			this.comboBoxBrakePipeChargeRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakePipeChargeRateUnit.TabIndex = 58;
			// 
			// comboBoxBrakePipeNormalPressureUnit
			// 
			this.comboBoxBrakePipeNormalPressureUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakePipeNormalPressureUnit.FormattingEnabled = true;
			this.comboBoxBrakePipeNormalPressureUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxBrakePipeNormalPressureUnit.Name = "comboBoxBrakePipeNormalPressureUnit";
			this.comboBoxBrakePipeNormalPressureUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakePipeNormalPressureUnit.TabIndex = 57;
			// 
			// labelBrakePipeEmergencyRate
			// 
			this.labelBrakePipeEmergencyRate.Location = new System.Drawing.Point(8, 88);
			this.labelBrakePipeEmergencyRate.Name = "labelBrakePipeEmergencyRate";
			this.labelBrakePipeEmergencyRate.Size = new System.Drawing.Size(160, 16);
			this.labelBrakePipeEmergencyRate.TabIndex = 52;
			this.labelBrakePipeEmergencyRate.Text = "EmergencyRate:";
			this.labelBrakePipeEmergencyRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakePipeEmergencyRate
			// 
			this.textBoxBrakePipeEmergencyRate.Location = new System.Drawing.Point(176, 88);
			this.textBoxBrakePipeEmergencyRate.Name = "textBoxBrakePipeEmergencyRate";
			this.textBoxBrakePipeEmergencyRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakePipeEmergencyRate.TabIndex = 50;
			// 
			// labelBrakePipeServiceRate
			// 
			this.labelBrakePipeServiceRate.Location = new System.Drawing.Point(8, 64);
			this.labelBrakePipeServiceRate.Name = "labelBrakePipeServiceRate";
			this.labelBrakePipeServiceRate.Size = new System.Drawing.Size(160, 16);
			this.labelBrakePipeServiceRate.TabIndex = 49;
			this.labelBrakePipeServiceRate.Text = "ServiceRate:";
			this.labelBrakePipeServiceRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakePipeServiceRate
			// 
			this.textBoxBrakePipeServiceRate.Location = new System.Drawing.Point(176, 64);
			this.textBoxBrakePipeServiceRate.Name = "textBoxBrakePipeServiceRate";
			this.textBoxBrakePipeServiceRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakePipeServiceRate.TabIndex = 47;
			// 
			// labelBrakePipeChargeRate
			// 
			this.labelBrakePipeChargeRate.Location = new System.Drawing.Point(8, 40);
			this.labelBrakePipeChargeRate.Name = "labelBrakePipeChargeRate";
			this.labelBrakePipeChargeRate.Size = new System.Drawing.Size(160, 16);
			this.labelBrakePipeChargeRate.TabIndex = 46;
			this.labelBrakePipeChargeRate.Text = "ChargeRate:";
			this.labelBrakePipeChargeRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakePipeChargeRate
			// 
			this.textBoxBrakePipeChargeRate.Location = new System.Drawing.Point(176, 40);
			this.textBoxBrakePipeChargeRate.Name = "textBoxBrakePipeChargeRate";
			this.textBoxBrakePipeChargeRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakePipeChargeRate.TabIndex = 44;
			// 
			// labelBrakePipeNormalPressure
			// 
			this.labelBrakePipeNormalPressure.Location = new System.Drawing.Point(8, 16);
			this.labelBrakePipeNormalPressure.Name = "labelBrakePipeNormalPressure";
			this.labelBrakePipeNormalPressure.Size = new System.Drawing.Size(160, 16);
			this.labelBrakePipeNormalPressure.TabIndex = 37;
			this.labelBrakePipeNormalPressure.Text = "NormalPressure:";
			this.labelBrakePipeNormalPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxBrakePipeNormalPressure
			// 
			this.textBoxBrakePipeNormalPressure.Location = new System.Drawing.Point(176, 16);
			this.textBoxBrakePipeNormalPressure.Name = "textBoxBrakePipeNormalPressure";
			this.textBoxBrakePipeNormalPressure.Size = new System.Drawing.Size(48, 19);
			this.textBoxBrakePipeNormalPressure.TabIndex = 38;
			// 
			// groupBoxEqualizingReservoir
			// 
			this.groupBoxEqualizingReservoir.Controls.Add(this.comboBoxEqualizingReservoirEmergencyRateUnit);
			this.groupBoxEqualizingReservoir.Controls.Add(this.comboBoxEqualizingReservoirServiceRateUnit);
			this.groupBoxEqualizingReservoir.Controls.Add(this.comboBoxEqualizingReservoirChargeRateUnit);
			this.groupBoxEqualizingReservoir.Controls.Add(this.labelEqualizingReservoirEmergencyRate);
			this.groupBoxEqualizingReservoir.Controls.Add(this.textBoxEqualizingReservoirEmergencyRate);
			this.groupBoxEqualizingReservoir.Controls.Add(this.labelEqualizingReservoirServiceRate);
			this.groupBoxEqualizingReservoir.Controls.Add(this.textBoxEqualizingReservoirServiceRate);
			this.groupBoxEqualizingReservoir.Controls.Add(this.labelEqualizingReservoirChargeRate);
			this.groupBoxEqualizingReservoir.Controls.Add(this.textBoxEqualizingReservoirChargeRate);
			this.groupBoxEqualizingReservoir.Location = new System.Drawing.Point(8, 208);
			this.groupBoxEqualizingReservoir.Name = "groupBoxEqualizingReservoir";
			this.groupBoxEqualizingReservoir.Size = new System.Drawing.Size(288, 96);
			this.groupBoxEqualizingReservoir.TabIndex = 43;
			this.groupBoxEqualizingReservoir.TabStop = false;
			this.groupBoxEqualizingReservoir.Text = "EqualizingReservoir";
			// 
			// comboBoxEqualizingReservoirEmergencyRateUnit
			// 
			this.comboBoxEqualizingReservoirEmergencyRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxEqualizingReservoirEmergencyRateUnit.FormattingEnabled = true;
			this.comboBoxEqualizingReservoirEmergencyRateUnit.Location = new System.Drawing.Point(232, 64);
			this.comboBoxEqualizingReservoirEmergencyRateUnit.Name = "comboBoxEqualizingReservoirEmergencyRateUnit";
			this.comboBoxEqualizingReservoirEmergencyRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxEqualizingReservoirEmergencyRateUnit.TabIndex = 59;
			// 
			// comboBoxEqualizingReservoirServiceRateUnit
			// 
			this.comboBoxEqualizingReservoirServiceRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxEqualizingReservoirServiceRateUnit.FormattingEnabled = true;
			this.comboBoxEqualizingReservoirServiceRateUnit.Location = new System.Drawing.Point(232, 40);
			this.comboBoxEqualizingReservoirServiceRateUnit.Name = "comboBoxEqualizingReservoirServiceRateUnit";
			this.comboBoxEqualizingReservoirServiceRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxEqualizingReservoirServiceRateUnit.TabIndex = 58;
			// 
			// comboBoxEqualizingReservoirChargeRateUnit
			// 
			this.comboBoxEqualizingReservoirChargeRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxEqualizingReservoirChargeRateUnit.FormattingEnabled = true;
			this.comboBoxEqualizingReservoirChargeRateUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxEqualizingReservoirChargeRateUnit.Name = "comboBoxEqualizingReservoirChargeRateUnit";
			this.comboBoxEqualizingReservoirChargeRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxEqualizingReservoirChargeRateUnit.TabIndex = 57;
			// 
			// labelEqualizingReservoirEmergencyRate
			// 
			this.labelEqualizingReservoirEmergencyRate.Location = new System.Drawing.Point(8, 64);
			this.labelEqualizingReservoirEmergencyRate.Name = "labelEqualizingReservoirEmergencyRate";
			this.labelEqualizingReservoirEmergencyRate.Size = new System.Drawing.Size(160, 16);
			this.labelEqualizingReservoirEmergencyRate.TabIndex = 43;
			this.labelEqualizingReservoirEmergencyRate.Text = "EmergencyRate:";
			this.labelEqualizingReservoirEmergencyRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxEqualizingReservoirEmergencyRate
			// 
			this.textBoxEqualizingReservoirEmergencyRate.Location = new System.Drawing.Point(176, 64);
			this.textBoxEqualizingReservoirEmergencyRate.Name = "textBoxEqualizingReservoirEmergencyRate";
			this.textBoxEqualizingReservoirEmergencyRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxEqualizingReservoirEmergencyRate.TabIndex = 41;
			// 
			// labelEqualizingReservoirServiceRate
			// 
			this.labelEqualizingReservoirServiceRate.Location = new System.Drawing.Point(8, 40);
			this.labelEqualizingReservoirServiceRate.Name = "labelEqualizingReservoirServiceRate";
			this.labelEqualizingReservoirServiceRate.Size = new System.Drawing.Size(160, 16);
			this.labelEqualizingReservoirServiceRate.TabIndex = 40;
			this.labelEqualizingReservoirServiceRate.Text = "ServiceRate:";
			this.labelEqualizingReservoirServiceRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxEqualizingReservoirServiceRate
			// 
			this.textBoxEqualizingReservoirServiceRate.Location = new System.Drawing.Point(176, 40);
			this.textBoxEqualizingReservoirServiceRate.Name = "textBoxEqualizingReservoirServiceRate";
			this.textBoxEqualizingReservoirServiceRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxEqualizingReservoirServiceRate.TabIndex = 38;
			// 
			// labelEqualizingReservoirChargeRate
			// 
			this.labelEqualizingReservoirChargeRate.Location = new System.Drawing.Point(8, 16);
			this.labelEqualizingReservoirChargeRate.Name = "labelEqualizingReservoirChargeRate";
			this.labelEqualizingReservoirChargeRate.Size = new System.Drawing.Size(160, 16);
			this.labelEqualizingReservoirChargeRate.TabIndex = 37;
			this.labelEqualizingReservoirChargeRate.Text = "ChargeRate:";
			this.labelEqualizingReservoirChargeRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxEqualizingReservoirChargeRate
			// 
			this.textBoxEqualizingReservoirChargeRate.Location = new System.Drawing.Point(176, 16);
			this.textBoxEqualizingReservoirChargeRate.Name = "textBoxEqualizingReservoirChargeRate";
			this.textBoxEqualizingReservoirChargeRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxEqualizingReservoirChargeRate.TabIndex = 35;
			// 
			// groupBoxAuxiliaryReservoir
			// 
			this.groupBoxAuxiliaryReservoir.Controls.Add(this.comboBoxAuxiliaryReservoirChargeRateUnit);
			this.groupBoxAuxiliaryReservoir.Controls.Add(this.labelAuxiliaryReservoirChargeRate);
			this.groupBoxAuxiliaryReservoir.Controls.Add(this.textBoxAuxiliaryReservoirChargeRate);
			this.groupBoxAuxiliaryReservoir.Location = new System.Drawing.Point(8, 152);
			this.groupBoxAuxiliaryReservoir.Name = "groupBoxAuxiliaryReservoir";
			this.groupBoxAuxiliaryReservoir.Size = new System.Drawing.Size(288, 48);
			this.groupBoxAuxiliaryReservoir.TabIndex = 42;
			this.groupBoxAuxiliaryReservoir.TabStop = false;
			this.groupBoxAuxiliaryReservoir.Text = "AuxiliaryReservoir";
			// 
			// comboBoxAuxiliaryReservoirChargeRateUnit
			// 
			this.comboBoxAuxiliaryReservoirChargeRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAuxiliaryReservoirChargeRateUnit.FormattingEnabled = true;
			this.comboBoxAuxiliaryReservoirChargeRateUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxAuxiliaryReservoirChargeRateUnit.Name = "comboBoxAuxiliaryReservoirChargeRateUnit";
			this.comboBoxAuxiliaryReservoirChargeRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxAuxiliaryReservoirChargeRateUnit.TabIndex = 57;
			// 
			// labelAuxiliaryReservoirChargeRate
			// 
			this.labelAuxiliaryReservoirChargeRate.Location = new System.Drawing.Point(8, 16);
			this.labelAuxiliaryReservoirChargeRate.Name = "labelAuxiliaryReservoirChargeRate";
			this.labelAuxiliaryReservoirChargeRate.Size = new System.Drawing.Size(160, 16);
			this.labelAuxiliaryReservoirChargeRate.TabIndex = 34;
			this.labelAuxiliaryReservoirChargeRate.Text = "ChargeRate:";
			this.labelAuxiliaryReservoirChargeRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxAuxiliaryReservoirChargeRate
			// 
			this.textBoxAuxiliaryReservoirChargeRate.Location = new System.Drawing.Point(176, 16);
			this.textBoxAuxiliaryReservoirChargeRate.Name = "textBoxAuxiliaryReservoirChargeRate";
			this.textBoxAuxiliaryReservoirChargeRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxAuxiliaryReservoirChargeRate.TabIndex = 32;
			// 
			// groupBoxMainReservoir
			// 
			this.groupBoxMainReservoir.Controls.Add(this.comboBoxMainReservoirMaximumPressureUnit);
			this.groupBoxMainReservoir.Controls.Add(this.comboBoxMainReservoirMinimumPressureUnit);
			this.groupBoxMainReservoir.Controls.Add(this.labelMainReservoirMaximumPressure);
			this.groupBoxMainReservoir.Controls.Add(this.labelMainReservoirMinimumPressure);
			this.groupBoxMainReservoir.Controls.Add(this.textBoxMainReservoirMaximumPressure);
			this.groupBoxMainReservoir.Controls.Add(this.textBoxMainReservoirMinimumPressure);
			this.groupBoxMainReservoir.Location = new System.Drawing.Point(8, 72);
			this.groupBoxMainReservoir.Name = "groupBoxMainReservoir";
			this.groupBoxMainReservoir.Size = new System.Drawing.Size(288, 72);
			this.groupBoxMainReservoir.TabIndex = 41;
			this.groupBoxMainReservoir.TabStop = false;
			this.groupBoxMainReservoir.Text = "MainReservoir";
			// 
			// comboBoxMainReservoirMaximumPressureUnit
			// 
			this.comboBoxMainReservoirMaximumPressureUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMainReservoirMaximumPressureUnit.FormattingEnabled = true;
			this.comboBoxMainReservoirMaximumPressureUnit.Location = new System.Drawing.Point(232, 40);
			this.comboBoxMainReservoirMaximumPressureUnit.Name = "comboBoxMainReservoirMaximumPressureUnit";
			this.comboBoxMainReservoirMaximumPressureUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMainReservoirMaximumPressureUnit.TabIndex = 57;
			// 
			// comboBoxMainReservoirMinimumPressureUnit
			// 
			this.comboBoxMainReservoirMinimumPressureUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMainReservoirMinimumPressureUnit.FormattingEnabled = true;
			this.comboBoxMainReservoirMinimumPressureUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxMainReservoirMinimumPressureUnit.Name = "comboBoxMainReservoirMinimumPressureUnit";
			this.comboBoxMainReservoirMinimumPressureUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMainReservoirMinimumPressureUnit.TabIndex = 56;
			// 
			// labelMainReservoirMaximumPressure
			// 
			this.labelMainReservoirMaximumPressure.Location = new System.Drawing.Point(8, 40);
			this.labelMainReservoirMaximumPressure.Name = "labelMainReservoirMaximumPressure";
			this.labelMainReservoirMaximumPressure.Size = new System.Drawing.Size(160, 16);
			this.labelMainReservoirMaximumPressure.TabIndex = 11;
			this.labelMainReservoirMaximumPressure.Text = "MaximumPressure:";
			this.labelMainReservoirMaximumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMainReservoirMinimumPressure
			// 
			this.labelMainReservoirMinimumPressure.Location = new System.Drawing.Point(8, 16);
			this.labelMainReservoirMinimumPressure.Name = "labelMainReservoirMinimumPressure";
			this.labelMainReservoirMinimumPressure.Size = new System.Drawing.Size(160, 16);
			this.labelMainReservoirMinimumPressure.TabIndex = 10;
			this.labelMainReservoirMinimumPressure.Text = "MinimumPressure:";
			this.labelMainReservoirMinimumPressure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMainReservoirMaximumPressure
			// 
			this.textBoxMainReservoirMaximumPressure.Location = new System.Drawing.Point(176, 40);
			this.textBoxMainReservoirMaximumPressure.Name = "textBoxMainReservoirMaximumPressure";
			this.textBoxMainReservoirMaximumPressure.Size = new System.Drawing.Size(48, 19);
			this.textBoxMainReservoirMaximumPressure.TabIndex = 34;
			// 
			// textBoxMainReservoirMinimumPressure
			// 
			this.textBoxMainReservoirMinimumPressure.Location = new System.Drawing.Point(176, 16);
			this.textBoxMainReservoirMinimumPressure.Name = "textBoxMainReservoirMinimumPressure";
			this.textBoxMainReservoirMinimumPressure.Size = new System.Drawing.Size(48, 19);
			this.textBoxMainReservoirMinimumPressure.TabIndex = 33;
			// 
			// groupBoxCompressor
			// 
			this.groupBoxCompressor.Controls.Add(this.comboBoxCompressorRateUnit);
			this.groupBoxCompressor.Controls.Add(this.labelCompressorRate);
			this.groupBoxCompressor.Controls.Add(this.textBoxCompressorRate);
			this.groupBoxCompressor.Location = new System.Drawing.Point(8, 16);
			this.groupBoxCompressor.Name = "groupBoxCompressor";
			this.groupBoxCompressor.Size = new System.Drawing.Size(288, 48);
			this.groupBoxCompressor.TabIndex = 40;
			this.groupBoxCompressor.TabStop = false;
			this.groupBoxCompressor.Text = "Compressor";
			// 
			// comboBoxCompressorRateUnit
			// 
			this.comboBoxCompressorRateUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCompressorRateUnit.FormattingEnabled = true;
			this.comboBoxCompressorRateUnit.Location = new System.Drawing.Point(232, 16);
			this.comboBoxCompressorRateUnit.Name = "comboBoxCompressorRateUnit";
			this.comboBoxCompressorRateUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxCompressorRateUnit.TabIndex = 57;
			// 
			// labelCompressorRate
			// 
			this.labelCompressorRate.Location = new System.Drawing.Point(8, 16);
			this.labelCompressorRate.Name = "labelCompressorRate";
			this.labelCompressorRate.Size = new System.Drawing.Size(160, 16);
			this.labelCompressorRate.TabIndex = 31;
			this.labelCompressorRate.Text = "Rate:";
			this.labelCompressorRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxCompressorRate
			// 
			this.textBoxCompressorRate.Location = new System.Drawing.Point(176, 16);
			this.textBoxCompressorRate.Name = "textBoxCompressorRate";
			this.textBoxCompressorRate.Size = new System.Drawing.Size(48, 19);
			this.textBoxCompressorRate.TabIndex = 20;
			// 
			// groupBoxBrake
			// 
			this.groupBoxBrake.Controls.Add(this.comboBoxBrakeControlSpeedUnit);
			this.groupBoxBrake.Controls.Add(this.textBoxBrakeControlSpeed);
			this.groupBoxBrake.Controls.Add(this.comboBoxBrakeControlSystem);
			this.groupBoxBrake.Controls.Add(this.comboBoxLocoBrakeType);
			this.groupBoxBrake.Controls.Add(this.comboBoxBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelLocoBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelBrakeType);
			this.groupBoxBrake.Controls.Add(this.labelBrakeControlSpeed);
			this.groupBoxBrake.Controls.Add(this.labelBrakeControlSystem);
			this.groupBoxBrake.Location = new System.Drawing.Point(320, 136);
			this.groupBoxBrake.Name = "groupBoxBrake";
			this.groupBoxBrake.Size = new System.Drawing.Size(464, 120);
			this.groupBoxBrake.TabIndex = 4;
			this.groupBoxBrake.TabStop = false;
			this.groupBoxBrake.Text = "Brake";
			// 
			// comboBoxBrakeControlSpeedUnit
			// 
			this.comboBoxBrakeControlSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBrakeControlSpeedUnit.FormattingEnabled = true;
			this.comboBoxBrakeControlSpeedUnit.Location = new System.Drawing.Point(408, 88);
			this.comboBoxBrakeControlSpeedUnit.Name = "comboBoxBrakeControlSpeedUnit";
			this.comboBoxBrakeControlSpeedUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxBrakeControlSpeedUnit.TabIndex = 58;
			// 
			// textBoxBrakeControlSpeed
			// 
			this.textBoxBrakeControlSpeed.Location = new System.Drawing.Point(136, 88);
			this.textBoxBrakeControlSpeed.Name = "textBoxBrakeControlSpeed";
			this.textBoxBrakeControlSpeed.Size = new System.Drawing.Size(264, 19);
			this.textBoxBrakeControlSpeed.TabIndex = 31;
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
			// groupBoxJerk
			// 
			this.groupBoxJerk.Controls.Add(this.buttonJerkBrakeSet);
			this.groupBoxJerk.Controls.Add(this.buttonJerkPowerSet);
			this.groupBoxJerk.Controls.Add(this.labelJerkPower);
			this.groupBoxJerk.Controls.Add(this.labelJerkBrake);
			this.groupBoxJerk.Location = new System.Drawing.Point(320, 264);
			this.groupBoxJerk.Name = "groupBoxJerk";
			this.groupBoxJerk.Size = new System.Drawing.Size(112, 72);
			this.groupBoxJerk.TabIndex = 3;
			this.groupBoxJerk.TabStop = false;
			this.groupBoxJerk.Text = "Jerk";
			// 
			// buttonJerkBrakeSet
			// 
			this.buttonJerkBrakeSet.Location = new System.Drawing.Point(56, 40);
			this.buttonJerkBrakeSet.Name = "buttonJerkBrakeSet";
			this.buttonJerkBrakeSet.Size = new System.Drawing.Size(48, 19);
			this.buttonJerkBrakeSet.TabIndex = 35;
			this.buttonJerkBrakeSet.Text = "Set...";
			this.buttonJerkBrakeSet.UseVisualStyleBackColor = true;
			this.buttonJerkBrakeSet.Click += new System.EventHandler(this.ButtonJerkBrakeSet_Click);
			// 
			// buttonJerkPowerSet
			// 
			this.buttonJerkPowerSet.Location = new System.Drawing.Point(56, 16);
			this.buttonJerkPowerSet.Name = "buttonJerkPowerSet";
			this.buttonJerkPowerSet.Size = new System.Drawing.Size(48, 19);
			this.buttonJerkPowerSet.TabIndex = 34;
			this.buttonJerkPowerSet.Text = "Set...";
			this.buttonJerkPowerSet.UseVisualStyleBackColor = true;
			this.buttonJerkPowerSet.Click += new System.EventHandler(this.ButtonJerkPowerSet_Click);
			// 
			// labelJerkPower
			// 
			this.labelJerkPower.Location = new System.Drawing.Point(8, 16);
			this.labelJerkPower.Name = "labelJerkPower";
			this.labelJerkPower.Size = new System.Drawing.Size(40, 16);
			this.labelJerkPower.TabIndex = 6;
			this.labelJerkPower.Text = "Power:";
			this.labelJerkPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelJerkBrake
			// 
			this.labelJerkBrake.Location = new System.Drawing.Point(8, 40);
			this.labelJerkBrake.Name = "labelJerkBrake";
			this.labelJerkBrake.Size = new System.Drawing.Size(40, 16);
			this.labelJerkBrake.TabIndex = 4;
			this.labelJerkBrake.Text = "Brake:";
			this.labelJerkBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxPerformance
			// 
			this.groupBoxPerformance.Controls.Add(this.comboBoxDecelerationUnit);
			this.groupBoxPerformance.Controls.Add(this.textBoxAerodynamicDragCoefficient);
			this.groupBoxPerformance.Controls.Add(this.textBoxCoefficientOfRollingResistance);
			this.groupBoxPerformance.Controls.Add(this.textBoxCoefficientOfStaticFriction);
			this.groupBoxPerformance.Controls.Add(this.textBoxDeceleration);
			this.groupBoxPerformance.Controls.Add(this.labelAerodynamicDragCoefficient);
			this.groupBoxPerformance.Controls.Add(this.labelCoefficientOfStaticFriction);
			this.groupBoxPerformance.Controls.Add(this.labelDeceleration);
			this.groupBoxPerformance.Controls.Add(this.labelCoefficientOfRollingResistance);
			this.groupBoxPerformance.Location = new System.Drawing.Point(320, 8);
			this.groupBoxPerformance.Name = "groupBoxPerformance";
			this.groupBoxPerformance.Size = new System.Drawing.Size(432, 120);
			this.groupBoxPerformance.TabIndex = 1;
			this.groupBoxPerformance.TabStop = false;
			this.groupBoxPerformance.Text = "Performance";
			// 
			// comboBoxDecelerationUnit
			// 
			this.comboBoxDecelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDecelerationUnit.FormattingEnabled = true;
			this.comboBoxDecelerationUnit.Location = new System.Drawing.Point(376, 16);
			this.comboBoxDecelerationUnit.Name = "comboBoxDecelerationUnit";
			this.comboBoxDecelerationUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxDecelerationUnit.TabIndex = 58;
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
			// groupBoxDelay
			// 
			this.groupBoxDelay.Controls.Add(this.buttonDelayLocoBrakeSet);
			this.groupBoxDelay.Controls.Add(this.buttonDelayBrakeSet);
			this.groupBoxDelay.Controls.Add(this.buttonDelayPowerSet);
			this.groupBoxDelay.Controls.Add(this.labelDelayLocoBrake);
			this.groupBoxDelay.Controls.Add(this.labelDelayBrake);
			this.groupBoxDelay.Controls.Add(this.labelDelayPower);
			this.groupBoxDelay.Location = new System.Drawing.Point(320, 344);
			this.groupBoxDelay.Name = "groupBoxDelay";
			this.groupBoxDelay.Size = new System.Drawing.Size(152, 96);
			this.groupBoxDelay.TabIndex = 2;
			this.groupBoxDelay.TabStop = false;
			this.groupBoxDelay.Text = "Delay";
			// 
			// buttonDelayLocoBrakeSet
			// 
			this.buttonDelayLocoBrakeSet.Location = new System.Drawing.Point(96, 64);
			this.buttonDelayLocoBrakeSet.Name = "buttonDelayLocoBrakeSet";
			this.buttonDelayLocoBrakeSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDelayLocoBrakeSet.TabIndex = 37;
			this.buttonDelayLocoBrakeSet.Text = "Set...";
			this.buttonDelayLocoBrakeSet.UseVisualStyleBackColor = true;
			this.buttonDelayLocoBrakeSet.Click += new System.EventHandler(this.ButtonDelayLocoBrakeSet_Click);
			// 
			// buttonDelayBrakeSet
			// 
			this.buttonDelayBrakeSet.Location = new System.Drawing.Point(96, 40);
			this.buttonDelayBrakeSet.Name = "buttonDelayBrakeSet";
			this.buttonDelayBrakeSet.Size = new System.Drawing.Size(48, 19);
			this.buttonDelayBrakeSet.TabIndex = 35;
			this.buttonDelayBrakeSet.Text = "Set...";
			this.buttonDelayBrakeSet.UseVisualStyleBackColor = true;
			this.buttonDelayBrakeSet.Click += new System.EventHandler(this.ButtonDelayBrakeSet_Click);
			// 
			// buttonDelayPowerSet
			// 
			this.buttonDelayPowerSet.Location = new System.Drawing.Point(96, 16);
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
			this.labelDelayLocoBrake.Size = new System.Drawing.Size(80, 16);
			this.labelDelayLocoBrake.TabIndex = 6;
			this.labelDelayLocoBrake.Text = "LocoBrake:";
			this.labelDelayLocoBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDelayBrake
			// 
			this.labelDelayBrake.Location = new System.Drawing.Point(8, 40);
			this.labelDelayBrake.Name = "labelDelayBrake";
			this.labelDelayBrake.Size = new System.Drawing.Size(80, 16);
			this.labelDelayBrake.TabIndex = 4;
			this.labelDelayBrake.Text = "Brake:";
			this.labelDelayBrake.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDelayPower
			// 
			this.labelDelayPower.Location = new System.Drawing.Point(8, 16);
			this.labelDelayPower.Name = "labelDelayPower";
			this.labelDelayPower.Size = new System.Drawing.Size(80, 16);
			this.labelDelayPower.TabIndex = 2;
			this.labelDelayPower.Text = "Power:";
			this.labelDelayPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageTrain
			// 
			this.tabPageTrain.Controls.Add(this.groupBoxTrainGeneral);
			this.tabPageTrain.Controls.Add(this.groupBoxDevice);
			this.tabPageTrain.Controls.Add(this.groupBoxHandle);
			this.tabPageTrain.Location = new System.Drawing.Point(4, 22);
			this.tabPageTrain.Name = "tabPageTrain";
			this.tabPageTrain.Size = new System.Drawing.Size(792, 686);
			this.tabPageTrain.TabIndex = 3;
			this.tabPageTrain.Text = "Train settings";
			this.tabPageTrain.UseVisualStyleBackColor = true;
			// 
			// groupBoxTrainGeneral
			// 
			this.groupBoxTrainGeneral.Controls.Add(this.comboBoxInitialDriverCar);
			this.groupBoxTrainGeneral.Controls.Add(this.labelInitialDriverCar);
			this.groupBoxTrainGeneral.Location = new System.Drawing.Point(8, 8);
			this.groupBoxTrainGeneral.Name = "groupBoxTrainGeneral";
			this.groupBoxTrainGeneral.Size = new System.Drawing.Size(424, 48);
			this.groupBoxTrainGeneral.TabIndex = 3;
			this.groupBoxTrainGeneral.TabStop = false;
			this.groupBoxTrainGeneral.Text = "General";
			// 
			// comboBoxInitialDriverCar
			// 
			this.comboBoxInitialDriverCar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxInitialDriverCar.FormattingEnabled = true;
			this.comboBoxInitialDriverCar.Location = new System.Drawing.Point(192, 16);
			this.comboBoxInitialDriverCar.Name = "comboBoxInitialDriverCar";
			this.comboBoxInitialDriverCar.Size = new System.Drawing.Size(216, 20);
			this.comboBoxInitialDriverCar.TabIndex = 3;
			// 
			// labelInitialDriverCar
			// 
			this.labelInitialDriverCar.Location = new System.Drawing.Point(8, 16);
			this.labelInitialDriverCar.Name = "labelInitialDriverCar";
			this.labelInitialDriverCar.Size = new System.Drawing.Size(176, 16);
			this.labelInitialDriverCar.TabIndex = 2;
			this.labelInitialDriverCar.Text = "InitialDriverCar:";
			this.labelInitialDriverCar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxDevice
			// 
			this.groupBoxDevice.Controls.Add(this.comboBoxDoorCloseMode);
			this.groupBoxDevice.Controls.Add(this.comboBoxDoorOpenMode);
			this.groupBoxDevice.Controls.Add(this.comboBoxPassAlarm);
			this.groupBoxDevice.Controls.Add(this.comboBoxAtc);
			this.groupBoxDevice.Controls.Add(this.comboBoxAts);
			this.groupBoxDevice.Controls.Add(this.checkBoxHoldBrake);
			this.groupBoxDevice.Controls.Add(this.checkBoxEb);
			this.groupBoxDevice.Controls.Add(this.checkBoxConstSpeed);
			this.groupBoxDevice.Controls.Add(this.labelDoorCloseMode);
			this.groupBoxDevice.Controls.Add(this.labelDoorOpenMode);
			this.groupBoxDevice.Controls.Add(this.labelPassAlarm);
			this.groupBoxDevice.Controls.Add(this.labelHoldBrake);
			this.groupBoxDevice.Controls.Add(this.labelConstSpeed);
			this.groupBoxDevice.Controls.Add(this.labelEb);
			this.groupBoxDevice.Controls.Add(this.labelAtc);
			this.groupBoxDevice.Controls.Add(this.labelAts);
			this.groupBoxDevice.Location = new System.Drawing.Point(440, 8);
			this.groupBoxDevice.Name = "groupBoxDevice";
			this.groupBoxDevice.Size = new System.Drawing.Size(312, 216);
			this.groupBoxDevice.TabIndex = 2;
			this.groupBoxDevice.TabStop = false;
			this.groupBoxDevice.Text = "Device";
			// 
			// comboBoxDoorCloseMode
			// 
			this.comboBoxDoorCloseMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDoorCloseMode.FormattingEnabled = true;
			this.comboBoxDoorCloseMode.Items.AddRange(new object[] {
            "Semi-automatic",
            "Automatic",
            "Manual"});
			this.comboBoxDoorCloseMode.Location = new System.Drawing.Point(128, 184);
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
			this.comboBoxDoorOpenMode.Location = new System.Drawing.Point(128, 160);
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
			this.comboBoxPassAlarm.Location = new System.Drawing.Point(128, 136);
			this.comboBoxPassAlarm.Name = "comboBoxPassAlarm";
			this.comboBoxPassAlarm.Size = new System.Drawing.Size(168, 20);
			this.comboBoxPassAlarm.TabIndex = 17;
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
			// labelDoorCloseMode
			// 
			this.labelDoorCloseMode.Location = new System.Drawing.Point(8, 184);
			this.labelDoorCloseMode.Name = "labelDoorCloseMode";
			this.labelDoorCloseMode.Size = new System.Drawing.Size(112, 16);
			this.labelDoorCloseMode.TabIndex = 8;
			this.labelDoorCloseMode.Text = "DoorCloseMode:";
			this.labelDoorCloseMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDoorOpenMode
			// 
			this.labelDoorOpenMode.Location = new System.Drawing.Point(8, 160);
			this.labelDoorOpenMode.Name = "labelDoorOpenMode";
			this.labelDoorOpenMode.Size = new System.Drawing.Size(112, 16);
			this.labelDoorOpenMode.TabIndex = 7;
			this.labelDoorOpenMode.Text = "DoorOpenMode:";
			this.labelDoorOpenMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPassAlarm
			// 
			this.labelPassAlarm.Location = new System.Drawing.Point(8, 136);
			this.labelPassAlarm.Name = "labelPassAlarm";
			this.labelPassAlarm.Size = new System.Drawing.Size(112, 16);
			this.labelPassAlarm.TabIndex = 6;
			this.labelPassAlarm.Text = "PassAlarm:";
			this.labelPassAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			this.groupBoxHandle.Location = new System.Drawing.Point(8, 64);
			this.groupBoxHandle.Name = "groupBoxHandle";
			this.groupBoxHandle.Size = new System.Drawing.Size(424, 240);
			this.groupBoxHandle.TabIndex = 0;
			this.groupBoxHandle.TabStop = false;
			this.groupBoxHandle.Text = "Handle";
			// 
			// numericUpDownLocoBrakeNotches
			// 
			this.numericUpDownLocoBrakeNotches.Location = new System.Drawing.Point(192, 208);
			this.numericUpDownLocoBrakeNotches.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.numericUpDownDriverBrakeNotches.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.numericUpDownDriverPowerNotches.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.numericUpDownPowerNotchReduceSteps.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.numericUpDownBrakeNotches.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			this.numericUpDownPowerNotches.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
			// tabControlEditor
			// 
			this.tabControlEditor.AllowDrop = true;
			this.tabControlEditor.Controls.Add(this.tabPageStatus);
			this.tabControlEditor.Controls.Add(this.tabPageTrain);
			this.tabControlEditor.Controls.Add(this.tabPageCar1);
			this.tabControlEditor.Controls.Add(this.tabPageCar2);
			this.tabControlEditor.Controls.Add(this.tabPageAccel);
			this.tabControlEditor.Controls.Add(this.tabPageMotor);
			this.tabControlEditor.Controls.Add(this.tabPagePanel);
			this.tabControlEditor.Controls.Add(this.tabPageCoupler);
			this.tabControlEditor.Controls.Add(this.tabPageSound);
			this.tabControlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlEditor.Location = new System.Drawing.Point(200, 24);
			this.tabControlEditor.Name = "tabControlEditor";
			this.tabControlEditor.SelectedIndex = 0;
			this.tabControlEditor.Size = new System.Drawing.Size(800, 744);
			this.tabControlEditor.TabIndex = 9;
			// 
			// tabPageCar2
			// 
			this.tabPageCar2.Controls.Add(this.groupBoxPressure);
			this.tabPageCar2.Controls.Add(this.groupBoxBrake);
			this.tabPageCar2.Controls.Add(this.groupBoxDelay);
			this.tabPageCar2.Controls.Add(this.groupBoxPerformance);
			this.tabPageCar2.Controls.Add(this.groupBoxJerk);
			this.tabPageCar2.Location = new System.Drawing.Point(4, 22);
			this.tabPageCar2.Name = "tabPageCar2";
			this.tabPageCar2.Size = new System.Drawing.Size(792, 686);
			this.tabPageCar2.TabIndex = 8;
			this.tabPageCar2.Text = "Car settings (2)";
			this.tabPageCar2.UseVisualStyleBackColor = true;
			// 
			// groupBoxUnit
			// 
			this.groupBoxUnit.Controls.Add(this.comboBoxMotorXUnit);
			this.groupBoxUnit.Controls.Add(this.labelMotorXUnit);
			this.groupBoxUnit.Location = new System.Drawing.Point(8, 64);
			this.groupBoxUnit.Name = "groupBoxUnit";
			this.groupBoxUnit.Size = new System.Drawing.Size(200, 48);
			this.groupBoxUnit.TabIndex = 4;
			this.groupBoxUnit.TabStop = false;
			this.groupBoxUnit.Text = "Unit setting";
			// 
			// comboBoxMotorXUnit
			// 
			this.comboBoxMotorXUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMotorXUnit.FormattingEnabled = true;
			this.comboBoxMotorXUnit.Location = new System.Drawing.Point(112, 16);
			this.comboBoxMotorXUnit.Name = "comboBoxMotorXUnit";
			this.comboBoxMotorXUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMotorXUnit.TabIndex = 66;
			// 
			// labelMotorXUnit
			// 
			this.labelMotorXUnit.Location = new System.Drawing.Point(8, 16);
			this.labelMotorXUnit.Name = "labelMotorXUnit";
			this.labelMotorXUnit.Size = new System.Drawing.Size(96, 16);
			this.labelMotorXUnit.TabIndex = 64;
			this.labelMotorXUnit.Text = "X unit:";
			this.labelMotorXUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxMotorAccelUnit
			// 
			this.comboBoxMotorAccelUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMotorAccelUnit.FormattingEnabled = true;
			this.comboBoxMotorAccelUnit.Location = new System.Drawing.Point(128, 64);
			this.comboBoxMotorAccelUnit.Name = "comboBoxMotorAccelUnit";
			this.comboBoxMotorAccelUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMotorAccelUnit.TabIndex = 67;
			// 
			// FormEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1000, 768);
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
			this.Resize += new System.EventHandler(this.FormEditor_Resize);
			this.panelCars.ResumeLayout(false);
			this.panelCarsNavi.ResumeLayout(false);
			this.menuStripMenu.ResumeLayout(false);
			this.menuStripMenu.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.tabPageStatus.ResumeLayout(false);
			this.tabPageStatus.PerformLayout();
			this.panelStatusNavi.ResumeLayout(false);
			this.menuStripStatus.ResumeLayout(false);
			this.menuStripStatus.PerformLayout();
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
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchLayer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchJumpScreen)).EndInit();
			this.groupBoxTouchSize.ResumeLayout(false);
			this.groupBoxTouchSize.PerformLayout();
			this.groupBoxTouchLocation.ResumeLayout(false);
			this.groupBoxTouchLocation.PerformLayout();
			this.tabPageCoupler.ResumeLayout(false);
			this.groupBoxCouplerGeneral.ResumeLayout(false);
			this.groupBoxCouplerGeneral.PerformLayout();
			this.tabPageMotor.ResumeLayout(false);
			this.splitContainerMotor.Panel1.ResumeLayout(false);
			this.splitContainerMotor.Panel2.ResumeLayout(false);
			this.splitContainerMotor.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMotor)).EndInit();
			this.splitContainerMotor.ResumeLayout(false);
			this.panelMoterNavi.ResumeLayout(false);
			this.toolStripContainerDrawArea.ContentPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.PerformLayout();
			this.toolStripContainerDrawArea.ResumeLayout(false);
			this.toolStripContainerDrawArea.PerformLayout();
			this.toolStripToolBar.ResumeLayout(false);
			this.toolStripToolBar.PerformLayout();
			this.panelMotorSetting.ResumeLayout(false);
			this.groupBoxPlay.ResumeLayout(false);
			this.groupBoxArea.ResumeLayout(false);
			this.groupBoxArea.PerformLayout();
			this.groupBoxSource.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunIndex)).EndInit();
			this.groupBoxDirect.ResumeLayout(false);
			this.groupBoxDirect.PerformLayout();
			this.groupBoxView.ResumeLayout(false);
			this.groupBoxView.PerformLayout();
			this.groupBoxTrack.ResumeLayout(false);
			this.menuStripMotor.ResumeLayout(false);
			this.menuStripMotor.PerformLayout();
			this.statusStripStatus.ResumeLayout(false);
			this.statusStripStatus.PerformLayout();
			this.tabPageAccel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxAccel)).EndInit();
			this.panelAccel.ResumeLayout(false);
			this.groupBoxPreview.ResumeLayout(false);
			this.groupBoxPreview.PerformLayout();
			this.groupBoxParameter.ResumeLayout(false);
			this.groupBoxParameter.PerformLayout();
			this.groupBoxNotch.ResumeLayout(false);
			this.tabPageCar1.ResumeLayout(false);
			this.groupBoxCarGeneral.ResumeLayout(false);
			this.groupBoxCarGeneral.PerformLayout();
			this.groupBoxAxles.ResumeLayout(false);
			this.groupBoxAxles.PerformLayout();
			this.groupBoxCab.ResumeLayout(false);
			this.groupBoxCab.PerformLayout();
			this.groupBoxExternalCab.ResumeLayout(false);
			this.groupBoxExternalCab.PerformLayout();
			this.groupBoxCameraRestriction.ResumeLayout(false);
			this.groupBoxCameraRestriction.PerformLayout();
			this.groupBoxPressure.ResumeLayout(false);
			this.groupBoxBrakeCylinder.ResumeLayout(false);
			this.groupBoxBrakeCylinder.PerformLayout();
			this.groupBoxStraightAirPipe.ResumeLayout(false);
			this.groupBoxStraightAirPipe.PerformLayout();
			this.groupBoxBrakePipe.ResumeLayout(false);
			this.groupBoxBrakePipe.PerformLayout();
			this.groupBoxEqualizingReservoir.ResumeLayout(false);
			this.groupBoxEqualizingReservoir.PerformLayout();
			this.groupBoxAuxiliaryReservoir.ResumeLayout(false);
			this.groupBoxAuxiliaryReservoir.PerformLayout();
			this.groupBoxMainReservoir.ResumeLayout(false);
			this.groupBoxMainReservoir.PerformLayout();
			this.groupBoxCompressor.ResumeLayout(false);
			this.groupBoxCompressor.PerformLayout();
			this.groupBoxBrake.ResumeLayout(false);
			this.groupBoxBrake.PerformLayout();
			this.groupBoxJerk.ResumeLayout(false);
			this.groupBoxPerformance.ResumeLayout(false);
			this.groupBoxPerformance.PerformLayout();
			this.groupBoxDelay.ResumeLayout(false);
			this.tabPageTrain.ResumeLayout(false);
			this.groupBoxTrainGeneral.ResumeLayout(false);
			this.groupBoxDevice.ResumeLayout(false);
			this.groupBoxHandle.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownLocoBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDriverPowerNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotchReduceSteps)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBrakeNotches)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerNotches)).EndInit();
			this.tabControlEditor.ResumeLayout(false);
			this.tabPageCar2.ResumeLayout(false);
			this.groupBoxUnit.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
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
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImport;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExport;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private TabControl tabControlEditor;
		private TabPage tabPageTrain;
		private GroupBox groupBoxDevice;
		private ComboBox comboBoxDoorCloseMode;
		private ComboBox comboBoxDoorOpenMode;
		private ComboBox comboBoxPassAlarm;
		private ComboBox comboBoxReAdhesionDevice;
		private ComboBox comboBoxAtc;
		private ComboBox comboBoxAts;
		private CheckBox checkBoxHoldBrake;
		private CheckBox checkBoxEb;
		private CheckBox checkBoxConstSpeed;
		private Label labelDoorCloseMode;
		private Label labelDoorOpenMode;
		private Label labelPassAlarm;
		private Label labelReAdhesionDevice;
		private Label labelHoldBrake;
		private Label labelConstSpeed;
		private Label labelEb;
		private Label labelAtc;
		private Label labelAts;
		private GroupBox groupBoxCab;
		private TextBox textBoxCabZ;
		private TextBox textBoxCabY;
		private TextBox textBoxCabX;
		private Label labelCabZ;
		private Label labelCabY;
		private Label labelCabX;
		private GroupBox groupBoxHandle;
		private NumericUpDown numericUpDownLocoBrakeNotches;
		private Label labelLocoBrakeNotches;
		private ComboBox comboBoxLocoBrakeHandleType;
		private Label labelLocoBrakeHandleType;
		private ComboBox comboBoxEbHandleBehaviour;
		private Label labelEbHandleBehaviour;
		private NumericUpDown numericUpDownDriverBrakeNotches;
		private Label labelDriverBrakeNotches;
		private NumericUpDown numericUpDownDriverPowerNotches;
		private Label labelDriverPowerNotches;
		private NumericUpDown numericUpDownPowerNotchReduceSteps;
		private Label labelPowerNotchReduceSteps;
		private NumericUpDown numericUpDownBrakeNotches;
		private Label labelBrakeNotches;
		private NumericUpDown numericUpDownPowerNotches;
		private Label labelPowerNotches;
		private ComboBox comboBoxHandleType;
		private Label labelHandleType;
		private TabPage tabPageCar1;
		private GroupBox groupBoxPressure;
		private TextBox textBoxBrakePipeNormalPressure;
		private Label labelBrakePipeNormalPressure;
		private TextBox textBoxMainReservoirMaximumPressure;
		private TextBox textBoxMainReservoirMinimumPressure;
		private TextBox textBoxBrakeCylinderEmergencyMaximumPressure;
		private TextBox textBoxBrakeCylinderServiceMaximumPressure;
		private Label labelMainReservoirMaximumPressure;
		private Label labelMainReservoirMinimumPressure;
		private Label labelBrakeCylinderServiceMaximumPressure;
		private Label labelBrakeCylinderEmergencyMaximumPressure;
		private GroupBox groupBoxBrake;
		private TextBox textBoxBrakeControlSpeed;
		private ComboBox comboBoxBrakeControlSystem;
		private ComboBox comboBoxLocoBrakeType;
		private ComboBox comboBoxBrakeType;
		private Label labelLocoBrakeType;
		private Label labelBrakeType;
		private Label labelBrakeControlSpeed;
		private Label labelBrakeControlSystem;
		private GroupBox groupBoxJerk;
		private TextBox textBoxBrakeCylinderReleaseRate;
		private TextBox textBoxBrakeCylinderEmergencyRate;
		private Label labelJerkPower;
		private Label labelJerkBrake;
		private Label labelBrakeCylinderEmergencyRate;
		private Label labelBrakeCylinderReleaseRate;
		private GroupBox groupBoxDelay;
		private Button buttonDelayLocoBrakeSet;
		private Button buttonDelayBrakeSet;
		private Button buttonDelayPowerSet;
		private Label labelDelayLocoBrake;
		private Label labelDelayBrake;
		private Label labelDelayPower;
		private GroupBox groupBoxPerformance;
		private TextBox textBoxAerodynamicDragCoefficient;
		private TextBox textBoxCoefficientOfRollingResistance;
		private TextBox textBoxCoefficientOfStaticFriction;
		private TextBox textBoxDeceleration;
		private Label labelAerodynamicDragCoefficient;
		private Label labelCoefficientOfStaticFriction;
		private Label labelDeceleration;
		private Label labelCoefficientOfRollingResistance;
		private GroupBox groupBoxCarGeneral;
		private Label labelRightDoor;
		private Label labelLeftDoor;
		private TextBox textBoxUnexposedFrontalArea;
		private Label labelUnexposedFrontalArea;
		private Label labelRearBogie;
		private Label labelFrontBogie;
		private Button buttonRearBogieSet;
		private Button buttonFrontBogieSet;
		private CheckBox checkBoxReversed;
		private CheckBox checkBoxLoadingSway;
		private CheckBox checkBoxDefinedAxles;
		private CheckBox checkBoxIsMotorCar;
		private Button buttonObjectOpen;
		private Label labelLoadingSway;
		private TextBox textBoxMass;
		private TextBox textBoxLength;
		private TextBox textBoxWidth;
		private TextBox textBoxHeight;
		private TextBox textBoxCenterOfMassHeight;
		private TextBox textBoxExposedFrontalArea;
		private TextBox textBoxObject;
		private Label labelObject;
		private Label labelReversed;
		private GroupBox groupBoxAxles;
		private TextBox textBoxRearAxle;
		private TextBox textBoxFrontAxle;
		private Label labelRearAxle;
		private Label labelFrontAxle;
		private Label labelDefinedAxles;
		private Label labelExposedFrontalArea;
		private Label labelCenterOfMassHeight;
		private Label labelHeight;
		private Label labelWidth;
		private Label labelLength;
		private Label labelMass;
		private Label labelIsMotorCar;
		private TabPage tabPageAccel;
		private PictureBox pictureBoxAccel;
		private Panel panelAccel;
		private GroupBox groupBoxPreview;
		private Button buttonAccelReset;
		private Button buttonAccelZoomOut;
		private Button buttonAccelZoomIn;
		private Label labelAccelYValue;
		private Label labelAccelXValue;
		private Label labelAccelXmaxUnit;
		private Label labelAccelXminUnit;
		private Label labelAccelYmaxUnit;
		private Label labelAccelYminUnit;
		private TextBox textBoxAccelYmax;
		private TextBox textBoxAccelYmin;
		private TextBox textBoxAccelXmax;
		private TextBox textBoxAccelXmin;
		private Label labelAccelYmax;
		private Label labelAccelYmin;
		private Label labelAccelXmax;
		private Label labelAccelXmin;
		private Label labelAccelY;
		private Label labelAccelX;
		private CheckBox checkBoxSubtractDeceleration;
		private GroupBox groupBoxParameter;
		private TextBox textBoxAccelA0;
		private Label labelAccelA0;
		private TextBox textBoxAccelA1;
		private Label labelAccelA1;
		private TextBox textBoxAccelV1;
		private Label labelAccelV1;
		private TextBox textBoxAccelV2;
		private Label labelAccelV2;
		private TextBox textBoxAccelE;
		private Label labelAccelE;
		private GroupBox groupBoxNotch;
		private ComboBox comboBoxNotch;
		private TabPage tabPageMotor;
		private SplitContainer splitContainerMotor;
		private TreeView treeViewMotor;
		private Panel panelMoterNavi;
		private Button buttonMotorCopy;
		private Button buttonMotorRemove;
		private Button buttonMotorAdd;
		private Button buttonMotorUp;
		private Button buttonMotorDown;
		private ToolStripContainer toolStripContainerDrawArea;
		private OpenTK.GLControl glControlMotor;
		private ToolStrip toolStripToolBar;
		private ToolStripButton toolStripButtonUndo;
		private ToolStripButton toolStripButtonRedo;
		private ToolStripSeparator toolStripSeparatorRedo;
		private ToolStripButton toolStripButtonCleanup;
		private ToolStripButton toolStripButtonDelete;
		private ToolStripSeparator toolStripSeparatorEdit;
		private ToolStripButton toolStripButtonSelect;
		private ToolStripButton toolStripButtonMove;
		private ToolStripButton toolStripButtonDot;
		private ToolStripButton toolStripButtonLine;
		private Panel panelMotorSetting;
		private GroupBox groupBoxPlay;
		private Button buttonStop;
		private Button buttonPause;
		private Button buttonPlay;
		private GroupBox groupBoxArea;
		private CheckBox checkBoxMotorConstant;
		private CheckBox checkBoxMotorLoop;
		private Button buttonMotorSwap;
		private TextBox textBoxMotorAreaLeft;
		private Label labelMotorAreaUnit;
		private TextBox textBoxMotorAreaRight;
		private Label labelMotorAccel;
		private TextBox textBoxMotorAccel;
		private GroupBox groupBoxSource;
		private NumericUpDown numericUpDownRunIndex;
		private Label labelRun;
		private GroupBox groupBoxDirect;
		private Button buttonDirectDot;
		private Button buttonDirectMove;
		private TextBox textBoxDirectY;
		private Label labelDirectY;
		private Label labelDirectXUnit;
		private TextBox textBoxDirectX;
		private Label labelDirectX;
		private GroupBox groupBoxView;
		private Button buttonMotorReset;
		private Button buttonMotorZoomOut;
		private Button buttonMotorZoomIn;
		private Label labelMotorMaxVolume;
		private TextBox textBoxMotorMaxVolume;
		private Label labelMotorMinVolume;
		private TextBox textBoxMotorMinVolume;
		private Label labelMotorMaxPitch;
		private TextBox textBoxMotorMaxPitch;
		private Label labelMotorMinPitch;
		private TextBox textBoxMotorMinPitch;
		private Label labelMotorMaxVelocityUnit;
		private TextBox textBoxMotorMaxVelocity;
		private Label labelMotorMaxVelocity;
		private Label labelMotorMinVelocityUnit;
		private TextBox textBoxMotorMinVelocity;
		private Label labelMotorMinVelocity;
		private GroupBox groupBoxTrack;
		private ComboBox comboBoxTrackType;
		private Label labelTrackType;
		private MenuStrip menuStripMotor;
		private ToolStripMenuItem toolStripMenuItemEdit;
		private ToolStripMenuItem toolStripMenuItemUndo;
		private ToolStripMenuItem toolStripMenuItemRedo;
		private ToolStripSeparator toolStripSeparatorUndo;
		private ToolStripMenuItem toolStripMenuItemCleanup;
		private ToolStripMenuItem toolStripMenuItemDelete;
		private ToolStripMenuItem toolStripMenuItemInput;
		private ToolStripMenuItem toolStripMenuItemPitch;
		private ToolStripMenuItem toolStripMenuItemVolume;
		private ToolStripMenuItem toolStripMenuItemIndex;
		private ToolStripComboBox toolStripComboBoxIndex;
		private ToolStripMenuItem toolStripMenuItemTool;
		private ToolStripMenuItem toolStripMenuItemSelect;
		private ToolStripMenuItem toolStripMenuItemMove;
		private ToolStripMenuItem toolStripMenuItemDot;
		private ToolStripMenuItem toolStripMenuItemLine;
		private StatusStrip statusStripStatus;
		private ToolStripStatusLabel toolStripStatusLabelY;
		private ToolStripStatusLabel toolStripStatusLabelX;
		private ToolStripStatusLabel toolStripStatusLabelTool;
		private ToolStripStatusLabel toolStripStatusLabelMode;
		private ToolStripStatusLabel toolStripStatusLabelType;
		private TabPage tabPageCoupler;
		private GroupBox groupBoxCouplerGeneral;
		private Button buttonCouplerObject;
		private TextBox textBoxCouplerObject;
		private Label labelCouplerObject;
		private TextBox textBoxCouplerMax;
		private Label labelCouplerMax;
		private TextBox textBoxCouplerMin;
		private Label labelCouplerMin;
		private TabPage tabPagePanel;
		private SplitContainer splitContainerPanel;
		private TreeView treeViewPanel;
		private ListView listViewPanel;
		private Panel panelPanelNavi;
		private Panel panelPanelNaviCmd;
		private Button buttonPanelCopy;
		private Button buttonPanelAdd;
		private Button buttonPanelRemove;
		private TabControl tabControlPanel;
		private TabPage tabPageThis;
		private Button buttonThisTransparentColorSet;
		private Button buttonThisNighttimeImageOpen;
		private Button buttonThisDaytimeImageOpen;
		private GroupBox groupBoxThisOrigin;
		private TextBox textBoxThisOriginY;
		private TextBox textBoxThisOriginX;
		private Label labelThisOriginY;
		private Label labelThisOriginX;
		private GroupBox groupBoxThisCenter;
		private TextBox textBoxThisCenterY;
		private TextBox textBoxThisCenterX;
		private Label labelThisCenterY;
		private Label labelThisCenterX;
		private TextBox textBoxThisTransparentColor;
		private TextBox textBoxThisNighttimeImage;
		private TextBox textBoxThisDaytimeImage;
		private TextBox textBoxThisBottom;
		private TextBox textBoxThisTop;
		private TextBox textBoxThisRight;
		private TextBox textBoxThisLeft;
		private TextBox textBoxThisResolution;
		private Label labelThisResolution;
		private Label labelThisLeft;
		private Label labelThisRight;
		private Label labelThisTop;
		private Label labelThisBottom;
		private Label labelThisDaytimeImage;
		private Label labelThisNighttimeImage;
		private Label labelThisTransparentColor;
		private TabPage tabPageScreen;
		private NumericUpDown numericUpDownScreenLayer;
		private NumericUpDown numericUpDownScreenNumber;
		private Label labelScreenLayer;
		private Label labelScreenNumber;
		private TabPage tabPagePilotLamp;
		private Button buttonPilotLampTransparentColorSet;
		private NumericUpDown numericUpDownPilotLampLayer;
		private GroupBox groupBoxPilotLampLocation;
		private TextBox textBoxPilotLampLocationY;
		private TextBox textBoxPilotLampLocationX;
		private Label labelPilotLampLocationY;
		private Label labelPilotLampLocationX;
		private Button buttonPilotLampSubjectSet;
		private Label labelPilotLampSubject;
		private Label labelPilotLampLayer;
		private Button buttonPilotLampNighttimeImageOpen;
		private Button buttonPilotLampDaytimeImageOpen;
		private TextBox textBoxPilotLampTransparentColor;
		private TextBox textBoxPilotLampNighttimeImage;
		private TextBox textBoxPilotLampDaytimeImage;
		private Label labelPilotLampDaytimeImage;
		private Label labelPilotLampNighttimeImage;
		private Label labelPilotLampTransparentColor;
		private TabPage tabPageNeedle;
		private CheckBox checkBoxNeedleDefinedDampingRatio;
		private CheckBox checkBoxNeedleDefinedNaturalFreq;
		private Label labelNeedleDefinedDampingRatio;
		private Label labelNeedleDefinedNaturalFreq;
		private Button buttonNeedleTransparentColorSet;
		private Button buttonNeedleColorSet;
		private CheckBox checkBoxNeedleDefinedOrigin;
		private Label labelNeedleDefinedOrigin;
		private CheckBox checkBoxNeedleDefinedRadius;
		private Label labelNeedleDefinedRadius;
		private NumericUpDown numericUpDownNeedleLayer;
		private CheckBox checkBoxNeedleSmoothed;
		private Label labelNeedleSmoothed;
		private CheckBox checkBoxNeedleBackstop;
		private Label labelNeedleBackstop;
		private TextBox textBoxNeedleDampingRatio;
		private Label labelNeedleDampingRatio;
		private TextBox textBoxNeedleNaturalFreq;
		private Label labelNeedleNaturalFreq;
		private TextBox textBoxNeedleMaximum;
		private Label labelNeedleMaximum;
		private TextBox textBoxNeedleMinimum;
		private Label labelNeedleMinimum;
		private TextBox textBoxNeedleLastAngle;
		private Label labelNeedleLastAngle;
		private TextBox textBoxNeedleInitialAngle;
		private Label labelNeedleInitialAngle;
		private GroupBox groupBoxNeedleOrigin;
		private TextBox textBoxNeedleOriginY;
		private TextBox textBoxNeedleOriginX;
		private Label labelNeedleOriginY;
		private Label labelNeedleOriginX;
		private TextBox textBoxNeedleColor;
		private Label labelNeedleColor;
		private TextBox textBoxNeedleRadius;
		private Label labelNeedleRadius;
		private GroupBox groupBoxNeedleLocation;
		private TextBox textBoxNeedleLocationY;
		private TextBox textBoxNeedleLocationX;
		private Label labelNeedleLocationY;
		private Label labelNeedleLocationX;
		private Button buttonNeedleSubjectSet;
		private Label labelNeedleSubject;
		private Label labelNeedleLayer;
		private Button buttonNeedleNighttimeImageOpen;
		private Button buttonNeedleDaytimeImageOpen;
		private TextBox textBoxNeedleTransparentColor;
		private TextBox textBoxNeedleNighttimeImage;
		private TextBox textBoxNeedleDaytimeImage;
		private Label labelNeedleDaytimeImage;
		private Label labelNeedleNighttimeImage;
		private Label labelNeedleTransparentColor;
		private TabPage tabPageDigitalNumber;
		private Button buttonDigitalNumberTransparentColorSet;
		private NumericUpDown numericUpDownDigitalNumberLayer;
		private NumericUpDown numericUpDownDigitalNumberInterval;
		private Label labelDigitalNumberInterval;
		private GroupBox groupBoxDigitalNumberLocation;
		private TextBox textBoxDigitalNumberLocationY;
		private TextBox textBoxDigitalNumberLocationX;
		private Label labelDigitalNumberLocationY;
		private Label labelDigitalNumberLocationX;
		private Button buttonDigitalNumberSubjectSet;
		private Label labelDigitalNumberSubject;
		private Label labelDigitalNumberLayer;
		private Button buttonDigitalNumberNighttimeImageOpen;
		private Button buttonDigitalNumberDaytimeImageOpen;
		private TextBox textBoxDigitalNumberTransparentColor;
		private TextBox textBoxDigitalNumberNighttimeImage;
		private TextBox textBoxDigitalNumberDaytimeImage;
		private Label labelDigitalNumberDaytimeImage;
		private Label labelDigitalNumberNighttimeImage;
		private Label labelDigitalNumberTransparentColor;
		private TabPage tabPageDigitalGauge;
		private Button buttonDigitalGaugeColorSet;
		private NumericUpDown numericUpDownDigitalGaugeLayer;
		private TextBox textBoxDigitalGaugeStep;
		private Label labelDigitalGaugeStep;
		private Label labelDigitalGaugeLayer;
		private TextBox textBoxDigitalGaugeMaximum;
		private Label labelDigitalGaugeMaximum;
		private TextBox textBoxDigitalGaugeMinimum;
		private Label labelDigitalGaugeMinimum;
		private TextBox textBoxDigitalGaugeLastAngle;
		private Label labelDigitalGaugeLastAngle;
		private TextBox textBoxDigitalGaugeInitialAngle;
		private Label labelDigitalGaugeInitialAngle;
		private TextBox textBoxDigitalGaugeColor;
		private Label labelDigitalGaugeColor;
		private TextBox textBoxDigitalGaugeRadius;
		private Label labelDigitalGaugeRadius;
		private GroupBox groupBoxDigitalGaugeLocation;
		private TextBox textBoxDigitalGaugeLocationY;
		private TextBox textBoxDigitalGaugeLocationX;
		private Label labelDigitalGaugeLocationY;
		private Label labelDigitalGaugeLocationX;
		private Button buttonDigitalGaugeSubjectSet;
		private Label labelDigitalGaugeSubject;
		private TabPage tabPageLinearGauge;
		private Button buttonLinearGaugeTransparentColorSet;
		private Button buttonLinearGaugeNighttimeImageOpen;
		private Button buttonLinearGaugeDaytimeImageOpen;
		private TextBox textBoxLinearGaugeTransparentColor;
		private TextBox textBoxLinearGaugeNighttimeImage;
		private TextBox textBoxLinearGaugeDaytimeImage;
		private Label labelLinearGaugeDaytimeImage;
		private Label labelLinearGaugeNighttimeImage;
		private Label labelLinearGaugeTransparentColor;
		private NumericUpDown numericUpDownLinearGaugeLayer;
		private NumericUpDown numericUpDownLinearGaugeWidth;
		private Label labelLinearGaugeWidth;
		private GroupBox groupBoxLinearGaugeDirection;
		private NumericUpDown numericUpDownLinearGaugeDirectionY;
		private NumericUpDown numericUpDownLinearGaugeDirectionX;
		private Label labelLinearGaugeDirectionY;
		private Label labelLinearGaugeDirectionX;
		private Label labelLinearGaugeLayer;
		private TextBox textBoxLinearGaugeMaximum;
		private Label labelLinearGaugeMaximum;
		private TextBox textBoxLinearGaugeMinimum;
		private Label labelLinearGaugeMinimum;
		private GroupBox groupBoxLinearGaugeLocation;
		private TextBox textBoxLinearGaugeLocationY;
		private TextBox textBoxLinearGaugeLocationX;
		private Label labelLinearGaugeLocationY;
		private Label labelLinearGaugeLocationX;
		private Button buttonLinearGaugeSubjectSet;
		private Label labelLinearGaugeSubject;
		private TabPage tabPageTimetable;
		private Button buttonTimetableTransparentColorSet;
		private NumericUpDown numericUpDownTimetableLayer;
		private Label labelTimetableLayer;
		private TextBox textBoxTimetableTransparentColor;
		private Label labelTimetableTransparentColor;
		private TextBox textBoxTimetableHeight;
		private Label labelTimetableHeight;
		private TextBox textBoxTimetableWidth;
		private Label labelTimetableWidth;
		private GroupBox groupBoxTimetableLocation;
		private TextBox textBoxTimetableLocationY;
		private TextBox textBoxTimetableLocationX;
		private Label labelTimetableLocationY;
		private Label labelTimetableLocationX;
		private TabPage tabPageTouch;
		private NumericUpDown numericUpDownTouchLayer;
		private Label labelTouchLayer;
		private Button buttonTouchSoundCommand;
		private Label labelTouchSoundCommand;
		private NumericUpDown numericUpDownTouchJumpScreen;
		private Label labelTouchJumpScreen;
		private GroupBox groupBoxTouchSize;
		private TextBox textBoxTouchSizeY;
		private TextBox textBoxTouchSizeX;
		private Label labelTouchSizeY;
		private Label labelTouchSizeX;
		private GroupBox groupBoxTouchLocation;
		private TextBox textBoxTouchLocationY;
		private TextBox textBoxTouchLocationX;
		private Label labelTouchLocationY;
		private Label labelTouchLocationX;
		private TabPage tabPageSound;
		private Panel panelSoundSetting;
		private SplitContainer splitContainerSound;
		private TreeView treeViewSound;
		private ListView listViewSound;
		private Panel panelSoundSettingEdit;
		private GroupBox groupBoxSoundEntry;
		private Button buttonSoundRemove;
		private GroupBox groupBoxSoundKey;
		private NumericUpDown numericUpDownSoundKeyIndex;
		private ComboBox comboBoxSoundKey;
		private Button buttonSoundAdd;
		private GroupBox groupBoxSoundValue;
		private CheckBox checkBoxSoundRadius;
		private CheckBox checkBoxSoundDefinedPosition;
		private TextBox textBoxSoundRadius;
		private GroupBox groupBoxSoundPosition;
		private Label labelSoundPositionZUnit;
		private TextBox textBoxSoundPositionZ;
		private Label labelSoundPositionZ;
		private Label labelSoundPositionYUnit;
		private TextBox textBoxSoundPositionY;
		private Label labelSoundPositionY;
		private Label labelSoundPositionXUnit;
		private TextBox textBoxSoundPositionX;
		private Label labelSoundPositionX;
		private Label labelSoundFileName;
		private Button buttonSoundFileNameOpen;
		private TextBox textBoxSoundFileName;
		private TabPage tabPageStatus;
		private ListView listViewStatus;
		private ColumnHeader columnHeaderLevel;
		private ColumnHeader columnHeaderText;
		private Panel panelStatusNavi;
		private Button buttonOutputLogs;
		private MenuStrip menuStripStatus;
		private ToolStripMenuItem toolStripMenuItemError;
		private ToolStripMenuItem toolStripMenuItemWarning;
		private ToolStripMenuItem toolStripMenuItemInfo;
		private ToolStripMenuItem toolStripMenuItemClear;
		private Button buttonPanelUp;
		private Button buttonPanelDown;
		private Button buttonSoundUp;
		private Button buttonSoundDown;
		private ToolStripMenuItem toolStripMenuItemImportTrain;
		private ToolStripMenuItem toolStripMenuItemImportPanel;
		private ToolStripMenuItem toolStripMenuItemImportSound;
		private ToolStripMenuItem toolStripMenuItemExportTrain;
		private ToolStripMenuItem toolStripMenuItemExportPanel;
		private ToolStripMenuItem toolStripMenuItemExportSound;
		private TabPage tabPageCar2;
		private GroupBox groupBoxTrainGeneral;
		private ComboBox comboBoxInitialDriverCar;
		private Label labelInitialDriverCar;
		private CheckBox checkBoxIsControlledCar;
		private Label labelIsControlledCar;
		private CheckBox checkBoxIsEmbeddedCab;
		private Label labelIsEmbeddedCab;
		private GroupBox groupBoxExternalCab;
		private TextBox textBoxCabFileName;
		private Label labelCabFileName;
		private GroupBox groupBoxCameraRestriction;
		private Button buttonCabFileNameOpen;
		private TextBox textBoxCameraRestrictionForwards;
		private Label labelCameraRestrictionForwards;
		private CheckBox checkBoxCameraRestrictionDefinedForwards;
		private Label labelCameraRestrictionDefinedForwards;
		private CheckBox checkBoxCameraRestrictionDefinedBackwards;
		private Label labelCameraRestrictionDefinedBackwards;
		private TextBox textBoxCameraRestrictionBackwards;
		private Label labelCameraRestrictionBackwards;
		private CheckBox checkBoxCameraRestrictionDefinedLeft;
		private Label labelCameraRestrictionDefinedLeft;
		private TextBox textBoxCameraRestrictionLeft;
		private Label labelCameraRestrictionLeft;
		private CheckBox checkBoxCameraRestrictionDefinedRight;
		private Label labelCameraRestrictionDefinedRight;
		private TextBox textBoxCameraRestrictionRight;
		private Label labelCameraRestrictionRight;
		private CheckBox checkBoxCameraRestrictionDefinedUp;
		private Label labelCameraRestrictionDefinedUp;
		private TextBox textBoxCameraRestrictionUp;
		private Label labelCameraRestrictionUp;
		private CheckBox checkBoxCameraRestrictionDefinedDown;
		private Label labelCameraRestrictionDefinedDown;
		private TextBox textBoxCameraRestrictionDown;
		private Label labelCameraRestrictionDown;
		private Button buttonRightDoorSet;
		private Button buttonLeftDoorSet;
		private Button buttonJerkBrakeSet;
		private Button buttonJerkPowerSet;
		private GroupBox groupBoxCompressor;
		private TextBox textBoxCompressorRate;
		private Label labelCompressorRate;
		private GroupBox groupBoxMainReservoir;
		private GroupBox groupBoxAuxiliaryReservoir;
		private Label labelAuxiliaryReservoirChargeRate;
		private TextBox textBoxAuxiliaryReservoirChargeRate;
		private GroupBox groupBoxEqualizingReservoir;
		private Label labelEqualizingReservoirChargeRate;
		private TextBox textBoxEqualizingReservoirChargeRate;
		private Label labelEqualizingReservoirEmergencyRate;
		private TextBox textBoxEqualizingReservoirEmergencyRate;
		private Label labelEqualizingReservoirServiceRate;
		private TextBox textBoxEqualizingReservoirServiceRate;
		private GroupBox groupBoxBrakePipe;
		private Label labelBrakePipeEmergencyRate;
		private TextBox textBoxBrakePipeEmergencyRate;
		private Label labelBrakePipeServiceRate;
		private TextBox textBoxBrakePipeServiceRate;
		private Label labelBrakePipeChargeRate;
		private TextBox textBoxBrakePipeChargeRate;
		private GroupBox groupBoxStraightAirPipe;
		private Label labelStraightAirPipeReleaseRate;
		private TextBox textBoxStraightAirPipeReleaseRate;
		private Label labelStraightAirPipeEmergencyRate;
		private TextBox textBoxStraightAirPipeEmergencyRate;
		private Label labelStraightAirPipeServiceRate;
		private TextBox textBoxStraightAirPipeServiceRate;
		private GroupBox groupBoxBrakeCylinder;
		private ComboBox comboBoxMassUnit;
		private ComboBox comboBoxCenterOfMassHeightUnit;
		private ComboBox comboBoxHeightUnit;
		private ComboBox comboBoxWidthUnit;
		private ComboBox comboBoxLengthUnit;
		private ComboBox comboBoxRearAxleUnit;
		private ComboBox comboBoxFrontAxleUnit;
		private ComboBox comboBoxCabZUnit;
		private ComboBox comboBoxCabYUnit;
		private ComboBox comboBoxCabXUnit;
		private ComboBox comboBoxCameraRestrictionDownUnit;
		private ComboBox comboBoxCameraRestrictionUpUnit;
		private ComboBox comboBoxCameraRestrictionRightUnit;
		private ComboBox comboBoxCameraRestrictionLeftUnit;
		private ComboBox comboBoxCameraRestrictionBackwardsUnit;
		private ComboBox comboBoxCameraRestrictionForwardsUnit;
		private ComboBox comboBoxCouplerMaxUnit;
		private ComboBox comboBoxCouplerMinUnit;
		private ComboBox comboBoxBrakeCylinderEmergencyMaximumPressureUnit;
		private ComboBox comboBoxBrakeCylinderServiceMaximumPressureUnit;
		private ComboBox comboBoxBrakePipeNormalPressureUnit;
		private ComboBox comboBoxMainReservoirMaximumPressureUnit;
		private ComboBox comboBoxMainReservoirMinimumPressureUnit;
		private ComboBox comboBoxEqualizingReservoirEmergencyRateUnit;
		private ComboBox comboBoxEqualizingReservoirServiceRateUnit;
		private ComboBox comboBoxEqualizingReservoirChargeRateUnit;
		private ComboBox comboBoxAuxiliaryReservoirChargeRateUnit;
		private ComboBox comboBoxCompressorRateUnit;
		private ComboBox comboBoxBrakeCylinderReleaseRateUnit;
		private ComboBox comboBoxBrakeCylinderEmergencyRateUnit;
		private ComboBox comboBoxStraightAirPipeReleaseRateUnit;
		private ComboBox comboBoxStraightAirPipeEmergencyRateUnit;
		private ComboBox comboBoxStraightAirPipeServiceRateUnit;
		private ComboBox comboBoxBrakePipeEmergencyRateUnit;
		private ComboBox comboBoxBrakePipeServiceRateUnit;
		private ComboBox comboBoxBrakePipeChargeRateUnit;
		private ComboBox comboBoxAccelYUnit;
		private ComboBox comboBoxAccelXUnit;
		private Label labelAccelYUnit;
		private Label labelAccelXUnit;
		private ComboBox comboBoxAccelV2Unit;
		private ComboBox comboBoxAccelV1Unit;
		private ComboBox comboBoxAccelA1Unit;
		private ComboBox comboBoxAccelA0Unit;
		private ComboBox comboBoxBrakeControlSpeedUnit;
		private ComboBox comboBoxDecelerationUnit;
		private ComboBox comboBoxUnexposedFrontalAreaUnit;
		private ComboBox comboBoxExposedFrontalAreaUnit;
		private GroupBox groupBoxUnit;
		private ComboBox comboBoxMotorXUnit;
		private Label labelMotorXUnit;
		private ComboBox comboBoxMotorAccelUnit;
	}
}
