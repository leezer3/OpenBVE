using System;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.FunctionScripting
{
	/// <summary>The base abstract function script which consumers must implement</summary>
	public class FunctionScript : AnimationScript
	{
		private readonly HostInterface currentHost;
		/// <summary>The instructions to perform</summary>
		public readonly Instructions[] InstructionSet;
		/// <summary>The stack for the script</summary>
		public readonly double[] Stack;
		/// <summary>All constants used for the script</summary>
		public readonly double[] Constants;
		/// <summary>The last result returned</summary>
		public double LastResult { get; set; }
		/// <summary>The minimum pinned result or NaN to set no minimum</summary>
		public double Maximum { get; set; } = Double.NaN;
		/// <summary>The maximum pinned result or NaN to set no maximum</summary>
		public double Minimum { get; set; } = Double.NaN;
		/// <summary>We caught an exception on the last execution of the script, so further execution has been stopped</summary> 
		private bool exceptionCaught;

		/// <summary>Performs the function script, and returns the current result</summary>
		public double ExecuteScript(AbstractTrain Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			if (exceptionCaught)
			{
				return 0;
			}
			try
			{
				currentHost.ExecuteFunctionScript(this, Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
			}
			catch(Exception ex)
			{
				if (!exceptionCaught)
				{
					currentHost.AddMessage(MessageType.Error, false, ex.Message);
					exceptionCaught = true;
				}
				
				this.LastResult = 0;
				return 0;
			}
			

			//Allows us to pin the result, but keep the underlying figure
			if (this.Minimum != Double.NaN & this.LastResult < Minimum)
			{
				return Minimum;
			}
			if (this.Maximum != Double.NaN & this.LastResult > Maximum)
			{
				return Maximum;
			}
			return this.LastResult;
		}

		/// <summary>Checks whether the specified function will return a constant result</summary>
		public bool ConstantResult()
		{
			if (InstructionSet.Length == 1 && InstructionSet[0] == Instructions.SystemConstant)
			{
				return true;
			}
			for (int i = 0; i < InstructionSet.Length; i++)
			{
				if ((int) InstructionSet[i] >= (int) Instructions.LogicalXor)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>Creates a new empty function script</summary>
		/// <param name="Host">A reference to the base application host interface</param>
		public FunctionScript(HostInterface Host)
		{
			currentHost = Host;
		}


		/// <summary>Creates a new function script</summary>
		/// <param name="Host">A reference to the base application host interface</param>
		/// <param name="Expression">The function string</param>
		/// <param name="Infix">Whether this is in Infix notation (TRUE) or Postfix notation (FALSE)</param>
		public FunctionScript(HostInterface Host, string Expression, bool Infix)
		{
			currentHost = Host;
			if (Infix)
			{
				//If in infix format, we must convert to postfix first
				Expression = FunctionScriptNotation.GetFunctionNotationFromInfixNotation(Expression, true);
				Expression = FunctionScriptNotation.GetPostfixNotationFromFunctionNotation(Expression);
			}
			Expression = FunctionScriptNotation.GetOptimizedPostfixNotation(Expression);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Arguments = Expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			InstructionSet = new Instructions[16]; int n = 0;
			Stack = new double[16]; int m = 0, s = 0;
			Constants = new double[16]; int c = 0;
			for (int i = 0; i < Arguments.Length; i++) {
				double d; if (double.TryParse(Arguments[i], System.Globalization.NumberStyles.Float, Culture, out d)) {
					if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
					InstructionSet[n] = Instructions.SystemConstant;
					if (c >= Constants.Length) Array.Resize(ref Constants, Constants.Length << 1);
					Constants[c] = d;
					n++; c++; s++; if (s >= m) m = s;
				} else if (Time.TryParseTime(Arguments[i], out d) && InstructionSet[n -1] == Instructions.TimeSecondsSinceMidnight) {
					if (c >= Constants.Length) Array.Resize(ref Constants, Constants.Length << 1);
					Constants[c] = d;
					n++;
					c++;
					s++;
					if (s >= m) m = s;
				}
				else {
					if (Arguments[i].IndexOf(':') != -1)
					{
						//The colon is required for formatting times, so exclude it from the initial character check, & do it here instead
						throw new System.IO.InvalidDataException("Invalid character encountered in variable " + Expression);
					}
					if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
					switch (Arguments[i].ToLowerInvariant()) {
							// system
						case "halt":
							throw new InvalidOperationException("The halt instruction was encountered in function script " + Expression);
						case "value":
							InstructionSet[n] = Instructions.SystemValue;
							n++; s++; if (s >= m) m = s; break;
						case "delta":
							InstructionSet[n] = Instructions.SystemDelta;
							n++; s++; if (s >= m) m = s; break;
							// stack
						case "~":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.StackCopy;
							n++; s++; if (s >= m) m = s; break;
						case "<>":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.StackSwap;
							n++; break;
							// math
						case "+":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathPlus;
							n++; s--; break;
						case "-":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathSubtract;
							n++; s--; break;
						case "minus":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathMinus;
							n++; break;
						case "*":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathTimes;
							n++; s--; break;
						case "/":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathDivide;
							n++; s--; break;
						case "reciprocal":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathReciprocal;
							n++; break;
						case "power":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathPower;
							n++; s--; break;
						case "++":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathIncrement;
							n++; break;
						case "--":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathDecrement;
							n++; break;
						case "fma":
							if (s < 3) throw new InvalidOperationException(Arguments[i] + " requires at least 3 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathFusedMultiplyAdd;
							n++; s -= 2; break;
						case "quotient":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathQuotient;
							n++; s--; break;
						case "mod":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathMod;
							n++; s--; break;
						case "random":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathRandom;
							n++; s--; break;
						case "randomint":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathRandomInt;
							n++; s--; break;
						case "floor":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathFloor;
							n++; break;
						case "ceiling":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathCeiling;
							n++; break;
						case "round":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathRound;
							n++; break;
						case "min":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathMin;
							n++; s--; break;
						case "max":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathMax;
							n++; s--; break;
						case "abs":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathAbs;
							n++; break;
						case "sign":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathSign;
							n++; break;
						case "exp":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathExp;
							n++; break;
						case "log":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathLog;
							n++; break;
						case "sqrt":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathSqrt;
							n++; break;
						case "sin":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathSin;
							n++; break;
						case "cos":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathCos;
							n++; break;
						case "tan":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathTan;
							n++; break;
						case "arctan":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.MathArcTan;
							n++; break;
						case "pi":
							InstructionSet[n] = Instructions.MathPi;
							n++; s++; if (s >= m) m = s; break;
						case "==":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareEqual;
							n++; s--; break;
						case "!=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareUnequal;
							n++; s--; break;
							// conditionals
						case "<":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (Arguments[i - 2].ToLowerInvariant() == "cars")
							{
								int nCars;
								NumberFormats.TryParseIntVb6(Arguments[i - 1], out nCars);
								if (System.Math.Abs(nCars) != nCars)
								{
									//It makes absolutely no sense to test whether there are less than 0 cars in a train, so let's at least throw a broken script error
									throw new InvalidOperationException("Cannot test against less than zero cars in function script " + Expression);
								}
							}
							InstructionSet[n] = Instructions.CompareLess;
							n++; s--; break;
						case ">":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareGreater;
							n++; s--; break;
						case "<=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareLessEqual;
							n++; s--; break;
						case ">=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareGreaterEqual;
							n++; s--; break;
						case "?":
							if (s < 3) throw new InvalidOperationException(Arguments[i] + " requires at least 3 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CompareConditional;
							n++; s -= 2; break;
							// logical
						case "!":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalNot;
							n++; break;
						case "&":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalAnd;
							n++; s--; break;
						case "|":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalOr;
							n++; s--; break;
						case "!&":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalNand;
							n++; s--; break;
						case "!|":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalNor;
							n++; s--; break;
						case "^":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LogicalXor;
							n++; s--; break;
							// time/camera
						case "time":
							InstructionSet[n] = Instructions.TimeSecondsSinceMidnight;
							n++; s++; if (s >= m) m = s; break;
						case "hour":
							InstructionSet[n] = Instructions.TimeHourDigit;
							n++; s++; if (s >= m) m = s; break;
						case "minute":
							InstructionSet[n] = Instructions.TimeMinuteDigit;
							n++; s++; if (s >= m) m = s; break;
						case "second":
							InstructionSet[n] = Instructions.TimeSecondDigit;
							n++; s++; if (s >= m) m = s; break;
						case "cameradistance":
							InstructionSet[n] = Instructions.CameraDistance;
							n++; s++; if (s >= m) m = s; break;
						case "cameraxdistance":
							InstructionSet[n] = Instructions.CameraXDistance;
							n++; s++; if (s >= m) m = s; break;
						case "cameraydistance":
							InstructionSet[n] = Instructions.CameraYDistance;
							n++; s++; if (s >= m) m = s; break;
						case "camerazdistance":
							InstructionSet[n] = Instructions.CameraZDistance;
							n++; s++; if (s >= m) m = s; break;
						case "cameramode":
							InstructionSet[n] = Instructions.CameraView;
							n++; s++; if (s >= m) m = s; break;
							// train
						case "playertrain":
							InstructionSet[n] = Instructions.PlayerTrain;
							n++; s++; if (s >= m) m = s; break;
						case "cars":
							InstructionSet[n] = Instructions.TrainCars;
							n++; s++; if (s >= m) m = s; break;
						case "destination":
							InstructionSet[n] = Instructions.TrainDestination;
							n++; s++; if (s >= m) m = s; break;
						case "speed":
							InstructionSet[n] = Instructions.TrainSpeed;
							n++; s++; if (s >= m) m = s; break;
						case "speedindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainSpeedOfCar;
							n++; break;
						case "speedometer":
							InstructionSet[n] = Instructions.TrainSpeedometer;
							n++; s++; if (s >= m) m = s; break;
						case "speedometerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainSpeedometerOfCar;
							n++; break;
						case "acceleration":
							InstructionSet[n] = Instructions.TrainAcceleration;
							n++; s++; if (s >= m) m = s; break;
						case "accelerationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainAccelerationOfCar;
							n++; break;
						case "accelerationmotor":
							InstructionSet[n] = Instructions.TrainAccelerationMotor;
							n++; s++; if (s >= m) m = s; break;
						case "accelerationmotorindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainAccelerationMotorOfCar;
							n++; break;
						case "distance":
							InstructionSet[n] = Instructions.TrainDistance;
							n++; s++; if (s >= m) m = s; break;
						case "distanceindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainDistanceToCar;
							n++; break;
						case "trackdistance":
							InstructionSet[n] = Instructions.TrainTrackDistance;
							n++; s++; if (s >= m) m = s; break;
						case "frontaxlecurveradius":
							InstructionSet[n] = Instructions.FrontAxleCurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "frontaxlecurveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.FrontAxleCurveRadiusOfCar;
							n++; break;
						case "rearaxlecurveradius":
							InstructionSet[n] = Instructions.RearAxleCurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "rearaxlecurveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.RearAxleCurveRadiusOfCar;
							n++; break;
						case "curveradius":
							InstructionSet[n] = Instructions.CurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "curveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CurveRadiusOfCar;
							n++; break;
						case "curvecant":
							InstructionSet[n] = Instructions.CurveCant;
							n++; s++; if (s >= m) m = s; break;
						case "curvecantindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.CurveCantOfCar;
							n++; break;
						case "pitch":
							InstructionSet[n] = Instructions.Pitch;
							n++; s++; if (s >= m) m = s; break;
						case "pitchindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.PitchOfCar;
							n++; break;
						case "odometer":
							InstructionSet[n] = Instructions.Odometer;
							n++; s++; if (s >= m) m = s;  break;
						case "odometerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.OdometerOfCar;
							n++; break;
						case "trackdistanceindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.TrainTrackDistanceToCar;
							n++; break;
							// train: doors
						case "doors":
							InstructionSet[n] = Instructions.Doors;
							n++; s++; if (s >= m) m = s; break;
						case "doorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.DoorsIndex;
							n++; break;
						case "leftdoors":
							InstructionSet[n] = Instructions.LeftDoors;
							n++; s++; if (s >= m) m = s; break;
						case "leftdoorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LeftDoorsIndex;
							n++; break;
						case "rightdoors":
							InstructionSet[n] = Instructions.RightDoors;
							n++; s++; if (s >= m) m = s; break;
						case "rightdoorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.RightDoorsIndex;
							n++; break;
						case "leftdoorstarget":
							InstructionSet[n] = Instructions.LeftDoorsTarget;
							n++; s++; if (s >= m) m = s; break;
						case "leftdoorstargetindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.LeftDoorsTargetIndex;
							n++; break;
						case "rightdoorstarget":
							InstructionSet[n] = Instructions.RightDoorsTarget;
							n++; s++; if (s >= m) m = s; break;
						case "rightdoorstargetindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.RightDoorsTargetIndex;
							n++; break;
						case "doorbuttonl":
						case "leftdoorbutton":
							InstructionSet[n] = Instructions.LeftDoorButton;
							n++; s++; if (s >= m) m = s; break;
						case "doorbuttonr":
						case "rightdoorbutton":
							InstructionSet[n] = Instructions.RightDoorButton;
							n++; s++; if (s >= m) m = s; break;
						case "pilotlamp":
							InstructionSet[n] = Instructions.PilotLamp;
							n++; s++; if (s >= m) m = s; break;
						case "passalarm":
							InstructionSet[n] = Instructions.PassAlarm;
							n++; s++; if (s >= m) m = s; break;
						case "stationadjustalarm":
							InstructionSet[n] = Instructions.StationAdjustAlarm;
							n++; s++; if (s >= m) m = s; break;
						case "headlights":
							InstructionSet[n] = Instructions.Headlights;
							n++; s++; if (s >= m) m = s; break;
						// train: handles
						case "reversernotch":
							InstructionSet[n] = Instructions.ReverserNotch;
							n++; s++; if (s >= m) m = s; break;
						case "powernotch":
							InstructionSet[n] = Instructions.PowerNotch;
							n++; s++; if (s >= m) m = s; break;
						case "powernotches":
							InstructionSet[n] = Instructions.PowerNotches;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotch":
							InstructionSet[n] = Instructions.BrakeNotch;
							n++; s++; if (s >= m) m = s; break;
						case "locobrakenotch":
							InstructionSet[n] = Instructions.LocoBrakeNotch;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotches":
							InstructionSet[n] = Instructions.BrakeNotches;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotchlinear":
							InstructionSet[n] = Instructions.BrakeNotchLinear;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotcheslinear":
							InstructionSet[n] = Instructions.BrakeNotchesLinear;
							n++; s++; if (s >= m) m = s; break;
						case "emergencybrake":
							InstructionSet[n] = Instructions.EmergencyBrake;
							n++; s++; if (s >= m) m = s; break;
						case "horn":
						case "klaxon":
							InstructionSet[n] = Instructions.Klaxon;
							n++; s++; if (s >= m) m = s; break;
						case "primaryhorn":
						case "primaryklaxon":
							InstructionSet[n] = Instructions.PrimaryKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "secondaryhorn":
						case "secondaryklaxon":
							InstructionSet[n] = Instructions.SecondaryKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "musichorn":
						case "musicklaxon":
							InstructionSet[n] = Instructions.MusicKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "hasairbrake":
							InstructionSet[n] = Instructions.HasAirBrake;
							n++; s++; if (s >= m) m = s; break;
						case "holdbrake":
							InstructionSet[n] = Instructions.HoldBrake;
							n++; s++; if (s >= m) m = s; break;
						case "hasholdbrake":
							InstructionSet[n] = Instructions.HasHoldBrake;
							n++; s++; if (s >= m) m = s; break;
						case "constspeed":
							InstructionSet[n] = Instructions.ConstSpeed;
							n++; s++; if (s >= m) m = s; break;
						case "hasconstspeed":
							InstructionSet[n] = Instructions.HasConstSpeed;
							n++; s++; if (s >= m) m = s; break;
							// train: brake
						case "mainreservoir":
							InstructionSet[n] = Instructions.BrakeMainReservoir;
							n++; s++; if (s >= m) m = s; break;
						case "mainreservoirindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrakeMainReservoirOfCar;
							n++; break;
						case "equalizingreservoir":
							InstructionSet[n] = Instructions.BrakeEqualizingReservoir;
							n++; s++; if (s >= m) m = s; break;
						case "equalizingreservoirindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrakeEqualizingReservoirOfCar;
							n++; break;
						case "brakepipe":
							InstructionSet[n] = Instructions.BrakeBrakePipe;
							n++; s++; if (s >= m) m = s; break;
						case "brakepipeindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrakeBrakePipeOfCar;
							n++; break;
						case "brakecylinder":
							InstructionSet[n] = Instructions.BrakeBrakeCylinder;
							n++; s++; if (s >= m) m = s; break;
						case "brakecylinderindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrakeBrakeCylinderOfCar;
							n++; break;
						case "straightairpipe":
							InstructionSet[n] = Instructions.BrakeStraightAirPipe;
							n++; s++; if (s >= m) m = s; break;
						case "straightairpipeindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrakeStraightAirPipeOfCar;
							n++; break;
							// train: safety
						case "hasplugin":
							InstructionSet[n] = Instructions.SafetyPluginAvailable;
							n++; s++; if (s >= m) m = s; break;
						case "pluginstate":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.SafetyPluginState;
							n++; break;
							// train: timetable
						case "timetable":
							InstructionSet[n] = Instructions.TimetableVisible;
							n++; s++; if (s >= m) m = s; break;
						case "panel2timetable":
							//This is an internal function, and does not form part of the documented API
							//Used for the [Timetable] section in panel2 trains
							InstructionSet[n] = Instructions.Panel2Timetable;
							n++; s++; if (s >= m) m = s; break;
						case "distancenextstation":
							InstructionSet[n] = Instructions.DistanceNextStation;
							n++; s++; if (s >= m) m = s; break;
						case "distancelaststation":
							InstructionSet[n] = Instructions.DistanceLastStation;
							n++; s++; if (s >= m) m = s; break;
						case "stopsnextstation":
							InstructionSet[n] = Instructions.StopsNextStation;
							n++; s++; if (s >= m) m = s; break;
						case "distancestationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.DistanceStation;
							n++; break;
						case "stopsstationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.StopsStation;
							n++; break;
						case "terminalstation":
							InstructionSet[n] = Instructions.TerminalStation;
							n++; s++; if (s >= m) m = s; break;
						case "nextstation":
							InstructionSet[n] = Instructions.NextStation;
							n++; s++; if (s >= m) m = s; break;
						case "nextstationstop":
							InstructionSet[n] = Instructions.NextStationStop;
							n++; s++; if (s >= m) m = s; break;
						case "routelimit":
							InstructionSet[n] = Instructions.RouteLimit;
							n++; s++; if (s >= m) m = s; break;
							// sections
						case "section":
							InstructionSet[n] = Instructions.SectionAspectNumber;
							n++; s++; if (s >= m) m = s; break;
							// state
						case "currentstate":
							InstructionSet[n] = Instructions.CurrentObjectState;
							n++; s++; if (s >= m) m = s; break;
							// windscreen and raindrops
						case "raindrop":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.RainDrop;
							n++; break;
						case "snowflake":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.SnowFlake;
							n++; break;
						case "wiperposition":
							InstructionSet[n] = Instructions.WiperPosition;
							n++; s++; if (s >= m) m = s; break;
						case "brightnessindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.BrightnessOfCar;
							n++; break;
						case "carnumber":
							InstructionSet[n] = Instructions.TrainCarNumber;
							n++; s++; if (s >= m) m = s; break;
						case "wheelradius":
							InstructionSet[n] = Instructions.WheelRadius;
							n++; s++; if (s >= m) m = s; break;
						case "wheelradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.WheelRadiusOfCar;
							n++; break;
						case "wheelslip":
							InstructionSet[n] = Instructions.WheelSlip;
							n++; s++; if (s >= m) m = s; break;
						case "wheelslipindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							InstructionSet[n] = Instructions.WheelSlipCar;
							n++; break;
						case "boilerpressure":
							InstructionSet[n] = Instructions.BoilerPressure;
							n++; s++; if (s >= m) m = s; break;
						case "boilerwater":
							InstructionSet[n] = Instructions.BoilerWaterLevel;
							n++; s++; if (s >= m) m = s; break;
						case "cutoff":
							InstructionSet[n] = Instructions.Cutoff;
							n++; s++; if (s >= m) m = s; break;
						case "blowers":
							InstructionSet[n] = Instructions.Blowers;
							n++; s++; if (s >= m) m = s; break;
						case "cylindercocks":
							InstructionSet[n] = Instructions.CylinderCocks;
							n++; s++; if (s >= m) m = s; break;
						case "bypassvalve":
							InstructionSet[n] = Instructions.BypassValve;
							n++; s++; if (s >= m) m = s; break;
						case "livesteaminjector":
							InstructionSet[n] = Instructions.LiveSteamInjector;
							n++; s++; if (s >= m) m = s; break;
						case "exhauststeaminjector":
							InstructionSet[n] = Instructions.ExhaustSteamInjector;
							n++; s++; if (s >= m) m = s; break;
						case "firearea":
							InstructionSet[n] = Instructions.FireArea;
							n++; s++; if (s >= m) m = s; break;
						case "firemass":
							InstructionSet[n] = Instructions.FireMass;
							n++; s++; if (s >= m) m = s; break;
						case "firetemperature":
							InstructionSet[n] = Instructions.FireTemperature;
							n++; s++; if (s >= m) m = s; break;
						case "tenderwater":
						case "tankswater":
							InstructionSet[n] = Instructions.TenderWater;
							n++; s++; if (s >= m) m = s; break;
						case "tenderfuel":
						case "tanksfuel":
							InstructionSet[n] = Instructions.TenderFuel;
							n++; s++; if (s >= m) m = s; break;
						case "sanders":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Sanders;
							n++; s++; if (s >= m) m = s; break;
						case "sandlevel":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SandLevel;
							n++; s++; if (s >= m) m = s; break;
						case "sandshots":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SandShots;
							n++; s++; if (s >= m) m = s; break;
						case "valvegearwheelposition":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ValveGearWheelPosition;
							n++; s++; if (s >= m) m = s; break;
						case "valvegearleftpivotx":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ValveGearLeftPivotX;
							n++; s++; if (s >= m) m = s; break;
						case "valvegearleftpivoty":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ValveGearLeftPivotY;
							n++; s++; if (s >= m) m = s; break;
						case "valvegearrightpivotx":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ValveGearRightPivotX;
							n++; s++; if (s >= m) m = s; break;
						case "valvegearrightpivoty":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ValveGearRightPivotY;
							n++; s++; if (s >= m) m = s; break;
						// default
						default:
							throw new System.IO.InvalidDataException("Unknown command " + Arguments[i] + " encountered in function script " + Expression);
					}
				}
			}
			if (s != 1) {
				throw new InvalidOperationException("There must be exactly one argument left on the stack at the end in function script " + Expression);
			}
			Array.Resize(ref InstructionSet, n);
			Array.Resize(ref Stack, m);
			Array.Resize(ref Constants, c);
		}
		

		/// <summary>Clones the function script</summary>
		public AnimationScript Clone()
		{
			return (FunctionScript) this.MemberwiseClone();
		}
	}
}
