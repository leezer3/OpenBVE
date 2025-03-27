﻿namespace TrainEditor2.Views
{
	partial class FormExport
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
            this.groupBoxPanel = new System.Windows.Forms.GroupBox();
            this.groupBoxPanelXml = new System.Windows.Forms.GroupBox();
            this.buttonPanelXmlFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxPanelXmlFileName = new System.Windows.Forms.TextBox();
            this.labelPanelXmlFileName = new System.Windows.Forms.Label();
            this.groupBoxPanel2Cfg = new System.Windows.Forms.GroupBox();
            this.buttonPanel2CfgFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxPanel2CfgFileName = new System.Windows.Forms.TextBox();
            this.labelPanel2CfgFileName = new System.Windows.Forms.Label();
            this.comboBoxPanelType = new System.Windows.Forms.ComboBox();
            this.labelPanelType = new System.Windows.Forms.Label();
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
            this.groupBoxSoundXml = new System.Windows.Forms.GroupBox();
            this.buttonSoundXmlFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxSoundXmlFileName = new System.Windows.Forms.TextBox();
            this.labelSoundXmlFileName = new System.Windows.Forms.Label();
            this.groupBoxSoundCfg = new System.Windows.Forms.GroupBox();
            this.buttonSoundCfgFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxSoundCfgFileName = new System.Windows.Forms.TextBox();
            this.labelSoundCfgFileName = new System.Windows.Forms.Label();
            this.comboBoxSoundType = new System.Windows.Forms.ComboBox();
            this.labelSoundType = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxTrain = new System.Windows.Forms.GroupBox();
            this.groupBoxOldFormat = new System.Windows.Forms.GroupBox();
            this.groupBoxTrainXML = new System.Windows.Forms.GroupBox();
            this.buttonTrainXMLFilenameOpen = new System.Windows.Forms.Button();
            this.textBoxTrainXmlFilename = new System.Windows.Forms.TextBox();
            this.labelTrainXMLFilename = new System.Windows.Forms.Label();
            this.groupBoxExtensionsCfg = new System.Windows.Forms.GroupBox();
            this.buttonExtensionsCfgFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxExtensionsCfgFileName = new System.Windows.Forms.TextBox();
            this.labelExtensionsCfgFileName = new System.Windows.Forms.Label();
            this.groupBoxTrainDat = new System.Windows.Forms.GroupBox();
            this.buttonTrainDatFileNameOpen = new System.Windows.Forms.Button();
            this.textBoxTrainDatFileName = new System.Windows.Forms.TextBox();
            this.labelTrainDatFileName = new System.Windows.Forms.Label();
            this.comboBoxTrainType = new System.Windows.Forms.ComboBox();
            this.labelTrainType = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxPanel.SuspendLayout();
            this.groupBoxPanelXml.SuspendLayout();
            this.groupBoxPanel2Cfg.SuspendLayout();
            this.groupBoxSound.SuspendLayout();
            this.groupBoxSoundXml.SuspendLayout();
            this.groupBoxSoundCfg.SuspendLayout();
            this.groupBoxTrain.SuspendLayout();
            this.groupBoxOldFormat.SuspendLayout();
            this.groupBoxTrainXML.SuspendLayout();
            this.groupBoxExtensionsCfg.SuspendLayout();
            this.groupBoxTrainDat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxPanel
            // 
            this.groupBoxPanel.Controls.Add(this.groupBoxPanelXml);
            this.groupBoxPanel.Controls.Add(this.groupBoxPanel2Cfg);
            this.groupBoxPanel.Controls.Add(this.comboBoxPanelType);
            this.groupBoxPanel.Controls.Add(this.labelPanelType);
            this.groupBoxPanel.Location = new System.Drawing.Point(248, 9);
            this.groupBoxPanel.Name = "groupBoxPanel";
            this.groupBoxPanel.Size = new System.Drawing.Size(216, 217);
            this.groupBoxPanel.TabIndex = 49;
            this.groupBoxPanel.TabStop = false;
            this.groupBoxPanel.Text = "Panel setting file";
            // 
            // groupBoxPanelXml
            // 
            this.groupBoxPanelXml.Controls.Add(this.buttonPanelXmlFileNameOpen);
            this.groupBoxPanelXml.Controls.Add(this.textBoxPanelXmlFileName);
            this.groupBoxPanelXml.Controls.Add(this.labelPanelXmlFileName);
            this.groupBoxPanelXml.Location = new System.Drawing.Point(8, 130);
            this.groupBoxPanelXml.Name = "groupBoxPanelXml";
            this.groupBoxPanelXml.Size = new System.Drawing.Size(200, 78);
            this.groupBoxPanelXml.TabIndex = 46;
            this.groupBoxPanelXml.TabStop = false;
            this.groupBoxPanelXml.Text = "Panel.xml";
            // 
            // buttonPanelXmlFileNameOpen
            // 
            this.buttonPanelXmlFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonPanelXmlFileNameOpen.Name = "buttonPanelXmlFileNameOpen";
            this.buttonPanelXmlFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonPanelXmlFileNameOpen.TabIndex = 38;
            this.buttonPanelXmlFileNameOpen.Text = "Open...";
            this.buttonPanelXmlFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonPanelXmlFileNameOpen.Click += new System.EventHandler(this.ButtonPanelXmlFileNameOpen_Click);
            // 
            // textBoxPanelXmlFileName
            // 
            this.textBoxPanelXmlFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxPanelXmlFileName.Name = "textBoxPanelXmlFileName";
            this.textBoxPanelXmlFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxPanelXmlFileName.TabIndex = 37;
            // 
            // labelPanelXmlFileName
            // 
            this.labelPanelXmlFileName.Location = new System.Drawing.Point(8, 17);
            this.labelPanelXmlFileName.Name = "labelPanelXmlFileName";
            this.labelPanelXmlFileName.Size = new System.Drawing.Size(56, 17);
            this.labelPanelXmlFileName.TabIndex = 39;
            this.labelPanelXmlFileName.Text = "Filename:";
            this.labelPanelXmlFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxPanel2Cfg
            // 
            this.groupBoxPanel2Cfg.Controls.Add(this.buttonPanel2CfgFileNameOpen);
            this.groupBoxPanel2Cfg.Controls.Add(this.textBoxPanel2CfgFileName);
            this.groupBoxPanel2Cfg.Controls.Add(this.labelPanel2CfgFileName);
            this.groupBoxPanel2Cfg.Location = new System.Drawing.Point(8, 43);
            this.groupBoxPanel2Cfg.Name = "groupBoxPanel2Cfg";
            this.groupBoxPanel2Cfg.Size = new System.Drawing.Size(200, 78);
            this.groupBoxPanel2Cfg.TabIndex = 45;
            this.groupBoxPanel2Cfg.TabStop = false;
            this.groupBoxPanel2Cfg.Text = "Panel2.cfg";
            // 
            // buttonPanel2CfgFileNameOpen
            // 
            this.buttonPanel2CfgFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonPanel2CfgFileNameOpen.Name = "buttonPanel2CfgFileNameOpen";
            this.buttonPanel2CfgFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonPanel2CfgFileNameOpen.TabIndex = 38;
            this.buttonPanel2CfgFileNameOpen.Text = "Open...";
            this.buttonPanel2CfgFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonPanel2CfgFileNameOpen.Click += new System.EventHandler(this.ButtonPanel2CfgFileNameOpen_Click);
            // 
            // textBoxPanel2CfgFileName
            // 
            this.textBoxPanel2CfgFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxPanel2CfgFileName.Name = "textBoxPanel2CfgFileName";
            this.textBoxPanel2CfgFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxPanel2CfgFileName.TabIndex = 37;
            // 
            // labelPanel2CfgFileName
            // 
            this.labelPanel2CfgFileName.Location = new System.Drawing.Point(8, 17);
            this.labelPanel2CfgFileName.Name = "labelPanel2CfgFileName";
            this.labelPanel2CfgFileName.Size = new System.Drawing.Size(56, 17);
            this.labelPanel2CfgFileName.TabIndex = 39;
            this.labelPanel2CfgFileName.Text = "Filename:";
            this.labelPanel2CfgFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxPanelType
            // 
            this.comboBoxPanelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPanelType.FormattingEnabled = true;
            this.comboBoxPanelType.Items.AddRange(new object[] {
            "Panel2.cfg",
            "Panel.xml"});
            this.comboBoxPanelType.Location = new System.Drawing.Point(80, 17);
            this.comboBoxPanelType.Name = "comboBoxPanelType";
            this.comboBoxPanelType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPanelType.TabIndex = 41;
            // 
            // labelPanelType
            // 
            this.labelPanelType.Location = new System.Drawing.Point(8, 17);
            this.labelPanelType.Name = "labelPanelType";
            this.labelPanelType.Size = new System.Drawing.Size(64, 17);
            this.labelPanelType.TabIndex = 40;
            this.labelPanelType.Text = "Type:";
            this.labelPanelType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxSound
            // 
            this.groupBoxSound.Controls.Add(this.groupBoxSoundXml);
            this.groupBoxSound.Controls.Add(this.groupBoxSoundCfg);
            this.groupBoxSound.Controls.Add(this.comboBoxSoundType);
            this.groupBoxSound.Controls.Add(this.labelSoundType);
            this.groupBoxSound.Location = new System.Drawing.Point(472, 9);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Size = new System.Drawing.Size(216, 217);
            this.groupBoxSound.TabIndex = 48;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "Sound setting file";
            // 
            // groupBoxSoundXml
            // 
            this.groupBoxSoundXml.Controls.Add(this.buttonSoundXmlFileNameOpen);
            this.groupBoxSoundXml.Controls.Add(this.textBoxSoundXmlFileName);
            this.groupBoxSoundXml.Controls.Add(this.labelSoundXmlFileName);
            this.groupBoxSoundXml.Location = new System.Drawing.Point(8, 130);
            this.groupBoxSoundXml.Name = "groupBoxSoundXml";
            this.groupBoxSoundXml.Size = new System.Drawing.Size(200, 78);
            this.groupBoxSoundXml.TabIndex = 44;
            this.groupBoxSoundXml.TabStop = false;
            this.groupBoxSoundXml.Text = "Sound.xml";
            // 
            // buttonSoundXmlFileNameOpen
            // 
            this.buttonSoundXmlFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonSoundXmlFileNameOpen.Name = "buttonSoundXmlFileNameOpen";
            this.buttonSoundXmlFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonSoundXmlFileNameOpen.TabIndex = 38;
            this.buttonSoundXmlFileNameOpen.Text = "Open...";
            this.buttonSoundXmlFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonSoundXmlFileNameOpen.Click += new System.EventHandler(this.ButtonSoundXmlFileNameOpen_Click);
            // 
            // textBoxSoundXmlFileName
            // 
            this.textBoxSoundXmlFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxSoundXmlFileName.Name = "textBoxSoundXmlFileName";
            this.textBoxSoundXmlFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxSoundXmlFileName.TabIndex = 37;
            // 
            // labelSoundXmlFileName
            // 
            this.labelSoundXmlFileName.Location = new System.Drawing.Point(8, 17);
            this.labelSoundXmlFileName.Name = "labelSoundXmlFileName";
            this.labelSoundXmlFileName.Size = new System.Drawing.Size(56, 17);
            this.labelSoundXmlFileName.TabIndex = 39;
            this.labelSoundXmlFileName.Text = "Filename:";
            this.labelSoundXmlFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxSoundCfg
            // 
            this.groupBoxSoundCfg.Controls.Add(this.buttonSoundCfgFileNameOpen);
            this.groupBoxSoundCfg.Controls.Add(this.textBoxSoundCfgFileName);
            this.groupBoxSoundCfg.Controls.Add(this.labelSoundCfgFileName);
            this.groupBoxSoundCfg.Location = new System.Drawing.Point(8, 43);
            this.groupBoxSoundCfg.Name = "groupBoxSoundCfg";
            this.groupBoxSoundCfg.Size = new System.Drawing.Size(200, 78);
            this.groupBoxSoundCfg.TabIndex = 43;
            this.groupBoxSoundCfg.TabStop = false;
            this.groupBoxSoundCfg.Text = "Sound.cfg";
            // 
            // buttonSoundCfgFileNameOpen
            // 
            this.buttonSoundCfgFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonSoundCfgFileNameOpen.Name = "buttonSoundCfgFileNameOpen";
            this.buttonSoundCfgFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonSoundCfgFileNameOpen.TabIndex = 38;
            this.buttonSoundCfgFileNameOpen.Text = "Open...";
            this.buttonSoundCfgFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonSoundCfgFileNameOpen.Click += new System.EventHandler(this.ButtonSoundCfgFileNameOpen_Click);
            // 
            // textBoxSoundCfgFileName
            // 
            this.textBoxSoundCfgFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxSoundCfgFileName.Name = "textBoxSoundCfgFileName";
            this.textBoxSoundCfgFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxSoundCfgFileName.TabIndex = 37;
            // 
            // labelSoundCfgFileName
            // 
            this.labelSoundCfgFileName.Location = new System.Drawing.Point(8, 17);
            this.labelSoundCfgFileName.Name = "labelSoundCfgFileName";
            this.labelSoundCfgFileName.Size = new System.Drawing.Size(56, 17);
            this.labelSoundCfgFileName.TabIndex = 39;
            this.labelSoundCfgFileName.Text = "Filename:";
            this.labelSoundCfgFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxSoundType
            // 
            this.comboBoxSoundType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSoundType.FormattingEnabled = true;
            this.comboBoxSoundType.Items.AddRange(new object[] {
            "Sound.cfg",
            "Sound.xml"});
            this.comboBoxSoundType.Location = new System.Drawing.Point(80, 17);
            this.comboBoxSoundType.Name = "comboBoxSoundType";
            this.comboBoxSoundType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSoundType.TabIndex = 41;
            // 
            // labelSoundType
            // 
            this.labelSoundType.Location = new System.Drawing.Point(8, 17);
            this.labelSoundType.Name = "labelSoundType";
            this.labelSoundType.Size = new System.Drawing.Size(64, 17);
            this.labelSoundType.TabIndex = 40;
            this.labelSoundType.Text = "Type:";
            this.labelSoundType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(632, 309);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(56, 26);
            this.buttonOK.TabIndex = 47;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // groupBoxTrain
            // 
            this.groupBoxTrain.Controls.Add(this.groupBoxTrainXML);
            this.groupBoxTrain.Controls.Add(this.groupBoxOldFormat);
            this.groupBoxTrain.Controls.Add(this.comboBoxTrainType);
            this.groupBoxTrain.Controls.Add(this.labelTrainType);
            this.groupBoxTrain.Location = new System.Drawing.Point(8, 9);
            this.groupBoxTrain.Name = "groupBoxTrain";
            this.groupBoxTrain.Size = new System.Drawing.Size(232, 326);
            this.groupBoxTrain.TabIndex = 46;
            this.groupBoxTrain.TabStop = false;
            this.groupBoxTrain.Text = "Train setting file";
            // 
            // groupBoxOldFormat
            // 
            this.groupBoxOldFormat.Controls.Add(this.groupBoxExtensionsCfg);
            this.groupBoxOldFormat.Controls.Add(this.groupBoxTrainDat);
            this.groupBoxOldFormat.Location = new System.Drawing.Point(8, 43);
            this.groupBoxOldFormat.Name = "groupBoxOldFormat";
            this.groupBoxOldFormat.Size = new System.Drawing.Size(216, 187);
            this.groupBoxOldFormat.TabIndex = 42;
            this.groupBoxOldFormat.TabStop = false;
            this.groupBoxOldFormat.Text = "Old format";
            // 
            // groupBoxTrainXML
            // 
            this.groupBoxTrainXML.Controls.Add(this.buttonTrainXMLFilenameOpen);
            this.groupBoxTrainXML.Controls.Add(this.textBoxTrainXmlFilename);
            this.groupBoxTrainXML.Controls.Add(this.labelTrainXMLFilename);
            this.groupBoxTrainXML.Location = new System.Drawing.Point(6, 236);
            this.groupBoxTrainXML.Name = "groupBoxTrainXML";
            this.groupBoxTrainXML.Size = new System.Drawing.Size(220, 80);
            this.groupBoxTrainXML.TabIndex = 45;
            this.groupBoxTrainXML.TabStop = false;
            this.groupBoxTrainXML.Text = "Train.xml";
            // 
            // buttonTrainXMLFilenameOpen
            // 
            this.buttonTrainXMLFilenameOpen.Location = new System.Drawing.Point(147, 43);
            this.buttonTrainXMLFilenameOpen.Name = "buttonTrainXMLFilenameOpen";
            this.buttonTrainXMLFilenameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonTrainXMLFilenameOpen.TabIndex = 38;
            this.buttonTrainXMLFilenameOpen.Text = "Open...";
            this.buttonTrainXMLFilenameOpen.UseVisualStyleBackColor = true;
            this.buttonTrainXMLFilenameOpen.Click += new System.EventHandler(this.buttonTrainXmlFilenameOpen_Click);
            // 
            // textBoxTrainXmlFilename
            // 
            this.textBoxTrainXmlFilename.Location = new System.Drawing.Point(83, 17);
            this.textBoxTrainXmlFilename.Name = "textBoxTrainXmlFilename";
            this.textBoxTrainXmlFilename.Size = new System.Drawing.Size(120, 20);
            this.textBoxTrainXmlFilename.TabIndex = 37;
            // 
            // labelTrainXMLFilename
            // 
            this.labelTrainXMLFilename.Location = new System.Drawing.Point(8, 17);
            this.labelTrainXMLFilename.Name = "labelTrainXMLFilename";
            this.labelTrainXMLFilename.Size = new System.Drawing.Size(56, 17);
            this.labelTrainXMLFilename.TabIndex = 39;
            this.labelTrainXMLFilename.Text = "Filename:";
            this.labelTrainXMLFilename.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxExtensionsCfg
            // 
            this.groupBoxExtensionsCfg.Controls.Add(this.buttonExtensionsCfgFileNameOpen);
            this.groupBoxExtensionsCfg.Controls.Add(this.textBoxExtensionsCfgFileName);
            this.groupBoxExtensionsCfg.Controls.Add(this.labelExtensionsCfgFileName);
            this.groupBoxExtensionsCfg.Location = new System.Drawing.Point(8, 104);
            this.groupBoxExtensionsCfg.Name = "groupBoxExtensionsCfg";
            this.groupBoxExtensionsCfg.Size = new System.Drawing.Size(200, 78);
            this.groupBoxExtensionsCfg.TabIndex = 43;
            this.groupBoxExtensionsCfg.TabStop = false;
            this.groupBoxExtensionsCfg.Text = "Extensions.cfg";
            // 
            // buttonExtensionsCfgFileNameOpen
            // 
            this.buttonExtensionsCfgFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonExtensionsCfgFileNameOpen.Name = "buttonExtensionsCfgFileNameOpen";
            this.buttonExtensionsCfgFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonExtensionsCfgFileNameOpen.TabIndex = 38;
            this.buttonExtensionsCfgFileNameOpen.Text = "Open...";
            this.buttonExtensionsCfgFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonExtensionsCfgFileNameOpen.Click += new System.EventHandler(this.ButtonExtensionsCfgFileNameOpen_Click);
            // 
            // textBoxExtensionsCfgFileName
            // 
            this.textBoxExtensionsCfgFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxExtensionsCfgFileName.Name = "textBoxExtensionsCfgFileName";
            this.textBoxExtensionsCfgFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxExtensionsCfgFileName.TabIndex = 37;
            // 
            // labelExtensionsCfgFileName
            // 
            this.labelExtensionsCfgFileName.Location = new System.Drawing.Point(8, 17);
            this.labelExtensionsCfgFileName.Name = "labelExtensionsCfgFileName";
            this.labelExtensionsCfgFileName.Size = new System.Drawing.Size(56, 17);
            this.labelExtensionsCfgFileName.TabIndex = 39;
            this.labelExtensionsCfgFileName.Text = "Filename:";
            this.labelExtensionsCfgFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxTrainDat
            // 
            this.groupBoxTrainDat.Controls.Add(this.buttonTrainDatFileNameOpen);
            this.groupBoxTrainDat.Controls.Add(this.textBoxTrainDatFileName);
            this.groupBoxTrainDat.Controls.Add(this.labelTrainDatFileName);
            this.groupBoxTrainDat.Location = new System.Drawing.Point(8, 17);
            this.groupBoxTrainDat.Name = "groupBoxTrainDat";
            this.groupBoxTrainDat.Size = new System.Drawing.Size(200, 78);
            this.groupBoxTrainDat.TabIndex = 42;
            this.groupBoxTrainDat.TabStop = false;
            this.groupBoxTrainDat.Text = "Train.dat";
            // 
            // buttonTrainDatFileNameOpen
            // 
            this.buttonTrainDatFileNameOpen.Location = new System.Drawing.Point(136, 43);
            this.buttonTrainDatFileNameOpen.Name = "buttonTrainDatFileNameOpen";
            this.buttonTrainDatFileNameOpen.Size = new System.Drawing.Size(56, 26);
            this.buttonTrainDatFileNameOpen.TabIndex = 38;
            this.buttonTrainDatFileNameOpen.Text = "Open...";
            this.buttonTrainDatFileNameOpen.UseVisualStyleBackColor = true;
            this.buttonTrainDatFileNameOpen.Click += new System.EventHandler(this.ButtonTrainDatFileNameOpen_Click);
            // 
            // textBoxTrainDatFileName
            // 
            this.textBoxTrainDatFileName.Location = new System.Drawing.Point(72, 17);
            this.textBoxTrainDatFileName.Name = "textBoxTrainDatFileName";
            this.textBoxTrainDatFileName.Size = new System.Drawing.Size(120, 20);
            this.textBoxTrainDatFileName.TabIndex = 37;
            // 
            // labelTrainDatFileName
            // 
            this.labelTrainDatFileName.Location = new System.Drawing.Point(8, 17);
            this.labelTrainDatFileName.Name = "labelTrainDatFileName";
            this.labelTrainDatFileName.Size = new System.Drawing.Size(56, 17);
            this.labelTrainDatFileName.TabIndex = 39;
            this.labelTrainDatFileName.Text = "Filename:";
            this.labelTrainDatFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxTrainType
            // 
            this.comboBoxTrainType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTrainType.FormattingEnabled = true;
            this.comboBoxTrainType.Items.AddRange(new object[] {
            "Old format",
            "Train.xml"});
            this.comboBoxTrainType.Location = new System.Drawing.Point(88, 17);
            this.comboBoxTrainType.Name = "comboBoxTrainType";
            this.comboBoxTrainType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTrainType.TabIndex = 41;
            // 
            // labelTrainType
            // 
            this.labelTrainType.Location = new System.Drawing.Point(8, 17);
            this.labelTrainType.Name = "labelTrainType";
            this.labelTrainType.Size = new System.Drawing.Size(72, 17);
            this.labelTrainType.TabIndex = 40;
            this.labelTrainType.Text = "Type:";
            this.labelTrainType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // FormExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 341);
            this.Controls.Add(this.groupBoxPanel);
            this.Controls.Add(this.groupBoxSound);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxTrain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormExport";
            this.Text = "Export";
            this.Load += new System.EventHandler(this.FormExport_Load);
            this.groupBoxPanel.ResumeLayout(false);
            this.groupBoxPanelXml.ResumeLayout(false);
            this.groupBoxPanelXml.PerformLayout();
            this.groupBoxPanel2Cfg.ResumeLayout(false);
            this.groupBoxPanel2Cfg.PerformLayout();
            this.groupBoxSound.ResumeLayout(false);
            this.groupBoxSoundXml.ResumeLayout(false);
            this.groupBoxSoundXml.PerformLayout();
            this.groupBoxSoundCfg.ResumeLayout(false);
            this.groupBoxSoundCfg.PerformLayout();
            this.groupBoxTrain.ResumeLayout(false);
            this.groupBoxOldFormat.ResumeLayout(false);
            this.groupBoxTrainXML.ResumeLayout(false);
            this.groupBoxTrainXML.PerformLayout();
            this.groupBoxExtensionsCfg.ResumeLayout(false);
            this.groupBoxExtensionsCfg.PerformLayout();
            this.groupBoxTrainDat.ResumeLayout(false);
            this.groupBoxTrainDat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxPanel;
		private System.Windows.Forms.GroupBox groupBoxPanelXml;
		private System.Windows.Forms.Button buttonPanelXmlFileNameOpen;
		private System.Windows.Forms.TextBox textBoxPanelXmlFileName;
		private System.Windows.Forms.Label labelPanelXmlFileName;
		private System.Windows.Forms.GroupBox groupBoxPanel2Cfg;
		private System.Windows.Forms.Button buttonPanel2CfgFileNameOpen;
		private System.Windows.Forms.TextBox textBoxPanel2CfgFileName;
		private System.Windows.Forms.Label labelPanel2CfgFileName;
		private System.Windows.Forms.ComboBox comboBoxPanelType;
		private System.Windows.Forms.Label labelPanelType;
		private System.Windows.Forms.GroupBox groupBoxSound;
		private System.Windows.Forms.GroupBox groupBoxSoundXml;
		private System.Windows.Forms.Button buttonSoundXmlFileNameOpen;
		private System.Windows.Forms.TextBox textBoxSoundXmlFileName;
		private System.Windows.Forms.Label labelSoundXmlFileName;
		private System.Windows.Forms.GroupBox groupBoxSoundCfg;
		private System.Windows.Forms.Button buttonSoundCfgFileNameOpen;
		private System.Windows.Forms.TextBox textBoxSoundCfgFileName;
		private System.Windows.Forms.Label labelSoundCfgFileName;
		private System.Windows.Forms.ComboBox comboBoxSoundType;
		private System.Windows.Forms.Label labelSoundType;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.GroupBox groupBoxTrain;
		private System.Windows.Forms.GroupBox groupBoxOldFormat;
		private System.Windows.Forms.GroupBox groupBoxExtensionsCfg;
		private System.Windows.Forms.Button buttonExtensionsCfgFileNameOpen;
		private System.Windows.Forms.TextBox textBoxExtensionsCfgFileName;
		private System.Windows.Forms.Label labelExtensionsCfgFileName;
		private System.Windows.Forms.GroupBox groupBoxTrainDat;
		private System.Windows.Forms.Button buttonTrainDatFileNameOpen;
		private System.Windows.Forms.TextBox textBoxTrainDatFileName;
		private System.Windows.Forms.Label labelTrainDatFileName;
		private System.Windows.Forms.ComboBox comboBoxTrainType;
		private System.Windows.Forms.Label labelTrainType;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.GroupBox groupBoxTrainXML;
		private System.Windows.Forms.Button buttonTrainXMLFilenameOpen;
		private System.Windows.Forms.TextBox textBoxTrainXmlFilename;
		private System.Windows.Forms.Label labelTrainXMLFilename;
	}
}
