using System;
using System.Threading;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Trains;
using SoundHandle = OpenBveApi.Runtime.SoundHandle;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : Plugin
	{

		// sound handle
		internal class SoundHandleEx : SoundHandle
		{
			internal readonly SoundSource Source;

			internal SoundHandleEx(double volume, double pitch, SoundSource source)
			{
				base.MyVolume = volume;
				base.MyPitch = pitch;
				base.MyValid = true;
				this.Source = source;
			}

			internal new void Stop()
			{
				base.MyValid = false;
				Source.Stop();
			}
		}

		// --- members ---
		/// <summary>The absolute on-disk path of the plugin's folder</summary>
		private readonly string PluginFolder;

		/// <summary>The absolute on-disk path of the train's folder</summary>
		private readonly string TrainFolder;

		private readonly IRuntime Api;

		/// <summary>An array containing all of the plugin's current sound handles</summary>
		private SoundHandleEx[] SoundHandles;

		/// <summary>The total number of sound handles currently in use</summary>
		private int SoundHandlesCount;

		// --- constructors ---
		/// <summary>Initialises a new instance of a .Net based plugin</summary>
		/// <param name="pluginFile">The absolute on-disk path of the plugin to load</param>
		/// <param name="trainFolder">The absolute on-disk path of the train's folder</param>
		/// <param name="api">The base OpenBVE runtime interface</param>
		/// <param name="train">The base train</param>
		internal NetPlugin(string pluginFile, string trainFolder, IRuntime api, TrainBase train)
		{
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Train = train;
			base.Panel = null;
			base.SupportsAI = AISupport.None;
			base.LastTime = 0.0;
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspects = new int[] { };
			base.LastSection = -1;
			base.LastException = null;
			this.PluginFolder = System.IO.Path.GetDirectoryName(pluginFile);
			this.TrainFolder = trainFolder;
			this.Api = api;
			this.SoundHandles = new SoundHandleEx[16];
			this.SoundHandlesCount = 0;
		}

		// --- functions ---
		public override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			LoadProperties properties = new LoadProperties(this.PluginFolder, this.TrainFolder, this.PlaySound, this.PlaySound, this.AddInterfaceMessage, this.AddScore);
			bool success;
			try
			{
				success = this.Api.Load(properties);
				base.SupportsAI = properties.AISupport;
			}
			catch (Exception ex)
			{
				if (ex is ThreadStateException)
				{
					//TTC plugin, broken when multi-threading is used
					success = false;
					properties.FailureReason = "This plugin does not function correctly with current versions of openBVE. Please ask the plugin developer to fix this.";
				}
				else
				{
					success = false;
					properties.FailureReason = ex.Message;
				}
			}

			if (success)
			{
				base.Panel = properties.Panel ?? new int[] { };
#if !DEBUG
				try {
#endif
				Api.SetVehicleSpecs(specs);
				Api.Initialize(mode);
#if !DEBUG
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
#endif
				UpdatePower();
				UpdateBrake();
				UpdateReverser();
				return true;
			}

			if (properties.FailureReason != null)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for the following reason: " + properties.FailureReason);
				return false;
			}
			TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for an unspecified reason.");
			return false;
		}

		public override void Unload()
		{
#if !DEBUG
			try {
#endif
			this.Api.Unload();
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void BeginJump(InitializationModes mode)
		{
#if !DEBUG
			try {
#endif
			this.Api.Initialize(mode);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void EndJump()
		{
		}

		protected override void Elapse(ref ElapseData data)
		{
#if !DEBUG
			try {
#endif
			this.Api.Elapse(data);
			for (int i = 0; i < this.SoundHandlesCount; i++)
			{
				if (this.SoundHandles[i].Stopped | this.SoundHandles[i].Source.State == SoundSourceState.Stopped)
				{
					this.SoundHandles[i].Stop();
					this.SoundHandles[i] = this.SoundHandles[this.SoundHandlesCount - 1];
					this.SoundHandlesCount--;
					i--;
				}
				else
				{
					this.SoundHandles[i].Source.Pitch = Math.Max(0.01, this.SoundHandles[i].Pitch);
					this.SoundHandles[i].Source.Volume = Math.Max(0.0, this.SoundHandles[i].Volume);
				}
			}
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void SetReverser(int reverser)
		{
#if !DEBUG
			try {
#endif
			this.Api.SetReverser(reverser);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void SetPower(int powerNotch)
		{
#if !DEBUG
			try {
#endif
			this.Api.SetPower(powerNotch);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void SetBrake(int brakeNotch)
		{
#if !DEBUG
			try {
#endif
			this.Api.SetBrake(brakeNotch);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void KeyDown(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			this.Api.KeyDown(key);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void KeyUp(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			this.Api.KeyUp(key);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void HornBlow(HornTypes type)
		{
#if !DEBUG
			try {
#endif
			this.Api.HornBlow(type);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		public override void DoorChange(DoorStates oldState, DoorStates newState)
		{
#if !DEBUG
			try {
#endif
			this.Api.DoorChange(oldState, newState);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void SetSignal(SignalData[] signal)
		{
#if !DEBUG
			try {
#endif
			this.Api.SetSignal(signal);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void SetBeacon(BeaconData beacon)
		{
#if !DEBUG
			try {
#endif
			this.Api.SetBeacon(beacon);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		protected override void PerformAI(AIData data)
		{
#if !DEBUG
			try {
#endif
			this.Api.PerformAI(data);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}

		/// <summary>May be called from a .Net plugin, in order to add a message to the in-game display</summary>
		/// <param name="Message">The message to display</param>
		/// <param name="Color">The color in which to display the message</param>
		/// <param name="Time">The time in seconds for which to display the message</param>
		internal void AddInterfaceMessage(string Message, MessageColor Color, double Time)
		{
			TrainManagerBase.currentHost.AddMessage(Message, MessageDependency.Plugin, GameMode.Expert, Color, TrainManagerBase.currentHost.InGameTime + Time, null);
		}

		/// <summary>May be called from a .Net plugin, in order to add a score to the post-game log</summary>
		/// <param name="Score">The score to add</param>
		/// <param name="Message">The message to display in the post-game log</param>
		/// <param name="Color">The color of the in-game message</param>
		/// <param name="Timeout">The time in seconds for which to display the in-game message</param>
		internal void AddScore(int Score, string Message, MessageColor Color, double Timeout)
		{
			TrainManagerBase.currentHost.AddScore(Score, Message, Color, Timeout);
		}

		/// <summary>May be called from a .Net plugin, in order to play a sound from the driver's car of a train</summary>
		/// <param name="index">The plugin-based of the sound to play</param>
		/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
		/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
		/// <param name="looped">Whether the sound is looped</param>
		/// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped)
		{
			if (index >= 0 && index < this.Train.Cars[this.Train.DriverCar].Sounds.Plugin.Length && this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer != null)
			{
				Train.Cars[Train.DriverCar].Sounds.Plugin[index].Play(pitch, volume, this.Train.Cars[this.Train.DriverCar], looped);
				if (this.SoundHandlesCount == this.SoundHandles.Length)
				{
					Array.Resize(ref this.SoundHandles, this.SoundHandles.Length << 1);
				}

				this.SoundHandles[this.SoundHandlesCount] = new SoundHandleEx(volume, pitch, Train.Cars[Train.DriverCar].Sounds.Plugin[index].Source);
				this.SoundHandlesCount++;
				return this.SoundHandles[this.SoundHandlesCount - 1];
			}

			return null;
		}

		/// <summary>May be called from a .Net plugin, in order to play a sound from a specific car of a train</summary>
		/// <param name="index">The plugin-based of the sound to play</param>
		/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
		/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
		/// <param name="looped">Whether the sound is looped</param>
		/// <param name="CarIndex">The index of the car which is to emit the sound</param>
		/// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped, int CarIndex)
		{
			if (index >= 0 && index < this.Train.Cars[this.Train.DriverCar].Sounds.Plugin.Length && this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer != null && CarIndex < this.Train.Cars.Length && CarIndex >= 0)
			{
				Train.Cars[Train.DriverCar].Sounds.Plugin[index].Play(pitch, volume, this.Train.Cars[CarIndex], looped);
				if (this.SoundHandlesCount == this.SoundHandles.Length)
				{
					Array.Resize(ref this.SoundHandles, this.SoundHandles.Length << 1);
				}

				this.SoundHandles[this.SoundHandlesCount] = new SoundHandleEx(volume, pitch, Train.Cars[Train.DriverCar].Sounds.Plugin[index].Source);
				this.SoundHandlesCount++;
				return this.SoundHandles[this.SoundHandlesCount - 1];
			}

			return null;
		}
	}
}
