using System;
using OpenBve.Formats.MsTs;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public class MsTsFrequencyCurve
	{
		private readonly CarBase car;
		private readonly Tuple<double, double>[] frequencyPoints;

		private readonly KujuTokenID controller;

		public MsTsFrequencyCurve(CarBase car, KujuTokenID controller, Tuple<double, double>[] points)
		{
			this.car = car;
			this.controller = controller;
			frequencyPoints = new Tuple<double, double>[points.Length];
			for (int i = 0; i < frequencyPoints.Length; i++)
			{
				frequencyPoints[i] = new Tuple<double, double>(points[i].Item1, points[i].Item2 / 11025); // MSTS base sound frequency is 11025hz, convert to percentage
			}
		}

		public double Pitch
		{
			get
			{
				switch (controller)
				{
					case KujuTokenID.SpeedControlled:
						for (int i = frequencyPoints.Length - 1; i >= 0; i--)
						{
							if (Math.Abs(car.CurrentSpeed) >= frequencyPoints[i].Item1)
							{
								return frequencyPoints[i].Item2;
							}
						}
						break;
					case KujuTokenID.Variable2Controlled:
						for (int i = frequencyPoints.Length - 1; i >= 0; i--)
						{
							if (car.TractionModel.CurrentPower >= frequencyPoints[i].Item1)
							{
								return frequencyPoints[i].Item2;
							}
						}
						break;

				}
				return 0;
			}
		}
	}
}
