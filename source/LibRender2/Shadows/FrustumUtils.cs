using System;

namespace LibRender2.ShadowMapping
{
	/// <summary>
	/// Utility class for camera frustum calculations, specifically for Cascaded Shadow Mapping.
	/// </summary>
	public static class FrustumUtils
	{
		/// <summary>Minimum vertical FOV (45 degrees in radians) used for the stable bounding sphere.</summary>
		private const double MinFovYRad = 0.785398;
		/// <summary>
		/// Computes split distances using the Parallel Split Shadow Maps (PSSM) algorithm.
		/// </summary>
		/// <param name="cascadeCount">Number of cascades (must be at least 1).</param>
		/// <param name="zNear">Camera near clip distance.</param>
		/// <param name="zFar">Shadow far distance (must be greater than zNear).</param>
		/// <param name="lambda">Blend between linear (0) and logarithmic (1) splits.</param>
		public static double[] ComputeSplitDistances(int cascadeCount, double zNear, double zFar, double lambda)
		{
			if (cascadeCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(cascadeCount));
			}
			if (zFar <= zNear)
			{
				throw new ArgumentException("Shadow far distance must be greater than the near clip.");
			}
			double[] splits = new double[cascadeCount + 1];
			splits[0] = zNear;
			splits[cascadeCount] = zFar;

			for (int i = 1; i < cascadeCount; i++)
			{
				double fraction = (double)i / cascadeCount;
				double logSplit = zNear * Math.Pow(zFar / zNear, fraction);
				double linearSplit = zNear + (zFar - zNear) * fraction;
				splits[i] = lambda * logSplit + (1.0 - lambda) * linearSplit;
			}

			return splits;
		}

		/// <summary>
		/// Computes a stable radius for a bounding sphere circumscribing the sub-frustum.
		/// </summary>
		public static double GetStableRadius(double zNear, double zFar, double fovYRad, double aspect)
		{
			// Clamp min FOV to 45 degrees to prevent the sphere from shrinking too much when zooming.
			// This ensures large objects (like trains) don't get clipped from the shadow map at high zoom.
			fovYRad = Math.Max(fovYRad, MinFovYRad); 
			// Half-height/width of the far plane of this sub-frustum in camera space
			double h = zFar * Math.Tan(fovYRad / 2.0);
			double w = h * aspect;
			
			// The farthest corner of the subfrustum is on the far plane
			// Vector3 farCorner = (w, h, zFar)
			// Vector3 center = (0, 0, (zNear + zFar) / 2)
			// radius = distance from center to farCorner
			double centerZ = (zNear + zFar) / 2.0;
			double dz = zFar - centerZ;
			
			return Math.Sqrt(w * w + h * h + dz * dz);
		}
	}
}
