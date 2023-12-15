using System;
using System.Collections.Generic;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenTK.Audio.OpenAL;
using SoundManager;
using TrainManager.Car;

namespace OpenBve
{
	using OpenBveApi.Math;

	internal partial class Sounds
	{
		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		protected override void UpdateInverseModel(double timeElapsed)
		{
			/*
			 * Set up the listener.
			 * */
			Vector3 listenerPosition = Program.Renderer.Camera.AbsolutePosition;
			Orientation3 listenerOrientation = new Orientation3(Program.Renderer.Camera.AbsoluteSide, Program.Renderer.Camera.AbsoluteUp, Program.Renderer.Camera.AbsoluteDirection);
			Vector3 listenerVelocity;
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead | Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior) {
				CarBase car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
				Vector3 diff = car.FrontAxle.Follower.WorldPosition - car.RearAxle.Follower.WorldPosition;
				if (diff.IsNullVector()) {
					listenerVelocity = car.CurrentSpeed * Vector3.Forward;
				} else {
					listenerVelocity = car.CurrentSpeed * Vector3.Normalize(diff);
				}
			} else {
				listenerVelocity = Vector3.Zero;
			}
			AL.Listener(ALListener3f.Position, 0.0f, 0.0f, 0.0f);
			AL.Listener(ALListener3f.Velocity, (float)listenerVelocity.X, (float)listenerVelocity.Y, (float)listenerVelocity.Z);
		    var Orientation = new[]{(float) listenerOrientation.Z.X, (float) listenerOrientation.Z.Y, (float) listenerOrientation.Z.Z,-(float) listenerOrientation.Y.X, -(float) listenerOrientation.Y.Y, -(float) listenerOrientation.Y.Z};
			AL.Listener(ALListenerfv.Orientation, ref Orientation);
			/*
			 * Set up the atmospheric attributes.
			 * */
			double elevation = Program.Renderer.Camera.AbsolutePosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation;
			double airTemperature = Program.CurrentRoute.Atmosphere.GetAirTemperature(elevation);
			double airPressure = Program.CurrentRoute.Atmosphere.GetAirPressure(elevation, airTemperature);
			double speedOfSound = Program.CurrentRoute.Atmosphere.GetSpeedOfSound(airPressure, airTemperature);
			try
			{
				AL.SpeedOfSound((float)speedOfSound);
			}
			catch
			{
				// negative or zero value will throw, but be ignored by AL
			}
			/*
			 * Collect all sounds that are to be played
			 * and ensure that all others are stopped.
			 * */
			List<SoundSourceAttenuation> toBePlayed = new List<SoundSourceAttenuation>();
			for (int i = 0; i < SourceCount; i++)
			{
				switch (Sources[i].State)
				{
					case SoundSourceState.StopPending:
						/*
						 * The sound is still playing but is to be stopped.
						 * Stop the sound, then remove it from the list of
						 * sound sources.
						 * */
						AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
						Sources[i].State = SoundSourceState.Stopped;
						Sources[i].OpenAlSourceName = 0;
						Sources[i] = Sources[SourceCount - 1];
						SourceCount--;
						i--;
						break;
					case SoundSourceState.PausePending:
						AL.SourcePause(Sources[i].OpenAlSourceName);
						Sources[i].State = SoundSourceState.Paused;
						break;
					case SoundSourceState.ResumePending:
						AL.SourcePlay(Sources[i].OpenAlSourceName);
						Sources[i].State = SoundSourceState.Playing;
						break;
					case SoundSourceState.Stopped:
						/*
						 * The sound was already stopped. Remove it from
						 * the list of sound sources.
						 * */
						Sources[i] = Sources[SourceCount - 1];
						SourceCount--;
						i--;
						break;
					default:
					{
						if (GlobalMute) {
							/*
							 * The sound is playing or about to be played, but
							 * the global mute option is enabled. Stop the sound
							 * sound if necessary, then remove it from the list
							 * of sound sources if the sound is not looping.
							 * */
							if (Sources[i].State == SoundSourceState.Playing) {
								AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
								Sources[i].State = SoundSourceState.PlayPending;
								Sources[i].OpenAlSourceName = 0;
							}
							if (!Sources[i].Looped) {
								Sources[i].State = SoundSourceState.Stopped;
								Sources[i].OpenAlSourceName = 0;
								Sources[i] = Sources[SourceCount - 1];
								SourceCount--;
								i--;
							}
						} else {
							/*
							 * The sound is to be played or is already playing.
							 * */
							if (Sources[i].State == SoundSourceState.Playing) {
								AL.GetSource(Sources[i].OpenAlSourceName, ALGetSourcei.SourceState, out int state);
								if (state != (int)ALSourceState.Initial & state != (int)ALSourceState.Playing) {
									/*
									 * The sound is not playing any longer.
									 * Remove it from the list of sound sources.
									 * */
									AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
									Sources[i].State = SoundSourceState.Stopped;
									Sources[i].OpenAlSourceName = 0;
									Sources[i] = Sources[SourceCount - 1];
									SourceCount--;
									i--;
									continue;
								}
							}
							/*
							 * Calculate the gain, then add the sound
							 * to the list of sounds to be played.
							 * */
							Vector3 position;
							switch (Sources[i].Type)
							{
								case SoundType.TrainCar:
									var Car = (AbstractCar)Sources[i].Parent;
									Car.CreateWorldCoordinates(Sources[i].Position, out position, out _);
									break;
								case SoundType.AnimatedObject:
									var WorldSound = (WorldSound)Sources[i].Parent;
									position = WorldSound.Follower.WorldPosition + WorldSound.Position;
									break;
								default:
									position = Sources[i].Position;
									break;
							}
							Vector3 positionDifference = position - listenerPosition;
							double distance = positionDifference.Norm();
							double radius = Sources[i].Radius;
							if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) {
								if (Sources[i].Parent != TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]) {
									radius *= 0.5;
								}
							}
							double gain;
							if (distance < 2.0 * radius) {
								gain = 1.0 - distance * distance * (4.0 * radius - distance) / (16.0 * radius * radius * radius);
							} else {
								gain = radius / distance;
							}
							gain *= Sources[i].Volume;
							if (gain <= 0.0) {
								/*
								 * The gain is too low. Stop the sound if playing,
								 * but keep looping sounds pending.
								 * */
								if (Sources[i].State == SoundSourceState.Playing) {
									AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
									Sources[i].State = SoundSourceState.PlayPending;
									Sources[i].OpenAlSourceName = 0;
								}
								if (!Sources[i].Looped) {
									Sources[i].State = SoundSourceState.Stopped;
									Sources[i].OpenAlSourceName = 0;
									Sources[i] = Sources[SourceCount - 1];
									SourceCount--;
									i--;
								}
							} else {
								/*
								 * Add the source.
								 * */
								toBePlayed.Add(new SoundSourceAttenuation(Sources[i], gain, distance));
							}
						}

						break;
					}
				}
			}
			/*
			 * Now that we have the list of sounds that are to be played,
			 * sort them by their gain so that we can determine and
			 * adjust the clamp factor.
			 * */
			double clampFactor = Math.Exp(LogClampFactor);
			for (int i = 0; i < toBePlayed.Count; i++) {
				toBePlayed[i].Gain -= clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
			}
			toBePlayed.Sort();
			for (int i = 0; i < toBePlayed.Count; i++) {
				toBePlayed[i].Gain += clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
			}
			double desiredLogClampFactor;
			int index = Math.Min(systemMaxSounds, Interface.CurrentOptions.SoundNumber);
			if (toBePlayed.Count <= index) {
				desiredLogClampFactor = MinLogClampFactor;
			} else {
				double cutoffDistance = toBePlayed[index].Distance;
				if (cutoffDistance <= 0.0) {
					desiredLogClampFactor = MaxLogClampFactor;
				} else {
					double cutoffGain = toBePlayed[index].Gain;
					desiredLogClampFactor = Math.Log(cutoffGain / (cutoffDistance * cutoffDistance));
					if (desiredLogClampFactor < MinLogClampFactor) {
						desiredLogClampFactor = MinLogClampFactor;
					} else if (desiredLogClampFactor > MaxLogClampFactor) {
						desiredLogClampFactor = MaxLogClampFactor;
					}
				}
			}
			const double rate = 3.0;
			if (LogClampFactor < desiredLogClampFactor) {
				LogClampFactor += timeElapsed * rate;
				if (LogClampFactor > desiredLogClampFactor) {
					LogClampFactor = desiredLogClampFactor;
				}
			} else if (LogClampFactor > desiredLogClampFactor) {
				LogClampFactor -= timeElapsed * rate;
				if (LogClampFactor < desiredLogClampFactor) {
					LogClampFactor = desiredLogClampFactor;
				}
			}
			/*
			 * Play the sounds.
			 * */
			clampFactor = Math.Exp(LogClampFactor);
			for (int i = index; i < toBePlayed.Count; i++) {
				toBePlayed[i].Gain = 0.0;
			}

			int idx2 = Math.Min(index, toBePlayed.Count);
			for (int i = 0; i < toBePlayed.Count; i++) {
				SoundSource source = toBePlayed[i].Source;
				double gain = toBePlayed[i].Gain - clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
				if (gain <= 0.0 || i > idx2) {
					/*
					 * Stop the sound.
					 * */
					if (source.State == SoundSourceState.Playing && source.State != SoundSourceState.Paused && source.State != SoundSourceState.PausePending) {
						AL.DeleteSources(1, ref source.OpenAlSourceName);
						source.State = SoundSourceState.PlayPending;
						source.OpenAlSourceName = 0;
					}
					if (!source.Looped) {
						source.State = SoundSourceState.Stopped;
						source.OpenAlSourceName = 0;
					}
				} else {
					/*
					 * Ensure the buffer is loaded, then play the sound.
					 * */
					if (source.State != SoundSourceState.Playing && source.State != SoundSourceState.Paused && source.State != SoundSourceState.PausePending) {
						LoadBuffer(source.Buffer);
						if (source.Buffer.Loaded) {
							AL.GenSources(1, out source.OpenAlSourceName);
							AL.Source(source.OpenAlSourceName, ALSourcei.Buffer, source.Buffer.OpenAlBufferName);
						} else {
							/*
							 * We cannot play the sound because
							 * the buffer could not be loaded.
							 * */
							source.State = SoundSourceState.Stopped;
							continue;
						}
					}
					Vector3 position;
					Vector3 velocity;
					switch (source.Type)
					{
						case SoundType.TrainCar:
							Vector3 direction;
							var Car = (AbstractCar)source.Parent;
							Car.CreateWorldCoordinates(source.Position, out position, out direction);
							velocity = Car.CurrentSpeed * direction;
							break;
						case SoundType.AnimatedObject:
							var WorldSound = (WorldSound)source.Parent;
							position = WorldSound.Follower.WorldPosition + WorldSound.Position;
							velocity = Vector3.Zero;
							break;
						default:
							position = source.Position;
							velocity = Vector3.Zero;
							break;
					}
					position -= listenerPosition;
					AL.Source(source.OpenAlSourceName, ALSource3f.Position, (float)position.X, (float)position.Y, (float)position.Z);
					AL.Source(source.OpenAlSourceName, ALSource3f.Velocity, (float)velocity.X, (float)velocity.Y, (float)velocity.Z);
					AL.Source(source.OpenAlSourceName, ALSourcef.Pitch, (float)source.Pitch);
					AL.Source(source.OpenAlSourceName, ALSourcef.Gain, (float)gain);
					if (source.State != SoundSourceState.Playing && source.State != SoundSourceState.Paused && source.State != SoundSourceState.PausePending) {
						AL.Source(source.OpenAlSourceName, ALSourceb.Looping, source.Looped);
						AL.SourcePlay(source.OpenAlSourceName);
						source.State = SoundSourceState.Playing;
					}
				}
			}

			RecAndPlay(listenerPosition, false, clampFactor);
		}

		private void RecAndPlay(Vector3 listenerPosition, bool IsLinear, double clampFactor) {
			if (OpenAlMic == null) {
				return;
			}

			// If the microphone sound playback is invalid, stop recording.
			if (!IsPlayingMicSounds) {
				if (OpenAlMic.IsRunning) {
					OpenAlMic.Stop();
				}
				return;
			}

			// Start recording.
			if (!OpenAlMic.IsRunning) {
				OpenAlMic.Start();
			}

			// Make sure that the source is playing.
			int[] states = new int[MicSources.Count];

			for (int i = 0; i < MicSources.Count; i++) {
				AL.GetSource(MicSources[i].OpenAlSourceName, ALGetSourcei.BuffersProcessed, out states[i]);
			}

			// Get the number of buffers that can be recorded.
			int sample = OpenAlMic.AvailableSamples;

			for (int i = 0; i < MicSources.Count; i++) {
				if (listenerPosition.Z < MicSources[i].Position.Z - MicSources[i].BackwardTolerance || listenerPosition.Z > MicSources[i].Position.Z + MicSources[i].ForwardTolerance) {
					continue;
				}

				// When playback is completed and recording is possible.
				if (sample > 0 && states[i] == 1) {
					// Store the recorded data in the buffer.
					OpenAlMic.ReadSamples(MicStore, sample);

					// Apply the recording data to the buffer.
					int buffer = AL.GenBuffer();
					AL.BufferData(buffer, OpenAlMic.SampleFormat, MicStore, MicStore.Length, OpenAlMic.SampleFrequency);

					// Apply buffer to source.
					AL.SourceQueueBuffer(MicSources[i].OpenAlSourceName, buffer);

					// Delete the buffer where playback has ended.
					UnloadMicBuffers(MicSources[i].OpenAlSourceName, states[i]);

					Vector3 positionDifference = MicSources[i].Position - listenerPosition;
					double distance = positionDifference.Norm();
					double innerRadius = 15.0;
					double gain = 1.0;

					if (GlobalMute) {
						gain = 0.0;
					} else {
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) {
							innerRadius *= 0.5;
						}
						if (IsLinear) {
							double outerRadius = OuterRadiusFactor * innerRadius;
							if (distance < outerRadius) {
								if (distance > innerRadius) {
									gain = (distance - outerRadius) / (innerRadius - outerRadius);
								}
								gain = 3.0 * gain * gain - 2.0 * gain * gain * gain;
							} else {
								gain = 0.0;
							}
							if (gain <= GainThreshold) {
								gain = 0.0;
							} else {
								gain = (gain - GainThreshold) / (1.0 - GainThreshold);
							}
						} else {
							if (distance < 2.0 * innerRadius) {
								gain = 1.0 - distance * distance * (4.0 * innerRadius - distance) / (16.0 * innerRadius * innerRadius * innerRadius);
							} else {
								gain = innerRadius / distance;
							}
							if (gain <= 0.0) {
								gain = 0.0;
							}
							gain -= clampFactor * distance * distance;
							if (gain <= 0.0) {
								gain = 0.0;
							}
						}
					}

					AL.Source(MicSources[i].OpenAlSourceName, ALSource3f.Position, (float)positionDifference.X, (float)positionDifference.Y, (float)positionDifference.Z);
					AL.Source(MicSources[i].OpenAlSourceName, ALSourcef.Gain, (float)gain);

					if (AL.GetSourceState(MicSources[i].OpenAlSourceName) != ALSourceState.Playing) {
						AL.SourcePlay(MicSources[i].OpenAlSourceName);
					}
				}
			}
		}
		
	}
}
