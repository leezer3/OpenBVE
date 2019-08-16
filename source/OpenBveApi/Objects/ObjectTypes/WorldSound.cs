using System;
using System.Runtime.Remoting.Services;
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
		/// <summary>Stores a reference to the current host</summary>
		private readonly Hosts.HostInterface currentHost;

		private Track[] Tracks;
		/// <summary>The sound buffer to play</summary>
		public SoundHandle Buffer;
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

		public WorldSound(Hosts.HostInterface Host)
		{
			Radius = 25.0;
			currentHost = Host;
		}

		public void CreateSound(Track[] Tracks, Vector3 position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double trackPosition)
		{
			int a = currentHost.AnimatedWorldObjectsUsed;
			
			WorldSound snd = new WorldSound(currentHost)
			{
				Buffer = this.Buffer,
				//Must clone the vector, not pass the reference
				Position = new Vector3(position),
				Follower = new TrackFollower(Tracks),
				currentTrackPosition = trackPosition
			};
			snd.Follower.UpdateAbsolute(trackPosition, true, true);
			if (this.TrackFollowerFunction != null)
			{
				snd.TrackFollowerFunction = this.TrackFollowerFunction.Clone();
			}

			currentHost.AnimatedWorldObjects[a] = snd;
			currentHost.AnimatedWorldObjectsUsed++;
		}

		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
		{
			if (Visible | ForceUpdate)
			{
				if (TimeElapsed > 0.05)
				{
					return;
				}

				if (this.TrackFollowerFunction != null)
				{

					double delta = this.TrackFollowerFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					this.Follower.UpdateRelative(this.currentTrackPosition + delta, true, true);
					this.Follower.UpdateWorldCoordinates(false);
				}

				if (this.VolumeFunction != null)
				{
					this.currentVolume = this.VolumeFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
				}

				if (this.PitchFunction != null)
				{
					this.currentPitch = this.PitchFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
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
	}
}
