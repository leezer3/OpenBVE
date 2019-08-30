namespace TrainEditor2.Views
{
	partial class FormBogie
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
			this.checkBoxReversed = new System.Windows.Forms.CheckBox();
			this.checkBoxDefinedAxles = new System.Windows.Forms.CheckBox();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.textBoxObject = new System.Windows.Forms.TextBox();
			this.labelObject = new System.Windows.Forms.Label();
			this.labelReversed = new System.Windows.Forms.Label();
			this.groupBoxAxles = new System.Windows.Forms.GroupBox();
			this.labelRearAxleUnit = new System.Windows.Forms.Label();
			this.labelFrontAxleUnit = new System.Windows.Forms.Label();
			this.textBoxRearAxle = new System.Windows.Forms.TextBox();
			this.textBoxFrontAxle = new System.Windows.Forms.TextBox();
			this.labelRearAxle = new System.Windows.Forms.Label();
			this.labelFrontAxle = new System.Windows.Forms.Label();
			this.labelDefinedAxles = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.groupBoxAxles.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// checkBoxReversed
			// 
			this.checkBoxReversed.Location = new System.Drawing.Point(96, 112);
			this.checkBoxReversed.Name = "checkBoxReversed";
			this.checkBoxReversed.Size = new System.Drawing.Size(48, 16);
			this.checkBoxReversed.TabIndex = 38;
			this.checkBoxReversed.UseVisualStyleBackColor = true;
			// 
			// checkBoxDefinedAxles
			// 
			this.checkBoxDefinedAxles.Location = new System.Drawing.Point(96, 8);
			this.checkBoxDefinedAxles.Name = "checkBoxDefinedAxles";
			this.checkBoxDefinedAxles.Size = new System.Drawing.Size(48, 16);
			this.checkBoxDefinedAxles.TabIndex = 37;
			this.checkBoxDefinedAxles.UseVisualStyleBackColor = true;
			// 
			// buttonOpen
			// 
			this.buttonOpen.Location = new System.Drawing.Point(144, 160);
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonOpen.TabIndex = 36;
			this.buttonOpen.Text = "Open...";
			this.buttonOpen.UseVisualStyleBackColor = true;
			this.buttonOpen.Click += new System.EventHandler(this.ButtonOpen_Click);
			// 
			// textBoxObject
			// 
			this.textBoxObject.Location = new System.Drawing.Point(96, 136);
			this.textBoxObject.Name = "textBoxObject";
			this.textBoxObject.Size = new System.Drawing.Size(104, 19);
			this.textBoxObject.TabIndex = 35;
			// 
			// labelObject
			// 
			this.labelObject.Location = new System.Drawing.Point(8, 136);
			this.labelObject.Name = "labelObject";
			this.labelObject.Size = new System.Drawing.Size(80, 16);
			this.labelObject.TabIndex = 34;
			this.labelObject.Text = "Object:";
			this.labelObject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelReversed
			// 
			this.labelReversed.Location = new System.Drawing.Point(8, 112);
			this.labelReversed.Name = "labelReversed";
			this.labelReversed.Size = new System.Drawing.Size(80, 16);
			this.labelReversed.TabIndex = 33;
			this.labelReversed.Text = "Reversed:";
			this.labelReversed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxAxles
			// 
			this.groupBoxAxles.Controls.Add(this.labelRearAxleUnit);
			this.groupBoxAxles.Controls.Add(this.labelFrontAxleUnit);
			this.groupBoxAxles.Controls.Add(this.textBoxRearAxle);
			this.groupBoxAxles.Controls.Add(this.textBoxFrontAxle);
			this.groupBoxAxles.Controls.Add(this.labelRearAxle);
			this.groupBoxAxles.Controls.Add(this.labelFrontAxle);
			this.groupBoxAxles.Location = new System.Drawing.Point(8, 32);
			this.groupBoxAxles.Name = "groupBoxAxles";
			this.groupBoxAxles.Size = new System.Drawing.Size(192, 72);
			this.groupBoxAxles.TabIndex = 32;
			this.groupBoxAxles.TabStop = false;
			this.groupBoxAxles.Text = "Axles";
			// 
			// labelRearAxleUnit
			// 
			this.labelRearAxleUnit.Location = new System.Drawing.Point(144, 40);
			this.labelRearAxleUnit.Name = "labelRearAxleUnit";
			this.labelRearAxleUnit.Size = new System.Drawing.Size(40, 16);
			this.labelRearAxleUnit.TabIndex = 27;
			this.labelRearAxleUnit.Text = "m";
			this.labelRearAxleUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelFrontAxleUnit
			// 
			this.labelFrontAxleUnit.Location = new System.Drawing.Point(144, 16);
			this.labelFrontAxleUnit.Name = "labelFrontAxleUnit";
			this.labelFrontAxleUnit.Size = new System.Drawing.Size(40, 16);
			this.labelFrontAxleUnit.TabIndex = 26;
			this.labelFrontAxleUnit.Text = "m";
			this.labelFrontAxleUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxRearAxle
			// 
			this.textBoxRearAxle.Location = new System.Drawing.Point(88, 40);
			this.textBoxRearAxle.Name = "textBoxRearAxle";
			this.textBoxRearAxle.Size = new System.Drawing.Size(48, 19);
			this.textBoxRearAxle.TabIndex = 17;
			// 
			// textBoxFrontAxle
			// 
			this.textBoxFrontAxle.Location = new System.Drawing.Point(88, 16);
			this.textBoxFrontAxle.Name = "textBoxFrontAxle";
			this.textBoxFrontAxle.Size = new System.Drawing.Size(48, 19);
			this.textBoxFrontAxle.TabIndex = 16;
			// 
			// labelRearAxle
			// 
			this.labelRearAxle.Location = new System.Drawing.Point(8, 40);
			this.labelRearAxle.Name = "labelRearAxle";
			this.labelRearAxle.Size = new System.Drawing.Size(72, 16);
			this.labelRearAxle.TabIndex = 10;
			this.labelRearAxle.Text = "RearAxle:";
			this.labelRearAxle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelFrontAxle
			// 
			this.labelFrontAxle.Location = new System.Drawing.Point(8, 16);
			this.labelFrontAxle.Name = "labelFrontAxle";
			this.labelFrontAxle.Size = new System.Drawing.Size(72, 16);
			this.labelFrontAxle.TabIndex = 9;
			this.labelFrontAxle.Text = "FrontAxle:";
			this.labelFrontAxle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDefinedAxles
			// 
			this.labelDefinedAxles.Location = new System.Drawing.Point(8, 8);
			this.labelDefinedAxles.Name = "labelDefinedAxles";
			this.labelDefinedAxles.Size = new System.Drawing.Size(80, 16);
			this.labelDefinedAxles.TabIndex = 31;
			this.labelDefinedAxles.Text = "DefinedAxles:";
			this.labelDefinedAxles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(144, 192);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 39;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormBogie
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(208, 221);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.checkBoxReversed);
			this.Controls.Add(this.checkBoxDefinedAxles);
			this.Controls.Add(this.buttonOpen);
			this.Controls.Add(this.textBoxObject);
			this.Controls.Add(this.labelObject);
			this.Controls.Add(this.labelReversed);
			this.Controls.Add(this.groupBoxAxles);
			this.Controls.Add(this.labelDefinedAxles);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormBogie";
			this.Text = "Bogie";
			this.Load += new System.EventHandler(this.FormBogie_Load);
			this.groupBoxAxles.ResumeLayout(false);
			this.groupBoxAxles.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBoxReversed;
		private System.Windows.Forms.CheckBox checkBoxDefinedAxles;
		private System.Windows.Forms.Button buttonOpen;
		private System.Windows.Forms.TextBox textBoxObject;
		private System.Windows.Forms.Label labelObject;
		private System.Windows.Forms.Label labelReversed;
		private System.Windows.Forms.GroupBox groupBoxAxles;
		private System.Windows.Forms.Label labelRearAxleUnit;
		private System.Windows.Forms.Label labelFrontAxleUnit;
		private System.Windows.Forms.TextBox textBoxRearAxle;
		private System.Windows.Forms.TextBox textBoxFrontAxle;
		private System.Windows.Forms.Label labelRearAxle;
		private System.Windows.Forms.Label labelFrontAxle;
		private System.Windows.Forms.Label labelDefinedAxles;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}
