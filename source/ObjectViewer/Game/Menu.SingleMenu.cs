using LibRender2.Menu;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Textures;
using System;
using System.IO;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
	public sealed partial class GameMenu
	{
		/// <summary>Provides implementation for a single menu of the menu stack.</summary>
		/// <remarks>The class is private to Menu, but all its fields are public to allow 'quick-and-dirty'
		/// access from Menu itself.</remarks>
		private class SingleMenu : MenuBase
		{
			public SingleMenu(MenuType menuType, int data = 0, double MaxWidth = 0) : base(menuType)
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
						Items = new MenuEntry[3];
						Items[0] = new MenuCommand("Open Object File", MenuTag.ObjectList, 0);
						Items[1] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "title" }), MenuTag.Options, 0);
						Items[2] = new MenuCommand(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "quit" }), MenuTag.MenuQuit, 0);
						SearchDirectory = Program.FileSystem.InitialRouteFolder;
						Align = TextAlignment.TopLeft;
						break;
					case MenuType.ObjectList:
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
							if (fileName.ToLowerInvariant().EndsWith(".csv") || fileName.ToLowerInvariant().EndsWith(".b3d") || fileName.ToLowerInvariant().EndsWith(".x") || fileName.ToLowerInvariant().EndsWith(".animated"))
							{
								Items[totalEntries] = new MenuCommand(fileName, MenuTag.ObjectFile, 0);
								Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\icon_object.png"), new TextureParameters(null, null), out Items[totalEntries].Icon);
								totalEntries++;
							}
						}
						Array.Resize(ref Items, totalEntries);
						Align = TextAlignment.TopLeft;
						break;
				}
			}
		}
	}
}
