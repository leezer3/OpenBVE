using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainEditor2.IO.Panels.Xml;
using TrainEditor2.Models.Trains;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static readonly ReadOnlyCollection<int> currentVersion = Array.AsReadOnly(new[] { 1, 8 });

		internal static void Parse(string fileName, out Train train)
		{
			XDocument xml = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			List<XElement> trainNodes = xml.XPathSelectElements("/openBVE/Train").ToList();

			if (!trainNodes.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"No train nodes defined in XML file {fileName}");
				train = new Train();
				return;
			}

			train = ParseTrainNode(fileName, trainNodes.First());

			if (!train.Cars.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"A train requires at least one car in {fileName}");

				train.Cars.Add(new ControlledMotorCar());
			}

			if (train.InitialDriverCar >= train.Cars.Count)
			{
				Interface.AddMessage(MessageType.Error, false, $"InitialDriverCar must be less than the total number of cars in {fileName}");

				train.InitialDriverCar = train.Cars.Count - 1;
			}

			if (!train.Cars.OfType<MotorCar>().Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"A train requires at least one motor car in {fileName}");

				train.Cars[0] = new UncontrolledMotorCar(train.Cars[0]);
			}

			if (!train.Cars.OfType<ControlledMotorCar>().Any() && !train.Cars.OfType<ControlledTrailerCar>().Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"A train requires at least one driver car in {fileName}");

				MotorCar motorCar = train.Cars[train.InitialDriverCar] as MotorCar;

				if (motorCar != null)
				{
					train.Cars[train.InitialDriverCar] = new ControlledMotorCar(motorCar);
				}
				else
				{
					train.Cars[train.InitialDriverCar] = new ControlledTrailerCar(train.Cars[train.InitialDriverCar]);
				}
			}

			train.ApplyPowerNotchesToCar();
			train.ApplyBrakeNotchesToCar();
			train.ApplyLocoBrakeNotchesToCar();
		}

		private static Train ParseTrainNode(string fileName, XElement parent)
		{
			Train train = new Train();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			bool isCompatibility = false;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "version":
						if (value.Any())
						{
							int[] version = value.Split('.')
								.Select(x =>
								{
									int result;

									if (!NumberFormats.TryParseIntVb6(x, out result) || result < 0)
									{
										Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.Where(x => x >= 0)
								.ToArray();

							int minVersionLength = Math.Min(version.Length, currentVersion.Count);

							for (int i = 0; i < minVersionLength; i++)
							{
								if (version[i] > currentVersion[i])
								{
									isCompatibility = false;
									break;
								}

								isCompatibility = true;

								if (version[i] < currentVersion[i])
								{
									break;
								}
							}

							if (version[minVersionLength - 1] == currentVersion[minVersionLength - 1] && version.Length > currentVersion.Count)
							{
								isCompatibility = false;
							}
						}
						break;
				}
			}

			if (!isCompatibility)
			{
				Interface.AddMessage(MessageType.Warning, false, $"The train.xml {fileName} was created with a newer version of openBVE. Please check for an update.");
				return train;
			}

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "handle":
						train.Handle = ParseHandleNode(fileName, keyNode);
						break;
					case "device":
						train.Device = ParseDeviceNode(fileName, keyNode);
						break;
					case "initialdrivercar":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								train.InitialDriverCar = result;
							}
						}
						break;
					case "cars":
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of {key} was defined in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							break;
						}

						ParseCarsNode(fileName, keyNode, train.Cars);
						break;
					case "couplers":
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of {key} was defined in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							break;
						}

						ParseCouplersNode(fileName, keyNode, train.Couplers);
						break;
					case "version":
						// Ignore
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}
			return train;
		}

		private static Handle ParseHandleNode(string fileName, XElement parent)
		{
			Handle handle = new Handle();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "handletype":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Handle.HandleTypes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.HandleType = (Handle.HandleTypes)result;
							}
						}
						break;
					case "powernotches":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.PowerNotches = result;
							}
						}
						break;
					case "brakenotches":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.BrakeNotches = result;
							}
						}
						break;
					case "powernotchreducesteps":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.PowerNotchReduceSteps = result;
							}
						}
						break;
					case "ebhandlebehaviour":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Handle.HandleTypes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.HandleBehaviour = (Handle.EbHandleBehaviour)result;
							}
						}
						break;
					case "locobraketype":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Handle.LocoBrakeType), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.LocoBrake = (Handle.LocoBrakeType)result;
							}
						}
						break;
					case "locobrakenotches":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.LocoBrakeNotches = result;
							}
						}
						break;
					case "driverpowernotches":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.DriverPowerNotches = result;
							}
						}
						break;
					case "driverbrakenotches":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								handle.DriverBrakeNotches = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			if (handle.DriverPowerNotches > handle.PowerNotches)
			{
				Interface.AddMessage(MessageType.Error, false, $"DriverPowerNotches must be less than or equal to PowerNotches in {section} in {fileName}");

				handle.DriverPowerNotches = handle.PowerNotches;
			}

			if (handle.DriverBrakeNotches > handle.BrakeNotches)
			{
				Interface.AddMessage(MessageType.Error, false, $"DriverBrakeNotches must be less than or equal to BrakeNotches in {section} in {fileName}");

				handle.DriverBrakeNotches = handle.BrakeNotches;
			}

			return handle;
		}

		private static Device ParseDeviceNode(string fileName, XElement parent)
		{
			Device device = new Device();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "ats":
						device.Ats = ParseDeviceAtsNode(fileName, keyNode);
						break;
					case "atc":
						device.Atc = ParseDeviceAtcNode(fileName, keyNode);
						break;
					case "eb":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.Eb = result;
							}
						}
						break;
					case "constspeed":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.ConstSpeed = result;
							}
						}
						break;
					case "holdbrake":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.HoldBrake = result;
							}
						}
						break;
					case "readhesiondevice":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.ReAdhesionDevices), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.ReAdhesionDevice = (Device.ReAdhesionDevices)result;
							}
						}
						break;
					case "loadcompensatingdevice":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.LoadCompensatingDevice = result;
							}
						}
						break;
					case "passalarm":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.PassAlarmModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.PassAlarm = (Device.PassAlarmModes)result;
							}
						}
						break;
					case "dooropenmode":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.DoorModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.DoorOpenMode = (Device.DoorModes)result;
							}
						}
						break;
					case "doorclosemode":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.DoorModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.DoorCloseMode = (Device.DoorModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return device;
		}

		private static Device.AtsModes ParseDeviceAtsNode(string fileName, XElement parent)
		{
			Device.AtsModes ats = Device.AtsModes.AtsSn;

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "type":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.AtsModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								ats = (Device.AtsModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return ats;
		}

		private static Device.AtcModes ParseDeviceAtcNode(string fileName, XElement parent)
		{
			Device.AtcModes atc = Device.AtcModes.None;

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "type":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.AtcModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								atc = (Device.AtcModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return atc;
		}

		private static void ParseCarsNode(string fileName, XElement parent, ICollection<Car> cars)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "car")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid car node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				cars.Add(ParseCarNode(fileName, keyNode));
			}
		}

		private static Car ParseCarNode(string fileName, XElement parent)
		{
			Car car;

			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			bool isMotorCar = false;
			bool isControlledCar = false;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "ismotorcar":
						if (value.Any())
						{
							if (!bool.TryParse(value, out isMotorCar))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "iscontrolledcar":
						if (value.Any())
						{
							if (!bool.TryParse(value, out isControlledCar))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
				}
			}

			if (isMotorCar)
			{
				if (isControlledCar)
				{
					car = new ControlledMotorCar();
				}
				else
				{
					car = new UncontrolledMotorCar();
				}
			}
			else
			{
				if (isControlledCar)
				{
					car = new ControlledTrailerCar();
				}
				else
				{
					car = new UncontrolledTrailerCar();
				}
			}

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "mass":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.Mass = result;
							}
						}
						break;
					case "length":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.Length = result;
							}
						}
						break;
					case "width":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.Width = result;
							}
						}
						break;
					case "height":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.Height = result;
							}
						}
						break;
					case "centerofgravityheight":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.CenterOfGravityHeight = result;
							}
						}
						break;
					case "leftdoorwidth":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.LeftDoorWidth = result;
							}
						}
						break;
					case "leftdoormaxtolerance":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.LeftDoorMaxTolerance = result;
							}
						}
						break;
					case "rightdoorwidth":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.RightDoorWidth = result;
							}
						}
						break;
					case "rightdoormaxtolerance":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.RightDoorMaxTolerance = result;
							}
						}
						break;
					case "axles":
						if (value.Any())
						{
							string[] values = value.Split(',');

							if (values.Length == 2)
							{
								double front, rear;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out front))
								{
									Interface.AddMessage(MessageType.Error, false, $"Front must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out rear))
								{
									Interface.AddMessage(MessageType.Error, false, $"Rear must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (front <= rear)
								{
									Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be less than Front in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									car.DefinedAxles = true;
									car.FrontAxle = front;
									car.RearAxle = rear;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "frontbogie":
						car.FrontBogie = ParseBogieNode(fileName, keyNode);
						break;
					case "rearbogie":
						car.RearBogie = ParseBogieNode(fileName, keyNode);
						break;
					case "exposedfrontalarea":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.ExposedFrontalArea = result;
							}
						}
						break;
					case "unexposedfrontalarea":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.UnexposedFrontalArea = result;
							}
						}
						break;
					case "performance":
						car.Performance = ParsePerformanceNode(fileName, keyNode);
						break;
					case "delay":
						car.Delay = ParseDelayNode(fileName, keyNode);
						break;
					case "move":
						car.Move = ParseMoveNode(fileName, keyNode);
						break;
					case "brake":
						car.Brake = ParseBrakeNode(fileName, keyNode);
						break;
					case "pressure":
						car.Pressure = ParsePressureNode(fileName, keyNode);
						break;
					case "acceleration":
						if (isMotorCar)
						{
							((MotorCar)car).Acceleration = ParseAccelerationNode(fileName, keyNode);
						}
						else
						{
							Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in trailer car at line {lineNumber.ToString(culture)} in XML file {fileName}");
						}
						break;
					case "motor":
						if (isMotorCar)
						{
							((MotorCar)car).Motor = ParseMotorNode(fileName, keyNode);
						}
						else
						{
							Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in trailer car at line {lineNumber.ToString(culture)} in XML file {fileName}");
						}
						break;
					case "reversed":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.Reversed = result;
							}
						}
						break;
					case "object":
						if (value.Any())
						{
							if (Path.ContainsInvalidChars(value))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								string file = Path.CombineFile(basePath, value);

								if (!File.Exists(file))
								{
									Interface.AddMessage(MessageType.Warning, true, $"The {key} object {file} does not exist in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								car.Object = file;
							}
						}
						break;
					case "loadingsway":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.LoadingSway = result;
							}
						}
						break;
					case "cab":
						if (isControlledCar)
						{
							Cab cab = ParseCabNode(fileName, keyNode);

							if (isMotorCar)
							{
								((ControlledMotorCar)car).Cab = cab;
							}
							else
							{
								((ControlledTrailerCar)car).Cab = cab;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in uncontrolled car at line {lineNumber.ToString(culture)} in XML file {fileName}");
						}
						break;
					case "ismotorcar":
					case "iscontrolledcar":
						// Ignore
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return car;
		}

		private static Car.Bogie ParseBogieNode(string fileName, XElement parent)
		{
			Car.Bogie bogie = new Car.Bogie();

			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "axles":
						if (value.Any())
						{
							string[] values = value.Split(',');

							if (values.Length == 2)
							{
								double front, rear;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out front))
								{
									Interface.AddMessage(MessageType.Error, false, $"Front must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out rear))
								{
									Interface.AddMessage(MessageType.Error, false, $"Rear must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (front <= rear)
								{
									Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be less than Front in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									bogie.DefinedAxles = true;
									bogie.FrontAxle = front;
									bogie.RearAxle = rear;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "reversed":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								bogie.Reversed = result;
							}
						}
						break;
					case "object":
						if (value.Any())
						{
							if (Path.ContainsInvalidChars(value))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								string file = Path.CombineFile(basePath, value);

								if (!File.Exists(file))
								{
									Interface.AddMessage(MessageType.Warning, true, $"The {key} object {file} does not exist in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								bogie.Object = file;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return bogie;
		}

		private static Performance ParsePerformanceNode(string fileName, XElement parent)
		{
			Performance performance = new Performance();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "deceleration":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								performance.Deceleration = result;
							}
						}
						break;
					case "coefficientofstaticfriction":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								performance.CoefficientOfStaticFriction = result;
							}
						}
						break;
					case "coefficientofrollingresistance":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								performance.CoefficientOfRollingResistance = result;
							}
						}
						break;
					case "aerodynamicdragcoefficient":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								performance.AerodynamicDragCoefficient = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return performance;
		}

		private static Delay ParseDelayNode(string fileName, XElement parent)
		{
			Delay delay = new Delay();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "power":
						ParseDelayEntriesNode(fileName, keyNode, delay.Power);
						break;
					case "brake":
						ParseDelayEntriesNode(fileName, keyNode, delay.Brake);
						break;
					case "locobrake":
						ParseDelayEntriesNode(fileName, keyNode, delay.LocoBrake);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return delay;
		}

		private static void ParseDelayEntriesNode(string fileName, XElement parent, ICollection<Delay.Entry> entries)
		{
			entries.Clear();

			double[] up = new double[0];
			double[] down = new double[0];

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "up":
						if (value.Any())
						{
							up = value.Split(',')
								.Select(x =>
								{
									double result;

									if (!NumberFormats.TryParseDoubleVb6(x, out result) || result < 0.0)
									{
										Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.Where(x => x >= 0.0)
								.ToArray();
						}
						break;
					case "down":
						if (value.Any())
						{
							down = value.Split(',')
								.Select(x =>
								{
									double result;

									if (!NumberFormats.TryParseDoubleVb6(x, out result) || result < 0.0)
									{
										Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.Where(x => x >= 0.0)
								.ToArray();
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			for (int i = 0; i < Math.Max(up.Length, down.Length); i++)
			{
				Delay.Entry entry = new Delay.Entry();

				if (i < up.Length)
				{
					entry.Up = up[i];
				}

				if (i < down.Length)
				{
					entry.Down = down[i];
				}

				entries.Add(entry);
			}
		}

		private static Move ParseMoveNode(string fileName, XElement parent)
		{
			Move move = new Move();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "jerkpowerup":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.JerkPowerUp = result;
							}
						}
						break;
					case "jerkpowerdown":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.JerkPowerDown = result;
							}
						}
						break;
					case "jerkbrakeup":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.JerkBrakeUp = result;
							}
						}
						break;
					case "jerkbrakedown":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.JerkBrakeDown = result;
							}
						}
						break;
					case "brakecylinderup":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.BrakeCylinderUp = result;
							}
						}
						break;
					case "brakecylinderdown":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								move.BrakeCylinderDown = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return move;
		}

		private static Brake ParseBrakeNode(string fileName, XElement parent)
		{
			Brake brake = new Brake();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "braketype":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Brake.BrakeTypes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brake.BrakeType = (Brake.BrakeTypes)result;
							}
						}
						break;
					case "locobraketype":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Brake.LocoBrakeTypes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brake.LocoBrakeType = (Brake.LocoBrakeTypes)result;
							}
						}
						break;
					case "brakecontrolsystem":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Brake.BrakeControlSystems), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brake.BrakeControlSystem = (Brake.BrakeControlSystems)result;
							}
						}
						break;
					case "brakecontrolspeed":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brake.BrakeControlSpeed = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return brake;
		}

		private static Pressure ParsePressureNode(string fileName, XElement parent)
		{
			Pressure pressure = new Pressure();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "brakecylinderservicemaximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								pressure.BrakeCylinderServiceMaximumPressure = result;
							}
						}
						break;
					case "brakecylinderemergencymaximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								pressure.BrakeCylinderEmergencyMaximumPressure = result;
							}
						}
						break;
					case "mainreservoirminimumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								pressure.MainReservoirMinimumPressure = result;
							}
						}
						break;
					case "mainreservoirmaximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								pressure.MainReservoirMaximumPressure = result;
							}
						}
						break;
					case "brakepipenormalpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								pressure.BrakePipeNormalPressure = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return pressure;
		}

		private static Acceleration ParseAccelerationNode(string fileName, XElement parent)
		{
			Acceleration acceleration = new Acceleration();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "entry")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid entry node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				if (value.Any())
				{
					string[] values = value.Split(',');

					if (values.Length == 5)
					{
						double a0, a1, v1, v2, e;

						if (!NumberFormats.TryParseDoubleVb6(values[0], out a0) || a0 < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"A0 must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseDoubleVb6(values[1], out a1) || a1 < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"A1 must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseDoubleVb6(values[2], out v1) || v1 < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"V1 must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseDoubleVb6(values[3], out v2) || v2 < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"V2 must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseDoubleVb6(values[4], out e))
						{
							Interface.AddMessage(MessageType.Error, false, $"E must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							if (v2 < v1)
							{
								double x = v1;
								v1 = v2;
								v2 = x;
							}

							acceleration.Entries.Add(new Acceleration.Entry { A0 = a0, A1 = a1, V1 = v1, V2 = v2, E = e });
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Exactly five arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					}
				}
			}

			return acceleration;
		}

		private static Motor ParseMotorNode(string fileName, XElement parent)
		{
			Motor motor = new Motor();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "powertracks":
						ParseMotorTracksNode(fileName, keyNode, motor, Motor.TrackType.Power);
						break;
					case "braketracks":
						ParseMotorTracksNode(fileName, keyNode, motor, Motor.TrackType.Brake);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return motor;
		}

		private static void ParseMotorTracksNode(string fileName, XElement parent, Motor baseMotor, Motor.TrackType trackType)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "track")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid track node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				baseMotor.Tracks.Add(ParseMotorTrackNode(fileName, keyNode, baseMotor, trackType));
			}
		}

		private static Motor.Track ParseMotorTrackNode(string fileName, XElement parent, Motor baseMotor, Motor.TrackType trackType)
		{
			List<TrainManager.MotorSound.Vertex<float>> pitchVertices = new List<TrainManager.MotorSound.Vertex<float>>();
			List<TrainManager.MotorSound.Vertex<float>> volumeVertices = new List<TrainManager.MotorSound.Vertex<float>>();
			List<TrainManager.MotorSound.Vertex<int, SoundBuffer>> soundIndexVertices = new List<TrainManager.MotorSound.Vertex<int, SoundBuffer>>();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "pitch":
						ParseMotorVerticesNode(fileName, keyNode, pitchVertices);
						break;
					case "volume":
						ParseMotorVerticesNode(fileName, keyNode, volumeVertices);
						break;
					case "soundindex":
						ParseMotorVerticesNode(fileName, keyNode, soundIndexVertices);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return Motor.Track.MotorSoundTableToTrack(baseMotor, trackType, new TrainManager.MotorSound.Table { PitchVertices = pitchVertices.ToArray(), GainVertices = volumeVertices.ToArray(), BufferVertices = soundIndexVertices.ToArray() }, x => x, x => x, x => x);
		}

		private static void ParseMotorVerticesNode(string fileName, XElement parent, ICollection<TrainManager.MotorSound.Vertex<float>> vertices)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "vertex")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid vertex node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				if (value.Any())
				{
					string[] values = value.Split(',');

					if (values.Length == 2)
					{
						float x, y;

						if (!NumberFormats.TryParseFloatVb6(values[0], out x) || x < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"X must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseFloatVb6(values[1], out y) || y < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"Y must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							vertices.Add(new TrainManager.MotorSound.Vertex<float> { X = x, Y = y });
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					}
				}
			}
		}

		private static void ParseMotorVerticesNode(string fileName, XElement parent, ICollection<TrainManager.MotorSound.Vertex<int, SoundBuffer>> vertices)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "vertex")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid vertex node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				if (value.Any())
				{
					string[] values = value.Split(',');

					if (values.Length == 2)
					{
						float x;
						int y;

						if (!NumberFormats.TryParseFloatVb6(values[0], out x) || x < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"X must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseIntVb6(values[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, $"Y must be a integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							if (y < 0)
							{
								y = -1;
							}

							vertices.Add(new TrainManager.MotorSound.Vertex<int, SoundBuffer> { X = x, Y = y });
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					}
				}
			}
		}

		private static Cab ParseCabNode(string fileName, XElement parent)
		{
			Cab cab = new EmbeddedCab();

			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			bool isExternalCab = false;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "panel":
						if (keyNode.HasElements)
						{
							isExternalCab = false;
							cab = new EmbeddedCab();

							PanelCfgXml.Parse(fileName, keyNode, ((EmbeddedCab)cab).Panel);
						}
						else
						{
							isExternalCab = true;
							cab = new ExternalCab();

							if (value.Any())
							{
								if (Path.ContainsInvalidChars(value))
								{
									Interface.AddMessage(MessageType.Error, false, $"Value contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									string file = Path.CombineFile(basePath, value);

									if (!File.Exists(file))
									{
										Interface.AddMessage(MessageType.Warning, true, $"The {key} object {file} does not exist in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									((ExternalCab)cab).FileName = file;
								}
							}
						}
						break;
				}
			}

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "position":
						if (value.Any())
						{
							string[] values = value.Split(',');

							if (values.Length == 3)
							{
								double x, y, z;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[2], out z))
								{
									Interface.AddMessage(MessageType.Error, false, $"Z must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									cab.PositionX = x;
									cab.PositionY = y;
									cab.PositionZ = z;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly three arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "camerarestriction":
						if (isExternalCab)
						{
							((ExternalCab)cab).CameraRestriction = ParseCameraRestrictionNode(fileName, keyNode);
						}
						else
						{
							Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in uncontrolled car at line {lineNumber.ToString(culture)} in XML file {fileName}");
						}
						break;
					case "panel":
						// Ignore
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return cab;
		}

		private static CameraRestriction ParseCameraRestrictionNode(string fileName, XElement parent)
		{
			CameraRestriction cameraRestriction = new CameraRestriction();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "forwards":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedForwards = true;
								cameraRestriction.Forwards = result;
							}
						}
						break;
					case "backwards":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedBackwards = true;
								cameraRestriction.Backwards = result;
							}
						}
						break;
					case "left":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedLeft = true;
								cameraRestriction.Left = result;
							}
						}
						break;
					case "right":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedRight = true;
								cameraRestriction.Right = result;
							}
						}
						break;
					case "up":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedUp = true;
								cameraRestriction.Up = result;
							}
						}
						break;
					case "down":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedDown = true;
								cameraRestriction.Down = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			if (cameraRestriction.DefinedForwards && cameraRestriction.DefinedBackwards && cameraRestriction.Forwards < cameraRestriction.Backwards)
			{
				Interface.AddMessage(MessageType.Error, false, $"Backwards is expected to be less than or equal to Forwards in {section} in {fileName}");

				cameraRestriction.DefinedForwards = cameraRestriction.DefinedBackwards = false;
			}

			if (cameraRestriction.DefinedLeft && cameraRestriction.DefinedRight && cameraRestriction.Right < cameraRestriction.Left)
			{
				Interface.AddMessage(MessageType.Error, false, $"Left is expected to be less than or equal to Right in {section} in {fileName}");

				cameraRestriction.DefinedLeft = cameraRestriction.DefinedRight = false;
			}

			if (cameraRestriction.DefinedUp && cameraRestriction.DefinedDown && cameraRestriction.Up < cameraRestriction.Down)
			{
				Interface.AddMessage(MessageType.Error, false, $"Down is expected to be less than or equal to Up in {section} in {fileName}");

				cameraRestriction.DefinedUp = cameraRestriction.DefinedDown = false;
			}

			return cameraRestriction;
		}

		private static void ParseCouplersNode(string fileName, XElement parent, ICollection<Coupler> couplers)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "coupler")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid coupler node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				couplers.Add(ParseCouplerNode(fileName, keyNode));
			}
		}

		private static Coupler ParseCouplerNode(string fileName, XElement parent)
		{
			Coupler coupler = new Coupler();

			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "distances":
						if (value.Any())
						{
							string[] values = value.Split(',');

							if (values.Length == 2)
							{
								double min, max;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out min))
								{
									Interface.AddMessage(MessageType.Error, false, $"Minimum must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out max))
								{
									Interface.AddMessage(MessageType.Error, false, $"Maximum must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (min > max)
								{
									Interface.AddMessage(MessageType.Error, false, $"Minimum is expected to be less than Maximum in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									coupler.Min = min;
									coupler.Max = max;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "object":
						if (value.Any())
						{
							if (Path.ContainsInvalidChars(value))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								string file = Path.CombineFile(basePath, value);

								if (!File.Exists(file))
								{
									Interface.AddMessage(MessageType.Warning, true, $"The {key} object {file} does not exist in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								coupler.Object = file;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return coupler;
		}
	}
}
