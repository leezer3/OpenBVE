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
			// todo: generic parameters- load from config
			Boiler = new Boiler(this, 100, 200, 200, 240, 1, 120);
			Cutoff = new Cutoff(this, 0.75, -0.5, 0.1);
			Regulator = new Regulator(this, 100);
			CylinderChest = new CylinderChest();
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
