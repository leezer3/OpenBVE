using OpenBveApi.Colors;

namespace LibRender2.Fogs
{
	public class Fog
	{
		/// <summary>Holds a reference to the base renderer</summary>
		private readonly BaseRenderer Renderer;

		public bool Enabled;

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

		public Fog(BaseRenderer renderer)
		{
			Renderer = renderer;
		}

		public void Set()
		{
			if (!Enabled)
			{
				return;
			}
			Renderer.CurrentShader.SetFog(true);
			Renderer.CurrentShader.SetFog(this);
        }
	}
}
