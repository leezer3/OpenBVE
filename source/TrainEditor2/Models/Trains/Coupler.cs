using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainManager.Car;

namespace TrainEditor2.Models.Trains
{
	internal class Coupler : BindableBase, ICloneable
	{
		private double min;
		private double max;
		private string exteriorObject;

		internal double Min
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

		internal double Max
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
			Min = 0.27;
			Max = 0.33;
			Object = string.Empty;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		internal void WriteExtensionsCfg(string fileName, StringBuilder builder, int couplerIndex)
		{
			builder.AppendLine($"[Coupler{couplerIndex.ToString(CultureInfo.InvariantCulture)}]");
			Utilities.WriteKey(builder, "Distances", Min, Max);
			Utilities.WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, Object));
		}
	}
}
