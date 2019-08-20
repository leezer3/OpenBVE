using System;
using LibRender;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using static LibRender.CameraProperties;

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
				TrainManager.Train train = null;
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z - Backgrounds.BackgroundImageDistance - Camera.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z + Backgrounds.BackgroundImageDistance + Camera.ExtraViewingDistance;
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

		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
		{
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}

		internal static int CreateStaticObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			StaticObject obj = (StaticObject) Prototype;
			if (obj == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Attempted to use an animated object where only static objects are allowed.");
				return -1;
			}
			return CreateStaticObject(obj, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
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
			Objects[a] = new StaticObject(Program.CurrentHost);
			Objects[a].ApplyData(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, Interface.CurrentOptions.ViewingDistance);
			for (int i = 0; i < Prototype.Mesh.Faces.Length; i++)
			{
				switch (Prototype.Mesh.Faces[i].Flags & MeshFace.FaceTypeMask)
				{
					case MeshFace.FaceTypeTriangles:
						LibRender.Renderer.InfoTotalTriangles++;
						break;
					case MeshFace.FaceTypeTriangleStrip:
						LibRender.Renderer.InfoTotalTriangleStrip++;
						break;
					case MeshFace.FaceTypeQuads:
						LibRender.Renderer.InfoTotalQuads++;
						break;
					case MeshFace.FaceTypeQuadStrip:
						LibRender.Renderer.InfoTotalQuadStrip++;
						break;
					case MeshFace.FaceTypePolygon:
						LibRender.Renderer.InfoTotalPolygon++;
						break;
				}
			}

			Objects[a].ObjectIndex = a;
			ObjectsUsed++;
			return a;
		}


		internal static void CreateDynamicObject(ref StaticObject internalObject)
		{
			int a = ObjectsUsed;
			if (a >= Objects.Length)
			{
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}

			if (internalObject == null)
			{
				
				Objects[a] = new StaticObject(Program.CurrentHost)
				{
					Dynamic = true,
					ObjectIndex = a
				};
				internalObject = Objects[a];
				ObjectsUsed++;
				return;
			}
			else
			{
				Objects[a] = internalObject;
				internalObject.Dynamic = true;
				internalObject.ObjectIndex = a;
				ObjectsUsed++;
			}
		}
	}
}
