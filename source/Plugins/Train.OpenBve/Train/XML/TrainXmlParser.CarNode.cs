//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Formats.OpenBve;
using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseCarBlock(Block<TrainXMLSection, TrainXMLKey> block, string fileName, int Car, ref TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref bool visibleFromInterior)
		{
			string currentXMLPath = Path.GetDirectoryName(fileName);
			Vector3 interiorDirection = Vector3.Zero;
			block.TryGetValue(TrainXMLKey.Length, ref Train.Cars[Car].Length);
			block.TryGetValue(TrainXMLKey.Width, ref Train.Cars[Car].Width);
			block.TryGetValue(TrainXMLKey.Height, ref Train.Cars[Car].Height);
			block.TryGetValue(TrainXMLKey.CenterOfGravityHeight, ref Train.Cars[Car].Specs.CenterOfGravityHeight);
			if (block.GetValue(TrainXMLKey.Mass, out double carMass) && carMass > 0)
			{
				Train.Cars[Car].EmptyMass = carMass;
				Train.Cars[Car].CargoMass = 0;
			}
			if (block.GetValue(TrainXMLKey.ExposedFrontalArea, out double ef) && ef > 0)
			{
				Train.Cars[Car].Specs.ExposedFrontalArea = ef;
			}
			else
			{
				// BVE default
				Train.Cars[Car].Specs.ExposedFrontalArea = 0.65 * Train.Cars[Car].Width * Train.Cars[Car].Height;
			}
			if (block.GetValue(TrainXMLKey.UnexposedFrontalArea, out double uf) && uf > 0)
			{
				Train.Cars[Car].Specs.UnexposedFrontalArea = uf;
			}
			else
			{
				// BVE default
				Train.Cars[Car].Specs.UnexposedFrontalArea = 0.2 * Train.Cars[Car].Width * Train.Cars[Car].Height;
			}
			block.TryGetValue(TrainXMLKey.FrontAxle, ref Train.Cars[Car].FrontAxle.Position);
			block.TryGetValue(TrainXMLKey.RearAxle, ref Train.Cars[Car].RearAxle.Position);
			if (block.GetPath(TrainXMLKey.Object, currentXMLPath, out string carObject))
			{
				Plugin.CurrentHost.LoadObject(carObject, Encoding.Default, out CarObjects[Car]);
			}

			block.TryGetValue(TrainXMLKey.VisibleFromInterior, ref visibleFromInterior);
			block.GetValue(TrainXMLKey.Reversed, out CarObjectsReversed[Car]);
			block.TryGetValue(TrainXMLKey.LoadingSway, ref Train.Cars[Car].EnableLoadingSway);
			if(block.GetVector3(TrainXMLKey.DriverPosition, ',', out Vector3 driverPosition))
			{
				driverPosition.Z += 0.5 * Train.Cars[Car].Length;
				Train.Cars[Car].Driver = driverPosition;
			}

			if (block.ReadBlock(TrainXMLSection.CameraRestriction, out Block<TrainXMLSection, TrainXMLKey> cameraRestrictionBlock))
			{
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Backwards, ref Train.Cars[Car].CameraRestriction.BottomLeft.Z);
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Forwards, ref Train.Cars[Car].CameraRestriction.TopRight.Z);
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Left, ref Train.Cars[Car].CameraRestriction.BottomLeft.X);
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Right, ref Train.Cars[Car].CameraRestriction.TopRight.X);
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Up, ref Train.Cars[Car].CameraRestriction.BottomLeft.Y);
				cameraRestrictionBlock.TryGetValue(TrainXMLKey.Down, ref Train.Cars[Car].CameraRestriction.TopRight.Y);
			}

			if (block.ReadBlock(TrainXMLSection.FrontBogie, out Block<TrainXMLSection, TrainXMLKey> frontBogieBlock))
			{
				frontBogieBlock.TryGetValue(TrainXMLKey.FrontAxle, ref Train.Cars[Car].FrontBogie.FrontAxle.Position);
				frontBogieBlock.TryGetValue(TrainXMLKey.RearAxle, ref Train.Cars[Car].FrontBogie.RearAxle.Position);
				if (frontBogieBlock.GetPath(TrainXMLKey.Object, currentXMLPath, out string frontBogieObject))
				{
					Plugin.CurrentHost.LoadObject(frontBogieObject, Encoding.Default, out BogieObjects[Car * 2]);
				}

				frontBogieBlock.GetValue(TrainXMLKey.Reversed, out BogieObjectsReversed[Car * 2]);
			}

			if (block.ReadBlock(TrainXMLSection.RearBogie, out Block<TrainXMLSection, TrainXMLKey> rearBogieBlock))
			{
				rearBogieBlock.TryGetValue(TrainXMLKey.FrontAxle, ref Train.Cars[Car].RearBogie.FrontAxle.Position);
				rearBogieBlock.TryGetValue(TrainXMLKey.RearAxle, ref Train.Cars[Car].RearBogie.RearAxle.Position);
				if (rearBogieBlock.GetPath(TrainXMLKey.Object, currentXMLPath, out string rearBogieObject))
				{
					Plugin.CurrentHost.LoadObject(rearBogieObject, Encoding.Default, out BogieObjects[Car * 2 + 1]);
				}

				rearBogieBlock.GetValue(TrainXMLKey.Reversed, out BogieObjectsReversed[Car * 2 + 1]);
			}

			block.TryGetVector3(TrainXMLKey.InteriorDirection, ',', ref interiorDirection);

			if (block.GetPath(TrainXMLKey.InteriorView, currentXMLPath, out string interiorFile) && Train.IsPlayerTrain)
			{
				if (!Train.Cars[Car].CarSections.ContainsKey(CarSectionType.Interior))
				{
					Train.Cars[Car].CarSections.Add(CarSectionType.Interior, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, false, Train.Cars[Car]));
				}
				Transformation viewTransformation = new Transformation(interiorDirection.X.ToRadians(), interiorDirection.Y.ToRadians(), interiorDirection.Z.ToRadians());
				Train.Cars[Car].CarSections[CarSectionType.Interior].ViewDirection = viewTransformation;
				if (interiorFile.ToLowerInvariant().EndsWith(".xml"))
				{
					XDocument CurrentXML = XDocument.Load(interiorFile, LoadOptions.SetLineInfo);

					// Check for null
					if (CurrentXML.Root == null)
					{
						// We couldn't find any valid XML, so return false
						throw new System.IO.InvalidDataException();
					}
					List<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated").ToList();
					if (DocumentElements != null && DocumentElements.Count != 0)
					{
						string t = Train.TrainFolder;
						Train.TrainFolder = currentXMLPath;
						Plugin.PanelAnimatedXmlParser.ParsePanelAnimatedXml(interiorFile, Train, Car);
						Train.TrainFolder = t;
						if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						{
							Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
						}
						return;
					}
					DocumentElements = CurrentXML.Root.Elements("Panel").ToList();
					if (DocumentElements != null && DocumentElements.Count != 0)
					{
						string t = Train.TrainFolder;
						Train.TrainFolder = currentXMLPath;
						Plugin.PanelXmlParser.ParsePanelXml(interiorFile, Train, Car);
						Train.TrainFolder = t;
						Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
						return;
					}
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".cfg"))
				{
					//Only supports panel2.cfg format
					Plugin.Panel2CfgParser.ParsePanel2Config(System.IO.Path.GetFileName(interiorFile), Path.GetDirectoryName(interiorFile), Train.Cars[Train.DriverCar]);
					Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".animated"))
				{
					Plugin.CurrentHost.LoadObject(interiorFile, Encoding.UTF8, out var currentObject);
					var a = (AnimatedObjectCollection)currentObject;
					if (a != null)
					{
						try
						{
							for (int i = 0; i < a.Objects.Length; i++)
							{
								Plugin.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
							}
							Train.Cars[Car].CarSections[CarSectionType.Interior].Groups[0].Elements = a.Objects;
							if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
							{
								Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							}
						}
						catch
						{
							Plugin.Cancel = true;
						}
					}

				}
				else
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior view file is not supported for Car " + Car + " in XML file " + fileName);
				}
			}

			if (block.ReadBlock(TrainXMLSection.Sanders, out Block<TrainXMLSection, TrainXMLKey> sandersBlock))
			{
				double rate = double.MaxValue;
				double level = 0;
				double applicationTime = 10.0;
				double activationTime = 5.0;
				int shots = int.MaxValue;
				sandersBlock.GetEnumValue(TrainXMLKey.Type, out SandersType type);
				sandersBlock.TryGetValue(TrainXMLKey.Rate, ref rate);
				sandersBlock.TryGetValue(TrainXMLKey.SandLevel, ref level);
				sandersBlock.TryGetValue(TrainXMLKey.ApplicationTime, ref applicationTime);
				sandersBlock.TryGetValue(TrainXMLKey.ActivationTime, ref activationTime);
				sandersBlock.TryGetValue(TrainXMLKey.NumberOfShots, ref shots);
				Train.Cars[Car].ReAdhesionDevice = new Sanders(Train.Cars[Car], type)
				{
					ApplicationTime = applicationTime,
					ActivationTime = activationTime,
					SandLevel = level,
					SandingRate = rate,
					NumberOfShots = shots
				};
			}
			else
			{
				if (block.GetEnumValue(TrainXMLKey.ReadhesionDevice, out ReadhesionDeviceType readhesionDevice) || Train.Cars[Car].ReAdhesionDevice == null)
				{
					Train.Cars[Car].ReAdhesionDevice = new BveReAdhesionDevice(Train.Cars[Car], readhesionDevice);
				}
			}

			if (block.ReadBlock(TrainXMLSection.DriverSupervisionDevice, out Block<TrainXMLSection, TrainXMLKey> driverSupervisionDeviceBlock))
			{
				double alarmTime = 0;
				double interventionTime = 0;
				double requiredStopTime = 0;
				bool loopingAlarm = false, loopingAlert = false;
				driverSupervisionDeviceBlock.TryGetValue(TrainXMLKey.AlarmTime, ref alarmTime);
				driverSupervisionDeviceBlock.TryGetValue(TrainXMLKey.InterventionTime, ref interventionTime);
				driverSupervisionDeviceBlock.GetEnumValue(TrainXMLKey.Type, out SafetySystemType driverSupervisionType);
				driverSupervisionDeviceBlock.TryGetValue(TrainXMLKey.RequiredStopTime, ref requiredStopTime);
				driverSupervisionDeviceBlock.TryGetValue(TrainXMLKey.LoopingAlert, ref loopingAlert);
				driverSupervisionDeviceBlock.TryGetValue(TrainXMLKey.LoopingAlarm, ref loopingAlarm);
				driverSupervisionDeviceBlock.GetEnumValue(TrainXMLKey.Mode, out DriverSupervisionDeviceMode driverSupervisionMode);
				driverSupervisionDeviceBlock.GetEnumValue(TrainXMLKey.TriggerMode, out SafetySystemTriggerMode triggerMode);
				if (alarmTime == 0)
				{
					alarmTime = interventionTime;
				}

				DriverSupervisionDevice dsd = new DriverSupervisionDevice(Train.Cars[Car], driverSupervisionType, driverSupervisionMode, triggerMode, alarmTime, interventionTime, requiredStopTime)
				{
					LoopingAlarm = loopingAlarm,
					LoopingAlert = loopingAlert
				};
				Train.Cars[Car].SafetySystems.Add(SafetySystem.DriverSupervisionDevice, dsd);
			}
			

			if (block.ReadBlock(TrainXMLSection.Brake, out Block<TrainXMLSection, TrainXMLKey> brakeBlock))
			{
				ParseBrakeNode(brakeBlock, fileName, Car, ref Train);
			}

			if (block.ReadBlock(TrainXMLSection.DieselEngine, out Block<TrainXMLSection, TrainXMLKey> dieselEngineBlock))
			{
				double rpmChangeUpRate = 0, rpmChangeDownRate = 0;
				dieselEngineBlock.GetValue(TrainXMLKey.IdleRPM, out double idleRPM, NumberRange.NonNegative);
				dieselEngineBlock.GetValue(TrainXMLKey.MinRPM, out double minRPM, NumberRange.NonNegative);
				dieselEngineBlock.GetValue(TrainXMLKey.MaxRPM, out double maxRPM, NumberRange.NonNegative);
				if (dieselEngineBlock.GetValue(TrainXMLKey.RPMChangeRate, out double rpmChangeRate, NumberRange.NonNegative))
				{
					rpmChangeUpRate = rpmChangeRate;
					rpmChangeDownRate = rpmChangeRate;
				}

				dieselEngineBlock.TryGetValue(TrainXMLKey.RPMChangeUpRate, ref rpmChangeUpRate, NumberRange.NonNegative);
				dieselEngineBlock.TryGetValue(TrainXMLKey.RPMChangeDownRate, ref rpmChangeDownRate, NumberRange.NonNegative);
				dieselEngineBlock.GetValue(TrainXMLKey.IdleFuelUse, out double idleFuelUse, NumberRange.NonNegative);
				dieselEngineBlock.GetValue(TrainXMLKey.MaxPowerFuelUse, out double maxPowerFuelUse, NumberRange.NonNegative);
				dieselEngineBlock.GetValue(TrainXMLKey.FuelCapacity, out double fuelCapacity, NumberRange.NonNegative);
				Train.Cars[Car].TractionModel = new DieselEngine(Train.Cars[Car], Plugin.AccelerationCurves, idleRPM, minRPM, maxRPM, rpmChangeUpRate, rpmChangeDownRate, idleFuelUse, maxPowerFuelUse);
				Train.Cars[Car].TractionModel.FuelTank = new FuelTank(fuelCapacity, 0, fuelCapacity);
				if (dieselEngineBlock.ReadBlock(new[] { TrainXMLSection.TractionMotor, TrainXMLSection.RegenerativeTractionMotor }, out Block<TrainXMLSection, TrainXMLKey> tractionMotorBlock))
				{
					tractionMotorBlock.GetValue(TrainXMLKey.MaxAmps, out double maxAmps, NumberRange.NonNegative);
					tractionMotorBlock.GetValue(TrainXMLKey.MaxRegenAmps, out double maxRegenAmps, NumberRange.NonNegative);
					if (tractionMotorBlock.Key == TrainXMLSection.TractionMotor)
					{
						Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(Train.Cars[Car].TractionModel, maxAmps));
					}
					else
					{
						Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new RegenerativeTractionMotor(Train.Cars[Car].TractionModel, maxAmps, maxRegenAmps));
					}
				}

			}
			else if (block.ReadBlock(TrainXMLSection.ElectricEngine, out Block<TrainXMLSection, TrainXMLKey> electricEngineBlock))
			{
				Train.Cars[Car].TractionModel = new ElectricEngine(Train.Cars[Car], Plugin.AccelerationCurves);
				if (electricEngineBlock.GetValue(TrainXMLKey.Pantograph, out bool hasPantograph) && hasPantograph)
				{
					Train.Cars[Car].TractionModel.Components.Add(EngineComponent.Pantograph, new Pantograph(Train.Cars[Car].TractionModel));
				}
				if (electricEngineBlock.ReadBlock(new[] { TrainXMLSection.TractionMotor, TrainXMLSection.RegenerativeTractionMotor }, out Block<TrainXMLSection, TrainXMLKey> tractionMotorBlock))
				{
					tractionMotorBlock.GetValue(TrainXMLKey.MaxAmps, out double maxAmps, NumberRange.NonNegative);
					tractionMotorBlock.GetValue(TrainXMLKey.MaxRegenAmps, out double maxRegenAmps, NumberRange.NonNegative);
					if (tractionMotorBlock.Key == TrainXMLSection.TractionMotor)
					{
						Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(Train.Cars[Car].TractionModel, maxAmps));
					}
					else
					{
						Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new RegenerativeTractionMotor(Train.Cars[Car].TractionModel, maxAmps, maxRegenAmps));
					}
				}
			}
			else if (block.GetValue(TrainXMLKey.MotorCar, out bool motorCar) && motorCar == true)
			{
				// initial cloned curves from train.dat, may be overwritten
				AccelerationCurve[] finalAccelerationCurves = new AccelerationCurve[Plugin.AccelerationCurves.Length];
				for (int i = 0; i < Plugin.AccelerationCurves.Length; i++)
				{
					finalAccelerationCurves[i] = Plugin.AccelerationCurves[i].Clone(1.0);
				}
				
				AbstractMotorSound motor = Train.Cars[Car].TractionModel.MotorSounds;
				if (block.ReadBlock(TrainXMLSection.Power, out Block<TrainXMLSection, TrainXMLKey> powerBlock))
				{
					if (powerBlock.ReadBlock(TrainXMLSection.Handle, out Block<TrainXMLSection, TrainXMLKey> powerHandleBlock))
					{
						AbstractHandle p = Train.Handles.Power; // yuck, but we can't store this as the base type due to constraints elsewhere
						ParseHandleNode(powerHandleBlock, ref p, Car, Train, fileName);
					}
					if (powerBlock.ReadBlock(TrainXMLSection.AccelerationCurves, out Block<TrainXMLSection, TrainXMLKey> powerCurvesBlock))
					{
						finalAccelerationCurves = ParseAccelerationBlock(powerCurvesBlock, fileName);
					}
				}

				if (block.ReadBlock(TrainXMLSection.AccelerationCurves, out Block<TrainXMLSection, TrainXMLKey> accelerationCurvesBlock))
				{
					// NOTE: AccelerationCurves were originally at /openBVE/Train/Car/AccelerationCurves. Moved to be a child of the Power node, but retaining this for backwards compatability
					finalAccelerationCurves = ParseAccelerationBlock(accelerationCurvesBlock, fileName);
				}
				
				Train.Cars[Car].TractionModel = new BVEMotorCar(Train.Cars[Car], finalAccelerationCurves);
				Train.Cars[Car].TractionModel.MaximumPossibleAcceleration = Plugin.MaximumAcceleration;
				Train.Cars[Car].TractionModel.MotorSounds = motor;
			}
			else
			{
				Train.Cars[Car].TractionModel = new BVETrailerCar(Train.Cars[Car]);
			}

			if (block.ReadBlock(TrainXMLSection.Doors, out Block<TrainXMLSection, TrainXMLKey> doorsBlock))
			{
				double doorWidth = 1.0;
				double doorTolerance = 0.0;
				if (doorsBlock.GetValue(TrainXMLKey.OpenSpeed, out double openSpeed, NumberRange.Positive))
				{
					Train.Cars[Car].Specs.DoorOpenFrequency = 1.0 / openSpeed;
				}
				if (doorsBlock.GetValue(TrainXMLKey.CloseSpeed, out double closeSpeed, NumberRange.Positive))
				{
					Train.Cars[Car].Specs.DoorCloseFrequency = 1.0 / closeSpeed;
				}

				doorsBlock.TryGetValue(TrainXMLKey.Width, ref doorWidth, NumberRange.Positive);
				doorsBlock.TryGetValue(TrainXMLKey.Tolerance, ref doorTolerance, NumberRange.Positive);
				// XML uses meters for all units to be consistant, so convert to mm for door usage
				doorWidth *= 1000.0;
				doorTolerance *= 1000.0;
				Train.Cars[Car].Doors[0] = new Door(-1, doorWidth, doorTolerance);
				Train.Cars[Car].Doors[1] = new Door(1, doorWidth, doorTolerance);
			}

			block.GetEnumValue(TrainXMLKey.Cargo, out CargoType cargoType);
			switch (cargoType)
			{
				case CargoType.Passengers:
					Train.Cars[Car].Cargo = new Passengers(Train.Cars[Car]);
					break;
				case CargoType.Freight:
					Train.Cars[Car].Cargo = new RobustFreight(Train.Cars[Car]);
					break;
				case CargoType.None:
					Train.Cars[Car].Cargo = new EmptyLoad();
					break;

			}

			if (block.ReadBlock(TrainXMLSection.Windscreen, out Block<TrainXMLSection, TrainXMLKey> windscreenBlock) && Train.IsPlayerTrain)
			{
				if (windscreenBlock.GetValue(TrainXMLKey.NumberOfDrops, out int numDrops, NumberRange.NonNegative))
				{
					double wipeSpeed = 1.0, holdTime = 1.0, dropLife = 10.0;
					windscreenBlock.TryGetValue(TrainXMLKey.WipeSpeed, ref wipeSpeed, NumberRange.Positive);
					windscreenBlock.TryGetValue(TrainXMLKey.HoldTime, ref holdTime, NumberRange.NonNegative);
					windscreenBlock.TryGetValue(TrainXMLKey.DropLife, ref dropLife, NumberRange.Positive);
					windscreenBlock.GetEnumValue(TrainXMLKey.RestPosition, out WiperPosition restPosition);
					windscreenBlock.GetEnumValue(TrainXMLKey.HoldPosition, out WiperPosition holdPosition);
					Train.Cars[Car].Windscreen = new Windscreen(numDrops, dropLife, Train.Cars[Car]);
					Train.Cars[Car].Windscreen.Wipers = new WindscreenWiper(Train.Cars[Car].Windscreen, restPosition, holdPosition, wipeSpeed, holdTime);
				}
				
			}

			if(block.ReadBlock(TrainXMLSection.ParticleSource, out Block<TrainXMLSection, TrainXMLKey> particleSourceBlock))
			{
				Vector3 initialMotion = Vector3.Down;
				Texture particleTexture = null;
				double maximumSize = 0.2;
				double maximumGrownSize = 1.0;
				double maximumLifeSpan = 15;
				particleSourceBlock.GetVector3(TrainXMLKey.Location, ',', out Vector3 emitterLocation);
				particleSourceBlock.TryGetValue(TrainXMLKey.MaximumSize, ref maximumSize, NumberRange.NonNegative);
				particleSourceBlock.TryGetValue(TrainXMLKey.MaximumGrownSize, ref maximumGrownSize, NumberRange.NonZero);
				particleSourceBlock.TryGetValue(TrainXMLKey.MaximumLifespan, ref maximumLifeSpan, NumberRange.Positive);
				particleSourceBlock.TryGetVector3(TrainXMLKey.InitialDirection, ',', ref initialMotion);
				if (particleSourceBlock.GetPath(TrainXMLKey.Texture, currentXMLPath, out string texturePath))
				{
					Plugin.CurrentHost.RegisterTexture(texturePath, TextureParameters.NoChange, out particleTexture);
				}
				ParticleSource particleSource = new ParticleSource(Plugin.Renderer, Train.Cars[Car], emitterLocation, maximumSize, maximumGrownSize, initialMotion, maximumLifeSpan);
				particleSourceBlock.TryGetValue(TrainXMLKey.EmitsAtIdle, ref particleSource.EmitsAtIdle);
				particleSource.ParticleTexture = particleTexture;
				Train.Cars[Car].ParticleSources.Add(particleSource);
			}

			if (block.ReadBlock(TrainXMLSection.SoundTable, out Block<TrainXMLSection, TrainXMLKey> soundTableBlock))
			{
				if (soundTableBlock.ReadBlock(TrainXMLSection.BVE5, out Block<TrainXMLSection, TrainXMLKey> bve5Block))
				{
					bve5Block.GetPath(TrainXMLKey.PowerFreq, currentXMLPath, out string powerFreq);
					bve5Block.GetPath(TrainXMLKey.PowerVol, currentXMLPath, out string powerVol);
					bve5Block.GetPath(TrainXMLKey.BrakeFreq, currentXMLPath, out string brakeFreq);
					bve5Block.GetPath(TrainXMLKey.BrakeVol, currentXMLPath, out string brakeVol);
					Train.Cars[Car].TractionModel.MotorSounds = Bve5MotorSoundTableParser.Parse(Train.Cars[Car], powerFreq, powerVol, brakeFreq, brakeVol);
				}
			}
			
			if (!Train.Cars[Car].TractionModel.ProvidesPower && Train.Cars[Car].ParticleSources.Count > 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Car " + Car + " has a particle source assigned, but is not a motor car in XML file " + fileName);
			}

			//Set toppling angle and exposed areas
			Train.Cars[Car].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[Car].Specs.CenterOfGravityHeight / Train.Cars[Car].Width);
			
		}
	}
}
