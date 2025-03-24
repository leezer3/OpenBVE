using System;
using System.Text;
using System.Xml.Linq;
using OpenBveApi.Math;
using Prism.Mvvm;

namespace TrainEditor2.Models.Panels
{
	internal abstract class PanelElement : BindableBase, ICloneable
	{
		protected Vector2 location;
		protected int layer;

		internal Vector2 Location
		{
			get
			{
				return location;
			}
			set
			{
				SetProperty(ref location, value);
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
			Location = Vector2.Null;
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
	}
}
