using System;
using System.Threading;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using SoundManager;
using SoundHandle = OpenBveApi.Runtime.SoundHandle;

namespace OpenBve
{
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : PluginManager.Plugin
	{

		// sound handle
		internal class SoundHandleEx : SoundHandle
		{
			internal readonly SoundsBase.SoundSource Source;
			internal SoundHandleEx(double volume, double pitch, SoundsBase.SoundSource source)
			{
				MyVolume = volume;
				MyPitch = pitch;
				MyValid = true;
				Source = source;
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
		internal NetPlugin(string pluginFile, string trainFolder, IRuntime api, TrainManager.Train train)
		{
			PluginTitle = System.IO.Path.GetFileName(pluginFile);
			PluginValid = true;
			PluginMessage = null;
			Train = train;
			Panel = null;
			SupportsAI = false;
			LastTime = 0.0;
			LastReverser = -2;
			LastPowerNotch = -1;
			LastBrakeNotch = -1;
			LastAspects = new int[] { };
			LastSection = -1;
			LastException = null;
			PluginFolder = System.IO.Path.GetDirectoryName(pluginFile);
			TrainFolder = trainFolder;
			Api = api;
			SoundHandles = new SoundHandleEx[16];
			SoundHandlesCount = 0;
		}

		// --- functions ---
		internal override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			LoadProperties properties = new LoadProperties(PluginFolder, TrainFolder, PlaySound, PlaySound, AddInterfaceMessage, AddScore);
			bool success;
			try
			{
				success = Api.Load(properties);
				SupportsAI = properties.AISupport == AISupport.Basic;
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
				Panel = properties.Panel ?? new int[] { };
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
				Interface.AddMessage(MessageType.Error, false, "The train plugin " + PluginTitle + " failed to load for the following reason: " + properties.FailureReason);
				return false;
			}

			Interface.AddMessage(MessageType.Error, false, "The train plugin " + PluginTitle + " failed to load for an unspecified reason.");
			return false;
		}
		internal override void Unload()
		{
#if !DEBUG
			try {
#endif
			Api.Unload();
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void BeginJump(InitializationModes mode)
		{
#if !DEBUG
			try {
#endif
			Api.Initialize(mode);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void EndJump()
		{
		}

		protected override void Elapse(ElapseData data)
		{
#if !DEBUG
			try {
#endif
			Api.Elapse(data);
			for (int i = 0; i < SoundHandlesCount; i++)
			{
				if (SoundHandles[i].Stopped | SoundHandles[i].Source.State == SoundSourceState.Stopped)
				{
					SoundHandles[i].Stop();
					SoundHandles[i].Source.Stop();
					SoundHandles[i] = SoundHandles[SoundHandlesCount - 1];
					SoundHandlesCount--;
					i--;
				}
				else
				{
					SoundHandles[i].Source.Pitch = Math.Max(0.01, SoundHandles[i].Pitch);
					SoundHandles[i].Source.Volume = Math.Max(0.0, SoundHandles[i].Volume);
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
			Api.SetReverser(reverser);
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
			Api.SetPower(powerNotch);
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
			Api.SetBrake(brakeNotch);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void KeyDown(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			Api.KeyDown(key);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void KeyUp(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			Api.KeyUp(key);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void HornBlow(HornTypes type)
		{
#if !DEBUG
			try {
#endif
			Api.HornBlow(type);
#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
#endif
		}
		internal override void DoorChange(DoorStates oldState, DoorStates newState)
		{
#if !DEBUG
			try {
#endif
			Api.DoorChange(oldState, newState);
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
			Api.SetSignal(signal);
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
			Api.SetBeacon(beacon);
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
			Api.PerformAI(data);
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
			Game.AddMessage(Message, MessageManager.MessageDependency.Plugin, Interface.GameMode.Expert, Color, Game.SecondsSinceMidnight + Time, null);
		}

		/// <summary>May be called from a .Net plugin, in order to add a score to the post-game log</summary>
		/// <param name="Score">The score to add</param>
		/// <param name="Message">The message to display in the post-game log</param>
		/// <param name="Color">The color of the in-game message</param>
		/// <param name="Timeout">The time in seconds for which to display the in-game message</param>
		internal void AddScore(int Score, string Message, MessageColor Color, double Timeout)
		{
			Game.CurrentScore.CurrentValue += Score;

			int n = Game.ScoreMessages.Length;
			Array.Resize(ref Game.ScoreMessages, n + 1);
			Game.ScoreMessages[n] = new Game.ScoreMessage
			{
				Value = Score,
				Color = Color,
				RendererPosition = new Vector2(0, 0),
				RendererAlpha = 0.0,
				Text = Message,
				Timeout = Timeout
			};
		}

		/// <summary>May be called from a .Net plugin, in order to play a sound from the driver's car of a train</summary>
		/// <param name="index">The plugin-based of the sound to play</param>
		/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
		/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
		/// <param name="looped">Whether the sound is looped</param>
		/// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped)
		{
			if (index >= 0 && index < Train.Cars[Train.DriverCar].Sounds.Plugin.Length && Train.Cars[Train.DriverCar].Sounds.Plugin[index].Buffer != null)
			{
				SoundsBase.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[index].Buffer;
				Vector3 position = Train.Cars[Train.DriverCar].Sounds.Plugin[index].Position;
				SoundsBase.SoundSource source = Program.Sounds.PlaySound(buffer, pitch, volume, position, Train, Train.DriverCar, looped);
				if (SoundHandlesCount == SoundHandles.Length)
				{
					Array.Resize(ref SoundHandles, SoundHandles.Length << 1);
				}
				SoundHandles[SoundHandlesCount] = new SoundHandleEx(volume, pitch, source);
				SoundHandlesCount++;
				return SoundHandles[SoundHandlesCount - 1];
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
			if (index >= 0 && index < Train.Cars[Train.DriverCar].Sounds.Plugin.Length && Train.Cars[Train.DriverCar].Sounds.Plugin[index].Buffer != null && CarIndex < Train.Cars.Length && CarIndex >= 0)
			{
				SoundsBase.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[index].Buffer;
				Vector3 position = Train.Cars[Train.DriverCar].Sounds.Plugin[index].Position;
				SoundsBase.SoundSource source = Program.Sounds.PlaySound(buffer, pitch, volume, position, Train, CarIndex, looped);
				if (SoundHandlesCount == SoundHandles.Length)
				{
					Array.Resize(ref SoundHandles, SoundHandles.Length << 1);
				}
				SoundHandles[SoundHandlesCount] = new SoundHandleEx(volume, pitch, source);
				SoundHandlesCount++;
				return SoundHandles[SoundHandlesCount - 1];
			}
			return null;
		}
	}

}
