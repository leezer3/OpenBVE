using System;
using OpenBveApi.Objects;
using OpenBveShared;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Called once at the start of the simulation to setup the visibility for all objects</summary>
		internal static void InitializeVisibility()
		{
			// sort objects
			GameObjectManager.ObjectsSortedByStart = new int[GameObjectManager.ObjectsUsed];
			GameObjectManager.ObjectsSortedByEnd = new int[GameObjectManager.ObjectsUsed];
			double[] a = new double[GameObjectManager.ObjectsUsed];
			double[] b = new double[GameObjectManager.ObjectsUsed];
			int n = 0;
			for (int i = 0; i < GameObjectManager.ObjectsUsed; i++)
			{
				if (!GameObjectManager.Objects[i].Dynamic)
				{
					GameObjectManager.ObjectsSortedByStart[n] = i;
					GameObjectManager.ObjectsSortedByEnd[n] = i;
					a[n] = GameObjectManager.Objects[i].StartingDistance;
					b[n] = GameObjectManager.Objects[i].EndingDistance;
					n++;
				}
			}
			Array.Resize<int>(ref GameObjectManager.ObjectsSortedByStart, n);
			Array.Resize<int>(ref GameObjectManager.ObjectsSortedByEnd, n);
			Array.Resize<double>(ref a, n);
			Array.Resize<double>(ref b, n);
			Array.Sort<double, int>(a, GameObjectManager.ObjectsSortedByStart);
			Array.Sort<double, int>(b, GameObjectManager.ObjectsSortedByEnd);
			GameObjectManager.ObjectsSortedByStartPointer = 0;
			GameObjectManager.ObjectsSortedByEndPointer = 0;
			// initial visiblity
			double p = World.CameraTrackFollower.TrackPosition + Camera.CameraCurrentAlignment.Position.Z;
			for (int i = 0; i < GameObjectManager.ObjectsUsed; i++)
			{
				if (!GameObjectManager.Objects[i].Dynamic)
				{
					if (GameObjectManager.Objects[i].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance & GameObjectManager.Objects[i].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance)
					{
						OpenBveShared.Renderer.ShowObject(i, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
					}
				}
			}
		}

		/// <summary>Is called when the primary camera moves to update the visibility</summary>
		/// <param name="TrackPosition">The camera's track position</param>
		/// <param name="ViewingDistanceChanged">Whether the viewing distance has changed (e.g. due to camera type etc)</param>
		internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged)
		{
			if (ViewingDistanceChanged)
			{
				UpdateVisibility(TrackPosition);
				UpdateVisibility(TrackPosition - 0.001);
				UpdateVisibility(TrackPosition + 0.001);
				UpdateVisibility(TrackPosition);
			}
			else
			{
				UpdateVisibility(TrackPosition);
			}
		}
		/// <summary>Is called to update the visibility of the camera</summary>
		/// <param name="TrackPosition">The camera's track position</param>
		internal static void UpdateVisibility(double TrackPosition)
		{
			double d = TrackPosition - GameObjectManager.LastUpdatedTrackPosition;
			int n = GameObjectManager.ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + Camera.CameraCurrentAlignment.Position.Z;
			if (d < 0.0)
			{
				if (GameObjectManager.ObjectsSortedByStartPointer >= n) GameObjectManager.ObjectsSortedByStartPointer = n - 1;
				if (GameObjectManager.ObjectsSortedByEndPointer >= n) GameObjectManager.ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (GameObjectManager.ObjectsSortedByStartPointer >= 0)
				{
					int o = GameObjectManager.ObjectsSortedByStart[GameObjectManager.ObjectsSortedByStartPointer];
					if (GameObjectManager.Objects[o].StartingDistance > p + OpenBveShared.World.ForwardViewingDistance)
					{
						OpenBveShared.Renderer.HideObject(o);
						GameObjectManager.ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (GameObjectManager.ObjectsSortedByEndPointer >= 0)
				{
					int o = GameObjectManager.ObjectsSortedByEnd[GameObjectManager.ObjectsSortedByEndPointer];
					if (GameObjectManager.Objects[o].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance)
					{
						if (GameObjectManager.Objects[o].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance)
						{
							OpenBveShared.Renderer.ShowObject(o, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
						}

						GameObjectManager.ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (GameObjectManager.ObjectsSortedByStartPointer < 0) GameObjectManager.ObjectsSortedByStartPointer = 0;
				if (GameObjectManager.ObjectsSortedByEndPointer < 0) GameObjectManager.ObjectsSortedByEndPointer = 0;
				// dispose
				while (GameObjectManager.ObjectsSortedByEndPointer < n)
				{
					int o = GameObjectManager.ObjectsSortedByEnd[GameObjectManager.ObjectsSortedByEndPointer];
					if (GameObjectManager.Objects[o].EndingDistance < p - OpenBveShared.World.BackwardViewingDistance)
					{
						OpenBveShared.Renderer.HideObject(o);
						GameObjectManager.ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (GameObjectManager.ObjectsSortedByStartPointer < n)
				{
					int o = GameObjectManager.ObjectsSortedByStart[GameObjectManager.ObjectsSortedByStartPointer];
					if (GameObjectManager.Objects[o].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance)
					{
						if (GameObjectManager.Objects[o].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance)
						{
							OpenBveShared.Renderer.ShowObject(o, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
						}

						GameObjectManager.ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}

			GameObjectManager.LastUpdatedTrackPosition = TrackPosition;
		}
	}
}
