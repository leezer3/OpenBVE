using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class BrakePipe : BindableBase, ICloneable
	{
		private Quantity.Pressure normalPressure;
		private Quantity.PressureRate chargeRate;
		private Quantity.PressureRate serviceRate;
		private Quantity.PressureRate emergencyRate;

		internal Quantity.Pressure NormalPressure
		{
			get
			{
				return normalPressure;
			}
			set
			{
				SetProperty(ref normalPressure, value);
			}
		}

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

		internal BrakePipe()
		{
			NormalPressure = new Quantity.Pressure(490.0, Unit.Pressure.Kilopascal);
			ChargeRate = new Quantity.PressureRate(10000.0, Unit.PressureRate.KilopascalPerSecond);
			ServiceRate = new Quantity.PressureRate(1500.0, Unit.PressureRate.KilopascalPerSecond);
			EmergencyRate = new Quantity.PressureRate(5000.0, Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
