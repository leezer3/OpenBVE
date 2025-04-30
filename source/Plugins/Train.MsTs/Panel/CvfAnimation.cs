using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	internal class CvfAnimation : AnimationScript
	{
		internal readonly PanelSubject Subject;

		internal readonly FrameMapping[] FrameMapping;

		private int lastResult;

		internal CvfAnimation(PanelSubject subject, FrameMapping[] frameMapping)
		{
			this.Subject = subject;
			FrameMapping = frameMapping;
			Minimum = 0;
			Maximum = FrameMapping.Length;
		}
		
		public double ExecuteScript(AbstractTrain Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			dynamic dynamicTrain = Train;
			switch (Subject)
			{
				case PanelSubject.Throttle:
					for (int i = 0; i < FrameMapping.Length; i++)
					{
						if (FrameMapping[i].MappingValue >= (double)dynamicTrain.Handles.Power.Actual / dynamicTrain.Handles.Power.MaximumNotch)
						{
							lastResult = FrameMapping[i].FrameKey;
							break;
						}
					}
					break;
				case PanelSubject.Train_Brake:
					for (int i = 0; i < FrameMapping.Length; i++)
					{
						if (FrameMapping[i].MappingValue  >= (double)dynamicTrain.Handles.Brake.Actual / dynamicTrain.Handles.Brake.MaximumNotch)
						{
							lastResult = FrameMapping[i].FrameKey;
							break;
						}
					}
					break;
				case PanelSubject.Direction:
					lastResult = (int)dynamicTrain.Handles.Reverser.Actual + 1;
					break;
			}

			return lastResult;
		}

		public AnimationScript Clone()
		{
			return new CvfAnimation(Subject, FrameMapping);
		}

		public double LastResult
		{
			get => lastResult;
			set { }
		}

		public double Maximum
		{
			get;
			set;
		}

		public double Minimum
		{
			get;
			set;
		}
	}
}
