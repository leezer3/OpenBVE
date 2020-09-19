namespace OpenBveApi.Runtime
{
	/// <summary>Represents information about a signal or section.</summary>
	public class SignalData
	{
		// --- members ---
		/// <summary>The aspect of the signal or section.</summary>
		private readonly int MyAspect;

		/// <summary>The underlying section. Possible values are 0 for the current section, 1 for the upcoming section, or higher values for sections further ahead.</summary>
		private readonly double MyDistance;

		// --- properties ---
		/// <summary>Gets the aspect of the signal or section.</summary>
		public int Aspect
		{
			get
			{
				return this.MyAspect;
			}
		}

		/// <summary>Gets the distance to the signal or section.</summary>
		public double Distance
		{
			get
			{
				return this.MyDistance;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="aspect">The aspect of the signal or section.</param>
		/// <param name="distance">The distance to the signal or section.</param>
		public SignalData(int aspect, double distance)
		{
			this.MyAspect = aspect;
			this.MyDistance = distance;
		}
	}
}
