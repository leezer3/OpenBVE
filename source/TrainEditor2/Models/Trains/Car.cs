using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

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
				get => definedAxles;
				set => SetProperty(ref definedAxles, value);
			}

			internal double FrontAxle
			{
				get => frontAxle;
				set => SetProperty(ref frontAxle, value);
			}

			internal double RearAxle
			{
				get => rearAxle;
				set => SetProperty(ref rearAxle, value);
			}

			internal bool Reversed
			{
				get => reversed;
				set => SetProperty(ref reversed, value);
			}

			internal string Object
			{
				get => _object;
				set => SetProperty(ref _object, value);
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			internal void WriteExtensionsCfg(string fileName, StringBuilder builder, int bogieIndex)
			{
				builder.AppendLine($"[Bogie{bogieIndex.ToString(CultureInfo.InvariantCulture)}]");
				Utilities.WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, Object));

				if (DefinedAxles)
				{
					Utilities.WriteKey(builder, "Axles", RearAxle, FrontAxle);
				}

				Utilities.WriteKey(builder, "Reversed", Reversed.ToString());
			}

			internal void WriteXML(string fileName, XElement carNode, bool isFront)
			{
				XElement bogieElement = new XElement(isFront ? "FrontBogie" : "RearBogie",
					new XElement("FrontAxle", FrontAxle),
					new XElement("RearAxle", RearAxle),
					new XElement("Reversed", Reversed),
					new XElement("Object", Utilities.MakeRelativePath(fileName, Object)));
				carNode.Add(bogieElement);
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

		internal ObservableCollection<ParticleSource> particleSources = new ObservableCollection<ParticleSource>();

		internal double Mass
		{
			get => mass;
			set => SetProperty(ref mass, value);
		}

		internal double Length
		{
			get => length;
			set => SetProperty(ref length, value);
		}

		internal double Width
		{
			get => width;
			set => SetProperty(ref width, value);
		}

		internal double Height
		{
			get => height;
			set => SetProperty(ref height, value);
		}

		internal double CenterOfGravityHeight
		{
			get => centerOfGravityHeight;
			set => SetProperty(ref centerOfGravityHeight, value);
		}

		internal bool DefinedAxles
		{
			get => definedAxles;
			set => SetProperty(ref definedAxles, value);
		}

		internal double FrontAxle
		{
			get => frontAxle;
			set => SetProperty(ref frontAxle, value);
		}

		internal double RearAxle
		{
			get => rearAxle;
			set => SetProperty(ref rearAxle, value);
		}

		internal Bogie FrontBogie
		{
			get => frontBogie;
			set => SetProperty(ref frontBogie, value);
		}

		internal Bogie RearBogie
		{
			get => rearBogie;
			set => SetProperty(ref rearBogie, value);
		}

		internal double ExposedFrontalArea
		{
			get => exposedFrontalArea;
			set => SetProperty(ref exposedFrontalArea, value);
		}

		internal double UnexposedFrontalArea
		{
			get => unexposedFrontalArea;
			set => SetProperty(ref unexposedFrontalArea, value);
		}

		internal Performance Performance
		{
			get => performance;
			set => SetProperty(ref performance, value);
		}

		internal Delay Delay
		{
			get => delay;
			set => SetProperty(ref delay, value);
		}

		internal Move Move
		{
			get => move;
			set => SetProperty(ref move, value);
		}

		internal Brake Brake
		{
			get => brake;
			set => SetProperty(ref brake, value);
		}

		internal Pressure Pressure
		{
			get => pressure;
			set => SetProperty(ref pressure, value);
		}

		internal bool Reversed
		{
			get => reversed;
			set => SetProperty(ref reversed, value);
		}

		internal string Object
		{
			get => _object;
			set => SetProperty(ref _object, value);
		}

		internal bool LoadingSway
		{
			get => loadingSway;
			set => SetProperty(ref loadingSway, value);
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

		public void WriteExtensionsCfg(string fileName, StringBuilder builder, int carIndex)
		{
			builder.AppendLine($"[Car{carIndex.ToString(CultureInfo.InvariantCulture)}]");
			Utilities.WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, Object));
			Utilities.WriteKey(builder, "Length", Length);

			if (DefinedAxles)
			{
				Utilities.WriteKey(builder, "Axles", RearAxle, FrontAxle);
			}

			Utilities.WriteKey(builder, "Reversed", Reversed.ToString());
			Utilities.WriteKey(builder, "LoadingSway", LoadingSway.ToString());
			FrontBogie.WriteExtensionsCfg(fileName, builder, carIndex * 2);
			RearBogie.WriteExtensionsCfg(fileName, builder, carIndex * 2 + 1);
		}

		public abstract void WriteXML(string fileName, XElement trainNode, Train train, int i);
	}

	internal class MotorCar : Car
	{
		private Acceleration acceleration;
		private Motor motor;

		internal Acceleration Acceleration
		{
			get => acceleration;
			set => SetProperty(ref acceleration, value);
		}

		internal Motor Motor
		{
			get => motor;
			set => SetProperty(ref motor, value);
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

		public override void WriteXML(string fileName, XElement trainNode, Train train, int i)
		{
			XElement carElement = new XElement("Car",
				new XElement("Mass", Mass),
				new XElement("Length", Length),
				new XElement("Width", Width),
				new XElement("Height", Height),
				new XElement("CenterOfGravityHeight", CenterOfGravityHeight),
				new XElement("FrontAxle", FrontAxle),
				new XElement("RearAxle", RearAxle),
				new XElement("ExposedFrontalArea", ExposedFrontalArea),
				new XElement("UnexposedFrontalArea", UnexposedFrontalArea),
				new XElement("Reversed", Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, Object)),
				new XElement("LoadingSway", LoadingSway)
			);
			Brake.WriteXML(fileName, carElement, true);
			FrontBogie.WriteXML(fileName, carElement, true);
			RearBogie.WriteXML(fileName, carElement, false);
			Acceleration.WriteXML(fileName, carElement);
			if (i == train.Cab.DriverCar)
			{
				train.Cab.WriteXML(fileName, carElement);
			}

			for (int p = 0; p < particleSources.Count; p++)
			{
				particleSources[p].WriteXML(fileName, carElement);
			}
			trainNode.Add(carElement);
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

		public override void WriteXML(string fileName, XElement trainNode, Train train, int i)
		{
			XElement carElement = new XElement("Car",
				new XElement("Mass", Mass),
				new XElement("Length", Length),
				new XElement("Width", Width),
				new XElement("Height", Height),
				new XElement("CenterOfGravityHeight", CenterOfGravityHeight),
				new XElement("FrontAxle", FrontAxle),
				new XElement("RearAxle", RearAxle),
				new XElement("ExposedFrontalArea", ExposedFrontalArea),
				new XElement("UnexposedFrontalArea", UnexposedFrontalArea),
				new XElement("Reversed", Reversed),
				new XElement("Object", Utilities.MakeRelativePath(fileName, Object)),
				new XElement("LoadingSway", LoadingSway)
			);
			Brake.WriteXML(fileName, carElement, false);
			FrontBogie.WriteXML(fileName, carElement, true);
			RearBogie.WriteXML(fileName, carElement, false);
			if (i == train.Cab.DriverCar)
			{
				train.Cab.WriteXML(fileName, carElement);
			}

			for (int p = 0; p < particleSources.Count; p++)
			{
				particleSources[p].WriteXML(fileName, carElement);
			}
			trainNode.Add(carElement);
		}
	}
}
