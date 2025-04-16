using System;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public class SpeedIncPast : SoundTrigger
	{
		private readonly double speedValue;

		private readonly bool soundLoops;

		private bool triggered;

		public SpeedIncPast(CarBase car, SoundBuffer buffer, double speedValue, bool soundLoops) : base(car, buffer)
		{
			this.speedValue = speedValue;
			this.soundLoops = soundLoops;
		}

		public override void Update(double timeElapsed, double pitchValue, double volumeValue)
		{
			if (Math.Abs(Car.CurrentSpeed) >= speedValue)
			{
				if (Buffer != null)
				{
					if (triggered == false)
					{
						this.Source = TrainManagerBase.currentHost.PlaySound(Buffer, pitchValue, volumeValue, Vector3.Zero, Car, soundLoops) as SoundSource;
					}
				}
				triggered = true;
			}
			else
			{
				triggered = false;
				Source?.Stop();
			}
		}
	}

	public class SpeedDecPast : SoundTrigger
	{
		private readonly double speedValue;

		private readonly bool soundLoops;

		private bool triggered;

		public SpeedDecPast(CarBase car, SoundBuffer buffer, double speedValue, bool soundLoops) : base(car, buffer)
		{
			this.speedValue = speedValue;
			this.soundLoops = soundLoops;
		}

		public override void Update(double timeElapsed, double pitchValue, double volumeValue)
		{
			if (Math.Abs(Car.CurrentSpeed) <= speedValue)
			{
				if (Buffer != null)
				{
					if (triggered == false)
					{
						this.Source = TrainManagerBase.currentHost.PlaySound(Buffer, pitchValue, volumeValue, Vector3.Zero, Car, soundLoops) as SoundSource;
					}
				}
				triggered = true;
			}
			else
			{
				triggered = false;
				Source?.Stop();
			}
		}
	}
}
