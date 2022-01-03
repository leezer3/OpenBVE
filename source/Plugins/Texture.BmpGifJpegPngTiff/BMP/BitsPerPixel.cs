namespace Plugin
{
	/// <summary>Bits per pixel for the bitmap format</summary>
	internal enum BitsPerPixel
	{
		/// <summary>Single color black / white</summary>
		Monochrome = 1,
		/// <summary>16 colors</summary>
		FourBitPalletized = 4,
		/// <summary>256 colors</summary>
		EightBitPalletized = 8,
		/// <summary>65536 colors</summary>
		SixteenBitRGB = 16,
		/// <summary>16 million colors</summary>
		TwentyFourBitRGB = 24
	}
}
