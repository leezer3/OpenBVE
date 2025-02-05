namespace RouteManager2.SignalManager
{
	/// <summary>The types of section</summary>
	public enum SectionType
	{
		/// <summary>A section aspect may have any value</summary>
		ValueBased,
		/// <summary>A section aspect may have any value</summary>
		/// <remarks>The section is held at red until the permissive key is pressed</remarks>
		PermissiveValueBased,
		/// <summary>Section aspect count upwards from zero (0,1,2,3....)</summary>
		IndexBased,
		/// <summary>Section aspect count upwards from zero (0,1,2,3....)</summary>
		/// <remarks>The section is held at red until the permissive key is pressed</remarks>
		PermissiveIndexBased

	}
}
