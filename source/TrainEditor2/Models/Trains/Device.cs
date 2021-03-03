using System;
using Prism.Mvvm;
using TrainManager.Car;
using TrainManager.SafetySystems;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Device section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Device : BindableBase, ICloneable
	{
		private AtsModes ats;
		private AtcModes atc;
		private bool eb;
		private bool constSpeed;
		private bool holdBrake;
		private ReadhesionDeviceType reAdhesionDevice;
		private double loadCompensatingDevice;
		private PassAlarmType passAlarm;
		private DoorMode doorOpenMode;
		private DoorMode doorCloseMode;
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

		internal ReadhesionDeviceType ReAdhesionDevice
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

		internal PassAlarmType PassAlarm
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

		internal DoorMode DoorOpenMode
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

		internal DoorMode DoorCloseMode
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
			ReAdhesionDevice = ReadhesionDeviceType.TypeA;
			LoadCompensatingDevice = 0.0;
			PassAlarm = PassAlarmType.None;
			DoorOpenMode = DoorMode.AutomaticManualOverride;
			DoorCloseMode = DoorMode.AutomaticManualOverride;
			DoorWidth = 1000.0;
			DoorMaxTolerance = 0.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
