using System;
using BackgroundManager;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBveShared
{
	/// <summary>The shared renderer</summary>
	public static partial class Renderer
	{
		/// <summary>Stores the current width of the screen.</summary>
		public static int Width = 0;

		/// <summary>Stores the current height of the screen.</summary>
		public static int Height = 0;

		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		public static bool Fullscreen = false;

		/// <summary>Whether the window is currently minimized</summary>
		public static bool Minimized = false;

		private static HostInterface currentHost;

		/// <summary>The current openGL viewport mode</summary>
		public static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;

		/// <summary>Used to convert 255 base (e.g. colors) to 1.0 base</summary>
		private const float inv255 = 1.0f / 255.0f;

		/// <summary>The current daytime-nighttime blend factor</summary>
		public static float OptionLightingResultingAmount = 1.0f;

		private static readonly Random RandomNumberGenerator = new Random();

		/// <summary>Whether alpha testing is currently enabled in openGL</summary>
		public static bool AlphaTestEnabled = false;

		/// <summary>The current openGL alpha function</summary>
		/// <remarks>Retains the last alpha function, even if alpha test is disabled</remarks>
		public static AlphaFunction AlphaFuncComparison = 0;

		/// <summary>The value to be compared against in openGL alpha testing</summary>
		public static float AlphaFuncValue = 0.0f;

		/// <summary>Whether blend is currently enabled in openGL</summary>
		public static bool BlendEnabled = false;

		/// <summary>Whether face culling is currently enabled in openGL</summary>
		public static bool CullEnabled = true;

		/// <summary>Whether fog is currently enabled in openGL</summary>
		public static bool FogEnabled = false;

		/// <summary>Whether texturing is currently enabled in openGL</summary>
		public static bool TexturingEnabled;

		/// <summary>Whether lighting is currently enabled in openGL</summary>
		public static bool LightingEnabled = false;

		/// <summary>Whether backface culling is currently enabled</summary>
		public static bool OptionBackfaceCulling = true;

		/// <summary>Whether lighting is currently enabled in the renderer settings</summary>
		public static bool OptionLighting;

		/// <summary>Whether the underlying vector for normals is visibly shown</summary>
		public static bool OptionNormals = false;

		/// <summary>Whether the renderer is currently running in wireframe mode</summary>
		public static bool OptionWireframe = false;

		/// <summary>The last texture bound by openGL</summary>
		public static OpenGlTexture LastBoundTexture;

		/// <summary>The current ambient lighting color</summary>
		public static Color24 OptionAmbientColor = new Color24(160, 160, 160);

		/// <summary>The current diffuse lighting color</summary>
		public static Color24 OptionDiffuseColor = new Color24(160, 160, 160);

		/// <summary>The current openGL light position</summary>
		public static Vector3 OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);

		/// <summary>The currently displayed background texture</summary>
		public static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);

		/// <summary>The new background texture (Currently fading in)</summary>
		public static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);

		public static float NoFogStart = 800.0f; // must not be 600 or below
		public static float NoFogEnd = 1600.0f;

		public static Fog PreviousFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.0);

		public static Fog CurrentFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.5);

		public static Fog NextFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 1.0);

		/// <summary>The user-selected viewing distance.</summary>
		public static double BackgroundImageDistance;

		public static RendererObject[] Objects = new RendererObject[256];

		/// <summary>
		/// The total number of objects in the simulation
		/// </summary>
		public static int ObjectCount;

		/// <summary>The list of static opaque face groups. Each group contains only objects that are associated the respective group index.</summary>
		public static ObjectGroup[] StaticOpaque = new ObjectGroup[] { };
		/// <summary>Whether to enforce updating all display lists.</summary>
		public static bool StaticOpaqueForceUpdate = true;

		public static int StaticOpaqueCount;

		// all other lists
		/// <summary>The list of dynamic opaque faces to be rendered.</summary>
		public static ObjectList DynamicOpaque = new ObjectList();
		/// <summary>The list of dynamic alpha faces to be rendered.</summary>
		public static ObjectList DynamicAlpha = new ObjectList();
		/// <summary>The list of overlay opaque faces to be rendered.</summary>
		public static ObjectList OverlayOpaque = new ObjectList();
		/// <summary>The list of overlay alpha faces to be rendered.</summary>
		public static ObjectList OverlayAlpha = new ObjectList();

		/// <summary>Initializes the renderer library</summary>
		/// <param name="host">The current host program</param>
		/// <returns>Whether initializing the renderer succeded</returns>
		public static bool Initialize(HostInterface host)
		{
			try
			{
				currentHost = host;
				GL.ShadeModel(ShadingModel.Smooth);
				GL.ClearColor(Color4.Black);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				GL.Enable(EnableCap.DepthTest);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				GL.DepthFunc(DepthFunction.Lequal);
				GL.Hint(HintTarget.FogHint, HintMode.Fastest);
				GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
				GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
				GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
				GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
				GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
				GL.Disable(EnableCap.Dither);
				GL.CullFace(CullFaceMode.Front);
				GL.Enable(EnableCap.CullFace); CullEnabled = true;
				GL.Disable(EnableCap.Lighting); LightingEnabled = false;
				GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
				Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookat);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				GL.Enable(EnableCap.Blend); BlendEnabled = true;
				GL.Disable(EnableCap.Lighting); LightingEnabled = false;
				GL.Disable(EnableCap.Fog);
			}
			catch
			{
				//We likely don't like something somewhere (Bad drivers?)....
				return false;
			}
			
			return true;
		}

		/// <summary>Initializes the lighting</summary>
		public static void InitializeLighting()
		{
			GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
			GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
			GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			GL.CullFace(CullFaceMode.Front); CullEnabled = true; // possibly undocumented, but required for correct lighting
			GL.Enable(EnableCap.Light0);
			GL.Enable(EnableCap.ColorMaterial);
			GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
			GL.ShadeModel(ShadingModel.Smooth);
			float x = (float)OptionAmbientColor.R + (float)OptionAmbientColor.G + (float)OptionAmbientColor.B;
			float y = (float)OptionDiffuseColor.R + (float)OptionDiffuseColor.G + (float)OptionDiffuseColor.B;
			if (x < y) x = y;
			OptionLightingResultingAmount = 0.00208333333333333f * x;
			if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
			GL.Enable(EnableCap.Lighting); LightingEnabled = true;
			GL.DepthFunc(DepthFunction.Lequal);
		}
	}
}
