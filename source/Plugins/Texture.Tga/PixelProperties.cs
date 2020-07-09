namespace Plugin
{
	internal enum VerticalTransferOrder
	{
		/// <summary>Unknown</summary>
		Unknown = -1,
		/// <summary>Bottom to top</summary>
		Bottom = 0,
		/// <summary>Top to bottom</summary>
		Top = 1
	}

	internal enum HorizontalTransferOrder
	{
		/// <summary>Unknown</summary>
		Unknown = -1,
		/// <summary>Right to left</summary>
		Right = 0,
		/// <summary>Left to right</summary>
		Left = 1
	}

	internal enum FirstPixelDestination
	{
		/// <summary>Unknown- Assume bottom right</summary>
		/// <remarks>Right to left, bottom to top pixel order</remarks>
		Unknown = -1,
		/// <summary>Bottom left</summary>
		/// <remarks>Left to right, bottom to top pixel order</remarks>
		BottomLeft = 0,
		/// <summary>Bottom right</summary>
		/// /// <remarks>Right to left, bottom to top pixel order</remarks>
		BottomRight = 1,
		/// <summary>Top left</summary>
		/// <remarks>Left to right, top to bottom pixel order</remarks>
		TopLeft = 2,
		/// <summary>Top right</summary>
		/// <remarks>Right to left, top to bottom pixel order</remarks>
		TopRight = 3
	}
}
