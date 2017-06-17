using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/*
		 * Contains the base definition of a train car
		 */
		/// <summary>The root class for a train car within the simulation</summary>
		internal struct Car
		{
			/// <summary>The width of the car in m</summary>
			internal double Width;
			/// <summary>The height of the car in m</summary>
			internal double Height;
			/// <summary>The length of the car in m</summary>
			internal double Length;
			/// <summary>The front axle</summary>
			internal Axle FrontAxle;
			/// <summary>The front bogie</summary>
			internal Bogie FrontBogie;
			/// <summary>The rear axle</summary>
			internal Axle RearAxle;
			/// <summary>The rear bogie</summary>
			internal Bogie RearBogie;

			internal Horn[] Horns;

			internal Vector3 Up;
			/// <summary>The car sections (Objects associated with the car)</summary>
			internal CarSection[] CarSections;
			/// <summary>The index of the current car section</summary>
			internal int CurrentCarSection;
			/// <summary>The position of the driver's eyes within the car</summary>
			internal Vector3 DriverPosition;
			/// <summary>The current camera yaw for the driver's eyes</summary>
			internal double DriverYaw;
			/// <summary>The current camera pitch for the driver's eyes</summary>
			internal double DriverPitch;
			/// <summary>The performance specifications associated with the car</summary>
			internal CarSpecs Specs;
			/// <summary>The car sounds</summary>
			internal CarSounds Sounds;
			/// <summary>Whether the car is currently visible</summary>
			internal bool CurrentlyVisible;
			/// <summary>Whether the car is currently derailed</summary>
			internal bool Derailed;
			/// <summary>Whether the car is currently toppled</summary>
			internal bool Topples;
			/// <summary>The current brightness value for the driver's view</summary>
			internal CarBrightness Brightness;
			/// <summary>The position of the beacon reciever in the car</summary>
			internal double BeaconReceiverPosition;
			/// <summary>The beacon reciever track follower</summary>
			internal TrackManager.TrackFollower BeaconReceiver;
		}
	}
}
