// ╔══════════════════════════════════════════════════════════════╗
// ║ Interface.cs and TrainManager.cs for the Structure Viewer    ║
// ╠══════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.        ║
// ║ The files from the openBVE main program cannot be used here. ║
// ╚══════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using TrainManager.Trains;

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
		
		internal static void Reset()
		{
			Program.TrainManager.Trains = new TrainBase[] { };
			TrainManager.PlayerTrain = null;
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
			private ObjectOptimizationMode objectOptimizationMode;

			/// <summary>
			/// The mode of optimization to be performed on an object
			/// </summary>
			internal ObjectOptimizationMode ObjectOptimizationMode
			{
				get
				{
					return objectOptimizationMode;
				}
				set
				{
					objectOptimizationMode = value;

					switch (value)
					{
						case ObjectOptimizationMode.None:
							ObjectOptimizationBasicThreshold = 0;
							ObjectOptimizationFullThreshold = 0;
							break;
						case ObjectOptimizationMode.Low:
							ObjectOptimizationBasicThreshold = 1000;
							ObjectOptimizationFullThreshold = 250;
							break;
						case ObjectOptimizationMode.High:
							ObjectOptimizationBasicThreshold = 10000;
							ObjectOptimizationFullThreshold = 1000;
							break;
					}
				}
			}

			internal Options()
			{
				ObjectOptimizationMode = ObjectOptimizationMode.Low;
			}
		}

		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;
	}
}
