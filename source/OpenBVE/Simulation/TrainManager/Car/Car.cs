using System;
using OpenBveApi.Math;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal class Car
		{
			/// <summary>Width in meters</summary>
			internal double Width;
			/// <summary>Height in meters</summary>
			internal double Height;
			/// <summary>Length in meters</summary>
			internal double Length;
			/// <summary>Front axle about which the car pivots</summary>
			internal Axle FrontAxle;
			internal Bogie FrontBogie;
			/// <summary>Rear axle about which the car pivots</summary>
			internal Axle RearAxle;
			internal Bogie RearBogie;
			internal Vector3 Up;
			/// <summary>The car sections (objects) attached to the car</summary>
			internal CarSection[] CarSections;
			/// <summary>The index of the current car section</summary>
			internal int CurrentCarSection;
			/// <summary>The driver's eye position within the car</summary>
			internal Vector3 Driver;
			/// <summary>The current yaw of the driver's eyes</summary>
			internal double DriverYaw;
			/// <summary>The current pitch of the driver's eyes</summary>
			internal double DriverPitch;
			internal CarSpecs Specs;
			internal CarSounds Sounds;
			/// <summary>Whether currently visible from the in-game camera location</summary>
			internal bool CurrentlyVisible;
			/// <summary>Whether currently derailed</summary>
			internal bool Derailed;
			/// <summary>Whether currently toppled over</summary>
			internal bool Topples;
			internal CarBrightness Brightness;
			internal double BeaconReceiverPosition;
			internal TrackManager.TrackFollower BeaconReceiver;
			/// <summary>A reference to the base train</summary>
			internal Train baseTrain;
			/// <summary>The index of the car within the train</summary>
			internal int Index;

			internal struct CarBrightness
			{
				internal float PreviousBrightness;
				internal double PreviousTrackPosition;
				internal float NextBrightness;
				internal double NextTrackPosition;
			}

			internal Car(Train train, int index)
			{
				baseTrain = train;
				Index = index;
				CarSections = new CarSection[] { };
			}

			/// <summary>Moves the car</summary>
			/// <param name="Delta">The delta to move</param>
			/// <param name="TimeElapsed">The time elapsed</param>
			internal void Move(double Delta, double TimeElapsed)
			{
				if (baseTrain.State != TrainState.Disposed)
				{
					TrackManager.UpdateTrackFollower(ref FrontAxle.Follower, FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref FrontBogie.FrontAxle.Follower, FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref FrontBogie.RearAxle.Follower, FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (baseTrain.State != TrainState.Disposed)
					{
						TrackManager.UpdateTrackFollower(ref RearAxle.Follower, RearAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref RearBogie.FrontAxle.Follower, RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref RearBogie.RearAxle.Follower, RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
						if (baseTrain.State != TrainState.Disposed)
						{
							TrackManager.UpdateTrackFollower(ref BeaconReceiver, BeaconReceiver.TrackPosition + Delta, true, true);
						}
					}
				}
			}

			/// <summary>Initializes the car</summary>
			internal void Initialize()
			{
				for (int i = 0; i < CarSections.Length; i++)
				{
					CarSections[i].Initialize(false);
				}
				for (int i = 0; i < FrontBogie.CarSections.Length; i++)
				{
					FrontBogie.CarSections[i].Initialize(false);
				}
				for (int i = 0; i < RearBogie.CarSections.Length; i++)
				{
					RearBogie.CarSections[i].Initialize(false);
				}
				Brightness.PreviousBrightness = 1.0f;
				Brightness.NextBrightness = 1.0f;
			}

			/// <summary>Synchronizes the car after a period of infrequent updates</summary>
			internal void Syncronize()
			{
				double s = 0.5 * (FrontAxle.Follower.TrackPosition + RearAxle.Follower.TrackPosition);
				double d = 0.5 * (FrontAxle.Follower.TrackPosition - RearAxle.Follower.TrackPosition);
				TrackManager.UpdateTrackFollower(ref FrontAxle.Follower, s + d, false, false);
				TrackManager.UpdateTrackFollower(ref RearAxle.Follower, s - d, false, false);
				double b = FrontAxle.Follower.TrackPosition - FrontAxle.Position + BeaconReceiverPosition;
				TrackManager.UpdateTrackFollower(ref BeaconReceiver, b, false, false);
			}

			/// <summary>Loads Car Sections (Exterior objects etc.) for this car</summary>
			/// <param name="currentObject">The object to add to the car sections array</param>
			internal void LoadCarSections(ObjectManager.UnifiedObject currentObject)
			{
				int j = CarSections.Length;
				Array.Resize(ref CarSections, j + 1);
				CarSections[j] = new CarSection();
				if (currentObject is ObjectManager.StaticObject)
				{
					ObjectManager.StaticObject s = (ObjectManager.StaticObject)currentObject;
					CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
					CarSections[j].Elements[0] = new ObjectManager.AnimatedObject();
					CarSections[j].Elements[0].States = new ObjectManager.AnimatedObjectState[1];
					CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
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
						CarSections[j].Elements[h] = a.Objects[h];
						CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
					}
				}
			}

			/// <summary>Changes the currently visible car section</summary>
			/// <param name="SectionIndex">The index of the new car section to display</param>
			internal void ChangeCarSection(int SectionIndex)
			{
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
					CarSections[SectionIndex].Initialize(true);
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
				baseTrain.Cars[Index].UpdateObjects(0.0, true, false);
			}

			/// <summary>Updates the currently displayed objects for this car</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			/// <param name="ForceUpdate">Whether this is a forced update</param>
			/// <param name="EnableDamping">Whether damping is applied during this update (Skipped on transitions between camera views etc.)</param>
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
					float b = (float)(Brightness.NextTrackPosition - Brightness.PreviousTrackPosition);

					//1.0f represents a route brightness value of 255
					//0.0f represents a route brightness value of 0

					if (b != 0.0f)
					{
						b = (float)(FrontAxle.Follower.TrackPosition - Brightness.PreviousTrackPosition) / b;
						if (b < 0.0f) b = 0.0f;
						if (b > 1.0f) b = 1.0f;
						b = Brightness.PreviousBrightness * (1.0f - b) + Brightness.NextBrightness * b;
					}
					else
					{
						b = Brightness.PreviousBrightness;
					}
					//Calculate the cab brightness
					double ccb = Math.Round(255.0 * (double)(1.0 - b));
					//DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
					dnb = (byte)Math.Min(Renderer.DynamicCabBrightness, ccb);
				}
				// update current section
				int s = CurrentCarSection;
				if (s >= 0)
				{
					for (int i = 0; i < CarSections[s].Elements.Length; i++)
					{
						UpdateCarSectionElement(s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

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

			/// <summary>Updates the given car section element</summary>
			/// <param name="SectionIndex">The car section</param>
			/// <param name="ElementIndex">The element within the car section</param>
			/// <param name="Position"></param>
			/// <param name="Direction"></param>
			/// <param name="Up"></param>
			/// <param name="Side"></param>
			/// <param name="Show"></param>
			/// <param name="TimeElapsed"></param>
			/// <param name="ForceUpdate"></param>
			/// <param name="EnableDamping"></param>
			internal void UpdateCarSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
			{
				Vector3 p;
				if (CarSections[SectionIndex].Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
				{
					p = new Vector3(Driver.X, Driver.Y, Driver.Z);
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
				CarSections[SectionIndex].Elements[ElementIndex].Update(true, baseTrain, Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta, EnableDamping);
			}

			internal void UpdateTopplingCantAndSpring(double TimeElapsed)
			{
				// get direction, up and side vectors
				double dx, dy, dz;
				double ux, uy, uz;
				double sx, sy, sz;
				{
					dx = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
					dy = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
					dz = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
					double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
					dx *= t; dy *= t; dz *= t;
					t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
					double ex = dx * t;
					double ez = dz * t;
					sx = ez;
					sy = 0.0;
					sz = -ex;
					World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
				}
				// cant and radius
				double c;
				{
					double ca = FrontAxle.Follower.CurveCant;
					double cb = RearAxle.Follower.CurveCant;
					c = Math.Tan(0.5 * (Math.Atan(ca) + Math.Atan(cb)));
				}
				double r, rs;
				if (FrontAxle.Follower.CurveRadius != 0.0 & RearAxle.Follower.CurveRadius != 0.0)
				{
					r = Math.Sqrt(Math.Abs(FrontAxle.Follower.CurveRadius * RearAxle.Follower.CurveRadius));
					rs = (double)Math.Sign(FrontAxle.Follower.CurveRadius + RearAxle.Follower.CurveRadius);
				}
				else if (FrontAxle.Follower.CurveRadius != 0.0)
				{
					r = Math.Abs(FrontAxle.Follower.CurveRadius);
					rs = (double)Math.Sign(FrontAxle.Follower.CurveRadius);
				}
				else if (RearAxle.Follower.CurveRadius != 0.0)
				{
					r = Math.Abs(RearAxle.Follower.CurveRadius);
					rs = (double)Math.Sign(RearAxle.Follower.CurveRadius);
				}
				else
				{
					r = 0.0;
					rs = 0.0;
				}
				// roll due to shaking
				{

					double a0 = Specs.CurrentRollDueToShakingAngle;
					double a1;
					if (Specs.CurrentRollShakeDirection != 0.0)
					{
						const double c0 = 0.03;
						const double c1 = 0.15;
						a1 = c1 * Math.Atan(c0 * Specs.CurrentRollShakeDirection);
						double d = 0.5 + Specs.CurrentRollShakeDirection * Specs.CurrentRollShakeDirection;
						if (Specs.CurrentRollShakeDirection < 0.0)
						{
							Specs.CurrentRollShakeDirection += d * TimeElapsed;
							if (Specs.CurrentRollShakeDirection > 0.0) Specs.CurrentRollShakeDirection = 0.0;
						}
						else
						{
							Specs.CurrentRollShakeDirection -= d * TimeElapsed;
							if (Specs.CurrentRollShakeDirection < 0.0) Specs.CurrentRollShakeDirection = 0.0;
						}
					}
					else
					{
						a1 = 0.0;
					}
					double SpringAcceleration;
					if (!Derailed)
					{
						SpringAcceleration = 15.0 * Math.Abs(a1 - a0);
					}
					else
					{
						SpringAcceleration = 1.5 * Math.Abs(a1 - a0);
					}
					double SpringDeceleration = 0.25 * SpringAcceleration;
					Specs.CurrentRollDueToShakingAngularSpeed += (double)Math.Sign(a1 - a0) * SpringAcceleration * TimeElapsed;
					double x = (double)Math.Sign(Specs.CurrentRollDueToShakingAngularSpeed) * SpringDeceleration * TimeElapsed;
					if (Math.Abs(x) < Math.Abs(Specs.CurrentRollDueToShakingAngularSpeed))
					{
						Specs.CurrentRollDueToShakingAngularSpeed -= x;
					}
					else
					{
						Specs.CurrentRollDueToShakingAngularSpeed = 0.0;
					}
					a0 += Specs.CurrentRollDueToShakingAngularSpeed * TimeElapsed;
					Specs.CurrentRollDueToShakingAngle = a0;
				}
				// roll due to cant (incorporates shaking)
				{
					double cantAngle = Math.Atan(c / Game.RouteRailGauge);
					Specs.CurrentRollDueToCantAngle = cantAngle + Specs.CurrentRollDueToShakingAngle;
				}
				// pitch due to acceleration
				{
					for (int i = 0; i < 3; i++)
					{
						double a, v, j;
						if (i == 0)
						{
							a = Specs.CurrentAcceleration;
							v = Specs.CurrentPitchDueToAccelerationFastValue;
							j = 1.8;
						}
						else if (i == 1)
						{
							a = Specs.CurrentPitchDueToAccelerationFastValue;
							v = Specs.CurrentPitchDueToAccelerationMediumValue;
							j = 1.2;
						}
						else
						{
							a = Specs.CurrentPitchDueToAccelerationFastValue;
							v = Specs.CurrentPitchDueToAccelerationSlowValue;
							j = 1.0;
						}
						double d = a - v;
						if (d < 0.0)
						{
							v -= j * TimeElapsed;
							if (v < a) v = a;
						}
						else
						{
							v += j * TimeElapsed;
							if (v > a) v = a;
						}
						if (i == 0)
						{
							Specs.CurrentPitchDueToAccelerationFastValue = v;
						}
						else if (i == 1)
						{
							Specs.CurrentPitchDueToAccelerationMediumValue = v;
						}
						else
						{
							Specs.CurrentPitchDueToAccelerationSlowValue = v;
						}
					}
					{
						double d = Specs.CurrentPitchDueToAccelerationSlowValue - Specs.CurrentPitchDueToAccelerationFastValue;
						Specs.CurrentPitchDueToAccelerationTargetAngle = 0.03 * Math.Atan(d);
					}
					{
						double a = 3.0 * (double)Math.Sign(Specs.CurrentPitchDueToAccelerationTargetAngle - Specs.CurrentPitchDueToAccelerationAngle);
						Specs.CurrentPitchDueToAccelerationAngularSpeed += a * TimeElapsed;
						double s = Math.Abs(Specs.CurrentPitchDueToAccelerationTargetAngle - Specs.CurrentPitchDueToAccelerationAngle);
						if (Math.Abs(Specs.CurrentPitchDueToAccelerationAngularSpeed) > s)
						{
							Specs.CurrentPitchDueToAccelerationAngularSpeed = s * (double)Math.Sign(Specs.CurrentPitchDueToAccelerationAngularSpeed);
						}
						Specs.CurrentPitchDueToAccelerationAngle += Specs.CurrentPitchDueToAccelerationAngularSpeed * TimeElapsed;
					}
				}
				// derailment
				if (Interface.CurrentOptions.Derailments & !Derailed)
				{
					double a = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
					double sa = (double)Math.Sign(a);
					double tc = Specs.CriticalTopplingAngle;
					if (a * sa > tc)
					{
						baseTrain.Derail(Index, TimeElapsed);
					}
				}
				// toppling roll
				if (Interface.CurrentOptions.Toppling | Derailed)
				{
					double a = Specs.CurrentRollDueToTopplingAngle;
					double ab = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
					double h = Specs.CenterOfGravityHeight;
					double s = Math.Abs(Specs.CurrentSpeed);
					double rmax = 2.0 * h * s * s / (Game.RouteAccelerationDueToGravity * Game.RouteRailGauge);
					double ta;
					Topples = false;
					if (Derailed)
					{
						double sab = (double)Math.Sign(ab);
						ta = 0.5 * Math.PI * (sab == 0.0 ? Program.RandomNumberGenerator.NextDouble() < 0.5 ? -1.0 : 1.0 : sab);
					}
					else
					{
						if (r != 0.0)
						{
							if (r < rmax)
							{
								double s0 = Math.Sqrt(r * Game.RouteAccelerationDueToGravity * Game.RouteRailGauge / (2.0 * h));
								const double fac = 0.25; // arbitrary coefficient
								ta = -fac * (s - s0) * rs;
								baseTrain.Topple(Index, TimeElapsed);
							}
							else
							{
								ta = 0.0;
							}
						}
						else
						{
							ta = 0.0;
						}
					}
					double td;
					if (Derailed)
					{
						td = Math.Abs(ab);
						if (td < 0.1) td = 0.1;
					}
					else
					{
						td = 1.0;
					}
					if (a > ta)
					{
						double d = a - ta;
						if (td > d) td = d;
						a -= td * TimeElapsed;
					}
					else if (a < ta)
					{
						double d = ta - a;
						if (td > d) td = d;
						a += td * TimeElapsed;
					}
					Specs.CurrentRollDueToTopplingAngle = a;
				}
				else
				{
					Specs.CurrentRollDueToTopplingAngle = 0.0;
				}
				// apply position due to cant/toppling
				{
					double a = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
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
					double a = -Specs.CurrentRollDueToTopplingAngle - Specs.CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
					Up.X = ux;
					Up.Y = uy;
					Up.Z = uz;
				}
				// apply pitching
				if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Overlay)
				{
					double a = Specs.CurrentPitchDueToAccelerationAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
					double cx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
					double cy = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
					double cz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
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
				// spring sound
				{
					double a = Specs.CurrentRollDueToShakingAngle;
					double diff = a - Sounds.SpringPlayedAngle;
					const double angleTolerance = 0.001;
					if (diff < -angleTolerance)
					{
						Sounds.SoundBuffer buffer = Sounds.SpringL.Buffer;
						if (buffer != null)
						{
							if (!OpenBve.Sounds.IsPlaying(Sounds.SpringL.Source))
							{
								Vector3 pos = Sounds.SpringL.Position;
								Sounds.SpringL.Source = OpenBve.Sounds.PlaySound(buffer, 1.0, 1.0, pos, baseTrain, Index, false);
							}
						}
						Sounds.SpringPlayedAngle = a;
					}
					else if (diff > angleTolerance)
					{
						Sounds.SoundBuffer buffer = Sounds.SpringR.Buffer;
						if (buffer != null)
						{
							if (!OpenBve.Sounds.IsPlaying(Sounds.SpringR.Source))
							{
								Vector3 pos = Sounds.SpringR.Position;
								Sounds.SpringR.Source = OpenBve.Sounds.PlaySound(buffer, 1.0, 1.0, pos, baseTrain, Index, false);
							}
						}
						Sounds.SpringPlayedAngle = a;
					}
				}
				// flange sound
				{
					/*
					 * This determines the amount of flange noise as a result of the angle at which the
					 * line that forms between the axles hits the rail, i.e. the less perpendicular that
					 * line is to the rails, the more flange noise there will be.
					 * */
					Vector3 d = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
					World.Normalize(ref d.X, ref d.Y, ref d.Z);
					double b0 = d.X * RearAxle.Follower.WorldSide.X + d.Y * RearAxle.Follower.WorldSide.Y + d.Z * RearAxle.Follower.WorldSide.Z;
					double b1 = d.X * FrontAxle.Follower.WorldSide.X + d.Y * FrontAxle.Follower.WorldSide.Y + d.Z * FrontAxle.Follower.WorldSide.Z;
					double spd = Math.Abs(Specs.CurrentSpeed);
					double pitch = 0.5 + 0.04 * spd;
					double b2 = Math.Abs(b0) + Math.Abs(b1);
					double basegain = 0.5 * b2 * b2 * spd * spd;
					/*
					 * This determines additional flange noise as a result of the roll angle of the car
					 * compared to the roll angle of the rails, i.e. if the car bounces due to inaccuracies,
					 * there will be additional flange noise.
					 * */
					double cdti = Math.Abs(FrontAxle.Follower.CantDueToInaccuracy) + Math.Abs(RearAxle.Follower.CantDueToInaccuracy);
					basegain += 0.2 * spd * spd * cdti * cdti;
					/*
					 * This applies the settings.
					 * */
					if (basegain < 0.0) basegain = 0.0;
					if (basegain > 0.75) basegain = 0.75;
					if (pitch > Sounds.FlangePitch)
					{
						Sounds.FlangePitch += TimeElapsed;
						if (Sounds.FlangePitch > pitch) Sounds.FlangePitch = pitch;
					}
					else
					{
						Sounds.FlangePitch -= TimeElapsed;
						if (Sounds.FlangePitch < pitch) Sounds.FlangePitch = pitch;
					}
					pitch = Sounds.FlangePitch;
					for (int i = 0; i < Sounds.Flange.Length; i++)
					{
						if (i == Sounds.FrontAxleFlangeIndex | i == Sounds.RearAxleFlangeIndex)
						{
							Sounds.FlangeVolume[i] += TimeElapsed;
							if (Sounds.FlangeVolume[i] > 1.0) Sounds.FlangeVolume[i] = 1.0;
						}
						else
						{
							Sounds.FlangeVolume[i] -= TimeElapsed;
							if (Sounds.FlangeVolume[i] < 0.0) Sounds.FlangeVolume[i] = 0.0;
						}
						double gain = basegain * Sounds.FlangeVolume[i];
						if (OpenBve.Sounds.IsPlaying(Sounds.Flange[i].Source))
						{
							if (pitch > 0.01 & gain > 0.0001)
							{
								Sounds.Flange[i].Source.Pitch = pitch;
								Sounds.Flange[i].Source.Volume = gain;
							}
							else
							{
								Sounds.Flange[i].Source.Stop();
							}
						}
						else if (pitch > 0.02 & gain > 0.01)
						{
							Sounds.SoundBuffer buffer = Sounds.Flange[i].Buffer;
							if (buffer != null)
							{
								Vector3 pos = Sounds.Flange[i].Position;
								Sounds.Flange[i].Source = OpenBve.Sounds.PlaySound(buffer, pitch, gain, pos, baseTrain, Index, true);
							}
						}
					}
				}
			}
		}
	}
}
