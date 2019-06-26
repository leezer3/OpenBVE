using NAudio.Wave;
using NLayer.NAudioSupport;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin
	{
		/// <summary>Reads sound data from a MP3 file.</summary>
		/// <param name="fileName">The file name of the MP3 file.</param>
		/// <returns>The raw sound data.</returns>
		private static Sound LoadFromFile(string fileName)
		{
			using (Mp3FileReader reader = new Mp3FileReader(fileName, wf => new Mp3FrameDecompressor(wf)))
			{
				int pcmLength = (int)reader.Length;
				byte[] leftBuffer = new byte[pcmLength / 2];
				byte[] rightBuffer = new byte[pcmLength / 2];
				byte[] buffer = new byte[pcmLength];
				int bytesRead = reader.Read(buffer, 0, pcmLength);

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

				return new Sound(44100, 16, new byte[][] { leftBuffer, rightBuffer });
			}
		}
	}
}
