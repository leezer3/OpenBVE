using System;
using OpenBveApi;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;
using OpenBveShared;
using TrackManager;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Represents a world sound attached to an .animated file</summary>
		internal class WorldSound : WorldObject
		{
			/// <summary>The sound buffer to play</summary>
			internal Sounds.SoundBuffer Buffer;
			/// <summary>The sound source for this file</summary>
			internal Sounds.SoundSource Source;
			/// <summary>The pitch to play the sound at</summary>
			internal double currentPitch = 1.0;
			/// <summary>The volume to play the sound at it's origin</summary>
			internal double currentVolume = 1.0;
			/// <summary>The track position</summary>
			internal double currentTrackPosition = 0;
			/// <summary>The track follower used to hold/ move the sound</summary>
			internal TrackFollower Follower;
			/// <summary>The function script controlling the sound's movement along the track, or a null reference</summary>
			internal FunctionScript TrackFollowerFunction;
			/// <summary>The function script controlling the sound's volume, or a null reference</summary>
			internal FunctionScript VolumeFunction;
			/// <summary>The function script controlling the sound's pitch, or a null reference</summary>
			internal FunctionScript PitchFunction;

			internal void CreateSound(Vector3 position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double trackPosition)
			{
				int a = GameObjectManager.AnimatedWorldObjectsUsed;
				if (a >= GameObjectManager.AnimatedWorldObjects.Length)
				{
					Array.Resize<WorldObject>(ref GameObjectManager.AnimatedWorldObjects, GameObjectManager.AnimatedWorldObjects.Length << 1);
				}
				WorldSound snd = new WorldSound
				{
					Buffer = this.Buffer,
					//Must clone the vector, not pass the reference
					Position = new Vector3(position.X, position.Y, position.Z),
					Follower =  new TrackFollower()
				};
				snd.currentTrackPosition = trackPosition;
				snd.Follower.Update(TrackManager.CurrentTrack, trackPosition, true, true);
				if (this.TrackFollowerFunction != null)
				{
					snd.TrackFollowerFunction = this.TrackFollowerFunction.Clone();
				}

				GameObjectManager.AnimatedWorldObjects[a] = snd;
				GameObjectManager.AnimatedWorldObjectsUsed++;
			}

			public override void Update(double TimeElapsed, bool ForceUpdate)
			{
				const double extraRadius = 10.0;
				const double Radius = 25.0;

				double pa = currentTrackPosition + Radius - extraRadius;
				double pb = currentTrackPosition + Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + Camera.CameraCurrentAlignment.Position.Z - OpenBveShared.Renderer.BackgroundImageDistance - OpenBveShared.World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + Camera.CameraCurrentAlignment.Position.Z + OpenBveShared.Renderer.BackgroundImageDistance + OpenBveShared.World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					if (Game.MinimalisticSimulation || TimeElapsed > 0.05)
					{
						return;
					}
					TrainManager.Train train = null;
					double trainDistance = double.MaxValue;
					for (int j = 0; j < TrainManager.Trains.Length; j++)
					{
						if (TrainManager.Trains[j].State == TrainState.Available)
						{
							double distance;
							if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < this.Follower.TrackPosition)
							{
								distance = this.Follower.TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
							}
							else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > this.Follower.TrackPosition)
							{
								distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - this.Follower.TrackPosition;
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
					if (this.TrackFollowerFunction != null)
					{

						double delta = this.TrackFollowerFunction.Perform(train, train == null ? 0 : train.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
						this.Follower.Update(TrackManager.CurrentTrack, this.currentTrackPosition + delta, true, true);
						this.Follower.UpdateWorldCoordinates(TrackManager.CurrentTrack, false);
					}
					if (this.VolumeFunction != null)
					{
						this.currentVolume = this.VolumeFunction.Perform(train, train == null ? 0 : train.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (this.PitchFunction != null)
					{
						this.currentPitch = this.PitchFunction.Perform(train, train == null ? 0 : train.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (this.Source != null)
					{
						this.Source.Pitch = this.currentPitch;
						this.Source.Volume = this.currentVolume;
					}
					//Buffer should never be null, but check it anyways
					if (!Sounds.IsPlaying(Source) && this.Buffer != null)
					{
						Source = Sounds.PlaySound(Buffer, 1.0, 1.0, Follower.WorldPosition + Position, this, true);
					}
				}
				else
				{
					if (Sounds.IsPlaying(Source))
					{
						Sounds.StopSound(Source);
					}
				}
				
			}
		}
	}
}
