using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;				// for Key
using System;
using System.Drawing;

namespace OpenBve
{
	/********************
		MENU CLASS
	*********************
	Implements the in-game menu system; manages addition and removal of individual menus.
	Implemented as a singleton.
	Keeps a stack of menus, allowing navigating forward and back */

	public sealed class Menu
	{
		public enum MenuTag		{ None, Caption, MenuBack, MenuJumpToStation, MenuExitToMainMenu, MenuControls, MenuQuit,
			BackToSim, JumpToStation, ExitToMainMenu, Quit, Control };
		public enum MenuType	{ None, Top, JumpToStation, ExitToMainMenu, Controls, Control, Quit };

		// components of the semi-transparent screen overlay
		private const float					ovlR			= 0.00f;
		private const float					ovlG			= 0.00f;
		private const float					ovlB			= 0.00f;
		private const float					ovlA			= 0.20f;
		// components of the menu background colour
		private const float					bkgMenuR		= 0.00f;
		private const float					bkgMenuG		= 0.00f;
		private const float					bkgMenuB		= 0.00f;
		private const float					bkgMenuA		= 1.00f;
		// components of the highlighted item background
		private const float					bkgHgltR		= 1.00f;
		private const float					bkgHgltG		= 0.69f;
		private const float					bkgHgltB		= 0.00f;
		private const float					bkgHgltA		= 1.00f;
		// text colours
		private static readonly Color128	ColourCaption	= new Color128(0.750f, 0.750f, 0.875f, 1.0f);
		private static readonly Color128	ColourDimmed	= new Color128(1.000f, 1.000f, 1.000f, 0.5f);
		private static readonly Color128	ColourHighlight	= Color128.Black;
		private static readonly Color128	ColourNormal	= Color128.White;

		// some sizes and constants
		// TODO: make borders Menu fields dependent on font size
		private const int					MenuBorderX		= 16;
		private const int					MenuBorderY		= 16;
		private const int					MenuItemBorderX	= 8;
		private const int					MenuItemBorderY	= 2;
		private const float					LineSpacing		= 1.75f;	// the ratio between the font size and line distance
		private const int					SelectionNone	= -1;

		/********************
			BASE MENU ENTRY CLASS
		*********************/
		private abstract class MenuEntry
		{
			internal string	Text;
		}

		/********************
			DERIVED MENU ITEM CLASSES
		*********************/
		private class MenuCaption : MenuEntry
		{
			internal MenuCaption(string Text)
			{
				this.Text	= Text;
			}
		}
		private class MenuCommand : MenuEntry
		{
			internal MenuTag	Tag;
			internal int		Data;
			internal MenuCommand(string Text, MenuTag Tag, int Data)
			{
				this.Text	= Text;
				this.Tag	= Tag;
				this.Data	= Data;
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
			public Renderer.TextAlignment	Align;
			public MenuEntry[]				Items		= { };
			public int						ItemWidth	= 0;
			public int						Height		= 0;
			public int						Selection	= SelectionNone;
			public int						TopItem;			// the top displayed menu item
			public int						Width		= 0;

			/********************
				MENU C'TOR
			*********************/
			public SingleMenu(MenuType menuType, int data = 0)
			{
				int		i, menuItem;
				int		jump	= 0;
				Size	size;

				Align		= Renderer.TextAlignment.TopMiddle;
				Height		= Width = 0;
				Selection	= 0;						// defaults to first menu item
				switch (menuType)
				{
				case MenuType.Top:			// top level menu
					for (i = 0; i < Game.Stations.Length; i++)
						if (Game.PlayerStopsAtStation(i) & Game.Stations[i].Stops.Length > 0)
						{
							jump = 1;
							break;
						}
					Items 			= new MenuEntry[4 + jump];
					Items[0]		= new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.BackToSim, 0);
					if (jump > 0)
						Items[1]	= new MenuCommand(Interface.GetInterfaceString("menu_jump"), MenuTag.MenuJumpToStation, 0);
					Items[1+jump]	= new MenuCommand(Interface.GetInterfaceString("menu_exit"), MenuTag.MenuExitToMainMenu, 0);
					Items[2+jump]	= new MenuCommand("Customise Controls", MenuTag.MenuControls, 0);
					Items[3+jump]	= new MenuCommand(Interface.GetInterfaceString("menu_quit"), MenuTag.MenuQuit, 0);
					break;

				case MenuType.JumpToStation:	// list of stations to jump to
					// count the number of available stations
					menuItem = 0;
					for (i = 0; i < Game.Stations.Length; i++)
						if (Game.PlayerStopsAtStation (i) & Game.Stations [i].Stops.Length > 0)
							menuItem++;
					// list available stations, selecting the next station as predefined choice
					jump		= 0;							// no jump found yet
					Items		= new MenuEntry[menuItem + 1];
					Items[0]	= new MenuCommand (Interface.GetInterfaceString ("menu_back"), MenuTag.MenuBack, 0);
					menuItem	= 1;
					for (i = 0; i < Game.Stations.Length; i++)
						if (Game.PlayerStopsAtStation(i) & Game.Stations[i].Stops.Length > 0)
						{
							Items[menuItem] = new MenuCommand(Game.Stations[i].Name, MenuTag.JumpToStation, i);
							// if no preferred jump-to-station found yet and this station is
							// after the last station the user stopped at, select this item
							if (jump == 0 && i > TrainManager.PlayerTrain.LastStation)
							{
								jump		= i;
								Selection	= menuItem;
							}
							menuItem++;
						}
					Align	= Renderer.TextAlignment.TopLeft;
					break;

				case MenuType.ExitToMainMenu:
					Items		= new MenuEntry[3];
					Items[0]	= new MenuCaption (Interface.GetInterfaceString ("menu_exit_question"));
					Items[1]	= new MenuCommand (Interface.GetInterfaceString ("menu_exit_no"), MenuTag.MenuBack, 0);
					Items[2]	= new MenuCommand (Interface.GetInterfaceString ("menu_exit_yes"), MenuTag.ExitToMainMenu, 0);
					Selection	= 1;
					break;

				case MenuType.Quit:			// ask for quit confirmation
					Items		= new MenuEntry[3];
					Items[0]	= new MenuCaption(Interface.GetInterfaceString("menu_quit_question"));
					Items[1]	= new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.MenuBack, 0);
					Items[2]	= new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0);
					Selection	= 1;
					break;

				case MenuType.Controls:
					Items		= new MenuEntry[Interface.CurrentControls.Length + 1];
					Items[0]	= new MenuCommand (Interface.GetInterfaceString ("menu_back"), MenuTag.MenuBack, 0);
					for (i = 0; i < Interface.CurrentControls.Length; i++)
						Items[i+1] = new MenuCommand(Interface.CurrentControls[i].Command.ToString(), MenuTag.Control, i);
					Align		= Renderer.TextAlignment.TopLeft;
					break;

				case MenuType.Control:
					Selection	= SelectionNone;
					Items		= new MenuEntry[4];
					// get code name and description
					Interface.Control loadedControl = Interface.CurrentControls[data];
					for (int h = 0; h < Interface.CommandInfos.Length; h++)
					{
						if (Interface.CommandInfos[h].Command == loadedControl.Command)
						{
							Items[0]	= new MenuCommand(loadedControl.Command.ToString() + " - " +
									Interface.CommandInfos[h].Description, MenuTag.None, 0);
							break;
						}
					}
					// get assignment
					String	str = "";
					switch (loadedControl.Method)
					{
					case Interface.ControlMethod.Keyboard:
						if (loadedControl.Modifier != Interface.KeyboardModifier.None)
							str = "Keyboard [" + loadedControl.Modifier + "-" + loadedControl.Key + "]";
						else
							str = "Keyboard [" + loadedControl.Key + "]";
						break;
					case Interface.ControlMethod.Joystick:
						str = "Joystick " + loadedControl.Device + " [" + loadedControl.Component + " " + loadedControl.Element + "]";
						switch (loadedControl.Component)
						{
						case Interface.JoystickComponent.FullAxis:
						case Interface.JoystickComponent.Axis:
							str += " " + (loadedControl.Direction == 1 ? "Positive" : "Negative");
							break;
//						case Interface.JoystickComponent.Button:
//							str = str;
//							break;
						case Interface.JoystickComponent.Hat:
							str += " " + (OpenTK.Input.HatPosition)loadedControl.Direction;
							break;
						case Interface.JoystickComponent.Invalid:
							str = "N/A";
							break;
						}
						break;
					case Interface.ControlMethod.Invalid:
						str = "N/A";
						break;
					}
					Items[1]	= new MenuCommand("Current assignment: " + str, MenuTag.None, 0);
					Items[2]	= new MenuCommand(" ", MenuTag.None, 0);
					Items[3]	= new MenuCommand("Please press any key or move a joystick axis to set this control...", MenuTag.None, 0);
					break;
				}

				// compute menu extent
				for (i = 0; i < Items.Length; i++)
				{
					size = Renderer.MeasureString(Game.Menu.MenuFont, Items [i].Text);
					if (size.Width > Width)
						Width		= size.Width;
					if (!(Items[i] is MenuCaption) && size.Width > ItemWidth)
						ItemWidth	= size.Width;
				}
				Height	= Items.Length * Game.Menu.LineHeight;
				TopItem	= 0;
			}

		}					// end of private class SingleMenu

		/********************
			MENU SYSTEM FIELDS
		*********************/
		private		int					CurrMenu				= -1;
		private		int					em;
		private		int					lineHeight;
		private		SingleMenu[]		Menus					= { };
		private		Fonts.OpenGlFont	menuFont				= null;
		private		bool				IsCustomisingControl	= false;
		private		bool				IsInitialized			= false;
		private		int					CustomControlIdx;	// the index of the control being customized
		// properties (to allow read-only access to some fields)
		internal	int					LineHeight				{ get { return lineHeight; } }
		internal	Fonts.OpenGlFont	MenuFont 				{ get { return menuFont; } }

		/********************
			MENU SYSTEM SINGLETON C'TOR
		*********************/
		private static readonly Menu instance = new Menu();
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static Menu()	{ }
		private Menu()	{ }
		public static Menu Instance	{ get { return instance; } }

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
			if (Screen.Height <= 512)		menuFont	= Fonts.SmallFont;
			else if (Screen.Height <= 680)	menuFont	= Fonts.NormalFont;
			else if (Screen.Height <= 890)	menuFont	= Fonts.LargeFont;
			else if (Screen.Height <= 1150)	menuFont	= Fonts.VeryLargeFont;
			else 							menuFont	= Fonts.EvenLargerFont;
			em				= (int)menuFont.FontSize;
			lineHeight		= (int)(em * LineSpacing);
			IsInitialized	= true;
		}

		//
		// RESET MENU SYSTEM TO INITIAL CONDITIONS
		//
		private void Reset()
		{
			CurrMenu	= -1;
			Menus		= new SingleMenu[] { };
		}

		//
		// PUSH ANOTHER MENU
		//
		public void PushMenu(MenuType type, int data = 0)
		{
			if (!IsInitialized)
				Init ();
			CurrMenu++;
			if (Menus.Length <= CurrMenu)
				Array.Resize(ref Menus, CurrMenu + 1);
			Menus[CurrMenu]			= new Menu.SingleMenu(type, data);
			Game.CurrentInterface	= Game.InterfaceType.Menu;
		}

		//
		// POP LAST MENU
		//
		public void PopMenu()
		{
			if (CurrMenu > 0)			// if more than one menu remaining...
				CurrMenu--;				// ...back to previous smenu
			else
			{							// if only one menu remaining...
				Reset();
				Game.CurrentInterface = Game.InterfaceType.Normal;	// return to simulation
			}
		}

		//
		// IS CUSTOMIZING CONTROL?
		//
		public bool IsCustomizingControl()
		{
			return IsCustomisingControl;
		}

		//
		// SET CONTROL CUSTOM DATA
		//
		internal void SetControlKbdCustomData(Key key, Interface.KeyboardModifier keybMod)
		{
			if (IsCustomisingControl)
			{
				Interface.CurrentControls[CustomControlIdx].Method		= Interface.ControlMethod.Keyboard;
				Interface.CurrentControls[CustomControlIdx].Key			= key;
				Interface.CurrentControls[CustomControlIdx].Modifier	= keybMod;
				Interface.SaveControls(null);
				PopMenu();
				IsCustomisingControl	= false;
			}
		}
		internal void SetControlJoyCustomData(int device, Interface.JoystickComponent component, int element, int dir)
		{
			if (IsCustomisingControl)
			{
				Interface.CurrentControls[CustomControlIdx].Method		= Interface.ControlMethod.Joystick;
				Interface.CurrentControls[CustomControlIdx].Device		= device;
				Interface.CurrentControls[CustomControlIdx].Component	= component;
				Interface.CurrentControls[CustomControlIdx].Element		= element;
				Interface.CurrentControls[CustomControlIdx].Direction	= dir;
				Interface.SaveControls(null);
				PopMenu();
				IsCustomisingControl	= false;
			}
		}

		//
		// PROCESS MENU COMMAND
		//
		/// <summary>Processes a user command for the current menu</summary>
		/// <param name="cmd">The command to apply to the current menu</param>
		/// <param name="timeElapsed">The time elapsed since previous frame</param>
		internal void ProcessCommand(Interface.Command cmd, double timeElapsed)
		{
			// MenuBack is managed independently from single menu data
			if (cmd == Interface.Command.MenuBack)
			{
				PopMenu();
				return;
			}

			SingleMenu menu	= Menus[CurrMenu];
			if (menu.Selection == SelectionNone)	// if menu has no selection, do nothing
				return;
			switch (cmd)
			{
			case Interface.Command.MenuUp:		// UP
				if (menu.Selection > 0 &&
					!(menu.Items[menu.Selection - 1] is MenuCaption) )
					menu.Selection--;
				break;
			case Interface.Command.MenuDown:	// DOWN
				if (menu.Selection < menu.Items.Length - 1)
					menu.Selection++;
				break;
//			case Interface.Command.MenuBack:	// ESC	// managed above
//				break;
			case Interface.Command.MenuEnter:	// ENTER
				if (menu.Items[menu.Selection] is MenuCommand)
				{
					MenuCommand menuItem = (MenuCommand)menu.Items[menu.Selection];
					switch (menuItem.Tag)
					{
					// menu management commands
					case MenuTag.MenuBack:				// BACK TO PREVIOUS MENU
						Menu.instance.PopMenu();
						break;
					case MenuTag.MenuJumpToStation:		// TO STATIONS MENU
						Menu.instance.PushMenu(MenuType.JumpToStation);
						break;
					case MenuTag.MenuExitToMainMenu:	// TO EXIT MENU
						Menu.instance.PushMenu(MenuType.ExitToMainMenu);
						break;
					case MenuTag.MenuQuit:				// TO QUIT MENU
						Menu.instance.PushMenu(MenuType.Quit);
						break;
					case MenuTag.MenuControls:			// TO CONTROLS MENU
						Menu.instance.PushMenu(MenuType.Controls);
						break;
					case MenuTag.BackToSim:				// OUT OF MENU BACK TO SIMULATION
						Reset();
						Game.CurrentInterface = Game.InterfaceType.Normal;
						break;

						// simulation commands
					case MenuTag.JumpToStation:			// JUMP TO STATION
						Reset();
						TrainManager.JumpTrain(TrainManager.PlayerTrain, menuItem.Data);
						break;
					case MenuTag.ExitToMainMenu:		// BACK TO MAIN MENU
						Reset();
						Program.RestartArguments =
							Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade ? "/review" : "";
						MainLoop.Quit = true;
						break;
					case MenuTag.Control:				// CONTROL CUSTOMIZATION
						PushMenu(MenuType.Control, ((MenuCommand)menu.Items[menu.Selection]).Data);
						IsCustomisingControl	= true;
						CustomControlIdx		= ((MenuCommand)menu.Items[menu.Selection]).Data;
						break;
					case MenuTag.Quit:					// QUIT PROGRAMME
						Reset();
						MainLoop.Quit = true;
						break;
					}
				}
				break;
			case Interface.Command.MiscFullscreen:
				// fullscreen
				Screen.ToggleFullscreen();
				break;
			case Interface.Command.MiscMute:
				// mute
				Sounds.GlobalMute = !Sounds.GlobalMute;
				Sounds.Update(timeElapsed, Interface.CurrentOptions.SoundModel);
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

			SingleMenu menu	= Menus[CurrMenu];
			// overlay background
			GL.Color4(ovlR, ovlG, ovlB, ovlA);
			Renderer.RenderOverlaySolid(0.0, 0.0, (double)Screen.Width, (double)Screen.Height);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

			// HORIZONTAL PLACEMENT: centre the menu in the main window
			int		menuXmin			= (Screen.Width - menu.Width) / 2;		// menu left edge (border excluded)
			int		menuXmax			= menuXmin + menu.Width;				// menu right edge (border excluded)
			int		itemLeft			= (Screen.Width - menu.ItemWidth) / 2;	// item left edge
			// if menu alignment is left, left-align items, otherwise centre them in the screen
			int		itemX				= (menu.Align & Renderer.TextAlignment.Left) != 0 ? itemLeft : Screen.Width / 2;
			// VERTICAL PLACEMENT: centre the menu in the main window
			int		menuYmin			= (Screen.Height- menu.Height)/ 2;		// menu top edge (border excluded)
			int		menuYmax			= menuYmin + menu.Height;				// menu bottom edge (border excluded)
			int		itemY				= menuYmin;								// item top edge
			// assume all items fit in the screen
			int		visibleItems		= menu.Items.Length;
			int		menuBottomItem		= visibleItems - 1;

			// if there are more items than can fit in the screen height,
			// (there should be at least room for the menu top border)
			if (menuYmin < MenuBorderY)
			{
				// the number of lines which fit in the screen
				int	numOfLines	= (Screen.Height - MenuBorderY*2) / lineHeight;
				visibleItems	= numOfLines - 2;					// at least an empty line at the top and at the bottom
				// split the menu in chunks of 'visibleItems' items
				// and display the chunk which contains the currently selected item
				menu.TopItem	= menu.Selection - (menu.Selection % visibleItems);
				visibleItems	= menu.Items.Length - menu.TopItem < visibleItems ?	// in the last chunk,
					menu.Items.Length - menu.TopItem : visibleItems;				// display remaining items only
				menuBottomItem	= menu.TopItem + visibleItems - 1;
				menuYmin			= (Screen.Height - numOfLines*lineHeight) / 2;
				menuYmax			= menuYmin + numOfLines * lineHeight;
				// first menu item is drawn on second line
				// first line is empty on first screnn and contains an ellipsis on following sreens
				itemY			= menuYmin + lineHeight;
			}

			// draw the menu background
			GL.Color4(bkgMenuR, bkgMenuG, bkgMenuB, bkgMenuA);
			Renderer.RenderOverlaySolid(menuXmin - MenuBorderX, menuYmin - MenuBorderY,
				menuXmax + MenuBorderX, menuYmax + MenuBorderY);

			// if not starting from the top of the menu, draw a dimmed ellipsis item
			if (menu.TopItem > 0)
				Renderer.DrawString(MenuFont, "...", new System.Drawing.Point(itemX, menuYmin),
					menu.Align, ColourDimmed, false);
			// draw the items
			for (i = menu.TopItem; i <= menuBottomItem; i++)
			{
				if (i == menu.Selection)
				{
					// draw a solid highlight rectangle under the text
					// HACK! the highlight rectangle has to be shifted a little down to match
					// the text body. OpenGL 'feature'?
					GL.Color4(bkgHgltR, bkgHgltG, bkgHgltB, bkgHgltA);
					Renderer.RenderOverlaySolid(itemLeft-MenuItemBorderX, itemY/*-MenuItemBorderY*/,
						itemLeft+menu.ItemWidth+MenuItemBorderX, itemY+em+MenuItemBorderY*2);
					// draw the text
					Renderer.DrawString(MenuFont, menu.Items[i].Text, new System.Drawing.Point(itemX, itemY),
						menu.Align, ColourHighlight, false);
				}
				else if (menu.Items[i] is MenuCaption)
					Renderer.DrawString(MenuFont, menu.Items[i].Text, new System.Drawing.Point(itemX, itemY),
						menu.Align, ColourCaption, false);
				else
					Renderer.DrawString(MenuFont, menu.Items[i].Text, new System.Drawing.Point(itemX, itemY),
						menu.Align, ColourNormal, false);
				itemY += lineHeight;
			}
			// if not at the end of the menu, draw a dimmed ellipsis item at the bottom
			if (i < menu.Items.Length - 1)
				Renderer.DrawString(MenuFont, "...", new System.Drawing.Point(itemX, itemY),
					menu.Align, ColourDimmed, false);
		}
	}

}
