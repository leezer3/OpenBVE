using OpenBveApi.Math;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public class Variable2IncPast : SoundTrigger
	{
		private readonly double speedValue;

		private readonly bool soundLoops;
		
		public Variable2IncPast(CarBase car, SoundBuffer buffer, double speedValue, bool soundLoops) : base(car, buffer)
		{
			this.speedValue = speedValue;
			this.soundLoops = soundLoops;
		}

		public override void Update(double timeElapsed, double pitchValue, double volumeValue)
		{
			if (Car.TractionModel.CurrentPower >= speedValue)
			{
				if (Buffer != null)
				{
					if (Triggered == false)
					{
						this.Source = TrainManagerBase.currentHost.PlaySound(Buffer, pitchValue, volumeValue, Vector3.Zero, Car, soundLoops) as SoundSource;
					}
					else
					{
						this.Source.Pitch = pitchValue;
						this.Source.Volume = volumeValue;
					}
				}
				Triggered = true;
			}
			else
			{
				Stop();
			}
		}
	}

	public class Variable2DecPast : SoundTrigger
	{
		private readonly double speedValue;

		private readonly bool soundLoops;

		public Variable2DecPast(CarBase car, SoundBuffer buffer, double speedValue, bool soundLoops) : base(car, buffer)
		{
			this.speedValue = speedValue;
			this.soundLoops = soundLoops;
		}

		public override void Update(double timeElapsed, double pitchValue, double volumeValue)
		{
			if (Car.TractionModel.CurrentPower <= speedValue)
			{
				if (Buffer != null)
				{
					if (Triggered == false)
					{
						this.Source = TrainManagerBase.currentHost.PlaySound(Buffer, pitchValue, volumeValue, Vector3.Zero, Car, soundLoops) as SoundSource;
					}
					else
					{
						this.Source.Pitch = pitchValue;
						this.Source.Volume = volumeValue;
					}
				}
				Triggered = true;
			}
			else
			{
				Stop();
			}
		}
	}
}
