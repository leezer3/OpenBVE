using TrainManager.Trains;

namespace TrainManager.Handles
{
	public class Regulator : AbstractHandle
	{
		/*
		 * Very similar to the PowerHandle, but less BVE related stuff, so use a dedicated class
		 * TODO: Should the current PowerHandle be renamed BVEPowerHandle or something?
		 */
		public Regulator(TrainBase Train) : base(Train)
		{
		}

		public override void Update()
		{
			throw new System.NotImplementedException();
		}
	}
}
