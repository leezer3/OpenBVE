using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace OpenBve.Parsers.Panel
{
	class PanelAnimatedXmlParser
	{
		/// <summary>Parses a openBVE panel.animated.xml file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal static void ParsePanelAnimatedXml(string PanelFile, string TrainPath, TrainManager.Train Train, int Car)
		{
			// The current XML file to load
			string FileName = PanelFile;
			if (!File.Exists(FileName))
			{
				FileName = Path.CombineFile(TrainPath, PanelFile);
			}
			
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);

			// Check for null
			if (CurrentXML.Root == null)
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated");

			// Check this file actually contains OpenBVE panel definition elements
			if (DocumentElements == null || !DocumentElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			foreach (XElement element in DocumentElements)
			{
				ParsePanelAnimatedNode(element, FileName, TrainPath, Train, Car, Train.Cars[Car].CarSections[0], 0);
			}
		}

		private static void ParsePanelAnimatedNode(XElement Element, string FileName, string TrainPath, TrainManager.Train Train, int Car, TrainManager.CarSection CarSection, int GroupIndex)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			int currentSectionElement = 0;
			int numberOfSectionElements = Element.Elements().Count();
			double invfac = numberOfSectionElements == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)numberOfSectionElements;

			foreach (XElement SectionElement in Element.Elements())
			{
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double) currentSectionElement;
				if ((currentSectionElement & 4) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}

				string Section = SectionElement.Name.LocalName;

				switch (SectionElement.Name.LocalName.ToLowerInvariant())
				{
					case "group":
						if (GroupIndex == 0)
						{
							int n = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "number":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out n))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (n + 1 >= CarSection.Groups.Length)
							{
								Array.Resize(ref CarSection.Groups, n + 2);
								CarSection.Groups[n + 1] = new TrainManager.ElementsGroup
								{
									Elements = new AnimatedObject[] { },
									Overlay = true
								};
							}

							ParsePanelAnimatedNode(SectionElement, FileName, TrainPath, Train, Car, CarSection, n + 1);
						}
						break;
					case "touch":
						if (GroupIndex > 0)
						{
							Vector3 Position = Vector3.Zero;
							Vector3 Size = Vector3.Zero;
							int JumpScreen = GroupIndex - 1;
							List<int> SoundIndices = new List<int>();
							List<TrainManager.CommandEntry> CommandEntries = new List<TrainManager.CommandEntry>();
							TrainManager.CommandEntry CommandEntry = new TrainManager.CommandEntry();

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "position":
										if (!Vector3.TryParse(KeyNode.Value, ',', out Position))
										{
											Interface.AddMessage(MessageType.Error, false, "Position is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "size":
										if (!Vector3.TryParse(KeyNode.Value, ',', out Size))
										{
											Interface.AddMessage(MessageType.Error, false, "Size is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "jumpscreen":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out JumpScreen))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "soundindex":
										if (Value.Length != 0)
										{
											int SoundIndex;

											if (!NumberFormats.TryParseIntVb6(Value, out SoundIndex))
											{
												Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}

											SoundIndices.Add(SoundIndex);
										}
										break;
									case "command":
										{
											if (!CommandEntries.Contains(CommandEntry))
											{
												CommandEntries.Add(CommandEntry);
											}

											if (string.Compare(Value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
											{
												break;
											}

											int i;
											for (i = 0; i < Translations.CommandInfos.Length; i++)
											{
												if (string.Compare(Value, Translations.CommandInfos[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
												{
													break;
												}
											}
											if (i == Translations.CommandInfos.Length || Translations.CommandInfos[i].Type != Translations.CommandType.Digital)
											{
												Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											else
											{
												CommandEntry.Command = Translations.CommandInfos[i].Command;
											}
										}
										break;
									case "commandoption":
										if (!CommandEntries.Contains(CommandEntry))
										{
											CommandEntries.Add(CommandEntry);
										}

										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out CommandEntry.Option))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "soundentries":
										if (!KeyNode.HasElements)
										{
											Interface.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchSoundEntryNode(FileName, KeyNode, SoundIndices);
										break;
									case "commandentries":
										if (!KeyNode.HasElements)
										{
											Interface.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchCommandEntryNode(FileName, KeyNode, CommandEntries);
										break;
								}
							}
							CreateTouchElement(CarSection.Groups[GroupIndex], Position, Size, JumpScreen, SoundIndices.ToArray(), CommandEntries.ToArray());
						}
						break;
					case "include":
						{
							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "filename":
										{
											string File = OpenBveApi.Path.CombineFile(TrainPath, Value);
											if (System.IO.File.Exists(File))
											{
												System.Text.Encoding e = TextEncoding.GetSystemEncodingFromFile(File);
												UnifiedObject currentObject;
												Program.CurrentHost.LoadObject(File, e, out currentObject);
												var a = currentObject as AnimatedObjectCollection;
												if (a != null)
												{
													for (int i = 0; i < a.Objects.Length; i++)
													{
														Program.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
													}
													CarSection.Groups[GroupIndex].Elements = a.Objects;
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
										}
										break;
								}
							}
						}
						break;
				}
			}
		}

		private static void ParseTouchSoundEntryNode(string fileName, XElement parent, ICollection<int> indices)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string key = keyNode.Name.LocalName;
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

						switch (keyNode.Name.LocalName.ToLowerInvariant())
						{
							case "index":
								if (value.Any())
								{
									int index;

									if (!NumberFormats.TryParseIntVb6(value, out index))
									{
										Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									indices.Add(index);
								}
								break;
						}
					}
				}
			}
		}

		private static void ParseTouchCommandEntryNode(string fileName, XElement parent, ICollection<TrainManager.CommandEntry> entries)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					TrainManager.CommandEntry entry = new TrainManager.CommandEntry();
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string key = keyNode.Name.LocalName;
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

						switch (keyNode.Name.LocalName.ToLowerInvariant())
						{
							case "name":
								if (string.Compare(value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									break;
								}

								int i;

								for (i = 0; i < Translations.CommandInfos.Length; i++)
								{
									if (string.Compare(value, Translations.CommandInfos[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
									{
										break;
									}
								}

								if (i == Translations.CommandInfos.Length || Translations.CommandInfos[i].Type != Translations.CommandType.Digital)
								{
									Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									entry.Command = Translations.CommandInfos[i].Command;
								}
								break;
							case "option":
								if (value.Any())
								{
									int option;

									if (!NumberFormats.TryParseIntVb6(value, out option))
									{
										Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}
									else
									{
										entry.Option = option;
									}
								}
								break;
						}
					}

					entries.Add(entry);
				}
			}
		}

		private static void CreateTouchElement(TrainManager.ElementsGroup Group, Vector3 Position, Vector3 Size, int ScreenIndex, int[] SoundIndices, TrainManager.CommandEntry[] CommandEntries)
		{
			Vertex t0 = new Vertex(Size.X, Size.Y, -Size.Z);
            Vertex t1 = new Vertex(Size.X, -Size.Y, -Size.Z);
            Vertex t2 = new Vertex(-Size.X, -Size.Y, -Size.Z);
            Vertex t3 = new Vertex(-Size.X, Size.Y, -Size.Z);
            Vertex t4 = new Vertex(Size.X, Size.Y, Size.Z);
            Vertex t5 = new Vertex(Size.X, -Size.Y, Size.Z);
            Vertex t6 = new Vertex(-Size.X, -Size.Y, Size.Z);
            Vertex t7 = new Vertex(-Size.X, Size.Y, Size.Z);
			StaticObject Object = new StaticObject(Program.CurrentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3, t4, t5, t6, t7 };
            Object.Mesh.Faces = new MeshFace[] { new MeshFace(new int[] { 0, 1, 2, 3 }), new MeshFace(new int[] { 0, 4, 5, 1 }), new MeshFace(new int[] { 0, 3, 7, 4 }), new MeshFace(new int[] { 6, 5, 4, 7 }), new MeshFace(new int[] { 6, 7, 3, 2 }), new MeshFace(new int[] { 6, 2, 1, 5 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = 0;
			Object.Mesh.Materials[0].Color = Color32.White;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = null;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TrainManager.TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TrainManager.TouchElement
			{
				Element = new AnimatedObject(Program.CurrentHost),
				JumpScreenIndex = ScreenIndex,
				SoundIndices = SoundIndices,
				ControlIndices = new int[CommandEntries.Length]
			};
			Group.TouchElements[n].Element.States = new [] { new ObjectState() };
			Group.TouchElements[n].Element.States[0].Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Group.TouchElements[n].Element.States[0].Prototype = Object;
			Group.TouchElements[n].Element.CurrentState = 0;
			Group.TouchElements[n].Element.internalObject = new ObjectState { Prototype = Object };
			Program.CurrentHost.CreateDynamicObject(ref Group.TouchElements[n].Element.internalObject);
			int m = Interface.CurrentControls.Length;
			Array.Resize(ref Interface.CurrentControls, m + CommandEntries.Length);
			for (int i = 0; i < CommandEntries.Length; i++)
			{
				Interface.CurrentControls[m + i].Command = CommandEntries[i].Command;
				Interface.CurrentControls[m + i].Method = Interface.ControlMethod.Touch;
				Interface.CurrentControls[m + i].Option = CommandEntries[i].Option;
				Group.TouchElements[n].ControlIndices[i] = m + i;
			}
		}
	}
}
