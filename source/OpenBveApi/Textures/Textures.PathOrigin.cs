namespace OpenBveApi.Textures
{
	/// <summary>Represents a file or directory where the texture can be loaded from.</summary>
		public class PathOrigin : TextureOrigin {
			// --- members ---
			public string Path;
			public OpenBveApi.Textures.TextureParameters Parameters;

			private readonly Hosts.HostInterface currentHost;
			// --- constructors ---
			/// <summary>Creates a new path origin.</summary>
			/// <param name="path">The path to the texture.</param>
			/// <param name="parameters">The parameters that specify how to process the texture.</param>
			public PathOrigin(string path, OpenBveApi.Textures.TextureParameters parameters, Hosts.HostInterface Host) {
				this.Path = path;
				this.Parameters = parameters;
				this.currentHost = Host;
			}
			// --- functions ---
			/// <summary>Gets the texture from this origin.</summary>
			/// <param name="texture">Receives the texture.</param>
			/// <returns>Whether the texture could be obtained successfully.</returns>
			public override bool GetTexture(out Texture texture)
			{
				if (!currentHost.LoadTexture(this.Path, this.Parameters, out texture)) {
					texture = null;
					return false;
				}
			    return true;
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

			/// <summary>Returns a string representing the absolute on-disk path of this texture</summary>
			public override string ToString()
			{
				return this.Path;
			}
		}
}
