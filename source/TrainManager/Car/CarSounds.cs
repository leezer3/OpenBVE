﻿using System.Collections.Generic;
using SoundManager;

namespace TrainManager.Car
{
	/// <summary>The set of sounds attached to a car</summary>
	public class CarSounds
	{
		/// <summary>The loop sound</summary>
		public CarSound Loop;
		/// <summary>The sounds triggered by the train's plugin</summary>
		public Dictionary<int, CarSound> Plugin = new Dictionary<int, CarSound>();
		/// <summary>The sounds triggered by a request stop</summary>
		public CarSound[] RequestStop;
		/// <summary>The sounds played when a touch sensitive panel control is pressed</summary>
		public Dictionary<int, CarSound> Touch = new Dictionary<int, CarSound>();
		/// <summary>The car sound played in the driver's cab when coupling occurs</summary>
		public CarSound CoupleCab;
		/// <summary>The car sound played in the driver's cab when uncoupling occurs</summary>
		public CarSound UncoupleCab;
	}
}
