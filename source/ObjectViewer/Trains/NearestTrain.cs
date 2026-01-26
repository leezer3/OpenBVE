using System;
using System.Collections.Generic;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Trains;

namespace ObjectViewer.Trains
{
	/// <summary>
	/// A class that represents the train nearest to the object
	/// </summary>
	internal static class NearestTrain
	{
		/// <summary>
		/// A synchronization object to be use when writing to members of this class
		/// </summary>
		internal static readonly object LockObj;

		internal static bool IsExtensionsCfg;
		internal static bool EnableSimulation;
		internal static bool EnablePluginSimulation;

		/// <summary>
		/// Whether to apply the status in the next frame
		/// </summary>
		internal static bool RequiredApply;

		internal static NearestTrainSpecs Specs;
		internal static NearestTrainStatus Status;

		static NearestTrain()
		{
			LockObj = new object();
			Specs = new NearestTrainSpecs();
			Status = new NearestTrainStatus();
		}

		/// <summary>
		/// Creates a dummy train from the specs
		/// </summary>
		/// <returns>A dummy train</returns>
		private static TrainBase CreateDummyTrain()
		{
			TrainBase train = new TrainBase(TrainState.Available, TrainType.LocalPlayerTrain);
			train.Handles.Power = new PowerHandle(Specs.PowerNotches, train);
			if (Specs.IsAirBrake)
			{
				train.Handles.Brake = new AirBrakeHandle(train);
			}
			else
			{
				train.Handles.Brake = new BrakeHandle(Specs.BrakeNotches, null, train);
				train.Handles.HasHoldBrake = Specs.HasHoldBrake;
			}
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Specs.HasConstSpeed = Specs.HasConstSpeed;

			Array.Resize(ref train.Cars, Specs.NumberOfCars);
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i] = new CarBase(train, i);

				if (Specs.IsAirBrake)
				{
					train.Cars[i].CarBrake = new AutomaticAirBrake(EletropneumaticBrakeType.None, train.Cars[i]);
				}
				else
				{
					train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Cars[i]);
				}

				train.Cars[i].TractionModel = new BVEMotorCar(train.Cars[i], null);
				//At the minute, Object Viewer uses dummy brake systems
				train.Cars[i].CarBrake.MainReservoir = new MainReservoir(Status.MainReservoirPressure * 1000.0);
				train.Cars[i].CarBrake.EqualizingReservoir = new EqualizingReservoir(Status.EqualizingReservoirPressure * 1000.0);
				train.Cars[i].CarBrake.BrakePipe = new BrakePipe(Status.BrakePipePressure * 1000.0);
				train.Cars[i].CarBrake.BrakeCylinder = new BrakeCylinder(Status.BrakeCylinderPressure * 1000.0);
				AirBrake airBrake = train.Cars[i].CarBrake as AirBrake;
				airBrake.StraightAirPipe = new StraightAirPipe(Status.StraightAirPipePressure * 1000.0);

				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i], i < train.Cars.Length - 1 ? train.Cars[i + 1] : null);

				train.Cars[i].Doors[0] = new Door(-1, 1000.0, 0.0);
				train.Cars[i].Doors[1] = new Door(1, 1000.0, 0.0);
			}

			if (Specs.HasLocoBrake)
			{
				train.Handles.HasLocoBrake = true;
				train.Handles.LocoBrake = new LocoBrakeHandle(Specs.LocoBrakeNotches, train.Handles.EmergencyBrake, new double[] { }, new double[] { }, train);
			}
			else
			{
				train.Handles.HasLocoBrake = false;
				train.Handles.LocoBrake = null;
			}
			return train;
		}

		/// <summary>
		/// Updates the specs from the loaded object
		/// </summary>
		/// <remarks>Calls only from the main UI thread.</remarks>
		internal static void UpdateSpecs()
		{
			lock (LockObj)
			{
				IsExtensionsCfg = Program.TrainManager.Trains.Count != 0;

				if (IsExtensionsCfg)
				{
					TrainBase train = Program.TrainManager.Trains[0];
					Specs = new NearestTrainSpecs
					{
						NumberOfCars = train.NumberOfCars,
						PowerNotches = train.Handles.Power.MaximumNotch,
						IsAirBrake = train.Handles.Brake is AirBrakeHandle,
						BrakeNotches = train.Handles.Brake.MaximumNotch,
						HasHoldBrake = train.Handles.HasHoldBrake,
						HasConstSpeed = train.Handles.HasHoldBrake,
					};

					if (train.Handles.HasLocoBrake)
					{
						Specs.HasLocoBrake = true;
						Specs.LocoBrakeNotches = train.Handles.LocoBrake.MaximumNotch;
					}
				}


				Status.PowerNotch = Math.Min(Status.PowerNotch, Specs.PowerNotches);
				Status.BrakeNotch = Math.Min(Status.BrakeNotch, Specs.BrakeNotches);
				Status.LocoBrakeNotch = Math.Min(Status.LocoBrakeNotch, Specs.LocoBrakeNotches);
				

				if (Specs.IsAirBrake || !Specs.HasHoldBrake)
				{
					Status.HoldBrake = false;
				}

				if (!Specs.HasConstSpeed)
				{
					Status.ConstSpeed = false;
				}

				RequiredApply = true;
			}

			formTrain.Instance?.UpdateSpecsUI();
		}

		/// <summary>
		/// Applies the status
		/// </summary>
		/// <remarks>Calls only from the main UI thread.</remarks>
		internal static void Apply()
		{
			lock (LockObj)
			{
				if (!RequiredApply)
				{
					return;
				}

				if (EnableSimulation)
				{
					TrainBase train;

					if (IsExtensionsCfg)
					{
						train = Program.TrainManager.Trains[0];
					}
					else
					{
						train = CreateDummyTrain();
						Program.TrainManager.Trains.Clear();
						Program.TrainManager.Trains.Add(train);
						TrainManagerBase.PlayerTrain = train;
					}
					Status.Apply(train);
				}
				else
				{
					// Initialize
					if (IsExtensionsCfg)
					{
						NearestTrainStatus.Initialize(Program.TrainManager.Trains[0]);
					}
					else
					{
						Program.TrainManager.Trains = new List<TrainBase>();
						TrainManagerBase.PlayerTrain = null;
					}

					EnablePluginSimulation = false;
					PluginManager.CurrentPlugin.Panel = new int[0];
				}

				RequiredApply = false;
			}
		}
	}
}
