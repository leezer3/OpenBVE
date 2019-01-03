namespace OpenBveApi.FunctionScripting
{
	/// <summary>The available instructions for use in a function script</summary>
	public enum Instructions {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		SystemConstant, SystemConstantArray,
		MathPlus, MathSubtract, MathMinus, MathTimes, MathDivide, MathReciprocal, MathPower, 
		MathIncrement, MathDecrement, MathFusedMultiplyAdd,
		MathQuotient, MathMod, MathFloor, MathCeiling, MathRound, MathMin, MathMax, MathAbs, MathSign,
		MathExp, MathLog, MathSqrt, MathSin, MathCos, MathTan, MathArcTan,
		CompareEqual, CompareUnequal, CompareLess, CompareGreater, CompareLessEqual, CompareGreaterEqual, CompareConditional,
		LogicalNot, LogicalAnd, LogicalOr, LogicalNand, LogicalNor, LogicalXor,
		/*
		 * Functions after this point may not return the same result when itinerated twice
		 */
		SystemHalt,   SystemValue, SystemDelta,
		StackCopy, StackSwap,
		MathRandom, MathRandomInt,
		TimeSecondsSinceMidnight, CameraDistance,CameraView,
		TrainCars, TrainDestination,
		TrainSpeed, TrainSpeedometer, TrainAcceleration, TrainAccelerationMotor,
		TrainSpeedOfCar, TrainSpeedometerOfCar, TrainAccelerationOfCar, TrainAccelerationMotorOfCar,
		TrainDistance, TrainDistanceToCar, TrainTrackDistance, TrainTrackDistanceToCar, CurveRadius, CurveRadiusOfCar, FrontAxleCurveRadius, FrontAxleCurveRadiusOfCar, RearAxleCurveRadius, RearAxleCurveRadiusOfCar, CurveCant, CurveCantOfCar, Pitch, PitchOfCar, Odometer, OdometerOfCar,
		Doors, DoorsIndex,
		LeftDoors, LeftDoorsIndex, RightDoors, RightDoorsIndex,
		LeftDoorsTarget, LeftDoorsTargetIndex, RightDoorsTarget, RightDoorsTargetIndex,
		LeftDoorButton, RightDoorButton,
		ReverserNotch, PowerNotch, PowerNotches, LocoBrakeNotch, LocoBrakeNotches, BrakeNotch, BrakeNotches, BrakeNotchLinear, BrakeNotchesLinear, EmergencyBrake, Klaxon, PrimaryKlaxon, SecondaryKlaxon, MusicKlaxon,
		HasAirBrake, HoldBrake, HasHoldBrake, ConstSpeed, HasConstSpeed,
		BrakeMainReservoir, BrakeEqualizingReservoir, BrakeBrakePipe, BrakeBrakeCylinder, BrakeStraightAirPipe,
		BrakeMainReservoirOfCar, BrakeEqualizingReservoirOfCar, BrakeBrakePipeOfCar, BrakeBrakeCylinderOfCar, BrakeStraightAirPipeOfCar,
		SafetyPluginAvailable, SafetyPluginState,
		TimetableVisible,
		SectionAspectNumber, CurrentObjectState
#pragma warning restore CS1591
			
		}
}
