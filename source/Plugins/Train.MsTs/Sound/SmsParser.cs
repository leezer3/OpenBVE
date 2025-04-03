using OpenBve.Formats.MsTs;
using SharpCompress.Compressors.Deflate;
using System.IO;
using System;
using System.Text;
using OpenBveApi.Interface;
using SharpCompress.Compressors;
using TrainManager.Car;
using SoundManager;

namespace Train.MsTs
{
	class SoundModelSystemParser
	{
		private static string currentFolder;

		private static string currentFile;
		
		internal static bool ParseSoundFile(string fileName, ref CarBase Car)
		{
			currentFile = fileName;
			currentFolder = Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);

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

			// SIMISA@F  means compressed
			// SIMISA@@  means uncompressed
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
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognized SMS file header " + headerString + " in " + fileName);
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
			if (subHeader[7] == 't')
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					byte[] newBytes = reader.ReadBytes((int)(fb.Length - fb.Position));
					string s = unicode ? Encoding.Unicode.GetString(newBytes) : Encoding.ASCII.GetString(newBytes);
					TextualBlock block = new TextualBlock(s, KujuTokenID.Tr_SMS);
					ParseBlock(block, ref soundSet, ref Car);
				}

			}
			else if (subHeader[7] != 'b')
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognized subHeader " + subHeader + " in " + fileName);
				return false;
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
					ParseBlock(block, ref soundSet, ref Car);
				}
			}

			return false;
		}


		internal struct SoundSet
		{
			internal bool Activation;
			internal double ActivationDistance;
			internal double DeactivationDistance;
			internal bool CamCam;
			internal bool PassengerCam;
			internal bool ExternalCam;
			internal double Priority;
			
		}
		
		private static SoundTrigger currentTrigger;
		private static KujuTokenID currentSoundType;

		private static void ParseBlock(Block block, ref SoundSet currentSoundSet, ref CarBase car)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.Tr_SMS:
					// file root
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock();
						ParseBlock(newBlock, ref currentSoundSet, ref car);
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
							ParseBlock(newBlock, ref currentSoundSet, ref car);
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
						ParseBlock(newBlock, ref currentSoundSet, ref car);
					}
					break;
				case KujuTokenID.Deactivation:
					// control the conditions under which the sounds in the group are deactivated
					currentSoundSet.Activation = false;
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentSoundSet, ref car);
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
				case KujuTokenID.CabCam:
				case KujuTokenID.PassengerCam:
					currentSoundSet.ExternalCam = currentSoundSet.Activation;
					break;
				case KujuTokenID.Streams:
					// each stream represents a unique sound
					int numStreams = block.ReadInt32();
					for (int i = 0; i < numStreams; i++)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.Stream, KujuTokenID.Skip });
						if (block.Token == KujuTokenID.Skip)
						{
							i--;
						}
						else
						{
							ParseBlock(newBlock, ref currentSoundSet, ref car);
						}
						if (block.Length() - block.Position() <= 3)
						{
							// WARN: incorrect number of streams supplied
							break;
						}
					}
					break;
				case KujuTokenID.Stream:
					while (block.Position() < block.Length() - 3)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.Priority, KujuTokenID.Triggers, KujuTokenID.Volume, KujuTokenID.VolumeCurve, KujuTokenID.FrequencyCurve, KujuTokenID.Granularity });
						ParseBlock(newBlock, ref currentSoundSet, ref car);
					}
					break;
				case KujuTokenID.Priority:
					currentSoundSet.Priority = block.ReadSingle();
					break;
				case KujuTokenID.Triggers:
					int numTriggers = block.ReadInt32();
					for (int i = 0; i < numTriggers; i++)
					{
						// two triggers per sound set  (start + stop)
						newBlock = block.ReadSubBlock(new [] {KujuTokenID.Variable_Trigger, KujuTokenID.Initial_Trigger, KujuTokenID.Discrete_Trigger, KujuTokenID.Random_Trigger, KujuTokenID.Dist_Travelled_Trigger});
						ParseBlock(newBlock, ref currentSoundSet, ref car);
						if (block.Length() - block.Position() <= 3)
						{
							// WARN: incorrect number of triggers supplied
							break;
						}
					}
					break;
				case KujuTokenID.Initial_Trigger:
					// when initially appears, hence nothing other than StartLoop should be valid
					newBlock = block.ReadSubBlock(new[] { KujuTokenID.StartLoop, KujuTokenID.DisableTrigger });
					ParseBlock(newBlock, ref currentSoundSet, ref car);
					break;
				case KujuTokenID.StartLoopRelease:
				case KujuTokenID.StartLoop:
					/* StartLoopRelease - Loop stops when key is released
					 * StartLoop - Loop continues when key is released
					 * ---------------------------------------------------
					 * NOTE: Handle these on a per-sound trigger, as where possible
					 * map to existing subsystems
					 */
					currentSoundType = block.Token;
					numStreams = block.ReadInt32();
					for (int i = 0; i < numStreams; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.File);
						ParseBlock(newBlock, ref currentSoundSet, ref car);
					}
					newBlock = block.ReadSubBlock(KujuTokenID.SelectionMethod);
					ParseBlock(newBlock, ref currentSoundSet, ref car);
					break;
				case KujuTokenID.ReleaseLoopRelease:
					// empty block expected
					// appear to be paired with StartLoopRelease
					break;
				case KujuTokenID.File:
					if (block.ReadPath(currentFolder, out string soundFile))
					{
						// n.b. MSTS does not distinguish between increase / decrease sounds for handles etc.
						switch (currentTrigger)
						{
							case SoundTrigger.ReverserChange:
								if (currentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Reverser.EngageSound = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Reverser.ReleaseSound = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.ThrottleChange:
								if (currentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Power.Decrease = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.DecreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Increase = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.IncreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Min = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Power.Max = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.TrainBrakeChange:
								if (currentSoundType == KujuTokenID.PlayOneShot)
								{
									car.baseTrain.Handles.Brake.Decrease = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.DecreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Increase = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.IncreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Min = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.Brake.Max = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.EngineBrakeChange:
								if (currentSoundType == KujuTokenID.PlayOneShot && car.baseTrain.Handles.LocoBrake != null)
								{
									car.baseTrain.Handles.LocoBrake.Decrease = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.DecreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Increase = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.IncreaseFast = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Min = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
									car.baseTrain.Handles.LocoBrake.Max = new CarSound(Plugin.currentHost, soundFile, 2.0, car.Driver);
								}
								break;
							case SoundTrigger.LightSwitchToggle:
								if (currentSoundType == KujuTokenID.PlayOneShot && car.baseTrain.SafetySystems.Headlights != null)
								{
									Plugin.currentHost.RegisterSound(soundFile, 2.0, out var soundHandle);
									car.baseTrain.SafetySystems.Headlights.SwitchSoundBuffer = soundHandle as SoundBuffer;
								}
								break;
							case SoundTrigger.HornOn:
								if (currentSoundType == KujuTokenID.StartLoopRelease && car.Horns[0] != null)
								{
									Plugin.currentHost.RegisterSound(soundFile, 2.0, out var soundHandle);
									car.Horns[0].LoopSound = soundHandle as SoundBuffer;
								}
								break;
							case SoundTrigger.BellOn:
								if (currentSoundType == KujuTokenID.StartLoopRelease && car.Horns[2] != null)
								{
									Plugin.currentHost.RegisterSound(soundFile, 2.0, out var soundHandle);
									car.Horns[0].LoopSound = soundHandle as SoundBuffer;
								}
								break;
						}
					}
					else
					{
						Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Sound File " + soundFile + " was not found in SMS " + currentFile);
					}
					int checkDigit = block.ReadInt32();
					if (checkDigit != -1)
					{
						// Unknown purpose at the minute- set to -1 everywhere
						throw new Exception();
					}
					
					break;
				case KujuTokenID.SelectionMethod:
					KujuTokenID token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.SequentialSelection:
							break;
						case KujuTokenID.RandomSelection:
							break;
					}
					break;
				case KujuTokenID.Discrete_Trigger:
					currentTrigger = (SoundTrigger)block.ReadInt32(); // stored as integer
					newBlock = block.ReadSubBlock(new[] { KujuTokenID.PlayOneShot, KujuTokenID.StartLoopRelease, KujuTokenID.ReleaseLoopRelease, KujuTokenID.ReleaseLoopReleaseWithJump, KujuTokenID.SetStreamVolume });
					ParseBlock(newBlock, ref currentSoundSet, ref car);
					break;
				case KujuTokenID.Variable_Trigger:
					token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.StartLoop:
							break;
						case KujuTokenID.Speed_Inc_Past:
							double speedValue = block.ReadSingle();
							break;
						case KujuTokenID.Speed_Dec_Past:
							speedValue = block.ReadSingle();
							break;
						case KujuTokenID.SpeedControlled:
							break;
						case KujuTokenID.DistanceControlled:
						case KujuTokenID.Distance_Inc_Past:
						case KujuTokenID.Distance_Dec_Past:
							break;
						case KujuTokenID.Variable1_Inc_Past:
						case KujuTokenID.Variable1_Dec_Past:
						case KujuTokenID.Variable1Controlled:
							break;
						case KujuTokenID.Variable2_Inc_Past:
						case KujuTokenID.Variable2_Dec_Past:
						case KujuTokenID.Variable2Controlled:
							break;
						case KujuTokenID.Variable3_Inc_Past:
						case KujuTokenID.Variable3_Dec_Past:
						case KujuTokenID.Variable3Controlled:
							break;
						default:
							throw new Exception("Unexpected enum value " + token + " encounted in SMS file " + currentFile);
					}
					break;
				case KujuTokenID.PlayOneShot:
					currentSoundType = block.Token;
					int numSounds = block.ReadInt16();
					for (int i = 0; i < numSounds; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.File);
						ParseBlock(newBlock, ref currentSoundSet, ref car);
					}
					break;
				case KujuTokenID.Volume:
					double volume = block.ReadSingle();
					break;
				case KujuTokenID.VolumeCurve:
					token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.SpeedControlled:
							newBlock = block.ReadSubBlock(KujuTokenID.CurvePoints);
							ParseBlock(newBlock, ref currentSoundSet, ref car);
							break;
						case KujuTokenID.DistanceControlled:
							break;
						case KujuTokenID.Variable1Controlled:
						case KujuTokenID.Variable2Controlled:
						case KujuTokenID.Variable3Controlled:
							break;
						default:
							throw new Exception("Unexpected enum value " + token + " encounted in SMS file " + currentFile);
					}
					break;
				case KujuTokenID.CurvePoints:
					int numPoints = block.ReadInt32();
					for (int i = 0; i < numPoints; i++)
					{
						double refenceValue = block.ReadSingle();
						double volumeValue = block.ReadSingle();
					}
					break;
				case KujuTokenID.Granularity:
					// presuming this is the step in km/h
					break;
			}
		}
	}
}
