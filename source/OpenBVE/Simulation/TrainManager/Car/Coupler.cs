using System;
using static LibRender.CameraProperties;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class Coupler : AbstractCoupler
		{
			internal CarSection[] CarSections;

			internal int CurrentCarSection;

			internal Car FrontCar;

			internal Car RearCar;

			internal Train baseTrain;

			internal Coupler(double minimumDistance, double maximumDistance, Car frontCar, Car rearCar)
			{
				MinimumDistanceBetweenCars = minimumDistance;
				MaximumDistanceBetweenCars = maximumDistance;
				FrontCar = frontCar;
				RearCar = rearCar;
				CarSections = new CarSection[] { };
			}

			internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
			{
				// calculate positions and directions for section element update
				Vector3 d = new Vector3(FrontCar.RearAxle.Follower.WorldPosition - RearCar.FrontAxle.Follower.WorldPosition);
				Vector3 u, s;
				double t = d.NormSquared();
				if (t != 0.0)
				{
					t = 1.0 / Math.Sqrt(t);
					d *= t;
					u = new Vector3((FrontCar.Up + RearCar.Up) * 0.5);
					s.X = d.Z * u.Y - d.Y * u.Z;
					s.Y = d.X * u.Z - d.Z * u.X;
					s.Z = d.Y * u.X - d.X * u.Y;
				}
				else
				{
					u = Vector3.Down;
					s = Vector3.Right;
				}

				Vector3 p = new Vector3(0.5 * (FrontCar.RearAxle.Follower.WorldPosition + RearCar.FrontAxle.Follower.WorldPosition));
				// determine visibility
				Vector3 cd = new Vector3(p - Camera.AbsolutePosition);
				double dist = cd.NormSquared();
				double bid = Interface.CurrentOptions.ViewingDistance + FrontCar.Length;
				bool CurrentlyVisible = dist < bid * bid;
				// Updates the brightness value
				byte dnb;
				{
					float b = (float) (FrontCar.Brightness.NextTrackPosition - FrontCar.Brightness.PreviousTrackPosition);

					//1.0f represents a route brightness value of 255
					//0.0f represents a route brightness value of 0

					if (b != 0.0f)
					{
						b = (float) (FrontCar.RearAxle.Follower.TrackPosition - FrontCar.Brightness.PreviousTrackPosition) / b;
						if (b < 0.0f) b = 0.0f;
						if (b > 1.0f) b = 1.0f;
						b = FrontCar.Brightness.PreviousBrightness * (1.0f - b) + FrontCar.Brightness.NextBrightness * b;
					}
					else
					{
						b = FrontCar.Brightness.PreviousBrightness;
					}

					//Calculate the cab brightness
					double ccb = Math.Round(255.0 * (double) (1.0 - b));
					//DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
					dnb = (byte) Math.Min(LibRender.Renderer.DynamicCabBrightness, ccb);
				}
				// update current section
				int cs = CurrentCarSection;
				if (cs >= 0 && CarSections.Length > 0 && CarSections.Length >= cs)
				{
					if (CarSections[cs].Groups.Length > 0)
					{
						for (int i = 0; i < CarSections[cs].Groups[0].Elements.Length; i++)
						{
							UpdateSectionElement(cs, i, p, d, u, s, CurrentlyVisible, TimeElapsed, ForceUpdate);

							// brightness change
							if (CarSections[cs].Groups[0].Elements[i].internalObject != null)
							{
								for (int j = 0; j < CarSections[cs].Groups[0].Elements[i].internalObject.Mesh.Materials.Length; j++)
								{
									CarSections[cs].Groups[0].Elements[i].internalObject.Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
								}
							}
						}
					}
				}
			}

			internal void ChangeSection(int SectionIndex)
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
						LibRender.Renderer.HideObject(ref CarSections[i].Groups[0].Elements[j].internalObject);
					}
				}
				if (SectionIndex >= 0)
				{
					CarSections[SectionIndex].Initialize(FrontCar.CurrentlyVisible);
					for (int j = 0; j < CarSections[SectionIndex].Groups[0].Elements.Length; j++)
					{
						Program.CurrentHost.ShowObject(CarSections[SectionIndex].Groups[0].Elements[j].internalObject, ObjectType.Dynamic);
					}
				}
				CurrentCarSection = SectionIndex;
				UpdateObjects(0.0, true);
			}

			private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
			{
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
					CarSections[SectionIndex].Groups[0].Elements[ElementIndex].Update(true, baseTrain, FrontCar.Index, CurrentCarSection, (FrontCar.RearAxle.Follower.TrackPosition + RearCar.FrontAxle.Follower.TrackPosition) * 0.5, p, Direction, Up, Side, updatefunctions, Show, timeDelta, true);
				}
			}

			internal void LoadCarSections(UnifiedObject currentObject)
			{
				int j = CarSections.Length;
				Array.Resize(ref CarSections, j + 1);
				CarSections[j] = new CarSection
				{
					Groups = new ElementsGroup[1]
				};
				CarSections[j].Groups[0] = new ElementsGroup();
				if (currentObject is StaticObject)
				{
					StaticObject s = (StaticObject)currentObject;
					CarSections[j].Groups[0].Elements = new AnimatedObject[1];
					CarSections[j].Groups[0].Elements[0] = new AnimatedObject(Program.CurrentHost)
					{
						States = new AnimatedObjectState[1]
						
					};
					CarSections[j].Groups[0].Elements[0].States[0].Position = Vector3.Zero;
					CarSections[j].Groups[0].Elements[0].States[0].Object = s;
					CarSections[j].Groups[0].Elements[0].CurrentState = 0;
					ObjectManager.CreateDynamicObject(ref CarSections[j].Groups[0].Elements[0].internalObject);
				}
				else if (currentObject is AnimatedObjectCollection)
				{
					AnimatedObjectCollection a = (AnimatedObjectCollection)currentObject;
					CarSections[j].Groups[0].Elements = new AnimatedObject[a.Objects.Length];
					for (int h = 0; h < a.Objects.Length; h++)
					{
						CarSections[j].Groups[0].Elements[h] = a.Objects[h].Clone();
						ObjectManager.CreateDynamicObject(ref CarSections[j].Groups[0].Elements[h].internalObject);
					}
				}
			}
		}
	}
}
