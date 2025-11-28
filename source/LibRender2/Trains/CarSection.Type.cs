namespace LibRender2.Trains
{
	/// <summary>The type of interior object</summary>
	public enum CarSectionType
	{
		/// <summary>Not visible</summary>
		NotVisible = -1,
		/// <summary>Interior (Displayed using overlay mode</summary>
		Interior = 0,
		/// <summary>Exterior (Displayed as a world object</summary>
		Exterior = 1,
		/// <summary>Left head-out view (Displayed using overlay mode)</summary>
		HeadOutLeft = 2,
		/// <summary>Right head-out view (displayed using overlay mode)</summary>
		HeadOutRight =3
	}
}
