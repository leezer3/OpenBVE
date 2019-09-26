using System;
using System.IO;
using System.Xml;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
	internal static class Ls3DGrpParser
	{

		private class GruppenObject
		{
			//A gruppenobject holds a list of ls3dobjs, which appear to be roughly equivilant to meshbuilders
			internal string Name;
			internal Vector3 Position;
			internal Vector3 Rotation;
			internal string FunctionScript;

			internal GruppenObject()
			{
				Name = string.Empty;
				Position = new Vector3();
				Rotation = new Vector3();
			}
		}

		internal static AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, Vector3 Rotation)
		{
			XmlDocument currentXML = new XmlDocument();
			AnimatedObjectCollection Result = new AnimatedObjectCollection(Plugin.currentHost)
			{
				Objects = new AnimatedObject[0]
			};
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
					while (Lines[i].IndexOf("\"\"", StringComparison.Ordinal) != -1)
					{
						//Loksim parser tolerates multiple quotes, strict XML does not
						Lines[i] = Lines[i].Replace("\"\"", "\"");
					}
					while (Lines[i].IndexOf("  ", StringComparison.Ordinal) != -1)
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
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Error parsing Loksim3D XML: " + ex.Message);
					return null;
				}
			}

			string BaseDir = System.IO.Path.GetDirectoryName(FileName);

			GruppenObject[] CurrentObjects = new GruppenObject[0];
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				UnifiedObject[] obj = new UnifiedObject[0];
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
											GruppenObject Object = new GruppenObject
											{
												Rotation = Rotation
											};
											foreach (XmlAttribute attribute in childNode.Attributes)
											{
												switch (attribute.Name)
												{
													case "Name":
														string ObjectFile = OpenBveApi.Path.Loksim3D.CombineFile(BaseDir, attribute.Value, Plugin.LoksimPackageFolder);
														if (!File.Exists(ObjectFile))
														{
															Object.Name = null;
															Plugin.currentHost.AddMessage(MessageType.Warning, true, "Ls3d Object file " + attribute.Value + " not found.");
														}
														else
														{
															Object.Name = ObjectFile;
														}
														break;
													case "Position":
														if (!Vector3.TryParse(attribute.Value, ';', out Object.Position))
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, true, "Invalid position vector " + attribute.Value + " supplied in Ls3d object file.");
														}
														break;
													case "Rotation":
														Vector3 r;
														if (!Vector3.TryParse(attribute.Value, ';', out r))
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, true, "Invalid rotation vector " + attribute.Value + " supplied in Ls3d object file.");
														}
														Object.Rotation += r;
														break;
													case "ShowOn":
														//Defines when the object should be shown
														try
														{
															Object.FunctionScript = FunctionScriptNotation.GetPostfixNotationFromInfixNotation(GetAnimatedFunction(attribute.Value, false));
														}
														catch
														{
															Object.FunctionScript = null;
														}
														
														break;
													case "HideOn":
														//Defines when the object should be hidden
														try
														{
															Object.FunctionScript = FunctionScriptNotation.GetPostfixNotationFromInfixNotation(GetAnimatedFunction(attribute.Value, true));
														}
														catch
														{
															Object.FunctionScript = null;
														}
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

					//Single mesh object, containing all static components of the LS3D object
					//If we use multiples, the Z-sorting throws a wobbly
					StaticObject staticObject = new StaticObject(Plugin.currentHost)
					{
						Mesh = new Mesh
						{
							Vertices = new VertexTemplate[0],
							Faces = new MeshFace[0],
							Materials = new MeshMaterial[0]
						}
					};
					for (int i = 0; i < CurrentObjects.Length; i++)
					{
						if (CurrentObjects[i] == null || string.IsNullOrEmpty(CurrentObjects[i].Name))
						{
							continue;
						}
						StaticObject Object = null;
						AnimatedObjectCollection AnimatedObject = null;
						try {
							if (CurrentObjects[i].Name.ToLowerInvariant().EndsWith(".l3dgrp"))
							{
								AnimatedObject = ReadObject(CurrentObjects[i].Name, Encoding, CurrentObjects[i].Rotation);
							}
							else if (CurrentObjects[i].Name.ToLowerInvariant().EndsWith(".l3dobj"))
							{
								Object = Ls3DObjectParser.ReadObject(CurrentObjects[i].Name, CurrentObjects[i].Rotation);
							}
							else
							{
								throw new Exception("Format " + System.IO.Path.GetExtension(CurrentObjects[i].Name) + " is not currently supported by the Loksim3D object parser");
							}
						}
						catch (Exception ex) {
							Plugin.currentHost.AddMessage(MessageType.Error, false, ex.Message);
						}
						
						if (Object != null)
						{
							if (!string.IsNullOrEmpty(CurrentObjects[i].FunctionScript))
							{
								//If the function script is not empty, this is a new animated object bit
								Array.Resize<UnifiedObject>(ref obj, obj.Length + 1);
								obj[obj.Length - 1] = Object;
								int aL = Result.Objects.Length;
								Array.Resize(ref Result.Objects, aL + 1);
								AnimatedObject a = new AnimatedObject(Plugin.currentHost);
								ObjectState aos = new ObjectState
								{
									Prototype = Object,
									Translation = OpenTK.Matrix4d.CreateTranslation(CurrentObjects[i].Position.X, CurrentObjects[i].Position.Y, -CurrentObjects[i].Position.Z)
								};
								a.States = new [] { aos };
								Result.Objects[aL] = a;
								Result.Objects[aL].StateFunction = new FunctionScript(Plugin.currentHost, CurrentObjects[i].FunctionScript + " 1 == --", false);
							}
							else
							{
								//Otherwise, join to the main static mesh & update co-ords
								for (int j = 0; j < Object.Mesh.Vertices.Length; j++)
								{
									Object.Mesh.Vertices[j].Coordinates += CurrentObjects[i].Position;
								}
								staticObject.JoinObjects(Object);
							}
						}
						else if (AnimatedObject != null)
						{
							int rl = Result.Objects.Length;
							int l = AnimatedObject.Objects.Length;
							Array.Resize(ref Result.Objects, Result.Objects.Length + l);
							for (int o = rl; o < rl + l; o++)
							{
								if (AnimatedObject.Objects[o - rl] != null)
								{
									Result.Objects[o] = AnimatedObject.Objects[o - rl].Clone();
									for (int si = 0; si < Result.Objects[o].States.Length; si++)
									{
										Result.Objects[o].States[si].Translation *= OpenTK.Matrix4d.CreateTranslation(CurrentObjects[i].Position.X, CurrentObjects[i].Position.Y, -CurrentObjects[i].Position.Z);
									}
								}
								else
								{
									Result.Objects[o] = new AnimatedObject(Plugin.currentHost);
									Result.Objects[o].States = new ObjectState[0];
								}
							}
						}
					}
					if (staticObject != null)
					{
						Array.Resize(ref Result.Objects, Result.Objects.Length + 1);
						AnimatedObject a = new AnimatedObject(Plugin.currentHost);
						ObjectState aos = new ObjectState
						{
							Prototype = staticObject
						};
						a.States = new [] { aos };
						Result.Objects[Result.Objects.Length - 1] = a;
					}
				}
				return Result;
			}
			//Didn't find an acceptable XML object
			//Probably will cause things to throw an absolute wobbly somewhere....
			return null;
		}

		private static string GetAnimatedFunction(string Value, bool Hidden)
		{
			string[] splitStrings = Value.Split(new char[] { });
			string script = string.Empty;
			for (int i = 0; i < splitStrings.Length; i++)
			{
				splitStrings[i] = splitStrings[i].Trim(new char[] { }).ToLowerInvariant();
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
						string[] finalStrings = splitStrings[i].Split(new char[] {'_'});
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
										script += Hidden ? "reversernotch != 1 & powernotch == 0" : "reversernotch == 1 & powernotch > 0";
										break;
									case "rueck":
										//Reverse
										script += Hidden ? "reversernotch != -1 & powernotch == 0" : "reversernotch == -1 & powernotch > 0";
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
