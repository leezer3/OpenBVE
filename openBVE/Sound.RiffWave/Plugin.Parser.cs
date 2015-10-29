#pragma warning disable 0660, 0661

using System;
using System.IO;
using OpenBveApi.Sounds;

namespace Plugin {
	public partial class Plugin : SoundInterface {
		
		// --- structures and enumerations ---
		
		/// <summary>Represents the format of wave data.</summary>
		private struct WaveFormat {
			// members
			/// <summary>The number of samples per second per channel.</summary>
			internal int SampleRate;
			/// <summary>The number of bits per sample.</summary>
			internal int BitsPerSample;
			/// <summary>The number of channels.</summary>
			internal int Channels;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="sampleRate">The number of samples per second per channel.</param>
			/// <param name="bitsPerSample">The number of bits per sample.</param>
			/// <param name="channels">The number of channels.</param>
			internal WaveFormat(int sampleRate, int bitsPerSample, int channels) {
				this.SampleRate = sampleRate;
				this.BitsPerSample = bitsPerSample;
				this.Channels = channels;
			}
			// operators
			public static bool operator ==(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return false;
				if (a.BitsPerSample != b.BitsPerSample) return false;
				if (a.Channels != b.Channels) return false;
				return true;
			}
			public static bool operator !=(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return true;
				if (a.BitsPerSample != b.BitsPerSample) return true;
				if (a.Channels != b.Channels) return true;
				return false;
			}
		}
		
		/// <summary>Represents wave data.</summary>
		private class WaveData {
			// members
			/// <summary>The format of the wave data.</summary>
			internal WaveFormat Format;
			/// <summary>The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</summary>
			internal byte[] Bytes;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="format">The format of the wave data.</param>
			/// <param name="bytes">The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</param>
			internal WaveData(WaveFormat format, byte[] bytes) {
				this.Format = format;
				this.Bytes = bytes;
			}
		}
		
		/// <summary>Represents the endianness of an integer.</summary>
		private enum Endianness {
			/// <summary>Represents little endian byte order, i.e. least-significant byte first.</summary>
			Little = 0,
			/// <summary>Represents big endian byte order, i.e. most-significant byte first.</summary>
			Big = 1
		}
		
		
		// --- format-specific data ---
		
		/// <summary>Represents format-specific data.</summary>
		private abstract class FormatData {
			internal int BlockSize;
		}
		
		/// <summary>Represents PCM-specific data.</summary>
		private class PcmData : FormatData { }
		
		/// <summary>Represents Microsoft-ADPCM-specific data.</summary>
		private class MicrosoftAdPcmData : FormatData {
			// structures
			internal struct ChannelData {
				internal int bPredictor;
				internal short iDelta;
				internal short iSamp1;
				internal short iSamp2;
				internal int iCoef1;
				internal int iCoef2;
			}
			// members
			internal int SamplesPerBlock;
			internal short[][] Coefficients = null;
			// read-only fields
			internal static readonly short[] AdaptionTable = new short[] {
				230, 230, 230, 230, 307, 409, 512, 614,
				768, 614, 512, 409, 307, 230, 230, 230
			};
		}

		
		// --- functions ---
		
		/// <summary>Reads wave data from a WAVE file.</summary>
		/// <param name="fileName">The file name of the WAVE file.</param>
		/// <returns>The wave data.</returns>
		private static Sound LoadFromFile(string fileName) {
			byte[] fileBytes = File.ReadAllBytes(fileName);
			using (MemoryStream stream = new MemoryStream(fileBytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// RIFF/RIFX chunk
					Endianness endianness;
					uint headerCkID = reader.ReadUInt32();
					if (headerCkID == 0x46464952) {
						endianness = Endianness.Little;
					} else if (headerCkID == 0x58464952) {
						endianness = Endianness.Big;
					} else {
						throw new InvalidDataException("Invalid chunk ID");
					}
					uint headerCkSize = ReadUInt32(reader, endianness);
					uint formType = ReadUInt32(reader, endianness);
					if (formType != 0x45564157) {
						throw new InvalidDataException("Unsupported format");
					}
					// data chunks
					WaveFormat format = new WaveFormat();
					FormatData data = null;
					byte[] dataBytes = null;
					while (stream.Position + 8 <= stream.Length) {
						uint ckID = reader.ReadUInt32();
						uint ckSize = ReadUInt32(reader, endianness);
						if (ckID == 0x20746d66) {
							// "fmt " chunk
							if (ckSize < 14) {
								throw new InvalidDataException("Unsupported fmt chunk size");
							}
							ushort wFormatTag = ReadUInt16(reader, endianness);
							ushort wChannels = ReadUInt16(reader, endianness);
							uint dwSamplesPerSec = ReadUInt32(reader, endianness);
							if (dwSamplesPerSec >= 0x80000000) {
								throw new InvalidDataException("Unsupported dwSamplesPerSec");
							}
							uint dwAvgBytesPerSec = ReadUInt32(reader, endianness);
							ushort wBlockAlign = ReadUInt16(reader, endianness);
							if (wFormatTag == 1) {
								// PCM
								if (ckSize < 16) {
									throw new InvalidDataException("Unsupported fmt chunk size");
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								stream.Position += ckSize - 16;
								if (wBitsPerSample < 1) {
									throw new InvalidDataException("Unsupported wBitsPerSample");
								}
								if (wBlockAlign != ((wBitsPerSample + 7) / 8) * wChannels) {
									throw new InvalidDataException("Unexpected wBlockAlign");
								}
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = (int)wBitsPerSample;
								format.Channels = (int)wChannels;
								PcmData pcmData = new PcmData();
								pcmData.BlockSize = (int)wBlockAlign;
								data = pcmData;
							} else if (wFormatTag == 2) {
								// Microsoft ADPCM
								if (ckSize < 22) {
									throw new InvalidDataException("Unsupported fmt chunk size");
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								if (wBitsPerSample != 4) {
									throw new InvalidDataException("Unsupported wBitsPerSample");
								}
								ushort cbSize = ReadUInt16(reader, endianness);
								MicrosoftAdPcmData adpcmData = new MicrosoftAdPcmData();
								adpcmData.SamplesPerBlock = ReadUInt16(reader, endianness);
								if (adpcmData.SamplesPerBlock == 0 | adpcmData.SamplesPerBlock > 2 * ((int)wBlockAlign - 6)) {
									throw new InvalidDataException("Unexpected nSamplesPerBlock");
								}
								ushort wNumCoef = ReadUInt16(reader, endianness);
								if (ckSize < 22 + 4 * wNumCoef) {
									throw new InvalidDataException("Unsupported fmt chunk size");
								}
								adpcmData.Coefficients = new short[wNumCoef][];
								unchecked {
									for (int i = 0; i < wNumCoef; i++) {
										adpcmData.Coefficients[i] = new short[] {
											(short)ReadUInt16(reader, endianness),
											(short)ReadUInt16(reader, endianness)
										};
									}
								}
								stream.Position += ckSize - (22 + 4 * wNumCoef);
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = 16;
								format.Channels = (int)wChannels;
								adpcmData.BlockSize = wBlockAlign;
								data = adpcmData;
							} else {
								// unsupported format
								throw new InvalidDataException("Unsupported wFormatTag");
							}
						} else if (ckID == 0x61746164) {
							// "data" chunk
							if (ckSize >= 0x80000000) {
								throw new InvalidDataException("Unsupported data chunk size");
							}
							if (data is PcmData) {
								// PCM
								int bytesPerSample = (format.BitsPerSample + 7) / 8;
								int samples = (int)ckSize / (format.Channels * bytesPerSample);
								int dataSize = samples * format.Channels * bytesPerSample;
								dataBytes = reader.ReadBytes(dataSize);
								stream.Position += ckSize - dataSize;
							} else if (data is MicrosoftAdPcmData) {
								// Microsoft ADPCM
								MicrosoftAdPcmData adpcmData = (MicrosoftAdPcmData)data;
								int blocks = (int)ckSize / adpcmData.BlockSize;
								dataBytes = new byte[2 * blocks * format.Channels * adpcmData.SamplesPerBlock];
								int position = 0;
								for (int i = 0; i < blocks; i++) {
									unchecked {
										MicrosoftAdPcmData.ChannelData[] channelData = new MicrosoftAdPcmData.ChannelData[format.Channels];
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].bPredictor = (int)reader.ReadByte();
											if (channelData[j].bPredictor >= adpcmData.Coefficients.Length) {
												throw new InvalidDataException("Invalid bPredictor");
											} else {
												channelData[j].iCoef1 = (int)adpcmData.Coefficients[channelData[j].bPredictor][0];
												channelData[j].iCoef2 = (int)adpcmData.Coefficients[channelData[j].bPredictor][1];
											}
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iDelta = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp1 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp2 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp2;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp2 >> 8);
											position += 2;
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp1;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp1 >> 8);
											position += 2;
										}
										uint nibbleByte = 0;
										bool nibbleFirst = true;
										for (int j = 0; j < adpcmData.SamplesPerBlock - 2; j++) {
											for (int k = 0; k < format.Channels; k++) {
												int lPredSample =
													(int)channelData[k].iSamp1 * channelData[k].iCoef1 +
													(int)channelData[k].iSamp2 * channelData[k].iCoef2 >> 8;
												int iErrorDeltaUnsigned;
												if (nibbleFirst) {
													nibbleByte = (uint)reader.ReadByte();
													iErrorDeltaUnsigned = (int)(nibbleByte >> 4);
													nibbleFirst = false;
												} else {
													iErrorDeltaUnsigned = (int)(nibbleByte & 15);
													nibbleFirst = true;
												}
												int iErrorDeltaSigned =
													iErrorDeltaUnsigned >= 8 ? iErrorDeltaUnsigned - 16 : iErrorDeltaUnsigned;
												int lNewSampInt =
													lPredSample + (int)channelData[k].iDelta * iErrorDeltaSigned;
												short lNewSamp =
													lNewSampInt <= -32768 ? (short)-32768 :
													lNewSampInt >= 32767 ? (short)32767 :
													(short)lNewSampInt;
												channelData[k].iDelta = (short)(
													(int)channelData[k].iDelta *
													(int)MicrosoftAdPcmData.AdaptionTable[iErrorDeltaUnsigned] >> 8
												);
												if (channelData[k].iDelta < 16) {
													channelData[k].iDelta = 16;
												}
												channelData[k].iSamp2 = channelData[k].iSamp1;
												channelData[k].iSamp1 = lNewSamp;
												dataBytes[position] = (byte)(ushort)lNewSamp;
												dataBytes[position + 1] = (byte)((ushort)lNewSamp >> 8);
												position += 2;
											}
										}
									}
									stream.Position += adpcmData.BlockSize - (format.Channels * (adpcmData.SamplesPerBlock - 2) + 1 >> 1) - 7 * format.Channels;
								}
								stream.Position += (int)ckSize - blocks * adpcmData.BlockSize;
							} else {
								// invalid
								throw new InvalidDataException("No fmt chunk before the data chunk");
							}
						} else {
							// unsupported chunk
							stream.Position += (long)ckSize;
						}
						// pad byte
						if ((ckSize & 1) == 1) {
							stream.Position++;
						}
					}
					// finalize
					if (dataBytes == null) {
						throw new InvalidDataException("No data chunk before the end of the file");
					} else if (format.Channels == 1) {
						return new Sound(format.SampleRate, ((format.BitsPerSample + 7) >> 3) << 3, new byte[][] { dataBytes });
					} else {
						byte[][] bytes = new byte[format.Channels][];
						for (int i = 0; i < format.Channels; i++) {
							bytes[i] = new byte[dataBytes.Length / format.Channels];
						}
						int bytesPerSample = (format.BitsPerSample + 7) >> 3;
						int samples = dataBytes.Length / (format.Channels * bytesPerSample);
						int pos1 = 0;
						int pos2 = 0;
						for (int i = 0; i < samples; i++) {
							for (int j = 0; j < format.Channels; j++) {
								for (int k = 0; k < bytesPerSample; k++) {
									bytes[j][pos1 + k] = dataBytes[pos2 + k];
								}
								pos2 += bytesPerSample;
							}
							pos1 += bytesPerSample;
						}
						return new Sound(format.SampleRate, ((format.BitsPerSample + 7) >> 3) << 3, bytes);
					}
				}
			}
		}
		
		/// <summary>Reads a System.UInt32 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt32 read from the reader.</returns>
		private static uint ReadUInt32(BinaryReader reader, Endianness endianness) {
			uint value = reader.ReadUInt32();
			if (endianness == Endianness.Big) {
				unchecked {
					return (value << 24) | (value & ((uint)0xFF00 << 8)) | ((value & (uint)0xFF0000) >> 8) | (value >> 24);
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Reads a System.UInt16 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt16 read from the reader.</returns>
		private static ushort ReadUInt16(BinaryReader reader, Endianness endianness) {
			ushort value = reader.ReadUInt16();
			if (endianness == Endianness.Big) {
				unchecked {
					return (ushort)(((uint)value << 8) | ((uint)value >> 8));
				}
			} else {
				return value;
			}
		}

		
	}
}