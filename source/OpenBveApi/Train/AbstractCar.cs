using OpenBveApi.Math;

namespace OpenBveApi.Trains
{
	/// <summary>An abstract train car</summary>
	public class AbstractCar
	{
		/// <summary>Front axle about which the car pivots</summary>
		public Axle FrontAxle;

		/// <summary>Rear axle about which the car pivots</summary>
		public Axle RearAxle;

		/// <summary>The width of the car in meters</summary>
		public double Width;

		/// <summary>The height of the car in meters</summary>
		public double Height;

		/// <summary>The length of the car in meters</summary>
		public double Length;

		/// <summary>The Up vector</summary>
		public Vector3 Up;

		/// <summary>The current speed of this car</summary>
		/// <remarks>Default units are km/h</remarks>
		public double CurrentSpeed;

		/// <summary>The empty mass of the car</summary>
		public double EmptyMass;

		/// <summary>Returns the current mass of the car including cargo</summary>
		public double CurrentMass
		{
			get
			{
				return EmptyMass + CargoMass;
			}
		}

		/// <summary>The current mass of any cargo in the car</summary>
		public double CargoMass;

		/// <summary>Contains the current brightness values</summary>
		public Brightness Brightness;

		/// <summary>Creates the in-world co-ordinates for a sound attached to this car</summary>
		public virtual void CreateWorldCoordinates(Vector3 Car, out Vector3 Position, out Vector3 Direction)
		{
			Position = Vector3.Zero;
			Direction = Vector3.Zero;
		}

		/// <summary>Gets the track position of this car</summary>
		public virtual double TrackPosition
		{
			get
			{
				return 0.0;
			}
		}

		/// <summary>The index of the car within the train</summary>
		public virtual int Index
		{
			get
			{
				// A single car is by itself a train, hence index zero
				return 0;
			}
		}

		/// <summary>Call this method to reverse (flip) the car</summary>
		public virtual void Reverse()
		{

		}
	}
}
