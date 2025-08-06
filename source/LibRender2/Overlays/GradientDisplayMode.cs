namespace LibRender2.Overlays
{
	/// <summary>The available gradient display modes</summary>
	public enum GradientDisplayMode
	{
		/// <summary>No gradient display</summary>
		None =0,
		/// <summary>Gradient is displayed as a percentage at the current location</summary>
		Percentage,
		/// <summary>Gradient change is displayed as N in N</summary>
		UnitOfChange, 
		/// <summary> Gradient change is displayed in per mill format</summary>
		Permil
	}
}
