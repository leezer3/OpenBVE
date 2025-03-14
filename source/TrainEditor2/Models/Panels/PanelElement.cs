using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Prism.Mvvm;

namespace TrainEditor2.Models.Panels
{
	internal abstract class PanelElement : BindableBase, ICloneable
	{
		protected double locationX;
		protected double locationY;
		protected int layer;

		internal double LocationX
		{
			get
			{
				return locationX;
			}
			set
			{
				SetProperty(ref locationX, value);
			}
		}

		internal double LocationY
		{
			get
			{
				return locationY;
			}
			set
			{
				SetProperty(ref locationY, value);
			}
		}

		internal int Layer
		{
			get
			{
				return layer;
			}
			set
			{
				SetProperty(ref layer, value);
			}
		}

		internal PanelElement()
		{
			LocationX = 0.0;
			LocationY = 0.0;
			Layer = 0;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>Writes the element to a Panel2.cfg file</summary>
		/// <param name="fileName">The output filename</param>
		/// <param name="builder">The stringbuilder</param>
		public abstract void WriteCfg(string fileName, StringBuilder builder);

		/// <summary>Writes the element to a Panel.xml file</summary>
		/// <param name="fileName">The output filename</param>
		/// <param name="parent">The parent element</param>
		public abstract void WriteXML(string fileName, XElement parent);

		internal static void WriteKey(StringBuilder builder, string key, params string[] values)
		{
			if (values.All(string.IsNullOrEmpty))
			{
				return;
			}

			builder.AppendLine($"{key} = {string.Join(", ", values)}");
		}

		internal static void WriteKey(StringBuilder builder, string key, params int[] values)
		{
			WriteKey(builder, key, values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		internal static void WriteKey(StringBuilder builder, string key, params double[] values)
		{
			WriteKey(builder, key, values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}
}
