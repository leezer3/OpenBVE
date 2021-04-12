// ╔══════════════════════════════════════════════════════════════╗
// ║ Interface.cs and TrainManager.cs for the Structure Viewer    ║
// ╠══════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.        ║
// ║ The files from the openBVE main program cannot be used here. ║
// ╚══════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace OpenBve {
	// --- PluginManager.cs ---
	internal static class PluginManager {
		internal static class CurrentPlugin {
			internal static int[] Panel = new int[] { };
		}
	}

	// --- Game.cs ---
	internal static class Game {
		internal static double SecondsSinceMidnight = 0.0;
		
		internal static void Reset() {
			Interface.LogMessages.Clear();
			Program.CurrentHost.MissingFiles.Clear();
			Program.Renderer.Reset();
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new WorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
		}
	}
	
	// --- Interface.cs ---
	internal static class Interface {

		internal static readonly List<LogMessage> LogMessages = new List<LogMessage>();
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			LogMessages.Add(new LogMessage(Type, FileNotFound, Text));
		}
		/// <summary>Holds the program specific options</summary>
		internal class Options : BaseOptions
		{
		}

		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;
	}
}
