// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Structure Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;

namespace OpenBve
{
    internal static partial class Renderer
    {

        // screen (output window)
        internal static int ScreenWidth = 960;
        internal static int ScreenHeight = 600;

        // first frame behavior
        internal enum LoadTextureImmediatelyMode { NotYet, Yes }
        internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;

        // object list
        
        private struct Object
        {
            internal int ObjectIndex;
            internal int[] FaceListIndices;
            internal ObjectType Type;
        }
        private static Object[] ObjectList = new Object[256];
        private static int ObjectListCount = 0;

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
        internal static bool TransparentColorDepthSorting = false;

        // options
        internal static bool OptionLighting = true;
        internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
        internal static Color24 OptionDiffuseColor = new Color24(159, 159, 159);
        internal static Vector3 OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
        internal static float OptionLightingResultingAmount = 1.0f;
        internal static bool OptionNormals = false;
        internal static bool OptionWireframe = false;
        internal static bool OptionBackfaceCulling = true;
        internal static bool OptionCoordinateSystem = false;
        internal static bool OptionInterface = true;

        // background color
        internal static int BackgroundColor = 0;
	    internal static Color128 TextColor = Color128.White;
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
					TextColor = Color128.White;
                    break;
                case 1:
                    GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
	                TextColor = Color128.Black;
                    break;
                case 2:
                    GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
	                TextColor = Color128.White;
                    break;
                case 3:
                    GL.ClearColor(0.33f, 0.33f, 0.33f, 1.0f);
	                TextColor = Color128.White;
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
            GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
        }

        // initialize
        internal static void Initialize()
        {
            LibRender.Renderer.Initialize();
            TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality & Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
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
                GL.Enable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = true;
                GL.Enable(EnableCap.Light0);
                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
                GL.ShadeModel(ShadingModel.Smooth);
                OptionLightingResultingAmount = (float)((int)OptionAmbientColor.R + (int)OptionAmbientColor.G + (int)OptionAmbientColor.B) / 480.0f;
                if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
            }
            else
            {
                GL.Disable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = false;
            }
        }
		
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
            var mat = Matrix4d.LookAt(0.0, 0.0, 0.0, World.AbsoluteCameraDirection.X, World.AbsoluteCameraDirection.Y, World.AbsoluteCameraDirection.Z, World.AbsoluteCameraUp.X, World.AbsoluteCameraUp.Y, World.AbsoluteCameraUp.Z);
            GL.MultMatrix(ref mat);
            if (OptionLighting)
            {
                GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OptionLightPosition.X, (float)OptionLightPosition.Y, (float)OptionLightPosition.Z, 0.0f });
            }
            // render polygons
            GL.Disable(EnableCap.DepthTest);
            if (OptionLighting)
            {
                if (!LibRender.Renderer.LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                    LibRender.Renderer.LightingEnabled = true;
                }
            }
            else if (LibRender.Renderer.LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting);
                LibRender.Renderer.LightingEnabled = false;
            }
            GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
            LibRender.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            //LastBoundTexture = 0;
            // opaque list
            if (OptionCoordinateSystem)
            {
                if (LibRender.Renderer.LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                }
                GL.Color3(1.0, 0.0, 0.0);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), World.AbsoluteCameraPosition, null);
                GL.Color3(0.0, 1.0, 0.0);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), World.AbsoluteCameraPosition, null);
                GL.Color3(0.0, 0.0, 1.0);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), World.AbsoluteCameraPosition, null);
                if (LibRender.Renderer.LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                }
            }
            LibRender.Renderer.ResetOpenGlState();
            for (int i = 0; i < OpaqueListCount; i++)
            {
                RenderFace(ref OpaqueList[i], World.AbsoluteCameraPosition);
            }
            LibRender.Renderer.ResetOpenGlState();
            // transparent color list
	        SortPolygons(TransparentColorList, TransparentColorListCount, TransparentColorListDistance, 1, 0.0);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality) {
				
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < TransparentColorListCount; i++)
				{
					int r = (int)ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Faces[TransparentColorList[i].FaceIndex].Material;
					if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < TransparentColorListCount; i++)
				{
					int r = (int)ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Faces[TransparentColorList[i].FaceIndex].Material;
					if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.AlphaTestEnabled = false;
							GL.Disable(EnableCap.AlphaTest);
							additive = true;
						}
						RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
					}
				}
			} else {
				for (int i = 0; i < TransparentColorListCount; i++) {
					RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
				}
			}
			LibRender.Renderer.ResetOpenGlState();
			SortPolygons(AlphaList, AlphaListCount, AlphaListDistance, 2, 0.0);
	        if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
	        {
		        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
		        GL.DepthMask(false);
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
		        }
	        }
	        else
	        {
		        GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
		        GL.DepthMask(true);
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
			        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
			        {
				        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
				        {
					        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
				        }
			        }
		        }
		        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
		        GL.DepthMask(false);
		        bool additive = false;
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
			        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
			        {
				        if (!additive)
				        {
					        LibRender.Renderer.AlphaTestEnabled = false;
					        GL.Disable(EnableCap.AlphaTest);
					        additive = true;
				        }
				        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
			        }
			        else
			        {
				        if (additive)
				        {
					        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					        additive = false;
				        }
				        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
			        }
		        }
	        }
            // overlay list
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            if (LibRender.Renderer.FogEnabled)
            {
                GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
            }
            SortPolygons(OverlayList, OverlayListCount, OverlayListDistance, 3, 0.0);
            for (int i = 0; i < OverlayListCount; i++)
            {
                RenderFace(ref OverlayList[i], World.AbsoluteCameraPosition);
            }
	        
            // render overlays
            LibRender.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
            LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
            LibRender.Renderer.AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.DepthTest);
            if (LibRender.Renderer.LightingEnabled)
            {
                GL.Disable(EnableCap.Lighting);
                LibRender.Renderer.LightingEnabled = false;
            }
            if (OptionCoordinateSystem)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
                GL.Color4(1.0, 0.0, 0.0, 0.2);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), World.AbsoluteCameraPosition, null);
                GL.Color4(0.0, 1.0, 0.0, 0.2);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), World.AbsoluteCameraPosition, null);
                GL.Color4(0.0, 0.0, 1.0, 0.2);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), World.AbsoluteCameraPosition, null);
            }
	        RenderOverlays();
	        LibRender.Renderer.LastBoundTexture = null; //We bind the character texture, so must reset it at the end of the render sequence
            // finalize rendering
            GL.PopMatrix();
        }

        private static void RenderFace(ref ObjectFace Face, Vector3 Camera)
	    {
		    if (LibRender.Renderer.CullEnabled)
		    {
			    if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
			    {
				    GL.Disable(EnableCap.CullFace);
				    LibRender.Renderer.CullEnabled = false;
			    }
		    }
		    else if (OptionBackfaceCulling)
		    {
			    if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
			    {
				    GL.Enable(EnableCap.CullFace);
				    LibRender.Renderer.CullEnabled = true;
			    }
		    }
		    int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
		    LibRender.Renderer.RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], Camera);
	    }
	    
        // render overlays
        private static void RenderOverlays()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
	        GL.MatrixMode(MatrixMode.Projection);
	        GL.PushMatrix();
	        GL.LoadIdentity();
	        GL.Ortho(0.0, (double)ScreenWidth, (double)ScreenHeight, 0.0, -1.0, 1.0);
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
                    Keys = new string[][] { new string[] { "F7" }, new string[] { "F8" }, new string[] { "F10" } };
                    LibRender.Renderer.RenderKeys(4, 4, 20, Fonts.SmallFont, Keys);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Open one or more objects", new Point(32,4),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32,24),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the train settings window", new Point(32, 44), TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(ScreenWidth - 8, ScreenHeight - 20), TextAlignment.TopLeft, TextColor);
                }
                else
                {
	                LibRender.Renderer.DrawString(Fonts.SmallFont, "Position: " + World.AbsoluteCameraPosition.X.ToString("0.00", Culture) + ", " + World.AbsoluteCameraPosition.Y.ToString("0.00", Culture) + ", " + World.AbsoluteCameraPosition.Z.ToString("0.00", Culture), new Point((int)(0.5 * ScreenWidth -88),4),TextAlignment.TopLeft, TextColor);
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F5" }, new string[] { "F7" }, new string[] { "del" }, new string[] { "F8" }, new string[] { "F10" } };
                    LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Reload the currently open objects", new Point(32,4),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Open additional objects", new Point(32,24),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Clear currently open objects", new Point(32,44),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32,64),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the train settings window", new Point(32, 84), TextAlignment.TopLeft, TextColor);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "L" }, new string[] { "G" }, new string[] { "B" }, new string[] { "I" } };
                    LibRender.Renderer.RenderKeys(ScreenWidth - 20, 4, 16, Fonts.SmallFont, Keys);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Wireframe: " + (Renderer.OptionWireframe ? "on" : "off"), new Point(ScreenWidth - 28,4),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Normals: " + (Renderer.OptionNormals ? "on" : "off"), new Point(ScreenWidth - 28,24),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Lighting: " + (Program.LightingTarget == 0 ? "night" : "day"), new Point(ScreenWidth - 28,44),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Grid: " + (Renderer.OptionCoordinateSystem ? "on" : "off"), new Point(ScreenWidth - 28,64),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Background: " + GetBackgroundColorName(), new Point(ScreenWidth - 28,84),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Hide interface", new Point(ScreenWidth - 28,104),TextAlignment.TopRight, TextColor);
                    Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
                    LibRender.Renderer.RenderKeys(4, ScreenHeight - 40, 16, Fonts.SmallFont, Keys);
                    Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
                    LibRender.Renderer.RenderKeys((int)(0.5 * (double)ScreenWidth - 28), ScreenHeight - 40, 16, Fonts.SmallFont, Keys);
                    Keys = new string[][] { new string[] { null, "8", "9" }, new string[] { "4", "5", "6" }, new string[] { null, "2", "3" } };
                    LibRender.Renderer.RenderKeys(ScreenWidth - 60, ScreenHeight - 60, 16, Fonts.SmallFont, Keys);
                    if (Interface.MessageCount == 1)
                    {
	                    LibRender.Renderer.RenderKeys(4, 112, 20, Fonts.SmallFont, new string[][] { new string[] { "F9" } });
	                    if (Interface.LogMessages[0].Type != MessageType.Information)
	                    {
		                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32,112),TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
	                    else
	                    {
							//If all of our messages are information, then print the message text in grey
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32,112),TextAlignment.TopLeft, TextColor);
						}
						
                    }
                    else if (Interface.MessageCount > 1)
                    {
	                    LibRender.Renderer.RenderKeys(4, 112, 20, Fonts.SmallFont, new string[][] { new string[] { "F9" } });
	                    bool error = false;
	                    for (int i = 0; i < Interface.MessageCount; i++)
	                    {
		                    if (Interface.LogMessages[i].Type != MessageType.Information)
		                    {
			                    error = true;
			                    break;
		                    }

	                    }
	                    if (error)
	                    {
		                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount.ToString(Culture) + " error messages recently generated.", new Point(32,112),TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
	                    else
	                    {
		                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount.ToString(Culture) + " messages recently generated.", new Point(32,112),TextAlignment.TopLeft, TextColor);
						}
						
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
	        GL.Ortho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
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
	                    OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
                        bool transparentcolor = false, alpha = false;
                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
                        {
                            alpha = true;
                        }
                        else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
                        {
                            alpha = true;
                        }
                        else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0)
                        {
                            alpha = true;
                        }
                        else
                        {
                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null)
                            {
	                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode == null)
	                            {
		                            
		                            // If the object does not have a stored wrapping mode, determine it now
		                            for (int v = 0; v < ObjectManager.Objects[ObjectIndex].Mesh.Vertices.Length; v++)
		                            {
			                            if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
			                                ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
			                            {
				                            wrap |= OpenGlTextureWrapMode.RepeatClamp;
			                            }
			                            if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
			                                ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
			                            {
				                            wrap |= OpenGlTextureWrapMode.ClampRepeat;
			                            }
		                            }

		                            ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode = wrap;
	                            }
	                            else
	                            {
		                            //Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
		                            wrap = (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode;
	                            }
	                            Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture, (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode);
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency == TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency == TextureTransparencyType.Partial)
                                {
                                    transparentcolor = true;
                                }
                            }
                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                            {
	                            Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture, (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode);
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency == TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency == TextureTransparencyType.Partial)
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
							AlphaList[AlphaListCount] = new ObjectFace();
                            AlphaList[AlphaListCount].ObjectIndex = ObjectIndex;
                            AlphaList[AlphaListCount].FaceIndex = i;
                            AlphaList[AlphaListCount].ObjectListIndex = ObjectListCount;
	                        AlphaList[AlphaListCount].Wrap = wrap;
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
							TransparentColorList[TransparentColorListCount] = new ObjectFace();
                            TransparentColorList[TransparentColorListCount].ObjectIndex = ObjectIndex;
                            TransparentColorList[TransparentColorListCount].FaceIndex = i;
                            TransparentColorList[TransparentColorListCount].ObjectListIndex = ObjectListCount;
	                        TransparentColorList[TransparentColorListCount].Wrap = wrap;
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
							OpaqueList[OpaqueListCount] = new ObjectFace();
                            OpaqueList[OpaqueListCount].ObjectIndex = ObjectIndex;
                            OpaqueList[OpaqueListCount].FaceIndex = i;
                            OpaqueList[OpaqueListCount].ObjectListIndex = ObjectListCount;
	                        OpaqueList[OpaqueListCount].Wrap = wrap;
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
    }
}
