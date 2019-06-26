using System;

namespace SoundManager
{
	public abstract partial class SoundsBase
	{
		protected class SoundSourceAttenuation : IComparable<SoundSourceAttenuation>
		{
			public readonly SoundSource Source;
			public double Gain;
			public readonly double Distance;

			public SoundSourceAttenuation(SoundSource source, double gain, double distance)
			{
				Source = source;
				Gain = gain;
				Distance = distance;
			}
			int IComparable<SoundSourceAttenuation>.CompareTo(SoundSourceAttenuation other)
			{
				return other.Gain.CompareTo(Gain);
			}
		}

		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		/// <param name="model">The sound model.</param>
		public void Update(double timeElapsed, SoundModels model)
		{
			//The time elapsed is used to work out the clamp factor
			//If this is zero, or above 0.5, then this causes sounds bugs
			//TODO: This is a nasty hack. Store the previous clamp factor in these cases??
			if (timeElapsed == 0.0 || timeElapsed > 0.5)
			{
				return;
			}

			if (model == SoundModels.Linear)
			{
				UpdateLinearModel(timeElapsed);
			}
			else
			{
				UpdateInverseModel(timeElapsed);
			}
		}

		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		protected abstract void UpdateLinearModel(double timeElapsed);

		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		protected abstract void UpdateInverseModel(double timeElapsed);
	}
}
