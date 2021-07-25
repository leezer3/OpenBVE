
using System.Drawing;
using LibRender2.Text;

namespace LibRender2.Text
{
	/// <summary>Provides fonts</summary>
	public class Fonts
	{
		/// <summary>Represents a very small sans serif font.</summary>
		public readonly OpenGlFont VerySmallFont;

		/// <summary>Represents a small sans serif font.</summary>
		public readonly OpenGlFont SmallFont;

		/// <summary>Represents a normal-sized sans serif font.</summary>
		public readonly OpenGlFont NormalFont;

		/// <summary>Represents a large sans serif font.</summary>
		public readonly OpenGlFont LargeFont;

		/// <summary>Represents a very large sans serif font.</summary>
		public readonly OpenGlFont VeryLargeFont;

		public readonly OpenGlFont EvenLargerFont;

		/// <summary>Gets the next smallest font</summary>
		/// <param name="currentFont">The font we require the smaller version for</param>
		/// <returns>The next smallest font</returns>
		public OpenGlFont NextSmallestFont(OpenGlFont currentFont)
		{
			switch ((int)currentFont.FontSize)
			{
				case 9:
				case 12:
					return VerySmallFont;
				case 16:
					return SmallFont;
				case 21:
					return NormalFont;
				case 27:
					return LargeFont;
				case 34:
					return VeryLargeFont;
				default:
					return EvenLargerFont;
			}
		}

		internal Fonts()
		{
			VerySmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 9.0f);
			SmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 12.0f);
			NormalFont = new OpenGlFont(FontFamily.GenericSansSerif, 16.0f);
			LargeFont = new OpenGlFont(FontFamily.GenericSansSerif, 21.0f);
			VeryLargeFont = new OpenGlFont(FontFamily.GenericSansSerif, 27.0f);
			EvenLargerFont = new OpenGlFont(FontFamily.GenericSansSerif, 34.0f);
		}
	}
}
