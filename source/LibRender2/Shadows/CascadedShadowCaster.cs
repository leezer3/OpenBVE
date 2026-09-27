using System;
using OpenBveApi.Math;

namespace LibRender2.ShadowMapping
{
    /// <summary>
    /// Computes per-cascade light-space matrices by fitting an orthographic
    /// projection to each sub-frustum of the camera's view.
    /// </summary>
    public class CascadedShadowCaster
    {
        /// <summary>Y threshold for switching the light-view up vector to avoid a degenerate look-at.</summary>
        private const double ParallelUpThreshold = 0.99;

        /// <summary>Epsilon for detecting a degenerate projection matrix when extracting the far plane.</summary>
        private const double FarPlaneEpsilon = 1e-8;

        /// <summary>Epsilon for the perspective divide in point transforms.</summary>
        private const double TransformWDivideEpsilon = 1e-10;

        /// <summary>Fallback far plane when the projection matrix is degenerate.</summary>
        private const double FallbackFarPlane = 1000.0;

        /// <summary>Number of shadow cascades.</summary>
        public int CascadeCount { get; private set; }

        /// <summary>The maximum distance from the camera that receives shadows.</summary>
        public double ShadowDistance { get; set; } = 400.0;

        /// <summary>PSSM lambda (0=linear, 1=log)</summary>
        /// <remarks>Higher values provide more resolution for near shadows</remarks>
        public double SplitLambda { get; set; } = 0.75;

        /// <summary>Extra depth behind the sub-frustum to catch tall occluders.</summary>
        public double DepthMargin { get; set; } = 40.0;

        /// <summary>Active shadow map resolution (used for texel snapping).</summary>
        public int Resolution { get; set; } = 2048;

        /// <summary>Per-cascade light-space VP matrices.</summary>
        public Matrix4D[] LightSpaceMatrices { get; private set; }

		/// <summary>Per-cascade split distances (view-space Z).</summary>
		/// <remarks>
		/// Length = CascadeCount. Each value is the frustum slice boundary
		/// in view-space Z where the shadow transition between cascades occurs.
		/// </remarks> 
		public float[] SplitDistances { get; private set; }

        /// <summary>Per-cascade depth bias for shadow acne prevention.</summary>
        public float[] CascadeBiases { get; private set; }

        /// <summary>Per-cascade world-space texel size (world units per shadow-map texel).</summary>
        /// <remarks>
        /// Used by the vertex shader for true receiver-side normal offset
        /// (Unity-style: offset = normalBiasTexels * texelWorldSize), which keeps
        /// contact (no peter-panning) while curing slope acne.
        /// </remarks>
        public float[] TexelWorldSizes { get; private set; }

        public CascadedShadowCaster(int cascadeCount = 3)
        {
            CascadeCount = cascadeCount;
            LightSpaceMatrices = new Matrix4D[cascadeCount];
            SplitDistances = new float[cascadeCount];
            CascadeBiases = new float[cascadeCount];
            TexelWorldSizes = new float[cascadeCount];

            for (int i = 0; i < cascadeCount; i++)
            {
                LightSpaceMatrices[i] = Matrix4D.Identity;
            }
        }

        /// <summary>
        /// Recomputes all cascade matrices.
        /// Call once per frame before the shadow depth passes.
        /// </summary>
        /// <param name="lightDirection">Normalized world-space direction FROM the light (toward scene).</param>
        /// <param name="cameraView">The camera's current view matrix.</param>
        /// <param name="cameraProjection">The camera's current projection matrix.</param>
        /// <param name="nearClip">Camera near clip distance.</param>
        /// <param name="fovY">Vertical field of view in radians.</param>
        /// <param name="aspect">Aspect ratio (width / height).</param>
        public void Update(Vector3 lightDirection, Matrix4D cameraView, Matrix4D cameraProjection, double nearClip, double fovY, double aspect)
        {
            if (lightDirection.IsNullVector())
            {
                return;
            }
            // Normalize light direction
            Vector3 ld = lightDirection;
            ld.Normalize();

            // Compute split distances using PSSM
            double shadowFar = Math.Min(ShadowDistance,
                GetFarPlaneFromProjection(cameraProjection));
            double[] splits = FrustumUtils.ComputeSplitDistances(
                CascadeCount, nearClip, shadowFar, SplitLambda);

            // Precompute inverse view for stable center calculation
            Matrix4D invView = Matrix4D.Inverse(cameraView);

            for (int i = 0; i < CascadeCount; i++)
            {
                UpdateCascade(i, ld, splits, invView, fovY, aspect);
            }
        }

        /// <summary>
        /// Recomputes the light-space matrix, split distance and biases for a single cascade.
        /// </summary>
        private void UpdateCascade(int index, Vector3 lightDirection, double[] splits, Matrix4D invView, double fovY, double aspect)
        {
            // Compute a mathematically stable bounding sphere for CSM stability.
            // Center is (0, 0, -(zNear + zFar)/2) in View Space.
            double stableNear = (index == 0) ? 0.0 : splits[index];
            double stableFar = splits[index + 1];
            double centerViewZ = -(stableNear + stableFar) / 2.0;

            // Transform the stable view-space center into world space
            Vector3 center = TransformPoint(new Vector3(0, 0, centerViewZ), invView);

            double radius = FrustumUtils.GetStableRadius(stableNear, stableFar, fovY, aspect);

            // Build light view matrix: Look along the light direction directly from the cascade center
            Vector3 lightPos = center;

            Vector3 up = Math.Abs(lightDirection.Y) < ParallelUpThreshold
                ? new Vector3(0, 1, 0)
                : new Vector3(1, 0, 0);

            Matrix4D lightView = Matrix4D.LookAt(lightPos, center + lightDirection, up);

            // Orthographic projection tightly fitting the bounding sphere
            double orthoSize = radius;

            // === Texel snapping to prevent shadow swimming on camera move ===
            // Snap the light-space center to texel boundaries
            // Each texel covers (2*orthoSize / Resolution) world units
            double worldTexelSize = (orthoSize * 2.0) / (double)Resolution;

            // Transform center into light view space to snap it
            Vector3 centerLS = TransformPoint(center, lightView);
            centerLS.X = Math.Round(centerLS.X / worldTexelSize) * worldTexelSize;
            centerLS.Y = Math.Round(centerLS.Y / worldTexelSize) * worldTexelSize;

            // Reconstruct light view by applying the snapped offset
            // We need to adjust the view matrix translation to account for the snap
            Matrix4D invLightView = Matrix4D.Inverse(lightView);
            Vector3 snappedCenter = TransformPoint(centerLS, invLightView);

            lightView = Matrix4D.LookAt(snappedCenter, snappedCenter + lightDirection, up);

            // Tight ortho depth range around the cascade sphere to preserve
            // 24-bit depth precision. Symmetric range still catches tall
            // occluders within DepthMargin.
            double zNear = -(radius + DepthMargin);
            double zFar = radius + DepthMargin;

            Matrix4D.CreateOrthographic(orthoSize * 2.0, orthoSize * 2.0, zNear, zFar, out Matrix4D lightProj);

            LightSpaceMatrices[index] = lightView * lightProj;
            SplitDistances[index] = (float)splits[index + 1];

            // Z-Bias: Convert physical texel size into a Depth Buffer fraction.
            // This ensures we push the depth exactly enough to cure acne, but no more.
            double texelWorldSize = (orthoSize * 2.0) / (double)Resolution;
            double depthRange = zFar - zNear;
            double baseBias = texelWorldSize / depthRange;
            CascadeBiases[index] = (float)baseBias;
            TexelWorldSizes[index] = (float)texelWorldSize;
        }

        /// <summary>Extracts the far clip distance from a projection matrix.</summary>
        private static double GetFarPlaneFromProjection(Matrix4D proj)
        {
            // For a standard perspective projection:
            // proj.Row2.Z = -(far+near)/(far-near)
            // proj.Row3.Z = -2*far*near/(far-near)
            // Solving: far = proj.Row3.Z / (proj.Row2.Z + 1)
            double a = proj.Row2.Z;
            double b = proj.Row3.Z;
            if (Math.Abs(a + 1.0) < FarPlaneEpsilon) return FallbackFarPlane; // fallback
            return b / (a + 1.0);
        }

        /// <summary>Transforms a 3D point through a 4x4 matrix with perspective divide.</summary>
        private static Vector3 TransformPoint(Vector3 p, Matrix4D m)
        {
            double x = p.X * m.Row0.X + p.Y * m.Row1.X + p.Z * m.Row2.X + m.Row3.X;
            double y = p.X * m.Row0.Y + p.Y * m.Row1.Y + p.Z * m.Row2.Y + m.Row3.Y;
            double z = p.X * m.Row0.Z + p.Y * m.Row1.Z + p.Z * m.Row2.Z + m.Row3.Z;
            double w = p.X * m.Row0.W + p.Y * m.Row1.W + p.Z * m.Row2.W + m.Row3.W;
            if (Math.Abs(w) > TransformWDivideEpsilon)
            {
                return new Vector3(x / w, y / w, z / w);
            }
            return new Vector3(x, y, z);
        }
    }
}
