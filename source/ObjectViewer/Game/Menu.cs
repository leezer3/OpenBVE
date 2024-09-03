﻿using System;
using System.IO;
using LibRender2.Menu;
using LibRender2.Primitives;
using LibRender2.Screens;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Input;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
	public sealed partial class GameMenu: AbstractMenu
	{

		internal Picturebox filePictureBox;
		internal Textbox fileTextBox;
		private double lastTimeElapsed;
		private static string SearchDirectory;
		private static string currentFile;

		/// <summary>Returns the current menu instance (If applicable)</summary>
		public static GameMenu Instance;

		internal GameMenu() : base(Program.Renderer)
		{
		}

		public override void Initialize()
		{
			filePictureBox = new Picturebox(Program.Renderer);
			fileTextBox = new Textbox(Program.Renderer, Program.Renderer.Fonts.NormalFont, Color128.White, Color128.Black);
			Reset();
			// choose the text font size according to screen height
			// the boundaries follow approximately the progression
			// of font sizes defined in Graphics/Fonts.cs
			if (Program.Renderer.Screen.Height <= 512) MenuFont = Program.Renderer.Fonts.SmallFont;
			else if (Program.Renderer.Screen.Height <= 680) MenuFont = Program.Renderer.Fonts.NormalFont;
			else if (Program.Renderer.Screen.Height <= 890) MenuFont = Program.Renderer.Fonts.LargeFont;
			else if (Program.Renderer.Screen.Height <= 1150) MenuFont = Program.Renderer.Fonts.VeryLargeFont;
			else MenuFont = Program.Renderer.Fonts.EvenLargerFont;

			lineHeight = (int)(MenuFont.FontSize * LineSpacing);
			MenuBackKey = Key.Escape; // fixed in viewers
			int quarterWidth = (int)(Program.Renderer.Screen.Width / 4.0);
			int quarterHeight = (int)(Program.Renderer.Screen.Height / 4.0);
			int descriptionLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 2;
			int descriptionWidth = quarterWidth + quarterWidth / 2;
			int descriptionHeight = descriptionWidth;
			if (descriptionHeight + quarterWidth > Program.Renderer.Screen.Height - 50)
			{
				descriptionHeight = Program.Renderer.Screen.Height - quarterWidth - 50;
			}
			fileTextBox.Location = new Vector2(descriptionLoc, quarterWidth);
			fileTextBox.Size = new Vector2(descriptionWidth, descriptionHeight);
			int imageLoc = Program.Renderer.Screen.Width - quarterWidth - quarterWidth / 4;
			filePictureBox.Location = new Vector2(imageLoc, 0);
			filePictureBox.Size = new Vector2(quarterWidth, quarterWidth);
			filePictureBox.BackgroundColor = Color128.White;
			SearchDirectory = Interface.CurrentOptions.ObjectSearchDirectory;
			IsInitialized = true;
		}

		public override void PushMenu(MenuType type, int data = 0, bool replace = false)
		{
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu)
			{
				// Deliberately set to the standard cursor, as touch controls may have set to something else
				Program.Renderer.SetCursor(OpenTK.MouseCursor.Default);
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
			Menus[CurrMenu] = new SingleMenu(type, data, MaxWidth);
			if (replace)
			{
				Menus[CurrMenu].Selection = 1;
			}
			ComputePosition();
			Program.Renderer.CurrentInterface = InterfaceType.Menu;
		}

		public override void ProcessCommand(Translations.Command cmd, double timeElapsed)
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
					Reset();
					Program.Renderer.CurrentInterface = InterfaceType.Normal;
				}
				else
				{
					PopMenu();
				}
				return;
			}
			if (menu.Selection == -1)    // if menu has no selection, do nothing
				return;
			switch (cmd)
			{
				case Translations.Command.MenuUp: // UP
					if (menu.Selection > 0 &&
					    !(menu.Items[menu.Selection - 1] is MenuCaption))
					{
						menu.Selection--;
						ComputePosition();
					}

					break;
				case Translations.Command.MenuDown: // DOWN
					if (menu.Selection < menu.Items.Length - 1)
					{
						menu.Selection++;
						ComputePosition();
					}

					break;
				//			case Translations.Command.MenuBack:	// ESC:	managed above
				//				break;
				case Translations.Command.MenuEnter: // ENTER
					if (menu.Items[menu.Selection] is MenuCommand menuItem)
					{
						switch (menuItem.Tag)
						{
							// menu management commands
							case MenuTag.MenuBack: // BACK TO PREVIOUS MENU
								GameMenu.Instance.PopMenu();
								break;
							case MenuTag.MenuJumpToStation: // TO STATIONS MENU
								GameMenu.Instance.PushMenu(MenuType.JumpToStation);
								break;
							case MenuTag.MenuExitToMainMenu: // TO EXIT MENU
								GameMenu.Instance.PushMenu(MenuType.ExitToMainMenu);
								break;
							case MenuTag.MenuQuit: // TO QUIT MENU
								GameMenu.Instance.PushMenu(MenuType.Quit);
								break;
							case MenuTag.MenuControls: // TO CONTROLS MENU
								GameMenu.Instance.PushMenu(MenuType.Controls);
								break;
							case MenuTag.BackToSim: // OUT OF MENU BACK TO SIMULATION
								Reset();
								Program.Renderer.CurrentInterface = InterfaceType.Normal;
								break;
							case MenuTag.ObjectList:             // TO OBJECT LIST MENU
								GameMenu.Instance.PushMenu(MenuType.ObjectList);
								fileTextBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "errors", "route_please_select" });
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), new TextureParameters(null, null), out filePictureBox.Texture);
								break;
							case MenuTag.Directory:     // SHOWS THE LIST OF FILES IN THE SELECTED DIR
								SearchDirectory = SearchDirectory == string.Empty ? menu.Items[menu.Selection].Text : Path.CombineDirectory(SearchDirectory, menu.Items[menu.Selection].Text);
								GameMenu.Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.ParentDirectory:       // SHOWS THE LIST OF FILES IN THE PARENT DIR
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
								GameMenu.Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.ObjectFile:
								currentFile = Path.CombineFile(SearchDirectory, menu.Items[menu.Selection].Text);
								Program.Files.Add(currentFile);
								Game.Reset();
								Program.RefreshObjects();
								Reset();
								Program.Renderer.CurrentInterface = InterfaceType.Normal;
								Interface.CurrentOptions.ObjectSearchDirectory = SearchDirectory;
								break;
						}
					}
					break;
			}
		
		}

		public override bool ProcessMouseMove(int x, int y)
		{
			Program.currentGameWindow.CursorVisible = true;
			if (CurrMenu < 0)
			{
				return false;
			}
			// if not in menu or during control customisation or down outside menu area, do nothing
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu)
				return false;

			// Load the current menu
			MenuBase menu = Menus[CurrMenu];
			if (menu.TopItem > 1 && y < topItemY && y > menuMin.Y)
			{
				//Item is the scroll up ellipsis
				menu.Selection = menu.TopItem - 1;
				return true;
			}
			if (menu.Type == MenuType.RouteList || menu.Type == MenuType.TrainList || menu.Type == MenuType.PackageInstall || menu.Type == MenuType.Packages || (int)menu.Type >= 107)
			{
				fileTextBox.MouseMove(x, y);
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

			int item = (int)((y - topItemY) / lineHeight + menu.TopItem);
			// if the mouse is above a command item, select it
			if (item >= 0 && item < menu.Items.Length && menu.Items[item] is MenuCommand /* || menu.Items[item] is MenuOption) */)
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

		public override void Draw(double RealTimeElapsed)
		{
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
			if (menu.Selection == menu.TopItem - 1)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, menuMin.Y /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + ItemBorder.X, MenuFont.FontSize + ItemBorder.Y * 2), highlightColor);
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
					if (menu.Items[i] is MenuCommand command)
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
						Program.Renderer.Rectangle.Draw(null, new Vector2(ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.Width + 2.0f * ItemBorder.X, MenuFont.FontSize + ItemBorder.Y * 2), color);
					}
					else
					{
						Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + 2.0f * ItemBorder.X, MenuFont.FontSize + ItemBorder.Y * 2), color);
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
//				if (menu.Items[i] is MenuOption opt)
//				{
//					Program.Renderer.OpenGlString.Draw(MenuFont, opt.CurrentOption.ToString(), new Vector2((menuMax.X - menuMin.X + 2.0f * Border.X) + 4.0f, itemY),
//						menu.Align, backgroundColor, false);
//				}
				itemY += lineHeight;
				if (menu.Items[i].Icon != null)
				{
					Program.Renderer.Rectangle.DrawAlpha(menu.Items[i].Icon, new Vector2(iconX, itemY - itemHeight * 1.5), new Vector2(itemHeight, itemHeight), Color128.White);
					itemX = iconX;
				}

			}


			if (menu.Selection == menu.TopItem + visibleItems)
			{
				Program.Renderer.Rectangle.Draw(null, new Vector2(itemLeft - ItemBorder.X, itemY /*-ItemBorder.Y*/), new Vector2(menu.ItemWidth + 2.0f * ItemBorder.X, MenuFont.FontSize + ItemBorder.Y * 2), highlightColor);
			}
			// if not at the end of the menu, draw a dimmed ellipsis item at the bottom
			if (i < menu.Items.Length - 1)
				Program.Renderer.OpenGlString.Draw(MenuFont, @"...", new Vector2(itemX, itemY),
					menu.Align, ColourDimmed, false);
		}
	}
}
