using System;
using OpenBveApi.Math;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Runtime;
using LibRender2.Overlays;
using TrainManager.BrakeSystems;
using TrainManager.Handles;
using TrainManager.Car.Systems;

namespace RouteViewer {
	internal static class FunctionScripts {

		// execute function script
		internal static void ExecuteFunctionScript(FunctionScript function, TrainManager.Train train, int carIndex, Vector3 position, double trackPosition, int sectionIndex, bool isPartOfTrain, double timeElapsed, int currentState) {
			int s = 0, c = 0;
			for (int i = 0; i < function.InstructionSet.Length; i++) {
				switch (function.InstructionSet[i]) {
						// system
					case Instructions.SystemHalt:
						i = function.InstructionSet.Length;
						break;
					case Instructions.SystemConstant:
						function.Stack[s] = function.Constants[c];
						s++; c++; break;
					case Instructions.SystemConstantArray:
						{
							int n = (int)function.InstructionSet[i + 1];
							for (int j = 0; j < n; j++) {
								function.Stack[s + j] = function.Constants[c + j];
							} s += n; c += n; i++;
						} break;
					case Instructions.SystemValue:
						function.Stack[s] = function.LastResult;
						s++; break;
					case Instructions.SystemDelta:
						function.Stack[s] = timeElapsed;
						s++; break;
						// stack
					case Instructions.StackCopy:
						function.Stack[s] = function.Stack[s - 1];
						s++; break;
					case Instructions.StackSwap:
						(function.Stack[s - 1], function.Stack[s - 2]) = (function.Stack[s - 2], function.Stack[s - 1]);
						break;
						// math
					case Instructions.MathPlus:
						function.Stack[s - 2] += function.Stack[s - 1];
						s--; break;
					case Instructions.MathSubtract:
						function.Stack[s - 2] -= function.Stack[s - 1];
						s--; break;
					case Instructions.MathMinus:
						function.Stack[s - 1] = -function.Stack[s - 1];
						break;
					case Instructions.MathTimes:
						function.Stack[s - 2] *= function.Stack[s - 1];
						s--; break;
					case Instructions.MathDivide:
						function.Stack[s - 2] = function.Stack[s - 1] == 0.0 ? 0.0 : function.Stack[s - 2] / function.Stack[s - 1];
						s--; break;
					case Instructions.MathReciprocal:
						function.Stack[s - 1] = function.Stack[s - 1] == 0.0 ? 0.0 : 1.0 / function.Stack[s - 1];
						break;
					case Instructions.MathPower:
						{
							double a = function.Stack[s - 2];
							double b = function.Stack[s - 1];
							if (b == 2.0) {
								function.Stack[s - 2] = a * a;
							} else if (b == 3.0) {
								function.Stack[s - 2] = a * a * a;
							} else if (b == 4.0) {
								double t = a * a;
								function.Stack[s - 2] = t * t;
							} else if (b == 5.0) {
								double t = a * a;
								function.Stack[s - 2] = t * t * a;
							} else if (b == 6.0) {
								double t = a * a * a;
								function.Stack[s - 2] = t * t;
							} else if (b == 7.0) {
								double t = a * a * a;
								function.Stack[s - 2] = t * t * a;
							} else if (b == 8.0) {
								double t = a * a; t *= t;
								function.Stack[s - 2] = t * t;
							} else if (b == 0.0) {
								function.Stack[s - 2] = 1.0;
							} else if (b < 0.0) {
								function.Stack[s - 2] = 0.0;
							} else {
								function.Stack[s - 2] = Math.Pow(a, b);
							}
							s--; break;
						}
                    case Instructions.MathRandom:
                        {
                            //Generates a random number between two given doubles
                            double min = function.Stack[s - 2];
                            double max = function.Stack[s - 1];
                            var randomGenerator = new Random();
                            function.Stack[s - 2] = min + randomGenerator.NextDouble() * (max - min);
                            s--;
                        }
                        break;
                    case Instructions.MathRandomInt:
                        {
                            //Generates a random number between two given doubles
                            int min = (int)function.Stack[s - 2];
                            int max = (int)function.Stack[s - 1];
                            var randomGenerator = new Random();
                            function.Stack[s - 2] = randomGenerator.Next(min, max);
                            s--;
                        }
                        break;
					case Instructions.MathIncrement:
						function.Stack[s - 1] += 1.0;
						break;
					case Instructions.MathDecrement:
						function.Stack[s - 1] -= 1.0;
						break;
					case Instructions.MathFusedMultiplyAdd:
						function.Stack[s - 3] = function.Stack[s - 3] * function.Stack[s - 2] + function.Stack[s - 1];
						s -= 2; break;
					case Instructions.MathQuotient:
						function.Stack[s - 2] = function.Stack[s - 1] == 0.0 ? 0.0 : Math.Floor(function.Stack[s - 2] / function.Stack[s - 1]);
						s--; break;
					case Instructions.MathMod:
						function.Stack[s - 2] = function.Stack[s - 1] == 0.0 ? 0.0 : function.Stack[s - 2] - function.Stack[s - 1] * Math.Floor(function.Stack[s - 2] / function.Stack[s - 1]);
						s--; break;
					case Instructions.MathFloor:
						function.Stack[s - 1] = Math.Floor(function.Stack[s - 1]);
						break;
					case Instructions.MathCeiling:
						function.Stack[s - 1] = Math.Ceiling(function.Stack[s - 1]);
						break;
					case Instructions.MathRound:
						function.Stack[s - 1] = Math.Round(function.Stack[s - 1]);
						break;
					case Instructions.MathMin:
						function.Stack[s - 2] = function.Stack[s - 2] < function.Stack[s - 1] ? function.Stack[s - 2] : function.Stack[s - 1];
						s--; break;
					case Instructions.MathMax:
						function.Stack[s - 2] = function.Stack[s - 2] > function.Stack[s - 1] ? function.Stack[s - 2] : function.Stack[s - 1];
						s--; break;
					case Instructions.MathAbs:
						function.Stack[s - 1] = Math.Abs(function.Stack[s - 1]);
						break;
					case Instructions.MathSign:
						function.Stack[s - 1] = Math.Sign(function.Stack[s - 1]);
						break;
					case Instructions.MathExp:
						function.Stack[s - 1] = Math.Exp(function.Stack[s - 1]);
						break;
					case Instructions.MathLog:
						function.Stack[s - 1] = Log(function.Stack[s - 1]);
						break;
					case Instructions.MathSqrt:
						function.Stack[s - 1] = Sqrt(function.Stack[s - 1]);
						break;
					case Instructions.MathSin:
						function.Stack[s - 1] = Math.Sin(function.Stack[s - 1]);
						break;
					case Instructions.MathCos:
						function.Stack[s - 1] = Math.Cos(function.Stack[s - 1]);
						break;
					case Instructions.MathTan:
						function.Stack[s - 1] = Tan(function.Stack[s - 1]);
						break;
					case Instructions.MathArcTan:
						function.Stack[s - 1] = Math.Atan(function.Stack[s - 1]);
						break;
					case Instructions.MathPi:
						function.Stack[s] = Math.PI;
						s++; break;
						// comparisons
					case Instructions.CompareEqual:
						function.Stack[s - 2] = function.Stack[s - 2] == function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareUnequal:
						function.Stack[s - 2] = function.Stack[s - 2] != function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareLess:
						function.Stack[s - 2] = function.Stack[s - 2] < function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareGreater:
						function.Stack[s - 2] = function.Stack[s - 2] > function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareLessEqual:
						function.Stack[s - 2] = function.Stack[s - 2] <= function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareGreaterEqual:
						function.Stack[s - 2] = function.Stack[s - 2] >= function.Stack[s - 1] ? 1.0 : 0.0;
						s--; break;
					case Instructions.CompareConditional:
						function.Stack[s - 3] = function.Stack[s - 3] != 0.0 ? function.Stack[s - 2] : function.Stack[s - 1];
						s -= 2; break;
						// logical
					case Instructions.LogicalNot:
						function.Stack[s - 1] = function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						break;
					case Instructions.LogicalAnd:
						function.Stack[s - 2] = function.Stack[s - 2] != 0.0 & function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.LogicalOr:
						function.Stack[s - 2] = function.Stack[s - 2] != 0.0 | function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.LogicalNand:
						function.Stack[s - 2] = function.Stack[s - 2] != 0.0 & function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						s--; break;
					case Instructions.LogicalNor:
						function.Stack[s - 2] = function.Stack[s - 2] != 0.0 | function.Stack[s - 1] != 0.0 ? 0.0 : 1.0;
						s--; break;
					case Instructions.LogicalXor:
						function.Stack[s - 2] = function.Stack[s - 2] != 0.0 ^ function.Stack[s - 1] != 0.0 ? 1.0 : 0.0;
						s--; break;
					case Instructions.CurrentObjectState:
						function.Stack[s] = currentState;
						s++;
						break;
						// time/camera
					case Instructions.TimeSecondsSinceMidnight:
						function.Stack[s] = Game.SecondsSinceMidnight;
						s++; break;
					case Instructions.TimeHourDigit:
						function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight / 3600.0);
						s++; break;
					case Instructions.TimeMinuteDigit:
						function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight / 60 % 60);
						s++; break;
					case Instructions.TimeSecondDigit:
						function.Stack[s] = Math.Floor(Program.CurrentRoute.SecondsSinceMidnight % 60);
						s++; break;
					case Instructions.CameraDistance:
						{
							double dx = Program.Renderer.Camera.AbsolutePosition.X - position.X;
							double dy = Program.Renderer.Camera.AbsolutePosition.Y - position.Y;
							double dz = Program.Renderer.Camera.AbsolutePosition.Z - position.Z;
							function.Stack[s] = Math.Sqrt(dx * dx + dy * dy + dz * dz);
							s++;
						} break;
					case Instructions.CameraXDistance:
						{
							function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.X - position.X;
							s++;
						} break;
					case Instructions.CameraYDistance:
						{
							function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.Y - position.Y;
							s++;
						} break;
					case Instructions.CameraZDistance:
						{
							function.Stack[s] = Program.Renderer.Camera.AbsolutePosition.Z - position.Z;
							s++;
						} break;
					case Instructions.BillboardX:
						{
							Vector3 toCamera = Program.Renderer.Camera.AbsolutePosition - position;
							function.Stack[s] = Math.Atan2(toCamera.Y, -toCamera.Z);
							s++;
						} break;
					case Instructions.BillboardY:
						{
							Vector3 toCamera = Program.Renderer.Camera.AbsolutePosition - position;
							function.Stack[s] = Math.Atan2(-toCamera.Z, toCamera.X);
							s++;
						} break;
					case Instructions.CameraView:
						//Returns whether the camera is in interior or exterior mode
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior)
						{
							function.Stack[s] = 0;
						}
						else
						{
							function.Stack[s] = 1;
						}
						s++; break;
						// train
					case Instructions.PlayerTrain:
						if (isPartOfTrain && train != null)
						{
							function.Stack[s] = train.IsPlayerTrain ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainCars:
						if (train != null) {
							function.Stack[s] = train.Cars.Length;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainDestination:
						if (train != null) {
							function.Stack[s] = train.Destination;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainLength:
						if (train != null) {
							function.Stack[s] = train.Length;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeed:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CurrentSpeed;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedOfCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CurrentSpeed;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainSpeedometer:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].Specs.PerceivedSpeed;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedometerOfCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].Specs.PerceivedSpeed;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainAcceleration:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].Specs.Acceleration;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainAccelerationOfCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].Specs.Acceleration;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainAccelerationMotor:
						if (train != null) {
							function.Stack[s] = 0.0;
							for (int j = 0; j < train.Cars.Length; j++) {
								if (train.Cars[j].Specs.IsMotorCar) {
									// hack: MotorAcceleration does not distinguish between forward/backward
									if (train.Cars[j].Specs.MotorAcceleration < 0.0) {
										function.Stack[s] = train.Cars[j].Specs.MotorAcceleration * Math.Sign(train.Cars[j].CurrentSpeed);
									} else if (train.Cars[j].Specs.MotorAcceleration > 0.0) {
										function.Stack[s] = train.Cars[j].Specs.MotorAcceleration * (double)train.Handles.Reverser.Actual;
									} else {
										function.Stack[s] = 0.0;
									}
									break;
								}
							}
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainAccelerationMotorOfCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								// hack: MotorAcceleration does not distinguish between forward/backward
								if (train.Cars[j].Specs.MotorAcceleration < 0.0) {
									function.Stack[s - 1] = train.Cars[j].Specs.MotorAcceleration * Math.Sign(train.Cars[j].CurrentSpeed);
								} else if (train.Cars[j].Specs.MotorAcceleration > 0.0) {
									function.Stack[s - 1] = train.Cars[j].Specs.MotorAcceleration * (double)train.Handles.Reverser.Actual;
								} else {
									function.Stack[s - 1] = 0.0;
								}
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PlayerTrainDistance:
						double playerDist = double.MaxValue;
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							double fx = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.X - position.X;
							double fy = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.Y - position.Y;
							double fz = TrainManager.PlayerTrain.Cars[j].FrontAxle.Follower.WorldPosition.Z - position.Z;
							double f = fx * fx + fy * fy + fz * fz;
							if (f < playerDist) playerDist = f;
							double rx = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.X - position.X;
							double ry = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.Y - position.Y;
							double rz = TrainManager.PlayerTrain.Cars[j].RearAxle.Follower.WorldPosition.Z - position.Z;
							double r = rx * rx + ry * ry + rz * rz;
							if (r < playerDist) playerDist = r;
						}
						function.Stack[s] = Math.Sqrt(playerDist);
						s++; break;
					case Instructions.TrainDistance:
						if (train != null) {
							double dist = double.MaxValue;
							for (int j = 0; j < train.Cars.Length; j++) {
								double fx = train.Cars[j].FrontAxle.Follower.WorldPosition.X - position.X;
								double fy = train.Cars[j].FrontAxle.Follower.WorldPosition.Y - position.Y;
								double fz = train.Cars[j].FrontAxle.Follower.WorldPosition.Z - position.Z;
								double f = fx * fx + fy * fy + fz * fz;
								if (f < dist) dist = f;
								double rx = train.Cars[j].RearAxle.Follower.WorldPosition.X - position.X;
								double ry = train.Cars[j].RearAxle.Follower.WorldPosition.Y - position.Y;
								double rz = train.Cars[j].RearAxle.Follower.WorldPosition.Z - position.Z;
								double r = rx * rx + ry * ry + rz * rz;
								if (r < dist) dist = r;
							}
							function.Stack[s] = Math.Sqrt(dist);
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainDistanceToCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								double x = 0.5 * (train.Cars[j].FrontAxle.Follower.WorldPosition.X + train.Cars[j].RearAxle.Follower.WorldPosition.X) - position.X;
								double y = 0.5 * (train.Cars[j].FrontAxle.Follower.WorldPosition.Y + train.Cars[j].RearAxle.Follower.WorldPosition.Y) - position.Y;
								double z = 0.5 * (train.Cars[j].FrontAxle.Follower.WorldPosition.Z + train.Cars[j].RearAxle.Follower.WorldPosition.Z) - position.Z;
								function.Stack[s - 1] = Math.Sqrt(x * x + y * y + z * z);
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PlayerTrackDistance:
						double pt0 = TrainManager.PlayerTrain.FrontCarTrackPosition;
						double pt1 = TrainManager.PlayerTrain.RearCarTrackPosition;
						function.Stack[s] = trackPosition > pt0 ? trackPosition - pt0 : trackPosition < pt1 ? trackPosition - pt1 : 0.0;
						s++; break;
					case Instructions.TrainTrackDistance:
						if (train != null) {
							double t0 = train.FrontCarTrackPosition;
							double t1 = train.RearCarTrackPosition;
							function.Stack[s] = trackPosition > t0 ? trackPosition - t0 : trackPosition < t1 ? trackPosition - t1 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.CurveRadius:
						function.Stack[s] = 0.0;
						s++;
						break;
                    case Instructions.CurveRadiusOfCar:
                        if (train == null)
                        {
                            function.Stack[s - 1] = 0.0;
                        }
                        else
                        {
                            int j = (int)Math.Round(function.Stack[s - 1]);
                            if (j < 0) j += train.Cars.Length;
                            if (j >= 0 & j < train.Cars.Length)
                            {
                                function.Stack[s - 1] = (train.Cars[j].FrontAxle.Follower.CurveRadius + train.Cars[j].RearAxle.Follower.CurveRadius) / 2;
                            }
                            else
                            {
                                function.Stack[s - 1] = 0.0;
                            }
                        }
                        break;
					case Instructions.FrontAxleCurveRadius:
						function.Stack[s] = 0.0;
						s++;
						break;
                    case Instructions.FrontAxleCurveRadiusOfCar:
                        if (train == null)
                        {
                            function.Stack[s - 1] = 0.0;
                        }
                        else
                        {
                            int j = (int)Math.Round(function.Stack[s - 1]);
                            if (j < 0) j += train.Cars.Length;
                            if (j >= 0 & j < train.Cars.Length)
                            {
                                function.Stack[s - 1] = train.Cars[j].FrontAxle.Follower.CurveRadius;
                            }
                            else
                            {
                                function.Stack[s - 1] = 0.0;
                            }
                        }
                        break;
					case Instructions.RearAxleCurveRadius:
						function.Stack[s] = 0.0;
						s++;
						break;
                    case Instructions.RearAxleCurveRadiusOfCar:
                        if (train == null)
                        {
                            function.Stack[s - 1] = 0.0;
                        }
                        else
                        {
                            int j = (int)Math.Round(function.Stack[s - 1]);
                            if (j < 0) j += train.Cars.Length;
                            if (j >= 0 & j < train.Cars.Length)
                            {
                                function.Stack[s - 1] = train.Cars[j].RearAxle.Follower.CurveRadius;
                            }
                            else
                            {
                                function.Stack[s - 1] = 0.0;
                            }
                        }
                        break;
					case Instructions.CurveCant:
						function.Stack[s] = 0.0;
						s++;
						break;
                    case Instructions.CurveCantOfCar:
                        if (train == null)
                        {
                            function.Stack[s - 1] = 0.0;
                        }
                        else
                        {
                            int j = (int)Math.Round(function.Stack[s - 1]);
                            if (j < 0) j += train.Cars.Length;
                            if (j >= 0 & j < train.Cars.Length)
                            {
                                function.Stack[s - 1] = train.Cars[j].FrontAxle.Follower.CurveCant;
                            }
                            else
                            {
                                function.Stack[s - 1] = 0.0;
                            }
                        }
                        break;
					case Instructions.Pitch:
						function.Stack[s] = 0.0;
						s++;
						break;
					case Instructions.PitchOfCar:
						if (train == null)
						{
							function.Stack[s - 1] = 0.0;
						}
						else
						{
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length)
							{
								function.Stack[s - 1] = train.Cars[j].FrontAxle.Follower.Pitch;
							}
							else
							{
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.Odometer:
						function.Stack[s] = 0.0;
						s++;
						break;
					case Instructions.OdometerOfCar:
						function.Stack[s -1] = 0.0;
						break;
					case Instructions.TrainTrackDistanceToCar:
						if (train != null) {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								double p = 0.5 * (train.Cars[j].FrontAxle.Follower.TrackPosition + train.Cars[j].RearAxle.Follower.TrackPosition);
								function.Stack[s - 1] = trackPosition - p;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
						// door
					case Instructions.Doors:
						if (train != null) {
							double a = 0.0;
							for (int j = 0; j < train.Cars.Length; j++) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s] = a;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.DoorsIndex:
						if (train != null) {
							double a = 0.0;
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s - 1] = a;
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.LeftDoors:
						if (train != null) {
							double a = 0.0;
							for (int j = 0; j < train.Cars.Length; j++) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].Direction == -1 & train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s] = a;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LeftDoorsIndex:
						if (train != null) {
							double a = 0.0;
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].Direction == -1 & train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s - 1] = a;
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RightDoors:
						if (train != null) {
							double a = 0.0;
							for (int j = 0; j < train.Cars.Length; j++) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].Direction == 1 & train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s] = a;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RightDoorsIndex:
						if (train != null) {
							double a = 0.0;
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[k].Direction == 1 & train.Cars[j].Doors[k].State > a) {
										a = train.Cars[j].Doors[k].State;
									}
								}
							}
							function.Stack[s - 1] = a;
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.LeftDoorsTarget:
						if (train != null) {
							bool q = false;
							for (int j = 0; j < train.Cars.Length; j++) {
								if (train.Cars[j].Doors[0].AnticipatedOpen) {
									q = true;
									break;
								}
							}
							function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.LeftDoorsTargetIndex:
						if (train != null) {
							bool q = false;
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[0].AnticipatedOpen) {
										q = true;
										break;
									}
								}
							}
							function.Stack[s - 1] = q ? 1.0 : 0.0;
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RightDoorsTarget:
						if (train != null) {
							bool q = false;
							for (int j = 0; j < train.Cars.Length; j++) {
								if (train.Cars[j].Doors[1].AnticipatedOpen) {
									q = true;
									break;
								}
							}
							function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.RightDoorsTargetIndex:
						if (train != null) {
							bool q = false;
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								for (int k = 0; k < train.Cars[j].Doors.Length; k++) {
									if (train.Cars[j].Doors[1].AnticipatedOpen) {
										q = true;
										break;
									}
								}
							}
							function.Stack[s - 1] = q ? 1.0 : 0.0;
						} else {
							function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.PilotLamp:
						//Not currently supported in viewers
						function.Stack[s] = 0.0;
						s++; break;
					case Instructions.PassAlarm:
						//Not currently supported in viewers
						function.Stack[s] = 0.0;
						s++; break;
					case Instructions.StationAdjustAlarm:
						//Not currently supported in viewers
						function.Stack[s] = 0.0;
						s++; break;
					case Instructions.Headlights:
						if (train != null)
						{
							function.Stack[s] = train.SafetySystems.Headlights.CurrentState;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					// handles
					case Instructions.ReverserNotch:
						if (train != null) {
							function.Stack[s] = (double)train.Handles.Reverser.Driver;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotch:
						if (train != null) {
							function.Stack[s] = train.Handles.Power.Driver;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotches:
						if (train != null) {
							function.Stack[s] = train.Handles.Power.MaximumNotch;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotch:
						if (train != null) {
							function.Stack[s] = train.Handles.Brake.Driver;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotches:
						if (train != null) {
							if (train.Handles.Brake is AirBrakeHandle) {
								function.Stack[s] = 2.0;
							} else {
								function.Stack[s] = train.Handles.Brake.MaximumNotch;
							}
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchLinear:
						if (train != null) {
							if (train.Handles.Brake is AirBrakeHandle) {
								if (train.Handles.EmergencyBrake.Driver) {
									function.Stack[s] = 3.0;
								} else {
									function.Stack[s] = train.Handles.Brake.Driver;
								}
							} else if (train.Handles.HasHoldBrake) {
								if (train.Handles.EmergencyBrake.Driver) {
									function.Stack[s] = train.Handles.Brake.MaximumNotch + 2.0;
								} else if (train.Handles.Brake.Driver > 0) {
									function.Stack[s] = train.Handles.Brake.Driver + 1.0;
								} else {
									function.Stack[s] = train.Handles.HoldBrake.Driver ? 1.0 : 0.0;
								}
							} else {
								if (train.Handles.EmergencyBrake.Driver) {
									function.Stack[s] = train.Handles.Brake.MaximumNotch + 1.0;
								} else {
									function.Stack[s] = train.Handles.Brake.Driver;
								}
							}
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchesLinear:
						if (train != null) {
							if (train.Handles.Brake is AirBrakeHandle) {
								function.Stack[s] = 3.0;
							} else if (train.Handles.HasHoldBrake) {
								function.Stack[s] = train.Handles.Brake.MaximumNotch + 2.0;
							} else {
								function.Stack[s] = train.Handles.Brake.MaximumNotch + 1.0;
							}
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.EmergencyBrake:
						if (train != null) {
							function.Stack[s] = train.Handles.EmergencyBrake.Driver ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.Klaxon:
						//Object Viewer doesn't actually have a sound player, so we can't test against it, thus return zero..
						function.Stack[s] = 0.0;
						s++; break;
					case Instructions.HasAirBrake:
						if (train != null) {
							function.Stack[s] = train.Cars[train.DriverCar].CarBrake is AutomaticAirBrake ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HoldBrake:
						if (train != null) {
							function.Stack[s] = train.Handles.HoldBrake.Driver ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasHoldBrake:
						if (train != null) {
							function.Stack[s] = train.Handles.HasHoldBrake ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.ConstSpeed:
						if (train != null) {
							function.Stack[s] = train.Specs.CurrentConstSpeed ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasConstSpeed:
						if (train != null) {
							function.Stack[s] = train.Specs.HasConstSpeed ? 1.0 : 0.0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
						// brake
					case Instructions.BrakeMainReservoir:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CarBrake.MainReservoir.CurrentPressure;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeMainReservoirOfCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CarBrake.MainReservoir.CurrentPressure;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeEqualizingReservoir:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CarBrake.EqualizingReservoir.CurrentPressure;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeEqualizingReservoirOfCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CarBrake.EqualizingReservoir.CurrentPressure;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakePipe:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CarBrake.BrakePipe.CurrentPressure;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeBrakePipeOfCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CarBrake.BrakePipe.CurrentPressure;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakeCylinder:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CarBrake.BrakeCylinder.CurrentPressure;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeBrakeCylinderOfCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CarBrake.BrakeCylinder.CurrentPressure;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeStraightAirPipe:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].CarBrake.StraightAirPipe.CurrentPressure;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeStraightAirPipeOfCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].CarBrake.StraightAirPipe.CurrentPressure;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
						// safety
					case Instructions.SafetyPluginAvailable:
						//Not supported by Route Viewer
						function.Stack[s] = 0.0;
						s++; break;
					case Instructions.SafetyPluginState:
						//Not supported by Route Viewer
						function.Stack[s - 1] = 0.0;
						break;
						// timetable
					case Instructions.TimetableVisible:
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Custom:
							case DisplayedTimetable.Default:
								function.Stack[s] = 1.0;
								break;
							case DisplayedTimetable.None:
								function.Stack[s] = 0.0;
								break;
						}
						s++; break;
					case Instructions.DistanceNextStation:
					case Instructions.DistanceLastStation:
					case Instructions.StopsNextStation:
					case Instructions.NextStation:
					case Instructions.NextStationStop:
					case Instructions.RouteLimit:
						function.Stack[s] = 0.0; //Unsupported in viewers
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
						function.Stack[s] = idx;
						s++; break;
					case Instructions.DistanceStation:
					case Instructions.StopsStation:
						function.Stack[s - 1] = 0.0; //Unsupported in viewers
						break;
						// sections
					case Instructions.SectionAspectNumber:
						if (isPartOfTrain) {
							int nextSectionIndex = train.CurrentSectionIndex + 1;
							if (nextSectionIndex >= 0 & nextSectionIndex < Program.CurrentRoute.Sections.Length) {
								int a = Program.CurrentRoute.Sections[nextSectionIndex].CurrentAspect;
								if (a >= 0 & a < Program.CurrentRoute.Sections[nextSectionIndex].Aspects.Length) {
									function.Stack[s] = Program.CurrentRoute.Sections[nextSectionIndex].Aspects[a].Number;
								} else {
									function.Stack[s] = 0;
								}
							}
						} else if (sectionIndex >= 0 & sectionIndex < Program.CurrentRoute.Sections.Length) {
							int a = Program.CurrentRoute.Sections[sectionIndex].CurrentAspect;
							if (a >= 0 & a < Program.CurrentRoute.Sections[sectionIndex].Aspects.Length) {
								function.Stack[s] = Program.CurrentRoute.Sections[sectionIndex].Aspects[a].Number;
							} else {
								function.Stack[s] = 0;
							}
						} else {
							function.Stack[s] = 0;
						}
						s++; break;
						case Instructions.Panel2Timetable:
						throw new InvalidOperationException("The instruction " + function.InstructionSet[i].ToString() + " is for internal use only, and should not be added to objects.");
					case Instructions.TrainCarNumber:
						if (!isPartOfTrain)
						{
							function.Stack[s] = -1;
						}
						else
						{
							function.Stack[s] = carIndex;
						}
						s++;
						break;
					case Instructions.WheelSlip:
						if (train != null) {
							function.Stack[s] = train.Cars[carIndex].FrontAxle.CurrentWheelSlip ? 1 : 0;
						} else {
							function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.WheelSlipCar:
						if (train == null) {
							function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(function.Stack[s - 1]);
							if (j < 0) j += train.Cars.Length;
							if (j >= 0 & j < train.Cars.Length) {
								function.Stack[s - 1] = train.Cars[j].FrontAxle.CurrentWheelSlip ? 1 : 0;
							} else {
								function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.Sanders:
						{
							if (train != null && train.Cars[carIndex].ReAdhesionDevice is Sanders sanders) {
								function.Stack[s] = sanders.Active ? 1 : 0;
							} else {
								function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.SandLevel:
						{
							if (train != null && train.Cars[carIndex].ReAdhesionDevice is Sanders sanders) {
								function.Stack[s] = sanders.SandLevel;
							} else {
								function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.SandShots:
						{
							if (train != null && train.Cars[carIndex].ReAdhesionDevice is Sanders sanders) {
								function.Stack[s] = sanders.NumberOfShots;
							} else {
								function.Stack[s] = 0.0;
							}
						} 
						s++; break;
					case Instructions.DSD:
						{
							if (train != null && train.Cars[train.DriverCar].DSD != null)
							{
								function.Stack[s] = train.Cars[train.DriverCar].DSD.Triggered ? 1 : 0;
							}
							else
							{
								function.Stack[s] = 0.0;
							}
						}
						s++; break;
					case Instructions.AmbientTemperature:
						{
							if (train != null)
							{
								function.Stack[s] = Program.CurrentRoute.Atmosphere.GetAirTemperature(train.Cars[carIndex].FrontAxle.Follower.WorldPosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation);
							}
							else
							{
								function.Stack[s] = Program.CurrentRoute.Atmosphere.GetAirTemperature(position.Y + Program.CurrentRoute.Atmosphere.InitialElevation);
							}
						} 
						s++; break;
					case Instructions.RainDrop:
					case Instructions.SnowFlake:
						// Only shown on the player train, so not helpful here
						function.Stack[s - 1] = 0.0;
						break;
					case Instructions.WiperPosition:
						function.Stack[s] = 1.0;
						s++; break;
					default:
						throw new InvalidOperationException("The unknown instruction " + function.InstructionSet[i].ToString() + " was encountered in ExecuteFunctionScript.");
				}
			}
			function.LastResult = function.Stack[s - 1];
		}
		
		// mathematical functions
		private static double Log(double X)
		{
			if (X <= 0.0) 
			{
				return 0.0; // ComplexInfinity or NonReal
			}
			return Math.Log(X);
		}
		private static double Sqrt(double X)
		{
			if (X < 0.0)
			{
				return 0.0; // NonReal
			}
			return Math.Sqrt(X);
		}
		private static double Tan(double X) {
			double c = X / Math.PI;
			double d = c - Math.Floor(c) - 0.5;
			double e = Math.Floor(X >= 0.0 ? X : -X) * 1.38462643383279E-16;
			if (d >= -e & d <= e)
			{
				return 0.0; // ComplexInfinity
			}
			return Math.Tan(X);
		}

	}
}
