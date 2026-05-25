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
        public FormOptions()
        {
            InitializeComponent();
            InterpolationMode.SelectedIndex = (int) Interface.CurrentOptions.Interpolation;
            AnsiotropicLevel.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
            AntialiasingLevel.Value = Interface.CurrentOptions.AntiAliasingLevel;
            TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance ? 0 : 2;
            width.Value = Program.Renderer.Screen.Width;
            height.Value = Program.Renderer.Screen.Height;
			checkBoxLogo.Checked = Interface.CurrentOptions.LoadingLogo;
			checkBoxBackgrounds.Checked = Interface.CurrentOptions.LoadingBackground;
			checkBoxProgressBar.Checked = Interface.CurrentOptions.LoadingProgressBar;
			comboBoxNewXParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentXParser;
			comboBoxNewObjParser.SelectedIndex = (int) Interface.CurrentOptions.CurrentObjParser;
			numericUpDownViewingDistance.Value = Interface.CurrentOptions.ViewingDistance;

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
            if (numericUpDownShadowBias.Value == 0)
            {
                numericUpDownShadowBias.Value = 0.000050m;
            }
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
			numericUpDownViewingDistance.Value = (decimal)Interface.CurrentOptions.ViewingDistance;
			numericUpDownNearClip.Value = (decimal)Interface.CurrentOptions.NearClipBase;
			if (Translations.CurrentLanguageCode != "en-US")
			{
				labelNearClip.Text = Translations.GetInterfaceString(OpenBveApi.Hosts.HostApplication.OpenBve, new[] { "options", "quality_distance_nearclip" });
			}
        }

        private void InitializeSunSliders()
        {
            trackBarSunElevation.Value = (int)Interface.CurrentOptions.LightElevation;
            trackBarSunAzimuth.Value = (int)Interface.CurrentOptions.LightAzimuth;
            labelSunAzimuthValue.Text = trackBarSunAzimuth.Value + "\u00b0";
            labelSunElevationValue.Text = trackBarSunElevation.Value + "\u00b0";
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

            trackBarSunAzimuth.Enabled = enabled;
            trackBarSunElevation.Enabled = enabled;
        }

        private void comboBoxShadowResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateShadowControlsEnabled();
        }

        private void UpdateSunDirection()
        {
            Interface.CurrentOptions.LightAzimuth = trackBarSunAzimuth.Value;
            Interface.CurrentOptions.LightElevation = trackBarSunElevation.Value;

            double azimuthRad = Interface.CurrentOptions.LightAzimuth * Math.PI / 180.0;
            double elevationRad = Interface.CurrentOptions.LightElevation * Math.PI / 180.0;

            // Convert spherical to direction vector (matching DirectionalLight docs)
            float x = (float)(-Math.Cos(elevationRad) * Math.Sin(azimuthRad));
            float y = (float)(Math.Sin(elevationRad));
            float z = (float)(-Math.Cos(elevationRad) * Math.Cos(azimuthRad));

            Program.Renderer.Lighting.OptionLightPosition = new Vector3(x, y, z);
        }

        private void trackBarSunAzimuth_Scroll(object sender, EventArgs e)
        {
            labelSunAzimuthValue.Text = trackBarSunAzimuth.Value + "\u00b0";
            UpdateSunDirection();
        }

        private void trackBarSunElevation_Scroll(object sender, EventArgs e)
        {
            labelSunElevationValue.Text = trackBarSunElevation.Value + "\u00b0";
            UpdateSunDirection();
        }

        internal static DialogResult ShowOptions()
        {
            FormOptions optionsDialog = new FormOptions();
            DialogResult dialogResult = optionsDialog.ShowDialog();
            return dialogResult;
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
            ShadowMapResolution previousShadowResolution = Interface.CurrentOptions.ShadowResolution;
	        ShadowDistance previousShadowDistance = Interface.CurrentOptions.ShadowDrawDistance;
	        ShadowCascadeCount previousShadowCascades = Interface.CurrentOptions.ShadowCascades;
	        double previousShadowStrength = Interface.CurrentOptions.ShadowStrength;
	        double previousShadowBias = Interface.CurrentOptions.ShadowBias;
	        double previousShadowNormalBias = Interface.CurrentOptions.ShadowNormalBias;

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

			//Ansiotropic filtering level
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int) AnsiotropicLevel.Value;
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
				Program.CurrentHost.StaticObjectCache.Clear(); // as a different parser may interpret differently
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
			if (previousInterpolationMode != Interface.CurrentOptions.Interpolation || previousAnisotropicLevel != Interface.CurrentOptions.AnisotropicFilteringLevel || GraphicsModeChanged || Interface.CurrentOptions.ViewingDistance != previousViewingDistance ||
			    previousShadowResolution != Interface.CurrentOptions.ShadowResolution || previousShadowDistance != Interface.CurrentOptions.ShadowDrawDistance || previousShadowCascades != Interface.CurrentOptions.ShadowCascades ||
			    previousShadowStrength != Interface.CurrentOptions.ShadowStrength || previousShadowBias != Interface.CurrentOptions.ShadowBias || previousShadowNormalBias != Interface.CurrentOptions.ShadowNormalBias || 
			    Interface.CurrentOptions.NearClipBase != previousNearClipBase)
			{
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				this.DialogResult = DialogResult.Abort;
			}
			Close();

        }

	    protected override void OnClosing(CancelEventArgs cancelEventArgs)
	    {
			
	    }
    }
}
