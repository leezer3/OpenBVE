namespace RouteViewer
{
    partial class FormOptions
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
            this.InterpolationLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.InterpolationMode = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AnisotropicLevel = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.AntialiasingLevel = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.TransparencyQuality = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.width = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.height = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.checkBoxLogo = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxBackgrounds = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.checkBoxProgressBar = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxNewXParser = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.comboBoxNewObjParser = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.numericUpDownViewingDistance = new System.Windows.Forms.NumericUpDown();
            this.labelOptimizeObjects = new System.Windows.Forms.Label();
            this.comboBoxOptimizeObjects = new System.Windows.Forms.ComboBox();
            this.labelVSync = new System.Windows.Forms.Label();
            this.comboBoxVSync = new System.Windows.Forms.ComboBox();
            this.labelFPSLimit = new System.Windows.Forms.Label();
            this.comboBoxFPSLimit = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip();
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
            this.labelNearClip = new System.Windows.Forms.Label();
            this.numericUpDownNearClip = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AnisotropicLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownViewingDistance)).BeginInit();
            this.tabPageShadows.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowStrength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunAzimuth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunElevation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowBias)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowNormalBias)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNearClip)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageOptions);
            this.tabControl1.Controls.Add(this.tabPageShadows);
            this.tabControl1.Location = new System.Drawing.Point(1, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(348, 476);
            this.tabControl1.TabIndex = 50;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.AutoScroll = true;
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
                    { this.InterpolationLabel, 0, 0 },
                    { this.label1, 0, 1 },
                    { this.InterpolationMode, 1, 1 },
                    { this.label2, 0, 2 },
                    { this.AnisotropicLevel, 1, 2 },
                    { this.label3, 0, 3 },
                    { this.AntialiasingLevel, 1, 3 },
                    { this.label4, 0, 4 },
                    { this.TransparencyQuality, 1, 4 },
                    // Resolution Settings
                    { this.label5, 0, 5 },
                    { this.label6, 0, 6 },
                    { this.width, 1, 6 },
                    { this.label7, 0, 7 },
                    { this.height, 1, 7 },
                    // Display Settings
                    { this.label8, 0, 8 },
                    { this.label9, 0, 9 },
                    { this.checkBoxLogo, 1, 9 },
                    { this.label10, 0, 10 },
                    { this.checkBoxBackgrounds, 1, 10 },
                    { this.label11, 0, 11 },
                    { this.checkBoxProgressBar, 1, 11 },
                    // Parser Settings
                    { this.label13, 0, 12 },
                    { this.label12, 0, 13 },
                    { this.comboBoxNewXParser, 1, 13 },
                    { this.label14, 0, 14 },
                    { this.comboBoxNewObjParser, 1, 14 },
                    // Viewing Settings
                    { this.label15, 0, 15 },
                    { this.numericUpDownViewingDistance, 1, 15 },
                    { this.labelNearClip, 0, 16 },
                    { this.numericUpDownNearClip, 1, 16 },
                    // Object Optimization
                    { this.labelOptimizeObjects, 0, 17 },
                    { this.comboBoxOptimizeObjects, 1, 17 },
                    // Display
                    { this.labelVSync, 0, 18 },
                    { this.comboBoxVSync, 1, 18 },
                    { this.labelFPSLimit, 0, 19 },
                    { this.comboBoxFPSLimit, 1, 19 }
                }
            };
            tlpOptions.SetColumnSpan(this.InterpolationLabel, 2);
            tlpOptions.SetColumnSpan(this.label5, 2);
            tlpOptions.SetColumnSpan(this.label8, 2);
            tlpOptions.SetColumnSpan(this.label13, 2);
            this.tabPageOptions.Controls.Add(tlpOptions);
            this.tabPageOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(304, 450);
            this.tabPageOptions.TabIndex = 0;
            this.tabPageOptions.Text = "Options";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // InterpolationLabel
            // 
            this.InterpolationLabel.AutoSize = true;
            this.InterpolationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InterpolationLabel.Location = new System.Drawing.Point(93, 3);
            this.InterpolationLabel.Name = "InterpolationLabel";
            this.InterpolationLabel.Size = new System.Drawing.Size(144, 15);
            this.InterpolationLabel.TabIndex = 1;
            this.InterpolationLabel.Text = "Interpolation Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mode:";
            // 
            // InterpolationMode
            // 
            this.InterpolationMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InterpolationMode.AutoSize = true;
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
            this.InterpolationMode.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Anisotropic Filtering Level:";
            // 
            // AnisotropicLevel
            // 
            this.AnisotropicLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AnisotropicLevel.AutoSize = false;
            this.AnisotropicLevel.Location = new System.Drawing.Point(160, 52);
            this.AnisotropicLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.AnisotropicLevel.Name = "AnisotropicLevel";
            this.AnisotropicLevel.Size = new System.Drawing.Size(120, 20);
            this.AnisotropicLevel.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Anitaliasing Level:";
            // 
            // AntialiasingLevel
            // 
            this.AntialiasingLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AntialiasingLevel.AutoSize = false;
            this.AntialiasingLevel.Location = new System.Drawing.Point(160, 77);
            this.AntialiasingLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.AntialiasingLevel.Name = "AntialiasingLevel";
            this.AntialiasingLevel.Size = new System.Drawing.Size(120, 20);
            this.AntialiasingLevel.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Transparency Quality:";
            // 
            // TransparencyQuality
            // 
            this.TransparencyQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransparencyQuality.AutoSize = true;
            this.TransparencyQuality.FormattingEnabled = true;
            this.TransparencyQuality.Items.AddRange(new object[] {
            "Sharp",
            "Intermediate",
            "Smooth"});
            this.TransparencyQuality.Location = new System.Drawing.Point(160, 106);
            this.TransparencyQuality.Name = "TransparencyQuality";
            this.TransparencyQuality.Size = new System.Drawing.Size(121, 21);
            this.TransparencyQuality.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(93, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Resolution Settings";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Width:";
            // 
            // width
            // 
            this.width.Dock = System.Windows.Forms.DockStyle.Fill;
            this.width.AutoSize = false;
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
            this.width.TabIndex = 11;
            this.width.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 191);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Height:";
            // 
            // height
            // 
            this.height.Dock = System.Windows.Forms.DockStyle.Fill;
            this.height.AutoSize = false;
            this.height.Location = new System.Drawing.Point(160, 189);
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
            this.height.TabIndex = 12;
            this.height.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(93, 222);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(164, 15);
            this.label8.TabIndex = 17;
            this.label8.Text = "Loading Screen Settings";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 247);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Show Logo:";
            // 
            // checkBoxLogo
            // 
            this.checkBoxLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLogo.AutoSize = true;
            this.checkBoxLogo.Location = new System.Drawing.Point(248, 247);
            this.checkBoxLogo.Name = "checkBoxLogo";
            this.checkBoxLogo.Size = new System.Drawing.Size(15, 14);
            this.checkBoxLogo.TabIndex = 21;
            this.checkBoxLogo.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 270);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Show Backgrounds:";
            // 
            // checkBoxBackgrounds
            // 
            this.checkBoxBackgrounds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxBackgrounds.AutoSize = true;
            this.checkBoxBackgrounds.Location = new System.Drawing.Point(248, 270);
            this.checkBoxBackgrounds.Name = "checkBoxBackgrounds";
            this.checkBoxBackgrounds.Size = new System.Drawing.Size(15, 14);
            this.checkBoxBackgrounds.TabIndex = 22;
            this.checkBoxBackgrounds.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 293);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "Show Progress Bar:";
            // 
            // checkBoxProgressBar
            // 
            this.checkBoxProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxProgressBar.AutoSize = true;
            this.checkBoxProgressBar.Location = new System.Drawing.Point(248, 293);
            this.checkBoxProgressBar.Name = "checkBoxProgressBar";
            this.checkBoxProgressBar.Size = new System.Drawing.Size(15, 14);
            this.checkBoxProgressBar.TabIndex = 23;
            this.checkBoxProgressBar.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(93, 318);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(42, 15);
            this.label13.TabIndex = 26;
            this.label13.Text = "Other";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 340);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(97, 13);
            this.label12.TabIndex = 32;
            this.label12.Text = "Use New X Parser:";
            // 
            // comboBoxNewXParser
            // 
            this.comboBoxNewXParser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxNewXParser.AutoSize = true;
            this.comboBoxNewXParser.FormattingEnabled = true;
            this.comboBoxNewXParser.Items.AddRange(new object[] {
            "OriginalXParser",
            "NewXParser",
            "AssimpXParser"});
            this.comboBoxNewXParser.Location = new System.Drawing.Point(160, 340);
            this.comboBoxNewXParser.Name = "comboBoxNewXParser";
            this.comboBoxNewXParser.Size = new System.Drawing.Size(121, 21);
            this.comboBoxNewXParser.TabIndex = 31;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 366);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(106, 13);
            this.label14.TabIndex = 28;
            this.label14.Text = "Use New Obj Parser:";
            // 
            // comboBoxNewObjParser
            // 
            this.comboBoxNewObjParser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxNewObjParser.AutoSize = true;
            this.comboBoxNewObjParser.FormattingEnabled = true;
            this.comboBoxNewObjParser.Items.AddRange(new object[] {
            "OriginalObjParser",
            "AssimpObjParser"});
            this.comboBoxNewObjParser.Location = new System.Drawing.Point(160, 366);
            this.comboBoxNewObjParser.Name = "comboBoxNewObjParser";
            this.comboBoxNewObjParser.Size = new System.Drawing.Size(121, 21);
            this.comboBoxNewObjParser.TabIndex = 27;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 393);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(109, 13);
            this.label15.TabIndex = 34;
            this.label15.Text = "Viewing Distance (m):";
            // 
            // numericUpDownViewingDistance
            // 
            this.numericUpDownViewingDistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownViewingDistance.AutoSize = false;
            this.numericUpDownViewingDistance.Location = new System.Drawing.Point(160, 391);
            this.numericUpDownViewingDistance.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.numericUpDownViewingDistance.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownViewingDistance.Name = "numericUpDownViewingDistance";
            this.numericUpDownViewingDistance.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownViewingDistance.TabIndex = 33;
            this.numericUpDownViewingDistance.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // tabPageShadows
            // 
            this.tabPageShadows.AutoScroll = true;
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
            var panelAzimuth = new System.Windows.Forms.TableLayoutPanel
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
            tlpShadows.Controls.Add(panelAzimuth, 0, 9);
            tlpShadows.SetColumnSpan(panelAzimuth, 2);
            var panelElevation = new System.Windows.Forms.TableLayoutPanel
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
            tlpShadows.Controls.Add(panelElevation, 0, 11);
            tlpShadows.SetColumnSpan(panelElevation, 2);
            this.tabPageShadows.Controls.Add(tlpShadows);
            this.tabPageShadows.Location = new System.Drawing.Point(4, 22);
            this.tabPageShadows.Name = "tabPageShadows";
            this.tabPageShadows.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageShadows.Size = new System.Drawing.Size(304, 440);
            this.tabPageShadows.TabIndex = 1;
            this.tabPageShadows.Text = "Shadow && Lighting";
            this.tabPageShadows.UseVisualStyleBackColor = true;
            // 
            // comboBoxShadowResolution
            // 
            this.comboBoxShadowResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowResolution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxShadowResolution.AutoSize = true;
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
            this.comboBoxShadowDistance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxShadowDistance.AutoSize = true;
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
            this.labelShadowDistance.Size = new System.Drawing.Size(80, 13);
            this.labelShadowDistance.TabIndex = 31;
            this.labelShadowDistance.Text = "Draw Distance:";
            // 
            // comboBoxShadowCascades
            // 
            this.comboBoxShadowCascades.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShadowCascades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxShadowCascades.AutoSize = true;
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
            this.numericUpDownShadowStrength.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownShadowStrength.AutoSize = false;
            this.numericUpDownShadowStrength.Location = new System.Drawing.Point(160, 114);
            this.numericUpDownShadowStrength.Name = "numericUpDownShadowStrength";
            this.numericUpDownShadowStrength.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowStrength.TabIndex = 34;
            // 
            // labelShadowStrength
            // 
            this.labelShadowStrength.AutoSize = true;
            this.labelShadowStrength.Location = new System.Drawing.Point(6, 116);
            this.labelShadowStrength.Name = "labelShadowStrength";
            this.labelShadowStrength.Size = new System.Drawing.Size(100, 13);
            this.labelShadowStrength.TabIndex = 35;
            this.labelShadowStrength.Text = "Strength (0 - 100%):";
            // 
            // labelSunDirection
            // 
            this.labelSunDirection.AutoSize = true;
            this.labelSunDirection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSunDirection.Location = new System.Drawing.Point(6, 220);
            this.labelSunDirection.Name = "labelSunDirection";
            this.labelSunDirection.Size = new System.Drawing.Size(94, 15);
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
            this.labelShadowBias.Location = new System.Drawing.Point(6, 150);
            this.labelShadowBias.Name = "labelShadowBias";
            this.labelShadowBias.Size = new System.Drawing.Size(72, 13);
            this.labelShadowBias.TabIndex = 43;
            this.labelShadowBias.Text = "Shadow Bias:";
            // 
            // numericUpDownShadowBias
            // 
            this.numericUpDownShadowBias.DecimalPlaces = 6;
            this.numericUpDownShadowBias.Increment = new decimal(new int[] {
            1,
            0,
            0,
            393216});
            this.numericUpDownShadowBias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownShadowBias.AutoSize = false;
            this.numericUpDownShadowBias.Location = new System.Drawing.Point(160, 148);
            this.numericUpDownShadowBias.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownShadowBias.Name = "numericUpDownShadowBias";
            this.numericUpDownShadowBias.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowBias.TabIndex = 44;
            this.numericUpDownShadowBias.Value = new decimal(new int[] {
            50,
            0,
            0,
            393216});
            // 
            // labelShadowNormalBias
            // 
            this.labelShadowNormalBias.AutoSize = true;
            this.labelShadowNormalBias.Location = new System.Drawing.Point(6, 172);
            this.labelShadowNormalBias.Name = "labelShadowNormalBias";
            this.labelShadowNormalBias.Size = new System.Drawing.Size(66, 13);
            this.labelShadowNormalBias.TabIndex = 45;
            this.labelShadowNormalBias.Text = "Normal Bias:";
            // 
            // numericUpDownShadowNormalBias
            // 
            this.numericUpDownShadowNormalBias.DecimalPlaces = 2;
            this.numericUpDownShadowNormalBias.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownShadowNormalBias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownShadowNormalBias.AutoSize = false;
            this.numericUpDownShadowNormalBias.Location = new System.Drawing.Point(160, 170);
            this.numericUpDownShadowNormalBias.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownShadowNormalBias.Name = "numericUpDownShadowNormalBias";
            this.numericUpDownShadowNormalBias.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownShadowNormalBias.TabIndex = 46;
            this.numericUpDownShadowNormalBias.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelShadowFilterCascades
            // 
            this.labelShadowFilterCascades.AutoSize = true;
            this.labelShadowFilterCascades.Location = new System.Drawing.Point(6, 194);
            this.labelShadowFilterCascades.Name = "labelShadowFilterCascades";
            this.labelShadowFilterCascades.Size = new System.Drawing.Size(126, 13);
            this.labelShadowFilterCascades.TabIndex = 54;
            this.labelShadowFilterCascades.Text = "Per-cascade culling:";
            // 
            // checkBoxShadowFilterCascades
            // 
            this.checkBoxShadowFilterCascades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxShadowFilterCascades.AutoSize = true;
            this.checkBoxShadowFilterCascades.Location = new System.Drawing.Point(160, 194);
            this.checkBoxShadowFilterCascades.Name = "checkBoxShadowFilterCascades";
            this.checkBoxShadowFilterCascades.Size = new System.Drawing.Size(15, 14);
            this.checkBoxShadowFilterCascades.TabIndex = 55;
            this.checkBoxShadowFilterCascades.UseVisualStyleBackColor = true;
            // 
            // labelNearClip
            // 
            this.labelNearClip.AutoSize = true;
            this.labelNearClip.Location = new System.Drawing.Point(7, 417);
            this.labelNearClip.Name = "labelNearClip";
            this.labelNearClip.Size = new System.Drawing.Size(70, 13);
            this.labelNearClip.TabIndex = 36;
            this.labelNearClip.Text = "Near Clip (m):";
            // 
            // numericUpDownNearClip
            // 
            this.numericUpDownNearClip.DecimalPlaces = 3;
            this.numericUpDownNearClip.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownNearClip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDownNearClip.AutoSize = false;
            this.numericUpDownNearClip.Location = new System.Drawing.Point(161, 417);
            this.numericUpDownNearClip.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownNearClip.Name = "numericUpDownNearClip";
            this.numericUpDownNearClip.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownNearClip.TabIndex = 35;
            this.numericUpDownNearClip.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // labelOptimizeObjects
            // 
            this.labelOptimizeObjects.AutoSize = true;
            this.labelOptimizeObjects.Location = new System.Drawing.Point(6, 443);
            this.labelOptimizeObjects.Name = "labelOptimizeObjects";
            this.labelOptimizeObjects.Size = new System.Drawing.Size(131, 13);
            this.labelOptimizeObjects.TabIndex = 56;
            this.labelOptimizeObjects.Text = "Object Optimization Mode:";
            // 
            // comboBoxOptimizeObjects
            // 
            this.comboBoxOptimizeObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxOptimizeObjects.FormattingEnabled = true;
            this.comboBoxOptimizeObjects.Items.AddRange(new object[] {
            "None",
            "Low",
            "High"});
            this.comboBoxOptimizeObjects.Location = new System.Drawing.Point(160, 443);
            this.comboBoxOptimizeObjects.Name = "comboBoxOptimizeObjects";
            this.comboBoxOptimizeObjects.Size = new System.Drawing.Size(121, 21);
            this.comboBoxOptimizeObjects.TabIndex = 57;
            // 
            // labelVSync
            // 
            this.labelVSync.AutoSize = true;
            this.labelVSync.Location = new System.Drawing.Point(10, 469);
            this.labelVSync.Name = "labelVSync";
            this.labelVSync.Size = new System.Drawing.Size(120, 13);
            this.labelVSync.TabIndex = 58;
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
            this.comboBoxVSync.Location = new System.Drawing.Point(160, 469);
            this.comboBoxVSync.Name = "comboBoxVSync";
            this.comboBoxVSync.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVSync.TabIndex = 59;
            this.comboBoxVSync.SelectedIndexChanged += new System.EventHandler(this.comboBoxVSync_SelectedIndexChanged);
            // 
            // labelFPSLimit
            // 
            this.labelFPSLimit.AutoSize = true;
            this.labelFPSLimit.Location = new System.Drawing.Point(10, 495);
            this.labelFPSLimit.Name = "labelFPSLimit";
            this.labelFPSLimit.Size = new System.Drawing.Size(120, 13);
            this.labelFPSLimit.TabIndex = 60;
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
            "60",
            "120",
            "240"});
            this.comboBoxFPSLimit.Location = new System.Drawing.Point(160, 495);
            this.comboBoxFPSLimit.Name = "comboBoxFPSLimit";
            this.comboBoxFPSLimit.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFPSLimit.TabIndex = 61;
            this.toolTip1.SetToolTip(this.labelVSync, "Enable vertical synchronization to prevent screen tearing.\nWhen enabled, FPS is capped to your monitor's refresh rate (e.g. 60Hz = 60 FPS).\nFPS Limit is disabled when VSync is ON.");
            this.toolTip1.SetToolTip(this.comboBoxVSync, "ON = sync to monitor refresh rate (e.g. 60Hz monitor → 60 FPS max)\nOFF = uncapped, use FPS Limit below to cap.");
            this.toolTip1.SetToolTip(this.labelFPSLimit, "Cap the maximum frames per second.\nDisabled when VSync is ON.");
            this.toolTip1.SetToolTip(this.comboBoxFPSLimit, "Select FPS cap. 'Unlimited' = no limit.\nDisabled when VSync is ON (monitor refresh rate is used instead).");
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(268, 504);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(350, 530);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(320, 420);
            this.Name = "FormOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Options";
            this.Shown += new System.EventHandler(this.formOptions_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AnisotropicLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownViewingDistance)).EndInit();
            this.tabPageShadows.ResumeLayout(false);
            this.tabPageShadows.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowStrength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunAzimuth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSunElevation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowBias)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShadowNormalBias)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNearClip)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageOptions;
        private System.Windows.Forms.TabPage tabPageShadows;
        private System.Windows.Forms.ComboBox InterpolationMode;
        private System.Windows.Forms.Label InterpolationLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox TransparencyQuality;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown width;
        private System.Windows.Forms.NumericUpDown height;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown AnisotropicLevel;
        private System.Windows.Forms.NumericUpDown AntialiasingLevel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox checkBoxLogo;
        private System.Windows.Forms.CheckBox checkBoxBackgrounds;
        private System.Windows.Forms.CheckBox checkBoxProgressBar;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBoxNewObjParser;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxNewXParser;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numericUpDownViewingDistance;
        private System.Windows.Forms.Label labelNearClip;
        private System.Windows.Forms.NumericUpDown numericUpDownNearClip;
        private System.Windows.Forms.Label labelShadowResolution;
        private System.Windows.Forms.ComboBox comboBoxShadowResolution;
        private System.Windows.Forms.Label labelShadowDistance;
        private System.Windows.Forms.ComboBox comboBoxShadowDistance;
        private System.Windows.Forms.Label labelShadowCascades;
        private System.Windows.Forms.ComboBox comboBoxShadowCascades;
        private System.Windows.Forms.Label labelShadowStrength;
        private System.Windows.Forms.NumericUpDown numericUpDownShadowStrength;
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
        private System.Windows.Forms.Label labelShadowFilterCascades;
        private System.Windows.Forms.CheckBox checkBoxShadowFilterCascades;
        private System.Windows.Forms.Label labelOptimizeObjects;
        private System.Windows.Forms.ComboBox comboBoxOptimizeObjects;
        private System.Windows.Forms.Label labelVSync;
        private System.Windows.Forms.ComboBox comboBoxVSync;
        private System.Windows.Forms.Label labelFPSLimit;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox comboBoxFPSLimit;
    }
}
