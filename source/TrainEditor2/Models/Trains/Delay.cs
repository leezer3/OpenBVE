﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Delay section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Delay : ICloneable
	{
		internal class Entry : BindableBase, ICloneable
		{
			private double up;
			private double down;

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

		internal ObservableCollection<Entry> Power;
		internal ObservableCollection<Entry> Brake;
		internal ObservableCollection<Entry> LocoBrake;

		internal Delay()
		{
			Power = new ObservableCollection<Entry>();
			Brake = new ObservableCollection<Entry>();
			LocoBrake = new ObservableCollection<Entry>();

			for (int i = 0; i < 8; i++)
			{
				Power.Add(new Entry());
				Brake.Add(new Entry());
				LocoBrake.Add(new Entry());
			}
		}

		public object Clone()
		{
			return new Delay
			{
				Power = new ObservableCollection<Entry>(Power.Select(x => (Entry)x.Clone())),
				Brake = new ObservableCollection<Entry>(Brake.Select(x => (Entry)x.Clone())),
				LocoBrake = new ObservableCollection<Entry>(LocoBrake.Select(x => (Entry)x.Clone()))
			};
		}
	}
}
