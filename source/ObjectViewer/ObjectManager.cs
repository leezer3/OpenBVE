using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve
{
    internal static class ObjectManager
    {
        internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
        internal static int AnimatedWorldObjectsUsed = 0;

        internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
        {
            for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				TrainManager.Train train = null;
				const double extraRadius = 10.0;
				double z = 0.0;
				if (AnimatedWorldObjects[i].Object != null)
				{
					//Standalone sound may not have an object file attached
					z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				}
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = Program.Renderer.Camera.Alignment.Position.Z - Program.CurrentRoute.CurrentBackground.BackgroundImageDistance - Program.Renderer.Camera.ExtraViewingDistance;
				double tb = Program.Renderer.Camera.Alignment.Position.Z + Program.CurrentRoute.CurrentBackground.BackgroundImageDistance + Program.Renderer.Camera.ExtraViewingDistance;
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
							if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition)
							{
								distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].TrackPosition;
							}
							else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > AnimatedWorldObjects[i].TrackPosition)
							{
								distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - AnimatedWorldObjects[i].TrackPosition;
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
