using System;
using System.Collections.Generic;
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
				new XElement("DriverCar", train.InitialDriverCar.ToString(culture)),
				WriteCarsNode(fileName, train.Cars, train.Couplers)
			);
			openBVE.Add(trainNode);

			xml.Save(fileName);
		}

		private static XElement WriteHandleNode(Handle handle)
		{
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

		private static XElement WriteCarsNode(string fileName, ICollection<Car> cars, ICollection<Coupler> couplers)
		{
			XElement[] carNodes = cars.Select(x => WriteCarNode(fileName, x)).ToArray();
			XElement[] couplerNodes = couplers.Select(x => WriteCouplerNode(fileName, x)).ToArray();

			return new XElement("Cars", carNodes.Zip(couplerNodes, (x, y) => new[] { x, y }).SelectMany(x => x).Concat(carNodes.Skip(couplerNodes.Length)));
		}

		private static XElement WriteCarNode(string fileName, Car car)
		{
			XElement carNode = new XElement("Car");

			carNode.Add(
				new XElement("MotorCar", car is MotorCar),
				car.Mass.ToXElement("Mass"),
				car.Length.ToXElement("Length"),
				car.Width.ToXElement("Width"),
				car.Height.ToXElement("Height"),
				car.CenterOfGravityHeight.ToXElement("CenterOfGravityHeight")
			);

			if (car.DefinedAxles)
			{
				carNode.Add(
					new XElement("FrontAxle",
						new XAttribute("Unit", $"{car.FrontAxle.UnitValue}"),
						$"{car.FrontAxle.Value.ToString(culture)}"
					),
					new XElement("RearAxle",
						new XAttribute("Unit", $"{car.RearAxle.UnitValue}"),
						$"{car.RearAxle.Value.ToString(culture)}"
					)
				);
			}

			carNode.Add(
				WriteBogieNode(fileName, "FrontBogie", car.FrontBogie),
				WriteBogieNode(fileName, "RearBogie", car.RearBogie),
				car.ExposedFrontalArea.ToXElement("ExposedFrontalArea"),
				car.UnexposedFrontalArea.ToXElement("UnexposedFrontalArea"),
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

			carNode.Add(new XElement("ControlledCar", cab != null));

			if (cab != null)
			{
				carNode.Add(WriteCabNode(fileName, cab));
			}

			return carNode;
		}

		private static XElement WriteBogieNode(string fileName, string nodeName, Car.Bogie bogie)
		{
			XElement bogieNode = new XElement(nodeName);

			if (bogie.DefinedAxles)
			{
				bogieNode.Add(
					new XElement("FrontAxle",
						new XAttribute("Unit", $"{bogie.FrontAxle.UnitValue}"),
						$"{bogie.FrontAxle.Value.ToString(culture)}"
					),
					new XElement("RearAxle",
						new XAttribute("Unit", $"{bogie.RearAxle.UnitValue}"),
						$"{bogie.RearAxle.Value.ToString(culture)}"
					)
				);
			}

			bogieNode.Add(
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, bogie.Object))
			);

			return bogieNode;
		}

		private static XElement WritePerformanceNode(Performance performance)
		{
			return new XElement("Performance",
				performance.Deceleration.ToXElement("Deceleration"),
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
			return new XElement(nodeName,
				new XElement("Up", new XAttribute("Unit", string.Join(", ", entries.Select(x => x.Up.UnitValue))), string.Join(", ", entries.Select(x => x.Up.Value.ToString(culture)))),
				new XElement("Down", new XAttribute("Unit", string.Join(", ", entries.Select(x => x.Down.UnitValue))), string.Join(", ", entries.Select(x => x.Down.Value.ToString(culture))))
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
			return new XElement(nodeName,
				entry.Up.ToXElement("Up"),
				entry.Down.ToXElement("Down")
			);
		}

		private static XElement WriteBrakeNode(Brake brake)
		{
			return new XElement("Brake",
				new XElement("BrakeType", ((int)brake.BrakeType).ToString(culture)),
				new XElement("LocoBrakeType", ((int)brake.LocoBrakeType).ToString(culture)),
				new XElement("BrakeControlSystem", ((int)brake.BrakeControlSystem).ToString(culture)),
				brake.BrakeControlSpeed.ToXElement("BrakeControlSpeed")
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
			return new XElement("Compressor",
				compressor.Rate.ToXElement("Rate")
			);
		}

		private static XElement WriteMainReservoirNode(MainReservoir mainReservoir)
		{
			return new XElement("MainReservoir",
				mainReservoir.MinimumPressure.ToXElement("MinimumPressure"),
				mainReservoir.MaximumPressure.ToXElement("MaximumPressure")
			);
		}

		private static XElement WriteAuxiliaryReservoirNode(AuxiliaryReservoir auxiliaryReservoir)
		{
			return new XElement("AuxiliaryReservoir",
				auxiliaryReservoir.ChargeRate.ToXElement("ChargeRate")
			);
		}

		private static XElement WriteEqualizingReservoirNode(EqualizingReservoir equalizingReservoir)
		{
			return new XElement("EqualizingReservoir",
				equalizingReservoir.ChargeRate.ToXElement("ChargeRate"),
				equalizingReservoir.ServiceRate.ToXElement("ServiceRate"),
				equalizingReservoir.EmergencyRate.ToXElement("EmergencyRate")
			);
		}

		private static XElement WriteBrakePipeNode(BrakePipe brakePipe)
		{
			return new XElement("BrakePipe",
				brakePipe.NormalPressure.ToXElement("NormalPressure"),
				brakePipe.ChargeRate.ToXElement("ChargeRate"),
				brakePipe.ServiceRate.ToXElement("ServiceRate"),
				brakePipe.EmergencyRate.ToXElement("EmergencyRate")
			);
		}

		private static XElement WriteStraightAirPipeNode(StraightAirPipe straightAirPipe)
		{
			return new XElement("StraightAirPipe",
				straightAirPipe.ServiceRate.ToXElement("ServiceRate"),
				straightAirPipe.EmergencyRate.ToXElement("EmergencyRate"),
				straightAirPipe.ReleaseRate.ToXElement("ReleaseRate")
			);
		}

		private static XElement WriteBrakeCylinderNode(BrakeCylinder brakeCylinder)
		{
			return new XElement("BrakeCylinder",
				brakeCylinder.ServiceMaximumPressure.ToXElement("ServiceMaximumPressure"),
				brakeCylinder.EmergencyMaximumPressure.ToXElement("EmergencyMaximumPressure"),
				brakeCylinder.EmergencyRate.ToXElement("EmergencyRate"),
				brakeCylinder.ReleaseRate.ToXElement("ReleaseRate")
			);
		}

		private static XElement WriteDoorNode(string nodeName, Car.Door door)
		{
			return new XElement(nodeName,
				door.Width.ToXElement("Width"),
				door.MaxTolerance.ToXElement("Tolerance")
			);
		}

		private static XElement WriteAccelerationNode(Acceleration acceleration)
		{
			return new XElement("Acceleration",
				acceleration.Entries.Select(entry => new XElement("Entry",
					new XAttribute("Unit", $"{entry.A0.UnitValue}, {entry.A1.UnitValue}, {entry.V1.UnitValue}, {entry.V2.UnitValue}"),
					$"{entry.A0.Value.ToString(culture)}, {entry.A1.Value.ToString(culture)}, {entry.V1.Value.ToString(culture)}, {entry.V2.Value.ToString(culture)}, {entry.E.ToString(culture)}"))
			);
		}

		private static XElement WriteMotorNode(Motor motor)
		{
			TrainEditor.MotorSound.Table[] powerTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Power).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y)).ToArray();
			TrainEditor.MotorSound.Table[] brakeTables = motor.Tracks.Where(x => x.Type == Motor.TrackType.Brake).Select(x => Motor.Track.TrackToMotorSoundTable(x,  y => y, y => y)).ToArray();

			return new XElement("Motor",
				new XElement("PowerTracks", powerTables.Select(WriteTrackNode)),
				new XElement("BrakeTracks", brakeTables.Select(WriteTrackNode))
			);
		}

		private static XElement WriteTrackNode(TrainEditor.MotorSound.Table table)
		{
			return new XElement("Track",
				new XElement("Pitch", table.PitchVertices.Select(WriteVertexNode)),
				new XElement("Volume", table.GainVertices.Select(WriteVertexNode)),
				new XElement("SoundIndex", table.BufferVertices.Select(WriteVertexNode))
			);
		}

		private static XElement WriteVertexNode<T>(TrainEditor.MotorSound.Vertex<T> vertex) where T : struct, IConvertible
		{
			return new XElement("Vertex",
				new XAttribute("Unit", vertex.X.UnitValue),
				$"{vertex.X.Value.ToString(culture)}, {vertex.Y.ToString(culture)}"
			);
		}

		private static XElement WriteCabNode(string fileName, Cab cab)
		{
			XElement cabNode = new XElement("Cab",
				new XElement("Position",
					new XAttribute("Unit", $"{cab.PositionX.UnitValue}, {cab.PositionY.UnitValue}, {cab.PositionZ.UnitValue}"),
					$"{cab.PositionX.Value.ToString(culture)}, {cab.PositionY.Value.ToString(culture)}, {cab.PositionZ.Value.ToString(culture)}"
				)
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

		private static XElement WriteCameraRestrictionNode(CameraRestriction restriction)
		{
			XElement cameraRestrictionNode = new XElement("CameraRestriction");

			if (restriction.DefinedForwards)
			{
				cameraRestrictionNode.Add(restriction.Forwards.ToXElement("Forwards"));
			}

			if (restriction.DefinedBackwards)
			{
				cameraRestrictionNode.Add(restriction.Backwards.ToXElement("Backwards"));
			}

			if (restriction.DefinedLeft)
			{
				cameraRestrictionNode.Add(restriction.Left.ToXElement("Left"));
			}

			if (restriction.DefinedRight)
			{
				cameraRestrictionNode.Add(restriction.Right.ToXElement("Right"));
			}

			if (restriction.DefinedUp)
			{
				cameraRestrictionNode.Add(restriction.Up.ToXElement("Up"));
			}

			if (restriction.DefinedDown)
			{
				cameraRestrictionNode.Add(restriction.Down.ToXElement("Down"));
			}

			return cameraRestrictionNode;
		}

		private static XElement WriteCouplerNode(string fileName, Coupler coupler)
		{
			return new XElement("Coupler",
				new XElement("Distances",
					new XAttribute("Unit", $"{coupler.Min.UnitValue}, {coupler.Max.UnitValue}"),
					$"{coupler.Min.Value.ToString(culture)}, {coupler.Max.Value.ToString(culture)}"
				),
				new XElement("Object", Utilities.MakeRelativePath(fileName, coupler.Object))
			);
		}
	}
}
