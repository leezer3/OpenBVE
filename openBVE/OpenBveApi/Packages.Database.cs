using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenBveApi.Packages
{
	/// <summary>Returns lists of all packages installed</summary>
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
	}

	/// <summary>Contains the database functions</summary>
	public class DatabaseFunctions
	{
		/// <summary>Checks whether uninstalling a package will break any dependancies</summary>
		/// <param name="currentDatabase">The database to check against</param>
		/// <param name="packagesToRemove">The list of packages that will be removed</param>
		/// <returns>A list of potentially broken packages</returns>
		public static List<Package> CheckUninstallDependancies(PackageDatabase currentDatabase, List<Package> packagesToRemove)
		{
			List<Package> brokenPackages = new List<Package>();
			foreach (Package Route in currentDatabase.InstalledRoutes)
			{
				foreach (Package dependancy in Route.Dependancies)
				{
					foreach (Package packageToRemove in packagesToRemove)
					{
						if (packageToRemove.GUID == dependancy.GUID)
						{
							brokenPackages.Add(Route);
							break;
						}
					}
				}
			}
			foreach (Package Train in currentDatabase.InstalledTrains)
			{
				foreach (Package dependancy in Train.Dependancies)
				{
					foreach (Package packageToRemove in packagesToRemove)
					{
						if (packageToRemove.GUID == dependancy.GUID)
						{
							brokenPackages.Add(Train);
							break;
						}
					}
				}
			}
			foreach (Package Other in currentDatabase.InstalledRoutes)
			{
				foreach (Package dependancy in Other.Dependancies)
				{
					foreach (Package packageToRemove in packagesToRemove)
					{
						if (packageToRemove.GUID == dependancy.GUID)
						{
							brokenPackages.Add(Other);
							break;
						}
					}
				}
			}
			return brokenPackages;
		}

		/// <summary>This function takes a list of files, and returns the files with corrected relative paths for compression or extraction</summary>
		/// <param name="tempList">The file list</param>
		/// <returns>The file list with corrected relative paths</returns>
		public static List<PackageFile> FindFileLocations(List<PackageFile> tempList)
		{
			//Now determine whether this is part of a recognised folder structure
			for (int i = 0; i < tempList.Count; i++)
			{
				if (tempList[i].relativePath.StartsWith("\\Railway", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith("\\Train", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith("\\Route", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith("\\Object", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (tempList[i].relativePath.StartsWith("\\Sound", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway" + tempList[j].relativePath;
					}
					return tempList;
				}

			}

			for (int i = 0; i < tempList.Count; i++)
			{
				var TestCase = tempList[i].absolutePath.Replace(tempList[i].absolutePath, "");
				if (TestCase.EndsWith("Railway\\", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Train\\", StringComparison.OrdinalIgnoreCase))
				{
					//Extraction path is the root folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Train" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Route\\", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway\\Route" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Object\\", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway\\Object" + tempList[j].relativePath;
					}
					return tempList;
				}
				if (TestCase.EndsWith("Sound\\", StringComparison.OrdinalIgnoreCase))
				{
					//Needs to be extracted to the root railway folder
					for (int j = 0; j < tempList.Count; j++)
					{
						tempList[j].relativePath = "\\Railway\\Sound" + tempList[j].relativePath;
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
					tempList[j].relativePath = "\\Railway\\Sound" + tempList[j].relativePath;
				}
				return tempList;
			}
			if (RouteFiles != 0 && ImageFiles < 20 && ObjectFiles == 0)
			{
				//If this is a ROUTE subfolder, we should not find any b3d objects, and
				//there should be less than 20 images
				for (int j = 0; j < tempList.Count; j++)
				{
					tempList[j].relativePath = "\\Railway\\Route" + tempList[j].relativePath;
				}
				return tempList;
			}
			if ((ObjectFiles != 0 || RouteFiles != 0) && ImageFiles > 20)
			{
				//We have csv or b3d files and more than 20 images
				//this means it's almost certainly an OBJECT subfolder
				for (int j = 0; j < tempList.Count; j++)
				{
					tempList[j].relativePath = "\\Railway\\Object" + tempList[j].relativePath;
				}
				return tempList;
			}
			return tempList;
		}
	}

}
