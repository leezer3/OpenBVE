using System;
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
		private string PluginFolder;
		private string TrainFolder;
		private IRuntime Api;
		private SoundHandleEx[] SoundHandles;
		private int SoundHandlesCount;
		
		// --- constructors ---
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
			LoadProperties properties = new LoadProperties(this.PluginFolder, this.TrainFolder, this.PlaySound);
			bool success;
			#if !DEBUG
			try {
				#endif
				success = this.Api.Load(properties);
				base.SupportsAI = properties.AISupport == AISupport.Basic;
				#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
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
					base.LastException = ex;
					throw;
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
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
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
				base.LastException = ex;
				throw;
			}
			#endif
		}
		internal override void SetSignal(SignalData[] signal) {
			#if !DEBUG
			try {
				#endif
//				if (this.Train == TrainManager.PlayerTrain) {
//					for (int i = 0; i < signal.Length; i++) {
//						Game.AddDebugMessage(i.ToString() + " - " + signal[i].Aspect.ToString(), 3.0);
//					}
//				}
				this.Api.SetSignal(signal);
				#if !DEBUG
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			#endif
		}
		internal override void SetBeacon(BeaconData beacon) {
//			if (this.Train == TrainManager.PlayerTrain) {
//				Game.AddDebugMessage("Beacon, type=" + beacon.Type.ToString() + ", aspect=" + beacon.Signal.Aspect.ToString() + ", data=" + beacon.Optional.ToString(), 3.0);
//			}
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
		internal override void PerformAI(AIData data) {
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
	}
	
}