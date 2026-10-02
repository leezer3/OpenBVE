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
	/// <summary>Implements the in-game menu system; manages addition and removal of individual menus.</summary>
	public sealed partial class GameMenu : AbstractMenu
	{
		private double pluginKeepAliveTimer;

		private ControlMethod lastControlMethod;

		public override void Draw(double RealTimeElapsed)
		{
			Renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Renderer.Screen.Width, Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Renderer.CurrentProjectionMatrix);
			Renderer.PushMatrix(MatrixMode.Modelview);
			Renderer.CurrentViewMatrix = Matrix4D.Identity;
			pluginKeepAliveTimer += RealTimeElapsed;
			if (pluginKeepAliveTimer > 100000 && TrainManagerBase.PlayerTrain != null && TrainManagerBase.PlayerTrain.Plugin != null)
			{
				TrainManagerBase.PlayerTrain.Plugin.KeepAlive();
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
				if (menu.Items[i] is MenuOption opt)
				{
					Program.Renderer.OpenGlString.Draw(MenuFont, opt.CurrentOption.ToString(), new Vector2((menuMax.X - menuMin.X + 2.0f * Border.X) + 4.0f, itemY),
						menu.Align, backgroundColor, false);
				}
				itemY += LineHeight;
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
					nextImageButton.Draw();
					previousImageButton.Draw();
					routePictureBox.Draw();
					routeDescriptionBox.Draw();
					nextStepButton.Text = menu.Type == MenuType.RouteList ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "start", "train_choose" }) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "start", "start_start" });
					nextStepButton.IsVisible = true;
					nextStepButton.Draw();

					//Choose Train and Game start button
					nextStepButton.Enabled = (menu.Type == MenuType.RouteList && RoutefileState == RouteState.Processed) || (menu.Type == MenuType.TrainList && Interface.CurrentOptions.TrainFolder != string.Empty);
						break;
				}
				case MenuType.PackageInstall:
					routePictureBox.Draw();
					routeDescriptionBox.Draw();
					nextStepButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "packages", "install_button" });
					nextStepButton.IsVisible = true;
					nextStepButton.Enabled = currentPackage != null;
					nextStepButton.Draw();
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
					nextStepButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "packages", "uninstall_button" });
					nextStepButton.IsVisible = true;
					nextStepButton.Enabled = currentPackage != null;
					nextStepButton.Draw();
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
											controlPictureBox.Texture = Program.Renderer.MasconTexture;
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
			Renderer.PopMatrix(MatrixMode.Modelview);
			Renderer.PopMatrix(MatrixMode.Projection);
		}
	}
}
