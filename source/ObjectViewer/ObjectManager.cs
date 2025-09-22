using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace ObjectViewer
{
    internal static class ObjectManager
    {
        internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
        internal static int AnimatedWorldObjectsUsed = 0;

        internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
        {
            for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				AbstractTrain train = null;
				bool visible = AnimatedWorldObjects[i].IsVisible(Program.Renderer.Camera.Alignment.Position, Program.CurrentRoute.CurrentBackground.BackgroundImageDistance, Program.Renderer.Camera.ExtraViewingDistance);
				if (visible | ForceUpdate)
				{
					train = Program.CurrentHost.ClosestTrain(AnimatedWorldObjects[i].Position);
				}
				AnimatedWorldObjects[i].Update(train, TimeElapsed, ForceUpdate, visible);
			}
        }
    }
}
