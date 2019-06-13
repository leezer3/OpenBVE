using System;
using LibRender;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal class DriverBody
	{
		internal double SlowX;
		internal double FastX;
		internal double Roll;
		internal Damping RollDamping;
		internal double SlowY;
		internal double FastY;
		internal double Pitch;
		internal Damping PitchDamping;

		private readonly TrainManager.Train Train;

		internal DriverBody(TrainManager.Train train)
		{
			this.Train = train;
		}

		internal void Update(double TimeElapsed)
		{
			if (Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
			{
				{
					// pitch
					double targetY = Train.Specs.CurrentAverageAcceleration;
					const double accelerationSlow = 0.25;
					const double accelerationFast = 2.0;
					if (SlowY < targetY)
					{
						SlowY += accelerationSlow * TimeElapsed;
						if (SlowY > targetY)
						{
							SlowY = targetY;
						}
					}
					else if (SlowY > targetY)
					{
						SlowY -= accelerationSlow * TimeElapsed;
						if (SlowY < targetY)
						{
							SlowY = targetY;
						}
					}

					if (FastY < targetY)
					{
						FastY += accelerationFast * TimeElapsed;
						if (FastY > targetY)
						{
							FastY = targetY;
						}
					}
					else if (FastY > targetY)
					{
						FastY -= accelerationFast * TimeElapsed;
						if (FastY < targetY)
						{
							FastY = targetY;
						}
					}

					double diffY = FastY - SlowY;
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
						double speed = Train.Cars[c].Specs.CurrentSpeed;
						targetX = speed * speed / radius;
					}
					else
					{
						targetX = 0.0;
					}

					const double accelerationSlow = 1.0;
					const double accelerationFast = 10.0;
					if (SlowX < targetX)
					{
						SlowX += accelerationSlow * TimeElapsed;
						if (SlowX > targetX)
						{
							SlowX = targetX;
						}
					}
					else if (SlowX > targetX)
					{
						SlowX -= accelerationSlow * TimeElapsed;
						if (SlowX < targetX)
						{
							SlowX = targetX;
						}
					}

					if (FastX < targetX)
					{
						FastX += accelerationFast * TimeElapsed;
						if (FastX > targetX)
						{
							FastX = targetX;
						}
					}
					else if (FastX > targetX)
					{
						FastX -= accelerationFast * TimeElapsed;
						if (FastX < targetX)
						{
							FastX = targetX;
						}
					}

					double diffX = SlowX - FastX;
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
