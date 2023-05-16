using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.World;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.IntermediateFile
{
	internal static partial class IntermediateFile
	{
		private static readonly Version editorVersion = new Version(1, 8, 0, 0);
		private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

		private enum FileVersion
		{
			v1700,
			v1800,
			Newer
		}

		internal static void Parse(string fileName, out Train train, out Sound sound)
		{
			XDocument xml = XDocument.Load(fileName);

			if (xml.XPathSelectElement("/openBVE") != null)
			{
				throw new InvalidDataException();
			}

			FileVersion fileVersion = ParseFileVersion(xml.XPathSelectElement("/TrainEditor/Version"));

			if (fileVersion == FileVersion.Newer)
			{
				Interface.AddMessage(MessageType.Warning, false, $"The .te {fileName} was created with a newer version of TrainEditor2. Please check for an update.");
			}

			train = ParseTrainNode(fileName, fileVersion, xml.XPathSelectElement("/TrainEditor/Train"));
			sound = ParseSoundsNode(fileName, xml.XPathSelectElement("/TrainEditor/Sounds"));
		}

		private static FileVersion ParseFileVersion(XElement parent)
		{
			if (parent == null)
			{
				return FileVersion.v1700;
			}

			return new Version((string)parent) > editorVersion ? FileVersion.Newer : FileVersion.v1800;
		}

		private static Train ParseTrainNode(string fileName, FileVersion fileVersion, XElement parent)
		{
			Train train = new Train
			{
				Handle = ParseHandleNode(parent.Element("Handle")),
				Device = ParseDeviceNode(parent.Element("Device")),
				InitialDriverCar = fileVersion > FileVersion.v1700 ? (int)parent.Element("InitialDriverCar") : (int)parent.XPathSelectElement("Cab/DriverCar"),
				Cars = new ObservableCollection<Car>(parent.XPathSelectElements("Cars/Car").Select(x => ParseCarNode(fileName, fileVersion, x))),
				Couplers = new ObservableCollection<Coupler>(parent.XPathSelectElements("Couplers/Coupler").Select(x => ParseCouplerNode(fileName, x)))
			};


			if (fileVersion == FileVersion.v1700)
			{
				MotorCar motorCar = train.Cars[train.InitialDriverCar] as MotorCar;
				Cab cab;

				if (motorCar != null)
				{
					ControlledMotorCar controlledMotorCar = new ControlledMotorCar(motorCar);
					cab = controlledMotorCar.Cab;

					train.Cars[train.InitialDriverCar] = controlledMotorCar;
				}
				else
				{
					ControlledTrailerCar controlledTrailerCar = new ControlledTrailerCar(train.Cars[train.InitialDriverCar]);
					cab = controlledTrailerCar.Cab;

					train.Cars[train.InitialDriverCar] = controlledTrailerCar;
				}

				double[] position = ((string)parent.XPathSelectElement("Cab/Position")).Split(',').Select(double.Parse).ToArray();

				cab.PositionX = new Quantity.Length(position[0], UnitOfLength.Millimeter);
				cab.PositionY = new Quantity.Length(position[1], UnitOfLength.Millimeter);
				cab.PositionZ = new Quantity.Length(position[2], UnitOfLength.Millimeter);

				((EmbeddedCab)cab).Panel = ParsePanelNode(fileName, parent.XPathSelectElement("/TrainEditor/Panel"));
			}

			return train;
		}

		private static Handle ParseHandleNode(XElement parent)
		{
			return new Handle
			{
				HandleType = (Handle.HandleTypes)Enum.Parse(typeof(Handle.HandleTypes), (string)parent.Element("HandleType")),
				PowerNotches = (int)parent.Element("PowerNotches"),
				BrakeNotches = (int)parent.Element("BrakeNotches"),
				PowerNotchReduceSteps = (int)parent.Element("PowerNotchReduceSteps"),
				HandleBehaviour = (Handle.EbHandleBehaviour)Enum.Parse(typeof(Handle.EbHandleBehaviour), (string)parent.Element("EbHandleBehaviour")),
				LocoBrake = (Handle.LocoBrakeType)Enum.Parse(typeof(Handle.LocoBrakeType), (string)parent.Element("LocoBrakeType")),
				LocoBrakeNotches = (int)parent.Element("LocoBrakeNotches"),
				DriverPowerNotches = (int)parent.Element("DriverPowerNotches"),
				DriverBrakeNotches = (int)parent.Element("DriverBrakeNotches")
			};
		}

		private static Device ParseDeviceNode(XElement parent)
		{
			return new Device
			{
				Ats = (Device.AtsModes)Enum.Parse(typeof(Device.AtsModes), (string)parent.Element("Ats")),
				Atc = (Device.AtcModes)Enum.Parse(typeof(Device.AtcModes), (string)parent.Element("Atc")),
				Eb = (bool)parent.Element("Eb"),
				ConstSpeed = (bool)parent.Element("ConstSpeed"),
				HoldBrake = (bool)parent.Element("HoldBrake"),
				LoadCompensatingDevice = (double)parent.Element("LoadCompensatingDevice"),
				PassAlarm = (Device.PassAlarmModes)Enum.Parse(typeof(Device.PassAlarmModes), (string)parent.Element("PassAlarm")),
				DoorOpenMode = (Device.DoorModes)Enum.Parse(typeof(Device.DoorModes), (string)parent.Element("DoorOpenMode")),
				DoorCloseMode = (Device.DoorModes)Enum.Parse(typeof(Device.DoorModes), (string)parent.Element("DoorCloseMode"))
			};
		}

		private static Car ParseCarNode(string fileName, FileVersion fileVersion, XElement parent)
		{
			Car car;

			bool isMotorCar = (bool)parent.Element("MotorCar");
			bool isControlledCar = parent.Element("Cab") != null && (bool)parent.Element("ControlledCar");

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

			car.Mass = Quantity.Mass.Parse(parent.Element("Mass"), false, UnitOfWeight.MetricTonnes);
			car.Length = Quantity.Length.Parse(parent.Element("Length"));
			car.Width = Quantity.Length.Parse(parent.Element("Width"));
			car.Height = Quantity.Length.Parse(parent.Element("Height"));
			car.CenterOfGravityHeight = Quantity.Length.Parse(parent.Element("CenterOfGravityHeight"));

			car.DefinedAxles = (bool)parent.Element("DefinedAxles");
			car.FrontAxle = Quantity.Length.Parse(parent.Element("FrontAxle"));
			car.RearAxle = Quantity.Length.Parse(parent.Element("RearAxle"));

			car.FrontBogie = ParseBogieNode(fileName, parent.Element("FrontBogie"));
			car.RearBogie = ParseBogieNode(fileName, parent.Element("RearBogie"));

			car.ExposedFrontalArea = Quantity.Area.Parse(parent.Element("ExposedFrontalArea"));
			car.UnexposedFrontalArea = Quantity.Area.Parse(parent.Element("UnexposedFrontalArea"));

			car.Performance = ParsePerformanceNode(parent.Element("Performance"));
			car.Delay = ParseDelayNode(fileVersion, parent.Element("Delay"));

			if (fileVersion > FileVersion.v1700)
			{
				car.Jerk = ParseJerkNode(parent.Element("Jerk"));
			}
			else
			{
				car.Jerk.Power.Up = new Quantity.Jerk((double)parent.XPathSelectElement("Move/JerkPowerUp"), Unit.Jerk.CentimeterPerSecondCubed);
				car.Jerk.Power.Down = new Quantity.Jerk((double)parent.XPathSelectElement("Move/JerkPowerDown"), Unit.Jerk.CentimeterPerSecondCubed);
				car.Jerk.Brake.Up = new Quantity.Jerk((double)parent.XPathSelectElement("Move/JerkBrakeUp"), Unit.Jerk.CentimeterPerSecondCubed);
				car.Jerk.Brake.Down = new Quantity.Jerk((double)parent.XPathSelectElement("Move/JerkBrakeDown"), Unit.Jerk.CentimeterPerSecondCubed);
			}

			car.Brake = ParseBrakeNode(parent.Element("Brake"));
			car.Pressure = ParsePressureNode(fileVersion, parent.Element("Pressure"));

			car.Reversed = (bool)parent.Element("Reversed");
			car.Object = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("Object"));
			car.LoadingSway = (bool)parent.Element("LoadingSway");

			if (fileVersion > FileVersion.v1700)
			{
				car.LeftDoor = ParseDoorNode(parent.Element("LeftDoor"));
				car.RightDoor = ParseDoorNode(parent.Element("RightDoor"));
			}
			else
			{
				car.LeftDoor.Width = car.RightDoor.Width = new Quantity.Length((double)parent.XPathSelectElement("../../Device/DoorWidth"), UnitOfLength.Millimeter);
				car.LeftDoor.MaxTolerance = car.RightDoor.MaxTolerance = new Quantity.Length((double)parent.XPathSelectElement("../../Device/DoorMaxTolerance"), UnitOfLength.Millimeter);
			}

			car.ReAdhesionDevice = (Car.ReAdhesionDevices)Enum.Parse(typeof(Car.ReAdhesionDevices), fileVersion > FileVersion.v1700 ? (string)parent.Element("ReAdhesionDevice") : (string)parent.XPathSelectElement("../../Device/ReAdhesionDevice"));

			MotorCar motorCar = car as MotorCar;

			if (motorCar != null)
			{
				motorCar.Acceleration = ParseAccelerationNode(parent.Element("Acceleration"));
				motorCar.Motor = ParseMotorNode(fileVersion, parent.Element("Motor"));
			}

			if (isControlledCar)
			{
				Cab cab = ParseCabNode(fileName, parent.Element("Cab"));

				if (isMotorCar)
				{
					((ControlledMotorCar)car).Cab = cab;
				}
				else
				{
					((ControlledTrailerCar)car).Cab = cab;
				}
			}

			return car;
		}

		private static Car.Bogie ParseBogieNode(string fileName, XElement parent)
		{
			return new Car.Bogie
			{
				DefinedAxles = (bool)parent.Element("DefinedAxles"),
				FrontAxle = Quantity.Length.Parse(parent.Element("FrontAxle")),
				RearAxle = Quantity.Length.Parse(parent.Element("RearAxle")),
				Reversed = (bool)parent.Element("Reversed"),
				Object = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("Object"))
			};
		}

		private static Performance ParsePerformanceNode(XElement parent)
		{
			return new Performance
			{
				Deceleration = Quantity.Acceleration.Parse(parent.Element("Deceleration"), false, Unit.Acceleration.KilometerPerHourPerSecond),
				CoefficientOfStaticFriction = (double)parent.Element("CoefficientOfStaticFriction"),
				CoefficientOfRollingResistance = (double)parent.Element("CoefficientOfRollingResistance"),
				AerodynamicDragCoefficient = (double)parent.Element("AerodynamicDragCoefficient")
			};
		}

		private static Delay ParseDelayNode(FileVersion fileVersion, XElement parent)
		{
			return new Delay
			{
				Power = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements($"{(fileVersion > FileVersion.v1700 ? "Power" : "DelayPower")}/Entry").Select(ParseDelayEntryNode)),
				Brake = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements($"{(fileVersion > FileVersion.v1700 ? "Brake" : "DelayBrake")}/Entry").Select(ParseDelayEntryNode)),
				LocoBrake = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements($"{(fileVersion > FileVersion.v1700 ? "LocoBrake" : "DelayLocoBrake")}/Entry").Select(ParseDelayEntryNode))
			};
		}

		private static Delay.Entry ParseDelayEntryNode(XElement parent)
		{
			return new Delay.Entry
			{
				Up = Quantity.Time.Parse(parent.Element("Up")),
				Down = Quantity.Time.Parse(parent.Element("Down"))
			};
		}

		private static Jerk ParseJerkNode(XElement parent)
		{
			return new Jerk
			{
				Power = ParseJerkEntryNode(parent.Element("Power")),
				Brake = ParseJerkEntryNode(parent.Element("Brake"))
			};
		}

		private static Jerk.Entry ParseJerkEntryNode(XElement parent)
		{
			return new Jerk.Entry
			{
				Up = Quantity.Jerk.Parse(parent.Element("Up")),
				Down = Quantity.Jerk.Parse(parent.Element("Down"))
			};
		}

		private static Brake ParseBrakeNode(XElement parent)
		{
			return new Brake
			{
				BrakeType = (Brake.BrakeTypes)Enum.Parse(typeof(Brake.BrakeTypes), (string)parent.Element("BrakeType")),
				LocoBrakeType = (Brake.LocoBrakeTypes)Enum.Parse(typeof(Brake.LocoBrakeTypes), (string)parent.Element("LocoBrakeType")),
				BrakeControlSystem = (Brake.BrakeControlSystems)Enum.Parse(typeof(Brake.BrakeControlSystems), (string)parent.Element("BrakeControlSystem")),
				BrakeControlSpeed = Quantity.Velocity.Parse(parent.Element("BrakeControlSpeed"), false, Unit.Velocity.KilometerPerHour)
			};
		}

		private static Pressure ParsePressureNode(FileVersion fileVersion, XElement parent)
		{
			if (fileVersion > FileVersion.v1700)
			{
				return new Pressure
				{
					Compressor = ParseCompressorNode(parent.Element("Compressor")),
					MainReservoir = ParseMainReservoirNode(parent.Element("MainReservoir")),
					AuxiliaryReservoir = ParseAuxiliaryReservoirNode(parent.Element("AuxiliaryReservoir")),
					EqualizingReservoir = ParseEqualizingReservoirNode(parent.Element("EqualizingReservoir")),
					BrakePipe = ParseBrakePipeNode(parent.Element("BrakePipe")),
					StraightAirPipe = ParseStraightAirPipeNode(parent.Element("StraightAirPipe")),
					BrakeCylinder = ParseBrakeCylinderNode(parent.Element("BrakeCylinder"))
				};
			}

			return new Pressure
			{
				MainReservoir = new MainReservoir
				{
					MinimumPressure = new Quantity.Pressure((double)parent.Element("MainReservoirMinimumPressure"), Unit.Pressure.Kilopascal),
					MaximumPressure = new Quantity.Pressure((double)parent.Element("MainReservoirMaximumPressure"), Unit.Pressure.Kilopascal)
				},
				BrakePipe = new BrakePipe
				{
					NormalPressure = new Quantity.Pressure((double)parent.Element("BrakePipeNormalPressure"), Unit.Pressure.Kilopascal)
				},
				BrakeCylinder = new BrakeCylinder
				{
					ServiceMaximumPressure = new Quantity.Pressure((double)parent.Element("BrakeCylinderServiceMaximumPressure"), Unit.Pressure.Kilopascal),
					EmergencyMaximumPressure = new Quantity.Pressure((double)parent.Element("BrakeCylinderEmergencyMaximumPressure"), Unit.Pressure.Kilopascal)
				}
			};
		}

		private static Compressor ParseCompressorNode(XElement parent)
		{
			return new Compressor
			{
				Rate = Quantity.PressureRate.Parse(parent.Element("Rate"))
			};
		}

		private static MainReservoir ParseMainReservoirNode(XElement parent)
		{
			return new MainReservoir
			{
				MinimumPressure = Quantity.Pressure.Parse(parent.Element("MinimumPressure")),
				MaximumPressure = Quantity.Pressure.Parse(parent.Element("MaximumPressure"))
			};
		}

		private static AuxiliaryReservoir ParseAuxiliaryReservoirNode(XElement parent)
		{
			return new AuxiliaryReservoir
			{
				ChargeRate = Quantity.PressureRate.Parse(parent.Element("ChargeRate"))
			};
		}

		private static EqualizingReservoir ParseEqualizingReservoirNode(XElement parent)
		{
			return new EqualizingReservoir
			{
				ChargeRate = Quantity.PressureRate.Parse(parent.Element("ChargeRate")),
				ServiceRate = Quantity.PressureRate.Parse(parent.Element("ServiceRate")),
				EmergencyRate = Quantity.PressureRate.Parse(parent.Element("EmergencyRate"))
			};
		}

		private static BrakePipe ParseBrakePipeNode(XElement parent)
		{
			return new BrakePipe
			{
				NormalPressure = Quantity.Pressure.Parse(parent.Element("NormalPressure")),
				ChargeRate = Quantity.PressureRate.Parse(parent.Element("ChargeRate")),
				ServiceRate = Quantity.PressureRate.Parse(parent.Element("ServiceRate")),
				EmergencyRate = Quantity.PressureRate.Parse(parent.Element("EmergencyRate"))
			};
		}

		private static StraightAirPipe ParseStraightAirPipeNode(XElement parent)
		{
			return new StraightAirPipe
			{
				ServiceRate = Quantity.PressureRate.Parse(parent.Element("ServiceRate")),
				EmergencyRate = Quantity.PressureRate.Parse(parent.Element("EmergencyRate")),
				ReleaseRate = Quantity.PressureRate.Parse(parent.Element("ReleaseRate"))
			};
		}

		private static BrakeCylinder ParseBrakeCylinderNode(XElement parent)
		{
			return new BrakeCylinder
			{
				ServiceMaximumPressure = Quantity.Pressure.Parse(parent.Element("ServiceMaximumPressure")),
				EmergencyMaximumPressure = Quantity.Pressure.Parse(parent.Element("EmergencyMaximumPressure")),
				EmergencyRate = Quantity.PressureRate.Parse(parent.Element("EmergencyRate")),
				ReleaseRate = Quantity.PressureRate.Parse(parent.Element("ReleaseRate"))
			};
		}

		private static Car.Door ParseDoorNode(XElement parent)
		{
			return new Car.Door
			{
				Width = Quantity.Length.Parse(parent.Element("Width")),
				MaxTolerance = Quantity.Length.Parse(parent.Element("MaxTolerance"))
			};
		}

		private static Acceleration ParseAccelerationNode(XElement parent)
		{
			return new Acceleration
			{
				Entries = new ObservableCollection<Acceleration.Entry>(parent.Elements("Entry").Select(n => new Acceleration.Entry
				{
					A0 = Quantity.Acceleration.Parse(n.Element("a0"), false, Unit.Acceleration.KilometerPerHourPerSecond),
					A1 = Quantity.Acceleration.Parse(n.Element("a1"), false, Unit.Acceleration.KilometerPerHourPerSecond),
					V1 = Quantity.Velocity.Parse(n.Element("v1"), false, Unit.Velocity.KilometerPerHour),
					V2 = Quantity.Velocity.Parse(n.Element("v2"), false, Unit.Velocity.KilometerPerHour),
					E = (double)n.Element("e")
				}))
			};
		}

		private static Motor ParseMotorNode(FileVersion fileVersion, XElement parent)
		{
			Motor motor = new Motor();
			motor.Tracks = new ObservableCollection<Motor.Track>(parent.Elements("Track").Select((x, y) => ParseTrackNode(fileVersion, motor, x, y)));
			return motor;
		}

		private static Motor.Track ParseTrackNode(FileVersion fileVersion, Motor baseMotor, XElement parent, int index)
		{
			Motor.Track track = new Motor.Track(baseMotor);

			XElement type = parent.Element("Type");

			if (type != null)
			{
				track.Type = (Motor.TrackType)Enum.Parse(typeof(Motor.TrackType), (string)type);
			}
			else
			{
				track.Type = index % 4 < 2 ? Motor.TrackType.Power : Motor.TrackType.Brake;
			}

			ParseVertexLineNode(fileVersion, parent.Element("Pitch"), out track.PitchVertices, out track.PitchLines);
			ParseVertexLineNode(fileVersion, parent.Element("Volume"), out track.VolumeVertices, out track.VolumeLines);
			ParseAreaNode(parent.Element("SoundIndex"), out track.SoundIndices);

			return track;
		}

		private static void ParseVertexLineNode(FileVersion fileVersion, XElement parent, out Motor.VertexLibrary vertices, out List<Motor.Line> lines)
		{
			vertices = new Motor.VertexLibrary();

			foreach (XElement element in parent.XPathSelectElements("Vertices/Vertex"))
			{
				XElement positionNode = element.Element("Position");

				if (positionNode == null)
				{
					continue;
				}

				double[] positionValues = ((string)positionNode).Split(',').Select(double.Parse).ToArray();
				Unit.Velocity[] positionUnitValues = positionNode.Attributes("Unit").Select(x => x.Value.Split(',')).SelectMany(x => x).Select(Unit.Parse<Unit.Velocity>).ToArray();

				vertices.Add((int)element.Element("Id"), new Motor.Vertex(new Quantity.Velocity(positionValues[0], fileVersion > FileVersion.v1700 ? positionUnitValues[0] : Unit.Velocity.KilometerPerHour), positionValues[1]));
			}

			lines = new List<Motor.Line>(parent.XPathSelectElements("Lines/Line").Select(n => new Motor.Line((int)n.Element("LeftID"), (int)n.Element("RightID"))));
		}

		private static void ParseAreaNode(XElement parent, out List<Motor.Area> areas)
		{
			areas = new List<Motor.Area>(parent.XPathSelectElements("Areas/Area").Select(n => new Motor.Area(Quantity.Velocity.Parse(n.Element("LeftX"), false, Unit.Velocity.KilometerPerHour), Quantity.Velocity.Parse(n.Element("RightX"), false, Unit.Velocity.KilometerPerHour), (int)n.Element("Index"))));
		}

		private static Cab ParseCabNode(string fileName, XElement parent)
		{
			Cab cab;

			XElement panelNode = parent.Element("Panel");

			if ((bool)parent.Element("IsEmbeddedCab"))
			{
				cab = new EmbeddedCab
				{
					Panel = ParsePanelNode(fileName, panelNode)
				};
			}
			else
			{
				cab = new ExternalCab
				{
					CameraRestriction = ParseCameraRestrictionNode(parent.Element("CameraRestriction")),
					FileName = Utilities.MakeAbsolutePath(fileName, (string)panelNode)
				};
			}

			XElement positionNode = parent.Element("Position");

			if (positionNode != null)
			{
				double[] positionValues = ((string)positionNode).Split(',').Select(double.Parse).ToArray();
				UnitOfLength[] positionUnitValues = positionNode.Attributes("Unit").Select(x => x.Value.Split(',')).SelectMany(x => x).Select(Unit.Parse<UnitOfLength>).ToArray();

				cab.PositionX = new Quantity.Length(positionValues[0], positionUnitValues[0]);
				cab.PositionY = new Quantity.Length(positionValues[1], positionUnitValues[1]);
				cab.PositionZ = new Quantity.Length(positionValues[2], positionUnitValues[2]);
			}

			return cab;
		}

		private static Panel ParsePanelNode(string fileName, XElement parent)
		{
			return new Panel
			{
				This = ParseThisNode(fileName, parent.Element("This")),
				Screens = new ObservableCollection<Screen>(parent.XPathSelectElements("Screens/Screen").Select(x => ParseScreenNode(fileName, x))),
				PanelElements = new ObservableCollection<PanelElement>(parent.XPathSelectElements("PanelElements/*").Select(x => ParsePanelElementNode(fileName, x)).Where(x => x != null))
			};
		}

		private static This ParseThisNode(string fileName, XElement parent)
		{
			double[] center = ((string)parent.Element("Center")).Split(',').Select(double.Parse).ToArray();
			double[] origin = ((string)parent.Element("Origin")).Split(',').Select(double.Parse).ToArray();

			return new This
			{
				Resolution = (double)parent.Element("Resolution"),
				Left = (double)parent.Element("Left"),
				Right = (double)parent.Element("Right"),
				Top = (double)parent.Element("Top"),
				Bottom = (double)parent.Element("Bottom"),
				DaytimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("DaytimeImage")),
				NighttimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("NighttimeImage")),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor")),
				CenterX = center[0],
				CenterY = center[1],
				OriginX = origin[0],
				OriginY = origin[1]
			};
		}

		private static Screen ParseScreenNode(string fileName, XElement parent)
		{
			Screen screen = new Screen
			{
				Number = (int)parent.Element("Number"),
				Layer = (int)parent.Element("Layer"),
				PanelElements = new ObservableCollection<PanelElement>(parent.XPathSelectElements("PanelElements/*").Select(x => ParsePanelElementNode(fileName, x)).Where(x => x != null))
			};

			screen.TouchElements = new ObservableCollection<Models.Panels.TouchElement>(parent.XPathSelectElements("TouchElements/Touch").Select(x => ParseTouchElementNode(screen, x)));

			return screen;
		}

		private static PanelElement ParsePanelElementNode(string fileName, XElement parent)
		{
			PanelElement element = null;

			switch (parent.Name.LocalName)
			{
				case "PilotLamp":
					ParsePilotLampElementNode(fileName, parent, out element);
					break;
				case "Needle":
					ParseNeedleElementNode(fileName, parent, out element);
					break;
				case "DigitalNumber":
					ParseDigitalNumberElementNode(fileName, parent, out element);
					break;
				case "DigitalGauge":
					ParseDigitalGaugeElementNode(parent, out element);
					break;
				case "LinearGauge":
					ParseLinearGaugeElementNode(fileName, parent, out element);
					break;
				case "Timetable":
					ParseTimetableElementNode(parent, out element);
					break;
			}

			return element;
		}

		private static void ParsePilotLampElementNode(string fileName, XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new Models.Panels.PilotLampElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("DaytimeImage")),
				NighttimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("NighttimeImage")),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor"))
			};
		}

		private static void ParseNeedleElementNode(string fileName, XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();
			double[] origin = ((string)parent.Element("Origin")).Split(',').Select(double.Parse).ToArray();

			element = new NeedleElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("DaytimeImage")),
				NighttimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("NighttimeImage")),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor")),
				DefinedRadius = (bool)parent.Element("DefinedRadius"),
				Radius = (double)parent.Element("Radius"),
				Color = Color24.ParseHexColor((string)parent.Element("Color")),
				DefinedOrigin = (bool)parent.Element("DefinedOrigin"),
				OriginX = origin[0],
				OriginY = origin[1],
				InitialAngle = (double)parent.Element("InitialAngle"),
				LastAngle = (double)parent.Element("LastAngle"),
				Minimum = (double)parent.Element("Minimum"),
				Maximum = (double)parent.Element("Maximum"),
				DefinedNaturalFreq = (bool)parent.Element("DefinedNaturalFreq"),
				NaturalFreq = (double)parent.Element("NaturalFreq"),
				DefinedDampingRatio = (bool)parent.Element("DefinedDampingRatio"),
				DampingRatio = (double)parent.Element("DampingRatio"),
				Backstop = (bool)parent.Element("Backstop"),
				Smoothed = (bool)parent.Element("Smoothed")
			};
		}

		private static void ParseDigitalNumberElementNode(string fileName, XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new DigitalNumberElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("DaytimeImage")),
				NighttimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("NighttimeImage")),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor")),
				Interval = (int)parent.Element("Interval")
			};
		}

		private static void ParseDigitalGaugeElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new DigitalGaugeElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				Radius = (double)parent.Element("Radius"),
				Color = Color24.ParseHexColor((string)parent.Element("Color")),
				InitialAngle = (double)parent.Element("InitialAngle"),
				LastAngle = (double)parent.Element("LastAngle"),
				Minimum = (double)parent.Element("Minimum"),
				Maximum = (double)parent.Element("Maximum"),
				Step = (double)parent.Element("Step")
			};
		}

		private static void ParseLinearGaugeElementNode(string fileName, XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();
			int[] direction = ((string)parent.Element("Direction")).Split(',').Select(int.Parse).ToArray();

			element = new LinearGaugeElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("DaytimeImage")),
				NighttimeImage = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("NighttimeImage")),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor")),
				Minimum = (double)parent.Element("Minimum"),
				Maximum = (double)parent.Element("Maximum"),
				DirectionX = direction[0],
				DirectionY = direction[1],
				Width = (int)parent.Element("Width")
			};
		}

		private static void ParseTimetableElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new TimetableElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Width = (double)parent.Element("Width"),
				Height = (double)parent.Element("Height"),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor"))
			};
		}

		private static Subject ParseSubjectNode(XElement parent)
		{
			return new Subject
			{
				Base = (SubjectBase)Enum.Parse(typeof(SubjectBase), (string)parent.Element("Base")),
				BaseOption = (int)parent.Element("BaseOption"),
				Suffix = (SubjectSuffix)Enum.Parse(typeof(SubjectSuffix), (string)parent.Element("Suffix")),
				SuffixOption = (int)parent.Element("SuffixOption")
			};
		}

		private static Models.Panels.TouchElement ParseTouchElementNode(Screen screen, XElement parent)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();
			double[] size = ((string)parent.Element("Size")).Split(',').Select(double.Parse).ToArray();

			Models.Panels.TouchElement element = new Models.Panels.TouchElement(screen)
			{
				LocationX = location[0],
				LocationY = location[1],
				SizeX = size[0],
				SizeY = size[1],
				JumpScreen = (int)parent.Element("JumpScreen")
			};

			if (parent.Element("SoundIndex") != null)
			{
				element.SoundEntries.Add(new Models.Panels.TouchElement.SoundEntry
				{
					Index = (int)parent.Element("SoundIndex")
				});
			}

			if (parent.Element("CommandInfo") != null && parent.Element("CommandOption") != null)
			{
				element.CommandEntries.Add(new Models.Panels.TouchElement.CommandEntry
				{
					Info = Translations.CommandInfos.TryGetInfo((Translations.Command)Enum.Parse(typeof(Translations.Command), (string)parent.Element("CommandInfo"))),
					Option = (int)parent.Element("CommandOption")
				});
			}

			element.SoundEntries.AddRange(parent.XPathSelectElements("SoundEntries/Entry").Select(ParseTouchElementSoundEntryNode));
			element.CommandEntries.AddRange(parent.XPathSelectElements("CommandEntries/Entry").Select(ParseTouchElementCommandEntryNode));
			element.Layer = (int)parent.Element("Layer");

			return element;
		}

		private static Models.Panels.TouchElement.SoundEntry ParseTouchElementSoundEntryNode(XElement parent)
		{
			return new Models.Panels.TouchElement.SoundEntry
			{
				Index = (int)parent.Element("Index")
			};
		}

		private static Models.Panels.TouchElement.CommandEntry ParseTouchElementCommandEntryNode(XElement parent)
		{
			return new Models.Panels.TouchElement.CommandEntry
			{
				Info = Translations.CommandInfos.TryGetInfo((Translations.Command)Enum.Parse(typeof(Translations.Command), (string)parent.Element("Info"))),
				Option = (int)parent.Element("Option")
			};
		}

		private static CameraRestriction ParseCameraRestrictionNode(XElement parent)
		{
			return new CameraRestriction
			{
				DefinedForwards = (bool)parent.Element("DefinedForwards"),
				DefinedBackwards = (bool)parent.Element("DefinedBackwards"),
				DefinedLeft = (bool)parent.Element("DefinedLeft"),
				DefinedRight = (bool)parent.Element("DefinedRight"),
				DefinedUp = (bool)parent.Element("DefinedUp"),
				DefinedDown = (bool)parent.Element("DefinedDown"),
				Forwards = Quantity.Length.Parse(parent.Element("Forwards")),
				Backwards = Quantity.Length.Parse(parent.Element("Backwards")),
				Left = Quantity.Length.Parse(parent.Element("Left")),
				Right = Quantity.Length.Parse(parent.Element("Right")),
				Up = Quantity.Length.Parse(parent.Element("Up")),
				Down = Quantity.Length.Parse(parent.Element("Down"))
			};
		}

		private static Coupler ParseCouplerNode(string fileName, XElement parent)
		{
			return new Coupler
			{
				Min = Quantity.Length.Parse(parent.Element("Min")),
				Max = Quantity.Length.Parse(parent.Element("Max")),
				Object = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("Object"))
			};
		}

		private static Sound ParseSoundsNode(string fileName, XElement parent)
		{
			Sound sound = new Sound();

			ParseArraySoundNode<RunElement>(fileName, parent.Element("Run"), sound.SoundElements);
			ParseArraySoundNode<FlangeElement>(fileName, parent.Element("Flange"), sound.SoundElements);
			ParseArraySoundNode<MotorElement>(fileName, parent.Element("Motor"), sound.SoundElements);
			ParseArraySoundNode<FrontSwitchElement>(fileName, parent.Element("FrontSwitch"), sound.SoundElements);
			ParseArraySoundNode<RearSwitchElement>(fileName, parent.Element("RearSwitch"), sound.SoundElements);
			ParseArraySoundNode<BrakeElement, SoundKey.Brake>(fileName, parent.Element("Brake"), sound.SoundElements);
			ParseArraySoundNode<CompressorElement, SoundKey.Compressor>(fileName, parent.Element("Compressor"), sound.SoundElements);
			ParseArraySoundNode<SuspensionElement, SoundKey.Suspension>(fileName, parent.Element("Suspension"), sound.SoundElements);
			ParseArraySoundNode<PrimaryHornElement, SoundKey.Horn>(fileName, parent.Element("PrimaryHorn"), sound.SoundElements);
			ParseArraySoundNode<SecondaryHornElement, SoundKey.Horn>(fileName, parent.Element("SecondaryHorn"), sound.SoundElements);
			ParseArraySoundNode<MusicHornElement, SoundKey.Horn>(fileName, parent.Element("MusicHornHorn"), sound.SoundElements);
			ParseArraySoundNode<DoorElement, SoundKey.Door>(fileName, parent.Element("Door"), sound.SoundElements);
			ParseArraySoundNode<AtsElement>(fileName, parent.Element("Ats"), sound.SoundElements);
			ParseArraySoundNode<BuzzerElement, SoundKey.Buzzer>(fileName, parent.Element("Buzzer"), sound.SoundElements);
			ParseArraySoundNode<Models.Sounds.PilotLampElement, SoundKey.PilotLamp>(fileName, parent.Element("PilotLamp"), sound.SoundElements);
			ParseArraySoundNode<BrakeHandleElement, SoundKey.BrakeHandle>(fileName, parent.Element("BrakeHandle"), sound.SoundElements);
			ParseArraySoundNode<MasterControllerElement, SoundKey.MasterController>(fileName, parent.Element("MasterController"), sound.SoundElements);
			ParseArraySoundNode<ReverserElement, SoundKey.Reverser>(fileName, parent.Element("Reverser"), sound.SoundElements);
			ParseArraySoundNode<BreakerElement, SoundKey.Breaker>(fileName, parent.Element("Breaker"), sound.SoundElements);
			ParseArraySoundNode<RequestStopElement, SoundKey.RequestStop>(fileName, parent.Element("RequestStop"), sound.SoundElements);
			ParseArraySoundNode<Models.Sounds.TouchElement>(fileName, parent.Element("Touch"), sound.SoundElements);
			ParseArraySoundNode<OthersElement, SoundKey.Others>(fileName, parent.Element("Others"), sound.SoundElements);

			return sound;
		}

		private static T ParseSoundNode<T>(string fileName, XElement parent) where T : SoundElement<int>, new()
		{
			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			return new T
			{
				Key = (int)parent.Element("Key"),
				FilePath = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("FilePath")),
				DefinedPosition = (bool)parent.Element("DefinedPosition"),
				PositionX = position[0],
				PositionY = position[1],
				PositionZ = position[2],
				DefinedRadius = (bool)parent.Element("DefinedRadius"),
				Radius = (double)parent.Element("Radius")
			};
		}

		private static T ParseSoundNode<T, U>(string fileName, XElement parent) where T : SoundElement<U>, new()
		{
			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			return new T
			{
				Key = (U)Enum.Parse(typeof(U), (string)parent.Element("Key")),
				FilePath = Utilities.MakeAbsolutePath(fileName, (string)parent.Element("FilePath")),
				DefinedPosition = (bool)parent.Element("DefinedPosition"),
				PositionX = position[0],
				PositionY = position[1],
				PositionZ = position[2],
				DefinedRadius = (bool)parent.Element("DefinedRadius"),
				Radius = (double)parent.Element("Radius")
			};
		}

		private static void ParseArraySoundNode<T>(string fileName, XElement parent, ICollection<SoundElement> elements) where T : SoundElement<int>, new()
		{
			if (parent != null)
			{
				elements.AddRange(parent.Elements("Sound").Select(x => ParseSoundNode<T>(fileName, x)));
			}
		}

		private static void ParseArraySoundNode<T, U>(string fileName, XElement parent, ICollection<SoundElement> elements) where T : SoundElement<U>, new()
		{
			if (parent != null)
			{
				elements.AddRange(parent.Elements("Sound").Select(x => ParseSoundNode<T, U>(fileName, x)));
			}
		}
	}
}
