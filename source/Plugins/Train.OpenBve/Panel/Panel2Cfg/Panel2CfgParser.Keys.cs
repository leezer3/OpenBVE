namespace Train.OpenBve.Panel2Cfg
{
	/// <summary>The different keys which can be found within a Panel2.cfg section</summary>
	internal enum Key
	{
		/// <summary>Invalid</summary>
		Invalid = 0,
		/// <summary>Sets the controlling animation subject</summary>
		Subject = 1,
		/// <summary>Sets the location of the element relative to the panel bitmap</summary>
		Location = 2,
		/// <summary>Sets the daytime texture for the element</summary>
		DaytimeImage = 3,
		/// <summary>Sets the nighttime texture for the element</summary>
		NighttimeImage = 4,
		/// <summary>Sets the transparent color used for the element's texture</summary>
		TransparentColor = 5,
		TransparentColour = 5,
		/// <summary>Sets the layer within the panel stack the element is created on</summary>
		Layer = 6,
		/// <summary>Sets the radius of a needle element</summary>
		Radius = 7,
		/// <summary>Sets the origin of a needle element</summary>
		Origin = 8,
		/// <summary>Sets the inital angle of a needle element</summary>
		/// <remarks>When the subject is at minimum</remarks>
		InitialAngle = 9,
		/// <summary>Sets the last angle of a needle element</summary>
		/// <remarks>When the subject is at maximum</remarks>
		LastAngle = 10,
		/// <summary>Sets the minimum value for angle calculations</summary>
		Minimum = 11,
		/// <summary>Sets the maximum value for angle calculations</summary>
		Maximum = 12,
		/// <summary>Sets the natural frequency for needle rotations</summary>
		NaturalFreq = 13,
		NaturalFrequency = 13,
		/// <summary>Sets the damping ratio for needle rotations</summary>
		DampingRatio = 14,
		/// <summary>Whether a needle has a backstop</summary>
		/// <remarks>If no backstop is present, then the needle will rotate past the minimum / maximum angles</remarks>
		Backstop = 15,
		/// <summary>Sets the width of an element</summary>
		Width = 16,
		/// <summary>Sets the height of an element</summary>
		Height = 17,
		/// <summary>Sets the movement / rotation direction</summary>
		Direction = 18,
		/// <summary>Sets the interval for DigitalNumber elements</summary>
		Interval = 19,
		/// <summary>Sets the step for DigitalNumber elements</summary>
		Step = 20,
		/// <summary>Sets the top left of the windscreen area</summary>
		TopLeft = 21,
		/// <summary>Sets the bottom right of the windscreen area</summary>
		BottomRight = 22,
		/// <summary>Sets the number of drops to be generated on the windscreen</summary>
		NumberOfDrops = 23,
		/// <summary>Sets the size in pixels of a drop</summary>
		DropSize = 24,
		/// <summary>Loads the daytime drop textures</summary>
		DaytimeDrops = 25,
		/// <summary>Loads the nighttime drop textures</summary>
		NighttimeDrops = 26,
		/// <summary>Loads the daytime snowflake textures</summary>
		DaytimeFlakes = 27,
		/// <summary>Loads the nighttime snowflake textures</summary>
		NighttimeFlakes = 28,
		/// <summary>Sets the speed at which the windscreen wipers sweep the screen</summary>
		WipeSpeed = 29,
		/// <summary>Sets the time for which the wipers hold at the rest position</summary>
		WiperHoldTime = 30,
		/// <summary>Sets the wiper rest position</summary>
		RestPosition = 31,
		WiperRestPosition = 31,
		/// <summary>Sets the wiper hold position</summary>
		HoldPosition = 32,
		WiperHoldPosition = 32,
		/// <summary>Sets the lifetime for a raindrop</summary>
		DropLife = 33,
		/// <summary>Sets the color of an element</summary>
		Color = 34,
		Colour = 34,
		/// <summary>Whether the rotation of an element is smoothed</summary>
		Smoothed = 35,
		/// <summary>Sets the resolution of the main panel element</summary>
		Resolution = 36,
		/// <summary>Sets the left edge of the main panel element relative to the texture</summary>
		Left = 37,
		/// <summary>Sets the right edge of the main panel element relative to the texture</summary>
		Right = 38,
		/// <summary>Sets the top edge of the main panel element relative to the texture</summary>
		Top = 39,
		/// <summary>Sets the bottom edge of the main panel element relative to the texture</summary>
		Bottom = 40,
		/// <summary>Sets the center of the main panel element relative to the texture</summary>
		Center = 41,
		/// <summary>The element is a numerical value used when activated</summary>
		/// <remarks>Screen switch, touch etc.</remarks>
		Number = 42,
		/// <summary>The element contains a size</summary>
		Size = 43,
		/// <summary>The element triggers a screen jump</summary>
		JumpScreen = 44,
		/// <summary>The element contains a sound index used when activated</summary>
		SoundIndex = 45,
		/// <summary>The element contains a list of sound indicies corresponding to states</summary>
		SoundEntries = 46,
		/// <summary>The element contains the command used when activated</summary>
		Command = 47,
		/// <summary>The element contains a list of command entries corresponding to states</summary>
		CommandEntries = 48,
		/// <summary>The option to be supplied to the command</summary>
		/// <remarks>e.g. Power notch etc.</remarks>
		CommandOption = 49
	}
}
