using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class BrakePipe : BindableBase, ICloneable
	{
		private double normalPressure;
		private double chargeRate;
		private double serviceRate;
		private double emergencyRate;

		internal double NormalPressure
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

		internal BrakePipe()
		{
			NormalPressure = 490.0;
			ChargeRate = 10000.0;
			ServiceRate = 1500.0;
			EmergencyRate = 5000.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
