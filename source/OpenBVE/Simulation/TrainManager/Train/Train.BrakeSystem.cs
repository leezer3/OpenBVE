using System;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Updates the brake system for a complete train</summary>
		/// <param name="Train">The train to update the brake system values for</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">An array containing the deceleration due to brakes for each car in the train</param>
		/// <param name="DecelerationDueToMotor">An array containing the deceleration due to motor retardation for each car in the train</param>
		private static void UpdateBrakeSystem(Train Train, double TimeElapsed, out double[] DecelerationDueToBrake, out double[] DecelerationDueToMotor)
		{
			// individual brake systems
			DecelerationDueToBrake = new double[Train.Cars.Length];
			DecelerationDueToMotor = new double[Train.Cars.Length];
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				UpdateBrakeSystem(Train, i, TimeElapsed, out DecelerationDueToBrake[i], out DecelerationDueToMotor[i]);
			}
			// brake pipe pressure distribution dummy (just averages)
			double TotalPressure = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (i > 0)
				{
					if (Train.Cars[i - 1].Derailed | Train.Cars[i].Derailed)
					{
						Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure = 0.0;
					}
				}
				if (i < Train.Cars.Length - 1)
				{
					if (Train.Cars[i].Derailed | Train.Cars[i + 1].Derailed)
					{
						Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure = 0.0;
					}
				}
				TotalPressure += Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure;
			}
			double AveragePressure = TotalPressure / (double)Train.Cars.Length;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.AirBrake.BrakePipe.CurrentPressure = AveragePressure;
			}
		}

		/// <summary>Updates the brake system for a car within a train</summary>
		/// <param name="Train">The train</param>
		/// <param name="CarIndex">The induvidual car</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">The total brake deceleration this car provides</param>
		/// <param name="DecelerationDueToMotor">The total motor deceleration this car provides</param>
		private static void UpdateBrakeSystem(Train Train, int CarIndex, double TimeElapsed, out double DecelerationDueToBrake, out double DecelerationDueToMotor)
		{
			Train.Cars[CarIndex].Specs.AirBrake.UpdateSystem(Train, CarIndex, TimeElapsed);

			// deceleration provided by brake
			double pressureratio = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinder.CurrentPressure / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinder.ServiceMaximumPressure;
			DecelerationDueToBrake = pressureratio * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
			// deceleration provided by motor
			if (Train.Cars[CarIndex].Specs.BrakeType != CarBrakeType.AutomaticAirBrake && Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) >= Train.Cars[CarIndex].Specs.BrakeControlSpeed & Train.Specs.CurrentReverser.Actual != 0 & !Train.Specs.CurrentEmergencyBrake.Actual)
			{
				double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
				double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
				DecelerationDueToMotor = f * a;
			}
			else
			{
				DecelerationDueToMotor = 0.0;
			}
			// hold brake
			if (Train.Specs.CurrentHoldBrake.Actual & DecelerationDueToMotor == 0.0)
			{
				if (Game.SecondsSinceMidnight >= Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime)
				{
					Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime = Game.SecondsSinceMidnight + Train.Cars[CarIndex].Specs.HoldBrake.UpdateInterval;
					Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput += 0.8 * Train.Cars[CarIndex].Specs.CurrentAcceleration * (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed);
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput < 0.0) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
					double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput > a) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = a;
				}
				DecelerationDueToMotor = Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput;
			}
			else
			{
				Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
			}
			{ // rub sound
				Sounds.SoundBuffer buffer;
				buffer = Train.Cars[CarIndex].Sounds.Rub.Buffer;
				if (buffer != null)
				{
					double spd = Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed);
					double pitch = 1.0 / (spd + 1.0) + 1.0;
					double gain = Train.Cars[CarIndex].Derailed ? 0.0 : pressureratio;
					if (spd < 1.38888888888889)
					{
						double t = spd * spd;
						gain *= 1.5552 * t - 0.746496 * spd * t;
					}
					else if (spd > 12.5)
					{
						double t = spd - 12.5;
						const double fadefactor = 0.1;
						gain *= 1.0 / (fadefactor * t * t + 1.0);
					}
					if (Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.Rub.Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Train.Cars[CarIndex].Sounds.Rub.Source.Pitch = pitch;
							Train.Cars[CarIndex].Sounds.Rub.Source.Volume = gain;
						}
						else
						{
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.Rub.Source);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Rub.Position;
						Train.Cars[CarIndex].Sounds.Rub.Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, CarIndex, true);
					}
				}
			}
		}


		/// <summary>Applies the emergency brake</summary>
		/// <param name="Train">The train</param>
		internal static void ApplyEmergencyBrake(Train Train)
		{
			// sound
			if (!Train.Specs.CurrentEmergencyBrake.Driver)
			{
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					buffer = Train.Cars[Train.DriverCar].Sounds.EmrBrake.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.EmrBrake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			// apply
			ApplyNotch(Train, 0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
			ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
			Train.Specs.CurrentEmergencyBrake.Driver = true;
			Train.Specs.CurrentHoldBrake.Driver = false;
			Train.Specs.CurrentConstSpeed = false;
			// plugin
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}

		/// <summary>Releases the emergency brake</summary>
		/// <param name="Train">The train</param>
		internal static void UnapplyEmergencyBrake(Train Train)
		{
			if (Train.Specs.CurrentEmergencyBrake.Driver)
			{
				// sound
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				// apply
				ApplyNotch(Train, 0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
				ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
				Train.Specs.CurrentEmergencyBrake.Driver = false;
				// plugin
				if (Train.Plugin == null) return;
				Train.Plugin.UpdatePower();
				Train.Plugin.UpdateBrake();
			}
		}

		/// <summary>Applies or releases the hold brake</summary>
		/// <param name="Train">The train</param>
		/// <param name="Value">Whether to apply (TRUE) or release (FALSE)</param>
		internal static void ApplyHoldBrake(Train Train, bool Value)
		{
			Train.Specs.CurrentHoldBrake.Driver = Value;
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}
	}
}
