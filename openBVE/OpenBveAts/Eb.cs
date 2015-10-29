using System;
using OpenBveApi.Runtime;

namespace Plugin {
	/// <summary>Represents the EB device.</summary>
	internal class Eb : Device {
		
		// --- members ---
		
		/// <summary>The underlying train.</summary>
		private Train Train;
		
		/// <summary>The counter. This starts at zero and counts up until the EbAlarm or EbBrake constants have been reached.</summary>
		internal double Counter;
		

		// --- constants ---
		
		/// <summary>The time after which the bell is sound.</summary>
		internal readonly double TimeUntilBell = 60.0;
		
		/// <summary>The time after which the brakes are applied.</summary>
		internal readonly double TimeUntilBrake = 65.0;
		
		/// <summary>The speed beyond which the EB is active.</summary>
		internal readonly double SpeedThreshold = 5.0 / 3.6;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this system.</summary>
		/// <param name="train">The train.</param>
		internal Eb(Train train) {
			this.Train = train;
			this.Counter = 0.0;
		}
		
		
		// --- inherited functions ---
		
		/// <summary>Is called when the system should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal override void Initialize(InitializationModes mode) {
			this.Counter = 0.0;
		}

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		/// <param name="blocking">Whether the device is blocked or will block subsequent devices.</param>
		internal override void Elapse(ElapseData data, ref bool blocking) {
			// --- behavior ---
			if (!blocking) {
				if (Math.Abs(data.Vehicle.Speed.KilometersPerHour) > SpeedThreshold | this.Counter >= TimeUntilBell) {
					this.Counter += data.ElapsedTime.Seconds;
					if (this.Counter >= TimeUntilBrake) {
						if (this.Train.AtsSx != null) {
							if (this.Train.AtsSx.State != AtsSx.States.Disabled) {
								this.Train.AtsSx.State = AtsSx.States.Emergency;
								if (this.Train.AtsP != null && (this.Train.AtsP.State == AtsP.States.Normal | this.Train.AtsP.State == AtsP.States.Pattern | this.Train.AtsP.State == AtsP.States.Brake | this.Train.AtsP.State == AtsP.States.Service | this.Train.AtsP.State == AtsP.States.Emergency)) {
									this.Train.AtsP.State = AtsP.States.Standby;
								}
								if (this.Train.Atc != null && (this.Train.Atc.State == Atc.States.Normal | this.Train.Atc.State == Atc.States.Service | this.Train.Atc.State == Atc.States.Emergency)) {
									this.Train.Atc.State = Atc.States.Ats;
								}
								this.Counter = 0.0;
							}
						} else {
							this.Train.Sounds.AtsBell.Play();
							data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches + 1;
						}
					} else if (this.Counter >= TimeUntilBell) {
						this.Train.Sounds.Eb.Play();
					}
				} else {
					this.Counter = 0.0;
				}
			} else {
				this.Counter = 0.0;
			}
			// --- panel ---
			if (this.Counter >= TimeUntilBrake) {
				int value = (int)data.TotalTime.Milliseconds % 1000 < 500 ? 1 : 0;
				this.Train.Panel[8] = value;
				this.Train.Panel[270] = value;
			} else if (this.Counter >= TimeUntilBell) {
				this.Train.Panel[8] = 1;
				this.Train.Panel[270] = 1;
			}
		}
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		internal override void SetReverser(int reverser) {
			if (this.Counter < TimeUntilBell) {
				this.Counter = 0.0;
			}
		}
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		internal override void SetPower(int powerNotch) {
			if (this.Counter < TimeUntilBell) {
				this.Counter = 0.0;
			}
		}
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		internal override void SetBrake(int brakeNotch) {
			if (this.Counter < TimeUntilBell) {
				this.Counter = 0.0;
			}
		}
		
		/// <summary>Is called when a key is pressed.</summary>
		/// <param name="key">The key.</param>
		internal override void KeyDown(VirtualKeys key) {
			switch (key) {
				case VirtualKeys.A2:
					// --- acknowledge the EB ---
					if (this.Counter >= TimeUntilBell) {
						this.Counter = 0.0;
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
			if (this.Counter < TimeUntilBrake) {
				this.Counter = 0.0;
			}
		}
		
		/// <summary>Is called to inform about signals.</summary>
		/// <param name="signal">The signal data.</param>
		internal override void SetSignal(SignalData[] signal) {
		}
		
		/// <summary>Is called when a beacon is passed.</summary>
		/// <param name="beacon">The beacon data.</param>
		internal override void SetBeacon(BeaconData beacon) {
		}
		
	}
}