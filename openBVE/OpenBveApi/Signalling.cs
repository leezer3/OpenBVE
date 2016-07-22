namespace OpenBveApi.Signalling
{
	/// <summary>Defines a signalling section</summary>
	public class Section
	{
		/*
		 * Basic data
		 * 
		 */

		/// <summary>The index of the previous section</summary>
		public int PreviousSection;
		/// <summary>The index of the next section</summary>
		public int NextSection;
		/// <summary>The indicies of this signal</summary>
		public int[] SignalIndices;
		/// <summary>The available aspects for this section</summary>
		public SignalAspect[] Aspects;
		/// <summary>The type of section</summary>
		public SectionType Type;
		/// <summary>The track position in meters at which this signal is placed</summary>
		public double TrackPosition;
		/// <summary>A public read-only variable, which returns the current aspect to external scripts</summary>
		public int currentAspect { get { return CurrentAspect; } }
		internal int CurrentAspect;
		/*
		 * Control data
		 * 
		 */
		/// <summary>Whether this section is invisible</summary>
		public bool Invisible;
		/// <summary>Whether the train has reached the station stop point (Used for departure controlled signals)</summary>
		public bool TrainReachedStopPoint;
		/// <summary>The index of the station which the signal is attached to within the stations array (Used for departure controlled signals)</summary>
		public int StationIndex;
		/// <summary>The number of free sections ahead of this signal</summary>
		public int FreeSections;
		/// <summary>Whether this section contains an approach controlled signal</summary>
		public bool ApproachControlled;
		/// <summary>Whether the approach controlled signal is currently held at red</summary>
		public bool ApproachControlAtRed;


		/* 
		 * Methods
		 */

		/// <summary>Sets the aspect of the section</summary>
		public void SetAspect(int Aspect)
		{
			CurrentAspect = Aspect;
		}
	}

	/// <summary>Defines an induvidual aspect within a section</summary>
	public struct SignalAspect
	{
		/// <summary>The aspect number presented for animations</summary>
		public int Number;
		/// <summary>The speed limit associated with this signal aspect (If applicable)</summary>
		public double Speed;
		/// <summary>Creates a new signal aspect with the given aspect number and speed</summary>
		public SignalAspect(int Number, double Speed)
		{
			this.Number = Number;
			this.Speed = Speed;
		}
	}

	/// <summary>The available types of section</summary>
	public enum SectionType
	{
		/// <summary>The section indicies are a series of arbritary ascending values (e.g 0,7,13...)</summary>
		ValueBased,
		/// <summary>The section indicies use a simple zero-based index (0,1,2,3....)</summary>
		IndexBased
	}
}
