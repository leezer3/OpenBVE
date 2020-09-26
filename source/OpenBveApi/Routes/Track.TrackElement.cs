using OpenBveApi.Math;

namespace OpenBveApi.Routes
{
	/// <summary>Defines a single track element (cell)</summary>
	public struct TrackElement
	{
		/// <summary>Whether the element is invalid</summary>
		public bool InvalidElement;
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
		/// <summary>The direction vector</summary>
		public Vector3 WorldDirection;
		/// <summary>The up vector</summary>
		public Vector3 WorldUp;
		/// <summary>The side vector</summary>
		public Vector3 WorldSide;
		/// <summary>An array containing all events attached to this element</summary>
		public GeneralEvent[] Events;
		/// <summary>Whether interpolation begins at this element</summary>
		public bool BeginInterpolation;

		/// <summary>Creates a new track element</summary>
		/// <param name="StartingTrackPosition">The starting position (relative to zero)</param>
		public TrackElement(double StartingTrackPosition)
		{
			this.InvalidElement = false;
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
			this.BeginInterpolation = false;
		}
	}
}
