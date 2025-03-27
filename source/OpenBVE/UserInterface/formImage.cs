using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OpenBve {
    internal partial class FormImage : Form {
	    private FormImage() {
            InitializeComponent();
        }

        // show image dialog
        internal static void ShowImageDialog(Image imageToShow) {
            FormImage imageDialog = new FormImage {currentImage = imageToShow};
            imageDialog.ShowDialog();
            imageDialog.Dispose();
        }

        // members
	    private Image currentImage;

        // resize
        private void formImage_Resize(object sender, EventArgs e) {
            Invalidate();
        }

        // key down
        private void formImage_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Close();
            }
        }

        // paint
        private void formImage_Paint(object sender, PaintEventArgs e) {
            float aw = ClientRectangle.Width;
            float ah = ClientRectangle.Height;
            float ar = aw / ah;
            float bw = currentImage.Width;
            float bh = currentImage.Height;
            float br = bw / bh;
            try
            {
	            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
	            LinearGradientBrush gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, ClientRectangle.Height), SystemColors.Control, SystemColors.ControlDark);
	            e.Graphics.FillRectangle(gradientBrush, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
	            e.Graphics.DrawImage(currentImage, ar > br ? new RectangleF(0.5f * (aw - ah * br), 0.0f, ah * br, ah) : new RectangleF(0.0f, 0.5f * (ah - aw / br), aw, aw / br), new RectangleF(0.0f, 0.0f, bw, bh), GraphicsUnit.Pixel);
            }
            catch
            {
				// Ignored
            }
        }

    }
}
