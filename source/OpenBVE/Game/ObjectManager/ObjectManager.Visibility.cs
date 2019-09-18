using System;
using static LibRender.CameraProperties;
using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Called once at the start of the simulation to setup the visibility for all objects</summary>
		internal static void InitializeVisibility()
		{
			// sort objects
			ObjectsSortedByStart = new int[ObjectsUsed];
			ObjectsSortedByEnd = new int[ObjectsUsed];
			double[] a = new double[ObjectsUsed];
			double[] b = new double[ObjectsUsed];
			int n = 0;
			for (int i = 0; i < ObjectsUsed; i++)
			{
				if (!Objects[i].Dynamic)
				{
					ObjectsSortedByStart[n] = i;
					ObjectsSortedByEnd[n] = i;
					a[n] = Objects[i].StartingDistance;
					b[n] = Objects[i].EndingDistance;
					n++;
				}
			}
			Array.Resize<int>(ref ObjectsSortedByStart, n);
			Array.Resize<int>(ref ObjectsSortedByEnd, n);
			Array.Resize<double>(ref a, n);
			Array.Resize<double>(ref b, n);
			Array.Sort<double, int>(a, ObjectsSortedByStart);
			Array.Sort<double, int>(b, ObjectsSortedByEnd);
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;
			// initial visiblity
			double p = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;
			for (int i = 0; i < ObjectsUsed; i++)
			{
				if (!Objects[i].Dynamic)
				{
					if (Objects[i].StartingDistance <= p + Camera.ForwardViewingDistance & Objects[i].EndingDistance >= p - Camera.BackwardViewingDistance)
					{
						LibRender.Renderer.ShowObject(Objects[i], ObjectType.Static);
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
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;
			if (d < 0.0)
			{
				if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
				if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (ObjectsSortedByStartPointer >= 0)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance > p + Camera.ForwardViewingDistance)
					{
						LibRender.Renderer.HideObject(ref ObjectManager.Objects[o]);
						ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (ObjectsSortedByEndPointer >= 0)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance >= p - Camera.BackwardViewingDistance)
					{
						if (Objects[o].StartingDistance <= p + Camera.ForwardViewingDistance)
						{
							LibRender.Renderer.ShowObject(Objects[o], ObjectType.Static);
						}
						ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
				if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
				// dispose
				while (ObjectsSortedByEndPointer < n)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance < p - Camera.BackwardViewingDistance)
					{
						LibRender.Renderer.HideObject(ref ObjectManager.Objects[o]);
						ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (ObjectsSortedByStartPointer < n)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance <= p + Camera.ForwardViewingDistance)
					{
						if (Objects[o].EndingDistance >= p - Camera.BackwardViewingDistance)
						{
							LibRender.Renderer.ShowObject(Objects[o], ObjectType.Static);
						}
						ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}
			LastUpdatedTrackPosition = TrackPosition;
		}
	}
}
