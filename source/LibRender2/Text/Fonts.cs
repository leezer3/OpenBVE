﻿using System.Drawing;
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

		/// <summary>Gets the next largest font</summary>
		/// <param name="currentFont">The font we require the larger version for</param>
		/// <returns>The next larger font</returns>
		public OpenGlFont NextLargestFont(OpenGlFont currentFont)
		{
			switch ((int)currentFont.FontSize)
			{
				case 9:
					return SmallFont;
				case 12:
					return NormalFont;
				case 16:
					return LargeFont;
				case 21:
					return VeryLargeFont;
				default:
					return EvenLargerFont;
			}
		}
		
		public Fonts(HostInterface host, string fontName)
		{
			currentHost = host;
			FontFamily uiFont = FontFamily.GenericSansSerif;
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
			VerySmallFont = new OpenGlFont(uiFont, 9.0f);
			SmallFont = new OpenGlFont(uiFont, 12.0f);
			NormalFont = new OpenGlFont(uiFont, 16.0f);
			LargeFont = new OpenGlFont(uiFont, 21.0f);
			VeryLargeFont = new OpenGlFont(uiFont, 27.0f);
			EvenLargerFont = new OpenGlFont(uiFont, 34.0f);
		}
	}
}
