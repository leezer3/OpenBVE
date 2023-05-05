namespace TrainEditor2.Views
{
	partial class FormJerk
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
			this.textBoxDown = new System.Windows.Forms.TextBox();
			this.textBoxUp = new System.Windows.Forms.TextBox();
			this.labelUp = new System.Windows.Forms.Label();
			this.labelDown = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.comboBoxDownUnit = new System.Windows.Forms.ComboBox();
			this.comboBoxUpUnit = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxDown
			// 
			this.textBoxDown.Location = new System.Drawing.Point(56, 32);
			this.textBoxDown.Name = "textBoxDown";
			this.textBoxDown.Size = new System.Drawing.Size(48, 19);
			this.textBoxDown.TabIndex = 28;
			// 
			// textBoxUp
			// 
			this.textBoxUp.Location = new System.Drawing.Point(56, 8);
			this.textBoxUp.Name = "textBoxUp";
			this.textBoxUp.Size = new System.Drawing.Size(48, 19);
			this.textBoxUp.TabIndex = 26;
			// 
			// labelUp
			// 
			this.labelUp.Location = new System.Drawing.Point(8, 8);
			this.labelUp.Name = "labelUp";
			this.labelUp.Size = new System.Drawing.Size(40, 16);
			this.labelUp.TabIndex = 25;
			this.labelUp.Text = "Up:";
			this.labelUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDown
			// 
			this.labelDown.Location = new System.Drawing.Point(8, 32);
			this.labelDown.Name = "labelDown";
			this.labelDown.Size = new System.Drawing.Size(40, 16);
			this.labelDown.TabIndex = 24;
			this.labelDown.Text = "Down:";
			this.labelDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(104, 56);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 56;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// comboBoxDownUnit
			// 
			this.comboBoxDownUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDownUnit.FormattingEnabled = true;
			this.comboBoxDownUnit.Location = new System.Drawing.Point(112, 32);
			this.comboBoxDownUnit.Name = "comboBoxDownUnit";
			this.comboBoxDownUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxDownUnit.TabIndex = 60;
			// 
			// comboBoxUpUnit
			// 
			this.comboBoxUpUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxUpUnit.FormattingEnabled = true;
			this.comboBoxUpUnit.Location = new System.Drawing.Point(112, 8);
			this.comboBoxUpUnit.Name = "comboBoxUpUnit";
			this.comboBoxUpUnit.Size = new System.Drawing.Size(48, 20);
			this.comboBoxUpUnit.TabIndex = 59;
			// 
			// FormJerk
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(169, 89);
			this.Controls.Add(this.comboBoxDownUnit);
			this.Controls.Add(this.comboBoxUpUnit);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxDown);
			this.Controls.Add(this.textBoxUp);
			this.Controls.Add(this.labelUp);
			this.Controls.Add(this.labelDown);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormJerk";
			this.Text = "Jerk";
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox textBoxDown;
		private System.Windows.Forms.TextBox textBoxUp;
		private System.Windows.Forms.Label labelUp;
		private System.Windows.Forms.Label labelDown;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.ComboBox comboBoxDownUnit;
		private System.Windows.Forms.ComboBox comboBoxUpUnit;
	}
}