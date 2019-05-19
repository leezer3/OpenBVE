namespace OpenBve.SignalManager
{
	/// <summary>The current route</summary>
	public static class CurrentRoute
	{
		/// <summary>Holds all signal sections within the current route</summary>
		public static Section[] Sections = new Section[] { };

		/// <summary>Holds all .PreTrain instructions for the current route</summary>
		/// <remarks>Must be in distance and time ascending order</remarks>
		public static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };
	}
}
