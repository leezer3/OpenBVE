using System;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when a generic sound should be played</summary>
		internal class SoundEvent : GeneralEvent
		{
			/// <summary>The sound buffer to play</summary>
			private readonly SoundBuffer SoundBuffer;
			/// <summary>Whether this sound is triggered by the player train only, or all trains</summary>
			private readonly bool PlayerTrainOnly;
			/// <summary>Whether this sound should play once, or repeat if triggered again</summary>
			private readonly bool Once;
			/// <summary>Whether the sound pitch is affected by the speed of the train</summary>
			private readonly bool Dynamic;
			/// <summary>The 3D position of the sound within the world</summary>
			internal Vector3 Position;
			/// <summary>The speed in km/h at which the sound is played at it's original pitch</summary>
			private readonly double Speed;

			/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
			/// <param name="SoundBuffer">The sound buffer to play.</param>
			/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
			/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
			/// <param name="Dynamic">Whether this sound is dynamic (Attached to a train)</param>
			/// <param name="Position">The position of the sound relative to it's track location</param>
			/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
			internal SoundEvent(double TrackPositionDelta, SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.SoundBuffer = SoundBuffer;
				this.PlayerTrainOnly = PlayerTrainOnly;
				this.Once = Once;
				this.Dynamic = Dynamic;
				this.Position = Position;
				this.Speed = Speed;
			}

			/// <summary>Triggers the playback of a sound</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="Car">The car which triggered this sound</param>
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (SuppressSoundEvents) return;
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
								Program.Sounds.PlaySound(buffer, pitch, gain, Position, Car, false);
							}
						}
						this.DontTriggerAnymore = this.Once;
					}
				}
			}
		}

		internal class PointSoundEvent : GeneralEvent
		{
			internal PointSoundEvent()
			{
				this.DontTriggerAnymore = false;
			}

			/// <summary>Triggers the playback of a sound</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="Car">The car which triggered this sound</param>
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (SuppressSoundEvents) return;
				TrainManager.Car c = (TrainManager.Car) Car;
				switch (TriggerType)
				{
					case EventTriggerType.FrontCarFrontAxle:
					case EventTriggerType.OtherCarFrontAxle:
						
						c.FrontAxle.PointSoundTriggered = true;
						DontTriggerAnymore = false;
						break;
					case EventTriggerType.OtherCarRearAxle:
					case EventTriggerType.RearCarRearAxle:
						c.RearAxle.PointSoundTriggered = true;
						DontTriggerAnymore = false;
						break;
				}
			}
		}

		/// <summary>Called when the rail played for a train should be changed</summary>
		internal class RailSoundsChangeEvent : GeneralEvent
		{
			private readonly int PreviousRunIndex;
			private readonly int PreviousFlangeIndex;
			private readonly int NextRunIndex;
			private readonly int NextFlangeIndex;
			internal RailSoundsChangeEvent(double TrackPositionDelta, int PreviousRunIndex, int PreviousFlangeIndex, int NextRunIndex, int NextFlangeIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousRunIndex = PreviousRunIndex;
				this.PreviousFlangeIndex = PreviousFlangeIndex;
				this.NextRunIndex = NextRunIndex;
				this.NextFlangeIndex = NextFlangeIndex;
			}
			/// <summary>Triggers a change in run and flange sounds</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="Car">The car which triggered this sound</param>
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				TrainManager.Car c = (TrainManager.Car) Car;
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						c.FrontAxle.RunIndex = this.PreviousRunIndex;
						c.FrontAxle.FlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						c.FrontAxle.RunIndex = this.NextRunIndex;
						c.FrontAxle.FlangeIndex = this.NextFlangeIndex;
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle | TriggerType == EventTriggerType.OtherCarRearAxle)
				{
					if (Direction < 0)
					{
						c.RearAxle.RunIndex = this.PreviousRunIndex;
						c.RearAxle.FlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						c.RearAxle.RunIndex = this.NextRunIndex;
						c.RearAxle.FlangeIndex = this.NextFlangeIndex;
					}
				}
			}
		}
	}
}
