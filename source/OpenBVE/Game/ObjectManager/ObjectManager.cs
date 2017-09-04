using System;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Holds all static objects currently in use within the game world</summary>
		internal static StaticObject[] Objects = new StaticObject[16];
		/// <summary>The total number of static objects used</summary>
		internal static int ObjectsUsed;
		/// <summary>An array containing the pointers to all objects within the object array, sorted by starting distance</summary>
		internal static int[] ObjectsSortedByStart = new int[] { };
		/// <summary>An array containing the pointers to all objects within the object array, sorted by ending distance</summary>
		internal static int[] ObjectsSortedByEnd = new int[] { };
		internal static int ObjectsSortedByStartPointer = 0;
		internal static int ObjectsSortedByEndPointer = 0;
		internal static double LastUpdatedTrackPosition = 0.0;
		/// <summary>Holds all animated objects currently in use within the game world</summary>
		internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
		/// <summary>The total number of animated objects used</summary>
		internal static int AnimatedWorldObjectsUsed = 0;

		/// <summary>Is called once a frame to update all animated objects</summary>
		/// <param name="TimeElapsed">The total frame time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update (e.g. camera change etc)</param>
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z - World.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z + World.BackgroundImageDistance + World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					if (AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate >= AnimatedWorldObjects[i].Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate + TimeElapsed;
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++)
						{
							if (TrainManager.Trains[j].State == TrainManager.TrainState.Available)
							{
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition)
								{
									distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
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
						if (AnimatedWorldObjects[i].FollowsTrack)
						{
							if (AnimatedWorldObjects[i].Visible)
							{
								//Calculate the distance travelled
								double delta = AnimatedWorldObjects[i].UpdateTrackFollowerScript(false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta);

								//Update the front and rear axle track followers
								TrackManager.UpdateTrackFollower(ref AnimatedWorldObjects[i].FrontAxleFollower, (AnimatedWorldObjects[i].TrackPosition + AnimatedWorldObjects[i].FrontAxlePosition) + delta, true, true);
								TrackManager.UpdateTrackFollower(ref AnimatedWorldObjects[i].RearAxleFollower, (AnimatedWorldObjects[i].TrackPosition + AnimatedWorldObjects[i].RearAxlePosition) + delta, true, true);
								//Update the base object position
								AnimatedWorldObjects[i].FrontAxleFollower.UpdateWorldCoordinates(false);
								AnimatedWorldObjects[i].RearAxleFollower.UpdateWorldCoordinates(false);
								AnimatedWorldObjects[i].UpdateTrackFollowingObject();

							}
							//Update the actual animated object- This must be done last in case the user has used Translation or Rotation
							AnimatedWorldObjects[i].Object.Update(false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].FrontAxleFollower.TrackPosition, AnimatedWorldObjects[i].FrontAxleFollower.WorldPosition, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta, true);

						}
						else
						{
							AnimatedWorldObjects[i].Object.Update(false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position,
								AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta, true);
						}

					}
					else
					{
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!AnimatedWorldObjects[i].Visible)
					{
						Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, Renderer.ObjectType.Dynamic);
						AnimatedWorldObjects[i].Visible = true;
					}
				}
				else
				{
					AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					if (AnimatedWorldObjects[i].Visible)
					{
						Renderer.HideObject(AnimatedWorldObjects[i].Object.ObjectIndex);
						AnimatedWorldObjects[i].Visible = false;
					}
				}
			}
		}

		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
		{
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
		{
			if (Prototype == null)
			{
				return -1;
			}
			int a = ObjectsUsed;
			if (a >= Objects.Length)
			{
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			Objects[a] = new StaticObject();
			Objects[a].ApplyData(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			for (int i = 0; i < Prototype.Mesh.Faces.Length; i++)
			{
				switch (Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask)
				{
					case World.MeshFace.FaceTypeTriangles:
						Game.InfoTotalTriangles++;
						break;
					case World.MeshFace.FaceTypeTriangleStrip:
						Game.InfoTotalTriangleStrip++;
						break;
					case World.MeshFace.FaceTypeQuads:
						Game.InfoTotalQuads++;
						break;
					case World.MeshFace.FaceTypeQuadStrip:
						Game.InfoTotalQuadStrip++;
						break;
					case World.MeshFace.FaceTypePolygon:
						Game.InfoTotalPolygon++;
						break;
				}
			}
			ObjectsUsed++;
			return a;
		}

		internal static int CreateDynamicObject()
		{
			int a = ObjectsUsed;
			if (a >= Objects.Length)
			{
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			Objects[a] = new StaticObject
			{
				Mesh =
				{
					Faces = new World.MeshFace[] {},
					Materials = new World.MeshMaterial[] {},
					Vertices = new World.Vertex[] {}
				},
				Dynamic = true
			};
			ObjectsUsed++;
			return a;
		}

		private static double Mod(double a, double b)
		{
			return a - b * Math.Floor(a / b);
		}
	}
}
