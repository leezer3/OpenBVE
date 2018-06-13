using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>
		/// Defines the behaviour for immediate texture loading
		/// </summary>
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		

		/// <summary>
		/// Defines the behaviour of the renderer for transparent textures
		/// </summary>
		internal enum TransparencyMode
		{
			/// <summary>Textures using color-key transparency are considered opaque, producing good performance but crisp outlines. Partially transparent faces are rendered in a single pass with z-buffer writes disabled, producing good performance but more depth-sorting issues.</summary>
			Performance = 0,
			/// <summary>Textures using color-key transparency are considered opaque, producing good performance but crisp outlines. Partially transparent faces are rendered in two passes, the first rendering only opaque pixels with z-buffer writes enabled, and the second rendering only partially transparent pixels with z-buffer writes disabled, producing best quality but worse performance.</summary>
			Intermediate = 1,
			/// <summary>Textures using color-key transparency are considered partially transparent. All partially transparent faces are rendered in two passes, the first rendering only opaque pixels with z-buffer writes enabled, and the second rendering only partially transparent pixels with z-buffer writes disabled, producing best quality but worse performance.</summary>
			Quality = 2
		}

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
			OverlayAlpha = 5
		}
		/// <summary>
		/// The type of object
		/// </summary>
		internal enum ObjectType : byte
		{
			/// <summary>The object is part of the static scenery. The matching ObjectListType is StaticOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
			Static = 1,
			/// <summary>The object is part of the animated scenery or of a train exterior. The matching ObjectListType is DynamicOpaque for fully opaque faces, and DynamicAlpha for all other faces.</summary>
			Dynamic = 2,
			/// <summary>The object is part of the cab. The matching ObjectListType is OverlayOpaque for fully opaque faces, and OverlayAlpha for all other faces.</summary>
			Overlay = 3
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
			internal Textures.OpenGlTextureWrapMode Wrap;
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
				this.WorldPosition = new Vector3(0.0, 0.0, 0.0);
				this.Update = true;
			}
		}
	}
}
