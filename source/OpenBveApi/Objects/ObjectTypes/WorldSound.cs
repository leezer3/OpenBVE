using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a world sound attached to an .animated file</summary>
	public class WorldSound : WorldObject
	{
		/// <summary>The sound buffer to play</summary>
		public readonly SoundHandle Buffer;
		/// <summary>The sound source for this file</summary>
		public dynamic Source;
		/// <summary>The pitch to play the sound at</summary>
		public double currentPitch = 1.0;
		/// <summary>The volume to play the sound at it's origin</summary>
		public double currentVolume = 1.0;
		/// <summary>The track position</summary>
		public double currentTrackPosition = 0;
		/// <summary>The track follower used to hold/ move the sound</summary>
		public TrackFollower Follower;
		/// <summary>The function script controlling the sound's movement along the track, or a null reference</summary>
		public FunctionScript TrackFollowerFunction;
		/// <summary>The function script controlling the sound's volume, or a null reference</summary>
		public FunctionScript VolumeFunction;
		/// <summary>The function script controlling the sound's pitch, or a null reference</summary>
		public FunctionScript PitchFunction;

		/// <inheritdoc/>
		/// <remarks>In this case, the position of the track follower is returned.</remarks>
		public override double RelativeTrackPosition => Follower.TrackPosition;

		/// <summary>Creates a new WorldSound</summary>
		public WorldSound(Hosts.HostInterface Host, SoundHandle buffer) : base(Host)
		{
			Radius = 25.0;
			Buffer = buffer;
		}

		/// <inheritdoc/>
		public override WorldObject Clone()
		{
			WorldSound ws = (WorldSound)base.Clone();
			ws.Source = null;
			ws.Follower = Follower?.Clone();
			ws.TrackFollowerFunction = TrackFollowerFunction?.Clone();
			ws.VolumeFunction = VolumeFunction?.Clone();
			ws.PitchFunction = PitchFunction?.Clone();
			return ws;
		}

		/// <summary>Creates the animated object within the game world</summary>
		/// <param name="position">The absolute position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="LocalTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="SectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="trackPosition">The absolute track position</param>
		public void CreateSound(Vector3 position, Transformation WorldTransformation, Transformation LocalTransformation, int SectionIndex, double trackPosition)
		{
			int a = currentHost.AnimatedWorldObjectsUsed;
			WorldSound snd = (WorldSound)Clone();
			snd.Position = position;
			snd.TrackPosition = trackPosition;
			snd.currentTrackPosition = trackPosition;
			snd.Follower = new TrackFollower(currentHost);
			snd.Follower.UpdateAbsolute(trackPosition, true, true);

			currentHost.AnimatedWorldObjects[a] = snd;
			currentHost.AnimatedWorldObjectsUsed++;
		}

		/// <inheritdoc/>
		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool CurrentlyVisible)
		{
			if (CurrentlyVisible | ForceUpdate)
			{
				if (TimeElapsed > 0.05)
				{
					return;
				}

				if (this.TrackFollowerFunction != null)
				{

					double delta = this.TrackFollowerFunction.Perform(NearestTrain, NearestTrain?.DriverCar ?? 0, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					this.Follower.UpdateRelative(this.currentTrackPosition + delta, true, true);
					this.Follower.UpdateWorldCoordinates(false);
				}

				if (this.VolumeFunction != null)
				{
					this.currentVolume = this.VolumeFunction.Perform(NearestTrain, NearestTrain?.DriverCar ?? 0, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
				}

				if (this.PitchFunction != null)
				{
					this.currentPitch = this.PitchFunction.Perform(NearestTrain, NearestTrain?.DriverCar ?? 0, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
				}

				if (this.Source != null)
				{
					this.Source.Pitch = this.currentPitch;
					this.Source.Volume = this.currentVolume;
				}

				//Buffer should never be null, but check it anyways
				if (!currentHost.SoundIsPlaying(Source) && Buffer != null)
				{
					Source = currentHost.PlaySound(Buffer, 1.0, 1.0, Follower.WorldPosition + Position, this, true);
				}
			}
			else
			{
				if (currentHost.SoundIsPlaying(Source))
				{
					currentHost.StopSound(Source);
				}
			}

		}

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 CameraPosition, double BackgroundImageDistance, double ExtraViewingDistance)
		{
			double pa = TrackPosition - Radius - 10.0;
			double pb = TrackPosition + Radius + 10.0;
			double ta = CameraPosition.Z - BackgroundImageDistance - ExtraViewingDistance;
			double tb = CameraPosition.Z + BackgroundImageDistance + ExtraViewingDistance;
			return pb >= ta & pa <= tb;
		}
	}
}
