namespace OpenBve
{
    partial class formAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAbout));
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.labelProductName = new System.Windows.Forms.Label();
            this.textBoxMain = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOpenSourceLicences = new System.Windows.Forms.TextBox();
            this.labelOpenSourceHeader = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxLogo.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(128, 128);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxLogo.TabIndex = 0;
            this.pictureBoxLogo.TabStop = false;
            // 
            // labelProductName
            // 
            this.labelProductName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProductName.AutoSize = true;
            this.labelProductName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProductName.Location = new System.Drawing.Point(146, 12);
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.Size = new System.Drawing.Size(201, 25);
            this.labelProductName.TabIndex = 0;
            this.labelProductName.Text = "openBVE v0.0.0.0";
            // 
            // textBoxMain
            // 
            this.textBoxMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMain.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxMain.Location = new System.Drawing.Point(147, 41);
            this.textBoxMain.Multiline = true;
            this.textBoxMain.Name = "textBoxMain";
            this.textBoxMain.ReadOnly = true;
            this.textBoxMain.Size = new System.Drawing.Size(315, 80);
            this.textBoxMain.TabIndex = 1;
            this.textBoxMain.TabStop = false;
            this.textBoxMain.Text = resources.GetString("textBoxMain.Text");
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Open-Source Licences";
            // 
            // textBoxOpenSourceLicences
            // 
            this.textBoxOpenSourceLicences.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOpenSourceLicences.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxOpenSourceLicences.Location = new System.Drawing.Point(17, 188);
            this.textBoxOpenSourceLicences.Multiline = true;
            this.textBoxOpenSourceLicences.Name = "textBoxOpenSourceLicences";
            this.textBoxOpenSourceLicences.ReadOnly = true;
            this.textBoxOpenSourceLicences.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOpenSourceLicences.Size = new System.Drawing.Size(454, 160);
            this.textBoxOpenSourceLicences.TabIndex = 4;
            this.textBoxOpenSourceLicences.TabStop = false;
            this.textBoxOpenSourceLicences.Text = resources.GetString("textBoxOpenSourceLicences.Text");
            // 
            // labelOpenSourceHeader
            // 
            this.labelOpenSourceHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOpenSourceHeader.AutoSize = true;
            this.labelOpenSourceHeader.Location = new System.Drawing.Point(17, 169);
            this.labelOpenSourceHeader.Name = "labelOpenSourceHeader";
            this.labelOpenSourceHeader.Size = new System.Drawing.Size(445, 13);
            this.labelOpenSourceHeader.TabIndex = 3;
            this.labelOpenSourceHeader.Text = "OpenBVE makes use of several open-source libraries, whose licences are reproduced" +
    " below:";
            // 
            // buttonClose
            // 
            this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClose.Location = new System.Drawing.Point(328, 354);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(144, 24);
            this.buttonClose.TabIndex = 5;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // formAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 387);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.labelOpenSourceHeader);
            this.Controls.Add(this.textBoxOpenSourceLicences);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxMain);
            this.Controls.Add(this.labelProductName);
            this.Controls.Add(this.pictureBoxLogo);
            this.Name = "formAbout";
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.TextBox textBoxMain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOpenSourceLicences;
        private System.Windows.Forms.Label labelOpenSourceHeader;
        private System.Windows.Forms.Button buttonClose;
    }
}