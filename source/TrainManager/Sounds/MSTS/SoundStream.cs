//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Runtime;
using System.Collections.Generic;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public class SoundStream
	{
		/// <summary>The list of sound triggers within the sound stream</summary>
		public List<SoundTrigger> Triggers;
		/// <summary>The volume curve for the sound stream </summary>
		public MsTsVolumeCurve VolumeCurve;
		/// <summary>The frequency curve for the sound stream</summary>
		public MsTsFrequencyCurve FrequencyCurve;
		/// <summary>The camera modes in which this sound stream is active</summary>
		public readonly CameraViewMode ActivationCameraModes;
		/// <summary>The modes in which this sound stream is not active</summary>
		public readonly CameraViewMode DeactivationCameraModes;
		/// <summary>The base volume</summary>
		public double BaseVolume;

		private SoundSource soundSource;

		private readonly CarBase car;

		public SoundStream(CarBase baseCar, CameraViewMode activationCameraModes, CameraViewMode deactivationCameraModes)
		{
			Triggers = new List<SoundTrigger>();
			ActivationCameraModes = activationCameraModes;
			DeactivationCameraModes = deactivationCameraModes;
			car = baseCar;
			BaseVolume = 1.0;
		}

		public void Update(double timeElapsed)
		{
			double volume = BaseVolume, pitch = 1.0;
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

			/*
			 * To get the playing buffer and loop state, we itinerate through all sound triggers in a stream
			 * in order. If the conditions for a trigger are met, the buffer ref and loop status are updated
			 *
			 * At the end of our loop, if the buffer is not null, either adjust the pitch / gain (if currently
			 * playing) or replace the playing buffer with it
			 * Otherwise, stop the playing buffer.
			 */
			SoundBuffer soundBuffer = null;
			if (soundSource != null)
			{
				soundBuffer = soundSource.Buffer;
			}
			bool loops = false;

			for (int i = 0; i < Triggers.Count; i++)
			{
				if (canActivate)
				{
					Triggers[i].Update(timeElapsed, car, ref soundBuffer, ref loops);
				}
			}

			if (soundBuffer != null)
			{
				if (soundSource != null && soundSource.Buffer == soundBuffer)
				{
					soundSource.Volume = volume;
					soundSource.Pitch = pitch;
				}
				else
				{
					soundSource?.Stop();
					soundSource = (SoundSource)TrainManagerBase.currentHost.PlaySound(soundBuffer, pitch, volume, Vector3.Zero, car, loops);
				}
			}
			else
			{
				if (soundSource != null && soundSource.IsPlaying())
				{
					soundSource.Stop();
				}
			}
		}
	}
}
