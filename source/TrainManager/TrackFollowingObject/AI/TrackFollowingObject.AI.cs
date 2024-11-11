using System;
using System.Linq;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using TrainManager.Handles;

namespace TrainManager.Trains
{
	/// <summary>The AI class used to operate a TFO</summary>
	public class TrackFollowingObjectAI : GeneralAI
	{
		private readonly ScriptedTrain Train;

		// Time to operate the door
		private readonly double OpeningDoorsTime;
		private readonly double ClosingDoorsTime;

		/// <summary>Travel plan of train</summary>
		private readonly TravelData[] Data;

		private double AppearanceTime;
		private double LeaveTime;

		private double TimeLastProcessed;
		private double CurrentPosition;

		public TrackFollowingObjectAI(ScriptedTrain train, TravelData[] data)
		{
			Train = train;
			Data = data;
			TimeLastProcessed = 0.0;

			if (Train.Cars[0].Specs.DoorOpenFrequency != 0.0)
			{
				OpeningDoorsTime = 1.0 / Train.Cars[0].Specs.DoorOpenFrequency;
			}

			if (Train.Cars[0].Specs.DoorCloseFrequency != 0.0)
			{
				ClosingDoorsTime = 1.0 / Train.Cars[0].Specs.DoorCloseFrequency;
			}

			SetupTravelData();
			CheckTravelData();
		}

		public override void Trigger(double timeElapsed)
		{
			if (TrainManagerBase.CurrentHost.InGameTime == TimeLastProcessed)
			{
				return;
			}

			// Initialize
			if (TimeLastProcessed == 0.0)
			{
				AppearanceTime = TrainManagerBase.CurrentHost.InGameTime;
				LeaveTime = AppearanceTime + Train.LeaveTime;
				CurrentPosition = Data[0].Position;
			}

			TimeLastProcessed = TrainManagerBase.CurrentHost.InGameTime;

			// Dispose the train if it is past the leave time of the train
			if (LeaveTime > AppearanceTime && TrainManagerBase.CurrentHost.InGameTime >= LeaveTime)
			{
				Train.Dispose();
				return;
			}

			// Calculate the position where the train is at the present time.
			GetNewState(TrainManagerBase.CurrentHost.InGameTime - AppearanceTime, out double newMileage, out double newPosition, out TravelDirection newDirection, out bool openLeftDoors, out bool openRightDoors);

			// Calculate the travel distance of the train.
			double deltaPosition = newPosition - CurrentPosition;

			// Set the state quantity of the train.
			if (deltaPosition < 0)
			{
				Train.Handles.Reverser.Driver = ReverserPosition.Reverse;
			}
			else if (deltaPosition > 0)
			{
				Train.Handles.Reverser.Driver = ReverserPosition.Forwards;
			}

			Train.Handles.Reverser.Actual = Train.Handles.Reverser.Driver;

			Train.OpenDoors(openLeftDoors, openRightDoors);
			Train.CloseDoors(!openLeftDoors, !openRightDoors);

			if (timeElapsed != 0.0)
			{
				double oldSpeed = Train.CurrentSpeed;
				Train.CurrentSpeed = deltaPosition / timeElapsed;
				Train.Specs.CurrentAverageAcceleration = (int)newDirection * (Train.CurrentSpeed - oldSpeed) / timeElapsed;
			}
			else
			{
				Train.CurrentSpeed = 0.0;
				Train.Specs.CurrentAverageAcceleration = 0.0;
			}

			foreach (var car in Train.Cars)
			{
				SetRailIndex(newMileage, newDirection, car.FrontAxle.Follower);
				SetRailIndex(newMileage, newDirection, car.RearAxle.Follower);
				SetRailIndex(newMileage, newDirection, car.FrontBogie.FrontAxle.Follower);
				SetRailIndex(newMileage, newDirection, car.FrontBogie.RearAxle.Follower);
				SetRailIndex(newMileage, newDirection, car.RearBogie.FrontAxle.Follower);
				SetRailIndex(newMileage, newDirection, car.RearBogie.RearAxle.Follower);

				car.Move(deltaPosition);

				car.CurrentSpeed = Train.CurrentSpeed;
				car.Specs.PerceivedSpeed = Train.CurrentSpeed;
				car.Specs.Acceleration = Train.Specs.CurrentAverageAcceleration;
				car.Specs.MotorAcceleration = Train.Specs.CurrentAverageAcceleration;
			}

			CurrentPosition = newPosition;
		}

		/// <summary>Create a train operation plan.</summary>
		internal void SetupTravelData()
		{
			// The first point must be TravelStopData.
			TravelStopData firstData = (TravelStopData)Data[0];

			// Calculate the end point of acceleration and the start point of deceleration.
			{
				TravelDirection lastDirection;
				double deltaPosition;

				// The start point does not slow down. Acceleration only.
				{
					firstData.Mileage = 0.0;
					lastDirection = firstData.Direction;
					deltaPosition = 0.0;
					if (firstData.Accelerate != 0.0)
					{
						deltaPosition = Math.Pow(firstData.TargetSpeed, 2.0) / (2.0 * firstData.Accelerate);
					}

					firstData.AccelerationEndPosition = firstData.Position + (int)lastDirection * deltaPosition;
				}

				for (int i = 1; i < Data.Length; i++)
				{
					deltaPosition = 0.0;
					if (Data[i].Decelerate != 0.0)
					{
						deltaPosition = (Math.Pow(Data[i].PassingSpeed, 2.0) - Math.Pow(Data[i - 1].TargetSpeed, 2.0)) / (2.0 * Data[i].Decelerate);
					}

					Data[i].DecelerationStartPosition = Data[i].Position - (int)lastDirection * deltaPosition;

					Data[i].Mileage = Data[i - 1].Mileage + Math.Abs(Data[i].Position - Data[i - 1].Position);
					lastDirection = (Data[i] as TravelStopData)?.Direction ?? lastDirection;
					deltaPosition = 0.0;
					if (Data[i].Accelerate != 0.0)
					{
						deltaPosition = (Math.Pow(Data[i].TargetSpeed, 2.0) - Math.Pow(Data[i].PassingSpeed, 2.0)) / (2.0 * Data[i].Accelerate);
					}

					Data[i].AccelerationEndPosition = Data[i].Position + (int)lastDirection * deltaPosition;
				}
			}

			// Reflect the delay until the TFO becomes effective at the first point.
			{
				if (firstData.OpenLeftDoors || firstData.OpenRightDoors)
				{
					firstData.OpeningDoorsEndTime += OpeningDoorsTime;
				}

				firstData.ClosingDoorsStartTime = firstData.OpeningDoorsEndTime + firstData.StopTime;
				firstData.DepartureTime = firstData.ClosingDoorsStartTime;
				if (firstData.OpenLeftDoors || firstData.OpenRightDoors)
				{
					firstData.DepartureTime += ClosingDoorsTime;
				}
			}

			// Calculate the time of each point.
			{
				double deltaT;

				// The start point does not slow down. Acceleration only.
				{
					deltaT = 0.0;
					if (firstData.Accelerate != 0.0)
					{
						deltaT = firstData.TargetSpeed / firstData.Accelerate;
					}

					firstData.AccelerationEndTime = firstData.DepartureTime + deltaT;
				}

				for (int i = 1; i < Data.Length; i++)
				{
					deltaT = 0.0;
					if (Data[i - 1].TargetSpeed != 0.0)
					{
						deltaT = Math.Abs(Data[i].DecelerationStartPosition - Data[i - 1].AccelerationEndPosition) / Data[i - 1].TargetSpeed;
					}

					Data[i].DecelerationStartTime = Data[i - 1].AccelerationEndTime + deltaT;

					deltaT = 0.0;
					if (Data[i].Decelerate != 0.0)
					{
						deltaT = (Data[i].PassingSpeed - Data[i - 1].TargetSpeed) / Data[i].Decelerate;
					}

					Data[i].ArrivalTime = Data[i].DecelerationStartTime + deltaT;

					if (Data[i] is TravelStopData stopData)
					{
						stopData.OpeningDoorsEndTime = stopData.ArrivalTime;
						if (stopData.OpenLeftDoors || stopData.OpenRightDoors)
						{
							stopData.OpeningDoorsEndTime += OpeningDoorsTime;
						}

						stopData.ClosingDoorsStartTime = stopData.OpeningDoorsEndTime + stopData.StopTime;
						stopData.DepartureTime = stopData.ClosingDoorsStartTime;
						if (stopData.OpenLeftDoors || stopData.OpenRightDoors)
						{
							stopData.DepartureTime += ClosingDoorsTime;
						}
					}

					deltaT = 0.0;
					if (Data[i].Accelerate != 0.0)
					{
						deltaT = (Data[i].TargetSpeed - Data[i].PassingSpeed) / Data[i].Accelerate;
					}

					Data[i].AccelerationEndTime = Data[i].DepartureTime + deltaT;
				}
			}
		}

		/// <summary>Check whether the travel plan of the train is valid.</summary>
		private void CheckTravelData()
		{
			bool recalculation = false;
			TravelDirection lastDirection = ((TravelStopData)Data[0]).Direction;

			for (int i = 1; i < Data.Length; i++)
			{
				// The deceleration start point must appear after the acceleration end point.
				if ((Data[i - 1].AccelerationEndPosition - Data[i].DecelerationStartPosition) * (int)lastDirection > 0)
				{
					// Reset acceleration and deceleration.
					double delta = Math.Abs(Data[i].Position - Data[i - 1].Position);
					if (delta != 0.0)
					{
						Data[i - 1].Accelerate = (Math.Pow(Data[i - 1].TargetSpeed, 2.0) - Math.Pow(Data[i - 1].PassingSpeed, 2.0)) / delta;
						Data[i].Decelerate = (Math.Pow(Data[i].PassingSpeed, 2.0) - Math.Pow(Data[i - 1].TargetSpeed, 2.0)) / delta;
						recalculation = true;
					}
				}

				lastDirection = (Data[i] as TravelStopData)?.Direction ?? lastDirection;
			}

			// Recreate the operation plan of the train.
			if (recalculation)
			{
				SetupTravelData();
			}
		}

		private void GetNewState(double time, out double newMileage, out double newPosition, out TravelDirection newDirection, out bool openLeftDoors, out bool openRightDoors)
		{
			double deltaT;
			double deltaPosition;
			openLeftDoors = false;
			openRightDoors = false;

			{
				// The first point must be TravelStopData.
				TravelStopData firstData = (TravelStopData)Data[0];

				newMileage = firstData.Mileage;
				newPosition = firstData.Position;
				newDirection = firstData.Direction;

				if (time <= firstData.ArrivalTime)
				{
					return;
				}

				if (time <= firstData.ClosingDoorsStartTime)
				{
					openLeftDoors = firstData.OpenLeftDoors;
					openRightDoors = firstData.OpenRightDoors;
					return;
				}

				if (time <= firstData.DepartureTime)
				{
					return;
				}

				// The start point does not slow down. Acceleration only.
				if (time <= firstData.AccelerationEndTime)
				{
					deltaT = time - firstData.DepartureTime;
					deltaPosition = 0.5 * firstData.Accelerate * Math.Pow(deltaT, 2.0);
					newMileage += deltaPosition;
					newPosition += (int)newDirection * deltaPosition;
					return;
				}

				newMileage += Math.Abs(firstData.AccelerationEndPosition - newPosition);
				newPosition = firstData.AccelerationEndPosition;
			}

			for (int i = 1; i < Data.Length; i++)
			{
				if (time <= Data[i].DecelerationStartTime)
				{
					deltaT = time - Data[i - 1].AccelerationEndTime;
					deltaPosition = Data[i - 1].TargetSpeed * deltaT;
					newMileage += deltaPosition;
					newPosition += (int)newDirection * deltaPosition;
					return;
				}

				newMileage += Math.Abs(Data[i].DecelerationStartPosition - newPosition);
				newPosition = Data[i].DecelerationStartPosition;

				if (time <= Data[i].ArrivalTime)
				{
					deltaT = time - Data[i].DecelerationStartTime;
					deltaPosition = Data[i - 1].TargetSpeed * deltaT + 0.5 * Data[i].Decelerate * Math.Pow(deltaT, 2.0);
					newMileage += deltaPosition;
					newPosition += (int)newDirection * deltaPosition;
					return;
				}

				TravelStopData stopData = Data[i] as TravelStopData;

				newMileage = Data[i].Mileage;
				newPosition = Data[i].Position;
				newDirection = stopData?.Direction ?? newDirection;

				if (time <= stopData?.ClosingDoorsStartTime)
				{
					openLeftDoors = stopData.OpenLeftDoors;
					openRightDoors = stopData.OpenRightDoors;
					return;
				}

				// The end point does not accelerate.
				if (time <= Data[i].DepartureTime || i == Data.Length - 1)
				{
					return;
				}

				if (time <= Data[i].AccelerationEndTime)
				{
					deltaT = time - Data[i].DepartureTime;
					deltaPosition = Data[i].PassingSpeed * deltaT + 0.5 * Data[i].Accelerate * Math.Pow(deltaT, 2.0);
					newMileage += deltaPosition;
					newPosition += (int)newDirection * deltaPosition;
					return;
				}

				newMileage += Math.Abs(Data[i].AccelerationEndPosition - newPosition);
				newPosition = Data[i].AccelerationEndPosition;
			}
		}

		private void SetRailIndex(double mileage, TravelDirection direction, TrackFollower trackFollower)
		{
			trackFollower.TrackIndex = Data.LastOrDefault(x => x.Mileage <= mileage + (int)direction * (trackFollower.TrackPosition - CurrentPosition))?.RailIndex ?? Data[0].RailIndex;
		}
	}
}
