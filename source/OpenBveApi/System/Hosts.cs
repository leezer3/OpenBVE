using System.Drawing;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

namespace OpenBveApi.Hosts {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	/// <summary>Represents the type of problem that is reported to the host.</summary>
	public enum ProblemType {
		/// <summary>Indicates that a file could not be found.</summary>
		FileNotFound = 1,
		/// <summary>Indicates that a directory could not be found.</summary>
		DirectoryNotFound = 2,
		/// <summary>Indicates that a file or directory could not be found.</summary>
		PathNotFound = 3,
		/// <summary>Indicates invalid data in a file or directory.</summary>
		InvalidData = 4,
		/// <summary>Indicates an invalid operation.</summary>
		InvalidOperation = 5,
		/// <summary>Indicates an unexpected exception.</summary>
		UnexpectedException = 6
	}
	/// <summary>The host application</summary>
	public enum HostApplication
	{
		/// <summary>The main game</summary>
		OpenBve = 0,
		/// <summary>Route Viewer</summary>
		RouteViewer = 1,
		/// <summary>Object Viewer</summary>
		ObjectViewer = 2,
		/// <summary>Train Editor</summary>
		TrainEditor = 3
	}
	
	/// <summary>Represents the host application and functionality it exposes.</summary>
	public abstract class HostInterface {

		/// <summary>The base host interface constructor</summary>
		protected HostInterface(HostApplication host)
		{
			Application = host;
		}

		/// <summary></summary>
		public readonly HostApplication Application;

		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public virtual void ReportProblem(ProblemType type, string text) { }
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public virtual bool QueryTextureDimensions(string path, out int width, out int height) {
			width = 0;
			height = 0;
			return false;
		}
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(string path, TextureParameters parameters, out Textures.Texture texture) {
			texture = null;
			return false;
		}
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <param name="wrapMode">The openGL wrap mode</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(Textures.Texture texture, OpenGlTextureWrapMode wrapMode) {
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(string path, TextureParameters parameters, out Textures.Texture handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Textures.Texture texture, TextureParameters parameters, out Textures.Texture handle) {
			handle = null;
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Bitmap texture, TextureParameters parameters, out Textures.Texture handle) {
			handle = null;
			return false;
		}

		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool LoadSound(string path, out Sounds.Sound sound) {
			sound = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(string path, out SoundHandle handle) {
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
		public virtual bool RegisterSound(Sounds.Sound sound, out SoundHandle handle) {
			handle = null;
			return false;
		}

		/// <summary>Loads an object</summary>
		/// <param name="Path">The absolute on-disk path to the object</param>
		/// <param name="Encoding">The detected text encoding</param>
		/// <param name="Object">The handle to the object</param>
		/// <returns>Whether loading the object was successful</returns>
		public virtual bool LoadObject(string Path, System.Text.Encoding Encoding, out UnifiedObject Object)
		{
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
		public virtual void ExecuteFunctionScript(FunctionScripting.FunctionScript functionScript, AbstractTrain train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState) { }

		/// <summary>Creates a static object within the world of the host application, and returns the ObjectManager ID</summary>
		/// <param name="Prototype">The prototype (un-transformed) static object</param>
		/// <param name="Position">The world position</param>
		/// <param name="BaseTransformation">The base world transformation to apply</param>
		/// <param name="AuxTransformation">The secondary rail transformation to apply</param>
		/// <param name="AccurateObjectDisposal">Whether accurate object disposal is in use</param>
		/// <param name="AccurateObjectDisposalZOffset">The offset for accurate Z-disposal</param>
		/// <param name="StartingDistance">The absolute route based starting distance for the object</param>
		/// <param name="EndingDistance">The absolute route based ending distance for the object</param>
		/// <param name="BlockLength">The block length</param>
		/// <param name="TrackPosition">The absolute route based track position</param>
		/// <param name="Brightness">The brightness value at this track position</param>
		/// <returns>The index to the created object, or -1 if this call fails</returns>
		public virtual int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			return -1;
		}

		/// <summary>Creates a static object within the world of the host application, and returns the ObjectManager ID</summary>
		/// <param name="Prototype">The prototype (un-transformed) static object</param>
		/// <param name="Position">The world position</param>
		/// <param name="AuxTransformation">The secondary rail transformation to apply NOTE: Only used for object disposal calcs</param>
		/// <param name="Rotate">The rotation matrix to apply</param>
		/// <param name="Translate">The translation matrix to apply</param>
		/// <param name="AccurateObjectDisposal">Whether accurate object disposal is in use</param>
		/// <param name="AccurateObjectDisposalZOffset">The offset for accurate Z-disposal</param>
		/// <param name="StartingDistance">The absolute route based starting distance for the object</param>
		/// <param name="EndingDistance">The absolute route based ending distance for the object</param>
		/// <param name="BlockLength">The block length</param>
		/// <param name="TrackPosition">The absolute route based track position</param>
		/// <param name="Brightness">The brightness value at this track position</param>
		/// <returns>The index to the created object, or -1 if this call fails</returns>
		public virtual int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation AuxTransformation, Matrix4D Rotate, Matrix4D Translate, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
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
		public virtual void AddMessage(MessageType type, bool FileNotFound, string text) { }

		/// <summary>Adds a message to the in-game display</summary>
		/// <param name="AbstractMesage">The message to add</param>
		public virtual void AddMessage(object AbstractMesage)
		{
			/*
			 * Using object as a parameter type allows us to keep the messages out the API...
			 */

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
		public virtual object PlaySound(SoundHandle buffer, double pitch, double volume, OpenBveApi.Math.Vector3 position, object parent, bool looped)
		{
			return null;
		}

		/// <summary>Stops a playing sound source</summary>
		public virtual void StopSound(object SoundSource)
		{

		}

		/// <summary>Returns whether the simulation is currently in progress</summary>
		public virtual bool SimulationSetup
		{
			get
			{
				return false;
			}
		}

		/// <summary>Returns the number of animated world objects used</summary>
		public virtual int AnimatedWorldObjectsUsed
		{
			get
			{
				return 0;
			}
			set
			{

			}
		}

		/// <summary>Returns the array of animated world objects from the host</summary>
		public virtual WorldObject[] AnimatedWorldObjects
		{
			get
			{
				return null;
			}
			set
			{

			}
		}

		/// <summary>Gets or sets the tracks array within the host application</summary>
		public virtual Track[] Tracks
		{
			get
			{
				return null;
			}
			set
			{

			}
		}

		/// <summary>Updates the custom timetable texture displayed when triggered by an event</summary>
		/// <param name="Daytime">The daytime texture</param>
		/// <param name="Nighttime">The nighttime texture</param>
		public virtual void UpdateCustomTimetable(Textures.Texture Daytime, Textures.Texture Nighttime)
		{

		}
	}
	
}
