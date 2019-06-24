using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>A track (route) is made up of an array of track elements (cells)</summary>
		internal class Track
		{
			internal TrackElement[] Elements;

			internal double RailGauge = 1.435;

			/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
			/// <param name="position">The track position</param>
			/// <param name="inaccuracy">The openBVE innacuaracy value</param>
			/// <param name="x">The X (horizontal) co-ordinate to update</param>
			/// <param name="y">The Y (vertical) co-ordinate to update</param>
			/// <param name="c">???</param>
			internal void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c)
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

		/// <summary>Defines a single track element (cell)</summary>
		internal struct TrackElement
		{
			/// <summary>The starting linear track position of this element</summary>
			internal double StartingTrackPosition;
			/// <summary>The curve radius applying to this element</summary>
			internal double CurveRadius;
			/// <summary>The curve cant applying to this element</summary>
			internal double CurveCant;
			/// <summary>The tangent value applied to the curve due to cant</summary>
			internal double CurveCantTangent;
			/// <summary>The adhesion multiplier applying to this element</summary>
			internal double AdhesionMultiplier;
			/// <summary>The accuracy level of this element (Affects cab sway etc) </summary>
			internal double CsvRwAccuracyLevel;
			/// <summary>The pitch of this element</summary>
			internal double Pitch;
			/// <summary>The absolute world position</summary>
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			/// <summary>An array containing all events attached to this element</summary>
			internal object[] Events;

			internal TrackElement(double StartingTrackPosition)
			{
				this.StartingTrackPosition = StartingTrackPosition;
				this.Pitch = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.CurveCantTangent = 0.0;
				this.AdhesionMultiplier = 1.0;
				this.CsvRwAccuracyLevel = 2.0;
				this.WorldPosition = Vector3.Zero;
				this.WorldDirection = Vector3.Forward;
				this.WorldUp = Vector3.Down;
				this.WorldSide = Vector3.Right;
				this.Events = new object[] { };
			}
		}
	}
}
