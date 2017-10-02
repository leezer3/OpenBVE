#pragma warning disable 0659, 0661

using System;
using OpenBveApi.Colors;

namespace OpenBveApi.Textures {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	// --- structures ---
	
	/// <summary>Represents the type of transparency encountered in a texture.</summary>
	public enum TextureTransparencyType {
		/// <summary>All pixels in the texture are fully opaque.</summary>
		Opaque = 1,
		/// <summary>All pixels in the texture are either fully opaque or fully transparent.</summary>
		Partial = 2,
		/// <summary>Some pixels in the texture are neither fully opaque nor fully transparent.</summary>
		Alpha = 3
	}

	/// <summary>Represents a texture.</summary>
	public class Texture {
		// --- members ---
		/// <summary>The width of the texture in pixels.</summary>
		private readonly int MyWidth;
		/// <summary>The height of the texture in pixels.</summary>
		private readonly int MyHeight;
		/// <summary>The number of bits per pixel. Must be 32.</summary>
		private readonly int MyBitsPerPixel;
		/// <summary>The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		private readonly byte[] MyBytes;
		/// <summary>The restricted color palette for this texture, or a null reference if the texture was 24/ 32 bit originally</summary>
		private readonly Color24[] MyPalette;

		/// <summary>Whether this texture uses the compatible transparency mode (Matches to the nearest color in a restricted pallete)</summary>
		public bool CompatibleTransparencyMode;

		/// <summary>Gets the color of the given pixel</summary>
		/// <param name="X">The X-coordinate of the pixel</param>
		/// <param name="Y">The Y-coordinate of the pixel</param>
		/// <returns></returns>
		public Color24 GetPixel(int X, int Y)
		{
			if (X > MyWidth)
			{
				throw new ArgumentException("X is outside the bounds of the image");
			}
			if (Y > MyWidth)
			{
				throw new ArgumentException("Y is outside the bounds of the image");
			}
			int firstByte = 4 * X * Y;
			return new Color24(MyBytes[firstByte],MyBytes[firstByte + 1],MyBytes[firstByte + 2]);
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="bitsPerPixel">The number of bits per pixel. Must be 32.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</param>
		/// <param name="palette">The original color pallete of the image (Before upsampling to 32-bit RGBA)</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, int bitsPerPixel, byte[] bytes, Color24[] palette)
		{
		    if (bitsPerPixel != 32) {
				throw new ArgumentException("The number of bits per pixel is supported.");
			}
		    if (bytes == null) {
		        throw new ArgumentNullException("bytes");
		    }
		    if (bytes.Length != 4 * width * height) {
		        throw new ArgumentException("The data bytes are not of the expected length.");
		    }
		    this.MyWidth = width;
		    this.MyHeight = height;
		    this.MyBitsPerPixel = bitsPerPixel;
		    this.MyBytes = bytes;
			this.MyPalette = palette;
		}

	    // --- properties ---
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
		/// <summary>Gets the number of bits per pixel.</summary>
		public int BitsPerPixel {
			get {
				return this.MyBitsPerPixel;
			}
		}

		/// <summary>Gets the restricted color palette for this texture, or a null reference if not applicable</summary>
		public Color24[] Palette
		{
			get
			{
				return this.MyPalette;
			}
		}

		/// <summary>Gets the texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		public byte[] Bytes {
			get {
				return this.MyBytes;
			}
		}
		// --- operators ---
		/// <summary>Checks whether two textures are equal.</summary>
		/// <param name="a">The first texture.</param>
		/// <param name="b">The second texture.</param>
		/// <returns>Whether the two textures are equal.</returns>
		public static bool operator ==(Texture a, Texture b) {
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.MyWidth != b.MyWidth) return false;
			if (a.MyHeight != b.MyHeight) return false;
			if (a.MyBitsPerPixel != b.MyBitsPerPixel) return false;
			if (a.MyBytes.Length != b.MyBytes.Length) return false;
			for (int i = 0; i < a.MyBytes.Length; i++) {
				if (a.MyBytes[i] != b.MyBytes[i]) return false;
			}
			return true;
		}
		/// <summary>Checks whether two textures are unequal.</summary>
		/// <param name="a">The first texture.</param>
		/// <param name="b">The second texture.</param>
		/// <returns>Whether the two textures are unequal.</returns>
		public static bool operator !=(Texture a, Texture b) {
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.MyWidth != b.MyWidth) return true;
			if (a.MyHeight != b.MyHeight) return true;
			if (a.MyBitsPerPixel != b.MyBitsPerPixel) return true;
			if (a.MyBytes.Length != b.MyBytes.Length) return true;
			for (int i = 0; i < a.MyBytes.Length; i++) {
				if (a.MyBytes[i] != b.MyBytes[i]) return true;
			}
			return false;
		}
		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj) {
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is Texture)) return false;
			Texture x = (Texture)obj;
			if (this.MyWidth != x.MyWidth) return false;
			if (this.MyHeight != x.MyHeight) return false;
			if (this.MyBitsPerPixel != x.MyBitsPerPixel) return false;
			if (this.MyBytes.Length != x.MyBytes.Length) return false;
			for (int i = 0; i < this.MyBytes.Length; i++) {
				if (this.MyBytes[i] != x.MyBytes[i]) return false;
			}
			return true;
		}
		// --- functions ---
		/// <summary>Applies the specified parameters onto this texture.</summary>
		/// <param name="parameters">The parameters, or a null reference.</param>
		/// <returns>The texture with the parameters applied.</returns>
		/// <exception cref="System.ArgumentException">Raised when the clip region is outside the texture bounds.</exception>
		/// <exception cref="System.NotSupportedException">Raised when the bits per pixel in the texture is not supported.</exception>
		public Texture ApplyParameters(TextureParameters parameters) {
			return Functions.ApplyParameters(this, parameters);
		}
		/// <summary>Gets the type of transparency encountered in this texture.</summary>
		/// <returns>The type of transparency encountered in this texture.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the bits per pixel in the texture is not supported.</exception>
		public TextureTransparencyType GetTransparencyType()
		{
		    if (this.MyBitsPerPixel == 24) {
				return TextureTransparencyType.Opaque;
			}
		    if (this.MyBitsPerPixel == 32) {
		        for (int i = 3; i < this.MyBytes.Length; i += 4) {
		            if (this.MyBytes[i] != 255) {
		                for (int j = i; j < this.MyBytes.Length; j += 4) {
		                    if (this.MyBytes[j] != 0 & this.MyBytes[j] != 255) {
		                        return TextureTransparencyType.Alpha;
		                    }
		                }
		                return TextureTransparencyType.Partial;
		            }
		        }
		        return TextureTransparencyType.Opaque;
		    }
		    throw new NotSupportedException();
		}
	}
	
	
	// --- handles ---
	
	/// <summary>Represents a handle to a texture.</summary>
	public abstract class TextureHandle { }
	
	
	// --- clip region ---
	
	/// <summary>Represents a region in a texture to be extracted.</summary>
	public class TextureClipRegion {
		// --- members ---
		/// <summary>The left coordinate.</summary>
		private int MyLeft;
		/// <summary>The top coordinate.</summary>
		private int MyTop;
		/// <summary>The width.</summary>
		private int MyWidth;
		/// <summary>The height.</summary>
		private int MyHeight;
		// --- properties ---
		/// <summary>Gets the left coordinate.</summary>
		public int Left {
			get {
				return this.MyLeft;
			}
		}
		/// <summary>Gets the top coordinate.</summary>
		public int Top {
			get {
				return this.MyTop;
			}
		}
		/// <summary>Gets the width.</summary>
		public int Width {
			get {
				return this.MyWidth;
			}
		}
		/// <summary>Gets the height.</summary>
		public int Height {
			get {
				return this.MyHeight;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new clip region.</summary>
		/// <param name="left">The left coordinate.</param>
		/// <param name="top">The top coordinate.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <exception cref="System.ArgumentException">Raised when the left or top are negative.</exception>
		/// <exception cref="System.ArgumentException">Raised when the width or height are non-positive.</exception>
		public TextureClipRegion(int left, int top, int width, int height) {
			if (left < 0 | top < 0) {
				throw new ArgumentException("The left or top coordinates are negative.");
			} else if (width <= 0 | height <= 0) {
				throw new ArgumentException("The width or height are non-positive.");
			} else {
				this.MyLeft = left;
				this.MyTop = top;
				this.MyWidth = width;
				this.MyHeight = height;
			}
		}
		// --- operators ---
		/// <summary>Checks whether two clip regions are equal.</summary>
		/// <param name="a">The first clip region.</param>
		/// <param name="b">The second clip region.</param>
		/// <returns>Whether the two clip regions are equal.</returns>
		public static bool operator ==(TextureClipRegion a, TextureClipRegion b) {
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.MyLeft != b.MyLeft) return false;
			if (a.MyTop != b.MyTop) return false;
			if (a.MyWidth != b.MyWidth) return false;
			if (a.MyHeight != b.MyHeight) return false;
			return true;
		}
		/// <summary>Checks whether two clip regions are unequal.</summary>
		/// <param name="a">The first clip region.</param>
		/// <param name="b">The second clip region.</param>
		/// <returns>Whether the two clip regions are unequal.</returns>
		public static bool operator !=(TextureClipRegion a, TextureClipRegion b) {
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.MyLeft != b.MyLeft) return true;
			if (a.MyTop != b.MyTop) return true;
			if (a.MyWidth != b.MyWidth) return true;
			if (a.MyHeight != b.MyHeight) return true;
			return false;
		}
		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj) {
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is TextureClipRegion)) return false;
			TextureClipRegion x = (TextureClipRegion)obj;
			if (this.MyLeft != x.MyLeft) return false;
			if (this.MyTop != x.MyTop) return false;
			if (this.MyWidth != x.MyWidth) return false;
			if (this.MyHeight != x.MyHeight) return false;
			return true;
		}
	}
	
	
	// --- parameters ---
	
	/// <summary>Represents additional parameters that specify how to process the texture.</summary>
	public class TextureParameters {
		// --- members ---
		/// <summary>The region in the texture to be extracted, or a null reference for the entire texture.</summary>
		private readonly TextureClipRegion MyClipRegion;
		/// <summary>The color in the texture that should become transparent, or a null reference for no transparent color.</summary>
		private readonly Nullable<Color24> MyTransparentColor;
		// --- properties ---
		/// <summary>Gets the region in the texture to be extracted, or a null reference for the entire texture.</summary>
		public TextureClipRegion ClipRegion {
			get {
				return this.MyClipRegion;
			}
		}
		/// <summary>Gets the color in the texture that should become transparent, or a null reference for no transparent color.</summary>
		public Color24? TransparentColor {
			get {
				return this.MyTransparentColor;
			}
		}
		// --- constructors ---
		/// <summary>Creates new texture parameters.</summary>
		/// <param name="clipRegion">The region in the texture to be extracted, or a null reference for the entire texture.</param>
		/// <param name="transparentColor">The color in the texture that should become transparent, or a null reference for no transparent color.</param>
		public TextureParameters(TextureClipRegion clipRegion, Nullable<Color24> transparentColor) {
			this.MyClipRegion = clipRegion;
			this.MyTransparentColor = transparentColor;
		}
		// --- operators ---
		/// <summary>Checks whether two texture parameters are equal.</summary>
		/// <param name="a">The first texture parameter.</param>
		/// <param name="b">The second texture parameter.</param>
		/// <returns>Whether the two texture parameters are equal.</returns>
		public static bool operator ==(TextureParameters a, TextureParameters b) {
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.MyClipRegion != b.MyClipRegion) return false;
			if (a.MyTransparentColor != b.MyTransparentColor) return false;
			return true;
		}
		/// <summary>Checks whether two texture parameters are unequal.</summary>
		/// <param name="a">The first texture parameter.</param>
		/// <param name="b">The second texture parameter.</param>
		/// <returns>Whether the two texture parameters are unequal.</returns>
		public static bool operator !=(TextureParameters a, TextureParameters b) {
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.MyClipRegion != b.MyClipRegion) return true;
			if (a.MyTransparentColor != b.MyTransparentColor) return true;
			return false;
		}
		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj) {
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is TextureParameters)) return false;
			TextureParameters x = (TextureParameters)obj;
			if (this.MyClipRegion != x.MyClipRegion) return false;
			if (this.MyTransparentColor != x.MyTransparentColor) return false;
			return true;
		}
	}
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading textures. Plugins must implement this interface if they wish to expose textures.</summary>
	public abstract class TextureInterface {
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public virtual void Load(Hosts.HostInterface host) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public abstract bool CanLoadTexture(string path);

		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public abstract bool QueryTextureDimensions(string path, out int width, out int height);

		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public abstract bool LoadTexture(string path, out Texture texture);
		
	}
	
}
