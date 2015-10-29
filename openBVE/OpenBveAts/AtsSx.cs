using System;
using OpenBveApi.Runtime;

namespace Plugin {
	/// <summary>Represents ATS-Sx.</summary>
	internal class AtsSx : Device {
		
		// --- enumerations ---
		
		/// <summary>Represents different states of ATS-Sx.</summary>
		internal enum States {
			/// <summary>The system is disabled.</summary>
			Disabled = 0,
			/// <summary>The system is enabled, but currently suppressed. This will change to States.Initializing once the emergency brakes are released.</summary>
			Suppressed = 1,
			/// <summary>The system is initializing. This will change to States.Chime once the initialization is complete.</summary>
			Initializing = 2,
			/// <summary>The chime is ringing.</summary>
			Chime = 3,
			/// <summary>The system is operating normally.</summary>
			Normal = 4,
			/// <summary>The alarm is ringing. This will change to States.Emergency once the countdown runs out.</summary>
			Alarm = 5,
			/// <summary>The system applies the emergency brakes.</summary>
			Emergency = 6
		}
		
		
		// --- members ---
		
		/// <summary>The underlying train.</summary>
		private Train Train;
		
		/// <summary>The current state of the system.</summary>
		internal States State;
		
		/// <summary>The current alarm countdown.</summary>
		/// <remarks>With States.Initializing, this counts down until the initialization is complete.</remarks>
		/// <remarks>With States.Alarm, this counts down until the emergency brakes are engaged.</remarks>
		private double AlarmCountdown;
		
		/// <summary>The current speed check countdown.</summary>
		private double SpeedCheckCountdown;
		
		/// <summary>The distance traveled since the last IIYAMA-style Ps2 beacon.</summary>
		private double CompatibilityDistanceAccumulator;
		
		/// <summary>The location of the farthest known red signal, or System.Double.MinValue.</summary>
		internal double RedSignalLocation;
		
		
		// --- parameters ---
		
		/// <summary>The duration of the alarm until the emergency brakes are applied.</summary>
		internal readonly double DurationOfAlarm = 5.0;
		
		/// <summary>The duration of the initialization process.</summary>
		internal readonly double DurationOfInitialization = 3.0;
		
		/// <summary>The duration of the Sx speed check.</summary>
		internal readonly double DurationOfSpeedCheck = 0.5;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this system with default parameters.</summary>
		/// <param name="train">The train.</param>
		internal AtsSx(Train train) {
			this.Train = train;
			this.State = States.Disabled;
			this.AlarmCountdown = 0.0;
			this.SpeedCheckCountdown = 0.0;
			this.RedSignalLocation = 0.0;
		}
		
		
		// --- inherited functions ---
		
		/// <summary>Is called when the system should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal override void Initialize(InitializationModes mode) {
			if (mode == InitializationModes.OffEmergency) {
				this.State = States.Suppressed;
			} else {
				this.State = States.Normal;
			}
		}

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		/// <param name="blocking">Whether the device is blocked or will block subsequent devices.</param>
		internal override void Elapse(ElapseData data, ref bool blocking) {
			// --- behavior ---
			if (this.State == States.Suppressed) {
				if (data.Handles.BrakeNotch <= this.Train.Specs.BrakeNotches) {
					this.AlarmCountdown = DurationOfInitialization;
					this.State = States.Initializing;
				}
			}
			if (this.State == States.Initializing) {
				this.AlarmCountdown -= data.ElapsedTime.Seconds;
				if (this.AlarmCountdown <= 0.0) {
					this.State = States.Chime;
				} else {
					data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches + 1;
					this.Train.Sounds.AtsBell.Play();
				}
			}
			if (blocking) {
				if (this.State != States.Disabled & this.State != States.Suppressed) {
					this.State = States.Normal;
				}
			} else {
				if (this.State == States.Chime) {
					this.Train.Sounds.AtsChime.Play();
				} else if (this.State == States.Alarm) {
					this.Train.Sounds.AtsBell.Play();
					this.AlarmCountdown -= data.ElapsedTime.Seconds;
					if (this.AlarmCountdown <= 0.0) {
						this.State = States.Emergency;
					}
				} else if (this.State == States.Emergency) {
					this.Train.Sounds.AtsBell.Play();
					data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches + 1;
				}
				if (this.SpeedCheckCountdown > 0.0 & data.ElapsedTime.Seconds > 0.0) {
					this.SpeedCheckCountdown -= data.ElapsedTime.Seconds;
				}
				if (this.CompatibilityDistanceAccumulator != 0.0) {
					this.CompatibilityDistanceAccumulator += data.Vehicle.Speed.MetersPerSecond * data.ElapsedTime.Seconds;
					if (this.CompatibilityDistanceAccumulator > 27.7) {
						this.CompatibilityDistanceAccumulator = 0.0;
					}
				}
				if (this.State != States.Disabled & (this.Train.Doors != DoorStates.None | data.Handles.BrakeNotch > 0)) {
					data.Handles.PowerNotch = 0;
				}
			}
			// --- panel ---
			if ((this.State == States.Chime | this.State == States.Normal) & !blocking) {
				this.Train.Panel[256] = 1;
			}
			if (this.State == States.Initializing | this.State == States.Alarm) {
				this.Train.Panel[257] = 1;
				this.Train.Panel[258] = 1;
			} else if (this.State == States.Emergency) {
				int value = (int)data.TotalTime.Milliseconds % 1000 < 500 ? 1 : 0;
				this.Train.Panel[257] = 2;
				this.Train.Panel[258] = value;
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
				case VirtualKeys.S:
					// --- acknowledge the alarm ---
					if (this.State == States.Alarm & this.Train.Handles.PowerNotch == 0 & this.Train.Handles.BrakeNotch >= this.Train.Specs.AtsNotch) {
						this.State = States.Chime;
					}
					break;
				case VirtualKeys.A1:
					// --- stop the chime ---
					if (this.State == States.Chime) {
						this.State = States.Normal;
					}
					break;
				case VirtualKeys.B1:
					// --- reset the system ---
					if (this.State == States.Emergency & this.Train.Handles.Reverser == 0 & this.Train.Handles.PowerNotch == 0 & this.Train.Handles.BrakeNotch == this.Train.Specs.BrakeNotches + 1) {
						this.State = States.Chime;
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
			if (this.RedSignalLocation != double.MinValue) {
				for (int i = 0; i < signal.Length; i++) {
					const double visibility = 200.0;
					if (signal[i].Distance < visibility) {
						double location = this.Train.State.Location + signal[i].Distance;
						if (Math.Abs(location - this.RedSignalLocation) < 50.0) {
							if (signal[i].Aspect != 0) {
								this.RedSignalLocation = double.MinValue;
							}
						}
					}
				}
			}
		}
		
		/// <summary>Is called when a beacon is passed.</summary>
		/// <param name="beacon">The beacon data.</param>
		internal override void SetBeacon(BeaconData beacon) {
			if (this.State != States.Disabled & this.State != States.Initializing) {
				switch (beacon.Type) {
					case 0:
						// --- Sx long ---
						if (beacon.Signal.Aspect == 0) {
							if (this.State == States.Chime | this.State == States.Normal) {
								this.AlarmCountdown = DurationOfAlarm;
								this.State = States.Alarm;
								UpdateRedSignalLocation(beacon);
							}
						}
						break;
					case 1:
						// --- Sx immediate stop ---
						if (beacon.Signal.Aspect == 0) {
							if (this.State == States.Chime | this.State == States.Normal | this.State == States.Alarm) {
								this.State = States.Emergency;
							}
						}
						break;
					case 2:
						// --- accidental departure ---
						if (beacon.Signal.Aspect == 0 & (beacon.Optional == 0 | beacon.Optional >= this.Train.Specs.Cars)) {
							if (this.State == States.Chime | this.State == States.Normal | this.State == States.Alarm) {
								this.State = States.Emergency;
							}
						}
						break;
				}
			}
		}
		
		
		// --- private functions ---
		
		/// <summary>Updates the location of the farthest known red signal from the specified beacon.</summary>
		/// <param name="beacon">The beacon that holds the distance to a known red signal.</param>
		private void UpdateRedSignalLocation(BeaconData beacon) {
			if (beacon.Signal.Distance < 1200.0) {
				double signalLocation = this.Train.State.Location + beacon.Signal.Distance;
				if (signalLocation > this.RedSignalLocation) {
					this.RedSignalLocation = signalLocation;
				}
			}
		}
		
	}
}