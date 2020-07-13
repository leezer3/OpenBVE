using OpenBveApi.Routes;
using RouteManager2.Climate;

namespace Bve5RouteParser
{
	internal class Block
	{
		internal int Background;
		internal Brightness[] Brightness;
		internal Fog Fog;
		internal bool FogDefined;
		internal int[] Cycle;
		internal double Height;
		internal Repeater[] Repeaters;
		internal Object[][] RailFreeObj;
		internal Object[] GroundFreeObj;
		internal Rail[] Rail;
		internal Crack[] Crack;
		internal int[] RailType;
		internal Signal[] Signal;
		internal Section[] Section;
		internal Limit[] Limit;
		internal Stop[] Stop;
		internal TrackSound[] RunSounds;
		internal TrackSound[] FlangeSounds;
		internal TrackElement CurrentTrackState;
		internal double Pitch;
		internal double Turn;
		internal int Station;
		internal bool StationPassAlarm;
		internal double Accuracy;
		internal double AdhesionMultiplier;
		internal bool JointNoise = false;
		internal bool BeginInterpolation = false;
		internal Sound[] SoundEvents;

		internal Block()
		{
			SoundEvents = new Sound[] { };
		}
	}
}
