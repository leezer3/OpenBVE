using System;

namespace TrainManager.Motor
{
	/// <summary>A fuel tank containing a liquid fuel</summary>
    public class FuelTank
    {
	    /// <summary>The maximum level of fuel</summary>
	    public readonly double MaxLevel;
		/// <summary>The minimum level of fuel</summary>
		public readonly double MinLevel;
		/// <summary>Gets or sets the current fuel level</summary>
		/// <remarks>This function does not allow the fuel level to go above maximum or below minimum</remarks>
		public double CurrentLevel
		{
			get => currentLevel;
			set => currentLevel = Math.Min(MaxLevel, Math.Max(0, value));
		}
		/// <summary>Backing property storing the current fuel level</summary>
		private double currentLevel;

		public FuelTank(double maxLevel, double minLevel, double currentLevel)
		{
			MaxLevel = maxLevel;
			MinLevel = minLevel;
			CurrentLevel = currentLevel;
		}

		public FuelTank(double currentLevel)
		{
			MaxLevel = currentLevel;
			MinLevel = 0;
			CurrentLevel = currentLevel;
		}
	}
}
