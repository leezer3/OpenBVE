using OpenBve.Formats.MsTs;
using TrainManager.Car;
using TrainManager.Car.Systems;

namespace Train.MsTs
{
	internal class Adhesion
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;
		/// <summary>The value used in wheelslip conditions</summary>
		private readonly double WheelSlip;
		/// <summary>The value used in normal conditions</summary>
		private readonly double Normal;
		/// <summary>The value used in sanding conditions</summary>
		private readonly double Sanding;

		internal Adhesion(Block block, CarBase car, bool isSteamEngine)
		{
			baseCar = car;
			try
			{
				WheelSlip = block.ReadSingle();
				Normal = block.ReadSingle();
				Sanding = block.ReadSingle();

			}
			catch
			{
				// Kuju suggested default values
				// see Eng_and_wag_file_reference_guideV2.doc
				if (isSteamEngine)
				{
					WheelSlip = 0.15;
					Normal = 0.3;
					Sanding = 2.0;
				}
				else
				{
					WheelSlip = 0.2;
					Normal = 0.4;
					Sanding = 2.0;
				}
			}
		}
		
		internal double GetWheelslipValue()
		{
			double multiplier;
			// https://www.trainsim.com/forums/forum/general-discussion/traction/68459-msts-diesel-sanding-effective
			// MSTS uses a per-axle drive force calculation, so our traction model's force is divided by the number of axles
			// whereas BVE thinks in terms of the whole car, so for a *hacky* version, we don't need the NumWheels divisor or
			// the mass, as we're not worrying about mass per axle yet...
			// Unlikely to be perfect, but doing it this way resolves massive wheelslip issues

			if (baseCar.ReAdhesionDevice is Sanders sanders && sanders.Active)
			{
				// Per-axle:
				// multiplier = 0.95 * WheelSlip * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
				multiplier = 0.95 * WheelSlip * Sanding;
			}
			else
			{
				if (!baseCar.FrontAxle.CurrentWheelSlip)
				{
					// Per-axle:
					// 	multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.TrailingWheels[0].TotalNumber / baseCar.CurrentMass;
					multiplier = Normal * Sanding;
				}
				else
				{
					// Per-axle:
					// 	multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
					multiplier = WheelSlip * Sanding;
				}
			}

			if (baseCar.TractionModel.MaximumPossibleAcceleration == 0)
			{
				// no possible acceleration, so can't wheelslip!
				return double.MaxValue;
			}

			return baseCar.TractionModel.MaximumPossibleAcceleration * multiplier;
		}
	}
}
