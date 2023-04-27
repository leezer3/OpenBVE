using OpenBveApi.Math;

namespace LibRender2.Camera
{
	public struct CameraRestriction
	{
		/// <summary>The absolute bottom left vector</summary>
		public Vector3 AbsoluteBottomLeft;
		/// <summary>The absolute top right vector</summary>
		public Vector3 AbsoluteTopRight;
		/// <summary>The relative bottom left vector</summary>
		public Vector3 BottomLeft;
		/// <summary>The relative top right vector</summary>
		public Vector3 TopRight;

		/// <summary>Reverses the camera restriction</summary>
		public void Reverse()
		{
			AbsoluteBottomLeft.Rotate(Vector3.Forward, 3.14159);
			AbsoluteTopRight.Rotate(Vector3.Forward, 3.14159);
		}
	}
}
