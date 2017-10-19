namespace OpenBve
{
	internal static partial class TrainDatParser
	{
		private enum TrainDatFormats
		{
			/// <summary>The train.dat format string is unsupported</summary>
			Unsupported = -1,
			/// <summary>The train.dat was written for BVE 1.2.0</summary>
			BVE1200000 = 0,
			/// <summary>The train.dat was written for BVE 1.2.1</summary>
			BVE1210000 = 1,
			/// <summary>The train.dat was written for BVE 1.2.2</summary>
			BVE1220000 = 2,
			/// <summary>The train.dat was written for BVE 2</summary>
			BVE2000000 = 3,
			/// <summary>The train.dat was written for openBVE</summary>
			openBVE = 3,
		}
	}
}
