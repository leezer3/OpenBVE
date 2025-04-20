using OpenBveApi.Math;

namespace LibRender2.Smoke
{
	internal class Particle
	{
		internal Vector3 Position;

		internal Vector3 Size;

		internal double RemainingLifeSpan;

		internal readonly double LifeSpan;
		
		internal Particle(Vector3 offset, Vector3 size, double life)
		{
			Position = offset;
			Size = size;
			LifeSpan = life;
			RemainingLifeSpan = life;
		}
	}
}
