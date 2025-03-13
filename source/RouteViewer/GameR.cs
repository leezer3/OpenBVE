// ╔═════════════════════════════════════════════════════════════╗
// ║ Game.cs for the Route Viewer                                ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Collections.Generic;
using OpenBveApi.Colors;
using OpenBveApi.Trains;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Stations;
using RouteManager2.Tracks;
using TrainManager.Trains;

namespace RouteViewer {
	internal static class Game {

		// date and time
		internal static double SecondsSinceMidnight = 0.0;
		/// <summary>The in-game menu system</summary>
		internal static readonly GameMenu Menu = GameMenu.Instance;

		// ================================

		internal static void Reset(bool resetRenderer = true) {
			if (resetRenderer)
			{
				Program.Renderer.Reset();
			}
			
			// track manager
			Program.CurrentRoute.Tracks = new Dictionary<int, Track>();
			Program.CurrentRoute.Tracks.Add(0, new Track());
			// train manager
			Program.TrainManager.Trains = new List<TrainBase>();
			// game
			Interface.LogMessages.Clear();
			Program.CurrentHost.ClearErrors();
			Program.CurrentRoute.Comment = "";
			Program.CurrentRoute.Image = "";
			Program.CurrentRoute.Atmosphere = new Atmosphere();
			Program.CurrentRoute.LightDefinitions = new LightDefinition[] { };
			Program.CurrentRoute.Stations = new RouteStation[] { };
			Program.CurrentRoute.Sections = new Section[] { };
			Program.CurrentRoute.BufferTrackPositions = new List<BufferStop>();
			Program.CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			Program.CurrentRoute.BogusPreTrainInstructions = new BogusPreTrainInstruction[] { };
			Interface.CurrentOptions.TrainName = "";
			Interface.CurrentOptions.TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			Program.CurrentRoute.NoFogStart = (float)Math.Max(1.33333333333333 * Interface.CurrentOptions.ViewingDistance, 800.0);
			Program.CurrentRoute.NoFogEnd = (float)Math.Max(2.66666666666667 * Interface.CurrentOptions.ViewingDistance, 1600.0);
			Program.CurrentRoute.PreviousFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.0);
			Program.CurrentRoute.CurrentFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.5);
			Program.CurrentRoute.NextFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 1.0);
			Program.Renderer.InfoTotalTriangles = 0;
			Program.Renderer.InfoTotalTriangleStrip = 0;
			Program.Renderer.InfoTotalQuads = 0;
			Program.Renderer.InfoTotalQuadStrip = 0;
			Program.Renderer.InfoTotalPolygon = 0;
			Program.Renderer.VisibleObjects.quadTree.Clear();
			// object manager
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new WorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
			// renderer / sound
			Program.Sounds.StopAllSounds();
			GC.Collect();
		}
	}
}
