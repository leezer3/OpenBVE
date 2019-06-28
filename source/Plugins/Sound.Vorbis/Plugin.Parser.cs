using System.IO;
using NAudio.Vorbis;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin
	{
		/// <summary>Reads sound data from a Ogg Vorbis file.</summary>
		/// <param name="fileName">The file name of the Ogg Vorbis file.</param>
		/// <returns>The raw sound data.</returns>
		private static Sound LoadFromFile(string fileName)
		{
			using (VorbisWaveReader reader = new VorbisWaveReader(fileName))
			{
				int sampleCount = (int)reader.Length / (reader.WaveFormat.Channels * sizeof(float));
				float[] dataFloats = new float[sampleCount * reader.WaveFormat.Channels];

				// Convert Ogg Vorbis to raw 32-bit float n channels PCM.
				int floatsRead = reader.Read(dataFloats, 0, sampleCount * reader.WaveFormat.Channels);

				byte[] dataBytes = new byte[floatsRead * sizeof(short)];
				byte[][] buffers = new byte[reader.WaveFormat.Channels][];

				for (int i = 0; i < buffers.Length; i++)
				{
					buffers[i] = new byte[dataBytes.Length / buffers.Length];
				}

				// Convert PCM bit depth from 32-bit float to 16-bit integer.
				using (MemoryStream stream = new MemoryStream(dataBytes))
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < floatsRead; i++)
					{
						float sample = dataFloats[i];

						if (sample < -1.0f)
						{
							sample = -1.0f;
						}

						if (sample > 1.0f)
						{
							sample = 1.0f;
						}

						writer.Write((short)(sample * short.MaxValue));
					}
				}

				// Separated for each channel.
				for (int i = 0; i < floatsRead / buffers.Length; i++)
				{
					for (int j = 0; j < buffers.Length; j++)
					{
						for (int k = 0; k < sizeof(short); k++)
						{
							buffers[j][i * sizeof(short) + k] = dataBytes[i * sizeof(short) * buffers.Length + sizeof(short) * j + k];
						}
					}
				}

				return new Sound(reader.WaveFormat.SampleRate, sizeof(short) * 8, buffers);
			}
		}
	}
}
