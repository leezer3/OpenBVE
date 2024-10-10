using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

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
		/// <summary>Enables / disables various hacks for BVE related content</summary>
		public bool EnableBveTsHacks;
		/// <summary>Stores whether to use fuzzy matching for transparency colors (Matches BVE2 / BVE4 behaviour)</summary>
		public bool OldTransparencyMode;
		/// <summary>The viewing distance in meters</summary>
		public int ViewingDistance;
		/// <summary>The size of a leaf when using QuadTree visibility</summary>
		public int QuadTreeLeafSize;
		/// <summary>Whether toppling is enabled</summary>
		public bool Toppling;
		/// <summary>Whether derailments are enabled</summary>
		public bool Derailments;
		/// <summary>The number 1km/h must be multiplied by to produce your desired speed units, or 0.0 to disable this</summary>
		public double SpeedConversionFactor = 0.0;
		/// <summary>The unit of speed displayed in in-game messages</summary>
		public string UnitOfSpeed = "km/h";
		/// <summary>The default mode for the train's safety system to start in</summary>
		public TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesAts;
		/// <summary>The initial destination for any train within the game</summary>
		public int InitialDestination = -1;
		/// <summary>The initial camera viewpoint</summary>
		public int InitialViewpoint = 0;
		/// <summary>The speed limit for any preceeding AI trains</summary>
		public double PrecedingTrainSpeedLimit = double.PositiveInfinity;
		/// <summary>The name of the current train</summary>
		public string TrainName = "";
		/// <summary>The current compatibility signal set</summary>
		public string CurrentCompatibilitySignalSet;
		/// <summary>Allows a forwards compatible context to be forced</summary>
		public bool ForceForwardsCompatibleContext;
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
		/// <summary>Whether debug logs should be generated</summary>
		public bool GenerateDebugLogging;
		/// <summary>Whether loading sway is added</summary>
		public bool LoadingSway;
		/// <summary>The game mode- Affects how the score is calculated</summary>
		public GameMode GameMode;
		/// <summary>Whether Panel2 is loaded using the extended touch controls mode</summary>
		public bool Panel2ExtendedMode;
		/// <summary>The minimum size for a Panel2 control to be considered touch sensitive</summary>
		public int Panel2ExtendedMinSize;
		/// <summary>Whether various accessibility helpers are enabled</summary>
		public bool Accessibility;
		/// <summary>The font to use</summary>
		public string Font;
		/// <summary>The object disposal mode in use</summary>
		/// <remarks>Not saved</remarks>
		public ObjectDisposalMode ObjectDisposalMode;
		/// <summary>Uses the native Windows GDI+ decoders for PNG / JPG</summary>
		public bool UseGDIDecoders;
		/// <summary>The filename of the current cursor</summary>
		public string CursorFileName;
		/// <summary>The download location for the train required by the current route</summary>
		public string TrainDownloadLocation = "";
		/// <summary>Whether delayed animated updates based upon track position are used</summary>
		/// <remarks>Not saved</remarks>
		public bool DelayedAnimatedUpdates;
		/// <summary>Enables scripted trains on BVE5 routes</summary>
		public bool EnableBve5ScriptedTrain;
		/// <summary>The scale factor for the user interface</summary>
		public int UserInterfaceScaleFactor;

		/// <summary>Saves the options to the specified filename</summary>
		/// <param name="fileName">The filename to save the options to</param>
		public abstract void Save(string fileName);
	}
}
