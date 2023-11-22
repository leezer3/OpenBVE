using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi.World;
using TrainEditor2.Models.Trains;
using TrainEditor2.Simulation.TrainManager;
using TrainManager.Motor;

namespace TrainEditor2.IO.Trains.TrainDat
{
	internal static partial class TrainDat
	{
		internal static void Write(string fileName, Train train)
		{
			StringBuilder builder = new StringBuilder();
			CultureInfo culture = CultureInfo.InvariantCulture;
			const int n = 15;

			MotorCar firstMotorCar = train.Cars.OfType<MotorCar>().First();
			TrailerCar firstTrailerCar = train.Cars.OfType<TrailerCar>().FirstOrDefault();
			Cab firstCab = new[] { train.Cars.OfType<ControlledMotorCar>().Select(x => x.Cab), train.Cars.OfType<ControlledTrailerCar>().Select(x => x.Cab) }.SelectMany(x => x).First();

			builder.AppendLine("OPENBVE" + currentVersion);

			builder.AppendLine("#ACCELERATION");

			for (int i = firstMotorCar.Acceleration.Entries.Count - 1; i >= train.Handle.PowerNotches; i--)
			{
				firstMotorCar.Acceleration.Entries.RemoveAt(i);
			}

			foreach (Acceleration.Entry entry in firstMotorCar.Acceleration.Entries)
			{
				builder.Append(entry.A0.ToNewUnit(Unit.Acceleration.KilometerPerHourPerSecond).Value.ToString(culture) + ",");
				builder.Append(entry.A1.ToNewUnit(Unit.Acceleration.KilometerPerHourPerSecond).Value.ToString(culture) + ",");
				builder.Append(entry.V1.ToNewUnit(Unit.Velocity.KilometerPerHour).Value.ToString(culture) + ",");
				builder.Append(entry.V2.ToNewUnit(Unit.Velocity.KilometerPerHour).Value.ToString(culture) + ",");
				builder.AppendLine(entry.E.ToString(culture));
			}

			builder.AppendLine("#PERFORMANCE");
			builder.AppendLine($"{firstMotorCar.Performance.Deceleration.ToNewUnit(Unit.Acceleration.KilometerPerHourPerSecond).Value.ToString(culture).PadRight(n, ' ')}; Deceleration");
			builder.AppendLine($"{firstMotorCar.Performance.CoefficientOfStaticFriction.ToString(culture).PadRight(n, ' ')}; CoefficientOfStaticFriction");
			builder.AppendLine($"{"0".PadRight(n, ' ')}; Reserved (not used)");
			builder.AppendLine($"{firstMotorCar.Performance.CoefficientOfRollingResistance.ToString(culture).PadRight(n, ' ')}; CoefficientOfRollingResistance");
			builder.AppendLine($"{firstMotorCar.Performance.AerodynamicDragCoefficient.ToString(culture).PadRight(n, ' ')}; AerodynamicDragCoefficient");

			builder.AppendLine("#DELAY");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.Power.Select(d => d.Up.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayPowerUp");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.Power.Select(d => d.Down.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayPowerDown");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.Brake.Select(d => d.Up.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayBrakeUp");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.Brake.Select(d => d.Down.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayBrakeDown");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.LocoBrake.Select(d => d.Up.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayLocoBrakeUp (1.5.3.4+)");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.LocoBrake.Select(d => d.Down.ToDefaultUnit().Value.ToString(culture))).PadRight(n, ' ')}; DelayLocoBrakeDown (1.5.3.4+)");

			builder.AppendLine("#MOVE");
			builder.AppendLine($"{firstMotorCar.Jerk.Power.Up.ToNewUnit(Unit.Jerk.CentimeterPerSecondCubed).Value.ToString(culture).PadRight(n, ' ')}; JerkPowerUp");
			builder.AppendLine($"{firstMotorCar.Jerk.Power.Down.ToNewUnit(Unit.Jerk.CentimeterPerSecondCubed).Value.ToString(culture).PadRight(n, ' ')}; JerkPowerDown");
			builder.AppendLine($"{firstMotorCar.Jerk.Brake.Up.ToNewUnit(Unit.Jerk.CentimeterPerSecondCubed).Value.ToString(culture).PadRight(n, ' ')}; JerkBrakeUp");
			builder.AppendLine($"{firstMotorCar.Jerk.Brake.Down.ToNewUnit(Unit.Jerk.CentimeterPerSecondCubed).Value.ToString(culture).PadRight(n, ' ')}; JerkBrakeDown");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinder.EmergencyRate.ToNewUnit(Unit.PressureRate.KilopascalPerSecond).Value.ToString(culture).PadRight(n, ' ')}; BrakeCylinderUp");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinder.ReleaseRate.ToNewUnit(Unit.PressureRate.KilopascalPerSecond).Value.ToString(culture).PadRight(n, ' ')}; BrakeCylinderDown");

			builder.AppendLine("#BRAKE");
			builder.AppendLine($"{((int)firstMotorCar.Brake.BrakeType).ToString(culture).PadRight(n, ' ')}; BrakeType");
			builder.AppendLine($"{((int)firstMotorCar.Brake.BrakeControlSystem).ToString(culture).PadRight(n, ' ')}; BrakeControlSystem");
			builder.AppendLine($"{firstMotorCar.Brake.BrakeControlSpeed.ToNewUnit(Unit.Velocity.KilometerPerHour).Value.ToString(culture).PadRight(n, ' ')}; BrakeControlSpeed");
			builder.AppendLine($"{((int)firstMotorCar.Brake.LocoBrakeType).ToString(culture).PadRight(n, ' ')}; LocoBrakeType (1.5.3.4+)");

			builder.AppendLine("#PRESSURE");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinder.ServiceMaximumPressure.ToNewUnit(Unit.Pressure.Kilopascal).Value.ToString(culture).PadRight(n, ' ')}; BrakeCylinderServiceMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinder.EmergencyMaximumPressure.ToNewUnit(Unit.Pressure.Kilopascal).Value.ToString(culture).PadRight(n, ' ')}; BrakeCylinderEmergencyMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.MainReservoir.MinimumPressure.ToNewUnit(Unit.Pressure.Kilopascal).Value.ToString(culture).PadRight(n, ' ')}; MainReservoirMinimumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.MainReservoir.MaximumPressure.ToNewUnit(Unit.Pressure.Kilopascal).Value.ToString(culture).PadRight(n, ' ')}; MainReservoirMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakePipe.NormalPressure.ToNewUnit(Unit.Pressure.Kilopascal).Value.ToString(culture).PadRight(n, ' ')}; BrakePipeNormalPressure");

			builder.AppendLine("#HANDLE");
			builder.AppendLine($"{((int)train.Handle.HandleType).ToString(culture).PadRight(n, ' ')}; HandleType");
			builder.AppendLine($"{train.Handle.PowerNotches.ToString(culture).PadRight(n, ' ')}; PowerNotches");
			builder.AppendLine($"{train.Handle.BrakeNotches.ToString(culture).PadRight(n, ' ')}; BrakeNotches");
			builder.AppendLine($"{train.Handle.PowerNotchReduceSteps.ToString(culture).PadRight(n, ' ')}; PowerNotchReduceSteps");
			builder.AppendLine($"{((int)train.Handle.HandleBehaviour).ToString(culture).PadRight(n, ' ')}; EbHandleBehaviour (1.5.3.3+)");
			builder.AppendLine($"{train.Handle.LocoBrakeNotches.ToString(culture).PadRight(n, ' ')}; LocoBrakeNotches (1.5.3.4+)");
			builder.AppendLine($"{((int)train.Handle.LocoBrake).ToString(culture).PadRight(n, ' ')}; LocoBrakeType (1.5.3.4+)");
			builder.AppendLine($"{train.Handle.DriverPowerNotches.ToString(culture).PadRight(n, ' ')}; DriverPowerNotches (1.5.3.11+)");
			builder.AppendLine($"{train.Handle.DriverBrakeNotches.ToString(culture).PadRight(n, ' ')}; DriverBrakeNotches (1.5.3.11+)");

			builder.AppendLine("#CAB");
			builder.AppendLine($"{firstCab.PositionX.ToNewUnit(UnitOfLength.Millimeter).Value.ToString(culture).PadRight(n, ' ')}; X");
			builder.AppendLine($"{firstCab.PositionY.ToNewUnit(UnitOfLength.Millimeter).Value.ToString(culture).PadRight(n, ' ')}; Y");
			builder.AppendLine($"{firstCab.PositionZ.ToNewUnit(UnitOfLength.Millimeter).Value.ToString(culture).PadRight(n, ' ')}; Z");
			builder.AppendLine($"{train.InitialDriverCar.ToString(culture).PadRight(n, ' ')}; DriverCar");

			builder.AppendLine("#CAR");
			builder.AppendLine($"{firstMotorCar.Mass.ToNewUnit(UnitOfWeight.MetricTonnes).Value.ToString(culture).PadRight(n, ' ')}; MotorCarMass");
			builder.AppendLine($"{train.Cars.Count(c => c is MotorCar).ToString(culture).PadRight(n, ' ')}; NumberOfMotorCars");
			builder.AppendLine($"{(firstTrailerCar ?? new UncontrolledTrailerCar()).Mass.ToNewUnit(UnitOfWeight.MetricTonnes).Value.ToString(culture).PadRight(n, ' ')}; TrailerCarMass");
			builder.AppendLine($"{train.Cars.Count(c => c is TrailerCar).ToString(culture).PadRight(n, ' ')}; NumberOfTrailerCars");
			builder.AppendLine($"{firstMotorCar.Length.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; LengthOfACar");
			builder.AppendLine($"{(train.Cars.First() is MotorCar ? "1" : "0").PadRight(n, ' ')}; FrontCarIsAMotorCar");
			builder.AppendLine($"{firstMotorCar.Width.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; WidthOfACar");
			builder.AppendLine($"{firstMotorCar.Height.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; HeightOfACar");
			builder.AppendLine($"{firstMotorCar.CenterOfGravityHeight.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; CenterOfGravityHeight");
			builder.AppendLine($"{firstMotorCar.ExposedFrontalArea.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; ExposedFrontalArea");
			builder.AppendLine($"{firstMotorCar.UnexposedFrontalArea.ToDefaultUnit().Value.ToString(culture).PadRight(n, ' ')}; UnexposedFrontalArea");

			builder.AppendLine("#DEVICE");
			builder.AppendLine($"{((int)train.Device.Ats).ToString(culture).PadRight(n, ' ')}; Ats");
			builder.AppendLine($"{((int)train.Device.Atc).ToString(culture).PadRight(n, ' ')}; Atc");
			builder.AppendLine($"{(train.Device.Eb ? "1" : "0").PadRight(n, ' ')}; Eb");
			builder.AppendLine($"{(train.Device.ConstSpeed ? "1" : "0").PadRight(n, ' ')}; ConstSpeed");
			builder.AppendLine($"{(train.Device.HoldBrake ? "1" : "0").PadRight(n, ' ')}; HoldBrake");
			builder.AppendLine($"{((int)firstMotorCar.ReAdhesionDevice).ToString(culture).PadRight(n, ' ')}; ReAdhesionDevice");
			builder.AppendLine($"{train.Device.LoadCompensatingDevice.ToString(culture).PadRight(n, ' ')}; Reserved (not used)");
			builder.AppendLine($"{((int)train.Device.PassAlarm).ToString(culture).PadRight(n, ' ')}; PassAlarm");
			builder.AppendLine($"{((int)train.Device.DoorOpenMode).ToString(culture).PadRight(n, ' ')}; DoorOpenMode");
			builder.AppendLine($"{((int)train.Device.DoorCloseMode).ToString(culture).PadRight(n, ' ')}; DoorCloseMode");
			builder.AppendLine($"{firstMotorCar.LeftDoor.Width.ToNewUnit(UnitOfLength.Millimeter).Value.ToString(culture).PadRight(n, ' ')}; DoorWidth");
			builder.AppendLine($"{firstMotorCar.LeftDoor.MaxTolerance.ToNewUnit(UnitOfLength.Millimeter).Value.ToString(culture).PadRight(n, ' ')}; DoorMaxTolerance");

			TrainEditor.MotorSound.Table[] powerTables = firstMotorCar.Motor.Tracks.Where(x => x.Type == Motor.TrackType.Power).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y)).ToArray();
			TrainEditor.MotorSound.Table[] brakeTables = firstMotorCar.Motor.Tracks.Where(x => x.Type == Motor.TrackType.Brake).Select(x => Motor.Track.TrackToMotorSoundTable(x, y => y, y => y)).ToArray();

			for (int i = 0; i < 4; i++)
			{
				TrainEditor.MotorSound.Table table = null;

				switch (i)
				{
					case 0:
						builder.AppendLine("#MOTOR_P1");
						table = powerTables.ElementAtOrDefault(0);
						break;
					case 1:
						builder.AppendLine("#MOTOR_P2");
						table = powerTables.ElementAtOrDefault(1);
						break;
					case 2:
						builder.AppendLine("#MOTOR_B1");
						table = brakeTables.ElementAtOrDefault(0);
						break;
					case 3:
						builder.AppendLine("#MOTOR_B2");
						table = brakeTables.ElementAtOrDefault(1);
						break;
				}

				if (table == null)
				{
					continue;
				}

				float maxSpeed = 0.0f;
				TrainEditor.MotorSound.Vertex<float> lastPitchVertex = table.PitchVertices.LastOrDefault();
				TrainEditor.MotorSound.Vertex<float> lastGainVertex = table.GainVertices.LastOrDefault();
				TrainEditor.MotorSound.Vertex<int> lastBufferVertex = table.BufferVertices.LastOrDefault();

				if (lastPitchVertex != null)
				{
					maxSpeed = Math.Max(lastPitchVertex.X.ToNewUnit(Unit.Velocity.KilometerPerHour).Value, maxSpeed);
				}

				if (lastGainVertex != null)
				{
					maxSpeed = Math.Max(lastGainVertex.X.ToNewUnit(Unit.Velocity.KilometerPerHour).Value, maxSpeed);
				}

				if (lastBufferVertex != null)
				{
					maxSpeed = Math.Max(lastBufferVertex.X.ToNewUnit(Unit.Velocity.KilometerPerHour).Value, maxSpeed);
				}

				for (float j = 0.0f; j < maxSpeed; j += 0.2f)
				{
					BVEMotorSoundTableEntry entry = table.GetEntry(j / 3.6f);

					builder.Append(entry.SoundIndex.ToString(culture) + ",");
					builder.Append(entry.Pitch.ToString(culture) + ",");
					builder.AppendLine(entry.Gain.ToString(culture));
				}
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}
	}
}
