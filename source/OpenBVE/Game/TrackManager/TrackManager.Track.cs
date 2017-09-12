using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>A track (route) is made up of an array of track elements (cells)</summary>
		internal struct Track
		{
			internal TrackElement[] Elements;
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
			internal GeneralEvent[] Events;

			internal TrackElement(double StartingTrackPosition)
			{
				this.StartingTrackPosition = StartingTrackPosition;
				this.Pitch = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.CurveCantTangent = 0.0;
				this.AdhesionMultiplier = 1.0;
				this.CsvRwAccuracyLevel = 2.0;
				this.WorldPosition = new Vector3(0.0, 0.0, 0.0);
				this.WorldDirection = new Vector3(0.0, 0.0, 1.0);
				this.WorldUp = new Vector3(0.0, 1.0, 0.0);
				this.WorldSide = new Vector3(1.0, 0.0, 0.0);
				this.Events = new GeneralEvent[] { };
			}
		}
	}
}
