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
using static LibRender.CameraProperties;

namespace OpenBve
{
    internal static partial class Renderer
    {
        // first frame behavior
        internal enum LoadTextureImmediatelyMode { NotYet, Yes }
        internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;

        /// <summary>Whether to enforce updating all display lists.</summary>
        internal static bool StaticOpaqueForceUpdate = true;
		
        // options
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

        // reset
        internal static void Reset()
        {
			LibRender.Renderer.Reset();
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            StaticOpaqueForceUpdate = true;
        }

        // initialize
        internal static void Initialize()
        {
            LibRender.Renderer.Initialize();
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
                LibRender.Renderer.ReAddObjects();
            }
	        
            // setup camera
            var mat = Matrix4d.LookAt(0.0, 0.0, 0.0, Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, Camera.AbsoluteDirection.Z, Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, Camera.AbsoluteUp.Z);
            GL.MultMatrix(ref mat);
            if (LibRender.Renderer.OptionLighting)
            {
                GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)LibRender.Renderer.OptionLightPosition.X, (float)LibRender.Renderer.OptionLightPosition.Y, (float)LibRender.Renderer.OptionLightPosition.Z, 0.0f });
            }
            // render polygons
            GL.Disable(EnableCap.DepthTest);
            if (LibRender.Renderer.OptionLighting)
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
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), Camera.AbsolutePosition, null);
                GL.Color3(0.0, 1.0, 0.0);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), Camera.AbsolutePosition, null);
                GL.Color3(0.0, 0.0, 1.0);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), Camera.AbsolutePosition, null);
                if (LibRender.Renderer.LightingEnabled)
                {
                    GL.Enable(EnableCap.Lighting);
                }
            }
            LibRender.Renderer.ResetOpenGlState();
            // render background

			GL.Disable(EnableCap.DepthTest);

			for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
			{
				if (LibRender.Renderer.StaticOpaque[i] != null)
				{
					if (LibRender.Renderer.StaticOpaque[i].Update | StaticOpaqueForceUpdate)
					{
						LibRender.Renderer.StaticOpaque[i].Update = false;
						if (LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
						{
							GL.DeleteLists(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList, 1);
							LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = false;
						}
						if (LibRender.Renderer.StaticOpaque[i].List.FaceCount != 0)
						{
							LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
							LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = true;
							LibRender.Renderer.ResetOpenGlState();
							GL.NewList(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
							for (int j = 0; j < LibRender.Renderer.StaticOpaque[i].List.FaceCount; j++)
							{
								if (LibRender.Renderer.StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref LibRender.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsolutePosition);
								}
							}
							GL.EndList();
						}

						LibRender.Renderer.StaticOpaque[i].WorldPosition = Camera.AbsolutePosition;
					}
				}
			}
			StaticOpaqueForceUpdate = false;
			for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
			{
				if (LibRender.Renderer.StaticOpaque[i] != null && LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
				{
					LibRender.Renderer.ResetOpenGlState();
					GL.PushMatrix();
					GL.Translate(LibRender.Renderer.StaticOpaque[i].WorldPosition.X - Camera.AbsolutePosition.X, LibRender.Renderer.StaticOpaque[i].WorldPosition.Y - Camera.AbsolutePosition.Y, LibRender.Renderer.StaticOpaque[i].WorldPosition.Z - Camera.AbsolutePosition.Z);
					GL.CallList(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList);
					GL.PopMatrix();
				}
			}
			//Update bounding box positions now we've rendered the objects
			int currentBox = 0;
			for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
			{
			if (LibRender.Renderer.StaticOpaque[i] != null)
				{
					currentBox++;
					
				}
			}

			// dynamic opaque
			LibRender.Renderer.ResetOpenGlState();
			for (int i = 0; i < LibRender.Renderer.DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref LibRender.Renderer.DynamicOpaque.Faces[i], Camera.AbsolutePosition);
			}
			// dynamic alpha
			LibRender.Renderer.ResetOpenGlState();
			LibRender.Renderer.DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
				}
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
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), Camera.AbsolutePosition, null);
                GL.Color4(0.0, 1.0, 0.0, 0.2);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), Camera.AbsolutePosition, null);
                GL.Color4(0.0, 0.0, 1.0, 0.2);
                LibRender.Renderer.DrawCube(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), Camera.AbsolutePosition, null);
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
			    if (!LibRender.Renderer.OptionBackfaceCulling || (Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
			    {
				    GL.Disable(EnableCap.CullFace);
				    LibRender.Renderer.CullEnabled = false;
			    }
		    }
		    else if (LibRender.Renderer.OptionBackfaceCulling)
		    {
			    if ((Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
			    {
				    GL.Enable(EnableCap.CullFace);
				    LibRender.Renderer.CullEnabled = true;
			    }
		    }
		    int r = (int)Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Material;
		    LibRender.Renderer.RenderFace(ref Face.ObjectReference.Mesh.Materials[r], Face.ObjectReference.Mesh.Vertices, Face.Wrap, ref Face.ObjectReference.Mesh.Faces[Face.FaceIndex], Camera);
	    }
	    
        // render overlays
        private static void RenderOverlays()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
	        GL.MatrixMode(MatrixMode.Projection);
	        GL.PushMatrix();
	        GL.LoadIdentity();
	        GL.Ortho(0.0, (double) Screen.Width, (double) Screen.Height, 0.0, -1.0, 1.0);
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
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, TextColor);
                }
                else
                {
	                LibRender.Renderer.DrawString(Fonts.SmallFont, "Position: " + Camera.AbsolutePosition.X.ToString("0.00", Culture) + ", " + Camera.AbsolutePosition.Y.ToString("0.00", Culture) + ", " + Camera.AbsolutePosition.Z.ToString("0.00", Culture), new Point((int)(0.5 * Screen.Width -88),4),TextAlignment.TopLeft, TextColor);
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F5" }, new string[] { "F7" }, new string[] { "del" }, new string[] { "F8" }, new string[] { "F10" } };
                    LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Reload the currently open objects", new Point(32,4),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Open additional objects", new Point(32,24),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Clear currently open objects", new Point(32,44),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32,64),TextAlignment.TopLeft, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the train settings window", new Point(32, 84), TextAlignment.TopLeft, TextColor);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "L" }, new string[] { "G" }, new string[] { "B" }, new string[] { "I" } };
                    LibRender.Renderer.RenderKeys(Screen.Width - 20, 4, 16, Fonts.SmallFont, Keys);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Wireframe: " + (LibRender.Renderer.OptionWireframe ? "on" : "off"), new Point(Screen.Width - 28,4),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Normals: " + (LibRender.Renderer.OptionNormals ? "on" : "off"), new Point(Screen.Width - 28,24),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Lighting: " + (Program.LightingTarget == 0 ? "night" : "day"), new Point(Screen.Width - 28,44),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Grid: " + (Renderer.OptionCoordinateSystem ? "on" : "off"), new Point(Screen.Width - 28,64),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Background: " + GetBackgroundColorName(), new Point(Screen.Width - 28,84),TextAlignment.TopRight, TextColor);
                    LibRender.Renderer.DrawString(Fonts.SmallFont, "Hide interface", new Point(Screen.Width - 28,104),TextAlignment.TopRight, TextColor);
                    Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
                    LibRender.Renderer.RenderKeys(4, Screen.Height - 40, 16, Fonts.SmallFont, Keys);
                    Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
                    LibRender.Renderer.RenderKeys((int)(0.5 * (double) Screen.Width - 28), Screen.Height - 40, 16, Fonts.SmallFont, Keys);
                    Keys = new string[][] { new string[] { null, "8", "9" }, new string[] { "4", "5", "6" }, new string[] { null, "2", "3" } };
                    LibRender.Renderer.RenderKeys(Screen.Width - 60, Screen.Height - 60, 16, Fonts.SmallFont, Keys);
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
	        GL.Ortho(0.0, (double) Screen.Width, 0.0, (double) Screen.Height, -1.0, 1.0);
        }
		
    }
}
