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
			/// <summary>Rear axle about which the car pivots</summary>
			internal Axle RearAxle;
			/// <summary>The front bogie</summary>
			internal Bogie FrontBogie;
			/// <summary>The rear bogie</summary>
			internal Bogie RearBogie;
			/// <summary>The horns attached to this car</summary>
			internal Horn[] Horns;
			/// <summary>The doors for this car</summary>
			internal Door[] Doors;

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
			/// <summary>Stores the camera restriction mode for the interior view of this car</summary>
			internal World.CameraRestrictionMode CameraRestrictionMode = World.CameraRestrictionMode.NotSpecified;
			/// <summary>Stores the camera interior camera alignment for this car</summary>
			internal World.CameraAlignment InteriorCamera;

			internal bool HasInteriorView = false;

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
				FrontAxle.Follower.Train = train;
				RearAxle.Follower.Train = train;
				BeaconReceiver.Train = train;
			}

			/// <summary>Moves the car</summary>
			/// <param name="Delta">The delta to move</param>
			/// <param name="TimeElapsed">The time elapsed</param>
			internal void Move(double Delta, double TimeElapsed)
			{
				if (baseTrain.State != TrainState.Disposed)
				{
					FrontAxle.Follower.Update(FrontAxle.Follower.TrackPosition + Delta, true, true);
					FrontBogie.FrontAxle.Follower.Update(FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					FrontBogie.RearAxle.Follower.Update(FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (baseTrain.State != TrainState.Disposed)
					{
						RearAxle.Follower.Update(RearAxle.Follower.TrackPosition + Delta, true, true);
						RearBogie.FrontAxle.Follower.Update(RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
						RearBogie.RearAxle.Follower.Update(RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
						if (baseTrain.State != TrainState.Disposed)
						{
							BeaconReceiver.Update(BeaconReceiver.TrackPosition + Delta, true, true);
						}
					}
				}
			}

			/// <summary>Call this method to update all track followers attached to the car</summary>
			/// <param name="NewTrackPosition">The track position change</param>
			/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
			/// <param name="AddTrackInaccurary">Whether to add track innaccuarcy</param>
			internal void UpdateTrackFollowers(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary)
			{
				//Car axles
				FrontAxle.Follower.Update(FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
				RearAxle.Follower.Update(RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
				//Front bogie axles
				FrontBogie.FrontAxle.Follower.Update(FrontBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
				FrontBogie.RearAxle.Follower.Update(FrontBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
				//Rear bogie axles

				RearBogie.FrontAxle.Follower.Update(RearBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
				RearBogie.RearAxle.Follower.Update(RearBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
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
				FrontAxle.Follower.Update(s + d, false, false);
				RearAxle.Follower.Update(s - d, false, false);
				double b = FrontAxle.Follower.TrackPosition - FrontAxle.Position + BeaconReceiverPosition;
				BeaconReceiver.Update(b, false, false);
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

			internal void UpdateRunSounds(double TimeElapsed)
			{
				const double factor = 0.04; // 90 km/h -> m/s -> 1/x
				double speed = Math.Abs(Specs.CurrentSpeed);
				if (Derailed)
				{
					speed = 0.0;
				}
				double pitch = speed * factor;
				double basegain;
				if (Specs.CurrentSpeed == 0.0)
				{
					if (Index != 0)
					{
						Sounds.RunNextReasynchronizationPosition = baseTrain.Cars[0].FrontAxle.Follower.TrackPosition;
					}
				}
				else if (Sounds.RunNextReasynchronizationPosition == double.MaxValue & FrontAxle.RunIndex >= 0)
				{
					double distance = Math.Abs(FrontAxle.Follower.TrackPosition - World.CameraTrackFollower.TrackPosition);
					const double minDistance = 150.0;
					const double maxDistance = 750.0;
					if (distance > minDistance)
					{
						if (FrontAxle.RunIndex < Sounds.Run.Length)
						{
							Sounds.SoundBuffer buffer = Sounds.Run[FrontAxle.RunIndex].Buffer;
							if (buffer != null)
							{
								double duration = OpenBve.Sounds.GetDuration(buffer);
								if (duration > 0.0)
								{
									double offset = distance > maxDistance ? 25.0 : 300.0;
									Sounds.RunNextReasynchronizationPosition = duration * Math.Ceiling((baseTrain.Cars[0].FrontAxle.Follower.TrackPosition + offset) / duration);
								}
							}
						}
					}
				}
				if (FrontAxle.Follower.TrackPosition >= Sounds.RunNextReasynchronizationPosition)
				{
					Sounds.RunNextReasynchronizationPosition = double.MaxValue;
					basegain = 0.0;
				}
				else
				{
					basegain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;
				}
				for (int j = 0; j < Sounds.Run.Length; j++)
				{
					if (j == RearAxle.RunIndex | j == RearAxle.RunIndex)
					{
						Sounds.RunVolume[j] += 3.0 * TimeElapsed;
						if (Sounds.RunVolume[j] > 1.0) Sounds.RunVolume[j] = 1.0;
					}
					else
					{
						Sounds.RunVolume[j] -= 3.0 * TimeElapsed;
						if (Sounds.RunVolume[j] < 0.0) Sounds.RunVolume[j] = 0.0;
					}
					double gain = basegain * Sounds.RunVolume[j];
					if (OpenBve.Sounds.IsPlaying(Sounds.Run[j].Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Sounds.Run[j].Source.Pitch = pitch;
							Sounds.Run[j].Source.Volume = gain;
						}
						else
						{
							OpenBve.Sounds.StopSound(Sounds.Run[j].Source);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						Sounds.SoundBuffer buffer = Sounds.Run[j].Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Sounds.Run[j].Position;
							Sounds.Run[j].Source = OpenBve.Sounds.PlaySound(buffer, pitch, gain, pos, baseTrain, Index, true);
						}
					}
				}
			}

			internal void UpdateMotorSounds(double TimeElapsed)
			{
				if (!this.Specs.IsMotorCar)
				{
					return;
				}
				OpenBveApi.Math.Vector3 pos = Sounds.Motor.Position;
				double speed = Math.Abs(Specs.CurrentPerceivedSpeed);
				int idx = (int)Math.Round(speed * Sounds.Motor.SpeedConversionFactor);
				int odir = Sounds.Motor.CurrentAccelerationDirection;
				int ndir = Math.Sign(Specs.CurrentAccelerationOutput);
				for (int h = 0; h < 2; h++)
				{
					int j = h == 0 ? TrainManager.MotorSound.MotorP1 : TrainManager.MotorSound.MotorP2;
					int k = h == 0 ? TrainManager.MotorSound.MotorB1 : TrainManager.MotorSound.MotorB2;
					if (odir > 0 & ndir <= 0)
					{
						if (j < Sounds.Motor.Tables.Length)
						{
							OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
							Sounds.Motor.Tables[j].Source = null;
							Sounds.Motor.Tables[j].Buffer = null;
						}
					}
					else if (odir < 0 & ndir >= 0)
					{
						if (k < Sounds.Motor.Tables.Length)
						{
							OpenBve.Sounds.StopSound(Sounds.Motor.Tables[k].Source);
							Sounds.Motor.Tables[k].Source = null;
							Sounds.Motor.Tables[k].Buffer = null;
						}
					}
					if (ndir != 0)
					{
						if (ndir < 0) j = k;
						if (j < Sounds.Motor.Tables.Length)
						{
							int idx2 = idx;
							if (idx2 >= Sounds.Motor.Tables[j].Entries.Length)
							{
								idx2 = Sounds.Motor.Tables[j].Entries.Length - 1;
							}
							if (idx2 >= 0)
							{
								Sounds.SoundBuffer obuf = Sounds.Motor.Tables[j].Buffer;
								Sounds.SoundBuffer nbuf = Sounds.Motor.Tables[j].Entries[idx2].Buffer;
								double pitch = Sounds.Motor.Tables[j].Entries[idx2].Pitch;
								double gain = Sounds.Motor.Tables[j].Entries[idx2].Gain;
								if (ndir == 1)
								{
									// power
									double max = Specs.AccelerationCurveMaximum;
									if (max != 0.0)
									{
										double cur = Specs.CurrentAccelerationOutput;
										if (cur < 0.0) cur = 0.0;
										gain *= Math.Pow(cur / max, 0.25);
									}
								}
								else if (ndir == -1)
								{
									// brake
									double max = Specs.BrakeDecelerationAtServiceMaximumPressure(this.baseTrain.Handles.Brake.Actual);
									if (max != 0.0)
									{
										double cur = -Specs.CurrentAccelerationOutput;
										if (cur < 0.0) cur = 0.0;
										gain *= Math.Pow(cur / max, 0.25);
									}
								}
								if (obuf != nbuf)
								{
									OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
									if (nbuf != null)
									{
										Sounds.Motor.Tables[j].Source = OpenBve.Sounds.PlaySound(nbuf, pitch, gain, pos, baseTrain, Index, true);
										Sounds.Motor.Tables[j].Buffer = nbuf;
									}
									else
									{
										Sounds.Motor.Tables[j].Source = null;
										Sounds.Motor.Tables[j].Buffer = null;
									}
								}
								else if (nbuf != null)
								{
									if (Sounds.Motor.Tables[j].Source != null)
									{
										Sounds.Motor.Tables[j].Source.Pitch = pitch;
										Sounds.Motor.Tables[j].Source.Volume = gain;
									}
								}
								else
								{
									OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
									Sounds.Motor.Tables[j].Source = null;
									Sounds.Motor.Tables[j].Buffer = null;
								}
							}
							else
							{
								OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
								Sounds.Motor.Tables[j].Source = null;
								Sounds.Motor.Tables[j].Buffer = null;
							}
						}
					}
				}
				Sounds.Motor.CurrentAccelerationDirection = ndir;
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
						CarSections[j].Elements[h] = a.Objects[h].Clone();
						CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
					}
				}
			}

			/// <summary>Changes the currently visible car section</summary>
			/// <param name="newCarSection">The type of new car section to display</param>
			internal void ChangeCarSection(CarSectionType newCarSection)
			{
				for (int i = 0; i < CarSections.Length; i++)
				{
					for (int j = 0; j < CarSections[i].Elements.Length; j++)
					{
						int o = CarSections[i].Elements[j].ObjectIndex;
						Renderer.HideObject(o);
					}
				}
				switch (newCarSection)
				{
					case CarSectionType.NotVisible:
						this.CurrentCarSection = -1;
						break;
					case CarSectionType.Interior:
						if (this.HasInteriorView && this.CarSections.Length > 0)
						{
							this.CurrentCarSection = 0;
							this.CarSections[0].Initialize(true);
							for (int j = 0; j < CarSections[0].Elements.Length; j++)
							{
								int o = CarSections[0].Elements[j].ObjectIndex;
								if (CarSections[0].Overlay)
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
								}
								else
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
								}
							}
							break;
						}
						this.CurrentCarSection = -1;
						break;
					case CarSectionType.Exterior:
						if (this.HasInteriorView && this.CarSections.Length > 1)
						{
							this.CurrentCarSection = 1;
							this.CarSections[1].Initialize(true);
							for (int j = 0; j < CarSections[1].Elements.Length; j++)
							{
								int o = CarSections[1].Elements[j].ObjectIndex;
								if (CarSections[1].Overlay)
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
								}
								else
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
								}
							}
							break;
						}
						else if(!this.HasInteriorView && this.CarSections.Length > 0)
						{
							this.CurrentCarSection = 0;
							this.CarSections[0].Initialize(true);
							for (int j = 0; j < CarSections[0].Elements.Length; j++)
							{
								int o = CarSections[0].Elements[j].ObjectIndex;
								if (CarSections[0].Overlay)
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
								}
								else
								{
									Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
								}
							}
							break;
						}
						this.CurrentCarSection = -1;
						break;
				}
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
						if (i == this.FrontAxle.FlangeIndex | i == this.RearAxle.FlangeIndex)
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

			/// <summary>Updates the position of the camera relative to this car</summary>
			internal void UpdateCamera()
			{
				double dx = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
				double dy = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
				double dz = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
				double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
				dx *= t; dy *= t; dz *= t;
				double ux = Up.X;
				double uy = Up.Y;
				double uz = Up.Z;
				double sx = dz * uy - dy * uz;
				double sy = dx * uz - dz * ux;
				double sz = dy * ux - dx * uy;
				double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
				double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
				double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
				double cx, cy, cz;
				if (this.HasInteriorView)
				{
					cx = rx + sx * Driver.X + ux * Driver.Y + dx * Driver.Z;
					cy = ry + sy * Driver.X + uy * Driver.Y + dy * Driver.Z;
					cz = rz + sz * Driver.X + uz * Driver.Y + dz * Driver.Z;
				}
				else
				{
					/*
					 * If we do not have an interior view, base the camera update on the driver car
					 */
					Vector3 d = this.baseTrain.Cars[this.baseTrain.DriverCar].Driver;
					cx = rx + sx * d.X + ux * d.Y + dx * d.Z;
					cy = ry + sy * d.X + uy * d.Y + dy * d.Z;
					cz = rz + sz * d.X + uz * d.Y + dz * d.Z;
				}
				World.CameraTrackFollower.WorldPosition = new Vector3(cx, cy, cz);
				World.CameraTrackFollower.WorldDirection = new Vector3(dx, dy, dz);
				World.CameraTrackFollower.WorldUp = new Vector3(ux, uy, uz);
				World.CameraTrackFollower.WorldSide = new Vector3(sx, sy, sz);
				double f = (Driver.Z - RearAxle.Position) / (FrontAxle.Position - RearAxle.Position);
				double tp = (1.0 - f) * RearAxle.Follower.TrackPosition + f * FrontAxle.Follower.TrackPosition;
				World.CameraTrackFollower.Update(tp, false, false);
			}
		}
	}
}
