using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenBveApi.Graphics;
using OpenTK.Graphics;

namespace OpenBve
{
    public partial class formOptions : Form
    {
        public formOptions()
        {
            InitializeComponent();
            InterpolationMode.SelectedIndex = (int) Interface.CurrentOptions.Interpolation;
            AnsiotropicLevel.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
            AntialiasingLevel.Value = Interface.CurrentOptions.AntialiasingLevel;
            TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance ? 0 : 2;
            width.Value = Renderer.ScreenWidth;
            height.Value = Renderer.ScreenHeight;
			checkBoxLogo.Checked = Interface.CurrentOptions.LoadingLogo;
			checkBoxBackgrounds.Checked = Interface.CurrentOptions.LoadingBackground;
			checkBoxProgressBar.Checked = Interface.CurrentOptions.LoadingProgressBar;
        }

        internal static DialogResult ShowOptions()
        {
            formOptions Dialog = new formOptions();
            DialogResult Result = Dialog.ShowDialog();
            return Result;
        }

        private void formOptions_Shown(object sender, EventArgs e)
        {
            button1.Focus();
        }

	    readonly int previousAntialasingLevel = Interface.CurrentOptions.AntialiasingLevel;
	    readonly int previousAnsiotropicLevel = Interface.CurrentOptions.AnisotropicFilteringLevel;
	    readonly InterpolationMode previousInterpolationMode = Interface.CurrentOptions.Interpolation;
	    readonly bool PreviousSort = Renderer.TransparentColorDepthSorting;
	    private bool GraphicsModeChanged = false;

        private void button1_Click(object sender, EventArgs e)
        {
            
            

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
            Interface.CurrentOptions.AntialiasingLevel = (int)AntialiasingLevel.Value;
            if (Interface.CurrentOptions.AntialiasingLevel != previousAntialasingLevel)
            {
                Program.currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntialiasingLevel);
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
			if (Renderer.ScreenWidth != width.Value || Renderer.ScreenHeight != height.Value)
			{
				Renderer.ScreenWidth = (int)width.Value;
				Renderer.ScreenHeight = (int)height.Value;
				Program.currentGameWindow.Width = (int)width.Value;
				Program.currentGameWindow.Height = (int)height.Value;
				Program.UpdateViewport();
			}
			Renderer.TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality & Interface.CurrentOptions.Interpolation != OpenBveApi.Graphics.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != OpenBveApi.Graphics.InterpolationMode.Bilinear;
			Interface.CurrentOptions.LoadingLogo = checkBoxLogo.Checked;
			Interface.CurrentOptions.LoadingBackground = checkBoxBackgrounds.Checked;
			Interface.CurrentOptions.LoadingProgressBar = checkBoxProgressBar.Checked;
			Options.SaveOptions();
			//Check if interpolation mode or ansiotropic filtering level has changed, and trigger a reload
			if (previousInterpolationMode != Interface.CurrentOptions.Interpolation || previousAnsiotropicLevel != Interface.CurrentOptions.AnisotropicFilteringLevel || PreviousSort != Renderer.TransparentColorDepthSorting || GraphicsModeChanged)
			{
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				this.DialogResult = DialogResult.Abort;
			}


        }

	    protected override void OnClosing(CancelEventArgs cancelEventArgs)
	    {
			
	    }
    }
}
