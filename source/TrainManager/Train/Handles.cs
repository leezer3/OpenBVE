using TrainManager.Handles;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	/// <summary>The root class for a train within the simulation</summary>
		public partial class TrainBase
		{
			/// <summary>Applies a loco brake notch to this train</summary>
			/// <param name="NotchValue">The loco brake notch value</param>
			/// <param name="Relative">Whether this is relative to the current notch</param>
			public void ApplyLocoBrakeNotch(int NotchValue, bool Relative)
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
			
			/// <summary>Moves the air brake handle to the specified state</summary>
			/// <param name="newState">The state</param>
			public void ApplyLocoAirBrakeHandle(AirBrakeHandleState newState)
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
						TrainManagerBase.currentHost.AddBlackBoxEntry();
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
