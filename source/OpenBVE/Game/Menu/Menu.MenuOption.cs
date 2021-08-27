using System.Globalization;
using LibRender2.Screens;
using OpenBveApi.Graphics;
using OpenTK;

namespace OpenBve
{
	public sealed partial class Menu
	{
		private class MenuOption : MenuEntry
		{
			private readonly MenuOptionType Type;

			/// <summary>Holds the entries for all options</summary>
			internal readonly object[] Entries;

			/// <summary>Gets the current option</summary>
			internal object CurrentOption => Entries[CurrentlySelectedOption];

			private int CurrentlySelectedOption;

			internal MenuOption(MenuOptionType type, string text, object[] entries)
			{
				Type = type;
				Text = text;
				Entries = entries;
				switch (type)
				{
					case MenuOptionType.ScreenResolution:
						ScreenResolution[] castEntries = entries as ScreenResolution[];
						for (int i = 0; i < castEntries.Length; i++)
						{
							if (castEntries[i].Width == Program.Renderer.Screen.Width && castEntries[i].Height == Program.Renderer.Screen.Height)
							{
								CurrentlySelectedOption = i;
								return;
							}
						}
						break;
					case MenuOptionType.FullScreen:
						CurrentlySelectedOption = Interface.CurrentOptions.FullscreenMode ? 0 : 1;
						return;
					case MenuOptionType.Interpolation:
						CurrentlySelectedOption = (int)Interface.CurrentOptions.Interpolation;
						return;
					case MenuOptionType.AnisotropicLevel:
						for (int i = 0; i < Entries.Length; i++)
						{
							int level = int.Parse(entries[i] as string, NumberStyles.Integer);
							if (level == Interface.CurrentOptions.AnisotropicFilteringLevel)
							{
								CurrentlySelectedOption = i;
								return;
							}
						}
						break;
					case MenuOptionType.AntialiasingLevel:
						for (int i = 0; i < Entries.Length; i++)
						{
							int level = int.Parse(entries[i] as string, NumberStyles.Integer);
							if (level == Interface.CurrentOptions.AntiAliasingLevel)
							{
								CurrentlySelectedOption = i;
								return;
							}
						}
						break;
					case MenuOptionType.ViewingDistance:
						switch (Interface.CurrentOptions.ViewingDistance)
						{
							case 400:
								CurrentlySelectedOption = 0;
								break;
							case 600:
								CurrentlySelectedOption = 1;
								break;
							case 800:
								CurrentlySelectedOption = 2;
								break;
							case 1000:
								CurrentlySelectedOption = 3;
								break;
							case 1500:
								CurrentlySelectedOption = 4;
								break;
							case 2000:
								CurrentlySelectedOption = 5;
								break;
						}
						return;
				}
				CurrentlySelectedOption = 0;
			}

			/// <summary>Flips to the next option</summary>
			internal void Flip()
			{
				if (CurrentlySelectedOption < Entries.Length - 1)
				{
					CurrentlySelectedOption++;
				}
				else
				{
					CurrentlySelectedOption = 0;
				}

				//Apply
				switch (Type)
				{
					case MenuOptionType.ScreenResolution:
						ScreenResolution res = CurrentOption as ScreenResolution;
						Program.Renderer.Screen.Width = res.Width;
						Program.Renderer.Screen.Height = res.Height;
						Program.currentGameWindow.Width = res.Width;
						Program.currentGameWindow.Height = res.Height;
						break;
					case MenuOptionType.FullScreen:
						Interface.CurrentOptions.FullscreenMode = !Interface.CurrentOptions.FullscreenMode;
						if (Program.currentGameWindow.WindowState == WindowState.Fullscreen)
						{
							Program.currentGameWindow.WindowState = WindowState.Normal;
						}
						else
						{
							Program.currentGameWindow.WindowState = WindowState.Fullscreen;
						}
						break;
					case MenuOptionType.Interpolation:
						Interface.CurrentOptions.Interpolation = (InterpolationMode)CurrentlySelectedOption;
						break;
					//HACK: We can't store plain ints due to to boxing, so store strings and parse instead
					case MenuOptionType.AnisotropicLevel:
						Interface.CurrentOptions.AnisotropicFilteringLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
						break;
					case MenuOptionType.AntialiasingLevel:
						Interface.CurrentOptions.AntiAliasingLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
						break;
					case MenuOptionType.ViewingDistance:
						Interface.CurrentOptions.ViewingDistance = int.Parse((string)CurrentOption, NumberStyles.Integer);
						break;
					
				}
				
			}
		}

	}
}
