using System;
using OpenBveApi.Units;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Performance section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Performance : BindableBase, ICloneable
	{
		private Quantity.Acceleration deceleration;
		private double coefficientOfStaticFriction;
		private double coefficientOfRollingResistance;
		private double aerodynamicDragCoefficient;

		internal Quantity.Acceleration Deceleration
		{
			get
			{
				return deceleration;
			}
			set
			{
				SetProperty(ref deceleration, value);
			}
		}

		internal double CoefficientOfStaticFriction
		{
			get
			{
				return coefficientOfStaticFriction;
			}
			set
			{
				SetProperty(ref coefficientOfStaticFriction, value);
			}
		}

		internal double CoefficientOfRollingResistance
		{
			get
			{
				return coefficientOfRollingResistance;
			}
			set
			{
				SetProperty(ref coefficientOfRollingResistance, value);
			}
		}

		internal double AerodynamicDragCoefficient
		{
			get
			{
				return aerodynamicDragCoefficient;
			}
			set
			{
				SetProperty(ref aerodynamicDragCoefficient, value);
			}
		}

		internal Performance()
		{
			Deceleration = new Quantity.Acceleration(1.0, Unit.Acceleration.KilometerPerHourPerSecond);
			CoefficientOfStaticFriction = 0.35;
			CoefficientOfRollingResistance = 0.0025;
			AerodynamicDragCoefficient = 1.2;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
