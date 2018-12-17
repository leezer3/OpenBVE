using OpenBveShared;
using OpenBveApi.Trains;

namespace TrackManager
{
	/// <summary>Is called when the in-game fog should be changed</summary>
	public class FogChangeEvent : GeneralEvent
	{
		/// <summary>The fog which applies previously to this point</summary>
		private readonly Fog PreviousFog;
		/// <summary>The fog which applies after this point</summary>
		private readonly Fog CurrentFog;
		/// <summary>The next upcoming fog (Used for distance based interpolation)</summary>
		public Fog NextFog;

		public FogChangeEvent(double TrackPositionDelta, Fog PreviousFog, Fog CurrentFog, Fog NextFog)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			this.DontTriggerAnymore = false;
			this.PreviousFog = PreviousFog;
			this.CurrentFog = CurrentFog;
			this.NextFog = NextFog;
		}
		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain train, int CarIndex)
		{
			if (TriggerType == EventTriggerType.Camera)
			{
				if (Direction < 0)
				{
					Renderer.PreviousFog = this.PreviousFog;
					Renderer.NextFog = this.CurrentFog;
				}
				else if (Direction > 0)
				{
					Renderer.PreviousFog = this.CurrentFog;
					Renderer.NextFog = this.NextFog;
				}
			}
		}
	}
}
