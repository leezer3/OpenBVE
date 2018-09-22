namespace OpenBveApi.Objects
{
	/// <summary>The available distance attenuation modes for glow</summary>
	public enum GlowAttenuationMode
	{
		/// <summary>The glow intensity remains constant</summary>
		None = 0,
		/// <summary>The glow intensity is determined via the function x² / (x² + GlowHalfDistance2), where x is the distance from the camera to the object in meters. </summary>
		DivisionExponent2 = 1,
		/// <summary>The glow intensity is determined via the function x⁴ / (x⁴ + GlowHalfDistance4), where x is the distance from the camera to the object in meters.</summary>
		DivisionExponent4 = 2,
	}
}
