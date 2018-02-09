using System;
using OpenBveApi.Colors;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
    internal static partial class Renderer
    {
        /// <summary>
        /// Performs a reset of OpenGL to the default state
        /// </summary>
        private static void ResetOpenGlState()
        {
            LastBoundTexture = null;
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Blend); BlendEnabled = false;
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            SetAlphaFunc(AlphaFunction.Greater, 0.9f);
        }

        /// <summary>
        /// Specifies the OpenGL alpha function to perform
        /// </summary>
        /// <param name="Comparison">The comparison to use</param>
        /// <param name="Value">The value to compare</param>
        internal static void SetAlphaFunc(AlphaFunction Comparison, float Value)
        {
            AlphaTestEnabled = true;
            AlphaFuncComparison = Comparison;
            AlphaFuncValue = Value;
            GL.AlphaFunc(Comparison, Value);
            GL.Enable(EnableCap.AlphaTest);
        }

        /// <summary>
        /// Disables OpenGL alpha testing
        /// </summary>
        private static void UnsetAlphaFunc()
        {
            AlphaTestEnabled = false;
            GL.Disable(EnableCap.AlphaTest);
        }

        /// <summary>
        /// Restores the OpenGL alpha function to it's previous state
        /// </summary>
        private static void RestoreAlphaFunc()
        {
            if (AlphaTestEnabled)
            {
                GL.AlphaFunc(AlphaFuncComparison, AlphaFuncValue);
                GL.Enable(EnableCap.AlphaTest);
            }
            else
            {
                GL.Disable(EnableCap.AlphaTest);
            }
        }

        /// <summary>
        /// Clears all currently registered OpenGL display lists
        /// </summary>
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

        /// <summary>
        /// Resets the state of the renderer
        /// </summary>
        internal static void Reset()
        {
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            Objects = new Object[256];
            ObjectCount = 0;
            StaticOpaque = new ObjectGroup[] { };
            StaticOpaqueForceUpdate = true;
            DynamicOpaque = new ObjectList();
            DynamicAlpha = new ObjectList();
            OverlayOpaque = new ObjectList();
            OverlayAlpha = new ObjectList();
            OptionLighting = true;
            OptionAmbientColor = new Color24(160, 160, 160);
            OptionDiffuseColor = new Color24(160, 160, 160);
            OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
            OptionLightingResultingAmount = 1.0f;
            OptionClock = false;
            OptionBrakeSystems = false;
        }

        /// <summary>
        /// Call this once to initialise the renderer
        /// </summary>
        internal static void Initialize()
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.FogHint, HintMode.Fastest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
            GL.Disable(EnableCap.Dither);
            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
            Interface.LoadHUD();
            InitLoading();
            Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend); BlendEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Fog);
        }

        /// <summary>
        /// De-initialize the renderer, and clear all remaining OpenGL display lists
        /// </summary>
        internal static void Deinitialize()
        {
            ClearDisplayLists();
        }

        /// <summary>
        /// Determines the maximum Anisotropic filtering level the system supports
        /// </summary>
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
