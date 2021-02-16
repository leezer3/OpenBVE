using TrainManager.Handles;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train
		{
			/// <summary>Applies a power and / or brake notch to this train</summary>
			/// <param name="PowerValue">The power notch value</param>
			/// <param name="PowerRelative">Whether this is relative to the current notch</param>
			/// <param name="BrakeValue">The brake notch value</param>
			/// <param name="BrakeRelative">Whether this is relative to the current notch</param>
			/// <param name="IsOverMaxDriverNotch"></param>
			internal void ApplyNotch(int PowerValue, bool PowerRelative, int BrakeValue, bool BrakeRelative, bool IsOverMaxDriverNotch = false)
			{
				// determine notch
				int p = PowerRelative ? PowerValue + Handles.Power.Driver : PowerValue;
				if (p < 0)
				{
					p = 0;
				}
				else if (p > Handles.Power.MaximumNotch)
				{
					p = Handles.Power.MaximumNotch;
				}
				if (!IsOverMaxDriverNotch && p > Handles.Power.MaximumDriverNotch)
				{
					p = Handles.Power.MaximumDriverNotch;
				}

				int b = BrakeRelative ? BrakeValue + Handles.Brake.Driver : BrakeValue;
				if (b < 0)
				{
					b = 0;
				}
				else if (b > Handles.Brake.MaximumNotch)
				{
					b = Handles.Brake.MaximumNotch;
				}
				if (!IsOverMaxDriverNotch && b > Handles.Brake.MaximumDriverNotch)
				{
					b = Handles.Brake.MaximumDriverNotch;
				}

				// power sound
				if (p < Handles.Power.Driver)
				{
					if (p > 0)
					{
						// down (not min)
						if (Handles.Power.Driver - p > 2 | Handles.Power.ContinuousMovement && Handles.Power.DecreaseFast.Buffer != null)
						{
							Handles.Power.DecreaseFast.Play(Cars[DriverCar], false);
						}
						else
						{
							Handles.Power.Decrease.Play(Cars[DriverCar], false);
						}
					}
					else
					{
						// min
						Handles.Power.Min.Play(Cars[DriverCar], false);
					}
				}
				else if (p > Handles.Power.Driver)
				{
					if (p < Handles.Power.MaximumDriverNotch)
					{
						// up (not max)
						if (Handles.Power.Driver - p > 2 | Handles.Power.ContinuousMovement && Handles.Power.IncreaseFast.Buffer != null)
						{
							Handles.Power.IncreaseFast.Play(Cars[DriverCar], false);
						}
						else
						{
							Handles.Power.Increase.Play(Cars[DriverCar], false);
						}
					}
					else
					{
						// max
						Handles.Power.Max.Play(Cars[DriverCar], false);
					}
				}

				// brake sound
				if (b < Handles.Brake.Driver)
				{
					// brake release
					Cars[DriverCar].CarBrake.Release.Play(Cars[DriverCar], false);
					
					if (b > 0)
					{
						// brake release (not min)
						if (Handles.Brake.Driver - b > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.DecreaseFast.Buffer != null)
						{
							Handles.Brake.DecreaseFast.Play(Cars[DriverCar], false);
						}
						else
						{
							Handles.Brake.Decrease.Play(Cars[DriverCar], false);
						}
					}
					else
					{
						// brake min
						Handles.Brake.Min.Play(Cars[DriverCar], false);
					}
				}
				else if (b > Handles.Brake.Driver)
				{
					// brake
					if (b - Handles.Brake.Driver > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.IncreaseFast.Buffer != null)
					{
						Handles.Brake.IncreaseFast.Play(Cars[DriverCar], false);
					}
					else
					{
						Handles.Brake.Increase.Play(Cars[DriverCar], false);
					}
				}

				// apply notch
				if (Handles.SingleHandle)
				{
					if (b != 0) p = 0;
				}

				Handles.Power.Driver = p;
				Handles.Brake.Driver = b;
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
				// plugin
				if (Plugin != null)
				{
					Plugin.UpdatePower();
					Plugin.UpdateBrake();
				}
			}

			/// <summary>Applies a loco brake notch to this train</summary>
			/// <param name="NotchValue">The loco brake notch value</param>
			/// <param name="Relative">Whether this is relative to the current notch</param>
			internal void ApplyLocoBrakeNotch(int NotchValue, bool Relative)
			{
				int b = Relative ? NotchValue + Handles.LocoBrake.Driver : NotchValue;
				if (b < 0)
				{
					b = 0;
				}
				else if (b > Handles.LocoBrake.MaximumNotch)
				{
					b = Handles.LocoBrake.MaximumNotch;
				}

				// brake sound 
				if (b < Handles.LocoBrake.Driver)
				{
					// brake release 
					Cars[DriverCar].CarBrake.Release.Play(Cars[DriverCar], false);
					if (b > 0)
					{
						// brake release (not min) 
						if (Handles.LocoBrake.Driver - b > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.DecreaseFast.Buffer != null)
						{
							Handles.Brake.DecreaseFast.Play(Cars[DriverCar], false);
						}
						else
						{
							Handles.Brake.Decrease.Play(Cars[DriverCar], false);
						}
					}
					else
					{
						// brake min 
						Handles.Brake.Min.Play(Cars[DriverCar], false);

					}
				}
				else if (b > Handles.LocoBrake.Driver)
				{
					// brake 
					if (b - Handles.LocoBrake.Driver > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.IncreaseFast.Buffer != null)
					{
						Handles.Brake.IncreaseFast.Play(Cars[DriverCar], false);
					}
					else
					{
						Handles.Brake.Increase.Play(Cars[DriverCar], false);
					}
				}

				Handles.LocoBrake.Driver = b;
				Handles.LocoBrake.Actual = b; //TODO: FIXME
			}

			/// <summary>Applies a reverser notch</summary>
			/// <param name="Value">The notch to apply</param>
			/// <param name="Relative">Whether this is an absolute value or relative to the previous</param>
			internal void ApplyReverser(int Value, bool Relative)
			{
				int a = (int)Handles.Reverser.Driver;
				int r = Relative ? a + Value : Value;
				if (r < -1) r = -1;
				if (r > 1) r = 1;
				if (a != r)
				{
					Handles.Reverser.Driver = (ReverserPosition)r;
					if (Plugin != null)
					{
						Plugin.UpdateReverser();
					}
					Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
					// sound
					if (a == 0 & r != 0)
					{
						Handles.Reverser.EngageSound.Play(Cars[DriverCar], false);
					}
					else if (a != 0 & r == 0)
					{
						Handles.Reverser.ReleaseSound.Play(Cars[DriverCar], false);
					}
				}
			}

			/// <summary>Applies the emergency brake</summary>
			internal void ApplyEmergencyBrake()
			{
				// sound
				if (!Handles.EmergencyBrake.Driver)
				{
					Handles.Brake.Max.Play(Cars[DriverCar], false);
					Handles.EmergencyBrake.ApplicationSound.Play(Cars[DriverCar], false);
				}

				// apply
				ApplyNotch(0, !Handles.SingleHandle, Handles.Brake.MaximumNotch, true);
				ApplyAirBrakeHandle(AirBrakeHandleState.Service);
				Handles.EmergencyBrake.Driver = true;
				Handles.HoldBrake.Driver = false;
				Specs.CurrentConstSpeed = false;
				if (Handles.EmergencyBrake.Driver)
				{
					switch (Handles.EmergencyBrake.OtherHandlesBehaviour)
					{
						case EbHandleBehaviour.PowerNeutral:
							if (!Handles.SingleHandle)
							{
								ApplyNotch(0, false, 0, true);
							}

							break;
						case EbHandleBehaviour.ReverserNeutral:
							ApplyReverser(0, false);
							break;
						case EbHandleBehaviour.PowerReverserNeutral:
							if (!Handles.SingleHandle)
							{
								ApplyNotch(0, false, 0, true);
							}

							ApplyReverser(0, false);
							break;
					}
				}

				// plugin
				if (Plugin == null) return;
				Plugin.UpdatePower();
				Plugin.UpdateBrake();
			}

			/// <summary>Releases the emergency brake</summary>
			internal void UnapplyEmergencyBrake()
			{
				if (Handles.EmergencyBrake.Driver)
				{
					// sound
					if (Handles.EmergencyBrake.ReleaseSound != null)
					{
						Handles.EmergencyBrake.ReleaseSound.Play(Cars[DriverCar], false);
					}
					else
					{
						Handles.Brake.Decrease.Play(Cars[DriverCar], false);
					}
					// apply
					
					if (Handles.Brake is AirBrakeHandle)
					{
						ApplyAirBrakeHandle(AirBrakeHandleState.Service);
					}
					else
					{
						ApplyNotch(0, !Handles.SingleHandle, Handles.Brake.MaximumNotch, true);
					}
					Handles.EmergencyBrake.Driver = false;
					// plugin
					if (Plugin == null) return;
					Plugin.UpdatePower();
					Plugin.UpdateBrake();
				}
			}

			/// <summary>Applies or releases the hold brake</summary>
			/// <param name="Value">Whether to apply (TRUE) or release (FALSE)</param>
			internal void ApplyHoldBrake(bool Value)
			{
				Handles.HoldBrake.Driver = Value;
				if (Plugin == null) return;
				Plugin.UpdatePower();
				Plugin.UpdateBrake();
			}

			/// <summary>Moves the air brake handle</summary>
			/// <param name="RelativeDirection">The direction: -1 for decrease, 1 for increase</param>
			internal void ApplyAirBrakeHandle(int RelativeDirection)
			{
				if (Handles.Brake is AirBrakeHandle)
				{
					if (RelativeDirection == -1)
					{
						if (Handles.Brake.Driver == (int) AirBrakeHandleState.Service)
						{
							ApplyAirBrakeHandle(AirBrakeHandleState.Lap);
						}
						else
						{
							ApplyAirBrakeHandle(AirBrakeHandleState.Release);
						}
					}
					else if (RelativeDirection == 1)
					{
						if (Handles.Brake.Driver == (int) AirBrakeHandleState.Release)
						{
							ApplyAirBrakeHandle(AirBrakeHandleState.Lap);
						}
						else
						{
							ApplyAirBrakeHandle(AirBrakeHandleState.Service);
						}
					}

					Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
				}
			}

			/// <summary>Moves the air brake handle to the specified state</summary>
			/// <param name="newState">The new state</param>
			internal void ApplyAirBrakeHandle(AirBrakeHandleState newState)
			{
				if (Handles.Brake is AirBrakeHandle)
				{
					if ((int) newState != Handles.Brake.Driver)
					{
						// sound when moved to service
						if (newState == AirBrakeHandleState.Service)
						{
							Cars[DriverCar].CarBrake.Release.Play(Cars[DriverCar], false);
						}

						// sound
						if ((int) newState < Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								if (Handles.Brake.Driver - (int)newState > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.DecreaseFast.Buffer != null)
								{
									Handles.Brake.DecreaseFast.Play(Cars[DriverCar], false);
								}
								else
								{
									Handles.Brake.Decrease.Play(Cars[DriverCar], false);
								}
							}
							else
							{
								// brake min
								Handles.Brake.Min.Play(Cars[DriverCar], false);
							}
						}
						else if ((int) newState > Handles.Brake.Driver)
						{
							// brake
							if ((int)newState - Handles.Brake.Driver > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.IncreaseFast.Buffer != null)
							{
								Handles.Brake.IncreaseFast.Play(Cars[DriverCar], false);
							}
							else
							{
								Handles.Brake.Increase.Play(Cars[DriverCar], false);
							}
						}

						// apply
						Handles.Brake.Driver = (int) newState;
						Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
						// plugin
						if (Plugin != null)
						{
							Plugin.UpdatePower();
							Plugin.UpdateBrake();
						}
					}
				}
			}

			/// <summary>Moves the air brake handle to the specified state</summary>
			/// <param name="newState">The state</param>
			internal void ApplyLocoAirBrakeHandle(AirBrakeHandleState newState)
			{
				if (Handles.LocoBrake is LocoAirBrakeHandle)
				{
					if ((int) newState != Handles.LocoBrake.Driver)
					{
						// sound when moved to service
						if (newState == AirBrakeHandleState.Service)
						{
							Cars[DriverCar].CarBrake.Release.Play(Cars[DriverCar], false);
						}

						// sound
						if ((int) newState < Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								if (Handles.Brake.Driver - (int)newState > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.DecreaseFast.Buffer != null)
								{
									Handles.Brake.DecreaseFast.Play(Cars[DriverCar], false);
								}
								else
								{
									Handles.Brake.Decrease.Play(Cars[DriverCar], false);
								}
							}
							else
							{
								// brake min
								Handles.Brake.Min.Play(Cars[DriverCar], false);
							}
						}
						else if ((int) newState > Handles.LocoBrake.Driver)
						{
							// brake
							if ((int)newState - Handles.LocoBrake.Driver > 2 | Handles.Brake.ContinuousMovement && Handles.Brake.IncreaseFast.Buffer != null)
							{
								Handles.Brake.IncreaseFast.Play(Cars[DriverCar], false);
							}
							else
							{
								Handles.Brake.Increase.Play(Cars[DriverCar], false);
							}
						}

						// apply
						Handles.LocoBrake.Driver = (int) newState;
						Handles.LocoBrake.Actual = (int) newState; //TODO: FIXME
						Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
						// plugin
						if (Plugin != null)
						{
							Plugin.UpdatePower();
							Plugin.UpdateBrake();
						}
					}
				}
			}
		}
	}
}
