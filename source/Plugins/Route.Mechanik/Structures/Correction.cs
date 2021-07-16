using OpenBveApi.Math;

namespace Route.Mechanik
{
	/// <summary>Describes a correction to be performed to the 3D game world</summary>
	internal class Correction
	{
		/// <summary>The first point to use</summary>
		internal readonly Vector2 FirstPoint;
		/// <summary>The second point to use</summary>
		internal readonly Vector2 SecondPoint;

		internal Correction(Vector2 firstPoint, Vector2 secondPoint)
		{
			FirstPoint = firstPoint;
			SecondPoint = secondPoint;
		}
	}
}
