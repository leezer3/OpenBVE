// ReSharper disable UnusedMember.Global
namespace TrainManager.Car.Systems
{
	public enum SandersState
	{
		/// <summary>The sanders are inactive</summary>
		Inactive,
		/// <summary>The sanders are active, and sand is being dispensed</summary>
		Active,
		/// <summary>The sanders are active, but the sand hopper is empty</summary>
		ActiveEmpty
	}
}
