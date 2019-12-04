using System.Drawing;
using LibRender2.Texts;

namespace TrainEditor2.Systems
{
	/// <summary>Provides font support.</summary>
	internal static class Fonts
	{
		// --- read-only fields ---

		/// <summary>Represents a very small sans serif font.</summary>
		internal static readonly OpenGlFont VerySmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 9.0f);
	}
}
