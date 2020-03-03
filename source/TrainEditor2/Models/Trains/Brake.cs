﻿using System;
using OpenBveApi.Units;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Brake : BindableBase, ICloneable
	{
		internal enum BrakeTypes
		{
			ElectromagneticStraightAirBrake = 0,
			ElectricCommandBrake = 1,
			AutomaticAirBrake = 2
		}

		internal enum LocoBrakeTypes
		{
			NotFitted = 0,
			NotchedAirBrake = 1,
			AutomaticAirBrake = 2
		}

		internal enum BrakeControlSystems
		{
			None = 0,
			ClosingElectromagneticValve = 1,
			DelayIncludingSystem = 2
		}

		private BrakeTypes brakeType;
		private LocoBrakeTypes locoBrakeType;
		private BrakeControlSystems brakeControlSystem;
		private Quantity.Velocity brakeControlSpeed;

		internal BrakeTypes BrakeType
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

		internal BrakeControlSystems BrakeControlSystem
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

		internal Quantity.Velocity BrakeControlSpeed
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
			BrakeType = BrakeTypes.ElectromagneticStraightAirBrake;
			LocoBrakeType = LocoBrakeTypes.NotFitted;
			BrakeControlSystem = BrakeControlSystems.None;
			BrakeControlSpeed = new Quantity.Velocity(0.0, Unit.Velocity.KilometerPerHour);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
