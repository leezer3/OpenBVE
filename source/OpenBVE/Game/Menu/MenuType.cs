namespace OpenBve
{
	/// <summary>The list of possible sub-menu types</summary>
	public enum MenuType
	{
		/// <summary>Not a sub menu</summary>
		None,
		/// <summary>Returns to the menu level above</summary>
		Top,
		/// <summary>The station jump menu</summary>
		JumpToStation,
		/// <summary>Returns to the main menu</summary>
		ExitToMainMenu,
		/// <summary>Provides a list of controls and allows customisation whilst in-game</summary>
		Controls,
		/// <summary>Customises the specified control</summary>
		Control,
		/// <summary>Quits the game</summary>
		Quit,
		/// <summary>The game start menu</summary>
		GameStart,
		/// <summary>Displays a list of routefiles</summary>
		RouteList,
		/// <summary>Asks whether the user wishes to use the default train</summary>
		TrainDefault,
		/// <summary>Displays a list of train folders</summary>
		TrainList,
		/// <summary>Displays the packages sub-menu</summary>
		Packages,
		/// <summary>Displays the package installation dialog</summary>
		PackageInstall
	}
}
