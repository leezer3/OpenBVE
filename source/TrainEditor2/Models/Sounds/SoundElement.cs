using System;
using Prism.Mvvm;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Sounds
{
	internal enum BrakeKey
	{
		[StringValue("Bc Release High", "ReleaseHigh")]
		BcReleaseHigh,

		[StringValue("Bc Release", "Release")]
		BcRelease,

		[StringValue("Bc Release Full", "ReleaseFull")]
		BcReleaseFull,

		[StringValue("Emergency")]
		Emergency,

		[StringValue("BP Decomp", "Application")]
		BpDecomp
	}

	internal enum BrakeHandleKey
	{
		[StringValue("Apply")]
		Apply,

		[StringValue("ApplyFast")]
		ApplyFast,

		[StringValue("Release")]
		Release,

		[StringValue("ReleaseFast")]
		ReleaseFast,

		[StringValue("Min", "Minimum")]
		Min,

		[StringValue("Max", "Maximum")]
		Max
	}

	internal enum BreakerKey
	{
		[StringValue("On")]
		On,

		[StringValue("Off")]
		Off
	}

	internal enum BuzzerKey
	{
		[StringValue("Correct")]
		Correct
	}

	internal enum CompressorKey
	{
		[StringValue("Attack", "Start")]
		Attack,

		[StringValue("Loop")]
		Loop,

		[StringValue("Release", "Stop", "End")]
		Release
	}

	internal enum DoorKey
	{
		[StringValue("Open Left", "OpenLeft", "LeftOpen")]
		OpenLeft,

		[StringValue("Close Left", "CloseLeft", "LeftClose")]
		CloseLeft,

		[StringValue("Open Right", "OpenRight", "RightOpen")]
		OpenRight,

		[StringValue("Close Right", "CloseRight", "RightClose")]
		CloseRight
	}

	internal enum HornKey
	{
		[StringValue("Start")]
		Start,

		[StringValue("Loop")]
		Loop,

		[StringValue("End", "Release", "Stop")]
		End
	}

	internal enum MasterControllerKey
	{
		[StringValue("Up", "Increase")]
		Up,

		[StringValue("UpFast", "IncreaseFast")]
		UpFast,

		[StringValue("Down", "Decrease")]
		Down,

		[StringValue("DownFast", "DecreaseFast")]
		DownFast,

		[StringValue("Min", "Minimum")]
		Min,

		[StringValue("Max", "Maximum")]
		Max
	}

	internal enum OthersKey
	{
		[StringValue("Noise")]
		Noise,

		[StringValue("Shoe")]
		Shoe,

		[StringValue("Halt")]
		Halt
	}

	internal enum PilotLampKey
	{
		[StringValue("On")]
		On,

		[StringValue("Off")]
		Off
	}

	internal enum RequestStopKey
	{
		[StringValue("Stop")]
		Stop,

		[StringValue("Pass")]
		Pass,

		[StringValue("Ignored")]
		Ignored
	}

	internal enum ReverserKey
	{
		[StringValue("On")]
		On,

		[StringValue("Off")]
		Off
	}

	internal enum SuspensionKey
	{
		[StringValue("Left")]
		Left,

		[StringValue("Right")]
		Right
	}

	internal abstract class SoundElement : BindableBase, ICloneable
	{
		protected object key;
		protected string filePath;
		protected bool definedPosition;
		protected double positionX;
		protected double positionY;
		protected double positionZ;
		protected bool definedRadius;
		protected double radius;

		internal object Key
		{
			get
			{
				return key;
			}
			set
			{
				SetProperty(ref key, value);
			}
		}

		internal string FilePath
		{
			get
			{
				return filePath;
			}
			set
			{
				SetProperty(ref filePath, value);
			}
		}

		internal bool DefinedPosition
		{
			get
			{
				return definedPosition;
			}
			set
			{
				SetProperty(ref definedPosition, value);
			}
		}

		internal double PositionX
		{
			get
			{
				return positionX;
			}
			set
			{
				SetProperty(ref positionX, value);
			}
		}

		internal double PositionY
		{
			get
			{
				return positionY;
			}
			set
			{
				SetProperty(ref positionY, value);
			}
		}

		internal double PositionZ
		{
			get
			{
				return positionZ;
			}
			set
			{
				SetProperty(ref positionZ, value);
			}
		}

		internal bool DefinedRadius
		{
			get
			{
				return definedRadius;
			}
			set
			{
				SetProperty(ref definedRadius, value);
			}
		}

		internal double Radius
		{
			get
			{
				return radius;
			}
			set
			{
				SetProperty(ref radius, value);
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	internal abstract class SoundElement<T> : SoundElement
	{
		internal new T Key
		{
			get
			{
				return (T)key;
			}
			set
			{
				SetProperty(ref key, value);
			}
		}

		internal SoundElement()
		{
			Key = default(T);
		}
	}

	internal class RunElement : SoundElement<int>
	{
	}

	internal class FlangeElement : SoundElement<int>
	{
	}

	internal class MotorElement : SoundElement<int>
	{
	}

	internal class FrontSwitchElement : SoundElement<int>
	{
	}

	internal class RearSwitchElement : SoundElement<int>
	{
	}

	internal class BrakeElement : SoundElement<BrakeKey>
	{
	}

	internal class CompressorElement : SoundElement<CompressorKey>
	{
	}

	internal class SuspensionElement : SoundElement<SuspensionKey>
	{
	}

	internal class PrimaryHornElement : SoundElement<HornKey>
	{
	}

	internal class SecondaryHornElement : SoundElement<HornKey>
	{
	}

	internal class MusicHornElement : SoundElement<HornKey>
	{
	}

	internal class DoorElement : SoundElement<DoorKey>
	{
	}

	internal class AtsElement : SoundElement<int>
	{
	}

	internal class BuzzerElement : SoundElement<BuzzerKey>
	{
	}

	internal class PilotLampElement : SoundElement<PilotLampKey>
	{
	}

	internal class BrakeHandleElement : SoundElement<BrakeHandleKey>
	{
	}

	internal class MasterControllerElement : SoundElement<MasterControllerKey>
	{
	}

	internal class ReverserElement : SoundElement<ReverserKey>
	{
	}

	internal class BreakerElement : SoundElement<BreakerKey>
	{
	}

	internal class RequestStopElement : SoundElement<RequestStopKey>
	{
	}

	internal class TouchElement : SoundElement<int>
	{
	}

	internal class OthersElement : SoundElement<OthersKey>
	{
	}
}
