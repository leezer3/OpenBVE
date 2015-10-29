#define CHECKSUMS // whether to check for CRC-8, CRC-16 and MD5

using System;
using System.IO;
using System.Security.Cryptography;

namespace Flac {
	internal static class Decoder {
		
		/// <summary>Decodes the specified native FLAC file and extracts the raw samples.</summary>
		/// <param name="file">The path to the native FLAC file.</param>
		/// <param name="sampleRate">Receives the sample rate in Hz.</param>
		/// <param name="bitsPerSample">Receives the number of bits per sample.</param>
		/// <returns>The raw samples per channel, padded into the most-significant bits (makes all samples 32-bits wide and signed).</returns>
		internal static int[][] Decode(string file, out int sampleRate, out int bitsPerSample) {
			unchecked {
				byte[] bytes = File.ReadAllBytes(file);
				BitReader reader = new BitReader(bytes);
				// --- identifier ---
				if (reader.ReadUInt32BE() != 0x664C6143) {
					throw new InvalidDataException();
				}
				sampleRate = 0;
				bitsPerSample = 0;
				int numberOfChannels = 0;
				int totalNumberOfSamples = 0;
				bool streaminfoPresent = false;
				byte[] md5 = null;
				// --- metadata blocks ---
				while (true) {
					// --- block header ---
					bool lastBlock = reader.ReadBit() == 1;
					int blockType = (int)reader.ReadBits(7);
					int blockLength = (int)reader.ReadUInt24BE();
					if (blockType == 0) {
						// --- STREAMINFO ---
						if (streaminfoPresent) {
							throw new InvalidDataException();
						}
						if (blockLength != 34) {
							throw new InvalidDataException();
						}
						int minimumBlockSize = (int)reader.ReadUInt16BE();
						if (minimumBlockSize < 16) {
							throw new InvalidDataException();
						}
						int maximumBlockSize = (int)reader.ReadUInt16BE();
						if (minimumBlockSize > maximumBlockSize | maximumBlockSize > 65535) {
							throw new InvalidDataException();
						}
						int minimumFrameSize = (int)reader.ReadUInt24BE();
						int maximumFrameSize = (int)reader.ReadUInt24BE();
						if (minimumFrameSize > maximumFrameSize & maximumFrameSize != 0) {
							throw new InvalidDataException();
						}
						sampleRate = (int)reader.ReadBits(20);
						if (sampleRate == 0 | sampleRate > 655350) {
							throw new InvalidDataException();
						}
						numberOfChannels = (int)reader.ReadBits(3) + 1;
						bitsPerSample = (int)reader.ReadBits(5) + 1;
						if (bitsPerSample < 4) {
							throw new InvalidDataException();
						}
						/* total number of samples: 36 bits
						 * only the lower 31 bits are supported */
						if (reader.ReadBits(4) != 0) {
							throw new NotSupportedException("Too many samples for this decoder.");
						}
						totalNumberOfSamples = (int)reader.ReadUInt32BE();
						if (totalNumberOfSamples < 0) {
							throw new NotSupportedException("Too many samples for this decoder.");
						}
						md5 = reader.ReadBytes(16);
						streaminfoPresent = true;
					} else if (blockType >= 1 & blockType <= 6) {
						// --- ignored ---
						reader.BytePosition += (int)blockLength;
					} else {
						// --- invalid ---
						throw new InvalidDataException();
					}
					if (lastBlock) {
						break;
					}
				}
				// --- prepare samples per channel ---
				if (!streaminfoPresent) {
					throw new InvalidDataException();
				}
				int[][] samples = new int[numberOfChannels][];
				int sampleCount = totalNumberOfSamples != 0 ? (int)totalNumberOfSamples : 65536;
				for (int i = 0; i < numberOfChannels; i++) {
					samples[i] = new int[sampleCount];
				}
				int samplesUsed = 0;
				// --- frames ---
				while (!reader.EndOfStream()) {
					// --- frame header ---
					int frameHeaderPosition = reader.BytePosition;
					uint syncCode = reader.ReadBits(14);
					if (syncCode != 0x3FFE) {
						throw new InvalidDataException();
					}
					uint reserved1 = reader.ReadBit();
					if (reserved1 != 0) {
						throw new InvalidDataException();
					}
					uint blockingStrategy = reader.ReadBit();
					int blockNumberOfSamples = (int)reader.ReadBits(4);
					if (blockNumberOfSamples == 0) {
						throw new InvalidDataException();
					} else if (blockNumberOfSamples == 1) {
						blockNumberOfSamples = 192;
					} else if (blockNumberOfSamples >= 2 & blockNumberOfSamples <= 5) {
						blockNumberOfSamples = 576 << (int)(blockNumberOfSamples - 2);
					} else if (blockNumberOfSamples >= 8 & blockNumberOfSamples <= 15) {
						blockNumberOfSamples = 256 << (int)(blockNumberOfSamples - 8);
					}
					int blockSampleRate = (int)reader.ReadBits(4);
					if (blockSampleRate == 0) {
						blockSampleRate = sampleRate;
					} else if (blockSampleRate == 1) {
						blockSampleRate = 88200;
					} else if (blockSampleRate == 2) {
						blockSampleRate = 176400;
					} else if (blockSampleRate == 3) {
						blockSampleRate = 192000;
					} else if (blockSampleRate == 4) {
						blockSampleRate = 8000;
					} else if (blockSampleRate == 5) {
						blockSampleRate = 16000;
					} else if (blockSampleRate == 6) {
						blockSampleRate = 22050;
					} else if (blockSampleRate == 7) {
						blockSampleRate = 24000;
					} else if (blockSampleRate == 8) {
						blockSampleRate = 32000;
					} else if (blockSampleRate == 9) {
						blockSampleRate = 44100;
					} else if (blockSampleRate == 10) {
						blockSampleRate = 48000;
					} else if (blockSampleRate == 11) {
						blockSampleRate = 96000;
					} else if (blockSampleRate == 15) {
						throw new InvalidDataException();
					}
					uint channelAssignment = reader.ReadBits(4);
					int blockBitsPerSample = (int)reader.ReadBits(3);
					if (blockBitsPerSample == 0) {
						blockBitsPerSample = bitsPerSample;
					} else if (blockBitsPerSample == 1) {
						blockBitsPerSample = 8;
					} else if (blockBitsPerSample == 2) {
						blockBitsPerSample = 12;
					} else if (blockBitsPerSample == 4) {
						blockBitsPerSample = 16;
					} else if (blockBitsPerSample == 5) {
						blockBitsPerSample = 20;
					} else if (blockBitsPerSample == 6) {
						blockBitsPerSample = 24;
					} else {
						throw new InvalidDataException();
					}
					uint reserved2 = reader.ReadBit();
					if (reserved2 != 0) {
						throw new InvalidDataException();
					}
					ulong sampleOrFrameNumber = (ulong)reader.ReadUTF8EncodedInteger();
					if (blockNumberOfSamples == 6) {
						blockNumberOfSamples = (int)reader.ReadByte() + 1;
					} else if (blockNumberOfSamples == 7) {
						blockNumberOfSamples = (int)reader.ReadUInt16BE() + 1;
					}
					if (blockSampleRate == 12) {
						blockSampleRate = (int)reader.ReadByte();
					} else if (blockSampleRate == 13) {
						blockSampleRate = (int)reader.ReadUInt16BE();
					} else if (blockSampleRate == 14) {
						blockSampleRate = 10 * (int)reader.ReadUInt16BE();
					}
					uint crc8 = reader.ReadByte();
					#if CHECKSUMS
					if (crc8 != Crc8.ComputeHash(reader.Bytes, frameHeaderPosition, reader.BytePosition - frameHeaderPosition - 1)) {
						throw new InvalidDataException("CRC-8 failed.");
					}
					#endif
					// --- subframes ---
					while (samplesUsed + blockNumberOfSamples > sampleCount) {
						sampleCount <<= 1;
						for (int i = 0; i < numberOfChannels; i++) {
							Array.Resize<int>(ref samples[i], (int)sampleCount);
						}
					}
					for (int i = 0; i < numberOfChannels; i++) {
						uint zero = reader.ReadBit();
						if (zero != 0) {
							throw new InvalidDataException();
						}
						uint subframeType = reader.ReadBits(6);
						int wastedBitsPerSample = (int)reader.ReadUnaryEncodedInteger();
						if (wastedBitsPerSample > bitsPerSample) {
							throw new InvalidDataException();
						}
						int subframeBitsPerSample = bitsPerSample - wastedBitsPerSample;
						if (channelAssignment == 8 & i == 1) {
							subframeBitsPerSample++;
						} else if (channelAssignment == 9 & i == 0) {
							subframeBitsPerSample++;
						} else if (channelAssignment == 10 & i == 1) {
							subframeBitsPerSample++;
						}
						if (subframeType == 0) {
							// --- SUBFRAME_CONSTANT ---
							int value = FromTwosComplement(reader.ReadBits((int)subframeBitsPerSample), (uint)1 << subframeBitsPerSample) << (int)(32 - subframeBitsPerSample);
							int numberOfSamples = blockNumberOfSamples * bitsPerSample / subframeBitsPerSample;
							for (int j = 0; j < numberOfSamples; j++) {
								samples[i][samplesUsed + j] = value;
							}
						} else if (subframeType == 1) {
							// --- SUBFRAME_VERBATIM ---
							if (blockSampleRate == sampleRate) {
								for (int j = 0; j < blockNumberOfSamples; j++) {
									int value = FromTwosComplement(reader.ReadBits((int)subframeBitsPerSample), (uint)1 << subframeBitsPerSample) << (int)(32 - subframeBitsPerSample);
									samples[i][samplesUsed + j] = value;
								}
							} else {
								throw new NotSupportedException("Variable sample rates are not supported by this decoder.");
							}
						} else if ((subframeType & 0x38) == 8) {
							// --- SUBFRAME_FIXED ---
							int predictorOrder = (int)subframeType & 7;
							if (predictorOrder > 4) {
								throw new InvalidDataException();
							}
							int[] blockSamples = new int[blockNumberOfSamples];
							for (int j = 0; j < predictorOrder; j++) {
								blockSamples[j] = FromTwosComplement(reader.ReadBits(subframeBitsPerSample), (uint)1 << subframeBitsPerSample);
							}
							int[] residuals = ReadResiduals(reader, predictorOrder, blockNumberOfSamples);
							int mask = (1 << subframeBitsPerSample) - 1;
							if (predictorOrder == 0) {
								for (int j = 0; j < blockNumberOfSamples; j++) {
									blockSamples[j] = residuals[j] & mask;
								}
							} else if (predictorOrder == 1) {
								for (int j = 1; j < blockNumberOfSamples; j++) {
									int predictor = blockSamples[j - 1];
									blockSamples[j] = (residuals[j] + predictor) & mask;
								}
							} else if (predictorOrder == 2) {
								for (int j = 2; j < blockNumberOfSamples; j++) {
									int predictor = 2 * blockSamples[j - 1] - blockSamples[j - 2];
									blockSamples[j] = (residuals[j] + predictor) & mask;
								}
							} else if (predictorOrder == 3) {
								for (int j = 3; j < blockNumberOfSamples; j++) {
									int predictor = 3 * blockSamples[j - 1] - 3 * blockSamples[j - 2] + blockSamples[j - 3];
									blockSamples[j] = (residuals[j] + predictor) & mask;
								}
							} else if (predictorOrder == 4) {
								for (int j = 4; j < blockNumberOfSamples; j++) {
									int predictor = 4 * blockSamples[j - 1] - 6 * blockSamples[j - 2] + 4 * blockSamples[j - 3] - blockSamples[j - 4];
									blockSamples[j] = (residuals[j] + predictor) & mask;
								}
							} else {
								throw new InvalidOperationException();
							}
							if (blockSampleRate == sampleRate) {
								for (int j = 0; j < blockNumberOfSamples; j++) {
									samples[i][samplesUsed + j] = blockSamples[j] << (int)(32 - subframeBitsPerSample);
								}
							} else {
								throw new NotSupportedException("Variable sample rates are not supported by this decoder.");
							}
						} else if ((subframeType & 0x20) == 0x20) {
							// --- SUBFRAME_LPC ---
							int predictorOrder = ((int)subframeType & 0x1F) + 1;
							int[] blockSamples = new int[blockNumberOfSamples];
							for (int j = 0; j < predictorOrder; j++) {
								blockSamples[j] = FromTwosComplement(reader.ReadBits((int)subframeBitsPerSample), (uint)1 << subframeBitsPerSample);
								if (blockSamples[j] != 0) {
									int asdgfefe = blockSamples[j];
								}
							}
							int coefficientPrecision = (int)reader.ReadBits(4) + 1;
							if (coefficientPrecision == 16) {
								throw new InvalidDataException();
							}
							int coefficientRange = 1 << coefficientPrecision;
							int predictorShift = FromTwosComplement(reader.ReadBits(5), 32);
							if (predictorShift < 0) {
								throw new NotSupportedException("A negative predictor shift is not supported by this decoder.");
							}
							int[] coefficients = new int[predictorOrder];
							for (int j = 0; j < predictorOrder; j++) {
								coefficients[j] = FromTwosComplement(reader.ReadBits((int)coefficientPrecision), (uint)coefficientRange);
							}
							int[] residuals = ReadResiduals(reader, predictorOrder, blockNumberOfSamples);
							if (subframeBitsPerSample == 8) {
								if (coefficientPrecision <= 18) {
									for (int j = predictorOrder; j < blockNumberOfSamples; j++) {
										int predictor = 0;
										for (int k = 0; k < predictorOrder; k++) {
											predictor += coefficients[k] * blockSamples[j - k - 1];
										}
										predictor >>= predictorShift;
										blockSamples[j] = (int)(sbyte)(predictor + residuals[j]);
									}
								} else {
									for (int j = predictorOrder; j < blockNumberOfSamples; j++) {
										long predictor = 0;
										for (int k = 0; k < predictorOrder; k++) {
											predictor += (long)coefficients[k] * (long)blockSamples[j - k - 1];
										}
										predictor >>= predictorShift;
										blockSamples[j] = (int)(sbyte)(predictor + residuals[j]);
									}
								}
							} else if (subframeBitsPerSample == 16) {
								if (coefficientPrecision <= 11) {
									for (int j = predictorOrder; j < blockNumberOfSamples; j++) {
										int predictor = 0;
										for (int k = 0; k < predictorOrder; k++) {
											predictor += coefficients[k] * blockSamples[j - k - 1];
										}
										predictor >>= predictorShift;
										blockSamples[j] = (int)(short)(predictor + residuals[j]);
									}
								} else {
									for (int j = predictorOrder; j < blockNumberOfSamples; j++) {
										long predictor = 0;
										for (int k = 0; k < predictorOrder; k++) {
											predictor += (long)coefficients[k] * (long)blockSamples[j - k - 1];
										}
										predictor >>= predictorShift;
										blockSamples[j] = (int)(short)(predictor + residuals[j]);
									}
								}
							} else {
								int range = 1 << subframeBitsPerSample;
								int mask = range - 1;
								for (int j = predictorOrder; j < blockNumberOfSamples; j++) {
									long predictor = 0;
									for (int k = 0; k < predictorOrder; k++) {
										predictor += (long)coefficients[k] * (long)blockSamples[j - k - 1];
									}
									predictor >>= predictorShift;
									blockSamples[j] = (int)(predictor + residuals[j]) & mask;
									if (blockSamples[j] >= range / 2) {
										blockSamples[j] -= range;
									}
								}
							}
							if (blockSampleRate == sampleRate) {
								for (int j = 0; j < blockNumberOfSamples; j++) {
									samples[i][samplesUsed + j] = blockSamples[j] << (int)(32 - subframeBitsPerSample);
								}
							} else {
								throw new NotSupportedException("Variable sample rates are not supported by this decoder.");
							}
						} else {
							// --- not supported ---
							throw new InvalidDataException();
						}
					}
					// --- inter-channel decorreleation ---
					if (channelAssignment == 8) {
						// --- left + difference ---
						for (int i = samplesUsed; i < samplesUsed + blockNumberOfSamples; i++) {
							samples[1][i] = ((samples[0][i] >> 1) - samples[1][i]) << 1;
						}
					} else if (channelAssignment == 9) {
						// --- difference + right ---
						for (int i = samplesUsed; i < samplesUsed + blockNumberOfSamples; i++) {
							samples[0][i] = ((samples[1][i] >> 1) + samples[0][i]) << 1;
						}
					} else if (channelAssignment == 10) {
						// --- average + difference ---
						int mask = 1 << (31 - blockBitsPerSample);
						for (int i = samplesUsed; i < samplesUsed + blockNumberOfSamples; i++) {
							int mid = samples[0][i];
							int side = samples[1][i];
							samples[0][i] = ((mid | (side & mask)) + side);
							samples[1][i] = ((mid | (side & mask)) - side);
						}
					}
					samplesUsed += blockNumberOfSamples;
					// --- padding ---
					reader.Align();
					// --- footer ---
					uint crc16 = reader.ReadUInt16BE();
					#if CHECKSUMS
					if (crc16 != Crc16.ComputeHash(reader.Bytes, frameHeaderPosition, reader.BytePosition - frameHeaderPosition - 2)) {
						throw new InvalidDataException("CRC-16 failed.");
					}
					#endif
				}
				// --- check md5 ---
				#if CHECKSUMS
				bool md5Set = false;
				for (int i = 0; i < md5.Length; i++) {
					if (md5[i] != 0) {
						md5Set = true;
						break;
					}
				}
				if (md5Set) {
					if (bitsPerSample == 8) {
						/* For 8 bits per sample */
						bytes = new byte[numberOfChannels * samplesUsed];
						int pos = 0;
						for (int i = 0; i < samplesUsed; i++) {
							for (int j = 0; j < numberOfChannels; j++) {
								bytes[pos] = (byte)(samples[j][i] >> 24);
								pos++;
							}
						}
						MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
						byte[] check = provider.ComputeHash(bytes);
						if (check.Length != md5.Length) {
							throw new InvalidOperationException();
						}
						for (int i = 0; i < check.Length; i++) {
							if (check[i] != md5[i]) {
								throw new InvalidDataException("MD5 failed.");
							}
						}
					} else if (bitsPerSample == 16) {
						/* For 16 bits per sample */
						bytes = new byte[2 * numberOfChannels * samplesUsed];
						int pos = 0;
						for (int i = 0; i < samplesUsed; i++) {
							for (int j = 0; j < numberOfChannels; j++) {
								bytes[pos + 0] = (byte)(samples[j][i] >> 16);
								bytes[pos + 1] = (byte)(samples[j][i] >> 24);
								pos += 2;
							}
						}
						MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
						byte[] check = provider.ComputeHash(bytes);
						if (check.Length != md5.Length) {
							throw new InvalidOperationException();
						}
						for (int i = 0; i < check.Length; i++) {
							if (check[i] != md5[i]) {
								throw new InvalidDataException("MD5 failed.");
							}
						}
					} else if (bitsPerSample == 24) {
						/* For 24 bits per sample */
						bytes = new byte[3 * numberOfChannels * samplesUsed];
						int pos = 0;
						for (int i = 0; i < samplesUsed; i++) {
							for (int j = 0; j < numberOfChannels; j++) {
								bytes[pos + 0] = (byte)(samples[j][i] >> 8);
								bytes[pos + 1] = (byte)(samples[j][i] >> 16);
								bytes[pos + 2] = (byte)(samples[j][i] >> 24);
								pos += 3;
							}
						}
						MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
						byte[] check = provider.ComputeHash(bytes);
						if (check.Length != md5.Length) {
							throw new InvalidOperationException();
						}
						for (int i = 0; i < check.Length; i++) {
							if (check[i] != md5[i]) {
								throw new InvalidDataException("MD5 failed.");
							}
						}
					} else {
						/* Let's just skip the MD5 check in other cases
						 * or feel free to implement the check here. */
					}
				}
				#endif
				// --- end of file ---
				if (sampleCount != samplesUsed) {
					for (int i = 0; i < numberOfChannels; i++) {
						Array.Resize<int>(ref samples[i], samplesUsed);
					}
				}
				return samples;
			}
		}
		
		/// <summary>Reads residuals and stores them in the specified array.</summary>
		/// <param name="reader">The bit reader.</param>
		/// <param name="predictorOrder">The predictor order.</param>
		/// <param name="blockSize">The block size.</param>
		/// <returns>The signed residuals.</returns>
		private static int[] ReadResiduals(BitReader reader, int predictorOrder, int blockSize) {
			int[] residuals = new int[blockSize];
			uint method = reader.ReadBits(2);
			if (method == 0 | method == 1) {
				// --- RESIDUAL_CODING_METHOD_PARTITIONED_RICE /
				//     RESIDUAL_CODING_METHOD_PARTITIONED_RICE2 ---
				uint partitionOrder = reader.ReadBits(4);
				int numberOfPartitions = 1 << (int)partitionOrder;
				int numberOfBits = 4 + (int)method;
				uint escape = method == 0 ? (uint)15 : (uint)31;
				int offset = predictorOrder;
				for (int i = 0; i < numberOfPartitions; i++) {
					int riceParameter = (int)reader.ReadBits(numberOfBits);
					int numberOfSamples;
					if (partitionOrder == 0) {
						numberOfSamples = blockSize - predictorOrder;
					} else if (i == 0) {
						numberOfSamples = (blockSize >> (int)partitionOrder) - predictorOrder;
					} else {
						numberOfSamples = blockSize >> (int)partitionOrder;
					}
					if (riceParameter == escape) {
						int bitsPerSample = (int)reader.ReadBits(5);
						for (int j = 0; j < numberOfSamples; j++) {
							residuals[offset + j] = (int)reader.ReadBits(bitsPerSample);
						}
						offset += numberOfSamples;
					} else {
						for (int j = 0; j < numberOfSamples; j++) {
							int value = reader.ReadRiceEncodedInteger(riceParameter);
							residuals[offset + j] = value;
						}
						offset += numberOfSamples;
					}
				}
				return residuals;
			} else {
				// --- not supported ---
				throw new InvalidDataException();
			}
		}
		
		/// <summary>Gets a signed integer from an unsigned integer assuming the unsigned integer is stored in two's complement notation.</summary>
		/// <param name="value">The unsigned integer.</param>
		/// <param name="range">The value range, e.g. 16 for 4 bits, 256 for 8 bits, 65536 for 16 bits, etc.</param>
		/// <returns>The signed integer.</returns>
		private static int FromTwosComplement(uint value, uint range) {
			if (value < (range >> 1)) {
				return (int)value;
			} else {
				return (int)value - (int)range;
			}
		}
		
	}
}