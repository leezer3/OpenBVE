using OpenBveApi.Hosts;

namespace OpenBveApi.Objects
{
	/// <summary>Optimization policy (hacks / thresholds) for <see cref="StaticObject"/></summary>
	public partial class StaticObject
	{
		/// <inheritdoc />
		public override void OptimizeObject(bool preserveVerticies, int faceThreshold, bool vertexCulling)
		{
			if (isOptimized)
			{
				return;
			}
			isOptimized = true;
			if (ShouldSkipOptimization(faceThreshold))
			{
				return;
			}
			if (Mesh.Vertices.Length > 10000)
			{
				// Don't attempt to de-duplicate where over 10k vertices
				preserveVerticies = true;
			}
			MeshOptimizer.Optimize(Mesh, preserveVerticies, vertexCulling);
		}

		private bool ShouldSkipOptimization(int faceThreshold)
		{
			if (currentHost.Platform != HostPlatform.AppleOSX)
			{
				/*
				 * HACK:
				 * A forwards compatible GL3 context (required on OS-X) only supports tris
				 * and thus an optimized object (decomposed into tris) in all circumstances
				 *
				 * When in viewers, skip optimisation if above the threshold to allow
				 * faster reload speeds.
				 *
				 * When in-game, force optimisation at all times for best possible performance
				 * even though this may have an effect on load-times
				 */
				int m = Mesh.Materials.Length;
				int f = Mesh.Faces.Length;
				if (m >= f / 500 && f >= faceThreshold && f < 20000 && currentHost.Application != HostApplication.OpenBve)
				{
					return true;
				}
			}
			return false;
		}
	}
}
