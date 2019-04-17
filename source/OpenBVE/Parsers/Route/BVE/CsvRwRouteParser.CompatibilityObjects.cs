using System;
using System.Xml;
using System.Linq;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <summary>Locates the absolute on-disk path of the object to be loaded, or an available compatible replacement if not found</summary>
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

			if (Interface.CurrentOptions.EnableBveTsHacks)
			{
				string fn;
				//The Midland Suburban Line has a malformed object zip, so let's try again....
				if (fileName.StartsWith("Midland Suburban Line", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = "Midland Suburban Line Objects" + fileName.Substring(21);
					try
					{
						//Catch completely malformed path references
						n = OpenBveApi.Path.CombineFile(objectPath, fn);
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
				}
				//The Midland Suburban Line expects BRSema4Sigs to be placed in it's object folder, but we've probably got them elsewhere
				if (fileName.StartsWith(@"Midland Suburban Line\BrSema4Sigs", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = "BrSema4Sigs" + fileName.Substring(33);
					try
					{
						//Catch completely malformed path references
						n = OpenBveApi.Path.CombineFile(objectPath, fn);
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
				}

				
			}
			//We haven't found the object on-disk, so check the compatibility objects to see if a replacement is available
			for (int i = 0; i < CompatibilityObjects.AvailableReplacements.Length; i++)
			{
				if (CompatibilityObjects.AvailableReplacements[i].ObjectNames.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < CompatibilityObjects.AvailableReplacements[i].ObjectNames.Length; j++)
				{
					if (CompatibilityObjects.AvailableReplacements[i].ObjectNames[j].ToLowerInvariant() == fileName.ToLowerInvariant())
					{
						//Available replacement found
						fileName = CompatibilityObjects.AvailableReplacements[i].ReplacementPath;
						if (!string.IsNullOrEmpty(CompatibilityObjects.AvailableReplacements[i].Message))
						{
							Interface.AddMessage(MessageType.Warning, false, CompatibilityObjects.AvailableReplacements[i].Message);
						}
						CompatibilityObjectsUsed++;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Locates the absolute on-disk path of the object to be loaded, or an available compatible replacement if not found</summary>
		/// <param name="fileName">The object's file-name</param>
		/// <param name="objectPath">The path to the objects directory for this route</param>
		internal static bool LocateSound(ref string fileName, string objectPath)
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
			//We haven't found the object on-disk, so check the compatibility objects to see if a replacement is available
			for (int i = 0; i < CompatibilityObjects.AvailableSounds.Length; i++)
			{
				if (CompatibilityObjects.AvailableSounds[i].ObjectNames.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < CompatibilityObjects.AvailableSounds[i].ObjectNames.Length; j++)
				{
					if (CompatibilityObjects.AvailableSounds[i].ObjectNames[j].ToLowerInvariant() == fileName.ToLowerInvariant())
					{
						//Available replacement found
						fileName = CompatibilityObjects.AvailableSounds[i].ReplacementPath;
						if (!string.IsNullOrEmpty(CompatibilityObjects.AvailableSounds[i].Message))
						{
							Interface.AddMessage(MessageType.Warning, false, CompatibilityObjects.AvailableSounds[i].Message);
						}
						CompatibilityObjectsUsed++;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>The total number of compatability objects used by the current route</summary>
		internal static int CompatibilityObjectsUsed;
	}

	internal class CompatibilityObjects
	{
		/// <summary>Defines a replacement compatibility object</summary>
		internal class ReplacementObject
		{
			/// <summary>The filename of the original object to be replaced</summary>
			internal string[] ObjectNames;
			/// <summary>The absolute on-disk path of the replacement object</summary>
			internal string ReplacementPath;
			/// <summary>The message to be added to the log if this object is used</summary>
			internal string Message;
			/// <summary>Creates a new replacement object</summary>
			internal ReplacementObject()
			{
				ObjectNames = new string[0];
				ReplacementPath = string.Empty;
			}
		}
		/*
		 * Various auto-generated objects
		 */
		internal static ObjectManager.StaticObject SignalPost = null;
		internal static ObjectManager.StaticObject LimitPostStraight = null;
		internal static ObjectManager.StaticObject LimitPostLeft = null;
		internal static ObjectManager.StaticObject LimitPostRight = null;
		internal static ObjectManager.StaticObject LimitPostInfinite = null;
		internal static ObjectManager.StaticObject LimitOneDigit = null;
		internal static ObjectManager.StaticObject LimitTwoDigits = null;
		internal static ObjectManager.StaticObject LimitThreeDigits = null;
		internal static ObjectManager.StaticObject StopPost = null;
		internal static ObjectManager.StaticObject TransponderS = null;
		internal static ObjectManager.StaticObject TransponderSN = null;
		internal static ObjectManager.StaticObject TransponderFalseStart = null;
		internal static ObjectManager.StaticObject TransponderPOrigin = null;
		internal static ObjectManager.StaticObject TransponderPStop = null;

		internal static void LoadAutoGeneratedObjects(string CompatibilityFolder, out string LimitGraphicsPath)
		{
			string SignalPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
			SignalPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalPath, "signal_post.csv"), System.Text.Encoding.UTF8, false);
			string LimitPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Limits");
			LimitGraphicsPath = OpenBveApi.Path.CombineDirectory(LimitPath, "Graphics");
			LimitPostStraight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_straight.csv"), System.Text.Encoding.UTF8, false);
			LimitPostLeft = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_left.csv"), System.Text.Encoding.UTF8, false);
			LimitPostRight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_right.csv"), System.Text.Encoding.UTF8, false);
			LimitPostInfinite = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_infinite.csv"), System.Text.Encoding.UTF8, false);
			LimitOneDigit = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_1_digit.csv"), System.Text.Encoding.UTF8, false);
			LimitTwoDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_2_digits.csv"), System.Text.Encoding.UTF8, false);
			LimitThreeDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_3_digits.csv"), System.Text.Encoding.UTF8, false);
			StopPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(CompatibilityFolder, "stop.csv"), System.Text.Encoding.UTF8, false);
			string TransponderPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Transponders");
			TransponderS = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "s.csv"), System.Text.Encoding.UTF8, false);
			TransponderSN = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "sn.csv"), System.Text.Encoding.UTF8, false);
			TransponderFalseStart = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "falsestart.csv"), System.Text.Encoding.UTF8, false);
			TransponderPOrigin = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "porigin.csv"), System.Text.Encoding.UTF8, false);
			TransponderPStop = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "pstop.csv"), System.Text.Encoding.UTF8, false);
		}

		/// <summary>The array containing the paths to all available replacement objects</summary>
		internal static ReplacementObject[] AvailableReplacements = new ReplacementObject[0];

		internal static ReplacementObject[] AvailableSounds = new ReplacementObject[0];

		/// <summary>Loads the available compatibility object database</summary>
		/// <param name="fileName">The database file</param>
		internal static void LoadCompatibilityObjects(string fileName)
		{
			if (!System.IO.File.Exists(fileName))
			{
				return;
			}
			string d = System.IO.Path.GetDirectoryName(fileName);
			XmlDocument currentXML = new XmlDocument();
			try
			{
				currentXML.Load(fileName);
			}
			catch
			{
				return;
			}
			
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Compatibility/Object");
				//Check this file actually contains OpenBVE compatibility nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							ReplacementObject o = new ReplacementObject();
							string[] names = null;
							foreach (XmlNode c in n.ChildNodes)
							{
								switch (c.Name.ToLowerInvariant())
								{
									case "name":
										if (c.InnerText.IndexOf(';') == -1)
										{
											names = new string[]
											{
												c.InnerText
											};
										}
										else
										{
											names = c.InnerText.Split(';');
										}
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
									default:
										Interface.AddMessage(MessageType.Warning, false, "Unexpected entry " + c.Name + " found in compatability object XML " + fileName);
										break;
								}
							}
							if (names != null)
							{
								o.ObjectNames = names;
								if (o.ReplacementPath != string.Empty)
								{
									int i = AvailableReplacements.Length;
									Array.Resize(ref AvailableReplacements, i + 1);
									AvailableReplacements[i] = o;
								}
							}
						}
					}
					DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Compatibility/Sound");
					//Check this file actually contains OpenBVE compatibility nodes
					if (DocumentNodes != null)
					{
						foreach (XmlNode n in DocumentNodes)
						{
							if (n.ChildNodes.OfType<XmlElement>().Any())
							{
								ReplacementObject o = new ReplacementObject();
								string[] names = null;
								foreach (XmlNode c in n.ChildNodes)
								{
									switch (c.Name.ToLowerInvariant())
									{
										case "name":
											if (c.InnerText.IndexOf(';') == -1)
											{
												names = new string[]
												{
													c.InnerText
												};
											}
											else
											{
												names = c.InnerText.Split(';');
											}
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
										default:
											Interface.AddMessage(MessageType.Warning, false,
												"Unexpected entry " + c.Name + " found in compatability object XML " + fileName);
											break;
									}
								}
								if (names != null)
								{
									o.ObjectNames = names;
									if (o.ReplacementPath != string.Empty)
									{
										int i = AvailableSounds.Length;
										Array.Resize(ref AvailableSounds, i + 1);
										AvailableSounds[i] = o;
									}
								}
							}
						}
					}
					//Now try and load any object list XML files this references
						DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Compatibility/ObjectList");
					
					if (DocumentNodes != null)
					{
						foreach (XmlNode n in DocumentNodes)
						{
							if (n.ChildNodes.OfType<XmlElement>().Any())
							{
								foreach (XmlNode c in n.ChildNodes)
								{
									switch (c.Name.ToLowerInvariant())
									{
										case "filename":
											var f = c.InnerText.Trim();
											if (!System.IO.File.Exists(f))
											{
												try
												{
													f = OpenBveApi.Path.CombineFile(d, f);
												}
												catch
												{ }
											}
											LoadCompatibilityObjects(f);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unexpected entry " + c.Name + " found in compatability XML list " + fileName);
											break;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
