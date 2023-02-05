using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using SoundManager;

namespace RouteManager2.Events
{
	/// <summary>Called when a generic sound should be played</summary>
	public class SoundEvent : GeneralEvent
	{
		/// <summary>Holds a reference to the host application callback function</summary>
		private readonly HostInterface currentHost;
		/// <summary>The sound buffer to play</summary>
		public readonly SoundBuffer SoundBuffer;
		/// <summary>Whether this sound is triggered by the player train only, or all trains</summary>
		private readonly bool PlayerTrainOnly;
		/// <summary>Whether this sound should play once, or repeat if triggered again</summary>
		private readonly bool Once;
		/// <summary>The 3D position of the sound within the world</summary>
		public Vector3 Position;
		/// <summary>The speed in km/h at which the sound is played at it's original pitch</summary>
		private readonly double Speed;
		/// <summary>The sound type</summary>
		private readonly SoundType Type;

		/// <param name="Host">The </param>
		/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
		/// <param name="SoundBuffer">The sound buffer to play.</param>
		/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
		/// <param name="Type">The type of sound</param>
		/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
		/// <param name="Position">The position of the sound relative to it's track location</param>
		/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
		public SoundEvent(HostInterface Host, double TrackPositionDelta, SoundHandle SoundBuffer, bool PlayerTrainOnly, SoundType Type, bool Once, Vector3 Position, double Speed) : base(TrackPositionDelta)
		{
			this.currentHost = Host;
			this.DontTriggerAnymore = false;
			this.SoundBuffer = (SoundBuffer)SoundBuffer;
			this.PlayerTrainOnly = PlayerTrainOnly;
			this.Once = Once;
			this.Position = Position;
			this.Speed = Speed;
			this.Type = Type;
		}

		/// <param name="Host">The </param>
		/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
		/// <param name="SoundBuffer">The sound buffer to play.</param>
		/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
		/// <param name="Type">The type of sound</param>
		/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
		/// <param name="Position">The position of the sound relative to it's track location</param>
		public SoundEvent(HostInterface Host, double TrackPositionDelta, SoundHandle SoundBuffer, bool PlayerTrainOnly, SoundType Type, bool Once, Vector3 Position)
			: this(Host, TrackPositionDelta, SoundBuffer, PlayerTrainOnly, Type, Once, Position, 0.0)
		{
		}

		/// <param name="Host">The </param>
		/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
		/// <param name="SoundBuffer">The sound buffer to play.</param>
		/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
		/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
		/// <param name="Position">The position of the sound relative to it's track location</param>
		public SoundEvent(HostInterface Host, double TrackPositionDelta, SoundHandle SoundBuffer, bool PlayerTrainOnly, bool Once, Vector3 Position)
			: this(Host, TrackPositionDelta, SoundBuffer, PlayerTrainOnly, SoundType.TrainCar, Once, Position)
		{
		}

		/// <summary>Triggers the playback of a sound</summary>
		/// <param name="direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
		/// <param name="trackFollower">The TrackFollower</param>
		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (SoundsBase.SuppressSoundEvents) return;
			EventTriggerType triggerType = trackFollower.TriggerType;
			if (triggerType == EventTriggerType.FrontCarFrontAxle | triggerType == EventTriggerType.OtherCarFrontAxle | triggerType == EventTriggerType.OtherCarRearAxle | triggerType == EventTriggerType.RearCarRearAxle)
			{
				if (Type == SoundType.TrainPlayerTrigger)
				{
					/*
					 * Special case sound!
					 * Minor abuse of the dynamic, but as the API train is passed to plugins and stuff we
					 * don't want to be chucking around what could easily be a mega large sound buffer
					 */
					if (trackFollower.Train.IsPlayerTrain)
					{
						dynamic train = trackFollower.Train;
						train.SafetySystems.QueuedAnnouncement = SoundBuffer;
					}
					return;
				}

				if (!PlayerTrainOnly | trackFollower.Train.IsPlayerTrain)
				{
					if ((Type == SoundType.TrainAllCarDynamic || Type == SoundType.TrainAllCarStatic) && triggerType == EventTriggerType.OtherCarRearAxle)
					{
						/*
						 * For a multi-car announce, we only want the front axles to trigger
						 * However, we also want the rear car, rear axle to run the final processing of DontTriggerAnymore
						 */
						return;
					}
					double pitch = 1.0;
					double gain = 1.0;
					//In order to play for all cars, we need to create a clone of the buffer, as 1 buffer can only be playing in a single location
					SoundBuffer buffer = Type == SoundType.TrainAllCarDynamic || Type == SoundType.TrainAllCarStatic ? SoundBuffer.Clone() : SoundBuffer;
					if (buffer != null)
					{
						if (Type == SoundType.TrainDynamic || Type == SoundType.TrainAllCarDynamic)
						{
							double spd = Math.Abs(trackFollower.Train.CurrentSpeed);
							pitch = spd / this.Speed;
							gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
							if (pitch < 0.2 | gain < 0.2)
							{
								buffer = null;
							}
						}
						if (buffer != null)
						{
							if (Type == SoundType.TrainAllCarDynamic || Type == SoundType.TrainAllCarStatic || triggerType != EventTriggerType.RearCarRearAxle)
							{
								currentHost.PlaySound(buffer, pitch, gain, Position, trackFollower.Car, false);
							}
						}
					}
					if (!(Type == SoundType.TrainAllCarDynamic || Type == SoundType.TrainAllCarStatic) || triggerType == EventTriggerType.RearCarRearAxle)
					{
						this.DontTriggerAnymore = this.Once;
					}
				}
			}
		}
	}
}
