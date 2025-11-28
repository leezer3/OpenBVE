using LibRender2.Menu;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Textures;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
	public sealed partial class GameMenu
	{
		/// <summary>Provides implementation for a single menu of the menu stack.</summary>
		/// <remarks>The class is private to Menu, but all its fields are public to allow 'quick-and-dirty'
		/// access from Menu itself.</remarks>
		private sealed class SingleMenu : MenuBase
		{
			[SuppressMessage("ReSharper", "CoVariantArrayConversion")]
			public SingleMenu(AbstractMenu menu, MenuType menuType, int data = 0, double maxWidth = 0) : base(menuType)
			{
				//Vector2 size;
				Align = TextAlignment.TopMiddle;
				Height = Width = 0;
				Selection = 0;                      // defaults to first menu item
				switch (menuType)
				{
					case MenuType.GameStart: // top level menu
						Items = new MenuEntry[4];
						Items[0] = new MenuCommand(menu, "Open Object File", MenuTag.ObjectList, 0);
						Items[1] = new MenuCommand(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "title" }), MenuTag.Options, 0);
						Items[2] = new MenuCommand(menu, "Show Errors", MenuTag.ErrorList, 0);
						Items[3] = new MenuCommand(menu, "Close", MenuTag.BackToSim, 0);
						if (string.IsNullOrEmpty(SearchDirectory))
						{
							SearchDirectory = Program.FileSystem.InitialRouteFolder;
						}
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.ObjectList:
						string[] potentialFiles = { };
						string[] directoryList = { };
						bool drives = false;
						int totalEntries;
						switch (SearchDirectory)
						{
							case "":
								// show drive list
								DriveInfo[] systemDrives = DriveInfo.GetDrives();
								directoryList = new string[systemDrives.Length];
								for (int k = 0; k < systemDrives.Length; k++)
								{
									directoryList[k] = systemDrives[k].Name;
								}
								drives = true;
								Items = new MenuEntry[directoryList.Length + 1];
								Items[0] = new MenuCaption(menu, SearchDirectory);
								totalEntries = 1;
								break;
							case "/":
								// root of Linux etc.
								// Mono seems to show block devices and stuff, but goes wonky if we select them
								try
								{
									potentialFiles = Directory.GetFiles(SearchDirectory);
									directoryList = Directory.GetDirectories(SearchDirectory);
								}
								catch
								{
									// Ignored
								}
								Items = new MenuEntry[potentialFiles.Length + directoryList.Length + 1];
								Items[0] = new MenuCaption(menu, SearchDirectory);
								totalEntries = 1;
								break;
							default:
								// actual directory
								try
								{
									potentialFiles = Directory.GetFiles(SearchDirectory);
									directoryList = Directory.GetDirectories(SearchDirectory);
								}
								catch
								{
									// Ignored
								}
								Items = new MenuEntry[potentialFiles.Length + directoryList.Length + 2];
								Items[0] = new MenuCaption(menu, SearchDirectory);
								Items[1] = new MenuCommand(menu, "...", MenuTag.ParentDirectory, 0);
								totalEntries = 2;
								break;
						}
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
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_disk.png"), TextureParameters.NoChange, out Items[totalEntries].Icon);
							}
							else
							{
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_folder.png"), TextureParameters.NoChange, out Items[totalEntries].Icon);
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
							FileInfo fi = new FileInfo(fileName);
							switch (fi.Extension.ToLowerInvariant())
							{
								case ".csv":
								case ".b3d":
								case ".animated":
									Items[totalEntries] = new MenuCommand(menu, fileName, MenuTag.ObjectFile, 0);
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_object.png"), TextureParameters.NoChange, out Items[totalEntries].Icon);
									totalEntries++;
									break;
								case ".obj":
									Items[totalEntries] = new MenuCommand(menu, fileName, MenuTag.ObjectFile, 0);
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_wavefront.png"), TextureParameters.NoChange, out Items[totalEntries].Icon);
									totalEntries++;
									break;
								case ".x":
									Items[totalEntries] = new MenuCommand(menu, fileName, MenuTag.ObjectFile, 0);
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_xobject.png"), TextureParameters.NoChange, out Items[totalEntries].Icon);
									totalEntries++;
									break;
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
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_information.png"), TextureParameters.NoChange, out Items[j + 1].Icon);
									break;
								case MessageType.Warning:
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_warning.png"), TextureParameters.NoChange, out Items[j + 1].Icon);
									break;
								case MessageType.Error:
									Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_error.png"), TextureParameters.NoChange, out Items[j + 1].Icon);
									break;
							}
						}
						Items[Items.Length -1] = new MenuCommand(menu, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "back" }), MenuTag.MenuBack, 0);
						Align = TextAlignment.TopLeft;
						break;
				}

				ComputeExtent(menuType, Game.Menu.MenuFont, maxWidth);
				Height = Items.Length * Game.Menu.lineHeight;
				TopItem = 0;

			}
		}
	}
}
