using OpenBveApi;
using OpenBveApi.Motor;

namespace TrainManager.Motor
{
	public class SafetyValve : AbstractComponent
	{
		/// <summary>The pressure at which the safety valve operates</summary>
		public readonly double OperatingPressure;
		/// <summary>The decrease in pressure per second</summary>
		public readonly double PressureDecrease;

		public SafetyValve(TractionModel engine, double operatingPressure, double pressureDecrease) : base(engine)
		{
			OperatingPressure = operatingPressure;
			PressureDecrease = pressureDecrease;
		}

		public override void Update(double timeElapsed)
		{
			if (!baseEngine.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				return;
			}

			if (boiler.CurrentPressure > OperatingPressure)
			{
				Active = true;
				boiler.CurrentPressure -= PressureDecrease * timeElapsed;
			}
			else
			{
				Active = false;
			}
		}
	}
}
