using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBveApi.Trains;

namespace MotorSoundEditor.Simulation.TrainManager
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
				Program.Sounds.StopAllSounds(this);
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
