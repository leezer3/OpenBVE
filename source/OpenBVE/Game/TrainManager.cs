using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public class TrainManager : TrainManagerBase
	{
		/// <inheritdoc/>
		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}
		
		/// <summary>This method should be called once a frame to update the position, speed and state of all trains within the simulation</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal void UpdateTrains(double TimeElapsed)
		{
			if (Interface.CurrentOptions.GameMode == GameMode.Developer)
			{
				return;
			}
			for (int i = 0; i < Trains.Count; i++) {
				Trains[i].Update(TimeElapsed);
			}

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (ScriptedTrain Train in TFOs) //Must not use var, as otherwise the wrong inferred type
			{
				Train.Update(TimeElapsed);
			}

			// detect collision
			if (!Game.MinimalisticSimulation & Interface.CurrentOptions.Collisions)
			{
				
				for (int i = 0; i < Trains.Count; i++) {
					Trains[i].DetectTrainCollision(TimeElapsed, i, Trains);
					Trains[i].DetectBufferCollision(TimeElapsed, Program.CurrentRoute.BufferTrackPositions);
				}
			}

			for (int i = Trains.Count - 1; i > 0; i--)
			{
				switch (Trains[i].State)
				{
					case TrainState.DisposePending:
						Trains[i].State = TrainState.Disposed;
						break;
					case TrainState.Disposed:
						Trains.RemoveAt(i);
						break;
				}
			}

			// compute final angles and positions
			System.Threading.Tasks.Parallel.For(0, Trains.Count, i =>
			{
				if (Trains[i].State < TrainState.DisposePending) 
				{
					for (int j = 0; j < Trains[i].Cars.Length; j++)
					{
						Trains[i].Cars[j].FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Trains[i].Cars[j].UpdateTopplingCantAndSpring(TimeElapsed);
						Trains[i].Cars[j].FrontBogie.UpdateTopplingCantAndSpring();
						Trains[i].Cars[j].RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});

			System.Threading.Tasks.Parallel.For(0, TFOs.Count, i =>
			{
				if (TFOs[i].State < TrainState.DisposePending)
				{
					ScriptedTrain t = (ScriptedTrain) TFOs[i];
					foreach (var Car in t.Cars)
					{
						Car.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Car.UpdateTopplingCantAndSpring(TimeElapsed);
						Car.FrontBogie.UpdateTopplingCantAndSpring();
						Car.RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});
		}
	}
}
