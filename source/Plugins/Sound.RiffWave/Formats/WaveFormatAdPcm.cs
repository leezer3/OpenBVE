using OpenBveApi.Math;

namespace Plugin
{
	/// <summary>A Wave file in AD-PCM format</summary>
	internal class WaveFormatAdPcm : WaveFormatEx
	{
		internal struct BlockData
		{
			internal readonly byte[] bPredictor;
			internal readonly short[] iDelta;
			internal readonly short[] iSamp1;
			internal readonly short[] iSamp2;
			internal readonly Vector2[] CoefSet;

			internal BlockData(int channels)
			{
				bPredictor = new byte[channels];
				iDelta = new short[channels];
				iSamp1 = new short[channels];
				iSamp2 = new short[channels];
				CoefSet = new Vector2[channels];
			}
		}

		internal ushort nSamplesPerBlock;
		internal ushort nNumCoef;
		internal Vector2[] aCoeff;

		internal static readonly short[] AdaptionTable =
		{
			230, 230, 230, 230, 307, 409, 512, 614,
			768, 614, 512, 409, 307, 230, 230, 230
		};

		public WaveFormatAdPcm(ushort tag) : base(tag)
		{

		}
	}
}
