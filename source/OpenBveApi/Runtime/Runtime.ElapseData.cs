using System.Collections.Generic;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents data given to the plugin in the Elapse call.</summary>
	public class ElapseData
	{
		// --- members ---
		/// <summary>The state of the train.</summary>
		private readonly VehicleState MyVehicle;

		/// <summary>The state of the preceding train, or a null reference if there is no preceding train.</summary>
		private readonly PrecedingVehicleState MyPrecedingVehicle;

		/// <summary>The virtual handles.</summary>
		private Handles MyHandles;

		/// <summary>The state of the door interlock.</summary>
		private DoorInterlockStates MyDoorInterlockState;

		/// <summary>The current absolute time.</summary>
		private readonly Time MyTotalTime;

		/// <summary>The elapsed time since the last call to Elapse.</summary>
		private readonly Time MyElapsedTime;

		/// <summary>The debug message the plugin wants the host application to display.</summary>
		private string MyDebugMessage;

		/// <summary>Whether the plugin requests that time acceleration is disabled.</summary>
		private bool MyDisableTimeAcceleration;

		/// <summary>Stores the list of current stations.</summary>
		private readonly List<Station> MyStations;

		/// <summary>The current camera view mode.</summary>
		private readonly CameraViewMode MyCameraViewMode;

		/// <summary>The current interface language code.</summary>
		private readonly string MyLanguageCode;

		/// <summary>The current destination code</summary>
		private readonly int CurrentDestination;

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="vehicle">The state of the train.</param>
		/// <param name="precedingVehicle">The state of the preceding train, or a null reference if there is no preceding train.</param>
		/// <param name="handles">The virtual handles.</param>
		/// <param name="doorinterlock">Whether the door interlock is currently enabled</param>
		/// <param name="totalTime">The current absolute time.</param>
		/// <param name="elapsedTime">The elapsed time since the last call to Elapse.</param>
		/// <param name="stations">The current route's list of stations.</param>
		/// <param name="cameraView">The current camera view mode</param>
		/// <param name="languageCode">The current language code</param>
		/// <param name="destination">The current destination</param>
		public ElapseData(VehicleState vehicle, PrecedingVehicleState precedingVehicle, Handles handles, DoorInterlockStates doorinterlock, Time totalTime, Time elapsedTime, List<Station> stations, CameraViewMode cameraView, string languageCode, int destination)
		{
			this.MyVehicle = vehicle;
			this.MyPrecedingVehicle = precedingVehicle;
			this.MyHandles = handles;
			this.MyDoorInterlockState = doorinterlock;
			this.MyTotalTime = totalTime;
			this.MyElapsedTime = elapsedTime;
			this.MyDebugMessage = null;
			this.MyStations = stations;
			this.MyCameraViewMode = cameraView;
			this.MyLanguageCode = languageCode;
			this.CurrentDestination = destination;
		}


		// --- properties ---
		/// <summary>Gets the state of the train.</summary>
		public VehicleState Vehicle
		{
			get
			{
				return this.MyVehicle;
			}
		}

		/// <summary>Gets the state of the preceding train, or a null reference if there is no preceding train.</summary>
		public PrecedingVehicleState PrecedingVehicle
		{
			get
			{
				return this.MyPrecedingVehicle;
			}
		}

		/// <summary>Gets or sets the virtual handles.</summary>
		public Handles Handles
		{
			get
			{
				return this.MyHandles;
			}
			set
			{
				this.MyHandles = value;
			}
		}

		/// <summary>Gets or sets the state of the door lock.</summary>
		public DoorInterlockStates DoorInterlockState
		{
			get
			{
				return this.MyDoorInterlockState;
			}
			set
			{
				this.MyDoorInterlockState = value;
			}
		}

		/// <summary>Gets the absolute in-game time.</summary>
		public Time TotalTime
		{
			get
			{
				return this.MyTotalTime;
			}
		}

		/// <summary>Gets the time that elapsed since the last call to Elapse.</summary>
		public Time ElapsedTime
		{
			get
			{
				return this.MyElapsedTime;
			}
		}

		/// <summary>Gets or sets the debug message the plugin wants the host application to display.</summary>
		public string DebugMessage
		{
			get
			{
				return this.MyDebugMessage;
			}
			set
			{
				this.MyDebugMessage = value;
			}
		}

		/// <summary>Gets or sets the disable time acceleration bool.</summary>
		public bool DisableTimeAcceleration
		{
			get
			{
				return this.MyDisableTimeAcceleration;
			}
			set
			{
				this.MyDisableTimeAcceleration = value;
			}
		}

		/// <summary>Returns the list of stations in the current route.</summary>
		public List<Station> Stations
		{
			get
			{
				return this.MyStations;
			}
		}

		/// <summary>Gets the current camera view mode.</summary>
		public CameraViewMode CameraViewMode
		{
			get
			{
				return this.MyCameraViewMode;
			}
		}

		/// <summary>Gets the current user interface language code.</summary>
		public string CurrentLanguageCode
		{
			get
			{
				return this.MyLanguageCode;
			}
		}

		/// <summary>Gets the destination variable as set by the plugin</summary>
		public int Destination
		{
			get
			{
				return this.CurrentDestination;
			}
		}
	}
}
