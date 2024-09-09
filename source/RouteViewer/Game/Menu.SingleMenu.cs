using LibRender2.Menu;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Textures;
using System;
using System.IO;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;
using RouteViewer;

namespace RouteViewer
{
	public sealed partial class GameMenu
	{
		/// <summary>Provides implementation for a single menu of the menu stack.</summary>
		/// <remarks>The class is private to Menu, but all its fields are public to allow 'quick-and-dirty'
		/// access from Menu itself.</remarks>
		private class SingleMenu : MenuBase
		{
			public SingleMenu(AbstractMenu menu, MenuType menuType, int data = 0, double MaxWidth = 0) : base(menuType)
			{
				int i;
				int jump = 0;
				//Vector2 size;
				Align = TextAlignment.TopMiddle;
				Height = Width = 0;
				Selection = 0;                      // defaults to first menu item
				switch (menuType)
				{
					case MenuType.GameStart: // top level menu
						Items = new MenuEntry[4];
						Items[0] = new MenuCommand(menu, "Open Route File", MenuTag.RouteList, 0);
						Items[1] = new MenuCommand(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "title" }), MenuTag.Options, 0);
						Items[2] = new MenuCommand(menu, "Show Errors", MenuTag.ErrorList, 0);
						Items[3] = new MenuCommand(menu, "Close", MenuTag.BackToSim, 0);
						if (string.IsNullOrEmpty(SearchDirectory))
						{
							SearchDirectory = Program.FileSystem.InitialRouteFolder;
						}
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.RouteList:
						string[] potentialFiles = new string[] { };
						string[] directoryList = new string[] { };
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
						Items[0] = new MenuCaption(menu, SearchDirectory);
						Items[1] = new MenuCommand(menu, "...", MenuTag.ParentDirectory, 0);
						int totalEntries = 2;
						for (int j = 0; j < directoryList.Length; j++)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(directoryList[j]);
							if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows && directoryInfo.Name[0] == '.')
							{
								continue;
							}
							Items[totalEntries] = new MenuCommand(menu, directoryInfo.Name, MenuTag.Directory, 0);
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
								Items[totalEntries] = new MenuCommand(menu, fileName, MenuTag.RouteFile, 0);
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_route.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
								totalEntries++;
							}
						}
						Array.Resize(ref Items, totalEntries);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.Options:
						Items = new MenuEntry[8];
						Items[0] = new MenuCaption(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "panel", "options" }));
						Items[1] = new MenuOption(menu, OptionType.ScreenResolution, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "resolution" }), Program.Renderer.Screen.AvailableResolutions.ToArray());
						Items[2] = new MenuOption(menu, OptionType.FullScreen, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "display_mode_fullscreen" }), new[] { "true", "false" });
						Items[3] = new MenuOption(menu, OptionType.Interpolation, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation" }), new[]
						{
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearest"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinear"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearestmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinearmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_trilinearmipmap"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_anisotropic"})
						});
						Items[4] = new MenuOption(menu, OptionType.AnisotropicLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_anisotropic_level" }), new[] { "0", "2", "4", "8", "16" });
						Items[5] = new MenuOption(menu, OptionType.AntialiasingLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_antialiasing_level" }), new[] { "0", "2", "4", "8", "16" });
						Items[6] = new MenuOption(menu, OptionType.ViewingDistance, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_distance_viewingdistance" }), new[] { "400", "600", "800", "1000", "1500", "2000" });
						Items[7] = new MenuCommand(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "back" }), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.ErrorList:
						Items = new MenuEntry[Interface.LogMessages.Count + 2];
						Items[0] = Interface.LogMessages.Count == 0 ? new MenuCaption(menu, "No current errors / warnings.") : new MenuCaption(menu, Interface.LogMessages.Count + " total errors / warnings.");

						for (int j = 0; j < Interface.LogMessages.Count; j++)
						{
							Items[j + 1] = new MenuErrorDisplay(menu, Interface.LogMessages[j].Text);
							switch (Interface.LogMessages[j].Type)
							{
								case MessageType.Information:
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_information.png"), new TextureParameters(null, null), out Items[j + 1].Icon);
									break;
								case MessageType.Warning:
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_warning.png"), new TextureParameters(null, null), out Items[j + 1].Icon);
									break;
								case MessageType.Error:
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_error.png"), new TextureParameters(null, null), out Items[j + 1].Icon);
									break;
							}
						}
						Items[Items.Length - 1] = new MenuCommand(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "back" }), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
				}

				ComputeExtent(menuType, Game.Menu.MenuFont, MaxWidth);
				Height = Items.Length * Game.Menu.lineHeight;
				TopItem = 0;

			}
		}
	}
}
