namespace OpenBve {
    partial class formImage {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // formImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96.0f, 96.0f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.DoubleBuffered = true;
            this.MinimizeBox = false;
            this.Name = "formImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "openBVE";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.formImage_Paint);
            this.Resize += new System.EventHandler(this.formImage_Resize);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formImage_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

    }
}