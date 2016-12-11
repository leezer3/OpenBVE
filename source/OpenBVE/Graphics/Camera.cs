using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBve {
	internal class Camera {
		
		// --- members ---
		
		/// <summary>The position of the camera.</summary>
		internal OpenBveApi.Math.Vector3 Position;
		
		/// <summary>The orientation of the camera.</summary>
		internal OpenBveApi.Math.Orientation3 Orientation;
		
		/// <summary>The horizontal viewing angle in radians.</summary>
		internal double HorizontalViewingAngle;
		
		/// <summary>The vertical viewing angle in radians.</summary>
		internal double VerticalViewingAngle;
		
		/// <summary>The viewing distance.</summary>
		internal double ViewingDistance;
		
		
		// --- constructors ---

		/// <summary>Creates a new camera. A call to SetViewingAngle must be made to set the perspective in OpenGL.</summary>
		/// <param name="viewingDistance">The viewing distance in meters.</param>
		internal Camera(double viewingDistance) {
			const double degrees = 0.0174532925199433;
			this.Position = OpenBveApi.Math.Vector3.Null;
			this.Orientation = OpenBveApi.Math.Orientation3.Default;
			this.HorizontalViewingAngle = 45.0 * degrees;
			this.VerticalViewingAngle = 45.0 * degrees;
			this.ViewingDistance = viewingDistance;
		}

		
		// --- functions ---
		
		/// <summary>Sets the viewing angle and updates the perspective accordingly. This function changes the current matrix to GL_MODELVIEW once finished.</summary>
		/// <param name="verticalViewingAngle">The vertical viewing angle in radians.</param>
		internal void SetViewingAngle(double verticalViewingAngle) {
			double aspectRatio = (double)Screen.Width / (double)Screen.Height;
			this.HorizontalViewingAngle = 2.0 * Math.Atan(aspectRatio * Math.Tan(0.5 * this.VerticalViewingAngle));
			this.VerticalViewingAngle = verticalViewingAngle;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			const double inverseDegrees = 57.295779513082320877;
			// TODO //
			// The old renderer assumes a negative aspect ratio. Once the new renderer
			// is implemented, change the aspect ratio in the following line to positive.
			// ref (float) (this.VerticalViewingAngle * inverseDegrees), (float) (-aspectRatio), 1.0f, (float) (this.ViewingDistance))
			float radViewingAngle = (float) (this.VerticalViewingAngle * inverseDegrees);
			float negAspect = (float) -aspectRatio;
			float viewing = (float) this.ViewingDistance;
			Matrix4 val = Matrix4.CreatePerspectiveFieldOfView(radViewingAngle, negAspect, 1.0f, viewing);
			GL.LoadMatrix(ref val);
			GL.MatrixMode(MatrixMode.Modelview);
		}
		
	}
}