using OpenBveApi.Textures;

namespace OpenBve
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
	}
}
