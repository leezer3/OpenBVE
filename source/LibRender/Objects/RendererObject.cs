using OpenBveApi.Objects;

namespace LibRender
{
	/// <summary>A single unique object to be drawn by the renderer</summary>
	public class RendererObject
	{
		//FIXME: Probably needs to be a StaticObject reference
		/// <summary>The index to the object in the ObjectManager array</summary>
		public readonly int ObjectIndex;
		/// <summary>The FaceList references in the renderer</summary>
		public ObjectListReference[] FaceListReferences;
		/// <summary>The type of object</summary>
		public readonly ObjectType Type;

		public RendererObject(int objectIndex, ObjectType objectType)
		{
			ObjectIndex = objectIndex;
			Type = objectType;
		}
	}
}
