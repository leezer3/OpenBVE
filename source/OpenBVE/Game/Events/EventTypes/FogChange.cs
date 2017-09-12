namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when the in-game fog should be changed</summary>
		internal class FogChangeEvent : GeneralEvent
		{
			/// <summary>The fog which applies previously to this point</summary>
			internal Game.Fog PreviousFog;
			/// <summary>The fog which applies after this point</summary>
			internal Game.Fog CurrentFog;
			/// <summary>The next upcoming fog (Used for distance based interpolation)</summary>
			internal Game.Fog NextFog;

			internal FogChangeEvent(double TrackPositionDelta, Game.Fog PreviousFog, Game.Fog CurrentFog, Game.Fog NextFog)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousFog = PreviousFog;
				this.CurrentFog = CurrentFog;
				this.NextFog = NextFog;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.Camera)
				{
					if (Direction < 0)
					{
						Game.PreviousFog = this.PreviousFog;
						Game.NextFog = this.CurrentFog;
					}
					else if (Direction > 0)
					{
						Game.PreviousFog = this.CurrentFog;
						Game.NextFog = this.NextFog;
					}
				}
			}
		}
	}
}
