using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin : SoundInterface
	{

		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host)
		{
			// CurrentHost = host;
		}

		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public override bool CanLoadSound(string path)
		{
			if (File.Exists(path))
			{
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						byte[] magicNumber = reader.ReadBytes(3);

						// without an ID3 tag or with an ID3v1 tag
						if (magicNumber[0] == 0xFF && magicNumber[1] == 0xFB)
						{
							return true;
						}

						// with an ID3v2 tag
						if (magicNumber[0] == 0x49 && magicNumber[1] == 0x44 && magicNumber[2] == 0x33)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>Loads the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out Sound sound)
		{
			sound = LoadFromFile(path);
			return true;
		}
	}
}
