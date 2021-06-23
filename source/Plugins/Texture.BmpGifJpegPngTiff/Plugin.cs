using System.Drawing;
using System.IO;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;

namespace Plugin {
	/// <summary>Implements the texture interface.</summary>
	public partial class Plugin : TextureInterface {
		
		// --- members ---
		
		/// <summary>The host that loaded the plugin.</summary>
		private HostInterface CurrentHost = null;
		/// <summary>The list of enabled hacks</summary>
		internal static CompatabilityHacks EnabledHacks;
		
		// --- functions ---
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host) {
			CurrentHost = host;
		}

		public override void SetCompatabilityHacks(CompatabilityHacks enabledHacks)
		{
			EnabledHacks = enabledHacks;
		}

		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height) {
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					uint identifier1 = reader.ReadUInt32();
					uint identifier2 = reader.ReadUInt32();
					if ((identifier1 & 0xFFFF) == 0x4D42) {
						/* BMP */
						stream.Position = 18;
						width = reader.ReadInt32();
						height = reader.ReadInt32();
						return true;
					}
				    if (identifier1 == 0x38464947 & ((identifier2 & 0xFFFF) == 0x6137 | (identifier2 & 0xFFFF) == 0x6139)) {
				        /* GIF */
				        stream.Position = 6;
				        width = (int)reader.ReadUInt16();
				        height = (int)reader.ReadUInt16();
				        return true;
				    }
				    if (identifier1 == 0x474E5089 & identifier2 == 0x0A1A0A0D) {
				        /* PNG */
				        if (reader.ReadUInt32() == 0x0D000000) {
				            if (reader.ReadUInt32() == 0x52444849) {
				                uint bigWidth = reader.ReadUInt32();
				                uint bigHeight = reader.ReadUInt32();
				                width = (int)((bigWidth >> 24) | ((bigWidth >> 8) & 0xFF00) | ((bigWidth & 0xFF00) << 8) | (bigWidth << 24));
				                height = (int)((bigHeight >> 24) | ((bigHeight >> 8) & 0xFF00) | ((bigHeight & 0xFF00) << 8) | (bigHeight << 24));
				                return true;
				            }
				        }
				    }
				}
			}
			using (Image image = Image.FromFile(path)) {
				width = image.Width;
				height = image.Height;
				return true;
			}
		}
		
		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public override bool CanLoadTexture(string path) {
			if (File.Exists(path)) {
				if (FileFormats.IsNautilusFile(path))
				{
					return false;
				}
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					using (BinaryReader reader = new BinaryReader(stream)) {
						if (stream.Length < 8)
						{
							return false;
						}
						uint identifier1 = reader.ReadUInt32();
						uint identifier2 = reader.ReadUInt32();
						if ((identifier1 & 0xFFFF) == 0x4D42) {
							/* BMP */
							return true;
						} else if (identifier1 == 0x38464947 & ((identifier2 & 0xFFFF) == 0x6137 | (identifier2 & 0xFFFF) == 0x6139)) {
							/* GIF */
							return true;
						} else if (identifier1 == 0xE0FFD8FF | identifier1 == 0xE1FFD8FF) {
							/* JPEG */
							return true;
						} else if (identifier1 == 0x474E5089 & identifier2 == 0x0A1A0A0D) {
							/* PNG */
							return true;
						} else if (identifier1 == 0x002A4949 | identifier1 == 0x2A004D4D) {
							/* TIFF */
							return true;
						}
					}
				}
			}
			return false;
		}
		
		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(string path, out Texture texture) {
			return Parse(path, out texture);
		}
		
	}
}
