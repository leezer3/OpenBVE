namespace OpenBve
{
	/// <summary>Represents a raindrop on a windscreen</summary>
	internal struct Raindrop
	{
		/// <summary>Whether the raindrop is currently visible</summary>
		internal bool Visible;
		/// <summary>The remaining life before the drop dries</summary>
		internal double RemainingLife;
	}
}
