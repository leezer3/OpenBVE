#pragma warning disable 0659, 0661

using System.Drawing;
using System.Drawing.Imaging;

namespace OpenBve {
	internal static partial class Textures {
		
		/*
		 * A texture origin defines where to load a texture from. This can be
		 * a path (file or directory), a System.Drawing.Bitmap, or raw data.
		 * */

		// --- texture origin ---
		
		/// <summary>Represents the origin where the texture can be loaded from.</summary>
		internal abstract class TextureOrigin {
			// --- functions ---
			/// <summary>Gets the texture from this origin.</summary>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>Whether the texture could be obtained successfully.</returns>
			internal abstract bool GetTexture(out OpenBveApi.Textures.Texture texture);
			// --- operators ---
			/// <summary>Checks whether two origins are equal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are equal.</returns>
			public static bool operator ==(TextureOrigin a, TextureOrigin b) {
				if (a is PathOrigin & b is PathOrigin) {
					return (PathOrigin)a == (PathOrigin)b;
				} else {
					return object.ReferenceEquals(a, b);
				}
			}
			/// <summary>Checks whether two origins are unequal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are unequal.</returns>
			public static bool operator !=(TextureOrigin a, TextureOrigin b) {
				if (a is PathOrigin & b is PathOrigin) {
					return (PathOrigin)a != (PathOrigin)b;
				} else {
					return !object.ReferenceEquals(a, b);
				}
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (this is PathOrigin & obj is PathOrigin) {
					return (PathOrigin)this == (PathOrigin)obj;
				} else {
					return object.ReferenceEquals(this, obj);
				}
			}
		}
		
		
		// --- path origin ---

		/// <summary>Represents a file or directory where the texture can be loaded from.</summary>
		internal class PathOrigin : TextureOrigin {
			// --- members ---
			internal string Path;
			internal OpenBveApi.Textures.TextureParameters Parameters;
			// --- constructors ---
			/// <summary>Creates a new path origin.</summary>
			/// <param name="path">The path to the texture.</param>
			/// <param name="parameters">The parameters that specify how to process the texture.</param>
			internal PathOrigin(string path, OpenBveApi.Textures.TextureParameters parameters) {
				this.Path = path;
				this.Parameters = parameters;
			}
			// --- functions ---
			/// <summary>Gets the texture from this origin.</summary>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>Whether the texture could be obtained successfully.</returns>
			internal override bool GetTexture(out OpenBveApi.Textures.Texture texture) {
				if (!Program.CurrentHost.LoadTexture(this.Path, this.Parameters, out texture)) {
					texture = null;
					return false;
				} else {
					return true;
				}
			}
			// --- operators ---
			/// <summary>Checks whether two origins are equal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are equal.</returns>
			public static bool operator ==(PathOrigin a, PathOrigin b) {
				if (object.ReferenceEquals(a, b)) return true;
				if (object.ReferenceEquals(a, null)) return false;
				if (object.ReferenceEquals(b, null)) return false;
				return a.Path == b.Path;
			}
			/// <summary>Checks whether two origins are unequal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are unequal.</returns>
			public static bool operator !=(PathOrigin a, PathOrigin b) {
				if (object.ReferenceEquals(a, b)) return false;
				if (object.ReferenceEquals(a, null)) return true;
				if (object.ReferenceEquals(b, null)) return true;
				return a.Path != b.Path;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (object.ReferenceEquals(this, obj)) return true;
				if (object.ReferenceEquals(this, null)) return false;
				if (object.ReferenceEquals(obj, null)) return false;
				if (!(obj is PathOrigin)) return false;
				return this.Path == ((PathOrigin)obj).Path;
			}
		}

		
		// --- bitmap origin ---

		/// <summary>Represents a System.Drawing.Bitmap where the texture can be loaded from.</summary>
		internal class BitmapOrigin : TextureOrigin {
			// --- members ---
			/// <summary>The bitmap.</summary>
			internal Bitmap Bitmap;
			// --- constructors ---
			/// <summary>Creates a new bitmap origin.</summary>
			/// <param name="bitmap">The bitmap.</param>
			internal BitmapOrigin(Bitmap bitmap) {
				this.Bitmap = bitmap;
			}
			// --- functions ---
			/// <summary>Gets the texture from this origin.</summary>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>Whether the texture could be obtained successfully.</returns>
			internal override bool GetTexture(out OpenBveApi.Textures.Texture texture) {
				Bitmap bitmap = this.Bitmap;
				Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
				/* 
				 * If the bitmap format is not already 32-bit BGRA,
				 * then convert it to 32-bit BGRA.
				 * */
				if (bitmap.PixelFormat != PixelFormat.Format32bppArgb) {
					Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
					Graphics graphics = Graphics.FromImage(compatibleBitmap);
					graphics.DrawImage(bitmap, rect, rect, GraphicsUnit.Pixel);
					graphics.Dispose();
					bitmap = compatibleBitmap;
				}
				/*
				 * Extract the raw bitmap data.
				 * */
				BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
				if (data.Stride == 4 * data.Width) {
					/*
					 * Copy the data from the bitmap
					 * to the array in BGRA format.
					 * */
					byte[] raw = new byte[data.Stride * data.Height];
					System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
					bitmap.UnlockBits(data);
					int width = bitmap.Width;
					int height = bitmap.Height;
					/*
					 * Change the byte order from BGRA to RGBA.
					 * */
					for (int i = 0; i < raw.Length; i += 4) {
						byte temp = raw[i];
						raw[i] = raw[i + 2];
						raw[i + 2] = temp;
					}
					texture = new OpenBveApi.Textures.Texture(width, height, 32, raw);
					return true;
				} else {
					/*
					 * The stride is invalid. This indicates that the
					 * CLI either does not implement the conversion to
					 * 32-bit BGRA correctly, or that the CLI has
					 * applied additional padding that we do not
					 * support.
					 * */
					bitmap.UnlockBits(data);
					texture = null;
					return false;
				}
			}
		}
		
		
		// --- raw origin ---

		/// <summary>Represents texture raw data.</summary>
		internal class RawOrigin : TextureOrigin {
			// --- members ---
			/// <summary>The texture raw data.</summary>
			internal OpenBveApi.Textures.Texture Texture;
			// --- constructors ---
			/// <summary>Creates a new raw data origin.</summary>
			/// <param name="texture">The texture raw data.</param>
			internal RawOrigin(OpenBveApi.Textures.Texture texture) {
				this.Texture = texture;
			}
			// --- functions ---
			/// <summary>Gets the texture from this origin.</summary>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>Whether the texture could be obtained successfully.</returns>
			internal override bool GetTexture(out OpenBveApi.Textures.Texture texture) {
				texture = this.Texture;
				return true;
			}
		}

		
	}
}