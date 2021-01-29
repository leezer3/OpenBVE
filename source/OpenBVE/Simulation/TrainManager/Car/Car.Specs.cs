using TrainManager.Car;
using TrainManager.Power;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		public class CarSpecs : CarPhysics
		{
			/// motor
			internal AccelerationCurve[] AccelerationCurves;
			internal double AccelerationCurveMaximum;
			
			internal double CurrentPitchDueToAccelerationAngle;
			internal double CurrentPitchDueToAccelerationAngularSpeed;
			internal double CurrentPitchDueToAccelerationTargetAngle;
			internal double CurrentPitchDueToAccelerationFastValue;
			internal double CurrentPitchDueToAccelerationMediumValue;
			internal double CurrentPitchDueToAccelerationSlowValue;
			/// systems
			internal CarHoldBrake HoldBrake;
			internal CarConstSpeed ConstSpeed;
			internal CarReAdhesionDevice ReAdhesionDevice;

			/// doors
			
			internal double DoorOpenFrequency;
			internal double DoorCloseFrequency;
			internal double DoorOpenPitch;
			internal double DoorClosePitch;
		}
	}
}
