using System;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using OpenBveApi.Interface;

namespace OpenBveApi
{
	/// <summary>Defines the base shared options to be passed to the Renderer etc.</summary>
	public abstract class BaseOptions
	{
		/// <summary>The ISO 639-1 code for the current user interface language</summary>
		public string LanguageCode;
		/// <summary>Whether the program is to be run in full-screen mode</summary>
		public bool FullscreenMode;
		/// <summary>Whether the program is to be rendered using vertical synchronisation</summary>
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
		/// <remarks>One of <see cref="AntiAliasingLevels"/>; run it through <see cref="NormalizeAntiAliasingLevel"/> when it comes from an options file or a hand-edited value</remarks>
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
		/// <summary>The speed limit for any preceding AI trains</summary>
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
		/// <summary>The maximum number of sounds playing at any one time</summary>
		public int SoundNumber;
		/// <summary>Shadow map resolution per cascade. Off disables shadows.</summary>
		public ShadowMapResolution ShadowResolution = ShadowMapResolution.Off;
		/// <summary>Maximum distance from the camera at which shadows appear.</summary>
		public ShadowDistance ShadowDrawDistance = ShadowDistance.Medium;
		/// <summary>Number of shadow cascades.</summary>
		public ShadowCascadeCount ShadowCascades = ShadowCascadeCount.Three;
		/// <summary>Shadow darkness strength. 0.0 = invisible, 1.0 = full black.</summary>
		public double ShadowStrength = 0.7;
		/// <summary>Shadow bias to prevent shadow acne.</summary>
		public double ShadowBias = 0.000005; // default synced to 0.000005
		/// <summary>Shadow normal bias in shadow-map texels (Unity-style). Typical 0.3-1.0; higher detaches shadows.</summary>
		public double ShadowNormalBias = 0.8;
		/// <summary>Whether to filter shadow casters per cascade to improve performance.</summary>
		public bool ShadowFilterCascades = true;
		/// <summary>Whether smooth (Vogel disk + IGN) shadow filtering is enabled. If false, sharp 4-tap grid is used.</summary>
		public bool ShadowSmooth = true;
		/// <summary>Shadow filter radius in texels for smooth mode (1.0 = sharper, 2.5 = softer).</summary>
		public double ShadowFilterRadius = 1.5;


		/// <summary>The sun azimuth in degrees</summary>
		public double LightAzimuth = -26.57;
		/// <summary>The sun elevation in degrees</summary>
		public double LightElevation = 60.0;
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
		/// <summary>Whether the adhesion hack is enabled</summary>
		/// <remarks>Not saved</remarks>
		public bool AdhesionHack;
		/// <summary>Enables scripted trains on BVE5 routes</summary>
		public bool EnableBve5ScriptedTrain;
		/// <summary>The scale factor for the user interface</summary>
		public int UserInterfaceScaleFactor;
		/// <summary>Whether loaded objects are automatically reloaded on change</summary>
		public bool AutoReloadObjects;
		
		/// <summary>The near clipping plane for scenery</summary>
		public double NearClipScenery = 0.5;
		/// <summary>The near clipping plane for the cab</summary>
		public double NearClipCab = 0.025;
		/// <summary>The near clipping plane for the base renderer</summary>
		public double NearClipBase = 0.2;
		/// <summary>The color used by the renderer when issuing GL.Clear()</summary>
		/// <remarks>Not saved</remarks>
		public Color24 ClearColor = new Color24(170, 170, 170);

		/// <summary>The MSAA sample counts we offer for antialiasing</summary>
		/// <remarks>Nothing above 8x is offered: consumer GPUs don't support it and the gain over 8x isn't visible anyway</remarks>
		public static readonly int[] AntiAliasingLevels = { 0, 2, 4, 8 };

		/// <summary>Snaps an arbitrary antialiasing sample count onto the nearest one we offer</summary>
		/// <param name="level">The requested sample count, which may be any value</param>
		/// <returns>One of <see cref="AntiAliasingLevels"/></returns>
		/// <remarks>Lets options files from older versions, which offered 16x, keep loading instead of falling back to no antialiasing</remarks>
		public static int NormalizeAntiAliasingLevel(int level)
		{
			//1x costs the same as turning antialiasing off without visibly helping, so treat it as off
			if (level <= 1)
			{
				return 0;
			}

			foreach (int offered in AntiAliasingLevels)
			{
				if (level <= offered)
				{
					return offered;
				}
			}

			return AntiAliasingLevels[AntiAliasingLevels.Length - 1];
		}

		/// <summary>Selects the antialiasing level in a dropdown, snapping the stored level if we no longer offer it</summary>
		/// <param name="dropdown">The dropdown to fill and select from</param>
		/// <param name="level">The sample count to select</param>
		public static void SelectAntiAliasingLevel(ComboBox dropdown, int level)
		{
			level = NormalizeAntiAliasingLevel(level);

			dropdown.Items.Clear();
			foreach (int offered in AntiAliasingLevels)
			{
				dropdown.Items.Add(offered == 0 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "quality_interpolation_antialiasing_off" }) : offered.ToString(CultureInfo.InvariantCulture));
			}

			//Items are in level order, so the index is the level's position rather than the level itself
			dropdown.SelectedIndex = Array.IndexOf(AntiAliasingLevels, level);
			if (dropdown.SelectedIndex < 0)
			{
				dropdown.SelectedIndex = 0;
			}
		}

		/// <summary>Reads the antialiasing level back out of a dropdown filled by <see cref="SelectAntiAliasingLevel"/></summary>
		/// <param name="dropdown">The dropdown to read from</param>
		public static int GetAntiAliasingLevel(ComboBox dropdown)
		{
			int index = dropdown.SelectedIndex;
			return index >= 0 && index < AntiAliasingLevels.Length ? AntiAliasingLevels[index] : 0;
		}

		/// <summary>Saves the options to the specified filename</summary>
		/// <param name="fileName">The filename to save the options to</param>
		public abstract void Save(string fileName);
	}
}
