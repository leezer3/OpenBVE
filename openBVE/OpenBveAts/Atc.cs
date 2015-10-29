using System;
using System.Collections.Generic;
using OpenBveApi.Runtime;

namespace Plugin {
	/// <summary>Represents ATC.</summary>
	internal class Atc : Device {
		
		
		// --- enumerations and structures ---
		
		/// <summary>Represents different states of ATC.</summary>
		internal enum States {
			/// <summary>The system is disabled.</summary>
			Disabled = 0,
			/// <summary>The system is enabled, but currently suppressed. This will change to States.Ats once the emergency brakes are released.</summary>
			Suppressed = 1,
			/// <summary>The system has been set to ATS mode.</summary>
			Ats = 2,
			/// <summary>The system is operating normally.</summary>
			Normal = 3,
			/// <summary>The system is applying the service brakes.</summary>
			Service = 4,
			/// <summary>The system is applying the emergency brakes.</summary>
			Emergency = 5
		}
		
		/// <summary>Represents different states of the compatibility ATC track.</summary>
		private enum CompatibilityStates {
			/// <summary>ATC is not available.</summary>
			Ats = 0,
			/// <summary>ATC is available. The ToAtc reminder plays when the train has come to a stop.</summary>
			ToAtc = 1,
			/// <summary>ATC is available.</summary>
			Atc = 2,
			/// <summary>ATC is available. The ToAts reminder plays when the train has come to a stop.</summary>
			ToAts = 3
		}
		
		/// <summary>Represents a speed limit at a specific track position.</summary>
		private struct CompatibilityLimit {
			// --- members ---
			/// <summary>The speed limit.</summary>
			internal double Limit;
			/// <summary>The track position.</summary>
			internal double Location;
			// --- constructors ---
			/// <summary>Creates a new compatibility limit.</summary>
			/// <param name="limit">The speed limit.</param>
			/// <param name="position">The track position.</param>
			internal CompatibilityLimit(double limit, double location) {
				this.Limit = limit;
				this.Location = location;
			}
		}
		
		
		// --- members ---
		
		/// <summary>The underlying train.</summary>
		private Train Train;
		
		/// <summary>The current state of the system.</summary>
		internal States State;
		
		/// <summary>Whether to switch to ATC in the next Elapse call. This is set by the Initialize call if the train should start in ATC mode. It is necessary to switch in the Elapse call because at the time of the Initialize call, the ATC track status is not yet known.</summary>
		private bool StateSwitch;
		
		/// <summary>The currently permitted ATC speed, or -1 if ATC is not available.</summary>
		internal double CurrentAtcSpeed;
		
		/// <summary>The state of the compatibility ATC track.</summary>
		private CompatibilityStates CompatibilityState;
		
		/// <summary>A list of all ATC speed limits in the route.</summary>
		private List<CompatibilityLimit> CompatibilityLimits;
		
		/// <summary>The element in the CompatibilityLimits list that holds the last encountered speed limit.</summary>
		private int CompatibilityLimitPointer;
		
		/// <summary>The state of the preceding train, or a null reference.</summary>
		private PrecedingVehicleState PrecedingTrain;
		
		
		// --- parameters ---
		
		/// <summary>Whether to automatically switch between ATS and ATC.</summary>
		private bool AutomaticSwitch = false;
		
		/// <summary>The permitted compatibility ATC speeds, which are X, 0, 15, 25, 45, 65, 75, 90, 100, 110 and 120.</summary>
		private readonly double[] CompatibilitySpeeds = new double[] {
			-1.0,
			0.0 / 3.6,
			15.0 / 3.6,
			25.0 / 3.6,
			45.0 / 3.6,
			55.0 / 3.6,
			65.0 / 3.6,
			75.0 / 3.6,
			90.0 / 3.6,
			100.0 / 3.6,
			110.0 / 3.6,
			120.0 / 3.6
		};
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this system.</summary>
		/// <param name="train">The train.</param>
		/// <param name="automaticSwitch">Whether to switch automatically between ATS to ATC.</param>
		internal Atc(Train train, bool automaticSwitch) {
			this.Train = train;
			this.State = States.Disabled;
			this.CompatibilityState = CompatibilityStates.Ats;
			this.CompatibilityLimits = new List<CompatibilityLimit>();
			this.AutomaticSwitch = automaticSwitch;
		}
		
		
		// --- functions ---
		
		/// <summary>Gets the current ATC speed, or -1 if ATC is not available. In emergency operation mode, returns 15 km/h.</summary>
		/// <returns>The ATC speed, or -1 if ATC is not available.</returns>
		private double GetCurrentAtcSpeed() {
			if (this.CompatibilityState != CompatibilityStates.Ats) {
				double a = GetAtcSpeedFromTrain();
				double b = GetAtcSpeedFromLimit();
				return Math.Min(a, b);
			} else {
				return -1.0;
			}
		}
		
		/// <summary>Gets the ATC speed from the distance to the preceding train if operating in compatibility ATC mode.</summary>
		/// <returns>The ATC speed, or -1 if ATC is not available.</returns>
		private double GetAtcSpeedFromTrain() {
			if (this.CompatibilityState != CompatibilityStates.Ats) {
				if (this.PrecedingTrain == null) {
					return this.CompatibilitySpeeds[11];
				} else {
					const double blockLength = 100.0;
					int a = (int)Math.Floor(this.PrecedingTrain.Location / blockLength);
					int b = (int)Math.Floor(this.Train.State.Location / blockLength);
					int blocks = a - b;
					switch (blocks) {
						case 0:
							return this.CompatibilitySpeeds[0];
						case 1:
							return this.CompatibilitySpeeds[1];
						case 2:
							return this.CompatibilitySpeeds[3];
						case 3:
							return this.CompatibilitySpeeds[4];
						case 4:
							return this.CompatibilitySpeeds[5];
						case 5:
							return this.CompatibilitySpeeds[6];
						case 6:
							return this.CompatibilitySpeeds[7];
						case 7:
							return this.CompatibilitySpeeds[8];
						case 8:
							return this.CompatibilitySpeeds[9];
						case 9:
							return this.CompatibilitySpeeds[10];
						default:
							return this.CompatibilitySpeeds[11];
					}
				}
			} else {
				return -1.0;
			}
		}
		
		/// <summary>Gets the ATC speed from the current and upcoming speed limits.</summary>
		/// <returns>The ATC speed, or -1 if ATC is not available.</returns>
		private double GetAtcSpeedFromLimit() {
			if (this.CompatibilityState != CompatibilityStates.Ats) {
				if (this.CompatibilityLimits.Count == 0) {
					return double.MaxValue;
				} else if (this.CompatibilityLimits.Count == 1) {
					return this.CompatibilityLimits[0].Limit;
				} else {
					while (CompatibilityLimitPointer > 0 && this.CompatibilityLimits[CompatibilityLimitPointer].Location > this.Train.State.Location) {
						CompatibilityLimitPointer--;
					}
					while (CompatibilityLimitPointer < this.CompatibilityLimits.Count - 1 && this.CompatibilityLimits[CompatibilityLimitPointer + 1].Location <= this.Train.State.Location) {
						CompatibilityLimitPointer++;
					}
					if (this.CompatibilityLimitPointer == this.CompatibilityLimits.Count - 1) {
						return this.CompatibilityLimits[this.CompatibilityLimitPointer].Limit;
					} else if (this.CompatibilityLimits[this.CompatibilityLimitPointer].Limit <= this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Limit) {
						return this.CompatibilityLimits[this.CompatibilityLimitPointer].Limit;
					} else {
						const double deceleration = 1.910 / 3.6;
						double currentLimit = this.CompatibilityLimits[this.CompatibilityLimitPointer].Limit;
						double upcomingLimit = this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Limit;
						double distance = (currentLimit * currentLimit - upcomingLimit * upcomingLimit) / (2.0 * deceleration);
						if (this.Train.State.Location < this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Location - distance) {
							return this.CompatibilityLimits[this.CompatibilityLimitPointer].Limit;
						} else {
							return this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Limit;
						}
					}
				}
			} else {
				return -1.0;
			}
		}
		
		/// <summary>Whether the driver should switch to ATS. This returns false if already operating in ATS.</summary>
		/// <returns>Whether the driver should switch to ATS.</returns>
		internal bool ShouldSwitchToAts() {
			if (this.CompatibilityState == CompatibilityStates.ToAts) {
				if (Math.Abs(this.Train.State.Speed.MetersPerSecond) < 0.01) {
					if (this.State == States.Normal | this.State == States.Service | this.State == States.Emergency) {
						return true;
					}
				}
			} else if (this.CompatibilityState == CompatibilityStates.Ats) {
				if (this.State == States.Normal | this.State == States.Service | this.State == States.Emergency) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Whether the driver should switch to ATC. This returns false if already operating in ATC.</summary>
		/// <returns>Whether the driver should switch to ATC.</returns>
		internal bool ShouldSwitchToAtc() {
			if (this.CompatibilityState == CompatibilityStates.Atc) {
				if (this.State == States.Ats) {
					return true;
				}
			} else if (this.CompatibilityState == CompatibilityStates.ToAtc) {
				if (Math.Abs(this.Train.State.Speed.MetersPerSecond) < 0.01) {
					if (this.State == States.Ats) {
						return true;
					}
				}
			}
			return false;
		}
		
		
		// --- inherited functions ---
		
		/// <summary>Is called when the system should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal override void Initialize(InitializationModes mode) {
			if (mode == InitializationModes.OffEmergency) {
				this.State = States.Suppressed;
			} else {
				this.State = States.Ats;
				this.StateSwitch = true;
			}
		}

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		/// <param name="blocking">Whether the device is blocked or will block subsequent devices.</param>
		internal override void Elapse(ElapseData data, ref bool blocking) {
			// --- behavior ---
			this.PrecedingTrain = data.PrecedingVehicle;
			if (this.StateSwitch) {
				this.State = States.Normal;
				this.StateSwitch = false;
			}
			if (this.State == States.Suppressed) {
				if (data.Handles.BrakeNotch <= this.Train.Specs.BrakeNotches) {
					if (this.Train.AtsSx != null | this.Train.AtsP != null) {
						this.State = States.Ats;
					} else {
						this.State = States.Normal;
					}
				}
			}
			if (blocking) {
				if (this.State != States.Disabled & this.State != States.Suppressed) {
					this.State = States.Ats;
				}
			} else {
				if (this.State == States.Normal | this.State == States.Service | this.State == States.Emergency) {
					double speed;
					speed = GetCurrentAtcSpeed();
					if (speed != this.CurrentAtcSpeed) {
						this.Train.Sounds.AtcBell.Play();
					}
					this.CurrentAtcSpeed = speed;
					if (speed < 0.0) {
						if (this.State != States.Emergency) {
							this.State = States.Emergency;
							this.Train.Sounds.AtcBell.Play();
						}
					} else if (Math.Abs(data.Vehicle.Speed.MetersPerSecond) > speed) {
						if (this.State != States.Service) {
							this.State = States.Service;
							this.Train.Sounds.AtcBell.Play();
						}
					} else if (Math.Abs(data.Vehicle.Speed.MetersPerSecond) < speed - 1.0 / 3.6) {
						if (this.State != States.Normal) {
							this.State = States.Normal;
							this.Train.Sounds.AtcBell.Play();
						}
					}
					if (this.State == States.Service) {
						if (data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches) {
							data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches;
						}
					} else if (this.State == States.Emergency) {
						data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches + 1;
					}
					blocking = true;
				}
				if (this.State != States.Disabled & (this.Train.Doors != DoorStates.None | data.Handles.BrakeNotch > 0)) {
					data.Handles.PowerNotch = 0;
				}
			}
			// --- panel ---
			if (this.State == States.Ats) {
				this.Train.Panel[271] = 12;
			} else if (this.State == States.Normal | this.State == States.Service | this.State == States.Emergency) {
				this.Train.Panel[265] = 1;
				if (this.CurrentAtcSpeed < 0.0) {
					this.Train.Panel[271] = 0;
				} else {
					int value = this.CompatibilitySpeeds.Length - 1;
					for (int i = 2; i < this.CompatibilitySpeeds.Length; i++) {
						if (this.CompatibilitySpeeds[i] > this.CurrentAtcSpeed) {
							value = i - 1;
							break;
						}
					}
					this.Train.Panel[271] = value;
				}
			}
			if (this.State == States.Service) {
				this.Train.Panel[267] = 1;
			} else if (this.State == States.Emergency) {
				this.Train.Panel[268] = 1;
			}
			if (this.State != States.Disabled & this.State != States.Suppressed) {
				this.Train.Panel[266] = 1;
			}
			// --- manual or automatic switch ---
			if (ShouldSwitchToAts()) {
				if (this.AutomaticSwitch & Math.Abs(data.Vehicle.Speed.MetersPerSecond) < 1.0 / 3.6) {
					KeyDown(VirtualKeys.C1);
				}
			} else if (ShouldSwitchToAtc()) {
				if (this.AutomaticSwitch & Math.Abs(data.Vehicle.Speed.MetersPerSecond) < 1.0 / 3.6) {
					KeyDown(VirtualKeys.C2);
				}
			}
		}
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		internal override void SetReverser(int reverser) {
		}
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		internal override void SetPower(int powerNotch) {
		}
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		internal override void SetBrake(int brakeNotch) {
		}
		
		/// <summary>Is called when a key is pressed.</summary>
		/// <param name="key">The key.</param>
		internal override void KeyDown(VirtualKeys key) {
			switch (key) {
				case VirtualKeys.C1:
					// --- switch to ats ---
					if (this.State == States.Normal | this.State == States.Service | this.State == States.Emergency) {
						if (this.Train.AtsSx != null | this.Train.AtsP != null) {
							this.State = States.Ats;
							this.Train.Sounds.ToAts.Play();
						}
					}
					break;
				case VirtualKeys.C2:
					// --- switch to atc ---
					if (this.State == States.Ats) {
						this.State = States.Normal;
						this.Train.Sounds.ToAtc.Play();
					}
					break;
			}
		}
		
		/// <summary>Is called when a key is released.</summary>
		/// <param name="key">The key.</param>
		internal override void KeyUp(VirtualKeys key) {
		}
		
		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		internal override void HornBlow(HornTypes type) {
		}
		
		/// <summary>Is called to inform about signals.</summary>
		/// <param name="signal">The signal data.</param>
		internal override void SetSignal(SignalData[] signal) {
		}
		
		/// <summary>Is called when a beacon is passed.</summary>
		/// <param name="beacon">The beacon data.</param>
		internal override void SetBeacon(BeaconData beacon) {
			switch (beacon.Type) {
				case -16777215:
					if (beacon.Optional >= 0 & beacon.Optional <= 3) {
						this.CompatibilityState = (CompatibilityStates)beacon.Optional;
					}
					break;
				case -16777214:
					{
						double limit = (double)(beacon.Optional & 4095) / 3.6;
						double position = (beacon.Optional >> 12);
						CompatibilityLimit item = new CompatibilityLimit(limit, position);
						if (!this.CompatibilityLimits.Contains(item)) {
							this.CompatibilityLimits.Add(item);
						}
					}
					break;
			}
		}

	}
}