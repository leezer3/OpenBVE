using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents an abstract reverser handle</summary>
	public abstract class AbstractReverser
	{
		internal readonly TrainBase baseTrain;
		/// <summary>The notch set by the driver</summary>
		public int Driver;
		/// <summary>The notch set by the safety sytem</summary>
		public int Safety;
		/// <summary>The actual notch, as used by the physics system etc.</summary>
		public int Actual;
		/// <summary>Contains the notch descriptions to be displayed on the in-game UI</summary>
		public string[] NotchDescriptions;
		/// <summary>The max width used in px for the reverser HUD string</summary>
		public double MaxWidth = 48;

		/// <summary>The current notch description</summary>
		public abstract string CurrentNotchDescription
		{
			get;
		}

		protected AbstractReverser(TrainBase Train)
		{
			baseTrain = Train;
		}

		public virtual void ApplyState(ReverserPosition position)
		{

		}

		public abstract void ApplyState(int Value, bool Relative);
	}
}
