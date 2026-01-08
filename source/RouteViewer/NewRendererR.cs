using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Objects;
using LibRender2.Screens;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FileSystem;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using RouteManager2.Events;
using RouteManager2.Tracks;
using Vector2 = OpenBveApi.Math.Vector2;
using Vector3 = OpenBveApi.Math.Vector3;

namespace RouteViewer
{
	internal class NewRenderer : BaseRenderer
	{
		// stats
		internal bool RenderStatsOverlay = true;

		// options
		internal bool OptionInterface = true;
		internal bool OptionEvents = false;
		internal bool OptionPaths = false;

		// textures
		private Texture BrightnessChangeTexture;
		private Texture BackgroundChangeTexture;
		private Texture StationStartTexture;
		private Texture StationEndTexture;
		private Texture LimitTexture;
		private Texture SectionTexture;
		private Texture TransponderTexture;
		private Texture SoundTexture;
		private Texture BufferTexture;
		private Texture StopTexture;
		private Texture PointSoundTexture;
		private Texture RunSoundTexture;
		private Texture LightingEventTexture;
		private Texture WeatherEventTexture;

		public override void Initialize()
		{
			base.Initialize();

			string Folder = Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "section.png"), out SectionTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "stop.png"), out StopTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "switchsound.png"), out PointSoundTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "runsound.png"), out RunSoundTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "lighting.png"), out LightingEventTexture);
			TextureManager.RegisterTexture(Path.CombineFile(Folder, "weather.png"), out WeatherEventTexture);
		}

		// render scene
		internal void RenderScene(double timeElapsed)
		{
			lastObjectState = null;
			ReleaseResources();
			// initialize
			ResetOpenGlState();

			if (OptionWireFrame)
			{
				if (Program.CurrentRoute.CurrentFog.Start < Program.CurrentRoute.CurrentFog.End)
				{
					const float fogDistance = 600.0f;
					float n = (fogDistance - Program.CurrentRoute.CurrentFog.Start) / (Program.CurrentRoute.CurrentFog.End - Program.CurrentRoute.CurrentFog.Start);
					float cr = n * inv255 * Program.CurrentRoute.CurrentFog.Color.R;
					float cg = n * inv255 * Program.CurrentRoute.CurrentFog.Color.G;
					float cb = n * inv255 * Program.CurrentRoute.CurrentFog.Color.B;
					GL.ClearColor(cr, cg, cb, 1.0f);
				}
				else
				{
					GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
				}
			}
			else
			{
				GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
			}

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// set up camera and lighting
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			if (Lighting.ShouldInitialize)
			{
				Lighting.Initialize();
				Lighting.ShouldInitialize = false;
			}
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
			// fog
			double fd = Program.CurrentRoute.NextFog.TrackPosition - Program.CurrentRoute.PreviousFog.TrackPosition;

			if (fd != 0.0)
			{
				float fr = (float)((CameraTrackFollower.TrackPosition - Program.CurrentRoute.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Program.CurrentRoute.CurrentFog.Start = Program.CurrentRoute.PreviousFog.Start * frc + Program.CurrentRoute.NextFog.Start * fr;
				Program.CurrentRoute.CurrentFog.End = Program.CurrentRoute.PreviousFog.End * frc + Program.CurrentRoute.NextFog.End * fr;
				Program.CurrentRoute.CurrentFog.Color.R = (byte)(Program.CurrentRoute.PreviousFog.Color.R * frc + Program.CurrentRoute.NextFog.Color.R * fr);
				Program.CurrentRoute.CurrentFog.Color.G = (byte)(Program.CurrentRoute.PreviousFog.Color.G * frc + Program.CurrentRoute.NextFog.Color.G * fr);
				Program.CurrentRoute.CurrentFog.Color.B = (byte)(Program.CurrentRoute.PreviousFog.Color.B * frc + Program.CurrentRoute.NextFog.Color.B * fr);
				if (!Program.CurrentRoute.CurrentFog.IsLinear)
				{
					Program.CurrentRoute.CurrentFog.Density = Program.CurrentRoute.PreviousFog.Density * frc + Program.CurrentRoute.NextFog.Density * fr;
				}
				
			}
			else
			{
				Program.CurrentRoute.CurrentFog = Program.CurrentRoute.PreviousFog;
			}

			if (AvailableNewRenderer)
			{
				DefaultShader.Activate();
            }
			

			// render background
			GL.Disable(EnableCap.DepthTest);
			Program.CurrentRoute.UpdateBackground(timeElapsed, false);

			// fog
			float aa = Program.CurrentRoute.CurrentFog.Start;
			float bb = Program.CurrentRoute.CurrentFog.End;

			if (aa < bb & aa < Program.CurrentRoute.CurrentBackground.BackgroundImageDistance)
			{
				Fog.Enabled = true;
				Fog.Start = aa;
				Fog.End = bb;
				Fog.Color = Program.CurrentRoute.CurrentFog.Color;
				Fog.Density = Program.CurrentRoute.CurrentFog.Density;
				Fog.IsLinear = Program.CurrentRoute.CurrentFog.IsLinear;
				Fog.Set();
			}
			else
			{
				Fog.Enabled = false;
			}

			// world layer
			// opaque face
			
			
			if (AvailableNewRenderer)
			{
				//Setup the shader for rendering the scene
				if (OptionLighting)
				{
					DefaultShader.SetIsLight(true);
					DefaultShader.SetLightPosition(TransformedLightPosition);
					DefaultShader.SetLightAmbient(Lighting.OptionAmbientColor);
					DefaultShader.SetLightDiffuse(Lighting.OptionDiffuseColor);
					DefaultShader.SetLightSpecular(Lighting.OptionSpecularColor);
					DefaultShader.SetLightModel(Lighting.LightModel);
				}
				Fog.Set();
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

			if (OptionPaths)
			{
				// TODO: Write a shader to draw point list....
				ResetOpenGlState();
				if (AvailableNewRenderer)
				{
					DefaultShader.Deactivate();
				}
				unsafe
				{
					GL.MatrixMode(MatrixMode.Projection);
					GL.PushMatrix();
					fixed (double* matrixPointer = &CurrentProjectionMatrix.Row0.X)
					{
						GL.LoadMatrix(matrixPointer);
					}

					GL.MatrixMode(MatrixMode.Modelview);
					GL.PushMatrix();

					fixed (double* matrixPointer = &CurrentViewMatrix.Row0.X)
					{
						GL.LoadMatrix(matrixPointer);
					}

					Matrix4D m = Camera.TranslationMatrix;
					double* matrixPointer2 = &m.Row0.X;
					{
						GL.MultMatrix(matrixPointer2);
					}
				}

				// render track paths
				for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
				{
					int key = Program.CurrentRoute.Tracks.ElementAt(i).Key;
					trackColors[key].Render();
				}
				GL.PopMatrix();
				GL.MatrixMode(MatrixMode.Projection);
				GL.PopMatrix();
			}

			if (OptionEvents)
			{
				if (Math.Abs(CameraTrackFollower.TrackPosition - lastTrackPosition) > 50)
				{
					lastTrackPosition = CameraTrackFollower.TrackPosition;
					CubesToDraw.Clear();
					for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
					{
						int railIndex = Program.CurrentRoute.Tracks.ElementAt(i).Key;
						FindEvents(railIndex);
					}
				}
				

				for (int i = 0; i < CubesToDraw.Count; i++)
				{
					Texture T = CubesToDraw.ElementAt(i).Key;
					foreach (Vector3 v in CubesToDraw[T])
					{
						Cube.Draw(v, Camera.AbsoluteDirection, Camera.AbsoluteUp, Camera.AbsoluteSide, 0.2, Camera.AbsolutePosition, T);
					}
				}

			}

			// render overlays
			if (AvailableNewRenderer)
			{
				DefaultShader.Deactivate();
			}
			ResetOpenGlState();
			OptionLighting = false;
			Fog.Enabled = false;
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //FIXME: Remove when text switches between two renderer types
			RenderOverlays(timeElapsed);
			OptionLighting = true;
		}

		private double lastTrackPosition;

		/// <summary>Finds visible events on a rail index</summary>
		/// <param name="railIndex">The rail index</param>
		internal void FindEvents(int railIndex)
		{
			if (Program.CurrentRoute.Tracks[railIndex].Elements == null)
			{
				return;
			}

			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			OptionLighting = false;

			double da = -Camera.BackwardViewingDistance - Camera.ExtraViewingDistance;
			double db = Camera.ForwardViewingDistance + Camera.ExtraViewingDistance;
			bool[] sta = new bool[Program.CurrentRoute.Stations.Length];

			// events
			for (int i = 0; i < Program.CurrentRoute.Tracks[railIndex].Elements.Length; i++)
			{
				double p = Program.CurrentRoute.Tracks[railIndex].Elements[i].StartingTrackPosition;
				double d = p -CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					foreach (GeneralEvent e in Program.CurrentRoute.Tracks[railIndex].Elements[i].Events)
					{
						double dy, dx = 0.0, dz = 0.0;
						Texture t;

						if (e is BrightnessChangeEvent)
						{
							dy = 4.0;
							t = BrightnessChangeTexture;
						}
						else if (e is BackgroundChangeEvent)
						{
							dy = 3.5;
							t = BackgroundChangeTexture;
						}
						else if (e is StationStartEvent startEvent)
						{
							dy = 1.6;
							t = StationStartTexture;
							sta[startEvent.StationIndex] = true;
						}
						else if (e is StationEndEvent endEvent)
						{
							dy = 1.6;
							t = StationEndTexture;
							sta[endEvent.StationIndex] = true;
						}
						else if (e is LimitChangeEvent)
						{
							dy = 1.1;
							t = LimitTexture;
						}
						else if (e is SectionChangeEvent)
						{
							dy = 0.8;
							t = SectionTexture;
						}
						else if (e is TransponderEvent transponderEvent)
						{
							dy = 0.4;
							// beacon type 21 is reserved for legacy weather events
							t = transponderEvent.Type == 21 ? WeatherEventTexture : TransponderTexture;

						}
						else if (e is SoundEvent soundEvent)
						{
							dx = soundEvent.Position.X;
							dy = soundEvent.Position.Y < 0.1 ? 0.1 : soundEvent.Position.Y;
							dz = soundEvent.Position.Z;
							t = SoundTexture;
						}
						else if (e is PointSoundEvent)
						{
							dx = 0;
							dy = 0.2;
							dz = 0;
							t = PointSoundTexture;
						}
						else if (e is RailSoundsChangeEvent)
						{
							dy = 0.8;
							t = RunSoundTexture;
						}
						else if (e is LightingChangeEvent)
						{
							dy = 1.5;
							t = LightingEventTexture;
						}
						else
						{
							dy = 1.0;
							t = null;
						}

						if (t != null)
						{
							TrackFollower f = new TrackFollower(Program.CurrentHost)
							{
								TriggerType = EventTriggerType.None,
								TrackPosition = p,
								TrackIndex = railIndex
							};

							f.UpdateAbsolute(p + e.TrackPositionDelta, true, false);
							Vector3 cubePos = new Vector3(f.WorldPosition);
							cubePos.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							cubePos.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							cubePos.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;

							if (CubesToDraw.ContainsKey(t))
							{
								CubesToDraw[t].Add(cubePos);
							}
							else
							{
								CubesToDraw.Add(t, new HashSet<Vector3>());
								CubesToDraw[t].Add(cubePos);
							}
						}
					}
				}

				if (d > db)
				{
					break;
				}
			}

			if (railIndex != 0)
			{
				return;
			}

			// stops
			for (int i = 0; i < sta.Length; i++)
			{
				if (sta[i])
				{
					for (int j = 0; j < Program.CurrentRoute.Stations[i].Stops.Length; j++)
					{
						const double dy = 1.4;
						const double s = 0.2;
						double p = Program.CurrentRoute.Stations[i].Stops[j].TrackPosition;
						TrackFollower f = new TrackFollower(Program.CurrentHost)
						{
							TriggerType = EventTriggerType.None,
							TrackPosition = p
						};
						f.UpdateAbsolute(p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;

						Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.AbsolutePosition, StopTexture);
					}
				}
			}

			// buffers
			foreach (BufferStop stop in Program.CurrentRoute.BufferTrackPositions)
			{
				double d = stop.TrackPosition - Program.Renderer.CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					const double dy = 2.5;
					const double s = 0.25;
					TrackFollower f = new TrackFollower(Program.CurrentHost)
					{
						TriggerType = EventTriggerType.None,
						TrackPosition = stop.TrackPosition,
						TrackIndex = stop.TrackIndex
					};
					f.UpdateAbsolute(stop.TrackPosition, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;

					Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.AbsolutePosition, BufferTexture);
				}
			}
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

			// marker
			if (OptionInterface)
			{
				Marker.Draw(150);
			}

			if (!Program.CurrentlyLoading)
			{
				string[][] keys;
				int totalObjects = 0;
				lock (VisibleObjects.LockObject)
				{
					totalObjects += VisibleObjects.Objects.Count;
					totalObjects += ObjectManager.AnimatedWorldObjectsUsed;
				}

				double scaleFactor = Program.Renderer.ScaleFactor.X;

				if (totalObjects == 0)
				{
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						keys = new[] { new[] { "esc" } };
						Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Display the menu", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, Color128.White, true);
					}
					else
					{
						keys = new[] { new[] { "F7" }, new[] { "F8" } };
						Keys.Render(4, 4, 20, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Open route", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, Color128.White);
						OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, Color128.White);
						OpenGlString.Draw(Fonts.SmallFont, $"v{Application.ProductVersion}", new Vector2(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, Color128.White);
					}
					
				}
				else if (OptionInterface)
				{
					// keys
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						keys = new[] { new[] { "F5" }, new[] { "esc" } };
						Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Reload route", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, "Display the menu", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, Color128.White, true);
					}
					else
					{
						keys = new[] { new[] { "F5" }, new[] { "F7" }, new[] { "F8" } };
						Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
						OpenGlString.Draw(Fonts.SmallFont, "Reload route", new Vector2(32 * scaleFactor, 4), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, "Open route", new Vector2(32 * scaleFactor, 24), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32 * scaleFactor, 44), TextAlignment.TopLeft, Color128.White, true);
					}
					

					keys = new[] { new[] { "F" }, new[] { "N" }, new[] { "E" }, new[] { "M" }, new[] { "I" } };
					Keys.Render(Screen.Width - (int)(20 * scaleFactor), 4, 16, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "WireFrame:", new Vector2(Screen.Width - (32 * scaleFactor), 4), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Normals:", new Vector2(Screen.Width - (32 * scaleFactor), 24), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Events:", new Vector2(Screen.Width - (32 * scaleFactor), 44), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Mute:", new Vector2(Screen.Width - (32 * scaleFactor), 64), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Hide interface:", new Vector2(Screen.Width - (32 * scaleFactor), 84), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"{(RenderStatsOverlay ? "Hide" : "Show")} renderer statistics", new Vector2(Screen.Width - 32, 104), TextAlignment.TopRight, Color128.White, true);
					if (!ForceLegacyOpenGL)
					{
						OpenGlString.Draw(Fonts.SmallFont, "Switch renderer type:", new Vector2(Screen.Width - (32 * scaleFactor), 124), TextAlignment.TopRight, Color128.White, true);
						keys = new[] { new[] { "R" } };
						Keys.Render(Screen.Width - (int)(20 * scaleFactor), 124, 16, Fonts.SmallFont, keys);
						if (Program.CurrentHost.Platform != HostPlatform.AppleOSX || IntPtr.Size == 4)
						{
							// only works on WinForms supporting systems
							OpenGlString.Draw(Fonts.SmallFont, "Draw Rail Paths:", new Vector2(Screen.Width - (32 * scaleFactor), 144), TextAlignment.TopRight, Color128.White, true);
							keys = new[] { new[] { "P" } };
							Keys.Render(Screen.Width - (int)(20 * scaleFactor), 144, 16, Fonts.SmallFont, keys);
						}
					}
					else
					{
						if (Program.CurrentHost.Platform != HostPlatform.AppleOSX || IntPtr.Size == 4)
						{
							// only works on WinForms supporting systems
							OpenGlString.Draw(Fonts.SmallFont, "Rail Paths:", new Vector2(Screen.Width - (32 * scaleFactor), 124), TextAlignment.TopRight, Color128.White, true);
							keys = new[] { new[] { "P" } };
							Keys.Render(Screen.Width - (int)(20 * scaleFactor), 124, 16, Fonts.SmallFont, keys);
						}
					}
					

					keys = new[] { new[] { "F10" } };
					Keys.Render(Screen.Width - (int)(32 * scaleFactor), 104, 30, Fonts.SmallFont, keys);
					
					keys = new[] { new[] { null, "W", null }, new[] { "A", "S", "D" } };
					Keys.Render(4, Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "↑", null }, new[] { "←", "↓", "→" } };
					Keys.Render((int)(0.5 * Screen.Width - (48 * scaleFactor)), Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { "P↑" }, new[] { "P↓" } };
					Keys.Render((int)(0.5 * Screen.Width + 32), Screen.Height - 40, 24, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "/", "*" }, new[] { "7", "8", "9" }, new[] { "4", "5", "6" }, new[] { "1", "2", "3" }, new[] { null, "0", "." } };
					Keys.Render(Screen.Width - (int)(60 * scaleFactor), Screen.Height - 100, 16, Fonts.SmallFont, keys);

					if (Program.JumpToPositionEnabled)
					{
						Vector2 jumpToPositionPos = new Vector2(4, Interface.LogMessages.Count == 0 ? 80 : 100);
						OpenGlString.Draw(Fonts.SmallFont, "Jump to track position:", jumpToPositionPos, TextAlignment.TopLeft, Color128.White, true);
						jumpToPositionPos.Y += 20;

						if (double.TryParse(Program.JumpToPositionValue, out double distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), jumpToPositionPos, TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), jumpToPositionPos, TextAlignment.TopLeft, distance > Program.CurrentRoute.Tracks[0].Elements[Program.CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100 ? Color128.Red : Color128.Yellow, true);
							}

						}
					}

					// info
					double x = 0.5 * Screen.Width - 256.0;
					double Yaw = Camera.Alignment.Yaw * 57.2957795130824;
					OpenGlString.Draw(Fonts.SmallFont, $"Position: {GetLengthString(Camera.Alignment.TrackPosition)} (X={GetLengthString(Camera.Alignment.Position.X)}, Y={GetLengthString(Camera.Alignment.Position.Y)}), Orientation: (Yaw={Yaw.ToString("0.00", culture)}°, Pitch={(Camera.Alignment.Pitch * 57.2957795130824).ToString("0.00", culture)}°, Roll={(Camera.Alignment.Roll * 57.2957795130824).ToString("0.00", culture)}°)", new Vector2((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"Radius: {GetLengthString(CameraTrackFollower.CurveRadius)}, Cant: {(1000.0 * CameraTrackFollower.CurveCant).ToString("0", culture)} mm, Pitch: {CameraTrackFollower.Pitch.ToString("0", culture)} ‰, Adhesion={(100.0 * CameraTrackFollower.AdhesionMultiplier).ToString("0", culture)}" + " , Rain intensity= " + CameraTrackFollower.RainIntensity +"%", new Vector2((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, ForceLegacyOpenGL ? "Renderer: Old (GL 1.2)- GL 4 not available" : $"Renderer: {(AvailableNewRenderer ? "New (GL 4)" : "Old (GL 1.2)")}", new Vector2((int)x, 40), TextAlignment.TopLeft, Color128.White, true);


					int stationIndex = Program.Renderer.CameraTrackFollower.StationIndex;

					if (stationIndex >= 0)
					{
						string s = Program.CurrentRoute.Stations[stationIndex].Name;

						if (Program.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
						{
							s+= $", Arrival: {GetTime(Program.CurrentRoute.Stations[stationIndex].ArrivalTime)}";
						}

						if (Program.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
						{
							s += $", Departure: {GetTime(Program.CurrentRoute.Stations[stationIndex].DepartureTime)}";
						}

						if (Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors & Program.CurrentRoute.Stations[stationIndex].OpenRightDoors)
						{
							s += ", [L][R]";
						}
						else if (Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors)
						{
							s += ", [L][-]";
						}
						else if (Program.CurrentRoute.Stations[stationIndex].OpenRightDoors)
						{
							s += ", [-][R]";
						}
						else
						{
							s += ", [-][-]";
						}

						switch (Program.CurrentRoute.Stations[stationIndex].StopMode)
						{
							case StationStopMode.AllStop:
								s += ", Stop";
								break;
							case StationStopMode.AllPass:
								s += ", Pass";
								break;
							case StationStopMode.PlayerStop:
								s += ", Player stops - others pass";
								break;
							case StationStopMode.PlayerPass:
								s += ", Player passes - others stop";
								break;
						}

						switch (Program.CurrentRoute.Stations[stationIndex].Type)
						{
							case StationType.ChangeEnds:
								s += ", Change ends";
								break;
							case StationType.Jump:
								s += ", then Jumps to " + Program.CurrentRoute.Stations[Program.CurrentRoute.Stations[stationIndex].JumpIndex].Name;
								break;
						}

						s += ", Ratio=" + (100.0 * Program.CurrentRoute.Stations[stationIndex].PassengerRatio).ToString("0", culture) + "%";

						OpenGlString.Draw(Fonts.SmallFont, s, new Vector2((int)x, 60), TextAlignment.TopLeft, Color128.White, true);
					}

					if (Interface.LogMessages.Count == 1)
					{
						keys = new[] { new[] { "F9" } };
						Keys.Render(4, 72, 24, Fonts.SmallFont, keys);

						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 error message recently generated.", new Vector2(32 * scaleFactor, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 message recently generated.", new Vector2(32 * scaleFactor, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					else if (Interface.LogMessages.Count > 1)
					{
						Keys.Render(4, 72, 24, Fonts.SmallFont, new[] { new[] { "F9" } });
						bool error = Interface.LogMessages.Any(m => m.Type != MessageType.Information);

						if (error)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count} error messages recently generated.", new Vector2(32 * scaleFactor, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count} messages recently generated.", new Vector2(32 * scaleFactor, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}

					if (RenderStatsOverlay)
					{
						Keys.Render(4, Screen.Height - 126, 116, Fonts.SmallFont, new[] { new[] { "Renderer Statistics" } });
						lock (VisibleObjects.LockObject)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Total static objects: {VisibleObjects.Objects.Count}", new Vector2(4, Screen.Height - 112), TextAlignment.TopLeft, Color128.White, true);
							OpenGlString.Draw(Fonts.SmallFont, $"Total animated objects: {ObjectManager.AnimatedWorldObjectsUsed}", new Vector2(4, Screen.Height - 100), TextAlignment.TopLeft, Color128.White, true);
							OpenGlString.Draw(Fonts.SmallFont, $"Current frame rate: {FrameRate.ToString("0.0", culture)}fps", new Vector2(4, Screen.Height - 88), TextAlignment.TopLeft, Color128.White, true);
							OpenGlString.Draw(Fonts.SmallFont, $"Total opaque faces: {VisibleObjects.OpaqueFaces.Count}", new Vector2(4, Screen.Height - 76), TextAlignment.TopLeft, Color128.White, true);
							OpenGlString.Draw(Fonts.SmallFont, $"Total alpha faces: {VisibleObjects.AlphaFaces.Count}", new Vector2(4, Screen.Height - 64), TextAlignment.TopLeft, Color128.White, true);
						}
						
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

		private static string GetTime(double Time)
		{
			int h = (int)Math.Floor(Time / 3600.0);
			Time -= h * 3600.0;
			int m = (int)Math.Floor(Time / 60.0);
			Time -= m * 60.0;
			int s = (int)Math.Floor(Time);
			return $"{h:00}:{m:00}:{s:00}";
		}

		// get length string 
		private static string GetLengthString(double Value)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			if (Program.CurrentRoute.UnitOfLength.Length == 1 && Program.CurrentRoute.UnitOfLength[0] == 1.0)
			{
				return Value.ToString("0.00", culture);
			}

			double[] values = new double[Program.CurrentRoute.UnitOfLength.Length];

			for (int i = 0; i < Program.CurrentRoute.UnitOfLength.Length - 1; i++)
			{
				values[i] = Math.Floor(Value / Program.CurrentRoute.UnitOfLength[i]);
				Value -= values[i] * Program.CurrentRoute.UnitOfLength[i];
			}

			values[Program.CurrentRoute.UnitOfLength.Length - 1] = Value / Program.CurrentRoute.UnitOfLength[Program.CurrentRoute.UnitOfLength.Length - 1];
			string s = string.Empty;

			for (int i = 0; i < values.Length - 1; i++)
			{
				s += values[i].ToString(culture) + ":";
			}

			s += values[values.Length - 1].ToString("0.00", culture);
			return s;
		}

		public NewRenderer(HostInterface currentHost, BaseOptions currentOptions, FileSystem fileSystem) : base(currentHost, currentOptions, fileSystem)
		{
			Screen.Width = Interface.CurrentOptions.WindowWidth;
			Screen.Height = Interface.CurrentOptions.WindowHeight;
		}
	}
}
