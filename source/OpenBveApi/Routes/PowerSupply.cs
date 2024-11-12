namespace OpenBveApi.Routes
{
	/// <summary>The available power supplies for a track element</summary>
	public class PowerSupply
	{
		/// <summary>The voltage type supplied</summary>
		public readonly PowerSupplyVoltageTypes VoltageType;
		/// <summary>The voltage supplied</summary>
		public readonly double Voltage;
		/// <summary>The maximum amperage supplied</summary>
		public readonly double Amperage;
		/// <summary>The contact height</summary>
		public readonly double ContactHeight;

		/// <summary>Creates a new power supply</summary>
		public PowerSupply(PowerSupplyVoltageTypes voltageType, double voltage, double amperage, double contactHeight)
		{
			VoltageType = voltageType;
			Voltage = voltage;
			Amperage = amperage;
			ContactHeight = contactHeight;
		}
	}
}
