using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;

namespace Plugin {
	/// <summary>Implements the texture interface.</summary>
	public class Plugin : TextureInterface {
		// --- functions ---
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host) {
			// CurrentHost = host;
		}
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height) {
			//QueryDimensionsFromFile(path, out width, out height);
			width = 0;
			height = 0;
			return true;
		}
		
		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public override bool CanLoadTexture(string path)
		{
			if (File.Exists(path)) {
				using (Stream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (BinaryReader reader = new BinaryReader(fileStream))
					{
						if (fileStream.Length < 4)
						{
							return false;
						}
						byte[] signature = reader.ReadBytes(4);
						if (!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
						{
							return false;
						}
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(string path, out OpenBveApi.Textures.Texture texture)
		{
			byte[] b = System.IO.File.ReadAllBytes(path);
			DDSImage d = new DDSImage(b);
			texture = d.myTexture;
			return true;
		}
		
	}
}
