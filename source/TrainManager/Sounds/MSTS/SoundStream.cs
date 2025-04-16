using System.Collections.Generic;

namespace TrainManager.MsTsSounds
{
	public class SoundStream
	{
		public List<SoundTrigger> Triggers;

		public MsTsVolumeCurve VolumeCurve;

		public MsTsFrequencyCurve FrequencyCurve;

		public SoundStream()
		{
			Triggers = new List<SoundTrigger>();
		}

		public void Update(double timeElapsed)
		{
			double volume = 1.0, pitch = 1.0;
			if (VolumeCurve != null)
			{
				volume = VolumeCurve.Volume;
			}

			if (FrequencyCurve != null)
			{
				pitch = FrequencyCurve.Pitch;
			}

			for (int i = 0; i < Triggers.Count; i++)
			{
				Triggers[i].Update(timeElapsed, pitch, volume);
			}
		}
	}
}
