using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class StraightAirPipe : BindableBase, ICloneable
	{
		private Quantity.PressureRate serviceRate;
		private Quantity.PressureRate emergencyRate;
		private Quantity.PressureRate releaseRate;

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

		internal StraightAirPipe()
		{
			ServiceRate = new Quantity.PressureRate(300.0, Unit.PressureRate.KilopascalPerSecond);
			EmergencyRate = new Quantity.PressureRate(400.0, Unit.PressureRate.KilopascalPerSecond);
			ReleaseRate = new Quantity.PressureRate(200.0, Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
