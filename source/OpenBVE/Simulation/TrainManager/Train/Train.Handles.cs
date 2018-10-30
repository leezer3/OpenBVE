using OpenBveApi.Math;

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
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerDown.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerDown.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// min
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMin.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (p > Handles.Power.Driver)
				{
					if (p < Handles.Power.MaximumDriverNotch)
					{
						// up (not max)
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerUp.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerUp.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// max
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMax.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMax.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}

				// brake sound
				if (b < Handles.Brake.Driver)
				{
					// brake release
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}

					if (b > 0)
					{
						// brake release (not min)
						buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// brake min
						buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (b > Handles.Brake.Driver)
				{
					// brake
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
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
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}

					if (b > 0)
					{
						// brake release (not min) 
						buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// brake min 
						buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (b > Handles.LocoBrake.Driver)
				{
					// brake 
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
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
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.ReverserOn.Buffer;
						if (buffer == null) return;
						Vector3 pos = Cars[DriverCar].Sounds.ReverserOn.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}
					else if (a != 0 & r == 0)
					{
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.ReverserOff.Buffer;
						if (buffer == null) return;
						Vector3 pos = Cars[DriverCar].Sounds.ReverserOff.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}
				}
			}

			/// <summary>Applies the emergency brake</summary>
			internal void ApplyEmergencyBrake()
			{
				// sound
				if (!Handles.EmergencyBrake.Driver)
				{
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleMax.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMax.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}

					for (int i = 0; i < Cars.Length; i++)
					{
						buffer = Cars[DriverCar].Sounds.EmrBrake.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[i].Sounds.EmrBrake.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
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
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
					if (buffer != null)
					{
						Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
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
							Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
							if (buffer != null)
							{
								Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
							}
						}

						// sound
						if ((int) newState < (int) Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
								if (buffer != null)
								{
									Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
								}
							}
							else
							{
								// brake min
								Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
								if (buffer != null)
								{
									Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
								}
							}
						}
						else if ((int) newState > (int) Handles.Brake.Driver)
						{
							// brake
							Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
							if (buffer != null)
							{
								Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
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
							Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
							if (buffer != null)
							{
								Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
							}
						}

						// sound
						if ((int) newState < (int) Handles.Brake.Driver)
						{
							// brake release
							if ((int) newState > 0)
							{
								// brake release (not min)
								Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
								if (buffer != null)
								{
									Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
								}
							}
							else
							{
								// brake min
								Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
								if (buffer != null)
								{
									Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
								}
							}
						}
						else if ((int) newState > (int) Handles.LocoBrake.Driver)
						{
							// brake
							Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
							if (buffer != null)
							{
								Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
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
