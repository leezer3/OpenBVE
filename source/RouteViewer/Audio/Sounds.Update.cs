using System;
using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenTK.Audio.OpenAL;
using SoundManager;
using TrainManager.Car;


namespace OpenBve
{
	internal class Sounds : SoundsBase
	{

		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		protected override void UpdateLinearModel(double timeElapsed)
		{
			/*
			 * Set up the listener
			 * */
			Vector3 listenerPosition = Program.Renderer.Camera.AbsolutePosition;
			Orientation3 listenerOrientation = new Orientation3(Program.Renderer.Camera.AbsoluteSide, Program.Renderer.Camera.AbsoluteUp, Program.Renderer.Camera.AbsoluteDirection);
			Vector3 listenerVelocity;
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead | Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior) {
				CarBase car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
				Vector3 diff = car.FrontAxle.Follower.WorldPosition - car.RearAxle.Follower.WorldPosition;
				listenerVelocity = car.CurrentSpeed * Vector3.Normalize(diff) + Program.Renderer.Camera.AlignmentSpeed.Position;
			} else {
				listenerVelocity = Program.Renderer.Camera.AlignmentSpeed.Position;
			}
            AL.Listener(ALListener3f.Position, 0.0f, 0.0f, 0.0f);
            AL.Listener(ALListener3f.Velocity, (float)listenerVelocity.X, (float)listenerVelocity.Y, (float)listenerVelocity.Z);
		    var Orientation = new[]{(float) listenerOrientation.Z.X, (float) listenerOrientation.Z.Y, (float) listenerOrientation.Z.Z,-(float) listenerOrientation.Y.X, -(float) listenerOrientation.Y.Y, -(float) listenerOrientation.Y.Z};
            AL.Listener(ALListenerfv.Orientation, ref Orientation );
			/*
			 * Set up the atmospheric attributes
			 * */
			double elevation = Program.Renderer.Camera.AbsolutePosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation;
			double airTemperature = Program.CurrentRoute.Atmosphere.GetAirTemperature(elevation);
			double airPressure = Program.CurrentRoute.Atmosphere.GetAirPressure(elevation, airTemperature);
			double speedOfSound = Program.CurrentRoute.Atmosphere.GetSpeedOfSound(airPressure, airTemperature);
			try {
                AL.SpeedOfSound((float)speedOfSound);
			} catch { }
			/*
			 * Update the sound sources
			 * */
			int actuallyPlaying = 0;
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].State == SoundSourceState.StopPending) {
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
				} else if (Sources[i].State == SoundSourceState.Stopped) {
					/*
					 * The sound was already stopped. Remove it from
					 * the list of sound sources.
					 * */
					Sources[i] = Sources[SourceCount - 1];
					SourceCount--;
					i--;
				} else if (GlobalMute) {
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
					 * Calculate the sound gain.
					 * */
					Vector3 direction;
					Vector3 position;
					Vector3 velocity;

					switch (Sources[i].Type)
					{
						case SoundType.TrainCar:
							var Car = (AbstractCar)Sources[i].Parent;
							Car.CreateWorldCoordinates(Sources[i].Position, out position, out direction);
							velocity = Car.CurrentSpeed * direction;
							break;
						default:
							position = Sources[i].Position;
							velocity = Vector3.Zero;
							break;
					}
					Vector3 positionDifference = position - listenerPosition;
					double gain;
					if (GlobalMute) {
						gain = 0.0;
					} else {
						double distance = positionDifference.Norm();
						double innerRadius = Sources[i].Radius;
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) {
							if (Sources[i].Parent != TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]) {
								innerRadius *= 0.5; 
							}
						}
						double outerRadius = OuterRadiusFactor * innerRadius;
						if (distance < outerRadius) {
							if (distance <= innerRadius) {
								gain = Sources[i].Volume;
							} else {
								gain = (distance - outerRadius) / (innerRadius - outerRadius);
								gain *= Sources[i].Volume;
							}
							gain = 3.0 * gain * gain - 2.0 * gain * gain * gain;
						} else {
							gain = 0.0;
						}
					}
					if (gain <= GainThreshold) {
						/*
						 * If the gain is too low to be audible, stop the sound.
						 * If the sound is not looping, stop it if necessary,
						 * then remove it from the list of sound sources.
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
						 * Play the sound and update position, velocity, pitch and gain.
						 * For non-looping sounds, check if the sound is still playing.
						 * */
						gain = (gain - GainThreshold) / (1.0 - GainThreshold);
						if (Sources[i].State != SoundSourceState.Playing) {
							LoadBuffer(Sources[i].Buffer);
							if (Sources[i].Buffer.Loaded) {
								AL.GenSources(1, out Sources[i].OpenAlSourceName);
								AL.Source(Sources[i].OpenAlSourceName, ALSourcei.Buffer, Sources[i].Buffer.OpenAlBufferName);
							} else {
								/*
								 * We cannot play the sound because
								 * the buffer could not be loaded.
								 * */
								Sources[i].State = SoundSourceState.Stopped;
								continue;
							}
						}
						AL.Source(Sources[i].OpenAlSourceName, ALSource3f.Position, (float)positionDifference.X, (float)positionDifference.Y, (float)positionDifference.Z);
						AL.Source(Sources[i].OpenAlSourceName, ALSource3f.Velocity, (float)velocity.X, (float)velocity.Y, (float)velocity.Z);
						AL.Source(Sources[i].OpenAlSourceName, ALSourcef.Pitch, (float)Sources[i].Pitch);
						AL.Source(Sources[i].OpenAlSourceName, ALSourcef.Gain, (float)gain);
						if (Sources[i].State != SoundSourceState.Playing) {
							AL.Source(Sources[i].OpenAlSourceName, ALSourceb.Looping, Sources[i].Looped);
							AL.SourcePlay(Sources[i].OpenAlSourceName);
							Sources[i].State = SoundSourceState.Playing;
						}
						if (!Sources[i].Looped) {
							int state;
							AL.GetSource(Sources[i].OpenAlSourceName, ALGetSourcei.SourceState, out state);
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
							} else {
								actuallyPlaying++;
							}
						} else {
							actuallyPlaying++;
						}
					}
				}
			}
			/*
			 * Adjust the outer radius factor / the clamp factor.
			 * */
			if (actuallyPlaying >= Interface.CurrentOptions.SoundNumber - 2) {
				/*
				 * Too many sounds are playing.
				 * Reduce the outer radius factor.
				 * */
				OuterRadiusFactorSpeed -= timeElapsed;
				if (OuterRadiusFactorSpeed < -OuterRadiusFactorMaximumSpeed) {
					OuterRadiusFactorSpeed = -OuterRadiusFactorMaximumSpeed;
				}
			} else if (actuallyPlaying <= Interface.CurrentOptions.SoundNumber - 6) {
				/*
				 * Only few sounds are playing.
				 * Increase the outer radius factor.
				 * */
				OuterRadiusFactorSpeed += timeElapsed;
				if (OuterRadiusFactorSpeed > OuterRadiusFactorMaximumSpeed) {
					OuterRadiusFactorSpeed = OuterRadiusFactorMaximumSpeed;
				}
			} else {
				/*
				 * Neither too many nor too few sounds are playing.
				 * Stabilize the outer radius factor.
				 * */
				if (OuterRadiusFactorSpeed < 0.0) {
					OuterRadiusFactorSpeed += timeElapsed;
					if (OuterRadiusFactorSpeed > 0.0) {
						OuterRadiusFactorSpeed = 0.0;
					}
				} else {
					OuterRadiusFactorSpeed -= timeElapsed;
					if (OuterRadiusFactorSpeed < 0.0) {
						OuterRadiusFactorSpeed = 0.0;
					}
				}
			}
			OuterRadiusFactor += OuterRadiusFactorSpeed * timeElapsed;
			if (OuterRadiusFactor < OuterRadiusFactorMinimum) {
				OuterRadiusFactor = OuterRadiusFactorMinimum;
				OuterRadiusFactorSpeed = 0.0;
			} else if (OuterRadiusFactor > OuterRadiusFactorMaximum) {
				OuterRadiusFactor = OuterRadiusFactorMaximum;
				OuterRadiusFactorSpeed = 0.0;
			}
		}

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
		    var Orientation = new float[]{(float) listenerOrientation.Z.X, (float) listenerOrientation.Z.Y, (float) listenerOrientation.Z.Z,-(float) listenerOrientation.Y.X, -(float) listenerOrientation.Y.Y, -(float) listenerOrientation.Y.Z};
			AL.Listener(ALListenerfv.Orientation, ref Orientation);
			/*
			 * Set up the atmospheric attributes.
			 * */
			double elevation = Program.Renderer.Camera.AbsolutePosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation;
			double airTemperature = Program.CurrentRoute.Atmosphere.GetAirTemperature(elevation);
			double airPressure = Program.CurrentRoute.Atmosphere.GetAirPressure(elevation, airTemperature);
			double speedOfSound = Program.CurrentRoute.Atmosphere.GetSpeedOfSound(airPressure, airTemperature);
			try {
				AL.SpeedOfSound((float)speedOfSound);
			} catch { }
			/*
			 * Collect all sounds that are to be played
			 * and ensure that all others are stopped.
			 * */
			List<SoundSourceAttenuation> toBePlayed = new List<SoundSourceAttenuation>();
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].State == SoundSourceState.StopPending) {
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
				} else if (Sources[i].State == SoundSourceState.Stopped) {
					/*
					 * The sound was already stopped. Remove it from
					 * the list of sound sources.
					 * */
					Sources[i] = Sources[SourceCount - 1];
					SourceCount--;
					i--;
				} else if (GlobalMute) {
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
						int state;
						AL.GetSource(Sources[i].OpenAlSourceName, ALGetSourcei.SourceState, out state);
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
							Vector3 direction;
							var Car = (AbstractCar)Sources[i].Parent;
							Car.CreateWorldCoordinates(Sources[i].Position, out position, out direction);
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
			int index = Interface.CurrentOptions.SoundNumber;
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
			for (int i = 0; i < toBePlayed.Count; i++) {
				SoundSource source = toBePlayed[i].Source;
				double gain = toBePlayed[i].Gain - clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
				if (gain <= 0.0) {
					/*
					 * Stop the sound.
					 * */
					if (source.State == SoundSourceState.Playing) {
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
					if (source.State != SoundSourceState.Playing) {
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
							var Car = (AbstractCar)Sources[i].Parent;
							Car.CreateWorldCoordinates(source.Position, out position, out direction);
							velocity = Car.CurrentSpeed * direction;
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
					if (source.State != SoundSourceState.Playing) {
						AL.Source(source.OpenAlSourceName, ALSourceb.Looping, source.Looped);
						AL.SourcePlay(source.OpenAlSourceName);
						source.State = SoundSourceState.Playing;
					}
				}
			}
		}
	}
}
