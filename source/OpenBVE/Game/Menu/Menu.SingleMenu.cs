using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using DavyKager;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Packages;
using OpenBveApi.Textures;
using TrainManager;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	public sealed partial class Menu
	{
		/// <summary>Describes a single menu of the menu stack.
		/// The class is private to Menu, but all its fields are public to allow 'quick-and-dirty'
		/// access from Menu itself.</summary>
		private class SingleMenu
		{
			/// <summary>The text alignment for the menu</summary>
			public readonly TextAlignment Align;
			/// <summary>The list of items to be shown</summary>
			public readonly MenuEntry[] Items = { };
			/// <summary>The smaller of the width of the largest item, and the absolute width</summary>
			public readonly double ItemWidth = 0;
			/// <summary>The absolute width</summary>
			public readonly double Width = 0;
			/// <summary>The absolute height</summary>
			public readonly double Height = 0;
			/// <summary>The previous menu selection</summary>
			internal int LastSelection = int.MaxValue;

			private int currentSelection;
			
			public int Selection
			{
				get
				{
					return currentSelection;
				}
				set
				{
					LastSelection = currentSelection;
					currentSelection = value;
					if (currentSelection != LastSelection && Interface.CurrentOptions.ScreenReaderAvailable)
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
			internal readonly MenuType Type;
			

			/********************
				MENU C'TOR
			*********************/
			public SingleMenu(MenuType menuType, int data = 0, double MaxWidth = 0)
			{
				Type = menuType;
				int i;
				int jump = 0;
				Align = TextAlignment.TopMiddle;
				Height = Width = 0;
				Selection = 0;                      // defaults to first menu item
				switch (menuType)
				{
					case MenuType.GameStart:          // top level menu
						if (routeWorkerThread == null)
						{
							//Create the worker threads on first launch of main menu
							routeWorkerThread = new BackgroundWorker();
							routeWorkerThread.DoWork += routeWorkerThread_doWork;
							routeWorkerThread.RunWorkerCompleted += routeWorkerThread_completed;
							packageWorkerThread = new BackgroundWorker();
							packageWorkerThread.DoWork += packageWorkerThread_doWork;
							packageWorkerThread.RunWorkerCompleted += packageWorkerThread_completed;
							Manipulation.ProgressChanged += OnWorkerProgressChanged;
							Manipulation.ProblemReport += OnWorkerReportsProblem;
							Manipulation.OperationCompleted += OnPackageOperationCompleted;
							//Load texture
							Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\loading.png"), new TextureParameters(null, null), out routePictureBox.Texture);
						}
						Items = new MenuEntry[5];
						Items[0] = new MenuCommand("Open Route File", MenuTag.RouteList, 0);
						
						if (!Interface.CurrentOptions.KioskMode)
						{
							//Don't allow quitting or customisation of the controls in kiosk mode
							Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","title"}), MenuTag.Options, 0);
							Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","customize_controls"}), MenuTag.MenuControls, 0);
							Items[3] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","title"}), MenuTag.Packages, 0);
							Items[4] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","quit"}), MenuTag.MenuQuit, 0);
						}
						else
						{
							Array.Resize(ref Items, Items.Length - 3);
						}
						SearchDirectory = Program.FileSystem.InitialRouteFolder;
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.Packages:
						Items = new MenuEntry[4];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","title"}));
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_header"}), MenuTag.PackageInstall, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","uninstall_button"}), MenuTag.PackageUninstall, 0);
						Items[3] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_cancel"}), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.PackageUninstall:
						routeDescriptionBox.Text = string.Empty;
						Items = new MenuEntry[5];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_type"}));
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_route"}), MenuTag.UninstallRoute, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_train"}), MenuTag.UninstallTrain, 0);
						Items[3] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_other"}), MenuTag.UninstallOther, 0);
						Items[4] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_cancel"}), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.UninstallRoute:
						Items = new MenuEntry[Database.currentDatabase.InstalledRoutes.Count + 1];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"}));
						for (int j = 0; j < Database.currentDatabase.InstalledRoutes.Count; j++)
						{
							Items[j + 1] = new MenuCommand(Database.currentDatabase.InstalledRoutes[j].Name, MenuTag.Package, Database.currentDatabase.InstalledRoutes[j]);
						}
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.UninstallTrain:
						Items = new MenuEntry[Database.currentDatabase.InstalledTrains.Count + 1];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"}));
						for (int j = 0; j < Database.currentDatabase.InstalledTrains.Count; j++)
						{
							Items[j + 1] = new MenuCommand(Database.currentDatabase.InstalledTrains[j].Name, MenuTag.Package, Database.currentDatabase.InstalledTrains[j]);
						}
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.UninstallOther:
						Items = new MenuEntry[Database.currentDatabase.InstalledOther.Count + 1];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"}));
						for (int j = 0; j < Database.currentDatabase.InstalledOther.Count; j++)
						{
							Items[j + 1] = new MenuCommand(Database.currentDatabase.InstalledOther[j].Name, MenuTag.Package, Database.currentDatabase.InstalledOther[j]);
						}
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.PackageInstall:
						string[] potentialFiles = { };
						string[] directoryList = { };
						bool drives = false;
						if (SearchDirectory != string.Empty)
						{
							try
							{
								potentialFiles = Directory.GetFiles(SearchDirectory);
								directoryList = Directory.GetDirectories(SearchDirectory);
							}
							catch
							{
								// Ignored
							}
						}
						else
						{
							DriveInfo[] systemDrives = DriveInfo.GetDrives();
							directoryList = new string[systemDrives.Length];
							for (int k = 0; k < systemDrives.Length; k++)
							{
								directoryList[k] = systemDrives[k].Name;
							}
							drives = true;
						}
						
						Items = new MenuEntry[potentialFiles.Length + directoryList.Length + 2];
						Items[0] = new MenuCaption(SearchDirectory);
						Items[1] = new MenuCommand("...", MenuTag.ParentDirectory, 0);
						int totalEntries = 2;
						for (int j = 0; j < directoryList.Length; j++)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(directoryList[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && directoryInfo.Name[0] == '.')
							{
								continue;
							}
							Items[totalEntries] = new MenuCommand(directoryInfo.Name, MenuTag.Directory, 0);
							if (drives)
							{
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_disk.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
							}
							else
							{
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_folder.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);	
							}
							
							totalEntries++;
						}

						for (int j = 0; j < potentialFiles.Length; j++)
						{
							string fileName = System.IO.Path.GetFileName(potentialFiles[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && fileName[0] == '.')
							{
								continue;
							}
							Items[totalEntries] = new MenuCommand(fileName, MenuTag.File, 0);
							string ext = System.IO.Path.GetExtension(fileName);
							if (!iconCache.ContainsKey(ext))
							{
								// As some people have used arbritary extensions for packages, let's show all files
								// Try and pull out the default icon from the cache for something a little nicer looking
								try
								{
									Icon icon = Icon.ExtractAssociatedIcon(potentialFiles[j]);
									if (icon != null)
									{
										Program.CurrentHost.RegisterTexture(icon.ToBitmap(), new TextureParameters(null, null), out Texture t);
										iconCache.Add(ext, t);
										Items[totalEntries].Icon = t;
									}
								}
								catch
								{
									// Ignored
								}
								
							}
							else
							{
								Items[totalEntries].Icon = iconCache[ext];
							}
							totalEntries++;
						}
						Array.Resize(ref Items, totalEntries);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.Options:
						Items = new MenuEntry[8];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","options"}));
						Items[1] = new MenuOption(MenuOptionType.ScreenResolution, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","resolution"}), Program.Renderer.Screen.AvailableResolutions.ToArray());
						Items[2] = new MenuOption(MenuOptionType.FullScreen, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_mode_fullscreen"}), new[] { "true", "false" });
						Items[3] = new MenuOption(MenuOptionType.Interpolation, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation"}), new[]
						{
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearest"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinear"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearestmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinearmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_trilinearmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_anisotropic"})
						});
						Items[4] = new MenuOption(MenuOptionType.AnisotropicLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_anisotropic_level"}), new[] { "0", "2", "4", "8", "16" });
						Items[5] = new MenuOption(MenuOptionType.AntialiasingLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_antialiasing_level"}), new[] { "0", "2", "4", "8", "16" });
						Items[6] = new MenuOption(MenuOptionType.ViewingDistance, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_viewingdistance"}), new[] { "400", "600", "800", "1000", "1500", "2000" });
						Items[7] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","back"}), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.RouteList:
						potentialFiles = new string[] { };
						directoryList = new string[] { };
						drives = false;
						if (SearchDirectory != string.Empty)
						{
							try
							{
								potentialFiles = Directory.GetFiles(SearchDirectory);
								directoryList = Directory.GetDirectories(SearchDirectory);
							}
							catch
							{
								// Ignored
							}
						}
						else
						{
							DriveInfo[] systemDrives = DriveInfo.GetDrives();
							directoryList = new string[systemDrives.Length];
							for (int k = 0; k < systemDrives.Length; k++)
							{
								directoryList[k] = systemDrives[k].Name;
							}
							drives = true;
						}
						
						Items = new MenuEntry[potentialFiles.Length + directoryList.Length + 2];
						Items[0] = new MenuCaption(SearchDirectory);
						Items[1] = new MenuCommand("...", MenuTag.ParentDirectory, 0);
						totalEntries = 2;
						for (int j = 0; j < directoryList.Length; j++)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(directoryList[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && directoryInfo.Name[0] == '.')
							{
								continue;
							}
							Items[totalEntries] = new MenuCommand(directoryInfo.Name, MenuTag.Directory, 0);
							if (drives)
							{
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_disk.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
							}
							else
							{
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_folder.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);	
							}
							
							totalEntries++;
						}

						for (int j = 0; j < potentialFiles.Length; j++)
						{
							string fileName = System.IO.Path.GetFileName(potentialFiles[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && fileName[0] == '.')
							{
								continue;
							}
							if (fileName.ToLowerInvariant().EndsWith(".csv") || fileName.ToLowerInvariant().EndsWith(".rw"))
							{
								Items[totalEntries] = new MenuCommand(fileName, MenuTag.RouteFile, 0);
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_route.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
								totalEntries++;
							}
						}
						Array.Resize(ref Items, totalEntries);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.TrainList:
						potentialFiles = new string[] { };
						directoryList = new string[] { };
						drives = false;
						if (SearchDirectory != string.Empty)
						{
							try
							{
								potentialFiles = Directory.GetFiles(SearchDirectory);
								directoryList = Directory.GetDirectories(SearchDirectory);
							}
							catch
							{
								// Ignored
							}
						}
						else
						{
							DriveInfo[] systemDrives = DriveInfo.GetDrives();
							directoryList = new string[systemDrives.Length];
							for (int k = 0; k < systemDrives.Length; k++)
							{
								directoryList[k] = systemDrives[k].Name;
							}
							drives = true;
						}
						
						Items = new MenuEntry[potentialFiles.Length + directoryList.Length + 2];
						Items[0] = new MenuCaption(SearchDirectory);
						Items[1] = new MenuCommand("...", MenuTag.ParentDirectory, 0);
						totalEntries = 2;
						for (int j = 0; j < directoryList.Length; j++)
						{
							bool isTrain = false;
							for (int k = 0; k < Program.CurrentHost.Plugins.Length; k++)
							{
								if (Program.CurrentHost.Plugins[k].Train != null && Program.CurrentHost.Plugins[k].Train.CanLoadTrain(directoryList[j]))
								{
									isTrain = true;
									break;
								}
							}
							DirectoryInfo directoryInfo = new DirectoryInfo(directoryList[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && directoryInfo.Name[0] == '.')
							{
								continue;
							}
							if (!isTrain)
							{
								Items[totalEntries] = new MenuCommand(directoryInfo.Name, MenuTag.Directory, 0);
								if (drives)
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_disk.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
								}
								else
								{
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_folder.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);	
								}
							}
							else
							{
								Items[totalEntries] = new MenuCommand(directoryInfo.Name, MenuTag.TrainDirectory, 0);
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_train.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);	
							}
							totalEntries++;
						}
						Array.Resize(ref Items, totalEntries);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.Top:          // top level menu
						if (Interface.CurrentOptions.ScreenReaderAvailable)
						{
							if (!Tolk.Output(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","title"})))
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
						Items[0] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","resume"}), MenuTag.BackToSim, 0);
						if (jump > 0)
							Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","jump"}), MenuTag.MenuJumpToStation, 0);
						if (!Interface.CurrentOptions.KioskMode)
						{
							//Don't allow quitting or customisation of the controls in kiosk mode
							Items[1 + jump] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","exit"}), MenuTag.MenuExitToMainMenu, 0);
							Items[2 + jump] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","customize_controls"}), MenuTag.MenuControls, 0);
							Items[3 + jump] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","quit"}), MenuTag.MenuQuit, 0);
						}
						else
						{
							Array.Resize(ref Items, Items.Length -3);
						}
						break;
					case MenuType.JumpToStation:    // list of stations to jump to
													// count the number of available stations
						int menuItem = 0;
						for (i = 0; i < Program.CurrentRoute.Stations.Length; i++)
							if (Program.CurrentRoute.Stations[i].PlayerStops() & Program.CurrentRoute.Stations[i].Stops.Length > 0)
								menuItem++;
						// list available stations, selecting the next station as predefined choice
						jump = 0;                           // no jump found yet
						Items = new MenuEntry[menuItem + 1];
						Items[0] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","back"}), MenuTag.MenuBack, 0);
						menuItem = 1;
						for (i = 0; i < Program.CurrentRoute.Stations.Length; i++)
							if (Program.CurrentRoute.Stations[i].PlayerStops() & Program.CurrentRoute.Stations[i].Stops.Length > 0)
							{
								Items[menuItem] = new MenuCommand(Program.CurrentRoute.Stations[i].Name, MenuTag.JumpToStation, i);
								// if no preferred jump-to-station found yet and this station is
								// after the last station the user stopped at, select this item
								if (jump == 0 && i > TrainManagerBase.PlayerTrain.LastStation)
								{
									jump = i;
									Selection = menuItem;
								}
								menuItem++;
							}
						break;

					case MenuType.ExitToMainMenu:
						Items = new MenuEntry[3];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","exit_question"}));
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","exit_no"}), MenuTag.MenuBack, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","exit_yes"}), MenuTag.ExitToMainMenu, 0);
						Selection = 1;
						break;

					case MenuType.Quit:         // ask for quit confirmation
						Items = new MenuEntry[3];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","quit_question"}));
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","quit_no"}), MenuTag.MenuBack, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","quit_yes"}), MenuTag.Quit, 0);
						Selection = 1;
						break;

					case MenuType.Controls:
						//Refresh the joystick list
						Program.Joysticks.RefreshJoysticks();
						Items = new MenuEntry[Interface.CurrentControls.Length + 2];
						Items[0] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","back"}), MenuTag.MenuBack, 0);
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","reset"}), MenuTag.ControlReset, 0);
						int ci = 2;
						for (i = 0; i < Interface.CurrentControls.Length; i++)
						{
							if (Interface.CurrentControls[i].Command != Translations.Command.None)
							{
								Items[ci] = new MenuCommand(Interface.CurrentControls[i].Command.ToString(), MenuTag.Control, i);
								ci++;
							}
						}
						Array.Resize(ref Items, ci);
						if (Instance.Menus[0].Type == MenuType.GameStart)
						{
							// If the first menu in the current stack is the GL game menu, use left-align
							Align = TextAlignment.TopLeft;
						}
						break;

					case MenuType.Control:
						//Refresh the joystick list
						Program.Joysticks.RefreshJoysticks();
						Selection = SelectionNone;
						Items = new MenuEntry[4];
						// get code name and description
						Control loadedControl = Interface.CurrentControls[data];
						Items[0] = new MenuCommand(loadedControl.Command + " - " +
						                           Translations.newCommandInfos[loadedControl.Command].Description, MenuTag.None, 0);
						// get assignment
						String str = "";
						switch (loadedControl.Method)
						{
							case ControlMethod.Keyboard:
								string keyName = loadedControl.Key.ToString();
								if (Translations.TranslatedKeys.ContainsKey(loadedControl.Key))
								{
									keyName = Translations.TranslatedKeys[loadedControl.Key].Description;
								}
								if (loadedControl.Modifier != KeyboardModifier.None)
								{
									str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","keyboard"}) + " [" + loadedControl.Modifier + "-" + keyName + "]";
								}
								else
								{
									str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","keyboard"}) + " [" + keyName + "]";
								}
								break;
							case ControlMethod.Joystick:
								str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystick"}) + " " + loadedControl.Device + " [" + loadedControl.Component + " " + loadedControl.Element + "]";
								switch (loadedControl.Component)
								{
									case JoystickComponent.FullAxis:
									case JoystickComponent.Axis:
										str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystickdirection_positive"}) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystickdirection_negative"}));
										break;
									//						case Interface.JoystickComponent.Button:	// NOTHING TO DO FOR THIS CASE!
									//							str = str;
									//							break;
									case JoystickComponent.Hat:
										str += " " + (OpenTK.Input.HatPosition)loadedControl.Direction;
										break;
									case JoystickComponent.Invalid:
										str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystick_notavailable"});
										break;
								}
								break;
							case ControlMethod.RailDriver:
								str = "RailDriver [" + loadedControl.Component + " " + loadedControl.Element + "]";
								switch (loadedControl.Component)
								{
									case JoystickComponent.FullAxis:
									case JoystickComponent.Axis:
										str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystickdirection_positive"}) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystickdirection_negative"}));
										break;
									case JoystickComponent.Invalid:
										str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystick_notavailable"});
										break;
								}
								break;
							case ControlMethod.Invalid:
								str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","joystick_notavailable"});
								break;
						}
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","assignment_current"}) + " " + str, MenuTag.None, 0);
						Items[2] = new MenuCommand(" ", MenuTag.None, 0);
						Items[3] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","assign"}), MenuTag.None, 0);
						break;
					case MenuType.ControlReset:
						Items = new MenuEntry[3];
						Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","reset_question"}));
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","train_default_yes"}), MenuTag.Yes, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","train_default_no"}), MenuTag.No, 0);
						Selection = 1;
						break;
					case MenuType.TrainDefault:
						Interface.CurrentOptions.TrainFolder = Loading.GetDefaultTrainFolder(currentFile);
						bool canLoad = false;
						for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
						{
							if (Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(Interface.CurrentOptions.TrainFolder))
							{
								canLoad = true;
								break;
							}
						}

						if (canLoad)
						{
							Items = new MenuEntry[3];
							Items[0] = new MenuCaption(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","train_default"}));
							Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","train_default_yes"}), MenuTag.Yes, 0);
							Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"menu","train_default_no"}), MenuTag.No, 0);
							Selection = 1;
						}
						else
						{
							SearchDirectory = Program.FileSystem.InitialTrainFolder;
							//Default train not found or not valid
							Instance.PushMenu(MenuType.TrainList);
						}
						break;
				}
				// compute menu extent
				for (i = 0; i < Items.Length; i++)
				{
					if (Items[i] == null)
					{
						continue;
					}
					Vector2 size = Game.Menu.MenuFont.MeasureString(Items[i].Text);
					if (Items[i].Icon != null)
					{
						size.X += size.Y * 1.25;
					}
					if (size.X > Width)
					{
						Width = size.X;
					}
					
					if (MaxWidth != 0 && size.X > MaxWidth)
					{
						for (int j = Items[i].Text.Length - 1; j > 0; j--)
						{
							string trimmedText = Items[i].Text.Substring(0, j);
							size = Game.Menu.MenuFont.MeasureString(trimmedText);
							double mwi = MaxWidth;
							if (Items[i].Icon != null)
							{
								mwi -= size.Y * 1.25;
							}
							if (size.X < mwi)
							{
								Items[i].DisplayLength = trimmedText.Length;
								break;
							}
						}
						Width = MaxWidth;
					}
					if (!(Items[i] is MenuCaption && menuType!= MenuType.RouteList && menuType != MenuType.GameStart && menuType != MenuType.Packages) && size.X > ItemWidth)
						ItemWidth = size.X;
				}
				Height = Items.Length * Game.Menu.LineHeight;
				TopItem = 0;
			}

		}
	}
}
