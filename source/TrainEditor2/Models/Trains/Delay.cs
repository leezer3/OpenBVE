﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using TrainEditor2.Extensions;

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
				get => up;
				set => SetProperty(ref up, value);
			}

			internal double Down
			{
				get => down;
				set => SetProperty(ref down, value);
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal ObservableCollection<Entry> DelayPower;
		internal ObservableCollection<Entry> DelayBrake;
		internal ObservableCollection<Entry> DelayLocoBrake;
		internal ObservableCollection<Entry> DelayElectricBrake;

		internal Delay()
		{
			DelayPower = new ObservableCollection<Entry>();
			DelayBrake = new ObservableCollection<Entry>();
			DelayLocoBrake = new ObservableCollection<Entry>();
			DelayElectricBrake = new ObservableCollection<Entry>();

			for (int i = 0; i < 8; i++)
			{
				DelayPower.Add(new Entry());
				DelayBrake.Add(new Entry());
				DelayLocoBrake.Add(new Entry());
				DelayElectricBrake.Add(new Entry());
			}
		}

		public object Clone()
		{
			return new Delay
			{
				DelayPower = new ObservableCollection<Entry>(DelayPower.Select(x => (Entry)x.Clone())),
				DelayBrake = new ObservableCollection<Entry>(DelayBrake.Select(x => (Entry)x.Clone())),
				DelayLocoBrake = new ObservableCollection<Entry>(DelayLocoBrake.Select(x => (Entry)x.Clone())),
				DelayElectricBrake = new ObservableCollection<Entry>(DelayElectricBrake.Select(x => (Entry)x.Clone()))
			};
		}
	}
}
