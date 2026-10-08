using System;

namespace OpenBveApi {

	/// <summary>Resolves the path variables used by package-based addons.</summary>
	/*
	 * A path variable is the first part of a path in a content file, wrapped in braces:
	 *
	 *     State = {Other}\MyAddon\Object.b3d
	 *
	 * This lets an addon point at the folder the user has configured, instead of
	 * hardcoding an absolute path which breaks as soon as that folder is moved.
	 * Path.CombineFile and Path.CombineDirectory do the substitution, so it works for
	 * every content format without each parser needing to know about it.
	 * */
	public static partial class Path {

		/// <summary>Kept as a reference so changes made in the options are picked up automatically.</summary>
		private static FileSystem.FileSystem currentFileSystem;


		// --- internal functions ---

		/// <summary>Registers the file system configuration to resolve path variables against.</summary>
		/// <param name="fileSystem">The file system configuration, or null to disable path variables</param>
		internal static void SetFileSystem(FileSystem.FileSystem fileSystem) {
			currentFileSystem = fileSystem;
		}

		/// <summary>Resolves a path variable at the start of a path into the folder it refers to.</summary>
		/// <param name="relative">The path to check</param>
		/// <param name="folder">The absolute path of the configured folder</param>
		/// <param name="remainder">The part of the path after the variable</param>
		/// <returns>False if there is no path variable, in which case the path should be used as-is</returns>
		internal static bool TryResolvePathVariable(string relative, out string folder, out string remainder) {
			folder = string.Empty;
			remainder = string.Empty;
			return TrySplitVariable(relative, out string name, out remainder) && TryGetFolder(name, out folder);
		}


		// --- private functions ---

		/// <summary>Removes a leading {Variable} from a path, if it has one.</summary>
		/// <param name="relative">The path to check</param>
		/// <param name="name">The name of the variable, without the braces</param>
		/// <param name="remainder">The part of the path after the variable</param>
		/// <returns>Whether the path starts with a {Variable}</returns>
		/*
		 * Only the first part of the path counts, so a folder which happens to have
		 * braces in its name is left alone.
		 * */
		private static bool TrySplitVariable(string relative, out string name, out string remainder) {
			name = null;
			remainder = null;
			if (string.IsNullOrEmpty(relative)) {
				return false;
			}
			int start = relative.Length - relative.TrimStart(PathSeparationChars).Length;
			if (start >= relative.Length || relative[start] != '{') {
				return false;
			}
			int end = relative.IndexOf('}', start + 1);
			if (end < 0) {
				return false;
			}
			name = relative.Substring(start + 1, end - start - 1);
			remainder = relative.Substring(end + 1).TrimStart(PathSeparationChars);
			return true;
		}

		/// <summary>Looks up the folder the user has configured for a path variable.</summary>
		/// <param name="name">The name of the variable, without the braces. Case-insensitive.</param>
		/// <param name="folder">The absolute path of the configured folder</param>
		/// <returns>Whether the variable is one we know about and has a folder set</returns>
		private static bool TryGetFolder(string name, out string folder) {
			folder = string.Empty;
			if (currentFileSystem == null || string.IsNullOrEmpty(name)) {
				return false;
			}
			switch (name.ToLowerInvariant()) {
				case "other":
					folder = currentFileSystem.OtherInstallationDirectory;
					break;
				default:
					return false;
			}
			return !string.IsNullOrEmpty(folder);
		}


		// --- public functions ---

		/// <summary>Gets the name of a path variable which has no folder set, e.g. a typo in {Other}</summary>
		/// <param name="relative">The path as written in the content file</param>
		/// <returns>The name of the variable, or null if the path has no undefined variable</returns>
		public static string GetUnknownPathVariable(string relative) {
			return TrySplitVariable(relative, out string name, out string remainder) && !TryGetFolder(name, out string folder) ? name : null;
		}

	}
}
