using System.Collections.Generic;
using OpenBveApi.Routes;
using RouteManager2.Climate;
namespace CsvRwRouteParser
{
	/// <summary>A single parsed block of data from a routefile</summary>
	/// <remarks>Default block length is 25m</remarks>
	internal class Block
	{
		internal int Background;
		internal Brightness[] BrightnessChanges;
		internal Fog Fog;
		internal bool FogDefined;
		internal int[] Cycle;
		internal RailCycle[] RailCycles;
		internal double Height;
		internal Dictionary<int, Rail> Rails;
		internal Switch[] Switches;
		internal int[] RailType;
		internal Dictionary<int, WallDike> RailWall;
		internal Dictionary<int, WallDike> RailDike;
		internal Pole[] RailPole;
		internal Dictionary<int,List<FreeObj>> RailFreeObj;
		internal List<FreeObj> GroundFreeObj;
		internal Form[] Forms;
		internal Crack[] Cracks;
		internal Signal[] Signals;
		internal Section[] Sections;
		internal Limit[] Limits;
		internal Stop[] StopPositions;
		internal Sound[] SoundEvents;
		internal Transponder[] Transponders;
		internal DestinationEvent[] DestinationChanges;
		internal PointOfInterest[] PointsOfInterest;
		internal HornBlowEvent[] HornBlows;
		internal TrackElement CurrentTrackState;
		internal double Pitch;
		internal double Turn;
		internal int Station;
		internal bool StationPassAlarm;
		internal int SnowIntensity;
		internal int RainIntensity;
		internal int WeatherObject;
		internal LightDefinition LightDefinition;
		internal int DynamicLightDefinition;
		internal LightingChange[] LightingChanges;
		internal Dictionary<int, PatternObj> PatternObjs;

		internal Block(bool PreviewOnly)
		{
			Rails = new Dictionary<int, Rail>();
			Limits = new Limit[] { };
			StopPositions = new Stop[] { };
			Station = -1;
			StationPassAlarm = false;
			Switches = new Switch[] { };
			if (!PreviewOnly)
			{
				BrightnessChanges = new Brightness[] { };
				Forms = new Form[] { };
				Cracks = new Crack[] { };
				Signals = new Signal[] { };
				Sections = new Section[] { };
				SoundEvents = new Sound[] { };
				Transponders = new Transponder[] { };
				DestinationChanges = new DestinationEvent[] { };
				HornBlows = new HornBlowEvent[] { };
				RailFreeObj = new Dictionary<int, List<FreeObj>>();
				GroundFreeObj = new List<FreeObj>();
				PointsOfInterest = new PointOfInterest[] { };
				DynamicLightDefinition = -1;
				LightDefinition = new LightDefinition();
				LightingChanges = new LightingChange[] { };
				PatternObjs = new Dictionary<int, PatternObj>();
			}
		}
	}
}
