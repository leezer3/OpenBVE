using System;
using System.Collections.Generic;
using Formats.OpenBve;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Motor;
using TrainManager.Trains;

namespace Train.OpenBve
{
	// ReSharper disable once InconsistentNaming
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

			// parse configuration file
			Dictionary<int, string> motorFiles = new Dictionary<int, string>();

			ConfigFile<SoundCfgSection, SoundCfgKey> cfg = new ConfigFile<SoundCfgSection, SoundCfgKey>(FileName, Plugin.CurrentHost, "Version 1.0");
			while (cfg.RemainingSubBlocks > 0)
			{
				Block<SoundCfgSection, SoundCfgKey> block = cfg.ReadNextBlock();
				switch (block.Key)
				{
					case SoundCfgSection.Run:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var runIndex, out var fileName))
						{
							for (int c = 0; c < train.Cars.Length; c++)
							{
								if (train.Cars[c].Run == null)
								{
									train.Cars[c].Run.Sounds = new Dictionary<int, CarSound>();
								}

								if (train.Cars[c].Run.Sounds.ContainsKey(runIndex))
								{
									train.Cars[c].Run.Sounds[runIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.mediumRadius, center);
								}
								else
								{
									train.Cars[c].Run.Sounds.Add(runIndex, new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.mediumRadius, center));
								}
							}
						}
						break;
					case SoundCfgSection.Flange:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var flangeIndex, out var fileName))
						{
							for (int c = 0; c < train.Cars.Length; c++)
							{
								if (train.Cars[c].Flange.Sounds == null)
								{
									train.Cars[c].Flange.Sounds = new Dictionary<int, CarSound>();
								}

								if (train.Cars[c].Flange.Sounds.ContainsKey(flangeIndex))
								{
									train.Cars[c].Flange.Sounds[flangeIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.mediumRadius, center);
								}
								else
								{
									train.Cars[c].Flange.Sounds.Add(flangeIndex, new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.mediumRadius, center));
								}
							}
						}

						break;
					case SoundCfgSection.Motor:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var motorIndex, out var fileName))
						{
							motorFiles[motorIndex] = fileName;

							if (!System.IO.File.Exists(motorFiles[motorIndex]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, true, "File " + motorFiles[motorIndex] + " does not exist in file " + FileName);
								motorFiles[motorIndex] = string.Empty;
							}
						}
						break;
					case SoundCfgSection.Switch:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var switchIndex, out var fileName))
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
								train.Cars[c].FrontAxle.PointSounds[switchIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.smallRadius, frontaxle);
								train.Cars[c].RearAxle.PointSounds[switchIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.smallRadius, rearaxle);
							}
						}
						break;
					case SoundCfgSection.Brake:
						block.GetPath(SoundCfgKey.BcReleaseHigh, trainFolder, out string bcReleaseHigh);
						block.GetPath(SoundCfgKey.BcRelease, trainFolder, out string bcRelease);
						block.GetPath(SoundCfgKey.BcReleaseFull, trainFolder, out string bcReleaseFull);
						block.GetPath(SoundCfgKey.Emergency, trainFolder, out string emergency);
						block.GetPath(SoundCfgKey.EmergencyRelease, trainFolder, out string emergencyRelease);
						block.GetPath(SoundCfgKey.BpDecomp, trainFolder, out string bpDecomp);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].CarBrake.AirHigh = new CarSound(Plugin.CurrentHost, bcReleaseHigh, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.Air = new CarSound(Plugin.CurrentHost, bcRelease, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.AirZero = new CarSound(Plugin.CurrentHost, bcReleaseFull, SoundCfgParser.smallRadius, center);
							train.Cars[c].CarBrake.Release = new CarSound(Plugin.CurrentHost, bpDecomp, SoundCfgParser.smallRadius, center);
							
						}
						train.Handles.EmergencyBrake.ApplicationSound = new CarSound(Plugin.CurrentHost, emergency, SoundCfgParser.mediumRadius, center);
						train.Handles.EmergencyBrake.ReleaseSound = new CarSound(Plugin.CurrentHost, emergencyRelease, SoundCfgParser.mediumRadius, center);
						break;
					case SoundCfgSection.Compressor:
						block.GetPath(SoundCfgKey.Attack, trainFolder, out string attack);
						block.GetPath(SoundCfgKey.Loop, trainFolder, out string loop);
						block.GetPath(SoundCfgKey.Release, trainFolder, out string release);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							if (train.Cars[c].CarBrake is AirBrake airBrake && airBrake.BrakeType == BrakeType.Main)
							{
								airBrake.Compressor.StartSound = new CarSound(Plugin.CurrentHost, attack, SoundCfgParser.mediumRadius, center);
								airBrake.Compressor.LoopSound = new CarSound(Plugin.CurrentHost, loop, SoundCfgParser.mediumRadius, center);
								airBrake.Compressor.EndSound = new CarSound(Plugin.CurrentHost, release, SoundCfgParser.mediumRadius, center);
							}
						}
						break;
					case SoundCfgSection.Suspension:
						block.GetPath(SoundCfgKey.Left, trainFolder, out string springL);
						block.GetPath(SoundCfgKey.Right, trainFolder, out string springR);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].Suspension.SpringL = new CarSound(Plugin.CurrentHost, springL, SoundCfgParser.smallRadius, left);
							train.Cars[c].Suspension.SpringR = new CarSound(Plugin.CurrentHost, springR, SoundCfgParser.smallRadius, left);
						}
						break;
					case SoundCfgSection.Horn:
						if (block.GetPath(SoundCfgKey.Primary, trainFolder, out string primaryLoop) || block.GetPath(SoundCfgKey.PrimaryLoop, trainFolder, out primaryLoop))
						{
							Plugin.CurrentHost.RegisterSound(primaryLoop, SoundCfgParser.largeRadius, out var sound);
							train.Cars[train.DriverCar].Horns[0].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[0].Loop = false;
							if (block.GetPath(SoundCfgKey.PrimaryStart, trainFolder, out string primaryStart))
							{
								Plugin.CurrentHost.RegisterSound(primaryStart, SoundCfgParser.largeRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[0].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}
							if (block.GetPath(SoundCfgKey.PrimaryEnd, trainFolder, out string primaryEnd))
							{
								Plugin.CurrentHost.RegisterSound(primaryEnd, SoundCfgParser.largeRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[0].EndSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
							}
						}
						if (block.GetPath(SoundCfgKey.Secondary, trainFolder, out string secondaryLoop) || block.GetPath(SoundCfgKey.SecondaryLoop, trainFolder, out secondaryLoop))
						{
							Plugin.CurrentHost.RegisterSound(secondaryLoop, SoundCfgParser.largeRadius, out var sound);
							train.Cars[train.DriverCar].Horns[1].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[1].Loop = false;
							if (block.GetPath(SoundCfgKey.SecondaryStart, trainFolder, out string secondaryStart))
							{
								Plugin.CurrentHost.RegisterSound(secondaryStart, SoundCfgParser.largeRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[1].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
							}
							if (block.GetPath(SoundCfgKey.SecondaryEnd, trainFolder, out string secondaryEnd))
							{
								Plugin.CurrentHost.RegisterSound(secondaryEnd, SoundCfgParser.largeRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[1].EndSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
							}

						}
						if (block.GetPath(SoundCfgKey.Music, trainFolder, out string musicLoop) || block.GetPath(SoundCfgKey.MusicLoop, trainFolder, out musicLoop))
						{
							Plugin.CurrentHost.RegisterSound(musicLoop, SoundCfgParser.mediumRadius, out var sound);
							train.Cars[train.DriverCar].Horns[2].LoopSound = sound as SoundBuffer;
							train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
							train.Cars[train.DriverCar].Horns[2].Loop = true;
							if (block.GetPath(SoundCfgKey.MusicStart, trainFolder, out string musicStart))
							{
								Plugin.CurrentHost.RegisterSound(musicStart, SoundCfgParser.mediumRadius, out var startSound);
								train.Cars[train.DriverCar].Horns[2].StartSound = startSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
							}
							if (block.GetPath(SoundCfgKey.MusicEnd, trainFolder, out string musicEnd))
							{
								Plugin.CurrentHost.RegisterSound(musicEnd, SoundCfgParser.mediumRadius, out var endSound);
								train.Cars[train.DriverCar].Horns[2].EndSound = endSound as SoundBuffer;
								train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
							}

						}
						break;
					case SoundCfgSection.Door:
						block.GetPath(SoundCfgKey.OpenLeft, trainFolder, out string openLeft);
						block.GetPath(SoundCfgKey.CloseLeft, trainFolder, out string closeLeft);
						block.GetPath(SoundCfgKey.OpenRight, trainFolder, out string openRight);
						block.GetPath(SoundCfgKey.CloseRight, trainFolder, out string closeRight);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].Doors[0].OpenSound = new CarSound(Plugin.CurrentHost, openLeft, SoundCfgParser.smallRadius, left);
							train.Cars[c].Doors[0].CloseSound = new CarSound(Plugin.CurrentHost, closeLeft, SoundCfgParser.smallRadius, left);
							train.Cars[c].Doors[1].OpenSound = new CarSound(Plugin.CurrentHost, openRight, SoundCfgParser.smallRadius, right);
							train.Cars[c].Doors[1].CloseSound = new CarSound(Plugin.CurrentHost, closeRight, SoundCfgParser.smallRadius, right);
						}
						break;
					case SoundCfgSection.ATS:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var atsIndex, out var fileName))
						{
							if (!train.Cars[train.DriverCar].Sounds.Plugin.ContainsKey(atsIndex))
							{
								train.Cars[train.DriverCar].Sounds.Plugin.Add(atsIndex, new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.tinyRadius, panel));
							}
							else
							{
								train.Cars[train.DriverCar].Sounds.Plugin[atsIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.tinyRadius, panel);
							}
						}
						break;
					case SoundCfgSection.Buzzer:
						block.GetPath(SoundCfgKey.Correct, trainFolder, out string buzzerCorrect);
						train.SafetySystems.StationAdjust.AdjustAlarm = new CarSound(Plugin.CurrentHost, buzzerCorrect, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.PilotLamp:
						block.GetPath(SoundCfgKey.On, trainFolder, out string lampOn);
						block.GetPath(SoundCfgKey.Off, trainFolder, out string lampOff);
						train.SafetySystems.PilotLamp.OnSound = new CarSound(Plugin.CurrentHost, lampOn, SoundCfgParser.tinyRadius, panel);
						train.SafetySystems.PilotLamp.OffSound = new CarSound(Plugin.CurrentHost, lampOff, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.BrakeHandle:
						block.GetPath(SoundCfgKey.Apply, trainFolder, out string apply);
						block.GetPath(SoundCfgKey.ApplyFast, trainFolder, out string applyFast);
						block.GetPath(SoundCfgKey.Release, trainFolder, out string brakeRelease);
						block.GetPath(SoundCfgKey.ReleaseFast, trainFolder, out string brakeReleaseFast);
						block.GetPath(SoundCfgKey.Min, trainFolder, out string brakeMin);
						block.GetPath(SoundCfgKey.Max, trainFolder, out string brakeMax);
						train.Handles.Brake.Increase = new CarSound(Plugin.CurrentHost, apply, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.IncreaseFast = new CarSound(Plugin.CurrentHost, applyFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Decrease = new CarSound(Plugin.CurrentHost, brakeRelease, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.DecreaseFast = new CarSound(Plugin.CurrentHost, brakeReleaseFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Min = new CarSound(Plugin.CurrentHost, brakeMin, SoundCfgParser.tinyRadius, panel);
						train.Handles.Brake.Max = new CarSound(Plugin.CurrentHost, brakeMax, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.MasterController:
						block.GetPath(SoundCfgKey.Up, trainFolder, out string up);
						block.GetPath(SoundCfgKey.UpFast, trainFolder, out string upFast);
						block.GetPath(SoundCfgKey.Down, trainFolder, out string down);
						block.GetPath(SoundCfgKey.DownFast, trainFolder, out string downFast);
						block.GetPath(SoundCfgKey.Min, trainFolder, out string powerMin);
						block.GetPath(SoundCfgKey.Max, trainFolder, out string powerMax);
						train.Handles.Power.Increase = new CarSound(Plugin.CurrentHost, up, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.IncreaseFast = new CarSound(Plugin.CurrentHost, upFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Decrease = new CarSound(Plugin.CurrentHost, down, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.DecreaseFast = new CarSound(Plugin.CurrentHost, downFast, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Min = new CarSound(Plugin.CurrentHost, powerMin, SoundCfgParser.tinyRadius, panel);
						train.Handles.Power.Max = new CarSound(Plugin.CurrentHost, powerMax, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.Reverser:
						block.GetPath(SoundCfgKey.On, trainFolder, out string reverserOn);
						block.GetPath(SoundCfgKey.Off, trainFolder, out string reverserOff);
						train.Handles.Reverser.EngageSound = new CarSound(Plugin.CurrentHost, reverserOn, SoundCfgParser.tinyRadius, panel);
						train.Handles.Reverser.ReleaseSound = new CarSound(Plugin.CurrentHost, reverserOff, SoundCfgParser.tinyRadius, panel);
						break;
					case SoundCfgSection.Breaker:
						block.GetPath(SoundCfgKey.On, trainFolder, out string breakerOn);
						block.GetPath(SoundCfgKey.Off, trainFolder, out string breakerOff);
						train.Cars[train.DriverCar].Breaker.Resume = new CarSound(Plugin.CurrentHost, breakerOn, SoundCfgParser.smallRadius, panel);
						train.Cars[train.DriverCar].Breaker.ResumeOrInterrupt = new CarSound(Plugin.CurrentHost, breakerOff, SoundCfgParser.smallRadius, panel);
						break;
					case SoundCfgSection.Others:
						block.GetPath(SoundCfgKey.Noise, trainFolder, out string noise);
						block.GetPath(SoundCfgKey.Shoe, trainFolder, out string rub);
						block.GetPath(SoundCfgKey.Halt, trainFolder, out string halt);
						train.SafetySystems.PassAlarm.Sound = new CarSound(Plugin.CurrentHost, halt, SoundCfgParser.tinyRadius, panel);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							if (train.Cars[c].TractionModel.ProvidesPower | c == train.DriverCar)
							{
								train.Cars[c].Sounds.Loop = new CarSound(Plugin.CurrentHost, noise, SoundCfgParser.mediumRadius, center);
							}
							train.Cars[c].CarBrake.Rub = new CarSound(Plugin.CurrentHost, rub, SoundCfgParser.mediumRadius, center);
						}
						break;
					case SoundCfgSection.RequestStop:
						block.GetPath(SoundCfgKey.Stop, trainFolder, out string requestStop);
						block.GetPath(SoundCfgKey.Pass, trainFolder, out string requestPass);
						block.GetPath(SoundCfgKey.Ignored, trainFolder, out string requestIgnored);
						for (int c = 0; c < train.Cars.Length; c++)
						{
							train.Cars[c].Sounds.RequestStop[0] = new CarSound(Plugin.CurrentHost, requestStop, SoundCfgParser.mediumRadius, panel);
							train.Cars[c].Sounds.RequestStop[1] = new CarSound(Plugin.CurrentHost, requestPass, SoundCfgParser.mediumRadius, panel);
							train.Cars[c].Sounds.RequestStop[2] = new CarSound(Plugin.CurrentHost, requestIgnored, SoundCfgParser.mediumRadius, panel);
						}
						break;
					case SoundCfgSection.Touch:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var touchIndex, out var fileName))
						{
							if (!train.Cars[train.DriverCar].Sounds.Touch.ContainsKey(touchIndex))
							{
								train.Cars[train.DriverCar].Sounds.Touch.Add(touchIndex, new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.tinyRadius, panel));
							}
							else
							{
								train.Cars[train.DriverCar].Sounds.Touch[touchIndex] = new CarSound(Plugin.CurrentHost, fileName, SoundCfgParser.tinyRadius, panel);
							}
						}
						break;
					case SoundCfgSection.Windscreen:
						if (train.Cars[train.DriverCar].Windscreen == null)
						{
							// e.g. non-player train
							break;
						}
						block.GetPath(SoundCfgKey.RainDrop, trainFolder, out string rainDrop);
						block.GetPath(SoundCfgKey.WetWipe, trainFolder, out string wetWipe);
						block.GetPath(SoundCfgKey.DryWipe, trainFolder, out string dryWipe);
						block.GetPath(SoundCfgKey.Switch, trainFolder, out string wiperSwitch);
						train.Cars[train.DriverCar].Windscreen.DropSound = new CarSound(Plugin.CurrentHost, rainDrop, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.WetWipeSound = new CarSound(Plugin.CurrentHost, wetWipe, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.DryWipeSound = new CarSound(Plugin.CurrentHost, dryWipe, SoundCfgParser.tinyRadius, panel);
						train.Cars[train.DriverCar].Windscreen.Wipers.SwitchSound = new CarSound(Plugin.CurrentHost, wiperSwitch, SoundCfgParser.tinyRadius, panel);
						break;
				}
				block.ReportErrors();
			}
			
			// Assign motor sounds to appropriate cars
			for (int c = 0; c < train.Cars.Length; c++)
			{
				if (train.Cars[c].TractionModel.MotorSounds == null)
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
							if (motorFiles.ContainsKey(index) && !string.IsNullOrEmpty(motorFiles[index]))
							{
								Plugin.CurrentHost.RegisterSound(motorFiles[index], SoundCfgParser.mediumRadius, out var mS);
								motorSound.Tables[i].Entries[j].Buffer = mS as SoundBuffer;
							}
						}
					}

					train.Cars[c].TractionModel.MotorSounds = motorSound;
				}
				else
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected motor sound model found in car " + c);
				}
			}
		}
	}
}
