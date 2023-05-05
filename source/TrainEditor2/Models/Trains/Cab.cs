using System;
using OpenBveApi.World;
using Prism.Mvvm;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.Models.Trains
{
	/// <summary>
	/// The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
	/// </summary>
	internal abstract class Cab : BindableBase, ICloneable
	{
		private Quantity.Length positionX;
		private Quantity.Length positionY;
		private Quantity.Length positionZ;

		internal Quantity.Length PositionX
		{
			get
			{
				return positionX;
			}
			set
			{
				SetProperty(ref positionX, value);
			}
		}

		internal Quantity.Length PositionY
		{
			get
			{
				return positionY;
			}
			set
			{
				SetProperty(ref positionY, value);
			}
		}

		internal Quantity.Length PositionZ
		{
			get
			{
				return positionZ;
			}
			set
			{
				SetProperty(ref positionZ, value);
			}
		}

		protected Cab()
		{
			PositionX = PositionY = PositionZ = new Quantity.Length(0.0, UnitOfLength.Millimeter);
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}
	}

	internal class EmbeddedCab : Cab
	{
		private Panel panel;

		internal Panel Panel
		{
			get
			{
				return panel;
			}
			set
			{
				SetProperty(ref panel, value);
			}
		}

		internal EmbeddedCab()
		{
			Panel = new Panel();
		}

		internal EmbeddedCab(Cab cab)
		{
			PositionX = cab.PositionX;
			PositionY = cab.PositionY;
			PositionZ = cab.PositionZ;
			Panel = new Panel();
		}

		public override object Clone()
		{
			EmbeddedCab cab = (EmbeddedCab)base.Clone();
			cab.Panel = (Panel)Panel.Clone();
			return cab;
		}
	}

	internal class ExternalCab : Cab
	{
		private CameraRestriction cameraRestriction;
		private string fileName;

		internal CameraRestriction CameraRestriction
		{
			get
			{
				return cameraRestriction;
			}
			set
			{
				SetProperty(ref cameraRestriction, value);
			}
		}

		internal string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				SetProperty(ref fileName, value);
			}
		}

		internal ExternalCab()
		{
			CameraRestriction = new CameraRestriction();
		}

		internal ExternalCab(Cab cab)
		{
			PositionX = cab.PositionX;
			PositionY = cab.PositionY;
			PositionZ = cab.PositionZ;
			CameraRestriction = new CameraRestriction();
		}

		public override object Clone()
		{
			ExternalCab cab = (ExternalCab)base.Clone();
			cab.CameraRestriction = (CameraRestriction)CameraRestriction.Clone();
			return cab;
		}
	}
}
