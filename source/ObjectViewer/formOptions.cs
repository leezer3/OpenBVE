using System;
using System.Windows.Forms;
using LibRender2.Viewports;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Input;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenTK.Graphics;

namespace ObjectViewer
{
	public partial class formOptions : Form
	{
		private formOptions()
		{
			InitializeComponent();
			InterpolationMode.SelectedIndex = (int) Interface.CurrentOptions.Interpolation;
			AnsiotropicLevel.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
			AntialiasingLevel.Value = Interface.CurrentOptions.AntiAliasingLevel;
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
			if (numericUpDownShadowBias.Value == 0)
			{
				numericUpDownShadowBias.Value = 0.000050m;
			}


			// Initialize sun direction sliders from current light position
			InitializeSunSliders();

			// Wire up shadow resolution change to enable/disable related controls
			comboBoxShadowResolution.SelectedIndexChanged += comboBoxShadowResolution_SelectedIndexChanged;
			UpdateShadowControlsEnabled();

			comboBoxLeft.DataSource = Enum.GetValues(typeof(Key));
			comboBoxLeft.SelectedItem = Interface.CurrentOptions.CameraMoveLeft;
			comboBoxRight.DataSource = Enum.GetValues(typeof(Key));
			comboBoxRight.SelectedItem = Interface.CurrentOptions.CameraMoveRight;
			comboBoxUp.DataSource = Enum.GetValues(typeof(Key));
			comboBoxUp.SelectedItem = Interface.CurrentOptions.CameraMoveUp;
			comboBoxDown.DataSource = Enum.GetValues(typeof(Key));
			comboBoxDown.SelectedItem = Interface.CurrentOptions.CameraMoveDown;
			comboBoxForwards.DataSource = Enum.GetValues(typeof(Key));
			comboBoxForwards.SelectedItem = Interface.CurrentOptions.CameraMoveForward;
			comboBoxBackwards.DataSource = Enum.GetValues(typeof(Key));
			comboBoxBackwards.SelectedItem = Interface.CurrentOptions.CameraMoveBackward;
			checkBoxAutoReload.Checked = Interface.CurrentOptions.AutoReloadObjects;
			checkBoxParallel.Checked = Interface.CurrentOptions.ParallelRouteLoading;
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
			bool enabled = comboBoxShadowResolution.SelectedIndex != 0; // 0 = Off
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

			// Convert spherical to direction vector
			float x = (float)(Math.Sin(azimuthRad) * Math.Cos(elevationRad));
			float y = (float)(Math.Sin(elevationRad));
			float z = (float)(Math.Cos(azimuthRad) * Math.Cos(elevationRad));

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
			formOptions Dialog = new formOptions();
			DialogResult Result = Dialog.ShowDialog();
			return Result;
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
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

			//Ansiotropic filtering level
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int) AnsiotropicLevel.Value;
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
			

			Interface.CurrentOptions.ObjectOptimizationMode = (ObjectOptimizationMode)comboBoxOptimizeObjects.SelectedIndex;
			Interface.CurrentOptions.CameraMoveLeft = (Key)comboBoxLeft.SelectedItem;
			Interface.CurrentOptions.CameraMoveRight = (Key)comboBoxRight.SelectedItem;
			Interface.CurrentOptions.CameraMoveUp = (Key)comboBoxUp.SelectedItem;
			Interface.CurrentOptions.CameraMoveDown = (Key)comboBoxDown.SelectedItem;
			Interface.CurrentOptions.CameraMoveForward = (Key)comboBoxForwards.SelectedItem;
			Interface.CurrentOptions.CameraMoveBackward = (Key)comboBoxBackwards.SelectedItem;
			Interface.CurrentOptions.AutoReloadObjects = checkBoxAutoReload.Checked;
			Interface.CurrentOptions.ParallelRouteLoading = checkBoxParallel.Checked;

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
			
			Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg"));
			Program.RefreshObjects();
			this.DialogResult = DialogResult.OK;
			Close();
		}
	}
}
