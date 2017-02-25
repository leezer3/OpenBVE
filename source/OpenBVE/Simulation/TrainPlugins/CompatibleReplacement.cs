using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using OpenBveApi.Colors;

namespace OpenBve
{
	internal static partial class PluginManager
	{
		internal struct ReplacementPlugin
		{
			/// <summary>The original plugin this is to replace</summary>
			internal BlackListEntry OriginalPlugin;

			/// <summary>The name of the new plugin</summary>
			internal string PluginName;

			/// <summary>The path of the compatible plugin to use</summary>
			internal string PluginPath;

			/// <summary>The train-name if this replacement is to be used for only one train, or null for all</summary>
			internal string[] Trains;

			/// <summary>The message displayed to the user when the compatible plugin is used for the first time</summary>
			/// eg. Control changes
			internal string Message;

			/// <summary>The required configuration file</summary>
			/// If applicable
			internal string ConfigurationFile;

			public static bool operator !=(ReplacementPlugin a, ReplacementPlugin b)
			{
				if ((a.Message ?? "") != (b.Message ?? "")) return true;
				if (a.OriginalPlugin != b.OriginalPlugin) return true;
				if (a.PluginPath.Trim() != b.PluginPath.Trim()) return true;
				if (a.Trains != null && b.Trains != null)
				{
					if (a.Trains.Length == b.Trains.Length)
					{
						if (a.Trains.Length < 0)
						{
							for (int i = 0; i < a.Trains.Length; i++)
							{
								if (a.Trains[i] != b.Trains[i])
								{
									return true;
								}
							}
						}
					}
					else
					{
						return true;
					}
				}
				if ((a.ConfigurationFile ?? "") != (b.ConfigurationFile ?? "")) return true;
				return false;
			}

			public static bool operator ==(ReplacementPlugin a, ReplacementPlugin b)
			{
				if ((a.Message ?? "") != (b.Message ?? "")) return false;
				if (a.OriginalPlugin != b.OriginalPlugin) return false;
				if (a.PluginPath.Trim() != b.PluginPath.Trim()) return false;
				if (a.Trains != null && b.Trains != null)
				{
					if (a.Trains.Length != b.Trains.Length) return false;
					if (a.Trains.Length < 0)
					{
						for (int i = 0; i < a.Trains.Length; i++)
						{
							if (a.Trains[i] != b.Trains[i])
							{
								return false;
							}
						}
					}
				}
				if ((a.ConfigurationFile ?? "") != (b.ConfigurationFile ?? "")) return false;
				return true;
			}
		}

		/// <summary>Holds the list of available replacement plugins</summary>
		internal static List<ReplacementPlugin> AvailableReplacementPlugins;

		internal static bool FindReplacementPlugin(ref string PluginPath)
		{
			if (AvailableReplacementPlugins.Count == 0)
			{
				PluginPath = null;
				return false;
			}
			var f = System.IO.Path.GetDirectoryName(PluginPath);
			var fl = System.IO.Path.GetFileName(PluginPath);
			if (fl == "OpenBveAts.dll")
			{
				return false;
			}

			for (int i = 0; i < AvailableReplacementPlugins.Count; i++)
			{
				if (AvailableReplacementPlugins[i].Trains == null || AvailableReplacementPlugins[i].Trains.Contains(f.Split(Path.DirectorySeparatorChar).Last()))
				{
					if (CheckBlackList(AvailableReplacementPlugins[i].OriginalPlugin))
					{
						//The original plugin is contained within the blacklist
						Interface.AddMessage(Interface.MessageType.Warning, true, "The blacklisted train plugin " + fl + " has been replaced with the following compatible alternative: " + 
						AvailableReplacementPlugins[i].PluginName);
						string s = "The blacklisted train plugin " + fl + " has been replaced with the following compatible alternative: " + AvailableReplacementPlugins[i].PluginName;
						Loading.MessageQueue.Add(new Game.Message(s, Game.MessageDependency.None, MessageColor.Green, 10.0));
						Loading.PluginMessageColor = MessageColor.Green;
						PluginPath = OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, AvailableReplacementPlugins[i].PluginPath);
						if (!String.IsNullOrEmpty(AvailableReplacementPlugins[i].Message))
						{
							MessageBox.Show(AvailableReplacementPlugins[i].Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						return true;
					}

					//Not in the blacklist, so check plugin length etc.
					var fi = new FileInfo(PluginPath);
					if (fi.Length == AvailableReplacementPlugins[i].OriginalPlugin.FileLength)
					{
						var md5 = MD5.Create();
						using (var stream = File.OpenRead(PluginPath))
						{
							md5.ComputeHash(stream);
						}
						//String.Empty is required, as otherwise it adds a null character.....
						string s = BitConverter.ToString(md5.Hash).Replace("-", String.Empty).ToUpper();
						if (s.ToLowerInvariant() == AvailableReplacementPlugins[i].OriginalPlugin.MD5.ToLowerInvariant())
						{
							if (AvailableReplacementPlugins[i].ConfigurationFile != null)
							{
								try
								{
									switch (AvailableReplacementPlugins[i].PluginName.ToLowerInvariant())
									{
										case "uktrainsys":
											//Exact case of configuration file required on Linux
											//TODO: Fix UKTrainsys to use API path resolution
											System.IO.File.Copy(AvailableReplacementPlugins[i].ConfigurationFile, OpenBveApi.Path.CombineFile(f, "UkTrainSys.cfg"));
											break;
										case "bvec_ats":
											System.IO.File.Copy(AvailableReplacementPlugins[i].ConfigurationFile, OpenBveApi.Path.CombineFile(f, "bvec_ats.cfg"));
											break;
									}
									
								}
								catch
								{
									Interface.AddMessage(Interface.MessageType.Error, true, "A compatible alternative for train plugin " + fl + " was found, but an error occured whilst attempting to copy a required configuration file.");
									return false;
								}
							}
							Interface.AddMessage(Interface.MessageType.Warning, true, "The train plugin " + fl + " has been replaced with the following compatible alternative: " +
							AvailableReplacementPlugins[i].PluginName);
							string t = "The train plugin " + fl + " has been replaced with the following compatible alternative: " + AvailableReplacementPlugins[i].PluginName;
							Loading.MessageQueue.Add(new Game.Message(t, Game.MessageDependency.None, MessageColor.Green, 10.0));
							Loading.PluginMessageColor = MessageColor.Green;
							PluginPath = OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, AvailableReplacementPlugins[i].PluginPath);
							
							if (!String.IsNullOrEmpty(AvailableReplacementPlugins[i].Message))
							{
								MessageBox.Show(AvailableReplacementPlugins[i].Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
							}
							return true;
						}
					}
				}
			}
			return false;

		}

		/// <summary>Loads the database of compatible replacement plugins from disk</summary>
		/// <param name="databasePath">The database path</param>
		internal static void LoadReplacementDatabase(string databasePath)
		{
			if (!System.IO.File.Exists(databasePath))
			{
				return;
			}
			AvailableReplacementPlugins = new List<ReplacementPlugin>();
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
			LoadReplacementDatabase(currentXML);
		}

		/// <summary>Adds compatible replacement plugins to the list from an XML document</summary>
		/// <param name="currentXML">The XML document to load</param>
		internal static void LoadReplacementDatabase(XmlDocument currentXML)
		{
			if (AvailableReplacementPlugins == null)
			{
				AvailableReplacementPlugins = new List<ReplacementPlugin>();
			}
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/TrainPlugins/Replacements");
				//Check this file actually contains an openBVE plugin blacklist
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						ReplacementPlugin plugin = ParseReplacementPlugin(n);
						if (AvailableReplacementPlugins.Count == 0)
						{
							AvailableReplacementPlugins.Add(plugin);
						}
						else
						{
							for (int i = 0; i < AvailableReplacementPlugins.Count; i++)
							{
								//List.Contains() doesn't use the custom equality operator
								if (AvailableReplacementPlugins[i] == plugin)
								{
									break;
								}
								if (i == AvailableReplacementPlugins.Count -1)
								{
									AvailableReplacementPlugins.Add(plugin);
								}
							}
						}
					}
				}
			}
		}

		internal static ReplacementPlugin ParseReplacementPlugin(XmlNode node)
		{
			if (node.HasChildNodes)
			{
				ReplacementPlugin r = new ReplacementPlugin();
				bool ch1 = false;
				foreach (XmlNode c in node.ChildNodes)
				{
					switch (c.Name.ToLowerInvariant())
					{
						case "plugin":
							if (c.HasChildNodes)
							{
								BlackListEntry p = new BlackListEntry();
								bool ch = false;
								foreach (XmlNode cn in c.ChildNodes)
								{
									switch (cn.Name.ToLowerInvariant())
									{
										case "filelength":
											Double.TryParse(cn.InnerText, out p.FileLength);
											ch = true;
											break;
										case "filename":
											p.FileName = cn.InnerText;
											ch = true;
											break;
										case "md5":
											p.MD5 = cn.InnerText;
											ch = true;
											break;
										case "train":
											p.Train = cn.InnerText;
											ch = true;
											break;
										case "reason":
											p.Reason = cn.InnerText;
											ch = true;
											break;
									}
								}
								if (ch && p.MD5 != string.Empty)
								{
									r.OriginalPlugin = p;
								}
							}
							break;
						case "name":
							r.PluginName = c.InnerText;
							ch1 = true;
							break;
						case "path":
							r.PluginPath = c.InnerText;
							ch1 = true;
							break;
						case "train":
							r.Trains = c.InnerText.Split(';');
							ch1 = true;
							break;
						case "configuration":
							r.ConfigurationFile = c.InnerText;
							break;
						case "message":
							r.Message = c.InnerText;
							ch1 = true;
							break;
					}
				}
				if (ch1 && r.OriginalPlugin != null)
				{
					return r;
				}
			}
			return new ReplacementPlugin();
		}

		internal static bool RemoveReplacementPlugin(ReplacementPlugin plugin)
		{
			if (AvailableReplacementPlugins == null)
			{
				return false;
			}
			int t = AvailableReplacementPlugins.Count -1;
			for (int i = 0; i < AvailableReplacementPlugins.Count; i++)
			{

				if (plugin == AvailableReplacementPlugins[i])
				{
					AvailableReplacementPlugins.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>Saves the list of available replacement plugins to disk</summary>
		internal static void WriteReplacementDatabase()
		{
			if (AvailableReplacementPlugins == null || AvailableReplacementPlugins.Count == 0)
			{
				try
				{
					File.Delete(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("PluginDatabase"),
						"compatiblereplacements.xml"));
				}
				catch {}
				return;
			}
			//This isn't a public class, hence building the XML manually for write out
			XmlDocument currentXML = new XmlDocument();
			XmlElement rootElement = (XmlElement)currentXML.AppendChild(currentXML.CreateElement("openBVE"));
			XmlElement firstElement = (XmlElement)rootElement.AppendChild(currentXML.CreateElement("TrainPlugins"));
			for (int i = 0; i < AvailableReplacementPlugins.Count; i++)
			{
				XmlElement entry = (XmlElement)firstElement.AppendChild(currentXML.CreateElement("Replacements"));
				XmlElement plugin = (XmlElement)entry.AppendChild(currentXML.CreateElement("Plugin"));
					plugin.AppendChild(currentXML.CreateElement("FileLength")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.FileLength.ToString(CultureInfo.InvariantCulture);
					plugin.AppendChild(currentXML.CreateElement("FileName")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.FileName;
					plugin.AppendChild(currentXML.CreateElement("MD5")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.MD5;
					plugin.AppendChild(currentXML.CreateElement("Train")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.Train;
					plugin.AppendChild(currentXML.CreateElement("Reason")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.Reason;
					plugin.AppendChild(currentXML.CreateElement("AllVersions")).InnerText = AvailableReplacementPlugins[i].OriginalPlugin.AllVersions ? "true" : "false";
				entry.AppendChild(currentXML.CreateElement("Name")).InnerText = AvailableReplacementPlugins[i].PluginName;
				entry.AppendChild(currentXML.CreateElement("Path")).InnerText = AvailableReplacementPlugins[i].PluginPath;
				entry.AppendChild(currentXML.CreateElement("Train")).InnerText = AvailableReplacementPlugins[i].Trains == null ? String.Empty : string.Join(",", AvailableReplacementPlugins[i].Trains);
				entry.AppendChild(currentXML.CreateElement("Configuration")).InnerText = AvailableReplacementPlugins[i].ConfigurationFile;
				entry.AppendChild(currentXML.CreateElement("Message")).InnerText = AvailableReplacementPlugins[i].Message;
			}
			using (StreamWriter sw = new StreamWriter(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("PluginDatabase"), "compatiblereplacements.xml")))
			{
				currentXML.Save(sw);
			}
		}
	}
}
