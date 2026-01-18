// ReSharper disable InconsistentNaming
namespace OpenBveApi.FunctionScripting
{
	/// <summary>The available instructions for use in a function script</summary>
	public enum Instructions {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		SystemConstant, SystemConstantArray,
		MathPlus, MathSubtract, MathMinus, MathTimes, MathDivide, MathReciprocal, MathPower, 
		MathIncrement, MathDecrement, MathFusedMultiplyAdd,
		MathQuotient, MathMod, MathFloor, MathCeiling, MathRound, MathMin, MathMax, MathAbs, MathSign,
		MathExp, MathLog, MathSqrt, MathSin, MathCos, MathTan, MathArcTan, MathPi,
		CompareEqual, CompareUnequal, CompareLess, CompareGreater, CompareLessEqual, CompareGreaterEqual, CompareConditional,
		LogicalNot, LogicalAnd, LogicalOr, LogicalNand, LogicalNor, LogicalXor,
		/*
		 * Functions after this point may not return the same result when itinerated twice
		 */
		SystemHalt,   SystemValue, SystemDelta,
		StackCopy, StackSwap,
		MathRandom, MathRandomInt,
		TimeSecondsSinceMidnight, TimeHourDigit, TimeMinuteDigit, TimeSecondDigit, CameraDistance, CameraXDistance, CameraYDistance, CameraZDistance,CameraView,
		TrainCars, TrainCarNumber, TrainDestination, PlayerTrain, TrainLength,
		TrainSpeed, TrainSpeedometer, TrainAcceleration, TrainAccelerationMotor,
		
		PlayerTrainDistance, TrainDistance,PlayerTrackDistance, TrainTrackDistance, CurveRadius, FrontAxleCurveRadius, RearAxleCurveRadius, CurveCant, Pitch, Odometer,
		Doors, DoorsIndex, FrontCoupler, RearCoupler, 
		LeftDoors,  RightDoors,
		LeftDoorsTarget,  RightDoorsTarget,
		LeftDoorButton, RightDoorButton, PilotLamp, Headlights,
		ReverserNotch, PowerNotch, PowerNotches, LocoBrakeNotch, LocoBrakeNotches, BrakeNotch, BrakeNotches, BrakeNotchLinear, BrakeNotchesLinear, EmergencyBrake, Klaxon, PrimaryKlaxon, SecondaryKlaxon, MusicKlaxon,
		HasAirBrake, HoldBrake, HasHoldBrake, ConstSpeed, HasConstSpeed,
		BrakeMainReservoir, BrakeEqualizingReservoir, BrakeBrakePipe, BrakeBrakeCylinder, BrakeStraightAirPipe,
		SafetyPluginAvailable, SafetyPluginState, PassAlarm, StationAdjustAlarm,
		TimetableVisible, Panel2Timetable, DistanceNextStation, DistanceLastStation, StopsNextStation, DistanceStation, StopsStation, NextStation, NextStationStop, TerminalStation,
		RouteLimit, SectionLimit,
		SectionAspectNumber, CurrentObjectState,
		RainDrop, SnowFlake, WiperPosition, WiperState,
		PantographState, 
		WheelRadius, 
		WheelSlip, 
		Sanders, SandLevel, SandShots, DSD,
		AmbientTemperature,
		BillboardX, BillboardY,
		EngineRunning, EngineRPM, FuelLevel, FuelLevelCar, Amps, AmpsCar,
		OverheadVolts,ThirdRailVolts,  FourthRailVolts, 
		OverheadAC, ThirdRailAC, FourthRailAC,
		OverheadHeight, ThirdRailHeight,  FourthRailHeight, 
		OverheadAmps, ThirdRailAmps, FourthRailAmps, 

		/// <summary>DUMMY</summary>
		/// <remarks>Used when correcting index dependant functions</remarks>
		CarIndexDependant = 1000,
		TrainDistanceToCar, TrainTrackDistanceToCar, CurveRadiusOfCar, FrontAxleCurveRadiusOfCar, RearAxleCurveRadiusOfCar, CurveCantOfCar, PitchOfCar, OdometerOfCar, BrightnessOfCar,
		FrontCouplerIndex, RearCouplerIndex,
		TrainSpeedOfCar, TrainSpeedometerOfCar, TrainAccelerationOfCar, TrainAccelerationMotorOfCar,
		LeftDoorsIndex, RightDoorsIndex,
		LeftDoorsTargetIndex, RightDoorsTargetIndex,
		BrakeMainReservoirOfCar, BrakeEqualizingReservoirOfCar, BrakeBrakePipeOfCar, BrakeBrakeCylinderOfCar, BrakeStraightAirPipeOfCar,
		PantographStateOfCar,
		CylinderCocksStateOfCar, BlowersStateOfCar,
		WheelRadiusOfCar,
		WheelSlipCar,
		EngineRunningCar,
		EngineRPMCar, EnginePowerCar,
		OverheadVoltsTarget, ThirdRailVoltsTarget, FourthRailVoltsTarget,
		OverheadHeightTarget, ThirdRailHeightTarget, FourthRailHeightTarget,
		OverheadAmpsTarget, ThirdRailAmpsTarget, FourthRailAmpsTarget,

#pragma warning restore CS1591

	}
}
