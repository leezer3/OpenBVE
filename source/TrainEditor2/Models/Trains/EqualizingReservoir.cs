using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class EqualizingReservoir : BindableBase, ICloneable
	{
		private double chargeRate;
		private double serviceRate;
		private double emergencyRate;

		internal double ChargeRate
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

		internal double ServiceRate
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

		internal double EmergencyRate
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
			ChargeRate = 50.0;
			ServiceRate = 250.0;
			EmergencyRate = 200.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
