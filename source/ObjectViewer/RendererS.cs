// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Structure Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using LibRender;
using OpenBve.RouteManager;
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
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            Objects = new RendererObject[256];
            ObjectCount = 0;
            StaticOpaque = new ObjectGroup[] { };
            StaticOpaqueForceUpdate = true;
            DynamicOpaque = new ObjectList();
            DynamicAlpha = new ObjectList();
            OverlayOpaque = new ObjectList();
            OverlayAlpha = new ObjectList();
            LibRender.Renderer.OptionLighting = true;
            LibRender.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
            LibRender.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
            LibRender.Renderer.OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
            LibRender.Renderer.OptionLightingResultingAmount = 1.0f;
            GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
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
                ReAddObjects();
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
							LibRender.Renderer.ResetOpenGlState();
							GL.NewList(StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
							for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
							{
								if (StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref StaticOpaque[i].List.Faces[j], Camera.AbsolutePosition);
								}
							}
							GL.EndList();
						}
						StaticOpaque[i].WorldPosition = Camera.AbsolutePosition;
					}
				}
			}
			StaticOpaqueForceUpdate = false;
			for (int i = 0; i < StaticOpaque.Length; i++)
			{
				if (StaticOpaque[i] != null && StaticOpaque[i].OpenGlDisplayListAvailable)
				{
					LibRender.Renderer.ResetOpenGlState();
					GL.PushMatrix();
					GL.Translate(StaticOpaque[i].WorldPosition.X - Camera.AbsolutePosition.X, StaticOpaque[i].WorldPosition.Y - Camera.AbsolutePosition.Y, StaticOpaque[i].WorldPosition.Z - Camera.AbsolutePosition.Z);
					GL.CallList(StaticOpaque[i].OpenGlDisplayList);
					GL.PopMatrix();
				}
			}
			//Update bounding box positions now we've rendered the objects
			int currentBox = 0;
			for (int i = 0; i < StaticOpaque.Length; i++)
			{
			if (StaticOpaque[i] != null)
				{
					currentBox++;
					
				}
			}

			// dynamic opaque
			LibRender.Renderer.ResetOpenGlState();
			for (int i = 0; i < DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref DynamicOpaque.Faces[i], Camera.AbsolutePosition);
			}
			// dynamic alpha
			LibRender.Renderer.ResetOpenGlState();
			DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
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
			    if (!LibRender.Renderer.OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
			    {
				    GL.Disable(EnableCap.CullFace);
				    LibRender.Renderer.CullEnabled = false;
			    }
		    }
		    else if (LibRender.Renderer.OptionBackfaceCulling)
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
		
        // readd objects
        private static void ReAddObjects()
        {
	        RendererObject[] list = new RendererObject[ObjectCount];
	        for (int i = 0; i < ObjectCount; i++)
	        {
		        list[i] = Objects[i];
	        }
	        for (int i = 0; i < list.Length; i++)
	        {
		        HideObject(ref ObjectManager.Objects[list[i].ObjectIndex]);
	        }
	        for (int i = 0; i < list.Length; i++)
	        {
		        ShowObject(ObjectManager.Objects[list[i].ObjectIndex], list[i].Type);
	        }
        }

		/// <summary>Makes an object visible within the world</summary>
		/// <param name="objectToShow">The object to show</param>
		/// <param name="Type">Whether this is a static or dynamic object</param>
		internal static void ShowObject(StaticObject objectToShow, ObjectType Type)
		{
			if (objectToShow == null)
			{
				return;
			}
			if (objectToShow.RendererIndex == 0)
			{
				if (ObjectCount >= Objects.Length)
				{
					Array.Resize<RendererObject>(ref Objects, Objects.Length << 1);
				}

				Objects[ObjectCount] = new RendererObject(objectToShow.ObjectIndex, Type);
				int f = objectToShow.Mesh.Faces.Length;
				Objects[ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					bool alpha = false;
					int k = objectToShow.Mesh.Faces[i].Material;
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (objectToShow.Mesh.Materials[k].DaytimeTexture != null | objectToShow.Mesh.Materials[k].NighttimeTexture != null)
					{
						if (objectToShow.Mesh.Materials[k].WrapMode == null)
						{
							// If the object does not have a stored wrapping mode, determine it now
							for (int v = 0; v < objectToShow.Mesh.Vertices.Length; v++)
							{
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.RepeatClamp;
								}
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.ClampRepeat;
								}
							}							
						}
						else
						{
							//Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
							wrap = (OpenGlTextureWrapMode)objectToShow.Mesh.Materials[k].WrapMode;
						}
						if (objectToShow.Mesh.Materials[k].DaytimeTexture != null)
						{
							if (Program.CurrentHost.LoadTexture(objectToShow.Mesh.Materials[k].DaytimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].DaytimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &&
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
						if (objectToShow.Mesh.Materials[k].NighttimeTexture != null)
						{
							if (Program.CurrentHost.LoadTexture(objectToShow.Mesh.Materials[k].NighttimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].NighttimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
					}
					if (Type == ObjectType.Overlay & CameraProperties.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].Color.A != 255)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].GlowAttenuationData != 0)
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
						int groupIndex = (int)objectToShow.GroupIndex;
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
							ObjectIndex = objectToShow.ObjectIndex,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						StaticOpaque[groupIndex].Update = true;
						Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
						LibRender.Renderer.InfoStaticOpaqueFaceCount++;

						/*
						 * Check if the given object has a bounding box, and insert it to the end of the list of bounding boxes if required
						 */
						if (objectToShow.Mesh.BoundingBox != null)
						{
							int Index = list.BoundingBoxes.Length;
							for (int j = 0; j < list.BoundingBoxes.Length; j++)
							{
								if (list.Faces[j] == null)
								{
									Index = j;
									break;
								}
							}
							if (Index == list.BoundingBoxes.Length)
							{
								Array.Resize<BoundingBox>(ref list.BoundingBoxes, list.BoundingBoxes.Length << 1);
							}
							list.BoundingBoxes[Index].Upper = objectToShow.Mesh.BoundingBox[0];
							list.BoundingBoxes[Index].Lower = objectToShow.Mesh.BoundingBox[1];
						}
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
							ObjectIndex = objectToShow.ObjectIndex,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
						list.FaceCount++;
					}

				}
				objectToShow.RendererIndex = ObjectCount + 1;
				ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world</summary>
		/// <param name="Object">The object to hide</param>
		internal static void HideObject(ref StaticObject Object)
		{
			if (Object == null)
			{
				return;
			}

			int k = Object.RendererIndex - 1;
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
						int groupIndex = (int) ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
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
						LibRender.Renderer.InfoStaticOpaqueFaceCount--;
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
						if (list.FaceCount > 0)
						{
							list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
						}

						Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
						if (list.FaceCount > 0)
						{
							list.FaceCount--;
						}
					}
				}

				// remove object
				if (k == ObjectCount - 1)
				{
					ObjectCount--;
				}
				else if (ObjectCount == 0)
				{
					return; //Outside the world?
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
								int groupIndex = (int) ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
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
						if (list.Faces[listIndex] == null)
						{
							continue;
						}

						list.Faces[listIndex].ObjectListIndex = k;
					}

					ObjectManager.Objects[Objects[k].ObjectIndex].RendererIndex = k + 1;
				}

				Object.RendererIndex = 0;
			}
		}
    }
}
