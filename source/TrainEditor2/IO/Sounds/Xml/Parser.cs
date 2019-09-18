using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Sounds.Xml
{
	internal static partial class SoundCfgXml
	{
		internal static void Parse(string fileName, out Sound sound)
		{
			sound = new Sound();

			XDocument xml = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			List<XElement> carSoundNodes = xml.XPathSelectElements("/openBVE/CarSounds").ToList();
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			if (!carSoundNodes.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"No car sound nodes defined in XML file {fileName}");
				return;
			}

			foreach (XElement carSoundNode in carSoundNodes)
			{
				foreach (XElement sectionNode in carSoundNode.Elements())
				{
					switch (sectionNode.Name.LocalName.ToLowerInvariant())
					{
						case "ats":
						case "plugin":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of plugin sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<AtsElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "brake":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of brake sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<BrakeElement, BrakeKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "brakehandle":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of brake handle sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<BrakeHandleElement, BrakeHandleKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "breaker":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of breaker sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<BreakerElement, BreakerKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "buzzer":
							{
								BuzzerElement buzzer = new BuzzerElement { Key = BuzzerKey.Correct };
								ParseNode(basePath, sectionNode, buzzer);
								sound.SoundElements.Add(buzzer);
							}
							break;
						case "compressor":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of compressor sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<CompressorElement, CompressorKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "door":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of door sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<DoorElement, DoorKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "flange":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of flange sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<FlangeElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "halt":
							{
								OthersElement others = new OthersElement { Key = OthersKey.Halt };
								ParseNode(basePath, sectionNode, others);
								sound.SoundElements.Add(others);
							}
							break;
						case "horn":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of horn sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							foreach (XElement keyNode in sectionNode.Elements())
							{
								switch (keyNode.Name.LocalName.ToLowerInvariant())
								{
									case "primary":
										//Primary horn
										ParseArrayNode<PrimaryHornElement, HornKey>(basePath, keyNode, sound.SoundElements);
										break;
									case "secondary":
										//Secondary horn
										ParseArrayNode<SecondaryHornElement, HornKey>(basePath, keyNode, sound.SoundElements);
										break;
									case "music":
										//Music horn
										ParseArrayNode<MusicHornElement, HornKey>(basePath, keyNode, sound.SoundElements);
										break;
									default:
										Interface.AddMessage(MessageType.Error, false, $"Declaration {keyNode.Name.LocalName} is unsupported in a {sectionNode.Name.LocalName} node at line {((IXmlLineInfo)keyNode).LineNumber}.");
										break;
								}
							}
							break;
						case "loop":
						case "noise":
							{
								OthersElement others = new OthersElement { Key = OthersKey.Noise };
								ParseNode(basePath, sectionNode, others);
								sound.SoundElements.Add(others);
							}
							break;
						case "mastercontroller":
						case "powerhandle":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of power handle sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<MasterControllerElement, MasterControllerKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "motor":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of motor sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<MotorElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "pilotlamp":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of pilot lamp sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<PilotLampElement, PilotLampKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "pointfrontaxle":
						case "switchfrontaxle":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of point front axle sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<FrontSwitchElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "pointrearaxle":
						case "switchrearaxle":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of point rear axle sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<RearSwitchElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "reverser":
						case "reverserhandle":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of reverser sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<ReverserElement, ReverserKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "run":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of run sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<RunElement>(basePath, sectionNode, sound.SoundElements);
							break;
						case "shoe":
						case "rub":
							{
								OthersElement others = new OthersElement { Key = OthersKey.Shoe };
								ParseNode(basePath, sectionNode, others);
								sound.SoundElements.Add(others);
							}
							break;
						case "suspension":
						case "spring":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of suspension sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<SuspensionElement, SuspensionKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "requeststop":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of request stop sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<RequestStopElement, RequestStopKey>(basePath, sectionNode, sound.SoundElements);
							break;
						case "touch":
							if (!sectionNode.HasElements)
							{
								Interface.AddMessage(MessageType.Error, false, $"An empty list of touch sounds was defined at line {((IXmlLineInfo)sectionNode).LineNumber} in XML file {fileName}");
								break;
							}

							ParseArrayNode<TouchElement>(basePath, sectionNode, sound.SoundElements);
							break;
					}
				}
			}

			sound.SoundElements = new ObservableCollection<SoundElement>(sound.SoundElements.GroupBy(x => new { Type = x.GetType(), x.Key }).Select(x => x.First()));
		}

		private static void ParseNode(string basePath, XElement parentNode, SoundElement element)
		{
			foreach (XElement childNode in parentNode.Elements())
			{
				switch (childNode.Name.LocalName.ToLowerInvariant())
				{
					case "filename":
						if (!childNode.Value.Any() || Path.ContainsInvalidChars(childNode.Value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName {childNode.Value} contains illegal characters or is empty in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}.");
							return;
						}

						element.FilePath = Path.CombineFile(basePath, childNode.Value);
						break;
					case "position":
						string[] Arguments = childNode.Value.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;

						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Interface.AddMessage(MessageType.Error, false, $"Sound radius X {Arguments[0]} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber} is invalid.");
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, $"Sound radius Y {Arguments[1]} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber} is invalid.");
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Interface.AddMessage(MessageType.Error, false, $"Sound radius Z {Arguments[2]} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber} is invalid.");
							z = 0.0;
						}

						element.PositionX = x;
						element.PositionY = y;
						element.PositionZ = z;
						element.DefinedPosition = true;
						break;
					case "radius":
						double radius;

						if (!NumberFormats.TryParseDoubleVb6(childNode.Value, out radius))
						{
							Interface.AddMessage(MessageType.Error, false, $"The sound radius {childNode.Value} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber} is invalid.");
						}

						element.Radius = radius;
						element.DefinedRadius = true;
						break;
				}
			}
		}

		private static void ParseArrayNode<T>(string basePath, XElement parentNode, ICollection<SoundElement> elements) where T : SoundElement<int>, new()
		{
			foreach (XElement childNode in parentNode.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "sound")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid array node {childNode.Name.LocalName} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					int idx;
					XElement indexNode = childNode.Element("Index");

					if (indexNode == null)
					{
						Interface.AddMessage(MessageType.Error, false, $"Invalid array index {childNode.Name.LocalName} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
						return;
					}

					if (!NumberFormats.TryParseIntVb6(indexNode.Value, out idx))
					{
						Interface.AddMessage(MessageType.Error, false, $"Invalid array index {childNode.Name.LocalName} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
						return;
					}

					if (idx >= 0)
					{
						T element = new T
						{
							Key = idx
						};

						ParseNode(basePath, childNode, element);

						elements.Add(element);
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Invalid array index {childNode.Name.LocalName} in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)indexNode).LineNumber}");
					}
				}
			}
		}

		private static void ParseArrayNode<T, U>(string basePath, XElement parentNode, ICollection<SoundElement> elements) where T : SoundElement<U>, new()
		{
			foreach (XElement childNode in parentNode.Elements())
			{
				U[] keys = Enum.GetValues(typeof(U)).OfType<U>().Where(x => ((Enum)(object)x).GetStringValues().Any(y => y.Equals(childNode.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))).ToArray();

				if (keys.Any())
				{
					T element = new T
					{
						Key = keys.First()
					};

					ParseNode(basePath, childNode, element);

					elements.Add(element);
				}
				else
				{
					Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {childNode.Name.LocalName} encountered in XML node {parentNode.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
			}
		}
	}
}
