using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Sounds;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace SoundManager
{
	public abstract partial class SoundsBase
	{
		// --- members ---

		/// <summary>The current OpenAL device.</summary>
		private IntPtr OpenAlDevice = IntPtr.Zero;

		/// <summary>The current OpenAL context.</summary>
		private ContextHandle OpenAlContext;

		/// <summary>A list of all sound buffers.</summary>
		private SoundBuffer[] Buffers = new SoundBuffer[16];

		/// <summary>The number of sound buffers.</summary>
		private int BufferCount;

		/// <summary>A list of all sound sources.</summary>
		protected internal static SoundSource[] Sources = new SoundSource[16];

		/// <summary>The number of sound sources.</summary>
		protected internal static int SourceCount = 0;

		/// <summary>The gain threshold. Sounds with gains below this value are not played.</summary>
		protected const double GainThreshold = 0.0001;

		/// <summary>Whether all sounds are mute.</summary>
		public bool GlobalMute = false;

		/// <summary>Whether to play microphone sound or not.</summary>
		public bool IsPlayingMicSounds = false;

		/// <summary>Sampling rate of a microphone</summary>
		private const int SamplingRate = 44100;

		/// <summary>Buffer size of a microphone</summary>
		private const int BufferSize = 4410;

		/// <summary>The current OpenAL AudioCapture device.</summary>
		protected AudioCapture OpenAlMic = null;

		/// <summary>A list of all microphone sources.</summary>
		protected readonly List<MicSource> MicSources = new List<MicSource>();

		/// <summary>Buffer for storing recorded data.</summary>
		protected readonly byte[] MicStore = new byte[BufferSize * 2];

		private readonly HostInterface CurrentHost;

		/// <summary>Whether sound events are currently suppressed</summary>
		public static bool SuppressSoundEvents = false;

		protected internal int systemMaxSounds = int.MaxValue;
		// --- linear distance clamp model ---

		/// <summary>The factor by which the inner radius is multiplied to give the outer radius.</summary>
		public double OuterRadiusFactor;
		protected double OuterRadiusFactorSpeed;
		protected double OuterRadiusFactorMaximumSpeed;
		protected double OuterRadiusFactorMinimum;
		protected double OuterRadiusFactorMaximum;


		// --- inverse distance clamp model ---

		public double LogClampFactor = -15.0;
		protected const double MinLogClampFactor = -20.0;
		protected const double MaxLogClampFactor = -1.0;

		internal readonly Thread SoundLoaderThread;

		private bool soundThread = true;
		private readonly ConcurrentQueue<ThreadStart> SoundLoaderQueue = new ConcurrentQueue<ThreadStart>();

		protected SoundsBase(HostInterface currentHost)
		{
			CurrentHost = currentHost;
			SoundLoaderThread = new Thread(SoundThread);
		}

		/// <summary>Initializes audio.</summary>
		/// <returns>Whether initializing audio was successful.</returns>
		/// <remarks>A call to DeInitialize should be made when closing the program to release any held resources</remarks>
		public void Initialize(SoundRange range)
		{
			if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				/*
				*  If shipping an AnyCPU build and OpenALSoft / SDL, these are architecture specific PInvokes
				*  Add the appropriate search path so this will work (common convention)
				*/
				string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
				if (path != null)
				{
					path = Path.Combine(path, IntPtr.Size == 4 ? "x86" : "x64");
					bool ok = SetDllDirectory(path);
					if (!ok) throw new System.ComponentModel.Win32Exception();
				}
			}
			DeInitialize();

			switch (range)
			{
				case SoundRange.Low:
					OuterRadiusFactorMinimum = 2.0;
					OuterRadiusFactorMaximum = 8.0;
					OuterRadiusFactorMaximumSpeed = 1.0;
					break;
				case SoundRange.Medium:
					OuterRadiusFactorMinimum = 4.0;
					OuterRadiusFactorMaximum = 16.0;
					OuterRadiusFactorMaximumSpeed = 2.0;
					break;
				case SoundRange.High:
					OuterRadiusFactorMinimum = 6.0;
					OuterRadiusFactorMaximum = 24.0;
					OuterRadiusFactorMaximumSpeed = 3.0;
					break;
			}
			OuterRadiusFactor = Math.Sqrt(OuterRadiusFactorMinimum * OuterRadiusFactorMaximum);
			OuterRadiusFactorSpeed = 0.0;
			OpenAlDevice = Alc.OpenDevice(null);
			string deviceName = Alc.GetString(OpenAlDevice, AlcGetString.DefaultDeviceSpecifier);
			if ((Environment.OSVersion.Platform == PlatformID.Win32S | Environment.OSVersion.Platform == PlatformID.Win32Windows | Environment.OSVersion.Platform == PlatformID.Win32NT) && deviceName == "Generic Software")
			{
				/*
				 * Creative OpenAL implementation on Windows seems to be limited to max 16 simulataneous sounds
				 * Now shipping OpenAL Soft, but detect this and don't glitch
				 * Further note that the current version of OpenAL Soft (1.20.0 at the time of writing) does not like OpenTK
				 * The version in use is 1.17.0 found here: https://openal-soft.org/openal-binaries/
				 */
				systemMaxSounds = 16;
			}
			try
			{
				OpenAlMic = new AudioCapture(AudioCapture.DefaultDevice, SamplingRate, ALFormat.Mono16, BufferSize);
			}
			catch
			{
				OpenAlMic = null;
			}
			SoundLoaderThread.Start();
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
						MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve,  new[] {"errors","sound_openal_version"}), Translations.GetInterfaceString(CurrentHost.Application, new[] {"program","title"}), MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					AL.DistanceModel(ALDistanceModel.None);
					return;
				}
				Alc.CloseDevice(OpenAlDevice);
				OpenAlDevice = IntPtr.Zero;
				if (OpenAlMic != null)
				{
					OpenAlMic.Dispose();
					OpenAlMic = null;
				}
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"errors","sound_openal_context"}), Translations.GetInterfaceString(CurrentHost.Application, new[] {"program","title"}), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			OpenAlContext = ContextHandle.Zero;
			MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"errors","sound_openal_device"}), Translations.GetInterfaceString(CurrentHost.Application, new[] {"program","title"}), MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}

		/// <summary>Deinitializes audio.</summary>
		public void DeInitialize()
		{
			soundThread = false;
			StopAllSounds();
			UnloadAllBuffers();
			UnloadAllMicBuffers();
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
			if (OpenAlMic != null)
			{
				OpenAlMic.Dispose();
				OpenAlMic = null;
			}
		}
		
		private void SoundThread()
		{
			soundThread = true;
			while (soundThread)
			{
				if (SoundLoaderQueue.TryDequeue(out ThreadStart result))
				{
					result.Invoke();
				}
				else
				{
					Thread.Sleep(100);	
				}
			}
		}

		// --- registering buffers ---

		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="path">The path to the sound.</param>
		/// <param name="radius">The default effective radius.</param>
		/// <returns>The handle to the sound buffer.</returns>
		public SoundBuffer RegisterBuffer(string path, double radius)
		{
			if (!File.Exists(path))
			{
				return null;
			}
			for (int i = 0; i < BufferCount; i++)
			{
				if (!(Buffers[i].Origin is PathOrigin))
				{
					continue;
				}

				if (((PathOrigin)Buffers[i].Origin).Path == path)
				{
					return Buffers[i];
				}
			}
			if (Buffers.Length == BufferCount)
			{
				Array.Resize(ref Buffers, Buffers.Length << 1);
			}

			try
			{
				Buffers[BufferCount] = new SoundBuffer(CurrentHost, path, radius);
			}
			catch
			{
				return null;
			}
			BufferCount++;
			return Buffers[BufferCount - 1];
		}

		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="data">The raw sound data.</param>
		/// <param name="radius">The default effective radius.</param>
		/// <returns>The handle to the sound buffer.</returns>
		public SoundBuffer RegisterBuffer(Sound data, double radius)
		{
			if (Buffers.Length == BufferCount)
			{
				Array.Resize(ref Buffers, Buffers.Length << 1);
			}

			try
			{
				Buffers[BufferCount] = new SoundBuffer(data, radius);
			}
			catch
			{
				return null;
			}
			BufferCount++;
			return Buffers[BufferCount - 1];
		}

		// --- loading buffers ---

		/// <summary>Loads the specified sound buffer.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <returns>Whether loading the buffer was successful.</returns>
		public void LoadBuffer(SoundBuffer buffer)
		{
			SoundLoaderQueue.Enqueue(buffer.Load);
		}

		// --- unloading buffers ---

		/// <summary>Unloads all sound buffers immediately.</summary>
		internal void UnloadAllBuffers()
		{
			for (int i = 0; i < BufferCount; i++)
			{
				Buffers[i].Unload();
			}
		}

		/// <summary>Unloads the specified microphone buffer.</summary>
		/// <param name="source"></param>
		/// <param name="number"></param>
		protected void UnloadMicBuffers(int source, int number)
		{
			if (number > 0)
			{
				int[] buffers = AL.SourceUnqueueBuffers(source, number);
				AL.DeleteBuffers(buffers);
			}
		}

		/// <summary>Unloads all microphone buffers immediately.</summary>
		private void UnloadAllMicBuffers()
		{
			foreach (MicSource source in MicSources)
			{
				AL.GetSource(source.OpenAlSourceName, ALGetSourcei.BuffersProcessed, out int state);
				UnloadMicBuffers(source.OpenAlSourceName, state);
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
		public SoundSource PlaySound(SoundBuffer buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, bool looped)
		{
			if (Sources.Length == SourceCount)
			{
				Array.Resize(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, buffer.Radius, pitch, volume, position, null, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train car is specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="parent">The parent object the sound is attached to, or a null reference.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		public SoundSource PlaySound(SoundHandle buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, object parent, bool looped)
		{
			if (buffer is SoundBuffer b)
			{
				if (Sources.Length == SourceCount)
				{
					Array.Resize(ref Sources, Sources.Length << 1);
				}
				Sources[SourceCount] = new SoundSource(b, b.Radius, pitch, volume, position, parent, looped);
				SourceCount++;
				return Sources[SourceCount - 1];
			}

			return null;
		}

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train car is specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		public SoundSource PlaySound(SoundHandle buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, bool looped)
		{
			if (buffer is SoundBuffer b)
			{
				if (Sources.Length == SourceCount)
				{
					Array.Resize(ref Sources, Sources.Length << 1);
				}
				Sources[SourceCount] = new SoundSource(b, b.Radius, pitch, volume, position, null, looped);
				SourceCount++;
				return Sources[SourceCount - 1];
			}

			return null;
		}

		/// <summary>Register the position to play microphone input.</summary>
		/// <param name="position">The position.</param>
		/// <param name="backwardTolerance">allowed tolerance in the backward direction</param>
		/// <param name="forwardTolerance">allowed tolerance in the forward direction</param>
		public void PlayMicSound(OpenBveApi.Math.Vector3 position, double backwardTolerance, double forwardTolerance)
		{
			if (OpenAlMic == null)
			{
				// This hardware has no AudioCapture device.
				return;
			}

			MicSources.Add(new MicSource(OpenAlMic, MicStore, position, backwardTolerance, forwardTolerance));
		}

		/// <summary>Stops the specified sound source.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		public void StopSound(SoundSource source)
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
		public void StopAllSounds()
		{
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i] == null)
				{
					continue;
				}
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
		public virtual void StopAllSounds(object train)
		{
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i] == null)
				{
					continue;
				}
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
		/// <param name="Source">The sound source, or a null reference.</param>
		/// <returns>Whether the sound is playing or supposed to be playing.</returns>
		public bool IsPlaying(object Source)
		{
			if (Source is SoundSource source)
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
		protected bool IsStopped(SoundSource source)
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

		// --- statistics ---

		/// <summary>Gets the number of registered sound buffers.</summary>
		/// <returns>The number of registered sound buffers.</returns>
		public int GetNumberOfRegisteredBuffers()
		{
			return BufferCount;
		}

		/// <summary>Gets the number of loaded sound buffers.</summary>
		/// <returns>The number of loaded sound buffers.</returns>
		public int GetNumberOfLoadedBuffers()
		{
			int count = 0;
			for (int i = 0; i < BufferCount; i++)
			{
				if (Buffers[i].Loaded == SoundBufferState.Loading || Buffers[i].Loaded == SoundBufferState.Loaded)
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>Gets the number of registered sound sources.</summary>
		/// <returns>The number of registered sound sources.</returns>
		public int GetNumberOfRegisteredSources()
		{
			return SourceCount;
		}

		/// <summary>Gets the number of playing sound sources.</summary>
		/// <returns>The number of playing sound sources.</returns>
		public int GetNumberOfPlayingSources()
		{
			int count = 0;
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i] == null)
				{
					continue;
				}
				if (Sources[i].State == SoundSourceState.Playing)
				{
					count++;
				}
			}
			return count;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetDllDirectory(string path);
	}
}
