using OpenBveApi.Interface;

namespace TrainManager.Handles
{
	/// <summary>The cab handles (controls) of a train</summary>
	public struct CabHandles
	{
		/// <summary>The Reverser</summary>
		public ReverserHandle Reverser;
		/// <summary>The Power</summary>
		public AbstractHandle Power;
		/// <summary>The Brake</summary>
		public AbstractHandle Brake;
		/// <summary>The Loco brake handle</summary>
		public AbstractHandle LocoBrake;
		/// <summary>The Emergency Brake</summary>
		public EmergencyHandle EmergencyBrake;
		/// <summary>The Hold Brake</summary>
		public HoldBrakeHandle HoldBrake;
		/// <summary>Whether the train has a combined power and brake handle</summary>
		public HandleType HandleType;
		/// <summary>Whether the train has the Hold Brake fitted</summary>
		public bool HasHoldBrake;
		/// <summary>Whether the train has a locomotive brake</summary>
		public bool HasLocoBrake;
		/// <summary>The loco brake type</summary>
		public LocoBrakeType LocoBrakeType;

		public void ControlDown(Control Control)
		{
			switch (Control.Command)
			{
				case Translations.Command.SinglePower:
					// single power
					if (HandleType == HandleType.SingleHandle)
					{
						int b = Brake.Driver;
						if (EmergencyBrake.Driver)
						{
							EmergencyBrake.Release();
						}
						else if (b == 1 & HasHoldBrake)
						{
							Brake.ApplyState(0, false);
							HoldBrake.ApplyState(true);
						}
						else if (HoldBrake.Driver)
						{
							HoldBrake.ApplyState(false);
						}
						else if (b > 0)
						{
							Brake.ApplyState(-1, true);
						}
						else
						{
							int p = Power.Driver;
							if (p < Power.MaximumNotch)
							{
								Power.ApplyState(1, true);
							}
						}
					}

					Power.ContinuousMovement = true;
					break;
				case Translations.Command.SingleNeutral:
					// single neutral
					if (HandleType == HandleType.SingleHandle)
					{
						int p = Power.Driver;
						if (p > 0)
						{
							Power.ApplyState(-1, true);
							Power.ContinuousMovement = true;
						}
						else
						{
							int b = Brake.Driver;
							if (EmergencyBrake.Driver)
							{
								EmergencyBrake.Release();
							}
							else if (b == 1 & HasHoldBrake)
							{
								Brake.ApplyState(0, false);
								HoldBrake.ApplyState(true);
							}
							else if (HoldBrake.Driver)
							{
								HoldBrake.ApplyState(false);
							}
							else if (b > 0)
							{
								Brake.ApplyState(-1, true);
							}

							Brake.ContinuousMovement = true;
						}
					}

					break;
				case Translations.Command.SingleBrake:
					// single brake
					if (HandleType == HandleType.SingleHandle)
					{
						int p = Power.Driver;
						if (p > 0)
						{
							Power.ApplyState(-1, true);
						}
						else
						{
							int b = Brake.Driver;
							if (HasHoldBrake & b == 0 & !HoldBrake.Driver)
							{
								HoldBrake.ApplyState(true);
							}
							else if (b < Brake.MaximumNotch)
							{
								Brake.ApplyState(1, true);
								HoldBrake.ApplyState(false);
							}
						}
					}

					//Set the brake handle fast movement bool at the end of the call in order to not catch it on the first movement
					Brake.ContinuousMovement = true;
					break;
				case Translations.Command.SingleEmergency:
					// single emergency
					if (HandleType == HandleType.SingleHandle)
					{
						EmergencyBrake.Apply();
					}

					break;
				case Translations.Command.PowerIncrease:
					// power increase
					if (HandleType != HandleType.SingleHandle)
					{
						int p = Power.Driver;
						if (p < Power.MaximumNotch)
						{
							Power.ApplyState(1, true);
						}
					}

					Power.ContinuousMovement = true;
					break;
				case Translations.Command.PowerDecrease:
					// power decrease
					if (HandleType != HandleType.SingleHandle)
					{
						int p = Power.Driver;
						if (p > 0)
						{
							Power.ApplyState(-1, true);
						}
					}

					Power.ContinuousMovement = true;
					break;
				case Translations.Command.BrakeIncrease:
					// brake increase
					if (HandleType != HandleType.SingleHandle)
					{
						if (Brake is AirBrakeHandle)
						{
							if (HasHoldBrake & Brake.Driver == (int)AirBrakeHandleState.Release & !HoldBrake.Driver)
							{
								HoldBrake.ApplyState(true);
							}
							else if (HoldBrake.Driver)
							{
								Brake.ApplyState(AirBrakeHandleState.Lap);
								HoldBrake.ApplyState(false);
							}
							else if (Brake.Driver ==
							         (int)AirBrakeHandleState.Lap)
							{
								Brake.ApplyState(AirBrakeHandleState.Service);
							}
							else if (Brake.Driver ==
							         (int)AirBrakeHandleState.Release)
							{
								Brake.ApplyState(AirBrakeHandleState.Lap);
							}
						}
						else
						{
							int b = Brake.Driver;
							if (HasHoldBrake & b == 0 & !HoldBrake.Driver)
							{
								HoldBrake.ApplyState(true);
							}
							else if (b < Brake.MaximumNotch)
							{
								Brake.ApplyState(1, true);
								HoldBrake.ApplyState(false);
							}
						}
					}

					Brake.ContinuousMovement = true;
					break;
				case Translations.Command.BrakeDecrease:
					// brake decrease
					if (HandleType != HandleType.SingleHandle)
					{
						if (Brake is AirBrakeHandle)
						{
							if (EmergencyBrake.Driver)
							{
								EmergencyBrake.Release();
							}
							else if (HasHoldBrake & Brake.Driver == (int)AirBrakeHandleState.Lap & !HoldBrake.Driver)
							{
								HoldBrake.ApplyState(true);
							}
							else if (HoldBrake.Driver)
							{
								Brake.ApplyState(AirBrakeHandleState.Release);
								HoldBrake.ApplyState(false);
							}
							else if (Brake.Driver ==
							         (int)AirBrakeHandleState.Lap)
							{
								Brake.ApplyState(AirBrakeHandleState.Release);
							}
							else if (Brake.Driver ==
							         (int)AirBrakeHandleState.Service)
							{
								Brake.ApplyState(AirBrakeHandleState.Lap);
							}
						}
						else
						{
							int b = Brake.Driver;
							if (EmergencyBrake.Driver)
							{
								EmergencyBrake.Release();
							}
							else if (b == 1 & HasHoldBrake)
							{
								Brake.ApplyState(0, false);
								HoldBrake.ApplyState(true);
							}
							else if (HoldBrake.Driver)
							{
								HoldBrake.ApplyState(false);
							}
							else if (b > 0)
							{
								Brake.ApplyState(-1, true);
							}
						}
					}

					Brake.ContinuousMovement = true;
					break;
				case Translations.Command.LocoBrakeIncrease:
					if (LocoBrake is LocoAirBrakeHandle)
					{
						if (LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
						{
							LocoBrake.ApplyState(AirBrakeHandleState.Service);
						}
						else if (LocoBrake.Driver == (int)AirBrakeHandleState.Release)
						{
							LocoBrake.ApplyState(AirBrakeHandleState.Lap);
						}
					}
					else
					{
						LocoBrake.ApplyState(1, true);
					}
					LocoBrake.ContinuousMovement = true;
					break;
				case Translations.Command.LocoBrakeDecrease:
					if (LocoBrake is LocoAirBrakeHandle)
					{
						if (LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
						{
							LocoBrake.ApplyState(AirBrakeHandleState.Release);
						}
						else if (LocoBrake.Driver == (int)AirBrakeHandleState.Service)
						{
							LocoBrake.ApplyState(AirBrakeHandleState.Lap);
						}
					}
					else
					{
						LocoBrake.ApplyState(-1, true);
					}
					LocoBrake.ContinuousMovement = true;
					break;
				case Translations.Command.BrakeEmergency:
					// brake emergency
					EmergencyBrake.Apply();
					break;
				case Translations.Command.PowerAnyNotch:
					if (HandleType == HandleType.SingleHandle && EmergencyBrake.Driver)
					{
						EmergencyBrake.Release();
					}

					Brake.ApplyState(0, HandleType != HandleType.SingleHandle);
					Power.ApplyState(Control.Option, false);
					break;
				case Translations.Command.BrakeAnyNotch:
					if (Brake is AirBrakeHandle)
					{
						if (EmergencyBrake.Driver)
						{
							EmergencyBrake.Release();
						}

						HoldBrake.ApplyState(false);
						if (Control.Option <= (int)AirBrakeHandleState.Release)
						{
							Brake.ApplyState(AirBrakeHandleState.Release);
						}
						else if (Control.Option == (int)AirBrakeHandleState.Lap)
						{
							Brake.ApplyState(AirBrakeHandleState.Lap);
						}
						else
						{
							Brake.ApplyState(AirBrakeHandleState.Service);
						}
					}
					else
					{
						if (EmergencyBrake.Driver)
						{
							EmergencyBrake.Release();
						}

						HoldBrake.ApplyState(false);
						Brake.ApplyState(Control.Option, false);
						Power.ApplyState(0, HandleType != HandleType.SingleHandle);
					}
					break;
				case Translations.Command.ReverserAnyPosition:
					Reverser.ApplyState((ReverserPosition)Control.Option);
					break;
				case Translations.Command.HoldBrake:
					if (HasHoldBrake && (Brake.Driver == 0 || Brake.Driver == 1) && !HoldBrake.Driver)
					{
						Brake.ApplyState(0, false);
						Power.ApplyState(0, HandleType != HandleType.SingleHandle);
						HoldBrake.ApplyState(true);
					}
					break;
				case Translations.Command.ReverserForward:
					if (Reverser.Driver < ReverserPosition.Forwards)
					{
						Reverser.ApplyState(1, true);
					}
					break;
				case Translations.Command.ReverserBackward:
					// reverser backward
					if (Reverser.Driver > ReverserPosition.Reverse)
					{
						Reverser.ApplyState(-1, true);
					}
					break;
			}
		}

		public void ControlUp(Control Control)
		{
			switch (Control.Command)
			{
				case Translations.Command.SingleBrake:
				case Translations.Command.BrakeIncrease:
				case Translations.Command.BrakeDecrease:
					Brake.ContinuousMovement = false;
					break;
				case Translations.Command.SinglePower:
				case Translations.Command.PowerIncrease:
				case Translations.Command.PowerDecrease:
					Power.ContinuousMovement = false;
					break;
				case Translations.Command.SingleNeutral:
					Brake.ContinuousMovement = false;
					Power.ContinuousMovement = false;
					break;
				case Translations.Command.LocoBrakeIncrease:
				case Translations.Command.LocoBrakeDecrease:
					LocoBrake.ContinuousMovement = false;
					break;
			}
		}
	}
}
