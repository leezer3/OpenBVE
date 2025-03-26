namespace Plugin
{
	internal enum ShaderNames
	{
		Unknown = 0,
		/// <summary>The material is rendered with no texture alpha, with no lighting applied</summary>
		Tex,
		/// <summary>The material is rendered with no texture alpha</summary>
		TexDiff,
		/// <summary>The material is rendered with no lighting applied</summary>
		BlendATex,
		/// <summary>The material is rendered with full lighting</summary>
		BlendATexDiff,
		/// <summary>The material is rendered with additive blending and no lighting applied</summary>
		AddATex,
		/// <summary>The material is rendered with additive blending</summary>
		AddATexDiff
	}
}
