using System.Linq;
using OpenBveApi.Sounds;
using OpenTK.Audio.OpenAL;
using SoundManager;
using TrainManager.Trains;

namespace OpenBve
{
	internal partial class Sounds : SoundsBase
	{
		/// <summary>Stops all sounds that are attached to the specified train.</summary>
		/// <param name="train">The train.</param>
		public override void StopAllSounds(object train)
		{
			if (train is TrainBase)
			{
				var t = (TrainBase) train;
				for (int i = 0; i < Sources.Count; i++)
				{
					if (t.Cars.Contains(Sources[i].Parent) || Sources[i].Parent == train)
					{
						if (Sources[i].State == SoundSourceState.Playing)
						{
							AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
							Sources[i].OpenAlSourceName = 0;
						}
						Sources[i].State = SoundSourceState.Stopped;
					}
				}
			}
		}
	}
}
