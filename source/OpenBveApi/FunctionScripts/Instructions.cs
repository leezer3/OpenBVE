﻿namespace OpenBveApi.FunctionScripting
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
		TrainSpeedOfCar, TrainSpeedometerOfCar, TrainAccelerationOfCar, TrainAccelerationMotorOfCar,
		PlayerTrainDistance, TrainDistance, TrainDistanceToCar, PlayerTrackDistance, TrainTrackDistance, TrainTrackDistanceToCar, CurveRadius, CurveRadiusOfCar, FrontAxleCurveRadius, FrontAxleCurveRadiusOfCar, RearAxleCurveRadius, RearAxleCurveRadiusOfCar, CurveCant, CurveCantOfCar, Pitch, PitchOfCar, Odometer, OdometerOfCar, BrightnessOfCar,
		Doors, DoorsIndex, FrontCoupler, FrontCouplerIndex, RearCoupler, RearCouplerIndex,
		LeftDoors, LeftDoorsIndex, RightDoors, RightDoorsIndex,
		LeftDoorsTarget, LeftDoorsTargetIndex, RightDoorsTarget, RightDoorsTargetIndex,
		LeftDoorButton, RightDoorButton, PilotLamp, Headlights,
		ReverserNotch, PowerNotch, PowerNotches, LocoBrakeNotch, LocoBrakeNotches, BrakeNotch, BrakeNotches, BrakeNotchLinear, BrakeNotchesLinear, EmergencyBrake, Klaxon, PrimaryKlaxon, SecondaryKlaxon, MusicKlaxon,
		HasAirBrake, HoldBrake, HasHoldBrake, ConstSpeed, HasConstSpeed,
		BrakeMainReservoir, BrakeEqualizingReservoir, BrakeBrakePipe, BrakeBrakeCylinder, BrakeStraightAirPipe,
		BrakeMainReservoirOfCar, BrakeEqualizingReservoirOfCar, BrakeBrakePipeOfCar, BrakeBrakeCylinderOfCar, BrakeStraightAirPipeOfCar,
		SafetyPluginAvailable, SafetyPluginState, PassAlarm, StationAdjustAlarm,
		TimetableVisible, Panel2Timetable, DistanceNextStation, DistanceLastStation, StopsNextStation, DistanceStation, StopsStation, NextStation, NextStationStop, TerminalStation,
		RouteLimit, SectionLimit,
		SectionAspectNumber, CurrentObjectState,
		RainDrop, SnowFlake, WiperPosition, WiperState,
		WheelRadius, WheelRadiusOfCar,
		WheelSlip, WheelSlipCar,
		Sanders, SandLevel, SandShots, DSD,
		AmbientTemperature,
		BillboardX, BillboardY,
		EngineRunning, EngineRunningCar, EngineRPM, EngineRPMCar, FuelLevel, FuelLevelCar, Amps, AmpsCar
#pragma warning restore CS1591
			
		}
}
