namespace TrainEditor2.Views
{
	partial class FormExportTrain
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
			this.groupBoxNewFormat = new System.Windows.Forms.GroupBox();
			this.groupBoxTrainXml = new System.Windows.Forms.GroupBox();
			this.buttonTrainXmlFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxTrainXmlFileName = new System.Windows.Forms.TextBox();
			this.labelTrainXmlFileName = new System.Windows.Forms.Label();
			this.labelType = new System.Windows.Forms.Label();
			this.comboBoxType = new System.Windows.Forms.ComboBox();
			this.groupBoxOldFormat = new System.Windows.Forms.GroupBox();
			this.groupBoxExtensionsCfg = new System.Windows.Forms.GroupBox();
			this.buttonExtensionsCfgFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxExtensionsCfgFileName = new System.Windows.Forms.TextBox();
			this.labelExtensionsCfgFileName = new System.Windows.Forms.Label();
			this.groupBoxTrainDat = new System.Windows.Forms.GroupBox();
			this.buttonTrainDatFileNameOpen = new System.Windows.Forms.Button();
			this.textBoxTrainDatFileName = new System.Windows.Forms.TextBox();
			this.labelTrainDatFileName = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.groupBoxNewFormat.SuspendLayout();
			this.groupBoxTrainXml.SuspendLayout();
			this.groupBoxOldFormat.SuspendLayout();
			this.groupBoxExtensionsCfg.SuspendLayout();
			this.groupBoxTrainDat.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(176, 320);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 50;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// groupBoxNewFormat
			// 
			this.groupBoxNewFormat.Controls.Add(this.groupBoxTrainXml);
			this.groupBoxNewFormat.Location = new System.Drawing.Point(8, 216);
			this.groupBoxNewFormat.Name = "groupBoxNewFormat";
			this.groupBoxNewFormat.Size = new System.Drawing.Size(224, 96);
			this.groupBoxNewFormat.TabIndex = 49;
			this.groupBoxNewFormat.TabStop = false;
			this.groupBoxNewFormat.Text = "New format";
			// 
			// groupBoxTrainXml
			// 
			this.groupBoxTrainXml.AllowDrop = true;
			this.groupBoxTrainXml.Controls.Add(this.buttonTrainXmlFileNameOpen);
			this.groupBoxTrainXml.Controls.Add(this.textBoxTrainXmlFileName);
			this.groupBoxTrainXml.Controls.Add(this.labelTrainXmlFileName);
			this.groupBoxTrainXml.Location = new System.Drawing.Point(8, 16);
			this.groupBoxTrainXml.Name = "groupBoxTrainXml";
			this.groupBoxTrainXml.Size = new System.Drawing.Size(208, 72);
			this.groupBoxTrainXml.TabIndex = 42;
			this.groupBoxTrainXml.TabStop = false;
			this.groupBoxTrainXml.Text = "Train.xml";
			// 
			// buttonTrainXmlFileNameOpen
			// 
			this.buttonTrainXmlFileNameOpen.Location = new System.Drawing.Point(136, 40);
			this.buttonTrainXmlFileNameOpen.Name = "buttonTrainXmlFileNameOpen";
			this.buttonTrainXmlFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonTrainXmlFileNameOpen.TabIndex = 38;
			this.buttonTrainXmlFileNameOpen.Text = "Open...";
			this.buttonTrainXmlFileNameOpen.UseVisualStyleBackColor = true;
			// 
			// textBoxTrainXmlFileName
			// 
			this.textBoxTrainXmlFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxTrainXmlFileName.Name = "textBoxTrainXmlFileName";
			this.textBoxTrainXmlFileName.Size = new System.Drawing.Size(120, 19);
			this.textBoxTrainXmlFileName.TabIndex = 37;
			// 
			// labelTrainXmlFileName
			// 
			this.labelTrainXmlFileName.Location = new System.Drawing.Point(8, 16);
			this.labelTrainXmlFileName.Name = "labelTrainXmlFileName";
			this.labelTrainXmlFileName.Size = new System.Drawing.Size(56, 16);
			this.labelTrainXmlFileName.TabIndex = 39;
			this.labelTrainXmlFileName.Text = "Filename:";
			this.labelTrainXmlFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelType
			// 
			this.labelType.Location = new System.Drawing.Point(8, 8);
			this.labelType.Name = "labelType";
			this.labelType.Size = new System.Drawing.Size(72, 16);
			this.labelType.TabIndex = 46;
			this.labelType.Text = "Type:";
			this.labelType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxType
			// 
			this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxType.FormattingEnabled = true;
			this.comboBoxType.Items.AddRange(new object[] {
            "Old format",
            "New format"});
			this.comboBoxType.Location = new System.Drawing.Point(88, 8);
			this.comboBoxType.Name = "comboBoxType";
			this.comboBoxType.Size = new System.Drawing.Size(120, 20);
			this.comboBoxType.TabIndex = 47;
			// 
			// groupBoxOldFormat
			// 
			this.groupBoxOldFormat.Controls.Add(this.groupBoxExtensionsCfg);
			this.groupBoxOldFormat.Controls.Add(this.groupBoxTrainDat);
			this.groupBoxOldFormat.Location = new System.Drawing.Point(8, 32);
			this.groupBoxOldFormat.Name = "groupBoxOldFormat";
			this.groupBoxOldFormat.Size = new System.Drawing.Size(224, 176);
			this.groupBoxOldFormat.TabIndex = 48;
			this.groupBoxOldFormat.TabStop = false;
			this.groupBoxOldFormat.Text = "Old format";
			// 
			// groupBoxExtensionsCfg
			// 
			this.groupBoxExtensionsCfg.AllowDrop = true;
			this.groupBoxExtensionsCfg.Controls.Add(this.buttonExtensionsCfgFileNameOpen);
			this.groupBoxExtensionsCfg.Controls.Add(this.textBoxExtensionsCfgFileName);
			this.groupBoxExtensionsCfg.Controls.Add(this.labelExtensionsCfgFileName);
			this.groupBoxExtensionsCfg.Location = new System.Drawing.Point(8, 96);
			this.groupBoxExtensionsCfg.Name = "groupBoxExtensionsCfg";
			this.groupBoxExtensionsCfg.Size = new System.Drawing.Size(208, 72);
			this.groupBoxExtensionsCfg.TabIndex = 43;
			this.groupBoxExtensionsCfg.TabStop = false;
			this.groupBoxExtensionsCfg.Text = "Extensions.cfg";
			// 
			// buttonExtensionsCfgFileNameOpen
			// 
			this.buttonExtensionsCfgFileNameOpen.Location = new System.Drawing.Point(136, 40);
			this.buttonExtensionsCfgFileNameOpen.Name = "buttonExtensionsCfgFileNameOpen";
			this.buttonExtensionsCfgFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonExtensionsCfgFileNameOpen.TabIndex = 38;
			this.buttonExtensionsCfgFileNameOpen.Text = "Open...";
			this.buttonExtensionsCfgFileNameOpen.UseVisualStyleBackColor = true;
			// 
			// textBoxExtensionsCfgFileName
			// 
			this.textBoxExtensionsCfgFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxExtensionsCfgFileName.Name = "textBoxExtensionsCfgFileName";
			this.textBoxExtensionsCfgFileName.Size = new System.Drawing.Size(120, 19);
			this.textBoxExtensionsCfgFileName.TabIndex = 37;
			// 
			// labelExtensionsCfgFileName
			// 
			this.labelExtensionsCfgFileName.Location = new System.Drawing.Point(8, 16);
			this.labelExtensionsCfgFileName.Name = "labelExtensionsCfgFileName";
			this.labelExtensionsCfgFileName.Size = new System.Drawing.Size(56, 16);
			this.labelExtensionsCfgFileName.TabIndex = 39;
			this.labelExtensionsCfgFileName.Text = "Filename:";
			this.labelExtensionsCfgFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxTrainDat
			// 
			this.groupBoxTrainDat.AllowDrop = true;
			this.groupBoxTrainDat.Controls.Add(this.buttonTrainDatFileNameOpen);
			this.groupBoxTrainDat.Controls.Add(this.textBoxTrainDatFileName);
			this.groupBoxTrainDat.Controls.Add(this.labelTrainDatFileName);
			this.groupBoxTrainDat.Location = new System.Drawing.Point(8, 16);
			this.groupBoxTrainDat.Name = "groupBoxTrainDat";
			this.groupBoxTrainDat.Size = new System.Drawing.Size(208, 72);
			this.groupBoxTrainDat.TabIndex = 42;
			this.groupBoxTrainDat.TabStop = false;
			this.groupBoxTrainDat.Text = "Train.dat";
			// 
			// buttonTrainDatFileNameOpen
			// 
			this.buttonTrainDatFileNameOpen.Location = new System.Drawing.Point(136, 40);
			this.buttonTrainDatFileNameOpen.Name = "buttonTrainDatFileNameOpen";
			this.buttonTrainDatFileNameOpen.Size = new System.Drawing.Size(56, 24);
			this.buttonTrainDatFileNameOpen.TabIndex = 38;
			this.buttonTrainDatFileNameOpen.Text = "Open...";
			this.buttonTrainDatFileNameOpen.UseVisualStyleBackColor = true;
			// 
			// textBoxTrainDatFileName
			// 
			this.textBoxTrainDatFileName.Location = new System.Drawing.Point(72, 16);
			this.textBoxTrainDatFileName.Name = "textBoxTrainDatFileName";
			this.textBoxTrainDatFileName.Size = new System.Drawing.Size(120, 19);
			this.textBoxTrainDatFileName.TabIndex = 37;
			// 
			// labelTrainDatFileName
			// 
			this.labelTrainDatFileName.Location = new System.Drawing.Point(8, 16);
			this.labelTrainDatFileName.Name = "labelTrainDatFileName";
			this.labelTrainDatFileName.Size = new System.Drawing.Size(56, 16);
			this.labelTrainDatFileName.TabIndex = 39;
			this.labelTrainDatFileName.Text = "Filename:";
			this.labelTrainDatFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormExportTrain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(241, 353);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBoxNewFormat);
			this.Controls.Add(this.labelType);
			this.Controls.Add(this.comboBoxType);
			this.Controls.Add(this.groupBoxOldFormat);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "FormExportTrain";
			this.Text = "Export train file";
			this.Load += new System.EventHandler(this.FormExportTrain_Load);
			this.groupBoxNewFormat.ResumeLayout(false);
			this.groupBoxTrainXml.ResumeLayout(false);
			this.groupBoxTrainXml.PerformLayout();
			this.groupBoxOldFormat.ResumeLayout(false);
			this.groupBoxExtensionsCfg.ResumeLayout(false);
			this.groupBoxExtensionsCfg.PerformLayout();
			this.groupBoxTrainDat.ResumeLayout(false);
			this.groupBoxTrainDat.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.GroupBox groupBoxNewFormat;
		private System.Windows.Forms.GroupBox groupBoxTrainXml;
		private System.Windows.Forms.Button buttonTrainXmlFileNameOpen;
		private System.Windows.Forms.TextBox textBoxTrainXmlFileName;
		private System.Windows.Forms.Label labelTrainXmlFileName;
		private System.Windows.Forms.Label labelType;
		private System.Windows.Forms.ComboBox comboBoxType;
		private System.Windows.Forms.GroupBox groupBoxOldFormat;
		private System.Windows.Forms.GroupBox groupBoxExtensionsCfg;
		private System.Windows.Forms.Button buttonExtensionsCfgFileNameOpen;
		private System.Windows.Forms.TextBox textBoxExtensionsCfgFileName;
		private System.Windows.Forms.Label labelExtensionsCfgFileName;
		private System.Windows.Forms.GroupBox groupBoxTrainDat;
		private System.Windows.Forms.Button buttonTrainDatFileNameOpen;
		private System.Windows.Forms.TextBox textBoxTrainDatFileName;
		private System.Windows.Forms.Label labelTrainDatFileName;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}
