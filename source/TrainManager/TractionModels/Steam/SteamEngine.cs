﻿using System;
using TrainManager.Car;
using TrainManager.Handles;

namespace TrainManager.TractionModels.Steam
{
	/// <summary>The traction model for a simplistic steam engine</summary>
	public class SteamEngine
	{
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase Car;

		public readonly Boiler Boiler;

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
			/*
			 * Cutoff
			 *			75% max forwards
			 *			50% max reverse
			 *			10% around zero where cutoff is ineffective (due to standing resistance etc.)
			 */
			Car.baseTrain.Handles.Reverser = new Cutoff(Car.baseTrain, 75, -50, 10);
		}

		private double lastTrackPosition;

		public void Update(double timeElapsed)
		{
			// update the boiler pressure and associated gubbins first
			Boiler.Update(timeElapsed);
			// get the distance travelled & convert to piston strokes
			CylinderChest.Update(timeElapsed, Car.FrontAxle.Follower.TrackPosition - lastTrackPosition);
			lastTrackPosition = Car.FrontAxle.Follower.TrackPosition;

		}
	}
}
