namespace RouteManager2.SignalManager.PreTrain
{
	/// <summary>An instruction for a bogus (non-visible) train, affecting the signalling system</summary>
	public struct BogusPreTrainInstruction
	{
		/// <summary>The track position at which this instruction is placed</summary>
		public double TrackPosition;
		
		/// <summary>The time at which the .PreTrain command specifies the bogus train reaches this position</summary>
		public double Time;
	}
}
