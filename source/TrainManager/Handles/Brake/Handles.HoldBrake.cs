using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents a hold-brake handle</summary>
	public class HoldBrakeHandle
	{
		/// <summary>The notch set by the driver</summary>
		public bool Driver;
		/// <summary>The actual notch</summary>
		public bool Actual;

		private readonly TrainBase baseTrain;

		public HoldBrakeHandle(TrainBase train)
		{
			baseTrain = train;
		}

		public void ApplyState(bool value)
		{
			Driver = value;
			if (baseTrain.Plugin == null) return;
			baseTrain.Plugin.UpdatePower();
			baseTrain.Plugin.UpdateBrake();
		}
	}
}
