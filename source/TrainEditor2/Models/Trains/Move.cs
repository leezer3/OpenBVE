using System;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Move section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Move : BindableBase, ICloneable
	{
		private double jerkPowerUp;
		private double jerkPowerDown;
		private double jerkBrakeUp;
		private double jerkBrakeDown;
		private double brakeCylinderUp;
		private double brakeCylinderDown;

		internal double JerkPowerUp
		{
			get => jerkPowerUp;
			set => SetProperty(ref jerkPowerUp, value);
		}

		internal double JerkPowerDown
		{
			get => jerkPowerDown;
			set => SetProperty(ref jerkPowerDown, value);
		}

		internal double JerkBrakeUp
		{
			get => jerkBrakeUp;
			set => SetProperty(ref jerkBrakeUp, value);
		}

		internal double JerkBrakeDown
		{
			get => jerkBrakeDown;
			set => SetProperty(ref jerkBrakeDown, value);
		}

		internal double BrakeCylinderUp
		{
			get => brakeCylinderUp;
			set => SetProperty(ref brakeCylinderUp, value);
		}

		internal double BrakeCylinderDown
		{
			get => brakeCylinderDown;
			set => SetProperty(ref brakeCylinderDown, value);
		}

		internal Move()
		{
			JerkPowerUp = 1000.0;
			JerkPowerDown = 1000.0;
			JerkBrakeUp = 1000.0;
			JerkBrakeDown = 1000.0;
			BrakeCylinderUp = 300.0;
			BrakeCylinderDown = 200.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
