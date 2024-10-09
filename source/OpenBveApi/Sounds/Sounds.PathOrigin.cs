﻿// ReSharper disable MergeCastWithTypeCheck
#pragma warning disable 0659, 0661

namespace OpenBveApi.Sounds
{
	/// <summary>Represents a file or directory where the sound can be loaded from.</summary>
		public class PathOrigin : SoundOrigin {
			// --- members ---
			/// <summary>The absolute on-disk path to the sound file</summary>
			public readonly string Path;

			private readonly Hosts.HostInterface currentHost;
			// --- constructors ---
			/// <summary>Creates a new path origin.</summary>
			/// <param name="path">The path to the sound.</param>
			/// <param name="Host">The callback function to the host application</param>
			public PathOrigin(string path, Hosts.HostInterface Host) {
				Path = path;
				currentHost = Host;
			}
			// --- functions ---
			/// <summary>Gets the sound from this origin.</summary>
			/// <param name="sound">Receives the sound.</param>
			/// <returns>Whether the sound could be obtained successfully.</returns>
			public override bool GetSound(out Sound sound)
			{
			    if (!currentHost.LoadSound(Path, out sound)) {
					sound = null;
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
				if (ReferenceEquals(a, b)) return true;
				if (a is null) return false;
				if (b is null) return false;
				return a.Path == b.Path;
			}
			/// <summary>Checks whether two origins are unequal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are unequal.</returns>
			public static bool operator !=(PathOrigin a, PathOrigin b) {
				if (ReferenceEquals(a, b)) return false;
				if (a is null) return true;
				if (b is null) return true;
				return a.Path != b.Path;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (ReferenceEquals(this, obj)) return true;
				if (obj is null) return false;
				if (!(obj is PathOrigin)) return false;
				return Path == ((PathOrigin)obj).Path;
			}
		}
}
