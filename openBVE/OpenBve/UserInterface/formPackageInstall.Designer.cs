namespace OpenBve
{
    partial class formPackageInstall
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
            this.PackageNextButton = new System.Windows.Forms.Button();
            this.PackagePanel1 = new System.Windows.Forms.Panel();
            this.PackageInstallImage = new System.Windows.Forms.PictureBox();
            this.labelPackageInstallText1 = new System.Windows.Forms.Label();
            this.labelPackageInstallTitle = new System.Windows.Forms.Label();
            this.PackagePanel2 = new System.Windows.Forms.Panel();
            this.labelPackageInstallSelectPackage = new System.Windows.Forms.Label();
            this.labelSelectPackageText1 = new System.Windows.Forms.Label();
            this.labelSelectPackageText2 = new System.Windows.Forms.Label();
            this.PackageButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.PackageType = new System.Windows.Forms.Label();
            this.PackageTypeText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.PackageFilename = new System.Windows.Forms.TextBox();
            this.PackageName = new System.Windows.Forms.Label();
            this.PackageNameText = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PackageAuthor = new System.Windows.Forms.Label();
            this.PackageVersionText = new System.Windows.Forms.Label();
            this.PackageVersion = new System.Windows.Forms.Label();
            this.PackageImage = new System.Windows.Forms.PictureBox();
            this.PackageTypeImage = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.PackagePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackageInstallImage)).BeginInit();
            this.PackagePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackageImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PackageTypeImage)).BeginInit();
            this.SuspendLayout();
            // 
            // PackageNextButton
            // 
            this.PackageNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackageNextButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageNextButton.Location = new System.Drawing.Point(378, 199);
            this.PackageNextButton.Name = "PackageNextButton";
            this.PackageNextButton.Size = new System.Drawing.Size(94, 42);
            this.PackageNextButton.TabIndex = 3;
            this.PackageNextButton.Text = "Next";
            this.PackageNextButton.UseVisualStyleBackColor = true;
            this.PackageNextButton.Click += new System.EventHandler(this.PackageNextButton_Click);
            // 
            // PackagePanel1
            // 
            this.PackagePanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackagePanel1.Controls.Add(this.PackageInstallImage);
            this.PackagePanel1.Controls.Add(this.labelPackageInstallTitle);
            this.PackagePanel1.Controls.Add(this.labelPackageInstallText1);
            this.PackagePanel1.Location = new System.Drawing.Point(0, 0);
            this.PackagePanel1.Name = "PackagePanel1";
            this.PackagePanel1.Size = new System.Drawing.Size(375, 255);
            this.PackagePanel1.TabIndex = 4;
            // 
            // PackageInstallImage
            // 
            this.PackageInstallImage.Location = new System.Drawing.Point(125, 25);
            this.PackageInstallImage.Name = "PackageInstallImage";
            this.PackageInstallImage.Size = new System.Drawing.Size(180, 180);
            this.PackageInstallImage.TabIndex = 7;
            this.PackageInstallImage.TabStop = false;
            // 
            // labelPackageInstallText1
            // 
            this.labelPackageInstallText1.AutoSize = true;
            this.labelPackageInstallText1.Location = new System.Drawing.Point(9, 215);
            this.labelPackageInstallText1.Name = "labelPackageInstallText1";
            this.labelPackageInstallText1.Size = new System.Drawing.Size(360, 13);
            this.labelPackageInstallText1.TabIndex = 6;
            this.labelPackageInstallText1.Text = "This wizard will allow you to install a packaged route or train into OpenBVE.";
            // 
            // labelPackageInstallTitle
            // 
            this.labelPackageInstallTitle.AutoSize = true;
            this.labelPackageInstallTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPackageInstallTitle.Location = new System.Drawing.Point(9, 6);
            this.labelPackageInstallTitle.Name = "labelPackageInstallTitle";
            this.labelPackageInstallTitle.Size = new System.Drawing.Size(133, 16);
            this.labelPackageInstallTitle.TabIndex = 5;
            this.labelPackageInstallTitle.Text = "Install A Package:";
            // 
            // PackagePanel2
            // 
            this.PackagePanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackagePanel2.Controls.Add(this.PackageTypeImage);
            this.PackagePanel2.Controls.Add(this.PackageImage);
            this.PackagePanel2.Controls.Add(this.PackageVersionText);
            this.PackagePanel2.Controls.Add(this.PackageVersion);
            this.PackagePanel2.Controls.Add(this.label6);
            this.PackagePanel2.Controls.Add(this.PackageAuthor);
            this.PackagePanel2.Controls.Add(this.PackageNameText);
            this.PackagePanel2.Controls.Add(this.PackageName);
            this.PackagePanel2.Controls.Add(this.PackageFilename);
            this.PackagePanel2.Controls.Add(this.label3);
            this.PackagePanel2.Controls.Add(this.PackageTypeText);
            this.PackagePanel2.Controls.Add(this.PackageType);
            this.PackagePanel2.Controls.Add(this.label1);
            this.PackagePanel2.Controls.Add(this.PackageButton);
            this.PackagePanel2.Controls.Add(this.labelSelectPackageText2);
            this.PackagePanel2.Controls.Add(this.labelSelectPackageText1);
            this.PackagePanel2.Controls.Add(this.labelPackageInstallSelectPackage);
            this.PackagePanel2.Location = new System.Drawing.Point(0, 0);
            this.PackagePanel2.Name = "PackagePanel2";
            this.PackagePanel2.Size = new System.Drawing.Size(375, 255);
            this.PackagePanel2.TabIndex = 5;
            // 
            // labelPackageInstallSelectPackage
            // 
            this.labelPackageInstallSelectPackage.AutoSize = true;
            this.labelPackageInstallSelectPackage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPackageInstallSelectPackage.Location = new System.Drawing.Point(12, 9);
            this.labelPackageInstallSelectPackage.Name = "labelPackageInstallSelectPackage";
            this.labelPackageInstallSelectPackage.Size = new System.Drawing.Size(122, 16);
            this.labelPackageInstallSelectPackage.TabIndex = 6;
            this.labelPackageInstallSelectPackage.Text = "Select Package:";
            // 
            // labelSelectPackageText1
            // 
            this.labelSelectPackageText1.AutoSize = true;
            this.labelSelectPackageText1.Location = new System.Drawing.Point(12, 29);
            this.labelSelectPackageText1.Name = "labelSelectPackageText1";
            this.labelSelectPackageText1.Size = new System.Drawing.Size(168, 13);
            this.labelSelectPackageText1.TabIndex = 7;
            this.labelSelectPackageText1.Text = "Please select a package to install.";
            // 
            // labelSelectPackageText2
            // 
            this.labelSelectPackageText2.AutoSize = true;
            this.labelSelectPackageText2.Location = new System.Drawing.Point(14, 85);
            this.labelSelectPackageText2.Name = "labelSelectPackageText2";
            this.labelSelectPackageText2.Size = new System.Drawing.Size(361, 13);
            this.labelSelectPackageText2.TabIndex = 8;
            this.labelSelectPackageText2.Text = "This may be a route or train packaged into .obp format, or a legacy archive.";
            // 
            // PackageButton
            // 
            this.PackageButton.Location = new System.Drawing.Point(125, 45);
            this.PackageButton.Name = "PackageButton";
            this.PackageButton.Size = new System.Drawing.Size(133, 37);
            this.PackageButton.TabIndex = 10;
            this.PackageButton.Text = "Select Package...";
            this.PackageButton.UseVisualStyleBackColor = true;
            this.PackageButton.Click += new System.EventHandler(this.PackageButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Selected Package Details:";
            // 
            // PackageType
            // 
            this.PackageType.AutoSize = true;
            this.PackageType.Location = new System.Drawing.Point(14, 138);
            this.PackageType.Name = "PackageType";
            this.PackageType.Size = new System.Drawing.Size(80, 13);
            this.PackageType.TabIndex = 13;
            this.PackageType.Text = "Package Type:";
            // 
            // PackageTypeText
            // 
            this.PackageTypeText.AutoSize = true;
            this.PackageTypeText.Location = new System.Drawing.Point(122, 138);
            this.PackageTypeText.Name = "PackageTypeText";
            this.PackageTypeText.Size = new System.Drawing.Size(109, 13);
            this.PackageTypeText.TabIndex = 14;
            this.PackageTypeText.Text = "No package selected";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Package Filename:";
            // 
            // PackageFilename
            // 
            this.PackageFilename.Enabled = false;
            this.PackageFilename.Location = new System.Drawing.Point(125, 115);
            this.PackageFilename.Name = "PackageFilename";
            this.PackageFilename.Size = new System.Drawing.Size(244, 20);
            this.PackageFilename.TabIndex = 16;
            // 
            // PackageName
            // 
            this.PackageName.AutoSize = true;
            this.PackageName.Location = new System.Drawing.Point(14, 157);
            this.PackageName.Name = "PackageName";
            this.PackageName.Size = new System.Drawing.Size(84, 13);
            this.PackageName.TabIndex = 17;
            this.PackageName.Text = "Package Name:";
            // 
            // PackageNameText
            // 
            this.PackageNameText.AutoSize = true;
            this.PackageNameText.Location = new System.Drawing.Point(122, 157);
            this.PackageNameText.Name = "PackageNameText";
            this.PackageNameText.Size = new System.Drawing.Size(109, 13);
            this.PackageNameText.TabIndex = 18;
            this.PackageNameText.Text = "No package selected";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(122, 176);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "No package selected";
            // 
            // PackageAuthor
            // 
            this.PackageAuthor.AutoSize = true;
            this.PackageAuthor.Location = new System.Drawing.Point(14, 176);
            this.PackageAuthor.Name = "PackageAuthor";
            this.PackageAuthor.Size = new System.Drawing.Size(87, 13);
            this.PackageAuthor.TabIndex = 19;
            this.PackageAuthor.Text = "Package Author:";
            // 
            // PackageVersionText
            // 
            this.PackageVersionText.AutoSize = true;
            this.PackageVersionText.Location = new System.Drawing.Point(122, 195);
            this.PackageVersionText.Name = "PackageVersionText";
            this.PackageVersionText.Size = new System.Drawing.Size(109, 13);
            this.PackageVersionText.TabIndex = 22;
            this.PackageVersionText.Text = "No package selected";
            // 
            // PackageVersion
            // 
            this.PackageVersion.AutoSize = true;
            this.PackageVersion.Location = new System.Drawing.Point(14, 195);
            this.PackageVersion.Name = "PackageVersion";
            this.PackageVersion.Size = new System.Drawing.Size(91, 13);
            this.PackageVersion.TabIndex = 21;
            this.PackageVersion.Text = "Package Version:";
            // 
            // PackageImage
            // 
            this.PackageImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PackageImage.Location = new System.Drawing.Point(240, 138);
            this.PackageImage.Name = "PackageImage";
            this.PackageImage.Size = new System.Drawing.Size(114, 114);
            this.PackageImage.TabIndex = 23;
            this.PackageImage.TabStop = false;
            // 
            // PackageTypeImage
            // 
            this.PackageTypeImage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PackageTypeImage.Location = new System.Drawing.Point(103, 138);
            this.PackageTypeImage.Name = "PackageTypeImage";
            this.PackageTypeImage.Size = new System.Drawing.Size(16, 16);
            this.PackageTypeImage.TabIndex = 24;
            this.PackageTypeImage.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // formPackageInstall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.PackagePanel2);
            this.Controls.Add(this.PackagePanel1);
            this.Controls.Add(this.PackageNextButton);
            this.Name = "formPackageInstall";
            this.Text = "Package Installer";
            this.PackagePanel1.ResumeLayout(false);
            this.PackagePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackageInstallImage)).EndInit();
            this.PackagePanel2.ResumeLayout(false);
            this.PackagePanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PackageImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PackageTypeImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button PackageNextButton;
        private System.Windows.Forms.Panel PackagePanel1;
        private System.Windows.Forms.PictureBox PackageInstallImage;
        private System.Windows.Forms.Label labelPackageInstallTitle;
        private System.Windows.Forms.Label labelPackageInstallText1;
        private System.Windows.Forms.Panel PackagePanel2;
        private System.Windows.Forms.PictureBox PackageImage;
        private System.Windows.Forms.Label PackageVersionText;
        private System.Windows.Forms.Label PackageVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label PackageAuthor;
        private System.Windows.Forms.Label PackageNameText;
        private System.Windows.Forms.Label PackageName;
        private System.Windows.Forms.TextBox PackageFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label PackageTypeText;
        private System.Windows.Forms.Label PackageType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button PackageButton;
        private System.Windows.Forms.Label labelSelectPackageText2;
        private System.Windows.Forms.Label labelSelectPackageText1;
        private System.Windows.Forms.Label labelPackageInstallSelectPackage;
        private System.Windows.Forms.PictureBox PackageTypeImage;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}