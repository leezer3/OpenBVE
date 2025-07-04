using FontStashSharp;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace LibRender2.Text
{
	/// <summary>Represents a font.</summary>
	public sealed class OpenGlFont
	{
		private readonly BaseRenderer renderer;
		/// <summary>The size of the underlying font in pixels.</summary>
		public readonly float FontSize;
		
		private DynamicSpriteFont Font;

		private bool disposed;

		// --- constructors ---
		/// <summary>Creates a new font.</summary>
		public OpenGlFont(BaseRenderer renderererReference, DynamicSpriteFont font, float size)
		{
			renderer = renderererReference;
			Font = font;
			FontSize= size;
		}

		/// <summary>Measures the size of a string as it would be rendered using this font.</summary>
		/// <param name="text">The string to render.</param>
		/// <returns>The size of the string.</returns>
		public Vector2 MeasureString(string text)
		{
			Vector2f size = Font.MeasureString(text);
			return new Vector2(size.X, size.Y);
		}

		public void DrawString(string text, double left, double top, Color128 color)
		{
			renderer.FontStashRenderer.Begin();
			Font.DrawText(renderer.FontStashRenderer, text, new Vector2f(left, top), color);
			renderer.FontStashRenderer.End();
			renderer.RestoreBlendFunc();
		}
	}
}
