using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class AuxiliaryReservoir : BindableBase, ICloneable
	{
		private Quantity.PressureRate chargeRate;

		internal Quantity.PressureRate ChargeRate
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
			ChargeRate = new Quantity.PressureRate(200.0,Unit.PressureRate.KilopascalPerSecond);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
