using OpenBveApi.Math;
using System.Collections.Generic;

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
		/// <summary>The rain intensity applying to this element</summary>
		public int RainIntensity;
		/// <summary>The snow intensity applying to this element</summary>
		public int SnowIntensity;
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
		public List<GeneralEvent> Events;
		/// <summary>Whether the rail is driveable</summary>
		public bool IsDriveable;
		/// <summary>Whether the element contains a switch</summary>
		public bool ContainsSwitch;
		/// <summary>Holds the properties for all available power supplies</summary>
		public Dictionary<PowerSupplyTypes, PowerSupply> PowerSupplies;

		/// <summary>Creates a new track element</summary>
		/// <param name="startingTrackPosition">The starting position (relative to zero)</param>
		public TrackElement(double startingTrackPosition)
		{
			InvalidElement = false;
			this.StartingTrackPosition = startingTrackPosition;
			Pitch = 0.0;
			CurveRadius = 0.0;
			CurveCant = 0.0;
			CurveCantTangent = 0.0;
			AdhesionMultiplier = 1.0;
			RainIntensity = 0;
			SnowIntensity = 0;
			CsvRwAccuracyLevel = 2.0;
			WorldPosition = Vector3.Zero;
			WorldDirection = Vector3.Forward;
			WorldUp = Vector3.Down;
			WorldSide = Vector3.Right;
			Events = new List<GeneralEvent>();
			IsDriveable = false;
			ContainsSwitch = false;
			PowerSupplies = new Dictionary<PowerSupplyTypes, PowerSupply>();
		}
	}
}
