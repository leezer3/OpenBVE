//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
			this.car = car;
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
					case KujuTokenID.Variable1Controlled:
						double value = Math.Abs(car.CurrentSpeed / 1000 / car.DrivingWheels[0].Radius / Math.PI * 5);
						for (int i = volumePoints.Length - 1; i >= 0; i--)
						{
							if (value >= volumePoints[i].Item1)
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
