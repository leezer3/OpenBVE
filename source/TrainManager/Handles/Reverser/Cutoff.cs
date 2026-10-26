//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using OpenBveApi.Colors;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	public class Cutoff : AbstractReverser
	{
		/// <summary>The maximum forwards cutoff setting</summary>
		public readonly int ForwardMax;
		/// <summary>The maximum reverse cutoff setting</summary>
		public readonly int ReverseMax;
		/// <summary>The ineffective range</summary>
		public readonly double IneffectiveRange;
		/// <summary>The actual used cutoff ratio</summary>
		public double Ratio
		{
			get
			{
				double absCurrent = Math.Abs(Driver);
				if (absCurrent < IneffectiveRange)
				{
					return 0;
				}
				if (Driver != absCurrent)
				{
					return (double)Driver / ReverseMax;
				}

				return (double)Driver / ForwardMax;
			}
		}

		public new int MaximumNotch
		{
			get
			{
				double absCurrent = Math.Abs(Driver);
				if (Driver != absCurrent)
				{
					return ReverseMax;
				}

				return ForwardMax;
			}
		}

		public new int MaximumDriverNotch
		{
			get
			{
				double absCurrent = Math.Abs(Driver);
				if (Driver != absCurrent)
				{
					return ReverseMax;
				}

				return ForwardMax;
			}
		}


		public Cutoff(TrainBase Train, int forwardsMax, int reverseMax, int ineffectiveRange) : base(Train)
		{
			ForwardMax = forwardsMax;
			ReverseMax = reverseMax;
			IneffectiveRange = ineffectiveRange;
			Driver = 0;
		}

		public override void ApplyState(int Value, bool Relative)
		{
			if (Value != 0)
			{
				if (Relative)
				{
					Driver = Value != Math.Abs(Value) ? Math.Max(ReverseMax, Driver + Value) : Math.Min(ForwardMax, Driver + Value);
				}
				else
				{
					Driver = Math.Min(ForwardMax, Math.Max(ReverseMax, Value));
				}

				Actual = Driver;
				if (baseTrain.Plugin != null)
				{
					baseTrain.Plugin.UpdateReverser();
				}
				TrainManagerBase.currentHost.AddBlackBoxEntry();
			}
		}

		/// <summary>Gets the description string for this notch</summary>
		/// <param name="color">The on-screen display color</param>
		/// <returns>The notch description</returns>
		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (Actual < 0)
			{
				color = MessageColor.Orange;
			}
			else if (Actual > 0)
			{
				color = MessageColor.Blue;
			}


			return Driver + "%";
		}
	}
}
