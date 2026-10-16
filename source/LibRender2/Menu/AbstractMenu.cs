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
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using System.Collections.Generic;
using LibRender2.Primitives;
using OpenBveApi;
using OpenBveApi.Input;
using OpenBveApi.Interface;
using OpenBveApi.Hosts;
using LibRender2.Viewports;
using OpenBveApi.Objects;

namespace LibRender2.Menu
{
	/// <summary>Provides the abstract class for creating OpenGL based menus</summary>
	public abstract class AbstractMenu
	{
		/// <summary>The color used for the overlay</summary>
		public readonly Color128 overlayColor = new Color128(0.0f, 0.0f, 0.0f, 0.45f);

		/// <summary>The color used for the menu background</summary>
		public readonly Color128 backgroundColor = new Color128(0.08f, 0.09f, 0.11f, 0.94f);

		/// <summary>The color used to draw the highlight over a selected item</summary>
		public readonly Color128 highlightColor = new Color128(0.0f, 0.47f, 0.83f, 1.0f);

		/// <summary> The color used to draw the highlight over a selected folder</summary>
		public readonly Color128 folderHighlightColor = new Color128(0.0f, 0.69f, 1.0f, 1.0f);

		/// <summary>The color used to draw the highlight over a selected routefile</summary>
		public readonly Color128 routeHighlightColor = new Color128(0.0f, 1.0f, 0.69f, 1.0f);

		/// <summary>The color used for caption text</summary>
		public static readonly Color128 ColourCaption = new Color128(0.58f, 0.75f, 0.98f, 1.0f);

		/// <summary>The color used to draw dimmed text</summary>
		public static readonly Color128 ColourDimmed = new Color128(0.6f, 0.64f, 0.7f, 0.65f);

		/// <summary>The color used to draw highlighted text</summary>
		public static readonly Color128 ColourHighlight = Color128.White;

		/// <summary>The color used to draw normal text</summary>
		public static readonly Color128 ColourNormal = Color128.White;

		/// <summary> Holds the stack of menus</summary>
		public MenuBase[] Menus = { };

		/// <summary>The font currently used for drawing menu items</summary>
		public OpenGlFont MenuFont = null;

		/// <summary>The ratio between the menu font size and the line spacing</summary>
		public const float LineSpacing = 1.75f;

		/// <summary>The number of visible items in the menu</summary>
		public int visibleItems;

		/// <summary>The screen co-ordinates of the top-left of the menu</summary>
		public Vector2 menuMin;

		/// <summary>The screen co-ordinates of the bottom-right of the menu</summary>
		public Vector2 menuMax;

		/// <summary>Holds a reference to the base renderer</summary>
		public readonly BaseRenderer Renderer;

		/// <summary>Holds a reference to the options</summary>
		public readonly BaseOptions CurrentOptions;
		
		/// <summary>The index of the current menu within the stack</summary>
		public int CurrMenu = -1;

		/// <summary>The border for the menu in pixels</summary>
		public static readonly Vector2 Border = new Vector2(16, 16);

		/// <summary>The border for each menu item in pixels</summary>
		public static readonly Vector2 ItemBorder = new Vector2(8, 2);

		/// <summary>The height of the top item in the current menu in pixels</summary>
		public double topItemY;

		/// <summary>The total height of one rendered menu item in pixels</summary>
		public int LineHeight;

		/// <summary>The controls within the menu</summary>
		public List<GLControl> menuControls = new List<GLControl>();

		/// <summary>The tab container for the options panel</summary>
		public GLTabContainer OptionsTabContainer;

		/// <summary>The index of the options row which currently holds keyboard focus</summary>
		private int optionsFocusRow = 0;

		/// <summary>The container holding the rows of the currently selected options tab</summary>
		private GLContainer CurrentOptionsContent
		{
			get { return OptionsTabContainer != null ? OptionsTabContainer.SelectedContent as GLContainer : null; }
		}

		/// <summary>Gets the interactive control within the currently focused options row</summary>
		private GLControl FocusedOptionsControl()
		{
			GLContainer content = CurrentOptionsContent;
			if (content == null || optionsFocusRow < 0 || optionsFocusRow >= content.Children.Count)
			{
				return null;
			}
			GLControl row = content.Children[optionsFocusRow];
			if (row is GLContainer rowContainer && rowContainer.Children.Count >= 2)
			{
				return rowContainer.Children[rowContainer.Children.Count - 1];
			}
			return row;
		}

		/// <summary>Whether the interactive control of an options row can receive input</summary>
		private static bool IsRowInteractive(GLControl row)
		{
			if (row is GLContainer rowContainer && rowContainer.Children.Count >= 2)
			{
				return rowContainer.Children[rowContainer.Children.Count - 1].Enabled;
			}
			return row.Enabled;
		}

		/// <summary>Moves keyboard focus between options rows, scrolling the focused row into view</summary>
		private void MoveOptionsFocus(int direction)
		{
			GLContainer content = CurrentOptionsContent;
			if (content == null || content.Children.Count == 0)
			{
				return;
			}
			// Skip rows whose control is disabled (e.g. FPS limit while VSync is on)
			int next = optionsFocusRow + direction;
			while (next >= 0 && next < content.Children.Count && !IsRowInteractive(content.Children[next]))
			{
				next += direction;
			}
			if (next >= 0 && next < content.Children.Count)
			{
				optionsFocusRow = next;
			}
			if (content is GLScrollContainer scroll && optionsFocusRow < scroll.Children.Count)
			{
				scroll.EnsureVisible(scroll.Children[optionsFocusRow]);
			}
			UpdateOptionsFocus();
		}

		/// <summary>Applies the keyboard focus highlight to the focused options control</summary>
		private void UpdateOptionsFocus()
		{
			GLContainer content = CurrentOptionsContent;
			if (content == null) return;
			for (int i = 0; i < content.Children.Count; i++)
			{
				GLControl row = content.Children[i];
				// Highlight the focused row itself, and ring the interactive control inside it
				// (no control ring when the focused row's control is disabled)
				row.KeyboardFocused = (i == optionsFocusRow);
				if (row is GLContainer rowContainer)
				{
					for (int j = 0; j < rowContainer.Children.Count; j++)
					{
						rowContainer.Children[j].KeyboardFocused = (i == optionsFocusRow && j == rowContainer.Children.Count - 1 && IsRowInteractive(row));
					}
				}
			}
		}

		/// <summary>Handles keyboard navigation of the options panel</summary>
		/// <returns>True if the key was consumed by the options panel</returns>
		private bool ProcessOptionsKey(OpenTK.Input.Key key)
		{
			if (OptionsTabContainer == null || !OptionsTabContainer.IsVisible)
			{
				return false;
			}
			switch (key)
			{
				case OpenTK.Input.Key.Up:
					if (FocusedOptionsControl() is GLDropdown upDropdown && upDropdown.Expanded)
					{
						upDropdown.OnAdjust(-1);
						return true;
					}
					MoveOptionsFocus(-1);
					return true;
				case OpenTK.Input.Key.Down:
					if (FocusedOptionsControl() is GLDropdown downDropdown && downDropdown.Expanded)
					{
						downDropdown.OnAdjust(1);
						return true;
					}
					MoveOptionsFocus(1);
					return true;
				case OpenTK.Input.Key.Left:
					FocusedOptionsControl()?.OnAdjust(-1);
					UpdateOptionsFocus();
					return true;
				case OpenTK.Input.Key.Right:
					FocusedOptionsControl()?.OnAdjust(1);
					UpdateOptionsFocus();
					return true;
				case OpenTK.Input.Key.Enter:
				case OpenTK.Input.Key.Space:
					FocusedOptionsControl()?.OnActivate();
					UpdateOptionsFocus();
					return true;
				case OpenTK.Input.Key.Tab:
					if (OptionsTabContainer.TabTitles.Count > 0)
					{
						OptionsTabContainer.SelectTab((OptionsTabContainer.SelectedTab + 1) % OptionsTabContainer.TabTitles.Count);
						optionsFocusRow = 0;
						UpdateOptionsFocus();
					}
					return true;
			}
			return false;
		}

		public void InitializeOptionsTabContainer(HostApplication host)
		{
			if (OptionsTabContainer != null) return;

			OptionsTabContainer = new GLTabContainer(Renderer);
			OptionsTabContainer.IsVisible = false;

			menuControls.Add(OptionsTabContainer);

			var displayTab = new GLScrollContainer(Renderer) { Spacing = 8f, Padding = 12f };
			var qualityTab = new GLScrollContainer(Renderer) { Spacing = 8f, Padding = 12f };
			var otherTab = new GLScrollContainer(Renderer) { Spacing = 8f, Padding = 12f };
			GLScrollContainer lightingTab = null;
			if (host == HostApplication.RouteViewer || host == HostApplication.ObjectViewer)
			{
				lightingTab = new GLScrollContainer(Renderer) { Spacing = 8f, Padding = 12f };
			}

			OptionsTabContainer.AddTab("Display", displayTab);
			OptionsTabContainer.AddTab("Quality", qualityTab);
			OptionsTabContainer.AddTab("Other", otherTab);
			if (lightingTab != null)
			{
				OptionsTabContainer.AddTab("Lighting", lightingTab);
			}

			// Populate Display Tab
			var resRow = CreateRow("Resolution");
			var resDropdown = new GLDropdown(Renderer);
			foreach (var r in Renderer.Screen.AvailableResolutions)
			{
				resDropdown.Items.Add(r.ToString());
			}
			for (int i = 0; i < Renderer.Screen.AvailableResolutions.Count; i++)
			{
				var r = Renderer.Screen.AvailableResolutions[i];
				if (System.Math.Abs(r.Width - Renderer.Screen.Width) < 5 && System.Math.Abs(r.Height - Renderer.Screen.Height) < 5)
				{
					resDropdown.SelectedIndex = i;
					break;
				}
			}
			resDropdown.SelectionChanged += (s, e) =>
			{
				if (resDropdown.SelectedIndex >= 0 && resDropdown.SelectedIndex < Renderer.Screen.AvailableResolutions.Count)
				{
					var res = Renderer.Screen.AvailableResolutions[resDropdown.SelectedIndex];
					Renderer.SetWindowSize((int)(res.Width * Renderer.ScaleFactor.X), (int)(res.Height * Renderer.ScaleFactor.Y));
					if (!CurrentOptions.FullscreenMode)
					{
						CurrentOptions.WindowWidth = res.Width;
						CurrentOptions.WindowHeight = res.Height;
					}
					ComputePosition();
				}
			};
			resRow.Children.Add(resDropdown);
			displayTab.Children.Add(resRow);

			var fsRow = CreateRow("Fullscreen");
			var fsToggle = new GLToggle(Renderer) { Checked = CurrentOptions.FullscreenMode, Text = "" };
			fsToggle.ValueChanged += (s, e) =>
			{
				CurrentOptions.FullscreenMode = fsToggle.Checked;
				if (!CurrentOptions.FullscreenMode)
				{
					Renderer.SetWindowState(OpenTK.WindowState.Normal);
					OpenTK.DisplayDevice.Default.RestoreResolution();
				}
				else
				{
					var resolutions = OpenTK.DisplayDevice.Default.AvailableResolutions;
					foreach (var res in resolutions)
					{
						if (res.Width == Renderer.Screen.Width / Renderer.ScaleFactor.X && res.Height == Renderer.Screen.Height / Renderer.ScaleFactor.Y)
						{
							try
							{
								OpenTK.DisplayDevice.Default.RestoreResolution();
								OpenTK.DisplayDevice.Default.ChangeResolution(res);
								Renderer.SetWindowState(OpenTK.WindowState.Fullscreen);
								Renderer.SetWindowSize((int)(res.Width * Renderer.ScaleFactor.X), (int)(res.Height * Renderer.ScaleFactor.Y));
								break;
							}
							catch { }
						}
					}
				}
				ComputePosition();
			};
			fsRow.Children.Add(fsToggle);
			displayTab.Children.Add(fsRow);

			if (host == HostApplication.RouteViewer || host == HostApplication.ObjectViewer)
			{
				var vsyncRow = CreateRow("Vertical Synchronization");
				var vsyncToggle = new GLToggle(Renderer) { Checked = CurrentOptions.VerticalSynchronization, Text = "" };
				var fpsRow = CreateRow("FPS Limit");
				var fpsDropdown = new GLDropdown(Renderer);
				fpsDropdown.Items.AddRange(new[] { "Unlimited", "30", "60", "120", "240" });
				int[] fpsPresets = { 0, 30, 60, 120, 240 };
				switch (FPSLimitValue)
				{
					case 30: fpsDropdown.SelectedIndex = 1; break;
					case 60: fpsDropdown.SelectedIndex = 2; break;
					case 120: fpsDropdown.SelectedIndex = 3; break;
					case 240: fpsDropdown.SelectedIndex = 4; break;
					default: fpsDropdown.SelectedIndex = 0; break;
				}
				fpsDropdown.Enabled = !vsyncToggle.Checked;
				vsyncToggle.ValueChanged += (s, e) =>
				{
					CurrentOptions.VerticalSynchronization = vsyncToggle.Checked;
					Renderer.GameWindow.VSync = vsyncToggle.Checked ? OpenTK.VSyncMode.On : OpenTK.VSyncMode.Off;
					fpsDropdown.Enabled = !vsyncToggle.Checked;
				};
				fpsDropdown.SelectionChanged += (s, e) =>
				{
					if (fpsDropdown.SelectedIndex < 0 || fpsDropdown.SelectedIndex >= fpsPresets.Length) return;
					FPSLimitValue = fpsPresets[fpsDropdown.SelectedIndex];
					Renderer.GameWindow.TargetRenderFrequency = FPSLimitValue > 0 ? FPSLimitValue : 0;
				};
				vsyncRow.Children.Add(vsyncToggle);
				displayTab.Children.Add(vsyncRow);
				fpsRow.Children.Add(fpsDropdown);
				displayTab.Children.Add(fpsRow);
			}

			if (host == HostApplication.OpenBve)
			{
				var scaleRow = CreateRow("UI Scale");
				var scaleDropdown = new GLDropdown(Renderer);
				scaleDropdown.Items.AddRange(new[] { "1x", "2x", "3x", "4x", "5x", "6x" });
				scaleDropdown.SelectedIndex = CurrentOptions.UserInterfaceScaleFactor - 1;
				scaleDropdown.SelectionChanged += (s, e) =>
				{
					CurrentOptions.UserInterfaceScaleFactor = scaleDropdown.SelectedIndex + 1;
				};
				scaleRow.Children.Add(scaleDropdown);
				displayTab.Children.Add(scaleRow);
			}

			// Populate Quality Tab
			var interpRow = CreateRow("Mode");
			var interpDropdown = new GLDropdown(Renderer);
			interpDropdown.Items.AddRange(new[] { "Nearest", "Bilinear", "NearestMipmap", "BilinearMipmap", "TrilinearMipmap", "Anisotropic" });
			interpDropdown.SelectedIndex = (int)CurrentOptions.Interpolation;
			interpDropdown.SelectionChanged += (s, e) =>
			{
				var newInterpolation = (InterpolationMode)interpDropdown.SelectedIndex;
				if (newInterpolation == CurrentOptions.Interpolation) return;
				CurrentOptions.Interpolation = newInterpolation;
				Renderer.TextureManager.UnloadAllTextures(false);
				Renderer.TextureManager.LoadAllTextures();
			};
			interpRow.Children.Add(interpDropdown);
			qualityTab.Children.Add(interpRow);

			var anisoRow = CreateRow("Anisotropic Filtering Level");
			var anisoDropdown = new GLDropdown(Renderer);
			anisoDropdown.Items.AddRange(new[] { "0", "2", "4", "8", "16" });
			int currentAniso = CurrentOptions.AnisotropicFilteringLevel;
			int selectAnisoIdx = anisoDropdown.Items.IndexOf(currentAniso.ToString());
			if (selectAnisoIdx >= 0) anisoDropdown.SelectedIndex = selectAnisoIdx;
			anisoDropdown.SelectionChanged += (s, e) =>
			{
				int newAniso = int.Parse(anisoDropdown.Items[anisoDropdown.SelectedIndex]);
				if (newAniso == CurrentOptions.AnisotropicFilteringLevel) return;
				CurrentOptions.AnisotropicFilteringLevel = newAniso;
				Renderer.TextureManager.UnloadAllTextures(false);
				Renderer.TextureManager.LoadAllTextures();
			};
			anisoRow.Children.Add(anisoDropdown);
			qualityTab.Children.Add(anisoRow);

			var aaRow = CreateRow("Antialiasing Level");
			var aaDropdown = new GLDropdown(Renderer);
			aaDropdown.Items.AddRange(new[] { "0", "2", "4", "8", "16" });
			int currentAA = CurrentOptions.AntiAliasingLevel;
			int selectAAIdx = aaDropdown.Items.IndexOf(currentAA.ToString());
			if (selectAAIdx >= 0) aaDropdown.SelectedIndex = selectAAIdx;
			aaDropdown.SelectionChanged += (s, e) =>
			{
				CurrentOptions.AntiAliasingLevel = int.Parse(aaDropdown.Items[aaDropdown.SelectedIndex]);
			};
			aaRow.Children.Add(aaDropdown);
			qualityTab.Children.Add(aaRow);

			var transRow = CreateRow("Transparency Quality");
			var transDropdown = new GLDropdown(Renderer);
			transDropdown.Items.AddRange(new[] { "Sharp", "Intermediate", "Smooth" });
			transDropdown.SelectedIndex = (int)CurrentOptions.TransparencyMode;
			transDropdown.SelectionChanged += (s, e) =>
			{
				CurrentOptions.TransparencyMode = (TransparencyMode)transDropdown.SelectedIndex;
			};
			transRow.Children.Add(transDropdown);
			qualityTab.Children.Add(transRow);

			var distRow = CreateRow("Viewing Distance (m)");
			var distUpDown = new GLNumericUpDown(Renderer) { Minimum = 10f, Maximum = 9999f, Increment = 50f, Value = CurrentOptions.ViewingDistance };
			distUpDown.ValueChanged += (s, e) =>
			{
				CurrentOptions.ViewingDistance = (int)distUpDown.Value;
				Renderer.UpdateViewport(ViewportChangeMode.ChangeToScenery);
				if (Renderer.CameraTrackFollower != null)
				{
					Renderer.UpdateViewingDistances(CurrentOptions.ViewingDistance);
				}
				OnOptionChanged(OptionType.ViewingDistance);
			};
			distRow.Children.Add(distUpDown);
			qualityTab.Children.Add(distRow);

			var clipRow = CreateRow("Near Clip (m)");
			var clipUpDown = new GLNumericUpDown(Renderer) { Minimum = 0.01f, Maximum = 100f, Increment = 0.01f, ValueFormat = "0.###", Value = (float)CurrentOptions.NearClipBase };
			clipUpDown.ValueChanged += (s, e) =>
			{
				CurrentOptions.NearClipBase = clipUpDown.Value;
				Renderer.UpdateViewport(ViewportChangeMode.ChangeToScenery);
				OnOptionChanged(OptionType.NearClip);
			};
			clipRow.Children.Add(clipUpDown);
			qualityTab.Children.Add(clipRow);

			if (host == HostApplication.RouteViewer || host == HostApplication.ObjectViewer)
			{
				var optimizeRow = CreateRow("Object Optimization Mode");
				var optimizeDropdown = new GLDropdown(Renderer);
				optimizeDropdown.Items.AddRange(new[] { "None", "Low", "High" });
				int optimizeIndex = ObjectOptimizationIndex;
				optimizeDropdown.SelectedIndex = optimizeIndex >= 0 && optimizeIndex < optimizeDropdown.Items.Count ? optimizeIndex : 1;
				optimizeDropdown.SelectionChanged += (s, e) =>
				{
					if (optimizeDropdown.SelectedIndex >= 0) ObjectOptimizationIndex = optimizeDropdown.SelectedIndex;
				};
				optimizeRow.Children.Add(optimizeDropdown);
				qualityTab.Children.Add(optimizeRow);
			}

			// Populate Other Tab
			var xParserRow = CreateRow("Use New X Parser");
			var xParserDropdown = new GLDropdown(Renderer);
			xParserDropdown.Items.AddRange(new[] { "Original", "NewXParser", "Assimp" });
			xParserDropdown.SelectedIndex = (int)CurrentOptions.CurrentXParser;
			xParserDropdown.SelectionChanged += (s, e) =>
			{
				CurrentOptions.CurrentXParser = (XParsers)xParserDropdown.SelectedIndex;
			};
			xParserRow.Children.Add(xParserDropdown);
			otherTab.Children.Add(xParserRow);

			var objParserRow = CreateRow("Use New Obj Parser");
			var objParserDropdown = new GLDropdown(Renderer);
			objParserDropdown.Items.AddRange(new[] { "Original", "Assimp" });
			objParserDropdown.SelectedIndex = (int)CurrentOptions.CurrentObjParser;
			objParserDropdown.SelectionChanged += (s, e) =>
			{
				CurrentOptions.CurrentObjParser = (ObjParsers)objParserDropdown.SelectedIndex;
			};
			objParserRow.Children.Add(objParserDropdown);
			otherTab.Children.Add(objParserRow);

			if (host == HostApplication.ObjectViewer)
			{
				var reloadRow = CreateRow("Automatically Reload Changed Objects");
				var reloadToggle = new GLToggle(Renderer) { Checked = CurrentOptions.AutoReloadObjects, Text = "" };
				reloadToggle.ValueChanged += (s, e) =>
				{
					CurrentOptions.AutoReloadObjects = reloadToggle.Checked;
				};
				reloadRow.Children.Add(reloadToggle);
				otherTab.Children.Add(reloadRow);
			}

			if (host == HostApplication.RouteViewer)
			{
				var logoRow = CreateRow("Show Logo");
				var logoToggle = new GLToggle(Renderer) { Checked = CurrentOptions.LoadingLogo, Text = "" };
				logoToggle.ValueChanged += (s, e) => { CurrentOptions.LoadingLogo = logoToggle.Checked; };
				logoRow.Children.Add(logoToggle);
				otherTab.Children.Add(logoRow);

				var bgRow = CreateRow("Show Backgrounds");
				var bgToggle = new GLToggle(Renderer) { Checked = CurrentOptions.LoadingBackground, Text = "" };
				bgToggle.ValueChanged += (s, e) => { CurrentOptions.LoadingBackground = bgToggle.Checked; };
				bgRow.Children.Add(bgToggle);
				otherTab.Children.Add(bgRow);

				var barRow = CreateRow("Show Progress Bar");
				var barToggle = new GLToggle(Renderer) { Checked = CurrentOptions.LoadingProgressBar, Text = "" };
				barToggle.ValueChanged += (s, e) => { CurrentOptions.LoadingProgressBar = barToggle.Checked; };
				barRow.Children.Add(barToggle);
				otherTab.Children.Add(barRow);
			}

			// Populate Lighting Tab (F8 Shadow & Lighting parity, viewers only)
			if (lightingTab != null)
			{
				var shadowResRow = CreateRow("Resolution");
				var shadowResDropdown = new GLDropdown(Renderer);
				shadowResDropdown.Items.AddRange(new[] { "Off", "Low (512)", "Medium (1024)", "High (2048)", "Ultra (4096)" });
				switch (CurrentOptions.ShadowResolution)
				{
					case ShadowMapResolution.Off: shadowResDropdown.SelectedIndex = 0; break;
					case ShadowMapResolution.Low: shadowResDropdown.SelectedIndex = 1; break;
					case ShadowMapResolution.Medium: shadowResDropdown.SelectedIndex = 2; break;
					case ShadowMapResolution.High: shadowResDropdown.SelectedIndex = 3; break;
					case ShadowMapResolution.Ultra: shadowResDropdown.SelectedIndex = 4; break;
					default: shadowResDropdown.SelectedIndex = 3; break;
				}
				shadowResDropdown.SelectionChanged += (s, e) =>
				{
					ShadowMapResolution res;
					switch (shadowResDropdown.SelectedIndex)
					{
						case 0: res = ShadowMapResolution.Off; break;
						case 1: res = ShadowMapResolution.Low; break;
						case 2: res = ShadowMapResolution.Medium; break;
						case 3: res = ShadowMapResolution.High; break;
						case 4: res = ShadowMapResolution.Ultra; break;
						default: return;
					}
					if (res == CurrentOptions.ShadowResolution) return;
					CurrentOptions.ShadowResolution = res;
					Renderer.ReloadShadowSettings();
				};
				shadowResRow.Children.Add(shadowResDropdown);
				lightingTab.Children.Add(shadowResRow);

				var shadowDistRow = CreateRow("Draw Distance");
				var shadowDistDropdown = new GLDropdown(Renderer);
				shadowDistDropdown.Items.AddRange(new[] { "Near (150m)", "Medium (300m)", "Far (500m)", "Very Far (800m)", "Viewing Distance" });
				switch (CurrentOptions.ShadowDrawDistance)
				{
					case ShadowDistance.Near: shadowDistDropdown.SelectedIndex = 0; break;
					case ShadowDistance.Medium: shadowDistDropdown.SelectedIndex = 1; break;
					case ShadowDistance.Far: shadowDistDropdown.SelectedIndex = 2; break;
					case ShadowDistance.VeryFar: shadowDistDropdown.SelectedIndex = 3; break;
					case ShadowDistance.ViewingDistance: shadowDistDropdown.SelectedIndex = 4; break;
					default: shadowDistDropdown.SelectedIndex = 1; break;
				}
				shadowDistDropdown.SelectionChanged += (s, e) =>
				{
					ShadowDistance dist;
					switch (shadowDistDropdown.SelectedIndex)
					{
						case 0: dist = ShadowDistance.Near; break;
						case 1: dist = ShadowDistance.Medium; break;
						case 2: dist = ShadowDistance.Far; break;
						case 3: dist = ShadowDistance.VeryFar; break;
						case 4: dist = ShadowDistance.ViewingDistance; break;
						default: return;
					}
					if (dist == CurrentOptions.ShadowDrawDistance) return;
					CurrentOptions.ShadowDrawDistance = dist;
					Renderer.ReloadShadowSettings();
				};
				shadowDistRow.Children.Add(shadowDistDropdown);
				lightingTab.Children.Add(shadowDistRow);

				var shadowCascadeRow = CreateRow("Cascades");
				var shadowCascadeDropdown = new GLDropdown(Renderer);
				shadowCascadeDropdown.Items.AddRange(new[] { "2 Cascades", "3 Cascades", "4 Cascades" });
				switch (CurrentOptions.ShadowCascades)
				{
					case ShadowCascadeCount.Two: shadowCascadeDropdown.SelectedIndex = 0; break;
					case ShadowCascadeCount.Three: shadowCascadeDropdown.SelectedIndex = 1; break;
					case ShadowCascadeCount.Four: shadowCascadeDropdown.SelectedIndex = 2; break;
					default: shadowCascadeDropdown.SelectedIndex = 1; break;
				}
				shadowCascadeDropdown.SelectionChanged += (s, e) =>
				{
					ShadowCascadeCount cascades;
					switch (shadowCascadeDropdown.SelectedIndex)
					{
						case 0: cascades = ShadowCascadeCount.Two; break;
						case 1: cascades = ShadowCascadeCount.Three; break;
						case 2: cascades = ShadowCascadeCount.Four; break;
						default: return;
					}
					if (cascades == CurrentOptions.ShadowCascades) return;
					CurrentOptions.ShadowCascades = cascades;
					Renderer.ReloadShadowSettings();
				};
				shadowCascadeRow.Children.Add(shadowCascadeDropdown);
				lightingTab.Children.Add(shadowCascadeRow);

				var shadowStrengthRow = CreateRow("Strength (0 - 100%)");
				var shadowStrengthNum = new GLNumericUpDown(Renderer) { Minimum = 1f, Maximum = 100f, Increment = 1f, ValueFormat = "0", Value = (float)System.Math.Max(1.0, System.Math.Min(100.0, System.Math.Round(CurrentOptions.ShadowStrength * 100.0))) };
				shadowStrengthNum.ValueChanged += (s, e) =>
				{
					CurrentOptions.ShadowStrength = shadowStrengthNum.Value / 100.0;
				};
				shadowStrengthRow.Children.Add(shadowStrengthNum);
				lightingTab.Children.Add(shadowStrengthRow);

				var shadowBiasRow = CreateRow("Shadow Bias");
				var shadowBiasNum = new GLNumericUpDown(Renderer) { Minimum = 0f, Maximum = 1f, Increment = 0.000001f, ValueFormat = "0.000000", Value = (float)CurrentOptions.ShadowBias };
				shadowBiasNum.ValueChanged += (s, e) =>
				{
					CurrentOptions.ShadowBias = shadowBiasNum.Value;
				};
				shadowBiasRow.Children.Add(shadowBiasNum);
				lightingTab.Children.Add(shadowBiasRow);

				var shadowNormalBiasRow = CreateRow("Normal Bias");
				var shadowNormalBiasNum = new GLNumericUpDown(Renderer) { Minimum = 0f, Maximum = 10f, Increment = 0.1f, ValueFormat = "0.0", Value = (float)CurrentOptions.ShadowNormalBias };
				shadowNormalBiasNum.ValueChanged += (s, e) =>
				{
					CurrentOptions.ShadowNormalBias = shadowNormalBiasNum.Value;
				};
				shadowNormalBiasRow.Children.Add(shadowNormalBiasNum);
				lightingTab.Children.Add(shadowNormalBiasRow);

				var shadowFilterRow = CreateRow("Per-cascade culling");
				var shadowFilterToggle = new GLToggle(Renderer) { Checked = CurrentOptions.ShadowFilterCascades, Text = "" };
				shadowFilterToggle.ValueChanged += (s, e) =>
				{
					CurrentOptions.ShadowFilterCascades = shadowFilterToggle.Checked;
				};
				shadowFilterRow.Children.Add(shadowFilterToggle);
				lightingTab.Children.Add(shadowFilterRow);

				var shadowSmoothRow = CreateRow("Smooth shadow");
				var shadowSmoothToggle = new GLToggle(Renderer) { Checked = CurrentOptions.ShadowSmooth, Text = "" };
				shadowSmoothToggle.ValueChanged += (s, e) =>
				{
					CurrentOptions.ShadowSmooth = shadowSmoothToggle.Checked;
				};
				shadowSmoothRow.Children.Add(shadowSmoothToggle);
				lightingTab.Children.Add(shadowSmoothRow);

				var shadowRadiusRow = CreateRow("Smooth Radius");
				var shadowRadiusDropdown = new GLDropdown(Renderer);
				shadowRadiusDropdown.Items.AddRange(new[] { "Low", "Medium", "High" });
				if (CurrentOptions.ShadowFilterRadius >= 2.5) shadowRadiusDropdown.SelectedIndex = 2;
				else if (CurrentOptions.ShadowFilterRadius <= 1.0) shadowRadiusDropdown.SelectedIndex = 0;
				else shadowRadiusDropdown.SelectedIndex = 1;
				shadowRadiusDropdown.SelectionChanged += (s, e) =>
				{
					switch (shadowRadiusDropdown.SelectedIndex)
					{
						case 0: CurrentOptions.ShadowFilterRadius = 1.0; break;
						case 2: CurrentOptions.ShadowFilterRadius = 2.5; break;
						default: CurrentOptions.ShadowFilterRadius = 1.5; break;
					}
				};
				shadowRadiusRow.Children.Add(shadowRadiusDropdown);
				lightingTab.Children.Add(shadowRadiusRow);

				var sunAzRow = CreateRow("Azimuth (Phi)");
				var sunAzSlider = new GLSlider(Renderer) { Minimum = -180.0, Maximum = 180.0, ShowValue = true, ValueFormat = "0°", Value = System.Math.Max(-180.0, System.Math.Min(180.0, CurrentOptions.LightAzimuth)) };
				sunAzSlider.ValueChanged += (s, e) =>
				{
					CurrentOptions.LightAzimuth = sunAzSlider.Value;
					ApplySunDirection();
				};
				sunAzRow.Children.Add(sunAzSlider);
				lightingTab.Children.Add(sunAzRow);

				var sunElRow = CreateRow("Elevation (Theta)");
				var sunElSlider = new GLSlider(Renderer) { Minimum = -90.0, Maximum = 90.0, ShowValue = true, ValueFormat = "0°", Value = System.Math.Max(-90.0, System.Math.Min(90.0, CurrentOptions.LightElevation)) };
				sunElSlider.ValueChanged += (s, e) =>
				{
					CurrentOptions.LightElevation = sunElSlider.Value;
					ApplySunDirection();
				};
				sunElRow.Children.Add(sunElSlider);
				lightingTab.Children.Add(sunElRow);
			}
		}

		private GLHBoxContainer CreateRow(string name)
		{
			var row = new GLHBoxContainer(Renderer) { Spacing = 12f };
			row.Size = new Vector2(300f, 24f);
			row.DrawBackground = true;
			var label = new Label(Renderer, name);
			label.BackgroundColor = new Color128(0, 0, 0, 0);
			// Let the label keep its natural measured width so long text never overflows
			label.Size = new Vector2(System.Math.Max(label.Size.X, 24f), 24f);
			row.Children.Add(label);
			return row;
		}

		/// <summary>Object optimization mode index (None/Low/High), or -1 when unsupported by this host</summary>
		public virtual int ObjectOptimizationIndex { get => -1; set { } }

		/// <summary>FPS limit preset value, or -1 when unsupported by this host</summary>
		public virtual int FPSLimitValue { get => -1; set { } }

		/// <summary>Applies a changed sun direction (hosts add route-specific propagation)</summary>
		public virtual void ApplySunDirection()
		{
			double azimuthRad = CurrentOptions.LightAzimuth * System.Math.PI / 180.0;
			double elevationRad = CurrentOptions.LightElevation * System.Math.PI / 180.0;
			Renderer.Lighting.OptionLightPosition = new Vector3(
				(float)(-System.Math.Cos(elevationRad) * System.Math.Sin(azimuthRad)),
				(float)System.Math.Sin(elevationRad),
				(float)(-System.Math.Cos(elevationRad) * System.Math.Cos(azimuthRad)));
		}

		/// <summary>Whether the menu system is initialized</summary>
		public bool IsInitialized = false;

		/// <summary>The key used to go back within the menu stack</summary>
		public Key MenuBackKey;

		/// <summary>Creates a new menu instance</summary>
		protected AbstractMenu(BaseRenderer renderer, BaseOptions currentOptions)
		{
			Renderer = renderer;
			CurrentOptions = currentOptions;
		}

		/// <summary>Initializes the menu system upon first use</summary>
		public abstract void Initialize();

		/// <summary>Resets the menu system to it's initial condition</summary>
		public virtual void Reset()
		{
			CurrMenu = -1;
			Menus = new MenuBase[] { };
		}

		/// <summary>Pushes a menu into the menu stack</summary>
		/// <param name= "type">The type of menu to push</param>
		/// <param name= "data">The index of the menu in the menu stack (If pushing an existing higher level menu)</param>
		/// <param name="replace">Whether we are replacing the selected menu item</param>
		public abstract void PushMenu(MenuType type, int data = 0, bool replace = false);

		/// <summary>Pops the previous menu in the menu stack</summary>
		public void PopMenu()
		{
			if (CurrMenu > 0)           // if more than one menu remaining...
			{
				CurrMenu--;             // ...back to previous menu
				if (CurrMenu > 0 && Menus[CurrMenu] == null)
				{
					// HACK: choose train dialog with not found train, needs special reset to get back to route list correctly
					CurrMenu--;
					Menus[CurrMenu].TopItem = 1;
					Menus[CurrMenu].Selection = -1;
				}
				ComputePosition();
			}
			else
			{                           // if only one menu remaining...
				Reset();
				Renderer.CurrentInterface = InterfaceType.Normal;  // return to simulation
				if (IsSidebarMode)
				{
					SidebarVisible = false;
					startOffset = currentOffset;
					animationElapsed = 0.0;
					isAnimating = true;
				}
			}
		}

		/// <summary>Processes a scroll wheel event</summary>
		/// <param name="Scroll">The delta</param>
		public virtual void ProcessMouseScroll(int Scroll)
		{
			if (Menus.Length == 0)
			{
				return;
			}
			if (CurrMenu >= 0 && Menus[CurrMenu].Type == MenuType.Options && OptionsTabContainer != null && OptionsTabContainer.IsVisible)
			{
				OptionsTabContainer.MouseWheel(Scroll);
				return;
			}
			// Load the current menu
			Menus[CurrMenu].ProcessScroll(Scroll, visibleItems);
		}

		/// <summary>Whether the menu system is configured as a sliding sidebar on the left</summary>
		public bool IsSidebarMode = true;

		/// <summary>Whether the sidebar is currently open/visible</summary>
		public bool SidebarVisible = false;

		/// <summary>The width of the sidebar as a fraction of screen width</summary>
		public double SidebarWidthRatio = 0.25;

		/// <summary>Gets the current calculated width of the sidebar panel</summary>
		public double SidebarWidth => Renderer.Screen.Width * SidebarWidthRatio;

		public double currentOffset = -99999.0;
		public double animationElapsed = 0.0;
		public double animationDuration = 0.4;
		public double startOffset = 0.0;
		public bool isAnimating = false;

		public bool isScrubbing = false;
		public MenuOption scrubbingOption = null;
		public int lastMouseX = 0;

		public Vector4 GetToggleButtonRect()
		{
			double buttonX = IsSidebarMode ? (currentOffset < -90000.0 ? (SidebarVisible ? SidebarWidth : 0) : (currentOffset + SidebarWidth)) : 0;
			double buttonY = Renderer.Screen.Height / 2.0 - 20;
			return new Vector4((float)buttonX, (float)buttonY, 24f, 40f);
		}

		/// <summary>The width (in px) of the grab/hover zone on the sidebar's right edge</summary>
		private const float SidebarResizeZone = 8f;

		/// <summary>Whether the user is currently dragging the sidebar edge to resize it</summary>
		public bool isResizingSidebar = false;

		/// <summary>Whether the mouse is currently hovering the sidebar resize edge (0..1 for highlight fade)</summary>
		public float sidebarResizeHover = 0f;

		/// <summary>Whether the sidebar resize cursor is currently applied (avoids redundant OS cursor updates)</summary>
		private bool sidebarResizeCursorActive = false;

		/// <summary>Shows the horizontal-resize cursor while on the sidebar edge, restoring the default otherwise</summary>
		private void SetSidebarResizeCursor(bool onEdge)
		{
			if (onEdge == sidebarResizeCursorActive) return;
			sidebarResizeCursorActive = onEdge;
			Renderer.SetCursor(onEdge ? AvailableCursors.GetResizeCursor() : OpenTK.MouseCursor.Default);
		}

		/// <summary>The screen X co-ordinate of the sidebar's right edge, or -1 if not currently shown</summary>
		public double SidebarEdgeX => IsSidebarMode && SidebarVisible ? (currentOffset < -90000.0 ? (SidebarVisible ? SidebarWidth : 0) : (currentOffset + SidebarWidth)) : -1;

		/// <summary>The background width of the current menu panel</summary>
		/// <remarks>In sidebar mode the panel ends exactly at the resize edge, so no
		/// border overhang is drawn past the grip line. Classic menus keep the border padding.</remarks>
		public double MenuBackgroundWidth =>
			IsSidebarMode ? menuMax.X - menuMin.X : menuMax.X - menuMin.X + 2.0f * Border.X;

		/// <summary>Tests whether the given point is within the sidebar resize grip</summary>
		private bool IsOnResizeEdge(int x, int y)
		{
			if (!IsSidebarMode || !SidebarVisible) return false;
			double edge = SidebarEdgeX;
			if (edge < 0) return false;
			return x >= edge - SidebarResizeZone && x <= edge + SidebarResizeZone && y >= 0 && y <= Renderer.Screen.Height;
		}

		/// <summary>Processes a mouse up event</summary>

		/// <summary>The current fade amount of the sidebar (0 = fully hidden, 1 = fully shown)</summary>
		public double SidebarFade = 0.0;

		public void UpdateTransition(double TimeElapsed)
		{
			if (!IsSidebarMode) return;

			double target = SidebarVisible ? 0 : -SidebarWidth;
			if (currentOffset < -90000.0)
			{
				currentOffset = target;
				startOffset = target;
				isAnimating = false;
			}

			if (isAnimating)
			{
				animationElapsed += TimeElapsed;
				double x = System.Math.Min(animationElapsed / animationDuration, 1.0);
				double progress = 1.0 - System.Math.Pow(1.0 - x, 3.0); // easeOutCubic
				currentOffset = startOffset + (target - startOffset) * progress;
				// Fade follows the same eased progress so the colour transition is smooth
				SidebarFade = SidebarVisible ? progress : 1.0 - progress;

				if (x >= 1.0)
				{
					currentOffset = target;
					SidebarFade = SidebarVisible ? 1.0 : 0.0;
					isAnimating = false;
					if (!SidebarVisible)
					{
						Reset();
					}
				}
			}
			else
			{
				currentOffset = target;
				SidebarFade = SidebarVisible ? 1.0 : 0.0;
			}

			menuMin.X = currentOffset;
			menuMax.X = menuMin.X + SidebarWidth;

			RepositionSidebarControls();
		}

		public virtual void RepositionSidebarControls()
		{
			if (OptionsTabContainer != null)
			{
				OptionsTabContainer.Location = new Vector2((float)menuMin.X + 16f, 50f);
				float panelWidth = (float)SidebarWidth - 32f;
				OptionsTabContainer.Size = new Vector2(panelWidth, (float)Renderer.Screen.Height - 100f);
				// Inner width available to a row after the scroll container's padding and scrollbar
				float innerWidth = panelWidth - 24f - 8f;

				foreach (var tab in OptionsTabContainer.TabContents)
				{
					if (tab is GLContainer container)
					{
						// First pass: the widest control in this tab sets the column width,
						// so every control in the tab lines up on both edges.
						const float minLabelWidth = 40f;
						float maxControlWidth = 0f;
						foreach (var child in container.Children)
						{
							if (child is GLHBoxContainer row && row.Children.Count >= 2)
							{
								var control = row.Children[1];
								float need = System.Math.Min(control.DesiredWidth, System.Math.Max(0f, innerWidth - row.Spacing - minLabelWidth));
								if (need > maxControlWidth) maxControlWidth = need;
							}
						}
						foreach (var child in container.Children)
						{
							if (child is GLHBoxContainer row)
							{
								// Row height follows the current UI font so labels/controls never clip
								float rowHeight = Renderer.Fonts.NormalFont.FontSize + 10f;
								row.Size = new Vector2(innerWidth, rowHeight);
								if (row.Children.Count >= 2)
								{
									var label = row.Children[0] as Label;
									var control = row.Children[1];
									// Fixed two-column layout: labels on the left, controls right-aligned.
									// Every control uses the tab's widest need; the label takes the rest,
									// so narrow panels no longer starve the label while the control cell sits empty.
									float labelWidth = System.Math.Max(0f, innerWidth - row.Spacing - maxControlWidth);
									if (label != null)
									{
										label.Size = new Vector2(labelWidth, rowHeight);
									}
									control.Size = new Vector2(maxControlWidth, rowHeight);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>Processes a mouse move event</summary>
		/// <param name="x">The screen-relative x coordinate of the move event</param>
		/// <param name="y">The screen-relative y coordinate of the move event</param>
		public virtual bool ProcessMouseMove(int x, int y)
		{
			return true;
		}

		/// <summary>Toggles the sidebar open/closed, mirroring the on-screen toggle button</summary>
		public void ToggleSidebar()
		{
			if (!IsSidebarMode) return;
			SidebarVisible = !SidebarVisible;
			startOffset = currentOffset;
			animationElapsed = 0.0;
			isAnimating = true;
			if (SidebarVisible)
			{
				if (CurrMenu == -1)
				{
					PushMenu(MenuType.Options);
				}
				else
				{
					Renderer.CurrentInterface = InterfaceType.Menu;
				}
			}
			else
			{
				Renderer.CurrentInterface = InterfaceType.Normal;
				SetSidebarResizeCursor(false);
			}
			ComputePosition();
		}

		/// <summary>Processes a mouse down event</summary>
		/// <param name="x">The screen-relative x coordinate of the down event</param>
		/// <param name="y">The screen-relative y coordinate of the down event</param>
		public void ProcessMouseDown(int x, int y)
		{
			if (IsSidebarMode)
			{
				if (IsOnResizeEdge(x, y))
				{
					isResizingSidebar = true;
					return;
				}
				Vector4 rect = GetToggleButtonRect();
				if (x >= rect.X && x <= rect.X + rect.Z && y >= rect.Y && y <= rect.Y + rect.W)
				{
					ToggleSidebar();
					return;
				}
			}
			if (GLControl.FocusedControl is GLNumericUpDown focusedNumber && !focusedNumber.HitTest(x, y))
			{
				// Clicking away from a numeric field commits the value and releases keyboard focus
				focusedNumber.CommitEdit();
			}
			if (CurrMenu >= 0 && Menus.Length > 0 && Menus[CurrMenu].Type == MenuType.Options)
			{
				if (OptionsTabContainer != null && OptionsTabContainer.IsVisible)
				{
					OptionsTabContainer.MouseDown(x, y);
					return;
				}
			}
			if (CurrMenu >= 0 && Menus.Length > 0)
			{
				var menu = Menus[CurrMenu];
				if (menu.Selection >= 0 && menu.Selection < menu.Items.Length)
				{
					var entry = menu.Items[menu.Selection];
					if (entry is MenuOption opt && (opt.Type == OptionType.ViewingDistance || opt.Type == OptionType.NearClip))
					{
						isScrubbing = true;
						scrubbingOption = opt;
						lastMouseX = x;
					}
				}
			}

			for (int i = 0; i < menuControls.Count; i++)
			{
				if (menuControls[i].IsVisible)
				{
					menuControls[i].MouseDown(x, y);
				}
			}
			if (ProcessMouseMove(x, y))
			{
				if (Menus[CurrMenu].Selection == Menus[CurrMenu].TopItem + visibleItems)
				{
					ProcessCommand(Translations.Command.MenuDown, 0);
					return;
				}
				if (Menus[CurrMenu].Selection == Menus[CurrMenu].TopItem - 1)
				{
					ProcessCommand(Translations.Command.MenuUp, 0);
					return;
				}
				// If scrubbing, do not trigger entry actions
				if (!isScrubbing)
				{
					ProcessCommand(Translations.Command.MenuEnter, 0);
				}
			}
		}

		/// <summary>Processes a mouse up event</summary>
		/// <summary>Handles hover highlighting and edge-drag resizing of the sidebar. Returns true if the event was consumed.</summary>
		/// <param name="x">The screen-relative x coordinate</param>
		/// <param name="y">The screen-relative y coordinate</param>
		public bool HandleSidebarResizeMove(int x, int y)
		{
			if (!IsSidebarMode || !SidebarVisible)
			{
				sidebarResizeHover = 0f;
				SetSidebarResizeCursor(false);
				return false;
			}
			if (isResizingSidebar)
			{
				double ratio = (double)x / Renderer.Screen.Width;
				SidebarWidthRatio = System.Math.Max(0.15, System.Math.Min(ratio, 0.5));
				ComputePosition();
				SetSidebarResizeCursor(true);
				return true;
			}
			bool onEdge = IsOnResizeEdge(x, y);
			sidebarResizeHover = onEdge ? 1f : 0f;
			SetSidebarResizeCursor(onEdge);
			return false;
		}

		/// <summary>Draws the sidebar resize grip / highlight on the right edge</summary>
		public void DrawSidebarResizeGrip()
		{
			if (!IsSidebarMode || !SidebarVisible) return;
			// Use the same edge as the hit-test and toggle button, so the grip
			// stays under the mouse while dragging / animating
			double edge = SidebarEdgeX;
			if (edge < 0) return;
			float fade = (float)System.Math.Max(SidebarFade, 0.001);
			float baseAlpha = 0.5f * fade;
			float alpha = (0.5f + 0.5f * sidebarResizeHover) * fade;
			float screenHeight = (float)Renderer.Screen.Height;
			// Leave a clean gap where the toggle tab sits, so it reads as a handle plugged into the edge
			Vector4 tab = GetToggleButtonRect();
			float gapTop = (float)System.Math.Max(0.0, tab.Y);
			float gapBottom = (float)System.Math.Min((double)screenHeight, tab.Y + tab.W);
			float upperHeight = gapTop;
			float lowerHeight = screenHeight - gapBottom;
			// Always-visible grip line so the user knows the edge is draggable
			Renderer.Rectangle.Draw(null, new Vector2((float)(edge - 1), 0f), new Vector2(2f, upperHeight), new Color128(0.0f, 0.47f, 0.83f, baseAlpha));
			Renderer.Rectangle.Draw(null, new Vector2((float)(edge - 1), gapBottom), new Vector2(2f, lowerHeight), new Color128(0.0f, 0.47f, 0.83f, baseAlpha));
			if (sidebarResizeHover > 0f)
			{
				Renderer.Rectangle.Draw(null, new Vector2((float)(edge - 3), 0f), new Vector2(6f, upperHeight), new Color128(0.0f, 0.47f, 0.83f, 0.4f * alpha));
				Renderer.Rectangle.Draw(null, new Vector2((float)(edge - 3), gapBottom), new Vector2(6f, lowerHeight), new Color128(0.0f, 0.47f, 0.83f, 0.4f * alpha));
				Renderer.Rectangle.Draw(null, new Vector2((float)edge - 0.5f, 0f), new Vector2(1.5f, upperHeight), new Color128(0.0f, 0.47f, 0.83f, alpha));
				Renderer.Rectangle.Draw(null, new Vector2((float)edge - 0.5f, gapBottom), new Vector2(1.5f, lowerHeight), new Color128(0.0f, 0.47f, 0.83f, alpha));
			}
		}

		public void ProcessMouseUp(int x, int y)
		{
			isScrubbing = false;
			scrubbingOption = null;
			isResizingSidebar = false;
			SetSidebarResizeCursor(IsOnResizeEdge(x, y));
			if (CurrMenu >= 0 && Menus.Length > 0 && Menus[CurrMenu].Type == MenuType.Options)
			{
				if (OptionsTabContainer != null && OptionsTabContainer.IsVisible)
				{
					OptionsTabContainer.MouseUp(x, y);
					return;
				}
			}
			for (int i = 0; i < menuControls.Count; i++)
			{
				if (menuControls[i].IsVisible)
				{
					menuControls[i].MouseUp(x, y);
				}
			}
		}

		/// <summary>Processes a user command for the current menu</summary>
		/// <param name="cmd">The command to apply to the current menu</param>
		/// <param name="timeElapsed">The time elapsed since previous frame</param>
		public virtual void ProcessCommand(Translations.Command cmd, double timeElapsed)
		{

		}

		/// <summary>Processes a file drop event</summary>
		public virtual void DragFile(object sender, OpenTK.Input.FileDropEventArgs e)
		{

		}

		/// <summary>Computes the position in the screen of the current menu.</summary>
		/// <remarks>Also sets the menu size</remarks>
		public void ComputePosition()
		{
			if (CurrMenu < 0 || CurrMenu >= Menus.Length)
				return;

			MenuBase menu = Menus[CurrMenu];
			if (OptionsTabContainer != null)
			{
				OptionsTabContainer.IsVisible = (menu.Type == MenuType.Options);
			}
			for (int i = 0; i < menu.Items.Length; i++)
			{
				/*
				 * HACK: This is a property method, and is also used to
				 * reset the timer and display string back to the starting values
				 */
				if (menu.Items[i] != null)
				{
					menu.Items[i].DisplayLength = menu.Items[i].DisplayLength;
				}
			}

			if (IsSidebarMode)
			{
				menu.Align = TextAlignment.TopLeft;
				menu.Width = SidebarWidth - 2.0 * Border.X;
				menu.ItemWidth = menu.Width;

				menuMin.X = SidebarVisible ? 0 : -SidebarWidth;
				menuMax.X = menuMin.X + SidebarWidth;
				menuMin.Y = 0;
				menuMax.Y = Renderer.Screen.Height;
				topItemY = Border.Y;
				visibleItems = (int)(Renderer.Screen.Height - Border.Y * 2) / LineHeight;

				RepositionSidebarControls();
			}
			else
			{
				// HORIZONTAL PLACEMENT
				switch (menu.Align)
				{
					case TextAlignment.TopLeft:
						// Left aligned
						menuMin.X = 0;
						break;
					default:
						// Centered in window
						menuMin.X = (Renderer.Screen.Width - menu.Width) / 2;     // menu left edge (border excluded)	
						break;
				}

				menuMax.X = menuMin.X + menu.Width;               // menu right edge (border excluded)
																  // VERTICAL PLACEMENT: centre the menu in the main window
				menuMin.Y = (Renderer.Screen.Height - menu.Height) / 2;       // menu top edge (border excluded)
				menuMax.Y = menuMin.Y + menu.Height;              // menu bottom edge (border excluded)
				topItemY = menuMin.Y;                                // top edge of top item
																	 // assume all items fit in the screen
				visibleItems = menu.Items.Length;

				// if there are more items than can fit in the screen height,
				// (there should be at least room for the menu top border)
				if (menuMin.Y < Border.Y)
				{
					// the number of lines which fit in the screen
					int numOfLines = (int)(Renderer.Screen.Height - Border.Y * 2) / LineHeight;
					visibleItems = numOfLines - 2;                  // at least an empty line at the top and at the bottom
																	// split the menu in chunks of 'visibleItems' items
																	// and display the chunk which contains the currently selected item
					menu.TopItem = menu.Selection - (menu.Selection % visibleItems);
					visibleItems = menu.Items.Length - menu.TopItem < visibleItems ?    // in the last chunk,
						menu.Items.Length - menu.TopItem : visibleItems;                // display remaining items only
					menuMin.Y = (Renderer.Screen.Height - numOfLines * LineHeight) / 2.0;
					menuMax.Y = menuMin.Y + numOfLines * LineHeight;
					// first menu item is drawn on second line (first line is empty
					// on first screen and contains an ellipsis on following screens
					topItemY = menuMin.Y + LineHeight;
				}
			}
		}

		public void DrawSidebarToggleButton(double RealTimeElapsed)
		{
			if (!IsSidebarMode) return;
			Vector4 rect = GetToggleButtonRect();
			Renderer.Rectangle.Draw(null, new Vector2(rect.X, rect.Y), new Vector2(rect.Z, rect.W), backgroundColor);
			var icon = SidebarVisible ? Renderer.SidebarCollapseTexture : Renderer.SidebarExpandTexture;
			if (icon != null)
			{
				// 16px icon centered in the 24x40 tab; fall back to a text glyph if the asset is missing
				Renderer.Rectangle.Draw(icon, new Vector2(rect.X + 4.0, rect.Y + 12.0), new Vector2(16.0, 16.0), Color128.White);
				return;
			}
			string arrow = SidebarVisible ? "<" : ">";
			if (MenuFont == null)
			{
				MenuFont = Renderer.Fonts.NormalFont;
			}
			Renderer.OpenGlString.Draw(MenuFont, arrow, new Vector2(rect.X + 8, rect.Y + 10), TextAlignment.TopLeft, ColourNormal, false);
		}

		public MenuEntry[] CreateOptionsEntries(HostApplication host)
		{
			List<MenuEntry> items = new List<MenuEntry>();
			items.Add(new MenuCaption(this, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "panel", "options" })));
			
			items.Add(new MenuOption(this, OptionType.ScreenResolution, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "resolution" }), Renderer.Screen.AvailableResolutions.ToArray()));
			items.Add(new MenuOption(this, OptionType.FullScreen, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "display_mode_fullscreen" }), new[] { "true", "false" }));
			
			items.Add(new MenuOption(this, OptionType.Interpolation, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation" }), new[]
			{
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearest"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinear"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearestmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinearmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_trilinearmipmap"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_anisotropic"})
			}));
			
			items.Add(new MenuOption(this, OptionType.AnisotropicLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_anisotropic_level" }), new[] { "0", "2", "4", "8", "16" }));
			items.Add(new MenuOption(this, OptionType.AntialiasingLevel, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_antialiasing_level" }), new[] { "0", "2", "4", "8", "16" }));
			
			items.Add(new MenuOption(this, OptionType.TransparencyQuality, "Transparency Quality", new[] { "Sharp", "Intermediate", "Smooth" }));
			
			items.Add(new MenuOption(this, OptionType.ViewingDistance, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_distance_viewingdistance" }), new[] { "400", "600", "800", "1000", "1500", "2000" }));
			items.Add(new MenuOption(this, OptionType.NearClip, "Near clip", new[] { "0.001", "0.01", "0.05", "0.1", "0.2", "0.5" }));

			if (host == HostApplication.OpenBve)
			{
				items.Add(new MenuOption(this, OptionType.UIScaleFactor, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "ui_scalefactor" }), new[] { "1x", "2x", "3x", "4x", "5x", "6x" }));
				items.Add(new MenuOption(this, OptionType.NumberOfSounds, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "misc_sound_number" }), new[] { "16", "32", "64", "128"}));
				items.Add(new MenuOption(this, OptionType.ShadowQuality, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "shadows_resolution" }), new[]
				{
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_off"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_low"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_medium"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_high"}),
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","shadows_resolution_ultra"})
				}));
				items.Add(new MenuOption(this, OptionType.ShadowFilterCascades, "Per-cascade culling", new[] { "true", "false" }));
			}

			items.Add(new MenuOption(this, OptionType.NewXParser, "Use New X Parser", new[] { "Original", "NewXParser", "Assimp" }));
			items.Add(new MenuOption(this, OptionType.NewObjParser, "Use New Obj Parser", new[] { "Original", "Assimp" }));

			if (host == HostApplication.ObjectViewer)
			{
				items.Add(new MenuOption(this, OptionType.AutoReloadObjects, "Automatically Reload Objects", new[] { "true", "false" }));
			}
			
			if (host == HostApplication.RouteViewer)
			{
				items.Add(new MenuOption(this, OptionType.ShowLogo, "Show Logo", new[] { "true", "false" }));
				items.Add(new MenuOption(this, OptionType.ShowBackgrounds, "Show Backgrounds", new[] { "true", "false" }));
				items.Add(new MenuOption(this, OptionType.ShowProgressBar, "Show Progress Bar", new[] { "true", "false" }));
			}

			items.Add(new MenuCommand(this, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "back" }), MenuTag.MenuBack, 0));
			return items.ToArray();
		}

		/// <summary>Called when a menu option changes</summary>
		/// <param name="type">The option type that changed</param>
		public virtual void OnOptionChanged(OptionType type)
		{
		}

		/// <summary>Processes raw key down events for numeric option entry</summary>
		/// <param name="key">The keyboard key pressed</param>
		public virtual void ProcessKeyDown(OpenTK.Input.Key key)
		{
			if (GLControl.FocusedControl != null && GLControl.FocusedControl.IsVisible)
			{
				GLControl.FocusedControl.KeyDown(key);
				return;
			}
			if (CurrMenu >= 0 && Menus.Length > 0 && Menus[CurrMenu].Type == MenuType.Options && ProcessOptionsKey(key))
			{
				return;
			}
			if (CurrMenu < 0 || Menus.Length == 0) return;
			var menu = Menus[CurrMenu];
			if (menu.Selection >= 0 && menu.Selection < menu.Items.Length)
			{
				var selectedEntry = menu.Items[menu.Selection];
				if (selectedEntry is MenuOption opt)
				{
					opt.ProcessKeyDown(key);
				}
			}
		}

		/// <summary>Draws the current menu</summary>
		/// <param name="RealTimeElapsed">The real time elapsed since the last draw call</param>
		/// <remarks>Note that the real time elapsed may be different to the game time elapsed</remarks>
		public abstract void Draw(double RealTimeElapsed);
	}
}
