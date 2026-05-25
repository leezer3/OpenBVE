using LibRender2;
using LibRender2.MotionBlurs;
using LibRender2.Objects;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Shaders;
using LibRender2.Viewports;
using LibRender2.Pipeline;
using LibRender2.Passes;
using OpenBve.Graphics.Renderers;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FileSystem;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainManager.Trains;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve.Graphics
{
	internal class NewRenderer : BaseRenderer
	{
		internal bool OptionClock = false;
		internal GradientDisplayMode OptionGradient = GradientDisplayMode.None;
		internal SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
		internal DistanceToNextStationDisplayMode OptionDistanceToNextStation = DistanceToNextStationDisplayMode.None;
		internal bool OptionFrameRates = false;
		internal bool OptionBrakeSystems = false;
		internal bool DebugTouchMode = false;

		internal Shader pickingShader;
		private Events events;
		private Overlays overlays;
		internal Touch Touch;

		public NewRenderer(HostInterface currentHost, BaseOptions currentOptions, FileSystem fileSystem) : base(currentHost, currentOptions, fileSystem)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			if (!ForceLegacyOpenGL && Interface.CurrentOptions.IsUseNewRenderer) // GL3 has already failed. Don't trigger unnecessary exceptions
			{
				try
				{
					if (pickingShader == null)
					{
						pickingShader = new Shader(this, "default", "picking", true);
					}

					pickingShader.Activate();
					pickingShader.Deactivate();
				}
				catch
				{
					Interface.AddMessage(MessageType.Error, false, "Initializing the touch shader failed- Falling back to legacy openGL.");
					Interface.CurrentOptions.IsUseNewRenderer = false;
					ForceLegacyOpenGL = true;
				}
			}

			events = new Events(this);
			overlays = new Overlays(this);
			Touch = new Touch(this);

			// Initialize Pipeline
			Pipeline.Clear();
			Pipeline.AddPass(new ShadowPass());
			Pipeline.AddPass(new SkyPass(UpdateBackground));
			Pipeline.AddPass(new GeometryPass());
			Pipeline.AddPass(new MotionBlurPass(this));
			Pipeline.AddPass(new OverlayPass(RenderUI));
			Pipeline.AddPass(new TouchPickingPass(this));

			Program.FileSystem.AppendToLogFile("Renderer initialised successfully.");
		}
		private void UpdateBackground(RenderContext context)
		{
			Program.CurrentRoute.UpdateBackground(context.TimeElapsed, Program.Renderer.CurrentInterface != InterfaceType.Normal);

			// fog setup
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
		}

		private void RenderUI(RenderContext context)
		{
			// particle sources
			if (CurrentInterface != InterfaceType.GLMainMenu)
			{
				foreach (TrainBase train in Program.TrainManager.Trains)
				{
					train.UpdateParticleSources(context.TimeElapsed);
				}

				foreach (ScriptedTrain scriptedTrain in Program.TrainManager.TFOs)
				{
					scriptedTrain.UpdateParticleSources(context.TimeElapsed);
				}
			}

			if (Interface.CurrentOptions.ShowEvents)
			{
				if (Math.Abs(CameraTrackFollower.TrackPosition - events.LastTrackPosition) > 50)
				{
					events.LastTrackPosition = CameraTrackFollower.TrackPosition;
					CubesToDraw.Clear();
					for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
					{
						int railIndex = Program.CurrentRoute.Tracks.ElementAt(i).Key;
						events.FindEvents(railIndex);
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

			overlays.Render(context.RealTimeElapsed);
			switch (CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.Draw(context.TimeElapsed);
					break;
				case InterfaceType.SwitchChangeMap:
					Game.SwitchChangeDialog.Draw();
					break;
			}
		}

		internal int CreateStaticObject(UnifiedObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			if (!(Prototype is StaticObject obj))
			{
				Interface.AddMessage(MessageType.Error, false, "Attempted to use an animated object where only static objects are allowed.");
				return -1;
			}
			return base.CreateStaticObject(obj, Position, WorldTransformation, LocalTransformation, AccurateObjectDisposal, Parameters, BlockLength);
		}

		protected override void UpdateViewport(int width, int height)
		{
			_programLogo = null;
			Screen.Width = width;
			Screen.Height = height;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);

			Screen.AspectRatio = Screen.Width / (double)Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);

			switch (CurrentViewportMode)
			{
				case ViewportMode.Scenery:
					double cd = Program.CurrentRoute.CurrentBackground is BackgroundObject b ? Math.Max(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance, b.ClipDistance) : Program.CurrentRoute.CurrentBackground.BackgroundImageDistance;
					cd = Math.Max(cd, currentOptions.ViewingDistance);
					double nearClipScenery = Math.Max(0.01, currentOptions.NearClipScenery);
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, nearClipScenery, cd);
					break;
				case ViewportMode.Cab:
					double nearClipCab = Math.Max(0.01, currentOptions.NearClipCab);
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, nearClipCab, 50.0);
					break;
			}

			Touch.UpdateViewport();
		}

		internal void RenderScene(double TimeElapsed, double RealTimeElapsed)
		{
			if (GameWindow != null && !Screen.Minimized)
			{
				Screen.Width = GameWindow.Width;
				Screen.Height = GameWindow.Height;
			}
			UpdateViewport(ViewportChangeMode.ChangeToScenery);
			UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
			ReleaseResources();
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

			// Setup Render Context
			RenderContext context = new RenderContext(this, TimeElapsed)
			{
				RealTimeElapsed = RealTimeElapsed,
				Camera = Camera,
				ProjectionMatrix = CurrentProjectionMatrix
			};

			// Update View Matrix
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			context.ViewMatrix = CurrentViewMatrix;

			if (Lighting.ShouldInitialize)
			{
				Lighting.Initialize();
				Lighting.ShouldInitialize = false;
			}

			Lighting.OptionLightingResultingAmount = (Lighting.OptionAmbientColor.R + Lighting.OptionAmbientColor.G + Lighting.OptionAmbientColor.B) / 480.0f;
			if (Lighting.OptionLightingResultingAmount > 1.0f) Lighting.OptionLightingResultingAmount = 1.0f;

			// Fog track interpolation
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

			TransformedLightPosition = new Vector3(Lighting.OptionLightPosition.X, Lighting.OptionLightPosition.Y, -Lighting.OptionLightPosition.Z);
			TransformedLightPosition.Transform(CurrentViewMatrix);

			// Execute Pipeline
			ExecutePipeline(context);
		}
	}
	internal class MotionBlurPass : IRenderPass
	{
		private readonly NewRenderer renderer;
		internal MotionBlurPass(NewRenderer renderer)
		{
			this.renderer = renderer;
		}
		public void Execute(RenderContext context)
		{
			renderer.ResetOpenGlState();
			renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			renderer.OptionLighting = false;

			if (Interface.CurrentOptions.MotionBlur != MotionBlurMode.None)
			{
				if (renderer.AvailableNewRenderer)
				{
					renderer.DefaultShader.Deactivate();
				}
				renderer.MotionBlur.RenderFullscreen(Interface.CurrentOptions.MotionBlur, renderer.FrameRate, Math.Abs(renderer.Camera.CurrentSpeed));
			}

			renderer.OptionLighting = true;
		}
	}

	internal class TouchPickingPass : IRenderPass
	{
		private readonly NewRenderer renderer;
		internal TouchPickingPass(NewRenderer renderer)
		{
			this.renderer = renderer;
		}
		public void Execute(RenderContext context)
		{
			renderer.OptionLighting = false;
			renderer.Touch.RenderScene();
			renderer.OptionLighting = true;
		}
	}
}
