using System;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using RouteManager2;
using TrainManager.Trains;
using TrackFollowingObject = TrainManager.Trains.TrackFollowingObject;

namespace TrainManager
{
	/// <summary>The base train manager class</summary>
	public abstract class TrainManagerBase
	{
		internal static HostInterface currentHost;
		internal static BaseRenderer Renderer;
		public static CurrentRoute CurrentRoute;
		internal static FileSystem FileSystem;
		public static bool Toppling;
		public static bool Derailments;
		internal static Random RandomNumberGenerator = new Random();

		/// <summary>The list of trains available in the simulation.</summary>
		public TrainBase[] Trains = { };
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		public static TrainBase PlayerTrain = null;
		/// <summary>The list of TrackFollowingObject available on other tracks in the simulation.</summary>
		public AbstractTrain[] TFOs = { };
		/// <summary>Stores a reference to the current options</summary>
		internal static BaseOptions CurrentOptions;
		/// <summary>Stores the plugin error message string, or a null reference if no error encountered</summary>
		public static string PluginError;

		protected TrainManagerBase(HostInterface host, BaseRenderer renderer, BaseOptions Options, FileSystem fileSystem)
		{
			currentHost = host;
			Renderer = renderer;
			CurrentOptions = Options;
			FileSystem = fileSystem;
		}

		/// <summary>Un-derails all trains within the simulation</summary>
		public void UnderailTrains()
		{
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UnderailTrain(Trains[i]);
			});
		}

		
		/// <summary>Un-derails a train</summary>
		/// <param name="Train">The train</param>
		public static void UnderailTrain(TrainBase Train)
		{
			Train.Derailed = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.RollDueToTopplingAngle = 0.0;
				Train.Cars[i].Derailed = false;
			}
		}

		/// <summary>Updates the objects for all trains within the simulation world</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		public void UpdateTrainObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < Trains.Length; i++)
			{
				Trains[i].UpdateObjects(TimeElapsed, ForceUpdate);
			}

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (TrackFollowingObject Train in TFOs) //Must not use var, as otherwise the wrong inferred type
			{
				Train.UpdateObjects(TimeElapsed, ForceUpdate);
			}
		}

		/// <summary>Called after passengers have finished boarding, in order to update the train's mass from the new passenger ratio</summary>
		/// <param name="Train">The train</param>
		internal static void UpdateTrainMassFromPassengerRatio(TrainBase Train)
		{
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				double area = Train.Cars[i].Width * Train.Cars[i].Length;
				const double passengersPerArea = 1.0;
				double randomFactor = 0.9 + 0.2 * RandomNumberGenerator.NextDouble();
				double passengers = Math.Round(randomFactor * Train.Passengers.PassengerRatio * passengersPerArea * area);
				const double massPerPassenger = 70.0;
				double passengerMass = passengers * massPerPassenger;
				Train.Cars[i].CargoMass = passengerMass;
			}
		}

		/// <summary>Performs the jump for all TFOs</summary>
		public void JumpTFO()
		{
			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (TrackFollowingObject Train in TFOs) //Must not use var, as otherwise the wrong inferred type
			{
				Train.Jump(-1);
			}
		}
	}
}
