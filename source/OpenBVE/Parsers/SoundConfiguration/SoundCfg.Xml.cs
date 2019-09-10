﻿using System;
using System.Linq;
using System.Xml;
using OpenBve.BrakeSystems;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;

namespace OpenBve
{
	internal class SoundXmlParser
	{
		internal static bool ParseTrain(string fileName, TrainManager.Train train)
		{
			for (int i = 0; i < train.Cars.Length; i++)
			{
				try
				{
					Parse(fileName, ref train, ref train.Cars[i], i == train.DriverCar);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}

		private static string currentPath;
		internal static void Parse(string fileName, ref TrainManager.Train Train, ref TrainManager.Car car, bool isDriverCar)
		{
			//3D center of the car
			Vector3 center = Vector3.Zero;
			//Positioned to the left of the car, but centered Y & Z
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			//Positioned to the right of the car, but centered Y & Z
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			//Positioned at the front of the car, centered X and Y
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * car.Length);
			//Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
			Vector3 panel = new Vector3(car.Driver.X, car.Driver.Y, car.Driver.Z + 1.0);



			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = System.IO.Path.GetDirectoryName(fileName);
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CarSounds");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Interface.AddMessage(MessageType.Error, false, "No car sound nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty sound.xml file");
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
										Interface.AddMessage(MessageType.Error, false, "An empty list of plugin sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									ParseArrayNode(c, out car.Sounds.Plugin, center, SoundCfgParser.mediumRadius);
									break;
								case "brake":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of brake sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "releasehigh":
												//Release brakes from high pressure
												ParseNode(cc, out car.CarBrake.AirHigh, center, SoundCfgParser.smallRadius);
												break;
											case "release":
												//Release brakes from normal pressure
												ParseNode(cc, out car.CarBrake.Air, center, SoundCfgParser.smallRadius);
												break;
											case "releasefull":
												//Release brakes from full pressure
												ParseNode(cc, out car.CarBrake.AirZero, center, SoundCfgParser.smallRadius);
												break;
											case "emergency":
												//Apply EB
												ParseNode(cc, out Train.Handles.EmergencyBrake.ApplicationSound, center, SoundCfgParser.smallRadius);
												break;
											case "emergencyrelease":
												//Release EB
												ParseNode(cc, out Train.Handles.EmergencyBrake.ReleaseSound, center, SoundCfgParser.smallRadius);
												break;
											case "application":
												//Standard application
												ParseNode(cc, out car.Sounds.Brake, center, SoundCfgParser.smallRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "brakehandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of brake handle sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "apply":
												ParseNode(cc, out Train.Handles.Brake.Increase, panel, SoundCfgParser.tinyRadius);
												break;
											case "applyfast":
												ParseNode(cc, out Train.Handles.Brake.IncreaseFast, panel, SoundCfgParser.tinyRadius);
												break;
											case "release":
												ParseNode(cc, out Train.Handles.Brake.Decrease, panel, SoundCfgParser.tinyRadius);
												break;
											case "releasefast":
												ParseNode(cc, out Train.Handles.Brake.DecreaseFast, panel, SoundCfgParser.tinyRadius);
												break;
											case "min":
											case "minimum":
												ParseNode(cc, out Train.Handles.Brake.Min, panel, SoundCfgParser.tinyRadius);
												break;
											case "max":
											case "maximum":
												ParseNode(cc, out Train.Handles.Brake.Max, panel, SoundCfgParser.tinyRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "breaker":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of breaker sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, out car.Sounds.BreakerResume, panel, SoundCfgParser.smallRadius);
												break;
											case "off":
												ParseNode(cc, out car.Sounds.BreakerResumeOrInterrupt, panel, SoundCfgParser.smallRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "buzzer":
									if (!isDriverCar)
									{
										break;
									}
									ParseNode(c, out Train.SafetySystems.StationAdjust.AdjustAlarm, panel, SoundCfgParser.tinyRadius);
									break;
								case "compressor":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of compressor sounds was defined in in XML file " + fileName);
										break;
									}
									if (car.CarBrake.brakeType != BrakeType.Main)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "attack":
											case "start":
												//Compressor starting sound
												ParseNode(cc, out car.CarBrake.airCompressor.StartSound, center, SoundCfgParser.mediumRadius);
												break;
											case "loop":
												//Compressor loop sound
												ParseNode(cc, out car.CarBrake.airCompressor.LoopSound, center, SoundCfgParser.mediumRadius);
												break;
											case "release":
											case "stop":
											case "end":
												//Compressor end sound
												ParseNode(cc, out car.CarBrake.airCompressor.EndSound, center, SoundCfgParser.mediumRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "door":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of door sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "openleft":
											case "leftopen":
												ParseNode(cc, out car.Doors[0].OpenSound, left, SoundCfgParser.smallRadius);
												break;
											case "openright":
											case "rightopen":
												ParseNode(cc, out car.Doors[1].OpenSound, right, SoundCfgParser.smallRadius);
												break;
											case "closeleft":
											case "leftclose":
												ParseNode(cc, out car.Doors[0].CloseSound, left, SoundCfgParser.smallRadius);
												break;
											case "closeright":
											case "rightclose":
												ParseNode(cc, out car.Doors[1].CloseSound, right, SoundCfgParser.smallRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "flange":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of flange sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out car.Sounds.Flange, center, SoundCfgParser.mediumRadius);
									break;
								case "horn":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of horn sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "primary":
												//Primary horn
												ParseHornNode(cc, out car.Horns[0], front, SoundCfgParser.largeRadius);
												break;
											case "secondary":
												//Secondary horn
												ParseHornNode(cc, out car.Horns[1], front, SoundCfgParser.largeRadius);
												break;
											case "music":
												//Music horn
												ParseHornNode(cc, out car.Horns[2], front, SoundCfgParser.largeRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "loop":
								case "noise":
									ParseNode(c, out car.Sounds.Loop, center, SoundCfgParser.mediumRadius);
									break;
								case "mastercontroller":
								case "powerhandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of power handle sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "up":
											case "increase":
												ParseNode(cc, out Train.Handles.Power.Increase, panel, SoundCfgParser.tinyRadius);
												break;
											case "upfast":
											case "increasefast":
												ParseNode(cc, out Train.Handles.Power.IncreaseFast, panel, SoundCfgParser.tinyRadius);
												break;
											case "down":
											case "decrease":
												ParseNode(cc, out Train.Handles.Power.Decrease, panel, SoundCfgParser.tinyRadius);
												break;
											case "downfast":
											case "decreasefast":
												ParseNode(cc, out Train.Handles.Power.DecreaseFast, panel, SoundCfgParser.tinyRadius);
												break;
											case "min":
											case "minimum":
												ParseNode(cc, out Train.Handles.Power.Min, panel, SoundCfgParser.tinyRadius);
												break;
											case "max":
											case "maximum":
												ParseNode(cc, out Train.Handles.Power.Max, panel, SoundCfgParser.tinyRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
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
									if (!car.Specs.IsMotorCar)
									{
										break;
									}
									ParseMotorSoundTableNode(c, ref car.Sounds.Motor.Tables, center, SoundCfgParser.mediumRadius);
									break;
								case "pilotlamp":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of pilot-lamp sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, out Train.SafetySystems.PilotLamp.OnSound, panel, SoundCfgParser.tinyRadius);
												break;
											case "off":
												ParseNode(cc, out Train.SafetySystems.PilotLamp.OffSound, panel, SoundCfgParser.tinyRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "pointfrontaxle":
								case "switchfrontaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of point front axle sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out car.FrontAxle.PointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									break;
								case "pointrearaxle":
								case "switchrearaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of point rear axle sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out car.RearAxle.PointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									break;
								case "reverser":
								case "reverserhandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of reverser sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, out Train.Handles.Reverser.EngageSound, panel, SoundCfgParser.tinyRadius);
												break;
											case "off":
												ParseNode(cc, out Train.Handles.Reverser.ReleaseSound, panel, SoundCfgParser.tinyRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "run":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of run sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out car.Sounds.Run, center, SoundCfgParser.mediumRadius);
									break;
								case "shoe":
								case "rub":
									ParseNode(c, out car.CarBrake.Rub, center, SoundCfgParser.mediumRadius);
									break;
								case "suspension":
								case "spring":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of suspension sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "left":
												//Left suspension springs
												ParseNode(cc, out car.Sounds.SpringL, left, SoundCfgParser.smallRadius);
												break;
											case "right":
												//right suspension springs
												ParseNode(cc, out car.Sounds.SpringR, right, SoundCfgParser.smallRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "requeststop":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of request stop sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "stop":
												ParseNode(cc, out car.Sounds.RequestStop[0], center, SoundCfgParser.smallRadius);
												break;
											case "pass":
												ParseNode(cc, out car.Sounds.RequestStop[1], center, SoundCfgParser.smallRadius);
												break;
											case "ignored":
												ParseNode(cc, out car.Sounds.RequestStop[2], center, SoundCfgParser.smallRadius);
												break;
											default:
												Interface.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "touch":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false, "An empty list of touch sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									ParseArrayNode(c, out car.Sounds.Touch, center, SoundCfgParser.mediumRadius);
									break;
							}
						}
					}
				}
				car.Sounds.RunVolume = new double[car.Sounds.Run.Length];
				car.Sounds.FlangeVolume = new double[car.Sounds.Flange.Length];
			}
		}

		/// <summary>Parses an XML horn node</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Horn">The horn to apply this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private static void ParseHornNode(XmlNode node, out TrainManager.Horn Horn, Vector3 Position, double Radius)
		{
			Horn = new TrainManager.Horn();
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "start":
						ParseNode(c, out Horn.StartSound, ref Position, Radius);
						break;
					case "loop":
						ParseNode(c, out Horn.LoopSound, ref Position, Radius);
						break;
					case "end":
					case "release":
					case "stop":
						ParseNode(c, out Horn.EndSound, ref Position, Radius);
						break;
					case "toggle":
						if (c.InnerText.ToLowerInvariant() == "true" || c.InnerText.ToLowerInvariant() == "1")
						{
							Horn.Loop = true;
						}
						break;
				}
			}
		}

		/// <summary>Parses an XML motor table node</summary>
		/// <param name="node">The node</param>
		/// <param name="Tables">The motor sound tables to assign this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private static void ParseMotorSoundTableNode(XmlNode node, ref TrainManager.MotorSoundTable[] Tables, Vector3 Position, double Radius)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Interface.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Interface.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}
					if (idx >= 0)
					{
						for (int i = 0; i < Tables.Length; i++)
						{
							Tables[i].Buffer = null;
							Tables[i].Source = null;
							for (int j = 0; j < Tables[i].Entries.Length; j++)
							{
								if (idx == Tables[i].Entries[j].SoundIndex)
								{
									ParseNode(c, out Tables[i].Entries[j].Buffer, ref Position, Radius);
								}
							}
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
		}

		/// <summary>Parses a single XML node into a sound buffer and position reference</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private static void ParseNode(XmlNode node, out SoundBuffer Sound, ref Vector3 Position, double Radius)
		{
			string fileName = null;
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "filename":
						try
						{
							fileName = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
							if (!System.IO.File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Interface.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = null;
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Interface.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = null;
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(new char[] { ',' });
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x, y, z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Interface.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;

				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Interface.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = null;
				return;
			}
			Sound = Program.Sounds.TryToLoad(fileName, Radius);
		}

		/// <summary>Parses a single XML node into a car sound</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private static void ParseNode(XmlNode node, out CarSound Sound, Vector3 Position, double Radius)
		{
			string fileName = null;
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "filename":
						try
						{
							fileName = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
							if (!System.IO.File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Interface.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = new CarSound();
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Interface.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = new CarSound();
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(new char[] { ',' });
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Interface.AddMessage(MessageType.Error, false, "Sound radius Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x,y,z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Interface.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;
					
				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Interface.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = new CarSound();
				return;
			}
			Sound = new CarSound(Program.Sounds.RegisterBuffer(fileName,Radius), Position);
		}

		/// <summary>Parses an XML node containing a list of sounds into a car sound array</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any node)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any node)</param>
		private static void ParseArrayNode(XmlNode node, out CarSound[] Sounds, Vector3 Position, double Radius)
		{
			Sounds = new CarSound[0];
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Interface.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Interface.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}
					if (idx >= 0)
					{
						int l = Sounds.Length;
						Array.Resize(ref Sounds, idx + 1);
						while (l < Sounds.Length)
						{
							Sounds[l] = new CarSound();
							l++;
						}
						ParseNode(c, out Sounds[idx], Position, Radius);
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
			
		}
	}
}
