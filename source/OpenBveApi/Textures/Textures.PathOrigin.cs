using System;
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Textures
{
	/// <summary>Represents a file or directory where the texture can be loaded from.</summary>
	public class PathOrigin : TextureOrigin
	{
		// --- members ---
		/// <summary>The absolute on-disk path to where the texture may be loaded</summary>
		public readonly string Path;
		/// <summary>The texture parameters to be applied when loading this texture from disk</summary>
		/// <remarks>Multiple textures with different parameters may be loaded from the same on-disk file</remarks>
		public readonly TextureParameters Parameters;

		private readonly Hosts.HostInterface currentHost;
		/// <summary>The last modification time (on load) of this texture</summary>
		private DateTime? lastModificationTime;
		/// <summary>The file size (on load) of this texture</summary>
		private long? fileSize;
		/// <summary>Returns the date and time the specified path was last modified</summary>
		public DateTime LastModificationTime
		{
			get
			{
				if (!lastModificationTime.HasValue)
				{
					lastModificationTime = System.IO.File.GetLastWriteTime(Path);
				}
				return lastModificationTime.Value;
			}
			set => lastModificationTime = value;
		}
		/// <summary>Gets the size in bytes of the file</summary>
		public long FileSize
		{
			get
			{
				if (!fileSize.HasValue)
				{
					fileSize = new System.IO.FileInfo(Path).Length;
				}
				return fileSize.Value;
			}
			set => fileSize = value;
		}

		// --- constructors ---
		/// <summary>Creates a new path origin.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="Host">The callback function to the host application</param>
		public PathOrigin(string path, TextureParameters parameters, Hosts.HostInterface Host)
		{
			Path = path;
			Parameters = parameters;
			currentHost = Host;
			// Snapshot on-disk state now so reloads can detect edits.
			if (System.IO.File.Exists(path))
			{
				LastModificationTime = System.IO.File.GetLastWriteTime(path);
				FileSize = new System.IO.FileInfo(path).Length;
			}
		}

		// --- functions ---
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			if (!currentHost.LoadTexture(Path, Parameters, out texture))
			{
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
		public static bool operator ==(PathOrigin a, PathOrigin b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (a is null) return false;
			if (b is null) return false;
			return a.Path == b.Path;
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(PathOrigin a, PathOrigin b)
		{
			if (ReferenceEquals(a, b)) return false;
			if (a is null) return true;
			if (b is null) return true;
			return a.Path != b.Path;
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;
			if (obj is null) return false;
			if (!(obj is PathOrigin)) return false;
			return Path == ((PathOrigin) obj).Path;
		}

		/// <summary>Returns a string representing the absolute on-disk path of this texture</summary>
		public override string ToString()
		{
			return Path;
		}

		/// <summary>Returns the hash code based on the path.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return Path?.GetHashCode() ?? 0;
		}
	}
}
