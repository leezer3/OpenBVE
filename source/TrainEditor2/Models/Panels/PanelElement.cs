using System;
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
	}
}
