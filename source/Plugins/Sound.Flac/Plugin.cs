using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace Plugin {
	public class Plugin : SoundInterface {
		// --- functions ---
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host) {
			// CurrentHost = host;
		}
		
		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public override bool CanLoadSound(string path) {
			if (File.Exists(path)) {
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					using (BinaryReader reader = new BinaryReader(stream)) {
						if (reader.ReadUInt32() != 0x43614C66) {
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
			// --- decode file ---
			int sampleRate;
			int bitsPerSample;
			int[][] samples = Flac.Decoder.Decode(path, out sampleRate, out bitsPerSample);
			// --- format data for API structure ---
			byte[][] bytes = new byte[samples.Length][];
			unchecked {
				int bytesPerSample = (int)((bitsPerSample + 7) >> 3);
				for (int i = 0; i < samples.Length; i++) {
					bytes[i] = new byte[samples[i].Length * bytesPerSample];
					if (bitsPerSample <= 8) {
						for (int j = 0; j < samples[i].Length; j++) {
							int value = (samples[i][j] >> 24) + 128;
							bytes[i][j] = (byte)value;
						}
						bitsPerSample = 8;
					} else {
						for (int j = 0; j < samples[i].Length; j++) {
							int value = samples[i][j] >> 16;
							bytes[i][2 * j + 0] = (byte)value;
							bytes[i][2 * j + 1] = (byte)(value >> 8);
						}
						bitsPerSample = 16;
					}
				}
			}
			sound = new Sound(sampleRate, bitsPerSample, bytes);
			return true;
		}
		
	}
}
