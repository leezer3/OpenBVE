namespace TrainEditor2.Views
{
	partial class FormDoor
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
			disposable.Dispose();

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
			this.components = new System.ComponentModel.Container();
			this.textBoxMaxTolerance = new System.Windows.Forms.TextBox();
			this.textBoxWidth = new System.Windows.Forms.TextBox();
			this.labelMaxToleranceUnit = new System.Windows.Forms.Label();
			this.labelWidthUnit = new System.Windows.Forms.Label();
			this.labelMaxTolerance = new System.Windows.Forms.Label();
			this.labelWidth = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxMaxTolerance
			// 
			this.textBoxMaxTolerance.Location = new System.Drawing.Point(104, 32);
			this.textBoxMaxTolerance.Name = "textBoxMaxTolerance";
			this.textBoxMaxTolerance.Size = new System.Drawing.Size(48, 19);
			this.textBoxMaxTolerance.TabIndex = 54;
			// 
			// textBoxWidth
			// 
			this.textBoxWidth.Location = new System.Drawing.Point(104, 8);
			this.textBoxWidth.Name = "textBoxWidth";
			this.textBoxWidth.Size = new System.Drawing.Size(48, 19);
			this.textBoxWidth.TabIndex = 53;
			// 
			// labelMaxToleranceUnit
			// 
			this.labelMaxToleranceUnit.Location = new System.Drawing.Point(160, 32);
			this.labelMaxToleranceUnit.Name = "labelMaxToleranceUnit";
			this.labelMaxToleranceUnit.Size = new System.Drawing.Size(24, 16);
			this.labelMaxToleranceUnit.TabIndex = 52;
			this.labelMaxToleranceUnit.Text = "mm";
			this.labelMaxToleranceUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelWidthUnit
			// 
			this.labelWidthUnit.Location = new System.Drawing.Point(160, 8);
			this.labelWidthUnit.Name = "labelWidthUnit";
			this.labelWidthUnit.Size = new System.Drawing.Size(24, 16);
			this.labelWidthUnit.TabIndex = 51;
			this.labelWidthUnit.Text = "mm";
			this.labelWidthUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelMaxTolerance
			// 
			this.labelMaxTolerance.Location = new System.Drawing.Point(8, 32);
			this.labelMaxTolerance.Name = "labelMaxTolerance";
			this.labelMaxTolerance.Size = new System.Drawing.Size(88, 16);
			this.labelMaxTolerance.TabIndex = 50;
			this.labelMaxTolerance.Text = "MaxTolerance:";
			this.labelMaxTolerance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelWidth
			// 
			this.labelWidth.Location = new System.Drawing.Point(8, 8);
			this.labelWidth.Name = "labelWidth";
			this.labelWidth.Size = new System.Drawing.Size(88, 16);
			this.labelWidth.TabIndex = 49;
			this.labelWidth.Text = "Width:";
			this.labelWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(128, 56);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 55;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormDoor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(193, 89);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxMaxTolerance);
			this.Controls.Add(this.textBoxWidth);
			this.Controls.Add(this.labelMaxToleranceUnit);
			this.Controls.Add(this.labelWidthUnit);
			this.Controls.Add(this.labelMaxTolerance);
			this.Controls.Add(this.labelWidth);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormDoor";
			this.Text = "Door";
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxMaxTolerance;
		private System.Windows.Forms.TextBox textBoxWidth;
		private System.Windows.Forms.Label labelMaxToleranceUnit;
		private System.Windows.Forms.Label labelWidthUnit;
		private System.Windows.Forms.Label labelMaxTolerance;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}