using System;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Holds the array of marker textures currently displayed in-game</summary>
		internal static Textures.Texture[] MarkerTextures = { };

		/// <summary>Adds a marker to be displayed</summary>
		/// <param name="Texture">The texture</param>
		internal static void AddMarker(Textures.Texture Texture)
		{
			int n = MarkerTextures.Length;
			Array.Resize<Textures.Texture>(ref MarkerTextures, n + 1);
			MarkerTextures[n] = Texture;
		}
		/// <summary>Removes a marker</summary>
		/// <param name="Texture">The texture</param>
		internal static void RemoveMarker(Textures.Texture Texture)
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
					Array.Resize<Textures.Texture>(ref MarkerTextures, n);
					break;
				}
			}
		}
	}
}
