using System;
using System.Linq;
using LibRender2;
using LibRender2.MotionBlurs;
using LibRender2.Objects;
using LibRender2.Viewports;
using OpenBve.Graphics.Renderers;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;
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

		private Events events;
		private Overlays overlays;
		internal Touch Touch;

		public override void Initialize(HostInterface CurrentHost, BaseOptions CurrentOptions)
		{
			base.Initialize(CurrentHost, CurrentOptions);

			events = new Events(this);
			overlays = new Overlays(this);
			Touch = new Touch(this);
			ObjectsSortedByStart = new int[] { };
			ObjectsSortedByEnd = new int[] { };
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
			if (obj.Mesh.Faces.Length == 0)
			{
				//Null object- Waste of time trying to calculate anything for these
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

		public override void UpdateViewport(int Width, int Height)
		{
			Screen.Width = Width;
			Screen.Height = Height;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);

			Screen.AspectRatio = Screen.Width / (double)Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);

			switch (CurrentViewportMode)
			{
				case ViewportMode.Scenery:
					BackgroundObject b = Program.CurrentRoute.CurrentBackground as BackgroundObject;
					double cd = b != null ? Math.Max(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance, b.ClipDistance) : Program.CurrentRoute.CurrentBackground.BackgroundImageDistance;
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.5, cd);
					break;
				case ViewportMode.Cab:
					CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.025, 50.0);
					break;
			}

			Touch.UpdateViewport();
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

			UpdateViewport(ViewportChangeMode.ChangeToScenery);

			// set up camera
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			GL.Light(LightName.Light0, LightParameter.Position, new[] { (float)Lighting.OptionLightPosition.X, (float)Lighting.OptionLightPosition.Y, (float)-Lighting.OptionLightPosition.Z, 0.0f });
			
			Lighting.OptionLightingResultingAmount = (Lighting.OptionAmbientColor.R + Lighting.OptionAmbientColor.G + Lighting.OptionAmbientColor.B) / 480.0f;

			if (Lighting.OptionLightingResultingAmount > 1.0f)
			{
				Lighting.OptionLightingResultingAmount = 1.0f;
			}
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
			Program.CurrentRoute.UpdateBackground(TimeElapsed, Game.CurrentInterface != Game.InterfaceType.Normal);

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
				SetFogForImmediateMode();
			}
			else
			{
				OptionFog = false;
			}

			// world layer
			// opaque face
			if (Interface.CurrentOptions.IsUseNewRenderer)
			{
				//Setup the shader for rendering the scene
				DefaultShader.Activate();
				if (OptionLighting)
				{
					DefaultShader.SetIsLight(true);
					DefaultShader.SetLightPosition(Lighting.OptionLightPosition);
					DefaultShader.SetLightAmbient(Lighting.OptionAmbientColor);
					DefaultShader.SetLightDiffuse(Lighting.OptionDiffuseColor);
					DefaultShader.SetLightSpecular(Lighting.OptionSpecularColor);
				}
				if (OptionFog)
				{
					DefaultShader.SetIsFog(true);
					DefaultShader.SetFogStart(Fog.Start);
					DefaultShader.SetFogEnd(Fog.End);
					DefaultShader.SetFogColor(Fog.Color);
				}
				DefaultShader.SetTexture(0);
				DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			}
			ResetOpenGlState();
			foreach (FaceState face in VisibleObjects.OpaqueFaces)
			{
				if (Interface.CurrentOptions.IsUseNewRenderer)
				{
					RenderFace(DefaultShader, face);
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
						RenderFace(DefaultShader, face);
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
								RenderFace(DefaultShader, face);
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
							RenderFace(DefaultShader, face);
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
							RenderFace(DefaultShader, face);
						}
						else
						{
							RenderFaceImmediateMode(face);
						}
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
				DefaultShader.Deactivate();
				MotionBlur.RenderFullscreen(Interface.CurrentOptions.MotionBlur, FrameRate, Math.Abs(Camera.CurrentSpeed));
			}
			if (Interface.CurrentOptions.IsUseNewRenderer)
			{
				DefaultShader.Activate();
				ResetShader(DefaultShader); //Must reset shader between overlay and world layers for correct lighting results
				DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			}
			// overlay layer
			OptionFog = false;
			UpdateViewport(ViewportChangeMode.ChangeToCab);
			CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(Camera.AbsoluteDirection.X, Camera.AbsoluteDirection.Y, -Camera.AbsoluteDirection.Z), new Vector3(Camera.AbsoluteUp.X, Camera.AbsoluteUp.Y, -Camera.AbsoluteUp.Z));
			if (Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				ResetOpenGlState(); // TODO: inserted
				GL.Clear(ClearBufferMask.DepthBufferBit);
				OptionLighting = true;
				Color24 prevOptionAmbientColor = Lighting.OptionAmbientColor;
				Color24 prevOptionDiffuseColor = Lighting.OptionDiffuseColor;
				Lighting.OptionAmbientColor = new Color24(178, 178, 178);
				Lighting.OptionDiffuseColor = new Color24(178, 178, 178);
				GL.Light(LightName.Light0, LightParameter.Ambient, new[] { inv255 * 178, inv255 * 178, inv255 * 178, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { inv255 * 178, inv255 * 178, inv255 * 178, 1.0f });

				// overlay opaque face
				foreach (FaceState face in VisibleObjects.OverlayOpaqueFaces)
				{
					if (Interface.CurrentOptions.IsUseNewRenderer)
					{
						RenderFace(DefaultShader, face);
					}
					else
					{
						RenderFaceImmediateMode(face);
					}
				}

				// overlay alpha face
				ResetOpenGlState();
				VisibleObjects.SortPolygonsInOverlayAlphaFaces();

				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					SetBlendFunc();
					SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					GL.DepthMask(false);

					foreach (FaceState face in VisibleObjects.OverlayAlphaFaces)
					{
						if (Interface.CurrentOptions.IsUseNewRenderer)
						{
							RenderFace(DefaultShader, face);
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

					foreach (FaceState face in VisibleObjects.OverlayAlphaFaces)
					{
						if (face.Object.Prototype.Mesh.Materials[face.Face.Material].BlendMode == MeshMaterialBlendMode.Normal && face.Object.Prototype.Mesh.Materials[face.Face.Material].GlowAttenuationData == 0)
						{
							if (face.Object.Prototype.Mesh.Materials[face.Face.Material].Color.A == 255)
							{
								if (Interface.CurrentOptions.IsUseNewRenderer)
								{
									RenderFace(DefaultShader, face);
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

					foreach (FaceState face in VisibleObjects.OverlayAlphaFaces)
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
								RenderFace(DefaultShader, face);
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
								RenderFace(DefaultShader, face);
							}
							else
							{
								RenderFaceImmediateMode(face);
							}
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
				SetBlendFunc();
				UnsetAlphaFunc();
				GL.Disable(EnableCap.DepthTest);
				GL.DepthMask(false);
				VisibleObjects.SortPolygonsInOverlayAlphaFaces();
				foreach (FaceState face in VisibleObjects.OverlayAlphaFaces)
				{
					if (Interface.CurrentOptions.IsUseNewRenderer)
					{
						RenderFace(DefaultShader, face);
					}
					else
					{
						RenderFaceImmediateMode(face);
					}
				}
			}
			if (Interface.CurrentOptions.IsUseNewRenderer)
			{
				/*
				 * Must remember to de-activate at the end of the render sequence if in GL3 mode.
				 * The overlays currently use immediate mode and do not work correctly with the shader active
				 */
				DefaultShader.Deactivate();
			}
			// render touch
			OptionLighting = false;
			Touch.RenderScene();
			
			// render overlays
			ResetOpenGlState();
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			overlays.Render(TimeElapsed);
			OptionLighting = true;
		}
	}
}
