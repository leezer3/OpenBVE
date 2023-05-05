using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class Compressor : BindableBase, ICloneable
	{
		private Quantity.PressureRate rate;

		internal Quantity.PressureRate Rate
		{
			get
			{
				return rate;
			}
			set
			{
				SetProperty(ref rate, value);
			}
		}

		internal Compressor()
		{
			Rate = new Quantity.PressureRate(5.0, Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
