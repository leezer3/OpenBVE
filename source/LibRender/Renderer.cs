using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
    public static partial class Renderer
    {
		/// <summary>The callback to the host application</summary>
	    public static HostInterface currentHost;
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
		/// <summary>Whether wireframe rendering is enabled in the debug options</summary>
		public static bool OptionWireframe = false;
		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object gdiPlusLock = new object();

		private const float inv255 = 1.0f / 255.0f;
    }
}
