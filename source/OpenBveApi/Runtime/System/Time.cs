using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents a time.</summary>
	[DataContract]
	public class Time
	{
		/// <summary>The time in seconds.</summary>
		[DataMember]
		private readonly double MyValue;

		/// <summary>Gets the time in seconds.</summary>
		public double Seconds => MyValue;

		/// <summary>Gets the time in milliseconds.</summary>
		public double Milliseconds => 1000.0 * MyValue;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The time in seconds.</param>
		public Time(double value)
		{
			MyValue = value;
		}
	}
}
