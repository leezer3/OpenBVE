using System;
using OpenBveApi.Sounds;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVE5MotorSound : AbstractMotorSound
	{
		/// <summary>Contains all sound buffers</summary>
		public SoundBuffer[] SoundBuffers;
		/// <summary>Contains all sound sources</summary>
		public SoundSource[] SoundSources;
		/// <summary>The motor pitch table</summary>
		public Tuple<double, double[]>[] MotorPitchTable;
		/// <summary>The motor gain table</summary>
		public Tuple<double, double[]>[] MotorGainTable;
		/// <summary>The brake pitch table</summary>
		public Tuple<double, double[]>[] BrakePitchTable;
		/// <summary>The brake gain table</summary>
		public Tuple<double, double[]>[] BrakeGainTable;

		public BVE5MotorSound(CarBase car) : base(car)
		{
			SoundBuffers = Array.Empty<SoundBuffer>();
			SoundSources = Array.Empty<SoundSource>();
		}

		private double GetValue(Tuple<double, double[]>[] values, double speed, int soundIndex)
		{
			Tuple<double, double[]> entry = values[0];
			Tuple<double, double[]> nextEntry = values[0];
			for (int i = 0; i < values.Length - 1; i++)
			{
				double nextSpeed = values[i + 1].Item1;
				entry = values[i];
				nextEntry = values[i + 1];
				if (values[i].Item1 <= speed && nextSpeed >= speed)
				{
					break;
				}

			}
			// linearly interpolated to the next entry
			double interpolate = (nextEntry.Item1 - speed) / (nextEntry.Item1 - entry.Item1);

			// check we have enough values, interpolate to zero if not
			double current = 0, next = 0;
			if (soundIndex < entry.Item2.Length)
			{
				current = entry.Item2[soundIndex];
			}
			if (soundIndex < nextEntry.Item2.Length)
			{
				next = nextEntry.Item2[soundIndex];
			}

			return current + (next - current) * interpolate;
			
		}

		public override void Update(double TimeElapsed)
		{
			if (!Car.TractionModel.ProvidesPower)
			{
				return;
			}
			double speed = Math.Abs(Car.CurrentSpeed) * 3.6; // km/h
			int ndir = Math.Sign(Car.TractionModel.CurrentAcceleration);

			for (int i = 0; i < SoundBuffers.Length; i++)
			{
				double pitch = GetValue(ndir == 1 ? MotorPitchTable : BrakePitchTable, speed, i);
				double gain = GetValue(ndir == 1 ? MotorGainTable : BrakeGainTable, speed, i);

				if (SoundBuffers[i] != null)
				{
					if (pitch <= 0 || gain <= 0)
					{
						TrainManagerBase.currentHost.StopSound(SoundSources[i]);
						continue;
					}

					/*
					 * Initial gain is that specified by the speed step of the current curve
					 * Now multiply that by the actual acceleration as opposed to the max acceleration to find the absolute
					 * gain
					 */
					if (Car.TractionModel.MaximumPossibleAcceleration != 0.0)
					{
						double cur = Car.TractionModel.CurrentAcceleration;
						if (cur < 0.0) cur = 0.0;
						gain *= Math.Pow(cur / Car.TractionModel.MaximumPossibleAcceleration, 0.25);
					}

					if (SoundSources[i] != null && SoundSources[i].State != SoundSourceState.Stopped)
					{

						SoundSources[i].Pitch = pitch;
						SoundSources[i].Volume = gain;
					}
					else
					{
						SoundSources[i] = TrainManagerBase.currentHost.PlaySound(SoundBuffers[i], pitch, gain, Position, Car, true) as SoundSource;
					}
				}
			}
		}

	}
}
