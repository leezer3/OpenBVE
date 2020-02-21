using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
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

			train = ParseTrainNode(fileVersion, xml.XPathSelectElement("/TrainEditor/Train"));
			sound = ParseSoundsNode(xml.XPathSelectElement("/TrainEditor/Sounds"));
		}

		private static FileVersion ParseFileVersion(XElement parent)
		{
			if (parent == null)
			{
				return FileVersion.v1700;
			}

			return new Version((string)parent) > editorVersion ? FileVersion.Newer : FileVersion.v1800;
		}

		private static Train ParseTrainNode(FileVersion fileVersion, XElement parent)
		{
			Train train = new Train
			{
				Handle = ParseHandleNode(parent.Element("Handle")),
				Device = ParseDeviceNode(parent.Element("Device")),
				InitialDriverCar = fileVersion > FileVersion.v1700 ? (int)parent.Element("InitialDriverCar") : (int)parent.XPathSelectElement("Cab/DriverCar"),
				Cars = new ObservableCollection<Car>(parent.XPathSelectElements("Cars/Car").Select(x => ParseCarNode(fileVersion, x))),
				Couplers = new ObservableCollection<Coupler>(parent.XPathSelectElements("Couplers/Coupler").Select(ParseCouplerNode))
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

				cab.PositionX = position[0];
				cab.PositionY = position[1];
				cab.PositionZ = position[2];

				((EmbeddedCab)cab).Panel = ParsePanelNode(parent.XPathSelectElement("/TrainEditor/Panel"));
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

		private static Car ParseCarNode(FileVersion fileVersion, XElement parent)
		{
			Car car;

			bool isMotorCar = (bool)parent.Element("IsMotorCar");
			bool isControlledCar = parent.Element("Cab") != null && (bool)parent.Element("IsControlledCar");

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

			car.Mass = (double)parent.Element("Mass");
			car.Length = (double)parent.Element("Length");
			car.Width = (double)parent.Element("Width");
			car.Height = (double)parent.Element("Height");
			car.CenterOfGravityHeight = (double)parent.Element("CenterOfGravityHeight");

			car.DefinedAxles = (bool)parent.Element("DefinedAxles");
			car.FrontAxle = (double)parent.Element("FrontAxle");
			car.RearAxle = (double)parent.Element("RearAxle");

			car.FrontBogie = ParseBogieNode(parent.Element("FrontBogie"));
			car.RearBogie = ParseBogieNode(parent.Element("RearBogie"));

			car.ExposedFrontalArea = (double)parent.Element("ExposedFrontalArea");
			car.UnexposedFrontalArea = (double)parent.Element("UnexposedFrontalArea");

			car.Performance = ParsePerformanceNode(parent.Element("Performance"));
			car.Delay = ParseDelayNode(fileVersion, parent.Element("Delay"));

			if (fileVersion > FileVersion.v1700)
			{
				car.Jerk = ParseJerkNode(parent.Element("Jerk"));
			}
			else
			{
				car.Jerk.Power.Up = (double)parent.XPathSelectElement("Move/JerkPowerUp");
				car.Jerk.Power.Down = (double)parent.XPathSelectElement("Move/JerkPowerDown");
				car.Jerk.Brake.Up = (double)parent.XPathSelectElement("Move/JerkBrakeUp");
				car.Jerk.Brake.Down = (double)parent.XPathSelectElement("Move/JerkBrakeDown");
			}

			car.Brake = ParseBrakeNode(parent.Element("Brake"));
			car.Pressure = ParsePressureNode(fileVersion, parent.Element("Pressure"));

			car.Reversed = (bool)parent.Element("Reversed");
			car.Object = (string)parent.Element("Object");
			car.LoadingSway = (bool)parent.Element("LoadingSway");

			if (fileVersion > FileVersion.v1700)
			{
				car.LeftDoor = ParseDoorNode(parent.Element("LeftDoor"));
				car.RightDoor = ParseDoorNode(parent.Element("RightDoor"));
			}
			else
			{
				car.LeftDoor.Width = car.RightDoor.Width = (double)parent.XPathSelectElement("../../Device/DoorWidth");
				car.LeftDoor.MaxTolerance = car.RightDoor.MaxTolerance = (double)parent.XPathSelectElement("../../Device/DoorMaxTolerance");
			}

			car.ReAdhesionDevice = (Car.ReAdhesionDevices)Enum.Parse(typeof(Car.ReAdhesionDevices), fileVersion > FileVersion.v1700 ? (string)parent.Element("ReAdhesionDevice") : (string)parent.XPathSelectElement("../../Device/ReAdhesionDevice"));

			MotorCar motorCar = car as MotorCar;

			if (motorCar != null)
			{
				motorCar.Acceleration = ParseAccelerationNode(parent.Element("Acceleration"));
				motorCar.Motor = ParseMotorNode(parent.Element("Motor"));
			}

			if (isControlledCar)
			{
				Cab cab = ParseCabNode(parent.Element("Cab"));

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

		private static Car.Bogie ParseBogieNode(XElement parent)
		{
			return new Car.Bogie
			{
				DefinedAxles = (bool)parent.Element("DefinedAxles"),
				FrontAxle = (double)parent.Element("FrontAxle"),
				RearAxle = (double)parent.Element("RearAxle"),
				Reversed = (bool)parent.Element("Reversed"),
				Object = (string)parent.Element("Object")
			};
		}

		private static Performance ParsePerformanceNode(XElement parent)
		{
			return new Performance
			{
				Deceleration = (double)parent.Element("Deceleration"),
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
				Up = (double)parent.Element("Up"),
				Down = (double)parent.Element("Down")
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
				Up = (double)parent.Element("Up"),
				Down = (double)parent.Element("Down")
			};
		}

		private static Brake ParseBrakeNode(XElement parent)
		{
			return new Brake
			{
				BrakeType = (Brake.BrakeTypes)Enum.Parse(typeof(Brake.BrakeTypes), (string)parent.Element("BrakeType")),
				LocoBrakeType = (Brake.LocoBrakeTypes)Enum.Parse(typeof(Brake.LocoBrakeTypes), (string)parent.Element("LocoBrakeType")),
				BrakeControlSystem = (Brake.BrakeControlSystems)Enum.Parse(typeof(Brake.BrakeControlSystems), (string)parent.Element("BrakeControlSystem")),
				BrakeControlSpeed = (double)parent.Element("BrakeControlSpeed")
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
				MainReservoir = new MainReservoir { MinimumPressure = (double)parent.Element("MainReservoirMinimumPressure"), MaximumPressure = (double)parent.Element("MainReservoirMaximumPressure") },
				BrakePipe = new BrakePipe { NormalPressure = (double)parent.Element("BrakePipeNormalPressure") },
				BrakeCylinder = new BrakeCylinder { ServiceMaximumPressure = (double)parent.Element("BrakeCylinderServiceMaximumPressure"), EmergencyMaximumPressure = (double)parent.Element("BrakeCylinderEmergencyMaximumPressure") }
			};
		}

		private static Compressor ParseCompressorNode(XElement parent)
		{
			return new Compressor
			{
				Rate = (double)parent.Element("Rate")
			};
		}

		private static MainReservoir ParseMainReservoirNode(XElement parent)
		{
			return new MainReservoir
			{
				MinimumPressure = (double)parent.Element("MinimumPressure"),
				MaximumPressure = (double)parent.Element("MaximumPressure")
			};
		}

		private static AuxiliaryReservoir ParseAuxiliaryReservoirNode(XElement parent)
		{
			return new AuxiliaryReservoir
			{
				ChargeRate = (double)parent.Element("ChargeRate")
			};
		}

		private static EqualizingReservoir ParseEqualizingReservoirNode(XElement parent)
		{
			return new EqualizingReservoir
			{
				ChargeRate = (double)parent.Element("ChargeRate"),
				ServiceRate = (double)parent.Element("ServiceRate"),
				EmergencyRate = (double)parent.Element("EmergencyRate")
			};
		}

		private static BrakePipe ParseBrakePipeNode(XElement parent)
		{
			return new BrakePipe
			{
				NormalPressure = (double)parent.Element("NormalPressure"),
				ChargeRate = (double)parent.Element("ChargeRate"),
				ServiceRate = (double)parent.Element("ServiceRate"),
				EmergencyRate = (double)parent.Element("EmergencyRate")
			};
		}

		private static StraightAirPipe ParseStraightAirPipeNode(XElement parent)
		{
			return new StraightAirPipe
			{
				ServiceRate = (double)parent.Element("ServiceRate"),
				EmergencyRate = (double)parent.Element("EmergencyRate"),
				ReleaseRate = (double)parent.Element("ReleaseRate")
			};
		}

		private static BrakeCylinder ParseBrakeCylinderNode(XElement parent)
		{
			return new BrakeCylinder
			{
				ServiceMaximumPressure = (double)parent.Element("ServiceMaximumPressure"),
				EmergencyMaximumPressure = (double)parent.Element("EmergencyMaximumPressure"),
				EmergencyRate = (double)parent.Element("EmergencyRate"),
				ReleaseRate = (double)parent.Element("ReleaseRate")
			};
		}

		private static Car.Door ParseDoorNode(XElement parent)
		{
			return new Car.Door
			{
				Width = (double)parent.Element("Width"),
				MaxTolerance = (double)parent.Element("MaxTolerance")
			};
		}

		private static Acceleration ParseAccelerationNode(XElement parent)
		{
			return new Acceleration
			{
				Entries = new ObservableCollection<Acceleration.Entry>(parent.Elements("Entry").Select(n => new Acceleration.Entry
				{
					A0 = (double)n.Element("a0"),
					A1 = (double)n.Element("a1"),
					V1 = (double)n.Element("v1"),
					V2 = (double)n.Element("v2"),
					E = (double)n.Element("e")
				}))
			};
		}

		private static Motor ParseMotorNode(XElement parent)
		{
			Motor motor = new Motor();
			motor.Tracks = new ObservableCollection<Motor.Track>(parent.Elements("Track").Select((x, y) => ParseTrackNode(motor, x, y)));
			return motor;
		}

		private static Motor.Track ParseTrackNode(Motor baseMotor, XElement parent, int index)
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

			ParseVertexLineNode(parent.Element("Pitch"), out track.PitchVertices, out track.PitchLines);
			ParseVertexLineNode(parent.Element("Volume"), out track.VolumeVertices, out track.VolumeLines);
			ParseAreaNode(parent.Element("SoundIndex"), out track.SoundIndices);

			return track;
		}

		private static void ParseVertexLineNode(XElement parent, out Motor.VertexLibrary vertices, out List<Motor.Line> lines)
		{
			vertices = new Motor.VertexLibrary();

			foreach (XElement n in parent.XPathSelectElements("Vertices/Vertex"))
			{
				double[] position = ((string)n.Element("Position")).Split(',').Select(double.Parse).ToArray();

				vertices.Add((int)n.Element("Id"), new Motor.Vertex(position[0], position[1]));
			}

			lines = new List<Motor.Line>(parent.XPathSelectElements("Lines/Line").Select(n => new Motor.Line((int)n.Element("LeftID"), (int)n.Element("RightID"))));
		}

		private static void ParseAreaNode(XElement parent, out List<Motor.Area> areas)
		{
			areas = new List<Motor.Area>(parent.XPathSelectElements("Areas/Area").Select(n => new Motor.Area((double)n.Element("LeftX"), (double)n.Element("RightX"), (int)n.Element("Index"))));
		}

		private static Cab ParseCabNode(XElement parent)
		{
			Cab cab;

			XElement panelNode = parent.Element("Panel");

			if ((bool)parent.Element("IsEmbeddedCab"))
			{
				cab = new EmbeddedCab
				{
					Panel = ParsePanelNode(panelNode)
				};
			}
			else
			{
				cab = new ExternalCab
				{
					CameraRestriction = ParseCameraRestrictionNode(parent.Element("CameraRestriction")),
					FileName = (string)panelNode
				};
			}

			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			cab.PositionX = position[0];
			cab.PositionY = position[1];
			cab.PositionZ = position[2];

			return cab;
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
				Forwards = (double)parent.Element("Forwards"),
				Backwards = (double)parent.Element("Backwards"),
				Left = (double)parent.Element("Left"),
				Right = (double)parent.Element("Right"),
				Up = (double)parent.Element("Up"),
				Down = (double)parent.Element("Down")
			};
		}

		private static Coupler ParseCouplerNode(XElement parent)
		{
			return new Coupler
			{
				Min = (double)parent.Element("Min"),
				Max = (double)parent.Element("Max"),
				Object = (string)parent.Element("Object")
			};
		}

		private static Panel ParsePanelNode(XElement parent)
		{
			return new Panel
			{
				This = ParseThisNode(parent.Element("This")),
				Screens = new ObservableCollection<Screen>(parent.XPathSelectElements("Screens/Screen").Select(ParseScreenNode)),
				PanelElements = new ObservableCollection<PanelElement>(parent.XPathSelectElements("PanelElements/*").Select(ParsePanelElementNode).Where(x => x != null))
			};
		}

		private static This ParseThisNode(XElement parent)
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
				DaytimeImage = (string)parent.Element("DaytimeImage"),
				NighttimeImage = (string)parent.Element("NighttimeImage"),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor")),
				CenterX = center[0],
				CenterY = center[1],
				OriginX = origin[0],
				OriginY = origin[1]
			};
		}

		private static Screen ParseScreenNode(XElement parent)
		{
			Screen screen = new Screen
			{
				Number = (int)parent.Element("Number"),
				Layer = (int)parent.Element("Layer"),
				PanelElements = new ObservableCollection<PanelElement>(parent.XPathSelectElements("PanelElements/*").Select(ParsePanelElementNode).Where(x => x != null))
			};

			screen.TouchElements = new ObservableCollection<Models.Panels.TouchElement>(parent.XPathSelectElements("TouchElements/Touch").Select(x => ParseTouchElementNode(screen, x)));

			return screen;
		}

		private static PanelElement ParsePanelElementNode(XElement parent)
		{
			PanelElement element = null;

			switch (parent.Name.LocalName)
			{
				case "PilotLamp":
					ParsePilotLampElementNode(parent, out element);
					break;
				case "Needle":
					ParseNeedleElementNode(parent, out element);
					break;
				case "DigitalNumber":
					ParseDigitalNumberElementNode(parent, out element);
					break;
				case "DigitalGauge":
					ParseDigitalGaugeElementNode(parent, out element);
					break;
				case "LinearGauge":
					ParseLinearGaugeElementNode(parent, out element);
					break;
				case "Timetable":
					ParseTimetableElementNode(parent, out element);
					break;
			}

			return element;
		}

		private static void ParsePilotLampElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new Models.Panels.PilotLampElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = (string)parent.Element("DaytimeImage"),
				NighttimeImage = (string)parent.Element("NighttimeImage"),
				TransparentColor = Color24.ParseHexColor((string)parent.Element("TransparentColor"))
			};
		}

		private static void ParseNeedleElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();
			double[] origin = ((string)parent.Element("Origin")).Split(',').Select(double.Parse).ToArray();

			element = new NeedleElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = (string)parent.Element("DaytimeImage"),
				NighttimeImage = (string)parent.Element("NighttimeImage"),
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

		private static void ParseDigitalNumberElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();

			element = new DigitalNumberElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = (string)parent.Element("DaytimeImage"),
				NighttimeImage = (string)parent.Element("NighttimeImage"),
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

		private static void ParseLinearGaugeElementNode(XElement parent, out PanelElement element)
		{
			double[] location = ((string)parent.Element("Location")).Split(',').Select(double.Parse).ToArray();
			int[] direction = ((string)parent.Element("Direction")).Split(',').Select(int.Parse).ToArray();

			element = new LinearGaugeElement
			{
				LocationX = location[0],
				LocationY = location[1],
				Layer = (int)parent.Element("Layer"),
				Subject = ParseSubjectNode(parent.Element("Subject")),
				DaytimeImage = (string)parent.Element("DaytimeImage"),
				NighttimeImage = (string)parent.Element("NighttimeImage"),
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

		private static Sound ParseSoundsNode(XElement parent)
		{
			Sound sound = new Sound();

			ParseArraySoundNode<RunElement>(parent.Element("Run"), sound.SoundElements);
			ParseArraySoundNode<FlangeElement>(parent.Element("Flange"), sound.SoundElements);
			ParseArraySoundNode<MotorElement>(parent.Element("Motor"), sound.SoundElements);
			ParseArraySoundNode<FrontSwitchElement>(parent.Element("FrontSwitch"), sound.SoundElements);
			ParseArraySoundNode<RearSwitchElement>(parent.Element("RearSwitch"), sound.SoundElements);
			ParseArraySoundNode<BrakeElement, BrakeKey>(parent.Element("Brake"), sound.SoundElements);
			ParseArraySoundNode<CompressorElement, CompressorKey>(parent.Element("Compressor"), sound.SoundElements);
			ParseArraySoundNode<SuspensionElement, SuspensionKey>(parent.Element("Suspension"), sound.SoundElements);
			ParseArraySoundNode<PrimaryHornElement, HornKey>(parent.Element("PrimaryHorn"), sound.SoundElements);
			ParseArraySoundNode<SecondaryHornElement, HornKey>(parent.Element("SecondaryHorn"), sound.SoundElements);
			ParseArraySoundNode<MusicHornElement, HornKey>(parent.Element("MusicHornHorn"), sound.SoundElements);
			ParseArraySoundNode<DoorElement, DoorKey>(parent.Element("Door"), sound.SoundElements);
			ParseArraySoundNode<AtsElement>(parent.Element("Ats"), sound.SoundElements);
			ParseArraySoundNode<BuzzerElement, BuzzerKey>(parent.Element("Buzzer"), sound.SoundElements);
			ParseArraySoundNode<Models.Sounds.PilotLampElement, PilotLampKey>(parent.Element("PilotLamp"), sound.SoundElements);
			ParseArraySoundNode<BrakeHandleElement, BrakeHandleKey>(parent.Element("BrakeHandle"), sound.SoundElements);
			ParseArraySoundNode<MasterControllerElement, MasterControllerKey>(parent.Element("MasterController"), sound.SoundElements);
			ParseArraySoundNode<ReverserElement, ReverserKey>(parent.Element("Reverser"), sound.SoundElements);
			ParseArraySoundNode<BreakerElement, BreakerKey>(parent.Element("Breaker"), sound.SoundElements);
			ParseArraySoundNode<RequestStopElement, RequestStopKey>(parent.Element("RequestStop"), sound.SoundElements);
			ParseArraySoundNode<Models.Sounds.TouchElement>(parent.Element("Touch"), sound.SoundElements);
			ParseArraySoundNode<OthersElement, OthersKey>(parent.Element("Others"), sound.SoundElements);

			return sound;
		}

		private static T ParseSoundNode<T>(XElement parent) where T : SoundElement<int>, new()
		{
			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			return new T
			{
				Key = (int)parent.Element("Key"),
				FilePath = (string)parent.Element("FilePath"),
				DefinedPosition = (bool)parent.Element("DefinedPosition"),
				PositionX = position[0],
				PositionY = position[1],
				PositionZ = position[2],
				DefinedRadius = (bool)parent.Element("DefinedRadius"),
				Radius = (double)parent.Element("Radius")
			};
		}

		private static T ParseSoundNode<T, U>(XElement parent) where T : SoundElement<U>, new()
		{
			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			return new T
			{
				Key = (U)Enum.Parse(typeof(U), (string)parent.Element("Key")),
				FilePath = (string)parent.Element("FilePath"),
				DefinedPosition = (bool)parent.Element("DefinedPosition"),
				PositionX = position[0],
				PositionY = position[1],
				PositionZ = position[2],
				DefinedRadius = (bool)parent.Element("DefinedRadius"),
				Radius = (double)parent.Element("Radius")
			};
		}

		private static void ParseArraySoundNode<T>(XElement parent, ICollection<SoundElement> elements) where T : SoundElement<int>, new()
		{
			if (parent != null)
			{
				elements.AddRange(parent.Elements("Sound").Select(ParseSoundNode<T>));
			}
		}

		private static void ParseArraySoundNode<T, U>(XElement parent, ICollection<SoundElement> elements) where T : SoundElement<U>, new()
		{
			if (parent != null)
			{
				elements.AddRange(parent.Elements("Sound").Select(ParseSoundNode<T, U>));
			}
		}
	}
}
