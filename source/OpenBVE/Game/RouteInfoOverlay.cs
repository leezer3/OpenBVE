using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenBveApi.Textures;
using OpenBveApi.Interface;

namespace OpenBve
{
	/********************
		ROUTE INFO OVERLAY
	*********************
	Implements the in-game overlay with graphical information about the route: map and gradient. */

	/// <summary>Displays an in-game overlay with information about the route, including a map and gradient profile</summary>
	public class RouteInfoOverlay
	{
		//
		// DEFINITIONS & TYPDEFS
		//
		private enum state
		{
			none	= 0,
			map,
			gradient,
			numOf
		};
		// properties of the train position vertical bar in gradient profile
		private const int			gradientPosWidth= 2;
		private static readonly Color128	gradientPosBar	= Color128.White;
		//and of reain dot in route map
		private const int			trainDotRadius	= 4;
		private const int			trainDotDiameter= (trainDotRadius * 2);
		private static readonly Color128	trainDotColour			= Color128.Red;
		private static readonly Color128	playerTrainDotColour	= Color128.Green;

		//
		// FIELDS
		//
		private state				currentState	= state.none;
		private Texture	gradientImage	= null;
		private Size				gradientSize;
		private Texture	mapImage		= null;
		private Size				mapSize;

		/********************
			PUBLIC METHODS
		*********************/
		//
		// PROCESS COMMAND
		//
		/// <summary>Processes commands.</summary>
		/// <returns><c>true</c>, if command was processed, <c>false</c> otherwise.</returns>
		/// <param name="command">The Translations.Command command to process.</param>
		internal bool ProcessCommand(Translations.Command command)
		{
			if (command != Translations.Command.RouteInformation)	// only accept RouteInformation command
				return false;
			// cycle through available state
			setState( (state)((int)(currentState + 1) % (int)state.numOf) );
			return true;
		}

		//
		// SHOW THE OVERLAY
		//
		/// <summary>Displays the current state into the simulation window.</summary>
		public void Show()
		{
			if (currentState == state.none)
				return;
			int xPos, zPos;
			// size the image to half of the smallest screen size, but not larger than default size
			// NO: compressing the image below its original size makes texs hard to read
//			int		width		= Math.Min(Math.Min(Screen.Height, Screen.Width) / 2,
//						Game.RouteInformation.DefaultRouteInfoSize);
			Point	origin		= new Point(0, 0);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			// draw the relevant image
			switch (currentState)
			{
			case state.map:
				Renderer.DrawRectangle(mapImage, origin, mapSize, null);
				// get current train position
				int n = TrainManager.Trains.Length;
				for (int i = 0; i < n; i++)
				{
					int trainX = (int)TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.X;
					int trainZ = (int)TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.Z;
					// convert to route map coordinates
					xPos = mapSize.Width * (trainX - Game.RouteInformation.RouteMinX) /
							(Game.RouteInformation.RouteMaxX - Game.RouteInformation.RouteMinX) - trainDotRadius;
					zPos = mapSize.Height - mapSize.Height * (trainZ - Game.RouteInformation.RouteMinZ) /
							(Game.RouteInformation.RouteMaxZ - Game.RouteInformation.RouteMinZ) - trainDotRadius;
					// draw a dot at current train position
					Renderer.DrawRectangle(null, new Point(xPos, zPos),
							new Size(trainDotDiameter, trainDotDiameter),
							TrainManager.Trains[i] == TrainManager.PlayerTrain ? playerTrainDotColour : trainDotColour);
				}
				break;
			case state.gradient:
				Renderer.DrawRectangle(gradientImage, origin, gradientSize, null);
				// get current train position in track
				int trackPos	= (int)(TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition
						- TrainManager.PlayerTrain.Cars[0].FrontAxle.Position
					+ 0.5 * TrainManager.PlayerTrain.Cars[0].Length);
				// convert to gradient profile offset
				xPos = gradientSize.Width * (trackPos - Game.RouteInformation.GradientMinTrack) /
						(Game.RouteInformation.GradientMaxTrack - Game.RouteInformation.GradientMinTrack);
				// draw a vertical bar at the current train position
				Renderer.DrawRectangle(null, new Point(xPos, gradientSize.Height / 2),
					new Size(gradientPosWidth, gradientSize.Height / 2), gradientPosBar);
				break;
			}
		}

		/// <summary>Returns whether the Route Information display is currently showing on screen</summary>
		public bool IsActive()
		{
			return currentState != state.none;
		}

		/********************
			PRIVATE METHODS
		*********************/
		//
		// SET STATE
		//
		/// <summary>Sets the state, intializing any required resource.</summary>
		/// <param name="newState">The new state to set to.</param>
		private void setState(state newState)
		{
			switch (newState)
			{
			case state.map:
				if (mapImage == null)
				{
					mapImage	= new Texture(Game.RouteInformation.RouteMap);
					mapSize		= Game.RouteInformation.RouteMap.Size;
				}
				break;
			case state.gradient:
				if (gradientImage == null)
				{
					gradientImage	= new Texture(Game.RouteInformation.GradientProfile);
					gradientSize	= Game.RouteInformation.GradientProfile.Size;
				}
				break;
			}
			currentState	= newState;
		}
	}
}
