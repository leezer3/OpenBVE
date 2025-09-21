using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

namespace OpenBveApi.Hosts {
	/// <summary>Represents the host application and functionality it exposes.</summary>
	public abstract partial class HostInterface
	{

		/// <summary>Returns whether the current host application is running under Mono</summary>
		public bool MonoRuntime => Type.GetType("Mono.Runtime") != null;

		private HostPlatform cachedPlatform = (HostPlatform)99; // value not in enum

		/// <summary>Returns the current host platform</summary>
		public HostPlatform Platform
		{
			get
			{
				if ((int)cachedPlatform != 99)
				{
					return cachedPlatform;
				}

				if (Environment.OSVersion.Platform == PlatformID.Win32S | Environment.OSVersion.Platform == PlatformID.Win32Windows | Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
#if !DEBUG
// Assume that if we're running under a debugger it's *not* WINE to avoid an exception every time we launch
					try
					{
						// ReSharper disable once UnusedVariable
						var version = GetWineVersion();
						cachedPlatform = HostPlatform.WINE;
						return cachedPlatform;
					}
					catch
					{
						//ignored
					}
#endif
					cachedPlatform = HostPlatform.MicrosoftWindows;
					return cachedPlatform;
				}

				if (System.IO.File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
				{
					//Mono's platform detection doesn't reliably differentiate between OS-X and Unix
					cachedPlatform = HostPlatform.AppleOSX;
					return cachedPlatform;
				}

				string kernelName = DetectUnixKernel();

				switch (kernelName)
				{
					case "Darwin":
						cachedPlatform = HostPlatform.AppleOSX;
						break;
					case "FreeBSD":
						cachedPlatform = HostPlatform.FreeBSD;
						break;
					default:
						cachedPlatform = HostPlatform.GNULinux;
						break;
				}

				return cachedPlatform;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		private struct UName
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal readonly string sysname;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			private readonly string nodename;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			private readonly string release;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			private readonly string version;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			private readonly string machine;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
			private readonly string extraJustInCase;

		}

		/// <summary>Detects the current kernel name by p/invoking uname (libc)</summary>
		private static string DetectUnixKernel()
		{
			Debug.Flush();
			uname(out UName uts);
			return uts.sysname;
		}

		[DllImport("libc")]
		private static extern void uname(out UName uname_struct);

		[DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		// ReSharper disable once UnusedMember.Local - release only
		private static extern string GetWineVersion();

		/// <summary>The base host interface constructor</summary>
		protected HostInterface(HostApplication host)
		{
			Application = host;
			StaticObjectCache = new Dictionary<ValueTuple<string, bool, DateTime>, StaticObject>();
			AnimatedObjectCollectionCache = new Dictionary<string, AnimatedObjectCollection>();
			MissingFiles = new List<string>();
			FailedObjects = new List<string>();
			FailedTextures = new List<string>();

			if (Platform == HostPlatform.GNULinux)
            {
				/*
                 * MESA multithreading on Linux appears to be broken.
                 *
                 * This is probably a driver issue
                 *https://github.com/leezer3/OpenBVE/issues/1050
                 */
				Environment.SetEnvironmentVariable(@"mesa_glthread", "false");
			}
		}

		/// <summary></summary>
		public readonly HostApplication Application;

		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public virtual void ReportProblem(ProblemType type, string text)
		{
		}

		/// <summary>Clears the error log</summary>
		public void ClearErrors()
		{
			MissingFiles.Clear();
			FailedObjects.Clear();
			FailedTextures.Clear();

		}

		/// <summary>Contains a list of missing files encountered</summary>
		public readonly List<string> MissingFiles;
		/// <summary>Contains a list of objects which failed to load</summary>
		public readonly List<string> FailedObjects;
		/// <summary>Contains a list of textures which failed to load</summary>
		public readonly List<string> FailedTextures;

		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public virtual bool QueryTextureDimensions(string path, out int width, out int height)
		{
			width = 0;
			height = 0;
			return false;
		}

		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(string path, TextureParameters parameters, out Texture texture)
		{
			texture = null;
			return false;
		}

		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <param name="wrapMode">The openGL wrap mode</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(ref Texture texture, OpenGlTextureWrapMode wrapMode)
		{
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <param name="loadTexture">Whether the texture should also be pre-loaded</param>
		/// <param name="timeout">The timeout for loading the texture</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(string path, TextureParameters parameters, out Texture handle, bool loadTexture = false, int timeout = 1000)
		{
			handle = null;
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Texture texture, TextureParameters parameters, out Texture handle)
		{
			handle = null;
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Bitmap texture, TextureParameters parameters, out Texture handle)
		{
			handle = null;
			return false;
		}

		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool LoadSound(string path, out Sounds.Sound sound)
		{
			sound = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(string path, out SoundHandle handle)
		{
			handle = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="radius">The sound radius</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(string path, double radius, out SoundHandle handle)
		{
			handle = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Sounds.Sound sound, out SoundHandle handle)
		{
			handle = null;
			return false;
		}

		/// <summary>
		/// Add an extension to the path of the object file that is missing the extension and no file.
		/// </summary>
		/// <param name="FilePath">The absolute on-disk path to the object</param>
		/// <returns>Whether the extension could be determined</returns>
		public bool DetermineObjectExtension(ref string FilePath)
		{
			if (System.IO.File.Exists(FilePath) || Path.HasExtension(FilePath))
			{
				return true;
			}

			if (DetermineStaticObjectExtension(ref FilePath))
			{
				return true;
			}

			foreach (string extension in SupportedAnimatedObjectExtensions)
			{
				string testPath = Path.CombineFile(Path.GetDirectoryName(FilePath), $"{Path.GetFileName(FilePath)}{extension}");

				if (System.IO.File.Exists(testPath))
				{
					FilePath = testPath;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Add an extension to the path of the static object file that is missing the extension and no file.
		/// </summary>
		/// <param name="FilePath">The absolute on-disk path to the object</param>
		/// <returns>Whether the extension could be determined</returns>
		public bool DetermineStaticObjectExtension(ref string FilePath)
		{
			if (System.IO.File.Exists(FilePath) || Path.HasExtension(FilePath))
			{
				return true;
			}

			// Search in the order of .x, .csv, .b3d, etc.
			foreach (string extension in SupportedStaticObjectExtensions.OrderByDescending(x => Array.IndexOf(new[] { ".b3d", ".csv", ".x" }, x)))
			{
				string testPath = Path.CombineFile(Path.GetDirectoryName(FilePath), $"{Path.GetFileName(FilePath)}{extension}");

				if (System.IO.File.Exists(testPath))
				{
					FilePath = testPath;
					return true;
				}
			}

			return false;
		}

		/// <summary>Loads an object</summary>
		/// <param name="Path">The absolute on-disk path to the object</param>
		/// <param name="Encoding">The detected text encoding</param>
		/// <param name="Object">The handle to the object</param>
		/// <returns>Whether loading the object was successful</returns>
		public virtual bool LoadObject(string Path, System.Text.Encoding Encoding, out UnifiedObject Object)
		{
			ValueTuple<string, bool, DateTime> key = ValueTuple.Create(Path, false, System.IO.File.GetLastWriteTime(Path));

			if (StaticObjectCache.TryGetValue(key, out var staticObject))
			{
				Object = staticObject.Clone();
				return true;
			}

			if (AnimatedObjectCollectionCache.TryGetValue(Path, out var animatedObject))
			{
				Object = animatedObject.Clone();
				return true;
			}

			Object = null;
			return false;
		}

		/// <summary>Loads a static object</summary>
		/// <param name="Path">The absolute on-disk path to the object</param>
		/// <param name="Encoding">The detected text encoding</param>
		/// <param name="PreserveVertices">Whether optimization is to be performed on the object</param>
		/// <param name="Object">The handle to the object</param>
		/// <returns>Whether loading the object was successful</returns>
		/// <remarks>This will return false if an animated object is attempted to be loaded.
		/// Selecting to preserve vertices may be useful if using the object as a deformable.</remarks>
		public virtual bool LoadStaticObject(string Path, System.Text.Encoding Encoding, bool PreserveVertices, out StaticObject Object)
		{
			ValueTuple<string, bool, DateTime> key = ValueTuple.Create(Path, PreserveVertices, System.IO.File.GetLastWriteTime(Path));

			if (StaticObjectCache.TryGetValue(key, out var staticObject))
			{
				Object = (StaticObject)staticObject.Clone();
				return true;
			}

			Object = null;
			return false;
		}

		/// <summary>Executes a function script in the host application</summary>
		/// <param name="functionScript">The function script to execute</param>
		/// <param name="train">The train or a null reference</param>
		/// <param name="CarIndex">The car index</param>
		/// <param name="Position">The world position</param>
		/// <param name="TrackPosition">The linear track position</param>
		/// <param name="SectionIndex">The index to the signalling section</param>
		/// <param name="IsPartOfTrain">Whether this is part of a train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="CurrentState">The current state of the attached object</param>
		public virtual void ExecuteFunctionScript(FunctionScripting.FunctionScript functionScript, AbstractTrain train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
		}

		/// <summary>Creates a static object within the world of the host application, and returns the ObjectManager ID</summary>
		/// <param name="Prototype">The prototype (un-transformed) static object</param>
		/// <param name="Position">The world position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="LocalTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="AccurateObjectDisposalZOffset">The offset for accurate Z-disposal</param>
		/// <param name="StartingDistance">The absolute route based starting distance for the object</param>
		/// <param name="EndingDistance">The absolute route based ending distance for the object</param>
		/// <param name="TrackPosition">The absolute route based track position</param>
		/// <returns>The index to the created object, or -1 if this call fails</returns>
		public virtual int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return -1;
		}

		/// <summary>Creates a static object within the world of the host application, and returns the ObjectManager ID</summary>
		/// <param name="Prototype">The prototype (un-transformed) static object</param>
		/// <param name="Position">The world position</param>
		/// <param name="LocalTransformation">
		///     <para>The local transformation to apply in order to rotate the model</para>
		///     <para>NOTE: Only used for object disposal calcs</para>
		/// </param>
		/// <param name="Rotate">The rotation matrix to apply</param>
		/// <param name="Translate">The translation matrix to apply</param>
		/// <param name="AccurateObjectDisposalZOffset">The offset for accurate Z-disposal</param>
		/// <param name="StartingDistance">The absolute route based starting distance for the object</param>
		/// <param name="EndingDistance">The absolute route based ending distance for the object</param>
		/// <param name="TrackPosition">The absolute route based track position</param>
		/// <returns>The index to the created object, or -1 if this call fails</returns>
		public virtual int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return -1;
		}

		/// <summary>Creates a dynamic object</summary>
		/// <param name="internalObject">The internal static object to be updated</param>
		/// <returns>The index of the dynamic object</returns>
		public virtual void CreateDynamicObject(ref ObjectState internalObject)
		{

		}

		/// <summary>Adds an object with a custom timetable texture</summary>
		public virtual void AddObjectForCustomTimeTable(AnimatedObject animatedObject)
		{

		}

		/// <summary>Shows an object in the base renderer</summary>
		/// <param name="objectToShow">The reference to the object to show</param>
		/// <param name="objectType">The object type</param>
		public virtual void ShowObject(ObjectState objectToShow, ObjectType objectType)
		{

		}

		/// <summary>Hides an object in the base renderer</summary>
		/// <param name="objectToHide">The reference to the object to hide</param>
		public virtual void HideObject(ObjectState objectToHide)
		{

		}

		/// <summary>Adds a log message to the host application.</summary>
		/// <param name="type">The type of message to be reported.</param>
		/// <param name="FileNotFound">Whether this message relates to a file not found</param>
		/// <param name="text">The textual message.</param>
		public virtual void AddMessage(MessageType type, bool FileNotFound, string text)
		{
		}

		/// <summary>Adds a fully constructed message to the in-game display</summary>
		/// <param name="AbstractMessage">The message to add</param>
		public virtual void AddMessage(object AbstractMessage)
		{
			/*
			 * Using object as a parameter type allows us to keep the messages out the API...
			 */

		}

		///  <summary>Adds a message to the in-game display</summary>
		/// <param name="Message">The text of the message</param>
		///  <param name="MessageDependancy">The dependancy of the message</param>
		///  <param name="Mode">The required game mode for the message to display</param>
		///  <param name="MessageColor">The color of the message font</param>
		///  <param name="MessageTimeOut">The timeout of the message</param>
		///  <param name="Key">The mesage key</param>
		public virtual void AddMessage(string Message, object MessageDependancy, GameMode Mode, MessageColor MessageColor, double MessageTimeOut, string Key)
		{

		}

		/// <summary>Checks whether the specified sound source is playing</summary>
		public virtual bool SoundIsPlaying(object SoundSource)
		{
			return false;
		}

		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train car is specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="parent">The parent object the sound is attached to, or a null reference.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		public virtual object PlaySound(SoundHandle buffer, double pitch, double volume, Vector3 position, object parent, bool looped)
		{
			return null;
		}

		/// <summary>Register the position to play microphone input.</summary>
		/// <param name="position">The position.</param>
		/// <param name="backwardTolerance">allowed tolerance in the backward direction</param>
		/// <param name="forwardTolerance">allowed tolerance in the forward direction</param>
		public virtual void PlayMicSound(Vector3 position, double backwardTolerance, double forwardTolerance)
		{

		}

		/// <summary>Stops a playing sound source</summary>
		public virtual void StopSound(object SoundSource)
		{

		}

		/// <summary>Stops all sounds with the specified parent object</summary>
		/// <param name="parent">The parent object</param>
		public virtual void StopAllSounds(object parent)
		{

		}

		/// <summary>Returns the current simulation state</summary>
		public virtual SimulationState SimulationState => SimulationState.Running;

		/// <summary>Returns the number of animated world objects used</summary>
		public virtual int AnimatedWorldObjectsUsed
		{
			get => 0;
			// ReSharper disable once ValueParameterNotUsed
			set
			{

			}
		}

		/// <summary>Returns the array of animated world objects from the host</summary>
		public virtual WorldObject[] AnimatedWorldObjects
		{
			get => null;
			// ReSharper disable once ValueParameterNotUsed
			set
			{

			}
		}

		/// <summary>Gets or sets the tracks array within the host application</summary>
		public virtual Dictionary<int, Track> Tracks
		{
			get => null;
			// ReSharper disable once ValueParameterNotUsed
			set
			{

			}
		}

		/// <summary>Updates the custom timetable texture displayed when triggered by an event</summary>
		/// <param name="Daytime">The daytime texture</param>
		/// <param name="Nighttime">The nighttime texture</param>
		public virtual void UpdateCustomTimetable(Texture Daytime, Texture Nighttime)
		{

		}

		/// <summary>Loads a track following object via the host program</summary>
		/// <param name="objectPath">The path to the object directory</param>
		/// /// <param name="tfoFile">The TFO parameters file</param>
		/// <returns>The track following object</returns>
		public abstract AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile);

		/// <summary>The list of available content loading plugins</summary>
		public ContentLoadingPlugin[] Plugins;

		/// <summary>The total number of available route loading plugins</summary>
		public int AvailableRoutePluginCount => Plugins.Count(x => x.Route != null);

		/// <summary>The total number of available object loading plugins</summary>
		public int AvailableObjectPluginCount => Plugins.Count(x => x.Object != null);

		/// <summary>The total number of available sound loading plugins</summary>
		public int AvailableSoundPluginCount => Plugins.Count(x => x.Sound != null);

		/// <summary>
		/// Array of supported animated object extensions.
		/// </summary>
		public string[] SupportedAnimatedObjectExtensions => Plugins.Where(x => x.Object != null).SelectMany(x => x.Object.SupportedAnimatedObjectExtensions).ToArray();

		/// <summary>
		/// Array of supported static object extensions.
		/// </summary>
		public string[] SupportedStaticObjectExtensions => Plugins.Where(x => x.Object != null).SelectMany(x => x.Object.SupportedStaticObjectExtensions).ToArray();

		/// <summary>
		/// Dictionary of StaticObject with Path and PreserveVertices as keys.
		/// </summary>
		public readonly Dictionary<ValueTuple<string, bool, DateTime>, StaticObject> StaticObjectCache;

		/// <summary>
		/// Dictionary of AnimatedObjectCollection with Path as key.
		/// </summary>

		public readonly Dictionary<string, AnimatedObjectCollection> AnimatedObjectCollectionCache;

		/// <summary>Adds a marker texture to the host application's display</summary>
		/// <param name="MarkerTexture">The texture to add</param>
		/// <param name="Size">The size to draw</param>
		public virtual void AddMarker(Texture MarkerTexture, Vector2 Size)
		{

		}

		/// <summary>Removes a marker texture if present in the host application's display</summary>
		/// <param name="MarkerTexture">The texture to remove</param>
		public virtual void RemoveMarker(Texture MarkerTexture)
		{

		}

		/// <summary>Called when a follower reaches the end of the world</summary>
		public virtual void CameraAtWorldEnd()
		{

		}

		/// <summary>Gets the current in-game time</summary>
		/// <returns>The time in seconds since midnight on the first day</returns>
		public virtual double InGameTime => 0.0;

		/// <summary>Adds an entry to the in-game black box recorder</summary>
		public virtual void AddBlackBoxEntry()
		{

		}

		/// <summary>Processes a jump</summary>
		/// <param name="Train">The train to be jumped</param>
		/// <param name="StationIndex">The station to jump to</param>
		/// <param name="TrackKey">The key of the track on which the station is placed</param>
		public virtual void ProcessJump(AbstractTrain Train, int StationIndex, int TrackKey)
		{

		}

		/// <summary>May be called from a .Net plugin, in order to add a score to the post-game log</summary>
		/// <param name="Score">The score to add</param>
		/// <param name="Message">The message to display in the post-game log</param>
		/// <param name="Color">The color of the in-game message</param>
		/// <param name="Timeout">The time in seconds for which to display the in-game message</param>
		public virtual void AddScore(int Score, string Message, MessageColor Color, double Timeout)
		{

		}

		/// <summary>Returns the trains within the simulation</summary>
		public virtual IEnumerable<AbstractTrain> Trains => null;

		/// <summary>Gets the closest train to the specified train</summary>
		/// <param name="Train">The specified train</param>
		/// <returns>The closest train, or null if no other trains</returns>
		public virtual AbstractTrain ClosestTrain(AbstractTrain Train)
		{
			// NOTE: This copy of the method is used in determining plugin data only
			return null;
		}

		/// <summary>Gets the closest train to the world location</summary>
		/// <param name="worldPosition">The specified track position</param>
		/// <returns>The closest train, or null if no other trains</returns>
		public virtual AbstractTrain ClosestTrain(Vector3 worldPosition)
		{
			// NOTE: This copy of the method is used by animated objects
			return null;
		}

		/// <summary>Adds a new train</summary>
		/// <param name="ReferenceTrain">The reference train, or a null reference to add the train at the end of the queue</param>
		/// <param name="NewTrain">The new train</param>
		/// <param name="Preceedes">Whether this train preceeds or follows the reference train</param>
		public virtual void AddTrain(AbstractTrain ReferenceTrain, AbstractTrain NewTrain, bool Preceedes)
		{

		}

		/*
		 * Used for interop with the 32-bit plugin host
		 */

		/// <summary>The event raised when the 32-bit host application signals it is ready to accept connections from clients</summary>
		public static readonly EventWaitHandle Win32PluginHostReady = new EventWaitHandle(false, EventResetMode.AutoReset, @"eventHostReady");

		/// <summary>The event raised when the proxy client quits and the host should stop</summary>
		public static readonly EventWaitHandle Win32PluginHostStopped = new EventWaitHandle(false, EventResetMode.AutoReset, @"eventHostShouldStop");

		/// <summary>The base pipe address</summary>
		public const string pipeBaseAddress = @"net.pipe://localhost";

		/// <summary>Pipe name</summary>
		public const string pipeName = @"pipename";

		/// <summary>Base addresses for the hosted service.</summary>
#pragma warning disable IDE1006
		public static Uri baseAddress => new Uri(pipeBaseAddress);
#pragma warning restore IDE1006

		/// <summary>Complete address of the named pipe endpoint.</summary>
		public static Uri Win32PluginHostEndpointAddress => new Uri(pipeBaseAddress + '/' + pipeName);

		/// <summary>Contains the list of commonly used 'empty' files</summary>
		/// <remarks>These generally aren't a valid object, and should be ignored for errors</remarks>
		public static readonly string[] NullFiles =
		{
			"empty",
			"null",
			"nothing",
			"nullrail",
			"null_rail"
		};
	}
}
