﻿using OpenBveApi.Math;
using OpenBve.BrakeSystems;

namespace OpenBve
{
	class BVE2SoundParser
	{
		/// <summary>Loads the sound set for a BVE2 based train</summary>
		/// <param name="train">The train</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		internal static void Parse(string trainFolder, TrainManager.Train train)
		{
			// set sound positions and radii
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			Vector3 center = new Vector3(0.0, 0.0, 0.0);
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 cab = new Vector3(-train.Cars[train.DriverCar].DriverPosition.X, train.Cars[train.DriverCar].DriverPosition.Y, train.Cars[train.DriverCar].DriverPosition.Z - 0.5);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].DriverPosition.X, train.Cars[train.DriverCar].DriverPosition.Y, train.Cars[train.DriverCar].DriverPosition.Z + 1.0);
			train.InitializeCarSounds();
			// load sounds for driver's car
			train.Cars[train.DriverCar].Sounds.Adjust = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Adjust.wav"), panel, SoundCfgParser.tinyRadius);
			train.Cars[train.DriverCar].Sounds.Brake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Brake.wav"), center, SoundCfgParser.smallRadius);
			train.Cars[train.DriverCar].Sounds.Halt = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Halt.wav"), cab, SoundCfgParser.tinyRadius);
			train.Cars[train.DriverCar].Horns[0].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon0.wav"), SoundCfgParser.smallRadius);
			train.Cars[train.DriverCar].Horns[0].Loop = false;
			train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
			if (train.Cars[train.DriverCar].Horns[0].LoopSound == null)
			{
				train.Cars[train.DriverCar].Horns[0].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon.wav"), SoundCfgParser.largeRadius);
			}
			train.Cars[train.DriverCar].Horns[1].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon1.wav"), SoundCfgParser.largeRadius);
			train.Cars[train.DriverCar].Horns[1].Loop = false;
			train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
			train.Cars[train.DriverCar].Horns[2].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon2.wav"), SoundCfgParser.mediumRadius);
			train.Cars[train.DriverCar].Horns[2].Loop = true;
			train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
			train.Cars[train.DriverCar].Sounds.PilotLampOn = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Leave.wav"), cab, SoundCfgParser.tinyRadius);
			train.Cars[train.DriverCar].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
			// load sounds for all cars
			for (int i = 0; i < train.Cars.Length; i++)
			{
				Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[i].FrontAxle.Position);
				Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[i].RearAxle.Position);
				train.Cars[i].AirBrake.AirSoundPosition = center;
				train.Cars[i].AirBrake.AirNormal = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Air.wav"), SoundCfgParser.smallRadius);
				train.Cars[i].AirBrake.AirHigh = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "AirHigh.wav"), SoundCfgParser.smallRadius);
				train.Cars[i].AirBrake.AirZero = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "AirZero.wav"), SoundCfgParser.smallRadius);
				if (train.Cars[i].AirBrake.Type == AirBrake.BrakeType.Main)
				{
					train.Cars[i].AirBrake.Compressor.EndSound = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpEnd.wav"), SoundCfgParser.mediumRadius);
					train.Cars[i].AirBrake.Compressor.LoopSound = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpLoop.wav"), SoundCfgParser.mediumRadius);
					train.Cars[i].AirBrake.Compressor.StartSound = SoundCfgParser.TryLoadSoundBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpStart.wav"), SoundCfgParser.mediumRadius);
					train.Cars[i].AirBrake.Compressor.SoundPosition = center;
				}
				train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumed = false;
				train.Cars[i].Sounds.DoorCloseL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsL.wav"), left, SoundCfgParser.smallRadius);
				train.Cars[i].Sounds.DoorCloseR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsR.wav"), right, SoundCfgParser.smallRadius);
				if (train.Cars[i].Sounds.DoorCloseL.Buffer == null)
				{
					train.Cars[i].Sounds.DoorCloseL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), left, SoundCfgParser.smallRadius);
				}
				if (train.Cars[i].Sounds.DoorCloseR.Buffer == null)
				{
					train.Cars[i].Sounds.DoorCloseR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), right, SoundCfgParser.smallRadius);
				}
				train.Cars[i].Sounds.DoorOpenL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnL.wav"), left, SoundCfgParser.smallRadius);
				train.Cars[i].Sounds.DoorOpenR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnR.wav"), right, SoundCfgParser.smallRadius);
				if (train.Cars[i].Sounds.DoorOpenL.Buffer == null)
				{
					train.Cars[i].Sounds.DoorOpenL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), left, SoundCfgParser.smallRadius);
				}
				if (train.Cars[i].Sounds.DoorOpenR.Buffer == null)
				{
					train.Cars[i].Sounds.DoorOpenR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), right, SoundCfgParser.smallRadius);
				}
				train.Cars[i].Sounds.EmrBrake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "EmrBrake.wav"), center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.Flange = SoundCfgParser.TryLoadSoundArray(trainFolder, "Flange", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
				train.Cars[i].Sounds.Loop = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Loop.wav"), center, SoundCfgParser.mediumRadius);
				train.Cars[i].FrontAxle.PointSounds = new TrainManager.CarSound[]
				{
					new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), frontaxle, SoundCfgParser.smallRadius)
				};
				train.Cars[i].RearAxle.PointSounds = new TrainManager.CarSound[]
				{
					new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), rearaxle, SoundCfgParser.smallRadius)
				};
				train.Cars[i].Sounds.Rub = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Rub.wav"), center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.Run = SoundCfgParser.TryLoadSoundArray(trainFolder, "Run", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.SpringL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "SpringL.wav"), left, SoundCfgParser.smallRadius);
				train.Cars[i].Sounds.SpringR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "SpringR.wav"), right, SoundCfgParser.smallRadius);
				// motor sound
				if (train.Cars[i].Specs.IsMotorCar)
				{
					System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
					train.Cars[i].Sounds.Motor.Position = center;
					for (int j = 0; j < train.Cars[i].Sounds.Motor.Tables.Length; j++)
					{
						for (int k = 0; k < train.Cars[i].Sounds.Motor.Tables[j].Entries.Length; k++)
						{
							int idx = train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundIndex;
							if (idx >= 0)
							{
								TrainManager.CarSound snd = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Motor" + idx.ToString(Culture) + ".wav"), center, SoundCfgParser.mediumRadius);
								train.Cars[i].Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
							}
						}
					}
				}
			}
		}
	}
}
