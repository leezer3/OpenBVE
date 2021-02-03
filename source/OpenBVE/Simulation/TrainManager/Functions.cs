namespace OpenBve
{
	public partial class TrainManager
	{
		/// <summary>Un-derails all trains within the simulation</summary>
		internal static void UnderailTrains()
		{
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UnderailTrain(Trains[i]);
			});
		}
		/// <summary>Un-derails a train</summary>
		/// <param name="Train">The train</param>
		internal static void UnderailTrain(Train Train)
		{
			Train.Derailed = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.RollDueToTopplingAngle = 0.0;
				Train.Cars[i].Derailed = false;
			}
		}

		internal static void JumpTFO()
		{
			foreach (var Train in TFOs)
			{
				Train.Jump(-1);
			}
		}

		/// <summary>Updates the objects for all trains within the simulation world</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		internal static void UpdateTrainObjects(double TimeElapsed, bool ForceUpdate)
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
	}
}
