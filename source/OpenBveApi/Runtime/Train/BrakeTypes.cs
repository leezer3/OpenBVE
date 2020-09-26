namespace OpenBveApi.Runtime
{
	/// <summary>Represents the type of brake the train uses.</summary>
	public enum BrakeTypes
	{
		/// <summary>The train uses the electromagnetic straight air brake. The numerical value of this constant is 0.</summary>
		ElectromagneticStraightAirBrake = 0,
		/// <summary>The train uses the analog/digital electro-pneumatic air brake without a brake pipe (electric command brake). The numerical value of this constant is 1.</summary>
		ElectricCommandBrake = 1,
		/// <summary>The train uses the automatic air brake with partial release. The numerical value of this constant is 2.</summary>
		AutomaticAirBrake = 2
	}
}
