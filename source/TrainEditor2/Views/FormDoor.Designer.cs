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
			this.labelMaxTolerance = new System.Windows.Forms.Label();
			this.labelWidth = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.comboBoxWidthUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxMaxToleranceUnit = new System.Windows.Forms.ComboBox();
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
			this.buttonOK.Location = new System.Drawing.Point(152, 56);
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
			// comboBoxWidthUnit
			// 
			this.comboBoxWidthUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxWidthUnit.FormattingEnabled = true;
			this.comboBoxWidthUnit.Location = new System.Drawing.Point(160, 8);
			this.comboBoxWidthUnit.Name = "comboBoxWidthUnit";
			this.comboBoxWidthUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxWidthUnit.TabIndex = 56;
			// 
			// comboBoxMaxToleranceUnit
			// 
			this.comboBoxMaxToleranceUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMaxToleranceUnit.FormattingEnabled = true;
			this.comboBoxMaxToleranceUnit.Location = new System.Drawing.Point(160, 32);
			this.comboBoxMaxToleranceUnit.Name = "comboBoxMaxToleranceUnit";
			this.comboBoxMaxToleranceUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxMaxToleranceUnit.TabIndex = 57;
			// 
			// FormDoor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(217, 89);
			this.Controls.Add(this.comboBoxMaxToleranceUnit);
			this.Controls.Add(this.comboBoxWidthUnit);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxMaxTolerance);
			this.Controls.Add(this.textBoxWidth);
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
		private System.Windows.Forms.Label labelMaxTolerance;
		private System.Windows.Forms.Label labelWidth;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.ComboBox comboBoxMaxToleranceUnit;
		private System.Windows.Forms.ComboBox comboBoxWidthUnit;
	}
}