using System;
using OpenBveApi.World;
using Prism.Mvvm;
using TrainManager.Car;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Car section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal abstract class Car : BindableBase, ICloneable
	{
		internal class Bogie : BindableBase, ICloneable
		{
			private bool definedAxles;
			private Quantity.Length frontAxle;
			private Quantity.Length rearAxle;
			private bool reversed;
			private string _object;

			internal bool DefinedAxles
			{
				get
				{
					return definedAxles;
				}
				set
				{
					SetProperty(ref definedAxles, value);
				}
			}

			internal Quantity.Length FrontAxle
			{
				get
				{
					return frontAxle;
				}
				set
				{
					SetProperty(ref frontAxle, value);
				}
			}

			internal Quantity.Length RearAxle
			{
				get
				{
					return rearAxle;
				}
				set
				{
					SetProperty(ref rearAxle, value);
				}
			}

			internal bool Reversed
			{
				get
				{
					return reversed;
				}
				set
				{
					SetProperty(ref reversed, value);
				}
			}

			internal string Object
			{
				get
				{
					return _object;
				}
				set
				{
					SetProperty(ref _object, value);
				}
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal class Door : BindableBase, ICloneable
		{
			private Quantity.Length width;
			private Quantity.Length maxTolerance;

			internal Quantity.Length Width
			{
				get
				{
					return width;
				}
				set
				{
					SetProperty(ref width, value);
				}
			}

			internal Quantity.Length MaxTolerance
			{
				get
				{
					return maxTolerance;
				}
				set
				{
					SetProperty(ref maxTolerance, value);
				}
			}

			internal Door()
			{
				Width = new Quantity.Length(1000.0, UnitOfLength.Millimeter);
				MaxTolerance = new Quantity.Length(0.0, UnitOfLength.Millimeter);
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}
		
		private Quantity.Mass mass;
		private Quantity.Length length;
		private Quantity.Length width;
		private Quantity.Length height;
		private Quantity.Length centerOfGravityHeight;
		private bool definedAxles;
		private Quantity.Length frontAxle;
		private Quantity.Length rearAxle;
		private Bogie frontBogie;
		private Bogie rearBogie;
		private Quantity.Area exposedFrontalArea;
		private Quantity.Area unexposedFrontalArea;
		private Performance performance;
		private Delay delay;
		private Jerk jerk;
		private Brake brake;
		private Pressure pressure;
		private bool reversed;
		private string _object;
		private bool loadingSway;
		private Door leftDoor;
		private Door rightDoor;
		private ReadhesionDeviceType reAdhesionDevice;

		internal Quantity.Mass Mass
		{
			get
			{
				return mass;
			}
			set
			{
				SetProperty(ref mass, value);
			}
		}

		internal Quantity.Length Length
		{
			get
			{
				return length;
			}
			set
			{
				SetProperty(ref length, value);
			}
		}

		internal Quantity.Length Width
		{
			get
			{
				return width;
			}
			set
			{
				SetProperty(ref width, value);
			}
		}

		internal Quantity.Length Height
		{
			get
			{
				return height;
			}
			set
			{
				SetProperty(ref height, value);
			}
		}

		internal Quantity.Length CenterOfGravityHeight
		{
			get
			{
				return centerOfGravityHeight;
			}
			set
			{
				SetProperty(ref centerOfGravityHeight, value);
			}
		}

		internal bool DefinedAxles
		{
			get
			{
				return definedAxles;
			}
			set
			{
				SetProperty(ref definedAxles, value);
			}
		}

		internal Quantity.Length FrontAxle
		{
			get
			{
				return frontAxle;
			}
			set
			{
				SetProperty(ref frontAxle, value);
			}
		}

		internal Quantity.Length RearAxle
		{
			get
			{
				return rearAxle;
			}
			set
			{
				SetProperty(ref rearAxle, value);
			}
		}

		internal Bogie FrontBogie
		{
			get
			{
				return frontBogie;
			}
			set
			{
				SetProperty(ref frontBogie, value);
			}
		}

		internal Bogie RearBogie
		{
			get
			{
				return rearBogie;
			}
			set
			{
				SetProperty(ref rearBogie, value);
			}
		}

		internal Quantity.Area ExposedFrontalArea
		{
			get
			{
				return exposedFrontalArea;
			}
			set
			{
				SetProperty(ref exposedFrontalArea, value);
			}
		}

		internal Quantity.Area UnexposedFrontalArea
		{
			get
			{
				return unexposedFrontalArea;
			}
			set
			{
				SetProperty(ref unexposedFrontalArea, value);
			}
		}

		internal Performance Performance
		{
			get
			{
				return performance;
			}
			set
			{
				SetProperty(ref performance, value);
			}
		}

		internal Delay Delay
		{
			get
			{
				return delay;
			}
			set
			{
				SetProperty(ref delay, value);
			}
		}

		internal Jerk Jerk
		{
			get
			{
				return jerk;
			}
			set
			{
				SetProperty(ref jerk, value);
			}
		}

		internal Brake Brake
		{
			get
			{
				return brake;
			}
			set
			{
				SetProperty(ref brake, value);
			}
		}

		internal Pressure Pressure
		{
			get
			{
				return pressure;
			}
			set
			{
				SetProperty(ref pressure, value);
			}
		}

		internal bool Reversed
		{
			get
			{
				return reversed;
			}
			set
			{
				SetProperty(ref reversed, value);
			}
		}

		internal string Object
		{
			get
			{
				return _object;
			}
			set
			{
				SetProperty(ref _object, value);
			}
		}

		internal bool LoadingSway
		{
			get
			{
				return loadingSway;
			}
			set
			{
				SetProperty(ref loadingSway, value);
			}
		}

		internal Door LeftDoor
		{
			get
			{
				return leftDoor;
			}
			set
			{
				SetProperty(ref leftDoor, value);
			}
		}

		internal Door RightDoor
		{
			get
			{
				return rightDoor;
			}
			set
			{
				SetProperty(ref rightDoor, value);
			}
		}

		internal ReadhesionDeviceType ReAdhesionDevice
		{
			get
			{
				return reAdhesionDevice;
			}
			set
			{
				SetProperty(ref reAdhesionDevice, value);
			}
		}

		internal Car()
		{
			Mass = new Quantity.Mass(40.0, UnitOfWeight.MetricTonnes);
			Length = new Quantity.Length(20.0);
			Width = new Quantity.Length(2.6);
			Height = new Quantity.Length(3.2);
			CenterOfGravityHeight = new Quantity.Length(1.5);
			DefinedAxles = false;
			FrontAxle = new Quantity.Length(8.0);
			RearAxle = new Quantity.Length(-8.0);
			FrontBogie = new Bogie();
			RearBogie = new Bogie();
			ExposedFrontalArea = new Quantity.Area(5.0);
			UnexposedFrontalArea = new Quantity.Area(1.6);
			Performance = new Performance();
			Delay = new Delay();
			Jerk = new Jerk();
			Brake = new Brake();
			Pressure = new Pressure();
			Reversed = false;
			Object = string.Empty;
			LoadingSway = false;
			LeftDoor = new Door();
			RightDoor = new Door();
			ReAdhesionDevice = ReadhesionDeviceType.TypeA;
		}

		public virtual object Clone()
		{
			Car car = (Car)MemberwiseClone();
			car.FrontBogie = (Bogie)FrontBogie.Clone();
			car.RearBogie = (Bogie)RearBogie.Clone();
			car.Performance = (Performance)Performance.Clone();
			car.Delay = (Delay)Delay.Clone();
			car.Jerk = (Jerk)Jerk.Clone();
			car.Brake = (Brake)Brake.Clone();
			car.Pressure = (Pressure)Pressure.Clone();
			car.LeftDoor = (Door)LeftDoor.Clone();
			car.RightDoor = (Door)RightDoor.Clone();
			return car;
		}
	}

	internal abstract class MotorCar : Car
	{
		private Acceleration acceleration;
		private Motor motor;

		internal Acceleration Acceleration
		{
			get
			{
				return acceleration;
			}
			set
			{
				SetProperty(ref acceleration, value);
			}
		}

		internal Motor Motor
		{
			get
			{
				return motor;
			}
			set
			{
				SetProperty(ref motor, value);
			}
		}

		internal MotorCar()
		{
			Acceleration = new Acceleration();
			Motor = new Motor();
		}

		public override object Clone()
		{
			MotorCar car = (MotorCar)base.Clone();
			car.Acceleration = (Acceleration)Acceleration.Clone();
			car.Motor = (Motor)Motor.Clone();
			return car;
		}
	}

	internal abstract class TrailerCar : Car
	{
	}

	internal class ControlledMotorCar : MotorCar
	{
		private Cab cab;

		internal Cab Cab
		{
			get
			{
				return cab;
			}
			set
			{
				SetProperty(ref cab, value);
			}
		}

		internal ControlledMotorCar()
		{
			Cab = new EmbeddedCab();
		}

		public ControlledMotorCar(MotorCar car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Acceleration = car.Acceleration;
			Motor = car.Motor;
			Cab = new EmbeddedCab();
		}

		public ControlledMotorCar(ControlledTrailerCar car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Acceleration = new Acceleration();
			Motor = new Motor();
			Cab = car.Cab;
		}

		public override object Clone()
		{
			ControlledMotorCar car = (ControlledMotorCar)base.Clone();
			car.Cab = (Cab)Cab.Clone();
			return car;
		}
	}

	internal class UncontrolledMotorCar : MotorCar
	{
		internal UncontrolledMotorCar()
		{
		}

		internal UncontrolledMotorCar(MotorCar car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Acceleration = car.Acceleration;
			Motor = car.Motor;
		}

		internal UncontrolledMotorCar(Car car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Acceleration = new Acceleration();
			Motor = new Motor();
		}
	}

	internal class ControlledTrailerCar : TrailerCar
	{
		private Cab cab;

		internal Cab Cab
		{
			get
			{
				return cab;
			}
			set
			{
				SetProperty(ref cab, value);
			}
		}

		internal ControlledTrailerCar()
		{
			Cab = new EmbeddedCab();
		}

		internal ControlledTrailerCar(ControlledMotorCar car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Cab = car.Cab;
		}

		internal ControlledTrailerCar(Car car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
			Cab = new EmbeddedCab();
		}

		public override object Clone()
		{
			ControlledTrailerCar car = (ControlledTrailerCar)base.Clone();
			car.Cab = (Cab)Cab.Clone();
			return car;
		}
	}

	internal class UncontrolledTrailerCar : TrailerCar
	{
		internal UncontrolledTrailerCar()
		{
		}

		internal UncontrolledTrailerCar(Car car)
		{
			Mass = car.Mass;
			Length = car.Length;
			Width = car.Width;
			Height = car.Height;
			CenterOfGravityHeight = car.CenterOfGravityHeight;
			DefinedAxles = car.DefinedAxles;
			FrontAxle = car.FrontAxle;
			RearAxle = car.RearAxle;
			FrontBogie = car.FrontBogie;
			RearBogie = car.RearBogie;
			ExposedFrontalArea = car.ExposedFrontalArea;
			UnexposedFrontalArea = car.UnexposedFrontalArea;
			Performance = car.Performance;
			Delay = car.Delay;
			Jerk = car.Jerk;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
		}
	}
}
