namespace LibRender2.Screens
{
	/// <summary>The game's current interface type</summary>
	public enum InterfaceType
	{
		/// <summary>The game is currently showing the loading screen</summary>
		LoadScreen,
		/// <summary>The game is currently showing a normal display</summary>
		Normal,
		/// <summary>The game is currently showing the pause display</summary>
		Pause,
		/// <summary>The game is currently showing a menu</summary>
		Menu,
		/// <summary>The game is currently showing the OpenGL main menu</summary>
		GLMainMenu,
		/// <summary>The game is currently showing the switch change map</summary>
		SwitchChangeMap
	}
}
