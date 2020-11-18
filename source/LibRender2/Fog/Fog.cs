using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Fogs
{
	public class Fog
	{
		/// <summary>The offset at which the fog starts</summary>
		public float Start;

		/// <summary>The offset at which the fog ends</summary>
		public float End;

		/// <summary>The fog density</summary>
		public float Density;

		/// <summary>Whether the fog is linear</summary>
		public bool IsLinear;

		/// <summary>The color of the fog</summary>
		public Color24 Color;

		public void SetForImmediateMode()
		{
			const float inv255 = 1.0f / 255.0f;
			if (IsLinear)
			{
				GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				GL.Fog(FogParameter.FogStart, Start);
				GL.Fog(FogParameter.FogEnd, End);
			}
			else
			{
				GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);
				GL.Fog(FogParameter.FogDensity, Density);	
			}
			
			
			GL.Fog(FogParameter.FogColor, new[] { inv255 * Color.R, inv255 * Color.G, inv255 * Color.B, 1.0f });
		}
	}
}
