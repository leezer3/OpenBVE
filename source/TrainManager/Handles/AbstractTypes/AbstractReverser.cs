using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents an abstract reverser handle</summary>
	public abstract class AbstractReverser : AbstractHandle
	{
		protected AbstractReverser(TrainBase Train) : base(Train)
		{
		}

		public virtual void ApplyState(ReverserPosition position)
		{

		}

		public abstract void ApplyState(int Value, bool Relative);
	}
}
