namespace Plugin
{
	internal partial class GifDecoder
	{
		internal enum DecoderStatus
		{
			/// <summary>No problems</summary>
			OK = 0,
			/// <summary>The GIF was of invalid format</summary>
			/// <remarks>The file may have been partially decoded</remarks>
			FormatError = 1,
			/// <summary>An error occured opening the GIF</summary>
			OpenError = 2
		}
	}
}
