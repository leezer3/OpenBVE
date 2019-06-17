using OpenBve.RouteManager;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Represents the bogus (non-visible) AI train, placed via .PreTrain commands</summary>
		internal class BogusPretrainAI : GeneralAI
		{
			private double TimeLastProcessed;
			private double CurrentInterval;
			private readonly TrainManager.Train Train;

			internal BogusPretrainAI(TrainManager.Train train)
			{
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
				this.Train = train;
			}

			internal override void Trigger(double TimeElapsed)
			{
				if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					TimeLastProcessed = SecondsSinceMidnight;
					CurrentInterval = 5.0;
					double ap = double.MaxValue, at = double.MaxValue;
					double bp = double.MinValue, bt = double.MinValue;
					for (int i = 0; i < CurrentRoute.BogusPretrainInstructions.Length; i++)
					{
						if (CurrentRoute.BogusPretrainInstructions[i].Time < SecondsSinceMidnight | at == double.MaxValue)
						{
							at = CurrentRoute.BogusPretrainInstructions[i].Time;
							ap = CurrentRoute.BogusPretrainInstructions[i].TrackPosition;
						}
					}
					for (int i = CurrentRoute.BogusPretrainInstructions.Length - 1; i >= 0; i--)
					{
						if (CurrentRoute.BogusPretrainInstructions[i].Time > at | bt == double.MinValue)
						{
							bt = CurrentRoute.BogusPretrainInstructions[i].Time;
							bp = CurrentRoute.BogusPretrainInstructions[i].TrackPosition;
						}
					}
					if (at != double.MaxValue & bt != double.MinValue & SecondsSinceMidnight <= CurrentRoute.BogusPretrainInstructions[CurrentRoute.BogusPretrainInstructions.Length - 1].Time)
					{
						double r = bt - at;
						if (r > 0.0)
						{
							r = (SecondsSinceMidnight - at) / r;
							if (r < 0.0) r = 0.0;
							if (r > 1.0) r = 1.0;
						}
						else
						{
							r = 0.0;
						}
						double p = ap + r * (bp - ap);
						double d = p - Train.Cars[0].FrontAxle.Follower.TrackPosition;
						for (int j = 0; j < Train.Cars.Length; j++)
						{
							Train.Cars[j].Move(d);
						}
					}
					else
					{
						Train.Dispose();
					}
				}
			}
		}
	}
}
