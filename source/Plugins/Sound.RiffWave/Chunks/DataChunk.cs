using System;
using System.IO;

namespace Plugin
{
    internal class DataChunk : Chunk
    {
	    public DataChunk(byte[] dataBytes, WaveFormatEx format) : base(format.Channels)
	    {
		    if (!(format is WaveFormatAdPcm))
		    {
			    BytesPerSample = format.BitsPerSample / 8;
		    }
		    else
		    {
			    BytesPerSample = 2;
		    }

		    // The size of the data chunk may not be correct, so we use sampleCount as the standard thereafter.
		    int sampleCount = dataBytes.Length / (format.Channels * BytesPerSample);

		    byte[][] buffers = new byte[format.Channels][];

		    for (int i = 0; i < format.Channels; i++)
		    {
			    buffers[i] = new byte[sampleCount * BytesPerSample];
		    }

		    for (int i = 0; i < sampleCount; i++)
		    {
			    for (int j = 0; j < format.Channels; j++)
			    {
				    for (int k = 0; k < BytesPerSample; k++)
				    {
					    buffers[j][i * BytesPerSample + k] = dataBytes[i * BytesPerSample * format.Channels + BytesPerSample * j + k];
				    }
			    }
		    }

		    Buffers = ChangeBitDepth(format.FormatTag, ref BytesPerSample, sampleCount, buffers);
	    }

		private static byte[][] ChangeBitDepth(WFormatTag formatTag, ref int bytesPerSample, int sampleCount, byte[][] buffers)
		{
			byte[][] newBuffers = new byte[buffers.Length][];

			for (int i = 0; i < buffers.Length; i++)
			{
				newBuffers[i] = new byte[sampleCount * sizeof(short)];
			}

			switch (formatTag)
			{
				case WFormatTag.Extensible:
				case WFormatTag.PCM:
					switch (bytesPerSample)
					{
						case 3:
							for (int i = 0; i < buffers.Length; i++)
							{
								using (var stream = new MemoryStream(newBuffers[i]))
								using (var writer = new BinaryWriter(stream))
								{
									for (int j = 0; j < buffers[i].Length; j += 3)
									{
										byte[] tmp = new byte[4];
										Array.Copy(buffers[i], j, tmp, 0, 3);
										tmp[3] = (byte)(tmp[2] > 0x7F ? 0xff : 0x00);
										float sample = BitConverter.ToInt32(tmp, 0) / 8388608.0f;
										writer.Write((short)(sample * short.MaxValue));
									}
								}
							}

							bytesPerSample = sizeof(short);
							break;
						case 4:
							for (int i = 0; i < buffers.Length; i++)
							{
								using (var stream = new MemoryStream(newBuffers[i]))
								using (var writer = new BinaryWriter(stream))
								{
									for (int j = 0; j < buffers[i].Length; j += 4)
									{
										float sample = BitConverter.ToInt32(buffers[i], j) / 2147483648.0f;
										writer.Write((short)(sample * short.MaxValue));
									}
								}
							}

							bytesPerSample = sizeof(short);
							break;
						default:
							newBuffers = buffers;
							break;
					}
					break;
				case WFormatTag.IEEE_Float:
					for (int i = 0; i < buffers.Length; i++)
					{
						using (var stream = new MemoryStream(newBuffers[i]))
						using (var writer = new BinaryWriter(stream))
						{
							for (int j = 0; j < buffers[i].Length; j += 4)
							{
								float sample = BitConverter.ToSingle(buffers[i], j);

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
					}

					bytesPerSample = sizeof(short);
					break;
				default:
					newBuffers = buffers;
					break;
			}

			return newBuffers;
		}
	}
}
