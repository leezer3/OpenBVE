// ReSharper disable UnusedMember.Global
using System;

namespace OpenBveApi.Graphics
{
	/// <summary>Represents the alignment of a text compared to a reference coordinate.</summary>
	[Flags]
	public enum TextAlignment
	{
		/// <summary>The reference coordinate represents the top-left corner.</summary>
		TopLeft = 1,
		/// <summary>The reference coordinate represents the top-middle corner.</summary>
		TopMiddle = 2,
		/// <summary>The reference coordinate represents the top-right corner.</summary>
		TopRight = 4,
		/// <summary>The reference coordinate represents the center-left corner.</summary>
		CenterLeft = 8,
		/// <summary>The reference coordinate represents the center-middle corner.</summary>
		CenterMiddle = 16,
		/// <summary>The reference coordinate represents the center-right corner.</summary>
		CenterRight = 32,
		/// <summary>The reference coordinate represents the bottom-left corner.</summary>
		BottomLeft = 64,
		/// <summary>The reference coordinate represents the bottom-middle corner.</summary>
		BottomMiddle = 128,
		/// <summary>The reference coordinate represents the bottom-right corner.</summary>
		BottomRight = 256,
		/// <summary>Represents the left for bitmasking.</summary>
		Left = TopLeft | CenterLeft | BottomLeft,
		/// <summary>Represents the (horizontal) middle for bitmasking.</summary>
		Middle = TopMiddle | CenterMiddle | BottomMiddle,
		/// <summary>Represents the right for bitmasking.</summary>
		Right = TopRight | CenterRight | BottomRight,
		/// <summary>Represents the top for bitmasking.</summary>
		Top = TopLeft | TopMiddle | TopRight,
		/// <summary>Represents the (vertical) center for bitmasking.</summary>
		Center = CenterLeft | CenterMiddle | CenterRight,
		/// <summary>Represents the bottom for bitmasking.</summary>
		Bottom = BottomLeft | BottomMiddle | BottomRight
	}
}
