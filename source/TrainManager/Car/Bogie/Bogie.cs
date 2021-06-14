using System;
using LibRender2.Trains;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace TrainManager.Car
{
	/// <summary>Represents a bogie fitted to a train car</summary>
	public class Bogie
	{
#pragma warning disable 0649
		/*
		 * Bogies currently share the parent car's physics, so these are not used
		 */
		public double Width;
		public double Height;
		public double Length;
#pragma warning restore 0649
		/// <summary>Front axle about which the bogie pivots</summary>
		public readonly Axle FrontAxle;

		/// <summary>Rear axle about which the bogie pivots</summary>
		public readonly Axle RearAxle;

		internal Vector3 Up;

		/// <summary>The car sections (objects) attached to the bogie</summary>
		public CarSection[] CarSections;

		/// <summary>The index of the current car section</summary>
		internal int CurrentCarSection;

		/// <summary>Whether currently visible from the in-game camera location</summary>
		internal bool CurrentlyVisible;

		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;

		/// <summary>Holds a reference to the base train</summary>
		// We don't want this to be read-only if we ever manage to uncouple cars...
		// ReSharper disable once FieldCanBeMadeReadOnly.Local
		private AbstractTrain baseTrain;

		public Bogie(AbstractTrain train, CarBase car)
		{
			baseTrain = train;
			baseCar = car;
			CarSections = new CarSection[] { };
			FrontAxle = new Axle(TrainManagerBase.currentHost, train, car);
			RearAxle = new Axle(TrainManagerBase.currentHost, train, car);
		}

		public void UpdateObjects(double TimeElapsed, bool ForceUpdate)
		{
			//Same hack: Check if any car sections are defined for the offending bogie
			if (CarSections.Length == 0)
			{
				return;
			}

			// calculate positions and directions for section element update
			Vector3 d = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
			Vector3 s;
			double t = d.NormSquared();
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				d *= t;
				s.X = d.Z * Up.Y - d.Y * Up.Z;
				s.Y = d.X * Up.Z - d.Z * Up.X;
				s.Z = d.Y * Up.X - d.X * Up.Y;
			}
			else
			{
				s = Vector3.Right;
			}

			Vector3 p = new Vector3(0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition));
			p -= d * (0.5 * (FrontAxle.Position + RearAxle.Position));
			// determine visibility
			Vector3 cd = new Vector3(p - TrainManagerBase.Renderer.Camera.AbsolutePosition);
			double dist = cd.NormSquared();
			double bid = TrainManagerBase.Renderer.Camera.ViewingDistance + Length;
			CurrentlyVisible = dist < bid * bid;
			// brightness
			byte dnb = (byte)baseCar.Brightness.CurrentBrightness(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness);
			// update current section
			int cs = CurrentCarSection;
			if (cs >= 0)
			{
				for (int i = 0; i < CarSections[cs].Groups[0].Elements.Length; i++)
				{
					UpdateSectionElement(cs, i, p, d, s, CurrentlyVisible, TimeElapsed, ForceUpdate);

					// brightness change
					if (CarSections[cs].Groups[0].Elements[i].internalObject != null)
					{
						CarSections[cs].Groups[0].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
					}
				}
			}
		}

		public void Reverse()
		{
			// reverse axle positions
			double temp = FrontAxle.Position;
			FrontAxle.Position = -RearAxle.Position;
			RearAxle.Position = -temp;
			if (CarSections.Length == 0 || CarSections == null)
			{
				return;
			}

			const int idxToReverse = 0; //cannot have an interior view

			foreach (AnimatedObject animatedObject in CarSections[idxToReverse].Groups[0].Elements)
			{
				animatedObject.Reverse();
			}
		}

		public void LoadCarSections(UnifiedObject currentObject, bool visibleFromInterior)
		{
			int j = CarSections.Length;
			Array.Resize(ref CarSections, j + 1);
			CarSections[j] = new CarSection(TrainManagerBase.currentHost, ObjectType.Dynamic, visibleFromInterior, currentObject);
		}

		public void ChangeSection(int SectionIndex)
		{
			if (CarSections.Length == 0)
			{
				CurrentCarSection = -1;
				//Hack: If no bogie objects are defined, just return
				return;
			}

			for (int i = 0; i < CarSections.Length; i++)
			{
				for (int j = 0; j < CarSections[i].Groups[0].Elements.Length; j++)
				{
					TrainManagerBase.currentHost.HideObject(CarSections[i].Groups[0].Elements[j].internalObject);
				}
			}

			if (SectionIndex >= 0)
			{
				CarSections[SectionIndex].Initialize(CurrentlyVisible);
				for (int j = 0; j < CarSections[SectionIndex].Groups[0].Elements.Length; j++)
				{
					TrainManagerBase.currentHost.ShowObject(CarSections[SectionIndex].Groups[0].Elements[j].internalObject, ObjectType.Dynamic);
				}
			}

			CurrentCarSection = SectionIndex;
			UpdateObjects(0.0, true);
		}

		private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
		{
			//TODO: Check whether the UP and SIDE vectors should actually be recalculated, as this just uses that of the root car
			{
				Vector3 p = Position;
				double timeDelta;
				bool updatefunctions;

				if (CarSections[SectionIndex].Groups[0].Elements[ElementIndex].RefreshRate != 0.0)
				{
					if (CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSections[SectionIndex].Groups[0].Elements[ElementIndex].RefreshRate)
					{
						timeDelta =
							CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate;
						CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate =
							TimeElapsed;
						updatefunctions = true;
					}
					else
					{
						timeDelta = TimeElapsed;
						CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
						updatefunctions = false;
					}
				}
				else
				{
					timeDelta = CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate;
					CarSections[SectionIndex].Groups[0].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}

				if (ForceUpdate)
				{
					updatefunctions = true;
				}

				CarSections[SectionIndex].Groups[0].Elements[ElementIndex].Update(true, baseTrain, baseCar.Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, updatefunctions, Show, timeDelta, true);
			}
		}

		public void UpdateTopplingCantAndSpring()
		{
			if (CarSections.Length != 0)
			{
				//FRONT BOGIE

				// get direction, up and side vectors
				Vector3 d = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
				Vector3 s;
				{
					double t = 1.0 / d.Norm();
					d *= t;
					t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
					double ex = d.X * t;
					double ez = d.Z * t;
					s = new Vector3(ez, 0.0, -ex);
					Up = Vector3.Cross(d, s);
				}
				// cant and radius

				//TODO: This currently uses the figures from the base car
				// apply position due to cant/toppling
				{
					double a = baseCar.Specs.RollDueToTopplingAngle +
					           baseCar.Specs.RollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * Math.Sin(a));
					Vector3 c = new Vector3(s.X * x + Up.X * y, s.Y * x + Up.Y * y, s.Z * x + Up.Z * y);
					FrontAxle.Follower.WorldPosition += c;
					RearAxle.Follower.WorldPosition += c;
				}
				// apply rolling
				{
					s.Rotate(d, -baseCar.Specs.RollDueToTopplingAngle - baseCar.Specs.RollDueToCantAngle);
					Up.Rotate(d, -baseCar.Specs.RollDueToTopplingAngle - baseCar.Specs.RollDueToCantAngle);
				}
				// apply pitching
				if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Groups[0].Type == ObjectType.Overlay)
				{
					d.Rotate(s, baseCar.Specs.PitchDueToAccelerationAngle);
					Up.Rotate(s, baseCar.Specs.PitchDueToAccelerationAngle);
					Vector3 cc = 0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition);
					FrontAxle.Follower.WorldPosition -= cc;
					RearAxle.Follower.WorldPosition -= cc;
					FrontAxle.Follower.WorldPosition.Rotate(s, baseCar.Specs.PitchDueToAccelerationAngle);
					RearAxle.Follower.WorldPosition.Rotate(s, baseCar.Specs.PitchDueToAccelerationAngle);
					FrontAxle.Follower.WorldPosition += cc;
					RearAxle.Follower.WorldPosition += cc;
				}
			}
		}
	}
}
