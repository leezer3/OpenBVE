using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Objects;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FileSystem;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;
using Vector3 = OpenBveApi.Math.Vector3;

namespace ObjectViewer.Graphics
{
	internal class NewRenderer : BaseRenderer
	{
		// stats
		internal bool RenderStatsOverlay = true;

		// options
		internal bool OptionCoordinateSystem = false;
		internal bool OptionInterface = true;

		// background color
		internal int BackgroundColor = 0;
		internal Color128 TextColor = Color128.White;
		internal const int MaxBackgroundColor = 4;

		private Cube redAxisVAO;
		private Cube greenAxisVAO;
		private Cube blueAxisVAO;

		public override void Initialize()
		{
			base.Initialize();

			if (!ForceLegacyOpenGL)
			{
				redAxisVAO = new Cube(this, Color128.Red);
				greenAxisVAO = new Cube(this, Color128.Green);
				blueAxisVAO = new Cube(this, Color128.Blue);
			}
		}

		internal string GetBackgroundColorName()
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

		internal void ApplyBackgroundColor()
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

		internal void ApplyBackgroundColor(byte red, byte green, byte blue)
		{
			GL.ClearColor(red / 255.0f, green / 255.0f, blue / 255.0f, 1.0f);
		}

		// render scene
		internal void RenderScene(double timeElapsed)
		{
			lastObjectState = null;
			if (AvailableNewRenderer)
			{
				CurrentShader.Deactivate();
			}
			ReleaseResources();
			// initialize
			ResetOpenGlState();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			UpdateViewport(ViewportChangeMode.ChangeToScenery);
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			TransformedLightPosition = new Vector3(Lighting.OptionLightPosition.X, Lighting.OptionLightPosition.Y, -Lighting.OptionLightPosition.Z);
			TransformedLightPosition.Transform(CurrentViewMatrix);
			if (!AvailableNewRenderer)
			{
				GL.Light(LightName.Light0, LightParameter.Position, new[] { (float)TransformedLightPosition.X, (float)TransformedLightPosition.Y, (float)TransformedLightPosition.Z, 0.0f });
			}

			Lighting.OptionLightingResultingAmount = (Lighting.OptionAmbientColor.R + Lighting.OptionAmbientColor.G + Lighting.OptionAmbientColor.B) / 480.0f;

			if (Lighting.OptionLightingResultingAmount > 1.0f)
			{
				Lighting.OptionLightingResultingAmount = 1.0f;
			}

			Fog.Enabled = false;

			if (OptionCoordinateSystem)
			{
				UnsetAlphaFunc();

				if (AvailableNewRenderer)
				{
					redAxisVAO.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), Camera.AbsolutePosition, null);
					greenAxisVAO.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), Camera.AbsolutePosition, null);
					blueAxisVAO.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), Camera.AbsolutePosition, null);
				}
				else
				{
					GL.Color4(1.0, 0.0, 0.0, 0.2);
					Cube.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(100.0, 0.01, 0.01), Camera.AbsolutePosition, null);
					GL.Color4(0.0, 1.0, 0.0, 0.2);
					Cube.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 100.0, 0.01), Camera.AbsolutePosition, null);
					GL.Color4(0.0, 0.0, 1.0, 0.2);
					Cube.Draw(Vector3.Zero, Vector3.Forward, Vector3.Down, Vector3.Right, new Vector3(0.01, 0.01, 100.0), Camera.AbsolutePosition, null);
				}
			}
			GL.Disable(EnableCap.DepthTest);
			// opaque face
			if (AvailableNewRenderer)
			{
				//Setup the shader for rendering the scene
				DefaultShader.Activate();
				if (OptionLighting)
				{
					DefaultShader.SetIsLight(true);
					DefaultShader.SetLightPosition(TransformedLightPosition);
					DefaultShader.SetLightAmbient(Lighting.OptionAmbientColor);
					DefaultShader.SetLightDiffuse(Lighting.OptionDiffuseColor);
					DefaultShader.SetLightSpecular(Lighting.OptionSpecularColor);
					DefaultShader.SetLightModel(Lighting.LightModel);
				}
				DefaultShader.SetTexture(0);
				DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			}
			ResetOpenGlState();
			List<FaceState> opaqueFaces, alphaFaces;
			lock (VisibleObjects.LockObject)
			{
				opaqueFaces = VisibleObjects.OpaqueFaces.ToList();
				alphaFaces = VisibleObjects.GetSortedPolygons();
			}

			foreach (FaceState face in opaqueFaces)
			{
				face.Draw();
			}

			// alpha face
			ResetOpenGlState();

			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				SetBlendFunc();
				SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				GL.DepthMask(false);

				foreach (FaceState face in alphaFaces)
				{
					face.Draw();
				}
			}
			else
			{
				UnsetBlendFunc();
				SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);

				foreach (FaceState face in alphaFaces)
				{
					if (face.Object.Prototype.Mesh.Materials[face.Face.Material].BlendMode == MeshMaterialBlendMode.Normal && face.Object.Prototype.Mesh.Materials[face.Face.Material].GlowAttenuationData == 0)
					{
						if (face.Object.Prototype.Mesh.Materials[face.Face.Material].Color.A == 255)
						{
							face.Draw();
						}
					}
				}

				SetBlendFunc();
				SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;

				foreach (FaceState face in alphaFaces)
				{
					if (face.Object.Prototype.Mesh.Materials[face.Face.Material].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							UnsetAlphaFunc();
							additive = true;
						}

						face.Draw();
					}
					else
					{
						if (additive)
						{
							SetAlphaFunc();
							additive = false;
						}

						face.Draw();
					}
				}
			}

			if (AvailableNewRenderer)
			{
				DefaultShader.Deactivate();
				lastVAO = -1;
			}

			// render overlays
			ResetOpenGlState();
			OptionLighting = false;
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //FIXME: Remove when text switches between two renderer types
			RenderOverlays(timeElapsed);
			OptionLighting = true;
		}

		private void RenderOverlays(double timeElapsed)
		{
			//Initialize openGL
			SetBlendFunc();
			PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Screen.Width, Screen.Height, 0.0f, -1.0f, 1.0f, out CurrentProjectionMatrix);
			PushMatrix(MatrixMode.Modelview);
			CurrentViewMatrix = Matrix4D.Identity;

			CultureInfo culture = CultureInfo.InvariantCulture;
			double scaleFactor = Program.Renderer.ScaleFactor.X;

			if (OptionInterface)
			{
				string[][] keys;
				int objectCount, animatedObjectsUsed, opaqueFaces, alphaFaces;
				lock (VisibilityUpdateLock)
				{
					objectCount = VisibleObjects.Objects.Count;
					animatedObjectsUsed = ObjectManager.AnimatedWorldObjectsUsed;
					opaqueFaces = VisibleObjects.OpaqueFaces.Count;
					alphaFaces = VisibleObjects.AlphaFaces.Count;
				}
				if (objectCount == 0 && ObjectManager.AnimatedWorldObjectsUsed == 0)
				{
					int errorPos;
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						keys = new[] { new[] { "esc" }};
						Keys.Render(4, 4, 20, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Display the menu", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, $"v{Application.ProductVersion}", new Vector2(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, TextColor);
						errorPos = 24;
					}
					else
					{
						keys = new[] { new[] { "F7" }, new[] { "F8" }, new[] { "F10" } };
						Keys.Render(4, 4, 20, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Open one or more objects", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Display the train settings window", new Vector2(32 * scaleFactor, 44), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, $"v{Application.ProductVersion}", new Vector2(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, TextColor);
						errorPos = 64;
					}
					
					if (Interface.LogMessages.Count == 1)
					{
						Keys.Render(4, errorPos, 20, Fonts.SmallFont, new[] { new[] { "F9" } });

						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 error message recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 message recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, TextColor);
						}
					}
					else if (Interface.LogMessages.Count > 1)
					{
						Keys.Render(4, errorPos, 20, Fonts.SmallFont, new[] { new[] { "F9" } });
						bool error = Interface.LogMessages.Any(x => x.Type != MessageType.Information);

						if (error)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count.ToString(culture)} error messages recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
						else
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count.ToString(culture)} messages recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, TextColor);
						}
					}
				}
				else
				{
					OpenGlString.Draw(Fonts.SmallFont, $"Position: {Camera.AbsolutePosition.X.ToString("0.00", culture)}, {Camera.AbsolutePosition.Y.ToString("0.00", culture)}, {Camera.AbsolutePosition.Z.ToString("0.00", culture)}", new Vector2((int)(0.5 * Screen.Width - 88), 4), TextAlignment.TopLeft, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, ForceLegacyOpenGL ? "Renderer: Old (GL 1.2)- GL 4 not available" : $"Renderer: {(AvailableNewRenderer ? "New (GL 4)" : "Old (GL 1.2)")}", new Vector2((int)(0.5 * Screen.Width - 88), 24), TextAlignment.TopLeft, Color128.White);

					int errorPos;
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						keys = new[] { new[] { "F5" }, new[] { "esc" }, new[] { "del" }};
						Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Reload the currently open objects", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Show the menu", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Clear currently open objects", new Vector2(32 * scaleFactor, 44), TextAlignment.TopLeft, TextColor);
						errorPos = 72;
					}
					else
					{
						OpenGlString.Draw(Fonts.SmallFont, $"Position: {Camera.AbsolutePosition.X.ToString("0.00", culture)}, {Camera.AbsolutePosition.Y.ToString("0.00", culture)}, {Camera.AbsolutePosition.Z.ToString("0.00", culture)}", new Vector2((int)(0.5 * Screen.Width - 88), 4), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, ForceLegacyOpenGL ? "Renderer: Old (GL 1.2)- GL 4 not available" : $"Renderer: {(AvailableNewRenderer ? "New (GL 4)" : "Old (GL 1.2)")}", new Vector2((int)(0.5 * Screen.Width - 88), 24), TextAlignment.TopLeft, Color128.White);
						keys = new[] { new[] { "F5" }, new[] { "F7" }, new[] { "del" }, new[] { "F8" }, new[] { "F10" } };
						Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Reload the currently open objects", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Open additional objects", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Clear currently open objects", new Vector2(32 * scaleFactor, 44), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32 * scaleFactor, 64), TextAlignment.TopLeft, TextColor);
						OpenGlString.Draw(Fonts.SmallFont, "Display the train settings window", new Vector2(32 * scaleFactor, 84), TextAlignment.TopLeft, TextColor);
						errorPos = 112;
					}
					

					keys = new[] { new[] { "F" }, new[] { "N" }, new[] { "L" }, new[] { "G" }, new[] { "B" }, new[] { "I" }, new[] { "R" } };
					Keys.Render(Screen.Width - 20, 4, 16, Fonts.SmallFont, keys);
					keys = new[] { new[] { "F11" } };
					Keys.Render(Screen.Width - 36, 124, 32, Fonts.SmallFont, keys);
					
					OpenGlString.Draw(Fonts.SmallFont, $"WireFrame: {(OptionWireFrame ? "on" : "off")}", new Vector2(Screen.Width - (28 * scaleFactor), 4), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, $"Normals: {(OptionNormals ? "on" : "off")}", new Vector2(Screen.Width - (28 * scaleFactor), 24), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, $"Lighting: {(Program.LightingTarget == 0 ? "night" : "day")}", new Vector2(Screen.Width - (28 * scaleFactor), 44), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, $"Grid: {(OptionCoordinateSystem ? "on" : "off")}", new Vector2(Screen.Width - (28 * scaleFactor), 64), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, $"Background: {GetBackgroundColorName()}", new Vector2(Screen.Width - (28 * scaleFactor), 84), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, "Hide interface:", new Vector2(Screen.Width - (28 * scaleFactor), 104), TextAlignment.TopRight, TextColor);
					OpenGlString.Draw(Fonts.SmallFont, $"{(RenderStatsOverlay ? "Hide" : "Show")} renderer statistics", new Vector2(Screen.Width - (44 * scaleFactor), 124), TextAlignment.TopRight, TextColor);
					if (!ForceLegacyOpenGL)
					{
						OpenGlString.Draw(Fonts.SmallFont, "Switch renderer type:", new Vector2(Screen.Width - (28 * scaleFactor), 144), TextAlignment.TopRight, TextColor);
						keys = new[] { new[] { "R" } };
						Keys.Render(Screen.Width - 20, 144, 16, Fonts.SmallFont, keys);
					}
					
					keys = new[] { new[] { null, "W", null }, new[] { "A", "S", "D" } };
					Keys.Render(4, Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "↑", null }, new[] { "←", "↓", "→" } };
					Keys.Render((int)(0.5 * Screen.Width - 28), Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "8", "9" }, new[] { "4", "5", "6" }, new[] { null, "2", "3" } };
					Keys.Render(Screen.Width - 60, Screen.Height - 60, 16, Fonts.SmallFont, keys);

					if (Interface.LogMessages.Count == 1)
					{
						Keys.Render(4, errorPos, 20, Fonts.SmallFont, new[] { new[] { "F9" } });

						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 error message recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 message recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, TextColor);
						}
					}
					else if (Interface.LogMessages.Count > 1)
					{
						Keys.Render(4, errorPos, 20, Fonts.SmallFont, new[] { new[] { "F9" } });
						bool error = Interface.LogMessages.Any(x => x.Type != MessageType.Information);

						if (error)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count.ToString(culture)} error messages recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, new Color128(1.0f, 0.5f, 0.5f));
						}
						else
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count.ToString(culture)} messages recently generated.", new Vector2(32 * scaleFactor, errorPos), TextAlignment.TopLeft, TextColor);
						}
					}

					if (RenderStatsOverlay)
					{
						Keys.Render(4, Screen.Height - 126, 116, Fonts.SmallFont, new[] { new[] { "Renderer Statistics" } });
						OpenGlString.Draw(Fonts.SmallFont, $"Total static objects: {objectCount}", new Vector2(4, Screen.Height - 112), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total animated objects: {animatedObjectsUsed}", new Vector2(4, Screen.Height - 100), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Current frame rate: {FrameRate.ToString("0.0", culture)}fps", new Vector2(4, Screen.Height - 88), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total opaque faces: {opaqueFaces}", new Vector2(4, Screen.Height - 76), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total alpha faces: {alphaFaces}", new Vector2(4, Screen.Height - 64), TextAlignment.TopLeft, Color128.White, true);
					}
				}
			}
			if (CurrentInterface == InterfaceType.Menu)
			{
				Game.Menu.Draw(timeElapsed);
			}
			// finalize
			PopMatrix(MatrixMode.Projection);
			PopMatrix(MatrixMode.Modelview);
		}

		public NewRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem) : base(CurrentHost, CurrentOptions, FileSystem)
		{
			Screen.Width = CurrentOptions.WindowWidth;
			Screen.Height = CurrentOptions.WindowHeight;
			CameraTrackFollower = new TrackFollower(CurrentHost);
		}
	}
}
