namespace Formats.OpenBve
{
	/// <summary>The sections in a train.dat file</summary>
	public enum TrainDatSection
	{
		/// <summary>Defines the acceleration characteristics of the train in each power notch</summary>
		Acceleration,
		/// <summary>Defines the brake co-efficients and other brake physics related values</summary>
		Deceleration,
		Performance = Deceleration,
		/// <summary>Defines various delays associated with power and braking</summary>
		Delay,
		/// <summary>Defines the jerk values associated with power and braking</summary>
		Move,
		/// <summary>Defines the brake type and related settings</summary>
		Brake,
		/// <summary>Defines pressures for the train brake</summary>
		Pressure,
		/// <summary>Defines the handle types and number of notches</summary>
		Handle,
		/// <summary>Defines the location of the driver's eyes within the driver car</summary>
		Cab,
		Cockpit = Cab,
		/// <summary>Defines the number of cars, and associated masses</summary>
		Car,
		/// <summary>Defines which built-in safety systems and other devices are present on the train</summary>
		Device,
		/// <summary>Defines the motor sound curves for P1</summary>
		Motor_P1,
		/// <summary>Defines the motor sound curves for P2</summary>
		Motor_P2,
		/// <summary>Defines the brake sound curves for P1</summary>
		Motor_B1,
		/// <summary>Defines the brake sound curves for P2</summary>
		Motor_B2
	}
}
