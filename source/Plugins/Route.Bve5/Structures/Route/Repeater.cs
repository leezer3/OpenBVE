namespace Bve5RouteParser
{
	internal struct Repeater
	{
		/// <summary>The name-key that this is referred to by in the routefile</summary>
		internal string Name;

		internal int Type;
		/// <summary>The objects to use</summary>
		internal int[] StructureTypes;
		/// <summary>The length of the object</summary>
		internal double Span;
		/// <summary>The distance at which the object should be repeated</summary>
		internal double RepetitionInterval;
		/// <summary>The distance at which the object was last placed</summary>
		internal double TrackPosition;

		internal int RailIndex;

		internal double X;
		internal double Y;
		internal double Z;
		internal double Yaw;
		internal double Pitch;
		internal double Roll;
	}
}
