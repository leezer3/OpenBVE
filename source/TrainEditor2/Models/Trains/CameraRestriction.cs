using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class CameraRestriction : BindableBase, ICloneable
	{
		private bool definedForwards;
		private bool definedBackwards;
		private bool definedLeft;
		private bool definedRight;
		private bool definedUp;
		private bool definedDown;

		private double forwards;
		private double backwards;
		private double left;
		private double right;
		private double up;
		private double down;

		internal bool DefinedForwards
		{
			get
			{
				return definedForwards;
			}
			set
			{
				SetProperty(ref definedForwards, value);
			}
		}

		internal bool DefinedBackwards
		{
			get
			{
				return definedBackwards;
			}
			set
			{
				SetProperty(ref definedBackwards, value);
			}
		}

		internal bool DefinedLeft
		{
			get
			{
				return definedLeft;
			}
			set
			{
				SetProperty(ref definedLeft, value);
			}
		}

		internal bool DefinedRight
		{
			get
			{
				return definedRight;
			}
			set
			{
				SetProperty(ref definedRight, value);
			}
		}

		internal bool DefinedUp
		{
			get
			{
				return definedUp;
			}
			set
			{
				SetProperty(ref definedUp, value);
			}
		}

		internal bool DefinedDown
		{
			get
			{
				return definedDown;
			}
			set
			{
				SetProperty(ref definedDown, value);
			}
		}

		internal double Forwards
		{
			get
			{
				return forwards;
			}
			set
			{
				SetProperty(ref forwards, value);
			}
		}

		internal double Backwards
		{
			get
			{
				return backwards;
			}
			set
			{
				SetProperty(ref backwards, value);
			}
		}

		internal double Left
		{
			get
			{
				return left;
			}
			set
			{
				SetProperty(ref left, value);
			}
		}

		internal double Right
		{
			get
			{
				return right;
			}
			set
			{
				SetProperty(ref right, value);
			}
		}

		internal double Up
		{
			get
			{
				return up;
			}
			set
			{
				SetProperty(ref up, value);
			}
		}

		internal double Down
		{
			get
			{
				return down;
			}
			set
			{
				SetProperty(ref down, value);
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
