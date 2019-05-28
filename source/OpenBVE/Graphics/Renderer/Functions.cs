using System;
using LibRender;
using OpenBveApi.Colors;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
    internal static partial class Renderer
    {
        /// <summary>Clears all currently registered OpenGL display lists</summary>
        internal static void ClearDisplayLists()
        {
            for (int i = 0; i < StaticOpaque.Length; i++)
            {
                if (StaticOpaque[i] != null)
                {
                    if (StaticOpaque[i].OpenGlDisplayListAvailable)
                    {
                        GL.DeleteLists(StaticOpaque[i].OpenGlDisplayList, 1);
                        StaticOpaque[i].OpenGlDisplayListAvailable = false;
                    }
                }
            }
            StaticOpaqueForceUpdate = true;
        }

        /// <summary>Resets the state of the renderer</summary>
        internal static void Reset()
        {
	        Objects = new Object[256];
            ObjectCount = 0;
            StaticOpaque = new ObjectGroup[] { };
            StaticOpaqueForceUpdate = true;
            DynamicOpaque = new ObjectList();
            DynamicAlpha = new ObjectList();
            OverlayOpaque = new ObjectList();
            OverlayAlpha = new ObjectList();
            Touch = new ObjectList();
            LibRender.Renderer.OptionLighting = true;
            OptionAmbientColor = new Color24(160, 160, 160);
            OptionDiffuseColor = new Color24(160, 160, 160);
            OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
            LibRender.Renderer.OptionLightingResultingAmount = 1.0f;
            OptionClock = false;
            OptionBrakeSystems = false;
        }

        /// <summary>Determines the maximum Anisotropic filtering level the system supports</summary>
        internal static void DetermineMaxAFLevel()
        {

            string[] Extensions = GL.GetString(StringName.Extensions).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
            for (int i = 0; i < Extensions.Length; i++)
            {
                if (string.Compare(Extensions[i], "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    float n; GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out n);
	                int MaxAF = (int) Math.Round(n);
	                if (MaxAF != Interface.CurrentOptions.AnisotropicFilteringMaximum)
	                {
		                Interface.CurrentOptions.AnisotropicFilteringMaximum = (int) Math.Round((double) n);
						Interface.SaveOptions();
	                }
	                break;
                }
            }
            if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0)
            {
                Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
                Interface.CurrentOptions.AnisotropicFilteringLevel = 0;
            }
            else if (Interface.CurrentOptions.AnisotropicFilteringLevel == 0 & Interface.CurrentOptions.AnisotropicFilteringMaximum > 0)
            {
                Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
            }
            else if (Interface.CurrentOptions.AnisotropicFilteringLevel > Interface.CurrentOptions.AnisotropicFilteringMaximum)
            {
                Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
            }
        }

		/// <summary>Updates the openGL viewport</summary>
		/// <param name="Mode">The viewport change mode</param>
	    internal static void UpdateViewport(ViewPortChangeMode Mode)
	    {
		    if (Mode == ViewPortChangeMode.ChangeToCab)
		    {
			    CurrentViewPortMode = ViewPortMode.Cab;
		    }
		    else
		    {
			    CurrentViewPortMode = ViewPortMode.Scenery;
		    }

		    GL.Viewport(0, 0, Screen.Width, Screen.Height);
		    World.AspectRatio = (double)Screen.Width / (double)Screen.Height;
		    World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
		    GL.MatrixMode(MatrixMode.Projection);
		    GL.LoadIdentity();
		    if (CurrentViewPortMode == ViewPortMode.Cab)
		    {

			    Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle, -World.AspectRatio, 0.025, 50.0);
			    GL.MultMatrix(ref perspective);
		    }
		    else
		    {
			    var b = BackgroundManager.CurrentBackground as BackgroundManager.BackgroundObject;
			    var cd = b != null ? Math.Max(World.BackgroundImageDistance, b.ClipDistance) : World.BackgroundImageDistance;
			    Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle, -World.AspectRatio, 0.5, cd);
			    GL.MultMatrix(ref perspective);
		    }
		    GL.MatrixMode(MatrixMode.Modelview);
		    GL.LoadIdentity();
	    }
    }
}
