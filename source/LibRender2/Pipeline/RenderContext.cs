using OpenBveApi.Math;
using OpenBveApi.Objects;
using LibRender2.Cameras;
using LibRender2.Lightings;
using LibRender2.Passes;

namespace LibRender2.Pipeline
{
	/// <summary>
	/// Contains all necessary information for a single frame rendering pass.
	/// </summary>
	public class RenderContext
	{
		/// <summary>The current projection matrix.</summary>
		public Matrix4D ProjectionMatrix;

		/// <summary>The current view matrix.</summary>
		public Matrix4D ViewMatrix;

		/// <summary>The current camera properties.</summary>
		public CameraProperties Camera;

		/// <summary>The current lighting properties.</summary>
		public Lighting Lighting;

		/// <summary>The time elapsed since the last frame in seconds.</summary>
		public double TimeElapsed;

		/// <summary>The real-time elapsed since the last frame in seconds.</summary>
		public double RealTimeElapsed;

		/// <summary>The renderer instance.</summary>
		public BaseRenderer Renderer;

		public RenderContext(BaseRenderer renderer, double timeElapsed)
		{
			Renderer = renderer;
			TimeElapsed = timeElapsed;
		}
	}
}
