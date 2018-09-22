namespace OpenBveApi.Objects
{
	/// <summary>The different types of object</summary>
	public enum ObjectType : byte
	{
		/// <summary>The object is part of the static scenery. The matching ObjectListType is StaticOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
		Static = 1,
		/// <summary>The object is part of the animated scenery or of a train exterior. The matching ObjectListType is DynamicOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
		Dynamic = 2,
		/// <summary>The object is part of the cab. The matching ObjectListType is OverlayOpaque for fully opaque faces, and OverlayAlpha for all other faces.</summary>
		Overlay = 3
	}
}
