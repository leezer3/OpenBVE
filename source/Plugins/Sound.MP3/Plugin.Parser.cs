using System;
using System.IO;
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
				byte[] dataBytes = new byte[reader.Length];

				// Convert MP3 to raw 32-bit float n channels PCM.
				int bytesRead = reader.Read(dataBytes, 0, (int)reader.Length);

				int sampleCount = bytesRead / (reader.WaveFormat.Channels * sizeof(float));
				byte[] newDataBytes = new byte[sampleCount * reader.WaveFormat.Channels * sizeof(short)];
				byte[][] buffers = new byte[reader.WaveFormat.Channels][];

				for (int i = 0; i < buffers.Length; i++)
				{
					buffers[i] = new byte[newDataBytes.Length / buffers.Length];
				}

				// Convert PCM bit depth from 32-bit float to 16-bit integer.
				using (MemoryStream stream = new MemoryStream(newDataBytes))
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < bytesRead; i += sizeof(float))
					{
						float sample = BitConverter.ToSingle(dataBytes, i);

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
				for (int i = 0; i < sampleCount; i++)
				{
					for (int j = 0; j < buffers.Length; j++)
					{
						for (int k = 0; k < sizeof(short); k++)
						{
							buffers[j][i * sizeof(short) + k] = newDataBytes[i * sizeof(short) * buffers.Length + sizeof(short) * j + k];
						}
					}
				}

				return new Sound(reader.WaveFormat.SampleRate, sizeof(short) * 8, buffers);
			}
		}
	}
}
