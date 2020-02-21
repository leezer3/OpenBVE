using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class AuxiliaryReservoir : BindableBase, ICloneable
	{
		private double chargeRate;

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

		internal AuxiliaryReservoir()
		{
			ChargeRate = 200.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
