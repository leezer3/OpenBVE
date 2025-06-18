using System;
using TrainEditor2.Extensions;
using TrainManager.Handles;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Handle section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Handle : BindableBase, ICloneable
	{
		private HandleType handleType;
		private int powerNotches;
		private int brakeNotches;
		private int powerNotchReduceSteps;
		private EbHandleBehaviour handleBehaviour;
		private LocoBrakeType locoBrake;
		private int locoBrakeNotches;
		private int driverPowerNotches;
		private int driverBrakeNotches;

		internal HandleType HandleType
		{
			get => handleType;
			set => SetProperty(ref handleType, value);
		}

		internal int PowerNotches
		{
			get => powerNotches;
			set => SetProperty(ref powerNotches, value);
		}

		internal int BrakeNotches
		{
			get => brakeNotches;
			set => SetProperty(ref brakeNotches, value);
		}

		internal int PowerNotchReduceSteps
		{
			get => powerNotchReduceSteps;
			set => SetProperty(ref powerNotchReduceSteps, value);
		}

		internal EbHandleBehaviour HandleBehaviour
		{
			get => handleBehaviour;
			set => SetProperty(ref handleBehaviour, value);
		}

		internal LocoBrakeType LocoBrake
		{
			get => locoBrake;
			set => SetProperty(ref locoBrake, value);
		}

		internal int LocoBrakeNotches
		{
			get => locoBrakeNotches;
			set => SetProperty(ref locoBrakeNotches, value);
		}

		internal int DriverPowerNotches
		{
			get => driverPowerNotches;
			set => SetProperty(ref driverPowerNotches, value);
		}

		internal int DriverBrakeNotches
		{
			get => driverBrakeNotches;
			set => SetProperty(ref driverBrakeNotches, value);
		}

		internal Handle()
		{
			HandleType = HandleType.TwinHandle;
			PowerNotches = 8;
			BrakeNotches = 8;
			PowerNotchReduceSteps = 0;
			LocoBrakeNotches = 0;
			HandleBehaviour = EbHandleBehaviour.NoAction;
			LocoBrake = LocoBrakeType.Combined;
			DriverPowerNotches = 8;
			DriverBrakeNotches = 8;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
