using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using Prism.Mvvm;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The representation of the train.dat.
	/// </summary>
	internal class Train : BindableBase, ICloneable
	{
		private Handle handle;
		private Cab cab;
		private Device device;

		internal Handle Handle
		{
			get
			{
				return handle;
			}
			set
			{
				SetProperty(ref handle, value);
			}
		}

		internal Cab Cab
		{
			get
			{
				return cab;
			}
			set
			{
				SetProperty(ref cab, value);
			}
		}

		internal Device Device
		{
			get
			{
				return device;
			}
			set
			{
				SetProperty(ref device, value);
			}
		}

		internal ObservableCollection<Car> Cars;
		internal ObservableCollection<Coupler> Couplers;

		internal Train()
		{
			Handle = new Handle();
			Cab = new Cab();
			Device = new Device();
			Cars = new ObservableCollection<Car>();
			Couplers = new ObservableCollection<Coupler>();
		}

		public object Clone()
		{
			return new Train
			{
				Handle = (Handle)Handle.Clone(),
				Cab = (Cab)Cab.Clone(),
				Device = (Device)Device.Clone(),
				Cars = new ObservableCollection<Car>(Cars.Select(c => (Car)c.Clone())),
				Couplers = new ObservableCollection<Coupler>(Couplers.Select(c => (Coupler)c.Clone()))
			};
		}

		#region Handle

		internal void ApplyPowerNotchesToCar()
		{
			foreach (Car car in Cars)
			{
				for (int i = car.Delay.DelayPower.Count; i < Handle.PowerNotches; i++)
				{
					car.Delay.DelayPower.Add(new Delay.Entry());
				}

				for (int i = car.Delay.DelayPower.Count - 1; i >= Handle.PowerNotches; i--)
				{
					car.Delay.DelayPower.RemoveAt(i);
				}
			}

			foreach (MotorCar car in Cars.OfType<MotorCar>().ToArray())
			{
				for (int i = car.Acceleration.Entries.Count; i < Handle.PowerNotches; i++)
				{
					car.Acceleration.Entries.Add(new Acceleration.Entry());
				}

				for (int i = car.Acceleration.Entries.Count - 1; i >= Handle.PowerNotches; i--)
				{
					car.Acceleration.Entries.RemoveAt(i);
				}
			}
		}

		internal void ApplyBrakeNotchesToCar()
		{
			foreach (Car car in Cars)
			{
				for (int i = car.Delay.DelayBrake.Count; i < Handle.BrakeNotches; i++)
				{
					car.Delay.DelayBrake.Add(new Delay.Entry());
				}

				for (int i = car.Delay.DelayBrake.Count - 1; i >= Handle.BrakeNotches; i--)
				{
					car.Delay.DelayBrake.RemoveAt(i);
				}
			}
		}

		internal void ApplyLocoBrakeNotchesToCar()
		{
			foreach (Car car in Cars)
			{
				for (int i = car.Delay.DelayLocoBrake.Count; i < Handle.LocoBrakeNotches; i++)
				{
					car.Delay.DelayLocoBrake.Add(new Delay.Entry());
				}

				for (int i = car.Delay.DelayLocoBrake.Count - 1; i >= Handle.LocoBrakeNotches; i--)
				{
					car.Delay.DelayLocoBrake.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Acceleration

		private double GetDeceleration(MotorCar car, double velocity)
		{
			const double AccelerationDueToGravity = 9.80665;
			const double AirDensity = 1.22497705587732;

			velocity /= 3.6;
			double mass = car.Mass * 1000.0;
			double frontalArea = Cars.IndexOf(car) == 0 ? car.ExposedFrontalArea : car.UnexposedFrontalArea;

			double f = frontalArea * car.Performance.AerodynamicDragCoefficient * AirDensity / (2.0 * mass);
			double a = AccelerationDueToGravity * car.Performance.CoefficientOfRollingResistance + f * Math.Pow(velocity, 2.0);

			return a * 3.6;
		}

		private void DrawAccelerationCurve(System.Drawing.Graphics g, MotorCar car, Acceleration.Entry entry, bool selected)
		{
			// curve
			Point[] points = new Point[car.Acceleration.ImageWidth];

			for (int x = 0; x < car.Acceleration.ImageWidth; x++)
			{
				double velocity = car.Acceleration.XtoVelocity(x);
				double acceleration;

				if (car.Acceleration.Resistance)
				{
					acceleration = Math.Max(car.Acceleration.GetAcceleration(entry, velocity) - GetDeceleration(car, velocity), 0.0);
				}
				else
				{
					acceleration = car.Acceleration.GetAcceleration(entry, velocity);
				}

				int y = (int)Math.Round(car.Acceleration.AccelerationToY(acceleration));

				points[x] = new Point(x, y);
			}

			double hue;

			if (car.Acceleration.Entries.Count <= 1)
			{
				hue = 1.0;
			}
			else
			{
				hue = 0.5 * car.Acceleration.Entries.IndexOf(entry) / (car.Acceleration.Entries.Count - 1);
			}

			Color color = Utilities.GetColor(hue, selected);

			g.DrawLines(new Pen(color), points);

			// points
			{
				double v1 = entry.V1;
				double a1 = entry.A1;

				if (car.Acceleration.Resistance)
				{
					a1 -= GetDeceleration(car, v1);
				}

				int x1 = (int)Math.Round(car.Acceleration.VelocityToX(v1));
				int y1 = (int)Math.Round(car.Acceleration.AccelerationToY(a1));

				g.FillEllipse(new SolidBrush(color), new Rectangle(x1 - 2, y1 - 2, 5, 5));

				double v2 = entry.V2;
				double a2 = car.Acceleration.GetAcceleration(entry, v2);

				if (car.Acceleration.Resistance)
				{
					a2 -= GetDeceleration(car, v2);
				}

				int x2 = (int)Math.Round(car.Acceleration.VelocityToX(v2));
				int y2 = (int)Math.Round(car.Acceleration.AccelerationToY(a2));

				g.FillEllipse(new SolidBrush(color), new Rectangle(x2 - 2, y2 - 2, 5, 5));
			}
		}

		private void DrawDecelerationCurve(System.Drawing.Graphics g, MotorCar car)
		{
			if (!car.Acceleration.Resistance)
			{
				// curve
				Point[] points = new Point[car.Acceleration.ImageWidth];

				for (int x = 0; x < car.Acceleration.ImageWidth; x++)
				{
					double velocity = car.Acceleration.XtoVelocity(x);
					double acceleration = GetDeceleration(car, velocity);

					int y = (int)Math.Round(car.Acceleration.AccelerationToY(acceleration));

					points[x] = new Point(x, y);
				}

				g.DrawLines(Pens.DimGray, points);
			}
		}

		internal void DrawAccelerationImage(MotorCar car)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Bitmap image = new Bitmap(car.Acceleration.ImageWidth, car.Acceleration.ImageHeight);

			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);

			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.High;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.Black);

			Font font = new Font("MS UI Gothic", 7.0f);
			Pen grayPen = new Pen(Color.DimGray);
			Brush grayBrush = Brushes.DimGray;

			// vertical grid
			for (double v = 0.0; v < car.Acceleration.MaxVelocity; v += 10.0)
			{
				float x = (float)car.Acceleration.VelocityToX(v);
				g.DrawLine(grayPen, new PointF(x, 0.0f), new PointF(x, car.Acceleration.ImageHeight));
				g.DrawString(v.ToString("0", culture), font, grayBrush, new PointF(x, 1.0f));
			}

			// horizontal grid
			for (double a = 0.0; a < car.Acceleration.MaxAcceleration; a += 1.0)
			{
				float y = (float)car.Acceleration.AccelerationToY(a);
				g.DrawLine(grayPen, new PointF(0.0f, y), new PointF(car.Acceleration.ImageWidth, y));
				g.DrawString(a.ToString("0", culture), font, grayBrush, new PointF(1.0f, y));
			}

			DrawDecelerationCurve(g, car);

			foreach (Acceleration.Entry entry in car.Acceleration.Entries)
			{
				if (entry != car.Acceleration.SelectedEntry)
				{
					DrawAccelerationCurve(g, car, entry, false);
				}
			}

			DrawAccelerationCurve(g, car, car.Acceleration.SelectedEntry, true);

			car.Acceleration.Image = image;
		}

		#endregion
	}
}
