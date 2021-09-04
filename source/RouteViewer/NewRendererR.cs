﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Objects;
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
		internal void RenderScene(double TimeElapsed)
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
					Program.CurrentRoute.CurrentFog.Density = (byte)(Program.CurrentRoute.PreviousFog.Density * frc + Program.CurrentRoute.NextFog.Density * fr);
				}
				
			}
			else
			{
				Program.CurrentRoute.CurrentFog = Program.CurrentRoute.PreviousFog;
			}

			// render background
			GL.Disable(EnableCap.DepthTest);
			Program.CurrentRoute.UpdateBackground(TimeElapsed, false);

			if (OptionEvents)
			{
				RenderEvents();
			}

			// fog
			float aa = Program.CurrentRoute.CurrentFog.Start;
			float bb = Program.CurrentRoute.CurrentFog.End;

			if (aa < bb & aa < Program.CurrentRoute.CurrentBackground.BackgroundImageDistance)
			{
				OptionFog = true;
				Fog.Start = aa;
				Fog.End = bb;
				Fog.Color = Program.CurrentRoute.CurrentFog.Color;
				Fog.Density = Program.CurrentRoute.CurrentFog.Density;
				Fog.IsLinear = Program.CurrentRoute.CurrentFog.IsLinear;
				if (!AvailableNewRenderer)
				{
					Fog.SetForImmediateMode();
				}
			}
			else
			{
				OptionFog = false;
			}

			// world layer
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
				if (OptionFog)
				{
					DefaultShader.SetIsFog(true);
					DefaultShader.SetFog(Fog);
				}
				DefaultShader.SetTexture(0);
				DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			}
			ResetOpenGlState();

			foreach (FaceState face in VisibleObjects.OpaqueFaces)
			{
				face.Draw();
			}

			// alpha face
			ResetOpenGlState();
			VisibleObjects.SortPolygonsInAlphaFaces();

			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				SetBlendFunc();
				SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				GL.DepthMask(false);

				foreach (FaceState face in VisibleObjects.AlphaFaces)
				{
					face.Draw();
				}
			}
			else
			{
				UnsetBlendFunc();
				SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);

				foreach (FaceState face in VisibleObjects.AlphaFaces)
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

				foreach (FaceState face in VisibleObjects.AlphaFaces)
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

			// render overlays
			if (AvailableNewRenderer)
			{
				DefaultShader.Deactivate();
			}
			ResetOpenGlState();
			OptionLighting = false;
			OptionFog = false;
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //FIXME: Remove when text switches between two renderer types
			RenderOverlays();
			OptionLighting = true;
		}

		private void RenderEvents()
		{
			if (Program.CurrentRoute.Tracks[0].Elements == null)
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
			for (int i = 0; i < Program.CurrentRoute.Tracks[0].Elements.Length; i++)
			{
				double p = Program.CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
				double d = p - CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					foreach (GeneralEvent e in Program.CurrentRoute.Tracks[0].Elements[i].Events)
					{
						double dy, dx = 0.0, dz = 0.0;
						double s;
						Texture t;

						if (e is BrightnessChangeEvent)
						{
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						}
						else if (e is BackgroundChangeEvent)
						{
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						}
						else if (e is StationStartEvent)
						{
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							StationStartEvent f = (StationStartEvent)e;
							sta[f.StationIndex] = true;
						}
						else if (e is StationEndEvent)
						{
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							StationEndEvent f = (StationEndEvent)e;
							sta[f.StationIndex] = true;
						}
						else if (e is LimitChangeEvent)
						{
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						}
						else if (e is SectionChangeEvent)
						{
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						}
						else if (e is TransponderEvent)
						{
							s = 0.15;
							dy = 0.4;
							TransponderEvent ev = e as TransponderEvent;
							if (ev.Type == 21)
							{
								// beacon type 21 is reserved for legacy weather events
								t = WeatherEventTexture;
							}
							else
							{
								t = TransponderTexture;
							}

						}
						else if (e is SoundEvent)
						{
							SoundEvent f = (SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = f.SoundBuffer == null ? PointSoundTexture : SoundTexture;
						}
						else if (e is RailSoundsChangeEvent)
						{
							s = 0.2;
							dy = 0.8;
							t = RunSoundTexture;
						}
						else if (e is LightingChangeEvent)
						{
							s = 0.2;
							dy = 1.5;
							t = LightingEventTexture;
						}
						else
						{
							s = 0.2;
							dy = 1.0;
							t = null;
						}

						if (t != null)
						{
							TrackFollower f = new TrackFollower(Program.CurrentHost)
							{
								TriggerType = EventTriggerType.None,
								TrackPosition = p
							};
							f.UpdateAbsolute(p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;

							Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.AbsolutePosition, t);
						}
					}
				}
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
			foreach (double p in Program.CurrentRoute.BufferTrackPositions)
			{
				double d = p - CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					const double dy = 2.5;
					const double s = 0.25;
					TrackFollower f = new TrackFollower(Program.CurrentHost)
					{
						TriggerType = EventTriggerType.None,
						TrackPosition = p
					};
					f.UpdateAbsolute(p, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;

					Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.AbsolutePosition, BufferTexture);
				}
			}

			OptionLighting = true;
		}

		private void RenderOverlays()
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
				int y = 150;

				foreach (Texture t in Marker.MarkerTextures)
				{
					if (Program.CurrentHost.LoadTexture(t, OpenGlTextureWrapMode.ClampClamp))
					{
						int w = t.Width;
						int h = t.Height;
						GL.Color4(1.0, 1.0, 1.0, 1.0);
						Rectangle.Draw(t, new Vector2(Screen.Width - w - 8, y), new Vector2(w, h));
						y += h + 8;
					}
				}
			}

			if (!Program.CurrentlyLoading)
			{
				string[][] keys;

				if (VisibleObjects.Objects.Count == 0 && ObjectManager.AnimatedWorldObjectsUsed == 0)
				{
					keys = new[] { new[] { "F7" }, new[] { "F8" } };
					Keys.Render(4, 4, 20, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "Open route", new Vector2(32, 4), TextAlignment.TopLeft, Color128.White);
					OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32, 24), TextAlignment.TopLeft, Color128.White);
					OpenGlString.Draw(Fonts.SmallFont, $"v{Application.ProductVersion}", new Vector2(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, Color128.White);
				}
				else if (OptionInterface)
				{
					// keys
					keys = new[] { new[] { "F5" }, new[] { "F7" }, new[] { "F8" } };
					Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "Reload route", new Vector2(32, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Open route", new Vector2(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Vector2(32, 44), TextAlignment.TopLeft, Color128.White, true);

					keys = new[] { new[] { "F" }, new[] { "N" }, new[] { "E" }, new[] { "C" }, new[] { "M" }, new[] { "I" } };
					Keys.Render(Screen.Width - 20, 4, 16, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "WireFrame:", new Vector2(Screen.Width - 32, 4), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Normals:", new Vector2(Screen.Width - 32, 24), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Events:", new Vector2(Screen.Width - 32, 44), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "CPU:", new Vector2(Screen.Width - 32, 64), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Mute:", new Vector2(Screen.Width - 32, 84), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Hide interface:", new Vector2(Screen.Width - 32, 104), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"{(RenderStatsOverlay ? "Hide" : "Show")} renderer statistics", new Vector2(Screen.Width - 32, 124), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"Switch renderer type:", new Vector2(Screen.Width - 32, 144), TextAlignment.TopRight, Color128.White, true);

					keys = new[] { new[] { "F10" } };
					Keys.Render(Screen.Width - 32, 124, 30, Fonts.SmallFont, keys);

					keys = new[] { new[] { "R" } };
					Keys.Render(Screen.Width - 20, 144, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "W", null }, new[] { "A", "S", "D" } };
					Keys.Render(4, Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "↑", null }, new[] { "←", "↓", "→" } };
					Keys.Render(0 * Screen.Width - 48, Screen.Height - 40, 16, Fonts.SmallFont, keys);

					keys = new[] { new[] { "P↑" }, new[] { "P↓" } };
					Keys.Render((int)(0.5 * Screen.Width + 32), Screen.Height - 40, 24, Fonts.SmallFont, keys);

					keys = new[] { new[] { null, "/", "*" }, new[] { "7", "8", "9" }, new[] { "4", "5", "6" }, new[] { "1", "2", "3" }, new[] { null, "0", "." } };
					Keys.Render(Screen.Width - 60, Screen.Height - 100, 16, Fonts.SmallFont, keys);

					if (Program.JumpToPositionEnabled)
					{
						OpenGlString.Draw(Fonts.SmallFont, "Jump to track position:", new Vector2(4, 80), TextAlignment.TopLeft, Color128.White, true);

						double distance;

						if (double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), new Vector2(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), new Vector2(4, 100), TextAlignment.TopLeft, distance > Program.CurrentRoute.Tracks[0].Elements[Program.CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100 ? Color128.Red : Color128.Yellow, true);
							}

						}
					}

					// info
					double x = 0.5 * Screen.Width - 256.0;
					OpenGlString.Draw(Fonts.SmallFont, $"Position: {GetLengthString(Camera.Alignment.TrackPosition)} (X={GetLengthString(Camera.Alignment.Position.X)}, Y={GetLengthString(Camera.Alignment.Position.Y)}), Orientation: (Yaw={(Camera.Alignment.Yaw * 57.2957795130824).ToString("0.00", culture)}°, Pitch={(Camera.Alignment.Pitch * 57.2957795130824).ToString("0.00", culture)}°, Roll={(Camera.Alignment.Roll * 57.2957795130824).ToString("0.00", culture)}°)", new Vector2((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"Radius: {GetLengthString(CameraTrackFollower.CurveRadius)}, Cant: {(1000.0 * CameraTrackFollower.CurveCant).ToString("0", culture)} mm, Adhesion={(100.0 * CameraTrackFollower.AdhesionMultiplier).ToString("0", culture)}" + " , Rain intensity= " + CameraTrackFollower.RainIntensity +"%", new Vector2((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"Renderer: {(AvailableNewRenderer ? "New (GL 3.0)" : "Old (GL 1.2)")}", new Vector2((int)x, 40), TextAlignment.TopLeft, Color128.White, true);

					int stationIndex = Program.Renderer.CameraTrackFollower.StationIndex;

					if (stationIndex >= 0)
					{
						StringBuilder t = new StringBuilder();
						t.Append(Program.CurrentRoute.Stations[stationIndex].Name);

						if (Program.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
						{
							t.Append($", Arrival: {GetTime(Program.CurrentRoute.Stations[stationIndex].ArrivalTime)}");
						}

						if (Program.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
						{
							t.Append($", Departure: {GetTime(Program.CurrentRoute.Stations[stationIndex].DepartureTime)}");
						}

						if (Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors & Program.CurrentRoute.Stations[stationIndex].OpenRightDoors)
						{
							t.Append(", [L][R]");
						}
						else if (Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors)
						{
							t.Append(", [L][-]");
						}
						else if (Program.CurrentRoute.Stations[stationIndex].OpenRightDoors)
						{
							t.Append(", [-][R]");
						}
						else
						{
							t.Append(", [-][-]");
						}

						switch (Program.CurrentRoute.Stations[stationIndex].StopMode)
						{
							case StationStopMode.AllStop:
								t.Append(", Stop");
								break;
							case StationStopMode.AllPass:
								t.Append(", Pass");
								break;
							case StationStopMode.PlayerStop:
								t.Append(", Player stops - others pass");
								break;
							case StationStopMode.PlayerPass:
								t.Append(", Player passes - others stop");
								break;
						}

						switch (Program.CurrentRoute.Stations[stationIndex].Type)
						{
							case StationType.ChangeEnds:
								t.Append(", Change ends");
								break;
							case StationType.Jump:
								t.Append(", then Jumps to " + Program.CurrentRoute.Stations[Program.CurrentRoute.Stations[stationIndex].JumpIndex].Name);
								break;
						}

						t.Append(", Ratio=").Append((100.0 * Program.CurrentRoute.Stations[stationIndex].PassengerRatio).ToString("0", culture)).Append("%");

						OpenGlString.Draw(Fonts.SmallFont, t.ToString(), new Vector2((int)x, 60), TextAlignment.TopLeft, Color128.White, true);
					}

					if (Interface.LogMessages.Count == 1)
					{
						keys = new[] { new[] { "F9" } };
						Keys.Render(4, 72, 24, Fonts.SmallFont, keys);

						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 error message recently generated.", new Vector2(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 message recently generated.", new Vector2(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					else if (Interface.LogMessages.Count > 1)
					{
						Keys.Render(4, 72, 24, Fonts.SmallFont, new[] { new[] { "F9" } });
						bool error = Interface.LogMessages.Any(m => m.Type != MessageType.Information);

						if (error)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count} error messages recently generated.", new Vector2(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.LogMessages.Count} messages recently generated.", new Vector2(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}

					if (RenderStatsOverlay)
					{
						Keys.Render(4, Screen.Height - 126, 116, Fonts.SmallFont, new[] { new[] { "Renderer Statistics" } });
						OpenGlString.Draw(Fonts.SmallFont, $"Total static objects: {VisibleObjects.Objects.Count}", new Vector2(4, Screen.Height - 112), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total animated objects: {ObjectManager.AnimatedWorldObjectsUsed}", new Vector2(4, Screen.Height - 100), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Current frame rate: {FrameRate.ToString("0.0", culture)}fps", new Vector2(4, Screen.Height - 88), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total opaque faces: {VisibleObjects.OpaqueFaces.Count}", new Vector2(4, Screen.Height - 76), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total alpha faces: {VisibleObjects.AlphaFaces.Count}", new Vector2(4, Screen.Height - 64), TextAlignment.TopLeft, Color128.White, true);
					}
				}
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
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < values.Length - 1; i++)
			{
				builder.Append(values[i].ToString(culture) + ":");
			}

			builder.Append(values[values.Length - 1].ToString("0.00", culture));
			return builder.ToString();
		}

		public NewRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem) : base(CurrentHost, CurrentOptions, FileSystem)
		{
			Screen.Width = Interface.CurrentOptions.WindowWidth;
			Screen.Height = Interface.CurrentOptions.WindowHeight;
		}
	}
}
