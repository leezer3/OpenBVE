using OpenBveApi;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Trains
{
	/// <summary>Represents a horn or whistle</summary>
	public class Horn
	{
		/// <summary>The sound source for this horn</summary>
		public SoundSource Source;
		/// <summary>The sound buffer to be played once when playback commences</summary>
		public SoundBuffer StartSound;
		/// <summary>The loop sound</summary>
		public SoundBuffer LoopSound;
		/// <summary>The sound buffer to be played once when playback ends</summary>
		public SoundBuffer EndSound;
		/// <summary>The position of the sound within the train car</summary>
		public Vector3 SoundPosition;
		/// <summary>Whether this horn has start and end sounds, or uses the legacy loop/ stretch method</summary>
		public bool StartEndSounds;
		/// <summary>Stores whether the sound is looped or stretched when using the legacy method</summary>
		public bool Loop;
		/// <summary>Stores the loop state</summary>
		private bool LoopStarted;
		/// <summary>Holds a reference to the base car</summary>
		public readonly CarBase baseCar;

		/// <summary>The default constructor</summary>
		public Horn(CarBase car)
		{
			this.StartSound = null;
			this.LoopSound = null;
			this.EndSound = null;
			this.Loop = false;
			this.baseCar = car;
		}

		public Horn(SoundBuffer startSound, SoundBuffer loopSound, SoundBuffer endSound, bool loop, CarBase car)
		{
			this.Source = null;
			this.StartSound = startSound;
			this.LoopSound = loopSound;
			this.EndSound = endSound;
			this.Loop = loop;
			this.StartEndSounds = false;
			this.LoopStarted = false;
			this.SoundPosition = new Vector3();
			this.baseCar = car;
		}

		/// <summary>Called by the controls loop to start playback of this horn</summary>
		public void Play()
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.MinimalisticSimulation)
			{
				return;
			}

			if (StartEndSounds)
			{
				//New style three-part sounds
				if (LoopStarted == false)
				{
					if (!TrainManagerBase.currentHost.SoundIsPlaying(Source))
					{
						if (StartSound != null)
						{
							//The start sound is not currently playing, so start it
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(StartSound, 1.0, 1.0, SoundPosition, baseCar, false);

							//Set the loop control variable to started
							LoopStarted = true;
						}
						else
						{
							if (LoopSound != null)
							{
								Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, baseCar, true);
							}
						}
					}
				}
				else
				{
					if (!TrainManagerBase.currentHost.SoundIsPlaying(Source) && LoopSound != null)
					{
						//Start our loop sound playing if the start sound is finished
						Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, baseCar, true);
					}
				}
			}
			else
			{
				//Original single part sounds
				if (LoopSound != null)
				{
					//Loop is ONLY true if this is a Music Horn
					if (Loop)
					{
						if (!TrainManagerBase.currentHost.SoundIsPlaying(Source) && !LoopStarted)
						{
							//On the first keydown event, start the sound source playing and trigger the loop control variable
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition,
								baseCar, true);
							LoopStarted = true;
						}
						else
						{
							if (!LoopStarted)
							{
								//Our loop control variable is reset by the keyup event so this code will only trigger on the 
								//second keydown meaning our horn toggles
								TrainManagerBase.currentHost.StopSound(Source);
								LoopStarted = true;
							}
						}
					}
					else
					{
						if (!LoopStarted)
						{
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, baseCar, false);
						}

						LoopStarted = true;
					}
				}
			}

		}

		/// <summary>Called by the controls loop to stop playback of this horn</summary>
		public void Stop()
		{
			//Reset loop control variable
			LoopStarted = false;
			if (!StartEndSounds & !Loop)
			{
				//Don't stop horns which are play-once single part sounds
				return;
			}

			if (!StartEndSounds & Loop)
			{
				//This sound is a toggle music horn sound
				return;
			}

			if (TrainManagerBase.currentHost.SoundIsPlaying(Source))
			{
				//Stop the loop sound playing
				TrainManagerBase.currentHost.StopSound(Source);
			}

			if (StartEndSounds && !TrainManagerBase.currentHost.SoundIsPlaying(Source) && EndSound != null)
			{
				//If our end sound is defined and in use, play once
				Source =(SoundSource)TrainManagerBase.currentHost.PlaySound(EndSound, 1.0, 1.0, SoundPosition, baseCar, false);
			}
		}
	}
}
