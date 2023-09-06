// ReSharper disable UnusedMember.Global
namespace OpenBveApi.Runtime
{
	/// <summary>Represents properties supplied to the plugin on loading.</summary>
	public class LoadProperties
	{
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

		/// <summary>The callback function for playing car-based  sounds.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly PlayMultipleCarSoundDelegate MyPlayMultipleCarSound;

		/// <summary>The callback function for adding interface messages.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly AddInterfaceMessageDelegate MyAddInterfaceMessage;

		/// <summary>The callback function for adding or subtracting scores.</summary>
		/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
		private readonly AddScoreDelegate MyAddScore;

		/// <summary>The callback function for opening the train doors.</summary>
		private readonly OpenDoorsDelegate MyOpenDoors;

		/// <summary>The callback function for closing the train doors.</summary>
		private readonly CloseDoorsDelegate MyCloseDoors;

		/// <summary>The extent to which the plugin supports the AI.</summary>
		private AISupport MyAISupport;

		/// <summary>The reason why the plugin failed loading.</summary>
		private string MyFailureReason;

		/// <summary>Gets the absolute path to the plugin folder.</summary>
		public string PluginFolder => MyPluginFolder;

		/// <summary>Gets the absolute path to the train folder.</summary>
		public string TrainFolder => MyTrainFolder;

		/// <summary>Gets or sets the array of panel variables.</summary>
		public int[] Panel
		{
			get => MyPanel;
			set => MyPanel = value;
		}

		/// <summary>Gets the callback function for playing sounds.</summary>
		public PlaySoundDelegate PlaySound => MyPlaySound;

		/// <summary>Gets the callback function for playing sounds.</summary>
		public PlayCarSoundDelegate PlayCarSound => MyPlayCarSound;

		/// <summary>Gets the callback function for playing sounds.</summary>
		public PlayMultipleCarSoundDelegate PlayMultipleCarSound => MyPlayMultipleCarSound;

		/// <summary>Gets the callback function for opening the train doors</summary>
		public OpenDoorsDelegate OpenDoors => MyOpenDoors;

		/// <summary>Gets the callback function for closing the train doors</summary>
		public CloseDoorsDelegate CloseDoors => MyCloseDoors;

		/// <summary>Gets the callback function for adding interface messages.</summary>
		public AddInterfaceMessageDelegate AddMessage => MyAddInterfaceMessage;

		/// <summary>Gets the callback function for adding interface messages.</summary>
		public AddScoreDelegate AddScore => MyAddScore;

		/// <summary>Gets or sets the extent to which the plugin supports the AI.</summary>
		public AISupport AISupport
		{
			get => MyAISupport;
			set => MyAISupport = value;
		}

		/// <summary>Gets or sets the reason why the plugin failed loading.</summary>
		public string FailureReason
		{
			get => MyFailureReason;
			set => MyFailureReason = value;
		}

		/*
		 * DetailManager.dll reconstructs the LoadProperties in order to pass it through to child plugins
		 * This isn't necessarily supported, but other people may do something similar with a shipped API
		 * so keep around the old constructors
		 * https://github.com/leezer3/OpenBVE/issues/813
		 */

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="pluginFolder">The absolute path to the plugin folder.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="playSound">The callback function for playing sounds.</param>
		/// <param name="playCarSound">The callback function for playing car-based sounds.</param>
		/// <param name="addMessage">The callback function for adding interface messages.</param>
		/// <param name="addScore">The callback function for adding scores.</param>
		public LoadProperties(string pluginFolder, string trainFolder, PlaySoundDelegate playSound, PlayCarSoundDelegate playCarSound, AddInterfaceMessageDelegate addMessage, AddScoreDelegate addScore)
		{
			MyPluginFolder = pluginFolder;
			MyTrainFolder = trainFolder;
			MyPlaySound = playSound;
			MyPlayCarSound = playCarSound;
			MyAddInterfaceMessage = addMessage;
			MyAddScore = addScore;
			MyFailureReason = null;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="pluginFolder">The absolute path to the plugin folder.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="playSound">The callback function for playing sounds.</param>
		/// <param name="playCarSound">The callback function for playing car-based sounds.</param>
		/// <param name="addMessage">The callback function for adding interface messages.</param>
		/// <param name="addScore">The callback function for adding scores.</param>
		/// <param name="openDoors">The callback function for opening the train doors</param>
		/// <param name="closeDoors">The callback function for closing the train doors</param>
		public LoadProperties(string pluginFolder, string trainFolder, PlaySoundDelegate playSound, PlayCarSoundDelegate playCarSound, AddInterfaceMessageDelegate addMessage, AddScoreDelegate addScore, OpenDoorsDelegate openDoors, CloseDoorsDelegate closeDoors)
		{
			MyPluginFolder = pluginFolder;
			MyTrainFolder = trainFolder;
			MyPlaySound = playSound;
			MyPlayCarSound = playCarSound;
			MyAddInterfaceMessage = addMessage;
			MyAddScore = addScore;
			MyOpenDoors = openDoors;
			MyCloseDoors = closeDoors;
			MyFailureReason = null;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="pluginFolder">The absolute path to the plugin folder.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="playSound">The callback function for playing sounds.</param>
		/// <param name="playCarSound">The callback function for playing car-based sounds.</param>
		/// <param name="playMultiCarSound">The callback function for playing a sound on multiple cars</param>
		/// <param name="addMessage">The callback function for adding interface messages.</param>
		/// <param name="addScore">The callback function for adding scores.</param>
		/// <param name="openDoors">The callback function for opening the train doors</param>
		/// <param name="closeDoors">The callback function for closing the train doors</param>
		public LoadProperties(string pluginFolder, string trainFolder, PlaySoundDelegate playSound, PlayCarSoundDelegate playCarSound, PlayMultipleCarSoundDelegate playMultiCarSound, AddInterfaceMessageDelegate addMessage, AddScoreDelegate addScore, OpenDoorsDelegate openDoors, CloseDoorsDelegate closeDoors)
		{
			MyPluginFolder = pluginFolder;
			MyTrainFolder = trainFolder;
			MyPlaySound = playSound;
			MyPlayCarSound = playCarSound;
			MyPlayMultipleCarSound = playMultiCarSound;
			MyAddInterfaceMessage = addMessage;
			MyAddScore = addScore;
			MyOpenDoors = openDoors;
			MyCloseDoors = closeDoors;
			MyFailureReason = null;
		}
	}
}
