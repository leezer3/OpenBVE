#pragma warning disable CS0618 // Type or member is obsolete

using System;
using System.Threading;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Input;
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
				MyVolume = volume;
				MyPitch = pitch;
				MyValid = true;
				Source = source;
			}

			internal new void Stop()
			{
				MyValid = false;
				Source.Stop();
			}
		}

		// --- members ---
		/// <summary>The absolute on-disk path of the plugin's folder</summary>
		private readonly string PluginFolder;

		/// <summary>The absolute on-disk path of the train's folder</summary>
		private readonly string TrainFolder;

		private readonly IRuntime BaseApi;

		private IRuntime Api => BaseApi;

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
		internal NetPlugin(string pluginFile, string trainFolder, object api, TrainBase train)
		{
			PluginTitle = System.IO.Path.GetFileName(pluginFile);
			PluginValid = true;
			PluginMessage = null;
			Train = train;
			Panel = null;
			SupportsAI = AISupport.None;
			LastTime = 0.0;
			LastReverser = -2;
			LastPowerNotch = -1;
			LastBrakeNotch = -1;
			LastAspects = new int[] { };
			LastSection = -1;
			LastException = null;
			PluginFolder = Path.GetDirectoryName(pluginFile);
			TrainFolder = trainFolder;
			BaseApi = (IRuntime)api;
			
			SoundHandles = new SoundHandleEx[16];
			SoundHandlesCount = 0;
		}

		// --- functions ---
		public override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			LoadProperties properties = new LoadProperties(PluginFolder, TrainFolder, PlayMultiCarSound, PlayCarSound, PlayMultiCarSound, AddInterfaceMessage, AddScore, OpenDoors, CloseDoors);
			bool success;
			try
			{
				success = Api.Load(properties);
				SupportsAI = properties.AISupport;
			}
			catch (Exception ex)
			{
				if (ex is ThreadStateException)
				{
					//TTC plugin, broken when multi-threading is used
					success = false;
					properties.FailureReason = "This plugin does not function correctly with the current version of " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}) + ". Please ask the plugin developer to fix this.";
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
					LastException = ex;
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
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + PluginTitle + " failed to load for the following reason: " + properties.FailureReason);
				return false;
			}
			TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + PluginTitle + " failed to load for an unspecified reason.");
			return false;
		}

		public override void Unload()
		{
#if !DEBUG
			try {
#endif
			Api.Unload();
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void BeginJump(InitializationModes mode)
		{
#if !DEBUG
			try {
#endif
			Api.Initialize(mode);
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
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
			Api.Elapse(data);
			for (int i = 0; i < SoundHandlesCount; i++)
			{
				if (SoundHandles[i].Stopped | SoundHandles[i].Source.State == SoundSourceState.Stopped)
				{
					SoundHandles[i].Stop();
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
				LastException = ex;
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
				LastException = ex;
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
				LastException = ex;
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
				LastException = ex;
				throw;
			}
#endif
		}

		public override void KeyDown(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			Api.KeyDown(key);
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void KeyUp(VirtualKeys key)
		{
#if !DEBUG
			try {
#endif
			Api.KeyUp(key);
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void HornBlow(HornTypes type)
		{
#if !DEBUG
			try {
#endif
			Api.HornBlow(type);
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void DoorChange(DoorStates oldState, DoorStates newState)
		{
#if !DEBUG
			try {
#endif
			Api.DoorChange(oldState, newState);
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
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
				LastException = ex;
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
				LastException = ex;
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
				LastException = ex;
				throw;
			}
#endif
		}

		public override void TouchEvent(int groupIndex, int commandIndex)
		{
#if !DEBUG
			try {
#endif
			if (BaseApi is IRawRuntime rawRuntime)
			{
				rawRuntime.TouchEvent(groupIndex, commandIndex);
				
			}
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void TouchEvent(int groupIndex, Translations.Command command)
		{
#if !DEBUG
			try {
#endif
			if (BaseApi is IRawRuntime2 rawRuntime2)
			{
				rawRuntime2.TouchEvent(groupIndex, command);
			}
				

#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void RawKeyDown(Key key)
		{
#if !DEBUG
			try {
#endif
			if (BaseApi is IRawRuntime rawRuntime)
			{
				rawRuntime.RawKeyDown(key);
			}
			
#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
				throw;
			}
#endif
		}

		public override void RawKeyUp(Key key)
		{
#if !DEBUG
			try {
#endif
			if (BaseApi is IRawRuntime rawRuntime)
			{
				rawRuntime.RawKeyUp(key);
			}

#if !DEBUG
			} catch (Exception ex) {
				LastException = ex;
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
			TrainManagerBase.currentHost.AddMessage(Message, MessageDependency.Plugin, GameMode.Expert, Color, Time, null);
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

		/// <summary>May be called from a .Net plugin to open the train doors</summary>
		internal void OpenDoors(bool left, bool right)
		{
			Train.OpenDoors(left, right);
		}

		/// <summary>May be called from a .Net plugin to close the train doors</summary>
		internal void CloseDoors(bool left, bool right)
		{
			Train.CloseDoors(left, right);
		}

		/// <summary>May be called from a .Net plugin, in order to play a sound from the driver's car of a train</summary>
		/// <param name="index">The plugin-based of the sound to play</param>
		/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
		/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
		/// <param name="looped">Whether the sound is looped</param>
		/// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx PlayMultiCarSound(int index, double volume, double pitch, bool looped)
		{
			if (Train.Cars[Train.DriverCar].Sounds.Plugin.ContainsKey(index) && Train.Cars[Train.DriverCar].Sounds.Plugin[index].Buffer != null)
			{
				Train.Cars[Train.DriverCar].Sounds.Plugin[index].Play(pitch, volume, Train.Cars[Train.DriverCar], looped);
				if (SoundHandlesCount == SoundHandles.Length)
				{
					Array.Resize(ref SoundHandles, SoundHandles.Length << 1);
				}

				SoundHandles[SoundHandlesCount] = new SoundHandleEx(volume, pitch, Train.Cars[Train.DriverCar].Sounds.Plugin[index].Source);
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
		internal SoundHandleEx PlayCarSound(int index, double volume, double pitch, bool looped, int CarIndex)
		{
			if (CarIndex < 0 || CarIndex >= Train.Cars.Length)
			{
				// Not a valid car
				return null;
			}
			
			/*
			 * WARNING:
			 * A bug combined with an oversight in the original implementation of this feature allowed sounds to duplicate
			 *
			 * Any given sound should only be playing from one source. Thus, if the sound does not
			 * exist in the new car, clone from the driver car into a new CarSound with the same index in the new car.
			 *
			 * This allows any given sound index to be playing *once* in each car of the train
			 * (As opposed to the original intention of any sound index being available in any one given place)
			 *
			 * NOTES:
			 * If a separate soundset has been loaded via XML for the car, this may produce different sounds, but is unavoidable
			 */

			if (!Train.Cars[CarIndex].Sounds.Plugin.ContainsKey(index))
			{
				if (Train.Cars[Train.DriverCar].Sounds.Plugin.ContainsKey(index))
				{
					Train.Cars[CarIndex].Sounds.Plugin.Add(index, Train.Cars[Train.DriverCar].Sounds.Plugin[index].Clone());
				}
			}

			Train.Cars[CarIndex].Sounds.Plugin[index].Play(pitch, volume, Train.Cars[CarIndex], looped);
			if (SoundHandlesCount == SoundHandles.Length)
			{
				Array.Resize(ref SoundHandles, SoundHandles.Length << 1);
			}

			SoundHandles[SoundHandlesCount] = new SoundHandleEx(volume, pitch, Train.Cars[CarIndex].Sounds.Plugin[index].Source);
			SoundHandlesCount++;
			return SoundHandles[SoundHandlesCount - 1];
		}

		/// <summary>May be called from a .Net plugin, in order to play a sound from multiple cars of a train</summary>
		/// <param name="index">The plugin-based of the sound to play</param>
		/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
		/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
		/// <param name="looped">Whether the sound is looped</param>
		/// <param name="CarIndicies">The index of the cars which are to emit the sound</param>
		/// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx[] PlayMultiCarSound(int index, double volume, double pitch, bool looped, int[] CarIndicies)
		{
			SoundHandleEx[] soundHandles = new SoundHandleEx[CarIndicies.Length];
			for (int i = 0; i < CarIndicies.Length; i++)
			{
				soundHandles[i] = PlayCarSound(index, volume, pitch, looped, CarIndicies[i]);
			}
			return soundHandles;
		}
	}
}
