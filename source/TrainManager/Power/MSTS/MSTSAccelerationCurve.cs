using TrainManager.Trains;

namespace TrainManager.Power
{
	/// <summary>Represents a MSTS acceleration curve</summary>
	public class MSTSAccelerationCurve : AccelerationCurve
	{
		private readonly TrainBase Train;
		/// <summary>The maximum force supplied by the engine</summary>
		private readonly double MaxForce;

		public MSTSAccelerationCurve(TrainBase train, double maxForce)
		{
			Train = train;
			MaxForce = maxForce;
		}

		public override double GetAccelerationOutput(double Speed)
		{
			/*
			 * According to Newton's second law, Acceleration = Force / Mass
			 * For the minute at least, we'll assume that the Force value specified in an
			 * ENG file is proportionate across power notches, and remains constant throughout
			 * the speed range. In practice, MSTS will actually have internally simulated this
			 * via engine RPM, boiler pressure etc. etc. but at present we obviously don't
			 * have these available.
			 *
			 * We don't in this case need to take the speed or loading values into account, but
			 * retain them as legacy
			 * REFACTOR: Store the train reference in BVE acceleration curves???
			 */
			double totalMass = 0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				totalMass += Train.Cars[i].CurrentMass;
			}

			if (Train.Handles.EmergencyBrake.Actual)
			{
				return totalMass / MaxForce / 3.6;
			}

			if (Train.Handles.Brake.Actual > 0)
			{
				return ((Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch) *  (totalMass / MaxForce)) / 3.6;
			}

			return ((Train.Handles.Power.Actual / (double)Train.Handles.Power.MaximumNotch) *  (totalMass / MaxForce)) / 3.6;

		}

		public override double MaximumAcceleration
		{
			get
			{
				double totalMass = 0;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					totalMass += Train.Cars[i].CurrentMass;
				}

				return totalMass / MaxForce;
			}
		}

	}
	public class MSTSDecelerationCurve : AccelerationCurve
	{
		private readonly TrainBase Train;
		/// <summary>The maximum force supplied by the engine</summary>
		private readonly double MaxForce;

		public MSTSDecelerationCurve(TrainBase train, double maxForce)
		{
			Train = train;
			MaxForce = maxForce;
		}
		public override double GetAccelerationOutput(double Speed)
		{
			/*
			 * According to Newton's second law, Acceleration = Force / Mass
			 * For the minute at least, we'll assume that the Force value specified in an
			 * ENG file is proportionate across power notches, and remains constant throughout
			 * the speed range. In practice, MSTS will actually have internally simulated this
			 * via engine RPM, boiler pressure etc. etc. but at present we obviously don't
			 * have these available.
			 *
			 * We don't in this case need to take the speed or loading values into account, but
			 * retain them as legacy
			 * REFACTOR: Store the train reference in BVE acceleration curves???
			 */
			double totalMass = 0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				totalMass += Train.Cars[i].CurrentMass;
			}

			if (Train.Handles.EmergencyBrake.Actual)
			{
				return totalMass / MaxForce / 3.6;
			}

			return ((Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch) *  (totalMass / MaxForce)) / 3.6;
		}

		public override double MaximumAcceleration
		{
			get
			{
				double totalMass = 0;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					totalMass += Train.Cars[i].CurrentMass;
				}

				return totalMass / MaxForce;
			}
		}
	}
}
