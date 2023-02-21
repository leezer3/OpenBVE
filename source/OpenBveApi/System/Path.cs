#pragma warning disable 0659, 0661

using System;
using System.IO;
using System.Security.Cryptography;

namespace OpenBveApi {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	/// <summary>Provides path-related functions for accessing files and directories in a cross-platform manner.</summary>
	public static partial class Path {
		
		// --- read-only fields ---
		
		/// <summary>The list of characters that are invalid in platform-independent relative paths.</summary>
		private static readonly char[] InvalidPathChars = { ':', '*', '?', '"', '<', '>', '|' };
		
		/// <summary>The list of characters at which relative paths are separated into parts.</summary>
		private static readonly char[] PathSeparationChars = { '/', '\\' };
		
		/// <summary>Combines a platform-specific absolute path with a platform-independent relative path that points to a directory.</summary>
		/// <param name="absolute">The platform-specific absolute path.</param>
		/// <param name="relative">The platform-independent relative path.</param>
		/// <param name="allowQueryStr">If a part similar to a URL query string at the end of the path should be preserved.</param>
		/// <returns>A platform-specific absolute path to the specified directory.</returns>
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths or due to unauthorized access.</exception>
		public static string CombineDirectory(string absolute, string relative, bool allowQueryStr) {
            int index = relative.IndexOf("??", StringComparison.Ordinal);
			if (index >= 0) {
				string directory = CombineDirectory(absolute, relative.Substring(0, index).TrimEnd());
				if (Directory.Exists(directory)) {
					return directory;
				}
				return CombineDirectory(absolute, relative.Substring(index + 2).TrimStart());
			}
			string queryString = "";
			int questionMarkIndex = relative.IndexOf("?", StringComparison.Ordinal);
			if (allowQueryStr && questionMarkIndex >= 0) {
				queryString = relative.Substring(questionMarkIndex);
				relative = relative.Substring(0, questionMarkIndex);
			}
			if (relative.IndexOfAny(InvalidPathChars) >= 0) {
				throw new ArgumentException("The relative path contains invalid characters.");
			}
			string[] parts = relative.Split(PathSeparationChars, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parts.Length; i++) {
				if (parts[i].Length != 0) {
					/*
					 * Consider only non-empty parts.
					 * */
					if (IsAllPeriods(parts[i])) {
						/*
						 * A string of periods is a reference to an
						 * upper directory. A single period is the
						 * current directory. For each additional
						 * period, jump one directory up.
						 * */
						for (int j = 1; j < parts[i].Length; j++) {
							absolute = System.IO.Path.GetDirectoryName(absolute);
						}
					} else {
						/*
						 * This part references a directory.
						 * */
					    if (absolute != null)
					    {
					        string directory = System.IO.Path.Combine(absolute, parts[i]);
					        if (Directory.Exists(directory)) {
					            absolute = directory;
					        } else {
					            /*
							 * Try to find the directory case-insensitively.
							 * */
					            bool found = false;
					            if (Directory.Exists(absolute)) {
						            try
						            {
							            string[] directories = Directory.GetDirectories(absolute);
							            for (int j = 0; j < directories.Length; j++)
							            {
								            string name = System.IO.Path.GetFileName(directories[j]);
								            if (name != null && name.Equals(parts[i], StringComparison.OrdinalIgnoreCase))
								            {
									            absolute = directories[j];
									            found = true;
									            break;
								            }
							            }
						            }
						            catch (Exception e)
						            {
										if (e is UnauthorizedAccessException)
										{
											//If we don't have access to the path, this causes an infinite recursion loop...
											throw;
										}
						            }
					            }
					            if (!found) {
					                absolute = directory;
					            }
					        }
					    }
					}
				}
			}
			return absolute + queryString;
		}

		/// <summary>Combines a platform-specific absolute path with a platform-independent relative path that points to a directory.</summary>
		/// <param name="absolute">The platform-specific absolute path.</param>
		/// <param name="relative">The platform-independent relative path.</param>
		/// <returns>A platform-specific absolute path to the specified directory.</returns>
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths or due to unauthorized access.</exception>
		public static string CombineDirectory(string absolute, string relative) {
			return CombineDirectory(absolute, relative, false);
		}

		/// <summary>Combines a platform-specific absolute path with a platform-independent relative path that points to a file.</summary>
		/// <param name="absolute">The platform-specific absolute path.</param>
		/// <param name="relative">The platform-independent relative path.</param>
		/// <returns>Whether the operation succeeded and the specified file was found.</returns>
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths or due to unauthorized access.</exception>
		public static string CombineFile(string absolute, string relative) {
            int index = relative.IndexOf("??", StringComparison.Ordinal);
			if (index >= 0) {
				string file = CombineFile(absolute, relative.Substring(0, index).TrimEnd());
				if (File.Exists(file)) {
					return file;
				}
				return CombineFile(absolute, relative.Substring(index + 2).TrimStart());
			}
			if (relative.IndexOfAny(InvalidPathChars) >= 0) {
				throw new ArgumentException("The relative path contains invalid characters.");
			}
			string[] parts = relative.Split(PathSeparationChars, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parts.Length; i++) {
				if (parts[i].Length != 0) {
					/*
					 * Consider only non-empty parts.
					 * */
					if (IsAllPeriods(parts[i]))
					{
						if (i == parts.Length - 1) {
							/*
							 * The last part must not be all periods because
							 * it would reference a directory then, not a file.
							 * */
							throw new ArgumentException("The relative path is malformed.");
						}
						/*
						 * A string of periods is a reference to an
						 * upper directory. A single period is the
						 * current directory. For each additional
						 * period, jump one directory up.
						 * */
						for (int j = 1; j < parts[i].Length; j++) {
							absolute = System.IO.Path.GetDirectoryName(absolute);
						}
					} else if (i == parts.Length - 1) {
						/*
						 * The last part references a file.
						 * */
					    if (absolute == null) continue;
					    string file = System.IO.Path.Combine(absolute, parts[i]);
					    if (File.Exists(file)) {
					        return file;
					    }
				        /*
						 * Try to find the file case-insensitively.
						 * */
				        if (Directory.Exists(absolute))
				        {
					        string[] files = Directory.GetFiles(absolute);
					        for (int j = 0; j < files.Length; j++)
					        {
						        string name = System.IO.Path.GetFileName(files[j]);
						        if (name != null && name.Equals(parts[i], StringComparison.OrdinalIgnoreCase))
						        {
							        return files[j];
						        }
					        }
				        }
				        return file;
					} else {
						/*
						 * This part references a directory.
						 * */
					    if (absolute == null) continue;
					    string directory = System.IO.Path.Combine(absolute, parts[i]);
					    if (Directory.Exists(directory)) {
					        absolute = directory;
					    } else {
					        /*
							 * Try to find the directory case-insensitively.
							 * */
					        bool found = false;
					        if (Directory.Exists(absolute)) {
					            string[] directories = Directory.GetDirectories(absolute);
					            for (int j = 0; j < directories.Length; j++) {
					                string name = System.IO.Path.GetFileName(directories[j]);
					                if (name != null && name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
					                    absolute = directories[j];
					                    found = true;
					                    break;
					                }
					            }
					        }
					        if (!found) {
					            absolute = directory;
					        }
					    }
					}
				}
			}
			throw new ArgumentException("The reference to the file is malformed.");
		}

		/// <summary>Tests whether a string contains characters invalid for use in a file name or path</summary>
		/// <param name="Expression">The string to test</param>
		/// <returns>True if this string contains invalid characters, false otherwise</returns>
		public static bool ContainsInvalidChars(string Expression)
		{
			char[] a = System.IO.Path.GetInvalidFileNameChars();
			char[] b = System.IO.Path.GetInvalidPathChars();

			if (!IsAbsolutePath(Expression))
			{
				if (Expression.IndexOfAny(InvalidPathChars) != -1)
				{
					// Check our platform independant list first
					return true;
				}
			}
			

			for (int i = 0; i < Expression.Length; i++)
			{
				for (int j = 0; j < a.Length; j++)
				{
					if (Expression[i] == a[j])
					{
						for (int k = 0; k < b.Length; k++)
						{
							if (Expression[i] == b[k])
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks whether the specified path is an absolute path.
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <returns>True if this path is an absolute path, false otherwise</returns>
		public static bool IsAbsolutePath(string path)
		{
			if (path.Length < 1)
			{
				return false;
			}

			if (path[0] == PathSeparationChars[0] || path[0] == PathSeparationChars[1])
			{
				// e.g.
				// \Test\Foo.txt (Windows)
				// /Test/Foo.txt (Windows, Unix)
				return true;
			}

			if (path.Length < 3)
			{
				return false;
			}

			if (path[1] == ':' && (path[2] == PathSeparationChars[0] || path[2] == PathSeparationChars[1]))
			{
				// e.g.
				// C:\Test\Foo.txt (Windows)
				// C:/Test/Foo.txt (Windows)
				return true;
			}

			return false;
		}


		// --- private functions ---
		
		/// <summary>Checks whether the specified string consists only of periods.</summary>
		/// <param name="text">The string to check.</param>
		/// <returns>Whether the string consists only of periods.</returns>
		private static bool IsAllPeriods(string text) {
			for (int i = 0; i < text.Length; i++) {
				if (text[i] != '.') {
					return false;
				}
			}
			return true;
		}

		/// <summary>Gets the SHA-256 checksum for a file</summary>
		/// <param name="file">The file to hash</param>
		/// <returns>The SHA-256 hash, or an empty string if not valid</returns>
		public static string GetChecksum(string file)
		{
			if (string.IsNullOrEmpty(file) || !File.Exists(file))
			{
				return string.Empty;
			}
			using (FileStream stream = File.OpenRead(file))
			{
				using (SHA256Managed sha = new SHA256Managed())
				{
					byte[] checksum = sha.ComputeHash(stream);
					return BitConverter.ToString(checksum).Replace("-", string.Empty);
				}
			}
		}
	}
}
