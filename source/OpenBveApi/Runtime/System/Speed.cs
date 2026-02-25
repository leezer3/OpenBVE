using System.Runtime.Serialization;
// ReSharper disable UnusedMember.Global

namespace OpenBveApi.Runtime
{
	/// <summary>Represents a speed.</summary>
	[DataContract]
	public class Speed
	{
		/// <summary>The speed in meters per second.</summary>
		[DataMember]
		private readonly double MyValue;

		/// <summary>Gets the speed in meters per second.</summary>
		public double MetersPerSecond => MyValue;

		/// <summary>Gets the speed in kilometers per hour.</summary>
		public double KilometersPerHour => 3.6 * MyValue;

		/// <summary>Gets the speed in miles per hour.</summary>
		public double MilesPerHour => 2.236936 * MyValue;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The speed in meters per second.</param>
		public Speed(double value)
		{
			MyValue = value;
		}
	}
}
