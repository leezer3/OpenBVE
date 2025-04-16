using OpenBve.Formats.MsTs;
using System;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public class MsTsVolumeCurve
	{
		private readonly CarBase car;
		private readonly Tuple<double, double>[] volumePoints;

		private readonly KujuTokenID controller;

		public MsTsVolumeCurve(CarBase car, KujuTokenID controller, Tuple<double, double>[] points)
		{
			this.car = car  as CarBase;
			this.controller = controller;
			volumePoints = points;
		}

		public double Volume
		{
			get
			{
				switch (controller)
				{
					case KujuTokenID.SpeedControlled:
						for (int i = volumePoints.Length - 1; i >= 0; i--)
						{
							if (Math.Abs(car.CurrentSpeed) >= volumePoints[i].Item1)
							{
								return volumePoints[i].Item2;
							}
						}
						break;
					case KujuTokenID.Variable2Controlled:
						for (int i = volumePoints.Length - 1; i >= 0; i--)
						{
							if (car.TractionModel.CurrentPower >= volumePoints[i].Item1)
							{
								return volumePoints[i].Item2;
							}
						}
						break;
				}
				return 0;
			}
		}
	}
}
