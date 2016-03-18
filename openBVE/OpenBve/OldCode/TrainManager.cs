using System;
using System.Windows.Forms;
using OpenBveApi.Math;

namespace OpenBve {
	public static class TrainManager {
		
		// axle
		internal struct Axle {
			internal TrackManager.TrackFollower Follower;
			internal bool CurrentWheelSlip;
		}

		// coupler
		internal struct Coupler {
			internal double MinimumDistanceBetweenCars;
			internal double MaximumDistanceBetweenCars;
		}

		// sections
		internal struct CarSection {
			internal ObjectManager.AnimatedObject[] Elements;
			internal bool Overlay;
		}

		// cars
		internal struct Door {
			/// <summary>A value of -1 (left) or 1 (right).</summary>
			internal int Direction;
			/// <summary>A value between 0 (closed) and 1 (opened).</summary>
			internal double State;
			/// <summary>The value of the state at which a door lock simulation is scheduled.</summary>
			internal double DoorLockState;
			/// <summary>The duration of the scheduled door lock simulation.</summary>
			internal double DoorLockDuration;
		}
		internal struct AccelerationCurve {
			internal double StageZeroAcceleration;
			internal double StageOneSpeed;
			internal double StageOneAcceleration;
			internal double StageTwoSpeed;
			internal double StageTwoExponent;
		}
		internal enum CarBrakeType {
			ElectromagneticStraightAirBrake = 0,
			ElectricCommandBrake = 1,
			AutomaticAirBrake = 2
		}
		internal enum EletropneumaticBrakeType {
			None = 0,
			ClosingElectromagneticValve = 1,
			DelayFillingControl = 2
		}
		internal enum AirBrakeHandleState {
			Invalid = -1,
			Release = 0,
			Lap = 1,
			Service = 2,
		}
		internal struct AirBrakeHandle {
			internal AirBrakeHandleState Driver;
			internal AirBrakeHandleState Safety;
			internal AirBrakeHandleState Actual;
			internal AirBrakeHandleState DelayedValue;
			internal double DelayedTime;
		}
		internal enum AirBrakeType { Main, Auxillary }
		internal struct CarAirBrake {
			internal AirBrakeType Type;
			internal bool AirCompressorEnabled;
			internal double AirCompressorMinimumPressure;
			internal double AirCompressorMaximumPressure;
			internal double AirCompressorRate;
			internal double MainReservoirCurrentPressure;
			internal double MainReservoirEqualizingReservoirCoefficient;
			internal double MainReservoirBrakePipeCoefficient;
			internal double EqualizingReservoirCurrentPressure;
			internal double EqualizingReservoirNormalPressure;
			internal double EqualizingReservoirServiceRate;
			internal double EqualizingReservoirEmergencyRate;
			internal double EqualizingReservoirChargeRate;
			internal double BrakePipeCurrentPressure;
			internal double BrakePipeNormalPressure;
			internal double BrakePipeFlowSpeed;
			internal double BrakePipeChargeRate;
			internal double BrakePipeServiceRate;
			internal double BrakePipeEmergencyRate;
			internal double AuxillaryReservoirCurrentPressure;
			internal double AuxillaryReservoirMaximumPressure;
			internal double AuxillaryReservoirChargeRate;
			internal double AuxillaryReservoirBrakePipeCoefficient;
			internal double AuxillaryReservoirBrakeCylinderCoefficient;
			internal double BrakeCylinderCurrentPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyChargeRate;
			internal double BrakeCylinderServiceChargeRate;
			internal double BrakeCylinderReleaseRate;
			internal double BrakeCylinderSoundPlayedForPressure;
			internal double StraightAirPipeCurrentPressure;
			internal double StraightAirPipeReleaseRate;
			internal double StraightAirPipeServiceRate;
			internal double StraightAirPipeEmergencyRate;
		}
		internal struct CarHoldBrake {
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double UpdateInterval;
		}
		internal struct CarConstSpeed {
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double UpdateInterval;
		}
		internal struct CarReAdhesionDevice {
			internal double UpdateInterval;
			internal double MaximumAccelerationOutput;
			internal double ApplicationFactor;
			internal double ReleaseInterval;
			internal double ReleaseFactor;
			internal double NextUpdateTime;
			internal double TimeStable;
		}
		internal struct CarSpecs {
			/// motor
			internal bool IsMotorCar;
			internal AccelerationCurve[] AccelerationCurves;
			internal double AccelerationCurvesMultiplier;
			internal double AccelerationCurveMaximum;
			internal double JerkPowerUp;
			internal double JerkPowerDown;
			internal double JerkBrakeUp;
			internal double JerkBrakeDown;
			/// brake
			internal double BrakeDecelerationAtServiceMaximumPressure;
			internal double BrakeControlSpeed;
			internal double MotorDeceleration;
			/// physical properties
			internal double MassEmpty;
			internal double MassCurrent;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;
			internal double CenterOfGravityHeight;
			internal double CriticalTopplingAngle;
			/// current data
			internal double CurrentSpeed;
			internal double CurrentPerceivedSpeed;
			internal double CurrentPerceivedTraveledDistance;
			internal double CurrentAcceleration;
			/// <summary>The acceleration generated by the motor. Is positive for power and negative for brake, regardless of the train's direction.</summary>
			internal double CurrentAccelerationOutput;
			internal double CurrentRollDueToTopplingAngle;
			internal double CurrentRollDueToCantAngle;
			internal double CurrentRollDueToShakingAngle;
			internal double CurrentRollDueToShakingAngularSpeed;
			internal double CurrentRollShakeDirection;
			internal double CurrentPitchDueToAccelerationAngle;
			internal double CurrentPitchDueToAccelerationAngularSpeed;
			internal double CurrentPitchDueToAccelerationTargetAngle;
			internal double CurrentPitchDueToAccelerationFastValue;
			internal double CurrentPitchDueToAccelerationMediumValue;
			internal double CurrentPitchDueToAccelerationSlowValue;
			/// systems
			internal CarHoldBrake HoldBrake;
			internal CarConstSpeed ConstSpeed;
			internal CarReAdhesionDevice ReAdhesionDevice;
			internal CarBrakeType BrakeType;
			internal EletropneumaticBrakeType ElectropneumaticType;
			internal CarAirBrake AirBrake;
			/// doors
			internal Door[] Doors;
			internal double DoorOpenFrequency;
			internal double DoorCloseFrequency;
			internal double DoorOpenPitch;
			internal double DoorClosePitch;
			internal bool AnticipatedLeftDoorsOpened;
			internal bool AnticipatedRightDoorsOpened;
		}
		internal struct CarBrightness {
			internal float PreviousBrightness;
			internal double PreviousTrackPosition;
			internal float NextBrightness;
			internal double NextTrackPosition;
		}
		internal struct Horn {
			internal CarSound Sound;
			internal bool Loop;
			private Horn(CarSound sound, bool loop) {
				this.Sound = sound;
				this.Loop = loop;
			}
			internal static readonly Horn Empty = new Horn(CarSound.Empty, false);
		}
		internal struct CarSound {
			internal Sounds.SoundBuffer Buffer;
			internal Sounds.SoundSource Source;
			internal Vector3 Position;
			private CarSound(Sounds.SoundBuffer buffer, Sounds.SoundSource source, Vector3 position) {
				this.Buffer = buffer;
				this.Source = source;
				this.Position = position;
			}
			internal static readonly CarSound Empty = new CarSound(null, null, new Vector3(0.0, 0.0, 0.0));
		}
		internal struct MotorSoundTableEntry {
			internal Sounds.SoundBuffer Buffer;
			internal int SoundIndex;
			internal float Pitch;
			internal float Gain;
		}
		internal struct MotorSoundTable {
			internal MotorSoundTableEntry[] Entries;
			internal Sounds.SoundBuffer Buffer;
			internal Sounds.SoundSource Source;
		}
		internal struct MotorSound {
			internal MotorSoundTable[] Tables;
			internal Vector3 Position;
			internal double SpeedConversionFactor;
			internal int CurrentAccelerationDirection;
			internal const int MotorP1 = 0;
			internal const int MotorP2 = 1;
			internal const int MotorB1 = 2;
			internal const int MotorB2 = 3;
		}
		internal struct CarSounds {
			internal MotorSound Motor;
			internal CarSound Adjust;
			internal CarSound Air;
			internal CarSound AirHigh;
			internal CarSound AirZero;
			internal CarSound Brake;
			internal CarSound BrakeHandleApply;
			internal CarSound BrakeHandleRelease;
			internal CarSound BrakeHandleMin;
			internal CarSound BrakeHandleMax;
			internal CarSound BreakerResume;
			internal CarSound BreakerResumeOrInterrupt;
			internal bool BreakerResumed;
			internal CarSound CpEnd;
			internal CarSound CpLoop;
			internal bool CpLoopStarted;
			internal CarSound CpStart;
			internal double CpStartTimeStarted;
			internal CarSound DoorCloseL;
			internal CarSound DoorCloseR;
			internal CarSound DoorOpenL;
			internal CarSound DoorOpenR;
			internal CarSound EmrBrake;
			internal CarSound[] Flange;
			internal double[] FlangeVolume;
			internal CarSound Halt;
			internal Horn[] Horns;
			internal CarSound Loop;
			internal CarSound MasterControllerUp;
			internal CarSound MasterControllerDown;
			internal CarSound MasterControllerMin;
			internal CarSound MasterControllerMax;
			internal CarSound PilotLampOn;
			internal CarSound PilotLampOff;
			internal CarSound PointFrontAxle;
			internal CarSound PointRearAxle;
			internal CarSound Rub;
			internal CarSound ReverserOn;
			internal CarSound ReverserOff;
			internal CarSound[] Run;
			internal double[] RunVolume;
			internal double RunNextReasynchronizationPosition;
			internal CarSound SpringL;
			internal CarSound SpringR;
			internal CarSound[] Plugin;
			internal int FrontAxleRunIndex;
			internal int RearAxleRunIndex;
			internal int FrontAxleFlangeIndex;
			internal int RearAxleFlangeIndex;
			internal double FlangePitch;
			internal double SpringPlayedAngle;
		}
		internal struct Car {
			internal double Width;
			internal double Height;
			internal double Length;
			internal Axle FrontAxle;
			internal Bogie FrontBogie;
			internal Axle RearAxle;
			internal Bogie RearBogie;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			internal Vector3 Up;
			internal CarSection[] CarSections;
			internal int CurrentCarSection;
			internal double DriverX;
			internal double DriverY;
			internal double DriverZ;
			internal double DriverYaw;
			internal double DriverPitch;
			internal CarSpecs Specs;
			internal CarSounds Sounds;
			internal bool CurrentlyVisible;
			internal bool Derailed;
			internal bool Topples;
			internal CarBrightness Brightness;
			internal double BeaconReceiverPosition;
			internal TrackManager.TrackFollower BeaconReceiver;
		}
		//Using a much cut-down version of the car struct for bogies
		internal struct Bogie {
			internal double Width;
			internal double Height;
			internal double Length;
			internal Axle FrontAxle;
			internal Axle RearAxle;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			internal Vector3 Up;
			internal CarSection[] CarSections;
			internal int CurrentCarSection;
			internal bool CurrentlyVisible;
		}

		// train
		internal struct HandleChange {
			internal int Value;
			internal double Time;
		}
		internal struct PowerHandle {
			internal int Driver;
			internal int Safety;
			internal int Actual;
			internal HandleChange[] DelayedChanges;
			internal void AddChange(Train Train, int Value, double Delay) {
				int n = DelayedChanges.Length;
				Array.Resize<HandleChange>(ref DelayedChanges, n + 1);
				DelayedChanges[n].Value = Value;
				DelayedChanges[n].Time = Game.SecondsSinceMidnight + Delay;
			}
			internal void RemoveChanges(int Count) {
				int n = DelayedChanges.Length;
				for (int i = 0; i < n - Count; i++) {
					DelayedChanges[i] = DelayedChanges[i + Count];
				}
				Array.Resize<HandleChange>(ref DelayedChanges, n - Count);
			}
		}
		internal struct BrakeHandle {
			internal int Driver;
			internal int Safety;
			internal int Actual;
			internal HandleChange[] DelayedChanges;
			internal void AddChange(Train Train, int Value, double Delay) {
				int n = DelayedChanges.Length;
				Array.Resize<HandleChange>(ref DelayedChanges, n + 1);
				DelayedChanges[n].Value = Value;
				DelayedChanges[n].Time = Game.SecondsSinceMidnight + Delay;
			}
			internal void RemoveChanges(int Count) {
				int n = DelayedChanges.Length;
				for (int i = 0; i < n - Count; i++) {
					DelayedChanges[i] = DelayedChanges[i + Count];
				}
				Array.Resize<HandleChange>(ref DelayedChanges, n - Count);
			}
		}
		internal struct EmergencyHandle {
			internal bool Driver;
			internal bool Safety;
			internal bool Actual;
			internal double ApplicationTime;
		}
		internal struct ReverserHandle {
			internal int Driver;
			internal int Actual;
		}
		internal struct HoldBrakeHandle {
			internal bool Driver;
			internal bool Actual;
		}
		// train specs
		internal enum PassAlarmType {
			None = 0,
			Single = 1,
			Loop = 2
		}
		internal struct TrainAirBrake {
			internal AirBrakeHandle Handle;
		}
		internal enum DoorMode {
			AutomaticManualOverride = 0,
			Automatic = 1,
			Manual = 2
		}

		[Flags]
		internal enum DefaultSafetySystems {
			AtsSn = 1,
			AtsP = 2,
			Atc = 4,
			Eb = 8
		}
		internal struct TrainSpecs {
			internal double TotalMass;
			internal ReverserHandle CurrentReverser;
			internal double CurrentAverageSpeed;
			internal double CurrentAverageAcceleration;
			internal double CurrentAverageJerk;
			internal double CurrentAirPressure;
			internal double CurrentAirDensity;
			internal double CurrentAirTemperature;
			internal double CurrentElevation;
			internal bool SingleHandle;
			internal int PowerNotchReduceSteps;
			internal int MaximumPowerNotch;
			internal PowerHandle CurrentPowerNotch;
			internal int MaximumBrakeNotch;
			internal BrakeHandle CurrentBrakeNotch;
			internal EmergencyHandle CurrentEmergencyBrake;
			internal bool HasHoldBrake;
			internal HoldBrakeHandle CurrentHoldBrake;
			internal DefaultSafetySystems DefaultSafetySystems;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			internal TrainAirBrake AirBrake;
			internal double DelayPowerUp;
			internal double DelayPowerDown;
			internal double DelayBrakeUp;
			internal double DelayBrakeDown;
			internal PassAlarmType PassAlarm;
			internal DoorMode DoorOpenMode;
			internal DoorMode DoorCloseMode;
		}
		// passengers
		internal struct TrainPassengers {
			internal double PassengerRatio;
			internal double CurrentAcceleration;
			internal double CurrentSpeedDifference;
			internal bool FallenOver;
		}
		// train
		internal enum TrainState {
			Pending = 0, Available = 1, Disposed = 2, Bogus = 3
		}
		internal enum TrainStopState {
			Pending = 0, Boarding = 1, Completed = 2
		}
		public class Train {
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;
			internal int TrainIndex;
			internal TrainState State;
			internal Car[] Cars;
			internal Coupler[] Couplers;
			internal int DriverCar;
			internal TrainSpecs Specs;
			internal TrainPassengers Passengers;
			internal int LastStation;
			internal int Station;
			internal bool StationFrontCar;
			internal bool StationRearCar;
			internal TrainStopState StationState;
			internal double StationArrivalTime;
			internal double StationDepartureTime;
			internal bool StationDepartureSoundPlayed;
			internal bool StationAdjust;
			internal double StationDistanceToStopPoint;
			internal double[] RouteLimits;
			internal double CurrentRouteLimit;
			internal double CurrentSectionLimit;
			internal int CurrentSectionIndex;
			internal double TimetableDelta;
			internal Game.GeneralAI AI;
			internal double InternalTimerTimeElapsed;
		}

		// trains
		/// <summary>The list of trains available in the simulation.</summary>
		internal static Train[] Trains = new Train[] { };
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		internal static Train PlayerTrain = null;

		// ================================

		// parse panel config
		internal static void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train)
		{
			string File = OpenBveApi.Path.CombineFile(TrainPath, "panel.animated");
			if (System.IO.File.Exists(File))
			{
				ObjectManager.AnimatedObjectCollection a = AnimatedObjectParser.ReadObject(File, Encoding, ObjectManager.ObjectLoadMode.DontAllowUnloadOfTextures);
				try
				{
					for (int i = 0; i < a.Objects.Length; i++)
					{
						a.Objects[i].ObjectIndex = ObjectManager.CreateDynamicObject();
					}
					Train.Cars[Train.DriverCar].CarSections[0].Elements = a.Objects;
					World.CameraRestriction = World.CameraRestrictionMode.NotAvailable;
				}
				catch
				{
					var currentError = Interface.GetInterfaceString("error_critical_file");
					currentError = currentError.Replace("[file]", "panel.animated");
					MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Program.RestartArguments = " ";
					Loading.Cancel = true;
				}
			}
			else
			{
				var Panel2 = false;
				try
				{
					File = OpenBveApi.Path.CombineFile(TrainPath, "panel2.cfg");
					if (System.IO.File.Exists(File))
					{
						Panel2 = true;
						Panel2CfgParser.ParsePanel2Config(TrainPath, Encoding, Train);
						World.CameraRestriction = World.CameraRestrictionMode.On;
					}
					else
					{
						File = OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg");
						if (System.IO.File.Exists(File))
						{
							PanelCfgParser.ParsePanelConfig(TrainPath, Encoding, Train);
							World.CameraRestriction = World.CameraRestrictionMode.On;
						}
						else
						{
							World.CameraRestriction = World.CameraRestrictionMode.NotAvailable;
						}
					}
				}
				catch
				{
					var currentError = Interface.GetInterfaceString("errors_critical_file");
					currentError = currentError.Replace("[file]", Panel2 == true ? "panel2.cfg" : "panel.cfg");
					MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Program.RestartArguments = " ";
					Loading.Cancel = true;
				}

			}
		}

		// ================================

		// move car
		internal static void MoveCar(Train Train, int CarIndex, double Delta, double TimeElapsed) {
			if (Train.State != TrainState.Disposed) {
				TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].FrontAxle.Follower, Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition + Delta, true, true);
				TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower, Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
				TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].FrontBogie.RearAxle.Follower, Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
				if (Train.State != TrainState.Disposed) {
					TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].RearAxle.Follower, Train.Cars[CarIndex].RearAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].RearBogie.FrontAxle.Follower, Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].RearBogie.RearAxle.Follower, Train.Cars[CarIndex].RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (Train.State != TrainState.Disposed) {
						TrackManager.UpdateTrackFollower(ref Train.Cars[CarIndex].BeaconReceiver, Train.Cars[CarIndex].BeaconReceiver.TrackPosition + Delta, true, true);
					}
				}
			}
		}

		// update atmospheric constants
		internal static void UpdateAtmosphericConstants(Train Train) {
			double h = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++) {
				h += Train.Cars[i].FrontAxle.Follower.WorldPosition.Y + Train.Cars[i].RearAxle.Follower.WorldPosition.Y;
			}
			Train.Specs.CurrentElevation = Game.RouteInitialElevation + h / (2.0 * (double)Train.Cars.Length);
			Train.Specs.CurrentAirTemperature = Game.GetAirTemperature(Train.Specs.CurrentElevation);
			Train.Specs.CurrentAirPressure = Game.GetAirPressure(Train.Specs.CurrentElevation, Train.Specs.CurrentAirTemperature);
			Train.Specs.CurrentAirDensity = Game.GetAirDensity(Train.Specs.CurrentAirPressure, Train.Specs.CurrentAirTemperature);
		}

		// get acceleration output
		internal static double GetAccelerationOutput(Train Train, int CarIndex, int CurveIndex, double Speed) {
			if (CurveIndex < Train.Cars[CarIndex].Specs.AccelerationCurves.Length) {
				double a0 = Train.Cars[CarIndex].Specs.AccelerationCurves[CurveIndex].StageZeroAcceleration;
				double s1 = Train.Cars[CarIndex].Specs.AccelerationCurves[CurveIndex].StageOneSpeed;
				double a1 = Train.Cars[CarIndex].Specs.AccelerationCurves[CurveIndex].StageOneAcceleration;
				double s2 = Train.Cars[CarIndex].Specs.AccelerationCurves[CurveIndex].StageTwoSpeed;
				double e2 = Train.Cars[CarIndex].Specs.AccelerationCurves[CurveIndex].StageTwoExponent;
				double f = Train.Cars[CarIndex].Specs.AccelerationCurvesMultiplier;
				if (Speed <= 0.0) {
					return f * a0;
				}
				if (Speed < s1) {
					double t = Speed / s1;
					return f * (a0 * (1.0 - t) + a1 * t);
				}
				if (Speed < s2) {
					return f * s1 * a1 / Speed;
				}
				return f * s1 * a1 * Math.Pow(s2, e2 - 1.0) * Math.Pow(Speed, -e2);
				
			} 
			return 0.0;
		}

		// get resistance
		private static double GetResistance(Train Train, int CarIndex, ref Axle Axle, double Speed) {
			double t;
			if (CarIndex == 0 & Train.Cars[CarIndex].Specs.CurrentSpeed >= 0.0 || CarIndex == Train.Cars.Length - 1 & Train.Cars[CarIndex].Specs.CurrentSpeed <= 0.0) {
				t = Train.Cars[CarIndex].Specs.ExposedFrontalArea;
			} else {
				t = Train.Cars[CarIndex].Specs.UnexposedFrontalArea;
			}
			double f = t * Train.Cars[CarIndex].Specs.AerodynamicDragCoefficient * Train.Specs.CurrentAirDensity / (2.0 * Train.Cars[CarIndex].Specs.MassCurrent);
			double a = Game.RouteAccelerationDueToGravity * Train.Cars[CarIndex].Specs.CoefficientOfRollingResistance + f * Speed * Speed;
			return a;
		}

		// get critical wheelslip acceleration
		private static double GetCriticalWheelSlipAccelerationForElectricMotor(Train Train, int CarIndex, double AdhesionMultiplier, double UpY, double Speed) {
			double NormalForceAcceleration = UpY * Game.RouteAccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			double coefficient = Train.Cars[CarIndex].Specs.CoefficientOfStaticFriction;
			return coefficient * AdhesionMultiplier * NormalForceAcceleration;
		}
		private static double GetCriticalWheelSlipAccelerationForFrictionBrake(Train Train, int CarIndex, double AdhesionMultiplier, double UpY, double Speed) {
			double NormalForceAcceleration = UpY * Game.RouteAccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			double coefficient = Train.Cars[CarIndex].Specs.CoefficientOfStaticFriction;
			return coefficient * AdhesionMultiplier * NormalForceAcceleration;
		}

		internal static void UpdateBogieTopplingCantAndSpring(Train Train, int CarIndex, double TimeElapsed)
		{
			if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
			{
				return;
			}
			//Same hack: Check if any car sections are defined for the offending bogie
			if (Train.Cars[CarIndex].FrontBogie.CarSections.Length != 0)
			{
				//FRONT BOGIE

				// get direction, up and side vectors
				double dx, dy, dz;
				double ux, uy, uz;
				double sx, sy, sz;
				{
					dx = Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X -
						 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X;
					dy = Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y -
						 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y;
					dz = Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z -
						 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z;
					double t = 1.0/Math.Sqrt(dx*dx + dy*dy + dz*dz);
					dx *= t;
					dy *= t;
					dz *= t;
					t = 1.0/Math.Sqrt(dx*dx + dz*dz);
					double ex = dx*t;
					double ez = dz*t;
					sx = ez;
					sy = 0.0;
					sz = -ex;
					World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
				}
				// cant and radius



				//TODO: Hopefully we can apply the base toppling and roll figures from the car itself
				// apply position due to cant/toppling
				{
					double a = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle +
							   Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
					double x = Math.Sign(a)*0.5*Game.RouteRailGauge*(1.0 - Math.Cos(a));
					double y = Math.Abs(0.5*Game.RouteRailGauge*Math.Sin(a));
					double cx = sx*x + ux*y;
					double cy = sy*x + uy*y;
					double cz = sz*x + uz*y;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z += cz;
				}
				// apply rolling
				{
					double a = -Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle -
							   Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
					Train.Cars[CarIndex].FrontBogie.Up.X = ux;
					Train.Cars[CarIndex].FrontBogie.Up.Y = uy;
					Train.Cars[CarIndex].FrontBogie.Up.Z = uz;
				}
				// apply pitching
				if (Train.Cars[CarIndex].FrontBogie.CurrentCarSection >= 0 &&
					Train.Cars[CarIndex].FrontBogie.CarSections[Train.Cars[CarIndex].FrontBogie.CurrentCarSection].Overlay)
				{
					double a = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
					double cx = 0.5*
								(Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X +
								 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X);
					double cy = 0.5*
								(Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y +
								 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y);
					double cz = 0.5*
								(Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z +
								 Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z);
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X -= cx;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y -= cy;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z -= cz;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X -= cx;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y -= cy;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z -= cz;
					World.Rotate(ref Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X,
						ref Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y,
						ref Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
					World.Rotate(ref Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X,
						ref Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y,
						ref Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].FrontBogie.FrontAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].FrontBogie.RearAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].FrontBogie.Up.X = ux;
					Train.Cars[CarIndex].FrontBogie.Up.Y = uy;
					Train.Cars[CarIndex].FrontBogie.Up.Z = uz;
				}
			}
			//Same hack: Check if any car sections are defined for the offending bogie
			if (Train.Cars[CarIndex].RearBogie.CarSections.Length != 0)
			{
				//REAR BOGIE

				// get direction, up and side vectors
				double dx, dy, dz;
				double ux, uy, uz;
				double sx, sy, sz;
				{
					dx = Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X -
						 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X;
					dy = Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y -
						 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y;
					dz = Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z -
						 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z;
					double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
					dx *= t;
					dy *= t;
					dz *= t;
					t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
					double ex = dx * t;
					double ez = dz * t;
					sx = ez;
					sy = 0.0;
					sz = -ex;
					World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
				}
				// cant and radius



				//TODO: Hopefully we can apply the base toppling and roll etc. figures from the car itself
				// apply position due to cant/toppling
				{
					double a = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle +
							   Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
					double cx = sx * x + ux * y;
					double cy = sy * x + uy * y;
					double cz = sz * x + uz * y;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z += cz;
				}
				// apply rolling
				{
					double a = -Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle -
							   Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
					Train.Cars[CarIndex].RearBogie.Up.X = ux;
					Train.Cars[CarIndex].RearBogie.Up.Y = uy;
					Train.Cars[CarIndex].RearBogie.Up.Z = uz;
				}
				// apply pitching
				if (Train.Cars[CarIndex].RearBogie.CurrentCarSection >= 0 &&
					Train.Cars[CarIndex].RearBogie.CarSections[Train.Cars[CarIndex].RearBogie.CurrentCarSection].Overlay)
				{
					double a = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
					double cx = 0.5 *
								(Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X +
								 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X);
					double cy = 0.5 *
								(Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y +
								 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y);
					double cz = 0.5 *
								(Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z +
								 Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z);
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X -= cx;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y -= cy;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z -= cz;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X -= cx;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y -= cy;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z -= cz;
					World.Rotate(ref Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X,
						ref Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y,
						ref Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
					World.Rotate(ref Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X,
						ref Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y,
						ref Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.X += cx;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Y += cy;
					Train.Cars[CarIndex].RearBogie.RearAxle.Follower.WorldPosition.Z += cz;
					Train.Cars[CarIndex].RearBogie.Up.X = ux;
					Train.Cars[CarIndex].RearBogie.Up.Y = uy;
					Train.Cars[CarIndex].RearBogie.Up.Z = uz;
				}
			}
		}

		// update toppling, cant and spring
		internal static void UpdateTopplingCantAndSpring(Train Train, int CarIndex, double TimeElapsed) {
			if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
			{
				return;
			}
			// get direction, up and side vectors
			double dx, dy, dz;
			double ux, uy, uz;
			double sx, sy, sz;
			{
				dx = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X;
				dy = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y;
				dz = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z;
				double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
				dx *= t; dy *= t; dz *= t;
				t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
				double ex = dx * t;
				double ez = dz * t;
				sx = ez;
				sy = 0.0;
				sz = -ex;
				World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
			}
			// cant and radius
			double c;
			{
				double ca = Train.Cars[CarIndex].FrontAxle.Follower.CurveCant;
				double cb = Train.Cars[CarIndex].RearAxle.Follower.CurveCant;
				c = Math.Tan(0.5 * (Math.Atan(ca) + Math.Atan(cb)));
			}
			double r, rs;
			if (Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius != 0.0 & Train.Cars[CarIndex].RearAxle.Follower.CurveRadius != 0.0) {
				r = Math.Sqrt(Math.Abs(Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius * Train.Cars[CarIndex].RearAxle.Follower.CurveRadius));
				rs = (double)Math.Sign(Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius + Train.Cars[CarIndex].RearAxle.Follower.CurveRadius);
			} else if (Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius != 0.0) {
				r = Math.Abs(Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius);
				rs = (double)Math.Sign(Train.Cars[CarIndex].FrontAxle.Follower.CurveRadius);
			} else if (Train.Cars[CarIndex].RearAxle.Follower.CurveRadius != 0.0) {
				r = Math.Abs(Train.Cars[CarIndex].RearAxle.Follower.CurveRadius);
				rs = (double)Math.Sign(Train.Cars[CarIndex].RearAxle.Follower.CurveRadius);
			} else {
				r = 0.0;
				rs = 0.0;
			}
			// roll due to shaking
			{

				double a0 = Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngle;
				double a1;
				if (Train.Cars[CarIndex].Specs.CurrentRollShakeDirection != 0.0) {
					const double c0 = 0.03;
					const double c1 = 0.15;
					a1 = c1 * Math.Atan(c0 * Train.Cars[CarIndex].Specs.CurrentRollShakeDirection);
					double d = 0.5 + Train.Cars[CarIndex].Specs.CurrentRollShakeDirection * Train.Cars[CarIndex].Specs.CurrentRollShakeDirection;
					if (Train.Cars[CarIndex].Specs.CurrentRollShakeDirection < 0.0) {
						Train.Cars[CarIndex].Specs.CurrentRollShakeDirection += d * TimeElapsed;
						if (Train.Cars[CarIndex].Specs.CurrentRollShakeDirection > 0.0) Train.Cars[CarIndex].Specs.CurrentRollShakeDirection = 0.0;
					} else {
						Train.Cars[CarIndex].Specs.CurrentRollShakeDirection -= d * TimeElapsed;
						if (Train.Cars[CarIndex].Specs.CurrentRollShakeDirection < 0.0) Train.Cars[CarIndex].Specs.CurrentRollShakeDirection = 0.0;
					}
				} else {
					a1 = 0.0;
				}
				double SpringAcceleration;
				if (!Train.Cars[CarIndex].Derailed) {
					SpringAcceleration = 15.0 * Math.Abs(a1 - a0);
				} else {
					SpringAcceleration = 1.5 * Math.Abs(a1 - a0);
				}
				double SpringDeceleration = 0.25 * SpringAcceleration;
				Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed += (double)Math.Sign(a1 - a0) * SpringAcceleration * TimeElapsed;
				double x = (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed) * SpringDeceleration * TimeElapsed;
				if (Math.Abs(x) < Math.Abs(Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed)) {
					Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed -= x;
				} else {
					Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed = 0.0;
				}
				a0 += Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngularSpeed * TimeElapsed;
				Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngle = a0;
			}
			// roll due to cant (incorporates shaking)
			{
				double cantAngle = Math.Atan(c / Game.RouteRailGauge);
				Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle = cantAngle + Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngle;
			}
			// pitch due to acceleration
			{
				for (int i = 0; i < 3; i++) {
					double a, v, j;
					if (i == 0) {
						a = Train.Cars[CarIndex].Specs.CurrentAcceleration;
						v = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationFastValue;
						j = 1.8;
					} else if (i == 1) {
						a = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationFastValue;
						v = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationMediumValue;
						j = 1.2;
					} else {
						a = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationFastValue;
						v = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationSlowValue;
						j = 1.0;
					}
					double d = a - v;
					if (d < 0.0) {
						v -= j * TimeElapsed;
						if (v < a) v = a;
					} else {
						v += j * TimeElapsed;
						if (v > a) v = a;
					}
					if (i == 0) {
						Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationFastValue = v;
					} else if (i == 1) {
						Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationMediumValue = v;
					} else {
						Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationSlowValue = v;
					}
				}
				{
					double d = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationSlowValue - Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationFastValue;
					Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationTargetAngle = 0.03 * Math.Atan(d);
				}
				{
					double a = 3.0 * (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationTargetAngle - Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle);
					Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngularSpeed += a * TimeElapsed;
					double s = Math.Abs(Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationTargetAngle - Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle);
					if (Math.Abs(Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngularSpeed) > s) {
						Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngularSpeed = s * (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngularSpeed);
					}
					Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle += Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngularSpeed * TimeElapsed;
				}
			}
			// derailment
			if (Interface.CurrentOptions.Derailments & !Train.Cars[CarIndex].Derailed) {
				double a = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle + Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
				double sa = (double)Math.Sign(a);
				double tc = Train.Cars[CarIndex].Specs.CriticalTopplingAngle;
				if (a * sa > tc) {
					Train.Cars[CarIndex].Derailed = true;
				}
			}
			// toppling roll
			if (Interface.CurrentOptions.Toppling | Train.Cars[CarIndex].Derailed) {
				double a = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle;
				double ab = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle + Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
				double h = Train.Cars[CarIndex].Specs.CenterOfGravityHeight;
				double s = Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed);
				double rmax = 2.0 * h * s * s / (Game.RouteAccelerationDueToGravity * Game.RouteRailGauge);
				double ta;
				Train.Cars[CarIndex].Topples = false;
				if (Train.Cars[CarIndex].Derailed) {
					double sab = (double)Math.Sign(ab);
					ta = 0.5 * Math.PI * (sab == 0.0 ? Program.RandomNumberGenerator.NextDouble() < 0.5 ? -1.0 : 1.0 : sab);
				} else {
					if (r != 0.0) {
						if (r < rmax) {
							double s0 = Math.Sqrt(r * Game.RouteAccelerationDueToGravity * Game.RouteRailGauge / (2.0 * h));
							const double fac = 0.25; // arbitrary coefficient
							ta = -fac * (s - s0) * rs;
							Train.Cars[CarIndex].Topples = true;
						} else {
							ta = 0.0;
						}
					} else {
						ta = 0.0;
					}
				}
				double td;
				if (Train.Cars[CarIndex].Derailed) {
					td = Math.Abs(ab);
					if (td < 0.1) td = 0.1;
				} else {
					td = 1.0;
				}
				if (a > ta) {
					double d = a - ta;
					if (td > d) td = d;
					a -= td * TimeElapsed;
				} else if (a < ta) {
					double d = ta - a;
					if (td > d) td = d;
					a += td * TimeElapsed;
				}
				Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle = a;
			} else {
				Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle = 0.0;
			}
			// apply position due to cant/toppling
			{
				double a = Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle + Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
				double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
				double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
				double cx = sx * x + ux * y;
				double cy = sy * x + uy * y;
				double cz = sz * x + uz * y;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X += cx;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y += cy;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z += cz;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X += cx;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y += cy;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z += cz;
			}
			// apply rolling
			{
				double a = -Train.Cars[CarIndex].Specs.CurrentRollDueToTopplingAngle - Train.Cars[CarIndex].Specs.CurrentRollDueToCantAngle;
				double cosa = Math.Cos(a);
				double sina = Math.Sin(a);
				World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
				World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
				Train.Cars[CarIndex].Up.X = ux;
				Train.Cars[CarIndex].Up.Y = uy;
				Train.Cars[CarIndex].Up.Z = uz;
			}
			// apply pitching
			if (Train.Cars[CarIndex].CurrentCarSection >= 0 && Train.Cars[CarIndex].CarSections[Train.Cars[CarIndex].CurrentCarSection].Overlay) {
				double a = Train.Cars[CarIndex].Specs.CurrentPitchDueToAccelerationAngle;
				double cosa = Math.Cos(a);
				double sina = Math.Sin(a);
				World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
				World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
				double cx = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X);
				double cy = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y);
				double cz = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z);
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X -= cx;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y -= cy;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z -= cz;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X -= cx;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y -= cy;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z -= cz;
				World.Rotate(ref Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X, ref Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y, ref Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
				World.Rotate(ref Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X, ref Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y, ref Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z, sx, sy, sz, cosa, sina);
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X += cx;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y += cy;
				Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z += cz;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X += cx;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y += cy;
				Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z += cz;
				Train.Cars[CarIndex].Up.X = ux;
				Train.Cars[CarIndex].Up.Y = uy;
				Train.Cars[CarIndex].Up.Z = uz;
			}
			// spring sound
			{
				double a = Train.Cars[CarIndex].Specs.CurrentRollDueToShakingAngle;
				double diff = a - Train.Cars[CarIndex].Sounds.SpringPlayedAngle;
				const double angleTolerance = 0.001;
				if (diff < -angleTolerance) {
					Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.SpringL.Buffer;
					if (buffer != null) {
						if (!Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.SpringL.Source)) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.SpringL.Position;
							Train.Cars[CarIndex].Sounds.SpringL.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
					}
					Train.Cars[CarIndex].Sounds.SpringPlayedAngle = a;
				} else if (diff > angleTolerance) {
					Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.SpringR.Buffer;
					if (buffer != null) {
						if (!Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.SpringR.Source)) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.SpringR.Position;
							Train.Cars[CarIndex].Sounds.SpringR.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
					}
					Train.Cars[CarIndex].Sounds.SpringPlayedAngle = a;
				}
			}
			// flange sound
			{
				/*
				 * This determines the amount of flange noise as a result of the angle at which the
				 * line that forms between the axles hits the rail, i.e. the less perpendicular that
				 * line is to the rails, the more flange noise there will be.
				 * */
				Vector3 d = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition;
				World.Normalize(ref d.X, ref d.Y, ref d.Z);
				double b0 = d.X * Train.Cars[CarIndex].RearAxle.Follower.WorldSide.X + d.Y * Train.Cars[CarIndex].RearAxle.Follower.WorldSide.Y + d.Z * Train.Cars[CarIndex].RearAxle.Follower.WorldSide.Z;
				double b1 = d.X * Train.Cars[CarIndex].FrontAxle.Follower.WorldSide.X + d.Y * Train.Cars[CarIndex].FrontAxle.Follower.WorldSide.Y + d.Z * Train.Cars[CarIndex].FrontAxle.Follower.WorldSide.Z;
				double spd = Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed);
				double pitch = 0.5 + 0.04 * spd;
				double b2 = Math.Abs(b0) + Math.Abs(b1);
				double basegain = 0.5 * b2 * b2 * spd * spd;
				/*
				 * This determines additional flange noise as a result of the roll angle of the car
				 * compared to the roll angle of the rails, i.e. if the car bounces due to inaccuracies,
				 * there will be additional flange noise.
				 * */
				double cdti = Math.Abs(Train.Cars[CarIndex].FrontAxle.Follower.CantDueToInaccuracy) + Math.Abs(Train.Cars[CarIndex].RearAxle.Follower.CantDueToInaccuracy);
				basegain += 0.2 * spd * spd * cdti * cdti;
				/*
				 * This applies the settings.
				 * */
				if (basegain < 0.0) basegain = 0.0;
				if (basegain > 0.75) basegain = 0.75;
				if (pitch > Train.Cars[CarIndex].Sounds.FlangePitch) {
					Train.Cars[CarIndex].Sounds.FlangePitch += TimeElapsed;
					if (Train.Cars[CarIndex].Sounds.FlangePitch > pitch) Train.Cars[CarIndex].Sounds.FlangePitch = pitch;
				} else {
					Train.Cars[CarIndex].Sounds.FlangePitch -= TimeElapsed;
					if (Train.Cars[CarIndex].Sounds.FlangePitch < pitch) Train.Cars[CarIndex].Sounds.FlangePitch = pitch;
				}
				pitch = Train.Cars[CarIndex].Sounds.FlangePitch;
				for (int i = 0; i < Train.Cars[CarIndex].Sounds.Flange.Length; i++) {
					if (i == Train.Cars[CarIndex].Sounds.FrontAxleFlangeIndex | i == Train.Cars[CarIndex].Sounds.RearAxleFlangeIndex) {
						Train.Cars[CarIndex].Sounds.FlangeVolume[i] += TimeElapsed;
						if (Train.Cars[CarIndex].Sounds.FlangeVolume[i] > 1.0) Train.Cars[CarIndex].Sounds.FlangeVolume[i] = 1.0;
					} else {
						Train.Cars[CarIndex].Sounds.FlangeVolume[i] -= TimeElapsed;
						if (Train.Cars[CarIndex].Sounds.FlangeVolume[i] < 0.0) Train.Cars[CarIndex].Sounds.FlangeVolume[i] = 0.0;
					}
					double gain = basegain * Train.Cars[CarIndex].Sounds.FlangeVolume[i];
					if (Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.Flange[i].Source)) {
						if (pitch > 0.01 & gain > 0.0001) {
							Train.Cars[CarIndex].Sounds.Flange[i].Source.Pitch = pitch;
							Train.Cars[CarIndex].Sounds.Flange[i].Source.Volume = gain;
						} else {
							Train.Cars[CarIndex].Sounds.Flange[i].Source.Stop();
						}
					} else if (pitch > 0.02 & gain > 0.01) {
						Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.Flange[i].Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Flange[i].Position;
							Train.Cars[CarIndex].Sounds.Flange[i].Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, CarIndex, true);
						}
					}
				}
			}
		}

		// update camera
		internal static void UpdateCamera(Train Train) {
			int i = Train.DriverCar;
			double dx = Train.Cars[i].FrontAxle.Follower.WorldPosition.X - Train.Cars[i].RearAxle.Follower.WorldPosition.X;
			double dy = Train.Cars[i].FrontAxle.Follower.WorldPosition.Y - Train.Cars[i].RearAxle.Follower.WorldPosition.Y;
			double dz = Train.Cars[i].FrontAxle.Follower.WorldPosition.Z - Train.Cars[i].RearAxle.Follower.WorldPosition.Z;
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double ux = Train.Cars[i].Up.X;
			double uy = Train.Cars[i].Up.Y;
			double uz = Train.Cars[i].Up.Z;
			double sx = dz * uy - dy * uz;
			double sy = dx * uz - dz * ux;
			double sz = dy * ux - dx * uy;
			double rx = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.X + Train.Cars[i].RearAxle.Follower.WorldPosition.X);
			double ry = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.Y + Train.Cars[i].RearAxle.Follower.WorldPosition.Y);
			double rz = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.Z + Train.Cars[i].RearAxle.Follower.WorldPosition.Z);
			double cx = rx + sx * Train.Cars[i].DriverX + ux * Train.Cars[i].DriverY + dx * Train.Cars[i].DriverZ;
			double cy = ry + sy * Train.Cars[i].DriverX + uy * Train.Cars[i].DriverY + dy * Train.Cars[i].DriverZ;
			double cz = rz + sz * Train.Cars[i].DriverX + uz * Train.Cars[i].DriverY + dz * Train.Cars[i].DriverZ;
			World.CameraTrackFollower.WorldPosition = new Vector3(cx, cy, cz);
			World.CameraTrackFollower.WorldDirection = new Vector3(dx, dy, dz);
			World.CameraTrackFollower.WorldUp = new Vector3(ux, uy, uz);
			World.CameraTrackFollower.WorldSide = new Vector3(sx, sy, sz);
			double f = (Train.Cars[i].DriverZ - Train.Cars[i].RearAxlePosition) / (Train.Cars[i].FrontAxlePosition - Train.Cars[i].RearAxlePosition);
			double tp = (1.0 - f) * Train.Cars[i].RearAxle.Follower.TrackPosition + f * Train.Cars[i].FrontAxle.Follower.TrackPosition;
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, tp, false, false);
		}

		// create world coordinates
		internal static void CreateWorldCoordinates(Train Train, int CarIndex, double CarX, double CarY, double CarZ, out double PositionX, out double PositionY, out double PositionZ, out double DirectionX, out double DirectionY, out double DirectionZ) {
			DirectionX = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X;
			DirectionY = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y;
			DirectionZ = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z - Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z;
			double t = DirectionX * DirectionX + DirectionY * DirectionY + DirectionZ * DirectionZ;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				DirectionX *= t; DirectionY *= t; DirectionZ *= t;
				double ux = Train.Cars[CarIndex].Up.X;
				double uy = Train.Cars[CarIndex].Up.Y;
				double uz = Train.Cars[CarIndex].Up.Z;
				double sx = DirectionZ * uy - DirectionY * uz;
				double sy = DirectionX * uz - DirectionZ * ux;
				double sz = DirectionY * ux - DirectionX * uy;
				double rx = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X);
				double ry = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y);
				double rz = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z);
				PositionX = rx + sx * CarX + ux * CarY + DirectionX * CarZ;
				PositionY = ry + sy * CarX + uy * CarY + DirectionY * CarZ;
				PositionZ = rz + sz * CarX + uz * CarY + DirectionZ * CarZ;
			} else {
				PositionX = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X;
				PositionY = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y;
				PositionZ = Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z;
				DirectionX = 0.0;
				DirectionY = 1.0;
				DirectionZ = 0.0;
			}
		}

		// initialize train
		internal static void InitializeTrain(Train Train) {
			for (int i = 0; i < Train.Cars.Length; i++) {
				InitializeCar(Train, i);
			}
			UpdateAtmosphericConstants(Train);
			UpdateTrain(Train, 0.0);
		}

		// initialize car
		private static void InitializeCar(Train Train, int CarIndex) {
			int c = CarIndex;
			for (int i = 0; i < Train.Cars[c].CarSections.Length; i++) {
				InitializeCarSection(Train, c, i);
			}
			for (int i = 0; i < Train.Cars[c].FrontBogie.CarSections.Length; i++)
			{
				InitializeFrontBogieSection(Train, c, i);
			}
			for (int i = 0; i < Train.Cars[c].RearBogie.CarSections.Length; i++)
			{
				InitializeRearBogieSection(Train, c, i);
			}
			Train.Cars[c].Brightness.PreviousBrightness = 1.0f;
			Train.Cars[c].Brightness.NextBrightness = 1.0f;
		}

		// initialize car section
		internal static void InitializeCarSection(Train Train, int CarIndex, int SectionIndex) {
			int c = CarIndex;
			int s = SectionIndex;
			for (int j = 0; j < Train.Cars[c].CarSections[s].Elements.Length; j++) {
				for (int k = 0; k < Train.Cars[c].CarSections[s].Elements[j].States.Length; k++) {
					InitializeCarSectionElement(Train, c, s, j, k);
				}
			}
		}

		//Initialize bogie sections
		internal static void InitializeFrontBogieSection(Train Train, int CarIndex, int SectionIndex)
		{
			
			int c = CarIndex;
			int s = SectionIndex;

			if (Train.Cars[c].FrontBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			for (int j = 0; j < Train.Cars[c].FrontBogie.CarSections[s].Elements.Length; j++)
			{
				for (int k = 0; k < Train.Cars[c].FrontBogie.CarSections[s].Elements[j].States.Length; k++)
				{
					InitializeFrontBogieSectionElement(Train, c, s, j, k);
				}
			}
		}

		internal static void InitializeRearBogieSection(Train Train, int CarIndex, int SectionIndex)
		{
			int c = CarIndex;
			int s = SectionIndex;
			if (Train.Cars[c].RearBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			for (int j = 0; j < Train.Cars[c].RearBogie.CarSections[s].Elements.Length; j++)
			{
				for (int k = 0; k < Train.Cars[c].RearBogie.CarSections[s].Elements[j].States.Length; k++)
				{
					InitializeRearBogieSectionElement(Train, c, s, j, k);
				}
			}
		}

		// initialize car section element
		internal static void InitializeCarSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex, int StateIndex) {
			ObjectManager.InitializeAnimatedObject(ref Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex], StateIndex, Train.Cars[CarIndex].CarSections[SectionIndex].Overlay, Train.Cars[CarIndex].CurrentlyVisible);
		}

		internal static void InitializeFrontBogieSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex, int StateIndex)
		{
			if (Train.Cars[CarIndex].FrontBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			ObjectManager.InitializeAnimatedObject(ref Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex], StateIndex, false, Train.Cars[CarIndex].FrontBogie.CurrentlyVisible);
		}

		internal static void InitializeRearBogieSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex, int StateIndex)
		{
			if (Train.Cars[CarIndex].RearBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			ObjectManager.InitializeAnimatedObject(ref Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex], StateIndex, false, Train.Cars[CarIndex].RearBogie.CurrentlyVisible);
		}

		// update train objects
		internal static void UpdateTrainObjects(double TimeElapsed, bool ForceUpdate)
		{
			/*
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UpdateTrainObjects(Trains[i], TimeElapsed, ForceUpdate);
			});
			 */
			for (int i = 0; i < Trains.Length; i++) {
				UpdateTrainObjects(Trains[i], TimeElapsed, ForceUpdate);
			}
		}
		private static void UpdateTrainObjects(Train Train, double TimeElapsed, bool ForceUpdate) {
			if (!Game.MinimalisticSimulation) {
				for (int i = 0; i < Train.Cars.Length; i++) {
					UpdateTrainObjects(Train, i, TimeElapsed, ForceUpdate);
					UpdateFrontBogieObjects(Train, i, TimeElapsed, ForceUpdate);
					UpdateRearBogieObjects(Train, i, TimeElapsed, ForceUpdate);
				}
			}
		}

		private static void UpdateTrainObjects(Train Train, int CarIndex, double TimeElapsed, bool ForceUpdate) {
			// calculate positions and directions for section element update
			int c = CarIndex;
			double dx = Train.Cars[c].FrontAxle.Follower.WorldPosition.X - Train.Cars[c].RearAxle.Follower.WorldPosition.X;
			double dy = Train.Cars[c].FrontAxle.Follower.WorldPosition.Y - Train.Cars[c].RearAxle.Follower.WorldPosition.Y;
			double dz = Train.Cars[c].FrontAxle.Follower.WorldPosition.Z - Train.Cars[c].RearAxle.Follower.WorldPosition.Z;
			double t = dx * dx + dy * dy + dz * dz;
			double ux, uy, uz, sx, sy, sz;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				dx *= t; dy *= t; dz *= t;
				ux = Train.Cars[c].Up.X;
				uy = Train.Cars[c].Up.Y;
				uz = Train.Cars[c].Up.Z;
				sx = dz * uy - dy * uz;
				sy = dx * uz - dz * ux;
				sz = dy * ux - dx * uy;
			} else {
				ux = 0.0; uy = 1.0; uz = 0.0;
				sx = 1.0; sy = 0.0; sz = 0.0;
			}
			double px = 0.5 * (Train.Cars[c].FrontAxle.Follower.WorldPosition.X + Train.Cars[c].RearAxle.Follower.WorldPosition.X);
			double py = 0.5 * (Train.Cars[c].FrontAxle.Follower.WorldPosition.Y + Train.Cars[c].RearAxle.Follower.WorldPosition.Y);
			double pz = 0.5 * (Train.Cars[c].FrontAxle.Follower.WorldPosition.Z + Train.Cars[c].RearAxle.Follower.WorldPosition.Z);
			double d = 0.5 * (Train.Cars[c].FrontAxlePosition + Train.Cars[c].RearAxlePosition);
			px -= dx * d;
			py -= dy * d;
			pz -= dz * d;
			// determine visibility
			double cdx = px - World.AbsoluteCameraPosition.X;
			double cdy = py - World.AbsoluteCameraPosition.Y;
			double cdz = pz - World.AbsoluteCameraPosition.Z;
			double dist = cdx * cdx + cdy * cdy + cdz * cdz;
			double bid = Interface.CurrentOptions.ViewingDistance + Train.Cars[c].Length;
			Train.Cars[c].CurrentlyVisible = dist < bid * bid;
			// brightness
			byte dnb;
			{
				float Brightness = (float)(Train.Cars[c].Brightness.NextTrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition);
				if (Brightness != 0.0f) {
					Brightness = (float)(Train.Cars[c].FrontAxle.Follower.TrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition) / Brightness;
					if (Brightness < 0.0f) Brightness = 0.0f;
					if (Brightness > 1.0f) Brightness = 1.0f;
					Brightness = Train.Cars[c].Brightness.PreviousBrightness * (1.0f - Brightness) + Train.Cars[c].Brightness.NextBrightness * Brightness;
				} else {
					Brightness = Train.Cars[c].Brightness.PreviousBrightness;
				}
				dnb = (byte)Math.Round(255.0 * (double)(1.0 - Brightness));
			}
			// update current section
			int s = Train.Cars[c].CurrentCarSection;
			if (s >= 0) {
				for (int i = 0; i < Train.Cars[c].CarSections[s].Elements.Length; i++) {
					UpdateCarSectionElement(Train, CarIndex, s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), Train.Cars[c].CurrentlyVisible, TimeElapsed, ForceUpdate);
					
					// brightness change
					int o = Train.Cars[c].CarSections[s].Elements[i].ObjectIndex;
					if (ObjectManager.Objects[o] != null) {
						for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++) {
							ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
						}
					}
				}
			}
		}

		private static void UpdateFrontBogieObjects(Train Train, int CarIndex, double TimeElapsed, bool ForceUpdate)
		{
			//Same hack: Check if any car sections are defined for the offending bogie
			if (Train.Cars[CarIndex].FrontBogie.CarSections.Length == 0)
			{
				return;
			}
			// calculate positions and directions for section element update
			int c = CarIndex;
			double dx = Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.X - Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.X;
			double dy = Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.Y - Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.Y;
			double dz = Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.Z - Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.Z;
			double t = dx * dx + dy * dy + dz * dz;
			double ux, uy, uz, sx, sy, sz;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				dx *= t; dy *= t; dz *= t;
				ux = Train.Cars[c].FrontBogie.Up.X;
				uy = Train.Cars[c].FrontBogie.Up.Y;
				uz = Train.Cars[c].FrontBogie.Up.Z;
				sx = dz * uy - dy * uz;
				sy = dx * uz - dz * ux;
				sz = dy * ux - dx * uy;
			}
			else
			{
				ux = 0.0; uy = 1.0; uz = 0.0;
				sx = 1.0; sy = 0.0; sz = 0.0;
			}
			double px = 0.5 * (Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.X + Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.X);
			double py = 0.5 * (Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.Y + Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.Y);
			double pz = 0.5 * (Train.Cars[c].FrontBogie.FrontAxle.Follower.WorldPosition.Z + Train.Cars[c].FrontBogie.RearAxle.Follower.WorldPosition.Z);
			double d = 0.5 * (Train.Cars[c].FrontBogie.FrontAxlePosition + Train.Cars[c].FrontBogie.RearAxlePosition);
			px -= dx * d;
			py -= dy * d;
			pz -= dz * d;
			// determine visibility
			double cdx = px - World.AbsoluteCameraPosition.X;
			double cdy = py - World.AbsoluteCameraPosition.Y;
			double cdz = pz - World.AbsoluteCameraPosition.Z;
			double dist = cdx * cdx + cdy * cdy + cdz * cdz;
			double bid = Interface.CurrentOptions.ViewingDistance + Train.Cars[c].Length;
			Train.Cars[c].FrontBogie.CurrentlyVisible = dist < bid * bid;
			// brightness
			byte dnb;
			{
				float Brightness = (float)(Train.Cars[c].Brightness.NextTrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition);
				if (Brightness != 0.0f)
				{
					Brightness = (float)(Train.Cars[c].FrontBogie.FrontAxle.Follower.TrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition) / Brightness;
					if (Brightness < 0.0f) Brightness = 0.0f;
					if (Brightness > 1.0f) Brightness = 1.0f;
					Brightness = Train.Cars[c].Brightness.PreviousBrightness * (1.0f - Brightness) + Train.Cars[c].Brightness.NextBrightness * Brightness;
				}
				else
				{
					Brightness = Train.Cars[c].Brightness.PreviousBrightness;
				}
				dnb = (byte)Math.Round(255.0 * (double)(1.0 - Brightness));
			}
			// update current section
			int s = Train.Cars[c].FrontBogie.CurrentCarSection;
			if (s >= 0)
			{
				for (int i = 0; i < Train.Cars[c].FrontBogie.CarSections[s].Elements.Length; i++)
				{
					UpdateFrontBogieSectionElement(Train, CarIndex, s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), Train.Cars[c].FrontBogie.CurrentlyVisible, TimeElapsed, ForceUpdate);

					// brightness change
					int o = Train.Cars[c].FrontBogie.CarSections[s].Elements[i].ObjectIndex;
					if (ObjectManager.Objects[o] != null)
					{
						for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++)
						{
							ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
						}
					}
				}
			}
		}

		private static void UpdateRearBogieObjects(Train Train, int CarIndex, double TimeElapsed, bool ForceUpdate)
		{
			//Same hack: Check if any car sections are defined for the offending bogie
			if (Train.Cars[CarIndex].RearBogie.CarSections.Length == 0)
			{
				return;
			}
			// calculate positions and directions for section element update
			int c = CarIndex;
			double dx = Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.X - Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.X;
			double dy = Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.Y - Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.Y;
			double dz = Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.Z - Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.Z;
			double t = dx * dx + dy * dy + dz * dz;
			double ux, uy, uz, sx, sy, sz;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				dx *= t; dy *= t; dz *= t;
				ux = Train.Cars[c].RearBogie.Up.X;
				uy = Train.Cars[c].RearBogie.Up.Y;
				uz = Train.Cars[c].RearBogie.Up.Z;
				sx = dz * uy - dy * uz;
				sy = dx * uz - dz * ux;
				sz = dy * ux - dx * uy;
			}
			else
			{
				ux = 0.0; uy = 1.0; uz = 0.0;
				sx = 1.0; sy = 0.0; sz = 0.0;
			}
			double px = 0.5 * (Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.X + Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.X);
			double py = 0.5 * (Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.Y + Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.Y);
			double pz = 0.5 * (Train.Cars[c].RearBogie.FrontAxle.Follower.WorldPosition.Z + Train.Cars[c].RearBogie.RearAxle.Follower.WorldPosition.Z);
			double d = 0.5 * (Train.Cars[c].RearBogie.FrontAxlePosition + Train.Cars[c].RearBogie.RearAxlePosition);
			px -= dx * d;
			py -= dy * d;
			pz -= dz * d;
			// determine visibility
			double cdx = px - World.AbsoluteCameraPosition.X;
			double cdy = py - World.AbsoluteCameraPosition.Y;
			double cdz = pz - World.AbsoluteCameraPosition.Z;
			double dist = cdx * cdx + cdy * cdy + cdz * cdz;
			double bid = Interface.CurrentOptions.ViewingDistance + Train.Cars[c].Length;
			Train.Cars[c].RearBogie.CurrentlyVisible = dist < bid * bid;
			// brightness
			byte dnb;
			{
				float Brightness = (float)(Train.Cars[c].Brightness.NextTrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition);
				if (Brightness != 0.0f)
				{
					Brightness = (float)(Train.Cars[c].RearBogie.FrontAxle.Follower.TrackPosition - Train.Cars[c].Brightness.PreviousTrackPosition) / Brightness;
					if (Brightness < 0.0f) Brightness = 0.0f;
					if (Brightness > 1.0f) Brightness = 1.0f;
					Brightness = Train.Cars[c].Brightness.PreviousBrightness * (1.0f - Brightness) + Train.Cars[c].Brightness.NextBrightness * Brightness;
				}
				else
				{
					Brightness = Train.Cars[c].Brightness.PreviousBrightness;
				}
				dnb = (byte)Math.Round(255.0 * (double)(1.0 - Brightness));
			}
			// update current section
			int s = Train.Cars[c].RearBogie.CurrentCarSection;
			if (s >= 0)
			{
				for (int i = 0; i < Train.Cars[c].RearBogie.CarSections[s].Elements.Length; i++)
				{
					UpdateRearBogieSectionElement(Train, CarIndex, s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), Train.Cars[c].RearBogie.CurrentlyVisible, TimeElapsed, ForceUpdate);

					// brightness change
					int o = Train.Cars[c].RearBogie.CarSections[s].Elements[i].ObjectIndex;
					if (ObjectManager.Objects[o] != null)
					{
						for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++)
						{
							ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
						}
					}
				}
			}
		}

		// change car section

		internal static void ChangeCarSection(Train Train, int CarIndex, int SectionIndex) {
			for (int i = 0; i < Train.Cars[CarIndex].CarSections.Length; i++) {
				for (int j = 0; j < Train.Cars[CarIndex].CarSections[i].Elements.Length; j++) {
					int o = Train.Cars[CarIndex].CarSections[i].Elements[j].ObjectIndex;
					Renderer.HideObject(o);
				}
			}
			if (SectionIndex >= 0) {
				InitializeCarSection(Train, CarIndex, SectionIndex);
				for (int j = 0; j < Train.Cars[CarIndex].CarSections[SectionIndex].Elements.Length; j++) {
					int o = Train.Cars[CarIndex].CarSections[SectionIndex].Elements[j].ObjectIndex;
					if (Train.Cars[CarIndex].CarSections[SectionIndex].Overlay) {
						Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
					} else {
						Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
					}
				}
			}
			Train.Cars[CarIndex].CurrentCarSection = SectionIndex;
			UpdateTrainObjects(Train, CarIndex, 0.0, true);
		}

		internal static void ChangeFrontBogieSection(Train Train, int CarIndex, int SectionIndex)
		{
			if (Train.Cars[CarIndex].FrontBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			for (int i = 0; i < Train.Cars[CarIndex].FrontBogie.CarSections.Length; i++)
			{
				for (int j = 0; j < Train.Cars[CarIndex].FrontBogie.CarSections[i].Elements.Length; j++)
				{
					int o = Train.Cars[CarIndex].FrontBogie.CarSections[i].Elements[j].ObjectIndex;
					Renderer.HideObject(o);
				}
			}
			if (SectionIndex >= 0)
			{
				InitializeFrontBogieSection(Train, CarIndex, SectionIndex);
				
				for (int j = 0; j < Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements.Length; j++)
				{
					int o = Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[j].ObjectIndex;
					Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
				}
			}
			Train.Cars[CarIndex].FrontBogie.CurrentCarSection = SectionIndex;
			UpdateFrontBogieObjects(Train, CarIndex, 0.0, true);
		}

		internal static void ChangeRearBogieSection(Train Train, int CarIndex, int SectionIndex)
		{
			if (Train.Cars[CarIndex].RearBogie.CarSections.Length == 0)
			{
				//Hack: If no bogie objects are defined, just return
				return;
			}
			for (int i = 0; i < Train.Cars[CarIndex].RearBogie.CarSections.Length; i++)
			{
				for (int j = 0; j < Train.Cars[CarIndex].RearBogie.CarSections[i].Elements.Length; j++)
				{
					int o = Train.Cars[CarIndex].RearBogie.CarSections[i].Elements[j].ObjectIndex;
					Renderer.HideObject(o);
				}
			}
			if (SectionIndex >= 0)
			{
				InitializeRearBogieSection(Train, CarIndex, SectionIndex);

				for (int j = 0; j < Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements.Length; j++)
				{
					int o = Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[j].ObjectIndex;
					Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
				}
			}
			Train.Cars[CarIndex].RearBogie.CurrentCarSection = SectionIndex;
			UpdateRearBogieObjects(Train, CarIndex, 0.0, true);
		}

		// update car section element
		private static void UpdateCarSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate) {
			Vector3 p;
			if (Train.Cars[CarIndex].CarSections[SectionIndex].Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
				p = new Vector3(Train.Cars[CarIndex].DriverX, Train.Cars[CarIndex].DriverY, Train.Cars[CarIndex].DriverZ);
			} else {
				p = Position;
			}
			double timeDelta;
			bool updatefunctions;
			if (Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0) {
				if (Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].RefreshRate) {
					timeDelta = Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
					Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				} else {
					timeDelta = TimeElapsed;
					Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
					updatefunctions = false;
				}
			} else {
				timeDelta = Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
				Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
				updatefunctions = true;
			}
			if (ForceUpdate) {
				updatefunctions = true;
			}
			ObjectManager.UpdateAnimatedObject(ref Train.Cars[CarIndex].CarSections[SectionIndex].Elements[ElementIndex], true, Train, CarIndex, Train.Cars[CarIndex].CurrentCarSection, Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition - Train.Cars[CarIndex].FrontAxlePosition, p, Direction, Up, Side, Train.Cars[CarIndex].CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta);
		}

		private static void UpdateFrontBogieSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
		{
			{
				Vector3 p;
				if (Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Overlay &
					World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
				{
					//Should never be hit, as a bogie cannot be a cab object
					p = new Vector3(Train.Cars[CarIndex].DriverX, Train.Cars[CarIndex].DriverY, Train.Cars[CarIndex].DriverZ);
				}
				else
				{
					p = Position;
				}
				double timeDelta;
				bool updatefunctions;

				if (Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
				{
					if (Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >=Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
					{
						timeDelta =
							Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
						Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate =
							TimeElapsed;
						updatefunctions = true;
					}
					else
					{
						timeDelta = TimeElapsed;
						Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
						updatefunctions = false;
					}
				}
				else
				{
					timeDelta = Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
					Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}
				if (ForceUpdate)
				{
					updatefunctions = true;
				}
				ObjectManager.UpdateAnimatedObject(ref Train.Cars[CarIndex].FrontBogie.CarSections[SectionIndex].Elements[ElementIndex], true, Train, CarIndex,Train.Cars[CarIndex].RearBogie.CurrentCarSection,Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.TrackPosition - Train.Cars[CarIndex].RearBogie.FrontAxlePosition, p, Direction, Up,Side, Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta);
			}
		}

		private static void UpdateRearBogieSectionElement(Train Train, int CarIndex, int SectionIndex, int ElementIndex,Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
		{
			{
				//REAR BOGIE
				Vector3 p;
				if (Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Overlay &
					World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
				{
					//Should never be hit, as a bogie cannot be a cab object
					p = new Vector3(Train.Cars[CarIndex].DriverX, Train.Cars[CarIndex].DriverY, Train.Cars[CarIndex].DriverZ);
				}
				else
				{
					p = Position;
				}
				double timeDelta;
				bool updatefunctions;
				if (Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
				{
					if (Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >=
						Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
					{
						timeDelta =
							Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
						Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate =
							TimeElapsed;
						updatefunctions = true;
					}
					else
					{
						timeDelta = TimeElapsed;
						Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate +=
							TimeElapsed;
						updatefunctions = false;
					}
				}
				else
				{
					timeDelta = Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
					Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate =
						TimeElapsed;
					updatefunctions = true;
				}
				if (ForceUpdate)
				{
					updatefunctions = true;
				}
				ObjectManager.UpdateAnimatedObject(ref Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Elements[ElementIndex], true, Train, CarIndex,Train.Cars[CarIndex].RearBogie.CurrentCarSection,Train.Cars[CarIndex].RearBogie.FrontAxle.Follower.TrackPosition - Train.Cars[CarIndex].RearBogie.FrontAxlePosition, p, Direction, Up,
					Side, Train.Cars[CarIndex].RearBogie.CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta);
			}
		}


		// update train
		internal static void UpdateTrain(Train Train, double TimeElapsed) {
			if (Train.State == TrainState.Pending) {
				// pending train
				bool forceIntroduction = Train == PlayerTrain && !Game.MinimalisticSimulation;
				double time = 0.0;
				if (!forceIntroduction) {
					for (int i = 0; i < Game.Stations.Length; i++) {
						if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass) {
							if (Game.Stations[i].ArrivalTime >= 0.0) {
								time = Game.Stations[i].ArrivalTime;
							} else if (Game.Stations[i].DepartureTime >= 0.0) {
								time = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
							}
							break;
						}
					}
					time -= Train.TimetableDelta;
				}
				if (Game.SecondsSinceMidnight >= time | forceIntroduction) {
					bool introduce = true;
					if (!forceIntroduction) {
						if (Train.CurrentSectionIndex >= 0) {
							if (!Game.Sections[Train.CurrentSectionIndex].IsFree()) {
								introduce = false;
							}
						}
					}
					if (introduce) {
						// train is introduced
						Train.State = TrainState.Available;
						for (int j = 0; j < Train.Cars.Length; j++) {
							if (Train.Cars[j].CarSections.Length != 0) {
								TrainManager.ChangeCarSection(Train, j, j <= Train.DriverCar | Train != PlayerTrain ? 0 : -1);
								TrainManager.ChangeFrontBogieSection(Train, j, j <= Train.DriverCar | Train != PlayerTrain ? 0 : -1);
								TrainManager.ChangeRearBogieSection(Train, j, j <= Train.DriverCar | Train != PlayerTrain ? 0 : -1);
							}
							if (Train.Cars[j].Specs.IsMotorCar) {
								if (Train.Cars[j].Sounds.Loop.Buffer != null) {
									OpenBveApi.Math.Vector3 pos = Train.Cars[j].Sounds.Loop.Position;
									Train.Cars[j].Sounds.Loop.Source = Sounds.PlaySound(Train.Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, pos, Train, j, true);
								}
							}
						}
					}
				}
			} else if (Train.State == TrainState.Available) {
				// available train
				UpdateTrainPhysicsAndControls(Train, TimeElapsed);
				if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
					if (Train.Specs.CurrentAverageSpeed > Train.CurrentRouteLimit) {
						Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), Game.MessageDependency.RouteLimit, Interface.GameMode.Arcade, Game.MessageColor.Orange, double.PositiveInfinity);
					}
					if (Train.CurrentSectionLimit == 0.0) {
						Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), Game.MessageDependency.SectionLimit, Interface.GameMode.Normal, Game.MessageColor.Red, double.PositiveInfinity);
					} else if (Train.Specs.CurrentAverageSpeed > Train.CurrentSectionLimit) {
						Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), Game.MessageDependency.SectionLimit, Interface.GameMode.Normal, Game.MessageColor.Orange, double.PositiveInfinity);
					}
				}
				if (Train.AI != null) {
					Train.AI.Trigger(Train, TimeElapsed);
				}
			} else if (Train.State == TrainState.Bogus) {
				// bogus train
				if (Train.AI != null) {
					Train.AI.Trigger(Train, TimeElapsed);
				}
			}
		}

		/// <summary>This method should be called once a frame to update the position, speed and state of all trains within the simulation</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal static void UpdateTrains(double TimeElapsed) {
			// individual trains (without objects)
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UpdateTrain(Trains[i], TimeElapsed);
			});
			//for (int i = 0; i < Trains.Length; i++) {
			//	UpdateTrain(Trains[i], TimeElapsed);
			//}
			// detect collision
			if (!Game.MinimalisticSimulation & Interface.CurrentOptions.Collisions) {
				
				//for (int i = 0; i < Trains.Length; i++) {
				System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
				{
					// with other trains
					if (Trains[i].State == TrainState.Available)
					{
						double a = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxlePosition +
								   0.5*Trains[i].Cars[0].Length;
						double b = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								   Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxlePosition - 0.5*Trains[i].Cars[0].Length;
						for (int j = i + 1; j < Trains.Length; j++)
						{
							if (Trains[j].State == TrainState.Available)
							{
								double c = Trains[j].Cars[0].FrontAxle.Follower.TrackPosition -
										   Trains[j].Cars[0].FrontAxlePosition + 0.5*Trains[j].Cars[0].Length;
								double d = Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition -
										   Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxlePosition -
										   0.5*Trains[j].Cars[0].Length;
								if (a > d & b < c)
								{
									if (a > c)
									{
										// i > j
										int k = Trains[i].Cars.Length - 1;
										if (Trains[i].Cars[k].Specs.CurrentSpeed < Trains[j].Cars[0].Specs.CurrentSpeed)
										{
											double v = Trains[j].Cars[0].Specs.CurrentSpeed -
													   Trains[i].Cars[k].Specs.CurrentSpeed;
											double s = (Trains[i].Cars[k].Specs.CurrentSpeed*Trains[i].Cars[k].Specs.MassCurrent +
														Trains[j].Cars[0].Specs.CurrentSpeed*Trains[j].Cars[0].Specs.MassCurrent)/
													   (Trains[i].Cars[k].Specs.MassCurrent + Trains[j].Cars[0].Specs.MassCurrent);
											Trains[i].Cars[k].Specs.CurrentSpeed = s;
											Trains[j].Cars[0].Specs.CurrentSpeed = s;
											double e = 0.5*(c - b) + 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[k].FrontAxle.Follower,Trains[i].Cars[k].FrontAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[k].RearAxle.Follower,Trains[i].Cars[k].RearAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[0].FrontAxle.Follower,Trains[j].Cars[0].FrontAxle.Follower.TrackPosition - e, false, false);

											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[0].RearAxle.Follower,Trains[j].Cars[0].RearAxle.Follower.TrackPosition - e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/
														   (Trains[i].Cars[k].Specs.MassCurrent +
															Trains[j].Cars[0].Specs.MassCurrent);
												double fi = Trains[j].Cars[0].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[k].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Cars[k].Derailed = true;
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Cars[i].Derailed = true;
											}
											// adjust cars for train i
											for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h + 1].FrontAxlePosition + 0.5*Trains[i].Cars[h + 1].Length;
												b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h].RearAxlePosition - 0.5*Trains[i].Cars[h].Length;
												d = b - a - Trains[i].Couplers[h].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition - d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition - d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[i].Cars[h + 1].Specs.MassCurrent +
																	Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Cars[h + 1].Derailed = true;
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Cars[h].Derailed = true;
													}
													Trains[i].Cars[h].Specs.CurrentSpeed =
														Trains[i].Cars[h + 1].Specs.CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = 1; h < Trains[j].Cars.Length; h++)
											{
												a = Trains[j].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h - 1].RearAxlePosition - 0.5*Trains[j].Cars[h - 1].Length;
												b = Trains[j].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h].FrontAxlePosition + 0.5*Trains[j].Cars[h].Length;
												d = a - b - Trains[j].Couplers[h - 1].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].FrontAxle.Follower,Trains[j].Cars[h].FrontAxle.Follower.TrackPosition + d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].RearAxle.Follower,Trains[j].Cars[h].RearAxle.Follower.TrackPosition + d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[j].Cars[h - 1].Specs.MassCurrent +
																	Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Cars[h - 1].Derailed = true;
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Cars[h].Derailed = true;
													}
													Trains[j].Cars[h].Specs.CurrentSpeed =
														Trains[j].Cars[h - 1].Specs.CurrentSpeed;
												}
											}
										}
									}
									else
									{
										// i < j
										int k = Trains[j].Cars.Length - 1;
										if (Trains[i].Cars[0].Specs.CurrentSpeed > Trains[j].Cars[k].Specs.CurrentSpeed)
										{
											double v = Trains[i].Cars[0].Specs.CurrentSpeed -
													   Trains[j].Cars[k].Specs.CurrentSpeed;
											double s = (Trains[i].Cars[0].Specs.CurrentSpeed*Trains[i].Cars[0].Specs.MassCurrent +
														Trains[j].Cars[k].Specs.CurrentSpeed*Trains[j].Cars[k].Specs.MassCurrent)/
													   (Trains[i].Cars[0].Specs.MassCurrent + Trains[j].Cars[k].Specs.MassCurrent);
											Trains[i].Cars[0].Specs.CurrentSpeed = s;
											Trains[j].Cars[k].Specs.CurrentSpeed = s;
											double e = 0.5*(a - d) + 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].FrontAxle.Follower,Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].RearAxle.Follower,Trains[i].Cars[0].RearAxle.Follower.TrackPosition - e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[k].FrontAxle.Follower,Trains[j].Cars[k].FrontAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[k].RearAxle.Follower,Trains[j].Cars[k].RearAxle.Follower.TrackPosition + e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/
														   (Trains[i].Cars[0].Specs.MassCurrent +
															Trains[j].Cars[k].Specs.MassCurrent);
												double fi = Trains[j].Cars[k].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[0].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Cars[0].Derailed = true;
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Cars[k].Derailed = true;
											}
											// adjust cars for train i
											for (int h = 1; h < Trains[i].Cars.Length; h++)
											{
												a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h - 1].RearAxlePosition - 0.5*Trains[i].Cars[h - 1].Length;
												b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h].FrontAxlePosition + 0.5*Trains[i].Cars[h].Length;
												d = a - b - Trains[i].Couplers[h - 1].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition + d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition + d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[i].Cars[h - 1].Specs.MassCurrent +
																	Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Cars[h - 1].Derailed = true;
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Cars[h].Derailed = true;
													}
													Trains[i].Cars[h].Specs.CurrentSpeed =
														Trains[i].Cars[h - 1].Specs.CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = Trains[j].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[j].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h + 1].FrontAxlePosition + 0.5*Trains[j].Cars[h + 1].Length;
												b = Trains[j].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h].RearAxlePosition - 0.5*Trains[j].Cars[h].Length;
												d = b - a - Trains[j].Couplers[h].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].FrontAxle.Follower,Trains[j].Cars[h].FrontAxle.Follower.TrackPosition - d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].RearAxle.Follower,Trains[j].Cars[h].RearAxle.Follower.TrackPosition - d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[j].Cars[h + 1].Specs.MassCurrent +
																	Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Cars[h + 1].Derailed = true;
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Cars[h].Derailed = true;
													}
													Trains[j].Cars[h].Specs.CurrentSpeed =
														Trains[j].Cars[h + 1].Specs.CurrentSpeed;
												}
											}
										}
									}
								}
							}

						}
					}
					// with buffers
					if (i == PlayerTrain.TrainIndex)
					{
						double a = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxlePosition +
								   0.5*Trains[i].Cars[0].Length;
						double b = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								   Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxlePosition - 0.5*Trains[i].Cars[0].Length;
						for (int j = 0; j < Game.BufferTrackPositions.Length; j++)
						{
							if (a > Game.BufferTrackPositions[j] & b < Game.BufferTrackPositions[j])
							{
								a += 0.0001;
								b -= 0.0001;
								double da = a - Game.BufferTrackPositions[j];
								double db = Game.BufferTrackPositions[j] - b;
								if (da < db)
								{
									// front
									TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].FrontAxle.Follower,Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - da, false, false);
									TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].RearAxle.Follower,Trains[i].Cars[0].RearAxle.Follower.TrackPosition - da, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[0].Specs.CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Cars[0].Derailed = true;
									}
									Trains[i].Cars[0].Specs.CurrentSpeed = 0.0;
									for (int h = 1; h < Trains[i].Cars.Length; h++)
									{
										a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h - 1].RearAxlePosition - 0.5*Trains[i].Cars[h - 1].Length;
										b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h].FrontAxlePosition + 0.5*Trains[i].Cars[h].Length;
										double d = a - b - Trains[i].Couplers[h - 1].MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition + d, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition + d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].Specs.CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Cars[h].Derailed = true;
											}
											Trains[i].Cars[h].Specs.CurrentSpeed = 0.0;
										}
									}
								}
								else
								{
									// rear
									int c = Trains[i].Cars.Length - 1;
									TrackManager.UpdateTrackFollower(ref Trains[i].Cars[c].FrontAxle.Follower,Trains[i].Cars[c].FrontAxle.Follower.TrackPosition + db, false, false);
									TrackManager.UpdateTrackFollower(ref Trains[i].Cars[c].RearAxle.Follower,Trains[i].Cars[c].RearAxle.Follower.TrackPosition + db, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[c].Specs.CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Cars[c].Derailed = true;
									}
									Trains[i].Cars[c].Specs.CurrentSpeed = 0.0;
									for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
									{
										a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h + 1].FrontAxlePosition + 0.5*Trains[i].Cars[h + 1].Length;
										b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h].RearAxlePosition - 0.5*Trains[i].Cars[h].Length;
										double d = b - a - Trains[i].Couplers[h].MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition - d, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition - d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].Specs.CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Cars[h].Derailed = true;
											}
											Trains[i].Cars[h].Specs.CurrentSpeed = 0.0;
										}
									}
								}
							}
						}
					}
				});
			}
			// compute final angles and positions
			//for (int i = 0; i < Trains.Length; i++) {
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				if (Trains[i].State != TrainState.Disposed & Trains[i].State != TrainManager.TrainState.Bogus)
				{
					for (int j = 0; j < Trains[i].Cars.Length; j++)
					{
						Trains[i].Cars[j].FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						UpdateTopplingCantAndSpring(Trains[i], j, TimeElapsed);
						UpdateBogieTopplingCantAndSpring(Trains[i], j, TimeElapsed);
					}
				}
			});
		}

		// dispose train
		internal static void DisposeTrain(Train Train) {
			Train.State = TrainState.Disposed;
			for (int i = 0; i < Train.Cars.Length; i++) {
				int s = Train.Cars[i].CurrentCarSection;
				if (s >= 0) {
					for (int j = 0; j < Train.Cars[i].CarSections[s].Elements.Length; j++) {
						Renderer.HideObject(Train.Cars[i].CarSections[s].Elements[j].ObjectIndex);
					}
					for (int j = 0; j < Train.Cars[i].FrontBogie.CarSections[s].Elements.Length; j++)
					{
						Renderer.HideObject(Train.Cars[i].FrontBogie.CarSections[s].Elements[j].ObjectIndex);
					}
				}
			}
			Sounds.StopAllSounds(Train);
			
			for (int i = 0; i < Game.Sections.Length; i++) {
				Game.Sections[i].Leave(Train);
			}
			if (Game.Sections.Length != 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
		}

		// synchronize train
		private static void SynchronizeTrain(Train Train) {
			for (int i = 0; i < Train.Cars.Length; i++) {
				double s = 0.5 * (Train.Cars[i].FrontAxle.Follower.TrackPosition + Train.Cars[i].RearAxle.Follower.TrackPosition);
				double d = 0.5 * (Train.Cars[i].FrontAxle.Follower.TrackPosition - Train.Cars[i].RearAxle.Follower.TrackPosition);
				TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontAxle.Follower, s + d, false, false);
				TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearAxle.Follower, s - d, false, false);
				double b = Train.Cars[i].FrontAxle.Follower.TrackPosition - Train.Cars[i].FrontAxlePosition + Train.Cars[i].BeaconReceiverPosition;
				TrackManager.UpdateTrackFollower(ref Train.Cars[i].BeaconReceiver, b , false, false);
			}
		}

		// update train station
		private static void UpdateTrainStation(Train Train, double TimeElapsed) {
			if (Train.Station >= 0) {
				int i = Train.Station;
				int n = Game.GetStopIndex(Train.Station, Train.Cars.Length);
				double tf, tb;
				if (n >= 0) {
					double p0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
					double p1 = Game.Stations[i].Stops[n].TrackPosition;
					tf = Game.Stations[i].Stops[n].ForwardTolerance;
					tb = Game.Stations[i].Stops[n].BackwardTolerance;
					Train.StationDistanceToStopPoint = p1 - p0;
				} else {
					Train.StationDistanceToStopPoint = 0.0;
					tf = 5.0;
					tb = 5.0;
				}
				if (Train.StationState == TrainStopState.Pending) {
					Train.StationDepartureSoundPlayed = false;
					if (Game.StopsAtStation(i, Train)) {
						Train.StationDepartureSoundPlayed = false;
						// automatically open doors
						if (Train.Specs.DoorOpenMode != DoorMode.Manual) {
							if ((GetDoorsState(Train, Game.Stations[i].OpenLeftDoors, Game.Stations[i].OpenRightDoors) & TrainDoorState.AllOpened) == 0) {
								if (Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.1 / 3.6 & Math.Abs(Train.Specs.CurrentAverageAcceleration) < 0.1 / 3.6) {
									if (Train.StationDistanceToStopPoint < tb & -Train.StationDistanceToStopPoint < tf) {
										OpenTrainDoors(Train, Game.Stations[i].OpenLeftDoors, Game.Stations[i].OpenRightDoors);
									}
								}
							}
						}
						// detect arrival
						if (Train.Specs.CurrentAverageSpeed > -0.277777777777778 & Train.Specs.CurrentAverageSpeed < 0.277777777777778) {
							bool left, right;
							if (Game.Stations[i].OpenLeftDoors) {
								left = false;
								for (int j = 0; j < Train.Cars.Length; j++) {
									if (Train.Cars[j].Specs.AnticipatedLeftDoorsOpened) {
										left = true; break;
									}
								}
							} else {
								left = true;
							}
							if (Game.Stations[i].OpenRightDoors) {
								right = false;
								for (int j = 0; j < Train.Cars.Length; j++) {
									if (Train.Cars[j].Specs.AnticipatedRightDoorsOpened) {
										right = true; break;
									}
								}
							} else {
								right = true;
							}
							if (left & right) {
								// arrival
								Train.StationState = TrainStopState.Boarding;
								Train.StationAdjust = false;
								Sounds.StopSound(Train.Cars[Train.DriverCar].Sounds.Halt.Source);
								Sounds.SoundBuffer buffer = Game.Stations[i].ArrivalSoundBuffer;
								if (buffer != null) {
									OpenBveApi.Math.Vector3 pos = Game.Stations[i].SoundOrigin;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, false);
								}
								Train.StationArrivalTime = Game.SecondsSinceMidnight;
								Train.StationDepartureTime = Game.Stations[i].DepartureTime - Train.TimetableDelta;
								if (Train.StationDepartureTime - Game.SecondsSinceMidnight < Game.Stations[i].StopTime) {
									Train.StationDepartureTime = Game.SecondsSinceMidnight + Game.Stations[i].StopTime;
								}
								Train.Passengers.PassengerRatio = Game.Stations[i].PassengerRatio;
								UpdateTrainMassFromPassengerRatio(Train);
								if (Train == PlayerTrain) {
									double early = 0.0;
									if (Game.Stations[i].ArrivalTime >= 0.0) {
										early = (Game.Stations[i].ArrivalTime - Train.TimetableDelta) - Train.StationArrivalTime;
									}
									string s;
									if (early < -1.0) {
										s = Interface.GetInterfaceString("message_station_arrival_late");
									} else if (early > 1.0) {
										s = Interface.GetInterfaceString("message_station_arrival_early");
									} else {
										s = Interface.GetInterfaceString("message_station_arrival");
									}
									System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
									TimeSpan a = TimeSpan.FromSeconds(Math.Abs(early));
									string b = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
									if (Train.StationDistanceToStopPoint < -0.1) {
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_overrun");
									} else if (Train.StationDistanceToStopPoint > 0.1) {
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_underrun");
									}
									double d = Math.Abs(Train.StationDistanceToStopPoint);
									string c = d.ToString("0.0", Culture);
									if (Game.Stations[i].StationType == Game.StationType.Terminal) {
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_terminal");
									}
									s = s.Replace("[name]", Game.Stations[i].Name);
									s = s.Replace("[time]", b);
									s = s.Replace("[difference]", c);
									Game.AddMessage(s, Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 10.0);
									if (Game.Stations[i].StationType == Game.StationType.Normal) {
										s = Interface.GetInterfaceString("message_station_deadline");
										Game.AddMessage(s, Game.MessageDependency.Station, Interface.GameMode.Normal, Game.MessageColor.Blue, double.PositiveInfinity);
									}
									Timetable.UpdateCustomTimetable(Game.Stations[i].TimetableDaytimeTexture, Game.Stations[i].TimetableNighttimeTexture);
								}
								// schedule door locks (passengers stuck between the doors)
								for (int j = 0; j < Train.Cars.Length; j++) {
									for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
										Train.Cars[j].Specs.Doors[k].DoorLockDuration = 0.0;
										if (Game.Stations[i].OpenLeftDoors & Train.Cars[j].Specs.Doors[k].Direction == -1 | Game.Stations[i].OpenRightDoors & Train.Cars[j].Specs.Doors[k].Direction == 1) {
											double p = 0.005 * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio;
											if (Program.RandomNumberGenerator.NextDouble() < p) {
												/*
												 * -- door lock at state --
												 * minimum: 0.2 (nearly closed)
												 * maximum: 0.8 (nearly opened)
												 * */
												Train.Cars[j].Specs.Doors[k].DoorLockState = 0.2 + 0.6 * Program.RandomNumberGenerator.NextDouble();
												/* -- waiting time --
												 * minimum: 2.9 s
												 * maximum: 40.0 s
												 * average: 7.6 s
												 * */
												p = Program.RandomNumberGenerator.NextDouble();
												Train.Cars[j].Specs.Doors[k].DoorLockDuration = (50.0 - 10.0 * p) / (17.0 - 16.0 * p);
											}
										}
									}
								}
							} else if (Train.Specs.CurrentAverageSpeed > -0.277777777777778 & Train.Specs.CurrentAverageSpeed < 0.277777777777778) {
								// correct stop position
								if (!Train.StationAdjust & (Train.StationDistanceToStopPoint > tb | Train.StationDistanceToStopPoint < -tf)) {
									Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Adjust.Buffer;
									if (buffer != null) {
										OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Adjust.Position;
										Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
									}
									if (Train == TrainManager.PlayerTrain) {
										Game.AddMessage(Interface.GetInterfaceString("message_station_correct"), Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Orange, Game.SecondsSinceMidnight + 5.0);
									}
									Train.StationAdjust = true;
								}
							} else {
								Train.StationAdjust = false;
							}
						}
					}
				} else if (Train.StationState == TrainStopState.Boarding) {
					// automatically close doors
					if (Train.Specs.DoorCloseMode != DoorMode.Manual & Game.Stations[i].StationType == Game.StationType.Normal) {
						if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - 1.0 / Train.Cars[Train.DriverCar].Specs.DoorCloseFrequency) {
							if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0) {
								CloseTrainDoors(Train, true, true);
							}
						}
					}
					// detect departure
					bool left, right;
					if (!Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors) {
						left = true;
						right = true;
					} else {
						if (Game.Stations[i].OpenLeftDoors) {
							left = false;
							for (int j = 0; j < Train.Cars.Length; j++) {
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].State != 0.0) {
										left = true; break;
									}
								} if (left) break;
							}
						} else {
							left = false;
						}
						if (Game.Stations[i].OpenRightDoors) {
							right = false;
							for (int j = 0; j < Train.Cars.Length; j++) {
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++) {
									if (Train.Cars[j].Specs.Doors[k].State != 0.0) {
										right = true; break;
									}
								} if (right) break;
							}
						} else {
							right = false;
						}
					}
					if (left | right) {
						// departure message
						if (Game.SecondsSinceMidnight > Train.StationDepartureTime) {
							Train.StationState = TrainStopState.Completed;
							if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal) {
								if (!Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors | Train.Specs.DoorCloseMode != DoorMode.Manual) {
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
								} else {
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart_closedoors"), Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
								}
							} else if (Game.Stations[i].StationType == Game.StationType.ChangeEnds) {
								//Game.AddMessage("CHANGE ENDS", Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Magenta, Game.SecondsSinceMidnight + 5.0);
								JumpTrain(Train, i + 1);
							}
						}
						// passengers boarding
						for (int j = 0; j < Train.Cars.Length; j++) {
							double r = 2.0 * Game.Stations[i].PassengerRatio * TimeElapsed;
							if (r >= Program.RandomNumberGenerator.NextDouble()) {
								int d = (int)Math.Floor(Program.RandomNumberGenerator.NextDouble() * (double)Train.Cars[j].Specs.Doors.Length);
								if (Train.Cars[j].Specs.Doors[d].State == 1.0) {
									Train.Cars[j].Specs.CurrentRollShakeDirection += (double)Train.Cars[j].Specs.Doors[d].Direction;
								}
							}
						}
					} else {
						Train.StationState = TrainStopState.Completed;
						if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal) {
							Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
						}
					}
					// departure sound
					if (!Train.StationDepartureSoundPlayed) {
						Sounds.SoundBuffer buffer = Game.Stations[i].DepartureSoundBuffer;
						if (buffer != null) {
							double dur = Sounds.GetDuration(buffer);
							if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - dur) {
								Sounds.PlaySound(buffer, 1.0, 1.0, Game.Stations[i].SoundOrigin, false);
								Train.StationDepartureSoundPlayed = true;
							}
						}
					}
				}
			} else {
				Train.StationState = TrainStopState.Pending;
			}
			// automatically close doors
			if (Train.Specs.DoorCloseMode == DoorMode.Automatic) {
				if (Train.Station == -1 | Train.StationState == TrainStopState.Completed) {
					if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0) {
						CloseTrainDoors(Train, true, true);
					}
				}
			}
		}

		// update train doors
		private static void UpdateTrainDoors(Train Train, double TimeElapsed) {
			OpenBveApi.Runtime.DoorStates oldState = OpenBveApi.Runtime.DoorStates.None;
			OpenBveApi.Runtime.DoorStates newState = OpenBveApi.Runtime.DoorStates.None;
			for (int i = 0; i < Train.Cars.Length; i++) {
				bool ld = Train.Cars[i].Specs.AnticipatedLeftDoorsOpened;
				bool rd = Train.Cars[i].Specs.AnticipatedRightDoorsOpened;
				double os = Train.Cars[i].Specs.DoorOpenFrequency;
				double cs = Train.Cars[i].Specs.DoorCloseFrequency;
				for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
					if (Train.Cars[i].Specs.Doors[j].Direction == -1 | Train.Cars[i].Specs.Doors[j].Direction == 1) {
						bool shouldBeOpen = Train.Cars[i].Specs.Doors[j].Direction == -1 ? ld : rd;
						if (Train.Cars[i].Specs.Doors[j].State > 0.0) {
							if (Train.Cars[i].Specs.Doors[j].Direction == -1) {
								oldState |= OpenBveApi.Runtime.DoorStates.Left;
							} else {
								oldState |= OpenBveApi.Runtime.DoorStates.Right;
							}
						}
						if (shouldBeOpen) {
							// open
							Train.Cars[i].Specs.Doors[j].State += os * TimeElapsed;
							if (Train.Cars[i].Specs.Doors[j].State > 1.0) {
								Train.Cars[i].Specs.Doors[j].State = 1.0;
							}
						} else {
							// close
							if (Train.Cars[i].Specs.Doors[j].DoorLockDuration > 0.0) {
								if (Train.Cars[i].Specs.Doors[j].State > Train.Cars[i].Specs.Doors[j].DoorLockState) {
									Train.Cars[i].Specs.Doors[j].State -= cs * TimeElapsed;
								}
								if (Train.Cars[i].Specs.Doors[j].State < Train.Cars[i].Specs.Doors[j].DoorLockState) {
									Train.Cars[i].Specs.Doors[j].State = Train.Cars[i].Specs.Doors[j].DoorLockState;
								}
								Train.Cars[i].Specs.Doors[j].DoorLockDuration -= TimeElapsed;
								if (Train.Cars[i].Specs.Doors[j].DoorLockDuration < 0.0) {
									Train.Cars[i].Specs.Doors[j].DoorLockDuration = 0.0;
								}
							} else {
								Train.Cars[i].Specs.Doors[j].State -= cs * TimeElapsed;
							}
							if (Train.Cars[i].Specs.Doors[j].State < 0.0) {
								Train.Cars[i].Specs.Doors[j].State = 0.0;
							}
						}
						if (Train.Cars[i].Specs.Doors[j].State > 0.0) {
							if (Train.Cars[i].Specs.Doors[j].Direction == -1) {
								newState |= OpenBveApi.Runtime.DoorStates.Left;
							} else {
								newState |= OpenBveApi.Runtime.DoorStates.Right;
							}
						}
					}
				}
			}
			// door changed
			if (oldState != OpenBveApi.Runtime.DoorStates.None & newState == OpenBveApi.Runtime.DoorStates.None) {
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.PilotLampOn.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.PilotLampOn.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			} else if (oldState == OpenBveApi.Runtime.DoorStates.None & newState != OpenBveApi.Runtime.DoorStates.None) {
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.PilotLampOff.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.PilotLampOff.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
			if (oldState != newState) {
				if (Train.Plugin != null) {
					Train.Plugin.DoorChange(oldState, newState);
				}
			}
		}

		// train doors
		internal static void OpenTrainDoors(Train Train, bool Left, bool Right) {
			bool sl = false, sr = false;
			for (int i = 0; i < Train.Cars.Length; i++) {
				if (Left & !Train.Cars[i].Specs.AnticipatedLeftDoorsOpened) {
					Train.Cars[i].Specs.AnticipatedLeftDoorsOpened = true;
					sl = true;
				}
				if (Right & !Train.Cars[i].Specs.AnticipatedRightDoorsOpened) {
					Train.Cars[i].Specs.AnticipatedRightDoorsOpened = true;
					sr = true;
				}
			}
			if (sl) {
				for (int i = 0; i < Train.Cars.Length; i++) {
					Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.DoorOpenL.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.DoorOpenL.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorOpenPitch, 1.0, pos, Train, i, false);
					}
					for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
						if (Train.Cars[i].Specs.Doors[j].Direction == -1) {
							Train.Cars[i].Specs.Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}
			if (sr) {
				for (int i = 0; i < Train.Cars.Length; i++) {
					Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.DoorOpenR.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.DoorOpenR.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorClosePitch, 1.0, pos, Train, i, false);
					}
					for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
						if (Train.Cars[i].Specs.Doors[j].Direction == 1) {
							Train.Cars[i].Specs.Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}
		}
		internal static void CloseTrainDoors(Train Train, bool Left, bool Right) {
			bool sl = false, sr = false;
			for (int i = 0; i < Train.Cars.Length; i++) {
				if (Left & Train.Cars[i].Specs.AnticipatedLeftDoorsOpened) {
					Train.Cars[i].Specs.AnticipatedLeftDoorsOpened = false;
					sl = true;
				}
				if (Right & Train.Cars[i].Specs.AnticipatedRightDoorsOpened) {
					Train.Cars[i].Specs.AnticipatedRightDoorsOpened = false;
					sr = true;
				}
			}
			if (sl) {
				for (int i = 0; i < Train.Cars.Length; i++) {
					Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.DoorCloseL.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.DoorCloseL.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorClosePitch, 1.0, pos, Train, i, false);
					}
				}
			}
			if (sr) {
				for (int i = 0; i < Train.Cars.Length; i++) {
					Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.DoorCloseR.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.DoorCloseR.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorClosePitch, 1.0, pos, Train, i, false);
					}
				}
			}
		}
		/// <summary>Specifies enumerated constants that can be combined to represent doors states.</summary>
		[Flags]
		internal enum TrainDoorState {
			None = 0,
			/// <summary>Fully closed doors are present in the train.</summary>
			Closed = 1,
			/// <summary>Fully opened doors are present in the train.</summary>
			Opened = 2,
			/// <summary>Doors are present in the train which are neither fully closed nor fully opened.</summary>
			Mixed = 4,
			/// <summary>All doors in the train are fully closed.</summary>
			AllClosed = 8,
			/// <summary>All doors in the train are fully opened.</summary>
			AllOpened = 16,
			/// <summary>All doors in the train are neither fully closed nor fully opened.</summary>
			AllMixed = 32
		}
		/// <summary>Returns the combination of door states encountered in a train.</summary>
		/// <param name="Train">The train to consider.</param>
		/// <param name="Left">Whether to include left doors.</param>
		/// <param name="Right">Whether to include right doors.</param>
		/// <returns>A bit mask combining encountered door states.</returns>
		internal static TrainDoorState GetDoorsState(Train Train, bool Left, bool Right) {
			bool opened = false, closed = false, mixed = false;
			for (int i = 0; i < Train.Cars.Length; i++) {
				for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
					if (Left & Train.Cars[i].Specs.Doors[j].Direction == -1 | Right & Train.Cars[i].Specs.Doors[j].Direction == 1) {
						if (Train.Cars[i].Specs.Doors[j].State == 0.0) {
							closed = true;
						} else if (Train.Cars[i].Specs.Doors[j].State == 1.0) {
							opened = true;
						} else {
							mixed = true;
						}
					}
				}
			}
			TrainDoorState Result = TrainDoorState.None;
			if (opened) Result |= TrainDoorState.Opened;
			if (closed) Result |= TrainDoorState.Closed;
			if (mixed) Result |= TrainDoorState.Mixed;
			if (opened & !closed & !mixed) Result |= TrainDoorState.AllOpened;
			if (!opened & closed & !mixed) Result |= TrainDoorState.AllClosed;
			if (!opened & !closed & mixed) Result |= TrainDoorState.AllMixed;
			return Result;
		}

		// update safety system
		private static void UpdateSafetySystem(Train Train) {
			Game.UpdatePluginSections(Train);
			if (Train.Plugin != null) {
				Train.Plugin.LastSection = Train.CurrentSectionIndex;
				Train.Plugin.UpdatePlugin();
			}
		}

		// update brake system
		private static void UpdateBrakeSystem(Train Train, double TimeElapsed, out double[] DecelerationDueToBrake, out double[] DecelerationDueToMotor) {
			// individual brake systems
			DecelerationDueToBrake = new double[Train.Cars.Length];
			DecelerationDueToMotor = new double[Train.Cars.Length];
			for (int i = 0; i < Train.Cars.Length; i++) {
				UpdateBrakeSystem(Train, i, TimeElapsed, out DecelerationDueToBrake[i], out DecelerationDueToMotor[i]);
			}
			// brake pipe pressure distribution dummy (just averages)
			double TotalPressure = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++) {
				if (i > 0) {
					if (Train.Cars[i - 1].Derailed | Train.Cars[i].Derailed) {
						Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					}
				}
				if (i < Train.Cars.Length - 1) {
					if (Train.Cars[i].Derailed | Train.Cars[i + 1].Derailed) {
						Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					}
				}
				TotalPressure += Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure;
			}
			double AveragePressure = TotalPressure / (double)Train.Cars.Length;
			for (int i = 0; i < Train.Cars.Length; i++) {
				Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = AveragePressure;
			}
		}
		private static void UpdateBrakeSystem(Train Train, int CarIndex, double TimeElapsed, out double DecelerationDueToBrake, out double DecelerationDueToMotor) {
			// air compressor
			if (Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main) {
				if (Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled) {
					if (Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AirCompressorMaximumPressure) {
						Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled = false;
						Train.Cars[CarIndex].Sounds.CpLoopStarted = false;
						Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpEnd.Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpEnd.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
						buffer = Train.Cars[CarIndex].Sounds.CpLoop.Buffer;
						if (buffer != null) {
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.CpLoop.Source);
						}
					} else {
						Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure += Train.Cars[CarIndex].Specs.AirBrake.AirCompressorRate * TimeElapsed;
						if (!Train.Cars[CarIndex].Sounds.CpLoopStarted && Game.SecondsSinceMidnight > Train.Cars[CarIndex].Sounds.CpStartTimeStarted + 5.0) {
							Train.Cars[CarIndex].Sounds.CpLoopStarted = true;
							Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpLoop.Buffer;
							if (buffer != null) {
								OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpLoop.Position;
								Train.Cars[CarIndex].Sounds.CpLoop.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, true);
							}
						}
					}
				} else {
					if (Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.AirCompressorMinimumPressure) {
						Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled = true;
						Train.Cars[CarIndex].Sounds.CpStartTimeStarted = Game.SecondsSinceMidnight;
						Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpStart.Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpStart.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
					}
				}
			}
			// initialize
			const double Tolerance = 5000.0;
			int airsound = -1;
			// equalizing reservoir
			if (Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main) {
				if (Train.Specs.CurrentEmergencyBrake.Actual) {
					double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirEmergencyRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure -= r;
				} else {
					if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake) {
						// automatic air brake
						if (Train.Specs.AirBrake.Handle.Actual == AirBrakeHandleState.Service) {
							double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirServiceRate;
							double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
							r = GetRate(d / m, r * TimeElapsed);
							if (r > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure -= r;
						} else if (Train.Specs.AirBrake.Handle.Actual == AirBrakeHandleState.Release) {
							double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirChargeRate;
							double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
							r = GetRate(d / m, r * TimeElapsed);
							if (r > d) r = d;
							d = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							if (r > d) r = d;
							double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient;
							double s = r * f * TimeElapsed;
							if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure) {
								r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
								s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
							}
							Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure += 0.5 * r;
							Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
						}
					} else if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake) {
						// electromagnetic straight air brake
						double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirChargeRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > d) r = d;
						d = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
						if (r > d) r = d;
						double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient;
						double s = r * f * TimeElapsed;
						if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure) {
							r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
							s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
						}
						Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure += 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
					}
				}
			}
			// brake pipe (main reservoir)
			if ((Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake | Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake) & Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main) {
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure + Tolerance) {
					// brake pipe exhaust valve
					double r = Train.Specs.CurrentEmergencyBrake.Actual ? Train.Cars[CarIndex].Specs.AirBrake.BrakePipeEmergencyRate : Train.Cars[CarIndex].Specs.AirBrake.BrakePipeServiceRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure -= r;
				} else if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure) {
					// fill brake pipe from main reservoir
					double r = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeChargeRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					if (r > d) r = d;
					double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirBrakePipeCoefficient;
					double s = r * f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure) {
						r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					}
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
				}
			}
			// triple valve (auxillary reservoir, brake pipe, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake) {
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure) {
					if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) {
						// back-flow from brake cylinder to auxillary reservoir
						double u = (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + r > m) {
							r = m - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d) {
							r *= d / s;
							s = d;
						}
						if (s > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) {
							r *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure / s;
							s = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure += 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= 0.5 * s;
					} else if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance) {
						// refill brake cylinder from auxillary reservoir
						double u = (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						double d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure) {
							r = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d) {
							r *= d / s;
							s = d;
						}
						d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (s > d) {
							r *= d / s;
							s = d;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure -= 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * s;
					}
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				} else if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance) {
					double u = (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					{ // refill auxillary reservoir from brake pipe
						double r = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirChargeRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure) {
							r = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
						}
						if (r > d) r = d;
						d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						if (r > d) r = d;
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
						double s = r / f;
						if (s > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure) {
							r *= Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure / s;
							s = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
						}
						if (s > d) {
							r *= d / s;
							s = d;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure += 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure -= 0.5 * s;
					}
					{ // brake cylinder release
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
						// air sound
						if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure) {
							double p = 0.8 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - 0.2 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
							if (p < 0.0) p = 0.0;
							Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
							airsound = p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
						}
					}
				} else {
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
			}
			// solenoid valve for electromagnetic straight air brake (auxillary reservoir, electric command, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake) {
				// refill auxillary reservoir from brake pipe
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance) {
					double r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirChargeRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure) {
						r = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					}
					if (r > d) r = d;
					d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
					if (r > d) r = d;
					double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
					double s = r / f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure) {
						r *= Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					}
					if (s > d) {
						r *= d / s;
						s = d;
					}
					Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure -= 0.5 * s;
				}
				{ // electric command
					bool emergency;
					if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure) {
						emergency = true;
					} else {
						emergency = Train.Specs.CurrentEmergencyBrake.Actual;
					}
					double p; if (emergency) {
						p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					} else {
						p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
						p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
					}
					if (Train.Cars[CarIndex].Specs.IsMotorCar & !Train.Specs.CurrentEmergencyBrake.Actual & Train.Specs.CurrentReverser.Actual != 0) {
						// brake control system
						if (Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.BrakeControlSpeed) {
							if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.ClosingElectromagneticValve) {
								// closing electromagnetic valve (lock-out valve)
								p = 0.0;
							} else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.DelayFillingControl) {
								// delay-filling control
								//Variable f is never used, so don't calculate it
								//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
								double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
								double pr = p / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								double b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
								double d = b - a;
								if (d > 0.0) {
									p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
									if (p > 1.0) p = 1.0;
									p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								} else {
									p = 0.0;
								}
							}
						}
					}
					if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > p + Tolerance) {
						// brake cylinder release
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - p;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (r > d) r = d;
						// air sound
						if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure) {
							Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
							airsound = p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
						}
						// pressure change
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
					} else if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < p) {
						// refill brake cylinder from auxillary reservoir
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r;
						if (emergency) {
							r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyChargeRate * f;
						} else {
							r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						}
						double d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure) {
							r = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d) {
							r *= d / s;
							s = d;
						}
						d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (s > d) {
							r *= d / s;
							s = d;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure -= 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * s;
						// air sound
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					} else {
						// air sound
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					}
				}
			}
			// valves for electric command brake (main reservoir, electric command, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectricCommandBrake) {
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual) {
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				} else {
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (!Train.Specs.CurrentEmergencyBrake.Actual & Train.Specs.CurrentReverser.Actual != 0) {
					// brake control system
					if (Train.Cars[CarIndex].Specs.IsMotorCar & Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.BrakeControlSpeed) {
						if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.ClosingElectromagneticValve) {
							// closing electromagnetic valve (lock-out valve)
							p = 0.0;
						} else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.DelayFillingControl) {
							// delay-filling control
							// Variable f is never used, so don't calculate it
							//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
							double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
							double pr = p / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
							double b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
							double d = b - a;
							if (d > 0.0) {
								p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
								if (p > 1.0) p = 1.0;
								p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
							} else {
								p = 0.0;
							}
						}
					}
				}
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > p + Tolerance | p == 0.0) {
					// brake cylinder exhaust valve
					double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					// air sound
					if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure) {
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
						airsound = p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
					}
					// pressure change
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
				} else if ((Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < p | p == Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure) & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure) {
					// fill brake cylinder from main reservoir
					double r;
					if (Train.Specs.CurrentEmergencyBrake.Actual) {
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyChargeRate;
					} else {
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate;
					}
					double pm = p < Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure ? p : Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					double d = pm - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					double f1 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
					double f2 = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirBrakePipeCoefficient;
					double f3 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
					double f = f1 * f2 / f3; // MainReservoirBrakeCylinderCoefficient
					double s = r * f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure) {
						r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					}
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				} else {
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
			}
			// straight air pipe (for compatibility needle only)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake & Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main) {
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual) {
					p = 0.0;
				} else {
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (p + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure) {
					double r;
					if (Train.Specs.CurrentEmergencyBrake.Actual) {
						r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeEmergencyRate;
					} else {
						r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeReleaseRate;
					}
					double d = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure - p;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure -= r;
				} else if (p > Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure + Tolerance) {
					double r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeServiceRate;
					double d = p - Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure += r;
				}
			} else if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectricCommandBrake) {
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual) {
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				} else {
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure = p;
			}
			// air sound
			if (airsound == 0) {
				// air zero
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.AirZero.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirZero.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			} else if (airsound == 1) {
				// air
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.Air.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Air.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			} else if (airsound == 2) {
				// air high
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.AirHigh.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirHigh.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			}
			// deceleration provided by brake
			double pressureratio = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
			DecelerationDueToBrake = pressureratio * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
			// deceleration provided by motor
			if (Train.Cars[CarIndex].Specs.BrakeType != CarBrakeType.AutomaticAirBrake && Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) >= Train.Cars[CarIndex].Specs.BrakeControlSpeed & Train.Specs.CurrentReverser.Actual != 0 & !Train.Specs.CurrentEmergencyBrake.Actual) {
				double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
				double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
				DecelerationDueToMotor = f * a;
			} else {
				DecelerationDueToMotor = 0.0;
			}
			// hold brake
			if (Train.Specs.CurrentHoldBrake.Actual & DecelerationDueToMotor == 0.0) {
				if (Game.SecondsSinceMidnight >= Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime) {
					Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime = Game.SecondsSinceMidnight + Train.Cars[CarIndex].Specs.HoldBrake.UpdateInterval;
					Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput += 0.8 * Train.Cars[CarIndex].Specs.CurrentAcceleration * (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed);
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput < 0.0) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
					double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput > a) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = a;
				}
				DecelerationDueToMotor = Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput;
			} else {
				Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
			}
			{ // rub sound
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.Rub.Buffer;
				if (buffer != null) {
					double spd = Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed);
					double pitch = 1.0 / (spd + 1.0) + 1.0;
					double gain = Train.Cars[CarIndex].Derailed ? 0.0 : pressureratio;
					if (spd < 1.38888888888889) {
						double t = spd * spd;
						gain *= 1.5552 * t - 0.746496 * spd * t;
					} else if (spd > 12.5) {
						double t = spd - 12.5;
						const double fadefactor = 0.1;
						gain *= 1.0 / (fadefactor * t * t + 1.0);
					}
					if (Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.Rub.Source)) {
						if (pitch > 0.01 & gain > 0.001) {
							Train.Cars[CarIndex].Sounds.Rub.Source.Pitch = pitch;
							Train.Cars[CarIndex].Sounds.Rub.Source.Volume = gain;
						} else {
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.Rub.Source);
						}
					} else if (pitch > 0.02 & gain > 0.01) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Rub.Position;
						Train.Cars[CarIndex].Sounds.Rub.Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, CarIndex, true);
					}
				}
			}
		}
		private static double GetRate(double Ratio, double Factor) {
			Ratio = Ratio < 0.0 ? 0.0 : Ratio > 1.0 ? 1.0 : Ratio;
			Ratio = 1.0 - Ratio;
			return 1.5 * Factor * (1.01 - Ratio * Ratio);
		}

		// apply emergency brake
		internal static void ApplyEmergencyBrake(Train Train) {
			// sound
			if (!Train.Specs.CurrentEmergencyBrake.Driver) {
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				for (int i = 0; i < Train.Cars.Length; i++) {
					//Don't assign buffer twice
					//buffer = Train.Cars[i].Sounds.EmrBrake.Buffer;
					buffer = Train.Cars[Train.DriverCar].Sounds.EmrBrake.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.EmrBrake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			// apply
			if (Train.Specs.SingleHandle) {
				ApplyNotch(Train, 0, false, Train.Specs.MaximumBrakeNotch, true);
			} else {
				ApplyNotch(Train, 0, true, Train.Specs.MaximumBrakeNotch, true);
			}
			ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
			Train.Specs.CurrentEmergencyBrake.Driver = true;
			Train.Specs.CurrentHoldBrake.Driver = false;
			Train.Specs.CurrentConstSpeed = false;
			// plugin
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}

		// unapply emergency brake
		internal static void UnapplyEmergencyBrake(Train Train) {
			if (Train.Specs.CurrentEmergencyBrake.Driver) {
				// sound
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				// apply
				if (Train.Specs.SingleHandle) {
					ApplyNotch(Train, 0, false, Train.Specs.MaximumBrakeNotch, true);
				} else {
					ApplyNotch(Train, 0, true, Train.Specs.MaximumBrakeNotch, true);
				}
				ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
				Train.Specs.CurrentEmergencyBrake.Driver = false;
				// plugin
				if (Train.Plugin == null) return;
				Train.Plugin.UpdatePower();
				Train.Plugin.UpdateBrake();
			}
		}

		// apply hold brake
		internal static void ApplyHoldBrake(Train Train, bool Value) {
			Train.Specs.CurrentHoldBrake.Driver = Value;
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}

		// apply reverser
		internal static void ApplyReverser(Train Train, int Value, bool Relative) {
			int a = Train.Specs.CurrentReverser.Driver;
			int r = Relative ? a + Value : Value;
			if (r < -1) r = -1;
			if (r > 1) r = 1;
			if (a != r) {
				Train.Specs.CurrentReverser.Driver = r;
				if (Train.Plugin != null) {
					Train.Plugin.UpdateReverser();
				}
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
				// sound
				if (a == 0 & r != 0) {
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.ReverserOn.Buffer;
					if (buffer == null) return;
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.ReverserOn.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				} else if (a != 0 & r == 0) {
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.ReverserOff.Buffer;
					if (buffer == null) return;
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.ReverserOff.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
		}

		// apply notch
		internal static void ApplyNotch(Train Train, int PowerValue, bool PowerRelative, int BrakeValue, bool BrakeRelative) {
			// determine notch
			int p = PowerRelative ? PowerValue + Train.Specs.CurrentPowerNotch.Driver : PowerValue;
			if (p < 0) {
				p = 0;
			} else if (p > Train.Specs.MaximumPowerNotch) {
				p = Train.Specs.MaximumPowerNotch;
			}
			int b = BrakeRelative ? BrakeValue + Train.Specs.CurrentBrakeNotch.Driver : BrakeValue;
			if (b < 0) {
				b = 0;
			} else if (b > Train.Specs.MaximumBrakeNotch) {
				b = Train.Specs.MaximumBrakeNotch;
			}
			// power sound
			if (p < Train.Specs.CurrentPowerNotch.Driver) {
				if (p > 0) {
					// down (not min)
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerDown.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerDown.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				} else {
					// min
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerMin.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerMin.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			} else if (p > Train.Specs.CurrentPowerNotch.Driver) {
				if (p < Train.Specs.MaximumPowerNotch) {
					// up (not max)
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerUp.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerUp.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				} else {
					// max
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerMax.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerMax.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			// brake sound
			if (b < Train.Specs.CurrentBrakeNotch.Driver) {
				// brake release
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Brake.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Brake.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				if (b > 0) {
					// brake release (not min)
					buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				} else {
					// brake min
					buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Buffer;
					if (buffer != null) {
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			} else if (b > Train.Specs.CurrentBrakeNotch.Driver) {
				// brake
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Buffer;
				if (buffer != null) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
			// apply notch
			if (Train.Specs.SingleHandle) {
				if (b != 0) p = 0;
			}
			Train.Specs.CurrentPowerNotch.Driver = p;
			Train.Specs.CurrentBrakeNotch.Driver = b;
			Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
			// plugin
			if (Train.Plugin != null) {
				Train.Plugin.UpdatePower();
				Train.Plugin.UpdateBrake();
			}
		}

		// apply air brake handle
		internal static void ApplyAirBrakeHandle(Train Train, int RelativeDirection) {
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake) {
				if (RelativeDirection == -1) {
					if (Train.Specs.AirBrake.Handle.Driver == AirBrakeHandleState.Service) {
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Lap);
					} else {
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Release);
					}
				} else if (RelativeDirection == 1) {
					if (Train.Specs.AirBrake.Handle.Driver == AirBrakeHandleState.Release) {
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Lap);
					} else {
						ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
					}
				}
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
			}
		}
		internal static void ApplyAirBrakeHandle(Train Train, AirBrakeHandleState State) {
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake) {
				if (State != Train.Specs.AirBrake.Handle.Driver) {
					// sound when moved to service
					if (State == AirBrakeHandleState.Service) {
						Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Brake.Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Brake.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
						}
					}
					// sound
					if ((int)State < (int)Train.Specs.AirBrake.Handle.Driver) {
						// brake release
						if ((int)State > 0) {
							// brake release (not min)
							Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
							if (buffer != null) {
								OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
							}
						} else {
							// brake min
							Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Buffer;
							if (buffer != null) {
								OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Position;
								Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
							}
						}
					} else if ((int)State > (int)Train.Specs.AirBrake.Handle.Driver) {
						// brake
						Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
						}
					}
					// apply
					Train.Specs.AirBrake.Handle.Driver = State;
					Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
					// plugin
					if (Train.Plugin != null) {
						Train.Plugin.UpdatePower();
						Train.Plugin.UpdateBrake();
					}
				}
			}
		}

		// update train passengers
		private static void UpdateTrainPassengers(Train Train, double TimeElapsed) {
			double accelerationDifference = Train.Specs.CurrentAverageAcceleration - Train.Passengers.CurrentAcceleration;
			double jerk = 0.25 + 0.10 * Math.Abs(accelerationDifference);
			double accelerationQuanta = jerk * TimeElapsed;
			if (Math.Abs(accelerationDifference) < accelerationQuanta) {
				Train.Passengers.CurrentAcceleration = Train.Specs.CurrentAverageAcceleration;
				accelerationDifference = 0.0;
			} else {
				Train.Passengers.CurrentAcceleration += (double)Math.Sign(accelerationDifference) * accelerationQuanta;
				accelerationDifference = Train.Specs.CurrentAverageAcceleration - Train.Passengers.CurrentAcceleration;
			}
			Train.Passengers.CurrentSpeedDifference += accelerationDifference * TimeElapsed;
			double acceleration = 0.10 + 0.35 * Math.Abs(Train.Passengers.CurrentSpeedDifference);
			double speedQuanta = acceleration * TimeElapsed;
			if (Math.Abs(Train.Passengers.CurrentSpeedDifference) < speedQuanta) {
				Train.Passengers.CurrentSpeedDifference = 0.0;
			} else {
				Train.Passengers.CurrentSpeedDifference -= (double)Math.Sign(Train.Passengers.CurrentSpeedDifference) * speedQuanta;
			}
			if (Train.Passengers.PassengerRatio > 0.0) {
				double threshold = 1.0 / Train.Passengers.PassengerRatio;
				if (Math.Abs(Train.Passengers.CurrentSpeedDifference) > threshold) {
					Train.Passengers.FallenOver = true;
				} else {
					Train.Passengers.FallenOver = false;
				}
			} else {
				Train.Passengers.FallenOver = false;
			}
		}

		// update speeds
		private static void UpdateSpeeds(Train Train, double TimeElapsed) {
			if (Game.MinimalisticSimulation & Train == PlayerTrain) {
				// hold the position of the player's train during startup
				for (int i = 0; i < Train.Cars.Length; i++) {
					Train.Cars[i].Specs.CurrentSpeed = 0.0;
					Train.Cars[i].Specs.CurrentAccelerationOutput = 0.0;
				}
				return;
			}
			// update brake system
			double[] DecelerationDueToBrake, DecelerationDueToMotor;
			UpdateBrakeSystem(Train, TimeElapsed, out DecelerationDueToBrake, out DecelerationDueToMotor);
			// calculate new car speeds
			double[] NewSpeeds = new double[Train.Cars.Length];
			for (int i = 0; i < Train.Cars.Length; i++) {
				double PowerRollingCouplerAcceleration;
				// rolling on an incline
				{
					double a = Train.Cars[i].FrontAxle.Follower.WorldDirection.Y;
					double b = Train.Cars[i].RearAxle.Follower.WorldDirection.Y;
					PowerRollingCouplerAcceleration = -0.5 * (a + b) * Game.RouteAccelerationDueToGravity;
				}
				// friction
				double FrictionBrakeAcceleration;
				{
					double v = Math.Abs(Train.Cars[i].Specs.CurrentSpeed);
					double a = GetResistance(Train, i, ref Train.Cars[i].FrontAxle, v);
					double b = GetResistance(Train, i, ref Train.Cars[i].RearAxle, v);
					FrictionBrakeAcceleration = 0.5 * (a + b);
				}
				// power
				double wheelspin = 0.0;
				double wheelSlipAccelerationMotorFront;
				double wheelSlipAccelerationMotorRear;
				double wheelSlipAccelerationBrakeFront;
				double wheelSlipAccelerationBrakeRear;
				if (Train.Cars[i].Derailed)
				{
					wheelSlipAccelerationMotorFront = 0.0;
					wheelSlipAccelerationBrakeFront = 0.0;
					wheelSlipAccelerationMotorRear = 0.0;
					wheelSlipAccelerationBrakeRear = 0.0;
				}
				else
				{
					wheelSlipAccelerationMotorFront = GetCriticalWheelSlipAccelerationForElectricMotor(Train, i,Train.Cars[i].FrontAxle.Follower.AdhesionMultiplier, Train.Cars[i].FrontAxle.Follower.WorldUp.Y,Train.Cars[i].Specs.CurrentSpeed);
					wheelSlipAccelerationMotorRear = GetCriticalWheelSlipAccelerationForElectricMotor(Train, i,Train.Cars[i].RearAxle.Follower.AdhesionMultiplier, Train.Cars[i].RearAxle.Follower.WorldUp.Y,Train.Cars[i].Specs.CurrentSpeed);
					wheelSlipAccelerationBrakeFront = GetCriticalWheelSlipAccelerationForFrictionBrake(Train, i,Train.Cars[i].FrontAxle.Follower.AdhesionMultiplier, Train.Cars[i].FrontAxle.Follower.WorldUp.Y,Train.Cars[i].Specs.CurrentSpeed);
					wheelSlipAccelerationBrakeRear = GetCriticalWheelSlipAccelerationForFrictionBrake(Train, i,Train.Cars[i].RearAxle.Follower.AdhesionMultiplier, Train.Cars[i].RearAxle.Follower.WorldUp.Y,Train.Cars[i].Specs.CurrentSpeed);
				}
				if (DecelerationDueToMotor[i] == 0.0) {
					double a;
					if (Train.Cars[i].Specs.IsMotorCar) {
						if (DecelerationDueToMotor[i] == 0.0) {
							if (Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Actual > 0 & !Train.Specs.CurrentHoldBrake.Actual & !Train.Specs.CurrentEmergencyBrake.Actual) {
								// target acceleration
								a = GetAccelerationOutput(Train, i, Train.Specs.CurrentPowerNotch.Actual - 1, (double)Train.Specs.CurrentReverser.Actual * Train.Cars[i].Specs.CurrentSpeed);
								// readhesion device
								if (a > Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput) {
									a = Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput;
								}
								// wheel slip
								if (a < wheelSlipAccelerationMotorFront) {
									Train.Cars[i].FrontAxle.CurrentWheelSlip = false;
								} else {
									Train.Cars[i].FrontAxle.CurrentWheelSlip = true;
									wheelspin += (double)Train.Specs.CurrentReverser.Actual * a * Train.Cars[i].Specs.MassCurrent;
								}
								if (a < wheelSlipAccelerationMotorRear) {
									Train.Cars[i].RearAxle.CurrentWheelSlip = false;
								} else {
									Train.Cars[i].RearAxle.CurrentWheelSlip = true;
									wheelspin += (double)Train.Specs.CurrentReverser.Actual * a * Train.Cars[i].Specs.MassCurrent;
								}
								// readhesion device
								{
									if (Game.SecondsSinceMidnight >= Train.Cars[i].Specs.ReAdhesionDevice.NextUpdateTime) {
										double d = Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval;
										double f = Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor;
										double t = Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval;
										double r = Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor;
										Train.Cars[i].Specs.ReAdhesionDevice.NextUpdateTime = Game.SecondsSinceMidnight + d;
										if (Train.Cars[i].FrontAxle.CurrentWheelSlip | Train.Cars[i].RearAxle.CurrentWheelSlip) {
											Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput = a * f;
											Train.Cars[i].Specs.ReAdhesionDevice.TimeStable = 0.0;
										} else {
											Train.Cars[i].Specs.ReAdhesionDevice.TimeStable += d;
											if (Train.Cars[i].Specs.ReAdhesionDevice.TimeStable >= t) {
												Train.Cars[i].Specs.ReAdhesionDevice.TimeStable -= t;
												if (r != 0.0 & Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput <= a + 1.0) {
													if (Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput < 0.025) {
														Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput = 0.025;
													} else {
														Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput *= r;
													}
												} else {
													Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput = double.PositiveInfinity;
												}
											}
										}
									}
								}
								// const speed
								if (Train.Specs.CurrentConstSpeed) {
									if (Game.SecondsSinceMidnight >= Train.Cars[i].Specs.ConstSpeed.NextUpdateTime) {
										Train.Cars[i].Specs.ConstSpeed.NextUpdateTime = Game.SecondsSinceMidnight + Train.Cars[i].Specs.ConstSpeed.UpdateInterval;
										Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput -= 0.8 * Train.Cars[i].Specs.CurrentAcceleration * (double)Train.Specs.CurrentReverser.Actual;
										if (Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput < 0.0) Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput = 0.0;
									}
									if (a > Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput) a = Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput;
									if (a < 0.0) a = 0.0;
								} else {
									Train.Cars[i].Specs.ConstSpeed.CurrentAccelerationOutput = a;
								}
								// finalize
								if (wheelspin != 0.0) a = 0.0;
							} else {
								a = 0.0;
								Train.Cars[i].FrontAxle.CurrentWheelSlip = false;
								Train.Cars[i].RearAxle.CurrentWheelSlip = false;
							}
						} else {
							a = 0.0;
							Train.Cars[i].FrontAxle.CurrentWheelSlip = false;
							Train.Cars[i].RearAxle.CurrentWheelSlip = false;
						}
					} else {
						a = 0.0;
						Train.Cars[i].FrontAxle.CurrentWheelSlip = false;
						Train.Cars[i].RearAxle.CurrentWheelSlip = false;
					}
					if (!Train.Cars[i].Derailed) {
						if (Train.Cars[i].Specs.CurrentAccelerationOutput < a) {
							if (Train.Cars[i].Specs.CurrentAccelerationOutput < 0.0) {
								Train.Cars[i].Specs.CurrentAccelerationOutput += Train.Cars[i].Specs.JerkBrakeDown * TimeElapsed;
							} else {
								Train.Cars[i].Specs.CurrentAccelerationOutput += Train.Cars[i].Specs.JerkPowerUp * TimeElapsed;
							}
							if (Train.Cars[i].Specs.CurrentAccelerationOutput > a) {
								Train.Cars[i].Specs.CurrentAccelerationOutput = a;
							}
						} else {
							Train.Cars[i].Specs.CurrentAccelerationOutput -= Train.Cars[i].Specs.JerkPowerDown * TimeElapsed;
							if (Train.Cars[i].Specs.CurrentAccelerationOutput < a) {
								Train.Cars[i].Specs.CurrentAccelerationOutput = a;
							}
						}
					} else {
						Train.Cars[i].Specs.CurrentAccelerationOutput = 0.0;
					}
				}
				// brake
				bool wheellock = wheelspin == 0.0 & Train.Cars[i].Derailed;
				if (!Train.Cars[i].Derailed & wheelspin == 0.0) {
					double a;
					// motor
					if (Train.Cars[i].Specs.IsMotorCar & DecelerationDueToMotor[i] != 0.0) {
						a = -DecelerationDueToMotor[i];
						if (Train.Cars[i].Specs.CurrentAccelerationOutput > a) {
							if (Train.Cars[i].Specs.CurrentAccelerationOutput > 0.0) {
								Train.Cars[i].Specs.CurrentAccelerationOutput -= Train.Cars[i].Specs.JerkPowerDown * TimeElapsed;
							} else {
								Train.Cars[i].Specs.CurrentAccelerationOutput -= Train.Cars[i].Specs.JerkBrakeUp * TimeElapsed;
							}
							if (Train.Cars[i].Specs.CurrentAccelerationOutput < a) {
								Train.Cars[i].Specs.CurrentAccelerationOutput = a;
							}
						} else {
							Train.Cars[i].Specs.CurrentAccelerationOutput += Train.Cars[i].Specs.JerkBrakeDown * TimeElapsed;
							if (Train.Cars[i].Specs.CurrentAccelerationOutput > a) {
								Train.Cars[i].Specs.CurrentAccelerationOutput = a;
							}
						}
					}
					// brake
					a = DecelerationDueToBrake[i];
					if (Train.Cars[i].Specs.CurrentSpeed >= -0.01 & Train.Cars[i].Specs.CurrentSpeed <= 0.01) {
						double rf = Train.Cars[i].FrontAxle.Follower.WorldDirection.Y;
						double rr = Train.Cars[i].RearAxle.Follower.WorldDirection.Y;
						double ra = Math.Abs(0.5 * (rf + rr) * Game.RouteAccelerationDueToGravity);
						if (a > ra) a = ra;
					}
					double factor = Train.Cars[i].Specs.MassEmpty / Train.Cars[i].Specs.MassCurrent;
					if (a >= wheelSlipAccelerationBrakeFront) {
						wheellock = true;
					} else {
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}
					if (a >= wheelSlipAccelerationBrakeRear) {
						wheellock = true;
					} else {
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}
				} else if (Train.Cars[i].Derailed) {
					FrictionBrakeAcceleration += Game.CoefficientOfGroundFriction * Game.RouteAccelerationDueToGravity;
				}
				// motor
				if (Train.Specs.CurrentReverser.Actual != 0) {
					double factor = Train.Cars[i].Specs.MassEmpty / Train.Cars[i].Specs.MassCurrent;
					if (Train.Cars[i].Specs.CurrentAccelerationOutput > 0.0) {
						PowerRollingCouplerAcceleration += (double)Train.Specs.CurrentReverser.Actual * Train.Cars[i].Specs.CurrentAccelerationOutput * factor;
					} else {
						double a = -Train.Cars[i].Specs.CurrentAccelerationOutput;
						if (a >= wheelSlipAccelerationMotorFront) {
							Train.Cars[i].FrontAxle.CurrentWheelSlip = true;
						} else if (!Train.Cars[i].Derailed) {
							FrictionBrakeAcceleration += 0.5 * a * factor;
						}
						if (a >= wheelSlipAccelerationMotorRear) {
							Train.Cars[i].RearAxle.CurrentWheelSlip = true;
						} else {
							FrictionBrakeAcceleration += 0.5 * a * factor;
						}
					}
				} else {
					Train.Cars[i].Specs.CurrentAccelerationOutput = 0.0;
				}
				// perceived speed
				{
					double target;
					if (wheellock) {
						target = 0.0;
					} else if (wheelspin == 0.0) {
						target = Train.Cars[i].Specs.CurrentSpeed;
					} else {
						target = Train.Cars[i].Specs.CurrentSpeed + wheelspin / 2500.0;
					}
					double diff = target - Train.Cars[i].Specs.CurrentPerceivedSpeed;
					double rate = (diff < 0.0 ? 5.0 : 1.0) * Game.RouteAccelerationDueToGravity * TimeElapsed;
					rate *= 1.0 - 0.7 / (diff * diff + 1.0);
					double factor = rate * rate;
					factor = 1.0 - factor / (factor + 1000.0);
					rate *= factor;
					if (diff >= -rate & diff <= rate) {
						Train.Cars[i].Specs.CurrentPerceivedSpeed = target;
					} else {
						Train.Cars[i].Specs.CurrentPerceivedSpeed += rate * (double)Math.Sign(diff);
					}
				}
				// perceived traveled distance
				Train.Cars[i].Specs.CurrentPerceivedTraveledDistance += Math.Abs(Train.Cars[i].Specs.CurrentPerceivedSpeed) * TimeElapsed;
				// calculate new speed
				{
					int d = Math.Sign(Train.Cars[i].Specs.CurrentSpeed);
					double a = PowerRollingCouplerAcceleration;
					double b = FrictionBrakeAcceleration;
					if (Math.Abs(a) < b) {
						if (Math.Sign(a) == d) {
							if (d == 0) {
								NewSpeeds[i] = 0.0;
							} else {
								double c = (b - Math.Abs(a)) * TimeElapsed;
								if (Math.Abs(Train.Cars[i].Specs.CurrentSpeed) > c) {
									NewSpeeds[i] = Train.Cars[i].Specs.CurrentSpeed - (double)d * c;
								} else {
									NewSpeeds[i] = 0.0;
								}
							}
						} else {
							double c = (Math.Abs(a) + b) * TimeElapsed;
							if (Math.Abs(Train.Cars[i].Specs.CurrentSpeed) > c) {
								NewSpeeds[i] = Train.Cars[i].Specs.CurrentSpeed - (double)d * c;
							} else {
								NewSpeeds[i] = 0.0;
							}
						}
					} else {
						NewSpeeds[i] = Train.Cars[i].Specs.CurrentSpeed + (a - b * (double)d) * TimeElapsed;
					}
				}
			}
			// calculate center of mass position
			double[] CenterOfCarPositions = new double[Train.Cars.Length];
			double CenterOfMassPosition = 0.0;
			double TrainMass = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++) {
				double pr = Train.Cars[i].RearAxle.Follower.TrackPosition - Train.Cars[i].RearAxlePosition;
				double pf = Train.Cars[i].FrontAxle.Follower.TrackPosition - Train.Cars[i].FrontAxlePosition;
				CenterOfCarPositions[i] = 0.5 * (pr + pf);
				CenterOfMassPosition += CenterOfCarPositions[i] * Train.Cars[i].Specs.MassCurrent;
				TrainMass += Train.Cars[i].Specs.MassCurrent;
			}
			if (TrainMass != 0.0) {
				CenterOfMassPosition /= TrainMass;
			}
			{ // coupler
				// determine closest cars
				int p = -1; // primary car index
				int s = -1; // secondary car index
				{
					double PrimaryDistance = double.MaxValue;
					for (int i = 0; i < Train.Cars.Length; i++) {
						double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
						if (d < PrimaryDistance) {
							PrimaryDistance = d;
							p = i;
						}
					}
					double SecondDistance = double.MaxValue;
					for (int i = p - 1; i <= p + 1; i++) {
						if (i >= 0 & i < Train.Cars.Length & i != p) {
							double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
							if (d < SecondDistance) {
								SecondDistance = d;
								s = i;
							}
						}
					}
					if (s >= 0 && PrimaryDistance <= 0.25 * (PrimaryDistance + SecondDistance)) {
						s = -1;
					}
				}
				// coupler
				bool[] CouplerCollision = new bool[Train.Couplers.Length];
				int cf, cr;
				if (s >= 0) {
					// use two cars as center of mass
					if (p > s) {
						int t = p; p = s; s = t;
					}
					double min = Train.Couplers[p].MinimumDistanceBetweenCars;
					double max = Train.Couplers[p].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[p] - CenterOfCarPositions[s] - 0.5 * (Train.Cars[p].Length + Train.Cars[s].Length);
					if (d < min) {
						double t = (min - d) / (Train.Cars[p].Specs.MassCurrent + Train.Cars[s].Specs.MassCurrent);
						double tp = t * Train.Cars[s].Specs.MassCurrent;
						double ts = t * Train.Cars[p].Specs.MassCurrent;
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontAxle.Follower, Train.Cars[p].FrontAxle.Follower.TrackPosition + tp, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontBogie.FrontAxle.Follower, Train.Cars[p].FrontBogie.FrontAxle.Follower.TrackPosition + tp, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontBogie.RearAxle.Follower, Train.Cars[p].FrontBogie.RearAxle.Follower.TrackPosition + tp, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearAxle.Follower, Train.Cars[p].RearAxle.Follower.TrackPosition + tp, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearBogie.FrontAxle.Follower, Train.Cars[p].RearBogie.FrontAxle.Follower.TrackPosition + tp, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearBogie.RearAxle.Follower, Train.Cars[p].RearBogie.RearAxle.Follower.TrackPosition + tp, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontAxle.Follower, Train.Cars[s].FrontAxle.Follower.TrackPosition - ts, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontBogie.FrontAxle.Follower, Train.Cars[s].FrontBogie.FrontAxle.Follower.TrackPosition - ts, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontBogie.RearAxle.Follower, Train.Cars[s].FrontBogie.RearAxle.Follower.TrackPosition - ts, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearAxle.Follower, Train.Cars[s].RearAxle.Follower.TrackPosition - ts, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearBogie.FrontAxle.Follower, Train.Cars[s].RearBogie.FrontAxle.Follower.TrackPosition - ts, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearBogie.RearAxle.Follower, Train.Cars[s].RearBogie.RearAxle.Follower.TrackPosition - ts, false, false);
						CenterOfCarPositions[p] += tp;
						CenterOfCarPositions[s] -= ts;
						CouplerCollision[p] = true;
					} else if (d > max & !Train.Cars[p].Derailed & !Train.Cars[s].Derailed) {
						double t = (d - max) / (Train.Cars[p].Specs.MassCurrent + Train.Cars[s].Specs.MassCurrent);
						double tp = t * Train.Cars[s].Specs.MassCurrent;
						double ts = t * Train.Cars[p].Specs.MassCurrent;
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontAxle.Follower, Train.Cars[p].FrontAxle.Follower.TrackPosition - tp, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontBogie.FrontAxle.Follower, Train.Cars[p].FrontBogie.FrontAxle.Follower.TrackPosition - tp, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].FrontBogie.RearAxle.Follower, Train.Cars[p].FrontBogie.RearAxle.Follower.TrackPosition - tp, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearAxle.Follower, Train.Cars[p].RearAxle.Follower.TrackPosition - tp, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearBogie.FrontAxle.Follower, Train.Cars[p].RearBogie.FrontAxle.Follower.TrackPosition - tp, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[p].RearBogie.RearAxle.Follower, Train.Cars[p].RearBogie.RearAxle.Follower.TrackPosition - tp, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontAxle.Follower, Train.Cars[s].FrontAxle.Follower.TrackPosition + ts, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontBogie.FrontAxle.Follower, Train.Cars[s].FrontBogie.FrontAxle.Follower.TrackPosition + ts, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].FrontBogie.RearAxle.Follower, Train.Cars[s].FrontBogie.RearAxle.Follower.TrackPosition + ts, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearAxle.Follower, Train.Cars[s].RearAxle.Follower.TrackPosition + ts, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearBogie.FrontAxle.Follower, Train.Cars[s].RearBogie.FrontAxle.Follower.TrackPosition + ts, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[s].RearBogie.RearAxle.Follower, Train.Cars[s].RearBogie.RearAxle.Follower.TrackPosition + ts, false, false);

						CenterOfCarPositions[p] -= tp;
						CenterOfCarPositions[s] += ts;
						CouplerCollision[p] = true;
					}
					cf = p;
					cr = s;
				} else {
					// use one car as center of mass
					cf = p;
					cr = p;
				}
				// front cars
				for (int i = cf - 1; i >= 0; i--) {
					double min = Train.Couplers[i].MinimumDistanceBetweenCars;
					double max = Train.Couplers[i].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[i] - CenterOfCarPositions[i + 1] - 0.5 * (Train.Cars[i].Length + Train.Cars[i + 1].Length);
					if (d < min) {
						double t = min - d + 0.0001;
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontAxle.Follower, Train.Cars[i].FrontAxle.Follower.TrackPosition + t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.FrontAxle.Follower, Train.Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition + t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.RearAxle.Follower, Train.Cars[i].FrontBogie.RearAxle.Follower.TrackPosition + t, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearAxle.Follower, Train.Cars[i].RearAxle.Follower.TrackPosition + t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.FrontAxle.Follower, Train.Cars[i].RearBogie.FrontAxle.Follower.TrackPosition + t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.RearAxle.Follower, Train.Cars[i].RearBogie.RearAxle.Follower.TrackPosition + t, false, false);

						CenterOfCarPositions[i] += t;
						CouplerCollision[i] = true;
					} else if (d > max & !Train.Cars[i].Derailed & !Train.Cars[i + 1].Derailed) {
						double t = d - max + 0.0001;
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontAxle.Follower, Train.Cars[i].FrontAxle.Follower.TrackPosition - t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.FrontAxle.Follower, Train.Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition - t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.RearAxle.Follower, Train.Cars[i].FrontBogie.RearAxle.Follower.TrackPosition - t, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearAxle.Follower, Train.Cars[i].RearAxle.Follower.TrackPosition - t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.FrontAxle.Follower, Train.Cars[i].RearBogie.FrontAxle.Follower.TrackPosition - t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.RearAxle.Follower, Train.Cars[i].RearBogie.RearAxle.Follower.TrackPosition - t, false, false);
						CenterOfCarPositions[i] -= t;
						CouplerCollision[i] = true;
					}
				}
				// rear cars
				for (int i = cr + 1; i < Train.Cars.Length; i++) {
					double min = Train.Couplers[i - 1].MinimumDistanceBetweenCars;
					double max = Train.Couplers[i - 1].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[i - 1] - CenterOfCarPositions[i] - 0.5 * (Train.Cars[i].Length + Train.Cars[i - 1].Length);
					if (d < min) {
						double t = min - d + 0.0001;
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontAxle.Follower, Train.Cars[i].FrontAxle.Follower.TrackPosition - t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.FrontAxle.Follower, Train.Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition - t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.RearAxle.Follower, Train.Cars[i].FrontBogie.RearAxle.Follower.TrackPosition - t, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearAxle.Follower, Train.Cars[i].RearAxle.Follower.TrackPosition - t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.FrontAxle.Follower, Train.Cars[i].RearBogie.FrontAxle.Follower.TrackPosition - t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.RearAxle.Follower, Train.Cars[i].RearBogie.RearAxle.Follower.TrackPosition - t, false, false);
						CenterOfCarPositions[i] -= t;
						CouplerCollision[i - 1] = true;
					} else if (d > max & !Train.Cars[i].Derailed & !Train.Cars[i - 1].Derailed) {
						double t = d - max + 0.0001;
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontAxle.Follower, Train.Cars[i].FrontAxle.Follower.TrackPosition + t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.FrontAxle.Follower, Train.Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition + t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].FrontBogie.RearAxle.Follower, Train.Cars[i].FrontBogie.RearAxle.Follower.TrackPosition + t, false, false);

						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearAxle.Follower, Train.Cars[i].RearAxle.Follower.TrackPosition + t, false, false);
						//
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.FrontAxle.Follower, Train.Cars[i].RearBogie.FrontAxle.Follower.TrackPosition + t, false, false);
						TrackManager.UpdateTrackFollower(ref Train.Cars[i].RearBogie.RearAxle.Follower, Train.Cars[i].RearBogie.RearAxle.Follower.TrackPosition + t, false, false);

						CenterOfCarPositions[i] += t;
						CouplerCollision[i - 1] = true;
					}
				}
				// update speeds
				for (int i = 0; i < Train.Couplers.Length; i++) {
					if (CouplerCollision[i]) {
						int j;
						for (j = i + 1; j < Train.Couplers.Length; j++) {
							if (!CouplerCollision[j]) {
								break;
							}
						}
						double v = 0.0;
						double m = 0.0;
						for (int k = i; k <= j; k++) {
							v += NewSpeeds[k] * Train.Cars[k].Specs.MassCurrent;
							m += Train.Cars[k].Specs.MassCurrent;
						}
						if (m != 0.0) {
							v /= m;
						}
						for (int k = i; k <= j; k++) {
							if (Interface.CurrentOptions.Derailments && Math.Abs(v - NewSpeeds[k]) > 0.5 * Game.CriticalCollisionSpeedDifference) {
								Train.Cars[k].Derailed = true;
							}
							NewSpeeds[k] = v;
						}
						i = j - 1;
					}
				}
			}
			// update average data
			Train.Specs.CurrentAverageSpeed = 0.0;
			Train.Specs.CurrentAverageAcceleration = 0.0;
			Train.Specs.CurrentAverageJerk = 0.0;
			double invtime = TimeElapsed != 0.0 ? 1.0 / TimeElapsed : 1.0;
			for (int i = 0; i < Train.Cars.Length; i++) {
				Train.Cars[i].Specs.CurrentAcceleration = (NewSpeeds[i] - Train.Cars[i].Specs.CurrentSpeed) * invtime;
				Train.Cars[i].Specs.CurrentSpeed = NewSpeeds[i];
				Train.Specs.CurrentAverageSpeed += NewSpeeds[i];
				Train.Specs.CurrentAverageAcceleration += Train.Cars[i].Specs.CurrentAcceleration;
			}
			double invcarlen = 1.0 / (double)Train.Cars.Length;
			Train.Specs.CurrentAverageSpeed *= invcarlen;
			Train.Specs.CurrentAverageAcceleration *= invcarlen;
		}
		
		// update train mass
		private static void UpdateTrainMassFromPassengerRatio(Train Train) {
			for (int i = 0; i < Train.Cars.Length; i++) {
				double area = Train.Cars[i].Width * Train.Cars[i].Length;
				const double passengersPerArea = 1.0;
				double randomFactor = 0.9 + 0.2 * Program.RandomNumberGenerator.NextDouble();
				double passengers = Math.Round(randomFactor * Train.Passengers.PassengerRatio * passengersPerArea * area);
				const double massPerPassenger = 70.0;
				double passengerMass = passengers * massPerPassenger;
				Train.Cars[i].Specs.MassCurrent = Train.Cars[i].Specs.MassEmpty + passengerMass;
			}
		}
		
		// un-derail train
		internal static void UnderailTrains()
		{
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UnderailTrain(Trains[i]);
			});
			//for (int i = 0; i < Trains.Length; i++) {
			//	UnderailTrain(Trains[i]);
			//}
		}
		internal static void UnderailTrain(Train Train) {
			for (int i = 0; i < Train.Cars.Length; i++) {
				Train.Cars[i].Specs.CurrentRollDueToTopplingAngle = 0.0;
				Train.Cars[i].Derailed = false;
			}
		}
		
		// jump train
		internal static void JumpTrain(Train train, int stationIndex) {
			int stopIndex = Game.GetStopIndex(stationIndex, train.Cars.Length);
			if (stopIndex >= 0) {
				if (train == PlayerTrain) {
					if (train.Plugin != null) {
						train.Plugin.BeginJump((OpenBveApi.Runtime.InitializationModes)Game.TrainStart);
					}
				}
				for (int h = 0; h < train.Cars.Length; h++) {
					train.Cars[h].Specs.CurrentSpeed = 0.0;
				}
				double d = Game.Stations[stationIndex].Stops[stopIndex].TrackPosition - train.Cars[0].FrontAxle.Follower.TrackPosition + train.Cars[0].FrontAxlePosition - 0.5 * train.Cars[0].Length;
				if (train == PlayerTrain) {
					TrackManager.SuppressSoundEvents = true;
				}
				while (d != 0.0) {
					double x;
					if (Math.Abs(d) > 1.0) {
						x = (double)Math.Sign(d);
					} else {
						x = d;
					}
					for (int h = 0; h < train.Cars.Length; h++) {
						TrainManager.MoveCar(train, h, x, 0.0);
					}
					if (Math.Abs(d) >= 1.0) {
						d -= x;
					} else {
						break;
					}
				}
				if (train == PlayerTrain) {
					TrainManager.UnderailTrains();
					TrackManager.SuppressSoundEvents = false;
				}
				if (train.Specs.CurrentEmergencyBrake.Driver) {
					TrainManager.ApplyNotch(train, 0, false, 0, true);
				} else {
					TrainManager.ApplyNotch(train, 0, false, train.Specs.MaximumBrakeNotch, false);
					TrainManager.ApplyAirBrakeHandle(train, TrainManager.AirBrakeHandleState.Service);
				}
				if (Game.Sections.Length > 0) {
					Game.UpdateSection(Game.Sections.Length - 1);
				}
				if (train == PlayerTrain) {
					if (Game.CurrentScore.ArrivalStation <= stationIndex) {
						Game.CurrentScore.ArrivalStation = stationIndex + 1;
					}
				}
				if (train == PlayerTrain) {
					if (Game.Stations[stationIndex].ArrivalTime >= 0.0) {
						Game.SecondsSinceMidnight = Game.Stations[stationIndex].ArrivalTime;
					} else if (Game.Stations[stationIndex].DepartureTime >= 0.0) {
						Game.SecondsSinceMidnight = Game.Stations[stationIndex].DepartureTime - Game.Stations[stationIndex].StopTime;
					}
				}
				for (int i = 0; i < train.Cars.Length; i++) {
					train.Cars[i].Specs.AnticipatedLeftDoorsOpened = Game.Stations[stationIndex].OpenLeftDoors;
					train.Cars[i].Specs.AnticipatedRightDoorsOpened = Game.Stations[stationIndex].OpenRightDoors;
				}
				if (train == PlayerTrain) {
					Game.CurrentScore.DepartureStation = stationIndex;
					Game.CurrentInterface = Game.InterfaceType.Normal;
					Game.Messages = new Game.Message[] { };
				}
				ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
				TrainManager.UpdateTrainObjects(0.0, true);
				if (train == PlayerTrain) {
					if (train.Plugin != null) {
						train.Plugin.EndJump();
					}
				}
			}
		}
		
		// update train physics and controls
		private static void UpdateTrainPhysicsAndControls(Train Train, double TimeElapsed) {
			if (TimeElapsed == 0.0) {
				return;
			}
			// move cars
			for (int i = 0; i < Train.Cars.Length; i++) {
				MoveCar(Train, i, Train.Cars[i].Specs.CurrentSpeed * TimeElapsed, TimeElapsed);
				if (Train.State == TrainState.Disposed) {
					return;
				}
			}
			// update station and doors
			UpdateTrainStation(Train, TimeElapsed);
			UpdateTrainDoors(Train, TimeElapsed);
			// delayed handles
			{
				// power notch
				if (Train.Specs.CurrentPowerNotch.DelayedChanges.Length == 0) {
					if (Train.Specs.CurrentPowerNotch.Safety < Train.Specs.CurrentPowerNotch.Actual) {
						if (Train.Specs.PowerNotchReduceSteps <= 1) {
							Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Actual - 1, Train.Specs.DelayPowerDown);
						} else if (Train.Specs.CurrentPowerNotch.Safety + Train.Specs.PowerNotchReduceSteps <= Train.Specs.CurrentPowerNotch.Actual | Train.Specs.CurrentPowerNotch.Safety == 0) {
							Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerDown);
						}
					} else if (Train.Specs.CurrentPowerNotch.Safety > Train.Specs.CurrentPowerNotch.Actual) {
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Actual + 1, Train.Specs.DelayPowerUp);
					}
				} else {
					int m = Train.Specs.CurrentPowerNotch.DelayedChanges.Length - 1;
					if (Train.Specs.CurrentPowerNotch.Safety < Train.Specs.CurrentPowerNotch.DelayedChanges[m].Value) {
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerDown);
					} else if (Train.Specs.CurrentPowerNotch.Safety > Train.Specs.CurrentPowerNotch.DelayedChanges[m].Value) {
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerUp);
					}
				}
				if (Train.Specs.CurrentPowerNotch.DelayedChanges.Length >= 1) {
					if (Train.Specs.CurrentPowerNotch.DelayedChanges[0].Time <= Game.SecondsSinceMidnight) {
						Train.Specs.CurrentPowerNotch.Actual = Train.Specs.CurrentPowerNotch.DelayedChanges[0].Value;
						Train.Specs.CurrentPowerNotch.RemoveChanges(1);
					}
				}
			}
			{
				// brake notch
				int sec = Train.Specs.CurrentEmergencyBrake.Safety ? Train.Specs.MaximumBrakeNotch : Train.Specs.CurrentBrakeNotch.Safety;
				if (Train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0) {
					if (sec < Train.Specs.CurrentBrakeNotch.Actual) {
						Train.Specs.CurrentBrakeNotch.AddChange(Train, Train.Specs.CurrentBrakeNotch.Actual - 1, Train.Specs.DelayBrakeDown);
					} else if (sec > Train.Specs.CurrentBrakeNotch.Actual) {
						Train.Specs.CurrentBrakeNotch.AddChange(Train, Train.Specs.CurrentBrakeNotch.Actual + 1, Train.Specs.DelayBrakeUp);
					}
				} else {
					int m = Train.Specs.CurrentBrakeNotch.DelayedChanges.Length - 1;
					if (sec < Train.Specs.CurrentBrakeNotch.DelayedChanges[m].Value) {
						Train.Specs.CurrentBrakeNotch.AddChange(Train, sec, Train.Specs.DelayBrakeDown);
					} else if (sec > Train.Specs.CurrentBrakeNotch.DelayedChanges[m].Value) {
						Train.Specs.CurrentBrakeNotch.AddChange(Train, sec, Train.Specs.DelayBrakeUp);
					}
				}
				if (Train.Specs.CurrentBrakeNotch.DelayedChanges.Length >= 1) {
					if (Train.Specs.CurrentBrakeNotch.DelayedChanges[0].Time <= Game.SecondsSinceMidnight) {
						Train.Specs.CurrentBrakeNotch.Actual = Train.Specs.CurrentBrakeNotch.DelayedChanges[0].Value;
						Train.Specs.CurrentBrakeNotch.RemoveChanges(1);
					}
				}
			}
			{
				// air brake handle
				if (Train.Specs.AirBrake.Handle.DelayedValue != AirBrakeHandleState.Invalid) {
					if (Train.Specs.AirBrake.Handle.DelayedTime <= Game.SecondsSinceMidnight) {
						Train.Specs.AirBrake.Handle.Actual = Train.Specs.AirBrake.Handle.DelayedValue;
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Invalid;
					}
				} else {
					if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Release & Train.Specs.AirBrake.Handle.Actual != AirBrakeHandleState.Release) {
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Release;
						Train.Specs.AirBrake.Handle.DelayedTime = Game.SecondsSinceMidnight;
					} else if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Service & Train.Specs.AirBrake.Handle.Actual != AirBrakeHandleState.Service) {
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Service;
						Train.Specs.AirBrake.Handle.DelayedTime = Game.SecondsSinceMidnight;
					} else if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Lap) {
						Train.Specs.AirBrake.Handle.Actual = AirBrakeHandleState.Lap;
					}
				}
			}
			{
				// emergency brake
				if (Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentEmergencyBrake.Actual) {
					double t = Game.SecondsSinceMidnight;
					if (t < Train.Specs.CurrentEmergencyBrake.ApplicationTime) Train.Specs.CurrentEmergencyBrake.ApplicationTime = t;
					if (Train.Specs.CurrentEmergencyBrake.ApplicationTime <= Game.SecondsSinceMidnight) {
						Train.Specs.CurrentEmergencyBrake.Actual = true;
						Train.Specs.CurrentEmergencyBrake.ApplicationTime = double.MaxValue;
					}
				} else if (!Train.Specs.CurrentEmergencyBrake.Safety) {
					Train.Specs.CurrentEmergencyBrake.Actual = false;
				}
			}
			Train.Specs.CurrentHoldBrake.Actual = Train.Specs.CurrentHoldBrake.Driver;
			// update speeds
			UpdateSpeeds(Train, TimeElapsed);
			// run sound
			for (int i = 0; i < Train.Cars.Length; i++) {
				const double factor = 0.04; // 90 km/h -> m/s -> 1/x
				double speed = Math.Abs(Train.Cars[i].Specs.CurrentSpeed);
				if (Train.Cars[i].Derailed) {
					speed = 0.0;
				}
				double pitch = speed * factor;
				double basegain;
				if (Train.Cars[i].Specs.CurrentSpeed == 0.0) {
					if (i != 0) {
						Train.Cars[i].Sounds.RunNextReasynchronizationPosition = Train.Cars[0].FrontAxle.Follower.TrackPosition;
					}
				} else if (Train.Cars[i].Sounds.RunNextReasynchronizationPosition == double.MaxValue & Train.Cars[i].Sounds.FrontAxleRunIndex >= 0) {
					double distance = Math.Abs(Train.Cars[i].FrontAxle.Follower.TrackPosition - World.CameraTrackFollower.TrackPosition);
					const double minDistance = 150.0;
					const double maxDistance = 750.0;
					if (distance > minDistance) {
						if (Train.Cars[i].Sounds.FrontAxleRunIndex < Train.Cars[i].Sounds.Run.Length) {
							Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.Run[Train.Cars[i].Sounds.FrontAxleRunIndex].Buffer;
							if (buffer != null) {
								double duration = Sounds.GetDuration(buffer);
								if (duration > 0.0) {
									double offset = distance > maxDistance ? 25.0 : 300.0;
									Train.Cars[i].Sounds.RunNextReasynchronizationPosition = duration * Math.Ceiling((Train.Cars[0].FrontAxle.Follower.TrackPosition + offset) / duration);
								}
							}
						}
					}
				}
				if (Train.Cars[i].FrontAxle.Follower.TrackPosition >= Train.Cars[i].Sounds.RunNextReasynchronizationPosition) {
					Train.Cars[i].Sounds.RunNextReasynchronizationPosition = double.MaxValue;
					basegain = 0.0;
				} else {
					basegain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;
				}
				for (int j = 0; j < Train.Cars[i].Sounds.Run.Length; j++) {
					if (j == Train.Cars[i].Sounds.FrontAxleRunIndex | j == Train.Cars[i].Sounds.RearAxleRunIndex) {
						Train.Cars[i].Sounds.RunVolume[j] += 3.0 * TimeElapsed;
						if (Train.Cars[i].Sounds.RunVolume[j] > 1.0) Train.Cars[i].Sounds.RunVolume[j] = 1.0;
					} else {
						Train.Cars[i].Sounds.RunVolume[j] -= 3.0 * TimeElapsed;
						if (Train.Cars[i].Sounds.RunVolume[j] < 0.0) Train.Cars[i].Sounds.RunVolume[j] = 0.0;
					}
					double gain = basegain * Train.Cars[i].Sounds.RunVolume[j];
					if (Sounds.IsPlaying(Train.Cars[i].Sounds.Run[j].Source)) {
						if (pitch > 0.01 & gain > 0.001) {
							Train.Cars[i].Sounds.Run[j].Source.Pitch = pitch;
							Train.Cars[i].Sounds.Run[j].Source.Volume = gain;
						} else {
							Sounds.StopSound(Train.Cars[i].Sounds.Run[j].Source);
						}
					} else if (pitch > 0.02 & gain > 0.01) {
						Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.Run[j].Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.Run[j].Position;
							Train.Cars[i].Sounds.Run[j].Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, i, true);
						}
					}
				}
			}
			// motor sound
			for (int i = 0; i < Train.Cars.Length; i++) {
				if (Train.Cars[i].Specs.IsMotorCar) {
					OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.Motor.Position;
					double speed = Math.Abs(Train.Cars[i].Specs.CurrentPerceivedSpeed);
					int idx = (int)Math.Round(speed * Train.Cars[i].Sounds.Motor.SpeedConversionFactor);
					int odir = Train.Cars[i].Sounds.Motor.CurrentAccelerationDirection;
					int ndir = Math.Sign(Train.Cars[i].Specs.CurrentAccelerationOutput);
					for (int h = 0; h < 2; h++) {
						int j = h == 0 ? TrainManager.MotorSound.MotorP1 : TrainManager.MotorSound.MotorP2;
						int k = h == 0 ? TrainManager.MotorSound.MotorB1 : TrainManager.MotorSound.MotorB2;
						if (odir > 0 & ndir <= 0) {
							if (j < Train.Cars[i].Sounds.Motor.Tables.Length) {
								Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
								Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
								Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
							}
						} else if (odir < 0 & ndir >= 0) {
							if (k < Train.Cars[i].Sounds.Motor.Tables.Length) {
								Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[k].Source);
								Train.Cars[i].Sounds.Motor.Tables[k].Source = null;
								Train.Cars[i].Sounds.Motor.Tables[k].Buffer = null;
							}
						}
						if (ndir != 0) {
							if (ndir < 0) j = k;
							if (j < Train.Cars[i].Sounds.Motor.Tables.Length) {
								int idx2 = idx;
								if (idx2 >= Train.Cars[i].Sounds.Motor.Tables[j].Entries.Length) {
									idx2 = Train.Cars[i].Sounds.Motor.Tables[j].Entries.Length - 1;
								}
								if (idx2 >= 0) {
									Sounds.SoundBuffer obuf = Train.Cars[i].Sounds.Motor.Tables[j].Buffer;
									Sounds.SoundBuffer nbuf = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Buffer;
									double pitch = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Pitch;
									double gain = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Gain;
									if (ndir == 1) {
										// power
										double max = Train.Cars[i].Specs.AccelerationCurveMaximum;
										if (max != 0.0) {
											double cur = Train.Cars[i].Specs.CurrentAccelerationOutput;
											if (cur < 0.0) cur = 0.0;
											gain *= Math.Pow(cur / max, 0.25);
										}
									} else if (ndir == -1) {
										// brake
										double max = Train.Cars[i].Specs.BrakeDecelerationAtServiceMaximumPressure;
										if (max != 0.0) {
											double cur = -Train.Cars[i].Specs.CurrentAccelerationOutput;
											if (cur < 0.0) cur = 0.0;
											gain *= Math.Pow(cur / max, 0.25);
										}
									}
									if (obuf != nbuf) {
										Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
										if (nbuf != null) {
											Train.Cars[i].Sounds.Motor.Tables[j].Source = Sounds.PlaySound(nbuf, pitch, gain, pos, Train, i, true);
											Train.Cars[i].Sounds.Motor.Tables[j].Buffer = nbuf;
										} else {
											Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
											Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
										}
									} else if (nbuf != null) {
										if (Train.Cars[i].Sounds.Motor.Tables[j].Source != null) {
											Train.Cars[i].Sounds.Motor.Tables[j].Source.Pitch = pitch;
											Train.Cars[i].Sounds.Motor.Tables[j].Source.Volume = gain;
										}
									} else {
										Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
										Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
										Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
									}
								} else {
									Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
									Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
									Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
								}
							}
						}
					}
					Train.Cars[i].Sounds.Motor.CurrentAccelerationDirection = ndir;
				}
			}
			// safety system
			if (!Game.MinimalisticSimulation | Train != PlayerTrain) {
				UpdateSafetySystem(Train);
			}
			{
				// breaker sound
				bool breaker;
				if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake) {
					breaker = Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Safety >= 1 & Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Release & !Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentHoldBrake.Actual;
				} else {
					breaker = Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Safety >= 1 & Train.Specs.CurrentBrakeNotch.Safety == 0 & !Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentHoldBrake.Actual;
				}
				if (breaker & !Train.Cars[Train.DriverCar].Sounds.BreakerResumed) {
					// resume
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResume.Buffer != null) {
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResume.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResume.Position, Train, Train.DriverCar, false);
					}
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null) {
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Train, Train.DriverCar, false);
					}
					Train.Cars[Train.DriverCar].Sounds.BreakerResumed = true;
				} else if (!breaker & Train.Cars[Train.DriverCar].Sounds.BreakerResumed) {
					// interrupt
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null) {
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Train, Train.DriverCar, false);
					}
					Train.Cars[Train.DriverCar].Sounds.BreakerResumed = false;
				}
			}
			// passengers
			UpdateTrainPassengers(Train, TimeElapsed);
			// signals
			if (Train.CurrentSectionLimit == 0.0) {
				if (Train.Specs.CurrentEmergencyBrake.Driver & Train.Specs.CurrentAverageSpeed > -0.03 & Train.Specs.CurrentAverageSpeed < 0.03) {
					Train.CurrentSectionLimit = 6.94444444444444;
					if (Train == PlayerTrain) {
						string s = Interface.GetInterfaceString("message_signal_proceed");
						double a = (3.6 * Train.CurrentSectionLimit) * Game.SpeedConversionFactor;
						s = s.Replace("[speed]", a.ToString("0", System.Globalization.CultureInfo.InvariantCulture));
						s = s.Replace("[unit]", Game.UnitOfSpeed);
						Game.AddMessage(s, Game.MessageDependency.None, Interface.GameMode.Normal, Game.MessageColor.Red, Game.SecondsSinceMidnight + 5.0);
					}
				}
			}
			// infrequent updates
			Train.InternalTimerTimeElapsed += TimeElapsed;
			if (Train.InternalTimerTimeElapsed > 10.0) {
				Train.InternalTimerTimeElapsed -= 10.0;
				SynchronizeTrain(Train);
				UpdateAtmosphericConstants(Train);
			}
		}

	}
}