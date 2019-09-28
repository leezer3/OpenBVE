// ╔══════════════════════════════════════════════════════════════╗
// ║ Interface.cs and TrainManager.cs for the Structure Viewer    ║
// ╠══════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.        ║
// ║ The files from the openBVE main program cannot be used here. ║
// ╚══════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve {

	// --- TimeTable.cs ---
	internal static class Timetable {
		internal static void AddObjectForCustomTimetable(AnimatedObject obj) { }
		internal enum TimetableState {
			None = 0,
			Custom = 1,
			Default = 2
		}
		internal static TimetableState CurrentTimetable = TimetableState.None;
		internal static bool CustomTimetableAvailable = false;
	}

	// --- PluginManager.cs ---
	internal static class PluginManager {
		internal static class CurrentPlugin {
			internal static int[] Panel = new int[] { };
		}
	}

#pragma warning disable 0649

	// --- Game.cs ---
	internal static class Game {
		internal static double SecondsSinceMidnight = 0.0;
		internal enum SectionType { ValueBased, IndexBased }
		internal struct SectionAspect {
			internal int Number;
			internal double Speed;
		}
		
		internal static void Reset() {
			Program.Renderer.Reset();
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new AnimatedWorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
		}
	}
	
	// --- Interface.cs ---
	internal static class Interface {

		internal static LogMessage[] LogMessages = new LogMessage[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (MessageCount == 0) {
				LogMessages = new LogMessage[16];
			} else if (MessageCount >= LogMessages.Length) {
				Array.Resize<LogMessage>(ref LogMessages, LogMessages.Length << 1);
			}
			LogMessages[MessageCount] = new LogMessage(Type, FileNotFound, Text);
			MessageCount++;
		}
		internal static void ClearMessages() {
			LogMessages = new LogMessage[] { };
			MessageCount = 0;
		}

		/// <summary>Holds the program specific options</summary>
		internal class Options : BaseOptions
		{
		}

		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;

		// ================================

#pragma warning restore 0649
	}
}
