using System;
using LibRender;
using OpenBve.RouteManager;
using OpenBveApi;
using OpenBveApi.Colors;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Moves the camera to a point of interest</summary>
		/// <param name="Value">The value of the jump to perform:
		/// -1= Previous POI
		///  0= Return to currently selected POI (From cab etc.)
		///  1= Next POI</param>
		/// <param name="Relative">Whether the relative camera position should be retained</param>
		/// <returns>False if the previous / next POI would be outside those defined, true otherwise</returns>
		internal static bool ApplyPointOfInterest(int Value, bool Relative)
		{
			double t = 0.0;
			int j = -1;
			if (Relative)
			{
				// relative
				if (Value < 0)
				{
					// previous poi
					t = double.NegativeInfinity;
					for (int i = 0; i < CurrentRoute.PointsOfInterest.Length; i++)
					{
						if (CurrentRoute.PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition)
						{
							if (CurrentRoute.PointsOfInterest[i].TrackPosition > t)
							{
								t = CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
				else if (Value > 0)
				{
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < CurrentRoute.PointsOfInterest.Length; i++)
					{
						if (CurrentRoute.PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition)
						{
							if (CurrentRoute.PointsOfInterest[i].TrackPosition < t)
							{
								t = CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			}
			else
			{
				// absolute
				j = Value >= 0 & Value < CurrentRoute.PointsOfInterest.Length ? Value : -1;
			}
			// process poi
			if (j < 0) return false;
			World.CameraTrackFollower.UpdateAbsolute(t, true, false);
			Camera.CurrentAlignment.Position = CurrentRoute.PointsOfInterest[j].TrackOffset;
			Camera.CurrentAlignment.Yaw = CurrentRoute.PointsOfInterest[j].TrackYaw;
			Camera.CurrentAlignment.Pitch = CurrentRoute.PointsOfInterest[j].TrackPitch;
			Camera.CurrentAlignment.Roll = CurrentRoute.PointsOfInterest[j].TrackRoll;
			Camera.CurrentAlignment.TrackPosition = t;
			World.UpdateAbsoluteCamera(0.0);
			if (CurrentRoute.PointsOfInterest[j].Text != null)
			{
				double n = 3.0 + 0.5 * Math.Sqrt((double) CurrentRoute.PointsOfInterest[j].Text.Length);
				Game.AddMessage(CurrentRoute.PointsOfInterest[j].Text, MessageDependency.PointOfInterest, GameMode.Expert, MessageColor.White, Game.SecondsSinceMidnight + n, null);
			}
			return true;
		}
	}
}
