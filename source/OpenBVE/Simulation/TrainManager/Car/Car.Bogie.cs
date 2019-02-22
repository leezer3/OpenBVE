using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents a bogie fitted to a train car</summary>
		internal class Bogie
		{
#pragma warning disable 0649
			/*
			 * Bogies currently share the parent car's physics, so these are not used
			 */
			internal double Width;
			internal double Height;
			internal double Length;
#pragma warning restore 0649
			/// <summary>Front axle about which the bogie pivots</summary>
			internal Axle FrontAxle;
			/// <summary>Rear axle about which the bogie pivots</summary>
			internal Axle RearAxle;
			internal Vector3 Up;
			/// <summary>The car sections (objects) attached to the bogie</summary>
			internal CarSection[] CarSections;
			/// <summary>The index of the current car section</summary>
			internal int CurrentCarSection;
			/// <summary>Whether currently visible from the in-game camera location</summary>
			internal bool CurrentlyVisible;
			/// <summary>Holds a reference to the base car</summary>
			private readonly Car baseCar;
			/// <summary>Holds a reference to the base train</summary>
			private Train baseTrain;

			internal Bogie(Train train, Car car)
			{
				baseTrain = train;
				baseCar = car;
				CarSections = new CarSection[] { };
			}

			internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
			{
				//Same hack: Check if any car sections are defined for the offending bogie
				if (CarSections.Length == 0)
				{
					return;
				}
				// calculate positions and directions for section element update
				Vector3 d = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
				Vector3 u, s;
				double t = d.NormSquared();
				if (t != 0.0)
				{
					t = 1.0 / Math.Sqrt(t);
					d *= t;
					u = new Vector3(Up);
					s.X = d.Z * u.Y - d.Y * u.Z;
					s.Y = d.X * u.Z - d.Z * u.X;
					s.Z = d.Y * u.X - d.X * u.Y;
				}
				else
				{
					u = Vector3.Down;
					s = Vector3.Right;
				}
				Vector3 p = new Vector3(0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition));
				p -= d * (0.5 * (FrontAxle.Position + RearAxle.Position));
				// determine visibility
				Vector3 cd = new Vector3(p - World.AbsoluteCameraPosition);
				double dist = cd.NormSquared();
				double bid = Interface.CurrentOptions.ViewingDistance + Length;
				CurrentlyVisible = dist < bid * bid;
				// brightness
				byte dnb;
				{
					float Brightness = (float)(baseCar.Brightness.NextTrackPosition - baseCar.Brightness.PreviousTrackPosition);
					if (Brightness != 0.0f)
					{
						Brightness = (float)(FrontAxle.Follower.TrackPosition - baseCar.Brightness.PreviousTrackPosition) / Brightness;
						if (Brightness < 0.0f) Brightness = 0.0f;
						if (Brightness > 1.0f) Brightness = 1.0f;
						Brightness = baseCar.Brightness.PreviousBrightness * (1.0f - Brightness) + baseCar.Brightness.NextBrightness * Brightness;
					}
					else
					{
						Brightness = baseCar.Brightness.PreviousBrightness;
					}
					dnb = (byte)Math.Round(255.0 * (double)(1.0 - Brightness));
				}
				// update current section
				int cs = CurrentCarSection;
				if (cs >= 0)
				{
					for (int i = 0; i < CarSections[cs].Groups[0].Elements.Length; i++)
					{
						UpdateSectionElement(cs, i, p, d, u, s, CurrentlyVisible, TimeElapsed, ForceUpdate);

						// brightness change
						int o = CarSections[cs].Groups[0].Elements[i].ObjectIndex;
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

			internal void LoadCarSections(UnifiedObject currentObject)
			{
				int j = CarSections.Length;
				Array.Resize(ref CarSections, j + 1);
				CarSections[j] = new CarSection
				{
					Groups = new ElementsGroup[1]
				};
				CarSections[j].Groups[0] = new ElementsGroup();
				if (currentObject is ObjectManager.StaticObject)
				{
					ObjectManager.StaticObject s = (ObjectManager.StaticObject)currentObject;
					CarSections[j].Groups[0].Elements = new ObjectManager.AnimatedObject[1];
					CarSections[j].Groups[0].Elements[0] = new ObjectManager.AnimatedObject
					{
						States = new ObjectManager.AnimatedObjectState[1]
						
					};
					CarSections[j].Groups[0].Elements[0].States[0].Position = Vector3.Zero;
					CarSections[j].Groups[0].Elements[0].States[0].Object = s;
					CarSections[j].Groups[0].Elements[0].CurrentState = 0;
					CarSections[j].Groups[0].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
				}
				else if (currentObject is ObjectManager.AnimatedObjectCollection)
				{
					ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)currentObject;
					CarSections[j].Groups[0].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
					for (int h = 0; h < a.Objects.Length; h++)
					{
						CarSections[j].Groups[0].Elements[h] = a.Objects[h].Clone();
						CarSections[j].Groups[0].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
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
						int o = CarSections[i].Groups[0].Elements[j].ObjectIndex;
						Renderer.HideObject(o);
					}
				}
				if (SectionIndex >= 0)
				{
					CarSections[SectionIndex].Initialize(CurrentlyVisible);
					for (int j = 0; j < CarSections[SectionIndex].Groups[0].Elements.Length; j++)
					{
						int o = CarSections[SectionIndex].Groups[0].Elements[j].ObjectIndex;
						Renderer.ShowObject(o, ObjectType.Dynamic);
					}
				}
				CurrentCarSection = SectionIndex;
				UpdateObjects(0.0, true);
			}

			private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate)
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
					CarSections[SectionIndex].Groups[0].Elements[ElementIndex].Update(true, baseTrain, baseCar.Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, CarSections[SectionIndex].Groups[0].Overlay, updatefunctions, Show, timeDelta, true);
				}
			}

			internal void UpdateTopplingCantAndSpring()
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
						double a = baseCar.Specs.CurrentRollDueToTopplingAngle +
						           baseCar.Specs.CurrentRollDueToCantAngle;
						double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
						double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
						Vector3 c = new Vector3(s.X * x + Up.X * y, s.Y * x + Up.Y * y, s.Z * x + Up.Z * y);
						FrontAxle.Follower.WorldPosition += c;
						RearAxle.Follower.WorldPosition += c;
					}
					// apply rolling
					{
						double a = -baseCar.Specs.CurrentRollDueToTopplingAngle -
						           baseCar.Specs.CurrentRollDueToCantAngle;
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						s.Rotate(d, cosa, sina);
						Up.Rotate(d, cosa, sina);
					}
					// apply pitching
					if (CurrentCarSection >= 0 &&
						CarSections[CurrentCarSection].Groups[0].Overlay)
					{
						double a = baseCar.Specs.CurrentPitchDueToAccelerationAngle;
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						d.Rotate(s, cosa, sina);
						Up.Rotate(s, cosa, sina);
						Vector3 cc = 0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition);
						FrontAxle.Follower.WorldPosition -= cc;
						RearAxle.Follower.WorldPosition -= cc;
						FrontAxle.Follower.WorldPosition.Rotate(s, cosa, sina);
						RearAxle.Follower.WorldPosition.Rotate(s, cosa, sina);
						FrontAxle.Follower.WorldPosition += cc;
						RearAxle.Follower.WorldPosition += cc;
					}
				}
			}
		}
	}
}
