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
		/// <summary>The current cutoff setting</summary>
		public int Current;
		/// <summary>The actual used cuttoff ratio</summary>
		public override double Ratio
		{
			get
			{
				double absCurrent = Math.Abs(Current);
				if (absCurrent < IneffectiveRange)
				{
					return 0;
				}
				if (Current != absCurrent)
				{
					return (double)Current / ReverseMax;
				}

				return (double)Current / ForwardMax;
			}
		}

		public new int MaximumNotch
		{
			get
			{
				double absCurrent = Math.Abs(Current);
				if (Current != absCurrent)
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
				double absCurrent = Math.Abs(Current);
				if (Current != absCurrent)
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
			Current = 0;
		}
		
		public override void ApplyState(int Value, bool Relative)
		{
			if (Value != 0)
			{
				if (Relative)
				{
					Current = Value != Math.Abs(Value) ? Math.Max(ReverseMax, Current + Value) : Math.Min(ForwardMax, Current + Value);
				}
				else
				{
					Current = Math.Min(ForwardMax, Math.Max(ReverseMax, Value));
				}
				if (baseTrain.Plugin != null)
				{
					baseTrain.Plugin.UpdateReverser();
				}
				TrainManagerBase.currentHost.AddBlackBoxEntry();
			}
		}

		public override void Update()
		{
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


			return Current + "%";
		}
	}
}
