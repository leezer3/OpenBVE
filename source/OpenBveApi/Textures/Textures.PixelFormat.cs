namespace OpenBveApi.Textures
{
	/// <summary>Describes the format of a pixel</summary>
	public enum PixelFormat
	{
		/// <summary>The pixel format is invalid</summary>
		Invalid = 0,
		/// <summary>The pixel is a single grayscale byte</summary>
		Grayscale = 1,
		/// <summary>The pixel is a grayscale byte and an alpha byte</summary>
		GrayscaleAlpha = 2,
		/// <summary>The pixel is a RGB triple</summary>
		RGB = 3,
		/// <summary>The pixel is a RGB triple and an alpha byte</summary>
		RGBAlpha = 4

	}
}
