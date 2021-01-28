using System;
using LibRender2.Trains;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve
{
	public partial class TrainManager
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
			internal readonly Axle FrontAxle;
			/// <summary>Rear axle about which the bogie pivots</summary>
			internal readonly Axle RearAxle;
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
			// We don't want this to be read-only if we ever manage to uncouple cars...
			// ReSharper disable once FieldCanBeMadeReadOnly.Local
			private Train baseTrain;
			
			internal Bogie(Train train, Car car)
			{
				baseTrain = train;
				baseCar = car;
				CarSections = new CarSection[] { };
				FrontAxle = new Axle(Program.CurrentHost, train, car);
				RearAxle = new Axle(Program.CurrentHost, train, car);
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
				Vector3 cd = new Vector3(p - Program.Renderer.Camera.AbsolutePosition);
				double dist = cd.NormSquared();
				double bid = Interface.CurrentOptions.ViewingDistance + Length;
				CurrentlyVisible = dist < bid * bid;
				// brightness
				byte dnb;
				{
					float Brightness = (float)(baseCar.Brightness.NextTrackPosition - baseCar.Brightness.PreviousTrackPosition);
					if (Brightness != 0.0f)
					{
						Brightness = (float)(baseCar.FrontAxle.Follower.TrackPosition - baseCar.Brightness.PreviousTrackPosition) / Brightness;
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
						if (CarSections[cs].Groups[0].Elements[i].internalObject != null)
						{
							CarSections[cs].Groups[0].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}
				}
			}

			internal void Reverse()
			{
				// reverse axle positions
				double temp = FrontAxle.Position;
				FrontAxle.Position = -RearAxle.Position;
				RearAxle.Position = -temp;
				if (CarSections.Length == 0 || CarSections == null)
				{
					return;
				}
				int idxToReverse = 0; //cannot have an interior view
				
				for (int i = 0; i < CarSections[idxToReverse].Groups[0].Elements.Length; i++)
				{
					for (int h = 0; h < CarSections[idxToReverse].Groups[0].Elements[i].States.Length; h++)
					{
						CarSections[idxToReverse].Groups[0].Elements[i].States[h].Prototype.ApplyScale(-1.0, 1.0, -1.0);
						Matrix4D t = CarSections[idxToReverse].Groups[0].Elements[i].States[h].Translation;
						t.Row3.X *= -1.0f;
						t.Row3.Z *= -1.0f;
						CarSections[idxToReverse].Groups[0].Elements[i].States[h].Translation = t;
					}
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateXDirection.X *= -1.0;
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateXDirection.Z *= -1.0;
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateYDirection.X *= -1.0;
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateYDirection.Z *= -1.0;
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateZDirection.X *= -1.0;
					CarSections[idxToReverse].Groups[0].Elements[i].TranslateZDirection.Z *= -1.0;
				}
			}

			internal void LoadCarSections(UnifiedObject currentObject, bool visibleFromInterior)
			{
				int j = CarSections.Length;
				Array.Resize(ref CarSections, j + 1);
				CarSections[j] = new CarSection(Program.CurrentHost, ObjectType.Dynamic);
				CarSections[j].VisibleFromInterior = visibleFromInterior;
				if (currentObject is StaticObject)
				{
					StaticObject s = (StaticObject)currentObject;
					CarSections[j].Groups[0].Elements = new AnimatedObject[1];
					CarSections[j].Groups[0].Elements[0] = new AnimatedObject(Program.CurrentHost)
					{
						States = new[] { new ObjectState(s) },
						CurrentState = 0
					};
					Program.CurrentHost.CreateDynamicObject(ref CarSections[j].Groups[0].Elements[0].internalObject);
				}
				else if (currentObject is AnimatedObjectCollection)
				{
					AnimatedObjectCollection a = (AnimatedObjectCollection)currentObject;
					CarSections[j].Groups[0].Elements = new AnimatedObject[a.Objects.Length];
					for (int h = 0; h < a.Objects.Length; h++)
					{
						CarSections[j].Groups[0].Elements[h] = a.Objects[h].Clone();
						Program.CurrentHost.CreateDynamicObject(ref CarSections[j].Groups[0].Elements[h].internalObject);
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
						Program.CurrentHost.HideObject(CarSections[i].Groups[0].Elements[j].internalObject);
					}
				}
				if (SectionIndex >= 0)
				{
					CarSections[SectionIndex].Initialize(CurrentlyVisible);
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
						double a = baseCar.Specs.RollDueToTopplingAngle +
						           baseCar.Specs.RollDueToCantAngle;
						double x = Math.Sign(a) * 0.5 * Program.CurrentRoute.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * (1.0 - Math.Cos(a));
						double y = Math.Abs(0.5 * Program.CurrentRoute.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * Math.Sin(a));
						Vector3 c = new Vector3(s.X * x + Up.X * y, s.Y * x + Up.Y * y, s.Z * x + Up.Z * y);
						FrontAxle.Follower.WorldPosition += c;
						RearAxle.Follower.WorldPosition += c;
					}
					// apply rolling
					{
						double a = -baseCar.Specs.RollDueToTopplingAngle -
						           baseCar.Specs.RollDueToCantAngle;
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						s.Rotate(d, cosa, sina);
						Up.Rotate(d, cosa, sina);
					}
					// apply pitching
					if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Groups[0].Type == ObjectType.Overlay)
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
