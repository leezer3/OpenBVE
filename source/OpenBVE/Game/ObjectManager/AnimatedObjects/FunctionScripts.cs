using LibRender2.Overlays;
using OpenBveApi;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using System;
using TrainManager.BrakeSystems;
using TrainManager.Car.Systems;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace OpenBve {
	internal static class FunctionScripts {
		// execute function script
		internal static void ExecuteFunctionScript(FunctionScript Function, TrainBase Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState) {
			int s = 0, c = 0;
			for (int i = 0; i < Function.InstructionSet.Length; i++) {
				switch (Function.InstructionSet[i]) {
						// system
					case Instructions.SystemHalt:
						i = Function.InstructionSet.Length;
						break;
					case Instructions.SystemConstant:
						Function.Stack[s] = Function.Constants[c];
						s++; c++; break;
					case Instructions.SystemConstantArray:
						{
							int n = (int)Function.InstructionSet[i + 1];
							for (int j = 0; j < n; j++) {
								Function.Stack[s + j] = Function.Constants[c + j];
							} s += n; c += n; i++;
						} break;
					case Instructions.SystemValue:
						Function.Stack[s] = Function.LastResult;
						s++; break;
					case Instructions.SystemDelta:
						Function.Stack[s] = TimeElapsed;
						s++; break;
						// stack
					case Instructions.StackCopy:
						Function.Stack[s] = Function.Stack[s - 1];
						s++; break;
					case Instructions.StackSwap:
						(Function.Stack[s - 1], Function.Stack[s - 2]) = (Function.Stack[s - 2], Function.Stack[s - 1]);
						break;
						// math
					case Instructions.MathPlus:
						Function.Stack[s - 2] += Function.Stack[s - 1];
						s--; break;
					case Instructions.MathSubtract:
						Function.Stack[s - 2] -= Function.Stack[s - 1];
						s--; break;
					case Instructions.MathMinus:
						Function.Stack[s - 1] = -Function.Stack[s - 1];
						break;
					case Instructions.MathTimes:
						Function.Stack[s - 2] *= Function.Stack[s - 1];
						s--; break;
					case Instructions.MathDivide:
						Function.Stack[s - 2] = Function.Stack[s - 1] == 0.0 ? 0.0 : Function.Stack[s - 2] / Function.Stack[s - 1];
						s--; break;
					case Instructions.MathReciprocal:
						Function.Stack[s - 1] = Function.Stack[s - 1] == 0.0 ? 0.0 : 1.0 / Function.Stack[s - 1];
						break;
					case Instructions.MathPower:
						{
							double a = Function.Stack[s - 2];
							double b = Function.Stack[s - 1];
							if (b == 2.0) {
								Function.Stack[s - 2] = a * a;
							} else if (b == 3.0) {
								Function.Stack[s - 2] = a * a * a;
							} else if (b == 4.0) {
								double t = a * a;
								Function.Stack[s - 2] = t * t;
							} else if (b == 5.0) {
								double t = a * a;
								Function.Stack[s - 2] = t * t * a;
							} else if (b == 6.0) {
								double t = a * a * a;
								Function.Stack[s - 2] = t * t;
							} else if (b == 7.0) {
								double t = a * a * a;
								Function.Stack[s - 2] = t * t * a;
							} else if (b == 8.0) {
								double t = a * a; t *= t;
								Function.Stack[s - 2] = t * t;
							} else if (b == 0.0) {
								Function.Stack[s - 2] = 1.0;
							} else if (b < 0.0) {
								Function.Stack[s - 2] = 0.0;
							} else {
								Function.Stack[s - 2] = Math.Pow(a, b);
							}
							s--; break;
						}
					case Instructions.MathRandom:
						{
							//Generates a random number between two given doubles
							double min = Function.Stack[s - 2];
							double max = Function.Stack[s - 1];
							var randomGenerator = new Random();
							Function.Stack[s - 2] =  min + randomGenerator.NextDouble() * (max - min);
							s--;
						}
						break;
					case Instructions.MathRandomInt:
						{
							//Generates a random number between two given doubles
							int min = (int)Function.Stack[s - 2];
							int max = (int)Function.Stack[s - 1];
							var randomGenerator = new Random();
							Function.Stack[s - 2] = randomGenerator.Next(min,max);
							s--;
						}
						break;
					case Instructions.MathIncrement:
						Function.Stack[s - 1] += 1.0;
						break;
					case Instructions.MathDecrement:
						Function.Stack[s - 1] -= 1.0;
						break;
					case Instructions.MathFusedMultiplyAdd:
						Function.Stack[s - 3] = Function.Stack[s - 3] * Function.Stack[s - 2] + Function.Stack[s - 1];
						s -= 2; break;
					case Instructions.MathQuotient:
						Function.Stack[s - 2] = Function.Stack[s - 1] == 0.0 ? 0.0 : Math.Floor(Function.Stack[s - 2] / Function.Stack[s - 1]);
						s--; break;
					case Instructions.MathMod:
						Function.Stack[s - 2] = Function.Stack[s - 1] == 0.0 ? 0.0 : Function.Stack[s - 2] - Function.Stack[s - 1] * Math.Floor(Function.Stack[s - 2] / Function.Stack[s - 1]);
						s--; break;
					case Instructions.MathFloor:
						Function.Stack[s - 1] = Math.Floor(Function.Stack[s - 1]);
						break;
					case Instructions.MathCeiling:
						Function.Stack[s - 1] = Math.Ceiling(Function.Stack[s - 1]);
						break;
					case Instructions.MathRound:
						Function.Stack[s - 1] = Math.Round(Function.Stack[s - 1]);
						break;
					case Instructions.MathMin:
						Function.Stack[s - 2] = Function.Stack[s - 2] < Function.Stack[s - 1] ? Function.Stack[s - 2] : Function.Stack[s - 1];
						s--; break;
					case Instructions.MathMax:
						Function.Stack[s - 2] = Function.Stack[s - 2] > Function.Stack[s - 1] ? Function.Stack[s - 2] : Function.Stack[s - 1];
						s--; break;
					case Instructions.MathAbs:
						Function.Stack[s - 1] = Math.Abs(Function.Stack[s - 1]);
						break;
					case Instructions.MathSign:
						Function.Stack[s - 1] = Math.Sign(Function.Stack[s - 1]);
						break;
					case Instructions.MathExp:
						Function.Stack[s - 1] = Math.Exp(Function.Stack[s - 1]);
						break;
					case Instructions.MathLog:
						Function.Stack[s - 1] = Extensions.LogC(Function.Stack[s - 1]);
						break;
					case Instructions.MathSqrt:
						Function.Stack[s - 1] = Extensions.SqrtC(Function.Stack[s - 1]);
						break;
					case Instructions.MathSin:
						Function.Stack[s - 1] = Math.Sin(Function.Stack[s - 1]);
						break;
					case Instructions.MathCos:
						Function.Stack[s - 1] = Math.Cos(Function.Stack[s - 1]);
						break;
					case Instructions.MathTan:
						Function.Stack[s - 1] = Extensions.TanC(Function.Stack[s - 1]);
						break;
					case Instructions.MathArcTan:
						Function.Stack[s - 1] = Math.Atan(Function.Stack[s - 1]);
						break;
					case Instructions.MathPi:
						Function.Stack[s] = Math.PI;
						s++; break;
						// comparisons
					case Instructions.CompareEqual:
						Function.Stack[s - 2] = Function.Stack[s - 2] == Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareUnequal:
						Function.Stack[s - 2] = Function.Stack[s - 2] != Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareLess:
						Function.Stack[s - 2] = Function.Stack[s - 2] < Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareGreater:
						Function.Stack[s - 2] = Function.Stack[s - 2] > Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareLessEqual:
						Function.Stack[s - 2] = Function.Stack[s - 2] <= Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareGreaterEqual:
						Function.Stack[s - 2] = Function.Stack[s - 2] >= Function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareConditional:
						Function.Stack[s - 3] = Function.Stack[s - 3] != 0.0 ? Function.Stack[s - 2] : Function.Stack[s - 1];
						s -= 2; break;
						// logical
					case Instructions.LogicalNot:
						Function.Stack[s - 1] = Function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						break;
					case Instructions.LogicalAnd:
						Function.Stack[s - 2] = Function.Stack[s - 2] != 0.0 & Function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.LogicalOr:
						Function.Stack[s - 2] = Function.Stack[s - 2] != 0.0 | Function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.LogicalNand:
						Function.Stack[s - 2] = Function.Stack[s - 2] != 0.0 & Function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						s--; break;
					case Instructions.LogicalNor:
						Function.Stack[s - 2] = Function.Stack[s - 2] != 0.0 | Function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						s--; break;
					case Instructions.LogicalXor:
						Function.Stack[s - 2] = Function.Stack[s - 2] != 0.0 ^ Function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.CurrentObjectState:
						Function.Stack[s] = CurrentState;
						s++;
						break;
						// time/camera
					case Instructions.TimeSecondsSinceMidnight:
						Function.Stack[s] = Program.CurrentRoute.SecondsSinceMidnight;
						s++; break;
					case Instructions.TimeHourDigit:
						Function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight / 3600.0);
						s++; break;
					case Instructions.TimeMinuteDigit:
						Function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight / 60 % 60);
						s++; break;
					case Instructions.TimeSecondDigit:
						Function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight % 60);
						s++; break;
					case Instructions.CameraDistance:
						{
							double dx = Program.Renderer.Camera.AbsolutePosition.X - Position.X;
							double dy = Program.Renderer.Camera.AbsolutePosition.Y - Position.Y;
							double dz = Program.Renderer.Camera.AbsolutePosition.Z - Position.Z;
							Function.Stack[s] = Math.Sqrt(dx * dx + dy * dy + dz * dz);
							s++;
						} break;
					case Instructions.CameraXDistance:
						{
							Function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.X - Position.X;
							s++;
						} break;
					case Instructions.CameraYDistance:
						{
							Function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.Y - Position.Y;
							s++;
						} break;
					case Instructions.CameraZDistance:
						{
							Function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.Z - Position.Z;
							s++;
						} break;
					case Instructions.BillboardX:
						{
							Vector3 toCamera = Program.Renderer.Camera.AbsolutePosition - Position;
							Function.Stack[s] = Math.Atan2(toCamera.Y, -toCamera.Z);
							s++;
						} break;
					case Instructions.BillboardY:
						{
							Vector3 toCamera = Program.Renderer.Camera.AbsolutePosition - Position;
							Function.Stack[s] = Math.Atan2(-toCamera.Z, toCamera.X);
							s++;
						} break;
					case Instructions.CameraView:
						//Returns whether the camera is in interior or exterior mode
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior || Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
						{
							Function.Stack[s] = 0;
						}
						else
						{
							Function.Stack[s] = 1;
						}
						s++; break;
						// train
					case Instructions.PlayerTrain:
						if (IsPartOfTrain && Train != null)
						{
							Function.Stack[s] = Train.IsPlayerTrain ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainCars:
						if (Train != null) {
							Function.Stack[s] = Train.Cars.Length;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainDestination:
						if (Train != null) {
							Function.Stack[s] = Train.Destination;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainLength:
						if (Train != null) {
							Function.Stack[s] = Train.Length;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeed:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CurrentSpeed;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].CurrentSpeed;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainSpeedometer:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.PerceivedSpeed;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedometerOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.PerceivedSpeed;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainAcceleration:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.Acceleration;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainAccelerationOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.Acceleration;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainAccelerationMotor:
						if (Train != null) {
							Function.Stack[s] = 0.0;
							for (int j = 0; j < Train.Cars.Length; j++) {
								if (Train.Cars[j].TractionModel.ProvidesPower) {
									// hack: MotorAcceleration does not distinguish between forward/backward
									if (Train.Cars[j].TractionModel.CurrentAcceleration < 0.0) {
										Function.Stack[s] = Train.Cars[j].TractionModel.CurrentAcceleration * Math.Sign(Train.Cars[j].CurrentSpeed);
									} else if (Train.Cars[j].TractionModel.CurrentAcceleration > 0.0) {
										Function.Stack[s] = Train.Cars[j].TractionModel.CurrentAcceleration * (double)Train.Handles.Reverser.Actual;
									} else {
										Function.Stack[s] = 0.0;
									}
									break;
								}
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainAccelerationMotorOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								// hack: MotorAcceleration does not distinguish between forward/backward
								if (Train.Cars[j].TractionModel.CurrentAcceleration < 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].TractionModel.CurrentAcceleration * Math.Sign(Train.Cars[j].CurrentSpeed);
								} else if (Train.Cars[j].TractionModel.CurrentAcceleration > 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].TractionModel.CurrentAcceleration * (double)Train.Handles.Reverser.Actual;
								} else {
									Function.Stack[s - 1] = 0.0;
								}
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PlayerTrainDistance:
						double playerDist = double.MaxValue;
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							double fx = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.X - Position.X;
							double fy = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.Y - Position.Y;
							double fz = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.Z - Position.Z;
							double f = fx * fx + fy * fy + fz * fz;
							if (f < playerDist) playerDist = f;
							double rx = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.X - Position.X;
							double ry = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.Y - Position.Y;
							double rz = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.Z - Position.Z;
							double r = rx * rx + ry * ry + rz * rz;
							if (r < playerDist) playerDist = r;
						}
						Function.Stack[s] = Math.Sqrt(playerDist);
						s++; break;
					case Instructions.TrainDistance:
						if (Train != null) {
							double dist = double.MaxValue;
							for (int j = 0; j < Train.Cars.Length; j++) {
								double fx = Train.Cars[j].FrontAxle.Follower.WorldPosition.X - Position.X;
								double fy = Train.Cars[j].FrontAxle.Follower.WorldPosition.Y - Position.Y;
								double fz = Train.Cars[j].FrontAxle.Follower.WorldPosition.Z - Position.Z;
								double f = fx * fx + fy * fy + fz * fz;
								if (f < dist) dist = f;
								double rx = Train.Cars[j].RearAxle.Follower.WorldPosition.X - Position.X;
								double ry = Train.Cars[j].RearAxle.Follower.WorldPosition.Y - Position.Y;
								double rz = Train.Cars[j].RearAxle.Follower.WorldPosition.Z - Position.Z;
								double r = rx * rx + ry * ry + rz * rz;
								if (r < dist) dist = r;
							}
							Function.Stack[s] = Math.Sqrt(dist);
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainDistanceToCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								double x = 0.5 * (Train.Cars[j].FrontAxle.Follower.WorldPosition.X + Train.Cars[j].RearAxle.Follower.WorldPosition.X) - Position.X;
								double y = 0.5 * (Train.Cars[j].FrontAxle.Follower.WorldPosition.Y + Train.Cars[j].RearAxle.Follower.WorldPosition.Y) - Position.Y;
								double z = 0.5 * (Train.Cars[j].FrontAxle.Follower.WorldPosition.Z + Train.Cars[j].RearAxle.Follower.WorldPosition.Z) - Position.Z;
								Function.Stack[s - 1] = Math.Sqrt(x * x + y * y + z * z);
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PlayerTrackDistance:
						double pt0 = TrainManager.PlayerTrain.FrontCarTrackPosition;
						double pt1 = TrainManager.PlayerTrain.RearCarTrackPosition;
						Function.Stack[s] = TrackPosition > pt0 ? TrackPosition - pt0 : TrackPosition < pt1 ? TrackPosition - pt1 : 0.0;
						s++; break;
					case Instructions.TrainTrackDistance:
						if (Train != null) {
							double t0 = Train.FrontCarTrackPosition;
							double t1 = Train.RearCarTrackPosition;
							Function.Stack[s] = TrackPosition > t0 ? TrackPosition - t0 : TrackPosition < t1 ? TrackPosition - t1 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.CurveRadius:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s - 1] = (Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius + Train.Cars[CarIndex].RearAxle.Follower.CurveRadius) / 2;
						}
						break;
					case Instructions.CurveRadiusOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = (Train.Cars[j].FrontAxle.Follower.CurveRadius + Train.Cars[j].RearAxle.Follower.CurveRadius) / 2;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.FrontAxleCurveRadius:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s] = Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius;
						}
						break;
					case Instructions.FrontAxleCurveRadiusOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].FrontAxle.Follower.CurveRadius;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.RearAxleCurveRadius:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s] = Train.Cars[CarIndex].RearAxle.Follower.CurveRadius;
						}
						break;
					case Instructions.RearAxleCurveRadiusOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].RearAxle.Follower.CurveRadius;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.CurveCant:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s] = Train.Cars[CarIndex].FrontAxle.Follower.CurveCant;
						}
						break;
					case Instructions.CurveCantOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].FrontAxle.Follower.CurveCant;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.Pitch:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s - 1] = Train.Cars[CarIndex].FrontAxle.Follower.Pitch;
						}
						break;
					case Instructions.PitchOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].FrontAxle.Follower.Pitch;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.Odometer:
						if (Train == null)
						{
							Function.Stack[s] = 0.0;
						}
						else
						{
							Function.Stack[s] = Train.Cars[CarIndex].FrontAxle.Follower.Odometer;
						}
						s++;
						break;
					case Instructions.OdometerOfCar:
						if (Train == null)
						{
							Function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].FrontAxle.Follower.Odometer;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.TrainTrackDistanceToCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								double p = 0.5 * (Train.Cars[j].FrontAxle.Follower.TrackPosition + Train.Cars[j].RearAxle.Follower.TrackPosition);
								Function.Stack[s - 1] = TrackPosition - p;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
						// door
					case Instructions.Doors:
						if (Train != null) {
							double a = 0.0;
							for (int j = 0; j < Train.Cars.Length; j++) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s] = a;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.DoorsIndex:
						if (Train != null) {
							double a = 0.0;
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s - 1] = a;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.LeftDoors:
						if (Train != null) {
							double a = 0.0;
							for (int j = 0; j < Train.Cars.Length; j++) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].Direction == -1 & Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s] = a;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LeftDoorsIndex:
						if (Train != null) {
							double a = 0.0;
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].Direction == -1 & Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s - 1] = a;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RightDoors:
						if (Train != null) {
							double a = 0.0;
							for (int j = 0; j < Train.Cars.Length; j++) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].Direction == 1 & Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s] = a;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RightDoorsIndex:
						if (Train != null) {
							double a = 0.0;
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[k].Direction == 1 & Train.Cars[j].Doors[k].State > a) {
										a = Train.Cars[j].Doors[k].State;
									}
								}
							}
							Function.Stack[s - 1] = a;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.LeftDoorsTarget:
						if (Train != null) {
							bool q = false;
							for (int j = 0; j < Train.Cars.Length; j++) {
								if (Train.Cars[j].Doors[0].AnticipatedOpen) {
									q = true;
									break;
								}
							}
							Function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LeftDoorsTargetIndex:
						if (Train != null) {
							bool q = false;
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[0].AnticipatedOpen) {
										q = true;
										break;
									}
								}
							}
							Function.Stack[s - 1] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RightDoorsTarget:
						if (Train != null) {
							bool q = false;
							for (int j = 0; j < Train.Cars.Length; j++) {
								if (Train.Cars[j].Doors[1].AnticipatedOpen) {
									q = true;
									break;
								}
							}
							Function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RightDoorsTargetIndex:
						if (Train != null) {
							bool q = false;
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++) {
									if (Train.Cars[j].Doors[1].AnticipatedOpen) {
										q = true;
										break;
									}
								}
							}
							Function.Stack[s - 1] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.LeftDoorButton:
						if (Train != null)
						{
							Function.Stack[s] = Train.Cars[Train.DriverCar].Doors[0].ButtonPressed ? 1.0 : 0.0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RightDoorButton:
						if (Train != null)
						{
							Function.Stack[s] = Train.Cars[Train.DriverCar].Doors[1].ButtonPressed ? 1.0 : 0.0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PilotLamp:
						if (Train != null) {
							if (Train.SafetySystems.PilotLamp.Lit)
							{
								Function.Stack[s] = 1.0;
							}
							else
							{
								Function.Stack[s] = 0.0;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PassAlarm:
						if (Train != null) {
							if (Train.SafetySystems.PassAlarm.Lit)
							{
								Function.Stack[s] = 1.0;
							}
							else
							{
								Function.Stack[s] = 0.0;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.StationAdjustAlarm:
						if (Train != null) {
							if (Train.SafetySystems.StationAdjust.Lit)
							{
								Function.Stack[s] = 1.0;
							}
							else
							{
								Function.Stack[s] = 0.0;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.Headlights:
						if (Train != null)
						{
							Function.Stack[s] = Train.SafetySystems.Headlights.CurrentState;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
						// handles
					case Instructions.ReverserNotch:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Handles.Reverser.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotch:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.Power.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotches:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.Power.MaximumNotch;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LocoBrakeNotch:
						if (Train != null  && Train.Handles.LocoBrake != null) {
							Function.Stack[s] = Train.Handles.LocoBrake.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LocoBrakeNotches:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.LocoBrake.MaximumNotch;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotch:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.Brake.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotches:
						if (Train != null) {
							if (Train.Handles.Brake is AirBrakeHandle) {
								Function.Stack[s] = 2.0;
							} else {
								Function.Stack[s] = Train.Handles.Brake.MaximumNotch;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchLinear:
						if (Train != null) {
							if (Train.Handles.Brake is AirBrakeHandle) {
								if (Train.Handles.EmergencyBrake.Driver) {
									Function.Stack[s] = 3.0;
								} else {
									Function.Stack[s] = Train.Handles.Brake.Driver;
								}
							} else if (Train.Handles.HasHoldBrake) {
								if (Train.Handles.EmergencyBrake.Driver) {
									Function.Stack[s] = Train.Handles.Brake.MaximumNotch + 2.0;
								} else if (Train.Handles.Brake.Driver > 0) {
									Function.Stack[s] = Train.Handles.Brake.Driver + 1.0;
								} else {
									Function.Stack[s] = Train.Handles.HoldBrake.Driver ? 1.0 : 0.0;
								}
							} else {
								if (Train.Handles.EmergencyBrake.Driver) {
									Function.Stack[s] = Train.Handles.Brake.MaximumNotch + 1.0;
								} else {
									Function.Stack[s] = Train.Handles.Brake.Driver;
								}
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchesLinear:
						if (Train != null) {
							if (Train.Handles.Brake is AirBrakeHandle) {
								Function.Stack[s] = 3.0;
							} else if (Train.Handles.HasHoldBrake) {
								Function.Stack[s] = Train.Handles.Brake.MaximumNotch + 2.0;
							} else {
								Function.Stack[s] = Train.Handles.Brake.MaximumNotch + 1.0;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.EmergencyBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.EmergencyBrake.Driver ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.Klaxon:
						if (Train != null && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns != null)
						{
							for (int j = 0; j < TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns.Length; j++)
							{
								/* Return the index of the currently playing horn sound
								 *
								 * 1 ==> Primary horn
								 * 2 ==> Secondary horn
								 * 3 ==> Music horn
								 */
								if (Program.Sounds.IsPlaying(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns[j].Source))
								{
									Function.Stack[s] = j + 1;
									break;
								}
								if (j == TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns.Length -1)
								{
									Function.Stack[s] = 0.0;
								}
							}
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PrimaryKlaxon:
						if (Train != null)
						{
							Function.Stack[s] = Program.Sounds.IsPlaying(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns[0].Source) ? 1.0 : 0.0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.SecondaryKlaxon:
						if (Train != null)
						{
							Function.Stack[s] = Program.Sounds.IsPlaying(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns[1].Source) ? 1.0 : 0.0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.MusicKlaxon:
						if (Train != null)
						{
							Function.Stack[s] = Program.Sounds.IsPlaying(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Horns[2].Source) ? 1.0 : 0.0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasAirBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.Brake is AirBrakeHandle ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HoldBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.HoldBrake.Driver ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasHoldBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Handles.HasHoldBrake ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.ConstSpeed:
						if (Train != null) {
							Function.Stack[s] = Train.Specs.CurrentConstSpeed ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasConstSpeed:
						if (Train != null) {
							Function.Stack[s] = Train.Specs.HasConstSpeed ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
						// brake
					case Instructions.BrakeMainReservoir:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.MainReservoir.CurrentPressure;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeMainReservoirOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.MainReservoir.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeEqualizingReservoir:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.EqualizingReservoir.CurrentPressure;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeEqualizingReservoirOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.EqualizingReservoir.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakePipe:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.BrakePipe.CurrentPressure;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeBrakePipeOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.BrakePipe.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakeCylinder:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.BrakeCylinder.CurrentPressure;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeBrakeCylinderOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.BrakeCylinder.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeStraightAirPipe:
						if (Train != null && Train.Cars[CarIndex].CarBrake is AirBrake airBrake) {
							Function.Stack[s] = airBrake.StraightAirPipe.CurrentPressure;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeStraightAirPipeOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length && Train.Cars[j].CarBrake is AirBrake carAirBrake) {
								Function.Stack[s - 1] = carAirBrake.StraightAirPipe.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
						// safety
					case Instructions.SafetyPluginAvailable:
						if (Train != null && Train.IsPlayerTrain && Train.Plugin != null) {
							Function.Stack[s] = TrainManager.PlayerTrain.Plugin.IsDefault ? 0.0 : 1.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.SafetyPluginState:
						if (Train == null || Train.Plugin == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int n = (int)Math.Round(Function.Stack[s - 1]);
							if (n >= 0 & n < Train.Plugin.Panel.Length) {
								Function.Stack[s - 1] = Train.Plugin.Panel[n];
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} break;
						// timetable
					case Instructions.TimetableVisible:
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Custom:
							case DisplayedTimetable.Default:
								Function.Stack[s] = 1.0;
								break;
							case DisplayedTimetable.None:
								Function.Stack[s] = 0.0;
								break;
						}
						s++; break;
					case Instructions.Panel2Timetable:
						//Internal instruction used to show / hide custom timetable overlay on Panel2 trains
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Custom:
								Function.Stack[s] = 0.0;
								break;
							default:
								Function.Stack[s] = -1.0;
								break;
						}
						s++; break;
					case Instructions.DistanceNextStation:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so distance is irrelevant
						}
						else
						{
							int stationIdx;
							if (Train.Station >= 0 && Train.StationState != TrainStopState.Completed)
							{
								stationIdx = Train.LastStation;
							}
							else
							{
								stationIdx = Train.LastStation + 1;
							}
							if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
							{
								stationIdx = Train.LastStation;
							}
							Function.Stack[s] = Program.CurrentRoute.Stations[stationIdx].GetStopPosition(Train) - Train.FrontCarTrackPosition;
						}
						s++; break;
					case Instructions.DistanceLastStation:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so distance is irrelevant
						}
						else
						{
							int stationIdx;
							if (Train.Station >= 0 && Train.StationState != TrainStopState.Completed)
							{
								stationIdx = Train.LastStation;
							}
							else
							{
								stationIdx = Train.LastStation + 1;
							}

							stationIdx -= 1;

							if (stationIdx < 0)
							{
								stationIdx = 0;
							}

							Function.Stack[s] = Program.CurrentRoute.Stations[stationIdx].GetStopPosition(Train) - Train.FrontCarTrackPosition;
						}
						s++; break;
					case Instructions.StopsNextStation:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so we obviously can't stop at a station....
						}
						else
						{
							int stationIdx;
							if (Train.Station >= 0 && Train.StationState != TrainStopState.Completed)
							{
								stationIdx = Train.LastStation;
							}
							else
							{
								stationIdx = Train.LastStation + 1;
							}
							if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
							{
								Function.Stack[s] = 0.0; //Passed the terminal station, hence cannot stop again
							}
							else
							{
								Function.Stack[s] = Program.CurrentRoute.Stations[stationIdx].StopsHere(Train) ? 1.0 : 0.0;
							}
						}
						s++; break;
					case Instructions.DistanceStation:
						if (Train != null)
						{
							int stationIdx = (int)Math.Round(Function.Stack[s - 1]); //Station index
							if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
							{
								Function.Stack[s - 1] = 0.0; //Invalid index
							}
							else
							{
								Function.Stack[s - 1] = Program.CurrentRoute.Stations[stationIdx].GetStopPosition(Train) - Train.FrontCarTrackPosition;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.StopsStation:
						if (Train != null)
						{
							int stationIdx = (int)Math.Round(Function.Stack[s - 1]); //Station index
							if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
							{
								Function.Stack[s - 1] = 0.0; //Invalid index
							}
							else
							{
								Function.Stack[s - 1] = Program.CurrentRoute.Stations[stationIdx].StopsHere(Train) ? 1.0 : 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.NextStation:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so distance is irrelevant
						}
						else
						{
							int stationIdx = Train.LastStation + 1;
							if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
							{
								stationIdx = Train.LastStation;
							}
							Function.Stack[s] = stationIdx;
						}
						s++; break;
					case Instructions.NextStationStop:
							if (Train == null)
							{
								Function.Stack[s] = 0.0; //Not part of a train, so distance is irrelevant
							}
							else
							{
								int stationIdx = Train.LastStation + 1;
								if (stationIdx > Program.CurrentRoute.Stations.Length - 1)
								{
									stationIdx = Train.LastStation;
								}

								while (stationIdx < Program.CurrentRoute.Stations.Length - 1)
								{
									if (Program.CurrentRoute.Stations[stationIdx].StopsHere(Train))
									{
										break;
									}
									stationIdx++;
								}
								Function.Stack[s] = stationIdx;
							}
							s++; break; 
					case Instructions.TerminalStation:
							int idx = Program.CurrentRoute.Stations.Length;
							for (int j = Program.CurrentRoute.Stations.Length - 1; j >= 0; j--)
							{
								if (Program.CurrentRoute.Stations[j].Type == StationType.Terminal)
								{
									idx = j;
									break;
								}
							}
							Function.Stack[s] = idx;
							s++; break;
					case Instructions.RouteLimit:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so irrelevant
						}
						else
						{
							if (Train.CurrentRouteLimit == double.PositiveInfinity)
							{
								Function.Stack[s] = -1;
							}
							else
							{
								Function.Stack[s] = Train.CurrentRouteLimit;
							}
						}
						s++; break;
					case Instructions.SectionLimit:
						if (Train == null)
						{
							Function.Stack[s] = 0.0; //Not part of a train, so irrelevant
						}
						else
						{
							if (Train.CurrentSectionLimit == double.PositiveInfinity)
							{
								Function.Stack[s] = -1;
							}
							else
							{
								Function.Stack[s] = Train.CurrentSectionLimit;
							}
						}
						s++; break;
					// sections
					case Instructions.SectionAspectNumber:
						if (IsPartOfTrain) {
							int nextSectionIndex = Train.CurrentSectionIndex + 1;
							if (nextSectionIndex >= 0 & nextSectionIndex < Program.CurrentRoute.Sections.Length) {
								int a = Program.CurrentRoute.Sections[nextSectionIndex].CurrentAspect;
								if (a >= 0 & a < Program.CurrentRoute.Sections[nextSectionIndex].Aspects.Length) {
									Function.Stack[s] = Program.CurrentRoute.Sections[nextSectionIndex].Aspects[a].Number;
								} else {
									Function.Stack[s] = 0;
								}
							}
						} else if (SectionIndex >= 0 & SectionIndex < Program.CurrentRoute.Sections.Length) {
							int a = Program.CurrentRoute.Sections[SectionIndex].CurrentAspect;
							if (a >= 0 & a < Program.CurrentRoute.Sections[SectionIndex].Aspects.Length) {
								Function.Stack[s] = Program.CurrentRoute.Sections[SectionIndex].Aspects[a].Number;
							} else {
								Function.Stack[s] = 0;
							}
						} else {
							Function.Stack[s] = 0;
						}
						s++; break;
					case Instructions.RainDrop:
						// n.b. windscreen may be null if we've changed driver car, or this is used in non XML train
						if (Train == null || Train.Cars[Train.DriverCar].Windscreen == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int n = (int)Math.Round(Function.Stack[s - 1]);
							if (n >= 0 & n < Train.Cars[Train.DriverCar].Windscreen.RainDrops.Length) {
								Function.Stack[s - 1] = Train.Cars[Train.DriverCar].Windscreen.RainDrops[n].Visible && !Train.Cars[Train.DriverCar].Windscreen.RainDrops[n].IsSnowFlake ? 1.0 : 0.0;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} break;
					case Instructions.SnowFlake:
							if (Train == null || !Train.IsPlayerTrain) {
								Function.Stack[s - 1] = 0.0;
							} else {
								int n = (int)Math.Round(Function.Stack[s - 1]);
								if (n >= 0 & n < Train.Cars[Train.DriverCar].Windscreen.RainDrops.Length) {
									Function.Stack[s - 1] = Train.Cars[Train.DriverCar].Windscreen.RainDrops[n].Visible && Train.Cars[Train.DriverCar].Windscreen.RainDrops[n].IsSnowFlake ? 1.0 : 0.0;
								} else {
									Function.Stack[s - 1] = 0.0;
								}
							} break;
					case Instructions.WiperPosition:
						if (Train == null || !Train.IsPlayerTrain)
						{
							Function.Stack[s] = 0.0; //Not part of player train, so irrelevant
						}
						else
						{
							Function.Stack[s] = Train.Cars[Train.DriverCar].Windscreen.Wipers.CurrentPosition;
						}
						s++; break;
					case Instructions.WiperState:
						if (Train == null || !Train.IsPlayerTrain)
						{
							Function.Stack[s] = 0.0; //Not part of player train, so irrelevant
						}
						else
						{
							Function.Stack[s] = (int)Train.Cars[Train.DriverCar].Windscreen.Wipers.CurrentSpeed;
						}
						s++; break;
					case Instructions.BrightnessOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Brightness.CurrentBrightness(Program.Renderer.Lighting.DynamicCabBrightness, 0.0);
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.TrainCarNumber:
						if (!IsPartOfTrain)
						{
							Function.Stack[s] = -1;
						}
						else
						{
							Function.Stack[s] = CarIndex;
						}
						s++;
						break;
					case Instructions.WheelSlip:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].FrontAxle.CurrentWheelSlip ? 1 : 0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.WheelSlipCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].FrontAxle.CurrentWheelSlip ? 1 : 0;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.Sanders:
						{
							if (Train != null && Train.Cars[CarIndex].ReAdhesionDevice is Sanders sanders) {
								Function.Stack[s] = sanders.State == SandersState.Active ? 1 : 0;
							} else {
								Function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.SandLevel:
						{
							if (Train != null && Train.Cars[CarIndex].ReAdhesionDevice is Sanders sanders) {
								Function.Stack[s] = sanders.SandLevel;
							} else {
								Function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.SandShots:
						{
							if (Train != null && Train.Cars[CarIndex].ReAdhesionDevice is Sanders sanders) {
								Function.Stack[s] = sanders.NumberOfShots;
							} else {
								Function.Stack[s] = 0.0;
							}
						} 
						s++; break;
					case Instructions.DSD:
						{
							if (Train != null && Train.Cars[Train.DriverCar].SafetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out DriverSupervisionDevice dsd))
							{
								Function.Stack[s] = dsd.CurrentState == SafetySystemState.Triggered ? 1 : 0;
							}
							else
							{
								Function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.AmbientTemperature:
						{
							if (Train != null)
							{
								Function.Stack[s] = Program.CurrentRoute.Atmosphere.GetAirTemperature(Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation);
							}
							else
							{
								Function.Stack[s] = Program.CurrentRoute.Atmosphere.GetAirTemperature(Position.Y + Program.CurrentRoute.Atmosphere.InitialElevation);
							}
						} 
						s++; break;
					case Instructions.FrontCoupler:
						if (Train != null)
						{
							if (CarIndex > 0 && CarIndex < Train.Cars.Length)
							{
								// not the first car in a train, hence must be coupled
								Function.Stack[s] = 1;
							}
							else
							{
								Function.Stack[s] = 0;
							}
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.FrontCouplerIndex:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								// not the first car in a train, hence must be coupled
								Function.Stack[s - 1] = 1.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RearCoupler:
						if (Train != null)
						{
							if (CarIndex == 0)
							{
								Function.Stack[s] = 0.0;
							}
							else
							{
								// if connected car is not null, then state is coupled
								Function.Stack[s] = Train.Cars[CarIndex].Coupler.ConnectedCar != null ? 1.0 : 0.0;
							}
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RearCouplerIndex:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].Coupler.ConnectedCar != null ? 1.0 : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					// default
					case Instructions.WheelRadius:
						Function.Stack[s] = 1.0;
						s++; break;
					case Instructions.WheelRadiusOfCar:
						Function.Stack[s - 1] = 1.0;
						break;
					case Instructions.EngineRunning:
					{
						if (Train != null)
						{
							bool isRunning = false;
							for (int k = 0; k < Train.Cars.Length; k++)
							{
								if (Train.Cars[k].TractionModel != null)
								{
									isRunning = Train.Cars[k].TractionModel.IsRunning;
								}

								if (isRunning)
								{
									break;
								}
							}
							Function.Stack[s] = isRunning ? 1 : 0;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
					} s++; break;
					case Instructions.EngineRunningCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel != null && Train.Cars[j].TractionModel.IsRunning)
								{
									Function.Stack[s - 1] = 1.0;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.EngineRPM:
					{
						if (Train != null)
						{
							int numEngines = 0;
							double rpmTotal = 0;
							for (int k = 0; k < Train.Cars.Length; k++)
							{
								if (Train.Cars[k].TractionModel is DieselEngine dieselEngine)
								{
									numEngines++;
									rpmTotal += dieselEngine.CurrentRPM;
								}
							}

							if (numEngines == 0)
							{
								Function.Stack[s] = 0;
							}
							else
							{
								Function.Stack[s] = rpmTotal / numEngines;
							}
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
					} s++; break;
					case Instructions.EngineRPMCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel is DieselEngine dieselEngine)
								{
									Function.Stack[s - 1] = dieselEngine.CurrentRPM;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.EnginePowerCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].TractionModel.CurrentPower;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.FuelLevel:
					{
						if (Train != null)
						{
							int totalTanks = 0;
							double fuelTotal = 0;
							for (int k = 0; k < Train.Cars.Length; k++)
							{
								if (Train.Cars[k].TractionModel is DieselEngine dieselEngine)
								{
									totalTanks++;
									fuelTotal += dieselEngine.FuelTank.CurrentLevel;
								}
							}

							if (totalTanks == 0)
							{
								Function.Stack[s] = 0;
							}
							else
							{
								Function.Stack[s] = fuelTotal / totalTanks;
							}
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
					} s++; break;
					case Instructions.FuelLevelCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel is DieselEngine dieselEngine)
								{
									Function.Stack[s - 1] = dieselEngine.FuelTank.CurrentLevel;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.Amps:
					{
						if (Train != null)
						{
							int totalMotors = 0;
							double ampsTotal = 0;
							for (int k = 0; k < Train.Cars.Length; k++)
							{
								if (Train.Cars[k].TractionModel.Components.TryGetTypedValue(EngineComponent.TractionMotor, out TractionMotor t))
								{
									totalMotors++;
									ampsTotal += t.CurrentAmps;
								}
								else if (Train.Cars[k].TractionModel.Components.TryGetTypedValue(EngineComponent.RegenerativeTractionMotor, out RegenerativeTractionMotor rt))
								{
									totalMotors++;
									ampsTotal += rt.CurrentAmps;
								}
							}

							Function.Stack[s] = ampsTotal;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
					} s++; break;
					case Instructions.AmpsCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel.Components.TryGetTypedValue(EngineComponent.TractionMotor, out TractionMotor t))
								{

									Function.Stack[s - 1] = t.CurrentAmps;
								}
								else if (Train.Cars[j].TractionModel.Components.TryGetTypedValue(EngineComponent.RegenerativeTractionMotor, out RegenerativeTractionMotor rt))
								{
									Function.Stack[s - 1] = rt.CurrentAmps;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PantographState:
					{
						if (Train != null)
						{
							int pantographState = 0;
							for (int k = 0; k < Train.Cars.Length; k++)
							{
								if (Train.Cars[k].TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph pantograph))
								{
									pantographState = (int)pantograph.State;
									break;
								}
							}
							Function.Stack[s] = pantographState;
						}
						else
						{
							Function.Stack[s] = 0.0;
						}
					} s++; break;
					case Instructions.PantographStateOfCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph p))
								{
									Function.Stack[s - 1] = (int)p.State;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.OverheadVolts:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].Voltage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.OverheadVoltsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].Voltage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailVolts:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].Voltage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailVoltsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].Voltage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailVolts:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.FourthRail].Voltage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailVoltsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.FourthRail].Voltage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.OverheadHeight:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].ContactHeight : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.OverheadHeightTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].ContactHeight : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailHeight:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].ContactHeight : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailHeightTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].ContactHeight : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailHeight:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.FourthRail].ContactHeight : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailHeightTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.FourthRail].ContactHeight : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.OverheadAC:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].VoltageType == PowerSupplyVoltageTypes.AC ? 1 : 0 : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailAC:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].VoltageType == PowerSupplyVoltageTypes.AC ? 1 : 0 : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailAC:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.FourthRail].VoltageType == PowerSupplyVoltageTypes.AC ? 1 : 0 : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.OverheadAmps:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].Amperage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.OverheadAmpsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.OverheadLine) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.OverheadLine].Amperage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailAmps:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].Amperage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.ThirdRailAmpsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.ThirdRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.ThirdRail].Amperage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.FourthRailAmps:
						if (Train != null)
						{
							Function.Stack[s - 1] = Train.AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.AvailablePowerSupplies[PowerSupplyTypes.FourthRail].Amperage : 0.0;
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.FourthRailAmpsTarget:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								Function.Stack[s - 1] = Train.Cars[j].AvailablePowerSupplies.ContainsKey(PowerSupplyTypes.FourthRail) ? Train.Cars[j].AvailablePowerSupplies[PowerSupplyTypes.FourthRail].Amperage : 0.0;
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						s++; break;
					case Instructions.CylinderCocksStateOfCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel.Components.TryGetTypedValue(EngineComponent.CylinderCocks, out CylinderCocks cc))
								{
									Function.Stack[s - 1] = cc.Opened ? 1 : 0;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.BlowersStateOfCar:
						if (Train != null)
						{
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length)
							{
								if (Train.Cars[j].TractionModel.Components.TryGetTypedValue(EngineComponent.Blowers, out Blowers b))
								{
									Function.Stack[s - 1] = b.Active ? 1 : 0;
								}
								else
								{
									Function.Stack[s - 1] = 0.0;
								}
							}
							else
							{
								Function.Stack[s - 1] = 0.0;
							}
						}
						else
						{
							Function.Stack[s - 1] = 0.0;
						}
						break;
					// default
					default:
						throw new InvalidOperationException("The unknown instruction " + Function.InstructionSet[i] + " was encountered in ExecuteFunctionScript.");
				}
			}
			Function.LastResult = Function.Stack[s - 1];
		}
	}
}
