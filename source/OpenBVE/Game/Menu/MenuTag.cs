// ReSharper disable UnusedMember.Global
namespace OpenBve
{
	/// <summary>The list of possible tags for a menu entry- These define the functionality of a given menu entry</summary>
	public enum MenuTag
	{
		/// <summary>Is unselectable</summary>
		Unselectable,
		/// <summary>Has no functionality/ is blank</summary>
		None,
		/// <summary>Is a caption for another menu item</summary>
		Caption,
		/// <summary>Moves up a menu level</summary>
		MenuBack,
		/// <summary>Enters the submenu containing the list of stations to which the player train may be jumped</summary>
		MenuJumpToStation,
		/// <summary>Enters the submenu for exiting to the main menu</summary>
		MenuExitToMainMenu,
		/// <summary>Enters the submenu for customising controls</summary>
		MenuControls,
		/// <summary>Enters the submenu for quitting the program</summary>
		MenuQuit,
		/// <summary>Returns to the simulation</summary>
		BackToSim,
		/// <summary>Jumps to the selected station</summary>
		JumpToStation,
		/// <summary>Exits to the main menu</summary>
		ExitToMainMenu,
		/// <summary>Quits the program</summary>
		Quit,
		/// <summary>Resets the controls to default</summary>
		ControlReset,
		/// <summary>Customises the selected control</summary>
		Control,
		/// <summary>Displays a list of routefiles</summary>
		RouteList,
		/// <summary>Selects a routefile to load</summary>
		RouteFile,
		/// <summary>A directory</summary>
		Directory,
		/// <summary>Enters the parent directory</summary>
		ParentDirectory,
		/// <summary>Selects Yes for a menu choice</summary>
		Yes,
		/// <summary>Selects No for a menu choice</summary>
		No,
		/// <summary>A train directory</summary>
		TrainDirectory,
		/// <summary>Shows the packages sub-menu</summary>
		Packages,
		/// <summary>Shows the package install menu</summary>
		PackageInstall,
		/// <summary>Shows the package uninstall menu</summary>
		PackageUninstall,
		/// <summary>Selects a file</summary>
		File,
		/// <summary>Selects a package</summary>
		Package,
		/// <summary>Uninstalls a route</summary>
		UninstallRoute,
		/// <summary>Uninstalls a train</summary>
		UninstallTrain,
		/// <summary>Uninstalls anything else</summary>
		UninstallOther,
		/// <summary>Shows the options menu</summary>
		Options,
		/// <summary>Shows the language list menu</summary>
		LanguageList,
		/// <summary>Selects a language</summary>
		LanguageSelect
	}
}
