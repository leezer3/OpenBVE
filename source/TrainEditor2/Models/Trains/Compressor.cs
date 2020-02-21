using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class Compressor : BindableBase, ICloneable
	{
		private double rate;

		internal double Rate
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
			Rate = 5.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
