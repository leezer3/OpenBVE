using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBveApi.Graphics
{
	/// <summary>Defines a platform-independent interface for basic 2D drawing capabilities</summary>
	public interface IGLRenderer
	{
		/// <summary>Loads a texture into the graphics device</summary>
		bool LoadTexture(ref Texture texture, OpenGlTextureWrapMode wrapMode);

		/// <summary>Draws a simple solid-colored rectangle or textured rectangle</summary>
		void DrawRectangle(Texture texture, Vector2 location, Vector2 size, Color128 color, OpenGlTextureWrapMode? wrapMode = null, float cornerRadius = 0f);

		/// <summary>Draws a rectangle with alpha scaling</summary>
		void DrawRectangleAlpha(Texture texture, Vector2 location, Vector2 size, Color128 color, Vector2 scale, OpenGlTextureWrapMode? wrapMode = null, float cornerRadius = 0f);

		/// <summary>Draws text on screen</summary>
		void DrawText(string text, Vector2 location, Color128 color);
	}
}
