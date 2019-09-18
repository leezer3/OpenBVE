using OpenBveApi.Math;
using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents a horn or whistle</summary>
		internal class Horn
		{
			/// <summary>The sound source for this horn</summary>
			internal SoundSource Source;
			/// <summary>The sound buffer to be played once when playback commences</summary>
			internal SoundBuffer StartSound;
			/// <summary>The loop sound</summary>
			internal SoundBuffer LoopSound;
			/// <summary>The sound buffer to be played once when playback ends</summary>
			internal SoundBuffer EndSound;
			/// <summary>The position of the sound within the train car</summary>
			internal Vector3 SoundPosition;
			/// <summary>Whether this horn has start and end sounds, or uses the legacy loop/ stretch method</summary>
			internal bool StartEndSounds;
			/// <summary>Stores whether the sound is looped or stretched when using the legacy method</summary>
			internal bool Loop;
			/// <summary>Stores the loop state</summary>
			private bool LoopStarted;

			/// <summary>The default constructor</summary>
			internal Horn()
			{
				this.StartSound = null;
				this.LoopSound = null;
				this.EndSound = null;
				this.Loop = false;
			}

			internal Horn(SoundBuffer startSound, SoundBuffer loopSound, SoundBuffer endSound, bool loop)
			{
				this.Source = null;
				this.StartSound = startSound;
				this.LoopSound = loopSound;
				this.EndSound = endSound;
				this.Loop = loop;
				this.StartEndSounds = false;
				this.LoopStarted = false;
				SoundPosition = new Vector3();
			}

			/// <summary>Called by the controls loop to start playback of this horn</summary>
			internal void Play()
			{
				if (StartEndSounds)
				{
					//New style three-part sounds
					if (LoopStarted == false)
					{
						if (!Program.Sounds.IsPlaying(Source))
						{
							if (StartSound != null)
							{
								//The start sound is not currently playing, so start it
								Source = Program.Sounds.PlaySound(StartSound, 1.0, 1.0, SoundPosition, PlayerTrain.Cars[PlayerTrain.DriverCar], false);

								//Set the loop control variable to started
								LoopStarted = true;
							}
							else
							{
								Source = Program.Sounds.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, PlayerTrain.Cars[PlayerTrain.DriverCar], true);
							}
						}
					}
					else
					{
						if (!Program.Sounds.IsPlaying(Source))
						{
							//Start our loop sound playing if the start sound is finished
							Source = Program.Sounds.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, PlayerTrain.Cars[PlayerTrain.DriverCar], true);
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
							if (!Program.Sounds.IsPlaying(Source) && !LoopStarted)
							{
								//On the first keydown event, start the sound source playing and trigger the loop control variable
								Source = Program.Sounds.PlaySound(LoopSound, 1.0, 1.0, SoundPosition,
										PlayerTrain.Cars[PlayerTrain.DriverCar], true);
								LoopStarted = true;
							}
							else
							{
								if (!LoopStarted)
								{
									//Our loop control variable is reset by the keyup event so this code will only trigger on the 
									//second keydown meaning our horn toggles
									Program.Sounds.StopSound(Source);
									LoopStarted = true;
								}
							}
						}
						else
						{
							if (!LoopStarted)
							{
								Source = Program.Sounds.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, PlayerTrain.Cars[PlayerTrain.DriverCar], false);
							}
							LoopStarted = true;
						}
					}
				}

			}

			/// <summary>Called by the controls loop to stop playback of this horn</summary>
			internal void Stop()
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
				if (Program.Sounds.IsPlaying(Source))
				{
					//Stop the loop sound playing
					Program.Sounds.StopSound(Source);
				}
				if (StartEndSounds && !Program.Sounds.IsPlaying(Source) && EndSound != null)
				{
					//If our end sound is defined and in use, play once
					Source = Program.Sounds.PlaySound(EndSound, 1.0, 1.0, SoundPosition, PlayerTrain.Cars[PlayerTrain.DriverCar], false);
				}
			}
		}
	}
}
