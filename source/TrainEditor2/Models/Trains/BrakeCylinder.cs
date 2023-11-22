using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class BrakeCylinder : BindableBase, ICloneable
	{
		private Quantity.Pressure serviceMaximumPressure;
		private Quantity.Pressure emergencyMaximumPressure;
		private Quantity.PressureRate emergencyRate;
		private Quantity.PressureRate releaseRate;

		internal Quantity.Pressure ServiceMaximumPressure
		{
			get
			{
				return serviceMaximumPressure;
			}
			set
			{
				SetProperty(ref serviceMaximumPressure, value);
			}
		}

		internal Quantity.Pressure EmergencyMaximumPressure
		{
			get
			{
				return emergencyMaximumPressure;
			}
			set
			{
				SetProperty(ref emergencyMaximumPressure, value);
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

		internal Quantity.PressureRate ReleaseRate
		{
			get
			{
				return releaseRate;
			}
			set
			{
				SetProperty(ref releaseRate, value);
			}
		}

		internal BrakeCylinder()
		{
			ServiceMaximumPressure = new Quantity.Pressure(480.0, Unit.Pressure.Kilopascal);
			EmergencyMaximumPressure = new Quantity.Pressure(480.0, Unit.Pressure.Kilopascal);
			EmergencyRate = new Quantity.PressureRate(300.0, Unit.PressureRate.KilopascalPerSecond);
			ReleaseRate = new Quantity.PressureRate(200.0, Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
