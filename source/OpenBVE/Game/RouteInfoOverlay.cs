using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveApi.Math;

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
		private state currentState	= state.none;
		private Texture gradientImage;
		private Vector2	gradientSize;
		private Texture mapImage;
		private Vector2 mapSize;

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
			double xPos, zPos;
			// size the image to half of the smallest screen size, but not larger than default size
			// NO: compressing the image below its original size makes texs hard to read
//			int		width		= Math.Min(Math.Min(Screen.Height, Screen.Width) / 2,
//						Game.RouteInformation.DefaultRouteInfoSize);
			Vector2	origin = Vector2.Null;
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			// draw the relevant image
			switch (currentState)
			{
			case state.map:
				Program.Renderer.Rectangle.Draw(mapImage, origin, mapSize);
				// get current train position
				int n = Program.TrainManager.Trains.Length;
				for (int i = 0; i < n; i++)
				{
					int trainX = (int)Program.TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.X;
					int trainZ = (int)Program.TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.Z;
					// convert to route map coordinates
					xPos = mapSize.X * (trainX - Program.CurrentRoute.Information.RouteMinX) /
							(Program.CurrentRoute.Information.RouteMaxX - Program.CurrentRoute.Information.RouteMinX) - trainDotRadius;
					zPos = mapSize.Y - mapSize.Y * (trainZ - Program.CurrentRoute.Information.RouteMinZ) /
							(Program.CurrentRoute.Information.RouteMaxZ - Program.CurrentRoute.Information.RouteMinZ) - trainDotRadius;
						// draw a dot at current train position
						Program.Renderer.Rectangle.Draw(null, new Vector2(xPos, zPos),
							new Vector2(trainDotDiameter, trainDotDiameter),
							Program.TrainManager.Trains[i].IsPlayerTrain ? playerTrainDotColour : trainDotColour);
				}
				break;
			case state.gradient:
				Program.Renderer.Rectangle.Draw(gradientImage, origin, gradientSize);
				// get current train position in track
				int trackPos	= (int)(TrainManager.PlayerTrain.FrontCarTrackPosition);
				// convert to gradient profile offset
				xPos = gradientSize.Y * (trackPos - Program.CurrentRoute.Information.GradientMinTrack) /
						(Program.CurrentRoute.Information.GradientMaxTrack - Program.CurrentRoute.Information.GradientMinTrack);
				// draw a vertical bar at the current train position
				Program.Renderer.Rectangle.Draw(null, new Vector2(xPos, gradientSize.Y / 2),
					new Vector2(gradientPosWidth, gradientSize.Y / 2), gradientPosBar);
				break;
			}
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
					mapImage	= new Texture(Program.CurrentRoute.Information.RouteMap);
					mapSize		= new Vector2(Program.CurrentRoute.Information.RouteMap.Width, Program.CurrentRoute.Information.RouteMap.Height);
				}
				break;
			case state.gradient:
				if (gradientImage == null)
				{
					gradientImage	= new Texture(Program.CurrentRoute.Information.GradientProfile);
					gradientSize	= new Vector2(Program.CurrentRoute.Information.GradientProfile.Width, Program.CurrentRoute.Information.GradientProfile.Height);
				}
				break;
			}
			currentState	= newState;
		}
	}
}
