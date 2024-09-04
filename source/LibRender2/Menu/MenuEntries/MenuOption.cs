using LibRender2.Screens;
using OpenTK;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenBveApi.Graphics;

namespace LibRender2.Menu
{
	public class MenuOption : MenuEntry
	{
		private readonly OptionType Type;

		/// <summary>Holds the entries for all options</summary>
		private readonly object[] Entries;

		/// <summary>Gets the current option</summary>
		public object CurrentOption => Entries[CurrentlySelectedOption];

		private int CurrentlySelectedOption;

		public MenuOption(AbstractMenu menu, OptionType type, string text, object[] entries) : base(menu)
		{
			Type = type;
			Text = text;
			Entries = entries;
			switch (type)
			{
				case OptionType.ScreenResolution:
					if (entries is ScreenResolution[] castEntries)
					{
						for (int i = 0; i < castEntries.Length; i++)
						{
							if (castEntries[i].Width == BaseMenu.Renderer.Screen.Width && castEntries[i].Height == BaseMenu.Renderer.Screen.Height)
							{
								CurrentlySelectedOption = i;
								return;
							}
						}
					}
					else
					{
						throw new InvalidDataException("Entries must be a list of screen resolutions");
					}

					break;
				case OptionType.FullScreen:
					CurrentlySelectedOption = BaseMenu.CurrentOptions.FullscreenMode ? 0 : 1;
					return;
				case OptionType.Interpolation:
					CurrentlySelectedOption = (int)BaseMenu.CurrentOptions.Interpolation;
					return;
				case OptionType.AnisotropicLevel:
					for (int i = 0; i < Entries.Length; i++)
					{
						int level = int.Parse(entries[i] as string ?? string.Empty, NumberStyles.Integer);
						if (level == BaseMenu.CurrentOptions.AnisotropicFilteringLevel)
						{
							CurrentlySelectedOption = i;
							return;
						}
					}
					break;
				case OptionType.AntialiasingLevel:
					for (int i = 0; i < Entries.Length; i++)
					{
						int level = int.Parse(entries[i] as string ?? string.Empty, NumberStyles.Integer);
						if (level == BaseMenu.CurrentOptions.AntiAliasingLevel)
						{
							CurrentlySelectedOption = i;
							return;
						}
					}
					break;
				case OptionType.ViewingDistance:
					switch (BaseMenu.CurrentOptions.ViewingDistance)
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
		public void Flip()
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
				case OptionType.ScreenResolution:
					if (!(CurrentOption is ScreenResolution res))
					{
						return;
					}
					BaseMenu.Renderer.SetWindowSize((int)(res.Width * DisplayDevice.Default.ScaleFactor.X), (int)(res.Height * DisplayDevice.Default.ScaleFactor.Y));
					if (BaseMenu.CurrentOptions.FullscreenMode)
					{
						IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
						foreach (DisplayResolution currentResolution in resolutions)
						{
							//Test resolution
							if (currentResolution.Width == BaseMenu.Renderer.Screen.Width / DisplayDevice.Default.ScaleFactor.X &&
								currentResolution.Height == BaseMenu.Renderer.Screen.Height / DisplayDevice.Default.ScaleFactor.Y)
							{
								try
								{
									//HACK: some resolutions will result in openBVE not appearing on screen in full screen, so restore resolution then change resolution
									DisplayDevice.Default.RestoreResolution();
									DisplayDevice.Default.ChangeResolution(currentResolution);
									BaseMenu.Renderer.SetWindowState(WindowState.Fullscreen);
									BaseMenu.Renderer.SetWindowSize((int)(currentResolution.Width * DisplayDevice.Default.ScaleFactor.X), (int)(currentResolution.Height * DisplayDevice.Default.ScaleFactor.Y));
									return;
								}
								catch
								{
									//refresh rate wrong? - Keep trying in case a different refresh rate works OK
								}
							}
						}
					}
					break;
				case OptionType.FullScreen:
					BaseMenu.CurrentOptions.FullscreenMode = !BaseMenu.CurrentOptions.FullscreenMode;
					if (BaseMenu.CurrentOptions.FullscreenMode)
					{
						BaseMenu.Renderer.SetWindowState(WindowState.Fullscreen);
						DisplayDevice.Default.RestoreResolution();
					}
					else
					{
						IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
						foreach (DisplayResolution currentResolution in resolutions)
						{
							//Test resolution
							if (currentResolution.Width == BaseMenu.Renderer.Screen.Width / DisplayDevice.Default.ScaleFactor.X &&
								currentResolution.Height == BaseMenu.Renderer.Screen.Height / DisplayDevice.Default.ScaleFactor.Y)
							{
								try
								{
									//HACK: some resolutions will result in openBVE not appearing on screen in full screen, so restore resolution then change resolution
									DisplayDevice.Default.RestoreResolution();
									DisplayDevice.Default.ChangeResolution(currentResolution);
									BaseMenu.Renderer.SetWindowState(WindowState.Fullscreen);
									BaseMenu.Renderer.SetWindowSize((int)(currentResolution.Width * DisplayDevice.Default.ScaleFactor.X), (int)(currentResolution.Height * DisplayDevice.Default.ScaleFactor.Y));
									return;
								}
								catch
								{
									//refresh rate wrong? - Keep trying in case a different refresh rate works OK
								}
							}
						}
					}
					break;
				case OptionType.Interpolation:
					BaseMenu.CurrentOptions.Interpolation = (InterpolationMode)CurrentlySelectedOption;
					break;
				//HACK: We can't store plain ints due to to boxing, so store strings and parse instead
				case OptionType.AnisotropicLevel:
					BaseMenu.CurrentOptions.AnisotropicFilteringLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.AntialiasingLevel:
					BaseMenu.CurrentOptions.AntiAliasingLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.ViewingDistance:
					BaseMenu.CurrentOptions.ViewingDistance = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;

			}

		}
	}
}
