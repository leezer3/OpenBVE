using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an animated object which plays a sound upon state change</summary>
	public class AnimatedWorldObjectStateSound : WorldObject
	{
		/// <summary>The sound buffer array</summary>
		public SoundHandle[] Buffers;
		/// <summary>Whether a single buffer is used</summary>
		public bool SingleBuffer;
		/// <summary>The sound source for this file</summary>
		public object Source;
		/// <summary>The pitch to play the sound at</summary>
		public double CurrentPitch = 1.0;
		/// <summary>The volume to play the sound at it's origin</summary>
		public double CurrentVolume = 1.0;
		/// <summary>Whether the sound should be played on showing a new state</summary>
		public bool PlayOnShow = true;
		/// <summary>Whether the sound should be played on hiding a new state</summary>
		public bool PlayOnHide = true;
		/*
		 * Note: The position vector will always be zero
		 *       We cannot however use this to store our sound position, as it's
		 *       required to position the *object* by the renderer (even though this is already transformed)
		 */
		/// <summary>The relative position of the sound</summary>
		public Vector3 SoundPosition;

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
		public override void Update(AbstractTrain nearestTrain, double timeElapsed, bool forceUpdate, bool currentlyVisible)
		{
			if (currentlyVisible | forceUpdate)
			{
				if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | forceUpdate)
				{
					double timeDelta = Object.SecondsSinceLastUpdate + timeElapsed;
					Object.SecondsSinceLastUpdate = 0.0;
					Object.Update(nearestTrain, nearestTrain?.DriverCar ?? 0, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
					if (Object.CurrentState != lastState && currentHost.SimulationState != SimulationState.Loading)
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
									Source = currentHost.PlaySound(Buffers[0], CurrentPitch, CurrentVolume, Position + SoundPosition, null, false);
								}
							}
						}
						else
						{
							int bufferIndex = Object.CurrentState;

							if (bufferIndex >= 0 && bufferIndex < Buffers.Length && Buffers[bufferIndex] != null)
							{
								Source = currentHost.PlaySound(Buffers[bufferIndex], CurrentPitch, CurrentVolume, Position + SoundPosition, null, false);
							}
						}
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += timeElapsed;
				}

				if (!Visible)
				{
					if (Object.CurrentState != -1)
					{
						currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
					}
					Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += timeElapsed;
				if (Visible)
				{
					currentHost.HideObject(Object.internalObject);
					Visible = false;
				}
			}

			lastState = Object.CurrentState;
		}

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 cameraPosition, double backgroundImageDistance, double extraViewingDistance)
		{
			double z = 0;
			if (Object != null && Object.TranslateZFunction != null)
			{
				z += Object.TranslateZFunction.LastResult;
			}
			double pa = TrackPosition + z - Radius - 10.0;
			double pb = TrackPosition + z + Radius + 10.0;
			double ta = cameraPosition.Z - backgroundImageDistance - extraViewingDistance;
			double tb = cameraPosition.Z + backgroundImageDistance + extraViewingDistance;
			return pb >= ta & pa <= tb;
		}

		/// <summary>Creates the animated object within the game world</summary>
		/// <param name="worldPosition">The absolute position</param>
		/// <param name="worldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="localTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="finalSectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="finalTrackPosition">The absolute track position</param>
		public void Create(Vector3 worldPosition, Transformation worldTransformation, Transformation localTransformation, int finalSectionIndex, double finalTrackPosition)
		{
			int a = currentHost.AnimatedWorldObjectsUsed;
			Transformation finalTransformation = new Transformation(localTransformation, worldTransformation);

			AnimatedWorldObjectStateSound currentObject = (AnimatedWorldObjectStateSound)Clone();
			currentObject.Position = worldPosition;
			currentObject.Direction = finalTransformation.Z;
			currentObject.Up = finalTransformation.Y;
			currentObject.Side = finalTransformation.X;
			currentObject.Object.SectionIndex = finalSectionIndex;
			currentObject.TrackPosition = finalTrackPosition;
			for (int i = 0; i < currentObject.Object.States.Length; i++)
			{
				if (currentObject.Object.States[i].Prototype == null)
				{
					currentObject.Object.States[i].Prototype = new StaticObject(currentHost);
				}
			}


			double r = 0.0;
			for (int i = 0; i < currentObject.Object.States.Length; i++)
			{
				for (int j = 0; j < currentObject.Object.States[i].Prototype.Mesh.Vertices.Length; j++)
				{
					double t = Object.States[i].Prototype.Mesh.Vertices[j].Coordinates.NormSquared();
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
