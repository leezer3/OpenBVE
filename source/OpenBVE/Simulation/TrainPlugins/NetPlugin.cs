﻿using System;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;

namespace OpenBve {
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : PluginManager.Plugin {
		
		// sound handle
		internal class SoundHandleEx : SoundHandle {
			internal Sounds.SoundSource Source;
			internal SoundHandleEx(double volume, double pitch, Sounds.SoundSource source) {
				base.MyVolume = volume;
				base.MyPitch = pitch;
				base.MyValid = true;
				this.Source = source;
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
		internal NetPlugin(string pluginFile, string trainFolder, IRuntime api, TrainManager.Train train) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Train = train;
			base.Panel = null;
			base.SupportsAI = false;
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
		internal override bool Load(VehicleSpecs specs, InitializationModes mode) {
			LoadProperties properties = new LoadProperties(this.PluginFolder, this.TrainFolder, this.PlaySound, this.AddInterfaceMessage);
			bool success = false;
			#if !DEBUG
			try {
				#endif
				success = this.Api.Load(properties);
				base.SupportsAI = properties.AISupport == AISupport.Basic;
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
			if (success) {
				base.Panel = properties.Panel ?? new int[] { };
				#if !DEBUG
				try {
					#endif
					Api.SetVehicleSpecs(specs);
					Api.Initialize(mode);
					#if !DEBUG
				} catch (Exception ex) {
					Crash(ex);
				}
				#endif
				UpdatePower();
				UpdateBrake();
				UpdateReverser();
				return true;
			} else if (properties.FailureReason != null) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for the following reason: " + properties.FailureReason);
				return false;
			} else {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for an unspecified reason.");
				return false;
			}
		}
		internal override void Unload() {
			#if !DEBUG
			try {
				#endif
				this.Api.Unload();
				#if !DEBUG
			} catch {
				//Not too worried about exceptions raised when unloading the plugin really...
			}
			#endif
		}
		internal override void BeginJump(InitializationModes mode) {
			#if !DEBUG
			try {
				#endif
				this.Api.Initialize(mode);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void EndJump() { }
		internal override void Elapse(ElapseData data) {
			#if !DEBUG
			try {
				#endif
				this.Api.Elapse(data);
				for (int i = 0; i < this.SoundHandlesCount; i++) {
					if (this.SoundHandles[i] == null)
					{
						continue;
					}
					if (this.SoundHandles[i].Stopped | this.SoundHandles[i].Source.State == Sounds.SoundSourceState.Stopped) {
						this.SoundHandles[i].Stop();
						this.SoundHandles[i].Source.Stop();
						this.SoundHandles[i] = this.SoundHandles[this.SoundHandlesCount - 1];
						this.SoundHandlesCount--;
						i--;
					} else {
						this.SoundHandles[i].Source.Pitch = Math.Max(0.01, this.SoundHandles[i].Pitch);
						this.SoundHandles[i].Source.Volume = Math.Max(0.0, this.SoundHandles[i].Volume);
					}
				}
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void SetReverser(int reverser) {
			#if !DEBUG
			try {
				#endif
				this.Api.SetReverser(reverser);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void SetPower(int powerNotch) {
			#if !DEBUG
			try {
				#endif
				this.Api.SetPower(powerNotch);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void SetBrake(int brakeNotch) {
			#if !DEBUG
			try {
				#endif
				this.Api.SetBrake(brakeNotch);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void KeyDown(VirtualKeys key) {
			#if !DEBUG
			try {
				#endif
				this.Api.KeyDown(key);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void KeyUp(VirtualKeys key) {
			#if !DEBUG
			try {
				#endif
				this.Api.KeyUp(key);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void HornBlow(HornTypes type) {
			#if !DEBUG
			try {
				#endif
				this.Api.HornBlow(type);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void DoorChange(DoorStates oldState, DoorStates newState) {
			#if !DEBUG
			try {
				#endif
				this.Api.DoorChange(oldState, newState);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void SetSignal(SignalData[] signal) {
			#if !DEBUG
			try {
				#endif
				this.Api.SetSignal(signal);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void SetBeacon(BeaconData beacon) {
			#if !DEBUG
			try {
				#endif
				this.Api.SetBeacon(beacon);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}
		internal override void PerformAI(AIData data) {
			#if !DEBUG
			try {
				#endif
				this.Api.PerformAI(data);
				#if !DEBUG
			} catch (Exception ex) {
				Crash(ex);
			}
			#endif
		}

		/// <summary>Is called when a .Net based plugin crashes during the game</summary>
		/// <param name="ex">The exception raised</param>
		internal void Crash(Exception ex)
		{
			try
			{
				base.LastException = ex;
				CrashHandler.PluginCrash(ex.ToString());
				Unload();
				for (int i = 0; i < TrainManager.Trains.Length; i++)
				{
					if (TrainManager.Trains[i].Plugin.PluginTitle == base.PluginTitle)
					{
						TrainManager.Trains[i].Plugin = null;
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>May be called from a .Net plugin, in order to add a message to the in-game display</summary>
		/// <param name="Message">The message to display</param>
		/// <param name="Color">The color in which to display the message</param>
		/// <param name="Time">The time in seconds for which to display the message</param>
		internal void AddInterfaceMessage(string Message, MessageColor Color, double Time)
		{
			Game.AddMessage(Message, Game.MessageDependency.None, Interface.GameMode.Expert, Color, Game.SecondsSinceMidnight + Time);
		}

        /// <summary>May be called from a .Net plugin, in order to play a sound from the driver's car of a train</summary>
        /// <param name="index">The plugin-based of the sound to play</param>
        /// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
        /// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
        /// <param name="looped">Whether the sound is looped</param>
        /// <returns>The sound handle, or null if not successful</returns>
		internal SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped) {
			if (index >= 0 && index < this.Train.Cars[this.Train.DriverCar].Sounds.Plugin.Length && this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer != null) {
				Sounds.SoundBuffer buffer = this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer;
				OpenBveApi.Math.Vector3 position = this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Position;
				Sounds.SoundSource source = Sounds.PlaySound(buffer, pitch, volume, position, this.Train, this.Train.DriverCar, looped);
				if (this.SoundHandlesCount == this.SoundHandles.Length) {
					Array.Resize<SoundHandleEx>(ref this.SoundHandles, this.SoundHandles.Length << 1);
				}
				this.SoundHandles[this.SoundHandlesCount] = new SoundHandleEx(volume, pitch, source);
				this.SoundHandlesCount++;
				return this.SoundHandles[this.SoundHandlesCount - 1];
			} else {
				return null;
			}
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
                Sounds.SoundBuffer buffer = this.Train.Cars[CarIndex].Sounds.Plugin[index].Buffer;
                OpenBveApi.Math.Vector3 position = this.Train.Cars[CarIndex].Sounds.Plugin[index].Position;
                Sounds.SoundSource source = Sounds.PlaySound(buffer, pitch, volume, position, this.Train, CarIndex, looped);
                if (this.SoundHandlesCount == this.SoundHandles.Length)
                {
                    Array.Resize<SoundHandleEx>(ref this.SoundHandles, this.SoundHandles.Length << 1);
                }
                this.SoundHandles[this.SoundHandlesCount] = new SoundHandleEx(volume, pitch, source);
                this.SoundHandlesCount++;
                return this.SoundHandles[this.SoundHandlesCount - 1];
            }
            else
            {
                return null;
            }
        }
	}
	
}