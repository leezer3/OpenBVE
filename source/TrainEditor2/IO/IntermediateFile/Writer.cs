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
				new XElement("InitialDriverCar", train.InitialDriverCar.ToString(culture)),
				new XElement("Cars", train.Cars.Select(WriteCarNode)),
				new XElement("Couplers", train.Couplers.Select(WriteCouplerNode))
			);
		}

		private static XElement WriteHandleNode(Handle handle)
		{
			return new XElement("Handle",
				new XElement("HandleType", handle.HandleType),
				new XElement("PowerNotches", handle.PowerNotches.ToString(culture)),
				new XElement("BrakeNotches", handle.BrakeNotches.ToString(culture)),
				new XElement("PowerNotchReduceSteps", handle.PowerNotchReduceSteps.ToString(culture)),
				new XElement("EbHandleBehaviour", handle.HandleBehaviour),
				new XElement("LocoBrakeType", handle.LocoBrake),
				new XElement("LocoBrakeNotches", handle.LocoBrakeNotches.ToString(culture)),
				new XElement("DriverPowerNotches", handle.DriverPowerNotches.ToString(culture)),
				new XElement("DriverBrakeNotches", handle.DriverBrakeNotches.ToString(culture))
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
				new XElement("LoadCompensatingDevice", device.LoadCompensatingDevice.ToString(culture)),
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
				car.Mass.ToXElement("Mass"),
				car.Length.ToXElement("Length"),
				car.Width.ToXElement("Width"),
				car.Height.ToXElement("Height"),
				car.CenterOfGravityHeight.ToXElement("CenterOfGravityHeight"),
				new XElement("DefinedAxles", car.DefinedAxles),
				car.FrontAxle.ToXElement("FrontAxle"),
				car.RearAxle.ToXElement("RearAxle"),
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
				bogie.FrontAxle.ToXElement("FrontAxle"),
				bogie.RearAxle.ToXElement("RearAxle"),
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", bogie.Object)
			);
		}

		private static XElement WritePerformanceNode(Performance performance)
		{
			return new XElement("Performance",
				new XElement("Deceleration", performance.Deceleration),
				new XElement("CoefficientOfStaticFriction", performance.CoefficientOfStaticFriction.ToString(culture)),
				new XElement("CoefficientOfRollingResistance", performance.CoefficientOfRollingResistance.ToString(culture)),
				new XElement("AerodynamicDragCoefficient", performance.AerodynamicDragCoefficient.ToString(culture))
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
				door.MaxTolerance.ToXElement("MaxTolerance")
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
					new XElement("e", entry.E.ToString(culture))
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
						new XElement("Id", vertex.Key.ToString(culture)),
						new XElement("Position", $"{vertex.Value.X.ToString(culture)}, {vertex.Value.Y.ToString(culture)}")
					))
				),
				new XElement("Lines",
					lines.Select(line => new XElement("Line",
						new XElement("LeftID", line.LeftID.ToString(culture)),
						new XElement("RightID", line.RightID.ToString(culture))
					))
				)
			);
		}

		private static XElement WriteAreaNode(IEnumerable<Motor.Area> areas)
		{
			return new XElement("SoundIndex",
				new XElement("Areas",
					areas.Select(area => new XElement("Area",
						new XElement("Index", area.Index.ToString(culture)),
						new XElement("LeftX", area.LeftX.ToString(culture)),
						new XElement("RightX", area.RightX.ToString(culture))
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
				new XElement("Position",
					new XAttribute("Unit", $"{cab.PositionX.UnitValue}, {cab.PositionY.UnitValue}, {cab.PositionZ.UnitValue}"),
					$"{cab.PositionX.Value.ToString(culture)}, {cab.PositionY.Value.ToString(culture)}, {cab.PositionZ.Value.ToString(culture)}"
				)
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
				new XElement("Resolution", This.Resolution.ToString(culture)),
				new XElement("Left", This.Left.ToString(culture)),
				new XElement("Right", This.Right.ToString(culture)),
				new XElement("Top", This.Top.ToString(culture)),
				new XElement("Bottom", This.Bottom.ToString(culture)),
				new XElement("DaytimeImage", This.DaytimeImage),
				new XElement("NighttimeImage", This.NighttimeImage),
				new XElement("TransparentColor", This.TransparentColor),
				new XElement("Center", $"{This.CenterX.ToString(culture)}, {This.CenterY.ToString(culture)}"),
				new XElement("Origin", $"{This.OriginX.ToString(culture)}, {This.OriginY.ToString(culture)}")
			);
		}

		private static XElement WriteScreenNode(Screen screen)
		{
			return new XElement("Screen",
				new XElement("Number", screen.Number.ToString(culture)),
				new XElement("Layer", screen.Layer.ToString(culture)),
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
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor)
			);
		}

		private static XElement WriteNeedleElementNode(NeedleElement element)
		{
			return new XElement("Needle",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("DefinedRadius", element.DefinedRadius),
				new XElement("Radius", element.Radius.ToString(culture)),
				new XElement("Color", element.Color),
				new XElement("DefinedOrigin", element.DefinedOrigin),
				new XElement("Origin", $"{element.OriginX.ToString(culture)}, {element.OriginY.ToString(culture)}"),
				new XElement("InitialAngle", element.InitialAngle.ToString(culture)),
				new XElement("LastAngle", element.LastAngle.ToString(culture)),
				new XElement("Minimum", element.Minimum.ToString(culture)),
				new XElement("Maximum", element.Maximum.ToString(culture)),
				new XElement("DefinedNaturalFreq", element.DefinedNaturalFreq),
				new XElement("NaturalFreq", element.NaturalFreq.ToString(culture)),
				new XElement("DefinedDampingRatio", element.DefinedDampingRatio),
				new XElement("DampingRatio", element.DampingRatio.ToString(culture)),
				new XElement("Backstop", element.Backstop),
				new XElement("Smoothed", element.Smoothed)
			);
		}

		private static XElement WriteDigitalNumberElementNode(DigitalNumberElement element)
		{
			return new XElement("DigitalNumber",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Interval", element.Interval.ToString(culture))
			);
		}

		private static XElement WriteDigitalGaugeElementNode(DigitalGaugeElement element)
		{
			return new XElement("DigitalGauge",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				WriteSubjectNode(element.Subject),
				new XElement("Radius", element.Radius.ToString(culture)),
				new XElement("Color", element.Color),
				new XElement("InitialAngle", element.InitialAngle.ToString(culture)),
				new XElement("LastAngle", element.LastAngle.ToString(culture)),
				new XElement("Minimum", element.Minimum.ToString(culture)),
				new XElement("Maximum", element.Maximum.ToString(culture)),
				new XElement("Step", element.Step.ToString(culture))
			);
		}

		private static XElement WriteLinearGaugeElementNode(LinearGaugeElement element)
		{
			return new XElement("LinearGauge",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Minimum", element.Minimum.ToString(culture)),
				new XElement("Maximum", element.Maximum.ToString(culture)),
				new XElement("Direction", $"{element.DirectionX.ToString(culture)}, {element.DirectionY.ToString(culture)}"),
				new XElement("Width", element.Width.ToString(culture))
			);
		}

		private static XElement WriteTimetableElementNode(TimetableElement element)
		{
			return new XElement("Timetable",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Layer", element.Layer.ToString(culture)),
				new XElement("Width", element.Width.ToString(culture)),
				new XElement("Height", element.Height.ToString(culture)),
				new XElement("TransparentColor", element.TransparentColor)
			);
		}

		private static XElement WriteSubjectNode(Subject subject)
		{
			return new XElement("Subject",
				new XElement("Base", subject.Base),
				new XElement("BaseOption", subject.BaseOption.ToString(culture)),
				new XElement("Suffix", subject.Suffix),
				new XElement("SuffixOption", subject.SuffixOption.ToString(culture))
			);
		}

		private static XElement WriteTouchElementNode(Models.Panels.TouchElement element)
		{
			return new XElement("Touch",
				new XElement("Location", $"{element.LocationX.ToString(culture)}, {element.LocationY.ToString(culture)}"),
				new XElement("Size", $"{element.SizeX.ToString(culture)}, {element.SizeY.ToString(culture)}"),
				new XElement("JumpScreen", element.JumpScreen.ToString(culture)),
				new XElement("SoundEntries", element.SoundEntries.Select(WriteTouchElementSoundEntryNode)),
				new XElement("CommandEntries", element.CommandEntries.Select(WriteTouchElementCommandEntryNode)),
				new XElement("Layer", element.Layer.ToString(culture))
			);
		}

		private static XElement WriteTouchElementSoundEntryNode(Models.Panels.TouchElement.SoundEntry entry)
		{
			return new XElement("Entry",
				new XElement("Index", entry.Index.ToString(culture))
			);
		}

		private static XElement WriteTouchElementCommandEntryNode(Models.Panels.TouchElement.CommandEntry entry)
		{
			return new XElement("Entry",
				new XElement("Info", entry.Info.Command),
				new XElement("Option", entry.Option.ToString(culture))
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
				cameraRestriction.Forwards.ToXElement("Forwards"),
				cameraRestriction.Backwards.ToXElement("Backwards"),
				cameraRestriction.Left.ToXElement("Left"),
				cameraRestriction.Right.ToXElement("Right"),
				cameraRestriction.Up.ToXElement("Up"),
				cameraRestriction.Down.ToXElement("Down")
			);
		}

		private static XElement WriteCouplerNode(Coupler coupler)
		{
			return new XElement("Coupler",
				coupler.Min.ToXElement("Min"),
				coupler.Max.ToXElement("Max"),
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
