using System;
using OpenBveApi.Trains;

namespace TrainEditor2.Simulation.TrainManager
{
	public static partial class TrainManager
	{
		internal class Train : AbstractTrain
		{
			internal Car Car;

			internal Train()
			{
				Car = new Car(this);
			}

			public override void Dispose()
			{
				Program.SoundApi.StopAllSounds(this);
				Car = null;
			}

			public override double FrontCarTrackPosition()
			{
				throw new NotImplementedException();
			}

			public override double RearCarTrackPosition()
			{
				throw new NotImplementedException();
			}
		}
	}
}
