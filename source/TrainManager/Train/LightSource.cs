namespace TrainManager.Trains
{
	/// <summary>Represents an abstract light source on the train</summary>
	public class LightSource
	{
		/// <summary>The number of states</summary>
		private int numberOfStates;
		/// <summary>The current state</summary>
		public int CurrentState;

		/// <summary>Creates a new light source</summary>
		/// <param name="NumberOfStates">The number of distinct lit light states</param>
		public LightSource(int NumberOfStates)
		{
			numberOfStates = NumberOfStates; // state zero is off
		}

		/// <summary>Changes the light source state</summary>
		public void ChangeState()
		{
			CurrentState++;
			if (CurrentState > numberOfStates)
			{
				CurrentState = 0;
			}

		}
	}
}
