using System;
using System.Windows.Forms;
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
            TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == Renderer.TransparencyMode.Sharp ? 0 : 2;
            width.Value = Renderer.ScreenWidth;
            height.Value = Renderer.ScreenHeight;
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

        private void button1_Click(object sender, EventArgs e)
        {
            TextureManager.InterpolationMode previousInterpolationMode = Interface.CurrentOptions.Interpolation;
            int previousAntialasingLevel = Interface.CurrentOptions.AntialiasingLevel;
            int previousAnsiotropicLevel = Interface.CurrentOptions.AnisotropicFilteringLevel;

            //Interpolation mode
            switch (InterpolationMode.SelectedIndex)
            {
                case 0:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighbor;
                    break;
                case 1:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.Bilinear;
                    break;
                case 2:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighborMipmapped;
                    break;
                case 3:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped;
                    break;
                case 4:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.TrilinearMipmapped;
                    break;
                case 5:
                    Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.AnisotropicFiltering;
                    break;
            }
            //Ansiotropic filtering level
            Interface.CurrentOptions.AnisotropicFilteringLevel = (int) AnsiotropicLevel.Value;
            //Antialiasing level
            Interface.CurrentOptions.AntialiasingLevel = (int)AntialiasingLevel.Value;
            if (Interface.CurrentOptions.AntialiasingLevel != previousAntialasingLevel)
            {
                Program.currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntialiasingLevel);
            }
            //Transparency quality
            switch (TransparencyQuality.SelectedIndex)
            {
                case 0:
                    Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp;
                    break;
                default:
                    Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth;
                    break;
            }
            //Set width and height
            if (Renderer.ScreenWidth != width.Value || Renderer.ScreenHeight != height.Value)
            {
                Renderer.ScreenWidth = (int) width.Value;
                Renderer.ScreenHeight = (int) height.Value;
                Program.currentGameWindow.Width = (int) width.Value;
                Program.currentGameWindow.Height = (int) height.Value;
                Program.UpdateViewport();
            }
            //Check if interpolation mode or ansiotropic filtering level has changed, and trigger a reload
            if (previousInterpolationMode != Interface.CurrentOptions.Interpolation || previousAnsiotropicLevel != Interface.CurrentOptions.AnisotropicFilteringLevel)
            {
                if (Program.CurrentRoute != null)
                {
                    Program.CurrentlyLoading = true;
                    Renderer.RenderScene(0.0);
                    Program.currentGameWindow.SwapBuffers();
                    World.CameraAlignment a = World.CameraCurrentAlignment;
                    if (Program.LoadRoute())
                    {
                        World.CameraCurrentAlignment = a;
                        TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
                        TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, a.TrackPosition, true, false);
                        World.CameraAlignmentDirection = new World.CameraAlignment();
                        World.CameraAlignmentSpeed = new World.CameraAlignment();
                        ObjectManager.UpdateVisibility(a.TrackPosition, true);
                        ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
                    }
                    Program.CurrentlyLoading = false;
                }
            }
            Renderer.TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == Renderer.TransparencyMode.Smooth & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.Bilinear;
            Options.SaveOptions();
            this.Dispose();
        }
    }
}
