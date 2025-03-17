using System;
using TrainManager.Car;

namespace TrainManager.Motor
{
    public class DieselEngine : AbstractEngine
    {
		/// <summary>The RPM at maximum power</summary>
	    public readonly double MaxRPM;
		/// <summary>The RPM at minimum power</summary>
		public readonly double MinRPM;
		/// <summary>The RPM at idle power</summary>
		public readonly double IdleRPM;
		/// <summary>The rate at which RPM changes up</summary>
		public readonly double RPMChangeUpRate;
		/// <summary>The rate at which RPM changes down</summary>
		public readonly double RPMChangeDownRate;
		/// <summary>The fuel used at idle</summary>
		public readonly double IdleFuelUse;
		/// <summary>The fuel used at max power</summary>
		public readonly double MaxPowerFuelUse;
	    /// <summary>Gets or sets the current engine RPM</summary>
		public double CurrentRPM
		{
			get => currentRPM;
			private set => currentRPM = value;
		}

		private double currentRPM;
		private double targetRPM;
		private readonly double perNotchRPM;


		public DieselEngine(CarBase car, double idleRPM, double minRPM, double maxRPM, double rpmChangeUpRate, double rpmChangeDownRate, double idleFuelUse = 0, double maxFuelUse = 0) : base (car)
		{
			MinRPM = minRPM;
			MaxRPM = maxRPM;
			IdleRPM = idleRPM;
			perNotchRPM = (MaxRPM - MinRPM) / car.baseTrain.Handles.Power.MaximumDriverNotch;
			RPMChangeUpRate = rpmChangeUpRate;
			RPMChangeDownRate = rpmChangeDownRate;
			IdleFuelUse = idleFuelUse;
			MaxPowerFuelUse = maxFuelUse;
		}

		public override void Update(double timeElapsed)
		{
			if (IsRunning)
			{
				if (BaseCar.baseTrain.Handles.Power.Actual == 0)
				{
					targetRPM = IdleRPM;
				}
				else
				{
					targetRPM = MinRPM + BaseCar.baseTrain.Handles.Power.Actual * perNotchRPM;
				}
			}
			else
			{
				targetRPM = 0;
			}

			if (targetRPM > currentRPM)
			{
				CurrentRPM += RPMChangeUpRate * timeElapsed;
				CurrentRPM = Math.Min(CurrentRPM, targetRPM);

			}
			else if(targetRPM < currentRPM) 
			{
				CurrentRPM -= RPMChangeDownRate * timeElapsed;
				CurrentRPM = Math.Max(CurrentRPM, targetRPM);
			}

			if (FuelTank != null)
			{
				if (currentRPM <= IdleRPM)
				{
					FuelTank.CurrentLevel -= IdleFuelUse * timeElapsed;
				}
				else
				{
					FuelTank.CurrentLevel -= (MaxPowerFuelUse - IdleFuelUse) / BaseCar.baseTrain.Handles.Power.MaximumDriverNotch * BaseCar.baseTrain.Handles.Power.Actual * timeElapsed;
				}
			}
		}
    }
}
