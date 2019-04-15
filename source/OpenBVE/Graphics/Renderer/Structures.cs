using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>
		/// Defines the behaviour for immediate texture loading
		/// </summary>
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		
		// output mode
		internal enum OutputMode
		{
			/// <summary>Overlays are shown if active</summary>
			Default = 0,
			/// <summary>The debug overlay is shown (F10)</summary>
			Debug = 1,
			/// <summary>The ATS debug overlay is shown (F10)</summary>
			DebugATS = 2,
			/// <summary>No overlays are shown</summary>
			None = 3
		}

		// object list
		private struct Object
		{
			internal int ObjectIndex;
			internal ObjectListReference[] FaceListReferences;
			internal ObjectType Type;
		}
		private static Object[] Objects = new Object[256];
		/// <summary>
		/// The total number of objects in the simulation
		/// </summary>
		private static int ObjectCount;

		private enum ObjectListType : byte
		{
			/// <summary>The face is fully opaque and originates from an object that is part of the static scenery.</summary>
			StaticOpaque = 1,
			/// <summary>The face is fully opaque and originates from an object that is part of the dynamic scenery or of a train exterior.</summary>
			DynamicOpaque = 2,
			/// <summary>The face is partly transparent and originates from an object that is part of the scenery or of a train exterior.</summary>
			DynamicAlpha = 3,
			/// <summary>The face is fully opaque and originates from an object that is part of the cab.</summary>
			OverlayOpaque = 4,
			/// <summary>The face is partly transparent and originates from an object that is part of the cab.</summary>
			OverlayAlpha = 5,
			/// <summary>The face is touch element and originates from an object that is part of the cab.</summary>
			Touch = 6
		}

		private struct ObjectListReference
		{
			/// <summary>The type of list.</summary>
			internal readonly ObjectListType Type;
			/// <summary>The index in the specified list.</summary>
			internal int Index;
			internal ObjectListReference(ObjectListType type, int index)
			{
				this.Type = type;
				this.Index = index;
			}
		}

		private class BoundingBox
		{
			internal Vector3 Upper;
			internal Vector3 Lower;
		}
		private class ObjectFace
		{
			internal int ObjectListIndex;
			internal int ObjectIndex;
			internal int FaceIndex;
			internal double Distance;
			internal OpenGlTextureWrapMode Wrap;
		}
		private class ObjectList
		{
			internal ObjectFace[] Faces;
			internal int FaceCount;
			internal BoundingBox[] BoundingBoxes;
			internal ObjectList()
			{
				this.Faces = new ObjectFace[256];
				this.FaceCount = 0;
				this.BoundingBoxes = new BoundingBox[256];
			}

			/// <summary>Sorts the polgons contained within this ObjectList, near to far</summary>
			internal void SortPolygons()
			{
				// calculate distance
				double cx = World.AbsoluteCameraPosition.X;
				double cy = World.AbsoluteCameraPosition.Y;
				double cz = World.AbsoluteCameraPosition.Z;
				for (int i = 0; i < FaceCount; i++)
				{
					int o = Faces[i].ObjectIndex;
					int f = Faces[i].FaceIndex;
					if (ObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3)
					{
						int v0 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
						int v1 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
						int v2 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
						double v0x = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
						double v0y = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
						double v0z = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
						double v1x = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
						double v1y = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
						double v1z = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
						double v2x = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
						double v2y = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
						double v2z = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
						double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
						double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
						double dx = -w1z * w2y + w1y * w2z;
						double dy = w1z * w2x - w1x * w2z;
						double dz = -w1y * w2x + w1x * w2y;
						double t = dx * dx + dy * dy + dz * dz;
						if (t == 0.0) continue;
						t = 1.0 / Math.Sqrt(t);
						dx *= t;
						dy *= t;
						dz *= t;
						double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
						t = dx * w0x + dy * w0y + dz * w0z;
						Faces[i].Distance = -t * t;
					}
				}

				// sort
				double[] distances = new double[FaceCount];
				for (int i = 0; i < FaceCount; i++)
				{
					distances[i] = Faces[i].Distance;
				}

				Array.Sort<double, ObjectFace>(distances, Faces, 0, FaceCount);
				// update objects
				for (int i = 0; i < FaceCount; i++)
				{
					Objects[Faces[i].ObjectListIndex].FaceListReferences[Faces[i].FaceIndex].Index = i;
				}
			}
		}
		private class ObjectGroup
		{
			internal readonly ObjectList List;
			internal int OpenGlDisplayList;
			internal bool OpenGlDisplayListAvailable;
			internal Vector3 WorldPosition;
			internal bool Update;
			internal ObjectGroup()
			{
				this.List = new ObjectList();
				this.OpenGlDisplayList = 0;
				this.OpenGlDisplayListAvailable = false;
				this.WorldPosition = Vector3.Zero;
				this.Update = true;
			}
		}
	}
}
