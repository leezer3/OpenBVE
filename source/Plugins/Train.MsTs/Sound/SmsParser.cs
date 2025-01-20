using OpenBve.Formats.MsTs;
using SharpCompress.Compressors.Deflate;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using OpenBveApi.Interface;
using OpenBveApi.Sounds;
using SharpCompress.Compressors;
using TrainManager.Car;

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
					string s;
					if (unicode)
					{
						s = Encoding.Unicode.GetString(newBytes);
					}
					else
					{
						s = Encoding.ASCII.GetString(newBytes);
					}

					TextualBlock block = new TextualBlock(s, KujuTokenID.Tr_SMS);
					ParseBlock(block, ref soundSet);
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
					ParseBlock(block, ref soundSet);
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
		

		private static void ParseBlock(Block block, ref SoundSet currentSoundSet)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.Tr_SMS:
					// file root
					while (block.Position() < block.Length())
					{
						newBlock = block.ReadSubBlock();
						ParseBlock(newBlock, ref currentSoundSet);
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
							ParseBlock(newBlock, ref currentSoundSet);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							throw;
						}
						
						int b = 0;
					}
					break;
				case KujuTokenID.Activation:
					// control the conditions under which the sounds in the group are activated
					currentSoundSet.Activation = true;
					while (block.Position() < block.Length())
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentSoundSet);
					}
					break;
				case KujuTokenID.Deactivation:
					// control the conditions under which the sounds in the group are deactivated
					currentSoundSet.Activation = false;
					while (block.Position() < block.Length() - 1)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentSoundSet);
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
							ParseBlock(newBlock, ref currentSoundSet);
						}
					}
					break;
				case KujuTokenID.Stream:
					while (block.Position() < block.Length() - 1)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.Priority, KujuTokenID.Triggers, KujuTokenID.VolumeCurve, KujuTokenID.Granularity });
						ParseBlock(newBlock, ref currentSoundSet);
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
						newBlock = block.ReadSubBlock(new [] {KujuTokenID.Variable_Trigger, KujuTokenID.Initial_Trigger});
						ParseBlock(newBlock, ref currentSoundSet);
					}
					break;
				case KujuTokenID.Initial_Trigger:
					// when initially appears, hence nothing other than StartLoop should be valid
					newBlock = block.ReadSubBlock(KujuTokenID.StartLoop);
					ParseBlock(newBlock, ref currentSoundSet);
					break;
				case KujuTokenID.StartLoop:
					numStreams = block.ReadInt32();
					for (int i = 0; i < numStreams; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.File);
						ParseBlock(newBlock, ref currentSoundSet);
						newBlock = block.ReadSubBlock(KujuTokenID.SelectionMethod);
						ParseBlock(newBlock, ref currentSoundSet);
					}
					break;
				case KujuTokenID.File:
					string soundFile = block.ReadString();
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
					block.ReadInt32();
					// 15 - reverserf
					// 16 - reverserb
					// 17 - brake+
					// 18 - brake-
					// 32 - damper
					// 33 - blowers
					// 34 - open cylinder cocks
					// 36 - open firebox
					// 44 - heat (shovel sound??)
					token = block.ReadEnumValue(default(KujuTokenID));
					if (token != KujuTokenID.PlayOneShot)
					{
						throw new Exception("Unexpected enum value " + token + " encounted in SMS file " + currentFile);
					}
					numStreams = block.ReadInt32();
					for (int i = 0; i < numStreams; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.File);
						ParseBlock(newBlock, ref currentSoundSet);
						newBlock = block.ReadSubBlock(KujuTokenID.SelectionMethod);
						ParseBlock(newBlock, ref currentSoundSet);
					}
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
						default:
							throw new Exception("Unexpected enum value " + token + " encounted in SMS file " + currentFile);

					}
					break;
				case KujuTokenID.VolumeCurve:
					token = block.ReadEnumValue(default(KujuTokenID));
					switch (token)
					{
						case KujuTokenID.SpeedControlled:
							newBlock = block.ReadSubBlock(KujuTokenID.CurvePoints);
							ParseBlock(newBlock, ref currentSoundSet);
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
