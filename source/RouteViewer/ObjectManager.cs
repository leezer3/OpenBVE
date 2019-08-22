using System;
using System.Text;
using LibRender;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using static LibRender.CameraProperties;

namespace OpenBve {
	internal static class ObjectManager {

		// static objects
		internal static StaticObject[] Objects = new StaticObject[16];
		internal static int ObjectsUsed;
		internal static int[] ObjectsSortedByStart = new int[] { };
		internal static int[] ObjectsSortedByEnd = new int[] { };
		internal static int ObjectsSortedByStartPointer = 0;
		internal static int ObjectsSortedByEndPointer = 0;
		internal static double LastUpdatedTrackPosition = 0.0;

		internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate) {
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

		// load object
		internal static UnifiedObject LoadObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				UnifiedObject Result;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
					case ".x":
					case ".obj":
					case ".animated":
					case ".l3dobj":
					case ".l3dgrp":
					case ".s":
						Program.CurrentHost.LoadObject(FileName, Encoding, out Result);
						break;
					default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}

				if (Result != null)
				{
					Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, false);
				}
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				StaticObject Result;
				UnifiedObject obj;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
					case ".x":
					case ".obj":
					case ".animated":
					case ".l3dobj":
					case ".l3dgrp":
					case ".s":
						Program.CurrentHost.LoadObject(FileName, Encoding, out obj);
						if (obj is AnimatedObjectCollection)
						{
							Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
							return null;
						}
						Result = (StaticObject)obj;
						break;
					default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}

				if (Result != null)
				{
					Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, false);
				}
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}

		// create object
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				a.CreateObject(Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, true);
			}
		}

		// create static object
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
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
			Objects[a].ApplyData(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, 600);
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
		
		// create dynamic object
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
		
		// finish creating objects
		internal static void FinishCreatingObjects() {
			Array.Resize<StaticObject>(ref Objects, ObjectsUsed);
			//Array.Resize(ref AnimatedWorldObjects, AnimatedWorldObjectsUsed);
		}

		// initialize visibility
		internal static void InitializeVisibility() {
			// sort objects
			ObjectsSortedByStart = new int[ObjectsUsed];
			ObjectsSortedByEnd = new int[ObjectsUsed];
			double[] a = new double[ObjectsUsed];
			double[] b = new double[ObjectsUsed];
			int n = 0;
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
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
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
					if (Objects[i].StartingDistance <= p + Camera.ForwardViewingDistance & Objects[i].EndingDistance >= p - Camera.BackwardViewingDistance) {
						Renderer.ShowObject(ObjectManager.Objects[i], ObjectType.Static);
					}
				}
			}
		}

		// update visibility
		internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged) {
			if (ViewingDistanceChanged) {
				UpdateVisibility(TrackPosition);
				UpdateVisibility(TrackPosition - 0.001);
				UpdateVisibility(TrackPosition + 0.001);
				UpdateVisibility(TrackPosition);
			} else {
				UpdateVisibility(TrackPosition);
			}
		}
		internal static void UpdateVisibility(double TrackPosition) {
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;
			if (d < 0.0) {
				if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
				if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (ObjectsSortedByStartPointer >= 0) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance > p + Camera.ForwardViewingDistance) {
						Renderer.HideObject(ref Objects[o]);
						ObjectsSortedByStartPointer--;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByEndPointer >= 0) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance >= p - Camera.BackwardViewingDistance) {
						if (Objects[o].StartingDistance <= p + Camera.ForwardViewingDistance) {
							Renderer.ShowObject(Objects[o], ObjectType.Static);
						}
						ObjectsSortedByEndPointer--;
					} else {
						break;
					}
				}
			} else if (d > 0.0) {
				if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
				if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
				// dispose
				while (ObjectsSortedByEndPointer < n) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance < p - Camera.BackwardViewingDistance) {
						Renderer.HideObject(ref ObjectManager.Objects[o]);
						ObjectsSortedByEndPointer++;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByStartPointer < n) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance <= p + Camera.ForwardViewingDistance) {
						if (Objects[o].EndingDistance >= p - Camera.BackwardViewingDistance) {
							Renderer.ShowObject(Objects[o], ObjectType.Static);
						}
						ObjectsSortedByStartPointer++;
					} else {
						break;
					}
				}
			}
			LastUpdatedTrackPosition = TrackPosition;
		}

	}
}
