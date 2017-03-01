using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace OpenBve
{
    /// <summary>Provides extension methods for working with images</summary>
    public static class ImageExtensions
    {
        /// <summary>Converts an image to a byte array</summary>
        /// <param name="image">The image to convert</param>
        /// <param name="format">The format of the image</param>
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        /// <summary>Combines a RGB bitmap and a greyscale mask into a RGBA bitmap</summary>
        /// <param name="b">The output bitmap</param>
        /// <param name="s">The RGB source bitmap</param>
        /// <param name="a">The greyscale mask</param>
        /// <returns></returns>
        public static bool CombineBitmapAlpha(Bitmap b, Bitmap s, Bitmap a)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData smData = s.LockBits(new Rectangle(0, 0, s.Width, s.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData amData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* pDestData = (byte*)(void*)bmData.Scan0;
                byte* pSourceData = (byte*)(void*)smData.Scan0;
                byte* pMaskData = (byte*)(void*)amData.Scan0;

                int nDestOffset = bmData.Stride - b.Width * 4;
                int nSourceOffset = smData.Stride - s.Width * 3;
                //If a BW mask, then this should be amData.Stride - a.Width
                //According to Loksim devs, these are both handled identically (Presume as grayscale)
                int nMaskOffset = amData.Stride - a.Width * 4;

                for (int y = 0; y < s.Height; ++y)
                {
                    for (int x = 0; x < s.Width; ++x)
                    {
                        pDestData[0] = pSourceData[0];
                        pDestData[1] = pSourceData[1];
                        pDestData[2] = pSourceData[2];
                        pDestData[3] = pMaskData[0];

                        pSourceData += 3;
                        pDestData += 4;
                        pMaskData += 4;
                    }
                    pSourceData += nSourceOffset;
                    pDestData += nDestOffset;
                    pMaskData += nMaskOffset;
                }
            }

            b.UnlockBits(bmData);
            s.UnlockBits(smData);
            a.UnlockBits(amData);

            return true;
        }
    }
}
