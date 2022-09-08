namespace TrainManager.TractionModels.Steam
{
	public class CylinderChest
	{
		public readonly SteamEngine Engine;
		/// <summary>The standing pressure loss through a stroke</summary>
		public readonly double StandingPressureLoss;
		/// <summary>The base pressure used by a stroke</summary>
		public readonly double BaseStrokePressure;
		/// <summary>The pressure used by a single stroke</summary>
		public double PressureUse => Engine.Regulator.Ratio * Engine.Cutoff.Ratio * (StandingPressureLoss + BaseStrokePressure * Engine.Cutoff.Ratio);

		public CylinderChest(SteamEngine engine, double standingPressureLoss, double baseStrokePressure)
		{
			Engine = engine;
			StandingPressureLoss = standingPressureLoss;
			BaseStrokePressure = baseStrokePressure;
		}
	}
}
