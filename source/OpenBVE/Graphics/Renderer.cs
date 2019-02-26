using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal static partial class Renderer
	{
		private static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;
		internal static OutputMode CurrentOutputMode = OutputMode.Default;
		internal static OutputMode PreviousOutputMode = OutputMode.Default;
		//Set LoadTextureImmediatelyMode to NotYet for the first frame
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

		// current opengl data
		private static AlphaFunction AlphaFuncComparison = 0;
		private static float AlphaFuncValue = 0.0f;
		private static bool AlphaTestEnabled = false;
		private static bool BlendEnabled = false;
		private static bool CullEnabled = true;
		internal static bool LightingEnabled = false;
		internal static bool FogEnabled = false;
		internal static bool TexturingEnabled = false;

		// options
		internal static bool OptionLighting = true;
		internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
		internal static Color24 OptionDiffuseColor = new Color24(160, 160, 160);
		internal static Vector3 OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
		internal static float OptionLightingResultingAmount = 1.0f;
		internal static bool OptionNormals = false;
		internal static bool OptionWireframe = false;
		internal static bool OptionBackfaceCulling = true;

		// interface options
		/// <summary>Whether the clock overlay is currently displayed</summary>
		internal static bool OptionClock = false;

		internal enum GradientDisplayMode
		{
			Percentage, UnitOfChange, Permil, None
		}
		/// <summary>Whether the gradient overlay is currently displayed</summary>
		internal static GradientDisplayMode OptionGradient = GradientDisplayMode.None;
		internal enum SpeedDisplayMode
		{
			None, Kmph, Mph
		}
		internal static SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
		internal enum DistanceToNextStationDisplayMode
		{
			None, Km, Mile
		}
		internal static DistanceToNextStationDisplayMode OptionDistanceToNextStation = DistanceToNextStationDisplayMode.None;
		internal static bool OptionFrameRates = false;
		internal static bool OptionBrakeSystems = false;

		// fade to black
		private static double FadeToBlackDueToChangeEnds = 0.0;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// render scene
		
		internal static void RenderScene(double TimeElapsed)
		{
			// initialize
			ResetOpenGlState();
			int OpenGlTextureIndex = 0;

			if (OptionWireframe | OpenGlTextureIndex == 0)
			{
				if (Game.CurrentFog.Start < Game.CurrentFog.End)
				{
					const float fogdistance = 600.0f;
					float n = (fogdistance - Game.CurrentFog.Start) / (Game.CurrentFog.End - Game.CurrentFog.Start);
					float cr = n * inv255 * (float)Game.CurrentFog.Color.R;
					float cg = n * inv255 * (float)Game.CurrentFog.Color.G;
					float cb = n * inv255 * (float)Game.CurrentFog.Color.B;
					GL.ClearColor(cr, cg, cb, 1.0f);
				}
				else
				{
					GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
				}
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
			else
			{
				GL.Clear(ClearBufferMask.DepthBufferBit);
			}
			GL.PushMatrix();
			UpdateViewport(ViewPortChangeMode.ChangeToScenery);
			if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet)
			{
				LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
			}
			// set up camera
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OptionLightPosition.X, (float)OptionLightPosition.Y, (float)OptionLightPosition.Z, 0.0f });
			// fog
			double fd = Game.NextFog.TrackPosition - Game.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - Game.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Game.CurrentFog.Start = Game.PreviousFog.Start * frc + Game.NextFog.Start * fr;
				Game.CurrentFog.End = Game.PreviousFog.End * frc + Game.NextFog.End * fr;
				Game.CurrentFog.Color.R = (byte)((float)Game.PreviousFog.Color.R * frc + (float)Game.NextFog.Color.R * fr);
				Game.CurrentFog.Color.G = (byte)((float)Game.PreviousFog.Color.G * frc + (float)Game.NextFog.Color.G * fr);
				Game.CurrentFog.Color.B = (byte)((float)Game.PreviousFog.Color.B * frc + (float)Game.NextFog.Color.B * fr);
			}
			else
			{
				Game.CurrentFog = Game.PreviousFog;
			}
			// render background
			if (FogEnabled)
			{
				GL.Disable(EnableCap.Fog); FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			UpdateBackground(TimeElapsed);
			RenderEvents(World.AbsoluteCameraPosition);
			// fog
			float aa = Game.CurrentFog.Start;
			float bb = Game.CurrentFog.End;
			if (aa < bb & aa < World.BackgroundImageDistance)
			{
				if (!FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
				if (!FogEnabled)
				{
					GL.Enable(EnableCap.Fog); FogEnabled = true;
				}
			}
			else if (FogEnabled)
			{
				GL.Disable(EnableCap.Fog); FogEnabled = false;
			}
			// world layer
			bool optionLighting = OptionLighting;
			LastBoundTexture = null;
			if (OptionLighting)
			{
				if (!LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); LightingEnabled = true;
				}
				if (World.CameraRestriction == Camera.RestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
				}
			}
			else if (LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); LightingEnabled = false;
			}
			// static opaque
			if (Interface.CurrentOptions.DisableDisplayLists)
			{
				ResetOpenGlState();
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null)
					{
						if (StaticOpaque[i].List != null)
						{
							for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
							{
								if (StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref StaticOpaque[i].List.Faces[j], World.AbsoluteCameraPosition);
								}
							}
						}
					}
				}
			}
			else
			{
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
								ResetOpenGlState();
								GL.NewList(StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
								{
									if (StaticOpaque[i].List.Faces[j] != null)
									{
										RenderFace(ref StaticOpaque[i].List.Faces[j], World.AbsoluteCameraPosition);
									}
								}
								GL.EndList();
							}
							StaticOpaque[i].WorldPosition = World.AbsoluteCameraPosition;
						}
					}
				}
				StaticOpaqueForceUpdate = false;
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null && StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(StaticOpaque[i].WorldPosition.X - World.AbsoluteCameraPosition.X, StaticOpaque[i].WorldPosition.Y - World.AbsoluteCameraPosition.Y, StaticOpaque[i].WorldPosition.Z - World.AbsoluteCameraPosition.Z);
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
			}
			// dynamic opaque
			ResetOpenGlState();
			for (int i = 0; i < DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref DynamicOpaque.Faces[i], World.AbsoluteCameraPosition);
			}
			// dynamic alpha
			ResetOpenGlState();
			DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); BlendEnabled = true;
				GL.DepthMask(false);
				SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); BlendEnabled = false;
				SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); BlendEnabled = true;
				SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
				}
			}
			// motion blur
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None)
			{
				if (LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LightingEnabled = false;
				}
				RenderFullscreenMotionBlur();
			}
			// overlay layer
			if (FogEnabled)
			{
				GL.Disable(EnableCap.Fog); FogEnabled = false;
			}
			GL.LoadIdentity();
			UpdateViewport(ViewPortChangeMode.ChangeToCab);
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			if (World.CameraRestriction == Camera.RestrictionMode.NotAvailable)
			{
				// 3d cab
				ResetOpenGlState(); // TODO: inserted
				GL.DepthMask(true);
				GL.Enable(EnableCap.DepthTest);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				if (!LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); LightingEnabled = true;
				}
				OptionLighting = true;
				GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				// overlay opaque
				SetAlphaFunc(AlphaFunction.Greater, 0.9f);
				for (int i = 0; i < OverlayOpaque.FaceCount; i++)
				{
					RenderFace(ref OverlayOpaque.Faces[i], World.AbsoluteCameraPosition);
				}
				// overlay alpha
				OverlayAlpha.SortPolygons();
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					GL.Enable(EnableCap.Blend); BlendEnabled = true;
					GL.DepthMask(false);
					SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
				}
				else
				{
					GL.Disable(EnableCap.Blend); BlendEnabled = false;
					SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
						if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
						{
							if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
							{
								RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
							}
						}
					}
					GL.Enable(EnableCap.Blend); BlendEnabled = true;
					SetAlphaFunc(AlphaFunction.Less, 1.0f);
					GL.DepthMask(false);
					bool additive = false;
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
						if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								UnsetAlphaFunc();
								additive = true;
							}
							RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
						}
						else
						{
							if (additive)
							{
								SetAlphaFunc(AlphaFunction.Less, 1.0f);
								additive = false;
							}
							RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
						}
					}
				}
			}
			else
			{
				/*
                 * Render 2D Cab
                 * This is actually an animated object generated on the fly and held in memory
                 */
				if (LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting); LightingEnabled = false; // TODO: was 'true' before
				}
				OptionLighting = false;
				if (!BlendEnabled)
				{
					GL.Enable(EnableCap.Blend); BlendEnabled = true;
				}
				GL.DepthMask(false);
				GL.Disable(EnableCap.DepthTest);
				UnsetAlphaFunc();
				OverlayAlpha.SortPolygons();
				for (int i = 0; i < OverlayAlpha.FaceCount; i++)
				{
					RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
				}

			}
			// render overlays
			OptionLighting = optionLighting;
			if (LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); LightingEnabled = false;
			}
			if (FogEnabled)
			{
				GL.Disable(EnableCap.Fog); FogEnabled = false;
			}
			if (BlendEnabled)
			{
				GL.Disable(EnableCap.Blend); BlendEnabled = false;
			}
			UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
			LoadTexturesImmediately = LoadTextureImmediatelyMode.NoLonger;
		}

		// set alpha func

		/// <summary> Stores the last bound OpenGL texture</summary>
		internal static OpenGlTexture LastBoundTexture = null;

		private static void RenderFace(ref ObjectFace Face, Vector3 Camera, bool IsDebugTouchMode = false)
		{
			if (CullEnabled)
			{
				if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
				{
					GL.Disable(EnableCap.CullFace);
					CullEnabled = false;
				}
			}
			else if (OptionBackfaceCulling)
			{
				if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
					CullEnabled = true;
				}
			}
			int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], Camera, IsDebugTouchMode);
		}
		private static void RenderFace(ref MeshMaterial Material, VertexTemplate[] Vertices, OpenGlTextureWrapMode wrap, ref MeshFace Face, Vector3 Camera, bool IsDebugTouchMode = false)
		{
			// texture
			if (Material.DaytimeTexture != null)
			{
				if (Textures.LoadTexture(Material.DaytimeTexture, wrap))
				{
					if (!TexturingEnabled)
					{
						GL.Enable(EnableCap.Texture2D);
						TexturingEnabled = true;
					}
					if (Material.DaytimeTexture.OpenGlTextures[(int)wrap] != LastBoundTexture)
					{
						GL.BindTexture(TextureTarget.Texture2D, Material.DaytimeTexture.OpenGlTextures[(int)wrap].Name);
						LastBoundTexture = Material.DaytimeTexture.OpenGlTextures[(int)wrap];
					}
				}
				else
				{
					if (TexturingEnabled)
					{
						GL.Disable(EnableCap.Texture2D);
						TexturingEnabled = false;
						LastBoundTexture = null;
					}
				}
			}
			else
			{
				if (TexturingEnabled)
				{
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
					LastBoundTexture = null;
				}
			}
			// blend mode
			float factor;
			if (Material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				factor = 1.0f;
				if (!BlendEnabled) GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
				if (FogEnabled)
				{
					GL.Disable(EnableCap.Fog);
				}
			}
			else if (Material.NighttimeTexture == null)
			{
				float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
				if (blend > 1.0f) blend = 1.0f;
				factor = 1.0f - 0.7f * blend;
			}
			else
			{
				factor = 1.0f;
			}
			if (Material.NighttimeTexture != null)
			{
				if (LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LightingEnabled = false;
				}
			}
			else
			{
				if (OptionLighting & !LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting);
					LightingEnabled = true;
				}
			}
			// render daytime polygon
			int FaceType = Face.Flags & MeshFace.FaceTypeMask;
			if (!IsDebugTouchMode)
			{
				switch (FaceType)
				{
					case MeshFace.FaceTypeTriangles:
						GL.Begin(PrimitiveType.Triangles);
						break;
					case MeshFace.FaceTypeTriangleStrip:
						GL.Begin(PrimitiveType.TriangleStrip);
						break;
					case MeshFace.FaceTypeQuads:
						GL.Begin(PrimitiveType.Quads);
						break;
					case MeshFace.FaceTypeQuadStrip:
						GL.Begin(PrimitiveType.QuadStrip);
						break;
					default:
						GL.Begin(PrimitiveType.Polygon);
						break;
				}
			}
			else
			{
				GL.Begin(PrimitiveType.LineLoop);
			}
			if (Material.GlowAttenuationData != 0)
			{
				float alphafactor = (float)Glow.GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, World.AbsoluteCameraPosition);
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				}
			}
			else
			{
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
				}
				
			}
			if ((Material.Flags & MeshMaterial.EmissiveColorMask) != 0)
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
			}
			else
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			}
			if (Material.DaytimeTexture != null)
			{
				if (LightingEnabled)
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
				else
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
			}
			else
			{
				if (LightingEnabled)
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
				else
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
			}
			GL.End();
			// render nighttime polygon
			if (Material.NighttimeTexture != null && Textures.LoadTexture(Material.NighttimeTexture, wrap))
			{
				if (!TexturingEnabled)
				{
					GL.Enable(EnableCap.Texture2D);
					TexturingEnabled = true;
				}
				if (!BlendEnabled)
				{
					GL.Enable(EnableCap.Blend);
				}
				GL.BindTexture(TextureTarget.Texture2D, Material.NighttimeTexture.OpenGlTextures[(int)wrap].Name);
				LastBoundTexture = null;
				GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
				GL.Enable(EnableCap.AlphaTest);
				switch (FaceType)
				{
					case MeshFace.FaceTypeTriangles:
						GL.Begin(PrimitiveType.Triangles);
						break;
					case MeshFace.FaceTypeTriangleStrip:
						GL.Begin(PrimitiveType.TriangleStrip);
						break;
					case MeshFace.FaceTypeQuads:
						GL.Begin(PrimitiveType.Quads);
						break;
					case MeshFace.FaceTypeQuadStrip:
						GL.Begin(PrimitiveType.QuadStrip);
						break;
					default:
						GL.Begin(PrimitiveType.Polygon);
						break;
				}
				float alphafactor;
				if (Material.GlowAttenuationData != 0)
				{
					alphafactor = (float)Glow.GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, Camera);
					float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (blend > 1.0f) blend = 1.0f;
					alphafactor *= blend;
				}
				else
				{
					alphafactor = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (alphafactor > 1.0f) alphafactor = 1.0f;
				}
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				}
				
				if ((Material.Flags & MeshMaterial.EmissiveColorMask) != 0)
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
				}
				else
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				}
				for (int j = 0; j < Face.Vertices.Length; j++)
				{
					GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
					if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
					{
						ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
						GL.Color3(v.Color.R, v.Color.G, v.Color.B);
					}
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
				}
				GL.End();
				RestoreAlphaFunc();
				if (!BlendEnabled)
				{
					GL.Disable(EnableCap.Blend);
				}
			}
			// normals
			if (OptionNormals)
			{
				if (TexturingEnabled)
				{
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++)
				{
					GL.Begin(PrimitiveType.Lines);
					GL.Color4(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - Camera.Z));
					GL.End();
				}
			}
			// finalize
			if (Material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				if (!BlendEnabled) GL.Disable(EnableCap.Blend);
				if (FogEnabled)
				{
					GL.Enable(EnableCap.Fog);
				}
			}
		}
		
	}
}
