using LibRender2;
using LibRender2.MotionBlurs;
using LibRender2.Objects;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Shaders;
using LibRender2.Viewports;
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
using TrainManager;
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

		public override void Initialize()
		{
			base.Initialize();
			if (!ForceLegacyOpenGL && Interface.CurrentOptions.IsUseNewRenderer) // GL3 has already failed. Don't trigger unneccessary exceptions
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
			ObjectsSortedByStart = new int[] { };
			ObjectsSortedByEnd = new int[] { };
			
			
			Program.FileSystem.AppendToLogFile("Renderer initialised successfully.");
		}
		
		internal int CreateStaticObject(UnifiedObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
		{
			if (!(Prototype is StaticObject obj))
			{
				Interface.AddMessage(MessageType.Error, false, "Attempted to use an animated object where only static objects are allowed.");
				return -1;
			}
			return base.CreateStaticObject(obj, Position, WorldTransformation, LocalTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition);
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
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.5, cd);
					break;
				case ViewportMode.Cab:
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.025, 50.0);
					break;
			}

			Touch.UpdateViewport();
		}
		
		// render scene
		internal void RenderScene(double TimeElapsed, double RealTimeElapsed)
		{
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

			UpdateViewport(ViewportChangeMode.ChangeToScenery);

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
			Program.CurrentRoute.UpdateBackground(TimeElapsed, Program.Renderer.CurrentInterface != InterfaceType.Normal);
			
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
			List<FaceState> opaqueFaces, alphaFaces, overlayOpaqueFaces, overlayAlphaFaces;
			lock (VisibleObjects.LockObject)
			{
				opaqueFaces = VisibleObjects.OpaqueFaces.ToList();
				alphaFaces = VisibleObjects.GetSortedPolygons();
				overlayOpaqueFaces = VisibleObjects.OverlayOpaqueFaces.ToList();
				overlayAlphaFaces = VisibleObjects.GetSortedPolygons(true);
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
					}
					else
					{
						if (additive)
						{
							SetAlphaFunc();
							additive = false;
						}
					}
					face.Draw();
				}
			}

			// motion blur
			ResetOpenGlState();
			SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			OptionLighting = false;

			if (Interface.CurrentOptions.MotionBlur != MotionBlurMode.None)
			{
				if (AvailableNewRenderer)
				{
					DefaultShader.Deactivate();
				}
				MotionBlur.RenderFullscreen(Interface.CurrentOptions.MotionBlur, FrameRate, Math.Abs(Camera.CurrentSpeed));
			}

			// particle sources
			SetBlendFunc();
			SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if(CurrentInterface != InterfaceType.GLMainMenu)
			{
				foreach (TrainBase train in Program.TrainManager.Trains)
				{
					train.UpdateParticleSources(TimeElapsed);
				}

				// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
				foreach (ScriptedTrain scriptedTrain in Program.TrainManager.TFOs)
				{
					scriptedTrain.UpdateParticleSources(TimeElapsed);
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

			// overlay (cab / interior) layer
			Fog.Enabled = false;
			UpdateViewport(ViewportChangeMode.ChangeToCab);
			
			if (AvailableNewRenderer)
			{
				/*
				 * We must reset the shader between overlay and world layers for correct lighting results.
				 * Additionally, the viewport change updates the projection matrix
				 */
				DefaultShader.Activate();
				ResetShader(DefaultShader);
				DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			}
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			
			if (Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				ResetOpenGlState(); // TODO: inserted
				GL.Clear(ClearBufferMask.DepthBufferBit);
				OptionLighting = true;
				Color24 prevOptionAmbientColor = Lighting.OptionAmbientColor;
				Color24 prevOptionDiffuseColor = Lighting.OptionDiffuseColor;
				Lighting.OptionAmbientColor = Color24.LightGrey;
				Lighting.OptionDiffuseColor = Color24.LightGrey;
				if (AvailableNewRenderer)
				{
					DefaultShader.SetIsLight(true);
					TransformedLightPosition = new Vector3(Lighting.OptionLightPosition.X, Lighting.OptionLightPosition.Y, -Lighting.OptionLightPosition.Z);
					DefaultShader.SetLightPosition(TransformedLightPosition);
					DefaultShader.SetLightAmbient(Lighting.OptionAmbientColor);
					DefaultShader.SetLightDiffuse(Lighting.OptionDiffuseColor);
					DefaultShader.SetLightSpecular(Lighting.OptionSpecularColor);
					DefaultShader.SetLightModel(Lighting.LightModel);
				}
				else
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new[] { inv255 * 178, inv255 * 178, inv255 * 178, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { inv255 * 178, inv255 * 178, inv255 * 178, 1.0f });	
				}
				

				// overlay opaque face
				foreach (FaceState face in overlayOpaqueFaces)
				{
					face.Draw();
				}

				// overlay alpha face
				ResetOpenGlState();
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					SetBlendFunc();
					SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					GL.DepthMask(false);

					foreach (FaceState face in overlayAlphaFaces)
					{
						face.Draw();
					}
				}
				else
				{
					UnsetBlendFunc();
					SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);

					foreach (FaceState face in overlayAlphaFaces)
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

					foreach (FaceState face in overlayAlphaFaces)
					{
						if (face.Object.Prototype.Mesh.Materials[face.Face.Material].BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								UnsetAlphaFunc();
								additive = true;
							}
						}
						else
						{
							if (additive)
							{
								SetAlphaFunc();
								additive = false;
							}
						}
						face.Draw();
					}
				}

				Lighting.OptionAmbientColor = prevOptionAmbientColor;
				Lighting.OptionDiffuseColor = prevOptionDiffuseColor;
				Lighting.Initialize();
			}
			else
			{
				/*
                 * Render 2D Cab
                 * This is actually an animated object generated on the fly and held in memory
                 */
				ResetOpenGlState();
				OptionLighting = false;
				if (AvailableNewRenderer)
				{
					DefaultShader.SetIsLight(false);
				}
				
				SetBlendFunc();
				UnsetAlphaFunc();
				GL.Disable(EnableCap.DepthTest);
				GL.DepthMask(false);
				foreach (FaceState face in overlayAlphaFaces)
				{
					face.Draw();
				}
			}
			// render touch
			OptionLighting = false;
			Touch.RenderScene();
			
			// render overlays
			ResetOpenGlState();
			UnsetAlphaFunc();
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //FIXME: Remove when text switches between two renderer types
			GL.Disable(EnableCap.DepthTest);
			overlays.Render(RealTimeElapsed);
			switch (CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.Draw(TimeElapsed);
					break;
				case InterfaceType.SwitchChangeMap:
					Game.SwitchChangeDialog.Draw();
					break;
			}
			OptionLighting = true;
		}

		public NewRenderer(HostInterface currentHost, BaseOptions currentOptions, FileSystem fileSystem) : base(currentHost, currentOptions, fileSystem)
		{
		}
	}
}
