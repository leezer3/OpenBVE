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
	}
}
