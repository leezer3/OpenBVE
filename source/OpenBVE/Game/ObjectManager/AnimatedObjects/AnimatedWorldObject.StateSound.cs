using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal class AnimatedWorldObjectStateSound : WorldObject
		{
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;
			/// <summary>The curve radius at the object's track position</summary>
			internal double Radius;
			/// <summary>The sound buffer array</summary>
			internal Sounds.SoundBuffer[] Buffers;
			/// <summary>Whether a single buffer is used</summary>
			internal bool SingleBuffer;
			/// <summary>The sound source for this file</summary>
			internal Sounds.SoundSource Source;
			/// <summary>The pitch to play the sound at</summary>
			internal double currentPitch = 1.0;
			/// <summary>The volume to play the sound at it's origin</summary>
			internal double currentVolume = 1.0;

			internal bool PlayOnShow = true;

			internal bool PlayOnHide = true;

			private int lastState;

			internal override void Update(double TimeElapsed, bool ForceUpdate)
			{
				const double extraRadius = 10.0;
				double z = Object.TranslateZFunction == null ? 0.0 : Object.TranslateZFunction.LastResult;
				double pa = TrackPosition + z - Radius - extraRadius;
				double pb = TrackPosition + z + Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z - World.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z + World.BackgroundImageDistance + World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
						Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++)
						{
							if (TrainManager.Trains[j].State == TrainState.Available)
							{
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < TrackPosition)
								{
									distance = TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
								}
								else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > TrackPosition)
								{
									distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - TrackPosition;
								}
								else
								{
									distance = 0;
								}
								if (distance < trainDistance)
								{
									train = TrainManager.Trains[j];
									trainDistance = distance;
								}
							}
						}
						Object.Update(false, train, train == null ? 0 : train.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, false, true, true, timeDelta, true);
						if (this.Object.CurrentState != this.lastState && Loading.SimulationSetup)
						{
							if (this.SingleBuffer && this.Buffers[0] != null)
							{
								switch (this.Object.CurrentState)
								{
									case -1:
										if (this.PlayOnHide)
										{
											this.Source = Sounds.PlaySound(this.Buffers[0], this.currentPitch, this.currentVolume, this.Position, false);
										}
										break;
									case 0:
										if (this.PlayOnShow || this.lastState != -1)
										{
											this.Source = Sounds.PlaySound(this.Buffers[0], this.currentPitch, this.currentVolume, this.Position, false);
										}
										break;
									default:
										this.Source = Sounds.PlaySound(this.Buffers[0], this.currentPitch, this.currentVolume, this.Position, false);
										break;
								}
							}
							else
							{
								int bufferIndex = this.Object.CurrentState + 1;
								if (bufferIndex < this.Buffers.Length && this.Buffers[bufferIndex] != null)
								{
									switch (bufferIndex)
									{
										case 0:
											if (this.PlayOnHide)
											{
												this.Source = Sounds.PlaySound(this.Buffers[bufferIndex], this.currentPitch, this.currentVolume, this.Position, false);
											}
											break;
										case 1:
											if (this.PlayOnShow || this.lastState != -1)
											{
												this.Source = Sounds.PlaySound(this.Buffers[bufferIndex], this.currentPitch, this.currentVolume, this.Position, false);
											}
											break;
										default:
											this.Source = Sounds.PlaySound(this.Buffers[bufferIndex], this.currentPitch, this.currentVolume, this.Position, false);
											break;
									}
								}
							}
							
						}
					}
					else
					{
						Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!Visible)
					{
						Renderer.ShowObject(Object.ObjectIndex, ObjectType.Dynamic);
						Visible = true;
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
					if (Visible)
					{
						Renderer.HideObject(Object.ObjectIndex);
						Visible = false;
					}
				}
				this.lastState = this.Object.CurrentState;
			}

			internal void Create(Vector3 objectPosition, Transformation BaseTransformation, Transformation AuxTransformation, int objectSectionIndex, double objectTrackPosition, double Brightness)
			{
				int a = AnimatedWorldObjectsUsed;
				if (a >= AnimatedWorldObjects.Length)
				{
					Array.Resize<WorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
				}
				Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);

				var o = this.Object.Clone();
				o.ObjectIndex = CreateDynamicObject();
				AnimatedWorldObjectStateSound currentObject = new AnimatedWorldObjectStateSound
				{
					Position = objectPosition,
					Direction = FinalTransformation.Z,
					Up = FinalTransformation.Y,
					Side = FinalTransformation.X,
					Object = o,
					SectionIndex = objectSectionIndex,
					TrackPosition = objectTrackPosition,
					Buffers = Buffers,
					SingleBuffer = SingleBuffer,
					PlayOnShow = PlayOnShow,
					PlayOnHide = PlayOnHide
				};
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					if (currentObject.Object.States[i].Object == null)
					{
						currentObject.Object.States[i].Object = new StaticObject { RendererIndex =  -1 };
					}
				}
				double r = 0.0;
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
					{
						currentObject.Object.States[i].Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)this.Object.States[i].Object.Mesh.Materials[j].Color.R * Brightness);
						currentObject.Object.States[i].Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)this.Object.States[i].Object.Mesh.Materials[j].Color.G * Brightness);
						currentObject.Object.States[i].Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)this.Object.States[i].Object.Mesh.Materials[j].Color.B * Brightness);
					}
					for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Vertices.Length; j++)
					{
						double t = this.Object.States[i].Object.Mesh.Vertices[j].Coordinates.NormSquared();
						if (t > r) r = t;
					}
				}
				currentObject.Radius = Math.Sqrt(r);
				currentObject.Visible = false;
				currentObject.Object.Initialize(0, false, false);
				AnimatedWorldObjects[a] = currentObject;
				AnimatedWorldObjectsUsed++;
			}
		}
	}
}
