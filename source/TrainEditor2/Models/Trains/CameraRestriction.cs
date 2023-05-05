using System;
using OpenBveApi.World;
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

		private Quantity.Length forwards;
		private Quantity.Length backwards;
		private Quantity.Length left;
		private Quantity.Length right;
		private Quantity.Length up;
		private Quantity.Length down;

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

		internal Quantity.Length Forwards
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

		internal Quantity.Length Backwards
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

		internal Quantity.Length Left
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

		internal Quantity.Length Right
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

		internal Quantity.Length Up
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

		internal Quantity.Length Down
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
