#pragma warning disable 0659, 0661

using System;

namespace OpenBveApi.Sounds {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	// --- structures ---
	
	/// <summary>Represents a sound.</summary>
	public class Sound {
		// --- members ---
		/// <summary>The number of samples per second.</summary>
		private int MySampleRate;
		/// <summary>The number of bits per sample. Allowed values are 8 or 16.</summary>
		private int MyBitsPerSample;
		/// <summary>The PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</summary>
		private byte[][] MyBytes;
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
				throw new ArgumentNullException("The data bytes are a null reference.");
			}
			if (bytes.Length == 0) {
				throw new ArgumentException("There must be at least one channel.");
			}
			for (int i = 0; i < bytes.Length; i++) {
				if (bytes[i] == null) {
					throw new ArgumentNullException("The data bytes channel " + i.ToString() + " are a null reference.");
				}
			}
			for (int i = 1; i < bytes.Length; i++) {
				if (bytes[i].Length != bytes[0].Length) {
					throw new ArgumentException("The data bytes of the channels are of unequal length.");
				}
			}
			this.MySampleRate = sampleRate;
			this.MyBitsPerSample = bitsPerSample;
			this.MyBytes = bytes;
		}
		// --- properties ---
		/// <summary>Gets the number of samples per second.</summary>
		public int SampleRate {
			get {
				return this.MySampleRate;
			}
		}
		/// <summary>Gets the number of bits per sample. Allowed values are 8 or 16.</summary>
		public int BitsPerSample {
			get {
				return this.MyBitsPerSample;
			}
		}
		/// <summary>Gets the PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</summary>
		public byte[][] Bytes {
			get {
				return this.MyBytes;
			}
		}
		/// <summary>Gets the duration of the sound in seconds.</summary>
		public double Duration {
			get {
				return (double)(8 * this.MyBytes[0].Length / this.MyBitsPerSample) / (double)this.MySampleRate;
			}
		}
		// --- operators ---
		/// <summary>Checks whether two sound are equal.</summary>
		/// <param name="a">The first sound.</param>
		/// <param name="b">The second sound.</param>
		/// <returns>Whether the two sounds are equal.</returns>
		public static bool operator ==(Sound a, Sound b) {
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.MySampleRate != b.MySampleRate) return false;
			if (a.MyBitsPerSample != b.MyBitsPerSample) return false;
			if (a.MyBytes.Length != b.MyBytes.Length) return false;
			for (int i = 0; i < a.MyBytes.Length; i++) {
				if (a.MyBytes[i].Length != b.MyBytes[i].Length) return false;
				for (int j = 0; j < a.MyBytes[i].Length; j++) {
					if (a.MyBytes[i][j] != b.MyBytes[i][j]) return false;
				}
			}
			return true;
		}
		/// <summary>Checks whether two sounds are unequal.</summary>
		/// <param name="a">The first sound.</param>
		/// <param name="b">The second sound.</param>
		/// <returns>Whether the two sounds are unequal.</returns>
		public static bool operator !=(Sound a, Sound b) {
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.MySampleRate != b.MySampleRate) return true;
			if (a.MyBitsPerSample != b.MyBitsPerSample) return true;
			if (a.MyBytes.Length != b.MyBytes.Length) return true;
			for (int i = 0; i < a.MyBytes.Length; i++) {
				if (a.MyBytes[i].Length != b.MyBytes[i].Length) return true;
				for (int j = 0; j < a.MyBytes[i].Length; j++) {
					if (a.MyBytes[i][j] != b.MyBytes[i][j]) return true;
				}
			}
			return false;
		}
		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj) {
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is Sound)) return false;
			Sound x = (Sound)obj;
			if (this.MySampleRate != x.MySampleRate) return false;
			if (this.MyBitsPerSample != x.MyBitsPerSample) return false;
			if (this.MyBytes.Length != x.MyBytes.Length) return false;
			for (int i = 0; i < this.MyBytes.Length; i++) {
				if (this.MyBytes[i].Length != x.MyBytes[i].Length) return false;
				for (int j = 0; j < this.MyBytes[i].Length; j++) {
					if (this.MyBytes[i][j] != x.MyBytes[i][j]) return false;
				}
			}
			return true;
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