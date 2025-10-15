using OpenBveApi.Trains;
using TrainManager.Car;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	/// <summary>The base class for an AirBrake</summary>
	/// <remarks>All air brake types should inherit from this</remarks>
	public abstract class AirBrake : CarBrake
	{
		public Compressor Compressor;

		internal EletropneumaticBrakeType electropneumaticBrakeType;

		public StraightAirPipe StraightAirPipe;

		protected AirBrake(CarBase car, AccelerationCurve[] decelerationCurves) : base(car, decelerationCurves)
		{
		}

		public override void Initialize(TrainStartMode startMode)
		{
			switch (startMode)
			{
				case TrainStartMode.ServiceBrakesAts:
					BrakeCylinder.CurrentPressure = BrakeCylinder.ServiceMaximumPressure;
					BrakePipe.CurrentPressure = BrakePipe.NormalPressure;
					StraightAirPipe.CurrentPressure = BrakeCylinder.ServiceMaximumPressure;
					EqualizingReservoir.CurrentPressure = EqualizingReservoir.NormalPressure;
					break;
				case TrainStartMode.EmergencyBrakesAts:
					BrakeCylinder.CurrentPressure = BrakeCylinder.EmergencyMaximumPressure;
					BrakePipe.CurrentPressure = 0.0;
					StraightAirPipe.CurrentPressure = 0.0;
					EqualizingReservoir.CurrentPressure = 0.0;
					break;
				default:
					BrakeCylinder.CurrentPressure = BrakeCylinder.EmergencyMaximumPressure;
					BrakePipe.CurrentPressure = 0.0;
					StraightAirPipe.CurrentPressure = 0.0;
					EqualizingReservoir.CurrentPressure = 0.0;
					break;
			}
		}
	}
}
