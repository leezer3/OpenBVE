using System;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	class Cutoff : AbstractReverser
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
		public new double Actual
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
					return absCurrent * ReverseMax;
				}

				return Current * ForwardMax;
			}
		}
		public Cutoff(TrainBase Train, int forwardsMax, int reverseMax, int ineffectiveRange) : base(Train)
		{
			ForwardMax = forwardsMax;
			ReverseMax = reverseMax;
			IneffectiveRange = ineffectiveRange;
			Current = 0;
		}

		public override string CurrentNotchDescription => Current + "%";

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
	}
}
