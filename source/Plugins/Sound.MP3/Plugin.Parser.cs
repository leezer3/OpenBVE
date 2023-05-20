//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
