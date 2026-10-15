using OpenBveApi.Interface;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using LibRender2.Menu;
using LibRender2.Screens;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Packages;
using OpenBveApi.Textures;
using TrainManager;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	/// <summary>Implements the in-game menu system; manages addition and removal of individual menus.</summary>
	public sealed partial class GameMenu : AbstractMenu
	{
		/// <summary>Processes a user command for the current menu</summary>
		/// <param name="cmd">The command to apply to the current menu</param>
		/// <param name="timeElapsed">The time elapsed since previous frame</param>
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
					Instance.PushMenu(MenuType.Quit);
				}
				else
				{
					if (!string.IsNullOrEmpty(PreviousSearchDirectory))
					{
						SearchDirectory = PreviousSearchDirectory;
					}
					PopMenu();	
				}
				return;
			}
			
			if (menu.Selection == SelectionNone)    // if menu has no selection, do nothing
				return;
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
								if (Menus[CurrMenu].Type == MenuType.Options)
								{
									Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg"));
									HUD.LoadHUD();
									Program.FileSystem.SaveCurrentFileSystemConfiguration();
									// re-position our stuff in case of screen resolution change
									
								}
								Instance.PopMenu();
								OnResize();
								Menus[CurrMenu].ComputeExtent(Menus[CurrMenu].Type, MenuFont, 0, LineHeight);
								ComputePosition();
								break;
							case MenuTag.MenuJumpToStation:     // TO STATIONS MENU
								Instance.PushMenu(MenuType.JumpToStation);
								break;
							case MenuTag.MenuExitToMainMenu:    // TO EXIT MENU
								Instance.PushMenu(MenuType.ExitToMainMenu);
								break;
							case MenuTag.MenuQuit:              // TO QUIT MENU
								Instance.PushMenu(MenuType.Quit);
								break;
							case MenuTag.MenuControls:          // TO CONTROLS MENU
								Instance.PushMenu(MenuType.Controls);
								break;
							case MenuTag.MenuTools:          // TO CONTROLS MENU
								Instance.PushMenu(MenuType.Tools);
								break;
							case MenuTag.BackToSim:             // OUT OF MENU BACK TO SIMULATION
								Reset();
								Program.Renderer.CurrentInterface = InterfaceType.Normal;
								break;
							case MenuTag.Packages:
								if (Database.LoadDatabase(Program.FileSystem.PackageDatabaseFolder, currentDatabaseFile, out _))
								{
									Instance.PushMenu(MenuType.Packages);
								}
								
								break;
							// route menu commands
							case MenuTag.PackageInstall:
								currentOperation = PackageOperation.Installing;
								packagePreview = true;
								Instance.PushMenu(MenuType.PackageInstall);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\package.png"), TextureParameters.NoChange, out routePictureBox.Texture);
								break;
							case MenuTag.PackageUninstall:
								currentOperation = PackageOperation.Uninstalling;
								Instance.PushMenu(MenuType.PackageUninstall);
								break;
							case MenuTag.UninstallRoute:
								if (Database.currentDatabase.InstalledRoutes.Count == 0)
								{
									return;
								}
								Instance.PushMenu(MenuType.UninstallRoute);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), TextureParameters.NoChange, out routePictureBox.Texture);
								break;
							case MenuTag.UninstallTrain:
								if (Database.currentDatabase.InstalledTrains.Count == 0)
								{
									return;
								}
								Instance.PushMenu(MenuType.UninstallTrain);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), TextureParameters.NoChange, out routePictureBox.Texture);
								break;
							case MenuTag.UninstallOther:
								if (Database.currentDatabase.InstalledOther.Count == 0)
								{
									return;
								}
								Instance.PushMenu(MenuType.UninstallOther);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selection_none"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), TextureParameters.NoChange, out routePictureBox.Texture);
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
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\package.png"), TextureParameters.NoChange, out routePictureBox.Texture);		
								}
								break;
							case MenuTag.Options:
								Instance.PushMenu(MenuType.Options);
								break;
							case MenuTag.Tools:
								Instance.PushMenu(MenuType.Tools);
								break;
							case MenuTag.RouteList:				// TO ROUTE LIST MENU
								Instance.PushMenu(MenuType.RouteList);
								routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","route_please_select"});
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), TextureParameters.NoChange, out routePictureBox.Texture);	
								break;
							case MenuTag.Directory:		// SHOWS THE LIST OF FILES IN THE SELECTED DIR
								SearchDirectory = SearchDirectory == string.Empty ? menu.Items[menu.Selection].Text : Path.CombineDirectory(SearchDirectory, menu.Items[menu.Selection].Text);
								Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
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
								Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
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
											Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
										}
										else
										{
											//Show details
											Interface.CurrentOptions.TrainFolder = trainDir;
											routeDescriptionBox.Text = Program.CurrentHost.Plugins[i].Train.GetDescription(trainDir);
											string trainImage = Program.CurrentHost.Plugins[i].Train.GetImage(trainDir);
											if (!string.IsNullOrEmpty(trainImage))
											{
												Program.CurrentHost.RegisterTexture(trainImage, TextureParameters.NoChange, out routePictureBox.Texture);
											}
											else
											{
												Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\train_unknown.png"), TextureParameters.NoChange, out routePictureBox.Texture);
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
								MainLoop.Quit = QuitMode.ExitToMenu;
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
								MainLoop.Quit = QuitMode.QuitProgram;
								break;
							case MenuTag.Yes:
								switch (menu.Type)
								{
									case MenuType.TrainDefault:
										Reset();
										//Launch the game!
										Loading.Complete = false;
										Loading.LoadAsynchronously(currentFile, Encoding.UTF8, Interface.CurrentOptions.TrainFolder, Encoding.UTF8);
										OpenBVEGame g = Program.Renderer.GameWindow as OpenBVEGame;
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
										Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\please_select.png"), TextureParameters.NoChange, out routePictureBox.Texture);
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
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "In-Game\\Switch-L.png"), TextureParameters.NoChange, out switchSettingPictureBox.Texture);
								}
								else
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "In-Game\\Switch-R.png"), TextureParameters.NoChange, out switchSettingPictureBox.Texture);
								}

								menu.Items[2].Text = "Current Setting: " + Program.CurrentRoute.Switches[switchToToggle].CurrentlySetTrack;
								switchesFound = false; // as switch has been toggled, need to recalculate switches along route
								Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.PreviousSwitch:
								FoundSwitch fs = previousSwitches[0];
								previousSwitches.RemoveAt(0);
								nextSwitches.Insert(0, fs);
								Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.NextSwitch:
								FoundSwitch ns = nextSwitches[0];
								nextSwitches.RemoveAt(0);
								previousSwitches.Insert(0, ns);
								Instance.PushMenu(Instance.Menus[CurrMenu].Type, 0, true);
								break;
							case MenuTag.ObjectViewer:
								string dir = AppDomain.CurrentDomain.BaseDirectory;
								string runCmd = Path.CombineFile(dir, "ObjectViewer.exe");

								if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows)
								{
									Process.Start("mono", runCmd);
								}
								else
								{
									Process.Start(runCmd);
								}
								break;
							case MenuTag.RouteViewer:
								dir = AppDomain.CurrentDomain.BaseDirectory;
								runCmd = Path.CombineFile(dir, "RouteViewer.exe");

								if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows)
								{
									Process.Start("mono", runCmd);
								}
								else
								{
									Process.Start(runCmd);
								}
								break;
							case MenuTag.ViewLog:
								try
								{
									var file = Path.CombineFile(Program.FileSystem.SettingsFolder, "log.txt");

									if (File.Exists(file))
									{
										Process.Start(file);
									}
									else
									{
										PushMenu(MenuType.Error);
									}
								}
								catch
								{
									PushMenu(MenuType.Error);
									// Actually failed to load, but same difference
								}
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
					Program.Sounds.Update(timeElapsed);
					break;
			}
		}
	}
}
