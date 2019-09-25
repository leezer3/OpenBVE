using System;
using OpenBveApi.Textures;

namespace LibRender2.Overlays
{
	public class Marker
	{
		/// <summary>Holds the array of marker textures currently displayed in-game</summary>
		public Texture[] MarkerTextures = { };

		internal Marker()
		{
		}

		/// <summary>Adds a marker to be displayed</summary>
		/// <param name="Texture">The texture</param>
		public void AddMarker(Texture Texture)
		{
			int n = MarkerTextures.Length;
			Array.Resize(ref MarkerTextures, n + 1);
			MarkerTextures[n] = Texture;
		}

		/// <summary>Removes a marker</summary>
		/// <param name="Texture">The texture</param>
		public void RemoveMarker(Texture Texture)
		{
			int n = MarkerTextures.Length;

			for (int i = 0; i < n; i++)
			{
				if (MarkerTextures[i] == Texture)
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
}
