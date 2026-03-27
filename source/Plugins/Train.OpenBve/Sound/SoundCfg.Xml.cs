using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Sounds;
using SoundManager;
using System;
using System.Collections.Generic;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Motor;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class SoundXmlParser
	{
		internal readonly Plugin Plugin;

		internal SoundXmlParser(Plugin plugin)
		{
			this.Plugin = plugin;
		}

		internal bool ParseTrain(string fileName, TrainBase train)
		{
			for (int i = 0; i < train.Cars.Length; i++)
			{
				try
				{
					Parse(fileName, ref train, ref train.Cars[i], i == train.DriverCar);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}

		private string currentPath;
		internal void Parse(string fileName, ref TrainBase Train, ref CarBase car, bool isDriverCar)
		{
			//3D center of the car
			Vector3 center = Vector3.Zero;
			//Positioned to the left of the car, but centered Y & Z
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			//Positioned to the right of the car, but centered Y & Z
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			//Positioned at the front of the car, centered X and Y
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * car.Length);
			//Positioned at the rear of the car centered X and Y
			Vector3 rear = new Vector3(0.0, 0.0, -0.5 * car.Length);
			//Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
			Vector3 panel = new Vector3(car.Driver.X, car.Driver.Y, car.Driver.Z + 1.0);
			currentPath = Path.GetDirectoryName(fileName);
			XMLFile<SoundXMLSection, SoundXMLKey> xmlFile = new XMLFile<SoundXMLSection, SoundXMLKey>(fileName, "/openBVE/CarSounds", Plugin.CurrentHost);
			while (xmlFile.RemainingSubBlocks > 0)
			{
				Block<SoundXMLSection, SoundXMLKey> subBlock = xmlFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case SoundXMLSection.ATS:
						if (isDriverCar)
						{
							ParseDictionaryBlock(subBlock, out car.Sounds.Plugin, center, SoundCfgParser.mediumRadius);
						}
						break;
					case SoundXMLSection.Brake:
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> brakeSubBlock = subBlock.ReadNextBlock();
							switch (brakeSubBlock.Key)
							{
								case SoundXMLSection.ReleaseHigh:
									//Release brakes from high pressure
									ParseBlock(brakeSubBlock, out car.CarBrake.AirHigh, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Release:
									//Release brakes from normal pressure
									ParseBlock(brakeSubBlock, out car.CarBrake.Air, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.ReleaseFull:
									//Release brakes from full pressure
									ParseBlock(brakeSubBlock, out car.CarBrake.AirZero, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Emergency:
									//Apply EB
									ParseBlock(brakeSubBlock, out Train.Handles.EmergencyBrake.ApplicationSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.EmergencyRelease:
									//Release EB
									ParseBlock(brakeSubBlock, out Train.Handles.EmergencyBrake.ReleaseSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Application:
									//Standard application
									ParseBlock(brakeSubBlock, out car.CarBrake.Release, center, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.BrakeHandle:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> brakeHandleSubBlock = subBlock.ReadNextBlock();
							switch (brakeHandleSubBlock.Key)
							{
								case SoundXMLSection.Apply:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.Increase, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.ApplyFast:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.IncreaseFast, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Release:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.Decrease, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.ReleaseFast:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.DecreaseFast, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Min:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.Min, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Max:
									ParseBlock(brakeHandleSubBlock, out Train.Handles.Brake.Max, panel, SoundCfgParser.tinyRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Breaker:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> breakerSubBlock = subBlock.ReadNextBlock();
							switch (breakerSubBlock.Key)
							{
								case SoundXMLSection.On:
									ParseBlock(breakerSubBlock, out car.Breaker.Resume, panel,
										SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Off:
									ParseBlock(breakerSubBlock, out car.Breaker.ResumeOrInterrupt, panel,
										SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Buzzer:
						ParseBlock(subBlock, out Train.SafetySystems.StationAdjust.AdjustAlarm, panel, SoundCfgParser.tinyRadius);
						break;
					case SoundXMLSection.Compressor:
						if (!(car.CarBrake is AirBrake airBrake) || airBrake.BrakeType != BrakeType.Main)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> compressorSubBlock = subBlock.ReadNextBlock();
							switch (compressorSubBlock.Key)
							{
								case SoundXMLSection.Start:
									ParseBlock(compressorSubBlock, out airBrake.Compressor.StartSound, center, SoundCfgParser.mediumRadius);
									break;
								case SoundXMLSection.Loop:
									ParseBlock(compressorSubBlock, out airBrake.Compressor.LoopSound, center, SoundCfgParser.mediumRadius);
									break;
								case SoundXMLSection.Stop:
									ParseBlock(compressorSubBlock, out airBrake.Compressor.EndSound, center, SoundCfgParser.mediumRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Door:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> doorSubBlock = subBlock.ReadNextBlock();
							switch (doorSubBlock.Key)
							{
								case SoundXMLSection.OpenLeft:
									ParseBlock(doorSubBlock, out car.Doors[0].OpenSound, left, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.OpenRight:
									ParseBlock(doorSubBlock, out car.Doors[1].OpenSound, right, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.CloseLeft:
									ParseBlock(doorSubBlock, out car.Doors[0].CloseSound, left, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.CloseRight:
									ParseBlock(doorSubBlock, out car.Doors[1].CloseSound, right, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Flange:
						ParseDictionaryBlock(subBlock, out car.Flange.Sounds, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.Horn:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> hornSubBlock = subBlock.ReadNextBlock();
							switch (hornSubBlock.Key)
							{
								case SoundXMLSection.Primary:
									ParseHornBlock(hornSubBlock, car, out car.Horns[0], front,
										SoundCfgParser.largeRadius);
									break;
								case SoundXMLSection.Secondary:
									ParseHornBlock(hornSubBlock, car, out car.Horns[1], front,
										SoundCfgParser.largeRadius);
									break;
								case SoundXMLSection.Music:
									ParseHornBlock(hornSubBlock, car, out car.Horns[2], front,
										SoundCfgParser.largeRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Loop:
						ParseBlock(subBlock, out car.Sounds.Loop, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.MasterController:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> masterControllerSubBlock = subBlock.ReadNextBlock();
							switch (masterControllerSubBlock.Key)
							{
								case SoundXMLSection.Up:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.Increase, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.UpFast:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.IncreaseFast, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Down:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.Decrease, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.DownFast:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.DecreaseFast, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Minimum:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.Min, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Maximum:
									ParseBlock(masterControllerSubBlock, out Train.Handles.Power.Max, panel, SoundCfgParser.tinyRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Motor:
						if (!car.TractionModel.ProvidesPower)
						{
							break;
						}
						if (TrainXmlParser.MotorSoundXMLParsed == null)
						{
							TrainXmlParser.MotorSoundXMLParsed = new bool[Train.Cars.Length];
						}
						TrainXmlParser.MotorSoundXMLParsed[car.Index] = true;
						ParseMotorSoundTableBlock(subBlock, car, ref car.TractionModel.MotorSounds, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.PilotLamp:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> pilotLampSubBlock = subBlock.ReadNextBlock();
							switch (pilotLampSubBlock.Key)
							{
								case SoundXMLSection.On:
									ParseBlock(pilotLampSubBlock, out Train.SafetySystems.PilotLamp.OnSound, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Off:
									ParseBlock(pilotLampSubBlock, out Train.SafetySystems.PilotLamp.OffSound, panel, SoundCfgParser.tinyRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Switch:
						ParseArrayBlock(subBlock, out CarSound[] pointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
						// ReSharper disable once CoVariantArrayConversion
						car.FrontAxle.PointSounds = pointSounds;
						// ReSharper disable once CoVariantArrayConversion
						car.RearAxle.PointSounds = pointSounds;
						break;
					case SoundXMLSection.SwitchFrontAxle:
						ParseArrayBlock(subBlock, out CarSound[] frontAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
						// ReSharper disable once CoVariantArrayConversion
						car.FrontAxle.PointSounds = frontAxlePointSounds;
						break;
					case SoundXMLSection.SwitchRearAxle:
						ParseArrayBlock(subBlock, out CarSound[] rearAxlePointSounds, new Vector3(0.0, 0.0, car.FrontAxle.Position), SoundCfgParser.smallRadius);
						// ReSharper disable once CoVariantArrayConversion
						car.RearAxle.PointSounds = rearAxlePointSounds;
						break;
					case SoundXMLSection.Reverser:
						if (!isDriverCar)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> reverserSubBlock = subBlock.ReadNextBlock();
							switch (reverserSubBlock.Key)
							{
								case SoundXMLSection.On:
									ParseBlock(reverserSubBlock, out Train.Handles.Reverser.EngageSound, panel, SoundCfgParser.tinyRadius);
									break;
								case SoundXMLSection.Off:
									ParseBlock(reverserSubBlock, out Train.Handles.Reverser.ReleaseSound, panel, SoundCfgParser.tinyRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Run:
						ParseDictionaryBlock(subBlock, out car.Run.Sounds, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.Shoe:
						ParseBlock(subBlock, out car.CarBrake.Rub, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.Suspension:
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> suspensionSubBlock = subBlock.ReadNextBlock();
							switch (suspensionSubBlock.Key)
							{
								case SoundXMLSection.Left:
									ParseBlock(suspensionSubBlock, out car.Suspension.SpringL, left, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Right:
									ParseBlock(suspensionSubBlock, out car.Suspension.SpringR, right, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.RequestStop:
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> requestStopSubBlock = subBlock.ReadNextBlock();
							switch (requestStopSubBlock.Key)
							{
								case SoundXMLSection.Stop:
									ParseBlock(requestStopSubBlock, out car.Sounds.RequestStop[0], center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Pass:
									ParseBlock(requestStopSubBlock, out car.Sounds.RequestStop[1], center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Ignored:
									ParseBlock(requestStopSubBlock, out car.Sounds.RequestStop[2], center, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Touch:
						if (!isDriverCar)
						{
							break;
						}
						ParseDictionaryBlock(subBlock, out car.Sounds.Touch, center, SoundCfgParser.mediumRadius);
						break;
					case SoundXMLSection.Sanders:
						Sanders sanders = car.ReAdhesionDevice as Sanders;
						if (sanders == null)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> sandersSubBlock = subBlock.ReadNextBlock();
							switch (sandersSubBlock.Key)
							{
								case SoundXMLSection.Activate:
									ParseBlock(sandersSubBlock, out sanders.ActivationSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.EmptyActivate:
									ParseBlock(sandersSubBlock, out sanders.EmptyActivationSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.DeActivate:
									ParseBlock(sandersSubBlock, out sanders.DeActivationSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Loop:
									ParseBlock(sandersSubBlock, out sanders.LoopSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Empty:
									ParseBlock(sandersSubBlock, out sanders.EmptySound, center, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Coupler:
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> couplerSubBlock = subBlock.ReadNextBlock();
							switch (couplerSubBlock.Key)
							{
								case SoundXMLSection.Uncouple:
									ParseBlock(couplerSubBlock, out car.Coupler.UncoupleSound, rear, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Couple:
									ParseBlock(couplerSubBlock, out car.Coupler.CoupleSound, rear, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.CoupleCab:
									ParseBlock(couplerSubBlock, out car.Sounds.CoupleCab, rear, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.UncoupleCab:
									ParseBlock(couplerSubBlock, out car.Sounds.UncoupleCab, rear, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.DriverSupervisionDevice:
						if (!car.SafetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out DriverSupervisionDevice dsd))
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> driverSupervisionSubBlock = subBlock.ReadNextBlock();
							switch (driverSupervisionSubBlock.Key)
							{
								case SoundXMLSection.Alert:
									ParseBlock(driverSupervisionSubBlock, out dsd.AlertSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Alarm:
									ParseBlock(driverSupervisionSubBlock, out dsd.AlarmSound, center, SoundCfgParser.smallRadius);
									break;
								case SoundXMLSection.Reset:
									ParseBlock(driverSupervisionSubBlock, out dsd.ResetSound, center, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Headlights:
						if (Train.SafetySystems.Headlights == null)
						{
							break;
						}
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> headlightsSubBlock = subBlock.ReadNextBlock();
							switch (headlightsSubBlock.Key)
							{
								case SoundXMLSection.Switch:
									ParseBlock(headlightsSubBlock, out Train.SafetySystems.Headlights.SwitchSoundBuffer, panel, SoundCfgParser.smallRadius);
									break;
							}
						}
						break;
					case SoundXMLSection.Pantograph:
						CarSound raiseSound = null, lowerSound = null;
						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<SoundXMLSection, SoundXMLKey> pantographSubBlock = subBlock.ReadNextBlock();
							switch (pantographSubBlock.Key)
							{
								case SoundXMLSection.Raise:
									ParseBlock(pantographSubBlock, out raiseSound, center, SoundCfgParser.mediumRadius);
									break;
								case SoundXMLSection.Lower:
									ParseBlock(pantographSubBlock, out lowerSound, center, SoundCfgParser.mediumRadius);
									break;
							}
						}
						for (int i = 0; i < Train.Cars.Length; i++)
						{
							if (Train.Cars[i].TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph p))
							{
								p.RaiseSound = raiseSound.Clone();
								p.LowerSound = lowerSound.Clone();
							}
						}
						break;
				}
			}
		}

		/// <summary>Parses an XML horn block</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="car">The car</param>
		/// <param name="Horn">The horn to apply this block's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private void ParseHornBlock(Block<SoundXMLSection, SoundXMLKey> block, CarBase car, out Horn Horn, Vector3 Position, double Radius)
		{
			Horn = new Horn(car);
			while (block.RemainingSubBlocks > 0)
			{
				Block<SoundXMLSection, SoundXMLKey> subBlock = block.ReadNextBlock();
				switch (subBlock.Key)
				{
					case SoundXMLSection.Start:
						ParseBlock(subBlock, out Horn.StartSound, ref Position, Radius);
						Horn.StartEndSounds = true;
						break;
					case SoundXMLSection.End:
						ParseBlock(subBlock, out Horn.EndSound, ref Position, Radius);
						Horn.StartEndSounds = true;
						break;
				}

				if (block.GetValue(SoundXMLKey.Toggle, out bool toggle) && toggle)
				{
					Horn.Loop = true;
				}
				Horn.SoundPosition = Position;
			}
		}

		/// <summary>Parses an XML motor table block into a BVE motor sound table</summary>
		/// <param name="block">The block</param>
		/// <param name="Car">The car</param>
		/// <param name="motorSound">The motor sound tables to assign this block's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private void ParseMotorSoundTableBlock(Block<SoundXMLSection, SoundXMLKey> block, CarBase Car, ref AbstractMotorSound motorSound, Vector3 Position, double Radius)
		{
			while (block.RemainingSubBlocks > 0)
			{
				Block<SoundXMLSection, SoundXMLKey> subBlock = block.ReadNextBlock();
				if (subBlock.Key != SoundXMLSection.Sound)
				{
					continue;
				}

				if (subBlock.GetValue(SoundXMLKey.Index, out int idx, NumberRange.NonNegative))
				{
					if (motorSound == null && Plugin.MotorSoundTables != null)
					{
						// We are using train.dat, and the sound.xml in extension mode but this car was not initially set as a motor car
						// Construct a new motor sound table
						motorSound = new BVEMotorSound(Car, 18.0, Plugin.MotorSoundTables);
					}

					if (motorSound is BVEMotorSound bveMotorSound)
					{
						for (int i = 0; i < bveMotorSound.Tables.Length; i++)
						{
							bveMotorSound.Tables[i].Buffer = null;
							bveMotorSound.Tables[i].Source = null;
							for (int j = 0; j < bveMotorSound.Tables[i].Entries.Length; j++)
							{
								if (idx == bveMotorSound.Tables[i].Entries[j].SoundIndex)
								{
									ParseBlock(subBlock, out bveMotorSound.Tables[i].Entries[j].Buffer, ref Position, Radius);
								}
							}
						}
					}
					else if (motorSound is BVE5MotorSound bve5MotorSound)
					{
						if (idx >= bve5MotorSound.SoundBuffers.Length)
						{
							Array.Resize(ref bve5MotorSound.SoundBuffers, idx + 1);
						}
						ParseBlock(subBlock, out bve5MotorSound.SoundBuffers[idx], ref Position, Radius);
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unsupported motor sound type in XML block " + subBlock.Key);
					}
				}
			}
		}

		/// <summary>Parses a single XML block into a sound buffer and position reference</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the block)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the block)</param>
		private void ParseBlock(Block<SoundXMLSection, SoundXMLKey> block, out SoundBuffer Sound, ref Vector3 Position, double Radius)
		{
			if (!block.GetPath(SoundXMLKey.FileName, currentPath, out string fileName))
			{
				Sound = null;
				return;
			}
			block.TryGetVector3(SoundXMLKey.Position, ',', ref Position);
			block.TryGetValue(SoundXMLKey.Radius, ref Radius, NumberRange.Positive);
			Plugin.CurrentHost.RegisterSound(fileName, Radius, out SoundHandle sndHandle);
			Sound = sndHandle as SoundBuffer;
		}

		/// <summary>Parses a single XML block into a sound buffer and position reference</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the block)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the block)</param>
		private void ParseBlock(Block<SoundXMLSection, SoundXMLKey> block, out SoundBuffer Sound, Vector3 Position, double Radius)
		{
			if (!block.GetPath(SoundXMLKey.FileName, currentPath, out string fileName))
			{
				Sound = null;
				return;
			}
			block.TryGetVector3(SoundXMLKey.Position, ',', ref Position);
			block.TryGetValue(SoundXMLKey.Radius, ref Radius, NumberRange.Positive);
			Plugin.CurrentHost.RegisterSound(fileName, Radius, out SoundHandle sndHandle);
			Sound = sndHandle as SoundBuffer;
		}

		/// <summary>Parses a single XML block into a car sound</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the block)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the block)</param>
		private void ParseBlock(Block<SoundXMLSection, SoundXMLKey> block, out CarSound Sound, Vector3 Position, double Radius)
		{
			if (!block.GetPath(SoundXMLKey.FileName, currentPath, out string soundFile))
			{
				Sound = new CarSound();
				return;
			}

			block.TryGetVector3(SoundXMLKey.Position, ',', ref Position);
			block.TryGetValue(SoundXMLKey.Radius, ref Radius, NumberRange.Positive);
			Sound = new CarSound(Plugin.CurrentHost, soundFile, Radius, Position);
		}

		/// <summary>Parses an XML block containing a list of sounds into a car sound array</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any block)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any block)</param>
		private void ParseArrayBlock(Block<SoundXMLSection, SoundXMLKey> block, out CarSound[] Sounds, Vector3 Position, double Radius)
		{
			Sounds = new CarSound[0];
			while (block.RemainingSubBlocks > 0)
			{
				Block<SoundXMLSection, SoundXMLKey> subBlock = block.ReadNextBlock();
				if (subBlock.Key != SoundXMLSection.Sound)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected extra block " + subBlock.Key);
					continue;
				}

				if (subBlock.GetValue(SoundXMLKey.Index, out int idx, NumberRange.NonNegative))
				{
					int l = Sounds.Length;
					Array.Resize(ref Sounds, idx + 1);
					while (l < Sounds.Length)
					{
						Sounds[l] = new CarSound();
						l++;
					}
					ParseBlock(subBlock, out Sounds[idx], Position, Radius);
				}
			}
		}

		/// <summary>Parses an XML block containing a list of sounds into a car sound array</summary>
		/// <param name="block">The block to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any block)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any block)</param>
		private void ParseDictionaryBlock(Block<SoundXMLSection, SoundXMLKey> block, out Dictionary<int, CarSound> Sounds, Vector3 Position, double Radius)
		{
			Sounds = new Dictionary<int, CarSound>();
			while (block.RemainingSubBlocks > 0)
			{
				Block<SoundXMLSection, SoundXMLKey> subBlock = block.ReadNextBlock();
				if (subBlock.Key != SoundXMLSection.Sound)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected extra block " + subBlock.Key);
					continue;
				}

				if (subBlock.GetValue(SoundXMLKey.Index, out int idx, NumberRange.NonNegative))
				{
					ParseBlock(subBlock, out CarSound sound, Position, Radius);
					Sounds[idx] = sound;
				}
			}
		}
	}
}
