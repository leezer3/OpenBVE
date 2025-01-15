// ReSharper disable InconsistentNaming
namespace Train.OpenBve
{
	/// <summary>The different train.dat format types</summary>
	internal enum TrainDatFormats
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
		/// <summary>The train.dat was written for BVE 2.0.6</summary>
		BVE2060000 = 4,
		/// <summary>The train.dat was written for openBVE</summary>
		openBVE = 5,
		/// <summary>The train.dat was created by an unknown BVE version</summary>
		UnknownBVE = 100,
		/// <summary>The train.dat file is missing the required header</summary>
		MissingHeader
	}
}
