using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Pressure section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Pressure : BindableBase, ICloneable
	{
		private double brakeCylinderServiceMaximumPressure;
		private double brakeCylinderEmergencyMaximumPressure;
		private double mainReservoirMinimumPressure;
		private double mainReservoirMaximumPressure;
		private double brakePipeNormalPressure;

		internal double BrakeCylinderServiceMaximumPressure
		{
			get
			{
				return brakeCylinderServiceMaximumPressure;
			}
			set
			{
				SetProperty(ref brakeCylinderServiceMaximumPressure, value);
			}
		}

		internal double BrakeCylinderEmergencyMaximumPressure
		{
			get
			{
				return brakeCylinderEmergencyMaximumPressure;
			}
			set
			{
				SetProperty(ref brakeCylinderEmergencyMaximumPressure, value);
			}
		}

		internal double MainReservoirMinimumPressure
		{
			get
			{
				return mainReservoirMinimumPressure;
			}
			set
			{
				SetProperty(ref mainReservoirMinimumPressure, value);
			}
		}

		internal double MainReservoirMaximumPressure
		{
			get
			{
				return mainReservoirMaximumPressure;
			}
			set
			{
				SetProperty(ref mainReservoirMaximumPressure, value);
			}
		}

		internal double BrakePipeNormalPressure
		{
			get
			{
				return brakePipeNormalPressure;
			}
			set
			{
				SetProperty(ref brakePipeNormalPressure, value);
			}
		}

		internal Pressure()
		{
			BrakeCylinderServiceMaximumPressure = 480.0;
			BrakeCylinderEmergencyMaximumPressure = 480.0;
			MainReservoirMinimumPressure = 690.0;
			MainReservoirMaximumPressure = 780.0;
			BrakePipeNormalPressure = 490.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
