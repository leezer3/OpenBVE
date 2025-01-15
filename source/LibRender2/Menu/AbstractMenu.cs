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

using LibRender2.Screens;
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using System.Collections.Generic;
using LibRender2.Primitives;
using OpenBveApi;
using OpenBveApi.Input;
using OpenBveApi.Interface;

namespace LibRender2.Menu
{
	/// <summary>Provides the abstract class for creating OpenGL based menus</summary>
	public abstract class AbstractMenu
	{
		/// <summary>The color used for the overlay</summary>
		public readonly Color128 overlayColor = new Color128(0.0f, 0.0f, 0.0f, 0.2f);

		/// <summary>The color used for the menu background</summary>
		public readonly Color128 backgroundColor = Color128.Black;

		/// <summary>The color used to draw the highlight over a selected item</summary>
		public readonly Color128 highlightColor = Color128.Orange;

		/// <summary> The color used to draw the highlight over a selected folder</summary>
		public readonly Color128 folderHighlightColor = new Color128(0.0f, 0.69f, 1.0f, 1.0f);

		/// <summary>The color used to draw the highlight over a selected routefile</summary>
		public readonly Color128 routeHighlightColor = new Color128(0.0f, 1.0f, 0.69f, 1.0f);

		/// <summary>The color used for caption text</summary>
		public static readonly Color128 ColourCaption = new Color128(0.750f, 0.750f, 0.875f, 1.0f);

		/// <summary>The color used to draw dimmed text</summary>
		public static readonly Color128 ColourDimmed = new Color128(1.000f, 1.000f, 1.000f, 0.5f);

		/// <summary>The color used to draw highlighted text</summary>
		public static readonly Color128 ColourHighlight = Color128.Black;

		/// <summary>The color used to draw normal text</summary>
		public static readonly Color128 ColourNormal = Color128.White;

		/// <summary> Holds the stack of menus</summary>
		public MenuBase[] Menus = { };

		/// <summary>The font currently used for drawing menu items</summary>
		public OpenGlFont MenuFont = null;

		/// <summary>The ratio between the menu font size and the line spacing</summary>
		public const float LineSpacing = 1.75f;

		/// <summary>The number of visible items in the menu</summary>
		public int visibleItems;

		/// <summary>The screen co-ordinates of the top-left of the menu</summary>
		public Vector2 menuMin;

		/// <summary>The screen co-ordinates of the bottom-right of the menu</summary>
		public Vector2 menuMax;

		/// <summary>Holds a reference to the base renderer</summary>
		public readonly BaseRenderer Renderer;

		/// <summary>Holds a reference to the options</summary>
		public readonly BaseOptions CurrentOptions;
		
		/// <summary>The index of the current menu within the stack</summary>
		public int CurrMenu = -1;

		/// <summary>The border for the menu in pixels</summary>
		public static readonly Vector2 Border = new Vector2(16, 16);

		/// <summary>The border for each menu item in pixels</summary>
		public static readonly Vector2 ItemBorder = new Vector2(8, 2);

		/// <summary>The height of the top item in the current menu in pixels</summary>
		public double topItemY;

		/// <summary>The total height of one rendered menu item in pixels</summary>
		public int lineHeight;

		/// <summary>The controls within the menu</summary>
		public List<GLControl> menuControls = new List<GLControl>();

		/// <summary>Whether the menu system is initialized</summary>
		public bool IsInitialized = false;

		/// <summary>The key used to go back within the menu stack</summary>
		public Key MenuBackKey;

		/// <summary>Creates a new menu instance</summary>
		protected AbstractMenu(BaseRenderer renderer, BaseOptions currentOptions)
		{
			Renderer = renderer;
			CurrentOptions = currentOptions;
		}

		/// <summary>Initializes the menu system upon first use</summary>
		public abstract void Initialize();

		/// <summary>Resets the menu system to it's initial condition</summary>
		public virtual void Reset()
		{
			CurrMenu = -1;
			Menus = new MenuBase[] { };
		}

		/// <summary>Pushes a menu into the menu stack</summary>
		/// <param name= "type">The type of menu to push</param>
		/// <param name= "data">The index of the menu in the menu stack (If pushing an existing higher level menu)</param>
		/// <param name="replace">Whether we are replacing the selected menu item</param>
		public abstract void PushMenu(MenuType type, int data = 0, bool replace = false);

		/// <summary>Pops the previous menu in the menu stack</summary>
		public void PopMenu()
		{
			if (CurrMenu > 0)           // if more than one menu remaining...
			{
				CurrMenu--;             // ...back to previous menu
				if (CurrMenu > 0 && Menus[CurrMenu] == null)
				{
					// HACK: choose train dialog with not found train, needs special reset to get back to route list correctly
					CurrMenu--;
					Menus[CurrMenu].TopItem = 1;
					Menus[CurrMenu].Selection = -1;
				}
				ComputePosition();
			}
			else
			{                           // if only one menu remaining...
				Reset();
				Renderer.CurrentInterface = InterfaceType.Normal;  // return to simulation
			}
		}

		/// <summary>Processes a scroll wheel event</summary>
		/// <param name="Scroll">The delta</param>
		public virtual void ProcessMouseScroll(int Scroll)
		{
			if (Menus.Length == 0)
			{
				return;
			}
			// Load the current menu
			Menus[CurrMenu].ProcessScroll(Scroll, visibleItems);
		}

		/// <summary>Processes a mouse move event</summary>
		/// <param name="x">The screen-relative x coordinate of the move event</param>
		/// <param name="y">The screen-relative y coordinate of the move event</param>
		public virtual bool ProcessMouseMove(int x, int y)
		{
			return true;
		}

		/// <summary>Processes a mouse down event</summary>
		/// <param name="x">The screen-relative x coordinate of the down event</param>
		/// <param name="y">The screen-relative y coordinate of the down event</param>
		public void ProcessMouseDown(int x, int y)
		{
			for (int i = 0; i < menuControls.Count; i++)
			{
				if (menuControls[i].IsVisible)
				{
					menuControls[i].MouseDown(x, y);
				}
			}
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

		/// <summary>Processes a user command for the current menu</summary>
		/// <param name="cmd">The command to apply to the current menu</param>
		/// <param name="timeElapsed">The time elapsed since previous frame</param>
		public virtual void ProcessCommand(Translations.Command cmd, double timeElapsed)
		{

		}

		/// <summary>Processes a file drop event</summary>
		public virtual void DragFile(object sender, OpenTK.Input.FileDropEventArgs e)
		{

		}


		/// <summary>Computes the position in the screen of the current menu.</summary>
		/// <remarks>Also sets the menu size</remarks>
		public void ComputePosition()
		{
			if (CurrMenu < 0 || CurrMenu >= Menus.Length)
				return;

			MenuBase menu = Menus[CurrMenu];
			for (int i = 0; i < menu.Items.Length; i++)
			{
				/*
				 * HACK: This is a property method, and is also used to
				 * reset the timer and display string back to the starting values
				 */
				if (menu.Items[i] != null)
				{
					menu.Items[i].DisplayLength = menu.Items[i].DisplayLength;
				}
			}

			// HORIZONTAL PLACEMENT
			switch (menu.Align)
			{
				case TextAlignment.TopLeft:
					// Left aligned
					menuMin.X = 0;
					break;
				default:
					// Centered in window
					menuMin.X = (Renderer.Screen.Width - menu.Width) / 2;     // menu left edge (border excluded)	
					break;
			}

			menuMax.X = menuMin.X + menu.Width;               // menu right edge (border excluded)
															  // VERTICAL PLACEMENT: centre the menu in the main window
			menuMin.Y = (Renderer.Screen.Height - menu.Height) / 2;       // menu top edge (border excluded)
			menuMax.Y = menuMin.Y + menu.Height;              // menu bottom edge (border excluded)
			topItemY = menuMin.Y;                                // top edge of top item
																 // assume all items fit in the screen
			visibleItems = menu.Items.Length;

			// if there are more items than can fit in the screen height,
			// (there should be at least room for the menu top border)
			if (menuMin.Y < Border.Y)
			{
				// the number of lines which fit in the screen
				int numOfLines = (int)(Renderer.Screen.Height - Border.Y * 2) / lineHeight;
				visibleItems = numOfLines - 2;                  // at least an empty line at the top and at the bottom
																// split the menu in chunks of 'visibleItems' items
																// and display the chunk which contains the currently selected item
				menu.TopItem = menu.Selection - (menu.Selection % visibleItems);
				visibleItems = menu.Items.Length - menu.TopItem < visibleItems ?    // in the last chunk,
					menu.Items.Length - menu.TopItem : visibleItems;                // display remaining items only
				menuMin.Y = (Renderer.Screen.Height - numOfLines * lineHeight) / 2.0;
				menuMax.Y = menuMin.Y + numOfLines * lineHeight;
				// first menu item is drawn on second line (first line is empty
				// on first screen and contains an ellipsis on following screens
				topItemY = menuMin.Y + lineHeight;
			}
		}

		/// <summary>Draws the current menu</summary>
		/// <param name="RealTimeElapsed">The real time elapsed since the last draw call</param>
		/// <remarks>Note that the real time elapsed may be different to the game time elapsed</remarks>
		public abstract void Draw(double RealTimeElapsed);

	}
}
