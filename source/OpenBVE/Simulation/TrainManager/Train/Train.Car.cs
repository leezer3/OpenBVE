using System;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/*
		 * Contains the base definition of a train car
		 */
		/// <summary>The root class for a train car within the simulation</summary>
		internal class Car
		{
			/// <summary>The width of the car in m</summary>
			internal double Width;
			/// <summary>The height of the car in m</summary>
			internal double Height;
			/// <summary>The length of the car in m</summary>
			internal double Length;
			/// <summary>The front axle</summary>
			internal Axle FrontAxle;
			/// <summary>The front bogie</summary>
			internal Bogie FrontBogie;
			/// <summary>The rear axle</summary>
			internal Axle RearAxle;
			/// <summary>The rear bogie</summary>
			internal Bogie RearBogie;

		    internal int Index;

			internal Horn[] Horns;

			internal Vector3 Up;
			/// <summary>The car sections (Objects associated with the car)</summary>
			internal CarSection[] CarSections;
			/// <summary>The index of the current car section</summary>
			internal int CurrentCarSection;
			/// <summary>The position of the driver's eyes within the car</summary>
			internal Vector3 DriverPosition;
			/// <summary>The current camera yaw for the driver's eyes</summary>
			internal double DriverYaw;
			/// <summary>The current camera pitch for the driver's eyes</summary>
			internal double DriverPitch;
			/// <summary>The performance specifications associated with the car</summary>
			internal CarSpecs Specs;
			/// <summary>The car sounds</summary>
			internal CarSounds Sounds;
			/// <summary>Whether the car is currently visible</summary>
			internal bool CurrentlyVisible;
			/// <summary>Whether the car is currently derailed</summary>
			internal bool Derailed;
			/// <summary>Whether the car is currently toppled</summary>
			internal bool Topples;
			/// <summary>The current brightness value for the driver's view</summary>
			internal CarBrightness Brightness;
			/// <summary>The position of the beacon reciever in the car</summary>
			internal double BeaconReceiverPosition;
			/// <summary>The beacon reciever track follower</summary>
			internal TrackManager.TrackFollower BeaconReceiver;
			/// <summary>Whether this car is currently in a station</summary>
			/// TODO: This appears only to be set by the station start/ end events and not checked elsewhere, why???
			internal bool CurrentlyInStation;

		    internal Train Train;

		    internal Car(Train train)
		    {
		        this.Train = train;
		    }

            /// <summary>Gets the current acceleration output for this car</summary>
            /// <param name="CurveIndex">The acceleration curve to use</param>
            /// <param name="Speed">The speed in km/h</param>
            /// <returns>The acceleration output in m/s</returns>
		    internal double GetAccelerationOutput(int CurveIndex, double Speed)
		    {
		        if (CurveIndex < Specs.AccelerationCurves.Length)
		        {
		            double a0 = Specs.AccelerationCurves[CurveIndex].StageZeroAcceleration;
		            double s1 = Specs.AccelerationCurves[CurveIndex].StageOneSpeed;
		            double a1 = Specs.AccelerationCurves[CurveIndex].StageOneAcceleration;
		            double s2 = Specs.AccelerationCurves[CurveIndex].StageTwoSpeed;
		            double e2 = Specs.AccelerationCurves[CurveIndex].StageTwoExponent;
		            double f = Specs.AccelerationCurvesMultiplier;
		            if (Speed <= 0.0)
		            {
		                return f * a0;
		            }
		            if (Speed < s1)
		            {
		                double t = Speed / s1;
		                return f * (a0 * (1.0 - t) + a1 * t);
		            }
		            if (Speed < s2)
		            {
		                return f * s1 * a1 / Speed;
		            }
		            return f * s1 * a1 * Math.Pow(s2, e2 - 1.0) * Math.Pow(Speed, -e2);

		        }
		        return 0.0;
		    }

		    internal void Initialize()
		    {
		        for (int i = 0; i < CarSections.Length; i++)
		        {
		            CarSections[i].Initialize(CurrentlyVisible);
		        }
		        for (int i = 0; i < FrontBogie.CarSections.Length; i++)
		        {
		            FrontBogie.CarSections[i].Initialize(CurrentlyVisible);
		        }
		        for (int i = 0; i < RearBogie.CarSections.Length; i++)
		        {
		            RearBogie.CarSections[i].Initialize(CurrentlyVisible);
		        }
		        Brightness.PreviousBrightness = 1.0f;
		        Brightness.NextBrightness = 1.0f;
		    }

		    // change car section
		    internal void ChangeCarSection(int SectionIndex)
		    {
		        if (CarSections.Length == 0)
		        {
		            SectionIndex = -1;
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
		                if (CarSections[SectionIndex].Overlay)
		                {
		                    Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
		                }
		                else
		                {
		                    Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
		                }
		            }
		        }
		        CurrentCarSection = SectionIndex;
		        //When changing car section, do not apply damping
		        //This stops objects from spinning if the last position before they were hidden is different
		        UpdateObjects(0.0, true, false);
		    }

		    internal void UpdateObjects(double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		    {
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
		        double d = 0.5 * (FrontAxle.Position + RearAxle.Position);
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
		        // Updates the brightness value
		        byte dnb;
		        {
		            float bb = (float)(Brightness.NextTrackPosition - Brightness.PreviousTrackPosition);

		            //1.0f represents a route brightness value of 255
		            //0.0f represents a route brightness value of 0

		            if (bb != 0.0f)
		            {
		                bb = (float)(FrontAxle.Follower.TrackPosition - Brightness.PreviousTrackPosition) / bb;
		                if (bb < 0.0f) bb = 0.0f;
		                if (bb > 1.0f) bb = 1.0f;
		                bb = Brightness.PreviousBrightness * (1.0f - bb) + Brightness.NextBrightness * bb;
		            }
		            else
		            {
		                bb = Brightness.PreviousBrightness;
		            }
		            //Calculate the cab brightness
		            double ccb = Math.Round(255.0 * (double)(1.0 - bb));
		            //DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
		            dnb = (byte)Math.Min(Renderer.DynamicCabBrightness, ccb);
		        }
		        // update current section
		        int s = CurrentCarSection;
		        if (s >= 0)
		        {
		            for (int i = 0; i < CarSections[s].Elements.Length; i++)
		            {
		                UpdateSectionElement(s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

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

		    private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		    {
		        Vector3 p;
		        if (CarSections[SectionIndex].Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
		        {
		            p = new Vector3(DriverPosition.X, DriverPosition.Y, DriverPosition.Z);
		        }
		        else
		        {
		            p = Position;
		        }
		        double timeDelta;
		        bool updatefunctions;
		        if (CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
		        {
		            if (CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
		            {
		                timeDelta = CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		                CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
		                updatefunctions = true;
		            }
		            else
		            {
		                timeDelta = TimeElapsed;
		                CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
		                updatefunctions = false;
		            }
		        }
		        else
		        {
		            timeDelta = CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		            CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
		            updatefunctions = true;
		        }
		        if (ForceUpdate)
		        {
		            updatefunctions = true;
		        }
		        ObjectManager.UpdateAnimatedObject(ref CarSections[SectionIndex].Elements[ElementIndex], true, Train, Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta, EnableDamping);
		    }

		    internal void CreateWorldCoordinates(double CarX, double CarY, double CarZ, out double PositionX, out double PositionY, out double PositionZ, out double DirectionX, out double DirectionY, out double DirectionZ)
		    {
		        DirectionX = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
		        DirectionY = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
		        DirectionZ = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
		        double t = DirectionX * DirectionX + DirectionY * DirectionY + DirectionZ * DirectionZ;
		        if (t != 0.0)
		        {
		            t = 1.0 / Math.Sqrt(t);
		            DirectionX *= t; DirectionY *= t; DirectionZ *= t;
		            double ux = Up.X;
		            double uy = Up.Y;
		            double uz = Up.Z;
		            double sx = DirectionZ * uy - DirectionY * uz;
		            double sy = DirectionX * uz - DirectionZ * ux;
		            double sz = DirectionY * ux - DirectionX * uy;
		            double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
		            double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
		            double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
		            PositionX = rx + sx * CarX + ux * CarY + DirectionX * CarZ;
		            PositionY = ry + sy * CarX + uy * CarY + DirectionY * CarZ;
		            PositionZ = rz + sz * CarX + uz * CarY + DirectionZ * CarZ;
		        }
		        else
		        {
		            PositionX = FrontAxle.Follower.WorldPosition.X;
		            PositionY = FrontAxle.Follower.WorldPosition.Y;
		            PositionZ = FrontAxle.Follower.WorldPosition.Z;
		            DirectionX = 0.0;
		            DirectionY = 1.0;
		            DirectionZ = 0.0;
		        }
		    }
        }
	}
}
