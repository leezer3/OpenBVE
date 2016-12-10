using System;
using System.IO;
using System.Text;

namespace OpenBve {
	internal static class Tar {
		
		/// <summary>Extracts the tar data into a specified directory.</summary>
		/// <param name="data">The tar data to extract.</param>
		/// <param name="directory">The directory to extract the content to.</param>
		/// <param name="prefix">The directory prefix that is trimmed off all the paths encountered in this archive, or a null reference.</param>
		internal static void Unpack(byte[] bytes, string directory, string prefix) {
			System.Text.ASCIIEncoding ascii = new ASCIIEncoding();
			System.Text.UTF8Encoding utf8 = new UTF8Encoding();
			int position = 0;
			string longLink = null;
			while (position < bytes.Length) {
				string name;
				if (longLink != null) {
					name = longLink;
					longLink = null;
				} else {
					name = utf8.GetString(bytes, position, 100).TrimEnd('\0');
				}
				if (name.Length == 0) {
					/*
					 * The name is empty. This marks the end of the file.
					 * */
					break;
				} else {
					/*
					 * Read the header and advance the position.
					 * */
					string sizeString = ascii.GetString(bytes, position + 124, 12).Trim('\0', ' ');
					int size = Convert.ToInt32(sizeString, 8);
					int mode;
					if (name[name.Length - 1] == '/') {
						mode = 53;
					} else {
						mode = (int)bytes[position + 156];
					}
					if (bytes[position + 257] == 0x75 && bytes[position + 258] == 0x73 && bytes[position + 259] == 0x74 && bytes[position + 260] == 0x61 && bytes[position + 261] == 0x72 && bytes[position + 262] == 0x00) {
						/*
						 * This is a POSIX ustar archive.
						 * */
						string namePrefix = utf8.GetString(bytes, position + 345, 155).TrimEnd(' ');
						if (namePrefix.Length != 0) {
							if (namePrefix[namePrefix.Length - 1] != '/' && name[0] != '/') {
								name = namePrefix + '/' + name;
							} else {
								name = namePrefix + name;
							}
						}
					} else if (bytes[position + 257] == 0x75 && bytes[position + 258] == 0x73 && bytes[position + 259] == 0x74 && bytes[position + 260] == 0x61 && bytes[position + 261] == 0x72 && bytes[position + 262] == 0x20) {
						/*
						 * This is a GNU tar archive.
						 * TODO: Implement support for GNU tar archives here.
						 * */
					}
					position += 512;
					/* 
					 * Process the data depending on the mode.
					 * */
					if (mode == 53) {
						/*
						 * This is a directory.
						 * */
						if (name[name.Length - 1] == '/') {
							name = name.Substring(0, name.Length - 1);
						}
						name = name.Replace('/', Path.DirectorySeparatorChar);
						if (prefix != null) {
							if (name.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) {
								name = name.Substring(prefix.Length + 1);
							} else if (string.Equals(name, prefix, StringComparison.OrdinalIgnoreCase)) {
								name = string.Empty;
							}
						}
						try {
							Directory.CreateDirectory(Path.Combine(directory, name));
						} catch { }
					} else if (mode < 49 | mode > 54) {
						/*
						 * This is a normal file.
						 * */
						if (name != "././@LongLink") {
							name = name.Replace('/', Path.DirectorySeparatorChar);
							if (prefix != null && name.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) {
								name = name.Substring(prefix.Length + 1);
							}
						}
						int blocks = size + 511 >> 9;
						byte[] buffer = new byte[size];
						Array.Copy(bytes, position, buffer, 0, size);
						position += blocks << 9;
						if (name == "././@LongLink") {
							longLink = utf8.GetString(buffer);
							if (longLink.Length != 0 && longLink[longLink.Length - 1] == '\0') {
								longLink = longLink.Substring(0, longLink.Length - 1);
							}
						} else {
							string file = Path.Combine(directory, name);
							try {
								Directory.CreateDirectory(Path.GetDirectoryName(file));
							} catch { }
							if (File.Exists(file)) {
								try {
									File.SetAttributes(file, FileAttributes.Normal);
								} catch { }
							}
							File.WriteAllBytes(Path.Combine(directory, name), buffer);
						}
					} else {
						/*
						 * Unsupported mode.
						 * */
					}
				}
			}
		}

	}
}