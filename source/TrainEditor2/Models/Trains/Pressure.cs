using System;
using Prism.Mvvm;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Pressure section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal class Pressure : BindableBase, ICloneable
	{
		private Compressor compressor;
		private MainReservoir mainReservoir;
		private AuxiliaryReservoir auxiliaryReservoir;
		private EqualizingReservoir equalizingReservoir;
		private BrakePipe brakePipe;
		private StraightAirPipe straightAirPipe;
		private BrakeCylinder brakeCylinder;

		internal Compressor Compressor
		{
			get
			{
				return compressor;
			}
			set
			{
				SetProperty(ref compressor, value);
			}
		}

		internal MainReservoir MainReservoir
		{
			get
			{
				return mainReservoir;
			}
			set
			{
				SetProperty(ref mainReservoir, value);
			}
		}

		internal AuxiliaryReservoir AuxiliaryReservoir
		{
			get
			{
				return auxiliaryReservoir;
			}
			set
			{
				SetProperty(ref auxiliaryReservoir, value);
			}
		}

		internal EqualizingReservoir EqualizingReservoir
		{
			get
			{
				return equalizingReservoir;
			}
			set
			{
				SetProperty(ref equalizingReservoir, value);
			}
		}

		internal BrakePipe BrakePipe
		{
			get
			{
				return brakePipe;
			}
			set
			{
				SetProperty(ref brakePipe, value);
			}
		}

		internal StraightAirPipe StraightAirPipe
		{
			get
			{
				return straightAirPipe;
			}
			set
			{
				SetProperty(ref straightAirPipe, value);
			}
		}

		internal BrakeCylinder BrakeCylinder
		{
			get
			{
				return brakeCylinder;
			}
			set
			{
				SetProperty(ref brakeCylinder, value);
			}
		}

		internal Pressure()
		{
			Compressor = new Compressor();
			MainReservoir = new MainReservoir();
			AuxiliaryReservoir = new AuxiliaryReservoir();
			EqualizingReservoir = new EqualizingReservoir();
			BrakePipe = new BrakePipe();
			StraightAirPipe = new StraightAirPipe();
			BrakeCylinder = new BrakeCylinder();
		}

		public object Clone()
		{
			return new Pressure
			{
				Compressor = (Compressor)Compressor.Clone(),
				MainReservoir = (MainReservoir)MainReservoir.Clone(),
				AuxiliaryReservoir = (AuxiliaryReservoir)AuxiliaryReservoir.Clone(),
				EqualizingReservoir = (EqualizingReservoir)EqualizingReservoir.Clone(),
				BrakePipe = (BrakePipe)BrakePipe.Clone(),
				StraightAirPipe = (StraightAirPipe)StraightAirPipe.Clone(),
				BrakeCylinder = (BrakeCylinder)BrakeCylinder.Clone()
			};
		}
	}
}
