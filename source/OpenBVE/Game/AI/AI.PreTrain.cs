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
				if (Program.CurrentRoute.SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;
					CurrentInterval = 5.0;
					double ap = double.MaxValue, at = double.MaxValue;
					double bp = double.MinValue, bt = double.MinValue;
					for (int i = 0; i < Program.CurrentRoute.BogusPreTrainInstructions.Length; i++)
					{
						if (Program.CurrentRoute.BogusPreTrainInstructions[i].Time < Program.CurrentRoute.SecondsSinceMidnight | at == double.MaxValue)
						{
							at = Program.CurrentRoute.BogusPreTrainInstructions[i].Time;
							ap = Program.CurrentRoute.BogusPreTrainInstructions[i].TrackPosition;
						}
					}
					for (int i = Program.CurrentRoute.BogusPreTrainInstructions.Length - 1; i >= 0; i--)
					{
						if (Program.CurrentRoute.BogusPreTrainInstructions[i].Time > at | bt == double.MinValue)
						{
							bt = Program.CurrentRoute.BogusPreTrainInstructions[i].Time;
							bp = Program.CurrentRoute.BogusPreTrainInstructions[i].TrackPosition;
						}
					}
					if (at != double.MaxValue & bt != double.MinValue & Program.CurrentRoute.SecondsSinceMidnight <= Program.CurrentRoute.BogusPreTrainInstructions[Program.CurrentRoute.BogusPreTrainInstructions.Length - 1].Time)
					{
						double r = bt - at;
						if (r > 0.0)
						{
							r = (Program.CurrentRoute.SecondsSinceMidnight - at) / r;
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
