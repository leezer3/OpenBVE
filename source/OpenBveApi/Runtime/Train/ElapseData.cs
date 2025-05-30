using System.Collections.Generic;
using System.Runtime.Serialization;
// ReSharper disable UnusedMember.Global

namespace OpenBveApi.Runtime
{
	/// <summary>Represents data given to the plugin in the Elapse call.</summary>
	[DataContract]
	public class ElapseData
	{
		/// <summary>The state of the train.</summary>
		[DataMember]
		private readonly VehicleState MyVehicle;

		/// <summary>The state of the preceding train, or a null reference if there is no preceding train.</summary>
		[DataMember]
		private readonly PrecedingVehicleState MyPrecedingVehicle;

		/// <summary>The virtual handles.</summary>
		[DataMember]
		private Handles MyHandles;

		/// <summary>The state of the door interlock.</summary>
		[DataMember]
		private DoorInterlockStates MyDoorInterlockState;

		/// <summary>The current absolute time.</summary>
		[DataMember]
		private readonly Time MyTotalTime;

		/// <summary>The elapsed time since the last call to Elapse.</summary>
		[DataMember]
		private readonly Time MyElapsedTime;

		/// <summary>The debug message the plugin wants the host application to display.</summary>
		[DataMember]
		private string MyDebugMessage;

		/// <summary>Whether the plugin requests that time acceleration is disabled.</summary>
		[DataMember]
		private bool MyDisableTimeAcceleration;

		/// <summary>Stores the list of current stations.</summary>
		[DataMember]
		private readonly List<Station> MyStations;

		/// <summary>The current camera view mode.</summary>
		[DataMember]
		private readonly CameraViewMode MyCameraViewMode;

		/// <summary>The current interface language code.</summary>
		[DataMember]
		private readonly string MyLanguageCode;

		/// <summary>The current destination code</summary>
		[DataMember]
		private readonly int CurrentDestination;

		/// <summary>The current headlights state</summary>
		[DataMember] 
		private int MyHeadlightState;

		/// <summary>Whether input from the host application is being blocked</summary>
		[DataMember] 
		private bool MyBlockingInput;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="vehicle">The state of the train.</param>
		/// <param name="precedingVehicle">The state of the preceding train, or a null reference if there is no preceding train.</param>
		/// <param name="handles">The virtual handles.</param>
		/// <param name="doorinterlock">Whether the door interlock is currently enabled</param>
		/// <param name="headlightState">The headlight state</param>
		/// <param name="totalTime">The current absolute time.</param>
		/// <param name="elapsedTime">The elapsed time since the last call to Elapse.</param>
		/// <param name="stations">The current route's list of stations.</param>
		/// <param name="cameraView">The current camera view mode</param>
		/// <param name="languageCode">The current language code</param>
		/// <param name="destination">The current destination</param>
		public ElapseData(VehicleState vehicle, PrecedingVehicleState precedingVehicle, Handles handles, DoorInterlockStates doorinterlock, int headlightState, Time totalTime, Time elapsedTime, List<Station> stations, CameraViewMode cameraView, string languageCode, int destination)
		{
			MyVehicle = vehicle;
			MyPrecedingVehicle = precedingVehicle;
			MyHandles = handles;
			MyDoorInterlockState = doorinterlock;
			MyTotalTime = totalTime;
			MyElapsedTime = elapsedTime;
			MyDebugMessage = null;
			MyStations = stations;
			MyCameraViewMode = cameraView;
			MyLanguageCode = languageCode;
			CurrentDestination = destination;
			MyHeadlightState = headlightState;
			MyBlockingInput = false; // Host application always unblocks input, must be set by plugin each frame to avoid deadlocking
		}


		/// <summary>Gets the state of the train.</summary>
		public VehicleState Vehicle => MyVehicle;

		/// <summary>Gets the state of the preceding train, or a null reference if there is no preceding train.</summary>
		public PrecedingVehicleState PrecedingVehicle => MyPrecedingVehicle;

		/// <summary>Gets or sets the virtual handles.</summary>
		public Handles Handles
		{
			get => MyHandles;
			set => MyHandles = value;
		}

		/// <summary>Gets or sets the state of the door lock.</summary>
		public DoorInterlockStates DoorInterlockState
		{
			get => MyDoorInterlockState;
			set => MyDoorInterlockState = value;
		}

		/// <summary>Gets the absolute in-game time.</summary>
		public Time TotalTime => MyTotalTime;

		/// <summary>Gets the time that elapsed since the last call to Elapse.</summary>
		public Time ElapsedTime => MyElapsedTime;

		/// <summary>Gets or sets the debug message the plugin wants the host application to display.</summary>
		public string DebugMessage
		{
			get => MyDebugMessage;
			set => MyDebugMessage = value;
		}

		/// <summary>Gets or sets the disable time acceleration bool.</summary>
		public bool DisableTimeAcceleration
		{
			get => MyDisableTimeAcceleration;
			set => MyDisableTimeAcceleration = value;
		}

		/// <summary>Returns the list of stations in the current route.</summary>
		public List<Station> Stations => MyStations;

		/// <summary>Gets the current camera view mode.</summary>
		public CameraViewMode CameraViewMode => MyCameraViewMode;

		/// <summary>Gets the current user interface language code.</summary>
		public string CurrentLanguageCode => MyLanguageCode;

		/// <summary>Gets the destination variable as set by the plugin</summary>
		public int Destination => CurrentDestination;

		/// <summary>Gets the headlight state variable as set by the plugin</summary>
		public int HeadlightState
		{
			get => MyHeadlightState;
			set => MyHeadlightState = value;
		}

		/// <summary>Gets the input blocking state as set by the plugin</summary>
		public bool BlockingInput
		{
			get => MyBlockingInput;
			set => MyBlockingInput = value;
		}
	}
}
