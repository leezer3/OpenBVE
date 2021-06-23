using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin : SoundInterface
	{
		// --- functions ---

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
						uint headerCkID = reader.ReadUInt32();

						// OggS
						if (headerCkID == 0x5367674F)
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
