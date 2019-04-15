using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBve
{
	internal static partial class Game
	{
		internal enum TravelDirection
		{
			Forward = 1,
			Backward = -1
		}

		/// <summary>Travel plan of the train moving to a certain point</summary>
		internal class TravelData
		{
			// Parameters from XML
			internal double Decelerate;
			internal double StopPosition;
			internal double StopTime;
			internal bool OpenLeftDoors;
			internal bool OpenRightDoors;
			internal double Accelerate;
			internal double TargetSpeed;
			internal TravelDirection Direction;
			internal int RailIndex;

			// Parameters calculated by the SetupTravelData function
			internal double DecelerationStartPosition;
			internal double DecelerationStartTime;
			internal double ArrivalTime;
			internal double OpeningDoorsEndTime;
			internal double ClosingDoorsStartTime;
			internal double DepartureTime;
			internal double AccelerationEndPosition;
			internal double AccelerationEndTime;
		}

		internal class TrackFollowingObjectAI : GeneralAI
		{
			private readonly TrainManager.TrackFollowingObject Train;

			/// <summary>Travel plan of train</summary>
			private readonly List<TravelData> Data;

			private double TimeLastProcessed;
			private double CurrentPosition;
			internal double AppearanceTime;
			internal double LeaveTime;

			internal TrackFollowingObjectAI(TrainManager.TrackFollowingObject train, List<TravelData> data)
			{
				Train = train;
				Data = data;
				TimeLastProcessed = 0.0;
			}

			internal override void Trigger(double TimeElapsed)
			{
				// Trains need to stop more than 2 points.
				if (Data == null || Data.Count < 2 || SecondsSinceMidnight == TimeLastProcessed)
				{
					return;
				}

				// Initialize
				if (TimeLastProcessed == 0.0)
				{
					SetupTravelData(SecondsSinceMidnight);
					CheckTravelData(SecondsSinceMidnight);
					CurrentPosition = Data[0].StopPosition;
				}

				TimeLastProcessed = SecondsSinceMidnight;

				// Calculate the position where the train is at the present time.
				double DeltaT;
				double Position = Data[0].StopPosition;
				int RailIndex = Data[0].RailIndex;
				bool OpenLeftDoors = false;
				bool OpenRightDoors = false;

				if (SecondsSinceMidnight >= Data[0].ArrivalTime)
				{
					OpenLeftDoors = Data[0].OpenLeftDoors;
					OpenRightDoors = Data[0].OpenRightDoors;
				}

				if (SecondsSinceMidnight >= Data[0].ClosingDoorsStartTime)
				{
					OpenLeftDoors = false;
					OpenRightDoors = false;
				}

				// The start point does not slow down. Acceleration only.
				if (SecondsSinceMidnight >= Data[0].DepartureTime)
				{
					if (SecondsSinceMidnight < Data[0].AccelerationEndTime)
					{
						DeltaT = SecondsSinceMidnight - Data[0].DepartureTime;
					}
					else
					{
						DeltaT = Data[0].AccelerationEndTime - Data[0].DepartureTime;
					}
					Position += (int)Data[0].Direction * (0.5 * Data[0].Accelerate * Math.Pow(DeltaT, 2.0));
				}

				for (int i = 1; i < Data.Count; i++)
				{
					if (SecondsSinceMidnight >= Data[i - 1].AccelerationEndTime)
					{
						if (SecondsSinceMidnight < Data[i].DecelerationStartTime)
						{
							DeltaT = SecondsSinceMidnight - Data[i - 1].AccelerationEndTime;
						}
						else
						{
							DeltaT = Data[i].DecelerationStartTime - Data[i - 1].AccelerationEndTime;
						}
						Position += (int)Data[i - 1].Direction * (Data[i - 1].TargetSpeed * DeltaT);
					}

					if (SecondsSinceMidnight >= Data[i].DecelerationStartTime)
					{
						if (SecondsSinceMidnight < Data[i].ArrivalTime)
						{
							DeltaT = SecondsSinceMidnight - Data[i].DecelerationStartTime;
						}
						else
						{
							DeltaT = Data[i].ArrivalTime - Data[i].DecelerationStartTime;
						}
						Position += (int)Data[i - 1].Direction * (Data[i - 1].TargetSpeed * DeltaT - 0.5 * Data[i].Decelerate * Math.Pow(DeltaT, 2.0));
					}

					if (SecondsSinceMidnight >= Data[i].ArrivalTime)
					{
						OpenLeftDoors = Data[i].OpenLeftDoors;
						OpenRightDoors = Data[i].OpenRightDoors;
					}

					if (SecondsSinceMidnight >= Data[i].ClosingDoorsStartTime)
					{
						OpenLeftDoors = false;
						OpenRightDoors = false;
					}

					if (SecondsSinceMidnight >= Data[i].DepartureTime)
					{
						if (SecondsSinceMidnight < Data[i].AccelerationEndTime)
						{
							DeltaT = SecondsSinceMidnight - Data[i].DepartureTime;
						}
						else
						{
							DeltaT = Data[i].AccelerationEndTime - Data[i].DepartureTime;
						}
						Position += (int)Data[i].Direction * (0.5 * Data[i].Accelerate * Math.Pow(DeltaT, 2.0));
						RailIndex = Data[i].RailIndex;
					}
				}

				// Calculate the travel distance of the train.
				double Delta = Position - CurrentPosition;
				CurrentPosition = Position;

				// Set the state quantity of the train.
				if (Delta < 0)
				{
					Train.Handles.Reverser.Driver = TrainManager.ReverserPosition.Reverse;
				}
				else if (Delta > 0)
				{
					Train.Handles.Reverser.Driver = TrainManager.ReverserPosition.Forwards;
				}
				Train.Handles.Reverser.Actual = Train.Handles.Reverser.Driver;

				TrainManager.OpenTrainDoors(Train, OpenLeftDoors, OpenRightDoors);
				TrainManager.CloseTrainDoors(Train, !OpenLeftDoors, !OpenRightDoors);

				foreach (var Car in Train.Cars)
				{
					Car.FrontAxle.Follower.TrackIndex = RailIndex;
					Car.RearAxle.Follower.TrackIndex = RailIndex;
					Car.FrontBogie.FrontAxle.Follower.TrackIndex = RailIndex;
					Car.FrontBogie.RearAxle.Follower.TrackIndex = RailIndex;
					Car.RearBogie.FrontAxle.Follower.TrackIndex = RailIndex;
					Car.RearBogie.RearAxle.Follower.TrackIndex = RailIndex;
					Car.Move(Delta);
					if (TimeElapsed != 0.0)
					{
						Car.Specs.CurrentSpeed = Delta / TimeElapsed;
						Car.Specs.CurrentPerceivedSpeed = Car.Specs.CurrentSpeed;
						if (Car.Specs.CurrentPerceivedSpeed < 0)
						{
							Car.Specs.CurrentAcceleration = -(Car.Specs.CurrentSpeed / TimeElapsed);
						}
						else
						{
							Car.Specs.CurrentAcceleration = Car.Specs.CurrentSpeed / TimeElapsed;
						}
						Car.Specs.CurrentAccelerationOutput = Car.Specs.CurrentAcceleration;
					}
					else
					{
						Car.Specs.CurrentSpeed = 0.0;
						Car.Specs.CurrentPerceivedSpeed = 0.0;
						Car.Specs.CurrentAcceleration = 0.0;
						Car.Specs.CurrentAccelerationOutput = 0.0;
					}
				}

				Train.Specs.CurrentAverageSpeed = Train.Cars[0].Specs.CurrentSpeed;
				Train.Specs.CurrentAverageAcceleration = Train.Cars[0].Specs.CurrentAcceleration;

				// Dispose the train if it is past the leave time of the train
				if (LeaveTime > AppearanceTime && SecondsSinceMidnight >= LeaveTime)
				{
					Train.Dispose();
				}
			}

			/// <summary>Create a train operation plan.</summary>
			internal void SetupTravelData(double InitializeTime)
			{
				// The start point does not slow down. Acceleration only.
				double DeltaPosition = 0.0;
				if (Data[0].Accelerate != 0.0)
				{
					DeltaPosition = Math.Pow(Data[0].TargetSpeed, 2.0) / (2.0 * Data[0].Accelerate);
				}
				Data[0].AccelerationEndPosition = Data[0].StopPosition + (int)Data[0].Direction * DeltaPosition;

				for (int i = 1; i < Data.Count; i++)
				{
					DeltaPosition = 0.0;
					if (Data[i].Decelerate != 0.0)
					{
						DeltaPosition = Math.Pow(Data[i - 1].TargetSpeed, 2.0) / (2.0 * Data[i].Decelerate);
					}
					Data[i].DecelerationStartPosition = Data[i].StopPosition - (int)Data[i - 1].Direction * DeltaPosition;

					DeltaPosition = 0.0;
					if (Data[i].Accelerate != 0.0)
					{
						DeltaPosition = Math.Pow(Data[i].TargetSpeed, 2.0) / (2.0 * Data[i].Accelerate);
					}
					Data[i].AccelerationEndPosition = Data[i].StopPosition + (int)Data[i].Direction * DeltaPosition;
				}

				// Time to operate the door
				double OpeningDoorsTime = 0.0;
				double ClosingDoorsTime = 0.0;

				if (Train.Cars[0].Specs.DoorOpenFrequency != 0.0)
				{
					OpeningDoorsTime = 1.0 / Train.Cars[0].Specs.DoorOpenFrequency;
				}
				if (Train.Cars[0].Specs.DoorCloseFrequency != 0.0)
				{
					ClosingDoorsTime = 1.0 / Train.Cars[0].Specs.DoorCloseFrequency;
				}

				// Delay
				Data[0].ArrivalTime = InitializeTime;
				AppearanceTime = InitializeTime;
				LeaveTime = Train.LeaveTime + InitializeTime;

				Data[0].OpeningDoorsEndTime = Data[0].ArrivalTime;
				if (Data[0].OpenLeftDoors || Data[0].OpenRightDoors)
				{
					Data[0].OpeningDoorsEndTime += OpeningDoorsTime;
				}
				Data[0].ClosingDoorsStartTime = Data[0].OpeningDoorsEndTime + Data[0].StopTime;
				Data[0].DepartureTime = Data[0].ClosingDoorsStartTime;
				if (Data[0].OpenLeftDoors || Data[0].OpenRightDoors)
				{
					Data[0].DepartureTime += ClosingDoorsTime;
				}

				// The start point does not slow down. Acceleration only.
				double DeltaT = 0.0;
				if (Data[0].Accelerate != 0.0)
				{
					DeltaT = Data[0].TargetSpeed / Data[0].Accelerate;
				}
				Data[0].AccelerationEndTime = Data[0].DepartureTime + DeltaT;

				for (int i = 1; i < Data.Count; i++)
				{
					DeltaT = 0.0;
					if (Data[i - 1].TargetSpeed != 0.0)
					{
						DeltaT = Math.Abs(Data[i].DecelerationStartPosition - Data[i - 1].AccelerationEndPosition) / Data[i - 1].TargetSpeed;
					}
					Data[i].DecelerationStartTime = Data[i - 1].AccelerationEndTime + DeltaT;

					DeltaT = 0.0;
					if (Data[i].Decelerate != 0.0)
					{
						DeltaT = Data[i - 1].TargetSpeed / Data[i].Decelerate;
					}
					Data[i].ArrivalTime = Data[i].DecelerationStartTime + DeltaT;

					Data[i].OpeningDoorsEndTime = Data[i].ArrivalTime;
					if (Data[i].OpenLeftDoors || Data[i].OpenRightDoors)
					{
						Data[i].OpeningDoorsEndTime += OpeningDoorsTime;
					}
					Data[i].ClosingDoorsStartTime = Data[i].OpeningDoorsEndTime + Data[i].StopTime;
					Data[i].DepartureTime = Data[i].ClosingDoorsStartTime;
					if (Data[i].OpenLeftDoors || Data[i].OpenRightDoors)
					{
						Data[i].DepartureTime += ClosingDoorsTime;
					}

					DeltaT = 0.0;
					if (Data[i].Accelerate != 0.0)
					{
						DeltaT = Data[i].TargetSpeed / Data[i].Accelerate;
					}
					Data[i].AccelerationEndTime = Data[i].DepartureTime + DeltaT;
				}
			}

			/// <summary>Check whether the travel plan of the train is valid.</summary>
			private void CheckTravelData(double InitializeTime)
			{
				bool Recalculation = false;

				for (int i = 1; i < Data.Count; i++)
				{
					// The deceleration start point must appear after the acceleration end point.
					if ((Data[i - 1].AccelerationEndPosition - Data[i].DecelerationStartPosition) * (int)Data[i - 1].Direction > 0)
					{
						// Reset acceleration and deceleration.
						double Delta = Math.Abs(Data[i].StopPosition - Data[i - 1].StopPosition);
						if (Delta != 0.0)
						{
							Data[i - 1].Accelerate = Math.Pow(Data[i - 1].TargetSpeed, 2.0) / Delta;
							Data[i].Decelerate = Data[i - 1].Accelerate;
							Recalculation = true;
						}
					}
				}

				// Recreate the operation plan of the train.
				if (Recalculation)
				{
					SetupTravelData(InitializeTime);
				}
			}
		}
	}
}
