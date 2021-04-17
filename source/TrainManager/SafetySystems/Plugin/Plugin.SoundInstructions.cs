namespace TrainManager.SafetySystems
{
	internal class SoundInstructions
	{
		/// <summary>The sound should be stopped</summary>
		internal const int Stop = -10000;
		/// <summary>The sound should be played looping</summary>
		internal const int PlayLooping = 0;
		/// <summary>The sound should be played once</summary>
		internal const int PlayOnce = 1;
		/// <summary>Playback of the looping sound should continue</summary>
		internal const int Continue = 2;
	}
}
