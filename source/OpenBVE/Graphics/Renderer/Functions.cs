using System;
using OpenBveShared;
using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
    internal static partial class Renderer
    {
       

        /// <summary>Resets the state of the renderer</summary>
        internal static void Reset()
        {
	        OpenBveShared.Renderer.Objects = new RendererObject[256];
	        OpenBveShared.Renderer.ObjectCount = 0;
	        OpenBveShared.Renderer.StaticOpaque = new ObjectGroup[] { };
	        OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
	        OpenBveShared.Renderer.DynamicOpaque = new ObjectList();
	        OpenBveShared.Renderer.DynamicAlpha = new ObjectList();
	        OpenBveShared.Renderer.OverlayOpaque = new ObjectList();
	        OpenBveShared.Renderer.OverlayAlpha = new ObjectList();
	        OpenBveShared.Renderer.OptionLighting = true;
	        OpenBveShared.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
	        OpenBveShared.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
	        OpenBveShared.Renderer.OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
            OpenBveShared.Renderer.OptionLightingResultingAmount = 1.0f;
            OptionClock = false;
            OptionBrakeSystems = false;
        }

	    /// <summary>De-initialize the renderer, and clear all remaining OpenGL display lists</summary>
        internal static void Deinitialize()
        {
            OpenBveShared.Renderer.ClearDisplayLists();
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
    }
}
