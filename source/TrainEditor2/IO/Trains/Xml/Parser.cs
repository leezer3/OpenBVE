using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static readonly Version editorVersion = new Version(1, 8, 0, 0);

		private enum FileVersion
		{
			v1800,
			Newer
		}

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

			FileVersion fileVersion = FileVersion.v1800;

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
							Version result;

							if (!Version.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid format in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								break;
							}

							fileVersion = result > editorVersion ? FileVersion.Newer : FileVersion.v1800;
						}
						break;
				}
			}

			if (fileVersion == FileVersion.Newer)
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
					case "jerk":
						car.Jerk = ParseJerkNode(fileName, keyNode);
						break;
					case "brake":
						car.Brake = ParseBrakeNode(fileName, keyNode);
						break;
					case "pressure":
						car.Pressure = ParsePressureNode(fileName, keyNode);
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
					case "leftdoor":
						car.LeftDoor = ParseDoorNode(fileName, keyNode);
						break;
					case "rightdoor":
						car.RightDoor = ParseDoorNode(fileName, keyNode);
						break;
					case "readhesiondevice":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Car.ReAdhesionDevices), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								car.ReAdhesionDevice = (Car.ReAdhesionDevices)result;
							}
						}
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

		private static Car.Door ParseDoorNode(string fileName, XElement parent)
		{
			Car.Door door = new Car.Door();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "width":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								door.Width = result;
							}
						}
						break;
					case "maxtolerance":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								door.MaxTolerance = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return door;
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
