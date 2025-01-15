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

namespace LibRender2.Menu
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
		/// <summary>Resets the controls to default</summary>
		ControlReset,
		/// <summary>Provides a list of tools</summary>
		Tools,
		/// <summary>Quits the game</summary>
		Quit,
		/// <summary>The game start menu</summary>
		GameStart = 100,
		/// <summary>Displays a list of routefiles</summary>
		RouteList,
		/// <summary>Asks whether the user wishes to use the default train</summary>
		TrainDefault,
		/// <summary>Displays a list of train folders</summary>
		TrainList,
		/// <summary>Displays the packages sub-menu</summary>
		Packages,
		/// <summary>Displays the package installation dialog</summary>
		PackageInstall,
		/// <summary>Displays the package uninstall sub-menu</summary>
		PackageUninstall,
		/// <summary>Uninstalls a route</summary>
		UninstallRoute,
		/// <summary>Uninstalls a train</summary>
		UninstallTrain,
		/// <summary>Uninstalls anything else</summary>
		UninstallOther,
		/// <summary>The options menu</summary>
		Options,
		/// <summary>A menu allowing switch settings to be changed</summary>
		ChangeSwitch,
		/// <summary>Displays a list of object files</summary>
		ObjectList,
		/// <summary>Displays the current list of errors</summary>
		ErrorList,
		/// <summary>Shows an error</summary>
		Error
	}
}
