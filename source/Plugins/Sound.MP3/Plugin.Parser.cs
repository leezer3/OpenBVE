using OpenBveApi.Sounds;
using NAudio.Wave;

namespace Plugin {
	public partial class Plugin : SoundInterface {
		/// <summary>Reads sound data from a MP3 file.</summary>
		/// <param name="fileName">The file name of the MP3 file.</param>
		/// <returns>The raw sound data.</returns>
		private static Sound LoadFromFile(string fileName)
		{
			using (var reader = new Mp3FileReader(fileName))
			{
				var pcmLength = (int)reader.Length;
				var leftBuffer = new byte[pcmLength / 2];
				var rightBuffer = new byte[pcmLength / 2];
				var buffer = new byte[pcmLength];
				var bytesRead = reader.Read(buffer, 0, pcmLength);

				int index = 0;
				//For simplicity just use let NLayer internally convert into raw 16-bit 2 channel PCM
				for (int i = 0; i < bytesRead; i += 4)
				{
					leftBuffer[index] = buffer[i];
					rightBuffer[index] = buffer[i + 2];
					index++;
					leftBuffer[index] = buffer[i + 1];
					rightBuffer[index] = buffer[i + 3];
					index++;
				}
				return new Sound(44100, 16, new byte[][] { leftBuffer, rightBuffer});
				
			}
		}
	}
}
