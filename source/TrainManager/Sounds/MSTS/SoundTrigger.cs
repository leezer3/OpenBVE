﻿using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public abstract class SoundTrigger
	{
		internal readonly SoundBuffer Buffer;

		internal SoundSource Source;

		internal readonly CarBase Car;

		internal bool Triggered;

		internal SoundTrigger(CarBase car, SoundBuffer buffer)
		{
			Buffer = buffer;
			Car = car;
		}

		public virtual void Update(double timeElapsed, double pitchValue, double volumeValue)
		{

		}

		internal void Stop()
		{
			Source?.Stop();
			Triggered = false;
		}
	}
}
