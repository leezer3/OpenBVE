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
			this.InterpolationLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.TransparencyQuality = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.width = new System.Windows.Forms.NumericUpDown();
			this.height = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.AnsiotropicLevel = new System.Windows.Forms.NumericUpDown();
			this.AntialiasingLevel = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.checkBoxLogo = new System.Windows.Forms.CheckBox();
			this.checkBoxBackgrounds = new System.Windows.Forms.CheckBox();
			this.checkBoxProgressBar = new System.Windows.Forms.CheckBox();
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
			// InterpolationLabel
			// 
			this.InterpolationLabel.AutoSize = true;
			this.InterpolationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InterpolationLabel.Location = new System.Drawing.Point(99, 9);
			this.InterpolationLabel.Name = "InterpolationLabel";
			this.InterpolationLabel.Size = new System.Drawing.Size(144, 15);
			this.InterpolationLabel.TabIndex = 1;
			this.InterpolationLabel.Text = "Interpolation Settings";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(37, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Mode:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(130, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Ansiotropic Filtering Level:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 85);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Anitaliasing Level:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 112);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(110, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Transparency Quality:";
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
			this.TransparencyQuality.TabIndex = 7;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(212, 333);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 9;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(99, 148);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(132, 15);
			this.label5.TabIndex = 10;
			this.label5.Text = "Resolution Settings";
			// 
			// width
			// 
			this.width.Location = new System.Drawing.Point(166, 169);
			this.width.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.width.Name = "width";
			this.width.Size = new System.Drawing.Size(120, 20);
			this.width.TabIndex = 11;
			// 
			// height
			// 
			this.height.Location = new System.Drawing.Point(167, 195);
			this.height.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.height.Name = "height";
			this.height.Size = new System.Drawing.Size(120, 20);
			this.height.TabIndex = 12;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 171);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(38, 13);
			this.label6.TabIndex = 13;
			this.label6.Text = "Width:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 197);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(41, 13);
			this.label7.TabIndex = 14;
			this.label7.Text = "Height:";
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
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(99, 232);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(164, 15);
			this.label8.TabIndex = 17;
			this.label8.Text = "Loading Screen Settings";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(15, 257);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(64, 13);
			this.label9.TabIndex = 18;
			this.label9.Text = "Show Logo:";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(15, 280);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(103, 13);
			this.label10.TabIndex = 19;
			this.label10.Text = "Show Backgrounds:";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(15, 303);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(100, 13);
			this.label11.TabIndex = 20;
			this.label11.Text = "Show Progress Bar:";
			// 
			// checkBoxLogo
			// 
			this.checkBoxLogo.AutoSize = true;
			this.checkBoxLogo.Location = new System.Drawing.Point(248, 257);
			this.checkBoxLogo.Name = "checkBoxLogo";
			this.checkBoxLogo.Size = new System.Drawing.Size(15, 14);
			this.checkBoxLogo.TabIndex = 21;
			this.checkBoxLogo.UseVisualStyleBackColor = true;
			// 
			// checkBoxBackgrounds
			// 
			this.checkBoxBackgrounds.AutoSize = true;
			this.checkBoxBackgrounds.Location = new System.Drawing.Point(248, 280);
			this.checkBoxBackgrounds.Name = "checkBoxBackgrounds";
			this.checkBoxBackgrounds.Size = new System.Drawing.Size(15, 14);
			this.checkBoxBackgrounds.TabIndex = 22;
			this.checkBoxBackgrounds.UseVisualStyleBackColor = true;
			// 
			// checkBoxProgressBar
			// 
			this.checkBoxProgressBar.AutoSize = true;
			this.checkBoxProgressBar.Location = new System.Drawing.Point(248, 303);
			this.checkBoxProgressBar.Name = "checkBoxProgressBar";
			this.checkBoxProgressBar.Size = new System.Drawing.Size(15, 14);
			this.checkBoxProgressBar.TabIndex = 23;
			this.checkBoxProgressBar.UseVisualStyleBackColor = true;
			// 
			// formOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 368);
			this.Controls.Add(this.checkBoxProgressBar);
			this.Controls.Add(this.checkBoxBackgrounds);
			this.Controls.Add(this.checkBoxLogo);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.AntialiasingLevel);
			this.Controls.Add(this.AnsiotropicLevel);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.height);
			this.Controls.Add(this.width);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.TransparencyQuality);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.InterpolationLabel);
			this.Controls.Add(this.InterpolationMode);
			this.Name = "formOptions";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Options";
			this.Shown += new System.EventHandler(this.formOptions_Shown);
			((System.ComponentModel.ISupportInitialize)(this.width)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.height)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AnsiotropicLevel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AntialiasingLevel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

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
        private System.Windows.Forms.NumericUpDown AnsiotropicLevel;
        private System.Windows.Forms.NumericUpDown AntialiasingLevel;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox checkBoxLogo;
		private System.Windows.Forms.CheckBox checkBoxBackgrounds;
		private System.Windows.Forms.CheckBox checkBoxProgressBar;
	}
}