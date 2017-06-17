using OpenBveApi.Math;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The root class for a bogie attached to a car</summary>
		internal struct Bogie
		{
#pragma warning disable 0649
			//These are currently deliberately unused
			//TODO: Allow separate physics implementation for bogies, rather than simply glued to the track
			internal double Width;
			internal double Height;
			internal double Length;
#pragma warning restore 0649
			internal Axle FrontAxle;
			internal Axle RearAxle;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			internal Vector3 Up;
			internal CarSection[] CarSections;
			internal int CurrentCarSection;
			internal bool CurrentlyVisible;
		}
	}
}
