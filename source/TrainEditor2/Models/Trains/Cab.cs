using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Cab : BindableBase, ICloneable
	{
		private double positionX;
		private double positionY;
		private double positionZ;
		private int driverCar;

		internal double PositionX
		{
			get
			{
				return positionX;
			}
			set
			{
				SetProperty(ref positionX, value);
			}
		}

		internal double PositionY
		{
			get
			{
				return positionY;
			}
			set
			{
				SetProperty(ref positionY, value);
			}
		}

		internal double PositionZ
		{
			get
			{
				return positionZ;
			}
			set
			{
				SetProperty(ref positionZ, value);
			}
		}

		internal int DriverCar
		{
			get
			{
				return driverCar;
			}
			set
			{
				SetProperty(ref driverCar, value);
			}
		}

		internal Cab()
		{
			PositionX = PositionY = PositionZ = 0.0;
			DriverCar = 0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
