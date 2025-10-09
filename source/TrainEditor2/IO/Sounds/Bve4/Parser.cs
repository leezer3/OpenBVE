using System.Collections.ObjectModel;
using System.Linq;
using Formats.OpenBve;
using TrainEditor2.Models.Sounds;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Sounds.Bve4
{
	internal static partial class SoundCfgBve4
	{
		internal static void Parse(string soundCfgFile, out Sound sound)
		{
			sound = new Sound();
			string trainFolder = Path.GetDirectoryName(soundCfgFile);

			ConfigFile<SoundCfgSection, SoundCfgKey> cfg = new ConfigFile<SoundCfgSection, SoundCfgKey>(soundCfgFile, Program.CurrentHost, "Version 1.0");
			while (cfg.RemainingSubBlocks > 0)
			{
				Block<SoundCfgSection, SoundCfgKey> block = cfg.ReadNextBlock();
				switch (block.Key)
				
				{
					case SoundCfgSection.Run:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var runIndex, out var fileName))
						{
							sound.SoundElements.Add(new RunElement { Key = runIndex, FilePath = Path.CombineFile(trainFolder, fileName) });
						}
						break;
					case SoundCfgSection.Flange:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var runIndex, out var fileName))
						{
							sound.SoundElements.Add(new FlangeElement { Key = runIndex, FilePath = Path.CombineFile(trainFolder, fileName) });
						}
						break;
					case SoundCfgSection.Motor:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var runIndex, out var fileName))
						{
							sound.SoundElements.Add(new MotorElement { Key = runIndex, FilePath = Path.CombineFile(trainFolder, fileName) });
						}
						break;
					case SoundCfgSection.Switch:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var runIndex, out var fileName))
						{
							sound.SoundElements.Add(new FrontSwitchElement { Key = runIndex, FilePath = Path.CombineFile(trainFolder, fileName) });
							sound.SoundElements.Add(new RearSwitchElement { Key = runIndex, FilePath = Path.CombineFile(trainFolder, fileName) });
						}
						break;
					case SoundCfgSection.Brake:
						block.GetPath(SoundCfgKey.BcReleaseHigh, trainFolder, out string bcReleaseHigh);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.BcReleaseHigh, FilePath = bcReleaseHigh });
						block.GetPath(SoundCfgKey.BcRelease, trainFolder, out string bcRelease);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.BcRelease, FilePath = bcRelease });
						block.GetPath(SoundCfgKey.BcReleaseFull, trainFolder, out string bcReleaseFull);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.BcReleaseFull, FilePath = bcReleaseFull });
						block.GetPath(SoundCfgKey.Emergency, trainFolder, out string emergency);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.Emergency, FilePath = emergency });
						block.GetPath(SoundCfgKey.EmergencyRelease, trainFolder, out string emergencyRelease);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.EmergencyRelease, FilePath = emergencyRelease });
						block.GetPath(SoundCfgKey.BpDecomp, trainFolder, out string bpDecomp);
						sound.SoundElements.Add(new BrakeElement { Key = BrakeKey.BpDecomp, FilePath = bpDecomp });
						break;
					case SoundCfgSection.Compressor:
						block.GetPath(SoundCfgKey.Attack, trainFolder, out string attack);
						sound.SoundElements.Add(new CompressorElement { Key = CompressorKey.Attack, FilePath = attack });
						block.GetPath(SoundCfgKey.Loop, trainFolder, out string loop);
						sound.SoundElements.Add(new CompressorElement { Key = CompressorKey.Loop, FilePath = loop });
						block.GetPath(SoundCfgKey.Release, trainFolder, out string release);
						sound.SoundElements.Add(new CompressorElement { Key = CompressorKey.Release, FilePath = release });
						break;
					case SoundCfgSection.Suspension:
						block.GetPath(SoundCfgKey.Left, trainFolder, out string springL);
						sound.SoundElements.Add(new SuspensionElement { Key = SuspensionKey.Left, FilePath = springL });
						block.GetPath(SoundCfgKey.Right, trainFolder, out string springR);
						sound.SoundElements.Add(new SuspensionElement { Key = SuspensionKey.Right, FilePath = springR });
						break;
					case SoundCfgSection.Horn:
						if (block.GetPath(SoundCfgKey.Primary, trainFolder, out string primaryLoop) || block.GetPath(SoundCfgKey.PrimaryLoop, trainFolder, out primaryLoop))
						{
							sound.SoundElements.Add(new PrimaryHornElement { Key = HornKey.Start, FilePath = primaryLoop });
							if (block.GetPath(SoundCfgKey.PrimaryStart, trainFolder, out string primaryStart))
							{
								// FIXME: Start / end sounds not supported by TE2
							}
							if (block.GetPath(SoundCfgKey.PrimaryEnd, trainFolder, out string primaryEnd))
							{
								// FIXME: Start / end sounds not supported by TE2
							}
						}
						if (block.GetPath(SoundCfgKey.Secondary, trainFolder, out string secondaryLoop) || block.GetPath(SoundCfgKey.SecondaryLoop, trainFolder, out secondaryLoop))
						{
							sound.SoundElements.Add(new SecondaryHornElement { Key = HornKey.Start, FilePath = secondaryLoop });
							if (block.GetPath(SoundCfgKey.SecondaryStart, trainFolder, out string secondaryStart))
							{
								// FIXME: Start / end sounds not supported by TE2
							}
							if (block.GetPath(SoundCfgKey.SecondaryEnd, trainFolder, out string secondaryEnd))
							{
								// FIXME: Start / end sounds not supported by TE2
							}

						}
						if (block.GetPath(SoundCfgKey.Music, trainFolder, out string musicLoop) || block.GetPath(SoundCfgKey.MusicLoop, trainFolder, out musicLoop))
						{
							sound.SoundElements.Add(new MusicHornElement { Key = HornKey.Start, FilePath = musicLoop });
							if (block.GetPath(SoundCfgKey.MusicStart, trainFolder, out string musicStart))
							{
								// FIXME: Start / end sounds not supported by TE2
							}
							if (block.GetPath(SoundCfgKey.MusicEnd, trainFolder, out string musicEnd))
							{
								// FIXME: Start / end sounds not supported by TE2
							}

						}
						break;
					case SoundCfgSection.Door:
						block.GetPath(SoundCfgKey.OpenLeft, trainFolder, out string openLeft);
						sound.SoundElements.Add(new DoorElement { Key = DoorKey.OpenLeft, FilePath = openLeft });
						block.GetPath(SoundCfgKey.CloseLeft, trainFolder, out string closeLeft);
						sound.SoundElements.Add(new DoorElement { Key = DoorKey.CloseLeft, FilePath = closeLeft });
						block.GetPath(SoundCfgKey.OpenRight, trainFolder, out string openRight);
						sound.SoundElements.Add(new DoorElement { Key = DoorKey.OpenRight, FilePath = openRight });
						block.GetPath(SoundCfgKey.CloseRight, trainFolder, out string closeRight);
						sound.SoundElements.Add(new DoorElement { Key = DoorKey.CloseRight, FilePath = closeRight });
						break;
					case SoundCfgSection.ATS:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var atsIndex, out var fileName))
						{
							sound.SoundElements.Add(new AtsElement { Key = atsIndex, FilePath = fileName });
						}
						break;
					case SoundCfgSection.Buzzer:
						block.GetPath(SoundCfgKey.Correct, trainFolder, out string buzzerCorrect);
						sound.SoundElements.Add(new BuzzerElement { Key = BuzzerKey.Correct, FilePath = buzzerCorrect });
						break;
					case SoundCfgSection.PilotLamp:
						block.GetPath(SoundCfgKey.On, trainFolder, out string lampOn);
						sound.SoundElements.Add(new PilotLampElement { Key = PilotLampKey.On, FilePath = lampOn });
						block.GetPath(SoundCfgKey.Off, trainFolder, out string lampOff);
						sound.SoundElements.Add(new PilotLampElement { Key = PilotLampKey.Off, FilePath = lampOff });
						break;
					case SoundCfgSection.BrakeHandle:
						block.GetPath(SoundCfgKey.Apply, trainFolder, out string apply);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.Apply, FilePath = apply });
						block.GetPath(SoundCfgKey.ApplyFast, trainFolder, out string applyFast);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.ApplyFast, FilePath = applyFast });
						block.GetPath(SoundCfgKey.Release, trainFolder, out string brakeRelease);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.Release, FilePath = brakeRelease });
						block.GetPath(SoundCfgKey.ReleaseFast, trainFolder, out string brakeReleaseFast);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.ReleaseFast, FilePath = brakeReleaseFast });
						block.GetPath(SoundCfgKey.Min, trainFolder, out string brakeMin);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.Min, FilePath = brakeMin });
						block.GetPath(SoundCfgKey.Max, trainFolder, out string brakeMax);
						sound.SoundElements.Add(new BrakeHandleElement { Key = BrakeHandleKey.Max, FilePath = brakeMax });
						break;
					case SoundCfgSection.MasterController:
						block.GetPath(SoundCfgKey.Up, trainFolder, out string up);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.Up, FilePath = up });
						block.GetPath(SoundCfgKey.UpFast, trainFolder, out string upFast);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.UpFast, FilePath = upFast });
						block.GetPath(SoundCfgKey.Down, trainFolder, out string down);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.Down, FilePath = down });
						block.GetPath(SoundCfgKey.DownFast, trainFolder, out string downFast);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.DownFast, FilePath = downFast });
						block.GetPath(SoundCfgKey.Min, trainFolder, out string powerMin);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.Min, FilePath = powerMin });
						block.GetPath(SoundCfgKey.Max, trainFolder, out string powerMax);
						sound.SoundElements.Add(new MasterControllerElement { Key = MasterControllerKey.Max, FilePath = powerMax });
						break;
					case SoundCfgSection.Reverser:
						block.GetPath(SoundCfgKey.On, trainFolder, out string reverserOn);
						sound.SoundElements.Add(new ReverserElement { Key = ReverserKey.On, FilePath = reverserOn });
						block.GetPath(SoundCfgKey.Off, trainFolder, out string reverserOff);
						sound.SoundElements.Add(new ReverserElement { Key = ReverserKey.Off, FilePath = reverserOff });
						break;
					case SoundCfgSection.Breaker:
						block.GetPath(SoundCfgKey.On, trainFolder, out string breakerOn);
						sound.SoundElements.Add(new BreakerElement { Key = BreakerKey.On, FilePath = breakerOn });
						block.GetPath(SoundCfgKey.Off, trainFolder, out string breakerOff);
						sound.SoundElements.Add(new BreakerElement { Key = BreakerKey.Off, FilePath = breakerOff });
						break;
					case SoundCfgSection.Others:
						block.GetPath(SoundCfgKey.Noise, trainFolder, out string noise);
						sound.SoundElements.Add(new OthersElement { Key = OthersKey.Noise, FilePath = noise });
						block.GetPath(SoundCfgKey.Shoe, trainFolder, out string rub);
						sound.SoundElements.Add(new OthersElement { Key = OthersKey.Shoe, FilePath = rub });
						block.GetPath(SoundCfgKey.Halt, trainFolder, out string halt);
						sound.SoundElements.Add(new OthersElement { Key = OthersKey.Halt, FilePath = halt });
						break;
					case SoundCfgSection.RequestStop:
						block.GetPath(SoundCfgKey.Stop, trainFolder, out string requestStop);
						sound.SoundElements.Add(new RequestStopElement { Key = RequestStopKey.Stop, FilePath = requestStop });
						block.GetPath(SoundCfgKey.Pass, trainFolder, out string requestPass);
						sound.SoundElements.Add(new RequestStopElement { Key = RequestStopKey.Pass, FilePath = requestPass });
						block.GetPath(SoundCfgKey.Ignored, trainFolder, out string requestIgnored);
						sound.SoundElements.Add(new RequestStopElement { Key = RequestStopKey.Ignored, FilePath = requestIgnored });
						break;
					case SoundCfgSection.Touch:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(trainFolder, out var touchIndex, out var fileName))
						{
							sound.SoundElements.Add(new TouchElement { Key = touchIndex, FilePath = fileName });
						}
						break;
				}
				block.ReportErrors();
			}
			sound.SoundElements = new ObservableCollection<SoundElement>(sound.SoundElements.GroupBy(x => new { Type = x.GetType(), x.Key }).Select(x => x.First()));
		}
	}
}
