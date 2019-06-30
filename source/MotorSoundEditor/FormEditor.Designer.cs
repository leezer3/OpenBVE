using System.Windows.Forms;

namespace MotorSoundEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditor));
			this.menuStripMenu = new System.Windows.Forms.MenuStrip();
			this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorOpen = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorSave = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
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
			this.toolStripComboBoxLanguage = new System.Windows.Forms.ToolStripComboBox();
			this.statusStripStatus = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTool = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelMode = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTrack = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelType = new System.Windows.Forms.ToolStripStatusLabel();
			this.pictureBoxDrawArea = new System.Windows.Forms.PictureBox();
			this.panelSetting = new System.Windows.Forms.Panel();
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
			this.checkBoxConstant = new System.Windows.Forms.CheckBox();
			this.checkBoxLoop = new System.Windows.Forms.CheckBox();
			this.buttonSwap = new System.Windows.Forms.Button();
			this.textBoxAreaLeft = new System.Windows.Forms.TextBox();
			this.labelAreaUnit = new System.Windows.Forms.Label();
			this.textBoxAreaRight = new System.Windows.Forms.TextBox();
			this.labelAccel = new System.Windows.Forms.Label();
			this.labelAccelUnit = new System.Windows.Forms.Label();
			this.textBoxAccel = new System.Windows.Forms.TextBox();
			this.groupBoxSource = new System.Windows.Forms.GroupBox();
			this.textBoxRunIndex = new System.Windows.Forms.TextBox();
			this.labelRun = new System.Windows.Forms.Label();
			this.checkBoxTrack2 = new System.Windows.Forms.CheckBox();
			this.checkBoxTrack1 = new System.Windows.Forms.CheckBox();
			this.groupBoxView = new System.Windows.Forms.GroupBox();
			this.buttonReset = new System.Windows.Forms.Button();
			this.buttonZoomOut = new System.Windows.Forms.Button();
			this.buttonZoomIn = new System.Windows.Forms.Button();
			this.labelMaxVolume = new System.Windows.Forms.Label();
			this.textBoxMaxVolume = new System.Windows.Forms.TextBox();
			this.labelMinVolume = new System.Windows.Forms.Label();
			this.textBoxMinVolume = new System.Windows.Forms.TextBox();
			this.labelMaxPitch = new System.Windows.Forms.Label();
			this.textBoxMaxPitch = new System.Windows.Forms.TextBox();
			this.labelMinPitch = new System.Windows.Forms.Label();
			this.textBoxMinPitch = new System.Windows.Forms.TextBox();
			this.labelMaxVelocityUnit = new System.Windows.Forms.Label();
			this.textBoxMaxVelocity = new System.Windows.Forms.TextBox();
			this.labelMaxVelocity = new System.Windows.Forms.Label();
			this.labelMinVelocityUnit = new System.Windows.Forms.Label();
			this.textBoxMinVelocity = new System.Windows.Forms.TextBox();
			this.labelMinVelocity = new System.Windows.Forms.Label();
			this.toolStripContainerDrawArea = new System.Windows.Forms.ToolStripContainer();
			this.toolStripToolBar = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorFile = new System.Windows.Forms.ToolStripSeparator();
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
			this.toolTipName = new System.Windows.Forms.ToolTip(this.components);
			this.menuStripMenu.SuspendLayout();
			this.statusStripStatus.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDrawArea)).BeginInit();
			this.panelSetting.SuspendLayout();
			this.groupBoxDirect.SuspendLayout();
			this.groupBoxPlay.SuspendLayout();
			this.groupBoxArea.SuspendLayout();
			this.groupBoxSource.SuspendLayout();
			this.groupBoxView.SuspendLayout();
			this.toolStripContainerDrawArea.ContentPanel.SuspendLayout();
			this.toolStripContainerDrawArea.TopToolStripPanel.SuspendLayout();
			this.toolStripContainerDrawArea.SuspendLayout();
			this.toolStripToolBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStripMenu
			// 
			this.menuStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile,
            this.toolStripMenuItemEdit,
            this.toolStripMenuItemView,
            this.toolStripMenuItemInput,
            this.toolStripMenuItemTool,
            this.toolStripComboBoxLanguage});
			this.menuStripMenu.Location = new System.Drawing.Point(0, 0);
			this.menuStripMenu.Name = "menuStripMenu";
			this.menuStripMenu.Size = new System.Drawing.Size(800, 24);
			this.menuStripMenu.TabIndex = 0;
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
			this.toolStripMenuItemNew.Click += new System.EventHandler(this.ToolStripMenuItemNew_Click);
			// 
			// toolStripMenuItemOpen
			// 
			this.toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
			this.toolStripMenuItemOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.toolStripMenuItemOpen.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemOpen.Text = "Open...(&O)";
			this.toolStripMenuItemOpen.Click += new System.EventHandler(this.ToolStripMenuItemOpen_Click);
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
			this.toolStripMenuItemSave.Click += new System.EventHandler(this.ToolStripMenuItemSave_Click);
			// 
			// toolStripMenuItemSaveAs
			// 
			this.toolStripMenuItemSaveAs.Name = "toolStripMenuItemSaveAs";
			this.toolStripMenuItemSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.toolStripMenuItemSaveAs.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemSaveAs.Text = "Save as...(&A)";
			this.toolStripMenuItemSaveAs.Click += new System.EventHandler(this.ToolStripMenuItemSaveAs_Click);
			// 
			// toolStripSeparatorSave
			// 
			this.toolStripSeparatorSave.Name = "toolStripSeparatorSave";
			this.toolStripSeparatorSave.Size = new System.Drawing.Size(197, 6);
			// 
			// toolStripMenuItemExit
			// 
			this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
			this.toolStripMenuItemExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.toolStripMenuItemExit.Size = new System.Drawing.Size(200, 22);
			this.toolStripMenuItemExit.Text = "Exit(&X)";
			this.toolStripMenuItemExit.Click += new System.EventHandler(this.ToolStripMenuItemExit_Click);
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
			this.toolStripMenuItemUndo.Click += new System.EventHandler(this.ToolStripMenuItemUndo_Click);
			// 
			// toolStripMenuItemRedo
			// 
			this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
			this.toolStripMenuItemRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.toolStripMenuItemRedo.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemRedo.Text = "Redo(&R)";
			this.toolStripMenuItemRedo.Click += new System.EventHandler(this.ToolStripMenuItemRedo_Click);
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
			this.toolStripMenuItemTearingOff.Click += new System.EventHandler(this.ToolStripMenuItemTearingOff_Click);
			// 
			// toolStripMenuItemCopy
			// 
			this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
			this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemCopy.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemCopy.Text = "Copy(&C)";
			this.toolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
			// 
			// toolStripMenuItemPaste
			// 
			this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
			this.toolStripMenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPaste.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemPaste.Text = "Paste(&P)";
			this.toolStripMenuItemPaste.Click += new System.EventHandler(this.ToolStripMenuItemPaste_Click);
			// 
			// toolStripMenuItemCleanup
			// 
			this.toolStripMenuItemCleanup.Name = "toolStripMenuItemCleanup";
			this.toolStripMenuItemCleanup.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemCleanup.Text = "Cleanup";
			this.toolStripMenuItemCleanup.Click += new System.EventHandler(this.ToolStripMenuItemCleanup_Click);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(152, 22);
			this.toolStripMenuItemDelete.Text = "Delete(&D)";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.ToolStripMenuItemDelete_Click);
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
			this.toolStripMenuItemPowerTrack1.Click += new System.EventHandler(this.ToolStripMenuItemPowerTrack1_Click);
			// 
			// toolStripMenuItemPowerTrack2
			// 
			this.toolStripMenuItemPowerTrack2.Name = "toolStripMenuItemPowerTrack2";
			this.toolStripMenuItemPowerTrack2.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemPowerTrack2.Text = "Track2(&2)";
			this.toolStripMenuItemPowerTrack2.Click += new System.EventHandler(this.ToolStripMenuItemPowerTrack2_Click);
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
			this.toolStripMenuItemBrakeTrack1.Click += new System.EventHandler(this.ToolStripMenuItemBrakeTrack1_Click);
			// 
			// toolStripMenuItemBrakeTrack2
			// 
			this.toolStripMenuItemBrakeTrack2.Name = "toolStripMenuItemBrakeTrack2";
			this.toolStripMenuItemBrakeTrack2.Size = new System.Drawing.Size(119, 22);
			this.toolStripMenuItemBrakeTrack2.Text = "Track2(&2)";
			this.toolStripMenuItemBrakeTrack2.Click += new System.EventHandler(this.ToolStripMenuItemBrakeTrack2_Click);
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
			this.toolStripMenuItemPitch.Click += new System.EventHandler(this.ToolStripMenuItemPitch_Click);
			// 
			// toolStripMenuItemVolume
			// 
			this.toolStripMenuItemVolume.Name = "toolStripMenuItemVolume";
			this.toolStripMenuItemVolume.Size = new System.Drawing.Size(181, 22);
			this.toolStripMenuItemVolume.Text = "Volume(&V)";
			this.toolStripMenuItemVolume.Click += new System.EventHandler(this.ToolStripMenuItemVolume_Click);
			// 
			// toolStripMenuItemIndex
			// 
			this.toolStripMenuItemIndex.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBoxIndex});
			this.toolStripMenuItemIndex.Name = "toolStripMenuItemIndex";
			this.toolStripMenuItemIndex.Size = new System.Drawing.Size(181, 22);
			this.toolStripMenuItemIndex.Text = "Sound source index(&I)";
			this.toolStripMenuItemIndex.Click += new System.EventHandler(this.ToolStripMenuItemIndex_Click);
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
			this.toolStripMenuItemSelect.Size = new System.Drawing.Size(117, 22);
			this.toolStripMenuItemSelect.Text = "Select(&S)";
			this.toolStripMenuItemSelect.Click += new System.EventHandler(this.ToolStripMenuItemSelect_Click);
			// 
			// toolStripMenuItemMove
			// 
			this.toolStripMenuItemMove.Name = "toolStripMenuItemMove";
			this.toolStripMenuItemMove.Size = new System.Drawing.Size(117, 22);
			this.toolStripMenuItemMove.Text = "Move(&M)";
			this.toolStripMenuItemMove.Click += new System.EventHandler(this.ToolStripMenuItemMove_Click);
			// 
			// toolStripMenuItemDot
			// 
			this.toolStripMenuItemDot.Name = "toolStripMenuItemDot";
			this.toolStripMenuItemDot.Size = new System.Drawing.Size(117, 22);
			this.toolStripMenuItemDot.Text = "Dot(&D)";
			this.toolStripMenuItemDot.Click += new System.EventHandler(this.ToolStripMenuItemDot_Click);
			// 
			// toolStripMenuItemLine
			// 
			this.toolStripMenuItemLine.Name = "toolStripMenuItemLine";
			this.toolStripMenuItemLine.Size = new System.Drawing.Size(117, 22);
			this.toolStripMenuItemLine.Text = "Move(&L)";
			this.toolStripMenuItemLine.Click += new System.EventHandler(this.ToolStripMenuItemLine_Click);
			// 
			// toolStripComboBoxLanguage
			// 
			this.toolStripComboBoxLanguage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripComboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxLanguage.Name = "toolStripComboBoxLanguage";
			this.toolStripComboBoxLanguage.Size = new System.Drawing.Size(121, 20);
			this.toolStripComboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBoxLanguage_SelectedIndexChanged);
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
			this.statusStripStatus.Location = new System.Drawing.Point(0, 596);
			this.statusStripStatus.Name = "statusStripStatus";
			this.statusStripStatus.Size = new System.Drawing.Size(800, 22);
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
			// pictureBoxDrawArea
			// 
			this.pictureBoxDrawArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBoxDrawArea.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxDrawArea.Name = "pictureBoxDrawArea";
			this.pictureBoxDrawArea.Size = new System.Drawing.Size(580, 547);
			this.pictureBoxDrawArea.TabIndex = 2;
			this.pictureBoxDrawArea.TabStop = false;
			this.pictureBoxDrawArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDrawArea_MouseDown);
			this.pictureBoxDrawArea.MouseEnter += new System.EventHandler(this.PictureBoxDrawArea_MouseEnter);
			this.pictureBoxDrawArea.MouseLeave += new System.EventHandler(this.PictureBoxDrawArea_MouseLeave);
			this.pictureBoxDrawArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDrawArea_MouseMove);
			this.pictureBoxDrawArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDrawArea_MouseUp);
			this.pictureBoxDrawArea.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PictureBoxDrawArea_PreviewKeyDown);
			// 
			// panelSetting
			// 
			this.panelSetting.Controls.Add(this.groupBoxDirect);
			this.panelSetting.Controls.Add(this.groupBoxPlay);
			this.panelSetting.Controls.Add(this.groupBoxView);
			this.panelSetting.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelSetting.Location = new System.Drawing.Point(580, 24);
			this.panelSetting.Name = "panelSetting";
			this.panelSetting.Size = new System.Drawing.Size(220, 572);
			this.panelSetting.TabIndex = 3;
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
			this.groupBoxDirect.Location = new System.Drawing.Point(8, 208);
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
			this.toolTipName.SetToolTip(this.buttonDirectDot, "Dot");
			this.buttonDirectDot.UseVisualStyleBackColor = true;
			this.buttonDirectDot.Click += new System.EventHandler(this.ButtonDirectDot_Click);
			// 
			// buttonDirectMove
			// 
			this.buttonDirectMove.Location = new System.Drawing.Point(72, 64);
			this.buttonDirectMove.Name = "buttonDirectMove";
			this.buttonDirectMove.Size = new System.Drawing.Size(56, 24);
			this.buttonDirectMove.TabIndex = 21;
			this.toolTipName.SetToolTip(this.buttonDirectMove, "Move");
			this.buttonDirectMove.UseVisualStyleBackColor = true;
			this.buttonDirectMove.Click += new System.EventHandler(this.ButtonDirectMove_Click);
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
			this.groupBoxPlay.Location = new System.Drawing.Point(8, 312);
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
			this.toolTipName.SetToolTip(this.buttonStop, "Stop");
			this.buttonStop.UseVisualStyleBackColor = true;
			this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);
			// 
			// buttonPause
			// 
			this.buttonPause.Location = new System.Drawing.Point(72, 216);
			this.buttonPause.Name = "buttonPause";
			this.buttonPause.Size = new System.Drawing.Size(56, 24);
			this.buttonPause.TabIndex = 3;
			this.toolTipName.SetToolTip(this.buttonPause, "Pause");
			this.buttonPause.UseVisualStyleBackColor = true;
			this.buttonPause.Click += new System.EventHandler(this.ButtonPause_Click);
			// 
			// buttonPlay
			// 
			this.buttonPlay.Location = new System.Drawing.Point(8, 216);
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(56, 24);
			this.buttonPlay.TabIndex = 2;
			this.toolTipName.SetToolTip(this.buttonPlay, "Playback");
			this.buttonPlay.UseVisualStyleBackColor = true;
			this.buttonPlay.Click += new System.EventHandler(this.ButtonPlay_Click);
			// 
			// groupBoxArea
			// 
			this.groupBoxArea.Controls.Add(this.checkBoxConstant);
			this.groupBoxArea.Controls.Add(this.checkBoxLoop);
			this.groupBoxArea.Controls.Add(this.buttonSwap);
			this.groupBoxArea.Controls.Add(this.textBoxAreaLeft);
			this.groupBoxArea.Controls.Add(this.labelAreaUnit);
			this.groupBoxArea.Controls.Add(this.textBoxAreaRight);
			this.groupBoxArea.Controls.Add(this.labelAccel);
			this.groupBoxArea.Controls.Add(this.labelAccelUnit);
			this.groupBoxArea.Controls.Add(this.textBoxAccel);
			this.groupBoxArea.Location = new System.Drawing.Point(8, 88);
			this.groupBoxArea.Name = "groupBoxArea";
			this.groupBoxArea.Size = new System.Drawing.Size(184, 120);
			this.groupBoxArea.TabIndex = 1;
			this.groupBoxArea.TabStop = false;
			this.groupBoxArea.Text = "Area setting";
			// 
			// checkBoxConstant
			// 
			this.checkBoxConstant.AutoSize = true;
			this.checkBoxConstant.Location = new System.Drawing.Point(8, 40);
			this.checkBoxConstant.Name = "checkBoxConstant";
			this.checkBoxConstant.Size = new System.Drawing.Size(104, 16);
			this.checkBoxConstant.TabIndex = 19;
			this.checkBoxConstant.Text = "Constant speed";
			this.checkBoxConstant.UseVisualStyleBackColor = true;
			this.checkBoxConstant.CheckedChanged += new System.EventHandler(this.CheckBoxConstant_CheckedChanged);
			// 
			// checkBoxLoop
			// 
			this.checkBoxLoop.AutoSize = true;
			this.checkBoxLoop.Location = new System.Drawing.Point(8, 16);
			this.checkBoxLoop.Name = "checkBoxLoop";
			this.checkBoxLoop.Size = new System.Drawing.Size(97, 16);
			this.checkBoxLoop.TabIndex = 18;
			this.checkBoxLoop.Text = "Loop playback";
			this.checkBoxLoop.UseVisualStyleBackColor = true;
			this.checkBoxLoop.CheckedChanged += new System.EventHandler(this.CheckBoxLoop_CheckedChanged);
			// 
			// buttonSwap
			// 
			this.buttonSwap.Location = new System.Drawing.Point(56, 88);
			this.buttonSwap.Name = "buttonSwap";
			this.buttonSwap.Size = new System.Drawing.Size(32, 19);
			this.buttonSwap.TabIndex = 17;
			this.toolTipName.SetToolTip(this.buttonSwap, "Swap");
			this.buttonSwap.UseVisualStyleBackColor = true;
			this.buttonSwap.Click += new System.EventHandler(this.ButtonSwap_Click);
			// 
			// textBoxAreaLeft
			// 
			this.textBoxAreaLeft.Location = new System.Drawing.Point(8, 88);
			this.textBoxAreaLeft.Name = "textBoxAreaLeft";
			this.textBoxAreaLeft.Size = new System.Drawing.Size(40, 19);
			this.textBoxAreaLeft.TabIndex = 16;
			this.textBoxAreaLeft.TextChanged += new System.EventHandler(this.TextBoxAreaLeft_TextChanged);
			// 
			// labelAreaUnit
			// 
			this.labelAreaUnit.Location = new System.Drawing.Point(144, 88);
			this.labelAreaUnit.Name = "labelAreaUnit";
			this.labelAreaUnit.Size = new System.Drawing.Size(32, 16);
			this.labelAreaUnit.TabIndex = 15;
			this.labelAreaUnit.Text = "km/h";
			this.labelAreaUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxAreaRight
			// 
			this.textBoxAreaRight.Location = new System.Drawing.Point(96, 88);
			this.textBoxAreaRight.Name = "textBoxAreaRight";
			this.textBoxAreaRight.Size = new System.Drawing.Size(40, 19);
			this.textBoxAreaRight.TabIndex = 3;
			this.textBoxAreaRight.TextChanged += new System.EventHandler(this.TextBoxAreaRight_TextChanged);
			// 
			// labelAccel
			// 
			this.labelAccel.Location = new System.Drawing.Point(8, 64);
			this.labelAccel.Name = "labelAccel";
			this.labelAccel.Size = new System.Drawing.Size(72, 16);
			this.labelAccel.TabIndex = 2;
			this.labelAccel.Text = "Acceleration:";
			this.labelAccel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAccelUnit
			// 
			this.labelAccelUnit.Location = new System.Drawing.Point(128, 64);
			this.labelAccelUnit.Name = "labelAccelUnit";
			this.labelAccelUnit.Size = new System.Drawing.Size(44, 16);
			this.labelAccelUnit.TabIndex = 1;
			this.labelAccelUnit.Text = "km/h/s";
			this.labelAccelUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxAccel
			// 
			this.textBoxAccel.Location = new System.Drawing.Point(88, 64);
			this.textBoxAccel.Name = "textBoxAccel";
			this.textBoxAccel.Size = new System.Drawing.Size(32, 19);
			this.textBoxAccel.TabIndex = 0;
			this.textBoxAccel.TextChanged += new System.EventHandler(this.TextBoxAccel_TextChanged);
			// 
			// groupBoxSource
			// 
			this.groupBoxSource.Controls.Add(this.textBoxRunIndex);
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
			// textBoxRunIndex
			// 
			this.textBoxRunIndex.Location = new System.Drawing.Point(112, 16);
			this.textBoxRunIndex.Name = "textBoxRunIndex";
			this.textBoxRunIndex.Size = new System.Drawing.Size(32, 19);
			this.textBoxRunIndex.TabIndex = 15;
			this.textBoxRunIndex.TextChanged += new System.EventHandler(this.TextBoxRunIndex_TextChanged);
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
			this.checkBoxTrack2.CheckedChanged += new System.EventHandler(this.CheckBoxTrack2_CheckedChanged);
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
			this.checkBoxTrack1.CheckedChanged += new System.EventHandler(this.CheckBoxTrack1_CheckedChanged);
			// 
			// groupBoxView
			// 
			this.groupBoxView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxView.Controls.Add(this.buttonReset);
			this.groupBoxView.Controls.Add(this.buttonZoomOut);
			this.groupBoxView.Controls.Add(this.buttonZoomIn);
			this.groupBoxView.Controls.Add(this.labelMaxVolume);
			this.groupBoxView.Controls.Add(this.textBoxMaxVolume);
			this.groupBoxView.Controls.Add(this.labelMinVolume);
			this.groupBoxView.Controls.Add(this.textBoxMinVolume);
			this.groupBoxView.Controls.Add(this.labelMaxPitch);
			this.groupBoxView.Controls.Add(this.textBoxMaxPitch);
			this.groupBoxView.Controls.Add(this.labelMinPitch);
			this.groupBoxView.Controls.Add(this.textBoxMinPitch);
			this.groupBoxView.Controls.Add(this.labelMaxVelocityUnit);
			this.groupBoxView.Controls.Add(this.textBoxMaxVelocity);
			this.groupBoxView.Controls.Add(this.labelMaxVelocity);
			this.groupBoxView.Controls.Add(this.labelMinVelocityUnit);
			this.groupBoxView.Controls.Add(this.textBoxMinVelocity);
			this.groupBoxView.Controls.Add(this.labelMinVelocity);
			this.groupBoxView.Location = new System.Drawing.Point(8, 8);
			this.groupBoxView.Name = "groupBoxView";
			this.groupBoxView.Size = new System.Drawing.Size(200, 192);
			this.groupBoxView.TabIndex = 0;
			this.groupBoxView.TabStop = false;
			this.groupBoxView.Text = "View setting";
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(136, 160);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(56, 24);
			this.buttonReset.TabIndex = 25;
			this.toolTipName.SetToolTip(this.buttonReset, "Reset");
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.ButtonReset_Click);
			// 
			// buttonZoomOut
			// 
			this.buttonZoomOut.Location = new System.Drawing.Point(72, 160);
			this.buttonZoomOut.Name = "buttonZoomOut";
			this.buttonZoomOut.Size = new System.Drawing.Size(56, 24);
			this.buttonZoomOut.TabIndex = 24;
			this.toolTipName.SetToolTip(this.buttonZoomOut, "Zoom Out");
			this.buttonZoomOut.UseVisualStyleBackColor = true;
			this.buttonZoomOut.Click += new System.EventHandler(this.ButtonZoomOut_Click);
			// 
			// buttonZoomIn
			// 
			this.buttonZoomIn.Location = new System.Drawing.Point(8, 160);
			this.buttonZoomIn.Name = "buttonZoomIn";
			this.buttonZoomIn.Size = new System.Drawing.Size(56, 24);
			this.buttonZoomIn.TabIndex = 23;
			this.toolTipName.SetToolTip(this.buttonZoomIn, "Zoom In");
			this.buttonZoomIn.UseVisualStyleBackColor = true;
			this.buttonZoomIn.Click += new System.EventHandler(this.ButtonZoomIn_Click);
			// 
			// labelMaxVolume
			// 
			this.labelMaxVolume.Location = new System.Drawing.Point(8, 136);
			this.labelMaxVolume.Name = "labelMaxVolume";
			this.labelMaxVolume.Size = new System.Drawing.Size(104, 16);
			this.labelMaxVolume.TabIndex = 14;
			this.labelMaxVolume.Text = "y-max(Volume):";
			this.labelMaxVolume.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMaxVolume
			// 
			this.textBoxMaxVolume.Location = new System.Drawing.Point(120, 136);
			this.textBoxMaxVolume.Name = "textBoxMaxVolume";
			this.textBoxMaxVolume.Size = new System.Drawing.Size(32, 19);
			this.textBoxMaxVolume.TabIndex = 13;
			this.textBoxMaxVolume.TextChanged += new System.EventHandler(this.TextBoxMaxVolume_TextChanged);
			// 
			// labelMinVolume
			// 
			this.labelMinVolume.Location = new System.Drawing.Point(8, 112);
			this.labelMinVolume.Name = "labelMinVolume";
			this.labelMinVolume.Size = new System.Drawing.Size(104, 16);
			this.labelMinVolume.TabIndex = 12;
			this.labelMinVolume.Text = "y-min(Volume):";
			this.labelMinVolume.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMinVolume
			// 
			this.textBoxMinVolume.Location = new System.Drawing.Point(120, 112);
			this.textBoxMinVolume.Name = "textBoxMinVolume";
			this.textBoxMinVolume.Size = new System.Drawing.Size(32, 19);
			this.textBoxMinVolume.TabIndex = 11;
			this.textBoxMinVolume.TextChanged += new System.EventHandler(this.TextBoxMinVolume_TextChanged);
			// 
			// labelMaxPitch
			// 
			this.labelMaxPitch.Location = new System.Drawing.Point(8, 88);
			this.labelMaxPitch.Name = "labelMaxPitch";
			this.labelMaxPitch.Size = new System.Drawing.Size(104, 16);
			this.labelMaxPitch.TabIndex = 10;
			this.labelMaxPitch.Text = "y-max(Pitch):";
			this.labelMaxPitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMaxPitch
			// 
			this.textBoxMaxPitch.Location = new System.Drawing.Point(120, 88);
			this.textBoxMaxPitch.Name = "textBoxMaxPitch";
			this.textBoxMaxPitch.Size = new System.Drawing.Size(32, 19);
			this.textBoxMaxPitch.TabIndex = 9;
			this.textBoxMaxPitch.TextChanged += new System.EventHandler(this.TextBoxMaxPitch_TextChanged);
			// 
			// labelMinPitch
			// 
			this.labelMinPitch.Location = new System.Drawing.Point(8, 64);
			this.labelMinPitch.Name = "labelMinPitch";
			this.labelMinPitch.Size = new System.Drawing.Size(104, 16);
			this.labelMinPitch.TabIndex = 8;
			this.labelMinPitch.Text = "y-min(Pitch):";
			this.labelMinPitch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxMinPitch
			// 
			this.textBoxMinPitch.Location = new System.Drawing.Point(120, 64);
			this.textBoxMinPitch.Name = "textBoxMinPitch";
			this.textBoxMinPitch.Size = new System.Drawing.Size(32, 19);
			this.textBoxMinPitch.TabIndex = 7;
			this.textBoxMinPitch.TextChanged += new System.EventHandler(this.TextBoxMinPitch_TextChanged);
			// 
			// labelMaxVelocityUnit
			// 
			this.labelMaxVelocityUnit.Location = new System.Drawing.Point(160, 40);
			this.labelMaxVelocityUnit.Name = "labelMaxVelocityUnit";
			this.labelMaxVelocityUnit.Size = new System.Drawing.Size(32, 16);
			this.labelMaxVelocityUnit.TabIndex = 5;
			this.labelMaxVelocityUnit.Text = "km/h";
			this.labelMaxVelocityUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMaxVelocity
			// 
			this.textBoxMaxVelocity.Location = new System.Drawing.Point(120, 40);
			this.textBoxMaxVelocity.Name = "textBoxMaxVelocity";
			this.textBoxMaxVelocity.Size = new System.Drawing.Size(32, 19);
			this.textBoxMaxVelocity.TabIndex = 4;
			this.textBoxMaxVelocity.TextChanged += new System.EventHandler(this.TextBoxMaxVelocity_TextChanged);
			// 
			// labelMaxVelocity
			// 
			this.labelMaxVelocity.Location = new System.Drawing.Point(8, 40);
			this.labelMaxVelocity.Name = "labelMaxVelocity";
			this.labelMaxVelocity.Size = new System.Drawing.Size(104, 16);
			this.labelMaxVelocity.TabIndex = 3;
			this.labelMaxVelocity.Text = "x-max(Velocity):";
			this.labelMaxVelocity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMinVelocityUnit
			// 
			this.labelMinVelocityUnit.Location = new System.Drawing.Point(160, 16);
			this.labelMinVelocityUnit.Name = "labelMinVelocityUnit";
			this.labelMinVelocityUnit.Size = new System.Drawing.Size(32, 16);
			this.labelMinVelocityUnit.TabIndex = 2;
			this.labelMinVelocityUnit.Text = "km/h";
			this.labelMinVelocityUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxMinVelocity
			// 
			this.textBoxMinVelocity.Location = new System.Drawing.Point(120, 16);
			this.textBoxMinVelocity.Name = "textBoxMinVelocity";
			this.textBoxMinVelocity.Size = new System.Drawing.Size(32, 19);
			this.textBoxMinVelocity.TabIndex = 1;
			this.textBoxMinVelocity.TextChanged += new System.EventHandler(this.TextBoxMinVelocity_TextChanged);
			// 
			// labelMinVelocity
			// 
			this.labelMinVelocity.Location = new System.Drawing.Point(8, 16);
			this.labelMinVelocity.Name = "labelMinVelocity";
			this.labelMinVelocity.Size = new System.Drawing.Size(104, 16);
			this.labelMinVelocity.TabIndex = 0;
			this.labelMinVelocity.Text = "x-min(Velocity):";
			this.labelMinVelocity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// toolStripContainerDrawArea
			// 
			// 
			// toolStripContainerDrawArea.ContentPanel
			// 
			this.toolStripContainerDrawArea.ContentPanel.AllowDrop = true;
			this.toolStripContainerDrawArea.ContentPanel.Controls.Add(this.pictureBoxDrawArea);
			this.toolStripContainerDrawArea.ContentPanel.Size = new System.Drawing.Size(580, 547);
			this.toolStripContainerDrawArea.ContentPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ToolStripContainerDrawArea_ContentPanel_DragDrop);
			this.toolStripContainerDrawArea.ContentPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ToolStripContainerDrawArea_ContentPanel_DragEnter);
			this.toolStripContainerDrawArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainerDrawArea.Location = new System.Drawing.Point(0, 24);
			this.toolStripContainerDrawArea.Name = "toolStripContainerDrawArea";
			this.toolStripContainerDrawArea.Size = new System.Drawing.Size(580, 572);
			this.toolStripContainerDrawArea.TabIndex = 4;
			this.toolStripContainerDrawArea.Text = "toolStripContainer1";
			// 
			// toolStripContainerDrawArea.TopToolStripPanel
			// 
			this.toolStripContainerDrawArea.TopToolStripPanel.Controls.Add(this.toolStripToolBar);
			// 
			// toolStripToolBar
			// 
			this.toolStripToolBar.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.toolStripButtonOpen,
            this.toolStripButtonSave,
            this.toolStripSeparatorFile,
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
			this.toolStripToolBar.Size = new System.Drawing.Size(350, 25);
			this.toolStripToolBar.TabIndex = 0;
			// 
			// toolStripButtonNew
			// 
			this.toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNew.Image")));
			this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNew.Name = "toolStripButtonNew";
			this.toolStripButtonNew.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonNew.Text = "New (Ctrl+N)";
			this.toolStripButtonNew.Click += new System.EventHandler(this.ToolStripButtonNew_Click);
			// 
			// toolStripButtonOpen
			// 
			this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOpen.Image")));
			this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpen.Name = "toolStripButtonOpen";
			this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonOpen.Text = "Open... (Ctrl+O)";
			this.toolStripButtonOpen.Click += new System.EventHandler(this.ToolStripButtonOpen_Click);
			// 
			// toolStripButtonSave
			// 
			this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSave.Image")));
			this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSave.Name = "toolStripButtonSave";
			this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonSave.Text = "Save (Ctrl+S)";
			this.toolStripButtonSave.Click += new System.EventHandler(this.ToolStripButtonSave_Click);
			// 
			// toolStripSeparatorFile
			// 
			this.toolStripSeparatorFile.Name = "toolStripSeparatorFile";
			this.toolStripSeparatorFile.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonUndo
			// 
			this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonUndo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonUndo.Image")));
			this.toolStripButtonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonUndo.Name = "toolStripButtonUndo";
			this.toolStripButtonUndo.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonUndo.Text = "Undo (Ctrl+Z)";
			this.toolStripButtonUndo.Click += new System.EventHandler(this.ToolStripButtonUndo_Click);
			// 
			// toolStripButtonRedo
			// 
			this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonRedo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRedo.Image")));
			this.toolStripButtonRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRedo.Name = "toolStripButtonRedo";
			this.toolStripButtonRedo.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonRedo.Text = "Redo (Ctrl+Y)";
			this.toolStripButtonRedo.Click += new System.EventHandler(this.ToolStripButtonRedo_Click);
			// 
			// toolStripSeparatorRedo
			// 
			this.toolStripSeparatorRedo.Name = "toolStripSeparatorRedo";
			this.toolStripSeparatorRedo.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonTearingOff
			// 
			this.toolStripButtonTearingOff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonTearingOff.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonTearingOff.Image")));
			this.toolStripButtonTearingOff.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonTearingOff.Name = "toolStripButtonTearingOff";
			this.toolStripButtonTearingOff.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonTearingOff.Text = "Cut (Ctrl+X)";
			this.toolStripButtonTearingOff.Click += new System.EventHandler(this.ToolStripButtonTearingOff_Click);
			// 
			// toolStripButtonCopy
			// 
			this.toolStripButtonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCopy.Image")));
			this.toolStripButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCopy.Name = "toolStripButtonCopy";
			this.toolStripButtonCopy.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonCopy.Text = "Copy (Ctrl+C)";
			this.toolStripButtonCopy.Click += new System.EventHandler(this.ToolStripButtonCopy_Click);
			// 
			// toolStripButtonPaste
			// 
			this.toolStripButtonPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonPaste.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPaste.Image")));
			this.toolStripButtonPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPaste.Name = "toolStripButtonPaste";
			this.toolStripButtonPaste.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonPaste.Text = "Paste (Ctrl+V)";
			this.toolStripButtonPaste.Click += new System.EventHandler(this.ToolStripButtonPaste_Click);
			// 
			// toolStripButtonCleanup
			// 
			this.toolStripButtonCleanup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonCleanup.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCleanup.Image")));
			this.toolStripButtonCleanup.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCleanup.Name = "toolStripButtonCleanup";
			this.toolStripButtonCleanup.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonCleanup.Text = "Cleanup";
			this.toolStripButtonCleanup.Click += new System.EventHandler(this.ToolStripButtonCleanup_Click);
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDelete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDelete.Image")));
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonDelete.Text = "Delete (Del)";
			this.toolStripButtonDelete.Click += new System.EventHandler(this.ToolStripButtonDelete_Click);
			// 
			// toolStripSeparatorEdit
			// 
			this.toolStripSeparatorEdit.Name = "toolStripSeparatorEdit";
			this.toolStripSeparatorEdit.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButtonSelect
			// 
			this.toolStripButtonSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonSelect.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSelect.Image")));
			this.toolStripButtonSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSelect.Name = "toolStripButtonSelect";
			this.toolStripButtonSelect.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonSelect.Text = "Select";
			this.toolStripButtonSelect.Click += new System.EventHandler(this.ToolStripButtonSelect_Click);
			// 
			// toolStripButtonMove
			// 
			this.toolStripButtonMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMove.Image")));
			this.toolStripButtonMove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMove.Name = "toolStripButtonMove";
			this.toolStripButtonMove.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonMove.Text = "Move";
			this.toolStripButtonMove.Click += new System.EventHandler(this.ToolStripButtonMove_Click);
			// 
			// toolStripButtonDot
			// 
			this.toolStripButtonDot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonDot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonDot.Image")));
			this.toolStripButtonDot.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDot.Name = "toolStripButtonDot";
			this.toolStripButtonDot.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonDot.Text = "Dot";
			this.toolStripButtonDot.Click += new System.EventHandler(this.ToolStripButtonDot_Click);
			// 
			// toolStripButtonLine
			// 
			this.toolStripButtonLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLine.Image")));
			this.toolStripButtonLine.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLine.Name = "toolStripButtonLine";
			this.toolStripButtonLine.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonLine.Text = "Line";
			this.toolStripButtonLine.Click += new System.EventHandler(this.ToolStripButtonLine_Click);
			// 
			// FormEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 618);
			this.Controls.Add(this.toolStripContainerDrawArea);
			this.Controls.Add(this.panelSetting);
			this.Controls.Add(this.statusStripStatus);
			this.Controls.Add(this.menuStripMenu);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStripMenu;
			this.Name = "FormEditor";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.menuStripMenu.ResumeLayout(false);
			this.menuStripMenu.PerformLayout();
			this.statusStripStatus.ResumeLayout(false);
			this.statusStripStatus.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDrawArea)).EndInit();
			this.panelSetting.ResumeLayout(false);
			this.groupBoxDirect.ResumeLayout(false);
			this.groupBoxDirect.PerformLayout();
			this.groupBoxPlay.ResumeLayout(false);
			this.groupBoxArea.ResumeLayout(false);
			this.groupBoxArea.PerformLayout();
			this.groupBoxSource.ResumeLayout(false);
			this.groupBoxSource.PerformLayout();
			this.groupBoxView.ResumeLayout(false);
			this.groupBoxView.PerformLayout();
			this.toolStripContainerDrawArea.ContentPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainerDrawArea.TopToolStripPanel.PerformLayout();
			this.toolStripContainerDrawArea.ResumeLayout(false);
			this.toolStripContainerDrawArea.PerformLayout();
			this.toolStripToolBar.ResumeLayout(false);
			this.toolStripToolBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStripMenu;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNew;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorOpen;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSave;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSave;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdit;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUndo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRedo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparatorUndo;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTearingOff;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
		private System.Windows.Forms.StatusStrip statusStripStatus;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemView;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPower;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrake;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPowerTrack1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPowerTrack2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrakeTrack1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBrakeTrack2;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInput;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPitch;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVolume;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemIndex;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxIndex;
		private System.Windows.Forms.PictureBox pictureBoxDrawArea;
		private ToolStripMenuItem toolStripMenuItemTool;
		private ToolStripMenuItem toolStripMenuItemSelect;
		private ToolStripMenuItem toolStripMenuItemMove;
		private ToolStripMenuItem toolStripMenuItemDot;
		private ToolStripMenuItem toolStripMenuItemLine;
		private Panel panelSetting;
		private GroupBox groupBoxView;
		private TextBox textBoxMinVelocity;
		private Label labelMinVelocity;
		private Label labelMinVelocityUnit;
		private Label labelMaxVelocityUnit;
		private TextBox textBoxMaxVelocity;
		private Label labelMaxVelocity;
		private Label labelMinPitch;
		private TextBox textBoxMinPitch;
		private Label labelMaxPitch;
		private TextBox textBoxMaxPitch;
		private Label labelMinVolume;
		private TextBox textBoxMinVolume;
		private Label labelMaxVolume;
		private TextBox textBoxMaxVolume;
		private ToolStripStatusLabel toolStripStatusLabelType;
		private ToolStripStatusLabel toolStripStatusLabelTrack;
		private ToolStripStatusLabel toolStripStatusLabelMode;
		private ToolStripStatusLabel toolStripStatusLabelTool;
		private ToolStripStatusLabel toolStripStatusLabelX;
		private ToolStripStatusLabel toolStripStatusLabelY;
		private ToolStripContainer toolStripContainerDrawArea;
		private ToolStrip toolStripToolBar;
		private ToolStripButton toolStripButtonNew;
		private ToolStripButton toolStripButtonOpen;
		private ToolStripButton toolStripButtonSave;
		private ToolStripSeparator toolStripSeparatorFile;
		private ToolStripButton toolStripButtonUndo;
		private ToolStripButton toolStripButtonRedo;
		private ToolStripSeparator toolStripSeparatorRedo;
		private ToolStripButton toolStripButtonTearingOff;
		private ToolStripButton toolStripButtonCopy;
		private ToolStripButton toolStripButtonPaste;
		private ToolStripButton toolStripButtonDelete;
		private ToolStripSeparator toolStripSeparatorEdit;
		private ToolStripButton toolStripButtonSelect;
		private ToolStripButton toolStripButtonMove;
		private ToolStripButton toolStripButtonDot;
		private ToolStripButton toolStripButtonLine;
		private GroupBox groupBoxPlay;
		private GroupBox groupBoxSource;
		private Label labelRun;
		private CheckBox checkBoxTrack2;
		private CheckBox checkBoxTrack1;
		private GroupBox groupBoxArea;
		private Label labelAccel;
		private Label labelAccelUnit;
		private TextBox textBoxAccel;
		private Button buttonSwap;
		private TextBox textBoxAreaLeft;
		private Label labelAreaUnit;
		private TextBox textBoxAreaRight;
		private Button buttonStop;
		private Button buttonPause;
		private Button buttonPlay;
		private CheckBox checkBoxConstant;
		private CheckBox checkBoxLoop;
		private TextBox textBoxRunIndex;
		private GroupBox groupBoxDirect;
		private Button buttonDirectDot;
		private Button buttonDirectMove;
		private TextBox textBoxDirectY;
		private Label labelDirectY;
		private Label labelDirectXUnit;
		private TextBox textBoxDirectX;
		private Label labelDirectX;
		private ToolStripMenuItem toolStripMenuItemCleanup;
		private ToolStripButton toolStripButtonCleanup;
		private Button buttonZoomOut;
		private Button buttonZoomIn;
		private Button buttonReset;
		private ToolStripComboBox toolStripComboBoxLanguage;
		private ToolTip toolTipName;
	}
}

