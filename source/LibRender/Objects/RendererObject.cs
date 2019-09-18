using OpenBveApi.Objects;

namespace LibRender
{
	/// <summary>A single unique object to be drawn by the renderer</summary>
	public class RendererObject
	{
		/// <summary>Holds a reference to the object in the ObjectManager array</summary>
		public StaticObject InternalObject;
		/// <summary>The FaceList references in the renderer</summary>
		public ObjectListReference[] FaceListReferences;
		/// <summary>The type of object</summary>
		public readonly ObjectType Type;

		public RendererObject(StaticObject internalObject, ObjectType objectType)
		{
			InternalObject = internalObject;
			Type = objectType;
		}
	}
}
