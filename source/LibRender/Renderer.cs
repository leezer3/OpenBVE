using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
    public static partial class Renderer
    {
		/// <summary>The callback to the host application</summary>
	    public static HostInterface currentHost;

		public static OpenGlTexture LastBoundTexture;

		public static AlphaFunction AlphaFuncComparison = 0;

		public static float AlphaFuncValue = 0.0f;

		public static float OptionLightingResultingAmount = 1.0f;

		public static bool AlphaTestEnabled = false;

		public static bool BlendEnabled = false;

		public static bool FogEnabled = false;

		public static bool LightingEnabled = false;

		/// <summary>Whether texturing is currently enabled in openGL</summary>
		public static bool TexturingEnabled = false;
		
		public static bool OptionLighting = true;

		public static bool OptionNormals = false;

		public static bool OptionWireframe = false;
		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object gdiPlusLock = new object();

		private const float inv255 = 1.0f / 255.0f;
    }
}
