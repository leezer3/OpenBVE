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
		RGBAlpha = 4,
		/// <summary>The pixel is a single palette index byte (requires Palette)</summary>
		Paletted = 5

	}

	/// <summary>Helper for PixelFormat</summary>
	public static class PixelFormatExtensions
	{
		/// <summary>Gets the bytes per pixel for the format (palette index counts as 1)</summary>
		public static int BytesPerPixel(this PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Grayscale: return 1;
				case PixelFormat.Paletted: return 1;
				case PixelFormat.GrayscaleAlpha: return 2;
				case PixelFormat.RGB: return 3;
				case PixelFormat.RGBAlpha: return 4;
				default: return 0;
			}
		}
	}
}
