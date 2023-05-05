using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class EqualizingReservoir : BindableBase, ICloneable
	{
		private Quantity.PressureRate chargeRate;
		private Quantity.PressureRate serviceRate;
		private Quantity.PressureRate emergencyRate;

		internal Quantity.PressureRate ChargeRate
		{
			get
			{
				return chargeRate;
			}
			set
			{
				SetProperty(ref chargeRate, value);
			}
		}

		internal Quantity.PressureRate ServiceRate
		{
			get
			{
				return serviceRate;
			}
			set
			{
				SetProperty(ref serviceRate, value);
			}
		}

		internal Quantity.PressureRate EmergencyRate
		{
			get
			{
				return emergencyRate;
			}
			set
			{
				SetProperty(ref emergencyRate, value);
			}
		}

		internal EqualizingReservoir()
		{
			ChargeRate = new Quantity.PressureRate(50.0, Unit.PressureRate.KilopascalPerSecond);
			ServiceRate = new Quantity.PressureRate(250.0, Unit.PressureRate.KilopascalPerSecond);
			EmergencyRate = new Quantity.PressureRate(200.0, Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
