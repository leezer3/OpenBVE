using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace RouteManager2.Events
{
	/// <summary>Called when a generic sound should be played</summary>
		public class SoundEvent : GeneralEvent
		{
			/// <summary>The sound buffer to play</summary>
			public readonly SoundBuffer SoundBuffer;
			/// <summary>Whether this sound is triggered by the player train only, or all trains</summary>
			private readonly bool PlayerTrainOnly;
			/// <summary>Whether this sound should play once, or repeat if triggered again</summary>
			private readonly bool Once;
			/// <summary>Whether the sound pitch is affected by the speed of the train</summary>
			private readonly bool Dynamic;
			/// <summary>The 3D position of the sound within the world</summary>
			public Vector3 Position;
			/// <summary>The speed in km/h at which the sound is played at it's original pitch</summary>
			private readonly double Speed;
			/// <summary>Holds a reference to the host application callback function</summary>
			private readonly HostInterface currentHost;

			/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
			/// <param name="SoundBuffer">The sound buffer to play.</param>
			/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
			/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
			/// <param name="Dynamic">Whether this sound is dynamic (Attached to a train)</param>
			/// <param name="Position">The position of the sound relative to it's track location</param>
			/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
			/// <param name="Host">The </param>
			public SoundEvent(double TrackPositionDelta, SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed, HostInterface Host)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.SoundBuffer = SoundBuffer;
				this.PlayerTrainOnly = PlayerTrainOnly;
				this.Once = Once;
				this.Dynamic = Dynamic;
				this.Position = Position;
				this.Speed = Speed;
				this.currentHost = Host;
			}

			/// <summary>Triggers the playback of a sound</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="Car">The car which triggered this sound</param>
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (SoundsBase.SuppressSoundEvents) return;
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle | TriggerType == EventTriggerType.OtherCarRearAxle | TriggerType == EventTriggerType.RearCarRearAxle)
				{
					if (!PlayerTrainOnly | Train.IsPlayerTrain)
					{
						double pitch = 1.0;
						double gain = 1.0;
						SoundBuffer buffer = SoundBuffer;
						if (buffer != null)
						{
							if (this.Dynamic)
							{
								double spd = Math.Abs(Train.CurrentSpeed);
								pitch = spd / this.Speed;
								gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
								if (pitch < 0.2 | gain < 0.2)
								{
									buffer = null;
								}
							}
							if (buffer != null)
							{
								currentHost.PlaySound(buffer, pitch, gain, Position, Car, false);
							}
						}
						this.DontTriggerAnymore = this.Once;
					}
				}
			}
		}
}
