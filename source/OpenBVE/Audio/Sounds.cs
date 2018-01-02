using System;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace OpenBve
{
	internal static partial class Sounds
	{
		// --- members ---

		/// <summary>The current OpenAL device.</summary>
		private static IntPtr OpenAlDevice = IntPtr.Zero;

		/// <summary>The current OpenAL context.</summary>
		private static ContextHandle OpenAlContext;

		/// <summary>A list of all sound buffers.</summary>
		private static SoundBuffer[] Buffers = new SoundBuffer[16];

		/// <summary>The number of sound buffers.</summary>
		private static int BufferCount = 0;

		/// <summary>A list of all sound sources.</summary>
		private static SoundSource[] Sources = new SoundSource[16];

		/// <summary>The number of sound sources.</summary>
		private static int SourceCount = 0;

		/// <summary>The gain threshold. Sounds with gains below this value are not played.</summary>
		internal const double GainThreshold = 0.0001;

		/// <summary>Whether all sounds are mute.</summary>
		internal static bool GlobalMute = false;


		// --- linear distance clamp model ---

		/// <summary>The factor by which the inner radius is multiplied to give the outer radius.</summary>
		internal static double OuterRadiusFactor;
		internal static double OuterRadiusFactorSpeed;
		private static double OuterRadiusFactorMaximumSpeed;
		internal static double OuterRadiusFactorMinimum;
		internal static double OuterRadiusFactorMaximum;


		// --- inverse distance clamp model ---

		internal static double LogClampFactor = -15.0;
		internal const double MinLogClampFactor = -20.0;
		internal const double MaxLogClampFactor = -1.0;


		// --- initialization and deinitialization ---

		/// <summary>Initializes audio. A call to Deinitialize must be made when terminating the program.</summary>
		/// <returns>Whether initializing audio was successful.</returns>
		internal static void Initialize()
		{
			Deinitialize();
			switch (Interface.CurrentOptions.SoundRange)
			{
				case Interface.SoundRange.Low:
					OuterRadiusFactorMinimum = 2.0;
					OuterRadiusFactorMaximum = 8.0;
					OuterRadiusFactorMaximumSpeed = 1.0;
					break;
				case Interface.SoundRange.Medium:
					OuterRadiusFactorMinimum = 4.0;
					OuterRadiusFactorMaximum = 16.0;
					OuterRadiusFactorMaximumSpeed = 2.0;
					break;
				case Interface.SoundRange.High:
					OuterRadiusFactorMinimum = 6.0;
					OuterRadiusFactorMaximum = 24.0;
					OuterRadiusFactorMaximumSpeed = 3.0;
					break;
			}
			OuterRadiusFactor = Math.Sqrt(OuterRadiusFactorMinimum * OuterRadiusFactorMaximum);
			OuterRadiusFactorSpeed = 0.0;
			OpenAlDevice = Alc.OpenDevice(null);
			if (OpenAlDevice != IntPtr.Zero)
			{
				OpenAlContext = Alc.CreateContext(OpenAlDevice, (int[])null);
				if (OpenAlContext != ContextHandle.Zero)
				{
					Alc.MakeContextCurrent(OpenAlContext);
					try
					{
						AL.SpeedOfSound(343.0f);
					}
					catch
					{
						MessageBox.Show(Interface.GetInterfaceString("errors_sound_openal_version"), Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					AL.DistanceModel(ALDistanceModel.None);
					return;
				}
				Alc.CloseDevice(OpenAlDevice);
				OpenAlDevice = IntPtr.Zero;
				MessageBox.Show(Interface.GetInterfaceString("errors_sound_openal_context"), Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			OpenAlContext = ContextHandle.Zero;
			MessageBox.Show(Interface.GetInterfaceString("errors_sound_openal_device"), Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}

		/// <summary>Deinitializes audio.</summary>
		internal static void Deinitialize()
		{
			StopAllSounds();
			UnloadAllBuffers();
			if (OpenAlContext != ContextHandle.Zero)
			{
				Alc.MakeContextCurrent(ContextHandle.Zero);
				Alc.DestroyContext(OpenAlContext);
				OpenAlContext = ContextHandle.Zero;
			}
			if (OpenAlDevice != IntPtr.Zero)
			{
				Alc.CloseDevice(OpenAlDevice);
				OpenAlDevice = IntPtr.Zero;
			}
		}


		// --- registering buffers ---

		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="path">The path to the sound.</param>
		/// <param name="radius">The default effective radius.</param>
		/// <returns>The handle to the sound buffer.</returns>
		internal static SoundBuffer RegisterBuffer(string path, double radius)
		{
			for (int i = 0; i < BufferCount; i++)
			{
				if (!(Buffers[i].Origin is PathOrigin)) continue;
				if (((PathOrigin)Buffers[i].Origin).Path == path)
				{
					return Buffers[i];
				}
			}
			if (Buffers.Length == BufferCount)
			{
				Array.Resize<SoundBuffer>(ref Buffers, Buffers.Length << 1);
			}
			Buffers[BufferCount] = new SoundBuffer(path, radius);
			BufferCount++;
			return Buffers[BufferCount - 1];
		}

		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="data">The raw sound data.</param>
		/// <param name="radius">The default effective radius.</param>
		/// <returns>The handle to the sound buffer.</returns>
		internal static SoundBuffer RegisterBuffer(OpenBveApi.Sounds.Sound data, double radius)
		{
			if (Buffers.Length == BufferCount)
			{
				Array.Resize<SoundBuffer>(ref Buffers, Buffers.Length << 1);
			}
			Buffers[BufferCount] = new SoundBuffer(data, radius);
			BufferCount++;
			return Buffers[BufferCount - 1];
		}


		// --- loading buffers ---

		/// <summary>Loads the specified sound buffer.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <returns>Whether loading the buffer was successful.</returns>
		internal static bool LoadBuffer(SoundBuffer buffer)
		{
			if (buffer.Loaded)
			{
				return true;
			}
			if (buffer.Ignore)
			{
				return false;
			}
			OpenBveApi.Sounds.Sound sound;
			if (buffer.Origin.GetSound(out sound))
			{
				if (sound.BitsPerSample == 8 | sound.BitsPerSample == 16)
				{
					byte[] bytes = GetMonoMix(sound);
					AL.GenBuffers(1, out buffer.OpenAlBufferName);
					ALFormat format = sound.BitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
					AL.BufferData(buffer.OpenAlBufferName, format, bytes, bytes.Length, sound.SampleRate);
					buffer.Duration = sound.Duration;
					buffer.Loaded = true;
					return true;
				}
			}
			buffer.Ignore = true;
			return false;
		}

		/// <summary>Loads all sound buffers immediately.</summary>
		internal static void LoadAllBuffers()
		{
			for (int i = 0; i < BufferCount; i++)
			{
				LoadBuffer(Buffers[i]);
			}
		}


		// --- unloading buffers ---

		/// <summary>Unloads the specified sound buffer.</summary>
		/// <param name="buffer"></param>
		internal static void UnloadBuffer(SoundBuffer buffer)
		{
			if (buffer.Loaded)
			{
				AL.DeleteBuffers(1, ref buffer.OpenAlBufferName);
				buffer.OpenAlBufferName = 0;
				buffer.Loaded = false;
				buffer.Ignore = false;
			}
		}

		/// <summary>Unloads all sound buffers immediately.</summary>
		internal static void UnloadAllBuffers()
		{
			for (int i = 0; i < BufferCount; i++)
			{
				UnloadBuffer(Buffers[i]);
			}
		}


		// --- play or stop sounds ---

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static SoundSource PlaySound(SoundBuffer buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, bool looped)
		{
			if (Sources.Length == SourceCount)
			{
				Array.Resize<SoundSource>(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, buffer.Radius, pitch, volume, position, null, 0, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="parent">The parent object the sound is attached to, or a null reference.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static SoundSource PlaySound(SoundBuffer buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, object parent, bool looped)
		{
			if (Sources.Length == SourceCount)
			{
				Array.Resize<SoundSource>(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, buffer.Radius, pitch, volume, position, parent, 0, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="train">The train the sound is attached to, or a null reference.</param>
		/// <param name="car">The car in the train the sound is attached to.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static SoundSource PlaySound(SoundBuffer buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, object train, int car, bool looped)
		{
			if (Sources.Length == SourceCount)
			{
				Array.Resize<SoundSource>(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, buffer.Radius, pitch, volume, position, train, car, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}

		/// <summary>Plays a car sound.</summary>
		/// <param name="sound">The car sound.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="train">The train the sound is attached to.</param>
		/// <param name="car">The car in the train the sound is attached to.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static void PlayCarSound(TrainManager.CarSound sound, double pitch, double volume, TrainManager.Train train, int car, bool looped)
		{
			if (sound.Buffer == null)
			{
				return;
			}
			if (train == null)
			{
				throw new InvalidDataException("A train and car must be specified");
			}
			sound.Source = PlaySound(sound.Buffer, pitch, volume, sound.Position, train, car, looped);
		}

		/// <summary>Stops the specified sound source.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		internal static void StopSound(SoundSource source)
		{
			if (source != null)
			{
				if (source.State == SoundSourceState.Playing)
				{
					AL.DeleteSources(1, ref source.OpenAlSourceName);
					source.OpenAlSourceName = 0;
				}
				source.State = SoundSourceState.Stopped;
			}
		}

		/// <summary>Stops all sounds.</summary>
		internal static void StopAllSounds()
		{
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i].State == SoundSourceState.Playing)
				{
					AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
					Sources[i].OpenAlSourceName = 0;
				}
				Sources[i].State = SoundSourceState.Stopped;
			}
		}

		/// <summary>Stops all sounds that are attached to the specified train.</summary>
		/// <param name="train">The train.</param>
		internal static void StopAllSounds(TrainManager.Train train)
		{
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i].Parent == train)
				{
					if (Sources[i].State == SoundSourceState.Playing)
					{
						AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
						Sources[i].OpenAlSourceName = 0;
					}
					Sources[i].State = SoundSourceState.Stopped;
				}
			}
		}


		// --- tests ---

		/// <summary>Checks whether the specified sound is playing or supposed to be playing.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		/// <returns>Whether the sound is playing or supposed to be playing.</returns>
		internal static bool IsPlaying(SoundSource source)
		{
			if (source != null)
			{
				if (source.State == SoundSourceState.PlayPending | source.State == SoundSourceState.Playing)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Checks whether the specified sound is stopped or supposed to be stopped.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		/// <returns>Whether the sound is stopped or supposed to be stopped.</returns>
		internal static bool IsStopped(SoundSource source)
		{
			if (source != null)
			{
				if (source.State == SoundSourceState.StopPending | source.State == SoundSourceState.Stopped)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Gets the duration of the specified sound buffer in seconds.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <returns>The duration of the sound buffer in seconds, or zero if the buffer could not be loaded.</returns>
		internal static double GetDuration(SoundBuffer buffer)
		{
			LoadBuffer(buffer);
			return buffer.Duration;
		}


		// --- statistics ---

		/// <summary>Gets the number of registered sound buffers.</summary>
		/// <returns>The number of registered sound buffers.</returns>
		internal static int GetNumberOfRegisteredBuffers()
		{
			return BufferCount;
		}

		/// <summary>Gets the number of loaded sound buffers.</summary>
		/// <returns>The number of loaded sound buffers.</returns>
		internal static int GetNumberOfLoadedBuffers()
		{
			int count = 0;
			for (int i = 0; i < BufferCount; i++)
			{
				if (Buffers[i].Loaded)
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>Gets the number of registered sound sources.</summary>
		/// <returns>The number of registered sound sources.</returns>
		internal static int GetNumberOfRegisteredSources()
		{
			return SourceCount;
		}

		/// <summary>Gets the number of playing sound sources.</summary>
		/// <returns>The number of playing sound sources.</returns>
		internal static int GetNumberOfPlayingSources()
		{
			int count = 0;
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i].State == SoundSourceState.Playing)
				{
					count++;
				}
			}
			return count;
		}

	}
}
