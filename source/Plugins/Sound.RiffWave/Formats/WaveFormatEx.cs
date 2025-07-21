namespace Plugin
{
	/// <summary>Common properties for all Wave formats</summary>
	internal abstract class WaveFormatEx
	{
		internal readonly WFormatTag FormatTag;
		internal ushort Channels;
		internal uint SamplesPerSec;
		internal uint AvgBytesPerSec;
		internal ushort BlockAlign;
		internal ushort BitsPerSample;
		internal ushort cbSize;

		protected WaveFormatEx(ushort tag)
		{
			FormatTag = (WFormatTag)tag;
		}
	}
}
