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
            this.labelPackageInstallTitle = new System.Windows.Forms.Label();
            this.labelPackageInstallText1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.PackageNextButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPackageInstallTitle
            // 
            this.labelPackageInstallTitle.AutoSize = true;
            this.labelPackageInstallTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPackageInstallTitle.Location = new System.Drawing.Point(12, 9);
            this.labelPackageInstallTitle.Name = "labelPackageInstallTitle";
            this.labelPackageInstallTitle.Size = new System.Drawing.Size(133, 16);
            this.labelPackageInstallTitle.TabIndex = 0;
            this.labelPackageInstallTitle.Text = "Install A Package:";
            // 
            // labelPackageInstallText1
            // 
            this.labelPackageInstallText1.AutoSize = true;
            this.labelPackageInstallText1.Location = new System.Drawing.Point(12, 218);
            this.labelPackageInstallText1.Name = "labelPackageInstallText1";
            this.labelPackageInstallText1.Size = new System.Drawing.Size(360, 13);
            this.labelPackageInstallText1.TabIndex = 1;
            this.labelPackageInstallText1.Text = "This wizard will allow you to install a packaged route or train into OpenBVE.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(128, 28);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(180, 180);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // PackageNextButton
            // 
            this.PackageNextButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageNextButton.Location = new System.Drawing.Point(378, 199);
            this.PackageNextButton.Name = "PackageNextButton";
            this.PackageNextButton.Size = new System.Drawing.Size(94, 42);
            this.PackageNextButton.TabIndex = 3;
            this.PackageNextButton.Text = "Next";
            this.PackageNextButton.UseVisualStyleBackColor = true;
            // 
            // formPackageInstall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.PackageNextButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelPackageInstallText1);
            this.Controls.Add(this.labelPackageInstallTitle);
            this.Name = "formPackageInstall";
            this.Text = "Package Installer";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPackageInstallTitle;
        private System.Windows.Forms.Label labelPackageInstallText1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button PackageNextButton;
    }
}