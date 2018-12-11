using System;
using OpenBveApi.Math;

namespace TrackManager
{
	/// <summary>A track (route) is made up of an array of track elements (cells)</summary>
	public class Track
	{
		public TrackElement[] Elements;

		public double RailGauge;

		public Track(double Gauge)
		{
			RailGauge = Gauge;
		}

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
				double z = Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * Math.Sin(0.5843 * z) + 0.82 * Math.Sin(0.2246 * z) + 0.55 * Math.Sin(0.1974 * z);
				x *= 0.0035 * RailGauge * inaccuracy;
				y = 0.18 * Math.Sin(0.5172 * z) + 0.37 * Math.Sin(0.3251 * z) + 0.91 * Math.Sin(0.3773 * z);
				y *= 0.0020 * RailGauge * inaccuracy;
				c = 0.23 * Math.Sin(0.3131 * z) + 0.54 * Math.Sin(0.5807 * z) + 0.81 * Math.Sin(0.3621 * z);
				c *= 0.0025 * RailGauge * inaccuracy;
			}
		}
	}

	/// <summary>Defines a single track element (cell)</summary>
	public struct TrackElement
	{
		/// <summary>The starting linear track position of this element</summary>
		public double StartingTrackPosition;
		/// <summary>The curve radius applying to this element</summary>
		public double CurveRadius;
		/// <summary>The curve cant applying to this element</summary>
		public double CurveCant;
		/// <summary>The tangent value applied to the curve due to cant</summary>
		public double CurveCantTangent;
		/// <summary>The adhesion multiplier applying to this element</summary>
		public double AdhesionMultiplier;
		/// <summary>The accuracy level of this element (Affects cab sway etc) </summary>
		public double CsvRwAccuracyLevel;
		/// <summary>The pitch of this element</summary>
		public double Pitch;
		/// <summary>The absolute world position</summary>
		public Vector3 WorldPosition;
		public Vector3 WorldDirection;
		public Vector3 WorldUp;
		public Vector3 WorldSide;
		/// <summary>An array containing all events attached to this element</summary>
		public GeneralEvent[] Events;

		public TrackElement(double StartingTrackPosition)
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
			this.Events = new GeneralEvent[] { };
		}
	}
}
