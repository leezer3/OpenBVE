using System.Collections.Generic;
using SoundManager;
using TrainManager.Motor;

namespace TrainManager.Car
{
	/// <summary>The set of sounds attached to a car</summary>
	public class CarSounds
	{
		/// <summary>The motor sounds</summary>
		public AbstractMotorSound Motor;
		/// <summary>The flange squeal sounds</summary>
		public Dictionary<int, CarSound> Flange = new Dictionary<int, CarSound>();
		/// <summary>The loop sound</summary>
		public CarSound Loop;
		/// <summary>The railtype run sounds</summary>
		public Dictionary<int, CarSound> Run = new Dictionary<int, CarSound>();
		/// <summary>The next track position at which the run sounds will be faded</summary>
		public double RunNextReasynchronizationPosition;
		/// <summary>The sounds triggered by the train's plugin</summary>
		public Dictionary<int, CarSound> Plugin = new Dictionary<int, CarSound>();
		/// <summary>The sounds triggered by a request stop</summary>
		public CarSound[] RequestStop;
		/// <summary>The current pitch of the flange sounds</summary>
		public double FlangePitch;
		/// <summary>The sounds played when a touch sensitive panel control is pressed</summary>
		public CarSound[] Touch;
	}
}
