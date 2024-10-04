using System.Collections.Generic;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Input;
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
			Program.TrainManager.Trains = new TrainBase[] { };
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
		/// <summary>Holds the program specific options</summary>
		internal class Options : BaseOptions
		{
			private ObjectOptimizationMode objectOptimizationMode;

			internal string ObjectSearchDirectory;

			internal Key CameraMoveLeft;

			internal Key CameraMoveRight;

			internal Key CameraMoveUp;

			internal Key CameraMoveDown;

			internal Key CameraMoveForward;

			internal Key CameraMoveBackward;

			/// <summary>
			/// The mode of optimization to be performed on an object
			/// </summary>
			internal ObjectOptimizationMode ObjectOptimizationMode
			{
				get => objectOptimizationMode;
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
