using System.Collections.Generic;
using OpenBveApi.Math;

namespace SoundEditor.Parsers.Sound
{
	internal static partial class SoundCfg
	{
		internal class Sound
		{
			internal string FileName;
			internal bool IsPositionDefined;
			internal Vector3 Position;
			internal bool IsRadiusDefined;
			internal double Radius;

			internal Sound()
			{
				Initialize();
			}

			internal void Initialize()
			{
				FileName = string.Empty;
				IsPositionDefined = false;
				Position = Vector3.Zero;
				IsRadiusDefined = false;
				Radius = 0.0;
			}

			internal void Apply(Sound sound)
			{
				FileName = sound.FileName;
				IsPositionDefined = sound.IsPositionDefined;
				Position = sound.Position;
				IsRadiusDefined = sound.IsRadiusDefined;
				Radius = sound.Radius;
			}
		}

		internal class IndexedSound : Sound
		{
			internal int Index;

			internal IndexedSound()
			{
				Index = -1;
			}

			internal IndexedSound(int index, Sound sound)
			{
				Index = index;
				Apply(sound);
			}
		}

		internal class ListedSound : List<IndexedSound>
		{
			internal new void Add(IndexedSound sound)
			{
				int index = FindIndex(s => s.Index == sound.Index);

				if (index >= 0)
				{
					base[index] = sound;
				}
				else
				{
					base.Add(sound);
				}
			}
		}

		internal class BrakeSound
		{
			internal Sound BcReleaseHigh;
			internal Sound BcRelease;
			internal Sound BcReleaseFull;
			internal Sound Emergency;
			internal Sound BpDecomp;

			internal BrakeSound()
			{
				BcReleaseHigh = new Sound();
				BcRelease = new Sound();
				BcReleaseFull = new Sound();
				Emergency = new Sound();
				BpDecomp = new Sound();
			}
		}

		internal class CompressorSound
		{
			internal Sound Attack;
			internal Sound Loop;
			internal Sound Release;

			internal CompressorSound()
			{
				Attack = new Sound();
				Loop = new Sound();
				Release = new Sound();
			}
		}

		internal class SuspensionSound
		{
			internal Sound Left;
			internal Sound Right;

			internal SuspensionSound()
			{
				Left = new Sound();
				Right = new Sound();
			}
		}

		internal class HornSound
		{
			internal Sound Start;
			internal Sound Loop;
			internal Sound End;
			internal bool Toggle;

			internal HornSound()
			{
				Start = new Sound();
				Loop = new Sound();
				End = new Sound();
				Toggle = false;
			}
		}

		internal class DoorSound
		{
			internal Sound OpenLeft;
			internal Sound CloseLeft;

			internal Sound OpenRight;
			internal Sound CloseRight;

			internal DoorSound()
			{
				OpenLeft = new Sound();
				CloseLeft = new Sound();

				OpenRight = new Sound();
				CloseRight = new Sound();
			}
		}

		internal class BuzzerSound
		{
			internal Sound Correct;

			internal BuzzerSound()
			{
				Correct = new Sound();
			}
		}

		internal class PilotLampSound
		{
			internal Sound On;
			internal Sound Off;

			internal PilotLampSound()
			{
				On = new Sound();
				Off = new Sound();
			}
		}

		internal class BrakeHandleSound
		{
			internal Sound Apply;
			internal Sound ApplyFast;

			internal Sound Release;
			internal Sound ReleaseFast;

			internal Sound Min;
			internal Sound Max;

			internal BrakeHandleSound()
			{
				Apply = new Sound();
				ApplyFast = new Sound();

				Release = new Sound();
				ReleaseFast = new Sound();

				Min = new Sound();
				Max = new Sound();
			}
		}

		internal class MasterControllerSound
		{
			internal Sound Up;
			internal Sound UpFast;

			internal Sound Down;
			internal Sound DownFast;

			internal Sound Min;
			internal Sound Max;

			internal MasterControllerSound()
			{
				Up = new Sound();
				UpFast = new Sound();

				Down = new Sound();
				DownFast = new Sound();

				Min = new Sound();
				Max = new Sound();
			}
		}

		internal class ReverserSound
		{
			internal Sound On;
			internal Sound Off;

			internal ReverserSound()
			{
				On = new Sound();
				Off = new Sound();
			}
		}

		internal class BreakerSound
		{
			internal Sound On;
			internal Sound Off;

			internal BreakerSound()
			{
				On = new Sound();
				Off = new Sound();
			}
		}

		internal class RequestStopSound
		{
			internal Sound Stop;
			internal Sound Pass;
			internal Sound Ignored;

			internal RequestStopSound()
			{
				Stop = new Sound();
				Pass = new Sound();
				Ignored = new Sound();
			}
		}

		internal class OthersSound
		{
			internal Sound Noise;
			internal Sound Shoe;
			internal Sound Halt;

			internal OthersSound()
			{
				Noise = new Sound();
				Shoe = new Sound();
				Halt = new Sound();
			}
		}

		internal class Sounds
		{
			internal readonly ListedSound Run;
			internal readonly ListedSound Flange;
			internal readonly ListedSound Motor;
			internal readonly ListedSound FrontSwitch;
			internal readonly ListedSound RearSwitch;
			internal readonly BrakeSound Brake;
			internal readonly CompressorSound Compressor;
			internal readonly SuspensionSound Suspension;
			internal readonly HornSound PrimaryHorn;
			internal readonly HornSound SecondaryHorn;
			internal readonly HornSound MusicHorn;
			internal readonly DoorSound Door;
			internal readonly ListedSound Ats;
			internal readonly BuzzerSound Buzzer;
			internal readonly PilotLampSound PilotLamp;
			internal readonly BrakeHandleSound BrakeHandle;
			internal readonly MasterControllerSound MasterController;
			internal readonly ReverserSound Reverser;
			internal readonly BreakerSound Breaker;
			internal readonly RequestStopSound RequestStop;
			internal readonly ListedSound Touch;
			internal readonly OthersSound Others;

			internal Sounds()
			{
				Run = new ListedSound();
				Flange = new ListedSound();
				Motor = new ListedSound();
				FrontSwitch = new ListedSound();
				RearSwitch = new ListedSound();
				Brake = new BrakeSound();
				Compressor = new CompressorSound();
				Suspension = new SuspensionSound();
				PrimaryHorn = new HornSound();
				SecondaryHorn = new HornSound();
				MusicHorn = new HornSound();
				Door = new DoorSound();
				Ats = new ListedSound();
				Buzzer = new BuzzerSound();
				PilotLamp = new PilotLampSound();
				BrakeHandle = new BrakeHandleSound();
				MasterController = new MasterControllerSound();
				Reverser = new ReverserSound();
				Breaker = new BreakerSound();
				RequestStop = new RequestStopSound();
				Touch = new ListedSound();
				Others = new OthersSound();
			}
		}
	}
}
