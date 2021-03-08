using OpenBveApi;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class BVE2SoundParser
	{
		internal readonly Plugin Plugin;
		internal BVE2SoundParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Loads the sound set for a BVE2 based train</summary>
		/// <param name="train">The train</param>
		internal void Parse(TrainBase train)
		{
			// set sound positions and radii
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			Vector3 center = Vector3.Zero;
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 cab = new Vector3(-train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z - 0.5);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);
			// load sounds for driver's car
			train.SafetySystems.StationAdjust.AdjustAlarm = new CarSound(Plugin.currentHost, train.TrainFolder, "Adjust.wav", SoundCfgParser.tinyRadius, panel);
			train.Cars[train.DriverCar].CarBrake.Release = new CarSound(Plugin.currentHost, train.TrainFolder, "Brake.wav", SoundCfgParser.smallRadius, center);
			train.SafetySystems.PassAlarm.Sound = new CarSound(Plugin.currentHost, train.TrainFolder, "Halt.wav", SoundCfgParser.tinyRadius, cab);
			Plugin.currentHost.RegisterSound(Path.CombineFile(train.TrainFolder, "Klaxon0.wav"), SoundCfgParser.smallRadius, out var loopSound);
			train.Cars[train.DriverCar].Horns[0].LoopSound = loopSound as SoundBuffer;
			train.Cars[train.DriverCar].Horns[0].Loop = false;
			train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
			if (train.Cars[train.DriverCar].Horns[0].LoopSound == null)
			{
				Plugin.currentHost.RegisterSound(Path.CombineFile(train.TrainFolder, "Klaxon.wav"), SoundCfgParser.smallRadius, out var loopSound1);
				train.Cars[train.DriverCar].Horns[0].LoopSound = loopSound1 as SoundBuffer;
			}
			Plugin.currentHost.RegisterSound(Path.CombineFile(train.TrainFolder, "Klaxon1.wav"), SoundCfgParser.smallRadius, out var loopSound2);
			train.Cars[train.DriverCar].Horns[1].LoopSound = loopSound2 as SoundBuffer;
			train.Cars[train.DriverCar].Horns[1].Loop = false;
			train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
			Plugin.currentHost.RegisterSound(Path.CombineFile(train.TrainFolder, "Klaxon2.wav"), SoundCfgParser.smallRadius, out var loopSound3);
			train.Cars[train.DriverCar].Horns[2].LoopSound = loopSound3 as SoundBuffer;
			train.Cars[train.DriverCar].Horns[2].Loop = true;
			train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
			train.SafetySystems.PilotLamp.OnSound = new CarSound(Plugin.currentHost, train.TrainFolder, "Leave.wav", SoundCfgParser.tinyRadius, cab);
			// load sounds for all cars
			for (int i = 0; i < train.Cars.Length; i++)
			{
				Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[i].FrontAxle.Position);
				Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[i].RearAxle.Position);
				train.Cars[i].CarBrake.Air = new CarSound(Plugin.currentHost, train.TrainFolder, "Air.wav", SoundCfgParser.smallRadius, center);
				train.Cars[i].CarBrake.AirHigh = new CarSound(Plugin.currentHost, train.TrainFolder, "AirHigh.wav", SoundCfgParser.smallRadius, center);
				train.Cars[i].CarBrake.AirZero = new CarSound(Plugin.currentHost, train.TrainFolder, "AirZero.wav", SoundCfgParser.smallRadius, center);
				if (train.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					train.Cars[i].CarBrake.airCompressor.EndSound = new CarSound(Plugin.currentHost, train.TrainFolder, "CpEnd.wav", SoundCfgParser.mediumRadius, center);
					train.Cars[i].CarBrake.airCompressor.LoopSound = new CarSound(Plugin.currentHost, train.TrainFolder, "CpLoop.wav", SoundCfgParser.mediumRadius, center);
					train.Cars[i].CarBrake.airCompressor.StartSound = new CarSound(Plugin.currentHost, train.TrainFolder, "CpStart.wav", SoundCfgParser.mediumRadius, center);
				}
				train.Cars[i].Doors[0].CloseSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorClsL.wav", SoundCfgParser.smallRadius, left);
				train.Cars[i].Doors[1].CloseSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorClsR.wav", SoundCfgParser.smallRadius, right);
				if (train.Cars[i].Doors[0].CloseSound.Buffer == null)
				{
					train.Cars[i].Doors[0].CloseSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorCls.wav", SoundCfgParser.smallRadius, left);
				}
				if (train.Cars[i].Doors[1].CloseSound.Buffer == null)
				{
					train.Cars[i].Doors[1].CloseSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorCls.wav", SoundCfgParser.smallRadius, right);
				}
				train.Cars[i].Doors[0].OpenSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorOpnL.wav", SoundCfgParser.smallRadius, left);
				train.Cars[i].Doors[1].OpenSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorOpnR.wav", SoundCfgParser.smallRadius, right);
				if (train.Cars[i].Doors[0].OpenSound.Buffer == null)
				{
					train.Cars[i].Doors[0].OpenSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorOpn.wav", SoundCfgParser.smallRadius, left);
				}
				if (train.Cars[i].Doors[1].OpenSound.Buffer == null)
				{
					train.Cars[i].Doors[1].OpenSound = new CarSound(Plugin.currentHost, train.TrainFolder, "DoorOpn.wav", SoundCfgParser.smallRadius, right);
				}
				train.Handles.EmergencyBrake.ApplicationSound = new CarSound(Plugin.currentHost, train.TrainFolder, "EmrBrake.wav", SoundCfgParser.mediumRadius, center);
				train.Cars[i].Sounds.Flange = SoundCfgParser.TryLoadSoundDictionary(train.TrainFolder, "Flange", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.Loop = new CarSound(Plugin.currentHost, train.TrainFolder, "Loop.wav", SoundCfgParser.mediumRadius, center);
				train.Cars[i].FrontAxle.PointSounds = new[]
				{
					new CarSound(Plugin.currentHost, train.TrainFolder, "Point.wav", SoundCfgParser.smallRadius, frontaxle)
				};
				train.Cars[i].RearAxle.PointSounds = new[]
				{
					new CarSound(Plugin.currentHost, train.TrainFolder, "Point.wav", SoundCfgParser.smallRadius, rearaxle)
				};
				train.Cars[i].CarBrake.Rub = new CarSound(Plugin.currentHost, train.TrainFolder, "Rub.wav", SoundCfgParser.mediumRadius, center);
				train.Cars[i].Sounds.Run = SoundCfgParser.TryLoadSoundDictionary(train.TrainFolder, "Run", ".wav", center, SoundCfgParser.mediumRadius);
				train.Cars[i].Sounds.SpringL = new CarSound(Plugin.currentHost, train.TrainFolder, "SpringL.wav", SoundCfgParser.smallRadius, left);
				train.Cars[i].Sounds.SpringR = new CarSound(Plugin.currentHost, train.TrainFolder, "SpringR.wav", SoundCfgParser.smallRadius, right);
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
								CarSound snd = new CarSound(Plugin.currentHost, train.TrainFolder, "Motor" + idx.ToString(Culture) + ".wav", SoundCfgParser.mediumRadius, center);
								train.Cars[i].Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
							}
						}
					}
				}
			}
		}
	}
}
