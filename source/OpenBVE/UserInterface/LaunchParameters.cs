namespace OpenBve
{
	/// <summary>The parameters to apply when launching the main game</summary>
	internal struct LaunchParameters
	{
		/// <summary>Whether to start the simulation</summary>
		internal bool Start;
		/// <summary>The absolute on-disk path of the route file to start the simulation with</summary>
		internal string RouteFile;
		/// <summary>The last file an error was encountered on (Used for changing character encodings)</summary>
		internal string ErrorFile;
		/// <summary>The text encoding of the selected route file</summary>
		internal System.Text.Encoding RouteEncoding;
		/// <summary>The absolute on-disk path of the train folder to start the simulation with</summary>
		internal string TrainFolder;
		/// <summary>The text encoding of the selected train</summary>
		internal System.Text.Encoding TrainEncoding;
		/// <summary>Whether the consist of the train is to be reversed on start</summary>
		internal bool ReverseConsist;
		/// <summary>The textual name of the station the player's train is to start at</summary>
		internal string InitialStation;
		/// <summary>The time the game is to start in seconds after midnight</summary>
		internal double StartTime;
		/// <summary>Whether the AI driver is on when the game starts</summary>
		internal bool AIDriver;
		/// <summary>Whether the game starts in full-screen mode</summary>
		internal bool FullScreen;
		/// <summary>The game width</summary>
		internal int Width;
		/// <summary>The game height</summary>
		internal int Height;
		/// <summary>Whether to show the experimental GL menu</summary>
		internal bool ExperimentalGLMenu;
	}
}
