namespace Train.OpenBve.PanelCfg
{
	/// <summary>The different keys which can be found within a Panel.cfg section</summary>
	internal enum Key
	{
		/// <summary>Invalid</summary>
		Invalid = 0,
		/// <summary>Sets the yaw for the panel element</summary>
		Yaw = 1,
		/// <summary>Sets the pitch for the panel element</summary>
		Pitch = 2,
		/// <summary>The type of element</summary>
		Type = 3,
		形態 = 3,
		/// <summary>A lower needle / hand element</summary>
		LowerNeedle = 4,
		LowerHand = 4,
		下針 = 4,
		/// <summary>An upper needle / hand element</summary>
		UpperNeedle = 5,
		UppperHand = 5,
		上針 = 5,
		/// <summary>Sets the center of an element</summary>
		Center = 6,
		中心 = 6,
		/// <summary>Sets the radius of a needle / hand element</summary>
		Radius = 7,
		半径 = 7,
		/// <summary>Sets the cover texture of an element</summary>
		Cover = 8,
		ふた = 8,
		/// <summary>Sets the units for an element's subject</summary>
		Unit = 9,
		単位 = 9,
		/// <summary>Sets the maximum for a needle / hand element</summary>
		Maximum = 10,
		最大 = 10,
		/// <summary>Sets the minimum for a needle / hand element</summary>
		Minimum = 11,
		最小 = 11,
		/// <summary>Sets the angle for a needle / hand element</summary>
		Angle = 12,
		角度 = 12,
		/// <summary>A generic needle / hand element</summary>
		Needle = 13,
		Hand = 13,
		針 = 13,
		/// <summary>An ATC lamp element</summary>
		/// <remarks>No SHIFT_JIS equivilant</remarks>
		ATC = 14,
		/// <summary>Sets the radius of the ATC speed display</summary>
		ATCRadius = 15,
		ATC半径 = 15,
		/// <summary>A number strip element</summary>
		Number = 16,
		数字 = 16,
		/// <summary>Sets the upper left corner of an element</summary>
		Corner = 17,
		左上 = 17,
		/// <summary>Sets the size of an element</summary>
		Size = 18,
		サイズ = 18,
		/// <summary>Sets the width of an element</summary>
		Width = 19,
		幅 = 19,
		/// <summary>Sets the texture to be displayed when a PilotLamp turns on</summary>
		TurnOn = 20,
		点灯 = 20,
		/// <summary>Sets the texture to be displayed when a PilotLamp turns off</summary>
		TurnOff = 21,
		消灯 = 21,
		/// <summary>Sets the texture for an element</summary>
		Image = 22,
		画像 = 22,
		/// <summary>Sets the background texture for an element</summary>
		Background = 23,
		背景 = 23,
	}
}
