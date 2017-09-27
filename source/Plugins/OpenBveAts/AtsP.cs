using System;
using System.Collections.Generic;
using OpenBveApi.Runtime;

namespace Plugin {
	/// <summary>Represents ATS-P.</summary>
	internal class AtsP : Device {
		
		// --- enumerations ---
		
		/// <summary>Represents different states of ATS-P.</summary>
		internal enum States {
			/// <summary>The system is disabled.</summary>
			Disabled = 0,
			/// <summary>The system is enabled, but currently suppressed. This will change to States.Initializing once the emergency brakes are released.</summary>
			Suppressed = 1,
			/// <summary>The system is initializing. This will change to States.Standby once the initialization is complete.</summary>
			Initializing = 2,
			/// <summary>The system is available but no ATS-P signal was yet picked up.</summary>
			Standby = 3,
			/// <summary>The system is operating normally.</summary>
			Normal = 4,
			/// <summary>The system is approaching a brake pattern.</summary>
			Pattern = 5,
			/// <summary>The system is braking due to speed excess.</summary>
			Brake = 6,
			/// <summary>The system applies the service brakes due to an immediate stop command.</summary>
			Service = 7,
			/// <summary>The system applies the emergency brakes due to an immediate stop command.</summary>
			Emergency = 8
		}

		
		// --- pattern ---
		
		/// <summary>Represents a pattern.</summary>
		private class Pattern {
			// --- members ---
			/// <summary>The underlying ATS-P device.</summary>
			internal AtsP Device;
			/// <summary>The position of the point of danger, or System.Double.MinValue, or System.Double.MaxValue.</summary>
			internal double Position;
			/// <summary>The warning pattern, or System.Double.MaxValue.</summary>
			internal double WarningPattern;
			/// <summary>The brake pattern, or System.Double.MaxValue.</summary>
			internal double BrakePattern;
			/// <summary>The speed limit at the point of danger, or System.Double.MaxValue.</summary>
			internal double TargetSpeed;
			/// <summary>The current gradient.</summary>
			internal double Gradient;
			// --- constructors ---
			/// <summary>Creates a new pattern.</summary>
			/// <param name="device">A reference to the underlying ATS-P device.</param>
			internal Pattern(AtsP device) {
				this.Device = device;
				this.Position = double.MaxValue;
				this.WarningPattern = double.MaxValue;
				this.BrakePattern = double.MaxValue;
				this.TargetSpeed = double.MaxValue;
				this.Gradient = 0.0;
			}
			// --- functions ---
			/// <summary>Updates the pattern.</summary>
			/// <param name="system">The current ATS-P system.</param>
			/// <param name="data">The elapse data.</param>
			internal void Perform(AtsP system, ElapseData data) {
				if (this.Position == double.MaxValue | this.TargetSpeed == double.MaxValue) {
					this.WarningPattern = double.MaxValue;
					this.BrakePattern = double.MaxValue;
				} else if (this.Position == double.MinValue) {
					this.WarningPattern = this.TargetSpeed - this.Device.PatternSpeedDifference;
					this.BrakePattern = Math.Max(this.TargetSpeed, this.Device.ReleaseSpeed);
				} else {
					const double earthGravity = 9.81;
                    double accelerationDueToGravity = earthGravity * this.Gradient / Math.Sqrt(1.0 + this.Gradient * this.Gradient);
					double deceleration = this.Device.DesignDeceleration + accelerationDueToGravity;
					double distance = this.Position - system.Position;
					/*
					 * Calculate the warning pattern.
					 * */
					{
						double sqrtTerm = 2.0 * deceleration * (distance - 50.0) + deceleration * deceleration * this.Device.ReactionDelay * this.Device.ReactionDelay + this.TargetSpeed * this.TargetSpeed;
						if (sqrtTerm <= 0.0) {
							this.WarningPattern = this.TargetSpeed - this.Device.PatternSpeedDifference;
						} else {
							this.WarningPattern = Math.Max(Math.Sqrt(sqrtTerm) - deceleration * this.Device.ReactionDelay, this.TargetSpeed - this.Device.PatternSpeedDifference);
						}
					}
					/*
					 * Calculate the brake pattern.
					 * */
					{
						double sqrtTerm = 2.0 * deceleration * distance + this.TargetSpeed * this.TargetSpeed;
						if (sqrtTerm <= 0.0) {
							this.BrakePattern = this.TargetSpeed;
						} else {
							this.BrakePattern = Math.Max(Math.Sqrt(sqrtTerm), TargetSpeed);
						}
						if (this.BrakePattern < this.Device.ReleaseSpeed) {
							this.BrakePattern = this.Device.ReleaseSpeed;
						}
					}
					
				}
			}
			/// <summary>Sets the position of the red signal.</summary>
			/// <param name="position">The position.</param>
			internal void SetRedSignal(double position) {
				this.Position = position;
				this.TargetSpeed = 0.0;
			}
			/// <summary>Sets the position of the green signal.</summary>
			/// <param name="position">The position.</param>
			internal void SetGreenSignal(double position) {
				this.Position = position;
				this.TargetSpeed = double.MaxValue;
			}
			/// <summary>Sets a speed limit and the position of the speed limit.</summary>
			/// <param name="speed">The speed.</param>
			/// <param name="position">The position.</param>
			internal void SetLimit(double speed, double position) {
				this.Position = position;
				this.TargetSpeed = speed;
			}
			/// <summary>Sets the gradient.</summary>
			/// <param name="gradient">The gradient.</param>
			internal void SetGradient(double gradient) {
				this.Gradient = gradient;
			}
			/// <summary>Clears the pattern.</summary>
			internal void Clear() {
				this.Position = double.MaxValue;
				this.WarningPattern = double.MaxValue;
				this.BrakePattern = double.MaxValue;
				this.TargetSpeed = double.MaxValue;
				this.Gradient = 0.0;
			}
		}
		
		// --- compatibility limit ---
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
			/// <param name="location">The track position.</param>
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
		
		/// <summary>Whether the brake release is currently active.</summary>
		private bool BrakeRelease;
		
		/// <summary>The remaining time before the brake release is over.</summary>
		private double BrakeReleaseCountdown;
		
		/// <summary>The current initialization countdown.</summary>
		private double InitializationCountdown;
		
		/// <summary>The position of the train as obtained from odometry.</summary>
		private double Position;
		
		/// <summary>A list of all compatibility temporary speed limits in the route.</summary>
		private List<CompatibilityLimit> CompatibilityLimits;
		
		/// <summary>The element in the CompatibilityLimits list that holds the last speed limit.</summary>
		private int CompatibilityLimitPointer;
		
		
		// --- patterns ---
		
		/// <summary>The signal pattern.</summary>
		private Pattern SignalPattern;
		
		/// <summary>The compatibility temporary pattern.</summary>
		private Pattern CompatibilityTemporaryPattern;

		/// <summary>The compatibility permanent pattern.</summary>
		private Pattern CompatibilityPermanentPattern;

		/// <summary>A list of all patterns.</summary>
		private Pattern[] Patterns;
		
		
		// --- parameters ---

		/// <summary>The duration of the initialization process.</summary>
		internal readonly double DurationOfInitialization = 3.0;

		/// <summary>The duration of the brake release. If zero, the brake release is not available.</summary>
		internal readonly double DurationOfBrakeRelease = 60.0;
		
		/// <summary>The design deceleration.</summary>
		internal readonly double DesignDeceleration = 2.445 / 3.6;

		/// <summary>The reaction delay.</summary>
		internal readonly double ReactionDelay = 5.5;

		/// <summary>The release speed.</summary>
		internal readonly double ReleaseSpeed = 15.0 / 3.6;
		
		/// <summary>The pattern speed difference.</summary>
		internal readonly double PatternSpeedDifference = 5.0 / 3.6;
		
		/// <summary>The signal offset.</summary>
		internal readonly double SignalOffset = 0.0;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this system.</summary>
		/// <param name="train">The train.</param>
		internal AtsP(Train train) {
			this.Train = train;
			this.State = States.Disabled;
			this.InitializationCountdown = 0.0;
			this.CompatibilityLimits = new List<CompatibilityLimit>();
			this.CompatibilityLimitPointer = 0;
			this.SignalPattern = new Pattern(this);
			this.CompatibilityTemporaryPattern = new Pattern(this);
			this.CompatibilityPermanentPattern = new Pattern(this);
			List<Pattern> patterns = new List<Pattern>
			{
				this.SignalPattern,
				this.CompatibilityTemporaryPattern,
				this.CompatibilityPermanentPattern
			};
			this.Patterns = patterns.ToArray();
		}
		
		
		// --- functions ---
		
		/// <summary>Changes to standby mode and continues in ATS-Sx.</summary>
		private void SwitchToSx() {
			if (this.Train.AtsSx != null) {
				foreach (Pattern pattern in this.Patterns) {
					pattern.Clear();
				}
				this.State = States.Standby;
				this.Train.Sounds.AtsPBell.Play();
				this.Train.AtsSx.State = AtsSx.States.Chime;
			} else if (this.State != States.Emergency) {
				this.State = States.Emergency;
				if (this.State != States.Brake & this.State != States.Service) {
					this.Train.Sounds.AtsPBell.Play();
				}
			}
		}
		
		/// <summary>Switches to ATS-P.</summary>
		/// <param name="state">The desired state.</param>
		private void SwitchToP(States state) {
			if (this.State == States.Standby) {
				if (this.Train.AtsSx == null || this.Train.AtsSx.State != AtsSx.States.Emergency) {
					this.State = state;
					this.Train.Sounds.AtsPBell.Play();
				}
			} else if (state == States.Service | state == States.Emergency) {
				if (this.State != States.Brake & this.State != States.Service & this.State != States.Emergency) {
					this.Train.Sounds.AtsPBell.Play();
				}
				this.State = state;
			}
		}
		
		/// <summary>Updates the compatibility temporary speed pattern from the list of known speed limits.</summary>
		private void UpdateCompatibilityTemporarySpeedPattern() {
			if (this.CompatibilityLimits.Count != 0) {
				if (this.CompatibilityTemporaryPattern.Position != double.MaxValue) {
					if (this.CompatibilityTemporaryPattern.BrakePattern < this.Train.State.Speed.MetersPerSecond) {
						return;
					}
					double delta = this.CompatibilityTemporaryPattern.Position - this.Train.State.Location;
					if (delta >= -50.0 & delta <= 0.0) {
						return;
					}
				}
				while (CompatibilityLimitPointer > 0 && this.CompatibilityLimits[CompatibilityLimitPointer].Location > this.Train.State.Location) {
					CompatibilityLimitPointer--;
				}
				while (CompatibilityLimitPointer < this.CompatibilityLimits.Count - 1 && this.CompatibilityLimits[CompatibilityLimitPointer + 1].Location <= this.Train.State.Location) {
					CompatibilityLimitPointer++;
				}
				if (this.CompatibilityLimitPointer == 0 && this.CompatibilityLimits[0].Location > this.Train.State.Location) {
					this.CompatibilityTemporaryPattern.SetLimit(this.CompatibilityLimits[0].Limit, this.CompatibilityLimits[0].Location);
				} else if (this.CompatibilityLimitPointer < this.CompatibilityLimits.Count - 1) {
					this.CompatibilityTemporaryPattern.SetLimit(this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Limit, this.CompatibilityLimits[this.CompatibilityLimitPointer + 1].Location);
				} else {
					this.CompatibilityTemporaryPattern.Clear();
				}
			}
		}
		
		
		// --- inherited functions ---
		
		/// <summary>Is called when the system should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal override void Initialize(InitializationModes mode) {
			if (mode == InitializationModes.OffEmergency) {
				this.State = States.Suppressed;
			} else {
				this.State = States.Standby;
			}
			foreach (Pattern pattern in this.Patterns) {
				if (Math.Abs(this.Train.State.Speed.MetersPerSecond) >= pattern.WarningPattern) {
					pattern.Clear();
				}
			}
		}

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		/// <param name="blocking">Whether the device is blocked or will block subsequent devices.</param>
		internal override void Elapse(ElapseData data, ref bool blocking) {
			// --- behavior ---
			if (this.State == States.Suppressed) {
				if (data.Handles.BrakeNotch <= this.Train.Specs.BrakeNotches) {
					this.InitializationCountdown = DurationOfInitialization;
					this.State = States.Initializing;
				}
			}
			if (this.State == States.Initializing) {
				this.InitializationCountdown -= data.ElapsedTime.Seconds;
				if (this.InitializationCountdown <= 0.0) {
					this.State = States.Standby;
					foreach (Pattern pattern in this.Patterns) {
						if (Math.Abs(data.Vehicle.Speed.MetersPerSecond) >= pattern.WarningPattern) {
							pattern.Clear();
						}
					}
					this.Train.Sounds.AtsPBell.Play();
				}
			}
			if (this.BrakeRelease) {
				this.BrakeReleaseCountdown -= data.ElapsedTime.Seconds;
				if (this.BrakeReleaseCountdown <= 0.0) {
					this.BrakeRelease = false;
					this.Train.Sounds.AtsPBell.Play();
				}
			}
			if (this.State != States.Disabled & this.State != States.Initializing) {
				this.Position += data.Vehicle.Speed.MetersPerSecond * data.ElapsedTime.Seconds;
			}
			if (blocking) {
				if (this.State != States.Disabled & this.State != States.Suppressed) {
					this.State = States.Standby;
				}
			} else {
				if (this.State == States.Normal | this.State == States.Pattern | this.State == States.Brake) {
					bool brake = false;
					bool warning = false;
					UpdateCompatibilityTemporarySpeedPattern();
					foreach (Pattern pattern in this.Patterns) {
						pattern.Perform(this, data);
						if (Math.Abs(data.Vehicle.Speed.MetersPerSecond) >= pattern.WarningPattern) {
							warning = true;
						}
						if (Math.Abs(data.Vehicle.Speed.MetersPerSecond) >= pattern.BrakePattern) {
							brake = true;
						}
					}
					if (BrakeRelease) {
						brake = false;
					}
					if (brake & this.State != States.Brake) {
						this.State = States.Brake;
						this.Train.Sounds.AtsPBell.Play();
					} else if (warning & this.State == States.Normal) {
						this.State = States.Pattern;
						this.Train.Sounds.AtsPBell.Play();
					} else if (!brake & !warning & (this.State == States.Pattern | this.State == States.Brake)) {
						this.State = States.Normal;
						this.Train.Sounds.AtsPBell.Play();
					}
					if (this.State == States.Brake) {
						if (data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches) {
							data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches;
						}
					}
					blocking = true;
				} else if (this.State == States.Service) {
					if (data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches) {
						data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches;
					}
					blocking = true;
				} else if (this.State == States.Emergency) {
					data.Handles.BrakeNotch = this.Train.Specs.BrakeNotches + 1;
					blocking = true;
				}
				if (this.State != States.Disabled & (this.Train.Doors != DoorStates.None | data.Handles.BrakeNotch > 0)) {
					data.Handles.PowerNotch = 0;
				}
			}
			// --- panel ---
			if (this.State != States.Disabled & this.State != States.Suppressed) {
				this.Train.Panel[2] = 1;
				this.Train.Panel[259] = 1;
			}
			if (this.State == States.Pattern | this.State == States.Brake | this.State == States.Service | this.State == States.Emergency) {
				this.Train.Panel[3] = 1;
				this.Train.Panel[260] = 1;
			}
			if (this.State == States.Brake | this.State == States.Service | this.State == States.Emergency) {
				this.Train.Panel[5] = 1;
				this.Train.Panel[262] = 1;
			}
			if (this.State != States.Disabled & this.State != States.Suppressed & this.State != States.Standby) {
				this.Train.Panel[6] = 1;
				this.Train.Panel[263] = 1;
			}
			if (this.State == States.Initializing) {
				this.Train.Panel[7] = 1;
				this.Train.Panel[264] = 1;
			}
			if (this.State == States.Disabled) {
				this.Train.Panel[50] = 1;
			}
			if (this.BrakeRelease) {
				this.Train.Panel[4] = 1;
				this.Train.Panel[261] = 1;
			}
//			if (this.State == States.Normal | this.State == States.Pattern | this.State == States.Brake | this.State == States.Service | this.State == States.Emergency) {
//				data.DebugMessage = (this.SignalPattern.TargetSpeed == double.MaxValue ? "INF" : (3.6 * this.SignalPattern.TargetSpeed).ToString("0") + "km/h") + "@" + (this.SignalPattern.Position == double.MaxValue ? "INF" : (this.SignalPattern.Position - this.Position).ToString("0") + "m");
//			}
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
				case VirtualKeys.B1:
					// --- reset the system ---
					if ((this.State == States.Brake | this.State == States.Service | this.State == States.Emergency) & this.Train.Handles.Reverser == 0 & this.Train.Handles.PowerNotch == 0 & this.Train.Handles.BrakeNotch >= this.Train.Specs.BrakeNotches) {
						foreach (Pattern pattern in this.Patterns) {
							if (Math.Abs(this.Train.State.Speed.MetersPerSecond) >= pattern.WarningPattern) {
								pattern.Clear();
							}
						}
						this.State = States.Normal;
						this.Train.Sounds.AtsPBell.Play();
					}
					break;
				case VirtualKeys.B2:
					// --- brake release ---
					if ((this.State == States.Normal | this.State == States.Pattern) & !BrakeRelease & DurationOfBrakeRelease > 0.0) {
						BrakeRelease = true;
						BrakeReleaseCountdown = DurationOfBrakeRelease;
						this.Train.Sounds.AtsPBell.Play();
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
			if (this.State != States.Disabled & this.State != States.Suppressed & this.State != States.Initializing) {
				switch (beacon.Type) {
					case 0:
					case 1:
						// --- P -> S ---
						if (beacon.Optional == 0) {
							if (this.State == States.Normal | this.State == States.Pattern | this.State == States.Brake) {
								SwitchToSx();
							}
						}
						break;
					case 3:
					case 4:
						// --- P pattern / P immediate stop ---
						this.Position = this.Train.State.Location;
						if (this.State != States.Service & this.State != States.Emergency) {
							if (this.State == States.Standby & (beacon.Type != 3 | beacon.Optional != -1)) {
								SwitchToP(States.Normal);
							}
							if (this.State != States.Standby) {
								double position = this.Position + beacon.Signal.Distance - this.SignalOffset;
								bool update = false;
								if (this.SignalPattern.Position == double.MaxValue) {
									update = true;
								} else if (position > this.SignalPattern.Position - 30.0) {
									update = true;
								}
								if (update) {
									if (beacon.Signal.Aspect == 0) {
										this.SignalPattern.SetRedSignal(position);
										if (beacon.Type != 3 & beacon.Signal.Distance < 50.0 & !BrakeRelease) {
											SwitchToP(States.Emergency);
										}
									} else {
										this.SignalPattern.SetGreenSignal(position);
									}
								}
							}
						}
						break;
				}
			}
			switch (beacon.Type) {
				case -16777213:
					// --- compatibility temporary pattern ---
					{
						double limit = (double)(beacon.Optional & 4095) / 3.6;
						double position = (beacon.Optional >> 12);
						CompatibilityLimit item = new CompatibilityLimit(limit, position);
						if (!this.CompatibilityLimits.Contains(item)) {
							this.CompatibilityLimits.Add(item);
						}
					}
					break;
				case -16777212:
					// --- compatibility permanent pattern ---
					if (beacon.Optional == 0) {
						this.CompatibilityPermanentPattern.Clear();
					} else {
						double limit = (double)beacon.Optional / 3.6;
						this.CompatibilityPermanentPattern.SetLimit(limit, double.MinValue);
					}
					break;
			}
		}

	}
}