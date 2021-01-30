using OpenBveApi.Trains;
using TrainManager.Handles;

namespace TrainManager.Trains
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public abstract class TrainBase : AbstractTrain
	{
		/// <summary>Contains information on the specifications of the train</summary>
		public TrainSpecs Specs;
		/// <summary>The cab handles</summary>
		public CabHandles Handles;
		/// <summary>Holds the passengers</summary>
		public TrainPassengers Passengers;
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		public int CameraCar;
		/// <summary>Coefficient of friction used for braking</summary>
		public const double CoefficientOfGroundFriction = 0.5;
		/// <summary>The speed difference in m/s above which derailments etc. will occur</summary>
		public double CriticalCollisionSpeedDifference = 8.0;
	}
}
