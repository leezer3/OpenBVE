using System.Drawing;
using OpenBveApi.Math;

namespace LibRender2.Texts
{
	/// <summary>Represents a single character.</summary>
	public struct OpenGlFontChar
	{
		// --- members ---
		/// <summary>The texture coordinates that represent the character in the underlying texture.</summary>
		public Vector4 TextureCoordinates;
		/// <summary>The physical size of the character.</summary>
		public Vector2 PhysicalSize;
		/// <summary>The typographic size of the character.</summary>
		public Vector2 TypographicSize;

		// --- constructors ---
		/// <summary>Creates a new character.</summary>
		/// <param name="textureCoordinates">The texture coordinates that represent the character in the underlying texture.</param>
		/// <param name="physicalSize">The physical size of the character.</param>
		/// <param name="typographicSize">The typographic size of the character.</param>
		public OpenGlFontChar(Vector4 textureCoordinates, Vector2 physicalSize, Vector2 typographicSize)
		{
			TextureCoordinates = textureCoordinates;
			PhysicalSize = physicalSize;
			TypographicSize = typographicSize;
		}
	}
}
