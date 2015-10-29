using System;

namespace OpenBveApi.Texture {
	
	// --- structures ---

	/// <summary>Represents a texture.</summary>
	public class Texture {
		// --- members ---
		/// <summary>The width of the texture in pixels.</summary>
		private int MyWidth;
		/// <summary>The height of the texture in pixels.</summary>
		private int MyHeight;
		/// <summary>The RGBA texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. All pixels take four bytes in the order red, green, blue, alpha.</summary>
		private byte[] MyBytes;
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="bytes">The RGBA texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. All pixels take four bytes in the order red, green, blue, alpha.</param>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the length of the byte array is not 4 * width * height.</exception>
		public Texture(int width, int height, byte[] bytes) {
			if (bytes != null) {
				throw new ArgumentNullException("The data bytes are a null reference.");
			} else if (4 * width * height != bytes.Length) {
				throw new ArgumentException("The data bytes are not of the expected length.");
			} else {
				this.MyWidth = width;
				this.MyHeight = height;
				this.MyBytes = bytes;
			}
		}
		// properties
		/// <summary>Gets the width of the texture in pixels.</summary>
		public int Width {
			get {
				return this.MyWidth;
			}
		}
		/// <summary>Gets the height of the texture in pixels.</summary>
		public int Height {
			get {
				return this.MyHeight;
			}
		}
		/// <summary>Gets the RGBA texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. All pixels take four bytes in the order red, green, blue, alpha.</summary>
		public byte[] Bytes {
			get {
				return this.MyBytes;
			}
		}
	}
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading textures.</summary>
	public interface ITexture {
		
		/// <summary>Is called to check whether the plugin can load the specified texture.</summary>
		/// <param name="file">The file to the texture.</param>
		/// <param name="optional">Additional information that describes how to process the file, or a null reference.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		/// <remarks>The plugin should only inspect file extensions, identifiers or headers. It should not perform a full file validation.</remarks>
		bool CanLoadTexture(string file, string optional);
		
		/// <summary>Is called to let the plugin load the specified texture.</summary>
		/// <param name="file">The file to the texture.</param>
		/// <param name="optional">Additional information that describes how to process the file, or a null reference.</param>
		/// <param name="texture">Receives the texture on success.</param>
		/// <returns>Whether the plugin succeeded in loading the texture.</returns>
		bool LoadTexture(string file, string optional, out Texture texture);
		
	}
	
}