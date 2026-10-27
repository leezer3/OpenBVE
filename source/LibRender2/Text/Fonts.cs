using System.Drawing;
using System.IO;
using FontStashSharp;
using OpenBveApi.Hosts;

namespace LibRender2.Text
{
	/// <summary>Provides fonts</summary>
	public partial class Fonts
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

		/// <summary>Represents the largest sans serif font.</summary>
		public readonly OpenGlFont EvenLargerFont;


		private static HostInterface currentHost;

		private readonly FontSystem fontSystem;
		private readonly FontFamily uiFont;
		private readonly float scaleFactor;

		/// <summary>Gets the next smallest font</summary>
		/// <param name="currentFont">The font we require the smaller version for</param>
		/// <returns>The next smallest font</returns>
		public OpenGlFont NextSmallestFont(OpenGlFont currentFont)
		{
			if (currentFont == VerySmallFont || currentFont == SmallFont)
			{
				return VerySmallFont;
			}
			if (currentFont == NormalFont)
			{
				return SmallFont;
			}
			if (currentFont == LargeFont)
			{
				return NormalFont;
			}
			if (currentFont == VeryLargeFont)
			{
				return LargeFont;
			}
			if (currentFont == EvenLargerFont)
			{
				return VeryLargeFont;
			}
			return EvenLargerFont;
		}

		/// <summary>Gets the next largest font</summary>
		/// <param name="currentFont">The font we require the larger version for</param>
		/// <returns>The next larger font</returns>
		public OpenGlFont NextLargestFont(OpenGlFont currentFont)
		{
			if (currentFont == null)
			{
				throw new InvalidDataException("No font was specified.");
			}
			if (currentFont == VerySmallFont)
			{
				return SmallFont;
			}
			if (currentFont == SmallFont)
			{
				return NormalFont;
			}
			if (currentFont == NormalFont)
			{
				return LargeFont;
			}
			if (currentFont == LargeFont)
			{
				return VeryLargeFont;
			}
			return EvenLargerFont;
		}
		
		public Fonts(HostInterface host, BaseRenderer renderer, string fontName)
		{
			currentHost = host;
			uiFont = FontFamily.GenericSansSerif;
			if (!string.IsNullOrEmpty(fontName))
			{
				try
				{
					FontFamily newFont = new FontFamily(fontName);
					uiFont = newFont;
				}
				catch
				{
					currentHost.ReportProblem(ProblemType.InvalidOperation, "Failed to load font " + fontName);
				}
				
			}

			fontSystem = OpenGlFont.CreateFontSystem(FontFileResolver.GetFontData(fontName));
			scaleFactor = (float)renderer.ScaleFactor.X;

			VerySmallFont = new OpenGlFont(uiFont, 9.0f * scaleFactor, fontSystem.GetFont(9.0f * scaleFactor));
			SmallFont = new OpenGlFont(uiFont, 12.0f * scaleFactor, fontSystem.GetFont(12.0f * scaleFactor));
			NormalFont = new OpenGlFont(uiFont, 16.0f * scaleFactor, fontSystem.GetFont(16.0f * scaleFactor));
			LargeFont = new OpenGlFont(uiFont, 21.0f * scaleFactor, fontSystem.GetFont(21.0f * scaleFactor));
			VeryLargeFont = new OpenGlFont(uiFont, 27.0f * scaleFactor, fontSystem.GetFont(27.0f * scaleFactor));
			EvenLargerFont = new OpenGlFont(uiFont, 34.0f * scaleFactor, fontSystem.GetFont(34.0f * scaleFactor));

			SetUserScale(renderer.currentOptions.UserInterfaceScaleFactor);
		}

		/// <summary>Applies the user interface scale factor to all fonts.</summary>
		internal void SetUserScale(int factor)
		{
			float scale = scaleFactor * System.Math.Max(factor, 1);

			VerySmallFont.SetFont(9.0f * scale, fontSystem.GetFont(9.0f * scale));
			SmallFont.SetFont(12.0f * scale, fontSystem.GetFont(12.0f * scale));
			NormalFont.SetFont(16.0f * scale, fontSystem.GetFont(16.0f * scale));
			LargeFont.SetFont(21.0f * scale, fontSystem.GetFont(21.0f * scale));
			VeryLargeFont.SetFont(27.0f * scale, fontSystem.GetFont(27.0f * scale));
			EvenLargerFont.SetFont(34.0f * scale, fontSystem.GetFont(34.0f * scale));
		}
	}
}
