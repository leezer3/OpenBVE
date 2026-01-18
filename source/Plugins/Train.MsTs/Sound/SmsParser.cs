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

using OpenBve.Formats.MsTs;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Runtime;
using OpenBveApi.World;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SoundManager;
using System;
using System.IO;
using System.Text;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Motor;
using TrainManager.MsTsSounds;
using TrainManager.SafetySystems;
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

namespace Train.MsTs
{
	internal class SoundModelSystemParser
	{
		private static string currentFolder;

		private static string currentFile;

		private static Tuple<double, double>[] curvePoints;
		
		internal static bool ParseSoundFile(string fileName, ref CarBase currentCar)
		{
			currentFile = fileName;
			currentFolder = System.IO.Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = buffer[0] == 0xFF && buffer[1] == 0xFE;

			string headerString;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				headerString = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 2, 14);
				headerString = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			// SIMISA@F means compressed
			// SIMISA@@ means uncompressed
			if (headerString.StartsWith("SIMISA@F"))
			{
				fb = new ZlibStream(fb, CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				Console.Error.WriteLine("Improper header in " + fileName);
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognized SMS header " + headerString + " in " + fileName);
				return false;
			}

			string subHeader;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				subHeader = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 0, 16);
				subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
			}
			SoundSet soundSet = new SoundSet();
			SoundStream soundStream = new SoundStream(currentCar, CameraViewMode.NotDefined, CameraViewMode.NotDefined);

			if (subHeader[7] == 't')
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					byte[] newBytes = reader.ReadBytes((int)(fb.Length - fb.Position));
					string s = unicode ? Encoding.Unicode.GetString(newBytes) : Encoding.ASCII.GetString(newBytes);
					TextualBlock block = new TextualBlock(s, KujuTokenID.Tr_SMS);
					ParseBlock(block, ref soundSet, ref soundStream, ref currentCar);
				}

			}
			else if (subHeader[7] != 'b')
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognized subHeader " + subHeader + " in " + fileName);
			}
			else
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					KujuTokenID currentToken = (KujuTokenID)reader.ReadUInt16();
					if (currentToken != KujuTokenID.Tr_SMS)
					{
						return false;
					}

					reader.ReadUInt16();
					uint remainingBytes = reader.ReadUInt32();
					byte[] newBytes = reader.ReadBytes((int)remainingBytes);
					BinaryBlock block = new BinaryBlock(newBytes, KujuTokenID.Tr_SMS);
					ParseBlock(block, ref soundSet, ref soundStream, ref currentCar);
				}
			}

			return false;
		}


		internal struct SoundSet
		{
			internal bool Activation;
			internal double ActivationDistance;
			internal double DeactivationDistance;
			internal double Priority;

			internal SoundTrigger CurrentTrigger;
			internal KujuTokenID CurrentSoundType;
			internal KujuTokenID VariableTriggerType;
			internal double VariableValue;
			internal SoundBuffer[] SoundBuffers;
			internal int CurrentBuffer;
			internal KujuTokenID SelectionMethod;
			internal CameraViewMode ActivationCameraModes;
			internal CameraViewMode DeactivationCameraModes;

			internal void Create(CarBase car, SoundStream currentSoundStream)
			{
				switch (VariableTriggerType)
				{
					case KujuTokenID.Initial_Trigger:
						currentSoundStream.Triggers.Add(new InitialTrigger(SoundBuffers, SelectionMethod, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Speed_Inc_Past:
						currentSoundStream.Triggers.Add(new SpeedIncPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Speed_Dec_Past:
						currentSoundStream.Triggers.Add(new SpeedDecPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Variable1_Inc_Past:
						if (VariableValue > 0)
						{
							// https://www.elvastower.com/forums/index.php?/topic/34789-volumecurve-variable1controlled-in-a-chuff-stream/#entry267315
							// should be positive (Check if this is actually an OR bug; e.g. GWR81xx\sound\fscotcab.sms uses negative Variable1 values)
							break;
						}
						currentSoundStream.Triggers.Add(new Variable1IncPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Variable1_Dec_Past:
						if (VariableValue > 0)
						{
							// see above
							break;
						}
						currentSoundStream.Triggers.Add(new Variable1DecPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Variable2_Inc_Past:
						currentSoundStream.Triggers.Add(new Variable2IncPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
					case KujuTokenID.Variable2_Dec_Past:
						currentSoundStream.Triggers.Add(new Variable2DecPast(SoundBuffers, SelectionMethod, VariableValue, CurrentSoundType != KujuTokenID.PlayOneShot));
						break;
				}
			}
		}
		
		

		private static void ParseBlock(Block block, ref SoundSet currentSoundSet, ref SoundStream currentSoundStream, ref CarBase car)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.Tr_SMS:
					// file root
					while (block.Position() < block.Length() - 3)
					{
						try
						{
							newBlock = block.ReadSubBlock(true);
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
						}
						catch
						{
							break;
						}
					}
					break;
				case KujuTokenID.ScalabiltyGroup:
					// root container for sound groups
					block.ReadSingle(); // number of groups
					while (block.Position() < block.Length())
					{
						try
						{
							newBlock = block.ReadSubBlock(true);
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							throw;
						}
					}
					break;
				case KujuTokenID.Activation:
					// control the conditions under which the sounds in the group are activated
					currentSoundSet.Activation = true;
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					}
					break;
				case KujuTokenID.Deactivation:
					// control the conditions under which the sounds in the group are deactivated
					currentSoundSet.Activation = false;
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					}
					break;
				case KujuTokenID.Distance:
					// absolute distance, presumably to camera
					if (currentSoundSet.Activation)
					{
						currentSoundSet.ActivationDistance = block.ReadSingle();
					}
					else
					{
						currentSoundSet.DeactivationDistance = block.ReadSingle();
					}
					break;
				case KujuTokenID.ExternalCam:
					if (currentSoundSet.Activation)
					{
						currentSoundSet.ActivationCameraModes |= CameraViewMode.Exterior;
						currentSoundSet.ActivationCameraModes |= CameraViewMode.Track;
						currentSoundSet.ActivationCameraModes |= CameraViewMode.FlyBy;
						currentSoundSet.ActivationCameraModes |= CameraViewMode.FlyByZooming;
					}
					else
					{
						currentSoundSet.DeactivationCameraModes |= CameraViewMode.Exterior;
						currentSoundSet.DeactivationCameraModes |= CameraViewMode.Track;
						currentSoundSet.DeactivationCameraModes |= CameraViewMode.FlyBy;
						currentSoundSet.DeactivationCameraModes |= CameraViewMode.FlyByZooming;
					}
					break;
				case KujuTokenID.CabCam:
					if (currentSoundSet.Activation)
					{
						currentSoundSet.ActivationCameraModes |= CameraViewMode.Interior;
					}
					else
					{
						currentSoundSet.DeactivationCameraModes |= CameraViewMode.Interior;
					}
					break;
				case KujuTokenID.PassengerCam:
					// FIXME: Passenger cam not currently distinguished from interior cam
					break;
				case KujuTokenID.Streams:
					// each stream represents a unique sound
					int numStreams = block.ReadInt32();
					for (int i = 0; i < numStreams; i++)
					{
						newBlock = block.ReadSubBlock();
						if (newBlock.Token != KujuTokenID.Stream)
						{
							i--;
							if (newBlock.Token != KujuTokenID.Skip)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unexpected additional block " + newBlock.Token + " encountered within Streams block in SMS " + currentFile);
							}
							if (block.Length() - block.Position() <= 3)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Expected " + numStreams + ", but only found " + i + " in Streams block in SMS " + currentFile);
								break;
							}
							continue;
						}
						
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
						if (block.Length() - block.Position() <= 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Expected " + numStreams + ", but only found " + i + " in Streams block in SMS " + currentFile);
							break;
						}
					}
					break;
				case KujuTokenID.Stream:
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock();
						if (newBlock.Token == KujuTokenID.Stream)
						{
							// EBPHNWSE121.sms - Completely bugged, copy + paste error (?)
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unexpected Stream found within a Stream block in SMS " + currentFile);
							continue;
						}
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					}

					if (currentSoundStream.Triggers.Count > 0)
					{
						car.Sounds.ControlledSounds.Add(currentSoundStream);
						currentSoundStream = new SoundStream(car, currentSoundSet.ActivationCameraModes, currentSoundSet.DeactivationCameraModes);
					}
					break;
				case KujuTokenID.Priority:
					currentSoundSet.Priority = block.ReadSingle();
					break;
				case KujuTokenID.Triggers:
					int numTriggers = block.ReadInt32();
					for (int i = 0; i < numTriggers; i++)
					{
						// two triggers per sound set (start + stop)
						newBlock = block.ReadSubBlock(new [] {KujuTokenID.Variable_Trigger, KujuTokenID.Initial_Trigger, KujuTokenID.Discrete_Trigger, KujuTokenID.Random_Trigger, KujuTokenID.Dist_Travelled_Trigger});
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
						if (block.Length() - block.Position() <= 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Expected " + numTriggers + ", but only found " + i + " in Triggers block in SMS " + currentFile);
							break;
						}
					}
					break;
				case KujuTokenID.Initial_Trigger:
					// when initially appears, hence nothing other than StartLoop should be valid
					currentSoundSet.VariableTriggerType = KujuTokenID.Initial_Trigger;
					newBlock = block.ReadSubBlock(new[] { KujuTokenID.StartLoop, KujuTokenID.StartLoopRelease, KujuTokenID.ReleaseLoopRelease, KujuTokenID.EnableTrigger, KujuTokenID.DisableTrigger, KujuTokenID.PlayOneShot, KujuTokenID.SetStreamVolume });
					ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					break;
				case KujuTokenID.StartLoopRelease:
				case KujuTokenID.StartLoop:
					/* StartLoopRelease - Loop stops when key is released
					 * StartLoop - Loop continues when key is released
					 * ---------------------------------------------------
					 * NOTE: Handle these on a per-sound trigger, as where possible
					 * map to existing subsystems
					 */
					currentSoundSet.CurrentSoundType = block.Token;
					int numSounds = block.ReadInt16();
					currentSoundSet.SoundBuffers = new SoundBuffer[numSounds];
					currentSoundSet.SelectionMethod = KujuTokenID.SequentialSelection;
					while (numSounds > 0 && block.Position() < block.Length() - 4)
					{
						newBlock = block.ReadSubBlock();
						if (newBlock.Token == KujuTokenID.File)
						{
							numSounds--;
						}
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					}
					currentSoundSet.Create(car, currentSoundStream);
					break;
				case KujuTokenID.ReleaseLoopRelease:
					// empty block expected
					// paired with StartLoopRelease
					currentSoundSet.Create(car, currentSoundStream);
					break;
				case KujuTokenID.File:
					if (block.ReadPath(currentFolder, out string soundFile))
					{
						// n.b. MSTS does not distinguish between increase / decrease sounds for handles etc.
						// sound radii are also fudged based upon BVE values; most MSTS content just seems to use massive radii
						switch (currentSoundSet.CurrentTrigger)
						{
							case SoundTrigger.VariableControlled:
								// hack
								Plugin.CurrentHost.RegisterSound(soundFile, currentSoundSet.ActivationDistance, out SoundHandle soundHandle);
								currentSoundSet.SoundBuffers[currentSoundSet.CurrentBuffer] = soundHandle as SoundBuffer;
								break;
							case SoundTrigger.ReverserToForwardBackward:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Reverser.EngageSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.ReverserToNeutral:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Reverser.ReleaseSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.ReverserChange:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
								{
									// as we don't know the order these may be presented in, check the buffer
									if (car.baseTrain.Handles.Reverser.EngageSound.Buffer == null)
									{
										car.baseTrain.Handles.Reverser.EngageSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									}

									if (car.baseTrain.Handles.Reverser.ReleaseSound.Buffer == null)
									{
										car.baseTrain.Handles.Reverser.ReleaseSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									}
								}
								break;
							case SoundTrigger.ThrottleChange:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Power.Decrease = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.DecreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Increase = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.IncreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Min = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Max = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.TrainBrakeChange:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Brake.Decrease = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.DecreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Increase = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.IncreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Min = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Max = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.EngineBrakeChange:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot && car.baseTrain.Handles.LocoBrake != null)
								{
									car.baseTrain.Handles.LocoBrake.Decrease = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.DecreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Increase = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.IncreaseFast = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Min = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Max = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.LightSwitchToggle:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot && car.baseTrain.SafetySystems.Headlights != null)
								{
									Plugin.CurrentHost.RegisterSound(soundFile, 2.0, out soundHandle);
									car.baseTrain.SafetySystems.Headlights.SwitchSoundBuffer = soundHandle as SoundBuffer;
								}
								break;
							case SoundTrigger.HornOn:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.StartLoopRelease && car.Horns[0] != null)
								{
									Plugin.CurrentHost.RegisterSound(soundFile, 30.0, out soundHandle);
									car.Horns[0].LoopSound = soundHandle as SoundBuffer;
								}
								break;
							case SoundTrigger.BellOn:
								if (currentSoundSet.CurrentSoundType == KujuTokenID.StartLoopRelease && car.Horns[2] != null)
								{
									Plugin.CurrentHost.RegisterSound(soundFile, 30.0, out soundHandle);
									car.Horns[0].LoopSound = soundHandle as SoundBuffer;
								}
								break;
							case SoundTrigger.Pantograph1Up:
							case SoundTrigger.Pantograph1Down:
							case SoundTrigger.Pantograph1Toggle:
								if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph pantograph))
								{
									switch (currentSoundSet.CurrentTrigger)
									{
										case SoundTrigger.Pantograph1Up:
											pantograph.RaiseSound = new CarSound(Plugin.CurrentHost, soundFile, 100, Vector3.Zero);
											break;
										case SoundTrigger.Pantograph1Down:
											pantograph.LowerSound = new CarSound(Plugin.CurrentHost, soundFile, 100, Vector3.Zero);
											break;
										default:
											pantograph.SwitchToggle = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
											break;
									}
								}
								else
								{
									// n.b. A WAG file may link to a model containing pantograph animations, or a SMS with pantograph sounds, but does not need to mention
									//		that it exists, so we may need to add it here.
									Pantograph newPantograph = new Pantograph(car.TractionModel);
									switch (currentSoundSet.CurrentTrigger)
									{
										case SoundTrigger.Pantograph1Up:
											newPantograph.RaiseSound = new CarSound(Plugin.CurrentHost, soundFile, 100, Vector3.Zero);
											break;
										case SoundTrigger.Pantograph1Down:
											newPantograph.LowerSound = new CarSound(Plugin.CurrentHost, soundFile, 100, Vector3.Zero);
											break;
										default:
											newPantograph.SwitchToggle = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
											break;
									}
									car.TractionModel.Components.Add(EngineComponent.Pantograph, newPantograph);
								}
								break;
							case SoundTrigger.WiperOn:
							case SoundTrigger.WiperOff:
								car.Windscreen.Wipers.SwitchSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								break;
							case SoundTrigger.SanderOn:
								if (car.ReAdhesionDevice is Sanders sanders)
								{
									sanders.LoopSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.TrainBrakePressureDecrease:
								if ((currentSoundStream.ActivationCameraModes & CameraViewMode.Interior) != 0)
								{
									car.CarBrake.AirZero = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.VigilanceAlarmOn:
								// NOTE: appears to only apply to DSD, not overspeed
								if (car.SafetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out DriverSupervisionDevice dsd))
								{
									dsd.AlarmSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									dsd.AlertSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.GearUp:
							case SoundTrigger.GearDown:
								if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.Gearbox, out Gearbox gearbox))
								{
									if (currentSoundSet.CurrentTrigger == SoundTrigger.GearUp)
									{
										gearbox.GearUpSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									}
									else
									{
										gearbox.GearDownSound = new CarSound(Plugin.CurrentHost, soundFile, 2.0, car.Driver);
									}
								}
								break;
							case SoundTrigger.CylinderCocksToggle:
								if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.CylinderCocks, out CylinderCocks cylinderCocks))
								{
									if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
									{
										cylinderCocks.OpenSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
										cylinderCocks.CloseSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
									}
									else
									{
										cylinderCocks.LoopSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
									}
								}
								break;
							case SoundTrigger.BlowerChange:
								if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.Blowers, out Blowers blowers))
								{
									if (currentSoundSet.CurrentSoundType == KujuTokenID.PlayOneShot)
									{
										blowers.ActivationSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
										blowers.DeactivationSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
									}
									else
									{
										blowers.LoopSound = new CarSound(Plugin.CurrentHost, soundFile, 20.0, car.Driver);
									}
								}
								break;
						}
					}
					else
					{
						if (currentSoundSet.CurrentTrigger != SoundTrigger.Skip)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "Sound File " + soundFile + " was not found in SMS " + currentFile);
						}
					}
					int checkDigit = block.ReadInt32();
					if (checkDigit != -1)
					{
						// Unknown purpose at the minute- set to -1 everywhere
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound Stream check digit was incorrect in SMS " + currentFile);
					}
					
					break;
				case KujuTokenID.SelectionMethod:
					KujuTokenID token = block.ReadEnumValue(default(KujuTokenID));
					if (token != KujuTokenID.SequentialSelection && token != KujuTokenID.RandomSelection)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected enum value " + token + " for SoundStream Selection Method in SMS " + currentFile);
						break;
					}
					currentSoundSet.SelectionMethod = token;
					break;
				case KujuTokenID.Discrete_Trigger:
					currentSoundSet.CurrentTrigger = (SoundTrigger)block.ReadInt32(); // stored as integer
					newBlock = block.ReadSubBlock(new[] { KujuTokenID.PlayOneShot, KujuTokenID.StartLoop, KujuTokenID.StartLoopRelease, KujuTokenID.ReleaseLoopRelease, KujuTokenID.ReleaseLoopReleaseWithJump, KujuTokenID.SetStreamVolume, KujuTokenID.EnableTrigger, KujuTokenID.DisableTrigger });
					ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					break;
				case KujuTokenID.Variable_Trigger:
					currentSoundSet.VariableTriggerType = block.ReadEnumValue(default(KujuTokenID));
					switch (currentSoundSet.VariableTriggerType)
					{
						case KujuTokenID.StartLoop:
							break;
						case KujuTokenID.Speed_Inc_Past:
						case KujuTokenID.Speed_Dec_Past:
							currentSoundSet.VariableValue = block.ReadSingle(UnitOfVelocity.KilometersPerHour, UnitOfVelocity.MetersPerSecond); // speed in m/s
							newBlock = block.ReadSubBlock(new[] { KujuTokenID.StartLoop, KujuTokenID.StartLoopRelease, KujuTokenID.ReleaseLoopRelease, KujuTokenID.ReleaseLoopReleaseWithJump, KujuTokenID.PlayOneShot, KujuTokenID.EnableTrigger, KujuTokenID.DisableTrigger, KujuTokenID.SetStreamVolume });
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
							break;
						case KujuTokenID.SpeedControlled:
							break;
						case KujuTokenID.DistanceControlled:
							break;
						case KujuTokenID.Distance_Inc_Past:
						case KujuTokenID.Distance_Dec_Past:
							break;
						case KujuTokenID.Variable1Controlled:
							break;
						case KujuTokenID.Variable1_Inc_Past:
						case KujuTokenID.Variable1_Dec_Past:
						case KujuTokenID.Variable2_Inc_Past:
						case KujuTokenID.Variable2_Dec_Past:
							currentSoundSet.VariableValue = block.ReadSingle(); // variable value
							newBlock = block.ReadSubBlock(new[] { KujuTokenID.StartLoop, KujuTokenID.StartLoopRelease, KujuTokenID.ReleaseLoopRelease, KujuTokenID.ReleaseLoopReleaseWithJump, KujuTokenID.EnableTrigger, KujuTokenID.DisableTrigger, KujuTokenID.PlayOneShot, KujuTokenID.SetStreamVolume });
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
							break;
						case KujuTokenID.Variable2Controlled:
							break;
						case KujuTokenID.Variable3_Inc_Past:
						case KujuTokenID.Variable3_Dec_Past:
						case KujuTokenID.Variable3Controlled:
							break;
						default:
							throw new Exception("Unexpected enum value " + currentSoundSet.VariableTriggerType + " encountered in SMS " + currentFile);
					}
					break;
				case KujuTokenID.PlayOneShot:
					currentSoundSet.CurrentSoundType = block.Token;
					numSounds = block.ReadInt16();
					currentSoundSet.SoundBuffers = new SoundBuffer[numSounds];
					currentSoundSet.SelectionMethod = KujuTokenID.SequentialSelection;
					while (numSounds > 0 && block.Position() < block.Length() - 4)
					{
						newBlock = block.ReadSubBlock();
						if (newBlock.Token == KujuTokenID.File)
						{
							numSounds--;
						}
						ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
					}
					currentSoundSet.Create(car, currentSoundStream);
					break;
				case KujuTokenID.Volume:
					currentSoundStream.BaseVolume = block.ReadSingle();
					break;
				case KujuTokenID.VolumeCurve:
					token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.SpeedControlled:
						case KujuTokenID.DistanceControlled:
						case KujuTokenID.Variable1Controlled:
						case KujuTokenID.Variable2Controlled:
						case KujuTokenID.Variable3Controlled:
							newBlock = block.ReadSubBlock(KujuTokenID.CurvePoints);
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
							break;
						default:
							throw new Exception("Unexpected enum value " + token + " encountered in SMS " + currentFile);
					}

					currentSoundStream.VolumeCurve = new MsTsVolumeCurve(car, token, curvePoints);
					break;
				case KujuTokenID.FrequencyCurve:
					token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.SpeedControlled:
						case KujuTokenID.DistanceControlled:
						case KujuTokenID.Variable1Controlled:
						case KujuTokenID.Variable2Controlled:
						case KujuTokenID.Variable3Controlled:
							newBlock = block.ReadSubBlock(KujuTokenID.CurvePoints);
							ParseBlock(newBlock, ref currentSoundSet, ref currentSoundStream, ref car);
							break;
						default:
							throw new Exception("Unexpected enum value " + token + " encountered in SMS " + currentFile);
					}

					currentSoundStream.FrequencyCurve = new MsTsFrequencyCurve(car, token, curvePoints);
					break;
				case KujuTokenID.CurvePoints:
					int numPoints = block.ReadInt32();
					curvePoints = new Tuple<double, double>[numPoints];
					for (int i = 0; i < numPoints; i++)
					{
						// Normalise Variable2 values to be consistent across traction models
						// MSTS yuck...
						if (car.TractionModel is ElectricEngine)
						{
							curvePoints[i] = new Tuple<double, double>(block.ReadSingle() / 100, block.ReadSingle());
						}
						else
						{
							curvePoints[i] = new Tuple<double, double>(block.ReadSingle(), block.ReadSingle());
						}
					}
					break;
				case KujuTokenID.Granularity:
					// presuming this is the step in km/h
					break;
			}
		}
	}
}
