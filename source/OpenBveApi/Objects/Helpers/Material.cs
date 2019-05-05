using System.Drawing;
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
		/// <summary>Whether the emissive color is in use</summary>
		public bool EmissiveColorUsed;
		/// <summary>The transparent color</summary>
		public Color24 TransparentColor;
		/// <summary>Whether the transparent color is in use</summary>
		public bool TransparentColorUsed;
		/// <summary>The absolute on-disk path to the daytime texture</summary>
		public string DaytimeTexture;
		/// <summary>The absolute on-disk path to the nighttime texture</summary>
		public string NighttimeTexture;
		/// <summary>
		/// The absolute on-disk path to the transparency texture (Used for both daytime and nighttime textures)</summary>
		public string TransparencyTexture;
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
		public Color TextColor;
		/// <summary>The background color for the text</summary>
		public Color BackgroundColor;
		/// <summary>The font to use for the text</summary>
		public string Font;
		/// <summary>The text padding to apply</summary>
		public Vector2 TextPadding; 

		/// <summary>Creates a new Material with default properties</summary>
		public Material() {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
		}

		/// <summary>Creates a new material from the specified texture file with default properties</summary>
		/// <param name="Texture"></param>
		public Material(string Texture) {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
			this.DaytimeTexture = Texture;
		}

		/// <summary>Clones an existing material</summary>
		public Material(Material Prototype) {
			this.Color = Prototype.Color;
			this.EmissiveColor = Prototype.EmissiveColor;
			this.EmissiveColorUsed = Prototype.EmissiveColorUsed;
			this.TransparentColor = Prototype.TransparentColor;
			this.TransparentColorUsed = Prototype.TransparentColorUsed;
			this.DaytimeTexture = Prototype.DaytimeTexture;
			this.NighttimeTexture = Prototype.NighttimeTexture;
			this.BlendMode = Prototype.BlendMode;
			this.GlowAttenuationData = Prototype.GlowAttenuationData;
			this.TextColor = Prototype.TextColor;
			this.BackgroundColor = Prototype.BackgroundColor;
			this.TextPadding = Prototype.TextPadding;
			this.Font = Prototype.Font;
			this.WrapMode = Prototype.WrapMode;
		}
	}
}
