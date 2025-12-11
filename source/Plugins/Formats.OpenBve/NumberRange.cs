namespace Formats.OpenBve
{
	/// <summary>Describes the allowed ranges for a number</summary>
	public enum NumberRange
	{
		/// <summary>Any number</summary>
		Any,
		/// <summary>Any positive number</summary>
		Positive,
		/// <summary>Zero, or any positive number</summary>
		NonNegative,
		/// <summary>Any non-zero number</summary>
		NonZero
	}
}
