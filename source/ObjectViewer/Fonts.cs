using System.Drawing;
using LibRender;

namespace OpenBve {
	/// <summary>Provides font support.</summary>
	internal static class Fonts {

		/// <summary>Represents a very small sans serif font.</summary>
		internal static readonly OpenGlFont VerySmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 9.0f);

		/// <summary>Represents a small sans serif font.</summary>
		internal static readonly OpenGlFont SmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 12.0f);
		
		/// <summary>Represents a normal-sized sans serif font.</summary>
		internal static readonly OpenGlFont NormalFont = new OpenGlFont(FontFamily.GenericSansSerif, 16.0f);

		/// <summary>Represents a large sans serif font.</summary>
		internal static readonly OpenGlFont LargeFont = new OpenGlFont(FontFamily.GenericSansSerif, 21.0f);

		/// <summary>Represents a very large sans serif font.</summary>
		internal static readonly OpenGlFont VeryLargeFont = new OpenGlFont(FontFamily.GenericSansSerif, 27.0f);

		internal static readonly OpenGlFont EvenLargerFont = new OpenGlFont(FontFamily.GenericSansSerif, 34.0f);
	}
}
