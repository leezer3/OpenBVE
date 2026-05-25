using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibRender2.Objects;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;

namespace LibRender2.Managers
{
	/// <summary>
	/// Manages the objects within the scene, including visibility and dynamic object updates.
	/// </summary>
	public class SceneManager
	{
		private readonly BaseRenderer renderer;

		/// <summary>The list of static object states in the scene.</summary>
		public List<ObjectState> StaticObjectStates = new List<ObjectState>();

		/// <summary>The list of dynamic object states in the scene.</summary>
		public List<ObjectState> DynamicObjectStates = new List<ObjectState>();

		/// <summary>The library of currently visible objects.</summary>
		public VisibleObjectLibrary VisibleObjects;

		public int[] ObjectsSortedByStart;
		public int[] ObjectsSortedByEnd;
		public int ObjectsSortedByStartPointer;
		public int ObjectsSortedByEndPointer;
		public double LastUpdatedTrackPosition;

		/// <summary>Whether the visibility update thread should continue running.</summary>
		public bool VisibilityThreadShouldRun = true;

		/// <summary>The lock to be held whilst visibility updates or loading operations are in progress</summary>
		public readonly object VisibilityUpdateLock = new object();

		private readonly object LockObject = new object();

		private VisibilityUpdate updateVisibility;
		private Thread visibilityThread;

		public void ShowObject(ObjectState Object, ObjectType Type)
		{
			VisibleObjects.ShowObject(Object, Type);
		}

		public void HideObject(ObjectState Object)
		{
			VisibleObjects.HideObject(Object);
		}

		/// <summary>A queue of jobs to be executed on the render thread (often related to object creation/disposal)</summary>
		public readonly ConcurrentQueue<ThreadStart> RenderThreadJobs;

		public SceneManager(BaseRenderer renderer)
		{
			this.renderer = renderer;
			StaticObjectStates = new List<ObjectState>();
			DynamicObjectStates = new List<ObjectState>();
			VisibleObjects = new VisibleObjectLibrary(renderer);
			RenderThreadJobs = new ConcurrentQueue<ThreadStart>();

			visibilityThread = new Thread(RunVisibilityThread)
			{
				IsBackground = true,
				Name = "VisibilityThread"
			};
			visibilityThread.Start();
		}

		/// <summary>
		/// Deinitializes the scene manager and stops the visibility thread.
		/// </summary>
		public void DeInitialize()
		{
			VisibilityThreadShouldRun = false;
		}

		private void RunVisibilityThread()
		{
			while (VisibilityThreadShouldRun)
			{
				lock (VisibilityUpdateLock)
				{
					if (updateVisibility != VisibilityUpdate.None && renderer.CameraTrackFollower != null)
					{
						UpdateVisibility(renderer.CameraTrackFollower.TrackPosition + renderer.Camera.Alignment.Position.Z);
					}
				}

				if (updateVisibility == VisibilityUpdate.None)
				{
					Thread.Sleep(100);
				}
			}
		}

		public void UpdateVisibility(bool force)
		{
			updateVisibility = force ? VisibilityUpdate.Force : VisibilityUpdate.Normal;
		}

		private void UpdateVisibility(double trackPosition)
		{
			if (renderer.currentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree)
			{
				UpdateQuadTreeVisibility();
			}
			else
			{
				if (updateVisibility == VisibilityUpdate.Normal)
				{
					UpdateLegacyVisibility(trackPosition);
				}
				else
				{
					UpdateLegacyVisibility(trackPosition + 0.01);
					UpdateLegacyVisibility(trackPosition - 0.01);
				}
			}

			updateVisibility = VisibilityUpdate.None;
		}

		private void UpdateQuadTreeVisibility()
		{
			if (VisibleObjects == null || VisibleObjects.quadTree == null)
			{
				Thread.Sleep(10);
				return;
			}
			renderer.Camera.UpdateQuadTreeLeaf();
		}

		private void UpdateLegacyVisibility(double trackPosition)
		{
			if (ObjectsSortedByStart == null || ObjectsSortedByEnd == null || ObjectsSortedByStart.Length == 0 || StaticObjectStates.Count == 0)
			{
				return;
			}

			double d = trackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = renderer.CameraTrackFollower.TrackPosition + renderer.Camera.Alignment.Position.Z;

			if (d < 0.0)
			{
				if (ObjectsSortedByStartPointer >= n)
				{
					ObjectsSortedByStartPointer = n - 1;
				}

				if (ObjectsSortedByEndPointer >= n)
				{
					ObjectsSortedByEndPointer = n - 1;
				}

				while (ObjectsSortedByStartPointer >= 0)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance > p + renderer.Camera.ForwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}

				while (ObjectsSortedByEndPointer >= 0)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance >= p - renderer.Camera.BackwardViewingDistance)
					{
						if (StaticObjectStates[o].StartingDistance <= p + renderer.Camera.ForwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (ObjectsSortedByStartPointer < 0)
				{
					ObjectsSortedByStartPointer = 0;
				}

				if (ObjectsSortedByEndPointer < 0)
				{
					ObjectsSortedByEndPointer = 0;
				}

				while (ObjectsSortedByEndPointer < n)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance < p - renderer.Camera.BackwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}

				while (ObjectsSortedByStartPointer < n)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance <= p + renderer.Camera.ForwardViewingDistance)
					{
						if (StaticObjectStates[o].EndingDistance >= p - renderer.Camera.BackwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}

			LastUpdatedTrackPosition = trackPosition;
		}

		public void UpdateViewingDistances(double backgroundImageDistance)
		{
			double f = Math.Atan2(renderer.CameraTrackFollower.WorldDirection.Z, renderer.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(renderer.Camera.AbsoluteDirection.Z, renderer.Camera.AbsoluteDirection.X) - f;
			if (c < -Math.PI)
			{
				c += 2.0 * Math.PI;
			}
			else if (c > Math.PI)
			{
				c -= 2.0 * Math.PI;
			}

			double a0 = c - 0.5 * renderer.Camera.HorizontalViewingAngle;
			double a1 = c + 0.5 * renderer.Camera.HorizontalViewingAngle;
			double max;
			if (a0 <= 0.0 & a1 >= 0.0)
			{
				max = 1.0;
			}
			else
			{
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				max = c0 > c1 ? c0 : c1;
				if (max < 0.0) max = 0.0;
			}

			double min;
			if (a0 <= -Math.PI | a1 >= Math.PI)
			{
				min = -1.0;
			}
			else
			{
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				min = c0 < c1 ? c0 : c1;
				if (min > 0.0) min = 0.0;
			}

			double d = Math.Max(backgroundImageDistance, renderer.currentOptions.ViewingDistance) + renderer.Camera.ExtraViewingDistance;
			renderer.Camera.ForwardViewingDistance = d * max;
			renderer.Camera.BackwardViewingDistance = -d * min;
			updateVisibility = VisibilityUpdate.Force;
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Matrix4D Rotate = (Matrix4D)new Transformation(LocalTransformation, WorldTransformation);
			return CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, AccurateObjectDisposal, Parameters, BlockLength);
		}

		public int CreateStaticObject(Vector3 Position, StaticObject Prototype, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			if (Prototype == null)
			{
				return -1;
			}

			if (Prototype.Mesh.Faces.Length == 0)
			{
				//Null object- Waste of time trying to calculate anything for these
				return -1;
			}

			float startingDistance = float.MaxValue;
			float endingDistance = float.MinValue;

			if (AccurateObjectDisposal == ObjectDisposalMode.Accurate)
			{
				foreach (VertexTemplate vertex in Prototype.Mesh.Vertices)
				{
					Vector3 Coordinates = new Vector3(vertex.Coordinates);
					Coordinates.Rotate(LocalTransformation);

					if (Coordinates.Z < startingDistance)
					{
						startingDistance = (float)Coordinates.Z;
					}

					if (Coordinates.Z > endingDistance)
					{
						endingDistance = (float)Coordinates.Z;
					}
				}

				startingDistance += (float)Parameters.AccurateObjectDisposalZOffset;
				endingDistance += (float)Parameters.AccurateObjectDisposalZOffset;
			}

			const double minBlockLength = 20.0;

			if (BlockLength < minBlockLength)
			{
				BlockLength *= Math.Ceiling(minBlockLength / BlockLength);
			}

			switch (AccurateObjectDisposal)
			{
				case ObjectDisposalMode.Accurate:
					startingDistance += (float)Parameters.TrackPosition;
					endingDistance += (float)Parameters.TrackPosition;
					double z = BlockLength * Math.Floor(Parameters.TrackPosition / BlockLength);
					Parameters.StartingDistance = Math.Min(z - BlockLength, startingDistance);
					Parameters.EndingDistance = Math.Max(z + 2.0 * BlockLength, endingDistance);
					startingDistance = (float)(BlockLength * Math.Floor(Parameters.StartingDistance / BlockLength));
					endingDistance = (float)(BlockLength * Math.Ceiling(Parameters.EndingDistance / BlockLength));
					break;
				case ObjectDisposalMode.Legacy:
					startingDistance = (float)Parameters.StartingDistance;
					endingDistance = (float)Parameters.EndingDistance;
					break;
				case ObjectDisposalMode.Mechanik:
					startingDistance = (float)Parameters.StartingDistance;
					endingDistance = (float)Parameters.EndingDistance + 1500;
					if (startingDistance < 0)
					{
						startingDistance = 0;
					}
					break;
			}
			StaticObjectStates.Add(new ObjectState
			{
				Prototype = Prototype,
				Translation = Translate,
				Rotate = Rotate,
				StartingDistance = startingDistance,
				EndingDistance = endingDistance,
				WorldPosition = Position,
				DisableShadowCasting = Parameters.DisableShadowCasting
			});

			foreach (MeshFace face in Prototype.Mesh.Faces)
			{
				switch (face.Flags & FaceFlags.FaceTypeMask)
				{
					case FaceFlags.Triangles:
						renderer.InfoTotalTriangles++;
						break;
					case FaceFlags.TriangleStrip:
						renderer.InfoTotalTriangleStrip++;
						break;
					case FaceFlags.Quads:
						renderer.InfoTotalQuads++;
						break;
					case FaceFlags.QuadStrip:
						renderer.InfoTotalQuadStrip++;
						break;
					case FaceFlags.Polygon:
						renderer.InfoTotalPolygon++;
						break;
				}
			}

			return StaticObjectStates.Count - 1;
		}

		public void CreateDynamicObject(ref ObjectState internalObject)
		{
			if (internalObject == null)
			{
				internalObject = new ObjectState(new StaticObject(renderer.currentHost));
			}

			internalObject.Prototype.Dynamic = true;

			DynamicObjectStates.Add(internalObject);
		}

		public void InitializeVisibility()
		{
			if (!renderer.ForceLegacyOpenGL) // as we might want to switch renderer types
			{
				for (int i = 0; i < StaticObjectStates.Count; i++)
				{
					VAOExtensions.CreateVAO(StaticObjectStates[i].Prototype.Mesh, false, renderer.DefaultShader.VertexLayout, renderer);
				}
				for (int i = 0; i < DynamicObjectStates.Count; i++)
				{
					VAOExtensions.CreateVAO(DynamicObjectStates[i].Prototype.Mesh, false, renderer.DefaultShader.VertexLayout, renderer);
				}
			}
			ObjectsSortedByStart = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.StartingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByEnd = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.EndingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;

			if (renderer.currentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree)
			{
				foreach (ObjectState state in StaticObjectStates)
				{
					VisibleObjects.quadTree.Add(state, Orientation3.Default);
				}
				VisibleObjects.quadTree.Initialize(renderer.currentOptions.QuadTreeLeafSize);
				UpdateQuadTreeVisibility();
			}
			else
			{
				double p = renderer.CameraTrackFollower.TrackPosition + renderer.Camera.Alignment.Position.Z;
				foreach (ObjectState state in StaticObjectStates.Where(recipe => recipe.StartingDistance <= p + renderer.Camera.ForwardViewingDistance & recipe.EndingDistance >= p - renderer.Camera.BackwardViewingDistance))
				{
					VisibleObjects.ShowObject(state, ObjectType.Static);
				}
			}
		}

		public void Reset()
		{
			renderer.currentHost.AnimatedObjectCollectionCache.Clear();
			List<ValueTuple<string, bool, DateTime>> keys = renderer.currentHost.StaticObjectCache.Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				if (!System.IO.File.Exists(keys[i].Item1) || System.IO.File.GetLastWriteTime(keys[i].Item1) != keys[i].Item3)
				{
					renderer.currentHost.StaticObjectCache.Remove(keys[i]);
				}
			}
			renderer.TextureManager.UnloadAllTextures(true);
			VisibleObjects.Clear();
		}
	}
}
