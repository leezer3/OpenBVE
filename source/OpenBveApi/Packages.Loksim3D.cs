using System;
using System.Xml;

namespace OpenBveApi.Packages
{
	class LoksimPackage
	{
		internal static Package Parse(XmlDocument currentXML, string fileName, ref string packageImagePath)
		{
			Package currentPackage = new Package();
			if (currentXML.DocumentElement == null)
			{
				//Null XML
				return currentPackage;
			}
			currentPackage.PackageType = PackageType.Loksim3D;
			currentPackage.GUID = Guid.NewGuid().ToString();
			//As we're using the filename as the package name, remove standard copy markers
			if (fileName.EndsWith(" - Copy", StringComparison.InvariantCultureIgnoreCase))
			{
				fileName = fileName.Substring(0, fileName.Length - 7);
			}
			if (fileName.EndsWith(" (1)"))
			{
				fileName = fileName.Substring(0, fileName.Length - 4);
			}
			currentPackage.Name = fileName;
			XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/PackageInfo");
			//Check this file actually contains Loksim3D object nodes
			if (DocumentNodes != null)
			{
				foreach (XmlNode baseNode in DocumentNodes)
				{
					foreach (XmlNode node in baseNode.ChildNodes)
					{
						switch (node.Name)
						{
							case "Props":
								if (node.Attributes != null)
								{
									foreach (XmlAttribute attribute in node.Attributes)
									{
										switch (attribute.Name)
										{
											case "FileAuthor":
												currentPackage.Author = attribute.InnerText;
												break;
											case "FileInfo":
												currentPackage.Description = attribute.InnerText;
												break;
											case "FilePicture":
												packageImagePath = attribute.InnerText;
												break;
										}
									}
								}
								break;
							case "VersionInfo":
								try
								{
									int v;
									int.TryParse(node.Attributes["Code"].Value, out v);
									currentPackage.PackageVersion = new Version(0,0,v);
								}
								catch
								{
								}
								break;
							case "DeleteFiles":
								break;
							case "DeinstallPackages":
								break;
						}
					}
				}
			}
			return currentPackage;
		}
	}
}
