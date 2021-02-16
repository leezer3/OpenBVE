namespace OpenBve {
	internal static class PluginManager {
		
		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		internal static bool CheckWin32Header(string file) {
			using (System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
				using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
					if (reader.ReadUInt16() != 0x5A4D) {
						/* Not MZ signature */
						return false;
					}
					stream.Position = 0x3C;
					stream.Position = reader.ReadInt32();
					if (reader.ReadUInt32() != 0x00004550) {
						/* Not PE signature */
						return false;
					}
					if (reader.ReadUInt16() != 0x014C) {
						/* Not IMAGE_FILE_MACHINE_I386 */
						return false;
					}
				}
			}
			return true;
		}
		
		
		
	}
}
