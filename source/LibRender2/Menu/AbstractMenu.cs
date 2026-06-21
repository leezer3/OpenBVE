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
using OpenBveApi.Hosts;

namespace LibRender2.Menu
{
	/// <summary>Provides the abstract class for creating OpenGL based menus</summary>
	public abstract class AbstractMenu
	{
		/// <summary>The color used for the overlay</summary>
		public readonly Color128 overlayColor = new Color128(0.0f, 0.0f, 0.0f, 0.45f);

		/// <summary>The color used for the menu background</summary>
		public readonly Color128 backgroundColor = new Color128(0.08f, 0.09f, 0.11f, 0.94f);

		/// <summary>The color used to draw the highlight over a selected item</summary>
		public readonly Color128 highlightColor = new Color128(0.0f, 0.47f, 0.83f, 1.0f);

		/// <summary> The color used to draw the highlight over a selected folder</summary>
		public readonly Color128 folderHighlightColor = new Color128(0.0f, 0.69f, 1.0f, 1.0f);

		/// <summary>The color used to draw the highlight over a selected routefile</summary>
		public readonly Color128 routeHighlightColor = new Color128(0.0f, 1.0f, 0.69f, 1.0f);

		/// <summary>The color used for caption text</summary>
		public static readonly Color128 ColourCaption = new Color128(0.58f, 0.75f, 0.98f, 1.0f);

		/// <summary>The color used to draw dimmed text</summary>
		public static readonly Color128 ColourDimmed = new Color128(0.6f, 0.64f, 0.7f, 0.65f);

		/// <summary>The color used to draw highlighted text</summary>
		public static readonly Color128 ColourHighlight = Color128.White;

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
		public int LineHeight;

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
				if (IsSidebarMode)
				{
					SidebarVisible = false;
					startOffset = currentOffset;
					animationElapsed = 0.0;
					isAnimating = true;
				}
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

		/// <summary>Whether the menu system is configured as a sliding sidebar on the left</summary>
		public bool IsSidebarMode = true;

		/// <summary>Whether the sidebar is currently open/visible</summary>
		public bool SidebarVisible = false;

		/// <summary>The width of the sidebar as a fraction of screen width</summary>
		public double SidebarWidthRatio = 0.25;

		/// <summary>Gets the current calculated width of the sidebar panel</summary>
		public double SidebarWidth => Renderer.Screen.Width * SidebarWidthRatio;

		public double currentOffset = -99999.0;
		public double animationElapsed = 0.0;
		public double animationDuration = 0.4;
		public double startOffset = 0.0;
		public bool isAnimating = false;

		public bool isScrubbing = false;
		public MenuOption scrubbingOption = null;
		public int lastMouseX = 0;

		public Vector4 GetToggleButtonRect()
		{
			double buttonX = IsSidebarMode ? (currentOffset < -90000.0 ? (SidebarVisible ? SidebarWidth : 0) : (currentOffset + SidebarWidth)) : 0;
			double buttonY = Renderer.Screen.Height / 2.0 - 20;
			return new Vector4((float)buttonX, (float)buttonY, 24f, 40f);
		}

		public void UpdateTransition(double TimeElapsed)
		{
			if (!IsSidebarMode) return;

			double target = SidebarVisible ? 0 : -SidebarWidth;
			if (currentOffset < -90000.0)
			{
				currentOffset = target;
				startOffset = target;
				isAnimating = false;
			}

			if (isAnimating)
			{
				animationElapsed += TimeElapsed;
				double x = System.Math.Min(animationElapsed / animationDuration, 1.0);
				double progress = 1.0 - System.Math.Pow(1.0 - x, 3.0); // easeOutCubic
				currentOffset = startOffset + (target - startOffset) * progress;

				if (x >= 1.0)
				{
					currentOffset = target;
					isAnimating = false;
					if (!SidebarVisible)
					{
						Reset();
					}
				}
			}
			else
			{
				currentOffset = target;
			}

			menuMin.X = currentOffset;
			menuMax.X = menuMin.X + SidebarWidth;

			RepositionSidebarControls();
		}

		public virtual void RepositionSidebarControls()
		{
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
			if (IsSidebarMode)
			{
				Vector4 rect = GetToggleButtonRect();
				if (x >= rect.X && x <= rect.X + rect.Z && y >= rect.Y && y <= rect.Y + rect.W)
				{
					SidebarVisible = !SidebarVisible;
					startOffset = currentOffset;
					animationElapsed = 0.0;
					isAnimating = true;
					if (SidebarVisible)
					{
						if (CurrMenu == -1)
						{
							PushMenu(MenuType.Options);
						}
						else
						{
							Renderer.CurrentInterface = InterfaceType.Menu;
						}
					}
					else
					{
						Renderer.CurrentInterface = InterfaceType.Normal;
					}
					ComputePosition();
					return;
				}
			}
			if (CurrMenu >= 0 && Menus.Length > 0)
			{
				var menu = Menus[CurrMenu];
				if (menu.Selection >= 0 && menu.Selection < menu.Items.Length)
				{
					var entry = menu.Items[menu.Selection];
					if (entry is MenuOption opt && (opt.Type == OptionType.ViewingDistance || opt.Type == OptionType.NearClip))
					{
						isScrubbing = true;
						scrubbingOption = opt;
						lastMouseX = x;
					}
				}
			}

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
				// If scrubbing, do not trigger entry actions
				if (!isScrubbing)
				{
					ProcessCommand(Translations.Command.MenuEnter, 0);
				}
			}
		}

		/// <summary>Processes a mouse up event</summary>
		public void ProcessMouseUp(int x, int y)
		{
			isScrubbing = false;
			scrubbingOption = null;
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

			if (IsSidebarMode)
			{
				menu.Align = TextAlignment.TopLeft;
				menu.Width = SidebarWidth - 2.0 * Border.X;
				menu.ItemWidth = menu.Width;

				menuMin.X = SidebarVisible ? 0 : -SidebarWidth;
				menuMax.X = menuMin.X + SidebarWidth;
				menuMin.Y = 0;
				menuMax.Y = Renderer.Screen.Height;
				topItemY = Border.Y;
				visibleItems = (int)(Renderer.Screen.Height - Border.Y * 2) / LineHeight;

				RepositionSidebarControls();
			}
			else
			{
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
					int numOfLines = (int)(Renderer.Screen.Height - Border.Y * 2) / LineHeight;
					visibleItems = numOfLines - 2;                  // at least an empty line at the top and at the bottom
																	// split the menu in chunks of 'visibleItems' items
																	// and display the chunk which contains the currently selected item
					menu.TopItem = menu.Selection - (menu.Selection % visibleItems);
					visibleItems = menu.Items.Length - menu.TopItem < visibleItems ?    // in the last chunk,
						menu.Items.Length - menu.TopItem : visibleItems;                // display remaining items only
					menuMin.Y = (Renderer.Screen.Height - numOfLines * LineHeight) / 2.0;
					menuMax.Y = menuMin.Y + numOfLines * LineHeight;
					// first menu item is drawn on second line (first line is empty
					// on first screen and contains an ellipsis on following screens
					topItemY = menuMin.Y + LineHeight;
				}
			}
		}

		public void DrawSidebarToggleButton(double RealTimeElapsed)
		{
			if (!IsSidebarMode) return;
			Vector4 rect = GetToggleButtonRect();
			Renderer.Rectangle.Draw(null, new Vector2(rect.X, rect.Y), new Vector2(rect.Z, rect.W), backgroundColor);
			string arrow = SidebarVisible ? "<" : ">";
			if (MenuFont == null)
			{
				MenuFont = Renderer.Fonts.NormalFont;
			}
			Renderer.OpenGlString.Draw(MenuFont, arrow, new Vector2(rect.X + 8, rect.Y + 10), TextAlignment.TopLeft, ColourNormal, false);
		}

		public MenuEntry[] CreateOptionsEntries(HostApplication host)
		{
			List<MenuEntry> items = new List<MenuEntry>();
			items.Add(new MenuCaption(this, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "panel", "options" })));
			
			items.Add(new MenuOption(this, OptionType.ScreenResolution, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "resolution" }), Renderer.Screen.AvailableResolutions.ToArray()));
			items.Add(new MenuOption(this, OptionType.FullScreen, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "display_mode_fullscreen" }), new[] { "true", "false" }));
			
			items.Add(new MenuOption(this, OptionType.Interpolation, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation" }), new[]
			{
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearest"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinear"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearestmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinearmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_trilinearmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_anisotropic"})
			}));
			
			items.Add(new MenuOption(this, OptionType.AnisotropicLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_anisotropic_level" }), new[] { "0", "2", "4", "8", "16" }));
			items.Add(new MenuOption(this, OptionType.AntialiasingLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_antialiasing_level" }), new[] { "0", "2", "4", "8", "16" }));
			
			items.Add(new MenuOption(this, OptionType.TransparencyQuality, "Transparency Quality", new[] { "Sharp", "Intermediate", "Smooth" }));
			
			items.Add(new MenuOption(this, OptionType.ViewingDistance, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_distance_viewingdistance" }), new[] { "400", "600", "800", "1000", "1500", "2000" }));
			items.Add(new MenuOption(this, OptionType.NearClip, "Near clip", new[] { "0.001", "0.01", "0.05", "0.1", "0.2", "0.5" }));

			if (host == HostApplication.OpenBve)
			{
				items.Add(new MenuOption(this, OptionType.UIScaleFactor, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "ui_scalefactor" }), new[] { "1x", "2x", "3x", "4x", "5x", "6x" }));
				items.Add(new MenuOption(this, OptionType.NumberOfSounds, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "misc_sound_number" }), new[] { "16", "32", "64", "128"}));
				items.Add(new MenuOption(this, OptionType.ShadowQuality, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "shadows_resolution" }), new[]
				{
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_off"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_low"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_medium"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_high"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_ultra"})
				}));
				items.Add(new MenuOption(this, OptionType.ShadowFilterCascades, "Per-cascade culling", new[] { "true", "false" }));
			}

			items.Add(new MenuOption(this, OptionType.NewXParser, "Use New X Parser", new[] { "Original", "NewXParser", "Assimp" }));
			items.Add(new MenuOption(this, OptionType.NewObjParser, "Use New Obj Parser", new[] { "Original", "Assimp" }));

			if (host == HostApplication.ObjectViewer)
			{
				items.Add(new MenuOption(this, OptionType.AutoReloadObjects, "Automatically Reload Objects", new[] { "true", "false" }));
			}
			
			if (host == HostApplication.RouteViewer)
			{
				items.Add(new MenuOption(this, OptionType.ShowLogo, "Show Logo", new[] { "true", "false" }));
				items.Add(new MenuOption(this, OptionType.ShowBackgrounds, "Show Backgrounds", new[] { "true", "false" }));
				items.Add(new MenuOption(this, OptionType.ShowProgressBar, "Show Progress Bar", new[] { "true", "false" }));
			}

			items.Add(new MenuCommand(this, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "back" }), MenuTag.MenuBack, 0));
			return items.ToArray();
		}

		/// <summary>Called when a menu option changes</summary>
		/// <param name="type">The option type that changed</param>
		public virtual void OnOptionChanged(OptionType type)
		{
		}

		/// <summary>Processes raw key down events for numeric option entry</summary>
		/// <param name="key">The keyboard key pressed</param>
		public virtual void ProcessKeyDown(OpenTK.Input.Key key)
		{
			if (CurrMenu < 0 || Menus.Length == 0) return;
			var menu = Menus[CurrMenu];
			if (menu.Selection >= 0 && menu.Selection < menu.Items.Length)
			{
				var selectedEntry = menu.Items[menu.Selection];
				if (selectedEntry is MenuOption opt)
				{
					opt.ProcessKeyDown(key);
				}
			}
		}

		/// <summary>Draws the current menu</summary>
		/// <param name="RealTimeElapsed">The real time elapsed since the last draw call</param>
		/// <remarks>Note that the real time elapsed may be different to the game time elapsed</remarks>
		public abstract void Draw(double RealTimeElapsed);
	}
}
