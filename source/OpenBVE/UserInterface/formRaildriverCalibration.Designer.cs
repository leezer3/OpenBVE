namespace OpenBve.UserInterface
{
	partial class formRaildriverCalibration
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.labelCalibrationText = new System.Windows.Forms.Label();
			this.buttonCalibrationNext = new System.Windows.Forms.Button();
			this.buttonCalibrationPrevious = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(13, 13);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(800, 420);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// labelCalibrationText
			// 
			this.labelCalibrationText.AutoSize = true;
			this.labelCalibrationText.Location = new System.Drawing.Point(12, 445);
			this.labelCalibrationText.Name = "labelCalibrationText";
			this.labelCalibrationText.Size = new System.Drawing.Size(185, 13);
			this.labelCalibrationText.TabIndex = 1;
			this.labelCalibrationText.Text = "Please press \'Next\' to start calibration.";
			// 
			// buttonCalibrationNext
			// 
			this.buttonCalibrationNext.Location = new System.Drawing.Point(737, 445);
			this.buttonCalibrationNext.Name = "buttonCalibrationNext";
			this.buttonCalibrationNext.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrationNext.TabIndex = 2;
			this.buttonCalibrationNext.Text = "Next";
			this.buttonCalibrationNext.UseVisualStyleBackColor = true;
			this.buttonCalibrationNext.Click += new System.EventHandler(this.buttonCalibrationNext_Click);
			// 
			// buttonCalibrationPrevious
			// 
			this.buttonCalibrationPrevious.Location = new System.Drawing.Point(656, 445);
			this.buttonCalibrationPrevious.Name = "buttonCalibrationPrevious";
			this.buttonCalibrationPrevious.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrationPrevious.TabIndex = 3;
			this.buttonCalibrationPrevious.Text = "Previous";
			this.buttonCalibrationPrevious.UseVisualStyleBackColor = true;
			this.buttonCalibrationPrevious.Click += new System.EventHandler(this.buttonCalibrationPrevious_Click);
			// 
			// formRaildriverCalibration
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(827, 480);
			this.Controls.Add(this.buttonCalibrationPrevious);
			this.Controls.Add(this.buttonCalibrationNext);
			this.Controls.Add(this.labelCalibrationText);
			this.Controls.Add(this.pictureBox1);
			this.Name = "formRaildriverCalibration";
			this.Text = "Raildriver Calibration";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formRaildriverCalibration_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label labelCalibrationText;
		private System.Windows.Forms.Button buttonCalibrationNext;
		private System.Windows.Forms.Button buttonCalibrationPrevious;
	}
}