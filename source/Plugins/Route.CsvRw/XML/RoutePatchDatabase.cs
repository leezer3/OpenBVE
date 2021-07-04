using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;
using XmlElement = System.Xml.XmlElement;

namespace CsvRwRouteParser
{
	class RoutePatchDatabaseParser
	{
		internal static void LoadRoutePatchDatabase(ref Dictionary<string, RoutefilePatch> routePatches, string databaseFile = "")
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();

			if (databaseFile == string.Empty)
			{
				databaseFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility\\RoutePatches"), "database.xml");
				if (!File.Exists(databaseFile))
				{
					return;
				}
			}
			currentXML.Load(databaseFile);
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/RoutePatches");
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
									case "Patch":
										if (childNode.HasChildNodes)
										{
											ParsePatchNode(childNode, ref routePatches);
										}
										break;
									case "PatchList":
										string folder = System.IO.Path.GetDirectoryName(databaseFile);
										string newFile = Path.CombineFile(folder, childNode.InnerText);
										if (File.Exists(newFile))
										{
											LoadRoutePatchDatabase(ref routePatches, newFile);
										}
										break;
								}
								
							}
						}
					}
				}
			}
		}

		internal static void ParsePatchNode(XmlNode node, ref Dictionary<string, RoutefilePatch> routeFixes)
		{
			RoutefilePatch currentPatch = new RoutefilePatch();
			string currentHash = string.Empty;
			foreach (XmlElement childNode in node.ChildNodes.OfType<XmlElement>())
			{
				string t;
				switch (childNode.Name)
				{
					case "Hash":
						currentHash = childNode.InnerText;
						break;
					case "FileName":
						currentPatch.FileName = childNode.InnerText;
						break;
					case "LineEndingFix":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.LineEndingFix = true;
						}
						else
						{
							currentPatch.LineEndingFix = false;
						}
						break;
					case "IgnorePitchRoll":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.IgnorePitchRoll = true;
						}
						else
						{
							currentPatch.IgnorePitchRoll = false;
						}
						break;
					case "LogMessage":
						currentPatch.LogMessage = childNode.InnerText;
						break;
					case "CylinderHack":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.CylinderHack = true;
						}
						else
						{
							currentPatch.CylinderHack = false;
						}
						break;
					case "Expression":
						t = childNode.Attributes["Number"].InnerText;
						int expressionNumber;
						if (NumberFormats.TryParseIntVb6(t, out expressionNumber))
						{
							currentPatch.ExpressionFixes.Add(expressionNumber, childNode.InnerText);
						}
						break;
					case "XParser":
						switch (childNode.InnerText.ToLowerInvariant())
						{
							case "original":
								currentPatch.XParser = XParsers.Original;
								break;
							case "new":
								currentPatch.XParser = XParsers.NewXParser;
								break;
							case "assimp":
								currentPatch.XParser = XParsers.Assimp;
								break;
						}
						break;
					case "DummyRailTypes":
						string[] splitString = childNode.InnerText.Split(',');
						for (int i = 0; i < splitString.Length; i++)
						{
							int rt;
							if (NumberFormats.TryParseIntVb6(splitString[i], out rt))
							{
								currentPatch.DummyRailTypes.Add(rt);
							}
						}
						break;
					case "DummyGroundTypes":
						splitString = childNode.InnerText.Split(',');
						for (int i = 0; i < splitString.Length; i++)
						{
							int gt;
							if (NumberFormats.TryParseIntVb6(splitString[i], out gt))
							{
								currentPatch.DummyGroundTypes.Add(gt);
							}
						}
						break;
					case "Derailments":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "0" || t == "false")
						{
							currentPatch.Derailments = false;
						}
						else
						{
							currentPatch.Derailments = true;
						}
						break;
					case "Toppling":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "0" || t == "false")
						{
							currentPatch.Derailments = false;
						}
						else
						{
							currentPatch.Derailments = true;
						}
						break;
					case "SignalSet":
						string signalFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility\\Signals"), childNode.InnerText.Trim());
						if (File.Exists(signalFile))
						{
							currentPatch.CompatibilitySignalSet = signalFile;
						}
						break;
					case "AccurateObjectDisposal":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.AccurateObjectDisposal = true;
						}
						else
						{
							currentPatch.AccurateObjectDisposal = false;
						}
						break;
					case "SplitLineHack":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.SplitLineHack = true;
						}
						else
						{
							currentPatch.SplitLineHack = false;
						}
						break;
					case "AllowTrackPositionArguments":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.AllowTrackPositionArguments = true;
						}
						else
						{
							currentPatch.AllowTrackPositionArguments = false;
						}
						break;
					case "DisableSemiTransparentFaces":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.DisableSemiTransparentFaces = true;
						}
						else
						{
							currentPatch.DisableSemiTransparentFaces = false;
						}
						break;
					case "ReducedColorTransparency":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.ReducedColorTransparency = true;
						}
						else
						{
							currentPatch.ReducedColorTransparency = false;
						}
						break;
					case "ViewingDistance":
						if (!int.TryParse(childNode.InnerText.Trim(), out currentPatch.ViewingDistance))
						{
							currentPatch.ViewingDistance = int.MaxValue;
						}
						break;
					case "Incompatible":
						t = childNode.InnerText.Trim().ToLowerInvariant();
						if (t == "1" || t == "true")
						{
							currentPatch.Incompatible = true;
						}
						else
						{
							currentPatch.Incompatible = false;
						}
						break;
				}
			}

			if (!routeFixes.ContainsKey(currentHash))
			{
				routeFixes.Add(currentHash, currentPatch);
			}
			else
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The RoutePatches database contains a duplicate entry with hash " + currentHash);
			}
		}
	}
}
