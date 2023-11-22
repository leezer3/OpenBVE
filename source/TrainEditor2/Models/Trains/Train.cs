using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using OpenBveApi.World;
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
		private Device device;
		private int initialDriverCar;

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

		internal int InitialDriverCar
		{
			get
			{
				return initialDriverCar;
			}
			set
			{
				SetProperty(ref initialDriverCar, value);
			}
		}

		internal ObservableCollection<Car> Cars;
		internal ObservableCollection<Coupler> Couplers;

		internal Train()
		{
			Handle = new Handle();
			Device = new Device();
			InitialDriverCar = 0;
			Cars = new ObservableCollection<Car>();
			Couplers = new ObservableCollection<Coupler>();
		}

		public object Clone()
		{
			return new Train
			{
				Handle = (Handle)Handle.Clone(),
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
				for (int i = car.Delay.Power.Count; i < Handle.PowerNotches; i++)
				{
					car.Delay.Power.Add(new Delay.Entry());
				}

				for (int i = car.Delay.Power.Count - 1; i >= Handle.PowerNotches; i--)
				{
					car.Delay.Power.RemoveAt(i);
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
				for (int i = car.Delay.Brake.Count; i < Handle.BrakeNotches; i++)
				{
					car.Delay.Brake.Add(new Delay.Entry());
				}

				for (int i = car.Delay.Brake.Count - 1; i >= Handle.BrakeNotches; i--)
				{
					car.Delay.Brake.RemoveAt(i);
				}
			}
		}

		internal void ApplyLocoBrakeNotchesToCar()
		{
			foreach (Car car in Cars)
			{
				for (int i = car.Delay.LocoBrake.Count; i < Handle.LocoBrakeNotches; i++)
				{
					car.Delay.LocoBrake.Add(new Delay.Entry());
				}

				for (int i = car.Delay.LocoBrake.Count - 1; i >= Handle.LocoBrakeNotches; i--)
				{
					car.Delay.LocoBrake.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Acceleration

		private Quantity.Acceleration GetDeceleration(MotorCar car, Quantity.Velocity velocity)
		{
			const double AccelerationDueToGravity = 9.80665;
			const double AirDensity = 1.22497705587732;

			double v = velocity.ToDefaultUnit().Value;
			double mass = car.Mass.ToDefaultUnit().Value;
			Quantity.Area frontalArea = Cars.IndexOf(car) == 0 ? car.ExposedFrontalArea : car.UnexposedFrontalArea;

			double f = frontalArea.ToDefaultUnit().Value * car.Performance.AerodynamicDragCoefficient * AirDensity / (2.0 * mass);
			double a = AccelerationDueToGravity * car.Performance.CoefficientOfRollingResistance + f * Math.Pow(v, 2.0);

			return new Quantity.Acceleration(a);
		}

		private void DrawAccelerationCurve(System.Drawing.Graphics g, MotorCar car, Acceleration.Entry entry, bool selected)
		{
			// curve
			Point[] points = new Point[car.Acceleration.ImageWidth];

			for (int x = 0; x < car.Acceleration.ImageWidth; x++)
			{
				Quantity.Velocity velocity = car.Acceleration.XtoVelocity(x);
				Quantity.Acceleration acceleration = car.Acceleration.GetAcceleration(entry, velocity);

				if (car.Acceleration.Resistance)
				{
					acceleration -= GetDeceleration(car, velocity);

					if (acceleration.ToDefaultUnit().Value < 0.0)
					{
						acceleration = new Quantity.Acceleration();
					}
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
				Quantity.Velocity v1 = entry.V1;
				Quantity.Acceleration a1 = entry.A1;

				if (car.Acceleration.Resistance)
				{
					a1 -= GetDeceleration(car, v1);
				}

				int x1 = (int)Math.Round(car.Acceleration.VelocityToX(v1));
				int y1 = (int)Math.Round(car.Acceleration.AccelerationToY(a1));

				g.FillEllipse(new SolidBrush(color), new Rectangle(x1 - 2, y1 - 2, 5, 5));

				Quantity.Velocity v2 = entry.V2;
				Quantity.Acceleration a2 = car.Acceleration.GetAcceleration(entry, v2);

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
					Quantity.Velocity velocity = car.Acceleration.XtoVelocity(x);
					Quantity.Acceleration acceleration = GetDeceleration(car, velocity);

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
				float x = (float)car.Acceleration.VelocityToX(new Quantity.Velocity(v, car.Acceleration.VelocityUnit));
				g.DrawLine(grayPen, new PointF(x, 0.0f), new PointF(x, car.Acceleration.ImageHeight));
				g.DrawString(v.ToString("0", culture), font, grayBrush, new PointF(x, 1.0f));
			}

			// horizontal grid
			for (double a = 0.0; a < car.Acceleration.MaxAcceleration; a += 1.0)
			{
				float y = (float)car.Acceleration.AccelerationToY(new Quantity.Acceleration(a, car.Acceleration.AccelerationUnit));
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
