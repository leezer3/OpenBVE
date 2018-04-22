namespace Plugin
{
	/// <summary>Various pixel formats/compressors used by the DDS image.</summary>
	internal enum PixelFormat
	{
		/// <summary>32-bit image, with 8-bit red, green, blue and alpha.</summary>
		RGBA,
		/// <summary>24-bit image with 8-bit red, green, blue.</summary>
		RGB,
		/// <summary>16-bit DXT-1 compression, 1-bit alpha.</summary>
		DXT1,
		/// <summary>DXT-2 Compression</summary>
		DXT2,
		/// <summary>DXT-3 Compression</summary>
		DXT3,
		/// <summary>DXT-4 Compression</summary>
		DXT4,
		/// <summary>DXT-5 Compression</summary>
		DXT5,
		/// <summary>3DC Compression</summary>
		THREEDC,
		/// <summary>ATI1n Compression</summary>
		ATI1N,
		LUMINANCE,
		LUMINANCE_ALPHA,
		RXGB,
		A16B16G16R16,
		R16F,
		G16R16F,
		A16B16G16R16F,
		R32F,
		G32R32F,
		A32B32G32R32F,
		/// <summary>Unknown pixel format.</summary>
		UNKNOWN
	}
}
