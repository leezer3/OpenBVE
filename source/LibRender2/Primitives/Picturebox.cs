using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Primitives
{
	public class Picturebox
	{
		/// <summary>Holds a reference to the base renderer</summary>
		private readonly BaseRenderer Renderer;
		/// <summary>The texture for the picturebox</summary>
		public Texture Texture;
		/// <summary>The background color for the picturebox</summary>
		public Color128 BackgroundColor;
		/// <summary>The image sizing mode</summary>
		public ImageSizeMode SizeMode;
		/// <summary>The stored location for the textbox</summary>
		public Vector2 Location;
		/// <summary>The stored size for the textbox</summary>
		public Vector2 Size;

		private bool flipX;
		private bool flipY;

		public Picturebox(BaseRenderer renderer)
		{
			Renderer = renderer;
			SizeMode = ImageSizeMode.Zoom;
		}

		public void Draw()
		{
			if (!Renderer.currentHost.LoadTexture(ref Texture, OpenGlTextureWrapMode.ClampClamp))
			{
				return;
			}
			
			GL.DepthMask(true);
			Vector2 newSize;
			switch (SizeMode)
			{
				case ImageSizeMode.Normal:
					//Draw box containing backing color first
					Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
					//Calculate the new size
					newSize = new Vector2(Texture.Width, Texture.Height);
					if (newSize.X > Size.X)
					{
						newSize.X = Size.X;
					}

					if (newSize.Y > Size.Y)
					{
						newSize.Y = Size.Y;
					}
					//Two-pass draw the texture in appropriate place
					Renderer.Rectangle.DrawAlpha(Texture, Location, newSize, Color128.White, new Vector2(newSize / Size));
					break;
				case ImageSizeMode.Center:
					//Draw box containing backing color first
					Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
					//Calculate the new size
					newSize = new Vector2(Texture.Width, Texture.Height);
					if (newSize.X > Size.X)
					{
						newSize.X = Size.X;
					}

					if (newSize.Y > Size.Y)
					{
						newSize.Y = Size.Y;
					}
					//Two-pass draw the texture in appropriate place
					Renderer.Rectangle.DrawAlpha(Texture, Location + new Vector2(newSize - Size) / 2, newSize, Color128.White, new Vector2(newSize / Size));
					break;
				case ImageSizeMode.Stretch:
					//No neeed to draw a backing color box as texture covers the whole thing
					Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
					break;
				case ImageSizeMode.Zoom:
					//Draw box containing backing color first
					Renderer.Rectangle.Draw(null, Location, Size, BackgroundColor);
					//Calculate the new size
					double ratioW = Size.X / Texture.Width;
					double ratioH = Size.Y / Texture.Height;
					double newRatio = ratioW < ratioH ? ratioW : ratioH;
					newSize = new Vector2(Texture.Width, Texture.Height) * newRatio;
					OpenGlTextureWrapMode wrapMode = OpenGlTextureWrapMode.ClampClamp;
					if (flipX)
					{
						wrapMode = OpenGlTextureWrapMode.RepeatClamp;
					}

					if (flipY)
					{
						wrapMode = wrapMode == OpenGlTextureWrapMode.RepeatClamp ? OpenGlTextureWrapMode.RepeatRepeat : OpenGlTextureWrapMode.ClampRepeat;
					}
					Renderer.Rectangle.DrawAlpha(Texture, new Vector2(Location.X + (Size.X - newSize.X) / 2,Location.Y + (Size.Y - newSize.Y) / 2), newSize, Color128.White, new Vector2(flipX ? -1 : 1,flipY ? -1 : 1), wrapMode);
					break;
			}
		}

		public void Flip(bool FlipX, bool FlipY)
		{
			flipX = FlipX;
			flipY = FlipY;
		}

	}
}
