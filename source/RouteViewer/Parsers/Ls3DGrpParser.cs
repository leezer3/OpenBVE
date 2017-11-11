using System;
using System.IO;
using System.Xml;
using OpenBveApi.Math;
using System.Linq;

namespace OpenBve
{
	internal static class Ls3DGrpParser
	{
		/// <summary>A GruppenObject contains a single textured mesh stored in a separate .ls3dobj file (Roughly equivilant to a MeshBuilder)</summary>
		private class GruppenObject
		{
			/// <summary>The on-disk path to the mesh</summary>
			internal string Name;
			/// <summary>The position of the mesh within the object</summary>
			internal Vector3 Position;
			/// <summary>The rotation to be applied to the mesh (During load, BEFORE position)</summary>
			internal Vector3 Rotation;
			/// <summary>The FunctionScript attached to the mesh, controlling it's animations</summary>
			internal string FunctionScript;

			/// <summary>Creates a new GruppenObject</summary>
			internal GruppenObject()
			{
				Name = string.Empty;
				Position = new Vector3();
				Rotation = new Vector3();
			}
		}

		/// <summary>Loads a Loksim3D GruppenObject</summary>
		/// <param name="FileName">The filename to load</param>
		/// <param name="Encoding">The text encoding of the containing file (Currently ignored, REMOVE??)</param>
		/// <param name="LoadMode">The object load mode</param>
		/// <returns>A new animated object collection, containing the GruppenObject's meshes etc.</returns>
		internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode)
		{
			XmlDocument currentXML = new XmlDocument();
			ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
			Result.Objects = new ObjectManager.AnimatedObject[0];
			try
			{
				currentXML.Load(FileName);
			}
			catch (Exception ex)
			{
				//The XML is not strictly valid
				string[] Lines = File.ReadAllLines(FileName);
				using (var stringReader = new StringReader(Lines[0]))
				{
					var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
					using (var xmlReader = XmlReader.Create(stringReader, settings))
					{
						if (xmlReader.Read())
						{
							//Attempt to find the text encoding and re-read the file
							var result = xmlReader.GetAttribute("encoding");
							if (result != null)
							{
								var e = System.Text.Encoding.GetEncoding(result);
								Lines = File.ReadAllLines(FileName, e);
								//Turf out the old encoding, as our string array should now be UTF-8
								Lines[0] = "<?xml version=\"1.0\"?>";
							}
						}
					}
				}
				for (int i = 0; i < Lines.Length; i++)
				{
					while (Lines[i].IndexOf("\"\"", StringComparison.InvariantCulture) != -1)
					{
						//Loksim parser tolerates multiple quotes, strict XML does not
						Lines[i] = Lines[i].Replace("\"\"", "\"");
					}
					while (Lines[i].IndexOf("  ", StringComparison.InvariantCulture) != -1)
					{
						//Replace double-spaces with singles
						Lines[i] = Lines[i].Replace("  ", " ");
					}
				}
				bool tryLoad = false;
				try
				{
					//Horrible hack: Write out our string array to a new memory stream, then load from this stream
					//Why can't XmlDocument.Load() just take a string array......
					using (var stream = new MemoryStream())
					{
						var sw = new StreamWriter(stream);
						foreach (var line in Lines)
						{
							sw.Write(line);
							sw.Flush();
						}
						sw.Flush();
						stream.Position = 0;
						currentXML.Load(stream);
						tryLoad = true;
					}
				}
				catch
				{
					//Generic catch-all clause
				}
				if (!tryLoad)
				{
					//Pass out the *original* XML error, not anything generated when we've tried to correct it
					Interface.AddMessage(Interface.MessageType.Error, false, "Error parsing Loksim3D XML: " + ex.Message);
					return null;
				}
			}

			string BaseDir = System.IO.Path.GetDirectoryName(FileName);

			GruppenObject[] CurrentObjects = new GruppenObject[0];
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				ObjectManager.UnifiedObject[] obj = new OpenBve.ObjectManager.UnifiedObject[0];
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/GRUPPENOBJECT");
				if (DocumentNodes != null)
				{
					foreach (XmlNode outerNode in DocumentNodes)
					{
						if (outerNode.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode node in outerNode.ChildNodes)
							{
								if (node.Name == "Object" && node.ChildNodes.OfType<XmlElement>().Any())
								{

									foreach (XmlNode childNode in node.ChildNodes)
									{
										if (childNode.Name == "Props" && childNode.Attributes != null)
										{
											GruppenObject Object = new GruppenObject();
											foreach (XmlAttribute attribute in childNode.Attributes)
											{
												switch (attribute.Name)
												{
													case "Name":
														string ObjectFile = OpenBveApi.Path.Loksim3D.CombineFile(BaseDir, attribute.Value, Program.FileSystem.LoksimPackageInstallationDirectory);
														if (!File.Exists(ObjectFile))
														{
															Object.Name = null;
															Interface.AddMessage(Interface.MessageType.Warning, true, "Ls3d Object file " + attribute.Value + " not found.");
														}
														else
														{
															Object.Name = ObjectFile;
														}
														break;
													case "Position":
														string[] SplitPosition = attribute.Value.Split(';');
														double.TryParse(SplitPosition[0], out Object.Position.X);
														double.TryParse(SplitPosition[1], out Object.Position.Y);
														double.TryParse(SplitPosition[2], out Object.Position.Z);
														break;
													case "Rotation":
														string[] SplitRotation = attribute.Value.Split(';');

														double.TryParse(SplitRotation[0], out Object.Rotation.X);
														double.TryParse(SplitRotation[1], out Object.Rotation.Y);
														double.TryParse(SplitRotation[2], out Object.Rotation.Z);
														break;
													case "ShowOn":
														//Defines when the object should be shown
														Object.FunctionScript = FunctionScripts.GetPostfixNotationFromInfixNotation(GetAnimatedFunction(attribute.Value, false));
														break;
													case "HideOn":
														//Defines when the object should be hidden
														Object.FunctionScript = FunctionScripts.GetPostfixNotationFromInfixNotation(GetAnimatedFunction(attribute.Value, true));
														break;
												}
											}
											if (Object.Name != null)
											{
												Array.Resize<GruppenObject>(ref CurrentObjects, CurrentObjects.Length + 1);
												CurrentObjects[CurrentObjects.Length - 1] = Object;
											}
										}
									}
								}
							}
						}
					}
					//We've loaded the XML references, now load the objects into memory
					for (int i = 0; i < CurrentObjects.Length; i++)
					{
						if (CurrentObjects[i] == null || string.IsNullOrEmpty(CurrentObjects[i].Name))
						{
							continue;
						}
						var Object = Ls3DObjectParser.ReadObject(CurrentObjects[i].Name, LoadMode, CurrentObjects[i].Rotation);
						if (Object != null)
						{
							Array.Resize<ObjectManager.UnifiedObject>(ref obj, obj.Length + 1);
							obj[obj.Length - 1] = Object;

							Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Objects.Length + 1);
							ObjectManager.AnimatedObject a = new ObjectManager.AnimatedObject();
							ObjectManager.AnimatedObjectState aos = new ObjectManager.AnimatedObjectState
							{
								Object = Object,
								Position = CurrentObjects[i].Position,
							};
							a.States = new ObjectManager.AnimatedObjectState[] { aos };
							Result.Objects[i] = a;
							if (!string.IsNullOrEmpty(CurrentObjects[i].FunctionScript))
							{
								Result.Objects[i].StateFunction =
									FunctionScripts.GetFunctionScriptFromPostfixNotation(CurrentObjects[i].FunctionScript + " 1 == --");
							}
						}
					}
				}
				return Result;
			}
			//Didn't find an acceptable XML object
			//Probably will cause things to throw an absolute wobbly somewhere....
			return null;
		}

		/// <summary>Gets the internal animation function string for the given Loksim3D function</summary>
		/// <param name="Value">The Loksim3D function</param>
		/// <param name="Hidden">If set, this function HIDES the object, if not it SHOWS the object</param>
		/// <returns>The new function string</returns>
		private static string GetAnimatedFunction(string Value, bool Hidden)
		{
			string[] splitStrings = Value.Split(' ');
			string script = string.Empty;
			for (int i = 0; i < splitStrings.Length; i++)
			{
				splitStrings[i] = splitStrings[i].Trim().ToLowerInvariant();
				if (i % 2 == 0)
				{
					if (splitStrings[i].StartsWith("spitzenlicht1-an"))
					{
						//Appears to be HEADLIGHTS (F)
						script += Hidden ? "reversernotch != -1" : "reversernotch == -1";
					}
					if (splitStrings[i].StartsWith("schlusslicht1-an"))
					{
						//Appears to be TAILLIGHTS (F)
						script += Hidden ? "reversernotch != 1" : "reversernotch == 1";
					}
					if (splitStrings[i].StartsWith("spitzenlicht2-an"))
					{
						//Appears to be HEADLIGHTS (R)
						script += Hidden ? "reversernotch != 1" : "reversernotch == 1";
					}
					if (splitStrings[i].StartsWith("schlusslicht2-an"))
					{
						//Appears to be TAILLIGHTS (R)
						script += Hidden ? "reversernotch != -1" : "reversernotch == -1";
					}
					if (splitStrings[i].StartsWith("tür") && splitStrings[i].EndsWith("offen"))
					{
						switch (splitStrings[i][3])
						{
							case '0':
							case '2':
							case '4':
							case '6':
							case '8':
								//Left doors (??)
								script += Hidden ? "rightdoors == 0" : "rightdoors != 0";
								break;
							case '1':
							case '3':
							case '5':
							case '7':
							case '9':
								//Right doors (??)
								script += Hidden ? "leftdoors == 0" : "leftdoors != 0";
								break;
						}
					}
					if (splitStrings[i].StartsWith("rauch"))
					{
						//Smoke (e.g. steam loco)
						string[] finalStrings = splitStrings[i].Split('_');
						switch (finalStrings[1])
						{
							case "stand":
								//Standing
								script += Hidden ? "reversernotch != 0 | powernotch != 0" : "reversernotch == 0 | powernotch == 0";
								break;
							case "fahrt":
								switch (finalStrings[2])
								{
									case "vor":
										//Forwards
										script += Hidden ? "reversernotch != 1 & powernotch == 0" : "reversernotch == 1 & powernotch > 1";
										break;
									case "rueck":
										//Reverse
										script += Hidden ? "reversernotch != -1 & powernotch == 0" : "reversernotch == -1 & powernotch > 1";
										break;
								}
								break;
						}
					}
				}
				else
				{
					switch (splitStrings[i].ToLowerInvariant())
					{
						case "or":
							script += " & ";
							break;
						case "and":
							script += " | ";
							break;
					}
				}
			}
			return script;
		}
	}
}
