using System;
using System.Collections.Generic;

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

		/// <summary>Kept as a reference so that changes made in the options are picked up automatically.</summary>
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
			if (!TrySplitVariable(relative, out string name, out remainder) || !TryGetFolder(name, out folder)) {
				return false;
			}
			return true;
		}


		// --- private functions ---

		/// <summary>Removes a leading {Variable} from a path, if it has one.</summary>
		/// <param name="relative">The path to check</param>
		/// <param name="name">The name of the variable, without the braces</param>
		/// <param name="remainder">The part of the path after the variable</param>
		/// <returns>Whether the path starts with a {Variable}</returns>
		/*
		 * Only the first part of the path is treated as a variable, so a folder which
		 * happens to have braces in its name is left alone.
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
				case "route":
					folder = currentFileSystem.RouteInstallationDirectory;
					break;
				case "train":
					folder = currentFileSystem.TrainInstallationDirectory;
					break;
				case "other":
					folder = currentFileSystem.OtherInstallationDirectory;
					break;
				default:
					return false;
			}
			return !string.IsNullOrEmpty(folder);
		}


		// --- public functions ---

		/// <summary>Gets the names of all of the supported path variables.</summary>
		/// <returns>The list of path variable names</returns>
		public static IReadOnlyList<string> GetPathVariableNames() {
			return new[] { "Route", "Train", "Other" };
		}

		/// <summary>Gets the name of a path variable which is not defined, if the path starts with one.</summary>
		/// <param name="relative">The path as written in the content file</param>
		/// <returns>The name of the variable, or null if the path has no undefined variable</returns>
		/// <remarks>Used to tell the user about a typo rather than failing silently.</remarks>
		public static string GetUnknownPathVariable(string relative) {
			return TrySplitVariable(relative, out string name, out string remainder) && !TryGetFolder(name, out string folder) ? name : null;
		}

	}
}
