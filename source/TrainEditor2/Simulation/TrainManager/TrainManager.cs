using System.Collections.Generic;
using TrainEditor2.Models.Sounds;

namespace TrainEditor2.Simulation.TrainManager
{
	public static partial class TrainManager
	{
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		internal static Train PlayerTrain = null;

		internal static List<RunElement> RunSounds = new List<RunElement>();
		internal static List<MotorElement> MotorSounds = new List<MotorElement>();
	}
}
