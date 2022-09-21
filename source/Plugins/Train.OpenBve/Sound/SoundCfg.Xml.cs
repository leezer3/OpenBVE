using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Car.Systems;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.TractionModels.BVE;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class SoundXmlParser
	{
		internal readonly Plugin Plugin;

		internal SoundXmlParser(Plugin plugin)
		{
			this.Plugin = plugin;
		}

		internal bool ParseTrain(string fileName, TrainBase train)
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

		private string currentPath;
		internal void Parse(string fileName, ref TrainBase Train, ref CarBase car, bool isDriverCar)
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
					Plugin.currentHost.AddMessage(MessageType.Error, false, "No car sound nodes defined in XML file " + fileName);
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
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of plugin sounds was defined in in XML file " + fileName);
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
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of brake sounds was defined in in XML file " + fileName);
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
												ParseNode(cc, out car.CarBrake.Release, center, SoundCfgParser.smallRadius);
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "brakehandle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of brake handle sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "breaker":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of breaker sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									car.Breaker = new Breaker(car);
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "on":
												ParseNode(cc, out car.Breaker.Resume, panel, SoundCfgParser.smallRadius);
												break;
											case "off":
												ParseNode(cc, out car.Breaker.ResumeOrInterrupt, panel, SoundCfgParser.smallRadius);
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
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
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of compressor sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "door":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of door sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "flange":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of flange sounds was defined in in XML file " + fileName);
										break;
									}
									ParseDictionaryNode(c, out car.Sounds.Flange, center, SoundCfgParser.mediumRadius);
									break;
								case "horn":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of horn sounds was defined in in XML file " + fileName);
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
												ParseHornNode(cc, car, out car.Horns[0], front, SoundCfgParser.largeRadius);
												break;
											case "secondary":
												//Secondary horn
												ParseHornNode(cc, car, out car.Horns[1], front, SoundCfgParser.largeRadius);
												break;
											case "music":
												//Music horn
												ParseHornNode(cc, car, out car.Horns[2], front, SoundCfgParser.largeRadius);
												car.Horns[2].Loop = true;
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
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
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of power handle sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "motor":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of motor sounds was defined in in XML file " + fileName);
										break;
									}
									if (car.TractionModel is BVEMotorCar)
									{
										ParseMotorSoundTableNode(c, ref car.TractionModel.Sounds, center, SoundCfgParser.mediumRadius);	
									}
									TrainXmlParser.MotorSoundXMLParsed[car.Index] = true;
									ParseMotorSoundTableNode(c, ref car.Sounds.Motor, center, SoundCfgParser.mediumRadius);
									break;
								case "pilotlamp":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of pilot-lamp sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "switch":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of point front axle sounds was defined in in XML file " + fileName);
										break;
									}
									CarSound[] pointSounds;
									ParseArrayNode(c, out pointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.FrontAxle.PointSounds = pointSounds;
									// ReSharper disable once CoVariantArrayConversion
									car.RearAxle.PointSounds = pointSounds;
									break;
								case "pointfrontaxle":
								case "switchfrontaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of point front axle sounds was defined in in XML file " + fileName);
										break;
									}

									CarSound[] frontAxlePointSounds;
									ParseArrayNode(c, out frontAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.FrontAxle.PointSounds = frontAxlePointSounds;
									break;
								case "pointrearaxle":
								case "switchrearaxle":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of point rear axle sounds was defined in in XML file " + fileName);
										break;
									}
									CarSound[] rearAxlePointSounds;
									ParseArrayNode(c, out rearAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.RearAxle.PointSounds = rearAxlePointSounds;
									break;
								case "reverser":
								case "reverserhandle":
									ReverserHandle reverser = Train.Handles.Reverser as ReverserHandle;
									if (reverser == null)
									{
										break;
									}
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of reverser sounds was defined in in XML file " + fileName);
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
												ParseNode(cc, out reverser.EngageSound, panel, SoundCfgParser.tinyRadius);
												break;
											case "off":
												ParseNode(cc, out reverser.ReleaseSound, panel, SoundCfgParser.tinyRadius);
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "run":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of run sounds was defined in in XML file " + fileName);
										break;
									}
									ParseDictionaryNode(c, out car.Sounds.Run, center, SoundCfgParser.mediumRadius);
									break;
								case "shoe":
								case "rub":
									ParseNode(c, out car.CarBrake.Rub, center, SoundCfgParser.mediumRadius);
									break;
								case "suspension":
								case "spring":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of suspension sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "requeststop":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of request stop sounds was defined in in XML file " + fileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case "touch":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of touch sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									ParseArrayNode(c, out car.Sounds.Touch, center, SoundCfgParser.mediumRadius);
									break;
								case "sanders":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "An empty list of sanders sounds was defined in in XML file " + fileName);
										break;
									}
									Sanders sanders = car.ReAdhesionDevice as Sanders;
									if (sanders == null)
									{
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "activate":
												ParseNode(cc, out sanders.ActivationSound, center, SoundCfgParser.smallRadius);
												break;
											case "emptyactivate":
												ParseNode(cc, out sanders.EmptyActivationSound, center, SoundCfgParser.smallRadius);
												break;
											case "deactivate":
												ParseNode(cc, out sanders.DeActivationSound, center, SoundCfgParser.smallRadius);
												break;
											case "loop":
												ParseNode(cc, out sanders.LoopSound, center, SoundCfgParser.smallRadius);
												break;
											case "empty":
												ParseNode(cc, out sanders.EmptySound, center, SoundCfgParser.smallRadius);
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
							}
						}
					}
				}
			}
		}

		/// <summary>Parses an XML horn node</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="car">The car</param>
		/// <param name="Horn">The horn to apply this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private void ParseHornNode(XmlNode node, CarBase car, out Horn Horn, Vector3 Position, double Radius)
		{
			Horn = new Horn(car);
			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "start":
						ParseNode(c, out Horn.StartSound, ref Position, Radius);
						Horn.StartEndSounds = true;
						break;
					case "loop":
						ParseNode(c, out Horn.LoopSound, ref Position, Radius);
						break;
					case "end":
					case "release":
					case "stop":
						ParseNode(c, out Horn.EndSound, ref Position, Radius);
						Horn.StartEndSounds = true;
						break;
					case "toggle":
						if (c.InnerText.ToLowerInvariant() == "true" || c.InnerText.ToLowerInvariant() == "1")
						{
							Horn.Loop = true;
						}
						break;
				}
				Horn.SoundPosition = Position;
			}
		}

		/// <summary>Parses an XML motor table node into a BVE motor sound table</summary>
		/// <param name="node">The node</param>
		/// <param name="motorSound">The motor sound tables to assign this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private void ParseMotorSoundTableNode(XmlNode node, ref AbstractMotorSound motorSound, Vector3 Position, double Radius)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}

					if (idx >= 0)
					{
						if (motorSound is BVEMotorSound bveMotorSound)
						{
							for (int i = 0; i < bveMotorSound.Tables.Length; i++)
							{
								bveMotorSound.Tables[i].Buffer = null;
								bveMotorSound.Tables[i].Source = null;
								for (int j = 0; j < bveMotorSound.Tables[i].Entries.Length; j++)
								{
									if (idx == bveMotorSound.Tables[i].Entries[j].SoundIndex)
									{
										ParseNode(c, out bveMotorSound.Tables[i].Entries[j].Buffer, ref Position, Radius);
									}
								}
							}
						}
						else if (motorSound is BVE5MotorSound bve5MotorSound)
						{
							if (idx >= bve5MotorSound.SoundBuffers.Length)
							{
								Array.Resize(ref bve5MotorSound.SoundBuffers, idx + 1);
							}
							ParseNode(c, out bve5MotorSound.SoundBuffers[idx], ref Position, Radius);
						}
						else
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Unsupported motor sound type in XML node " + node.Name);	
						}
					}
					else
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
		}

		/// <summary>Parses a single XML node into a sound buffer and position reference</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private void ParseNode(XmlNode node, out SoundBuffer Sound, ref Vector3 Position, double Radius)
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
								Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = null;
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = null;
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound radius X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound radius Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound radius Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x, y, z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;

				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Plugin.currentHost.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = null;
				return;
			}

			Plugin.currentHost.RegisterSound(fileName, Radius, out var sndHandle);
			Sound = sndHandle as SoundBuffer;
		}

		/// <summary>Parses a single XML node into a car sound</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private void ParseNode(XmlNode node, out CarSound Sound, Vector3 Position, double Radius)
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
								Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = new CarSound();
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = new CarSound();
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound position X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound position Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Sound position Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x,y,z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;
					
				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Plugin.currentHost.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = new CarSound();
				return;
			}
			Sound = new CarSound(Plugin.currentHost, fileName, Radius, Position);
		}

		/// <summary>Parses an XML node containing a list of sounds into a car sound array</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any node)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any node)</param>
		private void ParseArrayNode(XmlNode node, out CarSound[] Sounds, Vector3 Position, double Radius)
		{
			Sounds = new CarSound[0];
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
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
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
		}

		/// <summary>Parses an XML node containing a list of sounds into a car sound array</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any node)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any node)</param>
		private void ParseDictionaryNode(XmlNode node, out Dictionary<int, CarSound> Sounds, Vector3 Position, double Radius)
		{
			Sounds = new Dictionary<int, CarSound>();
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}
					if (idx >= 0)
					{
						ParseNode(c, out var sound, Position, Radius);
						if (Sounds.ContainsKey(idx))
						{
							Sounds[idx] = sound;
						}
						else
						{
							Sounds.Add(idx, sound);
						}
					}
					else
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
		}
	}
}
