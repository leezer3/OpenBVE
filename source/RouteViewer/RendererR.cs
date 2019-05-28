// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Route Viewer                            ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using Vector2 = OpenBveApi.Math.Vector2;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;


namespace OpenBve {
	internal static partial class Renderer {

		// screen (output window)
		internal static int ScreenWidth = 960;
		internal static int ScreenHeight = 600;

		// first frame behavior
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;

		private struct Object {
			internal int ObjectIndex;
			internal int[] FaceListIndices;
			internal ObjectType Type;
		}
		private static Object[] ObjectList = new Object[256];
		private static int ObjectListCount = 0;

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

		//Stats
		internal static bool RenderStatsOverlay = true;
		
		// current opengl data
		internal static bool TransparentColorDepthSorting = false;

		// textures
		private static Texture BackgroundChangeTexture = null;
		private static Texture BrightnessChangeTexture = null;
		private static Texture TransponderTexture = null;
		private static Texture SectionTexture = null;
		private static Texture LimitTexture = null;
		private static Texture StationStartTexture = null;
		private static Texture StationEndTexture = null;
		private static Texture StopTexture = null;
		private static Texture BufferTexture = null;
		private static Texture SoundTexture = null;
		private static Texture PointSoundTexture = null;

		// options
		internal static bool OptionLighting = true;
		internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
		internal static Color24 OptionDiffuseColor = new Color24(160, 160, 160);
		internal static Vector3 OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
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
			OptionAmbientColor = new Color24(160, 160, 160);
			OptionDiffuseColor = new Color24(160, 160, 160);
			OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
			OptionLightingResultingAmount = 1.0f;
			GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
		}

		// initialize
		internal static void Initialize() {
			LibRender.Renderer.Initialize();
			string Folder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "section.png"), out SectionTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "stop.png"), out StopTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "switchsound.png"), out PointSoundTexture);
			TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality& Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
		}

		// initialize lighting
		internal static void InitializeLighting() {
			if (OptionAmbientColor.R == 255 & OptionAmbientColor.G == 255 & OptionAmbientColor.B == 255 & OptionDiffuseColor.R == 0 & OptionDiffuseColor.G == 0 & OptionDiffuseColor.B == 0) {
				OptionLighting = false;
			} else {
				OptionLighting = true;
			}
			if (OptionLighting) {
				GL.CullFace(CullFaceMode.Front); LibRender.Renderer.CullEnabled = true;
				GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
				GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				GL.Enable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = true;
				GL.Enable(EnableCap.Light0);
				GL.Enable(EnableCap.ColorMaterial);
				GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
				GL.ShadeModel(ShadingModel.Smooth);
				OptionLightingResultingAmount = (float)((int)OptionAmbientColor.R + (int)OptionAmbientColor.G + (int)OptionAmbientColor.B) / 480.0f;
				if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
			} else {
				GL.Disable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = false;
			}
			GL.DepthFunc(DepthFunction.Lequal);
		}

		internal static void RenderScene(double TimeElapsed) {
			// initialize
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			if (OptionWireframe) {
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			} else
			{
				World.StaticBackground b = (World.StaticBackground)World.CurrentBackground;
				if (Textures.LoadTexture(b.Texture, OpenGlTextureWrapMode.RepeatRepeat))
				{
					GL.Clear(ClearBufferMask.DepthBufferBit);
				}
				else
				{
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				}
			}
			GL.PushMatrix();
			if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet) {
				LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
				ReAddObjects();
			}
			// setup camera
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			//TODO: May be required
			GL.LoadMatrix(ref lookat);
			//Glu.gluLookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			if (OptionLighting) {
				GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OptionLightPosition.X, (float)OptionLightPosition.Y, (float)OptionLightPosition.Z, 0.0f });
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
			if (LibRender.Renderer.FogEnabled) {
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			RenderBackground(TimeElapsed);
			// fog
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < World.BackgroundImageDistance) {
				if (!LibRender.Renderer.FogEnabled) {
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, Game.CurrentFog.Start);
				GL.Fog(FogParameter.FogEnd, Game.CurrentFog.End);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
				if (!LibRender.Renderer.FogEnabled) {
					GL.Enable(EnableCap.Fog); LibRender.Renderer.FogEnabled = true;
				}
				GL.ClearColor(inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f);
			} else if (LibRender.Renderer.FogEnabled) {
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			// render background
			GL.Disable(EnableCap.DepthTest);
			RenderBackground(TimeElapsed);
			// render polygons
			if (OptionLighting) {
				if (!LibRender.Renderer.LightingEnabled) {
					GL.Enable(EnableCap.Lighting);
					LibRender.Renderer.LightingEnabled = true;
				}
			} else if (LibRender.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}
			LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
			LibRender.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			LibRender.Renderer.LastBoundTexture = null;
			LibRender.Renderer.ResetOpenGlState();
            for (int i = 0; i < OpaqueListCount; i++)
            {
                RenderFace(ref OpaqueList[i], World.AbsoluteCameraPosition);
            }
            LibRender.Renderer.ResetOpenGlState();
			if(OptionEvents) RenderEvents(World.AbsoluteCameraPosition);
			LibRender.Renderer.ResetOpenGlState();
            // transparent color list
			SortPolygons(TransparentColorList, TransparentColorListCount, TransparentColorListDistance, 1, 0.0);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality) {
				
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < TransparentColorListCount; i++)
				{
					int r = (int)ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Faces[TransparentColorList[i].FaceIndex].Material;
					if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < TransparentColorListCount; i++)
				{
					int r = (int)ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Faces[TransparentColorList[i].FaceIndex].Material;
					if (ObjectManager.Objects[TransparentColorList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.AlphaTestEnabled = false;
							GL.Disable(EnableCap.AlphaTest);
							additive = true;
						}
						RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
					}
				}
			} else {
				for (int i = 0; i < TransparentColorListCount; i++) {
					RenderFace(ref TransparentColorList[i], World.AbsoluteCameraPosition);
				}
			}
			LibRender.Renderer.ResetOpenGlState();
	        GL.Enable(EnableCap.DepthTest);
	        GL.DepthMask(true);
			SortPolygons(AlphaList, AlphaListCount, AlphaListDistance, 2, 0.0);
	        if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
	        {
		        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
		        GL.DepthMask(false);
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
		        }
	        }
	        else
	        {
		        GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
		        GL.DepthMask(true);
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
			        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
			        {
				        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
				        {
					        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
				        }
			        }
		        }
		        GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
		        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
		        GL.DepthMask(false);
		        bool additive = false;
		        for (int i = 0; i < AlphaListCount; i++)
		        {
			        int r = (int)ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Faces[AlphaList[i].FaceIndex].Material;
			        if (ObjectManager.Objects[AlphaList[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
			        {
				        if (!additive)
				        {
					        LibRender.Renderer.AlphaTestEnabled = false;
					        GL.Disable(EnableCap.AlphaTest);
					        additive = true;
				        }
				        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
			        }
			        else
			        {
				        if (additive)
				        {
					        LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					        additive = false;
				        }
				        RenderFace(ref AlphaList[i], World.AbsoluteCameraPosition);
			        }
		        }
	        }
			// overlay list
			
			
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (LibRender.Renderer.FogEnabled) {
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			SortPolygons(OverlayList, OverlayListCount, OverlayListDistance, 3, TimeElapsed);
			for (int i = 0; i < OverlayListCount; i++) {
				RenderFace(ref OverlayList[i], World.AbsoluteCameraPosition);
			}
			// render overlays
			LibRender.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
			LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
			LibRender.Renderer.AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			if (LibRender.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
		}
		

		// render face
		private static void RenderFace(ref ObjectFace Face, Vector3 Camera) {
			if (LibRender.Renderer.CullEnabled) {
				if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0) {
					GL.Disable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = false;
				}
			} else if (OptionBackfaceCulling) {
				if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0) {
					GL.Enable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = true;
				}
			}
			int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			LibRender.Renderer.RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], Camera);
		}
		
		// render background
		private static void RenderBackground(double TimeElapsed) {
			// fog
			const float fogdistance = 600.0f;
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < fogdistance) {
				float cr = inv255 * (float)Game.CurrentFog.Color.R;
				float cg = inv255 * (float)Game.CurrentFog.Color.G;
				float cb = inv255 * (float)Game.CurrentFog.Color.B;
				if (!LibRender.Renderer.FogEnabled) {
					GL.Fog(FogParameter.FogMode, (int) FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, Game.CurrentFog.Start * (float)World.BackgroundImageDistance / fogdistance);
				GL.Fog(FogParameter.FogEnd, Game.CurrentFog.End * (float)World.BackgroundImageDistance / fogdistance);
				GL.Fog(FogParameter.FogColor, new float[] { cr, cg, cb, 1.0f });
				if (!LibRender.Renderer.FogEnabled) {
					GL.Enable(EnableCap.Fog); LibRender.Renderer.FogEnabled = true;
				}
			} else if (LibRender.Renderer.FogEnabled) {
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			// render
			if (World.TargetBackgroundCountdown >= 0.0) {
				// fade
				World.TargetBackgroundCountdown -= TimeElapsed;
				if (World.TargetBackgroundCountdown < 0.0) {
					World.CurrentBackground = World.TargetBackground;
					World.TargetBackgroundCountdown = -1.0;
					RenderBackground((World.StaticBackground)World.CurrentBackground, 1.0f);
				} else {
					RenderBackground((World.StaticBackground)World.CurrentBackground, 1.0f);
					LibRender.Renderer.AlphaFuncValue = 0.0f; GL.AlphaFunc(LibRender.Renderer.AlphaFuncComparison, LibRender.Renderer.AlphaFuncValue);
					float Alpha = (float)(1.0 - World.TargetBackgroundCountdown / World.TargetBackgroundDefaultCountdown);
					RenderBackground((World.StaticBackground)World.TargetBackground, Alpha);
				}
			} else {
				// single
				RenderBackground((World.StaticBackground)World.CurrentBackground, 1.0f);
			}
		}
		private static void RenderBackground(World.StaticBackground Data, float Alpha) {
			if (Data.Texture != null) {
				if (Textures.LoadTexture(Data.Texture, OpenGlTextureWrapMode.RepeatRepeat)) {
					if (LibRender.Renderer.LightingEnabled) {
						GL.Disable(EnableCap.Lighting);
						LibRender.Renderer.LightingEnabled = false;
					}
					if (!LibRender.Renderer.TexturingEnabled) {
						GL.Enable(EnableCap.Texture2D);
						LibRender.Renderer.TexturingEnabled = true;
					}
					if (Alpha == 1.0f) {
						if (LibRender.Renderer.BlendEnabled) {
							GL.Disable(EnableCap.Blend);
							LibRender.Renderer.BlendEnabled = false;
						}
					} else if (!LibRender.Renderer.BlendEnabled) {
						GL.Enable(EnableCap.Blend);
						LibRender.Renderer.BlendEnabled = true;
					}
					GL.BindTexture(TextureTarget.Texture2D, Data.Texture.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatRepeat].Name);
					GL.Color4(1.0f, 1.0f, 1.0f, Alpha);
					float y0, y1;
					if (Data.KeepAspectRatio) {
						int tw = Data.Texture.Width;
						int th = Data.Texture.Height;
						double hh = Math.PI * World.BackgroundImageDistance * (double)th / ((double)tw * (double)Data.Repetition);
						y0 = (float)(-0.5 * hh);
						y1 = (float)(1.5 * hh);
					} else {
						y0 = (float)(-0.125 * World.BackgroundImageDistance);
						y1 = (float)(0.375 * World.BackgroundImageDistance);
					}
					const int n = 32;
					Vector3[] bottom = new Vector3[n];
					Vector3[] top = new Vector3[n];
					double angleValue = 2.61799387799149 - 3.14159265358979 / (double)n;
					double angleIncrement = 6.28318530717958 / (double)n;
					for (int i = 0; i < n; i++) {
						float x = (float)(World.BackgroundImageDistance * Math.Cos(angleValue));
						float z = (float)(World.BackgroundImageDistance * Math.Sin(angleValue));
						bottom[i] = new Vector3(x, y0, z);
						top[i] = new Vector3(x, y1, z);
						angleValue += angleIncrement;
					}
					float textureStart = 0.5f * (float)Data.Repetition / (float)n;
					float textureIncrement = -(float)Data.Repetition / (float)n;
					double textureX = textureStart;
					for (int i = 0; i < n; i++) {
						int j = (i + 1) % n;
						// side wall
						GL.Begin(PrimitiveType.Quads);
						GL.TexCoord2(textureX, 0.005f);
						GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
						GL.TexCoord2(textureX, 0.995f);
						GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
						GL.TexCoord2(textureX + textureIncrement, 0.995f);
						GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
						GL.TexCoord2(textureX + textureIncrement, 0.005f);
						GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
						GL.End();
						// top cap
						GL.Begin(PrimitiveType.Triangles);
						GL.TexCoord2(textureX, 0.005f);
						GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
						GL.TexCoord2(textureX + textureIncrement, 0.005f);
						GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
						GL.TexCoord2(textureX + 0.5 * textureIncrement, 0.1f);
						GL.Vertex3(0.0f, top[i].Y, 0.0f);
						// bottom cap
						GL.TexCoord2(textureX + 0.5 * textureIncrement, 0.9f);
						GL.Vertex3(0.0f, bottom[i].Y, 0.0f);
						GL.TexCoord2(textureX + textureIncrement, 0.995f);
						GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
						GL.TexCoord2(textureX, 0.995f);
						GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
						GL.End();
						// finish
						textureX += textureIncrement;
					}
					GL.Disable(EnableCap.Texture2D);
					LibRender.Renderer.TexturingEnabled = false;
					if (!LibRender.Renderer.BlendEnabled) {
						GL.Enable(EnableCap.Blend);
						LibRender.Renderer.BlendEnabled = true;
					}
				}
			}
		}

		// render events
		private static void RenderEvents(Vector3 Camera) {
			if (TrackManager.CurrentTrack.Elements == null) {
				return;
			}
			LibRender.Renderer.LastBoundTexture = null;
			if (LibRender.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}
			if (LibRender.Renderer.AlphaTestEnabled) {
				GL.Disable(EnableCap.AlphaTest);
				LibRender.Renderer.AlphaTestEnabled = false;
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
						double s; Texture t;
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
							t = f.SoundBuffer == null ? PointSoundTexture : SoundTexture;
						} else {
							s = 0.2;
							dy = 1.0;
							t = null;
						}
						if (t != null) {
							TrackManager.TrackFollower f = new TrackManager.TrackFollower();
							f.TriggerType = TrackManager.EventTriggerType.None;
							f.TrackPosition = p;
							TrackManager.UpdateTrackFollower(ref f, p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;
							LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, t);
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
						LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, StopTexture);
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
					LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, BufferTexture);
				}
			}

			LibRender.Renderer.TexturingEnabled = true; //Set by the LibRender function
		}
		
		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)Renderer.ScreenWidth, (double)Renderer.ScreenHeight, 0.0, -1.0, 1.0);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// marker
			if (OptionInterface)
			{
				int y = 150;
				for (int i = 0; i < Game.MarkerTextures.Length; i++)
				{
					if (Textures.LoadTexture(Game.MarkerTextures[i], OpenGlTextureWrapMode.ClampClamp)) {
						int w = Game.MarkerTextures[i].Width;
						int h = Game.MarkerTextures[i].Height;
						GL.Color4(1.0, 1.0, 1.0, 1.0);
						LibRender.Renderer.DrawRectangle(Game.MarkerTextures[i], new Point(ScreenWidth - w - 8, y), new Size(w,h), null);
						y += h + 8;
					}
				}
			}
			// render
			if (!Program.CurrentlyLoading) {
				if (ObjectManager.ObjectsUsed == 0) {
					string[][] Keys = { new string[] { "F7" }, new string[] { "F8" } };
					LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32,4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(ScreenWidth - 8, ScreenHeight - 8), TextAlignment.BottomRight, Color128.White);
				} else if (OptionInterface) {
					// keys
					string[][] Keys = { new string[] { "F5" }, new string[] { "F7" }, new string[] { "F8" } };
					LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Reload route", new Point(32, 4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 44), TextAlignment.TopLeft, Color128.White, true);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "E" }, new string[] { "C" }, new string[] { "M" }, new string[] { "I" }};
					LibRender.Renderer.RenderKeys(ScreenWidth - 20, 4, 16, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Wireframe:", new Point(ScreenWidth -32, 4), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Normals:", new Point(ScreenWidth - 32, 24), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Events:", new Point(ScreenWidth - 32, 44), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "CPU:", new Point(ScreenWidth - 32, 64), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Mute:", new Point(ScreenWidth - 32, 84), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Hide interface:", new Point(ScreenWidth - 32, 104), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, (RenderStatsOverlay ? "Hide" : "Show") + " renderer statistics", new Point(ScreenWidth - 32, 124), TextAlignment.TopRight, Color128.White, true);
					Keys = new string[][] { new string[] { "F10" } };
					LibRender.Renderer.RenderKeys(ScreenWidth - 32, 124, 30, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
					LibRender.Renderer.RenderKeys(4, ScreenHeight - 40, 16, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
					LibRender.Renderer.RenderKeys(0 * ScreenWidth - 48, ScreenHeight - 40, 16, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { "P↑" }, new string[] { "P↓" } };
					LibRender.Renderer.RenderKeys((int)(0.5 * ScreenWidth + 32), ScreenHeight - 40, 24, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "/", "*" }, new string[] { "7", "8", "9" }, new string[] { "4", "5", "6" }, new string[] { "1", "2", "3" }, new string[] { null, "0", "." } };
					LibRender.Renderer.RenderKeys(ScreenWidth - 60, ScreenHeight - 100, 16, Fonts.SmallFont, Keys);
					if (Program.JumpToPositionEnabled) {
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Jump to track position:", new Point(4, 80),TextAlignment.TopLeft, Color128.White, true);
						double distance;
						if (Double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								LibRender.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								LibRender.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, distance > TrackManager.CurrentTrack.Elements[TrackManager.CurrentTrack.Elements.Length - 1].StartingTrackPosition + 100
								? Color128.Red : Color128.Yellow, true);
							}
							
						}
					}
					// info
					double x = 0.5 * (double)ScreenWidth - 256.0;
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Position: " + GetLengthString(World.CameraCurrentAlignment.TrackPosition) + " (X=" + GetLengthString(World.CameraCurrentAlignment.Position.X) + ", Y=" + GetLengthString(World.CameraCurrentAlignment.Position.Y) + "), Orientation: (Yaw=" + (World.CameraCurrentAlignment.Yaw * 57.2957795130824).ToString("0.00", Culture) + "°, Pitch=" + (World.CameraCurrentAlignment.Pitch * 57.2957795130824).ToString("0.00", Culture) + "°, Roll=" + (World.CameraCurrentAlignment.Roll * 57.2957795130824).ToString("0.00", Culture) + "°)", new Point((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Radius: " + GetLengthString(World.CameraTrackFollower.CurveRadius) + ", Cant: " + (1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", Culture) + " mm, Adhesion=" + (100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", Culture), new Point((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
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
						if (Game.Stations[Program.CurrentStation].Type == StationType.ChangeEnds) {
							t.Append(", Change ends");
						}
						t.Append(", Ratio=").Append((100.0 * Game.Stations[Program.CurrentStation].PassengerRatio).ToString("0", Culture)).Append("%");
						LibRender.Renderer.DrawString(Fonts.SmallFont, t.ToString(), new Point((int)x, 36), TextAlignment.TopLeft, Color128.White, true);
					}
					if (Interface.MessageCount == 1) {
						Keys = new string[][] { new string[] { "F9" } };
						LibRender.Renderer.RenderKeys(4, 72, 24, Fonts.SmallFont, Keys);
						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					} else if (Interface.MessageCount > 1) {
						LibRender.Renderer.RenderKeys(4, 72, 24, Fonts.SmallFont, new string[][] { new string[] { "F9" } });
						bool error = false;
						for (int i = 0; i < Interface.MessageCount; i++)
						{
							if (Interface.LogMessages[i].Type != MessageType.Information)
							{
								error = true;
								break;
							}

						}
						if (error)
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " error messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					if (RenderStatsOverlay)
					{
						LibRender.Renderer.RenderKeys(4, ScreenHeight - 126, 116, Fonts.SmallFont, new string[][] { new string[] { "Renderer Statistics" } });
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total static objects: " + ObjectManager.ObjectsUsed, new Point(4, ScreenHeight - 112), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed, new Point(4, ScreenHeight - 100), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Current framerate: " + Game.InfoFrameRate.ToString("0.0", Culture) + "fps", new Point(4, ScreenHeight - 88), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total opaque faces: " + Game.InfoStaticOpaqueFaceCount, new Point(4, ScreenHeight - 76), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total alpha faces: " + (Renderer.AlphaListCount + Renderer.TransparentColorListCount), new Point(4, ScreenHeight - 64), TextAlignment.TopLeft, Color128.White, true);
					}
				}

			}
			GL.PopMatrix();
			GL.LoadIdentity();
			// finalize
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.Disable(EnableCap.Blend);
		}
		private static string GetTime(double Time) {
			int h = (int)Math.Floor(Time / 3600.0);
			Time -= (double)h * 3600.0;
			int m = (int)Math.Floor(Time / 60.0);
			Time -= (double)m * 60.0;
			int s = (int)Math.Floor(Time);
			return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
		}

		// get length string 
		private static string GetLengthString(double Value)
		{
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			if (Game.RouteUnitOfLength.Length == 1 && Game.RouteUnitOfLength[0] == 1.0)
			{
				return Value.ToString("0.00", culture);
			}
			else
			{
				double[] values = new double[Game.RouteUnitOfLength.Length];
				for (int i = 0; i < Game.RouteUnitOfLength.Length - 1; i++)
				{
					values[i] = Math.Floor(Value/Game.RouteUnitOfLength[i]);
					Value -= values[i]*Game.RouteUnitOfLength[i];
				}
				values[Game.RouteUnitOfLength.Length - 1] = Value/Game.RouteUnitOfLength[Game.RouteUnitOfLength.Length - 1];
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				for (int i = 0; i < values.Length - 1; i++)
				{
					builder.Append(values[i].ToString(culture) + ":");
				}
				builder.Append(values[values.Length - 1].ToString("0.00", culture));
				return builder.ToString();
			}
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
		internal static void ShowObject(int ObjectIndex, ObjectType Type)
        {
            bool Overlay = Type == ObjectType.Overlay;
            if (ObjectManager.Objects[ObjectIndex] == null) return;
            if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0)
            {
                if (ObjectListCount >= ObjectList.Length)
                {
                    Array.Resize<Object>(ref ObjectList, ObjectList.Length << 1);
                }
                ObjectList[ObjectListCount].ObjectIndex = ObjectIndex;
                ObjectList[ObjectListCount].Type = Type;
                int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
                ObjectList[ObjectListCount].FaceListIndices = new int[f];
                for (int i = 0; i < f; i++)
                {
                    if (Overlay)
                    {
                        // overlay
                        if (OverlayListCount >= OverlayList.Length)
                        {
                            Array.Resize(ref OverlayList, OverlayList.Length << 1);
                            Array.Resize(ref OverlayListDistance, OverlayList.Length);
                        }
                        OverlayList[OverlayListCount].ObjectIndex = ObjectIndex;
                        OverlayList[OverlayListCount].FaceIndex = i;
                        OverlayList[OverlayListCount].ObjectListIndex = ObjectListCount;
                        ObjectList[ObjectListCount].FaceListIndices[i] = (OverlayListCount << 2) + 3;
                        OverlayListCount++;
                    }
                    else
                    {
                        int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
	                    OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
                        bool transparentcolor = false, alpha = false;
                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
                        {
                            alpha = true;
                        }
                        else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
                        {
                            alpha = true;
                        }
                        else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0)
                        {
                            alpha = true;
                        }
                        else
                        {
                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null)
                            {
	                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode == null)
	                            {
		                            
		                            // If the object does not have a stored wrapping mode, determine it now
		                            for (int v = 0; v < ObjectManager.Objects[ObjectIndex].Mesh.Vertices.Length; v++)
		                            {
			                            if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
			                                ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
			                            {
				                            wrap |= OpenGlTextureWrapMode.RepeatClamp;
			                            }
			                            if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
			                                ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
			                            {
				                            wrap |= OpenGlTextureWrapMode.ClampRepeat;
			                            }
		                            }

		                            ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode = wrap;
	                            }
	                            else
	                            {
		                            //Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
		                            wrap = (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode;
	                            }
	                            Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture, (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode);
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency == TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency == TextureTransparencyType.Partial)
                                {
									transparentcolor = true;
                                }
                            }
                            if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                            {
	                            Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture, (OpenGlTextureWrapMode)ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode);
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency == TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency == TextureTransparencyType.Partial)
                                {
                                    transparentcolor = true;
                                }
                            }
                        }
                        if (alpha)
                        {
                            // alpha
                            if (AlphaListCount >= AlphaList.Length)
                            {
                                Array.Resize(ref AlphaList, AlphaList.Length << 1);
                                Array.Resize(ref AlphaListDistance, AlphaList.Length);
                            }
							AlphaList[AlphaListCount] = new ObjectFace();
                            AlphaList[AlphaListCount].ObjectIndex = ObjectIndex;
                            AlphaList[AlphaListCount].FaceIndex = i;
                            AlphaList[AlphaListCount].ObjectListIndex = ObjectListCount;
	                        AlphaList[AlphaListCount].Wrap = wrap;
                            ObjectList[ObjectListCount].FaceListIndices[i] = (AlphaListCount << 2) + 2;
                            AlphaListCount++;
                        }
                        else if (transparentcolor)
                        {
                            // transparent color
                            if (TransparentColorListCount >= TransparentColorList.Length)
                            {
                                Array.Resize(ref TransparentColorList, TransparentColorList.Length << 1);
                                Array.Resize(ref TransparentColorListDistance, TransparentColorList.Length);
                            }
							TransparentColorList[TransparentColorListCount] = new ObjectFace();
                            TransparentColorList[TransparentColorListCount].ObjectIndex = ObjectIndex;
                            TransparentColorList[TransparentColorListCount].FaceIndex = i;
                            TransparentColorList[TransparentColorListCount].ObjectListIndex = ObjectListCount;
	                        TransparentColorList[TransparentColorListCount].Wrap = wrap;
                            ObjectList[ObjectListCount].FaceListIndices[i] = (TransparentColorListCount << 2) + 1;
                            TransparentColorListCount++;
                        }
                        else
                        {
                            // opaque
                            if (OpaqueListCount >= OpaqueList.Length)
                            {
                                Array.Resize(ref OpaqueList, OpaqueList.Length << 1);
                            }
							OpaqueList[OpaqueListCount] = new ObjectFace();
                            OpaqueList[OpaqueListCount].ObjectIndex = ObjectIndex;
                            OpaqueList[OpaqueListCount].FaceIndex = i;
                            OpaqueList[OpaqueListCount].ObjectListIndex = ObjectListCount;
	                        OpaqueList[OpaqueListCount].Wrap = wrap;
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
        internal static void HideObject(int ObjectIndex)
        {
            if (ObjectManager.Objects[ObjectIndex] == null) return;
            int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
            if (k >= 0)
            {
                // remove faces
                for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++)
                {
                    int h = ObjectList[k].FaceListIndices[i];
                    int hi = h >> 2;
                    switch (h & 3)
                    {
                        case 0:
                            // opaque
                            OpaqueList[hi] = OpaqueList[OpaqueListCount - 1];
                            OpaqueListCount--;
                            ObjectList[OpaqueList[hi].ObjectListIndex].FaceListIndices[OpaqueList[hi].FaceIndex] = h;
                            break;
                        case 1:
                            // transparent color
                            TransparentColorList[hi] = TransparentColorList[TransparentColorListCount - 1];
                            TransparentColorListCount--;
                            ObjectList[TransparentColorList[hi].ObjectListIndex].FaceListIndices[TransparentColorList[hi].FaceIndex] = h;
                            break;
                        case 2:
                            // alpha
                            AlphaList[hi] = AlphaList[AlphaListCount - 1];
                            AlphaListCount--;
                            ObjectList[AlphaList[hi].ObjectListIndex].FaceListIndices[AlphaList[hi].FaceIndex] = h;
                            break;
                        case 3:
                            // overlay
                            OverlayList[hi] = OverlayList[OverlayListCount - 1];
                            OverlayListCount--;
                            ObjectList[OverlayList[hi].ObjectListIndex].FaceListIndices[OverlayList[hi].FaceIndex] = h;
                            break;
                    }
                }
                // remove object
                if (k == ObjectListCount - 1)
                {
                    ObjectListCount--;
                }
                else
                {
                    ObjectList[k] = ObjectList[ObjectListCount - 1];
                    ObjectListCount--;
                    for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++)
                    {
                        int h = ObjectList[k].FaceListIndices[i];
                        int hi = h >> 2;
                        switch (h & 3)
                        {
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
	}
}
