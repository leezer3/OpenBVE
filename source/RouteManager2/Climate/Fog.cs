using OpenBveApi.Colors;

namespace RouteManager2.Climate
{
	/// <summary>Defines a region of fog</summary>
	public struct Fog
	{
		/// <summary>The offset at which the fog starts</summary>
		/// <remarks>Distance from camera</remarks>
		public float Start;

		/// <summary>The offset at which the fog ends</summary>
		/// /// <remarks>Distance from camera</remarks>
		public float End;
		
		/// <summary>The color of the fog</summary>
		public Color24 Color;
		
		/// <summary>The track position at which the fog is placed</summary>
		public double TrackPosition;

		/// <summary>The fog density value</summary>
		public float Density;

		/// <summary>Stores whether the fog is linear</summary>
		public bool IsLinear;

		/// <summary>Creates a new region of fog</summary>
		public Fog(float startDistance, float endDistance, Color24 fogColor, double trackPosition, bool isLinear = true, float density = 0.0f)
		{
			Start = startDistance;
			End = endDistance;
			Color = fogColor;
			TrackPosition = trackPosition;
			Density = density;
			IsLinear = isLinear;
		}
	}
}
