namespace TrainManager.Car
{
	/// <summary>Contains the physics related properties for the car</summary>
	public class CarPhysics
	{
		/// <summary>The current perceived speed of the car</summary>
		/// <remarks>Appears as if on a speedometer, accounting for wheel lock etc.</remarks>
		public double PerceivedSpeed;
		/// <summary>The current total acceleration output supplied by the car from all sources</summary>
		/// <remarks>Is positive for power and negative for brake, regardless of the train's direction</remarks>
		public double Acceleration;
		/// <summary>The jerk applied when the acceleration from the motor increases</summary>
		public double JerkPowerUp;
		/// <summary>The jerk applied when the acceleration from the motor decreases</summary>
		public double JerkPowerDown;
		/// <summary>The body roll / shake direction</summary>
		public double RollShakeDirection;
		/// <summary>The amount of roll in radians applied due to toppling</summary>
		public double RollDueToTopplingAngle;
		/// <summary>The amount of roll in radians applied due to the cant of the track</summary>
		public double RollDueToCantAngle;
		/// <summary>The amount of roll in radians applied due to shaking of the train</summary>
		public double RollDueToShakingAngle;
		/// <summary>The angular speed of the roll applied due to shaking of the train</summary>
		public double RollDueToShakingAngularSpeed;
		/// <summary>The pitch applied due to acceleration in radians</summary>
		public double PitchDueToAccelerationAngle;
		/// <summary>The angular speed of the pitch applied due to acceleration</summary>
		public double PitchDueToAccelerationAngularSpeed;
		/// <summary>The target angle of the pitch</summary>
		public double PitchDueToAccelerationTargetAngle;
		/// <summary>The fast update value for pitch</summary>
		public double PitchDueToAccelerationFastValue;
		/// <summary>The medium update value for pitch</summary>
		public double PitchDueToAccelerationMediumValue;
		/// <summary>The slow update value for pitch</summary>
		public double PitchDueToAccelerationSlowValue;
		/// <summary>The exposed frontal area of the car</summary>
		public double ExposedFrontalArea;
		/// <summary>The unexposed frontal area of the car</summary>
		public double UnexposedFrontalArea;
		/// <summary>The center of gravity height above the rails</summary>
		public double CenterOfGravityHeight;
		/// <summary>The critical toppling angle</summary>
		public double CriticalTopplingAngle;
		/*
		 * Used to determine the opening and closing times of the doors
		 * TODO: Move into the doors struct
		 */
		public double DoorOpenFrequency;
		public double DoorCloseFrequency;
		public double DoorOpenPitch;
		public double DoorClosePitch;
	}
}
