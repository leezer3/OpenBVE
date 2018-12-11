using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace ObjectManager
{
	public static class GameObjectManager
	{
		/// <summary>Holds all static objects currently in use within the game world</summary>
		public static StaticObject[] Objects = new StaticObject[16];
		/// <summary>The total number of static objects used</summary>
		public static int ObjectsUsed;
		/// <summary>Holds all animated objects currently in use within the game world</summary>
		public static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
		/// <summary>The total number of animated objects used</summary>
		public static int AnimatedWorldObjectsUsed = 0;
		/// <summary>An array containing the pointers to all objects within the object array, sorted by starting distance</summary>
		public static int[] ObjectsSortedByStart = new int[] { };
		/// <summary>An array containing the pointers to all objects within the object array, sorted by ending distance</summary>
		public static int[] ObjectsSortedByEnd = new int[] { };
		public static int ObjectsSortedByStartPointer = 0;
		public static int ObjectsSortedByEndPointer = 0;
		public static double LastUpdatedTrackPosition = 0.0;

		public static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, HostInterface CurrentHost, double ViewingDistance)
		{
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false, CurrentHost, ViewingDistance);
		}
		public static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials, HostInterface CurrentHost, double ViewingDistance)
		{
			if (Prototype == null)
			{
				return -1;
			}
			int a = GameObjectManager.ObjectsUsed;
			if (a >= GameObjectManager.Objects.Length)
			{
				Array.Resize<StaticObject>(ref GameObjectManager.Objects, GameObjectManager.Objects.Length << 1);
			}
			GameObjectManager.Objects[a] = new StaticObject(CurrentHost);
			GameObjectManager.Objects[a].ApplyData(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials, ViewingDistance);
			GameObjectManager.ObjectsUsed++;
			return a;
		}

		public static int CreateDynamicObject(HostInterface CurrentHost)
		{
			int a = GameObjectManager.ObjectsUsed;
			if (a >= GameObjectManager.Objects.Length)
			{
				Array.Resize<StaticObject>(ref GameObjectManager.Objects, GameObjectManager.Objects.Length << 1);
			}
			GameObjectManager.Objects[a] = new StaticObject(CurrentHost)
			{
				Mesh =
				{
					Faces = new MeshFace[] {},
					Materials = new MeshMaterial[] {},
					Vertices = new VertexTemplate[] {}
				},
				Dynamic = true
			};
			GameObjectManager.ObjectsUsed++;
			return a;
		}

		/// <summary>Is called once a frame to update all animated objects</summary>
		/// <param name="TimeElapsed">The total frame time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update (e.g. camera change etc)</param>
		public static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				AnimatedWorldObjects[i].Update(TimeElapsed, ForceUpdate);
			}
		}
	}
}
