using System;
using OpenBve.RouteManager;
using OpenBveApi.Textures;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Adds a marker to be displayed</summary>
		/// <param name="Texture">The texture</param>
		internal static void AddMarker(Texture Texture)
		{
			int n = CurrentRoute.MarkerTextures.Length;
			Array.Resize<Texture>(ref CurrentRoute.MarkerTextures, n + 1);
			CurrentRoute.MarkerTextures[n] = Texture;
		}
		/// <summary>Removes a marker</summary>
		/// <param name="Texture">The texture</param>
		internal static void RemoveMarker(Texture Texture)
		{
			int n = CurrentRoute.MarkerTextures.Length;
			for (int i = 0; i < n; i++)
			{
				if (CurrentRoute.MarkerTextures[i] == Texture)
				{
					for (int j = i; j < n - 1; j++)
					{
						CurrentRoute.MarkerTextures[j] = CurrentRoute.MarkerTextures[j + 1];
					}
					n--;
					Array.Resize<Texture>(ref CurrentRoute.MarkerTextures, n);
					break;
				}
			}
		}
	}
}
