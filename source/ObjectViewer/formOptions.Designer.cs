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
            this.comboBoxNewObjParser = new System.Windows.Forms.ComboBox();
            this.labelUseNewObjParser = new System.Windows.Forms.Label();
            this.comboBoxNewXParser = new System.Windows.Forms.ComboBox();
            this.labelUseNewXParser = new System.Windows.Forms.Label();
            this.labelOtherSettings = new System.Windows.Forms.Label();
            this.labelTransparencyQuality = new System.Windows.Forms.Label();
            this.TransparencyQuality = new System.Windows.Forms.ComboBox();
            this.AntialiasingLevel = new System.Windows.Forms.NumericUpDown();
            this.AnsiotropicLevel = new System.Windows.Forms.NumericUpDown();
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
            this.tabControl1.SuspendLayout();
            this.tabPageOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnsiotropicLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).BeginInit();
            this.tabPageKeys.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageOptions);
            this.tabControl1.Controls.Add(this.tabPageKeys);
            this.tabControl1.Location = new System.Drawing.Point(1, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(312, 374);
            this.tabControl1.TabIndex = 26;
            // 
            // tabPageOptions
            // 
            this.tabPageOptions.Controls.Add(this.comboBoxOptimizeObjects);
            this.tabPageOptions.Controls.Add(this.labelOptimizeObjects);
            this.tabPageOptions.Controls.Add(this.comboBoxNewObjParser);
            this.tabPageOptions.Controls.Add(this.labelUseNewObjParser);
            this.tabPageOptions.Controls.Add(this.comboBoxNewXParser);
            this.tabPageOptions.Controls.Add(this.labelUseNewXParser);
            this.tabPageOptions.Controls.Add(this.labelOtherSettings);
            this.tabPageOptions.Controls.Add(this.labelTransparencyQuality);
            this.tabPageOptions.Controls.Add(this.TransparencyQuality);
            this.tabPageOptions.Controls.Add(this.AntialiasingLevel);
            this.tabPageOptions.Controls.Add(this.AnsiotropicLevel);
            this.tabPageOptions.Controls.Add(this.labelHeight);
            this.tabPageOptions.Controls.Add(this.labelWidth);
            this.tabPageOptions.Controls.Add(this.height);
            this.tabPageOptions.Controls.Add(this.width);
            this.tabPageOptions.Controls.Add(this.labelResolutionSettings);
            this.tabPageOptions.Controls.Add(this.labelAntialisingLevel);
            this.tabPageOptions.Controls.Add(this.labelAnisotropicFilteringLevel);
            this.tabPageOptions.Controls.Add(this.labelInterpolationMode);
            this.tabPageOptions.Controls.Add(this.labelInterpolationSettings);
            this.tabPageOptions.Controls.Add(this.InterpolationMode);
            this.tabPageOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageOptions.Name = "tabPageOptions";
            this.tabPageOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptions.Size = new System.Drawing.Size(304, 348);
            this.tabPageOptions.TabIndex = 0;
            this.tabPageOptions.Text = "Options";
            this.tabPageOptions.UseVisualStyleBackColor = true;
            // 
            // comboBoxOptimizeObjects
            // 
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
            // comboBoxNewObjParser
            // 
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
            // AnsiotropicLevel
            // 
            this.AnsiotropicLevel.Location = new System.Drawing.Point(160, 52);
            this.AnsiotropicLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.AnsiotropicLevel.Name = "AnsiotropicLevel";
            this.AnsiotropicLevel.Size = new System.Drawing.Size(120, 20);
            this.AnsiotropicLevel.TabIndex = 37;
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
            this.labelAnisotropicFilteringLevel.Text = "Ansiotropic Filtering Level:";
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
            // tabPageKeys
            // 
            this.tabPageKeys.Controls.Add(this.labelControls);
            this.tabPageKeys.Controls.Add(this.comboBoxBackwards);
            this.tabPageKeys.Controls.Add(this.labelBackwards);
            this.tabPageKeys.Controls.Add(this.comboBoxForwards);
            this.tabPageKeys.Controls.Add(this.labelForwards);
            this.tabPageKeys.Controls.Add(this.comboBoxDown);
            this.tabPageKeys.Controls.Add(this.labelDown);
            this.tabPageKeys.Controls.Add(this.comboBoxUp);
            this.tabPageKeys.Controls.Add(this.labelUp);
            this.tabPageKeys.Controls.Add(this.comboBoxRight);
            this.tabPageKeys.Controls.Add(this.labelRight);
            this.tabPageKeys.Controls.Add(this.comboBoxLeft);
            this.tabPageKeys.Controls.Add(this.labelLeft);
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
            this.CloseButton.Location = new System.Drawing.Point(234, 388);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 32;
            this.CloseButton.Text = "OK";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // formOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 421);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.tabControl1);
            this.Name = "formOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Options";
            this.tabControl1.ResumeLayout(false);
            this.tabPageOptions.ResumeLayout(false);
            this.tabPageOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnsiotropicLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
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
		private System.Windows.Forms.NumericUpDown AnsiotropicLevel;
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
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Label labelControls;
	}
}
