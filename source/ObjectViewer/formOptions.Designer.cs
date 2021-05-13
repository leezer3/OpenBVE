namespace OpenBve
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
			this.InterpolationMode = new System.Windows.Forms.ComboBox();
			this.labelInterpolationSettings = new System.Windows.Forms.Label();
			this.labelInterpolationMode = new System.Windows.Forms.Label();
			this.labelAnisotropicFilteringLevel = new System.Windows.Forms.Label();
			this.labelAntialisingLevel = new System.Windows.Forms.Label();
			this.CloseButton = new System.Windows.Forms.Button();
			this.labelResolutionSettings = new System.Windows.Forms.Label();
			this.width = new System.Windows.Forms.NumericUpDown();
			this.height = new System.Windows.Forms.NumericUpDown();
			this.labelWidth = new System.Windows.Forms.Label();
			this.labelHeight = new System.Windows.Forms.Label();
			this.AnsiotropicLevel = new System.Windows.Forms.NumericUpDown();
			this.AntialiasingLevel = new System.Windows.Forms.NumericUpDown();
			this.labelTransparencyQuality = new System.Windows.Forms.Label();
			this.TransparencyQuality = new System.Windows.Forms.ComboBox();
			this.labelOtherSettings = new System.Windows.Forms.Label();
			this.labelUseNewXParser = new System.Windows.Forms.Label();
			this.comboBoxNewXParser = new System.Windows.Forms.ComboBox();
			this.labelUseNewObjParser = new System.Windows.Forms.Label();
			this.comboBoxNewObjParser = new System.Windows.Forms.ComboBox();
			this.labelOptimizeObjects = new System.Windows.Forms.Label();
			this.comboBoxOptimizeObjects = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.width)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.height)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AnsiotropicLevel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).BeginInit();
			this.SuspendLayout();
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
			this.InterpolationMode.Location = new System.Drawing.Point(166, 30);
			this.InterpolationMode.Name = "InterpolationMode";
			this.InterpolationMode.Size = new System.Drawing.Size(121, 21);
			this.InterpolationMode.TabIndex = 0;
			// 
			// labelInterpolationSettings
			// 
			this.labelInterpolationSettings.AutoSize = true;
			this.labelInterpolationSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInterpolationSettings.Location = new System.Drawing.Point(99, 9);
			this.labelInterpolationSettings.Name = "labelInterpolationSettings";
			this.labelInterpolationSettings.Size = new System.Drawing.Size(144, 15);
			this.labelInterpolationSettings.TabIndex = 1;
			this.labelInterpolationSettings.Text = "Interpolation Settings";
			// 
			// labelInterpolationMode
			// 
			this.labelInterpolationMode.AutoSize = true;
			this.labelInterpolationMode.Location = new System.Drawing.Point(12, 30);
			this.labelInterpolationMode.Name = "labelInterpolationMode";
			this.labelInterpolationMode.Size = new System.Drawing.Size(37, 13);
			this.labelInterpolationMode.TabIndex = 2;
			this.labelInterpolationMode.Text = "Mode:";
			// 
			// labelAnisotropicFilteringLevel
			// 
			this.labelAnisotropicFilteringLevel.AutoSize = true;
			this.labelAnisotropicFilteringLevel.Location = new System.Drawing.Point(9, 58);
			this.labelAnisotropicFilteringLevel.Name = "labelAnisotropicFilteringLevel";
			this.labelAnisotropicFilteringLevel.Size = new System.Drawing.Size(130, 13);
			this.labelAnisotropicFilteringLevel.TabIndex = 3;
			this.labelAnisotropicFilteringLevel.Text = "Ansiotropic Filtering Level:";
			// 
			// labelAntialisingLevel
			// 
			this.labelAntialisingLevel.AutoSize = true;
			this.labelAntialisingLevel.Location = new System.Drawing.Point(12, 85);
			this.labelAntialisingLevel.Name = "labelAntialisingLevel";
			this.labelAntialisingLevel.Size = new System.Drawing.Size(92, 13);
			this.labelAntialisingLevel.TabIndex = 6;
			this.labelAntialisingLevel.Text = "Anitaliasing Level:";
			// 
			// CloseButton
			// 
			this.CloseButton.Location = new System.Drawing.Point(212, 351);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "OK";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// labelResolutionSettings
			// 
			this.labelResolutionSettings.AutoSize = true;
			this.labelResolutionSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelResolutionSettings.Location = new System.Drawing.Point(99, 148);
			this.labelResolutionSettings.Name = "labelResolutionSettings";
			this.labelResolutionSettings.Size = new System.Drawing.Size(132, 15);
			this.labelResolutionSettings.TabIndex = 10;
			this.labelResolutionSettings.Text = "Resolution Settings";
			// 
			// width
			// 
			this.width.Location = new System.Drawing.Point(166, 169);
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
			// height
			// 
			this.height.Location = new System.Drawing.Point(167, 195);
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
			// labelWidth
			// 
			this.labelWidth.AutoSize = true;
			this.labelWidth.Location = new System.Drawing.Point(12, 171);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(38, 13);
			this.labelWidth.TabIndex = 13;
			this.labelWidth.Text = "Width:";
			// 
			// labelHeight
			// 
			this.labelHeight.AutoSize = true;
			this.labelHeight.Location = new System.Drawing.Point(12, 197);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(41, 13);
			this.labelHeight.TabIndex = 14;
			this.labelHeight.Text = "Height:";
			// 
			// AnsiotropicLevel
			// 
			this.AnsiotropicLevel.Location = new System.Drawing.Point(166, 58);
			this.AnsiotropicLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AnsiotropicLevel.Name = "AnsiotropicLevel";
			this.AnsiotropicLevel.Size = new System.Drawing.Size(120, 20);
			this.AnsiotropicLevel.TabIndex = 15;
			// 
			// AntialiasingLevel
			// 
			this.AntialiasingLevel.Location = new System.Drawing.Point(166, 83);
			this.AntialiasingLevel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AntialiasingLevel.Name = "AntialiasingLevel";
			this.AntialiasingLevel.Size = new System.Drawing.Size(120, 20);
			this.AntialiasingLevel.TabIndex = 16;
			// 
			// labelTransparencyQuality
			// 
			this.labelTransparencyQuality.AutoSize = true;
			this.labelTransparencyQuality.Location = new System.Drawing.Point(12, 112);
			this.labelTransparencyQuality.Name = "labelTransparencyQuality";
			this.labelTransparencyQuality.Size = new System.Drawing.Size(110, 13);
			this.labelTransparencyQuality.TabIndex = 18;
			this.labelTransparencyQuality.Text = "Transparency Quality:";
			// 
			// TransparencyQuality
			// 
			this.TransparencyQuality.FormattingEnabled = true;
			this.TransparencyQuality.Items.AddRange(new object[] {
            "Sharp",
            "Intermediate",
            "Smooth"});
			this.TransparencyQuality.Location = new System.Drawing.Point(166, 112);
			this.TransparencyQuality.Name = "TransparencyQuality";
			this.TransparencyQuality.Size = new System.Drawing.Size(121, 21);
			this.TransparencyQuality.TabIndex = 17;
			// 
			// labelOtherSettings
			// 
			this.labelOtherSettings.AutoSize = true;
			this.labelOtherSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOtherSettings.Location = new System.Drawing.Point(99, 233);
			this.labelOtherSettings.Name = "labelOtherSettings";
			this.labelOtherSettings.Size = new System.Drawing.Size(42, 15);
			this.labelOtherSettings.TabIndex = 19;
			this.labelOtherSettings.Text = "Other";
			// 
			// labelUseNewXParser
			// 
			this.labelUseNewXParser.AutoSize = true;
			this.labelUseNewXParser.Location = new System.Drawing.Point(15, 255);
			this.labelUseNewXParser.Name = "labelUseNewXParser";
			this.labelUseNewXParser.Size = new System.Drawing.Size(97, 13);
			this.labelUseNewXParser.TabIndex = 20;
			this.labelUseNewXParser.Text = "Use New X Parser:";
			// 
			// comboBoxNewXParser
			// 
			this.comboBoxNewXParser.FormattingEnabled = true;
			this.comboBoxNewXParser.Items.AddRange(new object[] {
            "OriginalXParser",
            "NewXParser",
            "AssimpXParser"});
			this.comboBoxNewXParser.Location = new System.Drawing.Point(166, 255);
			this.comboBoxNewXParser.Name = "comboBoxNewXParser";
			this.comboBoxNewXParser.Size = new System.Drawing.Size(121, 21);
			this.comboBoxNewXParser.TabIndex = 21;
			// 
			// labelUseNewObjParser
			// 
			this.labelUseNewObjParser.AutoSize = true;
			this.labelUseNewObjParser.Location = new System.Drawing.Point(15, 281);
			this.labelUseNewObjParser.Name = "labelUseNewObjParser";
			this.labelUseNewObjParser.Size = new System.Drawing.Size(106, 13);
			this.labelUseNewObjParser.TabIndex = 22;
			this.labelUseNewObjParser.Text = "Use New Obj Parser:";
			// 
			// comboBoxNewObjParser
			// 
			this.comboBoxNewObjParser.FormattingEnabled = true;
			this.comboBoxNewObjParser.Items.AddRange(new object[] {
            "OriginalObjParser",
            "AssimpObjParser"});
			this.comboBoxNewObjParser.Location = new System.Drawing.Point(166, 281);
			this.comboBoxNewObjParser.Name = "comboBoxNewObjParser";
			this.comboBoxNewObjParser.Size = new System.Drawing.Size(121, 21);
			this.comboBoxNewObjParser.TabIndex = 23;
			// 
			// labelOptimizeObjects
			// 
			this.labelOptimizeObjects.AutoSize = true;
			this.labelOptimizeObjects.Location = new System.Drawing.Point(16, 307);
			this.labelOptimizeObjects.Name = "labelOptimizeObjects";
			this.labelOptimizeObjects.Size = new System.Drawing.Size(124, 13);
			this.labelOptimizeObjects.TabIndex = 24;
			this.labelOptimizeObjects.Text = "Object Optimization Mode:";
			// 
			// comboBoxNewOptimizeObjects
			// 
			this.comboBoxOptimizeObjects.FormattingEnabled = true;
			this.comboBoxOptimizeObjects.Items.AddRange(new object[] {
				"None",
				"Low",
				"High"});
			this.comboBoxOptimizeObjects.Location = new System.Drawing.Point(166, 307);
			this.comboBoxOptimizeObjects.Name = "comboBoxOptimizeObjects";
			this.comboBoxOptimizeObjects.Size = new System.Drawing.Size(121, 21);
			this.comboBoxOptimizeObjects.TabIndex = 25;
			// 
			// formOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 386);
			this.Controls.Add(this.comboBoxOptimizeObjects);
			this.Controls.Add(this.labelOptimizeObjects);
			this.Controls.Add(this.comboBoxNewObjParser);
			this.Controls.Add(this.labelUseNewObjParser);
			this.Controls.Add(this.comboBoxNewXParser);
			this.Controls.Add(this.labelUseNewXParser);
			this.Controls.Add(this.labelOtherSettings);
			this.Controls.Add(this.labelTransparencyQuality);
			this.Controls.Add(this.TransparencyQuality);
			this.Controls.Add(this.AntialiasingLevel);
			this.Controls.Add(this.AnsiotropicLevel);
			this.Controls.Add(this.labelHeight);
			this.Controls.Add(this.labelWidth);
			this.Controls.Add(this.height);
			this.Controls.Add(this.width);
			this.Controls.Add(this.labelResolutionSettings);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.labelAntialisingLevel);
			this.Controls.Add(this.labelAnisotropicFilteringLevel);
			this.Controls.Add(this.labelInterpolationMode);
			this.Controls.Add(this.labelInterpolationSettings);
			this.Controls.Add(this.InterpolationMode);
			this.Name = "formOptions";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Options";
			((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.height)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AnsiotropicLevel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox InterpolationMode;
        private System.Windows.Forms.Label labelInterpolationSettings;
        private System.Windows.Forms.Label labelInterpolationMode;
        private System.Windows.Forms.Label labelAnisotropicFilteringLevel;
        private System.Windows.Forms.Label labelAntialisingLevel;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label labelResolutionSettings;
        private System.Windows.Forms.NumericUpDown width;
        private System.Windows.Forms.NumericUpDown height;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown AnsiotropicLevel;
        private System.Windows.Forms.NumericUpDown AntialiasingLevel;
        private System.Windows.Forms.Label labelTransparencyQuality;
        private System.Windows.Forms.ComboBox TransparencyQuality;
		private System.Windows.Forms.Label labelOtherSettings;
		private System.Windows.Forms.Label labelUseNewXParser;
		private System.Windows.Forms.ComboBox comboBoxNewXParser;
		private System.Windows.Forms.Label labelUseNewObjParser;
		private System.Windows.Forms.ComboBox comboBoxNewObjParser;
		private System.Windows.Forms.Label labelOptimizeObjects;
		private System.Windows.Forms.ComboBox comboBoxOptimizeObjects;
	}
}
