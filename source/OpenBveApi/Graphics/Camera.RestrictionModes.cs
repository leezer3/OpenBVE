namespace OpenBveApi.Graphics
{
	/// <summary>Describes the different possible camera restriction modes</summary>
	public enum CameraRestrictionMode
	{
		/// <summary>Represents a 3D cab.</summary>
		NotAvailable = -1,
		/// <summary>Represents a 2D cab with camera restriction disabled.</summary>
		Off = 0,
		/// <summary>Represents a 2D cab with camera restriction enabled.</summary>
		On = 1,
		/// <summary>A 3D cab with developer specified restrictions</summary>
		Restricted3D = 2,
		/// <summary>The camera restriction mode is not specified (Used by parsers)</summary>
		NotSpecified = 3

	}
}
