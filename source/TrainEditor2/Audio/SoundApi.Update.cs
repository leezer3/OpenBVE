using System;
using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenTK.Audio.OpenAL;
using SoundManager;

namespace TrainEditor2.Audio
{
	internal partial class SoundApi
	{
		protected override void UpdateLinearModel(double timeElapsed)
		{
			throw new NotImplementedException();
		}

		/// <summary>Updates the sound component. Should be called every frame.</summary>
		/// <param name="timeElapsed">The time in seconds that elapsed since the last call to this function.</param>
		protected override void UpdateInverseModel(double timeElapsed)
		{
			/*
			 * Set up the listener.
			 * */
			Vector3 listenerPosition = Vector3.Zero;
			Orientation3 listenerOrientation = new Orientation3(Vector3.Right, Vector3.Up, Vector3.Forward);
			Vector3 listenerVelocity = Vector3.Zero;
			AL.Listener(ALListener3f.Position, 0.0f, 0.0f, 0.0f);
			AL.Listener(ALListener3f.Velocity, (float)listenerVelocity.X, (float)listenerVelocity.Y, (float)listenerVelocity.Z);
			float[] Orientation = new float[] { (float)listenerOrientation.Z.X, (float)listenerOrientation.Z.Y, (float)listenerOrientation.Z.Z, -(float)listenerOrientation.Y.X, -(float)listenerOrientation.Y.Y, -(float)listenerOrientation.Y.Z };
			AL.Listener(ALListenerfv.Orientation, ref Orientation);

			/*
			 * Collect all sounds that are to be played
			 * and ensure that all others are stopped.
			 * */
			List<SoundSourceAttenuation> toBePlayed = new List<SoundSourceAttenuation>();
			for (int i = 0; i < SourceCount; i++)
			{
				if (Sources[i].State == SoundSourceState.StopPending)
				{
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
				}
				else if (Sources[i].State == SoundSourceState.Stopped)
				{
					/*
					 * The sound was already stopped. Remove it from
					 * the list of sound sources.
					 * */
					Sources[i] = Sources[SourceCount - 1];
					SourceCount--;
					i--;
				}
				else if (GlobalMute)
				{
					/*
					 * The sound is playing or about to be played, but
					 * the global mute option is enabled. Stop the sound
					 * sound if necessary, then remove it from the list
					 * of sound sources if the sound is not looping.
					 * */
					if (Sources[i].State == SoundSourceState.Playing)
					{
						AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
						Sources[i].State = SoundSourceState.PlayPending;
						Sources[i].OpenAlSourceName = 0;
					}
					if (!Sources[i].Looped)
					{
						Sources[i].State = SoundSourceState.Stopped;
						Sources[i].OpenAlSourceName = 0;
						Sources[i] = Sources[SourceCount - 1];
						SourceCount--;
						i--;
					}
				}
				else
				{
					/*
					 * The sound is to be played or is already playing.
					 * */
					if (Sources[i].State == SoundSourceState.Playing)
					{
						int state;
						AL.GetSource(Sources[i].OpenAlSourceName, ALGetSourcei.SourceState, out state);
						if (state != (int)ALSourceState.Initial & state != (int)ALSourceState.Playing)
						{
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
					Vector3 position = Sources[i].Position;
					Vector3 positionDifference = position - listenerPosition;
					double distance = positionDifference.Norm();
					double radius = Sources[i].Radius;
					double gain;
					if (distance < 2.0 * radius)
					{
						gain = 1.0 - distance * distance * (4.0 * radius - distance) / (16.0 * radius * radius * radius);
					}
					else
					{
						gain = radius / distance;
					}
					gain *= Sources[i].Volume;
					if (gain <= 0.0)
					{
						/*
						 * The gain is too low. Stop the sound if playing,
						 * but keep looping sounds pending.
						 * */
						if (Sources[i].State == SoundSourceState.Playing)
						{
							AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
							Sources[i].State = SoundSourceState.PlayPending;
							Sources[i].OpenAlSourceName = 0;
						}
						if (!Sources[i].Looped)
						{
							Sources[i].State = SoundSourceState.Stopped;
							Sources[i].OpenAlSourceName = 0;
							Sources[i] = Sources[SourceCount - 1];
							SourceCount--;
							i--;
						}
					}
					else
					{
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
			for (int i = 0; i < toBePlayed.Count; i++)
			{
				toBePlayed[i].Gain -= clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
			}
			toBePlayed.Sort();
			for (int i = 0; i < toBePlayed.Count; i++)
			{
				toBePlayed[i].Gain += clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
			}
			double desiredLogClampFactor;
			int index = 16;
			if (toBePlayed.Count <= index)
			{
				desiredLogClampFactor = MinLogClampFactor;
			}
			else
			{
				double cutoffDistance = toBePlayed[index].Distance;
				if (cutoffDistance <= 0.0)
				{
					desiredLogClampFactor = MaxLogClampFactor;
				}
				else
				{
					double cutoffGain = toBePlayed[index].Gain;
					desiredLogClampFactor = Math.Log(cutoffGain / (cutoffDistance * cutoffDistance));
					if (desiredLogClampFactor < MinLogClampFactor)
					{
						desiredLogClampFactor = MinLogClampFactor;
					}
					else if (desiredLogClampFactor > MaxLogClampFactor)
					{
						desiredLogClampFactor = MaxLogClampFactor;
					}
				}
			}
			const double rate = 3.0;
			if (LogClampFactor < desiredLogClampFactor)
			{
				LogClampFactor += timeElapsed * rate;
				if (LogClampFactor > desiredLogClampFactor)
				{
					LogClampFactor = desiredLogClampFactor;
				}
			}
			else if (LogClampFactor > desiredLogClampFactor)
			{
				LogClampFactor -= timeElapsed * rate;
				if (LogClampFactor < desiredLogClampFactor)
				{
					LogClampFactor = desiredLogClampFactor;
				}
			}

			/*
			 * Play the sounds.
			 * */
			clampFactor = Math.Exp(LogClampFactor);
			for (int i = index; i < toBePlayed.Count; i++)
			{
				toBePlayed[i].Gain = 0.0;
			}
			for (int i = 0; i < toBePlayed.Count; i++)
			{
				SoundSource source = toBePlayed[i].Source;
				double gain = toBePlayed[i].Gain - clampFactor * toBePlayed[i].Distance * toBePlayed[i].Distance;
				if (gain <= 0.0)
				{
					/*
					 * Stop the sound.
					 * */
					if (source.State == SoundSourceState.Playing)
					{
						AL.DeleteSources(1, ref source.OpenAlSourceName);
						source.State = SoundSourceState.PlayPending;
						source.OpenAlSourceName = 0;
					}
					if (!source.Looped)
					{
						source.State = SoundSourceState.Stopped;
						source.OpenAlSourceName = 0;
					}
				}
				else
				{
					/*
					 * Ensure the buffer is loaded, then play the sound.
					 * */
					if (source.State != SoundSourceState.Playing)
					{
						LoadBuffer(source.Buffer);
						if (source.Buffer.Loaded)
						{
							AL.GenSources(1, out source.OpenAlSourceName);
							AL.Source(source.OpenAlSourceName, ALSourcei.Buffer, source.Buffer.OpenAlBufferName);
						}
						else
						{
							/*
							 * We cannot play the sound because
							 * the buffer could not be loaded.
							 * */
							source.State = SoundSourceState.Stopped;
							continue;
						}
					}

					Vector3 position = source.Position;
					Vector3 velocity = Vector3.Zero;
					position -= listenerPosition;
					AL.Source(source.OpenAlSourceName, ALSource3f.Position, (float)position.X, (float)position.Y, (float)position.Z);
					AL.Source(source.OpenAlSourceName, ALSource3f.Velocity, (float)velocity.X, (float)velocity.Y, (float)velocity.Z);
					AL.Source(source.OpenAlSourceName, ALSourcef.Pitch, (float)source.Pitch);
					AL.Source(source.OpenAlSourceName, ALSourcef.Gain, (float)gain);
					if (source.State != SoundSourceState.Playing)
					{
						AL.Source(source.OpenAlSourceName, ALSourceb.Looping, source.Looped);
						AL.SourcePlay(source.OpenAlSourceName);
						source.State = SoundSourceState.Playing;
					}
				}
			}
		}
	}
}
