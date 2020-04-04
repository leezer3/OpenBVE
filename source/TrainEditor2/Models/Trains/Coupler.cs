using System;
using OpenBveApi.Units;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class Coupler : BindableBase, ICloneable
	{
		private Quantity.Length min;
		private Quantity.Length max;
		private string exteriorObject;

		internal Quantity.Length Min
		{
			get
			{
				return min;
			}
			set
			{
				SetProperty(ref min, value);
			}
		}

		internal Quantity.Length Max
		{
			get
			{
				return max;
			}
			set
			{
				SetProperty(ref max, value);
			}
		}

		internal string Object
		{
			get
			{
				return exteriorObject;
			}
			set
			{
				SetProperty(ref exteriorObject, value);
			}
		}

		internal Coupler()
		{
			Min = new Quantity.Length(0.27);
			Max = new Quantity.Length(0.33);
			Object = string.Empty;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
