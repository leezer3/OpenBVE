﻿using System;
using OpenBveApi.Colors;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class Renderer
    {

        private static void RenderOverlayTexture(Textures.Texture texture, double left, double top, double right, double bottom)
        {
            DrawRectangle(texture, new System.Drawing.Point((int)left, (int)top), new System.Drawing.Size((int)(right - left), (int)(bottom - top)), null);
        }
        private static void RenderOverlaySolid(double left, double top, double right, double bottom)
        {
            DrawRectangle(null, new System.Drawing.Point((int)left, (int)top), new System.Drawing.Size((int)(right - left), (int)(bottom - top)), null);
        }

        internal static OutputMode CurrentOutputMode = OutputMode.Default;
        //Set LoadTextureImmediatelyMode to NotYet for the first frame
        internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;


        // the static opaque lists
        /// <summary>The list of static opaque face groups. Each group contains only objects that are associated the respective group index.</summary>
        private static ObjectGroup[] StaticOpaque = new ObjectGroup[] { };
        /// <summary>Whether to enforce updating all display lists.</summary>
        internal static bool StaticOpaqueForceUpdate = true;

        // all other lists
        /// <summary>The list of dynamic opaque faces to be rendered.</summary>
        private static ObjectList DynamicOpaque = new ObjectList();
        /// <summary>The list of dynamic alpha faces to be rendered.</summary>
        private static ObjectList DynamicAlpha = new ObjectList();
        /// <summary>The list of overlay opaque faces to be rendered.</summary>
        private static ObjectList OverlayOpaque = new ObjectList();
        /// <summary>The list of overlay alpha faces to be rendered.</summary>
        private static ObjectList OverlayAlpha = new ObjectList();

        // current opengl data
        private static AlphaFunction AlphaFuncComparison = 0;
        private static float AlphaFuncValue = 0.0f;
        private static bool AlphaTestEnabled = false;
        private static bool BlendEnabled = false;
        private static bool CullEnabled = true;
        internal static bool LightingEnabled = false;
        internal static bool FogEnabled = false;
        private static bool TexturingEnabled = false;
        private static bool EmissiveEnabled = false;

        // options
        internal static bool OptionLighting = true;
        internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
        internal static Color24 OptionDiffuseColor = new Color24(160, 160, 160);
        internal static World.Vector3Df OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
        internal static float OptionLightingResultingAmount = 1.0f;
        internal static bool OptionNormals = false;
        internal static bool OptionWireframe = false;
        internal static bool OptionBackfaceCulling = true;

        // interface options
        internal static bool OptionClock = false;
        internal enum SpeedDisplayMode { None, Kmph, Mph }
        internal static SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
        internal static bool OptionFrameRates = false;
        internal static bool OptionBrakeSystems = false;

        // fade to black
        private static double FadeToBlackDueToChangeEnds = 0.0;

        // textures
        private static Textures.Texture TextureLogo = null;

        // constants
        private const float inv255 = 1.0f / 255.0f;       

        // render scene
        internal static byte[] PixelBuffer = null;
        internal static int PixelBufferOpenGlTextureIndex = 0;
        internal static void RenderScene(double TimeElapsed)
        {
            // initialize
            ResetOpenGlState();
            int OpenGlTextureIndex = 0;
            if (World.CurrentBackground.Texture != null)
            {
                //Textures.LoadTexture(World.CurrentBackground.Texture, Textures.OpenGlTextureWrapMode.RepeatClamp); // TODO
            }
            if (OptionWireframe | OpenGlTextureIndex == 0)
            {
                if (Game.CurrentFog.Start < Game.CurrentFog.End)
                {
                    const float fogdistance = 600.0f;
                    float n = (fogdistance - Game.CurrentFog.Start) / (Game.CurrentFog.End - Game.CurrentFog.Start);
                    float cr = n * inv255 * (float)Game.CurrentFog.Color.R;
                    float cg = n * inv255 * (float)Game.CurrentFog.Color.G;
                    float cb = n * inv255 * (float)Game.CurrentFog.Color.B;
                    GL.ClearColor(cr, cg, cb, 1.0f);
                }
                else
                {
                    GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                }
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            else
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }
            GL.PushMatrix();
            MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.ChangeToScenery);
            if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet)
            {
                LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
            }
            // set up camera
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            double dx = World.AbsoluteCameraDirection.X;
            double dy = World.AbsoluteCameraDirection.Y;
            double dz = World.AbsoluteCameraDirection.Z;
            double ux = World.AbsoluteCameraUp.X;
            double uy = World.AbsoluteCameraUp.Y;
            double uz = World.AbsoluteCameraUp.Z;
            Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float) OptionLightPosition.X, (float) OptionLightPosition.Y, (float) OptionLightPosition.Z, 0.0f });
            // fog
            double fd = Game.NextFog.TrackPosition - Game.PreviousFog.TrackPosition;
            if (fd != 0.0)
            {
                float fr = (float)((World.CameraTrackFollower.TrackPosition - Game.PreviousFog.TrackPosition) / fd);
                float frc = 1.0f - fr;
                Game.CurrentFog.Start = Game.PreviousFog.Start * frc + Game.NextFog.Start * fr;
                Game.CurrentFog.End = Game.PreviousFog.End * frc + Game.NextFog.End * fr;
                Game.CurrentFog.Color.R = (byte)((float)Game.PreviousFog.Color.R * frc + (float)Game.NextFog.Color.R * fr);
                Game.CurrentFog.Color.G = (byte)((float)Game.PreviousFog.Color.G * frc + (float)Game.NextFog.Color.G * fr);
                Game.CurrentFog.Color.B = (byte)((float)Game.PreviousFog.Color.B * frc + (float)Game.NextFog.Color.B * fr);
            }
            else
            {
                Game.CurrentFog = Game.PreviousFog;
            }
            // render background
            if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            GL.Disable(EnableCap.DepthTest);
            RenderBackground(TimeElapsed);
            // fog
            float aa = Game.CurrentFog.Start;
            float bb = Game.CurrentFog.End;
            if (aa < bb & aa < World.BackgroundImageDistance)
            {
                if (!FogEnabled)
                {
                    GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
                }
                GL.Fog(FogParameter.FogStart, aa);
                GL.Fog(FogParameter.FogEnd, bb);
                GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
                if (!FogEnabled)
                {
                    GL.Enable(EnableCap.Fog); FogEnabled = true;
                }
            }
            else if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            // world layer
            bool optionLighting = OptionLighting;
            LastBoundTexture = null;
            if (OptionLighting)
            {
                if (!LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting); LightingEnabled = true;
                }
                if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
                {
                    GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
                    GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
                }
            }
            else if (LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            }
            // static opaque
            if (Interface.CurrentOptions.DisableDisplayLists)
            {
                ResetOpenGlState();
                for (int i = 0; i < StaticOpaque.Length; i++)
                {
                    if (StaticOpaque[i] != null)
                    {
                        if (StaticOpaque[i].List != null)
                        {
                            for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
                            {
                                if (StaticOpaque[i].List.Faces[j] != null)
                                {
                                    RenderFace(ref StaticOpaque[i].List.Faces[j], cx, cy, cz);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < StaticOpaque.Length; i++)
                {
                    if (StaticOpaque[i] != null)
                    {
                        if (StaticOpaque[i].Update | StaticOpaqueForceUpdate)
                        {
                            StaticOpaque[i].Update = false;
                            if (StaticOpaque[i].OpenGlDisplayListAvailable)
                            {
                                GL.DeleteLists(StaticOpaque[i].OpenGlDisplayList, 1);
                                StaticOpaque[i].OpenGlDisplayListAvailable = false;
                            }
                            if (StaticOpaque[i].List.FaceCount != 0)
                            {
                                StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
                                StaticOpaque[i].OpenGlDisplayListAvailable = true;
                                ResetOpenGlState();
                                GL.NewList(StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
                                for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
                                {
                                    if (StaticOpaque[i].List.Faces[j] != null)
                                    {
                                        RenderFace(ref StaticOpaque[i].List.Faces[j], cx, cy, cz);
                                    }
                                }
                                GL.EndList();
                            }
                            StaticOpaque[i].WorldPosition = World.AbsoluteCameraPosition;
                        }
                    }
                }
                StaticOpaqueForceUpdate = false;
                for (int i = 0; i < StaticOpaque.Length; i++)
                {
                    if (StaticOpaque[i] != null)
                    {
                        if (StaticOpaque[i].OpenGlDisplayListAvailable)
                        {
                            ResetOpenGlState();
                            GL.PushMatrix();
                            GL.Translate(StaticOpaque[i].WorldPosition.X - World.AbsoluteCameraPosition.X, StaticOpaque[i].WorldPosition.Y - World.AbsoluteCameraPosition.Y, StaticOpaque[i].WorldPosition.Z - World.AbsoluteCameraPosition.Z);
                            GL.CallList(StaticOpaque[i].OpenGlDisplayList);
                            GL.PopMatrix();
                        }
                    }
                }
            }
            // dynamic opaque
            ResetOpenGlState();
            for (int i = 0; i < DynamicOpaque.FaceCount; i++)
            {
                RenderFace(ref DynamicOpaque.Faces[i], cx, cy, cz);
            }
            // dynamic alpha
            ResetOpenGlState();
            SortPolygons(DynamicAlpha);
            if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
            {
                GL.Enable(EnableCap.Blend); BlendEnabled = true;
                GL.DepthMask(false);
                SetAlphaFunc(AlphaFunction.Greater, 0.0f);
                for (int i = 0; i < DynamicAlpha.FaceCount; i++)
                {
                    RenderFace(ref DynamicAlpha.Faces[i], cx, cy, cz);
                }
            }
            else
            {
                GL.Disable(EnableCap.Blend); BlendEnabled = false;
                SetAlphaFunc(AlphaFunction.Equal, 1.0f);
                GL.DepthMask(true);
                for (int i = 0; i < DynamicAlpha.FaceCount; i++)
                {
                    int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
                    if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Normal & ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
                    {
                        if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
                        {
                            RenderFace(ref DynamicAlpha.Faces[i], cx, cy, cz);
                        }
                    }
                }
                GL.Enable(EnableCap.Blend); BlendEnabled = true;
                SetAlphaFunc(AlphaFunction.Less, 1.0f);
                GL.DepthMask(false);
                bool additive = false;
                for (int i = 0; i < DynamicAlpha.FaceCount; i++)
                {
                    int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
                    if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Additive)
                    {
                        if (!additive)
                        {
                            UnsetAlphaFunc();
                            additive = true;
                        }
                        RenderFace(ref DynamicAlpha.Faces[i], cx, cy, cz);
                    }
                    else
                    {
                        if (additive)
                        {
                            SetAlphaFunc(AlphaFunction.Less, 1.0f);
                            additive = false;
                        }
                        RenderFace(ref DynamicAlpha.Faces[i], cx, cy, cz);
                    }
                }
            }
            // motion blur
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            SetAlphaFunc(AlphaFunction.Greater, 0.0f);
            if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None)
            {
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                    LightingEnabled = false;
                }
                RenderFullscreenMotionBlur();
            }
            // overlay layer
            if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            GL.LoadIdentity();
            MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.ChangeToCab);
            lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
            {
                // 3d cab
                ResetOpenGlState(); // TODO: inserted
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                if (!LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting); LightingEnabled = true;
                }
                OptionLighting = true;
                GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
                GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
                // overlay opaque
                SetAlphaFunc(AlphaFunction.Greater, 0.9f);
                for (int i = 0; i < OverlayOpaque.FaceCount; i++)
                {
                    RenderFace(ref OverlayOpaque.Faces[i], cx, cy, cz);
                }
                // overlay alpha
                SortPolygons(OverlayAlpha);
                if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
                {
                    GL.Enable(EnableCap.Blend); BlendEnabled = true;
                    GL.DepthMask(false);
                    SetAlphaFunc(AlphaFunction.Greater, 0.0f);
                    for (int i = 0; i < OverlayAlpha.FaceCount; i++)
                    {
                        RenderFace(ref OverlayAlpha.Faces[i], cx, cy, cz);
                    }
                }
                else
                {
                    GL.Disable(EnableCap.Blend); BlendEnabled = false;
                    SetAlphaFunc(AlphaFunction.Equal, 1.0f);
                    GL.DepthMask(true);
                    for (int i = 0; i < OverlayAlpha.FaceCount; i++)
                    {
                        int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
                        if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Normal & ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
                        {
                            if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
                            {
                                RenderFace(ref OverlayAlpha.Faces[i], cx, cy, cz);
                            }
                        }
                    }
                    GL.Enable(EnableCap.Blend); BlendEnabled = true;
                    SetAlphaFunc(AlphaFunction.Less, 1.0f);
                    GL.DepthMask(false);
                    bool additive = false;
                    for (int i = 0; i < OverlayAlpha.FaceCount; i++)
                    {
                        int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
                        if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Additive)
                        {
                            if (!additive)
                            {
                                UnsetAlphaFunc();
                                additive = true;
                            }
                            RenderFace(ref OverlayAlpha.Faces[i], cx, cy, cz);
                        }
                        else
                        {
                            if (additive)
                            {
                                SetAlphaFunc(AlphaFunction.Less, 1.0f);
                                additive = false;
                            }
                            RenderFace(ref OverlayAlpha.Faces[i], cx, cy, cz);
                        }
                    }
                }
            }
            else
            {
                // not a 3d cab
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting); LightingEnabled = false; // TODO: was 'true' before
                }
                OptionLighting = false;
                if (!BlendEnabled)
                {
                    GL.Enable(EnableCap.Blend); BlendEnabled = true;
                }
                GL.DepthMask(false);
                GL.Disable(EnableCap.DepthTest);
                UnsetAlphaFunc();
                SortPolygons(OverlayAlpha);
                for (int i = 0; i < OverlayAlpha.FaceCount; i++)
                {
                    RenderFace(ref OverlayAlpha.Faces[i], cx, cy, cz);
                }
            }
            // render overlays
            OptionLighting = optionLighting;
            if (LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            }
            if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            if (BlendEnabled)
            {
                GL.Disable(EnableCap.Blend); BlendEnabled = false;
            }
            UnsetAlphaFunc();
            GL.Disable(EnableCap.DepthTest);
            RenderOverlays(TimeElapsed);
            // finalize rendering
            GL.PopMatrix();
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NoLonger;
        }

        // set alpha func
        
        /// <summary> Stores the last bound OpenGL texture</summary>
        private static Textures.OpenGlTexture LastBoundTexture = null;

        private static void RenderFace(ref ObjectFace Face, double CameraX, double CameraY, double CameraZ)
        {
            if (CullEnabled)
            {
                if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) != 0)
                {
                    GL.Disable(EnableCap.CullFace);
                    CullEnabled = false;
                }
            }
            else if (OptionBackfaceCulling)
            {
                if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) == 0)
                {
                    GL.Enable(EnableCap.CullFace);
                    CullEnabled = true;
                }
            }
            int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
            RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
        }
        private static void RenderFace(ref World.MeshMaterial Material, World.Vertex[] Vertices, Textures.OpenGlTextureWrapMode wrap, ref World.MeshFace Face, double CameraX, double CameraY, double CameraZ)
        {
            // texture
            if (Material.DaytimeTexture != null)
            {
                if (Textures.LoadTexture(Material.DaytimeTexture, wrap))
                {
                    if (!TexturingEnabled)
                    {
                        GL.Enable(EnableCap.Texture2D);
                        TexturingEnabled = true;
                    }
                    if (Material.DaytimeTexture.OpenGlTextures[(int)wrap] != LastBoundTexture)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, Material.DaytimeTexture.OpenGlTextures[(int)wrap].Name);
                        LastBoundTexture = Material.DaytimeTexture.OpenGlTextures[(int)wrap];
                    }
                }
                else
                {
                    if (TexturingEnabled)
                    {
                        GL.Disable(EnableCap.Texture2D);
                        TexturingEnabled = false;
                        LastBoundTexture = null;
                    }
                }
            }
            else
            {
                if (TexturingEnabled)
                {
                    GL.Disable(EnableCap.Texture2D);
                    TexturingEnabled = false;
                    LastBoundTexture = null;
                }
            }
            // blend mode
            float factor;
            if (Material.BlendMode == World.MeshMaterialBlendMode.Additive)
            {
                factor = 1.0f;
                if (!BlendEnabled) GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                if (FogEnabled)
                {
                    GL.Disable(EnableCap.Fog);
                }
            }
            else if (Material.NighttimeTexture == null)
            {
                float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
                if (blend > 1.0f) blend = 1.0f;
                factor = 1.0f - 0.7f * blend;
            }
            else
            {
                factor = 1.0f;
            }
            if (Material.NighttimeTexture != null)
            {
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                    LightingEnabled = false;
                }
            }
            else
            {
                if (OptionLighting & !LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                    LightingEnabled = true;
                }
            }
            // render daytime polygon
            int FaceType = Face.Flags & World.MeshFace.FaceTypeMask;
            switch (FaceType)
            {
                case World.MeshFace.FaceTypeTriangles:
                    GL.Begin(PrimitiveType.Triangles);
                    break;
                case World.MeshFace.FaceTypeTriangleStrip:
                    GL.Begin(PrimitiveType.TriangleStrip);
                    break;
                case World.MeshFace.FaceTypeQuads:
                    GL.Begin(PrimitiveType.Quads);
                    break;
                case World.MeshFace.FaceTypeQuadStrip:
                    GL.Begin(PrimitiveType.QuadStrip);
                    break;
                default:
                    GL.Begin(PrimitiveType.Polygon);
                    break;
            }
            if (Material.GlowAttenuationData != 0)
            {
                float alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
                GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
            }
            else
            {
                GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
            }
            if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
                EmissiveEnabled = true;
            }
            else if (EmissiveEnabled)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                EmissiveEnabled = false;
            }
            if (Material.DaytimeTexture != null)
            {
                if (LightingEnabled)
                {
                    for (int j = 0; j < Face.Vertices.Length; j++)
                    {
                        GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
                        GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
                        GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                    }
                }
                else
                {
                    for (int j = 0; j < Face.Vertices.Length; j++)
                    {
                        GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
                        GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                    }
                }
            }
            else
            {
                if (LightingEnabled)
                {
                    for (int j = 0; j < Face.Vertices.Length; j++)
                    {
                        GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
                        GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                    }
                }
                else
                {
                    for (int j = 0; j < Face.Vertices.Length; j++)
                    {
                        GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                    }
                }
            }
            GL.End();
            // render nighttime polygon
            if (Material.NighttimeTexture != null && Textures.LoadTexture(Material.NighttimeTexture, wrap))
            {
                if (!TexturingEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    TexturingEnabled = true;
                }
                if (!BlendEnabled)
                {
                    GL.Enable(EnableCap.Blend);
                }
                GL.BindTexture(TextureTarget.Texture2D, Material.NighttimeTexture.OpenGlTextures[(int)wrap].Name);
                LastBoundTexture = null;
                GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
                GL.Enable(EnableCap.AlphaTest);
                switch (FaceType)
                {
                    case World.MeshFace.FaceTypeTriangles:
                        GL.Begin(PrimitiveType.Triangles);
                        break;
                    case World.MeshFace.FaceTypeTriangleStrip:
                        GL.Begin(PrimitiveType.TriangleStrip);
                        break;
                    case World.MeshFace.FaceTypeQuads:
                        GL.Begin(PrimitiveType.Quads);
                        break;
                    case World.MeshFace.FaceTypeQuadStrip:
                        GL.Begin(PrimitiveType.QuadStrip);
                        break;
                    default:
                        GL.Begin(PrimitiveType.Polygon);
                        break;
                }
                float alphafactor;
                if (Material.GlowAttenuationData != 0)
                {
                    alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
                    float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
                    if (blend > 1.0f) blend = 1.0f;
                    alphafactor *= blend;
                }
                else
                {
                    alphafactor = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
                    if (alphafactor > 1.0f) alphafactor = 1.0f;
                }
                GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
                if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0)
                {
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
                    EmissiveEnabled = true;
                }
                else if (EmissiveEnabled)
                {
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                    EmissiveEnabled = false;
                }
                for (int j = 0; j < Face.Vertices.Length; j++)
                {
                    GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
                    GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                }
                GL.End();
                RestoreAlphaFunc();
                if (!BlendEnabled)
                {
                    GL.Disable(EnableCap.Blend);
                }
            }
            // normals
            if (OptionNormals)
            {
                if (TexturingEnabled)
                {
                    GL.Disable(EnableCap.Texture2D);
                    TexturingEnabled = false;
                }
                for (int j = 0; j < Face.Vertices.Length; j++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color4(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
                    GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
                    GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - CameraZ));
                    GL.End();
                }
            }
            // finalize
            if (Material.BlendMode == World.MeshMaterialBlendMode.Additive)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                if (!BlendEnabled) GL.Disable(EnableCap.Blend);
                if (FogEnabled)
                {
                    GL.Enable(EnableCap.Fog);
                }
            }
        }

        // render fullscreen motion blur


        // get color
        private static void CreateBackColor(Color32 Original, Game.MessageColor SystemColor, out float R, out float G, out float B, out float A)
        {
            if (Original.R == 0 & Original.G == 0 & Original.B == 0)
            {
                switch (SystemColor)
                {
                    case Game.MessageColor.Black:
                        R = 0.0f; G = 0.0f; B = 0.0f;
                        break;
                    case Game.MessageColor.Gray:
                        R = 0.4f; G = 0.4f; B = 0.4f;
                        break;
                    case Game.MessageColor.White:
                        R = 1.0f; G = 1.0f; B = 1.0f;
                        break;
                    case Game.MessageColor.Red:
                        R = 1.0f; G = 0.0f; B = 0.0f;
                        break;
                    case Game.MessageColor.Orange:
                        R = 0.9f; G = 0.7f; B = 0.0f;
                        break;
                    case Game.MessageColor.Green:
                        R = 0.2f; G = 0.8f; B = 0.0f;
                        break;
                    case Game.MessageColor.Blue:
                        R = 0.0f; G = 0.7f; B = 1.0f;
                        break;
                    case Game.MessageColor.Magenta:
                        R = 1.0f; G = 0.0f; B = 0.7f;
                        break;
                    default:
                        R = 1.0f; G = 1.0f; B = 1.0f;
                        break;
                }
            }
            else
            {
                R = inv255 * (float)Original.R;
                G = inv255 * (float)Original.G;
                B = inv255 * (float)Original.B;
            }
            A = inv255 * (float)Original.A;
        }
        private static void CreateTextColor(Color32 Original, Game.MessageColor SystemColor, out float R, out float G, out float B, out float A)
        {
            if (Original.R == 0 & Original.G == 0 & Original.B == 0)
            {
                switch (SystemColor)
                {
                    case Game.MessageColor.Black:
                        R = 0.0f; G = 0.0f; B = 0.0f;
                        break;
                    case Game.MessageColor.Gray:
                        R = 0.4f; G = 0.4f; B = 0.4f;
                        break;
                    case Game.MessageColor.White:
                        R = 1.0f; G = 1.0f; B = 1.0f;
                        break;
                    case Game.MessageColor.Red:
                        R = 1.0f; G = 0.0f; B = 0.0f;
                        break;
                    case Game.MessageColor.Orange:
                        R = 0.9f; G = 0.7f; B = 0.0f;
                        break;
                    case Game.MessageColor.Green:
                        R = 0.3f; G = 1.0f; B = 0.0f;
                        break;
                    case Game.MessageColor.Blue:
                        R = 1.0f; G = 1.0f; B = 1.0f;
                        break;
                    case Game.MessageColor.Magenta:
                        R = 1.0f; G = 0.0f; B = 0.7f;
                        break;
                    default:
                        R = 1.0f; G = 1.0f; B = 1.0f;
                        break;
                }
            }
            else
            {
                R = inv255 * (float)Original.R;
                G = inv255 * (float)Original.G;
                B = inv255 * (float)Original.B;
            }
            A = inv255 * (float)Original.A;
        }

        // re-add objects
        internal static void ReAddObjects()
        {
            Object[] list = new Object[ObjectCount];
            for (int i = 0; i < ObjectCount; i++)
            {
                list[i] = Objects[i];
            }
            for (int i = 0; i < list.Length; i++)
            {
                HideObject(list[i].ObjectIndex);
            }
            for (int i = 0; i < list.Length; i++)
            {
                ShowObject(list[i].ObjectIndex, list[i].Type);
            }
        }

        // show object
        internal static void ShowObject(int ObjectIndex, ObjectType Type)
        {
            if (ObjectManager.Objects[ObjectIndex] == null)
            {
                return;
            }
            if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0)
            {
                if (ObjectCount >= Objects.Length)
                {
                    Array.Resize<Object>(ref Objects, Objects.Length << 1);
                }
                Objects[ObjectCount].ObjectIndex = ObjectIndex;
                Objects[ObjectCount].Type = Type;
                int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
                Objects[ObjectCount].FaceListReferences = new ObjectListReference[f];
                for (int i = 0; i < f; i++)
                {
                    bool alpha = false;
                    int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
                    Textures.OpenGlTextureWrapMode wrap = Textures.OpenGlTextureWrapMode.ClampClamp;
                    if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null | ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                    {

                        // HACK: Objects do not store information on the texture wrapping mode.
                        //       Let's determine the best wrapping mode now and then save it
                        //       so we can quickly access it in the rendering loop.

                        if (ObjectManager.Objects[ObjectIndex].Dynamic)
                        {
                            wrap = Textures.OpenGlTextureWrapMode.RepeatRepeat;
                        }
                        else
                        {
                            for (int v = 0; v < ObjectManager.Objects[ObjectIndex].Mesh.Vertices.Length; v++)
                            {
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X < 0.0f | ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
                                {
                                    wrap |= Textures.OpenGlTextureWrapMode.RepeatClamp;
                                }
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y < 0.0f | ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
                                {
                                    wrap |= Textures.OpenGlTextureWrapMode.ClampRepeat;
                                }
                            }
                        }

                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null)
                        {
                            if (Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture, wrap))
                            {
                                OpenBveApi.Textures.TextureTransparencyType type = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency;
                                if (type == OpenBveApi.Textures.TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (type == OpenBveApi.Textures.TextureTransparencyType.Partial && Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
                                {
                                    alpha = true;
                                }
                            }
                        }
                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                        {
                            if (Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture, wrap))
                            {
                                OpenBveApi.Textures.TextureTransparencyType type = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency;
                                if (type == OpenBveApi.Textures.TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (type == OpenBveApi.Textures.TextureTransparencyType.Partial & Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
                                {
                                    alpha = true;
                                }
                            }
                        }
                    }
                    if (Type == ObjectType.Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == World.MeshMaterialBlendMode.Additive)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0)
                    {
                        alpha = true;
                    }
                    ObjectListType listType;
                    switch (Type)
                    {
                        case ObjectType.Static:
                            listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.StaticOpaque;
                            break;
                        case ObjectType.Dynamic:
                            listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.DynamicOpaque;
                            break;
                        case ObjectType.Overlay:
                            listType = alpha ? ObjectListType.OverlayAlpha : ObjectListType.OverlayOpaque;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    if (listType == ObjectListType.StaticOpaque)
                    {
                        /*
                         * For the static opaque list, insert the face into
                         * the first vacant position in the matching group's list.
                         * */
                        int groupIndex = (int)ObjectManager.Objects[ObjectIndex].GroupIndex;
                        if (groupIndex >= StaticOpaque.Length)
                        {
                            if (StaticOpaque.Length == 0)
                            {
                                StaticOpaque = new ObjectGroup[16];
                            }
                            while (groupIndex >= StaticOpaque.Length)
                            {
                                Array.Resize<ObjectGroup>(ref StaticOpaque, StaticOpaque.Length << 1);
                            }
                        }
                        if (StaticOpaque[groupIndex] == null)
                        {
                            StaticOpaque[groupIndex] = new ObjectGroup();
                        }
                        ObjectList list = StaticOpaque[groupIndex].List;
                        int newIndex = list.FaceCount;
                        for (int j = 0; j < list.FaceCount; j++)
                        {
                            if (list.Faces[j] == null)
                            {
                                newIndex = j;
                                break;
                            }
                        }
                        if (newIndex == list.FaceCount)
                        {
                            if (list.FaceCount == list.Faces.Length)
                            {
                                Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
                            }
                            list.FaceCount++;
                        }
                        list.Faces[newIndex] = new ObjectFace
                        {
                            ObjectListIndex = ObjectCount,
                            ObjectIndex = ObjectIndex,
                            FaceIndex = i,
                            Wrap = wrap
                        };

                        // HACK: Let's store the wrapping mode.

                        StaticOpaque[groupIndex].Update = true;
                        Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
                        Game.InfoStaticOpaqueFaceCount++;
                    }
                    else
                    {
                        /*
                         * For all other lists, insert the face at the end of the list.
                         * */
                        ObjectList list;
                        switch (listType)
                        {
                            case ObjectListType.DynamicOpaque:
                                list = DynamicOpaque;
                                break;
                            case ObjectListType.DynamicAlpha:
                                list = DynamicAlpha;
                                break;
                            case ObjectListType.OverlayOpaque:
                                list = OverlayOpaque;
                                break;
                            case ObjectListType.OverlayAlpha:
                                list = OverlayAlpha;
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        if (list.FaceCount == list.Faces.Length)
                        {
                            Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
                        }
                        list.Faces[list.FaceCount] = new ObjectFace
                        {
                            ObjectListIndex = ObjectCount,
                            ObjectIndex = ObjectIndex,
                            FaceIndex = i,
                            Wrap = wrap
                        };

                        // HACK: Let's store the wrapping mode.

                        Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
                        list.FaceCount++;
                    }

                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectCount + 1;
                ObjectCount++;
            }
        }

        // hide object
        internal static void HideObject(int ObjectIndex)
        {
            if (ObjectManager.Objects[ObjectIndex] == null)
            {
                return;
            }
            int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
            if (k >= 0)
            {
                // remove faces
                for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
                {
                    ObjectListType listType = Objects[k].FaceListReferences[i].Type;
                    if (listType == ObjectListType.StaticOpaque)
                    {
                        /*
                         * For static opaque faces, set the face to be removed
                         * to a null reference. If there are null entries at
                         * the end of the list, update the number of faces used
                         * accordingly.
                         * */
                        int groupIndex = (int)ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
                        ObjectList list = StaticOpaque[groupIndex].List;
                        int listIndex = Objects[k].FaceListReferences[i].Index;
                        list.Faces[listIndex] = null;
                        if (listIndex == list.FaceCount - 1)
                        {
                            int count = 0;
                            for (int j = list.FaceCount - 2; j >= 0; j--)
                            {
                                if (list.Faces[j] != null)
                                {
                                    count = j + 1;
                                    break;
                                }
                            }
                            list.FaceCount = count;
                        }
                        StaticOpaque[groupIndex].Update = true;
                        Game.InfoStaticOpaqueFaceCount--;
                    }
                    else
                    {
                        /*
                         * For all other kinds of faces, move the last face into place
                         * of the face to be removed and decrement the face counter.
                         * */
                        ObjectList list;
                        switch (listType)
                        {
                            case ObjectListType.DynamicOpaque:
                                list = DynamicOpaque;
                                break;
                            case ObjectListType.DynamicAlpha:
                                list = DynamicAlpha;
                                break;
                            case ObjectListType.OverlayOpaque:
                                list = OverlayOpaque;
                                break;
                            case ObjectListType.OverlayAlpha:
                                list = OverlayAlpha;
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        int listIndex = Objects[k].FaceListReferences[i].Index;
                        list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
                        Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
                        list.FaceCount--;
                    }
                }
                // remove object
                if (k == ObjectCount - 1)
                {
                    ObjectCount--;
                }
                else
                {
                    Objects[k] = Objects[ObjectCount - 1];
                    ObjectCount--;
                    for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
                    {
                        ObjectListType listType = Objects[k].FaceListReferences[i].Type;
                        ObjectList list;
                        switch (listType)
                        {
                            case ObjectListType.StaticOpaque:
                                {
                                    int groupIndex = (int)ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
                                    list = StaticOpaque[groupIndex].List;
                                }
                                break;
                            case ObjectListType.DynamicOpaque:
                                list = DynamicOpaque;
                                break;
                            case ObjectListType.DynamicAlpha:
                                list = DynamicAlpha;
                                break;
                            case ObjectListType.OverlayOpaque:
                                list = OverlayOpaque;
                                break;
                            case ObjectListType.OverlayAlpha:
                                list = OverlayAlpha;
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        int listIndex = Objects[k].FaceListReferences[i].Index;
                        list.Faces[listIndex].ObjectListIndex = k;
                    }
                    ObjectManager.Objects[Objects[k].ObjectIndex].RendererIndex = k + 1;
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
            }
        }

        // sort polygons
        private static void SortPolygons(ObjectList List)
        {
            // calculate distance
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            for (int i = 0; i < List.FaceCount; i++)
            {
                int o = List.Faces[i].ObjectIndex;
                int f = List.Faces[i].FaceIndex;
                if (ObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3)
                {
                    int v0 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
                    int v1 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
                    int v2 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
                    double v0x = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
                    double v0y = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
                    double v0z = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
                    double v1x = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
                    double v1y = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
                    double v1z = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
                    double v2x = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
                    double v2y = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
                    double v2z = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
                    double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
                    double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
                    double dx = -w1z * w2y + w1y * w2z;
                    double dy = w1z * w2x - w1x * w2z;
                    double dz = -w1y * w2x + w1x * w2y;
                    double t = dx * dx + dy * dy + dz * dz;
                    if (t == 0.0) continue;
                    t = 1.0 / Math.Sqrt(t);
                    dx *= t; dy *= t; dz *= t;
                    double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
                    t = dx * w0x + dy * w0y + dz * w0z;
                    List.Faces[i].Distance = -t * t;
                }
            }
            // sort
            double[] distances = new double[List.FaceCount];
            for (int i = 0; i < List.FaceCount; i++)
            {
                distances[i] = List.Faces[i].Distance;
            }
            Array.Sort<double, ObjectFace>(distances, List.Faces, 0, List.FaceCount);
            // update objects
            for (int i = 0; i < List.FaceCount; i++)
            {
                Objects[List.Faces[i].ObjectListIndex].FaceListReferences[List.Faces[i].FaceIndex].Index = i;
            }
        }

        // get distance factor
        private static double GetDistanceFactor(World.Vertex[] Vertices, ref World.MeshFace Face, ushort GlowAttenuationData, double CameraX, double CameraY, double CameraZ)
        {
            if (Face.Vertices.Length == 0)
            {
                return 1.0;
            }
            World.GlowAttenuationMode mode;
            double halfdistance;
            World.SplitGlowAttenuationData(GlowAttenuationData, out mode, out halfdistance);
            int i = (int)Face.Vertices[0].Index;
            double dx = Vertices[i].Coordinates.X - CameraX;
            double dy = Vertices[i].Coordinates.Y - CameraY;
            double dz = Vertices[i].Coordinates.Z - CameraZ;
            switch (mode)
            {
                case World.GlowAttenuationMode.DivisionExponent2:
                    {
                        double t = dx * dx + dy * dy + dz * dz;
                        return t / (t + halfdistance * halfdistance);
                    }
                case World.GlowAttenuationMode.DivisionExponent4:
                    {
                        double t = dx * dx + dy * dy + dz * dz;
                        t *= t;
                        halfdistance *= halfdistance;
                        return t / (t + halfdistance * halfdistance);
                    }
                default:
                    return 1.0;
            }
        }
    }
}