using System;
using System.Linq;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

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
		internal abstract class TravelData
		{
			// Parameters from XML
			internal double Decelerate;
			internal double Position;
			internal virtual double PassingSpeed
			{
				get;
				set;
			}
			internal double Accelerate;
			internal double TargetSpeed;
			internal int RailIndex;

			// Parameters calculated by the SetupTravelData function
			internal double DecelerationStartPosition;
			internal double DecelerationStartTime;
			internal virtual double ArrivalTime
			{
				get;
				set;
			}
			internal double Mileage;
			internal virtual double DepartureTime
			{
				get;
				set;
			}
			internal double AccelerationEndPosition;
			internal double AccelerationEndTime;
		}

		internal class TravelPointData : TravelData
		{
			// Parameters calculated by the SetupTravelData function
			private double PassingTime;
			internal override double ArrivalTime
			{
				get
				{
					return PassingTime;
				}
				set
				{
					PassingTime = value;
				}
			}
			internal override double DepartureTime
			{
				get
				{
					return PassingTime;
				}
				set
				{
					PassingTime = value;
				}
			}
		}

		internal class TravelStopData : TravelData
		{
			// Parameters from XML
			internal override double PassingSpeed => 0.0;
			internal double StopTime;
			internal bool OpenLeftDoors;
			internal bool OpenRightDoors;
			internal TravelDirection Direction;

			// Parameters calculated by the SetupTravelData function
			internal double OpeningDoorsEndTime;
			internal double ClosingDoorsStartTime;
		}

		internal class TrackFollowingObjectAI : GeneralAI
		{
			private readonly TrainManager.TrackFollowingObject Train;

			/// <summary>Travel plan of train</summary>
			private readonly TravelData[] Data;

			private double TimeLastProcessed;
			private double CurrentPosition;
			internal double AppearanceTime;
			internal double LeaveTime;

			internal TrackFollowingObjectAI(TrainManager.TrackFollowingObject train, TravelData[] data)
			{
				Train = train;
				Data = data;
				TimeLastProcessed = 0.0;
			}

			public override void Trigger(double TimeElapsed)
			{
				if (Program.CurrentRoute.SecondsSinceMidnight == TimeLastProcessed)
				{
					return;
				}

				// Initialize
				if (TimeLastProcessed == 0.0)
				{
					SetupTravelData(Program.CurrentRoute.SecondsSinceMidnight);
					CheckTravelData(Program.CurrentRoute.SecondsSinceMidnight);
					CurrentPosition = Data[0].Position;
				}

				TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;

				// Dispose the train if it is past the leave time of the train
				if (LeaveTime > AppearanceTime && Program.CurrentRoute.SecondsSinceMidnight >= LeaveTime)
				{
					Train.Dispose();
					return;
				}

				// Calculate the position where the train is at the present time.
				double NewMileage;
				double NewPosition;
				TravelDirection NewDirection;
				bool OpenLeftDoors;
				bool OpenRightDoors;
				GetNewState(Program.CurrentRoute.SecondsSinceMidnight, out NewMileage, out NewPosition, out NewDirection, out OpenLeftDoors, out OpenRightDoors);

				// Calculate the travel distance of the train.
				double DeltaPosition = NewPosition - CurrentPosition;

				// Set the state quantity of the train.
				if (DeltaPosition < 0)
				{
					Train.Handles.Reverser.Driver = TrainManager.ReverserPosition.Reverse;
				}
				else if (DeltaPosition > 0)
				{
					Train.Handles.Reverser.Driver = TrainManager.ReverserPosition.Forwards;
				}
				Train.Handles.Reverser.Actual = Train.Handles.Reverser.Driver;

				TrainManager.OpenTrainDoors(Train, OpenLeftDoors, OpenRightDoors);
				TrainManager.CloseTrainDoors(Train, !OpenLeftDoors, !OpenRightDoors);

				if (TimeElapsed != 0.0)
				{
					Train.CurrentSpeed = DeltaPosition / TimeElapsed;
					Train.Specs.CurrentAverageAcceleration = Math.Sign(Train.CurrentSpeed) * Train.CurrentSpeed / TimeElapsed;
				}
				else
				{
					Train.CurrentSpeed = 0.0;
					Train.Specs.CurrentAverageAcceleration = 0.0;
				}

				foreach (var Car in Train.Cars)
				{
					SetRailIndex(NewMileage, NewDirection, Car.FrontAxle.Follower);
					SetRailIndex(NewMileage, NewDirection, Car.RearAxle.Follower);
					SetRailIndex(NewMileage, NewDirection, Car.FrontBogie.FrontAxle.Follower);
					SetRailIndex(NewMileage, NewDirection, Car.FrontBogie.RearAxle.Follower);
					SetRailIndex(NewMileage, NewDirection, Car.RearBogie.FrontAxle.Follower);
					SetRailIndex(NewMileage, NewDirection, Car.RearBogie.RearAxle.Follower);

					Car.Move(DeltaPosition);

					Car.CurrentSpeed = Train.CurrentSpeed;
					Car.Specs.CurrentPerceivedSpeed = Train.CurrentSpeed;
					Car.Specs.CurrentAcceleration = Train.Specs.CurrentAverageAcceleration;
					Car.Specs.CurrentAccelerationOutput = Train.Specs.CurrentAverageAcceleration;
				}

				CurrentPosition = NewPosition;
			}

			/// <summary>Create a train operation plan.</summary>
			internal void SetupTravelData(double InitializeTime)
			{
				// The first point must be TravelStopData.
				TravelStopData FirstData = (TravelStopData)Data[0];

				// Calculate the end point of acceleration and the start point of deceleration.
				{
					TravelDirection LastDirection;
					double DeltaPosition;

					// The start point does not slow down. Acceleration only.
					{
						FirstData.Mileage = 0.0;
						LastDirection = FirstData.Direction;
						DeltaPosition = 0.0;
						if (FirstData.Accelerate != 0.0)
						{
							DeltaPosition = Math.Pow(FirstData.TargetSpeed, 2.0) / (2.0 * FirstData.Accelerate);
						}
						FirstData.AccelerationEndPosition = FirstData.Position + (int)LastDirection * DeltaPosition;
					}

					for (int i = 1; i < Data.Length; i++)
					{
						DeltaPosition = 0.0;
						if (Data[i].Decelerate != 0.0)
						{
							DeltaPosition = (Math.Pow(Data[i].PassingSpeed, 2.0) - Math.Pow(Data[i - 1].TargetSpeed, 2.0)) / (2.0 * Data[i].Decelerate);
						}
						Data[i].DecelerationStartPosition = Data[i].Position - (int)LastDirection * DeltaPosition;

						Data[i].Mileage = Data[i - 1].Mileage + Math.Abs(Data[i].Position - Data[i - 1].Position);
						LastDirection = (Data[i] as TravelStopData)?.Direction ?? LastDirection;
						DeltaPosition = 0.0;
						if (Data[i].Accelerate != 0.0)
						{
							DeltaPosition = (Math.Pow(Data[i].TargetSpeed, 2.0) - Math.Pow(Data[i].PassingSpeed, 2.0)) / (2.0 * Data[i].Accelerate);
						}
						Data[i].AccelerationEndPosition = Data[i].Position + (int)LastDirection * DeltaPosition;
					}
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

				// Reflect the delay until the TFO becomes effective at the first point.
				{
					FirstData.ArrivalTime = InitializeTime;
					AppearanceTime = InitializeTime;
					LeaveTime = Train.LeaveTime + InitializeTime;

					FirstData.OpeningDoorsEndTime = FirstData.ArrivalTime;
					if (FirstData.OpenLeftDoors || FirstData.OpenRightDoors)
					{
						FirstData.OpeningDoorsEndTime += OpeningDoorsTime;
					}
					FirstData.ClosingDoorsStartTime = FirstData.OpeningDoorsEndTime + FirstData.StopTime;
					FirstData.DepartureTime = FirstData.ClosingDoorsStartTime;
					if (FirstData.OpenLeftDoors || FirstData.OpenRightDoors)
					{
						FirstData.DepartureTime += ClosingDoorsTime;
					}
				}

				// Calculate the time of each point.
				{
					double DeltaT;

					// The start point does not slow down. Acceleration only.
					{
						DeltaT = 0.0;
						if (FirstData.Accelerate != 0.0)
						{
							DeltaT = FirstData.TargetSpeed / FirstData.Accelerate;
						}
						FirstData.AccelerationEndTime = FirstData.DepartureTime + DeltaT;
					}

					for (int i = 1; i < Data.Length; i++)
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
							DeltaT = (Data[i].PassingSpeed - Data[i - 1].TargetSpeed) / Data[i].Decelerate;
						}
						Data[i].ArrivalTime = Data[i].DecelerationStartTime + DeltaT;

						TravelStopData StopData = Data[i] as TravelStopData;

						if (StopData != null)
						{
							StopData.OpeningDoorsEndTime = StopData.ArrivalTime;
							if (StopData.OpenLeftDoors || StopData.OpenRightDoors)
							{
								StopData.OpeningDoorsEndTime += OpeningDoorsTime;
							}
							StopData.ClosingDoorsStartTime = StopData.OpeningDoorsEndTime + StopData.StopTime;
							StopData.DepartureTime = StopData.ClosingDoorsStartTime;
							if (StopData.OpenLeftDoors || StopData.OpenRightDoors)
							{
								StopData.DepartureTime += ClosingDoorsTime;
							}
						}

						DeltaT = 0.0;
						if (Data[i].Accelerate != 0.0)
						{
							DeltaT = (Data[i].TargetSpeed - Data[i].PassingSpeed) / Data[i].Accelerate;
						}
						Data[i].AccelerationEndTime = Data[i].DepartureTime + DeltaT;
					}
				}
			}

			/// <summary>Check whether the travel plan of the train is valid.</summary>
			private void CheckTravelData(double InitializeTime)
			{
				bool Recalculation = false;
				TravelDirection LastDirection = ((TravelStopData)Data[0]).Direction;

				for (int i = 1; i < Data.Length; i++)
				{
					// The deceleration start point must appear after the acceleration end point.
					if ((Data[i - 1].AccelerationEndPosition - Data[i].DecelerationStartPosition) * (int)LastDirection > 0)
					{
						// Reset acceleration and deceleration.
						double Delta = Math.Abs(Data[i].Position - Data[i - 1].Position);
						if (Delta != 0.0)
						{
							Data[i - 1].Accelerate = (Math.Pow(Data[i - 1].TargetSpeed, 2.0) - Math.Pow(Data[i - 1].PassingSpeed, 2.0)) / Delta;
							Data[i].Decelerate = (Math.Pow(Data[i].PassingSpeed, 2.0) - Math.Pow(Data[i - 1].TargetSpeed, 2.0)) / Delta;
							Recalculation = true;
						}
					}

					LastDirection = (Data[i] as TravelStopData)?.Direction ?? LastDirection;
				}

				// Recreate the operation plan of the train.
				if (Recalculation)
				{
					SetupTravelData(InitializeTime);
				}
			}

			private void GetNewState(double Time, out double NewMileage, out double NewPosition, out TravelDirection NewDirection, out bool OpenLeftDoors, out bool OpenRightDoors)
			{
				double DeltaT;
				double DeltaPosition;
				OpenLeftDoors = false;
				OpenRightDoors = false;

				{
					// The first point must be TravelStopData.
					TravelStopData FirstData = (TravelStopData)Data[0];

					NewMileage = FirstData.Mileage;
					NewPosition = FirstData.Position;
					NewDirection = FirstData.Direction;

					if (Time <= FirstData.ArrivalTime)
					{
						return;
					}

					if (Time <= FirstData.ClosingDoorsStartTime)
					{
						OpenLeftDoors = FirstData.OpenLeftDoors;
						OpenRightDoors = FirstData.OpenRightDoors;
						return;
					}

					if (Time <= FirstData.DepartureTime)
					{
						return;
					}

					// The start point does not slow down. Acceleration only.
					if (Time <= FirstData.AccelerationEndTime)
					{
						DeltaT = Time - FirstData.DepartureTime;
						DeltaPosition = 0.5 * FirstData.Accelerate * Math.Pow(DeltaT, 2.0);
						NewMileage += DeltaPosition;
						NewPosition += (int)NewDirection * DeltaPosition;
						return;
					}

					NewMileage += Math.Abs(FirstData.AccelerationEndPosition - NewPosition);
					NewPosition = FirstData.AccelerationEndPosition;
				}

				for (int i = 1; i < Data.Length; i++)
				{
					if (Time <= Data[i].DecelerationStartTime)
					{
						DeltaT = Time - Data[i - 1].AccelerationEndTime;
						DeltaPosition = Data[i - 1].TargetSpeed * DeltaT;
						NewMileage += DeltaPosition;
						NewPosition += (int)NewDirection * DeltaPosition;
						return;
					}

					NewMileage += Math.Abs(Data[i].DecelerationStartPosition - NewPosition);
					NewPosition = Data[i].DecelerationStartPosition;

					if (Time <= Data[i].ArrivalTime)
					{
						DeltaT = Time - Data[i].DecelerationStartTime;
						DeltaPosition = Data[i - 1].TargetSpeed * DeltaT + 0.5 * Data[i].Decelerate * Math.Pow(DeltaT, 2.0);
						NewMileage += DeltaPosition;
						NewPosition += (int)NewDirection * DeltaPosition;
						return;
					}

					TravelStopData StopData = Data[i] as TravelStopData;

					NewMileage = Data[i].Mileage;
					NewPosition = Data[i].Position;
					NewDirection = StopData?.Direction ?? NewDirection;

					if (Time <= StopData?.ClosingDoorsStartTime)
					{
						OpenLeftDoors = StopData.OpenLeftDoors;
						OpenRightDoors = StopData.OpenRightDoors;
						return;
					}

					// The end point does not accelerate.
					if (Time <= Data[i].DepartureTime || i == Data.Length - 1)
					{
						return;
					}

					if (Time <= Data[i].AccelerationEndTime)
					{
						DeltaT = Time - Data[i].DepartureTime;
						DeltaPosition = Data[i].PassingSpeed * DeltaT + 0.5 * Data[i].Accelerate * Math.Pow(DeltaT, 2.0);
						NewMileage += DeltaPosition;
						NewPosition += (int)NewDirection * DeltaPosition;
						return;
					}

					NewMileage += Math.Abs(Data[i].AccelerationEndPosition - NewPosition);
					NewPosition = Data[i].AccelerationEndPosition;
				}
			}

			private void SetRailIndex(double Mileage, TravelDirection Direction, TrackFollower TrackFollower)
			{
				TrackFollower.TrackIndex = Data.LastOrDefault(x => x.Mileage <= Mileage + (int)Direction * (TrackFollower.TrackPosition - CurrentPosition))?.RailIndex ?? Data[0].RailIndex;
			}
		}
	}
}
