using FontStashSharp;
using LibRender2.Shaders;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;

namespace LibRender2.Text
{
	public class OpenGlString
	{
		private readonly BaseRenderer renderer;

		private readonly Shader Shader;

		internal OpenGlString(BaseRenderer renderer)
		{
			this.renderer = renderer;
			try
			{
				this.Shader = new Shader(renderer, "text", "text", true);
			}
			catch
			{
				renderer.ForceLegacyOpenGL = true;
			}
		}
		
		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Vector2 location, TextAlignment alignment, Color128 color)
		{
			if (text == null || font == null)
			{
				return;
			}
			
			renderer.LastBoundTexture = null;
			/*
			 * Prepare the top-left coordinates for rendering, incorporating the
			 * orientation of the string in relation to the specified location.
			 * */
			double left;

			Vector2f size = font.MeasureString(text);

			if ((alignment & TextAlignment.Left) == 0)
			{
				double width = size.X;

				if ((alignment & TextAlignment.Right) != 0)
				{
					left = location.X - width;
				}
				else
				{
					left = location.X - width / 2;
				}
			}
			else
			{
				left = location.X;
			}

			double top;

			if ((alignment & TextAlignment.Top) == 0)
			{
				double height = size.Y;

				if ((alignment & TextAlignment.Bottom) != 0)
				{
					top = location.Y - height;
				}
				else
				{
					top = location.Y - height / 2;
				}
			}
			else
			{
				top = location.Y;
			}

			font.DrawString(text, left, top, color);
		}


	
		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <param name="shadow">Whether to draw a shadow.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Vector2 location, TextAlignment alignment, Color128 color, bool shadow)
		{
			if (shadow)
			{
				Draw(font, text, new Vector2(location.X - 1, location.Y + 1), alignment, new Color128(0.0f, 0.0f, 0.0f, 0.5f * color.A));
				Draw(font, text, location, alignment, color);
			}
			else
			{
				Draw(font, text, location, alignment, color);
			}
		}
	}
}
