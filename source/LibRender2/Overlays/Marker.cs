using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace LibRender2.Overlays
{
	public class Marker
	{

		private readonly BaseRenderer Renderer;
		/// <summary>Holds the array of marker textures currently displayed in-game</summary>
		public List<MarkerTexture> MarkerTextures;

		internal Marker(BaseRenderer renderer)
		{
			Renderer = renderer;
			MarkerTextures = new List<MarkerTexture>();
		}

		/// <summary>Adds a marker to be displayed</summary>
		/// <param name="texture">The texture</param>
		/// <param name="size">The size to draw</param>
		public void AddMarker(Texture texture, Vector2 size)
		{
			MarkerTextures.Add(new MarkerTexture(texture, size));
		}

		/// <summary>Removes a marker</summary>
		/// <param name="markerTexture">The texture</param>
		public void RemoveMarker(Texture markerTexture)
		{
			for (int i = MarkerTextures.Count - 1; i >= 0; i--)
			{
				if (MarkerTextures[i].Texture == markerTexture)
				{
					MarkerTextures.RemoveAt(i);
				}
			}
		}

		public void Draw(int yCoordinate)
		{
			for (int i = 0; i < MarkerTextures.Count; i++)
			{
				if (Renderer.currentHost.LoadTexture(ref MarkerTextures[i].Texture, OpenGlTextureWrapMode.ClampClamp))
				{
					double w = MarkerTextures[i].Size.X == 0 ? MarkerTextures[i].Texture.Width : MarkerTextures[i].Size.X;
					double h = MarkerTextures[i].Size.Y == 0 ? MarkerTextures[i].Texture.Height : MarkerTextures[i].Size.Y;
					GL.Color4(1.0, 1.0, 1.0, 1.0);
					Renderer.Rectangle.Draw(MarkerTextures[i].Texture, new Vector2(Renderer.Screen.Width - w - 8, yCoordinate), new Vector2(w, h));
					yCoordinate += (int)h + 8;
				}
			}
		}
	}

	public class MarkerTexture
	{
		/// <summary>The texture</summary>
		public Texture Texture;
		/// <summary>The size in pixels of the drawing surface</summary>
		/// <remarks>If zero, the size of the texture will be used</remarks>
		public readonly Vector2 Size;

		internal MarkerTexture(Texture texture, Vector2 size)
		{
			Texture = texture;
			Size = size;
		}
	}
}
