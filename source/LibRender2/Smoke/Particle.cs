using OpenBveApi.Math;

namespace LibRender2.Smoke
{
	internal class Particle
	{
		internal Vector3 Position;

		internal Vector2 Size;

		internal double RemainingLifeSpan;

		internal readonly double LifeSpan;

		internal readonly int Texture;

		internal Particle(Vector3 offset, Vector2 size, double life, int texture)
		{
			Position = offset;
			Size = size;
			LifeSpan = life;
			RemainingLifeSpan = life;
			Texture = texture;
		}
	}
}
