using System;
using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace Plugin {
	public partial class Plugin : SoundInterface {

		// --- members ---
		
		/// <summary>The host that loaded the plugin.</summary>
		private HostInterface CurrentHost = null;
		
		
		// --- functions ---
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host) {
			CurrentHost = host;
		}
		
		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public override bool CanLoadSound(string path) {
			if (File.Exists(path)) {
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
					using (BinaryReader reader = new BinaryReader(stream)) {
						Endianness endianness;
						uint headerCkID = reader.ReadUInt32();
						if (headerCkID == 0x46464952) {
							endianness = Endianness.Little;
						} else if (headerCkID == 0x58464952) {
							endianness = Endianness.Big;
						} else {
							return false;
						}
						uint headerCkSize = ReadUInt32(reader, endianness);
						uint formType = ReadUInt32(reader, endianness);
						if (formType != 0x45564157) {
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
		
		/// <summary>Loads the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out Sound sound) {
			sound = LoadFromFile(path);
			return true;
		}
		
	}
}