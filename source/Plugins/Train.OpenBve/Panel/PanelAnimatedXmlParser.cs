using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Formats.OpenBve;
using LibRender2;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	class PanelAnimatedXmlParser
	{

		internal Plugin Plugin;

		internal PanelAnimatedXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Parses a openBVE panel.animated.xml file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal void ParsePanelAnimatedXml(string PanelFile, TrainBase Train, int Car)
		{
			// The current XML file to load
			string FileName = PanelFile;
			if (!File.Exists(FileName))
			{
				FileName = Path.CombineFile(Train.TrainFolder, PanelFile);
			}
			
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);

			// Check for null
			if (CurrentXML.Root == null)
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException(FileName + " does not appear to be a valid XML file.");
			}

			List<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated").ToList();

			// Check this file actually contains OpenBVE panel definition elements
			if (DocumentElements == null || DocumentElements.Count == 0)
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException(FileName + " is not a valid PanelAnimatedXML file.");
			}

			foreach (XElement element in DocumentElements)
			{
				ParsePanelAnimatedNode(element, FileName, Train.TrainFolder, Train.Cars[Car].CarSections[CarSectionType.Interior], 0);
			}
		}

		private void ParsePanelAnimatedNode(XElement Element, string FileName, string TrainPath, CarSection CarSection, int GroupIndex)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			int currentSectionElement = 0;
			int numberOfSectionElements = Element.Elements().Count();
			double invfac = numberOfSectionElements == 0 ? 0.4 : 0.4 / numberOfSectionElements;

			foreach (XElement SectionElement in Element.Elements())
			{
				Plugin.CurrentProgress = Plugin.LastProgress + invfac * currentSectionElement;
				if ((currentSectionElement & 4) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}


				Enum.TryParse(SectionElement.Name.LocalName, true, out Panel2Sections Section);
				switch (Section)
				{
					case Panel2Sections.Group:
						if (GroupIndex == 0)
						{
							int n = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;
								Enum.TryParse(KeyNode.Name.LocalName, true, out Panel2Key key);

								switch (key)
								{
									case Panel2Key.Number:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out n))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (n + 1 >= CarSection.Groups.Length)
							{
								Array.Resize(ref CarSection.Groups, n + 2);
								CarSection.Groups[n + 1] = new ElementsGroup();
							}

							ParsePanelAnimatedNode(SectionElement, FileName, TrainPath, CarSection, n + 1);
						}
						break;
					case Panel2Sections.Touch:
						if (GroupIndex > 0)
						{
							Vector3 Position = Vector3.Zero;
							Vector3 Size = Vector3.Zero;
							int JumpScreen = GroupIndex - 1;
							List<int> SoundIndices = new List<int>();
							List<CommandEntry> CommandEntries = new List<CommandEntry>();
							CommandEntry CommandEntry = new CommandEntry();
							Bitmap cursorTexture = null;
							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;
								Enum.TryParse(KeyNode.Name.LocalName, true, out Panel2Key key);

								switch (key)
								{
									case Panel2Key.Position:
										if (!Vector3.TryParse(KeyNode.Value, ',', out Position))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Position is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Panel2Key.Size:
										if (!Vector3.TryParse(KeyNode.Value, ',', out Size))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Size is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Panel2Key.JumpScreen:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out JumpScreen))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Panel2Key.SoundIndex:
										if (Value.Length != 0)
										{
											if (!NumberFormats.TryParseIntVb6(Value, out var SoundIndex))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												break;
											}
											SoundIndices.Add(SoundIndex);
										}
										break;
									case Panel2Key.Command:
										{
											if (!CommandEntries.Contains(CommandEntry))
											{
												CommandEntries.Add(CommandEntry);
											}

											if (string.Compare(Value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
											{
												break;
											}

											if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command command))
											{
												CommandEntry.Command = command;
											}
											else
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case Panel2Key.CommandOption:
										if (!CommandEntries.Contains(CommandEntry))
										{
											CommandEntries.Add(CommandEntry);
										}

										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out CommandEntry.Option))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Panel2Key.SoundEntries:
										if (!KeyNode.HasElements)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchSoundEntryNode(FileName, KeyNode, SoundIndices);
										break;
									case Panel2Key.CommandEntries:
										if (!KeyNode.HasElements)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchCommandEntryNode(FileName, KeyNode, CommandEntries);
										break;
									case Panel2Key.Cursor:
										string cursorFile = Path.CombineFile(TrainPath, Value);
										if (File.Exists(cursorFile))
										{
											cursorTexture = (Bitmap)Image.FromFile(cursorFile);
										}
										break;
								}
							}
							CreateTouchElement(CarSection.Groups[GroupIndex], Position, Size, JumpScreen, SoundIndices.ToArray(), CommandEntries.ToArray());
							if (cursorTexture != null)
							{
								CarSection.Groups[GroupIndex].TouchElements[CarSection.Groups[GroupIndex].TouchElements.Length - 1].MouseCursor = new MouseCursor(Plugin.Renderer, string.Empty, cursorTexture);
							}
						}
						break;
					case Panel2Sections.Include:
						{
							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								Enum.TryParse(KeyNode.Name.LocalName, true, out Panel2Key key);

								switch (key)
								{
									case Panel2Key.FileName:
										{
											string includeFile = Path.CombineFile(TrainPath, Value);
											if (File.Exists(includeFile))
											{
												System.Text.Encoding e = TextEncoding.GetSystemEncodingFromFile(includeFile);
												Plugin.CurrentHost.LoadObject(includeFile, e, out var currentObject);
												var a = (AnimatedObjectCollection)currentObject;
												if (a != null)
												{
													for (int i = 0; i < a.Objects.Length; i++)
													{
														Plugin.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
													}
													CarSection.Groups[GroupIndex].Elements = a.Objects;
												}
												else
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
										}
										break;
								}
							}
						}
						break;
				}

				currentSectionElement++;
			}
		}

		private static void ParseTouchSoundEntryNode(string fileName, XElement parent, ICollection<int> indices)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
						Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

						switch (key)
						{
							case Panel2Key.Index:
								if (value.Any())
								{
									if (!NumberFormats.TryParseIntVb6(value, out var index))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
										break;
									}

									indices.Add(index);
								}
								break;
						}
					}
				}
			}
		}

		private static void ParseTouchCommandEntryNode(string fileName, XElement parent, ICollection<CommandEntry> entries)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					CommandEntry entry = new CommandEntry();
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
						Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

						switch (key)
						{
							case Panel2Key.Name:
								if (string.Compare(value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									break;
								}

								if (Enum.TryParse(value.Replace("_", string.Empty), true, out Translations.Command command))
								{
									entry.Command = command;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								break;
							case Panel2Key.Option:
								if (value.Any())
								{
									if (!NumberFormats.TryParseIntVb6(value, out var option))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
										break;
									}
									entry.Option = option;
								}
								break;
						}
					}

					entries.Add(entry);
				}
			}
		}

		private void CreateTouchElement(ElementsGroup Group, Vector3 Position, Vector3 Size, int ScreenIndex, int[] SoundIndices, CommandEntry[] CommandEntries)
		{
			Vertex t0 = new Vertex(Size.X, Size.Y, -Size.Z);
            Vertex t1 = new Vertex(Size.X, -Size.Y, -Size.Z);
            Vertex t2 = new Vertex(-Size.X, -Size.Y, -Size.Z);
            Vertex t3 = new Vertex(-Size.X, Size.Y, -Size.Z);
            Vertex t4 = new Vertex(Size.X, Size.Y, Size.Z);
            Vertex t5 = new Vertex(Size.X, -Size.Y, Size.Z);
            Vertex t6 = new Vertex(-Size.X, -Size.Y, Size.Z);
            Vertex t7 = new Vertex(-Size.X, Size.Y, Size.Z);
			StaticObject Object = new StaticObject(Plugin.CurrentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3, t4, t5, t6, t7 };
            Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 3 }), new MeshFace(new[] { 0, 4, 5, 1 }), new MeshFace(new[] { 0, 3, 7, 4 }), new MeshFace(new[] { 6, 5, 4, 7 }), new MeshFace(new[] { 6, 7, 3, 2 }), new MeshFace(new[] { 6, 2, 1, 5 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = 0;
			Object.Mesh.Materials[0].Color = Color32.White;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = null;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			int m = Plugin.CurrentControls.Length;
			Array.Resize(ref Plugin.CurrentControls, m + CommandEntries.Length);
			int[] controlIndicies = new int[CommandEntries.Length];
			for (int i = 0; i < CommandEntries.Length; i++)
			{
				Plugin.CurrentControls[m + i].Command = CommandEntries[i].Command;
				Plugin.CurrentControls[m + i].Method = ControlMethod.Touch;
				Plugin.CurrentControls[m + i].Option = CommandEntries[i].Option;
				controlIndicies[i] = m + i;
			}
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TouchElement(new AnimatedObject(Plugin.CurrentHost), ScreenIndex, SoundIndices, controlIndicies);
			Group.TouchElements[n].Element.States = new [] { new ObjectState() };
			Group.TouchElements[n].Element.States[0].Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Group.TouchElements[n].Element.States[0].Prototype = Object;
			Group.TouchElements[n].Element.CurrentState = 0;
			Group.TouchElements[n].Element.internalObject = new ObjectState(Object);
			Plugin.CurrentHost.CreateDynamicObject(ref Group.TouchElements[n].Element.internalObject);
			
		}
	}
}
