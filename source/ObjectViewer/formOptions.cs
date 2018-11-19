using System;
using System.Windows.Forms;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;
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
            AntialiasingLevel.Value = Interface.CurrentOptions.AntialiasingLevel;
            TransparencyQuality.SelectedIndex = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance ? 0 : 2;
            width.Value = Renderer.ScreenWidth;
            height.Value = Renderer.ScreenHeight;
	        comboBoxNewXParser.SelectedIndex = Interface.CurrentOptions.UseNewXParser;
        }

        internal static DialogResult ShowOptions()
        {
            formOptions Dialog = new formOptions();
            DialogResult Result = Dialog.ShowDialog();
            return Result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InterpolationMode previousInterpolationMode = Interface.CurrentOptions.Interpolation;
            int previousAntialasingLevel = Interface.CurrentOptions.AntialiasingLevel;
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
            Interface.CurrentOptions.AntialiasingLevel = (int)AntialiasingLevel.Value;
            if (Interface.CurrentOptions.AntialiasingLevel != previousAntialasingLevel)
            {
                Program.currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntialiasingLevel);
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
                Renderer.ScreenWidth = (int) width.Value;
                Renderer.ScreenHeight = (int) height.Value;
                Program.currentGameWindow.Width = (int) width.Value;
                Program.currentGameWindow.Height = (int) height.Value;
                Program.UpdateViewport();
            }
            //Check if interpolation mode or ansiotropic filtering level has changed, and trigger a reload
            if (previousInterpolationMode != Interface.CurrentOptions.Interpolation || previousAnsiotropicLevel != Interface.CurrentOptions.AnisotropicFilteringLevel)
            {
                    Program.ReducedMode = false;
                    Program.LightingRelative = -1.0;
                    Game.Reset();
                    Textures.UnloadAllTextures();
                    Interface.ClearMessages();
                    for (int i = 0; i < Program.Files.Length; i++)
                    {
#if !DEBUG
									try {
#endif
                        ObjectManager.UnifiedObject o = ObjectManager.LoadObject(Program.Files[i], System.Text.Encoding.UTF8, ObjectLoadMode.Normal, false, false, false);
                        ObjectManager.CreateObject(o, Vector3.Zero,
                            new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0,
                            0.0, 25.0, 0.0);
#if !DEBUG
									} catch (Exception ex) {
										Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + Program.Files[i] + ".");
									}
#endif
                    }
                    ObjectManager.InitializeVisibility();
                    ObjectManager.UpdateVisibility(0.0, true);
                    ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
                    
            }
            Renderer.TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality & Interface.CurrentOptions.Interpolation != OpenBveApi.Graphics.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != OpenBveApi.Graphics.InterpolationMode.Bilinear;
	        Interface.CurrentOptions.UseNewXParser = comboBoxNewXParser.SelectedIndex;
            Options.SaveOptions();
            this.Close();
        }
    }
}
