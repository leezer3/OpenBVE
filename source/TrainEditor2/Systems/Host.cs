using System;
using System.IO;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using TrainEditor2.Systems.Functions;

namespace TrainEditor2.Systems
{
	/// <summary>Represents the host application.</summary>
	internal class Host : HostInterface
	{
		public Host() : base(HostApplication.TrainEditor) { }

		// --- texture ---

		public override bool LoadTexture(Texture Texture, OpenGlTextureWrapMode wrapMode)
		{
			return Program.Renderer.TextureManager.LoadTexture(Texture, wrapMode, Environment.TickCount, InterpolationMode.BilinearMipmapped, 16);
		}

		// --- sound ---

		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out Sound sound)
		{
			if (File.Exists(path) || Directory.Exists(path))
			{
				foreach (Plugins.Plugin plugin in Plugins.LoadedPlugins)
				{
					if (plugin.Sound != null)
					{
						try
						{
							if (plugin.Sound.CanLoadSound(path))
							{
								try
								{
									if (plugin.Sound.LoadSound(path, out sound))
									{
										return true;
									}
								}
								catch (Exception)
								{
									// ignored
								}
							}
						}
						catch (Exception)
						{
							// ignored
						}
					}
				}
			}

			sound = null;
			return false;
		}
	}
}
