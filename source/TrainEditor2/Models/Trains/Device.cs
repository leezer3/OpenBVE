using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Device section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Device : BindableBase, ICloneable
	{
		internal enum AtsModes
		{
			None = -1,
			AtsSn = 0,
			AtsSnP = 1
		}

		internal enum AtcModes
		{
			None = 0,
			Manual = 1,
			Automatic = 2
		}

		internal enum ReAdhesionDevices
		{
			None = -1,
			TypeA = 0,
			TypeB = 1,
			TypeC = 2,
			TypeD = 3
		}

		internal enum PassAlarmModes
		{
			None = 0,
			Single = 1,
			Looping = 2
		}

		internal enum DoorModes
		{
			SemiAutomatic = 0,
			Automatic = 1,
			Manual = 2
		}

		private AtsModes ats;
		private AtcModes atc;
		private bool eb;
		private bool constSpeed;
		private bool holdBrake;
		private ReAdhesionDevices reAdhesionDevice;
		private double loadCompensatingDevice;
		private PassAlarmModes passAlarm;
		private DoorModes doorOpenMode;
		private DoorModes doorCloseMode;
		private double doorWidth;
		private double doorMaxTolerance;

		internal AtsModes Ats
		{
			get
			{
				return ats;
			}
			set
			{
				SetProperty(ref ats, value);
			}
		}

		internal AtcModes Atc
		{
			get
			{
				return atc;
			}
			set
			{
				SetProperty(ref atc, value);
			}
		}

		internal bool Eb
		{
			get
			{
				return eb;
			}
			set
			{
				SetProperty(ref eb, value);
			}
		}

		internal bool ConstSpeed
		{
			get
			{
				return constSpeed;
			}
			set
			{
				SetProperty(ref constSpeed, value);
			}
		}

		internal bool HoldBrake
		{
			get
			{
				return holdBrake;
			}
			set
			{
				SetProperty(ref holdBrake, value);
			}
		}

		internal ReAdhesionDevices ReAdhesionDevice
		{
			get
			{
				return reAdhesionDevice;
			}
			set
			{
				SetProperty(ref reAdhesionDevice, value);
			}
		}

		internal double LoadCompensatingDevice
		{
			get
			{
				return loadCompensatingDevice;
			}
			set
			{
				SetProperty(ref loadCompensatingDevice, value);
			}
		}

		internal PassAlarmModes PassAlarm
		{
			get
			{
				return passAlarm;
			}
			set
			{
				SetProperty(ref passAlarm, value);
			}
		}

		internal DoorModes DoorOpenMode
		{
			get
			{
				return doorOpenMode;
			}
			set
			{
				SetProperty(ref doorOpenMode, value);
			}
		}

		internal DoorModes DoorCloseMode
		{
			get
			{
				return doorCloseMode;
			}
			set
			{
				SetProperty(ref doorCloseMode, value);
			}
		}

		internal double DoorWidth
		{
			get
			{
				return doorWidth;
			}
			set
			{
				SetProperty(ref doorWidth, value);
			}
		}

		internal double DoorMaxTolerance
		{
			get
			{
				return doorMaxTolerance;
			}
			set
			{
				SetProperty(ref doorMaxTolerance, value);
			}
		}

		internal Device()
		{
			Ats = AtsModes.AtsSn;
			Atc = AtcModes.None;
			Eb = false;
			ConstSpeed = false;
			HoldBrake = false;
			ReAdhesionDevice = ReAdhesionDevices.TypeA;
			LoadCompensatingDevice = 0.0;
			PassAlarm = PassAlarmModes.None;
			DoorOpenMode = DoorModes.SemiAutomatic;
			DoorCloseMode = DoorModes.SemiAutomatic;
			DoorWidth = 1000.0;
			DoorMaxTolerance = 0.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
