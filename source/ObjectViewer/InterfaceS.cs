using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using TrainManager.Trains;

namespace ObjectViewer {
	// --- PluginManager.cs ---
	internal static class PluginManager {
		internal static class CurrentPlugin {
			internal static int[] Panel = { };
		}
	}

	// --- Game.cs ---
	internal static class Game {
		internal static double SecondsSinceMidnight = 0.0;
		
		internal static void Reset()
		{
			Program.TrainManager.Trains = new List<TrainBase>();
			TrainManager.PlayerTrain = null;
			Interface.LogMessages.Clear();
			Program.CurrentHost.ClearErrors();
			Program.Renderer.Reset();
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new WorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
		}

		/// <summary>The in-game menu system</summary>
		internal static readonly GameMenu Menu = GameMenu.Instance;
	}
	
	// --- Interface.cs ---
	internal static class Interface {

		internal static readonly List<LogMessage> LogMessages = new List<LogMessage>();

		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			LogMessages.Add(new LogMessage(Type, FileNotFound, Text));
		}
		
		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;
	}
}
