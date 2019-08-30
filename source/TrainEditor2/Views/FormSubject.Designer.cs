namespace TrainEditor2.Views
{
	partial class FormSubject
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
			this.labelBase = new System.Windows.Forms.Label();
			this.comboBoxBase = new System.Windows.Forms.ComboBox();
			this.labelBaseOption = new System.Windows.Forms.Label();
			this.numericUpDownBaseOption = new System.Windows.Forms.NumericUpDown();
			this.labelSuffix = new System.Windows.Forms.Label();
			this.comboBoxSuffix = new System.Windows.Forms.ComboBox();
			this.numericUpDownSuffixOption = new System.Windows.Forms.NumericUpDown();
			this.labelSuffixOption = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBaseOption)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSuffixOption)).BeginInit();
			this.SuspendLayout();
			// 
			// labelBase
			// 
			this.labelBase.Location = new System.Drawing.Point(8, 8);
			this.labelBase.Name = "labelBase";
			this.labelBase.Size = new System.Drawing.Size(72, 16);
			this.labelBase.TabIndex = 32;
			this.labelBase.Text = "Base:";
			this.labelBase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxBase
			// 
			this.comboBoxBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBase.FormattingEnabled = true;
			this.comboBoxBase.Location = new System.Drawing.Point(88, 8);
			this.comboBoxBase.Name = "comboBoxBase";
			this.comboBoxBase.Size = new System.Drawing.Size(113, 20);
			this.comboBoxBase.TabIndex = 33;
			// 
			// labelBaseOption
			// 
			this.labelBaseOption.Location = new System.Drawing.Point(8, 32);
			this.labelBaseOption.Name = "labelBaseOption";
			this.labelBaseOption.Size = new System.Drawing.Size(72, 16);
			this.labelBaseOption.TabIndex = 34;
			this.labelBaseOption.Text = "BaseOption:";
			this.labelBaseOption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericUpDownBaseOption
			// 
			this.numericUpDownBaseOption.Location = new System.Drawing.Point(88, 32);
			this.numericUpDownBaseOption.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericUpDownBaseOption.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownBaseOption.Name = "numericUpDownBaseOption";
			this.numericUpDownBaseOption.Size = new System.Drawing.Size(112, 19);
			this.numericUpDownBaseOption.TabIndex = 35;
			// 
			// labelSuffix
			// 
			this.labelSuffix.Location = new System.Drawing.Point(8, 56);
			this.labelSuffix.Name = "labelSuffix";
			this.labelSuffix.Size = new System.Drawing.Size(72, 16);
			this.labelSuffix.TabIndex = 36;
			this.labelSuffix.Text = "Suffix:";
			this.labelSuffix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxSuffix
			// 
			this.comboBoxSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSuffix.FormattingEnabled = true;
			this.comboBoxSuffix.Location = new System.Drawing.Point(88, 56);
			this.comboBoxSuffix.Name = "comboBoxSuffix";
			this.comboBoxSuffix.Size = new System.Drawing.Size(113, 20);
			this.comboBoxSuffix.TabIndex = 37;
			// 
			// numericUpDownSuffixOption
			// 
			this.numericUpDownSuffixOption.Location = new System.Drawing.Point(88, 80);
			this.numericUpDownSuffixOption.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownSuffixOption.Name = "numericUpDownSuffixOption";
			this.numericUpDownSuffixOption.Size = new System.Drawing.Size(112, 19);
			this.numericUpDownSuffixOption.TabIndex = 39;
			// 
			// labelSuffixOption
			// 
			this.labelSuffixOption.Location = new System.Drawing.Point(8, 80);
			this.labelSuffixOption.Name = "labelSuffixOption";
			this.labelSuffixOption.Size = new System.Drawing.Size(72, 16);
			this.labelSuffixOption.TabIndex = 38;
			this.labelSuffixOption.Text = "SuffixOption:";
			this.labelSuffixOption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(144, 104);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 40;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// FormSubject
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(208, 133);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.numericUpDownSuffixOption);
			this.Controls.Add(this.labelSuffixOption);
			this.Controls.Add(this.comboBoxSuffix);
			this.Controls.Add(this.labelSuffix);
			this.Controls.Add(this.numericUpDownBaseOption);
			this.Controls.Add(this.labelBaseOption);
			this.Controls.Add(this.comboBoxBase);
			this.Controls.Add(this.labelBase);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormSubject";
			this.Text = "Subject";
			this.Load += new System.EventHandler(this.FormSubject_Load);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBaseOption)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSuffixOption)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelBase;
		private System.Windows.Forms.ComboBox comboBoxBase;
		private System.Windows.Forms.Label labelBaseOption;
		private System.Windows.Forms.NumericUpDown numericUpDownBaseOption;
		private System.Windows.Forms.Label labelSuffix;
		private System.Windows.Forms.ComboBox comboBoxSuffix;
		private System.Windows.Forms.NumericUpDown numericUpDownSuffixOption;
		private System.Windows.Forms.Label labelSuffixOption;
		private System.Windows.Forms.Button buttonOK;
	}
}
