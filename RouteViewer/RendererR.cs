// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Route Viewer                            ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using Tao.OpenGl;

namespace OpenBve {
	internal static class Renderer {

		// screen (output window)
		internal static int ScreenWidth;
		internal static int ScreenHeight;

		// first frame behavior
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
		internal enum TransparencyMode { Sharp, Smooth }

		// object list
		internal enum ObjectType : byte {
			/// <summary>The object is part of the static scenery. The matching ObjectListType is StaticOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
			Static = 1,
			/// <summary>The object is part of the animated scenery or of a train exterior. The matching ObjectListType is DynamicOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
			Dynamic = 2,
			/// <summary>The object is part of the cab. The matching ObjectListType is OverlayOpaque for fully opaque faces, and OverlayAlpha for all other faces.</summary>
			Overlay = 3
		}
		private struct Object {
			internal int ObjectIndex;
			internal int[] FaceListIndices;
			internal ObjectType Type;
		}
		private static Object[] ObjectList = new Object[256];
		private static int ObjectListCount = 0;

		// face lists
		private struct ObjectFace {
			internal int ObjectListIndex;
			internal int ObjectIndex;
			internal int FaceIndex;
		}
		// opaque
		private static ObjectFace[] OpaqueList = new ObjectFace[256];
		internal static int OpaqueListCount = 0;
		// transparent color
		private static ObjectFace[] TransparentColorList = new ObjectFace[256];
		private static double[] TransparentColorListDistance = new double[256];
		internal static int TransparentColorListCount = 0;
		// alpha
		private static ObjectFace[] AlphaList = new ObjectFace[256];
		private static double[] AlphaListDistance = new double[256];
		internal static int AlphaListCount = 0;
		// overlay
		private static ObjectFace[] OverlayList = new ObjectFace[256];
		private static double[] OverlayListDistance = new double[256];
		internal static int OverlayListCount = 0;

		// current opengl data
		private static int AlphaFuncComparison = 0;
		private static float AlphaFuncValue = 0.0f;
		private static bool BlendEnabled = false;
		private static bool AlphaTestEnabled = false;
		private static bool CullEnabled = true;
		internal static bool LightingEnabled = false;
		internal static bool FogEnabled = false;
		private static bool TexturingEnabled = false;
		private static bool EmissiveEnabled = false;
		private static bool TransparentColorDepthSorting = false;

		// textures
		private static int BackgroundChangeTexture = -1;
		private static int BrightnessChangeTexture = -1;
		private static int TransponderTexture = -1;
		private static int SectionTexture = -1;
		private static int LimitTexture = -1;
		private static int StationStartTexture = -1;
		private static int StationEndTexture = -1;
		private static int StopTexture = -1;
		private static int BufferTexture = -1;
		private static int SoundTexture = -1;

		// options
		internal static bool OptionLighting = true;
		internal static World.ColorRGB OptionAmbientColor = new World.ColorRGB(160, 160, 160);
		internal static World.ColorRGB OptionDiffuseColor = new World.ColorRGB(160, 160, 160);
		internal static World.Vector3Df OptionLightPosition = new World.Vector3Df(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
		internal static float OptionLightingResultingAmount = 1.0f;
		internal static bool OptionNormals = false;
		internal static bool OptionWireframe = false;
		internal static bool OptionEvents = false;
		internal static bool OptionInterface = true;
		internal static bool OptionBackfaceCulling = true;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// reset
		internal static void Reset() {
			LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
			ObjectList = new Object[256];
			ObjectListCount = 0;
			OpaqueList = new ObjectFace[256];
			OpaqueListCount = 0;
			TransparentColorList = new ObjectFace[256];
			TransparentColorListDistance = new double[256];
			TransparentColorListCount = 0;
			AlphaList = new ObjectFace[256];
			AlphaListDistance = new double[256];
			AlphaListCount = 0;
			OverlayList = new ObjectFace[256];
			OverlayListDistance = new double[256];
			OverlayListCount = 0;
			OptionLighting = true;
			OptionAmbientColor = new World.ColorRGB(160, 160, 160);
			OptionDiffuseColor = new World.ColorRGB(160, 160, 160);
			OptionLightPosition = new World.Vector3Df(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
			OptionLightingResultingAmount = 1.0f;
			Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
		}

		// initialize
		internal static void Initialize() {
			// opengl
			Gl.glShadeModel(Gl.GL_DECAL);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glClearColor(0.67f, 0.67f, 0.67f, 0.0f);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glEnable(Gl.GL_TEXTURE_2D); TexturingEnabled = true;
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glDepthFunc(Gl.GL_LEQUAL);
			Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_GENERATE_MIPMAP_HINT, Gl.GL_NICEST);
			Gl.glEnable(Gl.GL_CULL_FACE); CullEnabled = true;
			Gl.glCullFace(Gl.GL_FRONT);
			Gl.glDisable(Gl.GL_DITHER);
			// textures
			string Folder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			BackgroundChangeTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "background.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			BrightnessChangeTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "brightness.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			TransponderTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "transponder.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			SectionTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "section.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			LimitTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "limit.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			StationStartTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_start.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			StationEndTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_end.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			StopTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "stop.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			BufferTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "buffer.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			SoundTexture = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "sound.png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, true);
			TextureManager.ValidateTexture(ref BackgroundChangeTexture);
			TextureManager.ValidateTexture(ref BrightnessChangeTexture);
			TextureManager.ValidateTexture(ref TransponderTexture);
			TextureManager.ValidateTexture(ref SectionTexture);
			TextureManager.ValidateTexture(ref LimitTexture);
			TextureManager.ValidateTexture(ref StationStartTexture);
			TextureManager.ValidateTexture(ref StationEndTexture);
			TextureManager.ValidateTexture(ref StopTexture);
			TextureManager.ValidateTexture(ref BufferTexture);
			TextureManager.ValidateTexture(ref SoundTexture);
			// opengl
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glPushMatrix();
			Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			Glu.gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
			Gl.glPopMatrix();
			TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.Bilinear;
		}

		// initialize lighting
		internal static void InitializeLighting() {
			if (OptionAmbientColor.R == 255 & OptionAmbientColor.G == 255 & OptionAmbientColor.B == 255 & OptionDiffuseColor.R == 0 & OptionDiffuseColor.G == 0 & OptionDiffuseColor.B == 0) {
				OptionLighting = false;
			} else {
				OptionLighting = true;
			}
			if (OptionLighting) {
				Gl.glCullFace(Gl.GL_FRONT); CullEnabled = true;
				Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
				Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
				Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				Gl.glEnable(Gl.GL_LIGHTING); LightingEnabled = true;
				Gl.glEnable(Gl.GL_LIGHT0);
				Gl.glEnable(Gl.GL_COLOR_MATERIAL);
				Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);
				Gl.glShadeModel(Gl.GL_SMOOTH);
				OptionLightingResultingAmount = (float)((int)OptionAmbientColor.R + (int)OptionAmbientColor.G + (int)OptionAmbientColor.B) / 480.0f;
				if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
			} else {
				Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
			}
			Gl.glDepthFunc(Gl.GL_LEQUAL);
		}

		// render scene
		internal static byte[] PixelBuffer = null;
		internal static int PixelBufferOpenGlTextureIndex = 0;
		internal static void RenderScene(double TimeElapsed) {
			// initialize
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glDepthMask(Gl.GL_TRUE);
			if (OptionWireframe | World.CurrentBackground.Texture == -1) {
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			} else {
				int OpenGlTextureIndex = TextureManager.UseTexture(World.CurrentBackground.Texture, TextureManager.UseMode.Normal);
				if (OpenGlTextureIndex > 0) {
					Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
				} else {
					Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
				}
			}
			Gl.glPushMatrix();
			if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet) {
				LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
				ReAddObjects();
			}
			// setup camera
			double cx = World.AbsoluteCameraPosition.X;
			double cy = World.AbsoluteCameraPosition.Y;
			double cz = World.AbsoluteCameraPosition.Z;
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Glu.gluLookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			if (OptionLighting) {
				Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { OptionLightPosition.X, OptionLightPosition.Y, OptionLightPosition.Z, 0.0f });
			}
			// fog
			double fd = Game.NextFog.TrackPosition - Game.PreviousFog.TrackPosition;
			if (fd != 0.0) {
				float fr = (float)((World.CameraTrackFollower.TrackPosition - Game.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Game.CurrentFog.Start = Game.PreviousFog.Start * frc + Game.NextFog.Start * fr;
				Game.CurrentFog.End = Game.PreviousFog.End * frc + Game.NextFog.End * fr;
				Game.CurrentFog.Color.R = (byte)((float)Game.PreviousFog.Color.R * frc + (float)Game.NextFog.Color.R * fr);
				Game.CurrentFog.Color.G = (byte)((float)Game.PreviousFog.Color.G * frc + (float)Game.NextFog.Color.G * fr);
				Game.CurrentFog.Color.B = (byte)((float)Game.PreviousFog.Color.B * frc + (float)Game.NextFog.Color.B * fr);
			} else {
				Game.CurrentFog = Game.PreviousFog;
				
			}
			// render background
			if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			RenderBackground(dx, dy, dz, TimeElapsed);
			// fog
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < World.BackgroundImageDistance) {
				if (!FogEnabled) {
					Gl.glFogi(Gl.GL_FOG_MODE, Gl.GL_LINEAR);
				}
				Gl.glFogf(Gl.GL_FOG_START, Game.CurrentFog.Start);
				Gl.glFogf(Gl.GL_FOG_END, Game.CurrentFog.End);
				Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
				if (!FogEnabled) {
					Gl.glEnable(Gl.GL_FOG); FogEnabled = true;
				}
				Gl.glClearColor(inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f);
			} else if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			// render background
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			RenderBackground(dx, dy, dz, TimeElapsed);
			// render polygons
			if (OptionLighting) {
				if (!LightingEnabled) {
					Gl.glEnable(Gl.GL_LIGHTING);
					LightingEnabled = true;
				}
			} else if (LightingEnabled) {
				Gl.glDisable(Gl.GL_LIGHTING);
				LightingEnabled = false;
			}
			SetAlphaFunc(Gl.GL_GREATER, 0.9f);
			BlendEnabled = false; Gl.glDisable(Gl.GL_BLEND);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glDepthMask(Gl.GL_TRUE);
			LastBoundTexture = 0;
			// opaque list
			for (int i = 0; i < OpaqueListCount; i++) {
				RenderFace(ref OpaqueList[i], cx, cy, cz);
			}
			// events
			if (OptionEvents) RenderEvents(cx, cy, cz);
			// transparent color list
			if (TransparentColorDepthSorting) {
				SortPolygons(TransparentColorList, TransparentColorListCount, TransparentColorListDistance, 1, TimeElapsed);
				BlendEnabled = true; Gl.glEnable(Gl.GL_BLEND);
				for (int i = 0; i < TransparentColorListCount; i++) {
					Gl.glDepthMask(Gl.GL_FALSE);
					SetAlphaFunc(Gl.GL_LESS, 1.0f);
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
					Gl.glDepthMask(Gl.GL_TRUE);
					SetAlphaFunc(Gl.GL_EQUAL, 1.0f);
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
				}
			} else {
				for (int i = 0; i < TransparentColorListCount; i++) {
					RenderFace(ref TransparentColorList[i], cx, cy, cz);
				}
			}
			// alpha list
			SortPolygons(AlphaList, AlphaListCount, AlphaListDistance, 2, TimeElapsed);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth) {
				BlendEnabled = true; Gl.glEnable(Gl.GL_BLEND);
				bool depthMask = true;
				for (int i = 0; i < AlphaListCount; i++) {
					int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
					if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Additive) {
						if (depthMask) {
							Gl.glDepthMask(Gl.GL_FALSE);
							depthMask = false;
						}
						SetAlphaFunc(Gl.GL_GREATER, 0.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
					} else {
						if (depthMask) {
							Gl.glDepthMask(Gl.GL_FALSE);
							depthMask = false;
						}
						SetAlphaFunc(Gl.GL_LESS, 1.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
						Gl.glDepthMask(Gl.GL_TRUE);
						depthMask = true;
						SetAlphaFunc(Gl.GL_EQUAL, 1.0f);
						RenderFace(ref AlphaList[i], cx, cy, cz);
					}
				}
			} else {
				BlendEnabled = true; Gl.glEnable(Gl.GL_BLEND);
				Gl.glDepthMask(Gl.GL_FALSE);
				SetAlphaFunc(Gl.GL_GREATER, 0.0f);
				for (int i = 0; i < AlphaListCount; i++) {
					RenderFace(ref AlphaList[i], cx, cy, cz);
				}
			}
			// overlay list
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			Gl.glDepthMask(Gl.GL_FALSE);
			if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			SortPolygons(OverlayList, OverlayListCount, OverlayListDistance, 3, TimeElapsed);
			for (int i = 0; i < OverlayListCount; i++) {
				RenderFace(ref OverlayList[i], cx, cy, cz);
			}
			// render overlays
			BlendEnabled = false; Gl.glDisable(Gl.GL_BLEND);
			SetAlphaFunc(Gl.GL_GREATER, 0.9f);
			AlphaTestEnabled = false; Gl.glDisable(Gl.GL_ALPHA_TEST);
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			if (LightingEnabled) {
				Gl.glDisable(Gl.GL_LIGHTING);
				LightingEnabled = false;
			}
			RenderOverlays(TimeElapsed);
			// finalize rendering
			Gl.glPopMatrix();
		}
		
		// set alpha func
		private static void SetAlphaFunc(int Comparison, float Value) {
			AlphaFuncComparison = Comparison;
			AlphaFuncValue = Value;
			Gl.glAlphaFunc(Comparison, Value);
		}

		// render face
		private static int LastBoundTexture = 0;
		private static void RenderFace(ref ObjectFace Face, double CameraX, double CameraY, double CameraZ) {
			if (CullEnabled) {
				if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) != 0) {
					Gl.glDisable(Gl.GL_CULL_FACE);
					CullEnabled = false;
				}
			} else if (OptionBackfaceCulling) {
				if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) == 0) {
					Gl.glEnable(Gl.GL_CULL_FACE);
					CullEnabled = true;
				}
			}
			int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
		}
		private static void RenderFace(ref World.MeshMaterial Material, World.Vertex[] Vertices, ref World.MeshFace Face, double CameraX, double CameraY, double CameraZ) {
			// texture
			int OpenGlNighttimeTextureIndex = Material.NighttimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.NighttimeTextureIndex, TextureManager.UseMode.Normal) : 0;
			int OpenGlDaytimeTextureIndex = Material.DaytimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.DaytimeTextureIndex, TextureManager.UseMode.Normal) : 0;
			if (OpenGlDaytimeTextureIndex != 0) {
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				if (OpenGlDaytimeTextureIndex != LastBoundTexture) {
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlDaytimeTextureIndex);
					LastBoundTexture = OpenGlDaytimeTextureIndex;
				}
				if (TextureManager.Textures[Material.DaytimeTextureIndex].Transparency != TextureManager.TextureTransparencyMode.None) {
					if (!AlphaTestEnabled) {
						Gl.glEnable(Gl.GL_ALPHA_TEST);
						AlphaTestEnabled = true;
					}
				} else if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
			} else {
				if (TexturingEnabled) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
					LastBoundTexture = 0;
				}
				if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
			}
			// blend mode
			float factor;
			if (Material.BlendMode == World.MeshMaterialBlendMode.Additive) {
				factor = 1.0f;
				if (!BlendEnabled) Gl.glEnable(Gl.GL_BLEND);
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
				if (FogEnabled) {
					Gl.glDisable(Gl.GL_FOG);
				}
			} else if (OpenGlNighttimeTextureIndex == 0) {
				float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
				if (blend > 1.0f) blend = 1.0f;
				factor = 1.0f - 0.8f * blend;
			} else {
				factor = 1.0f;
			}
			if (OpenGlNighttimeTextureIndex != 0) {
				if (LightingEnabled) {
					Gl.glDisable(Gl.GL_LIGHTING);
					LightingEnabled = false;
				}
			} else {
				if (OptionLighting & !LightingEnabled) {
					Gl.glEnable(Gl.GL_LIGHTING);
					LightingEnabled = true;
				}
			}
			// render daytime polygon
			int FaceType = Face.Flags & World.MeshFace.FaceTypeMask;
			switch (FaceType) {
				case World.MeshFace.FaceTypeTriangles:
					Gl.glBegin(Gl.GL_TRIANGLES);
					break;
				case World.MeshFace.FaceTypeTriangleStrip:
					Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
					break;
				case World.MeshFace.FaceTypeQuads:
					Gl.glBegin(Gl.GL_QUADS);
					break;
				case World.MeshFace.FaceTypeQuadStrip:
					Gl.glBegin(Gl.GL_QUAD_STRIP);
					break;
				default:
					Gl.glBegin(Gl.GL_POLYGON);
					break;
			}
			if (Material.GlowAttenuationData != 0) {
				float alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
			} else {
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
			}
			if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0) {
				Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
				EmissiveEnabled = true;
			} else if (EmissiveEnabled) {
				Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				EmissiveEnabled = false;
			}
			if (OpenGlDaytimeTextureIndex != 0) {
				if (LightingEnabled) {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				} else {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				}
			} else {
				if (LightingEnabled) {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				} else {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				}
			}
			Gl.glEnd();
			// render nighttime polygon
			if (OpenGlNighttimeTextureIndex != 0) {
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				if (!BlendEnabled) {
					Gl.glEnable(Gl.GL_BLEND);
				}
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlNighttimeTextureIndex);
				LastBoundTexture = 0;
				SetAlphaFunc(Gl.GL_GREATER, 0.0f);
				switch (FaceType) {
					case World.MeshFace.FaceTypeTriangles:
						Gl.glBegin(Gl.GL_TRIANGLES);
						break;
					case World.MeshFace.FaceTypeTriangleStrip:
						Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
						break;
					case World.MeshFace.FaceTypeQuads:
						Gl.glBegin(Gl.GL_QUADS);
						break;
					case World.MeshFace.FaceTypeQuadStrip:
						Gl.glBegin(Gl.GL_QUAD_STRIP);
						break;
					default:
						Gl.glBegin(Gl.GL_POLYGON);
						break;
				}
				float alphafactor;
				if (Material.GlowAttenuationData != 0) {
					alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
					float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (blend > 1.0f) blend = 1.0f;
					alphafactor *= blend;
				} else {
					alphafactor = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (alphafactor > 1.0f) alphafactor = 1.0f;
				}
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0) {
					Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
					EmissiveEnabled = true;
				} else if (EmissiveEnabled) {
					Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
					EmissiveEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++) {
					Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
				}
				Gl.glEnd();
				if (AlphaFuncValue != 0.0) {
					Gl.glAlphaFunc(AlphaFuncComparison, AlphaFuncValue);
				}
				if (!BlendEnabled) {
					Gl.glDisable(Gl.GL_BLEND);
				}
			}
			// normals
			if (OptionNormals) {
				if (TexturingEnabled) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
				}
				if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++) {
					Gl.glBegin(Gl.GL_LINES);
					Gl.glColor4f(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - CameraZ));
					Gl.glEnd();
				}
			}
			// finalize
			if (Material.BlendMode == World.MeshMaterialBlendMode.Additive) {
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
				if (!BlendEnabled) Gl.glDisable(Gl.GL_BLEND);
				if (FogEnabled) {
					Gl.glEnable(Gl.GL_FOG);
				}
			}
		}

		// render background
		private static void RenderBackground(double dx, double dy, double dz, double TimeElapsed) {
			// fog
			const float fogdistance = 600.0f;
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < fogdistance) {
				float cr = inv255 * (float)Game.CurrentFog.Color.R;
				float cg = inv255 * (float)Game.CurrentFog.Color.G;
				float cb = inv255 * (float)Game.CurrentFog.Color.B;
				if (!FogEnabled) {
					Gl.glFogi(Gl.GL_FOG_MODE, Gl.GL_LINEAR);
				}
				Gl.glFogf(Gl.GL_FOG_START, Game.CurrentFog.Start * (float)World.BackgroundImageDistance / fogdistance);
				Gl.glFogf(Gl.GL_FOG_END, Game.CurrentFog.End * (float)World.BackgroundImageDistance / fogdistance);
				Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { cr, cg, cb, 1.0f });
				if (!FogEnabled) {
					Gl.glEnable(Gl.GL_FOG); FogEnabled = true;
				}
			} else if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			// render
			if (World.TargetBackgroundCountdown >= 0.0) {
				// fade
				World.TargetBackgroundCountdown -= TimeElapsed;
				if (World.TargetBackgroundCountdown < 0.0) {
					World.CurrentBackground = World.TargetBackground;
					World.TargetBackgroundCountdown = -1.0;
					RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
				} else {
					RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
					AlphaFuncValue = 0.0f; Gl.glAlphaFunc(AlphaFuncComparison, AlphaFuncValue);
					float Alpha = (float)(1.0 - World.TargetBackgroundCountdown / World.TargetBackgroundDefaultCountdown);
					RenderBackground(World.TargetBackground, dx, dy, dz, Alpha);
				}
			} else {
				// single
				RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
			}
		}
		private static void RenderBackground(World.Background Data, double dx, double dy, double dz, float Alpha) {
			if (Data.Texture >= 0) {
				int OpenGlTextureIndex = TextureManager.UseTexture(Data.Texture, TextureManager.UseMode.LoadImmediately);
				if (OpenGlTextureIndex > 0) {
					if (LightingEnabled) {
						Gl.glDisable(Gl.GL_LIGHTING);
						LightingEnabled = false;
					}
					if (!TexturingEnabled) {
						Gl.glEnable(Gl.GL_TEXTURE_2D);
						TexturingEnabled = true;
					}
					if (Alpha == 1.0f) {
						if (BlendEnabled) {
							Gl.glDisable(Gl.GL_BLEND);
							BlendEnabled = false;
						}
					} else if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND);
						BlendEnabled = true;
					}
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlTextureIndex);
					Gl.glColor4f(1.0f, 1.0f, 1.0f, Alpha);
					float y0, y1;
					if (Data.KeepAspectRatio) {
						int tw = TextureManager.Textures[Data.Texture].Width;
						int th = TextureManager.Textures[Data.Texture].Height;
						double hh = Math.PI * World.BackgroundImageDistance * (double)th / ((double)tw * (double)Data.Repetition);
						y0 = (float)(-0.5 * hh);
						y1 = (float)(1.5 * hh);
					} else {
						y0 = (float)(-0.125 * World.BackgroundImageDistance);
						y1 = (float)(0.375 * World.BackgroundImageDistance);
					}
					const int n = 32;
					World.Vector3Df[] bottom = new World.Vector3Df[n];
					World.Vector3Df[] top = new World.Vector3Df[n];
					double angleValue = 2.61799387799149 - 3.14159265358979 / (double)n;
					double angleIncrement = 6.28318530717958 / (double)n;
					for (int i = 0; i < n; i++) {
						float x = (float)(World.BackgroundImageDistance * Math.Cos(angleValue));
						float z = (float)(World.BackgroundImageDistance * Math.Sin(angleValue));
						bottom[i] = new World.Vector3Df(x, y0, z);
						top[i] = new World.Vector3Df(x, y1, z);
						angleValue += angleIncrement;
					}
					float textureStart = 0.5f * (float)Data.Repetition / (float)n;
					float textureIncrement = -(float)Data.Repetition / (float)n;
					double textureX = textureStart;
					for (int i = 0; i < n; i++) {
						int j = (i + 1) % n;
						// side wall
						Gl.glBegin(Gl.GL_QUADS);
						Gl.glTexCoord2d(textureX, 0.005f);
						Gl.glVertex3f(top[i].X, top[i].Y, top[i].Z);
						Gl.glTexCoord2d(textureX, 0.995f);
						Gl.glVertex3f(bottom[i].X, bottom[i].Y, bottom[i].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
						Gl.glVertex3f(bottom[j].X, bottom[j].Y, bottom[j].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
						Gl.glVertex3f(top[j].X, top[j].Y, top[j].Z);
						Gl.glEnd();
						// top cap
						Gl.glBegin(Gl.GL_TRIANGLES);
						Gl.glTexCoord2d(textureX, 0.005f);
						Gl.glVertex3f(top[i].X, top[i].Y, top[i].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
						Gl.glVertex3f(top[j].X, top[j].Y, top[j].Z);
						Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.1f);
						Gl.glVertex3f(0.0f, top[i].Y, 0.0f);
						// bottom cap
						Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.9f);
						Gl.glVertex3f(0.0f, bottom[i].Y, 0.0f);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
						Gl.glVertex3f(bottom[j].X, bottom[j].Y, bottom[j].Z);
						Gl.glTexCoord2d(textureX, 0.995f);
						Gl.glVertex3f(bottom[i].X, bottom[i].Y, bottom[i].Z);
						Gl.glEnd();
						// finish
						textureX += textureIncrement;
					}
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
					if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND);
						BlendEnabled = true;
					}
				}
			}
		}

		// render events
		private static void RenderEvents(double CameraX, double CameraY, double CameraZ) {
			if (TrackManager.CurrentTrack.Elements == null) {
				return;
			}
			LastBoundTexture = 0;
			if (LightingEnabled) {
				Gl.glDisable(Gl.GL_LIGHTING);
				LightingEnabled = false;
			}
			if (AlphaTestEnabled) {
				Gl.glDisable(Gl.GL_ALPHA_TEST);
				AlphaTestEnabled = false;
			}
			double da = -World.BackwardViewingDistance - World.ExtraViewingDistance;
			double db = World.ForwardViewingDistance + World.ExtraViewingDistance;
			bool[] sta = new bool[Game.Stations.Length];
			// events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				double p = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						TrackManager.GeneralEvent e = TrackManager.CurrentTrack.Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; int t;
						if (e is TrackManager.BrightnessChangeEvent) {
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						} else if (e is TrackManager.BackgroundChangeEvent) {
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						} else if (e is TrackManager.StationStartEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							TrackManager.StationStartEvent f = (TrackManager.StationStartEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.StationEndEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.LimitChangeEvent) {
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						} else if (e is TrackManager.SectionChangeEvent) {
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						} else if (e is TrackManager.TransponderEvent) {
							s = 0.15;
							dy = 0.4;
							t = TransponderTexture;
						} else if (e is TrackManager.SoundEvent) {
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = SoundTexture;
						} else {
							s = 0.2;
							dy = 1.0;
							t = -1;
						}
						if (t >= 0) {
							TrackManager.TrackFollower f = new TrackManager.TrackFollower();
							f.TriggerType = TrackManager.EventTriggerType.None;
							f.TrackPosition = p;
							TrackManager.UpdateTrackFollower(ref f, p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;
							RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraX, CameraY, CameraZ, t);
						}
					}
				}
			}
			// stops
			for (int i = 0; i < sta.Length; i++) {
				if (sta[i]) {
					for (int j = 0; j < Game.Stations[i].Stops.Length; j++) {
						const double dy = 1.4;
						const double s = 0.2;
						double p = Game.Stations[i].Stops[j].TrackPosition;
						TrackManager.TrackFollower f = new TrackManager.TrackFollower();
						f.TriggerType = TrackManager.EventTriggerType.None;
						f.TrackPosition = p;
						TrackManager.UpdateTrackFollower(ref f, p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;
						RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraX, CameraY, CameraZ, StopTexture);
					}
				}
			}
			// buffers
			for (int i = 0; i < Game.BufferTrackPositions.Length; i++) {
				double p = Game.BufferTrackPositions[i];
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					const double dy = 2.5;
					const double s = 0.25;
					TrackManager.TrackFollower f = new TrackManager.TrackFollower();
					f.TriggerType = TrackManager.EventTriggerType.None;
					f.TrackPosition = p;
					TrackManager.UpdateTrackFollower(ref f, p, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;
					RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraX, CameraY, CameraZ, BufferTexture);
				}
			}
		}
		private static void RenderCube(World.Vector3D Position, World.Vector3D Direction, World.Vector3D Up, World.Vector3D Side, double Size, double CameraX, double CameraY, double CameraZ, int TextureIndex) {
			int OpenGlTextureIndex = TextureManager.UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
			if (OpenGlTextureIndex > 0) {
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
				}
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlTextureIndex);
			} else {
				if (TexturingEnabled) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
				}
			}
			World.Vector3D[] v = new World.Vector3D[8];
			v[0] = new World.Vector3D(Size, Size, -Size);
			v[1] = new World.Vector3D(Size, -Size, -Size);
			v[2] = new World.Vector3D(-Size, -Size, -Size);
			v[3] = new World.Vector3D(-Size, Size, -Size);
			v[4] = new World.Vector3D(Size, Size, Size);
			v[5] = new World.Vector3D(Size, -Size, Size);
			v[6] = new World.Vector3D(-Size, -Size, Size);
			v[7] = new World.Vector3D(-Size, Size, Size);
			for (int i = 0; i < 8; i++) {
				World.Rotate(ref v[i].X, ref v[i].Y, ref v[i].Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
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
			if (OpenGlTextureIndex != 0) {
				World.Vector2D[][] t = new World.Vector2D[6][];
				t[0] = new World.Vector2D[] { new World.Vector2D(1.0, 0.0), new World.Vector2D(1.0, 1.0), new World.Vector2D(0.0, 1.0), new World.Vector2D(0.0, 0.0) };
				t[1] = new World.Vector2D[] { new World.Vector2D(0.0, 0.0), new World.Vector2D(1.0, 0.0), new World.Vector2D(1.0, 1.0), new World.Vector2D(0.0, 1.0) };
				t[2] = new World.Vector2D[] { new World.Vector2D(1.0, 1.0), new World.Vector2D(0.0, 1.0), new World.Vector2D(0.0, 0.0), new World.Vector2D(1.0, 0.0) };
				t[3] = new World.Vector2D[] { new World.Vector2D(1.0, 1.0), new World.Vector2D(0.0, 1.0), new World.Vector2D(0.0, 0.0), new World.Vector2D(1.0, 0.0) };
				t[4] = new World.Vector2D[] { new World.Vector2D(0.0, 1.0), new World.Vector2D(0.0, 0.0), new World.Vector2D(1.0, 0.0), new World.Vector2D(1.0, 1.0) };
				t[5] = new World.Vector2D[] { new World.Vector2D(0.0, 1.0), new World.Vector2D(0.0, 0.0), new World.Vector2D(1.0, 0.0), new World.Vector2D(1.0, 1.0) };
				for (int i = 0; i < 6; i++) {
					Gl.glBegin(Gl.GL_QUADS);
					Gl.glColor3d(1.0, 1.0, 1.0);
					for (int j = 0; j < 4; j++) {
						Gl.glTexCoord2d(t[i][j].X, t[i][j].Y);
						Gl.glVertex3d(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
					}
					Gl.glEnd();
				}
			} else {
				for (int i = 0; i < 6; i++) {
					Gl.glBegin(Gl.GL_QUADS);
					Gl.glColor3d(1.0, 1.0, 1.0);
					for (int j = 0; j < 4; j++) {
						Gl.glVertex3d(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
					}
					Gl.glEnd();
				}
			}
		}

		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glEnable(Gl.GL_BLEND);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			Gl.glOrtho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// marker
			if (OptionInterface) {
				double y = 128.0;
				for (int i = 0; i < Game.MarkerTextures.Length; i++) {
					int t = TextureManager.UseTexture(Game.MarkerTextures[i], TextureManager.UseMode.LoadImmediately);
					if (t >= 0) {
						double w = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipWidth;
						double h = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipHeight;
						Gl.glColor4d(1.0, 1.0, 1.0, 1.0);
						RenderOverlayTexture(Game.MarkerTextures[i], (double)ScreenWidth - w - 8.0, y, (double)ScreenWidth - 8.0, y + h);
						y += h + 8.0;
					}
				}
			}
			// render
			if (Program.CurrentlyLoading) {
				RenderString(4.0, 4.0, Fonts.FontType.Small, "Loading...", -1, 1.0f, 1.0f, 1.0f, true);
			} else {
				if (ObjectManager.ObjectsUsed == 0) {
					string[][] Keys;
					Keys = new string[][] { new string[] { "F7" } };
					RenderKeys(4.0, 4.0, 24.0, Keys);
					RenderString(32.0, 4.0, Fonts.FontType.Small, "Open route", -1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 8.0, (double)ScreenHeight - 20.0, Fonts.FontType.Small, "v" + System.Windows.Forms.Application.ProductVersion, 1, 1.0f, 1.0f, 1.0f, true);
				} else if (OptionInterface) {
					// keys
					string[][] Keys;
					Keys = new string[][] { new string[] { "F5" }, new string[] { "F7" } };
					RenderKeys(4.0, 4.0, 24.0, Keys);
					RenderString(32.0, 4.0, Fonts.FontType.Small, "Reload route", -1, 1.0f, 1.0f, 1.0f, true);
					RenderString(32.0, 24.0, Fonts.FontType.Small, "Open route", -1, 1.0f, 1.0f, 1.0f, true);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "E" }, new string[] { "C" }, new string[] { "M" }, new string[] { "I" } };
					RenderKeys((double)ScreenWidth - 20.0, 4.0, 16.0, Keys);
					RenderString((double)ScreenWidth - 32.0, 4.0, Fonts.FontType.Small, "Wireframe: " + (Renderer.OptionWireframe ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 32.0, 24.0, Fonts.FontType.Small, "Normals: " + (Renderer.OptionNormals ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 32.0, 44.0, Fonts.FontType.Small, "Events: " + (Renderer.OptionEvents ? "on" : "off"), 1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 32.0, 64.0, Fonts.FontType.Small, "CPU: " + (Program.CpuAutomaticMode ? "auto " + (Program.CpuReducedMode ? "(low)" : "(high)") : "high"), 1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 32.0, 84.0, Fonts.FontType.Small, "Mute: " + (SoundManager.Mute ? "yes" : "no"), 1, 1.0f, 1.0f, 1.0f, true);
					RenderString((double)ScreenWidth - 32.0, 104.0, Fonts.FontType.Small, "Hide interface", 1, 1.0f, 1.0f, 1.0f, true);
					Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
					RenderKeys(4.0, (double)ScreenHeight - 40.0, 16.0, Keys);
					Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
					RenderKeys(0.5 * (double)ScreenWidth - 48.0, (double)ScreenHeight - 40.0, 16.0, Keys);
					Keys = new string[][] { new string[] { "P↑" }, new string[] { "P↓" } };
					RenderKeys(0.5 * (double)ScreenWidth + 32.0, (double)ScreenHeight - 40.0, 24.0, Keys);
					Keys = new string[][] { new string[] { null, "/", "*" }, new string[] { "7", "8", "9" }, new string[] { "4", "5", "6" }, new string[] { "1", "2", "3" }, new string[] { null, "0", "." } };
					RenderKeys((double)ScreenWidth - 60.0, (double)ScreenHeight - 100.0, 16.0, Keys);
					if (Program.JumpToPositionEnabled) {
						RenderString(4.0, 84.0, Fonts.FontType.Small, "Jump to track position:", -1, 1.0f, 1.0f, 1.0f, true);
						RenderString(4.0, 100.0, Fonts.FontType.Small, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), -1, 1.0f, 1.0f, 0.0f, true);
					}
					// info
					double x = 0.5 * (double)ScreenWidth - 256.0;
					RenderString(x, 4.0, Fonts.FontType.Small, "Position: " + GetLengthString(World.CameraCurrentAlignment.TrackPosition) + " (X=" + GetLengthString(World.CameraCurrentAlignment.Position.X) + ", Y=" + GetLengthString(World.CameraCurrentAlignment.Position.Y) + "), Orientation: (Yaw=" + (World.CameraCurrentAlignment.Yaw * 57.2957795130824).ToString("0.00", Culture) + "°, Pitch=" + (World.CameraCurrentAlignment.Pitch * 57.2957795130824).ToString("0.00", Culture) + "°, Roll=" + (World.CameraCurrentAlignment.Roll * 57.2957795130824).ToString("0.00", Culture) + "°)", -1, 1.0f, 1.0f, 1.0f, true);
					RenderString(x, 20.0, Fonts.FontType.Small, "Radius: " + GetLengthString(World.CameraTrackFollower.CurveRadius) + ", Cant: " + (1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", Culture) + " mm, Adhesion=" + (100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", Culture), -1, 1.0f, 1.0f, 1.0f, true);
					if (Program.CurrentStation >= 0) {
						System.Text.StringBuilder t = new System.Text.StringBuilder();
						t.Append(Game.Stations[Program.CurrentStation].Name);
						if (Game.Stations[Program.CurrentStation].ArrivalTime >= 0.0) {
							t.Append(", Arrival: " + GetTime(Game.Stations[Program.CurrentStation].ArrivalTime));
						}
						if (Game.Stations[Program.CurrentStation].DepartureTime >= 0.0) {
							t.Append(", Departure: " + GetTime(Game.Stations[Program.CurrentStation].DepartureTime));
						}
						if (Game.Stations[Program.CurrentStation].OpenLeftDoors & Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [L][R]");
						} else if (Game.Stations[Program.CurrentStation].OpenLeftDoors) {
							t.Append(", [L][-]");
						} else if (Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [-][R]");
						} else {
							t.Append(", [-][-]");
						}
						switch (Game.Stations[Program.CurrentStation].StopMode) {
							case Game.StationStopMode.AllStop:
								t.Append(", Stop");
								break;
							case Game.StationStopMode.AllPass:
								t.Append(", Pass");
								break;
							case Game.StationStopMode.PlayerStop:
								t.Append(", Player stops - others pass");
								break;
							case Game.StationStopMode.PlayerPass:
								t.Append(", Player passes - others stop");
								break;
						}
						if (Game.Stations[Program.CurrentStation].StationType == Game.StationType.ChangeEnds) {
							t.Append(", Change ends");
						}
						t.Append(", Ratio=").Append((100.0 * Game.Stations[Program.CurrentStation].PassengerRatio).ToString("0", Culture)).Append("%");
						RenderString(x, 36.0, Fonts.FontType.Small, t.ToString(), -1, 1.0f, 1.0f, 1.0f, true);
					}
					if (Interface.MessageCount == 1) {
						Keys = new string[][] { new string[] { "F9" } };
						RenderKeys(4.0, 52.0, 24.0, Keys);
						RenderString(32.0, 52.0, Fonts.FontType.Small, "Display the 1 message recently generated.", -1, 1.0f, 0.5f, 0.5f, true);
					} else if (Interface.MessageCount > 1) {
						Keys = new string[][] { new string[] { "F9" } };
						RenderKeys(4.0, 52.0, 24.0, Keys);
						RenderString(32.0, 52.0, Fonts.FontType.Small, "Display the " + Interface.MessageCount.ToString(Culture) + " messages recently generated.", -1, 1.0f, 0.5f, 0.5f, true);
					}
				}
			}
			// finalize
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glDisable(Gl.GL_BLEND);
		}
		private static string GetTime(double Time) {
			int h = (int)Math.Floor(Time / 3600.0);
			Time -= (double)h * 3600.0;
			int m = (int)Math.Floor(Time / 60.0);
			Time -= (double)m * 60.0;
			int s = (int)Math.Floor(Time);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
		}
		
		// get length string
		private static string GetLengthString(double Value) {
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			if (Game.RouteUnitOfLength.Length == 1 && Game.RouteUnitOfLength[0] == 1.0) {
				return Value.ToString("0.00", culture);
			} else {
				double[] values = new double[Game.RouteUnitOfLength.Length];
				for (int i = 0; i < Game.RouteUnitOfLength.Length - 1; i++) {
					values[i] = Math.Floor(Value / Game.RouteUnitOfLength[i]);
					Value -= values[i] * Game.RouteUnitOfLength[i];
				}
				values[Game.RouteUnitOfLength.Length - 1] = Value / Game.RouteUnitOfLength[Game.RouteUnitOfLength.Length - 1];
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				for (int i = 0; i < values.Length - 1; i++) {
					builder.Append(values[i].ToString(culture) + ":");
				}
				builder.Append(values[values.Length - 1].ToString("0.00", culture));
				return builder.ToString();
			}
		}

		// render keys
		private static void RenderKeys(double Left, double Top, double Width, string[][] Keys) {
			double py = Top;
			for (int y = 0; y < Keys.Length; y++) {
				double px = Left;
				for (int x = 0; x < Keys[y].Length; x++) {
					if (Keys[y][x] != null) {
						Gl.glColor4d(0.25, 0.25, 0.25, 0.5);
						RenderOverlaySolid(px - 1.0, py - 1.0, px + Width + 1.0, py + 17.0);
						Gl.glColor4d(0.75, 0.75, 0.75, 0.5);
						RenderOverlaySolid(px - 1.0, py - 1.0, px + Width - 1.0, py + 15.0);
						Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
						RenderOverlaySolid(px, py, px + Width, py + 16.0);
						RenderString(px + 2.0, py, Fonts.FontType.Small, Keys[y][x], -1, 1.0f, 1.0f, 1.0f, true);
					}
					px += Width + 4.0;
				}
				py += 20.0;
			}
		}

		// render box
		private static void RenderBox(double Left, double Top, double Width, double Height, string[] Lines) {
			Gl.glColor4d(0.8, 0.8, 0.8, 0.5);
			RenderOverlaySolid(Left - 1.0, Top - 1.0, Left + Width + 1.0, Top + Height+1.0);
			Gl.glColor4d(0.4, 0.4, 0.4, 0.5);
			RenderOverlaySolid(Left - 1.0, Top - 1.0, Left + Width - 1.0, Top + Height-1.0);
			Gl.glColor4d(0.8, 0.8, 0.8, 0.5);
			RenderOverlaySolid(Left, Top, Left + Width, Top + Height);
			for (int i = 0; i < Lines.Length; i++) {
				double y = Top + (double)i * 16.0;
				RenderString(Left + 2.0, y, Fonts.FontType.Small, Lines[i], -1, 1.0f, 1.0f, 1.0f, true);
			}
		}

		// render string
		private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, bool Shadow) {
			RenderString(PixelLeft, PixelTop, FontType, Text, Orientation, R, G, B, 1.0f, Shadow);
		}
		private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, float A, bool Shadow) {
			if (Text == null) return;
			int Font = (int)FontType;
			double c = 1;
			double x = PixelLeft;
			double y = PixelTop;
			double tw = 0.0;
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				if (a >= 0x10000) {
					i++;
				}
				Fonts.GetTextureIndex(FontType, a);
				tw += Fonts.Characters[Font][a].Width;
			}
			if (Orientation == 0) {
				x -= 0.5 * tw;
			} else if (Orientation == 1) {
				x -= tw;
			}
			for (int i = 0; i < Text.Length; i++) {
				int b = char.ConvertToUtf32(Text, i);
				if (b >= 0x10000) {
					i++;
				}
				int t = Fonts.GetTextureIndex(FontType, b);
				double w = (double)TextureManager.Textures[t].ClipWidth;
				double h = (double)TextureManager.Textures[t].ClipHeight;
				Gl.glBlendFunc(Gl.GL_ZERO, Gl.GL_ONE_MINUS_SRC_COLOR);
				Gl.glColor3f(A, A, A);
				RenderOverlayTexture(t, x, y, x + w, y + h);
				if (Shadow) {
					RenderOverlayTexture(t, x + c, y + c, x + w, y + h);
				}
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
				Gl.glColor4f(R, G, B, A);
				RenderOverlayTexture(t, x, y, x + w, y + h);
				x += Fonts.Characters[Font][b].Width;
			}
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
		}

		// render overlay texture
		private static void RenderOverlayTexture(int TextureIndex, double ax, double ay, double bx, double by) {
			double nay = (double)ScreenHeight - ay;
			double nby = (double)ScreenHeight - by;
			TextureManager.UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
			if (TextureIndex >= 0) {
				int OpenGlTextureIndex = TextureManager.Textures[TextureIndex].OpenGlTextureIndex;
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlTextureIndex);
			} else if (TexturingEnabled) {
				Gl.glDisable(Gl.GL_TEXTURE_2D);
				TexturingEnabled = false;
			}
			Gl.glBegin(Gl.GL_QUADS);
			Gl.glTexCoord2d(0.0, 1.0);
			Gl.glVertex2d(ax, nby);
			Gl.glTexCoord2d(0.0, 0.0);
			Gl.glVertex2d(ax, nay);
			Gl.glTexCoord2d(1.0, 0.0);
			Gl.glVertex2d(bx, nay);
			Gl.glTexCoord2d(1.0, 1.0);
			Gl.glVertex2d(bx, nby);
			Gl.glEnd();
		}

		// render overlay solid
		private static void RenderOverlaySolid(double ax, double ay, double bx, double by) {
			double nay = (double)ScreenHeight - ay;
			double nby = (double)ScreenHeight - by;
			if (TexturingEnabled) {
				Gl.glDisable(Gl.GL_TEXTURE_2D);
				TexturingEnabled = false;
			}
			Gl.glBegin(Gl.GL_QUADS);
			Gl.glTexCoord2d(0.0, 1.0);
			Gl.glVertex2d(ax, nby);
			Gl.glTexCoord2d(0.0, 0.0);
			Gl.glVertex2d(ax, nay);
			Gl.glTexCoord2d(1.0, 0.0);
			Gl.glVertex2d(bx, nay);
			Gl.glTexCoord2d(1.0, 1.0);
			Gl.glVertex2d(bx, nby);
			Gl.glEnd();
		}

		// readd objects
		private static void ReAddObjects() {
			Object[] List = new Object[ObjectListCount];
			for (int i = 0; i < ObjectListCount; i++) {
				List[i] = ObjectList[i];
			}
			for (int i = 0; i < List.Length; i++) {
				HideObject(List[i].ObjectIndex);
			}
			for (int i = 0; i < List.Length; i++) {
				ShowObject(List[i].ObjectIndex, List[i].Type);
			}
		}

		// show object
		internal static void ShowObject(int ObjectIndex, ObjectType Type) {
			bool Overlay = Type == ObjectType.Overlay;
			if (ObjectManager.Objects[ObjectIndex] == null) return;
			if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0) {
				if (ObjectListCount >= ObjectList.Length) {
					Array.Resize<Object>(ref ObjectList, ObjectList.Length << 1);
				}
				ObjectList[ObjectListCount].ObjectIndex = ObjectIndex;
				ObjectList[ObjectListCount].Type = Type;

				int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
				ObjectList[ObjectListCount].FaceListIndices = new int[f];
				for (int i = 0; i < f; i++) {
					if (Overlay) {
						/// overlay
						if (OverlayListCount >= OverlayList.Length) {
							Array.Resize(ref OverlayList, OverlayList.Length << 1);
							Array.Resize(ref OverlayListDistance, OverlayList.Length);
						}
						OverlayList[OverlayListCount].ObjectIndex = ObjectIndex;
						OverlayList[OverlayListCount].FaceIndex = i;
						OverlayList[OverlayListCount].ObjectListIndex = ObjectListCount;
						ObjectList[ObjectListCount].FaceListIndices[i] = (OverlayListCount << 2) + 3;
						OverlayListCount++;
					} else {
						int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
						bool transparentcolor = false, alpha = false;
						if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255) {
							alpha = true;
						} else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == World.MeshMaterialBlendMode.Additive) {
							alpha = true;
						} else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0) {
							alpha = true;
						} else {
							int tday = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTextureIndex;
							if (tday >= 0) {
								TextureManager.UseTexture(tday, TextureManager.UseMode.Normal);
								if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
									alpha = true;
								} else if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
									transparentcolor = true;
								}
							}
							int tnight = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTextureIndex;
							if (tnight >= 0) {
								TextureManager.UseTexture(tnight, TextureManager.UseMode.Normal);
								if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
									alpha = true;
								} else if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
									transparentcolor = true;
								}
							}
						}
						if (alpha) {
							/// alpha
							if (AlphaListCount >= AlphaList.Length) {
								Array.Resize(ref AlphaList, AlphaList.Length << 1);
								Array.Resize(ref AlphaListDistance, AlphaList.Length);
							}
							AlphaList[AlphaListCount].ObjectIndex = ObjectIndex;
							AlphaList[AlphaListCount].FaceIndex = i;
							AlphaList[AlphaListCount].ObjectListIndex = ObjectListCount;
							ObjectList[ObjectListCount].FaceListIndices[i] = (AlphaListCount << 2) + 2;
							AlphaListCount++;
						} else if (transparentcolor) {
							/// transparent color
							if (TransparentColorListCount >= TransparentColorList.Length) {
								Array.Resize(ref TransparentColorList, TransparentColorList.Length << 1);
								Array.Resize(ref TransparentColorListDistance, TransparentColorList.Length);
							}
							TransparentColorList[TransparentColorListCount].ObjectIndex = ObjectIndex;
							TransparentColorList[TransparentColorListCount].FaceIndex = i;
							TransparentColorList[TransparentColorListCount].ObjectListIndex = ObjectListCount;
							ObjectList[ObjectListCount].FaceListIndices[i] = (TransparentColorListCount << 2) + 1;
							TransparentColorListCount++;
						} else {
							/// opaque
							if (OpaqueListCount >= OpaqueList.Length) {
								Array.Resize(ref OpaqueList, OpaqueList.Length << 1);
							}
							OpaqueList[OpaqueListCount].ObjectIndex = ObjectIndex;
							OpaqueList[OpaqueListCount].FaceIndex = i;
							OpaqueList[OpaqueListCount].ObjectListIndex = ObjectListCount;
							ObjectList[ObjectListCount].FaceListIndices[i] = OpaqueListCount << 2;
							OpaqueListCount++;
						}
					}
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectListCount + 1;
				ObjectListCount++;
			}
		}

		// hide object
		internal static void HideObject(int ObjectIndex) {
			if (ObjectManager.Objects[ObjectIndex] == null) return;
			int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
			if (k >= 0) {
				// remove faces
				for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
					int h = ObjectList[k].FaceListIndices[i];
					int hi = h >> 2;
					switch (h & 3) {
						case 0:
							/// opaque
							OpaqueList[hi] = OpaqueList[OpaqueListCount - 1];
							OpaqueListCount--;
							ObjectList[OpaqueList[hi].ObjectListIndex].FaceListIndices[OpaqueList[hi].FaceIndex] = h;
							break;
						case 1:
							/// transparent color
							TransparentColorList[hi] = TransparentColorList[TransparentColorListCount - 1];
							TransparentColorListCount--;
							ObjectList[TransparentColorList[hi].ObjectListIndex].FaceListIndices[TransparentColorList[hi].FaceIndex] = h;
							break;
						case 2:
							/// alpha
							AlphaList[hi] = AlphaList[AlphaListCount - 1];
							AlphaListCount--;
							ObjectList[AlphaList[hi].ObjectListIndex].FaceListIndices[AlphaList[hi].FaceIndex] = h;
							break;
						case 3:
							/// overlay
							OverlayList[hi] = OverlayList[OverlayListCount - 1];
							OverlayListCount--;
							ObjectList[OverlayList[hi].ObjectListIndex].FaceListIndices[OverlayList[hi].FaceIndex] = h;
							break;
					}
				}
				// remove object
				if (k == ObjectListCount - 1) {
					ObjectListCount--;
				} else {
					ObjectList[k] = ObjectList[ObjectListCount - 1];
					ObjectListCount--;
					for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
						int h = ObjectList[k].FaceListIndices[i];
						int hi = h >> 2;
						switch (h & 3) {
							case 0:
								OpaqueList[hi].ObjectListIndex = k;
								break;
							case 1:
								TransparentColorList[hi].ObjectListIndex = k;
								break;
							case 2:
								AlphaList[hi].ObjectListIndex = k;
								break;
							case 3:
								OverlayList[hi].ObjectListIndex = k;
								break;
						}
					}
					ObjectManager.Objects[ObjectList[k].ObjectIndex].RendererIndex = k + 1;
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
			}
		}

		// sort polygons
		private static void SortPolygons(ObjectFace[] List, int ListCount, double[] ListDistance, int ListOffset, double TimeElapsed) {
			// calculate distance
			double cx = World.AbsoluteCameraPosition.X;
			double cy = World.AbsoluteCameraPosition.Y;
			double cz = World.AbsoluteCameraPosition.Z;
			for (int i = 0; i < ListCount; i++) {
				int o = List[i].ObjectIndex;
				int f = List[i].FaceIndex;
				if (ObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3) {
					int v0 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
					int v1 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
					int v2 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
					double v0x = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
					double v0y = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
					double v0z = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
					double v1x = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
					double v1y = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
					double v1z = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
					double v2x = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
					double v2y = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
					double v2z = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
					double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
					double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
					double dx = -w1z * w2y + w1y * w2z;
					double dy = w1z * w2x - w1x * w2z;
					double dz = -w1y * w2x + w1x * w2y;
					double t = dx * dx + dy * dy + dz * dz;
					if (t != 0.0) {
						t = 1.0 / Math.Sqrt(t);
						dx *= t; dy *= t; dz *= t;
						double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
						t = dx * w0x + dy * w0y + dz * w0z;
						ListDistance[i] = -t * t;
					}
				}
			}
			// sort
			Array.Sort<double, ObjectFace>(ListDistance, List, 0, ListCount);
			// update object list
			for (int i = 0; i < ListCount; i++) {
				ObjectList[List[i].ObjectListIndex].FaceListIndices[List[i].FaceIndex] = (i << 2) + ListOffset;
			}
		}

		// get distance factor
		private static double GetDistanceFactor(World.Vertex[] Vertices, ref World.MeshFace Face, ushort GlowAttenuationData, double CameraX, double CameraY, double CameraZ) {
			if (Face.Vertices.Length != 0) {
				World.GlowAttenuationMode mode; double halfdistance;
				World.SplitGlowAttenuationData(GlowAttenuationData, out mode, out halfdistance);
				int i = (int)Face.Vertices[0].Index;
				double dx = Vertices[i].Coordinates.X - CameraX;
				double dy = Vertices[i].Coordinates.Y - CameraY;
				double dz = Vertices[i].Coordinates.Z - CameraZ;
				switch (mode) {
						case World.GlowAttenuationMode.DivisionExponent2: {
							double t = dx * dx + dy * dy + dz * dz;
							return t / (t + halfdistance * halfdistance);
						}
						case World.GlowAttenuationMode.DivisionExponent4: {
							double t = dx * dx + dy * dy + dz * dz;
							t *= t; halfdistance *= halfdistance;
							return t / (t + halfdistance * halfdistance);
						}
					default:
						return 1.0;
				}
			} else {
				return 1.0;
			}
		}

	}
}