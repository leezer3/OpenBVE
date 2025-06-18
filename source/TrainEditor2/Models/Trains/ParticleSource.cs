using OpenBveApi.Math;
using System;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Trains
{
	internal class ParticleSource : BindableBase, ICloneable
	{

		private Vector3 location;

		private double initialSize;

		private double maximumSize;

		private Vector3 initialDirection;

		private string textureFile;

		internal double LocationX
		{
			get => location.X;
			set => SetProperty(ref location.X, value);
		}

		internal double LocationY
		{
			get => location.Y;
			set => SetProperty(ref location.Y, value);
		}

		internal double LocationZ
		{
			get => location.Z;
			set => SetProperty(ref location.Z, value);
		}

		internal double InitialSize
		{
			get => initialSize;
			set => SetProperty(ref initialSize, value);
		}

		internal double MaximiumSize
		{
			get => maximumSize;
			set => SetProperty(ref maximumSize, value);
		}

		internal double InitialDirectionX
		{
			get => initialDirection.X;
			set => SetProperty(ref initialDirection.X, value);
		}

		internal double InitialDirectionY
		{
			get => initialDirection.Y;
			set => SetProperty(ref initialDirection.Y, value);
		}

		internal double InitialDirectionZ
		{
			get => initialDirection.Z;
			set => SetProperty(ref initialDirection.Z, value);
		}

		internal string TextureFile
		{
			get => textureFile;
			set => SetProperty(ref textureFile, value);
		}

		internal ParticleSource()
		{
			InitialSize = 0.1;
			MaximiumSize = 1.0;
			InitialDirectionX = 0;
			InitialDirectionY = 1;
			InitialDirectionZ = 0;
		}

		public void WriteXML(string fileName, XElement carNode)
		{
			XElement particleElement = new XElement("ParticleSource",
				new XElement("Location", location),
				new XElement("MaximumSize", initialSize),
				new XElement("MaximumGrownSize", maximumSize),
				new XElement("InitialDirection", initialDirection),
				new XElement("Texture", textureFile)
			);
			carNode.Add(particleElement);
		}
		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
