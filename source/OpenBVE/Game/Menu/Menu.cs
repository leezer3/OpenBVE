using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OpenBveApi.Math;
using OpenBveApi.Packages;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
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
		internal static readonly Picturebox LogoPictureBox = new Picturebox(Program.Renderer);
		internal static List<FoundSwitch> nextSwitches = new List<FoundSwitch>();
		internal static List<FoundSwitch> previousSwitches = new List<FoundSwitch>();
		internal static bool switchesFound = false;
		
		private const int SelectionNone = -1;

		private double lastTimeElapsed;
		private static readonly string currentDatabaseFile = Path.CombineFile(Program.FileSystem.PackageDatabaseFolder, "packages.xml");

		/********************
			MENU SYSTEM FIELDS
		*********************/
		
		private int CustomControlIdx;   // the index of the control being customized
		private bool isCustomisingControl = false;
		

		

		/********************
			MENU SYSTEM SINGLETON C'TOR
		*********************/

		private GameMenu() : base(Program.Renderer, Interface.CurrentOptions)
		{
		}

		/// <summary>Returns the current menu instance (If applicable)</summary>
		public static readonly GameMenu Instance = new GameMenu();

		/********************
			MENU SYSTEM METHODS
		*********************/
		public override void Initialize()
		{
			Reset();
			OnResize();
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				//Find the current menu back key- It's unlikely that we want to set a new key to this
				if (Interface.CurrentControls[i].Command == Translations.Command.MenuBack)
				{
					MenuBackKey = Interface.CurrentControls[i].Key;
					break;
				}
			}
			
			IsInitialized = true;

			// add controls to list so we can itinerate them on mouse calls etc.
			menuControls.Add(routePictureBox);
			menuControls.Add(controlPictureBox);
			menuControls.Add(switchMainPictureBox);
			menuControls.Add(switchSettingPictureBox);
			menuControls.Add(switchMapPictureBox);
			menuControls.Add(LogoPictureBox);
			menuControls.Add(controlTextBox);
			menuControls.Add(routeDescriptionBox);
			menuControls.Add(nextImageButton);
			menuControls.Add(previousImageButton);
			menuControls.Add(nextStepButton);
		}

		/// <summary>Should be called when the screen resolution changes to re-position all items on the menu appropriately</summary>
		internal void OnResize()
		{
			// choose the text font size according to screen height
			// the boundaries follow approximately the progression
			// of font sizes defined in Graphics/Fonts.cs
			if (Program.Renderer.Screen.Height <= 512) MenuFont = Program.Renderer.Fonts.SmallFont;
			else if (Program.Renderer.Screen.Height <= 680) MenuFont = Program.Renderer.Fonts.NormalFont;
			else if (Program.Renderer.Screen.Height <= 890) MenuFont = Program.Renderer.Fonts.LargeFont;
			else if (Program.Renderer.Screen.Height <= 1150) MenuFont = Program.Renderer.Fonts.VeryLargeFont;
			else MenuFont = Program.Renderer.Fonts.EvenLargerFont;

			if (Interface.CurrentOptions.UserInterfaceFolder == "Large")
			{
				// If using the large HUD option, increase the text size in the menu too
				MenuFont = Program.Renderer.Fonts.NextLargestFont(MenuFont);
			}

			LineHeight = (int)(MenuFont.FontSize * LineSpacing);
			int quarterWidth = (int)(Program.Renderer.Screen.Width / 4.0);
			int quarterHeight = (int)(Program.Renderer.Screen.Height / 4.0);
			nextStepButton.Location = new Vector2(Program.Renderer.Screen.Width - 10, Program.Renderer.Screen.Height - 10);
			nextStepButton.Location -= nextStepButton.Size; // move into place
			nextStepButton.IsVisible = false;
			int descriptionLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 2;
			int descriptionWidth = quarterWidth + quarterWidth / 2;
			int descriptionHeight = descriptionWidth;
			if (descriptionHeight + quarterWidth > Program.Renderer.Screen.Height - 20 - nextStepButton.Size.Y)
			{
				descriptionHeight = Program.Renderer.Screen.Height - quarterWidth - 20 - (int)nextStepButton.Size.Y;
			}
			routeDescriptionBox.Location = new Vector2(descriptionLoc, quarterWidth);
			routeDescriptionBox.Size = new Vector2(descriptionWidth, descriptionHeight);
			int imageLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 4;
			routePictureBox.Location = new Vector2(imageLoc, 0);
			routePictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			routePictureBox.BackgroundColor = Color128.White;
			nextImageButton.Location = new Vector2(imageLoc + quarterWidth + nextImageButton.Size.X, quarterWidth / 2.0);
			nextImageButton.IsVisible = false;
			previousImageButton.Location = new Vector2(imageLoc - previousImageButton.Size.X * 2, quarterWidth / 2.0);
			previousImageButton.IsVisible = false;
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
			controlPictureBox.Location = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Height / 8.0);
			controlPictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			controlPictureBox.BackgroundColor = Color128.Transparent;
			controlTextBox.Location = new Vector2(Program.Renderer.Screen.Width / 2.0, Program.Renderer.Screen.Height / 8.0 + quarterWidth);
			controlTextBox.Size = new Vector2(quarterWidth, quarterWidth);
			controlTextBox.BackgroundColor = Color128.Black;
			if (CurrMenu >= 0)
			{
				Menus[CurrMenu].ComputeExtent(Menus[CurrMenu].Type, MenuFont, Renderer.Screen.Width / 2.0, LineHeight);
			}
			ComputePosition();
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
			if (!IsInitialized)
				Initialize();
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
			Menus[CurrMenu] = new SingleMenu(this, type, data, MaxWidth);
			if (replace)
			{
				Menus[CurrMenu].Selection = 1;
			}
			ComputePosition();
			Program.Renderer.CurrentInterface = TrainManagerBase.PlayerTrain == null ? InterfaceType.GLMainMenu : InterfaceType.Menu;
			
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

		/// <summary>Processes a scroll wheel event</summary>
		/// <param name="Scroll">The delta</param>
		public override void ProcessMouseScroll(int Scroll)
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
			base.ProcessMouseScroll(Scroll);
		}

		public override void DragFile(object sender, OpenTK.Input.FileDropEventArgs e)
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

		public override bool ProcessMouseMove(int x, int y)
		{
			Program.Renderer.GameWindow.CursorVisible = true;
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
				nextImageButton.MouseMove(x, y);
				previousImageButton.MouseMove(x, y);
				routeDescriptionBox.MouseMove(x, y);
				nextStepButton.MouseMove(x, y);
			}
			if (x < menuMin.X || x > menuMax.X || y < menuMin.Y || y > menuMax.Y)
			{
				return false;
			}

			int item = (int) ((y - topItemY) / LineHeight + menu.TopItem);
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
	}
}
