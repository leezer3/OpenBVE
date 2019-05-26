namespace LibRender
{
	/// <summary>The type of object list</summary>
	public enum ObjectListType : byte
	{
		/// <summary>The face is fully opaque and originates from an object that is part of the static scenery.</summary>
		StaticOpaque = 1,
		/// <summary>The face is fully opaque and originates from an object that is part of the dynamic scenery or of a train exterior.</summary>
		DynamicOpaque = 2,
		/// <summary>The face is partly transparent and originates from an object that is part of the scenery or of a train exterior.</summary>
		DynamicAlpha = 3,
		/// <summary>The face is fully opaque and originates from an object that is part of the cab.</summary>
		OverlayOpaque = 4,
		/// <summary>The face is partly transparent and originates from an object that is part of the cab.</summary>
		OverlayAlpha = 5,
		/// <summary>The face is touch element and originates from an object that is part of the cab.</summary>
		Touch = 6
	}
}
