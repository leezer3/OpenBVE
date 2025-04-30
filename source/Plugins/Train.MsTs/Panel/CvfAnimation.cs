using System;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	/// <summary>Animation class handling CVF elements with keyframe mappings</summary>
	internal class CvfAnimation : AnimationScript
	{
		internal readonly PanelSubject Subject;

		internal readonly FrameMapping[] FrameMapping;

		internal readonly int Digit;

		internal readonly double UnitConversionFactor;

		private int lastResult;

		internal CvfAnimation(PanelSubject subject, FrameMapping[] frameMapping)
		{
			Subject = subject;
			FrameMapping = frameMapping;
			Minimum = 0;
			Maximum = FrameMapping.Length;
			Digit = -1;
		}

		internal CvfAnimation(PanelSubject subject, Units unit, int digit)
		{
			Subject= subject;
			switch (unit)
			{
				case Units.Miles_Per_Hour:
					UnitConversionFactor = 2.2369362920544;
					break;
				case Units.Kilometers_Per_Hour:
					UnitConversionFactor = 3.6;
					break;
			}
			Digit = digit;
		}

		internal CvfAnimation(PanelSubject subject, Units unit, FrameMapping[] frameMapping)
		{
			Subject = subject;
			switch (unit)
			{
				case Units.Miles_Per_Hour:
					UnitConversionFactor = 2.2369362920544;
					break;
				case Units.Kilometers_Per_Hour:
					UnitConversionFactor = 3.6;
					break;
			}

			Digit = -1;
			FrameMapping = frameMapping;
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
				case PanelSubject.Speedlim_Display:
					double speedLim = Math.Min(Train.CurrentRouteLimit, Train.CurrentSectionLimit) * UnitConversionFactor;
					if (Digit == -1)
					{
						// color
						for (int i = 0; i < FrameMapping.Length; i++)
						{
							if (FrameMapping[i].MappingValue <= speedLim)
							{
								lastResult = FrameMapping[i].FrameKey;
								break;
							}
						}
					}
					else
					{
						// digit
						if (speedLim == double.PositiveInfinity)
						{
							lastResult = -1; // cheat to hide
						}
						else
						{
							lastResult = (int)(speedLim / (int)Math.Pow(10, Digit) % 10);
						}
					}
					break;
				case PanelSubject.Speedometer:
					double currentSpeed = Math.Abs(Train.CurrentSpeed) * UnitConversionFactor;
					if (Digit == -1)
					{
						// color
						for (int i = FrameMapping.Length - 1; i > 0; i--)
						{
							if (FrameMapping[i].MappingValue <= currentSpeed)
							{
								lastResult = FrameMapping[i].FrameKey;
								break;
							}
						}
					}
					else
					{
						// digit
						lastResult = (int)(currentSpeed / (int)Math.Pow(10, Digit) % 10);
					}
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
