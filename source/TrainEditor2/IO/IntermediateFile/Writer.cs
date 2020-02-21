using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.IO.IntermediateFile
{
	internal static partial class IntermediateFile
	{
		internal static void Write(string fileName, Train train, Sound sound)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement trainEditor = new XElement("TrainEditor",
				new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")),
				new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")),
				new XElement("Version", editorVersion.ToString()),
				WriteTrainNode(train),
				WriteSoundsNode(sound)
			);

			xml.Add(trainEditor);
			xml.Save(fileName);
		}

		private static XElement WriteTrainNode(Train train)
		{
			return new XElement("Train",
				WriteHandleNode(train.Handle),
				WriteDeviceNode(train.Device),
				new XElement("InitialDriverCar", train.InitialDriverCar),
				new XElement("Cars", train.Cars.Select(WriteCarNode)),
				new XElement("Couplers", train.Couplers.Select(WriteCouplerNode))
			);
		}

		private static XElement WriteHandleNode(Handle handle)
		{
			return new XElement("Handle",
				new XElement("HandleType", handle.HandleType),
				new XElement("PowerNotches", handle.PowerNotches),
				new XElement("BrakeNotches", handle.BrakeNotches),
				new XElement("PowerNotchReduceSteps", handle.PowerNotchReduceSteps),
				new XElement("EbHandleBehaviour", handle.HandleBehaviour),
				new XElement("LocoBrakeType", handle.LocoBrake),
				new XElement("LocoBrakeNotches", handle.LocoBrakeNotches),
				new XElement("DriverPowerNotches", handle.DriverPowerNotches),
				new XElement("DriverBrakeNotches", handle.DriverBrakeNotches)
			);
		}

		private static XElement WriteDeviceNode(Device device)
		{
			return new XElement("Device",
				new XElement("Ats", device.Ats),
				new XElement("Atc", device.Atc),
				new XElement("Eb", device.Eb),
				new XElement("ConstSpeed", device.ConstSpeed),
				new XElement("HoldBrake", device.HoldBrake),
				new XElement("LoadCompensatingDevice", device.LoadCompensatingDevice),
				new XElement("PassAlarm", device.PassAlarm),
				new XElement("DoorOpenMode", device.DoorOpenMode),
				new XElement("DoorCloseMode", device.DoorCloseMode)
			);
		}

		private static XElement WriteCarNode(Car car)
		{
			XElement carNode = new XElement("Car");

			carNode.Add(
				new XElement("IsMotorCar", car is MotorCar),
				new XElement("Mass", car.Mass),
				new XElement("Length", car.Length),
				new XElement("Width", car.Width),
				new XElement("Height", car.Height),
				new XElement("CenterOfGravityHeight", car.CenterOfGravityHeight),
				new XElement("DefinedAxles", car.DefinedAxles),
				new XElement("FrontAxle", car.FrontAxle),
				new XElement("RearAxle", car.RearAxle),
				WriteBogieNode("FrontBogie", car.FrontBogie),
				WriteBogieNode("RearBogie", car.RearBogie),
				new XElement("ExposedFrontalArea", car.ExposedFrontalArea),
				new XElement("UnexposedFrontalArea", car.UnexposedFrontalArea),
				WritePerformanceNode(car.Performance),
				WriteDelayNode(car.Delay),
				WriteJerkNode(car.Jerk),
				WriteBrakeNode(car.Brake),
				WritePressureNode(car.Pressure),
				new XElement("Reversed", car.Reversed),
				new XElement("Object", car.Object),
				new XElement("LoadingSway", car.LoadingSway),
				WriteDoorNode("LeftDoor", car.LeftDoor),
				WriteDoorNode("RightDoor", car.RightDoor),
				new XElement("ReAdhesionDevice", car.ReAdhesionDevice)
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
				carNode.Add(WriteCabNode(cab));
			}

			return carNode;
		}

		private static XElement WriteBogieNode(string nodeName, Car.Bogie bogie)
		{
			return new XElement(nodeName,
				new XElement("DefinedAxles", bogie.DefinedAxles),
				new XElement("FrontAxle", bogie.FrontAxle),
				new XElement("RearAxle", bogie.RearAxle),
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", bogie.Object)
			);
		}

		private static XElement WritePerformanceNode(Performance performance)
		{
			return new XElement("Performance",
				new XElement("Deceleration", performance.Deceleration),
				new XElement("CoefficientOfStaticFriction", performance.CoefficientOfStaticFriction),
				new XElement("CoefficientOfRollingResistance", performance.CoefficientOfRollingResistance),
				new XElement("AerodynamicDragCoefficient", performance.AerodynamicDragCoefficient)
			);
		}

		private static XElement WriteDelayNode(Delay delay)
		{
			return new XElement("Delay",
				new XElement("Power", delay.Power.Select(WriteDelayEntryNode)),
				new XElement("Brake", delay.Brake.Select(WriteDelayEntryNode)),
				new XElement("LocoBrake", delay.LocoBrake.Select(WriteDelayEntryNode))
			);
		}

		private static XElement WriteDelayEntryNode(Delay.Entry entry)
		{
			return new XElement("Entry",
				new XElement("Up", entry.Up),
				new XElement("Down", entry.Down)
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
				new XElement("Up", entry.Up),
				new XElement("Down", entry.Down)
			);
		}

		private static XElement WriteBrakeNode(Brake brake)
		{
			return new XElement("Brake",
				new XElement("BrakeType", brake.BrakeType),
				new XElement("LocoBrakeType", brake.LocoBrakeType),
				new XElement("BrakeControlSystem", brake.BrakeControlSystem),
				new XElement("BrakeControlSpeed", brake.BrakeControlSpeed)
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
				new XElement("Rate", compressor.Rate)
			);
		}

		private static XElement WriteMainReservoirNode(MainReservoir mainReservoir)
		{
			return new XElement("MainReservoir",
				new XElement("MinimumPressure", mainReservoir.MinimumPressure),
				new XElement("MaximumPressure", mainReservoir.MaximumPressure)
			);
		}

		private static XElement WriteAuxiliaryReservoirNode(AuxiliaryReservoir auxiliaryReservoir)
		{
			return new XElement("AuxiliaryReservoir",
				new XElement("ChargeRate", auxiliaryReservoir.ChargeRate)
			);
		}

		private static XElement WriteEqualizingReservoirNode(EqualizingReservoir equalizingReservoir)
		{
			return new XElement("EqualizingReservoir",
				new XElement("ChargeRate", equalizingReservoir.ChargeRate),
				new XElement("ServiceRate", equalizingReservoir.ServiceRate),
				new XElement("EmergencyRate", equalizingReservoir.EmergencyRate)
			);
		}

		private static XElement WriteBrakePipeNode(BrakePipe brakePipe)
		{
			return new XElement("BrakePipe",
				new XElement("NormalPressure", brakePipe.NormalPressure),
				new XElement("ChargeRate", brakePipe.ChargeRate),
				new XElement("ServiceRate", brakePipe.ServiceRate),
				new XElement("EmergencyRate", brakePipe.EmergencyRate)
			);
		}

		private static XElement WriteStraightAirPipeNode(StraightAirPipe straightAirPipe)
		{
			return new XElement("StraightAirPipe",
				new XElement("ServiceRate", straightAirPipe.ServiceRate),
				new XElement("EmergencyRate", straightAirPipe.EmergencyRate),
				new XElement("ReleaseRate", straightAirPipe.ReleaseRate)
			);
		}

		private static XElement WriteBrakeCylinderNode(BrakeCylinder brakeCylinder)
		{
			return new XElement("BrakeCylinder",
				new XElement("ServiceMaximumPressure", brakeCylinder.ServiceMaximumPressure),
				new XElement("EmergencyMaximumPressure", brakeCylinder.EmergencyMaximumPressure),
				new XElement("EmergencyRate", brakeCylinder.EmergencyRate),
				new XElement("ReleaseRate", brakeCylinder.ReleaseRate)
			);
		}

		private static XElement WriteDoorNode(string nodeName, Car.Door door)
		{
			return new XElement(nodeName,
				new XElement("Width", door.Width),
				new XElement("MaxTolerance", door.MaxTolerance)
			);
		}

		private static XElement WriteAccelerationNode(Acceleration acceleration)
		{
			return new XElement("Acceleration",
				acceleration.Entries.Select(entry => new XElement("Entry",
					new XElement("a0", entry.A0),
					new XElement("a1", entry.A1),
					new XElement("v1", entry.V1),
					new XElement("v2", entry.V2),
					new XElement("e", entry.E)
				))
			);
		}

		private static XElement WriteMotorNode(Motor motor)
		{
			return new XElement("Motor", motor.Tracks.Select(WriteTrackNode));
		}

		private static XElement WriteTrackNode(Motor.Track track)
		{
			return new XElement("Track",
				new XElement("Type", track.Type),
				WriteVertexLineNode("Pitch", track.PitchVertices, track.PitchLines),
				WriteVertexLineNode("Volume", track.VolumeVertices, track.VolumeLines),
				WriteAreaNode(track.SoundIndices)
			);
		}

		private static XElement WriteVertexLineNode(string nodeName, Motor.VertexLibrary vertices, ICollection<Motor.Line> lines)
		{
			return new XElement(nodeName,
				new XElement("Vertices",
					vertices.Select(vertex => new XElement("Vertex",
						new XElement("Id", vertex.Key),
						new XElement("Position", $"{vertex.Value.X}, {vertex.Value.Y}")
					))
				),
				new XElement("Lines",
					lines.Select(line => new XElement("Line",
						new XElement("LeftID", line.LeftID),
						new XElement("RightID", line.RightID)
					))
				)
			);
		}

		private static XElement WriteAreaNode(IEnumerable<Motor.Area> areas)
		{
			return new XElement("SoundIndex",
				new XElement("Areas",
					areas.Select(area => new XElement("Area",
						new XElement("Index", area.Index),
						new XElement("LeftX", area.LeftX),
						new XElement("RightX", area.RightX)
					))
				)
			);
		}

		private static XElement WriteCabNode(Cab cab)
		{
			XElement cabNode = new XElement("Cab");

			EmbeddedCab embeddedCab = cab as EmbeddedCab;

			cabNode.Add(
				new XElement("IsEmbeddedCab", embeddedCab != null),
				new XElement("Position", $"{cab.PositionX}, {cab.PositionY}, {cab.PositionZ}")
			);

			if (embeddedCab != null)
			{
				cabNode.Add(WritePanelNode(embeddedCab.Panel));
			}

			ExternalCab externalCab = cab as ExternalCab;

			if (externalCab != null)
			{
				cabNode.Add(
					WriteCameraRestrictionNode(externalCab.CameraRestriction),
					new XElement("Panel", externalCab.FileName)
				);
			}

			return cabNode;
		}

		private static XElement WritePanelNode(Panel panel)
		{
			return new XElement("Panel",
				WriteThisNode(panel.This),
				new XElement("Screens", panel.Screens.Select(WriteScreenNode)),
				new XElement("PanelElements", panel.PanelElements.Select(WritePanelElementNode))
			);
		}

		private static XElement WriteThisNode(This This)
		{
			return new XElement("This",
				new XElement("Resolution", This.Resolution),
				new XElement("Left", This.Left),
				new XElement("Right", This.Right),
				new XElement("Top", This.Top),
				new XElement("Bottom", This.Bottom),
				new XElement("DaytimeImage", This.DaytimeImage),
				new XElement("NighttimeImage", This.NighttimeImage),
				new XElement("TransparentColor", This.TransparentColor),
				new XElement("Center", $"{This.CenterX}, {This.CenterY}"),
				new XElement("Origin", $"{This.OriginX}, {This.OriginY}")
			);
		}

		private static XElement WriteScreenNode(Screen screen)
		{
			return new XElement("Screen",
				new XElement("Number", screen.Number),
				new XElement("Layer", screen.Layer),
				new XElement("PanelElements", screen.PanelElements.Select(WritePanelElementNode)),
				new XElement("TouchElements", screen.TouchElements.Select(WriteTouchElementNode))
			);
		}

		private static XElement WritePanelElementNode(PanelElement element)
		{
			Models.Panels.PilotLampElement pilotLamp = element as Models.Panels.PilotLampElement;

			if (pilotLamp != null)
			{
				return WritePilotLampElementNode(pilotLamp);
			}

			NeedleElement needle = element as NeedleElement;

			if (needle != null)
			{
				return WriteNeedleElementNode(needle);
			}

			DigitalNumberElement digitalNumber = element as DigitalNumberElement;

			if (digitalNumber != null)
			{
				return WriteDigitalNumberElementNode(digitalNumber);
			}

			DigitalGaugeElement digitalGauge = element as DigitalGaugeElement;

			if (digitalGauge != null)
			{
				return WriteDigitalGaugeElementNode(digitalGauge);
			}

			LinearGaugeElement linearGauge = element as LinearGaugeElement;

			if (linearGauge != null)
			{
				return WriteLinearGaugeElementNode(linearGauge);
			}

			TimetableElement timetable = element as TimetableElement;

			if (timetable != null)
			{
				return WriteTimetableElementNode(timetable);
			}

			throw new ArgumentException();
		}

		private static XElement WritePilotLampElementNode(Models.Panels.PilotLampElement element)
		{
			return new XElement("PilotLamp",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor)
			);
		}

		private static XElement WriteNeedleElementNode(NeedleElement element)
		{
			return new XElement("Needle",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("DefinedRadius", element.DefinedRadius),
				new XElement("Radius", element.Radius),
				new XElement("Color", element.Color),
				new XElement("DefinedOrigin", element.DefinedOrigin),
				new XElement("Origin", $"{element.OriginX}, {element.OriginY}"),
				new XElement("InitialAngle", element.InitialAngle),
				new XElement("LastAngle", element.LastAngle),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("DefinedNaturalFreq", element.DefinedNaturalFreq),
				new XElement("NaturalFreq", element.NaturalFreq),
				new XElement("DefinedDampingRatio", element.DefinedDampingRatio),
				new XElement("DampingRatio", element.DampingRatio),
				new XElement("Backstop", element.Backstop),
				new XElement("Smoothed", element.Smoothed)
			);
		}

		private static XElement WriteDigitalNumberElementNode(DigitalNumberElement element)
		{
			return new XElement("DigitalNumber",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Interval", element.Interval)
			);
		}

		private static XElement WriteDigitalGaugeElementNode(DigitalGaugeElement element)
		{
			return new XElement("DigitalGauge",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("Radius", element.Radius),
				new XElement("Color", element.Color),
				new XElement("InitialAngle", element.InitialAngle),
				new XElement("LastAngle", element.LastAngle),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("Step", element.Step)
			);
		}

		private static XElement WriteLinearGaugeElementNode(LinearGaugeElement element)
		{
			return new XElement("LinearGauge",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("Direction", $"{element.DirectionX}, {element.DirectionY}"),
				new XElement("Width", element.Width)
			);
		}

		private static XElement WriteTimetableElementNode(TimetableElement element)
		{
			return new XElement("Timetable",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Width", element.Width),
				new XElement("Height", element.Height),
				new XElement("TransparentColor", element.TransparentColor)
			);
		}

		private static XElement WriteSubjectNode(Subject subject)
		{
			return new XElement("Subject",
				new XElement("Base", subject.Base),
				new XElement("BaseOption", subject.BaseOption),
				new XElement("Suffix", subject.Suffix),
				new XElement("SuffixOption", subject.SuffixOption)
			);
		}

		private static XElement WriteTouchElementNode(Models.Panels.TouchElement element)
		{
			return new XElement("Touch",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Size", $"{element.SizeX}, {element.SizeY}"),
				new XElement("JumpScreen", element.JumpScreen),
				new XElement("SoundEntries", element.SoundEntries.Select(WriteTouchElementSoundEntryNode)),
				new XElement("CommandEntries", element.CommandEntries.Select(WriteTouchElementCommandEntryNode)),
				new XElement("Layer", element.Layer)
			);
		}

		private static XElement WriteTouchElementSoundEntryNode(Models.Panels.TouchElement.SoundEntry entry)
		{
			return new XElement("Entry",
				new XElement("Index", entry.Index)
			);
		}

		private static XElement WriteTouchElementCommandEntryNode(Models.Panels.TouchElement.CommandEntry entry)
		{
			return new XElement("Entry",
				new XElement("Info", entry.Info.Command),
				new XElement("Option", entry.Option)
			);
		}

		private static XElement WriteCameraRestrictionNode(CameraRestriction cameraRestriction)
		{
			return new XElement("CameraRestriction",
				new XElement("DefinedForwards", cameraRestriction.DefinedForwards),
				new XElement("DefinedBackwards", cameraRestriction.DefinedBackwards),
				new XElement("DefinedLeft", cameraRestriction.DefinedLeft),
				new XElement("DefinedRight", cameraRestriction.DefinedRight),
				new XElement("DefinedUp", cameraRestriction.DefinedUp),
				new XElement("DefinedDown", cameraRestriction.DefinedDown),
				new XElement("Forwards", cameraRestriction.Forwards),
				new XElement("Backwards", cameraRestriction.Backwards),
				new XElement("Left", cameraRestriction.Left),
				new XElement("Right", cameraRestriction.Right),
				new XElement("Up", cameraRestriction.Up),
				new XElement("Down", cameraRestriction.Down)
			);
		}

		private static XElement WriteCouplerNode(Coupler coupler)
		{
			return new XElement("Coupler",
				new XElement("Min", coupler.Min),
				new XElement("Max", coupler.Max),
				new XElement("Object", coupler.Object)
			);
		}

		private static XElement WriteSoundsNode(Sound sound)
		{
			return new XElement("Sounds",
				WriteArraySoundNode("Run", sound.SoundElements.OfType<RunElement>()),
				WriteArraySoundNode("Flange", sound.SoundElements.OfType<FlangeElement>()),
				WriteArraySoundNode("Motor", sound.SoundElements.OfType<MotorElement>()),
				WriteArraySoundNode("FrontSwitch", sound.SoundElements.OfType<FrontSwitchElement>()),
				WriteArraySoundNode("RearSwitch", sound.SoundElements.OfType<RearSwitchElement>()),
				WriteArraySoundNode("Brake", sound.SoundElements.OfType<BrakeElement>()),
				WriteArraySoundNode("Compressor", sound.SoundElements.OfType<CompressorElement>()),
				WriteArraySoundNode("Suspension", sound.SoundElements.OfType<SuspensionElement>()),
				WriteArraySoundNode("PrimaryHorn", sound.SoundElements.OfType<PrimaryHornElement>()),
				WriteArraySoundNode("SecondaryHorn", sound.SoundElements.OfType<SecondaryHornElement>()),
				WriteArraySoundNode("MusicHorn", sound.SoundElements.OfType<MusicHornElement>()),
				WriteArraySoundNode("Door", sound.SoundElements.OfType<DoorElement>()),
				WriteArraySoundNode("Ats", sound.SoundElements.OfType<AtsElement>()),
				WriteArraySoundNode("Buzzer", sound.SoundElements.OfType<BuzzerElement>()),
				WriteArraySoundNode("PilotLamp", sound.SoundElements.OfType<Models.Sounds.PilotLampElement>()),
				WriteArraySoundNode("BrakeHandle", sound.SoundElements.OfType<BrakeHandleElement>()),
				WriteArraySoundNode("MasterController", sound.SoundElements.OfType<MasterControllerElement>()),
				WriteArraySoundNode("Reverser", sound.SoundElements.OfType<ReverserElement>()),
				WriteArraySoundNode("Breaker", sound.SoundElements.OfType<BreakerElement>()),
				WriteArraySoundNode("RequestStop", sound.SoundElements.OfType<RequestStopElement>()),
				WriteArraySoundNode("Touch", sound.SoundElements.OfType<Models.Sounds.TouchElement>()),
				WriteArraySoundNode("Others", sound.SoundElements.OfType<OthersElement>())
			);
		}

		private static XElement WriteSoundNode(string nodeName, SoundElement element)
		{
			return new XElement(nodeName,
				new XElement("Key", element.Key),
				new XElement("FilePath", element.FilePath),
				new XElement("DefinedPosition", element.DefinedPosition),
				new XElement("Position", $"{element.PositionX}, {element.PositionY}, {element.PositionZ}"),
				new XElement("DefinedRadius", element.DefinedRadius),
				new XElement("Radius", element.Radius)
			);
		}

		private static XElement WriteArraySoundNode(string nodeName, IEnumerable<SoundElement> elements)
		{
			return new XElement(nodeName, elements.Select(x => WriteSoundNode("Sound", x)));
		}
	}
}
