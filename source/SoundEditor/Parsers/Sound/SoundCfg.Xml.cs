using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundEditor.Systems;

namespace SoundEditor.Parsers.Sound
{
	internal static partial class SoundCfg
	{
		private static void ParseXml(string fileName, Sounds result)
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();

			//Load the marker's XML file 
			currentXML.Load(fileName);

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CarSounds");

				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Interface.AddMessage(MessageType.Error, false, string.Format("No car sound nodes defined in XML file {0}", fileName));
					return;
				}

				foreach (XmlNode n in DocumentNodes)
				{
					if (n.ChildNodes.OfType<XmlElement>().Any())
					{
						foreach (XmlNode c in n.ChildNodes)
						{
							switch (c.Name.ToLowerInvariant())
							{
								case "ats":
								case "plugin":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of plugin sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.Ats);
									break;
								case "brake":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of brake sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "releasehigh":
												//Release brakes from high pressure
												ParseNode(cc, result.Brake.BcReleaseHigh);
												break;
											case "release":
												//Release brakes from normal pressure
												ParseNode(cc, result.Brake.BcRelease);
												break;
											case "releasefull":
												//Release brakes from full pressure
												ParseNode(cc, result.Brake.BcReleaseFull);
												break;
											case "emergency":
												//Apply EB
												ParseNode(cc, result.Brake.Emergency);
												break;
											case "application":
												//Standard application
												ParseNode(cc, result.Brake.BpDecomp);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "brakehandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of brake handle sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "apply":
												ParseNode(cc, result.BrakeHandle.Apply);
												break;
											case "applyfast":
												ParseNode(cc, result.BrakeHandle.ApplyFast);
												break;
											case "release":
												ParseNode(cc, result.BrakeHandle.Release);
												break;
											case "releasefast":
												ParseNode(cc, result.BrakeHandle.ReleaseFast);
												break;
											case "min":
											case "minimum":
												ParseNode(cc, result.BrakeHandle.Min);
												break;
											case "max":
											case "maximum":
												ParseNode(cc, result.BrakeHandle.Max);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "breaker":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of breaker sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, result.Breaker.On);
												break;
											case "off":
												ParseNode(cc, result.Breaker.Off);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "buzzer":
									ParseNode(c, result.Buzzer.Correct);
									break;
								case "compressor":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of compressor sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "attack":
											case "start":
												//Compressor starting sound
												ParseNode(cc, result.Compressor.Attack);
												break;
											case "loop":
												//Compressor loop sound
												ParseNode(cc, result.Compressor.Loop);
												break;
											case "release":
											case "stop":
											case "end":
												//Compressor end sound
												ParseNode(cc, result.Compressor.Release);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "door":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of door sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "openleft":
											case "leftopen":
												ParseNode(cc, result.Door.OpenLeft);
												break;
											case "openright":
											case "rightopen":
												ParseNode(cc, result.Door.OpenRight);
												break;
											case "closeleft":
											case "leftclose":
												ParseNode(cc, result.Door.CloseLeft);
												break;
											case "closeright":
											case "rightclose":
												ParseNode(cc, result.Door.CloseRight);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "flange":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of flange sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.Flange);
									break;
								case "halt":
									ParseNode(c, result.Others.Halt);
									break;
								case "horn":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of horn sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "primary":
												//Primary horn
												ParseHornNode(cc, result.PrimaryHorn);
												break;
											case "secondary":
												//Secondary horn
												ParseHornNode(cc, result.SecondaryHorn);
												break;
											case "music":
												//Music horn
												ParseHornNode(cc, result.MusicHorn);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "loop":
								case "noise":
									ParseNode(c, result.Others.Noise);
									break;
								case "mastercontroller":
								case "powerhandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of power handle sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "up":
											case "increase":
												ParseNode(cc, result.MasterController.Up);
												break;
											case "upfast":
											case "increasefast":
												ParseNode(cc, result.MasterController.UpFast);
												break;
											case "down":
											case "decrease":
												ParseNode(cc, result.MasterController.Down);
												break;
											case "downfast":
											case "decreasefast":
												ParseNode(cc, result.MasterController.DownFast);
												break;
											case "min":
											case "minimum":
												ParseNode(cc, result.MasterController.Min);
												break;
											case "max":
											case "maximum":
												ParseNode(cc, result.MasterController.Max);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "motor":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of motor sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.Motor);
									break;
								case "pilotlamp":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of pilot-lamp sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, result.PilotLamp.On);
												break;
											case "off":
												ParseNode(cc, result.PilotLamp.Off);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "pointfrontaxle":
								case "switchfrontaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of point front axle sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.FrontSwitch);
									break;
								case "pointrearaxle":
								case "switchrearaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of point rear axle sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.RearSwitch);
									break;
								case "reverser":
								case "reverserhandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of reverser sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, result.Reverser.On);
												break;
											case "off":
												ParseNode(cc, result.Reverser.Off);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "run":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of run sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.Run);
									break;
								case "shoe":
								case "rub":
									ParseNode(c, result.Others.Shoe);
									break;
								case "suspension":
								case "spring":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of suspension sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "left":
												//Left suspension springs
												ParseNode(cc, result.Suspension.Left);
												break;
											case "right":
												//right suspension springs
												ParseNode(cc, result.Suspension.Right);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "requeststop":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of request stop sounds was defined in in XML file {0}", fileName));
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "stop":
												ParseNode(cc, result.RequestStop.Stop);
												break;
											case "pass":
												ParseNode(cc, result.RequestStop.Pass);
												break;
											case "ignored":
												ParseNode(cc, result.RequestStop.Ignored);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, string.Format("Declaration {0} is unsupported in a {1} node.", cc.Name, c.Name));
												break;
										}
									}

									break;
								case "touch":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("An empty list of touch sounds was defined in in XML file {0}", fileName));
										break;
									}

									ParseArrayNode(c, result.Touch);
									break;
							}
						}
					}
				}
			}
		}

		/// <summary>Parses a single XML node into a car sound</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		private static void ParseNode(XmlNode node, Sound Sound)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "filename":
						if (c.InnerText.Length == 0 || Path.ContainsInvalidChars(c.InnerText))
						{
							Interface.AddMessage(MessageType.Error, false, string.Format("FileName {0} contains illegal characters or is empty in XML node {1}.", c.InnerText, node.Name));
							return;
						}

						Sound.FileName = c.InnerText;
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;

						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Interface.AddMessage(MessageType.Error, false, string.Format("Sound radius X {0} in XML node {1} is invalid.", Arguments[0], node.Name));
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, string.Format("Sound radius Y {0} in XML node {1} is invalid.", Arguments[1], node.Name));
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Interface.AddMessage(MessageType.Error, false, string.Format("Sound radius Z {0} in XML node {1} is invalid.", Arguments[2], node.Name));
							z = 0.0;
						}

						Sound.Position = new Vector3(x, y, z);
						Sound.IsPositionDefined = true;
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Sound.Radius))
						{
							Interface.AddMessage(MessageType.Error, false, string.Format("The sound radius {0} in XML node {1} is invalid.", c.InnerText, node.Name));
						}

						Sound.IsRadiusDefined = true;
						break;
				}
			}
		}

		/// <summary>Parses an XML node containing a list of sounds into a car sound array</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sounds">The car sound array</param>
		private static void ParseArrayNode(XmlNode node, ListedSound Sounds)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;

				if (c.Name.ToLowerInvariant() != "sound")
				{
					Interface.AddMessage(MessageType.Error, false, string.Format("Invalid array node {0} in XML node {1}", c.Name, node.Name));
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Interface.AddMessage(MessageType.Error, false, string.Format("Invalid array index {0} in XML node {1}", c.Name, node.Name));
								return;
							}
							break;
						}
					}

					if (idx >= 0)
					{
						IndexedSound sound = new IndexedSound
						{
							Index = idx
						};

						ParseNode(c, sound);

						Sounds.Add(sound);
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, string.Format("Invalid array index {0} in XML node {1}", c.Name, node.Name));
					}
				}
			}
		}

		/// <summary>Parses an XML horn node</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Horn">The horn to apply this node's contents to</param>
		private static void ParseHornNode(XmlNode node, HornSound Horn)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "start":
						ParseNode(c, Horn.Start);
						break;
					case "loop":
						ParseNode(c, Horn.Loop);
						break;
					case "end":
					case "release":
					case "stop":
						ParseNode(c, Horn.End);
						break;
					case "toggle":
						if (c.InnerText.ToLowerInvariant() == "true" || c.InnerText.ToLowerInvariant() == "1")
						{
							Horn.Toggle = true;
						}
						break;
				}
			}
		}

		private static void WriteXml(Sounds data, string fileName)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE");
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			xml.Add(openBVE);

			XElement carSounds = new XElement("CarSounds");
			openBVE.Add(carSounds);

			WriteArrayNode(carSounds, "Run", data.Run);
			WriteArrayNode(carSounds, "Flange", data.Flange);
			WriteArrayNode(carSounds, "Motor", data.Motor);
			WriteArrayNode(carSounds, "PointFrontAxle", data.FrontSwitch);
			WriteArrayNode(carSounds, "PointRearAxle", data.RearSwitch);

			{
				XElement brake = new XElement("Brake");
				WriteNode(brake, "ReleaseHigh", data.Brake.BcReleaseHigh);
				WriteNode(brake, "Release", data.Brake.BcRelease);
				WriteNode(brake, "ReleaseFull", data.Brake.BcReleaseFull);
				WriteNode(brake, "Emergency", data.Brake.Emergency);
				WriteNode(brake, "Application", data.Brake.BpDecomp);

				if (brake.HasElements)
				{
					carSounds.Add(brake);
				}
			}

			{
				XElement compressor = new XElement("Compressor");
				WriteNode(compressor, "Attack", data.Compressor.Attack);
				WriteNode(compressor, "Loop", data.Compressor.Loop);
				WriteNode(compressor, "Release", data.Compressor.Release);

				if (compressor.HasElements)
				{
					carSounds.Add(compressor);
				}
			}

			{
				XElement suspension = new XElement("Suspension");
				WriteNode(suspension, "Left", data.Suspension.Left);
				WriteNode(suspension, "Right", data.Suspension.Right);

				if (suspension.HasElements)
				{
					carSounds.Add(suspension);
				}
			}

			WriteHornNode(carSounds, data.PrimaryHorn, data.SecondaryHorn, data.MusicHorn);

			{
				XElement door = new XElement("Door");
				WriteNode(door, "OpenLeft", data.Door.OpenLeft);
				WriteNode(door, "CloseLeft", data.Door.CloseLeft);
				WriteNode(door, "OpenRight", data.Door.OpenRight);
				WriteNode(door, "CloseRight", data.Door.CloseRight);

				if (door.HasElements)
				{
					carSounds.Add(door);
				}
			}

			WriteArrayNode(carSounds, "Ats", data.Ats);
			WriteNode(carSounds, "Buzzer", data.Buzzer.Correct);

			{
				XElement pilotLamp = new XElement("PilotLamp");
				WriteNode(pilotLamp, "On", data.PilotLamp.On);
				WriteNode(pilotLamp, "Off", data.PilotLamp.Off);

				if (pilotLamp.HasElements)
				{
					carSounds.Add(pilotLamp);
				}
			}

			{
				XElement brakeHandle = new XElement("BrakeHandle");
				WriteNode(brakeHandle, "Apply", data.BrakeHandle.Apply);
				WriteNode(brakeHandle, "ApplyFast", data.BrakeHandle.ApplyFast);
				WriteNode(brakeHandle, "Release", data.BrakeHandle.Release);
				WriteNode(brakeHandle, "ReleaseFast", data.BrakeHandle.ReleaseFast);
				WriteNode(brakeHandle, "Min", data.BrakeHandle.Min);
				WriteNode(brakeHandle, "Max", data.BrakeHandle.Max);

				if (brakeHandle.HasElements)
				{
					carSounds.Add(brakeHandle);
				}
			}

			{
				XElement masterController = new XElement("MasterController");
				WriteNode(masterController, "Up", data.MasterController.Up);
				WriteNode(masterController, "UpFast", data.MasterController.UpFast);
				WriteNode(masterController, "Down", data.MasterController.Down);
				WriteNode(masterController, "DownFast", data.MasterController.DownFast);
				WriteNode(masterController, "Min", data.MasterController.Min);
				WriteNode(masterController, "Max", data.MasterController.Max);

				if (masterController.HasElements)
				{
					carSounds.Add(masterController);
				}
			}

			{
				XElement reverser = new XElement("Reverser");
				WriteNode(reverser, "On", data.Reverser.On);
				WriteNode(reverser, "Off", data.Reverser.Off);

				if (reverser.HasElements)
				{
					carSounds.Add(reverser);
				}
			}

			{
				XElement breaker = new XElement("Breaker");
				WriteNode(breaker, "On", data.Breaker.On);
				WriteNode(breaker, "Off", data.Breaker.Off);

				if (breaker.HasElements)
				{
					carSounds.Add(breaker);
				}
			}

			{
				XElement requestStop = new XElement("RequestStop");
				WriteNode(requestStop, "Stop", data.RequestStop.Stop);
				WriteNode(requestStop, "Pass", data.RequestStop.Pass);
				WriteNode(requestStop, "Ignored", data.RequestStop.Ignored);

				if (requestStop.HasElements)
				{
					carSounds.Add(requestStop);
				}
			}

			WriteNode(carSounds, "Noise", data.Others.Noise);
			WriteNode(carSounds, "Shoe", data.Others.Shoe);
			WriteNode(carSounds, "Halt", data.Others.Halt);

			xml.Save(fileName);
		}

		private static void WriteNode(XElement parent, string nodeName, Sound sound)
		{
			XElement newNode = new XElement(nodeName);

			if (!string.IsNullOrEmpty(sound.FileName))
			{
				newNode.Add(new XElement("FileName", sound.FileName));
			}
			else
			{
				return;
			}

			if (sound.IsPositionDefined)
			{
				newNode.Add(new XElement("Position", string.Format("{0}, {1}, {2}", sound.Position.X, sound.Position.Y, sound.Position.Z)));
			}

			if (sound.IsRadiusDefined)
			{
				newNode.Add(new XElement("Radius", sound.Radius));
			}

			parent.Add(newNode);
		}

		private static void WriteArrayNode(XElement parent, string nodeName, ListedSound sounds)
		{
			if (sounds.Any())
			{
				XElement newNode = new XElement(nodeName);

				foreach (IndexedSound sound in sounds)
				{
					WriteNode(newNode, "Sound", sound);
					newNode.Elements("Sound").Last().AddFirst(new XElement("Index", sound.Index));
				}

				parent.Add(newNode);
			}
		}

		private static void WriteHornNode(XElement parent, HornSound primary, HornSound secondary, HornSound music)
		{
			XElement newNode = new XElement("Horn");
			WriteHornNode(newNode, "Primary", primary);
			WriteHornNode(newNode, "Secondary", secondary);
			WriteHornNode(newNode, "Music", music);
			parent.Add(newNode);
		}

		private static void WriteHornNode(XElement parent, string nodeName, HornSound sound)
		{
			XElement newNode = new XElement(nodeName);
			WriteNode(newNode, "Start", sound.Start);
			WriteNode(newNode, "Loop", sound.Loop);
			WriteNode(newNode, "End", sound.End);
			newNode.Add(new XElement("Toggle", sound.Toggle));
			parent.Add(newNode);
		}
	}
}
