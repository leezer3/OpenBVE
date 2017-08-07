﻿using System;
using System.Collections.Generic;
using System.Reflection;
using OpenBveApi.Runtime;

namespace OpenBve {
	internal static partial class PluginManager {
		
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
			/// <summary>The current camera view mode</summary>
			internal OpenBveApi.Runtime.CameraViewMode CurrentCameraViewMode;

			private List<Station> currentRouteStations;
			internal bool StationsLoaded;
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
					foreach (Game.Station selectedStation in Game.Stations)
					{
						Station i = new Station
						{
							Name = selectedStation.Name,
							ArrivalTime = selectedStation.ArrivalTime,
							DepartureTime = selectedStation.DepartureTime,
							StopTime = selectedStation.StopTime,
							OpenLeftDoors = selectedStation.OpenLeftDoors,
							OpenRightDoors = selectedStation.OpenRightDoors,
							ForceStopSignal = selectedStation.ForceStopSignal,
							DefaultTrackPosition = selectedStation.DefaultTrackPosition
						};
						currentRouteStations.Add(i);
					}
					StationsLoaded = true;
				}
				//End of additions
				double speed = this.Train.Cars[this.Train.DriverCar].Specs.CurrentPerceivedSpeed;
				double bcPressure = this.Train.Cars[this.Train.DriverCar].AirBrake.BrakeCylinder.CurrentPressure;
				double mrPressure = this.Train.Cars[this.Train.DriverCar].AirBrake.MainReservoir.CurrentPressure;
				double erPressure = this.Train.Cars[this.Train.DriverCar].AirBrake.EqualizingReservoir.CurrentPressure;
				double bpPressure = this.Train.Cars[this.Train.DriverCar].AirBrake.BrakePipe.CurrentPressure;
				double sapPressure = this.Train.Cars[this.Train.DriverCar].AirBrake.StraightAirPipe.CurrentPressure;
				VehicleState vehicle = new VehicleState(location, new Speed(speed), bcPressure, mrPressure, erPressure, bpPressure, sapPressure, CurrentRadius, CurrentCant, CurrentPitch);
				/*
				 * Prepare the preceding vehicle state.
				 * */
				double bestLocation = double.MaxValue;
				double bestSpeed = 0.0;
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != this.Train & TrainManager.Trains[i].State == TrainManager.TrainState.Available) {
						int c = TrainManager.Trains[i].Cars.Length - 1;
						double z = TrainManager.Trains[i].Cars[c].RearAxle.Follower.TrackPosition - TrainManager.Trains[i].Cars[c].RearAxle.Position - 0.5 * TrainManager.Trains[i].Cars[c].Length;
						if (z >= location & z < bestLocation) {
							bestLocation = z;
							bestSpeed = TrainManager.Trains[i].Specs.CurrentAverageSpeed;
						}
					}
				}
				var precedingVehicle = bestLocation != double.MaxValue ? new PrecedingVehicleState(bestLocation, bestLocation - location, new Speed(bestSpeed)) : null;
				/*
				 * Get the driver handles.
				 * */
				Handles handles = GetHandles();
				/*
				 * Update the plugin.
				 * */
				double totalTime = Game.SecondsSinceMidnight;
				double elapsedTime = Game.SecondsSinceMidnight - LastTime;
				/* 
				 * Set the current camera view mode
				 * Could probably do away with the CurrentCameraViewMode and use a direct cast??
				 * 
				 */
				CurrentCameraViewMode = (OpenBveApi.Runtime.CameraViewMode)World.CameraMode;
				ElapseData data = new ElapseData(vehicle, precedingVehicle, handles, (DoorInterlockStates)this.Train.Specs.DoorInterlockState, new Time(totalTime), new Time(elapsedTime), currentRouteStations, CurrentCameraViewMode, Interface.CurrentLanguageCode);
				LastTime = Game.SecondsSinceMidnight;
				Elapse(data);
				this.PluginMessage = data.DebugMessage;
				this.Train.Specs.DoorInterlockState = (TrainManager.DoorInterlockStates)data.DoorInterlockState;
				DisableTimeAcceleration = data.DisableTimeAcceleration;
				/*
				 * Set the virtual handles.
				 * */
				this.PluginValid = true;
				SetHandles(data.Handles, true);
			}
			/// <summary>Gets the driver handles.</summary>
			/// <returns>The driver handles.</returns>
			private Handles GetHandles() {
				int reverser = this.Train.Specs.CurrentReverser.Driver;
				int powerNotch = this.Train.Specs.CurrentPowerNotch.Driver;
				int brakeNotch;
				if (this.Train.Cars[this.Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					brakeNotch = this.Train.EmergencyBrake.DriverApplied ? 3 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? this.Train.Specs.MaximumBrakeNotch + 2 : this.Train.Specs.CurrentBrakeNotch.Driver > 0 ? this.Train.Specs.CurrentBrakeNotch.Driver + 1 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? this.Train.Specs.MaximumBrakeNotch + 1 : this.Train.Specs.CurrentBrakeNotch.Driver;
					}
				}
				bool constSpeed = this.Train.Specs.CurrentConstSpeed;
				return new Handles(reverser, powerNotch, brakeNotch, constSpeed);
			}
			/// <summary>Sets the driver handles or the virtual handles.</summary>
			/// <param name="handles">The handles.</param>
			/// <param name="virtualHandles">Whether to set the virtual handles.</param>
			private void SetHandles(Handles handles, bool virtualHandles) {
				/*
				 * Process the handles.
				 */
				if (this.Train.Specs.SingleHandle & handles.BrakeNotch != 0) {
					handles.PowerNotch = 0;
				}
				/*
				 * Process the reverser.
				 */
				if (handles.Reverser >= -1 & handles.Reverser <= 1) {
					if (virtualHandles) {
						this.Train.Specs.CurrentReverser.Actual = handles.Reverser;
					} else {
						TrainManager.ApplyReverser(this.Train, handles.Reverser, false);
					}
				} else {
					if (virtualHandles) {
						this.Train.Specs.CurrentReverser.Actual = this.Train.Specs.CurrentReverser.Driver;
					}
					this.PluginValid = false;
				}
				/*
				 * Process the power.
				 * */
				if (handles.PowerNotch >= 0 & handles.PowerNotch <= this.Train.Specs.MaximumPowerNotch) {
					if (virtualHandles) {
						this.Train.Specs.CurrentPowerNotch.Safety = handles.PowerNotch;
					} else {
						Train.ApplyNotch(handles.PowerNotch, false, 0, true);
					}
				} else {
					if (virtualHandles) {
						this.Train.Specs.CurrentPowerNotch.Safety = this.Train.Specs.CurrentPowerNotch.Driver;
					}
					this.PluginValid = false;
				}

				/*
				 * Process the brakes.
				 * */
				if (virtualHandles) {
					this.Train.EmergencyBrake.SafetySystemApplied = false;
					this.Train.Specs.CurrentHoldBrake.Actual = false;
				}
				if (this.Train.Cars[this.Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (handles.BrakeNotch == 0) {
						if (virtualHandles) {
							this.Train.Specs.CurrentAirBrakeHandle.Safety = TrainManager.AirBrakeHandleState.Release;
						} else {
							Train.EmergencyBrake.Release();
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 1) {
						if (virtualHandles) {
							this.Train.Specs.CurrentAirBrakeHandle.Safety = TrainManager.AirBrakeHandleState.Lap;
						} else {
							Train.EmergencyBrake.Release();
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Lap);
						}
					} else if (handles.BrakeNotch == 2) {
						if (virtualHandles) {
							this.Train.Specs.CurrentAirBrakeHandle.Safety = TrainManager.AirBrakeHandleState.Service;
						} else {
							Train.EmergencyBrake.Release();
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 3) {
						if (virtualHandles) {
							this.Train.Specs.CurrentAirBrakeHandle.Safety = TrainManager.AirBrakeHandleState.Service;
						    this.Train.EmergencyBrake.SafetySystemApplied = true;
						} else {
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Service);
							Train.EmergencyBrake.Apply();
						}
					} else {
						this.PluginValid = false;
					}
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						if (handles.BrakeNotch == this.Train.Specs.MaximumBrakeNotch + 2) {
							if (virtualHandles)
							{
							    this.Train.EmergencyBrake.SafetySystemApplied = true;
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.MaximumBrakeNotch;
							} else {
								TrainManager.ApplyHoldBrake(this.Train, false);
								Train.ApplyNotch(0, true, this.Train.Specs.MaximumBrakeNotch, false);
								Train.EmergencyBrake.Apply();
							}
						} else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= this.Train.Specs.MaximumBrakeNotch + 1) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch - 1;
							} else {
								Train.EmergencyBrake.Release();
								TrainManager.ApplyHoldBrake(this.Train, false);
								Train.ApplyNotch(0, true, handles.BrakeNotch - 1, false);
							}
						} else if (handles.BrakeNotch == 1) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = 0;
								this.Train.Specs.CurrentHoldBrake.Actual = true;
							} else {
								Train.EmergencyBrake.Release();
								Train.ApplyNotch(0, true, 0, false);
								TrainManager.ApplyHoldBrake(this.Train, true);
							}
						} else if (handles.BrakeNotch == 0) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = 0;
							} else {
								Train.EmergencyBrake.Release();
								Train.ApplyNotch(0, true, 0, false);
								TrainManager.ApplyHoldBrake(this.Train, false);
							}
						} else {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.CurrentBrakeNotch.Driver;
							}
							this.PluginValid = false;
						}
					} else {
						if (handles.BrakeNotch == this.Train.Specs.MaximumBrakeNotch + 1) {
							if (virtualHandles)
							{
							    Train.EmergencyBrake.SafetySystemApplied = true;
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.MaximumBrakeNotch;
							} else {
								TrainManager.ApplyHoldBrake(this.Train, false);
								Train.EmergencyBrake.Apply();
							}
						} else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= this.Train.Specs.MaximumBrakeNotch | this.Train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch;
							} else {
								Train.EmergencyBrake.Release();
								Train.ApplyNotch(0, true, handles.BrakeNotch, false);
							}
						} else {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.CurrentBrakeNotch.Driver;
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
			internal abstract void Elapse(ElapseData data);
			/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occured.</summary>
			internal void UpdateReverser() {
				int reverser = this.Train.Specs.CurrentReverser.Driver;
				if (reverser != this.LastReverser) {
					this.LastReverser = reverser;
					SetReverser(reverser);
				}
			}
			/// <summary>Called to indicate a change of the reverser.</summary>
			/// <param name="reverser">The reverser.</param>
			/// <remarks>This function should not be called directly. Call UpdateReverser instead.</remarks>
			internal abstract void SetReverser(int reverser);
			/// <summary>Called to update the power notch. This invokes a call to SetPower only if a change actually occured.</summary>
			internal void UpdatePower() {
				int powerNotch = this.Train.Specs.CurrentPowerNotch.Driver;
				if (powerNotch != this.LastPowerNotch) {
					this.LastPowerNotch = powerNotch;
					SetPower(powerNotch);
				}
			}
			/// <summary>Called to indicate a change of the power notch.</summary>
			/// <param name="powerNotch">The power notch.</param>
			/// <remarks>This function should not be called directly. Call UpdatePower instead.</remarks>
			internal abstract void SetPower(int powerNotch);
			/// <summary>Called to update the brake notch. This invokes a call to SetBrake only if a change actually occured.</summary>
			internal void UpdateBrake() {
				int brakeNotch;
				if (this.Train.Cars[this.Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? 4 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Service ? 3 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Lap ? 2 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? 3 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Specs.CurrentAirBrakeHandle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
					}
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? this.Train.Specs.MaximumBrakeNotch + 2 : this.Train.Specs.CurrentBrakeNotch.Driver > 0 ? this.Train.Specs.CurrentBrakeNotch.Driver + 1 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.EmergencyBrake.DriverApplied ? this.Train.Specs.MaximumBrakeNotch + 1 : this.Train.Specs.CurrentBrakeNotch.Driver;
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
			internal abstract void SetBrake(int brakeNotch);
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
			internal abstract void SetSignal(SignalData[] signal);
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="type">The beacon type.</param>
			/// <param name="sectionIndex">The section the beacon is attached to, or -1 for the next red signal.</param>
			/// <param name="optional">Optional data attached to the beacon.</param>
			internal void UpdateBeacon(int type, int sectionIndex, int optional) {
				if (sectionIndex == -1) {
					sectionIndex = this.Train.CurrentSectionIndex + 1;
					SignalData signal = null;
					while (sectionIndex < Game.Sections.Length) {
						signal = Game.GetPluginSignal(this.Train, sectionIndex);
						if (signal.Aspect == 0) break;
						sectionIndex++;
					}
					if (sectionIndex < Game.Sections.Length) {
						SetBeacon(new BeaconData(type, optional, signal));
					} else {
						SetBeacon(new BeaconData(type, optional, new SignalData(-1, double.MaxValue)));
					}
				}
				if (sectionIndex >= 0) {
					SignalData signal;
					if (sectionIndex < Game.Sections.Length) {
						signal = Game.GetPluginSignal(this.Train, sectionIndex);
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
			internal abstract void SetBeacon(BeaconData beacon);
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
			internal abstract void PerformAI(AIData data);
			
		}
		
		/// <summary>Loads a custom plugin for the specified train.</summary>
		/// <param name="train">The train to attach the plugin to.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="encoding">The encoding to be used.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		internal static bool LoadCustomPlugin(TrainManager.Train train, string trainFolder, System.Text.Encoding encoding) {
			string config = OpenBveApi.Path.CombineFile(trainFolder, "ats.cfg");
			if (!System.IO.File.Exists(config)) {
				return false;
			}
			string[] lines = System.IO.File.ReadAllLines(config, encoding);
			if (lines.Length == 0) {
				return false;
			}
			string file = OpenBveApi.Path.CombineFile(trainFolder, lines[0]);
			string title = System.IO.Path.GetFileName(file);
			if (!System.IO.File.Exists(file)) {
				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + title + " could not be found in " + config);
				return false;
			}
			Program.AppendToLogFile("Loading train plugin: " + file);
			bool success = LoadPlugin(train, file, trainFolder);
			if (success == false)
			{
				Loading.PluginError = Interface.GetInterfaceString("errors_plugin_failure1").Replace("[plugin]", file);
			}
			else
			{
				Program.AppendToLogFile("Train plugin loaded successfully.");
			}
			return success;
		}
		
		/// <summary>Loads the default plugin for the specified train.</summary>
		/// <param name="train">The train to attach the plugin to.</param>
		/// <param name="trainFolder">The train folder.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		internal static bool LoadDefaultPlugin(TrainManager.Train train, string trainFolder) {
			string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Plugins"), "OpenBveAts.dll");
			bool success = LoadPlugin(train, file, trainFolder);
			if (success) {
				train.Plugin.IsDefault = true;
				SoundCfgParser.LoadDefaultATSSounds(train, trainFolder);
			}
			return success;
		}
		
		/// <summary>Loads the specified plugin for the specified train.</summary>
		/// <param name="train">The train to attach the plugin to.</param>
		/// <param name="pluginFile">The file to the plugin.</param>
		/// <param name="trainFolder">The train folder.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		private static bool LoadPlugin(TrainManager.Train train, string pluginFile, string trainFolder) {
			string pluginTitle = System.IO.Path.GetFileName(pluginFile);
			if (!System.IO.File.Exists(pluginFile)) {
				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " could not be found.");
				return false;
			}
			/*
			 * Unload plugin if already loaded.
			 * */
			if (train.Plugin != null) {
				UnloadPlugin(train);
			}
			/*
			 * Prepare initialization data for the plugin.
			 * */
			BrakeTypes brakeType = (BrakeTypes)train.Cars[train.DriverCar].BrakeType;
			int brakeNotches;
			int powerNotches;
			bool hasHoldBrake;
			if (brakeType == BrakeTypes.AutomaticAirBrake) {
				brakeNotches = 2;
				powerNotches = train.Specs.MaximumPowerNotch;
				hasHoldBrake = false;
			} else {
				brakeNotches = train.Specs.MaximumBrakeNotch + (train.Specs.HasHoldBrake ? 1 : 0);
				powerNotches = train.Specs.MaximumPowerNotch;
				hasHoldBrake = train.Specs.HasHoldBrake;
			}
			int cars = train.Cars.Length;
			VehicleSpecs specs = new VehicleSpecs(powerNotches, brakeType, brakeNotches, hasHoldBrake, cars);
			InitializationModes mode = (InitializationModes)Game.TrainStart;
			/*
			 * Check if the plugin is a .NET plugin.
			 * */
			Assembly assembly;
			try {
				assembly = Assembly.LoadFile(pluginFile);
			} catch (BadImageFormatException) {
				assembly = null;
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " could not be loaded due to the following exception: " + ex.Message);
				return false;
			}
			if (assembly != null) {
				Type[] types;
				try {
					types = assembly.GetTypes();
				} catch (ReflectionTypeLoadException ex) {
					foreach (Exception e in ex.LoaderExceptions) {
						Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " raised an exception on loading: " + e.Message);
					}
					return false;
				}
				foreach (Type type in types) {
					if (typeof(IRuntime).IsAssignableFrom(type)) {
						IRuntime api = assembly.CreateInstance(type.FullName) as IRuntime;
						train.Plugin = new NetPlugin(pluginFile, trainFolder, api, train);
						if (train.Plugin.Load(specs, mode)) {
							return true;
						} else {
							train.Plugin = null;
							return false;
						}
					}
				}
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
				return false;
			}
			/*
			 * Check if the plugin is a Win32 plugin.
			 * 
			 */
			try {
				if (!CheckWin32Header(pluginFile)) {
					Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " is of an unsupported binary format and therefore cannot be used with openBVE.");
					return false;
				}
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " could not be read due to the following reason: " + ex.Message);
				return false;
			}
			if (!Program.CurrentlyRunningOnWindows | IntPtr.Size != 4) {
				Interface.AddMessage(Interface.MessageType.Warning, false, "The train plugin " + pluginTitle + " can only be used on 32-bit Microsoft Windows or compatible.");
				return false;
			}
			if (Program.CurrentlyRunningOnWindows && !System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AtsPluginProxy.dll"))
			{
				Interface.AddMessage(Interface.MessageType.Warning, false, "AtsPluginProxy.dll is missing or corrupt- Please reinstall.");
				return false;
			}
			train.Plugin = new Win32Plugin(pluginFile, train);
			if (train.Plugin.Load(specs, mode)) {
				return true;
			} else {
				train.Plugin = null;
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
				return false;
			}
		}
		
		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		private static bool CheckWin32Header(string file) {
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
		
		/// <summary>Unloads the currently loaded plugin, if any.</summary>
		/// <param name="train">Unloads the plugin for the specified train.</param>
		internal static void UnloadPlugin(TrainManager.Train train) {
			if (train.Plugin != null) {
				train.Plugin.Unload();
				train.Plugin = null;
			}
		}
		
	}
}