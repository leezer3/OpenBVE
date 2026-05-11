#pragma warning disable 0659, 0661

using System;
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Sounds {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	/// <summary>Represents a sound.</summary>
	public class Sound 
	{
		/// <summary>The number of samples per second.</summary>
		public readonly int SampleRate;
		/// <summary>The number of bits per sample. Allowed values are 8 or 16.</summary>
		public readonly int BitsPerSample;
		/// <summary>The PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</summary>
		public readonly byte[][] Bytes;

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="sampleRate">The number of samples per second.</param>
		/// <param name="bitsPerSample">The number of bits per sample. Allowed values are 8 or 16.</param>
		/// <param name="bytes">The PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</param>
		/// <exception cref="System.ArgumentException">Raised when the number of samples per second is not positive.</exception>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per samples is neither 8 nor 16.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the bytes array or any of its subarrays is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the bytes array does not contain any elements.</exception>
		/// <exception cref="System.ArgumentException">Raised when the bytes' subarrays are of unequal length.</exception>
		public Sound(int sampleRate, int bitsPerSample, byte[][] bytes) {
			if (sampleRate <= 0) {
				throw new ArgumentException("The sample rate must be positive.");
			}
			if (bitsPerSample != 8 & bitsPerSample != 16) {
				throw new ArgumentException("The number of bits per sample is neither 8 nor 16.");
			}
			if (bytes == null) {
				throw new ArgumentNullException(nameof(bytes));
			}
			if (bytes.Length == 0) {
				throw new ArgumentException("There must be at least one channel.");
			}
			for (int i = 0; i < bytes.Length; i++) {
				if (bytes[i] == null) {
					throw new ArgumentNullException("The data bytes for channel " + i + " are a null reference.");
				}
			}
			for (int i = 1; i < bytes.Length; i++) {
				if (bytes[i].Length != bytes[0].Length) {
					throw new ArgumentException("The data bytes of the channels are of unequal length.");
				}
			}
			SampleRate = sampleRate;
			BitsPerSample = bitsPerSample;
			Bytes = bytes;
		}
		
		/// <summary>Gets the duration of the sound in seconds.</summary>
		public double Duration => 8.0 * Bytes[0].Length / BitsPerSample / SampleRate;

		// --- operators ---
		/// <summary>Checks whether two sound are equal.</summary>
		/// <param name="a">The first sound.</param>
		/// <param name="b">The second sound.</param>
		/// <returns>Whether the two sounds are equal.</returns>
		public static bool operator ==(Sound a, Sound b) {
			if (ReferenceEquals(a, b)) return true;
			if (a is null) return false;
			if (b is null) return false;
			if (a.SampleRate != b.SampleRate) return false;
			if (a.BitsPerSample != b.BitsPerSample) return false;
			if (a.Bytes.Length != b.Bytes.Length) return false;
			for (int i = 0; i < a.Bytes.Length; i++) {
				if (a.Bytes[i].Length != b.Bytes[i].Length) return false;
				for (int j = 0; j < a.Bytes[i].Length; j++) {
					if (a.Bytes[i][j] != b.Bytes[i][j]) return false;
				}
			}
			return true;
		}
		/// <summary>Checks whether two sounds are unequal.</summary>
		/// <param name="a">The first sound.</param>
		/// <param name="b">The second sound.</param>
		/// <returns>Whether the two sounds are unequal.</returns>
		public static bool operator !=(Sound a, Sound b) {
			if (ReferenceEquals(a, b)) return false;
			if (a is null) return true;
			if (b is null) return true;
			if (a.SampleRate != b.SampleRate) return true;
			if (a.BitsPerSample != b.BitsPerSample) return true;
			if (a.Bytes.Length != b.Bytes.Length) return true;
			for (int i = 0; i < a.Bytes.Length; i++) {
				if (a.Bytes[i].Length != b.Bytes[i].Length) return true;
				for (int j = 0; j < a.Bytes[i].Length; j++) {
					if (a.Bytes[i][j] != b.Bytes[i][j]) return true;
				}
			}
			return false;
		}
		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj) {
			if (ReferenceEquals(this, obj)) return true;
			if (obj is null) return false;
			if (!(obj is Sound)) return false;
			Sound x = (Sound)obj;
			if (SampleRate != x.SampleRate) return false;
			if (BitsPerSample != x.BitsPerSample) return false;
			if (Bytes.Length != x.Bytes.Length) return false;
			for (int i = 0; i < Bytes.Length; i++) {
				if (Bytes[i].Length != x.Bytes[i].Length) return false;
				for (int j = 0; j < Bytes[i].Length; j++) {
					if (Bytes[i][j] != x.Bytes[i][j]) return false;
				}
			}
			return true;
		}

		/// <summary>Mixes all channels into a single channel.</summary>
		/// <returns>The mono mix in the same format as the original.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the bits per sample are not supported.</exception>
		public byte[] GetMonoMix()
		{
			unchecked
			{
				/*
			 * Convert integer samples to floating-point samples.
			 */
				float[][] samples;
				if (Bytes.Length == 1 || Bytes[0].Length == 0)
				{
					return Bytes[0];
				}
				switch (BitsPerSample)
				{
					case 8:
						samples = new float[Bytes.Length][];
						for (int i = 0; i < Bytes.Length; i++)
						{
							samples[i] = new float[Bytes[i].Length];
							for (int j = 0; j < Bytes[i].Length; j++)
							{
								byte value = Bytes[i][j];
								samples[i][j] = (value - 128.0f) / (value < 128 ? 128.0f : 127.0f);
							}
						}
						break;
					case 16:
						samples = new float[Bytes.Length][];
						for (int i = 0; i < Bytes.Length; i++)
						{
							samples[i] = new float[Bytes[i].Length >> 1];
							for (int j = 0; j + 1 < Bytes[i].Length; j += 2)
							{
								short value = (short)(ushort)(Bytes[i][j] | (Bytes[i][j + 1] << 8));
								samples[i][j >> 1] = value / (value < 0 ? 32768.0f : 32767.0f);
							}
						}
						break;
					default:
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
				switch (BitsPerSample)
				{
					case 8:
						result = new byte[mix.Length];
						for (int i = 0; i < mix.Length; i++)
						{
							result[i] = (byte)((mix[i] < 0.0f ? 128.0f : 127.0f) * mix[i] + 128.0f);
						}
						break;
					case 16:
						result = new byte[2 * mix.Length];
						for (int i = 0; i < mix.Length; i++)
						{
							int value = (ushort)(short)((mix[i] < 0.0f ? 32768.0f : 32767.0f) * mix[i]);
							result[2 * i + 0] = (byte)value;
							result[2 * i + 1] = (byte)(value >> 8);
						}
						break;
					default:
						throw new NotSupportedException();
				}
				return result;
			}
		}

		private float[] GetNormalizedMonoMix(float[][] samples)
		{
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
			for (int i = 0; i < samples.Length; i++)
			{
				for (int j = 0; j < samples[i].Length; j++)
				{
					channelVolume[i] += System.Math.Abs(samples[i][j]);
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
			for (int i = 0; i < samples.Length; i++)
			{
				if (!(channelVolume[i] > silentThreshold * totalVolume))
				{
					continue;
				}

				channelVolume[remainingSamplesUsed] = channelVolume[i];
				remainingSamples[remainingSamplesUsed] = samples[i];
				remainingSamplesUsed++;
			}
			switch (remainingSamplesUsed)
			{
				case 1:
					return remainingSamples[0];
				case 0:
					remainingSamples = samples;
					remainingSamplesUsed = samples.Length;
					break;
				default:
					totalVolume = 0.0f;
					for (int i = 0; i < samples.Length; i++)
					{
						totalVolume += channelVolume[i];
					}
					totalVolume /= remainingSamplesUsed;
					break;
			}
			// --- produce a mono mix from all remaining channels ---
			float[] mix = new float[remainingSamples[0].Length];
			float mixVolume = 0.0f;
			for (int j = 0; j < remainingSamples[0].Length; j++)
			{
				for (int i = 0; i < remainingSamplesUsed; i++)
				{
					mix[j] += remainingSamples[i][j];
				}
				mix[j] /= remainingSamplesUsed;
				mixVolume += System.Math.Abs(mix[j]);
			}
			mixVolume /= remainingSamples[0].Length;
			// --- if the volume in the mono mix is below
			//     a certain threshold of the total volume,
			//     assume destructive interference and return
			//     the first non-silent channel ---
			const float destructiveInterferenceThreshold = 0.05f;
			if (mixVolume < destructiveInterferenceThreshold * totalVolume)
			{
				return remainingSamples[0];
			}
			// --- normalize the volume in the mono mix so that
			//     it corresponds to the average total volume ---
			float maximum = 0.0f;
			for (int j = 0; j < mix.Length; j++)
			{
				mix[j] *= totalVolume / mixVolume;
				float value = System.Math.Abs(mix[j]);
				if (value > maximum)
				{
					maximum = value;
				}
			}
			// --- if the maximum value now created exceeds the
			//     permissible range, normalize the mono mix further ---
			if (maximum > 1.0f)
			{
				for (int j = 0; j < mix.Length; j++)
				{
					mix[j] /= maximum;
				}
			}
			return mix;
		}
	}
	
	
	// --- handles ---
	
	/// <summary>Represents a handle to a sound.</summary>
	public abstract class SoundHandle { }
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading sounds. Plugins must implement this interface if they wish to expose sounds.</summary>
	public abstract class SoundInterface {
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public virtual void Load(Hosts.HostInterface host) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public abstract bool CanLoadSound(string path);
		
		/// <summary>Loads the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public abstract bool LoadSound(string path, out Sound sound);
		
	}
	
}
