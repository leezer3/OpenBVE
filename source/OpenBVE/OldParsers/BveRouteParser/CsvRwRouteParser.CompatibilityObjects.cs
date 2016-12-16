using System;
using System.Xml;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <summary>Locates the absolute on-disk path of the object to be loaded</summary>
		/// <param name="fileName">The object's file-name</param>
		/// <param name="objectPath">The path to the objects directory for this route</param>
		internal static bool LocateObject(ref string fileName, string objectPath)
		{
			string n;
			try
			{
				//Catch completely malformed path references
				n = OpenBveApi.Path.CombineFile(objectPath, fileName);
			}
			catch
			{
				return false;
			}
			if (System.IO.File.Exists(n))
			{
				fileName = n;
				//The object exists, and does not require a compatibility object
				return true;
			}
			for (int i = 0; i < CompatibilityObjects.AvailableReplacements.Length; i++)
			{
				if (CompatibilityObjects.AvailableReplacements[i].ObjectName.ToLowerInvariant() == fileName.ToLowerInvariant())
				{
					fileName = CompatibilityObjects.AvailableReplacements[i].ReplacementPath;
					if (!string.IsNullOrEmpty(CompatibilityObjects.AvailableReplacements[i].Message))
					{
						Interface.AddMessage(Interface.MessageType.Warning, false, CompatibilityObjects.AvailableReplacements[i].Message);
					}
					CompatibilityObjectsUsed++;
					return true;
				}
			}
			return false;
		}

		internal static int CompatibilityObjectsUsed = 0;
	}

	internal class CompatibilityObjects
	{
		internal class ReplacementObject
		{
			/// <summary>The filename of the original object to be replaced</summary>
			internal string ObjectName;

			/// <summary>The absolute on-disk path of the replacement object</summary>
			internal string ReplacementPath;

			internal string Message;

			/// <summary>Creates a new replacement object</summary>
			internal ReplacementObject()
			{
				ObjectName = string.Empty;
				ReplacementPath = string.Empty;
			}
		}

		internal static ReplacementObject[] AvailableReplacements;

		internal static void LoadCompatibilityObjects(string fileName)
		{
			string d = System.IO.Path.GetDirectoryName(fileName);
			AvailableReplacements = new ReplacementObject[0];
			XmlDocument currentXML = new XmlDocument();
			currentXML.Load(fileName);
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Compatibility/Object");
				//Check this file actually contains OpenBVE compatibility nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.HasChildNodes)
						{
							ReplacementObject o = new ReplacementObject();
							foreach (XmlNode c in n.ChildNodes)
							{
								switch (c.Name.ToLowerInvariant())
								{
									case "name":
										o.ObjectName = c.InnerText.Trim();
										break;
									case "path":
											string f = OpenBveApi.Path.CombineFile(d, c.InnerText.Trim());
											if (System.IO.File.Exists(f))
											{
												o.ReplacementPath = f;
											}
										break;
									case "message":
										o.Message = c.InnerText.Trim();
										break;
								}
							}
							if (o.ObjectName != string.Empty && o.ReplacementPath != string.Empty)
							{
								int i = AvailableReplacements.Length;
								Array.Resize(ref AvailableReplacements, i +1);
								AvailableReplacements[i] = o;
							}
						}
					}
				}
			}
		}
	}
}
