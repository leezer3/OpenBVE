using System;
using System.Drawing;

namespace OpenBve {
	internal static class Fonts {

		// characters
		internal struct Character {
			internal float Width;
			internal float Height;
			internal int Texture;
		}
		internal static Character[][] Characters = new Character[][] { };
		internal static float ExtraSmallFontSize = 9.0f;
		internal static float SmallFontSize = 12.0f;
		internal static float MediumFontSize = 16.0f;
		internal static float LargeFontSize = 24.0f;
		internal static float ExtraLargeFontSize = 36.0f;
		internal enum FontType {
			ExtraSmall = 0,
			Small = 1,
			Medium = 2,
			Large = 3,
			ExtraLarge = 4
		}

		// initialize
		internal static void Initialize() {
			const int n = 5;
			UnregisterTextures();
			Characters = new Character[n][];
			for (int i = 0; i < n; i++) {
				Characters[i] = new Character[] { };
			}
		}

		// unregister textures
		private static void UnregisterTextures() {
			for (int i = 0; i < Characters.Length; i++) {
				for (int j = 0; j < Characters[i].Length; j++) {
					TextureManager.UnregisterTexture(ref Characters[i][j].Texture);
				}
			}
		}

		// get opengl texture index
		internal static int GetTextureIndex(FontType FontType, int Codepoint) {
			int Font = (int)FontType;
			string t = char.ConvertFromUtf32(Codepoint);
			int i = char.ConvertToUtf32(t, 0);
			if (i >= Characters[Font].Length || Characters[Font][i].Texture == -1) {
				if (Characters[Font].Length == 0) {
					Characters[Font] = new Character[i + 1];
					for (int j = 0; j <= i; j++) {
						Characters[Font][j].Texture = -1;
					}
				}
				while (i >= Characters[Font].Length) {
					int n = Characters[Font].Length;
					Array.Resize<Character>(ref Characters[Font], 2 * n);
					for (int j = n; j < 2 * n; j++) {
						Characters[Font][j].Texture = -1;
					}
				}
				float s1;
				switch (Font) {
						case 0: s1 = ExtraSmallFontSize; break;
						case 1: s1 = SmallFontSize; break;
						case 2: s1 = MediumFontSize; break;
						case 3: s1 = LargeFontSize; break;
						case 4: s1 = ExtraLargeFontSize; break;
						default: s1 = SmallFontSize; break;
				}
				int s0w = Interface.RoundToPowerOfTwo((int)Math.Ceiling((double)s1 * 1.25));
				int s0h = s0w;
				FontStyle fs = Font == 0 ? FontStyle.Regular : FontStyle.Regular;
				Bitmap b = new Bitmap(s0w, s0h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				Graphics g = Graphics.FromImage(b);
				g.Clear(Color.Black);
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				Font f = new Font(FontFamily.GenericSansSerif, s1, fs, GraphicsUnit.Pixel);
				SizeF s = g.MeasureString(t, f, s0w, StringFormat.GenericTypographic);
				g.DrawString(t, f, Brushes.White, 0.0f, 0.0f);
				g.Dispose();
				Characters[Font][i].Texture = TextureManager.RegisterTexture(b, false);
				Characters[Font][i].Width = s.Width <= 0.05f ? 4.0f : (float)Math.Ceiling((double)s.Width);
				Characters[Font][i].Height = s.Height <= 0.05f ? 4.0f : (float)Math.Ceiling((double)s.Height);
				b.Dispose();
			}
			return Characters[Font][i].Texture;
		}

	}
}