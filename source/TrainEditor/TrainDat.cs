using System;
using System.Windows.Forms;

namespace TrainEditor {
	internal static class TrainDat {

		// data structures

		// acceleration
		/// <summary>The Acceleration section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Acceleration {
			internal struct Entry {
				internal double a0;
				internal double a1;
				internal double v1;
				internal double v2;
				internal double e;
			}
			internal Entry[] Entries;
			internal Acceleration() {
				const int n = 8;
				this.Entries = new Entry[n];
				for (int i = 0; i < n; i++) {
					this.Entries[i].a0 = 1.0;
					this.Entries[i].a1 = 1.0;
					this.Entries[i].v1 = 25.0;
					this.Entries[i].v2 = 25.0;
					this.Entries[i].e = 1.0;
				}
			}
		}
		
		// performance
		/// <summary>The Performance section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Performance {
			internal double Deceleration;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;
			internal Performance() {
				this.Deceleration = 1.0;
				this.CoefficientOfStaticFriction = 0.35;
				this.CoefficientOfRollingResistance = 0.0025;
				this.AerodynamicDragCoefficient = 1.2;
			}
		}
		
		// delay
		/// <summary>The Delay section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Delay {
			internal double DelayPowerUp;
			internal double DelayPowerDown;
			internal double DelayBrakeUp;
			internal double DelayBrakeDown;
			internal Delay() {
				this.DelayPowerUp = 0.0;
				this.DelayPowerDown = 0.0;
				this.DelayBrakeUp = 0.0;
				this.DelayBrakeDown = 0.0;
			}
		}
		
		// move
		/// <summary>The Move section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Move {
			internal double JerkPowerUp;
			internal double JerkPowerDown;
			internal double JerkBrakeUp;
			internal double JerkBrakeDown;
			internal double BrakeCylinderUp;
			internal double BrakeCylinderDown;
			internal Move() {
				this.JerkPowerUp = 1000.0;
				this.JerkPowerDown = 1000.0;
				this.JerkBrakeUp = 1000.0;
				this.JerkBrakeDown = 1000.0;
				this.BrakeCylinderUp = 300.0;
				this.BrakeCylinderDown = 200.0;
			}
		}
		
		// brake
		/// <summary>The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Brake {
			internal enum BrakeTypes {
				ElectromagneticStraightAirBrake = 0,
				ElectricCommandBrake = 1,
				AutomaticAirBrake = 2
			}
			internal enum BrakeControlSystems {
				None = 0,
				ClosingElectromagneticValve = 1,
				DelayIncludingSystem = 2
			}
			internal BrakeTypes BrakeType;
			internal BrakeControlSystems BrakeControlSystem;
			internal double BrakeControlSpeed;
			internal Brake() {
				this.BrakeType = BrakeTypes.ElectromagneticStraightAirBrake;
				this.BrakeControlSystem = BrakeControlSystems.None;
				this.BrakeControlSpeed = 0.0;
			}
		}
		
		// pressure
		/// <summary>The Pressure section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Pressure {
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double MainReservoirMinimumPressure;
			internal double MainReservoirMaximumPressure;
			internal double BrakePipeNormalPressure;
			internal Pressure() {
				this.BrakeCylinderServiceMaximumPressure = 480.0;
				this.BrakeCylinderEmergencyMaximumPressure = 480.0;
				this.MainReservoirMinimumPressure = 690.0;
				this.MainReservoirMaximumPressure = 780.0;
				this.BrakePipeNormalPressure = 490.0;
			}
		}
		
		// handle
		/// <summary>The Handle section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Handle {
			internal enum HandleTypes {
				Separate = 0,
				Combined = 1
			}
			internal HandleTypes HandleType;
			internal int PowerNotches;
			internal int BrakeNotches;
			internal int PowerNotchReduceSteps;
			internal Handle() {
				this.HandleType = HandleTypes.Separate;
				this.PowerNotches = 8;
				this.BrakeNotches = 8;
				this.PowerNotchReduceSteps = 0;
			}
		}
		
		// cab
		/// <summary>The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Cab {
			internal double X;
			internal double Y;
			internal double Z;
			internal double DriverCar;
			internal Cab() {
				this.X = 0.0;
				this.Y = 0.0;
				this.Z = 0.0;
				this.DriverCar = 0;
			}
		}

		
		// car
		/// <summary>The Car section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Car {
			internal double MotorCarMass;
			internal int NumberOfMotorCars;
			internal double TrailerCarMass;
			internal int NumberOfTrailerCars;
			internal double LengthOfACar;
			internal bool FrontCarIsAMotorCar;
			internal double WidthOfACar;
			internal double HeightOfACar;
			internal double CenterOfGravityHeight;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal Car() {
				this.MotorCarMass = 40.0;
				this.NumberOfMotorCars = 1;
				this.TrailerCarMass = 40.0;
				this.NumberOfTrailerCars = 1;
				this.LengthOfACar = 20.0;
				this.FrontCarIsAMotorCar = false;
				this.WidthOfACar = 2.6;
				this.HeightOfACar = 3.2;
				this.CenterOfGravityHeight = 1.5;
				this.ExposedFrontalArea = 5.0;
				this.UnexposedFrontalArea = 1.6;
			}
		}
		
		// device
		/// <summary>The Device section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Device {
			internal enum AtsModes {
				None = -1,
				AtsSn = 0,
				AtsSnP = 1
			}
			internal enum AtcModes {
				None = 0,
				Manual = 1,
				Automatic = 2
			}
			internal enum ReAdhesionDevices {
				None = -1,
				TypeA = 0,
				TypeB = 1,
				TypeC = 2,
				TypeD = 3
			}
			internal enum PassAlarmModes {
				None = 0,
				Single = 1,
				Looping = 2
			}
			internal enum DoorModes {
				SemiAutomatic = 0,
				Automatic = 1,
				Manual = 2
			}
			internal AtsModes Ats;
			internal AtcModes Atc;
			internal bool Eb;
			internal bool ConstSpeed;
			internal bool HoldBrake;
			internal ReAdhesionDevices ReAdhesionDevice;
			internal double LoadCompensatingDevice;
			internal PassAlarmModes PassAlarm;
			internal DoorModes DoorOpenMode;
			internal DoorModes DoorCloseMode;
			internal Device() {
				this.Ats = AtsModes.AtsSn;
				this.Atc = AtcModes.None;
				this.Eb = false;
				this.ConstSpeed = false;
				this.HoldBrake = false;
				this.ReAdhesionDevice = ReAdhesionDevices.TypeA;
				this.LoadCompensatingDevice = 0.0;
				this.PassAlarm = PassAlarmModes.None;
				this.DoorOpenMode = DoorModes.SemiAutomatic;
				this.DoorCloseMode = DoorModes.SemiAutomatic;
			}
		}
		
		// motor
		/// <summary>Any of the Motor sections of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Motor {
			internal struct Entry {
				internal int SoundIndex;
				internal double Pitch;
				internal double Volume;
			}
			internal Entry[] Entries;
			internal Motor() {
				const int n = 800;
				this.Entries = new Entry[n];
				for (int i = 0; i < n; i++) {
					this.Entries[i].SoundIndex = -1;
					this.Entries[i].Pitch = 100.0;
					this.Entries[i].Volume = 128.0;
				}
			}
		}
		
		// train
		/// <summary>The representation of the train.dat.</summary>
		internal class Train {
			internal Acceleration Acceleration;
			internal Performance Performance;
			internal Delay Delay;
			internal Move Move;
			internal Brake Brake;
			internal Pressure Pressure;
			internal Handle Handle;
			internal Cab Cab;
			internal Car Car;
			internal Device Device;
			internal Motor MotorP1;
			internal Motor MotorP2;
			internal Motor MotorB1;
			internal Motor MotorB2;
			internal Train () {
				this.Acceleration = new Acceleration();
				this.Performance = new Performance();
				this.Delay = new Delay();
				this.Move = new Move();
				this.Brake = new Brake();
				this.Pressure = new Pressure();
				this.Handle = new Handle();
				this.Cab = new Cab();
				this.Car = new Car();
				this.Device = new Device();
				this.MotorP1 = new Motor();
				this.MotorP2 = new Motor();
				this.MotorB1 = new Motor();
				this.MotorB2 = new Motor();
			}
		}

		// load
		/// <summary>Loads a file into an instance of the Train class.</summary>
		/// <param name="FileName">The train.dat file to load.</param>
		/// <returns>An instance of the Train class.</returns>
		internal static Train Load(string FileName) {
			Train t = new Train();
			t.Pressure.BrakePipeNormalPressure = 0.0;
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Lines = System.IO.File.ReadAllLines(FileName, new System.Text.UTF8Encoding());
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
			}
			bool ver1220000 = false;
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length != 0) {
					string s = Lines[i].ToLowerInvariant();
					if (s == "bve1220000") {
						ver1220000 = true;
					} else if (s != "bve2000000" & s != "openbve") {
						MessageBox.Show("The format of the train.dat is not recognized.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					break;
				}
			}
			for (int i = 0; i < Lines.Length; i++) {
				int n = 0;
				switch (Lines[i].ToLowerInvariant()) {
					case "#acceleration":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							if (n == t.Acceleration.Entries.Length) {
								Array.Resize<Acceleration.Entry>(ref t.Acceleration.Entries, t.Acceleration.Entries.Length << 1);
							}
							string u = Lines[i] + ",";
							int m = 0;
							while (true) {
								int j = u.IndexOf(',');
								if (j == -1) break;
								string s = u.Substring(0, j).Trim();
								u = u.Substring(j + 1);
								double a; if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out a)) {
									switch (m) {
										case 0:
											t.Acceleration.Entries[n].a0 = Math.Max(a, 0.0);
											break;
										case 1:
											t.Acceleration.Entries[n].a1 = Math.Max(a, 0.0);
											break;
										case 2:
											t.Acceleration.Entries[n].v1 = Math.Max(a, 0.0);
											break;
										case 3:
											t.Acceleration.Entries[n].v2 = Math.Max(a, 0.0);
											if (t.Acceleration.Entries[n].v2 < t.Acceleration.Entries[n].v1) {
												double x = t.Acceleration.Entries[n].v1;
												t.Acceleration.Entries[n].v1 = t.Acceleration.Entries[n].v2;
												t.Acceleration.Entries[n].v2 = x;
											}
											break;
										case 4:
											if (ver1220000) {
												if (a <= 0.0) {
													t.Acceleration.Entries[n].e = 1.0;
												} else {
													const double c = 1.23315173118822;
													t.Acceleration.Entries[n].e = 1.0 - Math.Log(a) * t.Acceleration.Entries[n].v2 * c;
													if (t.Acceleration.Entries[n].e > 4.0) {
														t.Acceleration.Entries[n].e = 4.0;
													}
												}
											} else {
												t.Acceleration.Entries[n].e = a;
											}
											break;
									}
								} m++;
							}
							i++;
							n++;
						}
						Array.Resize<Acceleration.Entry>(ref t.Acceleration.Entries, n);
						i--;
						break;
					case "#performance":
					case "#deceleration":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if (a >= 0.0) t.Performance.Deceleration = a;
										break;
									case 1:
										if (a >= 0.0) t.Performance.CoefficientOfStaticFriction = a;
										break;
									case 3:
										if (a >= 0.0) t.Performance.CoefficientOfRollingResistance = a;
										break;
									case 4:
										if (a >= 0.0) t.Performance.AerodynamicDragCoefficient = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#delay":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if(a >= 0.0) t.Delay.DelayPowerUp = a;
										break;
									case 1:
										if(a >= 0.0) t.Delay.DelayPowerDown = a;
										break;
									case 2:
										if(a >= 0.0) t.Delay.DelayBrakeUp = a;
										break;
									case 3:
										if(a >= 0.0) t.Delay.DelayBrakeDown = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#move":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if(a >= 0.0) t.Move.JerkPowerUp = a;
										break;
									case 1:
										if(a >= 0.0) t.Move.JerkPowerDown = a;
										break;
									case 2:
										if(a >= 0.0) t.Move.JerkBrakeUp = a;
										break;
									case 3:
										if(a >= 0.0) t.Move.JerkBrakeDown = a;
										break;
									case 4:
										if(a >= 0.0) t.Move.BrakeCylinderUp = a;
										break;
									case 5:
										if(a >= 0.0) t.Move.BrakeCylinderDown = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#brake":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b >= 0 & b <= 2) t.Brake.BrakeType = (Brake.BrakeTypes)b;
										break;
									case 1:
										if (b >= 0 & b <= 2) t.Brake.BrakeControlSystem = (Brake.BrakeControlSystems)b;
										break;
									case 2:
										if (a >= 0.0) t.Brake.BrakeControlSpeed = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#pressure":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if (a > 0.0) t.Pressure.BrakeCylinderServiceMaximumPressure = a;
										break;
									case 1:
										if (a > 0.0) t.Pressure.BrakeCylinderEmergencyMaximumPressure = a;
										break;
									case 2:
										if (a > 0.0) t.Pressure.MainReservoirMinimumPressure = a;
										break;
									case 3:
										if (a > 0.0) t.Pressure.MainReservoirMaximumPressure = a;
										break;
									case 4:
										if (a > 0.0) t.Pressure.BrakePipeNormalPressure = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#handle":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b == 0 | b == 1) t.Handle.HandleType = (Handle.HandleTypes)b;
										break;
									case 1:
										if (b > 0) t.Handle.PowerNotches = b;
										break;
									case 2:
										if (b > 0) t.Handle.BrakeNotches = b;
										break;
									case 3:
										if (b >= 0) t.Handle.PowerNotchReduceSteps = b;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										t.Cab.X = a;
										break;
									case 1:
										t.Cab.Y = a;
										break;
									case 2:
										t.Cab.Z = a;
										break;
									case 3:
										t.Cab.DriverCar = (int)Math.Round(a);
										break;
								}
							} i++; n++;
						} i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (a > 0.0) t.Car.MotorCarMass = a;
										break;
									case 1:
										if (b >= 1) t.Car.NumberOfMotorCars = b;
										break;
									case 2:
										if (a > 0.0) t.Car.TrailerCarMass = a;
										break;
									case 3:
										if (b >= 0) t.Car.NumberOfTrailerCars = b;
										break;
									case 4:
										if (b > 0.0) t.Car.LengthOfACar = a;
										break;
									case 5:
										t.Car.FrontCarIsAMotorCar = a == 1.0;
										break;
									case 6:
										if (a > 0.0) t.Car.WidthOfACar = a;
										break;
									case 7:
										if (a > 0.0) t.Car.HeightOfACar = a;
										break;
									case 8:
										t.Car.CenterOfGravityHeight = a;
										break;
									case 9:
										if (a > 0.0) t.Car.ExposedFrontalArea = a;
										break;
									case 10:
										if (a > 0.0) t.Car.UnexposedFrontalArea = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#device":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b >= -1 & b <= 1) t.Device.Ats = (Device.AtsModes)b;
										break;
									case 1:
										if (b >= 0 & b <= 2) t.Device.Atc = (Device.AtcModes)b;
										break;
									case 2:
										t.Device.Eb = a == 1.0;
										break;
									case 3:
										t.Device.ConstSpeed = a == 1.0;
										break;
									case 4:
										t.Device.HoldBrake = a == 1.0;
										break;
									case 5:
										if (b >= -1 & b <= 3) t.Device.ReAdhesionDevice = (Device.ReAdhesionDevices)b;
										break;
									case 6:
										t.Device.LoadCompensatingDevice = a;
										break;
									case 7:
										if (b >= 0 & b <= 2) t.Device.PassAlarm = (Device.PassAlarmModes)b;
										break;
									case 8:
										if (b >= 0 & b <= 2) t.Device.DoorOpenMode = (Device.DoorModes)b;
										break;
									case 9:
										if (b >= 0 & b <= 2) t.Device.DoorCloseMode = (Device.DoorModes)b;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#motor_p1":
					case "#motor_p2":
					case "#motor_b1":
					case "#motor_b2":
						{
							string section = Lines[i].ToLowerInvariant();
							i++;
							Motor m = new Motor();
							while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
								if (n == m.Entries.Length) {
									Array.Resize<Motor.Entry>(ref m.Entries, m.Entries.Length << 1);
								}
								string u = Lines[i] + ",";
								int k = 0;
								while (true) {
									int j = u.IndexOf(',');
									if (j == -1) break;
									string s = u.Substring(0, j).Trim();
									u = u.Substring(j + 1);
									double a; if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out a)) {
										int b = (int)Math.Round(a);
										switch (k) {
											case 0:
												m.Entries[n].SoundIndex = b >= 0 ? b : -1;
												break;
											case 1:
												m.Entries[n].Pitch = Math.Max(a, 0.0);
												break;
											case 2:
												m.Entries[n].Volume = Math.Max(a, 0.0);
												break;
										}
									} k++;
								}
								i++;
								n++;
							}
							Array.Resize<Motor.Entry>(ref m.Entries, n);
							i--;
							switch (section) {
								case "#motor_p1":
									t.MotorP1 = m;
									break;
								case "#motor_p2":
									t.MotorP2 = m;
									break;
								case "#motor_b1":
									t.MotorB1 = m;
									break;
								case "#motor_b2":
									t.MotorB2 = m;
									break;
							}
						}
						break;
				}
			}
			if (t.Pressure.BrakePipeNormalPressure <= 0.0) {
				if (t.Brake.BrakeType == Brake.BrakeTypes.AutomaticAirBrake) {
					t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);
					if (t.Pressure.BrakePipeNormalPressure > t.Pressure.MainReservoirMinimumPressure) {
						t.Pressure.BrakePipeNormalPressure = t.Pressure.MainReservoirMinimumPressure;
					}
				} else {
					if (t.Pressure.BrakeCylinderEmergencyMaximumPressure < 480000.0 & t.Pressure.MainReservoirMinimumPressure > 500000.0) {
						t.Pressure.BrakePipeNormalPressure = 490000.0;
					} else {
						t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);
					}
				}
			}
			if (t.Brake.BrakeType == Brake.BrakeTypes.AutomaticAirBrake) {
				t.Device.HoldBrake = false;
			}
			if (t.Device.HoldBrake & t.Handle.BrakeNotches <= 0) {
				t.Handle.BrakeNotches = 1;
			}
			if (t.Cab.DriverCar < 0 | t.Cab.DriverCar >= t.Car.NumberOfMotorCars + t.Car.NumberOfTrailerCars) {
				t.Cab.DriverCar = 0;
			}
			return t;
		}
		
		// save
		/// <summary>Saves an instance of the Train class into a specified file.</summary>
		/// <param name="FileName">The train.dat file to save.</param>
		/// <param name="t">An instance of the Train class to save.</param>
		internal static void Save(string FileName, Train t) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.StringBuilder b = new System.Text.StringBuilder();
			b.AppendLine("OPENBVE");
			b.AppendLine("#ACCELERATION");
			if (t.Acceleration.Entries.Length > t.Handle.PowerNotches) {
				Array.Resize<Acceleration.Entry>(ref t.Acceleration.Entries, t.Handle.PowerNotches);
			}
			for (int i = 0; i < t.Acceleration.Entries.Length; i++) {
				b.Append(t.Acceleration.Entries[i].a0.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].a1.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].v1.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].v2.ToString(Culture) + ",");
				b.AppendLine(t.Acceleration.Entries[i].e.ToString(Culture));
			}
			int n = 15;
			b.AppendLine("#PERFORMANCE");
			b.AppendLine(t.Performance.Deceleration.ToString(Culture).PadRight(n, ' ') + "; Deceleration");
			b.AppendLine(t.Performance.CoefficientOfStaticFriction.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfStaticFriction");
			b.AppendLine("0".PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(t.Performance.CoefficientOfRollingResistance.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfRollingResistance");
			b.AppendLine(t.Performance.AerodynamicDragCoefficient.ToString(Culture).PadRight(n, ' ') + "; AerodynamicDragCoefficient");
			b.AppendLine("#DELAY");
			b.AppendLine(t.Delay.DelayPowerUp.ToString(Culture).PadRight(n, ' ') + "; DelayPowerUp");
			b.AppendLine(t.Delay.DelayPowerDown.ToString(Culture).PadRight(n, ' ') + "; DelayPowerDown");
			b.AppendLine(t.Delay.DelayBrakeUp.ToString(Culture).PadRight(n, ' ') + "; DelayBrakeUp");
			b.AppendLine(t.Delay.DelayBrakeDown.ToString(Culture).PadRight(n, ' ') + "; DelayBrakeDown");
			b.AppendLine("#MOVE");
			b.AppendLine(t.Move.JerkPowerUp.ToString(Culture).PadRight(n, ' ') + "; JerkPowerUp");
			b.AppendLine(t.Move.JerkPowerDown.ToString(Culture).PadRight(n, ' ') + "; JerkPowerDown");
			b.AppendLine(t.Move.JerkBrakeUp.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeUp");
			b.AppendLine(t.Move.JerkBrakeDown.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeDown");
			b.AppendLine(t.Move.BrakeCylinderUp.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderUp");
			b.AppendLine(t.Move.BrakeCylinderDown.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderDown");
			b.AppendLine("#BRAKE");
			b.AppendLine(((int)t.Brake.BrakeType).ToString(Culture).PadRight(n, ' ') + "; BrakeType");
			b.AppendLine(((int)t.Brake.BrakeControlSystem).ToString(Culture).PadRight(n, ' ') + "; BrakeControlSystem");
			b.AppendLine(t.Brake.BrakeControlSpeed.ToString(Culture).PadRight(n, ' ') + "; BrakeControlSpeed");
			b.AppendLine("#PRESSURE");
			b.AppendLine(t.Pressure.BrakeCylinderServiceMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderServiceMaximumPressure");
			b.AppendLine(t.Pressure.BrakeCylinderEmergencyMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderEmergencyMaximumPressure");
			b.AppendLine(t.Pressure.MainReservoirMinimumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMinimumPressure");
			b.AppendLine(t.Pressure.MainReservoirMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMaximumPressure");
			b.AppendLine(t.Pressure.BrakePipeNormalPressure.ToString(Culture).PadRight(n, ' ') + "; BrakePipeNormalPressure");
			b.AppendLine("#HANDLE");
			b.AppendLine(((int)t.Handle.HandleType).ToString(Culture).PadRight(n, ' ') + "; HandleType");
			b.AppendLine(t.Handle.PowerNotches.ToString(Culture).PadRight(n, ' ') + "; PowerNotches");
			b.AppendLine(t.Handle.BrakeNotches.ToString(Culture).PadRight(n, ' ') + "; BrakeNotches");
			b.AppendLine(t.Handle.PowerNotchReduceSteps.ToString(Culture).PadRight(n, ' ') + "; PowerNotchReduceSteps");
			b.AppendLine("#CAB");
			b.AppendLine(t.Cab.X.ToString(Culture).PadRight(n, ' ') + "; X");
			b.AppendLine(t.Cab.Y.ToString(Culture).PadRight(n, ' ') + "; Y");
			b.AppendLine(t.Cab.Z.ToString(Culture).PadRight(n, ' ') + "; Z");
			b.AppendLine(t.Cab.DriverCar.ToString(Culture).PadRight(n, ' ') + "; DriverCar");
			b.AppendLine("#CAR");
			b.AppendLine(t.Car.MotorCarMass.ToString(Culture).PadRight(n, ' ') + "; MotorCarMass");
			b.AppendLine(t.Car.NumberOfMotorCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfMotorCars");
			b.AppendLine(t.Car.TrailerCarMass.ToString(Culture).PadRight(n, ' ') + "; TrailerCarMass");
			b.AppendLine(t.Car.NumberOfTrailerCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfTrailerCars");
			b.AppendLine(t.Car.LengthOfACar.ToString(Culture).PadRight(n, ' ') + "; LengthOfACar");
			b.AppendLine((t.Car.FrontCarIsAMotorCar ? "1" : "0").PadRight(n, ' ') + "; FrontCarIsAMotorCar");
			b.AppendLine(t.Car.WidthOfACar.ToString(Culture).PadRight(n, ' ') + "; WidthOfACar");
			b.AppendLine(t.Car.HeightOfACar.ToString(Culture).PadRight(n, ' ') + "; HeightOfACar");
			b.AppendLine(t.Car.CenterOfGravityHeight.ToString(Culture).PadRight(n, ' ') + "; CenterOfGravityHeight");
			b.AppendLine(t.Car.ExposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; ExposedFrontalArea");
			b.AppendLine(t.Car.UnexposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; UnexposedFrontalArea");
			b.AppendLine("#DEVICE");
			b.AppendLine(((int)t.Device.Ats).ToString(Culture).PadRight(n, ' ') + "; Ats");
			b.AppendLine(((int)t.Device.Atc).ToString(Culture).PadRight(n, ' ') + "; Atc");
			b.AppendLine((t.Device.Eb ? "1" : "0").PadRight(n, ' ') + "; Eb");
			b.AppendLine((t.Device.ConstSpeed ? "1" : "0").PadRight(n, ' ') + "; ConstSpeed");
			b.AppendLine((t.Device.HoldBrake ? "1" : "0").PadRight(n, ' ') + "; HoldBrake");
			b.AppendLine(((int)t.Device.ReAdhesionDevice).ToString(Culture).PadRight(n, ' ') + "; ReAdhesionDevice");
			b.AppendLine(t.Device.LoadCompensatingDevice.ToString(Culture).PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(((int)t.Device.PassAlarm).ToString(Culture).PadRight(n, ' ') + "; PassAlarm");
			b.AppendLine(((int)t.Device.DoorOpenMode).ToString(Culture).PadRight(n, ' ') + "; DoorOpenMode");
			b.AppendLine(((int)t.Device.DoorCloseMode).ToString(Culture).PadRight(n, ' ') + "; DoorCloseMode");
			for (int i = 0; i < 4; i++) {
				Motor m = null;
				switch (i) {
					case 0:
						b.AppendLine("#MOTOR_P1");
						m = t.MotorP1;
						break;
					case 1:
						b.AppendLine("#MOTOR_P2");
						m = t.MotorP2;
						break;
					case 2:
						b.AppendLine("#MOTOR_B1");
						m = t.MotorB1;
						break;
					case 3:
						b.AppendLine("#MOTOR_B2");
						m = t.MotorB2;
						break;
				}
				int k;
				for (k = m.Entries.Length - 1; k >= 0; k--) {
					if (m.Entries[k].SoundIndex >= 0) break;
				}
				k = Math.Min(k + 2, m.Entries.Length);
				Array.Resize<Motor.Entry>(ref m.Entries, k);
				for (int j = 0; j < m.Entries.Length; j++) {
					b.Append(m.Entries[j].SoundIndex.ToString(Culture) + ",");
					b.Append(m.Entries[j].Pitch.ToString(Culture) + ",");
					b.AppendLine(m.Entries[j].Volume.ToString(Culture));
				}
			}
			System.IO.File.WriteAllText(FileName, b.ToString(), new System.Text.UTF8Encoding(true));
		}

	}
}