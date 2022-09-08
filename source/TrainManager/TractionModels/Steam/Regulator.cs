namespace TrainManager.TractionModels.Steam
{
	public class Regulator
	{
		private readonly SteamEngine Engine;
		/// <summary>The current setting</summary>
		public double Current;
		/// <summary>The maximum setting</summary>
		private readonly double Max;
		/// <summary>The current ratio</summary>
		public double Ratio => Current * Max;

		public Regulator(SteamEngine engine, double max)
		{
			Engine = engine;
			Max = max;
		}
	}
}
