using System;

namespace OpenBveApi.Textures
{
	/// <summary>Represents how the texture wraps on each axis.</summary>
	[Flags]
	public enum OpenGlTextureWrapMode
	{
		/// <summary>The texture is clamped to edge on both axes.</summary>
		/// <remarks>The numerical value is 0.</remarks>
		ClampClamp = 0,
		/// <summary>The texture is clamped to edge on the x-axis and repeats on the y-axis.</summary>
		/// <remarks>The numerical value is 1.</remarks>
		ClampRepeat = 1,
		/// <summary>The texture repeats on the x-axis and is clamped to edge on the y-axis.</summary>
		/// <remarks>The numerical value is 2.</remarks>
		RepeatClamp = 2,
		/// <summary>The texture repeats on both axes.</summary>
		/// <remarks>The numerical value is 3.</remarks>
		RepeatRepeat = 3,
	}
}
