namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Moves the air brake handle</summary>
		/// <param name="Train">The train</param>
		/// <param name="RelativeDirection">The direction: -1 for decrease, 1 for increase</param>
		internal static void ApplyAirBrakeHandle(Train Train, int RelativeDirection)
		{
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
			{
				if (RelativeDirection == -1)
				{
					if (Train.Handles.AirBrake.Handle.Driver == AirBrakeHandleState.Service)
					{
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Lap);
					}
					else
					{
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Release);
					}
				}
				else if (RelativeDirection == 1)
				{
					if (Train.Handles.AirBrake.Handle.Driver == AirBrakeHandleState.Release)
					{
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Lap);
					}
					else
					{
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
					}
				}
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
			}
		}

		/// <summary>Moves the air brake handle to the specified state</summary>
		/// <param name="Train">The train</param>
		/// <param name="State">The state</param>
		internal static void ApplyAirBrakeHandle(Train Train, AirBrakeHandleState State)
		{
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
			{
				if (State != Train.Handles.AirBrake.Handle.Driver)
				{
					// sound when moved to service
					if (State == AirBrakeHandleState.Service)
					{
						Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Brake.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Brake.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
						}
					}
					// sound
					if ((int)State < (int)Train.Handles.AirBrake.Handle.Driver)
					{
						// brake release
						if ((int)State > 0)
						{
							// brake release (not min)
							Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
							if (buffer != null)
							{
								OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
							}
						}
						else
						{
							// brake min
							Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Buffer;
							if (buffer != null)
							{
								OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
							}
						}
					}
					else if ((int)State > (int)Train.Handles.AirBrake.Handle.Driver)
					{
						// brake
						Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
						}
					}
					// apply
					Train.Handles.AirBrake.Handle.Driver = State;
					Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
					// plugin
					if (Train.Plugin != null)
					{
						Train.Plugin.UpdatePower();
						Train.Plugin.UpdateBrake();
					}
				}
			}
		}
	}
}
