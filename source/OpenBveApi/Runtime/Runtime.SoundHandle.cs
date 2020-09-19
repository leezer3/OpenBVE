namespace OpenBveApi.Runtime
{
	/// <summary>Represents the handle to a sound.</summary>
	public class SoundHandle
	{
		// --- members ---
		/// <summary>Whether the handle to the sound is valid.</summary>
		protected bool MyValid;

		/// <summary>The volume. A value of 1.0 represents nominal volume.</summary>
		protected double MyVolume;

		/// <summary>The pitch. A value of 1.0 represents nominal pitch.</summary>
		protected double MyPitch;

		// --- properties ---
		/// <summary>Gets whether the sound is still playing. Once this returns false, the sound handle is invalid.</summary>
		public bool Playing
		{
			get
			{
				return this.MyValid;
			}
		}

		/// <summary>Gets whether the sound has stopped. Once this returns true, the sound handle is invalid.</summary>
		public bool Stopped
		{
			get
			{
				return !this.MyValid;
			}
		}

		/// <summary>Gets or sets the volume. A value of 1.0 represents nominal volume.</summary>
		public double Volume
		{
			get
			{
				return this.MyVolume;
			}
			set
			{
				this.MyVolume = value;
			}
		}

		/// <summary>Gets or sets the pitch. A value of 1.0 represents nominal pitch.</summary>
		public double Pitch
		{
			get
			{
				return this.MyPitch;
			}
			set
			{
				this.MyPitch = value;
			}
		}

		// functions
		/// <summary>Stops the sound and invalidates the handle.</summary>
		public void Stop()
		{
			this.MyValid = false;
		}
	}
}
