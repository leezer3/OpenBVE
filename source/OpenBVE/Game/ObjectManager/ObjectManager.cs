using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Holds all animated objects currently in use within the game world</summary>
		internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
		/// <summary>The total number of animated objects used</summary>
		internal static int AnimatedWorldObjectsUsed = 0;

		/// <summary>Is called once a frame to update all animated objects</summary>
		/// <param name="TimeElapsed">The total frame time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update (e.g. camera change etc)</param>
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				TrainManager.Train train = null;
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z - BackgroundHandle.BackgroundImageDistance - Program.Renderer.Camera.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z + BackgroundHandle.BackgroundImageDistance + Program.Renderer.Camera.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					//Find the closest train
					double trainDistance = double.MaxValue;
					for (int j = 0; j < TrainManager.Trains.Length; j++)
					{
						if (TrainManager.Trains[j].State == TrainState.Available)
						{
							double distance;
							if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].RelativeTrackPosition)
							{
								distance = AnimatedWorldObjects[i].RelativeTrackPosition - TrainManager.Trains[j].Cars[0].TrackPosition;
							}
							else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > AnimatedWorldObjects[i].RelativeTrackPosition)
							{
								distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - AnimatedWorldObjects[i].RelativeTrackPosition;
							}
							else
							{
								distance = 0;
							}
							if (distance < trainDistance)
							{
								train = TrainManager.Trains[j];
								trainDistance = distance;
							}
						}
					}
				}
				AnimatedWorldObjects[i].Update(train, TimeElapsed, ForceUpdate, visible);
			}
		}
	}
}
