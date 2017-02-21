using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Xml;


#pragma warning disable 660,661

namespace OpenBve
{
	internal static partial class PluginManager
	{
		internal struct BlackListEntry
		{
			/// <summary>The file length of the blacklisted plugin</summary>
			internal double FileLength;
			/// <summary>The file name of the blacklisted plugin</summary>
			internal string FileName;
			/// <summary>The MD5 sum of the plugin file</summary>
			internal string MD5;
			/// <summary>The train-name if this plugin is blacklisted for one train only</summary>
			internal string Train;
			/// <summary>A textual string describing the reason this plugin is blacklisted</summary>
			internal string Reason;
			/// <summary>Whether all possible versions of this plugin are blacklisted (Overrides MD5 and length)</summary>
			internal bool AllVersions;

			public static bool operator ==(BlackListEntry a, BlackListEntry b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(BlackListEntry a, BlackListEntry b)
			{
				return !(a == b);
			}
		}

		/// <summary>The currently blacklisted plugins</summary>
		internal static List<BlackListEntry> BlackListedPlugins;

		/// <summary>Checks whether the specified plugin is blacklisted</summary>
		/// <returns>True if blacklisted, false otherwise</returns>
		internal static bool CheckBlackList(string filePath, string trainFolder)
		{
			string pluginTitle = System.IO.Path.GetFileName(filePath);
			if (BlackListedPlugins == null || BlackListedPlugins.Count == 0)
			{
				return false;
			}
			var n = System.IO.Path.GetFileName(filePath);
			for (int i = 0; i < BlackListedPlugins.Count; i++)
			{
				if (BlackListedPlugins[i].FileName == n.ToLowerInvariant())
				{
					if (BlackListedPlugins[i].AllVersions == true)
					{
						Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " is blacklisted for the following reason:");
						if (BlackListedPlugins[i].Reason == String.Empty)
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "No reason specified.");
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, BlackListedPlugins[i].Reason);
						}
						return true;
					}
					var fi = new FileInfo(filePath);
					if (fi.Length == BlackListedPlugins[i].FileLength && (BlackListedPlugins[i].Train == null || trainFolder.ToLowerInvariant() == BlackListedPlugins[i].Train.ToLowerInvariant()))
					{
						var md5 = MD5.Create();
						using (var stream = File.OpenRead(filePath))
						{
							md5.ComputeHash(stream);
						}
						//String.Empty is required, as otherwise it adds a null character.....
						string s = BitConverter.ToString(md5.Hash).Replace("-", String.Empty).ToUpper();
						if (s == BlackListedPlugins[i].MD5)
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " is blacklisted for the following reason:");
							if (BlackListedPlugins[i].Reason == String.Empty)
							{
								Interface.AddMessage(Interface.MessageType.Error, true, "No reason specified.");
							}
							else
							{
								Interface.AddMessage(Interface.MessageType.Error, true, BlackListedPlugins[i].Reason);
							}
							return true;
						}
					}

					
				}
			}
			return false;
		}

		/// <summary>Checks whether the specified plugin entry is in the blacklist database</summary>
		/// <param name="plugin">The plugin entry</param>
		internal static bool CheckBlackList(BlackListEntry plugin)
		{
			if (BlackListedPlugins == null || BlackListedPlugins.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < BlackListedPlugins.Count; i++)
			{
				if (plugin == BlackListedPlugins[i])
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Loads the database of blacklisted plugins from disk (Called once on startup)</summary>
		/// <param name="databasePath">The database path</param>
		internal static void LoadBlackListDatabase(string databasePath)
		{
			if (!System.IO.File.Exists(databasePath))
			{
				return;
			}
			BlackListedPlugins = new List<BlackListEntry>();
			XmlDocument currentXML = new XmlDocument();
			//Load the XML file 
			try
			{
				currentXML.Load(databasePath);
			}
			catch
			{
				return;
			}
			LoadBlackListDatabase(currentXML);
		}

		/// <summary>Adds blacklisted plugins to the list from an XML document</summary>
		/// <param name="currentXML">The XML document to load</param>
		internal static void LoadBlackListDatabase(XmlDocument currentXML)
		{
			if (BlackListedPlugins == null)
			{
				BlackListedPlugins = new List<BlackListEntry>();
			}
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/TrainPlugins/Blacklist");
				//Check this file actually contains an openBVE plugin blacklist
				if (DocumentNodes != null)
				{
					for(int i = 0; i < DocumentNodes.Count; i++)
					{
						BlackListEntry entry = ParseBlackListEntry(DocumentNodes[i]);
						if (entry != new BlackListEntry() && !BlackListedPlugins.Contains(entry))
						{
							BlackListedPlugins.Add(entry);
						}
					}
					
				}
			}
		}

		internal static BlackListEntry ParseBlackListEntry(XmlNode node)
		{
			if (node.HasChildNodes)
			{
				BlackListEntry p = new BlackListEntry();
				bool ch = false;
				foreach (XmlNode c in node.ChildNodes)
				{
					switch (c.Name.ToLowerInvariant())
					{
						case "filelength":
							Double.TryParse(c.InnerText, out p.FileLength);
							ch = true;
							break;
						case "filename":
							p.FileName = c.InnerText.ToLowerInvariant();
							ch = true;
							break;
						case "md5":
							p.MD5 = c.InnerText.ToUpper().Trim();
							ch = true;
							break;
						case "train":
							p.Train = c.InnerText;
							ch = true;
							break;
						case "reason":
							p.Reason = c.InnerText;
							ch = true;
							break;
						case "allversions":
							string s = c.InnerText.ToLowerInvariant();
							if (s == "true" || s == "1")
							{
								p.AllVersions = true;
							}
							break;
					}
				}
				if (ch && (p.MD5 != string.Empty || p.AllVersions == true))
				{
					return p;
				}
			}
			return new BlackListEntry();
		}

		internal static bool RemoveBlackListEntry(BlackListEntry entry)
		{
			if (BlackListedPlugins == null)
			{
				return false;
			}
			for (int i = BlackListedPlugins.Count -1; i > 0; i--)
			{
				if (BlackListedPlugins[i] == entry)
				{
					BlackListedPlugins.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>Saves the list of blacklisted plugins to disk</summary>
		internal static void WriteBlackListDatabase()
		{
			if (BlackListedPlugins == null || BlackListedPlugins.Count == 0)
			{
				return;
			}
			//This isn't a public class, hence building the XML manually for write out
			XmlDocument currentXML = new XmlDocument();
			XmlElement rootElement = (XmlElement)currentXML.AppendChild(currentXML.CreateElement("openBVE"));
			XmlElement firstElement = (XmlElement)rootElement.AppendChild(currentXML.CreateElement("TrainPlugins"));
			for (int i = 0; i < BlackListedPlugins.Count; i++)
			{
				XmlElement entry = (XmlElement)firstElement.AppendChild(currentXML.CreateElement("BlackList"));
				entry.AppendChild(currentXML.CreateElement("FileLength")).InnerText = BlackListedPlugins[i].FileLength.ToString(CultureInfo.InvariantCulture);
				entry.AppendChild(currentXML.CreateElement("FileName")).InnerText = BlackListedPlugins[i].FileName;
				entry.AppendChild(currentXML.CreateElement("MD5")).InnerText = BlackListedPlugins[i].MD5;
				entry.AppendChild(currentXML.CreateElement("Train")).InnerText = BlackListedPlugins[i].Train;
				entry.AppendChild(currentXML.CreateElement("Reason")).InnerText = BlackListedPlugins[i].Reason;
				entry.AppendChild(currentXML.CreateElement("AllVersions")).InnerText = BlackListedPlugins[i].AllVersions ? "true" : "false";
			}
			using (StreamWriter sw = new StreamWriter(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("PluginDatabase"), "blacklist.xml")))
			{
				currentXML.Save(sw);
			}
		}
	}
}
