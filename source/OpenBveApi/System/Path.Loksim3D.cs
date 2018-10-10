using System;
using System.IO;

namespace OpenBveApi
{

	public static partial class Path
	{
		/// <summary>Provides path resolution functions for Loksim3D related content.</summary>
		public static class Loksim3D
		{
			/// <summary>
			/// Attempts to combine an absolute path
			/// </summary>
			/// <param name="absolute"></param>
			/// <param name="relative"></param>
			/// <param name="packageDirectory"></param>
			/// <returns></returns>
			public static string CombineFile(string absolute, string relative, string packageDirectory)
			{
				try
				{
					string file = Path.CombineFile(absolute, relative);
					//Trim
					relative = relative.Trim();
					//Make all path separators conform
					relative = relative.Replace('/', '\\');
					
					if (File.Exists(file))
					{
						//First try a standard path combine
						return file;
					}
					try
					{
						//Check the Loksim package install directory next
						if (File.Exists(Path.CombineFile(packageDirectory, relative)))
						{
							return Path.CombineFile(packageDirectory, relative);
						}
					}
					catch
					{ }
					if (relative[0] == '\\')
					{
						//We now need to start a fuzzy find.

						//A char of '\' or '/' denotes a reference to the base Loksim3D directory
						//Split the path, and find the first directory in it
						string[] splitPath = relative.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
						DirectoryInfo d = new DirectoryInfo(absolute);
						while (d.Parent != null)
						{
							//Recurse upwards and try to see if we can find this directory
							d = d.Parent;
							if (d.ToString().ToLowerInvariant() == splitPath[0].ToLowerInvariant())
							{
								d = d.Parent;
								file = Path.CombineFile(d.FullName, relative);
								if (File.Exists(file))
								{
									return file;
								}
								//Found the directory but not the file, so break out of the loop
								break;
							}
						}
					}
					if (!File.Exists(file))
					{
						//Last-ditch attempt: Check User & Public for the Loksim object directory
						file = Path.CombineFile(Path.CombineFile(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Loksim3D"), relative);
						if (File.Exists(file))
						{

							return file;
						}
						file = Path.CombineFile(Path.CombineFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Loksim3D"), relative);
					}
					if (File.Exists(file))
					{
						return file;
					}
					return absolute;
				}
				catch
				{
					return absolute;
				}
			}
		}
	}

}
