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
		private readonly Hosts.HostInterface currentHost;
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
		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool CurrentlyVisible)
		{
			if (CurrentlyVisible | ForceUpdate)
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
					currentHost.HideObject(Object.internalObject);
					base.Visible = false;
				}
			}

			this.lastState = this.Object.CurrentState;
		}

		/// <summary>Creates the animated object within the game world</summary>
		/// <param name="Position">The absolute position</param>
		/// <param name="BaseTransformation">The base transformation (Rail 0)</param>
		/// <param name="AuxTransformation">The auxilary transformation (Placed rail)</param>
		/// <param name="SectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="TrackPosition">The absolute track position</param>
		/// <param name="Brightness">The brightness value at the track position</param>
		public void Create(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness)
		{
			int a = currentHost.AnimatedWorldObjectsUsed;
			Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);

			var o = this.Object.Clone();
			currentHost.CreateDynamicObject(ref o.internalObject);
			AnimatedWorldObjectStateSound currentObject = new AnimatedWorldObjectStateSound(currentHost)
			{
				Position = Position,
				Direction = FinalTransformation.Z,
				Up = FinalTransformation.Y,
				Side = FinalTransformation.X,
				Object = o,
				SectionIndex = SectionIndex,
				TrackPosition = TrackPosition,
				Buffers = Buffers,
				SingleBuffer = SingleBuffer,
				PlayOnShow = PlayOnShow,
				PlayOnHide = PlayOnHide
			};
			for (int i = 0; i < currentObject.Object.States.Length; i++)
			{
				if (currentObject.Object.States[i].Prototype == null)
				{
					currentObject.Object.States[i].Prototype = new StaticObject(currentHost);
				}
			}

			currentObject.Object.internalObject.Brightness = Brightness;

			double r = 0.0;
			for (int i = 0; i < currentObject.Object.States.Length; i++)
			{
				for (int j = 0; j < currentObject.Object.States[i].Prototype.Mesh.Vertices.Length; j++)
				{
					double t = this.Object.States[i].Prototype.Mesh.Vertices[j].Coordinates.NormSquared();
					if (t > r) r = t;
				}
			}

			currentObject.Radius = System.Math.Sqrt(r);
			currentObject.Visible = false;
			currentObject.Object.Initialize(0, false, false);
			currentHost.AnimatedWorldObjects[a] = currentObject;
			currentHost.AnimatedWorldObjectsUsed++;
		}
	}
}
