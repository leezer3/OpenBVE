namespace Plugin.PNG
{
	/// <summary>The available scanline filter algorithms</summary>
	internal enum ScanlineFilterAlgorithm : byte
	{
		/// <summary>Each byte is unchanged</summary>
		None,
		/// <summary>Each byte is replaced with the difference between it and the corresponding byte to its left</summary>
		Sub,
		/// <summary>Each byte is replaced with the difference between it and the byte above it (in the previous row, as it was before filtering)</summary>
		Up,
		/// <summary>Each byte is replaced with the difference between it and the average of the corresponding bytes to its left and above it, truncating any fractional part</summary>
		Average,
		/// <summary>Each byte is replaced with the difference between it and the Paeth predictor of the corresponding bytes to its left, above it, and to its upper left</summary>
		Paeth
	}
}
