using OpenBveApi.Math;

namespace MechanikRouteParser
{
	internal class SoundEvent
	{
		internal readonly int SoundIndex;
		internal readonly bool Looped;
		internal readonly bool SpeedDependant;
		internal readonly int Volume;
		internal readonly Vector3 Position;

		internal SoundEvent(int soundIndex, Vector3 position, bool looped, bool speedDependant, int volume)
		{
			Position = position;
			SoundIndex = soundIndex;
			Looped = looped;
			SpeedDependant = speedDependant;
			Volume = volume;
		}
	}
}
