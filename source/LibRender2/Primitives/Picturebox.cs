using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Primitives
{
	public class Picturebox : GLControl
	{
		/// <summary>The image sizing mode</summary>
		public ImageSizeMode SizeMode;
		
		private bool flipX;
		private bool flipY;

		public Picturebox(BaseRenderer renderer) : base(renderer)
		{
			SizeMode = ImageSizeMode.Zoom;
		}

		public override void Draw()
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
					newSize = Texture.Size;
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
					newSize = Texture.Size;
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
					Vector2 ratio = Size / Texture.Size;
					double newRatio = ratio.X < ratio.Y ? ratio.X : ratio.Y;
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

		/// <summary>Flips the image displayed in the picturebox</summary>
		/// <param name="FlipX">Whether to flip the X axis</param>
		/// <param name="FlipY">Whether to flip the Y axis</param>
		public void Flip(bool FlipX, bool FlipY)
		{
			flipX = FlipX;
			flipY = FlipY;
		}
	}
}
