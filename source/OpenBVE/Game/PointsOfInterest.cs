using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Defines a point of interest within the game world</summary>
		internal struct PointOfInterest
		{
			/// <summary>The track position</summary>
			internal double TrackPosition;
			/// <summary>The offset from Track 0's position</summary>
			internal Vector3 TrackOffset;
			/// <summary>The yaw</summary>
			internal double TrackYaw;
			/// <summary>The pitch</summary>
			internal double TrackPitch;
			/// <summary>The roll</summary>
			internal double TrackRoll;
			/// <summary>The textual message to be displayed when jumping to this point</summary>
			internal string Text;
		}

		/// <summary>Holds all points of interest within the game world</summary>
		internal static PointOfInterest[] PointsOfInterest = new PointOfInterest[] { };

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
					for (int i = 0; i < PointsOfInterest.Length; i++)
					{
						if (PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition)
						{
							if (PointsOfInterest[i].TrackPosition > t)
							{
								t = PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
				else if (Value > 0)
				{
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < PointsOfInterest.Length; i++)
					{
						if (PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition)
						{
							if (PointsOfInterest[i].TrackPosition < t)
							{
								t = PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			}
			else
			{
				// absolute
				j = Value >= 0 & Value < PointsOfInterest.Length ? Value : -1;
			}
			// process poi
			if (j < 0) return false;
			World.CameraTrackFollower.Update(t, true, false);
			World.CameraCurrentAlignment.Position = PointsOfInterest[j].TrackOffset;
			World.CameraCurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
			World.CameraCurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
			World.CameraCurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
			World.CameraCurrentAlignment.TrackPosition = t;
			World.UpdateAbsoluteCamera(0.0);
			if (PointsOfInterest[j].Text != null)
			{
				double n = 3.0 + 0.5 * Math.Sqrt((double)PointsOfInterest[j].Text.Length);
				Game.AddMessage(PointsOfInterest[j].Text, MessageManager.MessageDependency.PointOfInterest, Interface.GameMode.Expert, MessageColor.White, Game.SecondsSinceMidnight + n, null);
			}
			return true;
		}
	}
}
