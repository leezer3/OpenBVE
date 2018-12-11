using OpenBveApi.Math;

namespace OpenBveShared
{
	/// <summary>Contains alignment data for the camera</summary>
	public struct CameraAlignment
	{
		/// <summary>The absolute camera position</summary>
		public Vector3 Position;
		/// <summary>The camera yaw</summary>
		public double Yaw;
		/// <summary>The camera pitch</summary>
		public double Pitch;
		/// <summary>The camera roll</summary>
		public double Roll;
		/// <summary>The camera track position</summary>
		public double TrackPosition;
		/// <summary>The camera zoom</summary>
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
