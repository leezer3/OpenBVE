using OpenBveApi.Math;

namespace OpenBveApi.Trains
{
	/// <summary>An abstract train car</summary>
	public class AbstractCar
	{
		/// <summary>The width of the car in meters</summary>
		public double Width;
		/// <summary>The height of the car in meters</summary>
		public double Height;
		/// <summary>The length of the car in meters</summary>
		public double Length;
		/// <summary>The Up vector</summary>
		public Vector3 Up;
	}
}
