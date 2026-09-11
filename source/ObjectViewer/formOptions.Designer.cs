namespace ObjectViewer
{
    partial class formOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageOptions = new System.Windows.Forms.TabPage();
            this.comboBoxOptimizeObjects = new System.Windows.Forms.ComboBox();
            this.labelOptimizeObjects = new System.Windows.Forms.Label();
            this.labelNearClip = new System.Windows.Forms.Label();
            this.nearClip = new System.Windows.Forms.NumericUpDown();
            this.comboBoxNewObjParser = new System.Windows.Forms.ComboBox();
            this.labelUseNewObjParser = new System.Windows.Forms.Label();
            this.comboBoxNewXParser = new System.Windows.Forms.ComboBox();
            this.labelUseNewXParser = new System.Windows.Forms.Label();
            this.labelOtherSettings = new System.Windows.Forms.Label();
            this.labelTransparencyQuality = new System.Windows.Forms.Label();
            this.TransparencyQuality = new System.Windows.Forms.ComboBox();
            this.AntialiasingLevel = new System.Windows.Forms.NumericUpDown();
            this.AnisotropicLevel = new System.Windows.Forms.NumericUpDown();
            this.labelHeight = new System.Windows.Forms.Label();
            this.labelWidth = new System.Windows.Forms.Label();
            this.height = new System.Windows.Forms.NumericUpDown();
            this.width = new System.Windows.Forms.NumericUpDown();
            this.labelResolutionSettings = new System.Windows.Forms.Label();
            this.labelAntialisingLevel = new System.Windows.Forms.Label();
            this.labelAnisotropicFilteringLevel = new System.Windows.Forms.Label();
            this.labelInterpolationMode = new System.Windows.Forms.Label();
            this.labelInterpolationSettings = new System.Windows.Forms.Label();
            this.InterpolationMode = new System.Windows.Forms.ComboBox();
            this.tabPageKeys = new System.Windows.Forms.TabPage();
            this.labelControls = new System.Windows.Forms.Label();
            this.comboBoxBackwards = new System.Windows.Forms.ComboBox();
            this.labelBackwards = new System.Windows.Forms.Label();
            this.comboBoxForwards = new System.Windows.Forms.ComboBox();
            this.labelForwards = new System.Windows.Forms.Label();
            this.comboBoxDown = new System.Windows.Forms.ComboBox();
            this.labelDown = new System.Windows.Forms.Label();
            this.comboBoxUp = new System.Windows.Forms.ComboBox();
            this.labelUp = new System.Windows.Forms.Label();
            this.comboBoxRight = new System.Windows.Forms.ComboBox();
            this.labelRight = new System.Windows.Forms.Label();
            this.comboBoxLeft = new System.Windows.Forms.ComboBox();
            this.labelLeft = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.labelAutoReloadChanged = new System.Windows.Forms.Label();
            this.checkBoxAutoReload = new System.Windows.Forms.CheckBox();
            this.tabPageShadows = new System.Windows.Forms.TabPage();
            this.comboBoxShadowResolution = new System.Windows.Forms.ComboBox();
            this.labelShadowResolution = new System.Windows.Forms.Label();
            this.comboBoxShadowDistance = new System.Windows.Forms.ComboBox();
            this.labelShadowDistance = new System.Windows.Forms.Label();
            this.comboBoxShadowCascades = new System.Windows.Forms.ComboBox();
            this.labelShadowCascades = new System.Windows.Forms.Label();
            this.numericUpDownShadowStrength = new System.Windows.Forms.NumericUpDown();
            this.labelShadowStrength = new System.Windows.Forms.Label();
            this.labelSunDirection = new System.Windows.Forms.Label();
            this.labelSunAzimuth = new System.Windows.Forms.Label();
            this.trackBarSunAzimuth = new System.Windows.Forms.TrackBar();
            this.labelSunAzimuthValue = new System.Windows.Forms.Label();
            this.labelSunElevation = new System.Windows.Forms.Label();
            this.trackBarSunElevation = new System.Windows.Forms.TrackBar();
            this.labelSunElevationValue = new System.Windows.Forms.Label();
            this.labelShadowBias = new System.Windows.Forms.Label();
            this.numericUpDownShadowBias = new System.Windows.Forms.NumericUpDown();
            this.labelShadowNormalBias = new System.Windows.Forms.Label();
            this.numericUpDownShadowNormalBias = new System.Windows.Forms.NumericUpDown();
            this.labelShadowFilterCascades = new System.Windows.Forms.Label();
            this.checkBoxShadowFilterCascades = new System.Windows.Forms.CheckBox();
            this.labelVSync = new System.Windows.Forms.Label();
            this.comboBoxVSync = new System.Windows.Forms.ComboBox();
            this.labelFPSLimit = new System.Windows.Forms.Label();
            this.comboBoxFPSLimit = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip();
            this.tabControl1.SuspendLayout();
            this.tabPageOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnisotropicLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).BeginInit();
            this.tabPageShadows.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowStrength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunAzimuth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunElevation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowBias)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowNormalBias)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nearClip)).BeginInit();
            this.tabPageKeys.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageOptions);
            this.tabControl1.Controls.Add(this.tabPageShadows);
            this.tabControl1.Controls.Add(this.tabPageKeys);
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Location = new System.Drawing.Point(1, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(348, 460);
            this.tabControl1.TabIndex = 26;
            // 
            // tabPageOptions
            // 
            var tlpOptions = new System.Windows.Forms.TableLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Top,
                Padding = new System.Windows.Forms.Padding(10),
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F)
                },
                Controls =
                {
                    // Interpolation Settings
                    { this.labelInterpolationSettings, 0, 0 },
                    { this.labelInterpolationMode, 0, 1 },
                    { this.InterpolationMode, 1, 1 },
                    { this.labelAnisotropicFilteringLevel, 0, 2 },
                    { this.AnisotropicLevel, 1, 2 },
                    { this.labelAntialisingLevel, 0, 3 },
                    { this.AntialiasingLevel, 1, 3 },
                    { this.labelTransparencyQuality, 0, 4 },
                    { this.TransparencyQuality, 1, 4 },
                    // Resolution Settings
                    { this.labelResolutionSettings, 0, 5 },
                    { this.labelWidth, 0, 6 },
                    { this.width, 1, 6 },
                    { this.labelHeight, 0, 7 },
                    { this.height, 1, 7 },
                    // Other Settings
                    { this.labelOtherSettings, 0, 8 },
                    { this.labelVSync, 0, 9 },
                    { this.comboBoxVSync, 1, 9 },
                    { this.labelFPSLimit, 0, 10 },
                    { this.comboBoxFPSLimit, 1, 10 },
                    { this.labelUseNewXParser, 0, 11 },
                    { this.comboBoxNewXParser, 1, 11 },
                    { this.labelUseNewObjParser, 0, 12 },
                    { this.comboBoxNewObjParser, 1, 12 },
                    { this.labelOptimizeObjects, 0, 13 },
                    { this.comboBoxOptimizeObjects, 1, 13 },
                    { this.labelNearClip, 0, 14 },
                    { this.nearClip, 1, 14 },
                    { this.labelAutoReloadChanged, 0, 15 },
                    { this.checkBoxAutoReload, 1, 15 }
                }
            };
            tlpOptions.SetColumnSpan(this.labelInterpolationSettings, 2);
            tlpOptions.SetColumnSpan(this.labelResolutionSettings, 2);
            tlpOptions.SetColumnSpan(this.labelOtherSettings, 2);
            this.tabPageOptions.Controls.Add(tlpOptions);
            this.tabPageOptions.AutoScroll = true;
            this.tabPageOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(304, 385);
            this.tabPageOptions.TabIndex = 0;
            this.tabPageOptions.Text = "Options";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // comboBoxOptimizeObjects
            // 
            this.comboBoxOptimizeObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOptimizeObjects.FormattingEnabled = true;
            this.comboBoxOptimizeObjects.Items.AddRange(new object[] {
            "None",
            "Low",
            "High"});
            this.comboBoxOptimizeObjects.Location = new System.Drawing.Point(160, 301);
            this.comboBoxOptimizeObjects.Name = "comboBoxOptimizeObjects";
            this.comboBoxOptimizeObjects.Size = new System.Drawing.Size(121, 21);
            this.comboBoxOptimizeObjects.TabIndex = 47;
            // 
            // labelOptimizeObjects
            // 
            this.labelOptimizeObjects.AutoSize = true;
            this.labelOptimizeObjects.Location = new System.Drawing.Point(10, 301);
            this.labelOptimizeObjects.Name = "labelOptimizeObjects";
            this.labelOptimizeObjects.Size = new System.Drawing.Size(131, 13);
            this.labelOptimizeObjects.TabIndex = 46;
            this.labelOptimizeObjects.Text = "Object Optimization Mode:";
            // 
            // labelNearClip
            // 
            this.labelNearClip.AutoSize = true;
            this.labelNearClip.Location = new System.Drawing.Point(10, 327);
            this.labelNearClip.Name = "labelNearClip";
            this.labelNearClip.Size = new System.Drawing.Size(73, 13);
            this.labelNearClip.TabIndex = 48;
            this.labelNearClip.Text = "Near Clip (m):";
            // 
            // nearClip
            // 
            this.nearClip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nearClip.DecimalPlaces = 3;
            this.nearClip.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nearClip.Location = new System.Drawing.Point(160, 327);
            this.nearClip.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nearClip.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nearClip.Name = "nearClip";
            this.nearClip.Size = new System.Drawing.Size(121, 20);
            this.nearClip.TabIndex = 49;
            this.nearClip.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // comboBoxNewObjParser
            // 
            this.comboBoxNewObjParser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxNewObjParser.FormattingEnabled = true;
            this.comboBoxNewObjParser.Items.AddRange(new object[] {
            "OriginalObjParser",
            "AssimpObjParser"});
            this.comboBoxNewObjParser.Location = new System.Drawing.Point(160, 275);
            this.comboBoxNewObjParser.Name = "comboBoxNewObjParser";
            this.comboBoxNewObjParser.Size = new System.Drawing.Size(121, 21);
            this.comboBoxNewObjParser.TabIndex = 45;
            // 
            // labelUseNewObjParser
            // 
            this.labelUseNewObjParser.AutoSize = true;
            this.labelUseNewObjParser.Location = new System.Drawing.Point(9, 275);
            this.labelUseNewObjParser.Name = "labelUseNewObjParser";
            this.labelUseNewObjParser.Size = new System.Drawing.Size(106, 13);
            this.labelUseNewObjParser.TabIndex = 44;
            this.labelUseNewObjParser.Text = "Use New Obj Parser:";
            // 
            // comboBoxNewXParser
            // 
            this.comboBoxNewXParser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxNewXParser.FormattingEnabled = true;
            this.comboBoxNewXParser.Items.AddRange(new object[] {
            "OriginalXParser",
            "NewXParser",
            "AssimpXParser"});
            this.comboBoxNewXParser.Location = new System.Drawing.Point(160, 249);
            this.comboBoxNewXParser.Name = "comboBoxNewXParser";
            this.comboBoxNewXParser.Size = new System.Drawing.Size(121, 21);
            this.comboBoxNewXParser.TabIndex = 43;
            // 
            // labelUseNewXParser
            // 
            this.labelUseNewXParser.AutoSize = true;
            this.labelUseNewXParser.Location = new System.Drawing.Point(9, 249);
            this.labelUseNewXParser.Name = "labelUseNewXParser";
            this.labelUseNewXParser.Size = new System.Drawing.Size(97, 13);
            this.labelUseNewXParser.TabIndex = 42;
            this.labelUseNewXParser.Text = "Use New X Parser:";
            // 
            // labelOtherSettings
            // 
            this.labelOtherSettings.AutoSize = true;
            this.labelOtherSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOtherSettings.Location = new System.Drawing.Point(93, 227);
            this.labelOtherSettings.Name = "labelOtherSettings";
            this.labelOtherSettings.Size = new System.Drawing.Size(42, 15);
            this.labelOtherSettings.TabIndex = 41;
            this.labelOtherSettings.Text = "Other";
            // 
            // labelTransparencyQuality
            // 
            this.labelTransparencyQuality.AutoSize = true;
            this.labelTransparencyQuality.Location = new System.Drawing.Point(6, 106);
            this.labelTransparencyQuality.Name = "labelTransparencyQuality";
            this.labelTransparencyQuality.Size = new System.Drawing.Size(110, 13);
            this.labelTransparencyQuality.TabIndex = 40;
            this.labelTransparencyQuality.Text = "Transparency Quality:";
            // 
            // TransparencyQuality
            // 
            this.TransparencyQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransparencyQuality.FormattingEnabled = true;
            this.TransparencyQuality.Items.AddRange(new object[] {
            "Sharp",
            "Intermediate",
            "Smooth"});
            this.TransparencyQuality.Location = new System.Drawing.Point(160, 106);
            this.TransparencyQuality.Name = "TransparencyQuality";
            this.TransparencyQuality.Size = new System.Drawing.Size(121, 21);
            this.TransparencyQuality.TabIndex = 39;
            // 
            // AntialiasingLevel
            // 
            this.AntialiasingLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AntialiasingLevel.Location = new System.Drawing.Point(160, 77);
            this.AntialiasingLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.AntialiasingLevel.Name = "AntialiasingLevel";
            this.AntialiasingLevel.Size = new System.Drawing.Size(120, 20);
            this.AntialiasingLevel.TabIndex = 38;
            // 
            // AnisotropicLevel
            // 
            this.AnisotropicLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AnisotropicLevel.Location = new System.Drawing.Point(160, 52);
            this.AnisotropicLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.AnisotropicLevel.Name = "AnisotropicLevel";
            this.AnisotropicLevel.Size = new System.Drawing.Size(120, 20);
            this.AnisotropicLevel.TabIndex = 37;
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(6, 191);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(41, 13);
            this.labelHeight.TabIndex = 36;
            this.labelHeight.Text = "Height:";
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(6, 165);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(38, 13);
            this.labelWidth.TabIndex = 35;
            this.labelWidth.Text = "Width:";
            // 
            // height
            // 
            this.height.Dock = System.Windows.Forms.DockStyle.Fill;
            this.height.Location = new System.Drawing.Point(161, 189);
            this.height.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.height.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.height.Name = "height";
            this.height.Size = new System.Drawing.Size(120, 20);
            this.height.TabIndex = 34;
            this.height.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // width
            // 
            this.width.Dock = System.Windows.Forms.DockStyle.Fill;
            this.width.Location = new System.Drawing.Point(160, 163);
            this.width.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.width.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.width.Name = "width";
            this.width.Size = new System.Drawing.Size(120, 20);
            this.width.TabIndex = 33;
            this.width.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // labelResolutionSettings
            // 
            this.labelResolutionSettings.AutoSize = true;
            this.labelResolutionSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResolutionSettings.Location = new System.Drawing.Point(93, 142);
            this.labelResolutionSettings.Name = "labelResolutionSettings";
            this.labelResolutionSettings.Size = new System.Drawing.Size(132, 15);
            this.labelResolutionSettings.TabIndex = 32;
            this.labelResolutionSettings.Text = "Resolution Settings";
            // 
            // labelAntialisingLevel
            // 
            this.labelAntialisingLevel.AutoSize = true;
            this.labelAntialisingLevel.Location = new System.Drawing.Point(6, 79);
            this.labelAntialisingLevel.Name = "labelAntialisingLevel";
            this.labelAntialisingLevel.Size = new System.Drawing.Size(92, 13);
            this.labelAntialisingLevel.TabIndex = 30;
            this.labelAntialisingLevel.Text = "Anitaliasing Level:";
            // 
            // labelAnisotropicFilteringLevel
            // 
            this.labelAnisotropicFilteringLevel.AutoSize = true;
            this.labelAnisotropicFilteringLevel.Location = new System.Drawing.Point(3, 52);
            this.labelAnisotropicFilteringLevel.Name = "labelAnisotropicFilteringLevel";
            this.labelAnisotropicFilteringLevel.Size = new System.Drawing.Size(130, 13);
            this.labelAnisotropicFilteringLevel.TabIndex = 29;
            this.labelAnisotropicFilteringLevel.Text = "Anisotropic Filtering Level:";
            // 
            // labelInterpolationMode
            // 
            this.labelInterpolationMode.AutoSize = true;
            this.labelInterpolationMode.Location = new System.Drawing.Point(6, 24);
            this.labelInterpolationMode.Name = "labelInterpolationMode";
            this.labelInterpolationMode.Size = new System.Drawing.Size(37, 13);
            this.labelInterpolationMode.TabIndex = 28;
            this.labelInterpolationMode.Text = "Mode:";
            // 
            // labelInterpolationSettings
            // 
            this.labelInterpolationSettings.AutoSize = true;
            this.labelInterpolationSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInterpolationSettings.Location = new System.Drawing.Point(93, 3);
            this.labelInterpolationSettings.Name = "labelInterpolationSettings";
            this.labelInterpolationSettings.Size = new System.Drawing.Size(144, 15);
            this.labelInterpolationSettings.TabIndex = 27;
            this.labelInterpolationSettings.Text = "Interpolation Settings";
            // 
            // InterpolationMode
            // 
            this.InterpolationMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InterpolationMode.FormattingEnabled = true;
            this.InterpolationMode.Items.AddRange(new object[] {
            "Nearest Neighbour",
            "Bilinear",
            "Nearest Neighbour (Mipmapped)",
            "Bilinear (Mipmapped)",
            "Trilinear (Mipmapped)",
            "Anisotropic Filtering"});
            this.InterpolationMode.Location = new System.Drawing.Point(160, 24);
            this.InterpolationMode.Name = "InterpolationMode";
            this.InterpolationMode.Size = new System.Drawing.Size(121, 21);
            this.InterpolationMode.TabIndex = 26;
            // 
            // tabPageShadows
            // 
            var tlpShadows = new System.Windows.Forms.TableLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Top,
                Padding = new System.Windows.Forms.Padding(10),
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F)
                },
                Controls =
                {
                    // Shadow Settings
                    { this.labelShadowResolution, 0, 0 },
                    { this.comboBoxShadowResolution, 1, 0 },
                    { this.labelShadowDistance, 0, 1 },
                    { this.comboBoxShadowDistance, 1, 1 },
                    { this.labelShadowCascades, 0, 2 },
                    { this.comboBoxShadowCascades, 1, 2 },
                    { this.labelShadowStrength, 0, 3 },
                    { this.numericUpDownShadowStrength, 1, 3 },
                    { this.labelShadowBias, 0, 4 },
                    { this.numericUpDownShadowBias, 1, 4 },
                    { this.labelShadowNormalBias, 0, 5 },
                    { this.numericUpDownShadowNormalBias, 1, 5 },
                    { this.labelShadowFilterCascades, 0, 6 },
                    { this.checkBoxShadowFilterCascades, 1, 6 },
                    // Sun Direction
                    { this.labelSunDirection, 0, 7 },
                    { this.labelSunAzimuth, 0, 8 },
                    { this.labelSunElevation, 0, 10 }
                }
            };
            tlpShadows.SetColumnSpan(this.labelSunDirection, 2);
            tlpShadows.SetColumnSpan(this.labelSunAzimuth, 2);
            tlpShadows.SetColumnSpan(this.labelSunElevation, 2);
            var azimuthPanel = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)
                },
                Controls =
                {
                    { this.trackBarSunAzimuth, 0, 0 },
                    { this.labelSunAzimuthValue, 1, 0 }
                }
            };
            this.trackBarSunAzimuth.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tlpShadows.Controls.Add(azimuthPanel, 0, 9);
            tlpShadows.SetColumnSpan(azimuthPanel, 2);
            var elevationPanel = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)
                },
                Controls =
                {
                    { this.trackBarSunElevation, 0, 0 },
                    { this.labelSunElevationValue, 1, 0 }
                }
            };
            this.trackBarSunElevation.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tlpShadows.Controls.Add(elevationPanel, 0, 11);
            tlpShadows.SetColumnSpan(elevationPanel, 2);
            this.tabPageShadows.Controls.Add(tlpShadows);
            this.tabPageShadows.AutoScroll = true;
            this.tabPageShadows.Location = new System.Drawing.Point(4, 22);
            this.tabPageShadows.Name = "tabPageShadows";
            this.tabPageShadows.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageShadows.Size = new System.Drawing.Size(304, 348);
            this.tabPageShadows.TabIndex = 2;
            this.tabPageShadows.Text = "Shadow && Lighting";
            this.tabPageShadows.UseVisualStyleBackColor = true;
            // 
            // comboBoxShadowResolution
            // 
            this.comboBoxShadowResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowResolution.FormattingEnabled = true;
            this.comboBoxShadowResolution.Items.AddRange(new object[] {
            "Off",
            "Low (512)",
            "Medium (1024)",
            "High (2048)",
            "Ultra (4096)"});
            this.comboBoxShadowResolution.Location = new System.Drawing.Point(160, 24);
            this.comboBoxShadowResolution.Name = "comboBoxShadowResolution";
            this.comboBoxShadowResolution.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowResolution.TabIndex = 28;
            // 
            // labelShadowResolution
            // 
            this.labelShadowResolution.AutoSize = true;
            this.labelShadowResolution.Location = new System.Drawing.Point(6, 27);
            this.labelShadowResolution.Name = "labelShadowResolution";
            this.labelShadowResolution.Size = new System.Drawing.Size(60, 13);
            this.labelShadowResolution.TabIndex = 29;
            this.labelShadowResolution.Text = "Resolution:";
            // 
            // comboBoxShadowDistance
            // 
            this.comboBoxShadowDistance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowDistance.FormattingEnabled = true;
            this.comboBoxShadowDistance.Items.AddRange(new object[] {
            "Near (150m)",
            "Medium (300m)",
            "Far (500m)",
            "Very Far (800m)",
            "Follow Viewing Distance"});
            this.comboBoxShadowDistance.Location = new System.Drawing.Point(160, 54);
            this.comboBoxShadowDistance.Name = "comboBoxShadowDistance";
            this.comboBoxShadowDistance.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowDistance.TabIndex = 30;
            // 
            // labelShadowDistance
            // 
            this.labelShadowDistance.AutoSize = true;
            this.labelShadowDistance.Location = new System.Drawing.Point(6, 57);
            this.labelShadowDistance.Name = "labelShadowDistance";
            this.labelShadowDistance.Size = new System.Drawing.Size(82, 13);
            this.labelShadowDistance.TabIndex = 31;
            this.labelShadowDistance.Text = "Draw Distance:";
            // 
            // comboBoxShadowCascades
            // 
            this.comboBoxShadowCascades.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowCascades.FormattingEnabled = true;
            this.comboBoxShadowCascades.Items.AddRange(new object[] {
            "2 Cascades",
            "3 Cascades",
            "4 Cascades"});
            this.comboBoxShadowCascades.Location = new System.Drawing.Point(160, 84);
            this.comboBoxShadowCascades.Name = "comboBoxShadowCascades";
            this.comboBoxShadowCascades.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShadowCascades.TabIndex = 32;
            // 
            // labelShadowCascades
            // 
            this.labelShadowCascades.AutoSize = true;
            this.labelShadowCascades.Location = new System.Drawing.Point(6, 87);
            this.labelShadowCascades.Name = "labelShadowCascades";
            this.labelShadowCascades.Size = new System.Drawing.Size(57, 13);
            this.labelShadowCascades.TabIndex = 33;
            this.labelShadowCascades.Text = "Cascades:";
            // 
            // numericUpDownShadowStrength
            // 
            this.numericUpDownShadowStrength.Location = new System.Drawing.Point(160, 114);
            this.numericUpDownShadowStrength.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownShadowStrength.Name = "numericUpDownShadowStrength";
            this.numericUpDownShadowStrength.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowStrength.TabIndex = 34;
            // 
            // labelShadowStrength
            // 
            this.labelShadowStrength.AutoSize = true;
            this.labelShadowStrength.Location = new System.Drawing.Point(6, 116);
            this.labelShadowStrength.Name = "labelShadowStrength";
            this.labelShadowStrength.Size = new System.Drawing.Size(109, 13);
            this.labelShadowStrength.TabIndex = 35;
            this.labelShadowStrength.Text = "Strength (0 - 100%):";
            // 
            // labelSunDirection
            // 
            this.labelSunDirection.AutoSize = true;
            this.labelSunDirection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSunDirection.Location = new System.Drawing.Point(6, 220);
            this.labelSunDirection.Name = "labelSunDirection";
            this.labelSunDirection.Size = new System.Drawing.Size(96, 15);
            this.labelSunDirection.TabIndex = 36;
            this.labelSunDirection.Text = "Sun Direction";
            // 
            // labelSunAzimuth
            // 
            this.labelSunAzimuth.AutoSize = true;
            this.labelSunAzimuth.Location = new System.Drawing.Point(6, 242);
            this.labelSunAzimuth.Name = "labelSunAzimuth";
            this.labelSunAzimuth.Size = new System.Drawing.Size(47, 13);
            this.labelSunAzimuth.TabIndex = 37;
            this.labelSunAzimuth.Text = "Azimuth (Phi):";
            // 
            // trackBarSunAzimuth
            // 
            this.trackBarSunAzimuth.Location = new System.Drawing.Point(6, 258);
            this.trackBarSunAzimuth.Maximum = 180;
            this.trackBarSunAzimuth.Minimum = -180;
            this.trackBarSunAzimuth.Name = "trackBarSunAzimuth";
            this.trackBarSunAzimuth.Size = new System.Drawing.Size(235, 45);
            this.trackBarSunAzimuth.TabIndex = 38;
            this.trackBarSunAzimuth.TickFrequency = 30;
            this.trackBarSunAzimuth.Value = -26;
            this.trackBarSunAzimuth.Scroll += new System.EventHandler(this.trackBarSunAzimuth_Scroll);
            // 
            // labelSunAzimuthValue
            // 
            this.labelSunAzimuthValue.AutoSize = false;
            this.labelSunAzimuthValue.Location = new System.Drawing.Point(247, 262);
            this.labelSunAzimuthValue.Name = "labelSunAzimuthValue";
            this.labelSunAzimuthValue.Size = new System.Drawing.Size(40, 13);
            this.labelSunAzimuthValue.TabIndex = 39;
            this.labelSunAzimuthValue.Text = "-26°";
            // 
            // labelSunElevation
            // 
            this.labelSunElevation.AutoSize = true;
            this.labelSunElevation.Location = new System.Drawing.Point(6, 306);
            this.labelSunElevation.Name = "labelSunElevation";
            this.labelSunElevation.Size = new System.Drawing.Size(54, 13);
            this.labelSunElevation.TabIndex = 40;
            this.labelSunElevation.Text = "Elevation (Theta):";
            // 
            // trackBarSunElevation
            // 
            this.trackBarSunElevation.Location = new System.Drawing.Point(6, 322);
            this.trackBarSunElevation.Maximum = 90;
            this.trackBarSunElevation.Minimum = -90;
            this.trackBarSunElevation.Name = "trackBarSunElevation";
            this.trackBarSunElevation.Size = new System.Drawing.Size(235, 45);
            this.trackBarSunElevation.TabIndex = 41;
            this.trackBarSunElevation.TickFrequency = 10;
            this.trackBarSunElevation.Value = 60;
            this.trackBarSunElevation.Scroll += new System.EventHandler(this.trackBarSunElevation_Scroll);
            // 
            // labelSunElevationValue
            // 
            this.labelSunElevationValue.AutoSize = false;
            this.labelSunElevationValue.Location = new System.Drawing.Point(247, 326);
            this.labelSunElevationValue.Name = "labelSunElevationValue";
            this.labelSunElevationValue.Size = new System.Drawing.Size(35, 13);
            this.labelSunElevationValue.TabIndex = 42;
            this.labelSunElevationValue.Text = "60°";
            // 
            // labelShadowBias
            // 
            this.labelShadowBias.AutoSize = true;
            this.labelShadowBias.Location = new System.Drawing.Point(6, 146);
            this.labelShadowBias.Name = "labelShadowBias";
            this.labelShadowBias.Size = new System.Drawing.Size(72, 13);
            this.labelShadowBias.TabIndex = 50;
            this.labelShadowBias.Text = "Shadow Bias:";
            // 
            // numericUpDownShadowBias
            // 
            this.numericUpDownShadowBias.DecimalPlaces = 6;
            this.numericUpDownShadowBias.Increment = new decimal(new int[] { 1, 0, 0, 393216 });
            this.numericUpDownShadowBias.Location = new System.Drawing.Point(160, 144);
            this.numericUpDownShadowBias.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDownShadowBias.Name = "numericUpDownShadowBias";
            this.numericUpDownShadowBias.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowBias.TabIndex = 51;
            this.numericUpDownShadowBias.Value = new decimal(new int[] { 50, 0, 0, 393216 });
            // 
            // labelShadowNormalBias
            // 
            this.labelShadowNormalBias.AutoSize = true;
            this.labelShadowNormalBias.Location = new System.Drawing.Point(12, 166);
            this.labelShadowNormalBias.Name = "labelShadowNormalBias";
            this.labelShadowNormalBias.Size = new System.Drawing.Size(102, 13);
            this.labelShadowNormalBias.TabIndex = 52;
            this.labelShadowNormalBias.Text = "Normal Bias:";
            // 
            // numericUpDownShadowNormalBias
            // 
            this.numericUpDownShadowNormalBias.DecimalPlaces = 2;
            this.numericUpDownShadowNormalBias.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numericUpDownShadowNormalBias.Location = new System.Drawing.Point(160, 166);
            this.numericUpDownShadowNormalBias.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numericUpDownShadowNormalBias.Name = "numericUpDownShadowNormalBias";
            this.numericUpDownShadowNormalBias.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowNormalBias.TabIndex = 53;
            this.numericUpDownShadowNormalBias.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // tabPageKeys
            // 
            var tlpKeys = new System.Windows.Forms.TableLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Top,
                Padding = new System.Windows.Forms.Padding(10),
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
                },
                Controls =
                {
                    // Camera Controls
                    { this.labelControls, 0, 0 },
                    { this.comboBoxLeft, 0, 1 },
                    { this.labelLeft, 1, 1 },
                    { this.comboBoxRight, 0, 2 },
                    { this.labelRight, 1, 2 },
                    { this.comboBoxUp, 0, 3 },
                    { this.labelUp, 1, 3 },
                    { this.comboBoxDown, 0, 4 },
                    { this.labelDown, 1, 4 },
                    { this.comboBoxForwards, 0, 5 },
                    { this.labelForwards, 1, 5 },
                    { this.comboBoxBackwards, 0, 6 },
                    { this.labelBackwards, 1, 6 }
                }
            };
            tlpKeys.SetColumnSpan(this.labelControls, 2);
            this.tabPageKeys.Controls.Add(tlpKeys);
            this.tabPageKeys.AutoScroll = true;
            this.tabPageKeys.Location = new System.Drawing.Point(4, 22);
            this.tabPageKeys.Name = "tabPageKeys";
            this.tabPageKeys.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageKeys.Size = new System.Drawing.Size(304, 348);
            this.tabPageKeys.TabIndex = 1;
            this.tabPageKeys.Text = "Keys";
            this.tabPageKeys.UseVisualStyleBackColor = true;
            // 
            // labelControls
            // 
            this.labelControls.AutoSize = true;
            this.labelControls.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControls.Location = new System.Drawing.Point(8, 7);
            this.labelControls.Name = "labelControls";
            this.labelControls.Size = new System.Drawing.Size(99, 13);
            this.labelControls.TabIndex = 12;
            this.labelControls.Text = "Camera Controls";
            // 
            // comboBoxBackwards
            // 
            this.comboBoxBackwards.FormattingEnabled = true;
            this.comboBoxBackwards.Location = new System.Drawing.Point(6, 168);
            this.comboBoxBackwards.Name = "comboBoxBackwards";
            this.comboBoxBackwards.Size = new System.Drawing.Size(126, 21);
            this.comboBoxBackwards.TabIndex = 11;
            // 
            // labelBackwards
            // 
            this.labelBackwards.AutoSize = true;
            this.labelBackwards.Location = new System.Drawing.Point(138, 176);
            this.labelBackwards.Name = "labelBackwards";
            this.labelBackwards.Size = new System.Drawing.Size(60, 13);
            this.labelBackwards.TabIndex = 10;
            this.labelBackwards.Text = "Backwards";
            // 
            // comboBoxForwards
            // 
            this.comboBoxForwards.FormattingEnabled = true;
            this.comboBoxForwards.Location = new System.Drawing.Point(6, 141);
            this.comboBoxForwards.Name = "comboBoxForwards";
            this.comboBoxForwards.Size = new System.Drawing.Size(126, 21);
            this.comboBoxForwards.TabIndex = 9;
            // 
            // labelForwards
            // 
            this.labelForwards.AutoSize = true;
            this.labelForwards.Location = new System.Drawing.Point(138, 149);
            this.labelForwards.Name = "labelForwards";
            this.labelForwards.Size = new System.Drawing.Size(50, 13);
            this.labelForwards.TabIndex = 8;
            this.labelForwards.Text = "Forwards";
            // 
            // comboBoxDown
            // 
            this.comboBoxDown.FormattingEnabled = true;
            this.comboBoxDown.Location = new System.Drawing.Point(6, 114);
            this.comboBoxDown.Name = "comboBoxDown";
            this.comboBoxDown.Size = new System.Drawing.Size(126, 21);
            this.comboBoxDown.TabIndex = 7;
            // 
            // labelDown
            // 
            this.labelDown.AutoSize = true;
            this.labelDown.Location = new System.Drawing.Point(138, 122);
            this.labelDown.Name = "labelDown";
            this.labelDown.Size = new System.Drawing.Size(35, 13);
            this.labelDown.TabIndex = 6;
            this.labelDown.Text = "Down";
            // 
            // comboBoxUp
            // 
            this.comboBoxUp.FormattingEnabled = true;
            this.comboBoxUp.Location = new System.Drawing.Point(6, 87);
            this.comboBoxUp.Name = "comboBoxUp";
            this.comboBoxUp.Size = new System.Drawing.Size(126, 21);
            this.comboBoxUp.TabIndex = 5;
            // 
            // labelUp
            // 
            this.labelUp.AutoSize = true;
            this.labelUp.Location = new System.Drawing.Point(138, 95);
            this.labelUp.Name = "labelUp";
            this.labelUp.Size = new System.Drawing.Size(21, 13);
            this.labelUp.TabIndex = 4;
            this.labelUp.Text = "Up";
            // 
            // comboBoxRight
            // 
            this.comboBoxRight.FormattingEnabled = true;
            this.comboBoxRight.Location = new System.Drawing.Point(6, 60);
            this.comboBoxRight.Name = "comboBoxRight";
            this.comboBoxRight.Size = new System.Drawing.Size(126, 21);
            this.comboBoxRight.TabIndex = 3;
            // 
            // labelRight
            // 
            this.labelRight.AutoSize = true;
            this.labelRight.Location = new System.Drawing.Point(138, 68);
            this.labelRight.Name = "labelRight";
            this.labelRight.Size = new System.Drawing.Size(32, 13);
            this.labelRight.TabIndex = 2;
            this.labelRight.Text = "Right";
            // 
            // comboBoxLeft
            // 
            this.comboBoxLeft.FormattingEnabled = true;
            this.comboBoxLeft.Location = new System.Drawing.Point(6, 33);
            this.comboBoxLeft.Name = "comboBoxLeft";
            this.comboBoxLeft.Size = new System.Drawing.Size(126, 21);
            this.comboBoxLeft.TabIndex = 1;
            // 
            // labelLeft
            // 
            this.labelLeft.AutoSize = true;
            this.labelLeft.Location = new System.Drawing.Point(138, 41);
            this.labelLeft.Name = "labelLeft";
            this.labelLeft.Size = new System.Drawing.Size(25, 13);
            this.labelLeft.TabIndex = 0;
            this.labelLeft.Text = "Left";
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(268, 433);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 32;
            this.CloseButton.Text = "OK";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // labelAutoReloadChanged
            // 
            this.labelAutoReloadChanged.AutoSize = true;
            this.labelAutoReloadChanged.Location = new System.Drawing.Point(10, 353);
            this.labelAutoReloadChanged.Name = "labelAutoReloadChanged";
            this.labelAutoReloadChanged.Size = new System.Drawing.Size(201, 13);
            this.labelAutoReloadChanged.TabIndex = 48;
            this.labelAutoReloadChanged.Text = "Automatically Reload Changed Objects:";
            // 
            // checkBoxAutoReload
            // 
            this.checkBoxAutoReload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxAutoReload.AutoSize = true;
            this.checkBoxAutoReload.Location = new System.Drawing.Point(265, 353);
            this.checkBoxAutoReload.Name = "checkBoxAutoReload";
            this.checkBoxAutoReload.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoReload.TabIndex = 49;
            this.checkBoxAutoReload.UseVisualStyleBackColor = true;
            // 
            // labelVSync
            // 
            this.labelVSync.AutoSize = true;
            this.labelVSync.Location = new System.Drawing.Point(10, 253);
            this.labelVSync.Name = "labelVSync";
            this.labelVSync.Size = new System.Drawing.Size(120, 13);
            this.labelVSync.TabIndex = 56;
            this.labelVSync.Text = "Vertical Synchronization:";
            // 
            // comboBoxVSync
            // 
            this.comboBoxVSync.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxVSync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVSync.FormattingEnabled = true;
            this.comboBoxVSync.Items.AddRange(new object[] {
            "Off",
            "On"});
            this.comboBoxVSync.Location = new System.Drawing.Point(160, 253);
            this.comboBoxVSync.Name = "comboBoxVSync";
            this.comboBoxVSync.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVSync.TabIndex = 57;
            this.comboBoxVSync.SelectedIndexChanged += new System.EventHandler(this.comboBoxVSync_SelectedIndexChanged);
            // 
            // labelFPSLimit
            // 
            this.labelFPSLimit.AutoSize = true;
            this.labelFPSLimit.Location = new System.Drawing.Point(10, 279);
            this.labelFPSLimit.Name = "labelFPSLimit";
            this.labelFPSLimit.Size = new System.Drawing.Size(80, 13);
            this.labelFPSLimit.TabIndex = 58;
            this.labelFPSLimit.Text = "FPS Limit:";
            // 
            // comboBoxFPSLimit
            // 
            this.comboBoxFPSLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxFPSLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFPSLimit.FormattingEnabled = true;
            this.comboBoxFPSLimit.Items.AddRange(new object[] {
            "Unlimited",
            "30",
            "45",
            "60",
            "75",
            "120",
            "240"});
            this.comboBoxFPSLimit.Location = new System.Drawing.Point(160, 279);
            this.comboBoxFPSLimit.Name = "comboBoxFPSLimit";
            this.comboBoxFPSLimit.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFPSLimit.TabIndex = 59;
            this.toolTip1.SetToolTip(this.labelVSync, "Enable vertical synchronization to prevent screen tearing.\nWhen enabled, FPS is capped to your monitor's refresh rate (e.g. 60Hz = 60 FPS).\nFPS Limit is disabled when VSync is ON.");
            this.toolTip1.SetToolTip(this.comboBoxVSync, "ON = sync to monitor refresh rate (e.g. 60Hz monitor → 60 FPS max)\nOFF = uncapped, use FPS Limit below to cap.");
            this.toolTip1.SetToolTip(this.labelFPSLimit, "Cap the maximum frames per second.\nDisabled when VSync is ON.");
            this.toolTip1.SetToolTip(this.comboBoxFPSLimit, "Select FPS cap. 'Unlimited' = no limit.\nDisabled when VSync is ON (monitor refresh rate is used instead).");
            // 
            // labelShadowFilterCascades
            // 
            this.labelShadowFilterCascades.AutoSize = true;
            this.labelShadowFilterCascades.Location = new System.Drawing.Point(6, 190);
            this.labelShadowFilterCascades.Name = "labelShadowFilterCascades";
            this.labelShadowFilterCascades.Size = new System.Drawing.Size(126, 13);
            this.labelShadowFilterCascades.TabIndex = 54;
            this.labelShadowFilterCascades.Text = "Per-cascade culling:";
            // 
            // checkBoxShadowFilterCascades
            // 
            this.checkBoxShadowFilterCascades.AutoSize = true;
            this.checkBoxShadowFilterCascades.Location = new System.Drawing.Point(160, 190);
            this.checkBoxShadowFilterCascades.Name = "checkBoxShadowFilterCascades";
            this.checkBoxShadowFilterCascades.Size = new System.Drawing.Size(15, 14);
            this.checkBoxShadowFilterCascades.TabIndex = 55;
            this.checkBoxShadowFilterCascades.UseVisualStyleBackColor = true;
            // 
            // formOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(350, 480);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(320, 420);
            this.Name = "formOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Options";
            this.tabControl1.ResumeLayout(false);
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnisotropicLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
            this.tabPageShadows.ResumeLayout(false);
            this.tabPageShadows.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowStrength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunAzimuth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunElevation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowBias)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowNormalBias)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nearClip)).EndInit();
            this.tabPageKeys.ResumeLayout(false);
            this.tabPageKeys.PerformLayout();
            this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageOptions;
		private System.Windows.Forms.ComboBox comboBoxOptimizeObjects;
		private System.Windows.Forms.Label labelOptimizeObjects;
		private System.Windows.Forms.ComboBox comboBoxNewObjParser;
		private System.Windows.Forms.Label labelUseNewObjParser;
		private System.Windows.Forms.ComboBox comboBoxNewXParser;
		private System.Windows.Forms.Label labelUseNewXParser;
		private System.Windows.Forms.Label labelOtherSettings;
		private System.Windows.Forms.Label labelTransparencyQuality;
		private System.Windows.Forms.ComboBox TransparencyQuality;
		private System.Windows.Forms.NumericUpDown AntialiasingLevel;
		private System.Windows.Forms.NumericUpDown AnisotropicLevel;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.NumericUpDown height;
		private System.Windows.Forms.NumericUpDown width;
		private System.Windows.Forms.Label labelResolutionSettings;
		private System.Windows.Forms.Label labelAntialisingLevel;
		private System.Windows.Forms.Label labelAnisotropicFilteringLevel;
		private System.Windows.Forms.Label labelInterpolationMode;
		private System.Windows.Forms.Label labelInterpolationSettings;
		private System.Windows.Forms.ComboBox InterpolationMode;
		private System.Windows.Forms.TabPage tabPageKeys;
		private System.Windows.Forms.ComboBox comboBoxBackwards;
		private System.Windows.Forms.Label labelBackwards;
		private System.Windows.Forms.ComboBox comboBoxForwards;
		private System.Windows.Forms.Label labelForwards;
		private System.Windows.Forms.ComboBox comboBoxDown;
		private System.Windows.Forms.Label labelDown;
		private System.Windows.Forms.ComboBox comboBoxUp;
		private System.Windows.Forms.Label labelUp;
		private System.Windows.Forms.ComboBox comboBoxRight;
		private System.Windows.Forms.Label labelRight;
		private System.Windows.Forms.ComboBox comboBoxLeft;
		private System.Windows.Forms.Label labelLeft;
		private System.Windows.Forms.Label labelNearClip;
		private System.Windows.Forms.NumericUpDown nearClip;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Label labelControls;
		private System.Windows.Forms.TabPage tabPageShadows;
		private System.Windows.Forms.ComboBox comboBoxShadowResolution;
		private System.Windows.Forms.Label labelShadowResolution;
		private System.Windows.Forms.ComboBox comboBoxShadowDistance;
		private System.Windows.Forms.Label labelShadowDistance;
		private System.Windows.Forms.ComboBox comboBoxShadowCascades;
		private System.Windows.Forms.Label labelShadowCascades;
		private System.Windows.Forms.NumericUpDown numericUpDownShadowStrength;
		private System.Windows.Forms.Label labelShadowStrength;
		private System.Windows.Forms.Label labelSunDirection;
		private System.Windows.Forms.Label labelSunAzimuth;
		private System.Windows.Forms.TrackBar trackBarSunAzimuth;
		private System.Windows.Forms.Label labelSunAzimuthValue;
		private System.Windows.Forms.Label labelSunElevation;
		private System.Windows.Forms.TrackBar trackBarSunElevation;
		private System.Windows.Forms.Label labelSunElevationValue;
		private System.Windows.Forms.Label labelShadowBias;
		private System.Windows.Forms.NumericUpDown numericUpDownShadowBias;
		private System.Windows.Forms.Label labelShadowNormalBias;
		private System.Windows.Forms.NumericUpDown numericUpDownShadowNormalBias;
		private System.Windows.Forms.CheckBox checkBoxAutoReload;
		private System.Windows.Forms.Label labelAutoReloadChanged;
		private System.Windows.Forms.Label labelShadowFilterCascades;
		private System.Windows.Forms.CheckBox checkBoxShadowFilterCascades;
		private System.Windows.Forms.Label labelVSync;
		private System.Windows.Forms.ComboBox comboBoxVSync;
		private System.Windows.Forms.Label labelFPSLimit;
		private System.Windows.Forms.ComboBox comboBoxFPSLimit;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
