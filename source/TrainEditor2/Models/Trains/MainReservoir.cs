using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class MainReservoir : BindableBase, ICloneable
	{
		private Quantity.Pressure minimumPressure;
		private Quantity.Pressure maximumPressure;

		internal Quantity.Pressure MinimumPressure
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

		internal Quantity.Pressure MaximumPressure
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
			MinimumPressure = new Quantity.Pressure(690.0, Unit.Pressure.Kilopascal);
			MaximumPressure = new Quantity.Pressure(780.0, Unit.Pressure.Kilopascal);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
