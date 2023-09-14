namespace Plugin.PNG
{
	/// <summary>The color types</summary>
	internal enum ColorType : byte
	{
		/// <summary>Each pixel is a grayscale sample</summary>
		Grayscale = 0,
		/// <summary>Each pixel is an R,G,B triple</summary>
		Rgb = 2,
		/// <summary>Each pixel is a palette index</summary>
		///<remarks>A PLTE chunk must be present</remarks>
		Palleted = 3,
		/// <summary>Each pixel is a grayscale sample, followed by an alpha sample</summary>
		GrayscaleAlpha = 4,
		/// <summary>Each pixel is a R,G,B triple, followed by an alpha sample</summary>
		Rgba = 6
	}
}
