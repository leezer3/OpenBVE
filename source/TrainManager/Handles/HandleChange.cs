namespace TrainManager.Handles
{
	/// <summary>Represents a delayed handle change to be applied</summary>
	public struct HandleChange
	{
		/// <summary>The notch to apply</summary>
		internal int Value;
		/// <summary>The time to apply this</summary>
		internal double Time;
	}
}
