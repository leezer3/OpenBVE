// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Structure Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Colors;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Math;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
    internal static class Renderer
    {

        // screen (output window)
        internal static int ScreenWidth = 960;
        internal static int ScreenHeight = 600;

        // first frame behavior
        internal enum LoadTextureImmediatelyMode { NotYet, Yes }
        internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
        internal enum TransparencyMode { Sharp, Smooth }

        // object list
        internal enum ObjectType : byte
        {
            /// <summary>The object is part of the static scenery. The matching ObjectListType is StaticOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
            Static = 1,
            /// <summary>The object is part of the animated scenery or of a train exterior. The matching ObjectListType is DynamicOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
            Dynamic = 2,
            /// <summary>The object is part of the cab. The matching ObjectListType is OverlayOpaque for fully opaque faces, and OverlayAlpha for all other faces.</summary>
            Overlay = 3
        }
        private struct Object
        {
            internal int ObjectIndex;
            internal int[] FaceListIndices;
            internal ObjectType Type;
        }
        private static Object[] ObjectList = new Object[256];
        private static int ObjectListCount = 0;

        // face lists
        private struct ObjectFace
        {
            internal int ObjectListIndex;
            internal int ObjectIndex;
            internal const int MeshIndex = 0;
            internal int FaceIndex;
        }
        // opaque
        private static ObjectFace[] OpaqueList = new ObjectFace[256];
        internal static int OpaqueListCount = 0;
        // transparent color
        private static ObjectFace[] TransparentColorList = new ObjectFace[256];
        private static double[] TransparentColorListDistance = new double[256];
        internal static int TransparentColorListCount = 0;
        // alpha
        private static ObjectFace[] AlphaList = new ObjectFace[256];
        private static double[] AlphaListDistance = new double[256];
        internal static int AlphaListCount = 0;
        // overlay
        private static ObjectFace[] OverlayList = new ObjectFace[256];
        private static double[] OverlayListDistance = new double[256];
        internal static int OverlayListCount = 0;

        // current opengl data
        private static AlphaFunction AlphaFuncComparison = 0;
        private static float AlphaFuncValue = 0.0f;
        private static bool BlendEnabled = false;
        private static bool AlphaTestEnabled = false;
        private static bool CullEnabled = true;
        internal static bool LightingEnabled = false;
        internal static bool FogEnabled = false;
        private static bool TexturingEnabled = false;
        private static bool EmissiveEnabled = false;
        internal static bool TransparentColorDepthSorting = false;

        // options
        internal static bool OptionLighting = true;
        internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
        internal static Color24 OptionDiffuseColor = new Color24(159, 159, 159);
        internal static Vector3 OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
        internal static float OptionLightingResultingAmount = 1.0f;
        internal static bool OptionFauxNighttime = true;
        internal static bool OptionNormals = false;
        internal static bool OptionWireframe = false;
        internal static bool OptionBackfaceCulling = true;
        internal static bool OptionCoordinateSystem = false;
        internal static bool OptionInterface = true;

        // background color
        internal static int BackgroundColor = 0;
        internal const int MaxBackgroundColor = 4;
        internal static string GetBackgroundColorName()
        {
            switch (BackgroundColor)
            {
                case 0: return "Light Gray";
                case 1: return "White";
                case 2: return "Black";
                case 3: return "Dark Gray";
                default: return "Custom";
            }
        }
        internal static void ApplyBackgroundColor()
        {
            switch (BackgroundColor)
            {
                case 0:
                    GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
                    break;
                case 1:
                    GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
                    break;
                case 2:
                    GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                    break;
                case 3:
                    GL.ClearColor(0.33f, 0.33f, 0.33f, 1.0f);
                    break;
            }
        }
        internal static void ApplyBackgroundColor(byte red, byte green, byte blue)
        {
            GL.ClearColor((float)red / 255.0f, (float)green / 255.0f, (float)blue / 255.0f, 1.0f);
        }

        // constants
        private const float inv255 = 1.0f / 255.0f;

        // reset
        internal static void Reset()
        {
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            ObjectList = new Object[256];
            ObjectListCount = 0;
            OpaqueList = new ObjectFace[256];
            OpaqueListCount = 0;
            TransparentColorList = new ObjectFace[256];
            TransparentColorListDistance = new double[256];
            TransparentColorListCount = 0;
            AlphaList = new ObjectFace[256];
            AlphaListDistance = new double[256];
            AlphaListCount = 0;
            OverlayList = new ObjectFace[256];
            OverlayListDistance = new double[256];
            OverlayListCount = 0;
            OptionLighting = true;
            OptionAmbientColor = new Color24(160, 160, 160);
            OptionDiffuseColor = new Color24(160, 160, 160);
            OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
            OptionLightingResultingAmount = 1.0f;
            GL.Disable(EnableCap.Fog); FogEnabled = false;
        }

        // initialize
        internal static void Initialize()
        {
            // opengl
            //GL.ShadeModel(ShadingModel.Decal); // what is decal?
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            if (!TexturingEnabled)
            {
                GL.Enable(EnableCap.Texture2D);
                TexturingEnabled = true;
            }
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.CullFace(CullFaceMode.Front);
            GL.Disable(EnableCap.Dither);
            // opengl
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PushMatrix();
            GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
            var mat = Matrix4d.LookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
            GL.MultMatrix(ref mat);
            GL.PopMatrix();
            TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.Bilinear;
        }

        // initialize lighting
        internal static void InitializeLighting()
        {
            if (OptionAmbientColor.R == 255 & OptionAmbientColor.G == 255 & OptionAmbientColor.B == 255 & OptionDiffuseColor.R == 0 & OptionDiffuseColor.G == 0 & OptionDiffuseColor.B == 0)
            {
                OptionLighting = false;
            }
            else
            {
                OptionLighting = true;
            }
            if (OptionLighting)
            {
                GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
                GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
                GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                GL.Enable(EnableCap.Lighting); LightingEnabled = true;
                GL.Enable(EnableCap.Light0);
                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
                GL.ShadeModel(ShadingModel.Smooth);
                OptionLightingResultingAmount = (float)((int)OptionAmbientColor.R + (int)OptionAmbientColor.G + (int)OptionAmbientColor.B) / 480.0f;
                if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
            }
            else
            {
                GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            }
        }

        // render scene
        internal static byte[] PixelBuffer = null;
        internal static int PixelBufferOpenGlTextureIndex = 0;
        internal static void RenderScene()
        {
            // initialize
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PushMatrix();
            if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet)
            {
                LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
                ReAddObjects();
            }
            // setup camera
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            double dx = World.AbsoluteCameraDirection.X;
            double dy = World.AbsoluteCameraDirection.Y;
            double dz = World.AbsoluteCameraDirection.Z;
            double ux = World.AbsoluteCameraUp.X;
            double uy = World.AbsoluteCameraUp.Y;
            double uz = World.AbsoluteCameraUp.Z;
            var mat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
            GL.MultMatrix(ref mat);
            if (OptionLighting)
            {
                GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OptionLightPosition.X, (float)OptionLightPosition.Y, (float)OptionLightPosition.Z, 0.0f });
            }
            // render polygons
            GL.Disable(EnableCap.DepthTest);
            if (OptionLighting)
            {
                if (!LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                    LightingEnabled = true;
                }
            }
            else if (LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting);
                LightingEnabled = false;
            }
            GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
            BlendEnabled = false; GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            LastBoundTexture = 0;
            // opaque list
            if (OptionCoordinateSystem)
            {
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                }
                GL.Color3(1.0, 0.0, 0.0);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(100.0, 0.01, 0.01), cx, cy, cz);
                GL.Color3(0.0, 1.0, 0.0);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(0.01, 100.0, 0.01), cx, cy, cz);
                GL.Color3(0.0, 0.0, 1.0);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(0.01, 0.01, 100.0), cx, cy, cz);
                if (LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                }
            }
            for (int i = 0; i < OpaqueListCount; i++)
            {
                RenderFace(ref OpaqueList[i], cx, cy, cz);
            }
            // transparent color list
			if (TransparentColorDepthSorting) {
				SortPolygons(TransparentColorList, TransparentColorListCount, TransparentColorListDistance, 1, 0.0);
				BlendEnabled = true; GL.Enable(EnableCap.Blend);
				for (int i = 0; i < TransparentColorListCount; i++) {
					GL.DepthMask(false);
					SetAlphaFunc(AlphaFunction.Less, 1.0f);
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
					GL.DepthMask(true);
					SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
				}
			} else {
				for (int i = 0; i < TransparentColorListCount; i++) {
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
				}
			}
			// alpha list
			SortPolygons(AlphaList, AlphaListCount, AlphaListDistance, 2, 0.0);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth) {
				BlendEnabled = true; GL.Enable(EnableCap.Blend);
				bool depthMask = true;
				for (int i = 0; i < AlphaListCount; i++) {
					int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
					if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Additive) {
						if (depthMask) {
							GL.DepthMask(false);
							depthMask = false;
						}
						SetAlphaFunc(AlphaFunction.Greater, 0.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
					} else {
						if (depthMask) {
							GL.DepthMask(false);
							depthMask = false;
						}
						SetAlphaFunc(AlphaFunction.Less, 1.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
						GL.DepthMask(true);
						depthMask = true;
						SetAlphaFunc(AlphaFunction.Equal, 1.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
					}
				}
			} else {
				BlendEnabled = true; GL.Enable(EnableCap.Blend);
				GL.DepthMask(false);
				SetAlphaFunc(AlphaFunction.Greater,  0.0f);
				for (int i = 0; i < AlphaListCount; i++) {
					RenderFace(ref AlphaList[i], cx, cy, cz);
				}
			}
            // overlay list
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            if (FogEnabled)
            {
                GL.Disable(EnableCap.Fog); FogEnabled = false;
            }
            SortPolygons(OverlayList, OverlayListCount, OverlayListDistance, 3, 0.0);
            for (int i = 0; i < OverlayListCount; i++)
            {
                RenderFace(ref OverlayList[i], cx, cy, cz);
            }
            // render overlays
            BlendEnabled = false; GL.Disable(EnableCap.Blend);
            SetAlphaFunc(AlphaFunction.Greater, 0.9f);
            AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.DepthTest);
            if (LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting);
                LightingEnabled = false;
            }
            if (OptionCoordinateSystem)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
                GL.Color4(1.0, 0.0, 0.0, 0.2);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(100.0, 0.01, 0.01), cx, cy, cz);
                GL.Color4(0.0, 1.0, 0.0, 0.2);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(0.01, 100.0, 0.01), cx, cy, cz);
                GL.Color4(0.0, 0.0, 1.0, 0.2);
                RenderBox(new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0), new Vector3(0.0, 1.0, 0.0), new Vector3(1.0, 0.0, 0.0), new Vector3(0.01, 0.01, 100.0), cx, cy, cz);
            }
            RenderOverlays();
            // finalize rendering
            GL.PopMatrix();
        }

        // set alpha func
        private static void SetAlphaFunc(AlphaFunction Comparison, float Value)
        {
            AlphaFuncComparison = Comparison;
            AlphaFuncValue = Value;
            GL.AlphaFunc(Comparison, Value);
        }

        // render face
        private static int LastBoundTexture = 0;
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
            RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
        }
        private static void RenderFace(ref World.MeshMaterial Material, World.Vertex[] Vertices, ref World.MeshFace Face, double CameraX, double CameraY, double CameraZ)
        {
            // texture
            int OpenGlNighttimeTextureIndex = Material.NighttimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.NighttimeTextureIndex, TextureManager.UseMode.Normal) : 0;
            int OpenGlDaytimeTextureIndex = Material.DaytimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.DaytimeTextureIndex, TextureManager.UseMode.Normal) : 0;
            if (OpenGlDaytimeTextureIndex != 0)
            {
                if (!TexturingEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    TexturingEnabled = true;
                }
                if (OpenGlDaytimeTextureIndex != LastBoundTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, OpenGlDaytimeTextureIndex);
                    LastBoundTexture = OpenGlDaytimeTextureIndex;
                }
                if (TextureManager.Textures[Material.DaytimeTextureIndex].Transparency != TextureManager.TextureTransparencyMode.None)
                {
                    if (!AlphaTestEnabled)
                    {
                        GL.Enable(EnableCap.AlphaTest);
                        AlphaTestEnabled = true;
                    }
                }
                else if (AlphaTestEnabled)
                {
                    GL.Disable(EnableCap.AlphaTest);
                    AlphaTestEnabled = false;
                }
            }
            else
            {
                if (TexturingEnabled)
                {
                    GL.Disable(EnableCap.Texture2D);
                    TexturingEnabled = false;
                    LastBoundTexture = 0;
                }
                if (AlphaTestEnabled)
                {
                    GL.Disable(EnableCap.AlphaTest);
                    AlphaTestEnabled = false;
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
            else if (OpenGlNighttimeTextureIndex == 0)
            {
                float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
                if (blend > 1.0f) blend = 1.0f;
                factor = 1.0f - 0.8f * blend;
            }
            else
            {
                factor = 1.0f;
            }
            if (OpenGlNighttimeTextureIndex != 0)
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
            if (OpenGlDaytimeTextureIndex != 0)
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
            if (OpenGlNighttimeTextureIndex != 0)
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
                GL.BindTexture(TextureTarget.Texture2D, OpenGlNighttimeTextureIndex);
                LastBoundTexture = 0;
                SetAlphaFunc(AlphaFunction.Greater, 0.0f);
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
                if (AlphaFuncValue != 0.0)
                {
                    GL.AlphaFunc(AlphaFuncComparison, AlphaFuncValue);
                }
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
                if (AlphaTestEnabled)
                {
                    GL.Disable(EnableCap.AlphaTest);
                    AlphaTestEnabled = false;
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

        // render cube
        private static void RenderBox(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, Vector3 Size, double CameraX, double CameraY, double CameraZ)
        {
            if (TexturingEnabled)
            {
                GL.Disable(EnableCap.Texture2D);
                TexturingEnabled = false;
            }
            Vector3[] v = new Vector3[8];
            v[0] = new Vector3(Size.X, Size.Y, -Size.Z);
            v[1] = new Vector3(Size.X, -Size.Y, -Size.Z);
            v[2] = new Vector3(-Size.X, -Size.Y, -Size.Z);
            v[3] = new Vector3(-Size.X, Size.Y, -Size.Z);
            v[4] = new Vector3(Size.X, Size.Y, Size.Z);
            v[5] = new Vector3(Size.X, -Size.Y, Size.Z);
            v[6] = new Vector3(-Size.X, -Size.Y, Size.Z);
            v[7] = new Vector3(-Size.X, Size.Y, Size.Z);
            for (int i = 0; i < 8; i++)
            {
                World.Rotate(ref v[i].X, ref v[i].Y, ref v[i].Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                v[i].X += Position.X - CameraX;
                v[i].Y += Position.Y - CameraY;
                v[i].Z += Position.Z - CameraZ;
            }
            int[][] Faces = new int[6][];
            Faces[0] = new int[] { 0, 1, 2, 3 };
            Faces[1] = new int[] { 0, 4, 5, 1 };
            Faces[2] = new int[] { 0, 3, 7, 4 };
            Faces[3] = new int[] { 6, 5, 4, 7 };
            Faces[4] = new int[] { 6, 7, 3, 2 };
            Faces[5] = new int[] { 6, 2, 1, 5 };
            for (int i = 0; i < 6; i++)
            {
                GL.Begin(PrimitiveType.Quads);
                for (int j = 0; j < 4; j++)
                {
                    GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
                }
                GL.End();
            }
        }

        // render overlays
        private static void RenderOverlays()
        {
            // initialize
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            // render
            if (OptionInterface)
            {
                if (ObjectManager.ObjectsUsed == 0 & ObjectManager.AnimatedWorldObjectsUsed == 0)
                {
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F7" }, new string[] { "F8" } };
                    RenderKeys(4.0, 4.0, 20.0, Keys);
                    RenderString(32.0, 4.0, Fonts.FontType.Small, "Open one or more objects", -1, 1.0f, 1.0f, 1.0f, true);
                    RenderString(32.0, 24.0, Fonts.FontType.Small, "Display the options window", -1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 8.0, (double)ScreenHeight - 20.0, Fonts.FontType.Small, "v" + System.Windows.Forms.Application.ProductVersion, 1, 1.0f, 1.0f, 1.0f, true);
                }
                else
                {
                    RenderString((double)0.5 * ScreenWidth - 88.0, 4.0, Fonts.FontType.Small, "Position: " + World.AbsoluteCameraPosition.X.ToString("0.00", Culture) + ", " + World.AbsoluteCameraPosition.Y.ToString("0.00", Culture) + ", " + World.AbsoluteCameraPosition.Z.ToString("0.00", Culture), -1, 1.0f, 1.0f, 1.0f, true);
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F5" }, new string[] { "F7" }, new string[] { "del" }, new string[] { "F8" } };
                    RenderKeys(4.0, 4.0, 24.0, Keys);
                    RenderString(32.0, 4.0, Fonts.FontType.Small, "Reload the currently open objects", -1, 1.0f, 1.0f, 1.0f, true);
                    RenderString(32.0, 24.0, Fonts.FontType.Small, "Open additional objects", -1, 1.0f, 1.0f, 1.0f, true);
                    RenderString(32.0, 44.0, Fonts.FontType.Small, "Clear currently open objects", -1, 1.0f, 1.0f, 1.0f, true);
                    RenderString(32.0, 64.0, Fonts.FontType.Small, "Display the options window", -1, 1.0f, 1.0f, 1.0f, true);
                    Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "L" }, new string[] { "G" }, new string[] { "B" }, new string[] { "I" } };
                    RenderKeys((double)ScreenWidth - 20.0, 4.0, 16.0, Keys);
                    RenderString((double)ScreenWidth - 28.0, 4.0, Fonts.FontType.Small, "Wireframe: " + (Renderer.OptionWireframe ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 28.0, 24.0, Fonts.FontType.Small, "Normals: " + (Renderer.OptionNormals ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 28.0, 44.0, Fonts.FontType.Small, "Lighting: " + (Program.LightingTarget == 0 ? "night" : "day"), 1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 28.0, 64.0, Fonts.FontType.Small, "Grid: " + (Renderer.OptionCoordinateSystem ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 28.0, 84.0, Fonts.FontType.Small, "Background: " + GetBackgroundColorName(), 1, 1.0f, 1.0f, 1.0f, true);
                    RenderString((double)ScreenWidth - 28.0, 104.0, Fonts.FontType.Small, "Hide interface", 1, 1.0f, 1.0f, 1.0f, true);
                    Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
                    RenderKeys(4.0, (double)ScreenHeight - 40.0, 16.0, Keys);
                    Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
                    RenderKeys(0.5 * (double)ScreenWidth - 28.0, (double)ScreenHeight - 40.0, 16.0, Keys);
                    Keys = new string[][] { new string[] { null, "8", "9" }, new string[] { "4", "5", "6" }, new string[] { null, "2", "3" } };
                    RenderKeys((double)ScreenWidth - 60.0, (double)ScreenHeight - 60.0, 16.0, Keys);
                    if (Interface.MessageCount == 1)
                    {
                        Keys = new string[][] { new string[] { "F9" } };
                        RenderKeys(4.0, 92.0, 20.0, Keys);
                        RenderString(32.0, 92.0, Fonts.FontType.Small, "Display the 1 message recently generated.", -1, 1.0f, 0.5f, 0.5f, true);
                    }
                    else if (Interface.MessageCount > 1)
                    {
                        Keys = new string[][] { new string[] { "F9" } };
                        RenderKeys(4.0, 92.0, 20.0, Keys);
                        RenderString(32.0, 92.0, Fonts.FontType.Small, "Display the " + Interface.MessageCount.ToString(Culture) + " messages recently generated.", -1, 1.0f, 0.5f, 0.5f, true);
                    }
                }
            }
            // finalize
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        // render keys
        private static void RenderKeys(double Left, double Top, double Width, string[][] Keys)
        {
            double py = Top;
            for (int y = 0; y < Keys.Length; y++)
            {
                double px = Left;
                for (int x = 0; x < Keys[y].Length; x++)
                {
                    if (Keys[y][x] != null)
                    {
                        GL.Color4(0.25, 0.25, 0.25, 0.5);
                        RenderOverlaySolid(px - 1.0, py - 1.0, px + Width + 1.0, py + 17.0);
                        GL.Color4(0.75, 0.75, 0.75, 0.5);
                        RenderOverlaySolid(px - 1.0, py - 1.0, px + Width - 1.0, py + 15.0);
                        GL.Color4(0.5, 0.5, 0.5, 0.5);
                        RenderOverlaySolid(px, py, px + Width, py + 16.0);
                        RenderString(px + 2.0, py, Fonts.FontType.Small, Keys[y][x], -1, 1.0f, 1.0f, 1.0f, true);
                    }
                    px += Width + 4.0;
                }
                py += 20.0;
            }
        }

        // render string
        private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, bool Shadow)
        {
            RenderString(PixelLeft, PixelTop, FontType, Text, Orientation, R, G, B, 1.0f, Shadow);
        }
        private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, float A, bool Shadow)
        {
            if (Text == null) return;
            int Font = (int)FontType;
            double c = 1;
            double x = PixelLeft;
            double y = PixelTop;
            double tw = 0.0;
            for (int i = 0; i < Text.Length; i++)
            {
                int a = char.ConvertToUtf32(Text, i);
                if (a >= 0x10000)
                {
                    i++;
                }
                Fonts.GetTextureIndex(FontType, a);
                tw += Fonts.Characters[Font][a].Width;
            }
            if (Orientation == 0)
            {
                x -= 0.5 * tw;
            }
            else if (Orientation == 1)
            {
                x -= tw;
            }
            for (int i = 0; i < Text.Length; i++)
            {
                int b = char.ConvertToUtf32(Text, i);
                if (b >= 0x10000)
                {
                    i++;
                }
                int t = Fonts.GetTextureIndex(FontType, b);
                double w = (double)TextureManager.Textures[t].ClipWidth;
                double h = (double)TextureManager.Textures[t].ClipHeight;
                GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);
                GL.Color3(A, A, A);
                RenderOverlayTexture(t, x, y, x + w, y + h);
                if (Shadow)
                {
                    RenderOverlayTexture(t, x + c, y + c, x + w, y + h);
                }
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                GL.Color4(R, G, B, A);
                RenderOverlayTexture(t, x, y, x + w, y + h);
                x += Fonts.Characters[Font][b].Width;
            }
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        // render overlay texture
        private static void RenderOverlayTexture(int TextureIndex, double ax, double ay, double bx, double by)
        {
            double nay = (double)ScreenHeight - ay;
            double nby = (double)ScreenHeight - by;
            TextureManager.UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
            if (TextureIndex >= 0)
            {
                int OpenGlTextureIndex = TextureManager.Textures[TextureIndex].OpenGlTextureIndex;
                if (!TexturingEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    TexturingEnabled = true;
                }
                GL.BindTexture(TextureTarget.Texture2D, OpenGlTextureIndex);
            }
            else if (TexturingEnabled)
            {
                GL.Disable(EnableCap.Texture2D);
                TexturingEnabled = false;
            }
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0, 1.0);
            GL.Vertex2(ax, nby);
            GL.TexCoord2(0.0, 0.0);
            GL.Vertex2(ax, nay);
            GL.TexCoord2(1.0, 0.0);
            GL.Vertex2(bx, nay);
            GL.TexCoord2(1.0, 1.0);
            GL.Vertex2(bx, nby);
            GL.End();
        }

        // render overlay solid
        private static void RenderOverlaySolid(double ax, double ay, double bx, double by)
        {
            double nay = (double)ScreenHeight - ay;
            double nby = (double)ScreenHeight - by;
            if (TexturingEnabled)
            {
                GL.Disable(EnableCap.Texture2D);
                TexturingEnabled = false;
            }
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0, 1.0);
            GL.Vertex2(ax, nby);
            GL.TexCoord2(0.0, 0.0);
            GL.Vertex2(ax, nay);
            GL.TexCoord2(1.0, 0.0);
            GL.Vertex2(bx, nay);
            GL.TexCoord2(1.0, 1.0);
            GL.Vertex2(bx, nby);
            GL.End();
        }

        // readd objects
        private static void ReAddObjects()
        {
            Object[] List = new Object[ObjectListCount];
            for (int i = 0; i < ObjectListCount; i++)
            {
                List[i] = ObjectList[i];
            }
            for (int i = 0; i < List.Length; i++)
            {
                HideObject(List[i].ObjectIndex);
            }
            for (int i = 0; i < List.Length; i++)
            {
                ShowObject(List[i].ObjectIndex, List[i].Type);
            }
        }

        // show object
        internal static void ShowObject(int ObjectIndex, ObjectType Type)
        {
            bool Overlay = Type == ObjectType.Overlay;
            if (ObjectManager.Objects[ObjectIndex] == null) return;
            if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0)
            {
                if (ObjectListCount >= ObjectList.Length)
                {
                    Array.Resize<Object>(ref ObjectList, ObjectList.Length << 1);
                }
                ObjectList[ObjectListCount].ObjectIndex = ObjectIndex;
                ObjectList[ObjectListCount].Type = Type;
                int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
                ObjectList[ObjectListCount].FaceListIndices = new int[f];
                for (int i = 0; i < f; i++)
                {
                    if (Overlay)
                    {
                        // overlay
                        if (OverlayListCount >= OverlayList.Length)
                        {
                            Array.Resize(ref OverlayList, OverlayList.Length << 1);
                            Array.Resize(ref OverlayListDistance, OverlayList.Length);
                        }
                        OverlayList[OverlayListCount].ObjectIndex = ObjectIndex;
                        OverlayList[OverlayListCount].FaceIndex = i;
                        OverlayList[OverlayListCount].ObjectListIndex = ObjectListCount;
                        ObjectList[ObjectListCount].FaceListIndices[i] = (OverlayListCount << 2) + 3;
                        OverlayListCount++;
                    }
                    else
                    {
                        int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
                        bool transparentcolor = false, alpha = false;
                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
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
                        else
                        {
                            int tday = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTextureIndex;
                            if (tday >= 0)
                            {
                                TextureManager.UseTexture(tday, TextureManager.UseMode.Normal);
                                if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.TransparentColor)
                                {
                                    transparentcolor = true;
                                }
                            }
                            int tnight = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTextureIndex;
                            if (tnight >= 0)
                            {
                                TextureManager.UseTexture(tnight, TextureManager.UseMode.Normal);
                                if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.TransparentColor)
                                {
                                    transparentcolor = true;
                                }
                            }
                        }
                        if (alpha)
                        {
                            // alpha
                            if (AlphaListCount >= AlphaList.Length)
                            {
                                Array.Resize(ref AlphaList, AlphaList.Length << 1);
                                Array.Resize(ref AlphaListDistance, AlphaList.Length);
                            }
                            AlphaList[AlphaListCount].ObjectIndex = ObjectIndex;
                            AlphaList[AlphaListCount].FaceIndex = i;
                            AlphaList[AlphaListCount].ObjectListIndex = ObjectListCount;
                            ObjectList[ObjectListCount].FaceListIndices[i] = (AlphaListCount << 2) + 2;
                            AlphaListCount++;
                        }
                        else if (transparentcolor)
                        {
                            // transparent color
                            if (TransparentColorListCount >= TransparentColorList.Length)
                            {
                                Array.Resize(ref TransparentColorList, TransparentColorList.Length << 1);
                                Array.Resize(ref TransparentColorListDistance, TransparentColorList.Length);
                            }
                            TransparentColorList[TransparentColorListCount].ObjectIndex = ObjectIndex;
                            TransparentColorList[TransparentColorListCount].FaceIndex = i;
                            TransparentColorList[TransparentColorListCount].ObjectListIndex = ObjectListCount;
                            ObjectList[ObjectListCount].FaceListIndices[i] = (TransparentColorListCount << 2) + 1;
                            TransparentColorListCount++;
                        }
                        else
                        {
                            // opaque
                            if (OpaqueListCount >= OpaqueList.Length)
                            {
                                Array.Resize(ref OpaqueList, OpaqueList.Length << 1);
                            }
                            OpaqueList[OpaqueListCount].ObjectIndex = ObjectIndex;
                            OpaqueList[OpaqueListCount].FaceIndex = i;
                            OpaqueList[OpaqueListCount].ObjectListIndex = ObjectListCount;
                            ObjectList[ObjectListCount].FaceListIndices[i] = OpaqueListCount << 2;
                            OpaqueListCount++;
                        }
                    }
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectListCount + 1;
                ObjectListCount++;
            }
        }

        // hide object
        internal static void HideObject(int ObjectIndex)
        {
            if (ObjectManager.Objects[ObjectIndex] == null) return;
            int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
            if (k >= 0)
            {
                // remove faces
                for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++)
                {
                    int h = ObjectList[k].FaceListIndices[i];
                    int hi = h >> 2;
                    switch (h & 3)
                    {
                        case 0:
                            // opaque
                            OpaqueList[hi] = OpaqueList[OpaqueListCount - 1];
                            OpaqueListCount--;
                            ObjectList[OpaqueList[hi].ObjectListIndex].FaceListIndices[OpaqueList[hi].FaceIndex] = h;
                            break;
                        case 1:
                            // transparent color
                            TransparentColorList[hi] = TransparentColorList[TransparentColorListCount - 1];
                            TransparentColorListCount--;
                            ObjectList[TransparentColorList[hi].ObjectListIndex].FaceListIndices[TransparentColorList[hi].FaceIndex] = h;
                            break;
                        case 2:
                            // alpha
                            AlphaList[hi] = AlphaList[AlphaListCount - 1];
                            AlphaListCount--;
                            ObjectList[AlphaList[hi].ObjectListIndex].FaceListIndices[AlphaList[hi].FaceIndex] = h;
                            break;
                        case 3:
                            // overlay
                            OverlayList[hi] = OverlayList[OverlayListCount - 1];
                            OverlayListCount--;
                            ObjectList[OverlayList[hi].ObjectListIndex].FaceListIndices[OverlayList[hi].FaceIndex] = h;
                            break;
                    }
                }
                // remove object
                if (k == ObjectListCount - 1)
                {
                    ObjectListCount--;
                }
                else
                {
                    ObjectList[k] = ObjectList[ObjectListCount - 1];
                    ObjectListCount--;
                    for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++)
                    {
                        int h = ObjectList[k].FaceListIndices[i];
                        int hi = h >> 2;
                        switch (h & 3)
                        {
                            case 0:
                                OpaqueList[hi].ObjectListIndex = k;
                                break;
                            case 1:
                                TransparentColorList[hi].ObjectListIndex = k;
                                break;
                            case 2:
                                AlphaList[hi].ObjectListIndex = k;
                                break;
                            case 3:
                                OverlayList[hi].ObjectListIndex = k;
                                break;
                        }
                    }
                    ObjectManager.Objects[ObjectList[k].ObjectIndex].RendererIndex = k + 1;
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
            }
        }

        // sort polygons
        private static void SortPolygons(ObjectFace[] List, int ListCount, double[] ListDistance, int ListOffset, double TimeElapsed)
        {
            // calculate distance
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            for (int i = 0; i < ListCount; i++)
            {
                int o = List[i].ObjectIndex;
                int f = List[i].FaceIndex;
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
                    if (t != 0.0)
                    {
                        t = 1.0 / Math.Sqrt(t);
                        dx *= t; dy *= t; dz *= t;
                        double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
                        t = dx * w0x + dy * w0y + dz * w0z;
                        ListDistance[i] = -t * t;
                    }
                }
            }
            // sort
            Array.Sort<double, ObjectFace>(ListDistance, List, 0, ListCount);
            // update object list
            for (int i = 0; i < ListCount; i++)
            {
                ObjectList[List[i].ObjectListIndex].FaceListIndices[List[i].FaceIndex] = (i << 2) + ListOffset;
            }
        }

        // get distance factor
        private static double GetDistanceFactor(World.Vertex[] Vertices, ref World.MeshFace Face, ushort GlowAttenuationData, double CameraX, double CameraY, double CameraZ)
        {
            if (Face.Vertices.Length != 0)
            {
                World.GlowAttenuationMode mode; double halfdistance;
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
                            t *= t; halfdistance *= halfdistance;
                            return t / (t + halfdistance * halfdistance);
                        }
                    default:
                        return 1.0;
                }
            }
            else
            {
                return 1.0;
            }
        }

    }
}