using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;

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
			internal string Train;
			/// <summary>The message displayed to the user when the compatible plugin is used for the first time</summary>
			/// eg. Control changes
			internal string Message;
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
				if (AvailableReplacementPlugins[i].Train == null || AvailableReplacementPlugins[i].Train == f)
				{
					if (CheckBlackList(AvailableReplacementPlugins[i].OriginalPlugin))
					{
						//If original plugin is in the blacklist
						Interface.AddMessage(Interface.MessageType.Warning, true, "The blacklisted train plugin " + fl + " has been replaced with the following compatible alternative: " + 
						AvailableReplacementPlugins[i].PluginName);
						PluginPath = OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, AvailableReplacementPlugins[i].PluginPath);
						if (!String.IsNullOrEmpty(AvailableReplacementPlugins[i].Message))
						{
							MessageBox.Show(AvailableReplacementPlugins[i].Message, Application.ProductName, MessageBoxButtons.OK,
								MessageBoxIcon.Information);
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
							Interface.AddMessage(Interface.MessageType.Warning, true, "The train plugin " + fl + " has been replaced with the following compatible alternative: " +
							AvailableReplacementPlugins[i].PluginName);
							PluginPath = OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, AvailableReplacementPlugins[i].PluginPath);
							if (!String.IsNullOrEmpty(AvailableReplacementPlugins[i].Message))
							{
								MessageBox.Show(AvailableReplacementPlugins[i].Message, Application.ProductName, MessageBoxButtons.OK,
									MessageBoxIcon.Information);
							}
							return true;
						}
					}
				}
			}
			return false;

		}

		/// <summary>Loads the database of blacklisted plugins from disk</summary>
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
			currentXML.Load(databasePath);
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/TrainPlugins/Replacements");
				//Check this file actually contains an openBVE plugin blacklist
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.HasChildNodes)
						{
							ReplacementPlugin r = new ReplacementPlugin();
							bool ch1 = false;
							foreach (XmlNode c in n.ChildNodes)
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
										r.Train = c.InnerText;
										ch1 = true;
										break;
									case "message":
										r.Message = c.InnerText;
										ch1 = true;
										break;
								}
							}
							if (ch1 && r.OriginalPlugin != null)
							{
								AvailableReplacementPlugins.Add(r);
							}
						}
					}
				}
			}
		}
	}
}
