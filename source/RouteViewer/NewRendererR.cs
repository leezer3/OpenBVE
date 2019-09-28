using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using LibRender2;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RouteManager2.Events;

namespace OpenBve
{
	internal class NewRenderer : BaseRenderer
	{
		// stats
		internal bool RenderStatsOverlay = true;

		// options
		internal bool OptionInterface = true;
		internal bool OptionEvents = false;

		// current opengl data
		internal bool TransparentColorDepthSorting = false;

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


		public override void Initialize(HostInterface CurrentHost, BaseOptions CurrentOptions)
		{
			base.Initialize(CurrentHost, CurrentOptions);

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

			TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality & Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
		}

		internal void CreateObject(UnifiedObject Prototype, OpenBveApi.Math.Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
		{
			if (Prototype != null)
			{
				CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
			}
		}

		internal void CreateObject(UnifiedObject Prototype, OpenBveApi.Math.Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			if (Prototype is StaticObject)
			{
				StaticObject s = (StaticObject)Prototype;
				CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
			}
			else if (Prototype is AnimatedObjectCollection)
			{
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				a.CreateObject(Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, true);
			}
		}

		internal int CreateStaticObject(StaticObject Prototype, OpenBveApi.Math.Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
		{
			return base.CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}

		internal int CreateStaticObject(UnifiedObject Prototype, OpenBveApi.Math.Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			StaticObject obj = Prototype as StaticObject;

			if (obj == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Attempted to use an animated object where only static objects are allowed.");
				return -1;
			}

			return base.CreateStaticObject(obj, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		public override void InitializeVisibility()
		{
			ObjectsSortedByStart = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.StartingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByEnd = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.EndingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;

			double p = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;

			foreach (ObjectState state in StaticObjectStates.Where(recipe => recipe.StartingDistance <= p + Camera.ForwardViewingDistance & recipe.EndingDistance >= p - Camera.BackwardViewingDistance))
			{
				VisibleObjects.ShowObject(state, ObjectType.Static);
			}
		}

		public override void UpdateVisibility(double TrackPosition)
		{
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;

			if (d < 0.0)
			{
				if (ObjectsSortedByStartPointer >= n)
				{
					ObjectsSortedByStartPointer = n - 1;
				}

				if (ObjectsSortedByEndPointer >= n)
				{
					ObjectsSortedByEndPointer = n - 1;
				}

				// dispose
				while (ObjectsSortedByStartPointer >= 0)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance > p + Camera.ForwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}

				// introduce
				while (ObjectsSortedByEndPointer >= 0)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance >= p - Camera.BackwardViewingDistance)
					{
						if (StaticObjectStates[o].StartingDistance <= p + Camera.ForwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (ObjectsSortedByStartPointer < 0)
				{
					ObjectsSortedByStartPointer = 0;
				}

				if (ObjectsSortedByEndPointer < 0)
				{
					ObjectsSortedByEndPointer = 0;
				}

				// dispose
				while (ObjectsSortedByEndPointer < n)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance < p - Camera.BackwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}

				// introduce
				while (ObjectsSortedByStartPointer < n)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance <= p + Camera.ForwardViewingDistance)
					{
						if (StaticObjectStates[o].EndingDistance >= p - Camera.BackwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}

			LastUpdatedTrackPosition = TrackPosition;
		}

		// render scene
		internal void RenderScene(double TimeElapsed)
		{
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

			// set up camera
			double dx = Camera.AbsoluteDirection.X;
			double dy = Camera.AbsoluteDirection.Y;
			double dz = Camera.AbsoluteDirection.Z;
			double ux = Camera.AbsoluteUp.X;
			double uy = Camera.AbsoluteUp.Y;
			double uz = Camera.AbsoluteUp.Z;
			CurrentViewMatrix = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, -dz, ux, uy, -uz);
			GL.Light(LightName.Light0, LightParameter.Position, new[] { (float)Lighting.OptionLightPosition.X, (float)Lighting.OptionLightPosition.Y, (float)-Lighting.OptionLightPosition.Z, 0.0f });

			// fog
			double fd = Program.CurrentRoute.NextFog.TrackPosition - Program.CurrentRoute.PreviousFog.TrackPosition;

			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - Program.CurrentRoute.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Program.CurrentRoute.CurrentFog.Start = Program.CurrentRoute.PreviousFog.Start * frc + Program.CurrentRoute.NextFog.Start * fr;
				Program.CurrentRoute.CurrentFog.End = Program.CurrentRoute.PreviousFog.End * frc + Program.CurrentRoute.NextFog.End * fr;
				Program.CurrentRoute.CurrentFog.Color.R = (byte)(Program.CurrentRoute.PreviousFog.Color.R * frc + Program.CurrentRoute.NextFog.Color.R * fr);
				Program.CurrentRoute.CurrentFog.Color.G = (byte)(Program.CurrentRoute.PreviousFog.Color.G * frc + Program.CurrentRoute.NextFog.Color.G * fr);
				Program.CurrentRoute.CurrentFog.Color.B = (byte)(Program.CurrentRoute.PreviousFog.Color.B * frc + Program.CurrentRoute.NextFog.Color.B * fr);
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

			if (aa < bb & aa < BackgroundHandle.BackgroundImageDistance)
			{
				OptionFog = true;
				Fog.Start = aa;
				Fog.End = bb;
				Fog.Color = Program.CurrentRoute.CurrentFog.Color;
				SetFogForImmediateMode();
			}
			else
			{
				OptionFog = false;
			}

			// world layer
			// opaque face
			ResetOpenGlState();

			foreach (FaceState face in VisibleObjects.OpaqueFaces)
			{
				if (Interface.CurrentOptions.IsUseNewRenderer)
				{
					DefaultShader.Use();
					ResetShader(DefaultShader);
					RenderFace(DefaultShader, face);
					DefaultShader.NonUse();
				}
				else
				{
					RenderFaceImmediateMode(face);
				}
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
					if (Interface.CurrentOptions.IsUseNewRenderer)
					{
						DefaultShader.Use();
						ResetShader(DefaultShader);
						RenderFace(DefaultShader, face);
						DefaultShader.NonUse();
					}
					else
					{
						RenderFaceImmediateMode(face);
					}
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
							if (Interface.CurrentOptions.IsUseNewRenderer)
							{
								DefaultShader.Use();
								ResetShader(DefaultShader);
								RenderFace(DefaultShader, face);
								DefaultShader.NonUse();
							}
							else
							{
								RenderFaceImmediateMode(face);
							}
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

						if (Interface.CurrentOptions.IsUseNewRenderer)
						{
							DefaultShader.Use();
							ResetShader(DefaultShader);
							RenderFace(DefaultShader, face);
							DefaultShader.NonUse();
						}
						else
						{
							RenderFaceImmediateMode(face);
						}
					}
					else
					{
						if (additive)
						{
							SetAlphaFunc();
							additive = false;
						}

						if (Interface.CurrentOptions.IsUseNewRenderer)
						{
							DefaultShader.Use();
							ResetShader(DefaultShader);
							RenderFace(DefaultShader, face);
							DefaultShader.NonUse();
						}
						else
						{
							RenderFaceImmediateMode(face);
						}
					}
				}
			}

			// render overlays
			ResetOpenGlState();
			OptionLighting = false;
			OptionFog = false;
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
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
				double d = p - World.CameraTrackFollower.TrackPosition;

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
						else if (e is TrackManager.StationEndEvent)
						{
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
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
							t = TransponderTexture;
						}
						else if (e is TrackManager.SoundEvent)
						{
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = f.SoundBuffer == null ? PointSoundTexture : SoundTexture;
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
			foreach (double p in Game.BufferTrackPositions)
			{
				double d = p - World.CameraTrackFollower.TrackPosition;

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
			CurrentProjectionMatrix = Matrix4d.CreateOrthographicOffCenter(0.0, Screen.Width, Screen.Height, 0.0, -1.0, 1.0);
			PushMatrix(MatrixMode.Modelview);
			CurrentViewMatrix = Matrix4d.Identity;

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
						Rectangle.Draw(t, new Point(Screen.Width - w - 8, y), new Size(w, h));
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
					OpenGlString.Draw(Fonts.SmallFont, "Open route", new Point(32, 4), TextAlignment.TopLeft, Color128.White);
					OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Point(32, 24), TextAlignment.TopLeft, Color128.White);
					OpenGlString.Draw(Fonts.SmallFont, $"v{System.Windows.Forms.Application.ProductVersion}", new Point(Screen.Width - 8, Screen.Height - 20), TextAlignment.TopLeft, Color128.White);
				}
				else if (OptionInterface)
				{
					// keys
					keys = new[] { new[] { "F5" }, new[] { "F7" }, new[] { "F8" } };
					Keys.Render(4, 4, 24, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "Reload route", new Point(32, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Open route", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Display the options window", new Point(32, 44), TextAlignment.TopLeft, Color128.White, true);

					keys = new[] { new[] { "F" }, new[] { "N" }, new[] { "E" }, new[] { "C" }, new[] { "M" }, new[] { "I" } };
					Keys.Render(Screen.Width - 20, 4, 16, Fonts.SmallFont, keys);
					OpenGlString.Draw(Fonts.SmallFont, "WireFrame:", new Point(Screen.Width - 32, 4), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Normals:", new Point(Screen.Width - 32, 24), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Events:", new Point(Screen.Width - 32, 44), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "CPU:", new Point(Screen.Width - 32, 64), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Mute:", new Point(Screen.Width - 32, 84), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, "Hide interface:", new Point(Screen.Width - 32, 104), TextAlignment.TopRight, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"{(RenderStatsOverlay ? "Hide" : "Show")} renderer statistics", new Point(Screen.Width - 32, 124), TextAlignment.TopRight, Color128.White, true);

					keys = new[] { new[] { "F10" } };
					Keys.Render(Screen.Width - 32, 124, 30, Fonts.SmallFont, keys);

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
						OpenGlString.Draw(Fonts.SmallFont, "Jump to track position:", new Point(4, 80), TextAlignment.TopLeft, Color128.White, true);

						double distance;

						if (double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								OpenGlString.Draw(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? $"{Program.JumpToPositionValue}_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, distance > Program.CurrentRoute.Tracks[0].Elements[Program.CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100 ? Color128.Red : Color128.Yellow, true);
							}

						}
					}

					// info
					double x = 0.5 * Screen.Width - 256.0;
					OpenGlString.Draw(Fonts.SmallFont, $"Position: {GetLengthString(Camera.Alignment.TrackPosition)} (X={GetLengthString(Camera.Alignment.Position.X)}, Y={GetLengthString(Camera.Alignment.Position.Y)}), Orientation: (Yaw={(Camera.Alignment.Yaw * 57.2957795130824).ToString("0.00", culture)}°, Pitch={(Camera.Alignment.Pitch * 57.2957795130824).ToString("0.00", culture)}°, Roll={(Camera.Alignment.Roll * 57.2957795130824).ToString("0.00", culture)}°)", new Point((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenGlString.Draw(Fonts.SmallFont, $"Radius: {GetLengthString(World.CameraTrackFollower.CurveRadius)}, Cant: {(1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", culture)} mm, Adhesion={(100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", culture)}", new Point((int)x, 20), TextAlignment.TopLeft, Color128.White, true);

					if (Program.CurrentStation >= 0)
					{
						StringBuilder t = new StringBuilder();
						t.Append(Program.CurrentRoute.Stations[Program.CurrentStation].Name);

						if (Program.CurrentRoute.Stations[Program.CurrentStation].ArrivalTime >= 0.0)
						{
							t.Append($", Arrival: {GetTime(Program.CurrentRoute.Stations[Program.CurrentStation].ArrivalTime)}");
						}

						if (Program.CurrentRoute.Stations[Program.CurrentStation].DepartureTime >= 0.0)
						{
							t.Append($", Departure: {GetTime(Program.CurrentRoute.Stations[Program.CurrentStation].DepartureTime)}");
						}

						if (Program.CurrentRoute.Stations[Program.CurrentStation].OpenLeftDoors & Program.CurrentRoute.Stations[Program.CurrentStation].OpenRightDoors)
						{
							t.Append(", [L][R]");
						}
						else if (Program.CurrentRoute.Stations[Program.CurrentStation].OpenLeftDoors)
						{
							t.Append(", [L][-]");
						}
						else if (Program.CurrentRoute.Stations[Program.CurrentStation].OpenRightDoors)
						{
							t.Append(", [-][R]");
						}
						else
						{
							t.Append(", [-][-]");
						}

						switch (Program.CurrentRoute.Stations[Program.CurrentStation].StopMode)
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

						if (Program.CurrentRoute.Stations[Program.CurrentStation].Type == StationType.ChangeEnds)
						{
							t.Append(", Change ends");
						}

						t.Append(", Ratio=").Append((100.0 * Program.CurrentRoute.Stations[Program.CurrentStation].PassengerRatio).ToString("0", culture)).Append("%");

						OpenGlString.Draw(Fonts.SmallFont, t.ToString(), new Point((int)x, 36), TextAlignment.TopLeft, Color128.White, true);
					}

					if (Interface.MessageCount == 1)
					{
						keys = new[] { new[] { "F9" } };
						Keys.Render(4, 72, 24, Fonts.SmallFont, keys);

						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenGlString.Draw(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					else if (Interface.MessageCount > 1)
					{
						Keys.Render(4, 72, 24, Fonts.SmallFont, new[] { new[] { "F9" } });
						bool error = Interface.LogMessages.Any(m => m.Type != MessageType.Information);

						if (error)
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.MessageCount} error messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							OpenGlString.Draw(Fonts.SmallFont, $"Display the {Interface.MessageCount} messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}

					if (RenderStatsOverlay)
					{
						Keys.Render(4, Screen.Height - 126, 116, Fonts.SmallFont, new[] { new[] { "Renderer Statistics" } });
						OpenGlString.Draw(Fonts.SmallFont, $"Total static objects: {VisibleObjects.Objects.Count}", new Point(4, Screen.Height - 112), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total animated objects: {ObjectManager.AnimatedWorldObjectsUsed}", new Point(4, Screen.Height - 100), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Current frame rate: {FrameRate.ToString("0.0", culture)}fps", new Point(4, Screen.Height - 88), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total opaque faces: {VisibleObjects.OpaqueFaces.Count}", new Point(4, Screen.Height - 76), TextAlignment.TopLeft, Color128.White, true);
						OpenGlString.Draw(Fonts.SmallFont, $"Total alpha faces: {VisibleObjects.AlphaFaces.Count}", new Point(4, Screen.Height - 64), TextAlignment.TopLeft, Color128.White, true);
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

			if (Game.RouteUnitOfLength.Length == 1 && Game.RouteUnitOfLength[0] == 1.0)
			{
				return Value.ToString("0.00", culture);
			}

			double[] values = new double[Game.RouteUnitOfLength.Length];

			for (int i = 0; i < Game.RouteUnitOfLength.Length - 1; i++)
			{
				values[i] = Math.Floor(Value / Game.RouteUnitOfLength[i]);
				Value -= values[i] * Game.RouteUnitOfLength[i];
			}

			values[Game.RouteUnitOfLength.Length - 1] = Value / Game.RouteUnitOfLength[Game.RouteUnitOfLength.Length - 1];
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < values.Length - 1; i++)
			{
				builder.Append(values[i].ToString(culture) + ":");
			}

			builder.Append(values[values.Length - 1].ToString("0.00", culture));
			return builder.ToString();
		}
	}
}
