using System;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class Renderer
    {

        /* --------------------------------------------------------------
		 * This file contains the drawing routines for the background texture
		 * -------------------------------------------------------------- */
        internal static bool DisplayListAvailable;
        internal static int BackgroundDisplayList;
        private static void RenderBackground(double TimeElapsed)
        {
            const float scale = 0.5f;
            // fog
            const float fogdistance = 600.0f;
            if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < fogdistance)
            {
                float cr = inv255 * (float)Game.CurrentFog.Color.R;
                float cg = inv255 * (float)Game.CurrentFog.Color.G;
                float cb = inv255 * (float)Game.CurrentFog.Color.B;
                if (!FogEnabled)
                {
                    GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
                }
                float ratio = (float)World.BackgroundImageDistance / fogdistance;
                GL.Fog(FogParameter.FogStart, Game.CurrentFog.Start * ratio * scale);
                GL.Fog(FogParameter.FogEnd, Game.CurrentFog.End * ratio * scale);
                GL.Fog(FogParameter.FogColor, new float[] { cr, cg, cb, 1.0f });
                if (!FogEnabled)
                {
                    GL.Enable(EnableCap.Fog); FogEnabled = true;
                }
            }
            else if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            // render
            if (World.TargetBackgroundCountdown >= 0.0)
            {
                // fade
                World.TargetBackgroundCountdown -= TimeElapsed;
                if (World.TargetBackgroundCountdown < 0.0)
                {
                    World.CurrentBackground = World.TargetBackground;
                    World.TargetBackgroundCountdown = -1.0;
                    RenderBackground(World.CurrentBackground, 1.0f, scale);
                }
                else
                {
                    RenderBackground(World.CurrentBackground, 1.0f, scale);
                    SetAlphaFunc(AlphaFunction.Greater, 0.0f); // ###
                    float Alpha = (float)(1.0 - World.TargetBackgroundCountdown / World.TargetBackgroundDefaultCountdown);
                    RenderBackground(World.TargetBackground, Alpha, scale);
                }
            }
            else
            {
                // single
                RenderBackground(World.CurrentBackground, 1.0f, scale);
            }
        }
        private static void RenderBackground(World.Background Data, float Alpha, float scale)
        {
            if (Data.Texture != null && Textures.LoadTexture(Data.Texture, Textures.OpenGlTextureWrapMode.RepeatClamp))
            {
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                    LightingEnabled = false;
                }
                if (!TexturingEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    TexturingEnabled = true;
                }
                if (Alpha == 1.0f)
                {
                    if (BlendEnabled)
                    {
                        GL.Disable(EnableCap.Blend);
                        BlendEnabled = false;
                    }
                }
                else if (!BlendEnabled)
                {
                    GL.Enable(EnableCap.Blend);
                    BlendEnabled = true;
                }
                GL.BindTexture(TextureTarget.Texture2D, Data.Texture.OpenGlTextures[(int)Textures.OpenGlTextureWrapMode.RepeatClamp].Name);
                GL.Color4(1.0f, 1.0f, 1.0f, Alpha);

                if (!DisplayListAvailable)
                {
                    BackgroundDisplayList = GL.GenLists(1);
                    GL.NewList(BackgroundDisplayList, ListMode.Compile);
                    float y0, y1;
                    if (Data.KeepAspectRatio)
                    {
                        int tw = Data.Texture.Width;
                        int th = Data.Texture.Height;
                        double hh = Math.PI*World.BackgroundImageDistance*(double) th/
                                    ((double) tw*(double) Data.Repetition);
                        y0 = (float) (-0.5*hh);
                        y1 = (float) (1.5*hh);
                    }
                    else
                    {
                        y0 = (float) (-0.125*World.BackgroundImageDistance);
                        y1 = (float) (0.375*World.BackgroundImageDistance);
                    }
                    const int n = 32;
                    Vector3[] bottom = new Vector3[n];
                    Vector3[] top = new Vector3[n];
                    double angleValue = 2.61799387799149 - 3.14159265358979/(double) n;
                    const double angleIncrement = 6.28318530717958/(double) n;
                    /*
                 * To ensure that the whole background cylinder is rendered inside the viewing frustum,
                 * the background is rendered before the scene with z-buffer writes disabled. Then,
                 * the actual distance from the camera is irrelevant as long as it is inside the frustum.
                 * */
                    for (int i = 0; i < n; i++)
                    {
                        float x = (float) (World.BackgroundImageDistance*Math.Cos(angleValue));
                        float z = (float) (World.BackgroundImageDistance*Math.Sin(angleValue));
                        bottom[i] = new Vector3(scale*x, scale*y0, scale*z);
                        top[i] = new Vector3(scale*x, scale*y1, scale*z);
                        angleValue += angleIncrement;
                    }
                    float textureStart = 0.5f*(float) Data.Repetition/(float) n;
                    float textureIncrement = -(float) Data.Repetition/(float) n;
                    double textureX = textureStart;
                    for (int i = 0; i < n; i++)
                    {
                        int j = (i + 1)%n;
                        // side wall
                        GL.Begin(PrimitiveType.Quads);
                        GL.TexCoord2(textureX, 0.005f);
                        GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
                        GL.TexCoord2(textureX, 0.995f);
                        GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
                        GL.TexCoord2(textureX + textureIncrement, 0.995f);
                        GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
                        GL.TexCoord2(textureX + textureIncrement, 0.005f);
                        GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
                        GL.End();
                        // top cap
                        GL.Begin(PrimitiveType.Triangles);
                        GL.TexCoord2(textureX, 0.005f);
                        GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
                        GL.TexCoord2(textureX + textureIncrement, 0.005f);
                        GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
                        GL.TexCoord2(textureX + 0.5*textureIncrement, 0.1f);
                        GL.Vertex3(0.0f, top[i].Y, 0.0f);
                        // bottom cap
                        GL.TexCoord2(textureX + 0.5*textureIncrement, 0.9f);
                        GL.Vertex3(0.0f, bottom[i].Y, 0.0f);
                        GL.TexCoord2(textureX + textureIncrement, 0.995f);
                        GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
                        GL.TexCoord2(textureX, 0.995f);
                        GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
                        GL.End();
                        // finish
                        textureX += textureIncrement;
                    }
                    GL.EndList();
                    GL.CallList(BackgroundDisplayList);
                    GL.Disable(EnableCap.Texture2D);
                    TexturingEnabled = false;
                    if (!BlendEnabled)
                    {
                        GL.Enable(EnableCap.Blend);
                        BlendEnabled = true;
                    }
                    
                    DisplayListAvailable = true;
                }
                else
                {
                    GL.CallList(BackgroundDisplayList);
                    GL.Disable(EnableCap.Texture2D);
                    TexturingEnabled = false;
                    if (!BlendEnabled)
                    {
                        GL.Enable(EnableCap.Blend);
                        BlendEnabled = true;
                    }
                }
            }
        }
    }
}
