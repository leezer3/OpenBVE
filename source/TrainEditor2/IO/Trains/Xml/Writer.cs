using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Extensions;
using TrainEditor2.IO.Panels.Xml;
using TrainEditor2.Models.Trains;
using TrainEditor2.Simulation.TrainManager;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		internal static void Write(string fileName, Train train)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE",
				new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")),
				new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema"))
			);
			xml.Add(openBVE);

			XElement trainNode = new XElement("Train",
				new XElement("Version", editorVersion.ToString()),
				WriteHandleNode(train.Handle),
				WriteDeviceNode(train.Device),
				new XElement("InitialDriverCar", train.InitialDriverCar.ToString(culture))
			);
			openBVE.Add(trainNode);

			if (train.Cars.Any())
			{
				trainNode.Add(new XElement("Cars", train.Cars.Select(x => WriteCarNode(fileName, x))));
			}

			if (train.Couplers.Any())
			{
				trainNode.Add(new XElement("Couplers", train.Couplers.Select(x => WriteCouplerNode(fileName, x))));
			}

			xml.Save(fileName);
		}

		private static XElement WriteHandleNode(Handle handle)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Handle",
				new XElement("HandleType", ((int)handle.HandleType).ToString(culture)),
				new XElement("PowerNotches", handle.PowerNotches.ToString(culture)),
				new XElement("BrakeNotches", handle.BrakeNotches.ToString(culture)),
				new XElement("PowerNotchReduceSteps", handle.PowerNotchReduceSteps.ToString(culture)),
				new XElement("EbHandleBehaviour", ((int)handle.HandleBehaviour).ToString(culture)),
				new XElement("LocoBrakeType", ((int)handle.LocoBrake).ToString(culture)),
				new XElement("LocoBrakeNotches", handle.LocoBrakeNotches.ToString(culture)),
				new XElement("DriverPowerNotches", handle.DriverPowerNotches.ToString(culture)),
				new XElement("DriverBrakeNotches", handle.DriverBrakeNotches.ToString(culture))
			);
		}

		private static XElement WriteDeviceNode(Device device)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Device",
				new XElement("Ats", new XElement("Type", ((int)device.Ats).ToString(culture))),
				new XElement("Atc", new XElement("Type", ((int)device.Atc).ToString(culture))),
				new XElement("Eb", device.Eb),
				new XElement("ConstSpeed", device.ConstSpeed),
				new XElement("HoldBrake", device.HoldBrake),
				new XElement("LoadCompensatingDevice", device.LoadCompensatingDevice.ToString(culture)),
				new XElement("PassAlarm", ((int)device.PassAlarm).ToString(culture)),
				new XElement("DoorOpenMode", ((int)device.DoorOpenMode).ToString(culture)),
				new XElement("DoorCloseMode", ((int)device.DoorCloseMode).ToString(culture))
			);
		}

		private static XElement WriteCarNode(string fileName, Car car)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement carNode = new XElement("Car");

			carNode.Add(
				new XElement("IsMotorCar", car is MotorCar),
				new XElement("Mass", car.Mass.ToString(culture)),
				new XElement("Length", car.Length.ToString(culture)),
				new XElement("Width", car.Width.ToString(culture)),
				new XElement("Height", car.Height.ToString(culture)),
				new XElement("CenterOfGravityHeight", car.CenterOfGravityHeight.ToString(culture))
			);

			if (car.DefinedAxles)
			{
				carNode.Add(new XElement("Axles", $"{car.FrontAxle.ToString(culture)}, {car.RearAxle.ToString(culture)}"));
			}

			carNode.Add(
				WriteBogieNode(fileName, "FrontBogie", car.FrontBogie),
				WriteBogieNode(fileName, "RearBogie", car.RearBogie),
				new XElement("ExposedFrontalArea", car.ExposedFrontalArea.ToString(culture)),
				new XElement("UnexposedFrontalArea", car.UnexposedFrontalArea.ToString(culture)),
				WritePerformanceNode(car.Performance),
				WriteDelayNode(car.Delay),
				WriteJerkNode(car.Jerk),
				WriteBrakeNode(car.Brake),
				WritePressureNode(car.Pressure),
				new XElement("Reversed", car.Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, car.Object)),
				new XElement("LoadingSway", car.LoadingSway),
				WriteDoorNode("LeftDoor", car.LeftDoor),
				WriteDoorNode("RightDoor", car.RightDoor),
				new XElement("ReAdhesionDevice", ((int)car.ReAdhesionDevice).ToString(culture))
			);

			MotorCar motorCar = car as MotorCar;

			if (motorCar != null)
			{
				carNode.Add(
					WriteAccelerationNode(motorCar.Acceleration),
					WriteMotorNode(motorCar.Motor)
				);
			}

			Cab cab = (car as ControlledMotorCar)?.Cab ?? (car as ControlledTrailerCar)?.Cab;

			carNode.Add(new XElement("IsControlledCar", cab != null));

			if (cab != null)
			{
				carNode.Add(WriteCabNode(fileName, cab));
			}

			return carNode;
		}

		private static XElement WriteBogieNode(string fileName, string nodeName, Car.Bogie bogie)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement bogieNode = new XElement(nodeName);

			if (bogie.DefinedAxles)
			{
				bogieNode.Add(new XElement("Axles", $"{bogie.FrontAxle.ToString(culture)}, {bogie.RearAxle.ToString(culture)}"));
			}

			bogieNode.Add(
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, bogie.Object))
			);

			return bogieNode;
		}

		private static XElement WritePerformanceNode(Performance performance)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Performance",
				new XElement("Deceleration", performance.Deceleration.ToString(culture)),
				new XElement("CoefficientOfStaticFriction", performance.CoefficientOfStaticFriction.ToString(culture)),
				new XElement("CoefficientOfRollingResistance", performance.CoefficientOfRollingResistance.ToString(culture)),
				new XElement("AerodynamicDragCoefficient", performance.AerodynamicDragCoefficient.ToString(culture))
			);
		}

		private static XElement WriteDelayNode(Delay delay)
		{
			return new XElement("Delay",
				WriteDelayEntriesNode("Power", delay.Power),
				WriteDelayEntriesNode("Brake", delay.Brake),
				WriteDelayEntriesNode("LocoBrake", delay.LocoBrake)
			);
		}

		private static XElement WriteDelayEntriesNode(string nodeName, ICollection<Delay.Entry> entries)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement(nodeName,
				new XElement("Up", string.Join(", ", entries.Select(x => x.Up.ToString(culture)))),
				new XElement("Down", string.Join(", ", entries.Select(x => x.Down.ToString(culture))))
			);
		}

		private static XElement WriteJerkNode(Jerk jerk)
		{
			return new XElement("Jerk",
				WriteJerkEntryNode("Power", jerk.Power),
				WriteJerkEntryNode("Brake", jerk.Brake)
			);
		}

		private static XElement WriteJerkEntryNode(string nodeName, Jerk.Entry entry)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement(nodeName,
				new XElement("Up", entry.Up.ToString(culture)),
				new XElement("Down", entry.Down.ToString(culture))
			);
		}

		private static XElement WriteBrakeNode(Brake brake)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Brake",
				new XElement("BrakeType", ((int)brake.BrakeType).ToString(culture)),
				new XElement("LocoBrakeType", ((int)brake.LocoBrakeType).ToString(culture)),
				new XElement("BrakeControlSystem", ((int)brake.BrakeControlSystem).ToString(culture)),
				new XElement("BrakeControlSpeed", brake.BrakeControlSpeed.ToString(culture))
			);
		}

		private static XElement WritePressureNode(Pressure pressure)
		{
			return new XElement("Pressure",
				WriteCompressorNode(pressure.Compressor),
				WriteMainReservoirNode(pressure.MainReservoir),
				WriteAuxiliaryReservoirNode(pressure.AuxiliaryReservoir),
				WriteEqualizingReservoirNode(pressure.EqualizingReservoir),
				WriteBrakePipeNode(pressure.BrakePipe),
				WriteStraightAirPipeNode(pressure.StraightAirPipe),
				WriteBrakeCylinderNode(pressure.BrakeCylinder)
			);
		}

		private static XElement WriteCompressorNode(Compressor compressor)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Compressor",
				new XElement("Rate", compressor.Rate.ToString(culture))
			);
		}

		private static XElement WriteMainReservoirNode(MainReservoir mainReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("MainReservoir",
				new XElement("MinimumPressure", mainReservoir.MinimumPressure.ToString(culture)),
				new XElement("MaximumPressure", mainReservoir.MaximumPressure.ToString(culture))
			);
		}

		private static XElement WriteAuxiliaryReservoirNode(AuxiliaryReservoir auxiliaryReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("AuxiliaryReservoir",
				new XElement("ChargeRate", auxiliaryReservoir.ChargeRate.ToString(culture))
			);
		}

		private static XElement WriteEqualizingReservoirNode(EqualizingReservoir equalizingReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("EqualizingReservoir",
				new XElement("ChargeRate", equalizingReservoir.ChargeRate.ToString(culture)),
				new XElement("ServiceRate", equalizingReservoir.ServiceRate.ToString(culture)),
				new XElement("EmergencyRate", equalizingReservoir.EmergencyRate.ToString(culture))
			);
		}

		private static XElement WriteBrakePipeNode(BrakePipe brakePipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("BrakePipe",
				new XElement("NormalPressure", brakePipe.NormalPressure.ToString(culture)),
				new XElement("ChargeRate", brakePipe.ChargeRate.ToString(culture)),
				new XElement("ServiceRate", brakePipe.ServiceRate.ToString(culture)),
				new XElement("EmergencyRate", brakePipe.EmergencyRate.ToString(culture))
			);
		}

		private static XElement WriteStraightAirPipeNode(StraightAirPipe straightAirPipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("StraightAirPipe",
				new XElement("ServiceRate", straightAirPipe.ServiceRate.ToString(culture)),
				new XElement("EmergencyRate", straightAirPipe.EmergencyRate.ToString(culture)),
				new XElement("ReleaseRate", straightAirPipe.ReleaseRate.ToString(culture))
			);
		}

		private static XElement WriteBrakeCylinderNode(BrakeCylinder brakeCylinder)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("BrakeCylinder",
				new XElement("ServiceMaximumPressure", brakeCylinder.ServiceMaximumPressure.ToString(culture)),
				new XElement("EmergencyMaximumPressure", brakeCylinder.EmergencyMaximumPressure.ToString(culture)),
				new XElement("EmergencyRate", brakeCylinder.EmergencyRate.ToString(culture)),
				new XElement("ReleaseRate", brakeCylinder.ReleaseRate.ToString(culture))
			);
		}

		private static XElement WriteDoorNode(string nodeName, Car.Door door)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement(nodeName,
				new XElement("Width", door.Width.ToString(culture)),
				new XElement("MaxTolerance", door.MaxTolerance.ToString(culture))
			);
		}

		private static XElement WriteAccelerationNode(Acceleration acceleration)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			return new XElement("Acceleration",
				acceleration.Entries.Select(entry => new XElement("Entry", $"{entry.A0.ToString(culture)}, {entry.A1.ToString(culture)}, {entry.V1.ToString(culture)}, {entry.V2.ToString(culture)}, {entry.E.ToString(culture)}"))
			);
		}

		private static XElement WriteMotorNode(Motor motor)
		{
			TrainManager.MotorSound.Table[] powerTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Power).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y, y => y)).ToArray();
			TrainManager.MotorSound.Table[] brakeTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Brake).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y, y => y)).ToArray();

			return new XElement("Motor",
				new XElement("PowerTracks", powerTables.Select(WriteTrackNode)),
				new XElement("BrakeTracks", brakeTables.Select(WriteTrackNode))
			);
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

		private static XElement WriteCabNode(string fileName, Cab cab)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement cabNode = new XElement("Cab",
				new XElement("Position", $"{cab.PositionX.ToString(culture)}, {cab.PositionY.ToString(culture)}, {cab.PositionZ.ToString(culture)}")
			);

			EmbeddedCab embeddedCab = cab as EmbeddedCab;

			if (embeddedCab != null)
			{
				PanelCfgXml.Write(fileName, cabNode, embeddedCab.Panel);
			}

			ExternalCab externalCab = cab as ExternalCab;

			if (externalCab != null)
			{
				cabNode.Add(
					WriteCameraRestrictionNode(externalCab.CameraRestriction),
					new XElement("Panel", Utilities.MakeRelativePath(fileName, externalCab.FileName))
				);
			}

			return cabNode;
		}

		private static XElement WriteCameraRestrictionNode(CameraRestriction cameraRestriction)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			XElement cameraRestrictionNode = new XElement("CameraRestriction");

			if (cameraRestriction.DefinedForwards)
			{
				cameraRestrictionNode.Add(new XElement("Forwards", cameraRestriction.Forwards.ToString(culture)));
			}

			if (cameraRestriction.DefinedBackwards)
			{
				cameraRestrictionNode.Add(new XElement("Backwards", cameraRestriction.Backwards.ToString(culture)));
			}

			if (cameraRestriction.DefinedLeft)
			{
				cameraRestrictionNode.Add(new XElement("Left", cameraRestriction.Left.ToString(culture)));
			}

			if (cameraRestriction.DefinedRight)
			{
				cameraRestrictionNode.Add(new XElement("Right", cameraRestriction.Right.ToString(culture)));
			}

			if (cameraRestriction.DefinedUp)
			{
				cameraRestrictionNode.Add(new XElement("Up", cameraRestriction.Up.ToString(culture)));
			}

			if (cameraRestriction.DefinedDown)
			{
				cameraRestrictionNode.Add(new XElement("Down", cameraRestriction.Down.ToString(culture)));
			}

			return cameraRestrictionNode;
		}

		private static XElement WriteCouplerNode(string fileName, Coupler coupler)
		{
			return new XElement("Coupler",
				new XElement("Distances", $"{coupler.Min}, {coupler.Max}"),
				new XElement("Object", Utilities.MakeRelativePath(fileName, coupler.Object))
			);
		}
	}
}
