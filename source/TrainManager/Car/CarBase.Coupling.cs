using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Camera;
using LibRender2.Cameras;
using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.World;
using TrainManager.Brake;
using TrainManager.BrakeSystems;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace TrainManager.Car
{
	public partial class CarBase : AbstractCar
	{
		public override void Uncouple(bool Front, bool Rear)
		{
			lock (baseTrain.updateLock)
			{
				if (!Front && !Rear)
					return;

				// Create new train
				TrainBase newTrain = new TrainBase(TrainState.Available, TrainType.StaticCars);
				UncouplingBehaviour uncouplingBehaviour = UncouplingBehaviour.Emergency;
				newTrain.Handles.Power = new PowerHandle(0, newTrain);
				newTrain.Handles.Brake = new BrakeHandle(0, newTrain.Handles.EmergencyBrake, newTrain);
				newTrain.Handles.HoldBrake = new HoldBrakeHandle(newTrain);
				if (Front)
				{
					int totalPreceedingCars = trainCarIndex;
					newTrain.Cars = new CarBase[trainCarIndex];
					for (int i = 0; i < totalPreceedingCars; i++)
					{
						newTrain.Cars[i] = baseTrain.Cars[i];
						newTrain.Cars[i].baseTrain = newTrain;
					}

					for (int i = totalPreceedingCars; i < baseTrain.Cars.Length; i++)
					{
						baseTrain.Cars[i - totalPreceedingCars] = baseTrain.Cars[i];
						baseTrain.Cars[i].Index = i - totalPreceedingCars;
					}

					Array.Resize(ref baseTrain.Cars, baseTrain.Cars.Length - totalPreceedingCars);
					baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.UncoupleSound.Play(baseTrain.Cars[baseTrain.Cars.Length - 1], false);
					uncouplingBehaviour = baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.UncouplingBehaviour;
					TrainManagerBase.currentHost.AddTrain(baseTrain, newTrain, false);

					if (baseTrain.DriverCar - totalPreceedingCars >= 0)
					{
						baseTrain.DriverCar -= totalPreceedingCars;
					}
				}

				if (Rear)
				{
					int totalFollowingCars = baseTrain.Cars.Length - (Index + 1);
					if (totalFollowingCars > 0)
					{
						newTrain.Cars = new CarBase[totalFollowingCars];
						// Move following cars to new train
						for (int i = 0; i < totalFollowingCars; i++)
						{
							newTrain.Cars[i] = baseTrain.Cars[Index + i + 1];
							newTrain.Cars[i].baseTrain = newTrain;
							newTrain.Cars[i].Index = i;
						}

						Array.Resize(ref baseTrain.Cars, baseTrain.Cars.Length - totalFollowingCars);
						baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.ConnectedCar = baseTrain.Cars[baseTrain.Cars.Length - 1];
					}
					else
					{
						return;
					}

					Coupler.UncoupleSound.Play(this, false);
					uncouplingBehaviour = Coupler.UncouplingBehaviour;
					TrainManagerBase.currentHost.AddTrain(baseTrain, newTrain, true);
				}

				for (int i = 0; i < newTrain.Cars.Length; i++)
				{
					/*
					 * Make visible if not part of player train
					 * Otherwise uncoupling from cab then changing to exterior, they will still be hidden
					 *
					 * Need to do this after everything has been done in case objects refer to other bits
					 */
					newTrain.Cars[i].ChangeCarSection(CarSectionType.Exterior);
				}

				if (baseTrain.DriverCar >= baseTrain.Cars.Length)
				{
					/*
					 * The driver car is no longer in the train
					 *
					 * Look for a car with an interior view to substitute
					 * If not found, this will stop at Car 0
					 */

					for (int i = baseTrain.Cars.Length; i > 0; i--)
					{
						baseTrain.DriverCar = i - 1;
						if (!baseTrain.Cars[i - 1].HasInteriorView)
						{
							/*
							 * Set the eye position to something vaguely sensible, rather than leaving it on the rails
							 * Whilst there will be no cab, at least it's a bit more usable like this
							 */
							baseTrain.Cars[i - 1].InteriorCamera = new CameraAlignment()
							{
								Position = new Vector3(0, 2, 0.5 * Length)
							};
						}
						else
						{
							break;
						}
					}
				}

				if (Front)
				{
					// Uncoupling the front will always make the car our first
					baseTrain.CameraCar = 0;
				}
				else
				{
					if (baseTrain.CameraCar >= baseTrain.Cars.Length)
					{
						baseTrain.CameraCar = baseTrain.DriverCar;
					}
				}

				switch (uncouplingBehaviour)
				{
					case UncouplingBehaviour.Emergency:
						baseTrain.Handles.EmergencyBrake.Apply();
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyUncoupledConsist:
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyPlayer:
						baseTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.Released:
						baseTrain.Handles.Brake.ApplyState(0, false);
						baseTrain.Handles.EmergencyBrake.Release();
						newTrain.Handles.Brake.ApplyState(0, false);
						newTrain.Handles.EmergencyBrake.Release();
						break;
					case UncouplingBehaviour.ReleasedUncoupledConsist:
						newTrain.Handles.Brake.ApplyState(0, false);
						newTrain.Handles.EmergencyBrake.Release();
						break;
				}

				baseTrain.Cars[baseTrain.DriverCar].Sounds.UncoupleCab?.Play(1.0, 1.0, baseTrain.Cars[baseTrain.DriverCar], false);
			}
		}
	}
}
