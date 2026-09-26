using System;
using System.Linq;
using System.Windows.Forms;
using LibRender2.Viewports;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Input;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenTK.Graphics;

namespace ObjectViewer
{
	public partial class formOptions : Form
	{
		private bool suppressSunEvents = false;
		private bool suppressShadowEvents = false;
		private bool committed = false;

		// Snapshot of the live (memory) sun + shadow state at dialog open.
		// The dialog mutates options live for realtime preview (.cfg is only
		// written on OK), so closing without OK must restore these.
		private double initialAzimuth;
		private double initialElevation;
		private ShadowMapResolution initialShadowResolution;
		private ShadowDistance initialShadowDistance;
		private ShadowCascadeCount initialShadowCascades;
		private double initialShadowStrength;
		private double initialShadowBias;
		private double initialShadowNormalBias;
		private bool initialShadowFilterCascades;
		private Vector3 initialOptionLightPosition;
		private bool initialShowGround;
		private double initialGroundHeight;
		private OpenBveApi.Colors.Color24 initialGroundColor;

		private formOptions()
		{
			// Must run before any control init that syncs live state.
			CaptureSnapshot();
			TopMost = true;
			FormClosed += formOptions_FormClosed;
			InitializeComponent();
			InterpolationMode.SelectedIndex = (int) Interface.CurrentOptions.Interpolation;
			AnisotropicLevel.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
			AntialiasingLevel.Value = Interface.CurrentOptions.AntiAliasingLevel;
			nearClip.Value = (decimal)Interface.CurrentOptions.NearClipBase;
			if (Translations.CurrentLanguageCode != "en-US")
			{
				labelNearClip.Text = Translations.GetInterfaceString(OpenBveApi.Hosts.HostApplication.OpenBve, new[] { "options", "quality_distance_nearclip" });
			}
			TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance ? 0 : 2;
			width.Value = Program.Renderer.Screen.Width;
			height.Value = Program.Renderer.Screen.Height;
			comboBoxNewXParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentXParser;
			comboBoxNewObjParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentObjParser;
			comboBoxOptimizeObjects.SelectedIndex = (int)Interface.CurrentOptions.ObjectOptimizationMode;
			
			// Loading current shadow settings
			switch (Interface.CurrentOptions.ShadowResolution)
			{
				case ShadowMapResolution.Off: comboBoxShadowResolution.SelectedIndex = 0; break;
				case ShadowMapResolution.Low: comboBoxShadowResolution.SelectedIndex = 1; break;
				case ShadowMapResolution.Medium: comboBoxShadowResolution.SelectedIndex = 2; break;
				case ShadowMapResolution.High: comboBoxShadowResolution.SelectedIndex = 3; break;
				case ShadowMapResolution.Ultra: comboBoxShadowResolution.SelectedIndex = 4; break;
				default: comboBoxShadowResolution.SelectedIndex = 3; break;
			}

			switch (Interface.CurrentOptions.ShadowDrawDistance)
			{
				case ShadowDistance.Near: comboBoxShadowDistance.SelectedIndex = 0; break;
				case ShadowDistance.Medium: comboBoxShadowDistance.SelectedIndex = 1; break;
				case ShadowDistance.Far: comboBoxShadowDistance.SelectedIndex = 2; break;
				case ShadowDistance.VeryFar: comboBoxShadowDistance.SelectedIndex = 3; break;
				case ShadowDistance.ViewingDistance: comboBoxShadowDistance.SelectedIndex = 4; break;
				default: comboBoxShadowDistance.SelectedIndex = 1; break;
			}

			switch (Interface.CurrentOptions.ShadowCascades)
			{
				case ShadowCascadeCount.Two: comboBoxShadowCascades.SelectedIndex = 0; break;
				case ShadowCascadeCount.Three: comboBoxShadowCascades.SelectedIndex = 1; break;
				case ShadowCascadeCount.Four: comboBoxShadowCascades.SelectedIndex = 2; break;
				default: comboBoxShadowCascades.SelectedIndex = 1; break;
			}

			numericUpDownShadowStrength.Value = (decimal)(Interface.CurrentOptions.ShadowStrength * 100.0);
			numericUpDownShadowBias.Value = (decimal)Interface.CurrentOptions.ShadowBias;
			numericUpDownShadowNormalBias.Value = (decimal)Interface.CurrentOptions.ShadowNormalBias;


			// Initialize sun direction sliders from current light position
			InitializeSunSliders();

			// Wire up shadow resolution change to enable/disable related controls
			comboBoxShadowResolution.SelectedIndexChanged += comboBoxShadowResolution_SelectedIndexChanged;
			UpdateShadowControlsEnabled();

			BindKey(comboBoxLeft, Interface.CurrentOptions.CameraMoveLeft, Key.A);
			BindKey(comboBoxRight, Interface.CurrentOptions.CameraMoveRight, Key.D);
			BindKey(comboBoxUp, Interface.CurrentOptions.CameraMoveUp, Key.W);
			BindKey(comboBoxDown, Interface.CurrentOptions.CameraMoveDown, Key.S);
			BindKey(comboBoxForwards, Interface.CurrentOptions.CameraMoveForward, Key.Q);
			BindKey(comboBoxBackwards, Interface.CurrentOptions.CameraMoveBackward, Key.E);
			checkBoxAutoReload.Checked = Interface.CurrentOptions.AutoReloadObjects;
			checkBoxProgressBar.Checked = Interface.CurrentOptions.LoadingProgressBar;
			checkBoxShadowFilterCascades.Checked = Interface.CurrentOptions.ShadowFilterCascades;
			checkBoxShowGround.Checked = Interface.CurrentOptions.ShowGround;
			numericUpDownGroundHeight.Value = Math.Max(numericUpDownGroundHeight.Minimum, Math.Min((decimal)Interface.CurrentOptions.GroundHeight, numericUpDownGroundHeight.Maximum));
			buttonGroundColor.BackColor = Interface.CurrentOptions.GroundColor;

			// VSync and FPS Limit
			comboBoxVSync.SelectedIndex = Interface.CurrentOptions.VerticalSynchronization ? 1 : 0;
			// Map FPSLimit value to combo index: 0=Unlimited, 1=30, 2=60, 3=120, 4=240
			switch (Interface.CurrentOptions.FPSLimit)
			{
				case 30: comboBoxFPSLimit.SelectedIndex = 1; break;
				case 60: comboBoxFPSLimit.SelectedIndex = 2; break;
				case 120: comboBoxFPSLimit.SelectedIndex = 3; break;
				case 240: comboBoxFPSLimit.SelectedIndex = 4; break;
				default: comboBoxFPSLimit.SelectedIndex = 0; break;
			}
			UpdateFPSLimitEnabled();
			// Wire realtime handlers last, after every control reflects the
			// current options, so init itself never flags live changes.
			SetupSunRealtime();
			SetupShadowRealtime();
			SetupGroundRealtime();
		}

		/// <summary>Captures the live sun + shadow state so Cancel / X can restore it.</summary>
		private void CaptureSnapshot()
		{
			initialAzimuth = Interface.CurrentOptions.LightAzimuth;
			initialElevation = Interface.CurrentOptions.LightElevation;
			initialShadowResolution = Interface.CurrentOptions.ShadowResolution;
			initialShadowDistance = Interface.CurrentOptions.ShadowDrawDistance;
			initialShadowCascades = Interface.CurrentOptions.ShadowCascades;
			initialShadowStrength = Interface.CurrentOptions.ShadowStrength;
			initialShadowBias = Interface.CurrentOptions.ShadowBias;
			initialShadowNormalBias = Interface.CurrentOptions.ShadowNormalBias;
			initialShadowFilterCascades = Interface.CurrentOptions.ShadowFilterCascades;
			initialShowGround = Interface.CurrentOptions.ShowGround;
			initialGroundHeight = Interface.CurrentOptions.GroundHeight;
			initialGroundColor = Interface.CurrentOptions.GroundColor;
			try
			{
				initialOptionLightPosition = Program.Renderer.Lighting.OptionLightPosition;
			}
			catch
			{
				// Best-effort
			}
		}

		/// <summary>Restores the snapshot (memory only; shadow realloc happens on the render thread).</summary>
		private void RestoreSnapshot()
		{
			Interface.CurrentOptions.LightAzimuth = initialAzimuth;
			Interface.CurrentOptions.LightElevation = initialElevation;
			Interface.CurrentOptions.ShadowResolution = initialShadowResolution;
			Interface.CurrentOptions.ShadowDrawDistance = initialShadowDistance;
			Interface.CurrentOptions.ShadowCascades = initialShadowCascades;
			Interface.CurrentOptions.ShadowStrength = initialShadowStrength;
			Interface.CurrentOptions.ShadowBias = initialShadowBias;
			Interface.CurrentOptions.ShadowNormalBias = initialShadowNormalBias;
			Interface.CurrentOptions.ShadowFilterCascades = initialShadowFilterCascades;
			Interface.CurrentOptions.ShowGround = initialShowGround;
			Interface.CurrentOptions.GroundHeight = initialGroundHeight;
			Interface.CurrentOptions.GroundColor = initialGroundColor;
			try
			{
				Program.Renderer.RefreshGround();
			}
			catch
			{
				// Best-effort
			}
			try
			{
				if (Program.Renderer != null)
				{
					Program.Renderer.Lighting.OptionLightPosition = initialOptionLightPosition;
				}
			}
			catch
			{
				// Best-effort
			}
			Program.ShadowSettingsDirty = true;
		}

		private void formOptions_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (!committed)
			{
				RestoreSnapshot();
			}
			Program.OptionsDialog = null;
		}

		private void InitializeSunSliders()
		{
			suppressSunEvents = true;
			trackBarSunElevation.Value = Math.Max(trackBarSunElevation.Minimum, Math.Min((int)Interface.CurrentOptions.LightElevation, trackBarSunElevation.Maximum));
			trackBarSunAzimuth.Value = Math.Max(trackBarSunAzimuth.Minimum, Math.Min((int)Interface.CurrentOptions.LightAzimuth, trackBarSunAzimuth.Maximum));
			labelSunAzimuthValue.Text = trackBarSunAzimuth.Value + "\u00b0";
			labelSunElevationValue.Text = trackBarSunElevation.Value + "\u00b0";
			suppressSunEvents = false;
		}

		private void SetupSunRealtime()
		{
			// Memory-only: the sliders update Interface.CurrentOptions +
			// renderer lighting. The running render loop picks that up on its
			// next frame, so no GL calls are ever made from this dialog.
			// (No .cfg write here; persisted to disk on OK only.)
			trackBarSunAzimuth.ValueChanged += SunSlider_Changed;
			trackBarSunElevation.ValueChanged += SunSlider_Changed;
		}

		private void SunSlider_Changed(object sender, EventArgs e)
		{
			if (suppressSunEvents)
			{
				return;
			}
			labelSunAzimuthValue.Text = trackBarSunAzimuth.Value + "\u00b0";
			labelSunElevationValue.Text = trackBarSunElevation.Value + "\u00b0";
			UpdateSunDirection();
		}

		private void SetupGroundRealtime()
		{
			// Memory-only; .cfg is written on OK. The loop renders
			// continuously, so the ground updates live on the next frame.
			checkBoxShowGround.CheckedChanged += GroundSetting_Changed;
			numericUpDownGroundHeight.ValueChanged += GroundSetting_Changed;
			buttonGroundColor.Click += buttonGroundColor_Click;
		}

		private void GroundSetting_Changed(object sender, EventArgs e)
		{
			ApplyGroundRealtime();
		}

		private void buttonGroundColor_Click(object sender, EventArgs e)
		{
			try
			{
				using (ColorDialog dialog = new ColorDialog())
				{
					dialog.FullOpen = true;
					dialog.Color = buttonGroundColor.BackColor;
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						buttonGroundColor.BackColor = dialog.Color;
						ApplyGroundRealtime();
					}
				}
			}
			catch
			{
				// ColorDialog is not available on every platform; the ground
				// color can still be set via options_ov.cfg (groundcolor = #RRGGBB)
			}
		}

		private void ApplyGroundRealtime()
		{
			Interface.CurrentOptions.ShowGround = checkBoxShowGround.Checked;
			Interface.CurrentOptions.GroundHeight = (double)numericUpDownGroundHeight.Value;
			Interface.CurrentOptions.GroundColor = new OpenBveApi.Colors.Color24(buttonGroundColor.BackColor.R, buttonGroundColor.BackColor.G, buttonGroundColor.BackColor.B);
			try
			{
				Program.Renderer.RefreshGround();
			}
			catch
			{
				// Best-effort; the loop reads options live anyway
			}
		}

		private void UpdateShadowControlsEnabled()
		{
			bool enabled = comboBoxShadowResolution.SelectedIndex != 0; // 0 = Off
			comboBoxShadowDistance.Enabled = enabled;
			comboBoxShadowCascades.Enabled = enabled;
			numericUpDownShadowStrength.Enabled = enabled;
			numericUpDownShadowBias.Enabled = enabled;
			numericUpDownShadowBias.ReadOnly = !enabled;
			numericUpDownShadowNormalBias.Enabled = enabled;
			numericUpDownShadowNormalBias.ReadOnly = !enabled;
			
			checkBoxShadowFilterCascades.Enabled = enabled;
		}

		private bool shadowDropDownOpen = false;

		private void comboBoxShadowResolution_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateShadowControlsEnabled();
			ApplyShadowMapSizeOrDefer();
		}

		private void SetupShadowRealtime()
		{
			// Memory-only; .cfg is written on OK. Resolution / distance /
			// cascade count only flag the render thread (GPU realloc must run
			// there, never on this dialog). Strength / bias / filter are
			// read live from options every frame, so they need no flag at all.
			comboBoxShadowDistance.SelectedIndexChanged += ShadowMapSetting_Changed;
			comboBoxShadowCascades.SelectedIndexChanged += ShadowMapSetting_Changed;
			numericUpDownShadowStrength.ValueChanged += ShadowValue_Changed;
			numericUpDownShadowBias.ValueChanged += ShadowValue_Changed;
			numericUpDownShadowNormalBias.ValueChanged += ShadowValue_Changed;
			checkBoxShadowFilterCascades.CheckedChanged += ShadowValue_Changed;
			// Defer the realloc while a size list is open: arrowing through
			// the dropdown fires SelectedIndexChanged per step, and each
			// realloc would stall this shared UI/render thread for a few ms.
			comboBoxShadowResolution.DropDown += ShadowDropDown_Opened;
			comboBoxShadowResolution.DropDownClosed += ShadowDropDown_Closed;
			comboBoxShadowDistance.DropDown += ShadowDropDown_Opened;
			comboBoxShadowDistance.DropDownClosed += ShadowDropDown_Closed;
			comboBoxShadowCascades.DropDown += ShadowDropDown_Opened;
			comboBoxShadowCascades.DropDownClosed += ShadowDropDown_Closed;
		}

		private void ShadowDropDown_Opened(object sender, EventArgs e)
		{
			shadowDropDownOpen = true;
		}

		private void ShadowDropDown_Closed(object sender, EventArgs e)
		{
			shadowDropDownOpen = false;
			ApplyShadowMapSize();
		}

		/// <summary>Memory-only while a size list is open; single realloc on close.</summary>
		private void ApplyShadowMapSizeOrDefer()
		{
			if (shadowDropDownOpen)
			{
				ReadShadowMapSize();
				ReadShadowTweaks();
				return;
			}
			ApplyShadowMapSize();
		}

		private void ShadowMapSetting_Changed(object sender, EventArgs e)
		{
			if (suppressShadowEvents)
			{
				return;
			}
			ApplyShadowMapSizeOrDefer();
		}

		private void ShadowValue_Changed(object sender, EventArgs e)
		{
			if (suppressShadowEvents)
			{
				return;
			}
			ApplyShadowTweaks();
		}

		private void ReadShadowMapSize()
		{
			switch (comboBoxShadowResolution.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Off; break;
				case 1: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Low; break;
				case 2: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Medium; break;
				case 3: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.High; break;
				case 4: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Ultra; break;
			}
			switch (comboBoxShadowDistance.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Near; break;
				case 1: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Medium; break;
				case 2: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Far; break;
				case 3: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.VeryFar; break;
				case 4: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.ViewingDistance; break;
			}
			switch (comboBoxShadowCascades.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Two; break;
				case 1: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Three; break;
				case 2: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Four; break;
			}
		}

		private void ReadShadowTweaks()
		{
			Interface.CurrentOptions.ShadowStrength = (double)numericUpDownShadowStrength.Value / 100.0;
			Interface.CurrentOptions.ShadowBias = (double)numericUpDownShadowBias.Value;
			Interface.CurrentOptions.ShadowNormalBias = (double)numericUpDownShadowNormalBias.Value;
			Interface.CurrentOptions.ShadowFilterCascades = checkBoxShadowFilterCascades.Checked;
		}

		private void ApplyShadowMapSize()
		{
			// Reallocates GPU shadow maps: flag only, the render thread
			// performs the reload at the top of its next frame. Skipped when
			// nothing actually changed (e.g. list opened and closed as-is).
			ShadowMapResolution oldResolution = Interface.CurrentOptions.ShadowResolution;
			ShadowDistance oldDistance = Interface.CurrentOptions.ShadowDrawDistance;
			ShadowCascadeCount oldCascades = Interface.CurrentOptions.ShadowCascades;
			ReadShadowMapSize();
			ReadShadowTweaks();
			if (Interface.CurrentOptions.ShadowResolution != oldResolution ||
				Interface.CurrentOptions.ShadowDrawDistance != oldDistance ||
				Interface.CurrentOptions.ShadowCascades != oldCascades)
			{
				Program.ShadowSettingsDirty = true;
			}
		}

		private void ApplyShadowTweaks()
		{
			// Consumed live by the shaders every frame; no reload needed.
			ReadShadowTweaks();
		}

		private void UpdateSunDirection()
		{
			Interface.CurrentOptions.LightAzimuth = trackBarSunAzimuth.Value;
			Interface.CurrentOptions.LightElevation = trackBarSunElevation.Value;

			double azimuthRad = Interface.CurrentOptions.LightAzimuth * Math.PI / 180.0;
			double elevationRad = Interface.CurrentOptions.LightElevation * Math.PI / 180.0;

			// Convert spherical to direction vector (matching DirectionalLight docs)
			float x = (float)(-Math.Cos(elevationRad) * Math.Sin(azimuthRad));
			float y = (float)Math.Sin(elevationRad);
			float z = (float)(-Math.Cos(elevationRad) * Math.Cos(azimuthRad));

			Program.Renderer.Lighting.OptionLightPosition = new Vector3(x, y, z);
		}

		private void UpdateFPSLimitEnabled()
		{
			bool vsyncEnabled = comboBoxVSync.SelectedIndex == 1;
			comboBoxFPSLimit.Enabled = !vsyncEnabled;
			labelFPSLimit.ForeColor = vsyncEnabled ? System.Drawing.SystemColors.GrayText : System.Drawing.SystemColors.ControlText;
		}

		private void comboBoxVSync_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateFPSLimitEnabled();
		}

		private void trackBarSunAzimuth_Scroll(object sender, EventArgs e)
		{
			SunSlider_Changed(sender, e);
		}

		private void trackBarSunElevation_Scroll(object sender, EventArgs e)
		{
			SunSlider_Changed(sender, e);
		}

		private static void BindKey(ComboBox box, Key value, Key fallback)
		{
			box.DropDownStyle = ComboBoxStyle.DropDownList;
			box.DataSource = new[] { Key.Disabled }.Concat(Enum.GetValues(typeof(Key)).Cast<Key>().Where(k => k != Key.LastKey && k != Key.Disabled).Distinct().ToList()).ToList();
			box.SelectedItem = box.Items.Contains(value) ? value : fallback;
		}

		internal static void ShowOptions()
		{
			// Modeless: the render loop keeps running behind the dialog, so
			// slider changes preview live on the next frame. The dialog only
			// touches plain memory; all GL work runs on the render thread,
			// which keeps this safe on Linux / macOS too. (A blocking
			// ShowDialog would starve the render loop, and rendering from the
			// nested WinForms pump is what breaks non-Windows platforms.)
			if (Program.OptionsDialog != null && !Program.OptionsDialog.IsDisposed)
			{
				try
				{
					Program.OptionsDialog.BringToFront();
					Program.OptionsDialog.Focus();
				}
				catch
				{
					// Best-effort
				}
				return;
			}
			formOptions Dialog = new formOptions();
			Program.OptionsDialog = Dialog;
			try
			{
				Dialog.Show();
			}
			catch
			{
				Program.OptionsDialog = null;
				throw;
			}
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			// Ensure latest slider values are in options before the full save below
			// (this is the only place that writes options_ov.cfg)
			Interface.CurrentOptions.LightAzimuth = trackBarSunAzimuth.Value;
			Interface.CurrentOptions.LightElevation = trackBarSunElevation.Value;
			UpdateSunDirection();
			int previousAntialiasingLevel = Interface.CurrentOptions.AntiAliasingLevel;

			//Interpolation mode
			InterpolationMode previousInterpolationMode = Interface.CurrentOptions.Interpolation;
			switch (InterpolationMode.SelectedIndex)
			{
				case 0:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.NearestNeighbor;
					break;
				case 1:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.Bilinear;
					break;
				case 2:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.NearestNeighborMipmapped;
					break;
				case 3:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.BilinearMipmapped;
					break;
				case 4:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.TrilinearMipmapped;
					break;
				case 5:
					Interface.CurrentOptions.Interpolation = OpenBveApi.Graphics.InterpolationMode.AnisotropicFiltering;
					break;
			}

			if (previousInterpolationMode != Interface.CurrentOptions.Interpolation)
			{
				// We have changed interpolation level, so the texture cache needs totally clearing (as opposed to changed files)
				Program.Renderer.TextureManager.UnloadAllTextures(false);
			}

			//Anisotropic filtering level
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int) AnisotropicLevel.Value;
			//Antialiasing level
			Interface.CurrentOptions.AntiAliasingLevel = (int) AntialiasingLevel.Value;
			if (Interface.CurrentOptions.AntiAliasingLevel != previousAntialiasingLevel)
			{
				Program.Renderer.GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
			}

			//Transparency quality
			switch (TransparencyQuality.SelectedIndex)
			{
				case 0:
					Interface.CurrentOptions.TransparencyMode = TransparencyMode.Performance;
					break;
				default:
					Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality;
					break;
			}

			//Set width and height
			if (Program.Renderer.Screen.Width != width.Value || Program.Renderer.Screen.Height != height.Value)
			{
				if (width.Value > 300 && height.Value > 300)
				{
					Program.Renderer.SetWindowSize((int)width.Value, (int)height.Value);
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
				}
			}

			XParsers xParser = (XParsers)comboBoxNewXParser.SelectedIndex;
			ObjParsers objParser = (ObjParsers)comboBoxNewObjParser.SelectedIndex;

			if (Interface.CurrentOptions.CurrentXParser != xParser || Interface.CurrentOptions.CurrentObjParser != objParser)
			{
				Interface.CurrentOptions.CurrentXParser = xParser;
				Interface.CurrentOptions.CurrentObjParser = objParser;
				Program.CurrentHost.ClearObjectCaches(); // as a different parser may interpret differently
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Object != null)
					{
						Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentXParser);
						Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentObjParser);
					}
				}
			}
			

			Interface.CurrentOptions.ObjectOptimizationMode = (ObjectOptimizationMode)comboBoxOptimizeObjects.SelectedIndex;
			Interface.CurrentOptions.CameraMoveLeft = (Key)comboBoxLeft.SelectedItem;
			Interface.CurrentOptions.CameraMoveRight = (Key)comboBoxRight.SelectedItem;
			Interface.CurrentOptions.CameraMoveUp = (Key)comboBoxUp.SelectedItem;
			Interface.CurrentOptions.CameraMoveDown = (Key)comboBoxDown.SelectedItem;
			Interface.CurrentOptions.CameraMoveForward = (Key)comboBoxForwards.SelectedItem;
			Interface.CurrentOptions.CameraMoveBackward = (Key)comboBoxBackwards.SelectedItem;
			Interface.CurrentOptions.NearClipBase = (double)nearClip.Value;
			// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
			if (Interface.CurrentOptions.ViewingDistance <= Interface.CurrentOptions.NearClipBase)
			{
				Interface.CurrentOptions.ViewingDistance = (int)Math.Ceiling(Interface.CurrentOptions.NearClipBase) + 1;
			}
			Interface.CurrentOptions.AutoReloadObjects = checkBoxAutoReload.Checked;
			Interface.CurrentOptions.LoadingProgressBar = checkBoxProgressBar.Checked;
			Interface.CurrentOptions.ShowGround = checkBoxShowGround.Checked;
			Interface.CurrentOptions.GroundHeight = (double)numericUpDownGroundHeight.Value;
			Interface.CurrentOptions.GroundColor = new OpenBveApi.Colors.Color24(buttonGroundColor.BackColor.R, buttonGroundColor.BackColor.G, buttonGroundColor.BackColor.B);
			try
			{
				Program.Renderer.RefreshGround();
			}
			catch
			{
				// Best-effort; the loop reads options live anyway
			}

			// VSync and FPS Limit
			Interface.CurrentOptions.VerticalSynchronization = comboBoxVSync.SelectedIndex == 1;
			// Map combo index to FPSLimit value
			int[] fpsPresets = { 0, 30, 60, 120, 240 };
			Interface.CurrentOptions.FPSLimit = comboBoxFPSLimit.SelectedIndex >= 0 ? fpsPresets[comboBoxFPSLimit.SelectedIndex] : 0;
			Program.Renderer.GameWindow.VSync = Interface.CurrentOptions.VerticalSynchronization ? OpenTK.VSyncMode.On : OpenTK.VSyncMode.Off;
			Program.Renderer.GameWindow.TargetRenderFrequency = Interface.CurrentOptions.FPSLimit > 0 ? Interface.CurrentOptions.FPSLimit : 0;

			// Saving shadow settings
			switch (comboBoxShadowResolution.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Off; break;
				case 1: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Low; break;
				case 2: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Medium; break;
				case 3: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.High; break;
				case 4: Interface.CurrentOptions.ShadowResolution = ShadowMapResolution.Ultra; break;
			}

			switch (comboBoxShadowDistance.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Near; break;
				case 1: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Medium; break;
				case 2: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.Far; break;
				case 3: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.VeryFar; break;
				case 4: Interface.CurrentOptions.ShadowDrawDistance = ShadowDistance.ViewingDistance; break;
			}

			switch (comboBoxShadowCascades.SelectedIndex)
			{
				case 0: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Two; break;
				case 1: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Three; break;
				case 2: Interface.CurrentOptions.ShadowCascades = ShadowCascadeCount.Four; break;
			}

			Interface.CurrentOptions.ShadowStrength = (double)numericUpDownShadowStrength.Value / 100.0;
			Interface.CurrentOptions.ShadowBias = (double)numericUpDownShadowBias.Value;
			Interface.CurrentOptions.ShadowNormalBias = (double)numericUpDownShadowNormalBias.Value;
			Interface.CurrentOptions.ShadowFilterCascades = checkBoxShadowFilterCascades.Checked;
			
			Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg"));
			// Deferred: the render loop drains the shadow flag and refreshes
			// objects at the top of its next frame (never nested inside a
			// WinForms dispatch).
			committed = true;
			Program.PendingOptionsCommit = true;
			Close();
		}
	}
}
