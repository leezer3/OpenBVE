using System;
using LibRender;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SoundManager;
using static LibRender.CameraProperties;

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
			/// <summary>The sound buffer array</summary>
			internal SoundBuffer[] Buffers;
			/// <summary>Whether a single buffer is used</summary>
			internal bool SingleBuffer;
			/// <summary>The sound source for this file</summary>
			internal SoundSource Source;
			/// <summary>The pitch to play the sound at</summary>
			internal double currentPitch = 1.0;
			/// <summary>The volume to play the sound at it's origin</summary>
			internal double currentVolume = 1.0;

			internal bool PlayOnShow = true;

			internal bool PlayOnHide = true;

			private int lastState;

			public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
			{
				if (Visible | ForceUpdate)
				{
					if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
						Object.SecondsSinceLastUpdate = 0.0;
						Object.Update(false, NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
						if (this.Object.CurrentState != this.lastState && Loading.SimulationSetup)
						{
							if (this.SingleBuffer && this.Buffers[0] != null)
							{
								switch (this.Object.CurrentState)
								{
									case -1:
										if (this.PlayOnHide)
										{
											Source = Program.Sounds.PlaySound(Buffers[0], currentPitch, currentVolume, Position, false);
										}
										break;
									case 0:
										if (this.PlayOnShow || this.lastState != -1)
										{
											Source = Program.Sounds.PlaySound(Buffers[0], currentPitch, currentVolume, Position, false);
										}
										break;
									default:
										Source = Program.Sounds.PlaySound(Buffers[0], currentPitch, currentVolume, Position, false);
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
												Source = Program.Sounds.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, false);
											}
											break;
										case 1:
											if (this.PlayOnShow || this.lastState != -1)
											{
												Source = Program.Sounds.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, false);
											}
											break;
										default:
											Source = Program.Sounds.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, false);
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
					if (!base.Visible)
					{
						Renderer.ShowObject(Object.internalObject, ObjectType.Dynamic);
						base.Visible = true;
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
					if (base.Visible)
					{
						Renderer.HideObject(ref Object.internalObject);
						base.Visible = false;
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
				CreateDynamicObject(ref o.internalObject);
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
						currentObject.Object.States[i].Object = new StaticObject(Program.CurrentHost) { RendererIndex =  -1 };
					}
				}
				double r = 0.0;
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
					{
						currentObject.Object.States[i].Object.Mesh.Materials[j].Color *= Brightness;
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
