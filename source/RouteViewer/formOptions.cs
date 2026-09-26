using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenTK.Graphics;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenBveApi.Math;

namespace RouteViewer
{
    public partial class FormOptions : Form
    {
        private bool suppressSunEvents = false;
        private bool suppressShadowEvents = false;

        public FormOptions()
        {
            TopMost = true;
            FormClosed += FormOptions_FormClosed;
            InitializeComponent();
            InterpolationMode.SelectedIndex = (int) Interface.CurrentOptions.Interpolation;
            AnisotropicLevel.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
            AntialiasingLevel.Value = Interface.CurrentOptions.AntiAliasingLevel;
            TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance ? 0 : 2;
            width.Value = Program.Renderer.Screen.Width;
            height.Value = Program.Renderer.Screen.Height;
			checkBoxLogo.Checked = Interface.CurrentOptions.LoadingLogo;
			checkBoxBackgrounds.Checked = Interface.CurrentOptions.LoadingBackground;
			checkBoxProgressBar.Checked = Interface.CurrentOptions.LoadingProgressBar;
			comboBoxNewXParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentXParser;
			comboBoxNewObjParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentObjParser;
			comboBoxOptimizeObjects.SelectedIndex = (int)Interface.CurrentOptions.ObjectOptimizationMode;
			numericUpDownViewingDistance.Value = Math.Min(Interface.CurrentOptions.ViewingDistance, numericUpDownViewingDistance.Maximum);

            // Shadows
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

            numericUpDownShadowStrength.Minimum = 1;
            numericUpDownShadowStrength.Maximum = 100;
            numericUpDownShadowStrength.Increment = 5;
            numericUpDownShadowStrength.DecimalPlaces = 0;
            numericUpDownShadowStrength.Value = (decimal)Math.Round(Interface.CurrentOptions.ShadowStrength * 100.0);
            if (numericUpDownShadowStrength.Value < 1) numericUpDownShadowStrength.Value = 1;
            numericUpDownShadowStrength.Refresh();
            numericUpDownShadowBias.Value = (decimal)Interface.CurrentOptions.ShadowBias;
            numericUpDownShadowBias.Refresh();

            numericUpDownShadowNormalBias.DecimalPlaces = 2;
            numericUpDownShadowNormalBias.Minimum = 0;
            numericUpDownShadowNormalBias.Maximum = 10;
            numericUpDownShadowNormalBias.Increment = 0.1m;
            numericUpDownShadowNormalBias.Value = (decimal)Interface.CurrentOptions.ShadowNormalBias;
            numericUpDownShadowNormalBias.Refresh();


            // Initialize sun direction sliders from current light position
            InitializeSunSliders();

            // Wire up shadow resolution change to enable/disable related controls
            comboBoxShadowResolution.SelectedIndexChanged += comboBoxShadowResolution_SelectedIndexChanged;
            UpdateShadowControlsEnabled();
			numericUpDownViewingDistance.Value = Math.Min(Interface.CurrentOptions.ViewingDistance, numericUpDownViewingDistance.Maximum);
			numericUpDownNearClip.Value = (decimal)Interface.CurrentOptions.NearClipBase;
			if (Translations.CurrentLanguageCode != "en-US")
			{
				labelNearClip.Text = Translations.GetInterfaceString(OpenBveApi.Hosts.HostApplication.OpenBve, new[] { "options", "quality_distance_nearclip" });
			}
			checkBoxShadowFilterCascades.Checked = Interface.CurrentOptions.ShadowFilterCascades;

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
        }



        private void FormOptions_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Closing (X or OK) always keeps the live-tweaked sun + shadow
            // values in memory. OK additionally writes .cfg and may trigger
            // a deferred reload; X skips both.
            Program.OptionsDialog = null;
            // Presents the kept state and clears paused input.
            // (Must run after OptionsDialog is cleared so the loop resumes.)
            Program.ExitOptionsPause();
        }

        private void InitializeSunSliders()
        {
            suppressSunEvents = true;
            // Initial position follows the route (live renderer state), not the persisted options.
            // Memory-only sync; .cfg is written on OK only.
            try
            {
                var livePos = Program.Renderer.Lighting.OptionLightPosition;
                double len = Math.Sqrt(livePos.X * livePos.X + livePos.Y * livePos.Y + livePos.Z * livePos.Z);
                if (len >= 1e-6)
                {
                    double ny = Math.Max(-1.0, Math.Min(1.0, livePos.Y / len));
                    double elevation = Math.Asin(ny) * 180.0 / Math.PI;
                    double azimuth = Math.Atan2(-livePos.X / len, -livePos.Z / len) * 180.0 / Math.PI;
                    if (azimuth > 180.0) azimuth -= 360.0;
                    if (azimuth < -180.0) azimuth += 360.0;
                    Interface.CurrentOptions.LightAzimuth = Math.Max(-180.0, Math.Min(180.0, azimuth));
                    Interface.CurrentOptions.LightElevation = Math.Max(-90.0, Math.Min(90.0, elevation));
                }
            }
            catch
            {
                // Keep persisted options on failure
            }
			trackBarSunElevation.Value = Math.Max(trackBarSunElevation.Minimum, Math.Min((int)Interface.CurrentOptions.LightElevation, trackBarSunElevation.Maximum));
			trackBarSunAzimuth.Value = Math.Max(trackBarSunAzimuth.Minimum, Math.Min((int)Interface.CurrentOptions.LightAzimuth, trackBarSunAzimuth.Maximum));
			labelSunAzimuthValue.Text = trackBarSunAzimuth.Value + "\u00b0";
            labelSunElevationValue.Text = trackBarSunElevation.Value + "\u00b0";
            suppressSunEvents = false;
        }

        private void SetupSunRealtime()
        {
            // Memory-only updates + PreviewDirty flag for the render loop,
            // plus a best-effort immediate present (see TryPresentPreview).
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
            // Paused loop renders on-demand: flag one preview frame, and
            // best-effort present immediately (covers a blocked loop too).
            Program.PreviewDirty = true;
            TryPresentPreview();
        }

        internal static Vector3 SunVectorFromOptions()
        {
            double azimuthRad = Interface.CurrentOptions.LightAzimuth * Math.PI / 180.0;
            double elevationRad = Interface.CurrentOptions.LightElevation * Math.PI / 180.0;
            float x = (float)(-Math.Cos(elevationRad) * Math.Sin(azimuthRad));
            float y = (float)Math.Sin(elevationRad);
            float z = (float)(-Math.Cos(elevationRad) * Math.Cos(azimuthRad));
            return new Vector3(x, y, z);
        }

        internal static void ApplySunToRoute(Vector3 pos)
        {
            try
            {
                if (Program.CurrentRoute != null)
                {
                    if (Program.CurrentRoute.LightDefinitions != null)
                    {
                        for (int i = 0; i < Program.CurrentRoute.LightDefinitions.Length; i++)
                        {
                            Program.CurrentRoute.LightDefinitions[i].LightPosition = pos;
                        }
                    }
                    if (Program.CurrentRoute.Atmosphere != null)
                    {
                        Program.CurrentRoute.Atmosphere.LightPosition = pos;
                    }
                }
            }
            catch
            {
                // Best-effort
            }
        }

        private void UpdateShadowControlsEnabled()
        {
            bool enabled = comboBoxShadowResolution.SelectedIndex != 0;
            comboBoxShadowDistance.Enabled = enabled;
            comboBoxShadowCascades.Enabled = enabled;
            numericUpDownShadowStrength.Enabled = enabled;
            numericUpDownShadowBias.Enabled = enabled;
            numericUpDownShadowBias.ReadOnly = !enabled;
            numericUpDownShadowNormalBias.Enabled = enabled;
            numericUpDownShadowNormalBias.ReadOnly = !enabled;

            // Sun position is independent of shadows and must stay enabled for realtime preview
            trackBarSunAzimuth.Enabled = true;
            trackBarSunElevation.Enabled = true;
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
            // there); handlers below also best-effort present immediately.
            // Strength / bias / filter are read live from options every frame,
            // so they need no flag at all.
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
            TryPresentPreview();
        }

        private void ApplyShadowTweaks()
        {
            // Consumed live by the shaders every frame; no reload needed.
            // (Paused loop renders on-demand, so still flag one preview frame.)
            ReadShadowTweaks();
            Program.PreviewDirty = true;
            TryPresentPreview();
        }

        private void TryPresentPreview()
        {
            // Best-effort immediate present, in addition to PreviewDirty.
            // Same render thread owns the GL context here (UpdateGraphicsSettings
            // already does RenderScene + SwapBuffers from this thread), so this
            // is safe; any failure silently falls back to the flag path.
            try
            {
                if (Program.Renderer == null || Program.Renderer.GameWindow == null || Program.CurrentlyLoading)
                {
                    return;
                }
                if (!Program.Renderer.GameWindow.Exists || Program.Renderer.GameWindow.IsExiting)
                {
                    return;
                }
                Program.Renderer.RenderScene(0.0);
                Program.Renderer.GameWindow.SwapBuffers();
            }
            catch
            {
                // Best-effort; the render loop path still applies
            }
        }

        private void UpdateSunDirection()
        {
            Interface.CurrentOptions.LightAzimuth = trackBarSunAzimuth.Value;
            Interface.CurrentOptions.LightElevation = trackBarSunElevation.Value;

            Vector3 pos = SunVectorFromOptions();
            Program.Renderer.Lighting.OptionLightPosition = pos;
            // RouteViewer re-computes lighting every frame from LightDefinitions,
            // so propagate the manual sun there too, otherwise the next frame overwrites the preview
            ApplySunToRoute(pos);
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
            Program.CaptureOptionsSnapshot();
            FormOptions optionsDialog = new FormOptions();
            Program.OptionsDialog = optionsDialog;
            try
            {
                optionsDialog.Show();
            }
            catch
            {
                Program.OptionsDialog = null;
                throw;
            }
            Program.EnterOptionsPause();
        }

        private void formOptions_Shown(object sender, EventArgs e)
        {
            button1.Focus();
        }

	    private readonly int previousAntialiasingLevel = Interface.CurrentOptions.AntiAliasingLevel;
	    private readonly int previousAnisotropicLevel = Interface.CurrentOptions.AnisotropicFilteringLevel;
	    private readonly int previousViewingDistance = Interface.CurrentOptions.ViewingDistance;
	    private readonly double previousNearClipBase = Interface.CurrentOptions.NearClipBase;
	    private bool GraphicsModeChanged = false;

        private void button1_Click(object sender, EventArgs e)
        {
            // Ensure latest slider values are in options before the full save below
            // (this is the only place that writes options_rv.cfg)
            Interface.CurrentOptions.LightAzimuth = trackBarSunAzimuth.Value;
            Interface.CurrentOptions.LightElevation = trackBarSunElevation.Value;
            UpdateSunDirection();

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
            Interface.CurrentOptions.AntiAliasingLevel = (int)AntialiasingLevel.Value;
            if (Interface.CurrentOptions.AntiAliasingLevel != previousAntialiasingLevel)
            {
                Program.Renderer.GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
	            GraphicsModeChanged = true;
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
			Interface.CurrentOptions.LoadingLogo = checkBoxLogo.Checked;
			Interface.CurrentOptions.LoadingBackground = checkBoxBackgrounds.Checked;
			Interface.CurrentOptions.LoadingProgressBar = checkBoxProgressBar.Checked;
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
			Interface.CurrentOptions.ViewingDistance = (int)numericUpDownViewingDistance.Value;
			Interface.CurrentOptions.ObjectOptimizationMode = (ObjectOptimizationMode)comboBoxOptimizeObjects.SelectedIndex;
			Interface.CurrentOptions.NearClipBase = (double)numericUpDownNearClip.Value;
			// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
			if (Interface.CurrentOptions.ViewingDistance <= Interface.CurrentOptions.NearClipBase)

			{
				Interface.CurrentOptions.ViewingDistance = (int)Math.Ceiling(Interface.CurrentOptions.NearClipBase) + 1;
			}
			Interface.CurrentOptions.QuadTreeLeafSize = Math.Max(50, (int)Math.Ceiling(Interface.CurrentOptions.ViewingDistance / 10.0d) * 10); // quad tree size set to 10% of viewing distance to the nearest 10

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

			// VSync and FPS Limit
			Interface.CurrentOptions.VerticalSynchronization = comboBoxVSync.SelectedIndex == 1;
			// Map combo index to FPSLimit value
			int[] fpsPresets = { 0, 30, 60, 120, 240 };
			Interface.CurrentOptions.FPSLimit = comboBoxFPSLimit.SelectedIndex >= 0 ? fpsPresets[comboBoxFPSLimit.SelectedIndex] : 0;
			Program.Renderer.GameWindow.VSync = Interface.CurrentOptions.VerticalSynchronization ? OpenTK.VSyncMode.On : OpenTK.VSyncMode.Off;
			Program.Renderer.GameWindow.TargetRenderFrequency = Interface.CurrentOptions.FPSLimit > 0 ? Interface.CurrentOptions.FPSLimit : 0;

            // Sun direction is already updated in real-time via slider events


			Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_rv.cfg"));
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Object != null)
				{
					Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentXParser);
					Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentObjParser);
				}
			}
			//Check if interpolation mode or anisotropic filtering level has changed, and trigger a reload
			//(Shadow state is compared against the pre-dialog snapshot: the
			// dialog mutates options live, so CurrentOptions no longer holds
			// the old values here.)
			bool shadowChanged = Program.PrevShadowResolution != Interface.CurrentOptions.ShadowResolution || Program.PrevShadowDistance != Interface.CurrentOptions.ShadowDrawDistance || Program.PrevShadowCascades != Interface.CurrentOptions.ShadowCascades ||
			    Program.PrevShadowStrength != Interface.CurrentOptions.ShadowStrength || Program.PrevShadowBias != Interface.CurrentOptions.ShadowBias || Program.PrevShadowNormalBias != Interface.CurrentOptions.ShadowNormalBias ||
			    Program.PrevShadowFilterCascades != Interface.CurrentOptions.ShadowFilterCascades;
			if (previousInterpolationMode != Interface.CurrentOptions.Interpolation || previousAnisotropicLevel != Interface.CurrentOptions.AnisotropicFilteringLevel || GraphicsModeChanged || Interface.CurrentOptions.ViewingDistance != previousViewingDistance ||
			    shadowChanged ||
			    Interface.CurrentOptions.NearClipBase != previousNearClipBase)
			{
				// Deferred: the render loop runs the post-OK reload at the top
				// of its next frame (never nested inside a WinForms dispatch).
				Program.PendingOptionsCommit = true;
			}
			else
			{
				// No heavy reload needed; the live values are already applied.
			}
			Close();

        }

	    protected override void OnClosing(CancelEventArgs cancelEventArgs)
	    {
			
	    }
    }
}
