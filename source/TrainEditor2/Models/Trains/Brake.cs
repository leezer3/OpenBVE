using System;
using System.Xml.Linq;
using TrainEditor2.Extensions;
using TrainManager.BrakeSystems;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Brake : BindableBase, ICloneable
	{
		internal enum LocoBrakeTypes
		{
			NotFitted = 0,
			NotchedAirBrake = 1,
			AutomaticAirBrake = 2
		}

		private BrakeSystemType brakeType;
		private LocoBrakeTypes locoBrakeType;
		private EletropneumaticBrakeType brakeControlSystem;
		private double brakeControlSpeed;

		internal BrakeSystemType BrakeType
		{
			get => brakeType;
			set => SetProperty(ref brakeType, value);
		}

		internal LocoBrakeTypes LocoBrakeType
		{
			get => locoBrakeType;
			set => SetProperty(ref locoBrakeType, value);
		}

		internal EletropneumaticBrakeType BrakeControlSystem
		{
			get => brakeControlSystem;
			set => SetProperty(ref brakeControlSystem, value);
		}

		internal double BrakeControlSpeed
		{
			get => brakeControlSpeed;
			set => SetProperty(ref brakeControlSpeed, value);
		}

		internal Brake()
		{
			BrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
			LocoBrakeType = LocoBrakeTypes.NotFitted;
			BrakeControlSystem = EletropneumaticBrakeType.None;
			BrakeControlSpeed = 0.0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public void WriteXML(string fileName, XElement carNode, bool isMainBrake)
		{
			// properties are not currently in TE2
			XElement brakeNode = new XElement("Brake",
				new XElement("MainReservoir",
					new XElement("MinimumPressure", 690000.0),
					new XElement("MaximumPressure", 780000.0)),
				new XElement("AuxiliaryReservoir",
					new XElement("ChargeRate", 200000.0)),
				new XElement("EqualizingReservoir",
					new XElement("ServiceRate", 50000.0),
					new XElement("EmergencyRate", 250000.0),
					new XElement("ChargeRate", 200000.0)),
				new XElement("EqualizingReservoir",
					new XElement("ServiceRate", 50000.0),
					new XElement("EmergencyRate", 250000.0),
					new XElement("ChargeRate", 200000.0)),
				new XElement("BrakePipe",
					new XElement("ServiceRate", 300000.0),
					new XElement("EmergencyRate", 400000.0),
					new XElement("ReleaseRate", 200000.0)),
				new XElement("BrakeCylinder",
					new XElement("ServiceMaximumPressure", 440000.0),
					new XElement("EmergencyMaximumPressure", 440000.0),
					new XElement("EmergencyRate", 75000.0),
					new XElement("ReleaseRate", 50000.0)));

			if (isMainBrake)
			{
				brakeNode.Add(new XElement("Compressor",
					new XElement("Rate", 5000.0)));

			}
			carNode.Add(brakeNode);

		}
	}
}
