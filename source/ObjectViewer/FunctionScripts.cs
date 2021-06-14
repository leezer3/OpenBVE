using System;
using LibRender2.Overlays;
using ObjectViewer.Trains;
using TrainManager.BrakeSystems;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using TrainManager.Handles;
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
						s++;
						break;
						// time/camera
					case Instructions.TimeSecondsSinceMidnight:
						Function.Stack[s] = Game.SecondsSinceMidnight;
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
					case Instructions.CameraView:
						//Returns whether the camera is in interior or exterior mode
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior)
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
						if (Train != null)
						{
							Function.Stack[s] = Train.IsPlayerTrain ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
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
								if (Train.Cars[j].Specs.IsMotorCar) {
									// hack: MotorAcceleration does not distinguish between forward/backward
									if (Train.Cars[j].Specs.MotorAcceleration < 0.0) {
										Function.Stack[s] = Train.Cars[j].Specs.MotorAcceleration * (double)Math.Sign(Train.Cars[j].CurrentSpeed);
									} else if (Train.Cars[j].Specs.MotorAcceleration > 0.0) {
										Function.Stack[s] = Train.Cars[j].Specs.MotorAcceleration * (double)Train.Handles.Reverser.Actual;
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
								if (Train.Cars[j].Specs.MotorAcceleration < 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].Specs.MotorAcceleration * (double)Math.Sign(Train.Cars[j].CurrentSpeed);
								} else if (Train.Cars[j].Specs.MotorAcceleration > 0.0) {
									Function.Stack[s - 1] = Train.Cars[j].Specs.MotorAcceleration * (double)Train.Handles.Reverser.Actual;
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
							double t0 = Train.FrontCarTrackPosition();
							double t1 = Train.RearCarTrackPosition();
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
					case Instructions.PilotLamp:
						//Not currently supported in viewers
						Function.Stack[s] = 0.0;
						s++; break;
					case Instructions.PassAlarm:
						//Not currently supported in viewers
						Function.Stack[s] = 0.0;
						s++; break;
					case Instructions.StationAdjustAlarm:
						//Not currently supported in viewers
						Function.Stack[s] = 0.0;
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
							Function.Stack[s] = (double)Train.Handles.Power.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.PowerNotches:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Handles.Power.MaximumNotch;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotch:
						if (Train != null) {
							Function.Stack[s] = (double)Train.Handles.Brake.Driver;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.BrakeNotches:
						if (Train != null) {
							if (Train.Handles.Brake is AirBrakeHandle) {
								Function.Stack[s] = 2.0;
							} else {
								Function.Stack[s] = (double)Train.Handles.Brake.MaximumNotch;
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
									Function.Stack[s] = (double)Train.Handles.Brake.Driver;
								}
							} else if (Train.Handles.HasHoldBrake) {
								if (Train.Handles.EmergencyBrake.Driver) {
									Function.Stack[s] = (double)Train.Handles.Brake.MaximumNotch + 2.0;
								} else if (Train.Handles.Brake.Driver > 0) {
									Function.Stack[s] = (double)Train.Handles.Brake.Driver + 1.0;
								} else {
									Function.Stack[s] = Train.Handles.HoldBrake.Driver ? 1.0 : 0.0;
								}
							} else {
								if (Train.Handles.EmergencyBrake.Driver) {
									Function.Stack[s] = (double)Train.Handles.Brake.MaximumNotch + 1.0;
								} else {
									Function.Stack[s] = (double)Train.Handles.Brake.Driver;
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
					case Instructions.PrimaryKlaxon:
					case Instructions.Klaxon:
					case Instructions.SecondaryKlaxon:
					case Instructions.MusicKlaxon:
						//Object Viewer doesn't actually have a sound player, so we can't test against it, thus return zero....
						Function.Stack[s] = 0.0;
						s++; break;
					case Instructions.HasAirBrake:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[Train.DriverCar].CarBrake is AutomaticAirBrake ? 1.0 : 0.0;
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
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.mainReservoir.CurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.mainReservoir.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeEqualizingReservoir:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.equalizingReservoir.CurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.equalizingReservoir.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakePipe:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.brakePipe.CurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.brakePipe.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeBrakeCylinder:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.brakeCylinder.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
					case Instructions.BrakeStraightAirPipe:
						if (Train != null) {
							Function.Stack[s] = Train.Cars[CarIndex].CarBrake.straightAirPipe.CurrentPressure;
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
								Function.Stack[s - 1] = Train.Cars[j].CarBrake.straightAirPipe.CurrentPressure;
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
						// safety
					case Instructions.SafetyPluginAvailable:
						if (Train != null && Train.IsPlayerTrain) {
							Function.Stack[s] = NearestTrain.EnablePluginSimulation ? 1.0 : 0.0;
						} else {
							Function.Stack[s] = 0.0;
						}
						s++; break;
					case Instructions.SafetyPluginState:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int n = (int)Math.Round(Function.Stack[s - 1]);
							if (NearestTrain.EnablePluginSimulation) {
								if (n >= 0 & n < PluginManager.CurrentPlugin.Panel.Length) {
									Function.Stack[s - 1] = (double)PluginManager.CurrentPlugin.Panel[n];
								} else {
									Function.Stack[s - 1] = 0.0;
								}
							} else {
								Function.Stack[s - 1] = 0.0;
							}
						}
						break;
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
					case Instructions.DistanceNextStation:
					case Instructions.StopsNextStation:
					case Instructions.NextStation:
					case Instructions.NextStationStop:
					case Instructions.RouteLimit:
					case Instructions.TerminalStation:
						Function.Stack[s] = 0.0; //Unsupported in viewers
						s++; break;
					case Instructions.DistanceStation:
					case Instructions.StopsStation:
						Function.Stack[s - 1] = 0.0; //Unsupported in viewers
						break;
						// sections
					case Instructions.SectionAspectNumber:
						if (IsPartOfTrain) {
							int nextSectionIndex = Train.CurrentSectionIndex + 1;
							if (nextSectionIndex >= 0 & nextSectionIndex < Program.CurrentRoute.Sections.Length) {
								int a = Program.CurrentRoute.Sections[nextSectionIndex].CurrentAspect;
								if (a >= 0 & a < Program.CurrentRoute.Sections[nextSectionIndex].Aspects.Length) {
									Function.Stack[s] = (double)Program.CurrentRoute.Sections[nextSectionIndex].Aspects[a].Number;
								} else {
									Function.Stack[s] = 0;
								}
							}
						} else if (SectionIndex >= 0 & SectionIndex < Program.CurrentRoute.Sections.Length) {
							int a = Program.CurrentRoute.Sections[SectionIndex].CurrentAspect;
							if (a >= 0 & a < Program.CurrentRoute.Sections[SectionIndex].Aspects.Length) {
								Function.Stack[s] = (double)Program.CurrentRoute.Sections[SectionIndex].Aspects[a].Number;
							} else {
								Function.Stack[s] = 0;
							}
						} else {
							Function.Stack[s] = 0;
						}
						s++; break;
					case Instructions.Panel2Timetable:
						throw new InvalidOperationException("The instruction " + Function.InstructionSet[i].ToString() + " is for internal use only, and should not be added to objects.");
					case Instructions.BrightnessOfCar:
						if (Train == null) {
							Function.Stack[s - 1] = 0.0;
						} else {
							int j = (int)Math.Round(Function.Stack[s - 1]);
							if (j < 0) j += Train.Cars.Length;
							if (j >= 0 & j < Train.Cars.Length) {
								Function.Stack[s - 1] = Train.Cars[j].Brightness.CurrentBrightness(Program.Renderer.Lighting.DynamicCabBrightness);
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
					default:
						throw new InvalidOperationException("The unknown instruction " + Function.InstructionSet[i].ToString() + " was encountered in ExecuteFunctionScript.");
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
