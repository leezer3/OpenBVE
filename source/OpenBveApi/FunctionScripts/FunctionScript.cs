using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using System;

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
					// use different exception types to show warning vs error for stuff deliberately not implemented in viewers
					currentHost.AddMessage(ex is NotImplementedException ? MessageType.Warning : MessageType.Error, false, ex.Message);
					exceptionCaught = true;
				}
				
				this.LastResult = 0;
				return 0;
			}
			

			//Allows us to pin the result, but keep the underlying figure
			if (!double.IsNaN(this.Minimum) & this.LastResult < Minimum)
			{
				return Minimum;
			}
			if (!double.IsNaN(this.Maximum) & this.LastResult > Maximum)
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

		/// <summary>Called when the underlying object is reversed</summary>
		public void CorrectCarIndices(int offset)
		{
			int c = 0;
			for (int i = 0; i < InstructionSet.Length - 1; i++)
			{
				if (InstructionSet[i] == Instructions.SystemConstant)
				{
					// must have at least one instruction on the stack afterwards
					if (InstructionSet[i + 1] > Instructions.CarIndexDependant)
					{
						Constants[c] += offset;
					}
					c++;
				}

				if (InstructionSet[i] == Instructions.SystemConstantArray)
				{
					int n = (int)InstructionSet[i + 1];
					c += n;
				}
				
			}
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
				if (double.TryParse(Arguments[i], System.Globalization.NumberStyles.Float, Culture, out double d)) {
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
					switch (Arguments[i].ToLowerInvariant()) {
							// system
						case "halt":
							throw new InvalidOperationException("The halt instruction was encountered in function script " + Expression);
						case "value":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SystemValue;
							n++; s++; if (s >= m) m = s; break;
						case "delta":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SystemDelta;
							n++; s++; if (s >= m) m = s; break;
							// stack
						case "~":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.StackCopy;
							n++; s++; if (s >= m) m = s; break;
						case "<>":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.StackSwap;
							n++; break;
							// math
						case "+":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathPlus;
							n++; s--; break;
						case "-":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathSubtract;
							n++; s--; break;
						case "minus":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathMinus;
							n++; break;
						case "*":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathTimes;
							n++; s--; break;
						case "/":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathDivide;
							n++; s--; break;
						case "reciprocal":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathReciprocal;
							n++; break;
						case "power":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathPower;
							n++; s--; break;
						case "++":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathIncrement;
							n++; break;
						case "--":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathDecrement;
							n++; break;
						case "fma":
							if (s < 3) throw new InvalidOperationException(Arguments[i] + " requires at least 3 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathFusedMultiplyAdd;
							n++; s -= 2; break;
						case "quotient":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathQuotient;
							n++; s--; break;
						case "mod":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathMod;
							n++; s--; break;
						case "random":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathRandom;
							n++; s--; break;
						case "randomint":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathRandomInt;
							n++; s--; break;
						case "floor":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathFloor;
							n++; break;
						case "ceiling":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathCeiling;
							n++; break;
						case "round":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathRound;
							n++; break;
						case "min":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathMin;
							n++; s--; break;
						case "max":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathMax;
							n++; s--; break;
						case "abs":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathAbs;
							n++; break;
						case "sign":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathSign;
							n++; break;
						case "exp":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathExp;
							n++; break;
						case "log":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathLog;
							n++; break;
						case "sqrt":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathSqrt;
							n++; break;
						case "sin":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathSin;
							n++; break;
						case "cos":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathCos;
							n++; break;
						case "tan":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathTan;
							n++; break;
						case "arctan":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathArcTan;
							n++; break;
						case "pi":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MathPi;
							n++; s++; if (s >= m) m = s; break;
						case "==":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareEqual;
							n++; s--; break;
						case "!=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareUnequal;
							n++; s--; break;
							// conditionals
						case "<":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (Arguments[i - 2].ToLowerInvariant() == "cars")
							{
								if (NumberFormats.TryParseIntVb6(Arguments[i - 1], out int nCars))
								{
									if (System.Math.Abs(nCars) != nCars)
									{
										//It makes absolutely no sense to test whether there are less than 0 cars in a train, so let's at least throw a broken script error
										throw new InvalidOperationException("Cannot test against less than zero cars in function script " + Expression);
									}
								}
								else
								{
									throw new InvalidOperationException("Unexpected argument " + Arguments[i -1] + " in function script " + Expression);
								}
								
							}
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareLess;
							n++; s--; break;
						case ">":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareGreater;
							n++; s--; break;
						case "<=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareLessEqual;
							n++; s--; break;
						case ">=":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareGreaterEqual;
							n++; s--; break;
						case "?":
							if (s < 3) throw new InvalidOperationException(Arguments[i] + " requires at least 3 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CompareConditional;
							n++; s -= 2; break;
							// logical
						case "!":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalNot;
							n++; break;
						case "&":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalAnd;
							n++; s--; break;
						case "|":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalOr;
							n++; s--; break;
						case "!&":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalNand;
							n++; s--; break;
						case "!|":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalNor;
							n++; s--; break;
						case "^":
							if (s < 2) throw new InvalidOperationException(Arguments[i] + " requires at least 2 arguments on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LogicalXor;
							n++; s--; break;
							// time/camera
						case "time":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TimeSecondsSinceMidnight;
							n++; s++; if (s >= m) m = s; break;
						case "hour":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TimeHourDigit;
							n++; s++; if (s >= m) m = s; break;
						case "minute":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TimeMinuteDigit;
							n++; s++; if (s >= m) m = s; break;
						case "second":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TimeSecondDigit;
							n++; s++; if (s >= m) m = s; break;
						case "cameradistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CameraDistance;
							n++; s++; if (s >= m) m = s; break;
						case "cameraxdistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CameraXDistance;
							n++; s++; if (s >= m) m = s; break;
						case "cameraydistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CameraYDistance;
							n++; s++; if (s >= m) m = s; break;
						case "camerazdistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CameraZDistance;
							n++; s++; if (s >= m) m = s; break;
						case "billboardx":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BillboardX;
							n++; s++; if (s >= m) m = s; break;
						case "billboardy":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BillboardY;
							n++; s++; if (s >= m) m = s; break;
						case "cameramode":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CameraView;
							n++; s++; if (s >= m) m = s; break;
							// train
						case "playertrain":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PlayerTrain;
							n++; s++; if (s >= m) m = s; break;
						case "cars":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainCars;
							n++; s++; if (s >= m) m = s; break;
						case "destination":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainDestination;
							n++; s++; if (s >= m) m = s; break;
						case "length":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainLength;
							n++; s++; if (s >= m) m = s; break;
						case "speed":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainSpeed;
							n++; s++; if (s >= m) m = s; break;
						case "speedindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainSpeedOfCar;
							n++; break;
						case "speedometer":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainSpeedometer;
							n++; s++; if (s >= m) m = s; break;
						case "speedometerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainSpeedometerOfCar;
							n++; break;
						case "acceleration":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainAcceleration;
							n++; s++; if (s >= m) m = s; break;
						case "accelerationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainAccelerationOfCar;
							n++; break;
						case "accelerationmotor":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainAccelerationMotor;
							n++; s++; if (s >= m) m = s; break;
						case "accelerationmotorindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainAccelerationMotorOfCar;
							n++; break;
						case "distance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainDistance;
							n++; s++; if (s >= m) m = s; break;
						case "playerdistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PlayerTrainDistance;
							n++; s++; if (s >= m) m = s; break;
						case "distanceindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainDistanceToCar;
							n++; break;
						case "playertrackdistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PlayerTrackDistance;
							n++; s++; if (s >= m) m = s; break;
						case "trackdistance":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainTrackDistance;
							n++; s++; if (s >= m) m = s; break;
						case "frontaxlecurveradius":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FrontAxleCurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "frontaxlecurveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FrontAxleCurveRadiusOfCar;
							n++; break;
						case "rearaxlecurveradius":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RearAxleCurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "rearaxlecurveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RearAxleCurveRadiusOfCar;
							n++; break;
						case "curveradius":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CurveRadius;
							n++; s++; if (s >= m) m = s; break;
						case "curveradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CurveRadiusOfCar;
							n++; break;
						case "curvecant":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CurveCant;
							n++; s++; if (s >= m) m = s; break;
						case "curvecantindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CurveCantOfCar;
							n++; break;
						case "pitch":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Pitch;
							n++; s++; if (s >= m) m = s; break;
						case "pitchindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PitchOfCar;
							n++; break;
						case "odometer":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Odometer;
							n++; s++; if (s >= m) m = s;  break;
						case "odometerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OdometerOfCar;
							n++; break;
						case "trackdistanceindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainTrackDistanceToCar;
							n++; break;
							// train: doors
						case "doors":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Doors;
							n++; s++; if (s >= m) m = s; break;
						case "doorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.DoorsIndex;
							n++; break;
						case "leftdoors":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LeftDoors;
							n++; s++; if (s >= m) m = s; break;
						case "leftdoorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LeftDoorsIndex;
							n++; break;
						case "rightdoors":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RightDoors;
							n++; s++; if (s >= m) m = s; break;
						case "rightdoorsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RightDoorsIndex;
							n++; break;
						case "leftdoorstarget":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LeftDoorsTarget;
							n++; s++; if (s >= m) m = s; break;
						case "leftdoorstargetindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LeftDoorsTargetIndex;
							n++; break;
						case "rightdoorstarget":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RightDoorsTarget;
							n++; s++; if (s >= m) m = s; break;
						case "rightdoorstargetindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RightDoorsTargetIndex;
							n++; break;
						case "doorbuttonl":
						case "leftdoorbutton":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LeftDoorButton;
							n++; s++; if (s >= m) m = s; break;
						case "doorbuttonr":
						case "rightdoorbutton":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RightDoorButton;
							n++; s++; if (s >= m) m = s; break;
						case "pilotlamp":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PilotLamp;
							n++; s++; if (s >= m) m = s; break;
						case "passalarm":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PassAlarm;
							n++; s++; if (s >= m) m = s; break;
						case "stationadjustalarm":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.StationAdjustAlarm;
							n++; s++; if (s >= m) m = s; break;
						case "headlights":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Headlights;
							n++; s++; if (s >= m) m = s; break;
						// train: handles
						case "reversernotch":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ReverserNotch;
							n++; s++; if (s >= m) m = s; break;
						case "powernotch":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PowerNotch;
							n++; s++; if (s >= m) m = s; break;
						case "powernotches":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PowerNotches;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotch":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeNotch;
							n++; s++; if (s >= m) m = s; break;
						case "locobrakenotch":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.LocoBrakeNotch;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotches":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeNotches;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotchlinear":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeNotchLinear;
							n++; s++; if (s >= m) m = s; break;
						case "brakenotcheslinear":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeNotchesLinear;
							n++; s++; if (s >= m) m = s; break;
						case "emergencybrake":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EmergencyBrake;
							n++; s++; if (s >= m) m = s; break;
						case "horn":
						case "klaxon":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Klaxon;
							n++; s++; if (s >= m) m = s; break;
						case "primaryhorn":
						case "primaryklaxon":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PrimaryKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "secondaryhorn":
						case "secondaryklaxon":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SecondaryKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "musichorn":
						case "musicklaxon":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.MusicKlaxon;
							n++; s++; if (s >= m) m = s; break;
						case "hasairbrake":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.HasAirBrake;
							n++; s++; if (s >= m) m = s; break;
						case "holdbrake":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.HoldBrake;
							n++; s++; if (s >= m) m = s; break;
						case "hasholdbrake":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.HasHoldBrake;
							n++; s++; if (s >= m) m = s; break;
						case "constspeed":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ConstSpeed;
							n++; s++; if (s >= m) m = s; break;
						case "hasconstspeed":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.HasConstSpeed;
							n++; s++; if (s >= m) m = s; break;
							// train: brake
						case "mainreservoir":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeMainReservoir;
							n++; s++; if (s >= m) m = s; break;
						case "mainreservoirindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeMainReservoirOfCar;
							n++; break;
						case "equalizingreservoir":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeEqualizingReservoir;
							n++; s++; if (s >= m) m = s; break;
						case "equalizingreservoirindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeEqualizingReservoirOfCar;
							n++; break;
						case "brakepipe":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeBrakePipe;
							n++; s++; if (s >= m) m = s; break;
						case "brakepipeindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeBrakePipeOfCar;
							n++; break;
						case "brakecylinder":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeBrakeCylinder;
							n++; s++; if (s >= m) m = s; break;
						case "brakecylinderindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeBrakeCylinderOfCar;
							n++; break;
						case "straightairpipe":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeStraightAirPipe;
							n++; s++; if (s >= m) m = s; break;
						case "straightairpipeindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrakeStraightAirPipeOfCar;
							n++; break;
							// train: safety
						case "hasplugin":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SafetyPluginAvailable;
							n++; s++; if (s >= m) m = s; break;
						case "pluginstate":
						case "ats":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SafetyPluginState;
							n++; break;
							// train: timetable
						case "timetable":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TimetableVisible;
							n++; s++; if (s >= m) m = s; break;
						case "panel2timetable":
							//This is an internal function, and does not form part of the documented API
							//Used for the [Timetable] section in panel2 trains
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Panel2Timetable;
							n++; s++; if (s >= m) m = s; break;
						case "distancenextstation":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.DistanceNextStation;
							n++; s++; if (s >= m) m = s; break;
						case "distancelaststation":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.DistanceLastStation;
							n++; s++; if (s >= m) m = s; break;
						case "stopsnextstation":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.StopsNextStation;
							n++; s++; if (s >= m) m = s; break;
						case "distancestationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.DistanceStation;
							n++; break;
						case "stopsstationindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.StopsStation;
							n++; break;
						case "terminalstation":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TerminalStation;
							n++; s++; if (s >= m) m = s; break;
						case "nextstation":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.NextStation;
							n++; s++; if (s >= m) m = s; break;
						case "nextstationstop":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.NextStationStop;
							n++; s++; if (s >= m) m = s; break;
						case "routelimit":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RouteLimit;
							n++; s++; if (s >= m) m = s; break;
							// sections
						case "section":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SectionAspectNumber;
							n++; s++; if (s >= m) m = s; break;
						case "sectionlimit":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SectionLimit;
							n++; s++; if (s >= m) m = s; break;
						// state
						case "currentstate":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CurrentObjectState;
							n++; s++; if (s >= m) m = s; break;
							// windscreen and raindrops
						case "raindrop":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.RainDrop;
							n++; break;
						case "snowflake":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.SnowFlake;
							n++; break;
						case "wiperposition":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WiperPosition;
							n++; s++; if (s >= m) m = s; break;
						case "wiperstate":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WiperState;
							n++; s++; if (s >= m) m = s; break;
						case "brightnessindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.BrightnessOfCar;
							n++; break;
						case "carnumber":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.TrainCarNumber;
							n++; s++; if (s >= m) m = s; break;
						case "wheelradius":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WheelRadius;
							n++; s++; if (s >= m) m = s; break;
						case "wheelradiusindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WheelRadiusOfCar;
							n++; break;
						case "wheelslip":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WheelSlip;
							n++; s++; if (s >= m) m = s; break;
						case "wheelslipindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.WheelSlipCar;
							n++; break;
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
						case "dsd":
						case "driversupervisiondevice":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.DSD;
							n++; s++; if (s >= m) m = s; break;
						case "ambienttemperature":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.AmbientTemperature;
							n++; s++; if (s >= m) m = s; break;
						case "frontcoupler":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FrontCoupler;
							n++; s++; if (s >= m) m = s; break;
						case "frontcouplerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FrontCouplerIndex;
							n++; break;
						case "enginerunning":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EngineRunning;
							n++; s++; if (s >= m) m = s; break;
						case "enginerunningindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EngineRunningCar;
							n++; break;
						case "enginerpm":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EngineRPM;
							n++; s++; if (s >= m) m = s; break;
						case "enginerpmindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EngineRPMCar;
							n++; break;
						case "enginepowerindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.EnginePowerCar;
							n++; break;
						case "fuellevel":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FuelLevel;
							n++; s++; if (s >= m) m = s; break;
						case "fuellevelindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FuelLevelCar;
							n++; break;
						case "amps":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.Amps;
							n++; s++; if (s >= m) m = s; break;
						case "ampsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.AmpsCar;
							n++; break;
						case "pantographstate":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PantographState;
							n++; s++; if (s >= m) m = s; break;
						case "pantographstateindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.PantographStateOfCar;
							n++; break;
						//New power supply bits from 1.9.0
						case "overheadvolts":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadVolts;
							n++; s++; if (s >= m) m = s; break;
						case "overheadvoltsindex":
							if (s < 1) throw new System.InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadVoltsTarget;
							n++; break;
						case "thirdrailvolts":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailVolts;
							n++; s++; if (s >= m) m = s; break;
						case "thirdrailvoltsindex":
							if (s < 1) throw new System.InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailVoltsTarget;
							n++; break;
						case "fourthrailvolts":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailVolts;
							n++; s++; if (s >= m) m = s; break;
						case "fourthrailvoltsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailVoltsTarget;
							n++; break;
						case "overheadheight":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadHeight;
							n++; s++; if (s >= m) m = s; break;
						case "overheadheightindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadHeightTarget;
							n++; break;
						case "thirdrailheight":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailHeight;
							n++; s++; if (s >= m) m = s; break;
						case "thirdrailheightindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailHeightTarget;
							n++; break;
						case "fourthrailheight":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailHeight;
							n++; s++; if (s >= m) m = s; break;
						case "fourthrailheightindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailHeightTarget;
							n++; break;
						case "overheadac":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadAC;
							n++; s++; if (s >= m) m = s; break;
						case "thirdrailac":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailAC;
							n++; s++; if (s >= m) m = s; break;
						case "fourthrailac":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailAC;
							n++; s++; if (s >= m) m = s; break;
						case "overheadamps":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadAmps;
							n++; s++; if (s >= m) m = s; break;
						case "overheadampsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.OverheadAmpsTarget;
							n++; break;
						case "thirdrailamps":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailAmps;
							n++; s++; if (s >= m) m = s; break;
						case "thirdrailampsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.ThirdRailAmpsTarget;
							n++; break;
						case "fourthrailamps":
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailAmps;
							n++; s++; if (s >= m) m = s; break;
						case "fourthrailampsindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.FourthRailAmpsTarget;
							n++; break;
						case "cylindercocksstateindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CylinderCocksStateOfCar;
							n++; break;
						case "blowerssstateindex":
							if (s < 1) throw new InvalidOperationException(Arguments[i] + " requires at least 1 argument on the stack in function script " + Expression);
							if (n >= InstructionSet.Length) Array.Resize(ref InstructionSet, InstructionSet.Length << 1);
							InstructionSet[n] = Instructions.CylinderCocksStateOfCar;
							n++; break;
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
