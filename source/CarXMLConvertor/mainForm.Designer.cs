namespace CarXmlConvertor
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelSelectTrain = new System.Windows.Forms.Label();
            this.buttonSelectFolder = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.radioButtonSingleFile = new System.Windows.Forms.RadioButton();
            this.radioButtonChildFiles = new System.Windows.Forms.RadioButton();
            this.panelCarXMLTypes = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panelCarXMLTypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.AllowDrop = true;
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(13, 57);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(293, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox1_DragDrop);
            this.textBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox1_DragEnter);
            // 
            // labelSelectTrain
            // 
            this.labelSelectTrain.AutoSize = true;
            this.labelSelectTrain.Location = new System.Drawing.Point(12, 37);
            this.labelSelectTrain.Name = "labelSelectTrain";
            this.labelSelectTrain.Size = new System.Drawing.Size(221, 13);
            this.labelSelectTrain.TabIndex = 1;
            this.labelSelectTrain.Text = "Please select the train folder to be converted:";
            // 
            // buttonSelectFolder
            // 
            this.buttonSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectFolder.Location = new System.Drawing.Point(312, 56);
            this.buttonSelectFolder.Name = "buttonSelectFolder";
            this.buttonSelectFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectFolder.TabIndex = 2;
            this.buttonSelectFolder.Text = "Select...";
            this.buttonSelectFolder.UseVisualStyleBackColor = true;
            this.buttonSelectFolder.Click += new System.EventHandler(this.buttonSelectFolder_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(10, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(265, 13);
            this.labelInfo.TabIndex = 3;
            this.labelInfo.Text = "This tool may be used to convert a train to XML format.";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(292, 417);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 35);
            this.button1.TabIndex = 4;
            this.button1.Text = "Convert!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.process_Click);
            // 
            // radioButtonSingleFile
            // 
            this.radioButtonSingleFile.AutoSize = true;
            this.radioButtonSingleFile.Checked = true;
            this.radioButtonSingleFile.Location = new System.Drawing.Point(3, 7);
            this.radioButtonSingleFile.Name = "radioButtonSingleFile";
            this.radioButtonSingleFile.Size = new System.Drawing.Size(73, 17);
            this.radioButtonSingleFile.TabIndex = 0;
            this.radioButtonSingleFile.TabStop = true;
            this.radioButtonSingleFile.Text = "Single File";
            this.radioButtonSingleFile.UseVisualStyleBackColor = true;
            // 
            // radioButtonChildFiles
            // 
            this.radioButtonChildFiles.AutoSize = true;
            this.radioButtonChildFiles.Location = new System.Drawing.Point(106, 8);
            this.radioButtonChildFiles.Name = "radioButtonChildFiles";
            this.radioButtonChildFiles.Size = new System.Drawing.Size(91, 17);
            this.radioButtonChildFiles.TabIndex = 1;
            this.radioButtonChildFiles.Text = "Child Car Files";
            this.radioButtonChildFiles.UseVisualStyleBackColor = true;
            // 
            // panelCarXMLTypes
            // 
            this.panelCarXMLTypes.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panelCarXMLTypes.Controls.Add(this.radioButtonSingleFile);
            this.panelCarXMLTypes.Controls.Add(this.radioButtonChildFiles);
            this.panelCarXMLTypes.Location = new System.Drawing.Point(93, 125);
            this.panelCarXMLTypes.Name = "panelCarXMLTypes";
            this.panelCarXMLTypes.Size = new System.Drawing.Size(200, 27);
            this.panelCarXMLTypes.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 96);
            this.label1.MaximumSize = new System.Drawing.Size(300, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(270, 26);
            this.label1.TabIndex = 8;
            this.label1.Text = "Please select whether to use a single Train.xml file, or a Train.xml file with ch" +
    "ild Car.xml files:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.Location = new System.Drawing.Point(15, 171);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(381, 240);
            this.textBoxOutput.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Logs:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 464);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelCarXMLTypes);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonSelectFolder);
            this.Controls.Add(this.labelSelectTrain);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 400);
            this.Name = "MainForm";
            this.Text = "Car XML Convertor";
            this.panelCarXMLTypes.ResumeLayout(false);
            this.panelCarXMLTypes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

	    internal System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label labelSelectTrain;
        private System.Windows.Forms.Button buttonSelectFolder;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.RadioButton radioButtonChildFiles;
		private System.Windows.Forms.RadioButton radioButtonSingleFile;
		private System.Windows.Forms.Panel panelCarXMLTypes;
		private System.Windows.Forms.Label label1;
	    internal System.Windows.Forms.TextBox textBoxOutput;
		private System.Windows.Forms.Label label2;
	}
}

