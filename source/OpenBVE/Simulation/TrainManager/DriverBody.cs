using System;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal class DriverBody
	{
		internal Vector2 Slow;
		internal Vector2 Fast;
		internal double Roll;
		internal Damping RollDamping;
		internal double Pitch;
		internal Damping PitchDamping;

		private readonly TrainManager.Train Train;

		internal DriverBody(TrainManager.Train train)
		{
			this.Train = train;
		}

		internal void Update(double TimeElapsed)
		{
			if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
			{
				{
					// pitch
					double targetY = Train.Specs.CurrentAverageAcceleration;
					const double accelerationSlow = 0.25;
					const double accelerationFast = 2.0;
					if (Slow.Y < targetY)
					{
						Slow.Y += accelerationSlow * TimeElapsed;
						if (Slow.Y > targetY)
						{
							Slow.Y = targetY;
						}
					}
					else if (Slow.Y > targetY)
					{
						Slow.Y -= accelerationSlow * TimeElapsed;
						if (Slow.Y < targetY)
						{
							Slow.Y = targetY;
						}
					}

					if (Fast.Y < targetY)
					{
						Fast.Y += accelerationFast * TimeElapsed;
						if (Fast.Y > targetY)
						{
							Fast.Y = targetY;
						}
					}
					else if (Fast.Y > targetY)
					{
						Fast.Y -= accelerationFast * TimeElapsed;
						if (Fast.Y < targetY)
						{
							Fast.Y = targetY;
						}
					}

					double diffY = Fast.Y - Slow.Y;
					diffY = (double) Math.Sign(diffY) * diffY * diffY;
					Pitch = 0.5 * Math.Atan(0.1 * diffY);
					if (Pitch > 0.1)
					{
						Pitch = 0.1;
					}

					if (PitchDamping == null)
					{
						PitchDamping = new Damping(6.0, 0.3);
					}

					PitchDamping.Update(TimeElapsed, ref Pitch, true);
				}
				{
					// roll
					int c = Train.DriverCar;
					double frontRadius = Train.Cars[c].FrontAxle.Follower.CurveRadius;
					double rearRadius = Train.Cars[c].RearAxle.Follower.CurveRadius;
					double radius;
					if (frontRadius != 0.0 & rearRadius != 0.0)
					{
						if (frontRadius != -rearRadius)
						{
							radius = 2.0 * frontRadius * rearRadius / (frontRadius + rearRadius);
						}
						else
						{
							radius = 0.0;
						}
					}
					else if (frontRadius != 0.0)
					{
						radius = 2.0 * frontRadius;
					}
					else if (rearRadius != 0.0)
					{
						radius = 2.0 * rearRadius;
					}
					else
					{
						radius = 0.0;
					}

					double targetX;
					if (radius != 0.0)
					{
						double speed = Train.Cars[c].CurrentSpeed;
						targetX = speed * speed / radius;
					}
					else
					{
						targetX = 0.0;
					}

					const double accelerationSlow = 1.0;
					const double accelerationFast = 10.0;
					if (Slow.X < targetX)
					{
						Slow.X += accelerationSlow * TimeElapsed;
						if (Slow.X > targetX)
						{
							Slow.X = targetX;
						}
					}
					else if (Slow.X > targetX)
					{
						Slow.X -= accelerationSlow * TimeElapsed;
						if (Slow.X < targetX)
						{
							Slow.X = targetX;
						}
					}

					if (Fast.X < targetX)
					{
						Fast.X += accelerationFast * TimeElapsed;
						if (Fast.X > targetX)
						{
							Fast.X = targetX;
						}
					}
					else if (Fast.X > targetX)
					{
						Fast.X -= accelerationFast * TimeElapsed;
						if (Fast.X < targetX)
						{
							Fast.X = targetX;
						}
					}

					double diffX = Slow.X - Fast.X;
					diffX = (double) Math.Sign(diffX) * diffX * diffX;
					Roll = 0.5 * Math.Atan(0.3 * diffX);
					if (RollDamping == null)
					{
						RollDamping = new Damping(6.0, 0.3);
					}

					RollDamping.Update(TimeElapsed, ref Roll, true);
				}
			}
		}
	}
}
