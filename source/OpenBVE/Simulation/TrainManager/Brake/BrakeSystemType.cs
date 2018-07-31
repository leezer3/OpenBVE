namespace OpenBve.BrakeSystems
{
	/// <summary>Defines the different types of brake system types</summary>
	enum BrakeSystemType
	{
		/// <summary>The brake command is synchronized on all cars, however the brake pipe is used to distribute air</summary>
		ElectromagneticStraightAirBrake = 0,
		/// <summary>Each car is an independant brake unit, and the brake command is synchronized</summary>
		ElectricCommandBrake = 1,
		/// <summary>The brake pipe distributes air and regulates the appplication throughout the train</summary>
		AutomaticAirBrake = 2
	}
}
