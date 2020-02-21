using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class BrakeCylinder : BindableBase, ICloneable
	{
		private double serviceMaximumPressure;
		private double emergencyMaximumPressure;
		private double emergencyRate;
		private double releaseRate;

		internal double ServiceMaximumPressure
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

		internal double EmergencyMaximumPressure
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

		internal double ReleaseRate
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
			ServiceMaximumPressure = 480.0;
			EmergencyMaximumPressure = 480.0;
			EmergencyRate = 300.0;
			ReleaseRate = 200.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
