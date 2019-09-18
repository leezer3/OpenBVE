using System;
using Prism.Mvvm;

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
			private double frontAxle;
			private double rearAxle;
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

			internal double FrontAxle
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

			internal double RearAxle
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

		private double mass;
		private double length;
		private double width;
		private double height;
		private double centerOfGravityHeight;
		private bool definedAxles;
		private double frontAxle;
		private double rearAxle;
		private Bogie frontBogie;
		private Bogie rearBogie;
		private double exposedFrontalArea;
		private double unexposedFrontalArea;
		private Performance performance;
		private Delay delay;
		private Move move;
		private Brake brake;
		private Pressure pressure;
		private bool reversed;
		private string _object;
		private bool loadingSway;

		internal double Mass
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

		internal double Length
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

		internal double Width
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

		internal double Height
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

		internal double CenterOfGravityHeight
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

		internal double FrontAxle
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

		internal double RearAxle
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

		internal double ExposedFrontalArea
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

		internal double UnexposedFrontalArea
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

		internal Move Move
		{
			get
			{
				return move;
			}
			set
			{
				SetProperty(ref move, value);
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

		internal Car()
		{
			Mass = 40.0;
			Length = 20.0;
			Width = 2.6;
			Height = 3.2;
			CenterOfGravityHeight = 1.5;
			DefinedAxles = false;
			FrontAxle = 8.0;
			RearAxle = -8.0;
			FrontBogie = new Bogie();
			RearBogie = new Bogie();
			ExposedFrontalArea = 5.0;
			UnexposedFrontalArea = 1.6;
			Performance = new Performance();
			Delay = new Delay();
			Move = new Move();
			Brake = new Brake();
			Pressure = new Pressure();
			Reversed = false;
			Object = string.Empty;
			LoadingSway = false;
		}

		public virtual object Clone()
		{
			Car car = (Car)MemberwiseClone();
			car.FrontBogie = (Bogie)FrontBogie.Clone();
			car.RearBogie = (Bogie)RearBogie.Clone();
			car.Performance = (Performance)Performance.Clone();
			car.Delay = (Delay)Delay.Clone();
			car.Move = (Move)Move.Clone();
			car.Brake = (Brake)Brake.Clone();
			car.Pressure = (Pressure)Pressure.Clone();
			return car;
		}
	}

	internal class MotorCar : Car
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

		internal MotorCar(TrailerCar car)
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
			Move = car.Move;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
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

	internal class TrailerCar : Car
	{
		internal TrailerCar()
		{
		}

		internal TrailerCar(MotorCar car)
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
			Move = car.Move;
			Brake = car.Brake;
			Pressure = car.Pressure;
			Reversed = car.Reversed;
			Object = car.Object;
			LoadingSway = car.LoadingSway;
		}
	}
}
