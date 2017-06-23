using System;
using OpenBveApi.Math;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The root class for a bogie attached to a car</summary>
		internal class Bogie
		{
#pragma warning disable 0649
			//These are currently deliberately unused
			//TODO: Allow separate physics implementation for bogies, rather than simply glued to the track
			internal double Width;
			internal double Height;
			internal double Length;
#pragma warning restore 0649
			internal Axle FrontAxle;
			internal Axle RearAxle;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			internal Vector3 Up;
			internal CarSection[] CarSections;
			internal int CurrentCarSection;
			internal bool CurrentlyVisible;

		    internal Car Car;

		    internal Train Train;

		    public Bogie(Car car, Train train)
		    {
		        this.Car = car;
		        this.Train = train;
		    }

		    internal void UpdateTopplingCantAndSpring(double TimeElapsed)
		    {
		        if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
		        {
		            return;
		        }
		        if (CarSections.Length != 0)
		        {
		            // get direction, up and side vectors
		            double dx, dy, dz;
		            double ux, uy, uz;
		            double sx, sy, sz;
		            {
		                dx = FrontAxle.Follower.WorldPosition.X -
		                     RearAxle.Follower.WorldPosition.X;
		                dy = FrontAxle.Follower.WorldPosition.Y -
		                     RearAxle.Follower.WorldPosition.Y;
		                dz = FrontAxle.Follower.WorldPosition.Z -
		                     RearAxle.Follower.WorldPosition.Z;
		                double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
		                dx *= t;
		                dy *= t;
		                dz *= t;
		                t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
		                double ex = dx * t;
		                double ez = dz * t;
		                sx = ez;
		                sy = 0.0;
		                sz = -ex;
		                World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
		            }
		            // cant and radius



		            //TODO: Hopefully we can apply the base toppling and roll figures from the car itself
		            // apply position due to cant/toppling
		            {
		                double a = Car.Specs.CurrentRollDueToTopplingAngle +
		                           Car.Specs.CurrentRollDueToCantAngle;
		                double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
		                double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
		                double cx = sx * x + ux * y;
		                double cy = sy * x + uy * y;
		                double cz = sz * x + uz * y;
		                FrontAxle.Follower.WorldPosition.X += cx;
		                FrontAxle.Follower.WorldPosition.Y += cy;
		                FrontAxle.Follower.WorldPosition.Z += cz;
		                RearAxle.Follower.WorldPosition.X += cx;
		                RearAxle.Follower.WorldPosition.Y += cy;
		                RearAxle.Follower.WorldPosition.Z += cz;
		            }
		            // apply rolling
		            {
		                double a = -Car.Specs.CurrentRollDueToTopplingAngle -
		                           Car.Specs.CurrentRollDueToCantAngle;
		                double cosa = Math.Cos(a);
		                double sina = Math.Sin(a);
		                World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
		                World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
		                Up.X = ux;
		                Up.Y = uy;
		                Up.Z = uz;
		            }
		            // apply pitching
		            if (CurrentCarSection >= 0 &&
		                CarSections[CurrentCarSection].Overlay)
		            {
		                double a = Car.Specs.CurrentPitchDueToAccelerationAngle;
		                double cosa = Math.Cos(a);
		                double sina = Math.Sin(a);
		                World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
		                World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
		                double cx = 0.5 *
		                            (FrontAxle.Follower.WorldPosition.X +
		                             RearAxle.Follower.WorldPosition.X);
		                double cy = 0.5 *
		                            (FrontAxle.Follower.WorldPosition.Y +
		                             RearAxle.Follower.WorldPosition.Y);
		                double cz = 0.5 *
		                            (FrontAxle.Follower.WorldPosition.Z +
		                             RearAxle.Follower.WorldPosition.Z);
		                FrontAxle.Follower.WorldPosition.X -= cx;
		                FrontAxle.Follower.WorldPosition.Y -= cy;
		                FrontAxle.Follower.WorldPosition.Z -= cz;
		                RearAxle.Follower.WorldPosition.X -= cx;
		                RearAxle.Follower.WorldPosition.Y -= cy;
		                RearAxle.Follower.WorldPosition.Z -= cz;
		                World.Rotate(ref FrontAxle.Follower.WorldPosition, sx, sy, sz, cosa, sina);
		                World.Rotate(ref RearAxle.Follower.WorldPosition, sx, sy, sz, cosa, sina);
		                FrontAxle.Follower.WorldPosition.X += cx;
		                FrontAxle.Follower.WorldPosition.Y += cy;
		                FrontAxle.Follower.WorldPosition.Z += cz;
		                RearAxle.Follower.WorldPosition.X += cx;
		                RearAxle.Follower.WorldPosition.Y += cy;
		                RearAxle.Follower.WorldPosition.Z += cz;
		                Up.X = ux;
		                Up.Y = uy;
		                Up.Z = uz;
		            }
		        }
            }

		    internal void ChangeCarSection(int SectionIndex)
		    {
		        if (CarSections.Length == 0)
		        {
		            CurrentCarSection = -1;
		            //Hack: If no bogie objects are defined, just return
		            return;
		        }
		        for (int i = 0; i < CarSections.Length; i++)
		        {
		            for (int j = 0; j < CarSections[i].Elements.Length; j++)
		            {
		                int o = CarSections[i].Elements[j].ObjectIndex;
		                Renderer.HideObject(o);
		            }
		        }
		        if (SectionIndex >= 0)
		        {
		            CarSections[SectionIndex].Initialize(CurrentlyVisible);
		            for (int j = 0; j < CarSections[SectionIndex].Elements.Length; j++)
		            {
		                int o = CarSections[SectionIndex].Elements[j].ObjectIndex;
		                Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
		            }
		        }
		        CurrentCarSection = SectionIndex;
		        UpdateObjects(0.0, true);
		    }

		    internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
		    {
		        if (CarSections.Length == 0)
		        {
		            return;
		        }
		        // calculate positions and directions for section element update
		        double dx = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
		        double dy = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
		        double dz = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
		        double t = dx * dx + dy * dy + dz * dz;
		        double ux, uy, uz, sx, sy, sz;
		        if (t != 0.0)
		        {
		            t = 1.0 / Math.Sqrt(t);
		            dx *= t; dy *= t; dz *= t;
		            ux = Up.X;
		            uy = Up.Y;
		            uz = Up.Z;
		            sx = dz * uy - dy * uz;
		            sy = dx * uz - dz * ux;
		            sz = dy * ux - dx * uy;
		        }
		        else
		        {
		            ux = 0.0; uy = 1.0; uz = 0.0;
		            sx = 1.0; sy = 0.0; sz = 0.0;
		        }
		        double px = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
		        double py = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
		        double pz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
		        double d = 0.5 * (FrontAxle.Position + RearAxlePosition);
		        px -= dx * d;
		        py -= dy * d;
		        pz -= dz * d;
		        // determine visibility
		        double cdx = px - World.AbsoluteCameraPosition.X;
		        double cdy = py - World.AbsoluteCameraPosition.Y;
		        double cdz = pz - World.AbsoluteCameraPosition.Z;
		        double dist = cdx * cdx + cdy * cdy + cdz * cdz;
		        double bid = Interface.CurrentOptions.ViewingDistance + Length;
		        CurrentlyVisible = dist < bid * bid;
		        // brightness
		        byte dnb;
		        {
		            float Brightness = (float)(Car.Brightness.NextTrackPosition - Car.Brightness.PreviousTrackPosition);
		            if (Brightness != 0.0f)
		            {
		                Brightness = (float)(FrontAxle.Follower.TrackPosition - Car.Brightness.PreviousTrackPosition) / Brightness;
		                if (Brightness < 0.0f) Brightness = 0.0f;
		                if (Brightness > 1.0f) Brightness = 1.0f;
		                Brightness = Car.Brightness.PreviousBrightness * (1.0f - Brightness) + Car.Brightness.NextBrightness * Brightness;
		            }
		            else
		            {
		                Brightness = Car.Brightness.PreviousBrightness;
		            }
		            dnb = (byte)Math.Round(255.0 * (double)(1.0 - Brightness));
		        }
		        // update current section
		        int s = CurrentCarSection;
		        if (s >= 0)
		        {
		            for (int i = 0; i < CarSections[s].Elements.Length; i++)
		            {
                        UpdateSectionElement(s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), CurrentlyVisible, TimeElapsed, ForceUpdate);

		                // brightness change
		                int o = CarSections[s].Elements[i].ObjectIndex;
		                if (ObjectManager.Objects[o] != null)
		                {
		                    for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++)
		                    {
		                        ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
		                    }
		                }
		            }
		        }
		    }

            private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
		    {
		        {
		            Vector3 p = Position;
		            double timeDelta;
		            bool updatefunctions;
		            if (CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
		            {
		                if (CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >=
		                    CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
		                {
		                    timeDelta =
		                        CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		                    CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate =
		                        TimeElapsed;
		                    updatefunctions = true;
		                }
		                else
		                {
		                    timeDelta = TimeElapsed;
		                    CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate +=
		                        TimeElapsed;
		                    updatefunctions = false;
		                }
		            }
		            else
		            {
		                timeDelta = CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		                CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate =
		                    TimeElapsed;
		                updatefunctions = true;
		            }
		            if (ForceUpdate)
		            {
		                updatefunctions = true;
		            }
		            ObjectManager.UpdateAnimatedObject(ref CarSections[SectionIndex].Elements[ElementIndex], true, Train, Car.Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up,
		                Side, CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta, true);
		        }
		    }
        }
	}
}
