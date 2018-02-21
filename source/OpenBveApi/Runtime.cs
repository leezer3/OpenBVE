using System;
using System.Collections.Generic;
using OpenBveApi.Colors;

namespace OpenBveApi.Runtime {
	
	/* ----------------------------------------------------
	 * This part of the API is stable as of openBVE 1.2.10.
	 * Any modification must retain backward compatibility.
	 * ---------------------------------------------------- */
	
	// --- load ---
	
	/// <summary>Represents the handle to a sound.</summary>
	public class SoundHandle {
		// --- members ---
		/// <summary>Whether the handle to the sound is valid.</summary>
		protected bool MyValid;
		/// <summary>The volume. A value of 1.0 represents nominal volume.</summary>
		protected double MyVolume;
		/// <summary>The pitch. A value of 1.0 represents nominal pitch.</summary>
		protected double MyPitch;
		// --- properties ---
		/// <summary>Gets whether the sound is still playing. Once this returns false, the sound handle is invalid.</summary>
		public bool Playing {
			get {
				return this.MyValid;
			}
		}
		/// <summary>Gets whether the sound has stopped. Once this returns true, the sound handle is invalid.</summary>
		public bool Stopped {
			get {
				return !this.MyValid;
			}
		}
		/// <summary>Gets or sets the volume. A value of 1.0 represents nominal volume.</summary>
		public double Volume {
			get {
				return this.MyVolume;
			}
			set {
				this.MyVolume = value;
			}
		}
		/// <summary>Gets or sets the pitch. A value of 1.0 represents nominal pitch.</summary>
		public double Pitch {
			get {
				return this.MyPitch;
			}
			set {
				this.MyPitch = value;
			}
		}
		// functions
		/// <summary>Stops the sound and invalidates the handle.</summary>
		public void Stop() {
			this.MyValid = false;
		}
	}

	/// <summary>Plays a sound.</summary>
	/// <param name="index">The index to the sound to be played.</param>
	/// <param name="volume">The initial volume of the sound. A value of 1.0 represents nominal volume.</param>
	/// <param name="pitch">The initial pitch of the sound. A value of 1.0 represents nominal pitch.</param>
	/// <param name="looped">Whether the sound should be played in an indefinate loop.</param>
	/// <returns>The handle to the sound, or a null reference if the sound could not be played.</returns>
	/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
	public delegate SoundHandle PlaySoundDelegate(int index, double volume, double pitch, bool looped);

    /// <summary>Plays a sound.</summary>
    /// <param name="index">The index to the sound to be played.</param>
    /// <param name="volume">The initial volume of the sound. A value of 1.0 represents nominal volume.</param>
    /// <param name="pitch">The initial pitch of the sound. A value of 1.0 represents nominal pitch.</param>
    /// <param name="looped">Whether the sound should be played in an indefinate loop.</param>
    /// <param name="carIndex">The index of the car this sound is to be attached to</param>
    /// <returns>The handle to the sound, or a null reference if the sound could not be played.</returns>
    /// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
    public delegate SoundHandle PlayCarSoundDelegate(int index, double volume, double pitch, bool looped, int carIndex);
	
	/// <summary>Adds a message to the in-game display</summary>
	/// <param name="Message">The message to display</param>
	/// <param name="Color">The color in which to display the message</param>
	/// <param name="Time">The time in seconds for which to display the message</param>
	public delegate void AddInterfaceMessageDelegate(string Message, MessageColor Color, double Time);

	/// <summary>Adds a score to the after game log</summary>
	/// <param name="Score">The score to add or subtract</param>
	/// <param name="Message">The message to be displayed in the post-game log</param>
	/// /// <param name="Color">The color in which to display the message</param>
	/// <param name="Time">The time in seconds for which to display the message</param>
	public delegate void AddScoreDelegate(int Score, string Message, MessageColor Color, double Time);
	
	/// <summary>Represents to which extent the plugin supports the AI.</summary>
	public enum AISupport {
		/// <summary>The plugin does not support the AI. Calls to PerformAI will not be made. Non-player trains will not use the plugin.</summary>
		None = 0,
		/// <summary>The plugin complements the built-in AI by performing only functions specific to the plugin.</summary>
		Basic = 1
	}

	/// <summary>Represents properties supplied to the plugin on loading.</summary>
	public class LoadProperties {
		// --- members ---
		/// <summary>The absolute path to the plugin folder.</summary>
		private readonly string MyPluginFolder;
		/// <summary>The absolute path to the train folder.</summary>
		private readonly string MyTrainFolder;
		/// <summary>The array of panel variables.</summary>
		private int[] MyPanel;
		/// <summary>The callback function for playing sounds.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly PlaySoundDelegate MyPlaySound;
		/// <summary>The callback function for playing car-based  sounds.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly PlayCarSoundDelegate MyPlayCarSound;
		/// <summary>The callback function for adding interface messages.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly AddInterfaceMessageDelegate MyAddInterfaceMessage;
		/// <summary>The callback function for adding or subtracting scores.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly AddScoreDelegate MyAddScore;
		/// <summary>The extent to which the plugin supports the AI.</summary>
		private AISupport MyAISupport;
		/// <summary>The reason why the plugin failed loading.</summary>
		private string MyFailureReason;
		// --- properties ---
		/// <summary>Gets the absolute path to the plugin folder.</summary>
		public string PluginFolder {
			get {
				return this.MyPluginFolder;
			}
		}
		/// <summary>Gets the absolute path to the train folder.</summary>
		public string TrainFolder {
			get {
				return this.MyTrainFolder;
			}
		}
		/// <summary>Gets or sets the array of panel variables.</summary>
		public int[] Panel {
			get {
				return this.MyPanel;
			}
			set {
				this.MyPanel = value;
			}
		}
		/// <summary>Gets the callback function for playing sounds.</summary>
		public PlaySoundDelegate PlaySound {
			get {
				return this.MyPlaySound;
			}
		}
		/// <summary>Gets the callback function for playing sounds.</summary>
		public PlayCarSoundDelegate PlayCarSound
		{
			get
			{
				return this.MyPlayCarSound;
			}
		}
		/// <summary>Gets the callback function for adding interface messages.</summary>
		public AddInterfaceMessageDelegate AddMessage
		{
			get{
				return this.MyAddInterfaceMessage;
				}
		}

		/// <summary>Gets the callback function for adding interface messages.</summary>
		public AddScoreDelegate AddScore
		{
			get
			{
				return this.MyAddScore;
			}
		}

		//public 
		/// <summary>Gets or sets the extent to which the plugin supports the AI.</summary>
		public AISupport AISupport {
			get {
				return this.MyAISupport;
			}
			set {
				this.MyAISupport = value;
			}
		}
		/// <summary>Gets or sets the reason why the plugin failed loading.</summary>
		public string FailureReason {
			get {
				return this.MyFailureReason;
			}
			set {
				this.MyFailureReason = value;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="pluginFolder">The absolute path to the plugin folder.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="playSound">The callback function for playing sounds.</param>
		/// <param name="playCarSound">The callback function for playing car-based sounds.</param>
		/// <param name="addMessage">The callback function for adding interface messages.</param>
		/// <param name="addScore">The callback function for adding scores.</param>
		public LoadProperties(string pluginFolder, string trainFolder, PlaySoundDelegate playSound, PlayCarSoundDelegate playCarSound, AddInterfaceMessageDelegate addMessage, AddScoreDelegate addScore) {
			this.MyPluginFolder = pluginFolder;
			this.MyTrainFolder = trainFolder;
			this.MyPlaySound = playSound;
			this.MyPlayCarSound = playCarSound;
			this.MyAddInterfaceMessage = addMessage;
			this.MyAddScore = addScore;
			this.MyFailureReason = null;
		}
	}
	
	
	// --- set vehicle specs ---
	
	/// <summary>Represents the type of brake the train uses.</summary>
	public enum BrakeTypes {
		/// <summary>The train uses the electromagnetic straight air brake. The numerical value of this constant is 0.</summary>
		ElectromagneticStraightAirBrake = 0,
		/// <summary>The train uses the analog/digital electro-pneumatic air brake without a brake pipe (electric command brake). The numerical value of this constant is 1.</summary>
		ElectricCommandBrake = 1,
		/// <summary>The train uses the automatic air brake with partial release. The numerical value of this constant is 2.</summary>
		AutomaticAirBrake = 2
	}
	
	/// <summary>Represents the specification of the train.</summary>
	public class VehicleSpecs {
		// --- members ---
		/// <summary>The number of power notches the train has.</summary>
		private readonly int MyPowerNotches;
		/// <summary>The type of brake the train uses.</summary>
		private readonly BrakeTypes MyBrakeType;
		/// <summary>Whether the train has a hold brake.</summary>
		private readonly bool MyHasHoldBrake;
		/// <summary>The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		private readonly int MyBrakeNotches;
		/// <summary>The number of cars the train has.</summary>
		private readonly int MyCars;
		// --- properties ---
		/// <summary>Gets the number of power notches the train has.</summary>
		public int PowerNotches {
			get {
				return this.MyPowerNotches;
			}
		}
		/// <summary>Gets the type of brake the train uses.</summary>
		public BrakeTypes BrakeType {
			get {
				return this.MyBrakeType;
			}
		}
		/// <summary>Gets the number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		public int BrakeNotches {
			get {
				return this.MyBrakeNotches;
			}
		}
		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasHoldBrake {
			get {
				return this.MyHasHoldBrake;
			}
		}
		/// <summary>Gets the index of the brake notch that corresponds to B1 or LAP.</summary>
		/// <remarks>For trains without a hold brake, this returns 1. For trains with a hold brake, this returns 2.</remarks>
		public int AtsNotch {
			get
			{
				if (this.MyHasHoldBrake) {
					return 2;
				}
				return 1;
			}
		}
		/// <summary>Gets the index of the brake notch that corresponds to 70% of the available brake notches.</summary>
		public int B67Notch {
			get {
				return (int)System.Math.Round(0.7 * this.MyBrakeNotches);
			}
		}
		/// <summary>Gets the number of cars the train has.</summary>
		public int Cars {
			get {
				return this.MyCars;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, int cars) {
			this.MyPowerNotches = powerNotches;
			this.MyBrakeType = brakeType;
			this.MyBrakeNotches = brakeNotches;
			this.MyHasHoldBrake = hasHoldBrake;
			this.MyCars = cars;
		}
	}
	
	
	// --- initialize ---
	
	/// <summary>Represents the mode in which the plugin should initialize.</summary>
	public enum InitializationModes {
		/// <summary>The safety system should be enabled. The train has its service brakes applied. The numerical value of this constant is -1.</summary>
		OnService = -1,
		/// <summary>The safety system should be enabled. The train has its emergency brakes applied. The numerical value of this constant is 0.</summary>
		OnEmergency = 0,
		/// <summary>The safety system should be disabled. The train has its emergency brakes applied. The numerical value of this constant is 1.</summary>
		OffEmergency = 1
	}

	
	// --- elapse ---

	/// <summary>Represents a speed.</summary>
	public class Speed {
		// --- members ---
		/// <summary>The speed in meters per second.</summary>
		private double MyValue;
		// --- properties ---
		/// <summary>Gets the speed in meters per second.</summary>
		public double MetersPerSecond {
			get {
				return this.MyValue;
			}
		}
		/// <summary>Gets the speed in kilometes per hour.</summary>
		public double KilometersPerHour {
			get {
				return 3.6 * this.MyValue;
			}
		}
		/// <summary>Gets the speed in miles per hour.</summary>
		public double MilesPerHour {
			get {
				return 2.236936 * this.MyValue;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The speed in meters per second.</param>
		public Speed(double value) {
			this.MyValue = value;
		}
	}
	
	/// <summary>Represents a time.</summary>
	public class Time {
		// --- members ---
		/// <summary>The time in seconds.</summary>
		private readonly double MyValue;
		// --- properties ---
		/// <summary>Gets the time in seconds.</summary>
		public double Seconds {
			get {
				return this.MyValue;
			}
		}
		/// <summary>Gets the time in milliseconds.</summary>
		public double Milliseconds {
			get {
				return 1000.0 * this.MyValue;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The time in seconds.</param>
		public Time(double value) {
			this.MyValue = value;
		}
	}

	/// <summary>Represents a station.</summary>
	public class Station
	{
		/// <summary>The name of the station.</summary>
		public string Name;
		/// <summary>The expected arrival time.</summary>
		public double ArrivalTime;
		/// <summary>The expected departure time.</summary>
		public double DepartureTime;
		/// <summary>The expected time stopped.</summary>
		public double StopTime;
		/// <summary>Whether the next signal is held red until departure.</summary>
		public bool ForceStopSignal;
		/// <summary>Whether the left doors are to open.</summary>
		public bool OpenLeftDoors;
		/// <summary>Whether the right doors are to open.</summary>
		public bool OpenRightDoors;
		/// <summary>The track position of this station.</summary>
		public double DefaultTrackPosition;
		/// <summary>The stop position applicable to the current train.</summary>
		public double StopPosition;
	}
	
	/// <summary>Represents the current state of the train.</summary>
	public class VehicleState {
		// --- members ---
		/// <summary>The location of the front of the train, in meters.</summary>
		private readonly double MyLocation;
		/// <summary>The speed of the train.</summary>
		private readonly Speed MySpeed;
		/// <summary>The pressure in the brake cylinder, in pascal.</summary>
		private readonly double MyBcPressure;
		/// <summary>The pressure in the main reservoir, in pascal.</summary>
		private readonly double MyMrPressure;
		/// <summary>The pressure in the emergency reservoir, in pascal.</summary>
		private readonly double MyErPressure;
		/// <summary>The pressure in the brake pipe, in pascal.</summary>
		private readonly double MyBpPressure;
		/// <summary>The pressure in the straight air pipe, in pascal.</summary>
		private readonly double MySapPressure;
		
		private readonly double MyRadius;
		private readonly double MyCant;
		private readonly double MyPitch;
		
		// --- properties ---
		/// <summary>Gets the location of the front of the train, in meters.</summary>
		public double Location {
			get {
				return this.MyLocation;
			}
		}
		/// <summary>Gets the speed of the train.</summary>
		public Speed Speed {
			get {
				return this.MySpeed;
			}
		}
		/// <summary>Gets the pressure in the brake cylinder, in pascal.</summary>
		public double BcPressure {
			get {
				return this.MyBcPressure;
			}
		}
		/// <summary>Gets the pressure in the main reservoir, in pascal.</summary>
		public double MrPressure {
			get {
				return this.MyMrPressure;
			}
		}
		/// <summary>Gets the pressure in the emergency reservoir, in pascal.</summary>
		public double ErPressure {
			get {
				return this.MyErPressure;
			}
		}
		/// <summary>Gets the pressure in the brake pipe, in pascal.</summary>
		public double BpPressure {
			get {
				return this.MyBpPressure;
			}
		}
		/// <summary>Gets the pressure in the straight air pipe, in pascal.</summary>
		public double SapPressure {
			get {
				return this.MySapPressure;
			}
		}
		/// <summary>Gets the curve radius at the front axle of the driver's car in m.</summary>
		public double Radius
		{
			get
			{
				return this.MyRadius;
			}
		}
		/// <summary>Gets the curve cant at the front axle of the driver's car in mm.</summary>
		public double Cant
		{
			get
			{
				return this.MyCant;
			}
		}
		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public double Pitch
		{
			get
			{
				return this.MyPitch;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// <param name="Radius">The curve radius at the front of the train, in meters.</param>
		/// <param name="Cant">The cant value for this curve radius.</param>
		/// <param name="Pitch">The pitch value at the front of the train.</param>
		/// Three paramaters added at the far end
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure, double Radius, double Cant, double Pitch)
		{
			this.MyRadius = Radius;
			this.MyCant = Cant;
			this.MyPitch = Pitch;
			this.MyLocation = location;
			this.MySpeed = speed;
			this.MyBcPressure = bcPressure;
			this.MyMrPressure = mrPressure;
			this.MyErPressure = erPressure;
			this.MyBpPressure = bpPressure;
			this.MySapPressure = sapPressure;
		}

		//This provides the overload for plugins built against versions of the OpenBVE API below 1.4.4.0
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// Three paramaters added at the far end
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure)
		{
			this.MyLocation = location;
			this.MySpeed = speed;
			this.MyBcPressure = bcPressure;
			this.MyMrPressure = mrPressure;
			this.MyErPressure = erPressure;
			this.MyBpPressure = bpPressure;
			this.MySapPressure = sapPressure;
		}
	}
	
	/// <summary>Represents the current state of the preceding train.</summary>
	public class PrecedingVehicleState {
		// --- members ---
		/// <summary>The location of the back of the preceding train, in meters.</summary>
		private readonly double MyLocation;
		/// <summary>The distance from the front of the current train to the back of the preceding train, in meters.</summary>
		private readonly double MyDistance;
		/// <summary>The current speed of the preceding train.</summary>
		private readonly Speed MySpeed;
		// --- properties ---
		/// <summary>Gets the location of the back of the preceding train, in meters.</summary>
		public double Location {
			get {
				return this.MyLocation;
			}
		}
		/// <summary>Gets the distance from the front of the current train to the back of the preceding train, in meters.</summary>
		public double Distance {
			get {
				return this.MyDistance;
			}
		}
		/// <summary>Gets the speed of the preceding train.</summary>
		public Speed Speed {
			get {
				return this.MySpeed;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">Gets the location of the back of the preceding train, in meters.</param>
		/// <param name="distance">The distance from the front of the current train to the back of the preceding train, in meters.</param>
		/// <param name="speed">Gets the speed of the preceding train.</param>
		public PrecedingVehicleState(double location, double distance, Speed speed) {
			this.MyLocation = location;
			this.MyDistance = distance;
			this.MySpeed = speed;
		}
	}
	
	/// <summary>Represents the handles of the cab.</summary>
	public class Handles {
		// --- members ---
		/// <summary>The reverser position.</summary>
		private int MyReverser;
		/// <summary>The power notch.</summary>
		private int MyPowerNotch;
		/// <summary>The brake notch.</summary>
		private int MyBrakeNotch;
		/// <summary>Whether the const speed system is enabled.</summary>
		private bool MyConstSpeed;
		// --- properties ---
		/// <summary>Gets or sets the reverser position.</summary>
		public int Reverser {
			get {
				return this.MyReverser;
			}
			set {
				this.MyReverser = value;
			}
		}
		/// <summary>Gets or sets the power notch.</summary>
		public int PowerNotch {
			get {
				return this.MyPowerNotch;
			}
			set {
				this.MyPowerNotch = value;
			}
		}
		/// <summary>Gets or sets the brake notch.</summary>
		public int BrakeNotch {
			get {
				return this.MyBrakeNotch;
			}
			set {
				this.MyBrakeNotch = value;
			}
		}
		/// <summary>Gets or sets whether the const speed system is enabled.</summary>
		public bool ConstSpeed {
			get {
				return this.MyConstSpeed;
			}
			set {
				this.MyConstSpeed = value;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, bool constSpeed) {
			this.MyReverser = reverser;
			this.MyPowerNotch = powerNotch;
			this.MyBrakeNotch = brakeNotch;
			this.MyConstSpeed = constSpeed;
		}
	}

	/// <summary>Represents data given to the plugin in the Elapse call.</summary>
	public class ElapseData {
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
		private int CurrentDestination;
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
		public ElapseData(VehicleState vehicle, PrecedingVehicleState precedingVehicle, Handles handles, DoorInterlockStates doorinterlock, Time totalTime, Time elapsedTime, List<Station> stations, CameraViewMode cameraView, string languageCode, int destination) {
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
		public VehicleState Vehicle {
			get {
				return this.MyVehicle;
			}
		}
		/// <summary>Gets the state of the preceding train, or a null reference if there is no preceding train.</summary>
		public PrecedingVehicleState PrecedingVehicle {
			get {
				return this.MyPrecedingVehicle;
			}
		}
		/// <summary>Gets or sets the virtual handles.</summary>
		public Handles Handles {
			get {
				return this.MyHandles;
			}
			set {
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
		public Time TotalTime {
			get {
				return this.MyTotalTime;
			}
		}
		/// <summary>Gets the time that elapsed since the last call to Elapse.</summary>
		public Time ElapsedTime {
			get {
				return this.MyElapsedTime;
			}
		}
		/// <summary>Gets or sets the debug message the plugin wants the host application to display.</summary>
		public string DebugMessage {
			get {
				return this.MyDebugMessage;
			}
			set {
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
			get { return this.MyStations; }
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
	
	// --- key down / key up ---
	
	/// <summary>Represents a virtual key.</summary>
	public enum VirtualKeys {
		/// <summary>The virtual S key. The default assignment is [Space]. The numerical value of this constant is 0.</summary>
		S = 0,
		/// <summary>The virtual A1 key. The default assignment is [Insert]. The numerical value of this constant is 1.</summary>
		A1 = 1,
		/// <summary>The virtual A2 key. The default assignment is [Delete]. The numerical value of this constant is 2.</summary>
		A2 = 2,
		/// <summary>The virtual B1 key. The default assignment is [Home]. The numerical value of this constant is 3.</summary>
		B1 = 3,
		/// <summary>The virtual B2 key. The default assignment is [End]. The numerical value of this constant is 4.</summary>
		B2 = 4,
		/// <summary>The virtual C1 key. The default assignment is [PageUp]. The numerical value of this constant is 5.</summary>
		C1 = 5,
		/// <summary>The virtual C2 key. The default assignment is [PageDown]. The numerical value of this constant is 6.</summary>
		C2 = 6,
		/// <summary>The virtual D key. The default assignment is [2]. The numerical value of this constant is 7.</summary>
		D = 7,
		/// <summary>The virtual E key. The default assignment is [3]. The numerical value of this constant is 8.</summary>
		E = 8,
		/// <summary>The virtual F key. The default assignment is [4]. The numerical value of this constant is 9.</summary>
		F = 9,
		/// <summary>The virtual G key. The default assignment is [5]. The numerical value of this constant is 10.</summary>
		G = 10,
		/// <summary>The virtual H key. The default assignment is [6]. The numerical value of this constant is 11.</summary>
		H = 11,
		/// <summary>The virtual I key. The default assignment is [7]. The numerical value of this constant is 12.</summary>
		I = 12,
		/// <summary>The virtual J key. The default assignment is [8]. The numerical value of this constant is 13.</summary>
		J = 13,
		/// <summary>The virtual K key. The default assignment is [9]. The numerical value of this constant is 14.</summary>
		K = 14,
		/// <summary>The virtual L key. The default assignment is [N/A]. The numerical value of this constant is 15.</summary>
		L = 15,
		/// <summary>The virtual M key. The default assignment is [N/A]. The numerical value of this constant is 16.</summary>
		M = 16,
		/// <summary>The virtual N key. The default assignment is [N/A]. The numerical value of this constant is 17.</summary>
		N = 17,
		/// <summary>The virtual O key. The default assignment is [N/A]. The numerical value of this constant is 18.</summary>
		O = 18,
		/// <summary>The virtual P key. The default assignment is [N/A]. The numerical value of this constant is 19.</summary>
		P = 19,
		//Keys Added
		//Common Keys
		/// <summary>Increases the speed of the windscreen wipers. The default assignment is [N/A]. The numerical value of this constant is 20.</summary>
		WiperSpeedUp = 20,
		/// <summary>Decreases the speed of the windscreen wipers. The default assignment is [N/A]. The numerical value of this constant is 21.</summary>
		WiperSpeedDown = 21,
		/// <summary>Fills fuel. The default assignment is [N/A]. The numerical value of this constant is 22.</summary>
		FillFuel = 22,
		//Steam locomotive
		/// <summary>Toggles the live-steam injector. The default assignment is [N/A]. The numerical value of this constant is 23.</summary>
		LiveSteamInjector= 23,
		/// <summary>Toggles the exhaust steam injector. The default assignment is [N/A]. The numerical value of this constant is 24.</summary>
		ExhaustSteamInjector= 24,
		/// <summary>Increases the cutoff. The default assignment is [N/A]. The numerical value of this constant is 25.</summary>
		IncreaseCutoff= 25,
		/// <summary>Decreases the cutoff. The default assignment is [N/A]. The numerical value of this constant is 26.</summary>
		DecreaseCutoff=26,
		/// <summary>Toggles the blowers. The default assignment is [N/A]. The numerical value of this constant is 27.</summary>
		Blowers= 27,
		//Diesel Locomotive
		/// <summary>Starts the engine. The default assignment is [N/A]. The numerical value of this constant is 28.</summary>
		EngineStart= 28,
		/// <summary>Stops the engine. The default assignment is [N/A]. The numerical value of this constant is 29.</summary>
		EngineStop= 29,
		/// <summary>Changes gear up. The default assignment is [N/A]. The numerical value of this constant is 30.</summary>
		GearUp= 30,
		/// <summary>Changes gear down. The default assignment is [N/A]. The numerical value of this constant is 31.</summary>
		GearDown= 31,
		//Electric Locomotive
		/// <summary>Raises the pantograph. The default assignment is [N/A]. The numerical value of this constant is 32.</summary>
		RaisePantograph= 32,
		/// <summary>Lowers the pantograph. The default assignment is [N/A]. The numerical value of this constant is 33.</summary>
		LowerPantograph= 33,
		/// <summary>Toggles the main breaker. The default assignment is [N/A]. The numerical value of this constant is 34.</summary>
		MainBreaker= 34,
		/// <summary>Called when the driver presses the left door button [NOTE: This is called whether or not opening succeeds/ is blocked]</summary>
		LeftDoors = 35,
		/// <summary>Called when the driver presses the right door button [NOTE: This is called whether or not opening succeeds/ is blocked]</summary>
		RightDoors = 36

	}
	
	
	// --- horn blow ---
	
	/// <summary>Represents the type of horn.</summary>
	public enum HornTypes {
		/// <summary>The primary horn. The numerical value of this constant is 0.</summary>
		Primary = 1,
		/// <summary>The secondary horn. The numerical value of this constant is 1.</summary>
		Secondary = 2,
		/// <summary>The music horn. The numerical value of this constant is 2.</summary>
		Music = 3
	}

	/// <summary>Represents the available camera view modes.</summary>
	public enum CameraViewMode
	{
		/// <summary>The interior of a 2D cab</summary>
		Interior,
		/// <summary>The interior of a 3D cab</summary>
		InteriorLookAhead,
		/// <summary>An exterior camera attached to a train</summary>
		Exterior,
		/// <summary>A camera attached to the track</summary>
		Track,
		/// <summary>A fly-by camera attached to a point on the track</summary>
		FlyBy,
		/// <summary>A fly-by zooming camera attached to a point on the track</summary>
		FlyByZooming
	}

	/// <summary>Represents the states of the door interlock.</summary>
	public enum DoorInterlockStates
	{
		/// <summary>The train doors are fully unlocked.</summary>
		Unlocked = 0,
		/// <summary>The train doors are unlocked only on the left side.</summary>
		Left = 1,
		/// <summary>The train doors are unlocked only on the right side.</summary>
		Right = 2,
		/// <summary>The train doors are fully locked.</summary>
		Locked = 3,
	}

	// --- door change ---
	
	/// <summary>Represents the state of the doors.</summary>
	[Flags]
	public enum DoorStates {
		/// <summary>No door is open.</summary>
		None = 0,
		/// <summary>The left doors are open.</summary>
		Left = 1,
		/// <summary>The right doors are open.</summary>
		Right = 2,
		/// <summary>All doors are open.</summary>
		Both = 3
	}
	
	
	// --- set signal ---
	
	/// <summary>Represents information about a signal or section.</summary>
	public class SignalData {
		// --- members ---
		/// <summary>The aspect of the signal or section.</summary>
		private readonly int MyAspect;
		/// <summary>The underlying section. Possible values are 0 for the current section, 1 for the upcoming section, or higher values for sections further ahead.</summary>
		private readonly double MyDistance;
		// --- properties ---
		/// <summary>Gets the aspect of the signal or section.</summary>
		public int Aspect {
			get {
				return this.MyAspect;
			}
		}
		/// <summary>Gets the distance to the signal or section.</summary>
		public double Distance {
			get {
				return this.MyDistance;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="aspect">The aspect of the signal or section.</param>
		/// <param name="distance">The distance to the signal or section.</param>
		public SignalData(int aspect, double distance) {
			this.MyAspect = aspect;
			this.MyDistance = distance;
		}
	}
	
	
	// --- set beacon ---
	
	/// <summary>Represents data trasmitted by a beacon.</summary>
	public class BeaconData {
		// --- members ---
		/// <summary>The type of beacon.</summary>
		private readonly int MyType;
		/// <summary>Optional data the beacon transmits.</summary>
		private readonly int MyOptional;
		/// <summary>The section the beacon is attached to.</summary>
		private readonly SignalData MySignal;
		// --- properties ---
		/// <summary>Gets the type of beacon.</summary>
		public int Type {
			get {
				return this.MyType;
			}
		}
		/// <summary>Gets optional data the beacon transmits.</summary>
		public int Optional {
			get {
				return this.MyOptional;
			}
		}
		/// <summary>Gets the section the beacon is attached to.</summary>
		public SignalData Signal {
			get {
				return this.MySignal;
			}
		}
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="type">The type of beacon.</param>
		/// <param name="optional">Optional data the beacon transmits.</param>
		/// <param name="signal">The section the beacon is attached to.</param>
		public BeaconData(int type, int optional, SignalData signal) {
			this.MyType = type;
			this.MyOptional = optional;
			this.MySignal = signal;
		}
	}
	
	// --- perform AI ---

	/// <summary>Represents responses by the AI.</summary>
	public enum AIResponse {
		/// <summary>No action was performed by the plugin.</summary>
		None = 0,
		/// <summary>The action performed took a short time.</summary>
		Short = 1,
		/// <summary>The action performed took an average amount of time.</summary>
		Medium = 2,
		/// <summary>The action performed took a long time.</summary>
		Long = 3
	}
	
	/// <summary>Represents AI data.</summary>
	public class AIData {
		// --- members ---
		/// <summary>The driver handles.</summary>
		private Handles MyHandles;
		/// <summary>The AI response.</summary>
		private AIResponse MyResponse;
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="handles">The driver handles.</param>
		public AIData(Handles handles) {
			this.MyHandles = handles;
			this.MyResponse = AIResponse.None;
		}
		// --- properties ---
		/// <summary>Gets or sets the driver handles.</summary>
		public Handles Handles {
			get {
				return this.MyHandles;
			}
			set {
				this.MyHandles = value;
			}
		}
		/// <summary>Gets or sets the AI response.</summary>
		public AIResponse Response {
			get {
				return this.MyResponse;
			}
			set {
				this.MyResponse = value;
			}
		}
	}
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for performing runtime train services.</summary>
	public interface IRuntime {

		/// <summary>Is called when the plugin is loaded.</summary>
		/// <param name="properties">The properties supplied to the plugin on loading.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		/// <remarks>If the plugin was not loaded successfully, the plugin should set the Reason property to supply the reason of failure.</remarks>
		bool Load(LoadProperties properties);
		
		/// <summary>Is called when the plugin is unloaded.</summary>
		void Unload();
		
		/// <summary>Is called after loading to inform the plugin about the specifications of the train.</summary>
		/// <param name="specs">The specifications of the train.</param>
		void SetVehicleSpecs(VehicleSpecs specs);
		
		/// <summary>Is called when the plugin should initialize or reinitialize.</summary>
		/// <param name="mode">The mode of initialization.</param>
		void Initialize(InitializationModes mode);
		
		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data passed to the plugin.</param>
		void Elapse(ElapseData data);
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		void SetReverser(int reverser);
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		void SetPower(int powerNotch);
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		void SetBrake(int brakeNotch);
		
		/// <summary>Is called when a virtual key is pressed.</summary>
		/// <param name="key">The virtual key that was pressed.</param>
		void KeyDown(VirtualKeys key);
		
		/// <summary>Is called when a virtual key is released.</summary>
		/// <param name="key">The virtual key that was released.</param>
		void KeyUp(VirtualKeys key);
		
		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		void HornBlow(HornTypes type);
		
		/// <summary>Is called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		void DoorChange(DoorStates oldState, DoorStates newState);
		
		/// <summary>Is called when the aspect in the current or in any of the upcoming sections changes, or when passing section boundaries.</summary>
		/// <param name="data">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
		/// <remarks>The signal array is guaranteed to have at least one element. When accessing elements other than index 0, you must check the bounds of the array first.</remarks>
		void SetSignal(SignalData[] data);
		
		/// <summary>Is called when the train passes a beacon.</summary>
		/// <param name="data">The beacon data.</param>
		void SetBeacon(BeaconData data);
		
		/// <summary>Is called when the plugin should perform the AI.</summary>
		/// <param name="data">The AI data.</param>
		void PerformAI(AIData data);
		
	}
	
}
