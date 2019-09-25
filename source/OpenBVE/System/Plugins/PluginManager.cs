﻿using System;
using System.Collections.Generic;
using LibRender2.Cameras;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using RouteManager2;
using RouteManager2.Stations;

namespace OpenBve {
	internal static class PluginManager {
		
		/// <summary>Represents an abstract plugin.</summary>
		internal abstract class Plugin {
			
			// --- members ---
			/// <summary>The file title of the plugin, including the file extension.</summary>
			internal string PluginTitle;
			/// <summary>Whether the plugin is the default ATS/ATC plugin.</summary>
			internal bool IsDefault;
			/// <summary>Whether the plugin returned valid information in the last Elapse call.</summary>
			internal bool PluginValid;
			/// <summary>The debug message the plugin returned in the last Elapse call.</summary>
			internal string PluginMessage;
			/// <summary>The train the plugin is attached to.</summary>
			internal TrainManager.Train Train;
			/// <summary>The array of panel variables.</summary>
			internal int[] Panel;
			/// <summary>Whether the plugin supports the AI.</summary>
			internal bool SupportsAI;
			/// <summary>The last in-game time reported to the plugin.</summary>
			internal double LastTime;
			/// <summary>The last reverser reported to the plugin.</summary>
			internal int LastReverser;
			/// <summary>The last power notch reported to the plugin.</summary>
			internal int LastPowerNotch;
			/// <summary>The last brake notch reported to the plugin.</summary>
			internal int LastBrakeNotch;
			/// <summary>The last aspects per relative section reported to the plugin. Section 0 is the current section, section 1 the upcoming section, and so on.</summary>
			internal int[] LastAspects;
			/// <summary>The absolute section the train was last.</summary>
			internal int LastSection;
			/// <summary>The last exception the plugin raised.</summary>
			internal Exception LastException;
			//NEW: Whether this plugin can disable the time acceleration factor
			/// <summary>Whether this plugin can disable time acceleration.</summary>
			internal static bool DisableTimeAcceleration;

			private List<Station> currentRouteStations;
			private bool StationsLoaded;
			// --- functions ---
			/// <summary>Called to load and initialize the plugin.</summary>
			/// <param name="specs">The train specifications.</param>
			/// <param name="mode">The initialization mode of the train.</param>
			/// <returns>Whether loading the plugin was successful.</returns>
			internal abstract bool Load(VehicleSpecs specs, InitializationModes mode);
			/// <summary>Called to unload the plugin.</summary>
			internal abstract void Unload();
			/// <summary>Called before the train jumps to a different location.</summary>
			/// <param name="mode">The initialization mode of the train.</param>
			internal abstract void BeginJump(InitializationModes mode);
			/// <summary>Called when the train has finished jumping to a different location.</summary>
			internal abstract void EndJump();
			/// <summary>Called every frame to update the plugin.</summary>
			internal void UpdatePlugin() {
				if (Train.Cars == null || Train.Cars.Length == 0)
				{
					return;
				}
				/*
				 * Prepare the vehicle state.
				 * */
				double location = this.Train.Cars[0].FrontAxle.Follower.TrackPosition - this.Train.Cars[0].FrontAxle.Position + 0.5 * this.Train.Cars[0].Length;
				//Curve Radius, Cant and Pitch Added
				double CurrentRadius = this.Train.Cars[0].FrontAxle.Follower.CurveRadius;
				double CurrentCant = this.Train.Cars[0].FrontAxle.Follower.CurveCant;
				double CurrentPitch = this.Train.Cars[0].FrontAxle.Follower.Pitch;
				//If the list of stations has not been loaded, do so
				if (!StationsLoaded)
				{
					currentRouteStations = new List<Station>();
					int s = 0;
					foreach (RouteStation selectedStation in Program.CurrentRoute.Stations)
					{
						double stopPosition = -1;
						int stopIdx = Program.CurrentRoute.Stations[s].GetStopIndex(Train.NumberOfCars);
						if (selectedStation.Stops.Length != 0)
						{
							stopPosition = selectedStation.Stops[stopIdx].TrackPosition;
						}
						Station i = new Station(selectedStation, stopPosition);
						currentRouteStations.Add(i);
						s++;
					}
					StationsLoaded = true;
					
				}
				//End of additions
				double speed = this.Train.Cars[this.Train.DriverCar].Specs.CurrentPerceivedSpeed;
				double bcPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.brakeCylinder.CurrentPressure;
				double mrPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.mainReservoir.CurrentPressure;
				double erPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.equalizingReservoir.CurrentPressure;
				double bpPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.brakePipe.CurrentPressure;
				double sapPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.straightAirPipe.CurrentPressure;
				VehicleState vehicle = new VehicleState(location, new Speed(speed), bcPressure, mrPressure, erPressure, bpPressure, sapPressure, CurrentRadius, CurrentCant, CurrentPitch);
				/*
				 * Prepare the preceding vehicle state.
				 * */
				double bestLocation = double.MaxValue;
				double bestSpeed = 0.0;
				PrecedingVehicleState precedingVehicle;
				try
				{
					for (int i = 0; i < TrainManager.Trains.Length; i++)
					{
						if (TrainManager.Trains[i] != this.Train & TrainManager.Trains[i].State == TrainState.Available & Train.Cars.Length > 0)
						{
							int c = TrainManager.Trains[i].Cars.Length - 1;
							double z = TrainManager.Trains[i].Cars[c].RearAxle.Follower.TrackPosition - TrainManager.Trains[i].Cars[c].RearAxle.Position - 0.5 * TrainManager.Trains[i].Cars[c].Length;
							if (z >= location & z < bestLocation)
							{
								bestLocation = z;
								bestSpeed = TrainManager.Trains[i].CurrentSpeed;
							}
						}
					}
					precedingVehicle = bestLocation != double.MaxValue ? new PrecedingVehicleState(bestLocation, bestLocation - location, new Speed(bestSpeed)) : null;
				}
				catch
				{
					precedingVehicle = null;
				}
				/*
				 * Get the driver handles.
				 * */
				Handles handles = GetHandles();
				/*
				 * Update the plugin.
				 * */
				double totalTime = Program.CurrentRoute.SecondsSinceMidnight;
				double elapsedTime = Program.CurrentRoute.SecondsSinceMidnight - LastTime;

				ElapseData data = new ElapseData(vehicle, precedingVehicle, handles, this.Train.SafetySystems.DoorInterlockState, new Time(totalTime), new Time(elapsedTime), currentRouteStations, Program.Renderer.Camera.CurrentMode, Translations.CurrentLanguageCode, this.Train.Destination);
				ElapseData inputDevicePluginData = data;
				LastTime = Program.CurrentRoute.SecondsSinceMidnight;
				Elapse(data);
				this.PluginMessage = data.DebugMessage;
				this.Train.SafetySystems.DoorInterlockState = data.DoorInterlockState;
				DisableTimeAcceleration = data.DisableTimeAcceleration;
				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++) {
					if (InputDevicePlugin.AvailablePluginInfos[i].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable) {
						InputDevicePlugin.AvailablePlugins[i].SetElapseData(inputDevicePluginData);
					}
				}
				/*
				 * Set the virtual handles.
				 * */
				this.PluginValid = true;
				SetHandles(data.Handles, true);
				this.Train.Destination = data.Destination;
			}
			/// <summary>Gets the driver handles.</summary>
			/// <returns>The driver handles.</returns>
			private Handles GetHandles() {
				int reverser = (int)this.Train.Handles.Reverser.Driver;
				int powerNotch = this.Train.Handles.Power.Driver;
				int brakeNotch;
				if (this.Train.Handles.Brake is TrainManager.AirBrakeHandle) {
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 3 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				} else {
					if (this.Train.Handles.HasHoldBrake) {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 2 : this.Train.Handles.Brake.Driver > 0 ? this.Train.Handles.Brake.Driver + 1 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 1 : this.Train.Handles.Brake.Driver;
					}
				}
				int locoBrakeNotch = this.Train.Handles.LocoBrake.Driver;
				bool constSpeed = this.Train.Specs.CurrentConstSpeed;
				return new Handles(reverser, powerNotch, brakeNotch, locoBrakeNotch, constSpeed);
			}
			/// <summary>Sets the driver handles or the virtual handles.</summary>
			/// <param name="handles">The handles.</param>
			/// <param name="virtualHandles">Whether to set the virtual handles.</param>
			private void SetHandles(Handles handles, bool virtualHandles) {
				/*
				 * Process the handles.
				 */
				if (this.Train.Handles.SingleHandle & handles.BrakeNotch != 0) {
					handles.PowerNotch = 0;
				}
				/*
				 * Process the reverser.
				 */
				if (handles.Reverser >= -1 & handles.Reverser <= 1) {
					if (virtualHandles) {
						this.Train.Handles.Reverser.Actual = (TrainManager.ReverserPosition)handles.Reverser;
					} else {
						this.Train.ApplyReverser(handles.Reverser, false);
					}
				} else {
					if (virtualHandles) {
						this.Train.Handles.Reverser.Actual = this.Train.Handles.Reverser.Driver;
					}
					this.PluginValid = false;
				}
				/*
				 * Process the power.
				 * */
				if (handles.PowerNotch >= 0 & handles.PowerNotch <= this.Train.Handles.Power.MaximumNotch) {
					if (virtualHandles) {
						this.Train.Handles.Power.Safety = handles.PowerNotch;
					} else {
						Train.ApplyNotch(handles.PowerNotch, false, 0, true, true);
					}
				} else {
					if (virtualHandles) {
						this.Train.Handles.Power.Safety = this.Train.Handles.Power.Driver;
					}
					this.PluginValid = false;
				}
				/*
				 * Process the brakes.
				 * */
				if (virtualHandles) {
					this.Train.Handles.EmergencyBrake.Safety = false;
					this.Train.Handles.HoldBrake.Actual = false;
				}
				if (this.Train.Handles.Brake is TrainManager.AirBrakeHandle) {
					if (handles.BrakeNotch == 0) {
						if (virtualHandles) {
							this.Train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Release;
						} else {
							this.Train.UnapplyEmergencyBrake();
							this.Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 1) {
						if (virtualHandles) {
							this.Train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Lap;
						} else {
							this.Train.UnapplyEmergencyBrake();
							this.Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
						}
					} else if (handles.BrakeNotch == 2) {
						if (virtualHandles) {
							this.Train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Service;
						} else {
							this.Train.UnapplyEmergencyBrake();
							this.Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 3) {
						if (virtualHandles) {
							this.Train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Service;
							this.Train.Handles.EmergencyBrake.Safety = true;
						} else {
							this.Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
							this.Train.ApplyEmergencyBrake();
						}
					} else {
						this.PluginValid = false;
					}
				} else {
					if (this.Train.Handles.HasHoldBrake) {
						if (handles.BrakeNotch == this.Train.Handles.Brake.MaximumNotch + 2) {
							if (virtualHandles) {
								this.Train.Handles.EmergencyBrake.Safety = true;
								this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.MaximumNotch;
							} else {
								this.Train.ApplyHoldBrake(false);
								Train.ApplyNotch(0, true, this.Train.Handles.Brake.MaximumNotch, false, true);
								this.Train.ApplyEmergencyBrake();
							}
						} else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= this.Train.Handles.Brake.MaximumNotch + 1) {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = handles.BrakeNotch - 1;
							} else {
								this.Train.UnapplyEmergencyBrake();
								this.Train.ApplyHoldBrake(false);
								Train.ApplyNotch(0, true, handles.BrakeNotch - 1, false, true);
							}
						} else if (handles.BrakeNotch == 1) {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = 0;
								this.Train.Handles.HoldBrake.Actual = true;
							} else {
								this.Train.UnapplyEmergencyBrake();
								Train.ApplyNotch(0, true, 0, false, true);
								this.Train.ApplyHoldBrake(true);
							}
						} else if (handles.BrakeNotch == 0) {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = 0;
							} else {
								this.Train.UnapplyEmergencyBrake();
								Train.ApplyNotch(0, true, 0, false, true);
								this.Train.ApplyHoldBrake(false);
							}
						} else {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.Driver;
							}
							this.PluginValid = false;
						}
					} else {
						if (handles.BrakeNotch == this.Train.Handles.Brake.MaximumNotch + 1) {
							if (virtualHandles) {
								this.Train.Handles.EmergencyBrake.Safety = true;
								this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.MaximumNotch;
							} else {
								this.Train.ApplyHoldBrake(false);
								this.Train.ApplyEmergencyBrake();
							}
						} else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= this.Train.Handles.Brake.MaximumNotch | this.Train.Handles.Brake.DelayedChanges.Length == 0) {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = handles.BrakeNotch;
							} else {
								this.Train.UnapplyEmergencyBrake();
								Train.ApplyNotch(0, true, handles.BrakeNotch, false, true);
							}
						} else {
							if (virtualHandles) {
								this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.Driver;
							}
							this.PluginValid = false;
						}
					}
				}
				/*
				 * Process the const speed system.
				 * */
				this.Train.Specs.CurrentConstSpeed = handles.ConstSpeed & this.Train.Specs.HasConstSpeed;
			}
			/// <summary>Called every frame to update the plugin.</summary>
			/// <param name="data">The data passed to the plugin on Elapse.</param>
			/// <remarks>This function should not be called directly. Call UpdatePlugin instead.</remarks>
			protected abstract void Elapse(ElapseData data);
			/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occured.</summary>
			internal void UpdateReverser() {
				int reverser = (int)this.Train.Handles.Reverser.Driver;
				if (reverser != this.LastReverser) {
					this.LastReverser = reverser;
					SetReverser(reverser);
				}
			}
			/// <summary>Called to indicate a change of the reverser.</summary>
			/// <param name="reverser">The reverser.</param>
			/// <remarks>This function should not be called directly. Call UpdateReverser instead.</remarks>
			protected abstract void SetReverser(int reverser);
			/// <summary>Called to update the power notch. This invokes a call to SetPower only if a change actually occured.</summary>
			internal void UpdatePower() {
				int powerNotch = this.Train.Handles.Power.Driver;
				if (powerNotch != this.LastPowerNotch) {
					this.LastPowerNotch = powerNotch;
					SetPower(powerNotch);
				}
			}
			/// <summary>Called to indicate a change of the power notch.</summary>
			/// <param name="powerNotch">The power notch.</param>
			/// <remarks>This function should not be called directly. Call UpdatePower instead.</remarks>
			protected abstract void SetPower(int powerNotch);
			/// <summary>Called to update the brake notch. This invokes a call to SetBrake only if a change actually occured.</summary>
			internal void UpdateBrake() {
				int brakeNotch;
				if (this.Train.Handles.Brake is TrainManager.AirBrakeHandle) {
					if (this.Train.Handles.HasHoldBrake) {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 4 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 3 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 2 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 3 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
					}
				} else {
					if (this.Train.Handles.HasHoldBrake) {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 2 : this.Train.Handles.Brake.Driver > 0 ? this.Train.Handles.Brake.Driver + 1 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 1 : this.Train.Handles.Brake.Driver;
					}
				}
				if (brakeNotch != this.LastBrakeNotch) {
					this.LastBrakeNotch = brakeNotch;
					SetBrake(brakeNotch);
				}
			}
			/// <summary>Called to indicate a change of the brake notch.</summary>
			/// <param name="brakeNotch">The brake notch.</param>
			/// <remarks>This function should not be called directly. Call UpdateBrake instead.</remarks>
			protected abstract void SetBrake(int brakeNotch);
			/// <summary>Called when a virtual key is pressed.</summary>
			internal abstract void KeyDown(VirtualKeys key);
			/// <summary>Called when a virtual key is released.</summary>
			internal abstract void KeyUp(VirtualKeys key);
			/// <summary>Called when a horn is played or stopped.</summary>
			internal abstract void HornBlow(HornTypes type);
			/// <summary>Called when the state of the doors changes.</summary>
			internal abstract void DoorChange(DoorStates oldState, DoorStates newState);
			/// <summary>Called to update the aspects of the section. This invokes a call to SetSignal only if a change in aspect occured or when changing section boundaries.</summary>
			/// <param name="data">The sections to submit to the plugin.</param>
			internal void UpdateSignals(SignalData[] data) {
				if (data.Length != 0) {
					bool update;
					if (this.Train.CurrentSectionIndex != this.LastSection) {
						update = true;
					} else if (data.Length != this.LastAspects.Length) {
						update = true;
					} else {
						update = false;
						for (int i = 0; i < data.Length; i++) {
							if (data[i].Aspect != this.LastAspects[i]) {
								update = true;
								break;
							}
						}
					}
					if (update) {
						SetSignal(data);
						this.LastAspects = new int[data.Length];
						for (int i = 0; i < data.Length; i++) {
							this.LastAspects[i] = data[i].Aspect;
						}
					}
				}
			}
			/// <summary>Is called when the aspect in the current or any of the upcoming sections changes.</summary>
			/// <param name="signal">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
			/// <remarks>This function should not be called directly. Call UpdateSignal instead.</remarks>
			protected abstract void SetSignal(SignalData[] signal);
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="type">The beacon type.</param>
			/// <param name="sectionIndex">The section the beacon is attached to, or -1 for the next red signal.</param>
			/// <param name="optional">Optional data attached to the beacon.</param>
			internal void UpdateBeacon(int type, int sectionIndex, int optional) {
				if (sectionIndex == -1) {
					sectionIndex = this.Train.CurrentSectionIndex + 1;
					SignalData signal = null;
					while (sectionIndex < Program.CurrentRoute.Sections.Length) {
						signal = Program.CurrentRoute.Sections[sectionIndex].GetPluginSignal(this.Train);
						if (signal.Aspect == 0) break;
						sectionIndex++;
					}
					if (sectionIndex < Program.CurrentRoute.Sections.Length) {
						SetBeacon(new BeaconData(type, optional, signal));
					} else {
						SetBeacon(new BeaconData(type, optional, new SignalData(-1, double.MaxValue)));
					}
				}
				if (sectionIndex >= 0) {
					SignalData signal;
					if (sectionIndex < Program.CurrentRoute.Sections.Length) {
						signal = Program.CurrentRoute.Sections[sectionIndex].GetPluginSignal(this.Train);
					} else {
						signal = new SignalData(0, double.MaxValue);
					}
					SetBeacon(new BeaconData(type, optional, signal));
				} else {
					SetBeacon(new BeaconData(type, optional, new SignalData(-1, double.MaxValue)));
				}
			}
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="beacon">The beacon data.</param>
			/// <remarks>This function should not be called directly. Call UpdateBeacon instead.</remarks>
			protected abstract void SetBeacon(BeaconData beacon);
			/// <summary>Updates the AI.</summary>
			/// <returns>The AI response.</returns>
			internal AIResponse UpdateAI() {
				if (this.SupportsAI) {
					AIData data = new AIData(GetHandles());
					this.PerformAI(data);
					if (data.Response != AIResponse.None) {
						SetHandles(data.Handles, false);
					}
					return data.Response;
				} else {
					return AIResponse.None;
				}
			}
			/// <summary>Called when the AI should be performed.</summary>
			/// <param name="data">The AI data.</param>
			/// <remarks>This function should not be called directly. Call UpdateAI instead.</remarks>
			protected abstract void PerformAI(AIData data);
			
		}
		
		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		internal static bool CheckWin32Header(string file) {
			using (System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
				using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
					if (reader.ReadUInt16() != 0x5A4D) {
						/* Not MZ signature */
						return false;
					}
					stream.Position = 0x3C;
					stream.Position = reader.ReadInt32();
					if (reader.ReadUInt32() != 0x00004550) {
						/* Not PE signature */
						return false;
					}
					if (reader.ReadUInt16() != 0x014C) {
						/* Not IMAGE_FILE_MACHINE_I386 */
						return false;
					}
				}
			}
			return true;
		}
		
		
		
	}
}
