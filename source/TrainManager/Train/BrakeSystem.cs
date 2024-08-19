﻿using System;
using TrainManager.BrakeSystems;
using TrainManager.Handles;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		/// <summary>Updates the brake system for the entire train</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">An array containing the deceleration figures generated by the brake system of each car in the train</param>
		/// <param name="DecelerationDueToMotor">An array containing the deceleration figures generated by the motor of each car in the train (If it is a motor car)</param>
		public void UpdateBrakeSystem(double TimeElapsed, out double[] DecelerationDueToBrake, out double[] DecelerationDueToMotor)
		{
			// individual brake systems
			DecelerationDueToBrake = new double[Cars.Length];
			DecelerationDueToMotor = new double[Cars.Length];
			for (int i = 0; i < Cars.Length; i++)
			{
				UpdateBrakeSystem(i, TimeElapsed, out DecelerationDueToBrake[i], out DecelerationDueToMotor[i]);
			}

			if (Specs.AveragesPressureDistribution)
			{
				// brake pipe pressure distribution dummy (just averages)
				double TotalPressure = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					if (i > 0)
					{
						if (Cars[i - 1].Derailed | Cars[i].Derailed)
						{
							Cars[i].CarBrake.brakePipe.CurrentPressure -= Cars[i].CarBrake.brakePipe.LeakRate * TimeElapsed;
							if (Cars[i].CarBrake.brakePipe.CurrentPressure < 0.0) Cars[i].CarBrake.brakePipe.CurrentPressure = 0.0;
						}
					}

					if (i < Cars.Length - 1)
					{
						if (Cars[i].Derailed | Cars[i + 1].Derailed)
						{
							Cars[i].CarBrake.brakePipe.CurrentPressure -= Cars[i].CarBrake.brakePipe.LeakRate * TimeElapsed;
							if (Cars[i].CarBrake.brakePipe.CurrentPressure < 0.0) Cars[i].CarBrake.brakePipe.CurrentPressure = 0.0;
						}
					}

					TotalPressure += Cars[i].CarBrake.brakePipe.CurrentPressure;
				}

				double averagePressure = TotalPressure / Cars.Length;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].CarBrake.brakePipe.CurrentPressure = averagePressure;
				}
			}
			else
			{

				/*
				 * Notes:
				 * This algorithm will have a 1 frame (~10ms at 100fps) delay
				 * in pressure changes rippling down the pipe
				 *
				 * For the minute (subject to change), assuming that the charge rate is
				 * the total from *both* ends of the pipe, so divide by 2
				 *
				 */

				int lastMainBrake = 0;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].CarBrake.brakePipe.CurrentPressure -= Cars[i].CarBrake.brakePipe.LeakRate * TimeElapsed;
					if (Cars[i].CarBrake.brakePipe.CurrentPressure < 0.0) Cars[i].CarBrake.brakePipe.CurrentPressure = 0.0;
					if (Cars[i].CarBrake.brakeType == BrakeType.Main)
					{
						// If at the end of the train or a compressor we don't need to flow into it
						bool nextCarIsMainBrake = i == Cars.Length - 1 || Cars[i + 1].CarBrake.brakeType == BrakeType.Main;
						
						if (i > 0)
						{
							// Back flow
							for (int j = i; j > lastMainBrake; j--)
							{
								double pressureChange = Math.Min(Cars[j].CarBrake.brakePipe.NormalPressure - Cars[j].CarBrake.brakePipe.CurrentPressure, Cars[j].CarBrake.brakePipe.ChargeRate / 2) * TimeElapsed;
								double pressureDiff = Math.Min(pressureChange, Cars[j].CarBrake.brakePipe.CurrentPressure) * TimeElapsed;
								Cars[i].CarBrake.brakePipe.CurrentPressure -= pressureDiff;
								Cars[j].CarBrake.brakePipe.CurrentPressure += pressureDiff;
							}
						}
						// Forwards flow
						for (int j = i; j < Cars.Length; j++)
						{
							double pressureChange = Math.Min(Cars[j].CarBrake.brakePipe.NormalPressure - Cars[j].CarBrake.brakePipe.CurrentPressure, Cars[j].CarBrake.brakePipe.ChargeRate / 2) * TimeElapsed;
							double pressureDiff = Math.Min(pressureChange, Cars[j].CarBrake.brakePipe.CurrentPressure) * TimeElapsed;
							Cars[i].CarBrake.brakePipe.CurrentPressure -= pressureDiff;
							Cars[j].CarBrake.brakePipe.CurrentPressure += pressureDiff;
							if (nextCarIsMainBrake)
							{
								break;
							}
						}
						lastMainBrake = i;
					}
				}
			}
		}

		/// <summary>Updates the brake system for a car within this train</summary>
		/// <remarks>This must remain a property of the train, for easy access to various base properties</remarks>
		/// <param name="CarIndex">The individual car</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">The total brake deceleration this car provides</param>
		/// <param name="DecelerationDueToMotor">The total motor deceleration this car provides</param>
		public void UpdateBrakeSystem(int CarIndex, double TimeElapsed, out double DecelerationDueToBrake, out double DecelerationDueToMotor)
		{
			DecelerationDueToBrake = 0.0;
			DecelerationDueToMotor = 0.0;
			// air compressor
			if (Cars[CarIndex].CarBrake.brakeType == BrakeType.Main)
			{
				Cars[CarIndex].CarBrake.airCompressor.Update(TimeElapsed);
			}

			if (CarIndex == DriverCar && Handles.HasLocoBrake)
			{
				switch (Handles.LocoBrakeType)
				{
					case LocoBrakeType.Independant:
						//With an independant Loco brake, we always want to use this handle
						Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
						break;
					case LocoBrakeType.Combined:
						if (Handles.LocoBrake is LocoBrakeHandle && Handles.Brake is NotchedHandle)
						{
							//Both handles are of the notched type
							if (Handles.Brake.MaximumNotch == Handles.LocoBrake.MaximumNotch)
							{
								//Identical number of notches, so return the handle with the higher setting
								if (Handles.LocoBrake.Actual >= Handles.Brake.Actual)
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
								}
								else
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
								}
							}
							else if (Handles.Brake.MaximumNotch > Handles.LocoBrake.MaximumNotch)
							{
								double nc = ((double) Handles.LocoBrake.Actual / Handles.LocoBrake.MaximumNotch) * Handles.Brake.MaximumNotch;
								if (nc > Handles.Brake.Actual)
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
								}
								else
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
								}
							}
							else
							{
								double nc = ((double) Handles.Brake.Actual / Handles.Brake.MaximumNotch) * Handles.LocoBrake.MaximumNotch;
								if (nc > Handles.LocoBrake.Actual)
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
								}
								else
								{
									Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
								}
							}
						}
						else if (Handles.LocoBrake is LocoAirBrakeHandle && Handles.Brake is AirBrakeHandle)
						{
							if (Handles.LocoBrake.Actual < Handles.Brake.Actual)
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
							}
							else
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
							}
						}
						else
						{
							double p, tp;
							//Calculate the pressure differentials for the two handles
							if (Handles.LocoBrake is LocoAirBrakeHandle)
							{
								//Air brake handle
								p = Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
								tp = (Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure / Handles.Brake.MaximumNotch) * Handles.Brake.Actual;
							}
							else
							{
								//Notched handle
								p = Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
								tp = (Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure / Handles.LocoBrake.MaximumNotch) * Handles.LocoBrake.Actual;
							}

							if (p < tp)
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
							}
							else
							{
								Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
							}
						}

						break;
					case LocoBrakeType.Blocking:
						if (Handles.LocoBrake.Actual != 0)
						{
							Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.LocoBrake, out DecelerationDueToBrake);
						}
						else
						{
							Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
						}

						break;
				}

			}
			else
			{
				Cars[CarIndex].CarBrake.Update(TimeElapsed, Cars[DriverCar].CurrentSpeed, Handles.Brake, out DecelerationDueToBrake);
			}

			if (Cars[CarIndex].CarBrake.airSound != null)
			{
				Cars[CarIndex].CarBrake.airSound.Play(Cars[CarIndex], false);

			}

			// deceleration provided by motor
			if (!(Cars[CarIndex].CarBrake is AutomaticAirBrake) && Math.Abs(Cars[CarIndex].CurrentSpeed) >= Cars[CarIndex].CarBrake.brakeControlSpeed & Handles.Reverser.Actual != 0 & !Handles.EmergencyBrake.Actual)
			{
				if (Handles.LocoBrake.Actual != 0 && CarIndex == DriverCar)
				{
					DecelerationDueToMotor = Cars[CarIndex].CarBrake.CurrentMotorDeceleration(TimeElapsed, Handles.LocoBrake);
				}
				else
				{
					DecelerationDueToMotor = Cars[CarIndex].CarBrake.CurrentMotorDeceleration(TimeElapsed, Handles.Brake);
				}
			}

			// hold brake
			Cars[CarIndex].HoldBrake.Update(ref DecelerationDueToMotor, Handles.HoldBrake.Actual);
			if(Cars[CarIndex].CarBrake.brakeType != BrakeType.None)
			{
				// brake shoe rub sound
				double spd = Math.Abs(Cars[CarIndex].CurrentSpeed);
				double pitch = 1.0 / (spd + 1.0) + 1.0;
				double gain = Cars[CarIndex].Derailed ? 0.0 : Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure / Cars[CarIndex].CarBrake.brakeCylinder.ServiceMaximumPressure;
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

				if (Cars[CarIndex].CarBrake.Rub.IsPlaying)
				{
					if (pitch > 0.01 & gain > 0.001)
					{
						Cars[CarIndex].CarBrake.Rub.Source.Pitch = pitch;
						Cars[CarIndex].CarBrake.Rub.Source.Volume = gain;
					}
					else
					{
						Cars[CarIndex].CarBrake.Rub.Stop();
					}
				}
				else if (pitch > 0.02 & gain > 0.01)
				{
					Cars[CarIndex].CarBrake.Rub.Play(pitch, gain, Cars[CarIndex], true);
				}
			}
		}
	}
}
