#pragma warning disable 0660, 0661
using System;
using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace OpenBveApi.Objects
{
	/// <summary>Represents material properties.</summary>
	public struct MeshMaterial
	{
		/// <summary>A bit mask combining constants of the MeshMaterial structure.</summary>
		public byte Flags;
		/// <summary>The base color of the material</summary>
		public Color32 Color;
		/// <summary>The texture transparent color</summary>
		/// <remarks>Only valid if using <see cref="TextureTransparencyType.Partial"/></remarks>
		public Color24 TransparentColor;
		/// <summary>The material emissive color</summary>
		public Color24 EmissiveColor;
		/// <summary>The daytime texture</summary>
		public Textures.Texture DaytimeTexture;         //BUG: Makefile requires this to be fully qualified
		/// <summary>The night-time texture</summary>
		public Textures.Texture NighttimeTexture;
		/// <summary>A value between 0 (daytime) and 255 (nighttime).</summary>
		public byte DaytimeNighttimeBlend;
		/// <summary>The blend mode for this material</summary>
		public MeshMaterialBlendMode BlendMode;
		/// <summary>A bit mask specifying the glow properties. Use GetGlowAttenuationData to create valid data for this field.</summary>
		[CLSCompliant(false)]
		public ushort GlowAttenuationData;
		/// <summary>The mask used when determining the emissive color</summary>
		public const int EmissiveColorMask = 1;
		/// <summary>The mask used when determining the transparent color</summary>
		public const int TransparentColorMask = 2;
		/// <summary>The wrap mode, or null to allow the renderer to decide</summary>
		public OpenGlTextureWrapMode? WrapMode;

		/// <summary>Returns whether two MeshMaterial structs are equal</summary>
		public static bool operator ==(MeshMaterial A, MeshMaterial B)
		{
			if (A.Flags != B.Flags) return false;
			if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return false;
			if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return false;
			if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return false;
			if (A.DaytimeTexture != B.DaytimeTexture) return false;
			if (A.NighttimeTexture != B.NighttimeTexture) return false;
			if (A.BlendMode != B.BlendMode) return false;
			if (A.GlowAttenuationData != B.GlowAttenuationData) return false;
			if (A.WrapMode != B.WrapMode) return false;
			return true;
		}

		/// <summary>Returns whether two MeshMaterial structs are unequal</summary>
		public static bool operator !=(MeshMaterial A, MeshMaterial B)
		{
			if (A.Flags != B.Flags) return true;
			if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return true;
			if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return true;
			if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return true;
			if (A.DaytimeTexture != B.DaytimeTexture) return true;
			if (A.NighttimeTexture != B.NighttimeTexture) return true;
			if (A.BlendMode != B.BlendMode) return true;
			if (A.GlowAttenuationData != B.GlowAttenuationData) return true;
			if (A.WrapMode != B.WrapMode) return true;
			return false;
		}
	}
}
