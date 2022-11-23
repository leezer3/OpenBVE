using System;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;

namespace SoundManager
{
	/// <summary>Defines a sound attached to a train car</summary>
	public class CarSound
	{
		/// <summary>The sound buffer to play</summary>
		public readonly SoundBuffer Buffer;
		/// <summary>The source of the sound within the car</summary>
		public SoundSource Source;
		/// <summary>A Vector3 describing the position of the sound source within the base car</summary>
		private readonly Vector3 Position;
		/// <summary>The target volume of the sound</summary>
		/// <remarks>Used when crossfading between multiple sounds of the same type</remarks>
		public double TargetVolume;

		public CarSound(HostInterface currentHost, string trainFolder, string soundFile, double radius, Vector3 position) : this(currentHost, trainFolder, string.Empty, -1, soundFile, radius, position)
		{
		}

		
		public CarSound(HostInterface currentHost, string trainFolder, string configurationFile, int currentLine, string soundFile, double radius, Vector3 position)
		{
			if (soundFile.Length == 0 || Path.ContainsInvalidChars(soundFile))
			{
				currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (currentLine + 1) + " in file " + configurationFile);
				return;
			}

			string absolutePathTosoundFile = Path.CombineFile(trainFolder, soundFile);

			if (!System.IO.File.Exists(absolutePathTosoundFile))
			{
				if (configurationFile != string.Empty)
				{
					//Only add missing file message for BVE4 / XML sound configs, not default
					currentHost.AddMessage(MessageType.Error, false, "The SoundFile " + soundFile + " was not found at line " + (currentLine + 1) + " in file " + configurationFile);
				}
				return;
			}
			SoundHandle handle;
			currentHost.RegisterSound(absolutePathTosoundFile, radius, out handle);
			Buffer = handle as SoundBuffer;
			this.Position = position;
		}

		public CarSound(HostInterface currentHost, string soundFile, double radius, Vector3 position)
		{
			SoundHandle handle;
			currentHost.RegisterSound(soundFile, radius, out handle);
			Buffer = handle as SoundBuffer;
			this.Position = position;
		}

		/// <summary>Creates a new car sound</summary>
		/// <param name="handle">The API handle to the sound buffer</param>
		/// <param name="Position">The position that the sound is emitted from within the car</param>
		/// <returns>The new car sound</returns>
		public CarSound(SoundHandle handle, Vector3 Position)
		{
			this.Buffer = handle as SoundBuffer;
			this.Position = Position;
			this.Source = null;
		}

		/// <summary>Creates a new car sound</summary>
		/// <param name="buffer">The sound buffer</param>
		/// <param name="Position">The position that the sound is emitted from within the car</param>
		/// <returns>The new car sound</returns>
		public CarSound(SoundBuffer buffer, Vector3 Position)
		{
			this.Position = Position;
			this.Source = null;
			this.Buffer = buffer;
		}

		/// <summary>Creates a new empty car sound</summary>
		public CarSound()
		{
			this.Position = Vector3.Zero;
			this.Source = null;
			this.Buffer = null;
		}

		/// <summary>Plays the sound at the original pitch and volume</summary>
		/// <param name="Car">The parent car</param>
		/// <param name="looped">Whether the sound is to be played looped</param>
		public void Play(AbstractCar Car, bool looped)
		{
			Play(1.0, 1.0, Car, looped);
		}

		/// <summary>Plays the sound at the specified pitch and volume</summary>
		/// <param name="pitch">The pitch</param>
		/// <param name="volume">The volume</param>
		/// <param name="Car">The parent car</param>
		/// <param name="looped">Whether the sound is to be played looped</param>
		public void Play(double pitch, double volume, AbstractCar Car, bool looped)
		{
			if (looped && IsPlaying)
			{
				// If looped and already playing, update the pitch / volume values
				Source.Volume = volume;
				Source.Pitch = pitch;
				return;
			}
			if (Buffer != null)
			{
				if (SoundsBase.Sources.Length == SoundsBase.SourceCount)
				{
					Array.Resize(ref SoundsBase.Sources, SoundsBase.Sources.Length << 1);
				}
				SoundsBase.Sources[SoundsBase.SourceCount] = new SoundSource(Buffer, Buffer.Radius, pitch, volume, Position, Car, looped);
				this.Source = SoundsBase.Sources[SoundsBase.SourceCount];
				SoundsBase.SourceCount++;
			}
			
		}

		/// <summary>Unconditionally stops the playing sound</summary>
		public void Stop()
		{
			if (Source == null)
			{
				return;
			}
			Source.Stop();
		}

		/// <summary>Whether the sound is currently playing</summary>
		public bool IsPlaying
		{
			get
			{
				if (Source != null)
				{
					return Source.IsPlaying();
				}
				return false;
			}
		}
	}
}
