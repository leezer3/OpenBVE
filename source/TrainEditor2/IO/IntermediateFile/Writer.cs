using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.IO.IntermediateFile
{
	internal static partial class IntermediateFile
	{
		internal static void Write(string fileName, Train train, Panel panel, Sound sound)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement trainEditor = new XElement("TrainEditor");
			trainEditor.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			trainEditor.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			xml.Add(trainEditor);

			WriteTrainNode(trainEditor, train);
			WritePanelNode(trainEditor, panel);
			WriteSoundsNode(trainEditor, sound);

			xml.Save(fileName);
		}

		private static void WriteTrainNode(XElement parent, Train train)
		{
			XElement trainNode = new XElement("Train");
			parent.Add(trainNode);

			WriteHandleNode(trainNode, train.Handle);
			WriteCabNode(trainNode, train.Cab);
			WriteDeviceNode(trainNode, train.Device);

			XElement carsNode = new XElement("Cars");
			trainNode.Add(carsNode);

			foreach (Car car in train.Cars)
			{
				WriteCarNode(carsNode, car);
			}

			XElement couplersNode = new XElement("Couplers");
			trainNode.Add(couplersNode);

			foreach (Coupler coupler in train.Couplers)
			{
				WriteCouplerNode(couplersNode, coupler);
			}
		}

		private static void WriteHandleNode(XElement parent, Handle handle)
		{
			parent.Add(new XElement("Handle",
				new XElement("HandleType", handle.HandleType),
				new XElement("PowerNotches", handle.PowerNotches),
				new XElement("BrakeNotches", handle.BrakeNotches),
				new XElement("PowerNotchReduceSteps", handle.PowerNotchReduceSteps),
				new XElement("EbHandleBehaviour", handle.HandleBehaviour),
				new XElement("LocoBrakeType", handle.LocoBrake),
				new XElement("LocoBrakeNotches", handle.LocoBrakeNotches),
				new XElement("DriverPowerNotches", handle.DriverPowerNotches),
				new XElement("DriverBrakeNotches", handle.DriverBrakeNotches)
				));
		}

		private static void WriteCabNode(XElement parent, Cab cab)
		{
			parent.Add(new XElement("Cab",
				new XElement("Position", $"{cab.PositionX}, {cab.PositionY}, {cab.PositionZ}"),
				new XElement("DriverCar", cab.DriverCar)
				));
		}

		private static void WriteDeviceNode(XElement parent, Device device)
		{
			parent.Add(new XElement("Device",
				new XElement("Ats", device.Ats),
				new XElement("Atc", device.Atc),
				new XElement("Eb", device.Eb),
				new XElement("ConstSpeed", device.ConstSpeed),
				new XElement("HoldBrake", device.HoldBrake),
				new XElement("ReAdhesionDevice", device.ReAdhesionDevice),
				new XElement("LoadCompensatingDevice", device.LoadCompensatingDevice),
				new XElement("PassAlarm", device.PassAlarm),
				new XElement("DoorOpenMode", device.DoorOpenMode),
				new XElement("DoorCloseMode", device.DoorCloseMode),
				new XElement("DoorWidth", device.DoorWidth),
				new XElement("DoorMaxTolerance", device.DoorMaxTolerance)
				));
		}

		private static void WriteCarNode(XElement parent, Car car)
		{
			XElement carNode = new XElement("Car");
			parent.Add(carNode);

			carNode.Add(
				new XElement("IsMotorCar", car is MotorCar),
				new XElement("Mass", car.Mass),
				new XElement("Length", car.Length),
				new XElement("Width", car.Width),
				new XElement("Height", car.Height),
				new XElement("CenterOfGravityHeight", car.CenterOfGravityHeight),
				new XElement("DefinedAxles", car.DefinedAxles),
				new XElement("FrontAxle", car.FrontAxle),
				new XElement("RearAxle", car.RearAxle)
				);

			WriteBogieNode(carNode, "FrontBogie", car.FrontBogie);
			WriteBogieNode(carNode, "RearBogie", car.RearBogie);

			carNode.Add(new XElement("ExposedFrontalArea", car.ExposedFrontalArea));
			carNode.Add(new XElement("UnexposedFrontalArea", car.UnexposedFrontalArea));

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
				new XElement("Object", car.Object),
				new XElement("LoadingSway", car.LoadingSway)
				);
		}

		private static void WriteBogieNode(XElement parent, string nodeName, Car.Bogie bogie)
		{
			parent.Add(new XElement(nodeName,
				new XElement("DefinedAxles", bogie.DefinedAxles),
				new XElement("FrontAxle", bogie.FrontAxle),
				new XElement("RearAxle", bogie.RearAxle),
				new XElement("Reversed", bogie.Reversed),
				new XElement("Object", bogie.Object)
				));
		}

		private static void WritePerformanceNode(XElement parent, Performance performance)
		{
			parent.Add(new XElement("Performance",
				new XElement("Deceleration", performance.Deceleration),
				new XElement("CoefficientOfStaticFriction", performance.CoefficientOfStaticFriction),
				new XElement("CoefficientOfRollingResistance", performance.CoefficientOfRollingResistance),
				new XElement("AerodynamicDragCoefficient", performance.AerodynamicDragCoefficient)
				));
		}

		private static void WriteDelayNode(XElement parent, Delay delay)
		{
			parent.Add(new XElement("Delay",
				new XElement("DelayPower", delay.DelayPower.Select(WriteDelayEntryNode)),
				new XElement("DelayBrake", delay.DelayBrake.Select(WriteDelayEntryNode)),
				new XElement("DelayLocoBrake", delay.DelayLocoBrake.Select(WriteDelayEntryNode))
				));
		}

		private static XElement WriteDelayEntryNode(Delay.Entry entry)
		{
			return new XElement(new XElement("Entry",
				new XElement("Up", entry.Up),
				new XElement("Down", entry.Down)
			));
		}

		private static void WriteMoveNode(XElement parent, Move move)
		{
			parent.Add(new XElement("Move",
				new XElement("JerkPowerUp", move.JerkPowerUp),
				new XElement("JerkPowerDown", move.JerkPowerDown),
				new XElement("JerkBrakeUp", move.JerkBrakeUp),
				new XElement("JerkBrakeDown", move.JerkBrakeDown),
				new XElement("BrakeCylinderUp", move.BrakeCylinderUp),
				new XElement("BrakeCylinderDown", move.BrakeCylinderDown)
				));
		}

		private static void WriteBrakeNode(XElement parent, Brake brake)
		{
			parent.Add(new XElement("Brake",
				new XElement("BrakeType", brake.BrakeType),
				new XElement("LocoBrakeType", brake.LocoBrakeType),
				new XElement("BrakeControlSystem", brake.BrakeControlSystem),
				new XElement("BrakeControlSpeed", brake.BrakeControlSpeed)
				));
		}

		private static void WritePressureNode(XElement parent, Pressure pressure)
		{
			parent.Add(new XElement("Pressure",
				new XElement("BrakeCylinderServiceMaximumPressure", pressure.BrakeCylinderServiceMaximumPressure),
				new XElement("BrakeCylinderEmergencyMaximumPressure", pressure.BrakeCylinderEmergencyMaximumPressure),
				new XElement("MainReservoirMinimumPressure", pressure.MainReservoirMinimumPressure),
				new XElement("MainReservoirMaximumPressure", pressure.MainReservoirMaximumPressure),
				new XElement("BrakePipeNormalPressure", pressure.BrakePipeNormalPressure)
				));
		}

		private static void WriteAccelerationNode(XElement parent, Acceleration acceleration)
		{
			parent.Add(new XElement("Acceleration",
				acceleration.Entries.Select(entry => new XElement("Entry",
					new XElement("a0", entry.A0),
					new XElement("a1", entry.A1),
					new XElement("v1", entry.V1),
					new XElement("v2", entry.V2),
					new XElement("e", entry.E)
					))
				));
		}

		private static void WriteMotorNode(XElement parent, Motor motor)
		{
			XElement motorNode = new XElement("Motor");
			parent.Add(motorNode);

			foreach (Motor.Track track in motor.Tracks)
			{
				XElement trackNode = new XElement("Track");
				motorNode.Add(trackNode);

				XElement pitchNode = new XElement("Pitch");
				WriteVertexLineNode(pitchNode, track.PitchVertices, track.PitchLines);
				trackNode.Add(pitchNode);

				XElement volumeNode = new XElement("Volume");
				WriteVertexLineNode(volumeNode, track.VolumeVertices, track.VolumeLines);
				trackNode.Add(volumeNode);

				XElement indexNode = new XElement("SoundIndex");
				WriteAreaNode(indexNode, track.SoundIndices);
				trackNode.Add(indexNode);
			}
		}

		private static void WriteVertexLineNode(XElement parent, Motor.VertexLibrary vertices, ObservableCollection<Motor.Line> lines)
		{
			parent.Add(new XElement("Vertices",
				vertices.Select(vertex => new XElement("Vertex",
					new XElement("Id", vertex.Key),
					new XElement("Position", $"{vertex.Value.X}, {vertex.Value.Y}")
					))
				));

			parent.Add(new XElement("Lines",
				lines.Select(line => new XElement("Line",
					new XElement("LeftID", line.LeftID),
					new XElement("RightID", line.RightID)
					))
				));
		}

		private static void WriteAreaNode(XElement parent, ObservableCollection<Motor.Area> areas)
		{
			parent.Add(new XElement("Areas",
				areas.Select(area => new XElement("Area",
					new XElement("Index", area.Index),
					new XElement("LeftX", area.LeftX),
					new XElement("RightX", area.RightX)
					))
				));
		}

		private static void WriteCouplerNode(XElement parent, Coupler coupler)
		{
			parent.Add(new XElement("Coupler",
				new XElement("Min", coupler.Min),
				new XElement("Max", coupler.Max),
			new XElement("Object", coupler.Object)
				));
		}

		private static void WritePanelNode(XElement parent, Panel panel)
		{
			XElement panelNode = new XElement("Panel");
			parent.Add(panelNode);

			WriteThisNode(panelNode, panel.This);

			XElement screensNode = new XElement("Screens");
			panelNode.Add(screensNode);

			foreach (Screen screen in panel.Screens)
			{
				WriteScreenNode(screensNode, screen);
			}

			XElement panelElementsNode = new XElement("PanelElements");
			panelNode.Add(panelElementsNode);

			foreach (PanelElement element in panel.PanelElements)
			{
				WritePanelElementNode(panelElementsNode, element);
			}
		}

		private static void WriteThisNode(XElement parent, This This)
		{
			parent.Add(new XElement("This",
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
				));
		}

		private static void WriteScreenNode(XElement parent, Screen screen)
		{
			XElement screenNode = new XElement("Screen",
				new XElement("Number", screen.Number),
				new XElement("Layer", screen.Layer)
				);
			parent.Add(screenNode);

			XElement panelElementsNode = new XElement("PanelElements");
			screenNode.Add(panelElementsNode);

			foreach (PanelElement element in screen.PanelElements)
			{
				WritePanelElementNode(panelElementsNode, element);
			}

			XElement touchElementsNode = new XElement("TouchElements");
			screenNode.Add(touchElementsNode);

			foreach (Models.Panels.TouchElement element in screen.TouchElements)
			{
				WriteTouchElementNode(touchElementsNode, element);
			}
		}

		private static void WritePanelElementNode(XElement parent, PanelElement element)
		{
			if (element is Models.Panels.PilotLampElement)
			{
				WritePilotLampElementNode(parent, (Models.Panels.PilotLampElement)element);
			}

			if (element is NeedleElement)
			{
				WriteNeedleElementNode(parent, (NeedleElement)element);
			}

			if (element is DigitalNumberElement)
			{
				WriteDigitalNumberElementNode(parent, (DigitalNumberElement)element);
			}

			if (element is DigitalGaugeElement)
			{
				WriteDigitalGaugeElementNode(parent, (DigitalGaugeElement)element);
			}

			if (element is LinearGaugeElement)
			{
				WriteLinearGaugeElementNode(parent, (LinearGaugeElement)element);
			}

			if (element is TimetableElement)
			{
				WriteTimetableElementNode(parent, (TimetableElement)element);
			}
		}

		private static void WritePilotLampElementNode(XElement parent, Models.Panels.PilotLampElement element)
		{
			parent.Add(new XElement("PilotLamp",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor)
				));
		}

		private static void WriteNeedleElementNode(XElement parent, NeedleElement element)
		{
			parent.Add(new XElement("Needle",
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
				));
		}

		private static void WriteDigitalNumberElementNode(XElement parent, DigitalNumberElement element)
		{
			parent.Add(new XElement("DigitalNumber",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				WriteSubjectNode(element.Subject),
				new XElement("DaytimeImage", element.DaytimeImage),
				new XElement("NighttimeImage", element.NighttimeImage),
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Interval", element.Interval)
				));
		}

		private static void WriteDigitalGaugeElementNode(XElement parent, DigitalGaugeElement element)
		{
			parent.Add(new XElement("DigitalGauge",
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
				));
		}

		private static void WriteLinearGaugeElementNode(XElement parent, LinearGaugeElement element)
		{
			parent.Add(new XElement("LinearGauge",
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
				));
		}

		private static void WriteTimetableElementNode(XElement parent, TimetableElement element)
		{
			parent.Add(new XElement("Timetable",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Width", element.Width),
				new XElement("Height", element.Height),
				new XElement("TransparentColor", element.TransparentColor)
				));
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

		private static void WriteTouchElementNode(XElement parent, Models.Panels.TouchElement element)
		{
			parent.Add(new XElement("Touch",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Size", $"{element.SizeX}, {element.SizeY}"),
				new XElement("JumpScreen", element.JumpScreen),
				new XElement("SoundIndex", element.SoundIndex),
				new XElement("CommandInfo", element.CommandInfo.Command),
				new XElement("CommandOption", element.CommandOption)
				));
		}

		private static void WriteSoundsNode(XElement parent, Sound sound)
		{
			XElement soundsNode = new XElement("Sounds");
			parent.Add(soundsNode);

			WriteArraySoundNode(soundsNode, "Run", sound.SoundElements.OfType<RunElement>());
			WriteArraySoundNode(soundsNode, "Flange", sound.SoundElements.OfType<FlangeElement>());
			WriteArraySoundNode(soundsNode, "Motor", sound.SoundElements.OfType<MotorElement>());
			WriteArraySoundNode(soundsNode, "FrontSwitch", sound.SoundElements.OfType<FrontSwitchElement>());
			WriteArraySoundNode(soundsNode, "RearSwitch", sound.SoundElements.OfType<RearSwitchElement>());
			WriteArraySoundNode(soundsNode, "Brake", sound.SoundElements.OfType<BrakeElement>());
			WriteArraySoundNode(soundsNode, "Compressor", sound.SoundElements.OfType<CompressorElement>());
			WriteArraySoundNode(soundsNode, "Suspension", sound.SoundElements.OfType<SuspensionElement>());
			WriteArraySoundNode(soundsNode, "PrimaryHorn", sound.SoundElements.OfType<PrimaryHornElement>());
			WriteArraySoundNode(soundsNode, "SecondaryHorn", sound.SoundElements.OfType<SecondaryHornElement>());
			WriteArraySoundNode(soundsNode, "MusicHorn", sound.SoundElements.OfType<MusicHornElement>());
			WriteArraySoundNode(soundsNode, "Door", sound.SoundElements.OfType<DoorElement>());
			WriteArraySoundNode(soundsNode, "Ats", sound.SoundElements.OfType<AtsElement>());
			WriteArraySoundNode(soundsNode, "Buzzer", sound.SoundElements.OfType<BuzzerElement>());
			WriteArraySoundNode(soundsNode, "PilotLamp", sound.SoundElements.OfType<Models.Sounds.PilotLampElement>());
			WriteArraySoundNode(soundsNode, "BrakeHandle", sound.SoundElements.OfType<BrakeHandleElement>());
			WriteArraySoundNode(soundsNode, "MasterController", sound.SoundElements.OfType<MasterControllerElement>());
			WriteArraySoundNode(soundsNode, "Reverser", sound.SoundElements.OfType<ReverserElement>());
			WriteArraySoundNode(soundsNode, "Breaker", sound.SoundElements.OfType<BreakerElement>());
			WriteArraySoundNode(soundsNode, "RequestStop", sound.SoundElements.OfType<RequestStopElement>());
			WriteArraySoundNode(soundsNode, "Touch", sound.SoundElements.OfType<Models.Sounds.TouchElement>());
			WriteArraySoundNode(soundsNode, "Others", sound.SoundElements.OfType<OthersElement>());
		}

		private static void WriteSoundNode(XElement parent, string nodeName, SoundElement element)
		{
			parent.Add(new XElement(nodeName,
				new XElement("Key", element.Key),
				new XElement("FilePath", element.FilePath),
				new XElement("DefinedPosition", element.DefinedPosition),
				new XElement("Position", $"{element.PositionX}, {element.PositionY}, {element.PositionZ}"),
				new XElement("DefinedRadius", element.DefinedRadius),
				new XElement("Radius", element.Radius)
				));
		}

		private static void WriteArraySoundNode(XElement parent, string nodeName, IEnumerable<SoundElement> elements)
		{
			XElement newNode = new XElement(nodeName);

			foreach (SoundElement element in elements)
			{
				WriteSoundNode(newNode, "Sound", element);
			}

			if (newNode.HasElements)
			{
				parent.Add(newNode);
			}
		}
	}
}
