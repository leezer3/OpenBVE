// ReSharper disable UnusedMember.Global
namespace TrainManager.Handles
{
	/// <summary>Describes the available handle spring types</summary>
	public enum SpringType
	{
		/// <summary>The handle is unsprung</summary>
		Unsprung,
		/// <summary>The handle spring is reset by this handle only</summary>
		Single,
		/// <summary>The handle spring is reset by any handle</summary>
		AnyHandle,
		/// <summary>The handle spring is reset by any keypress</summary>
		AnyKey
	}
}
