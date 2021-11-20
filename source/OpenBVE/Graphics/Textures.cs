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
				Program.Renderer.TextureManager.LoadTexture(ref TextureManager.RegisteredTextures[i], OpenGlTextureWrapMode.ClampClamp, CPreciseTimer.GetClockTicks(), Interface.CurrentOptions.Interpolation, Interface.CurrentOptions.AnisotropicFilteringLevel);
			}
		}

		internal static void UnloadUnusedTextures(double TimeElapsed)
		{
#if DEBUG
			//HACK: If when running in debug mode the frame time exceeds 1s, we can assume VS has hit a breakpoint
			//Don't unload textures in this case, as it just causes texture bugs
			if (TimeElapsed > 1000)
			{
				foreach (var Texture in TextureManager.RegisteredTextures)
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
				for(int i= 0; i < TextureManager.RegisteredTextures.Length; i++)
				{
					if (TextureManager.RegisteredTextures[i] != null && (CPreciseTimer.GetClockTicks() - TextureManager.RegisteredTextures[i].LastAccess) > 20000)
					{
						TextureManager.UnloadTexture(ref TextureManager.RegisteredTextures[i]);
					}
				}
			}
			else
			{
				//Don't unload textures if we are in a menu/ paused, as they may be required immediately after unpause
				foreach (var Texture in TextureManager.RegisteredTextures)
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
