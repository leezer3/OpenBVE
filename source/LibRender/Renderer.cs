using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
    public static partial class Renderer
    {
		/// <summary>The callback to the host application</summary>
	    public static HostInterface currentHost;
		/// <summary>Holds a reference to the current options</summary>
		public static BaseOptions currentOptions;
		/// <summary>A reference to the last texture bound by openGL</summary>
		public static OpenGlTexture LastBoundTexture;
		/// <summary>The current AlphaFunc comparison</summary>
		public static AlphaFunction AlphaFuncComparison = 0;
		/// <summary>The current AlphaFunc comparison value</summary>
		public static float AlphaFuncValue = 0.0f;
		/// <summary>The absolute current lighting value</summary>
		/// <remarks>0.0f represents no light, 1.0f represents full brightness</remarks>
		public static float OptionLightingResultingAmount = 1.0f;
		/// <summary>Whether Alpha Testing is enabled in openGL</summary>
		public static bool AlphaTestEnabled = false;
		/// <summary>Whether Blend is enabled in openGL</summary>
		public static bool BlendEnabled = false;
		/// <summary>Whether Cull is enabled in openGL</summary>
		public static bool CullEnabled = true;
		/// <summary>Whether Fog is enabled in openGL</summary>
		public static bool FogEnabled = false;
		/// <summary>Whether Lighting is enabled in openGL</summary>
		public static bool LightingEnabled = false;
		/// <summary>Whether Texturing is enabled in openGL</summary>
		public static bool TexturingEnabled = false;
		/// <summary>Whether lighting is enabled in the debug options</summary>
		public static bool OptionLighting = true;
		/// <summary>Whether normals rendering is enabled in the debug options</summary>
		public static bool OptionNormals = false;
		/// <summary>Whether backface culling is enabled</summary>
		public static bool OptionBackfaceCulling = true;
		/// <summary>Whether wireframe rendering is enabled in the debug options</summary>
		public static bool OptionWireframe = false;
		/// <summary>The current viewport mode</summary>
		public static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;
		/// <summary>The current debug output mode</summary>
		public static OutputMode CurrentOutputMode = OutputMode.Default;
		/// <summary>The previous debug output mode</summary>
		public static OutputMode PreviousOutputMode = OutputMode.Default;
		/// <summary>Holds the array of marker textures currently displayed in-game</summary>
		public static Texture[] MarkerTextures = { };
		/// <summary>The game's current framerate</summary>
		public static double FrameRate = 1.0;
		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object gdiPlusLock = new object();
		/// <summary>The total number of OpenGL triangles in the current frame</summary>
		public static int InfoTotalTriangles = 0;
		/// <summary>The total number of OpenGL triangle strips in the current frame</summary>
		public static int InfoTotalTriangleStrip = 0;
		/// <summary>The total number of OpenGL quad strips in the current frame</summary>
		public static int InfoTotalQuadStrip = 0;
		/// <summary>The total number of OpenGL quads in the current frame</summary>
		public static int InfoTotalQuads = 0;
		/// <summary>The total number of OpenGL polygons in the current frame</summary>
		public static int InfoTotalPolygon = 0;
		/// <summary>The total number of static opaque faces in the current frame</summary>
		public static int InfoStaticOpaqueFaceCount = 0;
		/// <summary>The list of all objects currently shown by the renderer</summary>
		public static RendererObject[] Objects = new RendererObject[256];
		/// <summary>The total number of objects in the simulation</summary>
		public static int ObjectCount;
		/// <summary>The list of static opaque face groups. Each group contains only objects that are associated the respective group index.</summary>
		public static ObjectGroup[] StaticOpaque = new ObjectGroup[] { };
		/// <summary>The list of dynamic opaque faces to be rendered.</summary>
		public static ObjectList DynamicOpaque = new ObjectList();
		/// <summary>The list of dynamic alpha faces to be rendered.</summary>
		public static ObjectList DynamicAlpha = new ObjectList();
		/// <summary>The list of overlay opaque faces to be rendered.</summary>
		public static ObjectList OverlayOpaque = new ObjectList();
		/// <summary>The list of overlay alpha faces to be rendered.</summary>
		public static ObjectList OverlayAlpha = new ObjectList();
		/// <summary>The list of touch element's faces to be rendered.</summary>
		public static ObjectList Touch = new ObjectList();
		internal const float inv255 = 1.0f / 255.0f;
    }
}
