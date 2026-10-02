using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Camera;
using LibRender2.Cameras;
using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.World;
using TrainManager.Brake;
using TrainManager.BrakeSystems;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace TrainManager.Car
{
	public partial class CarBase : AbstractCar
	{
		/// <summary>Changes the currently visible car section</summary>
		/// <param name="newCarSection">The type of new car section to display</param>
		/// <param name="trainVisible">Whether the train is visible</param>
		/// <param name="forceChange">Whether to force a show / rehide</param>
		public void ChangeCarSection(CarSectionType newCarSection, bool trainVisible = false, bool forceChange = false)
		{
			if(CurrentCarSection == newCarSection && !forceChange)
			{
				return;
			}
			if (trainVisible)
			{
				if (CurrentCarSection != CarSectionType.NotVisible && CarSections[CurrentCarSection].VisibleFromInterior)
				{
					return;
				}
			}

			if (CarSections.TryGetValue(CurrentCarSection, out CarSection currentCarSection))
			{
				for (int j = 0; j < currentCarSection.Groups.Length; j++)
				{
					for (int k = 0; k < currentCarSection.Groups[j].Elements.Length; k++)
					{
						TrainManagerBase.currentHost.HideObject(currentCarSection.Groups[j].Elements[k].internalObject);
					}

					if (currentCarSection.Groups[j].Keyframes != null)
					{
						for (int k = 0; k < currentCarSection.Groups[j].Keyframes.Objects.Length; k++)
						{
							TrainManagerBase.currentHost.HideObject(currentCarSection.Groups[j].Keyframes.Objects[k]);
						}
					}
				}
			}

			// HACK: Bogies are only visible in exterior views, hidden in all others, so do it once here to avoid duplication
			FrontBogie.ChangeSection(-1);
			RearBogie.ChangeSection(-1);
			Coupler?.ChangeSection(-1);
			switch (newCarSection)
			{
				case CarSectionType.NotVisible:
					this.CurrentCarSection = CarSectionType.NotVisible;
					break;
				case CarSectionType.Interior:
					if (CarSections.TryGetValue(CarSectionType.Interior, out CarSection interiorCarSection))
					{
						CurrentCarSection = CarSectionType.Interior;
						interiorCarSection.Initialize(false);
						interiorCarSection.Show();
						TrainManagerBase.Renderer.Camera.CurrentRestriction = CameraRestrictionMode;
					}
					break;
				case CarSectionType.Exterior:
					if (CarSections.TryGetValue(CarSectionType.Exterior, out CarSection exteriorCarSection))
					{
						CurrentCarSection = CarSectionType.Exterior;
						exteriorCarSection.Initialize(false);
						exteriorCarSection.Show();
					}
					else
					{
						CurrentCarSection = CarSectionType.NotVisible;
					}
					FrontBogie.ChangeSection(0);
					RearBogie.ChangeSection(0);
					Coupler?.ChangeSection(0);
					break;
				case CarSectionType.HeadOutLeft:
					if (CarSections.TryGetValue(CarSectionType.HeadOutLeft, out CarSection headOutLeftCarSection))
					{
						CurrentCarSection = CarSectionType.HeadOutLeft;
						headOutLeftCarSection.Initialize(false);
						headOutLeftCarSection.Show();
					}
					break;
				case CarSectionType.HeadOutRight:
					if (CarSections.TryGetValue(CarSectionType.HeadOutRight, out CarSection headOutRightCarSection))
					{
						CurrentCarSection = CarSectionType.HeadOutRight;
						headOutRightCarSection.Initialize(false);
						headOutRightCarSection.Show();
					}
					break;
			}

			//When changing car section, do not apply damping
			//This stops objects from spinning if the last position before they were hidden is different
			UpdateObjects(0.0, true, false);
		}

		/// <summary>Updates the given car section element</summary>
		/// <param name="CarSection">The car section</param>
		/// <param name="GroupIndex">The group within the car section</param>
		/// <param name="ElementIndex">The element within the group</param>
		/// <param name="Position"></param>
		/// <param name="Direction"></param>
		/// <param name="Side"></param>
		/// <param name="Show"></param>
		/// <param name="TimeElapsed"></param>
		/// <param name="ForceUpdate"></param>
		/// <param name="EnableDamping"></param>
		private void UpdateCarSectionElement(CarSection CarSection, int GroupIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		{
			Vector3 p;
			if (CarSection.Type == ObjectType.Overlay && (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = Position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSection.Groups[GroupIndex].Elements[ElementIndex].RefreshRate != 0.0)
			{
				if (CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSection.Groups[GroupIndex].Elements[ElementIndex].RefreshRate)
				{
					timeDelta = CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
					CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = TimeElapsed;
					CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
				CarSection.Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
				updatefunctions = true;
			}

			if (ForceUpdate)
			{
				updatefunctions = true;
			}

			CarSection.Groups[GroupIndex].Elements[ElementIndex].Update(baseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, false, CarSection.Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (CarSection.Groups[GroupIndex].Elements[ElementIndex].UpdateVAO)
			{
				VAOExtensions.CreateOrUpdateVAO(CarSection.Groups[GroupIndex].Elements[ElementIndex].internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}

		private void UpdateCarSectionTouchElement(CarSection CarSection, int GroupIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		{
			Vector3 p;
			if (CarSection.Type == ObjectType.Overlay && (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = Position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.RefreshRate != 0.0)
			{
				if (CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate >= CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.RefreshRate)
				{
					timeDelta = CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate;
					CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = TimeElapsed;
					CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate += TimeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate;
				CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate = TimeElapsed;
				updatefunctions = true;
			}

			if (ForceUpdate)
			{
				updatefunctions = true;
			}

			CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.Update(baseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, true, CarSection.Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.UpdateVAO)
			{
				VAOExtensions.CreateOrUpdateVAO(CarSection.Groups[GroupIndex].TouchElements[ElementIndex].Element.internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}
	}
}
