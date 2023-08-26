namespace OpenBveApi.Runtime
{
	/// <summary>Represents the handle to a sound.</summary>
	public class SoundHandle
	{
		/// <summary>Whether the handle to the sound is valid.</summary>
		protected bool MyValid;

		/// <summary>The volume. A value of 1.0 represents nominal volume.</summary>
		protected double MyVolume;

		/// <summary>The pitch. A value of 1.0 represents nominal pitch.</summary>
		protected double MyPitch;

		/// <summary>Gets whether the sound is still playing. Once this returns false, the sound handle is invalid.</summary>
		public bool Playing => MyValid;

		/// <summary>Gets whether the sound has stopped. Once this returns true, the sound handle is invalid.</summary>
		public bool Stopped => !MyValid;

		/// <summary>Gets or sets the volume. A value of 1.0 represents nominal volume.</summary>
		public double Volume
		{
			get => MyVolume;
			set => MyVolume = value;
		}

		/// <summary>Gets or sets the pitch. A value of 1.0 represents nominal pitch.</summary>
		public double Pitch
		{
			get => MyPitch;
			set => MyPitch = value;
		}

		/// <summary>Stops the sound and invalidates the handle.</summary>
		public void Stop()
		{
			MyValid = false;
		}
	}
}
