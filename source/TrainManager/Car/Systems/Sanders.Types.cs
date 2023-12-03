// ReSharper disable UnusedMember.Global
namespace TrainManager.Car.Systems
{
	public enum SandersType
	{
		/// <summary>The sanders are active whilst the key is held</summary>
		PressAndHold,
		/// <summary>The sanders toggle on and off</summary>
		Toggle,
		/// <summary>There are N timed shots available when the sanders key is pressed</summary>
		NumberOfShots,
		/// <summary>The sanders activate automatically</summary>
		Automatic,
		/// <summary>No sanders are fitted</summary>
		NotFitted
	}
}
