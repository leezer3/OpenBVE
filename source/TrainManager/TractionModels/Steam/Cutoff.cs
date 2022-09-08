using System;

namespace TrainManager.TractionModels.Steam
{
	/// <summary>Provides cutoff modelling</summary>
	public class Cutoff
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The maximum forwards cutoff setting</summary>
		public readonly double ForwardMax;
		/// <summary>The maximum reverse cutoff setting</summary>
		public readonly double ReverseMax;
		/// <summary>The ineffective range</summary>
		public readonly double IneffectiveRange;
		/// <summary>The current cutoff setting</summary>
		public double Current;
		/// <summary>The current cuttoff ratio</summary>
		public double Ratio
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

		public Cutoff(SteamEngine engine, double forwardMax, double reverseMax, double ineffectiveRange)
		{
			Engine = engine;
			ForwardMax = forwardMax;
			ReverseMax = reverseMax;
			IneffectiveRange = ineffectiveRange;
		}


	}
}
