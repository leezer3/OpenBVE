using System.Linq;
using OpenBve;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Trains;

namespace ObjectViewer.Trains
{
	/// <summary>
	/// A class that represents the status of the train nearest to the object
	/// </summary>
	internal class NearestTrainStatus
	{
		// Physics
		internal double Speed;
		internal double Acceleration;

		// Brake system
		internal double MainReservoirPressure;
		internal double EqualizingReservoirPressure;
		internal double BrakePipePressure;
		internal double BrakeCylinderPressure;
		internal double StraightAirPipePressure;

		// Door
		internal double LeftDoorState;
		internal double RightDoorState;
		internal bool LeftDoorAnticipatedOpen;
		internal bool RightDoorAnticipatedOpen;

		// Handle
		internal int Reverser;
		internal int PowerNotch;
		internal int BrakeNotch;
		internal bool HoldBrake;
		internal bool EmergencyBrake;
		internal bool ConstSpeed;

		internal PluginState[] PluginStates;

		internal NearestTrainStatus()
		{
			PluginStates = new PluginState[0];
		}

		/// <summary>
		/// Applies the status to the specified train
		/// </summary>
		/// <param name="train">The train loaded from extensions.cfg or dummy train</param>
		internal void Apply(TrainBase train)
		{
			foreach (CarBase car in train.Cars)
			{
				car.CurrentSpeed = Speed / 3.6;
				car.Specs.PerceivedSpeed = Speed / 3.6;
				car.Specs.Acceleration = Acceleration / 3.6;

				if (!NearestTrain.IsExtensionsCfg)
				{
					car.CarBrake.mainReservoir.CurrentPressure = MainReservoirPressure * 1000.0;
					car.CarBrake.equalizingReservoir.CurrentPressure = EqualizingReservoirPressure * 1000.0;
					car.CarBrake.brakePipe.CurrentPressure = BrakePipePressure * 1000.0;
					car.CarBrake.brakeCylinder.CurrentPressure = BrakeCylinderPressure * 1000.0;
					car.CarBrake.straightAirPipe.CurrentPressure = StraightAirPipePressure * 1000.0;
				}

				car.Doors[0].State = LeftDoorState;
				car.Doors[0].AnticipatedOpen = LeftDoorAnticipatedOpen;
				car.Doors[1].State = RightDoorState;
				car.Doors[1].AnticipatedOpen = RightDoorAnticipatedOpen;
			}

			train.Handles.Reverser.Driver = (ReverserPosition)Reverser;
			train.Handles.Reverser.Actual = (ReverserPosition)Reverser;
			train.Handles.Power.Driver = PowerNotch;
			train.Handles.Power.Actual = PowerNotch;
			train.Handles.Brake.Driver = BrakeNotch;
			train.Handles.Brake.Actual = BrakeNotch;
			if (train.Handles.HasHoldBrake)
			{
				train.Handles.HoldBrake.Driver = HoldBrake;
				train.Handles.HoldBrake.Actual = HoldBrake;
			}
			train.Handles.EmergencyBrake.Driver = EmergencyBrake;
			train.Handles.EmergencyBrake.Actual = EmergencyBrake;
			if (train.Specs.HasConstSpeed)
			{
				train.Specs.CurrentConstSpeed = ConstSpeed;
			}

			if (NearestTrain.EnablePluginSimulation && PluginStates.Length != 0)
			{
				PluginManager.CurrentPlugin.Panel = new int[PluginStates.Max(x => x.Index) + 1];
				foreach (PluginState pluginState in PluginStates)
				{
					PluginManager.CurrentPlugin.Panel[pluginState.Index] = pluginState.Value;
				}
			}
			else
			{
				PluginManager.CurrentPlugin.Panel = new int[0];
			}
		}

		/// <summary>
		/// Initializes the status of the specified train
		/// </summary>
		/// <param name="train">The train loaded from extensions.cfg</param>
		internal static void Initialize(TrainBase train)
		{
			foreach (CarBase car in train.Cars)
			{
				car.CurrentSpeed = 0.0;
				car.Specs.PerceivedSpeed = 0.0;
				car.Specs.Acceleration = 0.0;

				car.Doors[0].State = 0.0;
				car.Doors[0].AnticipatedOpen = false;
				car.Doors[1].State = 0.0;
				car.Doors[1].AnticipatedOpen = false;
			}

			train.Handles.Reverser.Driver = ReverserPosition.Neutral;
			train.Handles.Reverser.Actual = ReverserPosition.Neutral;
			train.Handles.Power.Driver = 0;
			train.Handles.Power.Actual = 0;
			train.Handles.Brake.Driver = 0;
			train.Handles.Brake.Actual = 0;
			if (train.Handles.HasHoldBrake)
			{
				train.Handles.HoldBrake.Driver = false;
				train.Handles.HoldBrake.Actual = false;
			}
			train.Handles.EmergencyBrake.Driver = false;
			train.Handles.EmergencyBrake.Actual = false;
			if (train.Specs.HasConstSpeed)
			{
				train.Specs.CurrentConstSpeed = false;
			}
		}
	}
}
