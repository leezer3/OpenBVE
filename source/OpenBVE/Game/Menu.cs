using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using System;
using System.Drawing;
using DavyKager;
using LibRender2.Screens;
using LibRender2.Texts;
using OpenBve.Input;
using OpenBveApi;
using OpenBveApi.Input;
using OpenTK;
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
	public sealed class Menu
	{
		/// <summary>The list of possible tags for a menu entry- These define the functionality of a given menu entry</summary>
		public enum MenuTag
		{
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
			/// <summary>Customises the selected control</summary>
			Control
		};

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
			Quit
		};

		// components of the semi-transparent screen overlay
		private readonly Color128 overlayColor = new Color128(0.0f, 0.0f, 0.0f, 0.2f);
		private readonly Color128 backgroundColor = new Color128(0.0f, 0.0f, 0.0f, 1.0f);
		private readonly Color128 highlightColor = new Color128(1.0f, 0.69f, 0.0f, 1.0f);
		// text colours
		private static readonly Color128 ColourCaption = new Color128(0.750f, 0.750f, 0.875f, 1.0f);
		private static readonly Color128 ColourDimmed = new Color128(1.000f, 1.000f, 1.000f, 0.5f);
		private static readonly Color128 ColourHighlight = Color128.Black;
		private static readonly Color128 ColourNormal = Color128.White;

		// some sizes and constants
		// TODO: make borders Menu fields dependent on font size
		private const int MenuBorderX = 16;
		private const int MenuBorderY = 16;
		private const int MenuItemBorderX = 8;
		private const int MenuItemBorderY = 2;
		private const float LineSpacing = 1.75f;    // the ratio between the font size and line distance
		private const int SelectionNone = -1;

		/********************
			BASE MENU ENTRY CLASS
		*********************/
		private abstract class MenuEntry
		{
			internal string Text;
		}

		/********************
			DERIVED MENU ITEM CLASSES
		*********************/
		private class MenuCaption : MenuEntry
		{
			internal MenuCaption(string Text)
			{
				this.Text = Text;
			}
		}
		private class MenuCommand : MenuEntry
		{
			internal readonly MenuTag Tag;
			internal readonly int Data;
			internal MenuCommand(string Text, MenuTag Tag, int Data)
			{
				this.Text = Text;
				this.Tag = Tag;
				this.Data = Data;
			}
		}

		/********************
			SINGLE-MENU ENTRY CLASS
		*********************
		Describes a single menu of the menu stack.
		The class is private to Menu, but all its fields are public to allow 'quick-and-dirty'
		access from Menu itself. */
		private class SingleMenu
		{
			/********************
				MENU FIELDS
			*********************/
			public readonly TextAlignment Align;
			public readonly MenuEntry[] Items = { };
			public readonly int ItemWidth = 0;
			public readonly int Width = 0;
			public readonly int Height = 0;

			private int lastSelection = int.MaxValue;
			private int currentSelection;
			
			public int Selection
			{
				get
				{
					return currentSelection;
				}
				set
				{
					lastSelection = currentSelection;
					currentSelection = value;
					if (currentSelection != lastSelection && Interface.CurrentOptions.ScreenReaderAvailable)
					{
						if (!Tolk.Output(Items[currentSelection].Text))
						{
							// failed to output to screen reader, so don't keep trying
							Interface.CurrentOptions.ScreenReaderAvailable = false;
						}
					}
				}
			}
			public int TopItem;         // the top displayed menu item
			

			/********************
				MENU C'TOR
			*********************/
			public SingleMenu(MenuType menuType, int data = 0)
			{
				int i, menuItem;
				int jump = 0;
				Size size;

				Align = TextAlignment.TopMiddle;
				Height = Width = 0;
				Selection = 0;                      // defaults to first menu item
				switch (menuType)
				{
					case MenuType.Top:          // top level menu
						if (Interface.CurrentOptions.ScreenReaderAvailable)
						{
							if (!Tolk.Output(Translations.GetInterfaceString("menu_title")))
							{
								// failed to output to screen reader, so don't keep trying
								Interface.CurrentOptions.ScreenReaderAvailable = false;
							}
						}
						for (i = 0; i < Program.CurrentRoute.Stations.Length; i++)
							if (Program.CurrentRoute.Stations[i].PlayerStops() & Program.CurrentRoute.Stations[i].Stops.Length > 0)
							{
								jump = 1;
								break;
							}
						Items = new MenuEntry[4 + jump];
						Items[0] = new MenuCommand(Translations.GetInterfaceString("menu_resume"), MenuTag.BackToSim, 0);
						if (jump > 0)
							Items[1] = new MenuCommand(Translations.GetInterfaceString("menu_jump"), MenuTag.MenuJumpToStation, 0);
						if (!Interface.CurrentOptions.KioskMode)
						{
							//Don't allow quitting or customisation of the controls in kiosk mode
							Items[1 + jump] = new MenuCommand(Translations.GetInterfaceString("menu_exit"), MenuTag.MenuExitToMainMenu, 0);
							Items[2 + jump] = new MenuCommand(Translations.GetInterfaceString("menu_customize_controls"), MenuTag.MenuControls, 0);
							Items[3 + jump] = new MenuCommand(Translations.GetInterfaceString("menu_quit"), MenuTag.MenuQuit, 0);
						}
						else
						{
							Array.Resize(ref Items, Items.Length -3);
						}
						break;

					case MenuType.JumpToStation:    // list of stations to jump to
													// count the number of available stations
						menuItem = 0;
						for (i = 0; i < Program.CurrentRoute.Stations.Length; i++)
							if (Program.CurrentRoute.Stations[i].PlayerStops() & Program.CurrentRoute.Stations[i].Stops.Length > 0)
								menuItem++;
						// list available stations, selecting the next station as predefined choice
						jump = 0;                           // no jump found yet
						Items = new MenuEntry[menuItem + 1];
						Items[0] = new MenuCommand(Translations.GetInterfaceString("menu_back"), MenuTag.MenuBack, 0);
						menuItem = 1;
						for (i = 0; i < Program.CurrentRoute.Stations.Length; i++)
							if (Program.CurrentRoute.Stations[i].PlayerStops() & Program.CurrentRoute.Stations[i].Stops.Length > 0)
							{
								Items[menuItem] = new MenuCommand(Program.CurrentRoute.Stations[i].Name, MenuTag.JumpToStation, i);
								// if no preferred jump-to-station found yet and this station is
								// after the last station the user stopped at, select this item
								if (jump == 0 && i > TrainManager.PlayerTrain.LastStation)
								{
									jump = i;
									Selection = menuItem;
								}
								menuItem++;
							}
						Align = TextAlignment.TopLeft;
						break;

					case MenuType.ExitToMainMenu:
						Items = new MenuEntry[3];
						Items[0] = new MenuCaption(Translations.GetInterfaceString("menu_exit_question"));
						Items[1] = new MenuCommand(Translations.GetInterfaceString("menu_exit_no"), MenuTag.MenuBack, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0);
						Selection = 1;
						break;

					case MenuType.Quit:         // ask for quit confirmation
						Items = new MenuEntry[3];
						Items[0] = new MenuCaption(Translations.GetInterfaceString("menu_quit_question"));
						Items[1] = new MenuCommand(Translations.GetInterfaceString("menu_quit_no"), MenuTag.MenuBack, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0);
						Selection = 1;
						break;

					case MenuType.Controls:
						//Refresh the joystick list
						Program.Joysticks.RefreshJoysticks();
						Items = new MenuEntry[Interface.CurrentControls.Length + 1];
						Items[0] = new MenuCommand(Translations.GetInterfaceString("menu_back"), MenuTag.MenuBack, 0);
						for (i = 0; i < Interface.CurrentControls.Length; i++)
							Items[i + 1] = new MenuCommand(Interface.CurrentControls[i].Command.ToString(), MenuTag.Control, i);
						Align = TextAlignment.TopLeft;
						break;

					case MenuType.Control:
						//Refresh the joystick list
						Program.Joysticks.RefreshJoysticks();
						Selection = SelectionNone;
						Items = new MenuEntry[4];
						// get code name and description
						Control loadedControl = Interface.CurrentControls[data];
						for (int h = 0; h < Translations.CommandInfos.Length; h++)
						{
							if (Translations.CommandInfos[h].Command == loadedControl.Command)
							{
								Items[0] = new MenuCommand(loadedControl.Command.ToString() + " - " +
										Translations.CommandInfos[h].Description, MenuTag.None, 0);
								break;
							}
						}
						// get assignment
						String str = "";
						switch (loadedControl.Method)
						{
							case ControlMethod.Keyboard:
								string keyName = loadedControl.Key.ToString();
								for (int k = 0; k < Translations.TranslatedKeys.Length; k++)
								{
									if (Translations.TranslatedKeys[k].Key == loadedControl.Key)
									{
										keyName = Translations.TranslatedKeys[k].Description;
										break;
									}
								}
								if (loadedControl.Modifier != KeyboardModifier.None)
								{
									str = Translations.GetInterfaceString("menu_keyboard") + " [" + loadedControl.Modifier + "-" + keyName + "]";
								}
								else
								{
									str = Translations.GetInterfaceString("menu_keyboard") + " [" + keyName + "]";
								}
								break;
							case ControlMethod.Joystick:
								str = Translations.GetInterfaceString("menu_joystick") + " " + loadedControl.Device + " [" + loadedControl.Component + " " + loadedControl.Element + "]";
								switch (loadedControl.Component)
								{
									case JoystickComponent.FullAxis:
									case JoystickComponent.Axis:
										str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString("menu_joystickdirection_positive") : Translations.GetInterfaceString("menu_joystickdirection_negative"));
										break;
									//						case Interface.JoystickComponent.Button:	// NOTHING TO DO FOR THIS CASE!
									//							str = str;
									//							break;
									case JoystickComponent.Hat:
										str += " " + (OpenTK.Input.HatPosition)loadedControl.Direction;
										break;
									case JoystickComponent.Invalid:
										str = Translations.GetInterfaceString("menu_joystick_notavailable");
										break;
								}
								break;
							case ControlMethod.RailDriver:
								str = "RailDriver [" + loadedControl.Component + " " + loadedControl.Element + "]";
								switch (loadedControl.Component)
								{
									case JoystickComponent.FullAxis:
									case JoystickComponent.Axis:
										str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString("menu_joystickdirection_positive") : Translations.GetInterfaceString("menu_joystickdirection_negative"));
										break;
									case JoystickComponent.Invalid:
										str = Translations.GetInterfaceString("menu_joystick_notavailable");
										break;
								}
								break;
							case ControlMethod.Invalid:
								str = Translations.GetInterfaceString("menu_joystick_notavailable");
								break;
						}
						Items[1] = new MenuCommand(Translations.GetInterfaceString("menu_assignment_current") + " " + str, MenuTag.None, 0);
						Items[2] = new MenuCommand(" ", MenuTag.None, 0);
						Items[3] = new MenuCommand(Translations.GetInterfaceString("menu_assign"), MenuTag.None, 0);
						break;
				}

				// compute menu extent
				for (i = 0; i < Items.Length; i++)
				{
					if (Items[i] == null)
					{
						continue;
					}
					size = Game.Menu.MenuFont.MeasureString(Items[i].Text);
					if (size.Width > Width)
						Width = size.Width;
					if (!(Items[i] is MenuCaption) && size.Width > ItemWidth)
						ItemWidth = size.Width;
				}
				Height = Items.Length * Game.Menu.LineHeight;
				TopItem = 0;
			}

		}                   // end of private class SingleMenu

		/********************
			MENU SYSTEM FIELDS
		*********************/
		private int CurrMenu = -1;
		private int CustomControlIdx;   // the index of the control being customized
		private int em;                 // the size of menu font (in pixels)
		private bool isCustomisingControl = false;
		private bool isInitialized = false;
		// the total line height from the top of an item to the top of the item below (in pixels)
		private int lineHeight;
		private SingleMenu[] Menus = { };
		private OpenGlFont menuFont = null;
		// area occupied by the items of the current menu in screen coordinates
		private int menuXmin, menuXmax, menuYmin, menuYmax;
		private int topItemY;           // the top edge of top item
		private int visibleItems;       // the number of visible items
										// properties (to allow read-only access to some fields)
		internal int LineHeight
		{
			get
			{
				return lineHeight;
			}
		}
		internal OpenGlFont MenuFont
		{
			get
			{
				return menuFont;
			}
		}

		internal Key MenuBackKey;

		/********************
			MENU SYSTEM SINGLETON C'TOR
		*********************/
		private static readonly Menu instance = new Menu();
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Menu()
		{
		}
		private Menu()
		{
		}

		/// <summary>Returns the current menu instance (If applicable)</summary>
		public static Menu Instance
		{
			get
			{
				return instance;
			}
		}

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
			isInitialized = true;
		}

		//
		// RESET MENU SYSTEM TO INITIAL CONDITIONS
		//
		private void Reset()
		{
			CurrMenu = -1;
			Menus = new SingleMenu[] { };
			isCustomisingControl = false;
		}

		//
		// PUSH ANOTHER MENU
		//

		/// <summary>Pushes a menu into the menu stack</summary>
		/// <param name= "type">The type of menu to push</param>
		/// <param name= "data">The index of the menu in the menu stack (If pushing an existing higher level menu)</param>
		public void PushMenu(MenuType type, int data = 0)
		{
			if (Program.Renderer.CurrentInterface != InterfaceType.Menu)
			{
				// Deliberately set to the standard cursor, as touch controls may have set to something else
				Program.currentGameWindow.Cursor = MouseCursor.Default;
			}
			if (!isInitialized)
				Init();
			CurrMenu++;
			if (Menus.Length <= CurrMenu)
				Array.Resize(ref Menus, CurrMenu + 1);
			Menus[CurrMenu] = new Menu.SingleMenu(type, data);
			PositionMenu();
			Program.Renderer.CurrentInterface = InterfaceType.Menu;
		}

		//
		// POP LAST MENU
		//
		/// <summary>Pops the previous menu in the menu stack</summary>
		public void PopMenu()
		{
			if (CurrMenu > 0)           // if more than one menu remaining...
			{
				CurrMenu--;             // ...back to previous smenu
				PositionMenu();
			}
			else
			{                           // if only one menu remaining...
				Reset();
				Program.Renderer.CurrentInterface = InterfaceType.Normal;  // return to simulation
			}
		}

		//
		// IS CUSTOMIZING CONTROL?
		//
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
				if (Program.Joysticks.AttachedJoysticks[device] is AbstractRailDriver)
				{
					Interface.CurrentControls[CustomControlIdx].Method = ControlMethod.RailDriver;
				}
				else
				{
					Interface.CurrentControls[CustomControlIdx].Method = ControlMethod.Joystick;
				}
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
			// Load the current menu
			SingleMenu menu = Menus[CurrMenu];
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
			if (Program.Renderer.CurrentInterface != InterfaceType.Menu ||
				isCustomisingControl)
				return false;

			// Load the current menu
			SingleMenu menu = Menus[CurrMenu];
			if (menu.TopItem > 1 && y < topItemY && y > menuYmin)
			{
				//Item is the scroll up ellipsis
				menu.Selection = menu.TopItem - 1;
				return true;
			}
			if (x < menuXmin || x > menuXmax || y < menuYmin || y > menuYmax)
			{
				return false;
			}

			int item = (y - topItemY) / lineHeight + menu.TopItem;
			// if the mouse is above a command item, select it
			if (item >= 0 && item < menu.Items.Length && menu.Items[item] is MenuCommand)
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
			// MenuBack is managed independently from single menu data
			if (cmd == Translations.Command.MenuBack)
			{
				PopMenu();
				return;
			}

			SingleMenu menu = Menus[CurrMenu];
			if (menu.Selection == SelectionNone)    // if menu has no selection, do nothing
				return;
			switch (cmd)
			{
				case Translations.Command.MenuUp:      // UP
					if (menu.Selection > 0 &&
						!(menu.Items[menu.Selection - 1] is MenuCaption))
					{
						menu.Selection--;
						PositionMenu();
					}
					break;
				case Translations.Command.MenuDown:    // DOWN
					if (menu.Selection < menu.Items.Length - 1)
					{
						menu.Selection++;
						PositionMenu();
					}
					break;
				//			case Translations.Command.MenuBack:	// ESC:	managed above
				//				break;
				case Translations.Command.MenuEnter:   // ENTER
					if (menu.Items[menu.Selection] is MenuCommand)
					{
						MenuCommand menuItem = (MenuCommand)menu.Items[menu.Selection];
						switch (menuItem.Tag)
						{
							// menu management commands
							case MenuTag.MenuBack:              // BACK TO PREVIOUS MENU
								Menu.instance.PopMenu();
								break;
							case MenuTag.MenuJumpToStation:     // TO STATIONS MENU
								Menu.instance.PushMenu(MenuType.JumpToStation);
								break;
							case MenuTag.MenuExitToMainMenu:    // TO EXIT MENU
								Menu.instance.PushMenu(MenuType.ExitToMainMenu);
								break;
							case MenuTag.MenuQuit:              // TO QUIT MENU
								Menu.instance.PushMenu(MenuType.Quit);
								break;
							case MenuTag.MenuControls:          // TO CONTROLS MENU
								Menu.instance.PushMenu(MenuType.Controls);
								break;
							case MenuTag.BackToSim:             // OUT OF MENU BACK TO SIMULATION
								Reset();
								Program.Renderer.CurrentInterface = InterfaceType.Normal;
								break;

							// simulation commands
							case MenuTag.JumpToStation:         // JUMP TO STATION
								Reset();
								TrainManager.PlayerTrain.Jump(menuItem.Data);
								Program.TrainManager.JumpTFO();
								break;
							case MenuTag.ExitToMainMenu:        // BACK TO MAIN MENU
								Reset();
								Program.RestartArguments =
									Interface.CurrentOptions.GameMode == GameMode.Arcade ? "/review" : "";
								MainLoop.Quit = MainLoop.QuitMode.ExitToMenu;
								break;
							case MenuTag.Control:               // CONTROL CUSTOMIZATION
								PushMenu(MenuType.Control, ((MenuCommand)menu.Items[menu.Selection]).Data);
								isCustomisingControl = true;
								CustomControlIdx = ((MenuCommand)menu.Items[menu.Selection]).Data;
								break;
							case MenuTag.Quit:                  // QUIT PROGRAMME
								Reset();
								MainLoop.Quit = MainLoop.QuitMode.QuitProgram;
								break;
						}
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

		//
		// DRAW MENU
		//
		/// <summary>Draws the current menu as a screen overlay</summary>
		internal void Draw()
		{

			int i;

			if (CurrMenu < 0 || CurrMenu >= Menus.Length)
				return;

			SingleMenu menu = Menus[CurrMenu];
			// overlay background
			Program.Renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height), overlayColor);

			// HORIZONTAL PLACEMENT: centre the menu in the main window
			int itemLeft = (Program.Renderer.Screen.Width - menu.ItemWidth) / 2; // item left edge
																// if menu alignment is left, left-align items, otherwise centre them in the screen
			int itemX = (menu.Align & TextAlignment.Left) != 0 ? itemLeft : Program.Renderer.Screen.Width / 2;

			int menuBottomItem = menu.TopItem + visibleItems - 1;

			// draw the menu background
			Program.Renderer.Rectangle.Draw(null, new Vector2(menuXmin - MenuBorderX, menuYmin - MenuBorderY), new Vector2(menuXmax - menuXmin + 2.0f * MenuBorderX, menuYmax - menuYmin + 2.0f * MenuBorderY), backgroundColor);

			// if not starting from the top of the menu, draw a dimmed ellipsis item
			if (menu.Selection == menu.TopItem - 1 && !isCustomisingControl)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - MenuItemBorderX, menuYmin /*-MenuItemBorderY*/), new Vector2(menu.ItemWidth + MenuItemBorderX, em + MenuItemBorderY * 2), highlightColor);
			}
			if (menu.TopItem > 0)
				Program.Renderer.OpenGlString.Draw(MenuFont, "...", new Point(itemX, menuYmin),
					menu.Align, ColourDimmed, false);
			// draw the items
			int itemY = topItemY;
			for (i = menu.TopItem; i <= menuBottomItem && i < menu.Items.Length; i++)
			{
				if (menu.Items[i] == null)
				{
					continue;
				}
				if (i == menu.Selection)
				{
					// draw a solid highlight rectangle under the text
					// HACK! the highlight rectangle has to be shifted a little down to match
					// the text body. OpenGL 'feature'?
					Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - MenuItemBorderX, itemY /*-MenuItemBorderY*/), new Vector2(menu.ItemWidth + 2.0f * MenuItemBorderX, em + MenuItemBorderY * 2), highlightColor);
					// draw the text
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].Text, new Point(itemX, itemY),
						menu.Align, ColourHighlight, false);
				}
				else if (menu.Items[i] is MenuCaption)
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].Text, new Point(itemX, itemY),
						menu.Align, ColourCaption, false);
				else
					Program.Renderer.OpenGlString.Draw(MenuFont, menu.Items[i].Text, new Point(itemX, itemY),
						menu.Align, ColourNormal, false);
				itemY += lineHeight;
			}


			if (menu.Selection == menu.TopItem + visibleItems)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - MenuItemBorderX, itemY /*-MenuItemBorderY*/), new Vector2(menu.ItemWidth + 2.0f * MenuItemBorderX, em + MenuItemBorderY * 2), highlightColor);
			}
			// if not at the end of the menu, draw a dimmed ellipsis item at the bottom
			if (i < menu.Items.Length - 1)
				Program.Renderer.OpenGlString.Draw(MenuFont, "...", new Point(itemX, itemY),
					menu.Align, ColourDimmed, false);
		}

		//
		// POSITION MENU
		//
		/// <summary>Computes the position in the screen of the current menu.
		/// Also sets the menu size</summary>
		private void PositionMenu()
		{
			//			int i;

			if (CurrMenu < 0 || CurrMenu >= Menus.Length)
				return;

			SingleMenu menu = Menus[CurrMenu];
			// HORIZONTAL PLACEMENT: centre the menu in the main window
			menuXmin = (Program.Renderer.Screen.Width - menu.Width) / 2;     // menu left edge (border excluded)
			menuXmax = menuXmin + menu.Width;               // menu right edge (border excluded)
															// VERTICAL PLACEMENT: centre the menu in the main window
			menuYmin = (Program.Renderer.Screen.Height - menu.Height) / 2;       // menu top edge (border excluded)
			menuYmax = menuYmin + menu.Height;              // menu bottom edge (border excluded)
			topItemY = menuYmin;                                // top edge of top item
																// assume all items fit in the screen
			visibleItems = menu.Items.Length;

			// if there are more items than can fit in the screen height,
			// (there should be at least room for the menu top border)
			if (menuYmin < MenuBorderY)
			{
				// the number of lines which fit in the screen
				int numOfLines = (Program.Renderer.Screen.Height - MenuBorderY * 2) / lineHeight;
				visibleItems = numOfLines - 2;                  // at least an empty line at the top and at the bottom
																// split the menu in chunks of 'visibleItems' items
																// and display the chunk which contains the currently selected item
				menu.TopItem = menu.Selection - (menu.Selection % visibleItems);
				visibleItems = menu.Items.Length - menu.TopItem < visibleItems ?    // in the last chunk,
					menu.Items.Length - menu.TopItem : visibleItems;                // display remaining items only
				menuYmin = (Program.Renderer.Screen.Height - numOfLines * lineHeight) / 2;
				menuYmax = menuYmin + numOfLines * lineHeight;
				// first menu item is drawn on second line (first line is empty
				// on first screen and contains an ellipsis on following screens
				topItemY = menuYmin + lineHeight;
			}
		}

	}

}
