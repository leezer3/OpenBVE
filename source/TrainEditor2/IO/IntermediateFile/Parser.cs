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

namespace TrainEditor2.IO.IntermediateFile
{
	internal static partial class IntermediateFile
	{
		internal static void Parse(string fileName, out Train train, out Panel panel, out Sound sound)
		{
			XDocument xml = XDocument.Load(fileName);
			if (xml.XPathSelectElement("/openBVE") != null)
			{
				throw new InvalidDataException();
			}
			train = ParseTrainNode(xml.XPathSelectElement("/TrainEditor/Train"));
			panel = ParsePanelNode(xml.XPathSelectElement("/TrainEditor/Panel"));
			sound = ParseSoundsNode(xml.XPathSelectElement("/TrainEditor/Sounds"));
		}

		private static Train ParseTrainNode(XElement parent)
		{
			return new Train
			{
				Handle = ParseHandleNode(parent.Element("Handle")),
				Cab = ParseCabNode(parent.Element("Cab")),
				Device = ParseDeviceNode(parent.Element("Device")),
				Cars = new ObservableCollection<Car>(parent.XPathSelectElements("Cars/Car").Select(ParseCarNode)),
				Couplers = new ObservableCollection<Coupler>(parent.XPathSelectElements("Couplers/Coupler").Select(ParseCouplerNode))
			};
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

		private static Cab ParseCabNode(XElement parent)
		{
			double[] position = ((string)parent.Element("Position")).Split(',').Select(double.Parse).ToArray();

			return new Cab
			{
				PositionX = position[0],
				PositionY = position[1],
				PositionZ = position[2],
				DriverCar = (int)parent.Element("DriverCar")
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
				ReAdhesionDevice = (Device.ReAdhesionDevices)Enum.Parse(typeof(Device.ReAdhesionDevices), (string)parent.Element("ReAdhesionDevice")),
				LoadCompensatingDevice = (double)parent.Element("LoadCompensatingDevice"),
				PassAlarm = (Device.PassAlarmModes)Enum.Parse(typeof(Device.PassAlarmModes), (string)parent.Element("PassAlarm")),
				DoorOpenMode = (Device.DoorModes)Enum.Parse(typeof(Device.DoorModes), (string)parent.Element("DoorOpenMode")),
				DoorCloseMode = (Device.DoorModes)Enum.Parse(typeof(Device.DoorModes), (string)parent.Element("DoorCloseMode")),
				DoorWidth = (double)parent.Element("DoorWidth"),
				DoorMaxTolerance = (double)parent.Element("DoorMaxTolerance")
			};
		}

		private static Car ParseCarNode(XElement parent)
		{
			Car car;

			if ((bool)parent.Element("IsMotorCar"))
			{
				car = new MotorCar();
			}
			else
			{
				car = new TrailerCar();
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
			car.Delay = ParseDelayNode(parent.Element("Delay"));
			car.Move = ParseMoveNode(parent.Element("Move"));
			car.Brake = ParseBrakeNode(parent.Element("Brake"));
			car.Pressure = ParsePressureNode(parent.Element("Pressure"));

			MotorCar motorCar = car as MotorCar;

			if (motorCar != null)
			{
				motorCar.Acceleration = ParseAccelerationNode(parent.Element("Acceleration"));
				motorCar.Motor = ParseMotorNode(parent.Element("Motor"));
			}

			car.Reversed = (bool)parent.Element("Reversed");
			car.Object = (string)parent.Element("Object");
			car.LoadingSway = (bool)parent.Element("LoadingSway");

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

		private static Delay ParseDelayNode(XElement parent)
		{
			return new Delay
			{
				DelayPower = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements("DelayPower/Entry").Select(ParseDelayEntryNode)),
				DelayBrake = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements("DelayBrake/Entry").Select(ParseDelayEntryNode)),
				DelayLocoBrake = new ObservableCollection<Delay.Entry>(parent.XPathSelectElements("DelayLocoBrake/Entry").Select(ParseDelayEntryNode))
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

		private static Move ParseMoveNode(XElement parent)
		{
			return new Move
			{
				JerkPowerUp = (double)parent.Element("JerkPowerUp"),
				JerkPowerDown = (double)parent.Element("JerkPowerDown"),
				JerkBrakeUp = (double)parent.Element("JerkBrakeUp"),
				JerkBrakeDown = (double)parent.Element("JerkBrakeDown")
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

		private static Pressure ParsePressureNode(XElement parent)
		{
			return new Pressure
			{
				BrakeCylinderServiceMaximumPressure = (double)parent.Element("BrakeCylinderServiceMaximumPressure"),
				BrakeCylinderEmergencyMaximumPressure = (double)parent.Element("BrakeCylinderEmergencyMaximumPressure"),
				MainReservoirMinimumPressure = (double)parent.Element("MainReservoirMinimumPressure"),
				MainReservoirMaximumPressure = (double)parent.Element("MainReservoirMaximumPressure"),
				BrakePipeNormalPressure = (double)parent.Element("BrakePipeNormalPressure")
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
			return new Motor
			{
				Tracks = new ObservableCollection<Motor.Track>(parent.Elements("Track").Select(ParseTrackNode))
			};
		}

		private static Motor.Track ParseTrackNode(XElement parent)
		{
			Motor.Track track = new Motor.Track();

			ParseVertexLineNode(parent.Element("Pitch"), out track.PitchVertices, out track.PitchLines);
			ParseVertexLineNode(parent.Element("Volume"), out track.VolumeVertices, out track.VolumeLines);
			ParseAreaNode(parent.Element("SoundIndex"), out track.SoundIndices);

			return track;
		}

		private static void ParseVertexLineNode(XElement parent, out Motor.VertexLibrary vertices, out ObservableCollection<Motor.Line> lines)
		{
			vertices = new Motor.VertexLibrary();

			foreach (XElement n in parent.XPathSelectElements("Vertices/Vertex"))
			{
				double[] position = ((string)n.Element("Position")).Split(',').Select(double.Parse).ToArray();

				vertices.Add((int)n.Element("Id"), new Motor.Vertex(position[0], position[1]));
			}

			lines = new ObservableCollection<Motor.Line>(parent.XPathSelectElements("Lines/Line").Select(n => new Motor.Line((int)n.Element("LeftID"), (int)n.Element("RightID"))));
		}

		private static void ParseAreaNode(XElement parent, out ObservableCollection<Motor.Area> areas)
		{
			areas = new ObservableCollection<Motor.Area>(parent.XPathSelectElements("Areas/Area").Select(n => new Motor.Area((double)n.Element("LeftX"), (double)n.Element("RightX"), (int)n.Element("Index"))));
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

			return new Models.Panels.TouchElement(screen)
			{
				LocationX = location[0],
				LocationY = location[1],
				SizeX = size[0],
				SizeY = size[1],
				JumpScreen = (int)parent.Element("JumpScreen"),
				SoundIndex = (int)parent.Element("SoundIndex"),
				CommandInfo = Translations.CommandInfos.TryGetInfo((Translations.Command)Enum.Parse(typeof(Translations.Command), (string)parent.Element("CommandInfo"))),
				CommandOption = (int)parent.Element("CommandOption")
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
