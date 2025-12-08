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
		private enum OverlayState
		{
			None = 0,
			Map,
			Gradient,
			NumOf
		}

		/// <summary>The width of the gradient position bar in pixels</summary>
		private const int gradientPosWidth= 2;
		/// <summary>The color of the gradient position bar</summary>
		private static readonly Color128 gradientPosBar	= Color128.White;
		/// <summary>The radius of a train dot in pixels</summary>
		private const int trainDotRadius = 4;
		/// <summary>The diameter of a train dot in pixels</summary>
		private const int trainDotDiameter = trainDotRadius * 2;
		/// <summary>The color used to render dots for non-player (AI) trains</summary>
		private static readonly Color128 trainDotColour	= Color128.Red;
		/// <summary>The color used to render the dot for the player train</summary>
		private static readonly Color128 playerTrainDotColour = Color128.Green;

		private OverlayState currentState	= OverlayState.None;
		private Texture gradientImage;
		private Vector2	gradientSize;
		private Texture mapImage;
		private Vector2 mapSize;

		/// <summary>Processes commands.</summary>
		/// <returns><c>true</c>, if command was processed, <c>false</c> otherwise.</returns>
		/// <param name="command">The Translations.Command command to process.</param>
		internal bool ProcessCommand(Translations.Command command)
		{
			if (command != Translations.Command.RouteInformation)	// only accept RouteInformation command
				return false;
			// cycle through available state
			SetState( (OverlayState)((int)(currentState + 1) % (int)OverlayState.NumOf) );
			return true;
		}

		/// <summary>Displays the current state into the simulation window.</summary>
		public void Show()
		{
			if (currentState == OverlayState.None)
				return;
			Vector2 Pos = Vector2.Null;
			Vector2	origin = Vector2.Null;
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			// draw the relevant image
			switch (currentState)
			{
			case OverlayState.Map:
				Program.Renderer.Rectangle.Draw(mapImage, origin, mapSize);
					// get current train position
				for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
				{
					int trainX = (int)Program.TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.X;
					int trainZ = (int)Program.TrainManager.Trains[i].Cars[0].FrontAxle.Follower.WorldPosition.Z;
					// convert to route map coordinates
					Pos.X = mapSize.X * (trainX - Program.CurrentRoute.Information.RouteMinX) /
							(Program.CurrentRoute.Information.RouteMaxX - Program.CurrentRoute.Information.RouteMinX) - trainDotRadius;
					Pos.Y = mapSize.Y - mapSize.Y * (trainZ - Program.CurrentRoute.Information.RouteMinZ) /
							(Program.CurrentRoute.Information.RouteMaxZ - Program.CurrentRoute.Information.RouteMinZ) - trainDotRadius;
						// draw a dot at current train position
						Program.Renderer.Rectangle.Draw(null, Pos,
							new Vector2(trainDotDiameter, trainDotDiameter),
							Program.TrainManager.Trains[i].IsPlayerTrain ? playerTrainDotColour : trainDotColour);
				}
				break;
			case OverlayState.Gradient:
				Program.Renderer.Rectangle.Draw(gradientImage, origin, gradientSize);
				// get current train position in track
				int trackPos = (int)(TrainManager.PlayerTrain.FrontCarTrackPosition);
				// convert to gradient profile offset
				Pos.X = gradientSize.Y * (trackPos - Program.CurrentRoute.Information.GradientMinTrack) /
						(Program.CurrentRoute.Information.GradientMaxTrack - Program.CurrentRoute.Information.GradientMinTrack);
				// draw a vertical bar at the current train position
				Program.Renderer.Rectangle.Draw(null, new Vector2(Pos.X, gradientSize.Y / 2),
					new Vector2(gradientPosWidth, gradientSize.Y / 2), gradientPosBar);
				break;
			}
		}

		/// <summary>Sets the state, intializing any required resource.</summary>
		/// <param name="newState">The new state to set to.</param>
		private void SetState(OverlayState newState)
		{
			switch (newState)
			{
			case OverlayState.Map:
				if (mapImage == null)
				{
					mapImage = new Texture(Program.CurrentRoute.Information.RouteMap);
					mapSize	= new Vector2(Program.CurrentRoute.Information.RouteMap.Width, Program.CurrentRoute.Information.RouteMap.Height);
				}
				break;
			case OverlayState.Gradient:
				if (gradientImage == null)
				{
					gradientImage = new Texture(Program.CurrentRoute.Information.GradientProfile);
					gradientSize = new Vector2(Program.CurrentRoute.Information.GradientProfile.Width, Program.CurrentRoute.Information.GradientProfile.Height);
				}
				break;
			}
			currentState	= newState;
		}
	}
}
