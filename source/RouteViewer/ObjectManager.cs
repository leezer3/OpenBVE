using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace RouteViewer {
	internal static class ObjectManager {

		internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		
		internal static void UpdateAnimatedWorldObjects(double timeElapsed, bool forceUpdate) {
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				AbstractTrain train = null;
				Vector3 cameraPos = Program.Renderer.Camera.Alignment.Position;
				cameraPos.Z += Program.Renderer.CameraTrackFollower.TrackPosition;
				bool visible = AnimatedWorldObjects[i].IsVisible(cameraPos, Program.CurrentRoute.CurrentBackground.BackgroundImageDistance, Program.Renderer.Camera.ExtraViewingDistance);
				if (visible | forceUpdate)
				{
					train = Program.CurrentHost.ClosestTrain(AnimatedWorldObjects[i].Position);
				}
				AnimatedWorldObjects[i].Update(train, timeElapsed, forceUpdate, visible);
			}
		}
	}
}
