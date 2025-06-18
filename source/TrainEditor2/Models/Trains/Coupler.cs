using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Trains
{
	internal class Coupler : BindableBase, ICloneable
	{
		private double min;
		private double max;
		private string exteriorObject;

		internal double Min
		{
			get => min;
			set => SetProperty(ref min, value);
		}

		internal double Max
		{
			get => max;
			set => SetProperty(ref max, value);
		}

		internal string Object
		{
			get => exteriorObject;
			set => SetProperty(ref exteriorObject, value);
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

		internal void WriteXML(XElement parent)
		{
			parent.Add(new XElement("Coupler",
				new XElement("Min", Min),
				new XElement("Max", Max),
				new XElement("Object", Object),
				new XElement("CanUncoupler", "false"),
				new XElement("UncouplngBehaviour", "Emergency")
			));
		}

		internal void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("Coupler",
				new XElement("Min", Min),
				new XElement("Max", Max),
				new XElement("Object", Object)
				));
		}
	}
}
