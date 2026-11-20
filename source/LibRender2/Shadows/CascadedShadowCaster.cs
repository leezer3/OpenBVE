using System;
using OpenBveApi.Math;

namespace LibRender2.ShadowMapping
{
    /// <summary>Fits a tight light-space AABB to each sub-frustum of the camera.</summary>
    public class CascadedShadowCaster
    {
        private struct LightBounds
        {
            public double MinX, MaxX, MinY, MaxY, MinZ, MaxZ;

            public double Width => MaxX - MinX;
            public double Height => MaxY - MinY;
            public double Aspect => Math.Min(Width, Height) / Math.Max(Width, Height);
        }

        /// <summary>Round extents up to this many texels so the projection changes in stable steps.</summary>
        private const int ExtentQuantum = 16;

        /// <summary>Texels of valid depth kept beyond the frustum so PCF taps at slot edges stay in range.</summary>
        private const int EdgePadding = 4;

        /// <summary>Number of shadow cascades.</summary>
        public int CascadeCount { get; private set; }

        /// <summary>The maximum distance from the camera that receives shadows.</summary>
        public double ShadowDistance { get; set; } = 400.0;

        /// <summary>Extra depth behind the sub-frustum to catch tall occluders.</summary>
        public double DepthMargin { get; set; } = 40.0;

        /// <summary>Active shadow map resolution (used for texel snapping).</summary>
        public int Resolution { get; set; } = 2048;

        /// <summary>Optional per-cascade resolutions; overrides <see cref="Resolution"/> when set.</summary>
        public int[] Resolutions { get; set; }

        /// <summary>
        /// PSSM lambda (0 = linear, 1 = log). Higher values give more resolution to near shadows.
        /// </summary>
        public double SplitLambda { get; set; } = 0.75;

        /// <summary>Per-cascade light-space VP matrices.</summary>
        public Matrix4D[] LightSpaceMatrices { get; private set; }

        /// <summary>
        /// Per-cascade split distances (view-space Z). These are the frustum slice boundaries
        /// where the shadow transition between cascades occurs.
        /// </summary>
        public float[] SplitDistances { get; private set; }

        /// <summary>Per-cascade depth bias for shadow acne prevention.</summary>
        public float[] CascadeBiases { get; private set; }

        /// <summary>
        /// Per-cascade world-space texel size (world units per shadow-map texel). Used by the
        /// vertex shader for receiver-side normal offset: normalBiasTexels * texelWorldSize.
        /// </summary>
        public float[] TexelWorldSizes { get; private set; }

        /// <summary>Measured AABB aspect per cascade, used to lay out the atlas.</summary>
        public float[] AspectRatios { get; private set; }

        public CascadedShadowCaster(int cascadeCount = 3)
        {
            CascadeCount = cascadeCount;
            LightSpaceMatrices = new Matrix4D[cascadeCount];
            SplitDistances = new float[cascadeCount];
            CascadeBiases = new float[cascadeCount];
            TexelWorldSizes = new float[cascadeCount];
            AspectRatios = new float[cascadeCount];

            for (int i = 0; i < cascadeCount; i++)
            {
                LightSpaceMatrices[i] = Matrix4D.Identity;
                AspectRatios[i] = 1.0f;
            }
        }

        /// <summary>
        /// Recomputes all cascade matrices. Call once per frame before the shadow depth passes.
        /// </summary>
        /// <param name="lightDirection">Normalized world-space direction FROM the light (toward scene).</param>
        /// <param name="cameraView">The camera's current view matrix.</param>
        /// <param name="cameraProjection">The camera's current projection matrix.</param>
        /// <param name="nearClip">Camera near clip distance.</param>
        /// <param name="targetAspects">Slot height/width ratios; ortho extents expand to cover them.</param>
        public void Update(Vector3 lightDirection, Matrix4D cameraView, Matrix4D cameraProjection,
            double nearClip, float[] targetAspects = null)
        {
            if (lightDirection.IsNullVector())
            {
                return;
            }

            Vector3 ld = lightDirection;
            ld.Normalize();

            Matrix4D invVP = Matrix4D.Inverse(cameraView * cameraProjection);
            Vector3[] fullCorners = FrustumUtils.GetFrustumCornersWorldSpace(invVP);

            double cameraFar = GetFarPlaneFromProjection(cameraProjection);
            double shadowFar = Math.Min(ShadowDistance, cameraFar);
            double[] splits = FrustumUtils.ComputeSplitDistances(CascadeCount, nearClip, shadowFar, SplitLambda);

            // GetSubFrustumCorners works in fractions of the FULL camera frustum.
            double fullDepth = Math.Max(1e-6, cameraFar - nearClip);
            Matrix4D invView = Matrix4D.Inverse(cameraView);

            for (int i = 0; i < CascadeCount; i++)
            {
                double near = (i == 0) ? 0.0 : splits[i];
                double far = splits[i + 1];

                Matrix4D lightView = CreateLightView(ld, invView, -(near + far) / 2.0);
                int resolution = (Resolutions != null && i < Resolutions.Length) ? Resolutions[i] : Resolution;
                resolution = Math.Max(1, resolution);

                Vector3[] subCorners = FrustumUtils.GetSubFrustumCorners(fullCorners,
                    Math.Max(0.0, (near - nearClip) / fullDepth),
                    Math.Min(1.0, (far - nearClip) / fullDepth));

                LightBounds bounds = FitBounds(subCorners, lightView);
                float slotAspect = targetAspects != null && i < targetAspects.Length ? targetAspects[i] : 1.0f;
                AspectRatios[i] = (float)bounds.Aspect;

                ExpandToAspect(ref bounds, slotAspect);
                double texelX = bounds.Width / resolution;
                double texelY = bounds.Height / resolution;
                SnapToTexels(ref bounds, texelX, texelY);

                // The light camera sits at the cascade centre, so casters exist on both
                // sides of it: zNear must stay negative to keep those closer to the light.
                double zNear = -bounds.MaxZ - DepthMargin;
                double zFar = Math.Max(zNear + 1.0, -bounds.MinZ + DepthMargin);

                Matrix4D.CreateOrthographicOffCenter(
                    bounds.MinX, bounds.MaxX, bounds.MinY, bounds.MaxY, zNear, zFar, out Matrix4D lightProj);
                LightSpaceMatrices[i] = lightView * lightProj;
                SplitDistances[i] = (float)far;

                double worldTexel = Math.Max(texelX, texelY);
                CascadeBiases[i] = (float)(worldTexel / (zFar - zNear));
                TexelWorldSizes[i] = (float)worldTexel;
            }
        }

        private static Matrix4D CreateLightView(Vector3 lightDirection, Matrix4D invView, double centerViewZ)
        {
            Vector3 center = new Vector3(0, 0, centerViewZ);
            Vector3 worldCenter = TransformPoint(center, invView);
            Vector3 up = Math.Abs(lightDirection.Y) < 0.99
                ? new Vector3(0, 1, 0)
                : new Vector3(1, 0, 0);
            return Matrix4D.LookAt(worldCenter, worldCenter + lightDirection, up);
        }

        private static LightBounds FitBounds(Vector3[] corners, Matrix4D lightView)
        {
            LightBounds b = new LightBounds
            {
                MinX = double.MaxValue, MaxX = double.MinValue,
                MinY = double.MaxValue, MaxY = double.MinValue,
                MinZ = double.MaxValue, MaxZ = double.MinValue
            };
            foreach (Vector3 corner in corners)
            {
                Vector3 p = TransformPoint(corner, lightView);
                b.MinX = Math.Min(b.MinX, p.X); b.MaxX = Math.Max(b.MaxX, p.X);
                b.MinY = Math.Min(b.MinY, p.Y); b.MaxY = Math.Max(b.MaxY, p.Y);
                b.MinZ = Math.Min(b.MinZ, p.Z); b.MaxZ = Math.Max(b.MaxZ, p.Z);
            }
            if (b.Width < 1e-3) b.MaxX = b.MinX + 1e-3;
            if (b.Height < 1e-3) b.MaxY = b.MinY + 1e-3;
            return b;
        }

        /// <summary>Widens the ortho box until it matches the atlas slot's aspect ratio.</summary>
        private static void ExpandToAspect(ref LightBounds b, double slotAspect)
        {
            double requiredWidth = b.Height / Math.Max(0.05, slotAspect);
            if (b.Width >= requiredWidth) return;
            double centerX = (b.MinX + b.MaxX) / 2.0;
            b.MinX = centerX - requiredWidth / 2.0;
            b.MaxX = centerX + requiredWidth / 2.0;
        }

        /// <summary>Keeps valid depth beyond the frustum, then snaps the box to texels.</summary>
        private static void SnapToTexels(ref LightBounds b, double texelX, double texelY)
        {
            b.MinX -= EdgePadding * texelX;
            b.MaxX += EdgePadding * texelX;
            b.MinY -= EdgePadding * texelY;
            b.MaxY += EdgePadding * texelY;

            b.MinX = Math.Floor(b.MinX / texelX) * texelX;
            b.MinY = Math.Floor(b.MinY / texelY) * texelY;
            int countX = Math.Max(1, (int)Math.Ceiling((b.MaxX - b.MinX) / texelX));
            int countY = Math.Max(1, (int)Math.Ceiling((b.MaxY - b.MinY) / texelY));
            countX = (countX + ExtentQuantum - 1) / ExtentQuantum * ExtentQuantum;
            countY = (countY + ExtentQuantum - 1) / ExtentQuantum * ExtentQuantum;
            b.MaxX = b.MinX + countX * texelX;
            b.MaxY = b.MinY + countY * texelY;
        }

        /// <summary>Extracts the far clip distance from a projection matrix.</summary>
        private static double GetFarPlaneFromProjection(Matrix4D proj)
        {
            double a = proj.Row2.Z;
            double b = proj.Row3.Z;
            return Math.Abs(a + 1.0) < 1e-8 ? 1000.0 : b / (a + 1.0);
        }

        /// <summary>Transforms a 3D point through a 4x4 matrix with perspective divide.</summary>
        private static Vector3 TransformPoint(Vector3 p, Matrix4D m)
        {
            double x = p.X * m.Row0.X + p.Y * m.Row1.X + p.Z * m.Row2.X + m.Row3.X;
            double y = p.X * m.Row0.Y + p.Y * m.Row1.Y + p.Z * m.Row2.Y + m.Row3.Y;
            double z = p.X * m.Row0.Z + p.Y * m.Row1.Z + p.Z * m.Row2.Z + m.Row3.Z;
            double w = p.X * m.Row0.W + p.Y * m.Row1.W + p.Z * m.Row2.W + m.Row3.W;
            return Math.Abs(w) > 1e-10 ? new Vector3(x / w, y / w, z / w) : new Vector3(x, y, z);
        }
    }
}
