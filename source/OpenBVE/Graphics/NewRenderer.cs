using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.MotionBlurs;
using LibRender2.Objects;
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
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;
using TrainManager.Trains;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve.Graphics
{
	internal class NewRenderer : BaseRenderer
	{
		// interface options
		internal enum GradientDisplayMode
		{
			Percentage, UnitOfChange, Permil, None
		}

		internal enum SpeedDisplayMode
		{
			None, Kmph, Mph
		}

		internal enum DistanceToNextStationDisplayMode
		{
			None, Km, Mile
		}

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
		
		internal int CreateStaticObject(UnifiedObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			StaticObject obj = Prototype as StaticObject;
			if (obj == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Attempted to use an animated object where only static objects are allowed.");
				return -1;
			}
			return base.CreateStaticObject(obj, Position, WorldTransformation, LocalTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		public override void UpdateViewport(int Width, int Height)
		{
			_programLogo = null;
			Screen.Width = Width;
			Screen.Height = Height;
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

			// render background
			GL.Disable(EnableCap.DepthTest);
			Program.CurrentRoute.UpdateBackground(TimeElapsed, CurrentInterface != InterfaceType.Normal);

			events.Render(Camera.AbsolutePosition);

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
			// overlay layer
			OptionFog = false;
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
			OptionLighting = true;
		}

		public override void UpdateAbsoluteCamera(double backgroundImageDistance, double TimeElapsed = 0.0)
		{
			// zoom
			double zm = Camera.Alignment.Zoom;
			Camera.AdjustAlignment(ref Camera.Alignment.Zoom, Camera.AlignmentDirection.Zoom, ref Camera.AlignmentSpeed.Zoom, TimeElapsed, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
			if (zm != Camera.Alignment.Zoom)
			{
				Camera.ApplyZoom();
			}
			if (Camera.CurrentMode == CameraViewMode.FlyBy | Camera.CurrentMode == CameraViewMode.FlyByZooming)
			{
				// fly-by
				Camera.AdjustAlignment(ref Camera.Alignment.Position.X, Camera.AlignmentDirection.Position.X, ref Camera.AlignmentSpeed.Position.X, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				Camera.AdjustAlignment(ref Camera.Alignment.Position.Y, Camera.AlignmentDirection.Position.Y, ref Camera.AlignmentSpeed.Position.Y, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				double tr = Camera.Alignment.TrackPosition;
				Camera.AdjustAlignment(ref Camera.Alignment.TrackPosition, Camera.AlignmentDirection.TrackPosition, ref Camera.AlignmentSpeed.TrackPosition, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				if (tr != Camera.Alignment.TrackPosition)
				{
					CameraTrackFollower.UpdateAbsolute(Camera.Alignment.TrackPosition, true, false);
					UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
				}
				// position to focus on
				Vector3 focusPosition = Vector3.Zero;
				double zoomMultiplier;
				{
					const double heightFactor = 0.75;
					TrainBase bestTrain = null;
					double bestDistanceSquared = double.MaxValue;
					TrainBase secondBestTrain = null;
					double secondBestDistanceSquared = double.MaxValue;
					foreach (TrainBase train in Program.TrainManager.Trains)
					{
						if (train.State == TrainState.Available)
						{
							double x = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.X + train.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Y + train.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * train.Cars[0].Height;
							double z = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Z + train.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double dx = x - CameraTrackFollower.WorldPosition.X;
							double dy = y - CameraTrackFollower.WorldPosition.Y;
							double dz = z - CameraTrackFollower.WorldPosition.Z;
							double d = dx * dx + dy * dy + dz * dz;
							if (d < bestDistanceSquared)
							{
								secondBestTrain = bestTrain;
								secondBestDistanceSquared = bestDistanceSquared;
								bestTrain = train;
								bestDistanceSquared = d;
							}
							else if (d < secondBestDistanceSquared)
							{
								secondBestTrain = train;
								secondBestDistanceSquared = d;
							}
						}
					}
					if (bestTrain != null)
					{
						const double maxDistance = 100.0;
						double bestDistance = Math.Sqrt(bestDistanceSquared);
						double secondBestDistance = Math.Sqrt(secondBestDistanceSquared);
						if (secondBestTrain != null && secondBestDistance - bestDistance <= maxDistance)
						{
							double x1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							double z1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double x2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * secondBestTrain.Cars[0].Height;
							double z2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double t = 0.5 - (secondBestDistance - bestDistance) / (2.0 * maxDistance);
							if (t < 0.0) t = 0.0;
							t = 2.0 * t * t; /* in order to change the shape of the interpolation curve */
							focusPosition.X = (1.0 - t) * x1 + t * x2;
							focusPosition.Y = (1.0 - t) * y1 + t * y2;
							focusPosition.Z = (1.0 - t) * z1 + t * z2;
							zoomMultiplier = 1.0 - 2.0 * t;
						}
						else
						{
							focusPosition.X = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							focusPosition.Y = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							focusPosition.Z = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							zoomMultiplier = 1.0;
						}
					}
					else
					{
						zoomMultiplier = 1.0;
					}
				}
				// camera
				{
					Camera.AbsoluteDirection = new Vector3(CameraTrackFollower.WorldDirection);
					Camera.AbsolutePosition = CameraTrackFollower.WorldPosition + CameraTrackFollower.WorldSide * Camera.Alignment.Position.X + CameraTrackFollower.WorldUp * Camera.Alignment.Position.Y + Camera.AbsoluteDirection * Camera.Alignment.Position.Z;
					Camera.AbsoluteDirection = focusPosition - Camera.AbsolutePosition;
					double t = Camera.AbsoluteDirection.Norm();
					double ti = 1.0 / t;
					Camera.AbsoluteDirection *= ti;

					Camera.AbsoluteSide = new Vector3(Camera.AbsoluteDirection.Z, 0.0, -Camera.AbsoluteDirection.X);
					Camera.AbsoluteSide.Normalize();
					Camera.AbsoluteUp = Vector3.Cross(Camera.AbsoluteDirection, Camera.AbsoluteSide);
					UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					if (Camera.CurrentMode == CameraViewMode.FlyByZooming)
					{
						// zoom
						const double fadeOutDistance = 600.0; /* the distance with the highest zoom factor is half the fade-out distance */
						const double maxZoomFactor = 7.0; /* the zoom factor at half the fade-out distance */
						const double factor = 256.0 / (fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance);
						double zoom;
						if (t < fadeOutDistance)
						{
							double tdist4 = fadeOutDistance - t; tdist4 *= tdist4; tdist4 *= tdist4;
							double t4 = t * t; t4 *= t4;
							zoom = 1.0 + factor * zoomMultiplier * (maxZoomFactor - 1.0) * tdist4 * t4;
						}
						else
						{
							zoom = 1.0;
						}
						Camera.VerticalViewingAngle = Camera.OriginalVerticalViewingAngle / zoom;
						UpdateViewport(ViewportChangeMode.NoChange);
					}
				}
			}
			else
			{
				// non-fly-by
				{
					// current alignment
					Camera.AdjustAlignment(ref Camera.Alignment.Position.X, Camera.AlignmentDirection.Position.X, ref Camera.AlignmentSpeed.Position.X, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Camera.AdjustAlignment(ref Camera.Alignment.Position.Y, Camera.AlignmentDirection.Position.Y, ref Camera.AlignmentSpeed.Position.Y, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Camera.AdjustAlignment(ref Camera.Alignment.Position.Z, Camera.AlignmentDirection.Position.Z, ref Camera.AlignmentSpeed.Position.Z, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					if ((Camera.CurrentMode == CameraViewMode.Interior | Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & Camera.CurrentRestriction == CameraRestrictionMode.On)
					{
						if (Camera.Alignment.Position.Z > 0.75)
						{
							Camera.Alignment.Position.Z = 0.75;
						}
					}
					bool q = Camera.AlignmentSpeed.Yaw != 0.0 | Camera.AlignmentSpeed.Pitch != 0.0 | Camera.AlignmentSpeed.Roll != 0.0;
					Camera.AdjustAlignment(ref Camera.Alignment.Yaw, Camera.AlignmentDirection.Yaw, ref Camera.AlignmentSpeed.Yaw, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Camera.AdjustAlignment(ref Camera.Alignment.Pitch, Camera.AlignmentDirection.Pitch, ref Camera.AlignmentSpeed.Pitch, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Camera.AdjustAlignment(ref Camera.Alignment.Roll, Camera.AlignmentDirection.Roll, ref Camera.AlignmentSpeed.Roll, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					double tr = Camera.Alignment.TrackPosition;
					Camera.AdjustAlignment(ref Camera.Alignment.TrackPosition, Camera.AlignmentDirection.TrackPosition, ref Camera.AlignmentSpeed.TrackPosition, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					if (tr != Camera.Alignment.TrackPosition)
					{
						CameraTrackFollower.UpdateAbsolute(Camera.Alignment.TrackPosition, true, false);
						q = true;
					}
					if (q)
					{
						UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					}
				}
				// camera
				Vector3 cF = new Vector3(CameraTrackFollower.WorldPosition);
				Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
				Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
				Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
				double lookaheadYaw;
				double lookaheadPitch;
				if (Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
				{
					// look-ahead
					double d = 20.0;
					if (TrainManager.PlayerTrain.CurrentSpeed > 0.0)
					{
						d += 3.0 * (Math.Sqrt(TrainManager.PlayerTrain.CurrentSpeed * TrainManager.PlayerTrain.CurrentSpeed + 1.0) - 1.0);
					}
					d -= TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Position;
					TrackFollower f = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.Clone();
					f.TriggerType = EventTriggerType.None;
					f.UpdateRelative(d, true, false);
					Vector3 r = new Vector3(f.WorldPosition - cF + CameraTrackFollower.WorldSide * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + CameraTrackFollower.WorldUp * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + CameraTrackFollower.WorldDirection * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z);
					r.Normalize();
					double t = dF.Z * (sF.Y * uF.X - sF.X * uF.Y) + dF.Y * (-sF.Z * uF.X + sF.X * uF.Z) + dF.X * (sF.Z * uF.Y - sF.Y * uF.Z);
					if (t != 0.0)
					{
						t = 1.0 / t;

						double tx = (r.Z * (-dF.Y * uF.X + dF.X * uF.Y) + r.Y * (dF.Z * uF.X - dF.X * uF.Z) + r.X * (-dF.Z * uF.Y + dF.Y * uF.Z)) * t;
						double ty = (r.Z * (dF.Y * sF.X - dF.X * sF.Y) + r.Y * (-dF.Z * sF.X + dF.X * sF.Z) + r.X * (dF.Z * sF.Y - dF.Y * sF.Z)) * t;
						double tz = (r.Z * (sF.Y * uF.X - sF.X * uF.Y) + r.Y * (-sF.Z * uF.X + sF.X * uF.Z) + r.X * (sF.Z * uF.Y - sF.Y * uF.Z)) * t;
						lookaheadYaw = tx * tz != 0.0 ? Math.Atan2(tx, tz) : 0.0;
						if (ty < -1.0)
						{
							lookaheadPitch = -0.5 * Math.PI;
						}
						else if (ty > 1.0)
						{
							lookaheadPitch = 0.5 * Math.PI;
						}
						else
						{
							lookaheadPitch = Math.Asin(ty);
						}
					}
					else
					{
						lookaheadYaw = 0.0;
						lookaheadPitch = 0.0;
					}
				}
				else
				{
					lookaheadYaw = 0.0;
					lookaheadPitch = 0.0;
				}
				{
					// cab pitch and yaw
					Vector3 d2 = new Vector3(dF);
					Vector3 u2 = new Vector3(uF);
					if ((Camera.CurrentMode == CameraViewMode.Interior | Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null)
					{
						int c = TrainManager.PlayerTrain.DriverCar;
						if (c >= 0)
						{
							if (TrainManager.PlayerTrain.Cars[c].CarSections.Length == 0 || TrainManager.PlayerTrain.Cars[c].CarSections[0].Type != ObjectType.Overlay)
							{
								d2.Rotate(sF, -TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch);
								u2.Rotate(sF, -TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch);
							}
						}
					}

					cF += sF * Camera.Alignment.Position.X + u2 * Camera.Alignment.Position.Y + d2 * Camera.Alignment.Position.Z;

				}
				// yaw, pitch, roll
				double headYaw = Camera.Alignment.Yaw + lookaheadYaw;
				if ((Camera.CurrentMode == CameraViewMode.Interior | Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null)
				{
					if (TrainManager.PlayerTrain.DriverCar >= 0)
					{
						headYaw += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverYaw;
					}
				}
				double headPitch = Camera.Alignment.Pitch + lookaheadPitch;
				if ((Camera.CurrentMode == CameraViewMode.Interior | Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null)
				{
					if (TrainManager.PlayerTrain.DriverCar >= 0)
					{
						headPitch += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
					}
				}
				double bodyPitch = 0.0;
				double bodyRoll = 0.0;
				double headRoll = Camera.Alignment.Roll;
				// rotation
				if ((Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D) & (Camera.CurrentMode == CameraViewMode.Interior | Camera.CurrentMode == CameraViewMode.InteriorLookAhead))
				{
					// with body and head
					bodyPitch += TrainManager.PlayerTrain.DriverBody.Pitch;
					headPitch -= 0.2 * TrainManager.PlayerTrain.DriverBody.Pitch;
					bodyRoll += TrainManager.PlayerTrain.DriverBody.Roll;
					headRoll += 0.2 * TrainManager.PlayerTrain.DriverBody.Roll;
					const double bodyHeight = 0.6;
					const double headHeight = 0.1;
					{
						// body pitch
						double ry = (Math.Cos(-bodyPitch) - 1.0) * bodyHeight;
						double rz = Math.Sin(-bodyPitch) * bodyHeight;
						cF += dF * rz + uF * ry;
						if (bodyPitch != 0.0)
						{
							dF.Rotate(sF, -bodyPitch);
							uF.Rotate(sF, -bodyPitch);
						}
					}
					{
						// body roll
						double rx = Math.Sin(bodyRoll) * bodyHeight;
						double ry = (Math.Cos(bodyRoll) - 1.0) * bodyHeight;
						cF += sF * rx + uF * ry;
						if (bodyRoll != 0.0)
						{
							uF.Rotate(dF, -bodyRoll);
							sF.Rotate(dF, -bodyRoll);
						}
					}
					{
						// head yaw
						double rx = Math.Sin(headYaw) * headHeight;
						double rz = (Math.Cos(headYaw) - 1.0) * headHeight;
						cF += sF * rx + dF * rz;
						if (headYaw != 0.0)
						{
							dF.Rotate(uF, headYaw);
							sF.Rotate(uF, headYaw);
						}
					}
					{
						// head pitch
						double ry = (Math.Cos(-headPitch) - 1.0) * headHeight;
						double rz = Math.Sin(-headPitch) * headHeight;
						cF += dF * rz + uF * ry;
						if (headPitch != 0.0)
						{
							dF.Rotate(sF, -headPitch);
							uF.Rotate(sF, -headPitch);
						}
					}
					{
						// head roll
						double rx = Math.Sin(headRoll) * headHeight;
						double ry = (Math.Cos(headRoll) - 1.0) * headHeight;
						cF += sF * rx + uF * ry;
						if (headRoll != 0.0)
						{
							uF.Rotate(dF, -headRoll);
							sF.Rotate(dF, -headRoll);
						}
					}
				}
				else
				{
					// without body or head
					double totalYaw = headYaw;
					double totalPitch = headPitch + bodyPitch;
					double totalRoll = bodyRoll + headRoll;
					if (totalYaw != 0.0)
					{
						dF.Rotate(uF, totalYaw);
						sF.Rotate(uF, totalYaw);
					}
					if (totalPitch != 0.0)
					{
						dF.Rotate(sF, -totalPitch);
						uF.Rotate(sF, -totalPitch);
					}
					if (totalRoll != 0.0)
					{
						uF.Rotate(dF, -totalRoll);
						sF.Rotate(dF, -totalRoll);
					}
				}
				// finish
				Camera.AbsolutePosition = cF;
				Camera.AbsoluteDirection = dF;
				Camera.AbsoluteUp = uF;
				Camera.AbsoluteSide = sF;
			}
		}

		public NewRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem) : base(CurrentHost, CurrentOptions, FileSystem)
		{
		}
	}
}
