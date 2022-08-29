namespace TrainManager.Trains
{
	/// <summary>Represents an abstract light source on the train</summary>
	public class LightSource
	{
		/// <summary>The number of states</summary>
		public int NumberOfStates;
		/// <summary>The current state</summary>
		public int CurrentState;

		/// <summary>Creates a new light source</summary>
		/// <param name="numberOfStates">The number of distinct lit light states</param>
		public LightSource(int numberOfStates)
		{
			this.NumberOfStates = numberOfStates; // state zero is off
		}

		/// <summary>Changes the light source state</summary>
		public void ChangeState()
		{
			CurrentState++;
			if (CurrentState > NumberOfStates)
			{
				CurrentState = 0;
			}
		}

		/// <summary>Sets a specific light source state</summary>
		/// <param name="newState">The new state</param>
		public void SetState(int newState)
		{
			if (newState >= NumberOfStates)
			{
				CurrentState = 0;
				return;
			}

			CurrentState = newState;
		}
	}
}
