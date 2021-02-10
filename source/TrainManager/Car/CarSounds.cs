using System.Collections.Generic;
using SoundManager;
using TrainManager.Motor;

namespace TrainManager.Car
{
	/// <summary>The set of sounds attached to a car</summary>
	public class CarSounds
	{
		/// <summary>The motor sounds</summary>
		public BVEMotorSound Motor;
		/// <summary>The flange squeal sounds</summary>
		public Dictionary<int, CarSound> Flange = new Dictionary<int, CarSound>();
		/// <summary>The loop sound</summary>
		public CarSound Loop;
		/// <summary>The railtype run sounds</summary>
		public Dictionary<int, CarSound> Run = new Dictionary<int, CarSound>();
		/// <summary>The next track position at which the run sounds will be faded</summary>
		public double RunNextReasynchronizationPosition;
		/// <summary>The left spring sound</summary>
		public CarSound SpringL;
		/// <summary>The right spring sound</summary>
		public CarSound SpringR;
		/// <summary>The sounds triggered by the train's plugin</summary>
		public CarSound[] Plugin;
		/// <summary>The sounds triggered by a request stop</summary>
		public CarSound[] RequestStop;
		/// <summary>The current pitch of the flange sounds</summary>
		public double FlangePitch;
		/// <summary>The angle used to determine whether the spring sounds are to be played</summary>
		public double SpringPlayedAngle;
		/// <summary>The sounds played when a touch sensitive panel control is pressed</summary>
		public CarSound[] Touch;
	}
}
