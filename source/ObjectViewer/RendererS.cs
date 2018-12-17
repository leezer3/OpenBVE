// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Structure Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveShared;

namespace OpenBve
{
    internal static partial class Renderer
    {
        // current opengl data
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
			OpenBveShared.Renderer.OptionLighting = true;
            OpenBveShared.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
	        OpenBveShared.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
	        OpenBveShared.Renderer.OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
            OpenBveShared.Renderer.OptionLightingResultingAmount = 1.0f;
            GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
        }

        internal static void RenderScene()
        {
	        OpenBveShared.Renderer.ResetOpenGlState();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
			//UpdateViewport(ViewPortChangeMode.ChangeToScenery);
			// set up camera
			double dx = Camera.AbsoluteCameraDirection.X;
			double dy = Camera.AbsoluteCameraDirection.Y;
			double dz = Camera.AbsoluteCameraDirection.Z;
			double ux = Camera.AbsoluteCameraUp.X;
			double uy = Camera.AbsoluteCameraUp.Y;
			double uz = Camera.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OpenBveShared.Renderer.OptionLightPosition.X, (float)OpenBveShared.Renderer.OptionLightPosition.Y, (float)OpenBveShared.Renderer.OptionLightPosition.Z, 0.0f });
			// fog
			double fd = OpenBveShared.Renderer.NextFog.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				OpenBveShared.Renderer.CurrentFog.Start = OpenBveShared.Renderer.PreviousFog.Start * frc + OpenBveShared.Renderer.NextFog.Start * fr;
				OpenBveShared.Renderer.CurrentFog.End = OpenBveShared.Renderer.PreviousFog.End * frc + OpenBveShared.Renderer.NextFog.End * fr;
				OpenBveShared.Renderer.CurrentFog.Color.R = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.R * frc + (float)OpenBveShared.Renderer.NextFog.Color.R * fr);
				OpenBveShared.Renderer.CurrentFog.Color.G = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.G * frc + (float)OpenBveShared.Renderer.NextFog.Color.G * fr);
				OpenBveShared.Renderer.CurrentFog.Color.B = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.B * frc + (float)OpenBveShared.Renderer.NextFog.Color.B * fr);
			}
			else
			{
				OpenBveShared.Renderer.CurrentFog = OpenBveShared.Renderer.PreviousFog;
			}
			// render background
			if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			// fog
			float aa = OpenBveShared.Renderer.CurrentFog.Start;
			float bb = OpenBveShared.Renderer.CurrentFog.End;
			if (aa < bb & aa < OpenBveShared.Renderer.BackgroundImageDistance)
			{
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.R, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.G, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.B, 1.0f });
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = true;
				}
			}
			else if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			// world layer
			bool optionLighting = OpenBveShared.Renderer.OptionLighting;
			OpenBveShared.Renderer.LastBoundTexture = null;
			if (OpenBveShared.Renderer.OptionLighting)
			{
				if (!OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = true;
				}
				if (OpenBveShared.Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.R, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.G, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.R, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.G, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.B, 1.0f });
				}
			}
			else if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = false;
			}
			// static opaque
			bool f = false;
			if (f) //TODO: Implement display list disabling
			{
				OpenBveShared.Renderer.ResetOpenGlState();
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].List != null)
						{
							for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
							{
								if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
								{
									OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsoluteCameraPosition);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].Update | OpenBveShared.Renderer.StaticOpaqueForceUpdate)
						{
							OpenBveShared.Renderer.StaticOpaque[i].Update = false;
							if (OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
							{
								GL.DeleteLists(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, 1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = false;
							}
							if (OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount != 0)
							{
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = true;
								OpenBveShared.Renderer.ResetOpenGlState();
								GL.NewList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
								{
									if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
									{
										OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsoluteCameraPosition);
									}
								}
								GL.EndList();
							}

							OpenBveShared.Renderer.StaticOpaque[i].WorldPosition = Camera.AbsoluteCameraPosition;
						}
					}
				}

				OpenBveShared.Renderer.StaticOpaqueForceUpdate = false;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null && OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						OpenBveShared.Renderer.ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.X - Camera.AbsoluteCameraPosition.X, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Y - Camera.AbsoluteCameraPosition.Y, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Z - Camera.AbsoluteCameraPosition.Z);
						GL.CallList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList);
						GL.PopMatrix();
					}
				}
				//Update bounding box positions now we've rendered the objects
				int currentBox = 0;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						currentBox++;

					}
				}
			}
			// dynamic opaque
			OpenBveShared.Renderer.ResetOpenGlState();
			for (int i = 0; i < OpenBveShared.Renderer.DynamicOpaque.FaceCount; i++)
			{
				OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicOpaque.Faces[i], Camera.AbsoluteCameraPosition);
			}
			// dynamic alpha
			OpenBveShared.Renderer.ResetOpenGlState();
			OpenBveShared.Renderer.DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = false;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							OpenBveShared.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
					}
				}
			}
			GL.LoadIdentity();
			//UpdateViewport(ViewPortChangeMode.ChangeToCab);
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			// render overlays
			OpenBveShared.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
			OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
			OpenBveShared.Renderer.AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			if (OpenBveShared.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				OpenBveShared.Renderer.LightingEnabled = false;
			}
			RenderOverlays();
			// finalize rendering
			GL.PopMatrix();
        }

        // render cube
        private static void RenderBox(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, Vector3 Size, double CameraX, double CameraY, double CameraZ)
        {
            if (OpenBveShared.Renderer.TexturingEnabled)
            {
                GL.Disable(EnableCap.Texture2D);
	            OpenBveShared.Renderer.TexturingEnabled = false;
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
	            v[i].Rotate(Direction, Up, Side);
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
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	        GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
	        GL.MatrixMode(MatrixMode.Projection);
	        GL.PushMatrix();
	        GL.LoadIdentity();
	        GL.Ortho(0.0, (double)OpenBveShared.Renderer.Width, (double)OpenBveShared.Renderer.Height, 0.0, -1.0, 1.0);
	        GL.MatrixMode(MatrixMode.Modelview);
	        GL.PushMatrix();
	        GL.LoadIdentity();
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            // render
            if (OptionInterface)
            {
                if (GameObjectManager.ObjectsUsed == 0 & GameObjectManager.AnimatedWorldObjectsUsed == 0)
                {
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F7" }, new string[] { "F8" } };
	                OpenBveShared.Renderer.RenderKeys(4, 4, 20, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open one or more objects", new Point(32,4),TextAlignment.TopLeft, Color128.White);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32,24),TextAlignment.TopLeft, Color128.White);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(OpenBveShared.Renderer.Width - 8, OpenBveShared.Renderer.Height - 20),TextAlignment.TopLeft, TextColor);
                }
                else
                {
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Position: " + Camera.AbsoluteCameraPosition.X.ToString("0.00", Culture) + ", " + Camera.AbsoluteCameraPosition.Y.ToString("0.00", Culture) + ", " + Camera.AbsoluteCameraPosition.Z.ToString("0.00", Culture), new Point((int)(0.5 * OpenBveShared.Renderer.Width -88),4),TextAlignment.TopLeft, TextColor);
                    string[][] Keys;
                    Keys = new string[][] { new string[] { "F5" }, new string[] { "F7" }, new string[] { "del" }, new string[] { "F8" } };
	                OpenBveShared.Renderer.RenderKeys(4, 4, 24, Keys);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Reload the currently open objects", new Point(32,4),TextAlignment.TopLeft, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open additional objects", new Point(32,24),TextAlignment.TopLeft, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Clear currently open objects", new Point(32,44),TextAlignment.TopLeft, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32,64),TextAlignment.TopLeft, TextColor);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "L" }, new string[] { "G" }, new string[] { "B" }, new string[] { "I" } };
	                OpenBveShared.Renderer.RenderKeys(OpenBveShared.Renderer.Width - 20, 4, 16, Keys);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Wireframe: " + (OpenBveShared.Renderer.OptionWireframe ? "on" : "off"), new Point(OpenBveShared.Renderer.Width - 28,4),TextAlignment.TopRight, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Normals: " + (OpenBveShared.Renderer.OptionNormals ? "on" : "off"), new Point(OpenBveShared.Renderer.Width - 28,24),TextAlignment.TopRight, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Lighting: " + (Program.LightingTarget == 0 ? "night" : "day"), new Point(OpenBveShared.Renderer.Width - 28,44),TextAlignment.TopRight, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Grid: " + (Renderer.OptionCoordinateSystem ? "on" : "off"), new Point(OpenBveShared.Renderer.Width - 28,64),TextAlignment.TopRight, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Background: " + GetBackgroundColorName(), new Point(OpenBveShared.Renderer.Width - 28,84),TextAlignment.TopRight, TextColor);
	                OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Hide interface", new Point(OpenBveShared.Renderer.Width - 28,104),TextAlignment.TopRight, Color128.White);
                    Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
	                OpenBveShared.Renderer.RenderKeys(4, OpenBveShared.Renderer.Height - 40, 16, Keys);
                    Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
	                OpenBveShared.Renderer.RenderKeys((int)(0.5 * OpenBveShared.Renderer.Width - 28), OpenBveShared.Renderer.Height - 40, 16, Keys);
                    Keys = new string[][] { new string[] { null, "8", "9" }, new string[] { "4", "5", "6" }, new string[] { null, "2", "3" } };
	                OpenBveShared.Renderer.RenderKeys(OpenBveShared.Renderer.Width - 60, OpenBveShared.Renderer.Height - 60, 16, Keys);
                    if (Interface.MessageCount == 1)
                    {
                        Keys = new string[][] { new string[] { "F9" } };
	                    OpenBveShared.Renderer.RenderKeys(4, 92, 20, Keys);
	                    if (Interface.LogMessages[0].Type != MessageType.Information)
	                    {
		                    OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32,92),TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
	                    else
	                    {
							//If all of our messages are information, then print the message text in grey
		                    OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32,92),TextAlignment.TopLeft, TextColor);
						}
						
                    }
                    else if (Interface.MessageCount > 1)
                    {
                        Keys = new string[][] { new string[] { "F9" } };
	                    OpenBveShared.Renderer.RenderKeys(4, 92, 20, Keys);
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
		                    OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount.ToString(Culture) + " error messages recently generated.", new Point(32,92),TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
	                    else
	                    {
		                    OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount.ToString(Culture) + " messages recently generated.", new Point(32,92),TextAlignment.TopLeft, TextColor);
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
	        GL.Ortho(0.0, (double)OpenBveShared.Renderer.Width, 0.0, (double)OpenBveShared.Renderer.Height, -1.0, 1.0);
        }
    }
}
