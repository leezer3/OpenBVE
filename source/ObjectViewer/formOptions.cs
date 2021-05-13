using System;
using System.Windows.Forms;
using ObjectViewer.Graphics;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using OpenTK.Graphics;

namespace OpenBve
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
		}

		internal static DialogResult ShowOptions()
		{
			formOptions Dialog = new formOptions();
			DialogResult Result = Dialog.ShowDialog();
			return Result;
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			InterpolationMode previousInterpolationMode = Interface.CurrentOptions.Interpolation;
			int previousAntialasingLevel = Interface.CurrentOptions.AntiAliasingLevel;
			int previousAnsiotropicLevel = Interface.CurrentOptions.AnisotropicFilteringLevel;

			//Interpolation mode
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

			//Ansiotropic filtering level
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int) AnsiotropicLevel.Value;
			//Antialiasing level
			Interface.CurrentOptions.AntiAliasingLevel = (int) AntialiasingLevel.Value;
			if (Interface.CurrentOptions.AntiAliasingLevel != previousAntialasingLevel)
			{
				Program.currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
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
				if (width.Value >= 300)
				{
					Program.Renderer.Screen.Width = (int) width.Value;
					Program.currentGameWindow.Width = (int) width.Value;
				}

				if (height.Value >= 300)
				{
					Program.Renderer.Screen.Height = (int) height.Value;
					Program.currentGameWindow.Height = (int) height.Value;
				}

				Program.Renderer.UpdateViewport();
			}

			Interface.CurrentOptions.CurrentXParser = (XParsers) comboBoxNewXParser.SelectedIndex;
			Interface.CurrentOptions.CurrentObjParser = (ObjParsers) comboBoxNewObjParser.SelectedIndex;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Object != null)
				{
					Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentXParser);
					Program.CurrentHost.Plugins[i].Object.SetObjectParser(Interface.CurrentOptions.CurrentObjParser);
				}
			}

			Interface.CurrentOptions.ObjectOptimizationMode = (ObjectOptimizationMode)comboBoxOptimizeObjects.SelectedIndex;

			Options.SaveOptions();
			Program.RefreshObjects();
			this.Close();
		}
	}
}
