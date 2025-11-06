namespace TrainManager.Handles
{
	/// <summary>Defines the behaviour of the other handles (Power and brake) when the EB is activated</summary>
	public enum EbHandleBehaviour
	{
		/// <summary>No action is taken</summary>
		NoAction = 0,
		/// <summary>The power handle returns to the neutral position</summary>
		PowerNeutral = 1,
		/// <summary>The reverser handle returns to the neutral position</summary>
		ReverserNeutral = 2,
		/// <summary>Both the power handle and the reverser handle return to the neutral position</summary>
		PowerReverserNeutral = 3
	}
}
