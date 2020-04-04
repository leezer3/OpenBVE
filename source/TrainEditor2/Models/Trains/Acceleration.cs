using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using OpenBveApi.Units;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Acceleration section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Acceleration : BindableBase, ICloneable
	{
		internal class Entry : BindableBase, ICloneable
		{
			private Quantity.Acceleration a0;
			private Quantity.Acceleration a1;
			private Quantity.Velocity v1;
			private Quantity.Velocity v2;
			private double e;

			internal Quantity.Acceleration A0
			{
				get
				{
					return a0;
				}
				set
				{
					SetProperty(ref a0, value);
				}
			}

			internal Quantity.Acceleration A1
			{
				get
				{
					return a1;
				}
				set
				{
					SetProperty(ref a1, value);
				}
			}

			internal Quantity.Velocity V1
			{
				get
				{
					return v1;
				}
				set
				{
					SetProperty(ref v1, value);
				}
			}

			internal Quantity.Velocity V2
			{
				get
				{
					return v2;
				}
				set
				{
					SetProperty(ref v2, value);
				}
			}

			internal double E
			{
				get
				{
					return e;
				}
				set
				{
					SetProperty(ref e, value);
				}
			}

			internal Entry()
			{
				A0 = A1 = new Quantity.Acceleration(1.0, Unit.Acceleration.KilometerPerHourPerSecond);
				V1 = V2 = new Quantity.Velocity(25.0, Unit.Velocity.KilometerPerHour);
				E = 1.0;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		private int selectedEntryIndex;
		private Unit.Velocity velocityUnit;
		private Unit.Acceleration accelerationUnit;
		private Quantity.Velocity minVelocity;
		private Quantity.Velocity maxVelocity;
		private Quantity.Acceleration minAcceleration;
		private Quantity.Acceleration maxAcceleration;
		private Quantity.Velocity nowVelocity;
		private Quantity.Acceleration nowAcceleration;
		private bool resistance;
		private int imageWidth;
		private int imageHeight;
		private Bitmap image;

		private double FactorVelocity => ImageWidth / (MaxVelocity - MinVelocity);
		private double FactorAcceleration => -ImageHeight / (MaxAcceleration - MinAcceleration);

		internal ObservableCollection<Entry> Entries;

		internal int SelectedEntryIndex
		{
			get
			{
				return selectedEntryIndex;
			}
			set
			{
				SetProperty(ref selectedEntryIndex, value);
			}
		}

		internal Entry SelectedEntry
		{
			get
			{
				return Entries[SelectedEntryIndex];
			}
			set
			{
				Entries[SelectedEntryIndex] = value;
			}
		}

		internal Unit.Velocity VelocityUnit
		{
			get
			{
				return velocityUnit;
			}
			set
			{
				SetProperty(ref velocityUnit, value);

				minVelocity = minVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
				maxVelocity = maxVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
				nowVelocity = nowVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(NowVelocity)));
			}
		}

		internal Unit.Acceleration AccelerationUnit
		{
			get
			{
				return accelerationUnit;
			}
			set
			{
				SetProperty(ref accelerationUnit, value);

				minAcceleration = minAcceleration.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
				maxAcceleration = maxAcceleration.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
				nowAcceleration = nowAcceleration.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(NowAcceleration)));
			}
		}

		internal double MinVelocity
		{
			get
			{
				return minVelocity.Value;
			}
			set
			{
				SetProperty(ref minVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double MaxVelocity
		{
			get
			{
				return maxVelocity.Value;
			}
			set
			{
				SetProperty(ref maxVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double MinAcceleration
		{
			get
			{
				return minAcceleration.Value;
			}
			set
			{
				SetProperty(ref minAcceleration, new Quantity.Acceleration(value, AccelerationUnit));
			}
		}

		internal double MaxAcceleration
		{
			get
			{
				return maxAcceleration.Value;
			}
			set
			{
				SetProperty(ref maxAcceleration, new Quantity.Acceleration(value, AccelerationUnit));
			}
		}

		internal double NowVelocity
		{
			get
			{
				return nowVelocity.Value;
			}
			set
			{
				SetProperty(ref nowVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double NowAcceleration
		{
			get
			{
				return nowAcceleration.Value;
			}
			set
			{
				SetProperty(ref nowAcceleration, new Quantity.Acceleration(value, AccelerationUnit));
			}
		}

		internal bool Resistance
		{
			get
			{
				return resistance;
			}
			set
			{
				SetProperty(ref resistance, value);
			}
		}

		internal int ImageWidth
		{
			get
			{
				return imageWidth;
			}
			set
			{
				SetProperty(ref imageWidth, value);
			}
		}

		internal int ImageHeight
		{
			get
			{
				return imageHeight;
			}
			set
			{
				SetProperty(ref imageHeight, value);
			}
		}

		internal Bitmap Image
		{
			get
			{
				return image;
			}
			set
 			{
				SetProperty(ref image, value);
			}
		}

		internal Acceleration()
		{
			Entries = new ObservableCollection<Entry>();

			for (int i = 0; i < 8; i++)
			{
				Entries.Add(new Entry());
			}

			SelectedEntryIndex = 0;

			VelocityUnit = Unit.Velocity.KilometerPerHour;
			AccelerationUnit = Unit.Acceleration.KilometerPerHourPerSecond;
			MinVelocity = 0.0;
			MaxVelocity = 160.0;
			MinAcceleration = 0.0;
			MaxAcceleration = 4.0;
			NowVelocity = 0.0;
			NowAcceleration = 0.0;

			Resistance = false;

			ImageWidth = 576;
			ImageHeight = 670;
			Image = new Bitmap(ImageWidth, ImageHeight);
		}

		public object Clone()
		{
			Acceleration acceleration = (Acceleration)MemberwiseClone();
			acceleration.Entries = new ObservableCollection<Entry>(Entries.Select(e => (Entry)e.Clone()));
			acceleration.SelectedEntry = acceleration.Entries[Entries.IndexOf(SelectedEntry)];
			acceleration.Image = (Bitmap)Image.Clone();
			return acceleration;
		}

		internal Quantity.Velocity XtoVelocity(double x)
		{
			return new Quantity.Velocity(MinVelocity + x / FactorVelocity, VelocityUnit);
		}

		internal Quantity.Acceleration YtoAcceleration(double y)
		{
			return new Quantity.Acceleration(MinAcceleration + (y - ImageHeight) / FactorAcceleration, AccelerationUnit);
		}

		internal double VelocityToX(Quantity.Velocity v)
		{
			return (v - minVelocity).ToNewUnit(VelocityUnit).Value * FactorVelocity;
		}

		internal double AccelerationToY(Quantity.Acceleration a)
		{
			return ImageHeight + (a - minAcceleration).ToNewUnit(AccelerationUnit).Value * FactorAcceleration;
		}

		internal Quantity.Acceleration GetAcceleration(Entry entry, Quantity.Velocity velocity)
		{
			double v = velocity.ToDefaultUnit().Value;
			double a0 = entry.A0.ToDefaultUnit().Value;
			double a1 = entry.A1.ToDefaultUnit().Value;
			double v1 = entry.V1.ToDefaultUnit().Value;
			double v2 = entry.V2.ToDefaultUnit().Value;
			double e = entry.E;
			double a;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (v == 0.0)
			{
				a = a0;
			}
			else if (v < v1)
			{
				a = a0 + (a1 - a0) * v / v1;
			}
			else if (v < v2)
			{
				a = v1 * a1 / v;
			}
			else
			{
				a = v1 * a1 * Math.Pow(v2, e - 1.0) * Math.Pow(v, -e);
			}

			return new Quantity.Acceleration(a);
		}

		internal void ZoomIn()
		{
			Utilities.ZoomIn(ref minVelocity, ref maxVelocity);
			Utilities.ZoomIn(ref minAcceleration, ref maxAcceleration);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
		}

		internal void ZoomOut()
		{
			Utilities.ZoomOut(ref minVelocity, ref maxVelocity);
			Utilities.ZoomOut(ref minAcceleration, ref maxAcceleration);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
		}

		internal void Reset()
		{
			Utilities.Reset(new Quantity.Velocity(0.5 * 160.0, VelocityUnit), ref minVelocity, ref maxVelocity);
			Utilities.Reset(new Quantity.Acceleration(0.5 * 4.0, AccelerationUnit), ref minAcceleration, ref maxAcceleration);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
		}

		internal void MoveLeft()
		{
			Utilities.MoveNegative(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
		}

		internal void MoveRight()
		{
			Utilities.MovePositive(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
		}

		internal void MoveBottom()
		{
			Utilities.MoveNegative(ref minAcceleration, ref maxAcceleration);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
		}

		internal void MoveTop()
		{
			Utilities.MovePositive(ref minAcceleration, ref maxAcceleration);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinAcceleration)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxAcceleration)));
		}

		internal void MouseMove(InputEventModel.EventArgs position)
		{
			NowVelocity = 0.01 * Math.Round(100.0 * XtoVelocity(position.X).Value);
			NowAcceleration = 0.01 * Math.Round(100.0 * YtoAcceleration(position.Y).Value);
		}
	}
}
