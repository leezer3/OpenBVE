using System;
using Prism.Mvvm;
using TrainManager.BrakeSystems;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Brake : BindableBase, ICloneable
	{
		internal enum LocoBrakeTypes
		{
			NotFitted = 0,
			NotchedAirBrake = 1,
			AutomaticAirBrake = 2
		}

		private BrakeSystemType brakeType;
		private LocoBrakeTypes locoBrakeType;
		private EletropneumaticBrakeType brakeControlSystem;
		private double brakeControlSpeed;

		internal BrakeSystemType BrakeType
		{
			get
			{
				return brakeType;
			}
			set
			{
				SetProperty(ref brakeType, value);
			}
		}

		internal LocoBrakeTypes LocoBrakeType
		{
			get
			{
				return locoBrakeType;
			}
			set
			{
				SetProperty(ref locoBrakeType, value);
			}
		}

		internal EletropneumaticBrakeType BrakeControlSystem
		{
			get
			{
				return brakeControlSystem;
			}
			set
			{
				SetProperty(ref brakeControlSystem, value);
			}
		}

		internal double BrakeControlSpeed
		{
			get
			{
				return brakeControlSpeed;
			}
			set
			{
				SetProperty(ref brakeControlSpeed, value);
			}
		}

		internal Brake()
		{
			BrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
			LocoBrakeType = LocoBrakeTypes.NotFitted;
			BrakeControlSystem = EletropneumaticBrakeType.None;
			BrakeControlSpeed = 0.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
