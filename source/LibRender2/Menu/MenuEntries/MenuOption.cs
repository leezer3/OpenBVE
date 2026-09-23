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
using OpenTK;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;

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
							// n.b. sometimes we seem to end up with a window size 1px different to that requested
							if (System.Math.Abs(castEntries[i].Width - BaseMenu.Renderer.Screen.Width) < 5 && System.Math.Abs(castEntries[i].Height - BaseMenu.Renderer.Screen.Height) < 5)
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
				case OptionType.AutoReloadObjects:
					CurrentlySelectedOption = BaseMenu.Renderer.currentHost.Options.AutoReloadObjects ? 0 : 1;
					return;
				case OptionType.FullScreen:
					CurrentlySelectedOption = BaseMenu.Renderer.currentHost.Options.FullscreenMode ? 0 : 1;
					return;
				case OptionType.Interpolation:
					CurrentlySelectedOption = (int)BaseMenu.Renderer.currentHost.Options.Interpolation;
					return;
				case OptionType.AnisotropicLevel:
					for (int i = 0; i < Entries.Length; i++)
					{
						int level = int.Parse(entries[i] as string ?? string.Empty, NumberStyles.Integer);
						if (level == BaseMenu.Renderer.currentHost.Options.AnisotropicFilteringLevel)
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
						if (level == BaseMenu.Renderer.currentHost.Options.AntiAliasingLevel)
						{
							CurrentlySelectedOption = i;
							return;
						}
					}
					break;
				case OptionType.ViewingDistance:
					switch (BaseMenu.Renderer.currentHost.Options.ViewingDistance)
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
				case OptionType.UIScaleFactor:
					CurrentlySelectedOption = BaseMenu.Renderer.currentHost.Options.UserInterfaceScaleFactor - 1;
					return;
				case OptionType.NumberOfSounds:
					switch (BaseMenu.Renderer.currentHost.Options.SoundNumber)
					{
						case 16:
							CurrentlySelectedOption = 0;
							break;
						case 32:
							CurrentlySelectedOption = 1;
							break;
						case 64:
							CurrentlySelectedOption = 2;
							break;
						case 128:
							CurrentlySelectedOption = 3;
							break;
						default:
							// n.b. This resets the sound number if edited manually in the file
							BaseMenu.Renderer.currentHost.Options.SoundNumber = 16;
							CurrentlySelectedOption = 0;
							break;
					}
					return;
				case OptionType.ShadowQuality:
					switch (BaseMenu.Renderer.currentHost.Options.ShadowResolution)
					{
						case ShadowMapResolution.Off:
							CurrentlySelectedOption = 0;
							break;
						case ShadowMapResolution.Low:
							CurrentlySelectedOption = 1;
							break;
						case ShadowMapResolution.Medium:
							CurrentlySelectedOption = 2;
							break;
						case ShadowMapResolution.High:
							CurrentlySelectedOption = 3;
							break;
						case ShadowMapResolution.Ultra:
							CurrentlySelectedOption = 4;
							break;
					}
					return;
				case OptionType.ShadowFilterCascades:
					CurrentlySelectedOption = BaseMenu.Renderer.currentHost.Options.ShadowFilterCascades ? 0 : 1;
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
					BaseMenu.Renderer.SetWindowSize((int)(res.Width * BaseMenu.Renderer.ScaleFactor.X), (int)(res.Height * BaseMenu.Renderer.ScaleFactor.Y));
					if (BaseMenu.Renderer.currentHost.Options.FullscreenMode)
					{
						IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
						foreach (DisplayResolution currentResolution in resolutions)
						{
							//Test resolution
							if (currentResolution.Width == BaseMenu.Renderer.Screen.Width / BaseMenu.Renderer.ScaleFactor.X &&
								currentResolution.Height == BaseMenu.Renderer.Screen.Height / BaseMenu.Renderer.ScaleFactor.Y)
							{
								try
								{
									//HACK: some resolutions will result in openBVE not appearing on screen in full screen, so restore resolution then change resolution
									DisplayDevice.Default.RestoreResolution();
									DisplayDevice.Default.ChangeResolution(currentResolution);
									BaseMenu.Renderer.SetWindowState(WindowState.Fullscreen);
									BaseMenu.Renderer.SetWindowSize((int)(currentResolution.Width * BaseMenu.Renderer.ScaleFactor.X), (int)(currentResolution.Height * BaseMenu.Renderer.ScaleFactor.Y));
									BaseMenu.Renderer.currentHost.Options.FullscreenWidth = currentResolution.Width;
									BaseMenu.Renderer.currentHost.Options.FullscreenHeight = currentResolution.Height;
									return;
								}
								catch
								{
									//refresh rate wrong? - Keep trying in case a different refresh rate works OK
								}
							}
						}
					}
					else
					{
						BaseMenu.Renderer.currentHost.Options.WindowWidth = res.Width;
						BaseMenu.Renderer.currentHost.Options.WindowHeight = res.Height;
					}
					BaseMenu.ComputePosition();
					break;
				case OptionType.FullScreen:
					BaseMenu.Renderer.currentHost.Options.FullscreenMode = !BaseMenu.Renderer.currentHost.Options.FullscreenMode;
					if (!BaseMenu.Renderer.currentHost.Options.FullscreenMode)
					{
						BaseMenu.Renderer.SetWindowState(WindowState.Normal);
						DisplayDevice.Default.RestoreResolution();
					}
					else
					{
						IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
						foreach (DisplayResolution currentResolution in resolutions)
						{
							//Test resolution
							if (currentResolution.Width == BaseMenu.Renderer.Screen.Width / BaseMenu.Renderer.ScaleFactor.X &&
								currentResolution.Height == BaseMenu.Renderer.Screen.Height / BaseMenu.Renderer.ScaleFactor.Y)
							{
								try
								{
									//HACK: some resolutions will result in openBVE not appearing on screen in full screen, so restore resolution then change resolution
									DisplayDevice.Default.RestoreResolution();
									DisplayDevice.Default.ChangeResolution(currentResolution);
									BaseMenu.Renderer.SetWindowState(WindowState.Fullscreen);
									BaseMenu.Renderer.SetWindowSize((int)(currentResolution.Width * BaseMenu.Renderer.ScaleFactor.X), (int)(currentResolution.Height * BaseMenu.Renderer.ScaleFactor.Y));
									return;
								}
								catch
								{
									//refresh rate wrong? - Keep trying in case a different refresh rate works OK
								}
							}
						}
					}
					BaseMenu.ComputePosition();
					break;
				case OptionType.Interpolation:
					BaseMenu.Renderer.currentHost.Options.Interpolation = (InterpolationMode)CurrentlySelectedOption;
					break;
				case OptionType.AutoReloadObjects:
					BaseMenu.Renderer.currentHost.Options.AutoReloadObjects = !BaseMenu.Renderer.currentHost.Options.AutoReloadObjects;
					break;
				//HACK: We can't store plain ints due to to boxing, so store strings and parse instead
				case OptionType.AnisotropicLevel:
					BaseMenu.Renderer.currentHost.Options.AnisotropicFilteringLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.AntialiasingLevel:
					BaseMenu.Renderer.currentHost.Options.AntiAliasingLevel = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.ViewingDistance:
					BaseMenu.Renderer.currentHost.Options.ViewingDistance = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.UIScaleFactor:
					string currentOption = (string)CurrentOption;
					currentOption = currentOption.Trim('x');
					BaseMenu.Renderer.currentHost.Options.UserInterfaceScaleFactor = int.Parse(currentOption, NumberStyles.Integer);
					break;
				case OptionType.NumberOfSounds:
					BaseMenu.Renderer.currentHost.Options.SoundNumber = int.Parse((string)CurrentOption, NumberStyles.Integer);
					break;
				case OptionType.ShadowQuality:
					// if finer control is wanted, edit the options file (GL menu options are hacky at best)
					switch (CurrentlySelectedOption)
					{
						case 0:
							BaseMenu.Renderer.currentHost.Options.ShadowResolution = ShadowMapResolution.Off;
							break;
						case 1:
							BaseMenu.Renderer.currentHost.Options.ShadowResolution = ShadowMapResolution.Low;
							BaseMenu.Renderer.currentHost.Options.ShadowDrawDistance = ShadowDistance.Medium;
							BaseMenu.Renderer.currentHost.Options.ShadowCascades = ShadowCascadeCount.Two;
							break;
						case 2:
							BaseMenu.Renderer.currentHost.Options.ShadowResolution = ShadowMapResolution.Medium;
							BaseMenu.Renderer.currentHost.Options.ShadowDrawDistance = ShadowDistance.Far;
							BaseMenu.Renderer.currentHost.Options.ShadowCascades = ShadowCascadeCount.Three;
							break;
						case 3:
							BaseMenu.Renderer.currentHost.Options.ShadowResolution = ShadowMapResolution.High;
							BaseMenu.Renderer.currentHost.Options.ShadowDrawDistance = ShadowDistance.VeryFar;
							BaseMenu.Renderer.currentHost.Options.ShadowCascades = ShadowCascadeCount.Four;
							break;
						case 4:
							BaseMenu.Renderer.currentHost.Options.ShadowResolution = ShadowMapResolution.Ultra;
							BaseMenu.Renderer.currentHost.Options.ShadowDrawDistance = ShadowDistance.ViewingDistance;
							BaseMenu.Renderer.currentHost.Options.ShadowCascades = ShadowCascadeCount.Four;
							break;
					}
					BaseMenu.Renderer.InitializeShadows();
					break;
				case OptionType.ShadowFilterCascades:
					BaseMenu.Renderer.currentHost.Options.ShadowFilterCascades = !BaseMenu.Renderer.currentHost.Options.ShadowFilterCascades;
					break;

			}

		}
	}
}
