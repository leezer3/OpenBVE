using System;
// ReSharper disable InconsistentNaming

namespace Plugin
{
    internal enum ChunkID : Int32
    {
		/// <summary>Format chunk</summary>
		FMT = 0x20746D66,
		/// <summary>Data chunk</summary>
		DATA = 0x61746164,
		/// <summary>List chunk</summary>
		LIST = 0x5453494c,
		/// <summary>ID3 chunk</summary>
		ID3 = 0x20336469,
		/// <summary>CUE chunk</summary>
		CUE = 0x20657563,
		/// <summary>PLST chunk</summary>
		PLST = 0x74736c70,
		/// <summary>SLNT chunk</summary>
		SLNT = 0x746e6c73
	}
}
