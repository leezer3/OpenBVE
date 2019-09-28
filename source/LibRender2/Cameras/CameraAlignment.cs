using OpenBveApi.Math;

namespace LibRender2.Cameras
{
	/// <summary>Defines a relative camera alignment</summary>
	public struct CameraAlignment
	{
		/// <summary>The absolute world position</summary>
		public Vector3 Position;
		/// <summary>The yaw value</summary>
		public double Yaw;
		/// <summary>The pitch value</summary>
		public double Pitch;
		/// <summary>The roll value</summary>
		public double Roll;
		/// <summary>The zero-based track position</summary>
		public double TrackPosition;
		/// <summary>The zoom value</summary>
		public double Zoom;

		public CameraAlignment(Vector3 Position, double Yaw, double Pitch, double Roll, double TrackPosition, double Zoom)
		{
			this.Position = Position;
			this.Yaw = Yaw;
			this.Pitch = Pitch;
			this.Roll = Roll;
			this.TrackPosition = TrackPosition;
			this.Zoom = Zoom;
		}
	}
}
