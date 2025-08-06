using System;
using System.Windows.Forms;
using LibRender2.Viewports;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Input;
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
		}

		internal static DialogResult ShowOptions()
		{
			formOptions Dialog = new formOptions();
			DialogResult Result = Dialog.ShowDialog();
			return Result;
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			int previousAntialasingLevel = Interface.CurrentOptions.AntiAliasingLevel;

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
			if (Interface.CurrentOptions.AntiAliasingLevel != previousAntialasingLevel)
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
			Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg"));
			Program.RefreshObjects();
			Close();
		}
	}
}
