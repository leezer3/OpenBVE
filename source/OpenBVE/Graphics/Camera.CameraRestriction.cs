namespace OpenBve
{
	internal partial class Camera
	{
		internal enum RestrictionMode
		{
			/// <summary>Represents a 3D cab.</summary>
			NotAvailable = -1,
			/// <summary>Represents a 2D cab with camera restriction disabled.</summary>
			Off = 0,
			/// <summary>Represents a 2D cab with camera restriction enabled.</summary>
			On = 1,
			/// <summary>The camera restriction mode is not specified (Used by parsers)</summary>
			NotSpecified = 3

		}
	}
}
