namespace OpenBveApi.Routes
{
	/// <summary>A track (route) is made up of an array of track elements (cells)</summary>
	public class Track
	{
		/// <summary>The elements array</summary>
		public TrackElement[] Elements;
		/// <summary>The rail gauge for this track</summary>
		public double RailGauge = 1.435;

		/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
		/// <param name="position">The track position</param>
		/// <param name="inaccuracy">The openBVE innacuaracy value</param>
		/// <param name="x">The X (horizontal) co-ordinate to update</param>
		/// <param name="y">The Y (vertical) co-ordinate to update</param>
		/// <param name="c">???</param>
		public void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c)
		{
			if (inaccuracy <= 0.0)
			{
				x = 0.0;
				y = 0.0;
				c = 0.0;
			}
			else
			{
				double z = System.Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * System.Math.Sin(0.5843 * z) + 0.82 * System.Math.Sin(0.2246 * z) + 0.55 * System.Math.Sin(0.1974 * z);
				x *= 0.0035 * RailGauge * inaccuracy;
				y = 0.18 * System.Math.Sin(0.5172 * z) + 0.37 * System.Math.Sin(0.3251 * z) + 0.91 * System.Math.Sin(0.3773 * z);
				y *= 0.0020 * RailGauge * inaccuracy;
				c = 0.23 * System.Math.Sin(0.3131 * z) + 0.54 * System.Math.Sin(0.5807 * z) + 0.81 * System.Math.Sin(0.3621 * z);
				c *= 0.0025 * RailGauge * inaccuracy;
			}
		}
	}
}
