using System;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Whether sound events are currently suppressed</summary>
		internal static bool SuppressSoundEvents = false;

		/// <summary>The list of tracks available in the simulation.</summary>
		internal static Track[] Tracks = new Track[] { new Track() };

		/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
		/// <param name="position">The track position</param>
		/// <param name="inaccuracy">The openBVE innacuaracy value</param>
		/// <param name="x">The X (horizontal) co-ordinate to update</param>
		/// <param name="y">The Y (vertical) co-ordinate to update</param>
		/// <param name="c">???</param>
		private static void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c)
		{
			if (inaccuracy <= 0.0)
			{
				x = 0.0;
				y = 0.0;
				c = 0.0;
			}
			else
			{
				double z = Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * Math.Sin(0.5843 * z) + 0.82 * Math.Sin(0.2246 * z) + 0.55 * Math.Sin(0.1974 * z);
				x *= 0.0035 * Game.RouteRailGauge * inaccuracy;
				y = 0.18 * Math.Sin(0.5172 * z) + 0.37 * Math.Sin(0.3251 * z) + 0.91 * Math.Sin(0.3773 * z);
				y *= 0.0020 * Game.RouteRailGauge * inaccuracy;
				c = 0.23 * Math.Sin(0.3131 * z) + 0.54 * Math.Sin(0.5807 * z) + 0.81 * Math.Sin(0.3621 * z);
				c *= 0.0025 * Game.RouteRailGauge * inaccuracy;
			}
		}
	}
}
