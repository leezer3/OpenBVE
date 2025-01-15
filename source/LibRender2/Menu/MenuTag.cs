//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Maurizo M. Gavioli, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// ReSharper disable UnusedMember.Global
namespace LibRender2.Menu
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
		/// <summary>Enters the submenu for tools</summary>
		MenuTools,
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
		/// <summary>Shows the tools menu</summary>
		Tools,
		/// <summary>Toggles a switch</summary>
		ToggleSwitch,
		/// <summary>Selects the previous switch</summary>
		PreviousSwitch,
		/// <summary>Selects the next switch</summary>
		NextSwitch,
		/// <summary>Launches Object Viewer</summary>
		ObjectViewer,
		/// <summary>Launches Route Viewer</summary>
		RouteViewer,
		/// <summary>Views the previous log file</summary>
		ViewLog,

		// OBJECT VIEWER
		/// <summary>Displays a list of objects</summary>
		ObjectList,
		/// <summary>Selects an object file to load</summary>
		ObjectFile,
		/// <summary>Shows the list of current errors</summary>
		ErrorList
	}
}
