using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.Simulation.TrainManager;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		internal static void Write(string fileName, Train train)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE");
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			xml.Add(openBVE);

			XElement trainNode = new XElement("Train");
			openBVE.Add(trainNode);

			WriteHandleNode(trainNode, train.Handle);
			WriteCabNode(trainNode, train.Cab);
			WriteDeviceNode(trainNode, train.Device);

			XElement carsNode = new XElement("Cars");
			trainNode.Add(carsNode);

			foreach (Car car in train.Cars)
			{
				WriteCarNode(fileName, carsNode, car);
			}

			XElement couplersNode = new XElement("Couplers");
			trainNode.Add(couplersNode);

			foreach (Coupler coupler in train.Couplers)
			{
				WriteCouplerNode(couplersNode, coupler);
			}

			xml.Save(fileName);
		}

		private static void WriteHandleNode(XElement parent, Handle handle)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Handle",
				new XElement("HandleType", ((int)handle.HandleType).ToString(culture)),
				new XElement("PowerNotches", handle.PowerNotches.ToString(culture)),
				new XElement("BrakeNotches", handle.BrakeNotches.ToString(culture)),
				new XElement("PowerNotchReduceSteps", handle.PowerNotchReduceSteps.ToString(culture)),
				new XElement("EbHandleBehaviour", ((int)handle.HandleBehaviour).ToString(culture)),
				new XElement("LocoBrakeType", ((int)handle.LocoBrake).ToString(culture)),
				new XElement("LocoBrakeNotches", handle.LocoBrakeNotches.ToString(culture)),
				new XElement("DriverPowerNotches", handle.DriverPowerNotches.ToString(culture)),
				new XElement("DriverBrakeNotches", handle.DriverBrakeNotches.ToString(culture))
			));
		}

		private static void WriteCabNode(XElement parent, Cab cab)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Cab",
				new XElement("Position", $"{cab.PositionX.ToString(culture)}, {cab.PositionY.ToString(culture)}, {cab.PositionZ.ToString(culture)}"),
				new XElement("DriverCar", cab.DriverCar.ToString(culture))
			));
		}

		private static void WriteDeviceNode(XElement parent, Device device)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Device",
				new XElement("Ats", ((int)device.Ats).ToString(culture)),
				new XElement("Atc", ((int)device.Atc).ToString(culture)),
				new XElement("Eb", device.Eb),
				new XElement("ConstSpeed", device.ConstSpeed),
				new XElement("HoldBrake", device.HoldBrake),
				new XElement("ReAdhesionDevice", ((int)device.ReAdhesionDevice).ToString(culture)),
				new XElement("LoadCompensatingDevice", device.LoadCompensatingDevice.ToString(culture)),
				new XElement("PassAlarm", ((int)device.PassAlarm).ToString(culture)),
				new XElement("DoorOpenMode", ((int)device.DoorOpenMode).ToString(culture)),
				new XElement("DoorCloseMode", ((int)device.DoorCloseMode).ToString(culture))
			));
		}

		private static void WriteCarNode(string fileName, XElement parent, Car car)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement carNode = new XElement("Car");
			parent.Add(carNode);

			carNode.Add(
				new XElement("IsMotorCar", car is MotorCar),
				new XElement("Mass", car.Mass.ToString(culture)),
				new XElement("Length", car.Length.ToString(culture)),
				new XElement("Width", car.Width.ToString(culture)),
				new XElement("Height", car.Height.ToString(culture)),
				new XElement("CenterOfGravityHeight", car.CenterOfGravityHeight.ToString(culture)),
				new XElement("LeftDoorWidth", car.LeftDoorWidth.ToString(culture)),
				new XElement("LeftDoorMaxTolerance", car.LeftDoorMaxTolerance.ToString(culture)),
				new XElement("RightDoorWidth", car.RightDoorWidth.ToString(culture)),
				new XElement("RightDoorMaxTolerance", car.RightDoorMaxTolerance.ToString(culture))
			);

			if (car.DefinedAxles)
			{
				carNode.Add(new XElement("FrontAxle", car.FrontAxle.ToString(culture)));
				carNode.Add(new XElement("RearAxle", car.RearAxle.ToString(culture)));
			}

			WriteBogieNode(fileName, carNode, "FrontBogie", car.FrontBogie);
			WriteBogieNode(fileName, carNode, "RearBogie", car.RearBogie);

			carNode.Add(new XElement("ExposedFrontalArea", car.ExposedFrontalArea.ToString(culture)));
			carNode.Add(new XElement("UnexposedFrontalArea", car.UnexposedFrontalArea.ToString(culture)));

			WritePerformanceNode(carNode, car.Performance);
			WriteDelayNode(carNode, car.Delay);
			WriteMoveNode(carNode, car.Move);
			WriteBrakeNode(carNode, car.Brake);
			WritePressureNode(carNode, car.Pressure);

			MotorCar motorCar = car as MotorCar;

			if (motorCar != null)
			{
				WriteAccelerationNode(carNode, motorCar.Acceleration);
				WriteMotorNode(carNode, motorCar.Motor);
			}

			carNode.Add(
				new XElement("Reversed", car.Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, car.Object)),
				new XElement("LoadingSway", car.LoadingSway)
			);
		}

		private static void WriteBogieNode(string fileName, XElement parent, string nodeName, Car.Bogie bogie)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement bogieNode = new XElement(nodeName);
			parent.Add(bogieNode);

			if (bogie.DefinedAxles)
			{
				bogieNode.Add(new XElement("FrontAxle", bogie.FrontAxle.ToString(culture)));
				bogieNode.Add(new XElement("RearAxle", bogie.RearAxle.ToString(culture)));
			}

			bogieNode.Add(
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, bogie.Object))
			);
		}

		private static void WritePerformanceNode(XElement parent, Performance performance)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Performance",
				new XElement("Deceleration", performance.Deceleration.ToString(culture)),
				new XElement("CoefficientOfStaticFriction", performance.CoefficientOfStaticFriction.ToString(culture)),
				new XElement("CoefficientOfRollingResistance", performance.CoefficientOfRollingResistance.ToString(culture)),
				new XElement("AerodynamicDragCoefficient", performance.AerodynamicDragCoefficient.ToString(culture))
			));
		}

		private static void WriteDelayNode(XElement parent, Delay delay)
		{
			parent.Add(new XElement("Delay",
				WriteDelayEntriesNode("DelayPower", delay.DelayPower),
				WriteDelayEntriesNode("DelayBrake", delay.DelayBrake),
				WriteDelayEntriesNode("DelayLocoBrake", delay.DelayLocoBrake)
			));
		}

		private static XElement WriteDelayEntriesNode(string nodeName, ObservableCollection<Delay.Entry> entries)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement(nodeName,
				new XElement("Up", string.Join(", ", entries.Select(x => x.Up.ToString(culture)))),
				new XElement("Down", string.Join(", ", entries.Select(x => x.Down.ToString(culture))))
			);
		}

		private static void WriteMoveNode(XElement parent, Move move)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Move",
				new XElement("JerkPowerUp", move.JerkPowerUp.ToString(culture)),
				new XElement("JerkPowerDown", move.JerkPowerDown.ToString(culture)),
				new XElement("JerkBrakeUp", move.JerkBrakeUp.ToString(culture)),
				new XElement("JerkBrakeDown", move.JerkBrakeDown.ToString(culture)),
				new XElement("BrakeCylinderUp", move.BrakeCylinderUp.ToString(culture)),
				new XElement("BrakeCylinderDown", move.BrakeCylinderDown.ToString(culture))
			));
		}

		private static void WriteBrakeNode(XElement parent, Brake brake)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Brake",
				new XElement("BrakeType", ((int)brake.BrakeType).ToString(culture)),
				new XElement("LocoBrakeType", ((int)brake.LocoBrakeType).ToString(culture)),
				new XElement("BrakeControlSystem", ((int)brake.BrakeControlSystem).ToString(culture)),
				new XElement("BrakeControlSpeed", brake.BrakeControlSpeed.ToString(culture))
			));
		}

		private static void WritePressureNode(XElement parent, Pressure pressure)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Pressure",
				new XElement("BrakeCylinderServiceMaximumPressure", pressure.BrakeCylinderServiceMaximumPressure.ToString(culture)),
				new XElement("BrakeCylinderEmergencyMaximumPressure", pressure.BrakeCylinderEmergencyMaximumPressure.ToString(culture)),
				new XElement("MainReservoirMinimumPressure", pressure.MainReservoirMinimumPressure.ToString(culture)),
				new XElement("MainReservoirMaximumPressure", pressure.MainReservoirMaximumPressure.ToString(culture)),
				new XElement("BrakePipeNormalPressure", pressure.BrakePipeNormalPressure.ToString(culture))
			));
		}

		private static void WriteAccelerationNode(XElement parent, Acceleration acceleration)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			parent.Add(new XElement("Acceleration",
				acceleration.Entries.Select(entry => new XElement("Entry", $"{entry.A0.ToString(culture)}, {entry.A1.ToString(culture)}, {entry.V1.ToString(culture)}, {entry.V2.ToString(culture)}, {entry.E.ToString(culture)}"))
			));
		}

		private static void WriteMotorNode(XElement parent, Motor motor)
		{
			TrainManager.MotorSound.Table[] powerTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Power).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y, y => y)).ToArray();
			TrainManager.MotorSound.Table[] brakeTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Brake).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y, y => y)).ToArray();

			parent.Add(new XElement("Motor",
				new XElement("PowerTracks", powerTables.Select(WriteTrackNode)),
				new XElement("BrakeTracks", brakeTables.Select(WriteTrackNode))
			));
		}

		private static XElement WriteTrackNode(TrainManager.MotorSound.Table table)
		{
			return new XElement("Track",
				new XElement("Pitch", table.PitchVertices.Select(WriteVertexNode)),
				new XElement("Volume", table.GainVertices.Select(WriteVertexNode)),
				new XElement("SoundIndex", table.BufferVertices.Select(WriteVertexNode))
			);
		}

		private static XElement WriteVertexNode<T>(TrainManager.MotorSound.Vertex<T> vertex) where T : struct, IConvertible
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Vertex", $"{vertex.X.ToString(culture)}, {vertex.Y.ToString(culture)}");
		}

		private static void WriteCouplerNode(XElement parent, Coupler coupler)
		{
			parent.Add(new XElement("Coupler",
				new XElement("Min", coupler.Min),
				new XElement("Max", coupler.Max),
				new XElement("Object", coupler.Object)
			));
		}
	}
}
