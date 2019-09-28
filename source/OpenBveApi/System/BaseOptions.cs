using OpenBveApi.Graphics;
using OpenBveApi.Objects;

namespace OpenBveApi
{
	/// <summary>Defines the base shared options to be passed to the Renderer etc.</summary>
	public abstract class BaseOptions
	{
		/// <summary>The ISO 639-1 code for the current user interface language</summary>
		public string LanguageCode;
		/// <summary>Whether the program is to be run in full-screen mode</summary>
		public bool FullscreenMode;
		/// <summary>Whether the program is to be rendered using vertical syncronisation</summary>
		public bool VerticalSynchronization;
		/// <summary>The screen width (Windowed Mode)</summary>
		public int WindowWidth;
		/// <summary>The screen height (Windowed Mode)</summary>
		public int WindowHeight;
		/// <summary>The screen width (Fullscreen Mode)</summary>
		public int FullscreenWidth;
		/// <summary>The screen height (Fullscreen Mode)</summary>
		public int FullscreenHeight;
		/// <summary>The number of bits per pixel (Only relevant in fullscreen mode)</summary>
		public int FullscreenBits;
		/// <summary>The current pixel interpolation mode </summary>
		public InterpolationMode Interpolation;
		/// <summary>The current transparency quality mode</summary>
		public TransparencyMode TransparencyMode;
		/// <summary>The level of anisotropic filtering to be applied</summary>
		public int AnisotropicFilteringLevel;
		/// <summary>The maximum level of anisotropic filtering supported by the system</summary>
		public int AnisotropicFilteringMaximum;
		/// <summary>The level of antialiasing to be applied</summary>
		public int AntiAliasingLevel;
		/// <summary>The parser to use for Microsoft DirectX objects</summary>
		public XParsers CurrentXParser;
		/// <summary>The parser to use for Wavefront Obj objects</summary>
		public ObjParsers CurrentObjParser;
		/*
		 * Note: Object optimisation takes time whilst loading, but may increase the render performance of an
		 * object by checking for duplicate vertices etc.
		 */
		/// <summary>The minimum number of vertices for basic optimisation to be performed on an object</summary>
		public int ObjectOptimizationBasicThreshold;
		/// <summary>The minimum number of verticies for full optimisation to be performed on an object</summary>
		public int ObjectOptimizationFullThreshold;
		/// <summary>The maximum number of sounds playing at any one time</summary>
		public int SoundNumber;
		/// <summary>Whether to use the new rendering method.</summary>
		public bool IsUseNewRenderer;
	}
}
