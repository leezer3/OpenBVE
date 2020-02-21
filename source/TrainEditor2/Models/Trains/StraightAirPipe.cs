using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class StraightAirPipe : BindableBase, ICloneable
	{
		private double serviceRate;
		private double emergencyRate;
		private double releaseRate;

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

		internal StraightAirPipe()
		{
			ServiceRate = 300.0;
			EmergencyRate = 400.0;
			ReleaseRate = 200;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
