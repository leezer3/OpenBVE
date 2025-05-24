// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Plugin.PNG
{
	/// <summary>The known types of chunk</summary>
	internal enum ChunkType
	{
		/// <summary>Unknown chunk type</summary>
		Unknown = 0,
		/// <summary>The image header</summary>
		IHDR,
		/// <summary>The color palette</summary>
		PLTE,
		/// <summary>Image data</summary>
		IDAT,
		/// <summary>Image end</summary>
		IEND,
		// ANCILLARY CHUNKS
		/// <summary>Background color to display</summary>
		bKGD,
		/// <summary>Chromacities / white point</summary>
		cHRM,
		/// <summary>Image gamma</summary>
		gAMA,
		/// <summary>Image histogram</summary>
		hIST,
		/// <summary>Physical pixel dimensions</summary>
		pHYs,
		/// <summary>Significant bits</summary>
		sBIT,
		/// <summary>Textual data</summary>
		tEXt,
		/// <summary>Last modification time</summary>
		tIME,
		/// <summary>Simple color key transparency data</summary>
		tRNS,
		/// <summary>Compressed textual data</summary>
		zTXt,
		/// <summary>Apple proprietary PNG for iPhone</summary>
		CgBI,
		/// <summary>Embedded color profile</summary>
		iCCP,
		/// <summary>Embedded textual information</summary>
		iTXt
	}
}
