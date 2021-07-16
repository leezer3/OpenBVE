using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace OpenBveApi.Packages
{
	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	/// <summary>Defines a package database</summary>
	[Serializable]
	[XmlType("openBVE")]
	public class PackageDatabase
	{
		/// <summary>The installed routes</summary>
		public List<Package> InstalledRoutes;

		/// <summary>The installed trains</summary>
		public List<Package> InstalledTrains;

		/// <summary>The installed other items</summary>
		public List<Package> InstalledOther;
		
		/// <summary>The database version</summary>
		public int DatabaseVersion;

		/// <summary>The expected database version</summary>
		[XmlIgnore]
		internal static readonly int ExpectedDatabaseVersion = 2;

		internal void Upgrade()
		{
			switch (DatabaseVersion)
			{
				case 0:
					FixDependancyList(ref InstalledRoutes);
					FixDependancyList(ref InstalledTrains);
					FixDependancyList(ref InstalledOther);
					break;
			}

			DatabaseVersion = ExpectedDatabaseVersion;
		}

		private void FixDependancyList(ref List<Package> packages)
		{
			/*
			 * Initial database version didn't correctly store the list of dependant packages, so we're going to have to go through the list to populate it....
			 */
			foreach (Package currentPackage in packages)
			{
				AddDependancies(currentPackage);
			}
		}

		/// <summary>Adds the database dependancies</summary>
		public void AddDependancies(Package currentPackage)
		{
			if (currentPackage.Dependancies != null)
			{
				PopulateDependantList(currentPackage.Dependancies, currentPackage.GUID);
			}
			if (currentPackage.Reccomendations != null)
			{
				PopulateDependantList(currentPackage.Reccomendations, currentPackage.GUID);
			}
		}

		private void PopulateDependantList(List<Package> packageList, string GUID)
		{
			foreach (Package currentDependancy in packageList)
			{
				switch (currentDependancy.PackageType)
				{
					case PackageType.Route:
						foreach (Package p in InstalledRoutes)
						{
							if (p.GUID == currentDependancy.GUID)
							{
								if (!p.DependantPackages.Contains(GUID))
								{
									p.DependantPackages.Add(GUID);
								}
							}
						}
						break;
					case PackageType.Train:
						foreach (Package p in InstalledTrains)
						{
							if (p.GUID == currentDependancy.GUID)
							{
								if (!p.DependantPackages.Contains(GUID))
								{
									p.DependantPackages.Add(GUID);
								}
							}
						}
						break;
					case PackageType.Other:
						foreach (Package p in InstalledOther)
						{
							if (p.GUID == currentDependancy.GUID)
							{
								if (!p.DependantPackages.Contains(GUID))
								{
									p.DependantPackages.Add(GUID);
								}
							}
						}
						break;
				}
			}
		}
		
	}

	/// <summary>Stores the current package database</summary>
	public static class Database
	{
		/// <summary>The current package database</summary>
		public static PackageDatabase currentDatabase;
		private static string currentDatabaseFolder;
		private static string currentDatabaseFile;
		/// <summary>Call this method to save the package list to disk.</summary>
		/// <remarks>Returns false if an error was encountered whilst saving the database.</remarks>
		public static bool SaveDatabase()
		{
			try
			{
				if (!Directory.Exists(currentDatabaseFolder))
				{
					Directory.CreateDirectory(currentDatabaseFolder);
				}
				if (File.Exists(currentDatabaseFile))
				{
					File.Delete(currentDatabaseFile);
				}
				using (StreamWriter sw = new StreamWriter(currentDatabaseFile))
				{
					XmlSerializer listWriter = new XmlSerializer(typeof(PackageDatabase));
					listWriter.Serialize(sw, currentDatabase);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		/// <summary>Loads a package database XML file as the current database</summary>
		/// <param name="Folder">The root database folder</param>
		/// <param name="File">The database file</param>
		/// <param name="ErrorMessage">The error message to return if failed</param>
		/// <returns>Whether the loading succeded</returns>
		public static bool LoadDatabase(string Folder, string File, out string ErrorMessage)
		{
			ErrorMessage = string.Empty;
			currentDatabaseFolder = Folder;
			currentDatabaseFile = File;
			if (System.IO.File.Exists(currentDatabaseFile))
			{
				try
				{
					XmlSerializer listReader = new XmlSerializer(typeof(PackageDatabase));
					bool saveDatabase = false;
					using (XmlReader reader = XmlReader.Create(currentDatabaseFile))
					{
						currentDatabase = (PackageDatabase) listReader.Deserialize(reader);
						if (currentDatabase.DatabaseVersion > PackageDatabase.ExpectedDatabaseVersion)
						{
							ErrorMessage = @"packages_database_newer_expected";
						}

						if (currentDatabase.DatabaseVersion < PackageDatabase.ExpectedDatabaseVersion)
						{
							currentDatabase.Upgrade();
							saveDatabase = true;
						}
					}

					if (saveDatabase)
					{
						//Can't do above as the reader locks it
						SaveDatabase();
					}
				}
				catch
				{
					//Loading the DB failed, so just create a new one
					ErrorMessage = @"packages_database_invalid_xml";
					currentDatabase = null;
				}
			}
			if (currentDatabase == null)
			{
				currentDatabase = new PackageDatabase
				{
					InstalledRoutes = new List<Package>(),
					InstalledTrains = new List<Package>(),
					InstalledOther = new List<Package>()
				};
				return false;
			}
			return true;
		}
		
		/*
		 * DATABASE FUNCTIONS
		 * 
		 */

		/// <summary>Checks a list of dependancies or reccomendations to see if they are installed</summary>
		public static List<Package> checkDependsReccomends(List<Package> currentList)
		{
			if (currentList == null)
			{
				return null;
			}
			foreach (Package currentDependancy in currentList.ToList())
			{
				switch (currentDependancy.PackageType)
				{
					case PackageType.Route:
						if (DependancyMet(currentDependancy, currentDatabase.InstalledRoutes))
						{
							currentList.Remove(currentDependancy);
						}
						break;
					case PackageType.Train:
						if (DependancyMet(currentDependancy, currentDatabase.InstalledTrains))
						{
							currentList.Remove(currentDependancy);
						}
						break;
					case PackageType.Other:
						if (DependancyMet(currentDependancy, currentDatabase.InstalledOther))
						{
							currentList.Remove(currentDependancy);
						}
						break;
				}
				if (currentList.Count == 0) break;
			}
			if (currentList.Count == 0)
			{
				//Return null if there are no unmet dependancies
				return null;
			}
			return currentList;
		}

		/// <summary>Checks whether a dependancy is met by a member of a list of packages</summary>
		/// <param name="dependancy">The dependancy</param>
		/// <param name="currentList">The package list to check</param>
		public static bool DependancyMet(Package dependancy, List<Package> currentList)
		{
			if (currentList == null)
			{
				return false;
			}
			foreach (Package Package in currentList)
			{
				//Check GUID
				if (Package.GUID == dependancy.GUID)
				{
					if ((dependancy.MinimumVersion == null || dependancy.MinimumVersion <= Package.PackageVersion) &&
						(dependancy.MaximumVersion == null || dependancy.MaximumVersion >= Package.PackageVersion))
					{
						//If the version is OK, remove
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Checks whether uninstalling a package will break any dependancies</summary>
		/// <param name="packagesToRemove">The list of packages that will be removed</param>
		/// <returns>A list of potentially broken packages</returns>
		public static List<Package> CheckUninstallDependancies(List<string> packagesToRemove)
		{
			List<Package> brokenPackages = new List<Package>();
			foreach (Package Route in currentDatabase.InstalledRoutes)
			{
				if (packagesToRemove.Contains(Route.GUID))
				{
					brokenPackages.Add(Route);
				}
			}
			foreach (Package Train in currentDatabase.InstalledTrains)
			{
				if (packagesToRemove.Contains(Train.GUID))
				{
					brokenPackages.Add(Train);
				}
			}
			foreach (Package Other in currentDatabase.InstalledRoutes)
			{
				if (packagesToRemove.Contains(Other.GUID))
				{
					brokenPackages.Add(Other);
				}
			}
			return brokenPackages;
		}

	}

	

	/// <summary>Contains the database functions</summary>
	public class DatabaseFunctions
	{
		/// <summary>Stores the directory separator character for the current platform</summary>
		private static readonly char c = System.IO.Path.DirectorySeparatorChar;
		/// <summary>This function takes a list of files, and returns the files with corrected relative paths for compression or extraction</summary>
		/// <param name="tempList">The file list</param>
		/// <returns>The file list with corrected relative paths</returns>
		public static List<PackageFile> FindFileLocations(List<PackageFile> tempList)
		{
			
			//Now determine whether this is part of a recognised folder structure
			for (int i = 0; i < tempList.Count; i++)
			{
				if (tempList[i].relativePath.StartsWith(c + "Railway" + c, StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = tempList[j].relativePath.Remove(0,8);
					}
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith(c + "Train" + c, StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root train folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = tempList[j].relativePath.Remove(0, 7);
					}
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith(c + "Route", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith(c + "Object", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith(c + "Sound", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					return tempList;
				}

			}

			for (int i = 0; i < tempList.Count; i++)
			{
				var TestCase = tempList[i].absolutePath.Replace(tempList[i].relativePath, "");
				if (TestCase.EndsWith("Railway", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					return tempList;
				}
				if (TestCase.EndsWith("Train", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					return tempList;
				}
				if (TestCase.EndsWith("Route", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = c + "Route" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Object", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = c + "Object" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Sound", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = c + "Sound" + tempList[j].relativePath;
					}
					return tempList;
				}

			}
			//So, this doesn't have any easily findable folders
			//We'll have to do this the hard way.
			//Remember that people can store stuff in odd places
			int SoundFiles = 0;
			int ImageFiles = 0;
			int ObjectFiles = 0;
			int RouteFiles = 0;
			int TrainFiles = 0;
			for (int i = 0; i < tempList.Count; i++)
			{
				if (tempList[i].relativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
				{
					SoundFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					ImageFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
				{
					ImageFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
				{
					ImageFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".ace", StringComparison.OrdinalIgnoreCase))
				{
					ImageFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase))
				{
					ObjectFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
				{
					//Why on earth are CSV files both routes and objects??!!
					RouteFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".animated", StringComparison.OrdinalIgnoreCase))
				{
					ObjectFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".rw", StringComparison.OrdinalIgnoreCase))
				{
					RouteFiles++;
				}
				else if (tempList[i].relativePath.EndsWith("train.dat", StringComparison.OrdinalIgnoreCase) || tempList[i].relativePath.EndsWith("panel.cfg", StringComparison.OrdinalIgnoreCase)
				|| tempList[i].relativePath.EndsWith("panel2.cfg", StringComparison.OrdinalIgnoreCase) || tempList[i].relativePath.EndsWith("extensions.cfg", StringComparison.OrdinalIgnoreCase)
				|| tempList[i].relativePath.EndsWith("ats.cfg", StringComparison.OrdinalIgnoreCase) || tempList[i].relativePath.EndsWith("train.txt", StringComparison.OrdinalIgnoreCase))
				{
					TrainFiles++;
				}
				else if (tempList[i].relativePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
				{
					//Not sure about this one
					//TXT files are commonly used for includes though
					RouteFiles++;
				}
				
				
			}
			//We've counted the number of files found:
			if (SoundFiles != 0 && ObjectFiles == 0 && ImageFiles == 0)
			{
				//This would appear to be a subfolder of the SOUND folder
				for (int j = 0; j < tempList.Count; j++)
				{
					tempList[j].relativePath = c + "Sound" + tempList[j].relativePath;
				}
				return tempList;
			}
			if (RouteFiles != 0 && ImageFiles < 20 && ObjectFiles == 0)
			{
				//If this is a ROUTE subfolder, we should not find any b3d objects, and
				//there should be less than 20 images
				for (int j = 0; j < tempList.Count; j++)
				{
					tempList[j].relativePath = c + "Route" + tempList[j].relativePath;
				}
				return tempList;
			}
			if ((ObjectFiles != 0 || RouteFiles != 0) && ImageFiles > 20 && (TrainFiles < 2 || ImageFiles > 200))
			{
				//We have csv or b3d files and more than 20 images
				//this means it's almost certainly an OBJECT subfolder

				//HACK:
				//If more than 200 images but also train stuff, assume it's a route object folder containing a misplaced DLL or something
				//NWM.....
				for (int j = 0; j < tempList.Count; j++)
				{
					tempList[j].relativePath = c + "Object" + tempList[j].relativePath;
				}
				return tempList;
			}
			//Can't decide, so just return the base list......
			return tempList;
		}

		/// <summary>This function cleans empty subdirectories after the uninstallation of a package</summary>
		/// <param name="currentDirectory">The directory to clean (Normally the root package install directory)</param>
		/// <param name="Result">The results output string</param>
		public static void cleanDirectory(string currentDirectory, ref string Result)
		{
			if (!Directory.Exists(currentDirectory))
			{
				return;
			}
			foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
			{
				cleanDirectory(directory, ref Result);		
			}
			IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(currentDirectory,"*", SearchOption.AllDirectories);
			if (!entries.Any())
			{
				Directory.Delete(currentDirectory, false);
				Result += currentDirectory + " : Empty directory deleted successfully. \r\n";
			}
			else
			{
				if (entries.Count() == 1)
				{
					if (File.Exists(currentDirectory + "thumbs.db"))
					{
						//thumbs.db files are auto-generated by Windows in any folder with pictures....
						File.Delete(currentDirectory + "thumbs.db");
						Directory.Delete(currentDirectory, false);
						Result += currentDirectory + " : Empty directory deleted successfully. \r\n";
					}
					else if (File.Exists(currentDirectory + ".DS_Store"))
					{
						//Hidden file auto-generated by the MacOS finder....
						File.Delete(currentDirectory + ".DS_Store");
						Directory.Delete(currentDirectory, false);
						Result += currentDirectory + " : Empty directory deleted successfully. \r\n";
					}
				}
			}
		}
	}

}
