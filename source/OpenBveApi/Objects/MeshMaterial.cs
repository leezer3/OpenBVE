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
		public MaterialFlags Flags;
		/// <summary>The base color of the material</summary>
		public Color32 Color;
		/// <summary>The texture transparent color</summary>
		/// <remarks>Only valid if using <see cref="TextureTransparencyType.Partial"/></remarks>
		public Color24 TransparentColor;
		/// <summary>The material emissive color (upgraded to Color32 to support optional RGBA; alpha defaults to 255)</summary>
		public Color32 EmissiveColor;
		/// <summary>The material specular color</summary>
		/// <remarks>Only valid if <see cref="MaterialFlags.Specular"/> is set</remarks>
		public Color24 SpecularColor;
		/// <summary>The daytime texture</summary>
		public Texture DaytimeTexture;
		/// <summary>The night-time texture</summary>
		public Texture NighttimeTexture;
		/// <summary>The lightmap texture</summary>
		public Texture LightMapTexture;
		/// <summary>The blend mode for this material</summary>
		public MeshMaterialBlendMode BlendMode;
		/// <summary>A bit mask specifying the glow properties. Use GetGlowAttenuationData to create valid data for this field.</summary>
		[CLSCompliant(false)]
		public ushort GlowAttenuationData;
		/// <summary>The wrap mode, or null to allow the renderer to decide</summary>
		public OpenGlTextureWrapMode? WrapMode;

		/// <summary>Returns whether two MeshMaterial structs are equal</summary>
		public static bool operator ==(MeshMaterial A, MeshMaterial B)
		{
			if (A.Flags != B.Flags) return false;
			if (A.Color != B.Color) return false;
			if (A.TransparentColor != B.TransparentColor) return false;
			if (A.EmissiveColor != B.EmissiveColor) return false;
			if (A.SpecularColor != B.SpecularColor) return false;
			if (A.DaytimeTexture != B.DaytimeTexture) return false;
			if (A.NighttimeTexture != B.NighttimeTexture) return false;
			if (A.LightMapTexture != B.LightMapTexture) return false;
			if (A.BlendMode != B.BlendMode) return false;
			if (A.GlowAttenuationData != B.GlowAttenuationData) return false;
			if (A.WrapMode != B.WrapMode) return false;
			return true;
		}

		/// <summary>Returns whether two MeshMaterial structs are unequal</summary>
		public static bool operator !=(MeshMaterial A, MeshMaterial B)
		{
			if (A.Flags != B.Flags) return true;
			if (A.Color != B.Color) return true;
			if (A.TransparentColor != B.TransparentColor) return true;
			if (A.EmissiveColor != B.EmissiveColor) return true;
			if (A.SpecularColor != B.SpecularColor) return true;
			if (A.DaytimeTexture != B.DaytimeTexture) return true;
			if (A.NighttimeTexture != B.NighttimeTexture) return true;
			if (A.LightMapTexture != B.LightMapTexture) return true;
			if (A.BlendMode != B.BlendMode) return true;
			if (A.GlowAttenuationData != B.GlowAttenuationData) return true;
			if (A.WrapMode != B.WrapMode) return true;
			return false;
		}
	}
}
