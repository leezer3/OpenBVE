using System;
using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when a generic sound should be played</summary>
		internal class SoundEvent : GeneralEvent
		{
			/// <summary>The sound buffer to play</summary>
			private readonly Sounds.SoundBuffer SoundBuffer;
			private readonly bool PlayerTrainOnly;
			private readonly bool Once;
			private readonly bool Dynamic;
			internal Vector3 Position;
			private readonly double Speed;

			/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
			/// <param name="SoundBuffer">The sound buffer to play. 
			/// HACK: Set to a null reference to indicate the train point sound.</param>
			/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
			/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
			/// <param name="Dynamic">Whether this sound is dynamic (Attached to a train)</param>
			/// <param name="Position">The position of the sound relative to it's track location</param>
			/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
			internal SoundEvent(double TrackPositionDelta, Sounds.SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed)
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
			/// <param name="CarIndex">The car index which triggered this sound</param>
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (SuppressSoundEvents) return;
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle | TriggerType == EventTriggerType.OtherCarRearAxle | TriggerType == EventTriggerType.RearCarRearAxle)
				{
					if (!PlayerTrainOnly | Train == TrainManager.PlayerTrain)
					{
						Vector3 p = this.Position;
						double pitch = 1.0;
						double gain = 1.0;
						Sounds.SoundBuffer buffer = this.SoundBuffer;
						if (buffer != null)
						{
							if (this.Dynamic)
							{
								double spd = Math.Abs(Train.Specs.CurrentAverageSpeed);
								pitch = spd / this.Speed;
								gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
								if (pitch < 0.2 | gain < 0.2)
								{
									buffer = null;
								}
							}
							if (buffer != null)
							{
								Sounds.PlaySound(buffer, pitch, gain, p, Train, CarIndex, false);
							}
						}
						this.DontTriggerAnymore = this.Once;
					}
				}
			}
		}

		internal class PointSoundEvent : GeneralEvent
		{
			private readonly double Speed;

			/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
			internal PointSoundEvent(double Speed)
			{
				this.DontTriggerAnymore = false;
				this.Speed = Speed;
			}

			/// <summary>Triggers the playback of a sound</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="CarIndex">The car index which triggered this sound</param>
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (SuppressSoundEvents) return;
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle | TriggerType == EventTriggerType.OtherCarRearAxle | TriggerType == EventTriggerType.RearCarRearAxle)
				{
					Vector3 p;
					Sounds.SoundBuffer buffer;
					if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
					{
						if (Train.Specs.CurrentAverageSpeed <= 0.0) return;
						int bufferIndex = Train.Cars[CarIndex].Sounds.FrontAxleRunIndex;
						if (Train.Cars[CarIndex].Sounds.PointFrontAxle == null || Train.Cars[CarIndex].Sounds.PointFrontAxle.Length == 0)
						{
							//No point sounds defined at all
							return;
						}
						if (bufferIndex > Train.Cars[CarIndex].Sounds.PointFrontAxle.Length - 1
						    || Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Buffer == null)
						{
							//If the switch sound does not exist, return zero
							//Required to handle legacy trains which don't have idx specific run sounds defined
							bufferIndex = 0;
						}
						buffer = Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Buffer;
						p = Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Position;
					}
					else
					{
						return; // HACK: Don't trigger sound for the rear axles
						//buffer = Train.Cars[CarIndex].Sounds.PointRearAxle.Buffer;
						//p = Train.Cars[CarIndex].Sounds.PointRearAxle.Position;
					}
					if (buffer != null)
					{
						double spd = Math.Abs(Train.Specs.CurrentAverageSpeed);
						double pitch = spd / this.Speed;
						double gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
						if (pitch < 0.2 | gain < 0.2)
						{
							buffer = null;
						}
						if (buffer != null)
						{
							Sounds.PlaySound(buffer, pitch, gain, p, Train, CarIndex, false);
						}
					}
					this.DontTriggerAnymore = false;
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
			/// <param name="CarIndex">The car index which triggered this sound</param>
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						Train.Cars[CarIndex].Sounds.FrontAxleRunIndex = this.PreviousRunIndex;
						Train.Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						Train.Cars[CarIndex].Sounds.FrontAxleRunIndex = this.NextRunIndex;
						Train.Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.NextFlangeIndex;
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle | TriggerType == EventTriggerType.OtherCarRearAxle)
				{
					if (Direction < 0)
					{
						Train.Cars[CarIndex].Sounds.RearAxleRunIndex = this.PreviousRunIndex;
						Train.Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						Train.Cars[CarIndex].Sounds.RearAxleRunIndex = this.NextRunIndex;
						Train.Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.NextFlangeIndex;
					}
				}
			}
		}
	}
}
