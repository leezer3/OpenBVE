namespace OpenBveApi.Graphics
{
	/// <summary>Defines the behaviour of the renderer for transparent textures</summary>
	public enum TransparencyMode
	{
		/// <summary>Textures using color-key transparency are considered opaque, producing good performance but crisp outlines. Partially transparent faces are rendered in a single pass with z-buffer writes disabled, producing good performance but more depth-sorting issues.</summary>
		Performance = 0,
		/// <summary>Textures using color-key transparency are considered opaque, producing good performance but crisp outlines. Partially transparent faces are rendered in two passes, the first rendering only opaque pixels with z-buffer writes enabled, and the second rendering only partially transparent pixels with z-buffer writes disabled, producing best quality but worse performance.</summary>
		Intermediate = 1,
		/// <summary>Textures using color-key transparency are considered partially transparent. All partially transparent faces are rendered in two passes, the first rendering only opaque pixels with z-buffer writes enabled, and the second rendering only partially transparent pixels with z-buffer writes disabled, producing best quality but worse performance.</summary>
		Quality = 2
	}
}
