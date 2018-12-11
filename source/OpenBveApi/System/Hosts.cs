using System.Drawing;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using OpenBveApi.World;

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
	
	/// <summary>Represents the host application and functionality it exposes.</summary>
	public abstract class HostInterface {

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

		/// <summary>Loads the specified texture wrap mode into openGL memory</summary>
		/// <param name="texture">The texture</param>
		/// <param name="wrap">The wrap mode</param>
		/// <returns>Whether loading the wrap mode into openGL was successful</returns>
		public virtual bool LoadTexture(Textures.Texture texture, OpenGlTextureWrapMode wrap)
		{
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
		/// <param name="bitmap">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Bitmap bitmap, TextureParameters parameters, out Textures.Texture handle) {
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
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Sounds.Sound sound, out SoundHandle handle) {
			handle = null;
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
		public virtual void ExecuteFunctionScript(FunctionScripting.FunctionScript functionScript, Train train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState) { }

		/// <summary>Uses callback functions to create a static object within the world</summary>
		/// <param name="Prototype">The prototype static object</param>
		/// <param name="Position">The absolute world position</param>
		/// <param name="BaseTransformation">The base transformation</param>
		/// <param name="AuxTransformation">The auxiliary translation</param>
		/// <param name="AccurateObjectDisposal">Whether accurate object disposal is used</param>
		/// <param name="AccurateObjectDisposalZOffset">The Z-offset</param>
		/// <param name="StartingDistance">The track-based starting distance for visibility</param>
		/// <param name="EndingDistance">The track-based ending distance for visibility</param>
		/// <param name="BlockLength">The block length</param>
		/// <param name="TrackPosition">The absolute track position of the center of the object</param>
		/// <param name="Brightness">The brightness to apply</param>
		/// <param name="DuplicateMaterials">Whether materials should be duplicated (Used when animations may transform the cloned object)</param>
		public virtual void CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) { }

	}
	
}
