using System;
using LibRender2.Trains;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager.Brake;

namespace TrainManager.Car
{
	public class Coupler : AbstractCoupler
	{
		/// <summary>The car sections to display</summary>
		internal CarSection[] CarSections;
		/// <summary>The currently displayed car section</summary>
		internal int CurrentCarSection;
		/// <summary>The base car which the coupling is attached to</summary>
		/// <remarks>This is the FRONT car when travelling in the notional forwards direction</remarks>
		internal CarBase BaseCar;
		/// <summary>The connected car</summary>
		/// <remarks>This is the REAR car when travelling in the notional forwards direction</remarks>
		public CarBase ConnectedCar;
		/// <summary>The sound played when this coupler is uncoupled</summary>
		public CarSound UncoupleSound;
		/// <summary>The sound played when this coupler is uncoupled</summary>
		public CarSound CoupleSound;
		/// <summary>The brake behaviour when this consist is uncoupled</summary>
		public UncouplingBehaviour UncouplingBehaviour;

		private bool canUncouple;

		public Coupler(double minimumDistance, double maximumDistance, CarBase frontCar, CarBase rearCar)
		{
			MinimumDistanceBetweenCars = minimumDistance;
			MaximumDistanceBetweenCars = maximumDistance;
			BaseCar = frontCar;
			ConnectedCar = rearCar ?? frontCar;

			CarSections = new CarSection[] { };
			ChangeSection(-1);
			UncoupleSound = new CarSound();
			CoupleSound = new CarSound();
			canUncouple = true;
			UncouplingBehaviour = UncouplingBehaviour.EmergencyUncoupledConsist;
		}

		public override bool CanUncouple
		{
			get => ConnectedCar != BaseCar && canUncouple;
			set => canUncouple = value;
		}

		public void UpdateObjects(double timeElapsed, bool forceUpdate)
		{
			// calculate positions and directions for section element update
			Vector3 d = new Vector3(BaseCar.RearAxle.Follower.WorldPosition - ConnectedCar.FrontAxle.Follower.WorldPosition);
			Vector3 u, s;
			double t = d.NormSquared();
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				d *= t;
				u = new Vector3((BaseCar.Up + ConnectedCar.Up) * 0.5);
				s.X = d.Z * u.Y - d.Y * u.Z;
				s.Y = d.X * u.Z - d.Z * u.X;
				s.Z = d.Y * u.X - d.X * u.Y;
			}
			else
			{
				u = Vector3.Down;
				s = Vector3.Right;
			}

			Vector3 p = new Vector3(0.5 * (BaseCar.RearAxle.Follower.WorldPosition + ConnectedCar.FrontAxle.Follower.WorldPosition));
			// determine visibility
			Vector3 cd = new Vector3(p - TrainManagerBase.Renderer.Camera.AbsolutePosition);
			double dist = cd.NormSquared();
			double bid = TrainManagerBase.Renderer.Camera.ViewingDistance + BaseCar.Length;
			bool currentlyVisible = dist < bid * bid;
			// Updates the brightness value
			byte dnb = (byte)BaseCar.Brightness.CurrentBrightness(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness, 1.0);
			
			// update current section
			int cs = CurrentCarSection;
			if (cs >= 0 && cs < CarSections.Length)
			{
				if (CarSections[cs].Groups.Length > 0)
				{
					for (int i = 0; i < CarSections[cs].Groups[0].Elements.Length; i++)
					{
						UpdateSectionElement(cs, i, p, d, u, s, currentlyVisible, timeElapsed, forceUpdate);

						// brightness change
						if (CarSections[cs].Groups[0].Elements[i].internalObject != null)
						{
							CarSections[cs].Groups[0].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}
				}
			}
		}

		public void ChangeSection(int sectionIndex)
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

			if (sectionIndex >= 0)
			{
				CarSections[sectionIndex].Initialize(BaseCar.CurrentlyVisible);
				for (int j = 0; j < CarSections[sectionIndex].Groups[0].Elements.Length; j++)
				{
					TrainManagerBase.currentHost.ShowObject(CarSections[sectionIndex].Groups[0].Elements[j].internalObject, ObjectType.Dynamic);
				}
			}

			CurrentCarSection = sectionIndex;
			UpdateObjects(0.0, true);
		}

		private void UpdateSectionElement(int sectionIndex, int elementIndex, Vector3 position, Vector3 direction, Vector3 up, Vector3 side, bool show, double timeElapsed, bool forceUpdate)
		{
			double timeDelta;
			bool updatefunctions;

			if (CarSections[sectionIndex].Groups[0].Elements[elementIndex].RefreshRate != 0.0)
			{
				if (CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate >= CarSections[sectionIndex].Groups[0].Elements[elementIndex].RefreshRate)
				{
					timeDelta =
						CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate;
					CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate =
						timeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = timeElapsed;
					CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate += timeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate;
				CarSections[sectionIndex].Groups[0].Elements[elementIndex].SecondsSinceLastUpdate = timeElapsed;
				updatefunctions = true;
			}

			if (forceUpdate)
			{
				updatefunctions = true;
			}

			CarSections[sectionIndex].Groups[0].Elements[elementIndex].Update(BaseCar.baseTrain, BaseCar.Index, (BaseCar.RearAxle.Follower.TrackPosition + ConnectedCar.FrontAxle.Follower.TrackPosition) * 0.5, position, direction, up, side, updatefunctions, show, timeDelta, true);
		}

		public void LoadCarSections(UnifiedObject currentObject, bool visibleFromInterior)
		{
			int j = CarSections.Length;
			Array.Resize(ref CarSections, j + 1);
			CarSections[j] = new CarSection(TrainManagerBase.currentHost, ObjectType.Dynamic, visibleFromInterior, BaseCar, currentObject);
		}
	}
}
