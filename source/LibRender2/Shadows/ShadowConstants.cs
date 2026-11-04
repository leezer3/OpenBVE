using OpenTK.Graphics.OpenGL;

namespace LibRender2.ShadowMapping
{
	/// <summary>
	/// Shared limits for the cascaded shadow mapping system.
	/// </summary>
	/// <remarks>
	/// The scene shader declares exactly 4 cascade slots (uShadowMap0..3 and friends)
	/// bound to texture units 4..7. Raising <see cref="MaxCascadeCount"/> requires
	/// matching GLSL changes — it is not just a number to turn up.
	/// </remarks>
	internal static class ShadowConstants
	{
		/// <summary>Maximum cascades supported by the scene shader.</summary>
		public const int MaxCascadeCount = 4;

		/// <summary>First texture unit reserved for shadow maps (units 4..7).</summary>
		public static readonly TextureUnit FirstShadowUnit = TextureUnit.Texture0 + FirstShadowSamplerIndex;

		/// <summary>Sampler index matching <see cref="FirstShadowUnit"/> (mirrors uShadowMap0..3).</summary>
		public const int FirstShadowSamplerIndex = 4;

		/// <summary>Fallback world-space texel size until shadow data is bound.</summary>
		public const float FallbackTexelWorldSize = 0.05f;
	}
}
