// ReSharper disable InconsistentNaming
namespace Plugin.BMP
{
	/// <summary>Compression format for the bitmap format</summary>
	enum CompressionFormat
	{
		/// <summary>No compression</summary>
		BI_RGB = 0,
		/// <summary>8bit RLE</summary>
		BI_RLE8 = 1,
		/// <summary>4bit RLE</summary>
		BI_RLE4 = 2,
		/// <summary>Bitfield encoded color data</summary>
		BITFIELDS = 3,
		/// <summary>24bit RLE</summary>
		BI_RLE24 = 4
	}
}
