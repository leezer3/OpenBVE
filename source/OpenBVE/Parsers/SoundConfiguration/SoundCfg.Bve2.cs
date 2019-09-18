using OpenBve.BrakeSystems;
using OpenBveApi.Math;
using SoundManager;

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
			Vector3 center = Vector3.Zero;
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 cab = new Vector3(-train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z - 0.5);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);
			// load sounds for driver's car
			train.SafetySystems.StationAdjust.AdjustAlarm = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Adjust.wav"), SoundCfgParser.tinyRadius), panel);
			train.Cars[train.DriverCar].Sounds.Brake = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Brake.wav"), SoundCfgParser.smallRadius), center);
			train.SafetySystems.PassAlarm.Sound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Halt.wav"), SoundCfgParser.tinyRadius), cab);
			train.Cars[train.DriverCar].Horns[0].LoopSound = Program.Sounds.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon0.wav"), SoundCfgParser.smallRadius);
			train.Cars[train.DriverCar].Horns[0].Loop = false;
			train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
			if (train.Cars[train.DriverCar].Horns[0].LoopSound == null)
			{
				train.Cars[train.DriverCar].Horns[0].LoopSound = Program.Sounds.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon.wav"), SoundCfgParser.largeRadius);
			}
			train.Cars[train.DriverCar].Horns[1].LoopSound = Program.Sounds.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon1.wav"), SoundCfgParser.largeRadius);
			train.Cars[train.DriverCar].Horns[1].Loop = false;
			train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
			train.Cars[train.DriverCar].Horns[2].LoopSound = Program.Sounds.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon2.wav"), SoundCfgParser.mediumRadius);
			train.Cars[train.DriverCar].Horns[2].Loop = true;
			train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
			train.SafetySystems.PilotLamp.OnSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Leave.wav"), SoundCfgParser.tinyRadius), cab);
			// load sounds for all cars
			for (int i = 0; i < train.Cars.Length; i++)
			{
				Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[i].FrontAxle.Position);
				Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[i].RearAxle.Position);
				train.Cars[i].CarBrake.Air = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Air.wav"), SoundCfgParser.smallRadius), center);
				train.Cars[i].CarBrake.AirHigh = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "AirHigh.wav"), SoundCfgParser.smallRadius), center);
				train.Cars[i].CarBrake.AirZero = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "AirZero.wav"), SoundCfgParser.smallRadius), center);
				if (train.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					train.Cars[i].CarBrake.airCompressor.EndSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpEnd.wav"), SoundCfgParser.mediumRadius), center);
					train.Cars[i].CarBrake.airCompressor.LoopSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpLoop.wav"), SoundCfgParser.mediumRadius), center);
					train.Cars[i].CarBrake.airCompressor.StartSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "CpStart.wav"), SoundCfgParser.mediumRadius), center);
				}
				train.Cars[i].Sounds.BreakerResume = new CarSound();
				train.Cars[i].Sounds.BreakerResumeOrInterrupt = new CarSound();
				train.Cars[i].Sounds.BreakerResumed = false;
				train.Cars[i].Doors[0].CloseSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsL.wav"), SoundCfgParser.smallRadius), left);
				train.Cars[i].Doors[1].CloseSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsR.wav"), SoundCfgParser.smallRadius), right);
				if (train.Cars[i].Doors[0].CloseSound.Buffer == null)
				{
					train.Cars[i].Doors[0].CloseSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), SoundCfgParser.smallRadius), left);
				}
				if (train.Cars[i].Doors[1].CloseSound.Buffer == null)
				{
					train.Cars[i].Doors[1].CloseSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), SoundCfgParser.smallRadius), right);
				}
				train.Cars[i].Doors[0].OpenSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnL.wav"), SoundCfgParser.smallRadius), left);
				train.Cars[i].Doors[1].OpenSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnR.wav"), SoundCfgParser.smallRadius), right);
				if (train.Cars[i].Doors[0].OpenSound.Buffer == null)
				{
					train.Cars[i].Doors[0].OpenSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), SoundCfgParser.smallRadius), left);
				}
				if (train.Cars[i].Doors[1].OpenSound.Buffer == null)
				{
					train.Cars[i].Doors[1].OpenSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), SoundCfgParser.smallRadius), right);
				}
				train.Handles.EmergencyBrake.ApplicationSound = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "EmrBrake.wav"), SoundCfgParser.mediumRadius), center);
				train.Cars[i].Sounds.Flange = SoundCfgParser.TryLoadSoundArray(trainFolder, "Flange", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
				train.Cars[i].Sounds.Loop = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Loop.wav"), SoundCfgParser.mediumRadius), center);
				train.Cars[i].FrontAxle.PointSounds = new CarSound[]
				{
					new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), SoundCfgParser.smallRadius), frontaxle)
				};
				train.Cars[i].RearAxle.PointSounds = new CarSound[]
				{
					new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), SoundCfgParser.smallRadius), rearaxle)
				};
				train.Cars[i].CarBrake.Rub = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Rub.wav"), SoundCfgParser.mediumRadius), center);
				train.Cars[i].Sounds.Run = SoundCfgParser.TryLoadSoundArray(trainFolder, "Run", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.SpringL = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "SpringL.wav"), SoundCfgParser.smallRadius), left);
				train.Cars[i].Sounds.SpringR = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "SpringR.wav"), SoundCfgParser.smallRadius), right);
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
								CarSound snd = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Motor" + idx.ToString(Culture) + ".wav"), SoundCfgParser.mediumRadius), center);
								train.Cars[i].Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
							}
						}
					}
				}
			}
		}
	}
}
