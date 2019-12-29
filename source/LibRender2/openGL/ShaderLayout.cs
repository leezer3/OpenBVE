namespace LibRender2
{
	/// <summary>
	/// Vertex layout
	/// </summary>
	public class VertexLayout
	{
		/// <summary>
		/// The handle of "iPosition" within the shader
		/// </summary>
		public short Position = -1;

		/// <summary>
		/// The handle of "iNormal" within the shader
		/// </summary>
		public short Normal = -1;

		/// <summary>
		/// The handle of "iUv" within the shader
		/// </summary>
		public short UV = -1;

		/// <summary>
		/// The handle of "iColor" within the shader
		/// </summary>
		public short Color = -1;
	}

	public class UniformLayout
	{
		/// <summary>
		/// The handle of "uCurrentProjectionMatrix" within the shader
		/// </summary>
		public short CurrentProjectionMatrix = -1;

		/// <summary>
		/// The handle of "uCurrentModelViewMatrix" within the shader
		/// </summary>
		public short CurrentModelViewMatrix = -1;
		
		/// <summary>
		/// The handle of "uCurrentTextureMatrix" within the shader
		/// </summary>
		public short CurrentTextureMatrix = -1;

		/// <summary>
		/// The handle of "uIsLight" within the shader
		/// </summary>
		public short IsLight = -1;

		/// <summary>
		/// The handle of "uLight.position" within the shader
		/// </summary>
		public short LightPosition = -1;

		/// <summary>
		/// The handle of "uLight.ambient" within the shader
		/// </summary>
		public short LightAmbient = -1;

		/// <summary>
		/// The handle of "uLight.diffuse" within the shader
		/// </summary>
		public short LightDiffuse = -1;

		/// <summary>
		/// The handle of "uLight.specular" within the shader
		/// </summary>
		public short LightSpecular = -1;

		/// <summary>
		/// The handle of "uMaterial.ambient" within the shader
		/// </summary>
		public short MaterialAmbient = -1;

		/// <summary>
		/// The handle of "uMaterial.diffuse" within the shader
		/// </summary>
		public short MaterialDiffuse = -1;

		/// <summary>
		/// The handle of "uMaterial.specular" within the shader
		/// </summary>
		public short MaterialSpecular = -1;

		/// <summary>
		/// The handle of "uMaterial.emission" within the shader
		/// </summary>
		public short MaterialEmission = -1;

		/// <summary>
		/// The handle of "uMaterial.shininess" within the shader
		/// </summary>
		public short MaterialShininess = -1;

		/// <summary>
		/// The handle of "uMaterial.isEmmisive" within the shader
		/// </summary>
		public short MaterialIsEmissive = -1;

		/// <summary>
		/// The handle of "uMaterial.isEmmisive" within the shader
		/// </summary>
		public short MaterialIsAdditive = -1;

		/// <summary>
		/// The handle of "uIsFog" within the shader
		/// </summary>
		public short IsFog = -1;

		/// <summary>
		/// The handle of "uFogStart" within the shader
		/// </summary>
		public short FogStart = -1;

		/// <summary>
		/// The handle of "uFogEnd" within the shader
		/// </summary>
		public short FogEnd = -1;

		/// <summary>
		/// The handle of "uFogColor" within the shader
		/// </summary>
		public short FogColor = -1;

		/// <summary>
		/// The handle of "uIsTexture" within the shader
		/// </summary>
		public short IsTexture = -1;

		/// <summary>
		/// The handle of "uTexture" within the shader
		/// </summary>
		public short Texture = -1;

		/// <summary>
		/// The handle of "uBrightness" within the shader
		/// </summary>
		public short Brightness = -1;

		/// <summary>
		/// The handle of "uOpacity" within the shader
		/// </summary>
		public short Opacity = -1;

		/// <summary>
		/// The handle of "uObjectIndex" within the shader
		/// </summary>
		public short ObjectIndex = -1;
	}
}
