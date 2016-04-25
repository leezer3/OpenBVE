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
}
