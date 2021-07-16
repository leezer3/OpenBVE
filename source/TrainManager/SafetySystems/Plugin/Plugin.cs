using System;
using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Stations;
using TrainManager.Handles;
using TrainManager.Trains;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents an abstract plugin.</summary>
	public abstract class Plugin
	{
		// --- members ---
		/// <summary>The file title of the plugin, including the file extension.</summary>
		public string PluginTitle;

		/// <summary>Whether the plugin is the default ATS/ATC plugin.</summary>
		public bool IsDefault;

		/// <summary>Whether the plugin returned valid information in the last Elapse call.</summary>
		public bool PluginValid;

		/// <summary>The debug message the plugin returned in the last Elapse call.</summary>
		public string PluginMessage;

		/// <summary>The train the plugin is attached to.</summary>
		public TrainBase Train;

		/// <summary>The array of panel variables.</summary>
		public int[] Panel;

		/// <summary>The array of sound variables used by legacy plugins</summary>
		internal int[] Sound;

		/// <summary>Whether the plugin supports the AI.</summary>
		public AISupport SupportsAI;

		/// <summary>The last in-game time reported to the plugin.</summary>
		public double LastTime;

		/// <summary>The last reverser reported to the plugin.</summary>
		public int LastReverser;

		/// <summary>The last power notch reported to the plugin.</summary>
		public int LastPowerNotch;

		/// <summary>The last brake notch reported to the plugin.</summary>
		public int LastBrakeNotch;

		/// <summary>The last aspects per relative section reported to the plugin. Section 0 is the current section, section 1 the upcoming section, and so on.</summary>
		public int[] LastAspects;

		/// <summary>The absolute section the train was last.</summary>
		public int LastSection;

		/// <summary>The last exception the plugin raised.</summary>
		public Exception LastException;

		//NEW: Whether this plugin can disable the time acceleration factor
		/// <summary>Whether this plugin can disable time acceleration.</summary>
		public bool DisableTimeAcceleration;

		private List<Station> currentRouteStations;

		private bool StationsLoaded;
		/// <summary>Holds the plugin specific AI class</summary>
		internal PluginAI AI;

		// --- functions ---
		/// <summary>Called to load and initialize the plugin.</summary>
		/// <param name="specs">The train specifications.</param>
		/// <param name="mode">The initialization mode of the train.</param>
		/// <returns>Whether loading the plugin was successful.</returns>
		public abstract bool Load(VehicleSpecs specs, InitializationModes mode);

		/// <summary>Called to unload the plugin.</summary>
		public abstract void Unload();

		/// <summary>Called before the train jumps to a different location.</summary>
		/// <param name="mode">The initialization mode of the train.</param>
		public abstract void BeginJump(InitializationModes mode);

		/// <summary>Called when the train has finished jumping to a different location.</summary>
		public abstract void EndJump();

		/// <summary>Called every frame to update the plugin.</summary>
		public void UpdatePlugin()
		{
			if (Train.Cars == null || Train.Cars.Length == 0)
			{
				return;
			}

			/*
			 * Prepare the vehicle state.
			 * */
			double location = this.Train.Cars[0].FrontAxle.Follower.TrackPosition - this.Train.Cars[0].FrontAxle.Position + 0.5 * this.Train.Cars[0].Length;
			//If the list of stations has not been loaded, do so
			if (!StationsLoaded)
			{
				currentRouteStations = new List<Station>();
				int s = 0;
				foreach (RouteStation selectedStation in TrainManagerBase.CurrentRoute.Stations)
				{
					double stopPosition = -1;
					int stopIdx = TrainManagerBase.CurrentRoute.Stations[s].GetStopIndex(Train.NumberOfCars);
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
			double speed = this.Train.Cars[this.Train.DriverCar].Specs.PerceivedSpeed;
			double bcPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.brakeCylinder.CurrentPressure;
			double mrPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.mainReservoir.CurrentPressure;
			double erPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.equalizingReservoir.CurrentPressure;
			double bpPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.brakePipe.CurrentPressure;
			double sapPressure = this.Train.Cars[this.Train.DriverCar].CarBrake.straightAirPipe.CurrentPressure;
			VehicleState vehicle = new VehicleState(location, new Speed(speed), bcPressure, mrPressure, erPressure, bpPressure, sapPressure, this.Train.Cars[0].FrontAxle.Follower);
			/*
			 * Prepare the preceding vehicle state.
			 * */
			

			PrecedingVehicleState precedingVehicle;
			try
			{
				AbstractTrain closestTrain = TrainManagerBase.currentHost.ClosestTrain(this.Train);
				precedingVehicle = closestTrain != null ? new PrecedingVehicleState(closestTrain.RearCarTrackPosition(), closestTrain.RearCarTrackPosition() - location, new Speed(closestTrain.CurrentSpeed)) : new PrecedingVehicleState(Double.MaxValue, Double.MaxValue - location, new Speed(0.0));
			}
			catch
			{
				precedingVehicle = null;
			}

			/*
			 * Get the driver handles.
			 * */
			OpenBveApi.Runtime.Handles handles = GetHandles();
			/*
			 * Update the plugin.
			 * */
			double totalTime = TrainManagerBase.currentHost.InGameTime;
			double elapsedTime = TrainManagerBase.currentHost.InGameTime - LastTime;

			ElapseData data = new ElapseData(vehicle, precedingVehicle, handles, this.Train.SafetySystems.DoorInterlockState, new Time(totalTime), new Time(elapsedTime), currentRouteStations, TrainManagerBase.Renderer.Camera.CurrentMode, Translations.CurrentLanguageCode, this.Train.Destination);
			ElapseData inputDevicePluginData = data;
			LastTime = TrainManagerBase.currentHost.InGameTime;
			Elapse(ref data);
			this.PluginMessage = data.DebugMessage;
			this.Train.SafetySystems.DoorInterlockState = data.DoorInterlockState;
			DisableTimeAcceleration = data.DisableTimeAcceleration;
			for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
			{
				if (InputDevicePlugin.AvailablePluginInfos[i].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable)
				{
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
		private OpenBveApi.Runtime.Handles GetHandles()
		{
			int reverser = (int) this.Train.Handles.Reverser.Driver;
			int powerNotch = this.Train.Handles.Power.Driver;
			int brakeNotch;
			if (this.Train.Handles.Brake is AirBrakeHandle)
			{
				brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 3 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Service ? 2 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Lap ? 1 : 0;
			}
			else
			{
				if (this.Train.Handles.HasHoldBrake)
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 2 : this.Train.Handles.Brake.Driver > 0 ? this.Train.Handles.Brake.Driver + 1 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 1 : this.Train.Handles.Brake.Driver;
				}
			}

			return new OpenBveApi.Runtime.Handles(reverser, powerNotch, brakeNotch, this.Train.Handles.LocoBrake.Driver, this.Train.Specs.CurrentConstSpeed, this.Train.Handles.HoldBrake.Driver);
		}

		/// <summary>Sets the driver handles or the virtual handles.</summary>
		/// <param name="handles">The handles.</param>
		/// <param name="virtualHandles">Whether to set the virtual handles.</param>
		private void SetHandles(OpenBveApi.Runtime.Handles handles, bool virtualHandles)
		{
			/*
			 * Process the handles.
			 */
			if (this.Train.Handles.HandleType == HandleType.SingleHandle & handles.BrakeNotch != 0)
			{
				handles.PowerNotch = 0;
			}

			/*
			 * Process the reverser.
			 */
			if (handles.Reverser >= -1 & handles.Reverser <= 1)
			{
				if (virtualHandles)
				{
					this.Train.Handles.Reverser.Actual = (ReverserPosition) handles.Reverser;
				}
				else
				{
					this.Train.Handles.Reverser.ApplyState((ReverserPosition)handles.Reverser);
				}
			}
			else
			{
				if (virtualHandles)
				{
					this.Train.Handles.Reverser.Actual = this.Train.Handles.Reverser.Driver;
				}

				this.PluginValid = false;
			}

			/*
			 * Process the power.
			 * */
			if (handles.PowerNotch >= 0 & handles.PowerNotch <= this.Train.Handles.Power.MaximumNotch)
			{
				if (virtualHandles)
				{
					this.Train.Handles.Power.Safety = handles.PowerNotch;
				}
				else
				{
					Train.Handles.Power.ApplyState(handles.PowerNotch, false, true);
				}
			}
			else
			{
				if (virtualHandles)
				{
					this.Train.Handles.Power.Safety = this.Train.Handles.Power.Driver;
				}

				this.PluginValid = false;
			}

			/*
			 * Process the brakes.
			 * */
			if (virtualHandles)
			{
				this.Train.Handles.EmergencyBrake.Safety = false;
				this.Train.Handles.HoldBrake.Actual = false;
			}

			if (this.Train.Handles.Brake is AirBrakeHandle)
			{
				if (handles.BrakeNotch == 0)
				{
					if (virtualHandles)
					{
						this.Train.Handles.Brake.Safety = (int) AirBrakeHandleState.Release;
					}
					else
					{
						this.Train.Handles.EmergencyBrake.Release();
						Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
					}
				}
				else if (handles.BrakeNotch == 1)
				{
					if (virtualHandles)
					{
						this.Train.Handles.Brake.Safety = (int) AirBrakeHandleState.Lap;
					}
					else
					{
						this.Train.Handles.EmergencyBrake.Release();
						Train.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
					}
				}
				else if (handles.BrakeNotch == 2)
				{
					if (virtualHandles)
					{
						this.Train.Handles.Brake.Safety = (int) AirBrakeHandleState.Service;
					}
					else
					{
						this.Train.Handles.EmergencyBrake.Release();
						Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
					}
				}
				else if (handles.BrakeNotch == 3)
				{
					if (virtualHandles)
					{
						this.Train.Handles.Brake.Safety = (int) AirBrakeHandleState.Service;
						this.Train.Handles.EmergencyBrake.Safety = true;
					}
					else
					{
						Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
						this.Train.Handles.EmergencyBrake.Apply();
					}
				}
				else
				{
					this.PluginValid = false;
				}
			}
			else
			{
				if (this.Train.Handles.HasHoldBrake)
				{
					if (handles.BrakeNotch == this.Train.Handles.Brake.MaximumNotch + 2)
					{
						if (virtualHandles)
						{
							this.Train.Handles.EmergencyBrake.Safety = true;
							this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.MaximumNotch;
						}
						else
						{
							this.Train.Handles.HoldBrake.ApplyState(false);
							this.Train.Handles.Brake.ApplyState(this.Train.Handles.Brake.MaximumNotch, false, true);
							this.Train.Handles.EmergencyBrake.Apply();
						}
					}
					else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= this.Train.Handles.Brake.MaximumNotch + 1)
					{
						if (virtualHandles)
						{
							this.Train.Handles.Brake.Safety = handles.BrakeNotch - 1;
						}
						else
						{
							this.Train.Handles.EmergencyBrake.Release();
							this.Train.Handles.HoldBrake.ApplyState(false);
							this.Train.Handles.Brake.ApplyState(handles.BrakeNotch - 1, false, true);
						}
					}
					else if (handles.BrakeNotch == 1)
					{
						if (virtualHandles)
						{
							this.Train.Handles.Brake.Safety = 0;
							this.Train.Handles.HoldBrake.Actual = true;
						}
						else
						{
							this.Train.Handles.EmergencyBrake.Release();
							this.Train.Handles.Brake.ApplyState(0, false, true);
							this.Train.Handles.HoldBrake.ApplyState(true);
						}
					}
					else if (handles.BrakeNotch == 0)
					{
						if (virtualHandles)
						{
							this.Train.Handles.Brake.Safety = 0;
						}
						else
						{
							this.Train.Handles.EmergencyBrake.Release();
							this.Train.Handles.Brake.ApplyState(0, false, true);
							this.Train.Handles.HoldBrake.ApplyState(false);
						}
					}
					else
					{
						if (virtualHandles)
						{
							this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.Driver;
						}

						this.PluginValid = false;
					}
				}
				else
				{
					if (handles.BrakeNotch == this.Train.Handles.Brake.MaximumNotch + 1)
					{
						if (virtualHandles)
						{
							this.Train.Handles.EmergencyBrake.Safety = true;
							this.Train.Handles.Brake.Safety = this.Train.Handles.Brake.MaximumNotch;
						}
						else
						{
							this.Train.Handles.HoldBrake.ApplyState(false);
							this.Train.Handles.EmergencyBrake.Apply();
						}
					}
					else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= this.Train.Handles.Brake.MaximumNotch | this.Train.Handles.Brake.DelayedChanges.Length == 0)
					{
						if (virtualHandles)
						{
							this.Train.Handles.Brake.Safety = handles.BrakeNotch;
						}
						else
						{
							this.Train.Handles.EmergencyBrake.Release();
							this.Train.Handles.Brake.ApplyState(handles.BrakeNotch, false, true);
						}
					}
					else
					{
						if (virtualHandles)
						{
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
			this.Train.Handles.HoldBrake.Actual = handles.HoldBrake & this.Train.Handles.HasHoldBrake;
		}

		/// <summary>Called every frame to update the plugin.</summary>
		/// <param name="data">The data passed to the plugin on Elapse.</param>
		/// <remarks>This function should not be called directly. Call UpdatePlugin instead.</remarks>
		protected abstract void Elapse(ref ElapseData data);

		/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occured.</summary>
		public void UpdateReverser()
		{
			int reverser = (int) this.Train.Handles.Reverser.Driver;
			if (reverser != this.LastReverser)
			{
				this.LastReverser = reverser;
				SetReverser(reverser);
			}
		}

		/// <summary>Called to indicate a change of the reverser.</summary>
		/// <param name="reverser">The reverser.</param>
		/// <remarks>This function should not be called directly. Call UpdateReverser instead.</remarks>
		protected abstract void SetReverser(int reverser);

		/// <summary>Called to update the power notch. This invokes a call to SetPower only if a change actually occured.</summary>
		public void UpdatePower()
		{
			int powerNotch = this.Train.Handles.Power.Driver;
			if (powerNotch != this.LastPowerNotch)
			{
				this.LastPowerNotch = powerNotch;
				SetPower(powerNotch);
			}
		}

		/// <summary>Called to indicate a change of the power notch.</summary>
		/// <param name="powerNotch">The power notch.</param>
		/// <remarks>This function should not be called directly. Call UpdatePower instead.</remarks>
		protected abstract void SetPower(int powerNotch);

		/// <summary>Called to update the brake notch. This invokes a call to SetBrake only if a change actually occured.</summary>
		public void UpdateBrake()
		{
			int brakeNotch;
			if (this.Train.Handles.Brake is AirBrakeHandle)
			{
				if (this.Train.Handles.HasHoldBrake)
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 4 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Service ? 3 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Lap ? 2 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? 3 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Service ? 2 : this.Train.Handles.Brake.Driver == (int) AirBrakeHandleState.Lap ? 1 : 0;
				}
			}
			else
			{
				if (this.Train.Handles.HasHoldBrake)
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 2 : this.Train.Handles.Brake.Driver > 0 ? this.Train.Handles.Brake.Driver + 1 : this.Train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = this.Train.Handles.EmergencyBrake.Driver ? this.Train.Handles.Brake.MaximumNotch + 1 : this.Train.Handles.Brake.Driver;
				}
			}

			if (brakeNotch != this.LastBrakeNotch)
			{
				this.LastBrakeNotch = brakeNotch;
				SetBrake(brakeNotch);
			}
		}

		/// <summary>Called to indicate a change of the brake notch.</summary>
		/// <param name="brakeNotch">The brake notch.</param>
		/// <remarks>This function should not be called directly. Call UpdateBrake instead.</remarks>
		protected abstract void SetBrake(int brakeNotch);

		/// <summary>Called when a virtual key is pressed.</summary>
		public abstract void KeyDown(VirtualKeys key);

		/// <summary>Called when a virtual key is released.</summary>
		public abstract void KeyUp(VirtualKeys key);

		/// <summary>Called when a horn is played or stopped.</summary>
		public abstract void HornBlow(HornTypes type);

		/// <summary>Called when the state of the doors changes.</summary>
		public abstract void DoorChange(DoorStates oldState, DoorStates newState);

		/// <summary>Called to update the aspects of the section. This invokes a call to SetSignal only if a change in aspect occured or when changing section boundaries.</summary>
		/// <param name="data">The sections to submit to the plugin.</param>
		public void UpdateSignals(SignalData[] data)
		{
			if (data.Length != 0)
			{
				bool update;
				if (this.Train.CurrentSectionIndex != this.LastSection)
				{
					update = true;
				}
				else if (data.Length != this.LastAspects.Length)
				{
					update = true;
				}
				else
				{
					update = false;
					for (int i = 0; i < data.Length; i++)
					{
						if (data[i].Aspect != this.LastAspects[i])
						{
							update = true;
							break;
						}
					}
				}

				if (update)
				{
					SetSignal(data);
					this.LastAspects = new int[data.Length];
					for (int i = 0; i < data.Length; i++)
					{
						this.LastAspects[i] = data[i].Aspect;
					}
					for (int j = 0; j < InputDevicePlugin.AvailablePluginInfos.Count; j++)
					{
						if (InputDevicePlugin.AvailablePluginInfos[j].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable && InputDevicePlugin.AvailablePlugins[j] is ITrainInputDevice)
						{
							ITrainInputDevice trainInputDevice = (ITrainInputDevice)InputDevicePlugin.AvailablePlugins[j];
							trainInputDevice.SetSignal(data);
						}
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
		public void UpdateBeacon(int type, int sectionIndex, int optional)
		{

			if (type == 21 && Train.IsPlayerTrain && Train.Cars[Train.DriverCar].Windscreen != null)
			{
				//Legacy rain beacon, so let's pass to the windscreen as well as the plugin
				Train.Cars[Train.DriverCar].Windscreen.SetRainIntensity(optional);
			}

			SignalData signal;
			if (sectionIndex == -1)
			{
				sectionIndex = this.Train.CurrentSectionIndex + 1;
				while (sectionIndex < TrainManagerBase.CurrentRoute.Sections.Length)
				{
					signal = TrainManagerBase.CurrentRoute.Sections[sectionIndex].GetPluginSignal(this.Train);
					if (signal.Aspect == 0) break;
					sectionIndex++;
				}

				if (sectionIndex >= TrainManagerBase.CurrentRoute.Sections.Length)
				{
					signal = new SignalData(-1, double.MaxValue);
				}
			}

			if (sectionIndex >= 0)
			{
				signal = sectionIndex < TrainManagerBase.CurrentRoute.Sections.Length ? TrainManagerBase.CurrentRoute.Sections[sectionIndex].GetPluginSignal(this.Train) : new SignalData(0, double.MaxValue);
			}
			else
			{
				signal = new SignalData(-1, double.MaxValue);
			}
			SetBeacon(new BeaconData(type, optional, signal));
			for (int j = 0; j < InputDevicePlugin.AvailablePluginInfos.Count; j++)
			{
				if (InputDevicePlugin.AvailablePluginInfos[j].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable && InputDevicePlugin.AvailablePlugins[j] is ITrainInputDevice)
				{
					ITrainInputDevice trainInputDevice = (ITrainInputDevice)InputDevicePlugin.AvailablePlugins[j];
					trainInputDevice.SetBeacon(new BeaconData(type, optional, signal));
				}
			}
		}

		/// <summary>Called when the train passes a beacon.</summary>
		/// <param name="beacon">The beacon data.</param>
		/// <remarks>This function should not be called directly. Call UpdateBeacon instead.</remarks>
		protected abstract void SetBeacon(BeaconData beacon);

		/// <summary>Updates the AI.</summary>
		/// <returns>The AI response.</returns>
		public AIResponse UpdateAI()
		{
			if (this.SupportsAI != AISupport.None)
			{
				AIData data = new AIData(GetHandles());
				this.PerformAI(data);
				if (data.Response != AIResponse.None)
				{
					SetHandles(data.Handles, false);
				}

				return data.Response;
			}

			return AIResponse.None;
		}

		/// <summary>Called when the AI should be performed.</summary>
		/// <param name="data">The AI data.</param>
		/// <remarks>This function should not be called directly. Call UpdateAI instead.</remarks>
		protected abstract void PerformAI(AIData data);

	}

}
