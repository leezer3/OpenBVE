using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
	public static partial class Renderer
	{
		/// <summary>The current ambient light color</summary>
		public static Color24 OptionAmbientColor = new Color24(160, 160, 160);
		/// <summary>The current diffuse light color</summary>
		public static Color24 OptionDiffuseColor = new Color24(160, 160, 160);

		/// <summary>Initializes the lighting model</summary>
		public static void InitializeLighting()
		{
			GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float) OptionAmbientColor.R, inv255 * (float) OptionAmbientColor.G, inv255 * (float) OptionAmbientColor.B, 1.0f });
			GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float) OptionDiffuseColor.R, inv255 * (float) OptionDiffuseColor.G, inv255 * (float) OptionDiffuseColor.B, 1.0f });
			GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			GL.CullFace(CullFaceMode.Front); CullEnabled = true; // possibly undocumented, but required for correct lighting
			GL.Enable(EnableCap.Light0);
			GL.Enable(EnableCap.ColorMaterial);
			GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
			GL.ShadeModel(ShadingModel.Smooth);
			float x = (float) OptionAmbientColor.R + (float) OptionAmbientColor.G + (float) OptionAmbientColor.B;
			float y = (float) OptionDiffuseColor.R + (float) OptionDiffuseColor.G + (float) OptionDiffuseColor.B;
			if (x < y) x = y;
			OptionLightingResultingAmount = 0.00208333333333333f * x;
			if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
			GL.Enable(EnableCap.Lighting); LightingEnabled = true;
			GL.DepthFunc(DepthFunction.Lequal);
		}
	}
}
