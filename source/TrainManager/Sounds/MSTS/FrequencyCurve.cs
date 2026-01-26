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
