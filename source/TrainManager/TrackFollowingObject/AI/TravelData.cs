namespace TrainManager.Trains
{
	/// <summary>Travel plan of the train moving to a certain point</summary>
	public abstract class TravelData
	{
		// Parameters from XML
		/// <summary>The deceleration to apply at this point</summary>
		public double Decelerate;
		/// <summary>The track position</summary>
		public double Position;
		/// <summary>The speed at this point</summary>
		public virtual double PassingSpeed
		{
			get;
			set;
		}
		/// <summary>The acceleration to apply from this point</summary>
		public double Accelerate;
		/// <summary>The target speed</summary>
		public double TargetSpeed;
		/// <summary>The rail index the train is travelling on</summary>
		public int RailIndex;

		// Parameters calculated by the SetupTravelData function
		internal double DecelerationStartPosition;
		internal double DecelerationStartTime;
		internal virtual double ArrivalTime
		{
			get;
			set;
		}
		internal double Mileage;
		internal virtual double DepartureTime
		{
			get;
			set;
		}
		internal double AccelerationEndPosition;
		internal double AccelerationEndTime;
	}
}
