using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace OpenBveApi.Objects {

	/// <summary>Represents a material with basic properties such as colors and textures.</summary>
	public class BasicMaterial : AbstractMaterial {
		// --- members ---
		/// <summary>The reflective color.</summary>
		public Color32 ReflectiveColor;
		/// <summary>The emissive color.</summary>
		public Color24 EmissiveColor;
		/// <summary>The daytime texture, or a null reference.</summary>
		public TextureHandle DaytimeTexture;
		/// <summary>The nighttime texture, or a null reference.</summary>
		public TextureHandle NighttimeTexture;
		/// <summary>The blend mode.</summary>
		public BlendModes BlendMode;
		/// <summary>The glow, or a null reference.</summary>
		public AbstractGlow Glow;
	}
	
}