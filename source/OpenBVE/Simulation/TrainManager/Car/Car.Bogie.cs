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

			internal void LoadCarSections(ObjectManager.UnifiedObject currentObject)
			{
				int j = CarSections.Length;
				Array.Resize(ref CarSections, j + 1);
				CarSections[j] = new CarSection();
				if (currentObject is ObjectManager.StaticObject)
				{
					ObjectManager.StaticObject s = (ObjectManager.StaticObject)currentObject;
					CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
					CarSections[j].Elements[0] = new ObjectManager.AnimatedObject
					{
						States = new ObjectManager.AnimatedObjectState[1]
						
					};
					CarSections[j].Elements[0].States[0].Position = Vector3.Zero;
					CarSections[j].Elements[0].States[0].Object = s;
					CarSections[j].Elements[0].CurrentState = 0;
					CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
				}
				else if (currentObject is ObjectManager.AnimatedObjectCollection)
				{
					ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)currentObject;
					CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
					for (int h = 0; h < a.Objects.Length; h++)
					{
						CarSections[j].Elements[h] = a.Objects[h].Clone();
						CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
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

					if (CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
					{
						if (CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
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
					CarSections[SectionIndex].Elements[ElementIndex].Update(true, baseTrain, baseCar.Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta, true);
				}
			}

			internal void UpdateTopplingCantAndSpring()
			{
				if (CarSections.Length != 0)
				{
					//FRONT BOGIE

					// get direction, up and side vectors
					Vector3 d = new Vector3();
					Vector3 u;
					Vector3 s;
					{
						d.X = FrontAxle.Follower.WorldPosition.X -
						     RearAxle.Follower.WorldPosition.X;
						d.Y = FrontAxle.Follower.WorldPosition.Y -
						     RearAxle.Follower.WorldPosition.Y;
						d.Z = FrontAxle.Follower.WorldPosition.Z -
						     RearAxle.Follower.WorldPosition.Z;
						double t = 1.0 / Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
						d *= t;
						t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
						double ex = d.X * t;
						double ez = d.Z * t;
						s = new Vector3(ez, 0.0, -ex);
						u = Vector3.Cross(d, s);
					}
					// cant and radius
					
					//TODO: This currently uses the figures from the base car
					// apply position due to cant/toppling
					{
						double a = baseCar.Specs.CurrentRollDueToTopplingAngle +
						           baseCar.Specs.CurrentRollDueToCantAngle;
						double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
						double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
						Vector3 c = new Vector3(s.X * x + u.X * y, s.Y * x + u.Y * y, s.Z * x + u.Z * y);
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
						u.Rotate(d, cosa, sina);
						Up = u;
					}
					// apply pitching
					if (CurrentCarSection >= 0 &&
					    CarSections[CurrentCarSection].Overlay)
					{
						double a = baseCar.Specs.CurrentPitchDueToAccelerationAngle;
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						d.Rotate(s, cosa, sina);
						u.Rotate(s, cosa, sina);
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
						FrontAxle.Follower.WorldPosition.Rotate(s, cosa, sina);
						RearAxle.Follower.WorldPosition.Rotate(s, cosa, sina);
						FrontAxle.Follower.WorldPosition.X += cx;
						FrontAxle.Follower.WorldPosition.Y += cy;
						FrontAxle.Follower.WorldPosition.Z += cz;
						RearAxle.Follower.WorldPosition.X += cx;
						RearAxle.Follower.WorldPosition.Y += cy;
						RearAxle.Follower.WorldPosition.Z += cz;
						Up = u;
					}
				}
			}
		}
	}
}
