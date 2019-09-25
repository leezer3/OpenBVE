namespace OpenBveApi.Graphics
{
	/// <summary>
	/// Vertex layout
	/// </summary>
	public class VertexLayout
	{
		/// <summary>
		/// The handle of "iPosition" within the shader
		/// </summary>
		public int Position = -1;

		/// <summary>
		/// The handle of "iNormal" within the shader
		/// </summary>
		public int Normal = -1;

		/// <summary>
		/// The handle of "iUv" within the shader
		/// </summary>
		public int UV = -1;

		/// <summary>
		/// The handle of "iColor" within the shader
		/// </summary>
		public int Color = -1;
	}

	public class UniformLayout
	{
		/// <summary>
		/// The handle of "uCurrentTranslateMatrix" within the shader
		/// </summary>
		public int CurrentTranslateMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentScaleMatrix" within the shader
		/// </summary>
		public int CurrentScaleMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentRotateMatrix" within the shader
		/// </summary>
		public int CurrentRotateMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentTextureTranslateMatrix" within the shader
		/// </summary>
		public int CurrentTextureTranslateMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentProjectionMatrix" within the shader
		/// </summary>
		public int CurrentProjectionMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentViewMatrix" within the shader
		/// </summary>
		public int CurrentViewMatrix = -1;

		/// <summary>
		/// The handle of "uEyePosition" within the shader
		/// </summary>
		public int EyePosition = -1;

		/// <summary>
		/// The handle of "uIsLight" within the shader
		/// </summary>
		public int IsLight = -1;

		/// <summary>
		/// The handle of "uLight.position" within the shader
		/// </summary>
		public int LightPosition = -1;

		/// <summary>
		/// The handle of "uLight.ambient" within the shader
		/// </summary>
		public int LightAmbient = -1;

		/// <summary>
		/// The handle of "uLight.diffuse" within the shader
		/// </summary>
		public int LightDiffuse = -1;

		/// <summary>
		/// The handle of "uLight.specular" within the shader
		/// </summary>
		public int LightSpecular = -1;

		/// <summary>
		/// The handle of "uMaterial.ambient" within the shader
		/// </summary>
		public int MaterialAmbient = -1;

		/// <summary>
		/// The handle of "uMaterial.diffuse" within the shader
		/// </summary>
		public int MaterialDiffuse = -1;

		/// <summary>
		/// The handle of "uMaterial.specular" within the shader
		/// </summary>
		public int MaterialSpecular = -1;

		/// <summary>
		/// The handle of "uMaterial.emission" within the shader
		/// </summary>
		public int MaterialEmission = -1;

		/// <summary>
		/// The handle of "uMaterial.shininess" within the shader
		/// </summary>
		public int MaterialShininess = -1;

		/// <summary>
		/// The handle of "uIsFog" within the shader
		/// </summary>
		public int IsFog = -1;

		/// <summary>
		/// The handle of "uFogStart" within the shader
		/// </summary>
		public int FogStart = -1;

		/// <summary>
		/// The handle of "uFogEnd" within the shader
		/// </summary>
		public int FogEnd = -1;

		/// <summary>
		/// The handle of "uFogColor" within the shader
		/// </summary>
		public int FogColor = -1;

		/// <summary>
		/// The handle of "uIsTexture" within the shader
		/// </summary>
		public int IsTexture = -1;

		/// <summary>
		/// The handle of "uTexture" within the shader
		/// </summary>
		public int Texture = -1;

		/// <summary>
		/// The handle of "uBrightness" within the shader
		/// </summary>
		public int Brightness = -1;

		/// <summary>
		/// The handle of "uOpacity" within the shader
		/// </summary>
		public int Opacity = -1;

		/// <summary>
		/// The handle of "uIsAlphaTest" within the shader
		/// </summary>
		public int IsAlphaTest = -1;

		/// <summary>
		/// The handle of "uAlphaFuncType" within the shader
		/// </summary>
		public int AlphaFuncType = -1;

		/// <summary>
		/// The handle of "uAlphaFuncValue" within the shader
		/// </summary>
		public int AlphaFuncValue = -1;

		/// <summary>
		/// The handle of "uObjectIndex" within the shader
		/// </summary>
		public int ObjectIndex = -1;
	}

	public enum AlphaFuncType
	{
		Never = 0,
		Less = 1,
		Equal = 2,
		Lequal = 3,
		Greater = 4,
		Notequal = 5,
		Gequal = 6,
		Always = 7
	}
}
