using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
			private double a0;
			private double a1;
			private double v1;
			private double v2;
			private double e;

			internal double A0
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

			internal double A1
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

			internal double V1
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

			internal double V2
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
				A0 = A1 = 1.0;
				V1 = V2 = 25.0;
				E = 1.0;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		private int selectedEntryIndex;
		private double minVelocity;
		private double maxVelocity;
		private double minAcceleration;
		private double maxAcceleration;
		private double nowVelocity;
		private double nowAcceleration;
		private bool resistance;
		private int imageWidth;
		private int imageHeight;
		private Bitmap image;

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

		internal double MinVelocity
		{
			get
			{
				return minVelocity;
			}
			set
			{
				SetProperty(ref minVelocity, value);
			}
		}

		internal double MaxVelocity
		{
			get
			{
				return maxVelocity;
			}
			set
			{
				SetProperty(ref maxVelocity, value);
			}
		}

		internal double MinAcceleration
		{
			get
			{
				return minAcceleration;
			}
			set
			{
				SetProperty(ref minAcceleration, value);
			}
		}

		internal double MaxAcceleration
		{
			get
			{
				return maxAcceleration;
			}
			set
			{
				SetProperty(ref maxAcceleration, value);
			}
		}

		internal double NowVelocity
		{
			get
			{
				return nowVelocity;
			}
			set
			{
				SetProperty(ref nowVelocity, value);
			}
		}

		internal double NowAcceleration
		{
			get
			{
				return nowAcceleration;
			}
			set
			{
				SetProperty(ref nowAcceleration, value);
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

		internal double XtoVelocity(double x)
		{
			double factorVelocity = ImageWidth / (MaxVelocity - MinVelocity);
			return MinVelocity + x / factorVelocity;
		}

		internal double YtoAcceleration(double y)
		{
			double factorAcceleration = -ImageHeight / (MaxAcceleration - MinAcceleration);
			return MinAcceleration + (y - ImageHeight) / factorAcceleration;
		}

		internal double VelocityToX(double v)
		{
			double factorVelocity = ImageWidth / (MaxVelocity - MinVelocity);
			return (v - MinVelocity) * factorVelocity;
		}

		internal double AccelerationToY(double a)
		{
			double factorAcceleration = -ImageHeight / (MaxAcceleration - MinAcceleration);
			return ImageHeight + (a - MinAcceleration) * factorAcceleration;
		}

		internal double GetAcceleration(Entry entry, double velocity)
		{
			velocity /= 3.6;
			double a0 = entry.A0 / 3.6;
			double a1 = entry.A1 / 3.6;
			double v1 = entry.V1 / 3.6;
			double v2 = entry.V2 / 3.6;
			double e = entry.E;
			double a;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (velocity == 0.0)
			{
				a = a0;
			}
			else if (velocity < v1)
			{
				a = a0 + (a1 - a0) * velocity / v1;
			}
			else if (velocity < v2)
			{
				a = v1 * a1 / velocity;
			}
			else
			{
				a = v1 * a1 * Math.Pow(v2, e - 1.0) * Math.Pow(velocity, -e);
			}

			return a * 3.6;
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
			Utilities.Reset(0.5 * 160.0, ref minVelocity, ref maxVelocity);
			Utilities.Reset(0.5 * 4.0, ref minAcceleration, ref maxAcceleration);

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
			NowVelocity = 0.01 * Math.Round(100.0 * XtoVelocity(position.X));
			NowAcceleration = 0.01 * Math.Round(100.0 * YtoAcceleration(position.Y));
		}
	}
}
