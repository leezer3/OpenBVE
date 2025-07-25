#pragma warning disable 0659, 0661

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace OpenBveApi {

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
			if (string.IsNullOrEmpty(absolute))
			{
				throw new ArgumentException("The absolute path was empty.");
			}
			if (string.IsNullOrEmpty(relative))
			{
				throw new ArgumentException("The relative path was empty.");
			}
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
							absolute = Path.GetDirectoryName(absolute);
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
			if (string.IsNullOrEmpty(absolute))
			{
				throw new ArgumentException("The absolute path was empty.");
			}
			if (string.IsNullOrEmpty(relative))
			{
				throw new ArgumentException("The relative path was empty.");
			}
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
							absolute = Path.GetDirectoryName(absolute);
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

		
		internal static HashSet<char> GetInvalidPathChars() => new HashSet<char>
		{
			/* Licensed to the .NET Foundation under one or more agreements.
			 * The .NET Foundation licenses this file to you under the MIT license.
			 *
			 * https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/Path.Windows.cs
			 *
			 * Use the Windows invalid path characters for a greater subset
			 * Mono only returns '\0'
			 *
			 * https://github.com/leezer3/OpenBVE/issues/1073
			 */
			'|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
			(char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
			(char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
			(char)31
		};

		internal static HashSet<char> GetInvalidFileNameChars() => new HashSet<char>
		{
			/* Licensed to the .NET Foundation under one or more agreements.
			 * The .NET Foundation licenses this file to you under the MIT license.
			 *
			 * https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/Path.Windows.cs
			 */
			'\"', '<', '>', '|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
			(char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
			(char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
			(char)31, ':', '*', '?', '\\', '/'
		};

		/// <summary>Tests whether a string contains characters invalid for use in a file name or path</summary>
		/// <param name="Expression">The string to test</param>
		/// <returns>True if this string contains invalid characters, false otherwise</returns>
		public static bool ContainsInvalidChars(string Expression)
		{
			HashSet<char> a = GetInvalidFileNameChars();
			HashSet<char> b = GetInvalidPathChars();

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
				if (a.Contains(Expression[i]) && b.Contains(Expression[i]))
				{
					return true;
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

		/*
		 * Provide easy mirrors to the System.IO.Path functions
		 */

		/// <summary>Returns the directory information for the specified path string</summary>
		/// <param name="path">The path string</param>
		public static string GetDirectoryName(string path)
		{
			return System.IO.Path.GetDirectoryName(path);
		}

		/// <summary>Returns the file name and extension for the specificed path string</summary>
		/// <param name="path">The path string</param>
		public static string GetFileName(string path)
		{
			return System.IO.Path.GetFileName(path);
		}

		/// <summary>Returns the file name for the specificed path string</summary>
		/// <param name="path">The path string</param>
		public static string GetFileNameWithoutExtension(string path)
		{
			return System.IO.Path.GetFileNameWithoutExtension(path);
		}

		/// <summary>Determines whether a path contains a filename extension</summary>
		/// <param name="path">The path string</param>
		public static bool HasExtension(string path)
		{
			return System.IO.Path.HasExtension(path);
		}

		/// <summary>Gets a value containing whether the specified path string contains a root</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsPathRooted(string path)
		{
			return System.IO.Path.IsPathRooted(path);
		}
	}
}
