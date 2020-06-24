namespace Bve5RouteParser
{
	internal struct TrackSound
	{
		internal double TrackPosition;
		internal int RunSoundIndex;
		internal int FlangeSoundIndex;

		internal TrackSound(double TrackPosition, int Run, int Flange)
		{
			this.TrackPosition = TrackPosition;
			this.RunSoundIndex = Run;
			this.FlangeSoundIndex = Flange;
		}
	}
}
