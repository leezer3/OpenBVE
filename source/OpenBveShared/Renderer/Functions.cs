using System;
using BackgroundManager;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Clears all currently registered OpenGL display lists</summary>
		public static void ClearDisplayLists()
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

		/// <summary>Updates the openGL viewport</summary>
		/// <param name="Mode">The viewport change mode</param>
		public static void UpdateViewport(ViewPortChangeMode Mode)
		{
			if (Mode == ViewPortChangeMode.ChangeToCab)
			{
				CurrentViewPortMode = ViewPortMode.Cab;
			}
			else
			{
				CurrentViewPortMode = ViewPortMode.Scenery;
			}

			GL.Viewport(0, 0, Width, Height);
			World.AspectRatio = (double)Width / (double)Height;
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
				var b = CurrentBackground as BackgroundObject;
				var cd = b != null ? Math.Max(BackgroundImageDistance, b.ClipDistance) : BackgroundImageDistance;
				Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle, -World.AspectRatio, 0.5, cd);
				GL.MultMatrix(ref perspective);
			}
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}
	}
}
