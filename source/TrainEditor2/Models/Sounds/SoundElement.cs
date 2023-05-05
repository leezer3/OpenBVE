using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Sounds
{
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

	internal class BrakeElement : SoundElement<SoundKey.Brake>
	{
	}

	internal class CompressorElement : SoundElement<SoundKey.Compressor>
	{
	}

	internal class SuspensionElement : SoundElement<SoundKey.Suspension>
	{
	}

	internal class PrimaryHornElement : SoundElement<SoundKey.Horn>
	{
	}

	internal class SecondaryHornElement : SoundElement<SoundKey.Horn>
	{
	}

	internal class MusicHornElement : SoundElement<SoundKey.Horn>
	{
	}

	internal class DoorElement : SoundElement<SoundKey.Door>
	{
	}

	internal class AtsElement : SoundElement<int>
	{
	}

	internal class BuzzerElement : SoundElement<SoundKey.Buzzer>
	{
	}

	internal class PilotLampElement : SoundElement<SoundKey.PilotLamp>
	{
	}

	internal class BrakeHandleElement : SoundElement<SoundKey.BrakeHandle>
	{
	}

	internal class MasterControllerElement : SoundElement<SoundKey.MasterController>
	{
	}

	internal class ReverserElement : SoundElement<SoundKey.Reverser>
	{
	}

	internal class BreakerElement : SoundElement<SoundKey.Breaker>
	{
	}

	internal class RequestStopElement : SoundElement<SoundKey.RequestStop>
	{
	}

	internal class TouchElement : SoundElement<int>
	{
	}

	internal class OthersElement : SoundElement<SoundKey.Others>
	{
	}
}
