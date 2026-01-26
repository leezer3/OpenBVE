//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
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

		internal readonly double MaximumLifeSpan;

		internal readonly double MaximumSize;

		internal readonly double MaximumGrownSize;

		internal readonly BaseRenderer Renderer;

		/// <summary>The texture used for drawing particles</summary>
		/// <remarks>Must be a 4x4 texture atlas</remarks>
		public Texture ParticleTexture;

		public ParticleType ParticleType;

		internal Vector3 MovementSpeed;

		public Vector3 Offset;

		internal readonly AbstractCar Car;

		internal double MaxSpeed;

		private double previousUpdatePosition;

		private double particleAdditionTimer;

		private double particleSizeTimer;

		public FunctionScript Controller;
		/// <summary>Whether particles are emitted at idle</summary>
		public bool EmitsAtIdle;


		/// <summary>Creates a new particle source</summary>
		/// <param name="renderer">A reference to the base renderer</param>
		/// <param name="car">The containing car</param>
		/// <param name="offset">The offset position relative to the car</param>
		/// <param name="maximumSize">The initial maximum size of a particle</param>
		/// <param name="maximumGrownSize">The maximum size a particle may grow to</param>
		/// <param name="movementSpeed">The initial movement vector for a particle (exhaust direction)</param>
		/// <param name="maximumLifeSpan">The maximum lifespan of a particle</param>
		/// <param name="type">The type of particles</param>
		public ParticleSource(BaseRenderer renderer, AbstractCar car, Vector3 offset, double maximumSize, double maximumGrownSize, Vector3 movementSpeed, double maximumLifeSpan, ParticleType type = ParticleType.Smoke)
		{
			Renderer = renderer;
			Random = new Random();
			MaximumSize = maximumSize;
			MaximumGrownSize = maximumGrownSize;
			Particles = new List<Particle>();
			MovementSpeed = movementSpeed;
			Offset = offset;
			MaximumLifeSpan = maximumLifeSpan;
			Car = car;
			ParticleType = type;
			if (type == ParticleType.Smoke)
			{
				MaxSpeed = 100;
			}
			else
			{
				// steam assumed to flow more than smoke
				MaxSpeed = 25;
			}
			EmitsAtIdle = true;
		}

		public void Update(double timeElapsed, bool currentlyVisible)
		{
			if (!Renderer.AvailableNewRenderer)
			{
				return;
			}

			dynamic dynamicCar = Car;
			Controller.ExecuteScript(dynamicCar.baseTrain, Car.Index, Car.FrontAxle.Follower.WorldPosition, Car.FrontAxle.Follower.TrackPosition, -1, true, timeElapsed, -1);
			double d = dynamicCar.TractionModel.CurrentPower;
			particleAdditionTimer += timeElapsed;
			particleSizeTimer += timeElapsed;
			Transformation directionalTransform = new Transformation(Car.FrontAxle.Follower.WorldDirection, Car.FrontAxle.Follower.WorldUp, Car.FrontAxle.Follower.WorldSide); // to correct for rotation of car
			for (int i = Particles.Count - 1; i >= 0; i--)
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
					Vector3 vehicleMovement = (Math.Abs(Car.CurrentSpeed) / MaxSpeed) * movementDirection * timeElapsed;

					if (Controller.LastResult > 0)
					{
						if (MaximumGrownSize < MaximumSize)
						{
							// particles shrink
							if (Particles[i].Size.X > MaximumGrownSize && Random.NextDouble() < Controller.LastResult * 0.5 && particleSizeTimer > 0.05)
							{
								if (Random.NextDouble() < 0.3)
								{
									Particles[i].Size.X += Random.NextDouble() * timeElapsed;
								}
								else
								{
									Particles[i].Size.X -= Random.NextDouble() * timeElapsed;
								}

								Particles[i].Size.X = Math.Max(0, Math.Max(Particles[i].Size.X, MaximumGrownSize));
								particleSizeTimer = 0;
							}

							if (Particles[i].Size.Y > MaximumGrownSize && Random.NextDouble() < Controller.LastResult * 0.5 && particleSizeTimer > 0.05)
							{
								if (Random.NextDouble() < 0.3)
								{
									Particles[i].Size.Y += Random.NextDouble() * timeElapsed;
								}
								else
								{
									Particles[i].Size.Y -= Random.NextDouble() * timeElapsed;
								}

								Particles[i].Size.Y = Math.Max(0, Math.Max(Particles[i].Size.Y, MaximumGrownSize));
								particleSizeTimer = 0;
							}
						}
						else
						{
							// particles grow (or do nothing)
							if (Particles[i].Size.X < MaximumGrownSize && Random.NextDouble() < Controller.LastResult * 0.5)
							{
								if (Random.NextDouble() < 0.3)
								{
									Particles[i].Size.X -= Random.NextDouble() * timeElapsed;
								}
								else
								{
									Particles[i].Size.X += Random.NextDouble() * timeElapsed;
								}

								Particles[i].Size.X = Math.Max(0, Math.Min(Particles[i].Size.X, MaximumGrownSize));
							}

							if (Particles[i].Size.Y < MaximumGrownSize && Random.NextDouble() < Controller.LastResult * 0.5)
							{
								if (Random.NextDouble() < 0.3)
								{
									Particles[i].Size.Y -= Random.NextDouble() * timeElapsed;
								}
								else
								{
									Particles[i].Size.Y += Random.NextDouble() * timeElapsed;
								}

								Particles[i].Size.Y = Math.Max(0, Math.Min(Particles[i].Size.Y, MaximumGrownSize));
							}
						}
						
					}
					Particles[i].Position += vehicleMovement;
				}
			}

			if (Particles.Count < MaximumParticles)
			{
				if (dynamicCar.TractionModel.IsRunning && timeElapsed > 0)
				{
					if ((EmitsAtIdle || Controller.LastResult > 0) && particleAdditionTimer > 0.05 && Random.NextDouble() >= 0.5)
					{
						Vector3 startingPosition = new Vector3(Offset);
						startingPosition.Rotate(directionalTransform);
						Particles.Add(new Particle(startingPosition, new Vector2(Random.NextDouble() * MaximumSize, Random.NextDouble() * MaximumSize), Random.NextDouble() * MaximumLifeSpan, Random.Next(0, 11)));
						particleAdditionTimer = 0;
					}
				}
			}

			previousUpdatePosition = Car.FrontAxle.Follower.TrackPosition;

			if (!currentlyVisible)
			{
				return;
			}

			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);

			if (ParticleTexture == null)
			{
				string compatibilityFolder = Path.CombineDirectory(Renderer.fileSystem.GetDataFolder(), "Compatibility");
				if (ParticleType == ParticleType.Smoke)
				{
					Renderer.TextureManager.RegisterTexture(Path.CombineFile(compatibilityFolder, "smoke.png"), out ParticleTexture);
				}
				else
				{
					Renderer.TextureManager.RegisterTexture(Path.CombineFile(compatibilityFolder, "steam.png"), out ParticleTexture);
				}
			}
			// EMITTER POSITION for debugging
			// Vector3 emitterPosition = new Vector3(Offset);
			// emitterPosition.Rotate(directionalTransform);
			// Renderer.Cube.DrawRetained(trackFollower.WorldPosition + emitterPosition, trackFollower.WorldDirection, trackFollower.WorldUp, trackFollower.WorldSide, new Vector3(MaximumSize,MaximumSize,MaximumSize), Renderer.Camera.AbsolutePosition, null, 1.0f);

			// set shader properties
			Renderer.DefaultShader.SetCurrentProjectionMatrix(Renderer.CurrentProjectionMatrix);
			Renderer.DefaultShader.SetMaterialAmbient(Color32.White);
			Renderer.DefaultShader.SetMaterialDiffuse(Color32.White);
			Renderer.DefaultShader.SetMaterialSpecular(Color32.White);
			Renderer.DefaultShader.SetBrightness(1.0f);
			Renderer.DefaultShader.SetAlphaTest(true);
			Vector3 carCenter = 0.5 * (Car.FrontAxle.Follower.WorldPosition + Car.RearAxle.Follower.WorldPosition);
			for (int i = 0; i < Particles.Count; i++)
			{
				Renderer.Particle.Draw(Particles[i].Texture, carCenter + Particles[i].Position, Car.FrontAxle.Follower.WorldDirection, Car.FrontAxle.Follower.WorldUp, Car.FrontAxle.Follower.WorldSide, Particles[i].Size, ParticleTexture, (float)(Particles[i].RemainingLifeSpan / Particles[i].LifeSpan));
			}
		}
	}
}
