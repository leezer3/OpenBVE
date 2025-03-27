
namespace OpenBve
{
	partial class FormImageExport
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
			this.buttonExport = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.labelInstructions = new System.Windows.Forms.Label();
			this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonBrowse = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonExport
			// 
			this.buttonExport.Location = new System.Drawing.Point(226, 170);
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.Size = new System.Drawing.Size(75, 23);
			this.buttonExport.TabIndex = 0;
			this.buttonExport.Text = "Export";
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(145, 170);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// labelInstructions
			// 
			this.labelInstructions.AutoSize = true;
			this.labelInstructions.Location = new System.Drawing.Point(13, 13);
			this.labelInstructions.Name = "labelInstructions";
			this.labelInstructions.Size = new System.Drawing.Size(276, 13);
			this.labelInstructions.TabIndex = 2;
			this.labelInstructions.Text = "Please enter the width and height for the exported image.";
			// 
			// numericUpDownWidth
			// 
			this.numericUpDownWidth.Location = new System.Drawing.Point(54, 31);
			this.numericUpDownWidth.Maximum = new decimal(new int[] {
			10000,
			0,
			0,
			0});
			this.numericUpDownWidth.Minimum = new decimal(new int[] {
			100,
			0,
			0,
			0});
			this.numericUpDownWidth.Name = "numericUpDownWidth";
			this.numericUpDownWidth.Size = new System.Drawing.Size(120, 20);
			this.numericUpDownWidth.TabIndex = 3;
			this.numericUpDownWidth.Value = new decimal(new int[] {
			1024,
			0,
			0,
			0});
			// 
			// numericUpDownHeight
			// 
			this.numericUpDownHeight.Location = new System.Drawing.Point(54, 57);
			this.numericUpDownHeight.Maximum = new decimal(new int[] {
			10000,
			0,
			0,
			0});
			this.numericUpDownHeight.Minimum = new decimal(new int[] {
			100,
			0,
			0,
			0});
			this.numericUpDownHeight.Name = "numericUpDownHeight";
			this.numericUpDownHeight.Size = new System.Drawing.Size(120, 20);
			this.numericUpDownHeight.TabIndex = 4;
			this.numericUpDownHeight.Value = new decimal(new int[] {
			768,
			0,
			0,
			0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Width";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(38, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Height";
			// 
			// textBoxPath
			// 
			this.textBoxPath.Location = new System.Drawing.Point(13, 113);
			this.textBoxPath.Name = "textBoxPath";
			this.textBoxPath.Size = new System.Drawing.Size(204, 20);
			this.textBoxPath.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 97);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(210, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Please enter a path for the exported image.";
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.Location = new System.Drawing.Point(226, 110);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowse.TabIndex = 9;
			this.buttonBrowse.Text = "Browse...";
			this.buttonBrowse.UseVisualStyleBackColor = true;
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			// 
			// formImageExport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(312, 202);
			this.Controls.Add(this.buttonBrowse);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxPath);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.numericUpDownHeight);
			this.Controls.Add(this.numericUpDownWidth);
			this.Controls.Add(this.labelInstructions);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonExport);
			this.Name = "FormImageExport";
			this.Text = "Export Image";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonExport;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label labelInstructions;
		private System.Windows.Forms.NumericUpDown numericUpDownWidth;
		private System.Windows.Forms.NumericUpDown numericUpDownHeight;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonBrowse;
	}
}
