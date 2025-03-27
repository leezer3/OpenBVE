using System;
using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;

namespace Texture.Tga
{
	/// <summary>Implements the texture interface.</summary>
	public partial class Plugin : TextureInterface, IDisposable
	{

		// --- members ---

		/// <summary>The host that loaded the plugin.</summary>
		private HostInterface CurrentHost = null;


		// --- functions ---

		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host)
		{
			CurrentHost = host;
		}

		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height)
		{
			width = 0;
			height = 0;
			return true;
		}

		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public override bool CanLoadTexture(string path)
		{
			if (path != null && File.Exists(path) && Path.GetExtension(path).ToLower() == ".tga")
			{
				return true;
			}
			return false;
		}

		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(string path, out OpenBveApi.Textures.Texture texture)
		{
			try
			{
				return Parse(path, out texture);
			}
			catch
			{
				texture = null;
				return false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool currentlyDisposing)
		{
			if(currentlyDisposing)
			{
				bitmap.Dispose();
			}
		}

	}
}
