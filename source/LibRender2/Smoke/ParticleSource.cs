using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Smoke
{
	/// <summary>A source of particles</summary>
	public class ParticleSource
	{
		internal readonly List<Particle> Particles;

		internal const int MaximumParticles = 200;
		
		internal readonly Random Random;

		internal const double MaximumLifeSpan = 15;
		
		internal readonly double MaximumSize;

		internal readonly BaseRenderer Renderer;

		internal Texture ParticleTexture;

		internal Vector3 MovementSpeed;

		internal readonly Vector3 Offset;

		internal readonly AbstractCar Car;

		internal const double MaxSpeed = 100;

		private double previousUpdatePosition;

		


		public ParticleSource(BaseRenderer renderer, AbstractCar car, Vector3 offset, double maximumSize, Vector3 movementSpeed)
		{
			Renderer = renderer;
			Random = new Random();
			MaximumSize = maximumSize;
			Particles = new List<Particle>();
			MovementSpeed = movementSpeed;
			Offset = offset;
			Car = car;
		}

		public void Update(double timeElapsed)
		{
			Transformation directionalTransform = new Transformation(Car.FrontAxle.Follower.WorldDirection, Car.FrontAxle.Follower.WorldUp, Car.FrontAxle.Follower.WorldSide); // to correct for rotation of car
			for (int i = Particles.Count - 1; i > 0; i--)
			{
				Particles[i].RemainingLifeSpan -= timeElapsed;
				if (Particles[i].RemainingLifeSpan <= 0)
				{
					Particles.RemoveAt(i);
				}
				else
				{
					
					// Apply base motion (stationary)
					Vector3 transformedSpeed = new Vector3(MovementSpeed);
					transformedSpeed.Rotate(directionalTransform);
					Vector3 baseMovement = Random.NextDouble() * transformedSpeed * timeElapsed;
					Particles[i].Position += baseMovement;

					Vector3 movementDirection = new Vector3(0, 0, 1); // moving forwards, so smoke flows backwards
					if (Car.FrontAxle.Follower.TrackPosition == previousUpdatePosition)
					{
						movementDirection = new Vector3(0, 0, 0);
					}
					else if (Car.FrontAxle.Follower.TrackPosition > previousUpdatePosition)
					{
						movementDirection = new Vector3(0, 0, -1);
					}
					movementDirection.Rotate(directionalTransform);
					Vector3 vehicleMovement = (Car.CurrentSpeed / MaxSpeed) * movementDirection * timeElapsed;
					Particles[i].Position += vehicleMovement;
				}


			}

			if (Particles.Count < MaximumParticles)
			{
				if (Random.NextDouble() >= 0.5)
				{
					Vector3 startingPosition = new Vector3(Offset);
					startingPosition.Rotate(directionalTransform);
					Particles.Add(new Particle(startingPosition, new Vector3(Random.NextDouble() * MaximumSize, Random.NextDouble() * MaximumSize, Random.NextDouble() * MaximumSize), Random.NextDouble() * MaximumLifeSpan));
				}
			}

			previousUpdatePosition = Car.FrontAxle.Follower.TrackPosition;


			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);

			if (ParticleTexture == null)
			{
				// TODO: Random smoke texture, choose from 3 or so
				string Folder = Path.CombineDirectory(Renderer.fileSystem.GetDataFolder(), "Compatibility");
				Renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "smoke.png"), out ParticleTexture);
			}
			// EMITTER POSITION for debugging
			// Vector3 emitterPosition = new Vector3(Offset);
			// emitterPosition.Rotate(directionalTransform);
			// Renderer.Cube.DrawRetained(trackFollower.WorldPosition + emitterPosition, trackFollower.WorldDirection, trackFollower.WorldUp, trackFollower.WorldSide, new Vector3(MaximumSize,MaximumSize,MaximumSize), Renderer.Camera.AbsolutePosition, null, 1.0f);

			for (int i = 0; i < Particles.Count; i++)
			{
				// TODO: Add better particle shape shader- cross probably
				Renderer.Cube.DrawRetained(Car.FrontAxle.Follower.WorldPosition + Particles[i].Position, Car.FrontAxle.Follower.WorldDirection, Car.FrontAxle.Follower.WorldUp, Car.FrontAxle.Follower.WorldSide, Particles[i].Size, Renderer.Camera.AbsolutePosition, ParticleTexture, (float)(Particles[i].RemainingLifeSpan / Particles[i].LifeSpan));
			}
		}
	}
}
