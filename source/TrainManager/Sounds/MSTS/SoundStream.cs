using OpenBveApi.Runtime;
using System.Collections.Generic;

namespace TrainManager.MsTsSounds
{
	public class SoundStream
	{
		public List<SoundTrigger> Triggers;

		public MsTsVolumeCurve VolumeCurve;

		public MsTsFrequencyCurve FrequencyCurve;
		/// <summary>The camera modes in which this sound stream is active</summary>
		public CameraViewMode ActivationCameraModes;
		/// <summary>The modes in which this sound stream is not active</summary>
		public CameraViewMode DeactivationCameraModes;

		public SoundStream()
		{
			Triggers = new List<SoundTrigger>();
			ActivationCameraModes = CameraViewMode.NotDefined;
			DeactivationCameraModes = CameraViewMode.NotDefined;
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

			bool canActivate = true;
			if (ActivationCameraModes != CameraViewMode.NotDefined)
			{
				canActivate = (ActivationCameraModes & TrainManagerBase.Renderer.Camera.CurrentMode) != 0;
			}

			if (DeactivationCameraModes != CameraViewMode.NotDefined)
			{
				if (canActivate)
				{
					canActivate = (DeactivationCameraModes & TrainManagerBase.Renderer.Camera.CurrentMode) == 0;
				}
			}

			
			for (int i = 0; i < Triggers.Count; i++)
			{
				if (canActivate)
				{
					Triggers[i].Update(timeElapsed, pitch, volume);
				}
				else
				{
					Triggers[i].Stop();
				}
				
			}
		}
	}
}
