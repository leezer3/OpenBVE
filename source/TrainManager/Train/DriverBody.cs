using System;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace TrainManager.Trains
{
	/// <summary>Represents the driver's body, as affected by pitch, roll etc.</summary>
	public class DriverBody
	{
		/// <summary>The slow update vector</summary>
		private Vector2 Slow;
		/// <summary>The fast update vector</summary>
		private Vector2 Fast;
		/// <summary>The current roll value</summary>
		public double Roll;
		/// <summary>The damping for the roll</summary>
		private readonly Damping RollDamping;
		/// <summary>The current pitch value</summary>
		public double Pitch;
		/// <summary>The damping for the pitch value</summary>
		private readonly Damping PitchDamping;
		/// <summary>Contains a reference to the base train</summary>
		private readonly TrainBase Train;
		/// <summary>The height of the driver's shoulders from the seat base / floor</summary>
		public readonly double ShoulderHeight;
		/// <summary>The height of the center of the driver's head from the shoulders</summary>
		public readonly double HeadHeight;

		public DriverBody(TrainBase train)
		{
			Train = train;
			PitchDamping = new Damping(6.0, 0.3);
			RollDamping = new Damping(6.0, 0.3);
			ShoulderHeight = 0.6;
			HeadHeight = 0.1;
		}

		public DriverBody(TrainBase train, double shoulderHeight, double headHeight)
		{
			Train = train;
			PitchDamping = new Damping(6.0, 0.3);
			RollDamping = new Damping(6.0, 0.3);
			ShoulderHeight = shoulderHeight;
			HeadHeight = headHeight;
		}

		public void Update(double TimeElapsed)
		{
			if (TrainManagerBase.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
			{
				// pitch
				double targetY = Train.Specs.CurrentAverageAcceleration;
				const double pitchAccelerationSlow = 0.25;
				const double pitchAccelerationFast = 2.0;
				if (Slow.Y < targetY)
				{
					Slow.Y += pitchAccelerationSlow * TimeElapsed;
					if (Slow.Y > targetY)
					{
						Slow.Y = targetY;
					}
				}
				else if (Slow.Y > targetY)
				{
					Slow.Y -= pitchAccelerationSlow * TimeElapsed;
					if (Slow.Y < targetY)
					{
						Slow.Y = targetY;
					}
				}

				if (Fast.Y < targetY)
				{
					Fast.Y += pitchAccelerationFast * TimeElapsed;
					if (Fast.Y > targetY)
					{
						Fast.Y = targetY;
					}
				}
				else if (Fast.Y > targetY)
				{
					Fast.Y -= pitchAccelerationFast * TimeElapsed;
					if (Fast.Y < targetY)
					{
						Fast.Y = targetY;
					}
				}

				double diffY = Fast.Y - Slow.Y;
				diffY = Math.Sign(diffY) * diffY * diffY;
				Pitch = 0.5 * Math.Atan(0.1 * diffY);
				if (Pitch > 0.1)
				{
					Pitch = 0.1;
				}
				PitchDamping.Update(TimeElapsed, ref Pitch, true);

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

				const double rollAccelerationSlow = 1.0;
				const double rollAccelerationFast = 10.0;
				if (Slow.X < targetX)
				{
					Slow.X += rollAccelerationSlow * TimeElapsed;
					if (Slow.X > targetX)
					{
						Slow.X = targetX;
					}
				}
				else if (Slow.X > targetX)
				{
					Slow.X -= rollAccelerationSlow * TimeElapsed;
					if (Slow.X < targetX)
					{
						Slow.X = targetX;
					}
				}

				if (Fast.X < targetX)
				{
					Fast.X += rollAccelerationFast * TimeElapsed;
					if (Fast.X > targetX)
					{
						Fast.X = targetX;
					}
				}
				else if (Fast.X > targetX)
				{
					Fast.X -= rollAccelerationFast * TimeElapsed;
					if (Fast.X < targetX)
					{
						Fast.X = targetX;
					}
				}

				double diffX = Slow.X - Fast.X;
				diffX = Math.Sign(diffX) * diffX * diffX;
				Roll = 0.5 * Math.Atan(0.3 * diffX);
				RollDamping.Update(TimeElapsed, ref Roll, true);
			}
		}
	}
}
