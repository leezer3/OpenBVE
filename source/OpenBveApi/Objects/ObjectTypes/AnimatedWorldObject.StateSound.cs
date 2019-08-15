using System;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an animated object which plays a sound upon state change</summary>
	public class AnimatedWorldObjectStateSound : WorldObject
	{
		/// <summary>Holds a reference to the host application</summary>
		private Hosts.HostInterface currentHost;
		/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
		public int SectionIndex;
		/// <summary>The sound buffer array</summary>
		public SoundHandle[] Buffers;
		/// <summary>Whether a single buffer is used</summary>
		public bool SingleBuffer;
		/// <summary>The sound source for this file</summary>
		public object Source;
		/// <summary>The pitch to play the sound at</summary>
		public double currentPitch = 1.0;
		/// <summary>The volume to play the sound at it's origin</summary>
		public double currentVolume = 1.0;
		/// <summary>Whether the sound should be played on showing a new state</summary>
		public bool PlayOnShow = true;
		/// <summary>Whether the sound should be played on hiding a new state</summary>
		public bool PlayOnHide = true;

		private int lastState;

		/// <summary>Creates a new AnimatedWorldObjectStateSound</summary>
		public AnimatedWorldObjectStateSound(Hosts.HostInterface Host)
		{
			currentHost = Host;
		}

		/// <inheritdoc/>
		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
		{
			if (Visible | ForceUpdate)
			{
				if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
				{
					double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
					Object.SecondsSinceLastUpdate = 0.0;
					Object.Update(false, NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
					if (this.Object.CurrentState != this.lastState && currentHost.SimulationSetup)
					{
						if (this.SingleBuffer && this.Buffers[0] != null)
						{
							switch (this.Object.CurrentState)
							{
								case -1:
									if (this.PlayOnHide)
									{
										Source = currentHost.PlaySound(Buffers[0], currentPitch, currentVolume, Position, null, false);
									}

									break;
								case 0:
									if (this.PlayOnShow || this.lastState != -1)
									{
										Source = currentHost.PlaySound(Buffers[0], currentPitch, currentVolume, Position, null, false);
									}

									break;
								default:
									Source = currentHost.PlaySound(Buffers[0], currentPitch, currentVolume, Position, null, false);
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
											Source = currentHost.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, null, false);
										}

										break;
									case 1:
										if (this.PlayOnShow || this.lastState != -1)
										{
											Source = currentHost.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, null, false);
										}

										break;
									default:
										Source = currentHost.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, null, false);
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
					currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
					base.Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += TimeElapsed;
				if (base.Visible)
				{
					currentHost.HideObject(ref Object.internalObject);
					base.Visible = false;
				}
			}

			this.lastState = this.Object.CurrentState;
		}

		public void Create(ref WorldObject[] AnimatedWorldObjects, ref int AnimatedWorldObjectsUsed, Vector3 objectPosition, Transformation BaseTransformation, Transformation AuxTransformation, int objectSectionIndex, double objectTrackPosition, double Brightness)
		{
			int a = AnimatedWorldObjectsUsed;
			if (a >= AnimatedWorldObjects.Length)
			{
				Array.Resize<WorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
			}

			Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);

			var o = this.Object.Clone();
			currentHost.CreateDynamicObject(ref o.internalObject);
			AnimatedWorldObjectStateSound currentObject = new AnimatedWorldObjectStateSound(currentHost)
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
					currentObject.Object.States[i].Object = new StaticObject(currentHost) {RendererIndex = -1};
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

			currentObject.Radius = System.Math.Sqrt(r);
			currentObject.Visible = false;
			currentObject.Object.Initialize(0, false, false);
			AnimatedWorldObjects[a] = currentObject;
			AnimatedWorldObjectsUsed++;
		}
	}
}
