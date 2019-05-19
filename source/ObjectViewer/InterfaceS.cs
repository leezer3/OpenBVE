// ╔══════════════════════════════════════════════════════════════╗
// ║ Interface.cs and TrainManager.cs for the Structure Viewer    ║
// ╠══════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.        ║
// ║ The files from the openBVE main program cannot be used here. ║
// ╚══════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve {

	// --- TimeTable.cs ---
	internal static class Timetable {
		internal static void AddObjectForCustomTimetable(ObjectManager.AnimatedObject obj) { }
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
		internal static int InfoTotalTriangles = 0;
		internal static int InfoTotalTriangleStrip = 0;
		internal static int InfoTotalQuads = 0;
		internal static int InfoTotalQuadStrip = 0;
		internal static int InfoTotalPolygon = 0;
		internal static void Reset() {
			Renderer.Reset();
			ObjectManager.Objects = new StaticObject[16];
			ObjectManager.ObjectsUsed = 0;
			ObjectManager.ObjectsSortedByStart = new int[] { };
			ObjectManager.ObjectsSortedByEnd = new int[] { };
			ObjectManager.ObjectsSortedByStartPointer = 0;
			ObjectManager.ObjectsSortedByEndPointer = 0;
			ObjectManager.LastUpdatedTrackPosition = 0.0;
			ObjectManager.AnimatedWorldObjects = new ObjectManager.AnimatedWorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
		}
	}
	
	// --- TrackManager.cs ---
	internal static class TrackManager {
		internal struct TrackFollower {
			internal double TrackPosition;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
            internal double CurveRadius;
            internal double CurveCant;
			internal double Pitch;
            internal double CantDueToInaccuracy;
		}
		internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) { }
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

		// ================================
		internal struct Options {
			internal InterpolationMode Interpolation;
            internal TransparencyMode TransparencyMode;
			internal int AnisotropicFilteringLevel;
			internal int AnisotropicFilteringMaximum;
		    internal int AntialiasingLevel;
			internal int ObjectOptimizationBasicThreshold;
			internal int ObjectOptimizationFullThreshold;
			internal XParsers CurrentXParser;
			internal ObjParsers CurrentObjParser;
		}
		internal static Options CurrentOptions;

		// ================================

#pragma warning restore 0649
	}
}
