using System;
using TrainManager.Car;

namespace TrainManager.TractionModels.Steam
{
	/// <summary>The traction model for a simplistic steam engine</summary>
	public class SteamEngine
	{
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase Car;

		public readonly Boiler Boiler;

		public readonly Cutoff Cutoff;

		public readonly Regulator Regulator;

		public readonly CylinderChest CylinderChest;

		public double PowerOutput;

		public SteamEngine(CarBase car)
		{
			Car = car;
			/* todo: generic parameters- load from config
			 * Fudged average numbers here at the minute, based upon a hypothetical large tender loco
			 *
			 * Boiler: 
			 *			2000L starting level
			 *			3000L capacity
			 *			200psi starting pressure
			 *			240psi absolute max pressure
			 *			220psi blowoff pressure
			 *			120psi minimum working pressure
			 *			1L water ==> 4.15psi steam
			 */
			Boiler = new Boiler(this, 2000, 3000, 200, 240, 220, 120, 4.15);
			/*
			 * Cutoff
			 *			75% max forwards
			 *			50% max reverse
			 *			10% around zero where cutoff is ineffective (due to standing resistance etc.)
			 */
			Cutoff = new Cutoff(this, 0.75, -0.5, 0.1);
			/*
			 * Regulator
			 *			100% max, assuming percentage based
			 *			TODO: should convert to slightly more generic, maybe actually use the Train.Handles
			 */
			Regulator = new Regulator(this, 100);
			/*
			 * Cylinder Chest
			 *			5psi standing pressure loss (leakage etc.)
			 *			20psi base stroke pressure, before reduction due to regulator / cutoff
			 */
			CylinderChest = new CylinderChest(this, 5, 20);
		}

		private double lastTrackPosition;

		public void Update(double timeElapsed)
		{
			// update the boiler pressure and associated gubbins first
			Boiler.Update(timeElapsed);
			// get the distance travelled & convert to piston strokes
			double distanceTravelled = Car.FrontAxle.Follower.TrackPosition - lastTrackPosition;
			double numberOfStrokes = Math.Abs(distanceTravelled) / Car.FrontAxle.WheelRadius;
			// drop the steam pressure appropriately
			Boiler.SteamPressure -= numberOfStrokes * Cutoff.Ratio * CylinderChest.PressureUse;
			lastTrackPosition = Car.FrontAxle.Follower.TrackPosition;
		}
	}
}
