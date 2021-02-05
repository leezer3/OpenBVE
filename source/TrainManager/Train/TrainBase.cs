using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public abstract partial class TrainBase : AbstractTrain
	{
		/// <summary>Contains information on the specifications of the train</summary>
		public TrainSpecs Specs;
		/// <summary>The cab handles</summary>
		public CabHandles Handles;
		/// <summary>Holds the passengers</summary>
		public TrainPassengers Passengers;
		/// <summary>Holds the safety systems for the train</summary>
		public TrainSafetySystems SafetySystems;
		/// <summary>Holds the cars</summary>
		public CarBase[] Cars;
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		public int CameraCar;
		/// <summary>Coefficient of friction used for braking</summary>
		public const double CoefficientOfGroundFriction = 0.5;
		/// <summary>The speed difference in m/s above which derailments etc. will occur</summary>
		public double CriticalCollisionSpeedDifference = 8.0;
		/// <summary>The time of the last station arrival in seconds since midnight</summary>
		public double StationArrivalTime;
		/// <summary>The time of the last station departure in seconds since midnight</summary>
		public double StationDepartureTime;
		/// <summary>Whether the station departure sound has been triggered</summary>
		public bool StationDepartureSoundPlayed;
		/// <summary>The adjust distance to the station stop point</summary>
		public double StationDistanceToStopPoint;

		/// <summary>Called once when the simulation loads to initalize the train</summary>
		public void Initialize()
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Initialize();
			}
			Update(0.0);
		}

		/// <summary>Synchronizes the entire train after a period of infrequent updates</summary>
		public void Synchronize()
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Syncronize();
			}
		}

		/// <summary>Updates the objects for all cars in this train</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		public void UpdateObjects(double TimeElapsed, bool ForceUpdate)
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.Running)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateObjects(TimeElapsed, ForceUpdate, true);
					Cars[i].FrontBogie.UpdateObjects(TimeElapsed, ForceUpdate);
					Cars[i].RearBogie.UpdateObjects(TimeElapsed, ForceUpdate);
					if (i == DriverCar && Cars[i].Windscreen != null)
					{
						Cars[i].Windscreen.Update(TimeElapsed);
					}
					Cars[i].Coupler.UpdateObjects(TimeElapsed, ForceUpdate);
				}
			}
		}

		/// <summary>Performs a forced update on all objects attached to the driver car</summary>
		/// <remarks>This function ignores damping of needles etc.</remarks>
		public void UpdateCabObjects()
		{
			Cars[this.DriverCar].UpdateObjects(0.0, true, false);
		}
	}
}
