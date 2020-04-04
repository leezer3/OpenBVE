namespace TrainEditor2.Views
{
	partial class FormExportSound
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBoxSoundXml = new System.Windows.Forms.GroupBox();
			this.buttonSoundXmlFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxSoundXmlFileName = new System.Windows.Forms.TextBox();
			this.labelSoundXmlFileName = new System.Windows.Forms.Label();
			this.groupBoxSoundCfg = new System.Windows.Forms.GroupBox();
			this.buttonSoundCfgFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxSoundCfgFileName = new System.Windows.Forms.TextBox();
			this.labelSoundCfgFileName = new System.Windows.Forms.Label();
			this.labelType = new System.Windows.Forms.Label();
			this.comboBoxType = new System.Windows.Forms.ComboBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.groupBoxSoundXml.SuspendLayout();
			this.groupBoxSoundCfg.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(160, 192);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 55;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// groupBoxSoundXml
			// 
			this.groupBoxSoundXml.AllowDrop = true;
			this.groupBoxSoundXml.Controls.Add(this.buttonSoundXmlFileNameOpen);
			this.groupBoxSoundXml.Controls.Add(this.textBoxSoundXmlFileName);
			this.groupBoxSoundXml.Controls.Add(this.labelSoundXmlFileName);
			this.groupBoxSoundXml.Location = new System.Drawing.Point(8, 112);
			this.groupBoxSoundXml.Name = "groupBoxSoundXml";
			this.groupBoxSoundXml.Size = new System.Drawing.Size(208, 72);
			this.groupBoxSoundXml.TabIndex = 54;
			this.groupBoxSoundXml.TabStop = false;
			this.groupBoxSoundXml.Text = "Sound.xml";
			// 
			// buttonSoundXmlFileNameOpen
			// 
			this.buttonSoundXmlFileNameOpen.Location = new System.Drawing.Point(136, 40);
			this.buttonSoundXmlFileNameOpen.Name = "buttonSoundXmlFileNameOpen";
			this.buttonSoundXmlFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundXmlFileNameOpen.TabIndex = 38;
			this.buttonSoundXmlFileNameOpen.Text = "Open...";
			this.buttonSoundXmlFileNameOpen.UseVisualStyleBackColor = true;
			// 
			// textBoxSoundXmlFileName
			// 
			this.textBoxSoundXmlFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxSoundXmlFileName.Name = "textBoxSoundXmlFileName";
			this.textBoxSoundXmlFileName.Size = new System.Drawing.Size(120, 19);
			this.textBoxSoundXmlFileName.TabIndex = 37;
			// 
			// labelSoundXmlFileName
			// 
			this.labelSoundXmlFileName.Location = new System.Drawing.Point(8, 16);
			this.labelSoundXmlFileName.Name = "labelSoundXmlFileName";
			this.labelSoundXmlFileName.Size = new System.Drawing.Size(56, 16);
			this.labelSoundXmlFileName.TabIndex = 39;
			this.labelSoundXmlFileName.Text = "Filename:";
			this.labelSoundXmlFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxSoundCfg
			// 
			this.groupBoxSoundCfg.AllowDrop = true;
			this.groupBoxSoundCfg.Controls.Add(this.buttonSoundCfgFileNameOpen);
			this.groupBoxSoundCfg.Controls.Add(this.textBoxSoundCfgFileName);
			this.groupBoxSoundCfg.Controls.Add(this.labelSoundCfgFileName);
			this.groupBoxSoundCfg.Location = new System.Drawing.Point(8, 32);
			this.groupBoxSoundCfg.Name = "groupBoxSoundCfg";
			this.groupBoxSoundCfg.Size = new System.Drawing.Size(208, 72);
			this.groupBoxSoundCfg.TabIndex = 53;
			this.groupBoxSoundCfg.TabStop = false;
			this.groupBoxSoundCfg.Text = "Sound.cfg";
			// 
			// buttonSoundCfgFileNameOpen
			// 
			this.buttonSoundCfgFileNameOpen.Location = new System.Drawing.Point(136, 40);
			this.buttonSoundCfgFileNameOpen.Name = "buttonSoundCfgFileNameOpen";
			this.buttonSoundCfgFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonSoundCfgFileNameOpen.TabIndex = 38;
			this.buttonSoundCfgFileNameOpen.Text = "Open...";
			this.buttonSoundCfgFileNameOpen.UseVisualStyleBackColor = true;
			// 
			// textBoxSoundCfgFileName
			// 
			this.textBoxSoundCfgFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxSoundCfgFileName.Name = "textBoxSoundCfgFileName";
			this.textBoxSoundCfgFileName.Size = new System.Drawing.Size(120, 19);
			this.textBoxSoundCfgFileName.TabIndex = 37;
			// 
			// labelSoundCfgFileName
			// 
			this.labelSoundCfgFileName.Location = new System.Drawing.Point(8, 16);
			this.labelSoundCfgFileName.Name = "labelSoundCfgFileName";
			this.labelSoundCfgFileName.Size = new System.Drawing.Size(56, 16);
			this.labelSoundCfgFileName.TabIndex = 39;
			this.labelSoundCfgFileName.Text = "Filename:";
			this.labelSoundCfgFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelType
			// 
			this.labelType.Location = new System.Drawing.Point(8, 8);
			this.labelType.Name = "labelType";
			this.labelType.Size = new System.Drawing.Size(64, 16);
			this.labelType.TabIndex = 51;
			this.labelType.Text = "Type:";
			this.labelType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxType
			// 
			this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxType.FormattingEnabled = true;
			this.comboBoxType.Items.AddRange(new object[] {
            "No setting file",
            "Sound.cfg",
            "Sound.xml"});
			this.comboBoxType.Location = new System.Drawing.Point(80, 8);
			this.comboBoxType.Name = "comboBoxType";
			this.comboBoxType.Size = new System.Drawing.Size(120, 20);
			this.comboBoxType.TabIndex = 52;
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormExportSound
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(225, 225);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBoxSoundXml);
			this.Controls.Add(this.groupBoxSoundCfg);
			this.Controls.Add(this.labelType);
			this.Controls.Add(this.comboBoxType);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "FormExportSound";
			this.Text = "FormExportSound";
			this.Load += new System.EventHandler(this.FormExportSound_Load);
			this.groupBoxSoundXml.ResumeLayout(false);
			this.groupBoxSoundXml.PerformLayout();
			this.groupBoxSoundCfg.ResumeLayout(false);
			this.groupBoxSoundCfg.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.GroupBox groupBoxSoundXml;
		private System.Windows.Forms.Button buttonSoundXmlFileNameOpen;
		private System.Windows.Forms.TextBox textBoxSoundXmlFileName;
		private System.Windows.Forms.Label labelSoundXmlFileName;
		private System.Windows.Forms.GroupBox groupBoxSoundCfg;
		private System.Windows.Forms.Button buttonSoundCfgFileNameOpen;
		private System.Windows.Forms.TextBox textBoxSoundCfgFileName;
		private System.Windows.Forms.Label labelSoundCfgFileName;
		private System.Windows.Forms.Label labelType;
		private System.Windows.Forms.ComboBox comboBoxType;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}
