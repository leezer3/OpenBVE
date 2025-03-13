using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBveApi.Objects
{
	/// <summary>Helper class describing textual material properties to be applied to a 3D face</summary>
	public class Material {
		/// <summary>The base color</summary>
		public Color32 Color;
		/// <summary>The emissive color</summary>
		public Color24 EmissiveColor;
		/// <summary>The transparent color</summary>
		public Color24 TransparentColor;
		/// <summary>Describes the special properties (if any) of the material</summary>
		public MaterialFlags Flags;
		/// <summary>The absolute on-disk path to the daytime texture</summary>
		public string DaytimeTexture;
		/// <summary>The absolute on-disk path to the nighttime texture</summary>
		public string NighttimeTexture;
		/// <summary>The absolute on-disk path to the transparency texture (Used for both daytime and nighttime textures)</summary>
		public string TransparencyTexture;
		/// <summary>The absolute on-disk path to the light map texture</summary>
		public string LightMap;
		/// <summary>The blend mode to be used</summary>
		public MeshMaterialBlendMode BlendMode;
		/// <summary>The wrap mode, or a null reference if not in use</summary>
		public OpenGlTextureWrapMode? WrapMode;
		/// <summary>The glow attenuation data, as a packed ushort</summary>
		public ushort GlowAttenuationData;
		/// <summary>The text to overlay onto the texture when it is loaded</summary>
		public string Text;
		/// <summary></summary>
		public string Key;
		/// <summary>The color of the text to overlay</summary>
		public Color24 TextColor;
		/// <summary>The background color for the text</summary>
		public Color24 BackgroundColor;
		/// <summary>The font to use for the text</summary>
		public string Font;
		/// <summary>The text padding to apply</summary>
		public Vector2 TextPadding;

		/// <summary>Creates a new Material with default properties</summary>
		public Material() {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.TransparentColor = Color24.Black;
			this.Flags = MaterialFlags.None;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = Color24.Black;
			this.BackgroundColor = Color24.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
		}

		/// <summary>Creates a new material from the specified texture file with default properties</summary>
		/// <param name="textureFile"></param>
		public Material(string textureFile) {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.TransparentColor = Color24.Black;
			this.Flags = MaterialFlags.None;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = Color24.Black;
			this.BackgroundColor = Color24.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
			this.DaytimeTexture = textureFile;
		}

		/// <summary>Clones an existing material</summary>
		public Material(Material prototypeMaterial) {
			this.Color = prototypeMaterial.Color;
			this.EmissiveColor = prototypeMaterial.EmissiveColor;
			this.TransparentColor = prototypeMaterial.TransparentColor;
			this.Flags = prototypeMaterial.Flags;
			this.DaytimeTexture = prototypeMaterial.DaytimeTexture;
			this.NighttimeTexture = prototypeMaterial.NighttimeTexture;
			this.BlendMode = prototypeMaterial.BlendMode;
			this.GlowAttenuationData = prototypeMaterial.GlowAttenuationData;
			this.TextColor = prototypeMaterial.TextColor;
			this.BackgroundColor = prototypeMaterial.BackgroundColor;
			this.TextPadding = prototypeMaterial.TextPadding;
			this.Font = prototypeMaterial.Font;
			this.WrapMode = prototypeMaterial.WrapMode;
		}
	}
}
