using System;
using OpenBveApi.World;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	internal class Jerk : BindableBase, ICloneable
	{
		internal class Entry : BindableBase, ICloneable
		{
			private Quantity.Jerk up;
			private Quantity.Jerk down;

			internal Quantity.Jerk Up
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

			internal Quantity.Jerk Down
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

			internal Entry()
			{
				Up = new Quantity.Jerk(1000.0, Unit.Jerk.CentimeterPerSecondCubed);
				Down = new Quantity.Jerk(1000.0, Unit.Jerk.CentimeterPerSecondCubed);
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		private Entry power;
		private Entry brake;

		internal Entry Power
		{
			get
			{
				return power;
			}
			set
			{
				SetProperty(ref power, value);
			}
		}

		internal Entry Brake
		{
			get
			{
				return brake;
			}
			set
			{
				SetProperty(ref brake, value);
			}
		}

		internal Jerk()
		{
			Power = new Entry();
			Brake = new Entry();
		}

		public object Clone()
		{
			return new Jerk
			{
				Power = (Entry)Power.Clone(),
				Brake = (Entry)Brake.Clone()
			};
		}
	}
}
