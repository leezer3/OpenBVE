using System;

namespace LibRender2.ShadowMapping
{
	/// <summary>
	/// Utility class for camera frustum calculations, specifically for Cascaded Shadow Mapping.
	/// </summary>
	public static class FrustumUtils
	{
		/// <summary>
		/// Computes split distances using the Parallel Split Shadow Maps (PSSM) algorithm.
		/// </summary>
		public static double[] ComputeSplitDistances(int cascadeCount, double zNear, double zFar, double lambda)
		{
			double[] splits = new double[cascadeCount + 1];
			splits[0] = zNear;
			splits[cascadeCount] = zFar;

			for (int i = 1; i < cascadeCount; i++)
			{
				double p = (double)i / cascadeCount;
				double log = zNear * Math.Pow(zFar / zNear, p);
				double lin = zNear + (zFar - zNear) * p;
				splits[i] = lambda * log + (1.0 - lambda) * lin;
			}

			return splits;
		}

		/// <summary>
		/// Computes a stable radius for a bounding sphere circumscribing the sub-frustum.
		/// </summary>
		public static double GetStableRadius(double zNear, double zFar, double fovYRad, double aspect)
		{
			// Clamp min FOV to 45 degrees (0.785 rad) to prevent the sphere from shrinking too much when zooming.
			// This ensures large objects (like trains) don't get clipped from the shadow map at high zoom.
			fovYRad = Math.Max(fovYRad, 0.785398); 
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
