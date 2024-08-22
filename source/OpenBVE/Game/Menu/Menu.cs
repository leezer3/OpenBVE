using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using LibRender2.Menu;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Text;
using OpenBve.Input;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Input;
using OpenBveApi.Packages;
using OpenBveApi.Textures;
using OpenTK;
using TrainManager;
using Path = OpenBveApi.Path;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve
{
	/********************
		MENU CLASS
	*********************
	Implements the in-game menu system; manages addition and removal of individual menus.
	Implemented as a singleton.
	Keeps a stack of menus, allowing navigating forward and back */

	/// <summary>Implements the in-game menu system; manages addition and removal of individual menus.</summary>
	public sealed partial class GameMenu : AbstractMenu
	{
		private static readonly Picturebox LogoPictureBox = new Picturebox(Program.Renderer);
		internal static List<FoundSwitch> nextSwitches = new List<FoundSwitch>();
		internal static List<FoundSwitch> previousSwitches = new List<FoundSwitch>();
		internal static bool switchesFound = false;
		
		private const float LineSpacing = 1.75f;    // the ratio between the font size and line distance
		private const int SelectionNone = -1;

		private double lastTimeElapsed;
		private static readonly string currentDatabaseFile = Path.CombineFile(Program.FileSystem.PackageDatabaseFolder, "packages.xml");

		/********************
			MENU SYSTEM FIELDS
		*********************/
		
		private int CustomControlIdx;   // the index of the control being customized
		private int em;                 // the size of menu font (in pixels)
		private bool isCustomisingControl = false;
		private bool isInitialized = false;

		internal OpenGlFont MenuFont => menuFont;

		internal Key MenuBackKey;

		/********************
			MENU SYSTEM SINGLETON C'TOR
		*********************/
		private static readonly GameMenu instance = new GameMenu();
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static GameMenu()
		{
		}
		private GameMenu() : base(Program.Renderer)
		{
		}

		/// <summary>Returns the current menu instance (If applicable)</summary>
		public static GameMenu Instance => instance;

		/********************
			MENU SYSTEM METHODS
		*********************/
		//
		// INITIALIZE THE MENU SYSTEM
		//
		private void Init()
		{
			Reset();
			// choose the text font size according to screen height
			// the boundaries follow approximately the progression
			// of font sizes defined in Graphics/Fonts.cs
			if (Program.Renderer.Screen.Height <= 512) menuFont = Program.Renderer.Fonts.SmallFont;
			else if (Program.Renderer.Screen.Height <= 680) menuFont = Program.Renderer.Fonts.NormalFont;
			else if (Program.Renderer.Screen.Height <= 890) menuFont = Program.Renderer.Fonts.LargeFont;
			else if (Program.Renderer.Screen.Height <= 1150) menuFont = Program.Renderer.Fonts.VeryLargeFont;
			else menuFont = Program.Renderer.Fonts.EvenLargerFont;

			if (Interface.CurrentOptions.UserInterfaceFolder == "Large")
			{
				// If using the large HUD option, increase the text size in the menu too
				menuFont = Program.Renderer.Fonts.NextLargestFont(menuFont);
			}

			em = (int)menuFont.FontSize;
			lineHeight = (int)(em * LineSpacing);
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				//Find the current menu back key- It's unlikely that we want to set a new key to this
				if (Interface.CurrentControls[i].Command == Translations.Command.MenuBack)
				{
					MenuBackKey = Interface.CurrentControls[i].Key;
					break;
				}
			}
			int quarterWidth = (int) (Program.Renderer.Screen.Width / 4.0);
			int quarterHeight = (int)(Program.Renderer.Screen.Height / 4.0);
			int descriptionLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 2;
			int descriptionWidth = quarterWidth + quarterWidth / 2;
			int descriptionHeight = descriptionWidth;
			if (descriptionHeight + quarterWidth > Program.Renderer.Screen.Height - 50)
			{
				descriptionHeight = Program.Renderer.Screen.Height - quarterWidth - 50;
			}
			routeDescriptionBox.Location = new Vector2(descriptionLoc, quarterWidth);
			routeDescriptionBox.Size = new Vector2(descriptionWidth, descriptionHeight);
			int imageLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 4;
			routePictureBox.Location = new Vector2(imageLoc, 0);
			routePictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			routePictureBox.BackgroundColor = Color128.White;
			switchMainPictureBox.Location = new Vector2(imageLoc, quarterHeight);
			switchMainPictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			switchMainPictureBox.BackgroundColor = Color128.Transparent;
			switchSettingPictureBox.Location = new Vector2(imageLoc, quarterHeight * 2);
			switchSettingPictureBox.Size = new Vector2(quarterWidth / 4.0, quarterWidth / 4.0);
			switchSettingPictureBox.BackgroundColor = Color128.Transparent;
			switchMapPictureBox.Location = new Vector2(imageLoc / 2.0, 0);
			switchMapPictureBox.Size = new Vector2(quarterWidth * 2.0, Program.Renderer.Screen.Height);
			LogoPictureBox.Location = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Height / 8.0);
			LogoPictureBox.Size = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Width / 2.0);
			LogoPictureBox.Texture = Program.Renderer.ProgramLogo;
			controlPictureBox.Location = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Height / 8.0);
			controlPictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			controlPictureBox.BackgroundColor = Color128.Transparent;
			controlTextBox.Location = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Height / 8.0 + quarterWidth);
			controlTextBox.Size = new Vector2(quarterWidth, quarterWidth);
			controlTextBox.BackgroundColor = Color128.Black;
			isInitialized = true;

			// add controls to list so we can itinerate them on mouse calls etc.
			menuControls.Add(routePictureBox);
			menuControls.Add(controlPictureBox);
			menuControls.Add(switchMainPictureBox);
			menuControls.Add(switchSettingPictureBox);
			menuControls.Add(switchMapPictureBox);
			menuControls.Add(LogoPictureBox);
			menuControls.Add(controlTextBox);
			menuControls.Add(routeDescriptionBox);
		}

		public override void Reset()
		{
			CurrMenu = -1;
			Menus = new MenuBase[] { };
			isCustomisingControl = false;
			routeDescriptionBox.CurrentlySelected = false;
		}


		/// <summary>Pushes a menu into the menu stack</summary>
		/// <param name= "type">The type of menu to push</param>
		/// <param name= "data">The index of the menu in the menu stack (If pushing an existing higher level menu)</param>
		/// <param name="replace">Whether we are replacing the selected menu item</param>
		public override void PushMenu(MenuType type, int data = 0,  bool replace = false)
		{
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu)
			{
				// Deliberately set to the standard cursor, as touch controls may have set to something else
				Program.Renderer.SetCursor(MouseCursor.Default);
			}
			if (!isInitialized)
				Init();
			if (!replace)
			{
				CurrMenu++;
			}
			
			if (Menus.Length <= CurrMenu)
				Array.Resize(ref Menus, CurrMenu + 1);
			int MaxWidth = 0;
			if ((int)type >= 100)
			{
				MaxWidth = Program.Renderer.Screen.Width / 2;
			}
			Menus[CurrMenu] = new SingleMenu(type, data, MaxWidth);
			if (replace)
			{
				Menus[CurrMenu].Selection = 1;
			}
			ComputePosition();
			Program.Renderer.CurrentInterface = TrainManager.PlayerTrain == null ? InterfaceType.GLMainMenu : InterfaceType.Menu;
			
		}
		
		
		/// <summary>Whether we are currently customising a control (Used for key/ joystick capture)</summary>
		/// <returns>True if currently capturing a control, false otherwise</returns>
		public bool IsCustomizingControl()
		{
			return isCustomisingControl;
		}

		//
		// SET CONTROL CUSTOM DATA
		//
		internal void SetControlKbdCustomData(Key key, KeyboardModifier keybMod)
		{
			//Check that we are customising a key, and that our key is NOT the menu back key
			if (isCustomisingControl && key != MenuBackKey && CustomControlIdx < Interface.CurrentControls.Length)
			{
				Interface.CurrentControls[CustomControlIdx].Method = ControlMethod.Keyboard;
				Interface.CurrentControls[CustomControlIdx].Key = key;
				Interface.CurrentControls[CustomControlIdx].Modifier = keybMod;
				Interface.SaveControls(null, Interface.CurrentControls);
			}
			PopMenu();
			isCustomisingControl = false;

		}
		internal void SetControlJoyCustomData(Guid device, JoystickComponent component, int element, int dir)
		{
			if (isCustomisingControl && CustomControlIdx < Interface.CurrentControls.Length)
			{
				Interface.CurrentControls[CustomControlIdx].Method = Program.Joysticks.AttachedJoysticks[device] is AbstractRailDriver ? ControlMethod.RailDriver : ControlMethod.Joystick;
				Interface.CurrentControls[CustomControlIdx].Device = device;
				Interface.CurrentControls[CustomControlIdx].Component = component;
				Interface.CurrentControls[CustomControlIdx].Element = element;
				Interface.CurrentControls[CustomControlIdx].Direction = dir;
				Interface.SaveControls(null, Interface.CurrentControls);
				PopMenu();
				isCustomisingControl = false;
			}
		}


		//
		// PROCESS MOUSE EVENTS
		//

		/// <summary>Processes a scroll wheel event</summary>
		/// <param name="Scroll">The delta</param>
		internal void ProcessMouseScroll(int Scroll)
		{
			if (Menus.Length == 0)
			{
				return;
			}
			// Load the current menu
			MenuBase menu = Menus[CurrMenu];
			if (menu.Type == MenuType.RouteList || menu.Type == MenuType.TrainList || menu.Type == MenuType.PackageInstall || menu.Type == MenuType.Packages || (int)menu.Type >= 107)
			{
				if (routeDescriptionBox.CurrentlySelected)
				{
					if (Math.Abs(Scroll) == Scroll)
					{
						routeDescriptionBox.VerticalScroll(-1);
					}
					else
					{
						routeDescriptionBox.VerticalScroll(1);
					}
					return;
				}
			}
			if (Math.Abs(Scroll) == Scroll)
			{
				//Negative
				if (menu.TopItem > 0)
				{
					menu.TopItem--;
				}
			}
			else
			{
				//Positive
				if (menu.Items.Length - menu.TopItem > visibleItems)
				{
					menu.TopItem++;
				}
			}
		}


		internal void DragFile(object sender, OpenTK.Input.FileDropEventArgs e)
		{
			if (Menus[CurrMenu].Type == MenuType.PackageInstall)
			{
				currentFile = e.FileName;
				if (!packageWorkerThread.IsBusy)
				{
					packageWorkerThread.RunWorkerAsync();
				}
			}
		}

		/// <summary>Processes a mouse move event</summary>
		/// <param name="x">The screen-relative x coordinate of the move event</param>
		/// <param name="y">The screen-relative y coordinate of the move event</param>
		internal bool ProcessMouseMove(int x, int y)
		{
			Program.currentGameWindow.CursorVisible = true;
			if (CurrMenu < 0)
			{
				return false;
			}
			// if not in menu or during control customisation or down outside menu area, do nothing
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu ||
				isCustomisingControl)
				return false;

			// Load the current menu
			MenuBase menu = Menus[CurrMenu];
			if (menu.TopItem > 1 && y < topItemY && y > menuMin.Y)
			{
				//Item is the scroll up ellipsis
				menu.Selection = menu.TopItem - 1;
				return true;
			}
			if (menu.Type == MenuType.RouteList || menu.Type == MenuType.TrainList || menu.Type == MenuType.PackageInstall  || menu.Type == MenuType.Packages || (int)menu.Type >= 107)
			{
				routeDescriptionBox.MouseMove(x, y);
				//HACK: Use this to trigger our menu start button!
				if (x > Program.Renderer.Screen.Width - 200 && x < Program.Renderer.Screen.Width - 10 && y > Program.Renderer.Screen.Height - 40 && y < Program.Renderer.Screen.Height - 10)
				{
					menu.Selection = int.MaxValue;
					return true;
				}
			}
			if (x < menuMin.X || x > menuMax.X || y < menuMin.Y || y > menuMax.Y)
			{
				return false;
			}

			int item = (int) ((y - topItemY) / lineHeight + menu.TopItem);
			// if the mouse is above a command item, select it
			if (item >= 0 && item < menu.Items.Length && (menu.Items[item] is MenuCommand || menu.Items[item] is MenuOption))
			{
				if (item < visibleItems + menu.TopItem + 1)
				{
					//Item is a standard menu entry or the scroll down elipsis
					menu.Selection = item;
					return true;
				}
			}
			return false;
		}

		//
		// PROCESS MOUSE DOWN EVENTS
		//
		/// <summary>Processes a mouse down event</summary>
		/// <param name="x">The screen-relative x coordinate of the down event</param>
		/// <param name="y">The screen-relative y coordinate of the down event</param>
		internal void ProcessMouseDown(int x, int y)
		{
			if (ProcessMouseMove(x, y))
			{
				if (Menus[CurrMenu].Selection == Menus[CurrMenu].TopItem + visibleItems)
				{
					ProcessCommand(Translations.Command.MenuDown, 0);
					return;
				}
				if (Menus[CurrMenu].Selection == Menus[CurrMenu].TopItem - 1)
				{
					ProcessCommand(Translations.Command.MenuUp, 0);
					return;
				}
				ProcessCommand(Translations.Command.MenuEnter, 0);
			}
		}

		//
		// PROCESS MENU COMMAND
		//
		/// <summary>Processes a user command for the current menu</summary>
		/// <param name="cmd">The command to apply to the current menu</param>
		/// <param name="timeElapsed">The time elapsed since previous frame</param>
		internal void ProcessCommand(Translations.Command cmd, double timeElapsed)
		{

			if (CurrMenu < 0)
			{
				return;
			}
			MenuBase menu = Menus[CurrMenu];
			// MenuBack is managed independently from single menu data
			if (cmd == Translations.Command.MenuBack)
			{
				if (menu.Type == MenuType.GameStart)
				{
					Instance.PushMenu(MenuType.Quit);
				}
				else
				{
					PopMenu();	
				}
				return;
			}
			
			if (menu.Selection == SelectionNone)    // if menu has no selection, do nothing
				return;
			if (menu.Selection == int.MaxValue)
			{
				switch (menu.Type)
				{
					case MenuType.RouteList:
						// Route not fully processed
						if (RoutefileState != RouteState.Processed)
							return;
						Instance.PushMenu(MenuType.TrainDefault);
						return;
					case MenuType.TrainDefault:
					case MenuType.TrainList:
						// No train selected
						if (Interface.CurrentOptions.TrainFolder == string.Empty)
							return;
						Reset();
						//Launch the game!
						Loading.Complete = false;
						Loading.LoadAsynchronously(currentFile, Encoding.UTF8, Interface.CurrentOptions.TrainFolder, Encoding.UTF8);
						OpenBVEGame g = Program.currentGameWindow as OpenBVEGame;
						// ReSharper disable once PossibleNullReferenceException
						g.LoadingScreenLoop();
						Program.Renderer.CurrentInterface = InterfaceType.Normal;
						return;
					case MenuType.PackageInstall:
						if (currentPackage != null)
						{
							switch (currentPackage.PackageType)
							{
								case PackageType.Route:
									installedFiles = string.Empty;
									Manipulation.ExtractPackage(currentPackage, Program.FileSystem.RouteInstallationDirectory, Program.FileSystem.PackageDatabaseFolder, ref installedFiles);
									break;
								case PackageType.Train:
									installedFiles = string.Empty;
									Manipulation.ExtractPackage(currentPackage, Program.FileSystem.TrainInstallationDirectory, Program.FileSystem.PackageDatabaseFolder, ref installedFiles);
									break;
								case PackageType.Other:
									installedFiles = string.Empty;
									Manipulation.ExtractPackage(currentPackage, Program.FileSystem.OtherInstallationDirectory, Program.FileSystem.PackageDatabaseFolder, ref installedFiles);
									break;
							}
						}
						break;
					case MenuType.UninstallRoute:
					case MenuType.UninstallTrain:
					case MenuType.UninstallOther:
						string s = string.Empty;
						if (Manipulation.UninstallPackage(currentPackage, Program.FileSystem.PackageDatabaseFolder, ref s))
						{
							switch (currentPackage.PackageType)
							{
								case PackageType.Route:
									DatabaseFunctions.CleanDirectory(Program.FileSystem.RouteInstallationDirectory, ref s);
									Database.currentDatabase.InstalledRoutes.Remove(currentPackage);
									break;
								case PackageType.Train:
									DatabaseFunctions.CleanDirectory(Program.FileSystem.TrainInstallationDirectory, ref s);
									Database.currentDatabase.InstalledTrains.Remove(currentPackage);
									break;
								case PackageType.Other:
									DatabaseFunctions.CleanDirectory(Program.FileSystem.OtherInstallationDirectory, ref s);
									Database.currentDatabase.InstalledOther.Remove(currentPackage);
									break;
							}
							routeDescriptionBox.Text = s;
							Database.SaveDatabase();
						}
						
						PopMenu();
						break;
				}
				return;

			}
			switch (cmd)
			{
				case Translations.Command.MenuUp:      // UP
					if (menu.Selection > 0 &&
						!(menu.Items[menu.Selection - 1] is MenuCaption))
					{
						menu.Selection--;
						ComputePosition();
					}
					break;
				case Translations.Command.MenuDown:    // DOWN
					if (menu.Selection < menu.Items.Length - 1)
					{
						menu.Selection++;
						ComputePosition();
					}
					break;
				//			case Translations.Command.MenuBack:	// ESC:	managed above
				//				break;
				case Translations.Command.MenuEnter:   // ENTER
					if (menu.Items[menu.Selection] is MenuCommand menuItem)
					{
						switch (menuItem.Tag)
						{
							// menu management commands
							case MenuTag.MenuBack:              // BACK TO PREVIOUS MENU
								GameMenu.instance.PopMenu();
								break;
							case MenuTag.MenuJumpToStation:     // TO STATIONS MENU
								GameMenu.instance.PushMenu(MenuType.JumpToStation);
								break;
							case MenuTag.MenuExitToMainMenu:    // TO EXIT MENU
								GameMenu.instance.PushMenu(MenuType.ExitToMainMenu);
								break;
							case MenuTag.MenuQuit:              // TO QUIT MENU
								GameMenu.instance.PushMenu(MenuType.Quit);
								break;
							case MenuTag.MenuControls:          // TO CONTROLS MENU
								GameMenu.instance.PushMenu(MenuType.Controls);
								break;
							case MenuTag.BackToSim:             // OUT OF MENU BACK TO SIMULATION
								Reset();
								Program.Renderer.CurrentInterface = InterfaceType.Normal;
								break;
							case MenuTag.Packages:
								if (Database.LoadDatabase(Program.FileSystem.PackageDatabaseFolder, currentDatabaseFile, out _))
								{
									GameMenu.instance.PushMenu(MenuType.Packages);
								}
								
								break;
							// route menu commands
							case MenuTag.PackageInstall:
								currentOperation = PackageOperation.Installing;
								packagePreview = true;
								instance.PushMenu(MenuType.PackageInstall);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\package.png"), new TextureParameters(null, null), out routePictureBox.Texture);
								break;
							case MenuTag.PackageUninstall:
								currentOperation = PackageOperation.Uninstalling;
								instance.PushMenu(MenuType.PackageUninstall);
								break;
							case MenuTag.UninstallRoute:
								if (Database.currentDatabase.InstalledRoutes.Count == 0)
								{
									return;
								}
								instance.PushMenu(MenuType.UninstallRoute);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out routePictureBox.Texture);
								break;
							case MenuTag.UninstallTrain:
								if (Database.currentDatabase.InstalledTrains.Count == 0)
								{
									return;
								}
								instance.PushMenu(MenuType.UninstallTrain);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out routePictureBox.Texture);
								break;
							case MenuTag.UninstallOther:
								if (Database.currentDatabase.InstalledOther.Count == 0)
								{
									return;
								}
								instance.PushMenu(MenuType.UninstallOther);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out routePictureBox.Texture);
								break;
							case MenuTag.File:
								if (currentOperation == PackageOperation.Installing)
								{
									currentFile = Path.CombineFile(SearchDirectory, menu.Items[menu.Selection].Text);
								}
								else
								{
									return;
								}
								
								if (!packageWorkerThread.IsBusy)
								{
									packageWorkerThread.RunWorkerAsync();
								}
								break;
							case MenuTag.Package:
								if (currentOperation == PackageOperation.Uninstalling)
								{
									currentPackage = (Package)((MenuCommand)menu.Items[menu.Selection]).Data;
								}
								else
								{
									return;
								}
								routeDescriptionBox.Text = currentPackage.Description;
								if (currentPackage.PackageImage != null)
								{
									routePictureBox.Texture = new Texture(currentPackage.PackageImage as Bitmap);
								}
								else
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\package.png"), new TextureParameters(null, null), out routePictureBox.Texture);		
								}
								break;
							case MenuTag.Options:
								GameMenu.instance.PushMenu(MenuType.Options);
								break;
							case MenuTag.RouteList:				// TO ROUTE LIST MENU
								GameMenu.instance.PushMenu(MenuType.RouteList);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","route_please_select"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out routePictureBox.Texture);	
								break;
							case MenuTag.Directory:		// SHOWS THE LIST OF FILES IN THE SELECTED DIR
								SearchDirectory = SearchDirectory == string.Empty ? menu.Items[menu.Selection].Text : Path.CombineDirectory(SearchDirectory, menu.Items[menu.Selection].Text);
								GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.ParentDirectory:		// SHOWS THE LIST OF FILES IN THE PARENT DIR
								if (string.IsNullOrEmpty(SearchDirectory))
								{
									return;
								}

								string oldSearchDirectory = SearchDirectory;
								try
								{
									DirectoryInfo newDirectory = Directory.GetParent(SearchDirectory);
									SearchDirectory = newDirectory == null ? string.Empty : Directory.GetParent(SearchDirectory)?.ToString();
								}
								catch
								{
									SearchDirectory = oldSearchDirectory;
									return;
								}
								GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.RouteFile:
								RoutefileState = RouteState.Loading;
								currentFile = Path.CombineFile(SearchDirectory, menu.Items[menu.Selection].Text);
								if (!routeWorkerThread.IsBusy)
								{
									routeWorkerThread.RunWorkerAsync();
								}
								break;
							case MenuTag.TrainDirectory:
								for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
								{
									string trainDir = Path.CombineDirectory(SearchDirectory, menu.Items[menu.Selection].Text);
									if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(trainDir))
									{
										if (Interface.CurrentOptions.TrainFolder == trainDir)
										{
											//enter folder
											SearchDirectory = SearchDirectory == string.Empty ? menu.Items[menu.Selection].Text : Path.CombineDirectory(SearchDirectory, menu.Items[menu.Selection].Text);
											GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
										}
										else
										{
											//Show details
											Interface.CurrentOptions.TrainFolder = trainDir;
											routeDescriptionBox.Text = Program.CurrentHost.Plugins[i].Train.GetDescription(trainDir);
											string trainImage = Program.CurrentHost.Plugins[i].Train.GetImage(trainDir);
											if (!string.IsNullOrEmpty(trainImage))
											{
												Program.CurrentHost.RegisterTexture(trainImage, new TextureParameters(null, null), out routePictureBox.Texture);
											}
											else
											{
												Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\train_unknown.png"), new TextureParameters(null, null), out routePictureBox.Texture);
											}
										}
									}
								}
								break;
								// simulation commands
							case MenuTag.JumpToStation:         // JUMP TO STATION
								Reset();
								TrainManagerBase.PlayerTrain.Jump((int)menuItem.Data, 0);
								Program.TrainManager.JumpTFO();
								break;
							case MenuTag.ExitToMainMenu:        // BACK TO MAIN MENU
								Reset();
								Program.RestartArguments =
									Interface.CurrentOptions.GameMode == GameMode.Arcade ? "/review" : "";
								MainLoop.Quit = MainLoop.QuitMode.ExitToMenu;
								break;
							case MenuTag.Control:               // CONTROL CUSTOMIZATION
								PushMenu(MenuType.Control, (int)((MenuCommand)menu.Items[menu.Selection]).Data);
								isCustomisingControl = true;
								CustomControlIdx = (int)((MenuCommand)menu.Items[menu.Selection]).Data;
								break;
							case MenuTag.ControlReset:
								PushMenu(MenuType.ControlReset, (int)((MenuCommand)menu.Items[menu.Selection]).Data);
								break;
							case MenuTag.Quit:                  // QUIT PROGRAMME
								Reset();
								MainLoop.Quit = MainLoop.QuitMode.QuitProgram;
								break;
							case MenuTag.Yes:
								switch (menu.Type)
								{
									case MenuType.TrainDefault:
										Reset();
										//Launch the game!
										Loading.Complete = false;
										Loading.LoadAsynchronously(currentFile, Encoding.UTF8, Interface.CurrentOptions.TrainFolder, Encoding.UTF8);
										OpenBVEGame g = Program.currentGameWindow as OpenBVEGame;
										// ReSharper disable once PossibleNullReferenceException
										g.LoadingScreenLoop();
										break;
									case MenuType.ControlReset:
										Interface.CurrentControls = null;
										var File = Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default.controls");
										Interface.LoadControls(File, out Interface.CurrentControls);
										Instance.PopMenu();
										break;
								}
								break;
							case MenuTag.No:
								switch (menu.Type)
								{
									case MenuType.TrainDefault:
										SearchDirectory = Program.FileSystem.InitialTrainFolder;
										Instance.PushMenu(MenuType.TrainList);
										routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_choose"});
										Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out routePictureBox.Texture);
										break;
									case MenuType.ControlReset:
										Instance.PopMenu();
										break;
								}
								break;
							case MenuTag.ToggleSwitch:
								Guid switchToToggle = (Guid)menuItem.Data;
								if (switchToToggle == null || !Program.CurrentRoute.Switches.ContainsKey(switchToToggle))
								{
									break;
								}
								int oldTrack = Program.CurrentRoute.Switches[switchToToggle].CurrentlySetTrack;
								Program.CurrentRoute.Switches[switchToToggle].Toggle();
								Program.CurrentHost.AddMessage(MessageType.Information, false, "Switch " + switchToToggle + " changed from Track " + oldTrack + " to " + Program.CurrentRoute.Switches[switchToToggle].CurrentlySetTrack);
								if (Program.CurrentRoute.Switches[switchToToggle].CurrentlySetTrack == Program.CurrentRoute.Switches[switchToToggle].LeftTrack)
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "In-Game\\Switch-L.png"), new TextureParameters(null, null), out switchSettingPictureBox.Texture);
								}
								else
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "In-Game\\Switch-R.png"), new TextureParameters(null, null), out switchSettingPictureBox.Texture);
								}

								menu.Items[2].Text = "Current Setting: " + Program.CurrentRoute.Switches[switchToToggle].CurrentlySetTrack;
								switchesFound = false; // as switch has been toggled, need to recalculate switches along route
								GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.PreviousSwitch:
								FoundSwitch fs = previousSwitches[0];
								previousSwitches.RemoveAt(0);
								nextSwitches.Insert(0, fs);
								GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.NextSwitch:
								FoundSwitch ns = nextSwitches[0];
								nextSwitches.RemoveAt(0);
								previousSwitches.Insert(0, ns);
								GameMenu.instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
						}
					}
					else if (menu.Items[menu.Selection] is MenuOption opt)
					{
						opt.Flip();
					}
					break;
				case Translations.Command.MiscFullscreen:
					// fullscreen
					Screen.ToggleFullscreen();
					break;
				case Translations.Command.MiscMute:
					// mute
					Program.Sounds.GlobalMute = !Program.Sounds.GlobalMute;
					Program.Sounds.Update(timeElapsed, Interface.CurrentOptions.SoundModel);
					break;
			}
		}

		private double pluginKeepAliveTimer;

		private ControlMethod lastControlMethod;

		//
		// DRAW MENU
		//
		/// <summary>Draws the current menu as a screen overlay</summary>
		internal void Draw(double RealTimeElapsed)
		{
			pluginKeepAliveTimer += RealTimeElapsed;
			if (pluginKeepAliveTimer > 100000 && TrainManager.PlayerTrain != null && TrainManager.PlayerTrain.Plugin != null)
			{
				TrainManager.PlayerTrain.Plugin.KeepAlive();
				pluginKeepAliveTimer = 0;
			}
			double TimeElapsed = RealTimeElapsed - lastTimeElapsed;
			lastTimeElapsed = RealTimeElapsed;
			int i;

			if (CurrMenu < 0 || CurrMenu >= Menus.Length)
				return;

			MenuBase menu = Menus[CurrMenu];
			// overlay background
			Program.Renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height), overlayColor);

			
			double itemLeft, itemX;
			if (menu.Align == TextAlignment.TopLeft)
			{
				itemLeft = 0;
				itemX = 16;
				Program.Renderer.Rectangle.Draw(null, new Vector2(0, menuMin.Y - Border.Y), new Vector2(menuMax.X - menuMin.X + 2.0f * Border.X, menuMax.Y - menuMin.Y + 2.0f * Border.Y), backgroundColor);
			}
			else
			{
				itemLeft = (Program.Renderer.Screen.Width - menu.ItemWidth) / 2; // item left edge
				// if menu alignment is left, left-align items, otherwise centre them in the screen
				itemX = (menu.Align & TextAlignment.Left) != 0 ? itemLeft : Program.Renderer.Screen.Width / 2.0;
				Program.Renderer.Rectangle.Draw(null, new Vector2(menuMin.X - Border.X, menuMin.Y - Border.Y), new Vector2(menuMax.X - menuMin.X + 2.0f * Border.X, menuMax.Y - menuMin.Y + 2.0f * Border.Y), backgroundColor);	
			}
			
			// draw the menu background
			
			
			int menuBottomItem = menu.TopItem + visibleItems - 1;

			

			// if not starting from the top of the menu, draw a dimmed ellipsis item
			if (menu.Selection == menu.TopItem - 1 && !isCustomisingControl)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, menuMin.Y /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + ItemBorder.X, em + ItemBorder.Y * 2), highlightColor);
			}
			if (menu.TopItem > 0)
				Program.Renderer.OpenGlString.Draw(MenuFont, @"...", new Vector2(itemX, menuMin.Y),
					menu.Align, ColourDimmed, false);
			// draw the items
			double itemY = topItemY;
			for (i = menu.TopItem; i <= menuBottomItem && i < menu.Items.Length; i++)
			{
				if (menu.Items[i] == null)
				{
					continue;
				}

				double itemHeight = MenuFont.MeasureString(menu.Items[i].Text).Y;
				double iconX = itemX;
				if (menu.Items[i].Icon != null)
				{
					itemX += itemHeight * 1.25;
				}
				if (i == menu.Selection)
				{
					// draw a solid highlight rectangle under the text
					// HACK! the highlight rectangle has to be shifted a little down to match
					// the text body. OpenGL 'feature'?
					Color128 color = highlightColor;
					if(menu.Items[i] is MenuCommand command)
					{
						switch (command.Tag)
						{
							case MenuTag.Directory:
							case MenuTag.ParentDirectory:
								color = folderHighlightColor;
								break;
							case MenuTag.RouteFile:
								color = routeHighlightColor;
								break;
							default:
								color = highlightColor;
								break;
						}
					}

					if (itemLeft == 0)
					{
						Program.Renderer.Rectangle.Draw(null, new Vector2(ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.Width + 2.0f * ItemBorder.X, em + ItemBorder.Y * 2), color);
					}
					else
					{
						Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + 2.0f * ItemBorder.X, em + ItemBorder.Y * 2), color);
					}
					
					// draw the text
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].DisplayText(TimeElapsed), new Vector2(itemX, itemY),
						menu.Align, ColourHighlight, false);
				}
				else if (menu.Items[i] is MenuCaption)
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].DisplayText(TimeElapsed), new Vector2(itemX, itemY),
						menu.Align, ColourCaption, false);
				else
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].DisplayText(TimeElapsed), new Vector2(itemX, itemY),
						menu.Align, ColourNormal, false);
				if (menu.Items[i] is MenuOption)
				{
					Program.Renderer.OpenGlString.Draw(MenuFont, (menu.Items[i] as MenuOption).CurrentOption.ToString(), new Vector2((menuMax.X - menuMin.X + 2.0f * Border.X) + 4.0f, itemY),
						menu.Align, backgroundColor, false);
				}
				itemY += lineHeight;
				if (menu.Items[i].Icon != null)
				{
					Program.Renderer.Rectangle.DrawAlpha(menu.Items[i].Icon, new Vector2(iconX, itemY - itemHeight * 1.5), new Vector2(itemHeight, itemHeight), Color128.White);
					itemX = iconX;
				}
				
			}


			if (menu.Selection == menu.TopItem + visibleItems)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + 2.0f * ItemBorder.X, em + ItemBorder.Y * 2), highlightColor);
			}
			// if not at the end of the menu, draw a dimmed ellipsis item at the bottom
			if (i < menu.Items.Length - 1)
				Program.Renderer.OpenGlString.Draw(MenuFont, @"...", new Vector2(itemX, itemY),
					menu.Align, ColourDimmed, false);
			switch (menu.Type)
			{
				case MenuType.GameStart:
				case MenuType.Packages:
					LogoPictureBox.Draw();
					string currentVersion =  @"v" + System.Windows.Forms.Application.ProductVersion + Program.VersionSuffix;
					if (IntPtr.Size != 4)
					{
						currentVersion += @" 64-bit";
					}

					OpenGlFont versionFont = Program.Renderer.Fonts.NextSmallestFont(MenuFont);
					Program.Renderer.OpenGlString.Draw(versionFont, currentVersion, new Vector2(Program.Renderer.Screen.Width - Program.Renderer.Screen.Width / 4.0, Program.Renderer.Screen.Height - versionFont.FontSize * 2), TextAlignment.TopLeft, Color128.Black);
					break;
				case MenuType.RouteList:
				case MenuType.TrainList:
				{
					routePictureBox.Draw();
					routeDescriptionBox.Draw();

					//Choose Train and Game start button
					bool allowNextPhase = (menu.Type == MenuType.RouteList && RoutefileState == RouteState.Processed) || (menu.Type == MenuType.TrainList && Interface.CurrentOptions.TrainFolder != string.Empty);
					
					if (menu.Selection == int.MaxValue && allowNextPhase) //HACK: Special value to make this work with minimum extra code
					{
						Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
						Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 197, Program.Renderer.Screen.Height - 37), new Vector2(184, 24), highlightColor);
						Program.Renderer.OpenGlString.Draw(MenuFont, menu.Type == MenuType.RouteList ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_choose"}) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","start_start"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, Color128.Black);
					}
					else
					{
						Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
						Program.Renderer.OpenGlString.Draw(MenuFont, menu.Type == MenuType.RouteList ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_choose"}) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","start_start"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, allowNextPhase ? Color128.White : Color128.Grey);
					}
					break;
				}
				case MenuType.PackageInstall:
					routePictureBox.Draw();
					routeDescriptionBox.Draw();
					if (currentPackage != null)
					{
						if (menu.Selection == int.MaxValue) //HACK: Special value to make this work with minimum extra code
						{
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 197, Program.Renderer.Screen.Height - 37), new Vector2(184, 24), highlightColor);
							Program.Renderer.OpenGlString.Draw(MenuFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_button"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, Color128.Black);
						}
						else
						{
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
							Program.Renderer.OpenGlString.Draw(MenuFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_button"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, Color128.White); 
						}
					}
					break;
				case MenuType.PackageUninstall:
					if (routeDescriptionBox.Text != string.Empty)
					{
						routeDescriptionBox.Draw();
					}
					else
					{
						LogoPictureBox.Draw();
					}
					break;
				case MenuType.UninstallRoute:
				case MenuType.UninstallTrain:
				case MenuType.UninstallOther:
					routePictureBox.Draw();
					routeDescriptionBox.Draw();
					if (currentPackage != null)
					{
						if (menu.Selection == int.MaxValue) //HACK: Special value to make this work with minimum extra code
						{
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 197, Program.Renderer.Screen.Height - 37), new Vector2(184, 24), highlightColor);
							Program.Renderer.OpenGlString.Draw(MenuFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","uninstall_button"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, Color128.Black);
						}
						else
						{
							Program.Renderer.Rectangle.Draw(null, new Vector2(Program.Renderer.Screen.Width - 200, Program.Renderer.Screen.Height - 40), new Vector2(190, 30), Color128.Black);
							Program.Renderer.OpenGlString.Draw(MenuFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","uninstall_button"}), new Vector2(Program.Renderer.Screen.Width - 180, Program.Renderer.Screen.Height - 35), TextAlignment.TopLeft, Color128.White); 
						}
					}
					break;
				case MenuType.Controls:
					int data = (int)((MenuCommand)menu.Items[menu.Selection]).Data;
					if (menu.Selection != menu.LastSelection)
					{
						switch (Interface.CurrentControls[data].Method)
						{
							case ControlMethod.Keyboard:
								if (lastControlMethod != ControlMethod.Keyboard)
								{
									controlPictureBox.Texture = Program.Renderer.KeyboardTexture;
								}
								lastControlMethod = ControlMethod.Keyboard;
								break;
							
							case ControlMethod.Joystick:
								if (lastControlMethod != ControlMethod.Joystick)
								{
									Guid guid = Interface.CurrentControls[data].Device;
									if (Program.Joysticks.AttachedJoysticks.ContainsKey(guid))
									{
										if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("gamepad", StringComparison.InvariantCultureIgnoreCase) != -1)
										{
											controlPictureBox.Texture = Program.Renderer.GamepadTexture;
										}
										else if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("xinput", StringComparison.InvariantCultureIgnoreCase) != -1)
										{
											controlPictureBox.Texture = Program.Renderer.XInputTexture;
										}
										else if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("mascon", StringComparison.InvariantCultureIgnoreCase) != -1)
										{
											controlPictureBox.Texture = Program.Renderer.MasconTeture;
										}
										else
										{
											controlPictureBox.Texture = Program.Renderer.JoystickTexture;
										}
									}
								}
								lastControlMethod = ControlMethod.Joystick;
								break;
							case ControlMethod.RailDriver:
								if (lastControlMethod != ControlMethod.RailDriver)
								{
									controlPictureBox.Texture = Program.Renderer.RailDriverTexture;
								}
								lastControlMethod = ControlMethod.RailDriver;
								break;
							default:
								// not really supported in the GL menu yet
								controlPictureBox.Texture = null;
								lastControlMethod = Interface.CurrentControls[data].Method;
								break;
						}
					}

					controlTextBox.Text = Translations.CommandInfos[Interface.CurrentControls[data].Command].Description + Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","assignment_current"}) + Environment.NewLine + Environment.NewLine + GetControlDescription(data);
					controlTextBox.Draw();
					controlPictureBox.Draw();
					break;
				case MenuType.ChangeSwitch:
					/*
					 * Set final image locs, which we don't know till the menu extent has been measured in the render sequence
					 */
					switchMainPictureBox.Location = new Vector2(menu.Width + (ItemBorder.X * 4), switchMainPictureBox.Location.Y);
					switchSettingPictureBox.Location = new Vector2(menu.Width + (ItemBorder.X * 4) + 80, switchSettingPictureBox.Location.Y);
					switchMainPictureBox.Draw();
					switchSettingPictureBox.Draw();
					switchMapPictureBox.Draw();
					break;
			}
			
		}
	}

}
