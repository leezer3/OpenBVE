﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TrainEditor2.Models.Trains;
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

			builder.AppendLine("OPENBVE" + currentVersion);

			builder.AppendLine("#ACCELERATION");

			for (int i = firstMotorCar.Acceleration.Entries.Count - 1; i >= train.Handle.PowerNotches; i--)
			{
				firstMotorCar.Acceleration.Entries.RemoveAt(i);
			}

			foreach (Acceleration.Entry entry in firstMotorCar.Acceleration.Entries)
			{
				builder.Append(entry.A0.ToString(culture) + ",");
				builder.Append(entry.A1.ToString(culture) + ",");
				builder.Append(entry.V1.ToString(culture) + ",");
				builder.Append(entry.V2.ToString(culture) + ",");
				builder.AppendLine(entry.E.ToString(culture));
			}

			builder.AppendLine("#PERFORMANCE");
			builder.AppendLine($"{firstMotorCar.Performance.Deceleration.ToString(culture),-n}; Deceleration");
			builder.AppendLine($"{firstMotorCar.Performance.CoefficientOfStaticFriction.ToString(culture),-n}; CoefficientOfStaticFriction");
			builder.AppendLine($"{"0",-n}; Reserved (not used)");
			builder.AppendLine($"{firstMotorCar.Performance.CoefficientOfRollingResistance.ToString(culture),-n}; CoefficientOfRollingResistance");
			builder.AppendLine($"{firstMotorCar.Performance.AerodynamicDragCoefficient.ToString(culture),-n}; AerodynamicDragCoefficient");

			builder.AppendLine("#DELAY");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayPower.Select(d => d.Up.ToString(culture))),-n}; DelayPowerUp");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayPower.Select(d => d.Down.ToString(culture))),-n}; DelayPowerDown");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayBrake.Select(d => d.Up.ToString(culture))),-n}; DelayBrakeUp");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayBrake.Select(d => d.Down.ToString(culture))),-n}; DelayBrakeDown");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayElectricBrake.Select(d => d.Up.ToString(culture))),-n}; DelayElectricBrakeUp");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayElectricBrake.Select(d => d.Down.ToString(culture))),-n}; DelayElectricBrakeDown");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayLocoBrake.Select(d => d.Up.ToString(culture))),-n}; DelayLocoBrakeUp (1.5.3.4+)");
			builder.AppendLine($"{string.Join(",", firstMotorCar.Delay.DelayLocoBrake.Select(d => d.Down.ToString(culture))),-n}; DelayLocoBrakeDown (1.5.3.4+)");

			builder.AppendLine("#MOVE");
			builder.AppendLine($"{firstMotorCar.Move.JerkPowerUp.ToString(culture),-n}; JerkPowerUp");
			builder.AppendLine($"{firstMotorCar.Move.JerkPowerDown.ToString(culture),-n}; JerkPowerDown");
			builder.AppendLine($"{firstMotorCar.Move.JerkBrakeUp.ToString(culture),-n}; JerkBrakeUp");
			builder.AppendLine($"{firstMotorCar.Move.JerkBrakeDown.ToString(culture),-n}; JerkBrakeDown");
			builder.AppendLine($"{firstMotorCar.Move.BrakeCylinderUp.ToString(culture),-n}; BrakeCylinderUp");
			builder.AppendLine($"{firstMotorCar.Move.BrakeCylinderDown.ToString(culture),-n}; BrakeCylinderDown");

			builder.AppendLine("#BRAKE");
			builder.AppendLine($"{((int)firstMotorCar.Brake.BrakeType).ToString(culture),-n}; BrakeType");
			builder.AppendLine($"{((int)firstMotorCar.Brake.BrakeControlSystem).ToString(culture),-n}; BrakeControlSystem");
			builder.AppendLine($"{firstMotorCar.Brake.BrakeControlSpeed.ToString(culture),-n}; BrakeControlSpeed");
			builder.AppendLine($"{((int)firstMotorCar.Brake.LocoBrakeType).ToString(culture),-n}; LocoBrakeType (1.5.3.4+)");

			builder.AppendLine("#PRESSURE");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinderServiceMaximumPressure.ToString(culture),-n}; BrakeCylinderServiceMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakeCylinderEmergencyMaximumPressure.ToString(culture),-n}; BrakeCylinderEmergencyMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.MainReservoirMinimumPressure.ToString(culture),-n}; MainReservoirMinimumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.MainReservoirMaximumPressure.ToString(culture),-n}; MainReservoirMaximumPressure");
			builder.AppendLine($"{firstMotorCar.Pressure.BrakePipeNormalPressure.ToString(culture),-n}; BrakePipeNormalPressure");

			builder.AppendLine("#HANDLE");
			builder.AppendLine($"{((int)train.Handle.HandleType).ToString(culture),-n}; HandleType");
			builder.AppendLine($"{train.Handle.PowerNotches.ToString(culture),-n}; PowerNotches");
			builder.AppendLine($"{train.Handle.BrakeNotches.ToString(culture),-n}; BrakeNotches");
			builder.AppendLine($"{train.Handle.PowerNotchReduceSteps.ToString(culture),-n}; PowerNotchReduceSteps");
			builder.AppendLine($"{((int)train.Handle.HandleBehaviour).ToString(culture),-n}; EbHandleBehaviour (1.5.3.3+)");
			builder.AppendLine($"{train.Handle.LocoBrakeNotches.ToString(culture),-n}; LocoBrakeNotches (1.5.3.4+)");
			builder.AppendLine($"{((int)train.Handle.LocoBrake).ToString(culture),-n}; LocoBrakeType (1.5.3.4+)");
			builder.AppendLine($"{train.Handle.DriverPowerNotches.ToString(culture),-n}; DriverPowerNotches (1.5.3.11+)");
			builder.AppendLine($"{train.Handle.DriverBrakeNotches.ToString(culture),-n}; DriverBrakeNotches (1.5.3.11+)");

			builder.AppendLine("#CAB");
			builder.AppendLine($"{train.Cab.PositionX.ToString(culture),-n}; X");
			builder.AppendLine($"{train.Cab.PositionY.ToString(culture),-n}; Y");
			builder.AppendLine($"{train.Cab.PositionZ.ToString(culture),-n}; Z");
			builder.AppendLine($"{train.Cab.DriverCar.ToString(culture),-n}; DriverCar");

			builder.AppendLine("#CAR");
			builder.AppendLine($"{firstMotorCar.Mass.ToString(culture),-n}; MotorCarMass");
			builder.AppendLine($"{train.Cars.Count(c => c is MotorCar).ToString(culture),-n}; NumberOfMotorCars");
			builder.AppendLine($"{(firstTrailerCar ?? new TrailerCar()).Mass.ToString(culture),-n}; TrailerCarMass");
			builder.AppendLine($"{train.Cars.Count(c => c is TrailerCar).ToString(culture),-n}; NumberOfTrailerCars");
			builder.AppendLine($"{firstMotorCar.Length.ToString(culture),-n}; LengthOfACar");
			builder.AppendLine($"{(train.Cars.First() is MotorCar ? "1" : "0"),-n}; FrontCarIsAMotorCar");
			builder.AppendLine($"{firstMotorCar.Width.ToString(culture),-n}; WidthOfACar");
			builder.AppendLine($"{firstMotorCar.Height.ToString(culture),-n}; HeightOfACar");
			builder.AppendLine($"{firstMotorCar.CenterOfGravityHeight.ToString(culture),-n}; CenterOfGravityHeight");
			builder.AppendLine($"{firstMotorCar.ExposedFrontalArea.ToString(culture),-n}; ExposedFrontalArea");
			builder.AppendLine($"{firstMotorCar.UnexposedFrontalArea.ToString(culture),-n}; UnexposedFrontalArea");

			builder.AppendLine("#DEVICE");
			builder.AppendLine($"{((int)train.Device.Ats).ToString(culture),-n}; Ats");
			builder.AppendLine($"{((int)train.Device.Atc).ToString(culture),-n}; Atc");
			builder.AppendLine($"{(train.Device.Eb ? "1" : "0"),-n}; Eb");
			builder.AppendLine($"{(train.Device.ConstSpeed ? "1" : "0"),-n}; ConstSpeed");
			builder.AppendLine($"{(train.Device.HoldBrake ? "1" : "0"),-n}; HoldBrake");
			builder.AppendLine($"{((int)train.Device.ReAdhesionDevice).ToString(culture),-n}; ReAdhesionDevice");
			builder.AppendLine($"{train.Device.LoadCompensatingDevice.ToString(culture),-n}; Reserved (not used)");
			builder.AppendLine($"{((int)train.Device.PassAlarm).ToString(culture),-n}; PassAlarm");
			builder.AppendLine($"{((int)train.Device.DoorOpenMode).ToString(culture),-n}; DoorOpenMode");
			builder.AppendLine($"{((int)train.Device.DoorCloseMode).ToString(culture),-n}; DoorCloseMode");
			builder.AppendLine($"{train.Device.DoorWidth.ToString(culture),-n}; DoorWidth");
			builder.AppendLine($"{train.Device.DoorMaxTolerance.ToString(culture),-n}; DoorMaxTolerance");

			for (int i = 0; i < 4; i++)
			{
				BVEMotorSoundTableEntry[] entries = new BVEMotorSoundTableEntry[0];

				switch (i)
				{
					case 0:
						builder.AppendLine("#MOTOR_P1");
						entries = Motor.Track.TrackToEntries(firstMotorCar.Motor.Tracks[(int)Motor.TrackInfo.Power1]);
						break;
					case 1:
						builder.AppendLine("#MOTOR_P2");
						entries = Motor.Track.TrackToEntries(firstMotorCar.Motor.Tracks[(int)Motor.TrackInfo.Power2]);
						break;
					case 2:
						builder.AppendLine("#MOTOR_B1");
						entries = Motor.Track.TrackToEntries(firstMotorCar.Motor.Tracks[(int)Motor.TrackInfo.Brake1]);
						break;
					case 3:
						builder.AppendLine("#MOTOR_B2");
						entries = Motor.Track.TrackToEntries(firstMotorCar.Motor.Tracks[(int)Motor.TrackInfo.Brake2]);
						break;
				}

				int k;

				for (k = entries.Length - 1; k >= 0; k--)
				{
					if (entries[k].SoundIndex >= 0)
					{
						break;
					}
				}

				k = Math.Min(k + 2, entries.Length);
				Array.Resize(ref entries, k);

				for (int j = 0; j < entries.Length; j++)
				{
					builder.Append(entries[j].SoundIndex.ToString(culture) + ",");
					builder.Append(entries[j].Pitch.ToString(culture) + ",");
					builder.AppendLine(entries[j].Gain.ToString(culture));
				}
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}
	}
}
