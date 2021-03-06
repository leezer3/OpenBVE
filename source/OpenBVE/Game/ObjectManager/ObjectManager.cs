using OpenBveApi.Objects;
using OpenBveApi.Trains;
using TrainManager.Trains;
using TrackFollowingObject = OpenBveApi.Objects.TrackFollowingObject;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static class ObjectManager
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
				AbstractTrain train = null;
				const double extraRadius = 10.0;
				double z = 0.0;
				if(AnimatedWorldObjects[i].Object != null)
				{
					//Standalone sound may not have an object file attached
					z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				}
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = Program.Renderer.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z - Program.CurrentRoute.CurrentBackground.BackgroundImageDistance - Program.Renderer.Camera.ExtraViewingDistance;
				double tb = Program.Renderer.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z + Program.CurrentRoute.CurrentBackground.BackgroundImageDistance + Program.Renderer.Camera.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					//Find the closest train
					train = Program.CurrentHost.ClosestTrain(AnimatedWorldObjects[i].RelativeTrackPosition);
				}
				AnimatedWorldObjects[i].Update(train, TimeElapsed, ForceUpdate, visible);
			}
		}

		public static void ProcessJump(AbstractTrain Train)
		{
			if (Train.IsPlayerTrain)
			{
				for (int i = 0; i < AnimatedWorldObjects.Length; i++)
				{
					var obj = AnimatedWorldObjects[i] as TrackFollowingObject;
					if (obj != null)
					{
						//Track followers should be reset if we jump between stations
						obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.FrontAxlePosition;
						obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.RearAxlePosition;
						obj.FrontAxleFollower.UpdateWorldCoordinates(false);
						obj.RearAxleFollower.UpdateWorldCoordinates(false);
					}

				}
			}
			UpdateAnimatedWorldObjects(0.0,true);
		}
	}
}
