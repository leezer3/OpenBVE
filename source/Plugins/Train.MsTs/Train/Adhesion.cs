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
			if (baseCar.ReAdhesionDevice is Sanders sanders && sanders.Active)
			{
				multiplier = 0.95 * WheelSlip * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
			}
			else
			{
				if (!baseCar.FrontAxle.CurrentWheelSlip)
				{
					if (baseCar.DrivingWheels.Count == 0)
					{
						multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.TrailingWheels[0].TotalNumber / baseCar.CurrentMass;
					}
					else
					{
						multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
					}

				}
				else
				{
					if (baseCar.DrivingWheels.Count == 0)
					{
						multiplier = WheelSlip * Sanding * baseCar.CurrentMass / baseCar.TrailingWheels[0].TotalNumber / baseCar.CurrentMass;
					}
					else
					{
						multiplier = WheelSlip * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
					}
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
