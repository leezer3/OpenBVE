using System;
using System.Linq;
using System.ServiceModel.Security;
using OpenBveApi.Colors;

namespace OpenBveApi.Textures {
	/// <summary>Provides functions for manipulating textures.</summary>
	internal static class Functions {
		
		
		// --- apply parameters ---
		
		/// <summary>Applies parameters onto a texture.</summary>
		/// <param name="texture">The original texture.</param>
		/// <param name="parameters">The parameters, or a null reference.</param>
		/// <returns>The texture with the parameters applied.</returns>
		/// <exception cref="System.ArgumentException">Raised when the clip region is outside the texture bounds.</exception>
		/// <exception cref="System.NotSupportedException">Raised when the bits per pixel in the texture is other than 32.</exception>
		internal static Texture ApplyParameters(Texture texture, TextureParameters parameters) {
			Texture result = texture;
			if (parameters != null) {
				if (parameters.ClipRegion != null) {
					result = ExtractClipRegion(result, parameters.ClipRegion);
				}
				if (parameters.TransparentColor != null) {
					result = ApplyTransparentColor(result, parameters.TransparentColor);
				}
			}
			return result;
		}
		
		
		// --- extract clip region ---
		
		/// <summary>Extracts a clip region from a texture.</summary>
		/// <param name="texture">The original texture.</param>
		/// <param name="region">The clip region, or a null reference.</param>
		/// <returns>The texture with the extracted clip region.</returns>
		/// <exception cref="System.ArgumentException">Raised when the clip region is outside the texture bounds.</exception>
		/// <exception cref="System.NotSupportedException">Raised when the number of bits per pixel in the texture is not supported.</exception>
		internal static Texture ExtractClipRegion(Texture texture, TextureClipRegion region)
		{
		    if (region == null || region.Left == 0 && region.Top == 0 && region.Width == texture.Width && region.Height == texture.Height) {
				return texture;
			}
		    if (region.Left < 0 || region.Top < 0 || region.Width <= 0 || region.Height <= 0 || region.Left + region.Width > texture.Width || region.Top + region.Height > texture.Height) {
		        throw new ArgumentException();
		    }
		    if (texture.BitsPerPixel == 24 | texture.BitsPerPixel == 32) {
		        int width = texture.Width;
		        byte[] bytes = texture.Bytes;
		        int clipLeft = region.Left;
		        int clipTop = region.Top;
		        int clipWidth = region.Width;
		        int clipHeight = region.Height;
		        if (texture.BitsPerPixel == 24) {
		            byte[] newBytes = new byte[3 * clipWidth * clipHeight];
		            int i = 0;
		            for (int y = 0; y < clipHeight; y++) {
		                int j = 3 * width * (clipTop + y) + 3 * clipLeft;
		                for (int x = 0; x < clipWidth; x++) {
		                    newBytes[i + 0] = bytes[j + 0];
		                    newBytes[i + 1] = bytes[j + 1];
		                    newBytes[i + 2] = bytes[j + 2];
		                    i += 3;
		                    j += 3;
		                }
		            }
		            return new Texture(clipWidth, clipHeight, 24, newBytes, texture.Palette);
		        } else {
		            byte[] newBytes = new byte[4 * clipWidth * clipHeight];
		            int i = 0;
		            for (int y = 0; y < clipHeight; y++) {
		                int j = 4 * width * (clipTop + y) + 4 * clipLeft;
		                for (int x = 0; x < clipWidth; x++) {
		                    newBytes[i + 0] = bytes[j + 0];
		                    newBytes[i + 1] = bytes[j + 1];
		                    newBytes[i + 2] = bytes[j + 2];
		                    newBytes[i + 3] = bytes[j + 3];
		                    i += 4;
		                    j += 4;
		                }
		            }
		            return new Texture(clipWidth, clipHeight, 32, newBytes, texture.Palette);
		        }
		    }
		    throw new NotSupportedException();
		}

		private static Color24 GetClosestColor(Color24[] colorArray, Color24 baseColor)
		{
			if (colorArray.Contains(baseColor))
			{
				return baseColor;
			}
			int colorDiffs = colorArray.Select(n => GetDiff(n, baseColor)).Min(n => n);
			Color24 c = colorArray[Array.FindIndex(colorArray,n => GetDiff(n, baseColor) == colorDiffs)];
			return new Color24(c.R, c.G, c.B);
		}

		private static int GetDiff(Color24 c1, Color24 c2)
		{
			return (int)System.Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
			                      + (c1.G - c2.G) * (c1.G - c2.G)
			                      + (c1.B - c2.B) * (c1.B - c2.B));
		}

		// --- apply transparent color ---

		/// <summary>Applies a transparent color onto a texture.</summary>
		/// <param name="texture">The original texture.</param>
		/// <param name="color">The transparent color, or a null reference.</param>
		/// <returns>The texture with the transparent color applied.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the number of bits per pixel in the texture is not supported.</exception>
		internal static Texture ApplyTransparentColor(Texture texture, Color24? color)
		{
		    if (color == null) {
				return texture;
			}
			if (texture.Palette != null && texture.CompatibleTransparencyMode == true)
			{
				switch (texture.Palette.Length)
				{
					case 0:
						//ignore if no reduced pallette
						break;
					default:
						Color24 c = (Color24)color;
						color = GetClosestColor(texture.Palette, c);
						break;
				}
			}
		    if (texture.BitsPerPixel == 32) {
		        int width = texture.Width;
		        int height = texture.Height;
		        byte[] source = texture.Bytes;
		        byte[] target = new byte[4 * width * height];
		        byte r = color.Value.R;
		        byte g = color.Value.G;
		        byte b = color.Value.B;
		        if (source[0] == r && source[1] == g && source[2] == b) {
		            target[0] = 128;
		            target[1] = 128;
		            target[2] = 128;
		            target[3] = 0;
		        } else {
		            target[0] = source[0];
		            target[1] = source[1];
		            target[2] = source[2];
		            target[3] = source[3];
		        }
		        for (int i = 4; i < source.Length; i += 4) {
		            if (source[i] == r && source[i + 1] == g && source[i + 2] == b) {
		                target[i + 0] = target[i - 4];
		                target[i + 1] = target[i - 3];
		                target[i + 2] = target[i - 2];
		                target[i + 3] = 0;
		            } else {
		                target[i + 0] = source[i + 0];
		                target[i + 1] = source[i + 1];
		                target[i + 2] = source[i + 2];
		                target[i + 3] = source[i + 3];
		            }
		        }
		        return new Texture(width, height, 32, target, texture.Palette);
		    }
		    throw new NotSupportedException();
		}
	}
}
