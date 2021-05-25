using System;
using OpenBve;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;
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
			TrainBase train = new TrainBase(TrainState.Available);

			train.Handles.Reverser = new ReverserHandle(train);
			train.Handles.Power = new PowerHandle(Specs.PowerNotches, Specs.PowerNotches, new double[] { }, new double[] { }, train);
			if (Specs.IsAirBrake)
			{
				train.Handles.Brake = new AirBrakeHandle(train);
			}
			else
			{
				train.Handles.Brake = new BrakeHandle(Specs.BrakeNotches, Specs.BrakeNotches, null, new double[] { }, new double[] { }, train);
				train.Handles.HasHoldBrake = Specs.HasHoldBrake;
			}
			train.Handles.EmergencyBrake = new EmergencyHandle(train);
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Specs.HasConstSpeed = Specs.HasConstSpeed;

			Array.Resize(ref train.Cars, Specs.NumberOfCars);
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i] = new CarBase(train, i);
				train.Cars[i].Specs = new CarPhysics();

				if (Specs.IsAirBrake)
				{
					train.Cars[i].CarBrake = new AutomaticAirBrake(EletropneumaticBrakeType.None, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0.0, 0.0, new AccelerationCurve[] { });
				}
				else
				{
					train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0.0, 0.0, new AccelerationCurve[] { });
				}
				//At the minute, Object Viewer uses dummy brake systems
				train.Cars[i].CarBrake.mainReservoir = new MainReservoir(Status.MainReservoirPressure * 1000.0);
				train.Cars[i].CarBrake.equalizingReservoir = new EqualizingReservoir(Status.EqualizingReservoirPressure * 1000.0);
				train.Cars[i].CarBrake.brakePipe = new BrakePipe(Status.BrakePipePressure * 1000.0);
				train.Cars[i].CarBrake.brakeCylinder = new BrakeCylinder(Status.BrakeCylinderPressure * 1000.0);
				train.Cars[i].CarBrake.straightAirPipe = new StraightAirPipe(Status.StraightAirPipePressure * 1000.0);

				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[i / 2 + 1] : null, train);

				train.Cars[i].Doors[0] = new Door(-1, 1000.0, 0.0);
				train.Cars[i].Doors[1] = new Door(1, 1000.0, 0.0);
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
				IsExtensionsCfg = Program.TrainManager.Trains.Length != 0;

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
						HasConstSpeed = train.Handles.HasHoldBrake
					};
				}

				if (Status.PowerNotch > Specs.PowerNotches)
				{
					Status.PowerNotch = Specs.PowerNotches;
				}

				if (Status.BrakeNotch > Specs.BrakeNotches)
				{
					Status.BrakeNotch = Specs.BrakeNotches;
				}

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
						Array.Resize(ref Program.TrainManager.Trains, 1);
						Program.TrainManager.Trains[0] = train;
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
						Program.TrainManager.Trains = new TrainBase[0];
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
