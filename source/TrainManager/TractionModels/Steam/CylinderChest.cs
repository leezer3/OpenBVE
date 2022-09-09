using System;
using TrainManager.Handles;

namespace TrainManager.TractionModels.Steam
{
	public class CylinderChest
	{
		/// <summary>Holds a reference to the base engine</summary>
		public readonly SteamEngine Engine;
		/// <summary>The standing pressure loss through a stroke</summary>
		public readonly double StandingPressureLoss;
		/// <summary>The base pressure used by a stroke</summary>
		public readonly double BaseStrokePressure;
		/// <summary>The pressure used by a single stroke</summary>
		public double PressureUse => Engine.Regulator.Ratio * Engine.Car.baseTrain.Handles.Reverser.Actual * (StandingPressureLoss + BaseStrokePressure * Engine.Car.baseTrain.Handles.Reverser.Actual);
		/// <summary>The cylinder cocks</summary>
		public CylinderCocks CylinderCocks;

		public CylinderChest(SteamEngine engine, double standingPressureLoss, double baseStrokePressure)
		{
			Engine = engine;
			StandingPressureLoss = standingPressureLoss;
			BaseStrokePressure = baseStrokePressure;
			// 5psi / s loss when the cylinder cocks are open
			CylinderCocks = new CylinderCocks(Engine, 5);
		}

		public void Update(double timeElapsed, double distanceTravelled)
		{
			double numberOfStrokes = Math.Abs(distanceTravelled) / Engine.Car.FrontAxle.WheelRadius;
			// drop the steam pressure appropriately
			Engine.Boiler.SteamPressure -= numberOfStrokes * Engine.Car.baseTrain.Handles.Reverser.Actual * PressureUse;
			CylinderCocks.Update(timeElapsed);
		}
	}
}
