using System;
using System.Xml;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace CsvRwRouteParser
{
	internal partial class Parser
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
				n = Path.CombineFile(objectPath, fileName);
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

			if (Plugin.CurrentOptions.EnableBveTsHacks)
			{
				string fn;
				//The Midland Suburban Line has a malformed object zip, so let's try again....
				if (fileName.StartsWith("Midland Suburban Line", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = "Midland Suburban Line Objects" + fileName.Substring(21);
					try
					{
						//Catch completely malformed path references
						n = Path.CombineFile(objectPath, fn);
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
						n = Path.CombineFile(objectPath, fn);
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
				//Some Chashinai downloads are badly formatted and don't specifiy that the objects and sounds should be placed within a Chashinai folder
				if (fileName.StartsWith(@"Chashinai", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = fileName.Substring(10);
					try
					{
						//Catch completely malformed path references
						n = Path.CombineFile(objectPath, fn);
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
				//Malformed First Brno Track: Origins downloads- https://bveworldwide.forumotion.com/t2317-fbt-cannot-start-routes-missing-objects#21405
				if (fileName.StartsWith("FirstBrnoTrack-Origins", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = "FirstBrnoTrack" + fileName.Substring(22);
					try
					{
						//Catch completely malformed path references
						n = Path.CombineFile(objectPath, fn);
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
				//Wood Lane (2010) looks for BRSigs inside the NWM folder
				//Later versions don't have these here, so let's try for the 'standard' copy.....
				if (fileName.StartsWith(@"NWM\brsigs", StringComparison.InvariantCultureIgnoreCase))
				{
					fn = "brsigs" + fileName.Substring(10);
					try
					{
						//Catch completely malformed path references
						n = Path.CombineFile(objectPath, fn);
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
					if (string.Equals(CompatibilityObjects.AvailableReplacements[i].ObjectNames[j], fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						//Available replacement found
						fileName = CompatibilityObjects.AvailableReplacements[i].ReplacementPath;
						if (!string.IsNullOrEmpty(CompatibilityObjects.AvailableReplacements[i].Message))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, CompatibilityObjects.AvailableReplacements[i].Message);
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
				n = Path.CombineFile(objectPath, fileName);
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

			if (!System.IO.Path.HasExtension(fileName))
			{
				/*
				 * Marginally hacky: No extension, so let's try as WAV
				 * (In some cases will produce results)
				 */
				fileName += ".wav";
			}
			try
			{
				//Catch completely malformed path references
				n = Path.CombineFile(objectPath, fileName);
			}
			catch
			{
				return false;
			}
			//Some Chashinai downloads are badly formatted and don't specifiy that the objects and sounds should be placed within a Chashinai folder
			if (fileName.StartsWith(@"Chashinai", StringComparison.InvariantCultureIgnoreCase))
			{
				string fn = fileName.Substring(10);
				try
				{
					//Catch completely malformed path references
					n = Path.CombineFile(objectPath, fn);
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
			if (System.IO.File.Exists(n))
			{
				fileName = n;
				//The object exists, and does not require a compatibility object
				return true;
			}

			//We still haven't found the object on-disk, so check the compatibility objects to see if a replacement is available
			for (int i = 0; i < CompatibilityObjects.AvailableSounds.Length; i++)
			{
				if (CompatibilityObjects.AvailableSounds[i].ObjectNames.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < CompatibilityObjects.AvailableSounds[i].ObjectNames.Length; j++)
				{
					if (string.Equals(CompatibilityObjects.AvailableSounds[i].ObjectNames[j], fileName, StringComparison.InvariantCultureIgnoreCase))
					{
						//Available replacement found
						fileName = CompatibilityObjects.AvailableSounds[i].ReplacementPath;
						if (!string.IsNullOrEmpty(CompatibilityObjects.AvailableSounds[i].Message))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, CompatibilityObjects.AvailableSounds[i].Message);
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
		internal static UnifiedObject SignalPost = null;
		internal static UnifiedObject LimitPostStraight = null;
		internal static UnifiedObject LimitPostLeft = null;
		internal static UnifiedObject LimitPostRight = null;
		internal static UnifiedObject LimitPostInfinite = null;
		internal static UnifiedObject LimitOneDigit = null;
		internal static UnifiedObject LimitTwoDigits = null;
		internal static UnifiedObject LimitThreeDigits = null;
		internal static UnifiedObject StopPost = null;
		internal static UnifiedObject TransponderS = null;
		internal static UnifiedObject TransponderSN = null;
		internal static UnifiedObject TransponderFalseStart = null;
		internal static UnifiedObject TransponderPOrigin = null;
		internal static UnifiedObject TransponderPStop = null;

		internal static string LimitGraphicsPath;

		private static bool CompatabilityObjectsLoaded = false;

		internal static void LoadAutoGeneratedObjects(string CompatibilityFolder)
		{
			string LimitPath = Path.CombineDirectory(CompatibilityFolder, "Limits");
			LimitGraphicsPath = Path.CombineDirectory(LimitPath, "Graphics");
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_straight.csv"), System.Text.Encoding.UTF8, out LimitPostStraight);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_left.csv"), System.Text.Encoding.UTF8, out LimitPostLeft);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_right.csv"), System.Text.Encoding.UTF8, out LimitPostRight);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_infinite.csv"), System.Text.Encoding.UTF8, out LimitPostInfinite);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_1_digit.csv"), System.Text.Encoding.UTF8, out LimitOneDigit);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_2_digits.csv"), System.Text.Encoding.UTF8, out LimitTwoDigits);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(LimitPath, "limit_3_digits.csv"), System.Text.Encoding.UTF8, out LimitThreeDigits);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(CompatibilityFolder, "stop.csv"), System.Text.Encoding.UTF8, out StopPost);
			string TransponderPath = Path.CombineDirectory(CompatibilityFolder, "Transponders");
			Plugin.CurrentHost.LoadObject(Path.CombineFile(TransponderPath, "s.csv"), System.Text.Encoding.UTF8, out TransponderS);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(TransponderPath, "sn.csv"), System.Text.Encoding.UTF8, out TransponderSN);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(TransponderPath, "falsestart.csv"), System.Text.Encoding.UTF8, out TransponderFalseStart);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(TransponderPath, "porigin.csv"), System.Text.Encoding.UTF8, out TransponderPOrigin);
			Plugin.CurrentHost.LoadObject(Path.CombineFile(TransponderPath, "pstop.csv"), System.Text.Encoding.UTF8, out TransponderPStop);
		}

		/// <summary>The array containing the paths to all available replacement objects</summary>
		internal static ReplacementObject[] AvailableReplacements = new ReplacementObject[0];

		internal static ReplacementObject[] AvailableSounds = new ReplacementObject[0];

		/// <summary>Loads the available compatibility object database</summary>
		/// <param name="fileName">The database file</param>
		internal static void LoadCompatibilityObjects(string fileName)
		{
			if (!System.IO.File.Exists(fileName) || CompatabilityObjectsLoaded)
			{
				return;
			}
			string d = Path.GetDirectoryName(fileName);
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
											names = new[]
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
											string f = Path.CombineFile(d, c.InnerText.Trim());
											if (System.IO.File.Exists(f))
											{
												o.ReplacementPath = f;
											}
											break;
									case "message":
										o.Message = c.InnerText.Trim();
										break;
									default:
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unexpected entry " + c.Name + " found in compatability object XML " + fileName);
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
												names = new[]
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
											string f = Path.CombineFile(d, c.InnerText.Trim());
											if (System.IO.File.Exists(f))
											{
												o.ReplacementPath = f;
											}
											break;
										case "message":
											o.Message = c.InnerText.Trim();
											break;
										default:
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false,
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
													f = Path.CombineFile(d, f);
												}
												catch
												{
													//Deliberately suppress all errors
												}
											}
											LoadCompatibilityObjects(f);
											break;
										default:
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unexpected entry " + c.Name + " found in compatability XML list " + fileName);
											break;
									}
								}
							}
						}
					}
				}
			}

			if (AvailableReplacements.Length != 0)
			{
				CompatabilityObjectsLoaded = true;
			}
		}
	}
}
