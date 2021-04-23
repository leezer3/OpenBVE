using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using Path = OpenBveApi.Path;
using XmlElement = System.Xml.XmlElement;

namespace MechanikRouteParser
{
	class RoutePropertiesDatabaseParser
	{
		internal static void LoadRoutePropertyDatabase(ref Dictionary<string, RouteProperties> routeProperties, ref List<string> moduleList, string databaseFile = "")
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();

			if (databaseFile == string.Empty)
			{
				databaseFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility\\Mechanik"), "database.xml");
				if (!File.Exists(databaseFile))
				{
					return;
				}
			}
			currentXML.Load(databaseFile);
			//Check for null
			if (currentXML.DocumentElement != null)
			{

				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/MechanikRoute");
				//Check this file actually contains OpenBVE route patch definition nodes
				if (DocumentNodes != null)
				{
					for (int i = 0; i < DocumentNodes.Count; i++)
					{
						if (DocumentNodes[i].HasChildNodes)
						{
							foreach (XmlElement childNode in DocumentNodes[i].ChildNodes.OfType<XmlElement>())
							{
								switch (childNode.Name)
								{
									case "Route":
										if (childNode.HasChildNodes)
										{
											ParsePropertiesNode(childNode, ref routeProperties);
										}
										break;
									case "PropertiesList":
										string folder = System.IO.Path.GetDirectoryName(databaseFile);
										string newFile = Path.CombineFile(folder, childNode.InnerText);
										if (File.Exists(newFile))
										{
											LoadRoutePropertyDatabase(ref routeProperties, ref moduleList, newFile);
										}
										break;
								}
								
							}
						}
					}
				}
				DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/MechanikModule");
				if (DocumentNodes != null)
				{
					for (int i = 0; i < DocumentNodes.Count; i++)
					{
						if (DocumentNodes[i].HasChildNodes)
						{
							foreach (XmlElement childNode in DocumentNodes[i].ChildNodes.OfType<XmlElement>())
							{
								switch (childNode.Name)
								{
									case "Module":
										if (!moduleList.Contains(childNode.InnerText))
										{
											moduleList.Add(childNode.InnerText);
										}
										break;
									case "ModuleList":
										string folder = System.IO.Path.GetDirectoryName(databaseFile);
										string newFile = Path.CombineFile(folder, childNode.InnerText);
										if (File.Exists(newFile))
										{
											LoadRoutePropertyDatabase(ref routeProperties, ref moduleList, newFile);
										}
										break;
								}
								
							}
						}
					}
				}
			}
		}

		internal static void ParsePropertiesNode(XmlNode node, ref Dictionary<string, RouteProperties> routeProperties)
		{
			RouteProperties currentProperties = new RouteProperties();
			string currentHash = string.Empty;
			foreach (XmlElement childNode in node.ChildNodes.OfType<XmlElement>())
			{
				switch (childNode.Name)
				{
					case "Hash":
						currentHash = childNode.InnerText;
						break;
					case "Description":
						currentProperties.Description = childNode.InnerText;
						break;
					case "StationNames":
						currentProperties.StationNames = childNode.InnerText.Split(',');
						break;
					case "DefaultTrain":
						currentProperties.DefaultTrain = childNode.InnerText;
						break;
					case "DepartureTimes":
						string[] splitTimes = childNode.InnerText.Split(',');
						currentProperties.DepartureTimes = new double[splitTimes.Length];
						for (int i = 0; i < splitTimes.Length; i++)
						{
							OpenBveApi.Time.TryParseTime(splitTimes[i], out currentProperties.DepartureTimes[i]);
						}
						break;
					case "Doors":
						string[] splitDoors = childNode.InnerText.Split(',');
						currentProperties.Doors = new DoorStates[splitDoors.Length];
						for (int i = 0; i < splitDoors.Length; i++)
						{
							if (!Enum.TryParse(splitDoors[i], true, out currentProperties.Doors[i]))
							{
								currentProperties.Doors[i] = DoorStates.None;
							}
						}
						break;
				}
			}

			if (!routeProperties.ContainsKey(currentHash))
			{
				routeProperties.Add(currentHash, currentProperties);
			}
			else
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The RouteProperties database contains a duplicate entry with hash " + currentHash);
			}
		}
	}
}
