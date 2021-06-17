using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace SoundManager
{
	/// <summary>Represents a sound source.</summary>
	public class SoundSource
	{
		// --- members ---
		/// <summary>The sound buffer.</summary>
		public readonly SoundBuffer Buffer;
		/// <summary>The effective sound radius.</summary>
		public double Radius;
		/// <summary>The pitch change factor.</summary>
		public double Pitch;
		/// <summary>The volume change factor.</summary>
		public double Volume;
		/// <summary>The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</summary>
		public Vector3 Position;
		/// <summary>The parent object this sound is attached to, or a null reference.</summary>
		public object Parent;
		/// <summary>Whether this sound plays in a loop.</summary>
		public bool Looped;
		/// <summary>The current state of the sound. Determines if the OpenAL sound name is valid.</summary>
		public SoundSourceState State;
		/// <summary>The OpenAL source name. Only valid if the sound is playing.</summary>
		public int OpenAlSourceName;
		/// <summary>The type of sound</summary>
		public readonly SoundType Type;

		// --- constructors ---
		/// <summary>Creates a new sound source.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="radius">The effective sound radius.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="parent">The parent object this sound source is attached to, or a null reference.</param>
		/// <param name="looped">Whether this sound source plays in a loop.</param>
		internal SoundSource(SoundBuffer buffer, double radius, double pitch, double volume, Vector3 position, object parent, bool looped)
		{
			Buffer = buffer;
			Radius = radius;
			Pitch = pitch;
			Volume = volume;
			Position = position;
			Parent = parent;
			Looped = looped;
			State = SoundSourceState.PlayPending;
			OpenAlSourceName = 0;
			//Set the sound type to undefined to use Michelle's original processing
			if (parent is AbstractCar)
			{
				Type = SoundType.TrainCar;
			}
			else if (parent is WorldObject)
			{
				Type = SoundType.AnimatedObject;
			}
			else
			{
				Type = SoundType.Undefined;
			}
		}

		/// <summary>Creates a new sound source.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="radius">The effective sound radius.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="train">The train this sound source is attached to, or a null reference.</param>
		/// <param name="type">The type of sound</param>
		/// <param name="looped">Whether this sound source plays in a loop.</param>
		internal SoundSource(SoundBuffer buffer, double radius, double pitch, double volume, Vector3 position, AbstractTrain train, SoundType type, bool looped)
		{
			Buffer = buffer;
			Radius = radius;
			Pitch = pitch;
			Volume = volume;
			Position = position;
			Parent = train;
			Looped = looped;
			State = SoundSourceState.PlayPending;
			OpenAlSourceName = 0;
			//Set sound type manually
			Type = type;
		}

		// --- functions ---
		/// <summary>Checks whether this sound is playing or supposed to be playing.</summary>
		/// <returns>Whether the sound is playing or supposed to be playing.</returns>
		public bool IsPlaying()
		{
			if (State == SoundSourceState.PlayPending | State == SoundSourceState.Playing)
			{
				return true;
			}

			return false;
		}

		/// <summary>Stops this sound.</summary>
		public void Stop()
		{
			if (State == SoundSourceState.PlayPending)
			{
				State = SoundSourceState.Stopped;
			}
			else if (State == SoundSourceState.Playing)
			{
				State = SoundSourceState.StopPending;
			}
		}
	}

	/// <summary>Represents a microphone source</summary>
	public class MicSource
	{
		// --- members ---
		/// <summary>The OpenAL source name. Only valid if the sound is playing.</summary>
		public int OpenAlSourceName;
		/// <summary>The position.</summary>
		public readonly Vector3 Position;
		/// <summary>Backward tolerance of position</summary>
		public double BackwardTolerance;
		/// <summary>Forward tolerance of position</summary>
		public double ForwardTolerance;

		// --- constructors ---
		/// <summary>Creates a new microphone source.</summary>
		/// <param name="openAlMic">The OpenAL microphone handle</param>
		/// <param name="micStore">The databuffer to read to</param>
		/// <param name="position">The position.</param>
		/// <param name="backwardTolerance">allowed tolerance in the backward direction</param>
		/// <param name="forwardTolerance">allowed tolerance in the forward direction</param>
		internal MicSource(AudioCapture openAlMic, byte[] micStore, Vector3 position, double backwardTolerance, double forwardTolerance)
		{
			Position = position;
			BackwardTolerance = backwardTolerance;
			ForwardTolerance = forwardTolerance;
			AL.GenSources(1, out OpenAlSourceName);

			// Prepare for monitoring the playback state.
			int dummyBuffer = AL.GenBuffer();
			AL.BufferData(dummyBuffer, openAlMic.SampleFormat, micStore, 0, openAlMic.SampleFrequency);
			AL.SourceQueueBuffer(OpenAlSourceName, dummyBuffer);
			AL.SourcePlay(OpenAlSourceName);

			AL.Source(OpenAlSourceName, ALSourceb.SourceRelative, true);
		}
	}
}
