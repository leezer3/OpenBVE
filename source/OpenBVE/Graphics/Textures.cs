using LibRender2.Screens;
using LibRender2.Textures;
using OpenBveApi;
using OpenBveApi.Textures;

namespace OpenBve.Graphics
{
	/// <summary>Provides functions for dealing with textures.</summary>
	internal static class Textures
	{
		/// <summary>Loads all registered textures.</summary>
		internal static void LoadAllTextures()
		{
			for (int i = 0; i < Program.Renderer.TextureManager.RegisteredTexturesCount; i++)
			{
				Program.Renderer.TextureManager.LoadTexture(Program.Renderer.TextureManager.RegisteredTextures[i], OpenGlTextureWrapMode.ClampClamp, CPreciseTimer.GetClockTicks(), Interface.CurrentOptions.Interpolation, Interface.CurrentOptions.AnisotropicFilteringLevel);
			}
		}

		internal static void UnloadUnusedTextures(double TimeElapsed)
		{
#if DEBUG
			//HACK: If when running in debug mode the frame time exceeds 1s, we can assume VS has hit a breakpoint
			//Don't unload textures in this case, as it just causes texture bugs
			if (TimeElapsed > 1000)
			{
				foreach (var Texture in Program.Renderer.TextureManager.RegisteredTextures)
				{
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
#endif
			if (Program.Renderer.CurrentInterface == InterfaceType.Normal)
			{
				foreach (var Texture in Program.Renderer.TextureManager.RegisteredTextures)
				{
					if (Texture != null && (CPreciseTimer.GetClockTicks() - Texture.LastAccess) > 20000)
					{
						TextureManager.UnloadTexture(Texture);
					}
				}
			}
			else
			{
				//Don't unload textures if we are in a menu/ paused, as they may be required immediately after unpause
				foreach (var Texture in Program.Renderer.TextureManager.RegisteredTextures)
				{
					//Texture can be null in certain cases....
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
		}
	}
}
