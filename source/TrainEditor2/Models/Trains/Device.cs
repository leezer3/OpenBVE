using System;
using TrainEditor2.Extensions;
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
			get => ats;
			set => SetProperty(ref ats, value);
		}

		internal AtcModes Atc
		{
			get => atc;
			set => SetProperty(ref atc, value);
		}

		internal bool Eb
		{
			get => eb;
			set => SetProperty(ref eb, value);
		}

		internal bool ConstSpeed
		{
			get => constSpeed;
			set => SetProperty(ref constSpeed, value);
		}

		internal bool HoldBrake
		{
			get => holdBrake;
			set => SetProperty(ref holdBrake, value);
		}

		internal ReadhesionDeviceType ReAdhesionDevice
		{
			get => reAdhesionDevice;
			set => SetProperty(ref reAdhesionDevice, value);
		}

		internal double LoadCompensatingDevice
		{
			get => loadCompensatingDevice;
			set => SetProperty(ref loadCompensatingDevice, value);
		}

		internal PassAlarmType PassAlarm
		{
			get => passAlarm;
			set => SetProperty(ref passAlarm, value);
		}

		internal DoorMode DoorOpenMode
		{
			get => doorOpenMode;
			set => SetProperty(ref doorOpenMode, value);
		}

		internal DoorMode DoorCloseMode
		{
			get => doorCloseMode;
			set => SetProperty(ref doorCloseMode, value);
		}

		internal double DoorWidth
		{
			get => doorWidth;
			set => SetProperty(ref doorWidth, value);
		}

		internal double DoorMaxTolerance
		{
			get => doorMaxTolerance;
			set => SetProperty(ref doorMaxTolerance, value);
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
