using System;
using System.Xml.Linq;
using OpenBveApi.Math;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Cab : BindableBase, ICloneable
	{
		private Vector3 position;
		private int driverCar;

		internal double PositionX
		{
			get => position.X;
			set => SetProperty(ref position.X, value);
		}

		internal double PositionY
		{
			get => position.Y;
			set => SetProperty(ref position.Y, value);
		}

		internal double PositionZ
		{
			get => position.Z;
			set => SetProperty(ref position.Z, value);
		}

		internal int DriverCar
		{
			get => driverCar;
			set => SetProperty(ref driverCar, value);
		}

		internal Cab()
		{
			PositionX = PositionY = PositionZ = 0.0;
			DriverCar = 0;
		}

		public void WriteXML(string fileName, XElement carNode)
		{
			carNode.Add(new XElement("DriverPosition", position));
			carNode.Add(new XElement("InteriorView", "panel.xml"));
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
