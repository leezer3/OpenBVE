using System;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBveShared
{
	public static class Camera
	{
		/// <summary>The currently applied camera restriction</summary>
		public static CameraRestrictionMode CameraRestriction = CameraRestrictionMode.NotAvailable;

		/// <summary>The current camera mode</summary>
		public static CameraViewMode CameraView;

		/// <summary>The current base camera alignment</summary>
		public static CameraAlignment CameraCurrentAlignment;
		
		/// <summary>The current camera alignment direction vectors</summary>
		public static CameraAlignment CameraAlignmentDirection;

		/// <summary>The current camera alignment speed vectors</summary>
		public static CameraAlignment CameraAlignmentSpeed;

		/// <summary>The absolute camera position</summary>
		public static Vector3 AbsoluteCameraPosition;

		/// <summary>The absolute camera direction vector</summary>
		public static Vector3 AbsoluteCameraDirection;

		/// <summary>The absolute camera up vector</summary>
		public static Vector3 AbsoluteCameraUp;

		/// <summary>The absolute camera side vector</summary>
		public static Vector3 AbsoluteCameraSide;
		
		/// <summary>The current camera movement speed</summary>
		public static double CameraSpeed;

		/// <summary>The top camera movement speed when in an interior view</summary>
		public const double CameraInteriorTopSpeed = 5.0;

		/// <summary>The top camera angular movement speed when in an interior view</summary>
		public const double CameraInteriorTopAngularSpeed = 5.0;

		/// <summary>The top camera movement speed when in an exterior view</summary>
		public const double CameraExteriorTopSpeed = 50.0;

		/// <summary>The top camera angular movement speed when in an exterior view</summary>
		public const double CameraExteriorTopAngularSpeed = 10.0;

		/// <summary>The top camera zoom speed</summary>
		public const double CameraZoomTopSpeed = 2.0;

		/// <summary>Applies the current camera zoom, taking account of the vertical viewing angle</summary>
		public static void ApplyZoom()
		{
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle * Math.Exp(CameraCurrentAlignment.Zoom);
			if (World.VerticalViewingAngle < 0.001) World.VerticalViewingAngle = 0.001;
			if (World.VerticalViewingAngle > 1.5) World.VerticalViewingAngle = 1.5;
			Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
		}
	}
}
