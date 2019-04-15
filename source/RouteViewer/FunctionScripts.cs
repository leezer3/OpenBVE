using System;
using OpenBveApi.Math;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Runtime;

namespace OpenBve {
	internal static class FunctionScripts {

		// execute function script
		internal static void ExecuteFunctionScript(FunctionScript Function, TrainManager.Train Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState) {
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
						{
							double a = Function.Stack[s - 1];
							Function.Stack[s - 1] = Function.Stack[s - 2];
							Function.Stack[s - 2] = a;
						} break;
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
                            Function.Stack[s - 2] = min + randomGenerator.NextDouble() * (max - min);
                            s--;
                        }
                        break;
                    case Instructions.MathRandomInt:
                        {
                            //Generates a random number between two given doubles
                            int min = (int)Function.Stack[s - 2];
                            int max = (int)Function.Stack[s - 1];
                            var randomGenerator = new Random();
                            Function.Stack[s - 2] = randomGenerator.Next(min, max);
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
						Function.Stack[s - 1] = Log(Function.Stack[s - 1]);
						break;
					case Instructions.MathSqrt:
						Function.Stack[s - 1] = Sqrt(Function.Stack[s - 1]);
						break;
					case Instructions.MathSin:
						Function.Stack[s - 1] = Math.Sin(Function.Stack[s - 1]);
						break;
					case Instructions.MathCos:
						Function.Stack[s - 1] = Math.Cos(Function.Stack[s - 1]);
						break;
					case Instructions.MathTan:
						Function.Stack[s - 1] = Tan(Function.Stack[s - 1]);
						break;
					case Instructions.MathArcTan:
						Function.Stack[s - 1] = Math.Atan(Function.Stack[s - 1]);
						break;
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
						break;
						// time/camera
					case Instructions.TimeSecondsSinceMidnight:
						Function.Stack[s] = Game.SecondsSinceMidnight;
						s++; break;
					case Instructions.CameraDistance:
						{
							double dx = World.AbsoluteCameraPosition.X - Position.X;
							double dy = World.AbsoluteCameraPosition.Y - Position.Y;
							double dz = World.AbsoluteCameraPosition.Z - Position.Z;
							Function.Stack[s] = Math.Sqrt(dx * dx + dy * dy + dz * dz);
							s++;
						} break;
					case Instructions.CameraView:
						//Returns whether the camera is in interior or exterior mode
						if (World.CameraMode == CameraViewMode.Interior)
						{
							Function.Stack[s] = 0;
						}
						else
						{
							Function.Stack[s] = 1;
						}
						s++; break;
						// train
					case Instructions.TrainCars:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Cars.Length;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainDestination:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Destination;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeed:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.CurrentSpeed;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.CurrentSpeed;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainSpeedometer:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainSpeedometerOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.CurrentPerceivedSpeed;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.TrainAcceleration:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.CurrentAcceleration;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.TrainAccelerationOfCar:
						if (Train != null) {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.CurrentAcceleration;
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
								if (Train.Cars[j].Specs.IsMotorCar) {
									// hack: CurrentAccelerationOutput does not distinguish between forward/backward
									if (Train.Cars[j].Specs.CurrentAccelerationOutput < 0.0) {
										Function.Stack[s] = Train.Cars[j].Specs.CurrentAccelerationOutput * (double)Math.Sign(Train.Cars[j].Specs.CurrentSpeed);
									} else if (Train.Cars[j].Specs.CurrentAccelerationOutput > 0.0) {
										Function.Stack[s] = Train.Cars[j].Specs.CurrentAccelerationOutput * (double)Train.Specs.CurrentReverser.Actual;
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
								// hack: CurrentAccelerationOutput does not distinguish between forward/backward
								if (Train.Cars[j].Specs.CurrentAccelerationOutput < 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].Specs.CurrentAccelerationOutput * (double)Math.Sign(Train.Cars[j].Specs.CurrentSpeed);
								} else if (Train.Cars[j].Specs.CurrentAccelerationOutput > 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].Specs.CurrentAccelerationOutput * (double)Train.Specs.CurrentReverser.Actual;
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
					case Instructions.TrainTrackDistance:
						if (Train != null) {
							int r = Train.Cars.Length - 1;
							double t0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
							double t1 = Train.Cars[r].RearAxle.Follower.TrackPosition - Train.Cars[r].RearAxlePosition - 0.5 * Train.Cars[r].Length;
							Function.Stack[s] = TrackPosition > t0 ? TrackPosition - t0 : TrackPosition < t1 ? TrackPosition - t1 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.CurveRadius:
						Function.Stack[s] = 0.0;
						s++;
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
						Function.Stack[s] = 0.0;
						s++;
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
						Function.Stack[s] = 0.0;
						s++;
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
						Function.Stack[s] = 0.0;
						s++;
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
						Function.Stack[s] = 0.0;
						s++;
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
						Function.Stack[s] = 0.0;
						s++;
						break;
					case Instructions.OdometerOfCar:
						Function.Stack[s -1] = 0.0;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].Direction == -1 & Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].Direction == -1 & Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].Direction == 1 & Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].Direction == 1 & Train.Cars[j].Specs.Doors[k].State > a) {
										a = Train.Cars[j].Specs.Doors[k].State;
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
								if (Train.Cars[j].Specs.AnticipatedLeftDoorsOpened) {
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.AnticipatedLeftDoorsOpened) {
										q = true;
										break;
									}
								}
							}
							Function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
					case Instructions.RightDoorsTarget:
						if (Train != null) {
							bool q = false;
							for (int j = 0; j < Train.Cars.Length; j++) {
								if (Train.Cars[j].Specs.AnticipatedRightDoorsOpened) {
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
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.AnticipatedRightDoorsOpened) {
										q = true;
										break;
									}
								}
							}
							Function.Stack[s] = q ? 1.0 : 0.0;
						} else {
							Function.Stack[s - 1] = 0.0;
						}
						break;
						// handles
					case Instructions.ReverserNotch:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Specs.CurrentReverser.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotch:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Specs.CurrentPowerNotch.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotches:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Specs.MaximumPowerNotch;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotch:
						if (Train != null) {
							if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
								Function.Stack[s] = (double)Train.Specs.AirBrake.Handle.Driver;
							} else {
								Function.Stack[s] = (double)Train.Specs.CurrentBrakeNotch.Driver;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotches:
						if (Train != null) {
							if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
								Function.Stack[s] = 2.0;
							} else {
								Function.Stack[s] = (double)Train.Specs.MaximumBrakeNotch;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchLinear:
						if (Train != null) {
							if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
								if (Train.Specs.CurrentEmergencyBrake.Driver) {
									Function.Stack[s] = 3.0;
								} else {
									Function.Stack[s] = (double)Train.Specs.AirBrake.Handle.Driver;
								}
							} else if (Train.Specs.HasHoldBrake) {
								if (Train.Specs.CurrentEmergencyBrake.Driver) {
									Function.Stack[s] = (double)Train.Specs.MaximumBrakeNotch + 2.0;
								} else if (Train.Specs.CurrentBrakeNotch.Driver > 0) {
									Function.Stack[s] = (double)Train.Specs.CurrentBrakeNotch.Driver + 1.0;
								} else {
									Function.Stack[s] = Train.Specs.CurrentHoldBrake.Driver ? 1.0 : 0.0;
								}
							} else {
								if (Train.Specs.CurrentEmergencyBrake.Driver) {
									Function.Stack[s] = (double)Train.Specs.MaximumBrakeNotch + 1.0;
								} else {
									Function.Stack[s] = (double)Train.Specs.CurrentBrakeNotch.Driver;
								}
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotchesLinear:
						if (Train != null) {
							if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
								Function.Stack[s] = 3.0;
							} else if (Train.Specs.HasHoldBrake) {
								Function.Stack[s] = Train.Specs.MaximumBrakeNotch + 2.0;
							} else {
								Function.Stack[s] = Train.Specs.MaximumBrakeNotch + 1.0;
							}
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.EmergencyBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Specs.CurrentEmergencyBrake.Driver ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.Klaxon:
						//Object Viewer doesn't actually have a sound player, so we can't test against it, thus return zero....
						Function.Stack[s] = 0.0;
						s++; break;
					case Instructions.HasAirBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HoldBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Specs.CurrentHoldBrake.Driver ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.HasHoldBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Specs.HasHoldBrake ? 1.0 : 0.0;
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
							Function.Stack[s] = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].Specs.AirBrake.MainReservoirCurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeEqualizingReservoir:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakePipe:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].Specs.AirBrake.BrakePipeCurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakeCylinder:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].Specs.AirBrake.BrakeCylinderCurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeStraightAirPipe:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure;
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
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Specs.AirBrake.StraightAirPipeCurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
						// safety
					case Instructions.SafetyPluginAvailable:
						if (Train == TrainManager.PlayerTrain) {
							Function.Stack[s] = TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Plugin ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.SafetyPluginState:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int n = (int)Math.Round(Function.Stack[s - 1]);
							if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.Plugin) {
								if (n >= 0 & n < PluginManager.CurrentPlugin.Panel.Length) {
									Function.Stack[s - 1] = (double)PluginManager.CurrentPlugin.Panel[n];
								} else {
									Function.Stack[s - 1] = 0.0;
								}
							} else {
								Function.Stack[s - 1] = 0.0;
								switch(n) {
									case 256:
										// ATS
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Normal | Train.Specs.Safety.State == TrainManager.SafetyState.Initialization) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 257:
										// ATS RUN (separate flashing)
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Ringing) {
												Function.Stack[s - 1] = 1.0;
											} else if (Train.Specs.Safety.State == TrainManager.SafetyState.Emergency | Train.Specs.Safety.State == TrainManager.SafetyState.Pattern | Train.Specs.Safety.State == TrainManager.SafetyState.Service) {
												Function.Stack[s - 1] = 2.0;
											}
										} break;
									case 258:
										// ATS RUN (integrated flashing)
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Ringing) {
												Function.Stack[s - 1] = 1.0;
											} else if (Train.Specs.Safety.State == TrainManager.SafetyState.Emergency | Train.Specs.Safety.State == TrainManager.SafetyState.Pattern | Train.Specs.Safety.State == TrainManager.SafetyState.Service) {
												if (((int)Math.Floor(2.0 * Game.SecondsSinceMidnight) & 1) == 0) {
													Function.Stack[s - 1] = 1.0;
												} else {
													Function.Stack[s - 1] = 0.0;
												}
											}
										} break;
									case 259:
										// P POWER
										if ((Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn | Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) & Train.Specs.Safety.Ats.AtsPAvailable) {
											Function.Stack[s - 1] = 1.0;
										} break;
									case 260:
										// PATTERN APPROACH
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Pattern | Train.Specs.Safety.State == TrainManager.SafetyState.Service) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 261:
										// BRAKE RELEASE
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
											if (Train.Specs.Safety.Ats.AtsPOverride) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 262:
										// BRAKE OPERATION
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Service & !Train.Specs.Safety.Ats.AtsPOverride) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 263:
										// ATS-P
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
											Function.Stack[s - 1] = 1.0;
										} break;
									case 264:
										// FAILURE
										if (Train.Specs.Safety.Mode != TrainManager.SafetySystem.None) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Initialization) {
												Function.Stack[s - 1] = 1.0;
											} else if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
												if (Train.Specs.Safety.State == TrainManager.SafetyState.Ringing | Train.Specs.Safety.State == TrainManager.SafetyState.Emergency) {
													Function.Stack[s - 1] = 1.0;
												}
											}
										} break;
									case 265:
										// ATC
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
											Function.Stack[s - 1] = 1.0;
										} break;
									case 266:
										// ATC POWER
										if ((Train.Specs.Safety.Mode == TrainManager.SafetySystem.Atc | Train.Specs.Safety.Mode != TrainManager.SafetySystem.None & Train.Specs.Safety.Atc.AutomaticSwitch)) {
											Function.Stack[s - 1] = 1.0;
										} break;
									case 267:
										// ATC SERVICE
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
											if (Train.Specs.Safety.State == TrainManager.SafetyState.Service) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 268:
										// ATC EMERGENCY
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
											if (!Train.Specs.Safety.Atc.Transmitting) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 269:
										// EB
										if (Train.Specs.Safety.Mode != TrainManager.SafetySystem.None) {
											if (Train.Specs.Safety.Eb.BellState == TrainManager.SafetyState.Ringing) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 270:
										// CONST SPEED
										if (Train.Specs.HasConstSpeed) {
											if (Train.Specs.CurrentConstSpeed) {
												Function.Stack[s - 1] = 1.0;
											}
										} break;
									case 271:
										// atc speedometer state
										if (Train.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
											if (!Train.Specs.Safety.Atc.Transmitting) {
												Function.Stack[s - 1] = 0.0;
											} else {
												if (Train.Specs.Safety.Atc.SpeedRestriction < 4.1666) {
													Function.Stack[s - 1] = 1.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 6.9443) {
													Function.Stack[s - 1] = 2.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 12.4999) {
													Function.Stack[s - 1] = 3.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 15.2777) {
													Function.Stack[s - 1] = 4.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 18.0555) {
													Function.Stack[s - 1] = 5.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 20.8333) {
													Function.Stack[s - 1] = 6.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 24.9999) {
													Function.Stack[s - 1] = 7.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 27.7777) {
													Function.Stack[s - 1] = 8.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 30.5555) {
													Function.Stack[s - 1] = 9.0;
												} else if (Train.Specs.Safety.Atc.SpeedRestriction < 33.3333) {
													Function.Stack[s - 1] = 10.0;
												} else {
													Function.Stack[s - 1] = 11.0;
												}
											}
										} else {
											Function.Stack[s - 1] = 12.0;
										} break;

								}
							}
						} break;
						// timetable
					case Instructions.TimetableVisible:
						Function.Stack[s] = Timetable.CurrentTimetable == Timetable.TimetableState.Custom & Timetable.CustomTimetableAvailable ? 0.0 : -1.0;
						s++; break;
						// sections
					case Instructions.SectionAspectNumber:
						if (IsPartOfTrain) {
							int nextSectionIndex = Train.CurrentSectionIndex + 1;
							if (nextSectionIndex >= 0 & nextSectionIndex < Game.Sections.Length) {
								int a = Game.Sections[nextSectionIndex].CurrentAspect;
								if (a >= 0 & a < Game.Sections[nextSectionIndex].Aspects.Length) {
									Function.Stack[s] = (double)Game.Sections[nextSectionIndex].Aspects[a].Number;
								} else {
									Function.Stack[s] = 0;
								}
							}
						} else if (SectionIndex >= 0 & SectionIndex < Game.Sections.Length) {
							int a = Game.Sections[SectionIndex].CurrentAspect;
							if (a >= 0 & a < Game.Sections[SectionIndex].Aspects.Length) {
								Function.Stack[s] = (double)Game.Sections[SectionIndex].Aspects[a].Number;
							} else {
								Function.Stack[s] = 0;
							}
						} else {
							Function.Stack[s] = 0;
						}
						s++; break;
						// default
					default:
						throw new System.InvalidOperationException("The unknown instruction " + Function.InstructionSet[i].ToString() + " was encountered in ExecuteFunctionScript.");
				}
			}
			Function.LastResult = Function.Stack[s - 1];
		}
		
		// mathematical functions
		private static double Log(double X) {
			if (X <= 0.0) {
				return 0.0; // ComplexInfinity or NonReal
			} else {
				return Math.Log(X);
			}
		}
		private static double Sqrt(double X) {
			if (X < 0.0) {
				return 0.0; // NonReal
			} else {
				return Math.Sqrt(X);
			}
		}
		private static double Tan(double X) {
			double c = X / Math.PI;
			double d = c - Math.Floor(c) - 0.5;
			double e = Math.Floor(X >= 0.0 ? X : -X) * 1.38462643383279E-16;
			if (d >= -e & d <= e) {
				return 0.0; // ComplexInfinity
			} else {
				return Math.Tan(X);
			}
		}

	}
}
