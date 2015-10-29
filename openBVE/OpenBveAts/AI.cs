using System;
using OpenBveApi.Runtime;

namespace Plugin {
	internal class AI {
		
		// --- members ---
		
		/// <summary>The underlying train.</summary>
		private Train Train;
		
		private bool AtcProbing;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new AI.</summary>
		/// <param name="train">The underlying train.</param>
		internal AI(Train train) {
			this.Train = train;
		}
		
		
		// --- functions ---
		
		/// <summary>Is called when the plugin should perform the AI.</summary>
		/// <param name="data">The AI data.</param>
		internal void Perform(AIData data) {
			// --- ats-sx ---
			if (this.Train.AtsSx != null) {
				if (this.Train.AtsSx.State == AtsSx.States.Disabled) {
					this.Train.KeyDown(VirtualKeys.D);
					data.Response = AIResponse.Long;
					return;
				} else if (this.Train.AtsSx.State == AtsSx.States.Chime) {
					bool cancel = false;
					if (this.Train.State.Location > this.Train.AtsSx.RedSignalLocation) {
						cancel = true;
					}
					if (cancel) {
						this.Train.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						return;
					}
				} else if (this.Train.AtsSx.State == AtsSx.States.Alarm) {
					if (data.Handles.PowerNotch > 0) {
						data.Handles.PowerNotch--;
						data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.BrakeNotch < this.Train.Specs.AtsNotch) {
						data.Handles.BrakeNotch++;
						data.Response = data.Handles.BrakeNotch < this.Train.Specs.AtsNotch ? AIResponse.Short : AIResponse.Medium;
						return;
					} else {
						this.Train.KeyDown(VirtualKeys.S);
						data.Response = AIResponse.Medium;
						return;
					}
				} else if (this.Train.AtsSx.State == AtsSx.States.Emergency) {
					if (data.Handles.PowerNotch > 0) {
						data.Handles.PowerNotch--;
						data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.BrakeNotch <= this.Train.Specs.BrakeNotches) {
						data.Handles.BrakeNotch++;
						data.Response = data.Handles.BrakeNotch <= this.Train.Specs.BrakeNotches ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.Reverser != 0) {
						data.Handles.Reverser = 0;
						data.Response = AIResponse.Medium;
						return;
					} else if (Math.Abs(this.Train.State.Speed.KilometersPerHour) < 1.0) {
						this.Train.KeyDown(VirtualKeys.B1);
						data.Response = AIResponse.Long;
						return;
					} else {
						data.Response = AIResponse.Long;
						return;
					}
				}
			}
			// --- ats-p ---
			if (this.Train.AtsP != null) {
				if (this.Train.AtsP.State == AtsP.States.Disabled) {
					this.Train.KeyDown(VirtualKeys.D);
					data.Response = AIResponse.Long;
					return;
				} else if (this.Train.AtsP.State == AtsP.States.Pattern) {
					if (this.Train.State.Speed.MetersPerSecond > 15.0 / 3.6) {
						if (data.Handles.PowerNotch > 0) {
							data.Handles.PowerNotch--;
							data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
							return;
						} else if (data.Handles.BrakeNotch <= this.Train.Specs.AtsNotch) {
							data.Handles.BrakeNotch++;
							data.Response = data.Handles.BrakeNotch <= this.Train.Specs.AtsNotch ? AIResponse.Short : AIResponse.Long;
							return;
						}
					}
				} else if (this.Train.AtsP.State == AtsP.States.Brake) {
					if (data.Handles.PowerNotch > 0) {
						data.Handles.PowerNotch--;
						data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (Math.Abs(this.Train.State.Speed.MetersPerSecond) < 1.0 / 3.6) {
						if (data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches) {
							data.Handles.BrakeNotch++;
							data.Response = data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches ? AIResponse.Short : AIResponse.Medium;
							return;
						} else if (data.Handles.Reverser != 0) {
							data.Handles.Reverser = 0;
							data.Response = AIResponse.Medium;
							return;
						} else {
							this.Train.KeyDown(VirtualKeys.B1);
							data.Response = AIResponse.Long;
							return;
						}
					}
				} else if (this.Train.AtsP.State == AtsP.States.Service | this.Train.AtsP.State == AtsP.States.Emergency) {
					if (data.Handles.PowerNotch > 0) {
						data.Handles.PowerNotch--;
						data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches) {
						data.Handles.BrakeNotch++;
						data.Response = data.Handles.BrakeNotch < this.Train.Specs.BrakeNotches ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.Reverser != 0) {
						data.Handles.Reverser = 0;
						data.Response = AIResponse.Medium;
						return;
					} else if (Math.Abs(this.Train.State.Speed.KilometersPerHour) < 1.0) {
						this.Train.KeyDown(VirtualKeys.B1);
						data.Response = AIResponse.Long;
						return;
					} else {
						data.Response = AIResponse.Long;
						return;
					}
				}
			}
			// --- atc ---
			if (this.Train.Atc != null) {
				if (this.Train.Atc.ShouldSwitchToAts()) {
					if (this.Train.AtsSx != null | this.Train.AtsP != null) {
						if (this.Train.Atc.State == Atc.States.Normal | this.Train.Atc.State == Atc.States.Service | this.Train.Atc.State == Atc.States.Emergency) {
							this.Train.KeyDown(VirtualKeys.C1);
							data.Response = AIResponse.Long;
							return;
						}
					} else if (this.Train.Atc.State != Atc.States.Disabled) {
						this.Train.KeyDown(VirtualKeys.E);
						data.Response = AIResponse.Long;
						return;
					}
				} else if (this.Train.Atc.ShouldSwitchToAtc()) {
					if (this.Train.Atc.State == Atc.States.Disabled) {
						this.Train.KeyDown(VirtualKeys.D);
						this.AtcProbing = this.Train.State.Speed.MetersPerSecond < 0.1 / 3.6;
						data.Response = AIResponse.Long;
						return;
					} else {
						this.Train.KeyDown(VirtualKeys.C2);
						data.Response = AIResponse.Long;
						return;
					}
				} else if (this.Train.Atc.State == Atc.States.Disabled & this.Train.State.Speed.MetersPerSecond < 0.1 / 3.6 & !this.AtcProbing) {
					this.Train.KeyDown(VirtualKeys.D);
					this.AtcProbing = true;
					data.Response = AIResponse.Long;
					return;
				} else if (this.AtcProbing & this.Train.State.Speed.MetersPerSecond > 10.0 / 3.6) {
					this.AtcProbing = false;
				} else if (this.Train.Atc.State == Atc.States.Normal | this.Train.Atc.State == Atc.States.Service) {
					if (this.Train.State.Speed.KilometersPerHour > 15.0) {
						if (this.Train.State.Speed.MetersPerSecond > this.Train.Atc.CurrentAtcSpeed - 5.0 / 3.6) {
							if (data.Handles.PowerNotch > 0) {
								data.Handles.PowerNotch--;
								data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
								return;
							} else if (data.Handles.BrakeNotch <= this.Train.Specs.AtsNotch) {
								data.Handles.BrakeNotch++;
								data.Response = data.Handles.BrakeNotch <= this.Train.Specs.AtsNotch ? AIResponse.Short : AIResponse.Long;
								return;
							}
						} else if (this.Train.State.Speed.MetersPerSecond > this.Train.Atc.CurrentAtcSpeed - 10.0 / 3.6) {
							if (data.Handles.PowerNotch > 0) {
								data.Handles.PowerNotch--;
								data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Long;
								return;
							}
						}
						if (this.Train.Atc.CurrentAtcSpeed == 0.0) {
							data.Response = AIResponse.Long;
						}
					}
				} else if (this.Train.Atc.State == Atc.States.Emergency) {
					if (data.Handles.PowerNotch > 0) {
						data.Handles.PowerNotch--;
						data.Response = data.Handles.PowerNotch > 0 ? AIResponse.Short : AIResponse.Medium;
						return;
					} else if (data.Handles.BrakeNotch < this.Train.Specs.B67Notch) {
						data.Handles.BrakeNotch++;
						data.Response = data.Handles.BrakeNotch < this.Train.Specs.B67Notch ? AIResponse.Short : AIResponse.Medium;
						return;
					} else {
						data.Response = AIResponse.Long;
						return;
					}
				}
			}
			// --- eb ---
			if (this.Train.Eb != null) {
				if (this.Train.Eb.Counter >= this.Train.Eb.TimeUntilBell) {
					this.Train.KeyDown(VirtualKeys.A2);
					data.Response = AIResponse.Long;
				}
			}
		}
		
	}
}