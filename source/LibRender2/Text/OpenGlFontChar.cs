using System.Drawing;

namespace LibRender2.Texts
{
	/// <summary>Represents a single character.</summary>
	public struct OpenGlFontChar
	{
		// --- members ---
		/// <summary>The texture coordinates that represent the character in the underlying texture.</summary>
		public RectangleF TextureCoordinates;
		/// <summary>The physical size of the character.</summary>
		public Size PhysicalSize;
		/// <summary>The typographic size of the character.</summary>
		public Size TypographicSize;

		// --- constructors ---
		/// <summary>Creates a new character.</summary>
		/// <param name="textureCoordinates">The texture coordinates that represent the character in the underlying texture.</param>
		/// <param name="physicalSize">The physical size of the character.</param>
		/// <param name="typographicSize">The typographic size of the character.</param>
		public OpenGlFontChar(RectangleF textureCoordinates, Size physicalSize, Size typographicSize)
		{
			TextureCoordinates = textureCoordinates;
			PhysicalSize = physicalSize;
			TypographicSize = typographicSize;
		}
	}
}
