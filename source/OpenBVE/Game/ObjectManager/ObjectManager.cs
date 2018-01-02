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
				AnimatedWorldObjects[i].Update(TimeElapsed, ForceUpdate);
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
