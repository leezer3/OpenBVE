using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class MainReservoir : BindableBase, ICloneable
	{
		private double minimumPressure;
		private double maximumPressure;

		internal double MinimumPressure
		{
			get
			{
				return minimumPressure;
			}
			set
			{
				SetProperty(ref minimumPressure, value);
			}
		}

		internal double MaximumPressure
		{
			get
			{
				return maximumPressure;
			}
			set
			{
				SetProperty(ref maximumPressure, value);
			}
		}

		internal MainReservoir()
		{
			MinimumPressure = 690.0;
			MaximumPressure = 780.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
