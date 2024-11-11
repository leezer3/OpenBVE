﻿using System;
using OpenBveApi;
using OpenBveApi.Colors;
using RouteManager2.MessageManager;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Moves the camera to a point of interest</summary>
		/// <param name="newValue">The value of the jump to perform:
		/// -1= Previous POI
		///  0= Return to currently selected POI (From cab etc.)
		///  1= Next POI</param>
		/// <param name="newValueIsRelative">Whether the relative camera position should be retained</param>
		/// <returns>False if the previous / next POI would be outside those defined, true otherwise</returns>
		internal static bool ApplyPointOfInterest(int newValue, bool newValueIsRelative)
		{
			double t = 0.0;
			int j = -1;
			if (newValueIsRelative)
			{
				// relative
				if (newValue < 0)
				{
					// previous poi
					t = double.NegativeInfinity;
					for (int i = 0; i < Program.CurrentRoute.PointsOfInterest.Length; i++)
					{
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition < Program.Renderer.CameraTrackFollower.TrackPosition)
						{
							if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition > t)
							{
								t = Program.CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
				else if (newValue > 0)
				{
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < Program.CurrentRoute.PointsOfInterest.Length; i++)
					{
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition > Program.Renderer.CameraTrackFollower.TrackPosition)
						{
							if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition < t)
							{
								t = Program.CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			}
			else
			{
				// absolute
				j = newValue >= 0 & newValue < Program.CurrentRoute.PointsOfInterest.Length ? newValue : -1;
			}
			// process poi
			if (j < 0) return false;
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(t, true, false);
			Program.Renderer.Camera.Alignment.Position = Program.CurrentRoute.PointsOfInterest[j].TrackOffset;
			Program.Renderer.Camera.Alignment.Yaw = Program.CurrentRoute.PointsOfInterest[j].TrackYaw;
			Program.Renderer.Camera.Alignment.Pitch = Program.CurrentRoute.PointsOfInterest[j].TrackPitch;
			Program.Renderer.Camera.Alignment.Roll = Program.CurrentRoute.PointsOfInterest[j].TrackRoll;
			Program.Renderer.Camera.Alignment.TrackPosition = t;
			World.UpdateAbsoluteCamera();
			if (Program.CurrentRoute.PointsOfInterest[j].Text != null)
			{
				double n = 3.0 + 0.5 * Math.Sqrt(Program.CurrentRoute.PointsOfInterest[j].Text.Length);
				MessageManager.AddMessage(Program.CurrentRoute.PointsOfInterest[j].Text, MessageDependency.PointOfInterest, GameMode.Expert, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + n, null);
			}
			return true;
		}
	}
}
