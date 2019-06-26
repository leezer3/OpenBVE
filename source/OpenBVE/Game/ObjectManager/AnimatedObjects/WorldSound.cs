using System;
using LibRender;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SoundManager;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Represents a world sound attached to an .animated file</summary>
		internal class WorldSound : WorldObject
		{
			/// <summary>The sound buffer to play</summary>
			internal SoundsBase.SoundBuffer Buffer;
			/// <summary>The sound source for this file</summary>
			internal SoundsBase.SoundSource Source;
			/// <summary>The pitch to play the sound at</summary>
			internal double currentPitch = 1.0;
			/// <summary>The volume to play the sound at it's origin</summary>
			internal double currentVolume = 1.0;
			/// <summary>The track position</summary>
			internal double currentTrackPosition = 0;
			/// <summary>The track follower used to hold/ move the sound</summary>
			internal TrackManager.TrackFollower Follower;
			/// <summary>The function script controlling the sound's movement along the track, or a null reference</summary>
			internal FunctionScript TrackFollowerFunction;
			/// <summary>The function script controlling the sound's volume, or a null reference</summary>
			internal FunctionScript VolumeFunction;
			/// <summary>The function script controlling the sound's pitch, or a null reference</summary>
			internal FunctionScript PitchFunction;

			internal void CreateSound(Vector3 position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double trackPosition)
			{
				int a = AnimatedWorldObjectsUsed;
				if (a >= AnimatedWorldObjects.Length)
				{
					Array.Resize(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
				}
				WorldSound snd = new WorldSound
				{
					Buffer = Buffer,
					//Must clone the vector, not pass the reference
					Position = new Vector3(position),
					Follower = new TrackManager.TrackFollower(),
					currentTrackPosition = trackPosition
				};
				snd.Follower.Update(trackPosition, true, true);
				if (TrackFollowerFunction != null)
				{
					snd.TrackFollowerFunction = TrackFollowerFunction.Clone();
				}
				AnimatedWorldObjects[a] = snd;
				AnimatedWorldObjectsUsed++;
			}

			public override void Update(double TimeElapsed, bool ForceUpdate)
			{
				const double extraRadius = 10.0;
				const double Radius = 25.0;

				double pa = currentTrackPosition + Radius - extraRadius;
				double pb = currentTrackPosition + Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + Camera.CurrentAlignment.Position.Z - Backgrounds.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + Camera.CurrentAlignment.Position.Z + Backgrounds.BackgroundImageDistance + World.ExtraViewingDistance;
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
							if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < Follower.TrackPosition)
							{
								distance = Follower.TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
							}
							else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > Follower.TrackPosition)
							{
								distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - Follower.TrackPosition;
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
					if (TrackFollowerFunction != null)
					{

						double delta = TrackFollowerFunction.Perform(train, train == null ? 0 : train.DriverCar, Position, Follower.TrackPosition, 0, false, TimeElapsed, 0);
						Follower.Update(currentTrackPosition + delta, true, true);
						Follower.UpdateWorldCoordinates(false);
					}
					if (VolumeFunction != null)
					{
						currentVolume = VolumeFunction.Perform(train, train == null ? 0 : train.DriverCar, Position, Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (PitchFunction != null)
					{
						currentPitch = PitchFunction.Perform(train, train == null ? 0 : train.DriverCar, Position, Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (Source != null)
					{
						Source.Pitch = currentPitch;
						Source.Volume = currentVolume;
					}
					//Buffer should never be null, but check it anyways
					if (!Program.Sounds.IsPlaying(Source) && Buffer != null)
					{
						Source = Program.Sounds.PlaySound(Buffer, 1.0, 1.0, Follower.WorldPosition + Position, this, true);
					}
				}
				else
				{
					if (Program.Sounds.IsPlaying(Source))
					{
						Program.Sounds.StopSound(Source);
					}
				}

			}
		}
	}
}
