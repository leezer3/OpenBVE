using System;
using OpenBveApi.Sounds;

namespace OpenBve {
	internal static partial class Sounds {
		
		/// <summary>Mixes all channels into a single channel.</summary>
		/// <param name="sound">The sound.</param>
		/// <returns>The mono mix in the same format as the original.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the bits per sample are not supported.</exception>
		private static byte[] GetMonoMix(Sound sound) {
			/*
			 * Convert integer samples to floating-point samples.
			 */
			float[][] samples;
			if (sound.Bytes.Length == 1 || sound.Bytes[0].Length == 0) {
				return sound.Bytes[0];
			} else if (sound.BitsPerSample == 8) {
				samples = new float[sound.Bytes.Length][];
				for (int i = 0; i < sound.Bytes.Length; i++) {
					samples[i] = new float[sound.Bytes[i].Length];
					for (int j = 0; j < sound.Bytes[i].Length; j++) {
						byte value = sound.Bytes[i][j];
						samples[i][j] = ((float)value - 128.0f) / (value < 128 ? 128.0f : 127.0f);
					}
				}
			} else if (sound.BitsPerSample == 16) {
				samples = new float[sound.Bytes.Length][];
				for (int i = 0; i < sound.Bytes.Length; i++) {
					samples[i] = new float[sound.Bytes[i].Length >> 1];
					for (int j = 0; j < sound.Bytes[i].Length; j += 2) {
						short value = (short)(ushort)((int)sound.Bytes[i][j] | ((int)sound.Bytes[i][j + 1] << 8));
						samples[i][j >> 1] = (float)value / (value < 0 ? 32768.0f : 32767.0f);
					}
				}
			} else {
				throw new NotSupportedException();
			}
			/*
			 * Mix floating-point samples to mono.
			 * */
			float[] mix = GetNormalizedMonoMix(samples);
			/*
			 * Convert floating-point samples to integer samples.
			 */
			byte[] result;
			if (sound.BitsPerSample == 8) {
				result = new byte[mix.Length];
				for (int i = 0; i < mix.Length; i++) {
					result[i] = (byte)((mix[i] < 0.0f ? 128.0f : 127.0f) * mix[i] + 128.0f);
				}
			} else if (sound.BitsPerSample == 16) {
				result = new byte[2 * mix.Length];
				for (int i = 0; i < mix.Length; i++) {
					int value = (int)(ushort)(short)((mix[i] < 0.0f ? 32768.0f : 32767.0f) * mix[i]);
					result[2 * i + 0] = (byte)value;
					result[2 * i + 1] = (byte)(value >> 8);
				}
			} else {
				throw new NotSupportedException();
			}
			return result;
		}
		
		private static float[] GetNormalizedMonoMix(float[][] samples) {
			/*
			 * This mixer tries to find silent channels and discards them.
			 * It then performs a mix to mono for all remaining channels
			 * and tries to detect destructive interference (in which case
			 * the first non-silent channel is returned). In all other cases,
			 * volume in the mono mix is normalized to the average volume
			 * in all non-silent channels. If necessary, the volume is
			 * further normalized to prevent overflow. This also prevents
			 * constructive interference.
			 * */
			// --- determine the volume per channel and the total volume ---
			float[] channelVolume = new float[samples.Length];
			float totalVolume = 0.0f;
			for (int i = 0; i < samples.Length; i++) {
				for (int j = 0; j < samples[i].Length; j++) {
					channelVolume[i] += Math.Abs(samples[i][j]);
				}
				channelVolume[i] /= samples[i].Length;
				totalVolume += channelVolume[i];
			}
			totalVolume /= samples.Length;
			// --- discard all channels that are below
			//     a certain threshold of the total volume ---
			const float silentThreshold = 0.05f;
			float[][] remainingSamples = new float[samples.Length][];
			int remainingSamplesUsed = 0;
			for (int i = 0; i < samples.Length; i++) {
				if (channelVolume[i] > silentThreshold * totalVolume) {
					channelVolume[remainingSamplesUsed] = channelVolume[i];
					remainingSamples[remainingSamplesUsed] = samples[i];
					remainingSamplesUsed++;
				}
			}
			if (remainingSamplesUsed == 1) {
				return remainingSamples[0];
			} else if (remainingSamplesUsed == 0) {
				remainingSamples = samples;
				remainingSamplesUsed = samples.Length;
			} else {
				totalVolume = 0.0f;
				for (int i = 0; i < samples.Length; i++) {
					totalVolume += channelVolume[i];
				}
				totalVolume /= remainingSamplesUsed;
			}
			// --- produce a mono mix from all remaining channels ---
			float[] mix = new float[remainingSamples[0].Length];
			float mixVolume = 0.0f;
			for (int j = 0; j < remainingSamples[0].Length; j++) {
				for (int i = 0; i < remainingSamplesUsed; i++) {
					mix[j] += remainingSamples[i][j];
				}
				mix[j] /= remainingSamplesUsed;
				mixVolume += Math.Abs(mix[j]);
			}
			mixVolume /= remainingSamples[0].Length;
			// --- if the volume in the mono mix is below
			//     a certain threshold of the total volume,
			//     assume destructive interference and return
			//     the first non-silent channel ---
			const float destructiveInterferenceThreshold = 0.05f;
			if (mixVolume < destructiveInterferenceThreshold * totalVolume) {
				return remainingSamples[0];
			}
			// --- normalize the volume in the mono mix so that
			//     it corresponds to the average total volume ---
			float maximum = 0.0f;
			for (int j = 0; j < mix.Length; j++) {
				mix[j] *= totalVolume / mixVolume;
				float value = Math.Abs(mix[j]);
				if (value > maximum) maximum = value;
			}
			// --- if the maximum value now created exceeds the
			//     permissible range, normalize the mono mix further ---
			if (maximum > 1.0f) {
				for (int j = 0; j < mix.Length; j++) {
					mix[j] /= maximum;
				}
			}
			return mix;
		}
		
	}
}