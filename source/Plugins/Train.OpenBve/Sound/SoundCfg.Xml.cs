using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
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
			//Positioned at the rear of the car centered X and Y
			Vector3 rear = new Vector3(0.0, 0.0, -0.5 * car.Length);
			//Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
			Vector3 panel = new Vector3(car.Driver.X, car.Driver.Y, car.Driver.Z + 1.0);



			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = Path.GetDirectoryName(fileName);
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CarSounds");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "No car sound nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty sound.xml file");
				}
				foreach (XmlNode n in DocumentNodes)
				{
					if (n.ChildNodes.OfType<XmlElement>().Any())
					{
						foreach (XmlNode c in n.ChildNodes)
						{
							Enum.TryParse(c.Name, true, out SoundCfgSection currentSection);
							switch (currentSection)
							{
								case SoundCfgSection.ATS:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of plugin sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									ParseDictionaryNode(c, out car.Sounds.Plugin, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.Brake:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of brake sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.BrakeHandle:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of brake handle sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Breaker:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of breaker sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Buzzer:
									if (!isDriverCar)
									{
										break;
									}
									ParseNode(c, out Train.SafetySystems.StationAdjust.AdjustAlarm, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundCfgSection.Compressor:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of compressor sounds was defined in in XML file " + fileName);
										break;
									}

									if (!(car.CarBrake is AirBrake airBrake) || airBrake.BrakeType != BrakeType.Main)
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
												ParseNode(cc, out airBrake.Compressor.StartSound, center, SoundCfgParser.mediumRadius);
												break;
											case "loop":
												//Compressor loop sound
												ParseNode(cc, out airBrake.Compressor.LoopSound, center, SoundCfgParser.mediumRadius);
												break;
											case "release":
											case "stop":
											case "end":
												//Compressor end sound
												ParseNode(cc, out airBrake.Compressor.EndSound, center, SoundCfgParser.mediumRadius);
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Door:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of door sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Flange:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of flange sounds was defined in in XML file " + fileName);
										break;
									}
									ParseDictionaryNode(c, out car.Flange.Sounds, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.Horn:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of horn sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Loop:
									ParseNode(c, out car.Sounds.Loop, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.MasterController:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of power handle sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Motor:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of motor sounds was defined in in XML file " + fileName);
										break;
									}
									if (!car.TractionModel.ProvidesPower)
									{
										break;
									}

									if (TrainXmlParser.MotorSoundXMLParsed == null)
									{
										TrainXmlParser.MotorSoundXMLParsed = new bool[Train.Cars.Length];
									}
									TrainXmlParser.MotorSoundXMLParsed[car.Index] = true;
									ParseMotorSoundTableNode(c, car, ref car.TractionModel.MotorSounds, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.PilotLamp:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of pilot-lamp sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Switch:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of point front axle sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out CarSound[] pointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.FrontAxle.PointSounds = pointSounds;
									// ReSharper disable once CoVariantArrayConversion
									car.RearAxle.PointSounds = pointSounds;
									break;
								case SoundCfgSection.SwitchFrontAxle:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of point front axle sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out CarSound[] frontAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.FrontAxle.PointSounds = frontAxlePointSounds;
									break;
								case SoundCfgSection.SwitchRearAxle:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of point rear axle sounds was defined in in XML file " + fileName);
										break;
									}
									ParseArrayNode(c, out CarSound[] rearAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
									// ReSharper disable once CoVariantArrayConversion
									car.RearAxle.PointSounds = rearAxlePointSounds;
									break;
								case SoundCfgSection.Reverser:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of reverser sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Run:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of run sounds was defined in in XML file " + fileName);
										break;
									}
									ParseDictionaryNode(c, out car.Run.Sounds, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.Shoe:
									ParseNode(c, out car.CarBrake.Rub, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.Suspension:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of suspension sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "left":
												//Left suspension springs
												ParseNode(cc, out car.Suspension.SpringL, left, SoundCfgParser.smallRadius);
												break;
											case "right":
												//right suspension springs
												ParseNode(cc, out car.Suspension.SpringR, right, SoundCfgParser.smallRadius);
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.RequestStop:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of request stop sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Touch:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of touch sounds was defined in in XML file " + fileName);
										break;
									}
									if (!isDriverCar)
									{
										break;
									}
									ParseDictionaryNode(c, out car.Sounds.Touch, center, SoundCfgParser.mediumRadius);
									break;
								case SoundCfgSection.Sanders:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of sanders sounds was defined in in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.Coupler:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of coupler sounds was defined in in XML file " + fileName);
										break;
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "uncouple":
												ParseNode(cc, out car.Coupler.UncoupleSound, rear, SoundCfgParser.smallRadius);
												break;
											case "couple":
												ParseNode(cc, out car.Coupler.CoupleSound, rear, SoundCfgParser.smallRadius);
												break;
											case "couplecab":
												ParseNode(cc, out car.Sounds.CoupleCab, rear, SoundCfgParser.smallRadius);
												break;
											case "uncouplecab":
												ParseNode(cc, out car.Sounds.UncoupleCab, rear, SoundCfgParser.smallRadius);
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Declaration " + cc.Name + " is unsupported in a " + c.Name + " node.");
												break;
										}
									}
									break;
								case SoundCfgSection.DriverSupervisionDevice:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of driver supervision device sounds was defined in in XML file " + fileName);
										break;
									}
									if (car.SafetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out DriverSupervisionDevice dsd))
									{
										foreach (XmlNode cc in c.ChildNodes)
										{
											switch (cc.Name.ToLowerInvariant())
											{
												case "alert":
													ParseNode(cc, out dsd.AlertSound, center, SoundCfgParser.smallRadius);
													break;
												case "alarm":
													ParseNode(cc, out dsd.AlarmSound, center, SoundCfgParser.smallRadius);
													break;
												case "reset":
													ParseNode(cc, out dsd.ResetSound, center, SoundCfgParser.smallRadius);
													break;
											}
										}
									}

									
									break;
								case SoundCfgSection.Headlights:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of headlights sounds was defined in in XML file " + fileName);
										break;
									}
									if (Train.SafetySystems.Headlights == null)
									{
										break;
									}

									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "switch":
												Vector3 pos = new Vector3(center);
												ParseNode(cc, out Train.SafetySystems.Headlights.SwitchSoundBuffer, ref pos, SoundCfgParser.smallRadius);
												break;
										}
									}
									break;
								case SoundCfgSection.Pantograph:
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An empty list of headlights sounds was defined in in XML file " + fileName);
										break;
									}

									CarSound raiseSound = null, lowerSound = null;
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "raise":
												ParseNode(cc, out raiseSound, center, SoundCfgParser.mediumRadius);
												break;
											case "lower":
												ParseNode(cc, out lowerSound, center, SoundCfgParser.mediumRadius);
												break;
										}
									}

									for (int i = 0; i < Train.Cars.Length; i++)
									{
										if (Train.Cars[i].TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph p))
										{
											p.RaiseSound = raiseSound.Clone();
											p.LowerSound = lowerSound.Clone();
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
		/// <param name="Car">The car</param>
		/// <param name="motorSound">The motor sound tables to assign this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private void ParseMotorSoundTableNode(XmlNode node, CarBase Car, ref AbstractMotorSound motorSound, Vector3 Position, double Radius)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() != "sound")
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}

					if (idx >= 0)
					{
						if (motorSound == null && Plugin.MotorSoundTables != null)
						{
							// We are using train.dat, and the sound.xml in extension mode but this car was not initially set as a motor car
							// Construct a new motor sound table
							motorSound = new BVEMotorSound(Car, 18.0, Plugin.MotorSoundTables);
						}

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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unsupported motor sound type in XML node " + node.Name);	
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
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
							fileName = Path.CombineFile(currentPath, c.InnerText);
							if (!System.IO.File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = null;
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = null;
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound radius X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound radius Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound radius Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x, y, z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;

				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = null;
				return;
			}

			Plugin.CurrentHost.RegisterSound(fileName, Radius, out var sndHandle);
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
							fileName = Path.CombineFile(currentPath, c.InnerText);
							if (!System.IO.File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " does not exist.");
								Sound = new CarSound();
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound path " + c.InnerText + " in XML node " + node.Name + " is invalid.");
							Sound = new CarSound();
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound position X " + Arguments[0] + " in XML node " + node.Name + " is invalid.");
							x = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound position Y " + Arguments[1] + " in XML node " + node.Name + " is invalid.");
							y = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Sound position Z " + Arguments[2] + " in XML node " + node.Name + " is invalid.");
							z = 0.0;
						}
						Position = new Vector3(x,y,z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The sound radius " + c.InnerText + " in XML node " + node.Name + " is invalid.");
						}
						break;
					
				}
			}
			if (fileName == null)
			{
				//No valid filename node specified
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "XML node " + node.Name + " does not point to a valid sound file.");
				Sound = new CarSound();
				return;
			}
			Sound = new CarSound(Plugin.CurrentHost, fileName, Radius, Position);
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
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
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
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
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
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array node " + c.Name + " in XML node " + node.Name);
				}
				else
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
								return;
							}
							break;
						}
					}
					if (idx >= 0)
					{
						ParseNode(c, out var sound, Position, Radius);
						Sounds[idx] = sound;
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid array index " + c.Name + " in XML node " + node.Name);
					}
				}
			}
		}
	}
}
