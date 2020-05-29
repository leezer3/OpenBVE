using OpenBveApi.Math;
using OpenBveApi.Sounds;
using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
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
						CarSound sound;
						if ((Handles.Power.Driver - p > 2 | Handles.Power.ContinuousMovement) && Handles.Power.DecreaseFast.Buffer != null)
						{
							sound = Handles.Power.DecreaseFast;
						}
						else
						{
							sound = Handles.Power.Decrease;
						}
						sound.Play(1.0, 1.0, false);
					}
					else
					{
						// min
						SoundBuffer buffer = Handles.Power.Min.Buffer;
						if (buffer != null)
						{
							Handles.Power.Min.Play(1.0, 1.0, false);
						}
					}
				}
				else if (p > Handles.Power.Driver)
				{
					if (p < Handles.Power.MaximumDriverNotch)
					{
						// up (not max)
						CarSound sound;
						if ((Handles.Power.Driver - p > 2 | Handles.Power.ContinuousMovement) && Handles.Power.IncreaseFast.Buffer != null)
						{
							sound = Handles.Power.IncreaseFast;
						}
						else
						{
							sound = Handles.Power.Increase;
						}

						sound.Play(1.0, 1.0, false);
					}
					else
					{
						// max
						Handles.Power.Max.Play(1.0, 1.0, false);
					}
				}

				// brake sound
				if (b < Handles.Brake.Driver)
				{
					// brake release
					SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						Cars[DriverCar].Sounds.Brake.Play(1.0, 1.0, false);
					}

					if (b > 0)
					{
						// brake release (not min)
						if ((Handles.Brake.Driver - b > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.DecreaseFast.Buffer != null)
						{
							buffer = Handles.Brake.DecreaseFast.Buffer;
						}
						else
						{
							buffer = Handles.Brake.Decrease.Buffer;
						}
						if (buffer != null)
						{
							Handles.Brake.Decrease.Play(1.0, 1.0, false);
						}
					}
					else
					{
						// brake min
						buffer = Handles.Brake.Min.Buffer;
						if (buffer != null)
						{
							Handles.Brake.Min.Play(1.0, 1.0, false);
						}
					}
				}
				else if (b > Handles.Brake.Driver)
				{
					// brake
					SoundBuffer buffer;
					if ((b - Handles.Brake.Driver > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.IncreaseFast.Buffer != null)
					{
						buffer = Handles.Brake.IncreaseFast.Buffer;
					}
					else
					{
						buffer = Handles.Brake.Increase.Buffer;
					}
					
					if (buffer != null)
					{
						Handles.Brake.Increase.Play(1.0, 1.0, false);
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
					SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						Cars[DriverCar].Sounds.Brake.Play(1.0, 1.0, false);
					}

					if (b > 0)
					{
						// brake release (not min) 
						if ((Handles.LocoBrake.Driver - b > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.DecreaseFast.Buffer != null)
						{
							buffer = Handles.Brake.DecreaseFast.Buffer;
						}
						else
						{
							buffer = Handles.Brake.Decrease.Buffer;
						}
						if (buffer != null)
						{
							Handles.Brake.Decrease.Play(1.0, 1.0, false);
						}
					}
					else
					{
						// brake min 
						buffer = Handles.Brake.Min.Buffer;
						if (buffer != null)
						{
							Handles.Brake.Min.Play(1.0, 1.0, false);
						}
					}
				}
				else if (b > Handles.LocoBrake.Driver)
				{
					// brake 
					SoundBuffer buffer;
					if ((b - Handles.LocoBrake.Driver > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.IncreaseFast.Buffer != null)
					{
						buffer = Handles.Brake.IncreaseFast.Buffer;
					}
					else
					{
						buffer = Handles.Brake.Increase.Buffer;
					}
					if (buffer != null)
					{
						Handles.Brake.Increase.Play(1.0, 1.0, false);
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
						Handles.Reverser.EngageSound.Play(1.0, 1.0, false);
					}
					else if (a != 0 & r == 0)
					{
						Handles.Reverser.ReleaseSound.Play(1.0, 1.0, false);
					}
				}
			}

			/// <summary>Applies the emergency brake</summary>
			internal void ApplyEmergencyBrake()
			{
				// sound
				if (!Handles.EmergencyBrake.Driver)
				{
					Handles.Brake.Max.Play(1.0, 1.0, false);
					for (int i = 0; i < Cars.Length; i++)
					{
						//BUG: This should be attached to every car, not the driver car I presume
						//Broken from Michelle or us?
						Handles.EmergencyBrake.ApplicationSound.Play(1.0, 1.0, false);
					}
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
					CarSound sound;
					if (Handles.EmergencyBrake.ReleaseSound != null)
					{
						sound = Handles.EmergencyBrake.ReleaseSound;
					}
					else
					{
						sound = Handles.Brake.Decrease;
					}
					sound.Play(1.0, 1.0, false);


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
							Cars[DriverCar].Sounds.Brake.Play(1.0, 1.0, false);
						}

						// sound
						if ((int) newState < (int) Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								CarSound sound;
								if ((Handles.Brake.Driver - (int)newState > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.Decrease.Buffer != null)
								{
									sound = Handles.Brake.DecreaseFast;
								}
								else
								{
									sound = Handles.Brake.Decrease;
								}
								sound.Play(1.0,1.0, false);
							}
							else
							{
								// brake min
								Handles.Brake.Min.Play(1.0, 1.0, false);
							}
						}
						else if ((int) newState > (int) Handles.Brake.Driver)
						{
							// brake
							SoundBuffer buffer;
							if (((int)newState - (int)Handles.Brake.Driver > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.IncreaseFast.Buffer != null)
							{
								buffer = Handles.Brake.IncreaseFast.Buffer;
							}
							else
							{
								buffer = Handles.Brake.Increase.Buffer;
							}
							if (buffer != null)
							{
								Handles.Brake.Increase.Play(1.0, 1.0, false);
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
							Cars[DriverCar].Sounds.Brake.Play(1.0, 1.0, false);
						}

						// sound
						if ((int) newState < (int) Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								CarSound sound;
								if ((Handles.Brake.Driver - (int)newState > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.DecreaseFast.Buffer != null)
								{
									sound = Handles.Brake.DecreaseFast;
								}
								else
								{
									sound = Handles.Brake.Decrease;
								}
								sound.Play(1.0, 1.0, false);
							}
							else
							{
								// brake min
								Handles.Brake.Min.Play(1.0, 1.0, false);
							}
						}
						else if ((int) newState > (int) Handles.LocoBrake.Driver)
						{
							// brake
							SoundBuffer buffer;
							if (((int)newState - (int)Handles.LocoBrake.Driver > 2 | Handles.Brake.ContinuousMovement) && Handles.Brake.IncreaseFast.Buffer != null)
							{
								buffer = Handles.Brake.IncreaseFast.Buffer;
							}
							else
							{
								buffer = Handles.Brake.Increase.Buffer;
							}
							if (buffer != null)
							{
								Handles.Brake.Increase.Play(1.0, 1.0, false);
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
