namespace OpenBveApi.Runtime
{
	/// <summary>Represents a time.</summary>
	public class Time
	{
		// --- members ---
		/// <summary>The time in seconds.</summary>
		private readonly double MyValue;

		// --- properties ---
		/// <summary>Gets the time in seconds.</summary>
		public double Seconds
		{
			get
			{
				return this.MyValue;
			}
		}

		/// <summary>Gets the time in milliseconds.</summary>
		public double Milliseconds
		{
			get
			{
				return 1000.0 * this.MyValue;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The time in seconds.</param>
		public Time(double value)
		{
			this.MyValue = value;
		}
	}
}
