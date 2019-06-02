using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
	public static partial class Renderer
    {
	    /// <summary>Call this once to initialise the renderer</summary>
	    public static void Initialize()
	    {
		    GL.ShadeModel(ShadingModel.Smooth);
		    GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
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
		    Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
		    GL.MatrixMode(MatrixMode.Modelview);
		    GL.LoadMatrix(ref lookat);
		    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		    GL.Enable(EnableCap.Blend); BlendEnabled = true;
		    GL.Disable(EnableCap.Lighting); LightingEnabled = false;
		    GL.Disable(EnableCap.Fog);
	    }

	    /// <summary>Performs a reset of OpenGL to the default state</summary>
	    public static void ResetOpenGlState()
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

	    /// <summary>Specifies the OpenGL alpha function to perform</summary>
        /// <param name="Comparison">The comparison to use</param>
        /// <param name="Value">The value to compare</param>
        public static void SetAlphaFunc(AlphaFunction Comparison, float Value)
        {
            AlphaTestEnabled = true;
            AlphaFuncComparison = Comparison;
            AlphaFuncValue = Value;
            GL.AlphaFunc(Comparison, Value);
            GL.Enable(EnableCap.AlphaTest);
        }

        /// <summary>Disables OpenGL alpha testing</summary>
        public static void UnsetAlphaFunc()
        {
            AlphaTestEnabled = false;
            GL.Disable(EnableCap.AlphaTest);
        }

        /// <summary>Restores the OpenGL alpha function to it's previous state</summary>
        public static void RestoreAlphaFunc()
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

        
    }
}
