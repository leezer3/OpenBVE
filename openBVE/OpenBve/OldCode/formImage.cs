using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenBve {
    internal partial class formImage : Form {
        internal formImage() {
            InitializeComponent();
        }

        // show image dialog
        internal static void ShowImageDialog(Image Image) {
            formImage Dialog = new formImage();
            Dialog.CurrentImage = Image;
            Dialog.ShowDialog();
            Dialog.Dispose();
        }

        // members
        internal Image CurrentImage = null;

        // resize
        private void formImage_Resize(object sender, EventArgs e) {
            this.Invalidate();
        }

        // key down
        private void formImage_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                this.Close();
            }
        }

        // paint
        private void formImage_Paint(object sender, PaintEventArgs e) {
            float aw = (float)this.ClientRectangle.Width;
            float ah = (float)this.ClientRectangle.Height;
            float ar = aw / ah;
            float bw = (float)CurrentImage.Width;
            float bh = (float)CurrentImage.Height;
            float br = bw / bh;
            try {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                System.Drawing.Drawing2D.LinearGradientBrush Gradient = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(0, this.ClientRectangle.Height), SystemColors.Control, SystemColors.ControlDark);
                e.Graphics.FillRectangle(Gradient, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
                if (ar > br) {
                    e.Graphics.DrawImage(CurrentImage, new RectangleF(0.5f * (aw - ah * br), 0.0f, ah * br, ah), new RectangleF(0.0f, 0.0f, bw, bh), GraphicsUnit.Pixel);
                } else {
                    e.Graphics.DrawImage(CurrentImage, new RectangleF(0.0f, 0.5f * (ah - aw / br), aw, aw / br), new RectangleF(0.0f, 0.0f, bw, bh), GraphicsUnit.Pixel);
                }
            } catch { }
        }

    }
}