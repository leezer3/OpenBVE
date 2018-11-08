namespace DefaultDisplayPlugin
{
	partial class Config
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
            this.checkBoxClock = new System.Windows.Forms.CheckBox();
            this.checkBoxSpeed = new System.Windows.Forms.CheckBox();
            this.checkBoxGradient = new System.Windows.Forms.CheckBox();
            this.checkBoxDistNextStation = new System.Windows.Forms.CheckBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.comboBoxSpeed = new System.Windows.Forms.ComboBox();
            this.comboBoxGradient = new System.Windows.Forms.ComboBox();
            this.comboBoxDistNextStation = new System.Windows.Forms.ComboBox();
            this.checkBoxFps = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // checkBoxClock
            //
            this.checkBoxClock.AutoSize = true;
            this.checkBoxClock.Location = new System.Drawing.Point(13, 13);
            this.checkBoxClock.Name = "checkBoxClock";
            this.checkBoxClock.Size = new System.Drawing.Size(53, 16);
            this.checkBoxClock.TabIndex = 0;
            this.checkBoxClock.Text = "Clock";
            this.checkBoxClock.UseVisualStyleBackColor = true;
            //
            // checkBoxSpeed
            //
            this.checkBoxSpeed.AutoSize = true;
            this.checkBoxSpeed.Location = new System.Drawing.Point(13, 36);
            this.checkBoxSpeed.Name = "checkBoxSpeed";
            this.checkBoxSpeed.Size = new System.Drawing.Size(55, 16);
            this.checkBoxSpeed.TabIndex = 1;
            this.checkBoxSpeed.Text = "Speed";
            this.checkBoxSpeed.UseVisualStyleBackColor = true;
            //
            // checkBoxGradient
            //
            this.checkBoxGradient.AutoSize = true;
            this.checkBoxGradient.Location = new System.Drawing.Point(13, 59);
            this.checkBoxGradient.Name = "checkBoxGradient";
            this.checkBoxGradient.Size = new System.Drawing.Size(67, 16);
            this.checkBoxGradient.TabIndex = 2;
            this.checkBoxGradient.Text = "Gradient";
            this.checkBoxGradient.UseVisualStyleBackColor = true;
            //
            // checkBoxDistNextStation
            //
            this.checkBoxDistNextStation.AutoSize = true;
            this.checkBoxDistNextStation.Location = new System.Drawing.Point(13, 82);
            this.checkBoxDistNextStation.Name = "checkBoxDistNextStation";
            this.checkBoxDistNextStation.Size = new System.Drawing.Size(105, 16);
            this.checkBoxDistNextStation.TabIndex = 3;
            this.checkBoxDistNextStation.Text = "DistNextStation";
            this.checkBoxDistNextStation.UseVisualStyleBackColor = true;
            //
            // buttonSave
            //
            this.buttonSave.Location = new System.Drawing.Point(170, 117);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            //
            // comboBoxSpeed
            //
            this.comboBoxSpeed.FormattingEnabled = true;
            this.comboBoxSpeed.Items.AddRange(new object[] {
            "Kmph",
            "Mph"});
            this.comboBoxSpeed.Location = new System.Drawing.Point(124, 36);
            this.comboBoxSpeed.Name = "comboBoxSpeed";
            this.comboBoxSpeed.Size = new System.Drawing.Size(121, 20);
            this.comboBoxSpeed.TabIndex = 5;
            //
            // comboBoxGradient
            //
            this.comboBoxGradient.FormattingEnabled = true;
            this.comboBoxGradient.Items.AddRange(new object[] {
            "Percentage",
            "Unit of change",
            "Permil"});
            this.comboBoxGradient.Location = new System.Drawing.Point(124, 59);
            this.comboBoxGradient.Name = "comboBoxGradient";
            this.comboBoxGradient.Size = new System.Drawing.Size(121, 20);
            this.comboBoxGradient.TabIndex = 6;
            //
            // comboBoxDistNextStation
            //
            this.comboBoxDistNextStation.FormattingEnabled = true;
            this.comboBoxDistNextStation.Items.AddRange(new object[] {
            "Km",
            "Mile"});
            this.comboBoxDistNextStation.Location = new System.Drawing.Point(124, 82);
            this.comboBoxDistNextStation.Name = "comboBoxDistNextStation";
            this.comboBoxDistNextStation.Size = new System.Drawing.Size(121, 20);
            this.comboBoxDistNextStation.TabIndex = 7;
            //
            // checkBoxFps
            //
            this.checkBoxFps.Location = new System.Drawing.Point(13, 105);
            this.checkBoxFps.Name = "checkBoxFps";
            this.checkBoxFps.Size = new System.Drawing.Size(80, 16);
            this.checkBoxFps.TabIndex = 0;
            this.checkBoxFps.Text = "Fps";
            this.checkBoxFps.UseVisualStyleBackColor = true;
            //
            // Config
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 154);
            this.Controls.Add(this.checkBoxFps);
            this.Controls.Add(this.comboBoxDistNextStation);
            this.Controls.Add(this.comboBoxGradient);
            this.Controls.Add(this.comboBoxSpeed);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.checkBoxDistNextStation);
            this.Controls.Add(this.checkBoxGradient);
            this.Controls.Add(this.checkBoxSpeed);
            this.Controls.Add(this.checkBoxClock);
            this.Name = "Config";
            this.Text = "Config";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBoxClock;
		private System.Windows.Forms.CheckBox checkBoxSpeed;
		private System.Windows.Forms.CheckBox checkBoxGradient;
		private System.Windows.Forms.CheckBox checkBoxDistNextStation;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.ComboBox comboBoxSpeed;
        private System.Windows.Forms.ComboBox comboBoxGradient;
        private System.Windows.Forms.ComboBox comboBoxDistNextStation;
        private System.Windows.Forms.CheckBox checkBoxFps;

	}
}
