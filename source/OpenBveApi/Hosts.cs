using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;

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
		public virtual bool RegisterTexture(string path, TextureParameters parameters, out TextureHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Textures.Texture texture, TextureParameters parameters, out TextureHandle handle) {
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
	}
	
}
