using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Performance section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Performance : BindableBase, ICloneable
	{
		private double deceleration;
		private double coefficientOfStaticFriction;
		private double coefficientOfRollingResistance;
		private double aerodynamicDragCoefficient;

		internal double Deceleration
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
			Deceleration = 1.0;
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
