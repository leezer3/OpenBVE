using System;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace LibRender2.Overlays
{
	public class Marker
	{
		/// <summary>Holds the array of marker textures currently displayed in-game</summary>
		public MarkerTexture[] MarkerTextures = { };

		internal Marker()
		{
		}

		/// <summary>Adds a marker to be displayed</summary>
		/// <param name="Texture">The texture</param>
		/// <param name="Size">The size to draw</param>
		public void AddMarker(Texture Texture, Vector2 Size)
		{
			int n = MarkerTextures.Length;
			Array.Resize(ref MarkerTextures, n + 1);
			MarkerTextures[n] = new MarkerTexture(Texture, Size);
		}

		/// <summary>Removes a marker</summary>
		/// <param name="Texture">The texture</param>
		public void RemoveMarker(Texture Texture)
		{
			int n = MarkerTextures.Length;

			for (int i = 0; i < n; i++)
			{
				if (MarkerTextures[i].Texture == Texture)
				{
					for (int j = i; j < n - 1; j++)
					{
						MarkerTextures[j] = MarkerTextures[j + 1];
					}

					n--;
					Array.Resize(ref MarkerTextures, n);
					break;
				}
			}
		}
	}

	public class MarkerTexture
	{
		public Texture Texture;
		public readonly Vector2 Size;

		internal MarkerTexture(Texture texture, Vector2 size)
		{
			Texture = texture;
			Size = size;
		}
	}
}
