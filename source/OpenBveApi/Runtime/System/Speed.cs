using System.Runtime.Serialization;

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
		public double MetersPerSecond
		{
			get
			{
				return this.MyValue;
			}
		}

		/// <summary>Gets the speed in kilometes per hour.</summary>
		public double KilometersPerHour
		{
			get
			{
				return 3.6 * this.MyValue;
			}
		}

		/// <summary>Gets the speed in miles per hour.</summary>
		public double MilesPerHour
		{
			get
			{
				return 2.236936 * this.MyValue;
			}
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The speed in meters per second.</param>
		public Speed(double value)
		{
			this.MyValue = value;
		}
	}
}
