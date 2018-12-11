using OpenBveApi.Objects;

namespace OpenBveShared
{
	/// <summary>Defines a complete object to be shown by the renderer</summary>
	public struct RendererObject
	{
		/// <summary>The index of the object</summary>
		public int ObjectIndex;
		/// <summary>The face list references to the faces within the object</summary>
		public ObjectListReference[] FaceListReferences;
		/// <summary>The object type</summary>
		public ObjectType Type;
	}
}
