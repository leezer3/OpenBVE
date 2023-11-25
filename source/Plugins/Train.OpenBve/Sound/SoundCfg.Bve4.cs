using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Motor;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class BVE4SoundParser
	{
		internal readonly Plugin Plugin;

		internal BVE4SoundParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Loads the sound set for a BVE4 or openBVE sound.cfg based train</summary>
		/// <param name="train">The train</param>
		/// <param name="FileName">The absolute on-disk path to the sound.cfg file</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		internal void Parse(string FileName, string trainFolder, TrainBase train)
		{
			/*
			 * NOTES:
			 * ------
			 *
			 * BVE2 and BVE4 content shares common (hardcoded) sound radii & positions
			 * Radius is used to define a sphere, in which the sound is audible at full
			 * volume, attenuated outside
			 *
			 * These are terrible for realism (generally much too small, and may well
			 * be in the 'wrong' position) but need to be retained for legacy purposes
			 * 
			 * The sound.xml format should be used if these are to be changed
			 *
			 */

			// 3D center of the car
			Vector3 center = Vector3.Zero;
			// Positioned to the left of the car, but centered Y & Z
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			// Positioned to the right of the car, but centered Y & Z
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			// Positioned at the front of the car, centered X and Y
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			// Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);

			
			

			Encoding Encoding = TextEncoding.GetSystemEncodingFromFile(FileName);

			// parse configuration file
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			Dictionary<int, string> MotorFiles = new Dictionary<int, string>();

			ConfigFile<SoundCfgSection, SoundCfgKey> cfg = new ConfigFile<SoundCfgSection, SoundCfgKey>(Lines, Plugin.currentHost, "Version 1.0");
			while (cfg.RemainingSubBlocks > 0)
			{
				Block<SoundCfgSection, SoundCfgKey> block = cfg.ReadNextBlock();
				switch (block.Key)
				{
					case SoundCfgSection.Run:
						while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var runIndex, out var fileName))
						{
							for (int c = 0; c < train.Cars.Length; c++)
							{
								if (train.Cars[c].Sounds.Run == null)
								{
									train.Cars[c].Sounds.Run = new Dictionary<int, CarSound>();
								}

								if (train.Cars[c].Sounds.Run.ContainsKey(runIndex))
								{
									train.Cars[c].Sounds.Run[runIndex] = new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.mediumRadius, center);
								}
								else
								{
									train.Cars[c].Sounds.Run.Add(runIndex, new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.mediumRadius, center));
								}
							}
						}
						break;
					case SoundCfgSection.Flange:
						while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var flangeIndex, out var fileName))
						{
							for (int c = 0; c < train.Cars.Length; c++)
							{
								if (train.Cars[c].Sounds.Flange == null)
								{
									train.Cars[c].Sounds.Flange = new Dictionary<int, CarSound>();
								}

								if (train.Cars[c].Sounds.Flange.ContainsKey(flangeIndex))
								{
									train.Cars[c].Sounds.Flange[flangeIndex] = new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.mediumRadius, center);
								}
								else
								{
									train.Cars[c].Sounds.Flange.Add(flangeIndex, new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.mediumRadius, center));
								}
							}
						}

						break;
					case SoundCfgSection.Motor:
						while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var motorIndex, out var fileName))
						{
							if (!MotorFiles.ContainsKey(motorIndex))
							{
								MotorFiles.Add(motorIndex, Path.CombineFile(trainFolder, fileName));
							}
							else
							{
								MotorFiles[motorIndex] = Path.CombineFile(trainFolder, fileName);
							}

							if (!System.IO.File.Exists(MotorFiles[motorIndex]))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, true, "File " + MotorFiles[motorIndex] + " does not exist in file " + FileName);
								MotorFiles[motorIndex] = string.Empty;
							}
						}
						break;
					case SoundCfgSection.Switch:
						while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var switchIndex, out var fileName))
						{
							for (int c = 0; c < train.Cars.Length; c++)
							{
								int n = train.Cars[c].FrontAxle.PointSounds.Length;
								if (switchIndex >= n)
								{
									Array.Resize(ref train.Cars[c].FrontAxle.PointSounds, switchIndex + 1);
									Array.Resize(ref train.Cars[c].RearAxle.PointSounds, switchIndex + 1);
									for (int h = n; h < switchIndex; h++)
									{
										train.Cars[c].FrontAxle.PointSounds[h] = new CarSound();
										train.Cars[c].RearAxle.PointSounds[h] = new CarSound();
									}
								}
								Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[c].FrontAxle.Position);
								Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[c].RearAxle.Position);
								train.Cars[c].FrontAxle.PointSounds[switchIndex] = new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.smallRadius, frontaxle);
								train.Cars[c].RearAxle.PointSounds[switchIndex] = new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.smallRadius, rearaxle);
							}
						}
						break;
					case SoundCfgSection.Brake:
						string bcReleaseHigh, bcRelease, bcReleaseFull, emergency, emergencyRelease, bpDecomp;
						block.GetValue(SoundCfgKey.BcReleaseHigh, out bcReleaseHigh);
						block.GetValue(SoundCfgKey.BcRelease, out bcRelease);
						block.GetValue(SoundCfgKey.BcReleaseFull, out bcReleaseFull);
						block.GetValue(SoundCfgKey.Emergency, out emergency);
						block.GetValue(SoundCfgKey.EmergencyRelease, out emergencyRelease);
						block.GetValue(SoundCfgKey.BpDecomp, out bpDecomp);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].CarBrake.AirHigh = new CarSound(Plugin.currentHost, trainFolder, bcReleaseHigh, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.Air = new CarSound(Plugin.currentHost, trainFolder, bcRelease, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.AirZero = new CarSound(Plugin.currentHost, trainFolder, bcReleaseFull, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.Release = new CarSound(Plugin.currentHost, trainFolder, bpDecomp, SoundCfgParser.smallRadius, center);
							
						}
						train.Handles.EmergencyBrake.ApplicationSound = new CarSound(Plugin.currentHost, trainFolder, emergency, SoundCfgParser.mediumRadius, center);
						train.Handles.EmergencyBrake.ReleaseSound = new CarSound(Plugin.currentHost, trainFolder, emergencyRelease, SoundCfgParser.mediumRadius, center);
						break;
					case SoundCfgSection.Compressor:
						string attack, loop, release;
						block.GetValue(SoundCfgKey.Attack, out attack);
						block.GetValue(SoundCfgKey.Loop, out loop);
						block.GetValue(SoundCfgKey.Release, out release);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							if (train.Cars[c].CarBrake.brakeType == BrakeType.Main)
							{
								train.Cars[c].CarBrake.airCompressor.StartSound = new CarSound(Plugin.currentHost, trainFolder, attack, SoundCfgParser.mediumRadius, center);
								train.Cars[c].CarBrake.airCompressor.LoopSound = new CarSound(Plugin.currentHost, trainFolder, loop, SoundCfgParser.mediumRadius, center);
								train.Cars[c].CarBrake.airCompressor.EndSound = new CarSound(Plugin.currentHost, trainFolder, release, SoundCfgParser.mediumRadius, center);
							}
						}
						break;
					case SoundCfgSection.Suspension:
						string springL, springR;
						block.GetValue(SoundCfgKey.Left, out springL);
						block.GetValue(SoundCfgKey.Right, out springR);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].Sounds.SpringL = new CarSound(Plugin.currentHost, trainFolder, springL, SoundCfgParser.smallRadius, left);
							train.Cars[c].Sounds.SpringR = new CarSound(Plugin.currentHost, trainFolder, springR, SoundCfgParser.smallRadius, left);
						}
						break;
					case SoundCfgSection.Horn:
						string primaryLoop, secondaryLoop, musicLoop;
						if (block.GetValue(SoundCfgKey.Primary, out primaryLoop) || block.GetValue(SoundCfgKey.PrimaryLoop, out primaryLoop))
						{
							Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, primaryLoop), SoundCfgParser.largeRadius, out var sound);
							train.Cars[train.DriverCar].Horns[0].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[0].Loop = false;
							if (block.GetValue(SoundCfgKey.PrimaryStart, out string primaryStart))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, primaryStart), SoundCfgParser.largeRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[0].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}
							if (block.GetValue(SoundCfgKey.PrimaryStart, out string primaryEnd))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, primaryEnd), SoundCfgParser.largeRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[0].StartSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}
						}
						if (block.GetValue(SoundCfgKey.Secondary, out secondaryLoop) || block.GetValue(SoundCfgKey.SecondaryLoop, out secondaryLoop))
						{
							Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, secondaryLoop), SoundCfgParser.largeRadius, out var sound);
							train.Cars[train.DriverCar].Horns[1].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[1].Loop = false;
							if (block.GetValue(SoundCfgKey.SecondaryStart, out string secondaryStart))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, secondaryStart), SoundCfgParser.largeRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[1].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
							}
							if (block.GetValue(SoundCfgKey.SecondaryEnd, out string secondaryEnd))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, secondaryEnd), SoundCfgParser.largeRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[1].EndSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
							}

						}
						if (block.GetValue(SoundCfgKey.Music, out musicLoop) || block.GetValue(SoundCfgKey.MusicLoop, out musicLoop))
						{
							Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, musicLoop), SoundCfgParser.mediumRadius, out var sound);
							train.Cars[train.DriverCar].Horns[0].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[0].Loop = false;
							if (block.GetValue(SoundCfgKey.MusicStart, out string musicStart))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, musicStart), SoundCfgParser.mediumRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[0].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}
							if (block.GetValue(SoundCfgKey.MusicEnd, out string musicEnd))
							{
								Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, musicEnd), SoundCfgParser.mediumRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[0].EndSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}

						}
						break;
					case SoundCfgSection.Door:
						string openLeft, closeLeft, openRight, closeRight;
						block.GetValue(SoundCfgKey.OpenLeft, out openLeft);
						block.GetValue(SoundCfgKey.CloseLeft, out closeLeft);
						block.GetValue(SoundCfgKey.OpenRight, out openRight);
						block.GetValue(SoundCfgKey.CloseRight, out closeRight);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].Doors[0].OpenSound = new CarSound(Plugin.currentHost, trainFolder, openLeft, SoundCfgParser.smallRadius, left);
							train.Cars[c].Doors[0].CloseSound = new CarSound(Plugin.currentHost, trainFolder, closeLeft, SoundCfgParser.smallRadius, left);
							train.Cars[c].Doors[1].OpenSound = new CarSound(Plugin.currentHost, trainFolder, openRight, SoundCfgParser.smallRadius, right);
							train.Cars[c].Doors[1].CloseSound = new CarSound(Plugin.currentHost, trainFolder, closeRight, SoundCfgParser.smallRadius, right);
						}
						break;
					case SoundCfgSection.ATS:
						while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var atsIndex, out var fileName))
						{
							if (!train.Cars[train.DriverCar].Sounds.Plugin.ContainsKey(atsIndex))
							{
								train.Cars[train.DriverCar].Sounds.Plugin.Add(atsIndex, new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.tinyRadius, panel));
							}
							else
							{
								train.Cars[train.DriverCar].Sounds.Plugin[atsIndex] = new CarSound(Plugin.currentHost, trainFolder, fileName, SoundCfgParser.tinyRadius, panel);
							}
						}
						break;
					case SoundCfgSection.Buzzer:
						string buzzerCorrect;
						block.GetValue(SoundCfgKey.Correct, out buzzerCorrect);
						train.SafetySystems.StationAdjust.AdjustAlarm = new CarSound(Plugin.currentHost, trainFolder, buzzerCorrect, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.PilotLamp:
						string lampOn, lampOff;
						block.GetValue(SoundCfgKey.On, out lampOn);
						block.GetValue(SoundCfgKey.Off, out lampOff);
						train.SafetySystems.PilotLamp.OnSound = new CarSound(Plugin.currentHost, trainFolder, lampOn, SoundCfgParser.tinyRadius, panel);
						train.SafetySystems.PilotLamp.OffSound = new CarSound(Plugin.currentHost, trainFolder, lampOff, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.BrakeHandle:
						string apply, applyFast, brakeRelease, brakeReleaseFast, brakeMin, brakeMax;
						block.GetValue(SoundCfgKey.Apply, out apply);
						block.GetValue(SoundCfgKey.ApplyFast, out applyFast);
						block.GetValue(SoundCfgKey.Release, out brakeRelease);
						block.GetValue(SoundCfgKey.ReleaseFast, out brakeReleaseFast);
						block.GetValue(SoundCfgKey.Min, out brakeMin);
						block.GetValue(SoundCfgKey.Max, out brakeMax);
						train.Handles.Brake.Increase = new CarSound(Plugin.currentHost, trainFolder, apply, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.IncreaseFast = new CarSound(Plugin.currentHost, trainFolder, applyFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Decrease = new CarSound(Plugin.currentHost, trainFolder, brakeRelease, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.DecreaseFast = new CarSound(Plugin.currentHost, trainFolder, brakeReleaseFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Min = new CarSound(Plugin.currentHost, trainFolder, brakeMin, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Max = new CarSound(Plugin.currentHost, trainFolder, brakeMax, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.MasterController:
						string up, upFast, down, downFast, powerMin, powerMax;
						block.GetValue(SoundCfgKey.Up, out up);
						block.GetValue(SoundCfgKey.UpFast, out upFast);
						block.GetValue(SoundCfgKey.Down, out down);
						block.GetValue(SoundCfgKey.DownFast, out downFast);
						block.GetValue(SoundCfgKey.Min, out powerMin);
						block.GetValue(SoundCfgKey.Max, out powerMax);
						train.Handles.Power.Increase = new CarSound(Plugin.currentHost, trainFolder, up, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.IncreaseFast = new CarSound(Plugin.currentHost, trainFolder, upFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Decrease = new CarSound(Plugin.currentHost, trainFolder, down, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.DecreaseFast = new CarSound(Plugin.currentHost, trainFolder, downFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Min = new CarSound(Plugin.currentHost, trainFolder, powerMin, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Max = new CarSound(Plugin.currentHost, trainFolder, powerMax, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.Reverser:
						string reverserOn, reverserOff;
						block.GetValue(SoundCfgKey.On, out reverserOn);
						block.GetValue(SoundCfgKey.Off, out reverserOff);
						train.Handles.Reverser.EngageSound = new CarSound(Plugin.currentHost, trainFolder, reverserOn, SoundCfgParser.tinyRadius, panel);
						train.Handles.Reverser.ReleaseSound = new CarSound(Plugin.currentHost, trainFolder, reverserOff, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.Breaker:
						string breakerOn, breakerOff;
						block.GetValue(SoundCfgKey.On, out breakerOn);
						block.GetValue(SoundCfgKey.Off, out breakerOff);
						train.Cars[train.DriverCar].Breaker.Resume = new CarSound(Plugin.currentHost, trainFolder, breakerOn, SoundCfgParser.smallRadius, panel);
						train.Cars[train.DriverCar].Breaker.ResumeOrInterrupt = new CarSound(Plugin.currentHost, trainFolder, breakerOff, SoundCfgParser.smallRadius, panel);
						break;
					case SoundCfgSection.Others:
						string noise, rub, halt;
						block.GetValue(SoundCfgKey.Noise, out noise);
						block.GetValue(SoundCfgKey.Shoe, out rub);
						block.GetValue(SoundCfgKey.Halt, out halt);
						train.SafetySystems.PassAlarm.Sound = new CarSound(Plugin.currentHost, trainFolder, halt, SoundCfgParser.tinyRadius, panel);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							if (train.Cars[c].Specs.IsMotorCar | c == train.DriverCar)
							{
								train.Cars[c].Sounds.Loop = new CarSound(Plugin.currentHost, trainFolder, noise, SoundCfgParser.mediumRadius, center);
							}
							train.Cars[c].CarBrake.Rub = new CarSound(Plugin.currentHost, trainFolder, rub, SoundCfgParser.mediumRadius, center);
						}
						break;
					case SoundCfgSection.Windscreen:
						string rainDrop, wetWipe, dryWipe, wiperSwitch;
						block.GetValue(SoundCfgKey.RainDrop, out rainDrop);
						block.GetValue(SoundCfgKey.WetWipe, out wetWipe);
						block.GetValue(SoundCfgKey.DryWipe, out dryWipe);
						block.GetValue(SoundCfgKey.Switch, out wiperSwitch);
						train.Cars[train.DriverCar].Windscreen.DropSound = new CarSound(Plugin.currentHost, trainFolder, rainDrop, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.WetWipeSound = new CarSound(Plugin.currentHost, trainFolder, wetWipe, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.DryWipeSound = new CarSound(Plugin.currentHost, trainFolder, dryWipe, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.SwitchSound = new CarSound(Plugin.currentHost, trainFolder, wiperSwitch, SoundCfgParser.tinyRadius, panel);
						break;
				}
			}
			
			// Assign motor sounds to appropriate cars
			for (int c = 0; c < train.Cars.Length; c++)
			{
				if (train.Cars[c].Specs.IsMotorCar)
				{
					if (train.Cars[c].Sounds.Motor == null)
					{
						BVEMotorSound motorSound = new BVEMotorSound(train.Cars[c], 18.0, Plugin.MotorSoundTables);
						motorSound.Position = center;
						for (int i = 0; i < motorSound.Tables.Length; i++)
						{
							motorSound.Tables[i].Buffer = null;
							motorSound.Tables[i].Source = null;
							for (int j = 0; j < motorSound.Tables[i].Entries.Length; j++)
							{
								int index = motorSound.Tables[i].Entries[j].SoundIndex;
								if (MotorFiles.ContainsKey(index) && !string.IsNullOrEmpty(MotorFiles[index]))
								{
									Plugin.currentHost.RegisterSound(MotorFiles[index], SoundCfgParser.mediumRadius, out var mS);
									motorSound.Tables[i].Entries[j].Buffer = mS as SoundBuffer;
								}
							}
						}

						train.Cars[c].Sounds.Motor = motorSound;
					}
					else
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Unexpected motor sound model found in car " + c);
					}
				}
			}
		}
	}
}
