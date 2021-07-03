using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an animated object which plays a sound upon state change</summary>
	public class AnimatedWorldObjectStateSound : WorldObject
	{
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
		public AnimatedWorldObjectStateSound(Hosts.HostInterface Host) : base(Host)
		{
		}

		/// <inheritdoc/>
		public override WorldObject Clone()
		{
			AnimatedWorldObjectStateSound awoss = (AnimatedWorldObjectStateSound)base.Clone();
			awoss.Source = null;
			return awoss;
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
					if (this.Object.CurrentState != this.lastState && currentHost.SimulationState != SimulationState.Loading)
					{
						if (SingleBuffer)
						{
							if (Buffers[0] != null)
							{
								bool isToBePlayed = false;

								if (Object.CurrentState == -1)
								{
									if (PlayOnHide)
									{
										isToBePlayed = true;
									}
								}
								else
								{
									if (PlayOnShow || lastState != -1)
									{
										isToBePlayed = true;
									}
								}

								if (isToBePlayed)
								{
									Source = currentHost.PlaySound(Buffers[0], currentPitch, currentVolume, Position, null, false);
								}
							}
						}
						else
						{
							int bufferIndex = Object.CurrentState;

							if (bufferIndex >= 0 && bufferIndex < Buffers.Length && Buffers[bufferIndex] != null)
							{
								Source = currentHost.PlaySound(Buffers[bufferIndex], currentPitch, currentVolume, Position, null, false);
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

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 CameraPosition, double BackgroundImageDistance, double ExtraViewingDistance)
		{
			double z = 0;
			if (Object != null && Object.TranslateZFunction != null)
			{
				z += Object.TranslateZFunction.LastResult;
			}
			double pa = TrackPosition + z - Radius - 10.0;
			double pb = TrackPosition + z + Radius + 10.0;
			double ta = CameraPosition.Z - BackgroundImageDistance - ExtraViewingDistance;
			double tb = CameraPosition.Z + BackgroundImageDistance + ExtraViewingDistance;
			return pb >= ta & pa <= tb;
		}

		/// <summary>Creates the animated object within the game world</summary>
		/// <param name="WorldPosition">The absolute position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="LocalTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="FinalSectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="FinalTrackPosition">The absolute track position</param>
		/// <param name="Brightness">The brightness value at the track position</param>
		public void Create(Vector3 WorldPosition, Transformation WorldTransformation, Transformation LocalTransformation, int FinalSectionIndex, double FinalTrackPosition, double Brightness)
		{
			int a = currentHost.AnimatedWorldObjectsUsed;
			Transformation FinalTransformation = new Transformation(LocalTransformation, WorldTransformation);

			AnimatedWorldObjectStateSound currentObject = (AnimatedWorldObjectStateSound)Clone();
			currentObject.Position = WorldPosition;
			currentObject.Direction = FinalTransformation.Z;
			currentObject.Up = FinalTransformation.Y;
			currentObject.Side = FinalTransformation.X;
			currentObject.SectionIndex = FinalSectionIndex;
			currentObject.TrackPosition = FinalTrackPosition;
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
			currentObject.Object.Initialize(0, ObjectType.Dynamic, false);
			currentHost.AnimatedWorldObjects[a] = currentObject;
			currentHost.AnimatedWorldObjectsUsed++;
		}
	}
}
